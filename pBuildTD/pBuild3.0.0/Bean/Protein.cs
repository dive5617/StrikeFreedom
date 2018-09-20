using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class Protein : INotifyPropertyChanged, IComparable<Protein>
    {
        public int ID { get; set; }
        public string AC { get; set; }
        public string DE { get; set; }
        public string SQ { get; set; }

        public double Score { get; set; }
        public double Q_value { get; set; }
        public double Coverage { get; set; }
        public int PSM_num { get; set; }
        public int Same_set_num { get; set; }
        public int Sub_set_num { get; set; }
        public bool Have_Distinct_PSM { get; set; }
        public double MS2_Ratio { get; set; }
        public double MS2_Ratio_a1 { get; set; }
        //
        public string Parent_Protein_AC { get; set; } //如果该蛋白是某个蛋白的SameSet或者是SubSet，那么就显示它对应的蛋白的AC
        public string Same_Sub_Flag { get; set; } //用来表示是SameSet还是SubSet，否则就显示“Group”表示是蛋白Group
        public List<int> Protein_index { get; set; } //如果该蛋白是Group（代表蛋白），那么将该蛋白对应的SameSet或SubSet蛋白的索引放入该数组中

        //蛋白质的定量比值和Sigma
        public double Ratio { get; set; }
        public double Sigma { get; set; }

        //蛋白质被鉴定到的子串对应于PSM列表中的索引号
        public List<int> psm_index { get; set; }

        //蛋白质被鉴定到的修饰列表
        public List<Protein_Mod> modification;

        public List<Protein_Fragment> fragments;

        public Protein(string AC, string DE)
        {
            this.AC = AC;
            this.DE = DE;
            this.psm_index = new List<int>();
            this.modification = new List<Protein_Mod>();
        }
        public Protein(string AC, string DE, string SQ)
        {
            this.AC = AC;
            this.DE = DE;
            this.SQ = SQ;
            this.psm_index = new List<int>();
            this.modification = new List<Protein_Mod>();
        }
        public Protein(int ID, string AC, string DE, string SQ, List<int> psm_index)
        {
            this.ID = ID;
            this.AC = AC;
            this.DE = DE;
            this.SQ = SQ;
            this.psm_index = psm_index;
            this.modification = new List<Protein_Mod>();
        }
        public Protein(int ID, string AC, string DE, string SQ, List<int> psm_index, double Ratio, double Sigma)
        {
            this.ID = ID;
            this.AC = AC;
            this.DE = DE;
            this.SQ = SQ;
            this.psm_index = psm_index;
            this.Ratio = Ratio;
            this.Sigma = Sigma;
            this.modification = new List<Protein_Mod>();
        }

        public static Protein getProteinByAC(ObservableCollection<Protein> proteins, string ac)
        {
            for (int i = 0; i < proteins.Count; ++i)
            {
                if (proteins[i].AC == ac)
                    return proteins[i];
            }
            return null;
        }

        public int CompareTo(Protein other)
        {
            Protein temp = (Protein)other;
            if (string.Compare(this.AC, temp.AC) < 0)
                return -1;
            else if (string.Compare(this.AC, temp.AC) > 0)
                return 1;
            return 0;
        }
        public bool Is_target_flag()
        {
            if (this.AC.StartsWith("REV_"))
                return false;
            return true;
        }
        public bool Is_Contaminant()
        {
            if (this.AC.StartsWith("CON_"))
                return true;
            return false;
        }
        public static string ToHeaderNoRatio()
        {
            string header = "#";
            header += "\tAC";
            header += "\tDE";
            header += "\tSQ Length";
            header += "\tPSM Count";
            header += "\tCoverage";
            header += "\tScore";
            header += "\tGroup";
            header += "\tFlag";
            return header;
        }
        public static string ToHeaderWithRatio()
        {
            string header = ToHeaderNoRatio();
            header += "\tRatio";
            return header;
        }
        public static string ToHeaderWithRatio2()
        {
            string header = ToHeaderNoRatio();
            header += "\tMS2 Ratio";
            header += "\tMS2 Ratio a1";
            return header;
        }
        public string ToStringNoRatio()
        {
            string information = this.ID + "";
            information += "\t" + this.AC;
            information += "\t" + this.DE;
            information += "\t" + this.SQ.Length;
            information += "\t" + this.psm_index.Count;
            information += "\t" + this.Coverage.ToString("P1");
            information += "\t" + this.Score.ToString("F2");
            information += "\t" + this.Parent_Protein_AC;
            information += "\t" + this.Same_Sub_Flag;
            return information;
        }
        public string ToStringWithRatio()
        {
            string information = ToStringNoRatio();
            information += "\t" + this.Ratio.ToString("F2");
            return information;
        }
        public string ToStringWithRatio2()
        {
            string information = ToStringNoRatio();
            information += "\t" + this.MS2_Ratio.ToString("F2");
            information += "\t" + this.MS2_Ratio_a1.ToString("F2");
            return information;
        }
        public override string ToString() { return this.ID.ToString(); }
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
    public class Protein_Mod : IComparable
    {
        public string modification;
        public int mod_count;

        public Protein_Mod(string modification)
        {
            this.modification = modification;
            this.mod_count = 1;
        }
        int IComparable.CompareTo(Object obj)
        {
            Protein_Mod temp = (Protein_Mod)obj;
            if (this.mod_count < temp.mod_count)
                return 1;
            else if (this.mod_count > temp.mod_count)
                return -1;
            return 0;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Protein_Mod temp = obj as Protein_Mod;
            if (temp == null) return false;
            else return Equals(temp);
        }
        public override int GetHashCode()
        {
            return this.modification.GetHashCode();
        }
        public bool Equals(Protein_Mod other)
        {
            if (other == null) return false;
            return (this.modification.Equals(other.modification)); // && this.mod_index.Equals(other.mod_index)
        }

    }
}
