using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class Spectra_MS1_Isotope
    {
        public int charge; //同位素峰簇的电荷信息
        public int count; //同位素峰簇的个数
        public List<double> mz; //同位素峰簇的m/z
        public List<double> intensity; //同位素峰簇的强度

        public Spectra_MS1_Isotope(int charge)
        {
            this.charge = charge;
            this.count = 0;
            mz = new List<double>();
            intensity = new List<double>();
        }
        public void add_OnePeak(double mz, double intensity)
        {
            this.mz.Add(mz);
            this.intensity.Add(intensity);
            this.count++;
        }
    }
}
