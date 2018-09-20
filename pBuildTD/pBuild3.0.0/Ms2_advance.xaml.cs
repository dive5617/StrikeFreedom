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
using System.Windows.Threading;

namespace pBuild
{
    /// <summary>
    /// ms2_advance.xaml 的交互逻辑
    /// </summary>
    public partial class Ms2_advance : Window
    {
        const string non_mod = "";
        public static bool is_preprocess = false;
        private ObservableCollection<Mod_Site> mod_Sites = new ObservableCollection<Mod_Site>();
        private MainWindow mainW;
        private int all_psms_index;
        public ObservableCollection<Mod_Site> Mod_Sites
        {
            get { return mod_Sites; }
            set { mod_Sites = value; }
        }
        public int charge_num = PSM_Help.charge_num;
        public List<CheckBox> Ion_Check = new List<CheckBox>();
        private void newCheck(int charge_num) //增加1-charge_num中所有离子
        {
            const int margin = 10;
            const int text_width = 80;
            for (int i = 0; i <= 23; ++i)
            {
                RowDefinition rd = new RowDefinition();
                //rd.Height = new GridLength(45);
                grid.RowDefinitions.Add(rd);
            }
            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            Grid.SetRow(sp, 0);
            grid.Children.Add(sp);
            TextBlock tb=new TextBlock();
            tb.Text = "     ";
            tb.Width = text_width;
            tb.Margin=new Thickness(margin);
            sp.Children.Add(tb);
            for (int j = 1; j <= charge_num; ++j)
            {
                tb = new TextBlock();
                tb.Width = text_width;
                for (int k = 1; k <= j; ++k)
                    tb.Text += "+";
                tb.Margin = new Thickness(margin);
                sp.Children.Add(tb);
            }
            for (int i = 1; i <= 3; ++i)
            {
                sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                Grid.SetRow(sp, i);
                grid.Children.Add(sp);
                tb = new TextBlock();
                tb.Width = text_width;
                tb.Margin = new Thickness(margin);
                switch (i)
                {
                    case 1:
                        tb.Text = "[M]";
                        break;
                    case 2:
                        tb.Text = "[M]-H2O";
                        break;
                    case 3:
                        tb.Text = "[M]-NH3";
                        break;
                }
                sp.Children.Add(tb);
                for (int j = 1; j <= charge_num; ++j)
                {
                    CheckBox cb = new CheckBox();
                    cb.Margin = new Thickness(margin);
                    cb.Width = text_width;
                    if (mainW.Dis_help.Psm_help.M_Match_Flag[(i - 1) * charge_num + (j - 1)] == 1)
                        cb.IsChecked = true;
                    sp.Children.Add(cb);
                    Ion_Check.Add(cb);
                }
            }
            for (int i = 4; i <= 21; ++i)
            {
                sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                Grid.SetRow(sp, i);
                grid.Children.Add(sp);
                tb = new TextBlock();
                tb.Width = text_width;
                tb.Margin = new Thickness(margin);
                switch (i - 3)
                {
                    case 1:
                        tb.Text = "a";
                        break;
                    case 2:
                        tb.Text = "a-H2O";
                        break;
                    case 3:
                        tb.Text = "a-NH3";
                        break;
                    case 4:
                        tb.Text = "b";
                        break;
                    case 5:
                        tb.Text = "b-H2O";
                        break;
                    case 6:
                        tb.Text = "b-NH3";
                        break;
                    case 7:
                        tb.Text = "c";
                        break;
                    case 8:
                        tb.Text = "c-H2O";
                        break;
                    case 9:
                        tb.Text = "c-NH3";
                        break;
                    case 10:
                        tb.Text = "x";
                        break;
                    case 11:
                        tb.Text = "x-H2O";
                        break;
                    case 12:
                        tb.Text = "x-NH3";
                        break;
                    case 13:
                        tb.Text = "y";
                        break;
                    case 14:
                        tb.Text = "y-H2O";
                        break;
                    case 15:
                        tb.Text = "y-NH3";
                        break;
                    case 16:
                        tb.Text = "z";
                        break;
                    case 17:
                        tb.Text = "z-H2O";
                        break;
                    case 18:
                        tb.Text = "z-NH3";
                        break;
                }
                sp.Children.Add(tb);
                for (int j = 1; j <= charge_num; ++j)
                {
                    CheckBox cb = new CheckBox();
                    cb.Margin = new Thickness(margin);
                    cb.Width = text_width;
                    if (i <= 12)
                    {
                        if (mainW.Dis_help.Psm_help.N_Match_Flag[(i - 4) * charge_num + (j - 1)] == 1)
                            cb.IsChecked = true;
                    }
                    else
                    {
                        if (mainW.Dis_help.Psm_help.C_Match_Flag[(i - 13) * charge_num + (j - 1)] == 1)
                            cb.IsChecked = true;
                    }
                    sp.Children.Add(cb);
                    Ion_Check.Add(cb);
                }
            }
            sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            Grid.SetRow(sp, 22); //表示内部离子
            grid.Children.Add(sp);
            tb = new TextBlock();
            tb.Width = text_width;
            tb.Margin = new Thickness(margin);
            tb.Text = "Internal Ion";
            sp.Children.Add(tb);
            for (int j = 1; j <= charge_num; ++j)
            {
                CheckBox cb = new CheckBox();
                cb.Margin = new Thickness(margin);
                cb.Width = text_width;
                if (mainW.Dis_help.Psm_help.I_Match_Flag[j - 1] == 1)
                    cb.IsChecked = true;
                sp.Children.Add(cb);
                Ion_Check.Add(cb);
            }
            sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            Grid.SetRow(sp, 23); //最后一行，表示immon ions亚氨离子
            grid.Children.Add(sp);
            tb = new TextBlock();
            tb.Width = text_width;
            tb.Margin = new Thickness(margin);
            tb.Text = "Immon Ion";
            sp.Children.Add(tb);
            for (int j = 1; j <= charge_num; ++j)
            {
                CheckBox cb = new CheckBox();
                cb.Margin = new Thickness(margin);
                cb.Width = text_width;
                if (mainW.Dis_help.Psm_help.O_Match_Flag[j - 1] == 1)
                    cb.IsChecked = true;
                sp.Children.Add(cb);
                Ion_Check.Add(cb);
            }
        }
        public Ms2_advance(MainWindow mw, int index)
        {
            InitializeComponent();
            this.preprocess_cbx.IsChecked = Ms2_advance.is_preprocess;
            this.mainW = mw;
            if (mw.task_flag == "MGF")
                this.cand_cb.IsEnabled = false;
            this.all_psms_index = index;
            for (int i = 1; i < Config_Help.label_name.Length; ++i)
            {
                if (Config_Help.label_name[i] != "")
                    this.aa_index_comboBox.Items.Add(Config_Help.label_name[i]);
                else
                    break;
            }
            //this.aa_index_comboBox.SelectedIndex = mainW.Dis_help.Psm_help.aa_index - 1;
            this.aa_index_comboBox.SelectedIndex = mainW.Dis_help.Psm_help.Pep.Tag_Flag - 1;
            newCheck(charge_num);
            if (mainW.Dis_help.Psm_help == null)
                return;
            this.cand_cb.SelectedIndex = mainW.Dis_help.Psm_help.cand_index;
            this.seq.Text = mainW.Dis_help.Psm_help.Pep.Sq;
            update_mods(mainW.Dis_help.Psm_help.Pep.Mods);
            if (mainW.Dis_help.Psm_help.Ppm_mass_error != 0.0)
            {
                this.tol.Text = mainW.Dis_help.Psm_help.Ppm_mass_error * 1.0e6 + "";
                this.ppm_chk.Text = "ppm";
            }
            else
            {
                this.tol.Text = mainW.Dis_help.Psm_help.Da_mass_error + "";
                this.ppm_chk.Text = "Da";
            }
            
        }

