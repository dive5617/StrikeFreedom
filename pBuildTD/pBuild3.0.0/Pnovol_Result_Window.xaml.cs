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
    /// Pnovol_Result.xaml 的交互逻辑
    /// </summary>
    public partial class Pnovol_Result_Window : Window
    {
        public ObservableCollection<Pnovo_Result> Pnovo_results;
        public MS2_Help MS2_help;
        public Pnovol_Result_Window(List<Pnovo_Result> Pnovo_results, MS2_Help ms2_help)
        {
            InitializeComponent();
            this.Pnovo_results = new ObservableCollection<Pnovo_Result>(Pnovo_results);
            this.pNovo_result_grid.ItemsSource = this.Pnovo_results;
            this.MS2_help = ms2_help;
        }

        private void select_clk(object sender, SelectionChangedEventArgs e)
        {
            Pnovo_Result pr = this.pNovo_result_grid.SelectedItem as Pnovo_Result;
            this.MS2_help.Den_help.refresh_pNovol(pr);
            this.MS2_help.window_sizeChg_Or_ZoomPan();
        }

    }
}
