using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pConfig
{
    public class Amino_Acid
    {
        public string Name { get; set; }
        public List<Element_composition> Element_composition { get; set; }
        public string Composition { get; set; }
        public double Mass { get; set; }

        public Amino_Acid(string Name)
        {
            this.Name = Name;
            this.Element_composition = new List<Element_composition>();
        }
    }
}
