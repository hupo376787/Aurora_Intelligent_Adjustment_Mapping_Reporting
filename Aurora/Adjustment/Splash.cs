using System;
using System.Windows.Forms;

namespace Adjustment
{
    public partial class Splash : Form
    {
        public Splash()
        {
            InitializeComponent();
        }

        private void Splash_Load(object sender, EventArgs e)
        {
            this.Timer1.Enabled = true;     //激活定时器
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            this.Close();       //关闭启动窗体
        }

        private void Splash_FormClosed(object sender, FormClosedEventArgs e)
        {
            Timer1.Enabled = false;
        }
        
        protected override void WndProc(ref Message m)                //移动窗体
        {
	        base.WndProc(ref m);
            if(m.Msg == 0x84)
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


    }
}
