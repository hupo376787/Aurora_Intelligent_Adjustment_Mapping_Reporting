using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace Adjustment
{
    public partial class MyCmd : Form
    {
        //鼠标拖动相关变量
        Point oldPoint = new Point(0, 0);
        bool mouseDown = false;

        public MyCmd()
        {
            InitializeComponent();
            this.DisplayMessage(Constants.CopyRight);
            this.DisplayMessage(Constants.LabelFormat);

            MouseDown += new MouseEventHandler(Console_MouseDown);
            MouseUp += new MouseEventHandler(Console_MouseUp);
            MouseMove += new MouseEventHandler(Console_MouseMove);
        }

        #region 可移动无边框窗体
        void Console_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Left += (e.X - oldPoint.X);
                this.Top += (e.Y - oldPoint.Y);
            }
        }

        void Console_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        void Console_MouseDown(object sender, MouseEventArgs e)
        {
            oldPoint = e.Location;
            mouseDown = true;
        }
        #endregion

        private void MyCmd_Load(object sender, EventArgs e)
        {
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)                  //一定要隐藏，才能关闭后再次调出来
        {
            this.Hide();
        }

        private void pictureBox1_MouseHover(object sender, EventArgs e)             //Hover
        {
            pictureBox1.Image = Properties.Resources.CloseHover;
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.Close;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox1.Image = Properties.Resources.CloseDown;
        }
        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (!new Keys[] { Keys.Right, Keys.Down }.Contains(e.KeyCode)
                && this.richTextBox1.SelectionStart <= this.UndeletablePositions
                || (e.KeyCode == Keys.Delete && this.richTextBox1.SelectionStart < this.UndeletablePositions))
            {
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                var command = this.GetCommand();
                this.ExecuteCommand(command);
                this.DisplayMessage(Constants.LabelFormat);
                e.Handled = true;
            }
        }

        private void richTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (this.richTextBox1.SelectionStart <= this.UndeletablePositions
                || (e.KeyChar == (char)Keys.Delete && this.richTextBox1.SelectionStart < this.UndeletablePositions))
            {
                //e.Handled = true;
            }
        }

        private int UndeletablePositions
        {
            get
            {
                return this.richTextBox1.Text.LastIndexOf(Constants.Label) + Constants.Label.Length;
            }
        }

        private void ExecuteCommand(string command)             //处理输入的指令。能够整合CMD的指令更好。
        {
            string strSysPath = System.Environment.GetFolderPath(Environment.SpecialFolder.System);

            switch (command.ToLower())
            {
                case Constants.CmdAddRow:
                    {
                        PublicClass.AuroraMain.toolStripButton_Add_Click(null, null);               //使用公共类PublicClass
                        
                        break;
                    }
                case Constants.CmdDeleteRow:
                    {
                        PublicClass.AuroraMain.toolStripButton_Delete_Click(null, null);
                        break;
                    }
                case Constants.CmdClearList:
                    {
                        PublicClass.AuroraMain.toolStripButton_Clear_Click(null, null);
                        break;
                    }
                case Constants.CmdCalcAdj:
                    {
                        PublicClass.AuroraMain.toolStripButton_Calc_Click(null, null);
                        PublicClass.AuroraMain.nCalcFlag = 1;       //计算完成标志
                        break;
                    }
                case Constants.CmdDrawMap:
                    {
                        PublicClass.AuroraMain.toolStripButton_Mapping_Click(null, null);
                        break;
                    }
                case Constants.CmdReport:
                    {
                        PublicClass.AuroraMain.toolStripButton_Report_Click(null, null);
                        break;
                    }
                case Constants.CmdNotepad:
                    {
                        System.Diagnostics.Process.Start(strSysPath + "\\notepad.exe");
                        break;
                    }
                case Constants.CmdCalc:
                    {
                        System.Diagnostics.Process.Start(strSysPath + "\\calc.exe");
                        //调用外部程序导cmd命令行
                        //Process p = new Process();
                        //p.StartInfo.FileName = "cmd.exe";
                        //p.StartInfo.UseShellExecute = false;
                        //p.StartInfo.RedirectStandardInput = true;
                        //p.StartInfo.RedirectStandardOutput = true;
                        //p.StartInfo.CreateNoWindow = false;
                        //p.Start();
                        //向cmd.exe输入command 
                        //p.StandardInput.WriteLine("ipconfig /all");

                        //p.StandardInput.WriteLine("exit"); //需要有这句，不然程序会挂机
                        //string output = p.StandardOutput.ReadToEnd(); //这句可以用来获取执行命令的输出结果，但要在退出之后
                        //MessageBox.Show(output);
                        break;
                    }
                case Constants.CmdMspaint:
                    {
                        System.Diagnostics.Process.Start(strSysPath + "\\mspaint.exe");
                        break;
                    }
                case Constants.CmdCmd:
                    {
                        System.Diagnostics.Process.Start(strSysPath + "\\cmd.exe");
                        break;
                    }
                case Constants.CmdExplorer:
                    {
                        System.Diagnostics.Process.Start(strSysPath + "\\explorer.exe");
                        break;
                    }
                case Constants.CmdRegedit:
                    {
                        System.Diagnostics.Process.Start(strSysPath + "\\regedit.exe");
                        break;
                    }
                case Constants.CmdTaskmgr:
                    {
                        System.Diagnostics.Process.Start(strSysPath + "\\taskmgr.exe");
                        break;
                    }
                case Constants.CmdWrite:
                    {
                        System.Diagnostics.Process.Start(strSysPath + "\\write.exe");
                        break;
                    }

                case Constants.Cmdhupo376787:
                    {
                        System.Diagnostics.Process.Start("http://weibo.com/aurorapro");
                        break;
                    }
                case Constants.CmdNokia:
                    {
                        MessageBox.Show("Nokia？Newkia？");
                        break;
                    }
                case Constants.CmdHelp:
                    {
                        if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                        {
                            this.DisplayMessage(Constants.Help);
                        }
                        else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                        {
                            string strHelpenCmd = "\r\n歡迎使用Aurora 命令行智慧説明系統。目前支援Aurora內部常見的命令和Windows的部分命令列指令(不區分大小寫)。" + "\r\n"
                                    + "AddRow：增加一行資料。" + "\r\n"
                                    + "DeleteRow：刪除選中行資料。" + "\r\n"
                                    + "ClearList：清空所有資料。" + "\r\n"
                                    + "CalcAdj：開始平差計算。" + "\r\n"
                                    + "DrawMap：繪製點點陣圖。" + "\r\n"
                                    + "Reports：生成報表。" + "\r\n";
                            this.DisplayMessage(strHelpenCmd);
                        }
                        else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                        {
                            string strHelpenCmd = "\r\nWelcome to Aurora cmd help. You can input Aurora internal command and part of windows command(ignore caps lock)." + "\r\n"
                                    + "AddRow: Add a row data." + "\r\n"
                                    + "DeleteRow: delete selected row data." + "\r\n"
                                    + "ClearList: Clear all data." + "\r\n"
                                    + "CalcAdj: Adjust calculation." + "\r\n"
                                    + "DrawMap: Draw map." + "\r\n"
                                    + "Reports: Generate Report." + "\r\n";
                            this.DisplayMessage(strHelpenCmd);
                        }
                        break;
                    }
                default: this.DisplayMessage(Constants.ErrorFormat, command); break;
            }
        }

        private string GetCommand()
        {
            try
            {
                var test = this.UndeletablePositions;

                return this.richTextBox1.Lines
                    .Last(s => s.Contains(Constants.Label))
                    .Split('>')
                    .Last();
            }
            catch (System.Exception ex)
            {
                return "";
            }
            
        }

        private void DisplayMessage(string message, string arg = null)
        {
            if (arg == null)
            {
                this.richTextBox1.AppendText(message);
            }
            else
            {
                this.richTextBox1.AppendText(string.Format(message, arg));
            }
        }


        #region 右键菜单：复制/剪切/粘贴/回车
        private void toolStripMenuItem_Copy_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(richTextBox1.SelectedText);
            }
            catch { }
        }

        private void toolStripMenuItem_Cut_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetData(DataFormats.Rtf, richTextBox1.SelectedRtf);
                richTextBox1.SelectedRtf = "";
            }
            catch { } 
        }

        private void toolStripMenuItem_Paste_Click(object sender, EventArgs e)
        {
            try
            {
                richTextBox1.Paste();
            }
            catch { } 
        }

        private void toolStripMenuItem_Enter_Click(object sender, EventArgs e)
        {
            SendKeys.Send("{ENTER}");
        }

        private void toolStripMenuItem_Clear_Click(object sender, EventArgs e)
        {
            this.richTextBox1.Clear();
            this.DisplayMessage(Constants.CopyRight);
            this.DisplayMessage(Constants.LabelFormat);
        }

        #endregion

        #region 单击Label1标签也可以移动窗体哦
        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            oldPoint = e.Location;
            mouseDown = true;
        }

        private void label1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Left += (e.X - oldPoint.X);
                this.Top += (e.Y - oldPoint.Y);
            }
        }

        private void label1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }
        #endregion

        private void MyCmd_Activated(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                label1.Text = "命令行";
                //System.ComponentModel.ComponentResourceManager res = new ComponentResourceManager(typeof(AuroraMain));
                //foreach (ToolStripItem item in this.contextMenuStrip1.Items)       //右键菜单
                //{
                //    res.ApplyResources(item, item.Name);
                //}
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                label1.Text = "命令行";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                label1.Text = "Cmd Line";
            }
        }




    }
}
