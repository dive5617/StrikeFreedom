using OxyPlot;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
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
    /// Ms2_setting.xaml 的交互逻辑
    /// </summary>
    public partial class Ms2_setting : Window
    {
        private bool changed_ladder = false;
        private MainWindow mainWindow;
        OxyColor A_color, B_color, C_color, X_color, Y_color, Z_color;
        OxyColor M_color, I_color, O_color;
        public Ms2_setting()
        {
            InitializeComponent();
        }

        public Ms2_setting(MainWindow mainW)
        {
            InitializeComponent();
            this.mainWindow = mainW;
            //添加所有的系统字体
            InstalledFontCollection ifc = new InstalledFontCollection();
            System.Drawing.FontFamily[] ffs = ifc.Families;
            foreach (System.Drawing.FontFamily ff in ffs)
            {
                ComboBoxItem cbi = new ComboBoxItem();
                cbi.FontFamily = new FontFamily(ff.GetName(1));
                cbi.Content = ff.GetName(1);
                if (ff.GetName(1) == "Calibri") //Arial Unicode MS
                    cbi.IsSelected = true;
                this.Font.Items.Add(cbi);
            }


            this.Font.Text = this.mainWindow.Dis_help.ddh.Font_SQ;
            int font_size = (int)(this.mainWindow.Dis_help.ddh.FontSize_SQ + 0.5);
            if (font_size < 10)
                font_size = 10;
            this.FontSize.Text = font_size + "";
            //this.FontSize.Text = 20 + "";
            this.MatchWeight.Text = this.mainWindow.Dis_help.ddh.Match_StrokeWidth + "";
            this.NoMatchWeight.Text = this.mainWindow.Dis_help.ddh.NoNMatch_StrokeWidth + "";
            this.MZWeight.Text = this.mainWindow.Dis_help.ddh.ME_Weight.ToString("N1");
            this.A_color = this.mainWindow.Dis_help.ddh.A_Match_Color;
            this.A_btn.Background = new SolidColorBrush(Color.FromArgb(this.A_color.A, this.A_color.R, this.A_color.G, this.A_color.B));
            this.B_color = this.mainWindow.Dis_help.ddh.B_Match_Color;
            this.B_btn.Background = new SolidColorBrush(Color.FromArgb(this.B_color.A, this.B_color.R, this.B_color.G, this.B_color.B));
            this.C_color = this.mainWindow.Dis_help.ddh.Y_Match_Color;
            this.C_btn.Background = new SolidColorBrush(Color.FromArgb(this.C_color.A, this.C_color.R, this.C_color.G, this.C_color.B));
            this.X_color = this.mainWindow.Dis_help.ddh.X_Match_Color;
            this.X_btn.Background = new SolidColorBrush(Color.FromArgb(this.X_color.A, this.X_color.R, this.X_color.G, this.X_color.B));
            this.Y_color = this.mainWindow.Dis_help.ddh.Y_Match_Color;
            this.Y_btn.Background = new SolidColorBrush(Color.FromArgb(this.Y_color.A, this.Y_color.R, this.Y_color.G, this.Y_color.B));
            this.Z_color = this.mainWindow.Dis_help.ddh.Z_Match_Color;
            this.Z_btn.Background = new SolidColorBrush(Color.FromArgb(this.Z_color.A, this.Z_color.R, this.Z_color.G, this.Z_color.B));
            this.M_color = this.mainWindow.Dis_help.ddh.M_Match_Color;
            this.M_btn.Background = new SolidColorBrush(Color.FromArgb(this.M_color.A, this.M_color.R, this.M_color.G, this.M_color.B));
            this.I_color = this.mainWindow.Dis_help.ddh.I_Match_Color;
            this.I_btn.Background = new SolidColorBrush(Color.FromArgb(this.I_color.A, this.I_color.R, this.I_color.G, this.I_color.B));
            this.O_color = this.mainWindow.Dis_help.ddh.O_Match_Color;
            this.O_btn.Background = new SolidColorBrush(Color.FromArgb(this.O_color.A, this.O_color.R, this.O_color.G, this.O_color.B));
        }

        private void update(object sender, RoutedEventArgs e)
        {
            this.mainWindow.Dis_help.ddh.Font_SQ = this.Font.Text;
            this.mainWindow.Dis_help.ddh.Font_BY = this.Font.Text;
            this.mainWindow.Dis_help.ddh.Match_StrokeWidth = double.Parse(this.MatchWeight.Text);
            this.mainWindow.Dis_help.ddh.NoNMatch_StrokeWidth = double.Parse(this.NoMatchWeight.Text);
            this.mainWindow.Dis_help.ddh.ME_Weight = double.Parse(this.MZWeight.Text);
            this.mainWindow.Dis_help.ddh.FontSize_SQ = double.Parse(this.FontSize.Text);
            this.mainWindow.Dis_help.ddh.FontSize_BY = this.mainWindow.Dis_help.ddh.FontSize_SQ * 5 / 8;
            this.mainWindow.Dis_help.ddh.A_Match_Color = this.A_color;
            this.mainWindow.Dis_help.ddh.B_Match_Color = this.B_color;
            this.mainWindow.Dis_help.ddh.C_Match_Color = this.C_color;
            this.mainWindow.Dis_help.ddh.X_Match_Color = this.X_color;
            this.mainWindow.Dis_help.ddh.Y_Match_Color = this.Y_color;
            this.mainWindow.Dis_help.ddh.Z_Match_Color = this.Z_color;
            this.mainWindow.Dis_help.ddh.M_Match_Color = this.M_color;
            this.mainWindow.Dis_help.ddh.I_Match_Color = this.I_color;
            this.mainWindow.Dis_help.ddh.O_Match_Color = this.O_color;
            if (changed_ladder)
            {
                this.mainWindow.Dis_help.ddh.FontSize_SQ = 0.0;
                this.mainWindow.Dis_help.ddh.FontSize_BY = 0.0;
            }
            MS2_Help ms2_help = new MS2_Help(this.mainWindow.Model2, this.mainWindow.Dis_help, Ladder_Help.Scale_Width, Ladder_Help.Scale_Height);
            ms2_help.window_sizeChg_Or_ZoomPan();
            //this.Close();
        }

        private void sel_Color(object sender, RoutedEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            var btn=sender as Button;
            if (btn == this.A_btn)
                colorDialog.SelectedColor = Color.FromArgb(A_color.A, A_color.R, A_color.G, A_color.B);
            else if (btn == this.B_btn)
                colorDialog.SelectedColor = Color.FromArgb(B_color.A, B_color.R, B_color.G, B_color.B);
            else if (btn == this.C_btn)
                colorDialog.SelectedColor = Color.FromArgb(C_color.A, C_color.R, C_color.G, C_color.B);
            else if (btn == this.X_btn)
                colorDialog.SelectedColor = Color.FromArgb(X_color.A, X_color.R, X_color.G, X_color.B);
            else if (btn == this.Y_btn)
                colorDialog.SelectedColor = Color.FromArgb(Y_color.A, Y_color.R, Y_color.G, Y_color.B);
            else if (btn == this.Z_btn)
                colorDialog.SelectedColor = Color.FromArgb(Z_color.A, Z_color.R, Z_color.G, Z_color.B);
            else if (btn == this.M_btn)
                colorDialog.SelectedColor = Color.FromArgb(M_color.A, M_color.R, M_color.G, M_color.B);
            else if (btn == this.I_btn)
                colorDialog.SelectedColor = Color.FromArgb(I_color.A, I_color.R, I_color.G, I_color.B);
            else if (btn == this.O_btn)
                colorDialog.SelectedColor = Color.FromArgb(O_color.A, O_color.R, O_color.G, O_color.B);
            colorDialog.Owner = this;
            if ((bool)colorDialog.ShowDialog())
            {
                Color selected_color = colorDialog.SelectedColor;
                if (btn == this.A_btn)
                {
                    A_color = OxyColor.FromArgb(selected_color.A,selected_color.R,selected_color.G,selected_color.B);
                    this.A_btn.Background = new SolidColorBrush(Color.FromArgb(this.A_color.A, this.A_color.R, this.A_color.G, this.A_color.B));
                }
                else if (btn == this.B_btn)
                {
                    B_color = OxyColor.FromArgb(selected_color.A, selected_color.R, selected_color.G, selected_color.B);
                    this.B_btn.Background = new SolidColorBrush(Color.FromArgb(this.B_color.A, this.B_color.R, this.B_color.G, this.B_color.B));
                }
                else if (btn == this.C_btn)
                {
                    C_color = OxyColor.FromArgb(selected_color.A, selected_color.R, selected_color.G, selected_color.B);
                    this.C_btn.Background = new SolidColorBrush(Color.FromArgb(this.C_color.A, this.C_color.R, this.C_color.G, this.C_color.B));
                }
                else if (btn == this.X_btn)
                {
                    X_color = OxyColor.FromArgb(selected_color.A, selected_color.R, selected_color.G, selected_color.B);
                    this.X_btn.Background = new SolidColorBrush(Color.FromArgb(this.X_color.A, this.X_color.R, this.X_color.G, this.X_color.B));
                }
                else if (btn == this.Y_btn)
                {
                    Y_color = OxyColor.FromArgb(selected_color.A, selected_color.R, selected_color.G, selected_color.B);
                    this.Y_btn.Background = new SolidColorBrush(Color.FromArgb(this.Y_color.A, this.Y_color.R, this.Y_color.G, this.Y_color.B));
                }
                else if (btn == this.Z_btn)
                {
                    Z_color = OxyColor.FromArgb(selected_color.A, selected_color.R, selected_color.G, selected_color.B);
                    this.Z_btn.Background = new SolidColorBrush(Color.FromArgb(this.Z_color.A, this.Z_color.R, this.Z_color.G, this.Z_color.B));
                }
                else if (btn == this.M_btn)
                {
                    M_color = OxyColor.FromArgb(selected_color.A, selected_color.R, selected_color.G, selected_color.B);
                    this.M_btn.Background = new SolidColorBrush(Color.FromArgb(this.M_color.A, this.M_color.R, this.M_color.G, this.M_color.B));
                }
                else if (btn == this.I_btn)
                {
                    I_color = OxyColor.FromArgb(selected_color.A, selected_color.R, selected_color.G, selected_color.B);
                    this.I_btn.Background = new SolidColorBrush(Color.FromArgb(this.I_color.A, this.I_color.R, this.I_color.G, this.I_color.B));
                }
                else if (btn == this.O_btn)
                {
                    O_color = OxyColor.FromArgb(selected_color.A, selected_color.R, selected_color.G, selected_color.B);
                    this.O_btn.Background = new SolidColorBrush(Color.FromArgb(this.O_color.A, this.O_color.R, this.O_color.G, this.O_color.B));
                }
            }
        }

        private void font_chd(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem font_str = this.Font.SelectedValue as ComboBoxItem;
            if (font_str != null)
                this.Font.FontFamily = new FontFamily(font_str.Content as string);
        }

        //显示高级设置框
        private void advance_setting_btn_clk(object sender, RoutedEventArgs e)
        {
            Button advance_btn = sender as Button;
            if (advance_btn.Content.ToString() == "+")
            {
                advance_btn.Content = "-";
                this.advance_setting_grid.Visibility = Visibility.Visible; //有问题
                this.Width = 520;
                this.ladder_height_cbb.Text = Display_Detail_Help.Ladder_Height.ToString("F2");
            }
            else
            {
                advance_btn.Content = "+";
                this.advance_setting_grid.Visibility = Visibility.Hidden; //有问题
                this.Width = 320;
            }
        }

        private void ladder_height_cbb_chd(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selected_item = this.ladder_height_cbb.SelectedItem as ComboBoxItem;
            if (selected_item.Content == null)
                return;
            //Display_Detail_Help.Old_Ladder_Height = Display_Detail_Help.Ladder_Height;
            Display_Detail_Help.Ladder_Height = double.Parse(selected_item.Content.ToString());
            changed_ladder = true;
        }

        private void window_enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                update(null, null);
            }
        }

        private void closed_event(object sender, EventArgs e)
        {
            this.mainWindow.ms2_setting_dialog = null;
        }
    }
}
