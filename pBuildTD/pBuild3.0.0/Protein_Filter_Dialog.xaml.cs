using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace pBuild
{
    /// <summary>
    /// Protein_Filter_Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class Protein_Filter_Dialog : Window
    {
        MainWindow mainW;
        public Protein_Filter_Dialog(MainWindow mainW)
        {
            InitializeComponent();
            this.mainW = mainW;
        }

        private ObservableCollection<Protein> filter_by_AC(string filter_string, ObservableCollection<Protein> all_proteins)
        {
            if (filter_string == "")
                return all_proteins;

            ObservableCollection<Protein> proteins = new ObservableCollection<Protein>();
            if (!filter_string.Contains("/")) //不包含/，说明是查找的单个蛋白
            {
                for (int i = 0; i < all_proteins.Count; ++i)
                {
                    if (all_proteins[i].AC.Contains(filter_string))
                    {
                        proteins.Add(all_proteins[i]);
                    }
                }
            }
            else //如果包含/，说明查找多个蛋白，需要将所有这些蛋白全部显示
            {
                string[] acs = filter_string.Split('/');
                for (int i = 0; i < all_proteins.Count; ++i)
                {
                    if (acs.Contains(all_proteins[i].AC))
                    {
                        proteins.Add(all_proteins[i]);
                    }
                }
            }
            return proteins;
        }
        private ObservableCollection<Protein> filter_by_DE(string filter_string, ObservableCollection<Protein> all_proteins)
        {
            if (filter_string == "")
                return all_proteins;
            ObservableCollection<Protein> proteins = new ObservableCollection<Protein>();
            for (int i = 0; i < all_proteins.Count; ++i)
            {
                if (all_proteins[i].DE.Contains(filter_string))
                {
                    proteins.Add(all_proteins[i]);
                }
            }
            return proteins;
        }
        private ObservableCollection<Protein> filter_by_Group(string filter_string, ObservableCollection<Protein> all_proteins)
        {
            if (filter_string == "")
                return all_proteins;
            ObservableCollection<Protein> proteins = new ObservableCollection<Protein>();
            for (int i = 0; i < all_proteins.Count; ++i)
            {
                if (all_proteins[i].Parent_Protein_AC != null && all_proteins[i].Parent_Protein_AC.Contains(filter_string))
                {
                    proteins.Add(all_proteins[i]);
                }
            }
            return proteins;
        }
        private ObservableCollection<Protein> filter_by_Pep_number(int pep_number, ObservableCollection<Protein> all_proteins)
        {
            if (pep_number <= 1)
                return all_proteins;
            ObservableCollection<Protein> proteins = new ObservableCollection<Protein>();
            for (int i = 0; i < all_proteins.Count; ++i)
            {
                HashSet<string> sets = new HashSet<string>();
                List<int> psms_index = all_proteins[i].psm_index;
                for (int j = 0; j < psms_index.Count; ++j)
                {
                    string ac = mainW.psms[psms_index[j]].AC;
                    string[] strs = ac.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (strs.Length > 1) //如果不是Unique的序列，则不处理
                        continue;
                    string sq_mod = mainW.psms[psms_index[j]].Sq + "#" + mainW.psms[psms_index[j]].Mod_sites;
                    sets.Add(sq_mod);
                }
                if(sets.Count>=pep_number)
                    proteins.Add(all_proteins[i]);
            }
            return proteins;
        }
        private ObservableCollection<Protein> filter_by_Sq_number(int sq_number, ObservableCollection<Protein> all_proteins)
        {
            if (sq_number <= 1)
                return all_proteins;
            ObservableCollection<Protein> proteins = new ObservableCollection<Protein>();
            for (int i = 0; i < all_proteins.Count; ++i)
            {
                HashSet<string> sets = new HashSet<string>();
                List<int> psms_index = all_proteins[i].psm_index;
                for (int j = 0; j < psms_index.Count; ++j)
                {
                    string ac = mainW.psms[psms_index[j]].AC;
                    string[] strs = ac.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (strs.Length > 1) //如果不是Unique的序列，则不处理
                        continue;
                    string sq = mainW.psms[psms_index[j]].Sq;
                    sets.Add(sq);
                }
                if (sets.Count >= sq_number)
                    proteins.Add(all_proteins[i]);
            }
            return proteins;
        }
        private ObservableCollection<Protein> filter_byTarget(int target_index, ObservableCollection<Protein> all_proteins)
        {
            if (target_index == 0)
                return all_proteins;
            ObservableCollection<Protein> proteins = new ObservableCollection<Protein>();
            if (target_index == 1) //target
            {
                for (int i = 0; i < all_proteins.Count; ++i)
                {
                    if (all_proteins[i].Is_target_flag())
                        proteins.Add(all_proteins[i]);
                }
            }
            else //decoy
            {
                for (int i = 0; i < all_proteins.Count; ++i)
                {
                    if (!all_proteins[i].Is_target_flag())
                        proteins.Add(all_proteins[i]);
                }
            }
            return proteins;
        }
        private ObservableCollection<Protein> filter_byContaminant(bool isCon_need, ObservableCollection<Protein> all_proteins)
        {
            if (isCon_need)
                return all_proteins;
            ObservableCollection<Protein> proteins = new ObservableCollection<Protein>();
            for (int i = 0; i < all_proteins.Count; ++i)
            {
                if (!all_proteins[i].Is_Contaminant())
                    proteins.Add(all_proteins[i]);
            }
            return proteins;
        }
        private void filter_btn_clk(object sender, RoutedEventArgs e)
        {
            string ac = protein_filter_AC_tb.Text;
            string de = protein_filter_DE_tb.Text;
            string group = protein_filter_Group_tb.Text;
            int pep_number = 0, sq_number = 0;
            int target_index = this.target_comboBox.SelectedIndex;
            bool is_con = (bool)this.con_need_cbx.IsChecked;
            if (protein_peptide_number_tb.Text == "")
                pep_number = 0;
            else
            {
                if (!Config_Help.IsIntegerAllowed(protein_peptide_number_tb.Text))
                {

                    MessageBox.Show(Message_Help.PROTEIN_PEP_NUMBER);
                    return;
                }
                else
                    pep_number = int.Parse(protein_peptide_number_tb.Text);
            }
            if (protein_sq_number_tb.Text == "")
                sq_number = 0;
            else
            {
                if (!Config_Help.IsIntegerAllowed(protein_sq_number_tb.Text))
                {
                    MessageBox.Show(Message_Help.PROTEIN_PEP_NUMBER);
                    return;
                }
                else
                    sq_number = int.Parse(protein_sq_number_tb.Text);
            }

            ObservableCollection<Protein> proteins = mainW.protein_panel.identification_proteins;
            proteins = filter_by_AC(ac, proteins);
            proteins = filter_by_DE(de, proteins);
            proteins = filter_by_Group(group, proteins);
            proteins = filter_by_Pep_number(pep_number, proteins);
            proteins = filter_by_Sq_number(sq_number, proteins);
            proteins = filter_byTarget(target_index, proteins);
            proteins = filter_byContaminant(is_con, proteins);
            mainW.protein_data.ItemsSource = proteins;
            mainW.display_size.Text = proteins.Count.ToString();

            this.Close();
        }

        private void enter_keyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;
            filter_btn_clk(null, null);
        }

        private void closed_event(object sender, EventArgs e)
        {
            this.mainW.protein_filter_dialog = null;
        }
    }
}
