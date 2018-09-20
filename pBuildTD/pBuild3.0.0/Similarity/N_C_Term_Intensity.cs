using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild.Similarity
{
    public class N_C_Term_Intensity
    {
        public string Title;
        public int Flag; //氨基酸表，0表示非标记，1表示标记
        public int Charge;
        public List<double> N_intensity;
        public List<double> C_intensity;
        public List<double> N_mass;
        public List<double> C_mass;

        public string Sq;

        public N_C_Term_Intensity(string title, int flag, int charge, string sq)
        {
            this.Title = title;
            this.Flag = flag;
            this.Charge = charge;
            this.Sq = sq;
            this.N_intensity = new List<double>();
            this.C_intensity = new List<double>();
            this.N_mass = new List<double>();
            this.C_mass = new List<double>();
        }

        public string get_title()
        {
            string[] strs = Title.Split('.');
            return strs[0] + "." + strs[1] + "." + strs[4];
        }
    }
}
