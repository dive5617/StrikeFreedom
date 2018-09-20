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
using System.Windows.Media;

namespace pBuild
{
    public class MS2_Help
    {
        public PlotModel Model;
        public Display_Help dis_help;
        public double scale_width;
        public double scale_height;
        public double width;
        public double height;
        public MainWindow mainW;

        public Spectra spec;
 
        public Denovol_Help Den_help;

        public MS2_Help(PlotModel model, Display_Help dis_help, double scale_width, double scale_height)
        {
            this.Model = model;
            this.dis_help = dis_help;
            this.scale_width = scale_width;
            this.scale_height = scale_height;
            this.width = 0.0;
            this.height = 0.0;
            this.mainW = null;
            if(this.dis_help.Psm_help == null)
                return;
            this.Den_help = new Denovol_Help(this.Model, this.dis_help.Psm_help, this);
            this.Den_help.initial_series();
        }

        public MS2_Help(PlotModel model, Display_Help dis_help, double scale_width, double scale_height, double width, double height, MainWindow mainW)
        {
            this.Model = model;
            this.dis_help = dis_help;
            this.scale_width = scale_width;
            this.scale_height = scale_height;
            this.width = width;
            this.height = height;
            this.mainW = mainW;
            if (this.dis_help.Psm_help == null)
                return;
            this.Den_help = new Denovol_Help(this.Model, this.dis_help.Psm_help, this);
            this.Den_help.initial_series();
        }
        
