using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Threading;

namespace pBuild
{
    /// <summary>
    /// PSM_Filter_Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class PSM_Filter_Dialog : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        MainWindow mainW;
        public PSM_Filter_Dialog() { }
        public PSM_Filter_Dialog(MainWindow mainW)
        {
            InitializeComponent();
            this.mainW = mainW;
            if (this.mainW.task.has_ratio)
            {
                this.ratio_tbk.Visibility = Visibility.Visible;
                this.ratio_tbx.Visibility = Visibility.Visible;
                this.ratio_tbk2.Visibility = Visibility.Visible;
                this.ratio_tbx2.Visibility = Visibility.Visible;
                this.sigma_tbk.Visibility = Visibility.Visible;
                this.sigma_tbk2.Visibility = Visibility.Visible;
                this.sigma_tbx.Visibility = Visibility.Visible;
                this.sigma_tbx2.Visibility = Visibility.Visible;
            }
            update_window(); //更新控件
            assign_window(); //初始化控件中的值
        }
        private void assign_window()
        {
            PSM_Filter_Bean pfb = Config_Help.psm_filter_bean;
            this.filter_mix_num.Text = (pfb.Mix_number == 0 ? "" : pfb.Mix_number.ToString());
            this.title_subStr_txt.Text = pfb.Title;
            this.title_cbb.SelectedIndex = pfb.Title_Regular_Flag;
            this.sq_subStr_txt.Text = pfb.SQ;
            this.sq_cbb.SelectedIndex = pfb.SQ_Regular_Flag;
            this.mods_comboBox.Text = pfb.Modification;
            this.mods_comboBox2.Text = pfb.Modification_label_name;
            this.specific_comboBox.Text = pfb.Specific;
            this.label_comboBox.Text = pfb.Label_name;
            this.ratio_tbx.Text = (pfb.Ratio1 == 1024.0 ? "" : pfb.Ratio1.ToString("F3"));
            this.ratio_tbx2.Text = (pfb.Ratio2 == 0.0 ? "" : pfb.Ratio2.ToString("F0"));
            this.sigma_tbx.Text = (pfb.Sigma1 == 1.0 ? "" : pfb.Sigma1.ToString("F2"));
            this.sigma_tbx2.Text = (pfb.Sigma2 == 0.0 ? "" : pfb.Sigma2.ToString("F2"));
            this.target_comboBox.Text = pfb.Target;
            this.con_need_cbx.IsChecked = pfb.isContainment;
            this.unique_cbx.IsChecked = pfb.isUnique;
        }
        private void update_window()
        {
            if (this.mods_comboBox.Items.Count > 0)
                return;
            if (this.mods_comboBox2.Items.Count > 0)
                return;
            if (this.label_comboBox.Items.Count > 0)
                return;
            ComboBoxItem item = new ComboBoxItem();
            item.Content = "Show All";
            this.mods_comboBox.Items.Add(item);
            this.mods_comboBox.Text = "Show All";
            ComboBoxItem item2 = new ComboBoxItem();
            item2.Content = "None";
            this.mods_comboBox.Items.Add(item2);
            List<Identification_Modification> modifications = null;
            File_Help.read_Summary_Modification_TXT(mainW.task.summary_file, ref modifications);
            for (int i = 0; i < modifications.Count; ++i)
            {
                item = new ComboBoxItem();
                item.Content = modifications[i].modification_name;
                this.mods_comboBox.Items.Add(item);
            }
            this.mods_comboBox.SelectedIndex = 0;
            item = new ComboBoxItem();
            item.Content = "Show All";
            this.mods_comboBox2.Items.Add(item);
            this.mods_comboBox2.Text = "Show All";
            for (int i = 0; i < Config_Help.mod_label_name.Count(); ++i)
            {
                if (Config_Help.mod_label_name[i] == "")
                    continue;
                item = new ComboBoxItem();
                item.Content = Config_Help.mod_label_name[i];
                this.mods_comboBox2.Items.Add(item);
            }
            this.mods_comboBox2.SelectedIndex = 0;
            item = new ComboBoxItem();
            item.Content = "Show All";
            this.label_comboBox.Items.Add(item);
            this.label_comboBox.Text = "Show All";
            for (int i = 1; i < Config_Help.label_name.Count(); ++i)
            {
                if (Config_Help.label_name[i] == "")
                    continue;
                item = new ComboBoxItem();
                item.Content = Config_Help.label_name[i];
                this.label_comboBox.Items.Add(item);
            }
            this.label_comboBox.SelectedIndex = 0;
        }
        public ObservableCollection<PSM> filter_bySQ_or_Title(string str, int flag, ObservableCollection<PSM> all_psms, int regular_flag = 0) //regular_flag为0表示Normal搜索，为1表示正则表达式搜索
        {
            if (str == "")
                return all_psms;
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            if(flag==0)
            {
                if (regular_flag == 0)
                {
                    for (int i = 0; i < all_psms.Count; ++i)
                    {
                        if (all_psms[i].Title.Contains(str))
                            psms.Add(all_psms[i]);
                    }
                }
                else if (regular_flag == 1)
                {
                    for (int i = 0; i < all_psms.Count; ++i)
                    {
                        if (Config_Help.IsMatch_RegularExpression(all_psms[i].Title, str))
                            psms.Add(all_psms[i]);
                    }
                }
            }
            else if (flag == 1)
            {
                if (regular_flag == 0)
                {
                    for (int i = 0; i < all_psms.Count; ++i)
                    {
                        if (all_psms[i].Sq.Contains(str))
                            psms.Add(all_psms[i]);
                    }
                }
                else if (regular_flag == 1)
                {
                    for (int i = 0; i < all_psms.Count; ++i)
                    {
                        if (Config_Help.IsMatch_RegularExpression(all_psms[i].Sq, str))
                            psms.Add(all_psms[i]);
                    }
                }
            }
            return psms;
        }
        public ObservableCollection<PSM> filter_removeByPeptide(ObservableCollection<PSM> all_psms)
        {
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            Hashtable peptide_score = new Hashtable();
            Hashtable peptide_id = new Hashtable();
            for (int i = 0; i < all_psms.Count; ++i)
            {
                string ss = all_psms[i].Sq + "@" + all_psms[i].Mod_sites;
                if (peptide_score[ss] == null)
                {
                    peptide_score[ss] = all_psms[i].Score;
                    peptide_id[ss] = i;
                }
                else
                {
                    double score = (double)peptide_score[ss];
                    if (all_psms[i].Score < score)
                    {
                        peptide_score[ss] = all_psms[i].Score;
                        peptide_id[ss] = i;
                    }
                }
            }
            foreach (string key in peptide_id.Keys)
            {
                int id = (int)peptide_id[key];
                psms.Add(all_psms[id]);
            }
            return psms;
        }
        public ObservableCollection<PSM> filter_removeBySQ(ObservableCollection<PSM> all_psms)
        {
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            Hashtable peptide_score = new Hashtable();
            Hashtable peptide_id = new Hashtable();
            for (int i = 0; i < all_psms.Count; ++i)
            {
                string ss = all_psms[i].Sq;
                if (peptide_score[ss] == null)
                {
                    peptide_score[ss] = all_psms[i].Score;
                    peptide_id[ss] = i;
                }
                else
                {
                    double score = (double)peptide_score[ss];
                    if (all_psms[i].Score < score)
                    {
                        peptide_score[ss] = all_psms[i].Score;
                        peptide_id[ss] = i;
                    }
                }
            }
            foreach (string key in peptide_id.Keys)
            {
                int id = (int)peptide_id[key];
                psms.Add(all_psms[id]);
            }
            return psms;
        }
        public ObservableCollection<PSM> filter_byPeptide(PSM psm, ObservableCollection<PSM> all_psms)
        {
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            string ss = psm.Sq + "@" + psm.Mod_sites;
            for (int i = 0; i < all_psms.Count; ++i)
            {
                string tmp_ss = all_psms[i].Sq + "@" + all_psms[i].Mod_sites;
                if (ss == tmp_ss)
                    psms.Add(all_psms[i]);
            }
            return psms;
        }
        public ObservableCollection<PSM> filter_bySQ(PSM psm, ObservableCollection<PSM> all_psms)
        {
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            string ss = psm.Sq;
            for (int i = 0; i < all_psms.Count; ++i)
            {
                if (ss == all_psms[i].Sq)
                    psms.Add(all_psms[i]);
            }
            return psms;
        }
        public ObservableCollection<PSM> filter_byRawName(string raw_name, ObservableCollection<PSM> all_psms)
        {
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            for (int i = 0; i < all_psms.Count; ++i)
            {
                if (all_psms[i].Title.Contains(raw_name))
                {
                    psms.Add(all_psms[i]);
                }
            }
            return psms;
        }
        public ObservableCollection<PSM> filter_bySpecific(char specific_flag, ObservableCollection<PSM> all_psms)
        {
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            for (int i = 0; i < all_psms.Count; ++i)
            {
                if (all_psms[i].Specific_flag == specific_flag)
                {
                    psms.Add(all_psms[i]);
                }
            }
            return psms;
        }
        public ObservableCollection<PSM> filter_byMix_Equal(int mixed_number, ObservableCollection<PSM> all_psms)
        {
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            Hashtable scan_num_hash = new Hashtable();
            for (int i = 0; i < all_psms.Count; ++i)
            {
                string title = all_psms[i].Title;
                string[] strs = title.Split('.');
                string raw_name = strs[0];
                int raw_index = (int)mainW.title_hash[raw_name];
                int scan_num = int.Parse(strs[1]);
                string raw_scan = raw_index + "." + scan_num;
                if (scan_num_hash[raw_scan] == null)
                    scan_num_hash[raw_scan] = 1;
                else
                {
                    int num = (int)scan_num_hash[raw_scan] + 1;
                    scan_num_hash[raw_scan] = num;
                }
            }
            for (int i = 0; i < all_psms.Count; ++i)
            {
                string title = all_psms[i].Title;
                string[] strs = title.Split('.');
                string raw_name = strs[0];
                int raw_index = (int)mainW.title_hash[raw_name];
                int scan_num = int.Parse(strs[1]);
                string raw_scan = raw_index + "." + scan_num;
                int num = (int)scan_num_hash[raw_scan];
                if (num == mixed_number)
                    psms.Add(all_psms[i]);
            }
            return psms;
        }
        public ObservableCollection<PSM> filter_byMix_Bigger(int mixed_number, ObservableCollection<PSM> all_psms)
        {
            if (mixed_number == 0 || mixed_number == 1)
                return all_psms;
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            Hashtable scan_num_hash = new Hashtable();
            for (int i = 0; i < all_psms.Count; ++i)
            {
                string title=all_psms[i].Title;
                string[] strs = title.Split('.');
                string raw_name = strs[0];
                int raw_index = (int)mainW.title_hash[raw_name];
                int scan_num = int.Parse(strs[1]);
                string raw_scan = raw_index + "." + scan_num;
                if (scan_num_hash[raw_scan] == null)
                    scan_num_hash[raw_scan] = 1;
                else
                {
                    int num = (int)scan_num_hash[raw_scan] + 1;
                    scan_num_hash[raw_scan] = num;
                }
            }
            for (int i = 0; i < all_psms.Count; ++i)
            {
                string title = all_psms[i].Title;
                string[] strs = title.Split('.');
                string raw_name = strs[0];
                int raw_index = (int)mainW.title_hash[raw_name];
                int scan_num = int.Parse(strs[1]);
                string raw_scan = raw_index + "." + scan_num;
                int num = (int)scan_num_hash[raw_scan];
                if (num >= mixed_number)
                    psms.Add(all_psms[i]);
            }
            return psms;
        }
        public ObservableCollection<PSM> filter_byPeptideMissedCleavage(int missed_cleavage_number, ObservableCollection<PSM> all_psms)
        {
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            for (int i = 0; i < all_psms.Count; ++i)
            {
                if (all_psms[i].Missed_cleavage_number == missed_cleavage_number)
                    psms.Add(all_psms[i]);
            }
            return psms;
        }
        public ObservableCollection<PSM> filter_byPeptideLength(int length, ObservableCollection<PSM> all_psms)
        {
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            for (int i = 0; i < all_psms.Count; ++i)
            {
                if (all_psms[i].Sq.Length == length)
                    psms.Add(all_psms[i]);
            }
            return psms;
        }
        public ObservableCollection<PSM> filter_byMod(string mod_name, ObservableCollection<PSM> all_psms)
        {
            if (mod_name == "")
                return all_psms;
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            if (mod_name == "None")
            {
                for (int i = 0; i < all_psms.Count; ++i)
                {
                    if (all_psms[i].Mod_sites == "")
                        psms.Add(all_psms[i]);
                }
            }
            else
            {
                for (int i = 0; i < all_psms.Count; ++i)
                {
                    if (all_psms[i].Mod_sites.Contains(mod_name))
                        psms.Add(all_psms[i]);
                }
            }
            return psms;
        }
        public ObservableCollection<PSM> filter_bySpecific(string specific_name, ObservableCollection<PSM> all_psms)
        {
            if (specific_name == "" || specific_name == "Show All")
                return all_psms;
            char specific_n = 's';
            switch (specific_name)
            {
                case "Specific":
                    specific_n = 'S';
                    break;
                case "N-specific":
                    specific_n = 'N';
                    break;
                case "C-specific":
                    specific_n = 'C';
                    break;
                case "Non-specific":
                    specific_n = 'O';
                    break;
            }
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            for (int i = 0; i < all_psms.Count; ++i)
            {
                if (all_psms[i].Specific_flag == specific_n)
                {
                    psms.Add(all_psms[i]);
                }
            }
            return psms;
        }
        public ObservableCollection<PSM> filter_byLabelName(string label_name, ObservableCollection<PSM> all_psms)
        {
            if (label_name == "")
                return all_psms;
            int index = 0;
            for (int i = 1; i < Config_Help.label_name.Count(); ++i)
            {
                if (Config_Help.label_name[i] == label_name)
                {
                    index = i;
                    break;
                }
            }
            label_name = index + "";
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            for (int i = 0; i < all_psms.Count; ++i)
            {
                if (all_psms[i].Pep_flag == label_name)
                    psms.Add(all_psms[i]);
            }
            return psms;
        }
        public ObservableCollection<PSM> filter_byTarget(int target_index, ObservableCollection<PSM> all_psms) //target_index=0: all,1: target,2: decoy
        {
            if (target_index == 0)
                return all_psms;
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            if (target_index == 1) //target
            {
                for (int i = 0; i < all_psms.Count; ++i)
                {
                    if (all_psms[i].Is_target_flag)
                        psms.Add(all_psms[i]);
                }
            }
            else if(target_index == 2) //decoy
            {
                for (int i = 0; i < all_psms.Count; ++i)
                {
                    if (!all_psms[i].Is_target_flag)
                        psms.Add(all_psms[i]);
                }
            }
            return psms;
        }
        public ObservableCollection<PSM> filter_bySigma_Less(double sigma, ObservableCollection<PSM> all_psms)
        {
            if (sigma == 1.0)
                return all_psms;
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            for (int i = 0; i < all_psms.Count; ++i)
            {
                if (all_psms[i].Sigma <= sigma)
                {
                    psms.Add(all_psms[i]);
                }
            }
            return psms;
        }
        public ObservableCollection<PSM> filter_bySigma_Bigger(double sigma, ObservableCollection<PSM> all_psms)
        {
            if (sigma == 0.0)
                return all_psms;
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            for (int i = 0; i < all_psms.Count; ++i)
            {
                if (all_psms[i].Sigma >= sigma)
                {
                    psms.Add(all_psms[i]);
                }
            }
            return psms;
        }
        public ObservableCollection<PSM> filter_byRatio_Less(double ratio, ObservableCollection<PSM> all_psms)
        {
            if (ratio == 1024.0)
                return all_psms;
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            for (int i = 0; i < all_psms.Count; ++i)
            {
                if (all_psms[i].Ratio <= ratio)
                {
                    psms.Add(all_psms[i]);
                }
            }
            return psms;
        }
        public ObservableCollection<PSM> filter_byRatio_Bigger(double ratio, ObservableCollection<PSM> all_psms)
        {
            if (ratio == 0.0)
                return all_psms;
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            for (int i = 0; i < all_psms.Count; ++i)
            {
                if (all_psms[i].Ratio >= ratio)
                {
                    psms.Add(all_psms[i]);
                }
            }
            return psms;
        }
        public ObservableCollection<PSM> filter_byContaminant(bool isCon_need, ObservableCollection<PSM> all_psms)
        {
            if (isCon_need)
                return all_psms;
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            for(int i=0;i<all_psms.Count;++i)
            {
                if (all_psms[i].is_Contaminant())
                    continue;
                psms.Add(all_psms[i]);
            }
            return psms;
        }
        public ObservableCollection<PSM> filter_byUnqiue(bool isUnique, ObservableCollection<PSM> all_psms)
        {
            if (!isUnique)
                return all_psms;
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            for (int i = 0; i < all_psms.Count; ++i)
            {
                string[] acs = all_psms[i].AC.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if(acs.Length==1)
                    psms.Add(all_psms[i]);
            }
            return psms;
        }
        private void filter_btn(object sender, RoutedEventArgs e)
        {
            try
            {
                string title_str = this.title_subStr_txt.Text;
                int title_regular_flag = this.title_cbb.SelectedIndex;
                string sq_str = this.sq_subStr_txt.Text;
                int sq_regular_flag = this.sq_cbb.SelectedIndex;
                ComboBoxItem selected_item = (ComboBoxItem)this.mods_comboBox.SelectedItem;
                ComboBoxItem selected_item_flag = (ComboBoxItem)this.mods_comboBox2.SelectedItem;
                string mod_name = selected_item.Content.ToString();
                string mod_name1 = mod_name;
                string mod_flag = selected_item_flag.Content.ToString();
                if (mod_name == "Show All")
                    mod_name = "";
                
                if (mod_flag != "Show All")
                {
                    int index = 0;
                    for (int i = 0; i < Config_Help.mod_label_name.Count(); ++i)
                    {
                        if (Config_Help.mod_label_name[i] == mod_flag)
                        {
                            index = i;
                            break;
                        }
                    }
                    mod_name = mod_name + "#" + index;
                }
                ComboBoxItem selected_item2 = (ComboBoxItem)this.specific_comboBox.SelectedItem;
                string specific_name = selected_item2.Content.ToString();

                if (this.filter_mix_num.Text != "" && !Config_Help.IsIntegerAllowed(this.filter_mix_num.Text))
                {
                    MessageBox.Show(Message_Help.MIXED_NUMBER_BE_INTEGER);
                    return;
                }
                int mix_num = 0;
                if (this.filter_mix_num.Text != "")
                    mix_num = int.Parse(this.filter_mix_num.Text);
                if (this.ratio_tbx.Text != "" && !Config_Help.IsDecimalAllowed(this.ratio_tbx.Text)
                    && this.ratio_tbx2.Text != "" && !Config_Help.IsDecimalAllowed(this.ratio_tbx2.Text))
                {
                    MessageBox.Show(Message_Help.RATIO_NUMBER_BE_DOUBLE);
                    return;
                }
                ComboBoxItem selected_item3 = (ComboBoxItem)this.label_comboBox.SelectedItem;
                string label_name = selected_item3.Content.ToString();
                string label_name1 = label_name;
                if (label_name == "Show All")
                    label_name = "";

                double ratio = 1024.0;
                double ratio2 = 0.0;
                double sigma = 1.0;
                double sigma2 = 0.0;
                if (this.ratio_tbx.Text != "")
                    ratio = double.Parse(this.ratio_tbx.Text);
                if (this.ratio_tbx2.Text != "")
                    ratio2 = double.Parse(this.ratio_tbx2.Text);
                if (this.sigma_tbx.Text != "")
                    sigma = double.Parse(this.sigma_tbx.Text);
                if (this.sigma_tbx2.Text != "")
                    sigma2 = double.Parse(this.sigma_tbx2.Text);
                
                ObservableCollection<PSM> display_psms = new ObservableCollection<PSM>();
                ObservableCollection<PSM> all_psms = new ObservableCollection<PSM>();
                if (!(bool)mainW.other_psms_cbx.IsChecked)
                {
                    all_psms = mainW.psms;
                }
                else
                {
                    for (int i = 0; i < mainW.all_psms.Count; ++i)
                    {
                        if (mainW.all_psms[i].Q_value > Config_Help.fdr_value)
                            all_psms.Add(mainW.all_psms[i]);
                    }
                }
                ComboBoxItem selected_item_target = (ComboBoxItem)this.target_comboBox.SelectedItem;
                string target_string = selected_item_target.Content.ToString();
                int target_index = 0;
                if (target_string == "Show All")
                    target_index = 0;
                else if (target_string == "Target")
                    target_index = 1;
                else if (target_string == "Decoy")
                    target_index = 2;
                display_psms = filter_byTarget(target_index, all_psms);
                display_psms = filter_bySQ_or_Title(title_str, 0, display_psms, title_regular_flag);
                display_psms = filter_bySQ_or_Title(sq_str, 1, display_psms, sq_regular_flag);
                display_psms = filter_byMod(mod_name, display_psms);
                display_psms = filter_bySpecific(specific_name, display_psms);
                display_psms = filter_byLabelName(label_name, display_psms);
                display_psms = filter_byRatio_Less(ratio, display_psms);
                display_psms = filter_byRatio_Bigger(ratio2, display_psms);
                display_psms = filter_bySigma_Less(sigma, display_psms);
                display_psms = filter_bySigma_Bigger(sigma2, display_psms);
                display_psms = filter_byContaminant((bool)this.con_need_cbx.IsChecked, display_psms);
                display_psms = filter_byUnqiue((bool)this.unique_cbx.IsChecked, display_psms);
                display_psms = filter_byMix_Bigger(mix_num, display_psms);
                mainW.display_psms = display_psms;
                mainW.data.ItemsSource = display_psms;
                mainW.display_size.Text = display_psms.Count.ToString();
                Config_Help.psm_filter_bean = new PSM_Filter_Bean(mix_num, title_str, title_regular_flag, sq_str, sq_regular_flag, mod_name1, mod_flag, specific_name, label_name1, target_string,
                     (bool)this.con_need_cbx.IsChecked, (bool)this.unique_cbx.IsChecked, ratio, ratio2, sigma, sigma2);
                //this.Close();
            }
            catch (Exception exe)
            {
                MessageBox.Show(exe.ToString());
            }
        }
        
