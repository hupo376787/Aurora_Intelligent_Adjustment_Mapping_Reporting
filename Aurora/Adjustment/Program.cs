using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Reflection;
using Microsoft.Win32;

namespace Adjustment
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]

        static void Main()
        {
            string MName = Process.GetCurrentProcess().MainModule.ModuleName;
            string PName = Path.GetFileNameWithoutExtension(MName);
            Process[] myProcess = Process.GetProcessesByName(PName);
            if (myProcess.Length > 1)
            {
                if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
                {
                    MessageBox.Show("Aurora侦测到本程式已经在运行，请勿重复打开。", "Aurora智能提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
                {
                    MessageBox.Show("Aurora偵測到本程式已經在運行，請勿重複打開。", "Aurora智慧提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
                {
                    MessageBox.Show("Aurora has detected this program is already running, please do not open again.", "Aurora Intelligent Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            //是否可以打开新进程
            else
            {
                //显示桌面提示
                RegistryKey MyReg, RegReminder;
                MyReg = Registry.CurrentUser;
                try
                {
                    RegReminder = MyReg.CreateSubKey("Software\\Aurora\\Reminder");
                    if (RegReminder.GetValue("ShowDesktopReminder").ToString() == "YES")
                    {
                        Type type = Type.GetTypeFromProgID("Shell.Application");
                        object instance = Activator.CreateInstance(type);
                        type.InvokeMember("ToggleDesktop", BindingFlags.InvokeMethod, null, instance, null);
                    }
                }
                catch { }

                if (File.Exists(Application.StartupPath + "\\Aurora_Splash.exe"))
                {
                    System.Diagnostics.Process.Start("Aurora_Splash.exe");
                }
                Thread.Sleep(2500);
                System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcesses();
                foreach (System.Diagnostics.Process p in ps)
                {
                    if (p.ProcessName == "Aurora_Splash")
                    {
                        p.Kill();
                        break;
                    }
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                //Application.Run(new AuroraMain());
                Application.Run(PublicClass.AuroraMain);
            }

        }
    }
}
