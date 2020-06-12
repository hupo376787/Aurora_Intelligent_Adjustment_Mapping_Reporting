using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Runtime.InteropServices;

namespace Adjustment
{
    public partial class FeedBack : Form
    {
        public FeedBack()
        {
            InitializeComponent();
        }

        [DllImport("wininet.dll ")]
        private static extern bool InternetGetConnectedState(ref int dwFlag, int dwReserved);

        private void button2_Click(object sender, EventArgs e)
        {
            string strG1 = "", strG2 = "", strG3 = "", strG4 = "", strG5 = "", strG6 = "";
            if (radioButton1.Checked == true)
            {
                strG1 = "软件界面美观程度  ----  " + radioButton1.Text + "\r\n";
            }
            else if (radioButton2.Checked == true)
            {
                strG1 = "软件界面美观程度  ----  " + radioButton2.Text + "\r\n";
            }
            else if (radioButton3.Checked == true)
            {
                strG1 = "软件界面美观程度  ----  " + radioButton3.Text + "\r\n";
            }
            if (radioButton1.Checked == false && radioButton2.Checked == false && radioButton3.Checked == false)
            {
                MessageBox.Show("您还没评价：软件界面美观程度");
                return;
            }

            if (radioButton4.Checked == true)
            {
                strG2 = "软件操作方便程度  ----  " + radioButton4.Text + "\r\n";
            }
            else if (radioButton5.Checked == true)
            {
                strG2 = "软件操作方便程度  ----  " + radioButton5.Text + "\r\n";
            }
            else if (radioButton6.Checked == true)
            {
                strG2 = "软件操作方便程度  ----  " + radioButton6.Text + "\r\n";
            }
            if (radioButton4.Checked == false && radioButton5.Checked == false && radioButton6.Checked == false)
            {
                MessageBox.Show("您还没评价：软件操作方便程度");
                return;
            }

            if (radioButton7.Checked == true)
            {
                strG3 = "软件稳定性  ----  " + radioButton7.Text + "\r\n";
            }
            else if (radioButton8.Checked == true)
            {
                strG3 = "软件稳定性  ----  " + radioButton8.Text + "\r\n";
            }
            else if (radioButton9.Checked == true)
            {
                strG3 = "软件稳定性  ----  " + radioButton9.Text + "\r\n";
            }
            if (radioButton7.Checked == false && radioButton8.Checked == false && radioButton9.Checked == false)
            {
                MessageBox.Show("您还没评价：软件稳定性");
                return;
            }

            if (radioButton10.Checked == true)
            {
                strG4 = "智能平差模块  ----  " + radioButton10.Text + "\r\n";
            }
            else if (radioButton11.Checked == true)
            {
                strG4 = "智能平差模块  ----  " + radioButton11.Text + "\r\n";
            }
            else if (radioButton12.Checked == true)
            {
                strG4 = "智能平差模块  ----  " + radioButton12.Text + "\r\n";
            }
            if (radioButton10.Checked == false && radioButton11.Checked == false && radioButton12.Checked == false)
            {
                MessageBox.Show("您还没评价：智能平差模块");
                return;
            }

            if (radioButton13.Checked == true)
            {
                strG5 = "智能绘图模块  ----  " + radioButton13.Text + "\r\n";
            }
            else if (radioButton14.Checked == true)
            {
                strG5 = "智能绘图模块  ----  " + radioButton14.Text + "\r\n";
            }
            else if (radioButton15.Checked == true)
            {
                strG5 = "智能绘图模块  ----  " + radioButton15.Text + "\r\n";
            }
            if (radioButton13.Checked == false && radioButton14.Checked == false && radioButton15.Checked == false)
            {
                MessageBox.Show("您还没评价：智能绘图模块");
                return;
            }

            if (radioButton16.Checked == true)
            {
                strG6 = "智能报表模块  ----  " + radioButton16.Text + "\r\n" + "\r\n";
            }
            else if (radioButton17.Checked == true)
            {
                strG6 = "智能报表模块  ----  " + radioButton17.Text + "\r\n" + "\r\n";
            }
            else if (radioButton18.Checked == true)
            {
                strG6 = "智能报表模块  ----  " + radioButton18.Text + "\r\n" + "\r\n";
            }
            if (radioButton16.Checked == false && radioButton17.Checked == false && radioButton18.Checked == false)
            {
                MessageBox.Show("您还没评价：智能报表模块");
                return;
            }

            string strResult = "";
            strResult = strG1 + strG2 + strG3 + strG4 + strG5 + strG6;

            int dwFlag = 0;
            if (!InternetGetConnectedState(ref dwFlag, 0))
            {
                MessageBox.Show("Opps~~~若要顺利发邮件，请您先插好网线！", "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                try
                {
                    //发送
                    SmtpClient Client = new SmtpClient("smtp.qq.com");   //设置邮件协议
                    Client.UseDefaultCredentials = false;//这一句得写前面
                    Client.DeliveryMethod = SmtpDeliveryMethod.Network; //通过网络发送到Smtp服务器
                    Client.Credentials = new NetworkCredential("xxxxx", "xxxxx"); //通过用户名和密码 认证

                    MailMessage Mail = new MailMessage("xxxxx", "xxxxx");
                    Mail.Subject = "【Aurora问卷调查】";
                    Mail.SubjectEncoding = Encoding.UTF8;   //主题编码

                    string regexEmail = @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
                    System.Text.RegularExpressions.RegexOptions options = ((System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace
                        | System.Text.RegularExpressions.RegexOptions.Multiline) | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    System.Text.RegularExpressions.Regex regEmail = new System.Text.RegularExpressions.Regex(regexEmail, options);
                    string email = textBox2.Text;
                    if (!regEmail.IsMatch(email))//email 填写符合正则表达式 
                    {
                        MessageBox.Show("E-mail格式错误！", "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    if (textBox1.Text == "")
                    {
                        MessageBox.Show("写点啥好的建议，再发也不迟。");
                    }
                    else
                    {
                        Mail.Body = strResult + textBox2.Text.ToString() + "\r\n" + "\r\n" + textBox1.Text.ToString();
                        Mail.BodyEncoding = Encoding.UTF8;      //正文编码
                        Mail.IsBodyHtml = false;    //设置为HTML格式           
                        Mail.Priority = MailPriority.Normal;   //优先级
                        Client.Send(Mail);
                        MessageBox.Show("邮件发送成功！", "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch { }
            }
        }

        private void FeedBack_Load(object sender, EventArgs e)
        {
            MessageBox.Show("写在前面：很感谢您能在百忙之中来抽出时间完成我们的调查。请根据实际情况填写，您的反馈对我们改进Aurora非常重要，谢谢配合。我们会认真阅读每一个用户的反馈和建议，并尽量在下一版本的更新中完善。本软件不会收集任何关于您的个人隐私信息。", "Aurora 问卷调查");

        }
    }
}
