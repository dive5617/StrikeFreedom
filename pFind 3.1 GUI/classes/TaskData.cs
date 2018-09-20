using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pFind.classes
{
    public enum StepOptions : int
    { 
        Data_Extraction=1,
        Identification=2,
        Result_Filter=3,
        Quantitation=4
    }


    public class TaskData:INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }


        private string taskpath;

        public string Taskpath
        {
            get { return taskpath; }
            set 
            {
                if (value != taskpath)
                {
                    taskpath = value;
                    NotifyPropertyChanged("Taskpath");
                }
            }
        }

        private string taskname;

        public string Taskname
        {
            get { return taskname; }
            set 
            {
                if (value != taskname)
                {
                    taskname = value;
                    NotifyPropertyChanged("Taskname");
                }
            }
        }

        private int statusindex;

        public int Statusindex
        {
            get { return statusindex; }
            set
            {
                if (value != statusindex)
                {
                    statusindex = value;
                    NotifyPropertyChanged("Statusindex");
                }
            }
        }

        private string status;

        public string Status
        {
            get { return status; }
            set
            {
                if (value != status)
                {
                    status = value;
                    NotifyPropertyChanged("Status");
                }
            }
        }

        private int progress;

        public int Progress
        {
            get { return progress; }
            set
            {
                if (value != progress)
                {
                    progress = value;
                    NotifyPropertyChanged("Progress");
                }
            }
        }

        private string progressText;

        public string ProgressText
        {
            get { return progressText; }
            set
            {
                if (value != progressText)
                {
                    progressText = value;
                    NotifyPropertyChanged("ProgressText");
                }
            }
        }

        private DateTime start_time;

        public DateTime Start_time
        {
            get { return start_time; }
            set
            {
                if (value != start_time)
                {
                    start_time = value;
                    NotifyPropertyChanged("Start_time");
                }
            }
        }

        private string run_time;

        public string Run_time
        {
            get { return run_time; }
            set
            {
                if (value != run_time)
                {
                    run_time = value;
                    NotifyPropertyChanged("Run_time");
                }
            }
        }

        private int stepFrom;

        public int StepFrom
        {
            get { return stepFrom; }
            set
            {
                if (value != stepFrom)
                {
                    stepFrom = value;
                    NotifyPropertyChanged("StepFrom");
                }
            }
        }
    


        public TaskData() { }


    }
}
