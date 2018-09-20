using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pTop.classes
{
    public class _Task : INotifyPropertyChanged 
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        
        private string task_name="";

        public string Task_name
        {
            get { return task_name; }
            set
            {
                if (value != task_name)
                {
                    task_name = value;
                    NotifyPropertyChanged("Task_name");
                }
            }
        }

        private string path = "unsaved";   //binding

        public string Path
        {
            get { return path; }
            set
            {
                if (value != path)
                {
                    path = value;
                    NotifyPropertyChanged("Path");
                }
            }
        }

        bool check_ok = true;     //whether everything is ok

        public bool Check_ok
        {
            get { return check_ok; }
            set { check_ok = value; }
        }

        private File t_file = new File();
        public File T_File
        {
            get { return t_file; }
            set { t_file = value; }
        }

        private Identification t_identify = new Identification();
        public Identification T_Identify
        {
            get { return t_identify; }
            set { t_identify = value; }
        }
        public _Task() { }

        public _Task(string _task_name, string _path, bool _check_ok)
        {
            this.task_name = _task_name;
            this.path = _path;
            this.check_ok = _check_ok;
        }
    }
}
