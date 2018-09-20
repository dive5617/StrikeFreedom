using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class MS2_Quant_Help2
    {
        public List<double> Masses { get; set; }
        public List<double> Mass_errors { get; set; }
        public List<int> Mass_error_flags { get; set; } //为0表示ppm,为1表示Da

        public MS2_Quant_Help2()
        {
            this.Masses = new List<double>();
            this.Mass_errors = new List<double>();
            this.Mass_error_flags = new List<int>();
        }

        public MS2_Quant_Help2(List<double> masses, List<double> mass_errors, List<int> mass_error_flags)
        {
            this.Masses = masses;
            this.Mass_errors = mass_errors;
            this.Mass_error_flags = mass_error_flags;
        }
    }
}
