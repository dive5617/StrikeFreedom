using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using pTop.classes;

namespace pTop
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        Advanced s;
        string defaultfilePath;
        public SettingsWindow(Advanced s)
        {
            InitializeComponent();
            this.s = s;
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
        }
        
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            s.Thread_Num=int.Parse(this.tbthreadnum.Text);
            s.Output_Path = this.tboutputpath.Text;
            s.WriteSettings();
            this.DialogResult = true;
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
