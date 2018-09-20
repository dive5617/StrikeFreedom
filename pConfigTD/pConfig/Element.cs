using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pConfig
{
    public class Element : IComparable
    {
        public static Hashtable index_hash = new Hashtable();
        public string Name { get; set; }
        public List<double> Mass { get; set; }
        public List<double> Ratio { get; set; }
        public double MMass { get; set; } //比例最大的质量
        public string All_Mass_Str { get; set; } //显示所有的质量及比例，格式为mass1,ratio1;mass2,ratio2;

        public Element()
        {
            this.Mass = new List<double>();
            this.Ratio = new List<double>();
            this.All_Mass_Str = "";
        }
        public Element(string Name)
        {
            this.Name = Name;
            this.Mass = new List<double>();
            this.Ratio = new List<double>();
            this.All_Mass_Str = "";
        }
        public void update_mass_str()
        {
            for (int i = 0; i < Mass.Count; ++i)
            {
                this.All_Mass_Str += Mass[i].ToString("F6") + "," + Ratio[i].ToString("P2") + ";";
            }
        }

        int IComparable.CompareTo(Object obj)
        {
            Element temp = (Element)obj;
            return this.Name.CompareTo(temp.Name);
        }

        public override bool Equals(object obj)
        {
            var element = obj as Element;
            if (element == null)
                return false;
            if (this.Name == element.Name)
                return true;
            return false;
        }
        public static bool operator ==(Element a, Element b)
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
        public static bool operator !=(Element a, Element b)
        {
            return !(a == b);
        }
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }

    public class Element_composition
    {
        public string Element_name { get; set; }
        public int Element_number { get; set; }

        public int Element_index { get; set; } //在总元素表中的索引位置

        public Element_composition(string Element_name, int Element_number, int Element_index)
        {
            this.Element_name = Element_name;
            this.Element_number = Element_number;
            this.Element_index = Element_index;
        }

        public static List<Element_composition> parse(MainWindow mainW, string composition, ref double mass)
        {
            mass = 0.0;
            List<Element_composition> element_compositions = new List<Element_composition>();
            string[] strs = composition.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < strs.Length; i = i + 2)
            {
                string name = strs[i];
                int number = int.Parse(strs[i + 1]);
                int element_index = (int)Element.index_hash[name];
                element_compositions.Add(new Element_composition(name, number, element_index));
                mass += number * (mainW.elements[element_index].MMass);
            }
            return element_compositions;
        }
    }
}
