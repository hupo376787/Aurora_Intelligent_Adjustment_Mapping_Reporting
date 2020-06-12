using System;
using System.Windows.Forms;

namespace Adjustment
{
    public partial class ProgressBar : Form
    {
        public ProgressBar()
        {
            InitializeComponent();
        }

        private void ProgressBar_Load(object sender, EventArgs e)
        {
            Delay(500); 
            //伪代码，显示进度条
             for (int i = 0; i <= 10000; i++)
            {
                ProgressBar1.Value = i / 100;
                //Thread.Sleep(10);     //使用Thread容易造成进度条不平滑前进。
                //Delay(5);
            }
        }

        private void Delay(int mm)        //delay延时函数
        {
            DateTime current = DateTime.Now;

            while (current.AddMilliseconds(mm) > DateTime.Now)
            {
                Application.DoEvents();
            }
            return;
        }

    }
}
