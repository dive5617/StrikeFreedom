
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pTop.classes
{

    public class ConfigHelper
    {
        public static string ptop_version = "2.0";
        public static string startup_path = Application.StartupPath;
        
        public static Hashtable DBmap = new Hashtable();   //数据库名字、路径
        public static Hashtable ReDBmap = new Hashtable();   //database path、name
        public static ObservableCollection<string> dblist = new ObservableCollection<string>();     //Indentification中数据库列表
        public static ObservableCollection<String> com_mods = new ObservableCollection<String>(); //常用的修饰
        public static ObservableCollection<String> all_mods = new ObservableCollection<String>(); //所有的修饰

        public static Hashtable Labelmap = new Hashtable();     //Map：标记名字、具体元素
        public static Hashtable ReLabelmap = new Hashtable();   //反Map：label element,label name
        public static ObservableCollection<LabelInfo> labels = new ObservableCollection<LabelInfo>(); //标记列表

        public static ObservableCollection<string> db_add = new ObservableCollection<string>();
        public static ObservableCollection<string> mod_add = new ObservableCollection<string>();
        public static ObservableCollection<LabelInfo> label_add = new ObservableCollection<LabelInfo>();

        public class TaskInfo:IComparable
        {
            public string tname { get; set; }
            public string tpath { get; set; }
            public string ttimespan { get; set; }
            public TaskInfo() { }
            public TaskInfo(string tname, string tpath)
            {
                this.tname = tname;
                this.tpath = tpath;
            }

            public static string ShowTime(DateTime date)
            {
                return "(" + showtime(date) + ")";
            }
            public static string showtime(DateTime date)
            {
                const int SECOND = 1;
                const int MINUTE = 60 * SECOND;
                const int HOUR = 60 * MINUTE;
                const int DAY = 24 * HOUR;
                const int MONTH = 30 * DAY;
                TimeSpan ts = DateTime.Now - date;
                double delta = ts.TotalSeconds;
                if (delta < 0)
                {
                    return "not yet";
                }
                if (delta < 1 * MINUTE)
                {
                    return ts.Seconds == 1 ? "1 second ago" : ts.Seconds + " seconds ago";
                }
                if (delta < 2 * MINUTE)
                {
                    return "1 minute ago";
                }
                if (delta < 45 * MINUTE)
                {
                    return ts.Minutes + " minutes";
                }
                if (delta < 90 * MINUTE)
                {
                    return "1 hour ago";
                }
                if (delta < 24 * HOUR)
                {
                    return ts.Hours + " hours ago";
                }
                if (delta < 48 * HOUR)
                {
                    return "yesterday";
                }
                if (delta < 30 * DAY)
                {
                    return ts.Days + " days ago";
                }
                if (delta < 12 * MONTH)
                {
                    int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                    return months <= 1 ? "1 month ago" : months + " months ago";
                }
                else
                {
                    int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                    return years <= 1 ? "one year ago" : years + " years ago";
                }
            }

            int IComparable.CompareTo(Object obj)
            {
                TaskInfo temp = (TaskInfo)obj;
                return this.tpath.CompareTo(temp.tpath);
            }
            public override bool Equals(object obj)
            {
                var taskinfo = obj as TaskInfo;
                if (taskinfo == null)
                    return false;
                if (this.tpath == taskinfo.tpath)
                    return true;
                return false;
            }
            public static bool operator ==(TaskInfo a, TaskInfo b)
            {
                // If both are null, or both are same instance, return true.
                if (System.Object.ReferenceEquals(a, b))
                {
                    return true;
                }

                // If one is null, but not both, return false.
                if (((object)a == null) || ((object)b == null))
                {
                    return false;
                }

                // Return true if the fields match:
                return a.tpath == b.tpath;
            }
            public static bool operator !=(TaskInfo a, TaskInfo b)
            {
                return !(a == b);
            }
            public override int GetHashCode()
            {
                return this.tpath.GetHashCode();
            }
        }

        //加载历史任务列表
        public static ObservableCollection<TaskInfo> load_all_recent_taskPath(string log_path)
        {
            ObservableCollection<TaskInfo> all_recent_taskPath = new ObservableCollection<TaskInfo>();
            try
            {
                FileStream fs = new FileStream(log_path, FileMode.OpenOrCreate, FileAccess.Read);
                StreamReader reader = new StreamReader(fs, Encoding.Default);

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line.Trim() == "")
                        continue;
                    if (line.EndsWith("\\".ToString()))
                    {
                        line = line.Remove(line.Length - 1);
                    }
                    TaskInfo ti = new TaskInfo();
                    ti.tpath = line.Substring(line.IndexOf(' ')+1);
                    ti.tname = ti.tpath.Substring(ti.tpath.LastIndexOf("\\")+1);
                    ti.ttimespan = TaskInfo.ShowTime(Directory.GetLastWriteTime(ti.tpath));

                    if (Directory.Exists(ti.tpath))
                    {
                        all_recent_taskPath.Add(ti);
                        if (all_recent_taskPath.Count >= 6)
                        {
                            break;
                        }
                    }
                }
                fs.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                //return all_recent_taskPath;
            }
            return all_recent_taskPath;
        }

        public static ObservableCollection<string> load_recent_taskPaths(string log_path)
        {
            ObservableCollection<string> all_recent_taskPath = new ObservableCollection<string>();
            try
            {
                FileStream fs = new FileStream(log_path, FileMode.Open, FileAccess.Read);
                StreamReader reader = new StreamReader(fs, Encoding.Default);

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line.Trim() == "")
                        continue;
                    if (Directory.Exists(line.Substring(line.IndexOf(' ')+1)))
                    {
                        all_recent_taskPath.Add(line);
                    }
                }
                fs.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return all_recent_taskPath;
            }
            return all_recent_taskPath;
        }
        
        
        //获取序列号
        public static string get_license(string license_path, ref bool flag)
        {
            string text = System.IO.File.ReadAllText(license_path);
            string[] strs = text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (strs.Length < 2)
                flag = false;
            return strs.First();
        }
        public static string get_userInformation_inLicense(string license_path)
        {
            string text = System.IO.File.ReadAllText(license_path);
            string[] strs = text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string information = "";
            for (int i = 1; i < strs.Length - 1; ++i)
                information += strs[i] + "\n";
            information += strs[strs.Length - 1];
            return information;
        }
        
        /*读取数据库信息*/
        public static void getDB()
        {
            try
            {
                dblist.Clear();
                DBmap.Clear();
                ReDBmap.Clear();
                //MessageBox.Show(startup_path + "\\db.ini");
                FileStream fst = new FileStream(startup_path+"\\db.ini", FileMode.Open);
                StreamReader sr = new StreamReader(fst,Encoding.Default);
                string strLine = sr.ReadLine();
                string name = null;
                string path = null;
                while (strLine != null)
                {
                    if (strLine.Length>2 && strLine.Substring(0, 2).Equals("DB"))
                    {
                        name = strLine.Substring(strLine.LastIndexOf("=")+1, strLine.LastIndexOf(";") - strLine.LastIndexOf("=")-1);
                        strLine = sr.ReadLine();   //读取数据库路径
                        int tmp = strLine.LastIndexOf("=");
                        path = strLine.Substring(tmp + 2, strLine.LastIndexOf("\"") - tmp - 2);
                        dblist.Add(name);
                        DBmap.Add(name, path);
                        ReDBmap.Add(path, name);
                    }
                    strLine = sr.ReadLine();
                }
                dblist.Add("Customize Database...");
                sr.Close();
                fst.Close();
            }
            catch (IOException e)
            {
                Console.Write(e.Message);
                System.Windows.MessageBox.Show("Sorry, The db.ini cannot be opened!");
            }

        }

        //read modification information
        public static void get_mods()
        {
            try
            {
                com_mods.Clear();
                all_mods.Clear();
                /*读取常用修饰和全部修饰*/
                FileStream fst = new FileStream(ConfigHelper.startup_path + "\\modification.ini", FileMode.Open);
                StreamReader sr = new StreamReader(fst, Encoding.Default);
                string strLine = sr.ReadLine();
                while (strLine != null)
                {
                    if (strLine != "" && strLine.Substring(0, 4).Equals("name"))
                    {
                        string[] tmp = strLine.Split(' ');
                        int stt = tmp[0].LastIndexOf("=");
                        all_mods.Add(tmp[0].Substring(stt + 1));
                        if (tmp[1].Trim().Equals("0"))
                        {
                            com_mods.Add(tmp[0].Substring(stt + 1));
                        }

                    }
                    strLine = sr.ReadLine();
                }
                sr.Close();
            }
            catch (IOException e)
            {
                Console.Write(e.Message);
                MessageBox.Show(e.Message);
            }
        }

        //read Quantitation label information
        public static void get_labels()
        {
            try
            {
                labels.Clear();
                Labelmap.Clear();
                ReLabelmap.Clear();
                /*读取标记信息*/
                FileStream fst = new FileStream(startup_path + "\\quant.ini", FileMode.Open);
                StreamReader sr = new StreamReader(fst, Encoding.Default);
                string strLine = sr.ReadLine();
                int label_num = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                int i = 0;
                strLine = sr.ReadLine();
                while (i < label_num && strLine != null)
                {
                    if (strLine != "" && strLine.Contains("="))
                    {
                        string lb_name = strLine.Substring(0, strLine.LastIndexOf("="));
                        string lb_element = strLine.Substring(strLine.LastIndexOf("=") + 1);
                        if (!(Labelmap.ContainsKey(lb_name) || ReLabelmap.ContainsKey(lb_element)))  // 已存在的标记名或标记元素，不再添加
                        {
                            labels.Add(new LabelInfo(lb_name, lb_element));
                            Labelmap.Add(lb_name, lb_element);
                            ReLabelmap.Add(lb_element, lb_name);
                        }
                    }
                    strLine = sr.ReadLine();
                }
                sr.Close();
            }
            catch (IOException e)
            {
                Console.Write(e.Message);
                System.Windows.MessageBox.Show(e.Message);
            }
        }

        ///<summary>
        /// 读取pConfig-pFind.txt,获取新加入的配置信息，即读即删
        ///</summary>
        ///<summary>
        /// 读取pConfig-pFind.txt,获取新加入的配置信息，即读即删
        ///</summary>
        public static void getNewAddedInfo()
        {
            try
            {
                //db_add.Clear();
                mod_add.Clear();
                label_add.Clear();
                //enzyme_add.Clear();
                string path = startup_path + "\\pConfig-pTop.txt";
                if (System.IO.File.Exists(path))
                {
                    FileStream fst = new FileStream(path, FileMode.Open);
                    StreamReader sr = new StreamReader(fst, Encoding.Default);
                    string strLine = sr.ReadLine();
                    string subtitle = "";
                    while (strLine != null)
                    {
                        strLine = strLine.Trim();
                        if (strLine.Length > 0 && strLine[0] == '[' && strLine[strLine.Length - 1] == ']')
                        {
                            subtitle = strLine;
                        }
                        else
                        {
                            if (subtitle.Equals("[database]"))
                            {
                                if (strLine.Length > 0)
                                {
                                    db_add.Add(strLine);
                                }
                            }
                            else if (subtitle.Equals("[modification]"))
                            {
                                if (strLine.Length > 0)
                                {
                                    mod_add.Add(strLine);
                                }

                            }
                            
                        }
                        strLine = sr.ReadLine();
                    }
                    sr.Close();
                    fst.Close();
                    System.IO.File.Delete(path);
                }
            }
            catch (IOException e)
            {
                Console.Write(e.Message);
                System.Windows.MessageBox.Show("Sorry, The pConfig-pTop.txt cannot be opened!");
            }
        }

        /// <summary>
        /// 通过路径和后缀名来获取文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="suffix_name"></param>
        /// <returns></returns>
        public static string getFileBySuffix(string path, string suffix_name) //根据后缀名来提取对应的文件名
        {
            string file_name = ""; ;
            string[] files = Directory.GetFiles(path, "*");
            for (int i = 0; i < files.Length; ++i)
            {
                string suffix = files[i].Substring(files[i].LastIndexOf(".")+1);          
                if (suffix == suffix_name)
                {
                    FileInfo fi = new FileInfo(files[i]);
                    file_name = fi.FullName;
                    return file_name;
                }
            }
            return file_name;
        }
        /// <summary>
        /// 根据后缀名判断是否存在相关文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="suffix_name"></param>
        /// <returns></returns>
        public static bool checkFileBySuffix(string path, string suffix_name) 
        {
            //string file_name = ""; ;
            string[] files = Directory.GetFiles(path, "*");
            for (int i = 0; i < files.Length; ++i)
            {
                if (files[i].EndsWith(suffix_name))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 备份整个任务
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destinationPath"></param>
        public static void CopyTask(string sourcePath, string destinationPath)
        {
            DirectoryInfo info = new DirectoryInfo(sourcePath);
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
            {                              
                    string destName = Path.Combine(destinationPath, fsi.Name);
                    if (fsi is System.IO.FileInfo) //子文件
                    {
                        System.IO.File.Copy(fsi.FullName, destName, true);
                    }
                    else  //子文件夹
                    {
                        if (fsi.Name.Equals("param") || fsi.Name.Equals("result"))
                        {
                            CopyTask(fsi.FullName, destName);
                        }
                    }                
            }
        }
    
        /// <summary>
        /// 拷贝整个文件夹
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destinationPath"></param>
        public static void CopyDirectory(string sourcePath, string destinationPath)
        {
            DirectoryInfo info = new DirectoryInfo(sourcePath);
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }
            
            foreach(FileSystemInfo fsi in info.GetFileSystemInfos())
            {
                if (fsi.FullName != destinationPath)   //避免死循环
                {
                    string destName = Path.Combine(destinationPath, fsi.Name);
                    if (fsi is System.IO.FileInfo) //子文件
                    {
                        System.IO.File.Copy(fsi.FullName, destName, true);
                    }
                    else  //子文件夹
                    {
                        CopyDirectory(fsi.FullName, destName);
                    }
                }
            }                   
        }

        public static bool CheckFrameworkDotNet()
        {
            bool flag = false;
            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                   RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\"))
            {
                foreach (string versionKeyName in ndpKey.GetSubKeyNames())
                {
                    if (versionKeyName.StartsWith("v"))
                    {

                        if (string.Compare(versionKeyName.Substring(0, 2), "v4") < 0)
                            continue;
                        RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
                        string name = (string)versionKey.GetValue("Version", "");

                        if (name != "")
                        {
                            string ver = name.Substring(0, 3).ToLower();
                            double v = double.Parse(ver);
                            if (v > 4.5 || Math.Abs(v - 4.5) < 1e-6)
                            {
                                return true;
                            }
                            continue;
                        }
                        foreach (string subKeyName in versionKey.GetSubKeyNames())
                        {
                            RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
                            name = (string)subKey.GetValue("Version", "");
                            if (name != "")
                            {
                                string ver = name.Substring(0, 3).ToLower();
                                double v = double.Parse(ver);
                                if (v > 4.5 || Math.Abs(v - 4.5) < 1e-6)
                                {
                                    return true;
                                }
                                continue;
                            }
                            foreach (string subsubKeyName in subKey.GetSubKeyNames())
                            {
                                RegistryKey subsubKey = versionKey.OpenSubKey(subKeyName);
                                name = (string)subsubKey.GetValue("Version", "");
                                if (name != "")
                                {
                                    string ver = name.Substring(0, 3).ToLower();
                                    double v = double.Parse(ver);
                                    if (v > 4.5 || Math.Abs(v - 4.5) < 1e-6)
                                    {
                                        return true;
                                    }
                                    continue;
                                }
                            }
                        }

                    }
                }

            }
            return flag;
        }
    }
}
