using System;
using System.Windows.Forms;

namespace RegAurora
{
    public partial class Password : Form
    {
        public Password()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)              //密码匹配
        {
            if (textBox1.Text == "aurora")
            {
                RegMain frm = new RegMain();
                frm.Show();
                this.Hide();
            }
            else
            {
                Application.Exit();
            }
        }
    }
}
