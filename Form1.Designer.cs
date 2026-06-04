namespace TeamApp
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer? components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                picFrame?.Image?.Dispose();
                picGraph?.Image?.Dispose();
                autoPlayTimer?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnOpenFolder = new Button();
            txtSelectedFolder = new TextBox();
            btnOpenDataFolder = new Button();
            grpList = new GroupBox();
            btnCheckAllFrames = new Button();
            btnClearCheckedFrames = new Button();
            lstFrames = new ColoredCheckedListBox();
            lblStats = new Label();
            grpPreview = new GroupBox();
            picFrame = new PictureBox();
            grpImageEdit = new GroupBox();
            lblEditHint = new Label();
            cmbMaskMode = new ComboBox();
            btnMaskRegion = new Button();
            btnReplaceRegion = new Button();
            btnClearSelection = new Button();
            btnRestoreImage = new Button();
            grpDeleteOps = new GroupBox();
            btnDelete = new Button();
            btnUndo = new Button();
            pnlTimeline = new Panel();
            trbFrame = new TrackBar();
            btnPrev = new Button();
            btnPlay = new Button();
            lblPlaySpeed = new Label();
            trbPlaySpeed = new TrackBar();
            btnNext = new Button();
            btnSave = new Button();
            lblCurrentIndex = new Label();
            lblCurrentImage = new Label();
            lblCurrentMode = new Label();
            lblAngle = new Label();
            txtAngle = new TextBox();
            lblThrottle = new Label();
            txtThrottle = new TextBox();
            lblGraph = new Label();
            picGraph = new PictureBox();
            grpFilter = new GroupBox();
            chkThrottlePositive = new CheckBox();
            chkExcludeAngleZero = new CheckBox();
            chkAngleRange = new CheckBox();
            chkThrottleRange = new CheckBox();
            chkAnomalyOnly = new CheckBox();
            chkDeletedOnly = new CheckBox();
            chkEditedOnly = new CheckBox();
            lblRangeMin = new Label();
            lblRangeMax = new Label();
            numAngleMin = new NumericUpDown();
            numAngleMax = new NumericUpDown();
            numThrottleMin = new NumericUpDown();
            numThrottleMax = new NumericUpDown();
            btnApplyFilter = new Button();
            btnClearFilter = new Button();
            grpAnomaly = new GroupBox();
            lblAnomalyWindow = new Label();
            numAnomalyWindow = new NumericUpDown();
            lblAnomalySigma = new Label();
            numAnomalySigma = new NumericUpDown();
            btnAnalyzeAnomaly = new Button();
            btnClearAnomaly = new Button();
            btnNextAnomaly = new Button();
            lblAnomalyStatus = new Label();
            lblAnomalyHint = new Label();
            grpTrain = new GroupBox();
            lblCommand = new Label();
            chkManualCommandEdit = new CheckBox();
            txtTrainCommand = new TextBox();
            btnTrainingPaths = new Button();
            btnTrain = new Button();
            lblHint = new Label();
            btnCheckDonkey = new Button();
            grpLog = new GroupBox();
            txtLog = new TextBox();
            btnCanny = new Button();
            grpList.SuspendLayout();
            grpPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picFrame).BeginInit();
            grpImageEdit.SuspendLayout();
            grpDeleteOps.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trbFrame).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trbPlaySpeed).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picGraph).BeginInit();
            grpFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numAngleMin).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numAngleMax).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numThrottleMin).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numThrottleMax).BeginInit();
            grpAnomaly.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numAnomalyWindow).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numAnomalySigma).BeginInit();
            grpTrain.SuspendLayout();
            grpLog.SuspendLayout();
            SuspendLayout();
            // 
            // btnOpenFolder
            // 
            btnOpenFolder.Location = new Point(12, 12);
            btnOpenFolder.Name = "btnOpenFolder";
            btnOpenFolder.Size = new Size(96, 30);
            btnOpenFolder.TabIndex = 0;
            btnOpenFolder.Text = "폴더 열기";
            btnOpenFolder.UseVisualStyleBackColor = true;
            btnOpenFolder.Click += btnOpenFolder_Click;
            // 
            // txtSelectedFolder
            // 
            txtSelectedFolder.Location = new Point(116, 15);
            txtSelectedFolder.Name = "txtSelectedFolder";
            txtSelectedFolder.ReadOnly = true;
            txtSelectedFolder.Size = new Size(770, 23);
            txtSelectedFolder.TabIndex = 1;
            txtSelectedFolder.Text = "mycar 폴더, data 폴더, 또는 tub 폴더를 선택하세요.";
            // 
            // btnOpenDataFolder
            // 
            btnOpenDataFolder.Location = new Point(894, 12);
            btnOpenDataFolder.Name = "btnOpenDataFolder";
            btnOpenDataFolder.Size = new Size(210, 30);
            btnOpenDataFolder.TabIndex = 2;
            btnOpenDataFolder.Text = "선택한 폴더 경로 확인하기";
            btnOpenDataFolder.UseVisualStyleBackColor = true;
            btnOpenDataFolder.Click += btnOpenDataFolder_Click;
            // 
            // grpList
            // 
            grpList.Controls.Add(btnCheckAllFrames);
            grpList.Controls.Add(btnClearCheckedFrames);
            grpList.Controls.Add(lstFrames);
            grpList.Controls.Add(lblStats);
            grpList.Location = new Point(12, 52);
            grpList.Name = "grpList";
            grpList.Size = new Size(270, 760);
            grpList.TabIndex = 3;
            grpList.TabStop = false;
            grpList.Text = "프레임 목록";
            // 
            // btnCheckAllFrames
            // 
            btnCheckAllFrames.Location = new Point(10, 24);
            btnCheckAllFrames.Name = "btnCheckAllFrames";
            btnCheckAllFrames.Size = new Size(120, 28);
            btnCheckAllFrames.TabIndex = 1;
            btnCheckAllFrames.Text = "전체 선택";
            btnCheckAllFrames.UseVisualStyleBackColor = true;
            btnCheckAllFrames.Click += btnCheckAllFrames_Click;
            // 
            // btnClearCheckedFrames
            // 
            btnClearCheckedFrames.Location = new Point(140, 24);
            btnClearCheckedFrames.Name = "btnClearCheckedFrames";
            btnClearCheckedFrames.Size = new Size(120, 28);
            btnClearCheckedFrames.TabIndex = 2;
            btnClearCheckedFrames.Text = "전체 해제";
            btnClearCheckedFrames.UseVisualStyleBackColor = true;
            btnClearCheckedFrames.Click += btnClearCheckedFrames_Click;
            // 
            // lstFrames
            // 
            lstFrames.CheckOnClick = true;
            lstFrames.FormattingEnabled = true;
            lstFrames.HorizontalScrollbar = true;
            lstFrames.IntegralHeight = false;
            lstFrames.Location = new Point(10, 58);
            lstFrames.Name = "lstFrames";
            lstFrames.ResolveVisualState = null;
            lstFrames.ScrollAlwaysVisible = true;
            lstFrames.Size = new Size(250, 615);
            lstFrames.TabIndex = 0;
            lstFrames.SelectedIndexChanged += lstFrames_SelectedIndexChanged;
            // 
            // lblStats
            // 
            lblStats.Location = new Point(10, 680);
            lblStats.Name = "lblStats";
            lblStats.Size = new Size(250, 70);
            lblStats.TabIndex = 1;
            lblStats.Text = "데이터를 불러오면 통계가 표시됩니다.";
            // 
            // grpPreview
            // 
            grpPreview.Controls.Add(picFrame);
            grpPreview.Controls.Add(grpImageEdit);
            grpPreview.Controls.Add(grpDeleteOps);
            grpPreview.Controls.Add(pnlTimeline);
            grpPreview.Controls.Add(trbFrame);
            grpPreview.Controls.Add(btnPrev);
            grpPreview.Controls.Add(btnPlay);
            grpPreview.Controls.Add(lblPlaySpeed);
            grpPreview.Controls.Add(trbPlaySpeed);
            grpPreview.Controls.Add(btnNext);
            grpPreview.Controls.Add(btnSave);
            grpPreview.Controls.Add(lblCurrentIndex);
            grpPreview.Controls.Add(lblCurrentImage);
            grpPreview.Controls.Add(lblCurrentMode);
            grpPreview.Controls.Add(lblAngle);
            grpPreview.Controls.Add(txtAngle);
            grpPreview.Controls.Add(lblThrottle);
            grpPreview.Controls.Add(txtThrottle);
            grpPreview.Controls.Add(lblGraph);
            grpPreview.Controls.Add(picGraph);
            grpPreview.Location = new Point(292, 52);
            grpPreview.Name = "grpPreview";
            grpPreview.Size = new Size(530, 760);
            grpPreview.TabIndex = 4;
            grpPreview.TabStop = false;
            grpPreview.Text = "이미지 / 프레임 탐색 · 편집";
            // 
            // picFrame
            // 
            picFrame.BackColor = Color.Black;
            picFrame.BorderStyle = BorderStyle.FixedSingle;
            picFrame.Location = new Point(12, 22);
            picFrame.Name = "picFrame";
            picFrame.Size = new Size(506, 268);
            picFrame.SizeMode = PictureBoxSizeMode.Zoom;
            picFrame.TabIndex = 0;
            picFrame.TabStop = false;
            picFrame.Paint += picFrame_Paint;
            picFrame.MouseDown += picFrame_MouseDown;
            picFrame.MouseMove += picFrame_MouseMove;
            picFrame.MouseUp += picFrame_MouseUp;
            // 
            // grpImageEdit
            // 
            grpImageEdit.Controls.Add(btnCanny);
            grpImageEdit.Controls.Add(lblEditHint);
            grpImageEdit.Controls.Add(cmbMaskMode);
            grpImageEdit.Controls.Add(btnMaskRegion);
            grpImageEdit.Controls.Add(btnReplaceRegion);
            grpImageEdit.Controls.Add(btnClearSelection);
            grpImageEdit.Controls.Add(btnRestoreImage);
            grpImageEdit.Location = new Point(12, 298);
            grpImageEdit.Name = "grpImageEdit";
            grpImageEdit.Size = new Size(506, 78);
            grpImageEdit.TabIndex = 1;
            grpImageEdit.TabStop = false;
            grpImageEdit.Text = "이미지 부분 가리기 / 바꾸기";
            // 
            // lblEditHint
            // 
            lblEditHint.AutoEllipsis = true;
            lblEditHint.Location = new Point(10, 20);
            lblEditHint.Name = "lblEditHint";
            lblEditHint.Size = new Size(490, 18);
            lblEditHint.TabIndex = 0;
            lblEditHint.Text = "이미지 위에서 마우스로 드래그해 영역을 선택한 뒤 편집 버튼을 누르세요.";
            // 
            // cmbMaskMode
            // 
            cmbMaskMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMaskMode.FormattingEnabled = true;
            cmbMaskMode.Items.AddRange(new object[] { "검정", "흰색", "회색", "평균색" });
            cmbMaskMode.Location = new Point(10, 44);
            cmbMaskMode.Name = "cmbMaskMode";
            cmbMaskMode.Size = new Size(80, 23);
            cmbMaskMode.TabIndex = 1;
            // 
            // btnMaskRegion
            // 
            btnMaskRegion.Location = new Point(98, 42);
            btnMaskRegion.Name = "btnMaskRegion";
            btnMaskRegion.Size = new Size(85, 27);
            btnMaskRegion.TabIndex = 2;
            btnMaskRegion.Text = "영역 가리기";
            btnMaskRegion.UseVisualStyleBackColor = true;
            btnMaskRegion.Click += btnMaskRegion_Click;
            // 
            // btnReplaceRegion
            // 
            btnReplaceRegion.Location = new Point(190, 42);
            btnReplaceRegion.Name = "btnReplaceRegion";
            btnReplaceRegion.Size = new Size(95, 27);
            btnReplaceRegion.TabIndex = 3;
            btnReplaceRegion.Text = "이미지로 교체";
            btnReplaceRegion.UseVisualStyleBackColor = true;
            btnReplaceRegion.Click += btnReplaceRegion_Click;
            // 
            // btnClearSelection
            // 
            btnClearSelection.Location = new Point(292, 42);
            btnClearSelection.Name = "btnClearSelection";
            btnClearSelection.Size = new Size(82, 27);
            btnClearSelection.TabIndex = 4;
            btnClearSelection.Text = "선택 해제";
            btnClearSelection.UseVisualStyleBackColor = true;
            btnClearSelection.Click += btnClearSelection_Click;
            // 
            // btnRestoreImage
            // 
            btnRestoreImage.Location = new Point(382, 42);
            btnRestoreImage.Name = "btnRestoreImage";
            btnRestoreImage.Size = new Size(92, 27);
            btnRestoreImage.TabIndex = 5;
            btnRestoreImage.Text = "원본 복구";
            btnRestoreImage.UseVisualStyleBackColor = true;
            btnRestoreImage.Click += btnRestoreImage_Click;
            // 
            // grpDeleteOps
            // 
            grpDeleteOps.Controls.Add(btnDelete);
            grpDeleteOps.Controls.Add(btnUndo);
            grpDeleteOps.Location = new Point(12, 384);
            grpDeleteOps.Name = "grpDeleteOps";
            grpDeleteOps.Size = new Size(506, 58);
            grpDeleteOps.TabIndex = 2;
            grpDeleteOps.TabStop = false;
            grpDeleteOps.Text = "이미지 삭제 / 복구";
            // 
            // btnDelete
            // 
            btnDelete.BackColor = Color.Red;
            btnDelete.ForeColor = Color.White;
            btnDelete.Location = new Point(12, 22);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(180, 28);
            btnDelete.TabIndex = 8;
            btnDelete.Text = "이미지 삭제";
            btnDelete.UseVisualStyleBackColor = false;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnUndo
            // 
            btnUndo.Location = new Point(268, 22);
            btnUndo.Name = "btnUndo";
            btnUndo.Size = new Size(180, 28);
            btnUndo.TabIndex = 7;
            btnUndo.Text = "삭제 이미지 복구";
            btnUndo.UseVisualStyleBackColor = true;
            btnUndo.Click += btnUndo_Click;
            // 
            // pnlTimeline
            // 
            pnlTimeline.BackColor = Color.White;
            pnlTimeline.BorderStyle = BorderStyle.FixedSingle;
            pnlTimeline.Location = new Point(12, 450);
            pnlTimeline.Name = "pnlTimeline";
            pnlTimeline.Size = new Size(506, 18);
            pnlTimeline.TabIndex = 2;
            pnlTimeline.Paint += pnlTimeline_Paint;
            // 
            // trbFrame
            // 
            trbFrame.LargeChange = 1;
            trbFrame.Location = new Point(12, 470);
            trbFrame.Maximum = 0;
            trbFrame.Name = "trbFrame";
            trbFrame.Size = new Size(506, 45);
            trbFrame.TabIndex = 3;
            trbFrame.Scroll += trbFrame_Scroll;
            // 
            // btnPrev
            // 
            btnPrev.Location = new Point(12, 517);
            btnPrev.Name = "btnPrev";
            btnPrev.Size = new Size(122, 30);
            btnPrev.TabIndex = 4;
            btnPrev.Text = "이전";
            btnPrev.UseVisualStyleBackColor = true;
            btnPrev.Click += btnPrev_Click;
            // 
            // btnPlay
            // 
            btnPlay.Location = new Point(142, 517);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(122, 30);
            btnPlay.TabIndex = 5;
            btnPlay.Text = "자동 재생";
            btnPlay.UseVisualStyleBackColor = true;
            btnPlay.Click += btnPlay_Click;
            // 
            // lblPlaySpeed
            // 
            lblPlaySpeed.Location = new Point(142, 550);
            lblPlaySpeed.Name = "lblPlaySpeed";
            lblPlaySpeed.Size = new Size(90, 20);
            lblPlaySpeed.TabIndex = 6;
            lblPlaySpeed.Text = "재생속도 250ms";
            // 
            // trbPlaySpeed
            // 
            trbPlaySpeed.Location = new Point(238, 547);
            trbPlaySpeed.Maximum = 20;
            trbPlaySpeed.Minimum = 1;
            trbPlaySpeed.Name = "trbPlaySpeed";
            trbPlaySpeed.Size = new Size(140, 45);
            trbPlaySpeed.TabIndex = 7;
            trbPlaySpeed.TickFrequency = 2;
            trbPlaySpeed.Value = 4;
            trbPlaySpeed.Scroll += trbPlaySpeed_Scroll;
            // 
            // btnNext
            // 
            btnNext.Location = new Point(272, 517);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(122, 30);
            btnNext.TabIndex = 6;
            btnNext.Text = "다음";
            btnNext.UseVisualStyleBackColor = true;
            btnNext.Click += btnNext_Click;
            // 
            // btnSave
            // 
            btnSave.BackColor = Color.LimeGreen;
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(402, 517);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(110, 30);
            btnSave.TabIndex = 9;
            btnSave.Text = "저장";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += btnSave_Click;
            // 
            // lblCurrentIndex
            // 
            lblCurrentIndex.Location = new Point(12, 555);
            lblCurrentIndex.Name = "lblCurrentIndex";
            lblCurrentIndex.Size = new Size(506, 20);
            lblCurrentIndex.TabIndex = 10;
            lblCurrentIndex.Text = "현재 인덱스: -";
            // 
            // lblCurrentImage
            // 
            lblCurrentImage.AutoEllipsis = true;
            lblCurrentImage.Location = new Point(12, 578);
            lblCurrentImage.Name = "lblCurrentImage";
            lblCurrentImage.Size = new Size(506, 20);
            lblCurrentImage.TabIndex = 11;
            lblCurrentImage.Text = "이미지: -";
            // 
            // lblCurrentMode
            // 
            lblCurrentMode.AutoEllipsis = true;
            lblCurrentMode.Location = new Point(12, 601);
            lblCurrentMode.Name = "lblCurrentMode";
            lblCurrentMode.Size = new Size(506, 20);
            lblCurrentMode.TabIndex = 12;
            lblCurrentMode.Text = "mode/catalog: -";
            // 
            // lblAngle
            // 
            lblAngle.Location = new Point(12, 627);
            lblAngle.Name = "lblAngle";
            lblAngle.Size = new Size(80, 20);
            lblAngle.TabIndex = 13;
            lblAngle.Text = "angle";
            // 
            // txtAngle
            // 
            txtAngle.Location = new Point(95, 624);
            txtAngle.Name = "txtAngle";
            txtAngle.Size = new Size(120, 23);
            txtAngle.TabIndex = 14;
            // 
            // lblThrottle
            // 
            lblThrottle.Location = new Point(235, 703);
            lblThrottle.Name = "lblThrottle";
            lblThrottle.Size = new Size(80, 20);
            lblThrottle.TabIndex = 15;
            lblThrottle.Text = "throttle";
            // 
            // txtThrottle
            // 
            txtThrottle.Location = new Point(315, 624);
            txtThrottle.Name = "txtThrottle";
            txtThrottle.Size = new Size(120, 23);
            txtThrottle.TabIndex = 16;
            // 
            // lblGraph
            // 
            lblGraph.Location = new Point(12, 730);
            lblGraph.Name = "lblGraph";
            lblGraph.Size = new Size(506, 20);
            lblGraph.TabIndex = 17;
            lblGraph.Text = "angle / throttle / 이동평균 / 이상 후보";
            // 
            // picGraph
            // 
            picGraph.BackColor = Color.White;
            picGraph.BorderStyle = BorderStyle.FixedSingle;
            picGraph.Location = new Point(12, 753);
            picGraph.Name = "picGraph";
            picGraph.Size = new Size(506, 65);
            picGraph.TabIndex = 18;
            picGraph.TabStop = false;
            // 
            // grpFilter
            // 
            grpFilter.Controls.Add(chkThrottlePositive);
            grpFilter.Controls.Add(chkExcludeAngleZero);
            grpFilter.Controls.Add(chkAngleRange);
            grpFilter.Controls.Add(chkThrottleRange);
            grpFilter.Controls.Add(chkAnomalyOnly);
            grpFilter.Controls.Add(chkDeletedOnly);
            grpFilter.Controls.Add(chkEditedOnly);
            grpFilter.Controls.Add(lblRangeMin);
            grpFilter.Controls.Add(lblRangeMax);
            grpFilter.Controls.Add(numAngleMin);
            grpFilter.Controls.Add(numAngleMax);
            grpFilter.Controls.Add(numThrottleMin);
            grpFilter.Controls.Add(numThrottleMax);
            grpFilter.Controls.Add(btnApplyFilter);
            grpFilter.Controls.Add(btnClearFilter);
            grpFilter.Location = new Point(832, 52);
            grpFilter.Name = "grpFilter";
            grpFilter.Size = new Size(380, 285);
            grpFilter.TabIndex = 5;
            grpFilter.TabStop = false;
            grpFilter.Text = "데이터 필터링";
            // 
            // chkThrottlePositive
            // 
            chkThrottlePositive.Location = new Point(14, 28);
            chkThrottlePositive.Name = "chkThrottlePositive";
            chkThrottlePositive.Size = new Size(180, 24);
            chkThrottlePositive.TabIndex = 0;
            chkThrottlePositive.Text = "throttle > 0만 표시";
            chkThrottlePositive.UseVisualStyleBackColor = true;
            // 
            // chkExcludeAngleZero
            // 
            chkExcludeAngleZero.Location = new Point(14, 56);
            chkExcludeAngleZero.Name = "chkExcludeAngleZero";
            chkExcludeAngleZero.Size = new Size(110, 24);
            chkExcludeAngleZero.TabIndex = 1;
            chkExcludeAngleZero.Text = "angle=0 제외";
            chkExcludeAngleZero.UseVisualStyleBackColor = true;
            // 
            // chkAngleRange
            // 
            chkAngleRange.Location = new Point(14, 90);
            chkAngleRange.Name = "chkAngleRange";
            chkAngleRange.Size = new Size(100, 24);
            chkAngleRange.TabIndex = 2;
            chkAngleRange.Text = "angle 범위";
            chkAngleRange.UseVisualStyleBackColor = true;
            // 
            // chkThrottleRange
            // 
            chkThrottleRange.Location = new Point(14, 124);
            chkThrottleRange.Name = "chkThrottleRange";
            chkThrottleRange.Size = new Size(110, 24);
            chkThrottleRange.TabIndex = 3;
            chkThrottleRange.Text = "throttle 범위";
            chkThrottleRange.UseVisualStyleBackColor = true;
            // 
            // chkAnomalyOnly
            // 
            chkAnomalyOnly.Location = new Point(14, 156);
            chkAnomalyOnly.Name = "chkAnomalyOnly";
            chkAnomalyOnly.Size = new Size(170, 24);
            chkAnomalyOnly.TabIndex = 4;
            chkAnomalyOnly.Text = "이상 후보만 표시";
            chkAnomalyOnly.UseVisualStyleBackColor = true;
            // 
            // chkDeletedOnly
            // 
            chkDeletedOnly.Location = new Point(14, 186);
            chkDeletedOnly.Name = "chkDeletedOnly";
            chkDeletedOnly.Size = new Size(150, 24);
            chkDeletedOnly.TabIndex = 13;
            chkDeletedOnly.Text = "삭제 이미지만";
            chkDeletedOnly.UseVisualStyleBackColor = true;
            // 
            // chkEditedOnly
            // 
            chkEditedOnly.Location = new Point(14, 216);
            chkEditedOnly.Name = "chkEditedOnly";
            chkEditedOnly.Size = new Size(150, 24);
            chkEditedOnly.TabIndex = 14;
            chkEditedOnly.Text = "교체/편집만";
            chkEditedOnly.UseVisualStyleBackColor = true;
            // 
            // lblRangeMin
            // 
            lblRangeMin.Location = new Point(130, 72);
            lblRangeMin.Name = "lblRangeMin";
            lblRangeMin.Size = new Size(50, 18);
            lblRangeMin.TabIndex = 5;
            lblRangeMin.Text = "최소";
            // 
            // lblRangeMax
            // 
            lblRangeMax.Location = new Point(250, 72);
            lblRangeMax.Name = "lblRangeMax";
            lblRangeMax.Size = new Size(50, 18);
            lblRangeMax.TabIndex = 6;
            lblRangeMax.Text = "최대";
            // 
            // numAngleMin
            // 
            numAngleMin.DecimalPlaces = 3;
            numAngleMin.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numAngleMin.Location = new Point(130, 90);
            numAngleMin.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numAngleMin.Minimum = new decimal(new int[] { 10, 0, 0, int.MinValue });
            numAngleMin.Name = "numAngleMin";
            numAngleMin.Size = new Size(100, 23);
            numAngleMin.TabIndex = 7;
            numAngleMin.Value = new decimal(new int[] { 1, 0, 0, int.MinValue });
            // 
            // numAngleMax
            // 
            numAngleMax.DecimalPlaces = 3;
            numAngleMax.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numAngleMax.Location = new Point(250, 90);
            numAngleMax.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numAngleMax.Minimum = new decimal(new int[] { 10, 0, 0, int.MinValue });
            numAngleMax.Name = "numAngleMax";
            numAngleMax.Size = new Size(100, 23);
            numAngleMax.TabIndex = 8;
            numAngleMax.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // numThrottleMin
            // 
            numThrottleMin.DecimalPlaces = 3;
            numThrottleMin.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numThrottleMin.Location = new Point(130, 124);
            numThrottleMin.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numThrottleMin.Minimum = new decimal(new int[] { 10, 0, 0, int.MinValue });
            numThrottleMin.Name = "numThrottleMin";
            numThrottleMin.Size = new Size(100, 23);
            numThrottleMin.TabIndex = 9;
            // 
            // numThrottleMax
            // 
            numThrottleMax.DecimalPlaces = 3;
            numThrottleMax.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numThrottleMax.Location = new Point(250, 124);
            numThrottleMax.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numThrottleMax.Minimum = new decimal(new int[] { 10, 0, 0, int.MinValue });
            numThrottleMax.Name = "numThrottleMax";
            numThrottleMax.Size = new Size(100, 23);
            numThrottleMax.TabIndex = 10;
            numThrottleMax.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // btnApplyFilter
            // 
            btnApplyFilter.Location = new Point(14, 245);
            btnApplyFilter.Name = "btnApplyFilter";
            btnApplyFilter.Size = new Size(110, 30);
            btnApplyFilter.TabIndex = 11;
            btnApplyFilter.Text = "필터 적용";
            btnApplyFilter.UseVisualStyleBackColor = true;
            btnApplyFilter.Click += btnApplyFilter_Click;
            // 
            // btnClearFilter
            // 
            btnClearFilter.Location = new Point(132, 245);
            btnClearFilter.Name = "btnClearFilter";
            btnClearFilter.Size = new Size(120, 30);
            btnClearFilter.TabIndex = 12;
            btnClearFilter.Text = "필터 초기화";
            btnClearFilter.UseVisualStyleBackColor = true;
            btnClearFilter.Click += btnClearFilter_Click;
            // 
            // grpAnomaly
            // 
            grpAnomaly.Controls.Add(lblAnomalyWindow);
            grpAnomaly.Controls.Add(numAnomalyWindow);
            grpAnomaly.Controls.Add(lblAnomalySigma);
            grpAnomaly.Controls.Add(numAnomalySigma);
            grpAnomaly.Controls.Add(btnAnalyzeAnomaly);
            grpAnomaly.Controls.Add(btnClearAnomaly);
            grpAnomaly.Controls.Add(btnNextAnomaly);
            grpAnomaly.Controls.Add(lblAnomalyStatus);
            grpAnomaly.Controls.Add(lblAnomalyHint);
            grpAnomaly.Location = new Point(832, 342);
            grpAnomaly.Name = "grpAnomaly";
            grpAnomaly.Size = new Size(380, 188);
            grpAnomaly.TabIndex = 6;
            grpAnomaly.TabStop = false;
            grpAnomaly.Text = "이상 주행 자동 탐지";
            // 
            // lblAnomalyWindow
            // 
            lblAnomalyWindow.Location = new Point(14, 28);
            lblAnomalyWindow.Name = "lblAnomalyWindow";
            lblAnomalyWindow.Size = new Size(120, 20);
            lblAnomalyWindow.TabIndex = 0;
            lblAnomalyWindow.Text = "이동평균 window";
            // 
            // numAnomalyWindow
            // 
            numAnomalyWindow.Location = new Point(140, 26);
            numAnomalyWindow.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            numAnomalyWindow.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            numAnomalyWindow.Name = "numAnomalyWindow";
            numAnomalyWindow.Size = new Size(80, 23);
            numAnomalyWindow.TabIndex = 1;
            numAnomalyWindow.Value = new decimal(new int[] { 15, 0, 0, 0 });
            // 
            // lblAnomalySigma
            // 
            lblAnomalySigma.Location = new Point(230, 28);
            lblAnomalySigma.Name = "lblAnomalySigma";
            lblAnomalySigma.Size = new Size(60, 20);
            lblAnomalySigma.TabIndex = 2;
            lblAnomalySigma.Text = "σ 배수";
            // 
            // numAnomalySigma
            // 
            numAnomalySigma.DecimalPlaces = 1;
            numAnomalySigma.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numAnomalySigma.Location = new Point(292, 26);
            numAnomalySigma.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numAnomalySigma.Minimum = new decimal(new int[] { 5, 0, 0, 65536 });
            numAnomalySigma.Name = "numAnomalySigma";
            numAnomalySigma.Size = new Size(65, 23);
            numAnomalySigma.TabIndex = 3;
            numAnomalySigma.Value = new decimal(new int[] { 25, 0, 0, 65536 });
            // 
            // btnAnalyzeAnomaly
            // 
            btnAnalyzeAnomaly.Location = new Point(14, 62);
            btnAnalyzeAnomaly.Name = "btnAnalyzeAnomaly";
            btnAnalyzeAnomaly.Size = new Size(120, 30);
            btnAnalyzeAnomaly.TabIndex = 4;
            btnAnalyzeAnomaly.Text = "이상 탐지 실행";
            btnAnalyzeAnomaly.UseVisualStyleBackColor = true;
            btnAnalyzeAnomaly.Click += btnAnalyzeAnomaly_Click;
            // 
            // btnClearAnomaly
            // 
            btnClearAnomaly.Location = new Point(144, 62);
            btnClearAnomaly.Name = "btnClearAnomaly";
            btnClearAnomaly.Size = new Size(110, 30);
            btnClearAnomaly.TabIndex = 5;
            btnClearAnomaly.Text = "탐지 초기화";
            btnClearAnomaly.UseVisualStyleBackColor = true;
            btnClearAnomaly.Click += btnClearAnomaly_Click;
            // 
            // btnNextAnomaly
            // 
            btnNextAnomaly.Location = new Point(264, 62);
            btnNextAnomaly.Name = "btnNextAnomaly";
            btnNextAnomaly.Size = new Size(92, 30);
            btnNextAnomaly.TabIndex = 6;
            btnNextAnomaly.Text = "다음 이상";
            btnNextAnomaly.UseVisualStyleBackColor = true;
            btnNextAnomaly.Click += btnNextAnomaly_Click;
            // 
            // lblAnomalyStatus
            // 
            lblAnomalyStatus.Location = new Point(14, 104);
            lblAnomalyStatus.Name = "lblAnomalyStatus";
            lblAnomalyStatus.Size = new Size(350, 22);
            lblAnomalyStatus.TabIndex = 6;
            lblAnomalyStatus.Text = "이상 탐지 전";
            // 
            // lblAnomalyHint
            // 
            lblAnomalyHint.Location = new Point(14, 130);
            lblAnomalyHint.Name = "lblAnomalyHint";
            lblAnomalyHint.Size = new Size(350, 52);
            lblAnomalyHint.TabIndex = 7;
            lblAnomalyHint.Text = "조향각 이동평균·변동성 밴드를 벗어난 프레임을 목록/그래프/타임라인에 빨간색으로 표시합니다.";
            // 
            // grpTrain
            // 
            grpTrain.Controls.Add(lblCommand);
            grpTrain.Controls.Add(chkManualCommandEdit);
            grpTrain.Controls.Add(txtTrainCommand);
            grpTrain.Controls.Add(btnTrainingPaths);
            grpTrain.Controls.Add(btnTrain);
            grpTrain.Controls.Add(lblHint);
            grpTrain.Location = new Point(832, 552);
            grpTrain.Name = "grpTrain";
            grpTrain.Size = new Size(380, 210);
            grpTrain.TabIndex = 7;
            grpTrain.TabStop = false;
            grpTrain.Text = "AI 학습 실행(C# -> Python/DonkeyCar)";
            // 
            // lblCommand
            // 
            lblCommand.Location = new Point(14, 25);
            lblCommand.Name = "lblCommand";
            lblCommand.Size = new Size(100, 20);
            lblCommand.TabIndex = 0;
            lblCommand.Text = "학습 명령";
            // 
            // chkManualCommandEdit
            // 
            chkManualCommandEdit.Location = new Point(250, 23);
            chkManualCommandEdit.Name = "chkManualCommandEdit";
            chkManualCommandEdit.Size = new Size(115, 24);
            chkManualCommandEdit.TabIndex = 1;
            chkManualCommandEdit.Text = "수동 편집";
            chkManualCommandEdit.UseVisualStyleBackColor = true;
            chkManualCommandEdit.CheckedChanged += chkManualCommandEdit_CheckedChanged;
            // 
            // txtTrainCommand
            // 
            txtTrainCommand.Location = new Point(14, 48);
            txtTrainCommand.Multiline = true;
            txtTrainCommand.Name = "txtTrainCommand";
            txtTrainCommand.ReadOnly = true;
            txtTrainCommand.ScrollBars = ScrollBars.Vertical;
            txtTrainCommand.Size = new Size(350, 58);
            txtTrainCommand.TabIndex = 1;
            txtTrainCommand.Text = "먼저 [학습 명령 생성] 버튼으로 실행 환경과 경로를 선택하세요.";
            // 
            // btnTrainingPaths
            // 
            btnTrainingPaths.Location = new Point(14, 114);
            btnTrainingPaths.Name = "btnTrainingPaths";
            btnTrainingPaths.Size = new Size(132, 30);
            btnTrainingPaths.TabIndex = 4;
            btnTrainingPaths.Text = "학습 명령 생성";
            btnTrainingPaths.UseVisualStyleBackColor = true;
            btnTrainingPaths.Click += btnTrainingPaths_Click;
            // 
            // btnTrain
            // 
            btnTrain.Location = new Point(154, 114);
            btnTrain.Name = "btnTrain";
            btnTrain.Size = new Size(104, 30);
            btnTrain.TabIndex = 2;
            btnTrain.Text = "학습 실행";
            btnTrain.UseVisualStyleBackColor = true;
            btnTrain.Click += btnTrain_Click;
            // 
            // lblHint
            // 
            lblHint.Location = new Point(14, 150);
            lblHint.Name = "lblHint";
            lblHint.Size = new Size(350, 44);
            lblHint.TabIndex = 5;
            lblHint.Text = "먼저 [학습 명령 생성]을 눌러 환경/경로를 확정하세요. 수동 편집은 체크 후 가능합니다.";
            // 
            // btnCheckDonkey
            // 
            btnCheckDonkey.Location = new Point(0, 0);
            btnCheckDonkey.Name = "btnCheckDonkey";
            btnCheckDonkey.Size = new Size(1, 1);
            btnCheckDonkey.TabIndex = 3;
            btnCheckDonkey.UseVisualStyleBackColor = true;
            btnCheckDonkey.Visible = false;
            btnCheckDonkey.Click += btnCheckDonkey_Click;
            // 
            // grpLog
            // 
            grpLog.Controls.Add(txtLog);
            grpLog.Location = new Point(832, 752);
            grpLog.Name = "grpLog";
            grpLog.Size = new Size(380, 127);
            grpLog.TabIndex = 8;
            grpLog.TabStop = false;
            grpLog.Text = "실행 로그";
            // 
            // txtLog
            // 
            txtLog.Location = new Point(10, 22);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(360, 115);
            txtLog.TabIndex = 0;
            // 
            // btnCanny
            // 
            btnCanny.Location = new Point(415, 15);
            btnCanny.Name = "btnCanny";
            btnCanny.Size = new Size(75, 23);
            btnCanny.TabIndex = 6;
            btnCanny.Text = "캐니에지";
            btnCanny.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            BackColor = Color.FromArgb(248, 248, 248);
            ClientSize = new Size(1224, 821);
            Controls.Add(btnOpenFolder);
            Controls.Add(txtSelectedFolder);
            Controls.Add(btnOpenDataFolder);
            Controls.Add(grpList);
            Controls.Add(grpPreview);
            Controls.Add(grpFilter);
            Controls.Add(grpAnomaly);
            Controls.Add(grpTrain);
            Controls.Add(grpLog);
            Font = new Font("맑은 고딕", 9F, FontStyle.Regular, GraphicsUnit.Point, 129);
            MinimumSize = new Size(980, 650);
            Name = "Form1";
            Text = "DonkeyCar UI 데이터 관리 도구";
            grpList.ResumeLayout(false);
            grpPreview.ResumeLayout(false);
            grpPreview.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picFrame).EndInit();
            grpImageEdit.ResumeLayout(false);
            grpDeleteOps.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)trbFrame).EndInit();
            ((System.ComponentModel.ISupportInitialize)trbPlaySpeed).EndInit();
            ((System.ComponentModel.ISupportInitialize)picGraph).EndInit();
            grpFilter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numAngleMin).EndInit();
            ((System.ComponentModel.ISupportInitialize)numAngleMax).EndInit();
            ((System.ComponentModel.ISupportInitialize)numThrottleMin).EndInit();
            ((System.ComponentModel.ISupportInitialize)numThrottleMax).EndInit();
            grpAnomaly.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numAnomalyWindow).EndInit();
            ((System.ComponentModel.ISupportInitialize)numAnomalySigma).EndInit();
            grpTrain.ResumeLayout(false);
            grpTrain.PerformLayout();
            grpLog.ResumeLayout(false);
            grpLog.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnOpenFolder = null!;
        private System.Windows.Forms.Button btnOpenDataFolder = null!;
        private System.Windows.Forms.TextBox txtSelectedFolder = null!;
        private System.Windows.Forms.GroupBox grpList = null!;
        private TeamApp.ColoredCheckedListBox lstFrames = null!;
        private System.Windows.Forms.Button btnCheckAllFrames = null!;
        private System.Windows.Forms.Button btnClearCheckedFrames = null!;
        private System.Windows.Forms.Label lblStats = null!;
        private System.Windows.Forms.GroupBox grpPreview = null!;
        private System.Windows.Forms.PictureBox picFrame = null!;
        private System.Windows.Forms.GroupBox grpImageEdit = null!;
        private System.Windows.Forms.Label lblEditHint = null!;
        private System.Windows.Forms.ComboBox cmbMaskMode = null!;
        private System.Windows.Forms.Button btnMaskRegion = null!;
        private System.Windows.Forms.Button btnReplaceRegion = null!;
        private System.Windows.Forms.Button btnClearSelection = null!;
        private System.Windows.Forms.Button btnRestoreImage = null!;
        private System.Windows.Forms.GroupBox grpDeleteOps = null!;
        private System.Windows.Forms.Panel pnlTimeline = null!;
        private System.Windows.Forms.TrackBar trbFrame = null!;
        private System.Windows.Forms.Button btnPrev = null!;
        private System.Windows.Forms.Button btnNext = null!;
        private System.Windows.Forms.Button btnPlay = null!;
        private System.Windows.Forms.Label lblPlaySpeed = null!;
        private System.Windows.Forms.TrackBar trbPlaySpeed = null!;
        private System.Windows.Forms.Button btnSave = null!;
        private System.Windows.Forms.Button btnDelete = null!;
        private System.Windows.Forms.Button btnUndo = null!;
        private System.Windows.Forms.TextBox txtAngle = null!;
        private System.Windows.Forms.TextBox txtThrottle = null!;
        private System.Windows.Forms.Label lblCurrentIndex = null!;
        private System.Windows.Forms.Label lblCurrentImage = null!;
        private System.Windows.Forms.Label lblCurrentMode = null!;
        private System.Windows.Forms.Label lblAngle = null!;
        private System.Windows.Forms.Label lblThrottle = null!;
        private System.Windows.Forms.Label lblGraph = null!;
        private System.Windows.Forms.PictureBox picGraph = null!;
        private System.Windows.Forms.GroupBox grpFilter = null!;
        private System.Windows.Forms.CheckBox chkThrottlePositive = null!;
        private System.Windows.Forms.CheckBox chkExcludeAngleZero = null!;
        private System.Windows.Forms.CheckBox chkAngleRange = null!;
        private System.Windows.Forms.CheckBox chkThrottleRange = null!;
        private System.Windows.Forms.CheckBox chkAnomalyOnly = null!;
        private System.Windows.Forms.CheckBox chkDeletedOnly = null!;
        private System.Windows.Forms.CheckBox chkEditedOnly = null!;
        private System.Windows.Forms.Label lblRangeMin = null!;
        private System.Windows.Forms.Label lblRangeMax = null!;
        private System.Windows.Forms.NumericUpDown numAngleMin = null!;
        private System.Windows.Forms.NumericUpDown numAngleMax = null!;
        private System.Windows.Forms.NumericUpDown numThrottleMin = null!;
        private System.Windows.Forms.NumericUpDown numThrottleMax = null!;
        private System.Windows.Forms.Button btnApplyFilter = null!;
        private System.Windows.Forms.Button btnClearFilter = null!;
        private System.Windows.Forms.GroupBox grpAnomaly = null!;
        private System.Windows.Forms.Label lblAnomalyWindow = null!;
        private System.Windows.Forms.NumericUpDown numAnomalyWindow = null!;
        private System.Windows.Forms.Label lblAnomalySigma = null!;
        private System.Windows.Forms.NumericUpDown numAnomalySigma = null!;
        private System.Windows.Forms.Button btnAnalyzeAnomaly = null!;
        private System.Windows.Forms.Button btnClearAnomaly = null!;
        private System.Windows.Forms.Button btnNextAnomaly = null!;
        private System.Windows.Forms.Label lblAnomalyStatus = null!;
        private System.Windows.Forms.Label lblAnomalyHint = null!;
        private System.Windows.Forms.GroupBox grpTrain = null!;
        private System.Windows.Forms.Label lblCommand = null!;
        private System.Windows.Forms.TextBox txtTrainCommand = null!;
        private System.Windows.Forms.Button btnTrain = null!;
        private System.Windows.Forms.Button btnCheckDonkey = null!;
        private System.Windows.Forms.Button btnTrainingPaths = null!;
        private System.Windows.Forms.CheckBox chkManualCommandEdit = null!;
        private System.Windows.Forms.Label lblHint = null!;
        private System.Windows.Forms.GroupBox grpLog = null!;
        private System.Windows.Forms.TextBox txtLog = null!;
        private Button btnCanny;
    }
}
