using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace pBuild
{
    /// <summary>
    /// Model_Window.xaml 的交互逻辑
    /// </summary>
    public partial class Model_Window : Window //绘制弹出形式的图
    {
        int flag = 0;
        public PlotModel Model;
        public Display_Help dis_help;
        public double scale_width;
        public double scale_height;
        public double width;
        public double height;

        public bool topMost = true;
        public MainWindow mainW;
        public int MS1_Ratio = 0; //MS1_Ratio=0表示绘制LC/MS图，MS1_Ratio=1表示绘制scan-ratio图

        public Model_Window(List<string> raws, MainWindow mainW, int ms1_ratio) //绘制m/z-scan的热度图，因为可以有多个RAW，所以必须选中该RAW
        {
            InitializeComponent();
            this.raw_comboBox.Visibility = Visibility.Visible;
            for (int i = 0; i < raws.Count; ++i)
            {
                this.raw_comboBox.Items.Add(raws[i]);
            }
            this.Width = 500;
            this.Height = 500;
            this.mainW = mainW;
            this.MS1_Ratio = ms1_ratio;
        }

        public Model_Window(PlotModel Model, string title, bool topMost = true) //绘制色谱曲线
        {
            this.topMost = topMost;
            InitializeComponent();
            this.Model = Model;
            this.model.Model = Model;
            this.Title = title;
        }
        public Model_Window(PlotModel Model, int index) //绘制质量误差图
        {
            InitializeComponent();
            this.Model = Model;
            this.model.Model = Model;
            this.Title = "Mass Deviation Figure " + index;
        }
        public Model_Window(PlotModel Model, Display_Help dis_help, double width, double height) //绘制二级匹配图
        {
            InitializeComponent();
            this.scale_width = Ladder_Help.Scale_Width;
            this.scale_height = Ladder_Help.Scale_Height;
            this.width = width;
            this.height = height;
            this.Model = Model;
            MS2_Help ms2_help = new MS2_Help(Model, dis_help, scale_width, scale_height);
            this.Model.Axes[1].AxisChanged += (s, e) =>
            {
                ms2_help = new MS2_Help(Model, dis_help, scale_width, scale_height);
                ms2_help.window_sizeChg_Or_ZoomPan();
            };
            this.dis_help = dis_help;
            this.model.Model = this.Model;
            ms2_help.window_sizeChg_Or_ZoomPan();
            this.SizeChanged += Window_sizeChg;
            this.Title = "PSM Figure";
        }
        private void Window_sizeChg(object sender, SizeChangedEventArgs e)
        {
            this.scale_width = this.scale_width * this.ActualWidth / this.width;
            this.scale_height = this.scale_height * this.ActualHeight / this.height;
            this.width = this.ActualWidth;
            this.height = this.ActualHeight;
            MS2_Help ms2_help = new MS2_Help(Model, dis_help, this.scale_width, this.scale_height);
            ms2_help.window_sizeChg_Or_ZoomPan();
        }
        private void Window_Initialized(object sender, EventArgs e)
        {
            this.Topmost = topMost;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if(this.dis_help != null)
                this.Topmost = false;
        }

        private void raw_select(object sender, SelectionChangedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            string raw_name = this.raw_comboBox.SelectedItem as string;
            List<string> pf_paths = mainW.task.get_raws_path();
            string pf_path = "";
            for (int i = 0; i < pf_paths.Count; ++i)
            {
                int index = pf_paths[i].LastIndexOf('\\');
                int index2 = pf_paths[i].LastIndexOf('.');
                if (pf_paths[i].Substring(index + 1, index2 - index - 1) == raw_name)
                {
                    pf_path = pf_paths[i].Substring(0, index2) + ".pf1";
                    break;
                }
            }
            if (MS1_Ratio == 0)
            {
                List<List<Spectra_MS1>> ms1_list = File_Help.load_ms1(pf_path, raw_name, mainW);
                Display_Help dh = new Display_Help(ms1_list, this.mainW);
                this.Model = dh.display_heat_map();
            }
            else if (MS1_Ratio == 1)
            {
                if (!mainW.task.has_ratio)
                {
                    MessageBox.Show("No ratio!");
                    return;
                }
                double max_scan = double.MinValue;
                double min_scan = double.MaxValue;
                for (int i = 0; i < mainW.psms.Count; ++i)
                {
                    string title = mainW.psms[i].Title.Split('.')[0];
                    if (title != raw_name)
                        continue;
                    double scan = mainW.psms[i].get_scan();
                    if (max_scan < scan)
                        max_scan = scan;
                    if (min_scan > scan)
                        min_scan = scan;
                }
                int bin_number = 49;
                double scan_width = (max_scan - min_scan) / bin_number; //打分宽度,让最终形成50个bin
                double cur_scan = min_scan;
                List<double> all_scans = new List<double>();
                List<int> scan_bin_list = new List<int>();
                while (cur_scan <= max_scan)
                {
                    all_scans.Add(cur_scan);
                    cur_scan += scan_width;
                    scan_bin_list.Add(0);
                }
                for (int i = 0; i < mainW.psms.Count; ++i)
                {
                    string title = mainW.psms[i].Title.Split('.')[0];
                    if (title != raw_name)
                        continue;
                    double ratio = mainW.psms[i].Ratio;
                    if (ratio >= 1024 || ratio <= 0.001)
                        continue;
                    int index = (int)((mainW.psms[i].get_scan() - min_scan) / scan_width);
                    if (index < scan_bin_list.Count)
                        ++scan_bin_list[index];
                }

                Display_Help dh = new Display_Help(all_scans, scan_bin_list, this.mainW);
                this.Model = dh.display_scan_ratio_graph();
            }
            this.model.Model = this.Model;
            this.Cursor = null;
        }

        private void ctrl_C_keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && (Keyboard.Modifiers & (ModifierKeys.Control)) == (ModifierKeys.Control))
            {
                EMFCopy.CopyVisualToWmfClipboard((Visual)this.border, Window.GetWindow(this));
                //如果不抖动，就会出现崩溃
                ++flag;
                if (flag % 2 == 0)
                    this.Height = this.ActualHeight + 1.0;
                else
                    this.Height = this.ActualHeight - 1.0;
                //MessageBox.Show("OK");
                e.Handled = true;
            }
        }  

        
    }
}
