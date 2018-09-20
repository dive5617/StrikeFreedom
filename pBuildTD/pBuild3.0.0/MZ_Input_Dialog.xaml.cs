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
    /// MZ_Input_Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class MZ_Input_Dialog : Window
    {
        MainWindow mainW;
        public MZ_Input_Dialog(MainWindow mainW)
        {
            InitializeComponent();
            this.mainW = mainW;
        }

        private void range_clk(object sender, RoutedEventArgs e)
        {
            PlotModel model = null;
            if (mainW.display_tab.SelectedIndex == 0) //显示的是MS1
                model = mainW.Model1;
            else if (mainW.display_tab.SelectedIndex == 1) //显示的是MS2
                model = mainW.Model2;
            double min_mz = model.Axes[1].AbsoluteMinimum;
            double max_mz = model.Axes[1].AbsoluteMaximum;
            if (Config_Help.IsDecimalAllowed(this.minMZ_txt.Text))
                min_mz = double.Parse(this.minMZ_txt.Text);
            if (Config_Help.IsDecimalAllowed(this.maxMZ_txt.Text))
                max_mz = double.Parse(this.maxMZ_txt.Text);
            mainW.zoom(min_mz, max_mz, model);
        }

        private void enter_keyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;
            range_clk(null, null);
        }

        private void closed_event(object sender, EventArgs e)
        {
            this.mainW.mz_input_dialog = null;
        }
    }
}
