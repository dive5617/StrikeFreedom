using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pTop
{
    public class Time_Help
    {
        public string time_first_path = "hjwpqjx.dat"; //第一次运行的时间
        public string time_last_path = "iqxdfnwsrwp.dat"; //记录每次的关闭时间
        public string dll_path = "PdfSharp.dll";
        public int days = 60; //设置使用限制是60天
        public DateTime expiry_date = DateTime.Parse("2017-12-31");  // 有效期至年底

        public Time_Help()
        {
            this.time_first_path = "hjwpqjx.dat";
            this.time_last_path = "iqxdfnwsrwp.dat";
            this.days = 30;
            this.expiry_date = DateTime.Parse("2017-12-31");
        }
        public Time_Help(int days)
        {
            this.time_first_path = "hjwpqjx.dat";
            this.time_last_path = "iqxdfnwsrwp.dat";
            this.days = days;
        }
        public Time_Help(string edate)
        {
            this.time_first_path = "hjwpqjx.dat";
            this.time_last_path = "iqxdfnwsrwp.dat";
            this.expiry_date = DateTime.Parse(edate);
        }
        public bool file_exist() //time_first_path文件是否存在，存在表示不是第一次运行，如果不存在表示第一次运行，将时间写入进去
        {
            return System.IO.File.Exists(time_first_path) && System.IO.File.Exists(time_last_path);
        }
        //首先判断时间文件与某个dll文件的修改时间一致，如果first_time文件为空，则需要写入时间，则返回true;
        //如果不一致则返回false
        public bool file_dll_time_isEqual() //首先判断时间文件与某个dll文件的修改时间一致，如果first_time文件为空，则需要写入时间，则返回true，如果不一致则返回false
        {
            const int second_error = 10; //忽略10秒的写入文件误差
            StreamReader sr = new StreamReader(this.time_first_path);
            string time = sr.ReadLine();
            sr.Close();
            DateTime first_time = System.IO.File.GetLastAccessTime(this.time_first_path);
            DateTime end_time = System.IO.File.GetLastAccessTime(this.time_last_path);
            DateTime dll_time = System.IO.File.GetLastAccessTime(this.dll_path);
            TimeSpan ts = first_time.Subtract(dll_time);
            TimeSpan ts2 = end_time.Subtract(dll_time);
            //如果是空，并且与pTop.exe修改时间一致，说明是第一次运行，需要将初始时间写入文件
            if (Math.Abs(ts.TotalSeconds) <= second_error && Math.Abs(ts2.TotalSeconds) <= second_error) 
            {
                if (time != null && time != "")
                {
                    return true;
                }
                update_time(DateTime.Now, this.time_first_path);
                update_time(DateTime.Now, this.time_last_path);
                return true;
            }
            //update_time(DateTime.Now, this.time_first_path); //delete
            //update_time(DateTime.Now, this.time_last_path); //delete
            return false; //return false
        }

        public bool is_OK() //如果当前时间比文件存的时间大并且，，则返回true，否则返回false
        {
            DateTime dt_now = DateTime.Now;
            DateTime dt_first = get_time(this.time_first_path);
            DateTime dt_last = get_time(this.time_last_path);
            // 有效期不能超过days天
            //if (DateTime.Compare(dt_now, dt_last) > 0)  // 粗略确保没有修改系统时间
            //{
            //    TimeSpan ts = dt_now.Subtract(dt_first);
            //    if (ts.Days > days)
            //        return false;
            //    return true;
            //}
            // 有效期至年底
            if(DateTime.Compare(dt_now, dt_last) > 0 && DateTime.Compare(this.expiry_date, dt_now) >= 0)
            {
                return true;
            }
            return false;
        }
        public DateTime get_time(string path)
        {
            StreamReader sr = new StreamReader(path);
            string time = decrypt(sr.ReadLine());
            sr.Close();
            string[] strs = time.Split(' ');
            int year = int.Parse(strs[0]);
            int month = int.Parse(strs[1]);
            int day = int.Parse(strs[2]);
            int hour = int.Parse(strs[3]);
            int minute = int.Parse(strs[4]);
            int second = int.Parse(strs[5]);
            return new DateTime(year, month, day, hour, minute, second);
        }
        public void update_time(DateTime time, string path)
        {
            StreamWriter sw = new StreamWriter(path);
            string information = time.Year + " " + time.Month + " " + time.Day + " " + time.Hour + " " + time.Minute + " " + time.Second;
            sw.WriteLine(encrypt(information));
            sw.Flush();
            sw.Close();
        }
        private string encrypt(string information) //加密
        {
            string res = "";
            for (int i = 0; i < information.Length; ++i)
            {
                res += (char)(information[i] + 'a' - 2);
            }
            return res;
        }
        private string decrypt(string information) //解密
        {
            string res = "";
            for (int i = 0; i < information.Length; ++i)
            {
                res += (char)(information[i] - 'a' + 2);
            }
            return res;
        }
    }
}
