using _3DTools;
using BCDev.XamlToys;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Reporting;
using OxyPlot.Series;
using pBuild.pLink;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Petzold.Media3D;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Xml;

namespace pBuild
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private BackgroundWorker backgroundWorker, backgroundWorker_fdr, backgroundWorker_cand, backgroundWorker_filter;
        bool backgroudWorker_cand_flag = false;
        //该窗口目前只支持一个任务，想要哪个任务，就点击Load进行加载。
        public Task task = null;
        public string task_flag = "pTop";
        //Dialog对话框
        public PSM_Filter_Dialog psm_filter_dialog = null;
        public pLink.PSM_Filter_Dialog pLink_psm_filter_dialog = null;
        public MGF_PSM_Filter_Dialog mgf_psm_filter_dialog = null;
        public MS2_Quant_Config_Dialog ms2_quant_config_dialog = null;
        public MS2_Quant_Config_Dialog2 ms2_quant_config_dialog2 = null;
        public Ms2_advance ms2_advance_dialog = null;
        public Ms2_setting ms2_setting_dialog = null;
        public Display_ms2_table display_ms2_dialog = null;
        public MS1_Advance ms1_advance_dialog = null;
        public MS1_Setting ms1_setting_dialog = null;
        public MZ_Input_Dialog mz_input_dialog = null;
        public Protein_Filter_Dialog protein_filter_dialog = null;
        public Protein_Adanvaced_Dialog protein_adanvaced_dialog = null;
        //MGF文件
        ObservableCollection<Spectra> spectra = new ObservableCollection<Spectra>();
        Hashtable spectra_hash = new Hashtable();

        //Summary 面板
        //public Display_Mass_Error_Points dis_ms_points = new Display_Mass_Error_Points();
        public Summary_Result_Information summary_result_information;
        public Summary_Param_Information summary_param_information;
        public ObservableCollection<PSM> all_psms = new ObservableCollection<PSM>(); //读取的所有PSM
        public ObservableCollection<PSM> psms = new ObservableCollection<PSM>(); //通过过滤的PSM，比如FDR=1%，那么为q_value<=0.01的所有PSM
        public ObservableCollection<PSM> display_psms = new ObservableCollection<PSM>(); //在Spectrum列表中展示的PSM
        public ObservableCollection<PSM> selected_psms = new ObservableCollection<PSM>(); //在Spectrum列表中选择的PSM

        public pLink_Result pLink_result = new pLink_Result();
        Protein selected_protein = null;

        public PSM selected_psm; //在Spectrum列表中选择的某一个PSM，用来显示MS2，MS1和3D定量图
        Spectra_MS1 selected_ms1; //在Spectrum列表中选择的某一个PSM，可以显示对应的MS1
        //Protein面板
        public Protein_Panel protein_panel;

        //按扫描号指引到索引文件的位置。因为一个任务有多个RAW文件，那么索引文件也有多个，所以Hashtable也有多个。
        public string fragment_type = "HCDFT"; //该任务搜索时候所使用的碎裂类型名，因为二级索引文件名中需要这个信息
        public System.Collections.Hashtable title_hash = new System.Collections.Hashtable();
        public ObservableCollection<System.Collections.Hashtable> ms2_scan_hash = new ObservableCollection<System.Collections.Hashtable>();
        public ObservableCollection<System.Collections.Hashtable> ms1_scan_hash = new ObservableCollection<System.Collections.Hashtable>();
        int Min_scan = int.MaxValue;
        int Max_scan = int.MinValue;

        //卡FDR1%获得的PSM，将这些结果按raw_index.scan_number.pParse_number来进行标注
        System.Collections.Hashtable psm_hash = new Hashtable();
        #region
        //已经不使用
        //支持对字符串子串的筛选
        public System.Collections.Hashtable title_index_hash = new System.Collections.Hashtable();
        public System.Collections.Hashtable sq_index_hash = new System.Collections.Hashtable();

        //hash_next和hash_first为考虑了序列和修饰的查询。
        ObservableCollection<int> hash_next = new ObservableCollection<int>();
        ObservableCollection<int> hash_first = new ObservableCollection<int>();

        //hash_next_OnlySQ和hash_first_OnlySQ为只考虑了序列的查询。
        ObservableCollection<int> hash_next_OnlySQ = new ObservableCollection<int>();
        ObservableCollection<int> hash_first_OnlySQ = new ObservableCollection<int>();
        #endregion
        //MS1加速
        public double one_width;
        public double Intensity_t = 5e5; //判断MS1中的谱峰的电荷值根据同位素峰簇，首先去噪，将强度小于Intensity_t的谱峰不考虑
        public double ms1_mass_error = 20e-6; //从20ppm内查找一级谱中的谱峰


        //支持MGF显示，默认加载的不是MGF文件，是Raw文件
        bool isnot_mgf = true;

        //理论谱图预测模型
        public List<int> all_feature = new List<int>(); //所有需要的特征
        public List<double> all_aa = new List<double>(); //所有的0-1特征
        public List<List<Interv>> all_interv = new List<List<Interv>>(); //所有的非0-1特征
        public int[] aa_int = new int[26];

        //打开文件夹的路径可以事先纪录下来
        string defaultfilePath = "";

        //StartPage界面
        public StartPage startPage;

        //用来显示动画效果
        public bool is_display_animation = true; //默认以动画效果显示轻重对儿的色谱曲线，而如果切换则会显示选中的两根峰的色谱曲线
        public DispatcherTimer timer = null;
        public DispatcherTimer check_time_timer = null; //每隔12个小时运行一次，检测pBuild使用是否超过使用日期限制
        public DispatcherTimer welcome_timer = null;
        Process welcome_process = null;

        //支持二级谱定量功能
        public MS2_Quant_Help ms2_quant_help = new MS2_Quant_Help();
        public MS2_Quant_Help2 ms2_quant_help2 = new MS2_Quant_Help2();

        //获取新获得的肽段信息，这样可以使用Emass来计算对应的同位素峰簇
        public Peptide new_peptide = null;

        //pFind调用pBuild，对于一样的任务，需要对该任务重新加载
        //则将need_check设置为false,表示不用检测当前树结构中存不存在该任务，当然如果存在的话就删除重新加载
        //如果为true表示不用重新加载，仅提示该任务已经加载过
        bool need_check = true;


        //轻重3D图
        //文件索引
        private Evidence Evi = null;
        public Chrome_Help Chrome_help = null;
        ObservableCollection<long> start = new ObservableCollection<long>();
        ObservableCollection<long> end = new ObservableCollection<long>();
        System.Collections.Hashtable qut_scan_hash = new System.Collections.Hashtable(); //以title+scan进行索引
        //(Scan,Intense,MZ)坐标比例
        double max_intense = double.MinValue;
        double intense_factor;//intense轴的坐标比例，max_intense / 10
        double max_mz = 0;//最大的M/Z值
        double min_mz = Double.MaxValue;//最小的M/Z值
        int min_scan = int.MaxValue;
        int max_scan = 0;
        double scan_factor;//scan轴的坐标比例，(max_scan - min_scan) / 10
        double mz_factor;//一共有多个profile，得再长度为10的轴上均匀显示，所以，用此控制,10 除以profile总数
        //
        private Getlist function;
        private Style textStyle;
        private Point3DCollection AxisPoints = new Point3DCollection();
        private Point3DCollection TextPoints = new Point3DCollection();

        //3D Mouse Tracker
        private Point _previousPosition2D;
        private Vector3D _previousPosition3D = new Vector3D(0, 0, 1);

        private FrameworkElement _eventSource;
        private Transform3DGroup _transform;
        private ScaleTransform3D _scale = new ScaleTransform3D();
        private AxisAngleRotation3D _rotation = new AxisAngleRotation3D();
        private TranslateTransform3D _translate = new TranslateTransform3D();

        int osType = 1;//控制换行字符数的变量，Windows和Linux对于定量结果的换行符大小可能不一样，这里以Windows为准，为1

        //轻重3D图 END
        private Display_Help dis_help = new Display_Help();
        public Display_Help Dis_help
        {
            get { return dis_help; }
            set { dis_help = value; }
        }

        public PSM_Help psm_help = new PSM_Help("HCD");

        private PlotModel model2;
        public PlotModel Model2
        {
            get { return model2; }
            set
            {
                model2 = value;
                OnPropertyChanged("Model2");
            }
        }
        private PlotModel model1;
        public PlotModel Model1
        {
            get { return model1; }
            set
            {
                model1 = value;
                OnPropertyChanged("Model1");
            }
        }
        private PlotModel model_FDR;
        public PlotModel Model_FDR
        {
            get { return model_FDR; }
            set
            {
                model_FDR = value;
                OnPropertyChanged("Model_FDR");
            }
        }
        private PlotModel model_MassError_Da; //Summary面板，质量误差分布图
        public PlotModel Model_MassError_Da
        {
            get { return model_MassError_Da; }
            set
            {
                model_MassError_Da = value;
                OnPropertyChanged("Model_MassError_Da");
            }
        }
        private PlotModel model_MassError_PPM;
        public PlotModel Model_MassError_PPM
        {
            get { return model_MassError_PPM; }
            set
            {
                model_MassError_PPM = value;
                OnPropertyChanged("Model_MassError_PPM");
            }
        }
        private PlotModel model_Score;
        public PlotModel Model_Score
        {
            get { return model_Score; }
            set
            {
                model_Score = value;
                OnPropertyChanged("Model_Score");
            }
        }
        private PlotModel model_Mixed_Spectra;
        public PlotModel Model_Mixed_Spectra
        {
            get { return model_Mixed_Spectra; }
            set
            {
                model_Mixed_Spectra = value;
                OnPropertyChanged("Model_Mixed_Spectra");
            }
        }
        private PlotModel model_Specific;
        public PlotModel Model_Specific
        {
            get { return model_Specific; }
            set
            {
                model_Specific = value;
                OnPropertyChanged("Model_Specific");
            }
        }
        private PlotModel model_Modification;
        public PlotModel Model_Modification
        {
            get { return model_Modification; }
            set
            {
                model_Modification = value;
                OnPropertyChanged("Model_Modification");
            }
        }
        private PlotModel model_Missed_Cleavage;
        public PlotModel Model_Missed_Cleavage
        {
            get { return model_Missed_Cleavage; }
            set
            {
                model_Missed_Cleavage = value;
                OnPropertyChanged("Model_Missed_Cleavage");
            }
        }
        private PlotModel model_Length;
        public PlotModel Model_Length
        {
            get { return model_Length; }
            set
            {
                model_Length = value;
                OnPropertyChanged("Model_Length");
            }
        }
        private PlotModel model_Quantification;
        public PlotModel Model_Quantification
        {
            get { return model_Quantification; }
            set
            {
                model_Quantification = value;
                OnPropertyChanged("Model_Quantification");
            }
        }
        private PlotModel model_RawRate;
        public PlotModel Model_RawRate
        {
            get { return model_RawRate; }
            set
            {
                model_RawRate = value;
                OnPropertyChanged("Model_RawRate");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            try
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
            catch (Exception exe)
            {
                //MessageBox.Show(exe.ToString());
            }
        }
        private void ExecuteCmd(string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/c C:\\Windows\\System32\\cmd.exe";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            startInfo.Verb = "RunAs";
            Process p = new Process();
            p.StartInfo = startInfo;
            p.Start();
            p.StandardInput.WriteLine(command);
            p.StandardInput.WriteLine("exit");
            p.WaitForExit();
            p.Close();
        }
        private void start_server()
        {
            ExecuteCmd("java RmiSampleServer");
        }
        private void Initial_aa()
        {
            int start = 0;
            for (int i = 0; i < 26; ++i)
            {
                char tmp = (char)('A' + i);
                if (tmp != 'B' && tmp != 'J' && tmp != 'O' && tmp != 'U' && tmp != 'X' && tmp != 'Z' && tmp != 'L')
                    aa_int[i] = start++;
                else if (tmp == 'L')
                    aa_int[i] = aa_int['I' - 'A'];
            }
        }
        private void load_timer(object sender, RoutedEventArgs e) //加载动画计时器
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1.5);
            timer.Tick += display_ms1_chrome;
            timer.IsEnabled = false;
        }
        private void check_time(object sender, EventArgs e) //检查日期是否超过使用限制
        {
            Time_Help time_help = new Time_Help();
            bool flag = time_help.is_OK();
            if (flag)
                time_help.update_time(DateTime.Now, time_help.time_last_path);
            else
            {
                MessageBox.Show(Message_Help.DATE_OVER);
                Application.Current.Shutdown();
            }
        }
        private bool time_start() //运行pBuild的时候进行时间限制的检查
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
                Application.Current.Shutdown();
                return false;
            }
            return true;
        }
        public MainWindow() //双击pBuild.exe调用该函数
        {
            //// 检查软件是否过期
            //bool flag = time_start();
            //if (!flag)
            //{
            //    Application.Current.Shutdown();
            //    this.Close();
            //    return;
            //}
            startPage = new StartPage();
            //异常捕捉
            startPage.add_CrashHandler();
            //// 检查是否有注册文件
            //flag = startPage.check_license();

            //if (!flag)
            //{
            //    this.Close();
            //    return;
            //}
            if (Application.Current.Properties["ArbitraryArgName"] == null)
            {
                this.Hide();
                welcome_timer = new DispatcherTimer();
                welcome_timer.Interval = TimeSpan.FromSeconds(3);
                welcome_timer.Tick += initial_window;
                welcome_timer.IsEnabled = false;
                welcome_timer.Start();

                welcome_process = Process.Start("Welcome.exe", "pBuild");
            }
            else
            {
                string task_path = Application.Current.Properties["ArbitraryArgName"] as string;
                task_flag = get_task_flag(task_path);
                Application.Current.Properties["ArbitraryArgName"] = null;
                InitializeComponent();
                load_task(task_path);
            }
        }
        private void initial_window(object sender, EventArgs e)
        {
            try
            {
                welcome_process.Kill();
                welcome_process = null;
                welcome_timer.Stop();
                welcome_timer = null;
                this.Show();
                InitializeComponent();
                this.recent_tasks.ItemsSource = startPage.recent_taskInfo;
                if (startPage.recent_taskInfo.Count == 0)
                    this.recent_tasks.Visibility = Visibility.Collapsed;
            }
            catch (Exception exe)
            {
                Application.Current.Shutdown();
            }
        }
        public MainWindow(string task_path) //pFind.exe调用pBuild的时候运行该函数
        {
            /*
            //增加关机的服务，客户那边直接调用这边，就运行关机命令。。。
            //ExecuteCmd("start rmiregistry 2001");
            ThreadStart entry = new ThreadStart(start_server);
            Thread workThread = new Thread(entry);
            workThread.Start();
            //获取IP，并发送给对方
            ExecuteCmd("java Client_IP_Main");
            //哈哈
            */
            time_start();

            startPage = new StartPage();
            startPage.add_CrashHandler();

            bool flag = startPage.check_license();
            if (!flag)
            {
                this.Close();
                return;
            }

            backgroudWorker_cand_flag = false;
            task_flag = get_task_flag(task_path);
            Application.Current.Properties["ArbitraryArgName"] = null;
            InitializeComponent();
            load_task(task_path);
        }
        private void update_windows()
        {
            if (task_flag == "pTop")
            {
                //this.task_treeView.Visibility = Visibility.Visible;
                this.frame_left.MinWidth = 0.0;
                this.frame_left.Width = new GridLength(0.0);
                TabItem summary_item = this.summary_tab.Items[0] as TabItem;
                TabItem peptide_item = this.summary_tab.Items[1] as TabItem;
                TabItem protein_item = this.summary_tab.Items[2] as TabItem;
                summary_item.Visibility = Visibility.Collapsed;
                peptide_item.Visibility = Visibility.Visible;
                protein_item.Visibility = Visibility.Collapsed;
                this.summary_tab.SelectedIndex = 1;
                TabItem ms1_item = this.display_tab.Items[0] as TabItem;
                TabItem ms2_item = this.display_tab.Items[1] as TabItem;
                TabItem chrome_item = this.display_tab.Items[2] as TabItem;
                ms1_item.Visibility = Visibility.Visible;
                chrome_item.Visibility = Visibility.Visible;
            }
            else if (task_flag == "pNovo")
            {
                this.task_treeView.Visibility = Visibility.Collapsed;
                this.frame_left.MinWidth = 0.0;
                this.frame_left.Width = new GridLength(0.0);
                TabItem summary_item = this.summary_tab.Items[0] as TabItem;
                TabItem peptide_item = this.summary_tab.Items[1] as TabItem;
                TabItem protein_item = this.summary_tab.Items[2] as TabItem;
                summary_item.Visibility = Visibility.Collapsed;
                protein_item.Visibility = Visibility.Collapsed;
                this.summary_tab.SelectedIndex = 1;
                TabItem ms1_item = this.display_tab.Items[0] as TabItem;
                TabItem ms2_item = this.display_tab.Items[1] as TabItem;
                TabItem chrome_item = this.display_tab.Items[2] as TabItem;
                ms1_item.Visibility = Visibility.Collapsed;
                chrome_item.Visibility = Visibility.Collapsed;
            }
            this.display_tab.SelectedIndex = 1;
        }
        void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(hwnd).AddHook(new HwndSourceHook(WndProc));
        }
        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_SHOWME)
            {
                this.WindowState = WindowState.Maximized;
                //this.Show();
                this.Focus();
                string task_path = File_Help.read_task_path_Process(Task.task_path_process);
                if (task_path != "")
                {
                    need_check = false; //这里是pFind.exe调用进来，那么需要重新加载任务。
                    Initial_Task_AddTreeView(task_path);
                }
                handled = true;
            }
            return IntPtr.Zero;
        }
        //初始化Recent菜单，将最近打开的任务列表放在菜单中
        private void Initial_Recent_Task_Menu()
        {
            bool old_check = need_check;
            need_check = false;
            backgroudWorker_cand_flag = false;
            this.recent_task_menu.Items.Clear();
            List<string> recent_task_path = File_Help.load_all_recent_taskPath(Task.recent_task_path_file);
            for (int i = 0; i < recent_task_path.Count; ++i)
            {
                MenuItem mi = new MenuItem();
                mi.Header = recent_task_path[i];
                mi.PreviewMouseDown += (s, e) =>
                {
                    string task_path = mi.Header as string;
                    int index = task_path.IndexOf(' ');
                    task_path = task_path.Substring(index + 1);
                    task_flag = get_task_flag(task_path);

                    //查看task_path是否存在，如果不存在，则提示用户不存在该文件夹，该删除该路径
                    if (!Directory.Exists(task_path))
                    {
                        MessageBox.Show(Message_Help.FOLDER_NOT_EXIST);
                        recent_task_path.Remove(mi.Header as string);
                        File_Help.write_recent_task2(Task.recent_task_path_file, recent_task_path);
                        return;
                    }
                    if (task_flag == "pTop")
                    {
                        //Initial_Task_AddTreeView(task_path);
                        load_task(task_path);
                        update_windows();
                    }
                    else if (task_flag == "pNovo")
                    {
                        File_Help.update_recent_task(Task.recent_task_path_file, task_path);
                        File_Help.read_ModifyINI(Task.modify_ini_file); //读取修饰信息，修饰名字对应于它的偏移质量.
                        File_Help.read_ElementINI(Task.element_ini_file);
                        File_Help.read_AAINI(Task.aa_ini_file);
                        Config_Help.update_AA_Mass();

                        task = new Task(task_path, "pNovo");
                        psm_help = new PSM_Help(task.HCD_ETD_Type);
                        ms2_scan_hash.Clear();
                        ms1_scan_hash.Clear();

                        int index0 = 0;
                        foreach (DictionaryEntry di in task.title_PF2_Path)
                        {
                            string ms2_pfindex_file = (string)di.Value + ".pf2idx";
                            //string ms1_pfindex_file = (string)di.Value + ".pf1idx";
                            ms2_scan_hash.Add(File_Help.read_ms2_index(ms2_pfindex_file)); //读取二级谱图的索引，扫描号对应于索引文件的位置，可以获得谱图信息
                            //ms1_scan_hash.Add(File_Help.read_ms1_index(ms1_pfindex_file, ref Min_scan, ref Max_scan)); //读取一级谱图的索引
                            title_hash[(string)di.Key] = index0++;
                        }

                        File_Help.load_pNovo_param(task.pNovo_param_path);
                        all_psms = File_Help.load_pNovo_result(task.pNovo_result_path);
                        psms = all_psms;
                        data.ItemsSource = psms;
                        this.display_size.Text = psms.Count.ToString();

                        update_windows();
                    }
                };
                this.recent_task_menu.Items.Add(mi);
                this.need_check = old_check;
            }
        }
        private void SortHandler(object sender, DataGridSortingEventArgs e)
        {
            DataGridColumn column = e.Column;
            IComparer comparer = null;

            e.Handled = true;
            ListSortDirection direction = (column.SortDirection != ListSortDirection.Ascending) ? ListSortDirection.Ascending : ListSortDirection.Descending;
            column.SortDirection = direction;
            ListCollectionView lcv = (ListCollectionView)CollectionViewSource.GetDefaultView(data.ItemsSource);
            comparer = new DataGrid_Compare_Peptide(direction, column.Header.ToString());
            lcv.CustomSort = comparer;

            object item = this.data.SelectedItem;
            if (item != null)
                this.data.ScrollIntoView(item);
        }
        private void SortHandler2(object sender, DataGridSortingEventArgs e)
        {
            DataGridColumn column = e.Column;
            IComparer comparer = null;

            e.Handled = true;
            ListSortDirection direction = (column.SortDirection != ListSortDirection.Ascending) ? ListSortDirection.Ascending : ListSortDirection.Descending;
            column.SortDirection = direction;
            ListCollectionView lcv = (ListCollectionView)CollectionViewSource.GetDefaultView(protein_data.ItemsSource);
            comparer = new DataGrid_Compare_Protein(direction, column.Header.ToString());
            lcv.CustomSort = comparer;

            object item = this.protein_data.SelectedItem;
            if (item != null)
                this.protein_data.ScrollIntoView(item);
        }
        private void update_psms_And_graph(double q_value_t, BackgroundWorker backgroundWorker)
        {
            update_psms_By_FDR(q_value_t, backgroundWorker); //更新PSM列表
            string file_path = task.folder_result_path + "pFind__" + q_value_t.ToString("F5") + ".summary";
            if (File.Exists(file_path))
                read_summary_txt(file_path); //根据新的summary文件来更新Summary面板上面的文字信息
            display(); //根据新的summary文件来更新Summary面板中的图表信息
        }
        private bool update_psms_By_FDR(double q_value_t, BackgroundWorker backgroundWorker)
        {
            int start_progress = 50, end_progress = 69;
            psms = new ObservableCollection<PSM>();
            int iteration = all_psms.Count / 50;
            if (backgroundWorker == this.backgroundWorker)
                iteration = all_psms.Count / (end_progress - start_progress);
            for (int i = 0; i < all_psms.Count; ++i)
            {
                if (backgroundWorker.CancellationPending) //如果后台程序已经取消，即执行了代码：backgroundWorker.CancelAsync();
                    return false;
                if (backgroundWorker.IsBusy && i % iteration == 0 && backgroundWorker != null)
                {
                    if (backgroundWorker.WorkerReportsProgress)
                        backgroundWorker.ReportProgress(start_progress + i / iteration);
                }
                if (all_psms[i].Q_value <= q_value_t)
                {
                    psms.Add(all_psms[i]);
                    string title = all_psms[i].Title;
                    string[] strs = title.Split('.');
                    int scan_number = int.Parse(strs[1]);
                    int pParse_number = 0;
                    if (task.is_notMGF)
                    {
                        pParse_number = int.Parse(strs[4]);
                    }
                    psm_hash[strs[0] + "." + scan_number + "." + pParse_number] = psms.Count - 1;
                }
            }
            if (backgroundWorker.CancellationPending)
                return false;
            if (backgroundWorker.IsBusy && backgroundWorker != null && backgroundWorker.WorkerReportsProgress)
            {
                if (backgroundWorker == this.backgroundWorker)
                    backgroundWorker.ReportProgress(end_progress);
                else
                    backgroundWorker.ReportProgress(50);
            }

            if (backgroundWorker == this.backgroundWorker_fdr)
                backgroundWorker.ReportProgress(100);
            return true;
        }
        private int[] get_FDR_bin()
        {
            int[] bin_count = new int[10000];
            for (int i = 0; i < all_psms.Count; ++i)
            {
                int q_value_int = (int)(all_psms[i].Q_value * 10000);
                if (q_value_int < bin_count.Length)
                    ++bin_count[q_value_int];
            }
            return bin_count;
        }
        //加载任务的时候，主要是这块耗时 
        private bool read_allINI(BackgroundWorker backgroundWorker)
        {
            if (backgroundWorker.CancellationPending)
                return false;
            if (backgroundWorker.IsBusy && backgroundWorker != null && backgroundWorker.WorkerReportsProgress)
                backgroundWorker.ReportProgress(0);

            File_Help.read_ElementINI(Task.element_ini_file);
            File_Help.read_AAINI(Task.aa_ini_file);   // [wrm] 读取aa.ini
            File_Help.read_ModifyINI(Task.modify_ini_file);   // 读取modification.ini
            //读取FDR的值。
            File_Help.read_pFind_PFD(task.pFind_param_file, ref Config_Help.fdr_value);
            //读取pParse.cfg文件，查看搜索的是RAW还是MGF，如果是MGF，设置isnot_mgf为false,否则为true
            isnot_mgf = File_Help.read_pParse_PARA(task.pParse_param_file);
            task.is_notMGF = isnot_mgf;
            //读取pQuant.cfg文件，获取标记信息并设置给Config_Help.label_type
            File_Help.update_AA_Mass(task.all_aa_files); //读取所有的氨基酸质量表包括氨基酸对应的元素信息
            File_Help.update_Mod_Mass(task.all_mod_files); //读取所有的修饰质量表包括修饰对应的元素信息

            if (backgroundWorker.CancellationPending)
                return false;
            if (backgroundWorker.IsBusy && backgroundWorker != null && backgroundWorker.WorkerReportsProgress)
                backgroundWorker.ReportProgress(10);

            all_psms = File_Help.readPSM(task.identification_file); //读PSM列表。
            //update_Cand_PSMs(task.all_res_files, ref all_psms); //比较慢 

            if (backgroundWorker.CancellationPending)
                return false;
            if (backgroundWorker.IsBusy && backgroundWorker != null && backgroundWorker.WorkerReportsProgress)
                backgroundWorker.ReportProgress(50);
            if (!update_psms_By_FDR(Config_Help.fdr_value, backgroundWorker)) //将所有all_psms中的q_value<=0.01的放入到psms中
                return false;

            if (backgroundWorker.CancellationPending)
                return false;
            if (backgroundWorker.IsBusy && backgroundWorker != null && backgroundWorker.WorkerReportsProgress)
                backgroundWorker.ReportProgress(70);

            List<string> raw_file_paths = File_Help.read_RawINI(Task.raw_ini_file, ref title_hash, ref fragment_type);
            ms2_scan_hash.Clear();
            ms1_scan_hash.Clear();
            for (int i = 0; i < raw_file_paths.Count; ++i)
            {
                string raw_name = raw_file_paths[i]; //已经切除掉其后缀名.raw
                string ms2_pfindex_file = task.get_pf2Index_path(raw_name);
                string ms1_pfindex_file = task.get_pf1Index_path(raw_name);
                ms2_scan_hash.Add(File_Help.read_ms2_index(ms2_pfindex_file)); //读取二级谱图的索引，扫描号对应于索引文件的位置，可以获得谱图信息
                ms1_scan_hash.Add(File_Help.read_ms1_index(ms1_pfindex_file, ref Min_scan, ref Max_scan)); //读取一级谱图的索引
            }
            if (backgroundWorker.CancellationPending)
                return false;
            if (backgroundWorker.IsBusy && backgroundWorker != null && backgroundWorker.WorkerReportsProgress)
                backgroundWorker.ReportProgress(80);

            bool is_ok = true;
            if (isnot_mgf && task.quantification_file != "") //如果不是MGF，则搜索的是RAW，那么需要读取定量结果文件，需要提速
                is_ok = File_Help.create_3D_index(task.quantification_file, osType, title_hash, ref start, ref end, ref qut_scan_hash);
            if (!is_ok)
            {
                MessageBox.Show(Message_Help.Quant_Wrong);
                //Environment.Exit(0);
                //return false;
            }
            if (backgroundWorker.CancellationPending)
                return false;
            if (backgroundWorker.IsBusy && backgroundWorker != null && backgroundWorker.WorkerReportsProgress)
                backgroundWorker.ReportProgress(90);

            if (task.has_ratio) //如果没有跑定量就不用加载定量结果文件，需要提速
            {
                File_Help.getPSM_Ratio(task.quantification_list_file, psms);
            }

            if (backgroundWorker.CancellationPending)
                return false;
            if (backgroundWorker.IsBusy && backgroundWorker != null && backgroundWorker.WorkerReportsProgress)
                backgroundWorker.ReportProgress(100);

            return true;
        }
        //当单击Load菜项可以弹出对话框，让用户选择路径。
        private void load_Task(object sender, RoutedEventArgs e)
        {
            //System.Windows.Forms.FolderBrowserDialog f_dialog = new System.Windows.Forms.FolderBrowserDialog();

            //if (defaultfilePath != "")
            //{
            //    //设置此次默认目录为上一次选中目录  
            //    f_dialog.SelectedPath = defaultfilePath;
            //}
            //System.Windows.Forms.DialogResult dresult = f_dialog.ShowDialog();
            //if (dresult == System.Windows.Forms.DialogResult.Cancel)
            //{
            //    return;
            //}
            //if (dresult == System.Windows.Forms.DialogResult.OK)
            //{
            //    //记录选中的目录  
            //    defaultfilePath = f_dialog.SelectedPath;
            //}
            if (this.protein_panel != null && this.protein_panel.identification_proteins != null)
                this.protein_panel.identification_proteins.Clear();
            System.Windows.Forms.OpenFileDialog f_dialog = new System.Windows.Forms.OpenFileDialog();
            f_dialog.Filter = "task files (*.tsk)|*.tsk";
            //f_dialog.Filter = "pFind task files (*.pFind)|*.pFind|pNovo task files (*.pNovo)|*.pNovo";
            System.Windows.Forms.DialogResult dresult = f_dialog.ShowDialog();
            if (dresult == System.Windows.Forms.DialogResult.Cancel)
                return;

            string file_path = f_dialog.FileName;
            int last_index = file_path.LastIndexOf('\\');
            string folder_path = file_path.Substring(0, last_index);
            task_flag = get_task_flag(folder_path);
            Task check_task = new Task();
            if (!check_task.check_task_path(folder_path, task_flag))
            {
                MessageBox.Show(Message_Help.PATH_INVALID);
                task = null;
                return;
            }
            task_flag = get_task_flag(folder_path);
            if (task_flag == "pTop")
            {
                defaultfilePath = File_Help.read_task_path_pfind_ini(Task.pfind_ini_file);
                //Initial_Task_AddTreeView(folder_path);
                //读取理论谱图预测模型
                Initial_aa();
                File_Help.read_model(Task.model_path, 193 * 2, ref all_feature, ref all_aa, ref all_interv);
                task = new Task(folder_path, "pTop");
                update_windows();
            }
            else if (task_flag == "pNovo")
            {
                File_Help.update_recent_task(Task.recent_task_path_file, folder_path);
                File_Help.read_ModifyINI(Task.modify_ini_file); //读取修饰信息，修饰名字对应于它的偏移质量.
                File_Help.read_ElementINI(Task.element_ini_file);
                File_Help.read_AAINI(Task.aa_ini_file);
                Config_Help.update_AA_Mass();

                task = new Task(folder_path, "pNovo");
                psm_help = new PSM_Help(task.HCD_ETD_Type);
                ms2_scan_hash.Clear();
                ms1_scan_hash.Clear();

                int index = 0;
                foreach (DictionaryEntry di in task.title_PF2_Path)
                {
                    string ms2_pfindex_file = (string)di.Value + ".pf2idx";
                    //string ms1_pfindex_file = (string)di.Value + ".pf1idx";
                    ms2_scan_hash.Add(File_Help.read_ms2_index(ms2_pfindex_file)); //读取二级谱图的索引，扫描号对应于索引文件的位置，可以获得谱图信息
                    //ms1_scan_hash.Add(File_Help.read_ms1_index(ms1_pfindex_file, ref Min_scan, ref Max_scan)); //读取一级谱图的索引
                    title_hash[(string)di.Key] = index++;
                }

                File_Help.load_pNovo_param(task.pNovo_param_path);
                all_psms = File_Help.load_pNovo_result(task.pNovo_result_path);
                psms = all_psms;
                data.ItemsSource = psms;
                this.display_size.Text = psms.Count.ToString();

                update_windows();
            }
            load_task(folder_path);
        }
        private void load_Task_F5(object sender, RoutedEventArgs e)
        {
            if (task == null)
                return;
            bool old_check = need_check;
            need_check = false;
            backgroudWorker_cand_flag = false;
            load_task(task.folder_path);
            need_check = old_check;
        }
        //task_flag表示pFind任务还是pNovo任务,task_flag=pFind或者task_flag=pNovo
        private void Initial_Task(string task_path, int index)
        {
            if (task == null || task.folder_path != task_path || !need_check) //需要重新加载一个新的Task
            {
                int[] args = new int[2];
                args[0] = index;
                if (task == null)
                    args[1] = 0; //用来表示加载第一个任务
                else
                    args[1] = 1; //用来表示切换任务
                selected_protein = null;
                display_psms = null;
                File_Help.update_recent_task(Task.recent_task_path_file, task_path);
                string task_flag = get_task_flag(task_path);
                task = new Task(task_path, task_flag); //Task 任务的路径
                this.progressBar.Value = 0;
                this.progressBar.Visibility = Visibility.Visible;
                this.Cursor = Cursors.Wait;
                if (!backgroundWorker.IsBusy)
                    backgroundWorker.RunWorkerAsync(args);
                else
                {
                    backgroundWorker.CancelAsync();
                    //backgroundWorker.RunWorkerAsync(args);
                }

                //read_allINI(); //初始化各种ini文件、索引文件等读入内存。
            }
            else
            {
                if (index == 0)
                    initial_Summary(); //初始化Summary面板
                else if (index == 1)
                    initial_Peptide();
                else if (index == 2)
                    initial_Protein();
                TabItem one = (TabItem)summary_tab.Items[index];
                one.IsSelected = true; //显示Summary面板
            }
        }
        //将路径为task_path的任务从树结构中删除
        public void Remove_Task_TreeView(string task_path)
        {
            Remove_Task_By_Path(task_path);
        }
        //根据任务的路径，在下面找是否存在后缀名为.pFind的文件，如果存在那么打开的是pFind任务，而存在后缀名为.pNovo的文件，那么打开的是pNovo任务
        private string get_task_flag(string task_path)
        {
            string task_flag = "pTop";
            string[] files = Directory.GetFiles(task_path, "*");
            string flag_file = "";
            for (int i = 0; i < files.Length; ++i)
            {
                if (files[i].EndsWith(".tsk"))
                {
                    flag_file = files[i];
                    break;
                }
            }
            string all_strings = File.ReadAllText(flag_file);
            string[] strs = all_strings.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (strs[1].Contains("pTop"))
                task_flag = "pTop";
            else if (strs[1].Contains("pNovo"))
                task_flag = "pNovo";
            else
                task_flag = "pLink";
            return task_flag;
        }
        public void Initial_Task_AddTreeView(string task_path)
        {
            Task check_task = new Task();
            string task_flag = get_task_flag(task_path);

            if (!check_task.check_task_path(task_path, task_flag))
            {
                MessageBox.Show(Message_Help.PATH_INVALID);
                task = null;
                return;
            }

            this.other_psms_cbx.IsChecked = false;
            Ladder_Help.Initial();
            //查找树形结构中的所有任务是否包含该任务，如果该任务已经在树形结构中，则提示用户该任务已在树结构中
            for (int i = 0; i < this.task_treeView.Items.Count; ++i)
            {
                TreeViewItem one_task = (TreeViewItem)task_treeView.Items[i];
                StackPanel sp = (StackPanel)one_task.Header;
                string cur_task_path = ((TextBlock)sp.Children[0]).Text;
                cur_task_path = cur_task_path.Split('(')[1];
                cur_task_path = cur_task_path.Split(')')[0];
                if (cur_task_path == task_path && need_check)
                {
                    MessageBox.Show(Message_Help.TASK_ALREADY_LOAD);
                    one_task.IsSelected = true;
                    return;
                }
                else if (!need_check) //如果pFind调用pBuild，打开一个已经在pBuild加载的任务，那么这个时候需要将之前加载的这个任务删除，重新加载一遍
                {
                    Remove_Task_By_Item(one_task);
                }
            }
            //Initial_Task(task_path, 0);
            addTreeView(task_path.Split('\\').Last() + "(" + task_path + ")");
        }
        private void Rename_Task(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            TreeViewItem tvi = ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(mi)) as TreeViewItem;
            StackPanel sp = tvi.Header as StackPanel;
            TextBlock tbk = sp.Children[0] as TextBlock;
            if (tbk != null)
            {
                tbk.Visibility = Visibility.Collapsed;
                TextBox tbx = sp.Children[1] as TextBox;
                tbx.Visibility = Visibility.Visible;
                tbx.Text = tbk.Text;
                tbx.PreviewKeyDown += new KeyEventHandler(TaskName_KeyDown);
                //tbx.LostFocus += new RoutedEventHandler(Save_NewTaskName);
                tbx.Focus();
                tbx.Select(0, tbx.Text.IndexOf('('));  //tbx.SelectAll();
            }
        }
        private void Remove_Task_By_Item(TreeViewItem tvi) //如果删除的任务是当前选中的任务需要把所有属性清空
        {
            for (int i = 0; i < this.task_treeView.Items.Count; ++i) //穷举所有的任务
            {
                TreeViewItem one_task = (TreeViewItem)this.task_treeView.Items[i];
                if (one_task == tvi)
                {
                    this.task_treeView.Items.Remove(one_task);
                    break;
                }
            }
            if (need_check && this.task_treeView.Items.Count == 0)
            {
                //MainWindow mainW = new MainWindow();
                //mainW.Show();
                //this.Close();
                this.Close();
                Process.Start("pBuild.exe");
            }
        }
        private void Remove_Task_By_Path(string task_path)
        {
            for (int i = 0; i < this.task_treeView.Items.Count; ++i) //穷举所有的任务
            {
                TreeViewItem one_task = (TreeViewItem)this.task_treeView.Items[i];
                StackPanel sp = one_task.Header as StackPanel;
                TextBlock tbk = sp.Children[0] as TextBlock;
                string task_path0 = tbk.Text;
                task_path0 = task_path0.Split('(')[1];
                task_path0 = task_path0.Split(')')[0];
                if (task_path0 == task_path)
                {
                    this.task_treeView.Items.Remove(one_task);
                    break;
                }
            }
        }
        private void clear_pLink_tempFolder()
        {
            string folder_path = "pLink_tmp";
            if (!Directory.Exists(folder_path))
                return;
            try
            {
                Directory.Delete(folder_path, true);
            }
            catch (Exception exe)
            {
                MessageBox.Show(Message_Help.PDF_DELETE_WRONG + "\r\n" + exe.ToString());
            }
        }
        private void clear_tempFolder(TreeViewItem tvi)
        {
            StackPanel sp = tvi.Header as StackPanel;
            TextBlock tbk = sp.Children[0] as TextBlock;
            string txt = tbk.Text;
            string[] strs = txt.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            string folder_path = strs[1] + "\\result\\" + File_Help.pBuild_tmp_file;
            if (!Directory.Exists(folder_path))
                return;
            try
            {
                Directory.Delete(folder_path, true);
            }
            catch (Exception exe)
            {
                MessageBox.Show(Message_Help.PDF_DELETE_WRONG + "\r\n" + exe.ToString());
            }
        }
        private void Open_Folder_Task(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            TreeViewItem tvi = ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(mi)) as TreeViewItem;
            string folder_path = get_task_path_byMenuItem(tvi);

            string windir = Environment.GetEnvironmentVariable("WINDIR");
            System.Diagnostics.Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = windir + @"\explorer.exe";
            prc.StartInfo.Arguments = folder_path;
            prc.Start();
        }
        private void Run_pFind_Task(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            TreeViewItem tvi = ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(mi)) as TreeViewItem;
            string folder_path = get_task_path_byMenuItem(tvi);

            //MessageBox.Show(folder_path);
            Process.Start("pFind.exe", folder_path);
        }
        private void Remove_Task(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            TreeViewItem tvi = ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(mi)) as TreeViewItem;
            clear_tempFolder(tvi);
            Remove_Task_By_Item(tvi);
        }
        private string get_task_path_byMenuItem(TreeViewItem tvi)
        {
            StackPanel sp = tvi.Header as StackPanel;
            TextBlock tbk = sp.Children[0] as TextBlock;
            string folder_path = tbk.Text;
            folder_path = folder_path.Split('(')[1];
            folder_path = folder_path.Split(')')[0];
            return folder_path;
        }
        private void Delete_Task(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            TreeViewItem tvi = ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(mi)) as TreeViewItem;
            string folder_path = get_task_path_byMenuItem(tvi);
            try
            {
                DirectoryInfo di = new DirectoryInfo(folder_path);
                string message = Message_Help.TASK_DELETE_PROMPT;
                string caption = "Warning";
                MessageBoxButton buttons = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Warning;
                if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.Yes)
                {
                    di.Delete(true);
                    Remove_Task_By_Item(tvi);
                }
                else
                {
                    return;// Do not close the window
                }
            }
            catch (Exception exe)
            {
                MessageBox.Show(Message_Help.TASK_DELETE_REMOVE_WRONG + "\r\n" + exe.ToString());
                return;
            }
            //然后从treeView中删除该任务  

        }
        //对任务名进行修改
        private void TaskName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tbx = sender as TextBox;
                tbx.Visibility = Visibility.Collapsed;
                StackPanel sp = LogicalTreeHelper.GetParent(tbx) as StackPanel;
                TextBlock tbk = sp.Children[0] as TextBlock;
                tbk.Visibility = Visibility.Visible;
                string old_path = tbx.Text;
                old_path = old_path.Split('(')[1];
                old_path = old_path.Split(')')[0];
                string[] path_strs = old_path.Split('\\');
                string new_path = "";
                for (int i = 0; i < path_strs.Length - 1; ++i)
                {
                    new_path += path_strs[i] + "\\";
                }
                new_path += tbx.Text.Split('(')[0];
                tbk.Text = tbx.Text.Split('(')[0] + "(" + new_path + ")";
                try
                {
                    System.IO.Directory.Move(old_path, new_path);
                    task.pFind_param_file = task.pFind_param_file.Replace(path_strs.Last(), tbx.Text.Split('(')[0]);
                    File_Help.update_Rename_Task(task.pFind_param_file, path_strs.Last(), tbx.Text.Split('(')[0]);
                    File_Help.update_recent_task_byRename(Task.recent_task_path_file, old_path, new_path);
                }
                catch (Exception exe)
                {
                    MessageBox.Show(Message_Help.TASK_DELETE_REMOVE_WRONG + "\r\n" + exe.ToString());
                    tbk.Text = path_strs.Last() + "(" + old_path + ")";
                }
            }
        }

        private void addTreeView(string task_name)
        {
            this.Title = this.Title.Split('-').First() + "-" + task_name;
            TreeView one_treeView = this.task_treeView;
            TreeViewItem one_task = new TreeViewItem();
            ContextMenu task_menu = new ContextMenu();
            MenuItem open_folder_item = new MenuItem();
            open_folder_item.Header = "Open the Folder";
            open_folder_item.Click += Open_Folder_Task;
            task_menu.Items.Add(open_folder_item);
            MenuItem run_pfind_item = new MenuItem();
            run_pfind_item.Header = "Run pFind";
            run_pfind_item.Click += Run_pFind_Task;
            task_menu.Items.Add(run_pfind_item);
            MenuItem rename_item = new MenuItem();
            rename_item.Header = "Rename";
            rename_item.Click += Rename_Task;
            //task_menu.Items.Add(rename_item); //浩哥说：重命名任务可能有某些意外BUG，这里暂时关闭该功能.
            MenuItem remove_item = new MenuItem();
            remove_item.Header = "Remove";
            remove_item.Click += Remove_Task;
            task_menu.Items.Add(remove_item);
            MenuItem delete_item = new MenuItem();
            delete_item.Header = "Delete";
            delete_item.Click += Delete_Task;
            task_menu.Items.Add(delete_item);
            one_task.ContextMenu = task_menu;
            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            TextBlock tbk = new TextBlock();
            tbk.Text = task_name;
            sp.Children.Add(tbk);
            TextBox tbx = new TextBox();
            tbx.Visibility = Visibility.Collapsed;
            sp.Children.Add(tbx);
            one_task.Header = sp;
            //one_task.Header = task_name;
            one_task.IsExpanded = true;
            one_treeView.Items.Add(one_task);
            TreeViewItem summary_item = new TreeViewItem();
            summary_item.Header = "Summary";
            one_task.Items.Add(summary_item);
            TreeViewItem peptide_item = new TreeViewItem();
            peptide_item.Header = "Peptide";
            one_task.Items.Add(peptide_item);
            TreeViewItem protein_item = new TreeViewItem();
            protein_item.Header = "Protein";
            one_task.Items.Add(protein_item);
            //one_task.IsSelected = true;
            //one_task.Background = (Brush)FindResource(SystemColors.HighlightBrushKey);
            //one_task.Foreground = (Brush)FindResource(SystemColors.HighlightTextBrushKey);
        }
        //将扫描号为ms1_scan_num的一级谱的后面多张二级谱的母离子的m/z提取出来，放到fragment_mzs中
        //表示，这一张一级谱有哪些谱峰进行了碎裂
        public void read_peaks2(string path, string raw_name, System.Collections.Hashtable scan_hash, int ms1_scan_num,
            ref List<double> fragment_mzs, ref List<double> identification_mzs, ref List<int> identification_mzs_index)
        {
            if (!File.Exists(path))
                return;
            FileStream filestream = new FileStream(path, FileMode.Open);
            if (filestream == null)
                return;
            BinaryReader objBinaryReader = new BinaryReader(filestream);
            fragment_mzs = new List<double>();
            identification_mzs = new List<double>();
            ++ms1_scan_num;
            while (scan_hash[ms1_scan_num] != null)
            {
                long offset = Convert.ToInt64(scan_hash[ms1_scan_num]);
                filestream.Seek(offset, SeekOrigin.Begin);
                int s_num = objBinaryReader.ReadInt32();
                int peak_num = objBinaryReader.ReadInt32();
                for (int i = 0; i < peak_num; ++i)
                {
                    double mz = objBinaryReader.ReadDouble();
                    double intensity = objBinaryReader.ReadDouble();
                }
                int pre_all_num = objBinaryReader.ReadInt32();
                for (int i = 0; i < pre_all_num; ++i)
                {
                    double pep_mass = objBinaryReader.ReadDouble();
                    fragment_mzs.Add(pep_mass);
                    int charge = objBinaryReader.ReadInt32();
                    if (psm_hash[raw_name + "." + ms1_scan_num + "." + i] != null) //表示该二级谱鉴定到了
                    {
                        identification_mzs.Add(pep_mass);
                        identification_mzs_index.Add((int)psm_hash[raw_name + "." + ms1_scan_num + "." + i]);
                    }
                }
                ++ms1_scan_num;
            }
            objBinaryReader.Close();
            filestream.Close();
        }
        public double read_peaks(string path, System.Collections.Hashtable scan_hash, int scan_num, int pre_num, ref ObservableCollection<PEAK> peaks, ref double pep_mass)
        {
            int charge = 0;
            return read_peaks(path, scan_hash, scan_num, pre_num, ref peaks, ref pep_mass, ref charge);
        }
        public double read_peaks(string path, System.Collections.Hashtable scan_hash, int scan_num, int pre_num, ref ObservableCollection<PEAK> peaks, ref double pep_mass, ref int charge)
        {
            if (!File.Exists(path))
                return 0.0;
            FileStream filestream = new FileStream(path, FileMode.Open);
            if (filestream == null)
                return 0.0;
            BinaryReader objBinaryReader = new BinaryReader(filestream);
            //if (scan_hash[scan_num] == null) //出现问题
            //{
            //    MessageBox.Show("NULL MS1.");
            //    Application.Current.Shutdown();
            //}
            long offset = Convert.ToInt64(scan_hash[scan_num]);
            filestream.Seek(offset, SeekOrigin.Begin);
            int s_num = objBinaryReader.ReadInt32();
            int peak_num = objBinaryReader.ReadInt32();
            double max_Inten = 0.0;
            for (int i = 0; i < peak_num; ++i)
            {
                double mz = objBinaryReader.ReadDouble();
                double intensity = objBinaryReader.ReadDouble();
                if (intensity > max_Inten)
                    max_Inten = intensity;
                peaks.Add(new PEAK(mz, intensity));
            }
            for (int i = 0; i < peaks.Count; ++i)
            {
                peaks[i].Intensity = 100 * peaks[i].Intensity / max_Inten;
            }
            int pre_all_num = objBinaryReader.ReadInt32();
            if (pre_num >= pre_all_num)
            {
                objBinaryReader.Close();
                filestream.Close();
                return 0.0;
            }
            for (int i = 0; i <= pre_num; ++i)
            {
                pep_mass = objBinaryReader.ReadDouble();
                charge = objBinaryReader.ReadInt32();
            }
            objBinaryReader.Close();
            filestream.Close();
            return max_Inten;
        }
        private Peptide get_peptide()
        {
            if (selected_psm == null)
                return null;
            if (this.new_peptide != null)
                return this.new_peptide;
            Peptide pep = new Peptide(selected_psm.Sq);
            double pep_theory_mass = 0.0;
            pep.Mods = Modification.get_modSites(selected_psm.Sq, selected_psm.Mod_sites, int.Parse(selected_psm.Pep_flag), ref pep_theory_mass);
            pep.Pepmass = pep_theory_mass;
            pep.Tag_Flag = int.Parse(selected_psm.Pep_flag);
            return pep;

        }
        private Spectra get_spectra()
        {
            ObservableCollection<PEAK> peaks = new ObservableCollection<PEAK>();
            double pepmass = 0.0;
            if (selected_psm == null)
                return null;
            string title = selected_psm.Title;
            string[] strs = title.Split('.');
            string raw_name = strs[0];
            int scan_num = int.Parse(strs[1]);
            int charge = int.Parse(strs[3]);
            int pre_num = 0;
            if (strs.Length > 4 && strs[4] != "dta")
                pre_num = int.Parse(strs[4]);
            string ms2_pf_file = task.get_pf2_path(raw_name);
            int title_hash_index = 0;
            if (title_hash[raw_name] != null)
            {
                title_hash_index = (int)(title_hash[raw_name]);
            }
            else
            {
                //MessageBox.Show("Title and pf2 file don't consistent");
            }

            double maxInten = read_peaks(ms2_pf_file, ms2_scan_hash[title_hash_index], scan_num, pre_num, ref peaks, ref pepmass, ref charge);
            return new Spectra(title, charge, pepmass, peaks, maxInten);
        }
        private void display_one_psm_ms2()
        {
            Spectra spec = get_spectra();
            Peptide pep = new Peptide(selected_psm.Sq);
            double pep_theory_mass = 0.0;
            pep.Mods = Modification.get_modSites(selected_psm.Sq, selected_psm.Mod_sites, int.Parse(selected_psm.Pep_flag), ref pep_theory_mass);
            pep.Pepmass = pep_theory_mass;

            psm_help.Switch_PSM_Help(spec, pep); //更新谱图Spec及肽段Pep
            psm_help.Pep.Tag_Flag = int.Parse(selected_psm.Pep_flag);

            dis_help = new Display_Help(psm_help, dis_help.ddh, dis_help.ddhms1);
            this.Model2 = dis_help.display_ms2();
            MS2_Help ms2_help = new MS2_Help(Model2, dis_help, Ladder_Help.Scale_Width, Ladder_Help.Scale_Height, this.frame_right.ActualWidth, this.display_tab.ActualHeight, this);
            this.Model2.Axes[1].Reset();
            this.Model2.Axes[3].Reset();
            this.Model2.Axes[1].AxisChanged += (s, er) =>
            {
                ms2_help = new MS2_Help(Model2, dis_help, Ladder_Help.Scale_Width, Ladder_Help.Scale_Height, this.frame_right.ActualWidth, this.display_tab.ActualHeight, this);
                ms2_help.window_sizeChg_Or_ZoomPan();
            };
            this.DataContext = this;
            this.Model2.RefreshPlot(true);
            if (selected_psm != null)
                ms2_help.window_sizeChg_Or_ZoomPan(); //伪造窗口改变，让阶梯图重新绘制
        }
        //更新MS1中的两个mz框的初始值
        private void update_ms1_mz_tbx()
        {
            this.ms1_mz1_txt.Text = this.selected_ms1.Pepmass_theory.ToString("F5");
            this.ms1_mz2_txt.Text = this.selected_ms1.Pepmass_theory.ToString("F5");
            this.ms1_mz3_txt.Text = "0.00000Th/0.00ppm";
        }
        public void parse_PSM(PSM psm, ref Spectra spec, ref Peptide pep)
        {
            ObservableCollection<PEAK> peaks = new ObservableCollection<PEAK>();
            double pepmass = 0.0;
            if (psm == null)
                return;

            string title = psm.Title;
            string[] strs = title.Split('.');
            string raw_name = strs[0];
            int scan_num = int.Parse(strs[1]);
            int charge = int.Parse(strs[3]);
            int pre_num = 0;
            if (strs.Length > 4 && strs[4] != "dta")
                pre_num = int.Parse(strs[4]);
            string ms2_pf_file = task.get_pf2_path(raw_name);
            int title_hash_index = 0;
            if (title_hash[raw_name] != null)
            {
                title_hash_index = (int)(title_hash[raw_name]);
            }
            else
            {
                //MessageBox.Show("Title and pf2 file don't consistent");
            }

            double maxInten = read_peaks(ms2_pf_file, ms2_scan_hash[title_hash_index], scan_num, pre_num, ref peaks, ref pepmass, ref charge);

            pep = new Peptide(psm.Sq);
            double pep_theory_mass = 0.0;
            pep.Mods = Modification.get_modSites(psm.Sq, psm.Mod_sites, int.Parse(psm.Pep_flag), ref pep_theory_mass);
            pep.Pepmass = pep_theory_mass;
            spec = new Spectra(title, charge, pepmass, peaks, maxInten);
        }
        //点击PSM列表中的某一行，那么就显示对应的匹配二级谱图
        public void s_ch(object sender, SelectionChangedEventArgs e)
        {
            //该if语句用来防止在多线程加载数据的时候，用户对PSM列表进行点击出现崩溃现象
            if (task_flag == "pTop")
            {
                if (ms2_scan_hash.Count == 0 || Config_Help.modStr_hash.Count == 0)
                    return;
                selected_psm = (PSM)data.SelectedItem;
                ObservableCollection<PEAK> peaks = new ObservableCollection<PEAK>();
                double pepmass = 0.0;
                if (selected_psm == null)
                    return;
                this.new_peptide = null;
                this.selected_size.Text = this.data.SelectedItems.Count + "";

                string title = selected_psm.Title;
                string[] strs = title.Split('.');
                string raw_name = strs[0];
                int scan_num = int.Parse(strs[1]);
                int charge = int.Parse(strs[3]);
                int pre_num = 0;
                if (strs.Length > 4 && strs[4] != "dta")
                    pre_num = int.Parse(strs[4]);


                string ms2_pf_file = task.get_pf2_path(raw_name);
                int title_hash_index = 0;
                if (title_hash[raw_name] != null)
                {
                    title_hash_index = (int)(title_hash[raw_name]);
                }
                else
                {
                    for (int i = 0; i < ms2_scan_hash.Count; ++i)
                    {
                        if (ms2_scan_hash[i][scan_num] != null)
                        {
                            title_hash_index = i;
                            break;
                        }
                    }
                }
                //MessageBox.Show(ms2_pf_file); // + ms2_scan_hash[title_hash_index] + "\r\n" + "\r\n" + scan_num + "\r\n" + pre_num
                double maxInten = read_peaks(ms2_pf_file, ms2_scan_hash[title_hash_index], scan_num, pre_num, ref peaks, ref pepmass, ref charge);

                Peptide pep = new Peptide(selected_psm.Sq);
                double pep_theory_mass = 0.0;
                pep.Mods = Modification.get_modSites(selected_psm.Sq, selected_psm.Mod_sites, int.Parse(selected_psm.Pep_flag), ref pep_theory_mass);
                pep.Pepmass = pep_theory_mass;
                Spectra spec = new Spectra(title, charge, pepmass, peaks, maxInten);
                //psm_help = new PSM_Help(task.HCD_ETD_Type);
                psm_help.Switch_PSM_Help(spec, pep); //更新谱图Spec及肽段Pep
                //psm_help.aa_index = int.Parse(selected_psm.Pep_flag);
                psm_help.Pep.Tag_Flag = int.Parse(selected_psm.Pep_flag);
                if (isnot_mgf)
                {
                    int tmp_scan = scan_num - 1;
                    while (ms1_scan_hash[(int)(title_hash[raw_name])][tmp_scan] == null)
                    {
                        tmp_scan--;
                    }

                    double tmp_pepmass = 0.0;
                    peaks = new ObservableCollection<PEAK>();
                    string ms1_pf_file = task.get_pf1_path(raw_name);
                    maxInten = read_peaks(ms1_pf_file, ms1_scan_hash[(int)(title_hash[raw_name])], tmp_scan, 0, ref peaks, ref tmp_pepmass);
                    selected_ms1 = new Spectra_MS1(peaks, tmp_scan, 0, pepmass,
                        (pep_theory_mass + (charge - 1) * Config_Help.massZI) / charge, charge, maxInten);
                    selected_ms1.Retention_Time = tmp_pepmass;
                }

                if (display_tab.SelectedIndex == 1) //MS2
                {
                    if (Ms2_advance.is_preprocess)
                        psm_help.Spec.preprocess(psm_help.Ppm_mass_error, psm_help.Da_mass_error, false);
                    dis_help = new Display_Help(psm_help, dis_help.ddh, dis_help.ddhms1);
                    this.Model2 = dis_help.display_ms2();
                    if (this.Model2 == null)
                        return;

                    MS2_Help ms2_help = new MS2_Help(Model2, dis_help, Ladder_Help.Scale_Width, Ladder_Help.Scale_Height, this.frame_right.ActualWidth, this.display_tab.ActualHeight, this);
                    this.Model2.Axes[1].AxisChanged += (s, er) =>
                    {
                        ms2_help = new MS2_Help(Model2, dis_help, Ladder_Help.Scale_Width, Ladder_Help.Scale_Height, this.frame_right.ActualWidth, this.display_tab.ActualHeight, this);
                        ms2_help.window_sizeChg_Or_ZoomPan();
                    };
                    this.DataContext = this;
                    this.Model2.RefreshPlot(true);
                }
                else if (display_tab.SelectedIndex == 0) //MS1
                {
                    if (!isnot_mgf)
                    {
                        MessageBox.Show(Message_Help.NO_DATA);
                        return;
                    }
                    update_ms1_mz_tbx();
                    dis_help = new Display_Help(selected_ms1, psm_help, this.dis_help.ddh, dis_help.ddhms1);
                    this.DataContext = this;
                    double ms1_pepmass = 0.0;
                    int frag_index = -1;
                    if (task.quantification_file == "")
                        this.Model1 = dis_help.display_ms1(selected_ms1.Pepmass_ms2 - 3, selected_ms1.Pepmass_ms2 + 3, ref ms1_pepmass, ref frag_index);
                    else
                    {
                        string[] strs1 = selected_psm.Title.Split('.');
                        int scan = int.Parse(strs1[1]);
                        int pParse_num = int.Parse(strs1[4]);
                        List<string> list = new List<string>();
                        if (qut_scan_hash[(int)(title_hash[selected_psm.Title.Split('.')[0]]) + "." + scan + "." + pParse_num] != null)
                        {
                            File_Help.read_3D_List(task.quantification_file, osType,
                                (int)qut_scan_hash[(int)(title_hash[selected_psm.Title.Split('.')[0]]) + "." + scan + "." + pParse_num], start, end, ref list);
                            this.function = new Getlist();
                            this.function.GetEvidenceFromList(list);
                            double min_mz = this.function.evi.TheoricalIsotopicMZList.First().First() - 2.5;
                            double max_mz = this.function.evi.TheoricalIsotopicMZList.Last().Last() + 2.5;
                            this.Model1 = dis_help.display_ms1(min_mz, max_mz, ref ms1_pepmass, ref frag_index);
                        }
                        else
                        {
                            this.Model1 = dis_help.display_ms1(selected_ms1.Pepmass_ms2 - 3, selected_ms1.Pepmass_ms2 + 3, ref ms1_pepmass, ref frag_index);
                        }
                    }
                    if (this.Model1 == null)
                        return;
                    this.Model1.Axes[1].Reset();
                    this.Model1.Axes[1].AxisChanged += (s, er) =>
                    {
                        window_sizeChg_Or_ZommPan_ms1();
                    };
                    selected_ms1.Pepmass = ms1_pepmass;
                    selected_ms1.Fragment_index = frag_index;
                    this.Model1.RefreshPlot(true);
                }
                else //3D
                {
                    if (!isnot_mgf)
                    {
                        MessageBox.Show(Message_Help.NO_DATA);
                        IsInnerChange = true;
                        display_tab.SelectedIndex = 2;
                        return;
                    }
                    if (timer != null)
                        timer.Stop();
                    if (selected_psm != null)
                    {
                        string[] strs1 = selected_psm.Title.Split('.');
                        int scan = int.Parse(strs1[1]);
                        int pParse_num = int.Parse(strs1[4]);
                        List<string> list = new List<string>();
                        this.function = new Getlist();
                        if (qut_scan_hash[(int)(title_hash[selected_psm.Title.Split('.')[0]]) + "." + scan + "." + pParse_num] != null &&
                            this.new_peptide == null)
                        {
                            File_Help.read_3D_List(task.quantification_file, osType,
                                (int)qut_scan_hash[(int)(title_hash[selected_psm.Title.Split('.')[0]]) + "." + scan + "." + pParse_num], start, end, ref list);
                            this.function.GetEvidenceFromList(list);
                        }
                        else //如果定量文件没有，或者定量文件中不存在该PSM的结果，那么需要根据emass自己计算3D图
                        {
                            this.Chrome_help = get_theory_iostope();
                            if (new_peptide == null)
                                new_peptide = this.Dis_help.Psm_help.Pep;
                            this.function.GetEvidenceFromChrom(this.Chrome_help, this);
                        }
                        if (this.function.evi.ScanTime.Count == 0)
                            return;
                        AddAxis();
                        TrackMouse();

                        quant_init();
                        this.viewport3D.Children.Clear();
                        this.text_canvas.Children.Clear();
                        TextPoints.Clear();
                        GenerateBackgroundGrid();
                        AddAxis2();
                        ChangeAxisTextPosition();
                        ChangeTextPosition();
                        if ((bool)this.chrom_3D.IsChecked)
                            UseGeometry();
                        else
                            UseWirePolyLine();
                    }
                }
            }
            else if (task_flag == "pNovo")
            {
                this.selected_size.Text = this.data.SelectedItems.Count + "";
                selected_psm = (PSM)data.SelectedItem;
                if (selected_psm == null)
                    return;
                if (display_tab.SelectedIndex == 1) //MS2
                {
                    

                    Peptide pep = new Peptide(selected_psm.Sq);
                    double pep_theory_mass = 0.0;
                    pep.Mods = Modification.get_modSites(selected_psm.Sq, selected_psm.Mod_sites, 0, ref pep_theory_mass);
                    pep.Pepmass = pep_theory_mass;
                    Spectra spec = this.spectra[(int)this.spectra_hash[selected_psm.Title]];
                    //psm_help = new PSM_Help(task.HCD_ETD_Type);
                    psm_help.Switch_PSM_Help(spec, pep); //更新谱图Spec及肽段Pep
                    //psm_help.aa_index = 0;
                    psm_help.Pep.Tag_Flag = 0;

                    dis_help = new Display_Help(psm_help, dis_help.ddh, dis_help.ddhms1);
                    this.Model2 = dis_help.display_ms2();
                    if (this.Model2 == null)
                        return;

                    MS2_Help ms2_help = new MS2_Help(Model2, dis_help, Ladder_Help.Scale_Width, Ladder_Help.Scale_Height, this.frame_right.ActualWidth, this.display_tab.ActualHeight, this);
                    this.Model2.Axes[1].AxisChanged += (s, er) =>
                    {
                        ms2_help = new MS2_Help(Model2, dis_help, Ladder_Help.Scale_Width, Ladder_Help.Scale_Height, this.frame_right.ActualWidth, this.display_tab.ActualHeight, this);
                        ms2_help.window_sizeChg_Or_ZoomPan();
                    };
                    this.DataContext = this;
                    this.Model2.RefreshPlot(true);
                }
            }
            else if (task_flag == "MGF") //打开的是MGF文件，不需要加载任务
            {
                selected_psm = (PSM)this.data.SelectedItem;
                if (selected_psm == null)
                    return;

                Spectra spec = spectra[selected_psm.Id - 1];
                ObservableCollection<PEAK> peaks = spec.Peaks;
                Peptide pep = new Peptide(selected_psm.Sq);
                double pep_theory_mass = 0.0;
                pep.Mods = Modification.get_modSites(selected_psm.Sq, selected_psm.Mod_sites, 0, ref pep_theory_mass);
                pep.Pepmass = pep_theory_mass;
                //psm_help = new PSM_Help(task.HCD_ETD_Type);
                psm_help.Switch_PSM_Help(spec, pep); //更新谱图Spec及肽段Pep
                //psm_help.aa_index = 0;
                psm_help.Pep.Tag_Flag = 0;

                dis_help = new Display_Help(psm_help, dis_help.ddh, dis_help.ddhms1);
                this.Model2 = dis_help.display_ms2();
                if (this.Model2 == null)
                    return;

                MS2_Help ms2_help = new MS2_Help(Model2, dis_help, Ladder_Help.Scale_Width, Ladder_Help.Scale_Height, this.frame_right.ActualWidth, this.display_tab.ActualHeight, this);
                this.Model2.Axes[1].AxisChanged += (s, er) =>
                {
                    ms2_help = new MS2_Help(Model2, dis_help, Ladder_Help.Scale_Width, Ladder_Help.Scale_Height, this.frame_right.ActualWidth, this.display_tab.ActualHeight, this);
                    ms2_help.window_sizeChg_Or_ZoomPan();
                };
                this.DataContext = this;
                this.Model2.RefreshPlot(true);
            }
            else if (task_flag == "pLink")
            {
                //this.new_peptide = null;
                pLink.PSM selected_psm = (pLink.PSM)this.data.SelectedItem;
                this.selected_psm = selected_psm;
                if (selected_psm == null)
                    return;
                if (selected_psm.Peptide_Number == 2)
                {
                    pLink.PSM_Help_2 psm_help_2 = new pLink.PSM_Help_2("HCD");
                    if (dis_help.Psm_help is pLink.PSM_Help_2)
                        psm_help_2 = dis_help.Psm_help as pLink.PSM_Help_2;
                    if (this.pLink_result.title_index == null || this.pLink_result.title_index.Count == 0)
                    {
                        psm_help_2.Spec = get_spectra();
                        double tmp_pepmass = 0.0;
                        ObservableCollection<PEAK> peaks = new ObservableCollection<PEAK>();
                        string title = selected_psm.Title;
                        string[] strs = title.Split('.');
                        string raw_name = strs[0];
                        int charge = psm_help_2.Spec.Charge;
                        string ms1_pf_file = task.get_pf1_path(raw_name);
                        int tmp_scan = psm_help_2.Spec.getScan() - 1;
                        while (ms1_scan_hash[(int)(title_hash[raw_name])][tmp_scan] == null)
                        {
                            tmp_scan--;
                        }
                        double maxInten = read_peaks(ms1_pf_file, ms1_scan_hash[(int)(title_hash[raw_name])], tmp_scan, 0, ref peaks, ref tmp_pepmass);
                        double pep_theory_mass = 0.0;
                        selected_ms1 = new Spectra_MS1(peaks, tmp_scan, 0, psm_help_2.Spec.Pepmass,
                            (pep_theory_mass + (charge - 1) * Config_Help.massZI) / charge, charge, maxInten);
                        selected_ms1.Retention_Time = tmp_pepmass;
                    }
                    else
                    {
                        int spec_index = (int)this.pLink_result.title_index[selected_psm.Title];
                        Spectra spectra = this.pLink_result.spectra[spec_index];
                        psm_help_2.Spec = new Spectra(selected_psm.Title, spectra.Charge, spectra.Pepmass, spectra.Peaks, spectra.Max_inten_E);
                    }
                    psm_help_2.Pep1 = selected_psm.Peptide[0];
                    psm_help_2.Pep2 = selected_psm.Peptide[1];
                    psm_help_2.Pep = psm_help_2.Pep1;
                    psm_help_2.Link_pos1 = selected_psm.Peptide_Link_Position[0];
                    psm_help_2.Link_pos2 = selected_psm.Peptide_Link_Position[1];
                    int xlink_flag = int.Parse(selected_psm.Pep_flag.Split('|')[1]);
                    psm_help_2.Xlink_mass = pLink_result.Link_masses[xlink_flag - 1];
                    double score = psm_help_2.Match(1);
                    dis_help = new Display_Help(psm_help_2, dis_help.ddh, dis_help.ddhms1);
                    this.Model2 = dis_help.display_ms2(psm_help_2.Peptide_Number);
                    if (this.Model2 == null)
                        return;
                }
                else
                {
                    pLink.PSM_Help_3 psm_help_3 = new pLink.PSM_Help_3("HCD");
                    if (dis_help.Psm_help is pLink.PSM_Help_3)
                        psm_help_3 = dis_help.Psm_help as pLink.PSM_Help_3;
                    int spec_index = (int)this.pLink_result.title_index[selected_psm.Title];
                    Spectra spectra = this.pLink_result.spectra[spec_index];
                    psm_help_3.Spec = new Spectra(selected_psm.Title, spectra.Charge, spectra.Pepmass, spectra.Peaks, spectra.Max_inten_E);
                    psm_help_3.Pep1 = selected_psm.Peptide[0];
                    psm_help_3.Pep2 = selected_psm.Peptide[1];
                    psm_help_3.Pep3 = selected_psm.Peptide[2];
                    psm_help_3.Pep = psm_help_3.Pep1;
                    psm_help_3.Link_pos1 = selected_psm.Peptide_Link_Position[0];
                    psm_help_3.Link_pos2 = selected_psm.Peptide_Link_Position[1];
                    psm_help_3.Link_pos3 = selected_psm.Peptide_Link_Position[2];
                    psm_help_3.Link_pos4 = selected_psm.Peptide_Link_Position[3];
                    psm_help_3.Xlink_mass = pLink_result.Link_mass;

                    double score = psm_help_3.Match(1);

                    dis_help = new Display_Help(psm_help_3, dis_help.ddh, dis_help.ddhms1);
                    this.Model2 = dis_help.display_ms2(psm_help_3.Peptide_Number);
                    if (this.Model2 == null)
                        return;
                }
                if (display_tab.SelectedIndex == 1)
                {
                    MS2_Help ms2_help = new MS2_Help(Model2, dis_help, Ladder_Help.Scale_Width, Ladder_Help.Scale_Height, this.frame_right.ActualWidth, this.display_tab.ActualHeight, this);
                    this.Model2.Axes[1].AxisChanged += (s, er) =>
                    {
                        ms2_help = new MS2_Help(Model2, dis_help, Ladder_Help.Scale_Width, Ladder_Help.Scale_Height, this.frame_right.ActualWidth, this.display_tab.ActualHeight, this);
                        ms2_help.window_sizeChg_Or_ZoomPan();
                    };
                    this.DataContext = this;
                    this.Model2.RefreshPlot(true);
                }
                else if (display_tab.SelectedIndex == 0)
                {
                    if (!isnot_mgf)
                    {
                        MessageBox.Show(Message_Help.NO_DATA);
                        return;
                    }
                    update_ms1_mz_tbx();
                    dis_help = new Display_Help(selected_ms1, dis_help.Psm_help, this.dis_help.ddh, dis_help.ddhms1);
                    this.DataContext = this;
                    double ms1_pepmass = 0.0;
                    int frag_index = -1;
                    if (task.quantification_file == "")
                        this.Model1 = dis_help.display_ms1(selected_ms1.Pepmass_ms2 - 3, selected_ms1.Pepmass_ms2 + 3, ref ms1_pepmass, ref frag_index);
                    else
                    {
                        string[] strs1 = selected_psm.Title.Split('.');
                        int scan = int.Parse(strs1[1]);
                        int pParse_num = int.Parse(strs1[4]);
                        List<string> list = new List<string>();
                        if (qut_scan_hash[(int)(title_hash[selected_psm.Title.Split('.')[0]]) + "." + scan + "." + pParse_num] != null)
                        {
                            File_Help.read_3D_List(task.quantification_file, osType,
                                (int)qut_scan_hash[(int)(title_hash[selected_psm.Title.Split('.')[0]]) + "." + scan + "." + pParse_num], start, end, ref list);
                            this.function = new Getlist();
                            this.function.GetEvidenceFromList(list);
                            double min_mz = this.function.evi.TheoricalIsotopicMZList.First().First() - 2.5;
                            double max_mz = this.function.evi.TheoricalIsotopicMZList.Last().Last() + 2.5;
                            this.Model1 = dis_help.display_ms1(min_mz, max_mz, ref ms1_pepmass, ref frag_index);
                        }
                        else
                        {
                            this.Model1 = dis_help.display_ms1(selected_ms1.Pepmass_ms2 - 3, selected_ms1.Pepmass_ms2 + 3, ref ms1_pepmass, ref frag_index);
                        }
                    }
                    if (this.Model1 == null)
                        return;
                    this.Model1.Axes[1].Reset();
                    this.Model1.Axes[1].AxisChanged += (s, er) =>
                    {
                        window_sizeChg_Or_ZommPan_ms1();
                    };
                    selected_ms1.Pepmass = ms1_pepmass;
                    selected_ms1.Fragment_index = frag_index;
                    this.Model1.RefreshPlot(true);
                }
                else //3D
                {
                    if (!isnot_mgf)
                    {
                        MessageBox.Show(Message_Help.NO_DATA);
                        IsInnerChange = true;
                        display_tab.SelectedIndex = 2;
                        return;
                    }
                    if (timer != null)
                        timer.Stop();
                    if (selected_psm != null)
                    {
                        string[] strs1 = selected_psm.Title.Split('.');
                        int scan = int.Parse(strs1[1]);
                        int pParse_num = int.Parse(strs1[4]);
                        List<string> list = new List<string>();
                        this.function = new Getlist();
                        if (qut_scan_hash[(int)(title_hash[selected_psm.Title.Split('.')[0]]) + "." + scan + "." + pParse_num] != null &&
                            this.new_peptide == null)
                        {
                            File_Help.read_3D_List(task.quantification_file, osType,
                                (int)qut_scan_hash[(int)(title_hash[selected_psm.Title.Split('.')[0]]) + "." + scan + "." + pParse_num], start, end, ref list);
                            this.function.GetEvidenceFromList(list);
                        }
                        else //如果定量文件没有，或者定量文件中不存在该PSM的结果，那么需要根据emass自己计算3D图
                        {
                            this.Chrome_help = get_theory_iostope();
                            if (new_peptide == null)
                                new_peptide = this.Dis_help.Psm_help.Pep;
                            this.function.GetEvidenceFromChrom(this.Chrome_help, this);
                        }
                        if (this.function.evi.ScanTime.Count == 0)
                            return;
                        AddAxis();
                        TrackMouse();

                        quant_init();
                        this.viewport3D.Children.Clear();
                        this.text_canvas.Children.Clear();
                        TextPoints.Clear();
                        GenerateBackgroundGrid();
                        AddAxis2();
                        ChangeAxisTextPosition();
                        ChangeTextPosition();
                        if ((bool)this.chrom_3D.IsChecked)
                            UseGeometry();
                        else
                            UseWirePolyLine();
                    }
                }
            }
            data_RD.Height = new GridLength(data_RD.ActualHeight + 1.0e-10, GridUnitType.Pixel);
        }

        private void initial_Peptide()
        {
            //this.other_psms_cbx.IsChecked = false;
            if (data_RD.ActualHeight != 0.0)
                data_RD.Height = new GridLength(data_RD.ActualHeight + 1.0e-10, GridUnitType.Pixel);
            if (task == null)
                return;
            //如果有定量文件，那么PSM列表中显示它们的ratio和sigma，否则，不进行显示
            if (task.has_ratio)
            {
                this.data_ratio_column.Visibility = Visibility.Visible;
                this.data_sigma_column.Visibility = Visibility.Visible;
                this.protein_ratio_column.Visibility = Visibility.Visible;
            }
            else
            {
                this.data_ratio_column.Visibility = Visibility.Collapsed;
                this.data_sigma_column.Visibility = Visibility.Collapsed;
                this.protein_ratio_column.Visibility = Visibility.Collapsed;
            }
            if (display_psms == null || display_psms.Count() == 0) //如果是用户手动选择了Peptide面板，则selected_psm为null，否则是由于用户点击Protein面板中的PSM列表选择的,selected_psm此时不为null
            {
                display_psms = new ObservableCollection<PSM>(psms);
                data.ItemsSource = display_psms;
                display_size.Text = display_psms.Count + "";
            }
            TabItem one = (TabItem)display_tab.Items[1];
            one.IsSelected = true; //显示下方的MS2
            this.Model2 = null;
        }

        //仅统计是Target的PSM，Decoy的比值一定是非数
        private List<double> read_Ratio(double min, double max)
        {
            min = Math.Log10(min) / Math.Log10(2.0);
            max = Math.Log10(max) / Math.Log10(2.0);
            List<double> all_ratios = new List<double>();
            for (int i = 0; i < psms.Count; ++i)
            {
                if (!psms[i].Is_target_flag)
                    continue;
                double ratio = Math.Log10(psms[i].Ratio) / Math.Log10(2.0);
                if (ratio >= min && ratio <= max)
                    all_ratios.Add(ratio);
                else if (ratio < min)
                    all_ratios.Add(min);
                else if (ratio > max)
                    all_ratios.Add(max);
            }
            return all_ratios;
        }
        private List<Scan_Raw> read_Scan_Mass_Error()
        {
            List<Scan_Raw> sr = new List<Scan_Raw>();
            try
            {
                for (int i = 0; i < title_hash.Count; ++i)
                {
                    sr.Add(new Scan_Raw());
                }
                for (int i = 0; i < psms.Count; ++i)
                {
                    int index = (int)title_hash[psms[i].Title.Split('.')[0]];
                    double mass_error_ppm = psms[i].Delta_mass * 1e6 / psms[i].Theory_sq_mass;
                    int scan = int.Parse(psms[i].Title.Split('.')[1]);
                    if (psms[i].Is_target_flag)
                        sr[index].Target_Scan_Raw_Simples.Add(new Scan_Raw_Simple(psms[i].Delta_mass, mass_error_ppm, scan, i));
                    else
                        sr[index].Decoy_Scan_Raw_Simples.Add(new Scan_Raw_Simple(psms[i].Delta_mass, mass_error_ppm, scan, i));
                }
                for (int i = 0; i < sr.Count; ++i)
                {
                    sr[i].Target_Scan_Raw_Simples.Sort();
                    sr[i].Decoy_Scan_Raw_Simples.Sort();
                }
            }
            catch (Exception exe)
            {
                //MessageBox.Show(exe.ToString());
            }
            return sr;
        }
        private List<MassError_Raw> read_Mass_Error() //根据更新的PSM列表来读取质量误差及分数信息，从而根据该信息来更新质量误差分布图
        {
            List<MassError_Raw> mer = new List<MassError_Raw>();
            try
            {
                for (int i = 0; i < title_hash.Count; ++i)
                {
                    mer.Add(new MassError_Raw());
                }
                for (int i = 0; i < psms.Count; ++i)
                {
                    if (title_hash[psms[i].Title.Split('.')[0]] == null)
                        continue;
                    int index = (int)title_hash[psms[i].Title.Split('.')[0]];
                    if (index >= mer.Count)
                        continue;
                    double mass_error_ppm = psms[i].Delta_mass * 1e6 / psms[i].Theory_sq_mass;
                    double score = -Math.Log10(psms[i].Score);
                    if (mer[index].Max_MassError_Da < Math.Abs(psms[i].Delta_mass))
                        mer[index].Max_MassError_Da = Math.Abs(psms[i].Delta_mass);
                    if (mer[index].Max_MassError_PPM < Math.Abs(mass_error_ppm))
                        mer[index].Max_MassError_PPM = Math.Abs(mass_error_ppm);
                    if (mer[index].Max_Score < score)
                        mer[index].Max_Score = score;
                    if (psms[i].Is_target_flag)
                    {
                        mer[index].Target_MassError_Scores.Add(new MassError_Simple(psms[i].Delta_mass, mass_error_ppm, score, i));
                    }
                    else
                    {
                        mer[index].Decoy_MassError_Scores.Add(new MassError_Simple(psms[i].Delta_mass, mass_error_ppm, score, i));
                    }
                }
            }
            catch (Exception exe)
            {
                //MessageBox.Show(exe.ToString());
            }
            return mer;
        }

        private void display()
        {
            if (summary_result_information == null) return;
            if (task == null)
                return;
            if (this.backgroundWorker != null && this.backgroundWorker.IsBusy)
                return;

            List<MassError_Raw> massError_raws = read_Mass_Error();
            //max_mass_error_score[0]:以Da为单位的最大误差，max_mass_error_score[1]：以ppm为单位的最大误差，max_mass_error_score[2]：最大分数
            //Delete the MassError Model
            List<BoxPlot_Help> boxplots = new List<BoxPlot_Help>();
            List<Scan_Raw> scan_raws = read_Scan_Mass_Error();
            for (int i = 0; i < massError_raws.Count; ++i)
            {
                List<double> target_mass_error_Da = massError_raws[i].get_target_da();
                if (target_mass_error_Da.Count != 0)
                    boxplots.Add(BoxPlot_Help.parse_boxplot(target_mass_error_Da));
            }
            dis_help = new Display_Help(boxplots, massError_raws, scan_raws, this);
            this.Model_MassError_Da = dis_help.display_boxplot("Da");

            boxplots = new List<BoxPlot_Help>();
            for (int i = 0; i < massError_raws.Count; ++i)
            {
                List<double> target_mass_error_PPM = massError_raws[i].get_target_ppm();
                if (target_mass_error_PPM.Count != 0)
                    boxplots.Add(BoxPlot_Help.parse_boxplot(target_mass_error_PPM));
            }
            dis_help = new Display_Help(boxplots, massError_raws, scan_raws, this);
            this.Model_MassError_PPM = dis_help.display_boxplot("PPM");



            //NEW
            List<double> target_score = new List<double>();
            List<double> decoy_score = new List<double>();
            double max_score = 0.0;
            for (int i = 0; i < massError_raws.Count; ++i)
            {
                MassError_Raw mer = massError_raws[i];
                for (int j = 0; j < mer.Target_MassError_Scores.Count; ++j)
                {
                    target_score.Add(mer.Target_MassError_Scores[j].Score);
                    if (max_score < mer.Target_MassError_Scores[j].Score)
                        max_score = mer.Target_MassError_Scores[j].Score;
                }
                for (int j = 0; j < mer.Decoy_MassError_Scores.Count; ++j)
                {
                    decoy_score.Add(mer.Decoy_MassError_Scores[j].Score);
                    if (max_score < mer.Decoy_MassError_Scores[j].Score)
                        max_score = mer.Decoy_MassError_Scores[j].Score;
                }
            }
            //上面的只有过FDR阈值的打分分布，decoy太少，基本只能看到target的分布
            //for (int i = 0; i < all_psms.Count; ++i)
            //{
            //    if (all_psms[i].Q_value >= 0.02)
            //        continue;
            //    double score = -Math.Log10(all_psms[i].Score);
            //    if (double.IsInfinity(score))
            //        continue;
            //    if (max_score < score)
            //        max_score = score;
            //    if (all_psms[i].Is_target_flag)
            //        target_score.Add(score);
            //    else
            //        decoy_score.Add(score);
            //}

            List<double> all_score_bins = new List<double>();
            List<int> target_score_bin_numbers = new List<int>();
            List<int> decoy_score_bin_numbers = new List<int>();

            double score_width = max_score / 49; //打分宽度,让最终形成50个bin
            double cur_score = 0.0;
            while (max_score > 0.0 && cur_score <= max_score)
            {
                all_score_bins.Add(cur_score);
                cur_score += score_width;
                target_score_bin_numbers.Add(0);
                decoy_score_bin_numbers.Add(0);
            }
            for (int i = 0; i < target_score.Count; ++i)
            {
                int index = (int)(target_score[i] / score_width);
                if (index < target_score_bin_numbers.Count)
                    ++target_score_bin_numbers[index];
            }
            for (int i = 0; i < decoy_score.Count; ++i)
            {
                int index = (int)(decoy_score[i] / score_width);
                if (index < decoy_score_bin_numbers.Count)
                    ++decoy_score_bin_numbers[index];
            }
            int max_number = 0;
            for (int i = 0; i < target_score_bin_numbers.Count; ++i)
            {
                if (max_number < target_score_bin_numbers[i])
                    max_number = target_score_bin_numbers[i];
            }
            for (int i = 0; i < decoy_score_bin_numbers.Count; ++i)
            {
                if (max_number < decoy_score_bin_numbers[i])
                    max_number = decoy_score_bin_numbers[i];
            }
            dis_help = new Display_Help(all_score_bins, target_score_bin_numbers, decoy_score_bin_numbers, max_number);
            this.Model_Score = dis_help.display_score();

            List<Mixed_Spectra> mixed_spectra_count = new List<Mixed_Spectra>();
            for (int i = 0; i < summary_result_information.mixed_spectra.Count; ++i)
            {
                mixed_spectra_count.Add(summary_result_information.mixed_spectra[i]);
            }
            dis_help = new Display_Help(mixed_spectra_count, this);
            this.Model_Mixed_Spectra = dis_help.display_mixed_spectra();

            List<double> Specific_percentage = new List<double>();
            Specific_percentage.Add(summary_result_information.Specific); // 
            Specific_percentage.Add(summary_result_information.C_term_specific);
            Specific_percentage.Add(summary_result_information.N_term_specific);
            Specific_percentage.Add(summary_result_information.Non_specific);
            dis_help = new Display_Help(Specific_percentage, this);
            this.Model_Specific = dis_help.display_specific();

            const int modification_top = 10;
            List<Identification_Modification> modifications = new List<Identification_Modification>();
            for (int i = 0; i < modification_top && i < summary_result_information.modifications.Count; ++i)
            {
                modifications.Add(summary_result_information.modifications[i]);
            }
            dis_help = new Display_Help(modifications, this);
            this.Model_Modification = dis_help.display_modification();

            dis_help = new Display_Help(summary_result_information.length_distribution, this);
            this.Model_Length = dis_help.display_length();

            dis_help = new Display_Help(summary_result_information.missed_cleavage_distribution, this);
            this.Model_Missed_Cleavage = dis_help.display_missed_cleavage();

            dis_help = new Display_Help(summary_result_information.raw_rates, this);
            this.Model_RawRate = dis_help.display_raw_rate();
            if (summary_result_information.raw_rates.Count >= 10)
            {
                Grid.SetColumnSpan(this.summary_raw_rate_grid, 2);
                Grid.SetRow(this.summary_quantification_grid, Grid.GetRow(this.summary_quantification_grid) + 1);
                Grid.SetColumn(this.summary_quantification_grid, 0);
            }

            List<double> all_ratio_bins = new List<double>();
            List<int> all_ratio_bin_numbers = new List<int>();
            double ratio_min_t = -10;
            double ratio_max_t = 10;
            List<double> all_ratios = read_Ratio(Math.Pow(2.0, ratio_min_t), Math.Pow(2.0, ratio_max_t));
            if (all_ratios.Count != 0)
            {
                all_ratios.Sort();
                double ratio_width = (ratio_max_t - ratio_min_t) / 40;
                double cur_ratio = ratio_min_t;
                while (cur_ratio <= ratio_max_t)
                {
                    all_ratio_bins.Add(cur_ratio);
                    cur_ratio += ratio_width;
                    all_ratio_bin_numbers.Add(0);
                }
                for (int i = 0; i < all_ratios.Count; ++i)
                {
                    int index = (int)((all_ratios[i] - ratio_min_t) / ratio_width);
                    if (index < all_ratio_bin_numbers.Count)
                        ++all_ratio_bin_numbers[index];
                }
                max_number = 0;
                for (int i = 0; i < all_ratio_bin_numbers.Count; ++i)
                {
                    if (max_number < all_ratio_bin_numbers[i])
                        max_number = all_ratio_bin_numbers[i];
                }
                dis_help = new Display_Help(all_ratio_bins, all_ratio_bin_numbers, max_number);
                this.Model_Quantification = dis_help.display_quantification();
            }
            int[] FDR_bin_count = get_FDR_bin();
            dis_help = new Display_Help(FDR_bin_count);
            this.Model_FDR = dis_help.display_FDR(Config_Help.fdr_value);
            LineAnnotation lineAnnotation3 = this.Model_FDR.Annotations.Last() as LineAnnotation;
            lineAnnotation3.MouseUp += (s, e) =>
            {
                TextBox tb0 = this.fdr_btn.Children[this.fdr_btn.Children.Count - 2] as TextBox;
                tb0.Text = lineAnnotation3.Y.ToString("P2");
                lineAnnotation3.StrokeThickness /= 3;
                lineAnnotation3.FontSize -= 4.0;
                this.Model_FDR.RefreshPlot(false);
                e.Handled = true;
            };
            if (this.fdr_btn.Children.Count != 0)
                return;
            TextBox tb = null;
            for (int i = 0; i < 3; ++i)
            {
                Button btn = new Button();
                switch (i)
                {
                    case 0:
                        btn.Content = "1.00%";
                        break;
                    case 1:
                        btn.Content = "2.00%";
                        break;
                    case 2:
                        btn.Content = "5.00%";
                        break;
                }
                btn.Margin = new Thickness(10);
                btn.Click += (s, e) =>
                {
                    string[] strs = ((string)btn.Content).Split('%');
                    double fdr_ratio = double.Parse(strs[0]) * 0.01;
                    update_FDR_Line(fdr_ratio);
                    if (tb != null)
                    {
                        tb.Text = (string)btn.Content;
                    }
                };
                this.fdr_btn.Children.Add(btn);
            }
            tb = new TextBox();
            tb.Text = Config_Help.fdr_value.ToString("P2");
            tb.Margin = new Thickness(10);
            tb.PreviewMouseLeftButtonUp += (s, e) =>
            {
                int precent_index = tb.Text.IndexOf('%');
                if (precent_index != -1)
                {
                    tb.Select(0, precent_index);
                }
            };
            Button filter_btn = new Button();
            tb.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    string[] strs = (tb.Text).Split('%');
                    if (strs.Length == 2) //用户输入的FDR值必须以百分号结尾
                    {
                        try
                        {
                            this.other_psms_cbx.IsChecked = false;
                            filter_btn.IsEnabled = false;
                            double fdr_ratio = double.Parse(strs[0]) * 0.01;
                            //update_FDR_Line(fdr_ratio); //通过输入Enter键来将TextBlock的值更新图中的线
                            Config_Help.fdr_value = fdr_ratio;
                            this.progressBar.Value = 0;
                            this.progressBar.Visibility = Visibility.Visible;
                            protein_panel = null;
                            this.Cursor = Cursors.Wait;
                            backgroundWorker_fdr.RunWorkerAsync(Config_Help.fdr_value);
                        }
                        catch (Exception exe)
                        {
                            MessageBox.Show(Message_Help.FDR_BE_NUMBER + "\r\n" + exe.ToString());
                        }
                    }
                    else
                    {
                        MessageBox.Show(Message_Help.FDR_END_BY_PERCENT);
                    }
                }
            };
            this.fdr_btn.Children.Add(tb);
            filter_btn.Content = "  Filter  ";
            filter_btn.Margin = new Thickness(20);
            filter_btn.Click += (s, e) =>
            {
                string[] strs = (tb.Text).Split('%');
                if (strs.Length == 2) //用户输入的FDR值必须以百分号结尾
                {
                    try
                    {
                        this.other_psms_cbx.IsChecked = false;
                        filter_btn.IsEnabled = false;
                        double fdr_ratio = double.Parse(strs[0]) * 0.01;
                        //update_FDR_Line(fdr_ratio); //通过输入Enter键来将TextBlock的值更新图中的线
                        Config_Help.fdr_value = fdr_ratio;
                        this.progressBar.Value = 0;
                        this.progressBar.Visibility = Visibility.Visible;
                        protein_panel = null;
                        this.Cursor = Cursors.Wait;
                        backgroundWorker_fdr.RunWorkerAsync(Config_Help.fdr_value);
                    }
                    catch (Exception exe)
                    {
                        MessageBox.Show(Message_Help.FDR_BE_NUMBER + "\r\n" + exe.ToString());
                    }
                }
                else
                {
                    MessageBox.Show(Message_Help.FDR_END_BY_PERCENT);
                }
            };
            this.fdr_btn.Children.Add(filter_btn);
            //this.DataContext = this;
        }

        private void update_FDR_Line(double fdr_ratio)
        {
            LineAnnotation fdr_line = (LineAnnotation)this.Model_FDR.Annotations[0];
            if (fdr_ratio <= 0.045)
                fdr_line.TextVerticalAlignment = OxyPlot.VerticalAlignment.Bottom;
            else
                fdr_line.TextVerticalAlignment = OxyPlot.VerticalAlignment.Top;
            LineSeries fdr_series = (LineSeries)this.Model_FDR.Series[0];
            fdr_line.Y = fdr_ratio;
            int index = (int)((fdr_line.Y - 0.005 * 0.01) / (0.01 * 0.01)) + 1;
            if (index < 0)
                index = 0;
            if (index >= fdr_series.Points.Count)
                index = fdr_series.Points.Count - 1;
            fdr_line.Text = fdr_line.Y.ToString("P2") + ":" + fdr_series.Points[index].X.ToString("N0");
            this.Model_FDR.RefreshPlot(true);
        }
        private void initial_Summary() //初始化Summary面板
        {
            selected_psm = null;
            read_summary_txt(); //读取Summary中的统计信息
            display(); //画这些图
            if (!backgroudWorker_cand_flag)
            {
                if (backgroundWorker_cand != null)
                {
                    backgroundWorker_cand.RunWorkerAsync();
                }
                //if (backgroundWorker_filter != null) //不用生成filter的六个文件，如果结果过大，速度太慢
                //{
                //    backgroundWorker_filter.RunWorkerAsync();
                //}
                backgroudWorker_cand_flag = true;
            }
        }
        private void read_Summary_Quant(ref Summary_Result_Information summary_result_information) //log2的结果
        {
            if (psms.Count == 0)
                return;
            List<double> all_ratios = new List<double>();
            for (int i = 0; i < psms.Count; ++i)
            {
                if (!psms[i].Is_target_flag)
                    continue;
                bool is_con = psms[i].is_Contaminant();
                if (is_con)
                    continue;
                all_ratios.Add(Math.Log10(psms[i].Ratio) / Math.Log10(2.0));
            }
            all_ratios.Sort();
            int start = 0;
            while (start < all_ratios.Count && all_ratios[start] <= Math.Log10(0.001) / Math.Log10(2.0))
                ++start;
            int end = all_ratios.Count - 1;
            while (end >= 0 && all_ratios[end] >= Math.Log10(1024.0) / Math.Log10(2.0))
                --end;
            int nan_number = all_ratios.Count - (end - start + 1);
            double nan_ratio = nan_number * 1.0 / all_ratios.Count;
            summary_result_information.NaN_number = nan_ratio.ToString("P2") + " (" + nan_number.ToString("N0") + "/" + all_ratios.Count.ToString("N0") + ")";
            double mean_value = 0.0, deviation = 0.0;
            for (int i = start; i <= end; ++i)
            {
                mean_value += all_ratios[i];
            }
            mean_value /= (end - start + 1);
            summary_result_information.Mean = Math.Pow(2.0, mean_value);
            summary_result_information.Median = Math.Pow(2.0, all_ratios[(start + end) / 2]);

            for (int i = start; i <= end; ++i)
            {
                deviation += (all_ratios[i] - mean_value) * (all_ratios[i] - mean_value);
            }
            deviation = Math.Sqrt(deviation / (end - start + 1));
            summary_result_information.Standard_Deviation = deviation;
        }
        private void read_summary_txt(string summary_file = "")
        {
            if (task == null)
                return;
            if (summary_file == "")
                summary_file = task.summary_file;
            File_Help.read_Summary_TXT(summary_file, ref summary_result_information); //更新定性的结果
            if (summary_result_information == null) return;
            double meanPreMe = 0.0, stdPreMe = 0.0;
            for (int i = 0; i < psms.Count; ++i)
            {
                meanPreMe += psms[i].Delta_mass_PPM;
            }
            meanPreMe /= psms.Count;
            for (int i = 0; i < psms.Count; ++i)
            {
                stdPreMe += (psms[i].Delta_mass_PPM - meanPreMe) * (psms[i].Delta_mass_PPM - meanPreMe);
            }
            stdPreMe = Math.Sqrt(stdPreMe / psms.Count);
            summary_result_information.meanPreMe = meanPreMe;
            summary_result_information.stdPreMe = stdPreMe;
            if (task.has_ratio) //统计定量的比值信息，包括：非数比值个数、均值、中位数和标准差
            {
                this.summary_quantification_grid.Visibility = Visibility.Visible;
                read_Summary_Quant(ref summary_result_information);
            }
            else
            {
                this.summary_quantification_grid.Visibility = Visibility.Collapsed;
            }
            File_Help.read_pFind_PFD(task.pFind_param_file, ref summary_param_information);
            this.psm_help = new PSM_Help(task.HCD_ETD_Type);
            File_Help.read_pParse_PARA(task.pParse_param_file, ref summary_param_information);
            string msms_tolerance = summary_param_information.msms_tolerance;
            if (msms_tolerance.EndsWith("ppm"))
            {
                this.ms2_quant_help.Ppm_mass_error = double.Parse(msms_tolerance.Substring(0, msms_tolerance.Length - 3)) * 1e-6;
            }
            else
            {
                this.ms2_quant_help.Da_mass_error = double.Parse(msms_tolerance.Substring(0, msms_tolerance.Length - 2));
            }
            //检查pFind.cfg中给定的数据库存不存在，及对应的RAW路径存不存在
            if (!File.Exists(summary_param_information.fasta_path))
            {
                MessageBox.Show(Message_Help.FASTA_PATH_NOT_EXIST0 + "'" + summary_param_information.fasta_path +
                    "'. " + Message_Help.FASTA_PATH_NOT_EXIST1);
                while (true)
                {
                    System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
                    ofd.Filter = "fasta files (*.fasta)|*.fasta";
                    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string[] str1 = summary_param_information.fasta_path.Split('\\');
                        string[] str2 = ofd.FileName.Split('\\');
                        if (str1.Last() != str2.Last())
                        {
                            MessageBox.Show(Message_Help.FASTA_PATH_NOT_EQUAL + "\nThe old fasta name is '" +
                                str1.Last() + "' , but new fasta name is '" + str2.Last() + "' .");
                            continue;
                        }
                        File_Help.update_pfind_cfg(task.pFind_param_file, summary_param_information.fasta_path, ofd.FileName);
                        summary_param_information.fasta_path = ofd.FileName;
                        task.fasta_file_path = summary_param_information.fasta_path;
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (summary_param_information.raws_path.Count > 0 &&
                !File.Exists(summary_param_information.raws_path[0]))
            {
                int str_index = summary_param_information.raws_path[0].LastIndexOf('\\');
                string raw_folder_path = summary_param_information.raws_path[0].Substring(0, str_index);
                MessageBox.Show(Message_Help.RAW_PATH_NOT_EXIST0 + "'" + raw_folder_path +
                    "'. " + Message_Help.RAW_PATH_NOT＿EXIST1);
                System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string path = fbd.SelectedPath;
                    for (int i = 0; i < summary_param_information.raws_path.Count; ++i)
                    {
                        string raw_name = summary_param_information.raws_path[i].Split('\\').Last();
                        File_Help.update_pfind_cfg(task.pFind_param_file, summary_param_information.raws_path[i], path + "\\" + raw_name);
                        summary_param_information.raws_path[i] = path + "\\" + raw_name;
                    }
                    for (int i = 0; i < summary_param_information.raws_path.Count; ++i)
                    {
                        int index_0 = summary_param_information.raws_path[i].LastIndexOf('\\');
                        string raw_name = summary_param_information.raws_path[i].Substring(index_0 + 1);
                        int index_1 = raw_name.LastIndexOf('_');
                        raw_name = raw_name.Substring(0, index_1);
                        task.title_PF2_Path[raw_name] = summary_param_information.raws_path[i];
                    }
                    ms2_scan_hash.Clear();
                    ms1_scan_hash.Clear();
                    for (int i = 0; i < summary_param_information.raws_path.Count; ++i)
                    {
                        string raw_name = summary_param_information.raws_path[i].Split('\\').Last(); //已经切除掉其后缀名.raw
                        int index_1 = raw_name.LastIndexOf('_');
                        raw_name = raw_name.Substring(0, index_1);
                        string ms2_pfindex_file = task.get_pf2Index_path(raw_name);
                        string ms1_pfindex_file = task.get_pf1Index_path(raw_name);
                        ms2_scan_hash.Add(File_Help.read_ms2_index(ms2_pfindex_file)); //读取二级谱图的索引，扫描号对应于索引文件的位置，可以获得谱图信息
                        ms1_scan_hash.Add(File_Help.read_ms1_index(ms1_pfindex_file, ref Min_scan, ref Max_scan)); //读取一级谱图的索引
                    }
                }
            }
            summary_param_information.ll_info_label = "None"; //如果使用pQuant，即没有pQuant的参数文件，那么这个设置成None
            if (task.pQuant_param_file != "")
            {
                File_Help.read_pQuant_QNT(task.pQuant_param_file, ref summary_param_information);
            }
            //将aa的路径放入到summary_param_information中
            for (int i = 0; i < task.all_aa_files.Count; ++i)
            {
                summary_param_information.aas_path.Add(task.all_aa_files[i]);
            }

            //显示results信息
            #region
            ObservableCollection<_Result> results_report = new ObservableCollection<_Result>();
            string Peptide_level = "Peptide Level:";
            string Cleavage_level = "Cleavage:";
            string Quantification_level = "Quantitation:";
            string Modifications_level = "Modifications:";
            string Missed_Cleavage_level = "Missed Cleavage:";
            string Mixed_Spectra_level = "Mixed Spectra:";
            string Charge_level = "Charge:";
            string Precusor_MassError = "MassError:";
            string RawRate_level = "ID Rate:";

            results_report.Add(new _Result(Peptide_level, ""));
            bool is_first = true;
            foreach (var prop in summary_result_information.GetType().GetProperties())
            {
                if (prop.Name.ToLower().Contains("specific"))
                {
                    if (is_first)
                    {
                        is_first = false;
                        results_report.Add(new _Result(Cleavage_level, ""));
                        results_report.Add(new _Result("Specific", summary_result_information.Specific.ToString("P2")
                            + " (" + summary_result_information.Specific_number.ToString("N0") + "/" + summary_result_information.peptides_number.ToString("N0") + ")"));
                        results_report.Add(new _Result("C-term specific", summary_result_information.C_term_specific.ToString("P2")
                            + " (" + summary_result_information.C_term_specific_number.ToString("N0") + "/" + summary_result_information.peptides_number.ToString("N0") + ")"));
                        results_report.Add(new _Result("N-term specific", summary_result_information.N_term_specific.ToString("P2")
                            + " (" + summary_result_information.N_term_specific_number.ToString("N0") + "/" + summary_result_information.peptides_number.ToString("N0") + ")"));
                        results_report.Add(new _Result("Non specific", summary_result_information.Non_specific.ToString("P2")
                            + " (" + summary_result_information.Non_specific_number.ToString("N0") + "/" + summary_result_information.peptides_number.ToString("N0") + ")"));
                    }
                    continue;
                }
                else if (task.has_ratio && prop.Name == "NaN_number")
                {
                    results_report.Add(new _Result(Quantification_level, ""));
                }
                bool is_quant_flag = false;
                if (prop.Name == "NaN_number" || prop.Name == "Mean" || prop.Name == "Median" || prop.Name == "Standard_Deviation")
                    is_quant_flag = true;
                if (!task.has_ratio && is_quant_flag)
                    continue;
                string param_name = Config_Help.delete_underline(prop.Name);
                if (param_name == "NaN number")
                    param_name = "NaN number (no contaminants)";
                if (prop.PropertyType == typeof(double))
                {
                    if (is_quant_flag)
                        results_report.Add(new _Result(param_name, String.Format("{0:F2}", prop.GetValue(summary_result_information, null))));
                    else
                        results_report.Add(new _Result(param_name, String.Format("{0:P2}", prop.GetValue(summary_result_information, null))));
                }
                else
                    results_report.Add(new _Result(param_name, String.Format("{0:N0}", prop.GetValue(summary_result_information, null))));
            }
            results_report.Add(new _Result(Modifications_level, ""));
            for (int i = 0; i < summary_result_information.modifications.Count && i < 10; ++i)
            {
                results_report.Add(new _Result(summary_result_information.modifications[i].modification_name,
                    summary_result_information.modifications[i].mod_spectra_percentage.ToString("P2") + " (" + summary_result_information.modifications[i].mod_spectra_count.ToString("N0") + "/" + summary_result_information.peptides_number.ToString("N0") + ")"));
            }
            results_report.Add(new _Result(Missed_Cleavage_level, ""));
            for (int i = 0; i < summary_result_information.missed_cleavage_distribution.Count; ++i)
            {
                results_report.Add(new _Result("number=" + summary_result_information.missed_cleavage_distribution[i].missed_number.ToString(),
                    summary_result_information.missed_cleavage_distribution[i].ratio.ToString("P2") + " (" + summary_result_information.missed_cleavage_distribution[i].num.ToString("N0") + "/" + summary_result_information.peptides_number.ToString("N0") + ")"));
            }
            results_report.Add(new _Result(Mixed_Spectra_level, ""));
            for (int i = 0; i < summary_result_information.mixed_spectra.Count; ++i)
            {
                results_report.Add(new _Result("number=" + summary_result_information.mixed_spectra[i].mixed_spectra_num.ToString(),
                    summary_result_information.mixed_spectra[i].ratio.ToString("P2") + " (" + summary_result_information.mixed_spectra[i].num.ToString("N0") + "/" + summary_result_information.scans_number.ToString("N0") + ")"));
            }
            results_report.Add(new _Result(Charge_level, ""));
            for (int i = 0; i < summary_result_information.charge_distribution.Count; ++i)
            {
                results_report.Add(new _Result("charge=" + summary_result_information.charge_distribution[i].charge.ToString(),
                    summary_result_information.charge_distribution[i].ratio.ToString("P2") + " (" + summary_result_information.charge_distribution[i].num.ToString("N0") + "/" + summary_result_information.peptides_number.ToString("N0") + ")"));
            }
            results_report.Add(new _Result(Precusor_MassError, ""));
            results_report.Add(new _Result("Precusor mass error: (mean)", summary_result_information.meanPreMe.ToString("F2") + "ppm"));
            results_report.Add(new _Result("Precusor mass error: (std)", "±" + summary_result_information.stdPreMe.ToString("F2") + "ppm"));
            results_report.Add(new _Result(RawRate_level, ""));
            for (int i = 0; i < summary_result_information.raw_rates.Count; ++i)
            {
                results_report.Add(new _Result(summary_result_information.raw_rates[i].Raw_name, String.Format("{0:P2}", summary_result_information.raw_rates[i].Rate) +
                    " (" + summary_result_information.raw_rates[i].Identification_num.ToString("N0") + "/" + summary_result_information.raw_rates[i].All_num.ToString("N0") + ")"));
            }
            this.resultReport.ItemsSource = results_report;
            #endregion
            //显示Param参数
            ObservableCollection<_Param> params_report = new ObservableCollection<_Param>();
            params_report.Add(new _Param("Param:", ""));
            foreach (var prop in summary_param_information.GetType().GetProperties())
            {
                if ((prop.Name == "chrom_tolerance" || prop.Name == "label_efficiency") && task.quantification_file == "")
                {
                    continue;
                }
                if (prop.Name == "modification_path")
                {
                    params_report.Add(new _Param("File:", ""));
                    for (int i = 0; i < summary_param_information.aas_path.Count; ++i)
                    {
                        params_report.Add(new _Param("aa path " + (i + 1), summary_param_information.aas_path[i]));
                    }
                }
                else if (prop.Name == "label_efficiency")
                {
                    params_report.Add(new _Param(Config_Help.delete_underline(prop.Name), (string)prop.GetValue(summary_param_information, null)));
                }
                if (prop.Name != "label_efficiency")
                {
                    if(prop.GetValue(summary_param_information) != null && prop.GetValue(summary_param_information) != "")
                    {
                    string param_name = Config_Help.delete_underline(prop.Name);
                    params_report.Add(new _Param(param_name, String.Format("{0:N0}", prop.GetValue(summary_param_information, null))));
                        }
                }
            }
            params_report.Add(new _Param("Raws:", ""));
            for (int i = 0; i < summary_param_information.raws_path.Count; ++i)
            {
                //使用PF的路径来解析出RAW的路径，因为PF文件和RAW文件在同一目录下，只是名字及拓展名不一致
                string pf_path = summary_param_information.raws_path[i];
                int last_index = pf_path.LastIndexOf('_');
                string raw_path = "";
                if (last_index >= 0)
                {
                    raw_path = pf_path.Substring(0, last_index) + ".raw";
                    if (!isnot_mgf)
                        raw_path = pf_path.Substring(0, last_index) + ".mgf";
                }
                else
                {
                    raw_path = pf_path.Replace(".pf2", ".raw");
                    if (!isnot_mgf)
                        raw_path = pf_path.Replace(".pf2", ".mgf");
                }
                params_report.Add(new _Param("raw path " + (i + 1), raw_path));
            }
            this.paramReport.ItemsSource = params_report;

            //更新value_column的宽度
            var summary_gridView = resultReport.View as GridView;
            if (summary_gridView == null || summary_gridView.Columns.Count < 1)
                return;
            double max_value_width = 0.0;
            for (int i = 0; i < results_report.Count; ++i)
            {
                string value_str = results_report[i]._property;
                if (value_str == null)
                    continue;
                FormattedText ft = new FormattedText(value_str, System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight, new Typeface(this.resultReport.FontFamily.ToString()), this.resultReport.FontSize, Brushes.Black);
                if (ft.WidthIncludingTrailingWhitespace > max_value_width)
                {
                    max_value_width = ft.WidthIncludingTrailingWhitespace;
                }
            }
            summary_gridView.Columns[0].Width = max_value_width + 30;
            max_value_width = 0.0;
            for (int i = 0; i < results_report.Count; ++i)
            {
                string value_str = results_report[i]._value;
                if (value_str == null)
                    continue;
                FormattedText ft = new FormattedText(value_str, System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight, new Typeface(this.resultReport.FontFamily.ToString()), this.resultReport.FontSize, Brushes.Black);
                if (ft.WidthIncludingTrailingWhitespace > max_value_width)
                {
                    max_value_width = ft.WidthIncludingTrailingWhitespace;
                }
            }
            summary_gridView.Columns[1].Width = max_value_width + 30;

            var summary_gridView2 = paramReport.View as GridView;
            if (summary_gridView2 == null || summary_gridView2.Columns.Count < 1)
                return;
            max_value_width = 0.0;
            for (int i = 0; i < params_report.Count; ++i)
            {
                string value_str = params_report[i]._property;
                if (value_str == null)
                    continue;
                FormattedText ft = new FormattedText(value_str, System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight, new Typeface(this.paramReport.FontFamily.ToString()), this.paramReport.FontSize, Brushes.Black);
                if (ft.WidthIncludingTrailingWhitespace > max_value_width)
                {
                    max_value_width = ft.WidthIncludingTrailingWhitespace;
                }
            }
            summary_gridView2.Columns[0].Width = max_value_width + 30;
            max_value_width = 0.0;
            for (int i = 0; i < params_report.Count; ++i)
            {
                string value_str = params_report[i]._value;
                if (value_str == null)
                    continue;
                FormattedText ft = new FormattedText(value_str, System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight, new Typeface(this.paramReport.FontFamily.ToString()), this.paramReport.FontSize, Brushes.Black);
                if (ft.WidthIncludingTrailingWhitespace > max_value_width)
                {
                    max_value_width = ft.WidthIncludingTrailingWhitespace;
                }
            }
            summary_gridView2.Columns[1].Width = max_value_width + 30;
        }
        private void clk(object sender, SelectionChangedEventArgs e)
        {
            if (task == null)
                return;
            TreeViewItem selected_treeViewItem = this.task_treeView.SelectedItem as TreeViewItem;
            if (selected_treeViewItem != null && task_flag == "pTop")
            {
                if (selected_treeViewItem.Header is string)
                    selected_treeViewItem = selected_treeViewItem.Parent as TreeViewItem;
                TreeViewItem selected_treeViewItem_children = selected_treeViewItem.Items[this.summary_tab.SelectedIndex] as TreeViewItem;
                selected_treeViewItem_children.IsSelected = true;
            }
            if (e.OriginalSource == summary_tab)
            {
                if (data != null && data.Items.Count == 1)
                {
                    data.SelectedItem = null;
                    data.SelectedItems.Clear();
                }
                if (summary_tab.SelectedIndex == 0) //Summary面板
                {
                    if (this.task_flag != "pTop")
                        return;
                    this.Cursor = Cursors.Wait;
                    initial_Summary();
                    this.Cursor = null;
                }
                else if (summary_tab.SelectedIndex == 1) //Peptide面板
                {
                    this.Cursor = Cursors.Wait;
                    initial_Peptide();
                    display_size.Text = display_psms.Count().ToString();
                    this.Cursor = null;
                }
                else if (summary_tab.SelectedIndex == 2) //Protein面板
                {
                    if (this.task_flag != "pTop")
                        return;
                    this.Cursor = Cursors.Wait;
                    initial_Protein();
                    if (protein_panel != null)
                        display_size.Text = protein_panel.identification_proteins.Count().ToString();
                    this.Cursor = null;
                }
            }
        }
        private int protein_group_number()
        {
            int res = 0;
            for (int i = 0; i < protein_panel.identification_proteins.Count; ++i)
            {
                if (protein_panel.identification_proteins[i].Same_Sub_Flag == null || protein_panel.identification_proteins[i].Same_Sub_Flag == "")
                    ++res;
            }
            return res;
        }
        public void initial_Protein()
        {
            if (task == null || task_flag != "pTop")
                return;
            //if (selected_protein == null)
            if (protein_panel == null || protein_panel.identification_proteins == null ||
                protein_panel.identification_proteins.Count == 0)
            {
                System.Collections.Hashtable AC_Protein_index = new Hashtable();
                File_Help.read_protein(task.fasta_file_path, psms, ref protein_panel, ref AC_Protein_index);
                if (protein_panel == null || AC_Protein_index == null)
                    return;
                File_Help.read_protein_file(task.identification_protein_file, AC_Protein_index, ref protein_panel.identification_proteins);
                protein_panel.identification_protein_groups = Protein_Help.update_group(protein_panel.identification_proteins);
                Protein_Ratio_Help.compute_protein_ratio(protein_panel.identification_proteins, psms);
                ObservableCollection<Protein> show_proteins = protein_panel.identification_proteins;
                show_proteins = new ObservableCollection<Protein>(show_proteins.OrderByDescending(a => a.psm_index.Count));
                for (int i = 0; i < show_proteins.Count; ++i)
                {
                    show_proteins[i].ID = i + 1;
                }
                protein_data.ItemsSource = show_proteins;
                this.protein_group_data.ItemsSource = protein_panel.identification_protein_groups;
                display_size.Text = show_proteins.Count.ToString();
            }
            this.group_size.Text = protein_group_number() + "";
            if (this.data_ms2_ratio_column.Visibility == Visibility.Visible) //MS2的ratio显示了，那么切换到蛋白的时候，会计算蛋白的MS2定量比值，并进行显示
            {
                this.data_protein_ratio_column.Visibility = Visibility.Visible;
                this.data_protein_ratio_column2.Visibility = Visibility.Visible;
                Protein_Help.update_MS2_ratio(this.protein_panel.identification_proteins, psms);
            }
        }

        private void s_ch_Pep(object sender, SelectionChangedEventArgs e)
        {

        }


        private void Help_About_Clk(object sender, RoutedEventArgs e)
        {
            About about_dialog = new About();
            about_dialog.Show();
            //Process.Start("http://pfind.ict.ac.cn");
            //MessageBox.Show("By pfinder. If you want more software and informations, please Enter this website http://pfind.ict.ac.cn");
        }
        bool IsInnerChange = false;
        private void display_ms1Or2(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource == display_tab)
            {
                if (IsInnerChange)
                {
                    IsInnerChange = false;
                    return;
                }
                if (display_tab.SelectedIndex == 0) //MS1
                {
                    if (!isnot_mgf)
                    {
                        MessageBox.Show(Message_Help.NO_DATA);
                        IsInnerChange = true;
                        display_tab.SelectedIndex = 0;
                        return;
                    }
                    if (selected_ms1 == null || selected_psm == null)
                    {
                        this.Model1 = null;
                        return;
                    }
                    update_ms1_mz_tbx();
                    dis_help = new Display_Help(selected_ms1, this.dis_help.Psm_help, this.dis_help.ddh, dis_help.ddhms1);
                    this.DataContext = this;
                    double ms1_pepmass = 0.0;
                    int frag_index = -1;
                    if (task.quantification_file == "")
                        this.Model1 = dis_help.display_ms1(selected_ms1.Pepmass_ms2 - 3, selected_ms1.Pepmass_ms2 + 3, ref ms1_pepmass, ref frag_index);
                    else
                    {
                        string[] strs = selected_psm.Title.Split('.');
                        int scan = int.Parse(strs[1]);
                        int pParse_num = int.Parse(strs[4]);
                        List<string> list = new List<string>();
                        if (qut_scan_hash[(int)(title_hash[selected_psm.Title.Split('.')[0]]) + "." + scan + "." + pParse_num] != null)
                        {
                            File_Help.read_3D_List(task.quantification_file, osType,
                                (int)qut_scan_hash[(int)(title_hash[selected_psm.Title.Split('.')[0]]) + "." + scan + "." + pParse_num], start, end, ref list);
                            this.function = new Getlist();
                            this.function.GetEvidenceFromList(list);
                            double min_mz = this.function.evi.TheoricalIsotopicMZList.First().First() - 2.5;
                            double max_mz = this.function.evi.TheoricalIsotopicMZList.Last().Last() + 2.5;
                            this.Model1 = dis_help.display_ms1(min_mz, max_mz, ref ms1_pepmass, ref frag_index);
                        }
                        else
                        {
                            this.Model1 = dis_help.display_ms1(selected_ms1.Pepmass_ms2 - 3, selected_ms1.Pepmass_ms2 + 3, ref ms1_pepmass, ref frag_index);
                        }

                    }
                    if (this.Model1 == null)
                        return;
                    this.Model1.Axes[1].Reset();
                    this.Model1.Axes[1].AxisChanged += (s, er) =>
                    {
                        window_sizeChg_Or_ZommPan_ms1();
                    };
                    selected_ms1.Pepmass = ms1_pepmass;
                    selected_ms1.Fragment_index = frag_index;

                    if (this.Model1 != null)
                    {
                        this.Model1.RefreshPlot(true);
                        data_RD.Height = new GridLength(data_RD.ActualHeight + 1.0e-10, GridUnitType.Pixel);
                    }
                }
                else if (display_tab.SelectedIndex == 2) //Chromatogram
                {
                    if (task_flag != "pTop" && task_flag != "pLink")
                        return;
                    if (!isnot_mgf)
                    {
                        MessageBox.Show(Message_Help.NO_DATA);
                        IsInnerChange = true;
                        display_tab.SelectedIndex = 2;
                        return;
                    }
                    if (timer != null)
                        timer.Stop();
                    if (selected_psm != null)
                    {
                        string[] strs = selected_psm.Title.Split('.');
                        int scan = int.Parse(strs[1]);
                        int pParse_num = int.Parse(strs[4]);
                        List<string> list = new List<string>();
                        this.function = new Getlist();
                        if (qut_scan_hash[(int)(title_hash[selected_psm.Title.Split('.')[0]]) + "." + scan + "." + pParse_num] != null &&
                            this.new_peptide == null)
                        {
                            File_Help.read_3D_List(task.quantification_file, osType,
                                (int)qut_scan_hash[(int)(title_hash[selected_psm.Title.Split('.')[0]]) + "." + scan + "." + pParse_num], start, end, ref list);
                            this.function.GetEvidenceFromList(list);
                        }
                        else //如果定量文件没有，或者定量文件中不存在该PSM的结果，那么需要根据emass自己计算3D图
                        {
                            this.Chrome_help = get_theory_iostope();
                            if (new_peptide == null)
                                new_peptide = this.Dis_help.Psm_help.Pep;
                            this.function.GetEvidenceFromChrom(this.Chrome_help, this);
                        }
                        if (this.function.evi.ScanTime.Count == 0)
                            return;
                        AddAxis();
                        TrackMouse();

                        quant_init();
                        this.viewport3D.Children.Clear();
                        this.text_canvas.Children.Clear();
                        TextPoints.Clear();
                        GenerateBackgroundGrid();
                        AddAxis2();
                        ChangeAxisTextPosition();
                        ChangeTextPosition();
                        if ((bool)this.chrom_3D.IsChecked)
                            UseGeometry();
                        else
                            UseWirePolyLine();
                    }
                }
                else if (display_tab.SelectedIndex == 1)
                {
                    if (timer != null)
                        timer.Stop();
                    if (selected_psm != null)
                    {
                        int pep_count = dis_help.Psm_help.Peptide_Number;
                        this.Model2 = dis_help.display_ms2(pep_count);
                        if (this.Model2 == null)
                            return;
                        //dis_help.Psm_help.Mix_Flag = 0; //切到MS2对二级谱的显示均是以单谱显示，而不能以混合谱图进行显示
                        MS2_Help ms2_help = new MS2_Help(Model2, dis_help, Ladder_Help.Scale_Width, Ladder_Help.Scale_Height, this.frame_right.ActualWidth, this.display_tab.ActualHeight, this);
                        this.Model2.Axes[1].Reset();
                        this.Model2.Axes[3].Reset();
                        this.Model2.Axes[1].AxisChanged += (s, er) =>
                        {
                            ms2_help = new MS2_Help(Model2, dis_help, Ladder_Help.Scale_Width, Ladder_Help.Scale_Height, this.frame_right.ActualWidth, this.display_tab.ActualHeight, this);
                            ms2_help.window_sizeChg_Or_ZoomPan();
                        };
                        if (selected_psm != null)
                            ms2_help.window_sizeChg_Or_ZoomPan();
                    }
                }
            }
        }
        private void quant_init()
        {
            max_intense = 0;
            //强度
            foreach (List<List<double>> ddlist in function.evi.ActualIsotopicPeakList)
            {
                double max_intense_tmp = 0.0;
                foreach (List<Double> dlist in ddlist)
                {
                    if (dlist.Count == 0)
                        max_intense_tmp = 0.0;
                    else
                        max_intense_tmp = max_intense_tmp > dlist.Max() ? max_intense_tmp : dlist.Max();
                }
                if (max_intense < max_intense_tmp)
                    max_intense = max_intense_tmp;
            }
            intense_factor = 10.0 / max_intense;

            //理论同位素的m/z是升序排列的
            max_mz = function.evi.TheoricalIsotopicMZList[function.evi.Number - 1].Last();
            min_mz = function.evi.TheoricalIsotopicMZList[0].First();
            int all_count = 0;
            foreach (List<double> mz in function.evi.TheoricalIsotopicMZList)
                all_count += mz.Count;
            mz_factor = 25.0 / (all_count + 1);

            //观测时间点 scanTime也是升序排列的
            //max_scan = function.evi.EndTime;
            //min_scan = function.evi.StartTime;
            //ME
            max_scan = function.evi.ScanTime.Count - 2;
            min_scan = 0;
            //max_scan = evidence.ScanTime[evidence.EndTime];
            //min_scan = evidence.ScanTime[evidence.StartTime];
            scan_factor = 10.0 / (double)(max_scan - min_scan);
        }
        private void UseGeometry()
        {
            /****************** Use Geometry ********************/
            double alpha_value = this.d3_alpha_slider.Value;
            ModelVisual3D visual = new ModelVisual3D();
            Model3DGroup group = new Model3DGroup();
            //为材质创建无穷远处的入射光
            DirectionalLight light = new DirectionalLight(Colors.Black, new Vector3D(10, 25, -1)); //RosyBrown
            group.Children.Add(light);
            int number = 0;
            for (int i = 0; i < function.evi.Number; ++i)
            {
                number += function.evi.TheoricalIsotopicMZList[i].Count;
            }
            ++number;
            GeometryModel3D[] models = new GeometryModel3D[number];
            MeshGeometry3D[] meshes = new MeshGeometry3D[number];
            visual.Content = group;
            viewport3D.Children.Add(visual);

            //this.GenerateBottomPanel(group);

            Point3DCollection[] positions = new Point3DCollection[number];
            Int32Collection[] indices = new Int32Collection[number];
            PointCollection[] textures = new PointCollection[number];
            LinearGradientBrush[] brushes = new LinearGradientBrush[number];

            //有几个色谱，就有几个
            int count_z = 0;
            for (int k = 0; k < function.evi.Number; ++k)
            {
                for (int j = 0; j < function.evi.TheoricalIsotopicMZList[k].Count; ++j)
                {
                    models[count_z] = new GeometryModel3D();
                    meshes[count_z] = new MeshGeometry3D();
                    models[count_z].Geometry = meshes[count_z];
                    //if (j < 4) continue;
                    group.Children.Add(models[count_z]);

                    //每个scan有一个positions点集，画图用
                    positions[count_z] = new Point3DCollection();
                    positions[count_z].Clear();
                    indices[count_z] = new Int32Collection();
                    indices[count_z].Clear();
                    textures[count_z] = new PointCollection();
                    textures[count_z].Clear();
                    brushes[count_z] = new LinearGradientBrush();
                    int count_x = 0;
                    for (int i = min_scan; i <= max_scan; ++i)
                    {
                        double x = (count_x + 1) * scan_factor;
                        double y = function.evi.ActualIsotopicPeakList[k][j][i] * intense_factor;
                        double z = (count_z + 1) * mz_factor;
                        ++count_x;
                        if (y > 0)
                        {
                            positions[count_z].Add(new Point3D(x, 0, z + 0.3));
                            positions[count_z].Add(new Point3D(x, 0, z - 0.3));

                        }
                        else
                        {
                            positions[count_z].Add(new Point3D(x, 0, z));
                            positions[count_z].Add(new Point3D(x, 0, z));
                        }
                        positions[count_z].Add(new Point3D(x, y, z));
                    }
                    //为每个片创建纹理坐标
                    for (int i = 0; i < positions[count_z].Count; i += 3)
                    {
                        textures[count_z].Add(new Point(0, 0));
                        textures[count_z].Add(new Point(0, 0));
                        textures[count_z].Add(new Point(positions[count_z][i + 2].Y / 10.0, 0));
                    }
                    textures[count_z].Freeze();
                    meshes[count_z].TextureCoordinates = textures[count_z];
                    positions[count_z].Freeze();
                    meshes[count_z].Positions = positions[count_z];

                    //片中头尾相连
                    for (int i = 5; i < positions[count_z].Count; i += 3)
                    {
                        indices[count_z].Add(i - 2);
                        indices[count_z].Add(i - 3);
                        indices[count_z].Add(i - 5);

                        indices[count_z].Add(i - 4);
                        indices[count_z].Add(i - 3);
                        indices[count_z].Add(i - 1);

                        indices[count_z].Add(i - 3);
                        indices[count_z].Add(i - 2);
                        indices[count_z].Add(i);

                        indices[count_z].Add(i);
                        indices[count_z].Add(i - 1);
                        indices[count_z].Add(i - 3);
                    }


                    indices[count_z].Freeze();
                    //
                    meshes[count_z].TriangleIndices = indices[count_z];
                    brushes[count_z].StartPoint = new Point(0, 0);
                    brushes[count_z].EndPoint = new Point(1, 0);
                    if (k % 2 == 0) //(count_z / function.evi.ActualIsotopicPeakList[k][j].Count) % 2 == 0
                    {
                        brushes[count_z].GradientStops.Add(new GradientStop(Color.FromArgb((byte)alpha_value, Colors.DarkBlue.R, Colors.DarkBlue.G, Colors.DarkBlue.B), 0));
                        brushes[count_z].GradientStops.Add(new GradientStop(Color.FromArgb((byte)alpha_value, Colors.SkyBlue.R, Colors.SkyBlue.G, Colors.SkyBlue.B), 0.1));
                        brushes[count_z].GradientStops.Add(new GradientStop(Color.FromArgb((byte)alpha_value, Colors.Lime.R, Colors.Lime.G, Colors.Lime.B), 0.25));
                        brushes[count_z].GradientStops.Add(new GradientStop(Color.FromArgb((byte)alpha_value, Colors.Gold.R, Colors.Gold.G, Colors.Gold.B), 0.5));
                        brushes[count_z].GradientStops.Add(new GradientStop(Color.FromArgb((byte)alpha_value, Colors.Red.R, Colors.Red.G, Colors.Red.B), 1));
                    }
                    else
                    {
                        brushes[count_z].GradientStops.Add(new GradientStop(Color.FromArgb((byte)alpha_value, (byte)((double)Colors.DarkBlue.R / 2), (byte)((double)Colors.DarkBlue.G / 2), (byte)((double)Colors.DarkBlue.B / 2)), 0));
                        brushes[count_z].GradientStops.Add(new GradientStop(Color.FromArgb((byte)alpha_value, (byte)((double)Colors.SkyBlue.R / 2), (byte)((double)Colors.SkyBlue.G / 2), (byte)((double)Colors.SkyBlue.B / 2)), 0.1));
                        brushes[count_z].GradientStops.Add(new GradientStop(Color.FromArgb((byte)alpha_value, (byte)((double)Colors.Lime.R / 2), (byte)((double)Colors.Lime.G / 2), (byte)((double)Colors.Lime.B / 2)), 0.25));
                        brushes[count_z].GradientStops.Add(new GradientStop(Color.FromArgb((byte)alpha_value, (byte)((double)Colors.Gold.R / 2), (byte)((double)Colors.Gold.G / 2), (byte)((double)Colors.Gold.B / 2)), 0.5));
                        brushes[count_z].GradientStops.Add(new GradientStop(Color.FromArgb((byte)alpha_value, (byte)((double)Colors.Red.R / 2), (byte)((double)Colors.Red.G / 2), (byte)((double)Colors.Red.B / 2)), 1));
                    }
                    MaterialGroup matgroup = new MaterialGroup();
                    MaterialGroup backgroup = new MaterialGroup();

                    //matgroup.Children.Add(new DiffuseMaterial(new SolidColorBrush(Colors.White)));
                    //matgroup.Children.Add(new EmissiveMaterial(new SolidColorBrush(colors[j % 3])));
                    matgroup.Children.Add(new DiffuseMaterial(brushes[count_z]));
                    matgroup.Children.Add(new EmissiveMaterial(brushes[count_z]));

                    //backgroup.Children.Add(new DiffuseMaterial(new SolidColorBrush(Colors.White)));
                    //backgroup.Children.Add(new EmissiveMaterial(new SolidColorBrush(colors[j % 3])));
                    backgroup.Children.Add(new DiffuseMaterial(brushes[count_z]));
                    backgroup.Children.Add(new EmissiveMaterial(brushes[count_z]));

                    //给model加上笔刷（颜色）
                    models[count_z].Material = matgroup;
                    models[count_z].BackMaterial = backgroup;
                    ++count_z;
                }
                ++count_z; //另一种标记再来一个间隔
            }
            WireLine line = new WireLine();
            double line_x = (function.evi.StartTime + 1) * scan_factor;
            double line_y = 0.0;
            double line_z_0 = 0;
            double line_z_1 = (count_z + 1) * mz_factor;
            Point3D line_point0 = new Point3D(line_x, line_y, line_z_0);
            Point3D line_point1 = new Point3D(line_x, line_y, line_z_1);
            line.Point1 = line_point0;
            line.Point2 = line_point1;
            line.Color = Colors.Black;
            line.Thickness = 1;
            viewport3D.Children.Add(line);
            line = new WireLine();
            line_x = (function.evi.EndTime + 1) * scan_factor;
            line_point0 = new Point3D(line_x, line_y, line_z_0);
            line_point1 = new Point3D(line_x, line_y, line_z_1);
            line.Point1 = line_point0;
            line.Point2 = line_point1;
            line.Color = Colors.Black;
            line.Thickness = 1;
            viewport3D.Children.Add(line);
        }
        //画3D轻重标记图
        private void UseWirePolyLine()
        {
            Color cl1 = Color.FromRgb(16, 37, 63);
            Color cl2 = Color.FromRgb(22, 55, 94);
            Color cl3 = Color.FromRgb(85, 142, 213);
            Color cl4 = Color.FromRgb(142, 180, 227);
            Color cl5 = Color.FromRgb(99, 36, 35);
            Color cl6 = Color.FromRgb(149, 55, 52);
            Color cl7 = Color.FromRgb(217, 150, 148);
            Color cl8 = Color.FromRgb(229, 185, 183);
            Color[,] colors = { { cl1, cl2, cl3, cl4 }, { cl5, cl6, cl7, cl8 } };

            //int count = evidence.TheoricalIsotopicMZList.Count;
            int all_count = 0;
            foreach (List<double> mz in function.evi.TheoricalIsotopicMZList)
                all_count += mz.Count;
            WirePolyline[] wpl = new WirePolyline[all_count];
            int wpl_index = 0;
            int count_z = 0;
            for (int k = 0; k < function.evi.Number; ++k) //轻标和重标
            {
                if (function.evi.StartTimes.Count > 0)
                {
                    WireLine line0 = new WireLine();
                    double line_x0 = (function.evi.StartTimes[k] + 1) * scan_factor;
                    double line_y0 = 0.0;
                    double line_z_00 = (count_z + 0.5) * mz_factor;
                    double line_z_10 = (count_z + function.evi.TheoricalIsotopicMZList[k].Count + 0.5) * mz_factor;
                    Point3D line_point00 = new Point3D(line_x0, line_y0, line_z_00);
                    Point3D line_point10 = new Point3D(line_x0, line_y0, line_z_10);
                    line0.Point1 = line_point00;
                    line0.Point2 = line_point10;
                    line0.Color = Colors.Black;
                    line0.Thickness = 4;
                    viewport3D.Children.Add(line0);
                    line0 = new WireLine();
                    line_x0 = (function.evi.EndTimes[k] + 1) * scan_factor;
                    line_point00 = new Point3D(line_x0, line_y0, line_z_00);
                    line_point10 = new Point3D(line_x0, line_y0, line_z_10);
                    line0.Point1 = line_point00;
                    line0.Point2 = line_point10;
                    line0.Color = Colors.Black;
                    line0.Thickness = 4;
                    viewport3D.Children.Add(line0);
                }
                for (int i = 0; i < function.evi.TheoricalIsotopicMZList[k].Count; i++) //例子中有三条曲线
                {
                    Point3DCollection points = new Point3DCollection();
                    PointCollection points2 = new PointCollection();
                    int count_x = 0;
                    for (int j = min_scan; j <= max_scan; ++j)
                    //for (int j = function.evi.StartTime; j <= function.evi.EndTime; j++)//5到26之间的点 包括26////////////////////////////////
                    {
                        //double x = 0;
                        double x = (count_x + 1) * scan_factor;//按照计算的比例均匀分配在坐标轴上
                        //double y = 0;
                        double y = function.evi.ActualIsotopicPeakList[k][i][j] * intense_factor;//第i条曲线的第j列 强度值 intensity
                        //double z = 0;
                        double z = (count_z + 1) * mz_factor;//第i条曲线的第j列 质荷比
                        count_x++;

                        Point3D point = new Point3D(x, y, z);
                        points.Add(point);

                        Point point2 = new Point(x, y);

                        //增加阴影效果
                        #region
                        /* 
                        if (j < max_scan) //绘制多条竖线，模拟倒影效果
                        {
                            const int interval = 10;
                            double x_delta = scan_factor / interval;
                            double y_delta = (function.evi.ActualIsotopicPeakList[k][i][j + 1] * intense_factor - y) / interval;
                            for (int p = 0; p < interval; ++p)
                            {
                                WireLine line_tmp = new WireLine();
                                double line_x_tmp = x + x_delta * p;
                                double line_y_tmp = y + y_delta * p;
                                double line_y_tmp_1 = 0;
                                double line_z_tmp = z;
                                Point3D line_point0_tmp = new Point3D(line_x_tmp, line_y_tmp, line_z_tmp);
                                Point3D line_point1_tmp = new Point3D(line_x_tmp, line_y_tmp_1, line_z_tmp);
                                line_tmp.Point1 = line_point0_tmp;
                                line_tmp.Point2 = line_point1_tmp;
                                line_tmp.Color = Color.FromArgb(0x33, 0x89, 0xC2, 0x52);
                                line_tmp.Thickness = 1;
                                viewport3D.Children.Add(line_tmp);
                            }
                        }
                         */
                        #endregion
                    }
                    count_z++;
                    wpl[wpl_index] = new WirePolyline();
                    wpl[wpl_index].Points = points;

                    if (i / 4 >= 1)
                        wpl[wpl_index].Color = colors[k % 2, 3];
                    else
                        wpl[wpl_index].Color = colors[k % 2, i % 4];
                    /* */
                    wpl[wpl_index].Thickness = 3;
                    viewport3D.Children.Add(wpl[wpl_index]);
                    wpl_index++;
                }
                ++count_z;
            }
            //Add two lines : start scan and end scan

            WireLine line = new WireLine();
            double line_x = (function.evi.StartTime + 1) * scan_factor;
            double line_y = 0.0;
            double line_z_0 = 0;
            double line_z_1 = (count_z + 1) * mz_factor;
            Point3D line_point0 = new Point3D(line_x, line_y, line_z_0);
            Point3D line_point1 = new Point3D(line_x, line_y, line_z_1);
            line.Point1 = line_point0;
            line.Point2 = line_point1;
            line.Color = Colors.ForestGreen;
            line.Thickness = 1;
            viewport3D.Children.Add(line);
            line = new WireLine();
            line_x = (function.evi.EndTime + 1) * scan_factor;
            line_point0 = new Point3D(line_x, line_y, line_z_0);
            line_point1 = new Point3D(line_x, line_y, line_z_1);
            line.Point1 = line_point0;
            line.Point2 = line_point1;
            line.Color = Colors.ForestGreen;
            line.Thickness = 1;
            viewport3D.Children.Add(line);

        }
        private void show_all_clk(object sender, RoutedEventArgs e)
        {
            display_psms = null;
            if ((bool)this.other_psms_cbx.IsChecked)
            {
                this.other_psms_cbx.IsChecked = false;
                other_psms_cbx_clk(null, null);
            }
            initial_Peptide();
        }
        private void update_selected_psms()
        {
            selected_psms.Clear();
            for (int i = 0; i < data.SelectedItems.Count; ++i)
            {
                PSM p = (PSM)data.SelectedItems[i];
                selected_psms.Add(p);
            }
        }
        private void remove_others_clk(object sender, RoutedEventArgs e)
        {
            update_selected_psms();
            display_psms = new ObservableCollection<PSM>(selected_psms);
            data.ItemsSource = display_psms;
            display_size.Text = display_psms.Count + "";
        }

        private void remove_selected_clk(object sender, RoutedEventArgs e)
        {
            update_selected_psms();
            Hashtable selected_Id = new Hashtable();
            for (int i = 0; i < selected_psms.Count; ++i)
            {
                selected_Id[selected_psms[i].Id] = 1;
            }
            ObservableCollection<PSM> tmp_display_psms = new ObservableCollection<PSM>();
            for (int i = 0; i < display_psms.Count; ++i)
            {
                if (selected_Id[display_psms[i].Id] == null)
                    tmp_display_psms.Add(display_psms[i]);
            }
            display_psms = tmp_display_psms;
            data.ItemsSource = display_psms;
            display_size.Text = display_psms.Count + "";
            selected_size.Text = "0";
        }
        private void copy_title_clk(object sender, RoutedEventArgs e)
        {
            selected_psms.Clear();
            for (int i = 0; i < data.SelectedItems.Count; ++i)
            {
                PSM p = (PSM)data.SelectedItems[i];
                selected_psms.Add(p);
            }
            if (selected_psms == null || selected_psms.Count == 0)
                return;
            string title = "";
            for (int i = 0; i < selected_psms.Count - 1; ++i)
                title += selected_psms[i].Title + "\n";
            title += selected_psms.Last().Title;
            copyToClipboard(title);
        }
        private void copyToClipboard(string txt)
        {
            try
            {
                Clipboard.SetData(DataFormats.Text, txt);
            }
            catch (Exception exe)
            {

            }
        }
        private void copy_SQ_clk(object sender, RoutedEventArgs e)
        {
            selected_psms.Clear();
            for (int i = 0; i < data.SelectedItems.Count; ++i)
            {
                PSM p = (PSM)data.SelectedItems[i];
                selected_psms.Add(p);
            }
            if (selected_psms == null || selected_psms.Count == 0)
                return;
            string sq = "";
            for (int i = 0; i < selected_psms.Count - 1; ++i)
                sq += selected_psms[i].Sq + "\n";
            sq += selected_psms.Last().Sq;
            copyToClipboard(sq);
        }
        private void copy_AC_clk(object sender, RoutedEventArgs e)
        {
            selected_psms.Clear();
            for (int i = 0; i < data.SelectedItems.Count; ++i)
            {
                PSM p = (PSM)data.SelectedItems[i];
                selected_psms.Add(p);
            }
            if (selected_psms == null || selected_psms.Count == 0)
                return;
            string ac = "";
            for (int i = 0; i < selected_psms.Count - 1; ++i)
                ac += selected_psms[i].AC.Substring(0, selected_psms[i].AC.Length - 1) + "\n";
            ac += selected_psms.Last().AC.Substring(0, selected_psms.Last().AC.Length - 1);
            copyToClipboard(ac);
        }
        private void plot_sizeChg(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                double old_height = Ladder_Help.Height;
                Ladder_Help.Height = this.display_tab.ActualHeight;
                if (old_height != 0.0)
                    Ladder_Help.Scale_Height = Ladder_Help.Scale_Height * Ladder_Help.Height / old_height;
            }
            if (e.WidthChanged)
            {
                double old_width = Ladder_Help.Width;
                Ladder_Help.Width = this.frame_right.ActualWidth;
                if (old_width != 0.0)
                    Ladder_Help.Scale_Width = Ladder_Help.Scale_Width * Ladder_Help.Width / old_width;
            }
            if (display_tab.SelectedIndex == 1) //如果当前选中的是MS2
            {
                if (e.HeightChanged)
                {
                    if (dis_help.ddh == null)
                        return;
                    dis_help.ddh.FontSize_BY = 0.0;
                    dis_help.ddh.FontSize_SQ = 0.0;
                }
                MS2_Help ms2_help = new MS2_Help(Model2, dis_help, Ladder_Help.Scale_Width, Ladder_Help.Scale_Height, this.frame_right.ActualWidth, this.display_tab.ActualHeight, this);
                ms2_help.window_sizeChg_Or_ZoomPan();
            }
            else if (display_tab.SelectedIndex == 0) //如果当前选中的是MS1
            {
                window_sizeChg_Or_ZommPan_ms1();
            }
            else if (display_tab.SelectedIndex == 2)
            {
                //Zoom(12);
                ChangeAxisTextPosition();
                ChangeTextPosition();
                //Zoom(-12);
                //ChangeAxisTextPosition();
                //ChangeTextPosition();
            }
        }
        public void ms1_draw_speedUp(double one_width) //用来对MS1进行加速，如果不加速，全图的话，MS1由于谱峰过多，导致绘制比较慢
        {
            if (selected_psm == null || selected_ms1 == null)
                return;
            if ((this.Model1.Axes[1].ActualMaximum - this.Model1.Axes[1].ActualMinimum) > 10)
                this.Model1.Axes[1].StringFormat = "f1";
            else if ((this.Model1.Axes[1].ActualMaximum - this.Model1.Axes[1].ActualMinimum) > 1)
                this.Model1.Axes[1].StringFormat = "f2";
            else if ((this.Model1.Axes[1].ActualMaximum - this.Model1.Axes[1].ActualMinimum) > 0.01)
                this.Model1.Axes[1].StringFormat = "f4";
            else
                this.Model1.Axes[1].StringFormat = "f6";

            if ((this.Model1.Axes[1].ActualMaximum - this.Model1.Axes[1].ActualMinimum) <= 1.0e-7) //如果用户放大的太小，不进行操作
                return;
            this.Model1.Axes[1].MajorStep = (this.Model1.Axes[1].ActualMaximum - this.Model1.Axes[1].ActualMinimum) / 5;
            bool is_refresh = (bool)this.is_refresh.IsChecked;
            bool is_add_arrow = (bool)this.is_addArrow.IsChecked;
            bool is_theory_isotope = (bool)this.is_theory.IsChecked;
            double cur_max_inten = 0.0, intensity_ratio = 0.0; //当前屏幕中的最高峰的绝对强度, intensity_ratio为refresh时候，强度需要缩放的比例
            if (one_width == 0.0 || double.IsInfinity(one_width))
                return;

            if ((bool)this.is_addCharge.IsChecked)
                PEAK.parse_charge(selected_ms1.Peaks, selected_ms1.Max_inten_E, Intensity_t);

            List<int> points_to_peaks = new List<int>();

            this.Model1.Series.Clear();
            this.Model1.Annotations.Clear();
            for (int i = 0; i < selected_ms1.Peaks.Count; ++i)
            {
                IList<IDataPoint> Points = new List<IDataPoint>();
                Points.Add(new DataPoint(selected_ms1.Peaks[i].Mass, 0));
                Points.Add(new DataPoint(selected_ms1.Peaks[i].Mass, selected_ms1.Peaks[i].Intensity));
                LineSeries Ls = new LineSeries();
                Ls.Color = OxyColors.Black;
                Ls.LineStyle = LineStyle.Solid;
                Ls.StrokeThickness = this.dis_help.ddhms1.peak_size;
                Ls.Points = Points;
                Ls.YAxisKey = this.Model1.Axes[0].Key;
                this.Model1.Series.Add(Ls);
            }

            Collection<Series> all_lines = this.Model1.Series;

            ObservableCollection<ObservableCollection<LineSeries>> all_lines_bin = new ObservableCollection<ObservableCollection<LineSeries>>();
            ObservableCollection<double> all_X = new ObservableCollection<double>();
            double cur_x = this.Model1.Axes[1].ActualMinimum;
            while (cur_x <= this.Model1.Axes[1].ActualMaximum)
            {
                all_X.Add(cur_x);
                all_lines_bin.Add(new ObservableCollection<LineSeries>());
                cur_x += one_width;
            }
            //保留每1个像素内的最高峰
            int cur_index = 0;
            for (int i = 0; i < all_lines.Count; ++i)
            {
                LineSeries cur_ls = (LineSeries)all_lines[i];
                cur_ls.TrackerFormatString = "m/z: {2:0.000}\nIntensity: {4:0.00}%";
                cur_ls.MouseDown += (s, e) =>
                {
                    DataPoint click_point = (DataPoint)cur_ls.Points[1]; //(int)Math.Round(e.HitTestResult.Index)
                    state_X.Text = click_point.X.ToString("f5");
                    double intensity = 0.0;
                    if (is_refresh)
                        intensity = click_point.Y * cur_max_inten / 100;
                    else
                        intensity = click_point.Y * selected_ms1.Max_inten_E / 100;
                    state_Y.Text = intensity.ToString("E2");
                    if (e.ChangedButton == OxyMouseButton.Left)
                    {
                        if (this.ms1_mz1_txt.Text == "NaN")
                        {
                            this.ms1_mz1_txt.Text = state_X.Text;
                        }
                        else
                        {
                            this.ms1_mz2_txt.Text = state_X.Text;
                        }
                        compute_btn();
                        one_width = (this.Model1.Axes[1].ActualMaximum - this.Model1.Axes[1].ActualMinimum) / ((int)this.Model1.Axes[1].ScreenMax.X - (int)this.Model1.Axes[1].ScreenMin.X);
                        ms1_draw_speedUp(one_width);
                    }
                };
                double cur_X = cur_ls.Points[0].X;
                while (cur_index < all_X.Count && cur_X >= all_X[cur_index])
                {
                    cur_index++;
                }
                if (cur_index > 0 && cur_index < all_X.Count)
                    all_lines_bin[cur_index - 1].Add(cur_ls);
                else if (cur_index == all_X.Count)
                {
                    if (cur_X <= this.Model1.Axes[1].ActualMaximum)
                        all_lines_bin[cur_index - 1].Add(cur_ls);
                    else
                        break;
                }
            }

            int cur_max_i = -1, cur_max_j = -1;
            Collection<Series> new_lines = new Collection<Series>();
            for (int i = 0; i < all_lines_bin.Count; ++i)
            {
                if (all_lines_bin[i].Count > 0)
                {
                    double maxInten = all_lines_bin[i][0].Points[1].Y;
                    int max_j = 0;
                    for (int j = 1; j < all_lines_bin[i].Count; ++j)
                    {
                        if (maxInten < all_lines_bin[i][j].Points[1].Y)
                        {
                            maxInten = all_lines_bin[i][j].Points[1].Y;
                            max_j = j;
                        }
                    }

                    new_lines.Add(all_lines_bin[i][max_j]);
                    if (cur_max_inten < maxInten)
                    {
                        cur_max_inten = maxInten;
                        cur_max_i = i;
                        cur_max_j = max_j;
                    }
                }
            }
            this.Model1.Series = new_lines;
            intensity_ratio = 100 / cur_max_inten;
            cur_max_inten = cur_max_inten * selected_ms1.Max_inten_E / 100;

            //END
            const string blank_space = "      "; //6个空格
            double specific_x = this.Model1.Axes[1].ActualMinimum + 0.01 * (this.Model1.Axes[1].ActualMaximum - this.Model1.Axes[1].ActualMinimum);
            double screen_x = this.Model1.Axes[1].Transform(specific_x);
            for (int i = 0; i < 6; ++i)
            {
                string title_str = "";
                string value_str = "";
                switch (i)
                {
                    case 0:
                        title_str = "Base Peak: ";
                        if ((bool)this.is_refresh.IsChecked)
                            value_str = cur_max_inten.ToString("E2") + blank_space;
                        else
                            value_str = selected_ms1.Max_inten_E.ToString("E2") + blank_space;
                        break;
                    case 1:
                        title_str = "Scan: ";
                        value_str = selected_ms1.Scan + blank_space;
                        break;
                    case 2:
                        title_str = "Relative Scan: ";
                        if (selected_ms1.Scan_relative > 0)
                            value_str = "+" + selected_ms1.Scan_relative + blank_space;
                        else
                            value_str = selected_ms1.Scan_relative + blank_space;
                        break;
                    case 3:
                        title_str = "Retention Time: ";
                        value_str = selected_ms1.Retention_Time.ToString("F2") + " sec" + blank_space;
                        break;
                    case 4:
                        if (this.task.has_ratio && this.selected_psm != null)
                        {
                            title_str = "Ratio: ";
                            value_str = this.selected_psm.Ratio.ToString("F2") + blank_space;
                        }
                        break;
                    case 5:
                        if (this.task.has_ratio && this.selected_psm != null)
                        {
                            title_str = "Interference Score: ";
                            value_str = this.selected_psm.Sigma.ToString("F2") + blank_space;
                        }
                        break;
                }
                TextAnnotation titleAnnotation = new TextAnnotation();
                TextAnnotation titleAnnotation2 = new TextAnnotation();
                titleAnnotation.Font = Display_Detail_Help.font_type;
                titleAnnotation2.Font = titleAnnotation.Font;
                titleAnnotation.HorizontalAlignment = OxyPlot.HorizontalAlignment.Left;
                titleAnnotation2.HorizontalAlignment = titleAnnotation.HorizontalAlignment;
                titleAnnotation.FontSize = 12;
                titleAnnotation2.FontSize = titleAnnotation.FontSize;
                titleAnnotation.Stroke = OxyColors.Transparent;
                titleAnnotation2.Stroke = titleAnnotation.Stroke;
                titleAnnotation.YAxisKey = this.Model1.Axes[3].Key;
                titleAnnotation2.YAxisKey = titleAnnotation.YAxisKey;
                titleAnnotation.TextColor = Display_Detail_Help.title_color;
                titleAnnotation2.TextColor = Display_Detail_Help.value_color;
                titleAnnotation.Text = title_str;
                titleAnnotation2.Text = value_str;
                titleAnnotation2.FontWeight = OxyPlot.FontWeights.Bold;
                titleAnnotation.Position = new DataPoint(this.Model1.Axes[1].InverseTransform(screen_x), 0);
                screen_x += get_annotation_width(titleAnnotation);
                titleAnnotation2.Position = new DataPoint(this.Model1.Axes[1].InverseTransform(screen_x), 0);
                screen_x += get_annotation_width(titleAnnotation2);
                this.Model1.Annotations.Add(titleAnnotation);
                this.Model1.Annotations.Add(titleAnnotation2);
            }

            //如果点击refresh按钮，那么参数is_refresh=true，需要对谱峰强度进行更新

            Spectra_MS1_Isotope max_actual_isotope = null;
            //is_add_arrow = (is_add_arrow) && (task.quantification_file == "");
            //下面函数，绘制MS2导出的母离子同位素峰簇信息，并使用红色标注，同时将质量信息附注上，最后返回实际的同位素簇。
            max_actual_isotope = dis_help.get_actual_isotope(is_refresh,
                (bool)is_addCharge.IsChecked, intensity_ratio, this.Model1, selected_ms1, dis_help);

            this.Chrome_help = get_theory_iostope();

            if (is_theory_isotope) //画鹰眼同时画星
            {
                dis_help.display_eagle_eye(this.Chrome_help.theory_isotopes, selected_ms1, this.Model1, this);
            }
            if (is_add_arrow) //标记同位素峰簇
            {
                //给昆哥增加的内容
                update_ms1_masserror(true);
                //dis_help.display_actual_isotope(screen_x, evi, selected_ms1, cur_max_inten, this.Model1);
            }
            this.Model1.RefreshPlot(true);
        }
        private double get_nearest(double mass, ref int index)
        {
            double min_mass = double.MaxValue;
            int mi = -1;
            for (int i = 0; i < this.Model1.Series.Count; ++i)
            {
                if (this.Model1.Series[i] is LineSeries)
                {
                    LineSeries ls = this.Model1.Series[i] as LineSeries;
                    double mz = ls.Points[0].X;
                    if (Math.Abs(mz - mass) < min_mass)
                    {
                        mi = i;
                        min_mass = Math.Abs(mz - mass);
                    }
                }
                else
                    break;
            }
            //for (int i = 0; i < selected_ms1.Peaks.Count; ++i)
            //{
            //    double mz = selected_ms1.Peaks[i].Mass;
            //    if (Math.Abs(mz - mass) < min_mass)
            //    {
            //        mi = i;
            //        min_mass = Math.Abs(mz - mass);
            //    }
            //}
            if (mi != -1 && min_mass <= 0.1)
            {
                LineSeries ls = this.Model1.Series[mi] as LineSeries;
                index = mi;
                return ls.Points[0].X;
                //return selected_ms1.Peaks[mi].Mass;
            }
            return 0.0;
        }
        private double get_nearest(double mass, List<Theory_IOSTOPE> theory_iostope)
        {
            double min_mass = double.MaxValue;
            int mi = -1, mj = -1;
            for (int i = 0; i < theory_iostope.Count; ++i)
            {
                for (int j = 0; j < theory_iostope[i].mz.Count; ++j)
                {
                    if (Math.Abs(theory_iostope[i].mz[j] - mass) < min_mass)
                    {
                        mi = i;
                        mj = j;
                        min_mass = Math.Abs(theory_iostope[i].mz[j] - mass);
                    }
                }
            }
            if (mi != -1 && mj != -1 && min_mass <= 0.1)
            {
                return theory_iostope[mi].mz[mj];
            }
            return 0.0;
        }
        public void update_ms1_masserror(List<Theory_IOSTOPE> theory_iostope, int index, ref List<List<PEAK_MS1>> ms_list, bool is_first)
        {
            if (selected_psm == null)
                return;
            int label_count = theory_iostope.Count;
            ms_list = new List<List<PEAK_MS1>>();
            for (int i = 0; i < label_count; ++i)
                ms_list.Add(new List<PEAK_MS1>());
            List<double> peptides_masses = new List<double>();
            if (this.dis_help.Psm_help is pBuild.PSM_Help)
                peptides_masses = this.psm_help.get_masses(label_count);
            else if (this.dis_help.Psm_help is pLink.PSM_Help_2)
            {
                pLink.PSM_Help_2 ph2 = this.dis_help.Psm_help as pLink.PSM_Help_2;
                peptides_masses = ph2.get_masses(pLink_result.pLink_label);
            }
            double delta_peptide_mass = Config_Help.mass_ISO / selected_psm.get_charge();

            for (int i = 0; i < label_count; ++i)
            {
                double peptide_mass = peptides_masses[i] + delta_peptide_mass * index;

                double star_mz = get_nearest(peptide_mass, theory_iostope);

                //flag1和flag2分别表示是否存在鉴定到的对应的理论Mono的m/z，及对应的对儿的m/z，理论flag1=true,如果没有标记（比值），flag2=false
                bool flag1 = true;
                if (star_mz == 0.0 || star_mz < this.Model1.Axes[1].ActualMinimum || star_mz > this.Model1.Axes[1].ActualMaximum)
                    flag1 = false;
                //只有是标记搜索才会显示鉴定结果对应的“对儿”信号,star2_mz == star1_mz表示是只有轻标信号，即没有ratio比值

                int index1 = 0;
                double star_ms_mz = get_nearest(star_mz, ref index1);

                double mz_error = (star_ms_mz - star_mz) * 1e6 / star_mz; //实验质量-理论质量

                TextAnnotation ta = new TextAnnotation();
                ta.Text = mz_error.ToString("F2") + "ppm";
                LineSeries ls1 = this.Model1.Series[index1] as LineSeries;
                double y1 = ls1.Points[1].Y + 10;
                if (ls1.Points[1].Y + 10 >= 95)
                    y1 = 95;
                ta.Position = new DataPoint(star_ms_mz, y1);
                ta.Stroke = OxyColors.Transparent;
                if (flag1 && is_first)
                    this.Model1.Annotations.Add(ta);

               
                ms_list[i] = get_List(star_mz);
            }
        }
        private void display_ms1_chrome(object sender, EventArgs e)
        {
            update_ms1_masserror();
        }
        //更新两根Mono峰与理论的m/z偏差，并且在右上角位置绘制两个根Mono峰的色谱曲线
        public void update_ms1_masserror(bool is_first = false)
        {
            if (this.Chrome_help == null || this.Chrome_help.theory_isotopes == null || this.Chrome_help.theory_isotopes.Count == 0)
                return;
            List<Theory_IOSTOPE> theory_iostope = this.Chrome_help.theory_isotopes;

            //int start_scan = this.Chrome_help.start_scan;
            //int end_scan = this.Chrome_help.end_scan;

            List<List<PEAK_MS1>> ms_list = new List<List<PEAK_MS1>>();
            if (this.is_display_animation)
            {
                int index = this.Chrome_help.index + 1;
                if (index >= this.Chrome_help.all_index)
                    index = 0;
                this.Chrome_help.index = index;
                update_ms1_masserror(theory_iostope, this.Chrome_help.index, ref ms_list, is_first);
            }
            else
            {
                this.Chrome_help.index = 0;
                for (int i = 0; i < 2; ++i)
                    ms_list.Add(new List<PEAK_MS1>());
                if (Config_Help.IsDecimalAllowed(this.ms1_mz1_txt.Text))
                {
                    double mz1 = double.Parse(this.ms1_mz1_txt.Text);
                    ms_list[0] = get_List(mz1);
                }
                if (Config_Help.IsDecimalAllowed(this.ms1_mz2_txt.Text))
                {
                    double mz2 = double.Parse(this.ms1_mz2_txt.Text);
                    ms_list[1] = get_List(mz2);
                }
            }
            if (ms_list[0].Count == 0)
                return;
            int start_scan = ms_list[0].First().scan;
            int end_scan = ms_list[0].Last().scan;
            Display_Help dh = new Display_Help();
            dh.mainW = this;
            //找到MS2的第一张一级谱的Scan
            string raw_name = selected_psm.get_rawname();
            int cur_scan = selected_psm.get_scan();
            while (ms1_scan_hash[(int)(title_hash[raw_name])][cur_scan] == null)
                --cur_scan;
            dh.display_chrom(ms_list, this.Model1, cur_scan, start_scan, end_scan, this.Chrome_help.index);
        }
        public void window_sizeChg_Or_ZommPan_ms1() //绘制MS1
        {
            if (this.Model1 == null)
                return;

            if (this.Model1.Axes[1].Scale != 0.0)
            {
                one_width = (this.Model1.Axes[1].ActualMaximum - this.Model1.Axes[1].ActualMinimum) / ((int)this.Model1.Axes[1].ScreenMax.X - (int)this.Model1.Axes[1].ScreenMin.X);
            }
            else if (this.Model2 != null)
            {
                one_width = (this.Model1.Axes[1].ActualMaximum - this.Model1.Axes[1].ActualMinimum) / ((int)this.Model2.Axes[1].ScreenMax.X - (int)this.Model2.Axes[1].ScreenMin.X);
            }
            this.Model1.Axes[1].Scale = 1.0 / one_width;
            ms1_draw_speedUp(one_width);
        }
        private double get_annotation_width(TextAnnotation txtAnnotation)
        {
            FormattedText ft = new FormattedText(txtAnnotation.Text, System.Globalization.CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(txtAnnotation.Font), txtAnnotation.FontSize, Brushes.Black);
            return ft.WidthIncludingTrailingWhitespace;
        }

        private void ms2_advance(object sender, RoutedEventArgs e)
        {
            if (this.Model2 == null)
                return;
            if (selected_psm == null && task_flag != "pLink")
            {
                MessageBox.Show(Message_Help.NULL_SELECT_PSM);
                return;
            }
            if (this.task_flag != "pLink")
            {
                int all_psms_index = -1;
                for (int i = 0; i < all_psms.Count; ++i)
                {
                    if (selected_psm.Title == all_psms[i].Title)
                    {
                        all_psms_index = i;
                        break;
                    }
                }
                if (ms2_advance_dialog != null)
                    ms2_advance_dialog.Close();
                ms2_advance_dialog = new Ms2_advance(this, all_psms_index);
                ms2_advance_dialog.Show();
            }
            else
            {
                pLink.Ms2_advance ms2_advance_dialog = new pLink.Ms2_advance(this, (pLink.PSM)selected_psm);
                ms2_advance_dialog.Show();
            }
        }

        private void ms2_setting(object sender, RoutedEventArgs e)
        {
            if (this.Model2 == null)
                return;
            if (ms2_setting_dialog != null)
                ms2_setting_dialog.Close();
            ms2_setting_dialog = new Ms2_setting(this);
            ms2_setting_dialog.Show();
        }

        private string GetCurrentDateTime()
        {
            try
            {
                string CurrentDateTime = "";
                DispatcherTimer timer2 = new DispatcherTimer(new TimeSpan(0, 0, 1),
                    DispatcherPriority.Normal,
                    delegate
                    {
                        CurrentDateTime = DateTime.Now.ToString("HH:mm:ss");
                    },
                    this.Dispatcher);

                return CurrentDateTime;
            }
            catch
            {
                return "";
            }
        }
        private void exp_pdf(object sender, RoutedEventArgs e)
        {
            string pdf_name_time = get_time();
            this.Cursor = Cursors.Wait;
            if (sender is Button)
            {
                var sender_obj = sender as Button;
                if (sender_obj == this.export_ms2_pdf_btn)
                    export_PDF(this.selected_psm.Title + "_" + pdf_name_time + ".pdf", this.Model2);
                else if (sender_obj == this.export_ms1_pdf_btn)
                    export_PDF(this.selected_psm.Title + "_MS1_" + pdf_name_time + ".pdf", this.Model1);
                else if (sender_obj == this.export_summary_pdf_btn)
                {
                    this.fdr_btn.Visibility = Visibility.Collapsed;
                    List<string> file_names = new List<string>();
                    file_names.Add(export_summary_PDF("Summary_Result_" + pdf_name_time + ".pdf", this.result_expander, false));
                    file_names.Add(export_summary_PDF("Summary_Param_" + pdf_name_time + ".pdf", this.param_expander, false));
                    file_names.Add(export_summary_PDF("Summary_Graph_" + pdf_name_time + ".pdf", this.graph_expander, false));
                    merge_PDF(file_names, "Summary_");
                    this.fdr_btn.Visibility = Visibility.Visible;
                }
                else if (sender_obj == this.export_result_pdf_btn)
                    export_summary_PDF("Summary_Result_" + pdf_name_time + ".pdf", this.result_expander);
                else if (sender_obj == this.export_param_pdf_btn)
                    export_summary_PDF("Summary_Param_" + pdf_name_time + ".pdf", this.param_expander);
                else if (sender_obj == this.export_graph_pdf_btn)
                {
                    this.fdr_btn.Visibility = Visibility.Collapsed;
                    export_summary_PDF("Summary_Graph_" + pdf_name_time + ".pdf", this.graph_expander);
                    this.fdr_btn.Visibility = Visibility.Visible;
                }
            }
            else if (sender is MenuItem)
            {
                var sender_obj = sender as MenuItem;
                if (sender_obj == this.export_summary_PDF_mi)
                {
                    this.fdr_btn.Visibility = Visibility.Collapsed;
                    List<string> file_names = new List<string>();
                    file_names.Add(export_summary_PDF("Summary_Result_" + pdf_name_time + ".pdf", this.result_expander, false));
                    file_names.Add(export_summary_PDF("Summary_Param_" + pdf_name_time + ".pdf", this.param_expander, false));
                    file_names.Add(export_summary_PDF("Summary_Graph_" + pdf_name_time + ".pdf", this.graph_expander, false));
                    merge_PDF(file_names, "Summary_");
                    this.fdr_btn.Visibility = Visibility.Visible;
                }
                else if (sender_obj == this.export_result_PDF_mi)
                    export_summary_PDF("Result_" + pdf_name_time + ".pdf", this.result_expander);
                else if (sender_obj == this.export_param_PDF_mi)
                    export_summary_PDF("Param_" + pdf_name_time + ".pdf", this.param_expander);
                else if (sender_obj == this.export_graph_PDF_mi)
                {
                    this.fdr_btn.Visibility = Visibility.Collapsed;
                    export_summary_PDF("Graph_" + pdf_name_time + ".pdf", this.graph_expander);
                    this.fdr_btn.Visibility = Visibility.Visible;
                }
            }
            this.Cursor = null;
        }
        private void exp_pdf_Command_Summary(object sender, ExecutedRoutedEventArgs e)
        {
            string pdf_name_time = get_time();
            export_summary_PDF("Summary_" + pdf_name_time + ".pdf", this.all_summary);
        }
        private string get_time()
        {
            string pdf_name_time = DateTime.Now.ToString();
            pdf_name_time = pdf_name_time.Replace(':', '_');
            pdf_name_time = pdf_name_time.Replace('/', '_');
            pdf_name_time = pdf_name_time.Replace(' ', '_');
            return pdf_name_time;
        }
        private void exp_pdf_Command_Result(object sender, ExecutedRoutedEventArgs e)
        {
            string pdf_name_time = get_time();
            export_summary_PDF("Summary_Result_" + pdf_name_time + ".pdf", this.result_expander);
        }

        private void exp_pdf_Command_Param(object sender, ExecutedRoutedEventArgs e)
        {
            string pdf_name_time = get_time();
            export_summary_PDF("Summary_Param_" + pdf_name_time + ".pdf", this.param_expander);
        }

        private void exp_pdf_Command_Graph(object sender, ExecutedRoutedEventArgs e)
        {
            string pdf_name_time = get_time();
            export_summary_PDF("Summary_Graph_" + pdf_name_time + ".pdf", this.graph_expander);
        }
        private string export_summary_PDF(string fileName, Visual visual, bool is_open = true)
        {
            try
            {
                //refresh_mass_error();
                MemoryStream lMemoryStream = new MemoryStream();
                Package package = Package.Open(lMemoryStream, FileMode.Create);
                XpsDocument doc = new XpsDocument(package);
                XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(doc);
                writer.Write(visual); //all_summary
                doc.Close();
                package.Close();
                var pdfXpsDoc = PdfSharp.Xps.XpsModel.XpsDocument.Open(lMemoryStream);
                string folder = task.folder_result_path + File_Help.pBuild_tmp_file;
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                fileName = folder + "\\" + fileName;
                PdfSharp.Xps.XpsConverter.Convert(pdfXpsDoc, fileName, 0);
                if (is_open) //is_open=true,将生成的fileName文件打开，否则，只产生该PDF文件，不打开
                {
                    System.Diagnostics.Process.Start(fileName);
                }
                return fileName;
            }
            catch (Exception exce)
            {
                MessageBox.Show(Message_Help.PDF_OPEN_WRONG + "\r\n" + exce.ToString());
                return "";
            }
        }
        private string export_PDF(string fileName, PlotModel model, double width = 0.0, double height = 0.0, bool is_open_file = true)
        {
            if (model == null)
            {
                MessageBox.Show(Message_Help.MODEL_OPEN_WRONG);
                return "";
            }
            try
            {
                string folder = "";
                if (task != null)
                    folder = task.folder_result_path + File_Help.pBuild_tmp_file;
                else
                    folder = "pLink_tmp";
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                fileName = folder + "\\" + fileName;
                var stream = File.Create(fileName);
                if (width == 0.0)
                    width = model.PlotArea.Width;
                if (height == 0.0)
                    height = model.PlotArea.Height;
                PdfExporter.Export(model, stream, width, height);
                if (is_open_file)
                {
                    System.Diagnostics.Process.Start(fileName);
                }
                return fileName;
                //MessageBox.Show("Save OK!");
            }
            catch (Exception exception)
            {
                MessageBox.Show(Message_Help.PDF_OPEN_WRONG + "\r\n" + exception.ToString());
                return "";
            }
        }
        //显示选中的某一个或者某几个肽段对应的多张谱图
        private void show_spec_clk(object sender, RoutedEventArgs e)
        {
            if ((bool)this.other_psms_cbx.IsChecked)
            {
                MessageBox.Show(Message_Help.OTHER_CHECKED_ERROR);
                return;
            }
            var mi = sender as MenuItem;

            selected_psms.Clear();
            for (int i = 0; i < data.SelectedItems.Count; ++i)
            {
                PSM p = (PSM)data.SelectedItems[i];
                //DataGridRow dr = (DataGridRow)(data.ItemContainerGenerator.ContainerFromItem(p));
                //dr.Background = new SolidColorBrush(Colors.Yellow);
                selected_psms.Add(p);
            }
            ObservableCollection<PSM> tmp_display_psms = new ObservableCollection<PSM>();
            PSM_Filter_Dialog pfd = new PSM_Filter_Dialog();
            if (mi == this.ss)
            {
                for (int i = 0; i < selected_psms.Count; ++i)
                {
                    ObservableCollection<PSM> tmp = pfd.filter_byPeptide(selected_psms[i], psms);
                    for (int j = 0; j < tmp.Count; ++j)
                    {
                        if (!tmp_display_psms.Contains(tmp[j]))
                            tmp_display_psms.Add(tmp[j]);
                    }
                }
            }
            else if (mi == this.ssOnlySQ)
            {
                for (int i = 0; i < selected_psms.Count; ++i)
                {
                    ObservableCollection<PSM> tmp = pfd.filter_bySQ(selected_psms[i], psms);
                    for (int j = 0; j < tmp.Count; ++j)
                    {
                        if (!tmp_display_psms.Contains(tmp[j]))
                            tmp_display_psms.Add(tmp[j]);
                    }
                }
            }
            display_psms = new ObservableCollection<PSM>(tmp_display_psms);
            if (e != null)
            {
                data.ItemsSource = display_psms;
                display_size.Text = display_psms.Count + "";
            }
        }
        //显示当前选中的PSM的相同AC的其余PSMs
        private void show_spec_byAC_clk(object sender, RoutedEventArgs e)
        {
            if (selected_psm == null)
                return;
            string AC = selected_psm.AC;
            ObservableCollection<PSM> psms_tmp = new ObservableCollection<PSM>();
            for (int i = 0; i < this.psms.Count; ++i)
            {
                if (psms[i].AC.Contains(AC))
                {
                    psms_tmp.Add(psms[i]);
                }
            }
            display_psms = psms_tmp;
            this.data.ItemsSource = display_psms;
            this.display_size.Text = display_psms.Count.ToString();
        }
        //删除冗余的肽段，每条肽段只保留最高分的PSM
        private void remove_rdd_clk(object sender, RoutedEventArgs e)
        {
            if ((bool)this.other_psms_cbx.IsChecked)
            {
                this.other_psms_cbx.IsChecked = false;
                other_psms_cbx_clk(null, null);
            }
            if (backgroundWorker.IsBusy || backgroundWorker_fdr.IsBusy)
                return;

            var btn = sender as RadioButton;
            PSM_Filter_Dialog pfd = new PSM_Filter_Dialog();
            ObservableCollection<PSM> tmp_display_psms = new ObservableCollection<PSM>();
            if (btn == this.rr)
                tmp_display_psms = pfd.filter_removeByPeptide(psms);
            else if (btn == this.rrOnlySQ)
                tmp_display_psms = pfd.filter_removeBySQ(psms);
            display_psms = new ObservableCollection<PSM>(tmp_display_psms);
            data.ItemsSource = display_psms;
            display_size.Text = display_psms.Count + "";
        }

        private void ms1_keyDown(object sender, KeyEventArgs e)
        {
            if (!isnot_mgf)
                return;
            string title = selected_psm.Title;
            string raw_name = title.Split('.')[0];
            if (e.Key == Key.Up)
            {
                int scan_num = selected_ms1.Scan - 1;
                while (ms1_scan_hash[(int)(title_hash[raw_name])][scan_num] == null)
                    scan_num--;
                if (scan_num < Min_scan)
                {
                    MessageBox.Show(Message_Help.SCAN_IS_FIRST);
                    return;
                }
                ObservableCollection<PEAK> peaks = new ObservableCollection<PEAK>();
                double tmp_pepmass = 0.0;
                string ms1_pf_file = task.get_pf1_path(raw_name);
                double maxInten = read_peaks(ms1_pf_file, ms1_scan_hash[(int)(title_hash[raw_name])], scan_num, 0, ref peaks, ref tmp_pepmass);
                selected_ms1 = new Spectra_MS1(peaks, scan_num, selected_ms1.Scan_relative - 1, selected_ms1.Pepmass_ms2,
                    selected_ms1.Pepmass_theory, selected_ms1.Charge, maxInten);
                selected_ms1.Retention_Time = tmp_pepmass;

                dis_help = new Display_Help(selected_ms1, this.dis_help.Psm_help, this.dis_help.ddh, dis_help.ddhms1);
                this.DataContext = this;
                double ms1_pepmass = 0.0;
                int frag_index = -1;
                this.Model1 = dis_help.display_ms1(this.Model1.Axes[1].ActualMinimum, this.Model1.Axes[1].ActualMaximum, ref ms1_pepmass, ref frag_index);
                //ms1_draw_speedUp(one_width, this.Model2.Axes[1].ActualMinimum, this.Model2.Axes[1].ActualMaximum);
                this.Model1.Axes[1].AxisChanged += (s, er) =>
                {
                    window_sizeChg_Or_ZommPan_ms1();
                };
                selected_ms1.Pepmass = ms1_pepmass;
                selected_ms1.Fragment_index = frag_index;
                if (this.Model1 != null)
                    this.Model1.RefreshPlot(true);
                data_RD.Height = new GridLength(data_RD.ActualHeight + 1.0e-10, GridUnitType.Pixel);
            }
            else if (e.Key == Key.Down)
            {
                int scan_num = selected_ms1.Scan + 1;
                while (ms1_scan_hash[(int)(title_hash[raw_name])][scan_num] == null)
                    scan_num++;
                if (scan_num > Max_scan)
                {
                    MessageBox.Show(Message_Help.SCAN_IS_LAST);
                    return;
                }
                ObservableCollection<PEAK> peaks = new ObservableCollection<PEAK>();
                double tmp_pepmass = 0.0;
                string ms1_pf_file = task.get_pf1_path(raw_name);
                double maxInten = read_peaks(ms1_pf_file, ms1_scan_hash[(int)(title_hash[raw_name])], scan_num, 0, ref peaks, ref tmp_pepmass);
                selected_ms1 = new Spectra_MS1(peaks, scan_num, selected_ms1.Scan_relative + 1, selected_ms1.Pepmass_ms2, selected_ms1.Pepmass_theory, selected_ms1.Charge, maxInten);
                selected_ms1.Retention_Time = tmp_pepmass;

                dis_help = new Display_Help(selected_ms1, this.dis_help.Psm_help, this.dis_help.ddh, dis_help.ddhms1);
                this.DataContext = this;
                double ms1_pepmass = 0.0;
                int frag_index = -1;
                this.Model1 = dis_help.display_ms1(this.Model1.Axes[1].ActualMinimum, this.Model1.Axes[1].ActualMaximum, ref ms1_pepmass, ref frag_index);
                //ms1_draw_speedUp(one_width, this.Model2.Axes[1].ActualMinimum, this.Model2.Axes[1].ActualMaximum);
                this.Model1.Axes[1].AxisChanged += (s, er) =>
                {
                    window_sizeChg_Or_ZommPan_ms1();
                };
                selected_ms1.Pepmass = ms1_pepmass;
                selected_ms1.Fragment_index = frag_index;
                if (this.Model1 != null)
                    this.Model1.RefreshPlot(true);
                data_RD.Height = new GridLength(data_RD.ActualHeight + 1.0e-10, GridUnitType.Pixel);
            }
        }

        private void ms1_advance(object sender, RoutedEventArgs e)
        {
            if (ms1_advance_dialog != null)
                ms1_advance_dialog.Close();
            ms1_advance_dialog = new MS1_Advance(this);
            ms1_advance_dialog.Show();
        }

        private void ms1_full_size_Clk(object sender, RoutedEventArgs e)
        {
            if (this.Model1 == null)
                return;
            this.Model1.Axes[1].Minimum = this.Model1.Axes[1].AbsoluteMinimum;
            this.Model1.Axes[1].Maximum = this.Model1.Axes[1].AbsoluteMaximum;
            this.Model1.Axes[1].Reset();
            one_width = (this.Model1.Axes[1].ActualMaximum - this.Model1.Axes[1].ActualMinimum) / ((int)this.Model1.Axes[1].ScreenMax.X - (int)this.Model1.Axes[1].ScreenMin.X);
            ms1_draw_speedUp(one_width);
            this.Model1.RefreshPlot(true);
            data_RD.Height = new GridLength(data_RD.ActualHeight + 1.0e-10, GridUnitType.Pixel);
        }

        private void ms1_ini_Clk(object sender, RoutedEventArgs e)
        {
            if (this.Model1 == null)
                return;
            string title = selected_psm.Title;
            string[] strs = title.Split('.');
            string raw_name = strs[0];
            int scan_num = int.Parse(strs[1]);
            int tmp_scan = scan_num - 1;
            while (ms1_scan_hash[(int)(title_hash[raw_name])][tmp_scan] == null)
            {
                tmp_scan--;
            }
            double tmp_pepmass = 0.0;
            ObservableCollection<PEAK> peaks = new ObservableCollection<PEAK>();
            string ms1_pf_file = task.get_pf1_path(raw_name);
            double maxInten = read_peaks(ms1_pf_file, ms1_scan_hash[(int)(title_hash[raw_name])], tmp_scan, 0, ref peaks, ref tmp_pepmass);
            selected_ms1 = new Spectra_MS1(peaks, tmp_scan, 0, selected_ms1.Pepmass_ms2, selected_ms1.Pepmass_theory, selected_ms1.Charge, maxInten);
            selected_ms1.Retention_Time = tmp_pepmass;
            dis_help = new Display_Help(selected_ms1, this.dis_help.Psm_help, this.dis_help.ddh, dis_help.ddhms1);
            this.DataContext = this;
            double ms1_pepmass = 0.0;
            int frag_index = -1;
            if (task.quantification_file == "")
                this.Model1 = dis_help.display_ms1(selected_ms1.Pepmass_ms2 - 3, selected_ms1.Pepmass_ms2 + 3, ref ms1_pepmass, ref frag_index);
            else
            {
                string[] strs1 = selected_psm.Title.Split('.');
                int scan = int.Parse(strs1[1]);
                int pParse_num = int.Parse(strs1[4]);
                List<string> list = new List<string>();
                if (qut_scan_hash[(int)(title_hash[selected_psm.Title.Split('.')[0]]) + "." + scan + "." + pParse_num] != null)
                    File_Help.read_3D_List(task.quantification_file, osType,
                        (int)qut_scan_hash[(int)(title_hash[selected_psm.Title.Split('.')[0]]) + "." + scan + "." + pParse_num], start, end, ref list);
                else
                    return;
                this.function = new Getlist();
                this.function.GetEvidenceFromList(list);
                double min_mz = this.function.evi.TheoricalIsotopicMZList.First().First() - 2.5;
                double max_mz = this.function.evi.TheoricalIsotopicMZList.Last().Last() + 2.5;
                this.Model1 = dis_help.display_ms1(min_mz, max_mz, ref ms1_pepmass, ref frag_index);
            }
            this.Model1.Axes[1].AxisChanged += (s, er) =>
            {
                window_sizeChg_Or_ZommPan_ms1();
            };
            selected_ms1.Pepmass = ms1_pepmass;
            selected_ms1.Fragment_index = frag_index;
            if (this.Model1 != null)
                this.Model1.RefreshPlot(true);
            data_RD.Height = new GridLength(data_RD.ActualHeight + 1.0e-10, GridUnitType.Pixel);
        }

        private void ms2_full_size_Clk(object sender, RoutedEventArgs e)
        {
            if (this.Model2 == null)
                return;
            this.Model2.Axes[1].Reset();
            this.Model2.Axes[3].Reset();
            this.Model2.RefreshPlot(true);
            data_RD.Height = new GridLength(data_RD.ActualHeight + 1.0e-10, GridUnitType.Pixel);
        }


        //3D 图

        private void GenerateBackgroundGrid()
        {

            WireLines background_lines = new WireLines();
            background_lines.Lines = new Point3DCollection();
            int number_background_lines = 5;
            double factor = 11.0 / (double)number_background_lines;
            for (int i = 0; i <= number_background_lines; i++)
            {
                Point3D point1 = new Point3D(-3, i * factor, 0);
                Point3D point2 = new Point3D(14, i * factor, 0);
                background_lines.Lines.Add(point1);
                background_lines.Lines.Add(point2);

                Point3D point3 = new Point3D(14, i * factor, 0);
                Point3D point4 = new Point3D(14, i * factor, 27);
                background_lines.Lines.Add(point3);
                background_lines.Lines.Add(point4);
            }

            Point3D point_a = new Point3D(14, 0, 0);
            Point3D point_b = new Point3D(14, 11, 0);
            background_lines.Lines.Add(point_a);
            background_lines.Lines.Add(point_b);

            Point3D point_c = new Point3D(14, 11, 27);
            Point3D point_d = new Point3D(14, 0, 27);
            background_lines.Lines.Add(point_c);
            background_lines.Lines.Add(point_d);

            viewport3D.Children.Add(background_lines);
            background_lines.Color = Colors.LightGray;
            background_lines.Thickness = 1;


            WireLines axis_lines = new WireLines();
            axis_lines.Lines = new Point3DCollection();
            Point3D point5 = new Point3D(-3, 0, 0);
            Point3D point6 = new Point3D(14, 0, 0);
            Point3D point7 = new Point3D(-3, 11, 0);
            Point3D point8 = new Point3D(-3, 0, 27);
            Point3D point9 = new Point3D(14, 0, 27);
            Point3D point10 = new Point3D(14, 11, 27);
            //axis_lines.Lines.Add(point5);
            //axis_lines.Lines.Add(point6);
            axis_lines.Lines.Add(point5);
            axis_lines.Lines.Add(point7);
            axis_lines.Lines.Add(point5);
            axis_lines.Lines.Add(point8);
            //axis_lines.Lines.Add(point6);
            //axis_lines.Lines.Add(point9);
            axis_lines.Lines.Add(point9);
            axis_lines.Lines.Add(point8);
            //axis_lines.Lines.Add(point9);
            //axis_lines.Lines.Add(point10);
            viewport3D.Children.Add(axis_lines);
        }
        public void AddAxis()
        {
            //Axis Label          
            AxisPoints.Add(new Point3D(12, -2, 29)); //scan
            AxisPoints.Add(new Point3D(-5, 14, -1)); //intensity
            AxisPoints.Add(new Point3D(-4, -2, 10));
            textStyle = axis_canvas.Resources["textStyle"] as Style;
            TextBlock scanText = new TextBlock();
            scanText.Text = "Scan";
            scanText.Style = textStyle;
            axis_canvas.Children.Add(scanText);
            TextBlock intenseText = new TextBlock();
            intenseText.Text = "Intensity";
            intenseText.Style = textStyle;
            axis_canvas.Children.Add(intenseText);
            TextBlock mzText = new TextBlock();
            mzText.Text = "m/z";
            mzText.Style = textStyle;
            axis_canvas.Children.Add(mzText);
        }
        public void AddAxis2()
        {
            TextPoints.Add(new Point3D(-5, 1, -1));
            TextPoints.Add(new Point3D(-5, 6, -1));
            TextPoints.Add(new Point3D(-5, 11, -1));
            TextBlock intenseText1 = new TextBlock();
            intenseText1.Text = (0.0 / intense_factor).ToString("E2");//格式化
            //            intenseText1.Style = textStyle;
            TextBlock intenseText2 = new TextBlock();
            intenseText2.Text = (5.0 / intense_factor).ToString("E2");
            //            intenseText2.Style = textStyle;
            TextBlock intenseText3 = new TextBlock();
            intenseText3.Text = (10.0 / intense_factor).ToString("E2");
            //            intenseText3.Style = textStyle;
            text_canvas.Children.Add(intenseText1);
            text_canvas.Children.Add(intenseText2);
            text_canvas.Children.Add(intenseText3);

            //m/z Axis 有几条曲线
            int t = 0;
            for (int k = 0; k < function.evi.Number; ++k)
            {
                for (int i = 0; i < function.evi.TheoricalIsotopicMZList[k].Count; i++)
                {
                    TextPoints.Add(new Point3D(-4.5, -1, (double)((0.5 + t) * mz_factor)));
                    t++;
                    TextBlock mzTextAxis = new TextBlock();
                    mzTextAxis.RenderTransform = new RotateTransform(-30.0);
                    mzTextAxis.Text = function.evi.TheoricalIsotopicMZList[k][i].ToString("F3");
                    if (i == 0)
                        mzTextAxis.Foreground = new SolidColorBrush(Colors.Red);
                    mzTextAxis.FontSize = 10;
                    mzTextAxis.Style = textStyle;
                    text_canvas.Children.Add(mzTextAxis);
                }
                t++;
            }
            //Scan axis
            TextPoints.Add(new Point3D((function.evi.StartTime + 1) * scan_factor, -1, 27));
            TextPoints.Add(new Point3D((function.evi.EndTime + 1) * scan_factor, -1, 27));

            TextBlock scanBeginText = new TextBlock();
            scanBeginText.Text = function.evi.ScanTime[function.evi.StartTime].ToString();
            text_canvas.Children.Add(scanBeginText);



            TextBlock scanEndText = new TextBlock();
            scanEndText.Text = function.evi.ScanTime[function.evi.EndTime].ToString();
            text_canvas.Children.Add(scanEndText);
        }
        public void TrackMouse()
        {
            _transform = new Transform3DGroup();
            _transform.Children.Add(_scale);
            _transform.Children.Add(new RotateTransform3D(_rotation));
            _transform.Children.Add(_translate);
            camera.Transform = _transform;
            _eventSource = bullet;
            _eventSource.MouseDown += this.OnMouseDown;
            _eventSource.MouseUp += this.OnMouseUp;
            _eventSource.MouseMove += this.OnMouseMove;
            _eventSource.MouseWheel += this.OnMouseWheel;
        }
        #region mouse event
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Zoom(-e.Delta))
            {
                ChangeAxisTextPosition();
                ChangeTextPosition();
            }
        }
        private void ChangeAxisTextPosition()
        {

            for (int i = 0; i < AxisPoints.Count; i++)
            {
                Point screenPoint = new Point(0, 0);
                //screenPoint = Point3DToScreen2D(_p3dCollection[i], viewport3d);
                screenPoint = Point3DTo2D(AxisPoints[i]);
                Canvas.SetLeft(axis_canvas.Children[i], screenPoint.X);
                Canvas.SetTop(axis_canvas.Children[i], screenPoint.Y);
            }

        }

        protected override void OnContentRendered(EventArgs args)
        {
            base.OnContentRendered(args);
            ChangeAxisTextPosition();
            ChangeTextPosition();
        }

        private void ChangeTextPosition()
        {
            for (int i = 0; i < TextPoints.Count; i++)
            {
                Point screenPoint = new Point(0, 0);
                //screenPoint = Point3DToScreen2D(_p3dCollection[i], viewport3d);
                screenPoint = Point3DTo2D(TextPoints[i]);
                Canvas.SetLeft(text_canvas.Children[i], screenPoint.X);
                Canvas.SetTop(text_canvas.Children[i], screenPoint.Y);
            }
        }
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            Mouse.Capture(_eventSource, CaptureMode.Element);
            _previousPosition2D = e.GetPosition(_eventSource);
            _previousPosition3D = ProjectToTrackball(
                _eventSource.ActualWidth,
                _eventSource.ActualHeight,
                _previousPosition2D);
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            Mouse.Capture(_eventSource, CaptureMode.None);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            Point currentPosition = e.GetPosition(_eventSource);

            // Prefer tracking to zooming if both buttons are pressed.
            if (e.RightButton == MouseButtonState.Pressed)
            {
                TranslateTrack(currentPosition);
                ChangeAxisTextPosition();
                ChangeTextPosition();
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                RotateTrack(currentPosition);
                ChangeAxisTextPosition();
                ChangeTextPosition();
            }

            _previousPosition2D = currentPosition;
        }
        private void RotateTrack(Point currentPosition)
        {
            try
            {
                //Vector3D currentPosition3D = ProjectToTrackball(
                //    _eventSource.ActualWidth, _eventSource.ActualHeight, currentPosition);

                //Vector3D axis = Vector3D.CrossProduct(_previousPosition3D, currentPosition3D);
                //double angle = Vector3D.AngleBetween(_previousPosition3D, currentPosition3D);
                //Quaternion delta = new Quaternion(axis, -angle);

                //// Get the current orientation from the RotateTransform3D
                //AxisAngleRotation3D r = _rotation;
                //Quaternion q = new Quaternion(_rotation.Axis, _rotation.Angle);

                //// Compose the delta with the previous orientation
                //q *= delta;

                //// Write the new orientation back to the Rotation3D
                //_rotation.Axis = q.Axis;
                //_rotation.Angle = q.Angle;

                //_previousPosition3D = currentPosition3D;

                Vector3D currentPosition3D = ProjectToTrackball(
                    _eventSource.ActualWidth, _eventSource.ActualHeight, currentPosition);

                Vector3D axisToRotate = new Vector3D(0, 1, 0); // Rotation around Y only 0.5,1,0.5

                Vector3D currProjected = Vector3D.CrossProduct(axisToRotate, currentPosition3D);
                Vector3D prevProjected = Vector3D.CrossProduct(axisToRotate, _previousPosition3D);
                double angle = Vector3D.AngleBetween(currProjected, prevProjected);

                int sign = Math.Sign(Vector3D.DotProduct(
                    axisToRotate,
                    Vector3D.CrossProduct(_previousPosition3D, currentPosition3D)));

                if (sign != 0)
                {
                    Quaternion delta = new Quaternion(axisToRotate * sign, -angle);

                    AxisAngleRotation3D r = _rotation;
                    Quaternion q = new Quaternion(_rotation.Axis, _rotation.Angle);

                    q *= delta;

                    _rotation.Axis = q.Axis;
                    _rotation.Angle = q.Angle;
                }

                _previousPosition3D = currentPosition3D;
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
            }
        }
        private void TranslateTrack(Point currentPosition)
        {
            double TranslateScale = 20;
            var qV = new Quaternion(((_previousPosition2D.X - currentPosition.X) / TranslateScale),
                ((currentPosition.Y - _previousPosition2D.Y) / TranslateScale), 0, 0);

            // Get the current orientantion from the RotateTransform3D
            var q = new Quaternion(_rotation.Axis, _rotation.Angle);
            var qC = q;
            qC.Conjugate();

            // Here we rotate our panning vector about the the rotaion axis of any current rotation transform
            // and then sum the new translation with any exisiting translation
            qV = q * qV * qC;
            _translate.OffsetX += qV.X;
            _translate.OffsetY += qV.Y;
            _translate.OffsetZ += qV.Z;
        }

        private Vector3D ProjectToTrackball(double width, double height, Point point)
        {
            double x = point.X / (width / 2);    // Scale so bounds map to [0,0] - [2,2]
            double y = point.Y / (height / 2);

            x = x - 1;                           // Translate 0,0 to the center
            y = 1 - y;                           // Flip so +Y is up instead of down

            double z2 = 1 - x * x - y * y;       // z^2 = 1 - x^2 - y^2
            double z = z2 > 0 ? Math.Sqrt(z2) : 0;

            return new Vector3D(x, y, z);
        }

        private bool Zoom(double delta)
        {

            double scale = Math.Exp(delta / 5000);    // e^(yDelta/100) is fairly arbitrary.

            _scale.ScaleX *= scale;
            _scale.ScaleY *= scale;
            _scale.ScaleZ *= scale;

            //_translate.OffsetX += 0.1;
            //_translate.OffsetY += 0;
            //_translate.OffsetZ += 0.1;

            if (_scale.ScaleX >= 5.0)
            {
                _scale.ScaleX /= scale;
                _scale.ScaleY /= scale;
                _scale.ScaleZ /= scale;
                return false;
            }
            return true;
        }
        #endregion

        #region transforms

        private Point Point3DTo2D(Point3D point3D)
        {
            Matrix3D m = TryWorldToViewportTransform();
            Point3D transformedPoint = m.Transform(point3D);
            return new Point(transformedPoint.X, transformedPoint.Y);
        }

        private Matrix3D TryWorldToCameraTransform()
        {
            Matrix3D result = Matrix3D.Identity;
            if (_transform == null)
                return result;
            Matrix3D m = _transform.Value;
            m.Invert();//旋转时保持相对位置
            result.Append(m);
            result.Append(MathUtils.GetViewMatrix(camera));
            return result;
        }

        private Matrix3D TryWorldToViewportTransform()
        {
            Matrix3D result = TryWorldToCameraTransform();
            result.Append(MathUtils.GetProjectionMatrix(camera, MathUtils.GetAspectRatio(viewport3D.RenderSize)));
            result.Append(GetHomogeneousToViewportTransform());
            return result;
        }

        private Matrix3D GetHomogeneousToViewportTransform()
        {
            double scaleX = text_canvas.ActualWidth / 2;
            double scaleY = text_canvas.ActualHeight / 2;

            return new Matrix3D(
                 scaleX, 0, 0, 0,
                      0, -scaleY, 0, 0,
                      0, 0, 1, 0,
                scaleX, scaleY, 0, 1);
        }
        #endregion



        private void task_switch(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //判断是哪个任务被选择了，对选择的任务进行加载
            for (int i = 0; i < this.task_treeView.Items.Count; ++i) //穷举所有的任务
            {
                bool isflag = false;
                int selected_index = 0;
                TreeViewItem one_task = (TreeViewItem)this.task_treeView.Items[i];
                if (one_task.IsSelected)
                {
                    StackPanel sp = (StackPanel)one_task.Header;
                    string task_path = ((TextBlock)sp.Children[0]).Text;
                    task_path = task_path.Split('(')[1];
                    task_path = task_path.Split(')')[0];
                    Initial_Task(task_path, selected_index);
                    break;
                }
                for (int j = 0; j < one_task.Items.Count; ++j)
                {
                    TreeViewItem one_task_tab = (TreeViewItem)one_task.Items[j];
                    if (one_task_tab.IsSelected)
                    {
                        isflag = true;
                        selected_index = j;
                        break;
                    }
                }
                if (isflag)
                {
                    StackPanel sp = (StackPanel)one_task.Header;
                    string task_path = ((TextBlock)sp.Children[0]).Text;
                    task_path = task_path.Split('(')[1];
                    task_path = task_path.Split(')')[0];
                    Initial_Task(task_path, selected_index);
                    break;
                }
            }
        }
        //以表格的形式查看二级谱图的匹配情况
        private void ms2_review(object sender, RoutedEventArgs e)
        {
            if (display_ms2_dialog != null)
                display_ms2_dialog.Close();
            display_ms2_dialog = new Display_ms2_table(this);
            display_ms2_dialog.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            display_ms2_dialog.Left = 0;
            display_ms2_dialog.Show();
        }

        private void convert_svg(string old_file_name, string new_file_name)
        {
            StreamReader sr = new StreamReader(old_file_name);
            StreamWriter sw = new StreamWriter(new_file_name);
            double y_height = 0.0;
            const double y_delta = 5.0; //阶梯图中的字母及b往上移的距离
            const double y_y_delta = 3.0; //阶梯图中的y往上移的距离
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine().Trim();
                string new_line = line;
                string[] strs = line.Split(new char[] { '<', '>' }, StringSplitOptions.RemoveEmptyEntries);
                if (strs.Length > 1 && strs[0].StartsWith("text") && strs[0].Contains("dominant-baseline=\"baseline\""))
                {
                    if (strs[1].Length == 1 && strs[1][0] >= 'A' && strs[1][0] <= 'Z')
                    {
                        int index = strs[0].IndexOf("translate(");
                        int start = index + "translate(".Length;
                        string laststr = strs[0].Substring(start);
                        int index2 = laststr.IndexOf(')');
                        string xy_str = laststr.Substring(0, index2);
                        string[] xys = xy_str.Split(',');
                        double y = double.Parse(xys[1]);
                        y -= y_delta;
                        string xy_str_new = xys[0] + "," + y.ToString("F4");
                        new_line = line.Replace(xy_str, xy_str_new);
                    }
                    else if (strs[1].Last() == '+') //阶梯图最前面的电荷值，2+\3+\4+
                    {
                        int index = strs[0].IndexOf("translate(");
                        int start = index + "translate(".Length;
                        string laststr = strs[0].Substring(start);
                        int index2 = laststr.IndexOf(')');
                        string xy_str = laststr.Substring(0, index2);
                        string[] xys = xy_str.Split(',');
                        y_height = double.Parse(xys[1]);
                    }
                    else if (Config_Help.IsIntegerAllowed(strs[1])) //是数字
                    {
                        int index = strs[0].IndexOf("translate(");
                        int start = index + "translate(".Length;
                        string laststr = strs[0].Substring(start);
                        int index2 = laststr.IndexOf(')');
                        string xy_str = laststr.Substring(0, index2);
                        string[] xys = xy_str.Split(',');
                        double y = double.Parse(xys[1]);
                        if (y > y_height) //说明是b的数字
                        {
                            y -= y_delta;
                            string xy_str_new = xys[0] + "," + y.ToString("F4");
                            new_line = line.Replace(xy_str, xy_str_new);
                        }
                        else //说明是y的数字
                        {
                            y -= y_y_delta;
                            string xy_str_new = xys[0] + "," + y.ToString("F4");
                            new_line = line.Replace(xy_str, xy_str_new);
                        }
                    }
                    else if (strs[1] == "b")
                    {
                        int index = strs[0].IndexOf("translate(");
                        int start = index + "translate(".Length;
                        string laststr = strs[0].Substring(start);
                        int index2 = laststr.IndexOf(')');
                        string xy_str = laststr.Substring(0, index2);
                        string[] xys = xy_str.Split(',');
                        double y = double.Parse(xys[1]);
                        y -= y_delta;
                        string xy_str_new = xys[0] + "," + y.ToString("F4");
                        new_line = line.Replace(xy_str, xy_str_new);
                    }
                    else if (strs[1] == "y")
                    {
                        int index = strs[0].IndexOf("translate(");
                        int start = index + "translate(".Length;
                        string laststr = strs[0].Substring(start);
                        int index2 = laststr.IndexOf(')');
                        string xy_str = laststr.Substring(0, index2);
                        string[] xys = xy_str.Split(',');
                        double y = double.Parse(xys[1]);
                        y -= y_y_delta;
                        string xy_str_new = xys[0] + "," + y.ToString("F4");
                        new_line = line.Replace(xy_str, xy_str_new);
                    }
                }
                else if (strs.Length > 1 && strs[0].Contains("rgb(255,255,255)"))
                    new_line = "";
                sw.WriteLine(new_line);
            }
            sr.Close();
            sw.Flush();
            sw.Close();
        }
        private void chk_click(object sender, RoutedEventArgs e)
        {
            if (this.Model1 == null)
                return;
            CheckBox is_switch_cb = sender as CheckBox;
            if (is_switch_cb == this.is_switch)
                is_display_animation = (bool)is_switch_cb.IsChecked;
            one_width = (this.Model1.Axes[1].ActualMaximum - this.Model1.Axes[1].ActualMinimum) / ((int)this.Model1.Axes[1].ScreenMax.X - (int)this.Model1.Axes[1].ScreenMin.X);
            ms1_draw_speedUp(one_width);
        }
        public void export_svg(OxyPlot.Wpf.Plot plot) //保存svg文件，并用专门软件进行打开
        {
            string fileName = "one.svg";
            string folder = File_Help.pBuild_tmp_file;
            if (task != null)
                folder = task.folder_result_path + File_Help.pBuild_tmp_file;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            fileName = folder + "\\" + fileName;
            using (var stream = File.Create(fileName))
            {
                var exporter = new SvgExporter() { Width = plot.ActualWidth, Height = plot.ActualHeight };
                exporter.Export(plot.Model, stream);
            }
            string new_file_name = folder + "\\" + get_time() + ".svg";
            convert_svg(fileName, new_file_name);
            File.Delete(fileName);
            System.Diagnostics.Process.Start("iexplore.exe", new_file_name);


            //var rc = new OxyPlot.Wpf.ShapesRenderContext(null);
            //Clipboard.SetText(plot.Model.ToSvg(plot.ActualWidth, plot.ActualHeight, true, rc)); //拷贝到剪切板，粘贴到WORD全是字符串，只能保存成.svg文件，让专门的软件进行打开
        }
        private void exp_clipboard(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            if (sender is Button)
            {
                Button sender_obj = sender as Button;
                if (sender_obj == this.export_ms2_clipboard_btn)
                {
                    if (this.Model2 != null)
                    {
                        export_svg(this.ms2_plot);
                        EMFCopy.CopyVisualToWmfClipboard((Visual)this.model2_border, Window.GetWindow(this));
                        //还原之前的图，不还原会有一个小bug
                        data_RD.Height = new GridLength(data_RD.ActualHeight + 1.0e-10, GridUnitType.Pixel);
                    }
                    else
                    {
                        MessageBox.Show(Message_Help.MODEL_OPEN_WRONG);
                    }
                }
                else if (sender_obj == this.export_ms1_clipboard_btn)
                {
                    if (this.Model1 != null)
                    {
                        EMFCopy.CopyVisualToWmfClipboard((Visual)this.model1_border, Window.GetWindow(this));
                        //还原之前的图，不还原会有一个小BUG
                        data_RD.Height = new GridLength(data_RD.ActualHeight + 1.0e-10, GridUnitType.Pixel);
                    }
                    else
                    {
                        MessageBox.Show(Message_Help.MODEL_OPEN_WRONG);
                    }
                }
            }
            else
            {
                var sender_obj = sender as MenuItem;
                ExecutedRoutedEventArgs args = e as ExecutedRoutedEventArgs;
                string param = "";
                if (args != null)
                    param = args.Parameter as string;
                if (sender_obj == this.export_fdr_mi || param == "0")
                {
                    EMFCopy.CopyVisualToWmfClipboard((Visual)this.FDR_border, Window.GetWindow(this));
                    frame_right.Width = new GridLength(frame_right.ActualWidth + 1.0e-10, GridUnitType.Pixel);
                }
                else if (sender_obj == this.export_me_Da_mi || param == "1") //先测试Da的图
                {
                    EMFCopy.CopyVisualToWmfClipboard((Visual)this.me_Da_border, Window.GetWindow(this));
                    frame_right.Width = new GridLength(frame_right.ActualWidth + 1.0e-10, GridUnitType.Pixel);
                }
                else if (sender_obj == this.export_me_ppm_mi || param == "2")
                {
                    EMFCopy.CopyVisualToWmfClipboard((Visual)this.me_ppm_border, Window.GetWindow(this));
                    frame_right.Width = new GridLength(frame_right.ActualWidth + 1.0e-10, GridUnitType.Pixel);
                }
                else if (sender_obj == this.export_score_mi || param == "3")
                {
                    EMFCopy.CopyVisualToWmfClipboard((Visual)this.score_border, Window.GetWindow(this));
                    frame_right.Width = new GridLength(frame_right.ActualWidth + 1.0e-10, GridUnitType.Pixel);
                }
                else if (sender_obj == this.export_mix_spectra_mi || param == "4")
                {
                    EMFCopy.CopyVisualToWmfClipboard((Visual)this.mixed_spectra_border, Window.GetWindow(this));
                    frame_right.Width = new GridLength(frame_right.ActualWidth + 1.0e-10, GridUnitType.Pixel);
                }
                else if (sender_obj == this.export_specific_mi || param == "5")
                {
                    EMFCopy.CopyVisualToWmfClipboard((Visual)this.specific_border, Window.GetWindow(this));
                    frame_right.Width = new GridLength(frame_right.ActualWidth + 1.0e-10, GridUnitType.Pixel);
                }
                else if (sender_obj == this.export_modification_mi || param == "6")
                {
                    EMFCopy.CopyVisualToWmfClipboard((Visual)this.modification_border, Window.GetWindow(this));
                    frame_right.Width = new GridLength(frame_right.ActualWidth + 1.0e-10, GridUnitType.Pixel);
                }
                else if (sender_obj == this.export_length_mi || param == "7")
                {
                    EMFCopy.CopyVisualToWmfClipboard((Visual)this.length_border, Window.GetWindow(this));
                    frame_right.Width = new GridLength(frame_right.ActualWidth + 1.0e-10, GridUnitType.Pixel);
                }
                else if (sender_obj == this.export_raw_rate_mi || param == "8")
                {
                    EMFCopy.CopyVisualToWmfClipboard((Visual)this.raw_rate_border, Window.GetWindow(this));
                    frame_right.Width = new GridLength(frame_right.ActualWidth + 1.0e-10, GridUnitType.Pixel);
                }
                else if (sender_obj == this.export_ratio_mi || param == "9")
                {
                    if (task.quantification_file == "")
                    {
                        MessageBox.Show(Message_Help.MODEL_OPEN_WRONG);
                        return;
                    }
                    EMFCopy.CopyVisualToWmfClipboard((Visual)this.quantification_border, Window.GetWindow(this));
                    frame_right.Width = new GridLength(frame_right.ActualWidth + 1.0e-10, GridUnitType.Pixel);
                }
            }
            this.Cursor = null;
        }


        private void protein_txt_sizeChd(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                /*
                this.protein_psms_data.ItemsSource = null;
                Protein selected_protein = (Protein)this.protein_data.SelectedItem;
                if (selected_protein == null)
                    return;
                int alphabet_num_per_row = 60;
                List<Protein_Fragment> fragments = Protein_Help.split(selected_protein, psms, alphabet_num_per_row);
                double width = this.protein_SQ_txt.ActualWidth;
                display_protein_Sq(selected_protein.SQ, fragments, width, alphabet_num_per_row);
                 * */
            }
        }
        private void protein_sch(object sender, SelectionChangedEventArgs e)
        {
            this.selected_size.Text = this.protein_data.SelectedItems.Count.ToString();
            display_protein();
        }
        public void display_protein()
        {
            if (protein_control.SelectedIndex == 0)
                selected_protein = this.protein_data.SelectedItem as Protein;
            else
            {
                Protein_Group group = this.protein_group_data.SelectedItem as Protein_Group;
                selected_protein = group.Protein;
            }
            if (selected_protein == null)
                return;
            int alphabet_num_per_row = protein_panel.pddh.alphabet_num_per_row;
            List<Protein_Fragment> fragments = Protein_Help.split(selected_protein, psms, alphabet_num_per_row);
            selected_protein.fragments = fragments;
            this.protein_coverage_tb.Text = selected_protein.Coverage.ToString("P1");
            double font_size = 15, font_size2 = 12;
            display_protein_Sq(selected_protein, alphabet_num_per_row, ref font_size);
            display_protein_modification_canvas(font_size2);
        }

        //以不同的颜色来显示不同的修饰
        private void display_protein_modification_canvas(double font_size)
        {
            this.protein_SQ_cavas_grid.RowDefinitions.Clear();
            this.protein_SQ_cavas_grid.Children.Clear();
            List<Protein_Mod> modification = selected_protein.modification;
            for (int i = 0; i < modification.Count; ++i)
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = GridLength.Auto;
                this.protein_SQ_cavas_grid.RowDefinitions.Add(rd);
                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                Grid.SetRow(sp, this.protein_SQ_cavas_grid.RowDefinitions.Count - 1);
                this.protein_SQ_cavas_grid.Children.Add(sp);
                Color color = Display_Help.get_brush_color(i);
                Rectangle rect = new Rectangle();
                rect.Fill = new SolidColorBrush(color);
                rect.Height = font_size;
                rect.Width = font_size;
                rect.Margin = new Thickness(5);
                sp.Children.Add(rect);
                Border bd = new Border();
                bd.BorderBrush = new SolidColorBrush(Colors.Blue);
                bd.BorderThickness = new Thickness(0);
                sp.Children.Add(bd);
                TextBlock tb = new TextBlock();
                tb.Text = modification[i].modification;
                tb.Text += ": " + modification[i].mod_count;
                tb.FontSize = font_size;
                tb.Foreground = new SolidColorBrush(color);
                tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                ContextMenu menu = new ContextMenu();
                MenuItem mi = new MenuItem();
                mi.Header = "Show Spectra";
                mi.Click += (s, e) =>
                {
                    string txt = tb.Text;
                    int index = txt.IndexOf(':');
                    string modification_name = txt.Substring(0, index);
                    ObservableCollection<PSM> psms_tmp = new ObservableCollection<PSM>();
                    for (int j = 0; j < selected_protein.psm_index.Count; ++j)
                    {
                        string mod_sits = psms[selected_protein.psm_index[j]].Mod_sites;
                        if (mod_sits.Contains(modification_name))
                        {
                            psms_tmp.Add(psms[selected_protein.psm_index[j]]);
                        }
                    }
                    display_psms = psms_tmp;
                    data.ItemsSource = display_psms;
                    display_size.Text = display_psms.Count().ToString();
                    summary_tab.SelectedIndex = 1;
                };
                menu.Items.Add(mi);
                MenuItem mi2 = new MenuItem();
                mi2.Header = "Copy Modification";
                mi2.Click += (s, e) =>
                {
                    string txt = tb.Text;
                    int index = txt.IndexOf(':');
                    copyToClipboard(txt.Substring(0, index));
                };
                menu.Items.Add(mi2);
                tb.ContextMenu = menu;
                bd.Child = tb;
            }
        }

        private void display_protein_Sq(Protein protein, int num, ref double font_size)
        {
            string sq = protein.SQ;
            List<Protein_Fragment> fragments = protein.fragments;
            num += 20;
            this.protein_SQ_grid.RowDefinitions.Clear();
            this.protein_SQ_grid.Children.Clear();
            double grid_width = this.protein_SQ_grid.ActualWidth;
            FormattedText ft_for_fontSize10 = new FormattedText("A", System.Globalization.CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Courier New"), 10, Brushes.Black);
            double width_for_fontSize10 = ft_for_fontSize10.WidthIncludingTrailingWhitespace;
            //double font_size = 20;
            double font_size0 = (10 * grid_width) / (width_for_fontSize10 * num);
            font_size = font_size0;
            RowDefinition rd_AC = new RowDefinition();
            rd_AC.Height = GridLength.Auto;
            this.protein_SQ_grid.RowDefinitions.Add(rd_AC);
            StackPanel sp_AC = new StackPanel();
            sp_AC.Orientation = Orientation.Horizontal;
            Grid.SetRow(sp_AC, this.protein_SQ_grid.RowDefinitions.Count - 1);
            this.protein_SQ_grid.Children.Add(sp_AC);
            TextBlock tb_AC = new TextBlock();
            tb_AC.Text = ">" + selected_protein.AC;
            tb_AC.FontSize = font_size - 2;
            tb_AC.FontWeight = System.Windows.FontWeights.Bold;
            tb_AC.Foreground = new SolidColorBrush(Colors.Black);
            sp_AC.Children.Add(tb_AC);

            RowDefinition rd_line = new RowDefinition(); //在每行都加入一条横线
            this.protein_SQ_grid.RowDefinitions.Add(rd_line);
            StackPanel sp_line = new StackPanel();
            sp_line.Orientation = Orientation.Horizontal;
            Grid.SetRow(sp_line, this.protein_SQ_grid.RowDefinitions.Count - 1);
            this.protein_SQ_grid.Children.Add(sp_line);
            Canvas canvas = new Canvas();
            canvas.Height = 10;
            canvas.Width = this.protein_SQ_grid.ActualWidth;
            Line line = new Line();
            line.X1 = 0;
            line.Y1 = 5;
            line.X2 = canvas.Width;
            line.Y2 = 5;
            line.StrokeThickness = 1;
            line.Stroke = new SolidColorBrush(Colors.Black);
            canvas.Children.Add(line);
            sp_line.Children.Add(canvas);
            //该层循环表示绘制的行号
            for (int i = 0; i < fragments.Count; ++i)
            {
                RowDefinition rd0 = new RowDefinition();
                rd0.Height = GridLength.Auto;
                this.protein_SQ_grid.RowDefinitions.Add(rd0);
                StackPanel sp0 = new StackPanel();
                sp0.Orientation = Orientation.Horizontal;
                Grid.SetRow(sp0, this.protein_SQ_grid.RowDefinitions.Count - 1);
                this.protein_SQ_grid.Children.Add(sp0);
                List<Interval>[] layer_interval = new List<Interval>[fragments[i].Max_layer + 1];
                for (int j = 1; j <= fragments[i].Max_layer; ++j)
                    layer_interval[j] = new List<Interval>();
                for (int j = 0; j < fragments[i].intervals.Count; ++j)
                {
                    layer_interval[fragments[i].intervals[j].layer_number].Add(fragments[i].intervals[j]);
                }
                TextBlock layer_number_tb0_1 = new TextBlock();
                layer_number_tb0_1.Width = 60;
                layer_number_tb0_1.TextAlignment = TextAlignment.Left;
                layer_number_tb0_1.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                layer_number_tb0_1.Text = "[" + (fragments[i].Start + 1) + "]";
                layer_number_tb0_1.FontSize = 12.0; //font_size * 0.8
                layer_number_tb0_1.Foreground = new SolidColorBrush(Colors.Green);
                sp0.Children.Add(layer_number_tb0_1);
                TextBlock layer0 = new TextBlock();
                layer0.Text = sq.Substring(fragments[i].Start, fragments[i].End - fragments[i].Start + 1);
                layer0.Foreground = new SolidColorBrush(Colors.Black);
                layer0.FontFamily = new FontFamily("Courier New");
                layer0.FontSize = font_size;
                sp0.Children.Add(layer0);
                TextBlock layer_number_tb0_2 = new TextBlock();
                layer_number_tb0_2.Width = layer_number_tb0_1.Width;
                layer_number_tb0_2.TextAlignment = TextAlignment.Right;
                layer_number_tb0_2.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                layer_number_tb0_2.Text = "[" + (fragments[i].End + 1) + "]";
                layer_number_tb0_2.FontSize = layer_number_tb0_1.FontSize;
                layer_number_tb0_2.Foreground = new SolidColorBrush(Colors.Green);
                sp0.Children.Add(layer_number_tb0_2);

                //layer_interval[j][k]表示第j层第k个子序列
                for (int j = 1; j <= fragments[i].Max_layer; ++j)
                {
                    RowDefinition rd = new RowDefinition();
                    rd.Height = GridLength.Auto;
                    this.protein_SQ_grid.RowDefinitions.Add(rd);
                    StackPanel sp = new StackPanel();
                    sp.Orientation = Orientation.Horizontal;
                    Grid.SetRow(sp, this.protein_SQ_grid.RowDefinitions.Count - 1);
                    TextBlock layer_number_tb = new TextBlock();
                    layer_number_tb.Width = layer_number_tb0_1.Width;
                    layer_number_tb.TextAlignment = TextAlignment.Left;
                    layer_number_tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    layer_number_tb.Text = "";
                    layer_number_tb.Foreground = new SolidColorBrush(Colors.Green);
                    sp.Children.Add(layer_number_tb);
                    int start = fragments[i].Start;
                    this.protein_SQ_grid.Children.Add(sp);
                    for (int k = 0; k < layer_interval[j].Count; ++k)
                    {
                        string blank_space_txt = "";
                        for (int p = start; p < layer_interval[j][k].Start; ++p)
                            blank_space_txt += " ";
                        TextBlock layer1 = new TextBlock();
                        layer1.Text = blank_space_txt;
                        layer1.Foreground = new SolidColorBrush(Colors.Black);
                        layer1.FontFamily = new FontFamily("Courier New");
                        layer1.FontSize = font_size;
                        if (blank_space_txt != "")
                            sp.Children.Add(layer1); //一层中非鉴定到的字符串，以空格表示
                        //绘制鉴定到的序列，首先查看是否包含修饰。layer_interval[j][k]表示第j层中的第k个子序列，mods认为是该子序列中的修饰。

                        Border border = new Border();
                        sp.Children.Add(border);
                        border.BorderBrush = new SolidColorBrush(Colors.Blue);
                        border.BorderThickness = new Thickness(0);
                        TextBlock layer2 = new TextBlock();
                        /*
                        layer2.Text = sq.Substring(layer_interval[j][k].Start, layer_interval[j][k].End - layer_interval[j][k].Start + 1);
                        layer2.Foreground = new SolidColorBrush(Colors.Blue);
                        layer2.FontFamily = new FontFamily("Courier New");
                        layer2.FontSize = font_size;
                         */
                        //更新layer2的Text，因为可能有修饰，使用不同的颜色进行标记，那么就需要加入TextBlock.Inlines
                        List<Protein_Modification> mods = layer_interval[j][k].modification;
                        int start2 = layer_interval[j][k].Start;
                        //layer_interval[j][k].row_index = this.protein_SQ_grid.RowDefinitions.Count - 1;
                        //layer_interval[j][k].column_index = sp.Children.Count - 1;
                        for (int p = 0; p < mods.Count; ++p)
                        {
                            //加入鉴定到的非修饰的子序列
                            Run identification_normal = new Run(sq.Substring(start2, mods[p].mod_site - start2));
                            identification_normal.Foreground = new SolidColorBrush(Colors.Blue);
                            identification_normal.FontFamily = new FontFamily("Courier New");
                            identification_normal.FontSize = font_size;
                            identification_normal.MouseDown += (s, e) =>
                            {
                                Thickness thickness_zero = new Thickness(0);
                                for (int q = 0; q < this.protein_SQ_cavas_grid.RowDefinitions.Count; ++q)
                                {
                                    StackPanel sp_tmp0 = this.protein_SQ_cavas_grid.Children[q] as StackPanel;
                                    Border bd = sp_tmp0.Children[1] as Border;
                                    if (bd.BorderThickness != thickness_zero)
                                    {
                                        bd.BorderThickness = new Thickness(0);
                                    }
                                }
                            };
                            if (identification_normal.Text.Length != 0)
                                layer2.Inlines.Add(identification_normal);
                            //加入鉴定到的修饰的子序列，一般为一个字母。
                            Run identification_modification = new Run(sq.Substring(mods[p].mod_site, 1));
                            ToolTip toolTip = new ToolTip();
                            toolTip.Content = mods[p].mod_index;
                            toolTip.Visibility = Visibility.Collapsed;
                            identification_modification.ToolTip = toolTip;
                            identification_modification.Foreground = new SolidColorBrush(Display_Help.get_brush_color(mods[p].mod_index));
                            identification_modification.FontFamily = new FontFamily("Courier New");
                            identification_modification.FontSize = font_size;
                            identification_modification.FontWeight = System.Windows.FontWeights.Bold;
                            identification_modification.MouseDown += (s, e) =>
                            {
                                ToolTip tt = identification_modification.ToolTip as ToolTip;
                                int mod_index = (int)tt.Content;
                                for (int q = 0; q < this.protein_SQ_cavas_grid.RowDefinitions.Count; ++q)
                                {
                                    StackPanel sp_tmp0 = this.protein_SQ_cavas_grid.Children[q] as StackPanel;
                                    Border bd = sp_tmp0.Children[1] as Border;
                                    if (q != mod_index)
                                    {
                                        bd.BorderThickness = new Thickness(0);
                                    }
                                    else
                                    {
                                        bd.BorderThickness = new Thickness(3);
                                        this.protein_SQ_cavas_scroll.ScrollToVerticalOffset(font_size0 * q);
                                    }
                                }
                            };
                            layer2.Inlines.Add(identification_modification);
                            start2 = mods[p].mod_site + 1;
                        }
                        Run identification_normal0 = new Run(sq.Substring(start2, layer_interval[j][k].End - start2 + 1));
                        identification_normal0.Foreground = new SolidColorBrush(Colors.Blue);
                        identification_normal0.FontFamily = new FontFamily("Courier New");
                        identification_normal0.FontSize = font_size;
                        identification_normal0.MouseDown += (s, e) =>
                        {
                            Thickness thickness_zero = new Thickness(0);
                            for (int q = 0; q < this.protein_SQ_cavas_grid.RowDefinitions.Count; ++q)
                            {
                                StackPanel sp_tmp0 = this.protein_SQ_cavas_grid.Children[q] as StackPanel;
                                Border bd = sp_tmp0.Children[1] as Border;
                                if (bd.BorderThickness != thickness_zero)
                                {
                                    bd.BorderThickness = new Thickness(0);
                                }
                            }
                        };
                        if (identification_normal0.Text.Length != 0)
                            layer2.Inlines.Add(identification_normal0);
                        border.Child = layer2;
                        //增加鉴定到的序列的点击事件，将Border加框，方便用户查看当前选中的是哪个序列
                        layer2.MouseDown += (s, e) =>
                        {
                            StackPanel sp1 = layer2.ToolTip as StackPanel;
                            TextBlock tb_tmp2 = sp1.Children[2] as TextBlock;
                            TextBlock tb_tmp3 = sp1.Children[3] as TextBlock;
                            TextBlock tb_tmp4 = sp1.Children[4] as TextBlock;
                            for (int q = 0; q < this.protein_SQ_grid.RowDefinitions.Count; ++q)
                            {
                                StackPanel sp2 = this.protein_SQ_grid.Children[q] as StackPanel;
                                for (int s_num = 0; s_num < sp2.Children.Count; ++s_num)
                                {
                                    if (sp2.Children[s_num] is Border)
                                    {
                                        Border bd = sp2.Children[s_num] as Border;
                                        bd.BorderThickness = new Thickness(0);
                                    }
                                }
                            }
                            string[] strs = tb_tmp3.Text.Split(':');
                            int r_index = int.Parse(strs[0]);
                            int c_index = int.Parse(strs[1]);
                            sp1 = this.protein_SQ_grid.Children[r_index] as StackPanel;
                            Border bd0 = sp1.Children[c_index] as Border;
                            bd0.BorderThickness = new Thickness(1);
                            //往上看一行，看是否由于截断，上面仍然有一个鉴定到的子序列
                            strs = tb_tmp2.Text.Split(':');
                            Interval interval = layer_interval[int.Parse(strs[0])][int.Parse(strs[1])];

                            int fragment_i = int.Parse(tb_tmp4.Text);
                            int old_r_index = r_index;
                            if (interval.Start == fragments[fragment_i].Start && fragment_i > 0)
                            {
                                r_index -= fragments[fragment_i - 1].Max_layer + 2;
                                StackPanel sp2 = this.protein_SQ_grid.Children[r_index] as StackPanel;
                                if (sp2.Children[sp2.Children.Count - 1] is Border)
                                {
                                    Border bd = sp2.Children[sp2.Children.Count - 1] as Border;
                                    bd.BorderThickness = new Thickness(1);
                                }
                            }
                            //往下看一行，看是否由于截断，下面仍然有一个鉴定到的子序列
                            r_index = old_r_index;
                            r_index += fragments[fragment_i].Max_layer + 2;
                            if (interval.End == fragments[fragment_i].End && r_index < this.protein_SQ_grid.Children.Count)
                            {
                                StackPanel sp2 = this.protein_SQ_grid.Children[r_index] as StackPanel;
                                if (1 < sp2.Children.Count && sp2.Children[1] is Border)
                                {
                                    Border bd = sp2.Children[1] as Border;
                                    bd.BorderThickness = new Thickness(1);
                                }
                            }
                        };
                        StackPanel sp_tmp = new StackPanel();
                        TextBlock tb1 = new TextBlock(); //ToolTip第一个为PSM的个数，默认显示状态。
                        tb1.Text = "psms count: " + layer_interval[j][k].Count.ToString();
                        sp_tmp.Children.Add(tb1);
                        TextBlock tb2 = new TextBlock(); //第二个为Protein的个数，默认显示状态。后面三个为隐藏状态。
                        tb2.Text = "proteins count: " + psms[layer_interval[j][k].psms_index[0]].Protein_index.Count.ToString();
                        sp_tmp.Children.Add(tb2);
                        TextBlock tb3 = new TextBlock(); //第三个为第几层中的第几个元素，这里的j和k不表示第几行第几列
                        tb3.Text = j + ":" + k;
                        tb3.Visibility = Visibility.Collapsed;
                        sp_tmp.Children.Add(tb3);
                        TextBlock tb4 = new TextBlock(); //第四个才表示第几行第几列
                        tb4.Text = (this.protein_SQ_grid.RowDefinitions.Count - 1) + ":" + (sp.Children.Count - 1);
                        tb4.Visibility = Visibility.Collapsed;
                        sp_tmp.Children.Add(tb4);
                        TextBlock tb5 = new TextBlock(); //第五个表示fragments中的索引号，表示是第几段，[0-59]表示第一段，[60-119]表示第二段
                        tb5.Text = i + "";
                        tb5.Visibility = Visibility.Collapsed;
                        sp_tmp.Children.Add(tb5);
                        layer2.ToolTip = sp_tmp;
                        layer2.MouseDown += (s, e) =>
                        {
                            if (e.LeftButton == MouseButtonState.Pressed)
                            {

                            }
                            else if (e.RightButton == MouseButtonState.Pressed)
                            {
                                TextBlock tb_tmp0 = s as TextBlock;
                                StackPanel sp0_tmp = tb_tmp0.ToolTip as StackPanel;
                                string[] strs = ((TextBlock)sp0_tmp.Children[2]).Text.Split(':');
                                Interval interval = layer_interval[int.Parse(strs[0])][int.Parse(strs[1])];
                                List<int> psms_index = interval.psms_index;
                                ContextMenu menu = new ContextMenu();
                                MenuItem menuItem0 = new MenuItem();
                                menuItem0.Header = "Show Protein";
                                menuItem0.Click += (s_i, e_i) => //根据PSM对应的索引来查找蛋白质有Bug，因为鉴定蛋白质的顺序已经变化。所以需要根据AC来枚举
                                {
                                    //List<int> protein_index = psms[psms_index[0]].Protein_index;
                                    string[] acs = psms[psms_index[0]].AC.Split(new char[]{'/'}, StringSplitOptions.RemoveEmptyEntries);
                                    ObservableCollection<Protein> display_protein = new ObservableCollection<Protein>();
                                    for (int p = 0; p < acs.Length; ++p)
                                    {
                                        Protein pro = Protein.getProteinByAC(protein_panel.identification_proteins, acs[p]);
                                        if (pro == null)
                                            continue;
                                        display_protein.Add(pro);
                                    }
                                    this.protein_data.ItemsSource = display_protein;
                                    display_size.Text = display_protein.Count.ToString();
                                };
                                menu.Items.Add(menuItem0);
                                MenuItem menuItem1 = new MenuItem();
                                menuItem1.Header = "Show Spectra";
                                menuItem1.Click += (s_i, e_i) =>
                                {
                                    ObservableCollection<PSM> psms_per_oneFragment = new ObservableCollection<PSM>();
                                    for (int p = 0; p < psms_index.Count; ++p)
                                    {
                                        psms_per_oneFragment.Add(psms[psms_index[p]]);
                                    }
                                    display_psms = psms_per_oneFragment;
                                    data.ItemsSource = display_psms;
                                    display_size.Text = display_psms.Count().ToString();
                                    summary_tab.SelectedIndex = 1;
                                };
                                menu.Items.Add(menuItem1);
                                MenuItem menuItem2 = new MenuItem();
                                menuItem2.Header = "Copy Sq";
                                menuItem2.Click += (s_i, e_i) =>
                                {
                                    copyToClipboard(psms[psms_index[0]].Sq);
                                };
                                menu.Items.Add(menuItem2);
                                tb_tmp0.ContextMenu = menu;
                            }
                        };
                        start = layer_interval[j][k].End + 1;
                    }
                    string blank_space_txt1 = "";
                    for (int p = start; p <= fragments[i].End; ++p)
                        blank_space_txt1 += " ";
                    TextBlock layer3 = new TextBlock();
                    layer3.Text = blank_space_txt1;
                    if (blank_space_txt1 != "")
                        sp.Children.Add(layer3);
                    layer3.Foreground = new SolidColorBrush(Colors.Black);
                    layer3.FontFamily = new FontFamily("Courier New");
                    layer3.FontSize = font_size;
                }
                RowDefinition rd_line0 = new RowDefinition(); //在每行都加入一条横线
                this.protein_SQ_grid.RowDefinitions.Add(rd_line0);
                StackPanel sp_line0 = new StackPanel();
                sp_line0.Orientation = Orientation.Horizontal;
                Grid.SetRow(sp_line0, this.protein_SQ_grid.RowDefinitions.Count - 1);
                this.protein_SQ_grid.Children.Add(sp_line0);
                Canvas canvas0 = new Canvas();
                canvas0.Height = 10;
                canvas0.Width = this.protein_SQ_grid.ActualWidth;
                Line line0 = new Line();
                line0.X1 = 0;
                line0.Y1 = 5;
                line0.X2 = canvas0.Width;
                line0.Y2 = 5;
                line0.StrokeThickness = 1;
                line0.Stroke = new SolidColorBrush(Colors.Black);
                canvas0.Children.Add(line0);
                sp_line0.Children.Add(canvas0);
            }
        }

        private void Help_Exit_Clk(object sender, RoutedEventArgs e)
        {
            if (Application.Current != null)
                Application.Current.Shutdown();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Config_Help.psm_filter_bean = new PSM_Filter_Bean(0, "", 0, "", 0, "Show All", "Show All", "Show All", "Show All", "Target", false, false, 1024.0, 0.0, 1.0, 0.0);
            int[] args = (int[])e.Argument;
            if (!read_allINI(this.backgroundWorker))
            {
                e.Cancel = true;
            }
            e.Result = args;
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            int value = e.ProgressPercentage;
            if ((bw == this.backgroundWorker && value == 40) || (bw == this.backgroundWorker_fdr && value == 100))
            {
                this.display_size.Text = psms.Count + "";
            }
            if (bw == this.backgroundWorker)
            {
                switch (value)
                {
                    case 0:
                        this.progressText.Text = Message_Help.LOAD_INI_START;
                        break;
                    case 10:
                        this.progressText.Text = Message_Help.LOAD_PSMS_START;
                        break;
                    case 50:
                        this.progressText.Text = Message_Help.LOAD_SS_START;
                        break;
                    case 70:
                        this.progressText.Text = Message_Help.LOAD_PARSE_QUANT_START;
                        break;
                    case 90:
                        this.progressText.Text = Message_Help.LOAD_RATIO_START;
                        break;
                    case 100:
                        this.progressText.Text = Message_Help.LOAD_CAND_START;
                        break;
                }
            }
            this.progressBar.Value = value;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Cancelled)
                {
                    int[] args0 = new int[2];
                    args0[0] = 0;
                    args0[1] = 1;
                    backgroundWorker.RunWorkerAsync(args0);
                    return;
                }
                if (e.Error != null)
                    return;
                if (this.all_psms.Count == 0)
                {
                    MessageBox.Show("Error");
                    if (Application.Current != null)
                        Application.Current.Shutdown();
                }
                this.Cursor = null;
                this.progressBar.Visibility = Visibility.Collapsed;
                if (summary_tab.SelectedIndex == 0)
                    initial_Summary();
                else if (summary_tab.SelectedIndex == 1)
                {
                    display_psms = null;
                    initial_Peptide();
                }
                else if (summary_tab.SelectedIndex == 2)
                {
                    selected_protein = null;
                    initial_Protein();
                }
                int[] args = (int[])e.Result;
                if (args[1] == 1) //切换任务的时候才会执行
                {
                    int index = (int)args[0];
                    if (index == 0)
                        initial_Summary(); //初始化Summary面板
                    else if (index == 1)
                        initial_Peptide();
                    else if (index == 2)
                        initial_Protein();
                    TabItem one = (TabItem)summary_tab.Items[index];
                    one.IsSelected = true; //显示Summary面板
                }
            }
            catch (Exception exe)
            {
                MessageBox.Show(exe.ToString());
                if (Application.Current != null)
                    Application.Current.Shutdown();
            }
            /* */
        }
        private void backgroundWorker_DoWork_cand(object sender, DoWorkEventArgs e)
        {
            //File_Help.update_Cand_PSMs(task.all_res_files, ref all_psms); //比较慢而且耗内存
        }
        private void backgroundWorker_DoWork_filter(object sender, DoWorkEventArgs e)
        {
            //由于psms列表重新更新，那么里面的过滤查找功能也得重新更新,该速度比较慢
            //updatePSM函数也非常慢，如何加速？这个功能只有在用户进行过滤才会用到，可以考虑先加载完后再使用后台来进行该操作
            File_Help.updatePSM(task.folder_result_path, psms, ref hash_next, ref hash_first, ref hash_next_OnlySQ, ref hash_first_OnlySQ, ref title_index_hash, ref sq_index_hash);
        }
        private void backgroundWorker_RunWorkerCompleted_cand(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!backgroundWorker_filter.IsBusy)
                this.progressText.Text = Message_Help.LOAD_OK;
        }
        private void backgroundWorker_RunWorkerCompleted_filter(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!backgroundWorker_cand.IsBusy)
                this.progressText.Text = Message_Help.LOAD_OK;
        }
        private void display_selected_item_clk(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == this.peptide_display_item_btn)
            {
                object item = this.data.SelectedItem;
                if (item != null)
                    this.data.ScrollIntoView(item);
            }
            else if (btn == this.protein_display_item_btn)
            {
                object item = this.protein_data.SelectedItem;
                if (item != null)
                    this.protein_data.ScrollIntoView(item);
            }
        }

        private void backgroundWorker_DoWork_fdr(object sender, DoWorkEventArgs e)
        {
            double fdr_value = (double)e.Argument;
            update_psms_And_graph(fdr_value, this.backgroundWorker_fdr);
        }

        private void backgroundWorker_RunWorkerCompleted_fdr(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = null;
            Button fdr_btn_tmp = this.fdr_btn.Children[this.fdr_btn.Children.Count - 1] as Button;
            fdr_btn_tmp.IsEnabled = true;
            this.progressBar.Visibility = Visibility.Collapsed;
            //if (summary_tab.SelectedIndex == 0)
            //    initial_Summary();
            display_psms = null;
            selected_protein = null;
            if (summary_tab.SelectedIndex == 1)
            {
                initial_Peptide();
            }
            else if (summary_tab.SelectedIndex == 2)
            {
                initial_Protein();
            }
            MessageBox.Show(Message_Help.FILTER_OK);
        }

        private void switch_welcome_page(object sender, RoutedEventArgs e)
        {
            //MainWindow mainW = new MainWindow();
            //mainW.Show();
            //this.Close();
            this.Close();
            Process.Start("pBuild.exe");
        }

        private void recent_task_menu_mousedown(object sender, MouseButtonEventArgs e)
        {
            Initial_Recent_Task_Menu();
        }

        private void protein_show_all_clk(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Protein> show_proteins = protein_panel.identification_proteins;
            show_proteins = new ObservableCollection<Protein>(show_proteins.OrderByDescending(a => a.psm_index.Count));
            for (int i = 0; i < show_proteins.Count; ++i)
            {
                show_proteins[i].ID = i + 1;
            }
            protein_data.ItemsSource = show_proteins;
            display_size.Text = show_proteins.Count.ToString();
        }

        private void protein_remove_others_clk(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Protein> selected_proteins = new ObservableCollection<Protein>();
            for (int i = 0; i < this.protein_data.SelectedItems.Count; ++i)
            {
                Protein protein = this.protein_data.SelectedItems[i] as Protein;
                selected_proteins.Add(protein);
            }
            this.protein_data.ItemsSource = selected_proteins;
            this.display_size.Text = selected_proteins.Count.ToString();
        }

        private void protein_remove_selected_clk(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Protein> display_proteins = this.protein_data.ItemsSource as ObservableCollection<Protein>;
            ObservableCollection<Protein> selected_proteins = new ObservableCollection<Protein>();
            for (int i = 0; i < this.protein_data.SelectedItems.Count; ++i)
            {
                Protein protein = this.protein_data.SelectedItems[i] as Protein;
                display_proteins.Remove(protein);
            }
            this.protein_data.ItemsSource = display_proteins;
            this.display_size.Text = display_proteins.Count.ToString();
        }


        //显示混合谱图的PSM列表



        //支持批量导出PDF
        private void batch_export_pdf_clk(object sender, RoutedEventArgs e)
        {
            double width = this.Model2.PlotArea.Width;
            double height = this.Model2.PlotArea.Height;
            List<string> files = new List<string>();
            for (int i = 0; i < data.SelectedItems.Count; ++i)
            {
                selected_psm = data.SelectedItems[i] as PSM;
                display_one_psm_ms2();
                string file_path = export_PDF(selected_psm.Title + ".pdf", this.Model2, width, height, false);
                if (file_path != "")
                    files.Add(file_path);
            }
            if (files.Count == 0)
            {
                MessageBox.Show(Message_Help.NULL_SELECT_PSM);
                return;
            }
            merge_PDF(files, "All_Match_");
        }

        //对批量导出的PDF进行合并，并打开该PDF图
        private void merge_PDF(List<string> files, string result_file_name)
        {
            // Open the output document
            PdfDocument outputDocument = new PdfDocument();

            // Iterate files
            foreach (string file in files)
            {
                // Open the document to import pages from it.
                PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import);

                // Iterate pages
                int count = inputDocument.PageCount;
                for (int idx = 0; idx < count; idx++)
                {
                    // Get the page from the external document...
                    PdfPage page = inputDocument.Pages[idx];
                    // ...and add it to the output document.
                    outputDocument.AddPage(page);
                }
            }

            // Save the document...
            string filename = task.folder_result_path + File_Help.pBuild_tmp_file + "\\";
            filename += result_file_name + get_time() + ".pdf";
            outputDocument.Save(filename);
            // ...and start a viewer.
            Process.Start(filename);
        }

        private void protein_advanced_btn_clk(object sender, RoutedEventArgs e)
        {
            if (protein_adanvaced_dialog != null)
                protein_adanvaced_dialog.Close();
            protein_adanvaced_dialog = new Protein_Adanvaced_Dialog(this);
            protein_adanvaced_dialog.Show();
        }


        /*
        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
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
        */

        public void zoom(double min_mz, double max_mz, PlotModel model)
        {
            if (model == null)
                return;
            if (min_mz >= max_mz || min_mz < model.Axes[1].AbsoluteMinimum ||
                max_mz > model.Axes[1].AbsoluteMaximum)
            {
                MessageBox.Show(Message_Help.MZ_INPUT_WRONG);
                return;
            }
            model.Axes[1].Minimum = min_mz;
            model.Axes[1].Maximum = max_mz;
            model.Axes[1].Reset();
            model.RefreshPlot(true);
        }
        private void ms1_setting_Clk(object sender, RoutedEventArgs e)
        {
            if (ms1_setting_dialog != null)
                ms1_setting_dialog.Close();
            ms1_setting_dialog = new MS1_Setting(this);
            ms1_setting_dialog.Show();
        }
        private void check_cand_file() //如果后台在加载候选肽段的时候，第一次加载需要写文件，当写到一半将程序关闭，那么需要将这个“半成品”清除，防止下次读到该“半成品”
        {
            if (backgroundWorker_cand != null && backgroundWorker_cand.IsBusy)
            {
                if (!File_Help.Cand_file_Exist && File.Exists(task.folder_result_path + "\\" + File_Help.pBuild_dat_file))
                {
                    File.Delete(task.folder_result_path + "\\" + File_Help.pBuild_dat_file);
                }
            }
        }
        private void closed_window(object sender, EventArgs e)
        {
            check_cand_file();
            clear_pLink_tempFolder();
            for (int i = 0; i < this.task_treeView.Items.Count; ++i)
            {
                this.clear_tempFolder((TreeViewItem)task_treeView.Items[i]);
            }

            bool wasCodeClosed = new StackTrace().GetFrames().FirstOrDefault(x => x.GetMethod() == typeof(Window).GetMethod("Close")) != null;
            if (!wasCodeClosed) //如果不是代码调用的(this.Close())那么表明是用户点击窗口上的右上角的“X”进行的关闭，则需要将程序关闭
            {
                if (Application.Current != null)
                    Application.Current.Shutdown();
            }
        }

        private void other_psms_cbx_clk(object sender, RoutedEventArgs e)
        {
            if ((bool)this.other_psms_cbx.IsChecked)
            {
                display_psms = new ObservableCollection<PSM>();
                for (int i = 0; i < all_psms.Count; ++i)
                {
                    if (all_psms[i].Q_value > Config_Help.fdr_value)
                        display_psms.Add(all_psms[i]);
                }
                this.data.ItemsSource = display_psms;
                this.display_size.Text = display_psms.Count() + "";
            }
            else
            {
                display_psms = new ObservableCollection<PSM>();
                for (int i = 0; i < all_psms.Count; ++i)
                {
                    if (all_psms[i].Q_value <= Config_Help.fdr_value)
                        display_psms.Add(all_psms[i]);
                }
                this.data.ItemsSource = display_psms;
                this.display_size.Text = display_psms.Count() + "";
            }
        }

        public void compute_btn()
        {
            string mz1_str = this.ms1_mz1_txt.Text;
            string mz2_str = this.ms1_mz2_txt.Text;
            if (!Config_Help.IsDecimalAllowed(mz1_str) || !Config_Help.IsDecimalAllowed(mz2_str))
                return;
            double mz1 = double.Parse(mz1_str);
            double mz2 = double.Parse(mz2_str);
            this.ms1_mz3_txt.Text = (mz1 - mz2).ToString("F5") + "Th/";
            this.ms1_mz3_txt.Text += ((mz1 - mz2) * 1e6 / mz1).ToString("F2") + "ppm";
        }

        private void clear_btn(object sender, RoutedEventArgs e)
        {
            this.ms1_mz1_txt.Text = "NaN";
            this.ms1_mz2_txt.Text = "NaN";
            this.ms1_mz3_txt.Text = "NaN";
            one_width = (this.Model1.Axes[1].ActualMaximum - this.Model1.Axes[1].ActualMinimum) / ((int)this.Model1.Axes[1].ScreenMax.X - (int)this.Model1.Axes[1].ScreenMin.X);
            ms1_draw_speedUp(one_width);
        }
        //根据二级谱图的title来找到pQuant给的结果文件对应的结果。获取理论的m/z及intensity
        //目前已经不需要太多pQuant的结果文件，当然也需要，只需要一级谱扫描号的起始和终止号，而计算的理论同位素峰簇信息均是由emass计算得到
        private Chrome_Help get_theory_iostope()
        {
            Chrome_Help chrome_help = new Chrome_Help();
            string[] strs = selected_psm.Title.Split('.');
            int scan = int.Parse(strs[1]);
            int pParse_num = int.Parse(strs[4]);
            List<string> list = new List<string>();
            List<Theory_IOSTOPE> theory_isotopes = new List<Theory_IOSTOPE>();

            if (qut_scan_hash[(int)(title_hash[selected_psm.Title.Split('.')[0]]) + "." + scan + "." + pParse_num] != null &&
                new_peptide == null)
            {
                File_Help.read_3D_List(task.quantification_file, osType,
                    (int)qut_scan_hash[(int)(title_hash[selected_psm.Title.Split('.')[0]]) + "." + scan + "." + pParse_num], start, end, ref list);
                this.function = new Getlist();
                this.function.GetEvidenceFromList(list);
                this.Evi = this.function.evi;
                chrome_help.start_scan = Evi.ScanTime[Evi.StartTime];
                chrome_help.end_scan = Evi.ScanTime[Evi.EndTime];
                for (int i = 0; i < Evi.TheoricalIsotopicMZList.Count; ++i)
                {
                    Theory_IOSTOPE theory_isotope = new Theory_IOSTOPE();
                    for (int j = 0; j < Evi.TheoricalIsotopicMZList[i].Count; ++j)
                    {
                        theory_isotope.mz.Add(Evi.TheoricalIsotopicMZList[i][j]);
                        theory_isotope.intensity.Add(Evi.TheoricalIsotopicPeakList[i][j]);
                    }
                    theory_isotopes.Add(theory_isotope);
                }
            }
            else
            {
                chrome_help.start_scan = selected_ms1.Scan - 20;
                chrome_help.end_scan = selected_ms1.Scan + 20;

                int label_number0 = 1;
                if (task.has_ratio)
                {
                    if (Config_Help.aas[3, 0] == null)
                        label_number0 = 2;
                    else
                        label_number0 = 3;
                }
                if (new_peptide == null)
                    new_peptide = this.Dis_help.Psm_help.Pep;
                if (task_flag == "pTop")
                {
                    for (int label_number = 1; label_number <= label_number0; ++label_number)
                    {
                        List<int> mod_flag = new List<int>();
                        string modSites = Modification.get_modSites(new_peptide.Mods);
                        Modification.get_modSites_list(modSites, ref mod_flag);
                        SuperAtom superAtom = SuperAtom.ControlMass(new_peptide, label_number, mod_flag);
                        Theory_IOSTOPE theory_isotope = new Theory_IOSTOPE();
                        for (int i = 0; i < superAtom.type; ++i)
                        {
                            if (superAtom.prop[i] >= 0.01)
                            {
                                theory_isotope.mz.Add((superAtom.mass[i] + Config_Help.massZI * selected_ms1.Charge) / selected_ms1.Charge);
                                theory_isotope.intensity.Add(superAtom.prop[i]);
                            }
                        }
                        theory_isotopes.Add(theory_isotope);
                    }
                }
                else if (task_flag == "pLink")
                {
                    if (pLink_result.pLink_label.Flag == 0) //交联剂标记
                    {
                        PSM_Help_2 psm_help_2 = this.Dis_help.Psm_help as PSM_Help_2;
                        string flag = selected_psm.Pep_flag;
                        int xlink_index = int.Parse(flag.Split('|')[1]) - 1;
                        for (int i = 0; i < 2; ++i)
                        {

                            if (i == 1)
                                xlink_index = (xlink_index == 0 ? 1 : 0);
                            SuperAtom superAtom = SuperAtom.ControlMass(psm_help_2.Pep1, psm_help_2.Pep2,
                                pLink_result.Link_Names[xlink_index], psm_help_2.Pep1.Tag_Flag, psm_help_2.Pep2.Tag_Flag);
                            Theory_IOSTOPE theory_isotope = new Theory_IOSTOPE();
                            for (int j = 0; j < superAtom.type; ++j)
                            {
                                if (superAtom.prop[j] >= 0.01)
                                {
                                    theory_isotope.mz.Add((superAtom.mass[j] + Config_Help.massZI * selected_ms1.Charge) / selected_ms1.Charge);
                                    theory_isotope.intensity.Add(superAtom.prop[j]);
                                }
                            }
                            theory_isotopes.Add(theory_isotope);
                        }
                    }
                    else if (pLink_result.pLink_label.Flag == 1) //肽段标记
                    {
                        PSM_Help_2 psm_help_2 = this.Dis_help.Psm_help as PSM_Help_2;
                        string flag = selected_psm.Pep_flag;
                        int xlink_index = int.Parse(flag.Split('|')[1]) - 1;
                        int pep1_index = psm_help_2.Pep1.Tag_Flag;
                        int pep2_index = psm_help_2.Pep2.Tag_Flag;
                        for (int i = 0; i < 2; ++i)
                        {
                            if (i == 1)
                            {
                                pep1_index = (pep1_index == 1 ? 2 : 1);
                                pep2_index = (pep2_index == 1 ? 2 : 1);
                            }
                            SuperAtom superAtom = SuperAtom.ControlMass(psm_help_2.Pep1, psm_help_2.Pep2,
                                    pLink_result.Link_Names[xlink_index], pep1_index, pep2_index);
                            Theory_IOSTOPE theory_isotope = new Theory_IOSTOPE();
                            for (int j = 0; j < superAtom.type; ++j)
                            {
                                if (superAtom.prop[j] >= 0.01)
                                {
                                    theory_isotope.mz.Add((superAtom.mass[j] + Config_Help.massZI * selected_ms1.Charge) / selected_ms1.Charge);
                                    theory_isotope.intensity.Add(superAtom.prop[j]);
                                }
                            }
                            theory_isotopes.Add(theory_isotope);
                        }
                    }
                    else if (pLink_result.pLink_label.Flag == 2) //无标记
                    {
                        PSM_Help_2 psm_help_2 = this.Dis_help.Psm_help as PSM_Help_2;
                        string flag = selected_psm.Pep_flag;
                        int xlink_index = int.Parse(flag.Split('|')[1]) - 1;
                        int pep1_index = psm_help_2.Pep1.Tag_Flag;
                        int pep2_index = psm_help_2.Pep2.Tag_Flag;

                        SuperAtom superAtom = SuperAtom.ControlMass(psm_help_2.Pep1, psm_help_2.Pep2,
                                pLink_result.Link_Names[xlink_index], pep1_index, pep2_index);
                        Theory_IOSTOPE theory_isotope = new Theory_IOSTOPE();
                        for (int j = 0; j < superAtom.type; ++j)
                        {
                            if (superAtom.prop[j] >= 0.01)
                            {
                                theory_isotope.mz.Add((superAtom.mass[j] + Config_Help.massZI * selected_ms1.Charge) / selected_ms1.Charge);
                                theory_isotope.intensity.Add(superAtom.prop[j]);
                            }
                        }
                        theory_isotopes.Add(theory_isotope);
                    }
                }
            }
            chrome_help.theory_isotopes = theory_isotopes;
            chrome_help.index = -1;
            chrome_help.all_index = theory_isotopes[0].mz.Count;

            return chrome_help;
        }
        //一级谱双击的函数，取消该功能，没实现完
        //private void ms1_mouseDouble_clk(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2 && (bool)is_addArrow.IsChecked)
        //    {
        //        List<Theory_IOSTOPE> theory_iostope = get_theory_iostope();
        //        List<PEAK_MS1> ms1_list = new List<PEAK_MS1>();
        //        List<PEAK_MS1> ms2_list = new List<PEAK_MS1>();
        //        update_ms1_masserror(theory_iostope, ref ms1_list, ref ms2_list);
        //        MessageBox.Show(ms1_list.Count + ":" + ms2_list.Count);
        //    }
        //}
        private void ms2_mouseDouble_clk(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                dis_help = new Display_Help(dis_help.Psm_help, dis_help.ddh, dis_help.ddhms1);
                PlotModel model_tmp = null;
                if (dis_help.Psm_help == null)
                    return;
                model_tmp = dis_help.display_ms2(dis_help.Psm_help.Peptide_Number);
                model_tmp.Axes[1].Reset();
                model_tmp.Axes[3].Reset();
                Model_Window model_window = new Model_Window(model_tmp, dis_help, this.model2_border.ActualWidth + 25,
                    this.model2_border.ActualHeight + 25);
                model_window.Width = model_window.width;
                model_window.Height = model_window.height;
                model_window.Show();
            }
        }

        private void pFind_start(object sender, RoutedEventArgs e)
        {
            Process.Start("pFind.exe", task.folder_path);
        }

        private void protein_show_group_protein_clk(object sender, RoutedEventArgs e)
        {
            Protein selected_protein = this.protein_data.SelectedItem as Protein;
            if (selected_protein == null)
                return;
            string parent_protein_ac = selected_protein.Parent_Protein_AC;
            if (parent_protein_ac == "")
            {
                MessageBox.Show(Message_Help.SHOW_GROUP_PROTEIN_WRONG);
                return;
            }
            ObservableCollection<Protein> display_proteins = new ObservableCollection<Protein>();
            string[] parent_acs = parent_protein_ac.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < this.protein_panel.identification_proteins.Count; ++i)
            {
                if (parent_acs.Contains(this.protein_panel.identification_proteins[i].AC))
                {
                    display_proteins.Add(this.protein_panel.identification_proteins[i]);
                }
            }
            this.protein_data.ItemsSource = display_proteins;
            this.display_size.Text = display_proteins.Count.ToString();
        }

        private void protein_copy_group_clk(object sender, RoutedEventArgs e)
        {
            Protein selected_protein = this.protein_data.SelectedItem as Protein;
            if (selected_protein == null)
                return;
            copyToClipboard(selected_protein.Parent_Protein_AC);
        }

        private void toolbar_loaded(object sender, RoutedEventArgs e)
        {
            ToolBar toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
                overflowGrid.Visibility = Visibility.Collapsed;
            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
                mainPanelBorder.Margin = new Thickness(0);
        }

        private void filter_btn_clk(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == this.peptide_filter_btn)
            {
                if (this.task_flag == "pTop" || this.task_flag == "pNovo")
                {
                    if (this.psm_filter_dialog != null)
                        this.psm_filter_dialog.Close();
                    psm_filter_dialog = new PSM_Filter_Dialog(this);
                    psm_filter_dialog.Show();
                }
                else if (this.task_flag == "pLink")
                {
                    if (this.pLink_psm_filter_dialog != null)
                        this.pLink_psm_filter_dialog.Close();
                    pLink_psm_filter_dialog = new pLink.PSM_Filter_Dialog(this);
                    pLink_psm_filter_dialog.Show();
                }
                else if (this.task_flag == "MGF")
                {
                    if (this.mgf_psm_filter_dialog != null)
                        this.mgf_psm_filter_dialog.Close();
                    mgf_psm_filter_dialog = new MGF_PSM_Filter_Dialog(this);
                    mgf_psm_filter_dialog.Show();
                }
            }
            else if (btn == this.protein_filter_btn)
            {
                if (protein_filter_dialog != null)
                    protein_filter_dialog.Close();
                protein_filter_dialog = new Protein_Filter_Dialog(this);
                protein_filter_dialog.Show();
            }
        }

        private void summary_mouseWheel(object sender, MouseWheelEventArgs e)
        {
            summary_scroll.ScrollToVerticalOffset(summary_scroll.VerticalOffset - e.Delta);
        }

        private void Help_Compare_Clk(object sender, RoutedEventArgs e)
        {
            Task_Compare_Tool tct = new Task_Compare_Tool();
            tct.Show();
        }
        private void Help_Compare_Clk2(object sender, RoutedEventArgs e)
        {
            Task_Compare_Tool2 tct = new Task_Compare_Tool2();
            tct.Show();
        }
        //从第几个raw_index中获取PF文件，然后找到scan号对应的一级谱，然后去找到与mz在7ppm之内的谱峰
        private PEAK_MS1 get_ms1_mz(double mz, ref int scan_num, string raw_name, int flag)
        {
            double mass_error = ms1_mass_error;
            if (flag == 0) //往前找
            {
                while (scan_num >= this.Min_scan && ms1_scan_hash[(int)(title_hash[raw_name])][scan_num] == null)
                    scan_num--;
            }
            else if (flag == 1) //往后找
            {
                while (scan_num <= this.Max_scan && ms1_scan_hash[(int)(title_hash[raw_name])][scan_num] == null)
                    scan_num++;
            }
            PEAK_MS1 peak = new PEAK_MS1(mz, 0.0, scan_num);
            if (scan_num < this.Min_scan || scan_num > this.Max_scan)
                return peak;
            ObservableCollection<PEAK> peaks = new ObservableCollection<PEAK>();
            double tmp_pepmass = 0.0;
            string ms1_pf_file = task.get_pf1_path(raw_name);
            double maxInten = read_peaks(ms1_pf_file, ms1_scan_hash[(int)(title_hash[raw_name])], scan_num, 0, ref peaks, ref tmp_pepmass);

            int[] mass_inten = new int[Config_Help.MaxMass];
            for (int k = 0; k < peaks.Count; ++k)
            {
                int massi = (int)peaks[k].Mass;
                mass_inten[massi] = k + 1;
            }
            int currindex = 0;
            for (int k = 0; k < Config_Help.MaxMass; ++k)
            {
                if (mass_inten[k] == 0)
                    mass_inten[k] = currindex;
                else
                    currindex = mass_inten[k];
            }

            int index = IsInWithPPM(mz, mass_inten, mass_error, peaks);
            if (index == -1)
                return peak;
            peak.mz = peaks[index].Mass;
            peak.intensity = peaks[index].Intensity * maxInten / 100;
            return peak;
        }
        private int IsInWithPPM(double mass, int[] mass_inten, double Ppm_mass_error, ObservableCollection<PEAK> peaks)
        {
            int start = (int)(mass - mass * Ppm_mass_error);
            int end = (int)(mass + mass * Ppm_mass_error);
            if (start <= 0 || end >= Config_Help.MaxMass)
                return -1;
            double maxInten = 0.0;
            int max_k = -1;
            for (int k = mass_inten[start - 1]; k < mass_inten[end]; ++k)
            {
                PEAK peak = (PEAK)(peaks[k]);
                double tmpd = System.Math.Abs((peak.Mass - mass) / mass);
                if (tmpd <= Ppm_mass_error && peak.Intensity > maxInten)
                {
                    maxInten = peak.Intensity;
                    max_k = k;
                }
            }
            return max_k;
        }
        //根据一张一级谱中的某根峰的mz来获得前后N张谱的mz的List
        public List<PEAK_MS1> get_List(double mz)
        {
            int scan_num_0 = Display_Detail_Help_MS1.Start_Scan;
            List<PEAK_MS1> mz_list = new List<PEAK_MS1>();
            if (selected_psm == null)
                return mz_list;
            string title = selected_psm.Title;
            string raw_name = title.Split('.')[0];
            int scan_num = selected_ms1.Scan + 1;
            for (int i = 0; i < scan_num_0; ++i)
            {
                --scan_num;
                mz_list.Add(get_ms1_mz(mz, ref scan_num, raw_name, 0));
            }
            mz_list.Reverse(); //将list按Scan从小到大排序
            //后scan_num_0张一级谱
            scan_num = selected_ms1.Scan;
            scan_num_0 = Display_Detail_Help_MS1.End_Scan;
            for (int i = 0; i < scan_num_0; ++i)
            {
                ++scan_num;
                mz_list.Add(get_ms1_mz(mz, ref scan_num, raw_name, 1));
            }
            return mz_list;
        }
        //根据两个质量来绘制色谱曲线
        private void draw_chrom(double mz1, double mz2)
        {

            List<PEAK_MS1> mz1_list = get_List(mz1);
            List<PEAK_MS1> mz2_list = get_List(mz2);

            Display_Help dh = new Display_Help(mz1_list, mz2_list);
            PlotModel model = dh.display_chromatogram(mz1, mz2);
            Model_Window mw = new Model_Window(model, "Chromatogram");
            mw.Show();
        }
        public void pop_up_chrom()
        {
            string mz1_str = ms1_mz1_txt.Text;
            string mz2_str = ms1_mz2_txt.Text;
            if (!Config_Help.IsDecimalAllowed(mz1_str) || !Config_Help.IsDecimalAllowed(mz2_str))
            {
                MessageBox.Show(Message_Help.MZ_BE_DOUBLE);
                return;
            }
            double mz1 = double.Parse(mz1_str);
            double mz2 = double.Parse(mz2_str);

            draw_chrom(mz1, mz2);
        }

        //绘制整个RAW的m/z-scan热度图
        private void export_ms1(object sender, RoutedEventArgs e)
        {
            List<string> raw_names = new List<string>();
            foreach (DictionaryEntry entry in title_hash)
            {
                raw_names.Add((string)entry.Key);
            }
            Model_Window model_w = new Model_Window(raw_names, this, 0);
            model_w.Show();
        }

        private void model_prediction_clk(object sender, RoutedEventArgs e)
        {
            if (selected_psm == null)
            {
                MessageBox.Show(Message_Help.NULL_SELECT_PSM);
                return;
            }
            string title = selected_psm.Title;
            string[] strs = title.Split('.');
            string raw_name = strs[0];
            int scan_num = int.Parse(strs[1]);
            int charge = int.Parse(strs[3]);
            int pre_num = 0;
            if (strs[4] != "dta")
                pre_num = int.Parse(strs[4]);

            string ms2_pf_file = task.get_pf2_path(raw_name);
            ObservableCollection<PEAK> all_peaks = new ObservableCollection<PEAK>();
            double pepmass = 0.0;
            double maxInten = read_peaks(ms2_pf_file, ms2_scan_hash[(int)(title_hash[raw_name])], scan_num, pre_num, ref all_peaks, ref pepmass, ref charge);

            string sq = selected_psm.Sq;
            double tmpmass = 0.0;
            List<ION> ions = new List<ION>();
            List<double> masses = new List<double>();
            List<string> annotations = new List<string>(); //by字符串
            for (int j = 0; j < sq.Length - 1; ++j)
            {
                tmpmass += Config_Help.mass_index[1, sq[j] - 'A']; //轻标
                double bmass = tmpmass + Config_Help.massZI;
                if (ION.addB(ions, bmass, sq, j, 0, aa_int))
                {
                    masses.Add(bmass);
                    annotations.Add("b" + (j + 1));
                }
            }
            tmpmass = 0.0;
            for (int j = sq.Length - 1; j > 0; --j)
            {
                tmpmass += Config_Help.mass_index[1, sq[j] - 'A'];
                double ymass = tmpmass + Config_Help.massZI + Config_Help.massH2O;
                if (ION.addY(ions, ymass, sq, j, 0, aa_int))
                {
                    masses.Add(ymass);
                    annotations.Add("y" + (sq.Length - j));
                }
            }
            List<PEAK> predict_peaks = new List<PEAK>(); //预测的谱峰
            List<PEAK> actual_peaks = new List<PEAK>(); //实际的谱峰

            for (int j = 0; j < ions.Count; ++j)
            {
                double score = 0.0;
                for (int k = 0; k < all_feature.Count; ++k)
                {
                    int feature_id = all_feature[k];
                    if ((feature_id >= 4 && feature_id <= 117) || (feature_id >= 156 && feature_id <= 193) ||
                        (feature_id >= 4 + 193 && feature_id <= 117 + 193) || (feature_id >= 156 + 193 && feature_id <= 193 + 193))
                    {
                        if (ions[j].feature[feature_id] == 1)
                            score += all_aa[feature_id];
                    }
                    else
                    {
                        double value = ions[j].feature[feature_id];
                        int p;
                        for (p = 0; p < all_interv[feature_id].Count; ++p)
                        {
                            if (all_interv[feature_id][p].split >= value)
                                break;
                        }
                        if (p != 0)
                            score += all_interv[feature_id][p - 1].score;
                    }
                }
                predict_peaks.Add(new PEAK(masses[j], score));
            }
            int[] mass_inten = new int[Config_Help.MaxMass];
            for (int k = 0; k < all_peaks.Count; ++k)
            {
                int massi = (int)all_peaks[k].Mass;
                mass_inten[massi] = k + 1;
            }
            int currindex = 0;
            for (int k = 0; k < Config_Help.MaxMass; ++k)
            {
                if (mass_inten[k] == 0)
                    mass_inten[k] = currindex;
                else
                    currindex = mass_inten[k];
            }
            for (int j = 0; j < masses.Count; ++j)
            {
                int index = IsInWithPPM(masses[j], mass_inten, 20e-6, all_peaks);
                if (index != -1)
                    actual_peaks.Add(all_peaks[index]);
                else
                    actual_peaks.Add(new PEAK(masses[j], 0.0));
            }
            Display_Help dh = new Display_Help();
            PlotModel model = dh.display_predict_model(predict_peaks, actual_peaks, annotations);
            Model_Window mw = new Model_Window(model, "Predict Model");
            mw.Show();
        }


        private void range_clk(object sender, RoutedEventArgs e)
        {
            if (mz_input_dialog != null)
                mz_input_dialog.Close();
            mz_input_dialog = new MZ_Input_Dialog(this);
            mz_input_dialog.Show();
        }



        private PlotModel display_me()
        {
            PlotModel model = new PlotModel();
            var linearAxis1 = new LinearAxis();
            linearAxis1.Title = "-log(Score)";
            linearAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis1.TitleFontSize = 15;
            linearAxis1.AxisTitleDistance = 0;
            linearAxis1.IsPanEnabled = false;
            linearAxis1.IsZoomEnabled = false;
            model.Axes.Add(linearAxis1);

            var linearAxis2 = new LinearAxis();
            linearAxis2.Position = AxisPosition.Bottom;
            linearAxis2.Title = "Mass Deviation (ppm)";
            linearAxis2.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis2.TitleFontSize = 15;
            linearAxis2.AxisTitleDistance = 3.0;
            model.Axes.Add(linearAxis2);

            ScatterSeries ss1 = new ScatterSeries();
            ss1.MarkerFill = OxyColors.Blue;
            ss1.MarkerSize = 1.0;
            List<IDataPoint> points1 = new List<IDataPoint>();
            for (int i = 0; i < display_psms.Count; ++i)
            {
                if (display_psms[i].Is_target_flag)
                {
                    points1.Add(new DataPoint(display_psms[i].Delta_mass_PPM, -Math.Log10(display_psms[i].Score)));
                }
            }
            ss1.Points = points1;
            model.Series.Add(ss1);

            ScatterSeries ss2 = new ScatterSeries();
            ss2.MarkerFill = OxyColors.Red;
            ss2.MarkerSize = 1.0;
            List<IDataPoint> points2 = new List<IDataPoint>();
            for (int i = 0; i < display_psms.Count; ++i)
            {
                if (!display_psms[i].Is_target_flag)
                {
                    points1.Add(new DataPoint(display_psms[i].Delta_mass_PPM, -Math.Log10(display_psms[i].Score)));
                }
            }
            ss2.Points = points2;
            model.Series.Add(ss2);
            return model;
        }

        //统计修饰非数的比例
        //private void ana_clk(object sender, RoutedEventArgs e)
        //{
        //    Hashtable all_count = new Hashtable();
        //    Hashtable nan_count = new Hashtable();
        //    for (int i = 0; i < psms.Count; ++i)
        //    {
        //        if (!psms[i].Is_target_flag)
        //            continue;
        //        string mod_site = psms[i].Mod_sites;
        //        string[] strs = mod_site.Split(new char[] { ',', '#', ';' }, StringSplitOptions.RemoveEmptyEntries);
        //        for (int j = 0; j < strs.Length; j += 3)
        //        {
        //            string mm_str = strs[j + 1];
        //            if (all_count[mm_str] == null)
        //                all_count[mm_str] = 1;
        //            else
        //            {
        //                int cc = (int)all_count[mm_str];
        //                all_count[mm_str] = cc + 1;
        //            }
        //            if (psms[i].Ratio <= 0.001 || psms[i].Ratio >= 1024)
        //            {
        //                if (nan_count[mm_str] == null)
        //                    nan_count[mm_str] = 1;
        //                else
        //                {
        //                    int cc = (int)nan_count[mm_str];
        //                    nan_count[mm_str] = cc + 1;
        //                }
        //            }
        //        }
        //    }
        //    StreamWriter sw = new StreamWriter("mod_nan.txt");
        //    string[] keyArray = new string[all_count.Count];
        //    int[] valueArray = new int[all_count.Count];
        //    all_count.Keys.CopyTo(keyArray, 0);
        //    all_count.Values.CopyTo(valueArray, 0);
        //    Array.Sort(valueArray, keyArray);
        //    for (int i = keyArray.Count() - 1; i >= 0; --i)
        //    {
        //        int nan_number = 0, all_number = (int)all_count[keyArray[i]];
        //        if (nan_count[keyArray[i]] != null)
        //            nan_number = (int)nan_count[keyArray[i]];
        //        sw.WriteLine(keyArray[i] + ":" + nan_number + "/" + all_number + "=" + nan_number * 1.0 / all_number);
        //    }
        //    sw.Flush();
        //    sw.Close();
        //}

        private double get_ratio_modification(string modification, ref int fz1, ref int fz2, ref int fm)
        {
            fz1 = 0;
            fz2 = 0;
            fm = 0;
            for (int i = 0; i < psms.Count; ++i)
            {
                if (!psms[i].Is_target_flag)
                    continue;
                if (psms[i].is_Contaminant())
                    continue;
                if (psms[i].Mod_sites.Contains(modification))
                {
                    ++fm;
                    if (psms[i].Ratio <= 0.001)
                        ++fz1;
                    else if (psms[i].Ratio >= 1024)
                        ++fz2;
                }
            }
            return (fz1 + fz2) * 1.0 / fm;
        }
        private double get_ratio_score(double score1, double score2, ref int fz, ref int fm)
        {
            fz = 0;
            fm = 0;
            for (int i = 0; i < psms.Count; ++i)
            {
                if (psms[i].is_Contaminant())
                    continue;
                if (!psms[i].Is_target_flag)
                    continue;
                double score = -Math.Log10(psms[i].Score);
                if (score >= score1 && score < score2)
                {
                    ++fm;
                    if (psms[i].Ratio <= 0.001 || psms[i].Ratio >= 1024)
                        ++fz;
                }
            }
            return fz * 1.0 / fm;
        }
        private double get_ratio_mass(double mass1, double mass2, ref int fz, ref int fm)
        {
            fz = 0;
            fm = 0;
            for (int i = 0; i < psms.Count; ++i)
            {
                if (psms[i].is_Contaminant())
                    continue;
                if (!psms[i].Is_target_flag)
                    continue;
                if (psms[i].Delta_mass_PPM >= mass1 && psms[i].Delta_mass_PPM < mass2)
                {
                    ++fm;
                    if (psms[i].Ratio <= 0.001 || psms[i].Ratio >= 1024)
                        ++fz;
                }
            }
            return fz * 1.0 / fm;
        }


        private void mod_ana_clk(object sender, RoutedEventArgs e)
        {
            StreamWriter sw_mod = new StreamWriter("Statistical_modification.txt");
            StreamWriter sw_mod2 = new StreamWriter("Statistical_modificatin2.txt");
            StreamWriter sw_spec3 = new StreamWriter("Statistical_specific.txt");
            StreamWriter sw_me = new StreamWriter("Statistical_masserror.txt");
            StreamWriter sw_all = new StreamWriter("Statistical_all.txt");
            StreamWriter sw_low_high = new StreamWriter("Statistical_low_high.txt");
            StreamWriter sw_raw = new StreamWriter("Statistical_raw.txt");
            StreamWriter sw_score = new StreamWriter("Statistical_score.txt");
            StreamWriter sw_missing_enzyme = new StreamWriter("Statistical_missing_enzyme.txt");
            List<Identification_Modification> modification = this.summary_result_information.modifications;
            List<int> nan_count = new List<int>();
            List<int> all_count = new List<int>();
            List<string> mod_name = new List<string>();
            List<int> fzs = new List<int>();
            List<int> fzs2 = new List<int>();
            List<int> fms = new List<int>();
            List<string> titles = new List<string>();
            foreach (string key in this.title_hash.Keys)
            {
                fzs.Add(0);
                fzs2.Add(0);
                fms.Add(0);
                titles.Add("");
            }
            foreach (string key in this.title_hash.Keys)
            {
                titles[(int)this.title_hash[key]] = key;
            }
            for (int i = 0; i < modification.Count; ++i)
            {
                int fz11 = 0, fz12 = 0, fm = 0;
                get_ratio_modification(modification[i].modification_name, ref fz11, ref fz12, ref fm);
                nan_count.Add((fz11 + fz12));
                all_count.Add(fm);
                mod_name.Add(modification[i].modification_name);
                double ratio = (fz11 + fz12) * 1.0 / fm;
                sw_mod.WriteLine(modification[i].modification_name + "\t(" + fz11 + " + " + fz12 + ") / " + fm + " = " + ratio.ToString("F3"));
            }
            Display_Help dh = new Display_Help();
            if (!Config_Help.IsIntegerAllowed(this.mod_number_tb.Text))
                return;


            int fz1 = 0, fm1 = 0, fz2 = 0, fm2 = 0, fz3 = 0, fm3 = 0, fz4 = 0, fm4 = 0;
            for (int i = 0; i < psms.Count; ++i)
            {
                if (!psms[i].Is_target_flag)
                    continue;
                if (psms[i].is_Contaminant())
                    continue;
                if (psms[i].Mod_sites == "")
                {
                    ++fm1;
                    if (psms[i].Ratio <= 0.001 || psms[i].Ratio >= 1024)
                        ++fz1;
                }
                else
                {
                    ++fm2;
                    if (psms[i].Ratio <= 0.001 || psms[i].Ratio >= 1024)
                        ++fz2;
                }
                int index = (int)this.title_hash[psms[i].Title.Split('.')[0]];
                ++fms[index];
                if (psms[i].Ratio <= 0.001)
                    ++fzs[index];
                else if (psms[i].Ratio >= 1024)
                    ++fzs2[index];
            }
            sw_mod2.WriteLine("Non-modification\t" + fz1 + " / " + fm1 + " = " + (fz1 * 1.0 / fm1).ToString("F3"));
            sw_mod2.WriteLine("Modification\t" + fz2 + " / " + fm2 + " = " + (fz2 * 1.0 / fm2).ToString("F3"));
            fz1 = 0; fz2 = 0; fz3 = 0; fm1 = 0; fm2 = 0; fm3 = 0; fz4 = 0; fm4 = 0;
            for (int i = 0; i < psms.Count; ++i)
            {
                if (!psms[i].Is_target_flag)
                    continue;
                if (psms[i].is_Contaminant())
                    continue;
                if (psms[i].Specific_flag == 'S')
                {
                    ++fm1;
                    if (psms[i].Ratio <= 0.001 || psms[i].Ratio >= 1024)
                        ++fz1;
                }
                else if (psms[i].Specific_flag == 'N')
                {
                    ++fm2;
                    if (psms[i].Ratio <= 0.001 || psms[i].Ratio >= 1024)
                        ++fz2;
                }
                else if (psms[i].Specific_flag == 'C')
                {
                    ++fm3;
                    if (psms[i].Ratio <= 0.001 || psms[i].Ratio >= 1024)
                        ++fz3;
                }
                else
                {
                    ++fm4;
                    if (psms[i].Ratio <= 0.001 || psms[i].Ratio >= 1024)
                        ++fz4;
                }
            }
            sw_spec3.WriteLine("Specific\t" + fz1 + " / " + fm1 + " = " + (fz1 * 1.0 / fm1).ToString("F3"));
            sw_spec3.WriteLine("N-SemiSpecific\t" + fz2 + " / " + fm2 + " = " + (fz2 * 1.0 / fm2).ToString("F3"));
            sw_spec3.WriteLine("C-SemiSpecific\t" + fz3 + " / " + fm3 + " = " + (fz3 * 1.0 / fm3).ToString("F3"));
            sw_spec3.WriteLine("NonSpecific\t" + fz4 + " / " + fm4 + " = " + (fz4 * 1.0 / fm4).ToString("F3"));

            double start = -20, delta_mass = 2;
            for (; start <= 20; )
            {
                int fz = 0, fm = 0;
                get_ratio_mass(start, start + delta_mass, ref fz, ref fm);
                sw_me.WriteLine("[" + start + " , " + (start + delta_mass) + ")\t" + fz + " / " + fm + " = " + (fz * 1.0 / fm).ToString("F3"));
                start += delta_mass;
            }
            start = 0; double delta_score = 1;
            for (; start <= 20; )
            {
                int fz = 0, fm = 0;
                double ratio = get_ratio_score(start, start + delta_score, ref fz, ref fm);
                sw_score.WriteLine("[" + start + ", " + (start + delta_score) + ")\t" + fz + " / " + fm + " = " + ratio.ToString("F3"));
                start += delta_score;
            }
            fz1 = 0; fm1 = 0;
            for (int i = 0; i < psms.Count; ++i)
            {
                if (!psms[i].Is_target_flag)
                    continue;
                if (psms[i].is_Contaminant())
                    continue;
                ++fm1;
                if (psms[i].Ratio <= 0.001 || psms[i].Ratio >= 1024)
                    ++fz1;
            }
            sw_all.WriteLine("NaN\t" + fz1 + " / " + fm1 + " = " + (fz1 * 1.0 / fm1).ToString("F3"));

            fz1 = 0; fm1 = 0; fz2 = 0; fm2 = 0;
            for (int i = 0; i < psms.Count; ++i)
            {
                if (!psms[i].Is_target_flag)
                    continue;
                if (psms[i].is_Contaminant())
                    continue;
                if (psms[i].Pep_flag == "1")
                {
                    ++fm1;
                    if (psms[i].Ratio <= 0.001 || psms[i].Ratio >= 1024)
                        ++fz1;
                }
                else if (psms[i].Pep_flag == "2")
                {
                    ++fm2;
                    if (psms[i].Ratio <= 0.001 || psms[i].Ratio >= 1024)
                        ++fz2;
                }
            }
            sw_low_high.WriteLine("Low\t" + fz1 + " / " + fm1 + " = " + (fz1 * 1.0 / fm1).ToString("F3"));
            sw_low_high.WriteLine("High\t" + fz2 + " / " + fm2 + " = " + (fz2 * 1.0 / fm2).ToString("F3"));
            for (int i = 0; i < fzs.Count; ++i)
            {
                sw_raw.WriteLine(titles[i] + "\t( " + fzs[i] + " + " + fzs2[i] + " ) / " + fms[i] + " = " + ((fzs[i] + fzs2[i]) * 1.0 / fms[i]).ToString("F3"));
            }
            int[] miss_fm = new int[10];
            int[] miss_fz = new int[10];
            for (int i = 0; i < psms.Count; ++i)
            {
                if (!psms[i].Is_target_flag)
                    continue;
                if (psms[i].is_Contaminant())
                    continue;
                if (psms[i].Missed_cleavage_number >= miss_fm.Length)
                    continue;
                ++miss_fm[psms[i].Missed_cleavage_number];
                if (psms[i].Ratio <= 0.001 || psms[i].Ratio >= 1024)
                    ++miss_fz[psms[i].Missed_cleavage_number];
            }
            for (int i = 0; i < miss_fm.Length; ++i)
            {
                sw_missing_enzyme.WriteLine(i + "\t" + miss_fz[i] + " / " + miss_fm[i] + " = " + (miss_fz[i] * 1.0 / miss_fm[i]).ToString("P2"));
            }
            sw_mod.Flush();
            sw_mod.Close();
            sw_mod2.Flush();
            sw_mod2.Close();
            sw_spec3.Flush();
            sw_spec3.Close();
            sw_me.Flush();
            sw_me.Close();
            sw_all.Flush();
            sw_all.Close();
            sw_low_high.Flush();
            sw_low_high.Close();
            sw_raw.Flush();
            sw_raw.Close();
            sw_score.Flush();
            sw_score.Close();
            sw_missing_enzyme.Flush();
            sw_missing_enzyme.Close();

            PlotModel model = dh.display_modification_nan_rate(nan_count, all_count, mod_name, int.Parse(this.mod_number_tb.Text));
            Model_Window mw = new Model_Window(model, "Modification Nan Rate");
            mw.Show();

            StreamWriter sw_mod_lh = new StreamWriter("Statistical_modificatin_LH.txt");
            Hashtable hhh = new Hashtable();
            for (int i = 0; i < psms.Count; ++i)
            {
                if (!psms[i].Is_target_flag)
                    continue;
                if (psms[i].is_Contaminant())
                    continue;
                if (psms[i].Pep_flag == "0" || psms[i].Pep_flag == "1")
                    continue;
                string mods = psms[i].Mod_sites;
                if (mods == "")
                    continue;
                string[] strs = mods.Split(new char[] { ',', '#', ';' }, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 1; j < strs.Length; j = j + 3)
                {
                    string modnn = strs[j];
                    if (hhh[modnn] == null)
                    {
                        int[] aas = new int[3];
                        aas[int.Parse(strs[j + 1])] = 1;
                        hhh[modnn] = aas;
                    }
                    else
                    {
                        int[] aas = hhh[modnn] as int[];
                        ++aas[int.Parse(strs[j + 1])];
                        hhh[modnn] = aas;
                    }
                }
            }
            foreach (string key in hhh.Keys)
            {
                int[] aas = hhh[key] as int[];
                sw_mod_lh.WriteLine(key + ":\t" + aas[0] + "\t" + aas[1] + "\t" + aas[2]);
            }
            sw_mod_lh.Flush();
            sw_mod_lh.Close();
        }

        private void ana_clk(object sender, RoutedEventArgs e)
        {
            StreamReader sr = new StreamReader("ppm_nan.txt");
            FileStream fs = new FileStream("ppm_nan2.txt", FileMode.OpenOrCreate);
            StreamWriter sw = new StreamWriter(fs, Encoding.Default);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                string[] strs = line.Split('\t');
                double mass1 = double.Parse(strs[0]);
                double mass2 = double.Parse(strs[1]);
                if (mass1 > 20 || mass1 < -20)
                    mass1 = 20;
                if (mass2 > 20 || mass2 < -20)
                    mass2 = 20;
                sw.WriteLine(mass1 + "\t" + mass2);
            }
            sr.Close();
            sw.Flush();
            sw.Close();
            fs.Close();
        }
        private void update_mix(ObservableCollection<PSM> psms, ref Hashtable mix_all, ref Hashtable mix_nan)
        {
            System.Collections.Hashtable title_hash = new Hashtable();
            System.Collections.Hashtable nan_hash = new Hashtable();
            for (int i = 0; i < psms.Count; ++i)
            {
                string scan_number = psms[i].Title.Split('.')[1];
                if (title_hash[scan_number] == null)
                    title_hash[scan_number] = 1;
                else
                {
                    int number = (int)title_hash[scan_number];
                    title_hash[scan_number] = number + 1;
                }
                if (psms[i].Ratio != 0.0 && (psms[i].Ratio >= 1024.0 || psms[i].Ratio <= 0.001))
                    nan_hash[scan_number] = 1;
            }
            foreach (DictionaryEntry di in title_hash)
            {
                int number = (int)di.Value;
                string scan_number = (string)di.Key;
                if (mix_all[number] == null)
                    mix_all[number] = 1;
                else
                {
                    int nn = (int)mix_all[number];
                    mix_all[number] = nn + 1;
                }
                if (nan_hash[scan_number] != null)
                {
                    if (mix_nan[number] == null)
                        mix_nan[number] = 1;
                    else
                    {
                        int nn = (int)mix_nan[number];
                        mix_nan[number] = nn + 1;
                    }
                }
            }
        }
        private void update_mod(ObservableCollection<PSM> psms, ref Hashtable mod)
        {
            for (int i = 0; i < psms.Count; ++i)
            {
                System.Collections.Hashtable mod_tmp = new Hashtable();
                ObservableCollection<Modification> modification = Modification.get_modSites(psms[i].Mod_sites);
                for (int j = 0; j < modification.Count; ++j)
                {
                    if (mod_tmp[modification[j].Mod_name] == null)
                    {
                        mod_tmp[modification[j].Mod_name] = 1;
                        if (mod[modification[j].Mod_name] == null)
                            mod[modification[j].Mod_name] = 1;
                        else
                        {
                            int number = (int)mod[modification[j].Mod_name];
                            mod[modification[j].Mod_name] = number + 1;
                        }
                    }
                }
            }
        }
        //private void ana_clk(object sender, RoutedEventArgs e)
        //{
        //    PSM old_selected_psm = selected_psm;
        //    display_psms = new ObservableCollection<PSM>();
        //    for (int i = 0; i < psms.Count; ++i)
        //    {
        //        if (psms[i].Ratio <= 0.001 || psms[i].Ratio >= 1024)
        //        {
        //            display_psms.Add(psms[i]);
        //        }
        //    }
        //    data.ItemsSource = display_psms;
        //    display_size.Text = display_psms.Count + "";

        //    selected_psm = old_selected_psm;


        //    StreamWriter sw = new StreamWriter("C:\\Users\\dell\\Desktop\\N15_ana.txt");

        //    string title1 = display_psms[0].Title;
        //    string raw_name = title1.Split('.')[0];
        //    const double N = Config_Help.massN15 - Config_Help.massN;
        //    const int TOP = 5;
        //    for (int i = 0; i < display_psms.Count; ++i)
        //    {
        //        int len = display_psms[i].Sq.Length;
        //        int charge = display_psms[i].Charge;
        //        int scan_num = int.Parse(display_psms[i].Title.Split('.')[1]);
        //        List<MS1_Pair_Peak> pairs = new List<MS1_Pair_Peak>();
        //        double mgf_mass = display_psms[i].Spectra_mass; //残基和+H2O+质子
        //        mgf_mass = (mgf_mass + (charge - 1) * Config_Help.massZI) / charge; //残基和+H2O
        //        PEAK_MS1 ori_peak = get_ms1_mz(mgf_mass, ref scan_num, raw_name, 0, 30e-6);
        //        for (int number = len - 6; number <= 2 * len; ++number) //[len - 6,2 * len]
        //        {
        //            double mass_deviation = number * N / charge;
        //            PEAK_MS1 pair_peak = get_ms1_mz(mgf_mass + mass_deviation, ref scan_num, raw_name, 0, 30e-6);
        //            if (pair_peak.mz != 0.0)
        //                pairs.Add(new MS1_Pair_Peak(new PEAK(ori_peak.mz, ori_peak.intensity), new PEAK(pair_peak.mz, pair_peak.intensity), mass_deviation, number));
        //            PEAK_MS1 pair_peak2 = get_ms1_mz(mgf_mass - mass_deviation, ref scan_num, raw_name, 0, 30e-6);
        //            if (pair_peak2.mz != 0.0)
        //                pairs.Add(new MS1_Pair_Peak(new PEAK(ori_peak.mz, ori_peak.intensity), new PEAK(pair_peak2.mz, pair_peak2.intensity), mass_deviation, number));
        //        }
        //        pairs.Sort();
        //        string line = display_psms[i].Title;
        //        for (int j = 0; j < TOP && j < pairs.Count; ++j)
        //        {
        //            line += "\t" + pairs[j].Pair_Peak.Mass.ToString("F5") + "," + pairs[j].N_Number;
        //        }
        //        sw.WriteLine(line);
        //    }
        //    sw.Flush();
        //    sw.Close();

        //    //对非数的PSM进行各修饰个数的统计
        //    //Hashtable mod_hash = new Hashtable();
        //    //for (int i = 0; i < display_psms.Count; ++i)
        //    //{
        //    //    string mods_str = display_psms[i].Mod_sites;
        //    //    string[] all_strs = mods_str.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
        //    //    for (int j = 1; j < all_strs.Length; j = j + 2)
        //    //    {
        //    //        if (mod_hash[all_strs[j]] == null)
        //    //            mod_hash[all_strs[j]] = 1;
        //    //        else
        //    //        {
        //    //            int num = (int)mod_hash[all_strs[j]];
        //    //            mod_hash[all_strs[j]] = num + 1;
        //    //        }
        //    //    }
        //    //}
        //    //List<string> mods = mod_hash.Keys as List<string>;
        //    //StreamWriter sw1 = new StreamWriter(task.folder_result_path + "mod_hash.txt");
        //    //foreach (DictionaryEntry d in mod_hash)
        //    //{
        //    //    sw1.WriteLine(d.Key + "\t" + d.Value);
        //    //}
        //    //sw1.Flush();
        //    //sw1.Close();
        //    //将所有非数比值全部拿出来

        //    //StreamWriter sw = new StreamWriter("ana_result.txt");
        //    //for (int i = 0; i < titles.Count; ++i)
        //    //{
        //    //    sw.WriteLine(titles[i]);
        //    //    for (int k = 0; k < 13; ++k)
        //    //    {
        //    //        string line = k + "," + masses[i][k] + ":";
        //    //        for (int j = 0; j < all_peaks[i][k].Count; ++j)
        //    //        {
        //    //            line += all_peaks[i][k][j].intensity + " ";
        //    //        }
        //    //        sw.WriteLine(line);
        //    //    }
        //    //}
        //    //sw.Flush();
        //    //sw.Close();
        //}

        private void config_clk(object sender, RoutedEventArgs e)
        {
            Config_Dialog cd = new Config_Dialog(this);
            cd.Show();
        }

        private void chrom_ctrl_C_keyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && (Keyboard.Modifiers & (ModifierKeys.Control)) == (ModifierKeys.Control))
            {
                this.chrom_border.Background = new SolidColorBrush(Colors.Transparent);
                EMFCopy.CopyUIElementToClipboard(this.chrom_border);
            }
        }

        //配置二级谱的定量:比如N端分别加多少，C端分别加多少
        private void ms2_config_btn_clk(object sender, RoutedEventArgs e)
        {
            if (ms2_quant_config_dialog != null)
                ms2_quant_config_dialog.Close();
            ms2_quant_config_dialog = new MS2_Quant_Config_Dialog(this);
            ms2_quant_config_dialog.Show();
        }
        //根据psm来获取对应的谱图，包括谱图中的所有谱峰
        private Spectra getSpectra_byPSM(PSM psm)
        {
            string title = psm.Title;
            string[] strs = title.Split('.');
            string raw_name = strs[0];
            int scan_num = int.Parse(strs[1]);
            int charge = int.Parse(strs[3]);
            int pre_num = 0;
            if (strs[4] != "dta")
                pre_num = int.Parse(strs[4]);
            string ms2_pf_file = task.get_pf2_path(raw_name);
            ObservableCollection<PEAK> peaks = new ObservableCollection<PEAK>();
            double pepmass = 0.0;
            double maxInten = read_peaks(ms2_pf_file, ms2_scan_hash[(int)(title_hash[raw_name])], scan_num, pre_num, ref peaks, ref pepmass, ref charge);
            return new Spectra(title, charge, pepmass, peaks, maxInten);
        }
        private List<List<PEAK>> get_pair(PSM psm, ref List<string> annotations)
        {
            Peptide pep = new Peptide(psm.Sq);
            double pep_theory_mass = 0.0;
            pep.Mods = Modification.get_modSites(psm.Sq, psm.Mod_sites, int.Parse(psm.Pep_flag), ref pep_theory_mass);
            Spectra spc = getSpectra_byPSM(psm);

            PSM_Help psm_help = new PSM_Help(task.HCD_ETD_Type);
            psm_help.Pep = pep;
            psm_help.Spec = spc;
            psm_help.Ppm_mass_error = this.ms2_quant_help.Ppm_mass_error;
            psm_help.Da_mass_error = this.ms2_quant_help.Da_mass_error;
            //psm_help.aa_index = int.Parse(psm.Pep_flag);
            psm_help.Pep.Tag_Flag = int.Parse(psm.Pep_flag);
            List<List<PEAK>> peaks = null;
            if (psm_help.Ppm_mass_error != 0.0)
                peaks = psm_help.Match2(1, this.ms2_quant_help, ref annotations);
            else
                peaks = psm_help.Match2(0, this.ms2_quant_help, ref annotations);
            return peaks;
        }
        private List<PEAK> get_pair(PSM psm)
        {
            Peptide pep = new Peptide(psm.Sq);
            double pep_theory_mass = 0.0;
            pep.Mods = Modification.get_modSites(psm.Sq, psm.Mod_sites, int.Parse(psm.Pep_flag), ref pep_theory_mass);
            Spectra spc = getSpectra_byPSM(psm);

            PSM_Help psm_help = new PSM_Help(task.HCD_ETD_Type);
            psm_help.Pep = pep;
            psm_help.Spec = spc;
            //psm_help.aa_index = int.Parse(psm.Pep_flag);
            psm_help.Pep.Tag_Flag = int.Parse(psm.Pep_flag);
            List<PEAK> peaks = null;
            peaks = psm_help.Match3(this.ms2_quant_help2);
            return peaks;
        }
        public void ms2_quant()
        {
            for (int i = 0; i < display_psms.Count; ++i)
            {
                Spectra cur_spec = getSpectra_byPSM(display_psms[i]);
                List<string> annotations = new List<string>();
                List<List<PEAK>> peaks = get_pair(display_psms[i], ref annotations);

                List<double> all_ratios = new List<double>();
                double fz = 0.0, fm = 0.0, a1_ratio = 0.0, a1_intensity = 0.0;
                for (int j = 0; j < peaks[0].Count; ++j)
                {
                    if (peaks[0][j].Intensity != 0.0 && peaks[1][j].Intensity != 0.0 && peaks[0][j].Mass != peaks[1][j].Mass)
                    {
                        if (peaks[0][j].Mass >= this.ms2_quant_help.mz1 && peaks[0][j].Mass <= this.ms2_quant_help.mz2 &&
                            peaks[1][j].Mass >= this.ms2_quant_help.mz1 && peaks[1][j].Mass <= this.ms2_quant_help.mz2)
                        {
                            all_ratios.Add(peaks[1][j].Intensity / peaks[0][j].Intensity);
                            fz += peaks[1][j].Intensity;
                            fm += peaks[0][j].Intensity;
                            if (annotations[j] == Config_Help.MS2_Match_Ion_Type) //只计算a1离子的比值
                            {
                                a1_ratio = peaks[1][j].Intensity / peaks[0][j].Intensity;
                                a1_intensity = peaks[1][j].Intensity * cur_spec.Max_inten_E / 100;
                            }
                        }
                    }
                }
                //将强度求和计算比值
                if (fm != 0)
                    display_psms[i].Ms2_ratio = fz / fm;
                else
                    display_psms[i].Ms2_ratio = 0.0;
                display_psms[i].Ms2_ratio_a1 = a1_ratio;
                display_psms[i].A1_intensity = a1_intensity;
                //all_ratios.Sort(); //取中位数的比值
                //if (all_ratios.Count > 0)
                //    display_psms[i].Ms2_ratio = all_ratios[all_ratios.Count / 2];
                //else
                //    display_psms[i].Ms2_ratio = 0.0;
            }
            this.data_ms2_ratio_column.Visibility = Visibility.Visible;
            this.data_ms2_ratio_column2.Visibility = Visibility.Visible;
            this.data_ms2_ratio_column3.Visibility = Visibility.Visible;
            this.data.Items.Refresh();
            MessageBox.Show("OK");
        }
        //弹出“对儿”的表格，用于二级谱定量，根据this.ms2_quant_help包含的N、C端标记质量，然后计算匹配到的谱峰信息
        private void review_all_pair_clk(object sender, RoutedEventArgs e)
        {
            if (this.data_ms2_ratio_column.Visibility == Visibility.Collapsed)
            {
                MessageBox.Show(Message_Help.NO_RATIO);
                return;
            }
            List<string> annotations = new List<string>();
            List<List<PEAK>> peaks = get_pair(selected_psm, ref annotations);
            Display_ms2_ratio_table dmrt = new Display_ms2_ratio_table(this, peaks, annotations);
            dmrt.Show();
        }
        //固定某些质量的谱峰
        private void ms2_config_btn_clk2(object sender, RoutedEventArgs e)
        {
            if (ms2_quant_config_dialog2 != null)
                ms2_quant_config_dialog2.Close();
            ms2_quant_config_dialog2 = new MS2_Quant_Config_Dialog2(this);
            ms2_quant_config_dialog2.Show();
        }
        public void ms2_quant2()
        {
            StreamWriter sw = new StreamWriter("ok.txt");
            for (int i = 0; i < display_psms.Count; ++i)
            {
                string line = display_psms[i].Title + "\t" + display_psms[i].get_scan() + "\t" + display_psms[i].Sq;
                List<PEAK> peaks = get_pair(display_psms[i]);
                List<double> all_ratios = new List<double>();
                if (peaks.Count > 1 && peaks[0].Intensity != 0.0)
                {
                    for (int j = 1; j < peaks.Count; ++j)
                    {
                        all_ratios.Add(peaks[j].Intensity / peaks[0].Intensity);
                        line += "\t" + all_ratios.Last();
                    }
                }
                all_ratios.Sort();
                if (all_ratios.Count > 0)
                    display_psms[i].Ms2_ratio = all_ratios[all_ratios.Count / 2];
                else
                    display_psms[i].Ms2_ratio = 1024;

                sw.WriteLine(line);
            }
            this.data_ms2_ratio_column.Visibility = Visibility.Visible;

            this.data.Items.Refresh();
            sw.Flush(); sw.Close();
            MessageBox.Show("OK");
        }

        private void datagrid_copy_clk(object sender, RoutedEventArgs e)
        {
            Button obj = sender as Button;
            try
            {
                if (obj == this.psm_copy_btn)
                    ApplicationCommands.Copy.Execute(null, this.data);
                else if (obj == this.protein_copy_btn)
                    ApplicationCommands.Copy.Execute(null, this.protein_data);
            }
            catch (Exception exe)
            {
                //MessageBox.Show(exe.ToString());
            }
        }

        private void startpage_open_task_clk(object sender, RoutedEventArgs e)
        {
            //System.Windows.Forms.OpenFileDialog f_dialog = new System.Windows.Forms.OpenFileDialog();
            //f_dialog.Filter = "task files (*.tsk)|*.tsk";
            ////f_dialog.Filter = "pFind task files (*.pFind)|*.pFind|pNovo task files (*.pNovo)|*.pNovo";
            //System.Windows.Forms.DialogResult dresult = f_dialog.ShowDialog();
            //if (dresult == System.Windows.Forms.DialogResult.Cancel)
            //    return;

            //string file_path = f_dialog.FileName;
            //int last_index = file_path.LastIndexOf('\\');
            //string folder_path = file_path.Substring(0, last_index);
            //task_flag = get_task_flag(folder_path);
            //Task check_task = new Task();
            //if (!check_task.check_task_path(folder_path, task_flag))
            //{
            //    MessageBox.Show(Message_Help.PATH_INVALID);
            //    task = null;
            //    return;
            //}
            //task_flag = get_task_flag(folder_path);
            //load_task(folder_path);
            load_Task(null, null);
        }

        private void startpage_about_clk(object sender, RoutedEventArgs e)
        {
            About about_dialog = new About();
            about_dialog.Show();
        }

        private void startpage_exit_clk(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void load_recent_task(object sender, SelectionChangedEventArgs e)
        {
            TaskInfo task_info = this.recent_tasks.SelectedItem as TaskInfo;
            if (task_info == null)
                return;
            string task_path = task_info.tpath;
            ObservableCollection<TaskInfo> recent_tasks = File_Help.load_all_recent_taskInfo(Task.recent_task_path_file);
            Task check_task = new Task();
            task_flag = get_task_flag(task_path);
            if (!check_task.check_task_path(task_path, task_flag))
            {
                recent_tasks.Remove(new TaskInfo("", task_path));
                File_Help.write_recent_task(Task.recent_task_path_file, recent_tasks);
                this.recent_tasks.ItemsSource = recent_tasks;
                MessageBox.Show(Message_Help.TSK_FOLDER_NOT_EXIST);
                return;
            }
            task_flag = get_task_flag(task_path);
            //查看task_path是否存在，如果不存在，则提示用户不存在该文件夹，该删除该路径
            if (!Directory.Exists(task_path))
            {
                MessageBox.Show(Message_Help.FOLDER_NOT_EXIST);
                TaskInfo ti = new TaskInfo("", task_path);
                startPage.recent_taskInfo.Remove(ti);
                this.recent_tasks.ItemsSource = startPage.recent_taskInfo;
                File_Help.write_recent_task(Task.recent_task_path_file, startPage.recent_taskInfo);
                return;
            }
            update_windows();
            load_task(task_path);
        }
        private void load_task(string task_path)
        {
            if (this.protein_panel != null && this.protein_panel.identification_proteins != null)
                this.protein_panel.identification_proteins.Clear();
            //if (this.task_treeView.Visibility == Visibility.Collapsed)
            //    this.task_treeView.Visibility = Visibility.Visible;
            if (this.summary_tab.Visibility == Visibility.Collapsed)
                this.summary_tab.Visibility = Visibility.Visible;
            if (this.left_grid.Visibility == Visibility.Visible)
                this.left_grid.Visibility = Visibility.Collapsed;
            if (this.right_grid.Visibility == Visibility.Visible)
                this.right_grid.Visibility = Visibility.Collapsed;
            this.WindowState = WindowState.Maximized;

            //this.Show();
            this.Focus();
            this.sa.IsChecked = true;
            this.DataContext = this;
            data.Sorting += new DataGridSortingEventHandler(SortHandler);
            protein_data.Sorting += new DataGridSortingEventHandler(SortHandler2);
            backgroundWorker = ((BackgroundWorker)this.FindResource("backgroundWorker"));
            backgroundWorker_cand = ((BackgroundWorker)this.FindResource("backgroundWorker_cand"));
            backgroundWorker_filter = ((BackgroundWorker)this.FindResource("backgroundWorker_filter"));
            backgroundWorker_fdr = ((BackgroundWorker)this.FindResource("backgroundWorker_fdr"));
            //this.SourceInitialized += new EventHandler(MainWindow_SourceInitialized); //使用单例模式
            if (task_flag == "pTop")
            {
                defaultfilePath = File_Help.read_task_path_pfind_ini(Task.pfind_ini_file);
                Initial_Task_AddTreeView(task_path);
                //读取理论谱图预测模型
                Initial_aa();
                File_Help.read_model(Task.model_path, 193 * 2, ref all_feature, ref all_aa, ref all_interv);
                task = new Task(task_path, "pTop");
                int[] args = new int[2];
                args[0] = 0;
                args[1] = 1;
                if(!backgroundWorker.IsBusy)
                    backgroundWorker.RunWorkerAsync(args);
                //read_allINI(backgroundWorker);
            }
            else if (task_flag == "pNovo")
            {
                File_Help.update_recent_task(Task.recent_task_path_file, task_path);
                File_Help.read_ModifyINI(Task.modify_ini_file); //读取修饰信息，修饰名字对应于它的偏移质量.
                File_Help.read_ElementINI(Task.element_ini_file);
                File_Help.read_AAINI(Task.aa_ini_file);
                Config_Help.update_AA_Mass();

                task = new Task(task_path, "pNovo");
                this.spectra = new ObservableCollection<Spectra>();
                for (int i = 0; i < task.mgf_file_paths.Count; ++i)
                {
                    File_Help.readMGF(task.mgf_file_paths[i], ref this.spectra);
                }
                for (int i = 0; i < this.spectra.Count; ++i)
                    this.spectra_hash[spectra[i].Title] = i;

                psm_help = new PSM_Help(task.HCD_ETD_Type);
                ms2_scan_hash.Clear();
                ms1_scan_hash.Clear();

                int index = 0;
                foreach (DictionaryEntry di in task.title_PF2_Path)
                {
                    string ms2_pfindex_file = (string)di.Value + ".pf2idx";
                    //string ms1_pfindex_file = (string)di.Value + ".pf1idx";
                    ms2_scan_hash.Add(File_Help.read_ms2_index(ms2_pfindex_file)); //读取二级谱图的索引，扫描号对应于索引文件的位置，可以获得谱图信息
                    //ms1_scan_hash.Add(File_Help.read_ms1_index(ms1_pfindex_file, ref Min_scan, ref Max_scan)); //读取一级谱图的索引
                    title_hash[(string)di.Key] = index++;
                }

                File_Help.load_pNovo_param(task.pNovo_param_path);
                all_psms = File_Help.load_pNovo_result(task.pNovo_result_path);
                psms = all_psms;
                data.ItemsSource = psms;
                this.display_size.Text = psms.Count.ToString();

                update_windows();
            }
            else if (task_flag == "pLink")
            {
                this.frame_left.MinWidth = 0;

                this.task_treeView.Visibility = Visibility.Collapsed;
                this.summary_tab.Visibility = Visibility.Visible;
                this.left_grid.Visibility = Visibility.Collapsed;
                this.right_grid.Visibility = Visibility.Collapsed;

                TabItem summary_table = summary_tab.Items[0] as TabItem;
                TabItem protein_table = summary_tab.Items[2] as TabItem;

                summary_table.Visibility = Visibility.Collapsed;
                protein_table.Visibility = Visibility.Collapsed;

                this.summary_tab.SelectedIndex = 1;

                psms.Clear();
                File_Help.read_ElementINI(Task.element_ini_file);
                File_Help.read_ModifyINI(Task.modify_ini_file);
                task = new Task(task_path, "pLink");
                List<string> raw_file_paths = new List<string>();
                List<string> link_names = pLink.File_Help.load_xlink_name(task.pLink_param_path, task, ref raw_file_paths, ref title_hash);
                pLink.File_Help.load_label_information(task.pQuant_param_file, pLink_result.pLink_label);
                ms2_scan_hash.Clear();
                ms1_scan_hash.Clear();
                for (int i = 0; i < raw_file_paths.Count; ++i)
                {
                    string raw_name = raw_file_paths[i]; //已经切除掉其后缀名.raw
                    string ms2_pfindex_file = task.get_pf2Index_path(raw_name);
                    string ms1_pfindex_file = task.get_pf1Index_path(raw_name);
                    ms2_scan_hash.Add(File_Help.read_ms2_index(ms2_pfindex_file)); //读取二级谱图的索引，扫描号对应于索引文件的位置，可以获得谱图信息
                    ms1_scan_hash.Add(File_Help.read_ms1_index(ms1_pfindex_file, ref Min_scan, ref Max_scan)); //读取一级谱图的索引
                }


                File_Help.update_AA_Mass(task.all_aa_files); //读取所有的氨基酸质量表包括氨基酸对应的元素信息
                File_Help.update_Mod_Mass(task.all_mod_files); //读取所有的修饰质量表包括修饰对应的元素信息

                pLink_result.psms = pLink.File_Help.readPSM_pLink(task.pLink_result_path);
                bool is_ok = true;
                if (isnot_mgf && task.quantification_file != "") //如果不是MGF，则搜索的是RAW，那么需要读取定量结果文件，需要提速
                    is_ok = File_Help.create_3D_index(task.quantification_file, osType, title_hash, ref start, ref end, ref qut_scan_hash);
                if (!is_ok)
                {
                    MessageBox.Show(Message_Help.Quant_Wrong);
                }
                if (task.has_ratio) //如果没有跑定量就不用加载定量结果文件，需要提速
                {
                    pLink.File_Help.getPSM_Ratio(task.quantification_list_file, pLink_result.psms);
                }
                pLink.File_Help.load_xlink(Task.pLink_ini_file);
                pLink.File_Help.load_xx(Task.pLink_xxINI_file);

                pLink_result.Link_Names = link_names;
                pLink_result.Link_Name = pLink_result.Link_Names.First();
                for (int i = 0; i < link_names.Count; ++i)
                {
                    string link_name = link_names[i];
                    pLink_result.Link_masses.Add((double)Config_Help.link_hash[link_name]);
                }
                pLink_result.Link_mass = pLink_result.Link_masses.First();
                pLink_result.pLink_label.Linker_Masses = pLink_result.Link_masses;

                this.data.ItemsSource = pLink_result.psms;
                this.display_size.Text = pLink_result.psms.Count.ToString();

                this.Cursor = null;
            }
        }

        private void export_scan_ratio(object sender, RoutedEventArgs e)
        {
            List<string> raw_names = new List<string>();
            foreach (DictionaryEntry entry in title_hash)
            {
                raw_names.Add((string)entry.Key);
            }
            Model_Window model_w = new Model_Window(raw_names, this, 1);
            model_w.Show();
        }

        private void run_pConfig_clk(object sender, RoutedEventArgs e)
        {
            Process.Start("pConfig.exe");
        }

        private void chrom_3D_clk(object sender, RoutedEventArgs e)
        {
            AddAxis();
            TrackMouse();

            quant_init();
            this.viewport3D.Children.Clear();
            this.text_canvas.Children.Clear();
            TextPoints.Clear();
            GenerateBackgroundGrid();
            AddAxis2();
            ChangeAxisTextPosition();
            ChangeTextPosition();
            if ((bool)this.chrom_3D.IsChecked)
            {
                UseGeometry();
                this.d3_alpha_slider.Visibility = Visibility.Visible;
            }
            else
            {
                UseWirePolyLine();
                this.d3_alpha_slider.Visibility = Visibility.Collapsed;
            }
        }

        private void d3_exp_clipboard(object sender, RoutedEventArgs e)
        {
            this.chrom_border.Background = new SolidColorBrush(Colors.Transparent);
            EMFCopy.CopyUIElementToClipboard(this.chrom_border);
        }

        private void d3_alpha_value_chg(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ((bool)this.chrom_3D.IsChecked)
            {
                AddAxis();
                TrackMouse();

                quant_init();
                this.viewport3D.Children.Clear();
                this.text_canvas.Children.Clear();
                TextPoints.Clear();
                GenerateBackgroundGrid();
                AddAxis2();
                ChangeAxisTextPosition();
                ChangeTextPosition();
                if ((bool)this.chrom_3D.IsChecked)
                    UseGeometry();
                else
                    UseWirePolyLine();
            }
        }
        //对蛋白质列表进行重排序，输出第一名的代表蛋白，再输出对应的SameSet或SubSet，然后输出第二名的代表蛋白，依次这样输出
        private void datagrid_sort_clk(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Protein> show_proteins = protein_panel.identification_proteins;
            show_proteins = new ObservableCollection<Protein>(show_proteins.OrderByDescending(a => a.psm_index.Count));
            for (int i = 0; i < show_proteins.Count; ++i)
            {
                show_proteins[i].ID = i + 1;
            }
            ObservableCollection<Protein> show_proteins2 = new ObservableCollection<Protein>();
            for (int i = 0; i < show_proteins.Count; ++i)
            {
                if (show_proteins[i].Parent_Protein_AC != "")
                    continue;
                show_proteins2.Add(show_proteins[i]);
                for (int j = 0; j < show_proteins[i].Protein_index.Count(); ++j)
                {
                    if (!show_proteins2.Contains(protein_panel.identification_proteins[show_proteins[i].Protein_index[j]]))
                        show_proteins2.Add(protein_panel.identification_proteins[show_proteins[i].Protein_index[j]]);
                }
            }
            protein_data.ItemsSource = show_proteins2;
            display_size.Text = show_proteins2.Count.ToString();
        }
        //显示混合谱图
        private void show_mix_clk(object sender, RoutedEventArgs e)
        {
            PSM selected_psm = this.data.SelectedItem as PSM;
            if (selected_psm == null)
                return;
            string title = selected_psm.Title;
            string[] strs = title.Split('.');
            string selected_title = strs[0] + "." + strs[1]; //raw_name+"."+scan
            string ms2_pf_file = task.get_pf2_path(strs[0]);
            List<Peptide> mix_peps = new List<Peptide>();
            List<double> mix_spec_mass = new List<double>();
            List<int> mix_spec_charge = new List<int>();
            for (int i = 0; i < psms.Count(); ++i)
            {
                string title1 = psms[i].Title;
                string[] strs1 = title1.Split('.');
                string compare_title = strs1[0] + "." + strs1[1]; //raw_name+"."+scan
                if (selected_title == compare_title)
                {
                    Peptide pep = new Peptide(psms[i].Sq);
                    double pep_theory_mass = 0.0;
                    pep.Mods = Modification.get_modSites(psms[i].Sq, psms[i].Mod_sites, int.Parse(psms[i].Pep_flag), ref pep_theory_mass);
                    pep.Pepmass = pep_theory_mass;
                    pep.Tag_Flag = int.Parse(psms[i].Pep_flag);
                    mix_peps.Add(pep);

                    string[] strs2 = title1.Split('.');
                    int pre_num = 0;
                    if (strs2[4] != "dta")
                        pre_num = int.Parse(strs2[4]);
                    ObservableCollection<PEAK> peaks = new ObservableCollection<PEAK>();
                    double pepmass = 0.0;
                    read_peaks(ms2_pf_file, ms2_scan_hash[(int)(title_hash[strs[0]])], int.Parse(strs[1]), pre_num, ref peaks, ref pepmass);
                    mix_spec_mass.Add(pepmass);
                    mix_spec_charge.Add(int.Parse(strs2[3]));
                }
            }
            ObservableCollection<PEAK> peaks1 = new ObservableCollection<PEAK>();
            double pepmass1 = 0.0;
            string raw_name = strs[0];
            int scan_num = int.Parse(strs[1]);
            int charge = int.Parse(strs[3]);
            int pre_num1 = 0;
            if (strs[4] != "dta")
                pre_num1 = int.Parse(strs[4]);

            double maxInten = read_peaks(ms2_pf_file, ms2_scan_hash[(int)(title_hash[raw_name])], scan_num, pre_num1, ref peaks1, ref pepmass1, ref charge);
            Spectra spec = new Spectra(title, charge, pepmass1, peaks1, maxInten);
            //psm_help = new PSM_Help(task.HCD_ETD_Type);
            psm_help.Switch_PSM_Help(spec, mix_peps, mix_spec_mass, mix_spec_charge); //更新谱图Spec及肽段Pep
            dis_help = new Display_Help(psm_help, dis_help.ddh, dis_help.ddhms1);
            this.Model2 = dis_help.display_ms2(mix_peps.Count);
            if (this.Model2 == null)
                return;

            MS2_Help ms2_help = new MS2_Help(Model2, dis_help, Ladder_Help.Scale_Width, Ladder_Help.Scale_Height, this.frame_right.ActualWidth, this.display_tab.ActualHeight, this);
            this.Model2.Axes[1].AxisChanged += (s, er) =>
            {
                ms2_help = new MS2_Help(Model2, dis_help, Ladder_Help.Scale_Width, Ladder_Help.Scale_Height, this.frame_right.ActualWidth, this.display_tab.ActualHeight, this);
                ms2_help.window_sizeChg_Or_ZoomPan();
            };
            this.DataContext = this;
            this.Model2.RefreshPlot(true);

            data_RD.Height = new GridLength(data_RD.ActualHeight + 1.0e-10, GridUnitType.Pixel);
        }

        private void protein_copy_peptides_clk(object sender, RoutedEventArgs e)
        {
            string clipboard_string = "";
            for (int i = 0; i < this.protein_data.SelectedItems.Count; ++i)
            {
                Protein selected_protein = (Protein)protein_data.SelectedItems[i];
                string ac_name = selected_protein.AC;
                clipboard_string += ac_name + "\n";
                //for (int j = 0; j < psms.Count; ++j)
                //{
                //    if (psms[j].AC.Contains(ac_name))
                //        clipboard_string += psms[j].ToString() + "\n";
                //}
                List<int> psm_index = selected_protein.psm_index;
                for (int j = 0; j < psm_index.Count; ++j)
                {
                    clipboard_string += psms[psm_index[j]].ToStringNoRatio() + "\n";
                }
            }
            copyToClipboard(clipboard_string);
        }

        private void load_MGF(object sender, RoutedEventArgs e)
        {
            File_Help.read_ElementINI(Task.element_ini_file);
            File_Help.read_ModifyINI(Task.modify_ini_file);
            File_Help.read_AAINI(Task.aa_ini_file);

            System.Windows.Forms.OpenFileDialog f_dialog = new System.Windows.Forms.OpenFileDialog();
            f_dialog.Filter = "mgf files (*.mgf)|*.mgf";
            //f_dialog.Filter = "pFind task files (*.pFind)|*.pFind|pNovo task files (*.pNovo)|*.pNovo";
            System.Windows.Forms.DialogResult dresult = f_dialog.ShowDialog();
            if (dresult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.Cursor = Cursors.Wait;
            task_flag = "MGF";
            this.frame_left.MinWidth = 0;

            this.task_treeView.Visibility = Visibility.Collapsed;
            this.summary_tab.Visibility = Visibility.Visible;
            this.left_grid.Visibility = Visibility.Collapsed;
            this.right_grid.Visibility = Visibility.Collapsed;
            string mgf_file_path = f_dialog.FileName;
            TabItem summary_table = summary_tab.Items[0] as TabItem;
            TabItem protein_table = summary_tab.Items[2] as TabItem;

            summary_table.Visibility = Visibility.Collapsed;
            protein_table.Visibility = Visibility.Collapsed;

            this.summary_tab.SelectedIndex = 1;

            spectra = File_Help.readMGF(mgf_file_path);
            psms.Clear();
            for (int i = 0; i < spectra.Count; ++i)
            {
                PSM psm = PSM.getPSM_BySpectra(spectra[i]);
                psm.Id = i + 1;
                psms.Add(psm);
            }
            this.data.ItemsSource = psms;
            this.display_size.Text = psms.Count + "";
            this.Cursor = null;
        }

        private void denovo_clk(object sender, RoutedEventArgs e)
        {
            if (this.data.SelectedItem == null)
                return;
            Peptide non_peptide = new Peptide();
            PSM_Help psm_help = new PSM_Help(this.dis_help.Psm_help as PSM_Help);
            psm_help.Pep = non_peptide;
            Display_Help dis_help = new Display_Help(psm_help, this.dis_help.ddh, this.dis_help.ddhms1);
            PlotModel model_tmp = null;
            if (dis_help.Psm_help == null)
                return;
            model_tmp = dis_help.display_ms2(dis_help.Psm_help.Peptide_Number);
            model_tmp.Axes[1].Reset();
            model_tmp.Axes[3].Reset();
            Model_Window_DeNovo model_window = new Model_Window_DeNovo(this, model_tmp, dis_help, this.model2_border.ActualWidth + 25,
                this.model2_border.ActualHeight + 25);
            model_window.Width = model_window.width;
            model_window.Height = model_window.height;
            model_window.Show();
        }

        private void group_data_sch(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            display_protein();
        }

        private void protein_tab_chg(object sender, SelectionChangedEventArgs e)
        {
        }
        private List<Similarity.N_C_Term_Intensity> get_NC_inten(ObservableCollection<PSM> psms)
        {
            Peptide pep = null;
            Spectra spec = null;
            List<Similarity.N_C_Term_Intensity> ncs = new List<Similarity.N_C_Term_Intensity>();
            for (int i = 0; i < psms.Count; ++i)
            {
                parse_PSM(psms[i], ref spec, ref pep);
                psm_help.Switch_PSM_Help(spec, pep);
                psm_help.Pep.Tag_Flag = int.Parse(psms[i].Pep_flag);
                ncs.Add(psm_help.Match_SIM());
            }
            return ncs;
        }
        private void show_sim_bySeq_clk(object sender, RoutedEventArgs e)
        {
            show_spec_clk(this.ssOnlySQ, null);
            List<Similarity.N_C_Term_Intensity> ncs = get_NC_inten(display_psms);
            List<string> titles = new List<string>();
            List<List<double>> scores = new List<List<double>>();
            for (int i = 0; i < ncs.Count; ++i)
            {
                titles.Add(ncs[i].Title);
                scores.Add(new List<double>());
                for (int j = 0; j < ncs.Count; ++j)
                    scores[i].Add(0.0);
            }
            for (int i = 0; i < ncs.Count; ++i)
            {
                scores[i][i] = 1.0;
                for (int j = i + 1; j < ncs.Count; ++j)
                {
                    Similarity.N_C_Term_Sim nc_s = new Similarity.N_C_Term_Sim(ncs[i], ncs[j]);
                    scores[i][j] = nc_s.Get_Cos_Sim();
                    scores[j][i] = scores[i][j];
                }
            }
            Similarity.N_C_Term_Display nc_dis = new Similarity.N_C_Term_Display(this, this.psm_hash, ncs, titles, scores);
            PlotModel model = nc_dis.display_heatMap();
            Model_Window mw = new Model_Window(model, "Cos Similarity");
            mw.Show();
        }

        private void show_all_similarity(object sender, RoutedEventArgs e)
        {
            Hashtable sq_int = new Hashtable();
            for (int i = 0; i < psms.Count; ++i)
            {
                if (!psms[i].Is_target_flag)
                    continue;
                string sq = psms[i].Sq;
                if (sq_int[sq] == null)
                {
                    ObservableCollection<PSM> tmp_psms = new ObservableCollection<PSM>();
                    tmp_psms.Add(psms[i]);
                    sq_int[sq] = tmp_psms;
                }
                else
                {
                    ObservableCollection<PSM> tmp_psms = sq_int[sq] as ObservableCollection<PSM>;
                    tmp_psms.Add(psms[i]);
                    sq_int[sq] = tmp_psms;
                }
            }
            StreamWriter sw = new StreamWriter("MS2_sim.txt");
            foreach (DictionaryEntry de in sq_int)
            {
                string sq = de.Key as string;
                ObservableCollection<PSM> tmp_psms = de.Value as ObservableCollection<PSM>;
                List<Similarity.N_C_Term_Intensity> ncs = get_NC_inten(tmp_psms);
                List<double> scores1 = new List<double>();
                List<double> scores2 = new List<double>();
                for (int i = 0; i < ncs.Count; ++i)
                {
                    for (int j = i + 1; j < ncs.Count; ++j)
                    {
                        if (ncs[i].Charge != ncs[j].Charge)
                            continue;
                        if (ncs[i].Flag == ncs[j].Flag) //标记与标记进行打分，非标记与非标记进行打分
                        {
                            Similarity.N_C_Term_Sim nc_sim = new Similarity.N_C_Term_Sim(ncs[i], ncs[j]);
                            scores1.Add(nc_sim.Get_Cos_Sim());
                        }
                        else if (ncs[i].Flag + ncs[j].Flag == 3) //标记与非标记进行打分
                        {
                            Similarity.N_C_Term_Sim nc_sim = new Similarity.N_C_Term_Sim(ncs[i], ncs[j]);
                            scores2.Add(nc_sim.Get_Cos_Sim());
                        }
                    }
                }
                if (scores1.Count == 0 || scores2.Count == 0)
                    continue;
                scores1.Sort();
                scores2.Sort();
                sw.WriteLine(sq + "\t" + scores1[scores1.Count / 2] + "\t" + scores2[scores2.Count / 2]);
            }
            sw.Flush();
            sw.Close();
        }


        //打开pLink的任务，一个plabel后缀的文件，同时会读取里面的那个MGF文件，将MGF的所有谱图信息加载进来
        private void load_pLink_result(object sender, RoutedEventArgs e)
        {
            File_Help.read_ElementINI(Task.element_ini_file);
            File_Help.read_ModifyINI(Task.modify_pLink_ini_file, 2);
            File_Help.read_AAINI(Task.aa_ini_file);
            pLink.File_Help.load_xlink(Task.pLink_ini_file);

            System.Windows.Forms.OpenFileDialog f_dialog = new System.Windows.Forms.OpenFileDialog();
            f_dialog.Filter = "pLink files (*.plabel)|*.plabel";
            System.Windows.Forms.DialogResult dresult = f_dialog.ShowDialog();
            if (dresult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.Cursor = Cursors.Wait;
            task_flag = "pLink";
            this.frame_left.MinWidth = 0;

            this.task_treeView.Visibility = Visibility.Collapsed;
            this.summary_tab.Visibility = Visibility.Visible;
            this.left_grid.Visibility = Visibility.Collapsed;
            this.right_grid.Visibility = Visibility.Collapsed;
            string mgf_file_path = f_dialog.FileName;
            TabItem summary_table = summary_tab.Items[0] as TabItem;
            TabItem protein_table = summary_tab.Items[2] as TabItem;

            summary_table.Visibility = Visibility.Collapsed;
            protein_table.Visibility = Visibility.Collapsed;

            this.summary_tab.SelectedIndex = 1;

            psms.Clear();
            ObservableCollection<Spectra> spectra = new ObservableCollection<Spectra>();
            Hashtable title_index = new Hashtable();
            string link_name = "";
            pLink_result.psms = pLink.File_Help.load_plabel_file(f_dialog.FileName, ref spectra, ref title_index, ref link_name);
            pLink_result.spectra = spectra;
            pLink_result.title_index = title_index;
            pLink_result.Link_Name = link_name;
            pLink_result.Link_mass = (double)Config_Help.link_hash[link_name];

            this.data.ItemsSource = pLink_result.psms;
            this.display_size.Text = pLink_result.psms.Count.ToString();

            this.Cursor = null;
        }
        private void Export_to_TXT(ObservableCollection<PSM> psms, string result_file)
        {
            this.Cursor = Cursors.Wait;
            string folder = task.folder_result_path + File_Help.pBuild_tmp_file;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            try
            {
                StreamWriter sw = new StreamWriter(folder + "\\" + result_file);
                if (task.has_ratio)
                {
                    sw.WriteLine(PSM.ToHeaderWithRatio());
                    for (int i = 0; i < psms.Count; ++i)
                    {
                        sw.WriteLine(psms[i].ToStringWithRatio());
                    }
                }
                else
                {
                    if (this.data_ms2_ratio_column.Visibility == Visibility.Visible)
                    {
                        sw.WriteLine(PSM.ToHeaderWithRatio2());
                        for (int i = 0; i < psms.Count; ++i)
                        {
                            sw.WriteLine(psms[i].ToStringWithRatio2());
                        }
                    }
                    else
                    {
                        sw.WriteLine(PSM.ToHeaderNoRatio());
                        for (int i = 0; i < psms.Count; ++i)
                        {
                            sw.WriteLine(psms[i].ToStringNoRatio());
                        }
                    }
                }

                sw.Flush();
                sw.Close();
                this.Cursor = null;
                if (MessageBox.Show("The result file is saved in \"" + folder + "\\" + result_file + "\".\nYou should copy this file to other place " +
                "before the application is closed. Because when the application is closed, the \"" + File_Help.pBuild_tmp_file + "\" folder will be deleted.\r\nYou can click \"yes\" button to open this folder.", "Confirm Message", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start("explorer.exe", folder);
                }
            }
            catch (Exception exe)
            {
                this.Cursor = null;
                MessageBox.Show(exe.ToString());
            }
        }
        private void Export_to_TXT(ObservableCollection<Protein> proteins, string result_file)
        {
            this.Cursor = Cursors.Wait;
            string folder = task.folder_result_path + File_Help.pBuild_tmp_file;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            try
            {
                StreamWriter sw = new StreamWriter(folder + "\\" + result_file);
                if (task.has_ratio)
                {
                    sw.WriteLine(Protein.ToHeaderWithRatio());
                    for (int i = 0; i < proteins.Count; ++i)
                    {
                        sw.WriteLine(proteins[i].ToStringWithRatio());
                    }
                }
                else
                {
                    if (this.data_protein_ratio_column.Visibility == Visibility.Visible)
                    {
                        sw.WriteLine(Protein.ToHeaderWithRatio2());
                        for (int i = 0; i < proteins.Count; ++i)
                        {
                            sw.WriteLine(proteins[i].ToStringWithRatio2());
                        }
                    }
                    else
                    {
                        sw.WriteLine(Protein.ToHeaderNoRatio());
                        for (int i = 0; i < proteins.Count; ++i)
                        {
                            sw.WriteLine(proteins[i].ToStringNoRatio());
                        }
                    }
                }

                sw.Flush();
                sw.Close();
                this.Cursor = null;
                if (MessageBox.Show("The result file is saved in \"" + folder + "\\" + result_file + "\".\nYou should copy this file to other place " +
                "before the application is closed. Because when the application is closed, the \"" + File_Help.pBuild_tmp_file + "\" folder will be deleted.\r\nYou can click \"yes\" button to open this folder.", "Confirm Message", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start("explorer.exe", folder);
                }
            }
            catch (Exception exe)
            {
                this.Cursor = null;
                MessageBox.Show(exe.ToString());
            }
        }
        private void datagrid_export_clk(object sender, RoutedEventArgs e)
        {
            Export_to_TXT(psms, "all_result.txt");
        }

        private void show_twoPsm_clk(object sender, RoutedEventArgs e)
        {
            selected_psms.Clear();
            List<Similarity.Report_Ion> ions = new List<Similarity.Report_Ion>();
            for (int i = 0; i < data.SelectedItems.Count; ++i)
            {
                selected_psm = data.SelectedItems[i] as PSM;
                selected_psms.Add(selected_psm);
                Similarity.Report_Ion ion_help = get_ion_help();
                ions.Add(ion_help);
            }
            if (selected_psms.Count != 2)
                return;
            double cos_sim = Similarity.N_C_Term_Sim.Get_Cos_Sim(ions[0], ions[1]);
            Similarity.N_C_Term_Display nctd = new Similarity.N_C_Term_Display(this);
            PlotModel model0 = nctd.display_TwoMS2(selected_psms[0], selected_psms[1]);
            Model_Window mw = new Model_Window(model0, selected_psms[1].Title + " - " + selected_psms[0].Title + "   Cos = " + cos_sim.ToString("F2"), false);
            mw.Show();
            s_ch(null, null);
        }

        private void exp_report(object sender, RoutedEventArgs e)
        {
            Report.Report_Help rh = new Report.Report_Help(this);
            rh.report_word();
        }

        private void exp_dta_file(object sender, RoutedEventArgs e)
        {
            Spectra spec = get_spectra();
            if (spec == null)
            {
                MessageBox.Show(Message_Help.NULL_SELECT_PSM);
                return;
            }
            string dta_file_path = File_Help.write_dta_file(task.folder_result_path, spec);
            Process.Start("notepad.exe", dta_file_path);
        }
        private Similarity.Report_Ion get_ion_help()
        {
            Spectra spec = get_spectra();
            Peptide pep = get_peptide();
            psm_help.Switch_PSM_Help(spec, pep);
            return new Similarity.Report_Ion(psm_help);
        }
        private void exp_ions_file(object sender, RoutedEventArgs e)
        {
            Similarity.Report_Ion ion_help = get_ion_help();
            string ion_file_path = ion_help.write_file(this.task.folder_result_path + File_Help.pBuild_tmp_file + "\\ion.txt");
            Process.Start("notepad.exe", ion_file_path);
            //StreamWriter sw = new StreamWriter("mz_error.txt");
            //for (int i = 0; i < psms.Count; ++i)
            //{
            //    selected_psm = psms[i];
            //    if (!selected_psm.Title.StartsWith("24"))
            //        continue;
            //    Similarity.Report_Ion ion_help = get_ion_help();
            //    ion_help.write_file(sw);
            //}
            //sw.Flush();
            //sw.Close();
        }
        private void data_ctrl_C(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && (Keyboard.Modifiers & (ModifierKeys.Control)) == (ModifierKeys.Control))
            {
                selected_psms.Clear();
                for (int i = 0; i < data.SelectedItems.Count; ++i)
                    selected_psms.Add((PSM)data.SelectedItems[i]);
                Export_to_TXT(selected_psms, "selected_result.txt");
                e.Handled = true;
            }
            else if (e.Key == Key.A && (Keyboard.Modifiers & (ModifierKeys.Control)) == (ModifierKeys.Control))
            {
                this.data.SelectedItems.Clear();
                this.data.SelectAll();
            }
        }

        private void protein_Ctrl_C(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && (Keyboard.Modifiers & (ModifierKeys.Control)) == (ModifierKeys.Control))
            {
                ObservableCollection<Protein> selected_proteins = new ObservableCollection<Protein>();
                for (int i = 0; i < protein_data.SelectedItems.Count; ++i)
                    selected_proteins.Add(protein_data.SelectedItems[i] as Protein);
                Export_to_TXT(selected_proteins, "selected_protein_result.txt");
                e.Handled = true;
            }
        }

        private void protein_show_ss_protein_clk(object sender, RoutedEventArgs e)
        {
            Protein selected_protein = this.protein_data.SelectedItem as Protein;
            if (selected_protein.Parent_Protein_AC != "")
            {
                MessageBox.Show("The selected protein is not group protein.");
                return;
            }
            List<int> protein_index = selected_protein.Protein_index;
            ObservableCollection<Protein> show_proteins = new ObservableCollection<Protein>();
            for (int i = 0; i < protein_index.Count; ++i)
            {
                show_proteins.Add(this.protein_panel.identification_proteins[protein_index[i]]);
            }
            this.protein_data.ItemsSource = show_proteins;
            this.display_size.Text = show_proteins.Count + "";
        }

        private void protein_peptide_SQ_tbx_keyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || this.protein_data.SelectedItem == null)
            {
                return;
            }
            Thickness thickness_zero = new Thickness(0);
            for (int i = 0; i < this.protein_SQ_grid.Children.Count; ++i)
            {
                StackPanel sp = this.protein_SQ_grid.Children[i] as StackPanel;
                for (int j = 0; j < sp.Children.Count; ++j) //j==1和j==sp.Children.Count-1表示的是从前面接下来和接下去的序列
                {
                    if (sp.Children[j] is Border && ((Border)sp.Children[j]).BorderThickness != thickness_zero)
                    {
                        ((Border)sp.Children[j]).BorderThickness = thickness_zero;
                    }
                }
            }
            Protein_Help ph = new Protein_Help(this.protein_panel.pddh.alphabet_num_per_row, this.protein_SQ_grid);
            string peptide_sq = this.protein_peptide_SQ_tbx.Text;
            for (int i = 0; i < this.protein_SQ_grid.Children.Count; ++i)
            {
                StackPanel sp = this.protein_SQ_grid.Children[i] as StackPanel;
                for (int j = 0; j < sp.Children.Count; ++j) //j==1和j==sp.Children.Count-1表示的是从前面接下来和接下去的序列
                {
                    if (sp.Children[j] is Border)
                    {
                        Border bd = sp.Children[j] as Border;
                        TextBlock tb = bd.Child as TextBlock;

                        string sq = ph.getStringByBorder(bd);

                        Border next_border = ph.next_floor(sp, i, j);
                        Border previous_border = ph.previous_floor(sp, i, j);
                        if (next_border == null && previous_border == null && sq == peptide_sq)
                        {
                            bd.BorderThickness = new Thickness(1);
                            return;
                        }
                        if (next_border != null)
                        {
                            sq += ph.getStringByBorder(next_border);
                            if (sq == peptide_sq)
                            {
                                bd.BorderThickness = new Thickness(1);
                                next_border.BorderThickness = new Thickness(1);
                                return;
                            }
                        }
                        if (previous_border != null)
                        {
                            sq = ph.getStringByBorder(previous_border) + sq;
                            if (sq == peptide_sq)
                            {
                                bd.BorderThickness = new Thickness(1);
                                previous_border.BorderThickness = new Thickness(1);
                                return;
                            }
                        }
                    }
                }
            }
            MessageBox.Show("Can not find this peptide in selected protein.");
        }

        private void retainSpecInitProtein(object sender, RoutedEventArgs e)
        {
            ObservableCollection<PSM> newPSMs = new ObservableCollection<PSM>();
            for (int i = 0; i < psms.Count; ++i)
            {
                if (psms[i].Specific_flag == 'S')
                    newPSMs.Add(psms[i]);
            }
            psms = newPSMs;
            protein_panel.identification_proteins.Clear();
            initial_Protein();
        }
    }
}