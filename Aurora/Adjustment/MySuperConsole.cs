using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Adjustment
{
    public partial class MySuperConsole : Form
    {
        public MySuperConsole()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RegistryKey MyReg0, RegGUIDFlag, RegFlag, RegValidGUIDDays, RegValidDays, RegStartGUIDDate, RegStartDate, RegPassword;
            MyReg0 = Registry.CurrentUser;

            //删除原有注册表项
            MyReg0.DeleteSubKeyTree("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}", false);
            MyReg0.DeleteSubKeyTree("Software\\Aurora", false);

            try
            {
                RegGUIDFlag = MyReg0.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                RegFlag = MyReg0.CreateSubKey("Software\\Aurora");

                RegValidGUIDDays = MyReg0.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                RegValidDays = MyReg0.CreateSubKey("Software\\Aurora");

                RegStartGUIDDate = MyReg0.CreateSubKey("Identities\\{D46B7C02-B796-4AAA-9D4F-2188CF2DBA30}");
                RegStartDate = MyReg0.CreateSubKey("Software\\Aurora");

                RegFlag.SetValue("nRegFlag", "0");
                RegValidDays.SetValue("nValidDays", "10");
                RegGUIDFlag.SetValue("nRegGUIDFlag", "0");
                RegValidGUIDDays.SetValue("nValidGUIDDays", "10");
                RegStartGUIDDate.SetValue("StartGUIDDate", DateTime.Now.ToString("yyyy-MM-dd"));
                RegStartDate.SetValue("StartDate", DateTime.Now.ToString("yyyy-MM-dd"));

                RegPassword = MyReg0.CreateSubKey("Software\\Aurora\\Locker");
                RegPassword.SetValue("Password", "000");
                //Application.Restart();
            }
            catch { }
        }
    }
}
