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
        private Form1.TrainingDatasetPreview? latestPreview;
        private int latestProcessScore = -1;
        private int latestOverallScore = -1;
        private bool isTrainingRunning;

        public Form2(Form1 owner)
        {
            this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
            InitializeComponent();
            FormClosing += Form2_FormClosing;
            InitializeTrainingAnalysisControls();
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
            RefreshTrainingPreview();
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
            RefreshTrainingPreview();
        }

        private void chkExcludeAnomaly_CheckedChanged(object? sender, EventArgs e)
        {
            GenerateCommand(false);
            RefreshTrainingPreview();
        }

        private void cmbEnvironment_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ConvertCurrentPathsForEnvironment();
        }

        private void GenerateCommandOnChanged(object? sender, EventArgs e)
        {
            GenerateCommand(false);
            RefreshTrainingPreview();
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

                // 학습 데이터셋은 data/_training_sets 아래에 따로 만들지만,
                // 결과 모델은 config.py/train.py가 있는 mycar/models에 저장되도록 유지한다.
                var modelRoot = string.IsNullOrWhiteSpace(owner.TrainingRootFolder)
                    ? owner.TrainingDataFolder
                    : owner.TrainingRootFolder;
                txtModel.Text = owner.ConvertPathForEnvironment(Path.Combine(modelRoot, "models", "mypilot.h5"), env);
                if (env == "virtualbox")
                {
                    txtRemoteWork.Text = owner.ConvertVirtualBoxPathForDialog(owner.TrainingRootFolder);
                }
                AppendLog("학습 데이터 생성 완료: " + exportPath);
                RefreshTrainingPreview();
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
            var command = string.Empty;
            if (!chkManualEdit.Checked)
            {
                try
                {
                    AppendLog("실행 전 정제 학습 데이터셋을 생성합니다. 삭제된 프레임은 제외하고, 편집된 이미지는 현재 파일 상태 그대로 복사합니다.");
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

                    AppendLog("실행용 학습 데이터셋 생성 완료: " + exportPath);
                }
                catch (Exception ex)
                {
                    SetScore(0, "0%", "학습 데이터셋 생성에 실패했습니다. " + ex.Message);
                    MessageBox.Show(this, ex.Message, "학습 데이터 생성 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                command = GenerateCommand(false);
            }
            else
            {
                command = txtCommand.Text.Trim();
            }

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
            btnClose.Enabled = false;
            isTrainingRunning = true;
            runStartedUtc = DateTime.UtcNow;
            latestProcessScore = 0;
            latestOverallScore = -1;
            RefreshTrainingPreview();
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
            isTrainingRunning = false;
            btnRun.Enabled = true;
            btnGenerate.Enabled = true;
            btnExport.Enabled = true;
            btnClose.Enabled = true;
        }

        private void ShowTrainingResult(int exitCode)
        {
            var score = CalculateScore(exitCode, txtModel.Text.Trim(), out var feedback);
            SetScore(score, score.ToString(CultureInfo.InvariantCulture) + "%", feedback);
        }

        private int CalculateScore(int exitCode, string modelPath, out string feedback)
        {
            var preview = RefreshTrainingPreview();
            var dataScore = preview?.DataIntegrityScore ?? 0;
            var throttleScore = preview?.ThrottleQualityScore ?? 0;
            var anomalyScore = preview?.AnomalyControlScore ?? 0;
            var modelUpdated = false;

            if (owner.TryMapTrainingPathToLocalFile(modelPath, out var localModelPath) && File.Exists(localModelPath))
            {
                modelUpdated = File.GetLastWriteTimeUtc(localModelPath) >= runStartedUtc.AddSeconds(-10);
            }

            if (exitCode != 0)
            {
                latestProcessScore = 0;
                latestOverallScore = 0;
                if (exitCode == 255)
                {
                    feedback = "0%: SSH 인증/연결 실패입니다. VM 사용자명, 비밀번호, 포트포워딩, sshd 설정을 확인하세요.";
                }
                else if (exitCode == -1)
                {
                    feedback = "0%: WinForms에서 프로세스를 시작하거나 감시하는 중 예외가 발생했습니다. 명령 형식과 콘솔 실행 권한을 확인하세요.";
                }
                else
                {
                    feedback = "0%: 학습 프로세스가 실패했습니다. 콘솔 마지막 오류를 확인하세요. 흔한 원인은 누락/손상 이미지, manifest.json 누락, 잘못된 tub 경로, Python 패키지 누락입니다.";
                }
                RefreshMetricBars();
                return 0;
            }

            latestProcessScore = modelUpdated ? 100 : 85;
            var overall = (int)Math.Round(latestProcessScore * 0.55 + dataScore * 0.25 + throttleScore * 0.10 + anomalyScore * 0.10);
            overall = Math.Max(0, Math.Min(100, overall));
            latestOverallScore = overall;

            var modelText = modelUpdated
                ? "모델 파일이 실행 시작 이후 생성/갱신되었습니다"
                : "종료 코드는 0이지만 모델 갱신 여부를 자동 확인하지 못했습니다";
            feedback = $"{overall}%: {modelText}. 데이터 무결성 {dataScore}%, 스로틀 품질 {throttleScore}%, 이상치 관리 {anomalyScore}% 기준으로 계산했습니다.";
            RefreshMetricBars();
            return overall;
        }

        private void InitializeTrainingAnalysisControls()
        {
            // 이 메서드는 더 이상 그래프 컨트롤을 동적으로 만들지 않습니다.
            // 그래프/통계 컨트롤은 Form2.Designer.cs에 배치되어 디자이너에서도 보이도록 유지합니다.
            AutoScroll = true;
            MinimumSize = new Size(1040, 760);
            resultBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            groupLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            groupMetrics.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlMetricsGraph.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblMetricSummary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            RefreshMetricBars();
        }

        private void Form2_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!isTrainingRunning)
            {
                return;
            }

            e.Cancel = true;
            MessageBox.Show(this,
                "학습 실행 중에는 AI 학습 창을 닫을 수 없습니다. 열린 콘솔에서 학습을 종료하거나 완료한 뒤 닫아 주세요.",
                "학습 실행 중",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private Form1.TrainingDatasetPreview? RefreshTrainingPreview()
        {
            if (cmbDatasetMode.SelectedIndex < 0)
            {
                return null;
            }

            try
            {
                latestPreview = owner.GetTrainingDatasetPreviewForDialog(cmbDatasetMode.SelectedIndex, chkExcludeAnomaly.Checked);
                UpdateMetricSummary(latestPreview);
            }
            catch (Exception ex)
            {
                latestPreview = null;
                if (lblMetricSummary != null)
                {
                    lblMetricSummary.Text = "학습 데이터 통계를 계산하지 못했습니다.\n" + ex.Message;
                }
            }

            RefreshMetricBars();
            return latestPreview;
        }

        private void UpdateMetricSummary(Form1.TrainingDatasetPreview? preview)
        {
            if (lblMetricSummary == null)
            {
                return;
            }

            if (preview == null)
            {
                lblMetricSummary.Text = "학습 데이터가 아직 로드되지 않았습니다.";
                return;
            }

            lblMetricSummary.Text =
                $"범위: {preview.ModeName}\n" +
                $"전체 {preview.TotalFrames} / 후보 {preview.CandidateFrames} / 사용 가능 {preview.UsableFrames}\n" +
                $"삭제 제외 {preview.DeletedFrames} / 편집 반영 {preview.EditedFrames}\n" +
                $"이상치 전체 {preview.TotalAnomalyFrames}, 후보 내 {preview.CandidateAnomalyFrames}\n" +
                $"스로틀 + {preview.PositiveThrottleFrames}, 0/후진 {preview.ZeroOrReverseThrottleFrames}\n" +
                $"스로틀 평균 {FormatDouble(preview.ThrottleAverage)}, 범위 {FormatDouble(preview.ThrottleMin)} ~ {FormatDouble(preview.ThrottleMax)}";
        }

        private void RefreshMetricBars()
        {
            var dataScore = latestPreview?.DataIntegrityScore ?? 0;
            var throttleScore = latestPreview?.ThrottleQualityScore ?? 0;
            var anomalyScore = latestPreview?.AnomalyControlScore ?? 0;
            var processScore = latestProcessScore < 0 ? 0 : latestProcessScore;

            var overall = latestOverallScore >= 0
                ? latestOverallScore
                : latestPreview == null
                    ? 0
                    : (int)Math.Round(dataScore * 0.45 + throttleScore * 0.25 + anomalyScore * 0.20 + latestPreview.AvailabilityScore * 0.10);
            overall = Math.Max(0, Math.Min(100, overall));

            SetMetricBar(pnlMetricDataFill, pnlMetricDataBack, lblMetricDataValue, dataScore);
            SetMetricBar(pnlMetricThrottleFill, pnlMetricThrottleBack, lblMetricThrottleValue, throttleScore);
            SetMetricBar(pnlMetricAnomalyFill, pnlMetricAnomalyBack, lblMetricAnomalyValue, anomalyScore);
            SetMetricBar(pnlMetricProcessFill, pnlMetricProcessBack, lblMetricProcessValue, processScore);

            lblMetricOverall.BackColor = ScoreToColor(overall);
            lblMetricOverall.ForeColor = overall < 60 ? Color.White : Color.Black;
            lblMetricOverall.Text = overall.ToString(CultureInfo.InvariantCulture) + "%\r\n" + (latestOverallScore >= 0 ? "최종" : "예상");
        }

        private static void SetMetricBar(Panel fill, Panel back, Label valueLabel, int score)
        {
            score = Math.Max(0, Math.Min(100, score));
            var maxWidth = Math.Max(0, back.ClientSize.Width - 2);
            fill.SetBounds(1, 1, (int)Math.Round(maxWidth * score / 100.0), Math.Max(0, back.ClientSize.Height - 2));
            fill.BackColor = ScoreToColor(score);
            valueLabel.Text = score.ToString(CultureInfo.InvariantCulture) + "%";
        }

        private void pnlMetricsGraph_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.White);
            using var titleFont = new Font(Font.FontFamily, 9.5F, FontStyle.Bold);
            using var smallFont = new Font(Font.FontFamily, 8.5F, FontStyle.Regular);

            if (latestPreview == null)
            {
                TextRenderer.DrawText(g, "학습 데이터 통계 없음", titleFont, new Rectangle(8, 8, 390, 24), Color.DimGray);
                return;
            }

            var dataScore = latestPreview.DataIntegrityScore;
            var throttleScore = latestPreview.ThrottleQualityScore;
            var anomalyScore = latestPreview.AnomalyControlScore;
            var modelScore = latestProcessScore < 0 ? 0 : latestProcessScore;

            DrawMetricBar(g, "데이터 무결성", dataScore, 10, titleFont, smallFont);
            DrawMetricBar(g, "스로틀 품질", throttleScore, 43, titleFont, smallFont);
            DrawMetricBar(g, "이상치 관리", anomalyScore, 76, titleFont, smallFont);
            DrawMetricBar(g, "모델/프로세스", modelScore, 109, titleFont, smallFont);

            var overall = latestOverallScore >= 0
                ? latestOverallScore
                : (int)Math.Round(dataScore * 0.40 + throttleScore * 0.30 + anomalyScore * 0.20 + latestPreview.AvailabilityScore * 0.10);
            using var overallBrush = new SolidBrush(ScoreToColor(overall));
            g.FillRectangle(overallBrush, new Rectangle(315, 12, 96, 124));
            TextRenderer.DrawText(g, overall.ToString(CultureInfo.InvariantCulture) + "%", new Font(Font.FontFamily, 20F, FontStyle.Bold), new Rectangle(315, 42, 96, 45), overall < 60 ? Color.White : Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            TextRenderer.DrawText(g, latestOverallScore >= 0 ? "최종" : "예상", smallFont, new Rectangle(315, 88, 96, 24), overall < 60 ? Color.White : Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private static void DrawMetricBar(Graphics g, string label, int score, int y, Font titleFont, Font smallFont)
        {
            var labelRect = new Rectangle(10, y, 92, 22);
            TextRenderer.DrawText(g, label, titleFont, labelRect, Color.Black, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            var barRect = new Rectangle(106, y + 3, 160, 16);
            g.DrawRectangle(Pens.Silver, barRect);
            var fillWidth = Math.Max(0, Math.Min(barRect.Width, (int)Math.Round(barRect.Width * score / 100.0)));
            using var brush = new SolidBrush(ScoreToColor(score));
            g.FillRectangle(brush, new Rectangle(barRect.Left + 1, barRect.Top + 1, Math.Max(0, fillWidth - 2), barRect.Height - 1));
            TextRenderer.DrawText(g, score.ToString(CultureInfo.InvariantCulture) + "%", smallFont, new Rectangle(272, y, 38, 22), Color.Black, TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        }

        private static string FormatDouble(double? value)
        {
            return value.HasValue ? value.Value.ToString("0.###", CultureInfo.InvariantCulture) : "-";
        }

        private void SetScore(int? score, string title, string feedback)
        {
            if (score.HasValue)
            {
                lblScore.BackColor = ScoreToColor(score.Value);
                lblScore.ForeColor = score.Value < 60 ? Color.White : Color.Black;
            }
            else
            {
                lblScore.BackColor = Color.SteelBlue;
                lblScore.ForeColor = Color.White;
            }
            lblScore.Text = title;
            lblFeedback.Text = feedback;
            RefreshMetricBars();
        }

        private static Color ScoreToColor(int score)
        {
            score = Math.Max(0, Math.Min(100, score));
            if (score <= 50)
            {
                var ratio = score / 50.0;
                var r = 230;
                var g = (int)(30 + (215 - 30) * ratio);
                var b = 25;
                return Color.FromArgb(r, g, b);
            }
            else
            {
                var ratio = (score - 50) / 50.0;
                var r = (int)(230 + (34 - 230) * ratio);
                var g = (int)(215 + (170 - 215) * ratio);
                var b = (int)(25 + (80 - 25) * ratio);
                return Color.FromArgb(r, g, b);
            }
        }

        private void AppendLog(string text)
        {
            txtLog.AppendText("[" + DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture) + "] " + text + Environment.NewLine);
        }
    }
}