        private void update_ladder_height()
        {
            int count = Display_Detail_Help.pTop_Ladder_Count;
            double start = 0.9 - Display_Detail_Help.Ladder_Height * count, end = 0.9, width = Display_Detail_Help.Ladder_Height;
            this.Model.Axes[0].EndPosition = start;
            for (int i = 0; i < count; ++i)
            {
                this.Model.Axes[3 + i].StartPosition = end - width;
                this.Model.Axes[3 + i].EndPosition = end;
                end -= width;
            }
        }
        //mix_flag=0表示匹配一条肽段，mix_flag==1表示匹配混合谱图
        //绘制MS2,is_ms2_in=true表示当前一直处在MS2面板,is_changed_aa表示是否修改了匹配的氨基酸表
        public void window_sizeChg_Or_ZoomPan() 
        {
            if (dis_help == null || Model == null)
                return;
            if (dis_help.Psm_help == null)
            {
                this.Model = null;
                return;
            }
            
            update_ladder_height();
            if (this.width != 0.0 && this.height != 0.0)
            {
                if (Ladder_Help.Scale_Width == 0.0)
                {
                    Ladder_Help.Width = this.width;
                    Ladder_Help.Scale_Width = this.Model.Axes[1].Scale * (this.Model.Axes[1].ActualMaximum - this.Model.Axes[1].ActualMinimum);
                    scale_width = Ladder_Help.Scale_Width;
                }
                if (Ladder_Help.Scale_Height == 0.0)
                {
                    Ladder_Help.Height = this.height;
                    Ladder_Help.Scale_Height = this.Model.Axes[3].Scale * (this.Model.Axes[3].ActualMaximum - this.Model.Axes[3].ActualMinimum);
                    scale_height = Ladder_Help.Scale_Height;
                }
            }
            if ((this.Model.Axes[1].ActualMaximum - this.Model.Axes[1].ActualMinimum) > 100)
                this.Model.Axes[1].StringFormat = "f0";
            else if ((this.Model.Axes[1].ActualMaximum - this.Model.Axes[1].ActualMinimum) > 10)
                this.Model.Axes[1].StringFormat = "f2";
            else if ((this.Model.Axes[1].ActualMaximum - this.Model.Axes[1].ActualMinimum) > 0.1)
                this.Model.Axes[1].StringFormat = "f4";
            else
                this.Model.Axes[1].StringFormat = "f6";

            this.Model.Axes[1].MajorStep = (this.Model.Axes[1].ActualMaximum - this.Model.Axes[1].ActualMinimum) / 10;
            this.Model.Axes[1].Scale = scale_width / (this.Model.Axes[1].ActualMaximum - this.Model.Axes[1].ActualMinimum);
            this.Model.Axes[3].Scale = scale_height / (this.Model.Axes[3].ActualMaximum - this.Model.Axes[3].ActualMinimum);

            this.Model.Axes[3].Scale = this.Model.Axes[3].Scale * Display_Detail_Help.Ladder_Height / Display_Detail_Help.Old_Ladder_Height;

            double match_score = 0.0;

            //dis_help.Psm_help.aa_index = int.Parse(selected_psm.Tag_flag);
            if (dis_help.Psm_help is PSM_Help)
            {
                PSM_Help ph = dis_help.Psm_help as PSM_Help;
                if (ph.Ppm_mass_error != 0.0) //使用PPM匹配
                {
                    if (ph.Mix_Flag == 0)
                        match_score = ph.Match(1);
                    else if (ph.Mix_Flag == 1)
                        match_score = ph.Match_Mix(1);
                }
                else
                {
                    if (ph.Mix_Flag == 0)
                        match_score = ph.Match(0);
                    else if (ph.Mix_Flag == 1)
                        match_score = ph.Match_Mix(0);
                }
            }
            else //pLink
            {
                if (dis_help.Psm_help.Ppm_mass_error != 0.0) //使用PPM匹配
                    match_score = dis_help.Psm_help.Match(1);
                else
                    match_score = dis_help.Psm_help.Match(0);
            }
            spec = dis_help.Psm_help.Spec;
            if (dis_help.Psm_help.Ppm_mass_error != 0.0)
                this.Model.Axes[2].Title = "ppm";
            else
                this.Model.Axes[2].Title = "Da";
            if (dis_help.Psm_help.Ppm_mass_error != 0.0)
            {
                this.Model.Axes[2].Minimum = -dis_help.Psm_help.Ppm_mass_error * 1.0e6;
                this.Model.Axes[2].Maximum = dis_help.Psm_help.Ppm_mass_error * 1.0e6;
            }
            else
            {
                this.Model.Axes[2].Minimum = -dis_help.Psm_help.Da_mass_error;
                this.Model.Axes[2].Maximum = dis_help.Psm_help.Da_mass_error;
            }
            this.Model.Axes[2].MajorStep = (this.Model.Axes[2].Maximum - this.Model.Axes[2].Minimum) / 2;
            this.Model.Annotations.Clear();
            this.Model.Series.Clear();
            
            /*   绘制谱峰  * */
            IList<IDataPoint> points = new List<IDataPoint>();
            points.Add(new DataPoint(0, 0));
            points.Add(new DataPoint(dis_help.Psm_help.Spec.Peaks[dis_help.Psm_help.Spec.Peaks.Count - 1].Mass + 50, 0));
            LineSeries ls = new LineSeries();
            ls.TrackerFormatString = "m/z: {2:0.000}\nIntensity: {4:0.00}%";
            ls.Color = OxyColors.Black;
            ls.LineStyle = LineStyle.Solid;
            ls.StrokeThickness = 2; //1
            ls.Points = points;
            ls.YAxisKey = this.Model.Axes[0].Key;
            this.Model.Series.Add(ls);

            for (int i = 0; i < dis_help.Psm_help.Spec.Peaks.Count; ++i)
            {
                if (dis_help.Psm_help.Psm_detail.BOry[i] == 0) //无匹配
                {
                    IList<IDataPoint> Points = new List<IDataPoint>();
                    Points.Add(new DataPoint(dis_help.Psm_help.Spec.Peaks[i].Mass, 0));
                    Points.Add(new DataPoint(dis_help.Psm_help.Spec.Peaks[i].Mass, dis_help.Psm_help.Spec.Peaks[i].Intensity));
                    LineSeries Ls = new LineSeries();
                    Ls.TrackerFormatString = "m/z: {2:0.000}\nIntensity: {4:0.00}%";
                    Ls.Color = OxyColors.Black;
                    Ls.LineStyle = LineStyle.Solid;
                    Ls.StrokeThickness = dis_help.ddh.NoNMatch_StrokeWidth;
                    Ls.Points = Points;
                    Ls.YAxisKey = this.Model.Axes[0].Key;
                    if (this.mainW != null)
                    {
                        Ls.MouseDown += (s, e) =>
                        {
                            this.mainW.state_X.Text = Ls.Points[1].X.ToString("f5");
                            this.mainW.state_Y.Text = (Ls.Points[1].Y * spec.Max_inten_E / 100).ToString("E2");
                        };
                    }
                    this.Model.Series.Add(Ls);
                }
                else if ((dis_help.Psm_help.Psm_detail.BOry[i] & 2) == 2) //C匹配
                {
                    IList<IDataPoint> Points = new List<IDataPoint>();
                    Points.Add(new DataPoint(dis_help.Psm_help.Spec.Peaks[i].Mass, 0));
                    Points.Add(new DataPoint(dis_help.Psm_help.Spec.Peaks[i].Mass, dis_help.Psm_help.Spec.Peaks[i].Intensity));
                    LineSeries Ls = new LineSeries();
                    Ls.TrackerFormatString = "m/z: {2:0.000}\nIntensity: {4:0.00}%";
                    Ls.Color = OxyColors.White;
                    Ls.LineStyle = LineStyle.Solid;
                    Ls.StrokeThickness = dis_help.ddh.Match_StrokeWidth;
                    Ls.Points = Points;
                    Ls.YAxisKey = this.Model.Axes[0].Key;
                    if (this.mainW != null)
                    {
                        Ls.MouseDown += (s, e) =>
                        {
                            this.mainW.state_X.Text = Ls.Points[1].X.ToString("f5");
                            this.mainW.state_Y.Text = (Ls.Points[1].Y * spec.Max_inten_E / 100).ToString("E2");
                        };
                    }
                    this.Model.Series.Add(Ls);

                    var lineAnnotation = new LineAnnotation();
                    lineAnnotation.Type = LineAnnotationType.Vertical;
                    lineAnnotation.StrokeThickness = dis_help.ddh.Match_StrokeWidth;
                    lineAnnotation.TextVerticalAlignment = OxyPlot.VerticalAlignment.Top;
                    if (dis_help.Psm_help.Spec.Peaks[i].Intensity >= 50)
                        lineAnnotation.TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Right;
                    else
                        lineAnnotation.TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Left;
                    lineAnnotation.X = dis_help.Psm_help.Spec.Peaks[i].Mass;
                    lineAnnotation.MaximumY = dis_help.Psm_help.Spec.Peaks[i].Intensity;
                    if (lineAnnotation.MaximumY <= 1)
                        lineAnnotation.MaximumY = 1;
                    lineAnnotation.LineStyle = LineStyle.Solid;
                    char flag_xyz = dis_help.Psm_help.Psm_detail.By_num[i][0][0];
                    if (dis_help.Psm_help.Mix_Flag == 0)
                    {
                        for (int p = 0; p < dis_help.Psm_help.Psm_detail.By_num[i].Count; ++p)
                            lineAnnotation.Text += dis_help.Psm_help.Psm_detail.By_num[i][p];
                    }
                    else
                    {
                        int flag_index = -1;
                        string txt = "";
                        for (int p = 0; p < dis_help.Psm_help.Psm_detail.By_num[i].Count; ++p)
                        {
                            string[] tmp_strs = dis_help.Psm_help.Psm_detail.By_num[i][p].Split('|');
                            if (tmp_strs.Length > 1)
                            {
                                int tmp_int = int.Parse(tmp_strs[1]);
                                if (tmp_int != flag_index)
                                {
                                    if (flag_index != -1)
                                        txt += ";";
                                    txt += (tmp_int + 1) + ":" + tmp_strs[0];
                                    flag_index = tmp_int;
                                }
                                else
                                    txt += "," + tmp_strs[0];
                            }
                            else
                            {
                                if (flag_index != -1)
                                    txt += ";";
                                txt += tmp_strs[0];
                                flag_index = int.MaxValue;
                            }
                        }
                        lineAnnotation.Text = txt;
                    }
                    lineAnnotation.TextPosition = 1.0;
                    lineAnnotation.TextMargin = 1;
                    if (flag_xyz == 'y')
                    {
                        if (dis_help.Psm_help.Mix_Flag == 0)
                        {
                            lineAnnotation.Color = dis_help.ddh.Y_Match_Color;
                            lineAnnotation.TextColor = dis_help.ddh.Y_Match_Color;
                        }
                        else
                        {
                            int pep_index = dis_help.Psm_help.Psm_detail.Peptide_index[i][0];
                            OxyColor color = Display_Help.get_color2(pep_index, 2);
                            lineAnnotation.Color = color;
                            lineAnnotation.TextColor = color;
                        }
                    }
                    else if (flag_xyz == 'x')
                    {
                        if (dis_help.Psm_help.Mix_Flag == 0)
                        {
                            lineAnnotation.Color = dis_help.ddh.X_Match_Color;
                            lineAnnotation.TextColor = dis_help.ddh.X_Match_Color;
                        }
                        else
                        {
                            int pep_index = dis_help.Psm_help.Psm_detail.Peptide_index[i][0];
                            OxyColor color = Display_Help.get_color2(pep_index, 2);
                            lineAnnotation.Color = color;
                            lineAnnotation.TextColor = color;
                        }
                    }
                    else if (flag_xyz == 'z')
                    {
                        if (dis_help.Psm_help.Mix_Flag == 0)
                        {
                            lineAnnotation.Color = dis_help.ddh.Z_Match_Color;
                            lineAnnotation.TextColor = dis_help.ddh.Z_Match_Color;
                        }
                        else
                        {
                            int pep_index = dis_help.Psm_help.Psm_detail.Peptide_index[i][0];
                            OxyColor color = Display_Help.get_color2(pep_index, 2);
                            lineAnnotation.Color = color;
                            lineAnnotation.TextColor = color;
                        }
                    }
                    else if (lineAnnotation.Text[0] == '*')
                    {
                        lineAnnotation.Color = dis_help.ddh.Lose_Match_Color;
                        lineAnnotation.TextColor = dis_help.ddh.Lose_Match_Color;
                    }
                    else
                    {
                        if (dis_help.Psm_help.Mix_Flag == 0)
                        {
                            lineAnnotation.Color = dis_help.ddh.Y_Match_Color;
                            lineAnnotation.TextColor = dis_help.ddh.Y_Match_Color;
                        }
                        else
                        {
                            int pep_index = dis_help.Psm_help.Psm_detail.Peptide_index[i][0];
                            OxyColor color = Display_Help.get_color2(pep_index, 2);
                            lineAnnotation.Color = color;
                            lineAnnotation.TextColor = color;
                        }
                    }
                    lineAnnotation.YAxisKey = this.Model.Axes[0].Key;
                    this.Model.Annotations.Add(lineAnnotation);
                }
                else if ((dis_help.Psm_help.Psm_detail.BOry[i] & 1) == 1) //N匹配
                {
                    IList<IDataPoint> Points = new List<IDataPoint>();
                    Points.Add(new DataPoint(dis_help.Psm_help.Spec.Peaks[i].Mass, 0));
                    Points.Add(new DataPoint(dis_help.Psm_help.Spec.Peaks[i].Mass, dis_help.Psm_help.Spec.Peaks[i].Intensity));
                    LineSeries Ls = new LineSeries();
                    Ls.TrackerFormatString = "m/z: {2:0.000}\nIntensity: {4:0.00}%";
                    Ls.Color = OxyColors.White;
                    Ls.LineStyle = LineStyle.Solid;
                    Ls.StrokeThickness = dis_help.ddh.Match_StrokeWidth;
                    Ls.Points = Points;
                    Ls.YAxisKey = this.Model.Axes[0].Key;
                    if (this.mainW != null)
                    {
                        Ls.MouseDown += (s, e) =>
                        {
                            this.mainW.state_X.Text = Ls.Points[1].X.ToString("f5");
                            this.mainW.state_Y.Text = (Ls.Points[1].Y * spec.Max_inten_E / 100).ToString("E2");
                        };
                    }
                    this.Model.Series.Add(Ls);

                    var lineAnnotation = new LineAnnotation();
                    lineAnnotation.Type = LineAnnotationType.Vertical;
                    lineAnnotation.StrokeThickness = dis_help.ddh.Match_StrokeWidth;
                    lineAnnotation.TextVerticalAlignment = OxyPlot.VerticalAlignment.Top;
                    if (dis_help.Psm_help.Spec.Peaks[i].Intensity >= 50)
                        lineAnnotation.TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Right;
                    else
                        lineAnnotation.TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Left;
                    lineAnnotation.X = dis_help.Psm_help.Spec.Peaks[i].Mass;
                    lineAnnotation.MaximumY = dis_help.Psm_help.Spec.Peaks[i].Intensity;
                    if (lineAnnotation.MaximumY <= 1)
                        lineAnnotation.MaximumY = 1;
                    lineAnnotation.LineStyle = LineStyle.Solid;
                    char flag_abc = dis_help.Psm_help.Psm_detail.By_num[i][0][0];
                    if (dis_help.Psm_help.Mix_Flag == 0)
                    {
                        for (int p = 0; p < dis_help.Psm_help.Psm_detail.By_num[i].Count; ++p)
                            lineAnnotation.Text += dis_help.Psm_help.Psm_detail.By_num[i][p];
                    }
                    else
                    {
                        int flag_index = -1;
                        string txt = "";
                        for (int p = 0; p < dis_help.Psm_help.Psm_detail.By_num[i].Count; ++p)
                        {
                            string[] tmp_strs = dis_help.Psm_help.Psm_detail.By_num[i][p].Split('|');
                            if (tmp_strs.Length > 1)
                            {
                                int tmp_int = int.Parse(tmp_strs[1]);
                                if (tmp_int != flag_index)
                                {
                                    if (flag_index != -1)
                                        txt += ";";
                                    txt += (tmp_int + 1) + ":" + tmp_strs[0];
                                    flag_index = tmp_int;
                                }
                                else
                                    txt += "," + tmp_strs[0];
                            }
                            else
                            {
                                if (flag_index != -1)
                                    txt += ";";
                                txt += tmp_strs[0];
                                flag_index = int.MaxValue;
                            }
                        }
                        lineAnnotation.Text = txt;
                    }
                    lineAnnotation.TextPosition = 1.0;
                    lineAnnotation.TextMargin = 1;
                    if (flag_abc == 'b')
                    {
                        if (dis_help.Psm_help.Mix_Flag == 0)
                        {
                            lineAnnotation.Color = dis_help.ddh.B_Match_Color;
                            lineAnnotation.TextColor = dis_help.ddh.B_Match_Color;
                        }
                        else
                        {
                            int pep_index = dis_help.Psm_help.Psm_detail.Peptide_index[i][0];
                            OxyColor color = Display_Help.get_color2(pep_index, 1);
                            lineAnnotation.Color = color;
                            lineAnnotation.TextColor = color;
                        }
                    }
                    else if (flag_abc == 'a')
                    {
                        if (dis_help.Psm_help.Mix_Flag == 0)
                        {
                            lineAnnotation.Color = dis_help.ddh.A_Match_Color;
                            lineAnnotation.TextColor = dis_help.ddh.A_Match_Color;
                        }
                        else
                        {
                            int pep_index = dis_help.Psm_help.Psm_detail.Peptide_index[i][0];
                            OxyColor color = Display_Help.get_color2(pep_index, 1);
                            lineAnnotation.Color = color;
                            lineAnnotation.TextColor = color;
                        }
                    }
                    else if (flag_abc == 'c')
                    {
                        if (dis_help.Psm_help.Mix_Flag == 0)
                        {
                            lineAnnotation.Color = dis_help.ddh.C_Match_Color;
                            lineAnnotation.TextColor = dis_help.ddh.C_Match_Color;
                        }
                        else
                        {
                            int pep_index = dis_help.Psm_help.Psm_detail.Peptide_index[i][0];
                            OxyColor color = Display_Help.get_color2(pep_index, 1);
                            lineAnnotation.Color = color;
                            lineAnnotation.TextColor = color;
                        }
                    }
                    else if (lineAnnotation.Text[0] == '*') //中性丢失
                    {
                        lineAnnotation.Color = dis_help.ddh.Lose_Match_Color;
                        lineAnnotation.TextColor = dis_help.ddh.Lose_Match_Color;
                    }
                    lineAnnotation.YAxisKey = this.Model.Axes[0].Key;
                    this.Model.Annotations.Add(lineAnnotation);
                }
                else if ((dis_help.Psm_help.Psm_detail.BOry[i] & 64) == 64) //母离子匹配上了
                {
                    IList<IDataPoint> Points = new List<IDataPoint>();
                    Points.Add(new DataPoint(dis_help.Psm_help.Spec.Peaks[i].Mass, 0));
                    Points.Add(new DataPoint(dis_help.Psm_help.Spec.Peaks[i].Mass, dis_help.Psm_help.Spec.Peaks[i].Intensity));
                    LineSeries Ls = new LineSeries();
                    Ls.TrackerFormatString = "m/z: {2:0.000}\nIntensity: {4:0.00}%";
                    Ls.Color = OxyColors.White;
                    Ls.LineStyle = LineStyle.Solid;
                    Ls.StrokeThickness = dis_help.ddh.Match_StrokeWidth;
                    Ls.Points = Points;
                    Ls.YAxisKey = this.Model.Axes[0].Key;
                    if (this.mainW != null)
                    {
                        Ls.MouseDown += (s, e) =>
                        {
                            this.mainW.state_X.Text = Ls.Points[1].X.ToString("f5");
                            this.mainW.state_Y.Text = (Ls.Points[1].Y * spec.Max_inten_E / 100).ToString("E2");
                        };
                    }
                    this.Model.Series.Add(Ls);

                    var lineAnnotation = new LineAnnotation();
                    lineAnnotation.Type = LineAnnotationType.Vertical;
                    lineAnnotation.StrokeThickness = dis_help.ddh.Match_StrokeWidth;
                    lineAnnotation.TextVerticalAlignment = OxyPlot.VerticalAlignment.Top;
                    if (dis_help.Psm_help.Spec.Peaks[i].Intensity >= 50)
                        lineAnnotation.TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Right;
                    else
                        lineAnnotation.TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Left;
                    lineAnnotation.X = dis_help.Psm_help.Spec.Peaks[i].Mass;
                    lineAnnotation.MaximumY = dis_help.Psm_help.Spec.Peaks[i].Intensity;
                    if (lineAnnotation.MaximumY <= 1)
                        lineAnnotation.MaximumY = 1;
                    lineAnnotation.LineStyle = LineStyle.Solid;
                    if (dis_help.Psm_help.Mix_Flag == 0)
                    {
                        for (int p = 0; p < dis_help.Psm_help.Psm_detail.By_num[i].Count; ++p)
                            lineAnnotation.Text += dis_help.Psm_help.Psm_detail.By_num[i][p];
                    }
                    else
                    {
                        int flag_index = -1;
                        string txt = "";
                        for (int p = 0; p < dis_help.Psm_help.Psm_detail.By_num[i].Count; ++p)
                        {
                            string[] tmp_strs = dis_help.Psm_help.Psm_detail.By_num[i][p].Split('|');
                            if (tmp_strs.Length > 1)
                            {
                                int tmp_int = int.Parse(tmp_strs[1]);
                                if (tmp_int != flag_index)
                                {
                                    if (flag_index != -1)
                                        txt += ";";
                                    txt += (tmp_int + 1) + ":" + tmp_strs[0];
                                    flag_index = tmp_int;
                                }
                                else
                                    txt += "," + tmp_strs[0];
                            }
                            else
                            {
                                if (flag_index != -1)
                                    txt += ";";
                                txt += tmp_strs[0];
                                flag_index = int.MaxValue;
                            }
                        }
                        lineAnnotation.Text = txt;
                    }
                    lineAnnotation.TextPosition = 1.0;
                    lineAnnotation.TextMargin = 1;
                    if (dis_help.Psm_help.Mix_Flag == 0)
                    {
                        lineAnnotation.Color = dis_help.ddh.M_Match_Color;
                        lineAnnotation.TextColor = dis_help.ddh.M_Match_Color;
                    }
                    else
                    {
                        int pep_index = dis_help.Psm_help.Psm_detail.Peptide_index[i][0];
                        OxyColor color = Display_Help.get_color2(pep_index, 0);
                        lineAnnotation.Color = color;
                        lineAnnotation.TextColor = color;
                    }
                    lineAnnotation.YAxisKey = this.Model.Axes[0].Key;
                    this.Model.Annotations.Add(lineAnnotation);
                }
                else if ((dis_help.Psm_help.Psm_detail.BOry[i] & 128) == 128) //内部离子匹配上了
                {
                    IList<IDataPoint> Points = new List<IDataPoint>();
                    Points.Add(new DataPoint(dis_help.Psm_help.Spec.Peaks[i].Mass, 0));
                    Points.Add(new DataPoint(dis_help.Psm_help.Spec.Peaks[i].Mass, dis_help.Psm_help.Spec.Peaks[i].Intensity));
                    LineSeries Ls = new LineSeries();
                    Ls.TrackerFormatString = "m/z: {2:0.000}\nIntensity: {4:0.00}%";
                    Ls.Color = OxyColors.White;
                    Ls.LineStyle = LineStyle.Solid;
                    Ls.StrokeThickness = dis_help.ddh.Match_StrokeWidth;
                    Ls.Points = Points;
                    Ls.YAxisKey = this.Model.Axes[0].Key;
                    if (this.mainW != null)
                    {
                        Ls.MouseDown += (s, e) =>
                        {
                            this.mainW.state_X.Text = Ls.Points[1].X.ToString("f5");
                            this.mainW.state_Y.Text = (Ls.Points[1].Y * spec.Max_inten_E / 100).ToString("E2");
                        };
                    }
                    this.Model.Series.Add(Ls);

                    var lineAnnotation = new LineAnnotation();
                    lineAnnotation.Type = LineAnnotationType.Vertical;
                    lineAnnotation.StrokeThickness = dis_help.ddh.Match_StrokeWidth;
                    lineAnnotation.TextVerticalAlignment = OxyPlot.VerticalAlignment.Top;
                    if (dis_help.Psm_help.Spec.Peaks[i].Intensity >= 50)
                        lineAnnotation.TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Right;
                    else
                        lineAnnotation.TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Left;
                    lineAnnotation.X = dis_help.Psm_help.Spec.Peaks[i].Mass;
                    lineAnnotation.MaximumY = dis_help.Psm_help.Spec.Peaks[i].Intensity;
                    if (lineAnnotation.MaximumY <= 1)
                        lineAnnotation.MaximumY = 1;
                    lineAnnotation.Color = dis_help.ddh.I_Match_Color;
                    lineAnnotation.LineStyle = LineStyle.Solid;
                    if (dis_help.Psm_help.Mix_Flag == 0)
                    {
                        for (int p = 0; p < dis_help.Psm_help.Psm_detail.By_num[i].Count; ++p)
                            lineAnnotation.Text += dis_help.Psm_help.Psm_detail.By_num[i][p];
                    }
                    else
                    {
                        int flag_index = -1;
                        string txt = "";
                        for (int p = 0; p < dis_help.Psm_help.Psm_detail.By_num[i].Count; ++p)
                        {
                            string[] tmp_strs = dis_help.Psm_help.Psm_detail.By_num[i][p].Split('|');
                            if (tmp_strs.Length > 1)
                            {
                                int tmp_int = int.Parse(tmp_strs[1]);
                                if (tmp_int != flag_index)
                                {
                                    if (flag_index != -1)
                                        txt += ";";
                                    txt += (tmp_int + 1) + ":" + tmp_strs[0];
                                    flag_index = tmp_int;
                                }
                                else
                                    txt += "," + tmp_strs[0];
                            }
                            else
                            {
                                if (flag_index != -1)
                                    txt += ";";
                                txt += tmp_strs[0];
                                flag_index = int.MaxValue;
                            }
                        }
                        lineAnnotation.Text = txt;
                    }
                    lineAnnotation.TextPosition = 1.0;
                    lineAnnotation.TextMargin = 1;
                    lineAnnotation.TextColor = dis_help.ddh.I_Match_Color;
                    lineAnnotation.YAxisKey = this.Model.Axes[0].Key;
                    this.Model.Annotations.Add(lineAnnotation);
                }
                else if ((dis_help.Psm_help.Psm_detail.BOry[i] & 256) == 256) //亚氨离子匹配上了
                {
                    IList<IDataPoint> Points = new List<IDataPoint>();
                    Points.Add(new DataPoint(dis_help.Psm_help.Spec.Peaks[i].Mass, 0));
                    Points.Add(new DataPoint(dis_help.Psm_help.Spec.Peaks[i].Mass, dis_help.Psm_help.Spec.Peaks[i].Intensity));
                    LineSeries Ls = new LineSeries();
                    Ls.TrackerFormatString = "m/z: {2:0.000}\nIntensity: {4:0.00}%";
                    Ls.Color = OxyColors.White;
                    Ls.LineStyle = LineStyle.Solid;
                    Ls.StrokeThickness = dis_help.ddh.Match_StrokeWidth;
                    Ls.Points = Points;
                    Ls.YAxisKey = this.Model.Axes[0].Key;
                    if (this.mainW != null)
                    {
                        Ls.MouseDown += (s, e) =>
                        {
                            this.mainW.state_X.Text = Ls.Points[1].X.ToString("f5");
                            this.mainW.state_Y.Text = (Ls.Points[1].Y * spec.Max_inten_E / 100).ToString("E2");
                        };
                    }
                    this.Model.Series.Add(Ls);

                    var lineAnnotation = new LineAnnotation();
                    lineAnnotation.Type = LineAnnotationType.Vertical;
                    lineAnnotation.StrokeThickness = dis_help.ddh.Match_StrokeWidth;
                    lineAnnotation.TextVerticalAlignment = OxyPlot.VerticalAlignment.Top;
                    if (dis_help.Psm_help.Spec.Peaks[i].Intensity >= 50)
                        lineAnnotation.TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Right;
                    else
                        lineAnnotation.TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Left;
                    lineAnnotation.X = dis_help.Psm_help.Spec.Peaks[i].Mass;
                    lineAnnotation.MaximumY = dis_help.Psm_help.Spec.Peaks[i].Intensity;
                    if (lineAnnotation.MaximumY <= 1)
                        lineAnnotation.MaximumY = 1;
                    lineAnnotation.Color = dis_help.ddh.O_Match_Color;
                    lineAnnotation.LineStyle = LineStyle.Solid;
                    if (dis_help.Psm_help.Mix_Flag == 0)
                    {
                        for (int p = 0; p < dis_help.Psm_help.Psm_detail.By_num[i].Count; ++p)
                            lineAnnotation.Text += dis_help.Psm_help.Psm_detail.By_num[i][p];
                    }
                    else
                    {
                        int flag_index = -1;
                        string txt = "";
                        for (int p = 0; p < dis_help.Psm_help.Psm_detail.By_num[i].Count; ++p)
                        {
                            string[] tmp_strs = dis_help.Psm_help.Psm_detail.By_num[i][p].Split('|');
                            if (tmp_strs.Length > 1)
                            {
                                int tmp_int = int.Parse(tmp_strs[1]);
                                if (tmp_int != flag_index)
                                {
                                    if (flag_index != -1)
                                        txt += ";";
                                    txt += (tmp_int + 1) + ":" + tmp_strs[0];
                                    flag_index = tmp_int;
                                }
                                else
                                    txt += "," + tmp_strs[0];
                            }
                            else
                            {
                                if (flag_index != -1)
                                    txt += ";";
                                txt += tmp_strs[0];
                                flag_index = int.MaxValue;
                            }
                        }
                        lineAnnotation.Text = txt;
                    }
                    lineAnnotation.TextPosition = 1.0;
                    lineAnnotation.TextMargin = 1;
                    lineAnnotation.TextColor = dis_help.ddh.O_Match_Color;
                    lineAnnotation.YAxisKey = this.Model.Axes[0].Key;
                    this.Model.Annotations.Add(lineAnnotation);
                }
            }

            /*  绘制下面的误差  * */
            for (int i = 0; i < dis_help.Psm_help.Spec.Peaks.Count; ++i)
            {
                if (dis_help.Psm_help.Psm_detail.BOry[i] != 0)
                {
                    for (int p = 0; p < dis_help.Psm_help.Psm_detail.Mass_error[i].Count; ++p)
                    {
                        ScatterSeries ss = new ScatterSeries();
                        ss.TrackerFormatString = "m/z: {2:0.000}\nmass_error: {4:0.00}";
                        IList<IDataPoint> Points = new List<IDataPoint>();
                        Points.Add(new DataPoint(dis_help.Psm_help.Spec.Peaks[i].Mass, dis_help.Psm_help.Psm_detail.Mass_error[i][p]));
                        char tmp_c = dis_help.Psm_help.Psm_detail.By_num[i][p][0];
                        char tmp_c2 = dis_help.Psm_help.Psm_detail.By_num[i][p][1];
                        if (tmp_c == 'b')
                        {
                            if (dis_help.Psm_help.Mix_Flag == 0)
                                ss.MarkerFill = dis_help.ddh.B_Match_Color;
                            else
                            {
                                int mark_index = 0;
                                string[] strs = dis_help.Psm_help.Psm_detail.By_num[i][p].Split('|');
                                if(strs.Length>1)
                                    mark_index = int.Parse(strs[1]);
                                ss.MarkerFill = Display_Help.get_color2(mark_index, 1);
                            }
                        }
                        else if (tmp_c == 'a')
                        {
                            if (dis_help.Psm_help.Mix_Flag == 0)
                                ss.MarkerFill = dis_help.ddh.A_Match_Color;
                            else
                            {
                                int mark_index = 0;
                                string[] strs = dis_help.Psm_help.Psm_detail.By_num[i][p].Split('|');
                                if (strs.Length > 1)
                                    mark_index = int.Parse(strs[1]);
                                ss.MarkerFill = Display_Help.get_color2(mark_index, 1);
                            }
                        }
                        else if (tmp_c == 'c')
                        {
                            if (dis_help.Psm_help.Mix_Flag == 0)
                                ss.MarkerFill = dis_help.ddh.C_Match_Color;
                            else
                            {
                                int mark_index = 0;
                                string[] strs = dis_help.Psm_help.Psm_detail.By_num[i][p].Split('|');
                                if (strs.Length > 1)
                                    mark_index = int.Parse(strs[1]);
                                ss.MarkerFill = Display_Help.get_color2(mark_index, 1);
                            }
                        }
                        else if (tmp_c == 'y')
                        {
                            if (dis_help.Psm_help.Mix_Flag == 0)
                                ss.MarkerFill = dis_help.ddh.Y_Match_Color;
                            else
                            {
                                int mark_index = 0;
                                string[] strs = dis_help.Psm_help.Psm_detail.By_num[i][p].Split('|');
                                if (strs.Length > 1)
                                    mark_index = int.Parse(strs[1]);
                                ss.MarkerFill = Display_Help.get_color2(mark_index, 2);
                            }
                        }
                        else if (tmp_c == 'x')
                        {
                            if (dis_help.Psm_help.Mix_Flag == 0)
                                ss.MarkerFill = dis_help.ddh.X_Match_Color;
                            else
                            {
                                int mark_index = 0;
                                string[] strs = dis_help.Psm_help.Psm_detail.By_num[i][p].Split('|');
                                if (strs.Length > 1)
                                    mark_index = int.Parse(strs[1]);
                                ss.MarkerFill = Display_Help.get_color2(mark_index, 2);
                            }
                        }
                        else if (tmp_c == 'z')
                        {
                            if (dis_help.Psm_help.Mix_Flag == 0)
                                ss.MarkerFill = dis_help.ddh.Z_Match_Color;
                            else
                            {
                                int mark_index = 0;
                                string[] strs = dis_help.Psm_help.Psm_detail.By_num[i][p].Split('|');
                                if (strs.Length > 1)
                                    mark_index = int.Parse(strs[1]);
                                ss.MarkerFill = Display_Help.get_color2(mark_index, 2);
                            }
                        }
                        else if (tmp_c == '*') //中性丢失
                            ss.MarkerFill = dis_help.ddh.Lose_Match_Color;
                        else if (tmp_c2 == 'M')
                        {
                            if (dis_help.Psm_help.Mix_Flag == 0)
                                ss.MarkerFill = dis_help.ddh.M_Match_Color;
                            else
                            {
                                int mark_index = 0;
                                string[] strs = dis_help.Psm_help.Psm_detail.By_num[i][p].Split('|');
                                if (strs.Length > 1)
                                    mark_index = int.Parse(strs[1]);
                                ss.MarkerFill = Display_Help.get_color2(mark_index, 0);
                            }
                        }
                        else if (tmp_c2 == 'I')
                            ss.MarkerFill = dis_help.ddh.I_Match_Color;
                        else if (tmp_c2 == 'O')
                            ss.MarkerFill = dis_help.ddh.O_Match_Color;
                        ss.MarkerSize = dis_help.ddh.ME_Weight;
                        ss.Points = Points;
                        ss.YAxisKey = this.Model.Axes[2].Key;
                        this.Model.Series.Add(ss);
                    }
                }
            }

            /*  绘制最上层的谱图title，序列，母离子质量，母离子误差等信息  * */
            string mod_sites_str = "";
            Hashtable modHash = new Hashtable();
            if (dis_help.Psm_help.Pep.Mods.Count > 0)
            {
                string mod_sites = "";
                for (int k = 0; k < dis_help.Psm_help.Pep.Mods.Count; ++k)
                {
                    string value = dis_help.Psm_help.Pep.Mods[k].Index + ", "; //+ "(" + Config_Help.label_name[dis_help.Psm_help.Pep.Mods[k].Flag_Index] + ")"
                    List<string> values = new List<string>();
                    if (modHash[dis_help.Psm_help.Pep.Mods[k].Mod_name] != null)
                        values = modHash[dis_help.Psm_help.Pep.Mods[k].Mod_name] as List<string>;
                    values.Add(value);
                    modHash[dis_help.Psm_help.Pep.Mods[k].Mod_name] = values;
                }
            }
            foreach (string key in modHash.Keys)
            {
                List<string> values = modHash[key] as List<string>;
                mod_sites_str += key + " : ";
                for (int i = 0; i < values.Count; ++i)
                    mod_sites_str += values[i];
                mod_sites_str += "  ";
            }
            double pep_mass = dis_help.Psm_help.Spec.Pepmass * dis_help.Psm_help.Spec.Charge - (dis_help.Psm_help.Spec.Charge - 1) * Config_Help.massZI; ;
            double pep_theory_mass = dis_help.Psm_help.Compute_Mass();
            double mass_error = pep_mass - pep_theory_mass;
            double mass_error_ppm = mass_error * 1e6 / pep_mass;

            int YAxisKey = 3 + Display_Detail_Help.pTop_Ladder_Count;
            const string blank_space = "      "; //6个空格
            double specific_x = Model.Axes[1].ActualMinimum + 0.01 * (Model.Axes[1].ActualMaximum - Model.Axes[1].ActualMinimum);
            double screen_x = Model.Axes[1].Transform(specific_x);
            for (int i = 0; i < 4; ++i)
            {
                string title = "";
                string value = "";
                switch (i)
                {
                    case 0:
                        title = "Base Peak: ";
                        value = dis_help.Psm_help.Spec.Max_inten_E.ToString("E2") + blank_space;
                        break;
                    case 1:
                        title = "MS2_Mass: ";
                        value = Math.Round(pep_mass, 6) + "Da / " + Math.Round(dis_help.Psm_help.Spec.Pepmass, 6) + "Th" + blank_space;
                        break;
                    case 2:
                        title = "MS2_mass - Theoretical_Mass: ";
                        value = mass_error.ToString("f6") + "Da / " + mass_error_ppm.ToString("f3") + "ppm" + blank_space;
                        break;
                    case 3:
                        title = "PSM_Score (%): ";
                        value = Math.Round(dis_help.Psm_help.Match_score * 100, 3) + "";
                        break;
                }
                TextAnnotation titleAnnotation = new TextAnnotation();
                TextAnnotation titleAnnotation_2 = new TextAnnotation();
                titleAnnotation.Font = Display_Detail_Help.font_type;
                titleAnnotation_2.Font = titleAnnotation.Font;
                titleAnnotation.FontSize = 12;
                titleAnnotation_2.FontSize = titleAnnotation.FontSize;
                titleAnnotation.FontWeight = OxyPlot.FontWeights.Bold;
                titleAnnotation_2.FontWeight = titleAnnotation.FontWeight;
                titleAnnotation.Stroke = OxyColors.Transparent;
                titleAnnotation_2.Stroke = titleAnnotation.Stroke;
                titleAnnotation.YAxisKey = this.Model.Axes[YAxisKey].Key;
                titleAnnotation_2.YAxisKey = titleAnnotation.YAxisKey;
                titleAnnotation.TextColor = Display_Detail_Help.title_color;
                titleAnnotation_2.TextColor = Display_Detail_Help.value_color;
                titleAnnotation_2.FontWeight = OxyPlot.FontWeights.Bold;
                titleAnnotation.HorizontalAlignment = OxyPlot.HorizontalAlignment.Left;
                titleAnnotation_2.HorizontalAlignment = OxyPlot.HorizontalAlignment.Left;

                titleAnnotation.Text = title;
                titleAnnotation_2.Text = value;
                titleAnnotation.Position = new DataPoint(Model.Axes[1].InverseTransform(screen_x), 0);

                screen_x += get_annotation_width(titleAnnotation);
                titleAnnotation_2.Position = new DataPoint(Model.Axes[1].InverseTransform(screen_x), 0);
                screen_x += get_annotation_width(titleAnnotation_2);

                this.Model.Annotations.Add(titleAnnotation);
                this.Model.Annotations.Add(titleAnnotation_2);
            }
            
            screen_x = Model.Axes[1].Transform(specific_x);
            for (int i = 0; i < 4; ++i)
            {
                string title = "";
                string value = "";
                switch (i)
                {
                    case 0:
                        title = "Title: ";
                        value = dis_help.Psm_help.Spec.Title + blank_space;
                        break;
                    case 1:
                        if (mod_sites_str != "")
                        {
                            title = "Mods: ";
                            value = mod_sites_str + blank_space;
                        }
                        else
                        {
                            title = "";
                            value = blank_space;
                        }
                        break;
                    case 2:
                        title = "Label: ";
                        value = Config_Help.label_name[this.dis_help.Psm_help.Pep.Tag_Flag] + blank_space;
                        break;
                    case 3:
                        title = "Info: ";
                        value = this.dis_help.Psm_help.Spec.Title.Split('.')[0];
                        break;
                }

                TextAnnotation titleAnnotation2 = new TextAnnotation();
                TextAnnotation titleAnnotation2_2 = new TextAnnotation();

                titleAnnotation2.HorizontalAlignment = OxyPlot.HorizontalAlignment.Left;
                titleAnnotation2_2.HorizontalAlignment = titleAnnotation2.HorizontalAlignment;
                titleAnnotation2.Font = Display_Detail_Help.font_type;
                titleAnnotation2_2.Font = titleAnnotation2.Font;
                titleAnnotation2.FontSize = 12;
                titleAnnotation2_2.FontSize = titleAnnotation2.FontSize;
                titleAnnotation2.FontWeight = OxyPlot.FontWeights.Bold;
                titleAnnotation2_2.FontWeight = titleAnnotation2.FontWeight;
                titleAnnotation2.Stroke = OxyColors.Transparent;
                titleAnnotation2_2.Stroke = titleAnnotation2.Stroke;
                titleAnnotation2.YAxisKey = this.Model.Axes[YAxisKey + 1].Key;
                titleAnnotation2_2.YAxisKey = titleAnnotation2.YAxisKey;
                titleAnnotation2.TextColor = Display_Detail_Help.title_color;
                titleAnnotation2_2.TextColor = Display_Detail_Help.value_color;
                titleAnnotation2_2.FontWeight = OxyPlot.FontWeights.Bold;
                titleAnnotation2.Position = new DataPoint(Model.Axes[1].InverseTransform(screen_x), 0);
                titleAnnotation2.Text = title;
                titleAnnotation2_2.Text = value;
                screen_x += get_annotation_width(titleAnnotation2);
                titleAnnotation2_2.Position = new DataPoint(Model.Axes[1].InverseTransform(screen_x), 0);
                screen_x += get_annotation_width(titleAnnotation2_2);
                this.Model.Annotations.Add(titleAnnotation2);
                this.Model.Annotations.Add(titleAnnotation2_2);
            }

            /*  绘制阶梯图  * */
            //首先计算SQ哪些位置是带修饰的
            display_ladder_diagram();
            
            this.Den_help.initial_all();

            this.Model.RefreshPlot(true);
        }
        
