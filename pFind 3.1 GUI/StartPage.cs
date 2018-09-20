using pFind.classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace pFind
{
    public class StartPage
    {
        public ObservableCollection<ConfigHelper.TaskInfo> recent_taskInfo;
        public bool check_license()
        {
            return true;  // 所有的check都交由TimeControler来检查了
            string license_file = "pFind.license";
            if (!System.IO.File.Exists(license_file))
            {
                License_Dialog ld = new License_Dialog();
                ld.Show();
                return false;
            }
            bool flag = true;
            string license = ConfigHelper.get_license(license_file, ref flag);
            MachineCode mc = new MachineCode();
            string code = mc.Get_Code();
            code = mc.MD5_PWD(code);
            string code_license = mc.MD5(code);
            Time_Help tHelp = new Time_Help();
            bool flag2 = tHelp.is_LicenseValidate(license_file);
            if (code_license != license || !flag || !flag2)
            {
                MessageBox.Show(Message_Help.LICENSE_WRONG);
                License_Dialog ld = new License_Dialog();
                ld.Show();
                return false;
            }
            return true;
        }
        public void add_CrashHandler()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            CrashReportMgr mgr = new CrashReportMgr();
        }
        public StartPage()
        {
            recent_taskInfo = ConfigHelper.load_all_recent_taskPath(Advanced.recent_tasks_file_path);
        }
    }
}
