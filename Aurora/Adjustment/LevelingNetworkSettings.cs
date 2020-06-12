using System;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Adjustment
{
    public partial class LevelingNetworkSettings : Form
    {
        public LevelingNetworkSettings()
        {
            InitializeComponent();
        }

        private void LevelingNetworkSettings_Load(object sender, EventArgs e)
        {
            AuroraMain frm1 = (AuroraMain)this.Owner;
            frm1.LevelingNetwork_textBox.Clear();
            textBox1.Text = frm1.nLevelingObsNum.ToString();        //将列表的统计行数总动填到观测值总数中
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                textBox4.Text = "输入已知点点号和已知点高程，用','分隔，回车换行(如 PT1,100.2315)。";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                textBox4.Text = "輸入已知點點號和已知點高程，用','分隔，回車換行(如 PT1,100.2315)。";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                textBox4.Text = "Input known point name and height, use comma to separate, use enter to change line.(e.g. PT1,100.2315).";
            }
        }

        private void textBox4_Click(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                if (textBox4.Text == "输入已知点点号和已知点高程，用空格分隔，回车换行(如 PT1 100.2315)。")
                {
                    textBox4.Text = "";
                }
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                if (textBox4.Text == "輸入已知點點號和已知點高程，用空格分隔，回車換行(如 PT1 100.2315)。")
                {
                    textBox4.Text = "";
                }
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                if (textBox4.Text == "Input known point name and height, use comma to separate, use enter to change line.(e.g. PT1 100.2315).")
                {
                    textBox4.Text = "";
                }
            }
        }

        private void textBox4_Leave(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                if (textBox4.Text == "")
                {
                    textBox4.Text = "输入已知点点号和已知点高程，用空格分隔，回车换行(如 PT1 100.2315)。";
                }
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                if (textBox4.Text == "")
                {
                    textBox4.Text = "輸入已知點點號和已知點高程，用空格分隔，回車換行(如 PT1 100.2315)。";
                }
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                if (textBox4.Text == "")
                {
                    textBox4.Text = "Input known point name and height, use comma to separate, use enter to change line.(e.g. PT1 100.2315).";
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "" || textBox2.Text == "" || textBox3.Text == "" || textBox4.Text.Trim() == "输入已知点点号和已知点高程，用空格分隔，回车换行(如 PT1 100.2315)。" || textBox4.Text.Trim() == "輸入已知點點號和已知點高程，用空格分隔，回車換行(如 PT1 100.2315)。" || textBox4.Text.Trim() == "Input known point name and height, use comma to separate, use enter to change line.(e.g. PT1 100.2315)." || textBox4.Text.Trim() == "")
            {
                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                {
                    MessageBox.Show("忘记输入哪个参数了？", "Aurora智能提示");
                }
                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                {
                    MessageBox.Show("忘記輸入哪個參數了？", "Aurora智慧提示");
                }
                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                {
                    MessageBox.Show("Forget a parameter?", "Aurora Intelligent Tips");
                }
                return;
            }
            else
            {
                string strHead = "";
                AuroraMain frm1 = (AuroraMain)this.Owner;
                strHead = textBox1.Text.Trim() + "," + textBox2.Text.Trim() + "," + textBox3.Text.Trim() + "\r\n" + textBox4.Text.Trim() + "\r\n";
                frm1.LevelingNetwork_textBox.Text = strHead + frm1.strLevelingNetworkData;
                frm1.m_Onumber = Convert.ToInt16(textBox1.Text.Trim());
                frm1.m_Tnumber = Convert.ToInt16(textBox2.Text.Trim());
                frm1.m_Knumber = Convert.ToInt16(textBox3.Text.Trim());
                this.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
