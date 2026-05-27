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
            components = new System.ComponentModel.Container();
            btnOpenFolder = new System.Windows.Forms.Button();
            txtSelectedFolder = new System.Windows.Forms.TextBox();
            btnOpenDataFolder = new System.Windows.Forms.Button();
            grpList = new System.Windows.Forms.GroupBox();
            lstFrames = new System.Windows.Forms.ListBox();
            lblStats = new System.Windows.Forms.Label();
            grpPreview = new System.Windows.Forms.GroupBox();
            picFrame = new System.Windows.Forms.PictureBox();
            grpImageEdit = new System.Windows.Forms.GroupBox();
            lblEditHint = new System.Windows.Forms.Label();
            cmbMaskMode = new System.Windows.Forms.ComboBox();
            btnMaskRegion = new System.Windows.Forms.Button();
            btnReplaceRegion = new System.Windows.Forms.Button();
            btnClearSelection = new System.Windows.Forms.Button();
            btnRestoreImage = new System.Windows.Forms.Button();
            pnlTimeline = new System.Windows.Forms.Panel();
            trbFrame = new System.Windows.Forms.TrackBar();
            btnPrev = new System.Windows.Forms.Button();
            btnPlay = new System.Windows.Forms.Button();
            btnNext = new System.Windows.Forms.Button();
            btnUndo = new System.Windows.Forms.Button();
            btnDelete = new System.Windows.Forms.Button();
            btnSave = new System.Windows.Forms.Button();
            lblCurrentIndex = new System.Windows.Forms.Label();
            lblCurrentImage = new System.Windows.Forms.Label();
            lblCurrentMode = new System.Windows.Forms.Label();
            lblAngle = new System.Windows.Forms.Label();
            txtAngle = new System.Windows.Forms.TextBox();
            lblThrottle = new System.Windows.Forms.Label();
            txtThrottle = new System.Windows.Forms.TextBox();
            lblGraph = new System.Windows.Forms.Label();
            picGraph = new System.Windows.Forms.PictureBox();
            grpFilter = new System.Windows.Forms.GroupBox();
            chkThrottlePositive = new System.Windows.Forms.CheckBox();
            chkExcludeAngleZero = new System.Windows.Forms.CheckBox();
            chkAngleRange = new System.Windows.Forms.CheckBox();
            chkThrottleRange = new System.Windows.Forms.CheckBox();
            chkAnomalyOnly = new System.Windows.Forms.CheckBox();
            lblRangeMin = new System.Windows.Forms.Label();
            lblRangeMax = new System.Windows.Forms.Label();
            numAngleMin = new System.Windows.Forms.NumericUpDown();
            numAngleMax = new System.Windows.Forms.NumericUpDown();
            numThrottleMin = new System.Windows.Forms.NumericUpDown();
            numThrottleMax = new System.Windows.Forms.NumericUpDown();
            btnApplyFilter = new System.Windows.Forms.Button();
            btnClearFilter = new System.Windows.Forms.Button();
            grpAnomaly = new System.Windows.Forms.GroupBox();
            lblAnomalyWindow = new System.Windows.Forms.Label();
            numAnomalyWindow = new System.Windows.Forms.NumericUpDown();
            lblAnomalySigma = new System.Windows.Forms.Label();
            numAnomalySigma = new System.Windows.Forms.NumericUpDown();
            btnAnalyzeAnomaly = new System.Windows.Forms.Button();
            btnClearAnomaly = new System.Windows.Forms.Button();
            btnNextAnomaly = new System.Windows.Forms.Button();
            lblAnomalyStatus = new System.Windows.Forms.Label();
            lblAnomalyHint = new System.Windows.Forms.Label();
            grpTrain = new System.Windows.Forms.GroupBox();
            lblCommand = new System.Windows.Forms.Label();
            txtTrainCommand = new System.Windows.Forms.TextBox();
            btnTrain = new System.Windows.Forms.Button();
            lblHint = new System.Windows.Forms.Label();
            grpLog = new System.Windows.Forms.GroupBox();
            txtLog = new System.Windows.Forms.TextBox();
            grpList.SuspendLayout();
            grpPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picFrame).BeginInit();
            grpImageEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trbFrame).BeginInit();
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
            btnOpenFolder.Location = new System.Drawing.Point(12, 12);
            btnOpenFolder.Name = "btnOpenFolder";
            btnOpenFolder.Size = new System.Drawing.Size(96, 30);
            btnOpenFolder.TabIndex = 0;
            btnOpenFolder.Text = "폴더 열기";
            btnOpenFolder.UseVisualStyleBackColor = true;
            btnOpenFolder.Click += btnOpenFolder_Click;
            // 
            // txtSelectedFolder
            // 
            txtSelectedFolder.Location = new System.Drawing.Point(116, 15);
            txtSelectedFolder.Name = "txtSelectedFolder";
            txtSelectedFolder.ReadOnly = true;
            txtSelectedFolder.Size = new System.Drawing.Size(770, 23);
            txtSelectedFolder.TabIndex = 1;
            txtSelectedFolder.Text = "mycar 폴더, data 폴더, 또는 tub 폴더를 선택하세요.";
            // 
            // btnOpenDataFolder
            // 
            btnOpenDataFolder.Location = new System.Drawing.Point(894, 12);
            btnOpenDataFolder.Name = "btnOpenDataFolder";
            btnOpenDataFolder.Size = new System.Drawing.Size(128, 30);
            btnOpenDataFolder.TabIndex = 2;
            btnOpenDataFolder.Text = "데이터 폴더 열기";
            btnOpenDataFolder.UseVisualStyleBackColor = true;
            btnOpenDataFolder.Click += btnOpenDataFolder_Click;
            // 
            // grpList
            // 
            grpList.Controls.Add(lstFrames);
            grpList.Controls.Add(lblStats);
            grpList.Location = new System.Drawing.Point(12, 52);
            grpList.Name = "grpList";
            grpList.Size = new System.Drawing.Size(270, 760);
            grpList.TabIndex = 3;
            grpList.TabStop = false;
            grpList.Text = "프레임 목록";
            // 
            // lstFrames
            // 
            lstFrames.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            lstFrames.FormattingEnabled = true;
            lstFrames.HorizontalScrollbar = true;
            lstFrames.IntegralHeight = false;
            lstFrames.ItemHeight = 18;
            lstFrames.Location = new System.Drawing.Point(10, 22);
            lstFrames.Name = "lstFrames";
            lstFrames.Size = new System.Drawing.Size(250, 660);
            lstFrames.TabIndex = 0;
            lstFrames.DrawItem += lstFrames_DrawItem;
            lstFrames.SelectedIndexChanged += lstFrames_SelectedIndexChanged;
            // 
            // lblStats
            // 
            lblStats.Location = new System.Drawing.Point(10, 692);
            lblStats.Name = "lblStats";
            lblStats.Size = new System.Drawing.Size(250, 55);
            lblStats.TabIndex = 1;
            lblStats.Text = "데이터를 불러오면 통계가 표시됩니다.";
            // 
            // grpPreview
            // 
            grpPreview.Controls.Add(picFrame);
            grpPreview.Controls.Add(grpImageEdit);
            grpPreview.Controls.Add(pnlTimeline);
            grpPreview.Controls.Add(trbFrame);
            grpPreview.Controls.Add(btnPrev);
            grpPreview.Controls.Add(btnPlay);
            grpPreview.Controls.Add(btnNext);
            grpPreview.Controls.Add(btnUndo);
            grpPreview.Controls.Add(btnDelete);
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
            grpPreview.Location = new System.Drawing.Point(292, 52);
            grpPreview.Name = "grpPreview";
            grpPreview.Size = new System.Drawing.Size(530, 760);
            grpPreview.TabIndex = 4;
            grpPreview.TabStop = false;
            grpPreview.Text = "이미지 / 프레임 탐색 · 편집";
            // 
            // picFrame
            // 
            picFrame.BackColor = System.Drawing.Color.Black;
            picFrame.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picFrame.Location = new System.Drawing.Point(12, 22);
            picFrame.Name = "picFrame";
            picFrame.Size = new System.Drawing.Size(506, 350);
            picFrame.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picFrame.TabIndex = 0;
            picFrame.TabStop = false;
            picFrame.Paint += picFrame_Paint;
            picFrame.MouseDown += picFrame_MouseDown;
            picFrame.MouseMove += picFrame_MouseMove;
            picFrame.MouseUp += picFrame_MouseUp;
            // 
            // grpImageEdit
            // 
            grpImageEdit.Controls.Add(lblEditHint);
            grpImageEdit.Controls.Add(cmbMaskMode);
            grpImageEdit.Controls.Add(btnMaskRegion);
            grpImageEdit.Controls.Add(btnReplaceRegion);
            grpImageEdit.Controls.Add(btnClearSelection);
            grpImageEdit.Controls.Add(btnRestoreImage);
            grpImageEdit.Location = new System.Drawing.Point(12, 378);
            grpImageEdit.Name = "grpImageEdit";
            grpImageEdit.Size = new System.Drawing.Size(506, 78);
            grpImageEdit.TabIndex = 1;
            grpImageEdit.TabStop = false;
            grpImageEdit.Text = "이미지 부분 가리기 / 바꾸기";
            // 
            // lblEditHint
            // 
            lblEditHint.Location = new System.Drawing.Point(10, 20);
            lblEditHint.Name = "lblEditHint";
            lblEditHint.Size = new System.Drawing.Size(490, 18);
            lblEditHint.TabIndex = 0;
            lblEditHint.Text = "이미지 위에서 마우스로 드래그해 영역을 선택한 뒤 편집 버튼을 누르세요.";
            // 
            // cmbMaskMode
            // 
            cmbMaskMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbMaskMode.FormattingEnabled = true;
            cmbMaskMode.Items.AddRange(new object[] { "검정", "흰색", "회색", "평균색" });
            cmbMaskMode.Location = new System.Drawing.Point(10, 44);
            cmbMaskMode.Name = "cmbMaskMode";
            cmbMaskMode.Size = new System.Drawing.Size(80, 23);
            cmbMaskMode.TabIndex = 1;
            // 
            // btnMaskRegion
            // 
            btnMaskRegion.Location = new System.Drawing.Point(98, 42);
            btnMaskRegion.Name = "btnMaskRegion";
            btnMaskRegion.Size = new System.Drawing.Size(85, 27);
            btnMaskRegion.TabIndex = 2;
            btnMaskRegion.Text = "영역 가리기";
            btnMaskRegion.UseVisualStyleBackColor = true;
            btnMaskRegion.Click += btnMaskRegion_Click;
            // 
            // btnReplaceRegion
            // 
            btnReplaceRegion.Location = new System.Drawing.Point(190, 42);
            btnReplaceRegion.Name = "btnReplaceRegion";
            btnReplaceRegion.Size = new System.Drawing.Size(95, 27);
            btnReplaceRegion.TabIndex = 3;
            btnReplaceRegion.Text = "이미지로 교체";
            btnReplaceRegion.UseVisualStyleBackColor = true;
            btnReplaceRegion.Click += btnReplaceRegion_Click;
            // 
            // btnClearSelection
            // 
            btnClearSelection.Location = new System.Drawing.Point(292, 42);
            btnClearSelection.Name = "btnClearSelection";
            btnClearSelection.Size = new System.Drawing.Size(82, 27);
            btnClearSelection.TabIndex = 4;
            btnClearSelection.Text = "선택 해제";
            btnClearSelection.UseVisualStyleBackColor = true;
            btnClearSelection.Click += btnClearSelection_Click;
            // 
            // btnRestoreImage
            // 
            btnRestoreImage.Location = new System.Drawing.Point(382, 42);
            btnRestoreImage.Name = "btnRestoreImage";
            btnRestoreImage.Size = new System.Drawing.Size(92, 27);
            btnRestoreImage.TabIndex = 5;
            btnRestoreImage.Text = "원본 복구";
            btnRestoreImage.UseVisualStyleBackColor = true;
            btnRestoreImage.Click += btnRestoreImage_Click;
            // 
            // pnlTimeline
            // 
            pnlTimeline.BackColor = System.Drawing.Color.White;
            pnlTimeline.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlTimeline.Location = new System.Drawing.Point(12, 462);
            pnlTimeline.Name = "pnlTimeline";
            pnlTimeline.Size = new System.Drawing.Size(506, 18);
            pnlTimeline.TabIndex = 2;
            pnlTimeline.Paint += pnlTimeline_Paint;
            // 
            // trbFrame
            // 
            trbFrame.LargeChange = 1;
            trbFrame.Location = new System.Drawing.Point(12, 482);
            trbFrame.Maximum = 0;
            trbFrame.Name = "trbFrame";
            trbFrame.Size = new System.Drawing.Size(506, 45);
            trbFrame.TabIndex = 3;
            trbFrame.Scroll += trbFrame_Scroll;
            // 
            // btnPrev
            // 
            btnPrev.Location = new System.Drawing.Point(12, 526);
            btnPrev.Name = "btnPrev";
            btnPrev.Size = new System.Drawing.Size(70, 30);
            btnPrev.TabIndex = 4;
            btnPrev.Text = "이전";
            btnPrev.UseVisualStyleBackColor = true;
            btnPrev.Click += btnPrev_Click;
            // 
            // btnPlay
            // 
            btnPlay.Location = new System.Drawing.Point(90, 526);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new System.Drawing.Size(92, 30);
            btnPlay.TabIndex = 5;
            btnPlay.Text = "자동 재생";
            btnPlay.UseVisualStyleBackColor = true;
            btnPlay.Click += btnPlay_Click;
            // 
            // btnNext
            // 
            btnNext.Location = new System.Drawing.Point(190, 526);
            btnNext.Name = "btnNext";
            btnNext.Size = new System.Drawing.Size(70, 30);
            btnNext.TabIndex = 6;
            btnNext.Text = "다음";
            btnNext.UseVisualStyleBackColor = true;
            btnNext.Click += btnNext_Click;
            // 
            // btnUndo
            // 
            btnUndo.Location = new System.Drawing.Point(268, 526);
            btnUndo.Name = "btnUndo";
            btnUndo.Size = new System.Drawing.Size(86, 30);
            btnUndo.TabIndex = 7;
            btnUndo.Text = "삭제 취소";
            btnUndo.UseVisualStyleBackColor = true;
            btnUndo.Click += btnUndo_Click;
            // 
            // btnDelete
            // 
            btnDelete.BackColor = System.Drawing.Color.Red;
            btnDelete.ForeColor = System.Drawing.Color.White;
            btnDelete.Location = new System.Drawing.Point(360, 526);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new System.Drawing.Size(60, 30);
            btnDelete.TabIndex = 8;
            btnDelete.Text = "삭제";
            btnDelete.UseVisualStyleBackColor = false;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnSave
            // 
            btnSave.BackColor = System.Drawing.Color.LimeGreen;
            btnSave.ForeColor = System.Drawing.Color.White;
            btnSave.Location = new System.Drawing.Point(428, 526);
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(80, 30);
            btnSave.TabIndex = 9;
            btnSave.Text = "저장";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += btnSave_Click;
            // 
            // lblCurrentIndex
            // 
            lblCurrentIndex.Location = new System.Drawing.Point(12, 565);
            lblCurrentIndex.Name = "lblCurrentIndex";
            lblCurrentIndex.Size = new System.Drawing.Size(506, 20);
            lblCurrentIndex.TabIndex = 10;
            lblCurrentIndex.Text = "현재 인덱스: -";
            // 
            // lblCurrentImage
            // 
            lblCurrentImage.Location = new System.Drawing.Point(12, 588);
            lblCurrentImage.Name = "lblCurrentImage";
            lblCurrentImage.Size = new System.Drawing.Size(506, 20);
            lblCurrentImage.TabIndex = 11;
            lblCurrentImage.Text = "이미지: -";
            // 
            // lblCurrentMode
            // 
            lblCurrentMode.Location = new System.Drawing.Point(12, 611);
            lblCurrentMode.Name = "lblCurrentMode";
            lblCurrentMode.Size = new System.Drawing.Size(506, 20);
            lblCurrentMode.TabIndex = 12;
            lblCurrentMode.Text = "mode/catalog: -";
            // 
            // lblAngle
            // 
            lblAngle.Location = new System.Drawing.Point(12, 637);
            lblAngle.Name = "lblAngle";
            lblAngle.Size = new System.Drawing.Size(80, 20);
            lblAngle.TabIndex = 13;
            lblAngle.Text = "angle";
            // 
            // txtAngle
            // 
            txtAngle.Location = new System.Drawing.Point(95, 634);
            txtAngle.Name = "txtAngle";
            txtAngle.Size = new System.Drawing.Size(120, 23);
            txtAngle.TabIndex = 14;
            // 
            // lblThrottle
            // 
            lblThrottle.Location = new System.Drawing.Point(235, 637);
            lblThrottle.Name = "lblThrottle";
            lblThrottle.Size = new System.Drawing.Size(80, 20);
            lblThrottle.TabIndex = 15;
            lblThrottle.Text = "throttle";
            // 
            // txtThrottle
            // 
            txtThrottle.Location = new System.Drawing.Point(315, 634);
            txtThrottle.Name = "txtThrottle";
            txtThrottle.Size = new System.Drawing.Size(120, 23);
            txtThrottle.TabIndex = 16;
            // 
            // lblGraph
            // 
            lblGraph.Location = new System.Drawing.Point(12, 664);
            lblGraph.Name = "lblGraph";
            lblGraph.Size = new System.Drawing.Size(506, 20);
            lblGraph.TabIndex = 17;
            lblGraph.Text = "angle / throttle / 이동평균 / 이상 후보";
            // 
            // picGraph
            // 
            picGraph.BackColor = System.Drawing.Color.White;
            picGraph.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picGraph.Location = new System.Drawing.Point(12, 687);
            picGraph.Name = "picGraph";
            picGraph.Size = new System.Drawing.Size(506, 65);
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
            grpFilter.Controls.Add(lblRangeMin);
            grpFilter.Controls.Add(lblRangeMax);
            grpFilter.Controls.Add(numAngleMin);
            grpFilter.Controls.Add(numAngleMax);
            grpFilter.Controls.Add(numThrottleMin);
            grpFilter.Controls.Add(numThrottleMax);
            grpFilter.Controls.Add(btnApplyFilter);
            grpFilter.Controls.Add(btnClearFilter);
            grpFilter.Location = new System.Drawing.Point(832, 52);
            grpFilter.Name = "grpFilter";
            grpFilter.Size = new System.Drawing.Size(380, 235);
            grpFilter.TabIndex = 5;
            grpFilter.TabStop = false;
            grpFilter.Text = "데이터 필터링";
            // 
            // chkThrottlePositive
            // 
            chkThrottlePositive.Location = new System.Drawing.Point(14, 28);
            chkThrottlePositive.Name = "chkThrottlePositive";
            chkThrottlePositive.Size = new System.Drawing.Size(180, 24);
            chkThrottlePositive.TabIndex = 0;
            chkThrottlePositive.Text = "throttle > 0만 표시";
            chkThrottlePositive.UseVisualStyleBackColor = true;
            // 
            // chkExcludeAngleZero
            // 
            chkExcludeAngleZero.Location = new System.Drawing.Point(14, 56);
            chkExcludeAngleZero.Name = "chkExcludeAngleZero";
            chkExcludeAngleZero.Size = new System.Drawing.Size(180, 24);
            chkExcludeAngleZero.TabIndex = 1;
            chkExcludeAngleZero.Text = "angle = 0 제외";
            chkExcludeAngleZero.UseVisualStyleBackColor = true;
            // 
            // chkAngleRange
            // 
            chkAngleRange.Location = new System.Drawing.Point(14, 90);
            chkAngleRange.Name = "chkAngleRange";
            chkAngleRange.Size = new System.Drawing.Size(100, 24);
            chkAngleRange.TabIndex = 2;
            chkAngleRange.Text = "angle 범위";
            chkAngleRange.UseVisualStyleBackColor = true;
            // 
            // chkThrottleRange
            // 
            chkThrottleRange.Location = new System.Drawing.Point(14, 124);
            chkThrottleRange.Name = "chkThrottleRange";
            chkThrottleRange.Size = new System.Drawing.Size(110, 24);
            chkThrottleRange.TabIndex = 3;
            chkThrottleRange.Text = "throttle 범위";
            chkThrottleRange.UseVisualStyleBackColor = true;
            // 
            // chkAnomalyOnly
            // 
            chkAnomalyOnly.Location = new System.Drawing.Point(14, 156);
            chkAnomalyOnly.Name = "chkAnomalyOnly";
            chkAnomalyOnly.Size = new System.Drawing.Size(180, 24);
            chkAnomalyOnly.TabIndex = 4;
            chkAnomalyOnly.Text = "이상 주행 후보만 표시";
            chkAnomalyOnly.UseVisualStyleBackColor = true;
            // 
            // lblRangeMin
            // 
            lblRangeMin.Location = new System.Drawing.Point(130, 72);
            lblRangeMin.Name = "lblRangeMin";
            lblRangeMin.Size = new System.Drawing.Size(50, 18);
            lblRangeMin.TabIndex = 5;
            lblRangeMin.Text = "최소";
            // 
            // lblRangeMax
            // 
            lblRangeMax.Location = new System.Drawing.Point(250, 72);
            lblRangeMax.Name = "lblRangeMax";
            lblRangeMax.Size = new System.Drawing.Size(50, 18);
            lblRangeMax.TabIndex = 6;
            lblRangeMax.Text = "최대";
            // 
            // numAngleMin
            // 
            numAngleMin.DecimalPlaces = 3;
            numAngleMin.Increment = 0.1M;
            numAngleMin.Location = new System.Drawing.Point(130, 90);
            numAngleMin.Maximum = 10M;
            numAngleMin.Minimum = -10M;
            numAngleMin.Name = "numAngleMin";
            numAngleMin.Size = new System.Drawing.Size(100, 23);
            numAngleMin.TabIndex = 7;
            numAngleMin.Value = -1M;
            // 
            // numAngleMax
            // 
            numAngleMax.DecimalPlaces = 3;
            numAngleMax.Increment = 0.1M;
            numAngleMax.Location = new System.Drawing.Point(250, 90);
            numAngleMax.Maximum = 10M;
            numAngleMax.Minimum = -10M;
            numAngleMax.Name = "numAngleMax";
            numAngleMax.Size = new System.Drawing.Size(100, 23);
            numAngleMax.TabIndex = 8;
            numAngleMax.Value = 1M;
            // 
            // numThrottleMin
            // 
            numThrottleMin.DecimalPlaces = 3;
            numThrottleMin.Increment = 0.1M;
            numThrottleMin.Location = new System.Drawing.Point(130, 124);
            numThrottleMin.Maximum = 10M;
            numThrottleMin.Minimum = -10M;
            numThrottleMin.Name = "numThrottleMin";
            numThrottleMin.Size = new System.Drawing.Size(100, 23);
            numThrottleMin.TabIndex = 9;
            // 
            // numThrottleMax
            // 
            numThrottleMax.DecimalPlaces = 3;
            numThrottleMax.Increment = 0.1M;
            numThrottleMax.Location = new System.Drawing.Point(250, 124);
            numThrottleMax.Maximum = 10M;
            numThrottleMax.Minimum = -10M;
            numThrottleMax.Name = "numThrottleMax";
            numThrottleMax.Size = new System.Drawing.Size(100, 23);
            numThrottleMax.TabIndex = 10;
            numThrottleMax.Value = 1M;
            // 
            // btnApplyFilter
            // 
            btnApplyFilter.Location = new System.Drawing.Point(14, 194);
            btnApplyFilter.Name = "btnApplyFilter";
            btnApplyFilter.Size = new System.Drawing.Size(100, 30);
            btnApplyFilter.TabIndex = 11;
            btnApplyFilter.Text = "필터 적용";
            btnApplyFilter.UseVisualStyleBackColor = true;
            btnApplyFilter.Click += btnApplyFilter_Click;
            // 
            // btnClearFilter
            // 
            btnClearFilter.Location = new System.Drawing.Point(122, 194);
            btnClearFilter.Name = "btnClearFilter";
            btnClearFilter.Size = new System.Drawing.Size(110, 30);
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
            grpAnomaly.Location = new System.Drawing.Point(832, 295);
            grpAnomaly.Name = "grpAnomaly";
            grpAnomaly.Size = new System.Drawing.Size(380, 178);
            grpAnomaly.TabIndex = 6;
            grpAnomaly.TabStop = false;
            grpAnomaly.Text = "이상 주행 자동 탐지";
            // 
            // lblAnomalyWindow
            // 
            lblAnomalyWindow.Location = new System.Drawing.Point(14, 28);
            lblAnomalyWindow.Name = "lblAnomalyWindow";
            lblAnomalyWindow.Size = new System.Drawing.Size(120, 20);
            lblAnomalyWindow.TabIndex = 0;
            lblAnomalyWindow.Text = "이동평균 window";
            // 
            // numAnomalyWindow
            // 
            numAnomalyWindow.Location = new System.Drawing.Point(140, 26);
            numAnomalyWindow.Maximum = 200M;
            numAnomalyWindow.Minimum = 2M;
            numAnomalyWindow.Name = "numAnomalyWindow";
            numAnomalyWindow.Size = new System.Drawing.Size(80, 23);
            numAnomalyWindow.TabIndex = 1;
            numAnomalyWindow.Value = 15M;
            // 
            // lblAnomalySigma
            // 
            lblAnomalySigma.Location = new System.Drawing.Point(230, 28);
            lblAnomalySigma.Name = "lblAnomalySigma";
            lblAnomalySigma.Size = new System.Drawing.Size(60, 20);
            lblAnomalySigma.TabIndex = 2;
            lblAnomalySigma.Text = "σ 배수";
            // 
            // numAnomalySigma
            // 
            numAnomalySigma.DecimalPlaces = 1;
            numAnomalySigma.Increment = 0.1M;
            numAnomalySigma.Location = new System.Drawing.Point(292, 26);
            numAnomalySigma.Maximum = 10M;
            numAnomalySigma.Minimum = 0.5M;
            numAnomalySigma.Name = "numAnomalySigma";
            numAnomalySigma.Size = new System.Drawing.Size(65, 23);
            numAnomalySigma.TabIndex = 3;
            numAnomalySigma.Value = 2.5M;
            // 
            // btnAnalyzeAnomaly
            // 
            btnAnalyzeAnomaly.Location = new System.Drawing.Point(14, 62);
            btnAnalyzeAnomaly.Name = "btnAnalyzeAnomaly";
            btnAnalyzeAnomaly.Size = new System.Drawing.Size(120, 30);
            btnAnalyzeAnomaly.TabIndex = 4;
            btnAnalyzeAnomaly.Text = "이상 탐지 실행";
            btnAnalyzeAnomaly.UseVisualStyleBackColor = true;
            btnAnalyzeAnomaly.Click += btnAnalyzeAnomaly_Click;
            // 
            // btnClearAnomaly
            // 
            btnClearAnomaly.Location = new System.Drawing.Point(144, 62);
            btnClearAnomaly.Name = "btnClearAnomaly";
            btnClearAnomaly.Size = new System.Drawing.Size(110, 30);
            btnClearAnomaly.TabIndex = 5;
            btnClearAnomaly.Text = "탐지 초기화";
            btnClearAnomaly.UseVisualStyleBackColor = true;
            btnClearAnomaly.Click += btnClearAnomaly_Click;
            // 
            // btnNextAnomaly
            // 
            btnNextAnomaly.Location = new System.Drawing.Point(264, 62);
            btnNextAnomaly.Name = "btnNextAnomaly";
            btnNextAnomaly.Size = new System.Drawing.Size(92, 30);
            btnNextAnomaly.TabIndex = 6;
            btnNextAnomaly.Text = "다음 이상";
            btnNextAnomaly.UseVisualStyleBackColor = true;
            btnNextAnomaly.Click += btnNextAnomaly_Click;
            // 
            // lblAnomalyStatus
            // 
            lblAnomalyStatus.Location = new System.Drawing.Point(14, 104);
            lblAnomalyStatus.Name = "lblAnomalyStatus";
            lblAnomalyStatus.Size = new System.Drawing.Size(350, 22);
            lblAnomalyStatus.TabIndex = 6;
            lblAnomalyStatus.Text = "이상 탐지 전";
            // 
            // lblAnomalyHint
            // 
            lblAnomalyHint.Location = new System.Drawing.Point(14, 130);
            lblAnomalyHint.Name = "lblAnomalyHint";
            lblAnomalyHint.Size = new System.Drawing.Size(350, 40);
            lblAnomalyHint.TabIndex = 7;
            lblAnomalyHint.Text = "조향각 이동평균·변동성 밴드를 벗어난 프레임을 목록/그래프/타임라인에 빨간색으로 표시합니다.";
            // 
            // grpTrain
            // 
            grpTrain.Controls.Add(lblCommand);
            grpTrain.Controls.Add(txtTrainCommand);
            grpTrain.Controls.Add(btnTrain);
            grpTrain.Controls.Add(lblHint);
            grpTrain.Location = new System.Drawing.Point(832, 483);
            grpTrain.Name = "grpTrain";
            grpTrain.Size = new System.Drawing.Size(380, 160);
            grpTrain.TabIndex = 7;
            grpTrain.TabStop = false;
            grpTrain.Text = "AI 학습 실행(C# -> Python/DonkeyCar)";
            // 
            // lblCommand
            // 
            lblCommand.Location = new System.Drawing.Point(14, 25);
            lblCommand.Name = "lblCommand";
            lblCommand.Size = new System.Drawing.Size(100, 20);
            lblCommand.TabIndex = 0;
            lblCommand.Text = "학습 명령";
            // 
            // txtTrainCommand
            // 
            txtTrainCommand.Location = new System.Drawing.Point(14, 48);
            txtTrainCommand.Multiline = true;
            txtTrainCommand.Name = "txtTrainCommand";
            txtTrainCommand.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtTrainCommand.Size = new System.Drawing.Size(350, 55);
            txtTrainCommand.TabIndex = 1;
            txtTrainCommand.Text = "donkey train --tub \"{DATA_FOLDER}\" --model \"{ROOT_FOLDER}\\models\\mypilot.h5\"";
            // 
            // btnTrain
            // 
            btnTrain.Location = new System.Drawing.Point(14, 111);
            btnTrain.Name = "btnTrain";
            btnTrain.Size = new System.Drawing.Size(100, 30);
            btnTrain.TabIndex = 2;
            btnTrain.Text = "학습 실행";
            btnTrain.UseVisualStyleBackColor = true;
            btnTrain.Click += btnTrain_Click;
            // 
            // lblHint
            // 
            lblHint.Location = new System.Drawing.Point(122, 116);
            lblHint.Name = "lblHint";
            lblHint.Size = new System.Drawing.Size(250, 20);
            lblHint.TabIndex = 3;
            lblHint.Text = "사용 가능: {ROOT_FOLDER}, {DATA_FOLDER}";
            // 
            // grpLog
            // 
            grpLog.Controls.Add(txtLog);
            grpLog.Location = new System.Drawing.Point(832, 653);
            grpLog.Name = "grpLog";
            grpLog.Size = new System.Drawing.Size(380, 159);
            grpLog.TabIndex = 8;
            grpLog.TabStop = false;
            grpLog.Text = "실행 로그";
            // 
            // txtLog
            // 
            txtLog.Location = new System.Drawing.Point(10, 22);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtLog.Size = new System.Drawing.Size(360, 145);
            txtLog.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(248, 248, 248);
            ClientSize = new System.Drawing.Size(1224, 821);
            Controls.Add(btnOpenFolder);
            Controls.Add(txtSelectedFolder);
            Controls.Add(btnOpenDataFolder);
            Controls.Add(grpList);
            Controls.Add(grpPreview);
            Controls.Add(grpFilter);
            Controls.Add(grpAnomaly);
            Controls.Add(grpTrain);
            Controls.Add(grpLog);
            Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 129);
            MinimumSize = new System.Drawing.Size(1180, 800);
            Name = "Form1";
            Text = "DonkeyCar UI 데이터 관리 도구";
            grpList.ResumeLayout(false);
            grpPreview.ResumeLayout(false);
            grpPreview.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picFrame).EndInit();
            grpImageEdit.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)trbFrame).EndInit();
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
        private System.Windows.Forms.ListBox lstFrames = null!;
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
        private System.Windows.Forms.Panel pnlTimeline = null!;
        private System.Windows.Forms.TrackBar trbFrame = null!;
        private System.Windows.Forms.Button btnPrev = null!;
        private System.Windows.Forms.Button btnNext = null!;
        private System.Windows.Forms.Button btnPlay = null!;
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
        private System.Windows.Forms.Label lblHint = null!;
        private System.Windows.Forms.GroupBox grpLog = null!;
        private System.Windows.Forms.TextBox txtLog = null!;
    }
}