        private void enter_keyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                if (e.Key == Key.V && (Keyboard.Modifiers & (ModifierKeys.Control)) == (ModifierKeys.Control))
                {
                    IDataObject iData = Clipboard.GetDataObject();
                    // 将数据与指定的格式进行匹配，返回bool
                    if (iData.GetDataPresent(DataFormats.Text))
                    {
                        // GetData检索数据并指定一个格式
                        string modification_name = (string)iData.GetData(DataFormats.Text);
                        int i = 0;
                        for (i = 0; i < this.mods_comboBox.Items.Count; ++i)
                        {
                            ComboBoxItem item = this.mods_comboBox.Items[i] as ComboBoxItem;
                            string mod = item.Content as string;
                            if (mod.Contains(modification_name))
                            {
                                item.IsSelected = true;
                                break;
                            }
                        }
                        if (i >= this.mods_comboBox.Items.Count)
                        {
                            this.warning_tbk.Text = "You input the modification name isn't in this list.";
                            this.warning_tbk.Visibility = Visibility.Visible;
                            timer = new DispatcherTimer();
                            timer.Interval = TimeSpan.FromSeconds(3);
                            timer.Tick += hidden_window;
                            timer.IsEnabled = false;
                            timer.Start();
                        }
                    }
                    else
                    {
                        this.warning_tbk.Text = "You can not paste some data which cannot be converted to text.";
                        this.warning_tbk.Visibility = Visibility.Visible;
                        timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromSeconds(3);
                        timer.Tick += hidden_window;
                        timer.IsEnabled = false;
                        timer.Start();
                    }
                }
                return;
            }
            filter_btn(null, null);
        }
        private void hidden_window(object sender, EventArgs e)
        {
            this.warning_tbk.Visibility = Visibility.Collapsed;
            timer.Stop();
        }
        private void closed_event(object sender, EventArgs e)
        {
            this.mainW.psm_filter_dialog = null;
        }

        private void display_btn(object sender, RoutedEventArgs e)
        {
            ObservableCollection<PSM> all_psms = this.mainW.display_psms;
            filter_btn(null, null);
            ObservableCollection<PSM> new_psms = this.mainW.display_psms;
            this.mainW.display_psms = all_psms;
            this.mainW.data.ItemsSource = all_psms;
            this.mainW.display_size.Text = all_psms.Count.ToString();
            if (new_psms.Count == 0)
                return;
            PSM selected_psm = new_psms[0];
            this.mainW.data.SelectedItem = selected_psm;
            object item = this.mainW.data.SelectedItem;
            if (item != null)
                this.mainW.data.ScrollIntoView(item);
        }

        private void filter_btn2(object sender, RoutedEventArgs e)
        {
            ObservableCollection<PSM> newP = new ObservableCollection<PSM>();
            string mod = this.modTxt2.Text;
            for (int i = 0; i < mainW.psms.Count; ++i)
            {
                if (mainW.psms[i].Mod_sites.Contains(mod))
                    newP.Add(mainW.psms[i]);
            }
            mainW.display_psms = newP;
            mainW.data.ItemsSource = newP;
            mainW.display_size.Text = newP.Count.ToString();
        }
    }
}
