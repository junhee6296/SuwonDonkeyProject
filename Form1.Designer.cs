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
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            btnMember1 = new Button();
            btnMember2 = new Button();
            btnMember3 = new Button();
            btnMember4 = new Button();
            txtName = new TextBox();
            txtSchool = new TextBox();
            txtClass = new TextBox();
            txtGGa = new TextBox();
            label5 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("맑은 고딕", 36F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label1.Location = new Point(31, 20);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(330, 81);
            label1.TabIndex = 0;
            label1.Text = "Team App";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("맑은 고딕", 27.75F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label2.ForeColor = Color.Blue;
            label2.Location = new Point(42, 127);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(121, 62);
            label2.TabIndex = 1;
            label2.Text = "이름";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("맑은 고딕", 27.75F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label3.ForeColor = Color.Blue;
            label3.Location = new Point(42, 212);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(121, 62);
            label3.TabIndex = 2;
            label3.Text = "학교";
            label3.Click += label3_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("맑은 고딕", 27.75F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label4.ForeColor = Color.Blue;
            label4.Location = new Point(42, 300);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(121, 62);
            label4.TabIndex = 3;
            label4.Text = "학년";
            // 
            // btnMember1
            // 
            btnMember1.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            btnMember1.Location = new Point(599, 132);
            btnMember1.Margin = new Padding(4);
            btnMember1.Name = "btnMember1";
            btnMember1.Size = new Size(152, 63);
            btnMember1.TabIndex = 4;
            btnMember1.Text = "멤버1";
            btnMember1.UseVisualStyleBackColor = true;
            // 
            // btnMember2
            // 
            btnMember2.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            btnMember2.Location = new Point(599, 217);
            btnMember2.Margin = new Padding(4);
            btnMember2.Name = "btnMember2";
            btnMember2.Size = new Size(152, 63);
            btnMember2.TabIndex = 5;
            btnMember2.Text = "멤버2";
            btnMember2.UseVisualStyleBackColor = true;
            // 
            // btnMember3
            // 
            btnMember3.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            btnMember3.Location = new Point(599, 305);
            btnMember3.Margin = new Padding(4);
            btnMember3.Name = "btnMember3";
            btnMember3.Size = new Size(152, 63);
            btnMember3.TabIndex = 6;
            btnMember3.Text = "멤버3";
            btnMember3.UseVisualStyleBackColor = true;
            // 
            // btnMember4
            // 
            btnMember4.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            btnMember4.Location = new Point(599, 393);
            btnMember4.Margin = new Padding(4);
            btnMember4.Name = "btnMember4";
            btnMember4.Size = new Size(152, 63);
            btnMember4.TabIndex = 7;
            btnMember4.Text = "멤버4";
            btnMember4.UseVisualStyleBackColor = true;
            // 
            // txtName
            // 
            txtName.Font = new Font("맑은 고딕", 24F, FontStyle.Regular, GraphicsUnit.Point, 129);
            txtName.Location = new Point(183, 127);
            txtName.Margin = new Padding(4);
            txtName.Name = "txtName";
            txtName.Size = new Size(359, 61);
            txtName.TabIndex = 8;
            // 
            // txtSchool
            // 
            txtSchool.Font = new Font("맑은 고딕", 24F, FontStyle.Regular, GraphicsUnit.Point, 129);
            txtSchool.Location = new Point(183, 215);
            txtSchool.Margin = new Padding(4);
            txtSchool.Name = "txtSchool";
            txtSchool.Size = new Size(359, 61);
            txtSchool.TabIndex = 9;
            // 
            // txtClass
            // 
            txtClass.Font = new Font("맑은 고딕", 24F, FontStyle.Regular, GraphicsUnit.Point, 129);
            txtClass.Location = new Point(183, 307);
            txtClass.Margin = new Padding(4);
            txtClass.Name = "txtClass";
            txtClass.Size = new Size(359, 61);
            txtClass.TabIndex = 10;
            // 
            // txtGGa
            // 
            txtGGa.Font = new Font("맑은 고딕", 24F, FontStyle.Regular, GraphicsUnit.Point, 129);
            txtGGa.Location = new Point(183, 395);
            txtGGa.Margin = new Padding(4);
            txtGGa.Name = "txtGGa";
            txtGGa.Size = new Size(359, 61);
            txtGGa.TabIndex = 11;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("맑은 고딕", 27.75F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label5.ForeColor = Color.Blue;
            label5.Location = new Point(42, 388);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(121, 62);
            label5.TabIndex = 12;
            label5.Text = "학과";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(786, 511);
            Controls.Add(label5);
            Controls.Add(txtGGa);
            Controls.Add(txtClass);
            Controls.Add(txtSchool);
            Controls.Add(txtName);
            Controls.Add(btnMember4);
            Controls.Add(btnMember3);
            Controls.Add(btnMember2);
            Controls.Add(btnMember1);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Margin = new Padding(4);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Button btnMember1;
        private Button btnMember2;
        private Button btnMember3;
        private Button btnMember4;
        private TextBox txtName;
        private TextBox txtSchool;
        private TextBox txtClass;
        private TextBox txtGGa;
        private Label label5;
    }
}
