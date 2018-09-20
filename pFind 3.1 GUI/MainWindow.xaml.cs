using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Xps;
using System.ComponentModel;
using Microsoft.Win32;
using System.Windows.Media.Animation;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Management;
using pFind.classes;
using WPF.ListViewDrag.ServiceProviders.UI;
using System.IO.Packaging;
using System.Windows.Xps.Packaging;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
//using System.Windows.Forms;
namespace pFind
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>

    public partial class MainWindow : Window
    {
        public enum Unit : int
        { 
            Da=0,
            ppm=1
        }
        public enum TabOption : int
        { 
            MS_Data=0,
            Identification=1,
            Quantitation=2,
            Summary=3
        }
        public enum Section : int
        {
            MS_Data = 0,
            Search = 1,
            Filter=2,
            Quantitation = 3
        }
        public enum Step : int
        { 
            Data_Extraction=1,
            Identification=2,
            Result_Filter=3,
            Quantitation=4
        }
        public class _DataFile
        {
            public int DFnum{get;set;}
            public double DFsize{get;set;}
            
        }

        public enum TaskStatus : int
        {
            removed = 0,
            waiting = 1,
            running = 2,
            done = 3
        }

        //pFind.WelcomeWindow welcome;
        public Advanced settings = new Advanced();

        //StartPage界面
        public StartPage startPage;
        //用来显示动画效果
        public DispatcherTimer check_time_timer = null; //每隔12个小时运行一次，检测pFind使用是否超过使用日期限制
        public DispatcherTimer welcome_timer = null;
        Process welcome_process = null;

        //打开文件对话框默认目录
        string defaultfilePath = "";
        public int task_count = 0;         //number of tasks
        string output_path = "";   //default output path
        int global_task_index = 1;
        int pre_tab_index = -1;     //last open tabControl index
        bool firstload = true;    //whether is first loaded

        // 任务树
        ObservableCollection<_Task> tasklist = new ObservableCollection<_Task>();
        // 运行队列
        ObservableCollection<TaskData> runtasklist = new ObservableCollection<TaskData>();
        int removed_num = 0;
        ListViewDragDropManager<TaskData> dragMgr;

        private BackgroundWorker bgworker1;
        private delegate void SimpleDelegate();

        private SolidColorBrush highlightBrush = new SolidColorBrush(Colors.Orange);
        private SolidColorBrush blueBrush = new SolidColorBrush(Colors.Blue);
        private SolidColorBrush normalBrush = new SolidColorBrush(Colors.White);

        private ObservableCollection<DataGridRow> selected_rows = new ObservableCollection<DataGridRow>();
        int current_task_index = -1; 
        _Task current_task = new _Task();
        bool current_taskname_changed = false;
        bool current_taskpath_changed = false;
        public _Task Current_Task
        {
            get { return current_task; }
            set { current_task = value; }
        }

        File _file = new File();
        public File _File
        {
            get { return _file; }
            set { _file = value; }
        }

        SearchParam _search = new SearchParam();
        public SearchParam _Search
        {
            get { return _search; }
            set { _search = value; }
        }

        FilterParam _filter = new FilterParam();
        public FilterParam _Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        QuantitationParam _quantitation = new QuantitationParam();
        public QuantitationParam _Quantitation
        {
            get { return _quantitation; }
            set { _quantitation = value; }
        }

        MS2Quant _ms2quant = new MS2Quant();

        public MS2Quant MS2Quant
        {
            get { return _ms2quant; }
            set { _ms2quant = value; }
        }

                  
        ObservableCollection<String> display_mods = new ObservableCollection<String>(); //在修饰列表中选中的修饰         
        
        ObservableCollection<String> display_labels = new ObservableCollection<String>(); //标记列表中选过后的标记
        //ObservableCollection<string> display_labelTypes = new ObservableCollection<string>();  // 待显示的标记类型

       
        //检查日期是否超过使用限制
        private void check_time(object sender, EventArgs e) 
        {
            Time_Help time_help = new Time_Help();
            bool flag = time_help.is_OK();
            if (flag)
            {
                time_help.update_time(DateTime.Now, time_help.time_last_path);  //记录每次的关闭时间
            }
            else
            {
                MessageBox.Show(Message_Help.DATE_OVER);
                Application.Current.Shutdown();
            }
        }
        //运行pFind的时候进行时间限制的检查
        private bool time_start() 
        {
            //Time_Help time_help = new Time_Help();
            //if (!time_help.file_exist()) 
            //{
            //    MessageBox.Show(Message_Help.DATE_FILE_NOT_EXIST);
            //    return false;
            //}
            //if (!time_help.file_dll_time_isEqual())
            //{
            //    MessageBox.Show(Message_Help.DATE_FILE_INVALID);
            //    return false;
            //}
            check_time_timer = new DispatcherTimer();
            check_time_timer.Interval = TimeSpan.FromSeconds(12*60*60); //12个小时，运行一次检测日期是否超过使用限制
            check_time_timer.Tick += check_time;
            check_time_timer.Start();

            //bool flag = time_help.is_OK();
            //if (flag)
            //    time_help.update_time(DateTime.Now, time_help.time_last_path);
            //else
            //{
            //    MessageBox.Show(Message_Help.DATE_OVER);
            //    Application.Current.Shutdown();
            //    return false;
            //}
            return true;
        }

        public MainWindow()
        {

            //if (!time_start())   // commented @2017.03.20
            //{
            //    Application.Current.Shutdown();
            //    this.Close();
            //    return;
            //}
            ProcessStartInfo info = new ProcessStartInfo("TimeController.exe");  // 1 时间错误  2 licence错误  3 成功
            info.UseShellExecute = false;
            Process proBach = Process.Start(info);
            proBach.WaitForExit();
            int returnValue = proBach.ExitCode;
            if (returnValue <= 2)
            {
                Application.Current.Shutdown();
                this.Close();
                return;
            }
            //获得180天的时间，赋值给Time_Help.Days，因为我们的pFind界面、pBuild界面仍然会自身每隔12小时会检查一下时间是否过期，如果不检查，那么用户可以一直打开界面，只要不关就可以一直使用
            Time_Help.Days = returnValue - 3;
           
            startPage = new StartPage();
            //bool flag = startPage.check_license(); // commented @2017.03.20
            //if (!flag)
            //{
            //    this.Close();
            //    return;
            //}
            startPage.add_CrashHandler();
            //崩溃测试
            //int a = 1, b = 0, c;
            //c = a / b;
            //wrm20151030: 检查.Net4.5 + 是否正确安装， 之前是在JustRun时才检查
            if (!ConfigHelper.CheckFrameworkDotNet())
            {
                MessageBox.Show(Message_Help.DOT_NET_ERROR, "pFind", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            if (Application.Current.Properties["ArbitraryArgName"] == null)
            {
                this.Hide();
                welcome_timer = new DispatcherTimer();
                welcome_timer.Interval = TimeSpan.FromSeconds(3);
                welcome_timer.Tick += initial_window;
                welcome_timer.IsEnabled = false;
                welcome_timer.Start();

                welcome_process = Process.Start("Welcome.exe", "pFind");
            }
            else
            {
                InitializeComponent();
                InitializeContent();
                this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            }     
        }

        
        private void initial_window(object sender,EventArgs e)
        {
            //若手动关闭了欢迎界面，则正常开始主程序
            if (!welcome_process.HasExited)
            {
                welcome_process.Kill();
                welcome_process = null;
                welcome_timer.Stop();
                welcome_timer = null;
            }
            this.Show();
            InitializeComponent();
            InitializeContent();
            #region Todo
            //20141013：首页tabItem待添加
            #endregion
            this.recent_tasks.ItemsSource = startPage.recent_taskInfo;
            if (startPage.recent_taskInfo.Count == 0)
                this.recent_tasks.Visibility = Visibility.Collapsed;
            settingWorkspace();
        }

        private void settingWorkspace()
        {
            if (settings.Output_Path == "" || settings.Output_Path == null || !System.IO.Directory.Exists(settings.Output_Path))
            {
                //[wrm] comment out by wrm 2015.12.09.   As users can set these params through the SettingWindow
                //[wrm] show but not create the default directory 
                settings.defaultAdvanced();   
                SettingsWindow dlg = new SettingsWindow(settings);
                bool dresult = (bool)dlg.ShowDialog();
            }           
        }


        private void home_switcher(int to)
        {
            switch(to)
            {
                case 0:    //回到首页
                    this.recent_tasks.ItemsSource=ConfigHelper.load_all_recent_taskPath(Advanced.recent_tasks_file_path);
                    if (this.recent_tasks.Items.Count == 0)
                        this.recent_tasks.Visibility = Visibility.Collapsed;
                    else
                        this.recent_tasks.Visibility = Visibility.Visible;
                    this.left_grid1.Visibility = Visibility.Collapsed;
                    this.right_grid1.Visibility = Visibility.Collapsed;
                    this.left_grid0.Visibility = Visibility.Visible;
                    this.right_grid0.Visibility = Visibility.Visible;
                    break;
                case 1:    //到配置界面
                       if (this.left_grid1.Visibility == Visibility.Collapsed)
                           this.left_grid1.Visibility = Visibility.Visible;
                       if (this.right_grid1.Visibility == Visibility.Collapsed)
                           this.right_grid1.Visibility = Visibility.Visible;
                       if (this.left_grid0.Visibility == Visibility.Visible)
                           this.left_grid0.Visibility = Visibility.Collapsed;
                       if (this.right_grid0.Visibility == Visibility.Visible)
                           this.right_grid0.Visibility = Visibility.Collapsed;
                       break;
            }
        }

        private void load_recent_task(object sender, SelectionChangedEventArgs e)
        {
            ConfigHelper.TaskInfo task_info = this.recent_tasks.SelectedItem as ConfigHelper.TaskInfo;
            if (task_info == null)
                return;
            string task_path = task_info.tpath;
            //查看task_path是否存在，如果不存在，则提示用户不存在该文件夹，该删除该路径
            if (!Directory.Exists(task_path)||!System.IO.File.Exists(task_path+"\\"+task_info.tname+".tsk"))
            {
                MessageBox.Show(Message_Help.FOLDER_NOT_EXIST);
                ConfigHelper.TaskInfo ti = new ConfigHelper.TaskInfo("", task_path);
                startPage.recent_taskInfo.Remove(ti);
                this.recent_tasks.ItemsSource = startPage.recent_taskInfo;
                ConfigHelper.write_recent_task(Advanced.recent_tasks_file_path, startPage.recent_taskInfo);
                return;
            }
            open_task_by_path(task_path, true);
        }
        
        private void Window_Closing_1(object sender, CancelEventArgs e)
        {
            try
            {
                if (current_task != null && current_task_index > -1)
                {
                    if (taskChanged())
                    {
                        System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Do you want to save the changes of " + current_task.Task_name + " before exit?", "pFind", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                        if (result == System.Windows.MessageBoxResult.Yes)
                        {
                            _task_save();
                        }
                        else if (result == System.Windows.MessageBoxResult.Cancel)
                        {
                            e.Cancel = true;
                        }
                    }
                }
            }
            catch(Exception exe)
            {
                MessageBox.Show(exe.Message);
            }
        }

        private void Window_Closed_1(object sender, EventArgs e)
        {
            KillChildrenProcess(System.Diagnostics.Process.GetCurrentProcess().Id);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(VisualTreeHelper.GetChildrenCount(b).ToString());
            //DependencyObject d = VisualTreeHelper.GetChild(b, 0);
            //Button dropDownButton = LogicalTreeHelper.FindLogicalNode(d, "DropDownButton") as Button;
            //dropDownButton.Click += new RoutedEventHandler(dropDownButton_Click);

            if (Application.Current.Properties["ArbitraryArgName"] == null)
            {
                
            }
            else
            {
                string task_path = Application.Current.Properties["ArbitraryArgName"] as string;
                Application.Current.Properties["ArbitraryArgName"] = null;
                open_task_by_path(task_path, true);
                settingWorkspace();
            }            
            
        }

        void dropDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.dropdownBtn.ContextMenu != null)
            {
                this.dropdownBtn.ContextMenu.PlacementTarget = this.dropdownBtn;
                this.dropdownBtn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                this.dropdownBtn.ContextMenu.IsOpen = true;
            }
            e.Handled = true;
        }
        void InitializeContent()
        {
            if (defaultfilePath == "") 
            {
                defaultfilePath = _get_output_path();
            }
            ConfigHelper.get_mods();
            show_DB();
            show_enzyme();
            ConfigHelper.get_labels();
            setDataSource();
        }

        public void setDataSource()
        {
            this.FileTab.DataContext = _file;
            this.search.DataContext = _search;
            this.filter.DataContext = _filter;
            this.V_MS1Quant.DataContext = _quantitation;
            this.V_MS2Quant.DataContext = _ms2quant;
        }
        private void recent_task_menu_FocusCHD(object sender, MouseButtonEventArgs e)
        {
            //MessageBox.Show("OK");
            Initial_Recent_Task_Menu();
        }
        //初始化Recent菜单，将最近打开的任务列表放在菜单中
        private void Initial_Recent_Task_Menu()
        {
            this.recent_task_menu.Items.Clear();
            ObservableCollection<string> recent_task_path = ConfigHelper.load_recent_taskPaths(Advanced.recent_tasks_file_path);
            for (int i = 0; i < recent_task_path.Count; ++i)
            {
                MenuItem mi = new MenuItem();
                mi.Header = recent_task_path[i];
                mi.PreviewMouseDown += (s, e) =>
                {
                    string task_path = mi.Header as string;
                    int index = task_path.IndexOf(' ');
                    task_path = task_path.Substring(index + 1);
                    open_task_by_path(task_path,false);
                };
                this.recent_task_menu.Items.Add(mi);
            }
        }
        private void New_Command(object sender, ExecutedRoutedEventArgs e) //快捷键New Ctrl+N
        {
                this.new_task();
        }
        private void Open_Command(object sender, ExecutedRoutedEventArgs e) //快捷键Open Ctrl+O
        {                 
                this.Open_Task(true);                      
        }

        private void Run_Command(object sender, ExecutedRoutedEventArgs e)
        {
            StartSearch(sender,e);
        }
        private void Rename_Command(object sender,ExecutedRoutedEventArgs e)
        {
            if (this.TaskTree.SelectedItem != null)
            {
                TreeViewItem tvi = (TreeViewItem)this.TaskTree.SelectedItem;
                if (tvi.Header is string)
                {
                    tvi = tvi.Parent as TreeViewItem;
                }
                _rename_task(tvi);
            }
        }
        private void CallpBuild(object sender, RoutedEventArgs e)
        {
            if (this.TaskTree.SelectedItem != null)
            {
                TreeViewItem tvi = (TreeViewItem)this.TaskTree.SelectedItem;
                while ((tvi.Parent as TreeViewItem) != null)
                {
                    tvi = tvi.Parent as TreeViewItem;
                }
                int index = this.TaskTree.Items.IndexOf(tvi);
                string tspath = tasklist[index].Path;
                if (System.IO.File.Exists(tspath + @"\result\pFind.summary"))
                {
                    try
                    {
                        System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                        string p = tspath;
                        if (p[p.Length - 1] == '\\')
                        {
                            p = p.Substring(0, p.Length - 1);
                        }
                        psi.Arguments = "\"" + p + "\"";
                        psi.FileName = "\"" + ConfigHelper.startup_path + "\\pBuild.exe" + "\"";
                        System.Diagnostics.Process.Start(psi);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show(Message_Help.RESULT_FILE_OK);
                }
            }
        }
        #region Todo
        //添加链接
        #endregion
        public void About_us(object sender, RoutedEventArgs e)
        {
            
            System.Diagnostics.Process.Start("http://pfind.ict.ac.cn/");           
        }

        public void CloseWindow(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        
        /*************************************任务栏交互逻辑**********************************************/
        //展开任务节点
        private void TaskContent_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = sender as TreeViewItem;
            if (tvi.Header.ToString().Equals("File")) 
            {
                this.tab.SelectedIndex = 0;
            }
            else if (tvi.Header.ToString().Equals("Identification"))
            {
                this.tab.SelectedIndex = 1;
            }
            else if (tvi.Header.ToString().Equals("Quantitation"))
            {
                this.tab.SelectedIndex = 2;
            }
            else if (tvi.Header.ToString().Equals("Summary"))
            {
                this.tab.SelectedIndex = 3;
            }
        }
        //重命名任务
        private void _rename_task(TreeViewItem tvi)
        {
            StackPanel sp = tvi.Header as StackPanel;
            TextBlock tbl = sp.Children[0] as TextBlock;
            if (tbl != null)
            {
                tbl.Visibility = Visibility.Collapsed;
                TextBox tb = sp.Children[1] as TextBox;
                tb.Visibility = Visibility.Visible;
                tb.Text = tbl.Text;
                tb.Focus();
                tb.SelectAll();
            }
        }
        private void Open_Task_Dir(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            ContextMenu cm = mi.Parent as ContextMenu;
            TreeViewItem tvi = ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(mi)) as TreeViewItem;
            while ((tvi.Parent as TreeViewItem) != null)
            {
                tvi = tvi.Parent as TreeViewItem;
            }
            int index = this.TaskTree.Items.IndexOf(tvi);
            string tspath = tasklist[index].Path;
            if (System.IO.Directory.Exists(tspath))
            {
                System.Diagnostics.Process.Start("explorer.exe", tspath);
            }
            else
            {
                System.Windows.MessageBox.Show("Sorry, " + "\"" + tspath + "\"" + " is not existed. Please make sure that the path is valid, or the task has been successfully saved.");
            }
        }
        private void Rename_Task(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
           
            ContextMenu cm = mi.Parent as ContextMenu;
            TreeViewItem tvi=ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(mi)) as TreeViewItem;
            _rename_task(tvi);
        }
        private void Save_NewTaskName(object sender, RoutedEventArgs e)
        {
             TextBox tb = sender as TextBox;
             if (tb != null)
             {
                 StackPanel sp = tb.Parent as StackPanel;
                 TextBlock tbl = sp.Children[0] as TextBlock;
                 if (tb.Text == "")
                 {
                     // [wrm] modified by wrm 2015.12.09. 与Windows重命名一致，当输入空白时，什么也不做，保留原有文件名。
                     //System.Windows.MessageBox.Show("The task name cannot be an empty string.");
                     //tb.Text = current_task.Task_name;
                     tb.Visibility = Visibility.Collapsed;
                     tbl.Visibility = Visibility.Visible;
                     return;
                 }
                 string old_path = current_task.Path;
                 string new_path = old_path.Substring(0, old_path.LastIndexOf('\\'));
                 new_path = new_path.Substring(0, new_path.LastIndexOf('\\'));
                 new_path += "\\" + tb.Text + "\\";
                 try
                 {
                     current_task.Task_name = tb.Text.ToString();
                     current_taskname_changed = true;
                     current_task.Path = new_path;
                     if (Directory.Exists(old_path)&&old_path!=new_path)
                     {
                         System.IO.Directory.Move(old_path, new_path);
                         Factory.Create_Run_Instance().JustSaveParams(current_task);
                         ConfigHelper.update_recent_task_byRename(Advanced.recent_tasks_file_path, old_path, new_path);
                     }
                     tbl.Text = tb.Text;
                     tb.Visibility = Visibility.Collapsed;
                     tbl.Visibility = Visibility.Visible;
                 }
                 catch (Exception exe)
                 {
                     current_task.Task_name = tbl.Text.ToString();
                     current_taskname_changed = false;
                     current_task.Path = old_path;
                     tb.Visibility = Visibility.Collapsed;
                     tbl.Visibility = Visibility.Visible;  
                     System.Windows.MessageBox.Show(exe.Message, "warning");
                     //System.Windows.MessageBox.Show("You have open the folder or parent folder! Please close them!");
                 }
             }
        }
        private void TaskName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tb = sender as TextBox;
                if (tb != null)
                {
                    StackPanel sp = tb.Parent as StackPanel;
                    TextBlock tbl = sp.Children[0] as TextBlock;
                    if (tb.Text == "")
                    {
                        // [wrm] modified by wrm 2015.12.09. 与Windows重命名一致，当输入空白时，什么也不做，保留原有文件名。
                        //System.Windows.MessageBox.Show("The task name cannot be an empty string.");
                        //tb.Text = current_task.Task_name;
                        tb.Visibility = Visibility.Collapsed;
                        tbl.Visibility = Visibility.Visible;
                        return;
                    }
                    string old_path = current_task.Path;
                    string new_path = old_path.Substring(0, old_path.LastIndexOf('\\'));
                    new_path = new_path.Substring(0, new_path.LastIndexOf('\\'));
                    new_path += "\\" + tb.Text + "\\";
                    try
                    {
                        if (old_path != new_path && Directory.Exists(new_path))
                        {
                            System.Windows.MessageBox.Show("The new task name is already existed.");
                            tb.Text = current_task.Task_name;
                            return;
                        }
                        string old_name = current_task.Task_name;
                        current_task.Task_name = tb.Text.ToString();
                        current_taskname_changed = true;
                        current_task.Path = new_path;
                        if (Directory.Exists(old_path) && old_path != new_path)
                        {
                            System.IO.File.Delete(old_path + "\\" + old_name + ".tsk");
                            System.IO.Directory.Move(old_path, new_path);
                            Factory.Create_Run_Instance().JustSaveParams(current_task);
                            ConfigHelper.update_recent_task_byRename(Advanced.recent_tasks_file_path,old_path,new_path);
                        }
                        tbl.Text = tb.Text;
                        tb.Visibility = Visibility.Collapsed;
                        tbl.Visibility = Visibility.Visible;
                    }
                    catch (Exception exe)
                    {
                        current_task.Task_name = tbl.Text.ToString();
                        current_taskname_changed = false;
                        current_task.Path = old_path;
                        tb.Visibility = Visibility.Collapsed;
                        tbl.Visibility = Visibility.Visible;
                        System.Windows.MessageBox.Show(exe.Message, "warning");
                        //System.Windows.MessageBox.Show("You have open the folder or parent folder! Please close them!");
                    }
                }
            }
        }
        
        /**新建一个节点**/
        public void New_TreeView(ref _Task _task, string mode)
        {          
            TreeViewItem tvi = new TreeViewItem();
            Binding binding = new Binding();
            binding.Path = new PropertyPath("Path");
            binding.Source = _task;
            ToolTip tip = new ToolTip();
            //tip.Style = (Style)this.FindResource("SimpleTip");
            TextBlock tiptext=new TextBlock();
            //Color col = Color.FromArgb(240, 204, 206, 219);
            //tip.Background = new SolidColorBrush(col);
            tip.Content = tiptext;           
            BindingOperations.SetBinding(tiptext, TextBlock.TextProperty, binding);
            tvi.ToolTip = tip;          
            
            TextBlock tbl = new TextBlock();
            //tbl.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(Rename_Task);     //单击重命名                   

            Binding tsnamebind = new Binding();
            tsnamebind.Path = new PropertyPath("Task_name");
            tsnamebind.Source = _task;
            if (_task.Task_name != "" && _task.Task_name != null)
            {
                BindingOperations.SetBinding(tbl, TextBlock.TextProperty, tsnamebind);
            }
            TextBox tb = new TextBox();
            Color coll = Color.FromArgb(240, 58, 185, 249);
            tb.SelectionBrush = new SolidColorBrush(coll);
            tb.Text = "Task" + (task_count+1).ToString();
            tb.PreviewKeyDown += new KeyEventHandler(TaskName_KeyDown);
            tb.LostFocus += new RoutedEventHandler(Save_NewTaskName);
            tb.Focus();
            tb.SelectAll(); 
             
            StackPanel sp = new StackPanel();
            sp.Children.Add(tbl);
            sp.Children.Add(tb);
            tvi.Header = sp;
            //if (mode.Equals("new"))
            //{
            //    while (Directory.Exists(_get_output_path() + _gen_taskname(global_task_index)))
            //    {
            //        ++global_task_index;
            //    }
            //    tbl.Text = "Task" + (global_task_index).ToString();
            //    tb.Visibility = Visibility.Collapsed;
            //    tbl.Visibility = Visibility.Visible;
            //    ++global_task_index;
            //}
            //else if(mode.Equals("open"))
            //{
                tbl.Text = _task.Task_name;
                tbl.Visibility = Visibility.Visible;
                tb.Visibility = Visibility.Collapsed;               
            //}
            //_task.Task_name = tbl.Text;
            TreeViewItem tvi1 = new TreeViewItem();
            tvi1.Header = "MS Data";
            //tvi1.Selected += new RoutedEventHandler(TaskContent_Selected);
            tvi.Items.Add(tvi1);
            TreeViewItem tvi2 = new TreeViewItem();
            tvi2.Header = "Identification";
            //tvi2.Selected += new RoutedEventHandler(TaskContent_Selected);
            tvi.Items.Add(tvi2);
            TreeViewItem tvi3 = new TreeViewItem();
            tvi3.Header = "Quantitation";
            //tvi3.Selected += new RoutedEventHandler(TaskContent_Selected);
            tvi.Items.Add(tvi3);
            TreeViewItem tvi4 = new TreeViewItem();
            tvi4.Header = "Summary";
            //tvi4.Selected += new RoutedEventHandler(TaskContent_Selected);
            tvi.Items.Add(tvi4);
            tvi.IsExpanded = true;
            this.TaskTree.Items.Add(tvi);

            //右键菜单
            ContextMenu cm = new ContextMenu();
            MenuItem mi1 = new MenuItem();
            mi1.Header = "Run";
            mi1.InputGestureText = "F5";
            mi1.Click += new RoutedEventHandler(Run_Task);
            cm.Items.Add(mi1);          
            MenuItem mi2 = new MenuItem();
            mi2.Header = "Rename";
            mi2.InputGestureText = "F2";
            mi2.Click += new RoutedEventHandler(Rename_Task);
            cm.Items.Add(mi2);
            MenuItem mi3 = new MenuItem();
            mi3.Header = "Remove";     //移除            
            mi3.Click += new RoutedEventHandler(Remove_Task);
            cm.Items.Add(mi3);
            MenuItem mi4 = new MenuItem();
            mi4.Header = "Delete";
            mi4.Click += new RoutedEventHandler(Delete_Task);
            cm.Items.Add(mi4);
            MenuItem mi5 = new MenuItem();
            mi5.Header = "Clone";     //打开所在目录            
            mi5.Click += new RoutedEventHandler(Clone_Task);
            cm.Items.Add(mi5);
            cm.Items.Add(new Separator());

            MenuItem mi6 = new MenuItem();
            mi6.Header = "Open the folder";     //打开所在目录            
            mi6.Click += new RoutedEventHandler(Open_Task_Dir);
            cm.Items.Add(mi6);
            
            MenuItem mi7 = new MenuItem();
            mi7.Header = "Run pBuild";
            mi7.InputGestureText = "F7";
            mi7.Click += new RoutedEventHandler(CallpBuild);
            cm.Items.Add(mi7);
            /*
            MenuItem mi5 = new MenuItem();
            mi5.Header = "Property";
            mi5.Click += new RoutedEventHandler(Show_Task_Property);
            cm.Items.Add(mi5);
             * * */
            tvi.ContextMenu = cm;
            tvi.IsSelected = true;
             
        }
        //右键run
        private void Run_Task(object sender, RoutedEventArgs e)
        {
            StartSearch(sender,e);
            //MenuItem mi = sender as MenuItem;
            //TreeViewItem tvi = ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(mi)) as TreeViewItem;
            //while ((tvi.Parent as TreeViewItem) != null)
            //{
            //    tvi = tvi.Parent as TreeViewItem;
            //}
            //int index = this.TaskTree.Items.IndexOf(tvi);
            //#region Todo
            ////要运行的不是当前任务，要切换后运行还是直接运行
            //#endregion
            ////current_task_index = index;
            ////current_task=tasklist[index];
            ////Initial_Task(tasklist[index],index,0);
            //saveDisplayParam();
            //tasklist[index].Check_ok=Factory.Create_Run_Instance().Check_Task(tasklist[index]);
            //if (tasklist[index].Check_ok == false)
            //{
            //    System.Windows.MessageBox.Show("The configuration of " + tasklist[index].Task_name + " is not completed!", "pFind warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    this.tab.SelectedIndex = 3;
            //}
            //else
            //{
            //    JustRun(index,tasklist[index].Task_name,0);
            //}
        }

        private void Show_Task_Property(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Delete_Task(object sender, RoutedEventArgs e)
        {
            if (this.TaskTree.SelectedItem != null)
            {
                TreeViewItem tvi = (TreeViewItem)this.TaskTree.SelectedItem;
                while ((tvi.Parent as TreeViewItem) != null)
                {
                    tvi = tvi.Parent as TreeViewItem;
                }
                int index = this.TaskTree.Items.IndexOf(tvi);
                string tsname = tasklist[index].Task_name;
                string tspath = tasklist[index].Path;
                for (int i = 0; i < runtasklist.Count; ++i)
                {
                    if (runtasklist[i].Taskpath == tspath && runtasklist[i].Statusindex == (int)TaskStatus.running)
                    {
                        System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Sorry, " + tsname + " is running, please stop it first.", "pFind Warning", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
                System.Windows.MessageBoxResult result1 = System.Windows.MessageBox.Show(tsname + " will be permanently deleted!", "pFind Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result1 == MessageBoxResult.Cancel)
                {
                    return;
                }
                else
                {
                    try
                    {
                        if (Directory.Exists(tspath))
                        {
                            if (tspath[tspath.Length - 1] == '\\') 
                            {
                                tspath = tspath.Substring(0,tspath.Length-1);
                            }
                            Directory.Delete(@tspath, true);
                        }
                        removeTreeViewItem(tvi, index);
                        // 从TaskTree删除任务时，也要将其从运行队列中移除
                        for (int i = 0; i < runtasklist.Count; ++i)
                        {
                            if (runtasklist[i].Taskpath == tspath)
                            {
                                runtasklist[i].Statusindex = (int)TaskStatus.removed;    
                                this.runningtasks.ItemsSource = runtasklist;
                                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.runningtasks.ItemsSource);
                                view.Filter = new Predicate<object>(o => { return (o as TaskData).Statusindex != (int)TaskStatus.removed; });
                                removed_num++;
                                if (runtasklist.Count - removed_num <= 0) this.RABtn.Visibility = Visibility.Collapsed;
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message);
                        return;
                    }
                }
            }
            //System.Windows.MessageBox.Show(index.ToString());           
            
        }

        private void Remove_Task(object sender, RoutedEventArgs e)
        {
            if (this.TaskTree.SelectedItem != null)
            {
                TreeViewItem tvi = (TreeViewItem)this.TaskTree.SelectedItem;
                while ((tvi.Parent as TreeViewItem) != null)
                {
                    tvi = tvi.Parent as TreeViewItem;
                }
                int index = this.TaskTree.Items.IndexOf(tvi);
                string tsname = tasklist[index].Task_name;
                string tspath = tasklist[index].Path;
                //for (int i = 0; i < runtasklist.Count; ++i)
                //{
                //    if (runtasklist[i].Taskpath == tasklist[index].Path && runtasklist[i].Statusindex == (int)TaskStatus.running)
                //    {
                //        System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Sorry, " + tsname + " is running, please stop it first.", "pFind Warning", MessageBoxButton.OK, MessageBoxImage.Information);
                //        return;
                //    }
                //}
                System.Windows.MessageBoxResult result1 = System.Windows.MessageBox.Show(tsname + " will be removed from the task bar!", "pFind Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result1 == MessageBoxResult.Cancel)
                {
                    return;
                }
                else
                {
                    removeTreeViewItem(tvi, index);
                    // 从TaskTree中移除任务时，也要将其从运行队列中移除
                    for (int i = 0; i < runtasklist.Count; ++i)
                    {
                        if (runtasklist[i].Taskpath == tspath)
                        {
                            runtasklist[i].Statusindex = (int)TaskStatus.removed;
                            this.runningtasks.ItemsSource = runtasklist;
                            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.runningtasks.ItemsSource);
                            view.Filter = new Predicate<object>(o => { return (o as TaskData).Statusindex != (int)TaskStatus.removed; });
                            removed_num++;
                            if (runtasklist.Count - removed_num <= 0) this.RABtn.Visibility = Visibility.Collapsed;
                            break;
                        }
                    }
                }
            }
        }
        private void removeTreeViewItem(object tvi,int index)
        {
            this.TaskTree.Items.Remove(tvi);
            this.tasklist.RemoveAt(index);
            this.task_count--;
            //没有节点，则返回首页
            if (this.TaskTree.Items.Count < 1)
            {
                home_switcher(0);               
            }
            #region Todo
            //如果当前展开的节点刚好是删除的节点
            #endregion
            if (current_task_index == index)
            {
                if (tasklist.Count > index)
                {
                    current_task = tasklist[index];
                    current_task_index = index;
                    Initial_Task(tasklist[index], index, 0);
                }
                else
                {
                    if (index > 0)
                    {
                        current_task = tasklist[index - 1];
                        current_task_index = index - 1;
                        Initial_Task(tasklist[index - 1], index - 1, 0);
                    }
                }
            }            
        }

        private bool taskChanged()
        {
            #region Todo
            //是否考虑任务名
            if (current_taskname_changed || current_taskpath_changed) 
            {
                return true;
            }
            #endregion
            if ((!current_task.T_File.Equals(_file)) || (!current_task.T_Search.Equals(_search)) 
                || (!current_task.T_Filter.Equals(_filter)) ||(!current_task.T_Quantitation.Equals(_quantitation)))
            {
                return true;
            }
            return false;
        }

        public void clone_task()
        {
            try
            {
                TreeViewItem tvi = (TreeViewItem)this.TaskTree.SelectedItem;

                while ((tvi.Parent as TreeViewItem) != null)
                {
                    tvi = tvi.Parent as TreeViewItem;
                }

                int index = this.TaskTree.Items.IndexOf(tvi);

                string tspath = tasklist[index].Path;

                saveDisplayParam();

                if (taskChanged())
                {
                    System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Do you want to save the settings of the current task?", "pFind", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (result == System.Windows.MessageBoxResult.Yes)
                    {
                        Factory.Create_Copy_Instance().FileCopy(_file, current_task.T_File);
                        Factory.Create_Copy_Instance().SearchParamCopy(_search, current_task.T_Search);
                        Factory.Create_Copy_Instance().FilterParamCopy(_filter, current_task.T_Filter);
                        Factory.Create_Copy_Instance().QuantitaionCopy(_quantitation, current_task.T_Quantitation);
                        current_task.T_MS2Quant = Factory.Create_Copy_Instance().DeepCopyWithXmlSerializer(_ms2quant);
                        Factory.Create_Run_Instance().SaveParams(current_task);
                    }
                    else if (result == System.Windows.MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }
                _Task nts = new _Task();
                Factory.Create_Copy_Instance().FileCopy(current_task.T_File, nts.T_File); // current_task.T_File.copyTo(nts.T_File);
                Factory.Create_Copy_Instance().SearchParamCopy(current_task.T_Search, nts.T_Search); // current_task.T_Search.copyTo(nts.T_Search);
                Factory.Create_Copy_Instance().FilterParamCopy(current_task.T_Filter, nts.T_Filter); //current_task.T_Filter.copyTo(nts.T_Filter);
                Factory.Create_Copy_Instance().QuantitaionCopy(current_task.T_Quantitation, nts.T_Quantitation); // current_task.T_Quantitation.copyTo(nts.T_Quantitation);
                nts.T_MS2Quant = Factory.Create_Copy_Instance().DeepCopyWithXmlSerializer(current_task.T_MS2Quant);

                nts.Task_name = current_task.Task_name + "_Clone";
                tasklist.Add(nts);

                current_task_index = tasklist.Count - 1;
                current_task = nts;
                this.New_TreeView(ref nts, "new");
                this.tab.SelectedIndex = 0;
                task_count++;
                //this.FRScroll.IsExpanded = false;
                //this.QRScroll.IsExpanded = false;
            }
            catch(Exception exe)
            {
                MessageBox.Show("[Clone Task] " + exe.Message);
            }

        }
        public bool new_task()
        {
            //try
            {
                if (tasklist.Count > 0)
                {
                    saveDisplayParam();
                    if (taskChanged())
                    {
                        System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Do you want to save the change before create another task?", "pFind", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                        if (result == System.Windows.MessageBoxResult.Yes)
                        {
                            Factory.Create_Copy_Instance().FileCopy(_file, current_task.T_File);
                            Factory.Create_Copy_Instance().SearchParamCopy(_search, current_task.T_Search);
                            Factory.Create_Copy_Instance().FilterParamCopy(_filter, current_task.T_Filter);
                            Factory.Create_Copy_Instance().QuantitaionCopy(_quantitation, current_task.T_Quantitation);
                            current_task.T_MS2Quant = Factory.Create_Copy_Instance().DeepCopyWithXmlSerializer(_ms2quant);
                            Factory.Create_Run_Instance().SaveParams(current_task);
                        }
                        else if (result == System.Windows.MessageBoxResult.Cancel)
                        {
                            return true;
                        }
                    }
                }
                string path = System.Windows.Forms.Application.StartupPath + "\\template";
                if (Directory.Exists(path + "\\param") && System.IO.File.Exists(path + "\\param\\pFind.cfg"))
                {
                    load_task_by_path(path, true, false);
                }
                else
                {
                    System.Windows.MessageBox.Show(Message_Help.NEW_TASK_FAIL);
                    return false;
                }
            }
            //catch(Exception exe)
            //{
            //    MessageBox.Show("[Create A Task] " + exe.Message);
            //    return false;
            //}
            return true;
        }
        private void New_Task(object sender, RoutedEventArgs e)
        {           
            this.new_task();
        }
        private void Clone_Task(object sender, RoutedEventArgs e)
        {
            this.clone_task();
        }
        #region Todo
        /*
        private void new_task(object sender,RoutedEventArgs e)
        {
            NewTask nt = new NewTask();
            nt.ChangeTextEvent += new ChangeNewTaskHandler(nt_ChangeTextEvent);
            nt.ShowDialog();
        }
        void nt_ChangeTextEvent(String tskname)
        {
            TreeView tv = new TreeView();
            TreeViewItem tvi = new TreeViewItem();
            tvi.Header = tskname;
            TreeViewItem tvi1 = new TreeViewItem();
            tvi1.Header = "File";
            tvi.Items.Add(tvi1);
            TreeViewItem tvi2 = new TreeViewItem();
            tvi2.Header = "Identification";
            tvi.Items.Add(tvi2);
            TreeViewItem tvi3 = new TreeViewItem();
            tvi3.Header = "Quantitation";
            tvi.Items.Add(tvi3);
            TreeViewItem tvi4 = new TreeViewItem();
            tvi4.Header = "Summary";
            tvi.Items.Add(tvi4);
            tv.Items.Add(tvi);
            this.Tasks.Children.Add(tv);

        } */
        #endregion
        /**打开一个任务**/
        private void open_task(object sender,RoutedEventArgs e)
        {            
            this.Open_Task(false);                
        }
        public void Open_Task(bool fromFirstPage)
        {
            try
            {
                if (tasklist.Count > 0)
                {
                    saveDisplayParam();
                    if ((!current_task.T_File.Equals(_file)) || (!current_task.T_Search.Equals(_search)) || (!current_task.T_Filter.Equals(_filter)) || (!current_task.T_Quantitation.Equals(_quantitation)))
                    {
                        System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Do you want to save the change before open another task?", "pFind", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                        if (result == System.Windows.MessageBoxResult.Yes)
                        {
                            Factory.Create_Copy_Instance().FileCopy(_file, current_task.T_File);
                            Factory.Create_Copy_Instance().SearchParamCopy(_search, current_task.T_Search);
                            Factory.Create_Copy_Instance().FilterParamCopy(_filter, current_task.T_Filter);
                            Factory.Create_Copy_Instance().QuantitaionCopy(_quantitation, current_task.T_Quantitation);
                            current_task.T_MS2Quant = Factory.Create_Copy_Instance().DeepCopyWithXmlSerializer(_ms2quant);
                            Factory.Create_Run_Instance().SaveParams(current_task);
                        }
                        else if (result == System.Windows.MessageBoxResult.Cancel)
                        {
                            return;
                        }

                    }
                }

                System.Windows.Forms.OpenFileDialog f_dialog = new System.Windows.Forms.OpenFileDialog();
                f_dialog.Filter = "pFind task files (*.tsk)|*.tsk";
                System.Windows.Forms.DialogResult dresult = f_dialog.ShowDialog();
                if (dresult == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
                if (dresult == System.Windows.Forms.DialogResult.OK)
                {
                    //记录选中的目录  
                    defaultfilePath = f_dialog.FileName;
                    int tmpIdx = defaultfilePath.LastIndexOf('\\');
                    if (tmpIdx != -1)
                        defaultfilePath = defaultfilePath.Substring(0, tmpIdx);
                }

                string task_path = defaultfilePath;
                int tmpIdx2 = defaultfilePath.LastIndexOf('\\');
                if (tmpIdx2 != -1)
                    defaultfilePath = defaultfilePath.Substring(0, tmpIdx2);

                if (open_task_by_path(task_path, fromFirstPage))
                {
                    //this.toolGrid.Visibility = Visibility.Visible;
                    //this.Background = Brushes.White;
                    //this.Visibility = System.Windows.Visibility.Visible;
                }
            }
            catch(Exception exe)
            {
                MessageBox.Show("[Open A Task] "+exe.Message);
            }
        }
        /**
         * param:task_path 路径
         * param:fromFirstPage是否来自首页
         * */
        public bool open_task_by_path(string task_path,bool fromFirstPage)
        {
            #region
            string tmp=task_path;
            int p=tmp.Length-1;
            while(tmp[p]=='\\'){
                p--;
            }
            string task_name = tmp.Substring(tmp.LastIndexOf('\\') + 1, p - tmp.LastIndexOf('\\'));
            if (System.IO.File.Exists(task_path + "\\"+task_name+".tsk") && Directory.Exists(task_path + "\\param") && System.IO.File.Exists(task_path + "\\param\\pFind.cfg"))
            {
                bool isexist = false;
                int selectedindex = 0;
                for (int i = 0; i < tasklist.Count; i++)
                {
                    if (tasklist[i].Path.Equals(task_path + "\\"))
                    {
                        isexist = true;
                        selectedindex = i;
                        break;
                    }
                }
                if (isexist)
                {
                    System.Windows.MessageBox.Show("This task is already open!", "pFind", MessageBoxButton.OK, MessageBoxImage.Information);
                    current_task = tasklist[selectedindex];
                    current_task_index = selectedindex;
                    Initial_Task(tasklist[selectedindex], selectedindex, 0);
                }
                else
                {
                    load_task_by_path(task_path,false,fromFirstPage);
                    if (task_path.EndsWith("\\")) 
                    {
                        task_path = task_path.Substring(0,task_path.Length-1);
                    }
                    ConfigHelper.update_recent_task(Advanced.recent_tasks_file_path,task_path);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Loading Error!\nSorry! This is not a valid folder for a pFind task.", "pFind", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
            #endregion
        }

        void InitialListInfo()
        {
            /*初始化标记列表*/
            show_labels();
            InitialLabelList();
            //初始化修饰列表
            this.disAllMod.IsChecked = false;
            show_com_mods();
            InitialModsList();
        }

        private string getDefaultTaskName()
        { 
            string task_name = "pFindTask";
            while (Directory.Exists(_get_output_path() + "\\" + task_name + (global_task_index).ToString()))
            {
                ++global_task_index;
            }
            task_name += (global_task_index).ToString();
            ++ global_task_index;
            return task_name;
        }

        public void load_task_by_path(string path,bool isnew,bool fromFirstPage)
        {
            _Task ots = new _Task();
            _Task tmp = current_task;
            current_task = ots;  // address assignment
            if (isnew)
            {
                NewTask new_task_window = new NewTask(getDefaultTaskName(), defaultfilePath);
                new_task_window.ChangeTextEvent += new NewTask.ChangeNewTaskHandler(ReceiveValues);
                new_task_window.ShowDialog();
                if (new_task_window.DialogResult != true)
                {
                    current_task = tmp;
                    return;
                }
            }
            home_switcher(1);   //切换至配置界面
            try
            {
                bool haspParse = false;
                this._file.Data_file_list.Clear(); ;
                this._search.Fix_mods.Clear();
                this._search.Var_mods.Clear();
                this._quantitation.Labeling.Light_label.Clear();
                this._quantitation.Labeling.Medium_label.Clear();
                this._quantitation.Labeling.Heavy_label.Clear();
                //_Task ots = new _Task();

                if (System.IO.File.Exists(path + "\\param\\pParse.cfg"))     //raw file -> pParse
                {
                    haspParse = true;
                    Factory.Create_pParse_Instance().readpParse_para(path, ref ots);
                }
                // mgf -> pFind
                Factory.Create_pFind_Instance().readpFind_pfd(path, haspParse, ref ots);
                if (System.IO.File.Exists(path + "\\param\\pQuant.cfg"))
                {
                    Factory.Create_pQuant_Instance().readpQuant_qnt(path, ref ots);
                }
                if (System.IO.File.Exists(path + "\\param\\pIsobariQ.cfg"))
                {
                    Factory.Create_pQuant_Instance().pIsobariQ_read(path, ref ots);
                }
                Factory.Create_Copy_Instance().FileCopy(ots.T_File, _file);
                Factory.Create_Copy_Instance().SearchParamCopy(ots.T_Search, _search);
                Factory.Create_Copy_Instance().FilterParamCopy(ots.T_Filter, _filter);
                Factory.Create_Copy_Instance().QuantitaionCopy(ots.T_Quantitation, _quantitation);
                _ms2quant = Factory.Create_Copy_Instance().DeepCopyWithXmlSerializer(ots.T_MS2Quant);
                // [wrm!] 由于对象地址改变，必须重新设置数据源，才能保证twoway binding 有效
                setDataSource();
                /*初始化列表信息：标记和修饰信息*/
                InitialListInfo();

                tasklist.Add(ots);
                current_task_index = tasklist.Count - 1;
                current_task = ots;
                if (isnew)
                {
                    New_TreeView(ref ots, "new");
                    this.dfnum.Text = "0";  //DataFile.DFnum = 0;
                    setDFSizeText(0.0);
                    //this.dfsize.Text = "0.0";  // DataFile.DFsize = 0.0;
                    firstload = false;
                }
                else
                {
                    New_TreeView(ref ots, "open");
                    this.dfnum.Text = _file.Data_file_list.Count.ToString();
                    setDFSizeText(getDFSize(_file.Data_file_list));
                    if (firstload)
                    {
                        firstload = true;
                    }
                }
                this.tab.SelectedIndex = 0;
                //this.FRScroll.IsExpanded = false;
                //this.QRScroll.IsExpanded = false;
                task_count++;
            }
            catch (Exception exe)
            {
                System.Windows.MessageBox.Show(exe.Message);
            }
        }

        private void ReceiveValues(string[] arr)
        {
            current_task.Task_name = arr[0];
            current_task.Path = arr[1];
        }

        private double getDFSize(ObservableCollection<_FileInfo> flist)
        {
            double totalsize = 0.0;
            for (int i = 0; i < flist.Count; i++)
            {
                string fs = flist[i].FileSize;
                totalsize += double.Parse(fs.Substring(0, fs.LastIndexOf("MB")));
            }
            totalsize = Math.Round(totalsize / 1024.0001, 3);
            return totalsize;
        }

        //chihao: 20140810 计算文件大小并动态更新单位
        private void setDFSizeText(double size)
        {
            double dfMul = 1.0;
            this.size_unit.Text = " GB";
            if(size < 1) // MB
            {
                dfMul *= 1024.001;
                this.size_unit.Text = " MB";
                if(size * 1024 < 1) // KB
                {
                    dfMul *= 1024.001;
                    this.size_unit.Text = " KB";
                }
            }
            this.dfsize.Text = Math.Round(size * dfMul, 3).ToString();
        }
        private void Initial_Task(_Task ts,int selected_index,int tab_index)
        {
            TreeViewItem tvi = this.TaskTree.Items[selected_index] as TreeViewItem;
            tvi.IsSelected = true;
            this._file.Data_file_list.Clear(); ;
            this._search.Fix_mods.Clear();
            this._search.Var_mods.Clear();
            this._quantitation.Labeling.Light_label.Clear();
            this._quantitation.Labeling.Medium_label.Clear();
            this._quantitation.Labeling.Heavy_label.Clear();
            Factory.Create_Copy_Instance().FileCopy(ts.T_File, _file);  // TODO: 待检查，如果把所有的Copy函数换成DeepCopyWithXmlSerializer，会否影响源数据到界面的绑定内容的更新
            Factory.Create_Copy_Instance().SearchParamCopy(ts.T_Search,_search);
            Factory.Create_Copy_Instance().FilterParamCopy(ts.T_Filter,_filter);
            Factory.Create_Copy_Instance().QuantitaionCopy(ts.T_Quantitation,_quantitation);
            _ms2quant = Factory.Create_Copy_Instance().DeepCopyWithXmlSerializer(ts.T_MS2Quant);
            InitialListInfo();
            this.dfnum.Text = _file.Data_file_list.Count.ToString();
            setDFSizeText(getDFSize(_file.Data_file_list)); 
            this.tab.SelectedIndex = tab_index;
        }
        
        private void Task_Switch(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                saveDisplayParam();

                //decide which treeviewitem is selected,and load the task
                //System.Windows.MessageBox.Show(this.TaskTree.SelectedItem.GetType().ToString());
                int selected_index = -1;
                int tab_index = 0;
                bool istviselected = false;
                for (int i = 0; i < this.TaskTree.Items.Count; i++)
                {
                    bool flag = false;
                    TreeViewItem selected_task = (TreeViewItem)this.TaskTree.Items[i];
                    if (selected_task.IsSelected)
                    {
                        selected_index = i;
                        tab_index = 0;
                        istviselected = true;
                        break;
                    }
                    for (int j = 0; j < selected_task.Items.Count; j++)
                    {
                        TreeViewItem selected_task_tab = (TreeViewItem)selected_task.Items[j];
                        if (selected_task_tab.IsSelected)
                        {
                            flag = true;
                            tab_index = j;
                            break;
                        }
                    }
                    if (flag)
                    {
                        selected_index = i;
                        istviselected = true;
                        break;
                    }
                }
                if (istviselected)
                {
                    if (current_task.Path != tasklist[selected_index].Path)
                    {
                        /*whether save the change*/
                        if (taskChanged())
                        {
                            System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Do you want to save the change before switching to another task?", "pFind", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == System.Windows.MessageBoxResult.Yes)
                            {
                                _task_save();
                            }

                        }
                        current_task = tasklist[selected_index];
                        current_task_index = selected_index;
                        current_taskname_changed = false;
                        current_taskpath_changed = false;
                        Initial_Task(tasklist[selected_index], selected_index, tab_index);

                        Is_task_running(current_task);
                    }
                    else // current_task 
                    {
                        Is_task_running(current_task);
                        if (this.FileTab.IsEnabled == true)
                        {
                            this.tab.SelectedIndex = tab_index;
                        }
                    }
                }
            }
            catch(Exception exe)
            {
                MessageBox.Show(exe.Message);
            }
        }
        // 检测打开或切换到的任务是否在运行，如果running的话，锁定按钮，否则解锁。
        private void Is_task_running(_Task tsk)
        {
            bool isrunning = false;
            for (int i = 0; i < runtasklist.Count; i++)
            {
                if (runtasklist[i].Taskpath == tsk.Path && runtasklist[i].Statusindex == (int)TaskStatus.running)
                {
                    isrunning = true;
                    break;
                }
            }
            if (isrunning) DisableButtons();
            else EnableButtons();
        }

        private void _task_save()
        {
            try
            {
                saveDisplayParam();
                Factory.Create_Copy_Instance().FileCopy(_file, current_task.T_File);
                Factory.Create_Copy_Instance().SearchParamCopy(_search, current_task.T_Search);
                Factory.Create_Copy_Instance().FilterParamCopy(_filter, current_task.T_Filter);
                Factory.Create_Copy_Instance().QuantitaionCopy(_quantitation, current_task.T_Quantitation);
                current_task.T_MS2Quant = Factory.Create_Copy_Instance().DeepCopyWithXmlSerializer(_ms2quant);
                Factory.Create_Run_Instance().SaveParams(current_task);
            }
            catch(Exception exe)
            {
                throw new Exception("[Save Task] "+exe.Message);
            }
        }
        private void clk(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource == tab)
            {               
                TreeViewItem tvi = this.TaskTree.SelectedItem as TreeViewItem;
                if (tvi != null)
                {
                    if (tvi.Header is string)
                    {
                        tvi = tvi.Parent as TreeViewItem;
                    }
                    TreeViewItem tvi_child = tvi.Items[this.tab.SelectedIndex] as TreeViewItem;
                    tvi_child.IsSelected = true;
                }
                
                if (tab.SelectedIndex == 0) //File
                {
                    
                }
                if (tab.SelectedIndex == 1) //Identification
                {
                  
                }
                else if (tab.SelectedIndex == 2) //Quantitation
                {
                    //show_com_mods();
                    //InitialModsList();
                }
                else if (tab.SelectedIndex == 3) //Summary
                {                  
                    show_summary();
                }
                pre_tab_index = this.tab.SelectedIndex;
            }
            
        }

        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var treeViewItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        static DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);

            return source;
        }

        /*************************File模块交互逻辑****************************************************/
        
        void FormatChange(object sender,SelectionChangedEventArgs e)
        {
            //_file.Data_file_list.Clear();
            string tips = "Please note that you can only add \"";
            string tmp=this.FormatChoices.SelectedValue.ToString();
            tips += tmp.Substring(tmp.LastIndexOf(":")+1);
            tips += " \"files";
            ToolTip tp = new ToolTip();
            //tp.Style = (Style)this.FindResource("SimpleTip");
            tp.Content = tips;
            this.addRawBtn.ToolTip = tp;
            int ft = this.FormatChoices.SelectedIndex;
            if (ft == (int)FormatOptions.MGF) {     //MGF
                this.MixSpec.Visibility = Visibility.Collapsed;
                this.pParseConfig.Visibility = Visibility.Collapsed;
                for (int i = _file.Data_file_list.Count-1; i >= 0; i--)
                {
                    string ftype = _file.Data_file_list[i].Type.ToUpper();
                    if (ftype != "MGF")
                    {
                        _file.Data_file_list.RemoveAt(i);
                    }
                }
                this.QuantTabContent.Visibility=Visibility.Collapsed;
                display_temporary_tooltip(this.QuantTab,PlacementMode.Bottom,Message_Help.NONE_QUANTITATION);                               
                            
            }         
            else if (ft == (int)FormatOptions.RAW) {
                this.MixSpec.Visibility = Visibility.Visible;
                this.pParseConfig.Visibility = Visibility.Visible;
                for (int i = _file.Data_file_list.Count - 1; i >= 0; i--)
                {
                    string ftype = _file.Data_file_list[i].Type.ToUpper();
                    if (ftype != "RAW")
                    {
                        _file.Data_file_list.RemoveAt(i);
                    }
                }
                this.QuantTabContent.Visibility = Visibility.Visible;    
            }

        }
        /**添加数据文件**/
        void Add_DataFiles(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            #region Todo
            //文件后缀名过滤
            #endregion
            openFileDialog.Multiselect = true;
            int file_format = this.FormatChoices.SelectedIndex;
            if (file_format == (int)FormatOptions.MGF) {
                openFileDialog.Filter = "mgf files (*.mgf)|*.mgf";
            }          
            else if(file_format==(int)FormatOptions.RAW){
                openFileDialog.Filter = "raw files (*.raw)|*.raw";     //|all files (*.*)|*.*
            }
            else if (file_format == (int)FormatOptions.WIFF)           // added by wrm. @2016.08.04
            {
                openFileDialog.Filter = "wiff files (*.wiff)|*.wiff";
            }
            string TMP = this.FormatChoices.SelectedValue.ToString();
            TMP=TMP.Substring(TMP.LastIndexOf(":") + 1).Trim();
            string tmp = TMP.ToLower();
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                {
                    _FileInfo fi = new _FileInfo(filename);
                    if (fi.Type.Equals(tmp) || fi.Type.Equals(TMP))
                    {
                        bool flag = true;
                        for (int i = 0; i < this._file.Data_file_list.Count; i++)
                        {
                            if (this._file.Data_file_list[i].FilePath.Equals(filename))
                            {
                                flag = false; break;
                            }
                        }
                        if (flag)
                        {
                            this._file.Data_file_list.Add(fi);                           
                        }
                    }
                    else {
                        System.Windows.MessageBox.Show("Only " + tmp + "/" + TMP  + "files can be added.","File Format",MessageBoxButton.OK,MessageBoxImage.Warning);
                    }
                    
                }

                //chihao 20140810: 这段代码和getDFSize里的一样
                //for (int i = 0; i < this._file.Data_file_list.Count; i++)
                //{
                //    string fs=this._file.Data_file_list[i].FileSize;
                //    totalsize += double.Parse(fs.Substring(0, fs.LastIndexOf("MB")));
                //}

                this.fileList.ItemsSource = this._file.Data_file_list;
                this.dfnum.Text = this._file.Data_file_list.Count.ToString();   //DataFile.DFnum = this._file.Data_file_list.Count;
                setDFSizeText(getDFSize(this._file.Data_file_list)); //Math.Round(totalsize / 1024.0001, 3).ToString();        //DataFile.DFsize += Math.Round(totalsize / 1024.0001, 3);              
                this.status1.Text = this._file.Data_file_list.Count + " " + tmp + " files";
            }
        }
     
        /**删除数据文件**/
        void Delete_DataFile(object sender, RoutedEventArgs e)
        {
            //迟浩：修改BUG，这里不能用for循环，因为count值会变的
            while(this.fileList.SelectedItems.Count > 0)
            {
                _FileInfo fi = this.fileList.SelectedItems[0] as _FileInfo;
                this._file.Data_file_list.Remove(fi);               
            }
            this._file.Data_file_list.Remove(this.fileList.SelectedItem as _FileInfo);
            this.fileList.SelectedIndex = -1;
            this.dfnum.Text = this._file.Data_file_list.Count.ToString();         //DataFile.DFnum = this._file.Data_file_list.Count;
            setDFSizeText(getDFSize(_file.Data_file_list));   //DataFile.DFsize -= Math.Round(totalsize / 1024.0001, 3);
            this.status1.Text = this._file.Data_file_list.Count + " data file(s)";
        }
        /**清除数据文件**/
        void Clear_DataFiles(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("Do you really want to clear all files?","Warning",
                MessageBoxButton.OKCancel,MessageBoxImage.Warning)== MessageBoxResult.OK) {
                    this._file.Data_file_list.Clear();
                    this.dfnum.Text = "0";   //DataFile.DFnum = 0;
                    setDFSizeText(0.0);  //DataFile.DFsize = 0.0;
                    this.status1.Text = "data file num: 0";
            }
            
        }
        
        private void show_pParse(object sender,RoutedEventArgs e)
        {
            this.pParseConfig.Visibility = Visibility.Visible;
        }

        private void hide_pParse(object sender, RoutedEventArgs e)
        {
            this.pParseConfig.Visibility = Visibility.Collapsed;
        }

        private void pParseConfig_Expanded(object sender, RoutedEventArgs e)
        {
            this.msdataScroll.ScrollToBottom();
        }
        /************************************Identification模块交互逻辑*********************************/

        /****************************************Search模块********************************************/      

        void show_DB()
        {
            int dbIndex = current_task.T_Search.Db_index;
            ConfigHelper.getDB();
            this.Database.ItemsSource = ConfigHelper.dblist;
            if (ConfigHelper.dblist.Count > 1)
            {
                if (_search.Db_index == -1)
                {
                    int num = ConfigHelper.db_add.Count;
                    if (num > 0)
                    {
                        this.Database.SelectedIndex = ConfigHelper.dblist.IndexOf(ConfigHelper.db_add[num-1]);
                        ConfigHelper.db_add.Clear();
                    }
                    else
                    {
                        this.Database.SelectedIndex = dbIndex;
                    }
                }
                else this.Database.SelectedIndex = _search.Db_index;
            }
            else
            {
                this.Database.SelectedIndex = -1;
            }
        }

        void show_enzyme()
        {
            int enzymeIndex = _search.Enzyme_index;
            ConfigHelper.getEnzyme();
            this.enzymeChoices.ItemsSource=ConfigHelper.enzymelist;
            if (_search.Enzyme_index > -1)
            {
                this.enzymeChoices.SelectedIndex = _search.Enzyme_index;
            }
            else
            {
                int num = ConfigHelper.enzyme_add.Count;
                if (num > 0)
                {
                    this.enzymeChoices.SelectedIndex = ConfigHelper.enzymelist.IndexOf(ConfigHelper.enzymelist[num - 1]);
                    ConfigHelper.enzyme_add.Clear();
                }
                else
                {
                    this.enzymeChoices.SelectedIndex = enzymeIndex;
                }
            }

            this.enzymespecChoices.ItemsSource = ConfigHelper.enzymespeclist;
            if (_search.Enzyme_Spec_index > -1)
            {
                this.enzymespecChoices.SelectedIndex = _search.Enzyme_Spec_index;
            }
            else
            {
                this.enzymespecChoices.SelectedIndex = 0;
            }
            
        }       

        
        /**选择开放搜索,无需设置修饰**/
        private void enable_open_search(Object sender, RoutedEventArgs e)
        {
            //this.ModConfig.Visibility = Visibility.Collapsed;
        }
        /**选择限定搜索,可以设置修饰**/
        private void disable_open_search(Object sender, RoutedEventArgs e)
        {
            //this.ModConfig.Visibility = Visibility.Visible; 
        }
        /** 显示常用修饰 **/
        private void show_com_mods()
        {
            display_mods.Clear();
            for (int i = 0; i < ConfigHelper.com_mods.Count; i++)
            {
                display_mods.Add(ConfigHelper.com_mods[i]);
            }
            this.Modifications.ItemsSource = display_mods;      //绑定修饰列表
        }
        private void show_com_mods(Object sender, RoutedEventArgs e)
        {
            show_com_mods();
            InitialModsList();
        }

        void InitialModsList()
        {
            for (int i = 0; i < _search.Var_mods.Count;i++ )
            {
                display_mods.Remove(_search.Var_mods[i]);
            }
            for (int i = 0; i < _search.Fix_mods.Count; i++)
            {
                display_mods.Remove(_search.Fix_mods[i]);
            }
        }
        /** 显示所有修饰 **/
        private void show_all_mods(Object sender,RoutedEventArgs e)
        {
            display_mods.Clear();
            for (int i = 0; i < ConfigHelper.all_mods.Count; i++)
            {
                display_mods.Add(ConfigHelper.all_mods[i]);
            }
            this.Modifications.ItemsSource = display_mods;      //绑定修饰列表
            InitialModsList();
        }
        private void select_mods(Object sender, MouseButtonEventArgs e)
        {

        }
        /*添加固定修饰*/
        private void add_fix_mods(Object sender,RoutedEventArgs e)
        {
            display_mods = (ObservableCollection<String>)this.Modifications.ItemsSource;
            //MessageBox.Show(Modifications.SelectedItems.Count.ToString());
            ObservableCollection<String> tmp = new ObservableCollection<String>();
            for (int i = 0; i < this.Modifications.SelectedItems.Count ; i++) {
                String fm = this.Modifications.SelectedItems[i].ToString();
                tmp.Add(fm);
            }
            for (int i = 0; i < tmp.Count; i++) {
                _search.Fix_mods.Add(tmp[i]);               
                display_mods.Remove(tmp[i]);
            }
            //selectedFixMod.Items.Clear();
            selectedFixMod.ItemsSource = _search.Fix_mods;
            //display_mods.OrderBy(p=>p);
            Modifications.ItemsSource = display_mods;
            
        }
        /*删除固定修饰*/
        private void remove_fix_mods(Object sender, RoutedEventArgs e)
        {
            display_mods = (ObservableCollection<String>)Modifications.ItemsSource;
            ObservableCollection<String> tmp = new ObservableCollection<String>();
            for (int i = 0; i < selectedFixMod.SelectedItems.Count; i++) {
                String fm = selectedFixMod.SelectedItems[i].ToString();
                tmp.Add(fm);
            }
            for (int i = 0; i < tmp.Count; i++) {
                _search.Fix_mods.Remove(tmp[i]);
                display_mods.Add(tmp[i]);
            }
            selectedFixMod.ItemsSource = _search.Fix_mods;
            display_mods=new ObservableCollection<String>(display_mods.OrderBy(p => p));   //升序排列
            Modifications.ItemsSource = display_mods;
        }
        /*添加可变修饰*/
        private void add_var_mods(Object sender, RoutedEventArgs e)
        {
            display_mods = (ObservableCollection<String>)Modifications.ItemsSource;
            ObservableCollection<String> tmp = new ObservableCollection<String>();
            for (int i = 0; i < Modifications.SelectedItems.Count; i++)
            {
                String fm = Modifications.SelectedItems[i].ToString();
                tmp.Add(fm);
            }
            for (int i = 0; i < tmp.Count; i++) {
                _search.Var_mods.Add(tmp[i]);
                display_mods.Remove(tmp[i]);
            }
            //selectedFixMod.Items.Clear();
            selectedVarMod.ItemsSource = _search.Var_mods;
            //display_mods.OrderBy(p=>p);
            Modifications.ItemsSource = display_mods;
        }
        /*删除可变修饰*/
        private void remove_var_mods(Object sender, RoutedEventArgs e)
        {
            display_mods = (ObservableCollection<String>)Modifications.ItemsSource;
            ObservableCollection<String> tmp = new ObservableCollection<String>();
            for (int i = 0; i < selectedVarMod.SelectedItems.Count; i++)
            {
                String fm = selectedVarMod.SelectedItems[i].ToString();
                tmp.Add(fm);
            }
            for (int i = 0; i < tmp.Count; i++) {
                _search.Var_mods.Remove(tmp[i]);
                display_mods.Add(tmp[i]);
            }
            selectedVarMod.ItemsSource = _search.Var_mods;
            display_mods = new ObservableCollection<String>(display_mods.OrderBy(p => p));     //升序排列
            Modifications.ItemsSource = display_mods;
        }
        /**展开Filter**/
        private void filter_Expanded(object sender, RoutedEventArgs e)
        {
            this.IdentScroll.ScrollToBottom();
        }
        private void speORpepTooltip(object sender, SelectionChangedEventArgs e)
        {
            ToolTip tp = new ToolTip();
            //tp.Style = (Style)this.FindResource("SimpleTip");
            if (this.speORpep.SelectedIndex == 0)
            {
                tp.Content = "At Spectra Level";
            }
            else    //1
            {
                tp.Content = "At Peptides Level";
            }
            this.speORpep.ToolTip = tp;
        } 
        private void User_Guide_Clk(object sender, RoutedEventArgs e)
        {                                  
            System.Diagnostics.Process.Start("http://pfind.ict.ac.cn/software/pFind/pFind_user_guide.htm");           
           
        }
        private void Help_About_Clk(object sender, RoutedEventArgs e)
        {
            pFind.About about = new pFind.About();
            about.ShowDialog();
        }
        
        
       

        /*******************************定量模块的交互逻辑******************************************************/
        
        /**选择定量方式**/
        
        private void select_quantitation_type(object sender,SelectionChangedEventArgs e)
        {
            this.model.SelectedIndex = 0;    //chihao 20140811 对于其余定量方式，model都取默认的吧
            this.ll_element_enrichment_calibration.SelectedIndex = (int)LL_ELEMENT_ENRICHMENT_CALIBRATION.None;
            if (this.QuanType.SelectedIndex == (int)Quant_Type.Labeling_None)
            {   //None
                this.Quant_Label.Visibility = Visibility.Visible;         //此处可以用触发器实现
                this.QLabelFree.Visibility = Visibility.Collapsed;
                this.sample_rate_sp.Visibility = Visibility.Collapsed;
                this.calibriation_15N_sp.Visibility = Visibility.Collapsed;
                this.multiplicity1.Visibility = Visibility.Visible;
                this.MultiplicityType.SelectedIndex = 0;
                this.MultiplicityType.IsEnabled = false;
                show_labels();
                _quantitation.Labeling.Medium_label.Clear();
                _quantitation.Labeling.Medium_label.Add(ConfigHelper.labels[0]);
                InitialLabelList();
                firstload = false;
                this.mediumBtn.IsEnabled = false;
            }
            else if (this.QuanType.SelectedIndex == (int)Quant_Type.Labeling_15N)
            {   //Label_15N, default label configuration
                this.Quant_Label.Visibility = Visibility.Visible;
                this.QLabelFree.Visibility = Visibility.Collapsed;
                this.calibriation_15N_sp.Visibility = Visibility.Visible;
                this.sample_rate_sp.Visibility = Visibility.Visible;

                this.MultiplicityType.SelectedIndex = 1;
                this.MultiplicityType.IsEnabled = false;     //使其不可编辑
                show_labels();
                _quantitation.Labeling.Light_label.Clear();
                _quantitation.Labeling.Light_label.Add(ConfigHelper.labels[0]);    //必定为 none
                _quantitation.Labeling.Heavy_label.Clear();
                _quantitation.Labeling.Heavy_label.Add(ConfigHelper.labels[1]);    //必定为 15N_Labeling  
                InitialLabelList();
                firstload = false;
                this.lightBtn.IsEnabled = false;
                this.heavyBtn.IsEnabled = false;
                this.model.SelectedIndex = 1;    //15N
                string tip="Already switch model to 15N for you!";
                display_temporary_tooltip(this.QuanType,PlacementMode.Right,tip);
                //this.ll_element_enrichment_calibration.SelectedIndex = (int)LL_ELEMENT_ENRICHMENT_CALIBRATION.N15;      //1,2 15N
            }
            else if (this.QuanType.SelectedIndex == (int)Quant_Type.Labeling_SILAC)
            {  //Label_SILAC etc
                this.Quant_Label.Visibility = Visibility.Visible;
                this.QLabelFree.Visibility = Visibility.Collapsed;
                this.sample_rate_sp.Visibility = Visibility.Visible;
                //this.ll_element_enrichment_calibration.SelectedIndex = (int)LL_ELEMENT_ENRICHMENT_CALIBRATION.None;
                this.calibriation_15N_sp.Visibility = Visibility.Collapsed;

                this.MultiplicityType.IsEnabled = true;
                this.multiplicity1.Visibility = Visibility.Collapsed;
                this.MultiplicityType.SelectedIndex = 1;
                show_labels();
                _quantitation.Labeling.Light_label.Clear();
                _quantitation.Labeling.Light_label.Add(ConfigHelper.labels[0]);    //必定为 none
                _quantitation.Labeling.Heavy_label.Clear();
                InitialLabelList();
                firstload = false;
                this.lightBtn.IsEnabled = true;
                this.mediumBtn.IsEnabled = true;
                this.heavyBtn.IsEnabled = true;               
            }
            else if (this.QuanType.SelectedIndex == (int)Quant_Type.Label_free)
            { //Label free
                this.Quant_Label.Visibility = Visibility.Collapsed;
                this.QLabelFree.Visibility = Visibility.Visible;
                this.sample_rate_sp.Visibility = Visibility.Visible;
            }
        }

        private void pQuant_Expanded(object sender, RoutedEventArgs e)
        {
            this.quantScroll.ScrollToBottom();
        }

        /**选择**/
        private void select_multiplicity_type(object sender, SelectionChangedEventArgs e) 
        {         
            if (this.MultiplicityType.SelectedIndex == 0)
            { //1  

                //this.maxLA.Visibility = Visibility.Collapsed;               
                this.lightLabel.Visibility = Visibility.Collapsed;
                this.heavyLabel.Visibility = Visibility.Collapsed;
                this.mediumLabel.Visibility = Visibility.Visible;
                this.mediumLabel.Content = "Label :";
                this.selectedLightLabels.Visibility = Visibility.Collapsed;
                this.selectedHeavyLabels.Visibility = Visibility.Collapsed;
                this.selectedMediumLabels.Visibility = Visibility.Visible;
                this.lightBtn.Visibility = Visibility.Collapsed;
                this.heavyBtn.Visibility = Visibility.Collapsed;
                this.mediumBtn.Visibility = Visibility.Visible;
                show_labels();                                            //
                if (!firstload)
                {
                    _quantitation.Labeling.Light_label.Clear();
                    _quantitation.Labeling.Medium_label.Clear();
                    _quantitation.Labeling.Heavy_label.Clear();
                    _quantitation.Labeling.Medium_label.Add(ConfigHelper.labels[0]);
                    display_labels.Remove(ConfigHelper.labels[0]);
                }
                else
                {
                    InitialLabelList();
                    firstload = false;
                }

            }
            else if (this.MultiplicityType.SelectedIndex == 1)  //2
            {
                //this.maxLA.Visibility = Visibility.Visible;
                this.lightLabel.Visibility = Visibility.Visible;
                this.heavyLabel.Visibility = Visibility.Visible;
                this.mediumLabel.Visibility = Visibility.Collapsed;

                this.selectedLightLabels.Visibility = Visibility.Visible;
                this.selectedHeavyLabels.Visibility = Visibility.Visible;
                this.selectedMediumLabels.Visibility = Visibility.Collapsed;
                this.lightBtn.Visibility = Visibility.Visible;
                this.heavyBtn.Visibility = Visibility.Visible;
                this.mediumBtn.Visibility = Visibility.Collapsed;
                show_labels();                                    //
                if (!firstload)
                {
                    _quantitation.Labeling.Light_label.Clear();
                    _quantitation.Labeling.Light_label.Add(ConfigHelper.labels[0]);
                    _quantitation.Labeling.Medium_label.Clear();
                    _quantitation.Labeling.Heavy_label.Clear();
                    display_labels.Remove(_quantitation.Labeling.Light_label[0]);
                    //display_labels.Remove(_quantitation.Labeling.Heavy_label[0]);
                }
                else
                {
                    InitialLabelList();
                    firstload = false;
                }

            }
            else if (this.MultiplicityType.SelectedIndex == 2)
            { //3
                //this.maxLA.Visibility = Visibility.Visible;
                this.lightLabel.Visibility = Visibility.Visible;
                this.heavyLabel.Visibility = Visibility.Visible;
                this.mediumLabel.Visibility = Visibility.Visible;
                this.mediumLabel.Content = "Medium Label :";
                this.selectedLightLabels.Visibility = Visibility.Visible;
                this.selectedHeavyLabels.Visibility = Visibility.Visible;
                this.selectedMediumLabels.Visibility = Visibility.Visible;
                this.lightBtn.Visibility = Visibility.Visible;
                this.heavyBtn.Visibility = Visibility.Visible;
                this.mediumBtn.Visibility = Visibility.Visible;
                show_labels();                                   //
                if (!firstload)
                {
                    _quantitation.Labeling.Light_label.Clear();
                    _quantitation.Labeling.Medium_label.Clear();
                    _quantitation.Labeling.Heavy_label.Clear();
                }
                else
                {
                    InitialLabelList();
                    firstload = false;
                }

            }
        }
        
        private void show_labels()
        {
            display_labels.Clear();
            for (int i = 0; i < ConfigHelper.labels.Count; i++)
            {
                display_labels.Add(ConfigHelper.labels[i]);
            }
            this.labellist.ItemsSource = display_labels;
           
        }

        private void InitialLabelList()
        {
            if (_quantitation.Labeling.Light_label.Count > 0)
            {
                display_labels.Remove(_quantitation.Labeling.Light_label[0]);
            }
            if (_quantitation.Labeling.Medium_label.Count > 0)
            {
                display_labels.Remove(_quantitation.Labeling.Medium_label[0]);
            }
            if (_quantitation.Labeling.Heavy_label.Count > 0)
            {
                display_labels.Remove(_quantitation.Labeling.Heavy_label[0]);
            }
        }
        /**添加标记**/
        private void add_light_labels(object sender, RoutedEventArgs e)
        {
            display_labels = (ObservableCollection<String>)this.labellist.ItemsSource;
            ObservableCollection<String> tmp = new ObservableCollection<String>();
            for (int i = 0; i < this.labellist.SelectedItems.Count; i++)
            {
                String llb = this.labellist.SelectedItems[i].ToString();
                tmp.Add(llb);
            }
            for (int i = 0; i < tmp.Count; i++)
            {
                _quantitation.Labeling.Light_label.Add(tmp[i]);
                display_labels.Remove(tmp[i]);
            }
            this.labellist.ItemsSource = display_labels;
            this.selectedLightLabels.ItemsSource = _quantitation.Labeling.Light_label;

        }
        private void add_medium_labels(object sender, RoutedEventArgs e)
        {

            display_labels = (ObservableCollection<String>)this.labellist.ItemsSource;
            ObservableCollection<String> tmp = new ObservableCollection<String>();
            for (int i = 0; i < this.labellist.SelectedItems.Count; i++)
            {
                String llb = this.labellist.SelectedItems[i].ToString();
                tmp.Add(llb);
            }
            for (int i = 0; i < tmp.Count; i++)
            {

                _quantitation.Labeling.Medium_label.Add(tmp[i]);
                display_labels.Remove(tmp[i]);
            }
            this.labellist.ItemsSource = display_labels;
            this.selectedMediumLabels.ItemsSource = _quantitation.Labeling.Medium_label;
        }
        private void add_heavy_labels(object sender, RoutedEventArgs e)
        {

            display_labels = (ObservableCollection<String>)this.labellist.ItemsSource;
            ObservableCollection<String> tmp = new ObservableCollection<String>();
            for (int i = 0; i < this.labellist.SelectedItems.Count; i++)
            {
                String llb = this.labellist.SelectedItems[i].ToString();
                tmp.Add(llb);
            }
            for (int i = 0; i < tmp.Count; i++)
            {

                _quantitation.Labeling.Heavy_label.Add(tmp[i]);
                display_labels.Remove(tmp[i]);
            }
            this.labellist.ItemsSource = display_labels;
            this.selectedHeavyLabels.ItemsSource = _quantitation.Labeling.Heavy_label;
        }
        /**删除标记**/
        private void remove_light_labels(object sender, RoutedEventArgs e)
        {
            display_labels = (ObservableCollection<String>)this.labellist.ItemsSource;
            ObservableCollection<String> tmp = new ObservableCollection<String>();
            for (int i = 0; i < this.selectedLightLabels.SelectedItems.Count; i++)
            {
                String llb = this.selectedLightLabels.SelectedItems[i].ToString();
                tmp.Add(llb);
            }
            for (int i = 0; i < tmp.Count; i++)
            {
                _quantitation.Labeling.Light_label.Remove(tmp[i]);
                display_labels.Add(tmp[i]);
            }
            this.selectedLightLabels.ItemsSource = _quantitation.Labeling.Light_label;
            display_labels = new ObservableCollection<String>(display_labels.OrderBy(p => p));   //升序排列
            this.labellist.ItemsSource = display_labels;
        }
        private void remove_medium_labels(object sender, RoutedEventArgs e)
        {
            display_labels = (ObservableCollection<String>)this.labellist.ItemsSource;
            ObservableCollection<String> tmp = new ObservableCollection<String>();
            for (int i = 0; i < this.selectedMediumLabels.SelectedItems.Count; i++)
            {
                String llb = this.selectedMediumLabels.SelectedItems[i].ToString();
                tmp.Add(llb);
            }
            for (int i = 0; i < tmp.Count; i++)
            {
                _quantitation.Labeling.Medium_label.Remove(tmp[i]);
                display_labels.Add(tmp[i]);
            }
            this.selectedMediumLabels.ItemsSource = _quantitation.Labeling.Medium_label;
            display_labels = new ObservableCollection<String>(display_labels.OrderBy(p => p));   //升序排列
            this.labellist.ItemsSource = display_labels;
        }
        private void remove_heavy_labels(object sender, RoutedEventArgs e)
        {
            display_labels = (ObservableCollection<String>)this.labellist.ItemsSource;
            ObservableCollection<String> tmp = new ObservableCollection<String>();
            for (int i = 0; i < this.selectedHeavyLabels.SelectedItems.Count; i++)
            {
                String llb = this.selectedHeavyLabels.SelectedItems[i].ToString();
                tmp.Add(llb);
            }
            for (int i = 0; i < tmp.Count; i++)
            {
                _quantitation.Labeling.Heavy_label.Remove(tmp[i]);
                display_labels.Add(tmp[i]);
            }
            this.selectedHeavyLabels.ItemsSource = _quantitation.Labeling.Heavy_label;
            display_labels = new ObservableCollection<String>(display_labels.OrderBy(p => p));   //升序排列
            this.labellist.ItemsSource = display_labels;
        }
        

        /*********************************************Summary面板的交互逻辑***********************************************************/

        string _get_output_path()
        {
            StreamReader sr = new StreamReader(ConfigHelper.startup_path+"\\pFind.ini", Encoding.Default);
            string strLine = sr.ReadLine();
            string outputpath = "";
            while (strLine != null)
            {
                if (strLine.Length > 10 && strLine.Substring(0, 10).Equals("outputpath"))
                {
                    outputpath = strLine.Substring(strLine.LastIndexOf("=") + 1);
                    break;
                }
                strLine = sr.ReadLine();
            }
            output_path = outputpath;
            return outputpath;
        }
        void Task_save()
        {
            if (current_task.Path == "" || current_task.Path == null || current_task.Path.Equals("unsaved"))
            {
                //获取当前默认路径 
                string outputpath = _get_output_path();
                current_task.Path = outputpath + current_task.Task_name + "\\";
            }
            current_task.Check_ok = true;
        }

        void File_save()       //file save
        {
            try
            {
                _file.File_format_index = this.FormatChoices.SelectedIndex;
                _file.setFileFormat();
                _file.Instrument_index = this.InstrumentChoices.SelectedIndex;
                _file.setInstrument();
                _file.Mix_spectra = this.MixSpecCB.IsChecked.Value;
                _file.Mz_decimal_index = this.mzDecimal.SelectedIndex;
                _file.setMZDecimal();
                _file.Intensity_decimal_index = this.IntensityDecimal.SelectedIndex;
                _file.setIntensityDecimal();
                _file.Model = ConfigHelper.mars_model[this.model.SelectedIndex];
                _file.ModelIndex = this.model.SelectedIndex;
                _file.Threshold = double.Parse(this.threshhold.Text.ToString());
                if (_file.Data_file_list == null || _file.Data_file_list.Count == 0 || _file.Threshold.ToString() == "")
                {
                    current_task.Check_ok = false;
                }
                //_file.Pparse_advanced.Output_mgf = (this.FormatChoices.SelectedIndex == (int)FormatOptions.RAW ? 0 : 1);
            }
            catch (Exception exe)
            {
                current_task.Check_ok = false;
                throw new Exception("[MS Data] " + exe.Message + "\n" + Message_Help.INVALID_INPUT);       
            }
        }

        void Identification_save()
        {
            try
            {
                #region search save and check

                _search.Db_index = this.Database.SelectedIndex;
                _search.setDatabase();    //database    
                if (this.Database.SelectedIndex == -1)
                {
                    current_task.Check_ok = false;
                    // throw new ArgumentException(Message_Help.DataBase_EMPTY);
                }
                _search.Enzyme_index = this.enzymeChoices.SelectedIndex;        //enzyme
                _search.setEnzyme();
                _search.Enzyme_Spec_index = this.enzymespecChoices.SelectedIndex;
                _search.setEnzymeSpec();
                _search.Cleavages = this.msChoices.SelectedIndex;  //cleavages
    
                //母离子和碎片离子检查
               
                Tolerance tmp_ptl = new Tolerance();              //Precursor Tolerance
                
                if (this.PreTol.Text.ToString() == "" || this.FraTol.Text.ToString() == "")
                {
                    current_task.Check_ok = false;
                    throw new ArgumentException(Message_Help.ALL_NOT_NULL);
                }
                tmp_ptl.Tl_value = double.Parse(this.PreTol.Text.ToString());
                tmp_ptl.Isppm = this.ptolUnits.SelectedIndex;   //1为ppm
                _search.Ptl = tmp_ptl;

                Tolerance tmp_ftl = new Tolerance();              //Fragment Tolerance
                tmp_ftl.Tl_value = double.Parse(this.FraTol.Text.ToString());
                tmp_ftl.Isppm = this.ftolUnits.SelectedIndex;

                if (tmp_ptl.Tl_value < 0 || tmp_ftl.Tl_value < 0)
                {
                    current_task.Check_ok = false;
                }
                _search.Ftl = tmp_ftl;
                _search.Open_search = this.openSearch.IsChecked.Value;           //open search
                
                #endregion

                // filter
                FDR tmp_fdr = new FDR();                             //FDR
                
                //FDR和peptide mass,length检查
                
                if (this.FDRValue.Text == "")
                {
                    current_task.Check_ok = false;
                    throw new ArgumentException(Message_Help.ALL_NOT_NULL);
                }
                tmp_fdr.Fdr_value = double.Parse(this.FDRValue.Text.ToString());                
                tmp_fdr.IsPeptides = this.speORpep.SelectedIndex; 
                _filter.Fdr = tmp_fdr;

                //peptide mass,length检查 , peptide num可不可以为空

                Range tmp_pepmass = new Range();
                if (this.PepMassMin.Text.ToString() == "" || this.PepMassMax.Text.ToString() == "") 
                {
                    current_task.Check_ok = false;
                    throw new ArgumentException(Message_Help.ALL_NOT_NULL);
                }
                tmp_pepmass.Left_value = double.Parse(this.PepMassMin.Text.ToString());
                tmp_pepmass.Right_value = double.Parse(this.PepMassMax.Text.ToString());
                if (tmp_pepmass.Left_value > tmp_pepmass.Right_value || tmp_pepmass.Right_value < 0)
                {
                    current_task.Check_ok = false;
                    throw new ArgumentException(Message_Help.PEP_MASS_RANGE);
                }
                _filter.Pep_mass_range = tmp_pepmass;
                Range tmp_peplen = new Range();                
                if (this.PepLenMin.Text.ToString() == "" || this.PepLenMax.Text.ToString() == "" || this.PepNumMin.Text.ToString() == "" || this.ProteinFdr.Text.Trim().ToString()=="")
                {
                    current_task.Check_ok = false;
                    throw new ArgumentException(Message_Help.ALL_NOT_NULL);
                }
                tmp_peplen.Left_value = double.Parse(this.PepLenMin.Text.ToString());
                tmp_peplen.Right_value = double.Parse(this.PepLenMax.Text.ToString());               
                if (tmp_peplen.Left_value > tmp_peplen.Right_value || tmp_peplen.Right_value < 0)
                {
                    current_task.Check_ok = false;
                    throw new ArgumentException(Message_Help.PEP_LEN_RANGE);
                }
                _filter.Pep_length_range = tmp_peplen;
                _filter.Min_pep_num = int.Parse(this.PepNumMin.Text.ToString());               
                _filter.Protein_Fdr = double.Parse(this.ProteinFdr.Text);
            }
            catch(Exception exe)
            {
                current_task.Check_ok = false;
                throw new Exception("[Identification] " + exe.Message + "\n" + Message_Help.INVALID_INPUT);
            }
        }

        void Quantitation_save()
        {
            try
            {
                //Quantitation的参数检查
               
                _quantitation.Quantitation_type = this.QuanType.SelectedIndex;
                if (this.QuanType.SelectedIndex != (int)Quant_Type.Label_free)
                {
                    _quantitation.Labeling.Multiplicity_index = this.MultiplicityType.SelectedIndex;
                    _quantitation.Labeling.setMultiplicity();
                }
                else  //label free
                {

                }
                int nshc = int.Parse(this.number_scans_half_cmtg.Text);
                _quantitation.Quant_advanced.Number_scans_half_cmtg = nshc;
                double pfc = double.Parse(this.ppm_for_calibration.Text);
                _quantitation.Quant_advanced.Ppm_for_calibration = pfc;
                double phwap = double.Parse(this.ppm_half_win_accuracy_peak.Text);
                _quantitation.Quant_advanced.Ppm_half_win_accuracy_peak = phwap;
                if (nshc < 0 || phwap < 0)
                {
                    current_task.Check_ok = false;
                    throw new ArgumentException(Message_Help.INVALID_INPUT);
                }
                _quantitation.Quant_advanced.Number_hole_in_cmtg = this.number_hole_in_cmtg.SelectedIndex;
                _quantitation.Quant_advanced.Type_same_start_end_between_evidence = this.type_same_start_end_between_evidence.SelectedIndex;
                _quantitation.Quant_advanced.Ll_element_enrichment_calibration = this.ll_element_enrichment_calibration.SelectedIndex;
            }
            catch (Exception exe)
            {
                current_task.Check_ok = false;
                throw new Exception("[Quantitation] " + exe.Message + "\n" + Message_Help.INVALID_INPUT);                
            }
        }

        void MS2Quantitation_save()
        {
            try
            {
                //MS2 Quantitation的参数检查
                _ms2quant.Enable_ms2quant = this.Is_ms2quant.IsChecked.Value;
                _ms2quant.QuantitativeMethod = this.ms2QuantMethod.SelectedIndex;

                _ms2quant.MS2_Advanced.FTMS_Tolerance.Isppm = this.ms2_ftolUnits.SelectedIndex;
                _ms2quant.MS2_Advanced.FTMS_Tolerance.Tl_value = double.Parse(this.ms2_FrgTol.Text);
                _ms2quant.MS2_Advanced.Peak_Range.Left_value = double.Parse(this.V_PeakRangeMin.Text);
                _ms2quant.MS2_Advanced.Peak_Range.Right_value = double.Parse(this.V_PeakRangeMax.Text);
                _ms2quant.MS2_Advanced.Pif = double.Parse(this.V_PIF.Text);
                _ms2quant.MS2_Advanced.Psm_Fdr = double.Parse(this.ms2_PSMFdr.Text);
                _ms2quant.MS2_Advanced.Protein_Fdr = double.Parse(this.ms2_ProteinFdr.Text);
                _ms2quant.MS2_Advanced.Correct = this.V_correctMatrix.IsChecked.Value;
                _ms2quant.MS2_Advanced.RunVSN = this.V_runVSN.IsChecked.Value;
            }
            catch (Exception exe)
            {
                current_task.Check_ok = false;
                throw new Exception("[MS2 Quantitation] " + exe.Message + "\n" + Message_Help.INVALID_INPUT);
            }
        }

        void show_summary()
        {
            try
            {
                saveDisplayParam();

                this.fileReport.ItemsSource = _file.getFileParam();

                this.searchReport.ItemsSource = _search.getSearchParam();

                this.filterReport.ItemsSource = _filter.getFilterParam();
                // TODO: mgf时允许二级谱定量吗？
                if (this.FormatChoices.SelectedIndex == (int)FormatOptions.RAW)
                {
                    this.quantitationReport.ItemsSource = _quantitation.getQuantitationParam();
                    this.QRScroll.Visibility = Visibility.Visible;
                    if (this.Is_ms2quant.IsChecked.Value)
                    {
                        this.ms2quantitationReport.ItemsSource = _ms2quant.getMS2QuantParam();
                        this.MS2QRScroll.Visibility = Visibility.Visible;
                    }
                    else 
                    {
                        this.MS2QRScroll.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    this.QRScroll.Visibility = Visibility.Collapsed;
                    this.MS2QRScroll.Visibility = Visibility.Collapsed;
                }
                ReportViewWidthChange();     //改变ListView列的宽度
            }
            catch(Exception exe)
            {
                throw new Exception("[Summary] " + exe.Message); 
            }
        }
        private void ReportViewWidthChange()
        {
            //if (section == (int)Section.MS_Data) { } 
            #region
            //ms_data
            var view1 = this.fileReport.View as GridView;
            if (view1 == null || view1.Columns.Count < 1) return;
            double len1 = 0.0;
            foreach (var atrr in this.fileReport.Items)
            {
                _Attribute att = (_Attribute)atrr;
                FormattedText ft = new FormattedText(att._value.ToString(),
                    System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight,
                    new Typeface(this.quantitationReport.FontFamily.ToString()),
                    this.quantitationReport.FontSize, Brushes.Black);
                if (ft.WidthIncludingTrailingWhitespace > len1)
                {
                    len1 = ft.WidthIncludingTrailingWhitespace;
                }
            }
            view1.Columns[1].Width = len1 + 30;
            #endregion
            #region
            //search
            var view2 = this.searchReport.View as GridView;
            if (view2 == null || view2.Columns.Count < 1) return;
            double len2 = 0.0;
            foreach (var atrr in this.searchReport.Items)
            {
                _Attribute att = (_Attribute)atrr;
                FormattedText ft = new FormattedText(att._value.ToString(),
                    System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight,
                    new Typeface(this.quantitationReport.FontFamily.ToString()),
                    this.quantitationReport.FontSize, Brushes.Black);
                if (ft.WidthIncludingTrailingWhitespace > len2)
                {
                    len2 = ft.WidthIncludingTrailingWhitespace;
                }
            }
            view2.Columns[1].Width = len2 + 30;
            #endregion
            #region
            //filter
            var view3 = this.filterReport.View as GridView;
            if (view3 == null || view3.Columns.Count < 1) return;
            double len3 = 0.0;
            foreach (var atrr in this.filterReport.Items)
            {
                _Attribute att = (_Attribute)atrr;
                FormattedText ft = new FormattedText(att._value.ToString(),
                    System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight,
                    new Typeface(this.quantitationReport.FontFamily.ToString()),
                    this.quantitationReport.FontSize, Brushes.Black);
                if (ft.WidthIncludingTrailingWhitespace > len3)
                {
                    len3 = ft.WidthIncludingTrailingWhitespace;
                }
            }
            view3.Columns[1].Width = len3 + 30;
            #endregion
            
            #region
            //quantitation
            var view4 = this.quantitationReport.View as GridView;
            if (view4 == null || view4.Columns.Count < 1) return;
            double len4 = 0.0;
            foreach (var atrr in this.quantitationReport.Items)
            {
                    _Attribute att = (_Attribute)atrr;
                    FormattedText ft = new FormattedText(att._value.ToString(),
                        System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                        FlowDirection.LeftToRight,
                        new Typeface(this.quantitationReport.FontFamily.ToString()),
                        this.quantitationReport.FontSize, Brushes.Black);
                    if (ft.WidthIncludingTrailingWhitespace > len4)
                    {
                        len4 = ft.WidthIncludingTrailingWhitespace;
                    }
             }
             view4.Columns[1].Width = len4 + 30;
            #endregion
        }
        #region Todo 
        //Loaded="RView_TargetUpdated" 改变summary面板value的宽度
        private void RView_TargetUpdated(object sender, RoutedEventArgs e)  //改变ListView列的宽度
        {
            var view = this.quantitationReport.View as GridView;
            if (view == null || view.Columns.Count < 1) return;                
            double width = this.quantitationReport.ActualWidth;
            if (width != 0.0)
            {
                view.Columns[1].Width = width - view.Columns[0].Width;
            }
        }
        #endregion

        //private void FRScroll_Expanded(object sender, RoutedEventArgs e)
        //{
        //    this.SumScroll.ScrollToVerticalOffset(this.FiRScroll.ActualHeight + this.SRScroll.ActualHeight);
        //}

        //private void QRScroll_Expanded(object sender, RoutedEventArgs e)
        //{
        //    this.SumScroll.ScrollToBottom();
        //}

        private void saveDisplayParam()
        {
            try
            {
                //// Gets the element with keyboard focus.
                //UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;

                //// Change keyboard focus.
                //if (elementWithFocus != null)
                //{
                //    elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                //}

                Task_save();
                File_save();
                Identification_save();
                Quantitation_save();
                MS2Quantitation_save();
            }
            catch (Exception exe)
            {
                throw new Exception(exe.Message);
            }
        }

        /**保存配置**/
        private void SaveConfiguration(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = 0; i < runtasklist.Count; ++i)
                {
                    if (runtasklist[i].Taskpath == current_task.Path && runtasklist[i].Statusindex == (int)TaskStatus.running)
                    {
                        System.Windows.MessageBox.Show("The task is already started and the configuration cannot be modified.");
                        return;
                    }
                }

                saveDisplayParam();
                Factory.Create_Copy_Instance().FileCopy(_file, current_task.T_File);
                Factory.Create_Copy_Instance().SearchParamCopy(_search, current_task.T_Search);
                Factory.Create_Copy_Instance().FilterParamCopy(_filter, current_task.T_Filter);
                Factory.Create_Copy_Instance().QuantitaionCopy(_quantitation, current_task.T_Quantitation);
                current_task.T_MS2Quant = Factory.Create_Copy_Instance().DeepCopyWithXmlSerializer(_ms2quant);  // good example!
                bool flag = Factory.Create_Run_Instance().SaveParams(current_task);
                if (flag)
                {
                    //create tooltip
                    string save_tip = "Save work done!";
                    TreeViewItem tvi = this.TaskTree.SelectedItem as TreeViewItem;
                    display_temporary_tooltip(tvi, PlacementMode.Top, save_tip);
                }
            }
            catch(Exception exe)
            {
                MessageBox.Show(exe.Message);
            }
        }       

        /// <summary>
        /// 运行前检查并保存参数
        /// </summary>
        private bool CheckCurrentBeforeStart()
        {
            try
            {
                bool backup = false;
                for (int i = 0; i < runtasklist.Count; ++i)
                {
                    if (runtasklist[i].Taskpath == current_task.Path)
                    {
                        if (runtasklist[i].Statusindex == (int)TaskStatus.running)
                        {
                            System.Windows.MessageBox.Show("The task is already started!");
                            return false;
                        }
                        else if (runtasklist[i].Statusindex == (int)TaskStatus.done)
                        {
                            MessageBoxResult res1 = System.Windows.MessageBox.Show("The task is already done!\n Do you want to rerun it?", "pFind", MessageBoxButton.OKCancel);
                            if (res1 == MessageBoxResult.Cancel)
                            {
                                return false;
                            }
                            MessageBoxResult res2 = System.Windows.MessageBox.Show("Do you want to backup it before you rerun?", "pFind", MessageBoxButton.OKCancel);
                            if (res2 == MessageBoxResult.OK)
                            {
                                backup = true;
                            }
                        }
                    }
                }
                if (backup)
                {
                    string tpath = current_task.Path;
                    if (!tpath.EndsWith("\\")) tpath += "\\";
                    string newpath = tpath + "backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "\\";
                    ConfigHelper.CopyTask(tpath, newpath);
                }
                saveDisplayParam();
                #region 是否应该无条件重写参数文件
                #endregion
                //if (taskChanged())
                {
                    //    System.Windows.MessageBoxResult res = System.Windows.MessageBox.Show("Do you want to save the changes of " + current_task.Task_name + " before running?", "pFind", MessageBoxButton.OKCancel);
                    //    if (res == MessageBoxResult.OK)
                    //    {
                    //        Dispatcher.Invoke(() =>
                    //        {
                    //            saveDisplayParam();
                    //        });
                    Factory.Create_Copy_Instance().FileCopy(_file, current_task.T_File);
                    Factory.Create_Copy_Instance().SearchParamCopy(_search, current_task.T_Search);
                    Factory.Create_Copy_Instance().FilterParamCopy(_filter, current_task.T_Filter);
                    Factory.Create_Copy_Instance().QuantitaionCopy(_quantitation, current_task.T_Quantitation);
                    current_task.T_MS2Quant = Factory.Create_Copy_Instance().DeepCopyWithXmlSerializer(_ms2quant);

                    Factory.Create_Run_Instance().SaveParams(current_task);
                    //}
                    //else if (res == System.Windows.MessageBoxResult.Cancel)
                    //{
                    //    return;
                    //}
                }
            }
            catch (Exception exe)
            {
                current_task.Check_ok = false;
                throw new Exception("[Parameter] " + exe.Message);
            }
            return current_task.Check_ok;           
        }

        private void StartSearch(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentBeforeStart())
            {
                JustRun(current_task_index, current_task.Task_name, 0);
            }
            else 
            {
                System.Windows.MessageBox.Show("The configuration of " + current_task.Task_name + " is not completed!", "pFind warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.tab.SelectedIndex = 3;
            }
        }
        /// <summary>
        /// 运行任务栏指定索引的任务
        /// </summary>
        /// <param name="taskindex">任务树中待运行任务的索引</param>
        /// <param name="taskname">任务名</param>
        /// <param name="stepfrom">从第几步开始run</param>
        private void JustRun(int taskindex,string taskname,int stepfrom)
        {
            //wrm 20141027: 
            try
            {
                //检查.Net4.5 + 是否正确安装
                //if (ConfigHelper.CheckFrameworkDotNet())
                //{
                    int ret = ExistInRunningList(taskindex);
                    if (ret == -1)   //not in the queue
                    {
                        TaskData td = new TaskData();
                        td.Taskpath = tasklist[taskindex].Path;
                        td.Taskname = taskname;
                        td.Statusindex = (int)TaskStatus.waiting;
                        td.Status = "Waiting";
                        td.Progress = 0;
                        td.ProgressText = "0%";
                        td.Start_time = DateTime.Now;    //开始时间
                        TimeSpan ts = (DateTime.Now.Subtract(td.Start_time));
                        td.Run_time = ts.Hours.ToString().PadLeft(2, '0') + ":" + ts.Minutes.ToString().PadLeft(2, '0') + ":" + ts.Seconds.ToString().PadLeft(2, '0');  //       
                        td.StepFrom = stepfrom;
                        runtasklist.Add(td);
                    }
                    else  //already in the queue
                    {
                        if (runtasklist[ret].Statusindex != (int)TaskStatus.running)  // waiting | done
                        {
                            runtasklist[ret].Start_time = DateTime.Now;
                            TimeSpan ts = (DateTime.Now.Subtract(runtasklist[ret].Start_time));
                            runtasklist[ret].Run_time = ts.Hours.ToString().PadLeft(2, '0') + ":" + ts.Minutes.ToString().PadLeft(2, '0') + ":" + ts.Seconds.ToString().PadLeft(2, '0');
                            runtasklist[ret].StepFrom = stepfrom;
                            if (runtasklist[ret].Statusindex == (int)TaskStatus.done)
                            {
                                TaskData td = new TaskData();
                                td.Statusindex = (int)TaskStatus.waiting;
                                td.Status = "waiting";
                                td.Progress = 0;
                                td.ProgressText = "0%";
                                td.Taskpath = runtasklist[ret].Taskpath;
                                td.Taskname = runtasklist[ret].Taskname;
                                td.Start_time = runtasklist[ret].Start_time;
                                td.Run_time = runtasklist[ret].Run_time;
                                td.StepFrom = stepfrom;
                                runtasklist[ret].Statusindex = (int)TaskStatus.removed;                  
                                removed_num++;
                                runtasklist.Add(td);
                            }
                        }
                    }
                    this.runningtasks.ItemsSource = runtasklist;
                    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.runningtasks.ItemsSource);
                    view.Filter = new Predicate<object>(o => { return (o as TaskData).Statusindex != (int)TaskStatus.removed; });
                    
                    if (bgworker1 != null && bgworker1.IsBusy)  //there is a task running
                    {

                    }
                    else   //there is no task running:(1)work done;(2)no work 
                    {

                        bgworker1 = new BackgroundWorker();
                        bgworker1.WorkerSupportsCancellation = true;                        
                        bgworker1.DoWork += new DoWorkEventHandler(DoSearchWork);                  
                        bgworker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SearchWorkDone);
                        bgworker1.RunWorkerAsync();
                    }
                //}
                //else
                //{
                //    throw new Exception(Message_Help.DOT_NET_ERROR);
                //}
            }catch(Exception exe)
            {
                MessageBox.Show(exe.Message,"pFind",MessageBoxButton.OK,MessageBoxImage.Stop);
            }
        }
        //在没有任务运行时，清除任务列表runtasklist中标志为removed的项
        private void TaskListItemRemoved()
        {
            //for (int i = 0; i < background_tasklist.Count; i++)
            int count = runtasklist.Count;
            for (int i = count-1; i >=0; i--)
            {
                if (runtasklist[i].Statusindex == (int)TaskStatus.removed)
                {
                    runtasklist.RemoveAt(i);
                }
            }
            removed_num = 0;
        }


        void StartSearchWork()
        {
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            this.Dispatcher.Invoke(() =>
            {
                this.Output.Clear();
                this.RABtn.Visibility = System.Windows.Visibility.Collapsed;
            });
            for (int i = 0; i < runtasklist.Count; i++)   
            {
                
                    // whether the task exist in the tasktree
                    if (-1 == _find_task_index_by_path(runtasklist[i].Taskpath))
                    {
                        continue;
                    }

                    int tmpstatus = runtasklist[i].Statusindex;
                    if (tmpstatus != (int)TaskStatus.done && tmpstatus != (int)TaskStatus.removed)
                    {
                        if (bgworker1.CancellationPending)
                        {
                            KillProcessAndChildren(process.Id);
                            return;
                        }
                        runtasklist[i].Statusindex = (int)TaskStatus.running;
                        runtasklist[i].Start_time = DateTime.Now;
                        TimeSpan ts = (DateTime.Now.Subtract(runtasklist[i].Start_time));
                        runtasklist[i].Run_time = ts.Hours.ToString().PadLeft(2, '0') + ":" + ts.Minutes.ToString().PadLeft(2, '0') + ":" + ts.Seconds.ToString().PadLeft(2, '0');
                        StartOneWork(process, runtasklist[i]);
                        //System.Windows.MessageBox.Show(runtasklist.Count.ToString());
                    }
                
            }
        }
        void KillProcessAndChildren(int pid)
        {
        ManagementObjectSearcher searcher = new ManagementObjectSearcher
  ("Select * From Win32_Process Where ParentProcessID=" + pid);
                        ManagementObjectCollection moc = searcher.Get();
                        foreach (ManagementObject mo in moc)
                        {
                            KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
                        }
                        try
                        {
                            Process proc = Process.GetProcessById(pid);
                            if(proc.ProcessName != "pBuild" && !proc.HasExited)
                                   proc.Kill();
                        }
                        catch (ArgumentException)
                        {
                            // Process already exited.
                        }
        }
        public void KillChildrenProcess(int pid)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
        }
        // 根据任务路径判断其在TaskTree中的索引
        int _find_task_index_by_path(string path)
        {
            for (int i = 0; i < tasklist.Count; ++i)
            {
                if (tasklist[i].Path == path)
                {
                    return i;
                }
            }
            return -1;
        }

        int ms2_cnt = 0;

        bool call_pXtract(Process process, TaskData taskdata)
        {
            bool pXtract_ok = true;
            try
            {               
                _Task r_task = tasklist[_find_task_index_by_path(taskdata.Taskpath)];
                File r_file = r_task.T_File;
                #region

                if (r_file.File_format.Equals("raw"))
                {   //wrm 20141024: 取消的对xcalibur是否存在的判断                                 
                    for (int i = 0; i < r_file.Data_file_list.Count; i++)
                    {
                        int pos = r_file.Data_file_list[i].FilePath.LastIndexOf('.');
                        string ms2 = r_file.Data_file_list[i].FilePath.Substring(0, pos) + ".ms2";
                        if (System.IO.File.Exists(ms2))
                        {
                            continue;
                        }
                        string xtract = "-a -ms -m " + r_file.Mz_decimal.ToString() + " -i " + r_file.Intensity_decimal.ToString() + " \"" + r_file.Data_file_list[i].FilePath.ToString() + "\"";
                        process.StartInfo.FileName = "xtract.exe";
                        process.StartInfo.Arguments = xtract;
                        if (process.Start())
                        {
                            DateTime xtract_begin = DateTime.Now;
                            while (!process.HasExited)
                            {
                                if (bgworker1.CancellationPending)
                                {
                                    //System.IO.File.Delete(ms2);
                                    KillProcessAndChildren(process.Id);
                                    //process.Kill();
                                    return false;
                                }
                                string line = process.StandardOutput.ReadLine();
                                ReportProgress(taskdata, line);
                                //process.ErrorDataReceived += new DataReceivedEventHandler(p_ErrorDataReceived);
                                //process.BeginErrorReadLine();
                                //string errline = process.StandardError.ReadLine();
                                //ReportProgress(taskdata, errline);
                            }
                            process.WaitForExit();

                        }
                        pXtract_ok = process_exit(process);
                    }
                }
                #endregion
            }
            catch (Exception exe)
            {
                MessageBox.Show(exe.Message);
                _stop_task();
                pXtract_ok = false;
            }
            return pXtract_ok;           
        }

        bool call_pParse(Process process, TaskData taskdata)
        {
            bool pParse_ok = true;
            try
            {
                _Task r_task = tasklist[_find_task_index_by_path(taskdata.Taskpath)];
                #region
                process.StartInfo.FileName = "pParse.exe";
                process.StartInfo.Arguments = "\"" + r_task.Path + "param\\pParse.cfg\"";
                if (process.Start())
                {
                    ms2_cnt = 0;
                    string line = "";
                    while (!process.StandardOutput.EndOfStream)  //!process.HasExited
                    {
                        //process.OutputDataReceived += new DataReceivedEventHandler(p_DataReceived);
                        //process.BeginOutputReadLine();
                        line = process.StandardOutput.ReadLine();
                        ReportProgress(taskdata, line);
                        //process.ErrorDataReceived += new DataReceivedEventHandler(p_ErrorDataReceived);
                        //process.BeginErrorReadLine();
                        //string errline = process.StandardError.ReadLine();   //错误流输出
                        //ReportProgress(taskdata, errline);
                        if (bgworker1.CancellationPending)
                        {
                            process.Kill();
                            return false;
                        }
                    }
                    process.WaitForExit();
                    //process.CancelOutputRead();
                }
                pParse_ok = process_exit(process);
                #endregion
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
                _stop_task();
                pParse_ok = false;
            }
            return pParse_ok;
        }

        bool call_pFind(Process process, TaskData taskdata,string fileName)
        {
            bool pFind_ok = true;
            try
            {
                _Task r_task = tasklist[_find_task_index_by_path(taskdata.Taskpath)];
                #region
                process.StartInfo.FileName = fileName;     //"Searcher.exe", "GroupFilter.exe"
                process.StartInfo.Arguments = "\"" + r_task.Path + "param\\pFind.cfg\"";
                if (process.Start())
                {
                    while (!process.HasExited)
                    {
                        if (bgworker1.CancellationPending)
                        {
                            KillProcessAndChildren(process.Id);
                            return false;
                        }
                        //process.OutputDataReceived += new DataReceivedEventHandler(p_DataReceived);
                        //process.BeginOutputReadLine();
                        string line = process.StandardOutput.ReadLine();
                        ReportProgress(taskdata, line);
                        //string errline = process.StandardError.ReadLine();   //错误流输出
                        //ReportProgress(taskdata, errline);
                    }
                    process.WaitForExit();
                }
                pFind_ok = process_exit(process);
                #endregion
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                _stop_task();
                pFind_ok = false;
            }

            return pFind_ok;
        }

        bool call_pQuant(Process process, TaskData taskdata)
        {
            bool pQuant_ok = true;
            try
            {
                _Task r_task = tasklist[_find_task_index_by_path(taskdata.Taskpath)];
                File r_file = r_task.T_File;
                #region pQuant调用
                //迟浩修改：任何时候，只要是Raw文件则调用定量程序
                //2014/9/27 wrm:定性时，不调用pQuant
                if (r_file.File_format.Equals("raw") && r_task.T_Quantitation.Quantitation_type!=(int)Quant_Type.Labeling_None)
                {
                    Factory.Create_pQuant_Instance().pQuant_writeResultFile(r_task);
                    process.StartInfo.FileName = "pQuant.exe";
                    string quant_param = "\"" + r_task.Path + "param\\pQuant.cfg\"";
                    process.StartInfo.Arguments = quant_param;
                    if (process.Start())
                    {
                        string line = "";
                        while ((line = process.StandardOutput.ReadLine()) != null)
                        {
                            if (bgworker1.CancellationPending)
                            {
                                KillProcessAndChildren(process.Id);
                                return false;
                            }
                            ReportProgress(taskdata, line);
                        }
                        //string errline="";
                        //while((errline=process.StandardError.ReadLine())!=null)
                        //{
                        //    if (bgworker1.CancellationPending)
                        //    {
                        //        KillProcessAndChildren(process.Id);
                        //        return;
                        //    }
                        //    ReportProgress(taskdata, errline);
                        //}
                        process.WaitForExit();
                    }
                    pQuant_ok=process_exit(process);
                }
                #endregion
            }
            catch (Exception exe)
            {
                MessageBox.Show(exe.Message);
                _stop_task();
                pQuant_ok = false;
            }
            return pQuant_ok;
        }

        bool call_pIsobariQ(Process process, TaskData taskdata)
        {
            bool pIsobariQ_ok = true;
            try
            {
                _Task r_task = tasklist[_find_task_index_by_path(taskdata.Taskpath)];
                File r_file = r_task.T_File;
                // 使用二级谱定量
                if (r_file.File_format.Equals("raw") && r_task.T_MS2Quant.Enable_ms2quant)
                {
                    process.StartInfo.FileName = "pIsobariQ.exe";
                    string quant_param = "\"" + r_task.Path + "param\\pIsobariQ.cfg\"";
                    process.StartInfo.Arguments = quant_param;
                    if (process.Start())
                    {
                        string line = "";
                        while ((line = process.StandardOutput.ReadLine()) != null)
                        {
                            if (bgworker1.CancellationPending)
                            {
                                KillProcessAndChildren(process.Id);
                                return false;
                            }
                            ReportProgress(taskdata, line);
                        }
                        //string errline="";
                        //while((errline=process.StandardError.ReadLine())!=null)
                        //{
                        //    if (bgworker1.CancellationPending)
                        //    {
                        //        KillProcessAndChildren(process.Id);
                        //        return;
                        //    }
                        //    ReportProgress(taskdata, errline);
                        //}
                        process.WaitForExit();
                    }
                    pIsobariQ_ok = process_exit(process);
                }
            }
            catch (Exception exe)
            {
                MessageBox.Show(exe.Message);
                _stop_task();
                pIsobariQ_ok = false;
            }
            return pIsobariQ_ok;
        }

        // 判断pBuild_dat是否存在，如果存在则删除
        void delete_pBuildTempFile(string taskPath, int stepfrom)
        { 
            string dpath = taskPath + "\\result\\pBuild_dat\\";
            if (Directory.Exists(dpath))
            {
                if (stepfrom < 4)  //删除文件夹中全部内容
                {
                    Directory.Delete(dpath,true);
                }
                else // 如果从定量开始，则删除 psm_ratio_sigma1.dat,quant_end.dat,quant_hash.dat,quant_start.dat
                {
                    string[] dfiles ={"psm_ratio_sigma1.dat", "quant_end.dat","quant_hash.dat","quant_start.dat"};
                    for (int i = 0; i < dfiles.Length; i++)
                    {
                        if (System.IO.File.Exists(dpath+dfiles[i]))
                        {
                            System.IO.File.Delete(dpath + dfiles[i]);
                        }
                    }
                }
            }      
        }

        void StartOneWork(Process process, TaskData taskdata)
        {
            try
            {
                int stepfrom = taskdata.StepFrom;
                int taskindex = _find_task_index_by_path(taskdata.Taskpath);
                _Task r_task = tasklist[taskindex];
                if (!r_task.Path.EndsWith("\\"))
                {
                    r_task.Path += "\\";
                }
                /* 禁掉该任务的右键菜单 */
                /* 禁掉部分按钮 */
                SimpleDelegate delt1 = delegate()
                {
                    DisableRightMenu(taskindex);
                    DisableButtons();
                    this.Output.AppendText("["+r_task.Path+" : start from "+ ConfigHelper.task_steps[stepfrom] +"]\n\n");
                };
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, delt1);               
                bool flow_ok=false;
                delete_pBuildTempFile(taskdata.Taskpath, stepfrom);
                if (stepfrom == 0 || stepfrom == 1)
                {
                    
                        bool parse_ok=call_pParse(process, taskdata);  //
                        if (parse_ok)
                        { 
                           bool find_ok=call_pFind(process, taskdata, "Searcher.exe");  //
                           if (find_ok) 
                           {
                               flow_ok = call_pQuant(process, taskdata);  //
                               flow_ok = call_pIsobariQ(process, taskdata);
                           }
                        }                                      
                }
                else if (stepfrom == 2)  //从鉴定
                {
                    bool find_ok = call_pFind(process, taskdata, "Searcher.exe");    //                 
                    if (find_ok)
                    {
                        flow_ok = call_pQuant(process, taskdata);    //
                        flow_ok = call_pIsobariQ(process, taskdata);
                    }                
                }
                else if (stepfrom == 3) //从过滤
                {
                    if (ConfigHelper.checkFileBySuffix(r_task.Path + "result", ".qry.res"))
                    {
                        bool filter_ok = call_pFind(process, taskdata, "Group.exe");  //                    
                        if (filter_ok)
                        {
                            //chihao 20140812: 内核现在做了修改，在groupfilter里面调用summary，逻辑上更合理一些，因为summary本来就是针对过滤后的结果进行统计，是个附加功能
                            flow_ok = call_pQuant(process, taskdata);    //
                            flow_ok = call_pIsobariQ(process, taskdata);
                            //bool summary_ok = call_pFind(process, taskdata, "Summary.exe");  //
                            //if (summary_ok) 
                            //{
                                
                            //}
                        }
                        else
                        {
                            _stop_task();
                        }
                    }                   
                }
                else if (stepfrom == 4) //从定量
                {
                    if (System.IO.File.Exists(r_task.Path + "result\\pFind.spectra"))
                    {
                        flow_ok = call_pQuant(process, taskdata);    //
                        flow_ok = call_pIsobariQ(process, taskdata);
                    }                 
                }
                #region pBuild调用
                if (flow_ok)
                {
                    SimpleDelegate delt2 = delegate()
                    {
                        int idx = IndexOfRunning(runtasklist);
                        if (idx != -1)
                        {
                            try
                            {
                                string p = r_task.Path;
                                if (p[p.Length - 1] == '\\')
                                {
                                    p = p.Substring(0, p.Length - 1);
                                }
                                System.Diagnostics.Process.Start("\"" + ConfigHelper.startup_path + "\\pBuild.exe" + "\"", "\"" + p + "\"");
                            }
                            catch (Exception ex)
                            {
                                System.Windows.MessageBox.Show(ex.Message);
                                _stop_task();
                            }
                            taskdata.Statusindex = (int)TaskStatus.done;
                            taskdata.Status = "Done";
                            TimeSpan ts = (DateTime.Now.Subtract(taskdata.Start_time));
                            taskdata.Run_time = ts.Hours.ToString().PadLeft(2, '0') + ":" + ts.Minutes.ToString().PadLeft(2, '0') + ":" + ts.Seconds.ToString().PadLeft(2, '0');  //
                            runtasklist[idx] = taskdata;
                            this.runningtasks.ItemsSource = runtasklist;
                            this.Output.AppendText("\n== == == == == Total Time Used for This Task: " + ts.Hours.ToString().PadLeft(2, '0') + ":" + ts.Minutes.ToString().PadLeft(2, '0') + ":" + ts.Seconds.ToString().PadLeft(2, '0') + " == == == == ==\n\n");                          
                        }
                        /* 菜单解禁 */
                        EnableRightMenu(taskindex);
                        EnableButtons();
                    };
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, delt2);
                }
                else
                {
                    _stop_task();
                }
                #endregion
            }
            catch(Exception exe)
            {
                MessageBox.Show(exe.Message);
                _stop_task();
            }
        }
        ///<summary
        ///分步执行
        ///</summary>
        private void DropDown_ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            //if (bgworker1 != null && bgworker1.IsBusy)  //there is a task running
            //{
            //    this.dropdownBtn.IsEnabled = false;
            //    string tip = "Sorry, there is a task running! So you cannot step through.\n";
            //    display_temporary_tooltip(this.startBtn,PlacementMode.Bottom,tip);
            //    return;
            //}
            MenuItem mi = e.Source as MenuItem;
            int stepfrom = 0;
            if(!current_task.Path.EndsWith("\\"))
            {
                current_task.Path+="\\";
            }
            switch (mi.Header.ToString())
            {
                case "from Data Extraction": stepfrom = 1; break;
                case "from Identification": stepfrom = 2; break;
                case "from Result Filter":
                    {
                        if (ConfigHelper.checkFileBySuffix(current_task.Path + "result", ".qry.res"))
                        {
                            stepfrom = 3;
                        }
                        else
                        {
                            MessageBox.Show(Message_Help.NO_FILTER_FILE);
                            return;
                        }
                        break;
                    }
                case "from Quantitation":
                    {
                        if (System.IO.File.Exists(current_task.Path + "result\\pFind.spectra"))
                        {
                            stepfrom = 4; 
                        }
                        else
                        {
                            MessageBox.Show(Message_Help.NO_PFIND_SPECTRA);
                            return;
                        }
                        break;
                    }
            }

            if (CheckCurrentBeforeStart())
            {
                this.RABtn.Visibility = Visibility.Collapsed;
                JustRun(current_task_index, current_task.Task_name, stepfrom);               
            }
            else
            {
                System.Windows.MessageBox.Show("The configuration of " + current_task.Task_name + " is not completed!", "pFind warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.tab.SelectedIndex = 3;
            }                
        }

        private int IndexOfRunning(ObservableCollection<TaskData> runlst)
        { 
            int idx=-1;
            int cnt = runlst.Count;
            for (int i = 0; i < cnt; i++)
            {
                if (runlst[i].Statusindex == (int)TaskStatus.running)
                {
                    idx = i;
                    break;
                }
            }
            return idx;
        }


        private void p_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                #region 如何传递TaskData这一参数
                #endregion
                int i=IndexOfRunning(runtasklist);
                if (i != -1)
                {
                    ReportProgress(runtasklist[i], e.Data);
                }
            }
        }

        string alreadyoutput = "";
        private void ReportProgress(TaskData td, string line)
        {
            SimpleDelegate del = delegate()
            {
                td.Statusindex = (int)TaskStatus.running;
                td.Status = "Running";
                TimeSpan ts = (DateTime.Now.Subtract(td.Start_time));
                td.Run_time = ts.Hours.ToString().PadLeft(2, '0') + ":" + ts.Minutes.ToString().PadLeft(2, '0') + ":" + ts.Seconds.ToString().PadLeft(2, '0');  //
                string pattern =@"<.*>.*:.*\d+%";
                Regex r = new Regex(pattern,RegexOptions.IgnoreCase);
                if (line != null && r.IsMatch(line))
                {
                    this.Output.Text = alreadyoutput + line + "\n"; 
                    this.Output.Select(this.Output.Text.Length,0);
                    int index1 = line.LastIndexOf("<");
                    td.Status = line.Substring(index1 + 1, line.LastIndexOf(">") - index1 - 1);
                    td.ProgressText = line.Substring(line.LastIndexOf(":") + 1).Trim();
                    td.Progress = (int)float.Parse(td.ProgressText.Substring(0, td.ProgressText.LastIndexOf("%")));
                }
                else if (line != null && line.Contains("Extracting MS2"))
                {
                    ms2_cnt++;
                    this.Output.Text = alreadyoutput + line + "\n";
                    this.Output.Select(this.Output.Text.Length,0);
                    //this.Output.ScrollToEnd();
                    td.Status = "Extracting MS2";
                    td.Progress=(2 * ms2_cnt) > 100 ? 100:2*ms2_cnt;
                    td.ProgressText = td.Progress.ToString() + "%";
                }
                else
                {
                    this.Output.AppendText(line + "\n");
                    this.Output.ScrollToEnd();      //滚动置底
                    alreadyoutput = this.Output.Text;
                }
                int i = IndexOfRunning(runtasklist);
                runtasklist[i] = td;
                //runtasklist.RemoveAt(i);
                //runtasklist.Insert(i, td);
                this.Output.ScrollToEnd(); 
                this.runningtasks.ItemsSource = runtasklist;
                this.status1.Text = "Running";
            };
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, del);
        }

        #region Todo
        //异步启动之后
        #endregion
        private void SearchWorkDone(object sender,RunWorkerCompletedEventArgs e)
        {
            TaskListItemRemoved();
            if (e.Cancelled)
            {
                #region Todo
                //after cancel,run next 
                #endregion
                int idx = IndexOfRunning(runtasklist);
                if (idx != -1)
                {
                    TaskData td=runtasklist[idx];
                    System.Windows.MessageBox.Show(td.Taskname + " has been stopped!");
                    td.Statusindex = (int)TaskStatus.waiting;
                    td.Status = "Waiting";
                    TimeSpan ts = (DateTime.Now.Subtract(td.Start_time));
                    td.Run_time = ts.Hours.ToString().PadLeft(2, '0') + ":" + ts.Minutes.ToString().PadLeft(2, '0') + ":" + ts.Seconds.ToString().PadLeft(2, '0');  //
                    runtasklist.RemoveAt(idx);
                    runtasklist.Insert(idx, td);
                    this.runningtasks.ItemsSource = runtasklist;
                    //enable 刚刚运行的任务的右键菜单
                    int taskindex = _find_task_index_by_path(td.Taskpath);
                    if (taskindex != -1) 
                    {
                        EnableRightMenu(taskindex);
                    }
                }
                this.RABtn.Visibility = Visibility.Visible;
                this.status1.Text = "Stopped!";               
            }
            else if (e.Error != null)
            {
                System.Windows.MessageBox.Show(e.Error.Message,"An error occured!");                
            }
            else
            {
                //System.Windows.MessageBox.Show("Work done!");
                this.status1.Text = "Done!";
            }
            EnableButtons();   
            this.dropdownBtn.IsEnabled = true;
            return;
        }
        /***********************************菜单栏交互逻辑**************************************************/
        //运行队列中已存在该任务, 即状态不为removed
        private int ExistInRunningList(int idx)
        {
            for (int i = 0; i < runtasklist.Count; i++)
            {
                if (runtasklist[i].Taskpath == tasklist[idx].Path && runtasklist[i].Statusindex!=(int)TaskStatus.removed)
                {
                    return i;
                }
            }
            return -1;
        }
        private void RunAllTasks(object sender, RoutedEventArgs e)
        {
            try
            {
                if (taskChanged())
                {
                    System.Windows.MessageBoxResult res = System.Windows.MessageBox.Show("Do you want to save the changes before running?", "pFind", MessageBoxButton.YesNoCancel);
                    if (res == MessageBoxResult.Yes)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            saveDisplayParam();
                        });
                        Factory.Create_Copy_Instance().FileCopy(_file, current_task.T_File);
                        Factory.Create_Copy_Instance().SearchParamCopy(_search, current_task.T_Search);
                        Factory.Create_Copy_Instance().FilterParamCopy(_filter, current_task.T_Filter);
                        Factory.Create_Copy_Instance().QuantitaionCopy(_quantitation, current_task.T_Quantitation);
                        current_task.T_MS2Quant = Factory.Create_Copy_Instance().DeepCopyWithXmlSerializer(_ms2quant);
                        Factory.Create_Run_Instance().SaveParams(current_task);
                    }
                    else if (res == System.Windows.MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }
                int index = 0;
                for (; index < tasklist.Count; ++index)
                {
                    tasklist[index].Check_ok = Factory.Create_Run_Instance().Check_Task(tasklist[index]);
                    if (tasklist[index].Check_ok == false)
                    {
                        System.Windows.MessageBox.Show("The configuration of " + tasklist[index].Task_name + " is not completed!", "pFind warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Dispatcher.Invoke(() =>
                        {
                            this.tab.SelectedIndex = 3;
                        });
                        return;
                    }
                }

                for (int i = 0; i < tasklist.Count; i++)
                {
                    int ret = ExistInRunningList(i);
                    if (ret == -1)
                    {
                        TaskData td = new TaskData();
                        td.Taskpath = tasklist[i].Path;
                        td.Taskname = tasklist[i].Task_name;
                        td.Statusindex = (int)TaskStatus.waiting;
                        td.Status = "Waiting";
                        td.Progress = 0;
                        td.ProgressText = "0%";
                        td.Start_time = DateTime.Now;
                        td.StepFrom = 0;
                        TimeSpan ts = (DateTime.Now.Subtract(td.Start_time));
                        td.Run_time = ts.Hours.ToString().PadLeft(2, '0') + ":" + ts.Minutes.ToString().PadLeft(2, '0') + ":" + ts.Seconds.ToString().PadLeft(2, '0');
                        runtasklist.Add(td);

                    }
                    else
                    {
                        if (runtasklist[ret].Statusindex != (int)TaskStatus.running)
                        {
                            runtasklist[ret].Start_time = DateTime.Now;
                            TimeSpan ts = (DateTime.Now.Subtract(runtasklist[ret].Start_time));
                            runtasklist[ret].Run_time = ts.Hours.ToString().PadLeft(2, '0') + ":" + ts.Minutes.ToString().PadLeft(2, '0') + ":" + ts.Seconds.ToString().PadLeft(2, '0');
                            if (runtasklist[ret].Statusindex == (int)TaskStatus.done)
                            {
                                TaskData td = new TaskData();
                                td.Statusindex = (int)TaskStatus.waiting;
                                td.Status = "waiting";
                                td.Progress = 0;
                                td.ProgressText = "0%";
                                td.Taskpath = runtasklist[ret].Taskpath;
                                td.Taskname = runtasklist[ret].Taskname;
                                td.Start_time = runtasklist[ret].Start_time;
                                td.Run_time = runtasklist[ret].Run_time;
                                td.StepFrom = 0;
                                runtasklist[ret].Statusindex = (int)TaskStatus.removed;
                                removed_num++;
                                runtasklist.Add(td);
                            }
                        }
                    }
                }
                this.runningtasks.ItemsSource = runtasklist;
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.runningtasks.ItemsSource);
                view.Filter = new Predicate<object>(o => { return (o as TaskData).Statusindex != (int)TaskStatus.removed; });

                ShowDragEffect();

                if (bgworker1 != null && bgworker1.IsBusy)  //there is a task running
                {

                }
                else   //there is no task running:(1)work done;(2)no work 
                {

                    bgworker1 = new BackgroundWorker();
                    bgworker1.WorkerSupportsCancellation = true;
                    bgworker1.DoWork += new DoWorkEventHandler(DoSearchWork);
                    bgworker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SearchWorkDone);
                    bgworker1.RunWorkerAsync();
                }
            }
            catch(Exception exe)
            {
                MessageBox.Show(exe.Message);
            }
        }

        

        private void DoSearchWork(object sender, DoWorkEventArgs e)
        {
            StartSearchWork();
            if (bgworker1.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
        }

        private void export_pdf(object sender, RoutedEventArgs e)
        {          
              var sender_obj = sender as Button;
              if (sender_obj != this.exp_pdf)
                  return;
              if (Directory.Exists(current_task.Path))
              {
              }
              else
              {
                  Directory.CreateDirectory(current_task.Path);
                  Directory.CreateDirectory(current_task.Path + "\\param");
                  Directory.CreateDirectory(current_task.Path + "\\result");
              }  
              export_PDF("summary.pdf");           
        }

        private void export_PDF(string fileName)
        {
            try
            {                              
                MemoryStream lMemoryStream = new MemoryStream();
                Package package = Package.Open(lMemoryStream, FileMode.Create);
                XpsDocument doc = new XpsDocument(package);
                XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(doc);               
                writer.Write(this.all_summary);
                doc.Close();
                package.Close();
                var pdfXpsDoc = PdfSharp.Xps.XpsModel.XpsDocument.Open(lMemoryStream);
                string c_path = current_task.Path + "param\\" + fileName;
                PdfSharp.Xps.XpsConverter.Convert(pdfXpsDoc, c_path, 0);               
                System.Diagnostics.Process.Start(c_path);
            }
            catch (Exception e)
            {
                //迟浩：信息补充完整
                System.Windows.MessageBox.Show(e.Message,"Failed to export PDF files");
                return;
            }
        }

        /*************************************************Task Queue模块交互逻辑**********************************************************/

        private void runningtasks_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void ShowDragEffect()
        {
            // This is all that you need to do, in order to use the ListViewDragManager.
            this.dragMgr = new ListViewDragDropManager<TaskData>(this.runningtasks);

            // Turn the ListViewDragManager on and off. 
            this.dragMgr.ListView = this.runningtasks;

            // Show and hide the drag adorner.
            this.dragMgr.ShowDragAdorner = true;
            this.dragMgr.DragAdornerOpacity = 0.8;

            // Hook up events on both ListViews to that we can drag-drop
            // items between them.
            this.runningtasks.DragEnter += OnListViewDragEnter;
            this.runningtasks.Drop += OnListViewDrop;
        }

        // Handles the DragEnter event for both ListViews.
        void OnListViewDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }

        // Handles the Drop event for both ListViews.
        void OnListViewDrop(object sender, DragEventArgs e)
        {
            if (e.Effects == DragDropEffects.None)
                return;

            TaskData task = e.Data.GetData(typeof(TaskData)) as TaskData;
            if (sender == this.runningtasks)
            {
                if (this.dragMgr.IsDragInProgress)
                    return;

                // An item was dragged from the bottom ListView into the top ListView
                // so remove that item from the bottom ListView.
                //(this.listView2.ItemsSource as ObservableCollection<Task>).Remove(task);
            }
            else
            {
                //if (this.dragMgr2.IsDragInProgress)
                //   return;

                // An item was dragged from the top ListView into the bottom ListView
                // so remove that item from the top ListView.
                (this.runningtasks.ItemsSource as ObservableCollection<TaskData>).Remove(task);
            }
        }

        private void RemoveTask(object sender, RoutedEventArgs e)
        {
            //bgworker1.CancelAsync();    //发出取消命令        
            //MenuItem mi = sender as MenuItem;
            //System.Windows.MessageBox.Show(ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(mi)).GetType().ToString());
           
            #region Todo
            #endregion
            for (int i = 0; i < this.runningtasks.SelectedItems.Count; i++)
            {
                TaskData td=(TaskData)this.runningtasks.SelectedItems[i];
                if (td.Statusindex != (int)TaskStatus.running)
                {
                    int index = runtasklist.IndexOf(td);
                    runtasklist[index].Statusindex = (int)TaskStatus.removed;    //在后台队列加标志
                    this.runningtasks.ItemsSource = runtasklist;
                    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.runningtasks.ItemsSource);
                    view.Filter = new Predicate<object>(o => { return (o as TaskData).Statusindex != (int)TaskStatus.removed; });
                    removed_num++;
                }
                else
                {
                    string cnt = "sorry,the running task can not be moved now!";             
                    ListViewItem lvi = this.runningtasks.ItemContainerGenerator.ContainerFromIndex(this.runningtasks.SelectedIndex) as ListViewItem;
                    display_temporary_tooltip(lvi,PlacementMode.Top,cnt);
                }
            }
            int realnum = 0;
            for (int i = 0; i < this.runtasklist.Count; i++)
            {
                if (runtasklist[i].Statusindex != (int)TaskStatus.removed)
                {
                    realnum++;
                }
            }
            if (realnum == 0) this.RABtn.Visibility = Visibility.Collapsed;
        }

        private void _stop_task()
        {
            
            if (bgworker1 != null && bgworker1.IsBusy)
            {
                bgworker1.CancelAsync();
            }
        }
        private bool process_exit(Process process)
        {
            if (process.ExitCode != 0) //不是正常退出
            {
                //Thread.Sleep(2000);
                _stop_task();
                return false;
            }
            return true;
        }
        private void StopTask(object sender, RoutedEventArgs e)
        {
            _stop_task();
        }

        private void RunTasks_Click(object sender, RoutedEventArgs e)
        {
            //wrm 20141020: 改成start前无条件重写参数文件
            try
            {
                _task_save();

                this.Output.Clear();
                bgworker1 = new BackgroundWorker();
                bgworker1.WorkerSupportsCancellation = true;
                bgworker1.DoWork += new DoWorkEventHandler(DoSearchWork);
                bgworker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SearchWorkDone);
                bgworker1.RunWorkerAsync();
                this.RABtn.Visibility = Visibility.Collapsed;
                this.dragMgr = null;
                this.AllowDrop = false;

                //if (this.runtasklist.Count > 0)
                //{
                //    int topindex = 0;
                //    while (topindex < runtasklist.Count && runtasklist[topindex].Statusindex != (int)TaskStatus.waiting)
                //    {
                //        ++topindex;
                //    }
                //    runtasklist[topindex].Start_time = DateTime.Now;
                //    TimeSpan ts = (DateTime.Now.Subtract(runtasklist[topindex].Start_time));
                //    runtasklist[topindex].Run_time = ts.Hours.ToString().PadLeft(2, '0') + ":" + ts.Minutes.ToString().PadLeft(2, '0') + ":" + ts.Seconds.ToString().PadLeft(2, '0');
                //}
            }
            catch(Exception exe)
            {
                MessageBox.Show(exe.Message);
            }

        }
        /*to the bottom of the waiting list*/
        private void Task_Bottom(object sender, RoutedEventArgs e)
        {
            if (runtasklist.Count <= 0)
            {
                return;
            }
            if (runningtasks.SelectedIndex < 0)
            {
                return;
            }
            //TaskData td=(TaskData)this.runningtasks.SelectedItems[i];
            if (this.runningtasks.SelectedIndex < this.runtasklist.Count - 1 && runtasklist[this.runningtasks.SelectedIndex].Statusindex == (int)TaskStatus.waiting)
            {
                this.runtasklist.Move(this.runningtasks.SelectedIndex, this.runtasklist.Count - 1);
                this.runningtasks.ItemsSource = runtasklist;
            }
            else
            {
                ToolTip tp = new ToolTip();
                tp.Content = "Sorry,can't move any more!";
                ListViewItem lvi = this.runningtasks.ItemContainerGenerator.ContainerFromIndex(this.runningtasks.SelectedIndex) as ListViewItem;
                lvi.ToolTip = tp;
                tp.IsOpen = true;
                DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(2) };
                timer.Tick += delegate(object o, EventArgs arg)
                {
                    ((DispatcherTimer)timer).Stop();
                    tp.IsOpen = false;
                };
                timer.Start();

            }
        }
        /*to the top of the waiting list*/
        //迟浩：感觉之前逻辑有点问题？
        private void Task_Top(object sender, RoutedEventArgs e)
        {
            if (runtasklist.Count <= 0)
            {
                return;
            }
            if (this.runningtasks.SelectedIndex < 0)
            {
                return;
            }
            int topindex = 0;
            while (topindex < runtasklist.Count && runtasklist[topindex].Statusindex == (int)TaskStatus.running)
            {
                ++topindex;
            }
            //TaskData td=(TaskData)this.runningtasks.SelectedItems[i];
            if (topindex < this.runningtasks.SelectedIndex && runtasklist[this.runningtasks.SelectedIndex].Statusindex == (int)TaskStatus.waiting)
            {
                this.runtasklist.Move(this.runningtasks.SelectedIndex, topindex);
                this.runningtasks.ItemsSource = runtasklist;
            }
            else
            {
                ToolTip tp = new ToolTip();
                //tp.Style = (Style)this.FindResource("SimpleTip");
                tp.Content = "Sorry,can't move any more!";
                //CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.runningtasks.ItemsSource);
                ListViewItem lvi = this.runningtasks.ItemContainerGenerator.ContainerFromIndex(this.runningtasks.SelectedIndex) as ListViewItem;
                lvi.ToolTip = tp;
                tp.IsOpen = true;
                DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(2) };
                timer.Tick += delegate(object o, EventArgs arg)
                {
                    ((DispatcherTimer)timer).Stop();
                    tp.IsOpen = false;
                };
                timer.Start();
            }
        }
        private void Task_Up(object sender, RoutedEventArgs e)
        {
            int idx = this.runningtasks.SelectedIndex;
            if (idx == -1)
            {
                return;
            }
            if (idx > 0 && runtasklist[idx].Statusindex == (int)TaskStatus.waiting && runtasklist[idx - 1].Statusindex == (int)TaskStatus.waiting)   //只有waiting队列可以调序
            {
                runtasklist.Move(this.runningtasks.SelectedIndex, idx - 1);
            }
            else
            {
                ToolTip tp = new ToolTip();
                //tp.Style = (Style)this.FindResource("SimpleTip");
                tp.Content = "Sorry,can't move up any more!";
                //CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.runningtasks.ItemsSource);
                ListViewItem lvi = this.runningtasks.ItemContainerGenerator.ContainerFromIndex(idx) as ListViewItem;
                lvi.ToolTip = tp;
                tp.IsOpen = true;
                DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(2) };
                timer.Tick += delegate(object o, EventArgs arg)
                {
                    ((DispatcherTimer)timer).Stop();
                    tp.IsOpen = false;
                };
                timer.Start();
            }
            this.runningtasks.ItemsSource = runtasklist;
        }
        private void Task_Down(object sender, RoutedEventArgs e)
        {
            int idx = this.runningtasks.SelectedIndex;
            if (idx < 0)
            {
                return;
            }
            if (idx < this.runtasklist.Count - 1 && runtasklist[idx].Statusindex == (int)TaskStatus.waiting && runtasklist[idx + 1].Statusindex == (int)TaskStatus.waiting)
            {
                this.runtasklist.Move(this.runningtasks.SelectedIndex, idx + 1);
            }
            else
            {
                ToolTip tp = new ToolTip();
                //tp.Style = (Style)this.FindResource("SimpleTip");
                tp.Content = "Sorry,can't move down any more!";
                ListViewItem lvi = this.runningtasks.ItemContainerGenerator.ContainerFromIndex(idx) as ListViewItem;
                lvi.ToolTip = tp;
                tp.IsOpen = true;
                DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(2) };
                timer.Tick += delegate(object o, EventArgs arg)
                {
                    ((DispatcherTimer)timer).Stop();
                    tp.IsOpen = false;
                };
                timer.Start();
            }
            this.runningtasks.ItemsSource = runtasklist;
        }

        private void TaskSaveAs(object sender, RoutedEventArgs e)
        {
            try
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
                string task_path = f_dialog.SelectedPath;
                if (tab.SelectedIndex != (int)TabOption.Summary)
                {
                    saveDisplayParam();
                }
                Factory.Create_Copy_Instance().FileCopy(_file, current_task.T_File);
                Factory.Create_Copy_Instance().SearchParamCopy(_search, current_task.T_Search);
                Factory.Create_Copy_Instance().FilterParamCopy(_filter, current_task.T_Filter);
                Factory.Create_Copy_Instance().QuantitaionCopy(_quantitation, current_task.T_Quantitation);
                current_task.T_MS2Quant = Factory.Create_Copy_Instance().DeepCopyWithXmlSerializer(_ms2quant);
                task_path += "\\" + current_task.Task_name + "\\";
                System.IO.File.Copy(current_task.Path + "\\result", task_path + "\\result", true);
                current_task.Path = task_path;

                bool flag = Factory.Create_Run_Instance().SaveParams(current_task);

                if (flag)
                {

                }
                else
                {

                }
            }
            catch(Exception exe)
            {
                MessageBox.Show("[Task Save As] "+exe.Message);
            }
        }


        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            SettingsWindow dlg = new SettingsWindow(settings);
            bool dresult = (bool)dlg.ShowDialog();

        }
        private void ToolBar_Loaded_1(object sender, RoutedEventArgs e)
        {
            ToolBar toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }

            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
            {
                mainPanelBorder.Margin = new Thickness(0);
            }
        }

        private void OpenConfig(object sender, RoutedEventArgs e)
        {
            Call_pConfig(-1);
        }
        private void Call_pConfig(int tabindex)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "pConfig.exe";
                if (tabindex > -1)
                {
                    process.StartInfo.Arguments = tabindex.ToString();
                }
                process.Start();
                process.WaitForExit();
                // 读取新添加的元数据配置信息
                ConfigHelper.getNewAddedInfo();

                //wrm 20141104: InitializeContent();该函数会重置数据库
                switch(tabindex)
                {
                    //database
                    case 0: show_DB();
                        break;
                    //modification
                    case 1: ConfigHelper.get_mods();
                        break;
                    //quantitation
                    case 2: ConfigHelper.get_labels();
                        break;
                    //enzyme
                    case 3: show_enzyme();
                        break;
                    default: show_DB();
                             ConfigHelper.get_mods();
                             ConfigHelper.get_labels();
                             show_enzyme();
                             break;
                }
                
                //2014/9/27 wrm:更新label和mod列表
                InitialListInfo();
            }
            catch (Exception exe)
            {
                System.Windows.MessageBox.Show("Sorry, something is wrong with pConfig.exe!\n" + exe.Message);
            }
        }

        private void DoubleClickMSData(object sender, MouseButtonEventArgs e)
        {
            this.tab.SelectedIndex = 0;
        }
        private void DoubleClickSearch(object sender, MouseButtonEventArgs e)
        {
            this.tab.SelectedIndex = 1;
            this.filter.IsExpanded = false;
        }
        private void DoubleClickFilter(object sender, MouseButtonEventArgs e)
        {
            this.tab.SelectedIndex = 1;
            this.filter.IsExpanded = true;
        }
        private void DoubleClickQuant(object sender, MouseButtonEventArgs e)
        {
            this.tab.SelectedIndex = 2;
        }

        private void SumScrollChanged(object sender, SizeChangedEventArgs e)
        {
            this.SumScroll.ScrollToEnd();
        }

        private void BeforeTabChange(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //System.Windows.MessageBox.Show(sender.GetType().ToString());
                //if ()
                {
                    int index = this.tab.SelectedIndex;
                    switch (index)
                    {
                        case 0: File_save();
                            break;
                        case 1: Identification_save();
                            break;
                        case 2: Quantitation_save();
                            break;
                    }
                }
                
            }
            catch (Exception exe)
            {
                System.Windows.MessageBox.Show(exe.Message);
                e.Handled = true;
                return;
            }
            
        }

        private void BeforeItemChange(object sender, RoutedEventArgs e)
        {
            try
            {
                //System.Windows.MessageBox.Show(this.tab.SelectedIndex.ToString());
                //if ()
                {
                    int index = this.tab.SelectedIndex;
                    switch (index)
                    {
                        case 0: File_save();
                            break;
                        case 1: Identification_save();
                            break;
                        case 2: Quantitation_save();
                            break;
                    }
                }
                
            }
            catch (Exception exe)
            {
                System.Windows.MessageBox.Show(exe.Message);
                e.Handled = true;
                return;
            }
        }

        private void Save_Output_Report(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.Output.Text.Trim().Length > 0)
                {
                    if (bgworker1 != null && bgworker1.IsBusy)
                    {
                        string info = "Sorry, you can only save report when there's no task running!";
                        display_temporary_tooltip(this.saveReportBtn as Button,PlacementMode.Right,info);
                        return;
                    }
                    string log_path = System.Windows.Forms.Application.StartupPath + "\\log\\";
                    if (!Directory.Exists(log_path))
                    {
                        Directory.CreateDirectory(log_path);
                    }
                    string date = DateTime.Now.Date.ToString("yyyy-MM-dd");
                    //System.Windows.MessageBox.Show(date);
                    string filepath = log_path + "running_log_" + date+".log";
                    
                    FileStream fs = new FileStream(filepath, FileMode.OpenOrCreate);
                    StreamReader sr = new StreamReader(fs, Encoding.Default);
                    StringBuilder sb = new StringBuilder(sr.ReadToEnd());
                    StreamWriter sw = new StreamWriter(fs, Encoding.Default);
                    sb.Append(this.Output.Text);
                    sw.Write(sb);
                    sw.Close();
                    sr.Close();
                    Process.Start("write.exe","\""+filepath+"\"");
                }
                else
                {
                    string info = "Sorry, there is no output content!";
                    display_temporary_tooltip(this.saveReportBtn as Button,PlacementMode.Right, info);
                }
            }
            catch (Exception exe)
            {
                System.Windows.MessageBox.Show(exe.Message);
            }
        }

        void display_temporary_tooltip(UIElement ui,PlacementMode mode,string info)
        {
            ToolTip tip = new ToolTip();
            tip.PlacementTarget = ui;
            tip.Placement = mode;
            //tip.HorizontalOffset=10;
            //tip.VerticalOffset=10;
            tip.Background = Brushes.LightCyan;
            TextBlock tiptext = new TextBlock();
            tiptext.Text = info;
            tip.Content = tiptext;
            tip.IsOpen = true;
            DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(2) };
            timer.Tick += delegate(object o, EventArgs arg)
            {
                ((DispatcherTimer)timer).Stop();
                tip.IsOpen = false;
            };
            timer.Start();
        }

        private void Open_the_folder(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < this.fileList.SelectedItems.Count; i++)
            {
                _FileInfo fi = this.fileList.SelectedItems[i] as _FileInfo;
                string fpath = fi.FilePath;
                if (fpath[fpath.Length - 1] == '\\') { fpath=fpath.Substring(0,fpath.Length-1); }
                fpath = fpath.Substring(0,fpath.LastIndexOf("\\"));
                if (System.IO.Directory.Exists(fpath))
                {
                    System.Diagnostics.Process.Start("explorer.exe", fpath);
                }
                else
                {
                    System.Windows.MessageBox.Show("Sorry, " + "\"" + fpath + "\"" + " is not existed. Please make sure that the path is valid, or the task has been successfully saved.");
                }
            }
        }

        private void DatabaseChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Database.Items.Count > 0 && this.Database.SelectedIndex + 1 == this.Database.Items.Count)
            {
                Call_pConfig(0);
            }
        }

        private void None_Quantitation_Tip(object sender, MouseButtonEventArgs e)
        {
            if (this.FormatChoices.SelectedIndex== (int)FormatOptions.MGF)
            {
                display_temporary_tooltip(this.QuantTab,PlacementMode.Bottom,Message_Help.NONE_QUANTITATION);
            }
            if (this.QuantTab.IsEnabled == false)
            {
                display_temporary_tooltip(this.QuantTab,PlacementMode.Bottom,Message_Help.NO_EDIT_WHEN_RUN);
            }
        }

        private void No_Editting_Tip(object sender, MouseButtonEventArgs e)
        {
            TabItem ti = sender as TabItem;
            if (ti != null)
            {
                switch (ti.Header.ToString())
                {
                    case "MS Data":
                        if (this.FileTab.IsEnabled == false)
                        {
                            display_temporary_tooltip(this.FileTab, PlacementMode.Bottom, Message_Help.NO_EDIT_WHEN_RUN);
                        }
                        break;
                    case "Identification":
                        if (this.FileTab.IsEnabled == false)
                        {
                            display_temporary_tooltip(this.IdentificationTab, PlacementMode.Bottom, Message_Help.NO_EDIT_WHEN_RUN);
                        }
                        break;                   
                }
            }
        }

        private void Config_Labels(object sender, RoutedEventArgs e)
        {
            Call_pConfig(2);
        }

        private void Config_Mods(object sender, RoutedEventArgs e)
        {
            Call_pConfig(1);
        }
        /// <summary>
        /// 运行中需要禁掉的按钮
        /// </summary>
        private void DisableButtons()   
        { 
            //save
            this.menuSave.IsEnabled = false;
            this.toolSave.IsEnabled = false;
            this.sumSave.IsEnabled = false;
            this.saveBtn.IsEnabled = false;
            //start
            this.toolRun.IsEnabled = false;
            this.sumRun.IsEnabled = false;
            this.startBtn.IsEnabled = false;
            this.dropdownBtn.IsEnabled = false;
            //remove
            this.toolRemove.IsEnabled = false;
            //delete
            this.toolDelete.IsEnabled = false;
            //stop
            this.toolStop.IsEnabled = true;
            //MS Data, Indentification, Quantitation
            this.FileTab.IsEnabled = false;
            this.IdentificationTab.IsEnabled = false;
            this.QuantTab.IsEnabled = false;

            //运行时不允许修改线程和输出路径
            manuItemSettings.IsEnabled = false;

            //切换到summary
            this.tab.SelectedIndex = 3;
            if (this.FileTab.IsEnabled == false && this.IdentificationTab.IsEnabled == false && this.QuantTab.IsEnabled == false)
            {
                display_temporary_tooltip(this.SummaryTab, PlacementMode.Left, Message_Help.NO_EDIT_WHEN_RUN);
            }
        }
        /// <summary>
        /// 当该任务为非运行状态时，按钮的状态
        /// </summary>
        private void EnableButtons()
        {
            //save
            this.menuSave.IsEnabled = true;
            this.toolSave.IsEnabled = true;
            this.sumSave.IsEnabled = true;
            this.saveBtn.IsEnabled = true;
            //start
            this.toolRun.IsEnabled = true;
            this.sumRun.IsEnabled = true;
            this.startBtn.IsEnabled = true;
            this.dropdownBtn.IsEnabled = true;
            //remove
            this.toolRemove.IsEnabled = true;
            //delete
            this.toolDelete.IsEnabled = true;
            //stop
            this.toolStop.IsEnabled = false;
            //MS Data, Indentification, Quantitation
            this.FileTab.IsEnabled = true;
            this.IdentificationTab.IsEnabled = true;
            this.QuantTab.IsEnabled = true;

            //停止时可修改线程和输出路径
            manuItemSettings.IsEnabled = true;
        }
        /// <summary>
        /// 运行时，需要禁掉的右键菜单,及下拉节点
        /// </summary>
        private void DisableRightMenu(int treeIndex)
        { //run,rename,remove,delete,run pBuid
            TreeViewItem tvi = this.TaskTree.Items[treeIndex] as TreeViewItem;
            for (int i = 0; i < tvi.Items.Count; i++)
            {
                TreeViewItem tvi_child = tvi.Items[i] as TreeViewItem;
                if (!tvi_child.Header.ToString().Equals("Summary"))
                {
                    tvi_child.IsEnabled=false;
                }
            }
            if (tvi.ContextMenu != null)
            {
                ContextMenu cm = tvi.ContextMenu;
                for (int i = 0; i < cm.Items.Count; i++)
                {
                    MenuItem mi = cm.Items[i] as MenuItem;
                    if (mi != null)
                    {
                        string header = mi.Header.ToString();
                        if (header.Equals("Run") || header.Equals("Rename") || header.Equals("Remove") || header.Equals("Delete") || header.Equals("Run pBuild"))
                        {
                            mi.IsEnabled = false;
                        }
                    }
                }           
            }
        }
        /// <summary>
        /// 非运行态时，恢复右键菜单，下拉节点
        /// </summary>
        private void EnableRightMenu(int treeIndex)
        {
            TreeViewItem tvi = this.TaskTree.Items[treeIndex] as TreeViewItem;
            for (int i = 0; i < tvi.Items.Count; i++)
            {
                TreeViewItem tvi_child = tvi.Items[i] as TreeViewItem;
                if (!tvi_child.Header.ToString().Equals("Summary"))
                {
                    tvi_child.IsEnabled = true;
                }
            }
            if (tvi.ContextMenu != null)
            {
                ContextMenu cm = tvi.ContextMenu;
                for (int i = 0; i < cm.Items.Count; i++)
                {
                    MenuItem mi = cm.Items[i] as MenuItem;
                    if (mi != null)
                    {
                        string header = mi.Header.ToString();
                        if (header.Equals("Run") || header.Equals("Rename") || header.Equals("Remove") || header.Equals("Delete") || header.Equals("Run pBuild"))
                        {
                            mi.IsEnabled = true;
                        }
                    }
                }

            }
        }

        //-------------------------------------二级谱定量模块-----------------------------------

        private void select_ms2quant_method(object sender, SelectionChangedEventArgs e)
        {
            switch (this.ms2QuantMethod.SelectedIndex)
            {
                case (int)MS2QuantitativeMethod.iTRAQ_4plex:
                    if (_ms2quant.ReporterIonMZ.Count != 4)
                    {
                        _ms2quant.ReporterIonMZ.Clear();
                        _ms2quant.ReporterIonMZ.Add(114.10);
                        _ms2quant.ReporterIonMZ.Add(115.10);
                        _ms2quant.ReporterIonMZ.Add(116.10);
                        _ms2quant.ReporterIonMZ.Add(117.10);
                    }
                    this.reportIonMZ.ItemsSource = _ms2quant.ReporterIonMZ;
                    this.V_reportIons_panel.Visibility = Visibility.Visible;
                    this.V_pIDL_panel.Visibility = Visibility.Collapsed;
                    break;
                case (int)MS2QuantitativeMethod.iTRAQ_8plex:
                    if (_ms2quant.ReporterIonMZ.Count != 8)
                    {
                        _ms2quant.ReporterIonMZ.Clear();
                        _ms2quant.ReporterIonMZ.Add(113.10);
                        _ms2quant.ReporterIonMZ.Add(114.10);
                        _ms2quant.ReporterIonMZ.Add(115.10);
                        _ms2quant.ReporterIonMZ.Add(116.10);
                        _ms2quant.ReporterIonMZ.Add(117.10);
                        _ms2quant.ReporterIonMZ.Add(118.10);
                        _ms2quant.ReporterIonMZ.Add(119.10);
                        _ms2quant.ReporterIonMZ.Add(121.10);
                    }
                    this.reportIonMZ.ItemsSource = _ms2quant.ReporterIonMZ;
                    this.V_reportIons_panel.Visibility = Visibility.Visible;
                    this.V_pIDL_panel.Visibility = Visibility.Collapsed;
                    break;
                case (int)MS2QuantitativeMethod.TMT_6plex:
                    if (_ms2quant.ReporterIonMZ.Count != 6)
                    {
                        _ms2quant.ReporterIonMZ.Clear();
                        _ms2quant.ReporterIonMZ.Add(126.127726);
                        _ms2quant.ReporterIonMZ.Add(127.131080);
                        _ms2quant.ReporterIonMZ.Add(128.134435);
                        _ms2quant.ReporterIonMZ.Add(129.137790);
                        _ms2quant.ReporterIonMZ.Add(130.141145);
                        _ms2quant.ReporterIonMZ.Add(131.138180);
                    }
                    this.reportIonMZ.ItemsSource = _ms2quant.ReporterIonMZ;
                    this.V_reportIons_panel.Visibility = Visibility.Visible;
                    this.V_pIDL_panel.Visibility = Visibility.Collapsed;
                    break;
                case (int)MS2QuantitativeMethod.TMT_10plex:
                    if (_ms2quant.ReporterIonMZ.Count != 10)
                    {
                        _ms2quant.ReporterIonMZ.Clear();
                        _ms2quant.ReporterIonMZ.Add(124.110);
                        _ms2quant.ReporterIonMZ.Add(125.110);
                        _ms2quant.ReporterIonMZ.Add(126.127726);
                        _ms2quant.ReporterIonMZ.Add(127.131080);
                        _ms2quant.ReporterIonMZ.Add(128.134435);
                        _ms2quant.ReporterIonMZ.Add(129.137790);
                        _ms2quant.ReporterIonMZ.Add(130.141145);
                        _ms2quant.ReporterIonMZ.Add(131.138180);
                        _ms2quant.ReporterIonMZ.Add(132.110);
                        _ms2quant.ReporterIonMZ.Add(133.110);
                    }
                    this.reportIonMZ.ItemsSource = _ms2quant.ReporterIonMZ;
                    this.V_reportIons_panel.Visibility = Visibility.Visible;
                    this.V_pIDL_panel.Visibility = Visibility.Collapsed;
                    break;
                case 4:
                    if (_ms2quant.PIDL.Count == 0)
                    {
                        _ms2quant.PIDL.Add(new pIDLplex("Dim34NL", 34.063119, "Dim34KL", 34.063119));
                        _ms2quant.PIDL.Add(new pIDLplex("Dim34NH", 34.068963, "Dim34KH", 34.068963));
                    }
                    this.V_pIDL_Info.ItemsSource = _ms2quant.PIDL;
                    this.V_reportIons_panel.Visibility = Visibility.Collapsed;
                    this.V_pIDL_panel.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void enable_ms2quant(object sender, RoutedEventArgs e)
        {
            this.ms2_quant_method_panel.Visibility = Visibility.Visible;
            this.V_ms2param.Visibility = Visibility.Visible;
        }

        private void disable_ms2quant(object sender, RoutedEventArgs e)
        {
            this.ms2_quant_method_panel.Visibility = Visibility.Collapsed;
            this.V_ms2param.Visibility = Visibility.Collapsed;
        }

        private void ms2quant_Expanded(object sender, RoutedEventArgs e)
        {

        }

        private void Add_pIDL_Item_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ms2quant.PIDL.Add(new pIDLplex(this.v_nmod_textbox.Text, double.Parse(this.v_nmass_textbox.Text), this.v_cmod_textbox.Text, double.Parse(this.v_cmass_textbox.Text)));
                this.V_pIDL_Info.ItemsSource = _ms2quant.PIDL;
            }
            catch (Exception exe)
            {
                MessageBox.Show(exe.Message);
            }
        }

        private void Delete_pIDL_Item_Click(object sender, RoutedEventArgs e)
        {
            while (this.V_pIDL_Info.SelectedItems.Count > 0)
            {
                pIDLplex plex = this.V_pIDL_Info.SelectedItems[0] as pIDLplex;
                this._ms2quant.PIDL.Remove(plex);
            }
            this.V_pIDL_Info.SelectedIndex = -1;
        }

        private void Clear_pIDL_Item_Click(object sender, RoutedEventArgs e)
        {
            this._ms2quant.PIDL.Clear();
            this.V_pIDL_Info.ItemsSource = this._ms2quant.PIDL;
        }


        
    }
}
