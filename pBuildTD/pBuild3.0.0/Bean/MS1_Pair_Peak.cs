using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class MS1_Pair_Peak : IComparable
    {
        public PEAK Ori_Peak;
        public PEAK Pair_Peak;

        public double N_Mass; //实际的N元素偏差质量
        public int N_Number; //实际的N元素偏差个数

        public double Mass_Error; //Ori和Pair两根峰与实际的质量差之间的偏差,ppm级
        public double Intensity_Error; //Ori和Pair两根峰间的强度倍数差

        public double Value; //Mass_Error+30*Intensity_Error

        public MS1_Pair_Peak(PEAK ori_peak, PEAK pair_peak, double N_mass, int N_number)
        {
            this.Ori_Peak = ori_peak;
            this.Pair_Peak = pair_peak;
            this.N_Mass = N_mass;
            this.N_Number = N_number;
            this.Mass_Error = Math.Abs(Math.Abs(Ori_Peak.Mass - Pair_Peak.Mass) - N_mass) * 1e6 / Ori_Peak.Mass;
            this.Intensity_Error = Math.Abs(Pair_Peak.Intensity / Ori_Peak.Intensity - 1);
            this.Value = Mass_Error + Intensity_Error * 30;
        }

        int IComparable.CompareTo(Object obj)
        {
            MS1_Pair_Peak temp = (MS1_Pair_Peak)obj;
            if (this.Value < temp.Value)
                return -1;
            else if (this.Value > temp.Value)
                return 1;
            return 0;
        }
    }
}
