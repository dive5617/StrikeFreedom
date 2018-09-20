using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace pBuild
{
    /// <summary>
    /// Model_Window.xaml 的交互逻辑
    /// </summary>
    public partial class Model_Window_DeNovo : Window //绘制弹出形式的图
    {
        int flag = 0;
        public PlotModel Model;
        public Display_Help dis_help;
        public double scale_width;
        public double scale_height;
        public double width;
        public double height;

        public MainWindow mainW;
        public int MS1_Ratio = 0; //MS1_Ratio=0表示绘制LC/MS图，MS1_Ratio=1表示绘制scan-ratio图

        public List<PEAK> Original_Peaks = new List<PEAK>();

        public MS2_Help ms2_help;

        public Model_Window_DeNovo(MainWindow mainW, PlotModel Model, Display_Help dis_help, double width, double height) //绘制二级匹配图
        {
            InitializeComponent();
            this.scale_width = Ladder_Help.Scale_Width;
            this.scale_height = Ladder_Help.Scale_Height;
            this.width = width;
            this.height = height;
            this.Model = Model;
            this.mainW = mainW;
            ms2_help = new MS2_Help(Model, dis_help, scale_width, scale_height);
            ms2_help.Den_help.isDenovol = true;
            this.Model.Axes[1].AxisChanged += (s, e) =>
            {
                //ms2_help = new MS2_Help(Model, dis_help, scale_width, scale_height);
                //ms2_help.isDenovol = true;
                ms2_help.window_sizeChg_Or_ZoomPan();
            };
            this.dis_help = dis_help;
            this.model.Model = this.Model;
            ms2_help.window_sizeChg_Or_ZoomPan();
            this.SizeChanged += Window_sizeChg;
            this.Title = "De novol";
            this.Original_Peaks = new List<PEAK>(dis_help.Psm_help.Spec.Peaks);
        }
        private void Window_sizeChg(object sender, SizeChangedEventArgs e)
        {
            this.scale_width = this.scale_width * this.ActualWidth / this.width;
            this.scale_height = this.scale_height * this.ActualHeight / this.height;
            this.width = this.ActualWidth;
            this.height = this.ActualHeight;
            //MS2_Help ms2_help = new MS2_Help(Model, dis_help, this.scale_width, this.scale_height);
            ms2_help.Den_help.isDenovol = true;
            ms2_help.window_sizeChg_Or_ZoomPan();
        }
        private void Window_Initialized(object sender, EventArgs e)
        {
            this.Topmost = true;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if(this.dis_help != null)
                this.Topmost = false;
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

        private void clear_btn_clk(object sender, RoutedEventArgs e)
        {
            this.ms2_help.Den_help.refresh_clear();
            this.ms2_help.window_sizeChg_Or_ZoomPan();
        }

        private void end_btn_clk(object sender, RoutedEventArgs e)
        {
            this.ms2_help.Den_help.refresh_end();
            this.ms2_help.window_sizeChg_Or_ZoomPan();
        }

        private void back_btn_clk(object sender, RoutedEventArgs e)
        {
            this.ms2_help.Den_help.refresh_back();
            this.ms2_help.window_sizeChg_Or_ZoomPan();
        }

        private void config_btn_clk(object sender, RoutedEventArgs e)
        {
            MS2_Denovol_Config mdc = new MS2_Denovol_Config();
            mdc.Show();
        }

        private void initial_clk(object sender, RoutedEventArgs e)
        {
            this.ms2_help.Den_help.refresh_initial();
            this.ms2_help.window_sizeChg_Or_ZoomPan();
        }

        private void deisotope_btn_clk(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if ((bool)cb.IsChecked)
            {
                this.ms2_help.Den_help.refresh_clear();
                double max_X = this.ms2_help.dis_help.Psm_help.Spec.preprocess(this.ms2_help.dis_help.Psm_help.Ppm_mass_error,
                    this.ms2_help.dis_help.Psm_help.Da_mass_error);
                this.ms2_help.Model.Axes[1].Maximum = max_X;
                this.ms2_help.Model.Axes[1].AbsoluteMaximum = this.ms2_help.Model.Axes[1].Maximum;
                this.ms2_help.Model.Axes[1].ActualMaximum = this.ms2_help.Model.Axes[1].Maximum;
                this.ms2_help.Den_help.initial_series();
                this.ms2_help.window_sizeChg_Or_ZoomPan();
            }
            else
            {
                this.ms2_help.Den_help.refresh_clear();
                this.dis_help.Psm_help.Spec.Peaks = new ObservableCollection<PEAK>(this.Original_Peaks);
                this.ms2_help.Model.Axes[1].Maximum = this.Original_Peaks.Last().Mass;
                this.ms2_help.Model.Axes[1].AbsoluteMaximum = this.ms2_help.Model.Axes[1].Maximum;
                this.ms2_help.Model.Axes[1].ActualMaximum = this.ms2_help.Model.Axes[1].Maximum;
                this.ms2_help.Den_help.initial_series();
                this.ms2_help.window_sizeChg_Or_ZoomPan();
            }
        }

        private void setting_btn_clk(object sender, RoutedEventArgs e)
        {
            MS2_Denovol_Setting mds = new MS2_Denovol_Setting(this.ms2_help, this.ms2_help.Den_help);
            mds.Show();
        }

        private void pNovo_btn_clk(object sender, RoutedEventArgs e)
        {
            string result_path = File_Help.pBuild_tmp_file + "\\";
            if (mainW.task != null)
                result_path = mainW.task.folder_result_path + File_Help.pBuild_tmp_file + "\\";
            if (!Directory.Exists(result_path))
                Directory.CreateDirectory(result_path);
            Pnovo_Help ph = new Pnovo_Help(result_path + "One_MGF.mgf", result_path, this.dis_help.Psm_help.Spec);
            ph.write_MGF();
            ph.write_pNovo_param_file();
            ph.run_pnovo();
            List<Pnovo_Result> prs = ph.get_results();
            Pnovol_Result_Window prw = new Pnovol_Result_Window(prs, this.ms2_help);
            prw.Show();
        }
      
        private void copy_to_clipboard_btn_clk(object sender, RoutedEventArgs e)
        {
            if (this.Model != null)
            {
                mainW.export_svg(this.model);
                EMFCopy.CopyVisualToWmfClipboard((Visual)this.model, Window.GetWindow(this));
                //还原之前的图，不还原会有一个小bug
                grid.Height = grid.ActualHeight + 1.0e-10;
            }
        }  
       
    }
}
