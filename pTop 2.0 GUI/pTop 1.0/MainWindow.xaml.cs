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
using System.Windows.Navigation;
using System.Windows.Shapes;
using pTop.classes;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.IO;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Text.RegularExpressions;
using System.IO.Packaging;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace pTop
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum TabOption : int
        {
            MS_Data = 0,
            Identification = 1,           
            Summary = 2
        }

        //StartPage界面
        public StartPage startPage;

        //用来显示动画效果
        public DispatcherTimer check_time_timer = null; //每隔12个小时运行一次，检测pTop使用是否超过使用日期限制
        public DispatcherTimer welcome_timer = null;
        Process welcome_process = null;

        _Task _task = new _Task();
        public _Task _Task
        {
            get { return _task; }
            set { _task = value; }
        }

        pTop.classes.File _file = new pTop.classes.File();
        public pTop.classes.File _File
        {
            get { return _file; }
            set { _file = value; }
        }

        Identification _search = new Identification();
        public Identification _Search
        {
            get { return _search; }
            set { _search = value; }
        }

        Quantitation _quantitation = new Quantitation();
        public Quantitation _Quantitation
        {
            get { return _quantitation; }
            set { _quantitation = value; }
        }

        public Advanced settings = new Advanced();

        string output_path = "";   //default output path
        string defaultfilePath = "";
        bool firstload = true;

        private BackgroundWorker bgworker1;
        private delegate void SimpleDelegate();

        ObservableCollection<String> display_mods = new ObservableCollection<String>(); //修饰列表中的修饰 
        ObservableCollection<LabelInfo> display_labels = new ObservableCollection<LabelInfo>(); //标记列表中选过后的标记

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

        private bool time_start() //运行pTop的时候进行时间限制的检查
        {
            Time_Help time_help = new Time_Help();
            if (!time_help.file_exist())
            {
                MessageBox.Show(Message_Help.DATE_FILE_NOT_EXIST);
                return false;
            }
            if (!time_help.file_dll_time_isEqual())
            {
                MessageBox.Show(Message_Help.DATE_FILE_INVALID);
                return false;
            }
            check_time_timer = new DispatcherTimer();
            check_time_timer.Interval = TimeSpan.FromSeconds(12 * 60 * 60); //12个小时，运行一次检测日期是否超过使用限制
            check_time_timer.Tick += check_time;
            check_time_timer.Start();

            bool flag = time_help.is_OK();
            if (flag)
                time_help.update_time(DateTime.Now, time_help.time_last_path);
            else
            {
                MessageBox.Show(Message_Help.DATE_OVER);
                //MessageBox.Show(Message_Help.EXPIRY_DATE);
                Application.Current.Shutdown();
                return false;
            }
            return true;
        }

        public MainWindow()
        {
            if (!time_start()) // 检查有效期
            {
                Application.Current.Shutdown();
                this.Close();
                return;
            }

            startPage = new StartPage();
            bool flag = startPage.check_license();  // 检查license
            if (!flag)
            {
                this.Close();
                return;
            }
            ///startPage.add_CrashHandler();
            //崩溃测试
            //int a = 1, b = 0, c;
            //c = a / b;
            //wrm20160220: 检查.Net4.5 + 是否正确安装， 之前是在JustRun时才检查
            if (!ConfigHelper.CheckFrameworkDotNet())
            {
                MessageBox.Show(Message_Help.DOT_NET_ERROR, "pTop", MessageBoxButton.OK, MessageBoxImage.Stop);
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

                welcome_process = Process.Start("Welcome.exe", "pTop");
            }
            else
            {
                InitializeComponent();
                this.Loaded += new RoutedEventHandler(window_loaded);
            }     
        }

        private void initial_window(object sender, EventArgs e)
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
            this.Loaded += new RoutedEventHandler(window_loaded);
        }

        private void window_loaded(object sender, RoutedEventArgs e)
        {
            settingWorkspace();
            InitializeContent();
       
            if (Application.Current.Properties["ArbitraryArgName"] == null)
            {
                this.new_task();
            }
            else
            {
                string task_path = Application.Current.Properties["ArbitraryArgName"] as string;
                Application.Current.Properties["ArbitraryArgName"] = null;
                open_task_by_path(task_path);
            }
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

        void InitializeContent()
        {
            ConfigHelper.get_mods();
            show_DB();
            //初始化修饰列表
            //show_com_mods();
            //InitialModsList();

            ConfigHelper.get_labels();
            setDataSource();

            if (defaultfilePath == "")
            {
                defaultfilePath = _get_output_path();
            }
        }

        public void setDataSource()
        {
            this.FileTab.DataContext = _file;
            this.IdentificationTab.DataContext = _search;
            this.V_MS1Quant.DataContext = _quantitation;
        }

        void show_DB()
        {
            int dbIndex =  _task.T_Identify.Db_index;
            ConfigHelper.getDB();
            this.Database.ItemsSource = ConfigHelper.dblist;
            if (ConfigHelper.dblist.Count > 1)
            {
                if (_search.Db_index == -1)
                {
                    int num = ConfigHelper.db_add.Count;
                    if (num > 0)
                    {
                        this.Database.SelectedIndex = ConfigHelper.dblist.IndexOf(ConfigHelper.db_add[num - 1]);
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

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            if (bgworker1 != null && bgworker1.IsBusy)  //there is a task running
            {
                bgworker1.CancelAsync();
            }
            Application.Current.Shutdown();
        }

        private void StartSearch(object sender, RoutedEventArgs e)
        {
            try
            {
                if (bgworker1 != null && bgworker1.IsBusy)  //there is a task running
                {
                    System.Windows.MessageBox.Show("The task is running!", "pTop warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (CheckCurrentBeforeStart())
                {
                    
                    //there is no task running:(1)work done;(2)no work 

                    bgworker1 = new BackgroundWorker();
                    bgworker1.WorkerSupportsCancellation = true;
                    bgworker1.DoWork += new DoWorkEventHandler(DoSearchWork);
                    bgworker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SearchWorkDone);
                    bgworker1.RunWorkerAsync();
                }
                else
                {
                    // The configuration of " + _task.Task_name + " is not completed!
                    System.Windows.MessageBox.Show("The configuration is not completed!", "pTop warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    this.tab.SelectedIndex = 3;
                }
            }
            catch(Exception exe)
            {
                MessageBox.Show("Running Error: " + exe.Message);
            }
        }

        /// <summary>
        /// 运行前检查并保存参数
        /// </summary>
        private bool CheckCurrentBeforeStart()
        {
            try
            {
                // 运行前无条件保存参数设置，并写入参数文件
                save_task(true);

                bool flag = Factory.Create_Run_Instance().SaveParams(_task);
                //Directory.CreateDirectory(_task.Path + "\\result");

                //_task.Check_ok = Factory.Create_Run_Instance().Check_Task(current_task);                
            }
            catch (Exception exe)
            {
                _task.Check_ok = false;
                throw new Exception("[Parameter] " + exe.Message);
            }
            return _task.Check_ok;
        }

        private void DoSearchWork(object sender, DoWorkEventArgs e)
        {
            StartOneWork();
            if (bgworker1.CancellationPending)
            {
                e.Cancel = true;
                return;
            }            
        }

        private void StartOneWork()
        {
            #region TODO
            /* 禁掉该任务的右键菜单 */
            /* 禁掉部分按钮 */

            //SimpleDelegate delt1 = delegate()
            //{
            //    DisableRightMenu(taskindex);
            //    DisableButtons();
            //};
            //this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, delt1);    
            #endregion
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.Output.Clear();
                    DisableButtons();
                });
                Process process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                if (bgworker1.CancellationPending)
                {
                    KillProcessAndChildren(process.Id);
                    return;
                }   
                process.StartInfo.FileName = "pTop.exe";     //"Searcher.exe"
                process.StartInfo.Arguments = "\"" + _task.Path + "param\\pTop.cfg\"";
                if (process.Start())
                {
                    while (!process.HasExited)
                    {
                        if (bgworker1.CancellationPending)
                        {
                            KillProcessAndChildren(process.Id);
                            return;
                        }
                        //process.OutputDataReceived += new DataReceivedEventHandler(p_DataReceived);
                        //process.BeginOutputReadLine();
                        string line = process.StandardOutput.ReadLine();
                        ReportProgress(line);
                        //string errline = process.StandardError.ReadLine();   //错误流输出
                        //ReportProgress(taskdata, errline);
                    }
                    process.WaitForExit();
                }
                process_exit(process);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                
            }
        }
        
        // 显示内核输出
        string alreadyoutput = "";
        private void ReportProgress(string line)
        {
            SimpleDelegate del = delegate()
            {
                string pattern = @"<.*>.*:.*\d+%";
                Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
                if (line != null && r.IsMatch(line))
                {
                    this.Output.Text = alreadyoutput + line + "\n";
                    this.Output.Select(this.Output.Text.Length, 0);
                    int index1 = line.LastIndexOf("<");
                    string status = line.Substring(index1 + 1, line.LastIndexOf(">") - index1 - 1);
                    string progress = line.Substring(line.LastIndexOf(":") + 1).Trim();
                    this.status1.Text = status + " : " + progress;
                }
                else
                {
                    this.Output.AppendText(line + "\n");
                    this.Output.ScrollToEnd();      //滚动置底
                    alreadyoutput = this.Output.Text;
                    this.status1.Text = "Running";
                }
                this.Output.ScrollToEnd();
                
            };
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, del);
        }

        private void process_exit(Process process)
        {
            if (process.ExitCode != 0)
            {
                _stop_task();
            }
        }

        private void _stop_task()
        {
            if (bgworker1 != null && bgworker1.IsBusy)
            {
                bgworker1.CancelAsync();
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
                if (!proc.HasExited)
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

        private void SearchWorkDone(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                #region Todo
                //after cancel,run next 
                #endregion
                
                System.Windows.MessageBox.Show("The work has been stopped!");
                
                this.status1.Text = "Stopped!";
            }
            else if (e.Error != null)
            {
                System.Windows.MessageBox.Show(e.Error.Message, "An error occured!");
            }
            else
            {
                System.Windows.MessageBox.Show("Work done!");
                this.status1.Text = "Done!";
            }
            EnableButtons();
           
            return;
        }

        

        private void StopTask(object sender, RoutedEventArgs e)
        {
            _stop_task();
        }

        private void clk(object sender, SelectionChangedEventArgs e)
        {
                
                if (e.OriginalSource == tab)
                {

                    if (tab.SelectedIndex == 0) //File
                    {

                    }
                    if (tab.SelectedIndex == 1) //Identification
                    {

                    }
                    if (tab.SelectedIndex == 2)
                    {

                    }
                    else if (tab.SelectedIndex == 3) //Quantitation
                    {
                        show_summary();
                    }

                }
            
        }

        private void show_summary()
        {
            try
            {
                saveDisplayParam();

                this.fileReport.ItemsSource = _file.getFileParam();

                this.searchReport.ItemsSource = _search.getSearchParam();

                if (_quantitation.Quantitation_type != (int)Quant_Type.Label_None)
                {
                    this.quantReport.ItemsSource = _quantitation.getQuantitationParam();
                    this.QRScroll.Visibility = Visibility.Visible;
                }
                else
                {
                    this.QRScroll.Visibility = Visibility.Collapsed;
                }

                ReportViewWidthChange();     //改变ListView列的宽度
            }
            catch(Exception exe)
            {
                throw new Exception("[Summary] "+exe.Message); 
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
                    new Typeface(this.fileReport.FontFamily.ToString()),
                    this.fileReport.FontSize, Brushes.Black);
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
                    new Typeface(this.searchReport.FontFamily.ToString()),
                    this.searchReport.FontSize, Brushes.Black);
                if (ft.WidthIncludingTrailingWhitespace > len2)
                {
                    len2 = ft.WidthIncludingTrailingWhitespace;
                }
            }
            view2.Columns[1].Width = len2 + 30;
            #endregion
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

        private void setDFSizeText(double size)
        {
            double dfMul = 1.0;
            this.size_unit.Text = " GB";
            if (size < 1) // MB
            {
                dfMul *= 1024.001;
                this.size_unit.Text = " MB";
                if (size * 1024 < 1) // KB
                {
                    dfMul *= 1024.001;
                    this.size_unit.Text = " KB";
                }
            }
            this.dfsize.Text = Math.Round(size * dfMul, 3).ToString();
        }

        private void Add_DataFiles(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            #region Todo
            //文件后缀名过滤
            #endregion
            openFileDialog.Multiselect = true;
            int file_format = this.FormatChoices.SelectedIndex;
            if (file_format == (int)FormatOptions.MGF)
            {
                openFileDialog.Filter = "mgf files (*.mgf)|*.mgf";
            }
            else if (file_format == (int)FormatOptions.RAW)
            {
                openFileDialog.Filter = "raw files (*.raw)|*.raw";     //|all files (*.*)|*.*
            }
            string TMP = this.FormatChoices.SelectedValue.ToString();
            TMP = TMP.Substring(TMP.LastIndexOf(":") + 1).Trim();
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
                    else
                    {
                        System.Windows.MessageBox.Show("Only " + tmp + "/" + TMP + "files can be added.", "File Format", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                }

                this.fileList.ItemsSource = this._file.Data_file_list;
                this.dfnum.Text = this._file.Data_file_list.Count.ToString();   //DataFile.DFnum = this._file.Data_file_list.Count;
                setDFSizeText(getDFSize(this._file.Data_file_list)); //Math.Round(totalsize / 1024.0001, 3).ToString();        //DataFile.DFsize += Math.Round(totalsize / 1024.0001, 3);              
                this.status1.Text = this._file.Data_file_list.Count + " " + tmp + " files";
            }
        }

        private void Delete_DataFile(object sender, RoutedEventArgs e)
        {
            while (this.fileList.SelectedItems.Count > 0)
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

        private void Clear_DataFiles(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("Do you really want to clear all files?", "Warning",
                MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                this._file.Data_file_list.Clear();
            }
            this.dfnum.Text = "0";   //DataFile.DFnum = 0;
            setDFSizeText(0.0);  //DataFile.DFsize = 0.0;
            this.status1.Text = "data file num: 0";
        }
        // 切换Tab之前保存当前页面的配置？
        private void BeforeTabChange(object sender, MouseButtonEventArgs e)
        {
            try
            {
                    int index = this.tab.SelectedIndex;
                    switch (index)
                    {
                        case 0: file_save();
                            break;
                        case 1: identification_save();
                            break;
                    }

            }
            catch (Exception exe)
            {
                System.Windows.MessageBox.Show(exe.Message);
                e.Handled = true;
                return;
            }
        }

        private void pParseConfig_Expanded(object sender, RoutedEventArgs e)
        {

        }
        // 改变输入文件格式
        private void FormatChange(object sender, SelectionChangedEventArgs e)
        {
            //_file.Data_file_list.Clear();
            string tips = "Please note that you can only add \"";
            string tmp = this.FormatChoices.SelectedValue.ToString();
            tips += tmp.Substring(tmp.LastIndexOf(":") + 1);
            tips += " \"files";
            ToolTip tp = new ToolTip();
            //tp.Style = (Style)this.FindResource("SimpleTip");
            tp.Content = tips;
            this.addRawBtn.ToolTip = tp;
            int ft = this.FormatChoices.SelectedIndex;
            if (ft == (int)FormatOptions.MGF)
            {     //MGF
       
                this.pParseConfig.Visibility = Visibility.Collapsed;
                for (int i = _file.Data_file_list.Count - 1; i >= 0; i--)
                {
                    string ftype = _file.Data_file_list[i].Type.ToUpper();
                    if (ftype != "MGF")
                    {
                        _file.Data_file_list.RemoveAt(i);
                    }
                }

            }
            else if (ft == (int)FormatOptions.RAW)
            {
                this.pParseConfig.Visibility = Visibility.Visible;
                for (int i = _file.Data_file_list.Count - 1; i >= 0; i--)
                {
                    string ftype = _file.Data_file_list[i].Type.ToUpper();
                    if (ftype != "RAW")
                    {
                        _file.Data_file_list.RemoveAt(i);
                    }
                }
            }
        }

        private void SumScrollChanged(object sender, SizeChangedEventArgs e)
        {
            this.SumScroll.ScrollToEnd();
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

        private void export_pdf(object sender, RoutedEventArgs e)
        {
            var sender_obj = sender as Button;
            if (sender_obj != this.exp_pdf)
                return;
            if (Directory.Exists(_task.Path))
            {
            }
            else
            {
                Directory.CreateDirectory(_task.Path);
                Directory.CreateDirectory(_task.Path + "\\param");
                Directory.CreateDirectory(_task.Path + "\\result");
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
                string c_path = _task.Path + "param\\" + fileName;
                PdfSharp.Xps.XpsConverter.Convert(pdfXpsDoc, c_path, 0);
                System.Diagnostics.Process.Start(c_path);
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, "Failed to export PDF files");
                return;
            }
        }

        // 工作目录设置
        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            SettingsWindow dlg = new SettingsWindow(settings);
            bool dresult = (bool)dlg.ShowDialog();
            defaultfilePath = _get_output_path();
        }

        string _get_output_path()
        {
            StreamReader sr = new StreamReader(ConfigHelper.startup_path + "\\pTop.ini", Encoding.Default);
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

        private void saveDisplayParam()
        {
            try
            {
                //string taskName = DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss");
                if (_task.Path == "" || _task.Path == null || _task.Path.Equals("unsaved"))
                {
                    //获取当前默认路径 
                    string outputpath = _get_output_path();
                    _task.Path = outputpath + "\\" + _task.Task_name + "\\";
                }
                _task.Check_ok = true;

                // 保存File 面板
                file_save();
                identification_save();
                quantitation_save();
            }
            catch(Exception exe)
            {
                throw new Exception(exe.Message);
            }
        }

        private void file_save()
        {
            try
            {
                _file.File_format_index = this.FormatChoices.SelectedIndex;
                _file.setFileFormat();
                _file.Instrument_index = this.InstrumentChoices.SelectedIndex;
                _file.setInstrument();
                _file.Pparse_advanced.Mix_spectra = this.MixSpecCB.IsChecked.Value;
                _file.Pparse_advanced.Mz_decimal_index = this.mzDecimal.SelectedIndex;
                _file.Pparse_advanced.setMZDecimal();
                _file.Pparse_advanced.Intensity_decimal_index = this.IntensityDecimal.SelectedIndex;
                _file.Pparse_advanced.setIntensityDecimal();
                //_file.Pparse_advanced.Model = this.model.SelectedIndex;
                _file.Pparse_advanced.Isolation_width = double.Parse(this.isolation_width.Text.ToString());
                //_file.Pparse_advanced.Threshold = double.Parse(this.threshhold.Text.ToString());
                _file.Pparse_advanced.Max_charge = int.Parse(this.Max_charge.Text.ToString());
                _file.Pparse_advanced.Max_mass = double.Parse(this.Max_mass.Text.ToString());
                _file.Pparse_advanced.Mz_tolerance = double.Parse(this.Mz_tolerance.Text.ToString());
                _file.Pparse_advanced.Sn_ratio = double.Parse(this.SN_ratio.Text.ToString());
                if (_file.Data_file_list == null || _file.Data_file_list.Count == 0 || _file.Pparse_advanced.Threshold.ToString() == "")
                {
                    _task.Check_ok = false;
                    //throw new ArgumentException(Message_Help.ALL_NOT_NULL);
                }
                
            }
            catch (Exception exe)
            {
                _task.Check_ok = false;
                //System.Windows.MessageBox.Show(exe.Message +"\n" + Message_Help.INVALID_INPUT);
                throw new Exception("[MS Data] " + exe.Message + "\n" + Message_Help.INVALID_INPUT);
            }
        }

        private void identification_save()
        {
            try
            {
                #region search save and check
                _search.Db_index = this.Database.SelectedIndex;
                _search.setDatabase();    //database   
                if (this.Database.SelectedIndex == -1)
                {
                    _task.Check_ok = false;
                    // throw new ArgumentException(Message_Help.DataBase_EMPTY);
                }
                _search.Max_mod = int.Parse(this.maxmod.Text.ToString());  
                Tolerance tmp_ptl = new Tolerance();                               //Precursor Tolerance
                #region Todo
                //母离子和碎片离子检查
                #endregion
                if (this.PreTol.Text.ToString() == "" || this.FraTol.Text.ToString() == "")
                {
                    _task.Check_ok = false;
                    throw new ArgumentException(Message_Help.ALL_NOT_NULL);
                }
                tmp_ptl.Tl_value = double.Parse(this.PreTol.Text.ToString());

                tmp_ptl.Isppm = this.ptolUnits.SelectedIndex;   //1为ppm
                _search.Ptl = tmp_ptl;
                Tolerance tmp_ftl = new Tolerance();                               //Fragment Tolerance
                tmp_ftl.Tl_value = double.Parse(this.FraTol.Text.ToString());
                tmp_ftl.Isppm = this.ftolUnits.SelectedIndex;
                if (tmp_ptl.Tl_value < 0 || tmp_ftl.Tl_value < 0)
                {
                    _task.Check_ok = false;
                }
                _search.Ftl = tmp_ftl;
                #endregion
                //filter
                                            //FDR
                _search.Filter.Fdr_value = double.Parse(this.FDRValue.Text.ToString());
                
                if (this.FDRValue.Text == "")
                {
                    _task.Check_ok = false;
                    throw new ArgumentException(Message_Help.ALL_NOT_NULL);
                }
            }
            catch (Exception exe)
            {
                _task.Check_ok = false;
                throw new Exception("[Identification] " +exe.Message + "\n" + Message_Help.INVALID_INPUT);
                
            }
        }

        void quantitation_save()
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
                    _task.Check_ok = false;
                    throw new ArgumentException(Message_Help.INVALID_INPUT);
                }
                _quantitation.Quant_advanced.Number_hole_in_cmtg = this.number_hole_in_cmtg.SelectedIndex;
                _quantitation.Quant_advanced.Type_same_start_end_between_evidence = this.type_same_start_end_between_evidence.SelectedIndex;
                _quantitation.Quant_advanced.Ll_element_enrichment_calibration = this.ll_element_enrichment_calibration.SelectedIndex;
            }
            catch (Exception exe)
            {
                _task.Check_ok = false;
                throw new Exception("[Quantitation] " + exe.Message + "\n" + Message_Help.INVALID_INPUT);
            }
        }

        private void SaveTask(object sender, RoutedEventArgs e)
        {
            try
            {
                // 如果任务正在运行，则不允许更改配置
                if (bgworker1 != null && bgworker1.IsBusy)
                {
                    System.Windows.MessageBox.Show("The task is already started and the configuration cannot be modified.");
                    return;
                }

                save_task(false);
                
                bool flag = Factory.Create_Run_Instance().SaveParams(_task);

                if (flag)
                {
                    //create tooltip
                    string save_tip = "Save work done!";
                    TabControl tvi = this.tab;
                    display_temporary_tooltip(tvi, PlacementMode.Center, save_tip);
                }
            }
            catch(Exception exe)
            {
                MessageBox.Show("Save Task Error: "+exe.Message);
            }
            
        }

        private void save_task(bool willRun)
        {
            try
            {
                saveDisplayParam();

                if (willRun)
                {
                    string task_name = getDefaultTaskName(), task_dir = defaultfilePath;
                    if (_task.Task_name != "")
                    {
                        task_name = _task.Task_name;
                    }
                    if (_task.Path != "" && _task.Path != "unsaved")
                    {
                        task_dir = _task.Path;
                        if (task_dir[task_dir.Length - 1] == '\\')
                        {
                            task_dir = task_dir.Substring(0, task_dir.Length - 1);
                        }
                        task_dir = task_dir.Substring(0,task_dir.LastIndexOf("\\")) + "\\";
                    }
                    SaveTask save_task_window = new SaveTask(task_name, task_dir);
                    save_task_window.ChangeTextEvent += new SaveTask.ChangeNewTaskHandler(ReceiveValues);

                    if (save_task_window.ShowDialog() != true)
                    {
                        return;
                    }
                }
                else
                {
                    if (_task.Task_name == "" || _task.Path == "" || _task.Path == null || _task.Path.Equals("unsaved"))
                    {
                        NewTask new_task_window = new NewTask(getDefaultTaskName(), defaultfilePath);
                        new_task_window.ChangeTextEvent += new NewTask.ChangeNewTaskHandler(ReceiveValues);
                        if(new_task_window.ShowDialog() != true)
                        {
                            return;
                        }
                    }
                }
                Factory.Create_Copy_Instance().FileCopy(_file, _task.T_File);
                //Factory.Create_Copy_Instance().SearchParamCopy(_search, _task.T_Identify);
                _task.T_Identify = Factory.Create_Copy_Instance().DeepCopyWithXmlSerializer(_search);
                _task.T_Quantify = Factory.Create_Copy_Instance().DeepCopyWithXmlSerializer(_quantitation);
                this.Title = "pTop - " + _task.Task_name + "(" + _task.Path + ")";
            }
            catch (Exception exe)
            {
                MessageBox.Show("Save Task Error: " + exe.Message);
            }

        }

        void display_temporary_tooltip(UIElement ui, PlacementMode mode, string info)
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
            if (info.Equals("Save work done!")) { 
               tip.FontSize = 20;
            }
            DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(2) };
            timer.Tick += delegate(object o, EventArgs arg)
            {
                ((DispatcherTimer)timer).Stop();
                tip.IsOpen = false;
            };
            timer.Start();
        }
        // 重命名任务
        private void RenameTask(object sender, RoutedEventArgs e)
        {
            Rename dlg = new Rename(ref _task);
            bool dresult = (bool)dlg.ShowDialog();
            this.Title = "pTop - " + _task.Task_name + "(" + _task.Path + ")";
        }

        // 更改了数据库
        private void DatabaseChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Database.Items.Count > 0 && this.Database.SelectedIndex + 1 == this.Database.Items.Count)
            {
                Call_pConfig(0);
            }
        }

        // 配置 Modification
        private void Config_Mods(object sender, RoutedEventArgs e)
        {
            Call_pConfig(1);
        }
        // 添加固定修饰
        private void add_fix_mods(object sender, RoutedEventArgs e)
        {
            display_mods = (ObservableCollection<String>)this.Modifications.ItemsSource;
            //MessageBox.Show(Modifications.SelectedItems.Count.ToString());
            ObservableCollection<String> tmp = new ObservableCollection<String>();
            for (int i = 0; i < this.Modifications.SelectedItems.Count; i++)
            {
                String fm = this.Modifications.SelectedItems[i].ToString();
                tmp.Add(fm);
            }
            for (int i = 0; i < tmp.Count; i++)
            {
                _search.Fix_mods.Add(tmp[i]);
                display_mods.Remove(tmp[i]);
            }
            //selectedFixMod.Items.Clear();
            selectedFixMod.ItemsSource = _search.Fix_mods;
            //display_mods.OrderBy(p=>p);
            Modifications.ItemsSource = display_mods;
        }
        // 删除固定修饰
        private void remove_fix_mods(object sender, RoutedEventArgs e)
        {
            display_mods = (ObservableCollection<String>)Modifications.ItemsSource;
            ObservableCollection<String> tmp = new ObservableCollection<String>();
            for (int i = 0; i < selectedFixMod.SelectedItems.Count; i++)
            {
                String fm = selectedFixMod.SelectedItems[i].ToString();
                tmp.Add(fm);
            }
            for (int i = 0; i < tmp.Count; i++)
            {
                _search.Fix_mods.Remove(tmp[i]);
                display_mods.Add(tmp[i]);
            }
            selectedFixMod.ItemsSource = _search.Fix_mods;
            display_mods = new ObservableCollection<String>(display_mods.OrderBy(p => p));   //升序排列
            Modifications.ItemsSource = display_mods;
        }
        // 添加可变修饰
        private void add_var_mods(object sender, RoutedEventArgs e)
        {
            display_mods = (ObservableCollection<String>)Modifications.ItemsSource;
            ObservableCollection<String> tmp = new ObservableCollection<String>();
            for (int i = 0; i < Modifications.SelectedItems.Count; i++)
            {
                String fm = Modifications.SelectedItems[i].ToString();
                tmp.Add(fm);
            }
            for (int i = 0; i < tmp.Count; i++)
            {
                _search.Var_mods.Add(tmp[i]);
                display_mods.Remove(tmp[i]);
            }
            //selectedFixMod.Items.Clear();
            selectedVarMod.ItemsSource = _search.Var_mods;
            //display_mods.OrderBy(p=>p);
            Modifications.ItemsSource = display_mods;
        }
        // 移除可变修饰
        private void remove_var_mods(object sender, RoutedEventArgs e)
        {
            display_mods = (ObservableCollection<String>)Modifications.ItemsSource;
            ObservableCollection<String> tmp = new ObservableCollection<String>();
            for (int i = 0; i < selectedVarMod.SelectedItems.Count; i++)
            {
                String fm = selectedVarMod.SelectedItems[i].ToString();
                tmp.Add(fm);
            }
            for (int i = 0; i < tmp.Count; i++)
            {
                _search.Var_mods.Remove(tmp[i]);
                display_mods.Add(tmp[i]);
            }
            selectedVarMod.ItemsSource = _search.Var_mods;
            display_mods = new ObservableCollection<String>(display_mods.OrderBy(p => p));     //升序排列
            Modifications.ItemsSource = display_mods;
        }

        private void select_mods(object sender, MouseButtonEventArgs e)
        {

        }

        void InitialModsList()
        {
            for (int i = 0; i < _search.Var_mods.Count; i++)
            {
                display_mods.Remove(_search.Var_mods[i]);
            }
            for (int i = 0; i < _search.Fix_mods.Count; i++)
            {
                display_mods.Remove(_search.Fix_mods[i]);
            }
        }

        private void show_all_mods(object sender, RoutedEventArgs e)
        {
            display_mods.Clear();
            for (int i = 0; i < ConfigHelper.all_mods.Count; i++)
            {
                display_mods.Add(ConfigHelper.all_mods[i]);
            }
            display_mods = new ObservableCollection<String>(display_mods.OrderBy(p => p));     //升序排列
            this.Modifications.ItemsSource = display_mods;      //绑定修饰列表
            InitialModsList();
        }

        /** 显示常用修饰 **/
        private void show_com_mods()
        {
            display_mods.Clear();
            for (int i = 0; i < ConfigHelper.com_mods.Count; i++)
            {
                display_mods.Add(ConfigHelper.com_mods[i]);
            }
            display_mods = new ObservableCollection<String>(display_mods.OrderBy(p => p));     //升序排列
            this.Modifications.ItemsSource = display_mods;      //绑定修饰列表
        }

        private void show_com_mods(Object sender, RoutedEventArgs e)
        {
            show_com_mods();
            InitialModsList();
        }

        private void DoubleClickMSData(object sender, MouseButtonEventArgs e)
        {
            this.tab.SelectedIndex = 0;
        }

        private void DoubleClickSearch(object sender, MouseButtonEventArgs e)
        {
            this.tab.SelectedIndex = 1;
        }

        private void OpenConfig(object sender, RoutedEventArgs e)
        {        
            Call_pConfig(-1);
        }

        private void Call_pConfig(int tabindex)
        {
            try
            {
                bool display_all = (bool)this.disAllMod.IsChecked;
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

                switch (tabindex)
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
                    default: show_DB();
                             ConfigHelper.get_mods();
                             break;      
                }
                
                InitialListInfo();
                this.disAllMod.IsChecked = display_all;
            }
            catch (Exception exe)
            {
                System.Windows.MessageBox.Show("Sorry, something is wrong with pConfig.exe!\n" + exe.Message);
            }
        }

        //private void ModelChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (this.model.SelectedIndex == (int)ModelOptions.MARS)
        //    {
        //        this.mars_thresh.Visibility = Visibility.Visible;
        //    }
        //    else 
        //    {
        //        this.mars_thresh.Visibility = Visibility.Collapsed;
        //    }
        //}

        // 打开已存在的任务
        private void Open_Command(object sender, ExecutedRoutedEventArgs e)
        {
            this.Open_Task(sender,e);
        }

        private void Open_Task(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (!firstLoad && taskChanged())
                //{
                //    System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Do you want to save the change before open another task?", "pFind", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                //    if (result == System.Windows.MessageBoxResult.Yes)
                //    {
                //        saveDisplayParam();
                //        _task.T_File = _file;
                //        _task.T_Identify = _search;
                //        Factory.Create_Run_Instance().SaveParams(_task);
                //    }
                //    else if (result == System.Windows.MessageBoxResult.Cancel)
                //    {
                //        return;
                //    }
                //}
                System.Windows.Forms.OpenFileDialog f_dialog = new System.Windows.Forms.OpenFileDialog();
                f_dialog.Filter = "pTop task files (*.tsk)|*.tsk";
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
                    {
                        defaultfilePath = defaultfilePath.Substring(0, tmpIdx);
                    }
                }

                string task_path = defaultfilePath;
                int tmpIdx2 = defaultfilePath.LastIndexOf('\\');
                if (tmpIdx2 != -1)
                {
                    defaultfilePath = defaultfilePath.Substring(0, tmpIdx2);
                }
                open_task_by_path(task_path);
            }
            catch(Exception exe)
            {
                MessageBox.Show("Open Task Error: " + exe.Message);
                Console.WriteLine("Open Task Error: " + exe.Message);
            }
        }

        private void open_task_by_path(string task_path)
        {
            int p = task_path.Length - 1;
            while (task_path[p] == '\\')
            {
                p--;
            }
            string task_name = task_path.Substring(task_path.LastIndexOf('\\') + 1, p - task_path.LastIndexOf('\\'));
            if (System.IO.File.Exists(task_path + "\\" + task_name + ".tsk") && Directory.Exists(task_path + "\\param") && System.IO.File.Exists(task_path + "\\param\\pTop.cfg"))
            {
                load_task_by_path(task_path, false);
            }
            else
            {
                System.Windows.MessageBox.Show("Loading Error!\nSorry! This is not a valid folder for a pTop task.", "pTop", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }


        // 新建任务：加载模版任务，初始化配置信息
        private void New_Command(object sender, ExecutedRoutedEventArgs e)
        {
            this.new_task();
        }
        
        private void New_Task(object sender, RoutedEventArgs e)
        {
            this.new_task();
        }

        private bool taskChanged()
        {
            
            return false;
        }

        private void new_task()
        {
            try
            {
                //if (!firstLoad && taskChanged())
                //{
                //    System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Do you want to save the change before create another task?", "pTop", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                //    if (result == System.Windows.MessageBoxResult.Yes)
                //    {
                //        saveDisplayParam();
                //        _task.T_File = _file;
                //        _task.T_Identify = _search;
                //        Factory.Create_Run_Instance().SaveParams(_task);
                //    }
                //    else if (result == System.Windows.MessageBoxResult.Cancel)
                //    {
                //        return;
                //    }
                //}

                string path = ConfigHelper.startup_path + "\\template";
                if (Directory.Exists(path + "\\param") && System.IO.File.Exists(path + "\\param\\pTop.cfg"))
                {
                    load_task_by_path(path, true);
                }
                else
                {
                    System.Windows.MessageBox.Show(Message_Help.NEW_TASK_FAIL);

                }
            }
            catch (Exception exe)
            {
                MessageBox.Show("New Task Error: " + exe.Message);
            }
        }

        private string getDefaultTaskName()
        {
            string task_name = "pTopTask" + DateTime.Now.ToString("yyyyMMddHHmmss");
            return task_name;
        }

        private void ReceiveValues(string[] arr)
        {
            _task.Task_name = arr[0];
            _task.Path = arr[1];
        }

        private void load_task_by_path(string path, bool isnew)
        {
            _Task nts = new _Task();
            _Task tmp = _task;
            _task = nts;
            try
            {
                if (isnew)
                {
                    NewTask new_task_window = new NewTask(getDefaultTaskName(), defaultfilePath);
                    new_task_window.ChangeTextEvent += new NewTask.ChangeNewTaskHandler(ReceiveValues);
                    new_task_window.ShowDialog();
                    if (new_task_window.DialogResult != true)
                    {
                        _task = tmp;
                        return;
                    }
                }

                bool haspParse = false;
                this._file.Data_file_list.Clear(); ;
                this._search.Fix_mods.Clear();
                this._search.Var_mods.Clear();
                this._quantitation.Labeling.Light_label.Clear();
                this._quantitation.Labeling.Medium_label.Clear();
                this._quantitation.Labeling.Heavy_label.Clear();
      

                if (System.IO.File.Exists(path + "\\param\\pParseTD.cfg"))     //raw file -> pParse
                {
                    haspParse = true;
                    Factory.Create_pParse_Instance().pParse_read(path, ref _task);
                }
                
                Factory.Create_pTop_Instance().pTop_read(path, haspParse, ref _task);
                if (System.IO.File.Exists(path + "\\param\\pQuant.cfg"))
                {
                    Factory.Create_pQuant_Instance().readpQuant_qnt(path, ref _task);
                }
                Factory.Create_Copy_Instance().FileCopy(_task.T_File, _file);
                //Factory.Create_Copy_Instance().SearchParamCopy(_task.T_Identify, _search);
                _search = Factory.Create_Copy_Instance().DeepCopyWithXmlSerializer(_task.T_Identify);
                _quantitation = Factory.Create_Copy_Instance().DeepCopyWithXmlSerializer(_task.T_Quantify);

                // [wrm!] 由于对象地址改变，必须重新设置数据源，才能保证twoway binding 有效
                setDataSource();
                /*初始化列表信息：标记和修饰信息*/
                InitialListInfo();

                if (isnew)
                {
                    this.dfnum.Text = "0";  //DataFile.DFnum = 0;
                    setDFSizeText(0.0);
                    //this.dfsize.Text = "0.0";  // DataFile.DFsize = 0.0;
                    firstload = false;
                }
                else
                {
                    this.dfnum.Text = _file.Data_file_list.Count.ToString();
                    setDFSizeText(getDFSize(_file.Data_file_list));
                    this.Title = "pTop - " + _task.Task_name + "(" + _task.Path + ")";
                    firstload = true;
                }
                this.tab.SelectedIndex = 0;
              
            }
            catch (Exception exe)
            {
                System.Windows.MessageBox.Show(exe.Message);
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
                        display_temporary_tooltip(this.saveReportBtn as Button, PlacementMode.Right, info);
                        return;
                    }
                    string log_path = System.Windows.Forms.Application.StartupPath + "\\log\\";
                    if (!Directory.Exists(log_path))
                    {
                        Directory.CreateDirectory(log_path);
                    }
                    string date = DateTime.Now.Date.ToString("yyyy-MM-dd");
                    //System.Windows.MessageBox.Show(date);
                    string filepath = log_path + "running_log_" + date + ".log";

                    FileStream fs = new FileStream(filepath, FileMode.OpenOrCreate);
                    StreamReader sr = new StreamReader(fs, Encoding.Default);
                    StringBuilder sb = new StringBuilder(sr.ReadToEnd());
                    StreamWriter sw = new StreamWriter(fs, Encoding.Default);
                    sb.Append(this.Output.Text);
                    sw.Write(sb);
                    sw.Close();
                    sr.Close();
                    Process.Start("write.exe", "\"" + filepath + "\"");
                }
                else
                {
                    string info = "Sorry, there is no output content!";
                    display_temporary_tooltip(this.saveReportBtn as Button, PlacementMode.Right, info);
                }
            }
            catch (Exception exe)
            {
                System.Windows.MessageBox.Show(exe.Message);
            }
        }

        private void Run_Command(object sender, ExecutedRoutedEventArgs e)
        {
            this.StartSearch(sender,e);
        }

        private void Rename_Command(object sender, ExecutedRoutedEventArgs e)
        {
            this.RenameTask(sender, e);
        }

        private void Help_About_Clk(object sender, RoutedEventArgs e)
        {
            pTop.About about = new pTop.About();
            about.ShowDialog();
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
       
            //stop
            this.toolStop.IsEnabled = true;
            //MS Data, Indentification, Quantitation
            //this.FileTab.IsEnabled = false;
            //this.IdentificationTab.IsEnabled = false;

            //切换到summary
            //this.tab.SelectedIndex = 3;
            //if (this.FileTab.IsEnabled == false && this.IdentificationTab.IsEnabled == false)
            //{
            //    display_temporary_tooltip(this.SummaryTab, PlacementMode.Left, Message_Help.NO_EDIT_WHEN_RUN);
            //}
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
      
            //stop
            this.toolStop.IsEnabled = false;
            //MS Data, Indentification, Quantitation
            //this.FileTab.IsEnabled = true;
            //this.IdentificationTab.IsEnabled = true;
            
        }

        private void SearchModeChange(object sender, SelectionChangedEventArgs e)
        {
            int search_mode_index = this.SearchMode.SelectedIndex;
            if (search_mode_index == (int)SearchModeOptions.TAG_Index)
            {
                SecondSearchSwitch.Visibility = Visibility.Visible;
                if (!firstload)
                {
                    _search.Ptl.Tl_value = 5.2;
                }
            }
            else if (search_mode_index == (int)SearchModeOptions.Ion_Index)
            {
                SecondSearchSwitch.Visibility = Visibility.Collapsed;
                if (!firstload)
                {
                    _search.Ptl.Tl_value = 50000;
                }
            }
        }

        /*******************************定量模块的交互逻辑******************************************************/

        private void pQuant_Expanded(object sender, RoutedEventArgs e)
        {
            this.quantScroll.ScrollToBottom();
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
                string labelname = (string)ConfigHelper.ReLabelmap[_quantitation.Labeling.Light_label[0]];
                display_labels.Remove(new LabelInfo(labelname, _quantitation.Labeling.Light_label[0]));
            }
            if (_quantitation.Labeling.Medium_label.Count > 0)
            {
                string labelname = (string)ConfigHelper.ReLabelmap[_quantitation.Labeling.Medium_label[0]];
                display_labels.Remove(new LabelInfo(labelname,_quantitation.Labeling.Medium_label[0]));
            }
            if (_quantitation.Labeling.Heavy_label.Count > 0)
            {
                string labelname = (string)ConfigHelper.ReLabelmap[_quantitation.Labeling.Heavy_label[0]];
                display_labels.Remove(new LabelInfo(labelname,_quantitation.Labeling.Heavy_label[0]));
            }
        }
       
        private void select_quantitation_type(object sender, SelectionChangedEventArgs e)
        {
            this.ll_element_enrichment_calibration.SelectedIndex = (int)LL_ELEMENT_ENRICHMENT_CALIBRATION.None;
            if (this.QuanType.SelectedIndex == (int)Quant_Type.Label_None)
            {   //None
                this.QuantAdvanced.Visibility = Visibility.Collapsed;  //此处可以用触发器实现
                this.Quant_Label.Visibility = Visibility.Collapsed;
                firstload = false;
            }
            else if (this.QuanType.SelectedIndex == (int)Quant_Type.Labeling)
            {  //Label etc
                this.Quant_Label.Visibility = Visibility.Visible;
                this.QuantAdvanced.Visibility = Visibility.Visible;
                //this.ll_element_enrichment_calibration.SelectedIndex = (int)LL_ELEMENT_ENRICHMENT_CALIBRATION.None;

                this.MultiplicityType.IsEnabled = true;
                //this.multiplicity1.Visibility = Visibility.Collapsed;
                if (!firstload)
                {
                    this.MultiplicityType.SelectedIndex = 1;
                }
                this.lightBtn.IsEnabled = true;
                this.mediumBtn.IsEnabled = true;
                this.heavyBtn.IsEnabled = true;
            }
            
        }

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
                    _quantitation.Labeling.Medium_label.Add(ConfigHelper.labels[0].Label_element);
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
                    _quantitation.Labeling.Light_label.Add(ConfigHelper.labels[0].Label_element);
                    _quantitation.Labeling.Medium_label.Clear();
                    _quantitation.Labeling.Heavy_label.Clear();
                    display_labels.Remove(ConfigHelper.labels[0]);
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

        private void Config_Labels(object sender, RoutedEventArgs e)
        {
            Call_pConfig(2);
        }

        /**添加标记**/
        private void add_light_labels(object sender, RoutedEventArgs e)
        {
            display_labels = (ObservableCollection<LabelInfo>)this.labellist.ItemsSource;
            ObservableCollection<LabelInfo> tmp = new ObservableCollection<LabelInfo>();
            for (int i = 0; i < this.labellist.SelectedItems.Count; i++)
            {
                LabelInfo llb = (LabelInfo)this.labellist.SelectedItems[i];
                tmp.Add(llb);
            }
            for (int i = 0; i < tmp.Count; i++)
            {
                _quantitation.Labeling.Light_label.Add(tmp[i].Label_element);
                display_labels.Remove(tmp[i]);
            }
            this.labellist.ItemsSource = display_labels;
            this.selectedLightLabels.ItemsSource = _quantitation.Labeling.Light_label;

        }

        private void add_medium_labels(object sender, RoutedEventArgs e)
        {

            display_labels = (ObservableCollection<LabelInfo>)this.labellist.ItemsSource;
            ObservableCollection<LabelInfo> tmp = new ObservableCollection<LabelInfo>();
            for (int i = 0; i < this.labellist.SelectedItems.Count; i++)
            {
                LabelInfo llb = (LabelInfo)this.labellist.SelectedItems[i];
                tmp.Add(llb);
            }
            for (int i = 0; i < tmp.Count; i++)
            {

                _quantitation.Labeling.Medium_label.Add(tmp[i].Label_element);
                display_labels.Remove(tmp[i]);
            }
            this.labellist.ItemsSource = display_labels;
            this.selectedMediumLabels.ItemsSource = _quantitation.Labeling.Medium_label;
        }

        private void add_heavy_labels(object sender, RoutedEventArgs e)
        {

            display_labels = (ObservableCollection<LabelInfo>)this.labellist.ItemsSource;
            ObservableCollection<LabelInfo> tmp = new ObservableCollection<LabelInfo>();
            for (int i = 0; i < this.labellist.SelectedItems.Count; i++)
            {
                LabelInfo llb = (LabelInfo)this.labellist.SelectedItems[i];
                tmp.Add(llb);
            }
            for (int i = 0; i < tmp.Count; i++)
            {

                _quantitation.Labeling.Heavy_label.Add(tmp[i].Label_element);
                display_labels.Remove(tmp[i]);
            }
            this.labellist.ItemsSource = display_labels;
            this.selectedHeavyLabels.ItemsSource = _quantitation.Labeling.Heavy_label;
        }

        /**删除标记**/
        private void remove_light_labels(object sender, RoutedEventArgs e)
        {
            display_labels = (ObservableCollection<LabelInfo>)this.labellist.ItemsSource;
            ObservableCollection<string> tmp = new ObservableCollection<string>();
            for (int i = 0; i < this.selectedLightLabels.SelectedItems.Count; i++)
            {
                string llb = (string)this.selectedLightLabels.SelectedItems[i];
                tmp.Add(llb);
            }
            for (int i = 0; i < tmp.Count; i++)
            {
                _quantitation.Labeling.Light_label.Remove(tmp[i]);
                display_labels.Add(new LabelInfo((string)ConfigHelper.ReLabelmap[tmp[i]], tmp[i]));
            }
            this.selectedLightLabels.ItemsSource = _quantitation.Labeling.Light_label;
            //display_labels = new ObservableCollection<LabelInfo>(display_labels.OrderBy(p => p));   //升序排列
            this.labellist.ItemsSource = display_labels;
        }

        private void remove_medium_labels(object sender, RoutedEventArgs e)
        {
            display_labels = (ObservableCollection<LabelInfo>)this.labellist.ItemsSource;
            ObservableCollection<string> tmp = new ObservableCollection<string>();
            for (int i = 0; i < this.selectedMediumLabels.SelectedItems.Count; i++)
            {
                string llb = (string)this.selectedMediumLabels.SelectedItems[i];
                tmp.Add(llb);
            }
            for (int i = 0; i < tmp.Count; i++)
            {
                _quantitation.Labeling.Medium_label.Remove(tmp[i]);
                display_labels.Add(new LabelInfo((string)ConfigHelper.ReLabelmap[tmp[i]], tmp[i]));
            }
            this.selectedMediumLabels.ItemsSource = _quantitation.Labeling.Medium_label;
            //display_labels = new ObservableCollection<LabelInfo>(display_labels.OrderBy(p => p));   //升序排列
            this.labellist.ItemsSource = display_labels;
        }

        private void remove_heavy_labels(object sender, RoutedEventArgs e)
        {
            display_labels = (ObservableCollection<LabelInfo>)this.labellist.ItemsSource;
            ObservableCollection<string> tmp = new ObservableCollection<string>();
            for (int i = 0; i < this.selectedHeavyLabels.SelectedItems.Count; i++)
            {
                string llb = (string)this.selectedHeavyLabels.SelectedItems[i];
                tmp.Add(llb);
            }
            for (int i = 0; i < tmp.Count; i++)
            {
                _quantitation.Labeling.Heavy_label.Remove(tmp[i]);
                display_labels.Add(new LabelInfo((string)ConfigHelper.ReLabelmap[tmp[i]], tmp[i]));
            }
            this.selectedHeavyLabels.ItemsSource = _quantitation.Labeling.Heavy_label;
            //display_labels = new ObservableCollection<LabelInfo>(display_labels.OrderBy(p => p));   //升序排列
            this.labellist.ItemsSource = display_labels;
        }

        private void DoubleClickQuant(object sender, MouseButtonEventArgs e)
        {
            this.tab.SelectedIndex = 2;
        }

         
    }
}
