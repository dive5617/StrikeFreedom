using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class Mod_Site : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Aa { get; set; }
        public ObservableCollection<string> Mods { get; set; }
        private string mod;
        public string Mod
        {
            get { return mod; }
            set {
                mod = value;
                NotifyPropertyChanged("Mod");
                update_element();
            }
        }
        public ObservableCollection<string> Mod_Flags { get; set; }
        public string Mod_Flag { get; set; }
        private string element;
        public string Element 
        {
            get { return element; }
            set
            {
                element = value;
                NotifyPropertyChanged("Element");
            }
        }
        private string mass;
        public string Mass
        {
            get { return mass; }
            set
            {
                mass = value;
                NotifyPropertyChanged("Mass");
            }
        }
        public Mod_Site(int id, string aa, ObservableCollection<string> mods, ObservableCollection<string> mod_flags)
        {
            this.Id = id;
            this.Aa = aa;
            this.Mods = mods;
            this.Mod_Flags = mod_flags;
        }
        public Mod_Site(int id, string aa, ObservableCollection<string> mods, string mod, ObservableCollection<string> mod_flags, string mod_flag)
        {
            this.Id = id;
            this.Aa = aa;
            this.Mods = mods;
            this.Mod = mod;
            this.Mod_Flags = mod_flags;
            this.Mod_Flag = mod_flag;
            update_element();
        }
        private void update_element()
        {
            if (this.Mod == "")
            {
                this.Element = "";
                this.Mass = "";
                return;
            }
            int index = Config_Help.get_label_index(this.Mod_Flag);
            Aa[] aas = Config_Help.modStr_elements_hash[this.Mod] as Aa[];
            double[] masses = Config_Help.modStr_hash[this.Mod] as double[];
            if (aas == null || aas[index] == null || masses == null || masses[index] == null)
                return;
            this.Element = pBuild.Aa.parse_String_byAa(aas[index]);
            this.Mass = masses[index].ToString("F2");
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
