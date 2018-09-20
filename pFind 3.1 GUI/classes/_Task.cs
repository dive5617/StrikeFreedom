using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pFind.classes
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
        
        private string task_name;

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
         
        private string path="unsaved";   //binding

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

        private bool check_ok = true;     //whether everything is ok

        public bool Check_ok
        {
            get { return check_ok; }
            set { check_ok = value; }
        }
        /*
        bool isFirst = true;    //whether the task is just loaded
        public bool IsFirst
        {
            get { return isFirst; }
            set { isFirst = value; }
        }*/

        public _Task() { }

        public _Task(string _task_name, string _path)
        {
            this.task_name = _task_name;
            this.path = _path;
        }

        public _Task(string _task_name,string _path,bool _check_ok)
        {
            this.task_name = _task_name;
            this.path = _path;
            this.check_ok = _check_ok;            
        }

        public void Clear()
        {
            this.task_name = "";
            this.path = "";
            this.check_ok = true;
            this.t_file.Reset();
            this.t_search.Reset();
            this.t_filter.Reset();
            this.t_quantitation.Reset();
        }

        private File t_file = new File();
        public File T_File
        {
            get { return t_file; }
            set { t_file = value; }
        }
        private SearchParam t_search = new SearchParam();

        public SearchParam T_Search
        {
            get { return t_search; }
            set { t_search = value; }
        }
        private FilterParam t_filter = new FilterParam();

        public FilterParam T_Filter
        {
            get { return t_filter; }
            set { t_filter = value; }
        }
        private QuantitationParam t_quantitation = new QuantitationParam();

        public QuantitationParam T_Quantitation
        {
            get { return t_quantitation; }
            set { t_quantitation = value; }
        }

        private MS2Quant t_ms2quant = new MS2Quant();

        public MS2Quant T_MS2Quant
        {
            get { return t_ms2quant; }
            set { t_ms2quant = value; }
        }


        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != this.GetType()) { return false; }
            _Task t = obj as _Task;
            if ((System.Object)t == null)
            {
                return false;
            }
            if (this.task_name!=t.task_name||this.path!=t.path||this.check_ok!=t.check_ok) { return false; }
            if (!this.t_file.Equals(t.t_file)) { return false; }
            if (!this.t_search.Equals(t.t_search)) { return false; }
            if (!this.t_filter.Equals(t.t_filter)) { return false; }
            if (!this.t_quantitation.Equals(t.t_quantitation)) { return false; }
            if (!this.t_ms2quant.Equals(t.t_ms2quant)) { return false; }
            return true;
        }

        public override int GetHashCode()
        {
            int hc = this.task_name.GetHashCode() + this.path.GetHashCode()+this.check_ok.GetHashCode()
                + this.t_file.GetHashCode() + this.t_search.GetHashCode()
                + this.t_filter.GetHashCode() + this.t_quantitation.GetHashCode() + this.t_ms2quant.GetHashCode();
            return base.GetHashCode();
        }

    }
}
