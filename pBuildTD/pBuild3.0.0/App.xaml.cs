using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace pBuild
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            string task_path = "";
            if (e.Args != null && e.Args.Count() > 0)
            {
                task_path = e.Args[0];
            }
            //下面注释掉的均是单例模式，检查是否已经存在pBuild.exe，如果存在则直接使用打开的pBuild进行打开。
            //bool is_in = is_process_in(task_path);
            //if (is_in)
            //{
            //    Application.Current.Shutdown();
            //    return;
            //}
            if (e.Args != null && e.Args.Count() > 0)
            {
                this.Properties["ArbitraryArgName"] = e.Args[0];
            }
            base.OnStartup(e);
        }
        private static bool is_process_in(string task_path)
        {
            //get this running process
            Process proc = Process.GetCurrentProcess();
            //get all other (possible) running instances
            Process[] processes = Process.GetProcessesByName(proc.ProcessName);

            if (processes.Length > 1)
            {
                //iterate through all running target applications
                foreach (Process p in processes)
                {
                    if (p.Id != proc.Id)
                    {
                        //COPYDATASTRUCT cds = new COPYDATASTRUCT();
                        //byte[] sarr = System.Text.Encoding.Default.GetBytes(task_path);
                        //int len = sarr.Length;
                        //cds.dwData = (IntPtr)100;
                        //cds.path = task_path;
                        //cds.cbData = len + 1;

                        //NativeMethods.SendMessage(p.MainWindowHandle, NativeMethods.WM_SHOWME, IntPtr.Zero, ref cds);
                        File_Help.write_task_path_Process(Task.task_path_process, task_path);
                        NativeMethods.SendMessage(p.MainWindowHandle, NativeMethods.WM_SHOWME, IntPtr.Zero, IntPtr.Zero);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
