using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace pBuild
{
    public class Display_Help
    {
        public MainWindow mainW;

        private PSM_Help_Parent psm_help;
        public PSM_Help_Parent Psm_help
        {
            get { return psm_help; }
            set { psm_help = value; }
        }
        
        private Spectra_MS1 ms1;
        public Spectra_MS1 Ms1
        {
            get { return ms1; }
            set { ms1 = value; }
        }
        //显示FDR曲线
        public int[] FDR_bin_count;

        //显示Da\ppm的质量误差
        public List<IDataPoint> Target_points;
        public List<IDataPoint> Decoy_points;
        public List<int> Target_psm_indexs;
        public List<int> Decoy_psm_indexs;

        public double Max_mass_error, Max_score;

        //显示正反库的打分分布
        public List<double> All_score_bins;
        public List<int> Target_score_bin_numbers;
        public List<int> Decoy_score_bin_numbers;
        public double Max_Number;

        //显示混合谱图的比例
        public List<Mixed_Spectra> mixed_spectra_count;

        //显示特异、半特异、非特异的比例
        public List<double> Specific_percentage;

        //显示意外修饰的比例
        public List<Identification_Modification> Modifications;

        //显示鉴定到的肽段的长度分布
        public List<Length_Distribution> Length_percentage;

        //显示鉴定到的肽段的遗漏酶切位点的分布
        public List<Missed_Cleavage_Distribution> Missed_cleavage_percentage;

        //显示定量比值的直方图
        public List<double> All_ratio_bins;
        public List<int> All_ratio_bin_numbers;

        //显示各个RAW的解析率的直方图
        public List<Raw_Rate> Raw_Rates;

        //显示质量误差的箱线图
        public List<BoxPlot_Help> BoxPlots;
        public List<MassError_Raw> MassError_Raws;

        //绘制按扫描号的质量图
        public List<Scan_Raw> Scan_Raws;

        //绘制任意两根峰的色谱曲线
        public List<PEAK_MS1> peaks1;
        public List<PEAK_MS1> peaks2;

        //绘制一级谱图的m/z-scan图，ms1_list[0]表示碎裂谱峰，ms1_list[1]表示鉴定到的谱峰
        //横轴为m/z，纵轴为Scan
        public List<List<Spectra_MS1>> ms1_list;

        //绘制比值随着扫描号的变化情况
        public List<double> all_scans;
        public List<int> scan_bin_list;

        public Display_Detail_Help_MS1 ddhms1 = new Display_Detail_Help_MS1();
        public Display_Detail_Help ddh = new Display_Detail_Help();
        public Display_Help()
        {
            this.ddh = new Display_Detail_Help();
        }
        public Display_Help(PSM_Help psm_help, Display_Detail_Help ddh, Display_Detail_Help_MS1 ddhms1)
        {
            this.psm_help = psm_help;
            this.ddh = ddh;
            this.ddhms1 = ddhms1;
        }
        public Display_Help(PSM_Help_Parent psm_help, Display_Detail_Help ddh, Display_Detail_Help_MS1 ddhms1)
        {
            this.psm_help = psm_help;
            this.ddh = ddh;
            this.ddhms1 = ddhms1;
        }
        public Display_Help(pLink.PSM_Help_2 psm_help_2, Display_Detail_Help ddh, Display_Detail_Help_MS1 ddhms1)
        {
            this.psm_help = psm_help_2;
            this.ddh = ddh;
            this.ddhms1 = ddhms1;
        }
        public Display_Help(Spectra_MS1 ms1, PSM_Help psm_help, Display_Detail_Help ddh, Display_Detail_Help_MS1 ddhms1)
        {
            this.ms1 = ms1;
            this.psm_help = psm_help;
            this.ddh = ddh;
            this.ddhms1 = ddhms1;
        }
        public Display_Help(Spectra_MS1 ms1, PSM_Help_Parent psm_help, Display_Detail_Help ddh, Display_Detail_Help_MS1 ddhms1)
        {
            this.ms1 = ms1;
            this.psm_help = psm_help;
            this.ddh = ddh;
            this.ddhms1 = ddhms1;
        }
        public Display_Help(int[] FDR_bin_count)
        {
            this.FDR_bin_count = FDR_bin_count;
        }
        public Display_Help(List<IDataPoint> Target_points, List<IDataPoint> Decoy_points
            , double Max_mass_error, double Max_score, MainWindow mainW)
        {
            this.Target_points = Target_points;
            this.Decoy_points = Decoy_points;
            this.Max_mass_error = Max_mass_error;
            this.Max_score = Max_score;
            this.mainW = mainW;
        }
        public Display_Help(List<double> All_score_bins, List<int> Target_score_bin_numbers, List<int> Decoy_score_bin_numbers,
            double Max_Number)
        {
            this.All_score_bins = All_score_bins;
            this.Target_score_bin_numbers = Target_score_bin_numbers;
            this.Decoy_score_bin_numbers = Decoy_score_bin_numbers;
            this.Max_Number = Max_Number;
        }
        public Display_Help(List<Mixed_Spectra> mixed_spectra_count, MainWindow mainW)
        {
            this.mixed_spectra_count = mixed_spectra_count;
            this.mainW = mainW;
        }
        public Display_Help(List<double> Specific_percentage, MainWindow mainW)
        {
            this.Specific_percentage = Specific_percentage;
            this.mainW = mainW;
        }
        public Display_Help(List<Identification_Modification> Modifications, MainWindow mainW)
        {
            this.Modifications = Modifications;
            this.mainW = mainW;
        }
        public Display_Help(List<Length_Distribution> lengths, MainWindow mainW)
        {
            this.Length_percentage = lengths;
            this.mainW = mainW;
        }
        public Display_Help(List<Missed_Cleavage_Distribution> missed_cleavages, MainWindow mainW)
        {
            this.Missed_cleavage_percentage = missed_cleavages;
            this.mainW = mainW;
        }
        public Display_Help(List<double> all_ratio_bins, List<int> all_ratio_bin_numbers, double max_number)
        {
            this.All_ratio_bins = all_ratio_bins;
            this.All_ratio_bin_numbers = all_ratio_bin_numbers;
            this.Max_Number = max_number;
        }
        public Display_Help(List<Raw_Rate> Raw_Rates, MainWindow mainW)
        {
            this.Raw_Rates = Raw_Rates;
            this.mainW = mainW;
        }
        public Display_Help(List<BoxPlot_Help> BoxPlots, List<MassError_Raw> MassError_Raws, List<Scan_Raw> Scan_Raws, MainWindow mainW)
        {
            this.BoxPlots = BoxPlots;
            this.MassError_Raws = MassError_Raws;
            this.Scan_Raws = Scan_Raws;
            this.mainW = mainW;
        }
        public Display_Help(List<PEAK_MS1> peaks1, List<PEAK_MS1> peaks2)
        {
            this.peaks1 = peaks1;
            this.peaks2 = peaks2;
        }

        public Display_Help(List<List<Spectra_MS1>> ms1_list, MainWindow mainW)
        {
            this.ms1_list = ms1_list;
            this.mainW = mainW;
        }
        public Display_Help(List<double> all_scans, List<int> scan_bin_list, MainWindow mainW)
        {
            this.all_scans = all_scans;
            this.scan_bin_list = scan_bin_list;
            this.mainW = mainW;
        }

        public PlotModel display_quantification()
        {
            var tmp = new PlotModel();
            tmp.Title = "Quantification Ratio Histogram";
            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.Position = AxisPosition.Bottom;
            categoryAxis1.Title = "log₂(Ratio)"; //unit²,上下标的字符串
            categoryAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            categoryAxis1.TitleFontSize = 15;
            categoryAxis1.AxisTitleDistance = 10;
            categoryAxis1.MajorStep = (All_ratio_bins.Last() - All_ratio_bins.First()) / 5;
            categoryAxis1.FontSize = 11.0;
            categoryAxis1.IsPanEnabled = false;
            categoryAxis1.IsZoomEnabled = false;
            categoryAxis1.IsTickCentered = true;
            //double ratio_offset = (All_ratio_bins[1] - All_ratio_bins[0]) / 2;
            double ratio_offset = 0.0;
            for (int i = 0; i < All_ratio_bins.Count; ++i)
            {
                categoryAxis1.Labels.Add((All_ratio_bins[i] + ratio_offset).ToString("F1"));
            }
            tmp.Axes.Add(categoryAxis1);

            var linearAxis1 = new LinearAxis();
            linearAxis1.Minimum = 0;
            linearAxis1.Maximum = Max_Number * 1.2;
            linearAxis1.Position = AxisPosition.Left;
            linearAxis1.IntervalLength = 20;
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MinorGridlineStyle = LineStyle.Dot;
            linearAxis1.Title = "Number of Ratios";
            linearAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis1.TitleFontSize = 15;
            linearAxis1.AxisTitleDistance = 10;
            linearAxis1.IsPanEnabled = false;
            linearAxis1.IsZoomEnabled = false;
            tmp.Axes.Add(linearAxis1);
            var barSeries1 = new ColumnSeries();
            barSeries1.StrokeThickness = 1;
            barSeries1.Title = "Ratio";
            barSeries1.FillColor = OxyColors.DarkOrange;
            for (int i = 0; i < All_ratio_bin_numbers.Count; ++i)
            {
                barSeries1.Items.Add(new ColumnItem(All_ratio_bin_numbers[i], -1));
            }
            tmp.Series.Add(barSeries1);
            return tmp;
        }
        public PlotModel display_length()
        {
            if (this.Length_percentage.Count == 0)
                return null;
            var tmp = new PlotModel();
            tmp.Title = "Length Percentage (%)";
            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.MinorStep = 1;
            categoryAxis1.Position = AxisPosition.Bottom;
            categoryAxis1.Title = "";
            categoryAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            categoryAxis1.TitleFontSize = 15;
            categoryAxis1.AxisTickToLabelDistance = 10;

            int max_length = 30, max_number = 0;
            double max_length_ratio = 0.0;
            List<Length_Distribution> lds = new List<Length_Distribution>();
            int ii = 0;
            for (; ii < this.Length_percentage.Count; ++ii)
            {
                if (this.Length_percentage[ii].length >= max_length)
                    break;
                lds.Add(this.Length_percentage[ii]);
            }
            for (; ii < this.Length_percentage.Count; ++ii)
            {
                max_length_ratio += this.Length_percentage[ii].ratio;
                max_number += this.Length_percentage[ii].num;
            }
            if (max_length_ratio != 0.0)
                lds.Add(new Length_Distribution(30, max_number, max_length_ratio));
            this.Length_percentage = lds;
            for (int i = 0; i < this.Length_percentage.Count; ++i)
            {
                if (this.Length_percentage[i].length != max_length)
                    categoryAxis1.Labels.Add(this.Length_percentage[i].length.ToString());
                else
                    categoryAxis1.Labels.Add("≥" + max_length);
            }

            categoryAxis1.IsPanEnabled = false;
            categoryAxis1.IsZoomEnabled = false;
            tmp.Axes.Add(categoryAxis1);

            double max = this.Length_percentage[0].ratio;
            for (int i = 1; i < this.Length_percentage.Count; ++i)
            {
                if (this.Length_percentage[i].ratio > max)
                    max = this.Length_percentage[i].ratio;
            }
            var linearAxis1 = new LinearAxis();
            linearAxis1.Minimum = 0;
            linearAxis1.Maximum = max * 100 * 1.2;
            //if (linearAxis1.Maximum > 100)
            //    linearAxis1.Maximum = 100;
            linearAxis1.Position = AxisPosition.Left;
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MinorGridlineStyle = LineStyle.Dot;
            linearAxis1.Title = "Percentage (%)";
            linearAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis1.TitleFontSize = 15;
            linearAxis1.AxisTitleDistance = 10;
            linearAxis1.IsPanEnabled = false;
            linearAxis1.IsZoomEnabled = false;
            tmp.Axes.Add(linearAxis1);

            for (int i = 0; i < this.Length_percentage.Count; ++i)
            {
                var barSeries = new ColumnSeries();
                barSeries.IsStacked = true;
                barSeries.LabelFormatString = "{0:f1}";
                barSeries.StrokeThickness = 1;
                barSeries.FillColor = OxyColors.Blue;
                barSeries.ColumnWidth = 0.3;
                //barSeries.Title = this.Length_percentage[i].length.ToString();
                ColumnItem columnItem = new ColumnItem(this.Length_percentage[i].ratio * 100, -1);
                columnItem.CategoryIndex = i;
                barSeries.Items.Add(columnItem);
                tmp.Series.Add(barSeries);
                PolygonAnnotation block_polygon = new PolygonAnnotation();
                block_polygon.Fill = OxyColors.Transparent;
                block_polygon.StrokeThickness = 0;
                block_polygon.Text = Length_percentage[i].length.ToString();
                block_polygon.TextColor = OxyColors.Transparent;
                block_polygon.Points.Add(new DataPoint(i - barSeries.ColumnWidth, 0));
                block_polygon.Points.Add(new DataPoint(i + barSeries.ColumnWidth, 0));
                block_polygon.Points.Add(new DataPoint(i + barSeries.ColumnWidth, linearAxis1.Maximum));
                block_polygon.Points.Add(new DataPoint(i - barSeries.ColumnWidth, linearAxis1.Maximum));
                block_polygon.Points.Add(new DataPoint(i - barSeries.ColumnWidth, 0));
                tmp.Annotations.Add(block_polygon);
                block_polygon.MouseDown += (s, e) =>
                {
                    if (e.ChangedButton != OxyMouseButton.Left || e.ClickCount != 2)
                        return;
                    int length = int.Parse(block_polygon.Text);
                    PSM_Filter_Dialog psfd = new PSM_Filter_Dialog(mainW);
                    mainW.display_psms = psfd.filter_byPeptideLength(length, mainW.psms);
                    mainW.data.ItemsSource = mainW.display_psms;
                    mainW.display_size.Text = mainW.display_psms.Count().ToString();
                    mainW.summary_tab.SelectedIndex = 1;
                    e.Handled = true;
                };
            }
            return tmp;
        }
        public PlotModel display_missed_cleavage()
        {
            if (this.Missed_cleavage_percentage.Count == 0)
                return null;
            var tmp = new PlotModel();
            tmp.Title = "Missed Cleavage Percentage (%)";
            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.MinorStep = 1;
            categoryAxis1.Position = AxisPosition.Bottom;
            categoryAxis1.Title = "";
            categoryAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            categoryAxis1.TitleFontSize = 15;
            categoryAxis1.AxisTickToLabelDistance = 10;
            for (int i = 0; i < this.Missed_cleavage_percentage.Count; ++i)
            {
                categoryAxis1.Labels.Add(this.Missed_cleavage_percentage[i].missed_number.ToString());
            }
            categoryAxis1.IsPanEnabled = false;
            categoryAxis1.IsZoomEnabled = false;
            tmp.Axes.Add(categoryAxis1);

            double max = this.Missed_cleavage_percentage[0].ratio;
            for (int i = 1; i < this.Missed_cleavage_percentage.Count; ++i)
            {
                if (this.Missed_cleavage_percentage[i].ratio > max)
                    max = this.Missed_cleavage_percentage[i].ratio;
            }
            var linearAxis1 = new LinearAxis();
            linearAxis1.Minimum = 0;
            linearAxis1.Maximum = max * 100 * 1.2;
            //if (linearAxis1.Maximum > 100)
            //    linearAxis1.Maximum = 100;
            linearAxis1.Position = AxisPosition.Left;
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MinorGridlineStyle = LineStyle.Dot;
            linearAxis1.Title = "Percentage (%)";
            linearAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis1.TitleFontSize = 15;
            linearAxis1.AxisTitleDistance = 10;
            linearAxis1.IsPanEnabled = false;
            linearAxis1.IsZoomEnabled = false;
            tmp.Axes.Add(linearAxis1);

            for (int i = 0; i < this.Missed_cleavage_percentage.Count; ++i)
            {
                var barSeries = new ColumnSeries();
                barSeries.IsStacked = true;
                barSeries.LabelFormatString = "{0:f1}";
                barSeries.StrokeThickness = 1;
                barSeries.FillColor = OxyColors.Blue;
                barSeries.ColumnWidth = 0.3;

                ColumnItem columnItem = new ColumnItem(this.Missed_cleavage_percentage[i].ratio * 100, -1);
                columnItem.CategoryIndex = i;
                barSeries.Items.Add(columnItem);
                tmp.Series.Add(barSeries);
                PolygonAnnotation block_polygon = new PolygonAnnotation();
                block_polygon.Fill = OxyColors.Transparent;
                block_polygon.StrokeThickness = 0;
                block_polygon.Text = Missed_cleavage_percentage[i].missed_number.ToString();
                block_polygon.TextColor = OxyColors.Transparent;
                block_polygon.Points.Add(new DataPoint(i - barSeries.ColumnWidth, 0));
                block_polygon.Points.Add(new DataPoint(i + barSeries.ColumnWidth, 0));
                block_polygon.Points.Add(new DataPoint(i + barSeries.ColumnWidth, linearAxis1.Maximum));
                block_polygon.Points.Add(new DataPoint(i - barSeries.ColumnWidth, linearAxis1.Maximum));
                block_polygon.Points.Add(new DataPoint(i - barSeries.ColumnWidth, 0));
                tmp.Annotations.Add(block_polygon);
                block_polygon.MouseDown += (s, e) =>
                {
                    if (e.ChangedButton != OxyMouseButton.Left || e.ClickCount != 2)
                        return;
                    int missed_cleavage_number = int.Parse(block_polygon.Text);
                    PSM_Filter_Dialog psfd = new PSM_Filter_Dialog(mainW);
                    mainW.display_psms = psfd.filter_byPeptideMissedCleavage(missed_cleavage_number, mainW.psms);
                    mainW.data.ItemsSource = mainW.display_psms;
                    mainW.display_size.Text = mainW.display_psms.Count().ToString();
                    mainW.summary_tab.SelectedIndex = 1;
                    e.Handled = true;
                };
            }
            return tmp;
        }
        public PlotModel display_modification2()
        {
            var plotModel1 = new PlotModel();
            plotModel1.Title = "Modification Percentage (%)";
            var pieSeries1 = new PieSeries();
            pieSeries1.InsideLabelPosition = 0.8;
            pieSeries1.Diameter = 1;
            //pieSeries1.ExplodedDistance = 0.2;
            pieSeries1.StrokeThickness = 2;
            for (int i = 0; i < this.Modifications.Count; ++i)
            {
                PieSlice pieSlice = new PieSlice();
                pieSlice.Value = this.Modifications[i].mod_spectra_percentage;
                pieSlice.Label = this.Modifications[i].modification_name;
                pieSlice.IsExploded = true;
                pieSlice.Fill = get_color(i);
                pieSeries1.Slices.Add(pieSlice);
            }
            plotModel1.Series.Add(pieSeries1);
            return plotModel1;
        }
        public PlotModel display_modification()
        {
            if (this.Modifications.Count == 0)
                return null;
            var tmp = new PlotModel();
            tmp.Title = "Modification Percentage (%)";
            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.MinorStep = 1;
            categoryAxis1.Position = AxisPosition.Bottom;
            categoryAxis1.Title = "";
            categoryAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            categoryAxis1.TitleFontSize = 15;
            categoryAxis1.AxisTickToLabelDistance = 10;
            for (int i = 0; i < this.Modifications.Count; ++i)
            {
                categoryAxis1.Labels.Add((i + 1).ToString());
            }
            categoryAxis1.IsPanEnabled = false;
            categoryAxis1.IsZoomEnabled = false;
            tmp.Axes.Add(categoryAxis1);


            var linearAxis1 = new LinearAxis();
            linearAxis1.Minimum = 0;
            linearAxis1.Maximum = this.Modifications[0].mod_spectra_percentage * 100 * 1.2;
            //if (linearAxis1.Maximum > 100)
            //    linearAxis1.Maximum = 100;
            linearAxis1.Position = AxisPosition.Left;
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MinorGridlineStyle = LineStyle.Dot;
            linearAxis1.Title = "Percentage (%)";
            linearAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis1.TitleFontSize = 15;
            linearAxis1.AxisTitleDistance = 10;
            linearAxis1.IsPanEnabled = false;
            linearAxis1.IsZoomEnabled = false;
            tmp.Axes.Add(linearAxis1);

            for (int i = 0; i < this.Modifications.Count; ++i)
            {
                var barSeries = new ColumnSeries();
                barSeries.IsStacked = true;
                barSeries.LabelFormatString = "{0:f2}";
                barSeries.StrokeThickness = 1;
                barSeries.FillColor = get_color(i);
                barSeries.ColumnWidth = 0.3;
                barSeries.Title = this.Modifications[i].modification_name;
                ColumnItem columnItem = new ColumnItem(this.Modifications[i].mod_spectra_percentage * 100, -1);
                columnItem.CategoryIndex = i;
                barSeries.Items.Add(columnItem);
                tmp.Series.Add(barSeries);
                PolygonAnnotation block_polygon = new PolygonAnnotation();
                block_polygon.Fill = OxyColors.Transparent;
                block_polygon.StrokeThickness = 0;
                block_polygon.Text = barSeries.Title;
                block_polygon.TextColor = OxyColors.Transparent;
                block_polygon.Points.Add(new DataPoint(i - barSeries.ColumnWidth / 2, 0));
                block_polygon.Points.Add(new DataPoint(i + barSeries.ColumnWidth / 2, 0));
                block_polygon.Points.Add(new DataPoint(i + barSeries.ColumnWidth / 2, linearAxis1.Maximum));
                block_polygon.Points.Add(new DataPoint(i - barSeries.ColumnWidth / 2, linearAxis1.Maximum));
                block_polygon.Points.Add(new DataPoint(i - barSeries.ColumnWidth / 2, 0));
                tmp.Annotations.Add(block_polygon);
                block_polygon.MouseDown += (s, e) =>
                {
                    if (e.ChangedButton != OxyMouseButton.Left || e.ClickCount != 2)
                        return;
                    string modification = block_polygon.Text;
                    PSM_Filter_Dialog psfd = new PSM_Filter_Dialog(mainW);
                    mainW.display_psms = psfd.filter_byMod(modification, mainW.psms);
                    mainW.data.ItemsSource = mainW.display_psms;
                    mainW.display_size.Text = mainW.display_psms.Count().ToString();
                    mainW.summary_tab.SelectedIndex = 1;
                    e.Handled = true;
                };
            }
            return tmp;
        }
        public PlotModel display_specific2()
        {
            var plotModel1 = new PlotModel();
            plotModel1.Title = "Cleavage Specific Percentage (%)";
            var pieSeries1 = new PieSeries();
            pieSeries1.InsideLabelPosition = 0.8;
            //pieSeries1.ExplodedDistance = 0.2;
            pieSeries1.StrokeThickness = 2;
            for (int i = 0; i < Specific_percentage.Count; ++i)
            {
                PieSlice pieSlice = new PieSlice();
                pieSlice.Value = Specific_percentage[i];
                pieSlice.IsExploded = true;
                pieSlice.Fill = get_color(i);
                switch (i)
                {
                    case 0:
                        pieSlice.Label = "Specific:";
                        break;
                    case 1:
                        pieSlice.Label = "N-term Specific:";
                        break;
                    case 2:
                        pieSlice.Label = "C-term Specific:";
                        break;
                    case 3:
                        pieSlice.Label = "Non-Specific:";
                        break;
                }
                pieSeries1.LabelField = "Hello";
                pieSeries1.Slices.Add(pieSlice);
            }
            plotModel1.Series.Add(pieSeries1);
            return plotModel1;
        }
        public PlotModel display_specific()
        {
            if (Specific_percentage.Count == 0)
                return null;
            var tmp = new PlotModel();
            tmp.Title = "Cleavage Specific Percentage (%)";
            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.MinorStep = 1;
            categoryAxis1.Position = AxisPosition.Bottom;
            categoryAxis1.Title = "";
            categoryAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            categoryAxis1.TitleFontSize = 15;
            categoryAxis1.AxisTickToLabelDistance = 10;
            for (int i = 0; i < Specific_percentage.Count; ++i)
                categoryAxis1.Labels.Add((i + 1).ToString());
            categoryAxis1.IsPanEnabled = false;
            categoryAxis1.IsZoomEnabled = false;
            tmp.Axes.Add(categoryAxis1);

            double max_percentage = double.MinValue;
            for (int i = 0; i < Specific_percentage.Count; ++i)
                if (Specific_percentage[i] > max_percentage)
                    max_percentage = Specific_percentage[i];

            var linearAxis1 = new LinearAxis();
            linearAxis1.Minimum = 0;
            linearAxis1.Maximum = max_percentage * 100 * 1.2;
            //if (linearAxis1.Maximum > 100)
            //    linearAxis1.Maximum = 100;
            linearAxis1.Position = AxisPosition.Left;
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MinorGridlineStyle = LineStyle.Dot;
            linearAxis1.Title = "Percentage (%)";
            linearAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis1.TitleFontSize = 15;
            linearAxis1.AxisTitleDistance = 10;
            linearAxis1.IsPanEnabled = false;
            linearAxis1.IsZoomEnabled = false;
            tmp.Axes.Add(linearAxis1);

            for (int i = 0; i < Specific_percentage.Count; ++i)
            {
                var barSeries = new ColumnSeries();
                barSeries.IsStacked = true;
                barSeries.LabelFormatString = "{0:f2}";
                barSeries.StrokeThickness = 1;
                barSeries.FillColor = get_color(i);
                barSeries.ColumnWidth = 0.3;
                switch (i)
                {
                    case 0:
                        barSeries.Title = "Specific";
                        break;
                    case 1:
                        barSeries.Title = "C-term Specific";
                        break;
                    case 2:
                        barSeries.Title = "N-term Specific";
                        break;
                    case 3:
                        barSeries.Title = "Non-Specific";
                        break;
                }
                ColumnItem columnItem = new ColumnItem(Specific_percentage[i] * 100, -1);
                columnItem.CategoryIndex = i;
                barSeries.Items.Add(columnItem);
                tmp.Series.Add(barSeries);
                PolygonAnnotation block_polygon = new PolygonAnnotation();
                block_polygon.Fill = OxyColors.Transparent;
                block_polygon.StrokeThickness = 0;
                block_polygon.Text = barSeries.Title;
                block_polygon.TextColor = OxyColors.Transparent;
                block_polygon.Points.Add(new DataPoint(i - barSeries.ColumnWidth / 2, 0));
                block_polygon.Points.Add(new DataPoint(i + barSeries.ColumnWidth / 2, 0));
                block_polygon.Points.Add(new DataPoint(i + barSeries.ColumnWidth / 2, linearAxis1.Maximum));
                block_polygon.Points.Add(new DataPoint(i - barSeries.ColumnWidth / 2, linearAxis1.Maximum));
                block_polygon.Points.Add(new DataPoint(i - barSeries.ColumnWidth / 2, 0));
                tmp.Annotations.Add(block_polygon);
                block_polygon.MouseDown += (s, e) =>
                {
                    if (e.ChangedButton != OxyMouseButton.Left || e.ClickCount != 2)
                        return;
                    string specific_flag_str = block_polygon.Text;
                    char specific_flag = 's';
                    switch (specific_flag_str)
                    {
                        case "Specific":
                            specific_flag = 'S';
                            break;
                        case "N-term Specific":
                            specific_flag = 'N';
                            break;
                        case "C-term Specific":
                            specific_flag = 'C';
                            break;
                        case "Non-Specific":
                            specific_flag = 'O';
                            break;
                    }
                    PSM_Filter_Dialog psfd = new PSM_Filter_Dialog(mainW);
                    mainW.display_psms = psfd.filter_bySpecific(specific_flag, mainW.psms);
                    if (mainW.display_psms.Count > 0)
                    {
                        mainW.data.ItemsSource = mainW.display_psms;
                        mainW.display_size.Text = mainW.display_psms.Count().ToString();
                        mainW.summary_tab.SelectedIndex = 1;
                    }
                    e.Handled = true;
                };
            }
            return tmp;
        }
        public PlotModel display_mixed_spectra()
        {
            if (mixed_spectra_count == null || mixed_spectra_count.Count == 0)
                return null;
            int all = 0;
            for (int i = 0; i < mixed_spectra_count.Count; ++i)
            {
                all += mixed_spectra_count[i].num;
            }
            var tmp = new PlotModel();
            tmp.Title = "Mixture Spectra Percentage (%)";
            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.MinorStep = 1;
            categoryAxis1.Position = AxisPosition.Bottom;
            categoryAxis1.Title = "";
            categoryAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            categoryAxis1.TitleFontSize = 15;
            categoryAxis1.AxisTickToLabelDistance = 10;
            for (int i = 0; i < mixed_spectra_count.Count; ++i)
                categoryAxis1.Labels.Add(mixed_spectra_count[i].mixed_spectra_num.ToString());
            categoryAxis1.IsPanEnabled = false;
            categoryAxis1.IsZoomEnabled = false;
            tmp.Axes.Add(categoryAxis1);

            var linearAxis1 = new LinearAxis();
            linearAxis1.Minimum = 0;
            linearAxis1.Maximum = mixed_spectra_count[0].num * 100 * 1.2 / all;
            //if (linearAxis1.Maximum > 100)
            //    linearAxis1.Maximum = 100;
            linearAxis1.Position = AxisPosition.Left;
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MinorGridlineStyle = LineStyle.Dot;
            linearAxis1.Title = "Percentage (%)";
            linearAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis1.TitleFontSize = 15;
            linearAxis1.AxisTitleDistance = 10;
            linearAxis1.IsPanEnabled = false;
            linearAxis1.IsZoomEnabled = false;
            tmp.Axes.Add(linearAxis1);

            for (int i = 0; i < mixed_spectra_count.Count; ++i)
            {
                var barSeries = new ColumnSeries();
                barSeries.IsStacked = true;
                barSeries.LabelFormatString = "{0:f2}";
                barSeries.ColumnWidth = 0.3;
                barSeries.StrokeThickness = 1;
                barSeries.FillColor = get_color(i);
                barSeries.Title = "Number=" + mixed_spectra_count[i].mixed_spectra_num;
                ColumnItem columnItem = new ColumnItem(mixed_spectra_count[i].num * 100.0 / all, -1);
                columnItem.CategoryIndex = i;
                barSeries.Items.Add(columnItem);
                tmp.Series.Add(barSeries);
                PolygonAnnotation block_polygon = new PolygonAnnotation();
                block_polygon.Fill = OxyColors.Transparent;
                block_polygon.StrokeThickness = 0;
                block_polygon.Text = barSeries.Title;
                block_polygon.TextColor = OxyColors.Transparent;
                block_polygon.Points.Add(new DataPoint(i - barSeries.ColumnWidth / 2, 0));
                block_polygon.Points.Add(new DataPoint(i + barSeries.ColumnWidth / 2, 0));
                block_polygon.Points.Add(new DataPoint(i + barSeries.ColumnWidth / 2, 100));
                block_polygon.Points.Add(new DataPoint(i - barSeries.ColumnWidth / 2, 100));
                block_polygon.Points.Add(new DataPoint(i - barSeries.ColumnWidth / 2, 0));
                tmp.Annotations.Add(block_polygon);
                block_polygon.MouseDown += (s, e) =>
                {
                    if (e.ChangedButton != OxyMouseButton.Left || e.ClickCount != 2)
                        return;
                    string mix_number_str = block_polygon.Text.Split('=')[1];
                    int mix_number = int.Parse(mix_number_str);
                    PSM_Filter_Dialog psfd = new PSM_Filter_Dialog(mainW);
                    mainW.display_psms = psfd.filter_byMix_Equal(mix_number, mainW.psms);
                    mainW.data.ItemsSource = mainW.display_psms;
                    mainW.display_size.Text = mainW.display_psms.Count().ToString();
                    mainW.summary_tab.SelectedIndex = 1;
                    e.Handled = true;
                };
                
            }
            return tmp;
        }
        public PlotModel display_raw_rate()
        {
            if (Raw_Rates == null || Raw_Rates.Count == 0)
                return null;
            int max = int.MinValue;
            int raw_count = Raw_Rates.Count - 1; //最后一个是平均解析率，不用画柱状图
            for (int i = 0; i < raw_count; ++i)
            {
                if (max < Raw_Rates[i].All_num)
                    max = Raw_Rates[i].All_num;
            }
            var tmp = new PlotModel();
            //tmp.LegendPlacement = LegendPlacement.Outside;
            tmp.Title = "ID Rate (%)";
            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.MinorStep = 1;
            categoryAxis1.Position = AxisPosition.Bottom;
            categoryAxis1.Title = "Raw";
            categoryAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            categoryAxis1.TitleFontSize = 15;
            categoryAxis1.AxisTickToLabelDistance = 10;
            categoryAxis1.GapWidth = 5;
            for (int i = 0; i < raw_count; ++i)
                categoryAxis1.Labels.Add("" + (i + 1));
            categoryAxis1.IsPanEnabled = false;
            categoryAxis1.IsZoomEnabled = false;
            tmp.Axes.Add(categoryAxis1);

            var linearAxis1 = new LinearAxis();
            //linearAxis1.Angle = -90;
            linearAxis1.Minimum = 0;
            linearAxis1.Maximum = max * 1.2;
            linearAxis1.Position = AxisPosition.Left;
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MinorGridlineStyle = LineStyle.Dot;
            linearAxis1.Title = "Number of Spectra";
            linearAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis1.TitleFontSize = 15;
            linearAxis1.AxisTitleDistance = 3;
            linearAxis1.IsPanEnabled = false;
            linearAxis1.IsZoomEnabled = false;
            tmp.Axes.Add(linearAxis1);
            var barSeries = new ColumnSeries();
            barSeries.ColumnWidth = 0.1;
            barSeries.IsStacked = true;
            barSeries.FontSize = 10;
            barSeries.StrokeThickness = 1;
            barSeries.FillColor = OxyColors.Gray;
            tmp.Series.Add(barSeries);
            var barSeries2 = new ColumnSeries();
            barSeries2.ColumnWidth = 0.1;
            barSeries2.IsStacked = true;
            barSeries2.FontSize = 10;
            barSeries2.StrokeThickness = 1;
            barSeries2.FillColor = OxyColors.Black;
            tmp.Series.Add(barSeries2);
            barSeries.MouseDown += (s, e) =>
            {
                if (e.ChangedButton != OxyMouseButton.Left || e.ClickCount != 2)
                    return;
                int index = (int)Math.Round(e.HitTestResult.Index);
                string raw_name = Raw_Rates[index].Raw_name;
                PSM_Filter_Dialog psfd = new PSM_Filter_Dialog(mainW);
                mainW.display_psms = psfd.filter_byRawName(raw_name, mainW.psms);
                mainW.data.ItemsSource = mainW.display_psms;
                mainW.display_size.Text = mainW.display_psms.Count().ToString();
                mainW.summary_tab.SelectedIndex = 1;
                e.Handled = true;
            };
            barSeries2.MouseDown += (s, e) =>
            {
                if (e.ChangedButton != OxyMouseButton.Left || e.ClickCount != 2)
                    return;
                int index = (int)Math.Round(e.HitTestResult.Index);
                string raw_name = Raw_Rates[index].Raw_name;
                PSM_Filter_Dialog psfd = new PSM_Filter_Dialog(mainW);
                mainW.display_psms = psfd.filter_byRawName(raw_name, mainW.psms);
                mainW.data.ItemsSource = mainW.display_psms;
                mainW.display_size.Text = mainW.display_psms.Count().ToString();
                mainW.summary_tab.SelectedIndex = 1;
                e.Handled = true;
            };
            double font_size = 10.0;
            for (int i = 0; i < raw_count; ++i) 
            {
                ColumnItem columnItem = new ColumnItem(Raw_Rates[i].Identification_num, -1);
                columnItem.CategoryIndex = i;
                barSeries.Items.Add(columnItem);

                columnItem = new ColumnItem(Raw_Rates[i].All_num - Raw_Rates[i].Identification_num, -1);
                columnItem.CategoryIndex = i;
                barSeries2.Items.Add(columnItem);

                TextAnnotation txtAnnotation = new TextAnnotation();
                txtAnnotation.Font = "Times New Roman";
                txtAnnotation.Text = Raw_Rates[i].Rate.ToString("P2");
                txtAnnotation.Stroke = OxyColors.Transparent;
                txtAnnotation.FontSize = font_size;
                txtAnnotation.FontWeight = OxyPlot.FontWeights.Bold;
                txtAnnotation.TextColor = OxyColors.Black;
                txtAnnotation.Position = new DataPoint(i, Raw_Rates[i].All_num);
                tmp.Annotations.Add(txtAnnotation);

                txtAnnotation = new TextAnnotation();
                txtAnnotation.Font = "Times New Roman";
                txtAnnotation.Text = Raw_Rates[i].Identification_num + "/" + Raw_Rates[i].All_num.ToString();
                txtAnnotation.Stroke = OxyColors.Transparent;
                txtAnnotation.FontSize = font_size;
                txtAnnotation.FontWeight = OxyPlot.FontWeights.Bold;
                txtAnnotation.TextColor = OxyColors.Black;
                txtAnnotation.Position = new DataPoint(i, Raw_Rates[i].All_num + 0.07 * max);
                tmp.Annotations.Add(txtAnnotation);
            }
            //让横轴可以自由缩放
            if (raw_count >= 10)
            {
                tmp.Axes[0].IsZoomEnabled = true;
                tmp.Axes[0].IsPanEnabled = true;
                tmp.Axes[0].AbsoluteMaximum = raw_count;
                tmp.Axes[0].AbsoluteMinimum = -1.0;
                tmp.Axes[0].Maximum = tmp.Axes[0].AbsoluteMaximum;
                tmp.Axes[0].Minimum = tmp.Axes[0].AbsoluteMinimum;
            }
            return tmp;
        }
        public PlotModel display_modification_nan_rate(List<int> fz, List<int> fm, List<string> modification_name, int top)
        {
            int max = int.MinValue;
            for (int i = 0; i < fm.Count; ++i)
                if (max < fm[i])
                    max = fm[i];
            var tmp = new PlotModel();
            //tmp.LegendPlacement = LegendPlacement.Outside;
            tmp.Title = "Modification NAN Rate (%)";
            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.MinorStep = 1;
            categoryAxis1.Position = AxisPosition.Bottom;
            categoryAxis1.Title = "Modifcation";
            categoryAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            categoryAxis1.TitleFontSize = 15;
            categoryAxis1.AxisTickToLabelDistance = 15;
            categoryAxis1.GapWidth = 5;
            categoryAxis1.FontSize = 12.0;
            for (int i = 0; i < modification_name.Count && i < top; ++i)
                categoryAxis1.Labels.Add(modification_name[i]);

            tmp.Axes.Add(categoryAxis1);

            var linearAxis1 = new LinearAxis();
            //linearAxis1.Angle = -90;
            linearAxis1.Position = AxisPosition.Left;
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MinorGridlineStyle = LineStyle.Dot;
            linearAxis1.Minimum = 0.0;
            linearAxis1.Maximum = max * 1.2;
            linearAxis1.Title = "Number";
            linearAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis1.TitleFontSize = 15;
            linearAxis1.AxisTitleDistance = 10;
            linearAxis1.IsPanEnabled = false;
            linearAxis1.IsZoomEnabled = false;
            tmp.Axes.Add(linearAxis1);
            var barSeries = new ColumnSeries();
            barSeries.ColumnWidth = 0.1;
            barSeries.IsStacked = true;
            barSeries.FontSize = 10;
            barSeries.StrokeThickness = 1;
            barSeries.FillColor = OxyColors.Gray;
            tmp.Series.Add(barSeries);
            var barSeries2 = new ColumnSeries();
            barSeries2.ColumnWidth = 0.1;
            barSeries2.IsStacked = true;
            barSeries2.FontSize = 10;
            barSeries2.StrokeThickness = 1;
            barSeries2.FillColor = OxyColors.Black;
            tmp.Series.Add(barSeries2);
            
            double font_size = 12.0;
            for (int i = 0; i < modification_name.Count && i < top; ++i)
            {
                ColumnItem columnItem = new ColumnItem(fz[i], -1);
                columnItem.CategoryIndex = i;
                barSeries.Items.Add(columnItem);

                columnItem = new ColumnItem(fm[i] - fz[i], -1);
                columnItem.CategoryIndex = i;
                barSeries2.Items.Add(columnItem);

                TextAnnotation txtAnnotation = new TextAnnotation();
                txtAnnotation.Font = "Times New Roman";
                double ratio = fz[i] * 1.0 / fm[i];
                txtAnnotation.Text = ratio.ToString("P2");
                txtAnnotation.Stroke = OxyColors.Transparent;
                txtAnnotation.FontSize = font_size;
                txtAnnotation.FontWeight = OxyPlot.FontWeights.Bold;
                txtAnnotation.TextColor = OxyColors.Black;
                txtAnnotation.Position = new DataPoint(i, fm[i]);
                tmp.Annotations.Add(txtAnnotation);

                txtAnnotation = new TextAnnotation();
                txtAnnotation.Font = "Times New Roman";
                txtAnnotation.Text = fz[i] + "/" + fm[i];
                txtAnnotation.Stroke = OxyColors.Transparent;
                txtAnnotation.FontSize = font_size;
                txtAnnotation.FontWeight = OxyPlot.FontWeights.Bold;
                txtAnnotation.TextColor = OxyColors.Black;
                txtAnnotation.Position = new DataPoint(i, fm[i] + 0.03 * max);
                tmp.Annotations.Add(txtAnnotation);
            }
            //让横轴可以自由缩放
            tmp.Axes[0].IsZoomEnabled = true;
            tmp.Axes[0].IsPanEnabled = true;
            return tmp;
        }
        public PlotModel display_score()
        {
            if (this.All_score_bins.Count == 0)
                return null;
            var tmp = new PlotModel();
            tmp.Title = "Score Distribution";
            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.MinorStep = 1;
            categoryAxis1.Position = AxisPosition.Bottom;
            categoryAxis1.Title = "-log(Score)";
            categoryAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            categoryAxis1.TitleFontSize = 15;
            categoryAxis1.AxisTitleDistance = 10;
            categoryAxis1.MajorStep = 5.0; // (All_score_bins[All_score_bins.Count - 1] - All_score_bins[0]) / 10;
            //categoryAxis1.IntervalLength = 1;
            categoryAxis1.IsPanEnabled = false;
            categoryAxis1.IsZoomEnabled = false;
            double score_offset = (All_score_bins[1] - All_score_bins[0]) / 2;
            for (int i = 0; i < All_score_bins.Count; ++i)
            {
                //if (i % 5 == 0)
                categoryAxis1.Labels.Add((All_score_bins[i] + score_offset).ToString("F1"));
                //else
                //   categoryAxis1.Labels.Add("");
            }
            tmp.Axes.Add(categoryAxis1);


            var linearAxis1 = new LinearAxis();
            linearAxis1.Minimum = 0;
            linearAxis1.Maximum = Max_Number * 1.02;
            linearAxis1.Position = AxisPosition.Left;
            linearAxis1.IntervalLength = 20;
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MinorGridlineStyle = LineStyle.Dot;
            linearAxis1.Title = "Number of PSMs";
            linearAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis1.TitleFontSize = 15;
            linearAxis1.AxisTitleDistance = 10;
            linearAxis1.IsPanEnabled = false;
            linearAxis1.IsZoomEnabled = false;
            tmp.Axes.Add(linearAxis1);
            var barSeries1 = new ColumnSeries();
            //barSeries1.TrackerFormatString = "-log(score): {2:0.00}\nnumber of PSMs: {4:N0}";
            //barSeries1.IsStacked = true;
            barSeries1.StrokeThickness = 1;
            barSeries1.Title = "Target";
            barSeries1.FillColor = OxyColors.Blue;
            for (int i = 0; i < Target_score_bin_numbers.Count; ++i)
            {
                barSeries1.Items.Add(new ColumnItem(Target_score_bin_numbers[i], -1));
            }

            var barSeries2 = new ColumnSeries();
            //barSeries2.IsStacked = true;
            barSeries2.StrokeThickness = 1;
            barSeries2.Title = "Decoy";
            barSeries2.FillColor = OxyColors.Red;
            for (int i = 0; i < Decoy_score_bin_numbers.Count; ++i)
            {
                barSeries2.Items.Add(new ColumnItem(Decoy_score_bin_numbers[i], -1));
            }
            tmp.Series.Add(barSeries1);
            tmp.Series.Add(barSeries2);
            var lineSeries1 = new LineSeries();
            lineSeries1.TrackerFormatString = "-log(score): {2:0.00}\nnumber of PSMs: {4:N0}";
            lineSeries1.Title = "Target";
            for (int i = 0; i < Target_score_bin_numbers.Count; ++i)
            {
                lineSeries1.Points.Add(new DataPoint(i, Target_score_bin_numbers[i]));
            }
            tmp.Series.Add(lineSeries1);
            var lineSeries2 = new LineSeries();
            lineSeries2.TrackerFormatString = "-log(score): {2:0.00}\nnumber of PSMs: {4:N0}";
            lineSeries2.Title = "Decoy";
            for (int i = 0; i < Decoy_score_bin_numbers.Count; ++i)
            {
                lineSeries2.Points.Add(new DataPoint(i, Decoy_score_bin_numbers[i]));
            }
            tmp.Series.Add(lineSeries2);
            return tmp;
        }

        public PlotModel display_mass_error(string Da_PPM, int x)
        {
            var tmp = new PlotModel();
            tmp.Title = "Raw " + (x + 1) + ": Mass Deviation-Score";
            var linearAxis1 = new LinearAxis();
            linearAxis1.Key = "Score";
            linearAxis1.Minimum = 0;
            linearAxis1.Maximum = Max_score;
            linearAxis1.MaximumPadding = 0;
            linearAxis1.IntervalLength = 20;
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MinorGridlineStyle = LineStyle.Dot;
            linearAxis1.Title = "-log(score)";
            linearAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis1.TitleFontSize = 15;
            linearAxis1.AxisTitleDistance = 10;
            linearAxis1.IsPanEnabled = false;
            linearAxis1.IsZoomEnabled = false;
            tmp.Axes.Add(linearAxis1);

            var linearAxis2 = new LinearAxis();
            linearAxis2.Position = AxisPosition.Bottom;
            linearAxis2.Minimum = -Max_mass_error;
            linearAxis2.Maximum = Max_mass_error;
            linearAxis2.AbsoluteMinimum = linearAxis2.Minimum;
            linearAxis2.AbsoluteMaximum = linearAxis2.Maximum;
            if (Da_PPM == "Da")
                linearAxis2.StringFormat = "f3";
            else
                linearAxis2.StringFormat = "f2";
            //linearAxis2.MajorStep = (linearAxis2.Maximum - linearAxis2.Minimum) / 10;
            //linearAxis2.IntervalLength = 80;
            linearAxis2.MajorGridlineStyle = LineStyle.Solid;
            linearAxis2.MinorGridlineStyle = LineStyle.Dot;
            linearAxis2.Title = "Mass Deviation (" + Da_PPM + ")";
            linearAxis2.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis2.TitleFontSize = 15;
            linearAxis2.AxisTitleDistance = 3.0;
            tmp.Axes.Add(linearAxis2);
            ScatterSeries ss = new ScatterSeries();
            ss.MarkerFill = OxyColors.Blue;
            ss.MarkerSize = 1;
            for (int i = 0; i < Target_points.Count; ++i)
            {
                ss.Points.Add(Target_points[i]);
            }
            tmp.Series.Add(ss);
            ss = new ScatterSeries();
            ss.MarkerFill = OxyColors.Red;
            ss.MarkerSize = 1;
            for (int i = 0; i < Decoy_points.Count; ++i)
            {
                ss.Points.Add(Decoy_points[i]);
            }
            tmp.Series.Add(ss);
            tmp.Axes[1].AxisChanged += (s, er) =>
            {
                mass_error_draw_speedUp(tmp, Da_PPM);
            };
            tmp.Axes[1].TransformChanged += (s, er) =>
            {
                mass_error_draw_speedUp(tmp, Da_PPM);
                tmp.RefreshPlot(true);
            };
            return tmp;
        }
        public PlotModel display_FDR(double fdr_value)
        {
            var tmp = new PlotModel();
            tmp.Title = "FDR Curve";
            var linearAxis1 = new LinearAxis();
            linearAxis1.Minimum = 0.0;
            linearAxis1.Title = "PSM Count";
            linearAxis1.StringFormat = "N0";
            linearAxis1.Position = AxisPosition.Bottom;
            linearAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis1.TitleFontSize = 15;
            linearAxis1.AxisTitleDistance = 5;
            linearAxis1.IsPanEnabled = false;
            linearAxis1.IsZoomEnabled = false;
            tmp.Axes.Add(linearAxis1);

            var linearAxis2 = new LinearAxis();
            linearAxis2.Minimum = -0.001;
            linearAxis2.Maximum = 0.05;
            linearAxis2.Title = "FDR";
            linearAxis2.StringFormat = "P1";
            linearAxis2.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis2.MajorStep = 0.01;
            linearAxis2.MajorGridlineStyle = LineStyle.Solid;
            linearAxis2.MinorGridlineStyle = LineStyle.Dash;
            linearAxis2.TitleFontSize = 15;
            linearAxis2.AxisTitleDistance = 10.0;
            linearAxis2.IsPanEnabled = false;
            linearAxis2.IsZoomEnabled = false;
            tmp.Axes.Add(linearAxis2);
            //linearAxis1.Reset();
            double start_y = 0.005 * 0.01;
            double delta_y = 0.01 * 0.01;
            int all_count = 0;
            LineSeries lineSeries = new LineSeries();
            lineSeries.TrackerFormatString = "PSM Count: {2:N0}\nFDR: {4:P2}"; //设置弹出小气泡中的内容的格式
            lineSeries.StrokeThickness = 3.0;
            lineSeries.Color = OxyColors.Blue;
            Collection<IDataPoint> all_points = new Collection<IDataPoint>();
            all_points.Add(new DataPoint(0.0, 0.0));
            for (int i = 0; i < FDR_bin_count.Length; ++i)
            {
                all_count += FDR_bin_count[i];
                all_points.Add(new DataPoint(all_count, start_y));
                start_y += delta_y;
            }
            lineSeries.Points = all_points;
            tmp.Series.Add(lineSeries);
            var lineAnnotation3 = new LineAnnotation();
            lineAnnotation3.Type = LineAnnotationType.Horizontal;
            lineAnnotation3.Y = fdr_value;
            lineAnnotation3.Color = OxyColors.Green;
            lineAnnotation3.StrokeThickness = 2;
            lineAnnotation3.MaximumY = all_count;
            lineAnnotation3.Text = lineAnnotation3.Y.ToString("P2") + " : " + all_points[(int)((lineAnnotation3.Y - 0.005 * 0.01) / delta_y) + 1].X.ToString("N0");
            lineAnnotation3.FontWeight = OxyPlot.FontWeights.Bold;
            lineAnnotation3.FontSize = 16.0;
            lineAnnotation3.TextOrientation = AnnotationTextOrientation.Horizontal;
            if (fdr_value <= 0.045)
                lineAnnotation3.TextVerticalAlignment = OxyPlot.VerticalAlignment.Bottom;
            else
                lineAnnotation3.TextVerticalAlignment = OxyPlot.VerticalAlignment.Top;
            lineAnnotation3.MouseDown += (s, e) =>
            {
                if (e.ChangedButton != OxyMouseButton.Left)
                {
                    return;
                }
                lineAnnotation3.StrokeThickness *= 3;
                lineAnnotation3.FontSize += 4.0;
                tmp.RefreshPlot(false);
                e.Handled = true;
            };

            // Handle mouse movements (note: this is only called when the mousedown event was handled)
            lineAnnotation3.MouseMove += (s, e) =>
            {
                double cur_Y = lineAnnotation3.InverseTransform(e.Position).Y;
                //将FDR控制在[0.0%-5%]范围内
                if (cur_Y < 0)
                    cur_Y = 0;
                else if (cur_Y > 0.05)
                    cur_Y = 0.05;
                lineAnnotation3.Y = cur_Y;
                if (cur_Y <= 0.045)
                    lineAnnotation3.TextVerticalAlignment = OxyPlot.VerticalAlignment.Bottom;
                else
                    lineAnnotation3.TextVerticalAlignment = OxyPlot.VerticalAlignment.Top;
                int index = (int)((lineAnnotation3.Y - 0.005 * 0.01) / delta_y) + 1;
                if (index < 0)
                    index = 0;
                lineAnnotation3.Text = lineAnnotation3.Y.ToString("P2") + ":" + all_points[index].X.ToString("N0");
                tmp.RefreshPlot(false);
                e.Handled = true;
            };
            /*
            lineAnnotation3.MouseUp += (s, e) =>
            {
                lineAnnotation3.StrokeThickness /= 3;
                lineAnnotation3.FontSize -= 4.0;
                tmp.RefreshPlot(false);
                e.Handled = true;
            };
             */
            tmp.Annotations.Add(lineAnnotation3);
            return tmp;
        }
        public PlotModel display_ms1(double min, double max, ref double ms1_pepmass, ref int frag_index)
        {
            if (ms1 == null || ms1.Peaks.Count <= 0)
                return null;
            var tmp = new PlotModel();

            var linearAxis1 = new LinearAxis();
            linearAxis1.Key = "peaks";
            linearAxis1.StartPosition = 0.0;
            linearAxis1.EndPosition = 0.80;
            linearAxis1.Minimum = 0;
            linearAxis1.Maximum = 115;
            linearAxis1.MaximumPadding = 0;
            linearAxis1.MajorStep = 20; //MajorStep=20;
            linearAxis1.ShowMinorTicks = false;
            linearAxis1.Title = "Relative Intensity (%)";
            linearAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis1.TitleFontSize = 15;
            linearAxis1.AxisTitleDistance = 0;
            linearAxis1.IsPanEnabled = false;
            linearAxis1.IsZoomEnabled = false;
            tmp.Axes.Add(linearAxis1);

            var linearAxis2 = new LinearAxis();
            linearAxis2.Position = AxisPosition.Bottom;
            linearAxis2.Minimum = min;
            linearAxis2.Maximum = max;
            linearAxis2.MajorStep = (linearAxis2.Maximum - linearAxis2.Minimum) / 20;
            linearAxis2.AbsoluteMinimum = 0;
            linearAxis2.AbsoluteMaximum = ms1.Peaks[ms1.Peaks.Count - 1].Mass + 50;
            linearAxis2.Title = "m/z";
            linearAxis2.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis2.TitleFontSize = 15;
            linearAxis2.AxisTitleDistance = 3.0;
            tmp.Axes.Add(linearAxis2);

            var linearAxis3 = new LinearAxis();
            linearAxis3.Key = "title";
            linearAxis3.StartPosition = 0.85;
            linearAxis3.EndPosition = 0.92;
            linearAxis3.Minimum = 0;
            linearAxis3.Maximum = 10;
            linearAxis3.TicklineColor = OxyColors.Transparent;
            linearAxis3.TextColor = OxyColors.Transparent;
            linearAxis3.IsPanEnabled = false;
            linearAxis3.IsZoomEnabled = false;
            tmp.Axes.Add(linearAxis3);

            var linearAxis4 = new LinearAxis();
            linearAxis4.Key = "title2";
            linearAxis4.StartPosition = 0.93;
            linearAxis4.EndPosition = 1.0;
            linearAxis4.Minimum = 0;
            linearAxis4.Maximum = 10;
            linearAxis4.TicklineColor = OxyColors.Transparent;
            linearAxis4.TextColor = OxyColors.Transparent;
            linearAxis4.IsPanEnabled = false;
            linearAxis4.IsZoomEnabled = false;
            tmp.Axes.Add(linearAxis4);

            var linearAxis5 = new LinearAxis();
            linearAxis5.Key = "Eagle eye";
            linearAxis5.StartPosition = 0.80;
            linearAxis5.EndPosition = 1.0;
            linearAxis5.Minimum = -0.02;
            linearAxis5.Maximum = 1.05;
            linearAxis5.TicklineColor = OxyColors.Transparent;
            linearAxis5.TextColor = OxyColors.Transparent;
            linearAxis5.IsPanEnabled = false;
            linearAxis5.IsZoomEnabled = false;
            tmp.Axes.Add(linearAxis5);

            double min_mz_error = 1.0e+10;
            int min_index = -1;
            for (int i = 0; i < ms1.Peaks.Count; ++i)
            {
                IList<IDataPoint> Points = new List<IDataPoint>();
                Points.Add(new DataPoint(ms1.Peaks[i].Mass, 0));
                Points.Add(new DataPoint(ms1.Peaks[i].Mass, ms1.Peaks[i].Intensity));
                LineSeries Ls = new LineSeries();
                Ls.TrackerFormatString = "m/z: {2:0.00}\nIntensity: {4:0.00}%";
                Ls.Color = OxyColors.Black;
                Ls.LineStyle = LineStyle.Solid;
                Ls.StrokeThickness = 1;
                Ls.Points = Points;
                Ls.YAxisKey = linearAxis1.Key;
                tmp.Series.Add(Ls);

                double tmp_mz_e = Math.Abs(ms1.Peaks[i].Mass - ms1.Pepmass_ms2);
                if (tmp_mz_e < min_mz_error)
                {
                    min_mz_error = tmp_mz_e;
                    min_index = i;
                }
            }
            ms1.Pepmass = ms1.Peaks[min_index].Mass;
            ms1_pepmass = ms1.Pepmass;
            ms1.Fragment_index = min_index;
            frag_index = ms1.Fragment_index;
            IList<IDataPoint> points = new List<IDataPoint>();
            points.Add(new DataPoint(ms1.Peaks[min_index].Mass, 0));
            points.Add(new DataPoint(ms1.Peaks[min_index].Mass, ms1.Peaks[min_index].Intensity));
            LineSeries ls = new LineSeries();
            ls.TrackerFormatString = "m/z: {2:0.00}\nIntensity: {4:0.00}%";
            ls.Color = OxyColors.Red;
            ls.LineStyle = LineStyle.Solid;
            ls.StrokeThickness = 3;
            ls.Points = points;
            ls.YAxisKey = linearAxis1.Key;
            tmp.Series.Add(ls);

            /*
            points = new List<IDataPoint>();
            points.Add(new DataPoint(ms1.Pepmass_ms2, 0));
            points.Add(new DataPoint(ms1.Pepmass_ms2, ms1.Peaks[min_index].Intensity));
            ls = new LineSeries();
            ls.Color = OxyColors.Green;
            ls.LineStyle = LineStyle.Solid;
            ls.StrokeThickness = 3;
            ls.Points = points;
            ls.YAxisKey = linearAxis1.Key;
            tmp.Series.Add(ls);
             * */
            tmp.Padding = new OxyThickness(0, 0, 0, 0);

            return tmp;
        }
        public Spectra_MS1_Isotope get_actual_isotope(bool is_refresh, bool is_add_charge, double intensity_ratio,
            PlotModel model, Spectra_MS1 selected_ms1, Display_Help dis_help)
        {
            if (model.Series.Count == 0)
                return null;
            Spectra_MS1_Isotope max_actual_isotope = null;
            Spectra_MS1_Isotope actual_isotope = null;

            ObservableCollection<PEAK> peaks = selected_ms1.Peaks;
            int[] mass_inten = new int[Config_Help.MaxMass];
            for (int k = 0; k < peaks.Count; ++k)
            {
                int massi = (int)peaks[k].Mass;
                mass_inten[massi] = k + 1;
            }
            int currindex = 0;
            for (int k = 0; k < Config_Help.MaxMass; ++k)
            {
                if (mass_inten[k] == 0)
                    mass_inten[k] = currindex;
                else
                    currindex = mass_inten[k];
            }

            int maxCount = 0;

            for (int i = 0; i < model.Series.Count; ++i)
            {
                LineSeries cur_ls = (LineSeries)model.Series[i];
                if (is_refresh)
                    cur_ls.Points[1].Y *= intensity_ratio;
                if (cur_ls.Points[1].Y > 0)
                {
                    if (is_add_charge)
                    {
                        int index = PEAK.IsInWithPPM2(cur_ls.Points[1].X, mass_inten, 20e-6, peaks);
                        if (index != -1 && peaks[index].Is_mono)
                        {
                            var lineAnnotation = new LineAnnotation();
                            lineAnnotation.Type = LineAnnotationType.Vertical;
                            lineAnnotation.StrokeThickness = 1;
                            lineAnnotation.TextVerticalAlignment = OxyPlot.VerticalAlignment.Bottom;
                            lineAnnotation.TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Center;
                            lineAnnotation.TextOrientation = AnnotationTextOrientation.Horizontal;
                            lineAnnotation.X = cur_ls.Points[1].X;
                            lineAnnotation.MaximumY = cur_ls.Points[1].Y;
                            lineAnnotation.Color = OxyColors.Transparent;
                            lineAnnotation.LineStyle = LineStyle.Solid;
                            if (peaks[index].Charge != 0)
                                lineAnnotation.Text = "z=" + peaks[index].Charge;
                            else
                                lineAnnotation.Text = "z=?";
                            lineAnnotation.TextPosition = 1.0;
                            lineAnnotation.TextMargin = 1;
                            lineAnnotation.TextColor = OxyColors.Black;
                            lineAnnotation.YAxisKey = model.Axes[0].Key;
                            model.Annotations.Add(lineAnnotation); //增加电荷信息
                            var txtAnnotation = new TextAnnotation();
                            txtAnnotation.Position = new DataPoint(cur_ls.Points[1].X, cur_ls.Points[1].Y + 4);
                            txtAnnotation.Text = cur_ls.Points[1].X.ToString("F2");
                            txtAnnotation.StrokeThickness = 0;
                            txtAnnotation.TextColor = OxyColors.Black;
                            model.Annotations.Add(txtAnnotation); //增加Th的质量信息
                        }
                    }

                    if (cur_ls.Points[1].X == selected_ms1.Pepmass)
                    {
                        int j = i;
                        actual_isotope = new Spectra_MS1_Isotope(dis_help.Psm_help.Spec.Charge);
                        actual_isotope.add_OnePeak(cur_ls.Points[1].X, cur_ls.Points[1].Y);
                        while (true)
                        {
                            cur_ls = (LineSeries)model.Series[j];
                            double next_mz = cur_ls.Points[1].X + Config_Help.mass_ISO / actual_isotope.charge;
                            bool isIn = false;
                            for (int k = j + 1; k < model.Series.Count; ++k)
                            {
                                LineSeries cur_ls2 = (LineSeries)model.Series[k];
                                if (Math.Abs(cur_ls2.Points[1].X - next_mz) / cur_ls2.Points[1].X <= 20e-6)
                                {
                                    actual_isotope.add_OnePeak(cur_ls2.Points[1].X, cur_ls2.Points[1].Y * intensity_ratio);
                                    j = k;
                                    isIn = true;
                                    break;
                                }
                            }
                            if (!isIn)
                                break;
                        }
                        if (actual_isotope.count > maxCount)
                        {
                            maxCount = actual_isotope.count;
                            max_actual_isotope = actual_isotope;
                        }
                    }
                }
            }
            return max_actual_isotope;
        }
        private int find_one_peak(double mz, double intensity, double max_inten_E, PlotModel model)
        {
            //for (int i = 0; i < model.Series.Count; ++i)
            //{
            //    LineSeries ls = model.Series[i] as LineSeries;
            //    if (ls != null && Math.Abs(ls.Points[1].X - mz) <= 0.01 &&
            //        Math.Abs(ls.Points[1].Y * max_inten_E / 100 - intensity) <= 100)
            //        return i;
            //}
            //return -1;
            double min_mz_error = double.MaxValue;
            int min_index = -1;
            for (int i = 0; i < model.Series.Count; ++i)
            {
                LineSeries ls = model.Series[i] as LineSeries;
                if (ls == null)
                    continue;
                double mz_error_tmp = Math.Abs(ls.Points[1].X - mz);
                if (mz_error_tmp < min_mz_error)
                {
                    min_mz_error = mz_error_tmp;
                    min_index = i;
                }
            }
            if (min_mz_error <= 0.02)
                return min_index;
            return -1;
        }
        private List<Spectra_MS1_Isotope> parse_actual_isotope(int scan, Evidence evi, double max_inten_E,
            PlotModel model, ref List<Theory_IOSTOPE> theory_isotopes)
        {
            List<Spectra_MS1_Isotope> actual_isotopes = new List<Spectra_MS1_Isotope>();
            theory_isotopes = new List<Theory_IOSTOPE>();
            int index = 0;
            for (int i = 0; i < evi.ScanTime.Count; ++i)
            {
                if (evi.ScanTime[i] == scan)
                {
                    index = i;
                    break;
                }
            }
            for (int i = 0; i < evi.ActualIsotopicPeakList.Count; ++i)
            {
                Spectra_MS1_Isotope isotope = new Spectra_MS1_Isotope(0);
                Theory_IOSTOPE isotope_t = new Theory_IOSTOPE();
                for (int j = 0; j < evi.ActualIsotopicPeakList[i].Count; ++j)
                {
                    double intensity = evi.ActualIsotopicPeakList[i][j][index];
                    double mz = evi.TheoricalIsotopicMZList[i][j];
                    double theory_intensity = evi.TheoricalIsotopicPeakList[i][j];
                    int index_model = find_one_peak(mz, intensity, max_inten_E, model);
                    if (index_model != -1)
                    {
                        LineSeries ls = model.Series[index_model] as LineSeries;
                        isotope.mz.Add(ls.Points[1].X);
                        isotope.intensity.Add(ls.Points[1].Y);
                        isotope_t.mz.Add(mz);
                        isotope_t.intensity.Add(theory_intensity);
                        ls.Color = OxyColors.Red;
                    }
                }
                actual_isotopes.Add(isotope);
                theory_isotopes.Add(isotope_t);
            }
            return actual_isotopes;
        }
        public void display_actual_isotope(double screen_x, Evidence evi, Spectra_MS1 selected_ms1, double max_inten_E, PlotModel model)
        {
            List<Theory_IOSTOPE> theory_isotopes = null;
            List<Spectra_MS1_Isotope> actual_isotopes = parse_actual_isotope(selected_ms1.Scan, evi, max_inten_E, model, ref theory_isotopes);
            for (int i = 0; i < actual_isotopes.Count; ++i)
            {
                for (int j = 0; j < actual_isotopes[i].mz.Count - 1; ++j)
                {
                    var arrowAnnotation = new ArrowAnnotation();
                    arrowAnnotation.Color = OxyColors.Green;
                    double min_y = (actual_isotopes[i].intensity[j] < actual_isotopes[i].intensity[j + 1] ?
                        actual_isotopes[i].intensity[j] : actual_isotopes[i].intensity[j + 1]);
                    min_y = min_y - 10;
                    if (min_y < 0)
                        min_y = 5;
                    arrowAnnotation.StartPoint = new DataPoint(actual_isotopes[i].mz[j], min_y);
                    arrowAnnotation.EndPoint = new DataPoint(actual_isotopes[i].mz[j + 1], min_y);
                    arrowAnnotation.Text = "";
                    model.Annotations.Add(arrowAnnotation);

                    var textAnnotation = new TextAnnotation();
                    textAnnotation.Position = new DataPoint((actual_isotopes[i].mz[j] + actual_isotopes[i].mz[j + 1]) / 2, min_y);
                    textAnnotation.Text = (actual_isotopes[i].mz[j + 1] - actual_isotopes[i].mz[j]).ToString("f4") + "Th";
                    textAnnotation.TextColor = arrowAnnotation.Color;
                    textAnnotation.Stroke = OxyColors.Transparent;
                    model.Annotations.Add(textAnnotation);
                    //var textAnnotation2 = new TextAnnotation();
                    //textAnnotation2.Position = new DataPoint(textAnnotation.Position.X, min_y + 5);
                    //textAnnotation2.Text = (((actual_isotopes[i].mz[j + 1] - actual_isotopes[i].mz[j]) -
                    //    (theory_isotopes[i].mz[j + 1] - theory_isotopes[i].mz[j])) * 1000).ToString("f3") + "mDa";
                    //textAnnotation2.TextColor = OxyColors.Red;
                    ////textAnnotation2.VerticalAlignment = OxyPlot.VerticalAlignment.Top;
                    //textAnnotation2.Stroke = OxyColors.Transparent;
                    //model.Annotations.Add(textAnnotation2);
                }
            }
            //计算Cos相似度
            List<double> cos_ss = new List<double>();
            List<double> cos_ss_mz = new List<double>();
            for (int i = 0; i < actual_isotopes.Count; ++i)
            {
                double fz = 0.0, fm1 = 0.0, fm2 = 0.0;
                double fz_mz = 0.0, fm1_mz = 0.0, fm2_mz = 0.0;
                for (int j = 0; j < actual_isotopes[i].intensity.Count; ++j)
                {
                    fz += actual_isotopes[i].intensity[j] * theory_isotopes[i].intensity[j];
                    fm1 += actual_isotopes[i].intensity[j] * actual_isotopes[i].intensity[j];
                    fm2 += theory_isotopes[i].intensity[j] * theory_isotopes[i].intensity[j];
                    fz_mz += actual_isotopes[i].mz[j] * theory_isotopes[i].mz[j];
                    fm1_mz += actual_isotopes[i].mz[j] * actual_isotopes[i].mz[j];
                    fm2_mz += theory_isotopes[i].mz[j] * theory_isotopes[i].mz[j];
                }
                if (fm1 != 0 && fm2 != 0 && fm1_mz != 0 && fm2_mz != 0)
                {
                    cos_ss.Add(fz / (Math.Sqrt(fm1) * Math.Sqrt(fm2)));
                    cos_ss_mz.Add(fz_mz / (Math.Sqrt(fm1_mz) * Math.Sqrt(fm2_mz)));
                }
            }
            string cos_text = "";
            for (int i = 0; i < cos_ss.Count; ++i)
            {
                TextAnnotation textAnnotation3 = new TextAnnotation();
                TextAnnotation textAnnotation3_2 = new TextAnnotation();
                textAnnotation3.HorizontalAlignment = OxyPlot.HorizontalAlignment.Left;
                textAnnotation3_2.HorizontalAlignment = textAnnotation3.HorizontalAlignment;
                textAnnotation3.FontSize = 12;
                textAnnotation3_2.FontSize = textAnnotation3.FontSize;
                textAnnotation3.Stroke = OxyColors.Transparent;
                textAnnotation3_2.Stroke = textAnnotation3.Stroke;
                textAnnotation3.YAxisKey = model.Axes[3].Key;
                textAnnotation3_2.YAxisKey = textAnnotation3.YAxisKey;
                textAnnotation3.Font = Display_Detail_Help.font_type;
                textAnnotation3_2.Font = textAnnotation3.Font;
                textAnnotation3.TextColor = Display_Detail_Help.title_color;
                textAnnotation3_2.TextColor = Display_Detail_Help.value_color;

                textAnnotation3.Position = new DataPoint(model.Axes[1].InverseTransform(screen_x), 0);
                textAnnotation3.Text = "Cos_SIM" + (i + 1) + ": ";
                FormattedText ft = new FormattedText(textAnnotation3.Text, System.Globalization.CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(textAnnotation3.Font), textAnnotation3.FontSize, Brushes.Black);
                screen_x += ft.WidthIncludingTrailingWhitespace;
                textAnnotation3_2.Text = cos_ss[i].ToString("f4") + "  "; //+ "/" + cos_ss_mz[i].ToString("f4") 
                textAnnotation3_2.Position = new DataPoint(model.Axes[1].InverseTransform(screen_x), 0);
                ft = new FormattedText(textAnnotation3_2.Text, System.Globalization.CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(textAnnotation3_2.Font), textAnnotation3_2.FontSize, Brushes.Black);
                screen_x += ft.WidthIncludingTrailingWhitespace;
                model.Annotations.Add(textAnnotation3);
                model.Annotations.Add(textAnnotation3_2);
            }
        }
        public void display_eagle_eye(List<Theory_IOSTOPE> theory_isotopes, Spectra_MS1 selected_ms1, PlotModel model, MainWindow mainW)
        {
            int count = 0;
            for (int i = 0; i < theory_isotopes.Count; ++i)
            {
                for (int j = 0; j < theory_isotopes[i].mz.Count; ++j)
                {
                    if (theory_isotopes[i].mz[j] >= model.Axes[1].ActualMinimum && theory_isotopes[i].mz[j] <= model.Axes[1].ActualMaximum)
                    {
                        ++count;
                    }
                }
            }
            LineSeries lineSeries = new LineSeries();
            lineSeries.YAxisKey = model.Axes[4].Key;
            lineSeries.Color = Display_Detail_Help.border_color;
            Collection<IDataPoint> border_points = new Collection<IDataPoint>();
            double start_X = model.Axes[1].ActualMaximum - 0.20 * (model.Axes[1].ActualMaximum - model.Axes[1].ActualMinimum);
            double end_X = model.Axes[1].ActualMaximum - 0.01 * (model.Axes[1].ActualMaximum - model.Axes[1].ActualMinimum);
            //double width = (end_X - start_X) / (count + 1);
            double width = (end_X - start_X) / 6;
            border_points.Add(new DataPoint(start_X, 0));
            border_points.Add(new DataPoint(end_X, 0));
            border_points.Add(new DataPoint(end_X, 1));
            border_points.Add(new DataPoint(start_X, 1));
            border_points.Add(new DataPoint(start_X, 0));
            lineSeries.Points = border_points;
            model.Series.Add(lineSeries);

            //绘制鹰眼为符号的解释

            ScatterSeries ss1 = new ScatterSeries();
            ss1.YAxisKey = model.Axes[4].Key;
            ss1.MarkerStroke = ddhms1.theory_color;
            ss1.MarkerFill = OxyColors.Transparent;
            ss1.MarkerSize = ddhms1.theory_size;
            ss1.FontWeight = OxyPlot.FontWeights.Bold;
            ss1.MarkerType = ddhms1.theory_marker;
            List<IDataPoint> points1 = new List<IDataPoint>();
            points1.Add(new DataPoint(start_X + 0.5 * width, 0.8));
            ss1.Points = points1;
            model.Series.Add(ss1);
            TextAnnotation txtAnnotation1 = new TextAnnotation();
            txtAnnotation1.YAxisKey = model.Axes[4].Key;
            txtAnnotation1.VerticalAlignment = OxyPlot.VerticalAlignment.Middle;
            txtAnnotation1.HorizontalAlignment = OxyPlot.HorizontalAlignment.Left;
            txtAnnotation1.Font = "Times New Roman";
            txtAnnotation1.Text = "Theoretical m/z";
            txtAnnotation1.Stroke = OxyColors.Transparent;
            txtAnnotation1.FontSize = 13;
            txtAnnotation1.FontWeight = OxyPlot.FontWeights.Bold;
            txtAnnotation1.TextColor = OxyColors.Black;
            txtAnnotation1.Position = new DataPoint(start_X + 1.5 * width, 0.8);
            model.Annotations.Add(txtAnnotation1);
            ScatterSeries ss2 = new ScatterSeries();
            ss2.YAxisKey = model.Axes[4].Key;
            ss2.MarkerStroke = ddhms1.mgf_color;
            ss2.MarkerFill = OxyColors.Transparent;
            ss2.MarkerSize = ddhms1.mgf_size;
            ss2.FontWeight = OxyPlot.FontWeights.Bold;
            ss2.MarkerType = ddhms1.mgf_marker;
            List<IDataPoint> points2 = new List<IDataPoint>();
            points2.Add(new DataPoint(start_X + 0.5 * width, 0.5));
            ss2.Points = points2;
            model.Series.Add(ss2);
            TextAnnotation txtAnnotation2 = new TextAnnotation();
            txtAnnotation2.YAxisKey = model.Axes[4].Key;
            txtAnnotation2.VerticalAlignment = OxyPlot.VerticalAlignment.Middle;
            txtAnnotation2.HorizontalAlignment = OxyPlot.HorizontalAlignment.Left;
            txtAnnotation2.Font = "Times New Roman";
            txtAnnotation2.Text = "Experimental m/z";
            txtAnnotation2.Stroke = OxyColors.Transparent;
            txtAnnotation2.FontSize = 13;
            txtAnnotation2.FontWeight = OxyPlot.FontWeights.Bold;
            txtAnnotation2.TextColor = OxyColors.Black;
            txtAnnotation2.Position = new DataPoint(start_X + 1.5 * width, 0.5);
            model.Annotations.Add(txtAnnotation2);
            ScatterSeries ss3 = new ScatterSeries();
            ss3.YAxisKey = model.Axes[4].Key;
            ss3.MarkerStroke = ddhms1.other_color;
            ss3.MarkerFill = OxyColors.Transparent;
            ss3.MarkerSize = ddhms1.other_size;
            ss3.FontWeight = OxyPlot.FontWeights.Bold;
            ss3.MarkerType = ddhms1.other_marker;
            List<IDataPoint> points3 = new List<IDataPoint>();
            points3.Add(new DataPoint(start_X + 0.5 * width, 0.2));
            ss3.Points = points3;
            model.Series.Add(ss3);
            TextAnnotation txtAnnotation3 = new TextAnnotation();
            txtAnnotation3.YAxisKey = model.Axes[4].Key;
            txtAnnotation3.VerticalAlignment = OxyPlot.VerticalAlignment.Middle;
            txtAnnotation3.HorizontalAlignment = OxyPlot.HorizontalAlignment.Left;
            txtAnnotation3.Font = "Times New Roman";
            txtAnnotation3.Text = "Theoretical Distribution";
            txtAnnotation3.Stroke = OxyColors.Transparent;
            txtAnnotation3.FontSize = 13;
            txtAnnotation3.FontWeight = OxyPlot.FontWeights.Bold;
            txtAnnotation3.TextColor = OxyColors.Black;
            txtAnnotation3.Position = new DataPoint(start_X + 1.5 * width, 0.2);
            model.Annotations.Add(txtAnnotation3);
            //END
            if (theory_isotopes.Count == 0 || theory_isotopes[0].intensity.Count == 0)
                return;
            int theory_max_i = 0, theory_max_j = 0;
            double theory_max = theory_isotopes[0].intensity[0];
            for (int i = 0; i < theory_isotopes.Count; ++i)
            {
                for (int j = 0; j < theory_isotopes[i].intensity.Count; ++j)
                {
                    if (theory_isotopes[i].intensity[j] > theory_max)
                    {
                        theory_max = theory_isotopes[i].intensity[j];
                        theory_max_i = i;
                        theory_max_j = j;
                    }
                }
            }
            List<IDataPoint> star_points = new List<IDataPoint>(); //理论同位素峰簇
            List<IDataPoint> pep_theroy_mz_points = new List<IDataPoint>(); //鉴定到的肽段的理论m/z
            List<IDataPoint> mgf_mz_points = new List<IDataPoint>(); //MGF给出的m/z
            double star_ratio = 100 / theory_isotopes[theory_max_i].intensity[theory_max_j];

            double mean_intensity = 0.0;
            int intensity_count = 0;
            List<LineSeries> theory_ls_tmp = new List<LineSeries>();
            for (int i = 0; i < theory_isotopes.Count; ++i)
            {
                intensity_count += theory_isotopes[i].intensity.Count;
                for (int j = 0; j < theory_isotopes[i].intensity.Count; ++j)
                {
                    mean_intensity += theory_isotopes[i].intensity[j];
                    DataPoint point = new DataPoint(theory_isotopes[i].mz[j], theory_isotopes[i].intensity[j] * star_ratio);
                    if (point.X < model.Axes[1].ActualMinimum || point.X > model.Axes[1].ActualMaximum)
                        continue;
                    star_points.Add(point);
                    List<IDataPoint> points = new List<IDataPoint>();
                    points.Add(new DataPoint(point.X, 0));
                    points.Add(new DataPoint(point.X, point.Y));
                    LineSeries ls = new LineSeries();
                    ls.Points = points;
                    ls.LineStyle = LineStyle.Dash;
                    ls.StrokeThickness = 1.0;
                    ls.Color = OxyColor.FromArgb(200, OxyColors.DarkRed.R, OxyColors.DarkRed.G, OxyColors.DarkRed.B);
                    theory_ls_tmp.Add(ls);
                    model.Series.Add(ls);
                }
            }
            mean_intensity = mean_intensity * star_ratio / intensity_count;
            double mean_intensity2 = mean_intensity + 10;
            if (mean_intensity2 >= 100)
                mean_intensity2 = 95;
            pep_theroy_mz_points.Add(new DataPoint(selected_ms1.Pepmass_theory, mean_intensity));
            mgf_mz_points.Add(new DataPoint(selected_ms1.Pepmass_ms2, mean_intensity2));
            ScatterSeries scatterSeries1 = new ScatterSeries();
            scatterSeries1.TrackerFormatString = "m/z: {2:0.000}\nIntensity: {4:0.00}%";
            scatterSeries1.MarkerStroke = ddhms1.other_color;
            scatterSeries1.MarkerFill = OxyColors.Transparent;
            scatterSeries1.MarkerSize = ddhms1.other_size;
            scatterSeries1.FontWeight = OxyPlot.FontWeights.Bold;
            scatterSeries1.MarkerType = ddhms1.other_marker;
            scatterSeries1.Points = star_points;
            model.Series.Add(scatterSeries1);

            ScatterSeries scatterSeries2 = new ScatterSeries();
            scatterSeries2.TrackerFormatString = "m/z: {2:0.000}\nIntensity: {4:0.00}%";
            scatterSeries2.MarkerStroke = ddhms1.theory_color;
            scatterSeries2.MarkerFill = OxyColors.Transparent;
            scatterSeries2.MarkerSize = ddhms1.theory_size;
            scatterSeries2.FontWeight = OxyPlot.FontWeights.Bold;
            scatterSeries2.MarkerType = ddhms1.theory_marker;
            scatterSeries2.Points = pep_theroy_mz_points;
            model.Series.Add(scatterSeries2);

            ScatterSeries scatterSeries3 = new ScatterSeries();
            scatterSeries3.TrackerFormatString = "m/z: {2:0.000}\nIntensity: {4:0.00}%";
            scatterSeries3.MarkerStroke = ddhms1.mgf_color;
            scatterSeries3.MarkerFill = OxyColors.Transparent;
            scatterSeries3.MarkerSize = ddhms1.mgf_size;
            scatterSeries3.FontWeight = OxyPlot.FontWeights.Bold;
            scatterSeries3.MarkerType = ddhms1.mgf_marker;
            scatterSeries3.Points = mgf_mz_points;
            model.Series.Add(scatterSeries3);

            bool is_move_1 = false, is_move_2 = false, is_move_3 = false;
            int index_point1 = -1, index_point2 = -1, index_point3 = -1;
            double min_intensity = 1, max_intensity = 102;
            scatterSeries1.MouseDown += (s, e) =>
            {
                index_point1 = (int)Math.Round(e.HitTestResult.Index);
                double mz = scatterSeries1.Points[index_point1].X;
                if (e.ChangedButton == OxyMouseButton.Left)
                {
                    if (mainW.ms1_mz1_txt.Text == "NaN")
                    {
                        mainW.ms1_mz1_txt.Text = mz.ToString("F5");
                    }
                    else
                    {
                        mainW.ms1_mz2_txt.Text = mz.ToString("F5");
                    }
                    mainW.compute_btn();
                    mainW.one_width = (mainW.Model1.Axes[1].ActualMaximum - mainW.Model1.Axes[1].ActualMinimum) / ((int)mainW.Model1.Axes[1].ScreenMax.X - (int)mainW.Model1.Axes[1].ScreenMin.X);
                    mainW.ms1_draw_speedUp(mainW.one_width);
                }
                if (e.ChangedButton != OxyMouseButton.Right)
                    return;
                is_move_1 = true;
                is_move_2 = false;
                is_move_3 = false;
                model.RefreshPlot(false);
                e.Handled = true;
            };
            scatterSeries2.MouseDown += (s, e) =>
            {
                index_point2 = (int)Math.Round(e.HitTestResult.Index);
                double mz = scatterSeries2.Points[index_point2].X;
                if (e.ChangedButton == OxyMouseButton.Left)
                {
                    if (mainW.ms1_mz1_txt.Text == "NaN")
                    {
                        mainW.ms1_mz1_txt.Text = mz.ToString("F5");
                    }
                    else
                    {
                        mainW.ms1_mz2_txt.Text = mz.ToString("F5");
                    }
                    mainW.compute_btn();
                    mainW.one_width = (mainW.Model1.Axes[1].ActualMaximum - mainW.Model1.Axes[1].ActualMinimum) / ((int)mainW.Model1.Axes[1].ScreenMax.X - (int)mainW.Model1.Axes[1].ScreenMin.X);
                    mainW.ms1_draw_speedUp(mainW.one_width);
                }
                if (e.ChangedButton != OxyMouseButton.Right)
                    return;
                is_move_1 = false;
                is_move_2 = true;
                is_move_3 = false;
                model.RefreshPlot(false);
                e.Handled = true;
            };
            scatterSeries3.MouseDown += (s, e) =>
            {
                index_point3 = (int)Math.Round(e.HitTestResult.Index);
                double mz = scatterSeries3.Points[index_point3].X;
                if (e.ChangedButton == OxyMouseButton.Left)
                {
                    if (mainW.ms1_mz1_txt.Text == "NaN")
                    {
                        mainW.ms1_mz1_txt.Text = mz.ToString("F5");
                    }
                    else
                    {
                        mainW.ms1_mz2_txt.Text = mz.ToString("F5");
                    }
                    mainW.compute_btn();
                    mainW.one_width = (mainW.Model1.Axes[1].ActualMaximum - mainW.Model1.Axes[1].ActualMinimum) / ((int)mainW.Model1.Axes[1].ScreenMax.X - (int)mainW.Model1.Axes[1].ScreenMin.X);
                    mainW.ms1_draw_speedUp(mainW.one_width);
                }
                if (e.ChangedButton != OxyMouseButton.Right)
                    return;
                is_move_1 = false;
                is_move_2 = false;
                is_move_3 = true;
                model.RefreshPlot(false);
                e.Handled = true;
            };
            scatterSeries1.MouseMove += (s, e) =>
            {
                if (is_move_1 && index_point1 >= 0)
                {
                    double new_scan = 0.0;
                    bool is_flag = false;
                    DataPoint new_point = Axis.InverseTransform(e.Position, model.Axes[1], model.Axes[0]);
                    double scan = new_point.Y / scatterSeries1.Points[index_point1].Y;
                    for (int i = 0; i < scatterSeries1.Points.Count; ++i)
                    {
                        scatterSeries1.Points[i].Y *= scan;
                        if (scatterSeries1.Points[i].Y > max_intensity)
                        {
                            new_scan = scan;
                            is_flag = true;
                        }
                        else if (scatterSeries1.Points[i].Y < min_intensity)
                        {
                            new_scan = scan;
                            is_flag = true;
                        }
                    }
                    if (is_flag)
                    {
                        scan = new_scan;
                        for (int i = 0; i < scatterSeries1.Points.Count; ++i)
                        {
                            scatterSeries1.Points[i].Y /= scan;
                        }
                    }
                }
                for (int i = 0; i < theory_ls_tmp.Count; ++i)
                {
                    theory_ls_tmp[i].Points[1].Y = scatterSeries1.Points[i].Y;
                }
                model.RefreshPlot(false);
                e.Handled = true;
            };
            scatterSeries2.MouseMove += (s, e) =>
            {
                if (is_move_2 && index_point2 >= 0)
                {
                    double new_scan = 0.0;
                    bool is_flag = false;
                    DataPoint new_point = Axis.InverseTransform(e.Position, model.Axes[1], model.Axes[0]);
                    double scan = new_point.Y / scatterSeries2.Points[index_point2].Y;
                    for (int i = 0; i < scatterSeries2.Points.Count; ++i)
                    {
                        scatterSeries2.Points[i].Y *= scan;
                        if (scatterSeries2.Points[i].Y > max_intensity)
                        {
                            new_scan = scan;
                            is_flag = true;
                        }
                        else if (scatterSeries2.Points[i].Y < min_intensity)
                        {
                            new_scan = scan;
                            is_flag = true;
                        }
                    }
                    if (is_flag)
                    {
                        scan = new_scan;
                        for (int i = 0; i < scatterSeries2.Points.Count; ++i)
                        {
                            scatterSeries2.Points[i].Y /= scan;
                        }
                    }
                }
                model.RefreshPlot(false);
                e.Handled = true;
            };
            scatterSeries3.MouseMove += (s, e) =>
            {
                if (is_move_3 && index_point3 >= 0)
                {
                    double new_scan = 0.0;
                    bool is_flag = false;
                    DataPoint new_point = Axis.InverseTransform(e.Position, model.Axes[1], model.Axes[0]);
                    double scan = new_point.Y / scatterSeries3.Points[index_point3].Y;
                    for (int i = 0; i < scatterSeries3.Points.Count; ++i)
                    {
                        scatterSeries3.Points[i].Y *= scan;
                        if (scatterSeries3.Points[i].Y > max_intensity)
                        {
                            new_scan = scan;
                            is_flag = true;
                        }
                        else if (scatterSeries3.Points[i].Y < min_intensity)
                        {
                            new_scan = scan;
                            is_flag = true;
                        }
                    }
                    if (is_flag)
                    {
                        scan = new_scan;
                        for (int i = 0; i < scatterSeries3.Points.Count; ++i)
                        {
                            scatterSeries3.Points[i].Y /= scan;
                        }
                    }
                }
                model.RefreshPlot(false);
                e.Handled = true;
            };
        }

        public PlotModel display_ms2(int mix_flag = 1) //mix_flag=1表示单肽段的二级匹配图，mix_flag=number表示混合谱的显示，阶梯需要有多行(number行)进行显示
        {
            mix_flag = Display_Detail_Help.pTop_Ladder_Count;
            if (psm_help.Spec.Peaks.Count == 0)
                return null;
            //ar tmp = new PlotModel("Peak Series") { LegendPlacement = LegendPlacement.Outside, LegendPosition = LegendPosition.RightTop, LegendOrientation = LegendOrientation.Vertical };
            var tmp = new PlotModel();

            double start = 0.9 - Display_Detail_Help.Ladder_Height, end = 0.9, width = Display_Detail_Help.Ladder_Height;

            if (mix_flag > 1)
                start -= (mix_flag - 1) * 0.15;

            var linearAxis1 = new LinearAxis();
            linearAxis1.Key = "peaks";
            linearAxis1.StartPosition = 0.15;
            linearAxis1.EndPosition = start;
            linearAxis1.Minimum = -0.0001; //设为<0的值可以防止横轴时隐时现现象
            linearAxis1.Maximum = 100;
            linearAxis1.MaximumPadding = 0;
            linearAxis1.IntervalLength = 40;
            linearAxis1.ShowMinorTicks = false;
            linearAxis1.Title = "Relative Intensity (%)";
            linearAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis1.TitleFontSize = 15;
            linearAxis1.TitleFont = "Calibri";
            linearAxis1.AxisTitleDistance = 0;
            linearAxis1.IsPanEnabled = false;
            linearAxis1.IsZoomEnabled = false;
            tmp.Axes.Add(linearAxis1);

            var linearAxis2 = new LinearAxis();
            linearAxis2.Position = AxisPosition.Bottom;
            linearAxis2.Minimum = psm_help.Spec.Peaks[0].Mass - 50;
            if (linearAxis2.Minimum < 0)
                linearAxis2.Minimum = 0;
            linearAxis2.Maximum = psm_help.Spec.Peaks[psm_help.Spec.Peaks.Count - 1].Mass + 50;
            linearAxis2.MajorStep = (linearAxis2.Maximum - linearAxis2.Minimum) / 20;
            linearAxis2.AbsoluteMinimum = linearAxis2.Minimum;
            linearAxis2.AbsoluteMaximum = linearAxis2.Maximum;
            linearAxis2.Title = "m/z";
            linearAxis2.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis2.TitleFontSize = 15;
            linearAxis2.TitleFont = "Calibri";
            linearAxis2.AxisTitleDistance = 3.0;
            linearAxis2.ShowMinorTicks = false; 
            tmp.Axes.Add(linearAxis2);

            var linearAxis3 = new LinearAxis();
            linearAxis3.Key = "mass_error";
            linearAxis3.StartPosition = 0.0;
            linearAxis3.EndPosition = 0.12;
            linearAxis3.ShowMinorTicks = false; //让坐标轴上没有间隔的小线出现

            linearAxis3.MajorStep = (linearAxis3.Maximum - linearAxis3.Minimum) / 2;
            linearAxis3.MajorGridlineThickness = 1.5;
            linearAxis3.MajorGridlineStyle = LineStyle.Dot;
            if (psm_help.Ppm_mass_error != 0.0)
                linearAxis3.Title = "ppm";
            else
                linearAxis3.Title = "Da";
            linearAxis3.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis3.TitleFontSize = 15;
            linearAxis3.TitleFont = "Calibri";
            if (psm_help.Ppm_mass_error != 0.0)
            {
                linearAxis3.Minimum = -psm_help.Ppm_mass_error * 1.0e6;
                linearAxis3.Maximum = psm_help.Ppm_mass_error * 1.0e6;
            }
            else
            {
                linearAxis3.Minimum = -psm_help.Da_mass_error;
                linearAxis3.Maximum = psm_help.Da_mass_error;
            }
            linearAxis3.IsPanEnabled = false;
            linearAxis3.IsZoomEnabled = false;
            linearAxis3.AxisTitleDistance = 20;
            tmp.Axes.Add(linearAxis3);


            for (int i = 0; i < mix_flag; ++i)
            {
                var linearAxis4 = new LinearAxis();
                linearAxis4.Key = "sq" + i;
                linearAxis4.StartPosition = end - width;
                linearAxis4.EndPosition = end;
                end -= width;
                linearAxis4.Minimum = 0; //100
                linearAxis4.Maximum = 20; //120
                linearAxis4.IsPanEnabled = false;
                linearAxis4.IsZoomEnabled = false;
                linearAxis4.TicklineColor = OxyColors.Transparent;
                linearAxis4.TextColor = OxyColors.Transparent;
                tmp.Axes.Add(linearAxis4);
            }

            var linearAxis5 = new LinearAxis();
            linearAxis5.Key = "title";
            linearAxis5.StartPosition = 0.90;
            linearAxis5.EndPosition = 0.95;
            linearAxis5.Minimum = 0;
            linearAxis5.Maximum = 10;
            linearAxis5.IsPanEnabled = false;
            linearAxis5.IsZoomEnabled = false;
            linearAxis5.TicklineColor = OxyColors.Transparent;
            linearAxis5.TextColor = OxyColors.Transparent;
            tmp.Axes.Add(linearAxis5);

            var linearAxis6 = new LinearAxis();
            linearAxis6.Key = "title2";
            linearAxis6.StartPosition = 0.95;
            linearAxis6.EndPosition = 1.0;
            linearAxis6.Minimum = 0;
            linearAxis6.Maximum = 10;
            linearAxis6.IsPanEnabled = false;
            linearAxis6.IsZoomEnabled = false;
            linearAxis6.TicklineColor = OxyColors.Transparent;
            linearAxis6.TextColor = OxyColors.Transparent;
            tmp.Axes.Add(linearAxis6);

            tmp.Padding = new OxyThickness(0, 0, 0, 0);

            tmp.MouseDown += (s, e) =>
            {
                if (e.ChangedButton == OxyMouseButton.Middle && e.ClickCount == 2)
                {
                    tmp.Axes[1].Minimum = tmp.Axes[1].AbsoluteMinimum;
                    tmp.Axes[1].Maximum = tmp.Axes[1].AbsoluteMaximum;
                    tmp.Axes[1].Reset();
                    tmp.RefreshPlot(false);
                    e.Handled = true;
                }
            };

            return tmp;
        }
        public PlotModel display_scan_me(string Da_PPM, int x)
        {
            var plotModel = new PlotModel();
            plotModel.Title = "Raw " + (x + 1) + ": Scan-Mass Deviation";
            var linearAxis1 = new LinearAxis();
            linearAxis1.IsPanEnabled = false;
            linearAxis1.IsZoomEnabled = false;
            linearAxis1.Title = "Mass Deviation (" + Da_PPM + ")";
            linearAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis1.TitleFontSize = 15;
            linearAxis1.AxisTitleDistance = 4.0;
            if (Da_PPM == "Da")
                linearAxis1.StringFormat = "f4";
            else
                linearAxis1.StringFormat = "f2";
            plotModel.Axes.Add(linearAxis1);
            var linearAxis2 = new LinearAxis();
            linearAxis2.Position = AxisPosition.Bottom;
            linearAxis2.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis2.TitleFontSize = 15;
            linearAxis2.AxisTitleDistance = 3.0;
            linearAxis2.Title = "Scan";
            plotModel.Axes.Add(linearAxis2);
            ScatterSeries series = new ScatterSeries();
            ScatterSeries series2 = new ScatterSeries();
            series.MarkerFill = OxyColors.Blue;
            series2.MarkerFill = OxyColors.Red;
            series.MarkerSize = 1;
            series2.MarkerSize = 1;
            //series.Title = "Raw " + (x + 1);
            List<IDataPoint> points = new List<IDataPoint>();
            List<IDataPoint> points2 = new List<IDataPoint>();
            if (Da_PPM == "Da")
            {
                for (int j = 0; j < Scan_Raws[x].Target_Scan_Raw_Simples.Count; ++j)
                {
                    points.Add(new DataPoint(Scan_Raws[x].Target_Scan_Raw_Simples[j].Scan, Scan_Raws[x].Target_Scan_Raw_Simples[j].Mass_error_Da));
                }
                for (int j = 0; j < Scan_Raws[x].Decoy_Scan_Raw_Simples.Count; ++j)
                {
                    points2.Add(new DataPoint(Scan_Raws[x].Decoy_Scan_Raw_Simples[j].Scan, Scan_Raws[x].Decoy_Scan_Raw_Simples[j].Mass_error_Da));
                }
            }
            else
            {
                for (int j = 0; j < Scan_Raws[x].Target_Scan_Raw_Simples.Count; ++j)
                {
                    points.Add(new DataPoint(Scan_Raws[x].Target_Scan_Raw_Simples[j].Scan, Scan_Raws[x].Target_Scan_Raw_Simples[j].Mass_error_ppm));
                }
                for (int j = 0; j < Scan_Raws[x].Decoy_Scan_Raw_Simples.Count; ++j)
                {
                    points2.Add(new DataPoint(Scan_Raws[x].Decoy_Scan_Raw_Simples[j].Scan, Scan_Raws[x].Decoy_Scan_Raw_Simples[j].Mass_error_ppm));
                }
            }
            series.Points = points;
            series2.Points = points2;
            series.MouseDown += (s, e) =>
            {
                if (e.ChangedButton != OxyMouseButton.Right)
                    return;
                int index = (int)Math.Round(e.HitTestResult.Index);
                index = Scan_Raws[x].Target_Scan_Raw_Simples[index].psm_index;
                if (index < 0 || index >= mainW.psms.Count)
                    return;
                PSM one_psm = mainW.psms[index];
                ObservableCollection<PSM> psms_tmp = new ObservableCollection<PSM>();
                psms_tmp.Add(one_psm);
                mainW.display_psms = psms_tmp;
                mainW.data.ItemsSource = mainW.display_psms;
                mainW.display_size.Text = mainW.display_psms.Count().ToString();
                mainW.summary_tab.SelectedIndex = 1;
            };
            plotModel.Series.Add(series);
            plotModel.Series.Add(series2);

            plotModel.Padding = new OxyThickness(0, 0, 30, 0);
            return plotModel;
        }
        public PlotModel display_boxplot(string Da_PPM)
        {
            if (this.BoxPlots.Count == 0)
                return null;
            double min = double.MaxValue;
            double max = double.MinValue;
            for (int i = 0; i < this.BoxPlots.Count; ++i)
            {
                if (min > this.BoxPlots[i].MIN)
                    min = this.BoxPlots[i].MIN;
                if (max < this.BoxPlots[i].MAX)
                    max = this.BoxPlots[i].MAX;
            }
            var plotModel1 = new PlotModel();
            plotModel1.LegendPlacement = LegendPlacement.Outside;
            plotModel1.Title = "Mass Deviation";
            var linearAxis1 = new LinearAxis();
            linearAxis1.Minimum = 1.05 * min;
            linearAxis1.Maximum = 1.05 * max;
            linearAxis1.AbsoluteMinimum = linearAxis1.Minimum;
            linearAxis1.AbsoluteMaximum = linearAxis1.Maximum;
            linearAxis1.Title = "Mass Deviation (" + Da_PPM + ")";
            linearAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis1.TitleFontSize = 15;
            linearAxis1.AxisTitleDistance = 4.0;
            if (Da_PPM == "Da")
                linearAxis1.StringFormat = "f4";
            else
                linearAxis1.StringFormat = "f2";
            linearAxis1.MajorStep = (linearAxis1.Maximum - linearAxis1.Minimum) / 5;
            plotModel1.Axes.Add(linearAxis1);
            var linearAxis2 = new LinearAxis();
            linearAxis2.IsPanEnabled = false;
            linearAxis2.IsZoomEnabled = false;
            linearAxis2.Title = "Raw";
            linearAxis2.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis2.TitleFontSize = 15;
            linearAxis2.AxisTitleDistance = 3.0;
            linearAxis2.Position = AxisPosition.Bottom;
            linearAxis2.Minimum = 0;
            linearAxis2.Maximum = this.BoxPlots.Count() + 1;
            linearAxis2.AbsoluteMinimum = linearAxis2.Minimum;
            linearAxis2.AbsoluteMaximum = linearAxis2.Maximum;
            linearAxis2.MajorStep = 1.0;
            linearAxis2.ShowMinorTicks = false; //让坐标轴上没有间隔的小线出现
            plotModel1.Axes.Add(linearAxis2);
            var boxPlotSeries1 = new BoxPlotSeries();
            boxPlotSeries1.TrackerFormatString = "X: {1:0}\nUpper Whisker: {2:0.0000}\n" +
                "Third Quartil: {3:0.0000}\nMedian: {4:0.0000}\nFirst Quartil: {5:0.0000}\n" +
                "Lower Whisker: {6:0.0000}";
            List<BoxPlotItem> bpis = new List<BoxPlotItem>();
            for (int i = 0; i < BoxPlots.Count; ++i)
            {
                BoxPlotItem bpi = new BoxPlotItem();
                bpi.X = (i + 1);
                bpi.LowerWhisker = BoxPlots[i].MIN;
                bpi.BoxBottom = BoxPlots[i].Q1;
                bpi.Median = BoxPlots[i].Median;
                bpi.BoxTop = BoxPlots[i].Q3;
                bpi.UpperWhisker = BoxPlots[i].MAX;
                bpi.Outliers = new List<double>();
                //bpi.Outliers = BoxPlots[i].Outliers;
                bpis.Add(bpi);
            }
            boxPlotSeries1.Items = bpis;
            boxPlotSeries1.Selectable = true;
            boxPlotSeries1.MouseDown += (s, e) =>
            {
                if (e.ClickCount != 2 || e.ChangedButton != OxyMouseButton.Left)
                    return;
                double specific_x = plotModel1.Axes[1].InverseTransform(e.Position.X);
                int x = (int)Math.Round(specific_x);
                if (x - 1 < 0 || x - 1 >= BoxPlots.Count)
                    return;
                BoxPlot_Help boxplot = BoxPlots[x - 1];
                Display_Help dis_help0 = null;
                List<int> target_psm_indexs = MassError_Raws[x - 1].get_target_psm_index();
                List<int> decoy_psm_indexs = MassError_Raws[x - 1].get_decoy_psm_index();
                if (Da_PPM == "Da")
                {
                    List<IDataPoint> target_points = MassError_Raws[x - 1].get_target_da_point();
                    List<IDataPoint> decoy_points = MassError_Raws[x - 1].get_decoy_da_point();
                    dis_help0 = new Display_Help(target_points, decoy_points,
                        MassError_Raws[x - 1].Max_MassError_Da, MassError_Raws[x - 1].Max_Score, mainW);
                }
                else
                {
                    List<IDataPoint> target_points = MassError_Raws[x - 1].get_target_ppm_point();
                    List<IDataPoint> decoy_points = MassError_Raws[x - 1].get_decoy_ppm_point();
                    dis_help0 = new Display_Help(target_points, decoy_points,
                        MassError_Raws[x - 1].Max_MassError_PPM, MassError_Raws[x - 1].Max_Score, mainW);
                }
                dis_help0.Target_psm_indexs = target_psm_indexs;
                dis_help0.Decoy_psm_indexs = decoy_psm_indexs;
                Model_Window model_window = new Model_Window(dis_help0.display_mass_error(Da_PPM, x - 1), 1);
                model_window.Show();
                dis_help0.Scan_Raws = Scan_Raws;
                Model_Window model_window2 = new Model_Window(dis_help0.display_scan_me(Da_PPM, x - 1), 2);
                model_window2.Show();
            };
            plotModel1.Series.Add(boxPlotSeries1);
            return plotModel1;
        }
        public PlotModel display_chromatogram(double mz1, double mz2)
        {
            var tmp = new PlotModel();

            var linearAxis1 = new LinearAxis();
            linearAxis1.MaximumPadding = 0.01;
            linearAxis1.IntervalLength = 20;
            linearAxis1.Title = "Absolute Intensity";
            linearAxis1.StringFormat = "E1";
            linearAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis1.TitleFontSize = 15;
            linearAxis1.AxisTitleDistance = 10.0;
            linearAxis1.IsPanEnabled = false;
            linearAxis1.IsZoomEnabled = false;
            tmp.Axes.Add(linearAxis1);

            var linearAxis2 = new LinearAxis();
            linearAxis2.Position = AxisPosition.Bottom;
            linearAxis2.Title = "Scan";
            linearAxis2.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis2.TitleFontSize = 15;
            linearAxis2.AxisTitleDistance = 3.0;
            tmp.Axes.Add(linearAxis2);
            double maxInten = double.MinValue;

            LineSeries ls1 = new LineSeries();
            ls1.Color = OxyColors.Red;
            ls1.Title = "XIC 1: " + mz1.ToString("F2");
            List<IDataPoint> points1 = new List<IDataPoint>();
            for (int i = 0; i < peaks1.Count; ++i)
            {
                points1.Add(new DataPoint(peaks1[i].scan, peaks1[i].intensity));
                if (peaks1[i].intensity > maxInten)
                    maxInten = peaks1[i].intensity;
            }
            ls1.Points = points1;
            tmp.Series.Add(ls1);
            LineSeries ls2 = new LineSeries();
            ls2.Color = OxyColors.Blue;
            ls2.Title = "XIC 2: " + mz2.ToString("F2");
            List<IDataPoint> points2 = new List<IDataPoint>();
            for (int i = 0; i < peaks2.Count; ++i)
            {
                points2.Add(new DataPoint(peaks2[i].scan, peaks2[i].intensity));
                if (peaks2[i].intensity > maxInten)
                    maxInten = peaks2[i].intensity;
            }
            ls2.Points = points2;
            tmp.Series.Add(ls2);

            //增加一条虚线
            LineSeries ls3 = new LineSeries();
            ls3.Color = OxyColors.Black;
            ls3.LineStyle = LineStyle.Dot;
            List<IDataPoint> points3 = new List<IDataPoint>();
            if (peaks1.Count > 0)
            {
                points3.Add(new DataPoint(peaks1[peaks1.Count / 2 - 1].scan, 0));
                points3.Add(new DataPoint(peaks1[peaks1.Count / 2 - 1].scan, maxInten));
            }
            ls3.Points = points3;
            tmp.Series.Add(ls3);
            return tmp;
        }
        public PlotModel display_heat_map()
        {
            List<int> ms1_scan_list = new List<int>();
            List<int> ms1_scan_list2 = new List<int>();
            var tmp = new PlotModel();

            var linearAxis1 = new LinearAxis();
            linearAxis1.Title = "Scan";
            linearAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis1.TitleFontSize = 15;
            linearAxis1.AxisTitleDistance = 0;
            tmp.Axes.Add(linearAxis1);

            var linearAxis2 = new LinearAxis();
            linearAxis2.Position = AxisPosition.Bottom;
            linearAxis2.Title = "m/z";
            linearAxis2.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis2.TitleFontSize = 15;
            linearAxis2.AxisTitleDistance = 3.0;
            tmp.Axes.Add(linearAxis2);

            ScatterSeries ss1 = new ScatterSeries();
            ss1.MarkerSize = 1.0;
            OxyColor color1 = OxyColor.FromArgb(60, OxyColors.Black.R, OxyColors.Black.G, OxyColors.Black.B);
            ss1.MarkerFill = color1;
            List<IDataPoint> points = new List<IDataPoint>();
            for (int i = 0; i < ms1_list[0].Count; ++i)
            {
                for (int j = 0; j < ms1_list[0][i].Peaks.Count; ++j)
                {
                    points.Add(new DataPoint(ms1_list[0][i].Peaks[j].Mass, ms1_list[0][i].Scan));
                }
            }
            ss1.Points = points;
            tmp.Series.Add(ss1);

            ScatterSeries ss2 = new ScatterSeries();
            ss2.MarkerSize = 1.0;
            OxyColor color2 = OxyColor.FromArgb(60, OxyColors.Red.R, OxyColors.Red.G, OxyColors.Red.B);
            ss2.MarkerFill = color2;
            List<IDataPoint> points2 = new List<IDataPoint>();
            for (int i = 0; i < ms1_list[1].Count; ++i)
            {
                for (int j = 0; j < ms1_list[1][i].Peaks.Count; ++j)
                {
                    points2.Add(new DataPoint(ms1_list[1][i].Peaks[j].Mass, ms1_list[1][i].Scan));
                    ms1_scan_list.Add(i);
                    ms1_scan_list2.Add(j);
                }
            }
            ss2.Points = points2;
            tmp.Series.Add(ss2);
            ss2.MouseDown += (s, e) =>
            {
                if (e.ChangedButton != OxyMouseButton.Right)
                    return;
                int index = (int)Math.Round(e.HitTestResult.Index);
                if (index < 0 || index >= ss2.Points.Count)
                    return;
                int x = ms1_scan_list[index];
                int y = ms1_scan_list2[index];
                int psm_index = ms1_list[1][x].Peaks_index[y];
                ObservableCollection<PSM> psms_tmp = new ObservableCollection<PSM>();
                psms_tmp.Add(mainW.psms[psm_index]);
                mainW.display_psms = psms_tmp;
                mainW.data.ItemsSource = mainW.display_psms;
                mainW.display_size.Text = mainW.display_psms.Count().ToString();
                mainW.summary_tab.SelectedIndex = 1;
            };

            return tmp;
        }
        public PlotModel display_scan_ratio_graph()
        {
            var tmp = new PlotModel();
            tmp.Title = "Scan Ratio Bin";
            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.Position = AxisPosition.Bottom;
            categoryAxis1.Title = "Scan"; //unit²,上下标的字符串
            categoryAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            categoryAxis1.TitleFontSize = 15;
            categoryAxis1.AxisTitleDistance = 10;
            //categoryAxis1.MajorStep = (all_scans.Last() - all_scans.First()) / 5;
            categoryAxis1.FontSize = 10.0;
            categoryAxis1.IsPanEnabled = false;
            categoryAxis1.IsZoomEnabled = false;
            categoryAxis1.IsTickCentered = true;
            double ratio_offset = 0.0;
            for (int i = 0; i < all_scans.Count; ++i)
            {
                categoryAxis1.Labels.Add((all_scans[i] + ratio_offset).ToString("F0"));
            }
            tmp.Axes.Add(categoryAxis1);

            var linearAxis1 = new LinearAxis();
            linearAxis1.Position = AxisPosition.Left;
            linearAxis1.IntervalLength = 20;
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MinorGridlineStyle = LineStyle.Dot;
            linearAxis1.Title = "Number of Non-Ratios";
            linearAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis1.TitleFontSize = 15;
            linearAxis1.Minimum = 0.0;
            linearAxis1.AxisTitleDistance = 10;
            linearAxis1.IsPanEnabled = false;
            linearAxis1.IsZoomEnabled = false;
            tmp.Axes.Add(linearAxis1);
            var barSeries1 = new ColumnSeries();
            barSeries1.StrokeThickness = 1;
            barSeries1.Title = "Non-Ratio";
            barSeries1.FillColor = OxyColors.DarkOrange;
            for (int i = 0; i < scan_bin_list.Count; ++i)
            {
                barSeries1.Items.Add(new ColumnItem(scan_bin_list[i], -1));
            }
            tmp.Series.Add(barSeries1);
            return tmp;
        }
        //绘制两个任务的比较结果,numbers包含三项，分别表示任务1的PSM数，任务2的PSM数及任务1与任务2的共同鉴定到的PSM数
        public PlotModel display_compare_result(List<int> numbers)
        {
            PlotModel tmp = new PlotModel();
            tmp.PlotAreaBorderColor = OxyColors.Transparent;

            var linearAxis1 = new LinearAxis();
            linearAxis1.IsAxisVisible = false;
            linearAxis1.Minimum = -20;
            linearAxis1.Maximum = 60;
            tmp.Axes.Add(linearAxis1);

            var linearAxis2 = new LinearAxis();
            linearAxis2.IsAxisVisible = false;
            linearAxis2.Position = AxisPosition.Bottom;
            linearAxis2.Minimum = 0;
            linearAxis2.Maximum = 100;
            tmp.Axes.Add(linearAxis2);

            var ellipseAnnotation1 = new EllipseAnnotation();
            ellipseAnnotation1.X = 20;
            ellipseAnnotation1.Y = 40;
            ellipseAnnotation1.Width = 40;
            ellipseAnnotation1.Height = 40;
            ellipseAnnotation1.Fill = OxyColor.FromArgb(99, 0, 128, 0);
            ellipseAnnotation1.StrokeThickness = 2;
            var textAnnotation1 = new TextAnnotation();
            textAnnotation1.Position = new DataPoint(5, 40);
            textAnnotation1.Text = (numbers[0] - numbers[2]) + "";
            textAnnotation1.Stroke = OxyColors.Transparent;

            var ellipseAnnotation2 = new EllipseAnnotation();
            ellipseAnnotation2.X = 30;
            ellipseAnnotation2.Y = 40;
            ellipseAnnotation2.Width = 40;
            ellipseAnnotation2.Height = 40;
            ellipseAnnotation2.Fill = OxyColor.FromArgb(99, 0, 76, 46);
            ellipseAnnotation2.StrokeThickness = 2;
            var textAnnotation2 = new TextAnnotation();
            textAnnotation2.Position = new DataPoint(45, 40);
            textAnnotation2.Text = (numbers[1] - numbers[2]) + "";
            textAnnotation2.Stroke = OxyColors.Transparent;

            var textAnnotation3 = new TextAnnotation();
            textAnnotation3.Position = new DataPoint(25, 40);
            textAnnotation3.Text = (numbers[2]) + "";
            textAnnotation3.Stroke = OxyColors.Transparent;

            tmp.Annotations.Add(ellipseAnnotation1);
            tmp.Annotations.Add(ellipseAnnotation2);
            tmp.Annotations.Add(textAnnotation1);
            tmp.Annotations.Add(textAnnotation2);
            tmp.Annotations.Add(textAnnotation3);

            return tmp;
        }
        //绘制三个任务的结果比较，numbers包含七项，分别表示任务1的PSM数，任务2的PSM数，任务3的PSM数，1与2的交，2与3的交，1与3的交，1、2、3共同的交
        public PlotModel display_compare_result2(List<int> numbers)
        {
            int same12 = numbers[3] - numbers[6];
            int same23 = numbers[4] - numbers[6];
            int same13 = numbers[5] - numbers[6];
            int only1 = numbers[0] - same12 - same13 - numbers[6];
            int only2 = numbers[1] - same23 - same12 - numbers[6];
            int only3 = numbers[2] - same13 - same23 - numbers[6];
            PlotModel tmp = new PlotModel();
            tmp.PlotAreaBorderColor = OxyColors.Transparent;

            var linearAxis1 = new LinearAxis();
            linearAxis1.IsAxisVisible = false;
            
            tmp.Axes.Add(linearAxis1);

            var linearAxis2 = new LinearAxis();
            linearAxis2.IsAxisVisible = false;
            linearAxis2.Position = AxisPosition.Bottom;
            linearAxis2.Minimum = -20;
            linearAxis2.Maximum = 80;
            tmp.Axes.Add(linearAxis2);

            var ellipseAnnotation1 = new EllipseAnnotation();
            ellipseAnnotation1.X = 20;
            ellipseAnnotation1.Y = 40;
            ellipseAnnotation1.Width = 40;
            ellipseAnnotation1.Height = 40;
            ellipseAnnotation1.Fill = OxyColor.FromArgb(99, 0, 128, 0);
            ellipseAnnotation1.StrokeThickness = 2;
            var textAnnotation1 = new TextAnnotation();
            textAnnotation1.Position = new DataPoint(5, 40);
            textAnnotation1.Text = (only1) + "";
            textAnnotation1.Stroke = OxyColors.Transparent;

            var ellipseAnnotation2 = new EllipseAnnotation();
            ellipseAnnotation2.X = 30;
            ellipseAnnotation2.Y = 40;
            ellipseAnnotation2.Width = 40;
            ellipseAnnotation2.Height = 40;
            ellipseAnnotation2.Fill = OxyColor.FromArgb(99, 0, 86, 46);
            ellipseAnnotation2.StrokeThickness = 2;
            var textAnnotation2 = new TextAnnotation();
            textAnnotation2.Position = new DataPoint(45, 40);
            textAnnotation2.Text = (only2) + "";
            textAnnotation2.Stroke = OxyColors.Transparent;

            var ellipseAnnotation3 = new EllipseAnnotation();
            ellipseAnnotation3.X = 25;
            ellipseAnnotation3.Y = 30;
            ellipseAnnotation3.Width = 40;
            ellipseAnnotation3.Height = 40;
            ellipseAnnotation3.Fill = OxyColor.FromArgb(99, 0, 46, 86);
            ellipseAnnotation3.StrokeThickness = 2;
            var textAnnotation3 = new TextAnnotation();
            textAnnotation3.Position = new DataPoint(25, 10);
            textAnnotation3.Text = (only3) + "";
            textAnnotation3.Stroke = OxyColors.Transparent;

            var textAnnotation4 = new TextAnnotation();
            textAnnotation4.Position = new DataPoint(25, 50);
            textAnnotation4.Text = (same12) + "";
            textAnnotation4.Stroke = OxyColors.Transparent;

            var textAnnotation5 = new TextAnnotation();
            textAnnotation5.Position = new DataPoint(42, 25);
            textAnnotation5.Text = (same23) + "";
            textAnnotation5.Stroke = OxyColors.Transparent;

            var textAnnotation6 = new TextAnnotation();
            textAnnotation6.Position = new DataPoint(8, 25);
            textAnnotation6.Text = (same13) + "";
            textAnnotation6.Stroke = OxyColors.Transparent;

            var textAnnotation7 = new TextAnnotation();
            textAnnotation7.Position = new DataPoint(25, 33);
            textAnnotation7.Text = (numbers[6]) + "";
            textAnnotation7.Stroke = OxyColors.Transparent;

            tmp.Annotations.Add(ellipseAnnotation1);
            tmp.Annotations.Add(ellipseAnnotation2);
            tmp.Annotations.Add(ellipseAnnotation3);
            tmp.Annotations.Add(textAnnotation1);
            tmp.Annotations.Add(textAnnotation2);
            tmp.Annotations.Add(textAnnotation3);
            tmp.Annotations.Add(textAnnotation4);
            tmp.Annotations.Add(textAnnotation5);
            tmp.Annotations.Add(textAnnotation6);
            tmp.Annotations.Add(textAnnotation7);

            return tmp;
        }
        //绘制理论谱图
        public PlotModel display_predict_model(List<PEAK> predict_peaks, List<PEAK> actual_peaks, List<string> annotations)
        {
            //首先将两组谱峰分别归一化
            double max_inten = 0.0;
            for (int i = 0; i < predict_peaks.Count; ++i)
            {
                if (predict_peaks[i].Intensity > max_inten)
                    max_inten = predict_peaks[i].Intensity;
            }
            for (int i = 0; i < predict_peaks.Count; ++i)
            {
                predict_peaks[i].Intensity /= max_inten;
            }
            max_inten = 0.0;
            for (int i = 0; i < actual_peaks.Count; ++i)
            {
                if (actual_peaks[i].Intensity > max_inten)
                    max_inten = actual_peaks[i].Intensity;
            }
            for (int i = 0; i < actual_peaks.Count; ++i)
            {
                actual_peaks[i].Intensity /= max_inten;
            }

            PlotModel tmp = new PlotModel();

            var linearAxis1 = new LinearAxis();
            linearAxis1.IntervalLength = 20;
            linearAxis1.Title = "Relative Intensity";
            linearAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis1.TitleFontSize = 15;
            linearAxis1.AxisTitleDistance = 5;
            linearAxis1.Minimum = 0.0;
            linearAxis1.IsPanEnabled = false;
            linearAxis1.IsZoomEnabled = false;
            tmp.Axes.Add(linearAxis1);

            var linearAxis2 = new LinearAxis();
            linearAxis2.IntervalLength = 20;
            linearAxis2.Title = "m/z";
            linearAxis2.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis2.TitleFontSize = 15;
            linearAxis2.AxisTitleDistance = 5;
            linearAxis2.Position = AxisPosition.Bottom;
            tmp.Axes.Add(linearAxis2);

            for (int i = 0; i < predict_peaks.Count; ++i)
            {
                LineSeries ls = new LineSeries();
                ls.Color = OxyColors.Green;
                List<IDataPoint> points = new List<IDataPoint>();
                points.Add(new DataPoint(predict_peaks[i].Mass - 1, 0));
                points.Add(new DataPoint(predict_peaks[i].Mass - 1, predict_peaks[i].Intensity));
                ls.Points = points;
                tmp.Series.Add(ls);
                LineAnnotation la = new LineAnnotation();
                la.Text = annotations[i];
                la.Type = LineAnnotationType.Vertical;
                la.X = predict_peaks[i].Mass;
                la.Color = OxyColors.Green;
                double maxY = predict_peaks[i].Intensity > actual_peaks[i].Intensity ? predict_peaks[i].Intensity : actual_peaks[i].Intensity;
                la.MaximumY = maxY;
                la.TextVerticalAlignment = OxyPlot.VerticalAlignment.Top;
                la.TextPosition = 1.0;
                la.TextMargin = 1;
                if (la.MaximumY >= 0.5)
                    la.TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Right;
                else
                    la.TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Left;
                tmp.Annotations.Add(la);
            }
            for (int i = 0; i < actual_peaks.Count; ++i)
            {
                LineSeries ls = new LineSeries();
                ls.Color = OxyColors.Blue;
                List<IDataPoint> points = new List<IDataPoint>();
                points.Add(new DataPoint(actual_peaks[i].Mass + 1, 0));
                points.Add(new DataPoint(actual_peaks[i].Mass + 1, actual_peaks[i].Intensity));
                ls.Points = points;
                tmp.Series.Add(ls);
            }

            return tmp;
        }
        private void add_One_LineSeries(List<PEAK_MS1> mzs, int index, PlotModel model, double startX, double endX, double startY, double endY
            , OxyColor color, double maxIntensity)
        {
            double width_X = endX - startX;
            double width_Y = endY - startY;
            int minScan = mzs.First().scan;
            int maxScan = mzs.Last().scan;
            int width_Scan = maxScan - minScan;
            double minIntensity = 0.0;
            double width_Intensity = maxIntensity - minIntensity;
            double ratioX = width_X / width_Scan;
            double ratioY = width_Y / width_Intensity;

            //增加一个标注信息
            LineSeries simple_ls = new LineSeries();
            simple_ls.Color = color;
            simple_ls.YAxisKey = model.Axes[4].Key;
            List<IDataPoint> points0 = new List<IDataPoint>();
            points0.Add(new DataPoint(endX - width_X * 0.27, endY - width_Y * 0.1 - width_Y * 0.1 * index));
            points0.Add(new DataPoint(endX - width_X * 0.22, endY - width_Y * 0.1 - width_Y * 0.1 * index));
            simple_ls.Points = points0;
            model.Series.Add(simple_ls);

            TextAnnotation simple_txt = new TextAnnotation();
            simple_txt.TextColor = color;
            simple_txt.FontSize = 8;
            simple_txt.Stroke = OxyColors.Transparent;
            simple_txt.Text = mzs.First().mz.ToString("F2");
            simple_txt.YAxisKey = model.Axes[4].Key;
            simple_txt.Position = new DataPoint(endX - width_X * 0.21, endY - width_Y * 0.1 - width_Y * 0.1 * index);
            simple_txt.HorizontalAlignment = OxyPlot.HorizontalAlignment.Left;
            simple_txt.VerticalAlignment = OxyPlot.VerticalAlignment.Middle;
            model.Annotations.Add(simple_txt);

            List<IDataPoint> points = new List<IDataPoint>();
            LineSeries ls = new LineSeries();
            ls.YAxisKey = model.Axes[4].Key;
            ls.Color = color;
            for (int i = 0; i < mzs.Count; ++i)
            {
                int scan = mzs[i].scan;
                double intensity = mzs[i].intensity;
                points.Add(new DataPoint(startX + (scan - minScan) * ratioX, startY + (intensity - minIntensity) * ratioY));
            }
            ls.Points = points;
            model.Series.Add(ls);
        }
        private double get_ratio(List<PEAK_MS1> mz1, List<PEAK_MS1> mz2, int star_scan, int end_scan)
        {
            int star_index = 0, end_index = mz1.Count - 1;
            for (int i = 0; i < mz1.Count; ++i)
            {
                if (mz1[i].scan == star_scan)
                    star_index = i;
                if (mz1[i].scan == end_scan)
                    end_index = i;
            }
            System.IO.StreamWriter sw = new System.IO.StreamWriter("file_ratio.txt");
            for (int i = star_index; i <= end_index; ++i)
            {
                sw.WriteLine(mz1[i].intensity + " " + mz2[i].intensity);
            }
            sw.Flush();
            sw.Close();
            double ratio = 0.0;
            return ratio;
        }
        //将右上角的第一个鹰眼的东西删除
        private void initial_model(PlotModel model, double start_X, double end_X)
        {
            Collection<Annotation> anns = new Collection<Annotation>();
            Collection<Series> sers = new Collection<Series>();
            for (int i = 0; i < model.Annotations.Count; ++i)
            {
                if (model.Annotations[i] is TextAnnotation)
                {
                    TextAnnotation txt = model.Annotations[i] as TextAnnotation;
                    if (txt.YAxisKey != model.Axes[4].Key || txt.Position.X < start_X || txt.Position.X > end_X)
                        anns.Add(txt);
                }
                else if (!(model.Annotations[i] is PolygonAnnotation))
                    anns.Add(model.Annotations[i]);
            }
            for (int i = 0; i < model.Series.Count; ++i)
            {
                if (model.Series[i] is LineSeries)
                {
                    LineSeries ls = model.Series[i] as LineSeries;
                    if (ls.YAxisKey != model.Axes[4].Key || ls.Points[1].X < start_X || ls.Points[1].X > end_X)
                        sers.Add(ls);
                }
                else
                    sers.Add(model.Series[i]);
            }
            model.Annotations = anns;
            model.Series = sers;
        }
        //x表示鉴定到的肽段对应的同位素色谱曲线的强度，y表示对应的“对儿”的强度。
        public PlotModel display_ratio(List<double> x, List<double> y)
        {
            if (x.Max() == 0.0 || y.Max() == 0.0)
                return null;
            PlotModel model = new PlotModel();
            var linearAxis1 = new LinearAxis();
            linearAxis1.Maximum = y.Max();
            linearAxis1.Minimum = 0;
            linearAxis1.MajorStep = (linearAxis1.Maximum - linearAxis1.Minimum) / 4;
            linearAxis1.IntervalLength = 20;
            linearAxis1.Title = "Y (Blue)";
            linearAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis1.TitleFontSize = 15;
            linearAxis1.AxisTitleDistance = 5;
            linearAxis1.Minimum = 0.0;
            linearAxis1.IsPanEnabled = false;
            linearAxis1.IsZoomEnabled = false;
            model.Axes.Add(linearAxis1);

            var linearAxis2 = new LinearAxis();
            linearAxis2.Maximum = x.Max();
            linearAxis2.Minimum = 0;
            linearAxis2.MajorStep = (linearAxis2.Maximum - linearAxis2.Minimum) / 4;
            linearAxis2.IntervalLength = 20;
            linearAxis2.Title = "X (Red)";
            linearAxis2.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis2.TitleFontSize = 15;
            linearAxis2.AxisTitleDistance = 5;
            linearAxis2.Position = AxisPosition.Bottom;
            linearAxis2.IsPanEnabled = false;
            linearAxis2.IsZoomEnabled = false;
            model.Axes.Add(linearAxis2);

            ScatterSeries ss = new ScatterSeries();
            ss.MarkerSize = 1.8;
            ss.MarkerFill = OxyColors.DarkGreen;
            List<IDataPoint> points = new List<IDataPoint>();
            for (int i = 0; i < x.Count; ++i)
                points.Add(new DataPoint(x[i], y[i]));
            ss.Points = points;
            model.Series.Add(ss);

            double fz = 0.0, fm = 0.0, max = x.Max();
            for (int i = 0; i < x.Count; ++i)
            {
                if (x[i] != 0.0 && y[i] != 0.0)
                {
                    fz += x[i] * y[i];
                    fm += x[i] * x[i];
                }
            }
            double k = 0.0;
            if (fm != 0.0)
                k = fz / fm;

            LineSeries ls = new LineSeries();
            ls.Color = OxyColors.Red;
            points = new List<IDataPoint>();
            points.Add(new DataPoint(0.0, 0.0));
            points.Add(new DataPoint(max, max * k));
            ls.Points = points;
            model.Series.Add(ls);

            TextAnnotation txt = new TextAnnotation();
            txt.Position = new DataPoint(max * 0.98, max * k * 0.9);
            txt.VerticalAlignment = OxyPlot.VerticalAlignment.Top;
            txt.HorizontalAlignment = OxyPlot.HorizontalAlignment.Right;
            txt.Stroke = OxyColors.Transparent;
            txt.Text = "y=" + k.ToString("F2") + "x";
            txt.FontSize = 12;
            txt.FontWeight = OxyPlot.FontWeights.Bold;
            model.Annotations.Add(txt);

            model.Padding = new OxyThickness(0, 30, 30, 0);

            return model;
        }
        //在ms1的model中绘制一个对儿的色谱曲线,根据star_scan和end_scan起始扫描号和终止扫描号来计算定量比值
        public void display_chrom(List<List<PEAK_MS1>> mz, PlotModel model, int cur_scan, int start_scan, int end_scan
            , int index)
        {
            if (mz.Count == 1)
            {
                mz.Add(new List<PEAK_MS1>());
                mz[1] = new List<PEAK_MS1>(mz[0]);
            }
            if (mz[1].Count != 0)
            {
                double ratio = get_ratio(mz[0], mz[1], start_scan, end_scan);
            }
            LineSeries border = new LineSeries();
            border.YAxisKey = model.Axes[4].Key;
            border.Color = Display_Detail_Help.border_color;
            double start_X = model.Axes[1].ActualMaximum - 0.42 * (model.Axes[1].ActualMaximum - model.Axes[1].ActualMinimum);
            double end_X = model.Axes[1].ActualMaximum - 0.22 * (model.Axes[1].ActualMaximum - model.Axes[1].ActualMinimum);
            initial_model(model, start_X, end_X);
            List<IDataPoint> points = new List<IDataPoint>();
            points.Add(new DataPoint(start_X, 0));
            points.Add(new DataPoint(end_X, 0));
            points.Add(new DataPoint(end_X, 1));
            points.Add(new DataPoint(start_X, 1));
            points.Add(new DataPoint(start_X, 0));
            border.Points = points;
            model.Series.Add(border);

            //int scan_index = -1;
            //for (int i = 0; i < mz[0].Count; ++i)
            //{
            //    if (mz[0][i].scan == cur_scan)
            //    {
            //        scan_index = i;
            //        break;
            //    }
            //}

            //增加一条虚线
            LineSeries ls0 = new LineSeries();
            ls0.Color = OxyColors.Black;
            ls0.YAxisKey = model.Axes[4].Key;
            ls0.LineStyle = LineStyle.Dot;
            List<IDataPoint> points0 = new List<IDataPoint>();
            double ratioX = (end_X - start_X) / (end_scan - start_scan);
            double cur_X = start_X;
            if (cur_scan >= start_scan && cur_scan <= end_scan)
                cur_X = start_X + (cur_scan - start_scan) * ratioX;
            else if (cur_scan < start_scan)
                cur_X = start_X;
            else
                cur_X = end_X;
            points0.Add(new DataPoint(cur_X, 0));
            points0.Add(new DataPoint(cur_X, 1));
            ls0.Points = points0;
            model.Series.Add(ls0);

            TextAnnotation star_scan_txt = new TextAnnotation();
            star_scan_txt.TextColor = OxyColors.Black;
            star_scan_txt.Stroke = OxyColors.Transparent;
            star_scan_txt.FontSize = 10;
            star_scan_txt.HorizontalAlignment = OxyPlot.HorizontalAlignment.Left;
            star_scan_txt.YAxisKey = model.Axes[4].Key;
            star_scan_txt.Text = start_scan.ToString();
            star_scan_txt.Position = new DataPoint(start_X, 0);
            model.Annotations.Add(star_scan_txt);

            TextAnnotation end_scan_txt = new TextAnnotation();
            end_scan_txt.TextColor = OxyColors.Black;
            end_scan_txt.Stroke = OxyColors.Transparent;
            end_scan_txt.FontSize = 10;
            end_scan_txt.HorizontalAlignment = OxyPlot.HorizontalAlignment.Right;
            end_scan_txt.YAxisKey = model.Axes[4].Key;
            end_scan_txt.Text = end_scan.ToString();
            end_scan_txt.Position = new DataPoint(end_X, 0);
            model.Annotations.Add(end_scan_txt);

            TextAnnotation index_txt = new TextAnnotation();
            index_txt.TextColor = OxyColors.Black;
            index_txt.Stroke = OxyColors.Transparent;
            index_txt.FontSize = 12;
            index_txt.FontWeight = OxyPlot.FontWeights.Bold;
            index_txt.HorizontalAlignment = OxyPlot.HorizontalAlignment.Right;
            index_txt.VerticalAlignment = OxyPlot.VerticalAlignment.Top;
            index_txt.YAxisKey = model.Axes[4].Key;
            index_txt.Text = (index + 1).ToString();
            index_txt.Position = new DataPoint(end_X, 1);
            model.Annotations.Add(index_txt);
            
           
            double width_X1 = (end_X - start_X) * 0.08;
            double width_Y1 = 0.2;
            
            PolygonAnnotation start_button = new PolygonAnnotation();
            start_button.YAxisKey = model.Axes[4].Key;
            start_button.Text = "S";
            start_button.FontSize = 10;
            List<IDataPoint> points1 = new List<IDataPoint>();
            points1.Add(new DataPoint(start_X, 1 - width_Y1));
            points1.Add(new DataPoint(start_X, 1));
            points1.Add(new DataPoint(start_X + width_X1, 1));
            points1.Add(new DataPoint(start_X + width_X1, 1 - width_Y1));
            start_button.Points = points1;
            start_button.MouseDown += (s, e) =>
            {
                if (e.ChangedButton != OxyMouseButton.Left)
                    return;
                if (!mainW.is_display_animation)
                    return;
                if (this.mainW.timer.IsEnabled)
                    this.mainW.timer.Stop();
                else
                    this.mainW.timer.Start();
            };
            model.Annotations.Add(start_button);
            PolygonAnnotation initial_button = new PolygonAnnotation();
            initial_button.YAxisKey = model.Axes[4].Key;
            initial_button.Text = "I";
            initial_button.FontSize = 10;
            List<IDataPoint> points2 = new List<IDataPoint>();
            //start_X += width_X1;
            points2.Add(new DataPoint(start_X + width_X1, 1 - width_Y1));
            points2.Add(new DataPoint(start_X + width_X1, 1));
            points2.Add(new DataPoint(start_X + 2 * width_X1, 1));
            points2.Add(new DataPoint(start_X + 2 * width_X1, 1 - width_Y1));
            initial_button.Points = points2;
            initial_button.MouseDown += (s, e) =>
            {
                if (e.ChangedButton != OxyMouseButton.Left)
                    return;
                if (!mainW.is_display_animation)
                    return;
                mainW.Chrome_help.index = -1;
                mainW.timer.Stop();
                mainW.update_ms1_masserror(false);
            };
            model.Annotations.Add(initial_button);

            PolygonAnnotation export_button = new PolygonAnnotation();
            export_button.YAxisKey = model.Axes[4].Key;
            export_button.Text = "E";
            export_button.FontSize = 10;
            List<IDataPoint> points3 = new List<IDataPoint>();
            //start_X += width_X1;
            points3.Add(new DataPoint(start_X + 2 * width_X1, 1 - width_Y1));
            points3.Add(new DataPoint(start_X + 2 * width_X1, 1));
            points3.Add(new DataPoint(start_X + 3 * width_X1, 1));
            points3.Add(new DataPoint(start_X + 3 * width_X1, 1 - width_Y1));
            export_button.Points = points3;
            export_button.MouseDown += (s, e) =>
            {
                if (e.ChangedButton != OxyMouseButton.Left)
                {
                    return;
                }
                List<double> x = new List<double>();
                List<double> y = new List<double>();
                if (mainW.is_display_animation)
                {
                    for (int i = 0; i < mainW.Chrome_help.all_index; ++i)
                    {
                        List<List<PEAK_MS1>> mz_1 = new List<List<PEAK_MS1>>();
                        for (int p = 0; p < 2; ++p)
                            mz_1.Add(new List<PEAK_MS1>());
                        mainW.update_ms1_masserror(mainW.Chrome_help.theory_isotopes, i, ref mz_1, false);
                        for (int j = 0; j < mz_1[0].Count; ++j)
                            x.Add(mz_1[0][j].intensity);
                        for (int j = 0; j < mz_1[1].Count; ++j)
                            y.Add(mz_1[1][j].intensity);
                    }
                }
                else
                {
                    for (int i = 0; i < mz[0].Count; ++i)
                        x.Add(mz[0][i].intensity);
                    for (int i = 0; i < mz[1].Count; ++i)
                        y.Add(mz[1][i].intensity);
                }
                if (x.Count != y.Count)
                {
                    if (x.Count < y.Count)
                    {
                        for (int i = x.Count; i < y.Count; ++i)
                            y.Remove(y[i]);
                    }
                    else
                    {
                        for (int i = y.Count; i < x.Count; ++i)
                            x.Remove(x[i]);
                    }
                }
                //没有跑定量就没有比值就不用绘制比值信息
                if (mainW.Chrome_help.theory_isotopes.Count > 1)
                {
                    PlotModel ratio_model = display_ratio(x, y);
                    if (ratio_model != null)
                    {
                        Model_Window mw = new Model_Window(ratio_model, "Ratio");
                        mw.Show();
                    }
                    else
                    {
                        MessageBox.Show(Message_Help.NO_RATIO);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show(Message_Help.NO_RATIO);
                }
            };
            model.Annotations.Add(export_button);
            PolygonAnnotation XIC_button = new PolygonAnnotation();
            XIC_button.YAxisKey = model.Axes[4].Key;
            XIC_button.Text = "X";
            XIC_button.FontSize = 10;
            List<IDataPoint> points4 = new List<IDataPoint>();
            //start_X += width_X1;
            points4.Add(new DataPoint(start_X + 3 * width_X1, 1 - width_Y1));
            points4.Add(new DataPoint(start_X + 3 * width_X1, 1));
            points4.Add(new DataPoint(start_X + 4 * width_X1, 1));
            points4.Add(new DataPoint(start_X + 4 * width_X1, 1 - width_Y1));
            XIC_button.Points = points4;
            XIC_button.MouseDown += (s, e) =>
            {
                if (e.ChangedButton != OxyMouseButton.Left)
                    return;
                mainW.pop_up_chrom();
            };
            model.Annotations.Add(XIC_button);

            double maxIntensity = double.MinValue;
            for (int i = 0; i < mz.Count; ++i)
            {
                for (int j = 0; j < mz[i].Count; ++j)
                {
                    if (mz[i][j].intensity > maxIntensity)
                        maxIntensity = mz[i][j].intensity;
                }
            }
            TextAnnotation txt = new TextAnnotation();
            txt.StrokeThickness = 0;
            txt.HorizontalAlignment = OxyPlot.HorizontalAlignment.Left;
            txt.YAxisKey = model.Axes[4].Key;
            txt.Text = maxIntensity.ToString("E2");
            txt.Position = new DataPoint(start_X, 1 - 2 * width_Y1);
            model.Annotations.Add(txt);

            for (int i = 0; i < mz.Count; ++i)
            {
                if (mz[i].Count > 0)
                    add_One_LineSeries(mz[i], i, model, start_X, end_X, 0, 1, get_color(i), maxIntensity);
            }
            
            double xy = 0, x_1 = 0, y_1 = 0, x_2 = 0, y_2 = 0;
            int count = (mz[0].Count() < mz[1].Count() ? mz[0].Count : mz[1].Count());
            for (int i = 0; i < count; ++i)
            {
                xy += mz[0][i].intensity * mz[1][i].intensity;
                x_1 += mz[0][i].intensity;
                y_1 += mz[1][i].intensity;
                x_2 += mz[0][i].intensity * mz[0][i].intensity;
                y_2 += mz[1][i].intensity * mz[1][i].intensity;
            }
            double fm = (Math.Sqrt(count * x_2 - x_1 * x_1) * Math.Sqrt(count * y_2 - y_1 * y_1));
            double ratio_0 = 0.0;
            if(fm != 0.0)
                ratio_0 = (count * xy - x_1 * y_1) / fm; //Pearson相关系数
            TextAnnotation ratio_txt = new TextAnnotation();
            //if (ratio_0 >= 0.9 && ratio_0 <= 1.1)
            //    ratio_txt.TextColor = OxyColors.Red;
            //else if (ratio_0 >= 0.7 && ratio_0 <= 1.3)
            //    ratio_txt.TextColor = OxyColors.Gold;
            //else if (ratio_0 >= 0.6 && ratio_0 <= 1.4)
            //    ratio_txt.TextColor = OxyColors.Lime;
            //else if (ratio_0 >= 0.4 && ratio_0 <= 2)
            //    ratio_txt.TextColor = OxyColors.SkyBlue;
            //else
            //    ratio_txt.TextColor = OxyColors.DarkBlue;
            ratio_txt.TextColor = OxyColors.Red;
            ratio_txt.Stroke = OxyColors.Transparent;
            ratio_txt.FontWeight = OxyPlot.FontWeights.Bold;
            ratio_txt.FontSize = 12;
            ratio_txt.HorizontalAlignment = OxyPlot.HorizontalAlignment.Left;
            ratio_txt.YAxisKey = model.Axes[4].Key;
            ratio_txt.Text = ratio_0.ToString("F2");
            ratio_txt.Position = new DataPoint(start_X, 0.3);
            model.Annotations.Add(ratio_txt);
            model.RefreshPlot(true);
        }
        public DataPoint InverseTransform(double X, double Y, LinearAxis xAxis, LinearAxis yAxis)
        {
            return new DataPoint(xAxis.InverseTransform(X), yAxis.InverseTransform(Y));
        }
        
        private OxyColor get_color(int index)
        {
            OxyColor[] all_colors = new OxyColor[] {OxyColors.Purple, OxyColors.RosyBrown,OxyColors.RoyalBlue,OxyColors.SaddleBrown,
            OxyColors.Salmon, OxyColors.SandyBrown, OxyColors.SeaGreen, OxyColors.SeaShell, OxyColors.Sienna, OxyColors.Silver};
            return all_colors[index % all_colors.Length];
        }
        public static bool IsEqual(OxyColor color1, OxyColor color2)
        {
            if (color1.A == color2.A && color1.R == color2.R && color1.G == color2.G && color1.B == color2.B)
                return true;
            return false;
        }
        public static OxyColor get_color2(int index, int index1)
        {
            OxyColor[] all_colors = new OxyColor[] {OxyColors.DarkSlateGray,OxyColors.DarkGreen,
            OxyColors.BlueViolet, OxyColors.DimGray,OxyColors.MediumSeaGreen,OxyColors.DarkMagenta,OxyColors.Gray,
            OxyColors.LimeGreen,OxyColors.MediumVioletRed,OxyColors.LightSlateGray,OxyColors.Lime,OxyColors.PaleVioletRed,
            OxyColors.SlateGray,OxyColors.PaleGreen,OxyColors.Violet
            }; //15种颜色
            //15种颜色，用来混合谱的显示，对于混合谱来说，M:i*3+0,a|b|c:i*3+1,x|y|z:i*3+2，i表示第几个肽段的匹配
            if (index >= 4)
                index = 4;
            return all_colors[index * 3 + index1];
        }
        public static Color get_brush_color(int index)
        {
            Color[] all_colors = new Color[] {Colors.Brown,Colors.BurlyWood,
            Colors.CadetBlue, Colors.LightSeaGreen,Colors.Chocolate,Colors.Coral,Colors.CornflowerBlue,
            Colors.Crimson,Colors.PaleVioletRed,Colors.DarkBlue,Colors.DarkCyan,Colors.DarkGoldenrod,
            Colors.DarkGray,Colors.DarkGreen,Colors.DarkKhaki,Colors.DarkMagenta,Colors.DarkOliveGreen,
            Colors.DarkOrange,Colors.DarkOrchid,Colors.DarkRed,Colors.DarkSalmon,Colors.DarkSeaGreen,
            Colors.DarkSlateBlue,Colors.DarkSlateGray,Colors.DarkTurquoise,Colors.DarkViolet,Colors.DeepPink,
            Colors.DeepSkyBlue,Colors.DimGray,Colors.DodgerBlue
            }; //20种颜色
            //20种颜色，用来绘制蛋白的修饰
            return all_colors[index % all_colors.Length];
        }

        private void mass_error_draw_speedUp(PlotModel model, string Da_PPM, bool is_speedUp = true)
        {
            double one_width = (model.Axes[1].ActualMaximum - model.Axes[1].ActualMinimum) / (model.Axes[1].ScreenMax.X - model.Axes[1].ScreenMin.X);
            double one_height = (model.Axes[0].ActualMaximum - model.Axes[0].ActualMinimum) / (model.Axes[0].ScreenMax.Y - model.Axes[0].ScreenMin.Y);

            List<List<int>> all_psm_indexs_target = new List<List<int>>();
            List<List<int>> all_psm_indexs_decoy = new List<List<int>>();
            ScatterSeries series = new ScatterSeries();
            if (Da_PPM == "Da")
                series.TrackerFormatString = "Mass Deviation: {2:0.000} Da\n-log(score): {4:0.00}";
            else
                series.TrackerFormatString = "Mass Deviation: {2:0.00} ppm\n-log(score): {4:0.00}";
            series.MarkerFill = OxyColors.Blue;
            series.MarkerSize = 1;
            for (int i = 0; i < Target_points.Count; ++i)
            {
                series.Points.Add(Target_points[i]);
            }
            if (is_speedUp)
            {
                int X_num = (int)((model.Axes[1].ScreenMax.X - model.Axes[1].ScreenMin.X)) + 1;
                int Y_num = (int)(model.Axes[0].ScreenMax.Y - model.Axes[0].ScreenMin.Y) + 1;
                List<IDataPoint>[,] all_points_bin = new List<IDataPoint>[X_num, Y_num];
                List<int>[,] all_psm_indexs_tmp = new List<int>[X_num, Y_num];
                for (int i = 0; i < X_num; ++i)
                {
                    for (int j = 0; j < Y_num; ++j)
                    {
                        all_points_bin[i, j] = new List<IDataPoint>();
                        all_psm_indexs_tmp[i, j] = new List<int>();
                    }
                }

                for (int i = 0; i < series.Points.Count; ++i)
                {
                    int cur_x_index = (int)((series.Points[i].X - model.Axes[1].ActualMinimum) / one_width);
                    int cur_y_index = (int)((series.Points[i].Y - model.Axes[0].ActualMinimum) / one_height);
                    if (cur_x_index >= 0 && cur_x_index < X_num && cur_y_index >= 0 && cur_y_index < Y_num)
                    {
                        if (all_points_bin[cur_x_index, cur_y_index].Count == 0)
                            all_points_bin[cur_x_index, cur_y_index].Add(series.Points[i]);
                        all_psm_indexs_tmp[cur_x_index, cur_y_index].Add(i);
                    }
                }
                series.Points.Clear();
                for (int i = 0; i < X_num; ++i)
                {
                    for (int j = 0; j < Y_num; ++j)
                    {
                        if (all_points_bin[i, j].Count > 0)
                        {
                            series.Points.Add(all_points_bin[i, j][0]);
                            all_psm_indexs_target.Add(all_psm_indexs_tmp[i, j]);
                        }
                    }
                }
                model.Series.Clear();
            }
            series.MouseDown += (s, e) =>
            {
                if (e.ChangedButton != OxyMouseButton.Right)
                    return;
                int index = (int)Math.Round(e.HitTestResult.Index);
                if (index < 0 || index >= all_psm_indexs_target.Count)
                    return;
                List<int> psm_indexs_tmp = all_psm_indexs_target[index];
                List<int> psm_indexs = new List<int>();
                for (int i = 0; i < psm_indexs_tmp.Count; ++i)
                {
                    psm_indexs.Add(Target_psm_indexs[psm_indexs_tmp[i]]);
                }
                ObservableCollection<PSM> psms_tmp = new ObservableCollection<PSM>();
                for (int i = 0; i < psm_indexs.Count; ++i)
                {
                    psms_tmp.Add(mainW.psms[psm_indexs[i]]);
                }
                mainW.display_psms = psms_tmp;
                mainW.data.ItemsSource = mainW.display_psms;
                mainW.display_size.Text = mainW.display_psms.Count().ToString();
                mainW.summary_tab.SelectedIndex = 1;
            };
            model.Series.Add(series);

            series = new ScatterSeries();
            if (Da_PPM == "Da")
                series.TrackerFormatString = "Mass Deviation: {2:0.000} Da\n-log(score): {4:0.00}";
            else
                series.TrackerFormatString = "Mass Deviation: {2:0.00} ppm\n-log(score): {4:0.00}";
            series.MarkerFill = OxyColors.Red;
            series.MarkerSize = 1;
            for (int i = 0; i < Decoy_points.Count; ++i)
            {
                series.Points.Add(Decoy_points[i]);
            }
            if (is_speedUp)
            {
                int X_num = (int)((model.Axes[1].ScreenMax.X - model.Axes[1].ScreenMin.X)) + 1;
                int Y_num = (int)(model.Axes[0].ScreenMax.Y - model.Axes[0].ScreenMin.Y) + 1;
                List<IDataPoint>[,] all_points_bin = new List<IDataPoint>[X_num, Y_num];
                List<int>[,] all_psm_indexs_tmp = new List<int>[X_num, Y_num];
                for (int i = 0; i < X_num; ++i)
                {
                    for (int j = 0; j < Y_num; ++j)
                    {
                        all_points_bin[i, j] = new List<IDataPoint>();
                        all_psm_indexs_tmp[i, j] = new List<int>();
                    }
                }

                for (int i = 0; i < series.Points.Count; ++i)
                {
                    int cur_x_index = (int)((series.Points[i].X - model.Axes[1].ActualMinimum) / one_width);
                    int cur_y_index = (int)((series.Points[i].Y - model.Axes[0].ActualMinimum) / one_height);
                    if (cur_x_index >= 0 && cur_x_index < X_num && cur_y_index >= 0 && cur_y_index < Y_num)
                    {
                        if (all_points_bin[cur_x_index, cur_y_index].Count == 0)
                            all_points_bin[cur_x_index, cur_y_index].Add(series.Points[i]);
                        all_psm_indexs_tmp[cur_x_index, cur_y_index].Add(i);
                    }
                }
                series.Points.Clear();
                for (int i = 0; i < X_num; ++i)
                {
                    for (int j = 0; j < Y_num; ++j)
                    {
                        if (all_points_bin[i, j].Count > 0)
                        {
                            series.Points.Add(all_points_bin[i, j][0]);
                            all_psm_indexs_decoy.Add(all_psm_indexs_tmp[i, j]);
                        }
                    }
                }
            }
            series.MouseDown += (s, e) =>
            {
                if (e.ChangedButton != OxyMouseButton.Right)
                    return;
                int index = (int)Math.Round(e.HitTestResult.Index);
                if (index < 0 || index >= all_psm_indexs_decoy.Count)
                    return;
                List<int> psm_indexs_tmp = all_psm_indexs_decoy[index];
                List<int> psm_indexs = new List<int>();
                for (int i = 0; i < psm_indexs_tmp.Count; ++i)
                {
                    psm_indexs.Add(Decoy_psm_indexs[psm_indexs_tmp[i]]);
                }
                ObservableCollection<PSM> psms_tmp = new ObservableCollection<PSM>();
                for (int i = 0; i < psm_indexs.Count; ++i)
                {
                    psms_tmp.Add(mainW.psms[psm_indexs[i]]);
                }
                mainW.display_psms = psms_tmp;
                mainW.data.ItemsSource = mainW.display_psms;
                mainW.display_size.Text = mainW.display_psms.Count().ToString();
                mainW.summary_tab.SelectedIndex = 1;
            };
            model.Series.Add(series);
        }
    }

    public class Protein_Display_Detail_Help
    {
        public int alphabet_num_per_row; //每一行显示多少个蛋白字母

        public Protein_Display_Detail_Help()
        {
            alphabet_num_per_row = 60;
        }
    }
    public class Display_Mass_Error_Points
    {
        public List<IDataPoint> target_points_Da, decoy_points_Da, target_points_ppm, decoy_points_ppm;
    }
}
