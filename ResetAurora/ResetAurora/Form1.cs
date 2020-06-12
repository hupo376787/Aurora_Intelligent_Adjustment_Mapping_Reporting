using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;
using System.Management;
using System.Security.Cryptography;
using System.Configuration;

//加密密码
//商业版加密密码ilovetangwei
//学习版加密密码DateTime.Now.Date.ToString("yyyy-MM-dd") + "hupo"

namespace ResetAurora
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public string strRC = "";       //rc
        public string strEnctr = "";        //jia mi chuan
        public string strKC = "";       //kc    
        public string strInputKC = "";       //用于接收Form2的注册码,只有注册过的机器才可以重置.
        public int nFlag = 0;

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dr = DialogResult.Yes;
            dr = MessageBox.Show("确定要重置注册表?", "Aurora智能提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.No)
            {
                return;
            }

            strRC = CreateSNCode();
            strInputKC = textBox1.Text;
            
            if (checkBox1.Checked == true)
            {
                strEnctr = EnText(strRC, DateTime.Now.Date.ToString("yyyy-MM-dd") + "hupo");       //学生版
                if (transform(strEnctr, DateTime.Now.Date.ToString("yyyy-MM-dd") + "hupo") == strInputKC)
                {
                    //调用外部程序导cmd命令行,实际上没Diao用
                    Process p = new Process();
                    p.StartInfo.FileName = "cmd.exe";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.CreateNoWindow = false;
                    p.Start();
                    //向cmd.exe输入command 
                    p.StandardInput.WriteLine("ipconfig");
                    p.StandardInput.WriteLine("exit");     //需要有这句，不然程序会挂机

                    RegistryKey MyReg, RegGUIDType, RegType, RegValidGUIDDays, RegValidDays, RegSuccess, RegCodeAllow, RegStartGUIDDate, RegStartDate;//声明注册表对象
                    MyReg = Registry.CurrentUser;//获取当前用户注册表项
                    try
                    {
                        RegGUIDType = MyReg.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");//在注册表项中创建子项
                        RegType = MyReg.CreateSubKey("Software\\Aurora");

                        RegValidGUIDDays = MyReg.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                        RegValidDays = MyReg.CreateSubKey("Software\\Aurora");

                        RegCodeAllow = MyReg.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                        RegSuccess = MyReg.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                        RegStartGUIDDate = MyReg.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                        RegStartDate = MyReg.CreateSubKey("Software\\Aurora");

                        RegType.SetValue("nRegFlag", "0");
                        RegValidDays.SetValue("nValidDays", "10");             //重置为初始
                        RegGUIDType.SetValue("nRegGUIDFlag", "0");
                        RegValidGUIDDays.SetValue("nValidGUIDDays", "10");
                        RegCodeAllow.SetValue("RegCodeAllow", "1");
                        RegSuccess.SetValue("RegSuccess", "false");
                        RegStartGUIDDate.SetValue("StartGUIDDate", DateTime.Now.ToString("yyyy-MM-dd"));
                        RegStartDate.SetValue("StartDate", DateTime.Now.ToString("yyyy-MM-dd"));

                        label1.Visible = true;
                        label1.Text = "注册表信息已成功恢复初始设置，重启Aurora生效。";
                        label1.TextAlign = ContentAlignment.MiddleCenter;
                        label1.ForeColor = Color.LawnGreen;
                    }
                    catch { }
                }
                else
                {
                    label1.Visible = true;
                    label1.Text = "                        注册码输入错误。";
                    label1.TextAlign = ContentAlignment.MiddleCenter;
                    label1.ForeColor = Color.Red;
                }
            } 
            else
            {
                strEnctr = EnText(strRC, "ilovetangwei");       //商业版
                if (transform(strEnctr, "ilovetangwei") == strInputKC)
                {
                    //调用外部程序导cmd命令行，实际上没Diao用
                    Process p = new Process();
                    p.StartInfo.FileName = "cmd.exe";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.CreateNoWindow = false;
                    p.Start();
                    //向cmd.exe输入command 
                    p.StandardInput.WriteLine("ipconfig");
                    p.StandardInput.WriteLine("exit");     //需要有这句，不然程序会挂机

                    RegistryKey MyReg, RegGUIDType, RegType, RegValidGUIDDays, RegValidDays, RegSuccess, RegCodeAllow, RegStartGUIDDate, RegStartDate;//声明注册表对象
                    MyReg = Registry.CurrentUser;//获取当前用户注册表项
                    try
                    {
                        RegGUIDType = MyReg.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");//在注册表项中创建子项
                        RegType = MyReg.CreateSubKey("Software\\Aurora");

                        RegValidGUIDDays = MyReg.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                        RegValidDays = MyReg.CreateSubKey("Software\\Aurora");

                        RegCodeAllow = MyReg.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                        RegSuccess = MyReg.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                        RegStartGUIDDate = MyReg.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                        RegStartDate = MyReg.CreateSubKey("Software\\Aurora");

                        RegType.SetValue("nRegFlag", "0");
                        RegValidDays.SetValue("nValidDays", "10");             //重置为初始
                        RegGUIDType.SetValue("nRegGUIDFlag", "0");
                        RegValidGUIDDays.SetValue("nValidGUIDDays", "10");
                        RegCodeAllow.SetValue("RegCodeAllow", "1");
                        RegSuccess.SetValue("RegSuccess", "false");
                        RegStartGUIDDate.SetValue("StartGUIDDate", DateTime.Now.ToString("yyyy-MM-dd"));
                        RegStartDate.SetValue("StartDate", DateTime.Now.ToString("yyyy-MM-dd"));

                        label1.Visible = true;
                        label1.Text = "注册表信息已成功恢复初始设置，重启Aurora生效。";
                        label1.ForeColor = Color.LawnGreen;
                    }
                    catch { }
                }
                else
                {
                    label1.Visible = true;
                    label1.Text = "                        注册码输入错误。";
                    label1.ForeColor = Color.Red;
                }
            }

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

        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Visible = false;
        }


        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                textBox1.Text = "请在此处输入原注册码";
            }
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "请在此处输入原注册码")
            {
                textBox1.Text = "";
            }
        }

        #region 拖动图片移动窗体
        //鼠标拖动相关变量
        Point oldPoint = new Point(0, 0);
        bool mouseDown = false;

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            oldPoint = e.Location;
            mouseDown = true;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Left += (e.X - oldPoint.X);
                this.Top += (e.Y - oldPoint.Y);
            }
        }
        #endregion
    }
}
