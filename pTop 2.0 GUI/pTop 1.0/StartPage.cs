using pTop.classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;

namespace pTop
{
    public class StartPage
    {
        public ObservableCollection<ConfigHelper.TaskInfo> recent_taskInfo;
        public bool check_license()
        {
            string license_file = "pTop.license";
            string dbgen_file = "dbgen.exe";
            int SUCCESS = 1024;

            ProcessStartInfo info = new ProcessStartInfo(dbgen_file);  // 1024:成功；其他:失败
            info.UseShellExecute = false;
            info.Arguments = "3"; // 3 for pTop
            Process proBach = Process.Start(info);
            proBach.WaitForExit();
            int returnValue = proBach.ExitCode;
            if (returnValue != SUCCESS)
            {
                Application.Current.Shutdown();
                Environment.Exit(0); // exit everything completely! better than Application.Current.Shutdown();
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
            //recent_taskInfo = ConfigHelper.load_all_recent_taskPath(Advanced.recent_tasks_file_path);
        }
    }
}
