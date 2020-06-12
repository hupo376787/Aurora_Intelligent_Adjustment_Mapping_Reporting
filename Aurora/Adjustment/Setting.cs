using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using System.Threading;

namespace Adjustment
{
    public partial class Setting : Form
    {
        public Setting()
        {
            InitializeComponent();
        }

        Color MyColor = Color.GreenYellow;

        private void button1_Click(object sender, EventArgs e)              //确定
        {
            string strProjectName = textBox1.Text;
            string strCalculator = textBox2.Text;
            string strChecker = textBox3.Text;
            string strMyGrade = textBox4.Text;

            AuroraMain frm1 = (AuroraMain)this.Owner;       //往主窗体传送参数.
            frm1.MyCellColor = MyColor;
            frm1.strProjectName = strProjectName;
            frm1.strCalculator = strCalculator;
            frm1.strChecker = strChecker;
            frm1.strMyGrade = strMyGrade;

            RegistryKey MyReg, RegProjectInfo;//声明注册表对象
            MyReg = Registry.CurrentUser;//获取当前用户注册表项
            RegProjectInfo = MyReg.CreateSubKey("Software\\Aurora\\ProjectInfo");//在注册表项中创建子项
            try
            {
                RegProjectInfo.SetValue("ProjectName", strProjectName);             //把信息放进注册表，供主窗体调用
                RegProjectInfo.SetValue("Calculator", strCalculator);
                RegProjectInfo.SetValue("Checker", strChecker);
                RegProjectInfo.SetValue("MyGrade", strMyGrade);
            }
            catch { }

            if (checkBox1.Checked == true)
            {
                frm1.nSCS = 1;               //勾选数据保护，将全局变量设置为1.主窗体直接判断nSCS的值即可。
                if (textBox5.Text != "")
                {
                    if (textBox5.Text != "0")
                    {
                        if (Math.Abs(Convert.ToDouble(textBox5.Text)) < 1)
                        {
                            MessageBox.Show("请输入大于1的整数时间。", "Aurora智能提示");
                            return;
                        }

                        frm1.nTimer = Convert.ToInt16(Math.Round(Math.Abs(Convert.ToDouble(textBox5.Text))));        //防止哪个~~~2B~~~输入负数，所以取绝对值。

                        RegistryKey MyReg1, RegLocker;//声明注册表对象
                        MyReg1 = Registry.CurrentUser;//获取当前用户注册表项
                        RegLocker = MyReg1.CreateSubKey("Software\\Aurora\\Locker");//在注册表项中创建子项
                        try
                        {
                            RegLocker.SetValue("Enabled", frm1.nSCS.ToString());             //把信息放进注册表，供主窗体调用
                            RegLocker.SetValue("Timer", frm1.nTimer.ToString());
                        }
                        catch { }
                    }
                    else
                    {
                        MessageBox.Show("时间输入0无效。", "Aurora智能提示");
                        return;
                    }
                    
                }
                else
                {
                    MessageBox.Show("您已开启智能保护锁，但是忘记了输入启动时间。","Aurora智能提示");
                    return;
                }
            }
            else
            {
                frm1.nSCS = 0;

                RegistryKey MyReg1, RegLocker;//声明注册表对象
                MyReg1 = Registry.CurrentUser;//获取当前用户注册表项
                RegLocker = MyReg1.CreateSubKey("Software\\Aurora\\Locker");//在注册表项中创建子项
                try
                {
                    RegLocker.SetValue("Enabled", frm1.nSCS.ToString());             //把信息放进注册表，供主窗体调用
                    RegLocker.SetValue("Timer", frm1.nTimer.ToString());
                }
                catch { }
            }

            if (checkBox3.Checked == true)              //显示桌面提示
            {
                RegistryKey MyReg1, RegReminder;
                MyReg1 = Registry.CurrentUser;
                RegReminder = MyReg1.CreateSubKey("Software\\Aurora\\Reminder");
                try
                {
                    RegReminder.SetValue("ShowDesktopReminder", "YES");
                }
                catch { }
            }
            else
            {
                RegistryKey MyReg1, RegReminder;
                MyReg1 = Registry.CurrentUser;
                RegReminder = MyReg1.CreateSubKey("Software\\Aurora\\Reminder");
                try
                {
                    RegReminder.SetValue("ShowDesktopReminder", "NO");
                }
                catch { }
            }

            if (checkBox2.Checked == true)              //退出提示
            {
                RegistryKey MyReg1, RegReminder;
                MyReg1 = Registry.CurrentUser;
                RegReminder = MyReg1.CreateSubKey("Software\\Aurora\\Reminder");
                try
                {
                    RegReminder.SetValue("ExitReminder", "YES");
                }
                catch { }
             }
            else
            {
                RegistryKey MyReg1, RegReminder;
                MyReg1 = Registry.CurrentUser;
                RegReminder = MyReg1.CreateSubKey("Software\\Aurora\\Reminder");
                try
                {
                    RegReminder.SetValue("ExitReminder", "NO");
                }
                catch { }
            }

            if (checkBox4.Checked == true)              //全屏运行提示
            {
                RegistryKey MyReg1, RegFullScreen;
                MyReg1 = Registry.CurrentUser;
                RegFullScreen = MyReg1.CreateSubKey("Software\\Aurora\\Locker");
                try
                {
                    RegFullScreen.SetValue("FullScreen", "YES");
                }
                catch { }
            }
            else
            {
                RegistryKey MyReg1, RegFullScreen;
                MyReg1 = Registry.CurrentUser;
                RegFullScreen = MyReg1.CreateSubKey("Software\\Aurora\\Locker");
                try
                {
                    RegFullScreen.SetValue("FullScreen", "NO");
                }
                catch { }
            }

            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)      //Choose Color
        {
            ColorDialog MyDlg = new ColorDialog();
            
            if (MyDlg.ShowDialog() == DialogResult.OK)
            {
                RegistryKey MyReg, RegColor;//声明注册表对象
                MyReg = Registry.CurrentUser;//获取当前用户注册表项
                RegColor = MyReg.CreateSubKey("Software\\Aurora\\Color");//在注册表项中创建子项
                try
                {
                    MyColor = MyDlg.Color;
                    pictureBox1.BackColor = MyColor;
                    RegColor.SetValue("CellColor", MyColor.ToArgb());             //把信息放进注册表，供主窗体调用
                }
                catch { }
            }
        }

