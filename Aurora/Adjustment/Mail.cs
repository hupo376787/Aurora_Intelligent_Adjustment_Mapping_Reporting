using System;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Threading;
using System.Globalization;

namespace Adjustment
{
    public partial class Mail : Form
    {
        public Mail()
        {
            //Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-Hant");//手动设置语言。
            InitializeComponent();
        }

        [DllImport("wininet.dll ")]
        private static extern bool InternetGetConnectedState(ref int dwFlag, int dwReserved);

        private void button1_Click(object sender, EventArgs e)              //添加附件
        {
            OpenFileDialog open = new OpenFileDialog();
            if (open.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = open.FileName.ToString();
            }
        }

        private void button2_Click(object sender, EventArgs e)              //发送
        {
            int dwFlag = 0;
            if (!InternetGetConnectedState(ref dwFlag, 0))
            {
                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                {
                    MessageBox.Show("Opps~~~若要顺利发邮件，请您先插好网线！", "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                {
                    MessageBox.Show("Opps~~~若要順利發郵件，請您先插好網線！", "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                {
                    MessageBox.Show("Opps~~~Please connect to Internet first！", "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            } 
            else
            {
                string regexEmail = @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
                System.Text.RegularExpressions.RegexOptions options = ((System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace
                    | System.Text.RegularExpressions.RegexOptions.Multiline) | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                System.Text.RegularExpressions.Regex regEmail = new System.Text.RegularExpressions.Regex(regexEmail, options);
                string email = textBox1.Text;
                if (!regEmail.IsMatch(email))//email 填写符合正则表达式 
                {
                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                    {
                        MessageBox.Show("Email格式错误！", "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                    {
                        MessageBox.Show("Email格式錯誤！", "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                    {
                        MessageBox.Show("Wrong Email format！", "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return;
                }

                if (textBox2.Text.Trim().ToString() == "")
                {
                    if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                    {
                        MessageBox.Show("请填写邮件主题！", "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                    {
                        MessageBox.Show("請填寫郵件主題！", "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                    {
                        MessageBox.Show("Don't forget Email theme！", "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return;
                }

                try
                {
                    if (this.textBox4.Text.Trim().ToString() != "")
                    {
                        //发送
                        SmtpClient Client = new SmtpClient("smtp.qq.com");   //设置邮件协议
                        Client.UseDefaultCredentials = false;//这一句得写前面
                        Client.DeliveryMethod = SmtpDeliveryMethod.Network; //通过网络发送到Smtp服务器
                        Client.Credentials = new NetworkCredential("xxxxxx", "xxxxx"); //通过用户名和密码 认证

                        MailMessage Mail = new MailMessage("xxxxxx", "xxxxxx");
                        Mail.Subject = "【Aurora反馈】" + this.textBox2.Text.Trim().ToString();
                        Mail.SubjectEncoding = Encoding.UTF8;   //主题编码

                        Mail.Body = "来自:" + textBox1.Text.Trim().ToString() + "\r\n" +
                                    "主题:" + textBox2.Text.Trim().ToString() + "\r\n" +
                                    "内容:" + "\r\n" + this.textBox4.Text.Trim().ToString();

                        Mail.BodyEncoding = Encoding.UTF8;      //正文编码
                        Mail.IsBodyHtml = false;    //设置为HTML格式           
                        Mail.Priority = MailPriority.Normal;   //优先级

                        //附件
                        if (textBox3.Text.Length > 0)
                        {
                            Mail.Attachments.Add(new Attachment(textBox3.Text, MediaTypeNames.Application.Octet));
                        }

                        Client.Send(Mail);

                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            MessageBox.Show("邮件发送成功！", "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            MessageBox.Show("郵件發送成功！", "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            MessageBox.Show("Email send success！", "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            MessageBox.Show("请填写邮件内容！", "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            MessageBox.Show("請填寫郵件內容！", "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            MessageBox.Show("Please complete Email content！", "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            
        }

        private void button3_Click(object sender, EventArgs e)              //退出
        {
            this.Close();
        }

        private void button1_MouseEnter(object sender, EventArgs e)             //鼠标提示
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            string Tips = "";
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                Tips = "如果上传附件过大，请稍等片刻(建议压缩后上传附件)。";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                Tips = "如果上傳附件過大，請稍等片刻(建議壓縮後上傳附件)。";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")   
            {
                Tips = "Please wait while Aurora is uploading files(compress your file is highly recommended).";
            }
            p.SetToolTip(this.button1, Tips);
        }

        #region 邮件内容水印
        private void textBox4_Click(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                if (textBox4.Text == "请尽量将您遇到的问题描述清楚，并确保电子邮箱可用。")
                {
                    textBox4.Text = "";
                }
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                if (textBox4.Text == "請儘量將您遇到的問題描述清楚，並確保電子郵箱可用。")
                {
                    textBox4.Text = "";
                }
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")   
            {
                if (textBox4.Text == "Please describle your problems as detailed as possible, and ensure your e-mail is correct.")
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
                    textBox4.Text = "请尽量将您遇到的问题描述清楚，并确保电子邮箱可用。";
                }
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                if (textBox4.Text == "")
                {
                    textBox4.Text = "請儘量將您遇到的問題描述清楚，並確保電子郵箱可用。";
                }
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")   
            {
                if (textBox4.Text == "")
                {
                    textBox4.Text = "Please describle your problems as detailed as possible, and ensure your e-mail is correct.";
                }
            }
            
        }
        #endregion

        private void Mail_Load(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                textBox4.Text = "请尽量将您遇到的问题描述清楚，并确保电子邮箱可用。";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                textBox4.Text = "請儘量將您遇到的問題描述清楚，並確保電子郵箱可用。";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")   
            {
                textBox4.Text = "Please describle your problems as detailed as possible, and ensure your e-mail is correct.";
            }
        }
        
    }
}
