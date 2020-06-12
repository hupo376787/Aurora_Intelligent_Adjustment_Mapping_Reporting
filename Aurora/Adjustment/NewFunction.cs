using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Adjustment
{
    public partial class NewFunction : Form
    {
        public NewFunction()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox1.Image = Properties.Resources.CloseDown;
        }

        private void pictureBox1_MouseHover(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.CloseHover;
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.Close;
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



    }
}
