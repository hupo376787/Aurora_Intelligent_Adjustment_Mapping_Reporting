using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Security.Cryptography;
using System.Configuration;

//加密密码
//商业版加密密码ilovetangwei               203
//学习版加密密码DateTime.Now.Date.ToString("yyyy-MM-dd") + "hupo"              101
//10元/1天版加密密码DateTime.Now.Date.ToString("yyyy-MM-dd") + "Lumia920"             920

namespace RegAurora
{
    public partial class RegMain : Form
    {
        public RegMain()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)              //商业版
        {
            string strEnCode = EnText(textBox_SN.Text, "ilovetangwei");
            string strRegCode = Transform(strEnCode, "ilovetangwei");
            string strOutputText = "您好，软件永久注册码：";
            strOutputText += strRegCode + "。" + "\r\n" + "如有任何疑问，请随时联系本邮箱。" + "\r\n" + "感谢关注Aurora。";
            textBox_KEY.Text = strOutputText;
            //textBox3.Text = EnText(textBox1.Text, "ilovetangwei");
            //textBox2.Text = Transform(textBox3.Text, "ilovetangwei");
        }

        private void button2_Click(object sender, EventArgs e)              //学习版
        {
            DateTime dDateToday = DateTime.Now.Date;
            string strToday = dDateToday.ToString("yyyy-MM-dd");
            string strEnCodeToday = EnText(textBox_SN.Text, strToday + "hupo");
            string strRegCodeToday = Transform(strEnCodeToday, strToday + "hupo");

            DateTime dDateTomorrow = DateTime.Now.Date.AddDays(1);
            string strTomorrow = dDateTomorrow.ToString("yyyy-MM-dd");
            string strEnCodeTomorrow = EnText(textBox_SN.Text, strTomorrow + "hupo");
            string strRegCodeTomorrow = Transform(strEnCodeTomorrow, strTomorrow + "hupo");

            string strOutputText = "您好，软件注册码：";
            strOutputText += strRegCodeToday + "。" + "\r\n" + "如果今天来不及注册，明天也可以，注册码为：";
            strOutputText += strRegCodeTomorrow + "。" + "\r\n" + "学习版禁止用于商业用途。\r\n感谢关注Aurora。";
            textBox_KEY.Text = strOutputText;
            //textBox3.Text = EnText(textBox1.Text, DateTime.Now.Date.ToString("yyyy-MM-dd") + "hupo");
            //textBox2.Text = transform(textBox3.Text, DateTime.Now.Date.ToString("yyyy-MM-dd") + "hupo");
        }

        private void button3_Click(object sender, EventArgs e)              //1天版
        {
            DateTime dDateToday = DateTime.Now.Date;
            string strToday = dDateToday.ToString("yyyy-MM-dd");
            string strEnCodeToday = EnText(textBox_SN.Text, strToday + "Lumia920");
            string strRegCodeToday = Transform(strEnCodeToday, strToday + "Lumia920");

            DateTime dDateTomorrow = DateTime.Now.Date.AddDays(1);
            string strTomorrow = dDateTomorrow.ToString("yyyy-MM-dd");
            string strEnCodeTomorrow = EnText(textBox_SN.Text, strTomorrow + "Lumia920");
            string strRegCodeTomorrow = Transform(strEnCodeTomorrow, strTomorrow + "Lumia920");

            string strOutputText = "您好，软件注册码：";
            strOutputText += strRegCodeToday + "。" + "\r\n" + "如果今天来不及注册，明天也可以，注册码为：";
            strOutputText += strRegCodeTomorrow + "。" + "\r\n" + "感谢关注Aurora。";
            textBox_KEY.Text = strOutputText;
            //textBox3.Text = EnText(textBox1.Text, DateTime.Now.Date.ToString("yyyy-MM-dd") + "Lumia920");
            //textBox2.Text = Transform(textBox3.Text, DateTime.Now.Date.ToString("yyyy-MM-dd") + "Lumia920");
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
        public string Transform(string input, string skey)
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

        private void RegMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }



    }
}