        private void button4_Click(object sender, EventArgs e)      //Set Default Color
        {
            AuroraMain frm1 = (AuroraMain)this.Owner;
            RegistryKey MyReg, RegColor;//声明注册表对象
            MyReg = Registry.CurrentUser;//获取当前用户注册表项
            RegColor = MyReg.CreateSubKey("Software\\Aurora\\Color");//在注册表项中创建子项
            try
            {
                MyColor = Color.GreenYellow;
                frm1.MyCellColor = MyColor;
                pictureBox1.BackColor = MyColor;
                RegColor.SetValue("CellColor", MyColor.ToArgb());             //把信息放进注册表，供主窗体调用
            }
            catch { }
        }

        private void button6_Click(object sender, EventArgs e)      //Choose Picture
        {
            AuroraMain frm1 = (AuroraMain)this.Owner;

            string fName = "";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JPG 文件交换格式(*.jpg;*.jpeg;*.jfif;*.jpe)|*.jpg;*.jpeg;*.jfif;*.jpe|可移植网络图形(*.png)|*.png|Windows 位图(*.bmp;*.dib;*.rle)|*.bmp;*.dib;*.rle|Tag 图像文件格式(*.tif;*.tiff)|*.tif;*.tiff|Windows 图元文件(*.wmf)|*.wmf|内嵌的 PostScript(*.eps)|*.eps|Macintosh PICT(*.pct;*.pict)|*.pct;*.pict|WordPerfect 图形(*.wpg)|*.wpg|所有文件|*.*";
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                RegistryKey MyReg, RegBKLocation;//声明注册表对象
                MyReg = Registry.CurrentUser;//获取当前用户注册表项
                RegBKLocation = MyReg.CreateSubKey("Software\\Aurora\\Color");//在注册表项中创建子项
                try
                {
                    fName = openFileDialog.FileName;
                    //File.Delete(Application.StartupPath + "\\BK.jpg");

                    frm1.BackgroundImage = Image.FromFile(fName);
                    frm1.BackgroundImageLayout = ImageLayout.Tile;
                    frm1.menuStrip1.BackgroundImage = Image.FromFile(fName);
                    frm1.menuStrip1.BackgroundImageLayout = ImageLayout.Tile;
                    frm1.toolStrip1.BackgroundImage = Image.FromFile(fName);
                    frm1.toolStrip1.BackgroundImageLayout = ImageLayout.Tile;
                    frm1.groupBox1.BackgroundImage = Image.FromFile(fName);
                    frm1.groupBox1.BackgroundImageLayout = ImageLayout.Tile;
                    frm1.statusStrip1.BackgroundImage = Image.FromFile(fName);
                    frm1.statusStrip1.BackgroundImageLayout = ImageLayout.Tile;
                    RegBKLocation.SetValue("BKEnabled", "true");

                    File.Copy(fName, Application.StartupPath + "\\BK1.jpg", true);
                }
                catch(Exception ex)
                { MessageBox.Show(ex.ToString()); }
                
            }
            else { return; }
        }

