using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace pTop.classes
{
    public class Advanced:INotifyPropertyChanged
    {
        public static string recent_tasks_file_path = "recentTasks.log";
        public static int recent_tasks_num = 10; 
        
        public event PropertyChangedEventHandler PropertyChanged;

        private int thread_num = 2;

        public int Thread_Num
        {
            get { return thread_num; }
            set
            {
                thread_num = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Thread_Num"));
                }
            }
        }
        private String output_path="";

        public String Output_Path
        {
            get { return output_path; }
            set
            {
                output_path = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Output_Path"));
                }
            }
        }
        // 当存在pTop.ini时
        public Advanced()
        {            
            string ini_path = System.Windows.Forms.Application.StartupPath + @"\pTop.ini";
            if (System.IO.File.Exists(ini_path))
            {
                StreamReader sr = new StreamReader(ini_path, Encoding.Default);
                string strLine = sr.ReadLine();
                while (strLine != null)
                {
                    if (strLine.Length > 6 && strLine.Substring(0, 6).Equals("thread"))
                    {
                        int.TryParse(strLine.Substring(strLine.LastIndexOf("=") + 1), out thread_num);
                    }
                    else if (strLine.Length > 10 && strLine.Substring(0, 10).Equals("outputpath"))
                    {
                        output_path = strLine.Substring(strLine.LastIndexOf("=") + 1);
                    }
                    strLine = sr.ReadLine();
                }
                sr.Close();
            }
        }
        // 当不存在pTop.ini或路径无效时，比如首次启动
        public void defaultAdvanced() 
        {
            thread_num = 2;
            string[] dr = new string[10];
            dr = Directory.GetLogicalDrives();
            string disk = dr[0];
            if (Directory.Exists("D:\\"))
            {
                disk = "D:\\";
            }
            else if (dr.Length > 1)
            {
                disk = dr[1];
            }
            output_path = disk + "pTopWorkspace";
            //WriteSettings();    // [wrm] comment out by wrm 2016.03.01.
        }


        public void WriteSettings()
        {
            try
            {
                StreamWriter sw = new StreamWriter(System.Windows.Forms.Application.StartupPath + @"\pTop.ini", false, Encoding.Default);
                sw.WriteLine("thread=" + this.thread_num);
                if (this.output_path[this.output_path.Length - 1] != '\\')
                {
                    this.output_path += '\\';
                }
                sw.WriteLine("outputpath=" + this.output_path);
                if (!Directory.Exists(this.output_path))
                {
                    Directory.CreateDirectory(this.output_path);
                }
                sw.Close();
            }
            catch(Exception exe)
            {
                // 可能会出现 pTop.ini 正在被占用
                MessageBox.Show(exe.Message + "Please reconfigure it later!");
            }
        }
    }
}
