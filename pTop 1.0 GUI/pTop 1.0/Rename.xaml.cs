using pTop.classes;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace pTop
{
    /// <summary>
    /// Rename.xaml 的交互逻辑
    /// </summary>
    public partial class Rename : Window
    {
        _Task tsk;

        public Rename(ref _Task _task)
        {
            InitializeComponent();
            this.TaskReName.Text = _task.Task_name;
            this.TaskReName.Focus();
            this.TaskReName.SelectAll();
            tsk = _task;
        }

        private void SaveNewName(object sender, RoutedEventArgs e)
        {
            save_task_name();
            this.DialogResult = true;
        }

        private void save_task_name()
        {
            if (this.TaskReName.Text == "")
            {
                System.Windows.MessageBox.Show("The task name cannot be an empty string.");
            
                return;
            }
            string old_path = tsk.Path;
            string new_path = old_path;
            int p = old_path.Length-1;
            while(old_path[p]=='\\')  p--;
            new_path = old_path.Substring(0, p + 1);            
            new_path = new_path.Substring(0, new_path.LastIndexOf('\\'));
            new_path += "\\" + this.TaskReName.Text + "\\";
            try
            {
                if (old_path != new_path && Directory.Exists(new_path))
                {
                    System.Windows.MessageBox.Show("The new task name is already existed.");
                    this.TaskReName.Text = tsk.Task_name;
                    return;
                }
                string old_name = tsk.Task_name;
                tsk.Task_name = this.TaskReName.Text.ToString();
                tsk.Path = new_path;
                if (Directory.Exists(old_path) && old_path != new_path)
                {
                    System.IO.File.Delete(old_path + "\\" + old_name + ".tsk");
                    System.IO.Directory.Move(old_path, new_path);
                    Factory.Create_Run_Instance().SaveParams(tsk);
                }
            }
            catch (Exception exe)
            {
                System.Windows.MessageBox.Show(exe.Message, "warning");
                //System.Windows.MessageBox.Show("You have open the folder or parent folder! Please close them!");
            }
        }

        private void Save_NewName(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                save_task_name();
                this.DialogResult = true;
            }
        }


    }
}
