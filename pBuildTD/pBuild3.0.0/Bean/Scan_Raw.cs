using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class Scan_Raw
    {
        public List<Scan_Raw_Simple> Target_Scan_Raw_Simples;
        public List<Scan_Raw_Simple> Decoy_Scan_Raw_Simples;

        public Scan_Raw()
        {
            Target_Scan_Raw_Simples = new List<Scan_Raw_Simple>();
            Decoy_Scan_Raw_Simples = new List<Scan_Raw_Simple>();
        }
    }
    public class Scan_Raw_Simple : IComparable
    {
        public double Mass_error_Da;
        public double Mass_error_ppm;
        public int Scan;
        public int psm_index; //直接索引到PSM列表中的索引位置

        public Scan_Raw_Simple(double Mass_error_Da, double Mass_error_ppm, int Scan, int psm_index)
        {
            this.Mass_error_Da = Mass_error_Da;
            this.Mass_error_ppm = Mass_error_ppm;
            this.Scan = Scan;
            this.psm_index = psm_index;
        }

        int IComparable.CompareTo(Object obj)
        {
            Scan_Raw_Simple srs_obj = (Scan_Raw_Simple)obj;
            if (this.Scan < srs_obj.Scan)
                return -1;
            else if (this.Scan > srs_obj.Scan)
                return 1;
            return 0;
        }
    }
}
