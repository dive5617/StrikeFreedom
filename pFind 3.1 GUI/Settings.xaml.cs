using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Windows.Forms;
using System.ComponentModel;
using pFind.classes;
using System.Management;
using System.Collections.ObjectModel;
namespace pFind
{
    /// <summary>
    /// NewTask.xaml 的交互逻辑
    /// </summary>
    /// 
    //定义委托

  
    public partial class SettingsWindow : Window
    {
        Advanced s;
        Advanced pre;   // 记录之前的信息
        string defaultfilePath;
        ObservableCollection<string> threadnum = new ObservableCollection<string>();
        public SettingsWindow(Advanced s)
        {
            InitializeComponent();
            int cpuNum = Environment.ProcessorCount;
            for (int i = 1; i <= cpuNum; i++)
            {
                threadnum.Add(i.ToString());
            }
            this.tbthreadnum.ItemsSource = threadnum;
            this.s = s;
            this.pre = s;
            defaultfilePath = s.Output_Path;
            System.Windows.Data.Binding binding = new System.Windows.Data.Binding();
            binding.Source = s;
            binding.Path = new PropertyPath("Thread_Num");
            binding.Mode = BindingMode.OneWay;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            //binding.NotifyOnValidationError = true;
            //binding.ValidationRules.Add(new ThreadValidationRule());
            this.tbthreadnum.SetBinding(System.Windows.Controls.ComboBox.TextProperty, binding);
            //this.tbthreadnum.AddHandler(Validation.ErrorEvent, new RoutedEventHandler(this.ValidationError));

            System.Windows.Data.Binding binding1 = new System.Windows.Data.Binding();
            binding1.Source = s;
            binding1.Path = new PropertyPath("Output_Path");
            binding1.Mode = BindingMode.OneWay;
            binding1.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            this.tboutputpath.SetBinding(System.Windows.Controls.TextBox.TextProperty, binding1);
            if (s.Output_Path.Length > 0)
            {
                string dis = s.Output_Path.Substring(0, 2);
                getFreeSpace(dis);
            }
        }
        
        void getFreeSpace(string dis)
        {
            ManagementObject disk = new ManagementObject(string.Format("win32_logicaldisk.deviceid='{0}'", dis));
            double freespace = Math.Round(Convert.ToDouble(disk["FreeSpace"]) / (1024 * 1024 * 1024),1);
            string distip = "Available Space on Drive " + dis[0] + " :  " + freespace.ToString() + " G";
            this.diskTip.Foreground = Brushes.Blue;
            this.diskTip.Text = distip;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string drive = this.tboutputpath.Text.Substring(0,3);
            if (System.IO.Directory.Exists(drive))
            {
                s.Thread_Num = int.Parse(this.tbthreadnum.Text);
                s.Output_Path = this.tboutputpath.Text;
                s.WriteSettings();
                this.DialogResult = true;
            }
            else 
            {
                string distip = "Please select a valid working directory.";
                this.diskTip.Foreground = Brushes.Red;
                this.diskTip.Text = distip;
            }
            
        }

        void ValidationError(object sender, RoutedEventArgs e)
        {
            if (Validation.GetErrors(this.tbthreadnum).Count > 0)
            {
                this.tbthreadnum.ToolTip = Validation.GetErrors(this.tbthreadnum)[0].ErrorContent.ToString();
            }
        }

        private void Button_Click_Browse(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog f_dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (defaultfilePath != "")
            {
                //设置此次默认目录为上一次选中目录  
                f_dialog.SelectedPath = defaultfilePath;
            }
            System.Windows.Forms.DialogResult dresult = f_dialog.ShowDialog();
            if (dresult == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            if (dresult == System.Windows.Forms.DialogResult.OK)
            {
                //记录选中的目录  
                defaultfilePath = f_dialog.SelectedPath;
            }
            this.tboutputpath.Text = f_dialog.SelectedPath;
            getFreeSpace(this.tboutputpath.Text.Substring(0,2));
        }

        private void CheckCpuNum(object sender, SelectionChangedEventArgs e)
        {
            //try
            //{
                int cpuNum = Environment.ProcessorCount;
                int num = this.tbthreadnum.SelectedIndex+1;
                if (num > cpuNum)
                {
                    this.threadNumWarn.Text = Message_Help.THREAD_NUM_WARNING;                    
                }
                else 
                {
                    this.threadNumWarn.Text = "";
                }
            //}
            //catch(Exception ex)
            //{
            //   System.Windows.MessageBox.Show(ex.Message);
            //}
        }

        private void tboutputpath_TextChanged(object sender, TextChangedEventArgs e)
        {
            string drive = this.tboutputpath.Text.Substring(0, 2);
            if (System.IO.Directory.Exists(drive))
            {
                getFreeSpace(drive);
            }
            else
            {
                string distip = "Please select a valid working directory.";
                this.diskTip.Foreground = Brushes.Red;
                this.diskTip.Text = distip;
            }         
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if(pre.Output_Path == "" || pre.Output_Path == null || !Directory.Exists(pre.Output_Path))
            {
                string distip = "Click \"OK\" to save your changes.";
                this.diskTip.Foreground = Brushes.Red;
                this.diskTip.Text = distip;
                e.Cancel = true;
            }
        }
#region
        //private void Browse_Path(object sender,RoutedEventArgs e)
        //{
        //    FolderBrowserDialog f_dialog = new FolderBrowserDialog();
        //    DialogResult dresult = f_dialog.ShowDialog();
        //    if (dresult == System.Windows.Forms.DialogResult.Cancel)
        //    {
        //        return;
        //    }
        //    string f_Dir = f_dialog.SelectedPath;
        //    f_Dir += "\\";
        //    this.newTaskPath.Text = f_Dir;
        //    new_task_path = f_Dir;
        //}
        ///**添加一个新任务**/
        /////<summary></summary>
        /////<param name="sender">事件源</param>
        /////<param name="e">事件对象</param>
        /////
        //private void Add_New_Task(object sender, RoutedEventArgs e)
        //{
        //    new_task_name = this.newTaskName.Text.ToString();
        //    string pathName = new_task_path + new_task_name;
        //    if (Directory.Exists(pathName))
        //    {
        //        String tmp = "The Directory Already Contains An Item Named '" + new_task_name + "'";
        //        System.Windows.Forms.MessageBox.Show(tmp,"pFind",MessageBoxButtons.OK,MessageBoxIcon.Error);
        //    }
        //    else {
        //        Directory.CreateDirectory(pathName);
                
        //        if (ChangeTextEvent != null) {
        //            ChangeTextEvent(new_task_name);
        //            this.Close();
        //        }
        //    }
            
        //}
        ///**取消添加，关闭窗口**/
        //private void Cancel_New_Task(object sender, RoutedEventArgs e)
        //{
        //    this.Close();
        //}

        #endregion
    }

    public class ThreadValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            int n= 0;
            if (int.TryParse(value.ToString(), out n))
            {
                if (n >= 1 && n <= 32)
                {
                    return new ValidationResult(true, null);
                }
            }

            return new ValidationResult(false, "Invalid Thread Number.");
        }
    }
}
