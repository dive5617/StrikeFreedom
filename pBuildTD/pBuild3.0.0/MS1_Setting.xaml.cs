using OxyPlot;
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
    /// MS1_Setting.xaml 的交互逻辑
    /// </summary>
    public partial class MS1_Setting : Window
    {
        private OxyColor t_color, m_color, o_color;
        private MainWindow mainW;

        public MS1_Setting(MainWindow mainW)
        {
            InitializeComponent();
            this.mainW = mainW;
            this.t_color = this.mainW.Dis_help.ddhms1.theory_color;
            this.theory_color_btn.Background = new SolidColorBrush(Color.FromArgb(this.t_color.A, this.t_color.R, this.t_color.G, this.t_color.B));
            this.m_color = this.mainW.Dis_help.ddhms1.mgf_color;
            this.mgf_color_btn.Background = new SolidColorBrush(Color.FromArgb(this.m_color.A, this.m_color.R, this.m_color.G, this.m_color.B));
            this.o_color = this.mainW.Dis_help.ddhms1.other_color;
            this.other_color_btn.Background = new SolidColorBrush(Color.FromArgb(this.o_color.A, this.o_color.R, this.o_color.G, this.o_color.B));
            
            this.theory_markerType_cb.Text = mainW.Dis_help.ddhms1.theory_marker.ToString();
            this.mgf_markerType_cb.Text = mainW.Dis_help.ddhms1.mgf_marker.ToString();
            this.other_markerType_cb.Text = mainW.Dis_help.ddhms1.other_marker.ToString();

            this.theory_size_cb.Text = mainW.Dis_help.ddhms1.theory_size.ToString("F0");
            this.mgf_size_cb.Text = mainW.Dis_help.ddhms1.mgf_size.ToString("F0");
            this.other_size_cb.Text = mainW.Dis_help.ddhms1.other_size.ToString("F0");
            this.peak_size_cb.Text = mainW.Dis_help.ddhms1.peak_size.ToString("F0");
        }

        private void btn_clk(object sender, RoutedEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            var btn = sender as Button;
            if (btn == this.theory_color_btn)
                colorDialog.SelectedColor = Color.FromArgb(t_color.A, t_color.R, t_color.G, t_color.B);
            else if (btn == this.mgf_color_btn)
                colorDialog.SelectedColor = Color.FromArgb(m_color.A, m_color.R, m_color.G, m_color.B);
            else if (btn == this.other_color_btn)
                colorDialog.SelectedColor = Color.FromArgb(o_color.A, o_color.R, o_color.G, o_color.B);
            colorDialog.Owner = this;
            if ((bool)colorDialog.ShowDialog())
            {
                Color selected_color = colorDialog.SelectedColor;
                if (btn == this.theory_color_btn)
                {
                    t_color = OxyColor.FromArgb(selected_color.A, selected_color.R, selected_color.G, selected_color.B);
                    this.theory_color_btn.Background = new SolidColorBrush(Color.FromArgb(this.t_color.A, this.t_color.R, this.t_color.G, this.t_color.B));
                }
                else if (btn == this.mgf_color_btn)
                {
                    m_color = OxyColor.FromArgb(selected_color.A, selected_color.R, selected_color.G, selected_color.B);
                    this.mgf_color_btn.Background = new SolidColorBrush(Color.FromArgb(this.m_color.A, this.m_color.R, this.m_color.G, m_color.B));
                }
                else if (btn == this.other_color_btn)
                {
                    o_color = OxyColor.FromArgb(selected_color.A, selected_color.R, selected_color.G, selected_color.B);
                    this.other_color_btn.Background = new SolidColorBrush(Color.FromArgb(this.o_color.A, this.o_color.R, this.o_color.G, this.o_color.B));
                }
            }
        }

        private MarkerType get_markerType(ComboBox cb)
        {
            MarkerType result = MarkerType.Circle;
            ComboBoxItem cbi = cb.SelectedItem as ComboBoxItem;
            switch (cbi.Content as string)
            {
                case "Circle":
                    result = MarkerType.Circle;
                    break;
                case "Cross":
                    result = MarkerType.Cross;
                    break;
                case "Diamond":
                    result = MarkerType.Diamond;
                    break;
                case "None":
                    result = MarkerType.None;
                    break;
                case "Plus":
                    result = MarkerType.Plus;
                    break;
                case "Square":
                    result = MarkerType.Square;
                    break;
                case "Star":
                    result = MarkerType.Star;
                    break;
            }
            return result;
        }
        private double get_size(ComboBox cb)
        {
            double size = 4;
            ComboBoxItem cbi = cb.SelectedItem as ComboBoxItem;
            string cbi_str = cbi.Content as string;
            size = double.Parse(cbi_str);
            return size;
        }

        private void update(object sender, RoutedEventArgs e)
        {
            mainW.Dis_help.ddhms1.theory_marker = get_markerType(this.theory_markerType_cb);
            mainW.Dis_help.ddhms1.mgf_marker = get_markerType(this.mgf_markerType_cb);
            mainW.Dis_help.ddhms1.other_marker = get_markerType(this.other_markerType_cb);
            mainW.Dis_help.ddhms1.theory_color = t_color;
            mainW.Dis_help.ddhms1.mgf_color = m_color;
            mainW.Dis_help.ddhms1.other_color = o_color;
            mainW.Dis_help.ddhms1.theory_size = get_size(this.theory_size_cb);
            mainW.Dis_help.ddhms1.mgf_size = get_size(this.mgf_size_cb);
            mainW.Dis_help.ddhms1.other_size = get_size(this.other_size_cb);
            mainW.Dis_help.ddhms1.peak_size = get_size(this.peak_size_cb);
            mainW.window_sizeChg_Or_ZommPan_ms1();
            //this.Close();
        }

        private void closed_event(object sender, EventArgs e)
        {
            this.mainW.ms1_setting_dialog = null;
        }
    }
}
