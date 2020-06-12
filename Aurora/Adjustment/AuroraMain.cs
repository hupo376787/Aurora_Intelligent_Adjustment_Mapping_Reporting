using System;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Collections;
using System.Threading;
using System.Resources;
using System.Configuration;
using System.Linq;

namespace Adjustment
{
    public partial class AuroraMain : Form
    {
        //#region 窗体边框阴影效果变量申明
        //const int CS_DropSHADOW = 0x20000;
        //const int GCL_STYLE = (-26);
        ////声明Win32 API
        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //public static extern int SetClassLong(IntPtr hwnd, int nIndex, int dwNewLong);
        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //public static extern int GetClassLong(IntPtr hwnd, int nIndex);
        //#endregion

        public AuroraMain()
        {
            InitializeComponent();
            //API函数加载，实现窗体边框阴影效果
            //SetClassLong(this.Handle, GCL_STYLE, GetClassLong(this.Handle, GCL_STYLE) | CS_DropSHADOW); 
        }

        //Log4net
        //本日志仅记录比较容易出错的按钮日志。
        //目前日志记录：导入标准文件/平差计算/绘图/生成报表/打开关闭Aurora
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region 启动数据保护锁
        //public int nScreenSaver;        //系统屏幕保护原始值.              //先决定暂时不屏蔽系统屏幕保护。系统的屏幕保护和本软件的数据锁无冲突。
        public int nSCS = 0;                //全局变量nSCS，0未开启数据保护，1开启数据保护。要从注册表读取。
        public int nTimer = 0;              //软件数据保护锁的启动时间.

        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public uint dwTime;
        }
        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        static long GetLastInputTime()
        {
            LASTINPUTINFO vLastInputInfo = new LASTINPUTINFO();
            vLastInputInfo.cbSize = Marshal.SizeOf(vLastInputInfo);
            if (!GetLastInputInfo(ref vLastInputInfo)) return 0;
            return Environment.TickCount - (long)vLastInputInfo.dwTime;
        }

        private void timer1_Tick(object sender, EventArgs e)        //窗体加载时就是true，并且间隔设置为1000.              //启动数据保护锁
        {
            //Text = string.Format("用户已经{0}秒没有路过了", GetLastInputTime() / 1000);
            if (nSCS == 1)      //nSCS == 1表示开启了数据保护锁。
            {
                if (nTimer != 0 && nTimer * 60 == GetLastInputTime() / 1000)       //nTimer单位是分，GetLastInputTime()单位是毫秒。这样就可以实现精确的秒！
                {
                    //MessageBox.Show("打雷啦，下雨收衣服啊!");
                    try
                    {
                        PublicClass.AuroraMain.Hide();
                        if (PublicClass.MyCmd != null)
                        {
                            PublicClass.MyCmd.Hide();
                        }
                        PublicClass.Locker.Show();

                    }
                    catch { }
                    
                }
            }
        }
        #endregion

        private Stopwatch stw = new Stopwatch();            //统计运行时间
        public int nTotalTime = 0;

        private void AuroraMain_Load(object sender, EventArgs e)
        {
            RegistryKey MyReg,RegLanguage;
            MyReg = Registry.CurrentUser;
            try
            {
                RegLanguage = MyReg.CreateSubKey("Software\\Aurora\\Language");
                string strLanguage = RegLanguage.GetValue("Language").ToString();

                if (strLanguage == "zh-CN")
                {
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-CN");
                }
                else if (strLanguage == "zh-Hant")
                {
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-Hant");
                }
                else        //en
                {
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en");
                }
            }
            catch { }

            //Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("En");//手动设置语言。
            //System.Windows.Forms.Cursor.Show(); //显示鼠标
            stw.Start();                //开始计时
            log.Info(DateTime.Now.ToString() + " Open Aurora");             //打开软件，写入一条新log

            toolStrip1.Visible = true;
            toolStrip2.Visible = false;
            toolStrip3.Visible = false;

            //Splash MySplashForm = new Splash();     //加载启动画面
            //MySplashForm.ShowDialog();

            //读取以前的的运行时间
            RegistryKey RegTotalTime;//声明注册表对象
            MyReg = Registry.CurrentUser;//获取当前用户注册表项
            try
            {
                RegTotalTime = MyReg.CreateSubKey("Software\\Aurora\\TotalTime");//在注册表项中创建子项
                nTotalTime = Convert.ToInt32(RegTotalTime.GetValue("TotalTime"));
            }
            catch { }

            //获取GUID信息
            //MessageBox.Show(Guid.NewGuid().ToString("B"));
            //GUID结果
            //{d46b7c02-b796-4aaa-9d4f-2188cf2dba30}
            //大写GUID结果
            //{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}

            //读取注册信息
            //软件在启动时同时比较注册表里面两个位置的数值，若一致则继续。
            RegistryKey RegGUIDFlag, RegFlag, RegValidGUIDDays, RegValidDays, RegStartGUIDDate, RegStartDate, RegSuccess;//声明注册表对象
            try
            {
                RegGUIDFlag = MyReg.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");//在注册表项中创建子项
                RegFlag = MyReg.CreateSubKey("Software\\Aurora");

                RegValidGUIDDays = MyReg.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                RegValidDays = MyReg.CreateSubKey("Software\\Aurora");

                RegStartGUIDDate = MyReg.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                RegStartDate = MyReg.CreateSubKey("Software\\Aurora");

                RegSuccess = MyReg.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                RegSuccess.SetValue("RegSuccess", "false");

                //首先判断注册表两处是否一致，否则退出。防止人为修改。
                if (RegFlag.GetValue("nRegFlag").ToString() != RegGUIDFlag.GetValue("nRegGUIDFlag").ToString() || Convert.ToInt16(RegValidGUIDDays.GetValue("nValidGUIDDays")) !=
                    Convert.ToInt16(RegValidDays.GetValue("nValidDays")) || RegStartGUIDDate.GetValue("StartGUIDDate").ToString() != RegStartDate.GetValue("StartDate").ToString())
                {
                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                    {
                        AutoClosingMessageBox.Show("Aurora侦测到软件注册信息已被破坏或修改，请尝试重新安装Aurora。" + "\r\n" + "\r\n"
                                                   + "程序即将在 5s 后自动退出。。。。。。", "Aurora智能提示", 5000);
                    }
                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                    {
                        AutoClosingMessageBox.Show("Aurora偵測到軟體註冊資訊已被破壞或修改，請嘗試重新安裝Aurora。" + "\r\n" + "\r\n"
                                                   + "程式即將在 5s 後自動退出。。。。。。", "Aurora智慧提示", 5000);
                    }
                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                    {
                        AutoClosingMessageBox.Show("Aurora has detected the registration information has been destroyed or modified" + "\r\n" + "\r\n"
                                                    + "Please try to reinstall Aurora. This application will exit in 5s...", "Aurora Intelligent Tips", 5000);
                    }

                    RegistryKey MyReg1, RegReminder;
                    MyReg1 = Registry.CurrentUser;
                    RegReminder = MyReg1.CreateSubKey("Software\\Aurora\\Reminder");
                    try
                    {
                        RegReminder.SetValue("ExitReminder", "NO"); //此段控制注册信息被破坏后，不弹出确认关闭的对话框，防止异常。
                    }
                    catch { }

                    Application.Exit();
                }
                else             //如果注册表未被修改，则备份注册表
                {
                    //System.Diagnostics.Process.Start("RegBAK.bat",);
                    Process p = new Process();
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;                //隐藏DOS窗口
                    p.StartInfo.FileName = "RegBAK.bat";
                    p.Start();
                }

                //计算剩余的使用天数
                string strStartGUIDDate, strStartDate;
                strStartGUIDDate = RegStartGUIDDate.GetValue("StartGUIDDate").ToString();
                strStartDate = RegStartDate.GetValue("StartDate").ToString();

                string[] sss = strStartGUIDDate.Split('-');
                int nYear = Convert.ToInt16(sss[0]);
                int nMonth = Convert.ToInt16(sss[1]);
                int nDay = Convert.ToInt16(sss[2]);
                string[] sss1 = strStartDate.Split('-');

                DateTime oldDate = new DateTime(nYear, nMonth, nDay);
                DateTime newDate = DateTime.Now;
                TimeSpan ts = newDate - oldDate;

                int nts = Convert.ToInt32(ts.Days);
                int n = 0, n1 = 0;
                if (RegGUIDFlag.GetValue("nRegGUIDFlag").ToString() == "0")             //判断软件类型。0为未注册版本，101为学习版，203为商业版。
                {
                    n = 10 - nts;              //计算有效时间和TimeSpan之间的差值。小于0过期。
                    n1 = 10 - nts;
                }
                else
                {
                    n = Convert.ToInt32(RegValidGUIDDays.GetValue("nValidGUIDDays")) - nts;              //计算有效时间和TimeSpan之间的差值。小于0过期。
                    n1 = Convert.ToInt32(RegValidDays.GetValue("nValidDays")) - nts;
                }

                if (RegGUIDFlag.GetValue("nRegGUIDFlag").ToString() == "0")             //判断软件类型。0为未注册版本，101为学习版，203为商业版。
                {
                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                    {
                        MessageBox.Show("此版本为未注册版本，试用期限10天，" + "剩余天数：" + n.ToString() + " 天。" + "\r\n" + "\r\n"
                                        + "未注册版本每次的试用时间为30分钟，注册商业版无时间限制。", "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        if (n == 0)
                        {
                            MessageBox.Show("今天是最后一天了啦，要抓紧时间联系作者购买激活码哦。", "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        }
                    }
                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                    {
                        MessageBox.Show("此版本為未註冊版本，試用期限10天，" + "剩餘天數：" + n.ToString() + " 天。" + "\r\n" + "\r\n"
                                        + "未註冊版本每次的試用時間為30分鐘，註冊商業版無時間限制。", "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        if (n == 0)
                        {
                            MessageBox.Show("今天是最後一天了啦，要抓緊時間聯繫作者購買啟動碼哦。", "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        }
                    }
                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                    {
                        MessageBox.Show("This is an unregistered version, probation for 10 days." + "\r\n" + "Left : " + n.ToString() + " Days." + "\r\n" + "\r\n" 
                            + "Trial time is 30 minutes, commercial version without time limit.", "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        if (n == 0)
                        {
                            MessageBox.Show("Today is the last day, seize time" + "\r\n" + "to contact author for activation code.", "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        }
                    }
                }
                if (RegGUIDFlag.GetValue("nRegGUIDFlag").ToString() == "101")               //学习版
                {
                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                    {
                        AutoClosingMessageBox.Show("此版本为学习专用版本，试用期限180天，请勿作商业用途，" + "剩余天数：" + n.ToString() + " 天。" + "\r\n" + "\r\n"
                                                    + "注册商业版无时间限制，" + "本窗体将在5s后自动关闭。", "Aurora智能提示", 5000);
                    }
                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                    {
                        AutoClosingMessageBox.Show("此版本為學習專用版本，試用期限180天，請勿作商業用途，" + "剩餘天數：" + n.ToString() + " 天。" + "\r\n" + "\r\n"
                                                    + "註冊商業版無時間限制，" + "本表單將在5s後自動關閉。", "Aurora智慧提示", 5000);
                    }
                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                    {
                        AutoClosingMessageBox.Show("This is a study version,no commercial use, probation for 180 days. Left: " + n.ToString() + " days." + "\r\n" + "\r\n" 
                                                    + "Commercial version without time limit. This form will auto-close in 5s.", "Aurora Intelligent Tips", 5000);
                    }
                }
                else if (RegGUIDFlag.GetValue("nRegGUIDFlag").ToString() == "920")              //10元/1天版
                {
                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                    {
                        AutoClosingMessageBox.Show("此版本为1天体验版本，" + "剩余天数：" + n.ToString() + " 天。" + "\r\n" + "\r\n"
                                                    + "注册商业版无时间限制，" + "本窗体将在5s后自动关闭。", "Aurora智能提示", 5000);
                    }
                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                    {
                        AutoClosingMessageBox.Show("此版本為1天體驗版本，" + "剩餘天數：" + n.ToString() + " 天。" + "\r\n" + "\r\n"
                                                    + "註冊商業版無時間限制，" + "本表單將在5s後自動關閉。", "Aurora智慧提示", 5000);
                    }
                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                    {
                        AutoClosingMessageBox.Show("This is a 1 day trial version. Left: " + n.ToString() + " days." + "\r\n" + "\r\n"
                                                    + "Commercial version without time limit. This form will auto-close in 5s.", "Aurora Intelligent Tips", 5000);
                    }
                }

                if (n < 0 || n1 < 0)
                {
                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                    {
                        AutoClosingMessageBox.Show("软件试用版期限已经到啦，请联系作者获取注册码哦。" + "\r\n" + "\r\n"
                                                    + "本窗体将在10s后自动跳转到注册窗口。", "Aurora智能提示", 10000);
                    }
                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                    {
                        AutoClosingMessageBox.Show("軟體試用版期限已經到啦，請聯繫作者獲取註冊碼哦。" + "\r\n" + "\r\n"
                                                    + "本表單將在10s後自動跳轉到註冊視窗。", "Aurora智慧提示", 10000);
                    }
                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                    {
                        AutoClosingMessageBox.Show("Trial date has expired. Please contact author for activation code." + "\r\n" + "\r\n"
                                                    + "This form will auto-close in 10s and jump to register form.", "Aurora Intelligent Tips", 10000);
                    }
                    RegisterAurora FrmRegister = new RegisterAurora();
                    FrmRegister.StartPosition = FormStartPosition.CenterScreen;      //创建模态对话框
                    FrmRegister.ShowDialog();
                }

            }
            catch
            {
                RegSuccess = MyReg.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                if (RegSuccess.GetValue("RegSuccess").ToString() == "true")
                {
                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                    {
                        AutoClosingMessageBox.Show("请重新启动 Aurora 以使注册生效。", "Aurora智能提示", 5000);
                    }
                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                    {
                        AutoClosingMessageBox.Show("請重新啟動 Aurora 以使註冊生效。", "Aurora智慧提示", 5000);
                    }
                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                    {
                        AutoClosingMessageBox.Show("Please restart Aurora for the changes to take effect.", "Aurora Intelligent Tips", 5000);
                    }

                    RegistryKey MyReg1, RegReminder;
                    MyReg1 = Registry.CurrentUser;
                    RegReminder = MyReg1.CreateSubKey("Software\\Aurora\\Reminder");
                    try
                    {
                        RegReminder.SetValue("ExitReminder", "NO");             //此段控制注册信息被破坏后，不弹出确认关闭的对话框，防止异常。
                    }
                    catch { }

                    Application.Exit();
                } 
                else
                {
                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                    {
                        AutoClosingMessageBox.Show("哦~~~卖糕de！Aurora遇到一个来自火星的错误。请尝试重新注册Aurora。" + "\r\n"
                                                    + "如果确认操作无误，请忽略此错误即可。" + "\r\n"
                                                    + "程序即将在 10s 后自动退出。。。。。。", "Aurora智能提示", 10000);
                    }
                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                    {
                        AutoClosingMessageBox.Show("哦~~~賣糕de！Aurora遇到一個來自火星的錯誤。請嘗試重新註冊Aurora。" + "\r\n"
                                                    + "如果確認操作無誤，請忽略此錯誤即可。" + "\r\n"
                                                    + "程式即將在 10s 後自動退出。。。。。。", "Aurora智慧提示", 10000);
                    }
                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                    {
                        AutoClosingMessageBox.Show("Oh~~~My god！Aurora has encountered an error from Mars. Please register Aurora again." + "\r\n" + "\r\n"
                                                    + "If done correctly, please ignore this error. This program will exit in 10s...", "Aurora Intelligent Tips", 10000);
                    }

                    RegistryKey MyReg1, RegReminder;
                    MyReg1 = Registry.CurrentUser;
                    RegReminder = MyReg1.CreateSubKey("Software\\Aurora\\Reminder");
                    try
                    {
                        RegReminder.SetValue("ExitReminder", "NO");             //此段控制注册信息被破坏后，不弹出确认关闭的对话框，防止异常。
                    }
                    catch { }

                    Application.Exit();             //捕获异常，防止草泥马de删除注册表、或者注册表信息丢失。
                }              
            }

            //新功能引导界面，仅在程序第一次运行时显示。
            string strIsFirstRun = "false";
            bool isFirstRun = false;
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            strIsFirstRun = config.AppSettings.Settings["IsFirstRun"].Value;
            if (string.IsNullOrEmpty(strIsFirstRun) || strIsFirstRun.ToLower() != "true")
            {
                isFirstRun = false;
            }
            else
            {
                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                {
                    AutoClosingMessageBox.Show("请您在使用本产品之前仔细阅读Aurora用户手册。" + "\r\n"
                                                + "本消息即将在 5s 后自动关闭。。。。。。", "Aurora智能提示", 5000);
                }
                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                {
                    AutoClosingMessageBox.Show("請您在使用本產品之前仔細閱讀Aurora使用者手冊。" + "\r\n"
                                                + "本消息即將在 5s 後自動關閉。。。。。。", "Aurora智慧提示", 5000);
                }
                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                {
                    AutoClosingMessageBox.Show("Please read Aurora user manual carefully before using this software."+ "\r\n"
                                                + "This message will close in 5s...", "Aurora Intelligent Tips", 5000);
                }

                isFirstRun = true;
                NewFunction frmNewFunction = new NewFunction();
                frmNewFunction.ShowDialog();
            }

            strIsFirstRun = "false";
            config.AppSettings.Settings.Remove("IsFirstRun");
            config.AppSettings.Settings.Add("IsFirstRun", strIsFirstRun);
            config.Save();

            //读取窗体保存的大小和位置
            //RegistryKey RegLocation, RegSize;
            //try
            //{
                //RegLocation = MyReg.CreateSubKey("Software\\Aurora\\Location");
                //RegSize = MyReg.CreateSubKey("Software\\Aurora\\Size");
                //this.Location = new Point(Convert.ToInt16(RegLocation.GetValue("LocationX")), Convert.ToInt16(RegLocation.GetValue("LocationY")));
                //this.WindowState = FormWindowState.Normal;
                //this.Size = new Size(Convert.ToInt16(RegSize.GetValue("Width")), Convert.ToInt16(RegSize.GetValue("Height")));
            //}
            //catch { }

            //读取窗体背景值
            try
            {
                RegistryKey RegBKLocation;//声明注册表对象
                RegBKLocation = MyReg.CreateSubKey("Software\\Aurora\\Color");//在注册表项中创建子项
                string ss = RegBKLocation.GetValue("BKEnabled").ToString();//显示注册表的位置

                File.Copy(Application.StartupPath + "\\BK1.jpg", Application.StartupPath + "\\BK.jpg", true);

                if (ss == "true")
                {
                    this.BackgroundImage = Image.FromFile(Application.StartupPath + "\\BK.jpg",true);
                    this.BackgroundImageLayout = ImageLayout.Tile;
                    this.menuStrip1.BackgroundImage = Image.FromFile(Application.StartupPath + "\\BK.jpg");
                    this.menuStrip1.BackgroundImageLayout = ImageLayout.Tile;
                    this.toolStrip1.BackgroundImage = Image.FromFile(Application.StartupPath + "\\BK.jpg");
                    this.toolStrip1.BackgroundImageLayout = ImageLayout.Tile;
                    this.groupBox1.BackgroundImage = Image.FromFile(Application.StartupPath + "\\BK.jpg");
                    this.groupBox1.BackgroundImageLayout = ImageLayout.Tile;
                    this.statusStrip1.BackgroundImage = Image.FromFile(Application.StartupPath + "\\BK.jpg");
                    this.statusStrip1.BackgroundImageLayout = ImageLayout.Tile;
                }
                else this.BackgroundImage = null;
                
            }
            catch { }

            //读取是否开始数据锁
            RegistryKey RegLockerSCS, RegnTimer;//声明注册表对象
            try
            {
                RegLockerSCS = MyReg.CreateSubKey("Software\\Aurora\\Locker");//在注册表项中创建子项
                RegnTimer = MyReg.CreateSubKey("Software\\Aurora\\Locker");
                nSCS = Convert.ToInt32(RegLockerSCS.GetValue("Enabled"));
                nTimer = Convert.ToInt32(RegnTimer.GetValue("Timer"));
            }
            catch {}
            
            //textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
            //textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")                //读取区域语言
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("zh-CN");
                ApplyResource();
                toolStripMenuItem_Chs.Checked = true;

                AdjustType.Items.Clear();
                AdjustType.Items.Add("闭合水准");
                AdjustType.Items.Add("附合水准");
                AdjustType.Items.Add("支水准");
                AdjustType.Items.Add("闭合导线");
                AdjustType.Items.Add("闭合导线(含外支点)");
                AdjustType.Items.Add("具有一个连接角的附和导线");
                AdjustType.Items.Add("具有两个连接角的附和导线");
                AdjustType.Items.Add("支导线");
                AdjustType.Items.Add("无连接角导线");
                AdjustType.Items.Add("水准网");
                AdjustType.Text = "闭合导线";

                AngleMode.Items.Clear();
                AngleMode.Items.Add("左角");
                AngleMode.Items.Add("右角");
                AngleMode.Text = "左角";

                LevelMode.Items.Clear();
                LevelMode.Items.Add("距离");
                LevelMode.Items.Add("测站数");
                LevelMode.Text = "距离";

                MeasGrade.Items.Clear();
                MeasGrade.Items.Add("三等");
                MeasGrade.Items.Add("四等");
                MeasGrade.Items.Add("一级");
                MeasGrade.Items.Add("二级");
                MeasGrade.Items.Add("三级");
                MeasGrade.Items.Add("自定义");
                MeasGrade.Text = "三级";

                MeasArea.Items.Clear();
                MeasArea.Items.Add("平地");
                MeasArea.Items.Add("山地");
                MeasArea.Text = "平地";
                MeasArea.Visible = false;

                toolStripTextBox1.Text = "图标大小：";
                toolStripComboBox1.Items.Clear();
                toolStripComboBox1.Items.Add("小图标");
                toolStripComboBox1.Items.Add("中图标");
                toolStripComboBox1.Items.Add("大图标");
                toolStripComboBox1.Text = "中图标";

                this.AdjustListView.Clear();
                this.AdjustListView.Columns.Add("序号", 50, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("测站", 50, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("观测角度", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("改正数", 60, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("改正后角度", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("方位角", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("边长(m)", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("X坐标增量ΔX(m)", 110, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Y坐标增量ΔY(m)", 110, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("改正后ΔX(m)", 100, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("改正后ΔY(m)", 100, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("X坐标(m)", 100, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Y坐标(m)", 100, HorizontalAlignment.Center);
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")                //读取区域语言
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("zh-Hant");
                ApplyResource();
                toolStripMenuItem_Cht.Checked = true;

                AdjustType.Items.Clear();
                AdjustType.Items.Add("閉合水準");
                AdjustType.Items.Add("附合水準");
                AdjustType.Items.Add("支水準");
                AdjustType.Items.Add("閉合導線");
                AdjustType.Items.Add("閉合導線(含外支點)");
                AdjustType.Items.Add("具有一个連接角的附和導線");
                AdjustType.Items.Add("具有两个連接角的附和導線");
                AdjustType.Items.Add("支導線");
                AdjustType.Items.Add("无連接角導線");
                AdjustType.Items.Add("水準網");
                AdjustType.Text = "閉合導線";

                AngleMode.Items.Clear();
                AngleMode.Items.Add("左角");
                AngleMode.Items.Add("右角");
                AngleMode.Text = "左角";

                LevelMode.Items.Clear();
                LevelMode.Items.Add("距離");
                LevelMode.Items.Add("測站數");
                LevelMode.Text = "距離";

                MeasGrade.Items.Clear();
                MeasGrade.Items.Add("叁等");
                MeasGrade.Items.Add("肆等");
                MeasGrade.Items.Add("壹级");
                MeasGrade.Items.Add("貳级");
                MeasGrade.Items.Add("叁级");
                MeasGrade.Items.Add("自定義");
                MeasGrade.Text = "叁级";

                MeasArea.Items.Clear();
                MeasArea.Items.Add("平地");
                MeasArea.Items.Add("山地");
                MeasArea.Text = "平地";
                MeasArea.Visible = false;

                toolStripTextBox1.Text = "圖標大小：";
                toolStripComboBox1.Items.Clear();
                toolStripComboBox1.Items.Add("小圖標");
                toolStripComboBox1.Items.Add("中圖標");
                toolStripComboBox1.Items.Add("大圖標");
                toolStripComboBox1.Text = "中圖標";

                this.AdjustListView.Clear();
                this.AdjustListView.Columns.Add("序號", 50, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("測站", 50, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("觀測角度", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("改正數", 60, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("改正後角度", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("方位角", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("邊長(m)", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("X座標增量ΔX(m)", 110, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Y座標增量ΔY(m)", 110, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("改正後ΔX(m)", 100, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("改正後ΔY(m)", 100, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("X座標(m)", 100, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Y座標(m)", 100, HorizontalAlignment.Center);
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString().Substring(0, 2) == "en")        //这招更狠，只要开头是en的都设置成英文版，管尼玛en-US,en-India神马的。
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");
                ApplyResource();
                toolStripMenuItem_En.Checked = true;

                AdjustType.Items.Clear();
                AdjustType.Items.Add("Closed Leveling");
                AdjustType.Items.Add("Annexed Leveling");
                AdjustType.Items.Add("Spur Leveling");
                AdjustType.Items.Add("Closed Traverse");
                AdjustType.Items.Add("Closed Traverse With Outer Point");
                AdjustType.Items.Add("One Angle Conn-Traverse");
                AdjustType.Items.Add("Two Angle Conn-Traverse");
                AdjustType.Items.Add("Open Traverse");
                AdjustType.Items.Add("No Angle Conn-Traverse");
                AdjustType.Items.Add("Leveling Network");
                AdjustType.Text = "Closed Traverse";

                AngleMode.Items.Clear();
                AngleMode.Items.Add("Left");
                AngleMode.Items.Add("Right");
                AngleMode.Text = "Left";

                LevelMode.Items.Clear();
                LevelMode.Items.Add("Dist");
                LevelMode.Items.Add("Stations");
                LevelMode.Text = "Dist";

                MeasGrade.Items.Clear();
                MeasGrade.Items.Add("3rd Grade");
                MeasGrade.Items.Add("4th Grade");
                MeasGrade.Items.Add("1st Class");
                MeasGrade.Items.Add("2nd Class");
                MeasGrade.Items.Add("3rd Class");
                MeasGrade.Items.Add("User defined");
                MeasGrade.Text = "3rd Class";

                MeasArea.Items.Clear();
                MeasArea.Items.Add("Flat");
                MeasArea.Items.Add("Hill");
                MeasArea.Text = "Flat";
                MeasArea.Visible = false;

                toolStripTextBox1.Text = "Icon Size:";
                toolStripComboBox1.Items.Clear();
                toolStripComboBox1.Items.Add("Small");
                toolStripComboBox1.Items.Add("Medium");
                toolStripComboBox1.Items.Add("Large");
                toolStripComboBox1.Text = "Medium";

                this.AdjustListView.Clear();
                this.AdjustListView.Columns.Add("ID", 50, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Station", 60, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Meas Angle", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Adjust", 60, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Adj-Angle", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Azimuth", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Length(m)", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("ΔX(m)", 110, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("ΔY(m)", 110, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Adj-ΔX(m)", 100, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Adj-ΔY(m)", 100, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("X(m)", 100, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Y(m)", 100, HorizontalAlignment.Center);

            }
            else           //不知道统统搞成鸟语，得了
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");
                ApplyResource();
                toolStripMenuItem_En.Checked = true;

                AdjustType.Items.Clear();
                AdjustType.Items.Add("Closed Leveling");
                AdjustType.Items.Add("Annexed Leveling");
                AdjustType.Items.Add("Spur Leveling");
                AdjustType.Items.Add("Closed Traverse");
                AdjustType.Items.Add("Closed Traverse With Outer Point");
                AdjustType.Items.Add("One Angle Conn-Traverse");
                AdjustType.Items.Add("Two Angle Conn-Traverse");
                AdjustType.Items.Add("Open Traverse");
                AdjustType.Items.Add("No Angle Conn-Traverse");
                AdjustType.Items.Add("Leveling Network");
                AdjustType.Text = "Closed Traverse";

                AngleMode.Items.Clear();
                AngleMode.Items.Add("Left");
                AngleMode.Items.Add("Right");
                AngleMode.Text = "Left";

                LevelMode.Items.Clear();
                LevelMode.Items.Add("Dist");
                LevelMode.Items.Add("Stations");
                LevelMode.Text = "Dist";

                MeasGrade.Items.Clear();
                MeasGrade.Items.Add("3rd Grade");
                MeasGrade.Items.Add("4th Grade");
                MeasGrade.Items.Add("1st Class");
                MeasGrade.Items.Add("2nd Class");
                MeasGrade.Items.Add("3rd Class");
                MeasGrade.Items.Add("User defined");
                MeasGrade.Text = "3rd Class";

                MeasArea.Items.Clear();
                MeasArea.Items.Add("Flat");
                MeasArea.Items.Add("Hill");
                MeasArea.Text = "Flat";
                MeasArea.Visible = false;

                toolStripTextBox1.Text = "Icon Size:";
                toolStripComboBox1.Items.Clear();
                toolStripComboBox1.Items.Add("Small");
                toolStripComboBox1.Items.Add("Medium");
                toolStripComboBox1.Items.Add("Large");
                toolStripComboBox1.Text = "Medium";

                this.AdjustListView.Clear();
                this.AdjustListView.Columns.Add("ID", 50, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Station", 60, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Meas Angle", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Adjust", 60, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Adj-Angle", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Azimuth", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Length(m)", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("ΔX(m)", 110, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("ΔY(m)", 110, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Adj-ΔX(m)", 100, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Adj-ΔY(m)", 100, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("X(m)", 100, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Y(m)", 100, HorizontalAlignment.Center);
            }

            this.Refresh();
        }

        private void AdjustType_SelectedIndexChanged(object sender, EventArgs e)            //选择平差类型
        {
            AdjustListView.BeginUpdate();

            #region 简体中文列表
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")                //读取区域语言
            {
                switch (AdjustType.SelectedIndex)
                {
                    case 0:         //闭合水准
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("序号", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("点号", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("距离(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("测站数", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("实测高差(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正数(mm)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正后高差(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("高程(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("二等");
                            MeasGrade.Items.Add("三等");
                            MeasGrade.Items.Add("四等");
                            MeasGrade.Items.Add("五等");
                            MeasGrade.Items.Add("自定义");
                            MeasArea.Visible = true;
                            MeasGrade.Text = "三等";
                            MeasArea.Text = "平地";
                        }
                        break;
                    case 1:         //附和水准
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("序号", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("点号", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("距离(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("测站数", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("实测高差(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正数(mm)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正后高差(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("高程(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("二等");
                            MeasGrade.Items.Add("三等");
                            MeasGrade.Items.Add("四等");
                            MeasGrade.Items.Add("五等");
                            MeasGrade.Items.Add("自定义");
                            MeasArea.Visible = true;
                            MeasGrade.Text = "三等";
                            MeasArea.Text = "平地";
                        }
                        break;
                    case 2:         //支水准
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("序号", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("点号", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("距离(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("测站数", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("实测高差(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("高程(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("二等");
                            MeasGrade.Items.Add("三等");
                            MeasGrade.Items.Add("四等");
                            MeasGrade.Items.Add("五等");
                            MeasGrade.Items.Add("自定义");
                            MeasArea.Visible = true;
                            MeasGrade.Text = "三等";
                            MeasArea.Text = "平地";
                        }
                        break;
                    case 3:         //闭合导线
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("序号", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("测站", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("观测角度", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正数", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正后角度", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("方位角", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("边长(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X坐标增量ΔX(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y坐标增量ΔY(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正后ΔX(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正后ΔY(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X坐标(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y坐标(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("三等");
                            MeasGrade.Items.Add("四等");
                            MeasGrade.Items.Add("一级");
                            MeasGrade.Items.Add("二级");
                            MeasGrade.Items.Add("三级");
                            MeasGrade.Items.Add("自定义");
                            MeasArea.Visible = false;
                            MeasGrade.Text = "三级";
                        }
                        break;
                    case 4:         //闭合导线(含外支点)
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("序号", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("测站", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("观测角度", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正数", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正后角度", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("方位角", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("边长(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X坐标增量ΔX(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y坐标增量ΔY(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正后ΔX(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正后ΔY(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X坐标(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y坐标(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("三等");
                            MeasGrade.Items.Add("四等");
                            MeasGrade.Items.Add("一级");
                            MeasGrade.Items.Add("二级");
                            MeasGrade.Items.Add("三级");
                            MeasGrade.Items.Add("自定义");
                            MeasArea.Visible = false;
                            MeasGrade.Text = "三级";
                        }
                        break;
                    case 5:         //一个连接角的附和导线
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("序号", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("测站", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("观测角度", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("方位角", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("边长(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X坐标增量ΔX(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y坐标增量ΔY(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正后ΔX(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正后ΔY(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X坐标(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y坐标(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("三等");
                            MeasGrade.Items.Add("四等");
                            MeasGrade.Items.Add("一级");
                            MeasGrade.Items.Add("二级");
                            MeasGrade.Items.Add("三级");
                            MeasGrade.Items.Add("自定义");
                            MeasArea.Visible = false;
                            MeasGrade.Text = "三级";
                        }
                        break;
                    case 6:         //两个连接角的附和导线
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("序号", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("测站", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("观测角度", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正数", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正后角度", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("方位角", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("边长(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X坐标增量ΔX(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y坐标增量ΔY(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正后ΔX(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正后ΔY(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X坐标(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y坐标(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("三等");
                            MeasGrade.Items.Add("四等");
                            MeasGrade.Items.Add("一级");
                            MeasGrade.Items.Add("二级");
                            MeasGrade.Items.Add("三级");
                            MeasGrade.Items.Add("自定义");
                            MeasArea.Visible = false;
                            MeasGrade.Text = "三级";
                        }
                        break;
                    case 7:         //支导线
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("序号", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("测站", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("观测角度", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("方位角", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("边长(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X坐标增量ΔX(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y坐标增量ΔY(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X坐标(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y坐标(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("三等");
                            MeasGrade.Items.Add("四等");
                            MeasGrade.Items.Add("一级");
                            MeasGrade.Items.Add("二级");
                            MeasGrade.Items.Add("三级");
                            MeasGrade.Items.Add("自定义");
                            MeasArea.Visible = false;
                            MeasGrade.Text = "三级";
                        }
                        break;
                    case 8:         //无连接角导线
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("序号", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("测站", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("观测角度", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("观测边长(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("假定方位角", 80, HorizontalAlignment.Center);

                            AdjustListView.Columns.Add("假定X坐标增量ΔX(m)", 130, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("假定Y坐标增量ΔY(m)", 130, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("假定X坐标(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("假定Y坐标(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X坐标(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y坐标(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("三等");
                            MeasGrade.Items.Add("四等");
                            MeasGrade.Items.Add("一级");
                            MeasGrade.Items.Add("二级");
                            MeasGrade.Items.Add("三级");
                            MeasGrade.Items.Add("自定义");
                            MeasArea.Visible = false;
                            MeasGrade.Text = "三级";
                        }
                        break;
                    case 9:         //水准网
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("序号", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("起始点", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("终止点", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("高差(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("平距(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("二等");
                            MeasGrade.Items.Add("三等");
                            MeasGrade.Items.Add("四等");
                            MeasGrade.Items.Add("五等");
                            MeasGrade.Items.Add("自定义");
                            MeasArea.Visible = true;
                            MeasGrade.Text = "三等";
                            MeasArea.Text = "平地";
                        }
                        break;
                }
            }
            #endregion

            #region 繁体中文列表
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")                //读取区域语言
            {
                switch (AdjustType.SelectedIndex)
                {
                    case 0:         //闭合水准
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("序號", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("點號", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("距離(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("測站數", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("實測高差(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正數(mm)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正後高差(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("高程(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("貳等");
                            MeasGrade.Items.Add("叁等");
                            MeasGrade.Items.Add("肆等");
                            MeasGrade.Items.Add("伍等");
                            MeasGrade.Items.Add("自定義");
                            MeasArea.Visible = true;
                            MeasGrade.Text = "叁等";
                            MeasArea.Text = "平地";
                        }
                        break;
                    case 1:         //附和水准
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("序號", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("點號", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("距離(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("測站數", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("實測高差(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正數(mm)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正後高差(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("高程(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("貳等");
                            MeasGrade.Items.Add("叁等");
                            MeasGrade.Items.Add("肆等");
                            MeasGrade.Items.Add("伍等");
                            MeasGrade.Items.Add("自定義");
                            MeasArea.Visible = true;
                            MeasGrade.Text = "叁等";
                            MeasArea.Text = "平地";
                        }
                        break;
                    case 2:         //支水准
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("序號", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("點號", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("距離(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("測站數", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("實測高差(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("高程(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("貳等");
                            MeasGrade.Items.Add("叁等");
                            MeasGrade.Items.Add("肆等");
                            MeasGrade.Items.Add("伍等");
                            MeasGrade.Items.Add("自定義");
                            MeasArea.Visible = true;
                            MeasGrade.Text = "叁等";
                            MeasArea.Text = "平地";
                        }
                        break;
                    case 3:         //闭合导线
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("序號", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("測站", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("觀測角度", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正數", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正後角度", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("方位角", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("邊長(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X座標增量ΔX(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y座標增量ΔY(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正後ΔX(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正後ΔY(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X座標(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y座標(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("叁等");
                            MeasGrade.Items.Add("肆等");
                            MeasGrade.Items.Add("壹级");
                            MeasGrade.Items.Add("貳级");
                            MeasGrade.Items.Add("叁级");
                            MeasGrade.Items.Add("自定義");
                            MeasArea.Visible = false;
                            MeasGrade.Text = "叁级";
                        }
                        break;
                    case 4:         //闭合导线(含外支点)
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("序號", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("測站", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("觀測角度", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正數", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正後角度", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("方位角", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("邊長(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X座標增量ΔX(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y座標增量ΔY(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正後ΔX(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正後ΔY(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X座標(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y座標(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("叁等");
                            MeasGrade.Items.Add("肆等");
                            MeasGrade.Items.Add("壹级");
                            MeasGrade.Items.Add("貳级");
                            MeasGrade.Items.Add("叁级");
                            MeasGrade.Items.Add("自定義");
                            MeasArea.Visible = false;
                            MeasGrade.Text = "叁级";
                        }
                        break;
                    case 5:         //一个连接角的附和导线
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("序號", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("測站", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("觀測角度", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("方位角", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("邊長(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X座標增量ΔX(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y座標增量ΔY(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正後ΔX(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正後ΔY(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X座標(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y座標(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("叁等");
                            MeasGrade.Items.Add("肆等");
                            MeasGrade.Items.Add("壹级");
                            MeasGrade.Items.Add("貳级");
                            MeasGrade.Items.Add("叁级");
                            MeasGrade.Items.Add("自定義");
                            MeasArea.Visible = false;
                            MeasGrade.Text = "叁级";
                        }
                        break;
                    case 6:         //两个连接角的附和导线
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("序號", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("測站", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("觀測角度", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正數", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正後角度", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("方位角", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("邊長(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X座標增量ΔX(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y座標增量ΔY(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正後ΔX(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("改正後ΔY(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X座標(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y座標(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("叁等");
                            MeasGrade.Items.Add("肆等");
                            MeasGrade.Items.Add("壹级");
                            MeasGrade.Items.Add("貳级");
                            MeasGrade.Items.Add("叁级");
                            MeasGrade.Items.Add("自定義");
                            MeasArea.Visible = false;
                            MeasGrade.Text = "叁级";
                        }
                        break;
                    case 7:         //支导线
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("序號", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("測站", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("觀測角度", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("方位角", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("邊長(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X座標增量ΔX(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y座標增量ΔY(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X座標(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y座標(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("叁等");
                            MeasGrade.Items.Add("肆等");
                            MeasGrade.Items.Add("壹级");
                            MeasGrade.Items.Add("貳级");
                            MeasGrade.Items.Add("叁级");
                            MeasGrade.Items.Add("自定義");
                            MeasArea.Visible = false;
                            MeasGrade.Text = "叁级";
                        }
                        break;
                    case 8:         //无连接角导线
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("序號", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("測站", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("觀測角度", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("觀測边长(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("假定方位角", 80, HorizontalAlignment.Center);

                            AdjustListView.Columns.Add("假定X座標增量ΔX(m)", 130, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("假定Y座標增量ΔY(m)", 130, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("假定X座標(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("假定Y座標(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X座標(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y座標(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("叁等");
                            MeasGrade.Items.Add("肆等");
                            MeasGrade.Items.Add("壹级");
                            MeasGrade.Items.Add("貳级");
                            MeasGrade.Items.Add("叁级");
                            MeasGrade.Items.Add("自定義");
                            MeasArea.Visible = false;
                            MeasGrade.Text = "叁级";
                        }
                        break;
                    case 9:         //水准网
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("序號", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("起始點", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("終止點", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("高差(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("平距(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("叁等");
                            MeasGrade.Items.Add("肆等");
                            MeasGrade.Items.Add("壹级");
                            MeasGrade.Items.Add("貳级");
                            MeasGrade.Items.Add("叁级");
                            MeasGrade.Items.Add("自定義");
                            MeasArea.Visible = false;
                            MeasGrade.Text = "叁级";
                        }
                        break;
                }
            }
            #endregion

            #region 英文列表
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")                //读取区域语言
            {
                switch (AdjustType.SelectedIndex)
                {
                    case 0:         //闭合水准
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("ID", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("PT", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Dist(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Stations", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("ObsLvlDiff(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Adjust(mm)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Adj-LvlDiff(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Elevation(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("2nd Grade");
                            MeasGrade.Items.Add("3rd Grade");
                            MeasGrade.Items.Add("4th Grade");
                            MeasGrade.Items.Add("5th Grade");
                            MeasGrade.Items.Add("User Defined");
                            MeasArea.Visible = true;
                            MeasGrade.Text = "3rd Grade";
                            MeasArea.Text = "Flat";
                        }
                        break;
                    case 1:         //附和水准
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("ID", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("PT", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Dist(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Stations", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("ObsLvlDiff(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Adjust(mm)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Adj-LvlDiff(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Elevation(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("2nd Grade");
                            MeasGrade.Items.Add("3rd Grade");
                            MeasGrade.Items.Add("4th Grade");
                            MeasGrade.Items.Add("5th Grade");
                            MeasGrade.Items.Add("User Defined");
                            MeasArea.Visible = true;
                            MeasGrade.Text = "3rd Grade";
                            MeasArea.Text = "Flat";
                        }
                        break;
                    case 2:         //支水准
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("ID", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("PT", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Dist(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Sattions", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("ObsLvlDiff(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Elevation(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("2nd Grade");
                            MeasGrade.Items.Add("3rd Grade");
                            MeasGrade.Items.Add("4th Grade");
                            MeasGrade.Items.Add("5th Grade");
                            MeasGrade.Items.Add("User Defined");
                            MeasArea.Visible = true;
                            MeasGrade.Text = "3rd Grade";
                            MeasArea.Text = "Flat";
                        }
                        break;
                    case 3:         //闭合导线
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("ID", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Station", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Meas Angle", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Adjust", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Adj-Angle", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Azimuth", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Length(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("ΔX(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("ΔY(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Adj-ΔX(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Adj-ΔY(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("3rd Grade");
                            MeasGrade.Items.Add("4th Grade");
                            MeasGrade.Items.Add("1st Class");
                            MeasGrade.Items.Add("2nd Class");
                            MeasGrade.Items.Add("3rd Class");
                            MeasGrade.Items.Add("User defined");
                            MeasArea.Visible = false;
                            MeasGrade.Text = "3rd Class";
                        }
                        break;
                    case 4:         //闭合导线(含外支点)
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("ID", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Station", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Meas Angle", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Adjust", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Adj-Angle", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Azimuth", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Length(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("ΔX(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("ΔY(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Adj-ΔX(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Adj-ΔY(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("3rd Grade");
                            MeasGrade.Items.Add("4th Grade");
                            MeasGrade.Items.Add("1st Class");
                            MeasGrade.Items.Add("2nd Class");
                            MeasGrade.Items.Add("3rd Class");
                            MeasGrade.Items.Add("User defined");
                            MeasArea.Visible = false;
                            MeasGrade.Text = "3rd Class";
                        }
                        break;
                    case 5:         //一个连接角的附和导线
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("ID", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Station", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Meas Angle", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Azimuth", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Length(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("ΔX(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("ΔY(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Adj-ΔX(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Adj-ΔY(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("3rd Grade");
                            MeasGrade.Items.Add("4th Grade");
                            MeasGrade.Items.Add("1st Class");
                            MeasGrade.Items.Add("2nd Class");
                            MeasGrade.Items.Add("3rd Class");
                            MeasGrade.Items.Add("User defined");
                            MeasArea.Visible = false;
                            MeasGrade.Text = "3rd Class";
                        }
                        break;
                    case 6:         //两个连接角的附和导线
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("ID", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Station", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Meas Angle", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Adjust", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Adj-Angle", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Azimuth", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Length(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("ΔX(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("ΔY(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Adj-ΔX(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Adj-ΔY(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("3rd Grade");
                            MeasGrade.Items.Add("4th Grade");
                            MeasGrade.Items.Add("1st Class");
                            MeasGrade.Items.Add("2nd Class");
                            MeasGrade.Items.Add("3rd Class");
                            MeasGrade.Items.Add("User defined");
                            MeasArea.Visible = false;
                            MeasGrade.Text = "3rd Class";
                        }
                        break;
                    case 7:         //支导线
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("ID", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Station", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Meas Angle", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Azimuth", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Length(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("ΔX(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("ΔY(m)", 110, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("3rd Grade");
                            MeasGrade.Items.Add("4th Grade");
                            MeasGrade.Items.Add("1st Class");
                            MeasGrade.Items.Add("2nd Class");
                            MeasGrade.Items.Add("3rd Class");
                            MeasGrade.Items.Add("User defined");
                            MeasArea.Visible = false;
                            MeasGrade.Text = "3rd Class";
                        }
                        break;
                    case 8:         //无连接角导线
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("ID", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Station", 60, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Meas Angle", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Length(m)", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Assumed Azimuth", 130, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Assumed ΔX(m)", 130, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Assumed ΔY(m)", 130, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Assumed X(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Assumed Y(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("X(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Y(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("3rd Grade");
                            MeasGrade.Items.Add("4th Grade");
                            MeasGrade.Items.Add("1st Class");
                            MeasGrade.Items.Add("2nd Class");
                            MeasGrade.Items.Add("3rd Class");
                            MeasGrade.Items.Add("User defined");
                            MeasArea.Visible = false;
                            MeasGrade.Text = "3rd Class";
                        }
                        break;
                    case 9:         //水准网
                        {
                            AdjustListView.Clear();
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                            AdjustListView.Columns.Add("ID", 50, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("Start", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("End", 80, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("ObsLvlDiff(m)", 100, HorizontalAlignment.Center);
                            AdjustListView.Columns.Add("HD(m)", 100, HorizontalAlignment.Center);

                            MeasGrade.Items.Clear();
                            MeasGrade.Items.Add("3rd Grade");
                            MeasGrade.Items.Add("4th Grade");
                            MeasGrade.Items.Add("1st Class");
                            MeasGrade.Items.Add("2nd Class");
                            MeasGrade.Items.Add("3rd Class");
                            MeasGrade.Items.Add("User defined");
                            MeasArea.Visible = false;
                            MeasGrade.Text = "3rd Class";
                        }
                        break;
                }
            }
            #endregion

            AdjustListView.EndUpdate();
            nCalcFlag = 0;              //更换平差类型后，将计算完成标志设置为0.
        }

        private void AuroraMain_FormClosing(object sender, FormClosingEventArgs e)              //窗体关闭前询问是否关闭
        {
            //退出提示
            RegistryKey MyReg, RegReminder;
            MyReg = Registry.CurrentUser;//获取当前用户注册表项
            try
            {
                RegReminder = MyReg.CreateSubKey("Software\\Aurora\\Reminder");
                if (RegReminder.GetValue("ExitReminder").ToString() == "YES")
                {
                    DialogResult dr = DialogResult.Yes;
                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                    {
                        dr = MessageBox.Show("真的要走么，亲~~~?", "Aurora智能提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    }
                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                    {
                        dr = MessageBox.Show("真的要走麼，親~~~?", "Aurora智慧提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    }
                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                    {
                        dr = MessageBox.Show("Really leaving~~~?", "Aurora Intelligent Tips", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    }
                    if (dr == DialogResult.Yes)
                    {
                        e.Cancel = false;        //退出
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch { }
        }

        private void AuroraMain_FormClosed(object sender, FormClosedEventArgs e)                //窗体关闭后写入注册表，记录位置和大小
        {
            //关闭时写入窗体位置和大小
            //RegistryKey MyReg, RegLocation, RegSize;//声明注册表对象
            //MyReg = Registry.CurrentUser;//获取当前用户注册表项
            //RegLocation = MyReg.CreateSubKey("Software\\Aurora\\Location");//在注册表项中创建子项
            //RegSize = MyReg.CreateSubKey("Software\\Aurora\\Size");
            //try
            //{
            //    int nLocationX, nLocationY;
            //    nLocationX = this.Location.X;
            //    nLocationY = this.Location.Y;
            //    if (nLocationX < 0 || nLocationY < 0)
            //    {
            //        nLocationX = 0;
            //        nLocationY = 0;
            //    }
            //    RegLocation.SetValue("LocationX", nLocationX.ToString());//将窗体关闭位置的x坐标写入注册表项中
            //    RegLocation.SetValue("LocationY", nLocationY.ToString());//将窗体关闭位置的y坐标写入注册表项中
            //    RegSize.SetValue("Width", this.Width.ToString());
            //    RegSize.SetValue("Height", this.Height.ToString());
            //}
            //catch { }

            RegistryKey MyReg1, RegProjectInfo;//声明注册表对象
            MyReg1 = Registry.CurrentUser;//获取当前用户注册表项
            RegProjectInfo = MyReg1.CreateSubKey("Software\\Aurora\\ProjectInfo");//在注册表项中创建子项

            try
            {
                RegProjectInfo.SetValue("ProjectName", strProjectName);
                RegProjectInfo.SetValue("Calculator", strCalculator);
                RegProjectInfo.SetValue("Checker", strChecker);
                RegProjectInfo.SetValue("MyGrade", strMyGrade);
            }
            catch { }

            RegistryKey MyReg2, RegTotalTime;//声明注册表对象
            MyReg2 = Registry.CurrentUser;//获取当前用户注册表项
            RegTotalTime = MyReg2.CreateSubKey("Software\\Aurora\\TotalTime");//在注册表项中创建子项

            try
            {
                int nThisTime = stw.Elapsed.Hours * 60 + stw.Elapsed.Minutes;
                nTotalTime = nTotalTime + nThisTime;
                RegTotalTime.SetValue("TotalTime", nTotalTime.ToString());               //将软件本次运行的时间和以前注册表中的时间累加，写入注册表
            }
            catch { }

            log.Info(DateTime.Now.ToString() + " Close Aurora");             //关闭软件，写入一条新log
        }

        private void AuroraMain_Resize(object sender, EventArgs e)              //改变窗体大小,限制窗体最小值
        {
            this.AdjustListView.Height = this.groupBox1.Top - this.AdjustListView.Top - 15;
            this.AdjustListView.Width = this.Width - 40;

            int minWidth = 1160;
            int minHeight = 630;
            if (this.Width <= minWidth)
            {
                this.Width = minWidth;
            }
            if (this.Height <= minHeight)
            {
                this.Height = minHeight;
            }

            if (this.WindowState == FormWindowState.Minimized)              //当主窗体最小化的时候，如果命令行在运行，则隐藏。
            {
                if (PublicClass.MyCmd != null)
                {
                    PublicClass.MyCmd.Hide();
                }
            }
        }

        private void AuroraMain_SizeChanged(object sender, EventArgs e)             //窗体大小改变后，重新绘图
        {
            if (WindowState != FormWindowState.Minimized)               //当窗体最小化，不在进行绘图。
            {
                toolStripButton_Fullextent_Click(sender, e);
            }
        }

        private void AuroraMain_KeyDown(object sender, KeyEventArgs e)              //主窗体热键————按F1查看帮助，F5计算。。。
        {
            if (e.KeyCode == Keys.F1)               //按F1查看帮助
            {
                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                {
                    if (File.Exists(Application.StartupPath + "\\HelpDoc\\AuroraHelp_Chs.pdf"))
                    {
                        System.Diagnostics.Process.Start(Application.StartupPath + "\\HelpDoc\\AuroraHelp_Chs.pdf");
                    }
                    else
                        MessageBox.Show("Aurora智能帮助档丢失，请尝试重新安装Aurora。", "Aurora智能提示");
                }
                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                {
                    if (File.Exists(Application.StartupPath + "\\HelpDoc\\AuroraHelp_Cht.pdf"))
                    {
                        System.Diagnostics.Process.Start(Application.StartupPath + "\\HelpDoc\\AuroraHelp_Cht.pdf");
                    }
                    else
                        MessageBox.Show("Aurora智能幫助檔丟失，請嘗試重新安裝Aurora。", "Aurora智慧提示");
                }
                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                {
                    if (File.Exists(Application.StartupPath + "\\HelpDoc\\AuroraHelp_En.pdf"))
                    {
                        System.Diagnostics.Process.Start(Application.StartupPath + "\\HelpDoc\\AuroraHelp_En.pdf");
                    }
                    else
                        MessageBox.Show("Aurora help file is missing, please reinstall Aurora.", "Aurora Intelligent Tips");
                }
            }
            if (e.KeyCode == Keys.F5)               //F5——平差计算
            {
                toolStripButton_Calc_Click(sender, e);
            }
            if (e.KeyCode == Keys.F6)               //F6——绘图
            {
                toolStripButton_Mapping_Click(sender, e);
            }
            if (e.KeyCode == Keys.F7)               //F7——报表
            {
                toolStripButton_Report_Click(sender, e);
            }
            if (e.KeyCode == Keys.F8)               //F8——数据锁
            {
                toolStripButton_Locker_Click(sender, e);
            }
            else return;
        }

        private void Delay(int mm)              //delay延时函数
        {
            DateTime current = DateTime.Now;
            while (current.AddMilliseconds(mm) > DateTime.Now)
            {
                Application.DoEvents();
            }
            return;
        }

        static public double CoortoRad(double X1, double Y1, double X2, double Y2)              //两点坐标(X1,Y1)(X2,Y2)计算方位角，返回弧度  X--N  ,  Y--E
        {
            double DeltaY = Y2 - Y1;
            double DeltaX = X2 - X1;
            double Alpha = 0;

            Alpha = Math.Atan(DeltaY / DeltaX);

            if (DeltaX > 0 && DeltaY == 0)
            { Alpha = 0; }
            if (DeltaY > 0 && DeltaX > 0)
            { Alpha = Alpha; }
            if (DeltaX == 0 && DeltaY > 0)
            { Alpha = Math.PI * 0.5; }
            if (DeltaX < 0 && DeltaY > 0)
            { Alpha = Math.PI + Alpha; }
            if (DeltaX < 0 && DeltaY == 0)
            { Alpha = Math.PI; }
            if (DeltaX < 0 && DeltaY < 0)
            { Alpha = Math.PI + Alpha; }
            if (DeltaX == 0 && DeltaY < 0)
            { Alpha = Math.PI * 1.5; }
            if (DeltaX > 0 && DeltaY < 0)
            { Alpha = Alpha + Math.PI * 2; }

            return Alpha;
        }

        static public double CoortoDist(double X1, double Y1, double X2, double Y2)             //两点坐标(X1,Y1)(X2,Y2)计算平距  X--N  ,  Y--E
        {
            double DeltaY = Y2 - Y1;
            double DeltaX = X2 - X1;
            double Dist = 0;

            Dist = Math.Sqrt(DeltaY * DeltaY + DeltaX * DeltaX);
            return Dist;
        }

        #region 角度单位转化
        //角度单位互相转化
        //Rad----弧度  ----3.1415弧度
        //Dec----小数度----179.9947度
        //DMS----度分秒----179.5941
        static public double RadtoDec(double Rad)        //将弧度转为小数度
        {
            double dDec = 0;
            dDec = Rad * 180 / Math.PI;
            return dDec;
        }

        static public double DectoRad(double Dec)        //将小数度转为弧度
        {
            double dRad = 0;
            dRad = Dec * Math.PI / 180;
            return dRad;
        }

        static public double DectoDMS(double Dec)        //小数度转化为ddd.mmss
        {
            int du = 0, fen = 0, miao = 0;
            du = Convert.ToInt32(Math.Floor(Dec));
            fen = Convert.ToInt32(Math.Floor((Dec - du + 0.00001) * 60));
            miao = Convert.ToInt32(((Dec - du + 0.00001) * 60 - fen + 0.00001) * 60);

            if (miao == 60)
            {
                fen = fen + 1;
                miao = 0;
            }
            if (fen == 60)
            {
                du = du + 1;
                fen = 0;
            }

            string sdu, sfen, smiao;
            sdu = du.ToString();
            sfen = fen.ToString();
            smiao = miao.ToString();

            if (sfen.Length < 2)
                sfen = "0" + sfen;
            if (smiao.Length < 2)
                smiao = "0" + smiao;

            string sDMS = du + "." + sfen + smiao;
            double dDMS = Convert.ToDouble(sDMS);

            if (dDMS >= 360)
                dDMS = dDMS - 360.0000;
            string strDMS;
            strDMS = dDMS.ToString("#0.0000");
            dDMS = Convert.ToDouble(strDMS);
            return dDMS;
        }

        static public double DMStoDec(double DMS)        //ddd.mmss转化为小数度
        {
            int fuhao = (int)(DMS / Math.Abs(DMS));
            DMS = Math.Abs(DMS);
            int d = (int)DMS;
            int m = ((int)(DMS * 100)) - d * 100;
            double s = DMS * 10000 - m * 100 - d * 10000;
            return ((d + m / 60.0 + s / 3600.0) * fuhao);
        }

        static public double RadtoDMS(double Rad)        //将弧度转为ddd.mmss
        {
            return DectoDMS((RadtoDec(Rad)));
        }

        static public double DMStoRad(double DMS)        //将ddd.mmss转为弧度
        {
            return DectoRad((DMStoDec(DMS)));
        }

        static public double DMStoS(double DMS)          //将ddd.mmss转为s
        {
            //DMS = Math.Abs(DMS);
            //int d = (int)DMS;
            //int m = ((int)(DMS * 100)) - d * 100;
            //double s = DMS * 10000 - m * 100 - d * 10000;
            //return(d * 3600 + m * 60 + s);
            double dDec = DMStoDec(DMS);        //两种方法等效
            return (dDec * 3600);
        }

        static public double StoDMS(double s)            //将s转为ddd.mmss
        {
            double dDec = s / 3600;
            double dDMS = DectoDMS(dDec);
            if (dDMS >= 360)
                dDMS = dDMS - 360.0000;
            string strDMS;
            strDMS = dDMS.ToString("#0.0000");
            dDMS = Convert.ToDouble(strDMS);
            return dDMS;
        }

        static public double DectoBigDMS(double Dec)        //小数度转化为ddd.mmss，包含大于360°的数值，主要用于计算角度和
        {
            int du = 0, fen = 0, miao = 0;
            du = Convert.ToInt32(Math.Floor(Dec));
            fen = Convert.ToInt32(Math.Floor((Dec - du + 0.00001) * 60));
            miao = Convert.ToInt32(((Dec - du + 0.00001) * 60 - fen + 0.00001) * 60);

            if (miao == 60)
            {
                fen = fen + 1;
                miao = 0;
            }
            if (fen == 60)
            {
                du = du + 1;
                fen = 0;
            }

            string sdu, sfen, smiao;
            sdu = du.ToString();
            sfen = fen.ToString();
            smiao = miao.ToString();

            if (sfen.Length < 2)
                sfen = "0" + sfen;
            if (smiao.Length < 2)
                smiao = "0" + smiao;

            string sDMS = du + "." + sfen + smiao;
            double dDMS = Convert.ToDouble(sDMS);

            //if (dDMS >= 360)
            //    dDMS = dDMS - 360.0000;
            string strDMS;
            strDMS = dDMS.ToString("#0.0000");
            dDMS = Convert.ToDouble(strDMS);
            return dDMS;
        }

        static public double StoBigDMS(double s)            //将s转为ddd.mmss包含大于360°的数值，主要用于计算角度和
        {
            double dDec = s / 3600;
            double dDMS = DectoBigDMS(dDec);
            //if (dDMS >= 360)
            //    dDMS = dDMS - 360.0000;
            string strDMS;
            strDMS = dDMS.ToString("#0.0000");
            dDMS = Convert.ToDouble(strDMS);
            return dDMS;
        }

        #endregion

        #region 天数加密、解密算法
        public string MyEncrypt(string strEncrypt)              //天数加密算法 -- string
        {
            strEncrypt.Replace("1", "Z"); strEncrypt.Replace("2", "Q"); strEncrypt.Replace("3", "W"); strEncrypt.Replace("4", "N"); strEncrypt.Replace("5", "M");
            strEncrypt.Replace("6", "X"); strEncrypt.Replace("7", "I"); strEncrypt.Replace("8", "U"); strEncrypt.Replace("9", "O"); strEncrypt.Replace("0", "H");
            return strEncrypt;
        }

        public string MyDecrypt(string strDecrypt)              //天数解密算法 -- int
        {
            strDecrypt.Replace("Z", "1"); strDecrypt.Replace("Q", "2"); strDecrypt.Replace("W", "3"); strDecrypt.Replace("N", "4"); strDecrypt.Replace("M", "5");
            strDecrypt.Replace("X", "6"); strDecrypt.Replace("I", "7"); strDecrypt.Replace("U", "8"); strDecrypt.Replace("O", "9"); strDecrypt.Replace("H", "0");
            return strDecrypt;
        }
        #endregion

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)                //切换工具栏图标大小
        {
            switch (toolStripComboBox1.SelectedIndex)
            {
                case 0://小图标
                    {
                        this.toolStrip1.ImageScalingSize = new System.Drawing.Size(16, 16);
                        //this.toolStripButton_Add.Size = new System.Drawing.Size(23, 22);
                        //this.toolStripButton_Delete.Size = new System.Drawing.Size(23, 22);
                        //this.toolStripButton_Clear.Size = new System.Drawing.Size(23, 22);
                        //this.toolStripButton_Calc.Size = new System.Drawing.Size(23, 22);
                        //this.toolStripButton_Report.Size = new System.Drawing.Size(23, 22);
                        //this.toolStripButton_Setup.Size = new System.Drawing.Size(23, 22);
                        //this.toolStripButton_About.Size = new System.Drawing.Size(23, 22);

                        this.toolStripButton_Add.Image = Properties.Resources.Add_16;
                        this.toolStripButton_Delete.Image = Properties.Resources.Delete_16;
                        this.toolStripButton_Clear.Image = Properties.Resources.Clear_16;
                        this.toolStripButton_Calc.Image = Properties.Resources.Calc_16;
                        this.toolStripButton_Mapping.Image = Properties.Resources.Mapping_16;
                        this.toolStripButton_Report.Image = Properties.Resources.Report_16;
                        this.toolStripButton_CMD.Image = Properties.Resources.MyCmd_16;
                        this.toolStripButton_Locker.Image = Properties.Resources.Locker_16;
                        this.toolStripButton_Setup.Image = Properties.Resources.Setup_16;
                        this.toolStripButton_About.Image = Properties.Resources.About_16;
                    }
                    break;
                case 1://中图标
                    {
                        this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
                        //this.toolStripButton_Add.Size = new System.Drawing.Size(23, 22);
                        //this.toolStripButton_Delete.Size = new System.Drawing.Size(23, 22);
                        //this.toolStripButton_Clear.Size = new System.Drawing.Size(23, 22);
                        //this.toolStripButton_Calc.Size = new System.Drawing.Size(23, 22);
                        //this.toolStripButton_Report.Size = new System.Drawing.Size(23, 22);
                        //this.toolStripButton_Setup.Size = new System.Drawing.Size(23, 22);
                        //this.toolStripButton_About.Size = new System.Drawing.Size(23, 22);

                        this.toolStripButton_Add.Image = Properties.Resources.Add_24;
                        this.toolStripButton_Delete.Image = Properties.Resources.Delete_24;
                        this.toolStripButton_Clear.Image = Properties.Resources.Clear_24;
                        this.toolStripButton_Calc.Image = Properties.Resources.Calc_24;
                        this.toolStripButton_Mapping.Image = Properties.Resources.Mapping_24;
                        this.toolStripButton_Report.Image = Properties.Resources.Report_24;
                        this.toolStripButton_CMD.Image = Properties.Resources.MyCmd_24;
                        this.toolStripButton_Locker.Image = Properties.Resources.Locker_24;
                        this.toolStripButton_Setup.Image = Properties.Resources.Setup_24;
                        this.toolStripButton_About.Image = Properties.Resources.About_24;
                    }
                    break;
                case 2://大图标
                    {
                        this.toolStrip1.ImageScalingSize = new System.Drawing.Size(33, 32);
                        //this.toolStripButton_Add.Size = new System.Drawing.Size(33, 32);
                        //this.toolStripButton_Delete.Size = new System.Drawing.Size(33, 32);
                        //this.toolStripButton_Clear.Size = new System.Drawing.Size(33, 32);
                        //this.toolStripButton_Calc.Size = new System.Drawing.Size(33, 32);
                        //this.toolStripButton_Report.Size = new System.Drawing.Size(33, 32);
                        //this.toolStripButton_Setup.Size = new System.Drawing.Size(33, 32);
                        //this.toolStripButton_About.Size = new System.Drawing.Size(33, 32);

                        this.toolStripButton_Add.Image = Properties.Resources.Add_32;
                        this.toolStripButton_Delete.Image = Properties.Resources.Delete_32;
                        this.toolStripButton_Clear.Image = Properties.Resources.Clear_32;
                        this.toolStripButton_Calc.Image = Properties.Resources.Calc_32;
                        this.toolStripButton_Mapping.Image = Properties.Resources.Mapping_32;
                        this.toolStripButton_Report.Image = Properties.Resources.Report_32;
                        this.toolStripButton_CMD.Image = Properties.Resources.MyCmd_32;
                        this.toolStripButton_Locker.Image = Properties.Resources.Locker_32;
                        this.toolStripButton_Setup.Image = Properties.Resources.Setup_32;
                        this.toolStripButton_About.Image = Properties.Resources.About_32;
                    }
                    break;
            }
        }

        //公共变量
        public Color MyCellColor = Color.GreenYellow;               //程序默认为Color.GreenYellow
        public string strProjectName;
        public string strCalculator;
        public string strChecker;
        public string strMyGrade;

        //设置全局变量nCalcFlag，默认为零。当完成平差计算时，nCalcFlag = 1。否则不能进行绘图和生成报表。
        public int nCalcFlag = 0;

        //定义公共的求和变量。因为写入报表的是字符串，无法在报表中求和。
        //故现在现在平差计算中算出这些数值，然后直接传到报表中。       
        public string pfb, pSumStations, pSumAdjust, pSumAdjustLevelDiff, pSumObsAngle, pSumAdjObsAngle, pfx, pfy, pSumAdjDeltaX, pSumAdjDeltaY, pf, pK;

        #region 水准网平差变量及函数
        public string strLevelingNetworkData = "";
        public int m_Onumber = 0; //高差总数、观测值总数
        public int m_Tnumber = 0; //总点数
        public int m_Knumber = 0; //已知点数
        public double m_pvv; //[pvv]
        public string[] StartP; //高差起点号
        public string[] EndP; //高差终点号
        public string[] Pname; //点名地址数组
        public double[] L; //观测值数组
        public double[] KHeight; //高程值数组
        public double[] P; //观测值的权
        public double[] ATPA; //法方程系数矩阵与自由项
        public double[] ATPL;
        public double[] dX; //参数平差值（高程改正数）
        public double[] V; //残差数组
        public double m_mu; //验后单位权中误差

        public double s;//中误差
        public double ml;


        //高程近似值计算
        private void ca_HO()
        {
            for (int i = m_Knumber; i < m_Tnumber; i++)   //将未知高程点高程设为极小，以判断该点是不是未知高程点
                KHeight[i] = -9999.9;

            int jj = 0; //计算出近似高程的点数
            for (int ii = 0; ; ii++)
            {
                for (int j = 0; j < m_Onumber; j++)
                {
                    ArrayList arr = new ArrayList(Pname);
                    int k1 = arr.IndexOf(StartP[j]);  //高差起点号
                    int k2 = arr.IndexOf(EndP[j]);  //高差起点号
                    if (KHeight[k1] > -9999.0 && KHeight[k2] < -9999.0)
                    {
                        KHeight[k2] = KHeight[k1] + L[j];
                        jj++;
                    }
                    else if (KHeight[k1] < -9999.0 && KHeight[k2] > -9999.0)
                    {
                        KHeight[k1] = KHeight[k2] - L[j];
                        jj++;
                    }
                }
                if (jj == m_Tnumber - m_Knumber) break;
                if (ii > m_Tnumber - m_Knumber)
                {
                    for (int k = 0; k < m_Tnumber; k++)
                    {
                        if (KHeight[k] < -9999.0) MessageBox.Show("无法计算" + Pname[k] + "的近似高程");
                    }
                }
            }
        }

        //组成法方程
        private void ca_ATPA()
        {
            int t = m_Tnumber;

            for (int i = 0; i < t * (t + 1) / 2; i++)
                ATPA[i] = 0.0;
            for (int j = 0; j < t; j++)
                ATPL[j] = 0.0;
            for (int k = 0; k < m_Onumber; k++)
            {
                ArrayList arr = new ArrayList(Pname);
                int i = arr.IndexOf(StartP[k]);  //高差起点号
                int j = arr.IndexOf(EndP[k]);  //高差起点号
                double Pk = P[k];
                double Lk = L[k] - (KHeight[j] - KHeight[i]);

                //每一组观测数据只在系数矩阵相应位置添加了数据，在此采用累积法得出法方程系数阵
                //如起始点号为i,终点号为j，则ATPA中增加的数值为第i、j行对角线上各增加Pk；
                //第i行，第j列增加为-Pk；第j行，第i列增加为-Pk
                ATPL[i] -= Pk * Lk;
                ATPL[j] += Pk * Lk;
                ATPA[ij(i, i)] += Pk;
                ATPA[ij(j, j)] += Pk;
                ATPA[ij(i, j)] -= Pk;
            }
        }

        //对称矩阵下标计算函数
        private static int ij(int i, int j)
        {
            return (i >= j) ? i * (i + 1) / 2 + j : j * (j + 1) / 2 + i;
        }

        //  对称正定矩阵求逆(仅存下三角元素)
        private static bool inverse(double[] a, int n)
        {
            double[] a0 = new double[n];
            for (int k = 0; k < n; k++)
            {
                double a00 = a[0];
                if (a00 + 1.0 == 1.0) //判断矩阵是否降秩
                {
                    a0 = null;
                    return false;
                }
                for (int i = 1; i < n; i++)
                {
                    double ai0 = a[i * (i + 1) / 2];

                    if (i <= n - k - 1)
                        a0[i] = -ai0 / a00;
                    else
                        a0[i] = ai0 / a00;

                    for (int j = 1; j <= i; j++)
                    {
                        a[(i - 1) * i / 2 + j - 1] = a[i * (i + 1) / 2 + j] + ai0 * a0[j];
                    }
                }

                for (int pp = 1; pp < n; pp++)
                {
                    a[(n - 1) * n / 2 + pp - 1] = a0[pp];
                }
                a[n * (n + 1) / 2 - 1] = 1.0 / a00;
            }
            a0 = null;
            return true;
        }

        //    高程平差值计算
        private void ca_dX()
        {
            if (!inverse(ATPA, m_Tnumber))
            {
                return;
            }

            for (int i = 0; i < m_Tnumber; i++)
            {
                double xi = 0.0;
                for (int j = 0; j < m_Tnumber; j++)
                {
                    xi += ATPA[ij(i, j)] * ATPL[j];
                }
                dX[i] = xi;
                KHeight[i] += xi;
            }
        }

        //残差计算
        private double ca_V()
        {
            double pvv = 0.0;
            for (int i = 0; i <= m_Onumber - 1; i++)
            {
                ArrayList arr = new ArrayList(Pname);
                int k1 = arr.IndexOf(StartP[i]);  //高差起点号
                int k2 = arr.IndexOf(EndP[i]);  //高差起点号
                V[i] = KHeight[k2] - KHeight[k1] - L[i];
                pvv += V[i] * V[i] * P[i];
            }
            return (pvv);
        }

        #endregion

        #region 工具栏代码、主体算法
        public void toolStripButton_Add_Click(object sender, EventArgs e)              //增加行
        {
            int nCount = this.AdjustListView.Items.Count;
            
            ListViewItem lstItem = new ListViewItem();
            lstItem.UseItemStyleForSubItems = false;//设置允许子项颜色不一致,否则无背景颜色

            lstItem.SubItems.Add("");
            lstItem.SubItems.Add("");
            lstItem.SubItems.Add("");
            lstItem.SubItems.Add("");
            lstItem.SubItems.Add("");
            lstItem.SubItems.Add("");
            lstItem.SubItems.Add("");
            lstItem.SubItems.Add("");
            lstItem.SubItems.Add("");
            lstItem.SubItems.Add("");
            lstItem.SubItems.Add("");
            lstItem.SubItems.Add("");
            AdjustListView.Items.Add(lstItem);

            //读取单元格颜色
            RegistryKey MyReg1, RegColor;//声明注册表对象
            MyReg1 = Registry.CurrentUser;//获取当前用户注册表项
            try
            {
                RegColor = MyReg1.CreateSubKey("Software\\Aurora\\Color");//在注册表项中创建子项
                this.MyCellColor = ColorTranslator.FromHtml(RegColor.GetValue("CellColor").ToString());//显示注册表的位置
            }
            catch { }


            switch (AdjustType.SelectedIndex)       //选择平差类型
            {
                case 0: //闭合水准
                case 1: //附合水准
                    {
                        AdjustListView.Items[nCount].SubItems[1].BackColor = MyCellColor;        //点号

                        AdjustListView.Items[nCount].SubItems[2].BackColor = Color.White;      //距离
                        AdjustListView.Items[nCount].SubItems[3].BackColor = Color.White;      //测站数
                        AdjustListView.Items[nCount].SubItems[4].BackColor = Color.White;      //实测高差
                        if (nCount >= 1)
                        {
                            AdjustListView.Items[nCount - 1].SubItems[2].BackColor = MyCellColor;
                            AdjustListView.Items[nCount - 1].SubItems[3].BackColor = MyCellColor;
                            AdjustListView.Items[nCount - 1].SubItems[4].BackColor = MyCellColor;
                        }

                        AdjustListView.Items[nCount].SubItems[7].BackColor = MyCellColor;      //高程
                        if (nCount > 1)
                        {
                            AdjustListView.Items[nCount - 1].SubItems[7].BackColor = Color.White;
                        }
                    }
                    break;

                case 2: //支水准
                    {
                        AdjustListView.Items[nCount].SubItems[1].BackColor = MyCellColor;        //点号

                        AdjustListView.Items[nCount].SubItems[2].BackColor = Color.White;      //距离
                        AdjustListView.Items[nCount].SubItems[3].BackColor = Color.White;      //测站数
                        AdjustListView.Items[nCount].SubItems[4].BackColor = Color.White;      //实测高差
                        if (nCount >= 1)
                        {
                            AdjustListView.Items[nCount - 1].SubItems[2].BackColor = MyCellColor;
                            AdjustListView.Items[nCount - 1].SubItems[3].BackColor = MyCellColor;
                            AdjustListView.Items[nCount - 1].SubItems[4].BackColor = MyCellColor;
                        }

                        if (nCount > 0)
                        {
                            AdjustListView.Items[0].SubItems[5].BackColor = MyCellColor;
                        }

                    }
                    break;

                case 3: //闭合导线
                    {
                        AdjustListView.Items[nCount].SubItems[1].BackColor = MyCellColor;        //测站

                        //AdjustListView.Items[nCount].SubItems[2].BackColor = Color.GreenYellow;      //角度观测值
                        //if (nCount == 1)
                        //{
                        //    AdjustListView.Items[0].SubItems[2].BackColor = Color.White;
                        //}
                        AdjustListView.Items[nCount].SubItems[2].BackColor = Color.White;      //角度观测值
                        if (nCount >= 1)
                        {
                            AdjustListView.Items[nCount].SubItems[2].BackColor = MyCellColor;
                        }

                        AdjustListView.Items[nCount].SubItems[6].BackColor = Color.White;      //边长
                        if (nCount > 1)
                        {
                            AdjustListView.Items[nCount - 1].SubItems[6].BackColor = MyCellColor;
                        }

                        AdjustListView.Items[nCount].SubItems[11].BackColor = MyCellColor;       //X、Y坐标
                        AdjustListView.Items[nCount].SubItems[12].BackColor = MyCellColor;
                        if (nCount > 1)
                        {
                            AdjustListView.Items[1].SubItems[11].BackColor = MyCellColor;
                            AdjustListView.Items[1].SubItems[12].BackColor = MyCellColor;
                            AdjustListView.Items[nCount - 1].SubItems[11].BackColor = Color.White;
                            AdjustListView.Items[nCount - 1].SubItems[12].BackColor = Color.White;
                        }
                    }
                    break;

                case 4://闭合导线(含外支点)
                    {
                        AdjustListView.Items[nCount].SubItems[1].BackColor = MyCellColor;        //测站

                        //AdjustListView.Items[nCount].SubItems[2].BackColor = Color.GreenYellow;      //角度观测值
                        //if (nCount == 1)
                        //{
                        //    AdjustListView.Items[0].SubItems[2].BackColor = Color.White;
                        //}
                        AdjustListView.Items[nCount].SubItems[2].BackColor = Color.White;      //角度观测值
                        if (nCount >= 1)
                        {
                            AdjustListView.Items[nCount].SubItems[2].BackColor = MyCellColor;
                        }

                        AdjustListView.Items[nCount].SubItems[6].BackColor = Color.White;      //边长
                        if (nCount > 1)
                        {
                            AdjustListView.Items[nCount - 1].SubItems[6].BackColor = MyCellColor;
                        }

                        AdjustListView.Items[nCount].SubItems[11].BackColor = MyCellColor;       //X、Y坐标
                        AdjustListView.Items[nCount].SubItems[12].BackColor = MyCellColor;
                        if (nCount > 1)
                        {
                            AdjustListView.Items[1].SubItems[11].BackColor = MyCellColor;
                            AdjustListView.Items[1].SubItems[12].BackColor = MyCellColor;
                            AdjustListView.Items[nCount - 1].SubItems[11].BackColor = Color.White;
                            AdjustListView.Items[nCount - 1].SubItems[12].BackColor = Color.White;
                        }
                    }
                    break;

                case 5: //具有一个连接角的附和导线
                    {
                        AdjustListView.Items[nCount].SubItems[1].BackColor = MyCellColor;        //测站

                        //AdjustListView.Items[nCount].SubItems[2].BackColor = Color.GreenYellow;      //角度观测值
                        //if (nCount == 1)
                        //{
                        //    AdjustListView.Items[0].SubItems[2].BackColor = Color.White;
                        //}
                        AdjustListView.Items[nCount].SubItems[2].BackColor = Color.White;      //角度观测值
                        if (nCount > 1)
                        {
                            AdjustListView.Items[nCount - 1].SubItems[2].BackColor = MyCellColor;
                        }

                        AdjustListView.Items[nCount].SubItems[4].BackColor = Color.White;      //边长
                        if (nCount > 1)
                        {
                            AdjustListView.Items[nCount - 1].SubItems[4].BackColor = MyCellColor;
                        }

                        AdjustListView.Items[nCount].SubItems[9].BackColor = MyCellColor;       //X、Y坐标
                        AdjustListView.Items[nCount].SubItems[10].BackColor = MyCellColor;
                        if (nCount > 1)
                        {
                            AdjustListView.Items[1].SubItems[9].BackColor = MyCellColor;
                            AdjustListView.Items[1].SubItems[10].BackColor = MyCellColor;
                            AdjustListView.Items[nCount - 1].SubItems[9].BackColor = Color.White;
                            AdjustListView.Items[nCount - 1].SubItems[10].BackColor = Color.White;
                        }
                    }
                    break;

                case 6: //具有两个连接角的附和导线
                    {
                        AdjustListView.Items[nCount].SubItems[1].BackColor = MyCellColor;        //测站

                        //AdjustListView.Items[nCount].SubItems[2].BackColor = Color.GreenYellow;      //角度观测值
                        //if (nCount == 1)
                        //{
                        //    AdjustListView.Items[0].SubItems[2].BackColor = Color.White;
                        //}
                        AdjustListView.Items[nCount].SubItems[2].BackColor = Color.White;      //角度观测值
                        if (nCount > 1)
                        {
                            AdjustListView.Items[nCount - 1].SubItems[2].BackColor = MyCellColor;
                        }

                        AdjustListView.Items[nCount].SubItems[6].BackColor = Color.White;      //边长
                        if (nCount > 3)
                        {

                            AdjustListView.Items[nCount - 2].SubItems[6].BackColor = MyCellColor;
                            AdjustListView.Items[nCount - 3].SubItems[6].BackColor = MyCellColor;
                        }

                        AdjustListView.Items[nCount].SubItems[11].BackColor = MyCellColor;       //X、Y坐标
                        AdjustListView.Items[nCount].SubItems[12].BackColor = MyCellColor;
                        if (nCount > 3)
                        {
                            AdjustListView.Items[0].SubItems[11].BackColor = MyCellColor;
                            AdjustListView.Items[0].SubItems[12].BackColor = MyCellColor;
                            AdjustListView.Items[1].SubItems[11].BackColor = MyCellColor;
                            AdjustListView.Items[1].SubItems[12].BackColor = MyCellColor;
                            AdjustListView.Items[nCount - 2].SubItems[11].BackColor = Color.White;
                            AdjustListView.Items[nCount - 2].SubItems[12].BackColor = Color.White;
                        }
                    }
                    break;

                case 7: //支导线
                    {
                        AdjustListView.Items[nCount].SubItems[1].BackColor = MyCellColor;        //测站

                        //AdjustListView.Items[nCount].SubItems[2].BackColor = Color.GreenYellow;      //角度观测值
                        //if (nCount == 1)
                        //{
                        //    AdjustListView.Items[0].SubItems[2].BackColor = Color.White;
                        //}
                        AdjustListView.Items[nCount].SubItems[2].BackColor = Color.White;      //角度观测值
                        if (nCount > 1)
                        {
                            AdjustListView.Items[nCount - 1].SubItems[2].BackColor = MyCellColor;
                        }

                        AdjustListView.Items[nCount].SubItems[4].BackColor = Color.White;      //边长
                        if (nCount > 1)
                        {
                            AdjustListView.Items[nCount - 1].SubItems[4].BackColor = MyCellColor;
                        }

                        if (nCount > 1)
                        {
                            AdjustListView.Items[0].SubItems[7].BackColor = MyCellColor;
                            AdjustListView.Items[0].SubItems[8].BackColor = MyCellColor;
                            AdjustListView.Items[1].SubItems[7].BackColor = MyCellColor;
                            AdjustListView.Items[1].SubItems[8].BackColor = MyCellColor;
                        }
                    }
                    break;

                case 8: //无连接角导线
                    {
                        AdjustListView.Items[nCount].SubItems[1].BackColor = MyCellColor;        //测站

                        //AdjustListView.Items[nCount].SubItems[2].BackColor = Color.GreenYellow;      //角度观测值
                        //if (nCount == 1)
                        //{
                        //    AdjustListView.Items[0].SubItems[2].BackColor = Color.White;
                        //}
                        AdjustListView.Items[nCount].SubItems[2].BackColor = Color.White;      //角度观测值
                        if (nCount > 1)
                        {
                            AdjustListView.Items[nCount - 1].SubItems[2].BackColor = MyCellColor;
                        }

                        AdjustListView.Items[nCount].SubItems[3].BackColor = Color.White;      //边长
                        if (nCount >= 1)
                        {
                            AdjustListView.Items[nCount - 1].SubItems[3].BackColor = MyCellColor;
                        }

                        AdjustListView.Items[0].SubItems[4].BackColor = MyCellColor;      //假定坐标方位角

                        AdjustListView.Items[nCount].SubItems[9].BackColor = MyCellColor;      //坐标
                        AdjustListView.Items[nCount].SubItems[10].BackColor = MyCellColor;      //坐标
                        if (nCount > 1)
                        {
                            AdjustListView.Items[nCount - 1].SubItems[9].BackColor = Color.White;
                            AdjustListView.Items[nCount - 1].SubItems[10].BackColor = Color.White;
                        }
                    }
                    break;

                case 9: //水准网
                    {
                        for (int i = 0; i < nCount; i++)
                        {
                            if (i % 2 == 1)
                            {
                                AdjustListView.Items[i].SubItems[0].BackColor = Color.WhiteSmoke;
                                AdjustListView.Items[i].SubItems[1].BackColor = Color.WhiteSmoke;
                                AdjustListView.Items[i].SubItems[2].BackColor = Color.WhiteSmoke;
                                AdjustListView.Items[i].SubItems[3].BackColor = Color.WhiteSmoke;
                                AdjustListView.Items[i].SubItems[4].BackColor = Color.WhiteSmoke;
                            }
                        }
                    }
                    break;
            }

            AdjustListView.EnsureVisible(nCount);    //确保焦点显示到最后一行

            int nnCount = this.AdjustListView.Items.Count;      //晕死，非得在这重新统计一次行数才可以。直接用nCount不对！
            for (int i = 0; i < nnCount; i++)
            {
                AdjustListView.Items[i].SubItems[0].Text = (i + 1).ToString();              //现在把新的序号写入第一列
            }

            AdjustListView.Refresh();
        }

        public void toolStripButton_Delete_Click(object sender, EventArgs e)           //删除行
        {
            if (AdjustListView.SelectedItems.Count > 0)
            {
                foreach (ListViewItem lvi in AdjustListView.SelectedItems)  //选中项遍历   
                {
                    AdjustListView.Items.RemoveAt(lvi.Index); // 按索引移除
                    //AdjustListView.Items.Remove(lvi);   //按项移除，两种方法都可以.   
                }
            }
            else
            {
                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                {
                    MessageBox.Show("请选择要删除的行[按Ctrl键可多选]。", "Aurora智能提示");
                }
                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                {
                    MessageBox.Show("請選擇要刪除的行[按Ctrl鍵可多選]。", "Aurora智慧提示");
                }
                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                {
                    MessageBox.Show("Please select the deleting row.[Press Ctrl to multi-select].", "Aurora Intelligent Tips");
                }
            }

            //读取单元格颜色
            RegistryKey MyReg1, RegColor;//声明注册表对象
            MyReg1 = Registry.CurrentUser;//获取当前用户注册表项
            try
            {
                RegColor = MyReg1.CreateSubKey("Software\\Aurora\\Color");//在注册表项中创建子项
                this.MyCellColor = ColorTranslator.FromHtml(RegColor.GetValue("CellColor").ToString());//显示注册表的位置
            }
            catch { }

            int nnCount = AdjustListView.Items.Count;
            for (int i = 0; i < nnCount; i++)
            {
                AdjustListView.Items[i].SubItems[0].Text = (i + 1).ToString();
            }
            //this.AdjustListView.Refresh();
        }

        public void toolStripButton_Clear_Click(object sender, EventArgs e)            //清空列表
        {
            if (this.AdjustListView.Items.Count > 0)
            {
                DialogResult dr = DialogResult.Yes;
                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                {
                    dr = MessageBox.Show("确定要清空列表数据?", "Aurora智能提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                }
                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                {
                    dr = MessageBox.Show("確定要清空清單資料?", "Aurora智慧提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                }
                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                {
                    dr = MessageBox.Show("Are you sure to clear all data?", "Aurora Intelligent Tips", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                }

                if (dr == DialogResult.Yes)
                {
                    AdjustListView.Items.Clear();
                    textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                    textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                    nCalcFlag = 0;              //将计算完成标志设置为0.
                }
                else
                {
                    return;
                }
            }
            
        }

        public int nLevelingObsNum = 0;        //将列表的统计行数总动填到观测值总数中

        public void toolStripButton_Calc_Click(object sender, EventArgs e)             //平差计算
        {
            try
            {
                int nCount = AdjustListView.Items.Count;

                if (nCount >=1)         //如果行数小于1，则无进度条.
                {
                    ProgressBar fProgressBar = new ProgressBar();           //调用进度条窗体
                    //fProgressBar.MdiParent = this;
                    //fProgressBar.StartPosition = FormStartPosition.CenterScreen;
                    //int fProgressBarX, fProgressBarY;
                    //fProgressBarX = (this.Right - this.Left) / 2 - fProgressBar.Width ;
                    //fProgressBarY = (this.Bottom - this.Top) / 2 - fProgressBar.Height;
                    //fProgressBar.Location = new Point(fProgressBarX, fProgressBarY);
                    fProgressBar.Show();
                    Delay(1234);
                    //MessageBox.Show("平差计算已完成!","提示");
                    fProgressBar.Hide();
                }

                string Zero = "0";

                switch (AdjustType.SelectedIndex)       //选择平差类型，进行对应平差
                {
                    case 0: //闭合水准
                    case 1: //附和水准
                        {
                            if (nCount < 3)
                            {
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show("行数小于3，不满足计算条件。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("行數小於3，不滿足計算條件。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Less than 3 rows, Aurora can't adjust.", "Aurora Intelligent Tips");
                                }
                                return;     //防止程序产生异常而退出
                            }

                            string strStartH = this.AdjustListView.Items[0].SubItems[7].Text;
                            string strEndH = this.AdjustListView.Items[nCount - 1].SubItems[7].Text;

                            if (strStartH == "" || strEndH == "")   //如果起始和结束高程都为空的话，提示输入
                            {
                                strStartH = Zero;
                                strEndH = Zero;
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show("请检查起始高程或者结束高程是否为空。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("請檢查起始高程或者結束高程是否為空。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Please check start or end elevation is null or not.", "Aurora Intelligent Tips");
                                }
                                return;
                            }

                            double dStartH = Convert.ToDouble(strStartH);
                            double dEndH = Convert.ToDouble(strEndH);

                            if (AdjustType.SelectedIndex == 0)
                            {
                                if (dStartH != dEndH)
                                {
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("起始高程和闭合高程不一致，请检查输入的数值或者平差类型。", "Aurora智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("起始高程和閉合高程不一致，請檢查輸入的數值或者平差類型。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Start elevation is not same with end elevation, please check input number or adjust type.", "Aurora Intelligent Tips");
                                    }
                                    return;
                                }
                            }

                            //求出距离、测站数、实测高差的和。
                            string strDist, strStns, strObsLevelDiff, strH;
                            double dDist, dStns, dObsLevelDiff, dH;
                            double dSumDist, dSumStns, dSumObsLevelDiff;

                            strDist = ""; strStns = ""; strObsLevelDiff = "";      //使用前一定先要初始化变量
                            dDist = 0; dStns = 0; dObsLevelDiff = 0; dH = 0;
                            dSumDist = 0; dSumStns = 0; dSumObsLevelDiff = 0;

                            for (int i = 0; i < nCount - 1; i++)
                            {
                                strStns = this.AdjustListView.Items[i].SubItems[3].Text;
                                if (strStns == "")
                                {
                                    this.AdjustListView.Items[i].SubItems[3].Text = "1";      //如果测站数为空，则默认填写为“1”
                                }
                            }

                            for (int i = 0; i < nCount - 1; i++)
                            {
                                strDist = this.AdjustListView.Items[i].SubItems[2].Text;
                                strStns = this.AdjustListView.Items[i].SubItems[3].Text;
                                strObsLevelDiff = this.AdjustListView.Items[i].SubItems[4].Text;

                                //if (strDist == "" || strStns == "" || strObsLevelDiff == "")     //如果单元格为空，则赋值为0；
                                if (strDist == "" || strObsLevelDiff == "")     //如果单元格为空，则赋值为0；
                                {
                                    strDist = Zero; strStns = Zero; strObsLevelDiff = Zero;
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("请检查距离、实测高差是否为空。", "Aurora智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("請檢查距離、實測高差是否為空。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Please check Dist, Obslvldiff is null or not.", "Aurora Intelligent Tips");
                                    }
                                    return;
                                }

                                dDist = Convert.ToDouble(strDist);  //字符串转换为双精度
                                dStns = Convert.ToDouble(strStns);
                                dObsLevelDiff = Convert.ToDouble(strObsLevelDiff);

                                dSumDist = dSumDist + dDist;
                                dSumStns = dSumStns + dStns;
                                dSumObsLevelDiff = dSumObsLevelDiff + dObsLevelDiff;
                            }

                            if (AdjustType.SelectedIndex == 1)      //附和水准闭合差计算。。。闭合水准和符合水准计算基本相同。
                            {
                                dSumObsLevelDiff = dSumObsLevelDiff - (dEndH - dStartH);
                            }

                            //误差分配
                            double dAdjust, dAdjustLevelDiff;      //改正数、改正后高差
                            double dSumAdjustLevelDiff, dSumAdjust;
                            dSumAdjust = -dSumObsLevelDiff * 1000;      //*1000转化为毫米.
                            dSumAdjust = Math.Round(dSumAdjust);        //将双精度的改正数之和四舍五入到最接近的整数

                            double[] a = new double[1000];
                            int nArrayLength;
                            nArrayLength = nCount - 1;

                            for (int i = 0; i < nCount - 1; i++)        //将距离或者测站数存入数组
                            {
                                if (LevelMode.SelectedIndex == 0)
                                {
                                    strDist = this.AdjustListView.Items[i].SubItems[2].Text;
                                    dDist = Convert.ToDouble(strDist);
                                    a[i] = dDist;
                                }

                                if (LevelMode.SelectedIndex == 1)
                                {
                                    strStns = this.AdjustListView.Items[i].SubItems[3].Text;
                                    dStns = Convert.ToDouble(strStns);
                                    a[i] = dStns;
                                }

                            }

                            for (int k = 0; k < nArrayLength; k++)          //将距离或者测站数按从大到小排序
                            {
                                for (int l = k + 1; l < nArrayLength; l++)
                                {
                                    if (a[k] < a[l])
                                    {
                                        double dTemp;
                                        dTemp = a[l];
                                        a[l] = a[k];
                                        a[k] = dTemp;
                                    }
                                }
                            }

                            string strAdjust;
                            int nAdjust = 0;

                            if (LevelMode.SelectedIndex == 0)     //按距离分配改正数
                            {
                                int nTempSumAdjust = 0;
                                for (int i = 0; i < nCount - 1; i++)
                                {
                                    strDist = this.AdjustListView.Items[i].SubItems[2].Text;
                                    dDist = Convert.ToDouble(strDist);
                                    dAdjust = (dDist * dSumAdjust) / dSumDist;      //思想：先按距离权分配。再分配余数
                                    nAdjust = (int)dAdjust;
                                    strAdjust = nAdjust.ToString();
                                    this.AdjustListView.Items[i].SubItems[5].Text = strAdjust;         //先把未改正的改正数填进去
                                    nTempSumAdjust = nTempSumAdjust + nAdjust;          //未改正的改正数之和
                                }

                                int nMod;
                                nMod = (int)dSumAdjust - nTempSumAdjust;

                                for (int j = 0; j < Math.Abs(nMod); j++)
                                {
                                    for (int k = 0; k < nArrayLength; k++)
                                    {
                                        strDist = this.AdjustListView.Items[k].SubItems[2].Text;
                                        dDist = Convert.ToDouble(strDist);
                                        strAdjust = this.AdjustListView.Items[k].SubItems[5].Text;
                                        nAdjust = Convert.ToInt32(strAdjust);
                                        if (a[j] == dDist)
                                        {
                                            if (nAdjust > 0) { nAdjust = nAdjust + 1; }
                                            if (nAdjust < 0) { nAdjust = nAdjust - 1; }
                                            if (nAdjust == 0)
                                            {
                                                if (dSumObsLevelDiff > 0) { nAdjust = nAdjust - 1; }
                                                if (dSumObsLevelDiff < 0) { nAdjust = nAdjust + 1; }
                                            }
                                            strAdjust = nAdjust.ToString();
                                            this.AdjustListView.Items[k].SubItems[5].Text = strAdjust;         //现在把真正的改正数填进去
                                            break;
                                        }
                                    }
                                }
                            }

                            if (LevelMode.SelectedIndex == 1)     //按测站数分配改正数
                            {
                                int nTempSumAdjust = 0;
                                for (int i = 0; i < nCount - 1; i++)
                                {
                                    strStns = this.AdjustListView.Items[i].SubItems[3].Text;
                                    dStns = Convert.ToDouble(strStns);
                                    dAdjust = (dStns * dSumAdjust) / dSumStns;      //思想：先按测站数权分配。再分配余数
                                    nAdjust = (int)dAdjust;
                                    strAdjust = nAdjust.ToString();
                                    this.AdjustListView.Items[i].SubItems[5].Text = strAdjust;         //先把未改正的改正数填进去
                                    nTempSumAdjust = nTempSumAdjust + (int)nAdjust;          //未改正的改正数之和
                                }

                                int nMod;
                                nMod = (int)dSumAdjust - nTempSumAdjust;        //求余数

                                for (int j = 0; j < Math.Abs(nMod); j++)
                                {
                                    for (int k = 0; k < nArrayLength; k++)
                                    {
                                        strStns = this.AdjustListView.Items[k].SubItems[3].Text;
                                        dStns = Convert.ToDouble(strStns);
                                        strAdjust = this.AdjustListView.Items[k].SubItems[5].Text;
                                        nAdjust = Convert.ToInt32(strAdjust);
                                        if (a[j] == dStns)
                                        {
                                            if (nAdjust > 0) { nAdjust = nAdjust + 1; }
                                            if (nAdjust < 0) { nAdjust = nAdjust - 1; }
                                            if (nAdjust == 0)
                                            {
                                                if (dSumObsLevelDiff > 0) { nAdjust = nAdjust - 1; }
                                                if (dSumObsLevelDiff < 0) { nAdjust = nAdjust + 1; }
                                            }
                                            strAdjust = nAdjust.ToString();
                                            this.AdjustListView.Items[k].SubItems[5].Text = strAdjust;         //现在把真正的改正数填进去
                                            break;
                                        }
                                    }
                                }
                            }

                            dSumAdjustLevelDiff = 0;    //初始化
                            for (int i = 0; i < nCount - 1; i++)        //计算改正后高差
                            {
                                strObsLevelDiff = this.AdjustListView.Items[i].SubItems[4].Text;
                                dObsLevelDiff = Convert.ToDouble(strObsLevelDiff);
                                strAdjust = this.AdjustListView.Items[i].SubItems[5].Text;
                                nAdjust = (int)Convert.ToDouble(strAdjust);//*******************************//nAdjust = Convert.ToInt32(strAdjust);

                                dAdjustLevelDiff = dObsLevelDiff + nAdjust / 1000.0;   //*******************************//
                                this.AdjustListView.Items[i].SubItems[6].Text = dAdjustLevelDiff.ToString("#0.0000");       //改正后高差
                                dSumAdjustLevelDiff = dSumAdjustLevelDiff + dAdjustLevelDiff;

                                strH = this.AdjustListView.Items[i].SubItems[7].Text;
                                dH = Convert.ToDouble(strH);
                                dH = dH + dAdjustLevelDiff;
                                if (i < nCount - 2)
                                {
                                    this.AdjustListView.Items[i + 1].SubItems[7].Text = dH.ToString("#0.0000");       //高程
                                }
                            }

                            //********************辅助计算**************************
                            pSumStations = dSumStns.ToString("#0");
                            //if (AdjustType.SelectedIndex == 1)      //附和水准闭合差计算
                            //{
                            //    dSumObsLevelDiff = dSumObsLevelDiff + (dEndH - dStartH);
                            //}
                            pSumObsAngle = dSumObsLevelDiff.ToString("#0.0000");
                            pSumAdjust = dSumAdjust.ToString("#0");
                            pSumAdjustLevelDiff = dSumAdjustLevelDiff.ToString("#0.0000");

                            string strfh = "" , strLength = "" ;
                        
                            //闭合差
                            strfh = (dSumObsLevelDiff * 1000).ToString("#0");       //转化为mm
                            textBox1.Text = strfh;
                            //线路长度
                            strLength = dSumDist.ToString("#0.0000");
                            textBox6.Text = strLength;
                            //限差
                            string strFr = "";
                            double dFr = 0;
                            if (MeasGrade.SelectedIndex == 0)               //二等
                            {
                                if (MeasArea.SelectedIndex == 0)        //平地
                                {
                                    dFr = 4 * Math.Sqrt(dSumDist / 1000);
                                }
                                if (MeasArea.SelectedIndex == 1)        //山地
                                {
                                    dFr = 0;      //  二等，山地：测量规范中无要求，这里自定义。
                                }
                            }
                            if (MeasGrade.SelectedIndex == 1)               //3等
                            {
                                if (MeasArea.SelectedIndex == 0)
                                {
                                    dFr = 12 * Math.Sqrt(dSumDist / 1000);
                                }
                                if (MeasArea.SelectedIndex == 1)
                                {
                                    dFr = 3 * Math.Sqrt(dSumStns);
                                }
                            }
                            if (MeasGrade.SelectedIndex == 2)               //4等
                            {
                                if (MeasArea.SelectedIndex == 0)
                                {
                                    dFr = 20 * Math.Sqrt(dSumDist / 1000);
                                }
                                if (MeasArea.SelectedIndex == 1)
                                {
                                    dFr = 5 * Math.Sqrt(dSumStns);
                                }
                            }
                            if (MeasGrade.SelectedIndex == 3)               //5等
                            {
                                if (MeasArea.SelectedIndex == 0)
                                {
                                    dFr = 30 * Math.Sqrt(dSumDist / 1000);
                                }
                                if (MeasArea.SelectedIndex == 1)
                                {
                                    dFr = 10 * Math.Sqrt(dSumStns);
                                }
                            }
                            strFr = dFr.ToString("#0.0000");
                            textBox2.Text = "±" + strFr;
                            //********************辅助计算**************************

                            if (CheckBox_HighPrecision.Checked == false)     //一般精度要求0.001m
                            {
                                if (Math.Abs(dH - dEndH) > 0.001)
                                {
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("Oh~NO！Aurora刚刚发现，水准线路未闭/附合，请仔细检查您输入的数据。", "Aurora智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("Oh~NO！Aurora剛剛發現，水準線路未閉/附合，請仔細檢查您輸入的資料。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Oh~NO！Aurora has found that leveling not closed, please check input data.", "Aurora Intelligent Tips");
                                    }
                                }
                            }
                            if (CheckBox_HighPrecision.Checked == true)     //高精度选项，精度可达到0.0001m
                            {
                                if (Math.Abs(dH - dEndH) > 0.0001)
                                {
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("Oh~NO！Aurora刚刚发现，水准线路未闭/附合，请仔细检查您输入的数据。", "Aurora智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("Oh~NO！Aurora剛剛發現，水準線路未閉/附合，請仔細檢查您輸入的資料。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Oh~NO！Aurora has found that leveling not closed, please check input data.", "Aurora Intelligent Tips");
                                    }
                                }
                            }

                            log.Info(DateTime.Now.ToString() + "Closed/Annexed Leveling" + sender.ToString() + e.ToString());//写入一条新log
                        }
                        break;
                    
                    case 2: //支水准
                        {
                            if (nCount < 2)
                            {
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show("行数小于2，不满足计算条件。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("行數小於2，不滿足計算條件。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Less than 2 rows, Aurora can't adjust.", "Aurora Intelligent Tips");
                                }
                                return;
                            }

                            string strStartH = this.AdjustListView.Items[0].SubItems[5].Text;
                            if (strStartH == "")   //如果起始高程都为空的话，提示
                            {
                                strStartH = Zero;
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show("请检查起始高程是否为空。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("請檢查起始高程是否為空。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Please check start elevation is null or not.", "Aurora Intelligent Tips");
                                }
                                return;
                            }

                            string strDist, strStns, strObsLevelDiff, strH;
                            double dDist, dStns, dObsLevelDiff, dH;
                            double dSumDist, dSumStns;

                            strDist = ""; strStns = ""; strObsLevelDiff = ""; strH = "";      //使用前一定先要初始化变量
                            dDist = 0; dStns = 0; dObsLevelDiff = 0; dH = 0;
                            dSumDist = 0; dSumStns = 0;

                            for (int i = 0; i < nCount - 1; i++)
                            {
                                strStns = this.AdjustListView.Items[i].SubItems[3].Text;
                                if (strStns == "")
                                {
                                    this.AdjustListView.Items[i].SubItems[3].Text = "1";      //如果测站数为空，则默认填写为“1”
                                }
                            }

                            for (int i = 0; i < nCount - 1; i++)
                            {
                                strDist = this.AdjustListView.Items[i].SubItems[2].Text;
                                strStns = this.AdjustListView.Items[i].SubItems[3].Text;
                                strObsLevelDiff = this.AdjustListView.Items[i].SubItems[4].Text;

                                //if (strDist == "" || strStns == "" || strObsLevelDiff == "")     //如果单元格为空，则赋值为0；
                                if (strDist == "" || strObsLevelDiff == "")     //如果单元格为空，则赋值为0；
                                {
                                    strDist = Zero; strStns = Zero; strObsLevelDiff = Zero;
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("请检查距离、实测高差是否为空。", "Aurora智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("請檢查距離、實測高差是否為空。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Please check Dist,  Obslevdiff is null or not.", "Aurora Intelligent Tips");
                                    }
                                    return;
                                }

                                dDist = Convert.ToDouble(strDist);  //字符串转换为双精度
                                dStns = Convert.ToDouble(strStns);
                                dObsLevelDiff = Convert.ToDouble(strObsLevelDiff);

                                dSumDist = dSumDist + dDist;
                                dSumStns = dSumStns + dStns;


                                strH = this.AdjustListView.Items[i].SubItems[5].Text;
                                dH = Convert.ToDouble(strH);
                                dH = dH + dObsLevelDiff;
                                this.AdjustListView.Items[i + 1].SubItems[5].Text = dH.ToString("#0.0000");       //高程
                            }

                            //********************辅助计算**************************
                            string strLength = "";
                            //线路长度
                            strLength = dSumDist.ToString("#0.0000");
                            textBox6.Text = strLength;
                            //********************辅助计算**************************

                            log.Info(DateTime.Now.ToString() + "Spur Leveling" + sender.ToString() + e.ToString());//写入一条新log
                        }
                        break;

                    case 3: //闭合导线
                        {
                            if (nCount < 4)
                            {
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show("行数小于4，不满足计算条件。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("行數小於4，不滿足計算條件。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Less than 4 rows, Aurora can't adjust.", "Aurora Intelligent Tips");
                                }
                                return;
                            }

                            string strCoorX1 = this.AdjustListView.Items[0].SubItems[11].Text;
                            string strCoorY1 = this.AdjustListView.Items[0].SubItems[12].Text;
                            string strCoorX2 = this.AdjustListView.Items[1].SubItems[11].Text;
                            string strCoorY2 = this.AdjustListView.Items[1].SubItems[12].Text;
                            string strCoorX3 = this.AdjustListView.Items[nCount - 1].SubItems[11].Text;     //最后一点坐标
                            string strCoorY3 = this.AdjustListView.Items[nCount - 1].SubItems[12].Text;
                            if (strCoorX1 == "" || strCoorY1 == "" || strCoorX2 == "" || strCoorY2 == "" || strCoorX3 == "" || strCoorY3 == "")   //如果已知坐标为空的话，提示输入
                            {
                                strCoorX1 = Zero; strCoorY1 = Zero; strCoorX2 = Zero; strCoorY2 = Zero;
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show("请检查闭合导线已知点坐标是否为空。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("請檢查閉合導線已知點座標是否為空。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Please check known coor is null or not.", "Aurora Intelligent Tips");
                                }
                                return;
                            }

                            double dCoorX1 = 0, dCoorY1 = 0, dCoorX2 = 0, dCoorY2 = 0, dCoorX3 = 0, dCoorY3 = 0;
                            dCoorX1 = Convert.ToDouble(strCoorX1);
                            dCoorY1 = Convert.ToDouble(strCoorY1);
                            dCoorX2 = Convert.ToDouble(strCoorX2);
                            dCoorY2 = Convert.ToDouble(strCoorY2);
                            dCoorX3 = Convert.ToDouble(strCoorX3);
                            dCoorY3 = Convert.ToDouble(strCoorY3);

                            if ((Math.Abs(dCoorX1 - dCoorX3) > 0.0001) || (Math.Abs(dCoorY1 - dCoorY3) > 0.0001))   //如果已知坐标为空的话，提示输入
                            {
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show("请检查闭合导线的闭合点坐标是否输入正确。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("請檢查閉合導線的閉合點座標是否輸入正確。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Please check known coor is right or not.", "Aurora Intelligent Tips");
                                }
                                return;
                            }

                            double dStartDist, dStartAzimuth;

                            dStartDist = CoortoDist(dCoorX1, dCoorY1, dCoorX2, dCoorY2);
                            dStartAzimuth = CoortoRad(dCoorX1, dCoorY1, dCoorX2, dCoorY2);
                            dStartAzimuth = RadtoDMS(dStartAzimuth);

                            this.AdjustListView.Items[0].SubItems[5].Text = dStartAzimuth.ToString("#0.0000");
                            this.AdjustListView.Items[0].SubItems[6].Text = dStartDist.ToString("#0.0000");

                            string strObsAngle, strAdjObsAngle, strAzimuth, strLength, strDeltaX, strDeltaY, strAdjDeltaX, strAdjDeltaY, strCoorX, strCoorY;
                            double dObsAngle, dAdjObsAngle, dAzimuth, dLength, dDeltaX, dDeltaY, dAdjDeltaX, dAdjDeltaY, dCoorX, dCoorY;
                            strObsAngle = ""; strAdjObsAngle = ""; strAzimuth = ""; strLength = ""; strDeltaX = ""; strDeltaY = ""; strAdjDeltaX = ""; strAdjDeltaY = ""; strCoorX = ""; strCoorY = "";
                            dObsAngle = 0; dAdjObsAngle = 0; dAzimuth = 0; dLength = 0; dDeltaX = 0; dDeltaY = 0; dAdjDeltaX = 0; dAdjDeltaY = 0; dCoorX = 0; dCoorY = 0;

                            string strAdjust = "", strMod = "";       //每个角度的改正数、余数
                            int nAdjust = 0, nMod = 0;

                            string strSumObsAngle, strSumDeltaX, strSumDeltaY, strSumLength, strFb, strFx, strFy;
                            double dSumObsAngle, dSumDeltaX, dSumDeltaY, dSumLength, dFb, dFx, dFy;
                            strSumObsAngle = ""; strSumDeltaX = ""; strSumDeltaY = ""; strSumLength = ""; strFb = ""; strFx = ""; strFy = "";
                            dSumObsAngle = 0; dSumDeltaX = 0; dSumDeltaY = 0; dSumLength = 0; dFb = 0; dFx = 0; dFy = 0;

                            double[] a = new double[1000];       //定义一个数组，存放观测角度
                            int nArrayLength;
                            nArrayLength = nCount - 1;

                            for (int i = 1; i < nCount - 1; i++)
                            {
                                strObsAngle = this.AdjustListView.Items[i].SubItems[2].Text;
                                strAzimuth = this.AdjustListView.Items[i - 1].SubItems[5].Text;
                                if (strObsAngle == "")   //如果观测角度为空的话，提示输入
                                {
                                    strObsAngle = Zero;
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("请检查观测角度是否为空。", "Aurora智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("請檢查觀測角度是否為空。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Please check Meas Angle is null or not.", "Aurora Intelligent Tips");
                                    }
                                    return;
                                }

                                dObsAngle = Convert.ToDouble(strObsAngle);

                                dObsAngle = DMStoDec(dObsAngle);
                                dAzimuth = DMStoDec(Convert.ToDouble(strAzimuth));      //将角度ddd.mmss转化为小数度进行计算

                                if (AngleMode.SelectedIndex == 0)
                                {
                                    dAzimuth = dAzimuth + dObsAngle - 180;
                                }
                                if (AngleMode.SelectedIndex == 1)
                                {
                                    dAzimuth = dAzimuth - dObsAngle + 180;
                                }

                                if (dAzimuth >= 360)
                                {
                                    dAzimuth = dAzimuth - 360;
                                }
                                if (dAzimuth <= 0)
                                {
                                    dAzimuth = dAzimuth + 360;
                                }

                                dAzimuth = DectoDMS(dAzimuth);
                                strAzimuth = dAzimuth.ToString("#0.0000");
                                this.AdjustListView.Items[i].SubItems[5].Text = strAzimuth;     //先将未改正的方位角写入ListView
                            }

                            dObsAngle = 0;
                            for (int i = 1; i < nCount; i++)        //求出闭合多边形内角和
                            {
                                strObsAngle = this.AdjustListView.Items[i].SubItems[2].Text;
                                dObsAngle = Convert.ToDouble(strObsAngle);
                                a[i - 1] = dObsAngle;        //将观测角度以ddd.mmss的形式存入数组
                                dObsAngle = DMStoS(dObsAngle);
                                dSumObsAngle = dSumObsAngle + dObsAngle;
                            }

                            dFb = dSumObsAngle - (nCount - 3) * 180 * 3600;     //求出方位角闭合差 

                            if (Math.Abs(dFb) > 0.00001)        //充分判断dFb不为0，但是又非常接近于0的情况。
                            {
                                if (dFb > 0)
                                {
                                    dFb = dSumObsAngle - (nCount - 3) * 180 * 3600 + 0.00001;       //求出方位角闭合差 +/-.0.00001是为了凑成整数
                                }
                                if (dFb < 0)
                                {
                                    dFb = dSumObsAngle - (nCount - 3) * 180 * 3600 - 0.00001;       //求出方位角闭合差 
                                }
                            }

                            for (int k = 0; k < nArrayLength; k++)              //将观测角度从大到小排序
                            {
                                for (int l = k + 1; l < nArrayLength; l++)
                                {
                                    if (a[k] < a[l])
                                    {
                                        double dTemp;
                                        dTemp = a[l];
                                        a[l] = a[k];
                                        a[k] = dTemp;
                                    }
                                }
                            }

                            if (Math.Abs(dFb) > 0.00001)
                            {
                                nAdjust = -Convert.ToInt32((dFb / Math.Abs(dFb))) * Convert.ToInt32(Math.Floor(Math.Abs(dFb) / (nCount - 1)));  //每个角度的改正数、余数
                            }
                            else { nAdjust = 0; }
                            nMod = Convert.ToInt32((dFb) % (nCount - 1));

                            for (int i = 1; i < nCount; i++)
                            {
                                strObsAngle = this.AdjustListView.Items[i].SubItems[2].Text;
                                dObsAngle = DMStoS(Convert.ToDouble(strObsAngle));

                                dAdjObsAngle = dObsAngle + nAdjust;
                                dAdjObsAngle = StoDMS(dAdjObsAngle);

                                strAdjust = nAdjust.ToString();
                                strAdjObsAngle = dAdjObsAngle.ToString("#0.0000");

                                this.AdjustListView.Items[i].SubItems[3].Text = strAdjust;      //先把未改正的改正数填进去
                                this.AdjustListView.Items[i].SubItems[4].Text = strAdjObsAngle;
                            }

                            for (int j = 0; j < Math.Abs(nMod); j++)
                            {
                                for (int k = 1; k <= nArrayLength; k++)
                                {
                                    strObsAngle = this.AdjustListView.Items[k].SubItems[2].Text;
                                    dObsAngle = DMStoS(Convert.ToDouble(strObsAngle));
                                    strAdjust = this.AdjustListView.Items[k].SubItems[3].Text;
                                    nAdjust = Convert.ToInt32(strAdjust);
                                    if (DMStoS(a[j]) == dObsAngle)
                                    {
                                        if (nAdjust > 0) { nAdjust = nAdjust + 1; }
                                        if (nAdjust < 0) { nAdjust = nAdjust - 1; }
                                        if (nAdjust == 0)
                                        {
                                            if (dFb > 0) { nAdjust = nAdjust - 1; }
                                            if (dFb < 0) { nAdjust = nAdjust + 1; }
                                        }
                                        strAdjust = nAdjust.ToString();
                                        this.AdjustListView.Items[k].SubItems[3].Text = strAdjust;         //现在把真正的改正数填进去
                                        break;
                                    }
                                }
                            }

                            for (int i = 1; i < nCount; i++)
                            {
                                strObsAngle = this.AdjustListView.Items[i].SubItems[2].Text;
                                dObsAngle = DMStoS(Convert.ToDouble(strObsAngle));
                                strAdjust = this.AdjustListView.Items[i].SubItems[3].Text;
                                nAdjust = Convert.ToInt32(strAdjust);
                                strAzimuth = this.AdjustListView.Items[i - 1].SubItems[5].Text;
                                dAzimuth = DMStoS(Convert.ToDouble(strAzimuth));

                                dAdjObsAngle = dObsAngle + nAdjust;

                                if (AngleMode.SelectedIndex == 0)
                                {
                                    dAzimuth = dAzimuth + dAdjObsAngle - 180 * 3600;
                                }
                                if (AngleMode.SelectedIndex == 1)
                                {
                                    dAzimuth = dAzimuth - dAdjObsAngle + 180 * 3600;
                                }

                                if (dAzimuth >= 360 * 3600)
                                {
                                    dAzimuth = dAzimuth - 360 * 3600;
                                }
                                if (dAzimuth <= 0)
                                {
                                    dAzimuth = dAzimuth + 360 * 3600;
                                }

                                dAdjObsAngle = StoDMS(dAdjObsAngle);
                                dAzimuth = StoDMS(dAzimuth);

                                strAdjObsAngle = dAdjObsAngle.ToString("#0.0000");
                                this.AdjustListView.Items[i].SubItems[4].Text = strAdjObsAngle;     //现在把真正的改正后角度填进去

                                if (i < nCount - 1)
                                {
                                    strAzimuth = dAzimuth.ToString("#0.0000");
                                    this.AdjustListView.Items[i].SubItems[5].Text = strAzimuth;     //现在把真正的方位角填进去
                                }
                            }

                            for (int i = 1; i < nCount - 1; i++)
                            {
                                strAzimuth = this.AdjustListView.Items[i].SubItems[5].Text;
                                strLength = this.AdjustListView.Items[i].SubItems[6].Text;
                                if (strLength == "")   //如果边长为空的话，提示输入
                                {
                                    strLength = Zero;
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("请检查边长是否为空。", "Aurora智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("請檢查邊長是否為空。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Please check Length is null or not.", "Aurora Intelligent Tips");
                                    }
                                    return;
                                }
                                dAzimuth = DMStoRad(Convert.ToDouble(strAzimuth));      //将角度ddd.mmss转化为弧度，进行三角函数计算
                                dLength = Convert.ToDouble(strLength);

                                dDeltaX = dLength * Math.Cos(dAzimuth);     //计算出未改正的DeltaX、DeltaY
                                dDeltaY = dLength * Math.Sin(dAzimuth);

                                dSumDeltaX = dSumDeltaX + dDeltaX;
                                dSumDeltaY = dSumDeltaY + dDeltaY;
                                dSumLength = dSumLength + dLength;

                                strDeltaX = dDeltaX.ToString("#0.0000");
                                strDeltaY = dDeltaY.ToString("#0.0000");
                                this.AdjustListView.Items[i].SubItems[7].Text = strDeltaX;
                                this.AdjustListView.Items[i].SubItems[8].Text = strDeltaY;
                            }

                            dFx = dCoorX2 + dSumDeltaX - dCoorX3;       //求出dFx、dFy
                            dFy = dCoorY2 + dSumDeltaY - dCoorY3;

                            for (int i = 1; i < nCount - 1; i++)
                            {
                                strLength = this.AdjustListView.Items[i].SubItems[6].Text;
                                strDeltaX = this.AdjustListView.Items[i].SubItems[7].Text;
                                strDeltaY = this.AdjustListView.Items[i].SubItems[8].Text;
                                dLength = Convert.ToDouble(strLength);
                                dDeltaX = Convert.ToDouble(strDeltaX);
                                dDeltaY = Convert.ToDouble(strDeltaY);

                                dAdjDeltaX = -(dLength * dFx / dSumLength) + dDeltaX;       //改正后DeltaX、DeltaY
                                dAdjDeltaY = -(dLength * dFy / dSumLength) + dDeltaY;

                                strAdjDeltaX = dAdjDeltaX.ToString("#0.0000");
                                strAdjDeltaY = dAdjDeltaY.ToString("#0.0000");
                                this.AdjustListView.Items[i].SubItems[9].Text = strAdjDeltaX;
                                this.AdjustListView.Items[i].SubItems[10].Text = strAdjDeltaY;

                                strCoorX = this.AdjustListView.Items[i].SubItems[11].Text;
                                strCoorY = this.AdjustListView.Items[i].SubItems[12].Text;
                                dCoorX = Convert.ToDouble(strCoorX);
                                dCoorY = Convert.ToDouble(strCoorY);

                                dCoorX = dCoorX + dAdjDeltaX;      //计算改正后X、Y坐标
                                dCoorY = dCoorY + dAdjDeltaY;

                                if (i < nCount - 2)
                                {
                                    strCoorX = dCoorX.ToString("#0.0000");
                                    strCoorY = dCoorY.ToString("#0.0000");
                                    this.AdjustListView.Items[i + 1].SubItems[11].Text = strCoorX;
                                    this.AdjustListView.Items[i + 1].SubItems[12].Text = strCoorY;
                                }
                            }

                            //********************辅助计算**************************
                            pSumObsAngle = StoBigDMS(dSumObsAngle).ToString("#0.0000");
                            pSumAdjust = dFb.ToString("#0");
                            pSumAdjObsAngle = ((nCount - 3) * 180).ToString("#0.0000");
                            pSumAdjDeltaX = (dSumDeltaX + dFx).ToString("#0.0000");
                            pSumAdjDeltaY = (dSumDeltaY + dFy).ToString("#0.0000");

                            //角度闭合差
                            textBox5.Text = dFb.ToString("#0");
                            //线路长度
                            textBox6.Text = dSumLength.ToString("#0.0000");
                            //限差
                            string strFr = "";
                            double dFr = 0;

                            if (MeasGrade.SelectedIndex == 0)               //三等
                            {
                                  dFr = 3.6 * Math.Sqrt(nCount - 1);
                            }
                            if (MeasGrade.SelectedIndex == 1)               //四等
                            {
                                dFr = 5 * Math.Sqrt(nCount - 1);
                            }
                            if (MeasGrade.SelectedIndex == 2)               //一级
                            {
                                dFr = 10 * Math.Sqrt(nCount - 1);
                            }
                            if (MeasGrade.SelectedIndex == 3)               //二级
                            {
                                dFr = 16 * Math.Sqrt(nCount - 1);
                            }
                            if (MeasGrade.SelectedIndex == 4)               //三等
                            {
                                dFr = 24 * Math.Sqrt(nCount - 1);
                            }
                            strFr = dFr.ToString("#0.0");
                            textBox2.Text = "±" + strFr;
                            //X坐标闭合差
                            textBox3.Text = dFx.ToString("#0.0000");
                            //Y坐标闭合差
                            textBox7.Text = dFy.ToString("#0.0000");
                            //全长闭合差
                            double dF = 0;
                            dF = Math.Sqrt(dFx * dFx + dFy * dFy);
                            textBox4.Text = dF.ToString("#0.0000");
                            //精度评定
                            int nPrecision;
                            nPrecision = Convert.ToInt32(dSumLength / dF);
                            textBox8.Text = nPrecision.ToString();
                            //********************辅助计算**************************

                            //********************写入公共变量**********************
                            pfb = dFb.ToString("#0");
                            pfx = dFx.ToString("#0.0000");
                            pfy = dFy.ToString("#0.0000");
                            pf = dF.ToString("#0.0000");
                            pK = nPrecision.ToString("#0");
                            //********************写入公共变量**********************

                            if (Math.Abs(dCoorX - dCoorX3) >= 0.001 || Math.Abs(dCoorY - dCoorY3) >= 0.001)//判断计算精度可以设置高精度选项0.0001m
                            {
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show("Oh~NO！Aurora刚刚发现，闭合导线在最后闭合坐标时计算错误，请检查你的数据。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("Oh~NO！Aurora剛剛發現，閉合導線在最後閉合座標時計算錯誤，請檢查你的資料。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Oh~NO！Aurora has found that a calculate error occures while closing line, check your data please.", "Aurora Intelligent Tips");
                                }
                            }

                            log.Info(DateTime.Now.ToString() + "Closed Traverse" + sender.ToString() + e.ToString());//写入一条新log
                        }
                        break;

                    case 4: //闭合导线(含外支点)
                        {
                            if (nCount < 5)
                            {
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")        //至少有两个已知点、两个未知点
                                {
                                    MessageBox.Show("行数小于5，不满足计算条件。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("行數小於5，不滿足計算條件。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Less than 5 rows, Aurora can't adjust.", "Aurora Intelligent Tips");
                                }
                                return;
                            }

                            string strCoorX1 = this.AdjustListView.Items[0].SubItems[11].Text;
                            string strCoorY1 = this.AdjustListView.Items[0].SubItems[12].Text;
                            string strCoorX2 = this.AdjustListView.Items[1].SubItems[11].Text;
                            string strCoorY2 = this.AdjustListView.Items[1].SubItems[12].Text;
                            string strCoorX3 = this.AdjustListView.Items[nCount - 1].SubItems[11].Text;     //最后一点坐标
                            string strCoorY3 = this.AdjustListView.Items[nCount - 1].SubItems[12].Text;
                            if (strCoorX1 == "" || strCoorY1 == "" || strCoorX2 == "" || strCoorY2 == "" || strCoorX3 == "" || strCoorY3 == "")   //如果已知坐标为空的话，提示输入
                            {
                                strCoorX1 = Zero; strCoorY1 = Zero; strCoorX2 = Zero; strCoorY2 = Zero;
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show("请检查闭合导线已知点坐标是否为空。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("請檢查閉合導線已知點座標是否為空。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Please check known coor is null or not.", "Aurora Intelligent Tips");
                                }
                                return;
                            }

                            double dCoorX1 = 0, dCoorY1 = 0, dCoorX2 = 0, dCoorY2 = 0, dCoorX3 = 0, dCoorY3 = 0;
                            dCoorX1 = Convert.ToDouble(strCoorX1);
                            dCoorY1 = Convert.ToDouble(strCoorY1);
                            dCoorX2 = Convert.ToDouble(strCoorX2);
                            dCoorY2 = Convert.ToDouble(strCoorY2);
                            dCoorX3 = Convert.ToDouble(strCoorX3);
                            dCoorY3 = Convert.ToDouble(strCoorY3);

                            if ((Math.Abs(dCoorX2 - dCoorX3) > 0.0001) || (Math.Abs(dCoorY2 - dCoorY3) > 0.0001))   //如果已知坐标为空的话，提示输入
                            {
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show("请检查闭合导线的闭合点坐标是否输入正确。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("請檢查閉合導線的閉合點座標是否輸入正確。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Please check known coor is right or not.", "Aurora Intelligent Tips");
                                }
                                return;
                            }

                            double dStartDist, dStartAzimuth;

                            dStartDist = CoortoDist(dCoorX1, dCoorY1, dCoorX2, dCoorY2);
                            dStartAzimuth = CoortoRad(dCoorX1, dCoorY1, dCoorX2, dCoorY2);
                            dStartAzimuth = RadtoDMS(dStartAzimuth);

                            this.AdjustListView.Items[0].SubItems[5].Text = dStartAzimuth.ToString("#0.0000");
                            this.AdjustListView.Items[0].SubItems[6].Text = dStartDist.ToString("#0.0000");

                            string strObsAngle, strAdjObsAngle, strAzimuth, strLength, strDeltaX, strDeltaY, strAdjDeltaX, strAdjDeltaY, strCoorX, strCoorY;
                            double dObsAngle, dAdjObsAngle, dAzimuth, dLength, dDeltaX, dDeltaY, dAdjDeltaX, dAdjDeltaY, dCoorX, dCoorY;
                            strObsAngle = ""; strAdjObsAngle = ""; strAzimuth = ""; strLength = ""; strDeltaX = ""; strDeltaY = ""; strAdjDeltaX = ""; strAdjDeltaY = ""; strCoorX = ""; strCoorY = "";
                            dObsAngle = 0; dAdjObsAngle = 0; dAzimuth = 0; dLength = 0; dDeltaX = 0; dDeltaY = 0; dAdjDeltaX = 0; dAdjDeltaY = 0; dCoorX = 0; dCoorY = 0;

                            string strAdjust = "", strMod = "";       //每个角度的改正数、余数
                            int nAdjust = 0, nMod = 0;

                            string strSumObsAngle, strSumDeltaX, strSumDeltaY, strSumLength, strFb, strFx, strFy;
                            double dSumObsAngle, dSumDeltaX, dSumDeltaY, dSumLength, dFb, dFx, dFy;
                            strSumObsAngle = ""; strSumDeltaX = ""; strSumDeltaY = ""; strSumLength = ""; strFb = ""; strFx = ""; strFy = "";
                            dSumObsAngle = 0; dSumDeltaX = 0; dSumDeltaY = 0; dSumLength = 0; dFb = 0; dFx = 0; dFy = 0;

                            double[] a = new double[1000];       //定义一个数组，存放观测角度
                            int nArrayLength;
                            nArrayLength = nCount - 1;

                            for (int i = 1; i < nCount - 1; i++)
                            {
                                strObsAngle = this.AdjustListView.Items[i].SubItems[2].Text;
                                strAzimuth = this.AdjustListView.Items[i - 1].SubItems[5].Text;
                                if (strObsAngle == "")   //如果观测角度为空的话，提示输入
                                {
                                    strObsAngle = Zero;
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("请检查观测角度是否为空。", "Aurora智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("請檢查觀測角度是否為空。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Please check Meas Angle is null or not.", "Aurora Intelligent Tips");
                                    }
                                    return;
                                }

                                dObsAngle = Convert.ToDouble(strObsAngle);

                                dObsAngle = DMStoDec(dObsAngle);
                                dAzimuth = DMStoDec(Convert.ToDouble(strAzimuth));      //将角度ddd.mmss转化为小数度进行计算

                                if (AngleMode.SelectedIndex == 0)
                                {
                                    dAzimuth = dAzimuth + dObsAngle - 180;
                                }
                                if (AngleMode.SelectedIndex == 1)
                                {
                                    dAzimuth = dAzimuth - dObsAngle + 180;
                                }

                                if (dAzimuth >= 360)
                                {
                                    dAzimuth = dAzimuth - 360;
                                }
                                if (dAzimuth <= 0)
                                {
                                    dAzimuth = dAzimuth + 360;
                                }

                                dAzimuth = DectoDMS(dAzimuth);
                                strAzimuth = dAzimuth.ToString("#0.0000");
                                this.AdjustListView.Items[i].SubItems[5].Text = strAzimuth;     //先将未改正的方位角写入ListView
                            }

                            dObsAngle = 0;
                            for (int i = 2; i < nCount; i++)        //求出闭合多边形内角和
                            {
                                strObsAngle = this.AdjustListView.Items[i].SubItems[2].Text;
                                dObsAngle = Convert.ToDouble(strObsAngle);
                                a[i - 2] = dObsAngle;        //将观测角度以ddd.mmss的形式存入数组
                                dObsAngle = DMStoS(dObsAngle);
                                dSumObsAngle = dSumObsAngle + dObsAngle;
                            }

                            dFb = dSumObsAngle - (nCount - 4) * 180 * 3600;     //求出方位角闭合差 

                            if (Math.Abs(dFb) > 0.00001)        //充分判断dFb不为0，但是又非常接近于0的情况。
                            {
                                if (dFb > 0)
                                {
                                    dFb = dSumObsAngle - (nCount - 4) * 180 * 3600 + 0.00001;       //求出方位角闭合差 +/-.0.00001是为了凑成整数
                                }
                                if (dFb < 0)
                                {
                                    dFb = dSumObsAngle - (nCount - 4) * 180 * 3600 - 0.00001;       //求出方位角闭合差 
                                }
                            }

                            for (int k = 0; k < nArrayLength; k++)              //将观测角度从大到小排序
                            {
                                for (int l = k + 1; l < nArrayLength; l++)
                                {
                                    if (a[k] < a[l])
                                    {
                                        double dTemp;
                                        dTemp = a[l];
                                        a[l] = a[k];
                                        a[k] = dTemp;
                                    }
                                }
                            }

                            if (Math.Abs(dFb) > 0.00001)
                            {
                                nAdjust = -Convert.ToInt32((dFb / Math.Abs(dFb))) * Convert.ToInt32(Math.Floor(Math.Abs(dFb) / (nCount - 2)));  //每个角度的改正数、余数
                            }
                            else { nAdjust = 0; }
                            nMod = Convert.ToInt32((dFb) % (nCount - 2));

                            this.AdjustListView.Items[1].SubItems[3].Text = "0";
                            this.AdjustListView.Items[1].SubItems[4].Text = this.AdjustListView.Items[1].SubItems[2].Text;

                            for (int i = 2; i < nCount; i++)
                            {
                                strObsAngle = this.AdjustListView.Items[i].SubItems[2].Text;
                                dObsAngle = DMStoS(Convert.ToDouble(strObsAngle));

                                dAdjObsAngle = dObsAngle + nAdjust;
                                dAdjObsAngle = StoDMS(dAdjObsAngle);

                                strAdjust = nAdjust.ToString();
                                strAdjObsAngle = dAdjObsAngle.ToString("#0.0000");

                                this.AdjustListView.Items[i].SubItems[3].Text = strAdjust;      //先把未改正的改正数填进去
                                this.AdjustListView.Items[i].SubItems[4].Text = strAdjObsAngle;
                            }

                            for (int j = 0; j < Math.Abs(nMod); j++)
                            {
                                for (int k = 1; k < nArrayLength; k++)
                                {
                                    strObsAngle = this.AdjustListView.Items[k].SubItems[2].Text;
                                    dObsAngle = DMStoS(Convert.ToDouble(strObsAngle));
                                    strAdjust = this.AdjustListView.Items[k].SubItems[3].Text;
                                    nAdjust = Convert.ToInt32(strAdjust);
                                    if (DMStoS(a[j]) == dObsAngle)
                                    {
                                        if (nAdjust > 0) { nAdjust = nAdjust + 1; }
                                        if (nAdjust < 0) { nAdjust = nAdjust - 1; }
                                        if (nAdjust == 0)
                                        {
                                            if (dFb > 0) { nAdjust = nAdjust - 1; }
                                            if (dFb < 0) { nAdjust = nAdjust + 1; }
                                        }
                                        strAdjust = nAdjust.ToString();
                                        this.AdjustListView.Items[k].SubItems[3].Text = strAdjust;         //现在把真正的改正数填进去
                                        break;
                                    }
                                }
                            }

                            for (int i = 1; i < nCount; i++)
                            {
                                strObsAngle = this.AdjustListView.Items[i].SubItems[2].Text;
                                dObsAngle = DMStoS(Convert.ToDouble(strObsAngle));
                                strAdjust = this.AdjustListView.Items[i].SubItems[3].Text;
                                nAdjust = Convert.ToInt32(strAdjust);
                                strAzimuth = this.AdjustListView.Items[i - 1].SubItems[5].Text;
                                dAzimuth = DMStoS(Convert.ToDouble(strAzimuth));

                                dAdjObsAngle = dObsAngle + nAdjust;

                                if (AngleMode.SelectedIndex == 0)
                                {
                                    dAzimuth = dAzimuth + dAdjObsAngle - 180 * 3600;
                                }
                                if (AngleMode.SelectedIndex == 1)
                                {
                                    dAzimuth = dAzimuth - dAdjObsAngle + 180 * 3600;
                                }

                                if (dAzimuth >= 360 * 3600)
                                {
                                    dAzimuth = dAzimuth - 360 * 3600;
                                }
                                if (dAzimuth <= 0)
                                {
                                    dAzimuth = dAzimuth + 360 * 3600;
                                }

                                dAdjObsAngle = StoDMS(dAdjObsAngle);
                                dAzimuth = StoDMS(dAzimuth);

                                strAdjObsAngle = dAdjObsAngle.ToString("#0.0000");
                                this.AdjustListView.Items[i].SubItems[4].Text = strAdjObsAngle;     //现在把真正的改正后角度填进去

                                if (i < nCount - 1)
                                {
                                    strAzimuth = dAzimuth.ToString("#0.0000");
                                    this.AdjustListView.Items[i].SubItems[5].Text = strAzimuth;     //现在把真正的方位角填进去
                                }

                            }

                            for (int i = 1; i < nCount - 1; i++)
                            {
                                strAzimuth = this.AdjustListView.Items[i].SubItems[5].Text;
                                strLength = this.AdjustListView.Items[i].SubItems[6].Text;
                                if (strLength == "")   //如果边长为空的话，提示输入
                                {
                                    strLength = Zero;
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("请检查边长是否为空。", "Aurora智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("請檢查邊長是否為空。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Please check Length is null or not.", "Aurora Intelligent Tips");
                                    }
                                    return;
                                }
                                dAzimuth = DMStoRad(Convert.ToDouble(strAzimuth));      //将角度ddd.mmss转化为弧度，进行三角函数计算
                                dLength = Convert.ToDouble(strLength);

                                dDeltaX = dLength * Math.Cos(dAzimuth);     //计算出未改正的DeltaX、DeltaY
                                dDeltaY = dLength * Math.Sin(dAzimuth);

                                dSumDeltaX = dSumDeltaX + dDeltaX;
                                dSumDeltaY = dSumDeltaY + dDeltaY;
                                dSumLength = dSumLength + dLength;

                                strDeltaX = dDeltaX.ToString("#0.0000");
                                strDeltaY = dDeltaY.ToString("#0.0000");
                                this.AdjustListView.Items[i].SubItems[7].Text = strDeltaX;
                                this.AdjustListView.Items[i].SubItems[8].Text = strDeltaY;
                            }

                            dFx = dCoorX2 + dSumDeltaX - dCoorX3;       //求出dFx、dFy
                            dFy = dCoorY2 + dSumDeltaY - dCoorY3;

                            for (int i = 1; i < nCount - 1; i++)
                            {
                                strLength = this.AdjustListView.Items[i].SubItems[6].Text;
                                strDeltaX = this.AdjustListView.Items[i].SubItems[7].Text;
                                strDeltaY = this.AdjustListView.Items[i].SubItems[8].Text;
                                dLength = Convert.ToDouble(strLength);
                                dDeltaX = Convert.ToDouble(strDeltaX);
                                dDeltaY = Convert.ToDouble(strDeltaY);

                                dAdjDeltaX = -(dLength * dFx / dSumLength) + dDeltaX;       //改正后DeltaX、DeltaY
                                dAdjDeltaY = -(dLength * dFy / dSumLength) + dDeltaY;

                                strAdjDeltaX = dAdjDeltaX.ToString("#0.0000");
                                strAdjDeltaY = dAdjDeltaY.ToString("#0.0000");
                                this.AdjustListView.Items[i].SubItems[9].Text = strAdjDeltaX;
                                this.AdjustListView.Items[i].SubItems[10].Text = strAdjDeltaY;

                                strCoorX = this.AdjustListView.Items[i].SubItems[11].Text;
                                strCoorY = this.AdjustListView.Items[i].SubItems[12].Text;
                                dCoorX = Convert.ToDouble(strCoorX);
                                dCoorY = Convert.ToDouble(strCoorY);

                                dCoorX = dCoorX + dAdjDeltaX;      //计算改正后X、Y坐标
                                dCoorY = dCoorY + dAdjDeltaY;

                                if (i < nCount - 2)
                                {
                                    strCoorX = dCoorX.ToString("#0.0000");
                                    strCoorY = dCoorY.ToString("#0.0000");
                                    this.AdjustListView.Items[i + 1].SubItems[11].Text = strCoorX;
                                    this.AdjustListView.Items[i + 1].SubItems[12].Text = strCoorY;
                                }
                            }

                            //********************辅助计算**************************
                            pSumObsAngle = StoBigDMS(dSumObsAngle).ToString("#0.0000");
                            pSumAdjust = dFb.ToString("#0");
                            pSumAdjObsAngle = ((nCount - 3) * 180).ToString("#0.0000");
                            pSumAdjDeltaX = (dSumDeltaX + dFx).ToString("#0.0000");
                            pSumAdjDeltaY = (dSumDeltaY + dFy).ToString("#0.0000");

                            //角度闭合差
                            textBox5.Text = dFb.ToString("#0");
                            //线路长度
                            textBox6.Text = dSumLength.ToString("#0.0000");
                            //限差
                            string strFr = "";
                            double dFr = 0;

                            if (MeasGrade.SelectedIndex == 0)
                            {
                                dFr = 3.6 * Math.Sqrt(nCount - 2);
                            }
                            if (MeasGrade.SelectedIndex == 1)
                            {
                                dFr = 5 * Math.Sqrt(nCount - 2);
                            }
                            if (MeasGrade.SelectedIndex == 2)
                            {
                                dFr = 10 * Math.Sqrt(nCount - 2);
                            }
                            if (MeasGrade.SelectedIndex == 3)
                            {
                                dFr = 16 * Math.Sqrt(nCount - 2);
                            }
                            if (MeasGrade.SelectedIndex == 4)
                            {
                                dFr = 24 * Math.Sqrt(nCount - 2);
                            }
                            strFr = dFr.ToString("#0.0");
                            textBox2.Text = "±" + strFr;
                            //X坐标闭合差
                            textBox3.Text = dFx.ToString("#0.0000");
                            //Y坐标闭合差
                            textBox7.Text = dFy.ToString("#0.0000");
                            //全长闭合差
                            double dF = 0;
                            dF = Math.Sqrt(dFx * dFx + dFy * dFy);
                            textBox4.Text = dF.ToString("#0.0000");
                            //精度评定
                            int nPrecision;
                            nPrecision = Convert.ToInt32(dSumLength / dF);
                            textBox8.Text = nPrecision.ToString();
                            //********************辅助计算**************************

                            //********************写入公共变量**********************
                            pfb = dFb.ToString("#0");
                            pfx = dFx.ToString("#0.0000");
                            pfy = dFy.ToString("#0.0000");
                            pf = dF.ToString("#0.0000");
                            pK = nPrecision.ToString("#0");
                            //********************写入公共变量**********************

                            if (Math.Abs(dCoorX - dCoorX3) >= 0.001 || Math.Abs(dCoorY - dCoorY3) >= 0.001)//判断计算精度可以设置高精度选项0.0001m
                            {
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show("Oh~NO！Aurora刚刚发现，闭合导线在最后闭合坐标时计算错误，请检查你的数据。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("Oh~NO！Aurora剛剛發現，閉合導線在最後閉合座標時計算錯誤，請檢查你的資料。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Oh~NO！Aurora has found that a calculate error occures while closing line, check your data please.", "Aurora Intelligent Tips");
                                }
                            }

                            log.Info(DateTime.Now.ToString() + "Closed Traverse With Outer Point" + sender.ToString() + e.ToString());//写入一条新log
                        }
                        break;

                    case 5: //具有一个连接角的附和导线
                        {
                            if (nCount < 4)
                            {
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show("行数小于4，不满足计算条件。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("行數小於4，不滿足計算條件。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Less than 4 rows, Aurora can't adjust.", "Aurora Intelligent Tips");
                                }
                                return;
                            }

                            string strCoorX1 = this.AdjustListView.Items[0].SubItems[9].Text;
                            string strCoorY1 = this.AdjustListView.Items[0].SubItems[10].Text;
                            string strCoorX2 = this.AdjustListView.Items[1].SubItems[9].Text;
                            string strCoorY2 = this.AdjustListView.Items[1].SubItems[10].Text;
                            string strCoorX3 = this.AdjustListView.Items[nCount - 1].SubItems[9].Text;     //最后一点坐标
                            string strCoorY3 = this.AdjustListView.Items[nCount - 1].SubItems[10].Text;
                            if (strCoorX1 == "" || strCoorY1 == "" || strCoorX2 == "" || strCoorY2 == "" || strCoorX3 == "" || strCoorY3 == "")   //如果已知坐标为空的话，提示输入
                            {
                                strCoorX1 = Zero; strCoorY1 = Zero; strCoorX2 = Zero; strCoorY2 = Zero;
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show("请检查附和导线已知点坐标是否为空。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("請檢查附和導線已知點座標是否為空。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Please check known coor is null or not.", "Aurora Intelligent Tips");
                                }
                                return;
                            }

                            double dCoorX1 = 0, dCoorY1 = 0, dCoorX2 = 0, dCoorY2 = 0, dCoorX3 = 0, dCoorY3 = 0;
                            dCoorX1 = Convert.ToDouble(strCoorX1);
                            dCoorY1 = Convert.ToDouble(strCoorY1);
                            dCoorX2 = Convert.ToDouble(strCoorX2);
                            dCoorY2 = Convert.ToDouble(strCoorY2);
                            dCoorX3 = Convert.ToDouble(strCoorX3);
                            dCoorY3 = Convert.ToDouble(strCoorY3);

                            double dStartDist, dStartAzimuth;

                            dStartDist = CoortoDist(dCoorX1, dCoorY1, dCoorX2, dCoorY2);
                            dStartAzimuth = CoortoRad(dCoorX1, dCoorY1, dCoorX2, dCoorY2);
                            dStartAzimuth = RadtoDMS(dStartAzimuth);

                            this.AdjustListView.Items[0].SubItems[3].Text = dStartAzimuth.ToString("#0.0000");
                            this.AdjustListView.Items[0].SubItems[4].Text = dStartDist.ToString("#0.0000");

                            string strObsAngle, strAzimuth, strLength, strDeltaX, strDeltaY, strAdjDeltaX, strAdjDeltaY, strCoorX, strCoorY;
                            double dObsAngle, dAzimuth, dLength, dDeltaX, dDeltaY, dAdjDeltaX, dAdjDeltaY, dCoorX, dCoorY;
                            strObsAngle = ""; strAzimuth = ""; strLength = ""; strDeltaX = ""; strDeltaY = ""; strAdjDeltaX = ""; strAdjDeltaY = ""; strCoorX = ""; strCoorY = "";
                            dObsAngle = 0; dAzimuth = 0; dLength = 0; dDeltaX = 0; dDeltaY = 0; dAdjDeltaX = 0; dAdjDeltaY = 0; dCoorX = 0; dCoorY = 0;

                            string strSumDeltaX, strSumDeltaY, strSumLength, strFx, strFy;
                            double dSumDeltaX, dSumDeltaY, dSumLength, dFx, dFy;
                            strSumDeltaX = ""; strSumDeltaY = ""; strSumLength = ""; strFx = ""; strFy = "";
                            dSumDeltaX = 0; dSumDeltaY = 0; dSumLength = 0; dFx = 0; dFy = 0;

                            for (int i = 1; i < nCount - 1; i++)
                            {
                                strObsAngle = this.AdjustListView.Items[i].SubItems[2].Text;
                                strAzimuth = this.AdjustListView.Items[i - 1].SubItems[3].Text;
                                if (strObsAngle == "")   //如果观测角度为空的话，提示输入
                                {
                                    strObsAngle = Zero;
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("请检查观测角度是否为空。", "Aurora智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("請檢查觀測角度是否為空。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Please check Meas Angle is null or not.", "Aurora Intelligent Tips");
                                    }
                                    return;
                                }
                                dObsAngle = DMStoDec(Convert.ToDouble(strObsAngle));
                                dAzimuth = DMStoDec(Convert.ToDouble(strAzimuth));      //将角度ddd.mmss转化为小数度进行计算

                                if (AngleMode.SelectedIndex == 0)
                                {
                                    dAzimuth = dAzimuth + dObsAngle - 180;
                                }
                                if (AngleMode.SelectedIndex == 1)
                                {
                                    dAzimuth = dAzimuth - dObsAngle + 180;
                                }

                                if (dAzimuth >= 360)
                                {
                                    dAzimuth = dAzimuth - 360;
                                }
                                if (dAzimuth <= 0)
                                {
                                    dAzimuth = dAzimuth + 360;
                                }

                                dAzimuth = DectoDMS(dAzimuth);
                                strAzimuth = dAzimuth.ToString("#0.0000");
                                this.AdjustListView.Items[i].SubItems[3].Text = strAzimuth;
                            }

                            for (int i = 1; i < nCount - 1; i++)
                            {
                                strAzimuth = this.AdjustListView.Items[i].SubItems[3].Text;
                                strLength = this.AdjustListView.Items[i].SubItems[4].Text;
                                if (strLength == "")   //如果边长为空的话，提示输入
                                {
                                    strLength = Zero;
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("请检查边长是否为空。", "Aurora智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("請檢查邊長是否為空。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Please check Length is null or not.", "Aurora Intelligent Tips");
                                    }
                                    return;
                                }
                                dAzimuth = DMStoRad(Convert.ToDouble(strAzimuth));      //将角度ddd.mmss转化为弧度，进行三角函数计算
                                dLength = Convert.ToDouble(strLength);

                                dDeltaX = dLength * Math.Cos(dAzimuth);     //计算出未改正的DeltaX、DeltaY
                                dDeltaY = dLength * Math.Sin(dAzimuth);

                                dSumDeltaX = dSumDeltaX + dDeltaX;
                                dSumDeltaY = dSumDeltaY + dDeltaY;
                                dSumLength = dSumLength + dLength;

                                strDeltaX = dDeltaX.ToString("#0.0000");
                                strDeltaY = dDeltaY.ToString("#0.0000");
                                this.AdjustListView.Items[i].SubItems[5].Text = strDeltaX;
                                this.AdjustListView.Items[i].SubItems[6].Text = strDeltaY;
                            }

                            dFx = dCoorX2 + dSumDeltaX - dCoorX3;       //求出dFx、dFy
                            dFy = dCoorY2 + dSumDeltaY - dCoorY3;

                            for (int i = 1; i < nCount - 1; i++)
                            {
                                strLength = this.AdjustListView.Items[i].SubItems[4].Text;
                                strDeltaX = this.AdjustListView.Items[i].SubItems[5].Text;
                                strDeltaY = this.AdjustListView.Items[i].SubItems[6].Text;
                                dLength = Convert.ToDouble(strLength);
                                dDeltaX = Convert.ToDouble(strDeltaX);
                                dDeltaY = Convert.ToDouble(strDeltaY);

                                dAdjDeltaX = -(dLength * dFx / dSumLength) + dDeltaX;       //改正后DeltaX、DeltaY
                                dAdjDeltaY = -(dLength * dFy / dSumLength) + dDeltaY;

                                strAdjDeltaX = dAdjDeltaX.ToString("#0.0000");
                                strAdjDeltaY = dAdjDeltaY.ToString("#0.0000");
                                this.AdjustListView.Items[i].SubItems[7].Text = strAdjDeltaX;
                                this.AdjustListView.Items[i].SubItems[8].Text = strAdjDeltaY;

                                strCoorX = this.AdjustListView.Items[i].SubItems[9].Text;
                                strCoorY = this.AdjustListView.Items[i].SubItems[10].Text;
                                dCoorX = Convert.ToDouble(strCoorX);
                                dCoorY = Convert.ToDouble(strCoorY);

                                dCoorX = dCoorX + dAdjDeltaX;      //计算改正后X、Y坐标
                                dCoorY = dCoorY + dAdjDeltaY;

                                if (i < nCount - 2)
                                {
                                    strCoorX = dCoorX.ToString("#0.0000");
                                    strCoorY = dCoorY.ToString("#0.0000");
                                    this.AdjustListView.Items[i + 1].SubItems[9].Text = strCoorX;
                                    this.AdjustListView.Items[i + 1].SubItems[10].Text = strCoorY;
                                }
                            }

                            //********************辅助计算**************************
                            pSumAdjDeltaX = (dSumDeltaX + dFx).ToString("#0.0000");
                            pSumAdjDeltaY = (dSumDeltaY + dFy).ToString("#0.0000");

                            //线路长度
                            textBox6.Text = dSumLength.ToString("#0.0000");
                            //X坐标闭合差
                            textBox3.Text = dFx.ToString("#0.0000");
                            //Y坐标闭合差
                            textBox7.Text = dFy.ToString("#0.0000");
                            //全长闭合差
                            double dF = 0;
                            dF = Math.Sqrt(dFx * dFx + dFy * dFy);
                            textBox4.Text = dF.ToString("#0.0000");
                            //精度评定
                            int nPrecision;
                            nPrecision = Convert.ToInt32(dSumLength / dF);
                            textBox8.Text = nPrecision.ToString();
                            //********************辅助计算**************************

                            //********************写入公共变量**********************
                            pfx = dFx.ToString("#0.0000");
                            pfy = dFy.ToString("#0.0000");
                            pf = dF.ToString("#0.0000");
                            pK = nPrecision.ToString("#0");
                            //********************写入公共变量**********************

                            if (Math.Abs(dCoorX - dCoorX3) >= 0.001 || Math.Abs(dCoorY - dCoorY3) >= 0.001)//判断计算精度可以设置高精度选项0.0001m
                            {
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show("Oh~NO！Aurora刚刚发现，附合导线坐标在最后附合时计算错误，请检查你的数据。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("Oh~NO！Aurora剛剛發現，附合導線座標在最後附合時計算錯誤，請檢查你的資料。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Oh~NO！Aurora has found that a calculate error occures while closing line, check your data please.", "Aurora Intelligent Tips");
                                }
                            }

                            log.Info(DateTime.Now.ToString() + "One Angle Conn Traverse" + sender.ToString() + e.ToString());//写入一条新log
                        }
                        break;

                    case 6: //具有两个连接角的附和导线
                        {
                            if (nCount < 5)
                            {
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show("行数小于5，不满足计算条件。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("行數小於5，不滿足計算條件。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Less than 5 rows, Aurora can't adjust.", "Aurora Intelligent Tips");
                                }
                                return;
                            }

                            string strCoorX1 = this.AdjustListView.Items[0].SubItems[11].Text;
                            string strCoorY1 = this.AdjustListView.Items[0].SubItems[12].Text;
                            string strCoorX2 = this.AdjustListView.Items[1].SubItems[11].Text;
                            string strCoorY2 = this.AdjustListView.Items[1].SubItems[12].Text;
                            string strCoorX3 = this.AdjustListView.Items[nCount - 2].SubItems[11].Text;     //最后两点坐标
                            string strCoorY3 = this.AdjustListView.Items[nCount - 2].SubItems[12].Text;
                            string strCoorX4 = this.AdjustListView.Items[nCount - 1].SubItems[11].Text;
                            string strCoorY4 = this.AdjustListView.Items[nCount - 1].SubItems[12].Text;
                            if (strCoorX1 == "" || strCoorY1 == "" || strCoorX2 == "" || strCoorY2 == "" || strCoorX3 == "" || strCoorY3 == "" || strCoorX4 == "" || strCoorY4 == "")   //如果已知坐标为空的话，提示输入
                            {
                                strCoorX1 = Zero; strCoorY1 = Zero; strCoorX2 = Zero; strCoorY2 = Zero; strCoorX3 = Zero; strCoorY3 = Zero; strCoorX4 = Zero; strCoorY4 = Zero;
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show("请检查附和导线已知点坐标是否为空。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("請檢查附和導線已知點座標是否為空。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Please check known coor is null or not.", "Aurora Intelligent Tips");
                                }
                                return;
                            }

                            double dCoorX1 = 0, dCoorY1 = 0, dCoorX2 = 0, dCoorY2 = 0, dCoorX3 = 0, dCoorY3 = 0, dCoorX4 = 0, dCoorY4 = 0;
                            dCoorX1 = Convert.ToDouble(strCoorX1);
                            dCoorY1 = Convert.ToDouble(strCoorY1);
                            dCoorX2 = Convert.ToDouble(strCoorX2);
                            dCoorY2 = Convert.ToDouble(strCoorY2);
                            dCoorX3 = Convert.ToDouble(strCoorX3);
                            dCoorY3 = Convert.ToDouble(strCoorY3);
                            dCoorX4 = Convert.ToDouble(strCoorX4);
                            dCoorY4 = Convert.ToDouble(strCoorY4);

                            double dStartDist = 0, dStartAzimuth = 0, dEndDist = 0, dEndAzimuth = 0;

                            dStartDist = CoortoDist(dCoorX1, dCoorY1, dCoorX2, dCoorY2);
                            dStartAzimuth = CoortoRad(dCoorX1, dCoorY1, dCoorX2, dCoorY2);
                            dStartAzimuth = RadtoDMS(dStartAzimuth);
                            dEndDist = CoortoDist(dCoorX3, dCoorY3, dCoorX4, dCoorY4);
                            dEndAzimuth = CoortoRad(dCoorX3, dCoorY3, dCoorX4, dCoorY4);
                            dEndAzimuth = RadtoDMS(dEndAzimuth);

                            this.AdjustListView.Items[0].SubItems[5].Text = dStartAzimuth.ToString("#0.0000");      //求出起始坐标方位角和终止坐标方位角、距离
                            this.AdjustListView.Items[0].SubItems[6].Text = dStartDist.ToString("#0.0000");
                            this.AdjustListView.Items[nCount - 2].SubItems[5].Text = dEndAzimuth.ToString("#0.0000");
                            this.AdjustListView.Items[nCount - 2].SubItems[6].Text = dEndDist.ToString("#0.0000");

                            string strObsAngle, strAdjObsAngle, strAzimuth, strLength, strDeltaX, strDeltaY, strAdjDeltaX, strAdjDeltaY, strCoorX, strCoorY;
                            double dObsAngle, dAdjObsAngle, dAzimuth, dLength, dDeltaX, dDeltaY, dAdjDeltaX, dAdjDeltaY, dCoorX, dCoorY;
                            strObsAngle = ""; strAdjObsAngle = ""; strAzimuth = ""; strLength = ""; strDeltaX = ""; strDeltaY = ""; strAdjDeltaX = ""; strAdjDeltaY = ""; strCoorX = ""; strCoorY = "";
                            dObsAngle = 0; dAdjObsAngle = 0; dAzimuth = 0; dLength = 0; dDeltaX = 0; dDeltaY = 0; dAdjDeltaX = 0; dAdjDeltaY = 0; dCoorX = 0; dCoorY = 0;

                            string strAdjust = "", strMod = "";       //每个角度的改正数、余数
                            int nAdjust = 0, nMod = 0;

                            string strSumDeltaX, strSumDeltaY, strSumLength, strFb, strFx, strFy;
                            double dSumDeltaX, dSumDeltaY, dSumLength, dFb, dFx, dFy;
                            strSumDeltaX = ""; strSumDeltaY = ""; strSumLength = ""; strFb = ""; strFx = ""; strFy = "";
                            dSumDeltaX = 0; dSumDeltaY = 0; dSumLength = 0; dFb = 0; dFx = 0; dFy = 0;

                            double[] a = new double[1000];       //定义一个数组，存放观测角度
                            int nArrayLength;
                            nArrayLength = nCount - 1;

                            for (int i = 1; i < nCount - 1; i++)
                            {
                                strObsAngle = this.AdjustListView.Items[i].SubItems[2].Text;
                                strAzimuth = this.AdjustListView.Items[i - 1].SubItems[5].Text;
                                if (strObsAngle == "")   //如果观测角度为空的话，提示输入
                                {
                                    strObsAngle = Zero;
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("请检查观测角度是否为空。", "Aurora智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("請檢查觀測角度是否為空。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Please check Meas Angle is null or not.", "Aurora Intelligent Tips");
                                    }
                                    return;
                                }

                                dObsAngle = Convert.ToDouble(strObsAngle);
                                a[i - 1] = dObsAngle;        //将观测角度以ddd.mmss的形式存入数组

                                dObsAngle = DMStoDec(dObsAngle);
                                dAzimuth = DMStoDec(Convert.ToDouble(strAzimuth));      //将角度ddd.mmss转化为小数度进行计算

                                if (AngleMode.SelectedIndex == 0)
                                {
                                    dAzimuth = dAzimuth + dObsAngle - 180;
                                }
                                if (AngleMode.SelectedIndex == 1)
                                {
                                    dAzimuth = dAzimuth - dObsAngle + 180;
                                }

                                if (dAzimuth >= 360)
                                {
                                    dAzimuth = dAzimuth - 360;
                                }
                                if (dAzimuth <= 0)
                                {
                                    dAzimuth = dAzimuth + 360;
                                }

                                dAzimuth = DectoDMS(dAzimuth);

                                if (i < nCount - 2)
                                {
                                    strAzimuth = dAzimuth.ToString("#0.0000");
                                    this.AdjustListView.Items[i].SubItems[5].Text = strAzimuth;     //先将未改正的方位角写入ListView
                                }
                            }

                            dFb = DMStoS(dAzimuth) - DMStoS(dEndAzimuth);

                            if (Math.Abs(dFb) > 0.00001)        //充分判断dFb不为0，但是又非常接近于0的情况。
                            {
                                if (dFb > 0)
                                {
                                    dFb = DMStoS(dAzimuth) - DMStoS(dEndAzimuth) + 0.00001;       //求出方位角闭合差 +/-.0.00001是为了凑成整数
                                }
                                if (dFb < 0)
                                {
                                    dFb = DMStoS(dAzimuth) - DMStoS(dEndAzimuth) - 0.00001;       //求出方位角闭合差 
                                }
                            }

                            for (int k = 0; k < nArrayLength; k++)              //将观测角度从大到小排序
                            {
                                for (int l = k + 1; l < nArrayLength; l++)
                                {
                                    if (a[k] < a[l])
                                    {
                                        double dTemp;
                                        dTemp = a[l];
                                        a[l] = a[k];
                                        a[k] = dTemp;
                                    }
                                }
                            }

                            if (Math.Abs(dFb) > 0.00001)
                            {
                                nAdjust = -Convert.ToInt32((dFb / Math.Abs(dFb))) * Convert.ToInt32(Math.Floor(Math.Abs(dFb) / (nCount - 2)));  //每个角度的改正数、余数
                            }
                            else { nAdjust = 0; }
                            nMod = Convert.ToInt32((dFb) % (nCount - 2));

                            for (int i = 1; i < nCount - 1; i++)
                            {
                                strObsAngle = this.AdjustListView.Items[i].SubItems[2].Text;
                                dObsAngle = DMStoS(Convert.ToDouble(strObsAngle));

                                dAdjObsAngle = dObsAngle + nAdjust;
                                dAdjObsAngle = StoDMS(dAdjObsAngle);

                                strAdjust = nAdjust.ToString();
                                strAdjObsAngle = dAdjObsAngle.ToString("#0.0000");

                                this.AdjustListView.Items[i].SubItems[3].Text = strAdjust;      //先把未改正的改正数填进去
                                this.AdjustListView.Items[i].SubItems[4].Text = strAdjObsAngle;
                            }

                            for (int j = 0; j < Math.Abs(nMod); j++)
                            {
                                for (int k = 1; k < nArrayLength; k++)
                                {
                                    strObsAngle = this.AdjustListView.Items[k].SubItems[2].Text;
                                    dObsAngle = DMStoS(Convert.ToDouble(strObsAngle));
                                    strAdjust = this.AdjustListView.Items[k].SubItems[3].Text;
                                    nAdjust = Convert.ToInt32(strAdjust);
                                    if (DMStoS(a[j]) == dObsAngle)
                                    {
                                        if (nAdjust > 0) { nAdjust = nAdjust + 1; }
                                        if (nAdjust < 0) { nAdjust = nAdjust - 1; }
                                        if (nAdjust == 0)
                                        {
                                            if (dFb > 0) { nAdjust = nAdjust - 1; }
                                            if (dFb < 0) { nAdjust = nAdjust + 1; }
                                        }
                                        strAdjust = nAdjust.ToString();
                                        this.AdjustListView.Items[k].SubItems[3].Text = strAdjust;         //现在把真正的改正数填进去
                                        break;
                                    }
                                }
                            }

                            for (int i = 1; i < nCount - 1; i++)
                            {
                                strObsAngle = this.AdjustListView.Items[i].SubItems[2].Text;
                                dObsAngle = DMStoS(Convert.ToDouble(strObsAngle));
                                strAdjust = this.AdjustListView.Items[i].SubItems[3].Text;
                                nAdjust = Convert.ToInt32(strAdjust);
                                strAzimuth = this.AdjustListView.Items[i - 1].SubItems[5].Text;
                                dAzimuth = DMStoS(Convert.ToDouble(strAzimuth));

                                dAdjObsAngle = dObsAngle + nAdjust;

                                if (AngleMode.SelectedIndex == 0)
                                {
                                    dAzimuth = dAzimuth + dAdjObsAngle - 180 * 3600;
                                }
                                if (AngleMode.SelectedIndex == 1)
                                {
                                    dAzimuth = dAzimuth - dAdjObsAngle + 180 * 3600;
                                }

                                if (dAzimuth >= 360 * 3600)
                                {
                                    dAzimuth = dAzimuth - 360 * 3600;
                                }
                                if (dAzimuth <= 0)
                                {
                                    dAzimuth = dAzimuth + 360 * 3600;
                                }

                                dAdjObsAngle = StoDMS(dAdjObsAngle);
                                dAzimuth = StoDMS(dAzimuth);

                                strAdjObsAngle = dAdjObsAngle.ToString("#0.0000");
                                this.AdjustListView.Items[i].SubItems[4].Text = strAdjObsAngle;     //现在把真正的改正后角度填进去

                                if (i < nCount - 2)
                                {
                                    strAzimuth = dAzimuth.ToString("#0.0000");
                                    this.AdjustListView.Items[i].SubItems[5].Text = strAzimuth;     //现在把真正的方位角填进去
                                }

                            }

                            for (int i = 1; i < nCount - 2; i++)
                            {
                                strAzimuth = this.AdjustListView.Items[i].SubItems[5].Text;
                                strLength = this.AdjustListView.Items[i].SubItems[6].Text;
                                if (strLength == "")   //如果边长为空的话，提示输入
                                {
                                    strLength = Zero;
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("请检查边长是否为空。", "Aurora智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("請檢查邊長是否為空。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Please check Length is null or not.", "Aurora Intelligent Tips");
                                    }
                                    return;
                                }
                                dAzimuth = DMStoRad(Convert.ToDouble(strAzimuth));      //将角度ddd.mmss转化为弧度，进行三角函数计算
                                dLength = Convert.ToDouble(strLength);

                                dDeltaX = dLength * Math.Cos(dAzimuth);     //计算出未改正的DeltaX、DeltaY
                                dDeltaY = dLength * Math.Sin(dAzimuth);

                                dSumDeltaX = dSumDeltaX + dDeltaX;
                                dSumDeltaY = dSumDeltaY + dDeltaY;
                                dSumLength = dSumLength + dLength;

                                strDeltaX = dDeltaX.ToString("#0.0000");
                                strDeltaY = dDeltaY.ToString("#0.0000");
                                this.AdjustListView.Items[i].SubItems[7].Text = strDeltaX;
                                this.AdjustListView.Items[i].SubItems[8].Text = strDeltaY;
                            }

                            dFx = dCoorX2 + dSumDeltaX - dCoorX3;       //求出dFx、dFy
                            dFy = dCoorY2 + dSumDeltaY - dCoorY3;

                            for (int i = 1; i < nCount - 2; i++)
                            {
                                strLength = this.AdjustListView.Items[i].SubItems[6].Text;
                                strDeltaX = this.AdjustListView.Items[i].SubItems[7].Text;
                                strDeltaY = this.AdjustListView.Items[i].SubItems[8].Text;
                                dLength = Convert.ToDouble(strLength);
                                dDeltaX = Convert.ToDouble(strDeltaX);
                                dDeltaY = Convert.ToDouble(strDeltaY);

                                dAdjDeltaX = -(dLength * dFx / dSumLength) + dDeltaX;       //改正后DeltaX、DeltaY
                                dAdjDeltaY = -(dLength * dFy / dSumLength) + dDeltaY;

                                strAdjDeltaX = dAdjDeltaX.ToString("#0.0000");
                                strAdjDeltaY = dAdjDeltaY.ToString("#0.0000");
                                this.AdjustListView.Items[i].SubItems[9].Text = strAdjDeltaX;
                                this.AdjustListView.Items[i].SubItems[10].Text = strAdjDeltaY;

                                strCoorX = this.AdjustListView.Items[i].SubItems[11].Text;
                                strCoorY = this.AdjustListView.Items[i].SubItems[12].Text;
                                dCoorX = Convert.ToDouble(strCoorX);
                                dCoorY = Convert.ToDouble(strCoorY);

                                dCoorX = dCoorX + dAdjDeltaX;      //计算改正后X、Y坐标
                                dCoorY = dCoorY + dAdjDeltaY;

                                if (i < nCount - 3)
                                {
                                    strCoorX = dCoorX.ToString("#0.0000");
                                    strCoorY = dCoorY.ToString("#0.0000");
                                    this.AdjustListView.Items[i + 1].SubItems[11].Text = strCoorX;
                                    this.AdjustListView.Items[i + 1].SubItems[12].Text = strCoorY;
                                }
                            }

                            //********************辅助计算**************************
                            pSumAdjust = dFb.ToString("#0");
                            pSumAdjDeltaX = (dSumDeltaX + dFx).ToString("#0.0000");
                            pSumAdjDeltaY = (dSumDeltaY + dFy).ToString("#0.0000");

                            //角度闭合差
                            textBox5.Text = dFb.ToString("#0");
                            //线路长度
                            textBox6.Text = dSumLength.ToString("#0.0000");
                            //限差
                            string strFr = "";
                            double dFr = 0;

                            if (MeasGrade.SelectedIndex == 0)
                            {
                                dFr = 3.6 * Math.Sqrt(nCount - 2);
                            }
                            if (MeasGrade.SelectedIndex == 1)
                            {
                                dFr = 5 * Math.Sqrt(nCount - 2);
                            }
                            if (MeasGrade.SelectedIndex == 2)
                            {
                                dFr = 10 * Math.Sqrt(nCount - 2);
                            }
                            if (MeasGrade.SelectedIndex == 3)
                            {
                                dFr = 16 * Math.Sqrt(nCount - 2);
                            }
                            if (MeasGrade.SelectedIndex == 4)
                            {
                                dFr = 24 * Math.Sqrt(nCount - 2);
                            }
                            strFr = dFr.ToString("#0.0");
                            textBox2.Text = "±" + strFr;
                            //X坐标闭合差
                            textBox3.Text = dFx.ToString("#0.0000");
                            //Y坐标闭合差
                            textBox7.Text = dFy.ToString("#0.0000");
                            //全长闭合差
                            double dF = 0;
                            dF = Math.Sqrt(dFx * dFx + dFy * dFy);
                            textBox4.Text = dF.ToString("#0.0000");
                            //精度评定
                            int nPrecision;
                            nPrecision = Convert.ToInt32(dSumLength / dF);
                            textBox8.Text = nPrecision.ToString();
                            //********************辅助计算**************************

                            //********************写入公共变量**********************
                            pfb = dFb.ToString("#0");
                            pfx = dFx.ToString("#0.0000");
                            pfy = dFy.ToString("#0.0000");
                            pf = dF.ToString("#0.0000");
                            pK = nPrecision.ToString("#0");
                            //********************写入公共变量**********************

                            if (Math.Abs(dCoorX - dCoorX3) >= 0.001 || Math.Abs(dCoorY - dCoorY3) >= 0.001)//判断计算精度可以设置高精度选项0.0001m
                            {
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show("Oh~NO！Aurora刚刚发现，附合导线坐标在最后附合时计算错误，请检查你的数据。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("Oh~NO！Aurora剛剛發現，附合導線座標在最後附合時計算錯誤，請檢查你的資料。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Oh~NO！Aurora has found that a calculate error occures while closing line, check your data please.", "Aurora Intelligent Tips");
                                }
                            }

                            log.Info(DateTime.Now.ToString() + "Two Angle Conn Traverse" + sender.ToString() + e.ToString());//写入一条新log
                        }
                        break;

                    case 7: //支导线
                        {
                            if (nCount < 3)
                            {
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show("行数小于3，不满足计算条件。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("行數小於3，不滿足計算條件。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Less than 3 rows, Aurora can't adjust.", "Aurora Intelligent Tips");
                                }
                                return;
                            }

                            string strCoorX1 = this.AdjustListView.Items[0].SubItems[7].Text;
                            string strCoorY1 = this.AdjustListView.Items[0].SubItems[8].Text;
                            string strCoorX2 = this.AdjustListView.Items[1].SubItems[7].Text;
                            string strCoorY2 = this.AdjustListView.Items[1].SubItems[8].Text;
                            if (strCoorX1 == "" || strCoorY1 == "" || strCoorX2 == "" || strCoorY2 == "")   //如果前两点坐标为空的话，提示输入
                            {
                                strCoorX1 = Zero; strCoorY1 = Zero; strCoorX2 = Zero; strCoorY2 = Zero;
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show("请检查支导线前两点坐标是否为空。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("請檢查支導線前兩點座標是否為空。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Please check first two known coor is null or not.", "Aurora Intelligent Tips");
                                }
                                return;
                            }

                            double dCoorX1 = 0, dCoorY1 = 0, dCoorX2 = 0, dCoorY2 = 0;
                            dCoorX1 = Convert.ToDouble(strCoorX1);
                            dCoorY1 = Convert.ToDouble(strCoorY1);
                            dCoorX2 = Convert.ToDouble(strCoorX2);
                            dCoorY2 = Convert.ToDouble(strCoorY2);

                            double dStartDist, dStartAzimuth;

                            dStartDist = CoortoDist(dCoorX1, dCoorY1, dCoorX2, dCoorY2);
                            dStartAzimuth = CoortoRad(dCoorX1, dCoorY1, dCoorX2, dCoorY2);
                            dStartAzimuth = RadtoDMS(dStartAzimuth);

                            this.AdjustListView.Items[0].SubItems[3].Text = dStartAzimuth.ToString("#0.0000");
                            this.AdjustListView.Items[0].SubItems[4].Text = dStartDist.ToString("#0.0000");

                            string strObsAngle, strAzimuth, strLength, strDeltaX, strDeltaY, strCoorX, strCoorY;
                            double dObsAngle, dAzimuth, dLength, dSumLength, dDeltaX, dDeltaY, dCoorX, dCoorY;
                            strObsAngle = ""; strAzimuth = ""; strLength = ""; strDeltaX = ""; strDeltaY = ""; strCoorX = ""; strCoorY = "";
                            dObsAngle = 0; dAzimuth = 0; dLength = 0; dSumLength = 0; dDeltaX = 0; dDeltaY = 0; dCoorX = 0; dCoorY = 0;

                            for (int i = 1; i < nCount - 1; i++)
                            {
                                strObsAngle = this.AdjustListView.Items[i].SubItems[2].Text;
                                strAzimuth = this.AdjustListView.Items[i - 1].SubItems[3].Text;
                                if (strObsAngle == "")   //如果观测角度为空的话，提示输入
                                {
                                    strObsAngle = Zero;
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("请检查观测角度是否为空。", "Aurora智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("請檢查觀測角度是否為空。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Please check Meas Angle is null or not.", "Aurora Intelligent Tips");
                                    }
                                    return;
                                }
                                dObsAngle = DMStoDec(Convert.ToDouble(strObsAngle));
                                dAzimuth = DMStoDec(Convert.ToDouble(strAzimuth));      //将角度ddd.mmss转化为小数度进行计算

                                if (AngleMode.SelectedIndex == 0)
                                {
                                    dAzimuth = dAzimuth + dObsAngle - 180;
                                }
                                if (AngleMode.SelectedIndex == 1)
                                {
                                    dAzimuth = dAzimuth - dObsAngle + 180;
                                }

                                if (dAzimuth >= 360)
                                {
                                    dAzimuth = dAzimuth - 360;
                                }
                                if (dAzimuth <= 0)
                                {
                                    dAzimuth = dAzimuth + 360;
                                }

                                dAzimuth = DectoDMS(dAzimuth);
                                strAzimuth = dAzimuth.ToString("#0.0000");
                                this.AdjustListView.Items[i].SubItems[3].Text = strAzimuth;
                            }

                            for (int i = 1; i < nCount - 1; i++)
                            {
                                strAzimuth = this.AdjustListView.Items[i].SubItems[3].Text;
                                strLength = this.AdjustListView.Items[i].SubItems[4].Text;
                                if (strLength == "")   //如果边长为空的话，提示输入
                                {
                                    strLength = Zero;
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("请检查边长是否为空。", "Aurora智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("請檢查邊長是否為空。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Please check Length is null or not.", "Aurora Intelligent Tips");
                                    }
                                    return;
                                }
                                dAzimuth = DMStoRad(Convert.ToDouble(strAzimuth));      //将角度ddd.mmss转化为弧度，进行三角函数计算
                                dLength = Convert.ToDouble(strLength);
                                dSumLength = dSumLength + dLength;

                                dDeltaX = dLength * Math.Cos(dAzimuth);     //计算出DeltaX、DeltaY
                                dDeltaY = dLength * Math.Sin(dAzimuth);

                                strCoorX = this.AdjustListView.Items[i].SubItems[7].Text;
                                strCoorY = this.AdjustListView.Items[i].SubItems[8].Text;
                                dCoorX = Convert.ToDouble(strCoorX);
                                dCoorY = Convert.ToDouble(strCoorY);

                                dCoorX = dCoorX + dDeltaX;      //计算X、Y坐标
                                dCoorY = dCoorY + dDeltaY;

                                strDeltaX = dDeltaX.ToString("#0.0000");
                                strDeltaY = dDeltaY.ToString("#0.0000");
                                strCoorX = dCoorX.ToString("#0.0000");
                                strCoorY = dCoorY.ToString("#0.0000");

                                this.AdjustListView.Items[i].SubItems[5].Text = strDeltaX;
                                this.AdjustListView.Items[i].SubItems[6].Text = strDeltaY;
                                this.AdjustListView.Items[i + 1].SubItems[7].Text = strCoorX;
                                this.AdjustListView.Items[i + 1].SubItems[8].Text = strCoorY;
                            }

                            //********************辅助计算**************************
                            //线路长度
                            textBox6.Text = dSumLength.ToString("#0.0000");
                            //********************辅助计算**************************

                            //********************写入公共变量**********************
                            //pfb = dFb.ToString();
                            //pfx = dFx.ToString();
                            //pfy = dFy.ToString();
                            //pf = dF.ToString();
                            //pK = nPrecision.ToString();
                            //********************写入公共变量**********************

                            log.Info(DateTime.Now.ToString() + "Open Traverse" + sender.ToString() + e.ToString());//写入一条新log
                        }
                        break;
                    case 8: //无连接角导线
                        {
                            if (nCount < 3)
                            {
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show("行数小于3，不满足计算条件。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("行數小於3，不滿足計算條件。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Less than 3 rows, Aurora can't adjust.", "Aurora Intelligent Tips");
                                }
                                return;
                            }

                            string strCoorX1 = this.AdjustListView.Items[0].SubItems[9].Text;
                            string strCoorY1 = this.AdjustListView.Items[0].SubItems[10].Text;
                            string strCoorX2 = this.AdjustListView.Items[nCount - 1].SubItems[9].Text;
                            string strCoorY2 = this.AdjustListView.Items[nCount - 1].SubItems[10].Text;
                            if (strCoorX1 == "" || strCoorY1 == "" || strCoorX2 == "" || strCoorY2 == "")   //如果已知点坐标为空的话，提示输入
                            {
                                strCoorX1 = Zero; strCoorY1 = Zero; strCoorX2 = Zero; strCoorY2 = Zero;
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show("请检查导线已知点坐标是否为空。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("請檢查導線已知點座標是否為空。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Please check known coor is null or not.", "Aurora Intelligent Tips");
                                }
                                return;
                            }

                            double dCoorX1 = 0, dCoorY1 = 0, dCoorX2 = 0, dCoorY2 = 0;
                            dCoorX1 = Convert.ToDouble(strCoorX1);
                            dCoorY1 = Convert.ToDouble(strCoorY1);
                            dCoorX2 = Convert.ToDouble(strCoorX2);
                            dCoorY2 = Convert.ToDouble(strCoorY2);

                            double dKnownDist = 0.0, dKnownAzimuth = 0.0, dKnownDeltaX = 0.0, dKnownDeltaY = 0.0;     //计算已知点距离和坐标方位角、X坐标增量、Y坐标增量

                            dKnownDist = CoortoDist(dCoorX1, dCoorY1, dCoorX2, dCoorY2);
                            dKnownAzimuth = CoortoRad(dCoorX1, dCoorY1, dCoorX2, dCoorY2);
                            dKnownAzimuth = RadtoDMS(dKnownAzimuth);
                            dKnownDeltaX = dCoorX2 - dCoorX1;
                            dKnownDeltaY = dCoorY2 - dCoorY1;

                            string strAssumedAzimuth = "", strObsAngle = "", strAzimuth = "", strLength = "";
                            string strAssumedCoorX = "", strAssumedCoorY = "", strAssumedDeltaX = "", strAssumedDeltaY = "";
                            double dAssumedAzimuth = 0.0, dObsAngle = 0.0, dAzimuth = 0.0, dLength = 0.0, dSumLength = 0.0;
                            double dAssumedDeltaX = 0.0, dAssumedDeltaY = 0.0, dSumAssumedDeltaX = 0.0, dSumAssumedDeltaY = 0.0, dAssumedCoorX = 0.0, dAssumedCoorY = 0.0;

                            strAssumedAzimuth = this.AdjustListView.Items[0].SubItems[4].Text;              //取出假定方位角
                            if (strAssumedAzimuth == "")   //如果假定方位角为空的话，提示输入
                            {
                                strAssumedAzimuth = Zero;
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show("请检查假定方位角是否为空。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("請檢查假定方位角是否為空。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Please check Assumed Azimuth is null or not.", "Aurora Intelligent Tips");
                                }
                                return;
                            }
                            dAssumedAzimuth = DMStoDec(Convert.ToDouble(strAssumedAzimuth));                //将假定方位角转化为小数度计算

                            for (int i = 1; i < nCount - 1; i++)
                            {
                                strObsAngle = this.AdjustListView.Items[i].SubItems[2].Text;
                                strAzimuth = this.AdjustListView.Items[i - 1].SubItems[4].Text;
                                if (strObsAngle == "")   //如果观测角度为空的话，提示输入
                                {
                                    strObsAngle = Zero;
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("请检查观测角度是否为空。", "Aurora智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("請檢查觀測角度是否為空。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Please check Meas Angle is null or not.", "Aurora Intelligent Tips");
                                    }
                                    return;
                                }
                                dObsAngle = DMStoDec(Convert.ToDouble(strObsAngle));
                                dAzimuth = DMStoDec(Convert.ToDouble(strAzimuth));      //将角度ddd.mmss转化为小数度进行计算

                                if (AngleMode.SelectedIndex == 0)
                                {
                                    dAzimuth = dAzimuth + dObsAngle - 180;
                                }
                                if (AngleMode.SelectedIndex == 1)
                                {
                                    dAzimuth = dAzimuth - dObsAngle + 180;
                                }

                                if (dAzimuth >= 360)
                                {
                                    dAzimuth = dAzimuth - 360;
                                }
                                if (dAzimuth <= 0)
                                {
                                    dAzimuth = dAzimuth + 360;
                                }

                                dAzimuth = DectoDMS(dAzimuth);
                                strAzimuth = dAzimuth.ToString("#0.0000");
                                this.AdjustListView.Items[i].SubItems[4].Text = strAzimuth;
                            }

                            for (int i = 0; i < nCount - 1; i++)
                            {
                                strAzimuth = this.AdjustListView.Items[i].SubItems[4].Text;
                                strLength = this.AdjustListView.Items[i].SubItems[3].Text;
                                if (strLength == "")   //如果边长为空的话，提示输入
                                {
                                    strLength = Zero;
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("请检查边长是否为空。", "Aurora智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("請檢查邊長是否為空。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Please check Length is null or not.", "Aurora Intelligent Tips");
                                    }
                                    return;
                                }
                                dAzimuth = DMStoRad(Convert.ToDouble(strAzimuth));      //将角度ddd.mmss转化为弧度，进行三角函数计算
                                dLength = Convert.ToDouble(strLength);
                                dSumLength = dSumLength + dLength;

                                dAssumedDeltaX = dLength * Math.Cos(dAzimuth);     //计算出DeltaX、DeltaY
                                dAssumedDeltaY = dLength * Math.Sin(dAzimuth);

                                if (i == 0)
                                {
                                    dAssumedCoorX = dCoorX1;
                                    dAssumedCoorY = dCoorY1;
                                }
                                else
                                {
                                    strAssumedCoorX = this.AdjustListView.Items[i].SubItems[7].Text;
                                    strAssumedCoorY = this.AdjustListView.Items[i].SubItems[8].Text;
                                    dAssumedCoorX = Convert.ToDouble(strAssumedCoorX);
                                    dAssumedCoorY = Convert.ToDouble(strAssumedCoorY);
                                }

                                dSumAssumedDeltaX = dSumAssumedDeltaX + dAssumedDeltaX;             //计算假定坐标增量之和
                                dSumAssumedDeltaY = dSumAssumedDeltaY + dAssumedDeltaY;

                                dAssumedCoorX = dAssumedCoorX + dAssumedDeltaX;             //计算假定X、Y坐标
                                dAssumedCoorY = dAssumedCoorY + dAssumedDeltaY;

                                strAssumedDeltaX = dAssumedDeltaX.ToString("#0.0000");
                                strAssumedDeltaY = dAssumedDeltaY.ToString("#0.0000");
                                strAssumedCoorX = dAssumedCoorX.ToString("#0.0000");
                                strAssumedCoorY = dAssumedCoorY.ToString("#0.0000");

                                this.AdjustListView.Items[i].SubItems[5].Text = strAssumedDeltaX;
                                this.AdjustListView.Items[i].SubItems[6].Text = strAssumedDeltaY;
                                this.AdjustListView.Items[i + 1].SubItems[7].Text = strAssumedCoorX;
                                this.AdjustListView.Items[i + 1].SubItems[8].Text = strAssumedCoorY;
                            }
                            
                            //下面计算Q1和Q2.
                            double Q1 = (dSumAssumedDeltaX * dKnownDeltaX + dSumAssumedDeltaY * dKnownDeltaY) / (dSumAssumedDeltaX * dSumAssumedDeltaX + dSumAssumedDeltaY * dSumAssumedDeltaY);
                            double Q2 = (dSumAssumedDeltaX * dKnownDeltaY - dSumAssumedDeltaY * dKnownDeltaX) / (dSumAssumedDeltaX * dSumAssumedDeltaX + dSumAssumedDeltaY * dSumAssumedDeltaY);

                            MessageBox.Show("Q1 = " + Q1.ToString());
                            MessageBox.Show("Q2 = " + Q2.ToString());

                            double dCoorX = 0.0, dCoorY = 0.0;
                            string strCoorX = "", strCoorY = "";

                            for (int i = 1; i < nCount - 1; i++)                //计算校正后的X、Y坐标
                            {
                                strAssumedCoorX = this.AdjustListView.Items[i].SubItems[7].Text;
                                strAssumedCoorY = this.AdjustListView.Items[i].SubItems[8].Text;
                                dAssumedCoorX = Convert.ToDouble(strAssumedCoorX);
                                dAssumedCoorY = Convert.ToDouble(strAssumedCoorY);

                                dCoorX = dCoorX1 + Q1 * (dAssumedCoorX - dCoorX1) - Q2 * (dAssumedCoorY - dCoorY1);
                                dCoorY = dCoorY1 + Q1 * (dAssumedCoorY - dCoorY1) + Q2 * (dAssumedCoorX - dCoorX1);
                                strCoorX = dCoorX.ToString("#0.0000");
                                strCoorY = dCoorY.ToString("#0.0000");
                                this.AdjustListView.Items[i].SubItems[9].Text = strCoorX;
                                this.AdjustListView.Items[i].SubItems[10].Text = strCoorY;
                            }

                            //********************辅助计算**************************
                            //线路长度
                            textBox6.Text = dSumLength.ToString("#0.0000");
                            double dCalDist = Math.Sqrt(dSumAssumedDeltaX * dSumAssumedDeltaX + dSumAssumedDeltaY * dSumAssumedDeltaY);
                            double K = Convert.ToInt32(dKnownDist / Math.Abs(dCalDist - dKnownDist));
                            textBox8.Text = K.ToString();
                            //********************辅助计算**************************

                            //********************写入公共变量**********************
                            //pfb = dFb.ToString();
                            //pfx = dFx.ToString();
                            //pfy = dFy.ToString();
                            //pf = dF.ToString();
                            pK = K.ToString("#0");
                            //********************写入公共变量**********************

                            log.Info(DateTime.Now.ToString() + "No Angle Conn Traverse" + sender.ToString() + e.ToString());//写入一条新log
                        }
                        break;
                    case 9:    //水准网
                        {
                            if (nCount < 1)
                            {
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show("行数小于1，不满足计算条件。", "Aurora智能提示");
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show("行數小於1，不滿足計算條件。", "Aurora智慧提示");
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show("Less than 1 row, Aurora can't adjust.", "Aurora Intelligent Tips");
                                }
                                return;
                            }

                            nLevelingObsNum = nCount;        //将列表的统计行数总动填到观测值总数中
                            strLevelingNetworkData = LevelingNetwork_textBox.Text.Trim();
                            LevelingNetworkSettings frmLevelingNetworkSettings = new LevelingNetworkSettings();
                            frmLevelingNetworkSettings.ShowDialog(this);

                            if (LevelingNetwork_textBox.Text.Trim() == "")      //如果LevelingNetwork_textBox.Text.Trim()还是空的话，说明在水准网设置那儿，点击的是“取消”
                            {
                                return;
                            }

                            ProgressBar fProgressBar1 = new ProgressBar();           //调用进度条窗体
                            fProgressBar1.Show();
                            Delay(1234);
                            fProgressBar1.Hide();

                            int unPnumber = m_Tnumber - m_Knumber; //未知高程点数

                            KHeight = new double[m_Tnumber];
                            Pname = new string[m_Tnumber];
                            dX = new double[m_Tnumber];
                            ATPA = new double[m_Tnumber * (m_Tnumber + 1) / 2];
                            ATPL = new double[m_Tnumber];
                            StartP = new string[m_Onumber];
                            EndP = new string[m_Onumber];
                            L = new double[m_Onumber];
                            V = new double[m_Onumber];
                            P = new double[m_Onumber];

                            //  读取已知高程数据
                            for (int i = 0; i < m_Knumber; i++)
                            {
                                string strLine1 = LevelingNetwork_textBox.Lines[i + 1];
                                string[] strElement1 = strLine1.Split(',');
                                Pname[i] = strElement1[0];              //已知点点名
                                KHeight[i] = Convert.ToDouble(strElement1[1]);      //已知点高程
                            }

                            //  读取观测数据
                            for (int j = 0; j < m_Onumber; j++)
                            {
                                StartP[j] = AdjustListView.Items[j].SubItems[1].Text;             //高差起点号
                                EndP[j] = AdjustListView.Items[j].SubItems[2].Text;               //高差终止号
                                L[j] = Convert.ToDouble(AdjustListView.Items[j].SubItems[3].Text);        //高差
                                P[j] = Convert.ToDouble(AdjustListView.Items[j].SubItems[4].Text);        //平距
                                P[j] = 1.0 / P[j];
                            }

                            string[] strTPname = new string[2 * m_Onumber];     //strTPname存放所有起始终点的集合。
                            for (int j = 0; j < m_Onumber; j++)
                            {
                                strTPname[2 * j] = StartP[j];
                                strTPname[2 * j + 1] = EndP[j];
                            }

                            string[] ResultPName = strTPname.Distinct().Except(Pname).ToArray();     //ResultPName去重复，然后排出已知点的集合。

                            for (int i = m_Knumber; i < Pname.Length; i++)
                            {
                                Pname[i] = ResultPName[i - 2];
                            }

                            ca_HO();
                            ca_ATPA();

                            for (int k = 0; k < m_Knumber; k++)
                            {
                                ATPA[ij(k, k)] = 1.0e30;
                            }

                            ca_dX();
                            m_pvv = ca_V();                //最小二乘平差结果
                            m_mu = Math.Sqrt(m_pvv / (m_Onumber - (m_Tnumber - m_Knumber)));      //单位权中误差

                            for (int m = 0; m < m_Tnumber; m++)
                            {
                                double dx = dX[m];
                                double qii = ATPA[ij(m, m)];
                                s = Math.Sqrt(qii) * m_mu;
                            }

                            for (int n = 0; n < m_Onumber; n++)
                            {
                                ArrayList arr = new ArrayList(Pname);
                                int k1 = arr.IndexOf(StartP[n]);  //高差起点号
                                int k2 = arr.IndexOf(EndP[n]);  //高差起点号
                                double qii = ATPA[ij(k1, k1)];
                                double qjj = ATPA[ij(k2, k2)];
                                double qij = ATPA[ij(k1, k2)];
                                ml = Math.Sqrt(qii + qjj - 2.0 * qij) * m_mu;
                            }

                            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                            {
                                MessageBox.Show("水准网平差完毕，请切换至智能报表页面查看平差结果。", "Aurora智能提示");
                            }
                            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                            {
                                MessageBox.Show("水準網平差完畢，請切換至智慧報表頁面查看平差結果。", "Aurora智慧提示");
                            }
                            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                            {
                                MessageBox.Show("Leveling Network Adjustment complete, Switch to Report View Result.", "Aurora Intelligent Tips");
                            }
                        }
                        break;
                }

                FileStream aFile1 = new FileStream(Application.StartupPath + "\\MapData\\AdjType.ini", FileMode.Create);
                StreamWriter MyWriter1 = new StreamWriter(aFile1);
                switch (AdjustType.SelectedIndex)       //写入本地ini配置文件，来表示哪一种平差来绘图或者显示报表。
                {
                    case 0: //闭合水准
                        {
                            MyWriter1.Write("0");
                        }
                        break;

                    case 1: //附和水准
                        {
                            MyWriter1.Write("1");
                        }
                        break;

                    case 2: //支水准
                        {
                            MyWriter1.Write("2");
                        }
                        break;

                    case 3: //闭合导线
                        {
                            MyWriter1.Write("3");
                        }
                        break;

                    case 4: //闭合导线(含外支点)
                        {
                            MyWriter1.Write("4");
                        }
                        break;

                    case 5: //具有一个连接角的附和导线
                        {
                            MyWriter1.Write("5");
                        }
                        break;

                    case 6: //具有两个连接角的附和导线
                        {
                            MyWriter1.Write("6");
                        }
                        break;

                    case 7: //支导线
                        {
                            MyWriter1.Write("7");
                        }
                        break;

                    case 8: //无连接角导线
                        {
                            MyWriter1.Write("8");
                        }
                        break;
                    case 9: //水准网
                        {
                            MyWriter1.Write("9");
                        }
                        break;
                }
                if (MyWriter1 != null)
                {
                    MyWriter1.Close();
                }

                nCalcFlag = 1;      //设置全局变量nCalcFlag，默认为零。当完成平差计算时，nCalcFlag = 1。否则不能进行绘图和生成报表。

                //log.Info(DateTime.Now.ToString() + sender.ToString() + e.ToString());//写入一条新log
                //放在每个平差中，以确定具体是哪一类平差
                System.Media.SoundPlayer sndPlayer = new System.Media.SoundPlayer(Application.StartupPath + "\\Finish.wav");    //wav格式的铃声 
                sndPlayer.Play();
            }
            catch (System.Exception ex)
            {
                log.Info(DateTime.Now.ToString() + "Adjust Calculation" + ex.ToString());//写入一条新log
            }
            
        }

        public void toolStripButton_Mapping_Click(object sender, EventArgs e)          //绘图功能
        {
            //点击工具栏中的绘图，后自动切换到绘图标签
            //具体参考tabControl1_Selected()函数
            tabControl1.SelectedTab = tabPage2;
        }

        public void toolStripButton_Report_Click(object sender, EventArgs e)           //生成报表
        {
            //点击工具栏中的报表，后自动切换到报表标签
            //具体参考tabControl1_Selected()函数
            tabControl1.SelectedTab = tabPage3;
        }

        private void toolStripButton_CMD_Click(object sender, EventArgs e)              //命令行
        {
            PublicClass.MyCmd.StartPosition = FormStartPosition.CenterScreen;
            PublicClass.MyCmd.Show();
            PublicClass.MyCmd.Focus();
        }

        private void toolStripButton_Setup_Click(object sender, EventArgs e)            //设置
        {
            Setting FrmSetup = new Setting();
            FrmSetup.StartPosition = FormStartPosition.CenterParent;      
            FrmSetup.ShowDialog(this);      //this 必须有，传递子窗体参数       //创建模态对话框
            //FrmSetup.Show(this);      //this 必须有，传递子窗体参数       //创建非模态对话框
        }

        private void toolStripButton_Locker_Click(object sender, EventArgs e)             //启用数据锁
        {
            try
            {
                PublicClass.AuroraMain.Hide();
                if (PublicClass.MyCmd != null)
                {
                    PublicClass.MyCmd.Hide();
                }
                PublicClass.Locker.Show();
                log.Info(DateTime.Now.ToString() + "Start Locker" + sender.ToString() + e.ToString());//写入一条新log
            }
            catch { }
        }

        private void toolStripButton_About_Click(object sender, EventArgs e)            //关于
        {
            AboutAurora FrmAbout = new AboutAurora();
            FrmAbout.StartPosition = FormStartPosition.CenterParent;      //创建模态对话框
            FrmAbout.ShowDialog();
        }

        #endregion

        #region 鼠标移动到特定位置显示提示信息。
        private void CheckBox_HighPrecision_MouseEnter(object sender, EventArgs e)      //高精度选项 提示
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            string Tips = "";
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")       
            {
                Tips = "高精度选项，距离可达到0.0001m，角度可达0.1'" + "\r\n"
                    + "此功能正在试验中。";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                Tips = "高精度選項，距離可達到0.0001m，角度可達0.1'" + "\r\n"
                    + "此功能正在試驗中。";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")   
            {
                Tips = "Option for high precision, dist can reach to 0.0001m and angle to 0.1'" + "\r\n"
                    + "We are still testing.";
            }
            p.SetToolTip(this.CheckBox_HighPrecision, Tips);
        }

        private void MeasGrade_MouseEnter(object sender, EventArgs e)                   //测量等级 提示
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            string Tips = "";
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN") 
            {
                Tips = "本功能根据《工程测量规范》（GB0026-2007）计算。" + "\r\n"
                    + "也可请依据具体项目测量要求的限差，自行填到fr文本框中。";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                Tips = "本功能根據《工程測量規範》（GB0026-2007）計算。" + "\r\n"
                    + "也可請依據具體專案測量要求的限差，自行填到fr文字方塊中。";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")  
            {
                Tips = "Reference to Chinese code for engineering surveying(GB0026-2007)." + "\r\n"
                    + "You can also use your own tolerance, and write to fr textbox.";
            }
            p.SetToolTip(this.MeasGrade, Tips);
        }

        private void AdjustType_MouseEnter(object sender, EventArgs e)                  //平差模式 提示
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            string Tips = "";
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                Tips = "请选择平差类型。" + "\r\n"
                    + "不清楚平差类型？请查阅帮助。";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                Tips = "請選擇平差類型。" + "\r\n"
                    + "不清楚平差類型？請查閱幫助。";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")             
            {
                Tips = "Selcet an adjustment type" + "\r\n"
                    + "No idea? Please reference help file.";
            }
            p.SetToolTip(this.AdjustType, Tips);
        }

        private void AngleMode_MouseEnter(object sender, EventArgs e)                   //测角模式 提示
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            string Tips = "";
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                Tips = "请选择导线的观测角度模式。";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                Tips = "請選擇導線的觀測角度模式。";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                Tips = "Select the measure angle mode.";
            }
            p.SetToolTip(this.AngleMode, Tips);
        }

        private void LevelMode_MouseEnter(object sender, EventArgs e)                   //水准误差分配模式 提示
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            string Tips = "";
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                Tips = "请选择水准误差的分配模式。" + "\r\n"
                    + "默认根据距离加权平均法来分配误差。";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                Tips = "請選擇水準誤差的分配模式。" + "\r\n"
                    + "預設根據距離加權平均法來分配誤差。";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                Tips = "Select leveling error adjust mode" + "\r\n"
                    + "Dist weighted average method is default.";
            }
            p.SetToolTip(this.LevelMode, Tips);
        }

        private void toolStripButton_Add_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_Add.ToolTipText = "添加行";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_Add.ToolTipText = "添加行";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_Add.ToolTipText = "Add Row";
            }
        }

        private void toolStripButton_Delete_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_Delete.ToolTipText = "删除行";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_Delete.ToolTipText = "刪除行";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_Delete.ToolTipText = "Delete Row";
            }
        }

        private void toolStripButton_Clear_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_Clear.ToolTipText = "清空列表";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_Clear.ToolTipText = "清空列表";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_Clear.ToolTipText = "Clear List";
            }
        }

        private void toolStripButton_Calc_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_Calc.ToolTipText = "平差计算";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_Calc.ToolTipText = "平差計算";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_Calc.ToolTipText = "Adjust";
            }
        }

        private void toolStripButton_Mapping_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_Mapping.ToolTipText = "绘图";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_Mapping.ToolTipText = "繪圖";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_Mapping.ToolTipText = "Mapping";
            }
        }

        private void toolStripButton_Report_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_Report.ToolTipText = "生成报表";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_Report.ToolTipText = "生成報表";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_Report.ToolTipText = "Report";
            }
        }

        private void toolStripButton_CMD_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_CMD.ToolTipText = "命令行";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_CMD.ToolTipText = "命令行";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_CMD.ToolTipText = "Cmd Line";
            }
        }

        private void toolStripButton_Locker_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_Locker.ToolTipText = "数据保护锁";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_Locker.ToolTipText = "資料保護鎖";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_Locker.ToolTipText = "Data Locker";
            }
        }

        private void toolStripButton_Setup_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_Setup.ToolTipText = "设置";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_Setup.ToolTipText = "設置";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_Setup.ToolTipText = "Settings";
            }
        }

        private void toolStripButton_About_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_About.ToolTipText = "关于";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_About.ToolTipText = "關於";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_About.ToolTipText = "About";
            }
        }

        #endregion

        #region 菜单栏代码 & 语言切换
        private void ToolStripMenuItem_New_Click(object sender, EventArgs e)                //文件-新建
        {
            this.AdjustListView.Items.Clear();
            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";
        }

        private void ToolStripMenuItem_Import_Click(object sender, EventArgs e)                //文件-导入
        {
            string fName = "";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Application.StartupPath + "\\SampleData";
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                openFileDialog.Filter = "Txt文本文件(*.txt)|*.txt|Dat数据文件(*.dat)|*.dat|Excel交换文件(*.csv)|*.csv|Xml-Aurora交换文件(*.xml)|*.xml|所有文件(*.*)|*.*";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                openFileDialog.Filter = "Txt文字檔(*.txt)|*.txt|Dat資料檔案(*.dat)|*.dat|Excel交換檔(*.csv)|*.csv|Xml-Aurora交換檔(*.xml)|*.xml|所有檔(*.*)|*.*";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                openFileDialog.Filter = "Txt file(*.txt)|*.txt|Dat file(*.dat)|*.dat|Excel file(*.csv)|*.csv|Xml-Aurora flie(*.xml)|*.xml|All file(*.*)|*.*";
            }
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                AdjustListView.Items.Clear();
                textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

                fName = openFileDialog.FileName;
                string strLine = "";
                FileStream fs = new FileStream(fName, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding("gb2312"));

                try
                {
                    strLine = sr.ReadLine();//.Split(new string[] { "\r\n" }, StringSplitOptions.None)[0];后面的可以指定行读取
                    string[] strElement = strLine.Split(',');

                    string strAdjType = strElement[0];
                    string strKnown = strElement[1];
                    string strUnknown = strElement[2];

                    if (strAdjType.Trim() == "")             //判断平差类型,若此位置为空(导入数据未指定平差类型)，则默认导入到当前的类型中。
                    {
                        if (this.AdjustType.SelectedIndex == 0)
                        {
                            strAdjType = "ClosedLeveling";
                        }
                        else if (this.AdjustType.SelectedIndex == 1)
                        {
                            strAdjType = "AnnexedLeveling";
                        }
                        else if (this.AdjustType.SelectedIndex == 2)
                        {
                            strAdjType = "SpurLeveling";
                        }
                        else if (this.AdjustType.SelectedIndex == 3)
                        {
                            strAdjType = "ClosedTraverse";
                        }
                        else if (this.AdjustType.SelectedIndex == 4)
                        {
                            strAdjType = "ClosedTraverseWithOuterPoint";
                        }
                        else if (this.AdjustType.SelectedIndex == 5)
                        {
                            strAdjType = "OneAngleConnTraverse";
                        }
                        else if (this.AdjustType.SelectedIndex == 6)
                        {
                            strAdjType = "TwoAngleConnTraverse";
                        }
                        else if (this.AdjustType.SelectedIndex == 7)
                        {
                            strAdjType = "OpenTraverse";
                        }
                        else if (this.AdjustType.SelectedIndex == 8)
                        {
                            strAdjType = "NoAngleConnTraverse";
                        }
                        else if (this.AdjustType.SelectedIndex == 9)
                        {
                            strAdjType = "LevelingNetwork";
                        }
                    }
                    else if (strAdjType == "ClosedLeveling")                //如果指定了平差类型，则按下面规则导入。
                    {
                        this.AdjustType.Text = AdjustType.Items[0].ToString();
                    }
                    else if (strAdjType == "AnnexedLeveling")
                    {
                        this.AdjustType.Text = AdjustType.Items[1].ToString();
                    }
                    else if (strAdjType == "SpurLeveling")
                    {
                        this.AdjustType.Text = AdjustType.Items[2].ToString();
                    }
                    else if (strAdjType == "ClosedTraverse")
                    {
                        this.AdjustType.Text = AdjustType.Items[3].ToString();
                    }
                    else if (strAdjType == "ClosedTraverseWithOuterPoint")
                    {
                        this.AdjustType.Text = AdjustType.Items[4].ToString();
                    }
                    else if (strAdjType == "OneAngleConnTraverse")
                    {
                        this.AdjustType.Text = AdjustType.Items[5].ToString();
                    }
                    else if (strAdjType == "TwoAngleConnTraverse")
                    {
                        this.AdjustType.Text = AdjustType.Items[6].ToString();
                    }
                    else if (strAdjType == "OpenTraverse")
                    {
                        this.AdjustType.Text = AdjustType.Items[7].ToString();
                    }
                    else if (strAdjType == "NoAngleConnTraverse")
                    {
                        this.AdjustType.Text = AdjustType.Items[8].ToString();
                    }
                    else if (strAdjType == "LevelingNetwork")
                    {
                        this.AdjustType.Text = AdjustType.Items[9].ToString();
                    }

                    if (strAdjType == "LevelingNetwork")        //水准网平差的导入规则和其他不一样
                    {
                        ListViewItem lstItem = new ListViewItem();
                        lstItem.UseItemStyleForSubItems = false;//设置允许子项颜色不一致,否则无背景颜色
                        for (int i = 0; i < Convert.ToInt16(strKnown); i++)     //此处的strKnown实际上是导入数据中的观测值总数strObsNumber。
                        {
                            toolStripButton_Add_Click(sender, e);               //循环调用“添加行”命令
                            Delay(200);             //延时产生动画效果
                        }
                        for (int i = 0; i < Convert.ToInt16(strKnown); i++)
                        {
                            strLine = sr.ReadLine();
                            string[] strObsData = strLine.Split(',');
                            string StartPoint = strObsData[0].Trim();
                            string EndPoint = strObsData[1].Trim();
                            string LevelDiff = strObsData[2].Trim();
                            string HD = strObsData[3].Trim();

                            AdjustListView.Items[i].SubItems[1].Text = StartPoint;
                            AdjustListView.Items[i].SubItems[2].Text = EndPoint;
                            AdjustListView.Items[i].SubItems[3].Text = LevelDiff;
                            AdjustListView.Items[i].SubItems[4].Text = HD;
                        }
                    }
                    else 
                    {

                        #region 先将已知点数和未知点数相加，得到总行数，在列表中增加nCount行
                        int nKnown = Convert.ToInt16(strKnown);
                        int nUnknown = Convert.ToInt16(strUnknown);
                        int nCount = 0;
                        if (strAdjType == "ClosedLeveling")             //闭合的类型，需要加上1行。
                        {
                            nCount = nKnown + nUnknown + 1;
                        }
                        else if (strAdjType == "ClosedTraverse")
                        {
                            nCount = nKnown + nUnknown + 1;
                        }
                        else if (strAdjType == "ClosedTraverseWithOuterPoint")
                        {
                            nCount = nKnown + nUnknown + 1;
                        }
                        else nCount = nKnown + nUnknown;

                        ListViewItem lstItem = new ListViewItem();
                        lstItem.UseItemStyleForSubItems = false;//设置允许子项颜色不一致,否则无背景颜色

                        for (int i = 0; i < nCount; i++ )
                        {
                            toolStripButton_Add_Click(sender, e);               //循环调用“添加行”命令
                            Delay(200);             //延时产生动画效果
                        }
                        #endregion

                        #region 读入测站名
                        strLine = sr.ReadLine();
                        string[] strStnName = strLine.Split(',');

                        int k = strStnName.GetLength(0);                //获取字符串数组长度
                        for (int j = 0; j < strStnName.GetLength(0); j++)
                        {
                            AdjustListView.Items[j].SubItems[1].Text = strStnName[j];                //把测站名读入ListView
                        }
                        #endregion

                        #region 读取已知点坐标/高程
                        switch (AdjustType.SelectedIndex)       //选择平差类型
                        {
                            case 0: //闭合水准
                                {
                                    strLine = sr.ReadLine();
                                    //string[] strHeight = strLine;
                                    AdjustListView.Items[0].SubItems[7].Text = strLine;
                                    AdjustListView.Items[nCount - 1].SubItems[7].Text = strLine;
                                }
                                break;
                            case 1: //附和水准
                                {
                                    strLine = sr.ReadLine();
                                    string[] strHeight = strLine.Split(',');
                                    AdjustListView.Items[0].SubItems[7].Text = strHeight[0];
                                    AdjustListView.Items[nCount - 1].SubItems[7].Text = strHeight[1];
                                }
                                break;

                            case 2: //支水准
                                {
                                    strLine = sr.ReadLine();
                                    //string[] strHeight = strLine;
                                    AdjustListView.Items[0].SubItems[5].Text = strLine;
                                }
                                break;

                            case 3: //闭合导线————两个已知点
                                {
                                    strLine = sr.ReadLine();
                                    string[] strCoor1 = strLine.Split(',');
                                    AdjustListView.Items[0].SubItems[11].Text = strCoor1[0];
                                    AdjustListView.Items[0].SubItems[12].Text = strCoor1[1];
                                    AdjustListView.Items[nCount - 1].SubItems[11].Text = strCoor1[0];
                                    AdjustListView.Items[nCount - 1].SubItems[12].Text = strCoor1[1];

                                    strLine = sr.ReadLine();
                                    string[] strCoor2 = strLine.Split(',');
                                    AdjustListView.Items[1].SubItems[11].Text = strCoor2[0];
                                    AdjustListView.Items[1].SubItems[12].Text = strCoor2[1];
                                }
                                break;

                            case 4: //闭合导线(含外支点)————两个已知点
                                {
                                    strLine = sr.ReadLine();
                                    string[] strCoor1 = strLine.Split(',');
                                    AdjustListView.Items[0].SubItems[11].Text = strCoor1[0];
                                    AdjustListView.Items[0].SubItems[12].Text = strCoor1[1];

                                    strLine = sr.ReadLine();
                                    string[] strCoor2 = strLine.Split(',');
                                    AdjustListView.Items[1].SubItems[11].Text = strCoor2[0];
                                    AdjustListView.Items[1].SubItems[12].Text = strCoor2[1];
                                    AdjustListView.Items[nCount - 1].SubItems[11].Text = strCoor2[0];
                                    AdjustListView.Items[nCount - 1].SubItems[12].Text = strCoor2[1];
                                }
                                break;

                            case 5: //一个连接角的附和导线————三个已知点
                                {
                                    strLine = sr.ReadLine();
                                    string[] strCoor1 = strLine.Split(',');
                                    AdjustListView.Items[0].SubItems[9].Text = strCoor1[0];
                                    AdjustListView.Items[0].SubItems[10].Text = strCoor1[1];

                                    strLine = sr.ReadLine();
                                    string[] strCoor2 = strLine.Split(',');
                                    AdjustListView.Items[1].SubItems[9].Text = strCoor2[0];
                                    AdjustListView.Items[1].SubItems[10].Text = strCoor2[1];

                                    strLine = sr.ReadLine();
                                    string[] strCoor3 = strLine.Split(',');
                                    AdjustListView.Items[nCount - 1].SubItems[9].Text = strCoor3[0];
                                    AdjustListView.Items[nCount - 1].SubItems[10].Text = strCoor3[1];
                                }
                                break;

                            case 6: //两个连接角的附和导线————四个已知点
                                {
                                    strLine = sr.ReadLine();
                                    string[] strCoor1 = strLine.Split(',');
                                    AdjustListView.Items[0].SubItems[11].Text = strCoor1[0];
                                    AdjustListView.Items[0].SubItems[12].Text = strCoor1[1];

                                    strLine = sr.ReadLine();
                                    string[] strCoor2 = strLine.Split(',');
                                    AdjustListView.Items[1].SubItems[11].Text = strCoor2[0];
                                    AdjustListView.Items[1].SubItems[12].Text = strCoor2[1];

                                    strLine = sr.ReadLine();
                                    string[] strCoor3 = strLine.Split(',');
                                    AdjustListView.Items[nCount - 2].SubItems[11].Text = strCoor3[0];
                                    AdjustListView.Items[nCount - 2].SubItems[12].Text = strCoor3[1];

                                    strLine = sr.ReadLine();
                                    string[] strCoor4 = strLine.Split(',');
                                    AdjustListView.Items[nCount - 1].SubItems[11].Text = strCoor4[0];
                                    AdjustListView.Items[nCount - 1].SubItems[12].Text = strCoor4[1];
                                }
                                break;

                            case 7: //支导线————两个已知点
                                {
                                    strLine = sr.ReadLine();
                                    string[] strCoor1 = strLine.Split(',');
                                    AdjustListView.Items[0].SubItems[7].Text = strCoor1[0];
                                    AdjustListView.Items[0].SubItems[8].Text = strCoor1[1];

                                    strLine = sr.ReadLine();
                                    string[] strCoor2 = strLine.Split(',');
                                    AdjustListView.Items[1].SubItems[7].Text = strCoor2[0];
                                    AdjustListView.Items[1].SubItems[8].Text = strCoor2[1];
                                }
                                break;

                            case 8: //无连接角导线————两个已知点
                                {
                                    strLine = sr.ReadLine();
                                    string[] strCoor1 = strLine.Split(',');
                                    AdjustListView.Items[0].SubItems[9].Text = strCoor1[0];
                                    AdjustListView.Items[0].SubItems[10].Text = strCoor1[1];

                                    strLine = sr.ReadLine();
                                    string[] strCoor2 = strLine.Split(',');
                                    AdjustListView.Items[nCount - 1].SubItems[9].Text = strCoor2[0];
                                    AdjustListView.Items[nCount - 1].SubItems[10].Text = strCoor2[1];
                                }
                                break;
                        }
                        #endregion

                        #region 现在读取水准距离值
                        switch (AdjustType.SelectedIndex)       //选择平差类型
                        {
                            case 0: //闭合水准
                                {
                                    strLine = sr.ReadLine();
                                    string[] strDist = strLine.Split(',');
                                    //for (int i = 0; i < nCount - 1; i++)
                                    for (int i = 0; i < strDist.GetLength(0); i++)
                                    {
                                        AdjustListView.Items[i].SubItems[2].Text = strDist[i];
                                    }
                                }
                                break;
                            case 1: //附和水准
                                {
                                    strLine = sr.ReadLine();
                                    string[] strDist = strLine.Split(',');
                                    //for (int i = 0; i < nCount - 1; i++)
                                    for (int i = 0; i < strDist.GetLength(0); i++)
                                    {
                                        AdjustListView.Items[i].SubItems[2].Text = strDist[i];
                                    }
                                }
                                break;

                            case 2: //支水准
                                {
                                    strLine = sr.ReadLine();
                                    string[] strDist = strLine.Split(',');
                                    //for (int i = 0; i < nCount - 1; i++)
                                    for (int i = 0; i < strDist.GetLength(0); i++)
                                    {
                                        AdjustListView.Items[i].SubItems[2].Text = strDist[i];
                                    }
                                }
                                break;
                        }
                        #endregion

                        #region 现在读取水准测站数
                        switch (AdjustType.SelectedIndex)       //选择平差类型
                        {
                            case 0: //闭合水准
                                {
                                    strLine = sr.ReadLine();
                                    string[] strStns = strLine.Split(',');
                                    //for (int i = 0; i < nCount - 1; i++)
                                    for (int i = 0; i < strStns.GetLength(0); i++)
                                    {
                                        AdjustListView.Items[i].SubItems[3].Text = strStns[i];
                                    }
                                }
                                break;
                            case 1: //附和水准
                                {
                                    strLine = sr.ReadLine();
                                    string[] strStns = strLine.Split(',');
                                    //for (int i = 0; i < nCount - 1; i++)
                                    for (int i = 0; i < strStns.GetLength(0); i++)
                                    {
                                        AdjustListView.Items[i].SubItems[3].Text = strStns[i];
                                    }
                                }
                                break;

                            case 2: //支水准
                                {
                                    strLine = sr.ReadLine();
                                    string[] strStns = strLine.Split(',');
                                    //for (int i = 0; i < nCount - 1; i++)
                                    for (int i = 0; i < strStns.GetLength(0); i++)
                                    {
                                        AdjustListView.Items[i].SubItems[3].Text = strStns[i];
                                    }
                                }
                                break;
                        }
                        #endregion

                        #region 现在读取水准实测高差
                        switch (AdjustType.SelectedIndex)       //选择平差类型
                        {
                            case 0: //闭合水准
                                {
                                    strLine = sr.ReadLine();
                                    string[] strObsLevelDiff = strLine.Split(',');
                                    //for (int i = 0; i < nCount - 1; i++)
                                    for (int i = 0; i < strObsLevelDiff.GetLength(0); i++)
                                    {
                                        AdjustListView.Items[i].SubItems[4].Text = strObsLevelDiff[i];
                                    }
                                }
                                break;
                            case 1: //附和水准
                                {
                                    strLine = sr.ReadLine();
                                    string[] strObsLevelDiff = strLine.Split(',');
                                    //for (int i = 0; i < nCount - 1; i++)
                                    for (int i = 0; i < strObsLevelDiff.GetLength(0); i++)
                                    {
                                        AdjustListView.Items[i].SubItems[4].Text = strObsLevelDiff[i];
                                    }
                                }
                                break;

                            case 2: //支水准
                                {
                                    strLine = sr.ReadLine();
                                    string[] strObsLevelDiff = strLine.Split(',');
                                    //for (int i = 0; i < nCount - 1; i++)
                                    for (int i = 0; i < strObsLevelDiff.GetLength(0); i++)
                                    {
                                        AdjustListView.Items[i].SubItems[4].Text = strObsLevelDiff[i];
                                    }
                                }
                                break;
                        }
                        #endregion

                        #region 现在读取观测角度
                        switch (AdjustType.SelectedIndex)       //选择平差类型
                        {
                            case 3: //闭合导线
                                {
                                    strLine = sr.ReadLine();
                                    string[] strObsAngle = strLine.Split(',');
                                    //for (int i = 1; i < nCount; i++ )
                                    for (int i = 1; i < strObsAngle.GetLength(0) + 1; i++)
                                    {
                                        AdjustListView.Items[i].SubItems[2].Text = strObsAngle[i - 1];
                                    }
                                }
                                break;

                            case 4: //闭合导线(含外支点)
                                {
                                    strLine = sr.ReadLine();
                                    string[] strObsAngle = strLine.Split(',');
                                    //for (int i = 1; i < nCount; i++)
                                    for (int i = 1; i < strObsAngle.GetLength(0) + 1; i++)
                                    {
                                        AdjustListView.Items[i].SubItems[2].Text = strObsAngle[i - 1];
                                    }
                                }
                                break;

                            case 5: //一个连接角的附和导线
                                {
                                    strLine = sr.ReadLine();
                                    string[] strObsAngle = strLine.Split(',');
                                    //for (int i = 1; i < nCount - 1; i++)
                                    for (int i = 1; i < strObsAngle.GetLength(0) + 1; i++)
                                    {
                                        AdjustListView.Items[i].SubItems[2].Text = strObsAngle[i - 1];
                                    }
                                }
                                break;

                            case 6: //两个连接角的附和导线
                                {
                                    strLine = sr.ReadLine();
                                    string[] strObsAngle = strLine.Split(',');
                                    //for (int i = 1; i < nCount - 1; i++)
                                    for (int i = 1; i < strObsAngle.GetLength(0) + 1; i++)
                                    {
                                        AdjustListView.Items[i].SubItems[2].Text = strObsAngle[i - 1];
                                    }
                                }
                                break;

                            case 7: //支导线
                                {
                                    strLine = sr.ReadLine();
                                    string[] strObsAngle = strLine.Split(',');
                                    //for (int i = 1; i < nCount - 1; i++)
                                    for (int i = 1; i < strObsAngle.GetLength(0) + 1; i++)
                                    {
                                        AdjustListView.Items[i].SubItems[2].Text = strObsAngle[i - 1];
                                    }
                                }
                                break;

                            case 8: //无连接角导线
                                {
                                    strLine = sr.ReadLine();
                                    string[] strObsAngle = strLine.Split(',');
                                    //for (int i = 1; i < nCount - 1; i++)
                                    for (int i = 1; i < strObsAngle.GetLength(0) + 1; i++)
                                    {
                                        AdjustListView.Items[i].SubItems[2].Text = strObsAngle[i - 1];
                                    }
                                }
                                break;
                        }
                        #endregion

                        #region 现在读取边长
                        switch (AdjustType.SelectedIndex)       //选择平差类型
                        {
                            case 3: //闭合导线
                                {
                                    strLine = sr.ReadLine();
                                    string[] strLength = strLine.Split(',');
                                    //for (int i = 1; i < nCount - 1; i++)
                                    for (int i = 1; i < strLength.GetLength(0) + 1; i++)
                                    {
                                        AdjustListView.Items[i].SubItems[6].Text = strLength[i - 1];
                                    }
                                }
                                break;

                            case 4: //闭合导线(含外支点)
                                {
                                    strLine = sr.ReadLine();
                                    string[] strLength = strLine.Split(',');
                                    //for (int i = 1; i < nCount - 1; i++)
                                    for (int i = 1; i < strLength.GetLength(0) + 1; i++)
                                    {
                                        AdjustListView.Items[i].SubItems[6].Text = strLength[i - 1];
                                    }
                                }
                                break;

                            case 5: //一个连接角的附和导线
                                {
                                    strLine = sr.ReadLine();
                                    string[] strLength = strLine.Split(',');
                                    //for (int i = 1; i < nCount - 1; i++)
                                    for (int i = 1; i < strLength.GetLength(0) + 1; i++)
                                    {
                                        AdjustListView.Items[i].SubItems[4].Text = strLength[i - 1];
                                    }
                                }
                                break;

                            case 6: //两个连接角的附和导线
                                {
                                    strLine = sr.ReadLine();
                                    string[] strLength = strLine.Split(',');
                                    //for (int i = 1; i < nCount - 2; i++)
                                    for (int i = 1; i < strLength.GetLength(0) + 1; i++)
                                    {
                                        AdjustListView.Items[i].SubItems[6].Text = strLength[i - 1];
                                    }
                                }
                                break;

                            case 7: //支导线
                                {
                                    strLine = sr.ReadLine();
                                    string[] strLength = strLine.Split(',');
                                    //for (int i = 1; i < nCount - 1; i++)
                                    for (int i = 1; i < strLength.GetLength(0) + 1; i++)
                                    {
                                        AdjustListView.Items[i].SubItems[4].Text = strLength[i - 1];
                                    }
                                }
                                break;

                            case 8: //无连接角导线
                                {
                                    strLine = sr.ReadLine();
                                    string[] strLength = strLine.Split(',');
                                    //for (int i = 1; i < nCount - 1; i++)
                                    for (int i = 0; i < strLength.GetLength(0); i++)
                                    {
                                        AdjustListView.Items[i].SubItems[3].Text = strLength[i];
                                    }
                                }
                                break;
                        }
                        #endregion

                    }

                    int nnCount = AdjustListView.Items.Count;
                    AdjustListView.EnsureVisible(nnCount - 1);    //确保焦点显示到最后一行
                    nCalcFlag = 0;              //将计算完成标志设置为0.

                    sr.Close();

                    log.Info(DateTime.Now.ToString() + sender.ToString() + e.ToString());//写入一条新log
                }
                catch (Exception ex)
                {
                    log.Info(DateTime.Now.ToString() + sender.ToString() + ex.ToString());//写入一条新log
                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                    {
                        MessageBox.Show("导入的数据格式貌似少了一些必要数据或者格式错误，请仔细检查。", "Aurora智能提示");
                    }
                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                    {
                        MessageBox.Show("導入的資料格式貌似少了一些必要資料或者格式錯誤，請仔細檢查。", "Aurora智慧提示");
                    }
                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                    {
                        MessageBox.Show("Seems imported file is missing some data or format error.", "Aurora Intelligent Tips");
                    }
                }
                fs.Close();
            }
            else return;  
        }

        private void ToolStripMenuItem1_Export_Click(object sender, EventArgs e)                //文件-导出
        {
            int nCount = this.AdjustListView.Items.Count;

            switch (AdjustType.SelectedIndex)       //选择平差类型，进行对应平差
            {
                case 0: //闭合水准
                case 1: //附和水准
                    {
                        string strPTName = "", strDist = "", strStns = "", strObsLevelDiff = "", strAdjust = "", strAdjObsLevelDiff = "", strHeight = "";

                        string strFilePath;
                        SaveFileDialog sfd = new SaveFileDialog();
                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            sfd.Filter = "Txt文本文件(*.txt)|*.txt|Dat数据文件(*.dat)|*.dat|Excel交换文件(*.csv)|*.csv|所有文件(*.*)|*.*";
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            sfd.Filter = "Txt文字檔(*.txt)|*.txt|Dat資料檔案(*.dat)|*.dat|Excel交換檔(*.csv)|*.csv|所有檔(*.*)|*.*";
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            sfd.Filter = "Txt file(*.txt)|*.txt|Dat file(*.dat)|*.dat|Excel exchange file(*.csv)|*.csv|All file(*.*)|*.*";
                        }
                        
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            strFilePath = sfd.FileName;
                        }
                        else return;

                        FileStream aFile = new FileStream(strFilePath, FileMode.OpenOrCreate);
                        StreamWriter MyWriter = new StreamWriter(aFile, Encoding.GetEncoding("gb2312"));                //需要加上编码器，否则到处csv出现乱码.
                        if (AdjustType.SelectedIndex == 0)
                        {
                            MyWriter.WriteLine("ClosedLeveling");
                        }
                        if (AdjustType.SelectedIndex == 1)
                        {
                            MyWriter.WriteLine("AnnexedLeveling");
                        }

                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            MyWriter.WriteLine("点号" + "," + "距离(m)" + "," + "测站数" + "," + "实测高差(m)" + "," + "改正数(mm)" + "," + "改正后高差(m)" + "," + "高程(m)", Encoding.GetEncoding("GB2312"));
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            MyWriter.WriteLine("點號" + "," + "距離(m)" + "," + "測站數" + "," + "實測高差(m)" + "," + "改正數(mm)" + "," + "改正後高差(m)" + "," + "高程(m)", Encoding.GetEncoding("GB2312"));
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            MyWriter.WriteLine("PT" + "," + "Dist(m)" + "," + "Stations" + "," + "ObsLvlDiff(m)" + "," + "Adjust(mm)" + "," + "Adj-LvlDiff(m)" + "," + "Elevation(m)", Encoding.GetEncoding("GB2312"));
                        }

                        for (int i = 0; i < nCount; i++)
                        {
                            strPTName = this.AdjustListView.Items[i].SubItems[1].Text;
                            strDist = this.AdjustListView.Items[i].SubItems[2].Text;
                            strStns = this.AdjustListView.Items[i].SubItems[3].Text;
                            strObsLevelDiff = this.AdjustListView.Items[i].SubItems[4].Text;
                            strAdjust = this.AdjustListView.Items[i].SubItems[5].Text;
                            strAdjObsLevelDiff = this.AdjustListView.Items[i].SubItems[6].Text;
                            strHeight = this.AdjustListView.Items[i].SubItems[7].Text;

                            string strMyData = strPTName + "," + strDist + "," + strStns + "," + strObsLevelDiff + "," + strAdjust + "," + strAdjObsLevelDiff + "," + strHeight;
                            try
                            {
                                MyWriter.WriteLine(strMyData);
                            }
                            catch (Exception Err)
                            {
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                return;
                            }
                        }
                        if (MyWriter != null)
                        {
                            MyWriter.Close();
                        }
                        aFile.Close();
                    }
                    break;

                case 2: //支水准
                    {
                        string strPTName = "", strDist = "", strStns = "", strObsLevelDiff = "", strHeight = "";

                        string strFilePath;
                        SaveFileDialog sfd = new SaveFileDialog();
                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            sfd.Filter = "Txt文本文件(*.txt)|*.txt|Dat数据文件(*.dat)|*.dat|Excel交换文件(*.csv)|*.csv|所有文件(*.*)|*.*";
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            sfd.Filter = "Txt文字檔(*.txt)|*.txt|Dat資料檔案(*.dat)|*.dat|Excel交換檔(*.csv)|*.csv|所有檔(*.*)|*.*";
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            sfd.Filter = "Txt file(*.txt)|*.txt|Dat file(*.dat)|*.dat|Excel exchange file(*.csv)|*.csv|All file(*.*)|*.*";
                        }
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            strFilePath = sfd.FileName;
                        }
                        else return;

                        FileStream aFile = new FileStream(strFilePath, FileMode.OpenOrCreate);
                        StreamWriter MyWriter = new StreamWriter(aFile, Encoding.GetEncoding("gb2312"));
                        MyWriter.WriteLine("SpurLeveling");
                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            MyWriter.WriteLine("点号" + "," + "距离(m)" + "," + "测站数" + "," + "实测高差(m)" + "," + "高程(m)", Encoding.GetEncoding("GB2312"));
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            MyWriter.WriteLine("點號" + "," + "距離(m)" + "," + "測站數" + "," + "實測高差(m)" + "," + "高程(m)", Encoding.GetEncoding("GB2312"));
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            MyWriter.WriteLine("PT" + "," + "Dist(m)" + "," + "Stations" + "," + "ObsLvlDiff(m)" + "," + "Elevation(m)", Encoding.GetEncoding("GB2312"));
                        }
                        for (int i = 0; i < nCount; i++)
                        {
                            strPTName = this.AdjustListView.Items[i].SubItems[1].Text;
                            strDist = this.AdjustListView.Items[i].SubItems[2].Text;
                            strStns = this.AdjustListView.Items[i].SubItems[3].Text;
                            strObsLevelDiff = this.AdjustListView.Items[i].SubItems[4].Text;
                            strHeight = this.AdjustListView.Items[i].SubItems[5].Text;

                            string strMyData = strPTName + "," + strDist + "," + strStns + "," + strObsLevelDiff + "," + strHeight;
                            try
                            {
                                MyWriter.WriteLine(strMyData);
                            }
                            catch (Exception Err)
                            {
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                return;
                            }
                        }
                        if (MyWriter != null)
                        {
                            MyWriter.Close();
                        }
                    }
                    break;

                case 3: //闭合导线
                    {
                        string strStnName = "", strObsAngle = "", strAdjust = "", strAdjObsAngle = "", strAzimuth = "", strLength = "", strDeltaX = "", strDeltaY = "";
                        string strAdjDeltaX = "", strAdjDeltaY = "", strCoorX = "", strCoorY = "";

                        string strFilePath;
                        SaveFileDialog sfd = new SaveFileDialog();
                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            sfd.Filter = "Txt文本文件(*.txt)|*.txt|Dat数据文件(*.dat)|*.dat|Excel交换文件(*.csv)|*.csv|所有文件(*.*)|*.*";
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            sfd.Filter = "Txt文字檔(*.txt)|*.txt|Dat資料檔案(*.dat)|*.dat|Excel交換檔(*.csv)|*.csv|所有檔(*.*)|*.*";
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            sfd.Filter = "Txt file(*.txt)|*.txt|Dat file(*.dat)|*.dat|Excel exchange file(*.csv)|*.csv|All file(*.*)|*.*";
                        }
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            strFilePath = sfd.FileName;
                        }
                        else return;

                        FileStream aFile = new FileStream(strFilePath, FileMode.OpenOrCreate);
                        StreamWriter MyWriter = new StreamWriter(aFile, Encoding.GetEncoding("gb2312"));
                        
                        MyWriter.WriteLine("ClosedTraverse");
                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            MyWriter.WriteLine("测站" + "," + "观测角度" + "," + "改正数" + "," + "改正后角度" + "," + "方位角" + "," + "边长(m)" + "," + "X坐标增量ΔX(m)" + "," + "Y坐标增量ΔY(m)" + "," + "改正后ΔX(m)" + "," + "改正后ΔY(m)" + "," + "X坐标(m)" + "," + "Y坐标(m)", Encoding.GetEncoding("GB2312"));
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            MyWriter.WriteLine("測站" + "," + "觀測角度" + "," + "改正數" + "," + "改正後角度" + "," + "方位角" + "," + "邊長(m)" + "," + "X座標增量ΔX(m)" + "," + "Y座標增量ΔY(m)" + "," + "改正後ΔX(m)" + "," + "改正後ΔY(m)" + "," + "X座標(m)" + "," + "Y座標(m)", Encoding.GetEncoding("GB2312"));
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            MyWriter.WriteLine("Station" + "," + "Meas Angle" + "," + "Adjust" + "," + "Adj-Angle" + "," + "Azimuth" + "," + "Length(m)" + "," + "ΔX(m)" + "," + "ΔY(m)" + "," + "Adj-ΔX(m)" + "," + "Adj-ΔY(m)" + "," + "X(m)" + "," + "Y(m)", Encoding.GetEncoding("GB2312"));
                        }
                        for (int i = 0; i < nCount; i++)
                        {
                            strStnName = this.AdjustListView.Items[i].SubItems[1].Text;
                            strObsAngle = this.AdjustListView.Items[i].SubItems[2].Text;
                            strAdjust = this.AdjustListView.Items[i].SubItems[3].Text;
                            strAdjObsAngle = this.AdjustListView.Items[i].SubItems[4].Text;
                            strAzimuth = this.AdjustListView.Items[i].SubItems[5].Text;
                            strLength = this.AdjustListView.Items[i].SubItems[6].Text;
                            strDeltaX = this.AdjustListView.Items[i].SubItems[7].Text;
                            strDeltaY = this.AdjustListView.Items[i].SubItems[8].Text;
                            strAdjDeltaX = this.AdjustListView.Items[i].SubItems[9].Text;
                            strAdjDeltaY = this.AdjustListView.Items[i].SubItems[10].Text;
                            strCoorX = this.AdjustListView.Items[i].SubItems[11].Text;
                            strCoorY = this.AdjustListView.Items[i].SubItems[12].Text;

                            string strMyData = strStnName + "," + strObsAngle + "," + strAdjust + "," + strAdjObsAngle + "," + strAzimuth + "," + strLength + "," + strDeltaX + "," + strDeltaY + "," + strAdjDeltaX + "," + strAdjDeltaY + "," + strCoorX + "," + strCoorY;
                            try
                            {
                                MyWriter.WriteLine(strMyData);
                            }
                            catch (Exception Err)
                            {
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                return;
                            }
                        }
                        if (MyWriter != null)
                        {
                            MyWriter.Close();
                        }
                    }
                    break;

                case 4: //闭合导线(含外支点)
                    {
                        string strStnName = "", strObsAngle = "", strAdjust = "", strAdjObsAngle = "", strAzimuth = "", strLength = "", strDeltaX = "", strDeltaY = "";
                        string strAdjDeltaX = "", strAdjDeltaY = "", strCoorX = "", strCoorY = "";

                        string strFilePath;
                        SaveFileDialog sfd = new SaveFileDialog();
                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            sfd.Filter = "Txt文本文件(*.txt)|*.txt|Dat数据文件(*.dat)|*.dat|Excel交换文件(*.csv)|*.csv|所有文件(*.*)|*.*";
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            sfd.Filter = "Txt文字檔(*.txt)|*.txt|Dat資料檔案(*.dat)|*.dat|Excel交換檔(*.csv)|*.csv|所有檔(*.*)|*.*";
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            sfd.Filter = "Txt file(*.txt)|*.txt|Dat file(*.dat)|*.dat|Excel exchange file(*.csv)|*.csv|All file(*.*)|*.*";
                        }
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            strFilePath = sfd.FileName;
                        }
                        else return;

                        FileStream aFile = new FileStream(strFilePath, FileMode.OpenOrCreate);
                        StreamWriter MyWriter = new StreamWriter(aFile, Encoding.GetEncoding("gb2312"));
                        MyWriter.WriteLine("ClosedTraverseWithOuterPoint");
                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            MyWriter.WriteLine("测站" + "," + "观测角度" + "," + "改正数" + "," + "改正后角度" + "," + "方位角" + "," + "边长(m)" + "," + "X坐标增量ΔX(m)" + "," + "Y坐标增量ΔY(m)" + "," + "改正后ΔX(m)" + "," + "改正后ΔY(m)" + "," + "X坐标(m)" + "," + "Y坐标(m)", Encoding.GetEncoding("GB2312"));
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            MyWriter.WriteLine("測站" + "," + "觀測角度" + "," + "改正數" + "," + "改正後角度" + "," + "方位角" + "," + "邊長(m)" + "," + "X座標增量ΔX(m)" + "," + "Y座標增量ΔY(m)" + "," + "改正後ΔX(m)" + "," + "改正後ΔY(m)" + "," + "X座標(m)" + "," + "Y座標(m)", Encoding.GetEncoding("GB2312"));
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            MyWriter.WriteLine("Station" + "," + "Meas Angle" + "," + "Adjust" + "," + "Adj-Angle" + "," + "Azimuth" + "," + "Length(m)" + "," + "ΔX(m)" + "," + "ΔY(m)" + "," + "Adj-ΔX(m)" + "," + "Adj-ΔY(m)" + "," + "X(m)" + "," + "Y(m)", Encoding.GetEncoding("GB2312"));
                        }
                        for (int i = 0; i < nCount; i++)
                        {
                            strStnName = this.AdjustListView.Items[i].SubItems[1].Text;
                            strObsAngle = this.AdjustListView.Items[i].SubItems[2].Text;
                            strAdjust = this.AdjustListView.Items[i].SubItems[3].Text;
                            strAdjObsAngle = this.AdjustListView.Items[i].SubItems[4].Text;
                            strAzimuth = this.AdjustListView.Items[i].SubItems[5].Text;
                            strLength = this.AdjustListView.Items[i].SubItems[6].Text;
                            strDeltaX = this.AdjustListView.Items[i].SubItems[7].Text;
                            strDeltaY = this.AdjustListView.Items[i].SubItems[8].Text;
                            strAdjDeltaX = this.AdjustListView.Items[i].SubItems[9].Text;
                            strAdjDeltaY = this.AdjustListView.Items[i].SubItems[10].Text;
                            strCoorX = this.AdjustListView.Items[i].SubItems[11].Text;
                            strCoorY = this.AdjustListView.Items[i].SubItems[12].Text;

                            string strMyData = strStnName + "," + strObsAngle + "," + strAdjust + "," + strAdjObsAngle + "," + strAzimuth + "," + strLength + "," + strDeltaX + "," + strDeltaY + "," + strAdjDeltaX + "," + strAdjDeltaY + "," + strCoorX + "," + strCoorY;
                            try
                            {
                                MyWriter.WriteLine(strMyData);
                            }
                            catch (Exception Err)
                            {
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                return;
                            }
                        }
                        if (MyWriter != null)
                        {
                            MyWriter.Close();
                        }
                    }
                    break;

                case 5: //一个连接角的附和导线
                    {
                        string strStnName = "", strObsAngle = "", strAzimuth = "", strLength = "", strDeltaX = "", strDeltaY = "";
                        string strAdjDeltaX = "", strAdjDeltaY = "", strCoorX = "", strCoorY = "";

                        string strFilePath;
                        SaveFileDialog sfd = new SaveFileDialog();
                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            sfd.Filter = "Txt文本文件(*.txt)|*.txt|Dat数据文件(*.dat)|*.dat|Excel交换文件(*.csv)|*.csv|所有文件(*.*)|*.*";
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            sfd.Filter = "Txt文字檔(*.txt)|*.txt|Dat資料檔案(*.dat)|*.dat|Excel交換檔(*.csv)|*.csv|所有檔(*.*)|*.*";
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            sfd.Filter = "Txt file(*.txt)|*.txt|Dat file(*.dat)|*.dat|Excel exchange file(*.csv)|*.csv|All file(*.*)|*.*";
                        }
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            strFilePath = sfd.FileName;
                        }
                        else return;

                        FileStream aFile = new FileStream(strFilePath, FileMode.OpenOrCreate);
                        StreamWriter MyWriter = new StreamWriter(aFile, Encoding.GetEncoding("gb2312"));
                        MyWriter.WriteLine("OneAngleConnTraverse");
                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            MyWriter.WriteLine("测站" + "," + "观测角度" + "," + "方位角" + "," + "边长(m)" + "," + "X坐标增量ΔX(m)" + "," + "Y坐标增量ΔY(m)" + "," + "改正后ΔX(m)" + "," + "改正后ΔY(m)" + "," + "X坐标(m)" + "," + "Y坐标(m)", Encoding.GetEncoding("GB2312"));
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            MyWriter.WriteLine("測站" + "," + "觀測角度" + "," + "方位角" + "," + "邊長(m)" + "," + "X座標增量ΔX(m)" + "," + "Y座標增量ΔY(m)" + "," + "改正後ΔX(m)" + "," + "改正後ΔY(m)" + "," + "X座標(m)" + "," + "Y座標(m)", Encoding.GetEncoding("GB2312"));
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            MyWriter.WriteLine("Station" + "," + "Meas Angle" + "," + "Azimuth" + "," + "Length(m)" + "," + "ΔX(m)" + "," + "ΔY(m)" + "," + "Adj-ΔX(m)" + "," + "Adj-ΔY(m)" + "," + "X(m)" + "," + "Y(m)", Encoding.GetEncoding("GB2312"));
                        }
                        for (int i = 0; i < nCount; i++)
                        {
                            strStnName = this.AdjustListView.Items[i].SubItems[1].Text;
                            strObsAngle = this.AdjustListView.Items[i].SubItems[2].Text;
                            strAzimuth = this.AdjustListView.Items[i].SubItems[3].Text;
                            strLength = this.AdjustListView.Items[i].SubItems[4].Text;
                            strDeltaX = this.AdjustListView.Items[i].SubItems[5].Text;
                            strDeltaY = this.AdjustListView.Items[i].SubItems[6].Text;
                            strAdjDeltaX = this.AdjustListView.Items[i].SubItems[7].Text;
                            strAdjDeltaY = this.AdjustListView.Items[i].SubItems[8].Text;
                            strCoorX = this.AdjustListView.Items[i].SubItems[9].Text;
                            strCoorY = this.AdjustListView.Items[i].SubItems[10].Text;

                            string strMyData = strStnName + "," + strObsAngle + "," + strAzimuth + "," + strLength + "," + strDeltaX + "," + strDeltaY + "," + strAdjDeltaX + "," + strAdjDeltaY + "," + strCoorX + "," + strCoorY;
                            try
                            {
                                MyWriter.WriteLine(strMyData);
                            }
                            catch (Exception Err)
                            {
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                return;
                            }
                        }
                        if (MyWriter != null)
                        {
                            MyWriter.Close();
                        }
                    }
                    break;

                case 6: //两个连接角的附和导线
                    {
                        string strStnName = "", strObsAngle = "", strAdjust = "", strAdjObsAngle = "", strAzimuth = "", strLength = "", strDeltaX = "", strDeltaY = "";
                        string strAdjDeltaX = "", strAdjDeltaY = "", strCoorX = "", strCoorY = "";

                        string strFilePath;
                        SaveFileDialog sfd = new SaveFileDialog();
                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            sfd.Filter = "Txt文本文件(*.txt)|*.txt|Dat数据文件(*.dat)|*.dat|Excel交换文件(*.csv)|*.csv|所有文件(*.*)|*.*";
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            sfd.Filter = "Txt文字檔(*.txt)|*.txt|Dat資料檔案(*.dat)|*.dat|Excel交換檔(*.csv)|*.csv|所有檔(*.*)|*.*";
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            sfd.Filter = "Txt file(*.txt)|*.txt|Dat file(*.dat)|*.dat|Excel exchange file(*.csv)|*.csv|All file(*.*)|*.*";
                        }
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            strFilePath = sfd.FileName;
                        }
                        else return;

                        FileStream aFile = new FileStream(strFilePath, FileMode.OpenOrCreate);
                        StreamWriter MyWriter = new StreamWriter(aFile, Encoding.GetEncoding("gb2312"));
                        MyWriter.WriteLine("TwoAngleConnTraverse");
                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            MyWriter.WriteLine("测站" + "," + "观测角度" + "," + "改正数" + "," + "改正后角度" + "," + "方位角" + "," + "边长(m)" + "," + "X坐标增量ΔX(m)" + "," + "Y坐标增量ΔY(m)" + "," + "改正后ΔX(m)" + "," + "改正后ΔY(m)" + "," + "X坐标(m)" + "," + "Y坐标(m)", Encoding.GetEncoding("GB2312"));
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            MyWriter.WriteLine("測站" + "," + "觀測角度" + "," + "改正數" + "," + "改正後角度" + "," + "方位角" + "," + "邊長(m)" + "," + "X座標增量ΔX(m)" + "," + "Y座標增量ΔY(m)" + "," + "改正後ΔX(m)" + "," + "改正後ΔY(m)" + "," + "X座標(m)" + "," + "Y座標(m)", Encoding.GetEncoding("GB2312"));
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            MyWriter.WriteLine("Station" + "," + "Meas Angle" + "," + "Adjust" + "," + "Adj-Angle" + "," + "Azimuth" + "," + "Length(m)" + "," + "ΔX(m)" + "," + "ΔY(m)" + "," + "Adj-ΔX(m)" + "," + "Adj-ΔY(m)" + "," + "X(m)" + "," + "Y(m)", Encoding.GetEncoding("GB2312"));
                        }
                        for (int i = 0; i < nCount; i++)
                        {
                            strStnName = this.AdjustListView.Items[i].SubItems[1].Text;
                            strObsAngle = this.AdjustListView.Items[i].SubItems[2].Text;
                            strAdjust = this.AdjustListView.Items[i].SubItems[3].Text;
                            strAdjObsAngle = this.AdjustListView.Items[i].SubItems[4].Text;
                            strAzimuth = this.AdjustListView.Items[i].SubItems[5].Text;
                            strLength = this.AdjustListView.Items[i].SubItems[6].Text;
                            strDeltaX = this.AdjustListView.Items[i].SubItems[7].Text;
                            strDeltaY = this.AdjustListView.Items[i].SubItems[8].Text;
                            strAdjDeltaX = this.AdjustListView.Items[i].SubItems[9].Text;
                            strAdjDeltaY = this.AdjustListView.Items[i].SubItems[10].Text;
                            strCoorX = this.AdjustListView.Items[i].SubItems[11].Text;
                            strCoorY = this.AdjustListView.Items[i].SubItems[12].Text;

                            string strMyData = strStnName + "," + strObsAngle + "," + strAdjust + "," + strAdjObsAngle + "," + strAzimuth + "," + strLength + "," + strDeltaX + "," + strDeltaY + "," + strAdjDeltaX + "," + strAdjDeltaY + "," + strCoorX + "," + strCoorY;
                            try
                            {
                                MyWriter.WriteLine(strMyData);
                            }
                            catch (Exception Err)
                            {
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                return;
                            }
                        }
                        if (MyWriter != null)
                        {
                            MyWriter.Close();
                        }
                    }
                    break;

                case 7: //支导线
                    {
                        string strStnName = "", strObsAngle = "", strAzimuth = "", strLength = "", strDeltaX = "", strDeltaY = "", strCoorX = "", strCoorY = "";

                        string strFilePath;
                        SaveFileDialog sfd = new SaveFileDialog();
                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            sfd.Filter = "Txt文本文件(*.txt)|*.txt|Dat数据文件(*.dat)|*.dat|Excel交换文件(*.csv)|*.csv|所有文件(*.*)|*.*";
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            sfd.Filter = "Txt文字檔(*.txt)|*.txt|Dat資料檔案(*.dat)|*.dat|Excel交換檔(*.csv)|*.csv|所有檔(*.*)|*.*";
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            sfd.Filter = "Txt file(*.txt)|*.txt|Dat file(*.dat)|*.dat|Excel exchange file(*.csv)|*.csv|All file(*.*)|*.*";
                        }
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            strFilePath = sfd.FileName;
                        }
                        else return;

                        FileStream aFile = new FileStream(strFilePath, FileMode.OpenOrCreate);
                        StreamWriter MyWriter = new StreamWriter(aFile, Encoding.GetEncoding("gb2312"));
                        MyWriter.WriteLine("OpenTraverse");
                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            MyWriter.WriteLine("测站" + "," + "观测角度" + "," + "方位角" + "," + "边长(m)" + "," + "X坐标增量ΔX(m)" + "," + "Y坐标增量ΔY(m)" + "," + "X坐标(m)" + "," + "Y坐标(m)", Encoding.GetEncoding("GB2312"));
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            MyWriter.WriteLine("測站" + "," + "觀測角度" + "," + "方位角" + "," + "邊長(m)" + "," + "X座標增量ΔX(m)" + "," + "Y座標增量ΔY(m)" + "," + "X座標(m)" + "," + "Y座標(m)", Encoding.GetEncoding("GB2312"));
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            MyWriter.WriteLine("Station" + "," + "Meas Angle" + "," + "Azimuth" + "," + "Length(m)" + "," + "ΔX(m)" + "," + "ΔY(m)" + "," + "X(m)" + "," + "Y(m)", Encoding.GetEncoding("GB2312"));
                        }
                        for (int i = 0; i < nCount; i++)
                        {
                            strStnName = this.AdjustListView.Items[i].SubItems[1].Text;
                            strObsAngle = this.AdjustListView.Items[i].SubItems[2].Text;
                            strAzimuth = this.AdjustListView.Items[i].SubItems[3].Text;
                            strLength = this.AdjustListView.Items[i].SubItems[4].Text;
                            strDeltaX = this.AdjustListView.Items[i].SubItems[5].Text;
                            strDeltaY = this.AdjustListView.Items[i].SubItems[6].Text;
                            strCoorX = this.AdjustListView.Items[i].SubItems[7].Text;
                            strCoorY = this.AdjustListView.Items[i].SubItems[8].Text;

                            string strMyData = strStnName + "," + strObsAngle + "," + strAzimuth + "," + strLength + "," + strDeltaX + "," + strDeltaY + "," + strCoorX + "," + strCoorY;
                            try
                            {
                                MyWriter.WriteLine(strMyData);
                            }
                            catch (Exception Err)
                            {
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                return;
                            }
                        }
                        if (MyWriter != null)
                        {
                            MyWriter.Close();
                        }
                    }
                    break;
                case 8: //无连接角导线
                    {
                        string strStnName = "", strObsAngle = "", strLength = "", strAssumedAzimuth = "", strAssumedDeltaX = "", strAssumedDeltaY = "", strAssumedCoorX = "", strAssumedCoorY = "", strCoorX = "", strCoorY = "";

                        string strFilePath;
                        SaveFileDialog sfd = new SaveFileDialog();
                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            sfd.Filter = "Txt文本文件(*.txt)|*.txt|Dat数据文件(*.dat)|*.dat|Excel交换文件(*.csv)|*.csv|所有文件(*.*)|*.*";
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            sfd.Filter = "Txt文字檔(*.txt)|*.txt|Dat資料檔案(*.dat)|*.dat|Excel交換檔(*.csv)|*.csv|所有檔(*.*)|*.*";
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            sfd.Filter = "Txt file(*.txt)|*.txt|Dat file(*.dat)|*.dat|Excel exchange file(*.csv)|*.csv|All file(*.*)|*.*";
                        }
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            strFilePath = sfd.FileName;
                        }
                        else return;

                        FileStream aFile = new FileStream(strFilePath, FileMode.OpenOrCreate);
                        StreamWriter MyWriter = new StreamWriter(aFile, Encoding.GetEncoding("gb2312"));
                        MyWriter.WriteLine("OpenTraverse");
                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            MyWriter.WriteLine("测站" + "," + "观测角度" + "," + "边长(m)" + "," + "假定方位角" + "," + "假定X坐标增量ΔX(m)" + "," + "假定Y坐标增量ΔY(m)" + "," + "假定X坐标(m)" + "," + "假定Y坐标(m)" + "," + "X坐标(m)" + "," + "Y坐标(m)", Encoding.GetEncoding("GB2312"));
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            MyWriter.WriteLine("測站" + "," + "觀測角度" + "," + "邊長(m)" + "," + "假定方位角" + "," + "假定X座標增量ΔX(m)" + "," + "假定Y座標增量ΔY(m)" + "," + "假定X座標(m)" + "," + "假定Y座標(m)" + "," + "X座標(m)" + "," + "Y座標(m)", Encoding.GetEncoding("GB2312"));
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            MyWriter.WriteLine("Station" + "," + "Meas Angle" + "," + "Length(m)" + "," + "Assumed Azimuth" + "," + "Assumed ΔX(m)" + "," + "Assumed ΔY(m)" + "," + "Assumed X(m)" + "," + "Assumed Y(m)" + "," + "X(m)" + "," + "Y(m)", Encoding.GetEncoding("GB2312"));
                        }
                        for (int i = 0; i < nCount; i++)
                        {
                            strStnName = this.AdjustListView.Items[i].SubItems[1].Text;
                            strObsAngle = this.AdjustListView.Items[i].SubItems[2].Text;
                            strLength = this.AdjustListView.Items[i].SubItems[3].Text;
                            strAssumedAzimuth = this.AdjustListView.Items[i].SubItems[4].Text;
                            strAssumedDeltaX = this.AdjustListView.Items[i].SubItems[5].Text;
                            strAssumedDeltaY = this.AdjustListView.Items[i].SubItems[6].Text;
                            strAssumedCoorX = this.AdjustListView.Items[i].SubItems[7].Text;
                            strAssumedCoorY = this.AdjustListView.Items[i].SubItems[8].Text;
                            strCoorX = this.AdjustListView.Items[i].SubItems[9].Text;
                            strCoorY = this.AdjustListView.Items[i].SubItems[10].Text;

                            string strMyData = strStnName + "," + strObsAngle + "," + strLength + "," + strAssumedAzimuth + "," + strAssumedDeltaX + "," + strAssumedDeltaY + "," + strAssumedCoorX + "," + strAssumedCoorY + "," + strCoorX + "," + strCoorY;
                            try
                            {
                                MyWriter.WriteLine(strMyData);
                            }
                            catch (Exception Err)
                            {
                                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                {
                                    MessageBox.Show(Err.ToString(), "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                return;
                            }
                        }
                        if (MyWriter != null)
                        {
                            MyWriter.Close();
                        }
                    }
                    break;
            }
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                MessageBox.Show("数据导出成功。", "Aurora智能提示");
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                MessageBox.Show("資料匯出成功。", "Aurora智慧提示");
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                MessageBox.Show("Data export success.", "Aurora Intelligent Tips");
            }
            
        }

        private void ToolStripMenuItem_Exit_Click(object sender, EventArgs e)                //文件-退出
        {
            Application.Exit();
            log.Info(DateTime.Now.ToString() + " Close Aurora");             //写入一条新log

        }

        private void ToolStripMenuItem_Add_Click(object sender, EventArgs e)               //编辑-添加行
        {
            toolStripButton_Add_Click(sender, e);
        }

        private void ToolStripMenuItem_Delete_Click(object sender, EventArgs e)               //编辑-删除行
        {
            toolStripButton_Delete_Click(sender, e);
        }

        private void ToolStripMenuItem_Clear_Click(object sender, EventArgs e)               //编辑-清空列表
        {
            toolStripButton_Clear_Click(sender, e);
        }

        private void ToolStripMenuItem_Adjust_Click(object sender, EventArgs e)             //平差计算
        {
            toolStripButton_Calc_Click(sender, e);
        }

        private void ToolStripMenuItem_Mapping_Click(object sender, EventArgs e)                //绘图
        {
            toolStripButton_Mapping_Click(sender, e);
        }

        private void ToolStripMenuItem_Report_Click(object sender, EventArgs e)             //生成报表
        {
            toolStripButton_Report_Click(sender, e);
        }

        private void toolStripMenuItem_CMD_Click(object sender, EventArgs e)                //命令行
        {
            toolStripButton_CMD_Click(sender, e);
        }

        private void toolStripMenuItem_OpenLocker_Click(object sender, EventArgs e)             //启用数据锁
        {
            toolStripButton_Locker_Click(sender, e);
        }

        private void ToolStripMenuItem_Setup_Click(object sender, EventArgs e)              //设置
        {
            Setting FrmSetup = new Setting();
            FrmSetup.StartPosition = FormStartPosition.CenterParent;
            FrmSetup.ShowDialog(this);      //this 必须有，传递子窗体参数       //创建模态对话框
            //FrmSetup.Show(this);      //this 必须有，传递子窗体参数       //创建非模态对话框
        }

        private void toolStripMenuItem_Chs_Click(object sender, EventArgs e)                //简体中文
        {
            DialogResult dr = DialogResult.Yes;
            dr = MessageBox.Show("即将切换到简体中文界面，列表需要重新加载，数据将被清空。" + "\r\n" + "\r\n"
                            + "点击'是'继续，点击'否'返回操作。", "Aurora智能提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                RegistryKey MyReg;
                RegistryKey RegLanguage;
                MyReg = Registry.CurrentUser;
                try
                {
                    RegLanguage = MyReg.CreateSubKey("Software\\Aurora\\Language");
                    RegLanguage.SetValue("Language", "zh-CN");
                }
                catch { }

                this.WindowState = FormWindowState.Normal;
                this.StartPosition = FormStartPosition.CenterScreen;
                //this.Size = new Size(nNowWidth, nNowHeight);

                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("zh-CN");
                ApplyResource();
                toolStripMenuItem_Chs.Checked = true;
                toolStripMenuItem_Cht.Checked = false;
                toolStripMenuItem_En.Checked = false;

                AdjustType.Items.Clear();
                AdjustType.Items.Add("闭合水准");
                AdjustType.Items.Add("附合水准");
                AdjustType.Items.Add("支水准");
                AdjustType.Items.Add("闭合导线");
                AdjustType.Items.Add("闭合导线(含外支点)");
                AdjustType.Items.Add("具有一个连接角的附和导线");
                AdjustType.Items.Add("具有两个连接角的附和导线");
                AdjustType.Items.Add("支导线");
                AdjustType.Items.Add("无连接角导线");
                AdjustType.Items.Add("水准网");
                AdjustType.Text = "闭合导线";

                AngleMode.Items.Clear();
                AngleMode.Items.Add("左角");
                AngleMode.Items.Add("右角");
                AngleMode.Text = "左角";

                LevelMode.Items.Clear();
                LevelMode.Items.Add("距离");
                LevelMode.Items.Add("测站数");
                LevelMode.Text = "距离";

                MeasGrade.Items.Clear();
                MeasGrade.Items.Add("三等");
                MeasGrade.Items.Add("四等");
                MeasGrade.Items.Add("一级");
                MeasGrade.Items.Add("二级");
                MeasGrade.Items.Add("三级");
                MeasGrade.Items.Add("自定义");
                MeasGrade.Text = "三级";

                MeasArea.Items.Clear();
                MeasArea.Items.Add("平地");
                MeasArea.Items.Add("山地");
                MeasArea.Text = "平地";
                MeasArea.Visible = false;

                toolStripTextBox1.Text = "图标大小：";
                toolStripComboBox1.Items.Clear();
                toolStripComboBox1.Items.Add("小图标");
                toolStripComboBox1.Items.Add("中图标");
                toolStripComboBox1.Items.Add("大图标");
                toolStripComboBox1.Text = "中图标";
                toolStripTextBox2.Text = "图标大小：";
                toolStripComboBox2.Items.Clear();
                toolStripComboBox2.Items.Add("小图标");
                toolStripComboBox2.Items.Add("中图标");
                toolStripComboBox2.Items.Add("大图标");
                toolStripComboBox2.Text = "中图标";
                toolStripTextBox3.Text = "图标大小：";
                toolStripComboBox3.Items.Clear();
                toolStripComboBox3.Items.Add("小图标");
                toolStripComboBox3.Items.Add("中图标");
                toolStripComboBox3.Text = "中图标";

                this.AdjustListView.Clear();
                this.AdjustListView.Columns.Add("序号", 50, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("测站", 50, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("观测角度", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("改正数", 60, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("改正后角度", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("方位角", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("边长(m)", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("X坐标增量ΔX(m)", 110, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Y坐标增量ΔY(m)", 110, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("改正后ΔX(m)", 100, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("改正后ΔY(m)", 100, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("X坐标(m)", 100, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Y坐标(m)", 100, HorizontalAlignment.Center);
            }
        }

        private void toolStripMenuItem_Cht_Click(object sender, EventArgs e)                //繁体中文
        {
            DialogResult dr = DialogResult.Yes;
            dr = MessageBox.Show("即將切換到繁體中文介面，清單需要重載，資料將被清空。" + "\r\n" + "\r\n"
                            + "點擊'是'繼續，點擊'否'返回操作。", "Aurora智慧提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                RegistryKey MyReg;
                RegistryKey RegLanguage;
                MyReg = Registry.CurrentUser;
                try
                {
                    RegLanguage = MyReg.CreateSubKey("Software\\Aurora\\Language");
                    RegLanguage.SetValue("Language", "zh-Hant");
                }
                catch { }


                this.WindowState = FormWindowState.Normal;
                this.StartPosition = FormStartPosition.CenterScreen;

                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("zh-Hant");
                ApplyResource();
                toolStripMenuItem_Chs.Checked = false;
                toolStripMenuItem_Cht.Checked = true;
                toolStripMenuItem_En.Checked = false;

                AdjustType.Items.Clear();
                AdjustType.Items.Add("閉合水準");
                AdjustType.Items.Add("附合水準");
                AdjustType.Items.Add("支水準");
                AdjustType.Items.Add("閉合導線");
                AdjustType.Items.Add("閉合導線(含外支點)");
                AdjustType.Items.Add("具有一个連接角的附和導線");
                AdjustType.Items.Add("具有两个連接角的附和導線");
                AdjustType.Items.Add("支導線");
                AdjustType.Items.Add("无連接角導線");
                AdjustType.Items.Add("水準網");
                AdjustType.Text = "閉合導線";

                AngleMode.Items.Clear();
                AngleMode.Items.Add("左角");
                AngleMode.Items.Add("右角");
                AngleMode.Text = "左角";

                LevelMode.Items.Clear();
                LevelMode.Items.Add("距離");
                LevelMode.Items.Add("測站數");
                LevelMode.Text = "距離";

                MeasGrade.Items.Clear();
                MeasGrade.Items.Add("叁等");
                MeasGrade.Items.Add("肆等");
                MeasGrade.Items.Add("壹级");
                MeasGrade.Items.Add("貳级");
                MeasGrade.Items.Add("叁级");
                MeasGrade.Items.Add("自定義");
                MeasGrade.Text = "叁级";

                MeasArea.Items.Clear();
                MeasArea.Items.Add("平地");
                MeasArea.Items.Add("山地");
                MeasArea.Text = "平地";
                MeasArea.Visible = false;

                toolStripTextBox1.Text = "圖標大小：";
                toolStripComboBox1.Items.Clear();
                toolStripComboBox1.Items.Add("小圖標");
                toolStripComboBox1.Items.Add("中圖標");
                toolStripComboBox1.Items.Add("大圖標");
                toolStripComboBox1.Text = "中圖標";
                toolStripTextBox2.Text = "圖標大小：";
                toolStripComboBox2.Items.Clear();
                toolStripComboBox2.Items.Add("小圖標");
                toolStripComboBox2.Items.Add("中圖標");
                toolStripComboBox2.Items.Add("大圖標");
                toolStripComboBox2.Text = "中圖標";
                toolStripTextBox3.Text = "圖標大小：";
                toolStripComboBox3.Items.Clear();
                toolStripComboBox3.Items.Add("小圖標");
                toolStripComboBox3.Items.Add("中圖標");
                toolStripComboBox3.Text = "中圖標";

                this.AdjustListView.Clear();
                this.AdjustListView.Columns.Add("序號", 50, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("測站", 50, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("觀測角度", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("改正數", 60, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("改正後角度", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("方位角", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("邊長(m)", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("X座標增量ΔX(m)", 110, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Y座標增量ΔY(m)", 110, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("改正後ΔX(m)", 100, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("改正後ΔY(m)", 100, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("X座標(m)", 100, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Y座標(m)", 100, HorizontalAlignment.Center);
            }
        }
   
        private void toolStripMenuItem_En_Click(object sender, EventArgs e)             //英文
        {
            DialogResult dr = DialogResult.Yes;
            dr = MessageBox.Show("Aurora is loading English interface. Data list need to be reloaded, and the data will be clear." + "\r\n" + "\r\n"
                            + "Click 'YES' to continue, or 'NO' to return.", "Aurora Intelligent Tips", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                RegistryKey MyReg;
                RegistryKey RegLanguage;
                MyReg = Registry.CurrentUser;
                try
                {
                    RegLanguage = MyReg.CreateSubKey("Software\\Aurora\\Language");
                    RegLanguage.SetValue("Language", "en");
                }
                catch { }

                this.WindowState = FormWindowState.Normal;
                this.StartPosition = FormStartPosition.CenterScreen;

                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");
                ApplyResource();
                toolStripMenuItem_Chs.Checked = false;
                toolStripMenuItem_Cht.Checked = false;
                toolStripMenuItem_En.Checked = true;

                AdjustType.Items.Clear();
                AdjustType.Items.Add("Closed Leveling");
                AdjustType.Items.Add("Annexed Leveling");
                AdjustType.Items.Add("Spur Leveling");
                AdjustType.Items.Add("Closed Traverse");
                AdjustType.Items.Add("Closed Traverse With Outer Point");
                AdjustType.Items.Add("One Angle Conn-Traverse");
                AdjustType.Items.Add("Two Angle Conn-Traverse");
                AdjustType.Items.Add("Open Traverse");
                AdjustType.Items.Add("No Angle Conn-Traverse");
                AdjustType.Items.Add("Leveling Network");
                AdjustType.Text = "Closed Traverse";

                AngleMode.Items.Clear();
                AngleMode.Items.Add("Left");
                AngleMode.Items.Add("Right");
                AngleMode.Text = "Left";

                LevelMode.Items.Clear();
                LevelMode.Items.Add("Dist");
                LevelMode.Items.Add("Stations");
                LevelMode.Text = "Dist";

                MeasGrade.Items.Clear();
                MeasGrade.Items.Add("3rd Grade");
                MeasGrade.Items.Add("4th Grade");
                MeasGrade.Items.Add("1st Class");
                MeasGrade.Items.Add("2nd Class");
                MeasGrade.Items.Add("3rd Class");
                MeasGrade.Items.Add("User defined");
                MeasGrade.Text = "3rd Class";

                MeasArea.Items.Clear();
                MeasArea.Items.Add("Flat");
                MeasArea.Items.Add("Hill");
                MeasArea.Text = "Flat";
                MeasArea.Visible = false;

                toolStripTextBox1.Text = "Icon Size:";
                toolStripComboBox1.Items.Clear();
                toolStripComboBox1.Items.Add("Small");
                toolStripComboBox1.Items.Add("Medium");
                toolStripComboBox1.Items.Add("Large");
                toolStripComboBox1.Text = "Medium";
                toolStripTextBox2.Text = "Icon Size:";
                toolStripComboBox2.Items.Clear();
                toolStripComboBox2.Items.Add("Small");
                toolStripComboBox2.Items.Add("Medium");
                toolStripComboBox2.Items.Add("Large");
                toolStripComboBox2.Text = "Medium";
                toolStripTextBox3.Text = "Icon Size:";
                toolStripComboBox3.Items.Clear();
                toolStripComboBox3.Items.Add("Small");
                toolStripComboBox3.Items.Add("Medium");
                toolStripComboBox3.Text = "Medium";

                this.AdjustListView.Clear();
                this.AdjustListView.Columns.Add("ID", 50, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Station", 60, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Meas Angle", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Adjust", 60, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Adj-Angle", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Azimuth", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Length(m)", 80, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("ΔX(m)", 110, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("ΔY(m)", 110, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Adj-ΔX(m)", 100, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Adj-ΔY(m)", 100, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("X(m)", 100, HorizontalAlignment.Center);
                this.AdjustListView.Columns.Add("Y(m)", 100, HorizontalAlignment.Center);
            }
        }

        private void ApplyResource()                //语言切换
        {
            System.ComponentModel.ComponentResourceManager res = new ComponentResourceManager(typeof(AuroraMain));
            foreach (Control ctl in Controls)       //控件
            {
                res.ApplyResources(ctl, ctl.Name);
            }

            res.ApplyResources(tabControl1.TabPages[0], tabControl1.TabPages[0].Name);
            res.ApplyResources(tabControl1.TabPages[1], tabControl1.TabPages[1].Name);
            res.ApplyResources(tabControl1.TabPages[2], tabControl1.TabPages[2].Name);
            res.ApplyResources(CheckBox_HighPrecision, CheckBox_HighPrecision.Name);
            res.ApplyResources(toolStripTextBox1, toolStripTextBox1.Name);      //图标大小
            res.ApplyResources(toolStripStatusLabel1, toolStripStatusLabel1.Name);      //状态栏
            res.ApplyResources(toolStripStatusLabel3, toolStripStatusLabel3.Name);

            res.ApplyResources(label1, label1.Name); res.ApplyResources(label2, label2.Name);
            res.ApplyResources(label3, label3.Name); res.ApplyResources(label18, label18.Name);
            res.ApplyResources(groupBox1, groupBox1.Name);

            res.ApplyResources(label4, label4.Name); res.ApplyResources(label5, label5.Name);       //辅助计算
            res.ApplyResources(label6, label6.Name); res.ApplyResources(label7, label7.Name);
            res.ApplyResources(label8, label8.Name); res.ApplyResources(label9, label9.Name);
            res.ApplyResources(label10, label10.Name); res.ApplyResources(label11, label11.Name);

            foreach (ToolStripMenuItem item in this.menuStrip1.Items)       //菜单
            {
                res.ApplyResources(item, item.Name);
                foreach (ToolStripItem subItem in item.DropDownItems)
                {
                    res.ApplyResources(subItem, subItem.Name);
                }
            }

            foreach (ToolStripItem item in this.contextMenuStrip1.Items)       //平差右键菜单
            {
                res.ApplyResources(item, item.Name);
            }
            foreach (ToolStripItem item in this.contextMenuStrip2.Items)       //绘图右键菜单
            {
                res.ApplyResources(item, item.Name);
            }
            foreach (ToolStripItem item in this.contextMenuStrip3.Items)       //报表右键菜单
            {
                res.ApplyResources(item, item.Name);
            }

            res.ApplyResources(this, "$this");      //标题
        }

        private void toolStripMenuItem_Algorithm_Click(object sender, EventArgs e)              //算法库
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                if (File.Exists(Application.StartupPath + "\\Algorithm_Library.exe"))
                {
                    System.Diagnostics.Process.Start(Application.StartupPath + "\\Algorithm_Library.exe");
                }
                else
                    MessageBox.Show("Aurora算法库不存在，请尝试重新安装Aurora。", "Aurora智能提示");
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                if (File.Exists(Application.StartupPath + "\\Algorithm_Library.exe"))
                {
                    System.Diagnostics.Process.Start(Application.StartupPath + "\\Algorithm_Library.exe");
                }
                else
                    MessageBox.Show("Aurora算法庫不存在，請嘗試重新安裝Aurora。", "Aurora智慧提示");
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                if (File.Exists(Application.StartupPath + "\\Algorithm_Library.exe"))
                {
                    System.Diagnostics.Process.Start(Application.StartupPath + "\\Algorithm_Library.exe");
                }
                else
                    MessageBox.Show("Aurora Algorithm Library file is missing, please reinstall Aurora.", "Aurora Intelligent Tips");
            }
        }

        private void toolStripMenuItem_FeedBack_Click(object sender, EventArgs e)               //问卷调查
        {
            FeedBack frmfd = new FeedBack();
            frmfd.Show();
        }

        private void ToolStripMenuItem_Viewhelp_Click(object sender, EventArgs e)               //查看帮助
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                if (File.Exists(Application.StartupPath + "\\HelpDoc\\AuroraHelp_Chs.pdf"))
                {
                    System.Diagnostics.Process.Start(Application.StartupPath + "\\HelpDoc\\AuroraHelp_Chs.pdf");
                }
                else
                MessageBox.Show("Aurora智能帮助档丢失，请尝试重新安装Aurora。", "Aurora智能提示");
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                if (File.Exists(Application.StartupPath + "\\HelpDoc\\AuroraHelp_Cht.pdf"))
                {
                    System.Diagnostics.Process.Start(Application.StartupPath + "\\HelpDoc\\AuroraHelp_Cht.pdf");
                }
                else
                MessageBox.Show("Aurora智能幫助檔丟失，請嘗試重新安裝Aurora。", "Aurora智慧提示");
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                if (File.Exists(Application.StartupPath + "\\HelpDoc\\AuroraHelp_En.pdf"))
                {
                    System.Diagnostics.Process.Start(Application.StartupPath + "\\HelpDoc\\AuroraHelp_En.pdf");
                }
                else
                MessageBox.Show("Aurora help file is missing, please reinstall Aurora.", "Aurora Intelligent Tips");
            }
        }

        private void toolStripMenuItem_Thanks_Click(object sender, EventArgs e)             //特别鸣谢
        {
            Acknowledgement frmThanks = new Acknowledgement();
            frmThanks.ShowDialog();
        }

        private void toolStripMenuItem_NewFunction_Click(object sender, EventArgs e)                //新功能介绍
        {
            NewFunction frmNewFunction = new NewFunction();
            frmNewFunction.ShowDialog();
        }

        public int CloseFormFlag = 1;           //0不退出主程序，1则退出。
        private void toolStripMenuItem_Register_Click(object sender, EventArgs e)               //注册
        {
            CloseFormFlag = 0;
            RegisterAurora FrmRegister = new RegisterAurora();
            FrmRegister.StartPosition = FormStartPosition.CenterParent;      //创建模态对话框
            FrmRegister.ShowDialog(this);
        }

        private void ToolStripMenuItem_About_Click(object sender, EventArgs e)              //关于
        {
            AboutAurora FrmAbout = new AboutAurora();
            FrmAbout.StartPosition = FormStartPosition.CenterParent;      //创建模态对话框
            FrmAbout.ShowDialog();
        }

        private void toolStripMenuItem_Donate_Click(object sender, EventArgs e)             //资助开发者
        {
            Donate FrmDonate = new Donate();
            FrmDonate.StartPosition = FormStartPosition.CenterParent;      //创建模态对话框
            FrmDonate.ShowDialog();
        }

        private void toolStripMenuItem_Mail_Click(object sender, EventArgs e)               //邮件反馈
        {
            //System.Diagnostics.Process.Start("Mail\\Mail.exe");

            log.Info(DateTime.Now.ToString() + "Email Author" + sender.ToString() + e.ToString());//写入一条新log
            Mail FrmMail = new Mail();
            FrmMail.StartPosition = FormStartPosition.CenterScreen;
            FrmMail.Show();      //this 必须有，传递子窗体参数       //创建模态对话框
        }

        #endregion

        #region 右键菜单
        private void toolStripMenuItem_RAdd_Click(object sender, EventArgs e)               //右键菜单-添加行
        {
            toolStripButton_Add_Click(sender, e);
        }

        private void toolStripMenuItem_RDelete_Click(object sender, EventArgs e)               //右键菜单-删除行
        {
            toolStripButton_Delete_Click(sender, e);
        }

        private void toolStripMenuItem_RClear_Click(object sender, EventArgs e)               //右键菜单-清空列表
        {
            toolStripButton_Clear_Click(sender, e);
        }

        private void toolStripMenuItem_RCalc_Click(object sender, EventArgs e)               //右键菜单-计算
        {
            toolStripButton_Calc_Click(sender, e);
        }

        private void toolStripMenuItem_RMapping_Click(object sender, EventArgs e)               //右键菜单-绘图
        {
            toolStripButton_Mapping_Click(sender, e);
            //tabControl1.SelectedTab = tabPage2;

            //if (tabControl1.SelectedTab == tabPage1)
            //{
            //    toolStrip1.Visible = true;
            //    toolStrip2.Visible = false;
            //    toolStrip3.Visible = false;
            //}

            //if (tabControl1.SelectedTab == tabPage2)
            //{
            //    toolStrip1.Visible = false;
            //    toolStrip2.Visible = true;
            //    toolStrip3.Visible = false;


            //    iMap(sender, e);               //调用绘图函数
            //    //toolStripButton_Mapping_Click(sender, e);               //调用绘图

            //    if (nCalcFlag == 0)             //如果未计算平差，则返回平差标签
            //    {
            //        tabControl1.SelectedTab = tabPage1;
            //    }
            //}

            //if (tabControl1.SelectedTab == tabPage3)
            //{
            //    toolStrip1.Visible = false;
            //    toolStrip2.Visible = false;
            //    toolStrip3.Visible = true;


            //    iReport(sender, e);               //调用报表函数
            //    //toolStripButton_Report_Click(sender, e);               //调用报表

            //    if (nCalcFlag == 0)             //如果未计算平差，则返回平差标签
            //    {
            //        tabControl1.SelectedTab = tabPage1;
            //    }
            //}
        }

        private void toolStripMenuItem_RReporting_Click(object sender, EventArgs e)               //右键菜单-报表
        {
            toolStripButton_Report_Click(sender, e);
        }

        #endregion

        #region 支持标准文件直接拖入listview。AllowDrop = true
        private void AdjustListView_DragDrop(object sender, DragEventArgs e)
        {
            AdjustListView.Items.Clear();
            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
            textBox5.Text = ""; textBox6.Text = ""; textBox7.Text = ""; textBox8.Text = "";

            string fName = "";
            fName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            string strLine = "";
            FileStream fs = new FileStream(fName, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding("gb2312"));
            try
            {
                strLine = sr.ReadLine();//.Split(new string[] { "\r\n" }, StringSplitOptions.None)[0];后面的可以指定行读取
                string[] strElement = strLine.Split(',');

                string strAdjType = strElement[0];
                string strKnown = strElement[1];
                string strUnknown = strElement[2];

                if (strAdjType == "ClosedLeveling")             //判断平差类型.
                {
                    this.AdjustType.Text = AdjustType.Items[0].ToString();
                }
                if (strAdjType == "AnnexedLeveling")
                {
                    this.AdjustType.Text = AdjustType.Items[1].ToString();
                }
                if (strAdjType == "SpurLeveling")
                {
                    this.AdjustType.Text = AdjustType.Items[2].ToString();
                }
                if (strAdjType == "ClosedTraverse")
                {
                    this.AdjustType.Text = AdjustType.Items[3].ToString();
                }
                if (strAdjType == "ClosedTraverseWithOuterPoint")
                {
                    this.AdjustType.Text = AdjustType.Items[4].ToString();
                }
                if (strAdjType == "OneAngleConnTraverse")
                {
                    this.AdjustType.Text = AdjustType.Items[5].ToString();
                }
                if (strAdjType == "TwoAngleConnTraverse")
                {
                    this.AdjustType.Text = AdjustType.Items[6].ToString();
                }
                if (strAdjType == "OpenTraverse")
                {
                    this.AdjustType.Text = AdjustType.Items[7].ToString();
                }
                if (strAdjType == "NoAngleConnTraverse")
                {
                    this.AdjustType.Text = AdjustType.Items[8].ToString();
                }

                #region 先将已知点数和未知点数相加，得到总行数，在列表中增加nCount行
                int nKnown = Convert.ToInt16(strKnown);
                int nUnknown = Convert.ToInt16(strUnknown);
                int nCount = 0;
                if (strAdjType == "ClosedLeveling")             //闭合的类型，需要加上1行。
                {
                    nCount = nKnown + nUnknown + 1;
                }
                else if (strAdjType == "ClosedTraverse")
                {
                    nCount = nKnown + nUnknown + 1;
                }
                else if (strAdjType == "ClosedTraverseWithOuterPoint")
                {
                    nCount = nKnown + nUnknown + 1;
                }
                else nCount = nKnown + nUnknown;

                ListViewItem lstItem = new ListViewItem();
                lstItem.UseItemStyleForSubItems = false;//设置允许子项颜色不一致,否则无背景颜色

                for (int i = 0; i < nCount; i++)
                {
                    toolStripButton_Add_Click(sender, e);               //循环调用“添加行”命令
                    Delay(200);             //延时产生动画效果
                }
                #endregion

                //现在读入测站名
                strLine = sr.ReadLine();
                string[] strStnName = strLine.Split(',');

                int k = strStnName.GetLength(0);                //获取字符串数组长度
                for (int j = 0; j < strStnName.GetLength(0); j++)
                {
                    AdjustListView.Items[j].SubItems[1].Text = strStnName[j];                //把测站名读入ListView
                }


                #region 读取已知点坐标/高程
                switch (AdjustType.SelectedIndex)       //选择平差类型
                {
                    case 0: //闭合水准
                        {
                            strLine = sr.ReadLine();
                            //string[] strHeight = strLine;
                            AdjustListView.Items[0].SubItems[7].Text = strLine;
                            AdjustListView.Items[nCount - 1].SubItems[7].Text = strLine;
                        }
                        break;
                    case 1: //附和水准
                        {
                            strLine = sr.ReadLine();
                            string[] strHeight = strLine.Split(',');
                            AdjustListView.Items[0].SubItems[7].Text = strHeight[0];
                            AdjustListView.Items[nCount - 1].SubItems[7].Text = strHeight[1];
                        }
                        break;

                    case 2: //支水准
                        {
                            strLine = sr.ReadLine();
                            //string[] strHeight = strLine;
                            AdjustListView.Items[0].SubItems[5].Text = strLine;
                        }
                        break;

                    case 3: //闭合导线————两个已知点
                        {
                            strLine = sr.ReadLine();
                            string[] strCoor1 = strLine.Split(',');
                            AdjustListView.Items[0].SubItems[11].Text = strCoor1[0];
                            AdjustListView.Items[0].SubItems[12].Text = strCoor1[1];
                            AdjustListView.Items[nCount - 1].SubItems[11].Text = strCoor1[0];
                            AdjustListView.Items[nCount - 1].SubItems[12].Text = strCoor1[1];

                            strLine = sr.ReadLine();
                            string[] strCoor2 = strLine.Split(',');
                            AdjustListView.Items[1].SubItems[11].Text = strCoor2[0];
                            AdjustListView.Items[1].SubItems[12].Text = strCoor2[1];
                        }
                        break;

                    case 4: //闭合导线(含外支点)————两个已知点
                        {
                            strLine = sr.ReadLine();
                            string[] strCoor1 = strLine.Split(',');
                            AdjustListView.Items[0].SubItems[11].Text = strCoor1[0];
                            AdjustListView.Items[0].SubItems[12].Text = strCoor1[1];

                            strLine = sr.ReadLine();
                            string[] strCoor2 = strLine.Split(',');
                            AdjustListView.Items[1].SubItems[11].Text = strCoor2[0];
                            AdjustListView.Items[1].SubItems[12].Text = strCoor2[1];
                            AdjustListView.Items[nCount - 1].SubItems[11].Text = strCoor2[0];
                            AdjustListView.Items[nCount - 1].SubItems[12].Text = strCoor2[1];
                        }
                        break;

                    case 5: //一个连接角的附和导线————三个已知点
                        {
                            strLine = sr.ReadLine();
                            string[] strCoor1 = strLine.Split(',');
                            AdjustListView.Items[0].SubItems[9].Text = strCoor1[0];
                            AdjustListView.Items[0].SubItems[10].Text = strCoor1[1];

                            strLine = sr.ReadLine();
                            string[] strCoor2 = strLine.Split(',');
                            AdjustListView.Items[1].SubItems[9].Text = strCoor2[0];
                            AdjustListView.Items[1].SubItems[10].Text = strCoor2[1];

                            strLine = sr.ReadLine();
                            string[] strCoor3 = strLine.Split(',');
                            AdjustListView.Items[nCount - 1].SubItems[9].Text = strCoor3[0];
                            AdjustListView.Items[nCount - 1].SubItems[10].Text = strCoor3[1];
                        }
                        break;

                    case 6: //两个连接角的附和导线————四个已知点
                        {
                            strLine = sr.ReadLine();
                            string[] strCoor1 = strLine.Split(',');
                            AdjustListView.Items[0].SubItems[11].Text = strCoor1[0];
                            AdjustListView.Items[0].SubItems[12].Text = strCoor1[1];

                            strLine = sr.ReadLine();
                            string[] strCoor2 = strLine.Split(',');
                            AdjustListView.Items[1].SubItems[11].Text = strCoor2[0];
                            AdjustListView.Items[1].SubItems[12].Text = strCoor2[1];

                            strLine = sr.ReadLine();
                            string[] strCoor3 = strLine.Split(',');
                            AdjustListView.Items[nCount - 2].SubItems[11].Text = strCoor3[0];
                            AdjustListView.Items[nCount - 2].SubItems[12].Text = strCoor3[1];

                            strLine = sr.ReadLine();
                            string[] strCoor4 = strLine.Split(',');
                            AdjustListView.Items[nCount - 1].SubItems[11].Text = strCoor4[0];
                            AdjustListView.Items[nCount - 1].SubItems[12].Text = strCoor4[1];
                        }
                        break;

                    case 7: //支导线————两个已知点
                        {
                            strLine = sr.ReadLine();
                            string[] strCoor1 = strLine.Split(',');
                            AdjustListView.Items[0].SubItems[7].Text = strCoor1[0];
                            AdjustListView.Items[0].SubItems[8].Text = strCoor1[1];

                            strLine = sr.ReadLine();
                            string[] strCoor2 = strLine.Split(',');
                            AdjustListView.Items[1].SubItems[7].Text = strCoor2[0];
                            AdjustListView.Items[1].SubItems[8].Text = strCoor2[1];
                        }
                        break;

                    case 8: //无连接角导线————两个已知点
                        {
                            strLine = sr.ReadLine();
                            string[] strCoor1 = strLine.Split(',');
                            AdjustListView.Items[0].SubItems[9].Text = strCoor1[0];
                            AdjustListView.Items[0].SubItems[10].Text = strCoor1[1];

                            strLine = sr.ReadLine();
                            string[] strCoor2 = strLine.Split(',');
                            AdjustListView.Items[nCount - 1].SubItems[9].Text = strCoor2[0];
                            AdjustListView.Items[nCount - 1].SubItems[10].Text = strCoor2[1];
                        }
                        break;
                }
                #endregion

                #region 现在读取水准距离值
                switch (AdjustType.SelectedIndex)       //选择平差类型
                {
                    case 0: //闭合水准
                        {
                            strLine = sr.ReadLine();
                            string[] strDist = strLine.Split(',');
                            //for (int i = 0; i < nCount - 1; i++)
                            for (int i = 0; i < strDist.GetLength(0); i++)
                            {
                                AdjustListView.Items[i].SubItems[2].Text = strDist[i];
                            }
                        }
                        break;
                    case 1: //附和水准
                        {
                            strLine = sr.ReadLine();
                            string[] strDist = strLine.Split(',');
                            //for (int i = 0; i < nCount - 1; i++)
                            for (int i = 0; i < strDist.GetLength(0); i++)
                            {
                                AdjustListView.Items[i].SubItems[2].Text = strDist[i];
                            }
                        }
                        break;

                    case 2: //支水准
                        {
                            strLine = sr.ReadLine();
                            string[] strDist = strLine.Split(',');
                            //for (int i = 0; i < nCount - 1; i++)
                            for (int i = 0; i < strDist.GetLength(0); i++)
                            {
                                AdjustListView.Items[i].SubItems[2].Text = strDist[i];
                            }
                        }
                        break;
                }
                #endregion

                #region 现在读取水准测站数
                switch (AdjustType.SelectedIndex)       //选择平差类型
                {
                    case 0: //闭合水准
                        {
                            strLine = sr.ReadLine();
                            string[] strStns = strLine.Split(',');
                            //for (int i = 0; i < nCount - 1; i++)
                            for (int i = 0; i < strStns.GetLength(0); i++)
                            {
                                AdjustListView.Items[i].SubItems[3].Text = strStns[i];
                            }
                        }
                        break;
                    case 1: //附和水准
                        {
                            strLine = sr.ReadLine();
                            string[] strStns = strLine.Split(',');
                            //for (int i = 0; i < nCount - 1; i++)
                            for (int i = 0; i < strStns.GetLength(0); i++)
                            {
                                AdjustListView.Items[i].SubItems[3].Text = strStns[i];
                            }
                        }
                        break;

                    case 2: //支水准
                        {
                            strLine = sr.ReadLine();
                            string[] strStns = strLine.Split(',');
                            //for (int i = 0; i < nCount - 1; i++)
                            for (int i = 0; i < strStns.GetLength(0); i++)
                            {
                                AdjustListView.Items[i].SubItems[3].Text = strStns[i];
                            }
                        }
                        break;
                }
                #endregion

                #region 现在读取水准实测高差
                switch (AdjustType.SelectedIndex)       //选择平差类型
                {
                    case 0: //闭合水准
                        {
                            strLine = sr.ReadLine();
                            string[] strObsLevelDiff = strLine.Split(',');
                            //for (int i = 0; i < nCount - 1; i++)
                            for (int i = 0; i < strObsLevelDiff.GetLength(0); i++)
                            {
                                AdjustListView.Items[i].SubItems[4].Text = strObsLevelDiff[i];
                            }
                        }
                        break;
                    case 1: //附和水准
                        {
                            strLine = sr.ReadLine();
                            string[] strObsLevelDiff = strLine.Split(',');
                            //for (int i = 0; i < nCount - 1; i++)
                            for (int i = 0; i < strObsLevelDiff.GetLength(0); i++)
                            {
                                AdjustListView.Items[i].SubItems[4].Text = strObsLevelDiff[i];
                            }
                        }
                        break;

                    case 2: //支水准
                        {
                            strLine = sr.ReadLine();
                            string[] strObsLevelDiff = strLine.Split(',');
                            //for (int i = 0; i < nCount - 1; i++)
                            for (int i = 0; i < strObsLevelDiff.GetLength(0); i++)
                            {
                                AdjustListView.Items[i].SubItems[4].Text = strObsLevelDiff[i];
                            }
                        }
                        break;
                }
                #endregion

                #region 现在读取观测角度
                switch (AdjustType.SelectedIndex)       //选择平差类型
                {
                    case 3: //闭合导线
                        {
                            strLine = sr.ReadLine();
                            string[] strObsAngle = strLine.Split(',');
                            //for (int i = 1; i < nCount; i++ )
                            for (int i = 1; i < strObsAngle.GetLength(0) + 1; i++)
                            {
                                AdjustListView.Items[i].SubItems[2].Text = strObsAngle[i - 1];
                            }
                        }
                        break;

                    case 4: //闭合导线(含外支点)
                        {
                            strLine = sr.ReadLine();
                            string[] strObsAngle = strLine.Split(',');
                            //for (int i = 1; i < nCount; i++)
                            for (int i = 1; i < strObsAngle.GetLength(0) + 1; i++)
                            {
                                AdjustListView.Items[i].SubItems[2].Text = strObsAngle[i - 1];
                            }
                        }
                        break;

                    case 5: //一个连接角的附和导线
                        {
                            strLine = sr.ReadLine();
                            string[] strObsAngle = strLine.Split(',');
                            //for (int i = 1; i < nCount - 1; i++)
                            for (int i = 1; i < strObsAngle.GetLength(0) + 1; i++)
                            {
                                AdjustListView.Items[i].SubItems[2].Text = strObsAngle[i - 1];
                            }
                        }
                        break;

                    case 6: //两个连接角的附和导线
                        {
                            strLine = sr.ReadLine();
                            string[] strObsAngle = strLine.Split(',');
                            //for (int i = 1; i < nCount - 1; i++)
                            for (int i = 1; i < strObsAngle.GetLength(0) + 1; i++)
                            {
                                AdjustListView.Items[i].SubItems[2].Text = strObsAngle[i - 1];
                            }
                        }
                        break;

                    case 7: //支导线
                        {
                            strLine = sr.ReadLine();
                            string[] strObsAngle = strLine.Split(',');
                            //for (int i = 1; i < nCount - 1; i++)
                            for (int i = 1; i < strObsAngle.GetLength(0) + 1; i++)
                            {
                                AdjustListView.Items[i].SubItems[2].Text = strObsAngle[i - 1];
                            }
                        }
                        break;

                    case 8: //无连接角导线
                        {
                            strLine = sr.ReadLine();
                            string[] strObsAngle = strLine.Split(',');
                            //for (int i = 1; i < nCount - 1; i++)
                            for (int i = 1; i < strObsAngle.GetLength(0) + 1; i++)
                            {
                                AdjustListView.Items[i].SubItems[2].Text = strObsAngle[i - 1];
                            }
                        }
                        break;
                }
                #endregion

                #region 现在读取边长
                switch (AdjustType.SelectedIndex)       //选择平差类型
                {
                    case 3: //闭合导线
                        {
                            strLine = sr.ReadLine();
                            string[] strLength = strLine.Split(',');
                            //for (int i = 1; i < nCount - 1; i++)
                            for (int i = 1; i < strLength.GetLength(0) + 1; i++)
                            {
                                AdjustListView.Items[i].SubItems[6].Text = strLength[i - 1];
                            }
                        }
                        break;

                    case 4: //闭合导线(含外支点)
                        {
                            strLine = sr.ReadLine();
                            string[] strLength = strLine.Split(',');
                            //for (int i = 1; i < nCount - 1; i++)
                            for (int i = 1; i < strLength.GetLength(0) + 1; i++)
                            {
                                AdjustListView.Items[i].SubItems[6].Text = strLength[i - 1];
                            }
                        }
                        break;

                    case 5: //一个连接角的附和导线
                        {
                            strLine = sr.ReadLine();
                            string[] strLength = strLine.Split(',');
                            //for (int i = 1; i < nCount - 1; i++)
                            for (int i = 1; i < strLength.GetLength(0) + 1; i++)
                            {
                                AdjustListView.Items[i].SubItems[4].Text = strLength[i - 1];
                            }
                        }
                        break;

                    case 6: //两个连接角的附和导线
                        {
                            strLine = sr.ReadLine();
                            string[] strLength = strLine.Split(',');
                            //for (int i = 1; i < nCount - 2; i++)
                            for (int i = 1; i < strLength.GetLength(0) + 1; i++)
                            {
                                AdjustListView.Items[i].SubItems[6].Text = strLength[i - 1];
                            }
                        }
                        break;

                    case 7: //支导线
                        {
                            strLine = sr.ReadLine();
                            string[] strLength = strLine.Split(',');
                            //for (int i = 1; i < nCount - 1; i++)
                            for (int i = 1; i < strLength.GetLength(0) + 1; i++)
                            {
                                AdjustListView.Items[i].SubItems[4].Text = strLength[i - 1];
                            }
                        }
                        break;

                    case 8: //无连接角导线
                        {
                            strLine = sr.ReadLine();
                            string[] strLength = strLine.Split(',');
                            //for (int i = 1; i < nCount - 1; i++)
                            for (int i = 0; i < strLength.GetLength(0); i++)
                            {
                                AdjustListView.Items[i].SubItems[3].Text = strLength[i];
                            }
                        }
                        break;
                }
                #endregion


                AdjustListView.EnsureVisible(nCount - 1);    //确保焦点显示到最后一行
                nCalcFlag = 0;              //将计算完成标志设置为0.

                sr.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("导入的数据格式貌似少了一些必要数据或者格式错误，请仔细检查。", "Aurora智能提示");
                MessageBox.Show(ex.ToString(), "Aurora智能提示");
            }
            fs.Close();
        }

        private void AdjustListView_DragEnter(object sender, DragEventArgs e)               //支持拖放事件的代码
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else e.Effect = DragDropEffects.None;
        }

        #endregion

        #region 超级控制台 keyPreview 为 true
        string strInput = "";
        DateTime _dt = DateTime.Now;
        private void AuroraMain_KeyPress(object sender, KeyPressEventArgs e)                //超级控制台
        {
            DateTime tempDt = DateTime.Now;         //保存按键按下时刻的时间点
            TimeSpan ts = tempDt.Subtract(_dt);     //获取时间间隔
            if (ts.Milliseconds > 500)               //如果时间间隔大于500毫秒，清空
                strInput = e.KeyChar.ToString();
            else
                strInput += e.KeyChar;
            _dt = tempDt;

            if (strInput == "376787823")                //超级控制台密码
            {
                if (MessageBox.Show("程序即将进入超级控制台。", "Aurora智能提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    MySuperConsole fSuperConsole = new MySuperConsole();
                    fSuperConsole.StartPosition = FormStartPosition.CenterScreen;
                    fSuperConsole.ShowDialog();
                }
                else return;
                
            }
        }

        #endregion
        
        #region 状态栏
        private void timerTotalRun_Tick(object sender, EventArgs e)             //实时显示运行时间              //试用版有时间限制
        {
            toolStripStatusLabel4.Text = stw.Elapsed.Hours.ToString() + "h " + stw.Elapsed.Minutes.ToString() + "m " + stw.Elapsed.Seconds.ToString() + "s";
            toolStripStatusLabel5.Text = this.Width + " x " + this.Height + "   ";
            toolStripStatusLabel6.Text = Control.MousePosition.X.ToString() + "," + Control.MousePosition.Y.ToString() + "   ";

            //试用版有时间限制，一共十天，每次30min。
            RegistryKey MyReg0, RegGUIDFlag, RegFlag;
            MyReg0 = Registry.CurrentUser;

            try
            {
                RegGUIDFlag = MyReg0.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");//在注册表项中创建子项
                RegFlag = MyReg0.CreateSubKey("Software\\Aurora");

                if (RegGUIDFlag.GetValue("nRegGUIDFlag").ToString() == "0" && stw.Elapsed.Minutes == 30)        //试用版未注册 + 30分限制
                {
                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                    {
                        AutoClosingMessageBox.Show("本次试用时间已到30分钟，谢谢您的使用。" + "\r\n" + "\r\n"
                                      + "程序即将在 5s 后自动退出。。。。。。", "Aurora智能提示", 5000);
                    }
                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                    {
                        AutoClosingMessageBox.Show("本次試用時間已到30分鐘，謝謝您的使用。" + "\r\n" + "\r\n"
                                      + "程式即將在 5s 後自動退出。。。。。。", "Aurora智慧提示", 5000);
                    }
                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                    {
                        AutoClosingMessageBox.Show("Trial time is 30 minutes, Thanks for using." + "\r\n" + "\r\n"
                                      + "This program will exit in 5s...", "Aurora Intelligent Tips", 5000);
                    }

                    RegistryKey MyReg1, RegReminder;
                    MyReg1 = Registry.CurrentUser;
                    RegReminder = MyReg1.CreateSubKey("Software\\Aurora\\Reminder");
                    try
                    {
                        RegReminder.SetValue("ExitReminder", "NO");             //此段控制注册信息被破坏后，不弹出确认关闭的对话框，防止异常。
                    }
                    catch { }

                    Application.Exit();
                }
            }
            catch { }

        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)                //程序状态
        {
            OleDbConnection dbConn = new OleDbConnection("Provider = Microsoft.ACE.OLEDB.12.0 ; Data Source = Adjustment.accdb; Persist Security Info=False; Jet OLEDB:Database Password=lmzl123456789");
            if (dbConn.State != ConnectionState.Open)
            {
                System.Media.SoundPlayer sndPlayer = new System.Media.SoundPlayer(Application.StartupPath + "\\System.wav");    //wav格式的铃声 
                sndPlayer.Play();
                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                {
                    MessageBox.Show("Aurora 程序运行正常，Aurora 绘图准备就绪。" + "\r\n" +
                                    "Aurora 报表准备就绪，Aurora 数据库准备就绪。", "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                {
                    MessageBox.Show("Aurora 程式運行正常，Aurora 繪圖準備就緒。" + "\r\n" +
                                    "Aurora 報表準備就緒，Aurora 資料庫準備就緒。", "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                {
                    MessageBox.Show("Aurora App runs well，Aurora Mapping is ready." + "\r\n" +
                                    "Aurora Reporting is ready, Aurora Database is ready.", "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                {
                    MessageBox.Show("Aurora 程序运行正常，数据库正在连接中。" + "\r\n" +
                                    "Aurora 绘图准备就绪，Aurora 报表准备就绪。", "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                {
                    MessageBox.Show("Aurora 程式運行正常，資料庫正在連接中。" + "\r\n" +
                                    "Aurora 繪圖準備就緒，Aurora 報表準備就緒。", "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                {
                    MessageBox.Show("Aurora runs well，Aurora Database is connecting..." + "\r\n" +
                                    "Aurora Mapping is ready, Aurora Reporting is ready.", "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private int i = 0;              //i用来计数，看点击了几次鼠标
        private DateTime lastClickTime = DateTime.MinValue;
        private void toolStripStatusLabel4_Click(object sender, EventArgs e)                //在线时间，显示等级
        {
            DateTime now = DateTime.Now;

            if ((now - lastClickTime).TotalMilliseconds <= 500)             // 两次点击间隔小于500毫秒时，算连续点击
            {
                i++;
                if (i >= 5)
                {
                    i = 0;// 连续点击完毕时，清0

                    // 5次点击触发的事件
                    TimeLevel frmLevel = new TimeLevel();
                    frmLevel.StartPosition = FormStartPosition.CenterParent;

                    frmLevel.pictureBox1.Image = null;
                    frmLevel.pictureBox2.Image = null;
                    frmLevel.pictureBox3.Image = null;

                    int nElapseTime = stw.Elapsed.Hours * 60 + stw.Elapsed.Minutes + nTotalTime;             //将时间转化为分钟计算

                    if (nElapseTime < 10)
                    {
                        frmLevel.pictureBox1.Image = Properties.Resources.Lvl_Star_16;
                    }
                    if (nElapseTime >= 10 && nElapseTime < 30)
                    {
                        frmLevel.pictureBox1.Image = Properties.Resources.Lvl_Star_16;
                        frmLevel.pictureBox2.Image = Properties.Resources.Lvl_Star_16;
                    }
                    if (nElapseTime >= 30 && nElapseTime < 60)
                    {
                        frmLevel.pictureBox1.Image = Properties.Resources.Lvl_Star_16;
                        frmLevel.pictureBox2.Image = Properties.Resources.Lvl_Star_16;
                        frmLevel.pictureBox3.Image = Properties.Resources.Lvl_Star_16;
                    }
                    if (nElapseTime >= 60 && nElapseTime < 100)
                    {
                        frmLevel.pictureBox1.Image = Properties.Resources.Lvl_Moon_16;
                    }
                    if (nElapseTime >= 100 && nElapseTime < 150)
                    {
                        frmLevel.pictureBox1.Image = Properties.Resources.Lvl_Moon_16;
                        frmLevel.pictureBox2.Image = Properties.Resources.Lvl_Star_16;
                    }
                    if (nElapseTime >= 150 && nElapseTime < 200)
                    {
                        frmLevel.pictureBox1.Image = Properties.Resources.Lvl_Moon_16;
                        frmLevel.pictureBox2.Image = Properties.Resources.Lvl_Star_16;
                        frmLevel.pictureBox3.Image = Properties.Resources.Lvl_Star_16;
                    }
                    if (nElapseTime >= 200 && nElapseTime < 300)
                    {
                        frmLevel.pictureBox1.Image = Properties.Resources.Lvl_Moon_16;
                        frmLevel.pictureBox2.Image = Properties.Resources.Lvl_Moon_16;
                    }

                    if (nElapseTime >= 300 && nElapseTime < 400)
                    {
                        frmLevel.pictureBox1.Image = Properties.Resources.Lvl_Moon_16;
                        frmLevel.pictureBox2.Image = Properties.Resources.Lvl_Moon_16;
                        frmLevel.pictureBox3.Image = Properties.Resources.Lvl_Star_16;
                    }
                    if (nElapseTime >= 400 && nElapseTime < 500)
                    {
                        frmLevel.pictureBox1.Image = Properties.Resources.Lvl_Moon_16;
                        frmLevel.pictureBox2.Image = Properties.Resources.Lvl_Moon_16;
                        frmLevel.pictureBox3.Image = Properties.Resources.Lvl_Moon_16;
                    }
                    if (nElapseTime >= 500 && nElapseTime < 600)
                    {
                        frmLevel.pictureBox1.Image = Properties.Resources.Lvl_Sun_16;
                    }
                    if (nElapseTime >= 600 && nElapseTime < 650)
                    {
                        frmLevel.pictureBox1.Image = Properties.Resources.Lvl_Sun_16;
                        frmLevel.pictureBox2.Image = Properties.Resources.Lvl_Star_16;
                    }
                    if (nElapseTime >= 650 && nElapseTime < 700)
                    {
                        frmLevel.pictureBox1.Image = Properties.Resources.Lvl_Sun_16;
                        frmLevel.pictureBox2.Image = Properties.Resources.Lvl_Star_16;
                        frmLevel.pictureBox3.Image = Properties.Resources.Lvl_Star_16;
                    }
                    if (nElapseTime >= 700 && nElapseTime < 850)
                    {
                        frmLevel.pictureBox1.Image = Properties.Resources.Lvl_Sun_16;
                        frmLevel.pictureBox2.Image = Properties.Resources.Lvl_Moon_16;
                    }
                    if (nElapseTime >= 850 && nElapseTime < 900)
                    {
                        frmLevel.pictureBox1.Image = Properties.Resources.Lvl_Sun_16;
                        frmLevel.pictureBox2.Image = Properties.Resources.Lvl_Moon_16;
                        frmLevel.pictureBox3.Image = Properties.Resources.Lvl_Star_16;
                    }
                    if (nElapseTime >= 900 && nElapseTime < 1050)
                    {
                        frmLevel.pictureBox1.Image = Properties.Resources.Lvl_Sun_16;
                        frmLevel.pictureBox2.Image = Properties.Resources.Lvl_Moon_16;
                        frmLevel.pictureBox3.Image = Properties.Resources.Lvl_Moon_16;
                    }
                    if (nElapseTime >= 1050 && nElapseTime < 1200)
                    {
                        frmLevel.pictureBox1.Image = Properties.Resources.Lvl_Sun_16;
                        frmLevel.pictureBox2.Image = Properties.Resources.Lvl_Sun_16;
                    }
                    if (nElapseTime >= 1200 && nElapseTime < 1400)
                    {
                        frmLevel.pictureBox1.Image = Properties.Resources.Lvl_Sun_16;
                        frmLevel.pictureBox2.Image = Properties.Resources.Lvl_Sun_16;
                        frmLevel.pictureBox3.Image = Properties.Resources.Lvl_Star_16;
                    }
                    if (nElapseTime >= 1400 && nElapseTime < 1700)
                    {
                        frmLevel.pictureBox1.Image = Properties.Resources.Lvl_Sun_16;
                        frmLevel.pictureBox2.Image = Properties.Resources.Lvl_Sun_16;
                        frmLevel.pictureBox3.Image = Properties.Resources.Lvl_Moon_16;
                    }
                    if (nElapseTime >= 1700)             //这尼玛太难了
                    {
                        frmLevel.pictureBox1.Image = Properties.Resources.Lvl_Sun_16;
                        frmLevel.pictureBox2.Image = Properties.Resources.Lvl_Sun_16;
                        frmLevel.pictureBox3.Image = Properties.Resources.Lvl_Sun_16;
                        if (nElapseTime >= 10000)
                        {
                            AutoClosingMessageBox.Show("Bingo！Biu~Biubiu~~Biubiubiu~~~", "H'm, Great, man.", 3000);
                        }
                    }

                    int nT = stw.Elapsed.Hours * 60 + stw.Elapsed.Minutes;              //本次流逝的时间
                    int nTT = nTotalTime + nT;              //总运行时间
                    int nTempH = nTT / 60;
                    int nTempM = nTT % 60;

                    frmLevel.label1.Text = "Time ： " + nTempH.ToString() + "h " + nTempM.ToString() + "m ";
                    frmLevel.ShowDialog();
                }
            }
            else
            {
                i = 1;// 不是连续点击时，清0
            }
            lastClickTime = now;
        }

        private void toolStripStatusLabel3_Click(object sender, EventArgs e)                //在线时间，显示等级
        {
            toolStripStatusLabel4_Click(sender, e);
        }

        #endregion

        private void tabControl1_Selected(object sender, TabControlEventArgs e)             //切换标签栏
        {
            //控制工具栏的显隐
            if (tabControl1.SelectedTab == tabPage1)
            {
                toolStrip1.Visible = true;
                toolStrip2.Visible = false;
                toolStrip3.Visible = false;
            }

            if (tabControl1.SelectedTab == tabPage2)
            {
                if (nCalcFlag == 0)             //如果未计算平差，则返回平差标签
                {
                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                    {
                        MessageBox.Show("请先完成平差计算功能。", "Aurora智能提示");
                    }
                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                    {
                        MessageBox.Show("請先完成平差計算功能。", "Aurora智慧提示");
                    }
                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                    {
                        MessageBox.Show("Please finish adjust first.", "Aurora Intelligent Tips");
                    }

                    tabControl1.SelectedTab = tabPage1;

                    return;
                }
                else
                {
                    toolStrip1.Visible = false;
                    toolStrip2.Visible = true;
                    toolStrip3.Visible = false;


                    iMap(sender, e);               //调用绘图函数
                    //toolStripButton_Mapping_Click(sender, e);               //调用绘图
                }
            }

            if (tabControl1.SelectedTab == tabPage3)
            {
                if (nCalcFlag == 0)             //如果未计算平差，则返回平差标签
                {
                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                    {
                        MessageBox.Show("请先完成平差计算功能。", "Aurora智能提示");
                    }
                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                    {
                        MessageBox.Show("請先完成平差計算功能。", "Aurora智慧提示");
                    }
                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                    {
                        MessageBox.Show("Please finish adjust first.", "Aurora Intelligent Tips");
                    }

                    tabControl1.SelectedTab = tabPage1;

                    return;
                }
                else
                {
                    toolStrip1.Visible = false;
                    toolStrip2.Visible = false;
                    toolStrip3.Visible = true;


                    iReport(sender, e);               //调用报表函数
                    //iReport(sender, e);               //调用报表
                }
            }

            #region        在加载绘图标签的时候

            if (tabControl1.SelectedTab == tabPage2)
            {
                this.MouseWheel += new MouseEventHandler(pictureBox1_MouseWheel);               //鼠标的滚动，实现缩放

                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                {
                    toolStripTextBox2.Text = "图标大小：";
                    toolStripComboBox2.Items.Clear();
                    toolStripComboBox2.Items.Add("小图标");
                    toolStripComboBox2.Items.Add("中图标");
                    toolStripComboBox2.Items.Add("大图标");
                    toolStripComboBox2.Text = "中图标";
                    toolStripMenuItem_Chs.Checked = true;

                    System.ComponentModel.ComponentResourceManager res = new ComponentResourceManager(typeof(AuroraMain));
                    foreach (ToolStripItem item in this.contextMenuStrip2.Items)       //右键菜单
                    {
                        res.ApplyResources(item, item.Name);
                    }
                }
                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                {
                    toolStripTextBox2.Text = "圖標大小：";
                    toolStripComboBox2.Items.Clear();
                    toolStripComboBox2.Items.Add("小圖標");
                    toolStripComboBox2.Items.Add("中圖標");
                    toolStripComboBox2.Items.Add("大圖標");
                    toolStripComboBox2.Text = "中圖標";
                    toolStripMenuItem_Cht.Checked = true;

                    System.ComponentModel.ComponentResourceManager res = new ComponentResourceManager(typeof(AuroraMain));
                    foreach (ToolStripItem item in this.contextMenuStrip2.Items)       //右键菜单
                    {
                        res.ApplyResources(item, item.Name);
                    }
                }
                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                {
                    toolStripTextBox2.Text = "Icon Size:";
                    toolStripComboBox2.Items.Clear();
                    toolStripComboBox2.Items.Add("Small");
                    toolStripComboBox2.Items.Add("Medium");
                    toolStripComboBox2.Items.Add("Large");
                    toolStripComboBox2.Text = "Medium";
                    toolStripMenuItem_En.Checked = true;

                    System.ComponentModel.ComponentResourceManager res = new ComponentResourceManager(typeof(AuroraMain));
                    foreach (ToolStripItem item in this.contextMenuStrip2.Items)       //右键菜单
                    {
                        res.ApplyResources(item, item.Name);
                    }
                }

                if (File.Exists(Application.StartupPath + "\\MapData\\AdjType.ini"))
                {
                    StreamReader aFile = new StreamReader(Application.StartupPath + "\\MapData\\AdjType.ini");
                    if (sLine != null)
                    {
                        sLine = aFile.ReadLine();
                        sLine.Trim();
                    }
                    else sLine = "3";
                    aFile.Close();
                }
                else sLine = "3";        //如果没有就默认闭合导线3。

                toolStripButton_Fullextent_Click(sender, e);
            }
            #endregion


            #region         在加载报表标签的时候，设置报表源

            if (tabControl1.SelectedTab == tabPage3)
            {
                //定制导出格式，限制导出用户.RPT文件，同时区分用户类别，仅商业版可以导出Word和RTF。
                RegistryKey MyReg, RegGUIDFlag;
                MyReg = Registry.CurrentUser;
                try
                {
                    RegGUIDFlag = MyReg.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                    if (RegGUIDFlag.GetValue("nRegGUIDFlag").ToString() == "203" || RegGUIDFlag.GetValue("nRegGUIDFlag").ToString() == "920")//判断软件类型。0为未注册版本，101为学习版，203为商业版，920为10元/30天版。
                    {
                        int exportFormatFlags = (int)(CrystalDecisions.Shared.ViewerExportFormats.PdfFormat | CrystalDecisions.Shared.ViewerExportFormats.CsvFormat | CrystalDecisions.Shared.ViewerExportFormats.ExcelFormat | CrystalDecisions.Shared.ViewerExportFormats.ExcelRecordFormat | CrystalDecisions.Shared.ViewerExportFormats.XLSXFormat | CrystalDecisions.Shared.ViewerExportFormats.WordFormat | CrystalDecisions.Shared.ViewerExportFormats.EditableRtfFormat | CrystalDecisions.Shared.ViewerExportFormats.RtfFormat | CrystalDecisions.Shared.ViewerExportFormats.XmlFormat);
                        crystalReportViewer1.AllowedExportFormats = exportFormatFlags;
                    }
                    else
                    {
                        int exportFormatFlags = (int)(CrystalDecisions.Shared.ViewerExportFormats.PdfFormat | CrystalDecisions.Shared.ViewerExportFormats.CsvFormat | CrystalDecisions.Shared.ViewerExportFormats.ExcelFormat | CrystalDecisions.Shared.ViewerExportFormats.ExcelRecordFormat | CrystalDecisions.Shared.ViewerExportFormats.XLSXFormat | CrystalDecisions.Shared.ViewerExportFormats.EditableRtfFormat | CrystalDecisions.Shared.ViewerExportFormats.XmlFormat);
                        crystalReportViewer1.AllowedExportFormats = exportFormatFlags;
                    }
                }
                catch { }
                

                //Viewer暂时不支持导出ExportFormatType.HTML40格式。如果用户需要Html格式，则需要ReportDocument导出。
                //ReportDocument rd = new ReportDocument();
                //rd.Load(Application.StartupPath + "\\ClosedLeveling_Cht.rpt");
                //rd.ExportToDisk(ExportFormatType.HTML40, Application.StartupPath + "\\formulas.html");

                string sLine = "";
                if (File.Exists(Application.StartupPath + "\\MapData\\AdjType.ini"))
                {
                    StreamReader aFile = new StreamReader(Application.StartupPath + "\\MapData\\AdjType.ini");
                    if (sLine != null)
                    {
                        sLine = aFile.ReadLine();
                        sLine.Trim();
                    }
                    else sLine = "3";
                    aFile.Close();
                }
                else sLine = "3";        //如果没有就默认闭合导线3。

                switch (Convert.ToInt32(sLine))
                {
                    case 0:
                        {
                            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                            {
                                crystalReportViewer1.ReportSource = this.ClosedLeveling_Chs1;
                            }
                            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                            {
                                crystalReportViewer1.ReportSource = this.ClosedLeveling_Cht1;
                            }
                            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                            {
                                crystalReportViewer1.ReportSource = this.ClosedLeveling_En1;
                            }
                        }
                        break;
                    case 1:
                        {
                            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                            {
                                crystalReportViewer1.ReportSource = this.AnnexedLeveling_Chs1;
                            }
                            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                            {
                                crystalReportViewer1.ReportSource = this.AnnexedLeveling_Cht1;
                            }
                            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                            {
                                crystalReportViewer1.ReportSource = this.AnnexedLeveling_En1;
                            }
                        }
                        break;
                    case 2:
                        {
                            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                            {
                                crystalReportViewer1.ReportSource = this.SpurLeveling_Chs1;
                            }
                            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                            {
                                crystalReportViewer1.ReportSource = this.SpurLeveling_Cht1;
                            }
                            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                            {
                                crystalReportViewer1.ReportSource = this.SpurLeveling_En1;
                            }
                        }
                        break;
                    case 3:
                        {
                            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                            {
                                crystalReportViewer1.ReportSource = this.ClosedTraverse_Chs1;
                            }
                            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                            {
                                crystalReportViewer1.ReportSource = this.ClosedTraverse_Cht1;
                            }
                            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                            {
                                crystalReportViewer1.ReportSource = this.ClosedTraverse_En1;
                            }
                        }
                        break;
                    case 4:
                        {
                            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                            {
                                crystalReportViewer1.ReportSource = this.ClosedTraverseWithOuterPoint_Chs1;
                            }
                            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                            {
                                crystalReportViewer1.ReportSource = this.ClosedTraverseWithOuterPoint_Cht1;
                            }
                            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                            {
                                crystalReportViewer1.ReportSource = this.ClosedTraverseWithOuterPoint_En1;
                            }
                        }
                        break;
                    case 5:
                        {
                            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                            {
                                crystalReportViewer1.ReportSource = this.OneAngleConnTraverse_Chs1;
                            }
                            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                            {
                                crystalReportViewer1.ReportSource = this.OneAngleConnTraverse_Cht1;
                            }
                            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                            {
                                crystalReportViewer1.ReportSource = this.OneAngleConnTraverse_En1;
                            }
                        }
                        break;
                    case 6:
                        {
                            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                            {
                                crystalReportViewer1.ReportSource = this.TwoAngleConnTraverse_Chs1;
                            }
                            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                            {
                                crystalReportViewer1.ReportSource = this.TwoAngleConnTraverse_Cht1;
                            }
                            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                            {
                                crystalReportViewer1.ReportSource = this.TwoAngleConnTraverse_En1;
                            }
                        }
                        break;
                    case 7:
                        {
                            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                            {
                                crystalReportViewer1.ReportSource = this.OpenTraverse_Chs1;
                            }
                            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                            {
                                crystalReportViewer1.ReportSource = this.OpenTraverse_Cht1;
                            }
                            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                            {
                                crystalReportViewer1.ReportSource = this.OpenTraverse_En1;
                            }
                        }
                        break;
                    case 8:
                        {
                            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                            {
                                crystalReportViewer1.ReportSource = this.NoAngleConnTraverse_Chs1;
                            }
                            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                            {
                                crystalReportViewer1.ReportSource = this.NoAngleConnTraverse_Cht1;
                            }
                            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                            {
                                crystalReportViewer1.ReportSource = this.NoAngleConnTraverse_En1;
                            }
                        }
                        break;
                    case 9:
                        {
                            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                            {
                                crystalReportViewer1.ReportSource = this.LevelingNetwork_Chs1;
                            }
                            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                            {
                                crystalReportViewer1.ReportSource = this.LevelingNetwork_Cht1;
                            }
                            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                            {
                                crystalReportViewer1.ReportSource = this.LevelingNetwork_En1;
                            }
                        }
                        break;
                }

                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                {
                    toolStripTextBox3.Text = "图标大小：";
                    toolStripComboBox3.Items.Clear();
                    toolStripComboBox3.Items.Add("小图标");
                    toolStripComboBox3.Items.Add("中图标");
                    toolStripComboBox3.Text = "中图标";
                    toolStripMenuItem_Chs.Checked = true;

                    System.ComponentModel.ComponentResourceManager res = new ComponentResourceManager(typeof(AuroraMain));
                    foreach (ToolStripItem item in this.contextMenuStrip3.Items)       //右键菜单
                    {
                        res.ApplyResources(item, item.Name);
                    }
                }
                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                {
                    toolStripTextBox3.Text = "圖標大小：";
                    toolStripComboBox3.Items.Clear();
                    toolStripComboBox3.Items.Add("小圖標");
                    toolStripComboBox3.Items.Add("中圖標");
                    toolStripComboBox3.Text = "中圖標";
                    toolStripMenuItem_Cht.Checked = true;

                    System.ComponentModel.ComponentResourceManager res = new ComponentResourceManager(typeof(AuroraMain));
                    foreach (ToolStripItem item in this.contextMenuStrip3.Items)       //右键菜单
                    {
                        res.ApplyResources(item, item.Name);
                    }
                }
                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                {
                    toolStripTextBox3.Text = "Icon Size:";
                    toolStripComboBox3.Items.Clear();
                    toolStripComboBox3.Items.Add("Small");
                    toolStripComboBox3.Items.Add("Medium");
                    toolStripComboBox3.Text = "Medium";
                    toolStripMenuItem_En.Checked = true;

                    System.ComponentModel.ComponentResourceManager res = new ComponentResourceManager(typeof(AuroraMain));
                    foreach (ToolStripItem item in this.contextMenuStrip3.Items)       //右键菜单
                    {
                        res.ApplyResources(item, item.Name);
                    }
                }

                crystalReportViewer1.RefreshReport();               //切换到报表，需要刷新一下数据

            }

            #endregion
            

        }

        #region     绘图标签--Mapping

        public int nLevel = 100;
        public int n = 0;                       //0不显示SaveFileDialog，1显示SaveFileDialog
        public int nPanFlag = 0;                //0为默认鼠标不能平移图像，1可以平移。
        public int nCursor = 0;                 //0为默认鼠标样式，1放大，2缩小，3平移，4全图，5点名

        protected Point WorldCenPoint = new Point(0, 0);                    //绘图区pictureBoxMap的中心像素坐标：
        protected double WorldCenNorth = 0, WorldCenEast = 0;               //世界坐标系像素中心对应的当地坐标值(Xlocal,Ylocal)

        double MapScale = 0.5;                  //全局图形缩放比例尺
        double DeltaMapScale = 0;               //比例尺缩放的大小
        bool ShowPointID = true;                //是否 显示点名的标记

        bool DrawLineFlag = false;              //画线是否闭合
        bool DrawClosedFlag = false;            //画线是否闭合

        public string sLine = "";               //判断MapData/AdjType.ini下的标记


        private Bitmap DrawCurPointOnScr(int PointCount, double SetMapScale)                //绘图函数
        {
            Bitmap bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics graphics = Graphics.FromImage(bitmap);
            Pen CurPen = new Pen(Color.Black, 1);
            Pen CenterPen = new Pen(Color.Blue, 1);

            Pen SpecialPen = new Pen(Color.Red, 1);
            graphics.Clear(Color.White);  //清背景，用不同颜色清除时，可看见width、height的范围。
            try
            {
                int i = 0;
                int j = 0;
                Point[] points = new Point[PointCount];
                Point[] pointsLine = new Point[PointCount];

                double LocalY = 0, LocalX = 0;
                string strName = "";
                for (i = 0; i < PointCount; i++)
                {
                    strName = listView1.Items[i].SubItems[1].Text;
                    LocalY = double.Parse(listView1.Items[i].SubItems[3].Text);
                    LocalX = double.Parse(listView1.Items[i].SubItems[2].Text);

                    Point CurPoint = new Point(0, 0);
                    CurPoint.X = WorldCenPoint.X + (int)((LocalY - WorldCenEast) * 50 / SetMapScale);
                    CurPoint.Y = WorldCenPoint.Y - (int)((LocalX - WorldCenNorth) * 50 / SetMapScale);
                    points[i] = CurPoint;

                    graphics.DrawEllipse(SpecialPen, CurPoint.X - 2, CurPoint.Y - 2, 4, 4);             //绘制小圆圈，标记点

                    if (ShowPointID)
                    {
                        graphics.DrawString(strName, new Font("微软雅黑", 11, FontStyle.Regular), new SolidBrush(Color.Blue), CurPoint.X + 2, CurPoint.Y + 1);
                    }
                    pointsLine[j] = points[i];
                    j++;
                }

                Point[] pointsDrawLine = new Point[j]; //实际用来绘制点之间连线的点集合
                for (i = 0; i < j; i++)
                {
                    pointsDrawLine[i] = pointsLine[i];
                }

                if (j > 0)
                {
                    if (DrawLineFlag == true)
                    {
                        graphics.DrawLines(CenterPen, pointsDrawLine);

                        //首尾闭合的标志
                        if (DrawClosedFlag == true)
                        {
                            if (sLine == "4")               //如果是闭合导线(含外支点)，则闭合到第二个点
                            {
                                graphics.DrawLine(CenterPen, pointsDrawLine[j - 1], pointsDrawLine[1]);
                            }
                            else graphics.DrawLine(CenterPen, pointsDrawLine[j - 1], pointsDrawLine[0]);
                        }
                    }
                }

                CurPen.Dispose();
                CenterPen.Dispose();
                return (bitmap);
            }
            catch
            {
                return (bitmap);
            }
        }

        private void toolStripComboBox2_SelectedIndexChanged(object sender, EventArgs e)                //改变图标大小
        {
            if (toolStripComboBox2.SelectedIndex == 0)
            {
                this.toolStrip2.ImageScalingSize = new System.Drawing.Size(16, 16);

                this.toolStripButton_Drawline.Image = Properties.Resources.DrawLine_16;
                this.toolStripButton_Export.Image = Properties.Resources.ImageExport_16;
                this.toolStripButton_Copy.Image = Properties.Resources.ImageCopy_16;
                this.toolStripButton_Zoomin.Image = Properties.Resources.ZoomIn_16;
                this.toolStripButton_Zoomout.Image = Properties.Resources.ZoomOut_16;
                this.toolStripButton_Pan.Image = Properties.Resources.Pan_16;
                this.toolStripButton_Fullextent.Image = Properties.Resources.FullExtent_16;
                this.toolStripButton_Viewtext.Image = Properties.Resources.ViewText_16;
                this.toolStripButton_Locker1.Image = Properties.Resources.Locker_16;
                this.toolStripButton_Setup1.Image = Properties.Resources.Setup_16;
                //this.toolStripDropDownButton_language.Image = Properties.Resources.Language_16;
                this.toolStripButton_About1.Image = Properties.Resources.About_16;
            }

            if (toolStripComboBox2.SelectedIndex == 1)
            {
                this.toolStrip2.ImageScalingSize = new System.Drawing.Size(24, 24);

                this.toolStripButton_Drawline.Image = Properties.Resources.DrawLine_24;
                this.toolStripButton_Export.Image = Properties.Resources.ImageExport_24;
                this.toolStripButton_Copy.Image = Properties.Resources.ImageCopy_24;
                this.toolStripButton_Zoomin.Image = Properties.Resources.ZoomIn_24;
                this.toolStripButton_Zoomout.Image = Properties.Resources.ZoomOut_24;
                this.toolStripButton_Pan.Image = Properties.Resources.Pan_24;
                this.toolStripButton_Fullextent.Image = Properties.Resources.FullExtent_24;
                this.toolStripButton_Viewtext.Image = Properties.Resources.ViewText_24;
                this.toolStripButton_Locker1.Image = Properties.Resources.Locker_24;
                this.toolStripButton_Setup1.Image = Properties.Resources.Setup_24;
                //this.toolStripDropDownButton_language.Image = Properties.Resources.Language_24;
                this.toolStripButton_About1.Image = Properties.Resources.About_24;
            }

            if (toolStripComboBox2.SelectedIndex == 2)
            {
                this.toolStrip2.ImageScalingSize = new System.Drawing.Size(33, 32);

                this.toolStripButton_Drawline.Image = Properties.Resources.DrawLine_32;
                this.toolStripButton_Export.Image = Properties.Resources.ImageExport_32;
                this.toolStripButton_Copy.Image = Properties.Resources.ImageCopy_32;
                this.toolStripButton_Zoomin.Image = Properties.Resources.ZoomIn_32;
                this.toolStripButton_Zoomout.Image = Properties.Resources.ZoomOut_32;
                this.toolStripButton_Pan.Image = Properties.Resources.Pan_32;
                this.toolStripButton_Fullextent.Image = Properties.Resources.FullExtent_32;
                this.toolStripButton_Viewtext.Image = Properties.Resources.ViewText_32;
                this.toolStripButton_Locker1.Image = Properties.Resources.Locker_32;
                this.toolStripButton_Setup1.Image = Properties.Resources.Setup_32;
                //this.toolStripDropDownButton_language.Image = Properties.Resources.Language_32;
                this.toolStripButton_About1.Image = Properties.Resources.About_32;
            }
        }

        private void toolStripButton_Drawline_Click(object sender, EventArgs e)             //工具栏-绘制导线图
        {
            if (DrawLineFlag == false)
            {
                DrawLineFlag = true;
            }
            else DrawLineFlag = false;

            if (sLine == "0")
            {
                DrawClosedFlag = false;
            }
            if (sLine == "1")
            {
                DrawClosedFlag = false;
            }
            if (sLine == "2")
            {
                DrawClosedFlag = false;
            }
            if (sLine == "3")
            {
                DrawClosedFlag = true;
            }
            if (sLine == "4")
            {
                DrawClosedFlag = true;
            }
            if (sLine == "5")
            {
                DrawClosedFlag = false;
            }
            if (sLine == "6")
            {
                DrawClosedFlag = false;
            }
            if (sLine == "7")
            {
                DrawClosedFlag = false;
            }
            if (sLine == "8")
            {
                DrawClosedFlag = false;
            }

            toolStripButton_Fullextent_Click(sender, e);                //画图
        }

        private void toolStripButton_Export_Click(object sender, EventArgs e)               //工具栏-输出图像
        {
            MapSetting frm2 = new MapSetting();
            frm2.StartPosition = FormStartPosition.CenterParent;
            frm2.ShowDialog(this);

            try
            {
                if (n == 1)
                {
                    System.Drawing.Image newimage = pictureBox1.Image;

                    string strPath = "";
                    SaveFileDialog sfdlg = new SaveFileDialog();
                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                    {
                        sfdlg.Filter = "JPG 文件交换格式(*.jpg;*.jpeg;*.jfif;*.jpe)|*.jpg;*.jpeg;*.jfif;*.jpe|可移植网络图形(*.png)|*.png|Windows 位图(*.bmp;*.dib;*.rle)|*.bmp;*.dib;*.rle|Tag 图像文件格式(*.tif;*.tiff)|*.tif;*.tiff|Windows 图元文件(*.wmf)|*.wmf|内嵌的 PostScript(*.eps)|*.eps|Macintosh PICT(*.pct;*.pict)|*.pct;*.pict|WordPerfect 图形(*.wpg)|*.wpg|所有文件|*.*";
                    }
                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                    {
                        sfdlg.Filter = "JPG 檔交換格式(*.jpg;*.jpeg;*.jfif;*.jpe)|*.jpg;*.jpeg;*.jfif;*.jpe|可移植網路圖形(*.png)|*.png|Windows 點陣圖(*.bmp;*.dib;*.rle)|*.bmp;*.dib;*.rle|Tag 影像檔格式(*.tif;*.tiff)|*.tif;*.tiff|Windows 圖中繼檔(*.wmf)|*.wmf|內嵌的 PostScript(*.eps)|*.eps|Macintosh PICT(*.pct;*.pict)|*.pct;*.pict|WordPerfect 圖形(*.wpg)|*.wpg|所有檔|*.*";
                    }
                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                    {
                        sfdlg.Filter = "JPG File Exchange File(*.jpg;*.jpeg;*.jfif;*.jpe)|*.jpg;*.jpeg;*.jfif;*.jpe|Portable Network Graphics(*.png)|*.png|Windows Bitmap(*.bmp;*.dib;*.rle)|*.bmp;*.dib;*.rle|Tag Image File Format(*.tif;*.tiff)|*.tif;*.tiff|Windows Meta File(*.wmf)|*.wmf|Inline PostScript(*.eps)|*.eps|Macintosh PICT(*.pct;*.pict)|*.pct;*.pict|WordPerfect Image(*.wpg)|*.wpg|All Files|*.*";
                    }
                    sfdlg.ShowDialog();
                    strPath = sfdlg.FileName;
                    if (strPath != "")
                    {
                        //处理JPG质量的函数
                        //int level = 100; //图像质量 1-100的范围
                        ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
                        ImageCodecInfo ici = null;
                        foreach (ImageCodecInfo codec in codecs)
                        {
                            if (codec.MimeType == "image/jpeg")
                                ici = codec;
                        }
                        EncoderParameters ep = new EncoderParameters();
                        ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)nLevel);

                        newimage.Save(strPath, ici, ep);
                        newimage.Dispose();   //释放位图缓存

                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            MessageBox.Show("图像保存成功。", "Aurora智能提示");
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            MessageBox.Show("圖像保存成功。", "Aurora智慧提示");
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            MessageBox.Show("Image Saved Success。", "Aurora Intelligent Tips");
                        }

                    }
                    else return;
                }
                else return;
            }
            catch { }
        }

        private void toolStripButton_Copy_Click(object sender, EventArgs e)               //工具栏-复制图像
        {
            System.Drawing.Image newimage = pictureBox1.Image;
            try
            {
                Clipboard.SetDataObject(newimage);
                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                {
                    MessageBox.Show("图像复制成功。" + "\r\n"
                                    + "现在你可以直接将图像粘贴在Word等工具中。", "Aurora智能提示");
                }
                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                {
                    MessageBox.Show("圖像複製成功。" + "\r\n"
                                    + "現在你可以直接將圖像粘貼在Word等工具中。", "Aurora智慧提示");
                }
                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                {
                    MessageBox.Show("Copy Image Success." + "\r\n"
                                    + "Now you can paste it into Word or other tools.", "Aurora Intelligent Tips");
                }
            }
            catch { }
        }

        private void toolStripButton_Zoomin_Click(object sender, EventArgs e)               //工具栏-放大
        {
            //Bitmap a = Properties.Resources.ZoomIn_24;
            //SetCursor(a, new Point(24, 24)); //new Point() 定义鼠标的可用点位置。 
            nCursor = 1;          //设置鼠标样式

            if (MapScale < 0.1)
            {
                MapScale = 0.1; //最大比例尺
            }
            else
            {
                MapScale -= DeltaMapScale;
            }
            //绘制
            int AllItems = 0;
            AllItems = listView1.Items.Count;   //全部点的数目
            pictureBox1.Image = DrawCurPointOnScr(AllItems, MapScale);
        }

        private void toolStripButton_Zoomout_Click(object sender, EventArgs e)               //工具栏-缩小
        {
            //Bitmap a = Properties.Resources.ZoomOut_24;
            //SetCursor(a, new Point(24, 24)); //new Point() 定义鼠标的可用点位置。 
            nCursor = 2;//设置鼠标样式

            if (MapScale > 500000)
            {
                MapScale = 500000; //最小比例尺
            }
            else
            {
                MapScale += DeltaMapScale;
            }
            //绘制
            int AllItems = 0;
            AllItems = listView1.Items.Count;   //全部点的数目
            pictureBox1.Image = DrawCurPointOnScr(AllItems, MapScale);
        }

        private void toolStripButton_Pan_Click(object sender, EventArgs e)               //工具栏-平移
        {
            //Bitmap a = Properties.Resources.Pan_24;
            //SetCursor(a, new Point(24, 24)); //new Point() 定义鼠标的可用点位置。 
            nCursor = 3;                //设置鼠标样式

            if (nPanFlag == 0)
            {
                nPanFlag = 1;
            }
            else nPanFlag = 0;
        }

        private void toolStripButton_Fullextent_Click(object sender, EventArgs e)               //工具栏-全图
        {
            //Bitmap a = Properties.Resources.FullExtent_24;
            //SetCursor(a, new Point(24, 24)); //new Point() 定义鼠标的可用点位置。 
            nCursor = 4;                //设置鼠标样式

            #region 先将水准/坐标读入listview，再进行显示
            int nRow = 1;
            listView1.Items.Clear();

            string strDataPath = Application.StartupPath + "\\MapData\\ClosedTraverse.txt";
            if (sLine == "0")
            {
                strDataPath = Application.StartupPath + "\\MapData\\ClosedLeveling.txt";
            }
            if (sLine == "1")
            {
                strDataPath = Application.StartupPath + "\\MapData\\AnnexedLeveling.txt";
            }
            if (sLine == "2")
            {
                strDataPath = Application.StartupPath + "\\MapData\\SpurLeveling.txt";
            }
            if (sLine == "3")
            {
                strDataPath = Application.StartupPath + "\\MapData\\ClosedTraverse.txt";
            }
            if (sLine == "4")
            {
                strDataPath = Application.StartupPath + "\\MapData\\ClosedTraverseWithOuterPoint.txt";
            }
            if (sLine == "5")
            {
                strDataPath = Application.StartupPath + "\\MapData\\OneAngleConnTraverse.txt";
            }
            if (sLine == "6")
            {
                strDataPath = Application.StartupPath + "\\MapData\\TwoAngleConnTraverse.txt";
            }
            if (sLine == "7")
            {
                strDataPath = Application.StartupPath + "\\MapData\\OpenTraverse.txt";
            }
            if (sLine == "8")
            {
                strDataPath = Application.StartupPath + "\\MapData\\NoAngleConnTraverse.txt";
            }

            try
            {
                StreamReader sr = new StreamReader(strDataPath, Encoding.GetEncoding("gb2312"));
                string strRead = "";
                strRead = sr.ReadLine();
                while (strRead != null)
                {
                    string[] strSplit = strRead.Split(',');
                    ListViewItem item = new ListViewItem();
                    item = new ListViewItem((nRow++).ToString());
                    item.SubItems.Add(strSplit[0]);
                    item.SubItems.Add(strSplit[1]);
                    item.SubItems.Add(strSplit[2]);
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    listView1.Items.AddRange(new ListViewItem[] { item });
                    strRead = sr.ReadLine();
                }
            }
            catch { MessageBox.Show("绘图数据包丢失，请尝试重新运行Aurora平差计算。", "Aurora智能提示"); }
            #endregion

            int i = 0;
            double MaxEast = 0, MaxNorth = 0, MinEast = 0, MinNorth = 0;

            ////计算放样点在屏幕上的位置,并绘制
            int AllItems = 0;
            AllItems = listView1.Items.Count;   //全部点的数目
            //
            if (AllItems == 0) return;

            MaxNorth = MinNorth = double.Parse(listView1.Items[0].SubItems[2].Text);    //X
            MaxEast = MinEast = double.Parse(listView1.Items[0].SubItems[3].Text);      //Y

            double LocalY = 0;
            double LocalX = 0;
            for (i = 0; i < AllItems; i++)
            {
                LocalY = double.Parse(listView1.Items[i].SubItems[3].Text);
                LocalX = double.Parse(listView1.Items[i].SubItems[2].Text);

                if (LocalY > MaxEast) MaxEast = LocalY;
                if (LocalY < MinEast) MinEast = LocalY;
                if (LocalX > MaxNorth) MaxNorth = LocalX;
                if (LocalX < MinNorth) MinNorth = LocalX;
            }
            //
            double DeltaEast = 0, DeltaNorth = 0;
            DeltaEast = MaxEast - MinEast;
            DeltaNorth = MaxNorth - MinNorth;

            //根据绘图框尺寸缩放图形：
            double TmpScaleSize = 0;
            TmpScaleSize = Math.Max(DeltaEast, DeltaNorth);
            int TmpSize = 0;
            TmpSize = Math.Min(pictureBox1.Width, pictureBox1.Height);

            //double MapScale = 0.5;
            MapScale = TmpScaleSize * 50 / (TmpSize - 30);  //原始比例尺与像素关系 50 pels = 200 m
            DeltaMapScale = MapScale / 10;  //缩放的梯度为大小的十分之一

            //居中：
            WorldCenPoint.X = pictureBox1.Width / 2;
            WorldCenPoint.Y = pictureBox1.Height / 2;
            //
            WorldCenNorth = MinNorth + 0.5 * DeltaNorth;
            WorldCenEast = MinEast + 0.5 * DeltaEast;
            //绘制：
            pictureBox1.Image = DrawCurPointOnScr(AllItems, MapScale);
        }

        private void toolStripButton_Viewtext_Click(object sender, EventArgs e)               //工具栏-查看点名
        {
            //Bitmap a = Properties.Resources.ViewText_24;
            //SetCursor(a, new Point(24, 24)); //new Point() 定义鼠标的可用点位置。 
            nCursor = 5;                //设置鼠标样式

            ShowPointID = !ShowPointID;
            int AllItems = 0;
            AllItems = listView1.Items.Count;   //全部点的数目
            pictureBox1.Image = DrawCurPointOnScr(AllItems, MapScale);
        }

        private void toolStripButton_Locker1_Click(object sender, EventArgs e)              //工具栏-数据锁
        {
            try
            {
                PublicClass.AuroraMain.Hide();
                if (PublicClass.MyCmd != null)
                {
                    PublicClass.MyCmd.Hide();
                }
                PublicClass.Locker.Show();
                log.Info(DateTime.Now.ToString() + "Start Locker" + sender.ToString() + e.ToString());//写入一条新log
            }
            catch { }
        }

        private void toolStripButton_Setup1_Click(object sender, EventArgs e)               ////工具栏-设置
        {
            Setting FrmSetup = new Setting();
            FrmSetup.StartPosition = FormStartPosition.CenterParent;
            FrmSetup.ShowDialog(this);      //this 必须有，传递子窗体参数       //创建模态对话框
            //FrmSetup.Show(this);      //this 必须有，传递子窗体参数       //创建非模态对话框
        }

        private void toolStripButton_About1_Click(object sender, EventArgs e)               //工具栏-关于
        {
            AboutAurora FrmAbout = new AboutAurora();
            FrmAbout.StartPosition = FormStartPosition.CenterParent;      //创建模态对话框
            FrmAbout.ShowDialog();
        }

        //记录平移移动状态时，鼠标按下的点位值：
        private Point MoveMouseDownPoint = new Point(0, 0);

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            MoveMouseDownPoint = Control.MousePosition;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (nPanFlag == 1)
            {
                Point pCur = new Point();
                pCur = Control.MousePosition;
                int DeltaX = 0, DeltaY = 0;
                DeltaX = pCur.X - MoveMouseDownPoint.X;
                DeltaY = pCur.Y - MoveMouseDownPoint.Y;
                //
                if ((Math.Abs(WorldCenPoint.X) <= 10000) && Math.Abs(WorldCenPoint.Y) <= 10000) //防止移动时点坐标超出int范围
                {
                    WorldCenPoint.X += DeltaX;
                    WorldCenPoint.Y += DeltaY;
                }
                //绘制
                int AllItems = 0;
                AllItems = listView1.Items.Count;   //全部点的数目
                pictureBox1.Image = DrawCurPointOnScr(AllItems, MapScale);
            }
        }

        private void toolStripMenuItem_Zommin_Click(object sender, EventArgs e)             //右键菜单-放大
        {
            toolStripButton_Zoomin_Click(sender, e);
        }

        private void toolStripMenuItem_Zoomout_Click(object sender, EventArgs e)             //右键菜单-缩小
        {
            toolStripButton_Zoomout_Click(sender, e);
        }

        private void toolStripMenuItem_Pan_Click(object sender, EventArgs e)             //右键菜单-平移
        {
            toolStripButton_Pan_Click(sender, e);
        }

        private void toolStripMenuItem_Fullextent_Click(object sender, EventArgs e)             //右键菜单-全图
        {
            toolStripButton_Fullextent_Click(sender, e);
        }

        private void toolStripMenuItem_Viewtext_Click(object sender, EventArgs e)             //右键菜单-查看点名
        {
            toolStripButton_Viewtext_Click(sender, e);
        }

        public void SetCursor(Bitmap cursor, Point hotPoint)                //设置鼠标函数
        {
            int hotX = hotPoint.X;
            int hotY = hotPoint.Y;
            Bitmap myNewCursor = new Bitmap(cursor.Width * 2 - hotX, cursor.Height * 2 - hotY);
            Graphics g = Graphics.FromImage(myNewCursor);
            g.Clear(Color.FromArgb(0, 0, 0, 0));
            g.DrawImage(cursor, cursor.Width - hotX, cursor.Height - hotY, cursor.Width, cursor.Height);

            this.Cursor = new Cursor(myNewCursor.GetHicon());

            g.Dispose();
            myNewCursor.Dispose();
        }

        private void pictureBox1_Click(object sender, EventArgs e)              //在图像上点击，做出对应的变化
        {
            if (nCursor == 1)
            {
                toolStripButton_Zoomin_Click(sender, e);
            }
            if (nCursor == 2)
            {
                toolStripButton_Zoomout_Click(sender, e);
            }
            if (nCursor == 3)
            {
                toolStripButton_Pan_Click(sender, e);
            }
            if (nCursor == 4)
            {
                toolStripButton_Fullextent_Click(sender, e);
            }
            if (nCursor == 5)
            {
                toolStripButton_Viewtext_Click(sender, e);
            }
            else pictureBox1.Cursor = Cursors.Default;
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)             //妹的，鼠标样式咋就不变呢！！！
        {
            pictureBox1.Focus();
            //if (nCursor == 1)
            //{
                //Bitmap a1 = Properties.Resources.ZoomIn_24;
                //SetCursor(a1, new Point(24, 24)); //new Point() 定义鼠标的可用点位置。
                //Bitmap bmp = new Bitmap(Properties.Resources.ZoomIn_24);
                //Cursor cursor = new Cursor(bmp.GetHicon());
                //pictureBox1.Cursor = cursor;
            //}
            //if (nCursor == 2)
            //{
            //    Bitmap a2 = Properties.Resources.ZoomOut_24;
            //    SetCursor(a2, new Point(24, 24)); //new Point() 定义鼠标的可用点位置。
            //}
            //if (nCursor == 3)
            //{
            //    Bitmap a3 = Properties.Resources.Pan_24;
            //    SetCursor(a3, new Point(24, 24)); //new Point() 定义鼠标的可用点位置。
            //}
            //if (nCursor == 4)
            //{
            //    Bitmap a4 = Properties.Resources.FullExtent_24;
            //    SetCursor(a4, new Point(24, 24)); //new Point() 定义鼠标的可用点位置。
            //}
            //if (nCursor == 5)
            //{
            //    Bitmap a5 = Properties.Resources.ViewText_24;
            //    SetCursor(a5, new Point(24, 24)); //new Point() 定义鼠标的可用点位置。
            //}
        }

        void pictureBox1_MouseWheel(object sender, MouseEventArgs e)                //鼠标的滚动，实现缩放
        {
            Bitmap b = (Bitmap)this.BackgroundImage;
            if (e.Delta > 0)
            {
                if (MapScale < 0.1)
                {
                    MapScale = 0.1; //最大比例尺
                }
                else
                {
                    MapScale -= DeltaMapScale / 3;
                }
                //绘制
                int AllItems = 0;
                AllItems = listView1.Items.Count;   //全部点的数目
                pictureBox1.Image = DrawCurPointOnScr(AllItems, MapScale);
            }
            else
            {
                if (MapScale > 500000)
                {
                    MapScale = 500000; //最小比例尺
                }
                else
                {
                    MapScale += DeltaMapScale / 3;
                }
                //绘制
                int AllItems = 0;
                AllItems = listView1.Items.Count;   //全部点的数目
                pictureBox1.Image = DrawCurPointOnScr(AllItems, MapScale);
            }
        }

        private bool IsMouseInpictureBox()               //判断鼠标在picturebox的范围内
        {
            if (this.pictureBox1.Left < PointToClient(Cursor.Position).X && PointToClient(Cursor.Position).X < this.pictureBox1.Left + this.pictureBox1.Width && this.pictureBox1.Top < PointToClient(Cursor.Position).Y && PointToClient(Cursor.Position).Y < this.pictureBox1.Top + this.pictureBox1.Height)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //鼠标移动到特定位置提示信息
        private void toolStripButton_Drawline_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_Drawline.ToolTipText = "绘制导线图";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_Drawline.ToolTipText = "繪製導線圖";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_Drawline.ToolTipText = "Draw Line";
            }
        }

        private void toolStripButton_Export_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_Export.ToolTipText = "输出图像";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_Export.ToolTipText = "輸出圖像";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_Export.ToolTipText = "Export Map";
            }
        }

        private void toolStripButton_Copy_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_Copy.ToolTipText = "复制图像";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_Copy.ToolTipText = "複製圖像";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_Copy.ToolTipText = "Copy Image";
            }
        }

        private void toolStripButton_Zoomin_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_Zoomin.ToolTipText = "放大";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_Zoomin.ToolTipText = "放大";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_Zoomin.ToolTipText = "Zoom In";
            }
        }

        private void toolStripButton_Zoomout_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_Zoomout.ToolTipText = "缩小";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_Zoomout.ToolTipText = "縮小";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_Zoomout.ToolTipText = "Zoom Out";
            }
        }

        private void toolStripButton_Pan_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_Pan.ToolTipText = "平移";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_Pan.ToolTipText = "平移";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_Pan.ToolTipText = "Pan";
            }
        }

        private void toolStripButton_Fullextent_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_Fullextent.ToolTipText = "全图";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_Fullextent.ToolTipText = "全圖";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_Fullextent.ToolTipText = "Full Extent";
            }
        }

        private void toolStripButton_Viewtext_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_Viewtext.ToolTipText = "显示点名";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_Viewtext.ToolTipText = "顯示點名";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_Viewtext.ToolTipText = "Show PTName";
            }
        }
        
        private void toolStripButton_Locker1_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_Locker1.ToolTipText = "数据保护锁";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_Locker1.ToolTipText = "資料保護鎖";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_Locker1.ToolTipText = "Data Locker";
            }
        }

        private void toolStripButton_Setup1_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_Setup1.ToolTipText = "设置";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_Setup1.ToolTipText = "設置";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_Setup1.ToolTipText = "Settings";
            }
        }

        private void toolStripButton_About1_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_About1.ToolTipText = "关于";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_About1.ToolTipText = "關於";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_About1.ToolTipText = "About";
            }
        }

        #endregion


        #region         报表标签--Reporting

        private void toolStripComboBox3_SelectedIndexChanged(object sender, EventArgs e)                //改变图标大小
        {
            if (toolStripComboBox3.SelectedIndex == 0)
            {
                this.toolStrip3.ImageScalingSize = new System.Drawing.Size(16, 16);

                this.toolStripButton_ReportExport.Image = Properties.Resources.Export_Report_16;
                this.toolStripButton_ReportPrint.Image = Properties.Resources.Print_Report_16;
                this.toolStripButton_ReportRefresh.Image = Properties.Resources.Refresh_Report_16;
                this.toolStripDropDownButton_ReportZoom.Image = Properties.Resources.Zoom_Report_16;
                this.toolStripButton_Locker2.Image = Properties.Resources.Locker_16;
                this.toolStripButton_Setup2.Image = Properties.Resources.Setup_16;
                //this.toolStripDropDownButton_language.Image = Properties.Resources.Language_16;
                this.toolStripButton_About2.Image = Properties.Resources.About_16;
            }

            if (toolStripComboBox3.SelectedIndex == 1)
            {
                this.toolStrip3.ImageScalingSize = new System.Drawing.Size(24, 24);

                this.toolStripButton_ReportExport.Image = Properties.Resources.Export_Report_24;
                this.toolStripButton_ReportPrint.Image = Properties.Resources.Print_Report_24;
                this.toolStripButton_ReportRefresh.Image = Properties.Resources.Refresh_Report_24;
                this.toolStripDropDownButton_ReportZoom.Image = Properties.Resources.Zoom_Report_24;
                this.toolStripButton_Locker2.Image = Properties.Resources.Locker_24;
                this.toolStripButton_Setup2.Image = Properties.Resources.Setup_24;
                //this.toolStripDropDownButton_language.Image = Properties.Resources.Language_24;
                this.toolStripButton_About2.Image = Properties.Resources.About_24;
            }
        }

        private void toolStripButton_ReportExport_Click(object sender, EventArgs e)              //工具栏-导出报表
        {
            crystalReportViewer1.ExportReport();
        }

        private void toolStripButton_ReportPrint_Click(object sender, EventArgs e)              //工具栏-打印报表
        {
            crystalReportViewer1.PrintReport();
        }

        private void toolStripButton_ReportRefresh_Click(object sender, EventArgs e)              //工具栏-刷新报表
        {
            crystalReportViewer1.RefreshReport();
        }

        private void toolStripMenuItem_Zoom25_Click(object sender, EventArgs e)              //工具栏-缩放报表
        {
            crystalReportViewer1.Zoom(25);
        }

        private void toolStripMenuItem_Zoom50_Click(object sender, EventArgs e)              //工具栏-缩放报表
        {
            crystalReportViewer1.Zoom(50);
        }

        private void toolStripMenuItem_Zoom75_Click(object sender, EventArgs e)              //工具栏-缩放报表
        {
            crystalReportViewer1.Zoom(75);
        }

        private void toolStripMenuItem_Zoom100_Click(object sender, EventArgs e)              //工具栏-缩放报表
        {
            crystalReportViewer1.Zoom(100);
        }

        private void toolStripMenuItem_Zoom150_Click(object sender, EventArgs e)              //工具栏-缩放报表
        {
            crystalReportViewer1.Zoom(150);
        }

        private void toolStripMenuItem_Zoom200_Click(object sender, EventArgs e)              //工具栏-缩放报表
        {
            crystalReportViewer1.Zoom(200);
        }

        private void toolStripMenuItem_Zoom300_Click(object sender, EventArgs e)              //工具栏-缩放报表
        {
            crystalReportViewer1.Zoom(300);
        }

        private void toolStripMenuItem_Zoom400_Click(object sender, EventArgs e)              //工具栏-缩放报表
        {
            crystalReportViewer1.Zoom(400);
        }

        private void toolStripButton_Locker2_Click(object sender, EventArgs e)              //工具栏-数据锁
        {
            try
            {
                PublicClass.AuroraMain.Hide();
                if (PublicClass.MyCmd != null)
                {
                    PublicClass.MyCmd.Hide();
                }
                PublicClass.Locker.Show();
                log.Info(DateTime.Now.ToString() + "Start Locker" + sender.ToString() + e.ToString());//写入一条新log
            }
            catch { }
        }

        private void toolStripButton_Setup2_Click(object sender, EventArgs e)               //工具栏-设置
        {
            Setting FrmSetup = new Setting();
            FrmSetup.StartPosition = FormStartPosition.CenterParent;
            FrmSetup.ShowDialog(this);      //this 必须有，传递子窗体参数       //创建模态对话框
            //FrmSetup.Show(this);      //this 必须有，传递子窗体参数       //创建非模态对话框
        }

        private void toolStripButton_About2_Click(object sender, EventArgs e)               //工具栏-关于
        {
            AboutAurora FrmAbout = new AboutAurora();
            FrmAbout.StartPosition = FormStartPosition.CenterParent;      //创建模态对话框
            FrmAbout.ShowDialog();
        }

        //右键菜单
        private void toolStripMenuItem_Export_Click(object sender, EventArgs e)
        {
            crystalReportViewer1.ExportReport();
        }

        private void toolStripMenuItem_Print_Click(object sender, EventArgs e)
        {
            crystalReportViewer1.PrintReport();
        }

        private void toolStripMenuItem_Refresh_Click(object sender, EventArgs e)
        {
            crystalReportViewer1.RefreshReport();
        }

        private void toolStripMenuItem_25_Click(object sender, EventArgs e)
        {
            crystalReportViewer1.Zoom(25);
        }

        private void toolStripMenuItem_50_Click(object sender, EventArgs e)
        {
            crystalReportViewer1.Zoom(50);
        }

        private void toolStripMenuItem_75_Click(object sender, EventArgs e)
        {
            crystalReportViewer1.Zoom(75);
        }

        private void toolStripMenuItem_100_Click(object sender, EventArgs e)
        {
            crystalReportViewer1.Zoom(100);
        }

        private void toolStripMenuItem_150_Click(object sender, EventArgs e)
        {
            crystalReportViewer1.Zoom(150);
        }

        private void toolStripMenuItem_200_Click(object sender, EventArgs e)
        {
            crystalReportViewer1.Zoom(200);
        }

        private void toolStripMenuItem_300_Click(object sender, EventArgs e)
        {
            crystalReportViewer1.Zoom(300);
        }

        private void toolStripMenuItem_400_Click(object sender, EventArgs e)
        {
            crystalReportViewer1.Zoom(400);
        }



        //鼠标移动到特定位置提示信息
        private void toolStripButton_ReportExport_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_ReportExport.ToolTipText = "导出报表";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_ReportExport.ToolTipText = "匯出報表";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_ReportExport.ToolTipText = "Export Report";
            }
        }

        private void toolStripButton_ReportPrint_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_ReportPrint.ToolTipText = "打印报表";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_ReportPrint.ToolTipText = "打印報表";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_ReportPrint.ToolTipText = "Print Report";
            }
        }

        private void toolStripButton_ReportRefresh_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_ReportRefresh.ToolTipText = "刷新报表";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_ReportRefresh.ToolTipText = "刷新報表";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_ReportRefresh.ToolTipText = "Refresh Report";
            }
        }

        private void toolStripDropDownButton_ReportZoom_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripDropDownButton_ReportZoom.ToolTipText = "缩放级别";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripDropDownButton_ReportZoom.ToolTipText = "縮放級別";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripDropDownButton_ReportZoom.ToolTipText = "Zoom Level";
            }
        }

        private void toolStripButton_Locker2_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_Locker2.ToolTipText = "数据保护锁";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_Locker2.ToolTipText = "資料保護鎖";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_Locker2.ToolTipText = "Data Locker";
            }
        }

        private void toolStripButton_Setup2_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_Setup2.ToolTipText = "设置";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_Setup2.ToolTipText = "設置";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_Setup2.ToolTipText = "Settings";
            }
        }

        private void toolStripButton_About2_MouseEnter(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                toolStripButton_About2.ToolTipText = "关于";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                toolStripButton_About2.ToolTipText = "關於";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                toolStripButton_About2.ToolTipText = "About";
            }
        }

        #endregion


        public void iMap(object sender, EventArgs e)                //绘图函数
        {
            if (nCalcFlag == 0)      //设置全局变量nCalcFlag，默认为零。当完成平差计算时，nCalcFlag = 1。否则不能进行绘图和生成报表。
            {
                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                {
                    MessageBox.Show("请先完成平差计算功能。", "Aurora智能提示");
                }
                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                {
                    MessageBox.Show("請先完成平差計算功能。", "Aurora智慧提示");
                }
                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                {
                    MessageBox.Show("Please finish adjust first.", "Aurora Intelligent Tips");
                }

                return;
            }
            else
            {
                ProgressBar fProgressBar = new ProgressBar();   //调用进度条窗体
                //fProgressBar.MdiParent = this;
                //fProgressBar.StartPosition = FormStartPosition.CenterScreen;
                //int fProgressBarX, fProgressBarY;
                //fProgressBarX = (this.Right - this.Left) / 2 - fProgressBar.Width;
                //fProgressBarY = (this.Bottom - this.Top) / 2 - fProgressBar.Height;
                //fProgressBar.Location = new Point(fProgressBarX, fProgressBarY);
                fProgressBar.Show();
                Delay(1234);
                fProgressBar.Hide();

                try
                {
                    int nCount = this.AdjustListView.Items.Count;
                    switch (AdjustType.SelectedIndex)       //选择平差类型，写入绘图需要的数据到本地
                    {
                        case 0: //闭合水准
                            {
                                string strPtName = "", strDist = "", strHeight = "", strMyData = "";
                                FileStream aFile = new FileStream(Application.StartupPath + "\\MapData\\ClosedLeveling.txt", FileMode.Create);
                                StreamWriter MyWriter = new StreamWriter(aFile, Encoding.GetEncoding("gb2312"));

                                double dLength = 0.0;
                                for (int i = 0; i < nCount; i++)
                                {
                                    strPtName = this.AdjustListView.Items[i].SubItems[1].Text;
                                    string strLength = "";
                                    double dDist = 0.0;
                                    if (i > 0)
                                    {
                                        strDist = this.AdjustListView.Items[i - 1].SubItems[2].Text;
                                        dDist = Convert.ToDouble(strDist);
                                    }

                                    dLength = dLength + dDist;              //保存水准数据时，将第一个的距离设置为0，往后累加。
                                    strLength = dLength.ToString("#0.0000");
                                    if (i == 0)
                                    {
                                        strLength = "0.0";
                                    }
                                    strHeight = this.AdjustListView.Items[i].SubItems[7].Text;
                                    strMyData = strPtName + "," + strHeight + "," + strLength;
                                    try
                                    {
                                        MyWriter.WriteLine(strMyData, Encoding.GetEncoding("gb2312"));
                                    }
                                    catch (Exception Err)
                                    {
                                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        return;
                                    }
                                }
                                if (MyWriter != null)
                                {
                                    MyWriter.Close();
                                }
                                log.Info(DateTime.Now.ToString() + "Closed Leveling Mapping" + sender.ToString() + e.ToString());//写入一条新log
                            }
                            break;

                        case 1: //附和水准
                            {
                                string strPtName = "", strDist = "", strHeight = "", strMyData = "";
                                FileStream aFile = new FileStream(Application.StartupPath + "\\MapData\\AnnexedLeveling.txt", FileMode.Create);
                                StreamWriter MyWriter = new StreamWriter(aFile, Encoding.GetEncoding("gb2312"));

                                double dLength = 0.0;
                                for (int i = 0; i < nCount; i++)
                                {
                                    strPtName = this.AdjustListView.Items[i].SubItems[1].Text;
                                    string strLength = "";
                                    double dDist = 0.0;
                                    if (i > 0)
                                    {
                                        strDist = this.AdjustListView.Items[i - 1].SubItems[2].Text;
                                        dDist = Convert.ToDouble(strDist);
                                    }

                                    dLength = dLength + dDist;
                                    strLength = dLength.ToString("#0.0000");
                                    if (i == 0)
                                    {
                                        strLength = "0.0";
                                    }
                                    strHeight = this.AdjustListView.Items[i].SubItems[7].Text;
                                    strMyData = strPtName + "," + strHeight + "," + strLength;
                                    try
                                    {
                                        MyWriter.WriteLine(strMyData);
                                    }
                                    catch (Exception Err)
                                    {
                                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        return;
                                    }
                                }
                                if (MyWriter != null)
                                {
                                    MyWriter.Close();
                                }
                                log.Info(DateTime.Now.ToString() + "Annexed Leveling Mapping" + sender.ToString() + e.ToString());//写入一条新log
                            }
                            break;

                        case 2: //支水准
                            {
                                string strPtName = "", strDist = "", strHeight = "", strMyData = "";
                                FileStream aFile = new FileStream(Application.StartupPath + "\\MapData\\SpurLeveling.txt", FileMode.Create);
                                StreamWriter MyWriter = new StreamWriter(aFile, Encoding.GetEncoding("gb2312"));

                                double dLength = 0.0;
                                for (int i = 0; i < nCount; i++)
                                {
                                    strPtName = this.AdjustListView.Items[i].SubItems[1].Text;
                                    string strLength = "";
                                    double dDist = 0.0;
                                    if (i > 0)
                                    {
                                        strDist = this.AdjustListView.Items[i - 1].SubItems[2].Text;
                                        dDist = Convert.ToDouble(strDist);
                                    }

                                    dLength = dLength + dDist;
                                    strLength = dLength.ToString("#0.0000");
                                    if (i == 0)
                                    {
                                        strLength = "0.0";
                                    }
                                    strHeight = this.AdjustListView.Items[i].SubItems[5].Text;
                                    strMyData = strPtName + "," + strHeight + "," + strLength;
                                    try
                                    {
                                        MyWriter.WriteLine(strMyData);
                                    }
                                    catch (Exception Err)
                                    {
                                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        return;
                                    }
                                }
                                if (MyWriter != null)
                                {
                                    MyWriter.Close();
                                }
                                log.Info(DateTime.Now.ToString() + "Spur Leveling Mapping" + sender.ToString() + e.ToString());//写入一条新log
                            }
                            break;

                        case 3: //闭合导线
                            {
                                string strStation = "", strCoorX = "", strCoorY = "", strMyData = "";
                                FileStream aFile = new FileStream(Application.StartupPath + "\\MapData\\ClosedTraverse.txt", FileMode.Create);
                                StreamWriter MyWriter = new StreamWriter(aFile, Encoding.GetEncoding("gb2312"));
                                for (int i = 0; i < nCount; i++)
                                {
                                    strStation = this.AdjustListView.Items[i].SubItems[1].Text;
                                    strCoorX = this.AdjustListView.Items[i].SubItems[11].Text;
                                    strCoorY = this.AdjustListView.Items[i].SubItems[12].Text;
                                    strMyData = strStation + "," + strCoorX + "," + strCoorY;
                                    try
                                    {
                                        MyWriter.WriteLine(strMyData);
                                    }
                                    catch (Exception Err)
                                    {
                                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        return;
                                    }
                                }
                                if (MyWriter != null)
                                {
                                    MyWriter.Close();
                                }
                                log.Info(DateTime.Now.ToString() + "Closed Traverse Mapping" + sender.ToString() + e.ToString());//写入一条新log
                            }
                            break;

                        case 4: //闭合导线(含外支点)
                            {
                                string strStation = "", strCoorX = "", strCoorY = "", strMyData = "";
                                FileStream aFile = new FileStream(Application.StartupPath + "\\MapData\\ClosedTraverseWithOuterPoint.txt", FileMode.Create);
                                StreamWriter MyWriter = new StreamWriter(aFile, Encoding.GetEncoding("gb2312"));
                                for (int i = 0; i < nCount; i++)
                                {
                                    strStation = this.AdjustListView.Items[i].SubItems[1].Text;
                                    strCoorX = this.AdjustListView.Items[i].SubItems[11].Text;
                                    strCoorY = this.AdjustListView.Items[i].SubItems[12].Text;
                                    strMyData = strStation + "," + strCoorX + "," + strCoorY;
                                    try
                                    {
                                        MyWriter.WriteLine(strMyData);
                                    }
                                    catch (Exception Err)
                                    {
                                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        return;
                                    }
                                }
                                if (MyWriter != null)
                                {
                                    MyWriter.Close();
                                }
                                log.Info(DateTime.Now.ToString() + "Closed Traverse With Outer Point Mapping" + sender.ToString() + e.ToString());//写入一条新log
                            }
                            break;

                        case 5: //具有一个连接角的附和导线
                            {
                                string strStation = "", strCoorX = "", strCoorY = "", strMyData = "";
                                FileStream aFile = new FileStream(Application.StartupPath + "\\MapData\\OneAngleConnTraverse.txt", FileMode.Create);
                                StreamWriter MyWriter = new StreamWriter(aFile, Encoding.GetEncoding("gb2312"));
                                for (int i = 0; i < nCount; i++)
                                {
                                    strStation = this.AdjustListView.Items[i].SubItems[1].Text;
                                    strCoorX = this.AdjustListView.Items[i].SubItems[9].Text;
                                    strCoorY = this.AdjustListView.Items[i].SubItems[10].Text;
                                    strMyData = strStation + "," + strCoorX + "," + strCoorY;
                                    try
                                    {
                                        MyWriter.WriteLine(strMyData);
                                    }
                                    catch (Exception Err)
                                    {
                                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        return;
                                    }
                                }
                                if (MyWriter != null)
                                {
                                    MyWriter.Close();
                                }
                                log.Info(DateTime.Now.ToString() + "One Angle Conn Traverse Mapping" + sender.ToString() + e.ToString());//写入一条新log
                            }
                            break;

                        case 6: //具有两个连接角的附和导线
                            {
                                string strStation = "", strCoorX = "", strCoorY = "", strMyData = "";
                                FileStream aFile = new FileStream(Application.StartupPath + "\\MapData\\TwoAngleConnTraverse.txt", FileMode.Create);
                                StreamWriter MyWriter = new StreamWriter(aFile, Encoding.GetEncoding("gb2312"));
                                for (int i = 0; i < nCount; i++)
                                {
                                    strStation = this.AdjustListView.Items[i].SubItems[1].Text;
                                    strCoorX = this.AdjustListView.Items[i].SubItems[11].Text;
                                    strCoorY = this.AdjustListView.Items[i].SubItems[12].Text;
                                    strMyData = strStation + "," + strCoorX + "," + strCoorY;
                                    try
                                    {
                                        MyWriter.WriteLine(strMyData);
                                    }
                                    catch (Exception Err)
                                    {
                                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        return;
                                    }
                                }
                                if (MyWriter != null)
                                {
                                    MyWriter.Close();
                                }
                                log.Info(DateTime.Now.ToString() + "Two Angle Conn Traverse Mapping" + sender.ToString() + e.ToString());//写入一条新log
                            }
                            break;

                        case 7: //支导线
                            {
                                string strStation = "", strCoorX = "", strCoorY = "", strMyData = "";
                                FileStream aFile = new FileStream(Application.StartupPath + "\\MapData\\OpenTraverse.txt", FileMode.Create);
                                StreamWriter MyWriter = new StreamWriter(aFile, Encoding.GetEncoding("gb2312"));
                                for (int i = 0; i < nCount; i++)
                                {
                                    strStation = this.AdjustListView.Items[i].SubItems[1].Text;
                                    strCoorX = this.AdjustListView.Items[i].SubItems[7].Text;
                                    strCoorY = this.AdjustListView.Items[i].SubItems[8].Text;
                                    strMyData = strStation + "," + strCoorX + "," + strCoorY;
                                    try
                                    {
                                        MyWriter.WriteLine(strMyData);
                                    }
                                    catch (Exception Err)
                                    {
                                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        return;
                                    }
                                }
                                if (MyWriter != null)
                                {
                                    MyWriter.Close();
                                }
                                log.Info(DateTime.Now.ToString() + "Open Traverse Mapping" + sender.ToString() + e.ToString());//写入一条新log
                            }
                            break;

                        case 8: //无连接角导线
                            {
                                string strStation = "", strCoorX = "", strCoorY = "", strMyData = "";
                                FileStream aFile = new FileStream(Application.StartupPath + "\\MapData\\NoAngleConnTraverse.txt", FileMode.Create);
                                StreamWriter MyWriter = new StreamWriter(aFile, Encoding.GetEncoding("gb2312"));
                                for (int i = 0; i < nCount; i++)
                                {
                                    strStation = this.AdjustListView.Items[i].SubItems[1].Text;
                                    strCoorX = this.AdjustListView.Items[i].SubItems[9].Text;
                                    strCoorY = this.AdjustListView.Items[i].SubItems[10].Text;
                                    strMyData = strStation + "," + strCoorX + "," + strCoorY;
                                    try
                                    {
                                        MyWriter.WriteLine(strMyData);
                                    }
                                    catch (Exception Err)
                                    {
                                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                        {
                                            MessageBox.Show(Err.ToString(), "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                        return;
                                    }
                                }
                                if (MyWriter != null)
                                {
                                    MyWriter.Close();
                                }
                                log.Info(DateTime.Now.ToString() + "No Angle Conn Traverse Mapping" + sender.ToString() + e.ToString());//写入一条新log
                            }
                            break;
                    }

                    //写在每一个平差之中以细分
                    //log.Info(DateTime.Now.ToString() + sender.ToString() + e.ToString());//写入一条新log
                }
                catch (System.Exception ex)
                {
                    log.Info(DateTime.Now.ToString() + "Mapping Exception" + ex.ToString());//写入一条新log
                }

                //if (File.Exists(Application.StartupPath + "\\Mapping.exe"))
                //{
                //    System.Diagnostics.Process.Start(Application.StartupPath + "\\Mapping.exe");            //调用绘图Mapping
                //}
                //else
                //{
                //    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                //    {
                //        MessageBox.Show("Aurora智能绘图.exe丢失，请尝试重新安装Aurora。", "Aurora智能提示");
                //    }
                //    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                //    {
                //        MessageBox.Show("Aurora智能繪圖.exe丟失，請嘗試重新安裝Aurora。", "Aurora智慧提示");
                //    }
                //    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                //    {
                //        MessageBox.Show("Aurora Mapping.exe is missing, Please try to reinstall Aurora.", "Aurora Intelligent Tips");
                //    }
                //}
            }
        }

        public void iReport(object sender, EventArgs e)              //报表函数,写入Access数据库
        {
            RegistryKey MyReg0, RegGUIDFlag;                //未注册版本不支持报表功能
            MyReg0 = Registry.CurrentUser;
            try
            {
                RegGUIDFlag = MyReg0.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                if (RegGUIDFlag.GetValue("nRegGUIDFlag").ToString() == "0")
                {
                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                    {
                        MessageBox.Show("未注册版本不支持报表功能，请申请试用学习版或购买商业版。", "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                    {
                        MessageBox.Show("未註冊版本不支援報表功能，請申請試用學習版或購買商業版。", "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                    {
                        MessageBox.Show("Unregistered version does not support Reports, Please try Study version or Buy Commercial version.", "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    tabControl1.SelectedTab = tabPage1;
                    toolStrip1.Visible = true;
                    toolStrip2.Visible = false;
                    toolStrip3.Visible = false;

                    return;
                }
            }
            catch { }

            if (nCalcFlag == 0)      //设置全局变量nCalcFlag，默认为零。当完成平差计算时，nCalcFlag = 1。否则不能进行绘图和生成报表。
            {
                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                {
                    MessageBox.Show("请先完成平差计算功能。", "Aurora智能提示");
                }
                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                {
                    MessageBox.Show("請先完成平差計算功能。", "Aurora智慧提示");
                }
                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                {
                    MessageBox.Show("Please finish adjust first.", "Aurora Intelligent Tips");
                }

                return;
            }
            else
            {
                ProgressBar fProgressBar = new ProgressBar();   //调用进度条窗体
                //fProgressBar.MdiParent = this;
                //fProgressBar.StartPosition = FormStartPosition.CenterScreen;
                //int fProgressBarX, fProgressBarY;
                //fProgressBarX = (this.Right - this.Left) / 2 - fProgressBar.Width;
                //fProgressBarY = (this.Bottom - this.Top) / 2 - fProgressBar.Height;
                //fProgressBar.Location = new Point(fProgressBarX, fProgressBarY);
                fProgressBar.Show();
                Delay(1234);
                fProgressBar.Hide();

                try
                {
                    //思路：先往数据库里写入数据，然后显示。

                    //从注册表中读取项目信息
                    RegistryKey MyReg, RegProjectInfo;
                    MyReg = Registry.CurrentUser;//获取当前用户注册表项
                    try
                    {
                        RegProjectInfo = MyReg.CreateSubKey("Software\\Aurora\\ProjectInfo");//在注册表项中创建子项
                        strProjectName = (RegProjectInfo.GetValue("ProjectName")).ToString();
                        strCalculator = (RegProjectInfo.GetValue("Calculator")).ToString();
                        strChecker = (RegProjectInfo.GetValue("Checker")).ToString();
                        strMyGrade = MeasGrade.Text;
                    }
                    catch { }
                    if (MeasGrade.Text == "自定义" || MeasGrade.Text == "自定義" || MeasGrade.Text == "User Defined")
                    {
                        try
                        {
                            RegProjectInfo = MyReg.CreateSubKey("Software\\Aurora\\ProjectInfo");//在注册表项中创建子项
                            strProjectName = (RegProjectInfo.GetValue("ProjectName")).ToString();
                            strCalculator = (RegProjectInfo.GetValue("Calculator")).ToString();
                            strChecker = (RegProjectInfo.GetValue("Checker")).ToString();
                            strMyGrade = (RegProjectInfo.GetValue("MyGrade")).ToString();
                        }
                        catch { }
                    }

                    //角度闭合差fb，其实就是改正数之和SumAdjust的取负。
                    //fx fy，就是SumAdjDeltaX、SumAdjDeltaY的取负。
                    //这些数据无需重复计算，在每个平差计算过后，直接赋值给对应的公共变量。

                    //pfb = (Convert.ToInt16(pSumAdjust) * -1).ToString("#0");
                    //pfx = (Convert.ToDouble(pSumAdjDeltaX) * -1).ToString("#0.0000");
                    //pfy = (Convert.ToDouble(pSumAdjDeltaY) * -1).ToString("#0.0000");
                    //pf = (Math.Sqrt(Convert.ToDouble(pSumAdjDeltaX) * Convert.ToDouble(pSumAdjDeltaX) + Convert.ToDouble(pSumAdjDeltaY) * Convert.ToDouble(pSumAdjDeltaY))).ToString("#0.0000");


                    switch (AdjustType.SelectedIndex)
                    {
                        case 0: //闭合水准
                            {
                                //连接access数据库
                                //OleDbConnection dbConn = new OleDbConnection("Provider = Microsoft.ACE.OLEDB.12.0 ; Data Source = Adjustment.accdb; Persist Security Info=False; Jet OLEDB:Database Password=lmzl123456789");

                                OleDbConnection dbConn = new OleDbConnection("Provider = Microsoft.ACE.OLEDB.12.0 ; Data Source = Adjustment.accdb; Persist Security Info=False");
                                if (dbConn.State != ConnectionState.Open) dbConn.Open();
                                else
                                {
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("Aurora数据库已经被其他程序占用，请关闭先。", "智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("Aurora資料庫已經被其他程式佔用，請關閉先。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Aurora database is occupied by some program else, Close them first.", "Aurora Intelligent Tips");
                                    }
                                }

                                OleDbCommand cmd = new OleDbCommand("DELETE from ClosedLeveling", dbConn);      //首先清空表里面的数据
                                cmd.ExecuteNonQuery();
                                OleDbCommand cmd1 = new OleDbCommand("DELETE from CommonFields", dbConn);
                                cmd1.ExecuteNonQuery();

                                string strfh = textBox1.Text;
                                string strfr = textBox2.Text;
                                string strfb = textBox5.Text;
                                string strSumLength = textBox6.Text;
                                try
                                {
                                    //string cmdString = "INSERT INTO CommonFields([ID],[fh],[fr],[SumStations],[SumLength],[SumAdjust],[SumAdjustLevelDiff],[ProjectName],[MyGrade],[Calculator],[Checker]) VALUES('1','" + strfh + "','" + strfr + "','" + pSumStations + "','" + strSumLength + "','" + pSumAdjust + "','" + pSumAdjustLevelDiff + "','" + strProjectName + "','" + strMyGrade + "','" + strCalculator + "','" + strChecker + "')";
                                    string cmdString = "INSERT INTO CommonFields([ID],[fh],[fr],[fb],[SumStations],[SumLength],[SumObsAngle],[SumAdjust],[SumAdjustLevelDiff],[ProjectName],[MyGrade],[Calculator],[Checker]) VALUES('1','" + strfh + "','" + strfr + "','" + strfb + "','" + pSumStations + "','" + strSumLength + "','" + pSumObsAngle + "','" + pSumAdjust + "','" + pSumAdjustLevelDiff + "','" + strProjectName + "','" + strMyGrade + "','" + strCalculator + "','" + strChecker + "')";

                                    cmd1 = new OleDbCommand();
                                    cmd1.Connection = dbConn;
                                    cmd1.CommandText = cmdString;
                                    cmd1.ExecuteNonQuery();
                                }
                                catch (Exception Ex)
                                { MessageBox.Show(Ex.ToString()); }

                                foreach (ListViewItem lvi in AdjustListView.Items)
                                {
                                    string strID = lvi.SubItems[0].Text;                //最好当字符串处理。否则空的单元格转化为双精度时报错。
                                    string strPTName = lvi.SubItems[1].Text;
                                    string strDist = lvi.SubItems[2].Text;
                                    string strStations = lvi.SubItems[3].Text;
                                    string strObsLevelDiff = lvi.SubItems[4].Text;
                                    string strAdjust = lvi.SubItems[5].Text;
                                    string strAdjustLevelDiff = lvi.SubItems[6].Text;
                                    string strHeight = lvi.SubItems[7].Text;

                                    try
                                    {
                                        //object oID = strID;
                                        //object oPTName = strPTName;
                                        //object oDist = strDist;
                                        //object oStations = strStations;
                                        //object oObsLevelDiff = strObsLevelDiff;
                                        //object oAdjust = strAdjust;
                                        //object oAdjustLevelDiff = strAdjustLevelDiff;
                                        //object oHeight = strHeight;
                                        string cmdString = "INSERT INTO ClosedLeveling([ID],[PTName],[Dist],[Stations],[ObsLevelDiff],[Adjust],[AdjustLevelDiff],[Height]) VALUES('" + strID + "','" + strPTName + "','" + strDist + "','" + strStations + "','" + strObsLevelDiff + "','" + strAdjust + "','" + strAdjustLevelDiff + "','" + strHeight + "')";
                                        //string cmdString = "INSERT INTO ClosedLeveling([ID],[PTName],[Dist],[Stations],[ObsLevelDiff],[Adjust],[AdjustLevelDiff],[Height]) VALUES('" + oID + "','" + oPTName + "','" + oDist + "','" + oStations + "','" + oObsLevelDiff + "','" + oAdjust + "','" + oAdjustLevelDiff + "','" + oHeight + "')";

                                        cmd = new OleDbCommand();
                                        cmd.Connection = dbConn;
                                        cmd.CommandText = cmdString;
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (Exception Ex)
                                    { }
                                    cmd.Dispose();
                                }

                                //关闭数据库的连接
                                dbConn.Close();
                                dbConn.Dispose();

                                log.Info(DateTime.Now.ToString() + "Closed Leveling Reporting" + sender.ToString() + e.ToString());//写入一条新log
                            }
                            break;

                        case 1: //附和水准
                            {
                                //连接access数据库
                                OleDbConnection dbConn = new OleDbConnection("Provider = Microsoft.ACE.OLEDB.12.0 ; Data Source = Adjustment.accdb; Jet OLEDB:Database Password=lmzl123456789");
                                if (dbConn.State != ConnectionState.Open) dbConn.Open();
                                else
                                {
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("Aurora数据库已经被其他程序占用，请关闭先。", "智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("Aurora資料庫已經被其他程式佔用，請關閉先。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Aurora database is occupied by some else program, Close them first.", "Aurora Intelligent Tips");
                                    }
                                }

                                OleDbCommand cmd = new OleDbCommand("DELETE from AnnexedLeveling", dbConn);      //首先清空表里面的数据
                                cmd.ExecuteNonQuery();
                                OleDbCommand cmd1 = new OleDbCommand("DELETE from CommonFields", dbConn);
                                cmd1.ExecuteNonQuery();

                                string strfh = textBox1.Text;
                                string strfr = textBox2.Text;
                                string strfb = textBox5.Text;
                                string strSumLength = textBox6.Text;
                                try
                                {
                                    string cmdString = "INSERT INTO CommonFields([ID],[fh],[fr],[fb],[SumStations],[SumLength],[SumObsAngle],[SumAdjust],[SumAdjustLevelDiff],[ProjectName],[MyGrade],[Calculator],[Checker]) VALUES('1','" + strfh + "','" + strfr + "','" + strfb + "','" + pSumStations + "','" + strSumLength + "','" + pSumObsAngle + "','" + pSumAdjust + "','" + pSumAdjustLevelDiff + "','" + strProjectName + "','" + strMyGrade + "','" + strCalculator + "','" + strChecker + "')";

                                    cmd1 = new OleDbCommand();
                                    cmd1.Connection = dbConn;
                                    cmd1.CommandText = cmdString;
                                    cmd1.ExecuteNonQuery();
                                }
                                catch (Exception Ex)
                                { }

                                foreach (ListViewItem lvi in AdjustListView.Items)
                                {
                                    string strID = lvi.SubItems[0].Text;
                                    string strPTName = lvi.SubItems[1].Text;
                                    string strDist = lvi.SubItems[2].Text;
                                    string strStations = lvi.SubItems[3].Text;
                                    string strObsLevelDiff = lvi.SubItems[4].Text;
                                    string strAdjust = lvi.SubItems[5].Text;
                                    string strAdjustLevelDiff = lvi.SubItems[6].Text;
                                    string strHeight = lvi.SubItems[7].Text;

                                    try
                                    {
                                        //object oID = strID;
                                        //object oPTName = strPTName;
                                        //object oDist = strDist;
                                        //object oStations = strStations;
                                        //object oObsLevelDiff = strObsLevelDiff;
                                        //object oAdjust = strAdjust;
                                        //object oAdjustLevelDiff = strAdjustLevelDiff;
                                        //object oHeight = strHeight;

                                        string cmdString = "INSERT INTO AnnexedLeveling([ID],[PTName],[Dist],[Stations],[ObsLevelDiff],[Adjust],[AdjustLevelDiff],[Height]) VALUES('" + strID + "','" + strPTName + "','" + strDist + "','" + strStations + "','" + strObsLevelDiff + "','" + strAdjust + "','" + strAdjustLevelDiff + "','" + strHeight + "')";

                                        cmd = new OleDbCommand();
                                        cmd.Connection = dbConn;
                                        cmd.CommandText = cmdString;
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (Exception Ex)
                                    { }
                                    cmd.Dispose();
                                }
                                //关闭数据库的连接
                                dbConn.Close();
                                dbConn.Dispose();

                                log.Info(DateTime.Now.ToString() + "Annexed Leveling Reporting" + sender.ToString() + e.ToString());//写入一条新log
                            }
                            break;

                        case 2: //支水准
                            {
                                //连接access数据库
                                OleDbConnection dbConn = new OleDbConnection("Provider = Microsoft.ACE.OLEDB.12.0 ; Data Source = Adjustment.accdb; Jet OLEDB:Database Password=lmzl123456789");
                                if (dbConn.State != ConnectionState.Open) dbConn.Open();
                                else
                                {
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("Aurora数据库已经被其他程序占用，请关闭先。", "智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("Aurora資料庫已經被其他程式佔用，請關閉先。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Aurora database is occupied by some else program, Close them first.", "Aurora Intelligent Tips");
                                    }
                                }

                                OleDbCommand cmd = new OleDbCommand("DELETE from SpurLeveling", dbConn);      //首先清空表里面的数据
                                cmd.ExecuteNonQuery();
                                OleDbCommand cmd1 = new OleDbCommand("DELETE from CommonFields", dbConn);
                                cmd1.ExecuteNonQuery();

                                string strSumLength = textBox6.Text;
                                try
                                {
                                    string cmdString = "INSERT INTO CommonFields([ID],[SumStations],[SumLength],[ProjectName],[MyGrade],[Calculator],[Checker]) VALUES('1','" + pSumStations + "','" + strSumLength + "','" + strProjectName + "','" + strMyGrade + "','" + strCalculator + "','" + strChecker + "')";

                                    cmd1 = new OleDbCommand();
                                    cmd1.Connection = dbConn;
                                    cmd1.CommandText = cmdString;
                                    cmd1.ExecuteNonQuery();
                                }
                                catch (Exception Ex)
                                { }

                                foreach (ListViewItem lvi in AdjustListView.Items)
                                {
                                    string strID = lvi.SubItems[0].Text;
                                    string strPTName = lvi.SubItems[1].Text;
                                    string strDist = lvi.SubItems[2].Text;
                                    string strStations = lvi.SubItems[3].Text;
                                    string strObsLevelDiff = lvi.SubItems[4].Text;
                                    string strHeight = lvi.SubItems[5].Text;

                                    try
                                    {
                                        //object oID = strID;
                                        //object oPTName = strPTName;
                                        //object oDist = strDist;
                                        //object oStations = strStations;
                                        //object oObsLevelDiff = strObsLevelDiff;
                                        //object oHeight = strHeight;

                                        string cmdString = "INSERT INTO SpurLeveling([ID],[PTName],[Dist],[Stations],[ObsLevelDiff],[Height]) VALUES('" + strID + "','" + strPTName + "','" + strDist + "','" + strStations + "','" + strObsLevelDiff + "','" + strHeight + "')";

                                        cmd = new OleDbCommand();
                                        cmd.Connection = dbConn;
                                        cmd.CommandText = cmdString;
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (Exception Ex)
                                    { }
                                    cmd.Dispose();
                                }
                                //关闭数据库的连接
                                dbConn.Close();
                                dbConn.Dispose();

                                log.Info(DateTime.Now.ToString() + "Spur Leveling Reporting" + sender.ToString() + e.ToString());//写入一条新log
                            }
                            break;

                        case 3: //闭合导线
                            {
                                //连接access数据库
                                OleDbConnection dbConn = new OleDbConnection("Provider = Microsoft.ACE.OLEDB.12.0 ; Data Source = Adjustment.accdb; Jet OLEDB:Database Password=lmzl123456789");
                                if (dbConn.State != ConnectionState.Open) dbConn.Open();
                                else
                                {
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("Aurora数据库已经被其他程序占用，请关闭先。", "智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("Aurora資料庫已經被其他程式佔用，請關閉先。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Aurora database is occupied by some else program, Close them first.", "Aurora Intelligent Tips");
                                    }
                                }

                                OleDbCommand cmd = new OleDbCommand("DELETE FROM [ClosedTraverse]", dbConn);      //首先清空表里面的数据
                                cmd.ExecuteNonQuery();
                                OleDbCommand cmd1 = new OleDbCommand("DELETE FROM CommonFields", dbConn);
                                cmd1.ExecuteNonQuery();

                                string strfb = textBox5.Text;
                                string strfr = textBox2.Text;
                                string strfx = textBox3.Text;
                                string strfy = textBox7.Text;
                                string strf = textBox4.Text;
                                string strK = textBox8.Text;
                                string strSumLength = textBox6.Text;
                                try
                                {
                                    string cmdString = "INSERT INTO CommonFields([ID],[fr],[fb],[SumStations],[SumLength],[SumAdjust],[SumAdjustLevelDiff],[SumObsAngle],[SumAdjObsAngle],[fx],[fy],[SumAdjDeltaX],[SumAdjDeltaY],[f],[K],[ProjectName],[MyGrade],[Calculator],[Checker]) VALUES('1','" + strfr + "','" + strfb + "','" + pSumStations + "','" + strSumLength + "','" + pSumAdjust + "','" + pSumAdjustLevelDiff + "','" + pSumObsAngle + "','" + pSumAdjObsAngle + "','" + strfx + "','" + strfy + "','" + pSumAdjDeltaX + "','" + pSumAdjDeltaY + "','" + strf + "','" + strK + "','" + strProjectName + "','" + strMyGrade + "','" + strCalculator + "','" + strChecker + "')";

                                    cmd1 = new OleDbCommand();
                                    cmd1.Connection = dbConn;
                                    cmd1.CommandText = cmdString;
                                    cmd1.ExecuteNonQuery();
                                }
                                catch (Exception Ex)
                                { }

                                foreach (ListViewItem lvi in AdjustListView.Items)
                                {
                                    string strID = lvi.SubItems[0].Text;
                                    string strStation = lvi.SubItems[1].Text;
                                    string strObsAngle = lvi.SubItems[2].Text;
                                    string strAdjust = lvi.SubItems[3].Text;
                                    string strAdjObsAngle = lvi.SubItems[4].Text;
                                    string strAzimuth = lvi.SubItems[5].Text;
                                    string strLength = lvi.SubItems[6].Text;
                                    string strDeltaX = lvi.SubItems[7].Text;
                                    string strDeltaY = lvi.SubItems[8].Text;
                                    string strAdjDeltaX = lvi.SubItems[9].Text;
                                    string strAdjDeltaY = lvi.SubItems[10].Text;
                                    string strCoorX = lvi.SubItems[11].Text;
                                    string strCoorY = lvi.SubItems[12].Text;

                                    try
                                    {
                                        string cmdString = "INSERT INTO ClosedTraverse([ID],[Station],[ObsAngle],[Adjust],[AdjObsAngle],[Azimuth],[Length],[DeltaX],[DeltaY],[AdjDeltaX],[AdjDeltaY],[CoorX],[CoorY]) VALUES('" + strID + "','" + strStation + "','" + strObsAngle + "','" + strAdjust + "','" + strAdjObsAngle + "','" + strAzimuth + "','" + strLength + "','" + strDeltaX + "','" + strDeltaY + "','" + strAdjDeltaX + "','" + strAdjDeltaY + "','" + strCoorX + "','" + strCoorY + "')";

                                        cmd = new OleDbCommand();
                                        cmd.Connection = dbConn;
                                        cmd.CommandText = cmdString;
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (Exception Ex)
                                    { }
                                    cmd.Dispose();
                                }
                                //关闭数据库的连接
                                dbConn.Close();
                                dbConn.Dispose();

                                log.Info(DateTime.Now.ToString() + "Closed Traverse Reporting" + sender.ToString() + e.ToString());//写入一条新log
                            }
                            break;

                        case 4: //闭合导线(含外支点)
                            {
                                //连接access数据库
                                OleDbConnection dbConn = new OleDbConnection("Provider = Microsoft.ACE.OLEDB.12.0 ; Data Source = Adjustment.accdb; Jet OLEDB:Database Password=lmzl123456789");
                                if (dbConn.State != ConnectionState.Open) dbConn.Open();
                                else
                                {
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("Aurora数据库已经被其他程序占用，请关闭先。", "智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("Aurora資料庫已經被其他程式佔用，請關閉先。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Aurora database is occupied by some else program, Close them first.", "Aurora Intelligent Tips");
                                    }
                                }

                                OleDbCommand cmd = new OleDbCommand("DELETE from [ClosedTraverseWithOuterPoint]", dbConn);      //首先清空表里面的数据
                                cmd.ExecuteNonQuery();
                                OleDbCommand cmd1 = new OleDbCommand("DELETE from CommonFields", dbConn);
                                cmd1.ExecuteNonQuery();

                                string strfb = textBox5.Text;
                                string strfr = textBox2.Text;
                                string strfx = textBox3.Text;
                                string strfy = textBox7.Text;
                                string strf = textBox4.Text;
                                string strK = textBox8.Text;
                                string strSumLength = textBox6.Text;
                                try
                                {
                                    string cmdString = "INSERT INTO CommonFields([ID],[fr],[fb],[SumStations],[SumLength],[SumAdjust],[SumAdjustLevelDiff],[SumObsAngle],[SumAdjObsAngle],[fx],[fy],[SumAdjDeltaX],[SumAdjDeltaY],[f],[K],[ProjectName],[MyGrade],[Calculator],[Checker]) VALUES('1','" + strfr + "','" + strfb + "','" + pSumStations + "','" + strSumLength + "','" + pSumAdjust + "','" + pSumAdjustLevelDiff + "','" + pSumObsAngle + "','" + pSumAdjObsAngle + "','" + strfx + "','" + strfy + "','" + pSumAdjDeltaX + "','" + pSumAdjDeltaY + "','" + strf + "','" + strK + "','" + strProjectName + "','" + strMyGrade + "','" + strCalculator + "','" + strChecker + "')";

                                    cmd1 = new OleDbCommand();
                                    cmd1.Connection = dbConn;
                                    cmd1.CommandText = cmdString;
                                    cmd1.ExecuteNonQuery();
                                }
                                catch (Exception Ex)
                                { }

                                foreach (ListViewItem lvi in AdjustListView.Items)
                                {
                                    string strID = lvi.SubItems[0].Text;
                                    string strStation = lvi.SubItems[1].Text;
                                    string strObsAngle = lvi.SubItems[2].Text;
                                    string strAdjust = lvi.SubItems[3].Text;
                                    string strAdjObsAngle = lvi.SubItems[4].Text;
                                    string strAzimuth = lvi.SubItems[5].Text;
                                    string strLength = lvi.SubItems[6].Text;
                                    string strDeltaX = lvi.SubItems[7].Text;
                                    string strDeltaY = lvi.SubItems[8].Text;
                                    string strAdjDeltaX = lvi.SubItems[9].Text;
                                    string strAdjDeltaY = lvi.SubItems[10].Text;
                                    string strCoorX = lvi.SubItems[11].Text;
                                    string strCoorY = lvi.SubItems[12].Text;

                                    try
                                    {
                                        string cmdString = "INSERT INTO [ClosedTraverseWithOuterPoint]([ID],[Station],[ObsAngle],[Adjust],[AdjObsAngle],[Azimuth],[Length],[DeltaX],[DeltaY],[AdjDeltaX],[AdjDeltaY],[CoorX],[CoorY]) VALUES('" + strID + "','" + strStation + "','" + strObsAngle + "','" + strAdjust + "','" + strAdjObsAngle + "','" + strAzimuth + "','" + strLength + "','" + strDeltaX + "','" + strDeltaY + "','" + strAdjDeltaX + "','" + strAdjDeltaY + "','" + strCoorX + "','" + strCoorY + "')";

                                        cmd = new OleDbCommand();
                                        cmd.Connection = dbConn;
                                        cmd.CommandText = cmdString;
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (Exception Ex)
                                    { }
                                    cmd.Dispose();
                                }
                                //关闭数据库的连接
                                dbConn.Close();
                                dbConn.Dispose();

                                log.Info(DateTime.Now.ToString() + "Closed Traverse With Outer Point Reporting" + sender.ToString() + e.ToString());//写入一条新log
                            }
                            break;

                        case 5: //具有一个连接角的附和导线
                            {
                                //连接access数据库
                                OleDbConnection dbConn = new OleDbConnection("Provider = Microsoft.ACE.OLEDB.12.0 ; Data Source = Adjustment.accdb; Jet OLEDB:Database Password=lmzl123456789");
                                if (dbConn.State != ConnectionState.Open) dbConn.Open();
                                else
                                {
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("Aurora数据库已经被其他程序占用，请关闭先。", "智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("Aurora資料庫已經被其他程式佔用，請關閉先。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Aurora database is occupied by some else program, Close them first.", "Aurora Intelligent Tips");
                                    }
                                }

                                OleDbCommand cmd = new OleDbCommand("DELETE from OneAngleConnTraverse", dbConn);      //首先清空表里面的数据
                                cmd.ExecuteNonQuery();
                                OleDbCommand cmd1 = new OleDbCommand("DELETE from CommonFields", dbConn);
                                cmd1.ExecuteNonQuery();

                                string strfx = textBox3.Text;
                                string strfy = textBox7.Text;
                                string strf = textBox4.Text;
                                string strK = textBox8.Text;
                                string strSumLength = textBox6.Text;
                                try
                                {
                                    string cmdString = "INSERT INTO CommonFields([ID],[SumStations],[SumLength],[SumAdjust],[SumAdjustLevelDiff],[SumObsAngle],[SumAdjObsAngle],[fx],[fy],[SumAdjDeltaX],[SumAdjDeltaY],[f],[K],[ProjectName],[MyGrade],[Calculator],[Checker]) VALUES('1','" + pSumStations + "','" + strSumLength + "','" + pSumAdjust + "','" + pSumAdjustLevelDiff + "','" + pSumObsAngle + "','" + pSumAdjObsAngle + "','" + strfx + "','" + strfy + "','" + pSumAdjDeltaX + "','" + pSumAdjDeltaY + "','" + strf + "','" + strK + "','" + strProjectName + "','" + strMyGrade + "','" + strCalculator + "','" + strChecker + "')";

                                    cmd1 = new OleDbCommand();
                                    cmd1.Connection = dbConn;
                                    cmd1.CommandText = cmdString;
                                    cmd1.ExecuteNonQuery();
                                }
                                catch (Exception Ex)
                                { }

                                foreach (ListViewItem lvi in AdjustListView.Items)
                                {
                                    string strID = lvi.SubItems[0].Text;
                                    string strStation = lvi.SubItems[1].Text;
                                    string strObsAngle = lvi.SubItems[2].Text;
                                    string strAzimuth = lvi.SubItems[3].Text;
                                    string strLength = lvi.SubItems[4].Text;
                                    string strDeltaX = lvi.SubItems[5].Text;
                                    string strDeltaY = lvi.SubItems[6].Text;
                                    string strAdjDeltaX = lvi.SubItems[7].Text;
                                    string strAdjDeltaY = lvi.SubItems[8].Text;
                                    string strCoorX = lvi.SubItems[9].Text;
                                    string strCoorY = lvi.SubItems[10].Text;

                                    try
                                    {
                                        string cmdString = "INSERT INTO OneAngleConnTraverse([ID],[Station],[ObsAngle],[Azimuth],[Length],[DeltaX],[DeltaY],[AdjDeltaX],[AdjDeltaY],[CoorX],[CoorY]) VALUES('" + strID + "','" + strStation + "','" + strObsAngle + "','" + strAzimuth + "','" + strLength + "','" + strDeltaX + "','" + strDeltaY + "','" + strAdjDeltaX + "','" + strAdjDeltaY + "','" + strCoorX + "','" + strCoorY + "')";

                                        cmd = new OleDbCommand();
                                        cmd.Connection = dbConn;
                                        cmd.CommandText = cmdString;
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (Exception Ex)
                                    { }
                                    cmd.Dispose();
                                }
                                //关闭数据库的连接
                                dbConn.Close();
                                dbConn.Dispose();

                                log.Info(DateTime.Now.ToString() + "One Angle Conn Traverse Reporting" + sender.ToString() + e.ToString());//写入一条新log
                            }
                            break;

                        case 6: //具有两个连接角的附和导线
                            {
                                //连接access数据库
                                OleDbConnection dbConn = new OleDbConnection("Provider = Microsoft.ACE.OLEDB.12.0 ; Data Source = Adjustment.accdb; Jet OLEDB:Database Password=lmzl123456789");
                                if (dbConn.State != ConnectionState.Open) dbConn.Open();
                                else
                                {
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("Aurora数据库已经被其他程序占用，请关闭先。", "智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("Aurora資料庫已經被其他程式佔用，請關閉先。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Aurora database is occupied by some else program, Close them first.", "Aurora Intelligent Tips");
                                    }
                                }

                                OleDbCommand cmd = new OleDbCommand("DELETE from TwoAngleConnTraverse", dbConn);      //首先清空表里面的数据
                                cmd.ExecuteNonQuery();
                                OleDbCommand cmd1 = new OleDbCommand("DELETE from CommonFields", dbConn);
                                cmd1.ExecuteNonQuery();

                                string strfb = textBox5.Text;
                                string strfr = textBox2.Text;
                                string strfx = textBox3.Text;
                                string strfy = textBox7.Text;
                                string strf = textBox4.Text;
                                string strK = textBox8.Text;
                                string strSumLength = textBox6.Text;
                                try
                                {
                                    string cmdString = "INSERT INTO CommonFields([ID],[fr],[fb],[SumStations],[SumLength],[SumAdjust],[SumAdjustLevelDiff],[SumObsAngle],[SumAdjObsAngle],[fx],[fy],[SumAdjDeltaX],[SumAdjDeltaY],[f],[K],[ProjectName],[MyGrade],[Calculator],[Checker]) VALUES('1','" + strfr + "','" + strfb + "','" + pSumStations + "','" + strSumLength + "','" + pSumAdjust + "','" + pSumAdjustLevelDiff + "','" + pSumObsAngle + "','" + pSumAdjObsAngle + "','" + strfx + "','" + strfy + "','" + pSumAdjDeltaX + "','" + pSumAdjDeltaY + "','" + strf + "','" + strK + "','" + strProjectName + "','" + strMyGrade + "','" + strCalculator + "','" + strChecker + "')";

                                    cmd1 = new OleDbCommand();
                                    cmd1.Connection = dbConn;
                                    cmd1.CommandText = cmdString;
                                    cmd1.ExecuteNonQuery();
                                }
                                catch (Exception Ex)
                                { }

                                foreach (ListViewItem lvi in AdjustListView.Items)
                                {
                                    string strID = lvi.SubItems[0].Text;
                                    string strStation = lvi.SubItems[1].Text;
                                    string strObsAngle = lvi.SubItems[2].Text;
                                    string strAdjust = lvi.SubItems[3].Text;
                                    string strAdjObsAngle = lvi.SubItems[4].Text;
                                    string strAzimuth = lvi.SubItems[5].Text;
                                    string strLength = lvi.SubItems[6].Text;
                                    string strDeltaX = lvi.SubItems[7].Text;
                                    string strDeltaY = lvi.SubItems[8].Text;
                                    string strAdjDeltaX = lvi.SubItems[9].Text;
                                    string strAdjDeltaY = lvi.SubItems[10].Text;
                                    string strCoorX = lvi.SubItems[11].Text;
                                    string strCoorY = lvi.SubItems[12].Text;

                                    try
                                    {
                                        string cmdString = "INSERT INTO TwoAngleConnTraverse([ID],[Station],[ObsAngle],[Adjust],[AdjObsAngle],[Azimuth],[Length],[DeltaX],[DeltaY],[AdjDeltaX],[AdjDeltaY],[CoorX],[CoorY]) VALUES('" + strID + "','" + strStation + "','" + strObsAngle + "','" + strAdjust + "','" + strAdjObsAngle + "','" + strAzimuth + "','" + strLength + "','" + strDeltaX + "','" + strDeltaY + "','" + strAdjDeltaX + "','" + strAdjDeltaY + "','" + strCoorX + "','" + strCoorY + "')";

                                        cmd = new OleDbCommand();
                                        cmd.Connection = dbConn;
                                        cmd.CommandText = cmdString;
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (Exception Ex)
                                    { }
                                    cmd.Dispose();
                                }
                                //关闭数据库的连接
                                dbConn.Close();
                                dbConn.Dispose();

                                log.Info(DateTime.Now.ToString() + "Two Angle Conn Traverse Reporting" + sender.ToString() + e.ToString());//写入一条新log
                            }
                            break;

                        case 7: //支导线
                            {
                                //连接access数据库
                                OleDbConnection dbConn = new OleDbConnection("Provider = Microsoft.ACE.OLEDB.12.0 ; Data Source = Adjustment.accdb; Jet OLEDB:Database Password=lmzl123456789");
                                if (dbConn.State != ConnectionState.Open) dbConn.Open();
                                else
                                {
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("Aurora数据库已经被其他程序占用，请关闭先。", "智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("Aurora資料庫已經被其他程式佔用，請關閉先。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Aurora database is occupied by some else program, Close them first.", "Aurora Intelligent Tips");
                                    }
                                }

                                OleDbCommand cmd = new OleDbCommand("DELETE from OpenTraverse", dbConn);      //首先清空表里面的数据
                                cmd.ExecuteNonQuery();
                                OleDbCommand cmd1 = new OleDbCommand("DELETE from CommonFields", dbConn);
                                cmd1.ExecuteNonQuery();

                                string strSumLength = textBox6.Text;
                                try
                                {
                                    string cmdString = "INSERT INTO CommonFields([ID],[SumStations],[SumLength],[SumAdjust],[SumAdjustLevelDiff],[SumObsAngle],[SumAdjObsAngle],[fx],[fy],[SumAdjDeltaX],[SumAdjDeltaY],[f],[K],[ProjectName],[MyGrade],[Calculator],[Checker]) VALUES('1','" + pSumStations + "','" + strSumLength + "','" + pSumAdjust + "','" + pSumAdjustLevelDiff + "','" + pSumObsAngle + "','" + pSumAdjObsAngle + "','" + pfx + "','" + pfy + "','" + pSumAdjDeltaX + "','" + pSumAdjDeltaY + "','" + pf + "','" + pK + "','" + strProjectName + "','" + strMyGrade + "','" + strCalculator + "','" + strChecker + "')";

                                    cmd1 = new OleDbCommand();
                                    cmd1.Connection = dbConn;
                                    cmd1.CommandText = cmdString;
                                    cmd1.ExecuteNonQuery();
                                }
                                catch (Exception Ex)
                                { }

                                foreach (ListViewItem lvi in AdjustListView.Items)
                                {
                                    string strID = lvi.SubItems[0].Text;
                                    string strStation = lvi.SubItems[1].Text;
                                    string strObsAngle = lvi.SubItems[2].Text;
                                    string strAzimuth = lvi.SubItems[3].Text;
                                    string strLength = lvi.SubItems[4].Text;
                                    string strDeltaX = lvi.SubItems[5].Text;
                                    string strDeltaY = lvi.SubItems[6].Text;
                                    string strCoorX = lvi.SubItems[7].Text;
                                    string strCoorY = lvi.SubItems[8].Text;

                                    try
                                    {
                                        string cmdString = "INSERT INTO OpenTraverse([ID],[Station],[ObsAngle],[Azimuth],[Length],[DeltaX],[DeltaY],[CoorX],[CoorY]) VALUES('" + strID + "','" + strStation + "','" + strObsAngle + "','" + strAzimuth + "','" + strLength + "','" + strDeltaX + "','" + strDeltaY + "','" + strCoorX + "','" + strCoorY + "')";

                                        cmd = new OleDbCommand();
                                        cmd.Connection = dbConn;
                                        cmd.CommandText = cmdString;
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (Exception Ex)
                                    { }
                                    cmd.Dispose();
                                }
                                //关闭数据库的连接
                                dbConn.Close();
                                dbConn.Dispose();

                                log.Info(DateTime.Now.ToString() + "Open Traverse Reporting" + sender.ToString() + e.ToString());//写入一条新log
                            }
                            break;

                        case 8: //无连接角导线
                            {
                                //连接access数据库
                                OleDbConnection dbConn = new OleDbConnection("Provider = Microsoft.ACE.OLEDB.12.0 ; Data Source = Adjustment.accdb; Jet OLEDB:Database Password=lmzl123456789");
                                if (dbConn.State != ConnectionState.Open) dbConn.Open();
                                else
                                {
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("Aurora数据库已经被其他程序占用，请关闭先。", "智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("Aurora資料庫已經被其他程式佔用，請關閉先。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Aurora database is occupied by some else program, Close them first.", "Aurora Intelligent Tips");
                                    }
                                }

                                OleDbCommand cmd = new OleDbCommand("DELETE from NoAngleConnTraverse", dbConn);      //首先清空表里面的数据
                                cmd.ExecuteNonQuery();
                                OleDbCommand cmd1 = new OleDbCommand("DELETE from CommonFields", dbConn);
                                cmd1.ExecuteNonQuery();

                                string strSumLength = textBox6.Text;
                                string strK = textBox8.Text;
                                try
                                {
                                    string cmdString = "INSERT INTO CommonFields([ID],[fb],[SumStations],[SumLength],[SumAdjust],[SumAdjustLevelDiff],[SumObsAngle],[SumAdjObsAngle],[fx],[fy],[SumAdjDeltaX],[SumAdjDeltaY],[f],[K],[ProjectName],[MyGrade],[Calculator],[Checker]) VALUES('1','" + pfb + "','" + pSumStations + "','" + strSumLength + "','" + pSumAdjust + "','" + pSumAdjustLevelDiff + "','" + pSumObsAngle + "','" + pSumAdjObsAngle + "','" + pfx + "','" + pfy + "','" + pSumAdjDeltaX + "','" + pSumAdjDeltaY + "','" + pf + "','" + pK + "','" + strProjectName + "','" + strMyGrade + "','" + strCalculator + "','" + strChecker + "')";

                                    cmd1 = new OleDbCommand();
                                    cmd1.Connection = dbConn;
                                    cmd1.CommandText = cmdString;
                                    cmd1.ExecuteNonQuery();
                                }
                                catch (Exception Ex)
                                { }

                                foreach (ListViewItem lvi in AdjustListView.Items)
                                {
                                    string strID = lvi.SubItems[0].Text;
                                    string strStation = lvi.SubItems[1].Text;
                                    string strObsAngle = lvi.SubItems[2].Text;
                                    string strLength = lvi.SubItems[3].Text;
                                    string strAssumedAzimuth = lvi.SubItems[4].Text;
                                    string strAssumedDeltaX = lvi.SubItems[5].Text;
                                    string strAssumedDeltaY = lvi.SubItems[6].Text;
                                    string strAssumedCoorX = lvi.SubItems[7].Text;
                                    string strAssumedCoorY = lvi.SubItems[8].Text;
                                    string strCoorX = lvi.SubItems[9].Text;
                                    string strCoorY = lvi.SubItems[10].Text;

                                    try
                                    {
                                        string cmdString = "INSERT INTO NoAngleConnTraverse([ID],[Station],[ObsAngle],[Length],[AssumedAzimuth],[AssumedDeltaX],[AssumedDeltaY],[AssumedCoorX],[AssumedCoorY],[CoorX],[CoorY]) VALUES('" + strID + "','" + strStation + "','" + strObsAngle + "','" + strLength + "','" + strAssumedAzimuth + "','" + strAssumedDeltaX + "','" + strAssumedDeltaY + "','" + strAssumedCoorX + "','" + strAssumedCoorY + "','" + strCoorX + "','" + strCoorY + "')";

                                        cmd = new OleDbCommand();
                                        cmd.Connection = dbConn;
                                        cmd.CommandText = cmdString;
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (Exception Ex)
                                    { }
                                    cmd.Dispose();
                                }
                                //关闭数据库的连接
                                dbConn.Close();
                                dbConn.Dispose();

                                log.Info(DateTime.Now.ToString() + "No Angle Conn Traverse Reporting" + sender.ToString() + e.ToString());//写入一条新log
                            }
                            break;

                        case 9: //水准网
                            {
                                OleDbConnection dbConn = new OleDbConnection("Provider = Microsoft.ACE.OLEDB.12.0 ; Data Source = Adjustment.accdb; Persist Security Info=False");
                                if (dbConn.State != ConnectionState.Open) dbConn.Open();
                                else
                                {
                                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                                    {
                                        MessageBox.Show("Aurora数据库已经被其他程序占用，请关闭先。", "智能提示");
                                    }
                                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                                    {
                                        MessageBox.Show("Aurora資料庫已經被其他程式佔用，請關閉先。", "Aurora智慧提示");
                                    }
                                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                                    {
                                        MessageBox.Show("Aurora database is occupied by some else program, Close them first.", "Aurora Intelligent Tips");
                                    }
                                }

                                OleDbCommand cmd = new OleDbCommand("DELETE from CommonFields", dbConn);
                                cmd.ExecuteNonQuery();
                                OleDbCommand cmd1 = new OleDbCommand("DELETE from LevelingNetwork1", dbConn);      //首先清空表里面的数据
                                cmd1.ExecuteNonQuery();
                                OleDbCommand cmd2 = new OleDbCommand("DELETE from LevelingNetwork2", dbConn);
                                cmd2.ExecuteNonQuery();
                                OleDbCommand cmd3 = new OleDbCommand("DELETE from LevelingNetwork3", dbConn);
                                cmd3.ExecuteNonQuery();
                                OleDbCommand cmd4 = new OleDbCommand("DELETE from LevelingNetwork4", dbConn);
                                cmd4.ExecuteNonQuery();
                                OleDbCommand cmd5 = new OleDbCommand("DELETE from LevelingNetwork5", dbConn);
                                cmd5.ExecuteNonQuery();

                                string strSumLength = textBox6.Text;
                                try
                                {
                                    string cmdString = "INSERT INTO CommonFields([ID],[fb],[SumStations],[SumLength],[SumAdjust],[SumAdjustLevelDiff],[SumObsAngle],[SumAdjObsAngle],[fx],[fy],[SumAdjDeltaX],[SumAdjDeltaY],[f],[K],[ProjectName],[MyGrade],[Calculator],[Checker]) VALUES('1','" + pfb + "','" + pSumStations + "','" + strSumLength + "','" + pSumAdjust + "','" + pSumAdjustLevelDiff + "','" + pSumObsAngle + "','" + pSumAdjObsAngle + "','" + pfx + "','" + pfy + "','" + pSumAdjDeltaX + "','" + pSumAdjDeltaY + "','" + pf + "','" + pK + "','" + strProjectName + "','" + strMyGrade + "','" + strCalculator + "','" + strChecker + "')";

                                    cmd1 = new OleDbCommand();
                                    cmd1.Connection = dbConn;
                                    cmd1.CommandText = cmdString;
                                    cmd1.ExecuteNonQuery();
                                }
                                catch (Exception Ex)
                                { }

                                for (int i = 0; i < m_Knumber; i++)             //写入已知点和已知高程
                                {
                                    try
                                    {
                                        string cmdString = "INSERT INTO LevelingNetwork5([ID],[KnownPT],[KnownHeight]) VALUES('" + (i + 1).ToString() + "','" + Pname[i].ToString() + "','" + KHeight[i].ToString() + "')";
                                        cmd5 = new OleDbCommand();
                                        cmd5.Connection = dbConn;
                                        cmd5.CommandText = cmdString;
                                        cmd5.ExecuteNonQuery();
                                    }
                                    catch (Exception Ex)
                                    { }
                                    cmd5.Dispose();
                                }

                                for (int i = 0; i < m_Tnumber; i++)             //写入高程平差值及其精度
                                {
                                    double qii = ATPA[ij(i, i)];
                                    s = Math.Sqrt(qii) * m_mu;

                                    try
                                    {
                                        string cmdString = "INSERT INTO LevelingNetwork2([ID],[PointName],[ApproHeight],[Delta],[AdjustHeight],[MeanError]) VALUES('" + (i + 1).ToString() + "','" + Pname[i].ToString() + "','" + (KHeight[i] - dX[i]).ToString("#0.0000") + "','" + dX[i].ToString("#0.0000") + "','" + KHeight[i].ToString("#0.0000") + "','" + s.ToString("#0.0000") + "')";
                                        cmd2 = new OleDbCommand();
                                        cmd2.Connection = dbConn;
                                        cmd2.CommandText = cmdString;
                                        cmd2.ExecuteNonQuery();
                                    }
                                    catch (Exception Ex)
                                    { }
                                    cmd2.Dispose();
                                }

                                for (int i = 0; i < m_Onumber; i++)             //写入观测值平差值及其精度
                                {
                                    ArrayList arr = new ArrayList(Pname);
                                    int k1 = arr.IndexOf(StartP[i]);  //高差起点号
                                    int k2 = arr.IndexOf(EndP[i]);  //高差起点号
                                    double qii = ATPA[ij(k1, k1)];
                                    double qjj = ATPA[ij(k2, k2)];
                                    double qij = ATPA[ij(k1, k2)];
                                    ml = Math.Sqrt(qii + qjj - 2.0 * qij) * m_mu;
                                    try
                                    {
                                        string cmdString = "INSERT INTO LevelingNetwork3([ID],[StartPoint],[EndPoint],[LevelDiff],[v],[AdjustLevelDiff],[ObsWeight],[MeanError]) VALUES('" + (i + 1).ToString() + "','" + StartP[i] + "','" + EndP[i] + "','" + L[i].ToString("#0.0000") + "','" + (V[i] * 1000).ToString("#0.0000") + "','" + (L[i] + V[i]).ToString("#0.0000") + "','" + P[i].ToString("#0.0000") + "','" + (ml * 1000).ToString("#0.0000") + "')";
                                        cmd3 = new OleDbCommand();
                                        cmd3.Connection = dbConn;
                                        cmd3.CommandText = cmdString;
                                        cmd3.ExecuteNonQuery();
                                    }
                                    catch (Exception Ex)
                                    { }
                                    cmd3.Dispose();
                                }

                                try         //写入观测值总数，总点数，已知点数，pvv，UnitWeightMeanError
                                {
                                    string strmu = "±" + (m_mu * 1000).ToString();
                                    string cmdString = "INSERT INTO LevelingNetwork4([ID],[Onumber],[Tnumber],[Knumber],[pvv],[UnitWeightMeanError]) VALUES('" + "1" + "','" + m_Onumber.ToString() + "','" + m_Tnumber.ToString() + "','" + m_Knumber.ToString() + "','" + m_pvv.ToString() + "','" + strmu + "')";
                                    cmd4 = new OleDbCommand();
                                    cmd4.Connection = dbConn;
                                    cmd4.CommandText = cmdString;
                                    cmd4.ExecuteNonQuery();
                                }
                                catch (Exception Ex)
                                { }
                                cmd4.Dispose();

                                foreach (ListViewItem lvi in AdjustListView.Items)           //写入观测数据
                                {
                                    string strID = lvi.SubItems[0].Text;                //最好当字符串处理。否则空的单元格转化为双精度时报错。
                                    string strStart = lvi.SubItems[1].Text;
                                    string strEnd = lvi.SubItems[2].Text;
                                    string strLevelDiff = lvi.SubItems[3].Text;
                                    string strHD = lvi.SubItems[4].Text;
                                    try
                                    {
                                        string cmdString = "INSERT INTO LevelingNetwork1([ID],[StartPoint],[EndPoint],[LevelDiff],[HD]) VALUES('" + strID + "','" + strStart + "','" + strEnd + "','" + strLevelDiff + "','" + strHD + "')";
                                        cmd1 = new OleDbCommand();
                                        cmd1.Connection = dbConn;
                                        cmd1.CommandText = cmdString;
                                        cmd1.ExecuteNonQuery();
                                    }
                                    catch (Exception Ex)
                                    { }
                                    cmd1.Dispose();
                                }

                                //关闭数据库的连接
                                dbConn.Close();
                                dbConn.Dispose();

                                log.Info(DateTime.Now.ToString() + "Leveling Network Reporting" + sender.ToString() + e.ToString());//写入一条新log
                            }
                            break;

                    }

                }
                catch (System.Exception ex)
                {
                    log.Info(DateTime.Now.ToString() + "Report Exception" + ex.ToString());//写入一条新log
                }

                //现在调用Reporting.exe
                //if (File.Exists(Application.StartupPath + "\\Reporting.exe"))
                //{
                //    System.Diagnostics.Process.Start(Application.StartupPath + "\\Reporting.exe");            //调用报表Reporting
                //}
                //else
                //{
                //    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                //    {
                //        MessageBox.Show("Aurora智能报表.exe丢失，请尝试重新安装Aurora。", "Aurora智能提示");
                //    }
                //    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                //    {
                //        MessageBox.Show("Aurora智慧報表.exe丟失，請嘗試重新安裝Aurora。", "Aurora智慧提示");
                //    }
                //    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                //    {
                //        MessageBox.Show("Aurora Reporting.exe is missing, Please try to reinstall Aurora.", "Aurora Intelligent Tips");
                //    }
                //}
            }
        }






    }
}
