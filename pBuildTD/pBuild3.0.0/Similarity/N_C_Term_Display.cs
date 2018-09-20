using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace pBuild.Similarity
{
    public class N_C_Term_Display
    {
        public MainWindow MainW;
        public Hashtable Psm_hash;
        public List<N_C_Term_Intensity> Ncs;
        public List<string> Titles;
        public List<List<double>> Scores;
        public double Min, Max;
        public double Width;
        public N_C_Term_Display(MainWindow mainW)
        {
            this.MainW = mainW;
        }
        public N_C_Term_Display(MainWindow mainW, Hashtable psm_hash, List<N_C_Term_Intensity> ncs,
            List<string> titles, List<List<double>> scores)
        {
            this.MainW = mainW;
            this.Psm_hash = psm_hash;
            this.Ncs = ncs;
            this.Titles = titles;
            this.Scores = scores;
            this.Min = 0;
            this.Max = 1;
            this.Width = (this.Max - this.Min) / titles.Count;
        }
        //public PlotModel display_heatMap()
        //{
        //    var model = new PlotModel();
        //    model.Title = "Cos Similarity";
        //    var linearAxis1 = new LinearAxis();
        //    linearAxis1.Position = AxisPosition.Bottom;
        //    model.Axes.Add(linearAxis1);
        //    var linearAxis2 = new LinearAxis();
        //    model.Axes.Add(linearAxis2);
        //    return model;
        //}
        public PlotModel display_heatMap()
        {
            var model = new PlotModel();
            model.Title = "Cos Similarity";
            var linearColorAxis1 = new LinearColorAxis();
            linearColorAxis1.HighColor = OxyColors.Gray;
            linearColorAxis1.LowColor = OxyColors.Black;
            linearColorAxis1.Position = AxisPosition.Right;
            model.Axes.Add(linearColorAxis1);
            var linearAxis1 = new LinearAxis();
            linearAxis1.Position = AxisPosition.Bottom;
            linearAxis1.Minimum = Min;
            linearAxis1.Maximum = Max;
            model.Axes.Add(linearAxis1);
            var linearAxis2 = new LinearAxis();
            linearAxis2.Minimum = Min;
            linearAxis2.Maximum = Max;
            model.Axes.Add(linearAxis2);
            var heatMapSeries1 = new HeatMapSeries();
            heatMapSeries1.X0 = 0;
            heatMapSeries1.X1 = 1.0;
            heatMapSeries1.Y0 = 0;
            heatMapSeries1.Y1 = 1.0;
            heatMapSeries1.Interpolate = false;
            heatMapSeries1.Data = new Double[Scores.Count, Scores.Count];
            for (int i = 0; i < Scores.Count; ++i)
            {
                for (int j = 0; j < Scores.Count; ++j)
                {
                    heatMapSeries1.Data[i, j] = Scores[i][j];
                }
            }
            model.Series.Add(heatMapSeries1);
            heatMapSeries1.MouseDown += (s, e) =>
            {
                DataPoint point = model.Axes[1].InverseTransform(e.Position.X, e.Position.Y, model.Axes[2]);
                int x_index = (int)((point.X - this.Min) / this.Width);
                int y_index = (int)((point.Y - this.Min) / this.Width);
                if (x_index < 0 || x_index >= Ncs.Count || y_index < 0 || y_index >= Ncs.Count)
                    return;
                string title1 = Ncs[x_index].get_title();
                string title2 = Ncs[y_index].get_title();
                int psm1_index = (int)Psm_hash[title1];
                int psm2_index = (int)Psm_hash[title2];
                PSM psm1 = MainW.psms[psm1_index];
                PSM psm2 = MainW.psms[psm2_index];
                PlotModel model0 = display_TwoMS2(psm1, psm2);
                Model_Window mw = new Model_Window(model0, psm2.Title + "-" + psm1.Title);
                mw.Show();
            };
            return model;
        }
        public PlotModel display_TwoMS2(PSM psm1, PSM psm2)
        {
            Spectra spec1 = null, spec2 = null;
            Peptide pep1 = null, pep2 = null;
            MainW.parse_PSM(psm1, ref spec1, ref pep1);
            MainW.parse_PSM(psm2, ref spec2, ref pep2);
            
            PlotModel model = new PlotModel();
            LinearAxis x_axis = new LinearAxis();
            x_axis.Position = AxisPosition.Bottom;
            x_axis.Maximum = (spec1.Peaks.Last().Mass > spec2.Peaks.Last().Mass ? spec1.Peaks.Last().Mass : spec2.Peaks.Last().Mass) + 50;
            x_axis.Minimum = (spec1.Peaks[0].Mass < spec2.Peaks[0].Mass ? spec1.Peaks[0].Mass : spec2.Peaks[0].Mass) - 50;
            x_axis.AbsoluteMaximum = x_axis.Maximum;
            x_axis.AbsoluteMinimum = x_axis.Minimum;
            x_axis.PositionAtZeroCrossing = true;
            x_axis.TickStyle = TickStyle.Crossing;
            x_axis.IsAxisVisible = false;
            model.Axes.Add(x_axis);
            LinearAxis y_axis = new LinearAxis();
            y_axis.Minimum = -100;
            y_axis.Maximum = 100;
            y_axis.IsPanEnabled = false;
            y_axis.IsZoomEnabled = false;
            y_axis.Title = psm2.Title + "-" + psm1.Title;
            model.Axes.Add(y_axis);
            LineSeries ls = new LineSeries();
            ls.Color = OxyColors.Black;
            ls.LineStyle = LineStyle.Solid;
            ls.Points.Add(new DataPoint(x_axis.Minimum, 0));
            ls.Points.Add(new DataPoint(x_axis.Maximum, 0));
            model.Series.Add(ls);

            MainW.psm_help.Switch_PSM_Help(spec1, pep1);
            MainW.psm_help.Pep.Tag_Flag = int.Parse(psm1.Pep_flag);
            MainW.psm_help.Match_BY();
            update_peaks(spec1, ref model);
            MainW.psm_help.Switch_PSM_Help(spec2, pep2);
            MainW.psm_help.Pep.Tag_Flag = int.Parse(psm2.Pep_flag);
            MainW.psm_help.Match_BY();
            for (int i = 0; i < spec2.Peaks.Count; ++i)
                spec2.Peaks[i].Intensity = -spec2.Peaks[i].Intensity;
            update_peaks(spec2, ref model);
            return model;
        }
        private void update_peaks(Spectra spec1, ref PlotModel model)
        {
            for (int i = 0; i < spec1.Peaks.Count; ++i)
            {
                LineSeries Ls0 = new LineSeries();
                Ls0.TrackerFormatString = "m/z: {2:0.000}\nIntensity: {4:0.00}%";
                Ls0.Color = OxyColors.Black;
                Ls0.LineStyle = LineStyle.Solid;
                Ls0.StrokeThickness = MainW.Dis_help.ddh.NoNMatch_StrokeWidth;
                Ls0.Points.Add(new DataPoint(spec1.Peaks[i].Mass, 0));
                Ls0.Points.Add(new DataPoint(spec1.Peaks[i].Mass, spec1.Peaks[i].Intensity));
                model.Series.Add(Ls0);
                if (MainW.psm_help.Psm_detail.BOry[i] != 0)
                {
                    LineSeries Ls = new LineSeries();
                    Ls.TrackerFormatString = "m/z: {2:0.000}\nIntensity: {4:0.00}%";
                    Ls.Color = OxyColors.White;
                    Ls.LineStyle = LineStyle.Solid;
                    Ls.StrokeThickness = MainW.Dis_help.ddh.Match_StrokeWidth;
                    Ls.Points.Add(new DataPoint(spec1.Peaks[i].Mass, 0));
                    Ls.Points.Add(new DataPoint(spec1.Peaks[i].Mass, spec1.Peaks[i].Intensity));
                    model.Series.Add(Ls);
                    var lineAnnotation = new LineAnnotation();
                    lineAnnotation.Type = LineAnnotationType.Vertical;
                    lineAnnotation.StrokeThickness = MainW.Dis_help.ddh.Match_StrokeWidth;
                    lineAnnotation.TextVerticalAlignment = OxyPlot.VerticalAlignment.Top;
                    if (spec1.Peaks[i].Intensity > 0.0)
                    {
                        if (spec1.Peaks[i].Intensity >= 50)
                            lineAnnotation.TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Right;
                        else
                            lineAnnotation.TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Left;
                    }
                    else
                    {
                        if (spec1.Peaks[i].Intensity <= -50)
                            lineAnnotation.TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Left;
                        else
                            lineAnnotation.TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Right;
                    }
                    lineAnnotation.X = spec1.Peaks[i].Mass;
                    if (spec1.Peaks[i].Intensity > 0)
                    {
                        lineAnnotation.MinimumY = 0;
                        lineAnnotation.MaximumY = spec1.Peaks[i].Intensity;
                        if (lineAnnotation.MaximumY <= 1)
                            lineAnnotation.MaximumY = 1;
                    }
                    else
                    {
                        lineAnnotation.MaximumY = 0;
                        lineAnnotation.MinimumY = spec1.Peaks[i].Intensity;
                        if (lineAnnotation.MinimumY >= -1)
                            lineAnnotation.MinimumY = -1;
                    }
                    lineAnnotation.LineStyle = LineStyle.Solid;
                    for (int p = 0; p < MainW.psm_help.Psm_detail.By_num[i].Count; ++p)
                        lineAnnotation.Text += MainW.psm_help.Psm_detail.By_num[i][p];
                    if (spec1.Peaks[i].Intensity > 0.0)
                        lineAnnotation.TextPosition = 1.0;
                    else
                        lineAnnotation.TextPosition = 0.0;
                    lineAnnotation.TextMargin = 1;
                    if ((MainW.psm_help.Psm_detail.BOry[i] & 1) == 1)
                    {
                        if (MainW.psm_help.Psm_detail.By_num[i][0][0] == 'a')
                        {
                            lineAnnotation.Color = MainW.Dis_help.ddh.A_Match_Color;
                            lineAnnotation.TextColor = MainW.Dis_help.ddh.A_Match_Color;
                        }
                        else
                        {
                            lineAnnotation.Color = MainW.Dis_help.ddh.B_Match_Color;
                            lineAnnotation.TextColor = MainW.Dis_help.ddh.B_Match_Color;
                        }
                    }
                    else if ((MainW.psm_help.Psm_detail.BOry[i] & 2) == 2)
                    {
                        lineAnnotation.Color = MainW.Dis_help.ddh.Y_Match_Color;
                        lineAnnotation.TextColor = MainW.Dis_help.ddh.Y_Match_Color;
                    }
                    model.Annotations.Add(lineAnnotation);
                }
            }
        }
    }
}
