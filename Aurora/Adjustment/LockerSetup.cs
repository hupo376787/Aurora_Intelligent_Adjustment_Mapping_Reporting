using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Adjustment
{
    public partial class LockerSetup : Form
    {
        public LockerSetup()
        {
            InitializeComponent();
        }

        public int Flag1 = 0;
        public int Flag2 = 0;

        private void LockerSetup_Load(object sender, EventArgs e)
        {
            pictureBox1.Visible = false;
            pictureBox2.Visible = false;
            pictureBox3.Visible = false;

            RegistryKey MyReg1, RegShowText;
            MyReg1 = Registry.CurrentUser;
            RegShowText = MyReg1.CreateSubKey("Software\\Aurora\\Locker");
            try
            {
                textBox4.Text = RegShowText.GetValue("ShowText").ToString();
            }
            catch { }
        }

        private void LockerSetup_Activated(object sender, EventArgs e)
        {
            textBox4.Focus();               //测试，在formload中无效
        }

        private void button1_Click(object sender, EventArgs e)              //确认
        {
            RegistryKey MyReg1, RegPassword;
            MyReg1 = Registry.CurrentUser;
            RegPassword = MyReg1.CreateSubKey("Software\\Aurora\\Locker");

            string strPassword = textBox3.Text;
            string strShowText = textBox4.Text;

            if (Flag1 == 1 && Flag2 == 1 && textBox3.Text != "")               //将修改后的密码写入本地ini文件
            {
                try
                {
                    RegPassword.SetValue("Password", strPassword);
                }
                catch { }
            }

            if (textBox4 != null)
            {
                try
                {
                    RegPassword.SetValue("ShowText", strShowText);
                }
                catch { }
            }

            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)              //取消
        {
            this.Close();
        }

        private void textBox1_Leave(object sender, EventArgs e)             //判断输入密码和原来的密码
        {
            if (textBox1.Text == "")
            {
                pictureBox1.Visible = false;
            }
            string strPassword = "";
            RegistryKey MyReg1, RegPassword;
            MyReg1 = Registry.CurrentUser;
            RegPassword = MyReg1.CreateSubKey("Software\\Aurora\\Locker");
            try
            {
                strPassword = RegPassword.GetValue("Password").ToString();
            }
            catch { }

            if (textBox1.Text == strPassword)       // || textBox1.Text == "Administrator")
            {
                pictureBox1.Visible = true;
                pictureBox1.Image = Properties.Resources.Right;
                Flag1 = 1;
                textBox2.Enabled = true;
                textBox3.Enabled = true;
            }
            else
            {
                pictureBox1.Visible = true;
                pictureBox1.Image = Properties.Resources.Wrong;
                //MessageBox.Show("原密码输入错误", "Aurora智能提示");
                textBox2.Enabled = false;
                textBox3.Enabled = false;
            }
        }

        private void textBox2_Enter(object sender, EventArgs e)          //进入textBox2，判断输入的原始密码是否为空。
        {
            if (textBox1.Text == "")
            {
                pictureBox1.Visible = true;
                pictureBox1.Image = Properties.Resources.Wrong;
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (textBox2.Text == "")
            {
                pictureBox3.Visible = true;
                pictureBox3.Image = Properties.Resources.Wrong;
            }
            else
            {
                pictureBox3.Visible = true;
                pictureBox3.Image = Properties.Resources.Right;
            }
        }

        private void textBox3_Leave(object sender, EventArgs e)             //判断修改后的两次输入密码
        {
            if (textBox3.Text == textBox2.Text)
            {
                pictureBox2.Visible = true;
                pictureBox2.Image = Properties.Resources.Right;
                Flag2 = 1;
            }
            else
            {
                pictureBox2.Visible = true;
                pictureBox2.Image = Properties.Resources.Wrong;
            }
            if (textBox2.Text == "")
            {
                pictureBox2.Visible = true;
                pictureBox2.Image = Properties.Resources.Wrong;
            }
        }






    }
}
