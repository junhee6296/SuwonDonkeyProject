namespace TeamApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            btnMember1.Click += BtnMember1_Click;
            btnMember2.Click += BtnMember2_Click;
            btnMember3.Click += BtnMember3_Click;
            btnMember4.Click += BtnMember4_Click;
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void LoadMemberInfo(string fileName)
        {
            try
            {
                string[] lines = File.ReadAllLines(fileName);
                if (lines.Length >= 3)
                {
                    txtName.Text = lines[0];
                    txtSchool.Text = lines[1];
                    txtClass.Text = lines[2];
                    txtGGa.Text = lines[3];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"파일을 읽는 중 오류가 발생했습니다: {ex.Message}");
            }
        }

        private void BtnMember1_Click(object sender, EventArgs e)
        {
            LoadMemberInfo("member1.txt");
        }

        private void BtnMember2_Click(object sender, EventArgs e)
        {
            LoadMemberInfo("member2.txt");
        }

        private void BtnMember3_Click(object sender, EventArgs e)
        {
            LoadMemberInfo("member3.txt");
        }

        private void BtnMember4_Click(object sender, EventArgs e)
        {
            LoadMemberInfo("member4.txt");
        }
    }
}
