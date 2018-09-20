using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild.pLink
{
    public class pLink_Result
    {
        public ObservableCollection<pLink.PSM> psms { get; set; }
        public ObservableCollection<Spectra> spectra { get; set; }
        public Hashtable title_index { get; set; }
        public string Link_Name { get; set; }
        public double Link_mass { get; set; }
        public List<string> Link_Names { get; set; }
        public List<double> Link_masses { get; set; }
        public pLink_Label pLink_label { get; set; }

        public pLink_Result()
        {
            this.psms = new ObservableCollection<pLink.PSM>();
            spectra = new ObservableCollection<Spectra>();
            title_index = new Hashtable();
            Link_Names = new List<string>();
            Link_masses = new List<double>();
            pLink_label = new pLink_Label();
        }
    }
}
