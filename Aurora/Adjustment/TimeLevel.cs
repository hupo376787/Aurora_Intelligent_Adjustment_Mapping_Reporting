using System;
using System.Windows.Forms;

namespace Adjustment
{
    public partial class TimeLevel : Form
    {
        public TimeLevel()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void TimeLevel_Load(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.enableContextMenu = false;
            axWindowsMediaPlayer1.settings.volume = 100;
            Random rd = new Random();
            int n = rd.Next(1, 7);              //随机数不可以取上界值
            if (n == 1)
            {
                axWindowsMediaPlayer1.URL = Application.StartupPath + "\\Music\\Music01.mp3";
            }
            if (n == 2)
            {
                axWindowsMediaPlayer1.URL = Application.StartupPath + "\\Music\\Music02.mp3";
            }
            if (n == 3)
            {
                axWindowsMediaPlayer1.URL = Application.StartupPath + "\\Music\\Music03.mp3";
            }
            if (n == 4)
            {
                axWindowsMediaPlayer1.URL = Application.StartupPath + "\\Music\\Music04.mp3";
            }
            if (n == 5)
            {
                axWindowsMediaPlayer1.URL = Application.StartupPath + "\\Music\\Music05.mp3";
            }
            if (n == 6)
            {
                axWindowsMediaPlayer1.URL = Application.StartupPath + "\\Music\\Music06.mp3";
            }
            axWindowsMediaPlayer1.Ctlcontrols.play();             //默认打开不播放音乐
            pictureBox4.Enabled = false;                //不播放音乐，gif停止
            axWindowsMediaPlayer1.settings.setMode("shuffle", true);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPaused)                 //检测播放器状态，停用/播放gif动画。
            {
                pictureBox4.Enabled = false;
            } 
            else
            {
                pictureBox4.Enabled = true;
            }

            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsStopped)                 //检测播放器状态，如果停止则随机下一首。
            {
                pictureBox1_Click(sender, e);
            } 

            //AuroraMain frmMain = new AuroraMain();
            //this.label1.Text = "Time：" + frmMain.stw.Elapsed.Hours.ToString() + "h " + frmMain.stw.Elapsed.Minutes.ToString() + "m " + frmMain.stw.Elapsed.Seconds.ToString() + "s";

