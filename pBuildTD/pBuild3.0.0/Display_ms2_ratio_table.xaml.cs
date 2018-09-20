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
    /// Display_ms2_ratio_table.xaml 的交互逻辑
    /// </summary>
    public partial class Display_ms2_ratio_table : Window
    {
        public MainWindow mainW;
        public List<List<PEAK>> peaks;
        public List<string> annotations;
        public Display_ms2_ratio_table(MainWindow mainW, List<List<PEAK>> peaks, List<string> annotations)
        {
            InitializeComponent();
            this.mainW = mainW;
            this.peaks = peaks;
            this.annotations = annotations;

            Initialize();
        }

        private void Initialize()
        {
            if (this.peaks.Count < 2)
                return;
            List<PEAK> peak1 = new List<PEAK>();
            List<PEAK> peak2 = new List<PEAK>();
            List<string> annotations_new = new List<string>();
            for (int i = 0; i < this.peaks[0].Count; ++i)
            {
                if (peaks[0][i].Intensity != 0.0 && peaks[1][i].Intensity != 0.0 && peaks[0][i].Mass != peaks[1][i].Mass)
                {
                    if (peaks[0][i].Mass >= mainW.ms2_quant_help.mz1 && peaks[0][i].Mass <= mainW.ms2_quant_help.mz2 &&
                            peaks[1][i].Mass >= mainW.ms2_quant_help.mz1 && peaks[1][i].Mass <= mainW.ms2_quant_help.mz2)
                    {
                        peak1.Add(peaks[0][i]);
                        peak2.Add(peaks[1][i]);
                        annotations_new.Add(annotations[i]);
                    }
                }
            }
            const double margin = 3;
            RowDefinition rd0 = new RowDefinition();
            rd0.Height = new GridLength();
            this.psm_ratio_table.RowDefinitions.Add(rd0);
            for (int i = 0; i <= 6; ++i)
            {
                ColumnDefinition cd = new ColumnDefinition();
                this.psm_ratio_table.ColumnDefinitions.Add(cd);
                TextBlock tb = new TextBlock();
                switch (i)
                {
                    case 0:
                        tb.Text = "#";
                        break;
                    case 1:
                        tb.Text = "Pair1 MZ";
                        break;
                    case 2:
                        tb.Text = "Pair1 Intensity (%)";
                        break;
                    case 3:
                        tb.Text = "Pair2 MZ";
                        break;
                    case 4:
                        tb.Text = "Pair2 Intensity (%)";
                        break;
                    case 5:
                        tb.Text = "Mass Deviation";
                        break;
                    case 6:
                        tb.Text = "H/L Ratio";
                        break;
                }
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                tb.Margin = new Thickness(margin);
                Grid.SetColumn(tb, i);
                Grid.SetRow(tb, 0);
                this.psm_ratio_table.Children.Add(tb);
            }
            for (int i = 0; i < peak1.Count; ++i)
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = new GridLength();
                this.psm_ratio_table.RowDefinitions.Add(rd);

                TextBlock tb0 = new TextBlock();
                tb0.Text = annotations_new[i];
                tb0.HorizontalAlignment = HorizontalAlignment.Center;
                tb0.Margin = new Thickness(margin);
                Grid.SetColumn(tb0, 0);
                Grid.SetRow(tb0, this.psm_ratio_table.RowDefinitions.Count - 1);
                this.psm_ratio_table.Children.Add(tb0);
                TextBlock tb = new TextBlock();
                tb.Text = peak1[i].Mass.ToString("F5");
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                tb.Margin = new Thickness(margin);
                Grid.SetColumn(tb, 1);
                Grid.SetRow(tb, this.psm_ratio_table.RowDefinitions.Count - 1);
                this.psm_ratio_table.Children.Add(tb);
                TextBlock tb1 = new TextBlock();
                tb1.Text = peak1[i].Intensity.ToString("F5");
                tb1.HorizontalAlignment = HorizontalAlignment.Center;
                tb1.Margin = new Thickness(margin);
                Grid.SetColumn(tb1, 2);
                Grid.SetRow(tb1, this.psm_ratio_table.RowDefinitions.Count - 1);
                this.psm_ratio_table.Children.Add(tb1);

                TextBlock tb2 = new TextBlock();
                tb2.Text = peak2[i].Mass.ToString("F5");
                tb2.HorizontalAlignment = HorizontalAlignment.Center;
                tb2.Margin = new Thickness(margin);
                Grid.SetColumn(tb2, 3);
                Grid.SetRow(tb2, this.psm_ratio_table.RowDefinitions.Count - 1);
                this.psm_ratio_table.Children.Add(tb2);
                TextBlock tb3 = new TextBlock();
                tb3.Text = peak2[i].Intensity.ToString("F5");
                tb3.HorizontalAlignment = HorizontalAlignment.Center;
                tb3.Margin = new Thickness(margin);
                Grid.SetColumn(tb3, 4);
                Grid.SetRow(tb3, this.psm_ratio_table.RowDefinitions.Count - 1);
                this.psm_ratio_table.Children.Add(tb3);

                TextBlock tb4 = new TextBlock();
                tb4.Text = (peak1[i].Mass - peak2[i].Mass).ToString("F5");
                tb4.HorizontalAlignment = HorizontalAlignment.Center;
                tb4.Margin = new Thickness(margin);
                Grid.SetColumn(tb4, 5);
                Grid.SetRow(tb4, this.psm_ratio_table.RowDefinitions.Count - 1);
                this.psm_ratio_table.Children.Add(tb4);

                TextBlock tb5 = new TextBlock();
                tb5.Text = (peak2[i].Intensity / peak1[i].Intensity).ToString("F3");
                tb5.HorizontalAlignment = HorizontalAlignment.Center;
                tb5.Margin = new Thickness(margin);
                Grid.SetColumn(tb5, 6);
                Grid.SetRow(tb5, this.psm_ratio_table.RowDefinitions.Count - 1);
                this.psm_ratio_table.Children.Add(tb5);
            }
        }
    }
}
