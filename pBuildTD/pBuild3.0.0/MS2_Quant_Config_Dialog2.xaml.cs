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
    /// MS2_Quant_Config_Dialog2.xaml 的交互逻辑
    /// </summary>
    public partial class MS2_Quant_Config_Dialog2 : Window
    {
        public MainWindow mainW;
        public MS2_Quant_Config_Dialog2(MainWindow mainW)
        {
            InitializeComponent();
            this.mainW = mainW;
        }

        private void add_btn_clk(object sender, RoutedEventArgs e)
        {
            const double margin = 3;
            RowDefinition rd = new RowDefinition();
            rd.Height = new GridLength();
            this.grid.RowDefinitions.Add(rd);
            TextBlock tbk = new TextBlock();
            tbk.Width = 50;
            tbk.Text = "Mass: ";
            tbk.Margin = new Thickness(margin);
            Grid.SetRow(tbk, this.grid.RowDefinitions.Count - 1);
            Grid.SetColumn(tbk, 0);
            this.grid.Children.Add(tbk);
            TextBox tbx = new TextBox();
            tbx.Width = 200;
            tbx.Margin = new Thickness(margin);
            Grid.SetRow(tbx, this.grid.RowDefinitions.Count - 1);
            Grid.SetColumn(tbx, 1);
            this.grid.Children.Add(tbx);
            TextBox tbx2 = new TextBox();
            tbx2.Width = 50;
            tbx2.Margin = new Thickness(margin);
            Grid.SetRow(tbx2, this.grid.RowDefinitions.Count - 1);
            Grid.SetColumn(tbx2, 2);
            this.grid.Children.Add(tbx2);
            ComboBox cb = new ComboBox();
            cb.Height = 20;
            cb.Width = 55;
            cb.Items.Add("ppm");
            cb.Items.Add("Da");
            cb.SelectedIndex = 0;
            Grid.SetRow(cb, this.grid.RowDefinitions.Count - 1);
            Grid.SetColumn(cb, 3);
            this.grid.Children.Add(cb);
        }

        private void ok_btn_clk(object sender, RoutedEventArgs e)
        {
            List<double> masses = new List<double>();
            List<double> mass_errors = new List<double>();
            List<int> mass_error_flags = new List<int>();
            bool flag = true;
            for (int i = 0; i < this.grid.RowDefinitions.Count; ++i)
            {
                TextBox tbx1 = this.grid.Children[i * 4 + 1] as TextBox;
                TextBox tbx2 = this.grid.Children[i * 4 + 2] as TextBox;
                ComboBox cb = this.grid.Children[i * 4 + 3] as ComboBox;
                string cb_str = cb.SelectedItem as string;
                if (!Config_Help.IsDecimalAllowed(tbx1.Text) || !Config_Help.IsDecimalAllowed(tbx2.Text))
                {
                    flag = false;
                    break;
                }
                masses.Add(double.Parse(tbx1.Text));
                double mass_error = double.Parse(tbx2.Text);
                int me_flag = 1;
                if (cb_str == "ppm")
                {
                    mass_error = mass_error * 1e-6;
                    me_flag = 0;
                }
                mass_errors.Add(mass_error);
                mass_error_flags.Add(me_flag);
            }
            if (!flag)
            {
                MessageBox.Show(Message_Help.ALL_TERM_BE_DOUBLE);
                return;
            }

            this.mainW.ms2_quant_help2 = new MS2_Quant_Help2(masses, mass_errors, mass_error_flags);
            this.Cursor = Cursors.Wait;
            this.mainW.ms2_quant2();
            this.Close();
            this.Cursor = null;
        }

        private void closed_event(object sender, EventArgs e)
        {
            this.mainW.ms2_quant_config_dialog2 = null;
        }
    }
}