            //if (frmMain.stw.Elapsed.Minutes < 5)
            //{
            //    pictureBox1.Image = Properties.Resources.Lvl_Star_16;
            //}
            //if (frmMain.stw.Elapsed.Minutes >= 5 && frmMain.stw.Elapsed.Minutes < 10)
            //{
            //    pictureBox1.Image = Properties.Resources.Lvl_Star_16;
            //    pictureBox2.Image = Properties.Resources.Lvl_Star_16;
            //}
            //if (frmMain.stw.Elapsed.Minutes >= 10 && frmMain.stw.Elapsed.Minutes < 15)
            //{
            //    pictureBox1.Image = Properties.Resources.Lvl_Star_16;
            //    pictureBox2.Image = Properties.Resources.Lvl_Star_16;
            //    pictureBox3.Image = Properties.Resources.Lvl_Star_16;
            //}
            //if (frmMain.stw.Elapsed.Minutes >= 15 && frmMain.stw.Elapsed.Minutes < 20)
            //{
            //    pictureBox1.Image = Properties.Resources.Lvl_Moon_16;
            //}
            //if (frmMain.stw.Elapsed.Minutes >= 20 && frmMain.stw.Elapsed.Minutes < 30)
            //{
            //    pictureBox1.Image = Properties.Resources.Lvl_Moon_16;
            //    pictureBox2.Image = Properties.Resources.Lvl_Star_16;
            //}
            //if (frmMain.stw.Elapsed.Minutes >= 30 && frmMain.stw.Elapsed.Minutes < 40)
            //{
            //    pictureBox1.Image = Properties.Resources.Lvl_Moon_16;
            //    pictureBox2.Image = Properties.Resources.Lvl_Star_16;
            //    pictureBox3.Image = Properties.Resources.Lvl_Star_16;
            //}
            //if (frmMain.stw.Elapsed.Minutes >= 40 && frmMain.stw.Elapsed.Minutes < 60)
            //{
            //    pictureBox1.Image = Properties.Resources.Lvl_Moon_16;
            //    pictureBox2.Image = Properties.Resources.Lvl_Moon_16;
            //}
            //if (frmMain.stw.Elapsed.Minutes >= 60 && frmMain.stw.Elapsed.Minutes < 80)
            //{
            //    pictureBox1.Image = Properties.Resources.Lvl_Moon_16;
            //    pictureBox2.Image = Properties.Resources.Lvl_Moon_16;
            //    pictureBox3.Image = Properties.Resources.Lvl_Star_16;
            //}
            //if (frmMain.stw.Elapsed.Minutes >= 80 && frmMain.stw.Elapsed.Minutes < 100)
            //{
            //    pictureBox1.Image = Properties.Resources.Lvl_Moon_16;
            //    pictureBox2.Image = Properties.Resources.Lvl_Moon_16;
            //    pictureBox3.Image = Properties.Resources.Lvl_Moon_16;
            //}
            //if (frmMain.stw.Elapsed.Minutes >= 100 && frmMain.stw.Elapsed.Minutes < 150)
            //{
            //    pictureBox1.Image = Properties.Resources.Lvl_Sun_16;
            //}
            //if (frmMain.stw.Elapsed.Minutes >= 150 && frmMain.stw.Elapsed.Minutes < 200)
            //{
            //    pictureBox1.Image = Properties.Resources.Lvl_Sun_16;
            //    pictureBox2.Image = Properties.Resources.Lvl_Star_16;
            //}
            //if (frmMain.stw.Elapsed.Minutes >= 200 && frmMain.stw.Elapsed.Minutes < 250)
            //{
            //    pictureBox1.Image = Properties.Resources.Lvl_Sun_16;
            //    pictureBox2.Image = Properties.Resources.Lvl_Star_16;
            //    pictureBox3.Image = Properties.Resources.Lvl_Star_16;
            //}
            //if (frmMain.stw.Elapsed.Minutes >= 250 && frmMain.stw.Elapsed.Minutes < 300)
            //{
            //    pictureBox1.Image = Properties.Resources.Lvl_Sun_16;
            //    pictureBox2.Image = Properties.Resources.Lvl_Moon_16;
            //}
            //if (frmMain.stw.Elapsed.Minutes >= 300 && frmMain.stw.Elapsed.Minutes < 350)
            //{
            //    pictureBox1.Image = Properties.Resources.Lvl_Sun_16;
            //    pictureBox2.Image = Properties.Resources.Lvl_Moon_16;
            //    pictureBox3.Image = Properties.Resources.Lvl_Star_16;
            //}
            //if (frmMain.stw.Elapsed.Minutes >= 350 && frmMain.stw.Elapsed.Minutes < 400)
            //{
            //    pictureBox1.Image = Properties.Resources.Lvl_Sun_16;
            //    pictureBox2.Image = Properties.Resources.Lvl_Moon_16;
            //    pictureBox3.Image = Properties.Resources.Lvl_Moon_16;
            //}
            //if (frmMain.stw.Elapsed.Minutes >= 400 && frmMain.stw.Elapsed.Minutes < 500)
            //{
            //    pictureBox1.Image = Properties.Resources.Lvl_Sun_16;
            //    pictureBox2.Image = Properties.Resources.Lvl_Sun_16;
            //}
            //if (frmMain.stw.Elapsed.Minutes >= 500 && frmMain.stw.Elapsed.Minutes < 600)
            //{
            //    pictureBox1.Image = Properties.Resources.Lvl_Sun_16;
            //    pictureBox2.Image = Properties.Resources.Lvl_Sun_16;
            //    pictureBox3.Image = Properties.Resources.Lvl_Star_16;
            //}
            //if (frmMain.stw.Elapsed.Minutes >= 600 && frmMain.stw.Elapsed.Minutes < 1000)
            //{
            //    pictureBox1.Image = Properties.Resources.Lvl_Sun_16;
            //    pictureBox2.Image = Properties.Resources.Lvl_Sun_16;
            //    pictureBox3.Image = Properties.Resources.Lvl_Moon_16;
            //}
            //if (frmMain.stw.Elapsed.Minutes >= 1000)             //这尼玛太难了
            //{
            //    pictureBox1.Image = Properties.Resources.Lvl_Sun_16;
            //    pictureBox2.Image = Properties.Resources.Lvl_Sun_16;
            //    pictureBox3.Image = Properties.Resources.Lvl_Sun_16;
            //}
        }

        private void TimeLevel_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Stop();
            axWindowsMediaPlayer1.Ctlcontrols.stop();
        }

        private void pictureBox1_Click(object sender, EventArgs e)              //点击第一个等级图标，换歌曲
        {
            Random rd = new Random();
            int n = rd.Next(1, 7);              //随机数不可以取上界值
            if (n == 1)
            {
                axWindowsMediaPlayer1.URL = Application.StartupPath + "\\Music\\Music01.mp3";
            }
            if (n == 2)
            {
                axWindowsMediaPlayer1.URL = Application.StartupPath + "\\Music\\Music02.mp3";
            }
            if (n == 3)
            {
                axWindowsMediaPlayer1.URL = Application.StartupPath + "\\Music\\Music03.mp3";
            }
            if (n == 4)
            {
                axWindowsMediaPlayer1.URL = Application.StartupPath + "\\Music\\Music04.mp3";
            }
            if (n == 5)
            {
                axWindowsMediaPlayer1.URL = Application.StartupPath + "\\Music\\Music05.mp3";
            }
            if (n == 6)
            {
                axWindowsMediaPlayer1.URL = Application.StartupPath + "\\Music\\Music06.mp3";
            }
            axWindowsMediaPlayer1.Ctlcontrols.play();
            axWindowsMediaPlayer1.settings.volume = 100;
            axWindowsMediaPlayer1.settings.setMode("shuffle", true);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            pictureBox1_Click(sender, e);
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            pictureBox1_Click(sender, e);
        }
    }
}
