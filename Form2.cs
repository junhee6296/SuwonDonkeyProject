using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TeamApp
{
    public sealed partial class Form2 : Form
    {
        private readonly Form1 owner;
        private string lastGeneratedCommand = string.Empty;
        private DateTime runStartedUtc;

        public Form2(Form1 owner)
        {
            this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
            InitializeComponent();
            LoadDefaults();
            GenerateCommand(false);
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
        }

        private void chkExcludeAnomaly_CheckedChanged(object? sender, EventArgs e)
        {
            GenerateCommand(false);
        }

        private void cmbEnvironment_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ConvertCurrentPathsForEnvironment();
        }

        private void GenerateCommandOnChanged(object? sender, EventArgs e)
        {
            GenerateCommand(false);
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

        private void btnClose_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void UseLoadedPaths()
        {
            txtData.Text = owner.GetTrainingDataPathForDialog();
            txtModel.Text = owner.GetTrainingModelPathForDialog();
            txtRemoteWork.Text = owner.ConvertVirtualBoxPathForDialog(owner.TrainingRootFolder);
            GenerateCommand(false);
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
        }

        private void BrowseDataPath()
        {
            using var dialog = new FolderBrowserDialog { Description = "학습 data/tub 폴더를 선택하세요" };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                txtData.Text = dialog.SelectedPath;
                GenerateCommand(false);
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
                var modelRoot = Path.GetDirectoryName(exportPath) ?? owner.TrainingRootFolder;
                txtModel.Text = owner.ConvertPathForEnvironment(Path.Combine(modelRoot, "models", "mypilot.h5"), env);
                if (env == "virtualbox")
                {
                    txtRemoteWork.Text = owner.ConvertVirtualBoxPathForDialog(owner.TrainingRootFolder);
                }
                AppendLog("학습 데이터 생성 완료: " + exportPath);
                MessageBox.Show(this, "학습 데이터셋을 생성했습니다.\n" + exportPath, "학습 데이터 생성", MessageBoxButtons.OK, MessageBoxIcon.Information);
                GenerateCommand(false);
            }
            catch (Exception ex)
            {
                AppendLog("학습 데이터 생성 실패: " + ex.Message);
                MessageBox.Show(this, ex.Message, "학습 데이터 생성 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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

            btnRun.Enabled = false;
            btnGenerate.Enabled = false;
            btnExport.Enabled = false;
            runStartedUtc = DateTime.UtcNow;
            SetScore(null, "학습 실행 중", "열린 콘솔 창에서 SSH 비밀번호를 입력하세요. 학습이 끝나면 콘솔의 안내에 따라 아무 키나 누르면 결과가 표시됩니다.");
            AppendLog("> " + Form1.MaskSensitiveCommand(command));
            owner.AppendTrainingLog("AI 학습 실행: " + Form1.MaskSensitiveCommand(command));

            int exitCode;
            try
            {
                exitCode = await owner.RunInteractiveConsoleCommandAsync(command).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                exitCode = -1;
                AppendLog("실행 오류: " + ex.Message);
            }

            AppendLog("프로세스 종료 코드: " + exitCode.ToString(CultureInfo.InvariantCulture));
            owner.AppendTrainingLog("AI 학습 종료 코드: " + exitCode.ToString(CultureInfo.InvariantCulture));
            ShowTrainingResult(exitCode);
            btnRun.Enabled = true;
            btnGenerate.Enabled = true;
            btnExport.Enabled = true;
        }

        private void ShowTrainingResult(int exitCode)
        {
            var score = CalculateScore(exitCode, txtModel.Text.Trim(), out var feedback);
            SetScore(score, score.ToString(CultureInfo.InvariantCulture) + "%", feedback);
        }

        private int CalculateScore(int exitCode, string modelPath, out string feedback)
        {
            var modelUpdated = false;
            if (owner.TryMapTrainingPathToLocalFile(modelPath, out var localModelPath) && File.Exists(localModelPath))
            {
                modelUpdated = File.GetLastWriteTimeUtc(localModelPath) >= runStartedUtc.AddSeconds(-10);
            }

            if (exitCode == 0 && modelUpdated)
            {
                feedback = "학습 프로세스가 정상 종료되었고 모델 파일이 생성/갱신되었습니다.";
                return 98;
            }

            if (exitCode == 0)
            {
                feedback = "프로세스는 정상 종료되었습니다. 모델 파일 갱신 여부는 직접 확인하세요. 콘솔에 epoch 완료 로그가 있었다면 학습 성공으로 볼 수 있습니다.";
                return 86;
            }

            if (exitCode == 255)
            {
                feedback = "SSH 인증 또는 연결 오류 가능성이 큽니다. 사용자명, 비밀번호, 포트포워딩, sshd 설정을 확인하세요.";
                return 20;
            }

            if (exitCode == -1)
            {
                feedback = "프로그램에서 실행 예외가 발생했습니다. 명령 형식이나 콘솔 실행 권한을 확인하세요.";
                return 10;
            }

            feedback = "학습 중 오류가 발생했습니다. 콘솔 마지막 오류를 확인하세요. 흔한 원인은 깨진 이미지, 잘못된 tub 경로, TensorFlow/패키지 문제입니다.";
            return 35;
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
            var ratio = score / 100.0;
            var r = (int)(210 * (1 - ratio) + 34 * ratio);
            var g = (int)(55 * (1 - ratio) + 160 * ratio);
            var b = (int)(55 * (1 - ratio) + 70 * ratio);
            return Color.FromArgb(r, g, b);
        }

        private void AppendLog(string text)
        {
            txtLog.AppendText("[" + DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture) + "] " + text + Environment.NewLine);
        }
    }
}
