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

namespace pBuild
{
    /// <summary>
    /// MS1_Advance.xaml 的交互逻辑
    /// </summary>
    public partial class MS1_Advance : Window
    {
        MainWindow mainW;
        public double old_ms1_mass_error = 0.0;
        public MS1_Advance(MainWindow mainW)
        {
            InitializeComponent();
            this.mainW = mainW;
            this.intensity_t_tb.Text = mainW.Intensity_t.ToString("E1");
            this.mass_error_tb.Text = mainW.ms1_mass_error.ToString("E1");
            this.start_scan_tb.Text = Display_Detail_Help_MS1.Start_Scan.ToString();
            this.end_scan_tb.Text = Display_Detail_Help_MS1.End_Scan.ToString();
            old_ms1_mass_error = mainW.ms1_mass_error;
        }

        private void update_clk(object sender, RoutedEventArgs e)
        {
            string intensity_str = this.intensity_t_tb.Text;
            string masserror_str = this.mass_error_tb.Text;
            string start_scan_str = this.start_scan_tb.Text;
            string end_scan_str = this.end_scan_tb.Text;
            try
            {
                double intensity = double.Parse(intensity_str);
                double mass_error = double.Parse(masserror_str);
                Display_Detail_Help_MS1.Start_Scan = int.Parse(start_scan_str);
                Display_Detail_Help_MS1.End_Scan = int.Parse(end_scan_str);
                if (intensity < 1e4)
                {
                    MessageBox.Show("The intensity threshold must be Larger than Or Equal 1e4");
                    return;
                }
                if (mass_error < 5e-6)
                {
                    MessageBox.Show("The mass tolerance must be Larger than Or Equal 5e-6");
                    return;
                }
                if (mass_error != old_ms1_mass_error) //如果质量误差不一样，说明用户进行改变，需要重新使用Emass计算一下3D图
                {
                    mainW.new_peptide = mainW.Dis_help.Psm_help.Pep;
                }
                mainW.Intensity_t = intensity;
                mainW.ms1_mass_error = mass_error;
                mainW.window_sizeChg_Or_ZommPan_ms1();
            }
            catch (Exception exe)
            {
                MessageBox.Show("The number must be double!");
                return;
            }
        }

        private void closed_event(object sender, EventArgs e)
        {
            this.mainW.ms1_advance_dialog = null;
        }
    }
}
