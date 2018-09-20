using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;
using pTop.classes;

namespace pTop
{
    public delegate Int32 UnhandledExceptionFilter(ref long a);

    public class CrashReportMgr
    {
        [DllImport("kernel32")]
        private static extern Int32 SetUnhandledExceptionFilter(UnhandledExceptionFilter fun);
        public CrashReportMgr()
        {
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //try
            //{
                GenerateReport(e.ExceptionObject, 1);

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "CrashReporter.exe";
                Process p = Process.GetCurrentProcess();
                string pTop_license = "pTop.license";
                string user_information = ConfigHelper.get_userInformation_inLicense(pTop_license);
                //psi.Arguments = p.ProcessName + " " + "\"" + e.ExceptionObject.ToString() + "\"";
                psi.Arguments = p.ProcessName + " " + "CrashReportLog.txt" + " " + user_information;
                Process.Start(psi);

                if (e.IsTerminating)
                {
                    Environment.Exit(0);
                }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("UnhandledException: " + ex.ToString());
            //}
        }

        void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            //try
            //{
                GenerateReport(e.Exception, 0);
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "CrashReporter.exe";
                Process p = Process.GetCurrentProcess();
                // psi.Arguments = p.ProcessName + " " + "\"" + e.Exception.ToString() + "\"";
                psi.Arguments = p.ProcessName + " " + "CrashReportLog.txt";
                Process.Start(psi);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("ThreadException: " + ex.ToString());
            //}
        }

        private void GenerateReport(object o,int type)
        {            
            FileStream fs = null;
            StreamWriter sw = null;
            try
            {
                MiniDump.TryDump("CrashReportDmp.dmp", pTop.MiniDump.MiniDumpType.Normal);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
            try
            {
                DateTime dt = DateTime.Now;
                string str = dt.Day + dt.TimeOfDay.Hours.ToString() + dt.TimeOfDay.Minutes.ToString() + dt.TimeOfDay.Seconds.ToString();
                fs = System.IO.File.Create(@"CrashReportLog" + ".txt");
                sw = new StreamWriter(fs);
                sw.WriteLine("当前时间:{0}", dt.ToString());
                sw.WriteLine("错误类别:" + (type == 0 ? "UIException" : "Unhandledception"));
                sw.WriteLine(o.ToString());
                sw.Flush();
                sw.Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
            finally
            {
                if(sw!=null)
                    sw.Close();
            }
        }

    }
}
