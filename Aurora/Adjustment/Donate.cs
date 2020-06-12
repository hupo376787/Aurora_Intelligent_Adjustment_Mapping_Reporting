using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace Adjustment
{
    public partial class Donate : Form
    {
        public Donate()
        {
            InitializeComponent();
        }
        
        //动画窗体调用
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern bool AnimateWindow(IntPtr hwnd, int dwTime, int dwFlags);
        const int AW_HOR_POSITIVE = 0x0001;
        const int AW_HOR_NEGATIVE = 0x0002;
        const int AW_VER_POSITIVE = 0x0004;
        const int AW_VER_NEGATIVE = 0x0008;
        const int AW_CENTER = 0x0010;
        const int AW_HIDE = 0x10000;
        const int AW_ACTIVATE = 0x20000;
        const int AW_SLIDE = 0x40000;
        const int AW_BLEND = 0x80000;

        private void Donate_Load(object sender, EventArgs e)
        {
            AnimateWindow(this.Handle, 2500, AW_SLIDE | AW_VER_POSITIVE);
        }

        private void Donate_FormClosing(object sender, FormClosingEventArgs e)
        {
            AnimateWindow(this.Handle, 2500, AW_SLIDE | AW_HIDE | AW_VER_NEGATIVE);
        }

        private void button1_Click(object sender, EventArgs e)              //Donate
        {
            System.Diagnostics.Process.Start("https://me.alipay.com/hupo376787");
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)              //Leave
        {
            this.Close();
        }

        protected override void WndProc(ref Message m)                //移动窗体
        {
            base.WndProc(ref m);
            if (m.Msg == 0x84)
            {
                switch (m.Result.ToInt32())
                {
                    case 1:
                        m.Result = new IntPtr(2);
                        break;
                }
            }
            //if (m.Msg == 0x0201)
            //{
            //    m.Msg = 0x00A1;//更改消息为非客户区按下鼠标
            //    m.LParam = IntPtr.Zero;
            //    m.WParam = new IntPtr(2);//鼠标放在标题栏内
            //}
            //base.WndProc(ref m);
        }


        //鼠标拖动相关变量
        Point oldPoint = new Point(0, 0);
        bool mouseDown = false;

        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                oldPoint = e.Location;
                mouseDown = true;
            }
        }

        private void label1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Left += (e.X - oldPoint.X);
                this.Top += (e.Y - oldPoint.Y);
            }
        }

        private void label1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void button1_MouseEnter(object sender, EventArgs e)
        {
            button1.BackgroundImage = Properties.Resources.ButtonBKG1;
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
            button1.BackgroundImage = Properties.Resources.ButtonBKG;
        }

        private void button2_MouseEnter(object sender, EventArgs e)
        {
            button2.BackgroundImage = Properties.Resources.ButtonBKG1;
        }

        private void button2_MouseLeave(object sender, EventArgs e)
        {
            button2.BackgroundImage = Properties.Resources.ButtonBKG;
        }

        private void linkLabel1_MouseEnter(object sender, EventArgs e)              //科学
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            //p.AutoPopDelay = 8;
            p.IsBalloon = true;
            //p.UseAnimation = true;
            //p.UseFading = true;
            //p.ToolTipTitle = "000";
            string Tips = "";
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                Tips = "地理信息科学(GIS)专业是地球科学、测绘科学与计算机科学等交叉形成的" + "\r\n"
                        + "新兴技术学科。该专业以地表与近地表的自然、社会、经济、文化等现象分" + "\r\n"
                        +"布的空间信息为研究对象，研究利用计算机、卫星定位、遥感等现代技术进" + "\r\n"
                        +"行空间信息的获取、管理、表达、处理与应用等基本理论和技术方法。";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                Tips = "地理資訊科學(GIS)專業是地球科學、測繪科學與電腦科學等交叉形成的" + "\r\n"
                        + "新興技術學科。該專業以地表與近地表的自然、社會、經濟、文化等現象分" + "\r\n"
                        +"布的空間資訊為研究物件，研究利用電腦、衛星定位、遙感等現代技術進" + "\r\n"
                        +"行空間資訊的獲取、管理、表達、處理與應用等基本理論和技術方法。";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")  
            {
                Tips = "Geographic Information Science, google or wiki it!";
            }
            p.SetToolTip(this.linkLabel1, Tips);
        }







    }
}
