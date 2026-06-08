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
        private Button btnClose = null!;
        private GroupBox groupLog = null!;
        private TextBox txtLog = null!;

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
            components = new Container();
            lblTitle = new Label();
            lblGuide = new Label();
            resultBox = new GroupBox();
            lblScore = new Label();
            lblFeedback = new Label();
            groupSettings = new GroupBox();
            cmbDatasetMode = new ComboBox();
            chkExcludeAnomaly = new CheckBox();
            cmbEnvironment = new ComboBox();
            txtData = new TextBox();
            btnBrowseData = new Button();
            txtModel = new TextBox();
            btnBrowseModel = new Button();
            txtExtraArgs = new TextBox();
            groupRemote = new GroupBox();
            txtActivate = new TextBox();
            txtRemoteWork = new TextBox();
            txtSshUser = new TextBox();
            txtSshHost = new TextBox();
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
            btnClose = new Button();
            groupLog = new GroupBox();
            txtLog = new TextBox();
            var lblDatasetMode = new Label();
            var lblEnvironment = new Label();
            var lblData = new Label();
            var lblModel = new Label();
            var lblExtraArgs = new Label();
            var lblExtraExample = new Label();
            var lblActivate = new Label();
            var lblRemoteWork = new Label();
            var lblSshUser = new Label();
            var lblSshHost = new Label();
            var lblSshPort = new Label();

            resultBox.SuspendLayout();
            groupSettings.SuspendLayout();
            groupRemote.SuspendLayout();
            groupLog.SuspendLayout();
            SuspendLayout();

            // lblTitle
            lblTitle.AutoSize = false;
            lblTitle.Font = new Font("맑은 고딕", 16F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblTitle.Location = new Point(14, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(180, 32);
            lblTitle.Text = "AI 학습";

            // lblGuide
            lblGuide.AutoSize = false;
            lblGuide.Location = new Point(14, 48);
            lblGuide.Name = "lblGuide";
            lblGuide.Size = new Size(720, 38);
            lblGuide.Text = "학습 범위, 실행 환경, 경로를 이 창에서 설정합니다. VirtualBox는 config.py가 있는 원격 mycar 폴더에서 python train.py를 실행합니다.";

            // resultBox
            resultBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            resultBox.Controls.Add(lblScore);
            resultBox.Controls.Add(lblFeedback);
            resultBox.Location = new Point(760, 12);
            resultBox.Name = "resultBox";
            resultBox.Size = new Size(290, 150);
            resultBox.TabStop = false;
            resultBox.Text = "학습 결과";

            // lblScore
            lblScore.BackColor = Color.Gainsboro;
            lblScore.Font = new Font("맑은 고딕", 28F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblScore.Location = new Point(16, 24);
            lblScore.Name = "lblScore";
            lblScore.Size = new Size(258, 54);
            lblScore.Text = "대기";
            lblScore.TextAlign = ContentAlignment.MiddleCenter;

            // lblFeedback
            lblFeedback.Location = new Point(16, 86);
            lblFeedback.Name = "lblFeedback";
            lblFeedback.Size = new Size(258, 52);
            lblFeedback.Text = "명령을 생성한 뒤 학습을 실행하세요.";

            // groupSettings
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
            groupSettings.TabStop = false;
            groupSettings.Text = "학습 환경 설정";

            // lblDatasetMode
            lblDatasetMode.Location = new Point(14, 30);
            lblDatasetMode.Name = "lblDatasetMode";
            lblDatasetMode.Size = new Size(130, 22);
            lblDatasetMode.Text = "학습 데이터 범위";

            // cmbDatasetMode
            cmbDatasetMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDatasetMode.Items.AddRange(new object[] { "이상치 포함 전체 데이터 학습", "이상치 제외 학습", "데이터 필터링 선택군만 학습" });
            cmbDatasetMode.Location = new Point(150, 28);
            cmbDatasetMode.Name = "cmbDatasetMode";
            cmbDatasetMode.Size = new Size(330, 23);
            cmbDatasetMode.SelectedIndexChanged += cmbDatasetMode_SelectedIndexChanged;

            // chkExcludeAnomaly
            chkExcludeAnomaly.Location = new Point(500, 27);
            chkExcludeAnomaly.Name = "chkExcludeAnomaly";
            chkExcludeAnomaly.Size = new Size(190, 24);
            chkExcludeAnomaly.Text = "선택군에서도 이상치 제외";
            chkExcludeAnomaly.CheckedChanged += chkExcludeAnomaly_CheckedChanged;

            // lblEnvironment
            lblEnvironment.Location = new Point(14, 66);
            lblEnvironment.Name = "lblEnvironment";
            lblEnvironment.Size = new Size(130, 22);
            lblEnvironment.Text = "실행 환경";

            // cmbEnvironment
            cmbEnvironment.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEnvironment.Items.AddRange(new object[] { "Windows 환경 빌드", "WSL 환경 빌드", "VirtualBox 환경 빌드" });
            cmbEnvironment.Location = new Point(150, 64);
            cmbEnvironment.Name = "cmbEnvironment";
            cmbEnvironment.Size = new Size(220, 23);
            cmbEnvironment.SelectedIndexChanged += cmbEnvironment_SelectedIndexChanged;

            // lblData
            lblData.Location = new Point(14, 102);
            lblData.Name = "lblData";
            lblData.Size = new Size(130, 22);
            lblData.Text = "학습 data/tub";

            // txtData
            txtData.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtData.Location = new Point(150, 100);
            txtData.Name = "txtData";
            txtData.Size = new Size(450, 23);
            txtData.TextChanged += GenerateCommandOnChanged;

            // btnBrowseData
            btnBrowseData.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowseData.Location = new Point(610, 98);
            btnBrowseData.Name = "btnBrowseData";
            btnBrowseData.Size = new Size(94, 28);
            btnBrowseData.Text = "찾아보기";
            btnBrowseData.UseVisualStyleBackColor = true;
            btnBrowseData.Click += btnBrowseData_Click;

            // lblModel
            lblModel.Location = new Point(14, 138);
            lblModel.Name = "lblModel";
            lblModel.Size = new Size(130, 22);
            lblModel.Text = "모델 저장 경로";

            // txtModel
            txtModel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtModel.Location = new Point(150, 136);
            txtModel.Name = "txtModel";
            txtModel.Size = new Size(450, 23);
            txtModel.TextChanged += GenerateCommandOnChanged;

            // btnBrowseModel
            btnBrowseModel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowseModel.Location = new Point(610, 134);
            btnBrowseModel.Name = "btnBrowseModel";
            btnBrowseModel.Size = new Size(94, 28);
            btnBrowseModel.Text = "저장 위치";
            btnBrowseModel.UseVisualStyleBackColor = true;
            btnBrowseModel.Click += btnBrowseModel_Click;

            // lblExtraArgs
            lblExtraArgs.Location = new Point(14, 174);
            lblExtraArgs.Name = "lblExtraArgs";
            lblExtraArgs.Size = new Size(130, 22);
            lblExtraArgs.Text = "추가/보정 인자";

            // txtExtraArgs
            txtExtraArgs.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtExtraArgs.Location = new Point(150, 172);
            txtExtraArgs.Name = "txtExtraArgs";
            txtExtraArgs.Size = new Size(450, 23);
            txtExtraArgs.TextChanged += GenerateCommandOnChanged;

            // lblExtraExample
            lblExtraExample.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblExtraExample.Location = new Point(610, 174);
            lblExtraExample.Name = "lblExtraExample";
            lblExtraExample.Size = new Size(120, 22);
            lblExtraExample.Text = "예: --type linear";

            // groupRemote
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
            groupRemote.TabStop = false;
            groupRemote.Text = "WSL / VirtualBox 옵션";

            // lblActivate
            lblActivate.Location = new Point(10, 28);
            lblActivate.Name = "lblActivate";
            lblActivate.Size = new Size(85, 22);
            lblActivate.Text = "환경 활성화";

            // txtActivate
            txtActivate.Location = new Point(96, 25);
            txtActivate.Name = "txtActivate";
            txtActivate.Size = new Size(245, 23);
            txtActivate.TextChanged += GenerateCommandOnChanged;

            // lblRemoteWork
            lblRemoteWork.Location = new Point(356, 28);
            lblRemoteWork.Name = "lblRemoteWork";
            lblRemoteWork.Size = new Size(80, 22);
            lblRemoteWork.Text = "원격 mycar";

            // txtRemoteWork
            txtRemoteWork.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtRemoteWork.Location = new Point(436, 25);
            txtRemoteWork.Name = "txtRemoteWork";
            txtRemoteWork.Size = new Size(235, 23);
            txtRemoteWork.TextChanged += GenerateCommandOnChanged;

            // lblSshUser
            lblSshUser.Location = new Point(10, 62);
            lblSshUser.Name = "lblSshUser";
            lblSshUser.Size = new Size(70, 22);
            lblSshUser.Text = "SSH user";

            // txtSshUser
            txtSshUser.Location = new Point(80, 59);
            txtSshUser.Name = "txtSshUser";
            txtSshUser.Size = new Size(100, 23);
            txtSshUser.TextChanged += GenerateCommandOnChanged;

            // lblSshHost
            lblSshHost.Location = new Point(190, 62);
            lblSshHost.Name = "lblSshHost";
            lblSshHost.Size = new Size(40, 22);
            lblSshHost.Text = "host";

            // txtSshHost
            txtSshHost.Location = new Point(230, 59);
            txtSshHost.Name = "txtSshHost";
            txtSshHost.Size = new Size(100, 23);
            txtSshHost.TextChanged += GenerateCommandOnChanged;

            // lblSshPort
            lblSshPort.Location = new Point(340, 62);
            lblSshPort.Name = "lblSshPort";
            lblSshPort.Size = new Size(40, 22);
            lblSshPort.Text = "port";

            // txtSshPort
            txtSshPort.Location = new Point(380, 59);
            txtSshPort.Name = "txtSshPort";
            txtSshPort.Size = new Size(65, 23);
            txtSshPort.TextChanged += GenerateCommandOnChanged;

            // chkPasswordInput
            chkPasswordInput.Location = new Point(464, 58);
            chkPasswordInput.Name = "chkPasswordInput";
            chkPasswordInput.Size = new Size(210, 24);
            chkPasswordInput.Text = "비밀번호 입력형 콘솔 사용";
            chkPasswordInput.CheckedChanged += GenerateCommandOnChanged;

            // btnUseLoaded
            btnUseLoaded.Location = new Point(14, 423);
            btnUseLoaded.Name = "btnUseLoaded";
            btnUseLoaded.Size = new Size(130, 30);
            btnUseLoaded.Text = "현재 로드 경로";
            btnUseLoaded.UseVisualStyleBackColor = true;
            btnUseLoaded.Click += btnUseLoaded_Click;

            // btnWsl
            btnWsl.Location = new Point(154, 423);
            btnWsl.Name = "btnWsl";
            btnWsl.Size = new Size(130, 30);
            btnWsl.Text = "WSL 경로 변환";
            btnWsl.UseVisualStyleBackColor = true;
            btnWsl.Click += btnWsl_Click;

            // btnVBox
            btnVBox.Location = new Point(294, 423);
            btnVBox.Name = "btnVBox";
            btnVBox.Size = new Size(160, 30);
            btnVBox.Text = "VirtualBox 경로 변환";
            btnVBox.UseVisualStyleBackColor = true;
            btnVBox.Click += btnVBox_Click;

            // btnExport
            btnExport.Location = new Point(464, 423);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(140, 30);
            btnExport.Text = "학습 데이터 생성";
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Click += btnExport_Click;

            // btnGenerate
            btnGenerate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnGenerate.Location = new Point(614, 423);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new Size(120, 30);
            btnGenerate.Text = "명령 생성";
            btnGenerate.UseVisualStyleBackColor = true;
            btnGenerate.Click += btnGenerate_Click;

            // lblCommand
            lblCommand.Location = new Point(14, 466);
            lblCommand.Name = "lblCommand";
            lblCommand.Size = new Size(100, 22);
            lblCommand.Text = "학습 명령";

            // chkManualEdit
            chkManualEdit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            chkManualEdit.Location = new Point(630, 464);
            chkManualEdit.Name = "chkManualEdit";
            chkManualEdit.Size = new Size(110, 24);
            chkManualEdit.Text = "수동 편집";
            chkManualEdit.CheckedChanged += chkManualEdit_CheckedChanged;

            // txtCommand
            txtCommand.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCommand.Location = new Point(14, 492);
            txtCommand.Multiline = true;
            txtCommand.Name = "txtCommand";
            txtCommand.ReadOnly = true;
            txtCommand.ScrollBars = ScrollBars.Vertical;
            txtCommand.Size = new Size(720, 86);

            // btnRun
            btnRun.Font = new Font("맑은 고딕", 10F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnRun.Location = new Point(14, 590);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(160, 38);
            btnRun.Text = "학습 실행";
            btnRun.UseVisualStyleBackColor = true;
            btnRun.Click += btnRun_Click;

            // btnClose
            btnClose.Location = new Point(184, 590);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(90, 38);
            btnClose.Text = "닫기";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;

            // groupLog
            groupLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            groupLog.Controls.Add(txtLog);
            groupLog.Location = new Point(760, 170);
            groupLog.Name = "groupLog";
            groupLog.Size = new Size(290, 540);
            groupLog.TabStop = false;
            groupLog.Text = "실행 로그 / 안내";

            // txtLog
            txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtLog.Location = new Point(10, 22);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(270, 505);

            // Form2
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1064, 721);
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
            Controls.Add(btnRun);
            Controls.Add(btnClose);
            Controls.Add(groupLog);
            Font = new Font("맑은 고딕", 9F, FontStyle.Regular, GraphicsUnit.Point, 129);
            MinimumSize = new Size(980, 700);
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
            ResumeLayout(false);
        }
    }
}
