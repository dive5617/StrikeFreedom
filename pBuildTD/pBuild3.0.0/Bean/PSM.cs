using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class PSM : IComparable
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Charge { get; set; }
        public string Sq { get; set; }
        public string Mod_sites { get; set; }
        public double Spectra_mass { get; set; }

        public double Theory_sq_mass { get; set; }
        public double Delta_mass { get; set; }
        public double Delta_mass_PPM { get; set; }

        public double Score { get; set; }

        public double Q_value { get; set; }

        public char Specific_flag { get; set; }
        public int Missed_cleavage_number { get; set; }
        public string Pep_flag { get; set; } //用来表示是哪种标记，然后可以方便更换质量表
        public bool Is_target_flag { get; set; } //表示是否是正库，1表示正库，0表示反库
        //用来进行标记，如果该PSM在列表中显示了，则为1，如果没有显示则为0
        public string AC { get; set; } //蛋白AC号
        public List<Peptide> Cand_peptides { get; set; } //候选的肽段，包括肽段序列及修饰

        public List<int> Protein_index { get; set; } //该PSM即肽段对应于哪些蛋白，可以通过解析AC号来计算出来

        public double Ms2_ratio { get; set; } //二级谱的定量比值
        public List<double> Ms2_ratios { get; set; } //二级谱的定量比值,A/B, A/C, A/D
        public double Ms2_ratio_a1 { get; set; } //二级谱的定量比值
        public double A1_intensity { get; set; } //二级谱中a1匹配的强度

        //定量的结果
        public double Ratio { get; set; } //定量的比例
        public double Sigma { get; set; }

        public PSM() { }

        public PSM(int id, string title, string sq, string mod_sites, double score)
        {
            this.Id = id;
            this.Title = title;
            this.Sq = sq;
            this.Mod_sites = mod_sites;
            this.Score = score;
            this.Pep_flag = "0";
            this.Is_target_flag = true;
        }
        public PSM(int id, string title, string sq, string mod_sites, double score, double spectra_mass
            , double q_value, double theory_sq_mass, double delta_mass, double delta_mass_ppm, char specific_flag, string pep_flag, bool is_target_flag, string AC, int missed_cleavage_number)
        {
            this.Id = id;
            this.Title = title;
            this.Sq = sq;
            this.Mod_sites = mod_sites;
            this.Score = score;
            this.Spectra_mass = spectra_mass;
            this.Q_value = q_value;
            this.Theory_sq_mass = theory_sq_mass;
            this.Delta_mass = delta_mass;
            this.Delta_mass_PPM = delta_mass_ppm;
            this.Specific_flag = specific_flag;
            this.Pep_flag = pep_flag;
            this.Is_target_flag = is_target_flag;
            this.AC = AC;
            this.Missed_cleavage_number = missed_cleavage_number;
            this.Protein_index = new List<int>();
            this.Cand_peptides = new List<Peptide>();
        }

        public bool is_Contaminant()
        {
            bool is_con = false; //是否是污染蛋白
            string[] strs = this.AC.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (int j = 0; j < strs.Length; ++j)
            {
                if (strs[j].ToUpper().StartsWith("CON_")) // || strs[j].ToUpper().StartsWith("REV_CON")需要将反库的污染库删除吗？不能删除
                {
                    is_con = true;
                    break;
                }
            }
            return is_con;
        }
        int IComparable.CompareTo(Object obj)
        {
            PSM temp = (PSM)obj;
            if (string.Compare(this.Title, temp.Title) < 0)
                return -1;
            else if (string.Compare(this.Title, temp.Title) > 0)
                return 1;
            return 0;
            //int scan_number = int.Parse(this.Title.Split('.')[1]);
            //int scan_number2 = int.Parse(temp.Title.Split('.')[1]);
            //if (scan_number < scan_number2)
            //    return -1;
            //else if (scan_number > scan_number2)
            //    return 1;
            //int pParse_number = int.Parse(this.Title.Split('.')[4]);
            //int pParse_number2 = int.Parse(temp.Title.Split('.')[4]);
            //if (pParse_number < pParse_number2)
            //    return -1;
            //else if (pParse_number > pParse_number2)
            //    return 1;
            //return 0;
        }
        public int get_charge()
        {
            int charge = 1;
            string charge_str = this.Title.Split('.')[3];
            charge = int.Parse(charge_str);
            return charge;
        }
        public string get_rawname()
        {
            return this.Title.Split('.')[0];
        }
        public int get_scan()
        {
            string title = this.Title;
            return int.Parse(title.Split('.')[1]);
        }

        //如果有轻重标记，那么将鉴定到的肽段的理论m/z放到list[0]中，而对应的“对儿”的理论m/z放到list[1]中
        public List<double> get_masses()
        {
            List<double> masses = new List<double>();
            masses.Add(0.0);
            masses.Add(0.0);
            int label_index1 = int.Parse(this.Pep_flag);
            int label_index2 = (label_index1 == 1 ? 2 : 1);
            for (int i = 0; i < this.Sq.Length; ++i)
            {
                masses[0] += Config_Help.mass_index[label_index1, this.Sq[i] - 'A'];
                masses[1] += Config_Help.mass_index[label_index2, this.Sq[i] - 'A'];
            }
            List<int> mod_flag = new List<int>();
            List<string> mod_names = Modification.get_modSites_list(this.Mod_sites, ref mod_flag);
            for (int i = 0; i < mod_names.Count; ++i)
            {
                masses[0] += ((double[])Config_Help.modStr_hash[mod_names[i]])[mod_flag[i]];
                masses[1] += ((double[])Config_Help.modStr_hash[mod_names[i]])[mod_flag[i]]; //等待超哥将pQuant修改完再进行修改
            }
            masses[0] = (masses[0] + Config_Help.massH2O + get_charge() * Config_Help.massZI) / get_charge();
            masses[1] = (masses[1] + Config_Help.massH2O + get_charge() * Config_Help.massZI) / get_charge();
            return masses;
        }

        public double get_silac_mass()
        {
            int K_count = 0;
            int R_count = 0;
            double K_mass = 8.014199;
            double R_mass = 10.008269;
            for (int i = 0; i < this.Sq.Length; ++i)
            {
                if (this.Sq[i] == 'K')
                    ++K_count;
                else if (this.Sq[i] == 'R')
                    ++R_count;
            }
            double mass = K_count * K_mass + R_count * R_mass;
            mass = mass / get_charge();
            if (this.Pep_flag == "0") //轻标，返回重标+
                return mass;
            return -mass;
        }

        //获取氨基酸aa中元素element_name的个数
        private int get_Aa_element_number(int index, string element_name, char aa)
        {
            Aa one_aa = Config_Help.aas[index, aa - 'A'];
            for (int i = 0; i < one_aa.elements.Count; ++i)
            {
                if (one_aa.elements[i] == element_name)
                    return one_aa.numbers[i];
            }
            return 0;
        }
        //获取修饰名为mod_name中元素element_name的个数
        private int get_Modification_element_number(int index, string element_name, string mod_name)
        {
            Aa[] aas = Config_Help.modStr_elements_hash[mod_name] as Aa[];
            for (int i = 0; i < aas[index].elements.Count; ++i)
            {
                if (aas[index].elements[i] == element_name)
                    return aas[index].numbers[i];
            }
            return 0;
        }
        public double get_N15_mass() //该函数可能有问题，没有使用过该函数
        {
            int N_number = 0;
            const double N15_N14 = 15.0001088 - 14.0030732;
            for (int i = 0; i < this.Sq.Length; ++i)
            {
                N_number += get_Aa_element_number(1, "N", this.Sq[i]);
            }
            List<string> mod_names = Modification.get_modSites_list(this.Mod_sites);
            for (int i = 0; i < mod_names.Count; ++i)
            {
                N_number += get_Modification_element_number(1, "N", mod_names[i]);
            }
            if (this.Pep_flag == "1") //轻标，返回重标+
                return N_number * N15_N14 / get_charge();
            return -N_number * N15_N14 / get_charge();
        }
        public int get_N15_number()
        {
            int N_number = 0;
            for (int i = 0; i < this.Sq.Length; ++i)
            {
                N_number += get_Aa_element_number(1, "N", this.Sq[i]);
            }
            List<string> mod_names = Modification.get_modSites_list(this.Mod_sites);
            for (int i = 0; i < mod_names.Count; ++i)
            {
                N_number += get_Modification_element_number(0, "N", mod_names[i]);
            }
            return N_number;
        }
        public static PSM getPSM_BySpectra(Spectra spec)
        {
            PSM psm = new PSM(0, spec.Title, "", "", 0.0, spec.Pepmass, 0.0, 0.0, 0.0, 0.0, ' ', "0", true, "", 0);
            psm.Charge = spec.Charge;
            psm.Spectra_mass = spec.Pepmass * spec.Charge - Config_Help.massZI * (spec.Charge - 1);
            return psm;
        }
        public static string ToHeaderWithRatio()
        {
            string header = ToHeaderNoRatio();
            header += "\tRatio";
            header += "\tSigma";
            return header;
        }
        public static string ToHeaderWithRatio2()
        {
            string header = ToHeaderNoRatio();
            header += "\tMS2_ratio";
            header += "\tMS2_ratio_" + Config_Help.MS2_Match_Ion_Type;
            header += "\t" + Config_Help.MS2_Match_Ion_Type + "_Intensity";
            return header;
        }
        public static string ToHeaderNoRatio()
        {
            string header = "#";
            header += "\tTitle";
            header += "\tCharge";
            header += "\tSq";
            header += "\tMod_Sites";
            header += "\tScore";
            header += "\tSpectra Mass";
            header += "\tQ value";
            header += "\tTheory Sq Mass";
            header += "\tDelta Mass";
            header += "\tDelta Mass (PPM)";
            header += "\tSpecific Flag";
            header += "\tLabel Flag";
            header += "\tTarget_Decoy";
            header += "\tProtein AC";
            return header;
        }
        public string ToStringNoRatio()
        {
            string information = "";
            information += this.Id;
            information += "\t" + this.Title;
            information += "\t" + this.Charge;
            information += "\t" + this.Sq;
            information += "\t" + this.Mod_sites;
            information += "\t" + this.Score.ToString("E2");
            information += "\t" + this.Spectra_mass.ToString("F2");
            information += "\t" + this.Q_value.ToString("F5");
            information += "\t" + this.Theory_sq_mass.ToString("F2");
            information += "\t" + this.Delta_mass.ToString("F5");
            information += "\t" + this.Delta_mass_PPM.ToString("F2");
            information += "\t" + this.Specific_flag;
            information += "\t" + this.Pep_flag;
            information += "\t" + this.Is_target_flag;
            information += "\t" + this.AC;
            return information;
        }
        public string ToStringWithRatio()
        {
            string information = ToStringNoRatio();
            information += "\t" + this.Ratio;
            information += "\t" + this.Sigma;
            return information;
        }
        public string ToStringWithRatio2()
        {
            string information = ToStringNoRatio();
            information += "\t" + this.Ms2_ratio.ToString("F5");
            information += "\t" + this.Ms2_ratio_a1.ToString("F5");
            information += "\t" + this.A1_intensity.ToString("E2");
            return information;
        }
    }
}
