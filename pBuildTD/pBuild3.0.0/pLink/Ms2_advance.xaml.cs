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

namespace pBuild.pLink
{
    /// <summary>
    /// Ms2_advance.xaml 的交互逻辑
    /// </summary>
    public partial class Ms2_advance : Window
    {
        public MainWindow mainW;
        public PSM psm;
        int pep1_flag = 0, pep2_flag = 0, xlink_flag = 0;
        string xlink_label_name = "";
        const string non_mod = "";
        public ObservableCollection<Mod_Site> mod_Sites = new ObservableCollection<Mod_Site>();
        public ObservableCollection<Mod_Site> Mod_Sites
        {
            get { return mod_Sites; }
            set { mod_Sites = value; }
        }

        public Ms2_advance()
        {
            InitializeComponent();
        }

        public Ms2_advance(MainWindow mainW, PSM psm) //peptide_number=2 表示二肽交联，peptide_number=3 表示三肽交联
        {
            InitializeComponent();
            this.mainW = mainW;
            this.psm = psm;
            this.normal_ckb.IsChecked = PSM_Help_2.isNormal;
            this.internal_ckb.IsChecked = PSM_Help_2.isInternal;
            List<string> label_names = this.mainW.pLink_result.Link_Names;
            string[] pep_flag = this.psm.Pep_flag.Split('|');
            xlink_flag = int.Parse(pep_flag[1]) - 1;
            pep1_flag = int.Parse(pep_flag[0]) - 1;
            pep2_flag = int.Parse(pep_flag[2]) - 1;
            xlink_label_name = label_names[xlink_flag];
            this.pep1_label_cbx.SelectedIndex = pep1_flag;
            this.pep1_sq_tbx.Text = psm.Peptide[0].Sq;
            update_mods(psm.Peptide[0].Sq, psm.Peptide[0].Mods);
            this.pep1_mod_dg.ItemsSource = new ObservableCollection<Mod_Site>(this.Mod_Sites);
            update_links();
            this.pep2_label_cbx.SelectedIndex = pep2_flag;
            this.pep2_sq_tbx.Text = psm.Peptide[1].Sq;
            update_mods(psm.Peptide[1].Sq, psm.Peptide[1].Mods);
            this.pep2_mod_dg.ItemsSource = new ObservableCollection<Mod_Site>(this.Mod_Sites);
            this.link_position_tbx.Text = psm.Peptide_Link_Position[0] + "-" + psm.Peptide_Link_Position[1];
        }
        private void update_links()
        {
            for (int i = 0; i < this.mainW.pLink_result.Link_Names.Count; ++i)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = this.mainW.pLink_result.Link_Names[i];
                if (this.mainW.pLink_result.Link_Names[i] == this.xlink_label_name)
                    item.IsSelected = true;
                this.link_cbx.Items.Add(item);
            }
        }
        private void update_mods(string Sq, ObservableCollection<Modification> mods)
        {
            if (Sq == "")
                return;
            ObservableCollection<string> mod_flags = new ObservableCollection<string>();
            for (int p = 0; p < Config_Help.mod_label_name.Count(); ++p)
            {
                if (Config_Help.mod_label_name[p] == "")
                    continue;
                mod_flags.Add(Config_Help.mod_label_name[p]);
            }
            int mods_index = 0;
            mod_Sites.Clear();
            ObservableCollection<string> ss = new ObservableCollection<string>();
            if (mods_index < mods.Count() && mod_Sites.Count() == mods[mods_index].Index)
            {
                if (Config_Help.PEP_N_mod[Sq[0] - 'A'].Contains(mods[mods_index].Mod_name) ||
                    Config_Help.PRO_N_mod[Sq[0] - 'A'].Contains(mods[mods_index].Mod_name))
                {
                    ss.Add(mods[mods_index].Mod_name);
                    ++mods_index;
                }
            }
            ss.Add(non_mod);
            for (int p = 0; p < Config_Help.PEP_N_mod[Sq[0] - 'A'].Count; ++p)
            {
                string mod_tmp = Config_Help.PEP_N_mod[Sq[0] - 'A'][p];
                if (!ss.Contains(mod_tmp))
                    ss.Add(mod_tmp);
            }
            for (int p = 0; p < Config_Help.PRO_N_mod[Sq[0] - 'A'].Count; ++p)
            {
                string mod_tmp = Config_Help.PRO_N_mod[Sq[0] - 'A'][p];
                if (!ss.Contains(mod_tmp))
                    ss.Add(mod_tmp);
            }
            if (ss[0] == non_mod)
                mod_Sites.Add(new Mod_Site(0, "N_term", ss, ss[0], mod_flags, mod_flags[0]));
            else
                mod_Sites.Add(new Mod_Site(0, "N_term", ss, ss[0], mod_flags, Config_Help.mod_label_name[mods[mods_index - 1].Flag_Index]));
            for (int i = 0; i < Sq.Length; ++i)
            {
                ss = new ObservableCollection<string>();
                if (mods_index < mods.Count() && mod_Sites.Count() == mods[mods_index].Index)
                {
                    if (Config_Help.normal_mod[Sq[i] - 'A'].Contains(mods[mods_index].Mod_name))
                    {
                        ss.Add(mods[mods_index].Mod_name);
                        ++mods_index;
                    }
                }
                ss.Add(non_mod);
                for (int p = 0; p < Config_Help.normal_mod[Sq[i] - 'A'].Count; ++p)
                {
                    string mod_tmp = Config_Help.normal_mod[Sq[i] - 'A'][p];
                    if (!ss.Contains(mod_tmp))
                        ss.Add(mod_tmp);
                }
                if (ss[0] == non_mod)
                    mod_Sites.Add(new Mod_Site(i + 1, Sq[i] + "", ss, ss[0], mod_flags, mod_flags[0]));
                else
                    mod_Sites.Add(new Mod_Site(i + 1, Sq[i] + "", ss, ss[0], mod_flags, Config_Help.mod_label_name[mods[mods_index - 1].Flag_Index]));
            }
            ss = new ObservableCollection<string>();
            if (mods_index < mods.Count() && mod_Sites.Count() == mods[mods_index].Index)
            {
                if (Config_Help.PEP_C_mod[Sq[Sq.Length - 1] - 'A'].Contains(mods[mods_index].Mod_name) ||
                    Config_Help.PRO_C_mod[Sq[Sq.Length - 1] - 'A'].Contains(mods[mods_index].Mod_name))
                {
                    ss.Add(mods[mods_index].Mod_name);
                    ++mods_index;
                }
            }
            ss.Add(non_mod);
            for (int p = 0; p < Config_Help.PEP_C_mod[Sq[Sq.Length - 1] - 'A'].Count; ++p)
            {
                string mod_tmp = Config_Help.PEP_C_mod[Sq[Sq.Length - 1] - 'A'][p];
                if (!ss.Contains(mod_tmp))
                    ss.Add(mod_tmp);
            }
            for (int p = 0; p < Config_Help.PRO_C_mod[Sq[Sq.Length - 1] - 'A'].Count; ++p)
            {
                string mod_tmp = Config_Help.PRO_C_mod[Sq[Sq.Length - 1] - 'A'][p];
                if (!ss.Contains(mod_tmp))
                    ss.Add(mod_tmp);
            }
            if (ss[0] == non_mod)
                mod_Sites.Add(new Mod_Site(Sq.Length + 1, "C_term", ss, ss[0], mod_flags, mod_flags[0]));
            else
                mod_Sites.Add(new Mod_Site(Sq.Length + 1, "C_term", ss, ss[0], mod_flags, Config_Help.mod_label_name[mods[mods_index - 1].Flag_Index]));
        }

