using System;
using System.Text;
using System.Windows.Forms;
using System.Management;
using System.Security.Cryptography;
using Microsoft.Win32;
using System.Threading;

//加密密码
//商业版加密密码ilovetangwei               203
//学习版加密密码DateTime.Now.Date.ToString("yyyy-MM-dd") + "hupo"              101
//10元/1天版加密密码DateTime.Now.Date.ToString("yyyy-MM-dd") + "Lumia920"             920

namespace Adjustment
{
    public partial class RegisterAurora : Form
    {
        //Log4net
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);    

        public RegisterAurora()
        {
            InitializeComponent();
        }

        private void RegisterAurora_Load(object sender, EventArgs e)
        {
            textBox1.Text = CreateSNCode();
            checkBox1.Checked = true;
            checkBox1.Checked = false;          //一定要有

            RegistryKey MyReg0, RegFlag, RegKC;
            MyReg0 = Registry.CurrentUser;
            RegFlag = MyReg0.CreateSubKey("Software\\Aurora");
            RegKC = MyReg0.CreateSubKey("Software\\Aurora");
            if (RegFlag.GetValue("nRegFlag").ToString() != "0")
            {
                textBox2.Text = RegKC.GetValue("KC").ToString();
            }
        }

        private void RegisterAurora_Activated(object sender, EventArgs e)               //设置焦点
        {
            textBox2.Focus();
        }

        //获得CPU的序列号
        public string getCpu()
        {
            string strCpu = null;
            ManagementClass myCpu = new ManagementClass("win32_Processor");
            ManagementObjectCollection myCpuConnection = myCpu.GetInstances();
            foreach (ManagementObject myObject in myCpuConnection)
            {
                strCpu = myObject.Properties["Processorid"].Value.ToString();
                break;
            }
            return strCpu;
        }

