using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pConfig
{
    public class Modification : IComparable
    {
        public string Name { get; set; } //修饰的名字
        public bool Is_common { get; set; } //是否是常见修饰
        public string Mod_site { get; set; } //修饰发生在哪些氨基酸上
        public string Position { get; set; } //修饰发生在肽段或者蛋白的位置，N端——C端 ——NORMAL
        public string Position_Display { get; set; } //Datagrid中显示的Position
        public double Mod_mass { get; set; } //修饰的质量
        public ObservableCollection<double> Neutral_loss { get; set; } //有没有中性丢失
        public string Neutral_loss_str { get; set; }
        public string Composition { get; set; } //该修饰是由于哪元素组成

        public Modification() 
        {
            this.Neutral_loss = new ObservableCollection<double>();
        }
        public List<Element_composition> parse_element_composition()
        {
            List<Element_composition> element_composition = new List<Element_composition>();
            string[] strs = this.Composition.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < strs.Length; i = i + 2)
            {
                string element_name = strs[i];
                int element_number = int.Parse(strs[i + 1]);
                element_composition.Add(new Element_composition(element_name, element_number, (int)Element.index_hash[element_name]));
            }
            return element_composition;
        }

        public Modification(string Name, bool Is_common, string Mod_site, string Position,
            double Mod_mass, string Composition)
        {
            this.Name = Name;
            this.Is_common = Is_common;
            this.Mod_site = Mod_site;
            this.Position = Position;
            this.Mod_mass = Mod_mass;
            this.Composition = Composition;
            this.Neutral_loss = new ObservableCollection<double>();
        }
        public Modification(string Name, bool Is_common, string Mod_site, string Position,
            double Mod_mass, string Composition, ObservableCollection<double> Netural_loss)
        {
            this.Name = Name;
            this.Is_common = Is_common;
            this.Mod_site = Mod_site;
            this.Position = Position;
            this.Mod_mass = Mod_mass;
            this.Composition = Composition;
            this.Neutral_loss = Netural_loss;
        }

        public void parse_netural_loss()
        {
            this.Neutral_loss_str = "";
            for (int i = 0; i < Neutral_loss.Count; ++i)
            {
                this.Neutral_loss_str += Neutral_loss[i] + ";";
            }
        }

        int IComparable.CompareTo(Object obj)
        {
            Modification temp = (Modification)obj;
            return this.Name.CompareTo(temp.Name);
        }

        public override bool Equals(object obj)
        {
            var modification = obj as Modification;
            if (modification == null)
                return false;
            if (this.Name == modification.Name)
                return true;
            return false;
        }
        public static bool operator ==(Modification a, Modification b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Name == b.Name;
        }
        public static bool operator !=(Modification a, Modification b)
        {
            return !(a == b);
        }
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }
}