        private void lostFocus_seq1(object sender, RoutedEventArgs e)
        {
            if (!isSq_right(this.pep1_sq_tbx.Text))
            {
                MessageBox.Show("Sq1 Error!");
                return;
            }
            update_mods(this.pep1_sq_tbx.Text, this.psm.Peptide[0].Mods);
            this.pep1_mod_dg.ItemsSource = new ObservableCollection<Mod_Site>(this.Mod_Sites);
        }

        private void lostFocus_seq2(object sender, RoutedEventArgs e)
        {
            if (!isSq_right(this.pep2_sq_tbx.Text))
            {
                MessageBox.Show("Sq2 Error!");
                return;
            }
            update_mods(this.pep2_sq_tbx.Text, this.psm.Peptide[1].Mods);
            this.pep2_mod_dg.ItemsSource = new ObservableCollection<Mod_Site>(this.Mod_Sites);
        }
        private string get_modification_str(int pep1_pep2) //pep1_pep2==0表示加载第一条肽段的修饰信息，=1表示加载第二条肽段的修饰信息
        {
            DataGrid grid = this.pep1_mod_dg;
            string sq = this.pep1_sq_tbx.Text;
            if (pep1_pep2 == 1)
            {
                grid = this.pep2_mod_dg;
                sq = this.pep2_sq_tbx.Text;
            }
            string mods_str = "";
            ObservableCollection<Mod_Site> mod_Sites = grid.ItemsSource as ObservableCollection<Mod_Site>;
            if (mod_Sites[0].Mod != non_mod)
            {
                int index = 0;
                for (int l = 0; l < Config_Help.mod_label_name.Count(); ++l)
                {
                    if (mod_Sites[0].Mod_Flag == Config_Help.mod_label_name[l])
                    {
                        index = l;
                        break;
                    }
                }
                mods_str += "0," + mod_Sites[0].Mod + "#" + index + ";";
                //mainW.Dis_help.Psm_help.Pep.Mods.Add(new Modification(0, (double)Config_Help.modStr_hash[mod_Sites[0].Mod], mod_Sites[0].Mod));
            }
            for (int k = 0; k < sq.Length; ++k)
            {
                if (mod_Sites[k + 1].Mod != non_mod)
                {
                    int index = 0;
                    for (int l = 0; l < Config_Help.mod_label_name.Count(); ++l)
                    {
                        if (mod_Sites[k + 1].Mod_Flag == Config_Help.mod_label_name[l])
                        {
                            index = l;
                            break;
                        }
                    }
                    mods_str += (k + 1) + "," + mod_Sites[k + 1].Mod + "#" + index + ";";
                    //mainW.Dis_help.Psm_help.Pep.Mods.Add(new Modification(k + 1, (double)Config_Help.modStr_hash[mod_Sites[k + 1].Mod], mod_Sites[k + 1].Mod));
                }
            }
            if (mod_Sites[sq.Length + 1].Mod != non_mod)
            {
                int index = 0;
                for (int l = 0; l < Config_Help.mod_label_name.Count(); ++l)
                {
                    if (mod_Sites[sq.Length + 1].Mod_Flag == Config_Help.mod_label_name[l])
                    {
                        index = l;
                        break;
                    }
                }
                mods_str += (sq.Length + 1) + "," + mod_Sites[sq.Length + 1].Mod + "#" + index + ";";
                //mainW.Dis_help.Psm_help.Pep.Mods.Add(new Modification(mainW.Dis_help.Psm_help.Pep.Sq.Length + 1, (double)Config_Help.modStr_hash[mod_Sites[mainW.Dis_help.Psm_help.Pep.Sq.Length + 1].Mod], mod_Sites[mainW.Dis_help.Psm_help.Pep.Sq.Length + 1].Mod));
            }
            return mods_str;
        }
        private bool isSq_right(string sq)
        {
            for (int i = 0; i < sq.Length; ++i)
            {
                if (sq[i] < 'A' || sq[i] > 'Z')
                    return false;
            }
            return true;
        }
        private bool isLinkPosition_right(string link, ref int index1, ref int index2)
        {
            if (!link.Contains('-'))
                return false;
            string[] strs = link.Split('-');
            try
            {
                index1 = int.Parse(strs[0]);
                index2 = int.Parse(strs[1]);
                if (index1 <= 0 || index1 > this.pep1_sq_tbx.Text.Length || index2 <= 0 || index2 > this.pep2_sq_tbx.Text.Length)
                    return false;
            }
            catch (Exception exe)
            {
                return false;
            }
            return true;
        }
        private void update(object sender, RoutedEventArgs e)
        {
            pep1_flag = this.pep1_label_cbx.SelectedIndex;
            pep2_flag = this.pep2_label_cbx.SelectedIndex;
            PSM_Help_2 psm_help_2 = this.mainW.Dis_help.Psm_help as PSM_Help_2;
            psm_help_2.Pep1 = new Peptide(this.pep1_sq_tbx.Text);
            string mods_str = get_modification_str(0);
            double pep_theory_mass = 0.0;
            psm_help_2.Pep1.Tag_Flag = pep1_flag + 1;
            psm_help_2.Pep1.Mods = Modification.get_modSites(psm_help_2.Pep1.Sq, mods_str, psm_help_2.Pep1.Tag_Flag, ref pep_theory_mass);
            psm_help_2.Pep2 = new Peptide(this.pep2_sq_tbx.Text);
            mods_str = get_modification_str(1);
            psm_help_2.Pep2.Tag_Flag = pep2_flag + 1;
            psm_help_2.Pep2.Mods = Modification.get_modSites(psm_help_2.Pep2.Sq, mods_str, psm_help_2.Pep2.Tag_Flag, ref pep_theory_mass);
            ComboBoxItem item = this.link_cbx.SelectedItem as ComboBoxItem;
            psm_help_2.Xlink_mass = (double)Config_Help.link_hash[item.Content as string];
            int index1 = 0, index2 = 0;
            if (!isLinkPosition_right(this.link_position_tbx.Text, ref index1, ref index2))
            {
                MessageBox.Show("xLink position has error!");
                return;
            }
            psm_help_2.Link_pos1 = index1;
            psm_help_2.Link_pos2 = index2;
            PSM_Help_2.isNormal = (bool)this.normal_ckb.IsChecked;
            PSM_Help_2.isInternal = (bool)this.internal_ckb.IsChecked;
            this.mainW.Dis_help.Psm_help = psm_help_2;
            MS2_Help ms2_help = new MS2_Help(this.mainW.Model2, this.mainW.Dis_help, Ladder_Help.Scale_Width, Ladder_Help.Scale_Height);
            mainW.new_peptide = psm_help_2.Pep1;
            ms2_help.window_sizeChg_Or_ZoomPan();
        }
        #region
        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
            {
                if (!cell.IsFocused)
                {
                    cell.Focus();
                }
                DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
                if (dataGrid != null)
                {
                    if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                    {
                        if (!cell.IsSelected)
                            cell.IsSelected = true;
                    }
                    else
                    {
                        DataGridRow row = FindVisualParent<DataGridRow>(cell);
                        if (row != null && !row.IsSelected)
                        {
                            row.IsSelected = true;
                        }
                    }
                    //DataGridComboBoxColumn dcbc = null;
                    //if (cell.Column.Header == "Modification")
                    //{
                    //    dcbc = dataGrid.Columns[2] as DataGridComboBoxColumn;
                    //}
                    //else if (cell.Column.Header == "Mod_Flag")
                    //{
                    //    dcbc = dataGrid.Columns[3] as DataGridComboBoxColumn;
                    //}
                    //if (dcbc == null)
                    //    return;
                    //Style style = dcbc.EditingElementStyle;

                }
            }
        }
        static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }
        #endregion      

    }
}