        private double get_annotation_width(TextAnnotation txtAnnotation)
        {
            Color color = Color.FromArgb(txtAnnotation.TextColor.A, txtAnnotation.TextColor.R, txtAnnotation.TextColor.G, txtAnnotation.TextColor.B);
            FormattedText ft = new FormattedText(txtAnnotation.Text, System.Globalization.CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, 
                new Typeface(txtAnnotation.Font), txtAnnotation.FontSize, new SolidColorBrush(color)); //Brushes.Black
            return ft.WidthIncludingTrailingWhitespace;
        }
        private void display_ladder_diagram()
        {
            if (this.Model.Axes[1].Scale == 0 || this.Model.Axes[3].Scale == 0)
                return;

            double match_score = 0.0;

            //dis_help.Psm_help.aa_index = int.Parse(selected_psm.Tag_flag);
            if (dis_help.Psm_help is PSM_Help)
            {
                PSM_Help ph = dis_help.Psm_help as PSM_Help;
                if (ph.Ppm_mass_error != 0.0) //使用PPM匹配
                {
                    if (ph.Mix_Flag == 0)
                        match_score = ph.Match(1);
                    else
                        match_score = ph.Match_Mix(1);
                }
                else
                {
                    if (ph.Mix_Flag == 0)
                        match_score = ph.Match(0);
                    else
                        match_score = ph.Match_Mix(0);
                }
            }
 
            double x = Model.Axes[1].Transform(Model.Axes[1].ActualMinimum);
            double y = Model.Axes[3].Transform(Model.Axes[3].ActualMinimum);
            dis_help.ddh.LeftBottomPoint.X = x;
            dis_help.ddh.LeftBottomPoint.Y = y;
            
            if (dis_help.ddh.FontSize_SQ == 0)
            {
                string font_str = dis_help.ddh.Font_SQ;
                dis_help.ddh.FontHeight_SQ = Model.Axes[3].Transform(106) - Model.Axes[3].Transform(114);
                System.Windows.Media.FontFamily fontFamily = new System.Windows.Media.FontFamily(font_str);
                dis_help.ddh.FontSize_SQ = (int)(dis_help.ddh.FontHeight_SQ / fontFamily.LineSpacing);
            }
            else
            {
                string font_str = dis_help.ddh.Font_SQ;
                System.Windows.Media.FontFamily fontFamily = new System.Windows.Media.FontFamily(font_str);
                dis_help.ddh.FontHeight_SQ = dis_help.ddh.FontSize_SQ * fontFamily.LineSpacing;
            }

            if (dis_help.ddh.FontSize_BY == 0)
            {
                string font_str = dis_help.ddh.Font_BY;
                dis_help.ddh.FontHeight_BY = Model.Axes[3].Transform(100) - Model.Axes[3].Transform(105);
                System.Windows.Media.FontFamily fontFamily = new System.Windows.Media.FontFamily(font_str);
                dis_help.ddh.FontSize_BY = dis_help.ddh.FontHeight_BY / fontFamily.LineSpacing;
            }
            else
            {
                string font_str = dis_help.ddh.Font_BY;
                System.Windows.Media.FontFamily fontFamily = new System.Windows.Media.FontFamily(font_str);
                dis_help.ddh.FontHeight_BY = dis_help.ddh.FontSize_BY * fontFamily.LineSpacing;
            }

            if (dis_help.ddh.FontSize_SQ >= 30.0)
            {
                dis_help.ddh.FontSize_SQ = 30.0;
                string font_str = dis_help.ddh.Font_BY;
                System.Windows.Media.FontFamily fontFamily = new System.Windows.Media.FontFamily(font_str);
                dis_help.ddh.FontHeight_SQ = dis_help.ddh.FontSize_SQ * fontFamily.LineSpacing;
                dis_help.ddh.FontSize_BY = dis_help.ddh.FontSize_SQ * 5 / 8;
                fontFamily = new System.Windows.Media.FontFamily(font_str);
                dis_help.ddh.FontHeight_BY = dis_help.ddh.FontSize_BY * fontFamily.LineSpacing;
            }
            dis_help.ddh.FontWidth_SQ = 1.2 * dis_help.ddh.FontHeight_SQ;
            //ddh.X_Delta_SQ = linearAxis2.Transform(50 + linearAxis2.ActualMinimum) - linearAxis2.Transform(linearAxis2.ActualMinimum);
            dis_help.ddh.X_Delta_SQ = 50;
            dis_help.ddh.Y_Delta_Interval = 1;
            dis_help.ddh.B_Delta_Interval = 1;
            //dis_help.ddh.Y_Delta_Interval = Model1.Axes[3].Transform(105) - Model1.Axes[3].Transform(106);
            //ddh.LineWidth = linearAxis2.Transform(10 + linearAxis2.ActualMinimum) - linearAxis2.Transform(linearAxis2.ActualMinimum);
            dis_help.ddh.LineWidth = 5 * dis_help.ddh.FontWidth_SQ / 12;
            //ddh.Delta_Width = linearAxis2.Transform(2 + linearAxis2.ActualMinimum) - linearAxis2.Transform(linearAxis2.ActualMinimum);
            dis_help.ddh.Delta_Width = 2 * dis_help.ddh.FontWidth_SQ / 12;

            double num_by = 0.8; //定义BY数字与BY的大小关系，[0，1]之间
            dis_help.ddh.FontSize_BY_NUM = num_by * dis_help.ddh.FontSize_BY;
            /* */
            
            //对混合谱的支持，每一个阶梯图用一行来表示
            List<int> charges = new List<int>();
            List<Peptide> peptides = new List<Peptide>();
            List<List<int>> B_flag_mix = new List<List<int>>();
            List<List<int>> C_flag_mix = new List<List<int>>();
            List<List<int>> Y_flag_mix = new List<List<int>>();
            List<List<int>> Z_flag_mix = new List<List<int>>();
            int[] x_pos_index = new int[3];
            const int aaCount = 60; //[!!!] 一行显示四十个氨基酸
            int yStartAACount = dis_help.Psm_help.Pep.Sq.Length % aaCount;
            if (dis_help.Psm_help.Mix_Flag == 0)
            {
                charges.Add(dis_help.Psm_help.Spec.Charge);
                //peptides.Add(dis_help.Psm_help.Pep);
                for(int l = 0; l < dis_help.Psm_help.Psm_detail.B_flag.Length / aaCount + 1; ++l)
                {
                    List<int> B_flag = new List<int>();
                    List<int> C_flag = new List<int>();
                    int start = l * aaCount, end = ((l + 1) * aaCount < dis_help.Psm_help.Psm_detail.B_flag.Length - 1 ? (l + 1) * aaCount : dis_help.Psm_help.Psm_detail.B_flag.Length - 1);
                    for (int i = start; i <= end; ++i)
                    {
                        B_flag.Add(dis_help.Psm_help.Psm_detail.B_flag[i]);
                        C_flag.Add(dis_help.Psm_help.Psm_detail.C_flag[i]);
                    }
                    B_flag_mix.Add(B_flag);
                    C_flag_mix.Add(C_flag);
                    
                }
                int end2 = dis_help.Psm_help.Psm_detail.B_flag.Length - 1;
                for (int l = 0; l < dis_help.Psm_help.Psm_detail.B_flag.Length / aaCount + 1; ++l)
                {
                    List<int> Y_flag = new List<int>();
                    List<int> Z_flag = new List<int>();
                    int floor = dis_help.Psm_help.Psm_detail.B_flag.Length / aaCount - l;

                    int start2 = (end2 - aaCount >= 0 ? end2 - aaCount : 0);
                    for (int i = start2; i <= end2; ++i)
                    {
                        Y_flag.Add(dis_help.Psm_help.Psm_detail.Y_flag[i]);
                        Z_flag.Add(dis_help.Psm_help.Psm_detail.Z_flag[i]);
                    }
                    Y_flag_mix.Add(Y_flag);
                    Z_flag_mix.Add(Z_flag);
                    end2 -= aaCount;
                }
                ObservableCollection<Modification> allMods = dis_help.Psm_help.Pep.Mods;
                for (int l = 0; l < dis_help.Psm_help.Pep.Sq.Length / aaCount + 1; ++l)
                {
                    int start = l * aaCount, end = ((l + 1) * aaCount < dis_help.Psm_help.Pep.Sq.Length ? (l + 1) * aaCount : dis_help.Psm_help.Pep.Sq.Length);
                    Peptide pep = new Peptide(dis_help.Psm_help.Pep.Sq.Substring(start, end - start));
                    peptides.Add(pep);
                }
                for (int l = 0; l < allMods.Count; ++l)
                {
                    int index = allMods[l].Index;
                    int index2 = 0, modIndex = 0;
                    if (index != 0)
                        index2 = (index - 1) / aaCount;
                    if(index != 0 && index % aaCount == 0)
                        modIndex = aaCount;
                    else if(index != 0)
                        modIndex = index % aaCount;
                    peptides[index2].Mods.Add(new Modification(modIndex, allMods[l].Mass, allMods[l].Mod_name, allMods[l].Mass_Loss));
                }
            }
            #region
            for (int p = 0; p < peptides.Count; ++p)
            {
                dis_help.ddh.LeftBottomPoint.X = x;
                dis_help.ddh.LeftBottomPoint.Y = y;
                if (dis_help.Psm_help is pLink.PSM_Help_2 || dis_help.Psm_help is pLink.PSM_Help_3)
                {
                    dis_help.ddh.LeftBottomPoint.X += x_pos_index[p] * dis_help.ddh.FontWidth_SQ;
                }
                ObservableCollection<Modification> mods = peptides[p].Mods;
                int[] mod_flag = new int[peptides[p].Sq.Length + 2]; //BUG
                for (int i = 0; i < mods.Count; ++i)
                {
                    mod_flag[mods[i].Index] = 1;
                }
                if (mod_flag[0] == 1)
                {
                    FormattedText ft = new FormattedText("*", System.Globalization.CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(dis_help.ddh.Font_SQ), dis_help.ddh.FontSize_SQ, Brushes.Black);
                    dis_help.ddh.BY_Mod_Width = ft.WidthIncludingTrailingWhitespace;
                    var starAnnotation = new TextAnnotation();
                    starAnnotation.Position = dis_help.InverseTransform(dis_help.ddh.LeftBottomPoint.X + dis_help.ddh.X_Delta_SQ, dis_help.ddh.LeftBottomPoint.Y - dis_help.ddh.FontHeight_BY - dis_help.ddh.B_Delta_Interval, (LinearAxis)Model.Axes[1], (LinearAxis)Model.Axes[3]);
                    starAnnotation.Font = dis_help.ddh.Font_SQ;
                    starAnnotation.FontSize = dis_help.ddh.FontSize_SQ;
                    starAnnotation.Text = "*";
                    starAnnotation.FontWeight = OxyPlot.FontWeights.Bold;
                    starAnnotation.TextColor = OxyColors.Red;
                    starAnnotation.Stroke = OxyColors.Transparent;
                    starAnnotation.YAxisKey = this.Model.Axes[3 + p].Key;
                    this.Model.Annotations.Add(starAnnotation);
                }

                var chargeAnnotation = new TextAnnotation();
                chargeAnnotation.Position = dis_help.InverseTransform(dis_help.ddh.LeftBottomPoint.X + dis_help.ddh.X_Delta_SQ, dis_help.ddh.LeftBottomPoint.Y - dis_help.ddh.FontHeight_BY - dis_help.ddh.Y_Delta_Interval - dis_help.ddh.B_Delta_Interval - dis_help.ddh.FontHeight_SQ, (LinearAxis)Model.Axes[1], (LinearAxis)Model.Axes[3]);
                chargeAnnotation.Font = dis_help.ddh.Font_BY;
                chargeAnnotation.FontSize = dis_help.ddh.FontSize_BY;
                chargeAnnotation.HorizontalAlignment = OxyPlot.HorizontalAlignment.Right;
                if (p == 0)
                    chargeAnnotation.Text = charges[p] + "+"; //写电荷
                else
                    chargeAnnotation.Text = "";
                chargeAnnotation.FontWeight = OxyPlot.FontWeights.Bold;
                chargeAnnotation.TextColor = OxyColors.Red;
                chargeAnnotation.Stroke = OxyColors.Transparent;
                chargeAnnotation.YAxisKey = this.Model.Axes[3 + p].Key;
                if (peptides[p].Sq != "")
                    this.Model.Annotations.Add(chargeAnnotation);
                
                for (int i = 0; i < peptides[p].Sq.Length; ++i)
                {
                    if (B_flag_mix[p][i + 1] == 1 || C_flag_mix[p][i + 1] == 1)
                    {
                        var verAnnotation = new TextAnnotation();
                        verAnnotation.Position = dis_help.InverseTransform(dis_help.ddh.LeftBottomPoint.X + dis_help.ddh.X_Delta_SQ + dis_help.ddh.FontWidth_SQ - dis_help.ddh.Delta_Width + dis_help.ddh.FontWidth_SQ * i + dis_help.ddh.BY_Mod_Width, dis_help.ddh.LeftBottomPoint.Y, (LinearAxis)Model.Axes[1], (LinearAxis)Model.Axes[3]);
                        verAnnotation.Font = dis_help.ddh.Font_BY;
                        verAnnotation.FontSize = dis_help.ddh.FontSize_BY_NUM;
                        verAnnotation.HorizontalAlignment = OxyPlot.HorizontalAlignment.Right;
                        verAnnotation.Text = "" + (p * aaCount + (i + 1));
                        verAnnotation.TextColor = dis_help.ddh.B_Match_Color;
                        verAnnotation.FontWeight = OxyPlot.FontWeights.Bold;
                        verAnnotation.Stroke = OxyColors.Transparent;
                        verAnnotation.YAxisKey = this.Model.Axes[3 + p].Key;
                        this.Model.Annotations.Add(verAnnotation);


                        FormattedText ft = new FormattedText(verAnnotation.Text, System.Globalization.CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(verAnnotation.Font), verAnnotation.FontSize, Brushes.Black);
                        if (ft == null)
                            return;
                        verAnnotation = new TextAnnotation();
                        verAnnotation.Position = dis_help.InverseTransform(dis_help.ddh.LeftBottomPoint.X + dis_help.ddh.X_Delta_SQ + dis_help.ddh.FontWidth_SQ - dis_help.ddh.Delta_Width + dis_help.ddh.FontWidth_SQ * i - ft.WidthIncludingTrailingWhitespace + dis_help.ddh.BY_Mod_Width, dis_help.ddh.LeftBottomPoint.Y, (LinearAxis)Model.Axes[1], (LinearAxis)Model.Axes[3]);
                        verAnnotation.Font = dis_help.ddh.Font_BY;
                        verAnnotation.FontSize = dis_help.ddh.FontSize_BY;
                        verAnnotation.HorizontalAlignment = OxyPlot.HorizontalAlignment.Right;
                        if (B_flag_mix[p][i + 1] == 1)
                            verAnnotation.Text = "b";
                        else if (C_flag_mix[p][i + 1] == 1)
                            verAnnotation.Text = "c";
                        verAnnotation.TextColor = dis_help.ddh.B_Match_Color;
                        verAnnotation.FontWeight = OxyPlot.FontWeights.Bold;
                        verAnnotation.Stroke = OxyColors.Transparent;
                        verAnnotation.YAxisKey = this.Model.Axes[3 + p].Key;
                        this.Model.Annotations.Add(verAnnotation);


                        IList<IDataPoint> Points = new List<IDataPoint>();
                        Points.Add(dis_help.InverseTransform(dis_help.ddh.LeftBottomPoint.X + dis_help.ddh.X_Delta_SQ + dis_help.ddh.FontWidth_SQ - dis_help.ddh.Delta_Width + dis_help.ddh.FontWidth_SQ * i - dis_help.ddh.LineWidth + dis_help.ddh.BY_Mod_Width, dis_help.ddh.LeftBottomPoint.Y - dis_help.ddh.FontHeight_BY - dis_help.ddh.B_Delta_Interval, (LinearAxis)Model.Axes[1], (LinearAxis)Model.Axes[3]));
                        Points.Add(dis_help.InverseTransform(dis_help.ddh.LeftBottomPoint.X + dis_help.ddh.X_Delta_SQ + dis_help.ddh.FontWidth_SQ - dis_help.ddh.Delta_Width + dis_help.ddh.FontWidth_SQ * i + dis_help.ddh.BY_Mod_Width, dis_help.ddh.LeftBottomPoint.Y - dis_help.ddh.FontHeight_BY - dis_help.ddh.B_Delta_Interval, (LinearAxis)Model.Axes[1], (LinearAxis)Model.Axes[3]));
                        Points.Add(dis_help.InverseTransform(dis_help.ddh.LeftBottomPoint.X + dis_help.ddh.X_Delta_SQ + dis_help.ddh.FontWidth_SQ - dis_help.ddh.Delta_Width + dis_help.ddh.FontWidth_SQ * i + dis_help.ddh.BY_Mod_Width, dis_help.ddh.LeftBottomPoint.Y - dis_help.ddh.FontHeight_BY - dis_help.ddh.B_Delta_Interval - dis_help.ddh.FontHeight_SQ, (LinearAxis)Model.Axes[1], (LinearAxis)Model.Axes[3]));
                        LineSeries Ls = new LineSeries();
                        Ls.Points = Points;
                        Ls.Color = OxyColors.Black;
                        Ls.LineStyle = LineStyle.Solid;
                        Ls.StrokeThickness = 1;
                        Ls.YAxisKey = this.Model.Axes[3 + p].Key;
                        Ls.MouseDown += (s, e) => //去掉没用的ToolTip，但是点比较远的地方还是会显示ToolTip
                        {
                            if ((e.ChangedButton != OxyMouseButton.Left))
                            {
                                return;
                            }
                            e.Handled = true;
                        }; 
                        this.Model.Series.Add(Ls);
                    }

                    if (Y_flag_mix[p][peptides[p].Sq.Length - i - 1] == 1 || Z_flag_mix[p][peptides[p].Sq.Length - i - 1] == 1)
                    {
                        var verAnnotation = new TextAnnotation();
                        verAnnotation.Position = dis_help.InverseTransform(dis_help.ddh.LeftBottomPoint.X + dis_help.ddh.X_Delta_SQ + dis_help.ddh.FontWidth_SQ - dis_help.ddh.Delta_Width + dis_help.ddh.FontWidth_SQ * i + dis_help.ddh.BY_Mod_Width, dis_help.ddh.LeftBottomPoint.Y - dis_help.ddh.FontHeight_BY - dis_help.ddh.Y_Delta_Interval - dis_help.ddh.B_Delta_Interval - dis_help.ddh.FontHeight_SQ, (LinearAxis)Model.Axes[1], (LinearAxis)Model.Axes[3]);
                        verAnnotation.Font = dis_help.ddh.Font_BY;
                        verAnnotation.FontSize = dis_help.ddh.FontSize_BY;
                        verAnnotation.HorizontalAlignment = OxyPlot.HorizontalAlignment.Left;
                        if (Y_flag_mix[p][peptides[p].Sq.Length - i - 1] == 1)
                            verAnnotation.Text = "y";
                        else if (Z_flag_mix[p][peptides[p].Sq.Length - i - 1] == 1)
                            verAnnotation.Text = "z";
                        verAnnotation.TextColor = dis_help.ddh.Y_Match_Color;
                        verAnnotation.FontWeight = OxyPlot.FontWeights.Bold;
                        verAnnotation.Stroke = OxyColors.Transparent;
                        verAnnotation.YAxisKey = this.Model.Axes[3 + p].Key;
                        this.Model.Annotations.Add(verAnnotation);

                        FormattedText ft = new FormattedText(verAnnotation.Text, System.Globalization.CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(verAnnotation.Font), verAnnotation.FontSize, Brushes.Black);

                        verAnnotation = new TextAnnotation();
                        verAnnotation.Position = dis_help.InverseTransform(dis_help.ddh.LeftBottomPoint.X + dis_help.ddh.X_Delta_SQ + dis_help.ddh.FontWidth_SQ - dis_help.ddh.Delta_Width + dis_help.ddh.FontWidth_SQ * i + ft.WidthIncludingTrailingWhitespace + dis_help.ddh.BY_Mod_Width, dis_help.ddh.LeftBottomPoint.Y - dis_help.ddh.FontHeight_BY - dis_help.ddh.Y_Delta_Interval - dis_help.ddh.B_Delta_Interval - dis_help.ddh.FontHeight_SQ, (LinearAxis)Model.Axes[1], (LinearAxis)Model.Axes[3]);
                        verAnnotation.Font = dis_help.ddh.Font_BY;
                        verAnnotation.FontSize = dis_help.ddh.FontSize_BY_NUM;
                        verAnnotation.HorizontalAlignment = OxyPlot.HorizontalAlignment.Left;
                        if (p != peptides.Count - 1)
                            verAnnotation.Text = "" + (yStartAACount + (peptides.Count - 2 - p) * aaCount + peptides[p].Sq.Length - i - 1);
                        else
                            verAnnotation.Text = "" + (peptides[p].Sq.Length - i - 1);
                        verAnnotation.TextColor = dis_help.ddh.Y_Match_Color;
                        verAnnotation.FontWeight = OxyPlot.FontWeights.Bold;
                        verAnnotation.Stroke = OxyColors.Transparent;
                        verAnnotation.YAxisKey = this.Model.Axes[3 + p].Key;
                        this.Model.Annotations.Add(verAnnotation);

                      

                        IList<IDataPoint> Points = new List<IDataPoint>();
                        Points.Add(dis_help.InverseTransform(dis_help.ddh.LeftBottomPoint.X + dis_help.ddh.X_Delta_SQ + dis_help.ddh.FontWidth_SQ - dis_help.ddh.Delta_Width + dis_help.ddh.FontWidth_SQ * i + dis_help.ddh.BY_Mod_Width, dis_help.ddh.LeftBottomPoint.Y - dis_help.ddh.FontHeight_BY - dis_help.ddh.B_Delta_Interval, (LinearAxis)Model.Axes[1], (LinearAxis)Model.Axes[3]));
                        Points.Add(dis_help.InverseTransform(dis_help.ddh.LeftBottomPoint.X + dis_help.ddh.X_Delta_SQ + dis_help.ddh.FontWidth_SQ - dis_help.ddh.Delta_Width + dis_help.ddh.FontWidth_SQ * i + dis_help.ddh.BY_Mod_Width, dis_help.ddh.LeftBottomPoint.Y - dis_help.ddh.FontHeight_BY - dis_help.ddh.B_Delta_Interval - dis_help.ddh.FontHeight_SQ, (LinearAxis)Model.Axes[1], (LinearAxis)Model.Axes[3]));
                        Points.Add(dis_help.InverseTransform(dis_help.ddh.LeftBottomPoint.X + dis_help.ddh.X_Delta_SQ + dis_help.ddh.FontWidth_SQ - dis_help.ddh.Delta_Width + dis_help.ddh.FontWidth_SQ * i + dis_help.ddh.LineWidth + dis_help.ddh.BY_Mod_Width, dis_help.ddh.LeftBottomPoint.Y - dis_help.ddh.FontHeight_BY - dis_help.ddh.B_Delta_Interval - dis_help.ddh.FontHeight_SQ, (LinearAxis)Model.Axes[1], (LinearAxis)Model.Axes[3]));
                        LineSeries Ls = new LineSeries();
                        Ls.Points = Points;
                        Ls.Color = OxyColors.Black;
                        Ls.LineStyle = LineStyle.Solid;
                        Ls.StrokeThickness = 1;
                        Ls.YAxisKey = this.Model.Axes[3 + p].Key;
                        Ls.MouseDown += (s, e) => //去掉没用的ToolTip
                        {
                            if ((e.ChangedButton != OxyMouseButton.Left))
                            {
                                return;
                            }
                            e.Handled = true;
                        }; 
                        this.Model.Series.Add(Ls);
                    }
                    var textAnnotation = new TextAnnotation();
                    textAnnotation.Position = dis_help.InverseTransform(dis_help.ddh.LeftBottomPoint.X + dis_help.ddh.X_Delta_SQ + dis_help.ddh.FontWidth_SQ * i + dis_help.ddh.BY_Mod_Width, dis_help.ddh.LeftBottomPoint.Y - dis_help.ddh.FontHeight_BY - dis_help.ddh.B_Delta_Interval, (LinearAxis)Model.Axes[1], (LinearAxis)Model.Axes[3]);
                    textAnnotation.Font = dis_help.ddh.Font_SQ;
                    textAnnotation.FontSize = dis_help.ddh.FontSize_SQ;
                    textAnnotation.HorizontalAlignment = OxyPlot.HorizontalAlignment.Left;
                    textAnnotation.Text = peptides[p].Sq[i] + ""; //写字母
                    textAnnotation.FontWeight = OxyPlot.FontWeights.Bold;
                    if (mod_flag[i + 1] == 0)
                        textAnnotation.TextColor = OxyColors.Black;
                    else
                        textAnnotation.TextColor = OxyColors.Red;
                    textAnnotation.Stroke = OxyColors.Transparent;
                    textAnnotation.YAxisKey = this.Model.Axes[3 + p].Key;
                    this.Model.Annotations.Add(textAnnotation);
                   
                }
                if (mod_flag[peptides[p].Sq.Length + 1] == 1)
                {
                    FormattedText ft = new FormattedText(peptides[p].Sq[peptides[p].Sq.Length - 1] + "", System.Globalization.CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(dis_help.ddh.Font_SQ), dis_help.ddh.FontSize_SQ, Brushes.Black);
                    double ZIMU_Width = ft.WidthIncludingTrailingWhitespace * 1.5;
                    var starAnnotation = new TextAnnotation();
                    starAnnotation.Position = dis_help.InverseTransform(dis_help.ddh.LeftBottomPoint.X + dis_help.ddh.X_Delta_SQ + dis_help.ddh.FontWidth_SQ * (dis_help.Psm_help.Pep.Sq.Length - 1) + ZIMU_Width + dis_help.ddh.BY_Mod_Width, dis_help.ddh.LeftBottomPoint.Y - dis_help.ddh.FontHeight_BY - dis_help.ddh.B_Delta_Interval, (LinearAxis)Model.Axes[1], (LinearAxis)Model.Axes[3]);
                    starAnnotation.Font = dis_help.ddh.Font_SQ;
                    starAnnotation.FontSize = dis_help.ddh.FontSize_SQ;
                    starAnnotation.Text = "*";
                    starAnnotation.FontWeight = OxyPlot.FontWeights.Bold;
                    starAnnotation.TextColor = OxyColors.Red;
                    starAnnotation.Stroke = OxyColors.Transparent;
                    starAnnotation.YAxisKey = this.Model.Axes[3 + p].Key;
                    this.Model.Annotations.Add(starAnnotation);
                }
            }
            #endregion
        }
    }
}
