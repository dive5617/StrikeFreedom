using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class Protein_Group
    {
        public Protein Protein { get; set; }
        public ObservableCollection<Protein_Group> Protein_Children { get; set; }

        public Protein_Group(Protein pro)
        {
            this.Protein = pro;
            this.Protein_Children = new ObservableCollection<Protein_Group>();
        }
    }
}
