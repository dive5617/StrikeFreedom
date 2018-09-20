using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class MS2_Quant_Help
    {
        public List<NC_Term> nc_terms;
        public double Ppm_mass_error;
        public double Da_mass_error;
        public double mz1, mz2;

        public MS2_Quant_Help()
        {
            this.nc_terms = new List<NC_Term>();
            for (int i = 0; i < 2; ++i)
                this.nc_terms.Add(new NC_Term());
            this.Ppm_mass_error = 0.0;
            this.Da_mass_error = 0.0;
            this.mz1 = 0;
            this.mz2 = 100000;
        }

        public MS2_Quant_Help(double n1_mass, double n2_mass, double c1_mass, double c2_mass, string n1_str, 
            string n2_str, string c1_str, string c2_str, double Ppm_mass_error, double Da_mass_error, double mz1, double mz2)
        {
            this.nc_terms = new List<NC_Term>();
            this.nc_terms.Add(new NC_Term(n1_mass, c1_mass, n1_str, c1_str));
            this.nc_terms.Add(new NC_Term(n2_mass, c2_mass, n2_str, c2_str));
            this.Ppm_mass_error = Ppm_mass_error;
            this.Da_mass_error = Da_mass_error;
            this.mz1 = mz1;
            this.mz2 = mz2;
        }
    }
    public class NC_Term
    {
        public double n_mass { get; set; }
        public double c_mass { get; set; }

        public string n_str { get; set; }
        public string c_str { get; set; }

        public NC_Term()
        {
            n_mass = 0.0;
            c_mass = 0.0;

            n_str = "";
            c_str = "";
        }
        public NC_Term(double n_mass, double c_mass, string n_str, string c_str)
        {
            this.n_mass = n_mass;
            this.c_mass = c_mass;
            this.n_str = n_str;
            this.c_str = c_str;
        }
    }
}
