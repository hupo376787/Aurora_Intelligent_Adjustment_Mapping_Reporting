using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Threading;

namespace Adjustment
{
    public partial class Locker : Form
    {
        public int PreviousX;
        public int PreviousY;

        public Locker()
        {
            InitializeComponent();
            //this.ControlBox = false;
        }

        private void Locker_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.Cursor.Hide(); //隐藏鼠标
            if (PublicClass.AuroraMain.WindowState != FormWindowState.Minimized)
            {
                this.Location = new Point(PublicClass.AuroraMain.Location.X, PublicClass.AuroraMain.Location.Y);
                this.Height = PublicClass.AuroraMain.Height;
                this.Width = PublicClass.AuroraMain.Width;
            } 
            else
            {
                this.WindowState = FormWindowState.Minimized;
            }

            //是否全屏运行Locker
            RegistryKey MyReg, RegFullScreen;
            MyReg = Registry.CurrentUser;
            try
            {
                RegFullScreen = MyReg.CreateSubKey("Software\\Aurora\\Locker");
                if (RegFullScreen.GetValue("FullScreen").ToString() == "YES")
                {
                    this.FormBorderStyle = FormBorderStyle.None;
                    this.WindowState = FormWindowState.Maximized;
                    this.TopMost = true;
                }
            }
            catch { }
            
        }

        private void Locker_FormClosing(object sender, FormClosingEventArgs e)      //关闭特效
        {
            //AnimateWindow(this.Handle, 500, AW_HIDE | AW_SLIDE | AW_VER_NEGATIVE);        //向上卷起有残影，此处不再使用。
            System.Windows.Forms.Cursor.Show();     //显示鼠标
        }

        private void timer1_Tick(object sender, EventArgs e)            //Timer_tick
        {
            //标题栏文字左右移动
            //this.Text = this.Text.Substring(1) + this.Text.Substring(0, 1);

            //使Logo和文本同步移动
            try
            {
                Rectangle rect = this.Bounds;
                int screenWidth = rect.Width - 30;
                int screenHeight = rect.Height - 40;

                Random ra = new Random();
                int x = ra.Next(0, screenWidth);
                int y = ra.Next(0, screenHeight);
                if (x + label1.Width >= screenWidth)
                {
                    x = screenWidth - label1.Width;
                }
                if (y + label1.Height >= screenHeight || y + pictureBox1.Height >= screenHeight)
                {
                    y = screenHeight - label1.Height;

                }
                if (y - pictureBox1.Height < 0) y = y + pictureBox1.Height;

                label1.Location = new Point(x, y);      //定位label
                pictureBox1.Location = new Point(x + label1.Width / 2 - pictureBox1.Width / 2, y - pictureBox1.Height);     //定位pic

            }
            catch { }

        }

        protected override void WndProc(ref Message m)              //屏蔽Alt + F4关闭
        {
            const int WM_SYSCOMMAND = 0x112;
            const int SC_CLOSE = 0xF060;
            if (m.Msg == WM_SYSCOMMAND && m.WParam == (IntPtr)SC_CLOSE && m.LParam == IntPtr.Zero)
                return;
            base.WndProc(ref m);
        }

        protected override CreateParams CreateParams            //禁用关闭按钮
        {
            get
            {
                int CS_NOCLOSE = 0x200;
                CreateParams parameters = base.CreateParams;
                parameters.ClassStyle |= CS_NOCLOSE;
                return parameters;
            }
        }

        private void Locker_KeyDown(object sender, KeyEventArgs e)               //监视键盘动作
        {
            LockerPsd frmPsd = new LockerPsd();
            frmPsd.StartPosition = FormStartPosition.CenterParent;
            frmPsd.ShowDialog();
            frmPsd.Focus();
        }

        private void Locker_KeyPress(object sender, KeyPressEventArgs e)               //监视键盘动作
        {
            //LockerPsd frmPsd = new LockerPsd();
            //frmPsd.StartPosition = FormStartPosition.CenterParent;
            //frmPsd.ShowDialog();
        }

        private void Locker_MouseMove(object sender, MouseEventArgs e)               //监视鼠标移动动作
        {
            int CurrentX = Control.MousePosition.X;
            int CurrentY = Control.MousePosition.Y;
            
            if (Math.Abs(PreviousX - CurrentX) > 5 || Math.Abs(PreviousY - CurrentY) > 5)
            {
                LockerPsd frmPsd = new LockerPsd();
                frmPsd.StartPosition = FormStartPosition.CenterParent;
                frmPsd.ShowDialog();
                frmPsd.Focus();

                PreviousX = Control.MousePosition.X;               //重新获取上一次的x，y坐标。
                PreviousY = Control.MousePosition.Y;
            }
        }

        private void Locker_MouseClick(object sender, MouseEventArgs e)               //监视鼠标点击动作
        {
            LockerPsd frmPsd = new LockerPsd();
            frmPsd.StartPosition = FormStartPosition.CenterParent;
            frmPsd.ShowDialog();
            frmPsd.Focus();
        }

        private void Locker_SizeChanged(object sender, EventArgs e)             //窗体最小化的时候要停止timer
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.timer1.Enabled = false;
                //PublicClass.AuroraMain.WindowState = FormWindowState.Minimized;
            }
            else 
            { 
                timer1.Enabled = true;
            }
        }

        private void Locker_LocationChanged(object sender, EventArgs e)
        {
            //PublicClass.AuroraMain.Location = new Point(this.Location.X, this.Location.Y);
        }

        private void Locker_Activated(object sender, EventArgs e)               //旧坐标需要在此获取
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                this.Text = "Aurora智能数据锁 ● 正在积极的保护您的数据";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                this.Text = "Aurora智慧資料鎖 ● 正在積極的保護您的資料";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                this.Text = "Aurora Intelligent Data Locker ● is Protecting Your Data Positively";
            }

            PreviousX = Control.MousePosition.X;
            PreviousY = Control.MousePosition.Y;

            RegistryKey MyReg1, RegPassword;
            MyReg1 = Registry.CurrentUser;
            RegPassword = MyReg1.CreateSubKey("Software\\Aurora\\Locker");
            try
            {
                label1.Text = RegPassword.GetValue("ShowText").ToString();
            }
            catch { }

        }



    }
}
