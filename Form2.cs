using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TeamApp
{
    public sealed partial class Form2 : Form
    {
        private readonly Form1 owner;
        private string lastGeneratedCommand = string.Empty;
        private DateTime runStartedUtc;
        private readonly System.Windows.Forms.Timer trainingProgressTimer = new System.Windows.Forms.Timer();
        private int liveProgressPercent;
        private int expectedDataPercent;
        private bool trainingRunning;
        private bool stopRequested;
        private string? trainingMirrorLogPath;
        private string? trainingSessionFolder;
        private int currentEpoch;
        private int totalEpochs;
        private int currentBatch;
        private int totalBatches;
        private string lastLossText = string.Empty;
        private int lastLoggedProgress = -1;

        public Form2(Form1 owner)
        {
            this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
            InitializeComponent();
            trainingProgressTimer.Interval = 1000;
            trainingProgressTimer.Tick += trainingProgressTimer_Tick;
            FormClosing += Form2_FormClosing;
            LoadDefaults();
            GenerateCommand(false);
            RefreshMetricSummary();
        }

        private void LoadDefaults()
        {
            owner.RefreshTrainingPathDefaultsForDialog(false);
            cmbDatasetMode.SelectedIndex = 0;
            cmbEnvironment.SelectedIndex = 2;
            txtData.Text = owner.GetTrainingDataPathForDialog();
            txtModel.Text = owner.GetTrainingModelPathForDialog();
            txtActivate.Text = "source ~/miniconda3/bin/activate donkey_conda_tf";
            txtSshUser.Text = "xytron";
            txtSshHost.Text = "127.0.0.1";
            txtSshPort.Text = "2222";
            txtRemoteWork.Text = owner.ConvertVirtualBoxPathForDialog(owner.TrainingRootFolder);
            chkPasswordInput.Checked = true;
            txtCommand.ReadOnly = true;
            btnStop.Enabled = false;
            AppendLog("AI 학습 창을 열었습니다. 먼저 명령을 생성한 뒤 실행하세요.");
        }

        private string EnvironmentName()
        {
            return cmbEnvironment.SelectedIndex switch
            {
                1 => "wsl",
                2 => "virtualbox",
                _ => "windows"
            };
        }

        private void cmbDatasetMode_SelectedIndexChanged(object? sender, EventArgs e)
        {
            GenerateCommand(false);
            RefreshMetricSummary();
        }

        private void chkExcludeAnomaly_CheckedChanged(object? sender, EventArgs e)
        {
            GenerateCommand(false);
            RefreshMetricSummary();
        }

        private void cmbEnvironment_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ConvertCurrentPathsForEnvironment();
        }

        private void GenerateCommandOnChanged(object? sender, EventArgs e)
        {
            GenerateCommand(false);
            RefreshMetricSummary();
        }

        private void btnBrowseData_Click(object? sender, EventArgs e)
        {
            BrowseDataPath();
        }

        private void btnBrowseModel_Click(object? sender, EventArgs e)
        {
            BrowseModelPath();
        }

        private void btnUseLoaded_Click(object? sender, EventArgs e)
        {
            UseLoadedPaths();
        }

        private void btnWsl_Click(object? sender, EventArgs e)
        {
            cmbEnvironment.SelectedIndex = 1;
            ConvertCurrentPathsForEnvironment();
        }

        private void btnVBox_Click(object? sender, EventArgs e)
        {
            cmbEnvironment.SelectedIndex = 2;
            ConvertCurrentPathsForEnvironment();
        }

        private void btnExport_Click(object? sender, EventArgs e)
        {
            ExportTrainingData();
        }

        private void btnGenerate_Click(object? sender, EventArgs e)
        {
            GenerateCommand(true);
        }

        private void chkManualEdit_CheckedChanged(object? sender, EventArgs e)
        {
            txtCommand.ReadOnly = !chkManualEdit.Checked;
        }

        private async void btnRun_Click(object? sender, EventArgs e)
        {
            await RunTrainingAsync();
        }

        private void btnStop_Click(object? sender, EventArgs e)
        {
            stopRequested = true;
            UpdateProgressFromMirrorLog(forceLog: true);
            SaveTrainingRunState(null, "stop-requested");
            TryArchiveInterruptedModel("stop-requested");

            var stopped = owner.StopInteractiveTrainingProcess();
            var message = stopped
                ? "학습 중지 버튼으로 열린 cmd/ssh 프로세스 종료를 요청했습니다. 현재까지 복제된 로그, progress.json, 이미 저장된 모델/체크포인트를 _training_runs 폴더에 보존합니다."
                : "실행 중인 cmd 프로세스를 찾지 못했습니다. 열린 cmd 창이 남아 있으면 해당 창을 선택한 뒤 Ctrl+C를 누르세요. 현재까지의 로그와 진행 상태는 저장했습니다.";
            AppendLog(message);
            UpdateProcessProgress(GetInterruptedProgressPercent(), "중지 요청", "중지 시점까지 감지된 학습 진행률과 모델 파일을 보존했습니다. cmd 창이 남아 있으면 Ctrl+C로 안전 종료하세요.");
            MessageBox.Show(this, message, "학습 중지", MessageBoxButtons.OK, stopped ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        private void btnClose_Click(object? sender, EventArgs e)
        {
            if (trainingRunning)
            {
                MessageBox.Show(this, "학습 실행 중에는 창을 닫을 수 없습니다. 열린 cmd 창에서 Ctrl+C를 눌러 중지하거나 학습 완료 후 닫아 주세요.", "학습 실행 중", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Close();
        }

        private void Form2_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (trainingRunning)
            {
                e.Cancel = true;
                MessageBox.Show(this, "학습 실행 중에는 AI 학습 창을 닫을 수 없습니다. 열린 cmd 창에서 Ctrl+C를 눌러 안전하게 중지하세요.", "학습 실행 중", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void UseLoadedPaths()
        {
            txtData.Text = owner.GetTrainingDataPathForDialog();
            txtModel.Text = owner.GetTrainingModelPathForDialog();
            txtRemoteWork.Text = owner.ConvertVirtualBoxPathForDialog(owner.TrainingRootFolder);
            GenerateCommand(false);
            RefreshMetricSummary();
        }

        private void ConvertCurrentPathsForEnvironment()
        {
            var env = EnvironmentName();
            txtData.Text = owner.ConvertPathForEnvironment(owner.GetTrainingDataPathForDialog(), env);
            txtModel.Text = owner.ConvertPathForEnvironment(owner.GetTrainingModelPathForDialog(), env);
            if (env == "virtualbox")
            {
                txtRemoteWork.Text = owner.ConvertVirtualBoxPathForDialog(owner.TrainingRootFolder);
            }
            else if (env == "wsl")
            {
                txtRemoteWork.Text = owner.ConvertPathForEnvironment(owner.TrainingRootFolder, "wsl");
            }
            GenerateCommand(false);
            RefreshMetricSummary();
        }

        private void BrowseDataPath()
        {
            using var dialog = new FolderBrowserDialog { Description = "학습 data/tub 폴더를 선택하세요" };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                txtData.Text = dialog.SelectedPath;
                GenerateCommand(false);
                RefreshMetricSummary();
            }
        }

        private void BrowseModelPath()
        {
            using var dialog = new SaveFileDialog
            {
                Title = "모델 저장 위치 선택",
                Filter = "Keras/H5 model (*.h5)|*.h5|All files (*.*)|*.*",
                FileName = "mypilot.h5"
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                txtModel.Text = dialog.FileName;
                GenerateCommand(false);
            }
        }

        private void ExportTrainingData()
        {
            try
            {
                var exportPath = owner.CreateTrainingDatasetForDialog(cmbDatasetMode.SelectedIndex, chkExcludeAnomaly.Checked, cmbDatasetMode.Text);
                var env = EnvironmentName();
                txtData.Text = owner.ConvertPathForEnvironment(exportPath, env);

                var modelRoot = string.IsNullOrWhiteSpace(owner.TrainingRootFolder)
                    ? owner.TrainingDataFolder
                    : owner.TrainingRootFolder;
                txtModel.Text = owner.ConvertPathForEnvironment(Path.Combine(modelRoot, "models", "mypilot.h5"), env);
                if (env == "virtualbox")
                {
                    txtRemoteWork.Text = owner.ConvertVirtualBoxPathForDialog(owner.TrainingRootFolder);
                }
                AppendLog("학습 데이터 생성 완료: " + exportPath);
                RefreshMetricSummary();
                MessageBox.Show(this, "학습 데이터셋을 생성했습니다.\n" + exportPath, "학습 데이터 생성", MessageBoxButtons.OK, MessageBoxIcon.Information);
                GenerateCommand(false);
            }
            catch (Exception ex)
            {
                AppendLog("학습 데이터 생성 실패: " + ex.Message);
                MessageBox.Show(this, ex.Message, "학습 데이터 생성 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void PrepareSelectedDatasetForRun()
        {
            if (cmbDatasetMode.SelectedIndex <= 0 && !chkExcludeAnomaly.Checked)
            {
                return;
            }

            var exportPath = owner.CreateTrainingDatasetForDialog(cmbDatasetMode.SelectedIndex, chkExcludeAnomaly.Checked, cmbDatasetMode.Text);
            var env = EnvironmentName();
            txtData.Text = owner.ConvertPathForEnvironment(exportPath, env);

            var modelRoot = string.IsNullOrWhiteSpace(owner.TrainingRootFolder)
                ? owner.TrainingDataFolder
                : owner.TrainingRootFolder;
            txtModel.Text = owner.ConvertPathForEnvironment(Path.Combine(modelRoot, "models", "mypilot.h5"), env);
            if (env == "virtualbox")
            {
                txtRemoteWork.Text = owner.ConvertVirtualBoxPathForDialog(owner.TrainingRootFolder);
            }

            AppendLog("선택한 학습 범위 기준으로 학습 데이터셋을 자동 생성했습니다: " + exportPath);
            RefreshMetricSummary();
            GenerateCommand(false);
        }

        private string GenerateCommand(bool showLog)
        {
            var command = owner.BuildTrainingCommand(
                EnvironmentName(),
                txtData.Text.Trim(),
                txtModel.Text.Trim(),
                txtExtraArgs.Text.Trim(),
                txtActivate.Text.Trim(),
                txtSshUser.Text.Trim(),
                txtSshHost.Text.Trim(),
                txtSshPort.Text.Trim(),
                txtRemoteWork.Text.Trim(),
                chkPasswordInput.Checked && EnvironmentName() == "virtualbox" ? "interactive" : string.Empty);

            lastGeneratedCommand = command;
            if (!chkManualEdit.Checked)
            {
                txtCommand.Text = command;
            }
            owner.SetTrainingOverridesFromDialog(txtData.Text.Trim(), txtModel.Text.Trim());
            owner.StoreGeneratedTrainingCommand(command, chkPasswordInput.Checked && EnvironmentName() == "virtualbox");
            if (showLog)
            {
                AppendLog("명령 생성 완료: " + Form1.MaskSensitiveCommand(command));
            }
            return command;
        }

        private async Task RunTrainingAsync()
        {
            if (!chkManualEdit.Checked)
            {
                try
                {
                    PrepareSelectedDatasetForRun();
                }
                catch (Exception ex)
                {
                    AppendLog("학습 데이터 범위 준비 실패: " + ex.Message);
                    MessageBox.Show(this, ex.Message, "학습 데이터 준비 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            var command = chkManualEdit.Checked ? txtCommand.Text.Trim() : GenerateCommand(false);
            if (string.IsNullOrWhiteSpace(command))
            {
                MessageBox.Show(this, "실행할 학습 명령이 비어 있습니다.", "명령 없음", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (EnvironmentName() == "virtualbox" && chkPasswordInput.Checked)
            {
                command = Form1.EnableOpenSshPasswordPrompt(command);
            }

            RefreshMetricSummary();
            PrepareTrainingSession(command);
            var monitoredCommand = BuildMirroredConsoleCommand(command, trainingMirrorLogPath!);

            btnRun.Enabled = false;
            btnGenerate.Enabled = false;
            btnExport.Enabled = false;
            btnStop.Enabled = true;
            btnClose.Enabled = false;
            trainingRunning = true;
            stopRequested = false;
            runStartedUtc = DateTime.UtcNow;
            liveProgressPercent = 0;
            currentEpoch = 0;
            totalEpochs = 0;
            currentBatch = 0;
            totalBatches = 0;
            lastLossText = string.Empty;
            lastLoggedProgress = -1;

            UpdateProcessProgress(0, "0%", "cmd 학습 로그와 동기화 대기 중입니다. 열린 콘솔에서 SSH 비밀번호를 입력하세요. 중단해도 현재까지의 로그와 저장된 모델을 보존합니다.");
            trainingProgressTimer.Start();

            AppendLog("> " + Form1.MaskSensitiveCommand(command));
            AppendLog("실시간 진행률은 cmd 출력이 복제된 로그 파일을 읽어 Epoch/Batch 기준으로 계산합니다: " + trainingMirrorLogPath);
            AppendLog("중지 안내: cmd 창 Ctrl+C, cmd 창 X, 학습 중지 버튼 모두 중단으로 감지해 현재까지의 로그/progress.json/저장된 모델을 _training_runs에 보존합니다.");
            owner.AppendTrainingLog("AI 학습 실행: " + Form1.MaskSensitiveCommand(command));
            SaveTrainingRunState(null, "started");

            int exitCode;
            try
            {
                exitCode = await owner.RunInteractiveConsoleCommandAsync(monitoredCommand).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                exitCode = -1;
                AppendLog("실행 오류: " + ex.Message);
            }

            trainingProgressTimer.Stop();
            UpdateProgressFromMirrorLog(forceLog: true);
            trainingRunning = false;
            btnRun.Enabled = true;
            btnGenerate.Enabled = true;
            btnExport.Enabled = true;
            btnStop.Enabled = false;
            btnClose.Enabled = true;

            AppendLog("프로세스 종료 코드: " + exitCode.ToString(CultureInfo.InvariantCulture));
            owner.AppendTrainingLog("AI 학습 종료 코드: " + exitCode.ToString(CultureInfo.InvariantCulture));
            var interrupted = IsInterruptedRun(exitCode);
            SaveTrainingRunState(exitCode, exitCode == 0 ? "completed" : (interrupted ? "interrupted" : "failed"));
            if (exitCode != 0 || interrupted)
            {
                TryArchiveInterruptedModel("exit-" + exitCode.ToString(CultureInfo.InvariantCulture));
            }
            CopyMirrorLogToSession();
            ShowTrainingResult(exitCode);
        }

        private void trainingProgressTimer_Tick(object? sender, EventArgs e)
        {
            if (!trainingRunning)
            {
                return;
            }

            UpdateProgressFromMirrorLog(forceLog: false);
            SaveTrainingRunState(null, "running");
        }

        private void UpdateProgressFromMirrorLog(bool forceLog)
        {
            if (string.IsNullOrWhiteSpace(trainingMirrorLogPath) || !File.Exists(trainingMirrorLogPath))
            {
                if (forceLog)
                {
                    AppendLog("학습 로그 파일을 아직 찾지 못했습니다.");
                }
                return;
            }

            string text;
            try
            {
                text = ReadSharedText(trainingMirrorLogPath);
            }
            catch (Exception ex)
            {
                if (forceLog)
                {
                    AppendLog("학습 로그 읽기 실패: " + ex.Message);
                }
                return;
            }

            var progress = ParseProgress(text, out var status);
            if (progress.HasValue)
            {
                var next = Math.Max(liveProgressPercent, Math.Min(100, progress.Value));
                if (next != liveProgressPercent || forceLog)
                {
                    liveProgressPercent = next;
                    UpdateProcessProgress(liveProgressPercent, liveProgressPercent.ToString(CultureInfo.InvariantCulture) + "%", status);
                    if (forceLog || liveProgressPercent >= lastLoggedProgress + 5 || liveProgressPercent == 100)
                    {
                        lastLoggedProgress = liveProgressPercent;
                        AppendLog("학습 진행 동기화: " + liveProgressPercent.ToString(CultureInfo.InvariantCulture) + "% - " + status);
                    }
                }
            }
            else if (trainingRunning)
            {
                UpdateProcessProgress(liveProgressPercent, liveProgressPercent.ToString(CultureInfo.InvariantCulture) + "%", status);
            }
        }

        private int? ParseProgress(string text, out string status)
        {
            status = "cmd 학습 로그를 기다리는 중입니다.";
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            if (text.IndexOf("Finished training", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("TFLite conversion done", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                status = "학습 완료 로그가 감지되었습니다.";
                return 100;
            }

            if (ContainsFailureText(text))
            {
                status = "오류 로그가 감지되었습니다. cmd 창의 마지막 오류를 확인하세요.";
                return Math.Max(0, liveProgressPercent);
            }

            var epochMatches = Regex.Matches(text, @"Epoch\s+(\d+)\s*/\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (epochMatches.Count > 0)
            {
                var m = epochMatches[epochMatches.Count - 1];
                currentEpoch = ParseInt(m.Groups[1].Value);
                totalEpochs = Math.Max(1, ParseInt(m.Groups[2].Value));
                var afterEpoch = text.Substring(m.Index);
                var batchMatches = Regex.Matches(afterEpoch, @"(?<!\d)(\d+)\s*/\s*(\d+)\s*\[", RegexOptions.CultureInvariant);
                if (batchMatches.Count > 0)
                {
                    var b = batchMatches[batchMatches.Count - 1];
                    currentBatch = ParseInt(b.Groups[1].Value);
                    totalBatches = Math.Max(1, ParseInt(b.Groups[2].Value));
                }
                else
                {
                    currentBatch = 0;
                    totalBatches = 0;
                }

                var batchRatio = totalBatches > 0 ? Math.Max(0.0, Math.Min(1.0, currentBatch / (double)totalBatches)) : 0.0;
                var raw = ((Math.Max(1, currentEpoch) - 1 + batchRatio) / totalEpochs) * 100.0;
                var percent = Math.Max(0, Math.Min(99, (int)Math.Floor(raw)));
                lastLossText = ExtractLatestLossText(text);
                var batchText = totalBatches > 0 ? $", batch {currentBatch}/{totalBatches}" : string.Empty;
                var lossText = string.IsNullOrWhiteSpace(lastLossText) ? string.Empty : " / " + lastLossText;
                status = $"Epoch {currentEpoch}/{totalEpochs}{batchText}{lossText}. 중단하려면 cmd 창에서 Ctrl+C를 누르세요.";
                return percent;
            }

            if (text.IndexOf("Starting training", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                status = "학습 루프 시작 로그가 감지되었습니다.";
                return Math.Max(liveProgressPercent, 1);
            }

            if (text.IndexOf("Records # Training", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                status = "데이터셋 로딩 완료, 학습 시작 대기 중입니다.";
                return Math.Max(liveProgressPercent, 1);
            }

            if (text.IndexOf("loading config", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("using donkey", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                status = "DonkeyCar 실행 및 config 로딩 중입니다.";
                return Math.Max(liveProgressPercent, 0);
            }

            return null;
        }

        private static string ExtractLatestLossText(string text)
        {
            var matches = Regex.Matches(text, @"loss:\s*([0-9.]+)(?:.*?val_loss:\s*([0-9.]+))?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline);
            if (matches.Count == 0)
            {
                return string.Empty;
            }
            var m = matches[matches.Count - 1];
            var loss = m.Groups[1].Success ? m.Groups[1].Value : string.Empty;
            var val = m.Groups[2].Success ? m.Groups[2].Value : string.Empty;
            if (!string.IsNullOrWhiteSpace(loss) && !string.IsNullOrWhiteSpace(val))
            {
                return "loss " + loss + ", val_loss " + val;
            }
            if (!string.IsNullOrWhiteSpace(loss))
            {
                return "loss " + loss;
            }
            return string.Empty;
        }

        private static bool ContainsFailureText(string text)
        {
            return text.IndexOf("Traceback", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   text.IndexOf("ModuleNotFoundError", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   text.IndexOf("FileNotFoundError", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   text.IndexOf("Permission denied", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   text.IndexOf("core dumped", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void ShowTrainingResult(int exitCode)
        {
            var score = CalculateScore(exitCode, txtModel.Text.Trim(), out var feedback);
            UpdateProcessProgress(score, score.ToString(CultureInfo.InvariantCulture) + "%", feedback, final: true);
        }

        private int CalculateScore(int exitCode, string modelPath, out string feedback)
        {
            var modelUpdated = false;
            if (owner.TryMapTrainingPathToLocalFile(modelPath, out var localModelPath) && File.Exists(localModelPath))
            {
                modelUpdated = File.GetLastWriteTimeUtc(localModelPath) >= runStartedUtc.AddSeconds(-20);
            }

            if (exitCode == 0 && modelUpdated)
            {
                feedback = "학습 프로세스가 정상 종료되었고 모델 파일이 생성/갱신되었습니다.";
                return 100;
            }

            if (exitCode == 0)
            {
                feedback = "프로세스는 정상 종료되었습니다. 모델 파일 갱신 여부는 직접 확인하세요. epoch 완료 로그가 있었다면 학습 성공으로 볼 수 있습니다.";
                return Math.Max(80, Math.Max(liveProgressPercent, expectedDataPercent));
            }

            if (IsInterruptedRun(exitCode))
            {
                var interruptedPercent = GetInterruptedProgressPercent();
                feedback = modelUpdated
                    ? "학습이 중간에 중지되었지만 중지 전 저장된 모델 파일과 진행 로그를 _training_runs 폴더에 보존했습니다. 이 모델은 interrupted_*.h5로도 복사됩니다."
                    : "학습이 중간에 중지되었습니다. 체크포인트가 저장되기 전이면 모델 파일은 없을 수 있지만, 중지 시점까지의 console 로그와 progress.json은 _training_runs 폴더에 보존했습니다.";
                return interruptedPercent;
            }

            if (exitCode == 255)
            {
                feedback = "SSH 인증 또는 연결 오류입니다. 사용자명, 비밀번호, 포트포워딩, sshd 설정을 확인하세요.";
                return 0;
            }

            if (exitCode == -1)
            {
                feedback = "프로그램에서 실행 예외가 발생했습니다. 명령 형식이나 콘솔 실행 권한을 확인하세요.";
                return 0;
            }

            feedback = "학습 중 오류가 발생했습니다. 콘솔 마지막 오류를 확인하세요. 흔한 원인은 깨진 이미지, 잘못된 tub 경로, TensorFlow/패키지 문제입니다.";
            return 0;
        }

        private bool IsInterruptedRun(int exitCode)
        {
            if (stopRequested || IsLikelyInterrupted(exitCode) || MirrorLogContainsInterruption())
            {
                return true;
            }

            // 사용자가 cmd 창을 X로 닫거나 SSH가 Ctrl+C로 끊기면 255/-1로 끝나는 경우가 있다.
            // 이때 이미 Epoch/Batch가 진행되었고 명확한 Python 오류가 없으면 중단으로 취급해 진행률을 보존한다.
            return (exitCode == 255 || exitCode == -1) && HasMeaningfulTrainingProgress() && !MirrorLogContainsHardFailure();
        }

        private static bool IsLikelyInterrupted(int exitCode)
        {
            return exitCode == 130 || exitCode == -1073741510 || exitCode == -1073741511;
        }

        private bool MirrorLogContainsInterruption()
        {
            var text = ReadMirrorLogQuietly();
            return text.IndexOf("KeyboardInterrupt", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   text.IndexOf("Training interrupted", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   text.IndexOf("Process exit code: 130", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool MirrorLogContainsHardFailure()
        {
            var text = ReadMirrorLogQuietly();
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            if (MirrorLogContainsInterruption())
            {
                return false;
            }

            return text.IndexOf("Traceback", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   text.IndexOf("ModuleNotFoundError", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   text.IndexOf("FileNotFoundError", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   text.IndexOf("Permission denied", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   text.IndexOf("can't open file", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   text.IndexOf("core dumped", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private string ReadMirrorLogQuietly()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(trainingMirrorLogPath) || !File.Exists(trainingMirrorLogPath))
                {
                    return string.Empty;
                }
                return ReadSharedText(trainingMirrorLogPath);
            }
            catch
            {
                return string.Empty;
            }
        }

        private bool HasMeaningfulTrainingProgress()
        {
            if (liveProgressPercent > 0 || currentEpoch > 0 || currentBatch > 0)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(trainingMirrorLogPath) || !File.Exists(trainingMirrorLogPath))
            {
                return false;
            }

            try
            {
                var text = ReadSharedText(trainingMirrorLogPath);
                return text.IndexOf("Starting training", StringComparison.OrdinalIgnoreCase) >= 0 ||
                       text.IndexOf("Epoch ", StringComparison.OrdinalIgnoreCase) >= 0 ||
                       text.IndexOf("loss:", StringComparison.OrdinalIgnoreCase) >= 0 ||
                       text.IndexOf("KeyboardInterrupt", StringComparison.OrdinalIgnoreCase) >= 0 ||
                       text.IndexOf("Interrupted", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch
            {
                return false;
            }
        }

        private int GetInterruptedProgressPercent()
        {
            UpdateProgressFromMirrorLog(forceLog: false);
            var percent = liveProgressPercent;
            if (percent <= 0 && currentEpoch > 0 && totalEpochs > 0)
            {
                var batchRatio = totalBatches > 0 ? Math.Max(0.0, Math.Min(1.0, currentBatch / (double)totalBatches)) : 0.0;
                percent = (int)Math.Floor(((currentEpoch - 1 + batchRatio) / totalEpochs) * 100.0);
            }
            if (percent <= 0 && HasMeaningfulTrainingProgress())
            {
                percent = 1;
            }
            return Math.Max(0, Math.Min(99, percent));
        }

        private void RefreshMetricSummary()
        {
            var metrics = CalculateLocalTrainingMetrics();
            expectedDataPercent = metrics.ExpectedPercent;
            SetMetricBar(pnlMetricDataBack, pnlMetricDataFill, lblMetricDataValue, metrics.DataIntegrityPercent);
            SetMetricBar(pnlMetricThrottleBack, pnlMetricThrottleFill, lblMetricThrottleValue, metrics.ThrottleQualityPercent);
            SetMetricBar(pnlMetricAnomalyBack, pnlMetricAnomalyFill, lblMetricAnomalyValue, metrics.AnomalyQualityPercent);
            SetMetricBar(pnlMetricProcessBack, pnlMetricProcessFill, lblMetricProcessValue, trainingRunning ? liveProgressPercent : 0);
            lblMetricOverall.BackColor = ScoreToColor(metrics.ExpectedPercent);
            lblMetricOverall.ForeColor = metrics.ExpectedPercent >= 55 ? Color.White : Color.Black;
            lblMetricOverall.Text = metrics.ExpectedPercent.ToString(CultureInfo.InvariantCulture) + "%" + Environment.NewLine + "예상";
            lblMetricSummary.Text = metrics.Summary;
        }

        private TrainingMetrics CalculateLocalTrainingMetrics()
        {
            try
            {
                var localData = ResolveLocalTrainingFolder(txtData.Text.Trim());
                if (string.IsNullOrWhiteSpace(localData) || !Directory.Exists(localData))
                {
                    return TrainingMetrics.Empty("학습 데이터 폴더를 찾을 수 없습니다. 경로를 확인하세요.");
                }

                var catalogFiles = Directory.GetFiles(localData, "catalog_*.catalog").OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray();
                if (catalogFiles.Length == 0)
                {
                    return TrainingMetrics.Empty("catalog_*.catalog 파일이 없습니다. DonkeyCar v5 tub/data 폴더를 선택하세요.");
                }

                var imageRoot = Path.Combine(localData, "images");
                var total = 0;
                var usable = 0;
                var missing = 0;
                var positiveThrottle = 0;
                var zeroOrReverseThrottle = 0;
                var anomaly = 0;
                var throttleValues = new List<double>();
                double? previousAngle = null;

                foreach (var catalog in catalogFiles)
                {
                    foreach (var line in File.ReadLines(catalog))
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }
                        total++;
                        try
                        {
                            using var document = JsonDocument.Parse(line);
                            var root = document.RootElement;
                            var imageName = ExtractImageName(root);
                            var imageOk = true;
                            if (!string.IsNullOrWhiteSpace(imageName))
                            {
                                var imagePath = Path.IsPathRooted(imageName) ? imageName : Path.Combine(imageRoot, Path.GetFileName(imageName));
                                imageOk = File.Exists(imagePath);
                            }
                            if (imageOk)
                            {
                                usable++;
                            }
                            else
                            {
                                missing++;
                            }

                            var angle = ExtractDouble(root, "user/angle") ?? ExtractDouble(root, "angle");
                            var throttle = ExtractDouble(root, "user/throttle") ?? ExtractDouble(root, "throttle");
                            if (throttle.HasValue)
                            {
                                throttleValues.Add(throttle.Value);
                                if (throttle.Value > 0.0001)
                                {
                                    positiveThrottle++;
                                }
                                else
                                {
                                    zeroOrReverseThrottle++;
                                }
                            }
                            if (angle.HasValue)
                            {
                                if (previousAngle.HasValue && Math.Abs(angle.Value - previousAngle.Value) > 0.55)
                                {
                                    anomaly++;
                                }
                                previousAngle = angle.Value;
                            }
                        }
                        catch
                        {
                            missing++;
                        }
                    }
                }

                var dataIntegrity = total == 0 ? 0 : Percent(usable, total);
                var throttleQuality = total == 0 ? 0 : Percent(positiveThrottle, Math.Max(1, positiveThrottle + zeroOrReverseThrottle));
                var anomalyQuality = total == 0 ? 0 : Math.Max(0, 100 - Percent(anomaly, total));
                var expected = (int)Math.Round(dataIntegrity * 0.45 + throttleQuality * 0.35 + anomalyQuality * 0.20);
                var avg = throttleValues.Count == 0 ? 0 : throttleValues.Average();
                var min = throttleValues.Count == 0 ? 0 : throttleValues.Min();
                var max = throttleValues.Count == 0 ? 0 : throttleValues.Max();

                return new TrainingMetrics
                {
                    DataIntegrityPercent = dataIntegrity,
                    ThrottleQualityPercent = throttleQuality,
                    AnomalyQualityPercent = anomalyQuality,
                    ExpectedPercent = expected,
                    Summary =
                        $"범위: {cmbDatasetMode.Text} / 전체 {total} / 사용 가능 {usable} / 누락·오류 {missing}" + Environment.NewLine +
                        $"양수 스로틀 {positiveThrottle} / 0·후진 {zeroOrReverseThrottle} / 조향 급변 후보 {anomaly}" + Environment.NewLine +
                        $"스로틀 평균 {avg:0.###}, 범위 {min:0.###} ~ {max:0.###}"
                };
            }
            catch (Exception ex)
            {
                return TrainingMetrics.Empty("학습 데이터 요약을 계산할 수 없습니다: " + ex.Message);
            }
        }

        private string ResolveLocalTrainingFolder(string trainingPath)
        {
            if (owner.TryMapTrainingPathToLocalFile(trainingPath, out var mapped) && Directory.Exists(mapped))
            {
                return mapped;
            }
            if (Directory.Exists(trainingPath))
            {
                return trainingPath;
            }
            var root = owner.TrainingRootFolder;
            if (!string.IsNullOrWhiteSpace(root) && Directory.Exists(root))
            {
                var candidate = Path.Combine(root, "data");
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }
            }
            return trainingPath;
        }

        private static string ExtractImageName(JsonElement root)
        {
            foreach (var property in root.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.String && property.Name.IndexOf("image", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return property.Value.GetString() ?? string.Empty;
                }
            }
            return string.Empty;
        }

        private static double? ExtractDouble(JsonElement root, string name)
        {
            if (root.TryGetProperty(name, out var value))
            {
                if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number))
                {
                    return number;
                }
                if (value.ValueKind == JsonValueKind.String && double.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                {
                    return parsed;
                }
            }
            return null;
        }

        private static int Percent(int value, int total)
        {
            if (total <= 0)
            {
                return 0;
            }
            return Math.Max(0, Math.Min(100, (int)Math.Round(value * 100.0 / total)));
        }

        private void UpdateProcessProgress(int percent, string title, string feedback, bool final = false)
        {
            percent = Math.Max(0, Math.Min(100, percent));
            SetMetricBar(pnlMetricProcessBack, pnlMetricProcessFill, lblMetricProcessValue, percent);
            lblMetricOverall.BackColor = ScoreToColor(percent);
            lblMetricOverall.ForeColor = percent >= 55 ? Color.White : Color.Black;
            lblMetricOverall.Text = percent.ToString(CultureInfo.InvariantCulture) + "%" + Environment.NewLine + (final ? "최종" : "진행");
            SetScore(percent, title, feedback);
        }

        private static void SetMetricBar(Panel backPanel, Panel fillPanel, Label valueLabel, int percent)
        {
            percent = Math.Max(0, Math.Min(100, percent));
            var maxWidth = Math.Max(0, backPanel.ClientSize.Width - 2);
            fillPanel.SetBounds(1, 1, (int)Math.Round(maxWidth * percent / 100.0), Math.Max(1, backPanel.ClientSize.Height - 2));
            fillPanel.BackColor = ScoreToColor(percent);
            valueLabel.Text = percent.ToString(CultureInfo.InvariantCulture) + "%";
        }

        private void SetScore(int? score, string title, string feedback)
        {
            if (score.HasValue)
            {
                lblScore.BackColor = ScoreToColor(score.Value);
                lblScore.ForeColor = score.Value >= 55 ? Color.White : Color.Black;
            }
            else
            {
                lblScore.BackColor = Color.SteelBlue;
                lblScore.ForeColor = Color.White;
            }
            lblScore.Text = title;
            lblFeedback.Text = feedback;
        }

        private static Color ScoreToColor(int score)
        {
            score = Math.Max(0, Math.Min(100, score));
            if (score < 50)
            {
                var ratio = score / 50.0;
                var r = 220;
                var g = (int)(45 + 175 * ratio);
                var b = 45;
                return Color.FromArgb(r, g, b);
            }
            else
            {
                var ratio = (score - 50) / 50.0;
                var r = (int)(220 * (1 - ratio) + 30 * ratio);
                var g = (int)(220 * (1 - ratio) + 165 * ratio);
                var b = (int)(45 * (1 - ratio) + 75 * ratio);
                return Color.FromArgb(r, g, b);
            }
        }

        private void PrepareTrainingSession(string command)
        {
            var baseFolder = ResolveModelDirectory();
            trainingSessionFolder = Path.Combine(baseFolder, "_training_runs", DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture));
            Directory.CreateDirectory(trainingSessionFolder);
            trainingMirrorLogPath = Path.Combine(trainingSessionFolder, "console_mirror.log");
            File.WriteAllText(Path.Combine(trainingSessionFolder, "command.txt"), Form1.MaskSensitiveCommand(command), Encoding.UTF8);
            SaveTrainingRunState(null, "prepared");
        }

        private string ResolveModelDirectory()
        {
            if (owner.TryMapTrainingPathToLocalFile(txtModel.Text.Trim(), out var localModelPath))
            {
                var dir = Path.GetDirectoryName(localModelPath);
                if (!string.IsNullOrWhiteSpace(dir))
                {
                    Directory.CreateDirectory(dir);
                    return dir;
                }
            }
            var root = owner.TrainingRootFolder;
            if (!string.IsNullOrWhiteSpace(root))
            {
                var models = Path.Combine(root, "models");
                Directory.CreateDirectory(models);
                return models;
            }
            var fallback = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TeamAppTrainingRuns");
            Directory.CreateDirectory(fallback);
            return fallback;
        }

        private void SaveTrainingRunState(int? exitCode, string state)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(trainingSessionFolder))
                {
                    return;
                }
                Directory.CreateDirectory(trainingSessionFolder);
                var json = "{" + Environment.NewLine +
                           $"  \"state\": \"{EscapeJson(state)}\"," + Environment.NewLine +
                           $"  \"updatedAt\": \"{DateTime.Now:O}\"," + Environment.NewLine +
                           $"  \"exitCode\": {(exitCode.HasValue ? exitCode.Value.ToString(CultureInfo.InvariantCulture) : "null")}," + Environment.NewLine +
                           $"  \"progressPercent\": {liveProgressPercent}," + Environment.NewLine +
                           $"  \"epoch\": {currentEpoch}," + Environment.NewLine +
                           $"  \"totalEpochs\": {totalEpochs}," + Environment.NewLine +
                           $"  \"batch\": {currentBatch}," + Environment.NewLine +
                           $"  \"totalBatches\": {totalBatches}," + Environment.NewLine +
                           $"  \"loss\": \"{EscapeJson(lastLossText)}\"," + Environment.NewLine +
                           $"  \"dataPath\": \"{EscapeJson(txtData.Text.Trim())}\"," + Environment.NewLine +
                           $"  \"modelPath\": \"{EscapeJson(txtModel.Text.Trim())}\"" + Environment.NewLine +
                           "}";
                File.WriteAllText(Path.Combine(trainingSessionFolder, "progress.json"), json, Encoding.UTF8);
            }
            catch
            {
                // 진행 상태 저장 실패가 학습 자체를 방해하지 않게 한다.
            }
        }

        private void TryArchiveInterruptedModel(string reason)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(trainingSessionFolder))
                {
                    return;
                }
                Directory.CreateDirectory(trainingSessionFolder);
                if (!owner.TryMapTrainingPathToLocalFile(txtModel.Text.Trim(), out var localModelPath) || !File.Exists(localModelPath))
                {
                    SaveTrainingRunState(null, reason + "-no-model-yet");
                    File.WriteAllText(Path.Combine(trainingSessionFolder, "interrupted_model_status.txt"),
                        "중지 시점에 복사 가능한 모델 파일을 찾지 못했습니다." + Environment.NewLine +
                        "progressPercent=" + liveProgressPercent.ToString(CultureInfo.InvariantCulture) + Environment.NewLine +
                        "epoch=" + currentEpoch.ToString(CultureInfo.InvariantCulture) + "/" + totalEpochs.ToString(CultureInfo.InvariantCulture) + Environment.NewLine,
                        Encoding.UTF8);
                    return;
                }

                var extension = Path.GetExtension(localModelPath);
                var archiveName = "interrupted_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + extension;
                var archivePath = Path.Combine(trainingSessionFolder, archiveName);
                File.Copy(localModelPath, archivePath, true);

                var sameDir = Path.Combine(Path.GetDirectoryName(localModelPath) ?? trainingSessionFolder, archiveName);
                File.Copy(localModelPath, sameDir, true);
                AppendLog("중지/비정상 종료 시점 모델 보존: " + archivePath);
            }
            catch (Exception ex)
            {
                AppendLog("중지 모델 보존 실패: " + ex.Message);
            }
        }

        private void CopyMirrorLogToSession()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(trainingSessionFolder) || string.IsNullOrWhiteSpace(trainingMirrorLogPath) || !File.Exists(trainingMirrorLogPath))
                {
                    return;
                }
                var destination = Path.Combine(trainingSessionFolder, "console_final.log");
                File.Copy(trainingMirrorLogPath, destination, true);
            }
            catch (Exception ex)
            {
                AppendLog("학습 로그 보존 실패: " + ex.Message);
            }
        }

        private string BuildMirroredConsoleCommand(string command, string logPath)
        {
            if (EnvironmentName() == "virtualbox" && TryBuildSshRemoteMirroredCommand(command, logPath, out var remoteMirroredCommand))
            {
                return remoteMirroredCommand;
            }

            // 로컬/WSL 실행은 콘솔 출력을 그대로 보여준다. PowerShell Tee-Object를 사용하면
            // TensorFlow stderr가 NativeCommandError로 변환되어 콘솔이 지저분해지는 문제가 있어 사용하지 않는다.
            return command;
        }

        private bool TryBuildSshRemoteMirroredCommand(string command, string localLogPath, out string mirroredCommand)
        {
            mirroredCommand = string.Empty;
            if (!TrySplitSshCommand(command, out var sshPrefix, out var remoteCommand))
            {
                return false;
            }

            var remoteLogPath = MapLocalRunPathToRemote(localLogPath);
            if (string.IsNullOrWhiteSpace(remoteLogPath))
            {
                return false;
            }

            var remoteLogDirectory = GetRemoteDirectoryName(remoteLogPath);
            var script =
                "set -o pipefail; " +
                "mkdir -p " + BashQuote(remoteLogDirectory) + "; " +
                "echo " + BashQuote("[TeamApp] mirror log: " + remoteLogPath) + " | tee -a " + BashQuote(remoteLogPath) + "; " +
                "trap 'ec=130; echo [TeamApp] Training interrupted by signal | tee -a " + BashQuote(remoteLogPath) + "; echo [TeamApp] Process exit code: ${ec} | tee -a " + BashQuote(remoteLogPath) + "; exit ${ec}' INT TERM; " +
                "(" + remoteCommand + ") 2>&1 | tee -a " + BashQuote(remoteLogPath) + "; " +
                "ec=${PIPESTATUS[0]}; " +
                "echo [TeamApp] Process exit code: ${ec} | tee -a " + BashQuote(remoteLogPath) + "; " +
                "exit ${ec}";

            var remoteArgument = "bash -lc " + BashQuote(script);
            mirroredCommand = sshPrefix + "\"" + remoteArgument.Replace("\"", "\\\"") + "\"";
            return true;
        }

        private static bool TrySplitSshCommand(string command, out string sshPrefix, out string remoteCommand)
        {
            sshPrefix = string.Empty;
            remoteCommand = string.Empty;
            var trimmed = command.Trim();
            if (!trimmed.StartsWith("ssh ", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var firstQuote = trimmed.IndexOf('"');
            var lastQuote = trimmed.LastIndexOf('"');
            if (firstQuote < 0 || lastQuote <= firstQuote)
            {
                return false;
            }

            var tail = trimmed.Substring(lastQuote + 1);
            if (!string.IsNullOrWhiteSpace(tail))
            {
                return false;
            }

            sshPrefix = trimmed.Substring(0, firstQuote);
            remoteCommand = trimmed.Substring(firstQuote + 1, lastQuote - firstQuote - 1);
            return !string.IsNullOrWhiteSpace(remoteCommand);
        }

        private string MapLocalRunPathToRemote(string localPath)
        {
            var remoteRoot = NormalizeRemotePath(txtRemoteWork.Text.Trim());
            if (string.IsNullOrWhiteSpace(remoteRoot))
            {
                return string.Empty;
            }

            try
            {
                var localRoot = Path.GetFullPath(owner.TrainingRootFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var fullLocalPath = Path.GetFullPath(localPath);
                if (fullLocalPath.StartsWith(localRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                    fullLocalPath.Equals(localRoot, StringComparison.OrdinalIgnoreCase))
                {
                    var relative = Path.GetRelativePath(localRoot, fullLocalPath).Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
                    return remoteRoot + "/" + relative.TrimStart('/');
                }
            }
            catch
            {
                // 아래 fallback을 사용한다.
            }

            var sessionName = !string.IsNullOrWhiteSpace(trainingSessionFolder) ? Path.GetFileName(trainingSessionFolder) : DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            return remoteRoot + "/models/_training_runs/" + sessionName + "/" + Path.GetFileName(localPath);
        }

        private static string NormalizeRemotePath(string path)
        {
            return path.Replace('\\', '/').Trim().Trim('"').TrimEnd('/');
        }

        private static string GetRemoteDirectoryName(string path)
        {
            var value = NormalizeRemotePath(path);
            var index = value.LastIndexOf('/');
            return index > 0 ? value.Substring(0, index) : ".";
        }

        private static string BashQuote(string text)
        {
            return "'" + text.Replace("'", "'\\''", StringComparison.Ordinal) + "'";
        }

        private static string ReadSharedText(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var reader = new StreamReader(stream, Encoding.UTF8, true);
            return reader.ReadToEnd();
        }

        private static int ParseInt(string text)
        {
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : 0;
        }

        private static string EscapeJson(string text)
        {
            return text.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal).Replace("\r", "\\r", StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal);
        }

        private void AppendLog(string text)
        {
            txtLog.AppendText("[" + DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture) + "] " + text + Environment.NewLine);
        }

        private sealed class TrainingMetrics
        {
            public int DataIntegrityPercent { get; set; }
            public int ThrottleQualityPercent { get; set; }
            public int AnomalyQualityPercent { get; set; }
            public int ExpectedPercent { get; set; }
            public string Summary { get; set; } = string.Empty;

            public static TrainingMetrics Empty(string summary)
            {
                return new TrainingMetrics
                {
                    DataIntegrityPercent = 0,
                    ThrottleQualityPercent = 0,
                    AnomalyQualityPercent = 0,
                    ExpectedPercent = 0,
                    Summary = summary
                };
            }
        }
    }
}