        private void lostFocus_seq(object sender, RoutedEventArgs e)
        {
            update_mods(mainW.Dis_help.Psm_help.Pep.Mods);
        }
        private void update_mods(ObservableCollection<Modification> mods)
        {
            string Sq = this.seq.Text;
            if (Sq == "")
                return;
            bool isWrong = false;
            for (int i = 0; i < Sq.Length; ++i)
            {
                if (Sq[i] < 'A' || Sq[i] > 'Z')
                {
                    isWrong = true;
                    break;
                }
                else if (Sq[i] == 'B' || Sq[i] == 'J' || Sq[i] == 'O' || Sq[i] == 'U' || Sq[i] == 'X' || Sq[i] == 'Z')
                {
                    isWrong = true;
                    break;
                }
            }
            if (isWrong)
            {
                this.seq.Text = "Error!";
                this.seq.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
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
                    if (Config_Help.PEP_N_mod[this.seq.Text[0] - 'A'].Contains(mods[mods_index].Mod_name) ||
                        Config_Help.PRO_N_mod[this.seq.Text[0] - 'A'].Contains(mods[mods_index].Mod_name))
                    {
                        ss.Add(mods[mods_index].Mod_name);
                        ++mods_index;
                    }
                }
                ss.Add(non_mod);
                for (int p = 0; p < Config_Help.PEP_N_mod[this.seq.Text[0] - 'A'].Count; ++p)
                {
                    string mod_tmp = Config_Help.PEP_N_mod[this.seq.Text[0] - 'A'][p];
                    if (!ss.Contains(mod_tmp))
                        ss.Add(mod_tmp);
                }
                for (int p = 0; p < Config_Help.PRO_N_mod[this.seq.Text[0] - 'A'].Count; ++p)
                {
                    string mod_tmp = Config_Help.PRO_N_mod[this.seq.Text[0] - 'A'][p];
                    if (!ss.Contains(mod_tmp))
                        ss.Add(mod_tmp);
                }
                if (ss[0] == non_mod)
                    mod_Sites.Add(new Mod_Site(0, "N_term", ss, ss[0], mod_flags, mod_flags[0]));
                else
                    mod_Sites.Add(new Mod_Site(0, "N_term", ss, ss[0], mod_flags, Config_Help.mod_label_name[mods[mods_index - 1].Flag_Index]));
                for (int i = 0; i < this.seq.Text.Length; ++i)
                {
                    ss = new ObservableCollection<string>();
                    if (mods_index < mods.Count() && mod_Sites.Count() == mods[mods_index].Index)
                    {
                        if (Config_Help.normal_mod[this.seq.Text[i] - 'A'].Contains(mods[mods_index].Mod_name))
                        {
                            ss.Add(mods[mods_index].Mod_name);
                            ++mods_index;
                        }
                    }
                    ss.Add(non_mod);
                    for (int p = 0; p < Config_Help.normal_mod[this.seq.Text[i] - 'A'].Count; ++p)
                    {
                        string mod_tmp = Config_Help.normal_mod[this.seq.Text[i] - 'A'][p];
                        if (!ss.Contains(mod_tmp))
                            ss.Add(mod_tmp);
                    }
                    if (ss[0] == non_mod)
                        mod_Sites.Add(new Mod_Site(i + 1, this.seq.Text[i] + "", ss, ss[0], mod_flags, mod_flags[0]));
                    else
                        mod_Sites.Add(new Mod_Site(i + 1, this.seq.Text[i] + "", ss, ss[0], mod_flags, Config_Help.mod_label_name[mods[mods_index - 1].Flag_Index]));
                }
                ss = new ObservableCollection<string>();
                if (mods_index < mods.Count() && mod_Sites.Count() == mods[mods_index].Index)
                {
                    if (Config_Help.PEP_C_mod[this.seq.Text[this.seq.Text.Length - 1] - 'A'].Contains(mods[mods_index].Mod_name) ||
                        Config_Help.PRO_C_mod[this.seq.Text[this.seq.Text.Length - 1] - 'A'].Contains(mods[mods_index].Mod_name))
                    {
                        ss.Add(mods[mods_index].Mod_name);
                        ++mods_index;
                    }
                }
                ss.Add(non_mod);
                for (int p = 0; p < Config_Help.PEP_C_mod[this.seq.Text[this.seq.Text.Length - 1] - 'A'].Count; ++p)
                {
                    string mod_tmp = Config_Help.PEP_C_mod[this.seq.Text[this.seq.Text.Length - 1] - 'A'][p];
                    if(!ss.Contains(mod_tmp))
                        ss.Add(mod_tmp);
                }
                for (int p = 0; p < Config_Help.PRO_C_mod[this.seq.Text[this.seq.Text.Length - 1] - 'A'].Count; ++p)
                {
                    string mod_tmp = Config_Help.PRO_C_mod[this.seq.Text[this.seq.Text.Length - 1] - 'A'][p];
                    if (!ss.Contains(mod_tmp))
                        ss.Add(mod_tmp);
                }
                if (ss[0] == non_mod)
                    mod_Sites.Add(new Mod_Site(this.seq.Text.Length + 1, "C_term", ss, ss[0], mod_flags, mod_flags[0]));
                else
                    mod_Sites.Add(new Mod_Site(this.seq.Text.Length + 1, "C_term", ss, ss[0], mod_flags, Config_Help.mod_label_name[mods[mods_index - 1].Flag_Index]));
            }
        }
        private void update(object sender, RoutedEventArgs e)
        {
            if (mainW.Dis_help.Psm_help == null)
                return;
            //查看你选择了哪些匹配离子
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < charge_num; ++j)
                {
                    if (Ion_Check[i * charge_num + j].IsChecked == true)
                    {
                        mainW.Dis_help.Psm_help.M_Match_Flag[i * charge_num + j] = 1;
                    }
                    else
                    {
                        mainW.Dis_help.Psm_help.M_Match_Flag[i * charge_num + j] = 0;
                    }
                }
            }
            for (int i = 3; i < 21; ++i)
            {
                for (int j = 0; j < charge_num; ++j)
                {
                    if (Ion_Check[i * charge_num + j].IsChecked == true)
                    {
                        if (i < 12)
                        {
                            mainW.Dis_help.Psm_help.N_Match_Flag[(i - 3) * charge_num + j] = 1;
                        }
                        else
                        {
                            mainW.Dis_help.Psm_help.C_Match_Flag[(i - 12) * charge_num + j] = 1;
                        }
                    }
                    else
                    {
                        if (i < 12)
                        {
                            mainW.Dis_help.Psm_help.N_Match_Flag[(i - 3) * charge_num + j] = 0;
                        }
                        else
                        {
                            mainW.Dis_help.Psm_help.C_Match_Flag[(i - 12) * charge_num + j] = 0;
                        }
                    }
                }
            }
            int internal_index = 21;
            for (int j = 0; j < charge_num; ++j)
            {
                if (Ion_Check[internal_index * charge_num + j].IsChecked == true)
                {
                    mainW.Dis_help.Psm_help.I_Match_Flag[j] = 1;
                }
                else
                {
                    mainW.Dis_help.Psm_help.I_Match_Flag[j] = 0;
                }
            }
            int immon_index = 22;
            for (int j = 0; j < charge_num; ++j)
            {
                if (Ion_Check[immon_index * charge_num + j].IsChecked == true)
                {
                    mainW.Dis_help.Psm_help.O_Match_Flag[j] = 1;
                }
                else
                {
                    mainW.Dis_help.Psm_help.O_Match_Flag[j] = 0;
                }
            }
            //
            mainW.Dis_help.Psm_help.Pep.Sq = this.seq.Text;
            string ppm = this.ppm_chk.Text;
            try
            {
                if (ppm == "ppm")
                {
                    mainW.Dis_help.Psm_help.Ppm_mass_error = double.Parse(this.tol.Text) * 1.0e-6;
                    mainW.Dis_help.Psm_help.Da_mass_error = 0.0;
                    if (mainW.Dis_help.Psm_help.Ppm_mass_error == 0.0)
                        return;
                }
                else
                {
                    mainW.Dis_help.Psm_help.Da_mass_error = double.Parse(this.tol.Text);
                    mainW.Dis_help.Psm_help.Ppm_mass_error = 0.0;
                    if (mainW.Dis_help.Psm_help.Da_mass_error == 0.0)
                        return;
                }
                mainW.Dis_help.Psm_help.Pep.Mods.Clear();
                string mods_str = "";
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
                for (int k = 0; k < mainW.Dis_help.Psm_help.Pep.Sq.Length; ++k)
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
                if (mod_Sites[mainW.Dis_help.Psm_help.Pep.Sq.Length + 1].Mod != non_mod)
                {
                    int index = 0;
                    for (int l = 0; l < Config_Help.mod_label_name.Count(); ++l)
                    {
                        if (mod_Sites[mainW.Dis_help.Psm_help.Pep.Sq.Length + 1].Mod_Flag == Config_Help.mod_label_name[l])
                        {
                            index = l;
                            break;
                        }
                    }
                    mods_str += (mainW.Dis_help.Psm_help.Pep.Sq.Length + 1) + "," + mod_Sites[mainW.Dis_help.Psm_help.Pep.Sq.Length + 1].Mod + "#" + index + ";";
                    //mainW.Dis_help.Psm_help.Pep.Mods.Add(new Modification(mainW.Dis_help.Psm_help.Pep.Sq.Length + 1, (double)Config_Help.modStr_hash[mod_Sites[mainW.Dis_help.Psm_help.Pep.Sq.Length + 1].Mod], mod_Sites[mainW.Dis_help.Psm_help.Pep.Sq.Length + 1].Mod));
                }
                double pep_theory_mass = 0.0;
                if(mainW.selected_psm != null)
                    mainW.Dis_help.Psm_help.Pep.Mods = Modification.get_modSites(mainW.Dis_help.Psm_help.Pep.Sq, mods_str, int.Parse(mainW.selected_psm.Pep_flag), ref pep_theory_mass);
                else
                    mainW.Dis_help.Psm_help.Pep.Mods = Modification.get_modSites(mainW.Dis_help.Psm_help.Pep.Sq, mods_str, 0, ref pep_theory_mass);
                mainW.Dis_help.Psm_help.Pep.Pepmass = pep_theory_mass;
                //mainW.Dis_help.Psm_help.aa_index = this.aa_index_comboBox.SelectedIndex + 1;
                mainW.Dis_help.Psm_help.Pep.Tag_Flag = this.aa_index_comboBox.SelectedIndex + 1;
                mainW.psm_help = mainW.Dis_help.Psm_help as PSM_Help;
                MS2_Help ms2_help = new MS2_Help(this.mainW.Model2, this.mainW.Dis_help, Ladder_Help.Scale_Width, Ladder_Help.Scale_Height);
                mainW.new_peptide = mainW.Dis_help.Psm_help.Pep;
                ms2_help.window_sizeChg_Or_ZoomPan();
            }
            catch (Exception exe)
            {
                return;
            }
        }

        private void getFocus_seq(object sender, RoutedEventArgs e)
        {
            this.seq.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void lostFocus_tol(object sender, RoutedEventArgs e)
        {
            string tol_str = this.tol.Text;
            try
            {
                double tol_d = double.Parse(tol_str);
                if (tol_d == 0.0)
                {
                    this.tol.Text = "Number!";
                    this.tol.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
            catch (Exception exe)
            {
                this.tol.Text = "Number!";
                this.tol.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void getFocus_tol(object sender, RoutedEventArgs e)
        {
            this.tol.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void Seq_Enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                update_mods(mainW.Dis_help.Psm_help.Pep.Mods);
            }
        }

        private void cand_cb_clk(object sender, SelectionChangedEventArgs e)
        {
            if (mainW == null)
                return;
            int index = this.cand_cb.SelectedIndex;
            mainW.Dis_help.Psm_help.cand_index = index;
            List<Peptide> cand_peptides = mainW.all_psms[all_psms_index].Cand_peptides;
            if (index >= cand_peptides.Count)
            {
                MessageBox.Show(Message_Help.INDEX_OVER + cand_peptides.Count + Message_Help.INDEX_OVER2);
                return;
            }
            Peptide selected_peptide = cand_peptides[index];
            this.seq.Text = selected_peptide.Sq;
            this.aa_index_comboBox.SelectedIndex = selected_peptide.Tag_Flag;
            update_mods(selected_peptide.Mods);
            update(null, null);
        }

        private void Window_Enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                update(null, null);
            }
        }

        private void closed_event(object sender, EventArgs e)
        {
            this.mainW.ms2_advance_dialog = null;
        }
        //预处理，去掉同位素峰
        private void ms2_preprocess(object sender, RoutedEventArgs e)
        {
            if (!Ms2_advance.is_preprocess)
            {
                Ms2_advance.is_preprocess = true;
                this.mainW.psm_help.Spec.preprocess(this.mainW.psm_help.Ppm_mass_error, this.mainW.psm_help.Da_mass_error, false);
            }
            else
            {
                Ms2_advance.is_preprocess = false;
                string title = mainW.selected_psm.Title;
                string[] strs = title.Split('.');
                string raw_name = strs[0];
                int scan_num = int.Parse(strs[1]);
                int charge = int.Parse(strs[3]);
                int pre_num = 0;
                if (strs[4] != "dta")
                    pre_num = int.Parse(strs[4]);
                string ms2_pf_file = mainW.task.get_pf2_path(raw_name);
                int title_hash_index = 0;
                if (mainW.title_hash[raw_name] != null)
                {
                    title_hash_index = (int)(mainW.title_hash[raw_name]);
                }
                else
                {
                    for (int i = 0; i < mainW.ms2_scan_hash.Count; ++i)
                    {
                        if (mainW.ms2_scan_hash[i][scan_num] != null)
                        {
                            title_hash_index = i;
                            break;
                        }
                    }
                }
                ObservableCollection<PEAK> peaks = new ObservableCollection<PEAK>();
                double pepmass = 0.0;
                double maxInten = mainW.read_peaks(ms2_pf_file, mainW.ms2_scan_hash[title_hash_index], scan_num, pre_num, ref peaks, ref pepmass, ref charge);
                this.mainW.psm_help.Spec.Peaks = peaks;
            }
            mainW.data_RD.Height = new GridLength(mainW.data_RD.ActualHeight + 1.0e-10, GridUnitType.Pixel);
        }

        //End
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
    }
    
}
