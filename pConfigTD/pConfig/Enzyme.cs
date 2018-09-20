using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pConfig
{
    public class Enzyme : IComparable
    {
        public string Name { get; set; }
        public string Cleave_site { get; set; }
        public string Ignore_site { get; set; }
        public string N_C { get; set; }

        public Enzyme(string Name, string Cleave_site, string Ignore_site, string N_C)
        {
            this.Name = Name;
            this.Cleave_site = Cleave_site;
            this.Ignore_site = Ignore_site;
            this.N_C = N_C;
        }

        int IComparable.CompareTo(Object obj)
        {
            Enzyme temp = (Enzyme)obj;
            return this.Name.CompareTo(temp.Name);
        }

        public override bool Equals(object obj)
        {
            var enzyme = obj as Enzyme;
            if (enzyme == null)
                return false;
            if (this.Name == enzyme.Name)
                return true;
            return false;
        }
        public static bool operator ==(Enzyme a, Enzyme b)
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
        public static bool operator !=(Enzyme a, Enzyme b)
        {
            return !(a == b);
        }
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }
}
