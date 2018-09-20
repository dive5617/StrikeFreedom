using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild.Similarity
{
    public class N_C_Term_Sim
    {
        public N_C_Term_Intensity N1;
        public N_C_Term_Intensity N2;

        public N_C_Term_Sim(N_C_Term_Intensity n1, N_C_Term_Intensity n2)
        {
            this.N1 = n1;
            this.N2 = n2;
        }

        public double Get_Cos_Sim()
        {
            double fz = 0.0, fm1 = 0.0, fm2 = 0.0;
            List<double> a1 = new List<double>();
            List<double> a2 = new List<double>();
            if (this.N1.N_intensity.Count != this.N2.N_intensity.Count)
            { 
                System.Windows.MessageBox.Show("N\t" + this.N1.Title + ":" + this.N2.Title);
                return 0.0;
            }
            if (this.N1.C_intensity.Count != this.N2.C_intensity.Count)
            {
                System.Windows.MessageBox.Show("C\t" + this.N1.Title + ":" + this.N2.Title);
                return 0.0;
            }
            for(int i=0;i<this.N1.N_intensity.Count;++i)
            {
                a1.Add(N1.N_intensity[i]);
                a2.Add(N2.N_intensity[i]);
            }
            for(int i=0;i<this.N1.C_intensity.Count;++i)
            {
                a1.Add(N1.C_intensity[i]);
                a2.Add(N2.C_intensity[i]);
            }
            for(int i=0;i<a1.Count;++i)
            {
                fz += a1[i] * a2[i];
                fm1 += a1[i] * a1[i];
                fm2 += a2[i] * a2[i];
            }
            if (fm1 == 0.0 || fm2 == 0.0)
                return 0.0;
            return fz / Math.Sqrt(fm1 * fm2);
        }
        public static bool Is_matched(string[] matched_name, string[] matched_name2, string name)
        {
            for (int i = 0; i < matched_name.Length; ++i)
            {
                if (!name.StartsWith(matched_name[i]))
                    continue;
                for (int j = 0; j < matched_name2.Length; ++j)
                {
                    if (name.EndsWith(matched_name2[j]))
                        return true;
                }
            }
            return false;
        }
        public static double Get_Cos_Sim(Report_Ion ion1, Report_Ion ion2)
        {
            string[] matched_name = {"b","y" };
            string[] matched_name2 = { "+", "++" }; //所有理论离子均考虑计算 Cos相似度，如果只考试y+离子，那么可以matched_name={"y"},matched_name2={"+"}

            List<double> sim1 = new List<double>();
            List<double> sim2 = new List<double>();
            List<Report_Ion.Ion> ions1 = ion1.get_Ion();
            List<Report_Ion.Ion> ions2 = ion2.get_Ion();
            for (int i = 0; i < ions1.Count; ++i)
            {
                if (!Is_matched(matched_name, matched_name2, ions1[i].name))
                    continue;
                sim1.Add(ions1[i].intensity);
            }
            for (int i = 0; i < ions2.Count; ++i)
            {
                if (!Is_matched(matched_name, matched_name2, ions2[i].name))
                    continue;
                sim2.Add(ions2[i].intensity);
            }
            if (sim1.Count != sim2.Count)
            {
                System.Windows.MessageBox.Show("The count of two PSMs are not same.");
                return 0.0;
            }
            return get_COS(sim1, sim2);
        }
        public static double get_COS(List<double> a, List<double> b)
        {
            double fz = 0.0, fm1 = 0.0, fm2 = 0.0;
            for (int i = 0; i < a.Count; ++i)
            {
                fz += a[i] * b[i];
                fm1 += a[i] * a[i];
                fm2 += b[i] * b[i];
            }
            return fz / (Math.Sqrt(fm1) * Math.Sqrt(fm2));
        }
    }
}
