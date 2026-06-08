using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TeamApp
{
    partial class Form2
    {
        private IContainer components = null!;
        private Label lblTitle = null!;
        private Label lblGuide = null!;
        private GroupBox resultBox = null!;
        private Label lblScore = null!;
        private Label lblFeedback = null!;
        private GroupBox groupSettings = null!;
        private ComboBox cmbDatasetMode = null!;
        private CheckBox chkExcludeAnomaly = null!;
        private ComboBox cmbEnvironment = null!;
        private TextBox txtData = null!;
        private Button btnBrowseData = null!;
        private TextBox txtModel = null!;
        private Button btnBrowseModel = null!;
        private TextBox txtExtraArgs = null!;
        private GroupBox groupRemote = null!;
        private TextBox txtActivate = null!;
        private TextBox txtRemoteWork = null!;
        private TextBox txtSshUser = null!;
        private TextBox txtSshHost = null!;
        private TextBox txtSshPort = null!;
        private CheckBox chkPasswordInput = null!;
        private Button btnUseLoaded = null!;
        private Button btnWsl = null!;
        private Button btnVBox = null!;
        private Button btnExport = null!;
        private Button btnGenerate = null!;
        private Label lblCommand = null!;
        private CheckBox chkManualEdit = null!;
        private TextBox txtCommand = null!;
        private Button btnRun = null!;
        private Button btnStop = null!;
        private Button btnClose = null!;
        private GroupBox groupLog = null!;
        private TextBox txtLog = null!;
        private GroupBox groupMetrics = null!;
        private Panel pnlMetricsGraph = null!;
        private Label lblMetricSummary = null!;
        private Label lblMetricDataTitle = null!;
        private Label lblMetricThrottleTitle = null!;
        private Label lblMetricAnomalyTitle = null!;
        private Label lblMetricProcessTitle = null!;
        private Label lblMetricDataValue = null!;
        private Label lblMetricThrottleValue = null!;
        private Label lblMetricAnomalyValue = null!;
        private Label lblMetricProcessValue = null!;
        private Panel pnlMetricDataBack = null!;
        private Panel pnlMetricThrottleBack = null!;
        private Panel pnlMetricAnomalyBack = null!;
        private Panel pnlMetricProcessBack = null!;
        private Panel pnlMetricDataFill = null!;
        private Panel pnlMetricThrottleFill = null!;
        private Panel pnlMetricAnomalyFill = null!;
        private Panel pnlMetricProcessFill = null!;
        private Label lblMetricOverall = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblTitle = new Label();
            lblGuide = new Label();
            resultBox = new GroupBox();
            lblScore = new Label();
            lblFeedback = new Label();
            groupSettings = new GroupBox();
            lblDatasetMode = new Label();
            cmbDatasetMode = new ComboBox();
            chkExcludeAnomaly = new CheckBox();
            lblEnvironment = new Label();
            cmbEnvironment = new ComboBox();
            lblData = new Label();
            txtData = new TextBox();
            btnBrowseData = new Button();
            lblModel = new Label();
            txtModel = new TextBox();
            btnBrowseModel = new Button();
            lblExtraArgs = new Label();
            txtExtraArgs = new TextBox();
            lblExtraExample = new Label();
            groupRemote = new GroupBox();
            lblActivate = new Label();
            txtActivate = new TextBox();
            lblRemoteWork = new Label();
            txtRemoteWork = new TextBox();
            lblSshUser = new Label();
            txtSshUser = new TextBox();
            lblSshHost = new Label();
            txtSshHost = new TextBox();
            lblSshPort = new Label();
            txtSshPort = new TextBox();
            chkPasswordInput = new CheckBox();
            btnUseLoaded = new Button();
            btnWsl = new Button();
            btnVBox = new Button();
            btnExport = new Button();
            btnGenerate = new Button();
            lblCommand = new Label();
            chkManualEdit = new CheckBox();
            txtCommand = new TextBox();
            btnRun = new Button();
            btnStop = new Button();
            btnClose = new Button();
            groupLog = new GroupBox();
            txtLog = new TextBox();
            groupMetrics = new GroupBox();
            pnlMetricsGraph = new Panel();
            lblMetricDataTitle = new Label();
            pnlMetricDataBack = new Panel();
            pnlMetricDataFill = new Panel();
            lblMetricDataValue = new Label();
            lblMetricThrottleTitle = new Label();
            pnlMetricThrottleBack = new Panel();
            pnlMetricThrottleFill = new Panel();
            lblMetricThrottleValue = new Label();
            lblMetricAnomalyTitle = new Label();
            pnlMetricAnomalyBack = new Panel();
            pnlMetricAnomalyFill = new Panel();
            lblMetricAnomalyValue = new Label();
            lblMetricProcessTitle = new Label();
            pnlMetricProcessBack = new Panel();
            pnlMetricProcessFill = new Panel();
            lblMetricProcessValue = new Label();
            lblMetricOverall = new Label();
            lblMetricSummary = new Label();
            resultBox.SuspendLayout();
            groupSettings.SuspendLayout();
            groupRemote.SuspendLayout();
            groupLog.SuspendLayout();
            groupMetrics.SuspendLayout();
            pnlMetricsGraph.SuspendLayout();
            pnlMetricDataBack.SuspendLayout();
            pnlMetricThrottleBack.SuspendLayout();
            pnlMetricAnomalyBack.SuspendLayout();
            pnlMetricProcessBack.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("맑은 고딕", 16F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblTitle.Location = new Point(14, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(180, 32);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "AI 학습";
            // 
            // lblGuide
            // 
            lblGuide.Location = new Point(14, 48);
            lblGuide.Name = "lblGuide";
            lblGuide.Size = new Size(720, 38);
            lblGuide.TabIndex = 1;
            lblGuide.Text = "학습 범위, 실행 환경, 경로를 이 창에서 설정합니다. VirtualBox는 config.py가 있는 원격 mycar 폴더에서 python train.py를 실행합니다.";
            // 
            // resultBox
            // 
            resultBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            resultBox.Controls.Add(lblScore);
            resultBox.Controls.Add(lblFeedback);
            resultBox.Location = new Point(760, 12);
            resultBox.Name = "resultBox";
            resultBox.Size = new Size(290, 150);
            resultBox.TabIndex = 2;
            resultBox.TabStop = false;
            resultBox.Text = "학습 결과";
            // 
            // lblScore
            // 
            lblScore.BackColor = Color.Gainsboro;
            lblScore.Font = new Font("맑은 고딕", 28F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblScore.Location = new Point(16, 24);
            lblScore.Name = "lblScore";
            lblScore.Size = new Size(258, 54);
            lblScore.TabIndex = 0;
            lblScore.Text = "대기";
            lblScore.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblFeedback
            // 
            lblFeedback.Location = new Point(16, 86);
            lblFeedback.Name = "lblFeedback";
            lblFeedback.Size = new Size(258, 52);
            lblFeedback.TabIndex = 1;
            lblFeedback.Text = "명령을 생성한 뒤 학습을 실행하세요.";
            // 
            // groupSettings
            // 
            groupSettings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupSettings.Controls.Add(lblDatasetMode);
            groupSettings.Controls.Add(cmbDatasetMode);
            groupSettings.Controls.Add(chkExcludeAnomaly);
            groupSettings.Controls.Add(lblEnvironment);
            groupSettings.Controls.Add(cmbEnvironment);
            groupSettings.Controls.Add(lblData);
            groupSettings.Controls.Add(txtData);
            groupSettings.Controls.Add(btnBrowseData);
            groupSettings.Controls.Add(lblModel);
            groupSettings.Controls.Add(txtModel);
            groupSettings.Controls.Add(btnBrowseModel);
            groupSettings.Controls.Add(lblExtraArgs);
            groupSettings.Controls.Add(txtExtraArgs);
            groupSettings.Controls.Add(lblExtraExample);
            groupSettings.Controls.Add(groupRemote);
            groupSettings.Location = new Point(14, 96);
            groupSettings.Name = "groupSettings";
            groupSettings.Size = new Size(720, 315);
            groupSettings.TabIndex = 3;
            groupSettings.TabStop = false;
            groupSettings.Text = "학습 환경 설정";
            // 
            // lblDatasetMode
            // 
            lblDatasetMode.Location = new Point(14, 30);
            lblDatasetMode.Name = "lblDatasetMode";
            lblDatasetMode.Size = new Size(130, 22);
            lblDatasetMode.TabIndex = 0;
            lblDatasetMode.Text = "학습 데이터 범위";
            // 
            // cmbDatasetMode
            // 
            cmbDatasetMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDatasetMode.Items.AddRange(new object[] { "이상치 포함 전체 데이터 학습", "이상치 제외 학습", "데이터 필터링 선택군만 학습" });
            cmbDatasetMode.Location = new Point(150, 28);
            cmbDatasetMode.Name = "cmbDatasetMode";
            cmbDatasetMode.Size = new Size(330, 23);
            cmbDatasetMode.TabIndex = 1;
            cmbDatasetMode.SelectedIndexChanged += cmbDatasetMode_SelectedIndexChanged;
            // 
            // chkExcludeAnomaly
            // 
            chkExcludeAnomaly.Location = new Point(500, 27);
            chkExcludeAnomaly.Name = "chkExcludeAnomaly";
            chkExcludeAnomaly.Size = new Size(190, 24);
            chkExcludeAnomaly.TabIndex = 2;
            chkExcludeAnomaly.Text = "선택군에서도 이상치 제외";
            chkExcludeAnomaly.CheckedChanged += chkExcludeAnomaly_CheckedChanged;
            // 
            // lblEnvironment
            // 
            lblEnvironment.Location = new Point(14, 66);
            lblEnvironment.Name = "lblEnvironment";
            lblEnvironment.Size = new Size(130, 22);
            lblEnvironment.TabIndex = 3;
            lblEnvironment.Text = "실행 환경";
            // 
            // cmbEnvironment
            // 
            cmbEnvironment.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEnvironment.Items.AddRange(new object[] { "Windows 환경 빌드", "WSL 환경 빌드", "VirtualBox 환경 빌드" });
            cmbEnvironment.Location = new Point(150, 64);
            cmbEnvironment.Name = "cmbEnvironment";
            cmbEnvironment.Size = new Size(220, 23);
            cmbEnvironment.TabIndex = 4;
            cmbEnvironment.SelectedIndexChanged += cmbEnvironment_SelectedIndexChanged;
            // 
            // lblData
            // 
            lblData.Location = new Point(14, 102);
            lblData.Name = "lblData";
            lblData.Size = new Size(130, 22);
            lblData.TabIndex = 5;
            lblData.Text = "학습 data/tub";
            // 
            // txtData
            // 
            txtData.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtData.Location = new Point(150, 100);
            txtData.Name = "txtData";
            txtData.Size = new Size(450, 23);
            txtData.TabIndex = 6;
            txtData.TextChanged += GenerateCommandOnChanged;
            // 
            // btnBrowseData
            // 
            btnBrowseData.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowseData.Location = new Point(610, 98);
            btnBrowseData.Name = "btnBrowseData";
            btnBrowseData.Size = new Size(94, 28);
            btnBrowseData.TabIndex = 7;
            btnBrowseData.Text = "찾아보기";
            btnBrowseData.UseVisualStyleBackColor = true;
            btnBrowseData.Click += btnBrowseData_Click;
            // 
            // lblModel
            // 
            lblModel.Location = new Point(14, 138);
            lblModel.Name = "lblModel";
            lblModel.Size = new Size(130, 22);
            lblModel.TabIndex = 8;
            lblModel.Text = "모델 저장 경로";
            // 
            // txtModel
            // 
            txtModel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtModel.Location = new Point(150, 136);
            txtModel.Name = "txtModel";
            txtModel.Size = new Size(450, 23);
            txtModel.TabIndex = 9;
            txtModel.TextChanged += GenerateCommandOnChanged;
            // 
            // btnBrowseModel
            // 
            btnBrowseModel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowseModel.Location = new Point(610, 134);
            btnBrowseModel.Name = "btnBrowseModel";
            btnBrowseModel.Size = new Size(94, 28);
            btnBrowseModel.TabIndex = 10;
            btnBrowseModel.Text = "저장 위치";
            btnBrowseModel.UseVisualStyleBackColor = true;
            btnBrowseModel.Click += btnBrowseModel_Click;
            // 
            // lblExtraArgs
            // 
            lblExtraArgs.Location = new Point(14, 174);
            lblExtraArgs.Name = "lblExtraArgs";
            lblExtraArgs.Size = new Size(130, 22);
            lblExtraArgs.TabIndex = 11;
            lblExtraArgs.Text = "추가/보정 인자";
            // 
            // txtExtraArgs
            // 
            txtExtraArgs.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtExtraArgs.Location = new Point(150, 172);
            txtExtraArgs.Name = "txtExtraArgs";
            txtExtraArgs.Size = new Size(450, 23);
            txtExtraArgs.TabIndex = 12;
            txtExtraArgs.TextChanged += GenerateCommandOnChanged;
            // 
            // lblExtraExample
            // 
            lblExtraExample.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblExtraExample.Location = new Point(610, 174);
            lblExtraExample.Name = "lblExtraExample";
            lblExtraExample.Size = new Size(120, 22);
            lblExtraExample.TabIndex = 13;
            lblExtraExample.Text = "예: --type linear";
            // 
            // groupRemote
            // 
            groupRemote.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupRemote.Controls.Add(lblActivate);
            groupRemote.Controls.Add(txtActivate);
            groupRemote.Controls.Add(lblRemoteWork);
            groupRemote.Controls.Add(txtRemoteWork);
            groupRemote.Controls.Add(lblSshUser);
            groupRemote.Controls.Add(txtSshUser);
            groupRemote.Controls.Add(lblSshHost);
            groupRemote.Controls.Add(txtSshHost);
            groupRemote.Controls.Add(lblSshPort);
            groupRemote.Controls.Add(txtSshPort);
            groupRemote.Controls.Add(chkPasswordInput);
            groupRemote.Location = new Point(14, 205);
            groupRemote.Name = "groupRemote";
            groupRemote.Size = new Size(690, 96);
            groupRemote.TabIndex = 14;
            groupRemote.TabStop = false;
            groupRemote.Text = "WSL / VirtualBox 옵션";
            // 
            // lblActivate
            // 
            lblActivate.Location = new Point(10, 28);
            lblActivate.Name = "lblActivate";
            lblActivate.Size = new Size(85, 22);
            lblActivate.TabIndex = 0;
            lblActivate.Text = "환경 활성화";
            // 
            // txtActivate
            // 
            txtActivate.Location = new Point(96, 25);
            txtActivate.Name = "txtActivate";
            txtActivate.Size = new Size(245, 23);
            txtActivate.TabIndex = 1;
            txtActivate.TextChanged += GenerateCommandOnChanged;
            // 
            // lblRemoteWork
            // 
            lblRemoteWork.Location = new Point(356, 28);
            lblRemoteWork.Name = "lblRemoteWork";
            lblRemoteWork.Size = new Size(80, 22);
            lblRemoteWork.TabIndex = 2;
            lblRemoteWork.Text = "원격 mycar";
            // 
            // txtRemoteWork
            // 
            txtRemoteWork.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtRemoteWork.Location = new Point(436, 25);
            txtRemoteWork.Name = "txtRemoteWork";
            txtRemoteWork.Size = new Size(235, 23);
            txtRemoteWork.TabIndex = 3;
            txtRemoteWork.TextChanged += GenerateCommandOnChanged;
            // 
            // lblSshUser
            // 
            lblSshUser.Location = new Point(10, 62);
            lblSshUser.Name = "lblSshUser";
            lblSshUser.Size = new Size(70, 22);
            lblSshUser.TabIndex = 4;
            lblSshUser.Text = "SSH user";
            // 
            // txtSshUser
            // 
            txtSshUser.Location = new Point(80, 59);
            txtSshUser.Name = "txtSshUser";
            txtSshUser.Size = new Size(100, 23);
            txtSshUser.TabIndex = 5;
            txtSshUser.TextChanged += GenerateCommandOnChanged;
            // 
            // lblSshHost
            // 
            lblSshHost.Location = new Point(190, 62);
            lblSshHost.Name = "lblSshHost";
            lblSshHost.Size = new Size(40, 22);
            lblSshHost.TabIndex = 6;
            lblSshHost.Text = "host";
            // 
            // txtSshHost
            // 
            txtSshHost.Location = new Point(230, 59);
            txtSshHost.Name = "txtSshHost";
            txtSshHost.Size = new Size(100, 23);
            txtSshHost.TabIndex = 7;
            txtSshHost.TextChanged += GenerateCommandOnChanged;
            // 
            // lblSshPort
            // 
            lblSshPort.Location = new Point(340, 62);
            lblSshPort.Name = "lblSshPort";
            lblSshPort.Size = new Size(40, 22);
            lblSshPort.TabIndex = 8;
            lblSshPort.Text = "port";
            // 
            // txtSshPort
            // 
            txtSshPort.Location = new Point(380, 59);
            txtSshPort.Name = "txtSshPort";
            txtSshPort.Size = new Size(65, 23);
            txtSshPort.TabIndex = 9;
            txtSshPort.TextChanged += GenerateCommandOnChanged;
            // 
            // chkPasswordInput
            // 
            chkPasswordInput.Location = new Point(464, 58);
            chkPasswordInput.Name = "chkPasswordInput";
            chkPasswordInput.Size = new Size(210, 24);
            chkPasswordInput.TabIndex = 10;
            chkPasswordInput.Text = "비밀번호 입력형 콘솔 사용";
            chkPasswordInput.CheckedChanged += GenerateCommandOnChanged;
            // 
            // btnUseLoaded
            // 
            btnUseLoaded.Location = new Point(14, 423);
            btnUseLoaded.Name = "btnUseLoaded";
            btnUseLoaded.Size = new Size(130, 30);
            btnUseLoaded.TabIndex = 4;
            btnUseLoaded.Text = "현재 로드 경로";
            btnUseLoaded.UseVisualStyleBackColor = true;
            btnUseLoaded.Click += btnUseLoaded_Click;
            // 
            // btnWsl
            // 
            btnWsl.Location = new Point(154, 423);
            btnWsl.Name = "btnWsl";
            btnWsl.Size = new Size(130, 30);
            btnWsl.TabIndex = 5;
            btnWsl.Text = "WSL 경로 변환";
            btnWsl.UseVisualStyleBackColor = true;
            btnWsl.Click += btnWsl_Click;
            // 
            // btnVBox
            // 
            btnVBox.Location = new Point(294, 423);
            btnVBox.Name = "btnVBox";
            btnVBox.Size = new Size(160, 30);
            btnVBox.TabIndex = 6;
            btnVBox.Text = "VirtualBox 경로 변환";
            btnVBox.UseVisualStyleBackColor = true;
            btnVBox.Click += btnVBox_Click;
            // 
            // btnExport
            // 
            btnExport.Location = new Point(464, 423);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(140, 30);
            btnExport.TabIndex = 7;
            btnExport.Text = "학습 데이터 생성";
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Click += btnExport_Click;
            // 
            // btnGenerate
            // 
            btnGenerate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnGenerate.Location = new Point(614, 423);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new Size(120, 30);
            btnGenerate.TabIndex = 8;
            btnGenerate.Text = "명령 생성";
            btnGenerate.UseVisualStyleBackColor = true;
            btnGenerate.Click += btnGenerate_Click;
            // 
            // lblCommand
            // 
            lblCommand.Location = new Point(14, 466);
            lblCommand.Name = "lblCommand";
            lblCommand.Size = new Size(100, 22);
            lblCommand.TabIndex = 9;
            lblCommand.Text = "학습 명령";
            // 
            // chkManualEdit
            // 
            chkManualEdit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            chkManualEdit.Location = new Point(630, 464);
            chkManualEdit.Name = "chkManualEdit";
            chkManualEdit.Size = new Size(110, 24);
            chkManualEdit.TabIndex = 10;
            chkManualEdit.Text = "수동 편집";
            chkManualEdit.CheckedChanged += chkManualEdit_CheckedChanged;
            // 
            // txtCommand
            // 
            txtCommand.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCommand.Location = new Point(14, 492);
            txtCommand.Multiline = true;
            txtCommand.Name = "txtCommand";
            txtCommand.ReadOnly = true;
            txtCommand.ScrollBars = ScrollBars.Vertical;
            txtCommand.Size = new Size(720, 86);
            txtCommand.TabIndex = 11;
            // 
            // btnRun
            // 
            btnRun.Font = new Font("맑은 고딕", 10F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnRun.Location = new Point(14, 803);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(160, 38);
            btnRun.TabIndex = 13;
            btnRun.Text = "학습 실행";
            btnRun.UseVisualStyleBackColor = true;
            btnRun.Click += btnRun_Click;
            // 
            // btnStop
            // 
            btnStop.Enabled = false;
            btnStop.Font = new Font("맑은 고딕", 10F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnStop.Location = new Point(184, 803);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(120, 38);
            btnStop.TabIndex = 14;
            btnStop.Text = "학습 중지";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(314, 803);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(90, 38);
            btnClose.TabIndex = 15;
            btnClose.Text = "닫기";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // groupLog
            // 
            groupLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            groupLog.Controls.Add(txtLog);
            groupLog.Location = new Point(760, 170);
            groupLog.Name = "groupLog";
            groupLog.Size = new Size(290, 650);
            groupLog.TabIndex = 15;
            groupLog.TabStop = false;
            groupLog.Text = "실행 로그 / 안내";
            // 
            // txtLog
            // 
            txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtLog.Location = new Point(10, 22);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(270, 615);
            txtLog.TabIndex = 0;
            // 
            // groupMetrics
            // 
            groupMetrics.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupMetrics.Controls.Add(pnlMetricsGraph);
            groupMetrics.Controls.Add(lblMetricSummary);
            groupMetrics.Location = new Point(14, 590);
            groupMetrics.Name = "groupMetrics";
            groupMetrics.Size = new Size(720, 207);
            groupMetrics.TabIndex = 12;
            groupMetrics.TabStop = false;
            groupMetrics.Text = "학습 진행 / 데이터 품질 그래프";
            // 
            // pnlMetricsGraph
            // 
            pnlMetricsGraph.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlMetricsGraph.BackColor = Color.White;
            pnlMetricsGraph.BorderStyle = BorderStyle.FixedSingle;
            pnlMetricsGraph.Controls.Add(lblMetricDataTitle);
            pnlMetricsGraph.Controls.Add(pnlMetricDataBack);
            pnlMetricsGraph.Controls.Add(lblMetricDataValue);
            pnlMetricsGraph.Controls.Add(lblMetricThrottleTitle);
            pnlMetricsGraph.Controls.Add(pnlMetricThrottleBack);
            pnlMetricsGraph.Controls.Add(lblMetricThrottleValue);
            pnlMetricsGraph.Controls.Add(lblMetricAnomalyTitle);
            pnlMetricsGraph.Controls.Add(pnlMetricAnomalyBack);
            pnlMetricsGraph.Controls.Add(lblMetricAnomalyValue);
            pnlMetricsGraph.Controls.Add(lblMetricProcessTitle);
            pnlMetricsGraph.Controls.Add(pnlMetricProcessBack);
            pnlMetricsGraph.Controls.Add(lblMetricProcessValue);
            pnlMetricsGraph.Controls.Add(lblMetricOverall);
            pnlMetricsGraph.Location = new Point(12, 22);
            pnlMetricsGraph.Name = "pnlMetricsGraph";
            pnlMetricsGraph.Size = new Size(696, 117);
            pnlMetricsGraph.TabIndex = 0;
            // 
            // lblMetricDataTitle
            // 
            lblMetricDataTitle.Location = new Point(12, 12);
            lblMetricDataTitle.Name = "lblMetricDataTitle";
            lblMetricDataTitle.Size = new Size(100, 20);
            lblMetricDataTitle.TabIndex = 0;
            lblMetricDataTitle.Text = "데이터 무결성";
            // 
            // pnlMetricDataBack
            // 
            pnlMetricDataBack.BackColor = Color.WhiteSmoke;
            pnlMetricDataBack.BorderStyle = BorderStyle.FixedSingle;
            pnlMetricDataBack.Controls.Add(pnlMetricDataFill);
            pnlMetricDataBack.Location = new Point(116, 14);
            pnlMetricDataBack.Name = "pnlMetricDataBack";
            pnlMetricDataBack.Size = new Size(245, 16);
            pnlMetricDataBack.TabIndex = 1;
            // 
            // pnlMetricDataFill
            // 
            pnlMetricDataFill.BackColor = Color.Gainsboro;
            pnlMetricDataFill.Location = new Point(1, 1);
            pnlMetricDataFill.Name = "pnlMetricDataFill";
            pnlMetricDataFill.Size = new Size(0, 12);
            pnlMetricDataFill.TabIndex = 0;
            // 
            // lblMetricDataValue
            // 
            lblMetricDataValue.Location = new Point(366, 10);
            lblMetricDataValue.Name = "lblMetricDataValue";
            lblMetricDataValue.Size = new Size(42, 22);
            lblMetricDataValue.TabIndex = 2;
            lblMetricDataValue.Text = "0%";
            lblMetricDataValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblMetricThrottleTitle
            // 
            lblMetricThrottleTitle.Location = new Point(12, 36);
            lblMetricThrottleTitle.Name = "lblMetricThrottleTitle";
            lblMetricThrottleTitle.Size = new Size(100, 20);
            lblMetricThrottleTitle.TabIndex = 3;
            lblMetricThrottleTitle.Text = "스로틀 품질";
            // 
            // pnlMetricThrottleBack
            // 
            pnlMetricThrottleBack.BackColor = Color.WhiteSmoke;
            pnlMetricThrottleBack.BorderStyle = BorderStyle.FixedSingle;
            pnlMetricThrottleBack.Controls.Add(pnlMetricThrottleFill);
            pnlMetricThrottleBack.Location = new Point(116, 38);
            pnlMetricThrottleBack.Name = "pnlMetricThrottleBack";
            pnlMetricThrottleBack.Size = new Size(245, 16);
            pnlMetricThrottleBack.TabIndex = 4;
            // 
            // pnlMetricThrottleFill
            // 
            pnlMetricThrottleFill.BackColor = Color.Gainsboro;
            pnlMetricThrottleFill.Location = new Point(1, 1);
            pnlMetricThrottleFill.Name = "pnlMetricThrottleFill";
            pnlMetricThrottleFill.Size = new Size(0, 12);
            pnlMetricThrottleFill.TabIndex = 0;
            // 
            // lblMetricThrottleValue
            // 
            lblMetricThrottleValue.Location = new Point(366, 34);
            lblMetricThrottleValue.Name = "lblMetricThrottleValue";
            lblMetricThrottleValue.Size = new Size(42, 22);
            lblMetricThrottleValue.TabIndex = 5;
            lblMetricThrottleValue.Text = "0%";
            lblMetricThrottleValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblMetricAnomalyTitle
            // 
            lblMetricAnomalyTitle.Location = new Point(12, 60);
            lblMetricAnomalyTitle.Name = "lblMetricAnomalyTitle";
            lblMetricAnomalyTitle.Size = new Size(100, 20);
            lblMetricAnomalyTitle.TabIndex = 6;
            lblMetricAnomalyTitle.Text = "이상치 관리";
            // 
            // pnlMetricAnomalyBack
            // 
            pnlMetricAnomalyBack.BackColor = Color.WhiteSmoke;
            pnlMetricAnomalyBack.BorderStyle = BorderStyle.FixedSingle;
            pnlMetricAnomalyBack.Controls.Add(pnlMetricAnomalyFill);
            pnlMetricAnomalyBack.Location = new Point(116, 62);
            pnlMetricAnomalyBack.Name = "pnlMetricAnomalyBack";
            pnlMetricAnomalyBack.Size = new Size(245, 16);
            pnlMetricAnomalyBack.TabIndex = 7;
            // 
            // pnlMetricAnomalyFill
            // 
            pnlMetricAnomalyFill.BackColor = Color.Gainsboro;
            pnlMetricAnomalyFill.Location = new Point(1, 1);
            pnlMetricAnomalyFill.Name = "pnlMetricAnomalyFill";
            pnlMetricAnomalyFill.Size = new Size(0, 12);
            pnlMetricAnomalyFill.TabIndex = 0;
            // 
            // lblMetricAnomalyValue
            // 
            lblMetricAnomalyValue.Location = new Point(366, 58);
            lblMetricAnomalyValue.Name = "lblMetricAnomalyValue";
            lblMetricAnomalyValue.Size = new Size(42, 22);
            lblMetricAnomalyValue.TabIndex = 8;
            lblMetricAnomalyValue.Text = "0%";
            lblMetricAnomalyValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblMetricProcessTitle
            // 
            lblMetricProcessTitle.Location = new Point(12, 84);
            lblMetricProcessTitle.Name = "lblMetricProcessTitle";
            lblMetricProcessTitle.Size = new Size(100, 20);
            lblMetricProcessTitle.TabIndex = 9;
            lblMetricProcessTitle.Text = "모델/프로세스";
            // 
            // pnlMetricProcessBack
            // 
            pnlMetricProcessBack.BackColor = Color.WhiteSmoke;
            pnlMetricProcessBack.BorderStyle = BorderStyle.FixedSingle;
            pnlMetricProcessBack.Controls.Add(pnlMetricProcessFill);
            pnlMetricProcessBack.Location = new Point(116, 86);
            pnlMetricProcessBack.Name = "pnlMetricProcessBack";
            pnlMetricProcessBack.Size = new Size(245, 16);
            pnlMetricProcessBack.TabIndex = 10;
            // 
            // pnlMetricProcessFill
            // 
            pnlMetricProcessFill.BackColor = Color.Gainsboro;
            pnlMetricProcessFill.Location = new Point(1, 1);
            pnlMetricProcessFill.Name = "pnlMetricProcessFill";
            pnlMetricProcessFill.Size = new Size(0, 12);
            pnlMetricProcessFill.TabIndex = 0;
            // 
            // lblMetricProcessValue
            // 
            lblMetricProcessValue.Location = new Point(366, 82);
            lblMetricProcessValue.Name = "lblMetricProcessValue";
            lblMetricProcessValue.Size = new Size(42, 22);
            lblMetricProcessValue.TabIndex = 11;
            lblMetricProcessValue.Text = "0%";
            lblMetricProcessValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblMetricOverall
            // 
            lblMetricOverall.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblMetricOverall.BackColor = Color.Gainsboro;
            lblMetricOverall.Font = new Font("맑은 고딕", 20F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblMetricOverall.Location = new Point(590, 13);
            lblMetricOverall.Name = "lblMetricOverall";
            lblMetricOverall.Size = new Size(92, 78);
            lblMetricOverall.TabIndex = 12;
            lblMetricOverall.Text = "0%\r\n예상";
            lblMetricOverall.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblMetricSummary
            // 
            lblMetricSummary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblMetricSummary.Location = new Point(12, 145);
            lblMetricSummary.Name = "lblMetricSummary";
            lblMetricSummary.Size = new Size(696, 56);
            lblMetricSummary.TabIndex = 1;
            lblMetricSummary.Text = "학습 데이터 요약을 계산 중입니다.";
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(1064, 850);
            Controls.Add(lblTitle);
            Controls.Add(lblGuide);
            Controls.Add(resultBox);
            Controls.Add(groupSettings);
            Controls.Add(btnUseLoaded);
            Controls.Add(btnWsl);
            Controls.Add(btnVBox);
            Controls.Add(btnExport);
            Controls.Add(btnGenerate);
            Controls.Add(lblCommand);
            Controls.Add(chkManualEdit);
            Controls.Add(txtCommand);
            Controls.Add(groupMetrics);
            Controls.Add(btnRun);
            Controls.Add(btnStop);
            Controls.Add(btnClose);
            Controls.Add(groupLog);
            Font = new Font("맑은 고딕", 9F, FontStyle.Regular, GraphicsUnit.Point, 129);
            MinimumSize = new Size(980, 800);
            Name = "Form2";
            StartPosition = FormStartPosition.CenterParent;
            Text = "AI 학습 환경 / 명령 실행";
            resultBox.ResumeLayout(false);
            groupSettings.ResumeLayout(false);
            groupSettings.PerformLayout();
            groupRemote.ResumeLayout(false);
            groupRemote.PerformLayout();
            groupLog.ResumeLayout(false);
            groupLog.PerformLayout();
            groupMetrics.ResumeLayout(false);
            pnlMetricsGraph.ResumeLayout(false);
            pnlMetricDataBack.ResumeLayout(false);
            pnlMetricThrottleBack.ResumeLayout(false);
            pnlMetricAnomalyBack.ResumeLayout(false);
            pnlMetricProcessBack.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private Label lblDatasetMode;
        private Label lblEnvironment;
        private Label lblData;
        private Label lblModel;
        private Label lblExtraArgs;
        private Label lblExtraExample;
        private Label lblActivate;
        private Label lblRemoteWork;
        private Label lblSshUser;
        private Label lblSshHost;
        private Label lblSshPort;
    }
}
