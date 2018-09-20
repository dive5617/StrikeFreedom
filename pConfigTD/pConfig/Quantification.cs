using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pConfig
{
    public class Quantification : IComparable
    {
        public string Name { get; set; }
        public string All_quant_str { get; set; }
        public List<Quant_Simple> All_quant { get; set; }

        public Quantification(string Name)
        {
            this.Name = Name;
            this.All_quant = new List<Quant_Simple>();
        }
        public Quantification(string Name, string All_quant_str)
        {
            this.Name = Name;
            this.All_quant_str = All_quant_str;
            this.All_quant = new List<Quant_Simple>();
        }

        public static string get_string(string Name, List<Quant_Simple> All_quant)
        {
            string res = "";
            for (int i = 0; i < All_quant.Count; ++i)
            {
                res += "R:" + All_quant[i].AA + "{" + All_quant[i].Label0 + "," + All_quant[i].Label1 + "}";
            }
            return res;
        }
        int IComparable.CompareTo(Object obj)
        {
            Quantification temp = (Quantification)obj;
            return this.Name.CompareTo(temp.Name);
        }

        public override bool Equals(object obj)
        {
            var quantification = obj as Quantification;
            if (quantification == null)
                return false;
            if (this.Name == quantification.Name)
                return true;
            return false;
        }
        public static bool operator ==(Quantification a, Quantification b)
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
        public static bool operator !=(Quantification a, Quantification b)
        {
            return !(a == b);
        }
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }
    public class Quant_Simple
    {
        public char AA { get; set; }
        public string Label0 { get; set; }
        public string Label1 { get; set; }

        public Quant_Simple(char AA, string Label0, string Label1)
        {
            this.AA = AA;
            this.Label0 = Label0;
            this.Label1 = Label1;
        }
    }
}
