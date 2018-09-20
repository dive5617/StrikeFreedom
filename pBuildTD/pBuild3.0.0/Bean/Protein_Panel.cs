using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class Protein_Panel
    {
        public ObservableCollection<Protein> identification_proteins;
        public ObservableCollection<Protein_Group> identification_protein_groups;
        public Protein_Display_Detail_Help pddh;

        public Protein_Panel()
        {
            this.pddh = new Protein_Display_Detail_Help();
            this.identification_proteins = new ObservableCollection<Protein>();
            this.identification_protein_groups = new ObservableCollection<Protein_Group>();
        }
    }
}
