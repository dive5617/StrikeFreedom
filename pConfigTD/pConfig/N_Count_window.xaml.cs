using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace pConfig
{
    /// <summary>
    /// N_Count_window.xaml 的交互逻辑
    /// </summary>
    public partial class N_Count_window : Window
    {
        public MainWindow mainW;
        public N_Count_window(MainWindow mainW)
        {
            InitializeComponent();
            this.mainW = mainW;
        }

        private void get_N_count_clk(object sender, RoutedEventArgs e)
        {
            string sq = sq_txt.Text;
            string modification_str = mod_txt.Text;
            int N_count = 0;
            for (int i = 0; i < sq.Length; ++i)
            {
                Amino_Acid aa = mainW.aas[sq[i] - 'A'];
                List<Element_composition> ecs = aa.Element_composition;
                for (int j = 0; j < ecs.Count; ++j)
                {
                    if (ecs[j].Element_name == "N")
                        N_count += ecs[j].Element_number;
                }
            }
            System.Collections.Hashtable modStr_Modification_hash = new System.Collections.Hashtable();
            for (int i = 0; i < this.mainW.modifications.Count; ++i)
            {
                modStr_Modification_hash[this.mainW.modifications[i].Name] = i;
            }
            List<Modification> modifications = new List<Modification>();
            string[] strs = modification_str.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < strs.Length; ++i)
            {
                modifications.Add(this.mainW.modifications[(int)modStr_Modification_hash[strs[i]]]);
            }
            for (int i = 0; i < modifications.Count; ++i)
            {
                List<Element_composition> ecs = modifications[i].parse_element_composition();
                for (int j = 0; j < ecs.Count; ++j)
                {
                    if (ecs[j].Element_name == "N")
                        N_count += ecs[j].Element_number;
                }
            }
            this.N_count_txt.Text = N_count + "";
        }

        private void close_event(object sender, EventArgs e)
        {
            if(this.mainW.tab_control.SelectedIndex != 4)
                this.mainW.aas = null;
            if (this.mainW.tab_control.SelectedIndex != 1)
                this.mainW.modifications = null;
        }

        private void get_C_count_clk(object sender, RoutedEventArgs e)
        {
            string sq = sq_txt.Text;
            string modification_str = mod_txt.Text;
            int C_count = 0;
            for (int i = 0; i < sq.Length; ++i)
            {
                Amino_Acid aa = mainW.aas[sq[i] - 'A'];
                List<Element_composition> ecs = aa.Element_composition;
                for (int j = 0; j < ecs.Count; ++j)
                {
                    if (ecs[j].Element_name == "C")
                        C_count += ecs[j].Element_number;
                }
            }
            System.Collections.Hashtable modStr_Modification_hash = new System.Collections.Hashtable();
            for (int i = 0; i < this.mainW.modifications.Count; ++i)
            {
                modStr_Modification_hash[this.mainW.modifications[i].Name] = i;
            }
            List<Modification> modifications = new List<Modification>();
            string[] strs = modification_str.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < strs.Length; ++i)
            {
                modifications.Add(this.mainW.modifications[(int)modStr_Modification_hash[strs[i]]]);
            }
            for (int i = 0; i < modifications.Count; ++i)
            {
                List<Element_composition> ecs = modifications[i].parse_element_composition();
                for (int j = 0; j < ecs.Count; ++j)
                {
                    if (ecs[j].Element_name == "C")
                        C_count += ecs[j].Element_number;
                }
            }
            this.N_count_txt.Text = C_count + "";
        }
    }
}
