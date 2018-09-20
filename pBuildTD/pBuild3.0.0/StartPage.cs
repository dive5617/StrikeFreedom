using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace pBuild
{
    public class StartPage
    {
        public ObservableCollection<TaskInfo> recent_taskInfo;
        public bool check_license()
        {
            string license_file = Task.pFind_license;
            if (!File.Exists(license_file))
            {
                License_Dialog ld = new License_Dialog();
                ld.Show();
                return false;
            }
            bool flag = true;
            string license = File_Help.get_license(license_file, ref flag);
            MachineCode mc = new MachineCode();
            string code = mc.Get_Code();
            code = mc.MD5_PWD(code);
            string code_license = mc.MD5(code);
            if (code_license != license || !flag)
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
            recent_taskInfo = File_Help.load_all_recent_taskInfo(Task.recent_task_path_file);
        }
    }
}
