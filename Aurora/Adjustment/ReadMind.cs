using System;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Adjustment
{
    public partial class ReadMind : Form
    {
        public ReadMind()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                if (button1.Text == "开始读心")
                {
                    pictureBox101.Visible = true;
                    button1.Text = "不相信？再来一次？";
                }
                else
                {
                    pictureBox101.Visible = false;
                    button1.Text = "开始读心";
                    onLoad();
                }
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                if (button1.Text == "開始讀心")
                {
                    pictureBox101.Visible = true;
                    button1.Text = "不相信？再來一次？";
                }
                else
                {
                    pictureBox101.Visible = false;
                    button1.Text = "開始讀心";
                    onLoad();
                }
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")   
            {
                if (button1.Text == "Start Read Mind")
                {
                    pictureBox101.Visible = true;
                    button1.Text = "Unbelievable? Try Again?";
                }
                else
                {
                    pictureBox101.Visible = false;
                    button1.Text = "Start Read Mind";
                    onLoad();
                }
            }
            
        }

        private void ReadMind_Load(object sender, EventArgs e)
        {
            pictureBox101.Visible = false;
            onLoad();
        }

        private void onLoad()           //洗牌
        {
            Random rd = new Random();
            int fixIndex = -1;
            string controlName = "";
            for (int i = 1; i <= 100; i++)
            {
                controlName = "pictureBox" + i.ToString();
                PictureBox p = (PictureBox)this.groupBox1.Controls.Find(controlName, false)[0];
                if (i % 9 != 0)
                {
                    p.Image = imageList1.Images[rd.Next(0, 40)];
                }
                else
                {
                    if (fixIndex == -1)
                    {
                        fixIndex = rd.Next(0, 40);
                    }
                    p.Image = imageList1.Images[fixIndex];
                    pictureBox101.Image = imageList1.Images[fixIndex];
                }
            }
        }

    }
}
