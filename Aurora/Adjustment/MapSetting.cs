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
    public partial class MapSetting : Form
    {
        public MapSetting()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)                //确定
        {
            AuroraMain frm1 = (AuroraMain)this.Owner;
            frm1.nLevel = Convert.ToInt16(this.comboBox1.Text);
            frm1.n = 1;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)                //取消
        {
            AuroraMain frm1 = (AuroraMain)this.Owner;
            //frm1.nLevel = Convert.ToInt16(this.comboBox1.Text);
            frm1.n = 0;
            this.Close();
        }

        private void MapSetting_Load(object sender, EventArgs e)
        {
            comboBox1.Text = "100";             //默认100输出。

            AuroraMain frm1 = (AuroraMain)this.Owner;
            frm1.n = 0;             //设置为零。否则关闭图像设置的时候，仍然显示保存对话框。
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)             //选择不同数值，进度条也随之变化。
        {
            if (comboBox1.Text == "100")
            {
                progressBar1.Value = 100;
            }
            if (comboBox1.Text == "90")
            {
                progressBar1.Value = 90;
            }
            if (comboBox1.Text == "80")
            {
                progressBar1.Value = 80;
            }
            if (comboBox1.Text == "70")
            {
                progressBar1.Value = 70;
            }
            if (comboBox1.Text == "60")
            {
                progressBar1.Value = 60;
            }
            if (comboBox1.Text == "50")
            {
                progressBar1.Value = 50;
            }
            if (comboBox1.Text == "40")
            {
                progressBar1.Value = 40;
            }
            if (comboBox1.Text == "30")
            {
                progressBar1.Value = 30;
            }
            if (comboBox1.Text == "20")
            {
                progressBar1.Value = 20;
            }
            if (comboBox1.Text == "10")
            {
                progressBar1.Value = 10;
            }
        }
    
    }
}