        //取得设备硬盘的卷标号
        public string GetDiskVolumeSerialNumber()
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObject disk = new ManagementObject("win32_logicaldisk.deviceid=\"c:\"");
            disk.Get();
            return disk.GetPropertyValue("VolumeSerialNumber").ToString();
        }

        //生成机器码
        public string CreateSNCode()
        {
            string temp = getCpu() + GetDiskVolumeSerialNumber();//获得24位Cpu和硬盘序列号
            string[] strid = new string[24];//
            for (int i = 0; i < 24; i++)//把字符赋给数组
            {
                strid[i] = temp.Substring(i, 1);
            }
            temp = "";
            //Random rdid = new Random();
            for (int i = 0; i < 24; i++)//从数组随机抽取24个字符组成新的字符生成机器三
            {
                //temp += strid[rdid.Next(0, 24)];
                temp += strid[i + 3 >= 24 ? 0 : i + 3];
            }
            return GetMd5(temp);
        }

        public string GetMd5(object text)
        {
            string path = text.ToString();

            MD5CryptoServiceProvider MD5Pro = new MD5CryptoServiceProvider();
            Byte[] buffer = Encoding.GetEncoding("utf-8").GetBytes(text.ToString());
            Byte[] byteResult = MD5Pro.ComputeHash(buffer);

            string md5result = BitConverter.ToString(byteResult).Replace("-", "");
            return md5result;
        }

        //加密数据
        private string EnText(string Text, string sKey)
        {
            StringBuilder ret = new StringBuilder();
            try
            {
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                byte[] inputByteArray;
                inputByteArray = Encoding.Default.GetBytes(Text);
                //通过两次哈希密码设置对称算法的初始化向量   
                des.Key = ASCIIEncoding.ASCII.GetBytes(System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile
                (System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(sKey, "md5").Substring(0, 8), "sha1").Substring(0, 8));
                //通过两次哈希密码设置算法的机密密钥   
                des.IV = ASCIIEncoding.ASCII.GetBytes(System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile
                (System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(sKey, "md5").Substring(0, 8), "md5").Substring(0, 8));
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                foreach (byte b in ms.ToArray())
                {
                    ret.AppendFormat("{0:X2}", b);
                }
                return ret.ToString();
            }
            catch
            {
                return "";
            }
        }

        //解密数据
        private string DeText(string Text, string sKey)
        {
            try
            {
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();   //定义DES加密对象   
                int len;
                len = Text.Length / 2;
                byte[] inputByteArray = new byte[len];
                int x, i;
                for (x = 0; x < len; x++)
                {
                    i = Convert.ToInt32(Text.Substring(x * 2, 2), 16);
                    inputByteArray[x] = (byte)i;
                }
                //通过两次哈希密码设置对称算法的初始化向量   
                des.Key = ASCIIEncoding.ASCII.GetBytes(System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile
                (System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(sKey, "md5").Substring(0, 8), "sha1").Substring(0, 8));
                //通过两次哈希密码设置算法的机密密钥   
                des.IV = ASCIIEncoding.ASCII.GetBytes(System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile
                (System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(sKey, "md5").Substring(0, 8), "md5").Substring(0, 8));
                System.IO.MemoryStream ms = new System.IO.MemoryStream();//定义内存流
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);//定义加密流
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                return Encoding.Default.GetString(ms.ToArray());
            }
            catch
            {
                return "";
            }
        }

        //将加密的字符串转换为注册码形式
        public string transform(string input, string skey)
        {
            string transactSn = string.Empty;
            if (input == "")
            {
                return transactSn;
            }
            string initSn = string.Empty;
            try
            {
                initSn = this.EnText(this.EnText(input, skey), skey).ToString();
                transactSn = initSn.Substring(0, 5) + "-" + initSn.Substring(5, 5) +
                "-" + initSn.Substring(10, 5) + "-" + initSn.Substring(15, 5) +
                "-" + initSn.Substring(20, 5);
                return transactSn;
            }
            catch
            {
                return transactSn;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)               //计算出加密串
        {
            if (checkBox1.Checked == true)              //首先计算出加密串
            {
                textBox3.Text = EnText(textBox1.Text, DateTime.Now.Date.ToString("yyyy-MM-dd") + "hupo");
            }
            else textBox3.Text = EnText(textBox1.Text, "ilovetangwei");
        }

        private void button1_Click(object sender, EventArgs e)              //注册,学习版根据当天日期生成激活码，需要当天激活。商业版无限制。
        {
            if (textBox2.Text == "")
            {
                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                {
                    MessageBox.Show("请输入注册码！", "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                {
                    MessageBox.Show("請輸入註冊碼！", "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                {
                    MessageBox.Show("Please input key code！", "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }

            RegistryKey MyReg0, RegGUIDFlag, RegFlag, RegValidGUIDDays, RegValidDays, RegSuccess, RegStartGUIDDate, RegStartDate, RegKC;//声明注册表对象
            MyReg0 = Registry.CurrentUser;//获取当前用户注册表项

            try
            {
                RegGUIDFlag = MyReg0.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");//在注册表项中创建子项
                RegFlag = MyReg0.CreateSubKey("Software\\Aurora");

                RegValidGUIDDays = MyReg0.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                RegValidDays = MyReg0.CreateSubKey("Software\\Aurora");

                RegSuccess = MyReg0.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                RegStartGUIDDate = MyReg0.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                RegStartDate = MyReg0.CreateSubKey("Software\\Aurora");

                RegKC = MyReg0.CreateSubKey("Software\\Aurora");

                if (checkBox1.Checked == true)
                {
                    if (transform(textBox3.Text, DateTime.Now.Date.ToString("yyyy-MM-dd") + "hupo") == textBox2.Text) //判断转换后的加密串 == 输入的注册码？     【学习版】
                    {
                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            AutoClosingMessageBox.Show("学习版注册成功，请重新启动Aurora。", "Aurora智能提示", 5000);
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            AutoClosingMessageBox.Show("學習版註冊成功，請重新啟動Aurora。", "Aurora智慧提示", 5000);
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            AutoClosingMessageBox.Show("Study version register success, restart Aurora again.", "Aurora Intelligent Tips", 5000);
                        }

                        log.Info(DateTime.Now.ToString() + "Register Stu Aurora" + sender.ToString() + e.ToString());//写入一条新log
                        //textBox2.ReadOnly = true;

                        RegFlag.SetValue("nRegFlag", "101");
                        RegValidDays.SetValue("nValidDays", "180"); //设置学习版天数180。
                        RegGUIDFlag.SetValue("nRegGUIDFlag", "101");
                        RegValidGUIDDays.SetValue("nValidGUIDDays", "180");
                        RegSuccess.SetValue("RegSuccess", "true");
                        RegStartGUIDDate.SetValue("StartGUIDDate", DateTime.Now.ToString("yyyy-MM-dd"));
                        RegStartDate.SetValue("StartDate", DateTime.Now.ToString("yyyy-MM-dd"));
                        RegKC.SetValue("KC", textBox2.Text);

                        this.Close();

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
                    else
                    {
                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            MessageBox.Show("注册失败！请重新尝试新的激活码。", "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            MessageBox.Show("註冊失敗！請重新嘗試新的啟動碼。", "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            MessageBox.Show("Register failed！Please try again.", "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        //Application.Exit();
                    }
                }
                else 
                {
                    if (transform(textBox3.Text, "ilovetangwei") == textBox2.Text) //判断转换后的加密串 == 输入的注册码？     【商业版】
                    {
                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            AutoClosingMessageBox.Show("商业版注册成功，请重新启动Aurora。", "Aurora智能提示", 5000);
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            AutoClosingMessageBox.Show("商業版註冊成功，請重新啟動Aurora。", "Aurora智慧提示", 5000);
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            AutoClosingMessageBox.Show("Commercial version register success, restart Aurora again.", "Aurora Intelligent Tips", 5000);
                        }

                        log.Info(DateTime.Now.ToString() + "Register Com Aurora" + sender.ToString() + e.ToString());//写入一条新log
                        //textBox2.ReadOnly = true;

                        RegFlag.SetValue("nRegFlag", "203");
                        RegValidDays.SetValue("nValidDays", "9999");
                        RegGUIDFlag.SetValue("nRegGUIDFlag", "203");
                        RegValidGUIDDays.SetValue("nValidGUIDDays", "9999");
                        RegSuccess.SetValue("RegSuccess", "true");
                        RegStartGUIDDate.SetValue("StartGUIDDate", DateTime.Now.ToString("yyyy-MM-dd"));
                        RegStartDate.SetValue("StartDate", DateTime.Now.ToString("yyyy-MM-dd"));
                        RegKC.SetValue("KC", textBox2.Text);

                        this.Close();

                        RegistryKey MyReg1, RegReminder;
                        MyReg1 = Registry.CurrentUser;
                        RegReminder = MyReg1.CreateSubKey("Software\\Aurora\\Reminder");
                        try
                        {
                            RegReminder.SetValue("ExitReminder", "NO"); //不弹出确认关闭的对话框，防止异常。
                        }
                        catch { }


                        Application.Exit();

                    }
                    else
                    {
                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            MessageBox.Show("注册失败！请重新尝试新的激活码。", "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            MessageBox.Show("註冊失敗！請重新嘗試新的啟動碼。", "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            MessageBox.Show("Register failed！Please try again.", "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        //Application.Exit();
                    }
                }
            }
            catch
            {
                //Application.Exit();
            }

        }

        private void RegisterAurora_FormClosed(object sender, FormClosedEventArgs e)                //判断何种情况下退出程序.
        {
            RegistryKey MyReg0, RegGUIDFlag;
            MyReg0 = Registry.CurrentUser;
            try
            {
                RegGUIDFlag = MyReg0.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                if ((PublicClass.AuroraMain.CloseFormFlag != 0) && (RegGUIDFlag.GetValue("nRegGUIDFlag").ToString() == "0" || RegGUIDFlag.GetValue("nRegGUIDFlag").ToString() == "101" || RegGUIDFlag.GetValue("nRegGUIDFlag").ToString() == "920"))
                {
                    Application.Exit();
                    return;
                }
            }
            catch { }
        }

        private void button2_Click(object sender, EventArgs e)              //申请注册码
        {
            //this.Close();
            Mail FrmMail = new Mail();
            FrmMail.StartPosition = FormStartPosition.CenterScreen;
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                FrmMail.textBox2.Text = "【申请注册码】";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                FrmMail.textBox2.Text = "【申請註冊碼】";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                FrmMail.textBox2.Text = "【Apply for KC】";
            }
            
            FrmMail.Show(this);
        }

        private void button3_MouseEnter(object sender, EventArgs e)             //注册10元/1天版  提示
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            string Tips = "";
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                Tips = "注册后可以使用1天，过期后需重新购买注册。";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                Tips = "註冊後可以使用1天，過期後需重新購買註冊。";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")   
            {
                Tips = "Validate for 1 Days with 1.5$.";
            }
            p.SetToolTip(this.button3, Tips);
        }

        private void button3_Click(object sender, EventArgs e)             //注册10元/1天版
        {
            textBox3.Text = EnText(textBox1.Text, DateTime.Now.Date.ToString("yyyy-MM-dd") + "Lumia920");           //首先计算加密串

            if (textBox2.Text == "")
            {
                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                {
                    MessageBox.Show("请输入注册码！", "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                {
                    MessageBox.Show("請輸入註冊碼！", "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                {
                    MessageBox.Show("Please input key code！", "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }

            RegistryKey MyReg0, RegGUIDFlag, RegFlag, RegValidGUIDDays, RegValidDays, RegSuccess, RegStartGUIDDate, RegStartDate, RegKC;//声明注册表对象
            MyReg0 = Registry.CurrentUser;//获取当前用户注册表项

            try
            {
                RegGUIDFlag = MyReg0.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");//在注册表项中创建子项
                RegFlag = MyReg0.CreateSubKey("Software\\Aurora");

                RegValidGUIDDays = MyReg0.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                RegValidDays = MyReg0.CreateSubKey("Software\\Aurora");

                RegSuccess = MyReg0.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                RegStartGUIDDate = MyReg0.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                RegStartDate = MyReg0.CreateSubKey("Software\\Aurora");

                RegKC = MyReg0.CreateSubKey("Software\\Aurora");

                if (transform(textBox3.Text, DateTime.Now.Date.ToString("yyyy-MM-dd") + "Lumia920") == textBox2.Text) //判断转换后的加密串 == 输入的注册码？【10元/1天版】
                {
                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                    {
                        AutoClosingMessageBox.Show("1天版注册成功，请重新启动Aurora。", "Aurora智能提示", 5000);
                    }
                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                    {
                        AutoClosingMessageBox.Show("1天版註冊成功，請重新啟動Aurora。", "Aurora智慧提示", 5000);
                    }
                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                    {
                        AutoClosingMessageBox.Show("1 Day version register success, restart Aurora again.", "Aurora Intelligent Tips", 5000);
                    }


                    log.Info(DateTime.Now.ToString() + "Register Stu Aurora" + sender.ToString() + e.ToString());//写入一条新log
                    //textBox2.ReadOnly = true;

                    RegFlag.SetValue("nRegFlag", "920");
                    RegValidDays.SetValue("nValidDays", "1"); //设置10元/1天版天数1。
                    RegGUIDFlag.SetValue("nRegGUIDFlag", "920");
                    RegValidGUIDDays.SetValue("nValidGUIDDays", "1");
                    RegSuccess.SetValue("RegSuccess", "true");
                    RegStartGUIDDate.SetValue("StartGUIDDate", DateTime.Now.ToString("yyyy-MM-dd"));
                    RegStartDate.SetValue("StartDate", DateTime.Now.ToString("yyyy-MM-dd"));
                    RegKC.SetValue("KC", textBox2.Text);

                    this.Close();

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
                else
                {
                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                    {
                        MessageBox.Show("注册失败！请重新尝试新的激活码。", "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                    {
                        MessageBox.Show("註冊失敗！請重新嘗試新的啟動碼。", "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                    {
                        MessageBox.Show("Register failed！Please try again.", "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    //Application.Exit();
                }
            }
            catch
            {
                //Application.Exit();
            }
        }
    }
}
