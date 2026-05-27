namespace TeamApp
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnSelectFolder = new Button();
            lstIndexList = new ListBox();
            txtSelectedFolder = new TextBox();
            picCurrentIndexImage = new PictureBox();
            trbChangeIndex = new TrackBar();
            btnBefore = new Button();
            btnAfter = new Button();
            btnDelete = new Button();
            btnSave = new Button();
            grpEditValue = new GroupBox();
            txtChangeAngle = new TextBox();
            txtChangeThrottle = new TextBox();
            lblChangeAngle = new Label();
            lblChangeThrottle = new Label();
            lblCurrentIndex = new Label();
            lblCurrentThrottle = new Label();
            lblCurrentAngle = new Label();
            btnUndo = new Button();
            ((System.ComponentModel.ISupportInitialize)picCurrentIndexImage).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trbChangeIndex).BeginInit();
            grpEditValue.SuspendLayout();
            SuspendLayout();
            // 
            // btnSelectFolder
            // 
            btnSelectFolder.Location = new Point(12, 22);
            btnSelectFolder.Name = "btnSelectFolder";
            btnSelectFolder.Size = new Size(94, 29);
            btnSelectFolder.TabIndex = 0;
            btnSelectFolder.Text = "폴더선택";
            btnSelectFolder.UseVisualStyleBackColor = true;
            btnSelectFolder.Click += btnSelectFolder_Click;
            // 
            // lstIndexList
            // 
            lstIndexList.FormattingEnabled = true;
            lstIndexList.Location = new Point(12, 67);
            lstIndexList.Name = "lstIndexList";
            lstIndexList.Size = new Size(175, 504);
            lstIndexList.TabIndex = 1;
            // 
            // txtSelectedFolder
            // 
            txtSelectedFolder.Location = new Point(112, 22);
            txtSelectedFolder.Name = "txtSelectedFolder";
            txtSelectedFolder.Size = new Size(401, 27);
            txtSelectedFolder.TabIndex = 2;
            txtSelectedFolder.Text = "mycar 폴더를 선택해주세요";
            // 
            // picCurrentIndexImage
            // 
            picCurrentIndexImage.Location = new Point(203, 67);
            picCurrentIndexImage.Name = "picCurrentIndexImage";
            picCurrentIndexImage.Size = new Size(310, 293);
            picCurrentIndexImage.TabIndex = 3;
            picCurrentIndexImage.TabStop = false;
            // 
            // trbChangeIndex
            // 
            trbChangeIndex.Location = new Point(203, 371);
            trbChangeIndex.Name = "trbChangeIndex";
            trbChangeIndex.Size = new Size(310, 56);
            trbChangeIndex.TabIndex = 4;
            trbChangeIndex.Scroll += trackBar1_Scroll;
            // 
            // btnBefore
            // 
            btnBefore.Location = new Point(203, 412);
            btnBefore.Name = "btnBefore";
            btnBefore.Size = new Size(36, 29);
            btnBefore.TabIndex = 5;
            btnBefore.Text = "<";
            btnBefore.UseVisualStyleBackColor = true;
            btnBefore.Click += btnBefore_Click;
            // 
            // btnAfter
            // 
            btnAfter.Location = new Point(245, 412);
            btnAfter.Name = "btnAfter";
            btnAfter.Size = new Size(36, 29);
            btnAfter.TabIndex = 6;
            btnAfter.Text = ">";
            btnAfter.UseVisualStyleBackColor = true;
            btnAfter.Click += btnAfter_Click;
            // 
            // btnDelete
            // 
            btnDelete.BackColor = Color.Red;
            btnDelete.ForeColor = SystemColors.ButtonFace;
            btnDelete.Location = new Point(380, 412);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(66, 29);
            btnDelete.TabIndex = 7;
            btnDelete.Text = "삭제";
            btnDelete.UseVisualStyleBackColor = false;
            // 
            // btnUndo
            // 
            btnUndo.BackColor = Color.White;
            btnUndo.ForeColor = Color.Black;
            btnUndo.Location = new Point(447, 412);
            btnUndo.Name = "btnUndo";
            btnUndo.Size = new Size(78, 29);
            btnUndo.TabIndex = 13;
            btnUndo.Text = "되돌리기";
            btnUndo.UseVisualStyleBackColor = false;
            // 
            // btnSave
            // 
            btnSave.BackColor = Color.Lime;
            btnSave.Location = new Point(447, 412);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(66, 29);
            btnSave.TabIndex = 8;
            btnSave.Text = "저장";
            btnSave.UseVisualStyleBackColor = false;
            // 
            // grpEditValue
            // 
            grpEditValue.Controls.Add(txtChangeAngle);
            grpEditValue.Controls.Add(txtChangeThrottle);
            grpEditValue.Controls.Add(lblChangeAngle);
            grpEditValue.Controls.Add(lblChangeThrottle);
            grpEditValue.Location = new Point(203, 445);
            grpEditValue.Name = "grpEditValue";
            grpEditValue.Size = new Size(310, 126);
            grpEditValue.TabIndex = 9;
            grpEditValue.TabStop = false;
            grpEditValue.Text = "값 수정";
            // 
            // txtChangeAngle
            // 
            txtChangeAngle.Location = new Point(85, 80);
            txtChangeAngle.Name = "txtChangeAngle";
            txtChangeAngle.Size = new Size(196, 27);
            txtChangeAngle.TabIndex = 3;
            // 
            // txtChangeThrottle
            // 
            txtChangeThrottle.Location = new Point(85, 43);
            txtChangeThrottle.Name = "txtChangeThrottle";
            txtChangeThrottle.Size = new Size(196, 27);
            txtChangeThrottle.TabIndex = 2;
            // 
            // lblChangeAngle
            // 
            lblChangeAngle.AutoSize = true;
            lblChangeAngle.Location = new Point(20, 80);
            lblChangeAngle.Name = "lblChangeAngle";
            lblChangeAngle.Size = new Size(47, 20);
            lblChangeAngle.TabIndex = 1;
            lblChangeAngle.Text = "angle";
            // 
            // lblChangeThrottle
            // 
            lblChangeThrottle.AutoSize = true;
            lblChangeThrottle.Location = new Point(20, 43);
            lblChangeThrottle.Name = "lblChangeThrottle";
            lblChangeThrottle.Size = new Size(59, 20);
            lblChangeThrottle.TabIndex = 0;
            lblChangeThrottle.Text = "throttle";
            // 
            // lblCurrentIndex
            // 
            lblCurrentIndex.AutoSize = true;
            lblCurrentIndex.Location = new Point(519, 67);
            lblCurrentIndex.Name = "lblCurrentIndex";
            lblCurrentIndex.Size = new Size(50, 20);
            lblCurrentIndex.TabIndex = 10;
            lblCurrentIndex.Text = "label1";
            // 
            // lblCurrentThrottle
            // 
            lblCurrentThrottle.AutoSize = true;
            lblCurrentThrottle.Location = new Point(519, 87);
            lblCurrentThrottle.Name = "lblCurrentThrottle";
            lblCurrentThrottle.Size = new Size(50, 20);
            lblCurrentThrottle.TabIndex = 11;
            lblCurrentThrottle.Text = "label1";
            // 
            // lblCurrentAngle
            // 
            lblCurrentAngle.AutoSize = true;
            lblCurrentAngle.Location = new Point(519, 107);
            lblCurrentAngle.Name = "lblCurrentAngle";
            lblCurrentAngle.Size = new Size(50, 20);
            lblCurrentAngle.TabIndex = 12;
            lblCurrentAngle.Text = "label1";
            // 
            // btnUndo
            // 
            btnUndo.BackColor = Color.White;
            btnUndo.ForeColor = Color.Black;
            btnUndo.Location = new Point(296, 412);
            btnUndo.Name = "btnUndo";
            btnUndo.Size = new Size(78, 29);
            btnUndo.TabIndex = 13;
            btnUndo.Text = "되돌리기";
            btnUndo.UseVisualStyleBackColor = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(837, 592);
            Controls.Add(btnUndo);
            Controls.Add(lblCurrentAngle);
            Controls.Add(lblCurrentThrottle);
            Controls.Add(lblCurrentIndex);
            Controls.Add(grpEditValue);
            Controls.Add(btnSave);
            Controls.Add(btnDelete);
            Controls.Add(btnAfter);
            Controls.Add(btnBefore);
            Controls.Add(trbChangeIndex);
            Controls.Add(picCurrentIndexImage);
            Controls.Add(txtSelectedFolder);
            Controls.Add(lstIndexList);
            Controls.Add(btnSelectFolder);
            Margin = new Padding(4);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)picCurrentIndexImage).EndInit();
            ((System.ComponentModel.ISupportInitialize)trbChangeIndex).EndInit();
            grpEditValue.ResumeLayout(false);
            grpEditValue.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnSelectFolder;
        private ListBox lstIndexList;
        private TextBox txtSelectedFolder;
        private PictureBox picCurrentIndexImage;
        private TrackBar trbChangeIndex;
        private Button btnBefore;
        private Button btnAfter;
        private Button btnDelete;
        private Button btnSave;
        private GroupBox grpEditValue;
        private TextBox txtChangeAngle;
        private TextBox txtChangeThrottle;
        private Label lblChangeAngle;
        private Label lblChangeThrottle;
        private Label lblCurrentIndex;
        private Label lblCurrentThrottle;
        private Label lblCurrentAngle;
        private Button btnUndo;
    }
}
