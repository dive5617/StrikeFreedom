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
    /// MS2_Denovol_Setting.xaml 的交互逻辑
    /// </summary>
    public partial class MS2_Denovol_Setting : Window
    {
        public MS2_Help Ms2_help;
        public Denovol_Help Denovol_help;
        public int Color_flag = 2; //0:N_color, 1:C_color, 2:A_color
        public MS2_Denovol_Setting(MS2_Help ms2_help, Denovol_Help den_help)
        {
            InitializeComponent();
            this.Ms2_help = ms2_help;
            this.Denovol_help = den_help;
            if (Display_Help.IsEqual(this.Denovol_help.b_color, this.Denovol_help.current_color))
                Color_flag = 0;
            else if (Display_Help.IsEqual(this.Denovol_help.y_color, this.Denovol_help.current_color))
                Color_flag = 1;
            else if (Display_Help.IsEqual(this.Denovol_help.default_color, this.Denovol_help.current_color))
                Color_flag = 2;
            Color n_color = Color.FromArgb(this.Denovol_help.b_color.A,this.Denovol_help.b_color.R,
                this.Denovol_help.b_color.G, this.Denovol_help.b_color.B);
            Color c_color = Color.FromArgb(this.Denovol_help.y_color.A, this.Denovol_help.y_color.R,
                this.Denovol_help.y_color.G, this.Denovol_help.y_color.B);
            Color a_color = Color.FromArgb(this.Denovol_help.default_color.A, this.Denovol_help.default_color.R,
                this.Denovol_help.default_color.G, this.Denovol_help.default_color.B);
            this.N_btn.Background = new SolidColorBrush(n_color);
            this.C_btn.Background = new SolidColorBrush(c_color);
            this.A_btn.Background = new SolidColorBrush(a_color);
        }
        
        private void sel_Color(object sender, RoutedEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            var btn = sender as Button;
            if (btn == this.N_btn)
                colorDialog.SelectedColor = Color.FromArgb(Denovol_help.b_color.A, Denovol_help.b_color.R, Denovol_help.b_color.G, Denovol_help.b_color.B);
            else if (btn == this.C_btn)
                colorDialog.SelectedColor = Color.FromArgb(Denovol_help.y_color.A, Denovol_help.y_color.R, Denovol_help.y_color.G, Denovol_help.y_color.B);
            else if (btn == this.A_btn)
                colorDialog.SelectedColor = Color.FromArgb(Denovol_help.default_color.A, Denovol_help.default_color.R, Denovol_help.default_color.G, Denovol_help.default_color.B);
            
            colorDialog.Owner = this;
            if ((bool)colorDialog.ShowDialog())
            {
                Color selected_color = colorDialog.SelectedColor;
                if (btn == this.N_btn)
                {
                    Denovol_help.b_color = OxyColor.FromArgb(selected_color.A, selected_color.R, selected_color.G, selected_color.B);
                    this.N_btn.Background = new SolidColorBrush(Color.FromArgb(selected_color.A, selected_color.R, selected_color.G, selected_color.B));
                    if (this.Color_flag == 0)
                        Denovol_help.current_color = Denovol_help.b_color;
                }
                else if (btn == this.C_btn)
                {
                    Denovol_help.y_color = OxyColor.FromArgb(selected_color.A, selected_color.R, selected_color.G, selected_color.B);
                    this.C_btn.Background = new SolidColorBrush(Color.FromArgb(selected_color.A, selected_color.R, selected_color.G, selected_color.B));
                    if (this.Color_flag == 1)
                        Denovol_help.current_color = Denovol_help.y_color;
                }
                else if (btn == this.A_btn)
                {
                    Denovol_help.default_color = OxyColor.FromArgb(selected_color.A, selected_color.R, selected_color.G, selected_color.B);
                    this.A_btn.Background = new SolidColorBrush(Color.FromArgb(selected_color.A, selected_color.R, selected_color.G, selected_color.B));
                    if (this.Color_flag == 2)
                        Denovol_help.current_color = Denovol_help.default_color;
                }
            }
        }

        private void update_btn_clk(object sender, RoutedEventArgs e)
        {
            Ms2_help.window_sizeChg_Or_ZoomPan();
        }
    }
}
