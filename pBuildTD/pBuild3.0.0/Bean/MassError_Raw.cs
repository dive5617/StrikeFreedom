using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    //每个RAW都有自己的质量误差分布图，在Summary中只显示多个RAW的箱线图，每个RAW都有一个箱线图。
    //每个RAW的质量误差分布图中，每个点对应一个鉴定结果
    public class MassError_Raw
    {
        public List<MassError_Simple> Target_MassError_Scores;
        public List<MassError_Simple> Decoy_MassError_Scores;
        public double Max_MassError_Da;
        public double Max_MassError_PPM;
        public double Max_Score;

        public MassError_Raw()
        {
            this.Target_MassError_Scores = new List<MassError_Simple>();
            this.Decoy_MassError_Scores = new List<MassError_Simple>();
            this.Max_MassError_Da = 0.0;
            this.Max_MassError_PPM = 0.0;
            this.Max_Score = 0.0;
        }

        public List<double> get_target_da()
        {
            List<double> me_da = new List<double>();
            for (int i = 0; i < this.Target_MassError_Scores.Count; ++i)
                me_da.Add(this.Target_MassError_Scores[i].MassError_Da);
            return me_da;
        }
        public List<double> get_target_ppm()
        {
            List<double> me_ppm = new List<double>();
            for (int i = 0; i < this.Target_MassError_Scores.Count; ++i)
                me_ppm.Add(this.Target_MassError_Scores[i].MassError_PPM);
            return me_ppm;
        }
        public List<IDataPoint> get_target_da_point()
        {
            List<IDataPoint> points = new List<IDataPoint>();
            for (int i = 0; i < this.Target_MassError_Scores.Count; ++i)
            {
                points.Add(new DataPoint(this.Target_MassError_Scores[i].MassError_Da,
                    this.Target_MassError_Scores[i].Score));
            }
            return points;
        }
        public List<IDataPoint> get_decoy_da_point()
        {
            List<IDataPoint> points = new List<IDataPoint>();
            for (int i = 0; i < this.Decoy_MassError_Scores.Count; ++i)
            {
                points.Add(new DataPoint(this.Decoy_MassError_Scores[i].MassError_Da,
                    this.Decoy_MassError_Scores[i].Score));
            }
            return points;
        }
        public List<IDataPoint> get_target_ppm_point()
        {
            List<IDataPoint> points = new List<IDataPoint>();
            for (int i = 0; i < this.Target_MassError_Scores.Count; ++i)
            {
                points.Add(new DataPoint(this.Target_MassError_Scores[i].MassError_PPM,
                    this.Target_MassError_Scores[i].Score));
            }
            return points;
        }
        public List<IDataPoint> get_decoy_ppm_point()
        {
            List<IDataPoint> points = new List<IDataPoint>();
            for (int i = 0; i < this.Decoy_MassError_Scores.Count; ++i)
            {
                points.Add(new DataPoint(this.Decoy_MassError_Scores[i].MassError_PPM,
                    this.Decoy_MassError_Scores[i].Score));
            }
            return points;
        }
        public List<int> get_target_psm_index()
        {
            List<int> psm_indexs = new List<int>();
            for (int i = 0; i < this.Target_MassError_Scores.Count; ++i)
            {
                psm_indexs.Add(this.Target_MassError_Scores[i].psm_index);
            }
            return psm_indexs;
        }
        public List<int> get_decoy_psm_index()
        {
            List<int> psm_indexs = new List<int>();
            for (int i = 0; i < this.Decoy_MassError_Scores.Count; ++i)
            {
                psm_indexs.Add(this.Decoy_MassError_Scores[i].psm_index);
            }
            return psm_indexs;
        }
    }
    //每个鉴定结果的质量误差及打分
    public class MassError_Simple
    {
        public double MassError_Da { get; set; }
        public double MassError_PPM { get; set; }
        public double Score { get; set; }

        public int psm_index { get; set; } //直接索引到PSM列表中的索引位置

        public MassError_Simple(double MassError_Da, double MassError_PPM, double Score, int psm_index)
        {
            this.MassError_Da = MassError_Da;
            this.MassError_PPM = MassError_PPM;
            this.Score = Score;
            this.psm_index = psm_index;
        }
    }
}
