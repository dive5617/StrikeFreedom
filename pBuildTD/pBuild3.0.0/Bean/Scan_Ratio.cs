using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class Scan_Ratio
    {
        public double Scan { get; set; }
        public double Ratio { get; set; }

        public Scan_Ratio(double scan, double ratio)
        {
            this.Scan = scan;
            this.Ratio = ratio;
        }
    }
}
