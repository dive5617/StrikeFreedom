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
using pFind.classes;
namespace pFind
{
    /// <summary>
    /// NewTask.xaml 的交互逻辑
    /// </summary>
    /// 
 
    public partial class NewTask : Window
    {
        private String new_task_name;

        public String New_task_name
        {
            get { return new_task_name; }
            set { new_task_name = value; }
        }
        private String new_task_path;

        public String New_task_path
        {
            get { return new_task_path; }
            set { new_task_path = value; }
        }
        //定义委托
        public delegate void ChangeNewTaskHandler(String[] taskStr);


        //定义事件
        public event ChangeNewTaskHandler ChangeTextEvent;

        public NewTask(string default_task_name, string default_path)
        {
            InitializeComponent();
            new_task_name = default_task_name;
            new_task_path = default_path;
            this.newTaskName.Text = new_task_name;
            this.newTaskPath.Text = new_task_path;
        }
        /**获取Task的存放目录**/
        private void Browse_Path(object sender,RoutedEventArgs e)
        {
            FolderBrowserDialog f_dialog = new FolderBrowserDialog();
            f_dialog.SelectedPath = new_task_path;
            DialogResult dresult = f_dialog.ShowDialog();
            if (dresult == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            string f_Dir = f_dialog.SelectedPath;
            f_Dir += "\\";
            this.newTaskPath.Text = f_Dir;
            new_task_path = f_Dir;
        }
        /**添加一个新任务**/
        ///<summary></summary>
        ///<param name="sender">事件源</param>
        ///<param name="e">事件对象</param>
        ///
        private void Add_New_Task(object sender, RoutedEventArgs e)
        {
            new_task_name = this.newTaskName.Text.ToString();
            string pathName = new_task_path + new_task_name + '\\';
            if(new_task_name == "" || new_task_name == null)
            {
                String tmp = "The Task Name cannot be empty!!!";
                System.Windows.Forms.MessageBox.Show(tmp, "pFind", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (Directory.Exists(pathName))
            {
                String tmp = "The Directory Already Contains An Item Named '" + new_task_name + "'";
                System.Windows.Forms.MessageBox.Show(tmp,"pFind",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
            else {
                //Directory.CreateDirectory(pathName);
                
                if (ChangeTextEvent != null) {
                    string[] task = {new_task_name, pathName};
                    ChangeTextEvent(task);
                    this.DialogResult = true;
                    this.Close();
                }
            }
            
        }
        /**取消添加，关闭窗口**/
        private void Cancel_New_Task(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }

}
