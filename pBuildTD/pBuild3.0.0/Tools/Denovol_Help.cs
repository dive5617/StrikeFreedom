using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class Denovol_Help
    {
        public PlotModel Model;
        public PSM_Help_Parent Psm_help;
        public MS2_Help Ms2_help;

        public MS2_Help_Denovol MS2_help_denovol = new MS2_Help_Denovol();
        public MS2_Help_Denovol_pNovol MS2_help_denovol_pNovo = new MS2_Help_Denovol_pNovol();

        public List<MS2_Help_Denovol> MS2_help_denovol_history = new List<MS2_Help_Denovol>();
        public List<LineSeries> b_series = new List<LineSeries>();
        public List<LineSeries> y_series = new List<LineSeries>();

        public bool isDenovol = false, isBack = false, isInitial = false; //是否是denovo显示二级谱，如果是，则支持用户手动点击进行人工标注。

        public List<LineSeries> all_dot_ls = new List<LineSeries>();
        public System.Collections.Hashtable double_to_index = new System.Collections.Hashtable();

        public const double denovol_Y_Max = 115;
        public const double arrow_Y = 100, arrow_Y_Delta = 7;
        public double arrow_Y_add = 50, arrow_Y_minus = 50;

        public OxyColor b_color = OxyColors.Green, y_color = OxyColors.Red, default_color = OxyColors.RosyBrown;
        public OxyColor current_color;

        public Denovol_Help(PlotModel model, PSM_Help_Parent psm_help, MS2_Help ms2_help)
        {
            this.Model = model;
            this.Psm_help = psm_help;
            this.Ms2_help = ms2_help;

            current_color = default_color;
        }

        public void initial_series()
        {
            MS2_help_denovol.all_series.Clear();
            if (Psm_help.Spec.Peaks == null || this.Model == null)
                return;
            for (int i = 0; i < Psm_help.Spec.Peaks.Count; ++i)
            {
                LineSeries new_ls = new LineSeries();
                List<IDataPoint> points = new List<IDataPoint>();
                points.Add(new DataPoint(Psm_help.Spec.Peaks[i].Mass, 0));
                points.Add(new DataPoint(Psm_help.Spec.Peaks[i].Mass, denovol_Y_Max));
                new_ls.Points = points;
                new_ls.LineStyle = LineStyle.Dot;
                new_ls.Color = OxyColors.Transparent;
                new_ls.YAxisKey = this.Model.Axes[0].Key;
                MS2_help_denovol.all_series.Add(new_ls);
            }
        }

        public void initial_all()
        {
            if (isDenovol)
            {
                this.Model.Axes[0].Maximum = denovol_Y_Max;
                refresh_series_click();
                refresh_all();
            }
        }

        private void copy_denovol_history()
        {
            if (this.MS2_help_denovol_history.Count > 0)
            {
                MS2_Help_Denovol mhd = this.MS2_help_denovol_history.Last();
                if (mhd.all_annotation.Count == this.MS2_help_denovol.all_annotation.Count)
                    return;
            }
            this.MS2_help_denovol_history.Add(new MS2_Help_Denovol(this.MS2_help_denovol));
        }
        //更新所有的Series和Annotation
        private void refresh_all()
        {
            if (!isBack)
                copy_denovol_history();
            for (int i = 0; i < this.MS2_help_denovol.all_series.Count; ++i)
                ((LineSeries)this.MS2_help_denovol.all_series[i]).Color = OxyColors.Transparent;
            for (int i = 0; i < this.MS2_help_denovol.all_series_index2.Count; ++i)
            {
                ((LineSeries)this.MS2_help_denovol.all_series[this.MS2_help_denovol.all_series_index2[i]]).LineStyle = LineStyle.Dash;
                ((LineSeries)this.MS2_help_denovol.all_series[this.MS2_help_denovol.all_series_index2[i]]).StrokeThickness = 0.7;
                ((LineSeries)this.MS2_help_denovol.all_series[this.MS2_help_denovol.all_series_index2[i]]).Color = OxyColor.FromArgb(120, current_color.R, current_color.G, current_color.B);
            }
            for (int i = 0; i < this.MS2_help_denovol.all_series_index.Count; ++i)
            {
                ((LineSeries)this.MS2_help_denovol.all_series[this.MS2_help_denovol.all_series_index[i]]).LineStyle = LineStyle.Dot;
                ((LineSeries)this.MS2_help_denovol.all_series[this.MS2_help_denovol.all_series_index[i]]).StrokeThickness = 2.0;
                ((LineSeries)this.MS2_help_denovol.all_series[this.MS2_help_denovol.all_series_index[i]]).Color = current_color;
            }
            for (int i = 0; i < this.MS2_help_denovol.all_series.Count; ++i)
                this.Model.Series.Add(this.MS2_help_denovol.all_series[i]);
            for (int i = 0; i < this.MS2_help_denovol.all_annotation.Count; ++i)
            {
                if (!isInitial)
                {
                    if (this.MS2_help_denovol.all_annotation[i] is ArrowAnnotation)
                        ((ArrowAnnotation)this.MS2_help_denovol.all_annotation[i]).Color = current_color;
                    else
                        ((TextAnnotation)this.MS2_help_denovol.all_annotation[i]).TextColor = current_color;
                }
                this.Model.Annotations.Add(this.MS2_help_denovol.all_annotation[i]);
            }
            for (int i = 0; i < this.MS2_help_denovol.arrow_annotation.Count; ++i)
            {
                if (!isInitial)
                {
                    if (this.MS2_help_denovol.arrow_annotation[i] is ArrowAnnotation)
                        ((ArrowAnnotation)this.MS2_help_denovol.arrow_annotation[i]).Color = current_color;
                    else
                        ((TextAnnotation)this.MS2_help_denovol.arrow_annotation[i]).TextColor = current_color;
                }
                this.Model.Annotations.Add(this.MS2_help_denovol.arrow_annotation[i]);
            }
            for (int i = 0; i < this.MS2_help_denovol_pNovo.ArrowAnnotation.Count; ++i)
                this.Model.Annotations.Add(this.MS2_help_denovol_pNovo.ArrowAnnotation[i]);
            for (int i = 0; i < this.MS2_help_denovol_pNovo.TxtAnnotation.Count; ++i)
                this.Model.Annotations.Add(this.MS2_help_denovol_pNovo.TxtAnnotation[i]);
            for (int i = 0; i < this.MS2_help_denovol_pNovo.LineSeries.Count; ++i)
                this.Model.Series.Add(this.MS2_help_denovol_pNovo.LineSeries[i]);
            for (int i = 0; i < this.MS2_help_denovol_pNovo.ScatterSeries.Count; ++i)
                this.Model.Series.Add(this.MS2_help_denovol_pNovo.ScatterSeries[i]);
            this.Model.RefreshPlot(false);
            if (isInitial)
                isInitial = false;
        }
        public void refresh_initial()
        {
            refresh_clear();
            this.isInitial = true;
            List<Denovol_Config.DCC> b_mass = new List<Denovol_Config.DCC>();
            List<Denovol_Config.DCC> y_mass = new List<Denovol_Config.DCC>();
            get_all_mass(ref b_mass, ref y_mass);
            int[] mass_inten = new int[Config_Help.MaxMass];
            for (int k = 0; k < Psm_help.Spec.Peaks.Count; ++k)
            {
                int massi = (int)Psm_help.Spec.Peaks[k].Mass;
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
            this.MS2_help_denovol.all_series_index2.Clear();
            List<double> scores = new List<double>();
            for (int i = 0; i < b_mass.Count; ++i)
            {
                int index = -1;
                double me = 0.0;
                if (this.Psm_help.Ppm_mass_error != 0)
                    index = this.Psm_help.IsInWithPPM(b_mass[i].Mass, mass_inten, ref me);
                else
                    index = this.Psm_help.IsInWithDa(b_mass[i].Mass, mass_inten, ref me);
                if (index != -1)
                {
                    this.MS2_help_denovol.all_series_index2.Add(index);
                    scores.Add(add_arrow(this.Model.Axes[1].Minimum, this.Psm_help.Spec.Peaks[index].Mass, -1, index, me, b_mass[i].Name, b_color));
                    this.MS2_help_denovol.arrow_mass.Add(this.Psm_help.Spec.Peaks[index].Mass);
                    b_series.Add((LineSeries)this.MS2_help_denovol.all_series[index]);
                }
            }
            for (int i = 0; i < y_mass.Count; ++i)
            {
                int index = -1;
                double me = 0.0;
                if (this.Psm_help.Ppm_mass_error != 0)
                    index = this.Psm_help.IsInWithPPM(y_mass[i].Mass, mass_inten, ref me);
                else
                    index = this.Psm_help.IsInWithDa(y_mass[i].Mass, mass_inten, ref me);
                if (index != -1)
                {
                    this.MS2_help_denovol.all_series_index2.Add(index);
                    scores.Add(add_arrow(this.Model.Axes[1].Minimum, this.Psm_help.Spec.Peaks[index].Mass, -1, index, me, b_mass[i].Name, y_color));
                    this.MS2_help_denovol.arrow_mass.Add(this.Psm_help.Spec.Peaks[index].Mass);
                    y_series.Add((LineSeries)this.MS2_help_denovol.all_series[index]);
                }
            }
            update_arrowY(scores);
            this.MS2_help_denovol.arrow_annotation_last = new List<Annotation>(this.MS2_help_denovol.arrow_annotation);
        }
        public void refresh_clear()
        {
            this.MS2_help_denovol.arrow_mass0 = 0.0;
            this.MS2_help_denovol_history.Clear();
            this.MS2_help_denovol.all_annotation.Clear();
            this.MS2_help_denovol.arrow_annotation.Clear();
            this.MS2_help_denovol.arrow_annotation_last.Clear();
            this.MS2_help_denovol.arrow_mass.Clear();
            this.MS2_help_denovol.all_series_index.Clear();
            this.MS2_help_denovol.all_series_index2.Clear();
            this.MS2_help_denovol.arrowMass_to_index.Clear();
            this.MS2_help_denovol_pNovo = new MS2_Help_Denovol_pNovol();
            this.b_series.Clear();
            this.y_series.Clear();
            this.current_color = default_color;
            for (int i = 0; i < this.MS2_help_denovol.all_series.Count; ++i)
                ((LineSeries)this.MS2_help_denovol.all_series[i]).Color = OxyColors.Transparent;
        }
        public void refresh_end()
        {
            this.MS2_help_denovol.arrow_annotation.Clear();
            for (int i = 0; i < this.MS2_help_denovol.arrow_mass.Count; ++i)
            {
                ((LineSeries)this.MS2_help_denovol.all_series[(int)this.double_to_index[(int)(this.MS2_help_denovol.arrow_mass[i] * 100)]]).Color = OxyColors.Transparent;
            }
            this.MS2_help_denovol.all_series_index2.Clear();
        }
        public void refresh_back()
        {
            this.isBack = true;
            if (this.MS2_help_denovol_history.Count <= 1)
                return;
            MS2_Help_Denovol mhd = this.MS2_help_denovol_history.Last();
            this.MS2_help_denovol_history.Remove(mhd);
            mhd = new MS2_Help_Denovol(this.MS2_help_denovol_history.Last());
            this.MS2_help_denovol = mhd;
        }
        //根据用户点击的pNovol答案进行标注
        public void refresh_pNovol(Pnovo_Result result)
        {
            refresh_clear();
            string sq = result.SQ;
            List<double> bmass = result.get_all_bMass();
            List<double> ymass = result.get_all_yMass();
            this.MS2_help_denovol_pNovo = new MS2_Help_Denovol_pNovol();
            add_pNovol_annotation(bmass, result, 0);
            add_pNovol_annotation(ymass, result, 1);
        }
        private void add_pNovol_annotation(List<double> mass, Pnovo_Result result, int flag) //flag=0:b,flag=1:y;
        {
            int[] mass_inten = new int[Config_Help.MaxMass];
            for (int k = 0; k < Psm_help.Spec.Peaks.Count; ++k)
            {
                int massi = (int)Psm_help.Spec.Peaks[k].Mass;
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
            string txt = "";
            double startPosition = this.Model.Axes[1].Minimum;
            double Y = arrow_Y;
            OxyColor color = b_color;
            if (flag == 1)
            {
                Y += 7;
                color = y_color;
            }
            for (int i = 0; i < mass.Count; ++i)
            {
                if (flag == 0)
                    txt += result.SQ[i];
                else
                    txt += result.SQ[result.SQ.Length - 1 - i];
                int index = -1;
                double me = 0.0;
                if (this.Psm_help.Ppm_mass_error != 0.0)
                    index = this.Psm_help.IsInWithPPM(mass[i], mass_inten, ref me);
                else
                    index = this.Psm_help.IsInWithDa(mass[i], mass_inten, ref me);
                if (index != -1)
                {
                    ArrowAnnotation aa = new ArrowAnnotation();
                    aa.StartPoint = new DataPoint(startPosition, Y);
                    aa.EndPoint = new DataPoint(this.Psm_help.Spec.Peaks[index].Mass, Y);
                    aa.Color = color;
                    aa.HeadWidth = 2;
                    aa.HeadLength = 5;
                    aa.LineStyle = LineStyle.Dot;
                    this.MS2_help_denovol_pNovo.ArrowAnnotation.Add(aa);
                    TextAnnotation ta = new TextAnnotation();
                    ta.Position = new DataPoint((startPosition + this.Psm_help.Spec.Peaks[index].Mass) / 2, Y);
                    ta.Text = txt;
                    ta.Stroke = OxyColors.Transparent;
                    ta.TextColor = color;
                    this.MS2_help_denovol_pNovo.TxtAnnotation.Add(ta);
                    LineSeries ls = new LineSeries();
                    ls.Points.Add(new DataPoint(this.Psm_help.Spec.Peaks[index].Mass, 0));
                    ls.Points.Add(new DataPoint(this.Psm_help.Spec.Peaks[index].Mass, denovol_Y_Max));
                    ls.Color = color;
                    ls.LineStyle = LineStyle.Dot;
                    this.MS2_help_denovol_pNovo.LineSeries.Add(ls);
                    ScatterSeries ss = new ScatterSeries();
                    ss.TrackerFormatString = "m/z: {2:0.000}\nmass_error: {4:0.00}";
                    ss.Points.Add(new DataPoint(this.Psm_help.Spec.Peaks[index].Mass, me));
                    ss.YAxisKey = this.Model.Axes[2].Key;
                    ss.MarkerFill = color;
                    ss.MarkerSize = 3;
                    this.MS2_help_denovol_pNovo.ScatterSeries.Add(ss);
                    startPosition = this.Psm_help.Spec.Peaks[index].Mass;
                    txt = "";
                }
            }
        }
        //获取所有可能的b1及y1质量
        private void get_all_mass(ref List<Denovol_Config.DCC> b_mass, ref List<Denovol_Config.DCC> y_mass)
        {
            b_mass = new List<Denovol_Config.DCC>();
            y_mass = new List<Denovol_Config.DCC>();
            for (int i = 0; i < Denovol_Config.All_mass.Count; ++i)
            {
                if (!Denovol_Config.All_mass[i].CanUse)
                    continue;
                b_mass.Add(new Denovol_Config.DCC(Denovol_Config.All_mass[i].Mass + Config_Help.massZI, Denovol_Config.All_mass[i].Name));
                y_mass.Add(new Denovol_Config.DCC(Denovol_Config.All_mass[i].Mass + Config_Help.massZI + Config_Help.massH2O, Denovol_Config.All_mass[i].Name));
            }
        }
        //更新所有峰的点击事件，让点击denovo
        private void refresh_series_click()
        {
            this.all_dot_ls.Clear();
            this.double_to_index.Clear();
            for (int i = 0; i < this.MS2_help_denovol.all_series.Count; ++i)
            {
                LineSeries new_ls = this.MS2_help_denovol.all_series[i] as LineSeries;
                new_ls.MouseDown += (s, e) =>
                {
                    if (e.ChangedButton != OxyMouseButton.Left)
                        return;
                    this.isBack = false;
                    if (b_series.Count != 0 || y_series.Count != 0)
                    {
                        if (b_series.Contains(new_ls))
                            current_color = b_color;
                        else
                            current_color = y_color;
                        b_series.Clear();
                        y_series.Clear();
                    }
                    DataPoint click_point = (DataPoint)(new_ls.Points[1]); //(int)Math.Round(e.HitTestResult.Index)
                    double mass = click_point.X;
                    update_MZ(mass);
                    e.Handled = true;
                };
                this.all_dot_ls.Add(new_ls);
                this.double_to_index[(int)(Psm_help.Spec.Peaks[i].Mass * 100)] = this.all_dot_ls.Count - 1;
            }
            for (int i = 0; i < this.Model.Series.Count; ++i)
            {
                if (!(this.Model.Series[i] is LineSeries))
                    continue;
                LineSeries new_ls = this.Model.Series[i] as LineSeries;
                if (new_ls.YAxisKey != this.Model.Axes[0].Key || new_ls.LineStyle == LineStyle.Dot)
                    continue;
                new_ls.MouseDown += (s, e) =>
                {
                    if (e.ChangedButton != OxyMouseButton.Left)
                        return;
                    DataPoint click_point = (DataPoint)new_ls.Points[1]; //(int)Math.Round(e.HitTestResult.Index)
                    double mass = click_point.X;
                    update_MZ(mass);
                    e.Handled = true;
                };
            }
        }
        private bool update_arrow(double mass) //更新箭头
        {
            if (this.MS2_help_denovol.arrow_mass.Count == 0)
                return true;
            const double mass_error = 0.01;
            bool isIn = false;
            for (int i = 0; i < this.MS2_help_denovol.arrow_mass.Count; ++i)
            {
                if (Math.Abs(this.MS2_help_denovol.arrow_mass[i] - mass) <= mass_error)
                {
                    isIn = true;
                    break;
                }
            }
            if (!isIn)
                return false;
            for (int i = 0; i < this.MS2_help_denovol.arrow_mass.Count; ++i)
            {
                if (Math.Abs(this.MS2_help_denovol.arrow_mass[i] - mass) <= mass_error)
                    continue;
                ((LineSeries)this.MS2_help_denovol.all_series[(int)this.double_to_index[(int)(this.MS2_help_denovol.arrow_mass[i] * 100)]]).Color = OxyColors.Transparent;
            }
            for (int i = 0; i < this.MS2_help_denovol.arrow_annotation_last.Count; i += 2)
            {
                ArrowAnnotation arrowAnn = this.MS2_help_denovol.arrow_annotation_last[i] as ArrowAnnotation;
                TextAnnotation txtAnn = this.MS2_help_denovol.arrow_annotation_last[i + 1] as TextAnnotation;
                ArrowAnnotation new_arrowAnn = new ArrowAnnotation();
                TextAnnotation new_txtAnn = new TextAnnotation();
                new_arrowAnn.HeadLength = arrowAnn.HeadLength;
                new_arrowAnn.HeadWidth = arrowAnn.HeadWidth;
                new_arrowAnn.Color = arrowAnn.Color;
                new_txtAnn.Text = txtAnn.Text;
                new_txtAnn.Stroke = txtAnn.Stroke;
                new_txtAnn.TextColor = txtAnn.TextColor;
                if (Math.Abs(arrowAnn.EndPoint.X - mass) <= mass_error) //K==Q,出现两个字母重叠
                {
                    new_arrowAnn.StartPoint = new DataPoint(arrowAnn.StartPoint.X, arrow_Y);
                    new_arrowAnn.EndPoint = new DataPoint(arrowAnn.EndPoint.X, arrow_Y);
                    new_arrowAnn.LineStyle = LineStyle.Solid;
                    new_txtAnn.Position = new DataPoint(txtAnn.Position.X, arrow_Y);
                    if (this.MS2_help_denovol.arrowMass_to_index[(int)(arrowAnn.EndPoint.X * 100)] == null)
                    {
                        this.MS2_help_denovol.all_annotation.Add(new_arrowAnn);
                        this.MS2_help_denovol.all_annotation.Add(new_txtAnn);
                        this.MS2_help_denovol.arrowMass_to_index[(int)(arrowAnn.EndPoint.X * 100)] = this.MS2_help_denovol.all_annotation.Count - 1;
                    }
                    else
                    {
                        int index = (int)this.MS2_help_denovol.arrowMass_to_index[(int)(arrowAnn.EndPoint.X * 100)];
                        ((TextAnnotation)this.MS2_help_denovol.all_annotation[index]).Text += "/" + txtAnn.Text;
                    }
                }
            }
            return true;
        }
        private double get_Edge_score(double intensity1, double intensity2, double mass_error)
        {
            return intensity1 * intensity2 / mass_error;
        }
        private void update_MZ(double mass)
        {
            const double mass_error = 0.01;
            if (Math.Abs(mass - this.MS2_help_denovol.arrow_mass0) <= mass_error)
                return;
            this.MS2_help_denovol.all_series_index.Add((int)this.double_to_index[(int)(mass * 100)]);
            this.MS2_help_denovol.arrow_annotation_last = new List<Annotation>(this.MS2_help_denovol.arrow_annotation);
            this.MS2_help_denovol.all_series_index2.Clear();
            this.MS2_help_denovol.arrow_annotation.Clear();
            this.MS2_help_denovol.arrow_mass0 = mass;
            bool flag = update_arrow(mass);
            if (!flag)
                return;
            this.MS2_help_denovol.arrow_mass.Clear();
            this.all_dot_ls[(int)(this.double_to_index[(int)(mass * 100)])].Color = OxyColors.Black;
            ((LineSeries)this.MS2_help_denovol.all_series[(int)(this.double_to_index[(int)(mass * 100)])]).Color = OxyColors.Black;
            int[] mass_inten = new int[Config_Help.MaxMass];
            for (int k = 0; k < Psm_help.Spec.Peaks.Count; ++k)
            {
                int massi = (int)Psm_help.Spec.Peaks[k].Mass;
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
            List<double> scores = new List<double>();

            for (int i = 0; i < Denovol_Config.All_mass.Count; ++i)
            {
                if (!Denovol_Config.All_mass[i].CanUse)
                    continue;
                double mass_add = mass + Denovol_Config.All_mass[i].Mass;
                double mass_minus = mass - Denovol_Config.All_mass[i].Mass;
                string aa_name = Denovol_Config.All_mass[i].Name;
                double me0 = 0.0, me1 = 0.0, me2 = 0.0;
                int min_k0 = -1, min_k1 = -1, min_k2 = -1;
                if (Psm_help.Ppm_mass_error != 0)
                {
                    min_k0 = Psm_help.IsInWithPPM(mass, mass_inten, ref me0);
                    min_k1 = Psm_help.IsInWithPPM(mass_add, mass_inten, ref me1);
                    min_k2 = Psm_help.IsInWithPPM(mass_minus, mass_inten, ref me2);
                }
                else
                {
                    min_k0 = Psm_help.IsInWithDa(mass, mass_inten, ref me0);
                    min_k1 = Psm_help.IsInWithDa(mass_add, mass_inten, ref me1);
                    min_k2 = Psm_help.IsInWithDa(mass_minus, mass_inten, ref me2);
                }
                if (min_k1 != -1 && !this.MS2_help_denovol.all_series_index.Contains(min_k1))
                {
                    scores.Add(add_arrow(mass, Psm_help.Spec.Peaks[min_k1].Mass, min_k0, min_k1, me1 - me0, aa_name, current_color));
                    this.MS2_help_denovol.arrow_mass.Add(Psm_help.Spec.Peaks[min_k1].Mass);
                    this.MS2_help_denovol.all_series_index2.Add(min_k1);
                }
                if (min_k2 != -1 && !this.MS2_help_denovol.all_series_index.Contains(min_k2))
                {
                    scores.Add(add_arrow(mass, Psm_help.Spec.Peaks[min_k2].Mass, min_k0, min_k2, me2 - me0, aa_name, current_color));
                    this.MS2_help_denovol.arrow_mass.Add(Psm_help.Spec.Peaks[min_k2].Mass);
                    this.MS2_help_denovol.all_series_index2.Add(min_k2);
                }
            }
            update_arrowY(scores);
            this.Ms2_help.window_sizeChg_Or_ZoomPan();
        }
        private void update_arrowY(List<double> scores) //根据打分排个序
        {
            this.arrow_Y_add = 50;
            this.arrow_Y_minus = 50;
            List<int> all_sort_index = new List<int>();
            const double max_score = double.MaxValue;
            while (true)
            {
                double min_score = double.MaxValue;
                int min_index = -1;
                for (int i = 0; i < scores.Count; ++i)
                {
                    if (scores[i] < min_score)
                    {
                        min_score = scores[i];
                        min_index = i;
                    }
                }
                if (min_index == -1)
                    break;
                all_sort_index.Add(min_index);
                scores[min_index] = max_score;
            }
            List<Annotation> arrow_annotation_tmp = new List<Annotation>(this.MS2_help_denovol.arrow_annotation);
            this.MS2_help_denovol.arrow_annotation.Clear();
            for (int i = 0; i < all_sort_index.Count; ++i)
            {
                ArrowAnnotation aa = arrow_annotation_tmp[all_sort_index[i] * 2] as ArrowAnnotation;
                TextAnnotation ta = arrow_annotation_tmp[all_sort_index[i] * 2 + 1] as TextAnnotation;
                if (aa.StartPoint.X < aa.EndPoint.X) //向右
                {
                    aa.StartPoint = new DataPoint(aa.StartPoint.X, this.arrow_Y_add);
                    aa.EndPoint = new DataPoint(aa.EndPoint.X, this.arrow_Y_add);
                    ta.Position = new DataPoint(ta.Position.X, this.arrow_Y_add);
                    this.arrow_Y_add += arrow_Y_Delta;
                }
                else //向左
                {
                    aa.StartPoint = new DataPoint(aa.StartPoint.X, this.arrow_Y_minus);
                    aa.EndPoint = new DataPoint(aa.EndPoint.X, this.arrow_Y_minus);
                    ta.Position = new DataPoint(ta.Position.X, this.arrow_Y_minus);
                    this.arrow_Y_minus += arrow_Y_Delta;
                }
                this.MS2_help_denovol.arrow_annotation.Add(aa);
                this.MS2_help_denovol.arrow_annotation.Add(ta);
            }
        }
        private double add_arrow(double mass1, double mass2, int index0, int index, double mass_error, string aa_name, OxyColor color)
        {
            double score = 0.0;
            if(index0 != -1)
                score = get_Edge_score(Psm_help.Spec.Peaks[index0].Intensity, Psm_help.Spec.Peaks[index].Intensity, Math.Abs(mass_error));
            else
                score = get_Edge_score(1, Psm_help.Spec.Peaks[index].Intensity, Math.Abs(mass_error));

            this.all_dot_ls[index].Color = OxyColors.Black;
            ArrowAnnotation arrowAnnotation = new ArrowAnnotation();
            arrowAnnotation.StartPoint = new DataPoint(mass1, 0);
            arrowAnnotation.EndPoint = new DataPoint(mass2, 0);
            arrowAnnotation.HeadWidth = 2;
            arrowAnnotation.HeadLength = 5;
            arrowAnnotation.LineStyle = LineStyle.Dot;
            arrowAnnotation.Color = color;
            TextAnnotation txtAnn = new TextAnnotation();
            txtAnn.Text = aa_name + " (" + score.ToString("F0") + ")";
            txtAnn.Position = new DataPoint((mass1 + mass2) / 2, 0);
            txtAnn.Stroke = OxyColors.Transparent;
            txtAnn.TextColor = color;

            this.MS2_help_denovol.arrow_annotation.Add(arrowAnnotation);
            this.MS2_help_denovol.arrow_annotation.Add(txtAnn);
            return score;
        }
    }
}
