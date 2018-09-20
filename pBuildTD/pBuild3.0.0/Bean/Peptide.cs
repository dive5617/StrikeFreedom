using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class Peptide // : IComparable
    {
        public string Sq { get; set; }
        public double Pepmass { get; set; }
        public ObservableCollection<Modification> Mods { get; set; }

        public int Tag_Flag { get; set; } //候选的肽段所使用的标记类型
        //public double Score { get; set; } // 候选的肽段的打分,最后按照该打分进行排序

        public Peptide()
        {
            this.Sq = "";
            this.Pepmass = 0.0;
            this.Tag_Flag = 0;
            Mods = new ObservableCollection<Modification>();
        }
        public Peptide(string sq)
        {
            Mods = new ObservableCollection<Modification>();
            this.Sq = sq;
        }
        public Peptide(Peptide pp)
        {
            this.Sq = pp.Sq;
            this.Pepmass = pp.Pepmass;
            this.Mods = new ObservableCollection<Modification>(pp.Mods);
            this.Tag_Flag = pp.Tag_Flag;
        }

        //public int CompareTo(object obj)
        //{
        //    Peptide pep = (Peptide)obj;
        //    if (this.Score > pep.Score)
        //        return 1;
        //    else if (this.Score < pep.Score)
        //        return -1;
        //    return 0;
        //}
        //获取该肽段中，element_name元素个数
        public int Get_Number_ByElementName(string element_name)
        {
            int element_number = 0;
            for (int i = 0; i < this.Sq.Length; ++i)
            {
                Aa aa = Config_Help.aas[1, this.Sq[i] - 'A'];
                for (int j = 0; j < aa.elements.Count; ++j)
                {
                    if (aa.elements[j] == element_name)
                        element_number += aa.numbers[j];
                }
            }
            for (int i = 0; i < this.Mods.Count; ++i)
            {
                Aa[] aas = Config_Help.modStr_elements_hash[this.Mods[i].Mod_name] as Aa[];
                Aa aa = aas[0];
                for (int j = 0; j < aa.elements.Count; ++j)
                {
                    if (aa.elements[j] == element_name)
                        element_number += aa.numbers[j];
                }
            }
            return element_number;
        }
        public double getAAMass(int start, int end)
        {
            List<double> modMass = new List<double>();
            for (int i = 0; i < this.Sq.Length + 2; ++i)
            {
                modMass.Add(0.0);
            }
            for (int i = 0; i < this.Mods.Count; ++i)
            {
                modMass[this.Mods[i].Index] = this.Mods[i].Mass;
            }
            double mass = modMass[0];
            for (int i = start; i <= end; ++i)
                mass += Config_Help.mass_index[this.Tag_Flag, this.Sq[i] - 'A'] + modMass[i + 1];
            return mass;
        }
        public void update() //this.Pepmass等于残基和+H2O
        {
            int label_index = this.Tag_Flag;
            double mass = Config_Help.massH2O;
            for (int i = 0; i < this.Sq.Length; ++i)
            {
                mass += Config_Help.mass_index[label_index, this.Sq[i] - 'A'];
            }
            for (int i = 0; i < this.Mods.Count; ++i)
            {
                mass += this.Mods[i].Mass;
            }
            this.Pepmass = mass;
        }
    }
    public class Modification
    {
        public int Index { get; set; }
        public double Mass { get; set; }
        public string Mod_name { get; set; }
        public List<double> Mass_Loss { get; set; } //一些中性丢失的质量，比如磷酸化等
        public int Flag_Index { get; set; } //表示该修饰对应的修饰表

        public Modification(int index, double mass)
        {
            this.Index = index;
            this.Mass = mass;
        }
        public Modification(int index, string mod_name)
        {
            this.Index = index;
            this.Mod_name = mod_name;
            this.Flag_Index = 0;
        }
        public Modification(int index, string mod_name, int flag_index)
        {
            this.Index = index;
            this.Mod_name = mod_name;
            this.Flag_Index = flag_index;
        }
        public Modification(int index, double mass, string mod_name)
        {
            this.Index = index;
            this.Mass = mass;
            this.Mod_name = mod_name;
        }
        public Modification(int index, double mass, string mod_name, List<double> mass_loss)
        {
            this.Index = index;
            this.Mass = mass;
            this.Mod_name = mod_name;
            this.Mass_Loss = mass_loss;
        }
        public Modification(int index, double mass, string mod_name, List<double> mass_loss, int flag_index)
        {
            this.Index = index;
            this.Mass = mass;
            this.Mod_name = mod_name;
            this.Mass_Loss = mass_loss;
            this.Flag_Index = flag_index;
        }
        public static ObservableCollection<Modification> get_modSites(string modStr)
        {
            ObservableCollection<Modification> modifications = new ObservableCollection<Modification>();
            if (modStr.Contains('#')) //pFind
            {
                string[] strs = modStr.Split(new char[] { ',', ';', '#' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 1; i < strs.Length; i += 3)
                {
                    Modification modification = new Modification(int.Parse(strs[i - 1]), strs[i], int.Parse(strs[i + 1]));
                    modifications.Add(modification);
                }
            }
            else //pNovo
            {
                string[] strs = modStr.Split(new char[] { ',', ';'}, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 1; i < strs.Length; i += 2)
                {
                    Modification modification = new Modification(int.Parse(strs[i - 1]), strs[i]);
                    modifications.Add(modification);
                }
            }
            return modifications;
        }
        
        public static string get_modSites(ObservableCollection<Modification> mods, int pFind_pNovo = 0)
        {
            string mod_sites = "";
            if (pFind_pNovo == 0) //pFind
            {
                for (int i = 0; i < mods.Count; ++i)
                    mod_sites += mods[i].Index + "," + mods[i].Mod_name + "#" + mods[i].Flag_Index + ";";
            }
            else //pNovo
            {
                for (int i = 0; i < mods.Count; ++i)
                    mod_sites += mods[i].Index + "," + mods[i].Mod_name + ";";
            }
            return mod_sites;
        }
        public static ObservableCollection<Modification> get_modSites(string sq, string modStr, int mass_index, ref double pepmass)
        {
            pepmass = Config_Help.massH2O + Config_Help.massZI;
            for (int i = 0; i < sq.Length; ++i)
            {
                pepmass += Config_Help.mass_index[mass_index, sq[i] - 'A'];
            }
            ObservableCollection<Modification> mods = new ObservableCollection<Modification>();
            string[] all_mod = modStr.Split(';');

            for (int i = 0; i < all_mod.Length - 1; ++i)
            {
                string[] one_mod = all_mod[i].Split(',');
                int index = int.Parse(one_mod[0]);
                string[] strs = one_mod[1].Split('#');
                if (Config_Help.modStr_hash[strs[0]] != null)
                {
                    int index0 = 0;
                    if (strs.Length > 1)
                        index0 = int.Parse(strs[1]);
                    pepmass += ((double[])Config_Help.modStr_hash[strs[0]])[index0];
                    mods.Add(new Modification(index, ((double[])Config_Help.modStr_hash[strs[0]])[index0], strs[0],
                        (List<double>)Config_Help.modStrLoss_hash[strs[0]], index0));
                }
                else
                {
                    System.Windows.MessageBox.Show("The modification is unknown.");
                    if (System.Windows.Application.Current != null)
                        System.Windows.Application.Current.Shutdown();
                }
            }
            return mods;
        }
        public static List<string> get_modSites_list(string modStr)
        {
            List<int> mod_flag = new List<int>();
            return get_modSites_list(modStr, ref mod_flag);
        }
        public static List<string> get_modSites_list(string modStr, ref List<int> mod_flag)
        {
            mod_flag = new List<int>();
            List<string> mods = new List<string>();
            string[] all_mod = modStr.Split(';');
            for (int i = 0; i < all_mod.Length - 1; ++i)
            {
                string[] one_mod = all_mod[i].Split(',');
                string[] strs = one_mod[1].Split('#');
                mods.Add(strs[0]);
                mod_flag.Add(int.Parse(strs[1]));
            }
            return mods;
        }
        public static List<Protein_Modification> get_modSites_list(int start, int peptide_length, string modStr, Protein protein)
        {
            List<Protein_Modification> mods = new List<Protein_Modification>();
            string[] all_mod = modStr.Split(';');
            for (int i = 0; i < all_mod.Length - 1; ++i)
            {
                string[] one_mod = all_mod[i].Split(',');
                int peptide_index = int.Parse(one_mod[0]);
                if (peptide_index == 0)
                    peptide_index = 1;
                else if (peptide_index == peptide_length + 1)
                    peptide_index = peptide_length;
                string mod_name = one_mod[1].Split('#')[0];
                Protein_Mod mod_tmp = new Protein_Mod(mod_name);
                mods.Add(new Protein_Modification(start + peptide_index - 1, protein.modification.IndexOf(mod_tmp)));
            }
            return mods;
        }
        
    }
    public class Protein_Modification : IComparable
    {
        public int mod_site; //修饰在整个蛋白上的位置
        public int mod_index; //修饰在蛋白protein中的modification中的索引位置，用来获取修饰名字

        public Protein_Modification(int mod_site, int mod_index)
        {
            this.mod_site = mod_site;
            this.mod_index = mod_index;
        }

        int IComparable.CompareTo(Object obj)
        {
            Protein_Modification temp = (Protein_Modification)obj;
            if (this.mod_site < temp.mod_site)
                return -1;
            else if (this.mod_site > temp.mod_site)
                return 1;
            return 0;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Protein_Modification temp = obj as Protein_Modification;
            if (temp == null) return false;
            else return Equals(temp);
        }
        public override int GetHashCode()
        {
            return this.mod_site.GetHashCode();
        }
        public bool Equals(Protein_Modification other)
        {
            if (other == null) return false;
            return (this.mod_site.Equals(other.mod_site)); // && this.mod_index.Equals(other.mod_index)
        }
    }
}
