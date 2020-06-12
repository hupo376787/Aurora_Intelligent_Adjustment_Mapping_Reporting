using System;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Threading;

namespace Adjustment
{
    public partial class LockerPsd : Form
    {
        public LockerPsd()
        {
            InitializeComponent();
            System.Windows.Forms.Cursor.Show(); //Show鼠标
        }

        protected override void WndProc(ref Message m)              //屏蔽Alt + F4关闭
        {
            const int WM_SYSCOMMAND = 0x112;
            const int SC_CLOSE = 0xF060;
            if (m.Msg == WM_SYSCOMMAND && m.WParam == (IntPtr)SC_CLOSE && m.LParam == IntPtr.Zero)
                return;
            base.WndProc(ref m);
        }

        private void button1_Click(object sender, EventArgs e)      //和注册表Locker\\Password对比，一样可以退出。
        {
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
                this.Close();
                PublicClass.Locker.Hide();
                PublicClass.AuroraMain.Show();
                PublicClass.AuroraMain.Activate();              //为毛焦点不能到主窗体上去。退出Locker后按任意键仍然弹出密码框。
                PublicClass.AuroraMain.WindowState = FormWindowState.Normal;
                
                System.Windows.Forms.Cursor.Show(); //显示鼠标

            }
            else
            {
                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                {
                    label1.Text = "输入密码不正确，请重新输入。";
                }
                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                {
                    label1.Text = "輸入密碼不正確，請重新輸入。";
                } 
                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                {
                    label1.Text = "Incorrect password, please try again.";
                }
                textBox1.Clear();
                textBox1.Focus();
            }
        }

        private void button2_Click(object sender, EventArgs e)              //修改密码，打开LockerSetup
        {
            this.Close();
            LockerSetup frmSetup = new LockerSetup();
            frmSetup.StartPosition = FormStartPosition.CenterScreen;
            frmSetup.ShowDialog();
        }

        private void LockerPsd_FormClosed(object sender, FormClosedEventArgs e)
        {
            //System.Windows.Forms.Cursor.Hide(); //隐藏鼠标
        }


    }
}
