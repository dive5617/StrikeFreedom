using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class Summary_Result_Information
    {
        //Peptide Level:
        public int spectra_number { get; set; }
        public int scans_number { get; set; }
        public int peptides_number { get; set; }
        public int sequences_number { get; set; }
        public int proteins_number { get; set; }
        public int protein_groups_number { get; set; }
        public int decoy_spectra_number { get; set; }
        public int decoy_peptides_number { get; set; }
        public int decoy_proteins_number { get; set; }
        public int decoy_protein_groups_number { get; set; }

        //Cleavage:
        public int Specific_number;
        public double Specific { get; set; }
        public int C_term_specific_number;
        public double C_term_specific { get; set; }
        public int N_term_specific_number;
        public double N_term_specific { get; set; }
        public int Non_specific_number;
        public double Non_specific { get; set; }

        //Modification:
        public List<Identification_Modification> modifications;

        //Length Distribution:
        public List<Length_Distribution> length_distribution;

        //Charge Distribution:
        public List<Charge_Distribution> charge_distribution;

        //Missed Cleavage Distribution:
        public List<Missed_Cleavage_Distribution> missed_cleavage_distribution;

        //Mixed Spectra:
        public List<Mixed_Spectra> mixed_spectra = new List<Mixed_Spectra>();

        //Precusor mass error:
        public double meanPreMe, stdPreMe;

        //ID Rate:
        public List<Raw_Rate> raw_rates = new List<Raw_Rate>();

        //定量信息，非数比值个数、均值、中位数、标准差
        public string NaN_number { get; set; }
        public double Mean { get; set; }
        public double Median { get; set; }
        public double Standard_Deviation { get; set; }
    }
    public class Identification_Modification
    {
        public string modification_name;
        public int mod_spectra_count;
        public double mod_spectra_percentage;
        public int mod_sites_count;

        public Identification_Modification(string modification_name, int mod_spectra_count, double mod_spectra_percentage, int mod_sites_count)
        {
            this.modification_name = modification_name;
            this.mod_spectra_count = mod_spectra_count;
            this.mod_spectra_percentage = mod_spectra_percentage;
            this.mod_sites_count = mod_sites_count;
        }
    }
    public class Length_Distribution
    {
        public int length;
        public int num;
        public double ratio;

        public Length_Distribution(int length, int num, double ratio)
        {
            this.length = length;
            this.num = num;
            this.ratio = ratio;
        }
    }
    public class Charge_Distribution
    {
        public int charge;
        public int num;
        public double ratio;

        public Charge_Distribution(int charge, int num, double ratio)
        {
            this.charge = charge;
            this.num = num;
            this.ratio = ratio;
        }
    }
    public class Missed_Cleavage_Distribution
    {
        public int missed_number;
        public int num;
        public double ratio;

        public Missed_Cleavage_Distribution(int mis_num, int num, double ratio)
        {
            this.missed_number = mis_num;
            this.num = num;
            this.ratio = ratio;
        }
    }
    public class Mixed_Spectra
    {
        public int mixed_spectra_num; //1,2,3,4
        public int num;
        public double ratio;

        public Mixed_Spectra(int mixed_spectra_num, int num, double ratio)
        {
            this.mixed_spectra_num = mixed_spectra_num;
            this.num = num;
            this.ratio = ratio;
        }
    }
    public class Raw_Rate
    {
        public string Raw_name { get; set; } //RAW的名字
        public int Identification_num { get; set; } //鉴定到的数目
        public int All_num { get; set; } //SCAN总数
        public double Rate { get; set; } //解析率

        public Raw_Rate(string Raw_name, int Identification_num, int All_num, double Rate)
        {
            this.Raw_name = Raw_name;
            this.Identification_num = Identification_num;
            this.All_num = All_num;
            this.Rate = Rate;
        }
    }
}