        private void button5_Click(object sender, EventArgs e)      //Set Default
        {
            AuroraMain frm1 = (AuroraMain)this.Owner;
            frm1.BackgroundImage = null;
            frm1.menuStrip1.BackgroundImage = null;
            frm1.toolStrip1.BackgroundImage = null;
            frm1.groupBox1.BackgroundImage = null;
            frm1.statusStrip1.BackgroundImage = null;
            RegistryKey MyReg, RegBKLocation;//声明注册表对象
            MyReg = Registry.CurrentUser;//获取当前用户注册表项
            RegBKLocation = MyReg.CreateSubKey("Software\\Aurora\\Color");//在注册表项中创建子项
            try
            {
                RegBKLocation.SetValue("BKEnabled", "false");
            }
            catch { }
        }

        private void button3_MouseEnter(object sender, EventArgs e)
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                p.SetToolTip(this.button3, "选择一个颜色。");
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                p.SetToolTip(this.button3, "選擇一個顏色。");
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                p.SetToolTip(this.button3, "Choose a color.");
            }
        }

        private void button4_MouseEnter(object sender, EventArgs e)
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                p.SetToolTip(this.button4, "恢复成默认颜色。");
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                p.SetToolTip(this.button4, "恢復成默認顏色。");
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                p.SetToolTip(this.button4, "Reset to default.");
            }
        }

        private void button5_MouseEnter(object sender, EventArgs e)
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                p.SetToolTip(this.button5, "恢复成默认。");
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                p.SetToolTip(this.button5, "恢復成默認。");
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                p.SetToolTip(this.button5, "Reset to default.");
            }
        }

        private void button6_MouseEnter(object sender, EventArgs e)
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;

            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                p.SetToolTip(this.button6, "选择一个背景图片。");
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                p.SetToolTip(this.button6, "選擇一個背景圖片。");
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                p.SetToolTip(this.button6, "Choose a BK picture.");
            }
        }

        private void Setting_Load(object sender, EventArgs e)
        {
            label8.Visible = false;
            label9.Visible = false;
            textBox5.Visible = false;       //默认数据保护锁的时间文本框不可见。
            checkBox4.Visible = false;

            RegistryKey MyReg, RegProjectInfo;//声明注册表对象
            MyReg = Registry.CurrentUser;//获取当前用户注册表项
            try
            {
                RegProjectInfo = MyReg.CreateSubKey("Software\\Aurora\\ProjectInfo");//在注册表项中创建子项
                this.textBox1.Text = (RegProjectInfo.GetValue("ProjectName")).ToString();
                this.textBox2.Text = (RegProjectInfo.GetValue("Calculator")).ToString();
                this.textBox3.Text = (RegProjectInfo.GetValue("Checker")).ToString();
                this.textBox4.Text = (RegProjectInfo.GetValue("MyGrade")).ToString();
            }
            catch { }

            RegistryKey MyReg1, RegLocker;//声明注册表对象
            MyReg1 = Registry.CurrentUser;//获取当前用户注册表项
            try
            {
                RegLocker = MyReg.CreateSubKey("Software\\Aurora\\Locker");//在注册表项中创建子项
                if ((RegLocker.GetValue("Enabled")).ToString() == "1")
                {
                    checkBox1.Checked = true;
                }

                this.textBox5.Text = (RegLocker.GetValue("Timer")).ToString();
            }
            catch { }

            //全屏保护提示
            RegistryKey RegFullScreen;
            try
            {
                RegFullScreen = MyReg.CreateSubKey("Software\\Aurora\\Locker");
                if (RegFullScreen.GetValue("FullScreen").ToString() == "YES")
                {
                    checkBox4.Checked = true;
                }
                else checkBox4.Checked = false;
            }
            catch { }

            //读取单元格颜色
            RegistryKey RegColor;//声明注册表对象
            MyReg = Registry.CurrentUser;//获取当前用户注册表项
            try
            {
                RegColor = MyReg.CreateSubKey("Software\\Aurora\\Color");//在注册表项中创建子项
                pictureBox1.BackColor = ColorTranslator.FromHtml(RegColor.GetValue("CellColor").ToString());//显示注册表的位置
            }
            catch { }

            //显示桌面提示
            RegistryKey RegReminder;
            try
            {
                RegReminder = MyReg.CreateSubKey("Software\\Aurora\\Reminder");
                if (RegReminder.GetValue("ShowDesktopReminder").ToString() == "YES")
                {
                    checkBox3.Checked = true;
                }
                else checkBox3.Checked = false;
            }
            catch { }

            //退出提示
            try
            {
                RegReminder = MyReg.CreateSubKey("Software\\Aurora\\Reminder");
                if (RegReminder.GetValue("ExitReminder").ToString() == "YES")
                {
                    checkBox2.Checked = true;
                }
                else checkBox2.Checked = false;
            }
            catch { }
        }

        private void checkBox1_MouseEnter(object sender, EventArgs e)       //数据锁提示
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            string Tips = "";
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                Tips = "开启数据锁保护功能是指当您的计算机在指定时间内" + "\r\n"
                            + "无操作时，软件自动开启数据锁屏幕保护。防止您的数据泄露。";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                Tips = "開啟資料鎖保護功能是指當您的電腦在指定時間內" + "\r\n"
                            + "無操作時，軟體自動開啟資料鎖螢幕保護裝置。防止您的資料洩露。";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                Tips = "Open data locker means when no operation in specified time" + "\r\n"
                            + "Aurora locker will protect your data from leaking.";
            }

            p.SetToolTip(this.checkBox1, Tips);
        }

        private void checkBox3_MouseEnter(object sender, EventArgs e)       //显示桌面提示
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            string Tips = "";
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                Tips = "此功能将决定在打开软件时，是否先显示桌面然后再打开Aurora。";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                Tips = "此功能將決定在打開軟體時，是否先顯示桌面然後再打開Aurora。";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                Tips = "Whether show desktop when Open Aurora.";
            }

            p.SetToolTip(this.checkBox3, Tips);
        }

        private void checkBox2_MouseEnter(object sender, EventArgs e)       //退出提示
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            string Tips = "";
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                Tips = "此功能将决定在退出软件时，弹出对话框询问是否关闭Aurora。";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                Tips = "此功能將決定在退出軟體時，彈出對話方塊詢問是否關閉Aurora。";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                Tips = "A messagebox pops up to confirm when exit Aurora .";
            }

            p.SetToolTip(this.checkBox2, Tips);
        }

        private void checkBox4_MouseEnter(object sender, EventArgs e)       //全屏提示
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            string Tips = "";
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                Tips = "开启数据保护锁是否以全屏方式运行。";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                Tips = "開啟資料保護鎖是否以全屏方式運行。";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                Tips = "This function determines whether to run data locker full screen or not.";
            }

            p.SetToolTip(this.checkBox4, Tips);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)       //开启数据保护功能的时候，显示关闭相关项
        {
            if (checkBox1.Checked == false)
            {
                label8.Visible = false;
                label9.Visible = false;
                textBox5.Visible = false;
                checkBox4.Visible = false;
            }
            else
            {
                label8.Visible = true;
                label9.Visible = true;
                textBox5.Visible = true;
                checkBox4.Visible = true;
            }
        }



    }
}
