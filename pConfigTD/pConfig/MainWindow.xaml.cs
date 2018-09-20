using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace pConfig
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public void add_CrashHandler()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            CrashReportMgr mgr = new CrashReportMgr();
        }
        public MainWindow()
        {
            //add_CrashHandler();
            InitializeComponent();
            elements = File_Helper.load_Element(Config_Helper.element_ini, ref base_elements); //加载界面的时候即把元素表加载进来，因为修饰，氨基酸表都依靠它
            //注意！！！更新MainWindow中的elements的时候，同时也要更新Element中的index_hash
            if (Application.Current.Properties["ArbitraryArgName"] != null)
            {
                this.tab_control.SelectedIndex = int.Parse(Application.Current.Properties["ArbitraryArgName"] as string);

                for (int i = 0; i < this.tab_control.Items.Count; ++i)
                {
                    if(i != this.tab_control.SelectedIndex)
                    {
                        TabItem item = this.tab_control.Items[i] as TabItem;
                        item.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
        string file_path = "pConfig-pTop.txt";  //pConfig-pFind.txt
        string database_title = "[database]";
        string modification_title = "[modification]";
        string quantification_title = "[quantification]";
        string enzyme_title = "[enzyme]";


        public ObservableCollection<Database> databases = null;
        public ObservableCollection<Modification> modifications = null;
        public ObservableCollection<Enzyme> enzymes = null;
        public ObservableCollection<Element> elements = null;
        public ObservableCollection<Amino_Acid> aas = null;
        public ObservableCollection<Quantification> quantifications = null;

        public ObservableCollection<Database> add_databases = new ObservableCollection<Database>();
        public ObservableCollection<Modification> add_modifications = new ObservableCollection<Modification>();
        public ObservableCollection<Quantification> add_quantifications = new ObservableCollection<Quantification>();
        public ObservableCollection<Enzyme> add_enzymes = new ObservableCollection<Enzyme>();

        //存储element_default.ini中所有的元素的名字，如果删除这些元素，提示用户不能删除，只能删除用户新增加的“自定义”元素
        public List<string> base_elements = new List<string>();

        public bool[] is_update = new bool[6];

        public void is_update_f()
        {
            for (int i = 0; i < 6; ++i)
            {
                TabItem ti = tab_control.Items[i] as TabItem;
                StackPanel sp = ti.Header as StackPanel;
                if (sp == null)
                    continue;
                if (is_update[i])
                {
                    TextBlock tb = sp.Children[0] as TextBlock;
                    tb.Text = "*";
                    tb.Visibility = Visibility.Visible;
                }
                else
                {
                    TextBlock tb = sp.Children[0] as TextBlock;
                    tb.Visibility = Visibility.Collapsed;
                }
            }
        }
        private void Load_ini()
        {
            if (this.tab_control.SelectedIndex == 0 && databases == null) //数据库配置界面
            {
                databases = File_Helper.load_DB(Config_Helper.database_ini);
                this.db_listView.ItemsSource = databases;
            }
            else if (this.tab_control.SelectedIndex == 1 && modifications == null) //修饰配置界面
            {
                modifications = File_Helper.load_Modification(Config_Helper.modification_ini);
                this.mod_listView.ItemsSource = modifications;
            }
            else if (this.tab_control.SelectedIndex == 2) //定量配置界面
            {
                quantifications = File_Helper.load_Quant(Config_Helper.quant_ini);
                this.quant_listView.ItemsSource = quantifications;
            }
            else if (this.tab_control.SelectedIndex == 3 && enzymes == null) //酶配置界面
            {
                enzymes = File_Helper.load_Enzyme(Config_Helper.enzyme_ini);
                this.enzyme_listView.ItemsSource = enzymes;
            }
            else if (this.tab_control.SelectedIndex == 4 && aas == null) //氨基酸配置界面
            {
                aas = File_Helper.load_AA(Config_Helper.aa_ini, this);
                this.aa_listView.ItemsSource = aas;
            }
            else if (this.tab_control.SelectedIndex == 5) //元素配置界面
            {
                this.element_listView.ItemsSource = elements;
            }
        }
        private void TabControl_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource is TabControl)
            {
                Load_ini();
            }
        }

        private void Add_Database_btn_clk(object sender, RoutedEventArgs e)
        {
            Database_Add_Dialog dad = new Database_Add_Dialog(this);
            dad.ShowDialog();
        }

        private void Delete_Database_btn_clk(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Database> selected_dbs = new ObservableCollection<Database>();
            for (int i = 0; i < this.db_listView.SelectedItems.Count; ++i)
            {
                Database selected_db = this.db_listView.SelectedItems[i] as Database;
                selected_dbs.Add(selected_db);
            }
            for (int i = 0; i < selected_dbs.Count; ++i)
            {
                databases.Remove(selected_dbs[i]);
            }
            is_update[0] = true;
            is_update_f();
        }

        private void Save_Databse_btn_clk(object sender, RoutedEventArgs e)
        {
            bool save_ok = File_Helper.save_DB(Config_Helper.database_ini, databases);
            if (save_ok)
            {
                write_file();
                is_update[0] = false;
                is_update_f();
            }
        }

        private void Add_Modification_btn_clk(object sender, RoutedEventArgs e)
        {
            Modification_Add_Dialog mad = new Modification_Add_Dialog(this);
            mad.ShowDialog();
        }

        private void Delete_Modification_btn_clk(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Modification> selected_mods = new ObservableCollection<Modification>();
            for (int i = 0; i < this.mod_listView.SelectedItems.Count; ++i)
            {
                Modification selected_mod = this.mod_listView.SelectedItems[i] as Modification;
                selected_mods.Add(selected_mod);
            }
            for (int i = 0; i < selected_mods.Count; ++i)
            {
                modifications.Remove(selected_mods[i]);
            }
            is_update[1] = true;
            is_update_f();
        }

        private void Save_Modification_btn_clk(object sender, RoutedEventArgs e)
        {
            bool save_ok = File_Helper.save_Mod(Config_Helper.modification_ini, modifications);
            if (save_ok)
            {
                write_file();
                is_update[1] = false;
                is_update_f();
            }
        }

        private void Add_Enzymes_btn_clk(object sender, RoutedEventArgs e)
        {
            Enzymes_Add_Dialog ead = new Enzymes_Add_Dialog(this);
            ead.ShowDialog();
        }

        private void Delete_Enzymes_btn_clk(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Enzyme> selected_enzymes = new ObservableCollection<Enzyme>();
            for (int i = 0; i < this.enzyme_listView.SelectedItems.Count; ++i)
            {
                Enzyme selected_enzyme = this.enzyme_listView.SelectedItems[i] as Enzyme;
                selected_enzymes.Add(selected_enzyme);
            }
            for (int i = 0; i < selected_enzymes.Count; ++i)
            {
                enzymes.Remove(selected_enzymes[i]);
            }
            is_update[3] = true;
            is_update_f();
        }

        private void Save_Enzymes_btn_clk(object sender, RoutedEventArgs e)
        {
            bool save_ok = File_Helper.save_Enzyme(Config_Helper.enzyme_ini, enzymes);
            if (save_ok)
            {
                write_file();
                is_update[3] = false;
                is_update_f();
            }
        }

        private void Edit_Amino_Acid_btn_clk(object sender, RoutedEventArgs e)
        {
            Amino_Acid selected_aa = this.aa_listView.SelectedItem as Amino_Acid;
            if (selected_aa == null)
            {
                MessageBox.Show(Message_Helper.AA_Selected_NULL_Message);
                return;
            }
            Amino_Acid_Edit_Dialog eaad = new Amino_Acid_Edit_Dialog(this, selected_aa);
            eaad.ShowDialog();
        }

        private void Save_Amino_Acid_btn_clk(object sender, RoutedEventArgs e)
        {
            bool save_ok = File_Helper.save_AA(Config_Helper.aa_ini, aas);
            if (save_ok)
            {
                is_update[4] = false;
                is_update_f();
            }
        }

        private void Add_Element_btn_clk(object sender, RoutedEventArgs e)
        {
            Element_Add_Dialog ead = new Element_Add_Dialog(this);
            ead.ShowDialog();
        }

        private void Delete_Element_btn_clk(object sender, RoutedEventArgs e)
        {
            bool can_delete = true;
            ObservableCollection<Element> selected_eles = new ObservableCollection<Element>();
            for (int i = 0; i < this.element_listView.SelectedItems.Count; ++i)
            {
                Element selected_ele = this.element_listView.SelectedItems[i] as Element;
                selected_eles.Add(selected_ele);
                if (base_elements.Contains(selected_ele.Name))
                {
                    can_delete = false;
                    break;
                }
            }
            if (!can_delete) //如果用户选择的元素中包含“基本”元素（在element_default.ini中的所有元素），那么提示用户不能删除
            {
                MessageBox.Show(Message_Helper.EE_CANNOT_DELETE);
                return;
            }
            for (int i = 0; i < selected_eles.Count; ++i)
            {
                Element.index_hash[selected_eles[i].Name] = null;
                elements.Remove(selected_eles[i]);
            }
            is_update[5] = true;
            is_update_f();
        }

        private void Save_Element_btn_clk(object sender, RoutedEventArgs e)
        {
            bool save_ok = File_Helper.save_Element(Config_Helper.element_ini, elements);
            if (save_ok)
            {
                is_update[5] = false;
                is_update_f();
            }
        }

        private void Add_Quantification_btn_clk(object sender, RoutedEventArgs e)
        {
            Quantification_Add_Dialog qad = new Quantification_Add_Dialog(this);
            qad.ShowDialog();
        }

        private void Delete_Quantification_btn_clk(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Quantification> selected_quants = new ObservableCollection<Quantification>();
            for (int i = 0; i < this.quant_listView.SelectedItems.Count; ++i)
            {
                Quantification selected_mod = this.quant_listView.SelectedItems[i] as Quantification;
                selected_quants.Add(selected_mod);
            }
            Quantification none_quant = new Quantification("None");
            Quantification N15_quant = new Quantification("15N_Labeling");
            if (selected_quants.Contains(none_quant))
            {
                MessageBox.Show(Message_Helper.QU_NONE_NOT_DELETE);
                return;
            }
            if (selected_quants.Contains(N15_quant))
            {
                MessageBox.Show(Message_Helper.QU_N15_NOT_DELETE);
                return;
            }
            for (int i = 0; i < selected_quants.Count; ++i)
            {
                quantifications.Remove(selected_quants[i]);
            }
            is_update[2] = true;
            is_update_f();
        }

        private void Save_Quantification_btn_clk(object sender, RoutedEventArgs e)
        {
            bool save_ok = File_Helper.save_Quantification(Config_Helper.quant_ini, quantifications);
            if (save_ok)
            {
                write_file();
                is_update[2] = false;
                is_update_f();
            }
        }
        
        private void refresh_file()
        {
            FileStream filestream = new FileStream(file_path, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(filestream, Encoding.Default);
            sw.Flush();
            sw.Close();
        }
        private void write_file()
        {
            FileStream filestream = new FileStream(file_path, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(filestream, Encoding.Default);
            if (this.add_databases.Count != 0)
            {
                sw.WriteLine(database_title);
                for (int i = 0; i < this.add_databases.Count; ++i)
                    sw.WriteLine(this.add_databases[i].DB_Name);
            }
            if (this.add_modifications.Count != 0)
            {
                sw.WriteLine(modification_title);
                for (int i = 0; i < this.add_modifications.Count; ++i)
                    sw.WriteLine(this.add_modifications[i].Name);
            }
            if (this.add_quantifications.Count != 0)
            {
                sw.WriteLine(quantification_title);
                for (int i = 0; i < this.add_quantifications.Count; ++i)
                    sw.WriteLine(this.add_quantifications[i].Name);
            }
            if (this.add_enzymes.Count != 0)
            {
                sw.WriteLine(enzyme_title);
                for (int i = 0; i < this.add_enzymes.Count; ++i)
                    sw.WriteLine(this.add_enzymes[i].Name);
            }
            sw.Flush();
            sw.Close();
        }
        private void close_window(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool is_update_t = false;
            for (int i = 0; i < 6; ++i)
            {
                if (is_update[i])
                {
                    is_update_t = true;
                    break;
                }
            }
            if (!is_update_t)
            {
                //refresh_file();
                return;
            }
            MessageBoxButton buttons = MessageBoxButton.YesNoCancel;
            MessageBoxImage icon = MessageBoxImage.Warning;
            string message = Message_Helper.Tab_Saved_Prompt;
            string caption = "Warning";
            MessageBoxResult mbr = MessageBox.Show(message, caption, buttons, icon);
            if (mbr == MessageBoxResult.Yes)
            {
                write_file();
                for (int i = 0; i < 6; ++i)
                {
                    if (!is_update[i])
                        continue;
                    switch (i)
                    {
                        case 0:
                            if (!File_Helper.save_DB(Config_Helper.database_ini, databases))
                                return;
                            break;
                        case 1:
                            if (!File_Helper.save_Mod(Config_Helper.modification_ini, modifications))
                                return;
                            break;
                        case 2:
                            if (!File_Helper.save_Quantification(Config_Helper.quant_ini, quantifications))
                                return;
                            break;
                        case 3:
                            if (!File_Helper.save_Enzyme(Config_Helper.enzyme_ini, enzymes))
                                return;
                            break;
                        case 4:
                            if (!File_Helper.save_AA(Config_Helper.aa_ini, aas))
                                return;
                            break;
                        case 5:
                            if (!File_Helper.save_Element(Config_Helper.element_ini, elements))
                                return;
                            break;
                    }
                }
            }
            else if(mbr == MessageBoxResult.No)
            {
                refresh_file();
                return;// not save data, but still close the window.
            }
            else
            {
                e.Cancel = true;
            }
        }
        
        private void db_clk(object sender, MouseButtonEventArgs e)
        {
            Database selected_db = this.db_listView.SelectedItem as Database;
            if (selected_db == null)
                return;
            Database_Edit_Dialog ded = new Database_Edit_Dialog(this);
            ded.ShowDialog();
        }

        private void mod_clk(object sender, MouseButtonEventArgs e)
        {
            Modification selected_mod = this.mod_listView.SelectedItem as Modification;
            if (selected_mod == null)
                return;
            Modification_Edit_Dialog med = new Modification_Edit_Dialog(this);
            med.ShowDialog();
        }

        private void quant_clk(object sender, MouseButtonEventArgs e)
        {
            Quantification selected_quant = this.quant_listView.SelectedItem as Quantification;
            if (selected_quant == null)
                return;
            if (selected_quant.Name == "None")
            {
                MessageBox.Show(Message_Helper.QU_NONE_NOT_EDIT);
                return;
            }
            if (selected_quant.Name == "15N_Labeling")
            {
                MessageBox.Show(Message_Helper.QU_N15_NOT_EDIT);
                return;
            }
            Quantification_Edit_Dialog qed = new Quantification_Edit_Dialog(this);
            qed.ShowDialog();
        }

        private void enzy_clk(object sender, MouseButtonEventArgs e)
        {
            Enzyme selected_enz = this.enzyme_listView.SelectedItem as Enzyme;
            if (selected_enz == null)
                return;
            Enzymes_Edit_Dialog eed = new Enzymes_Edit_Dialog(this);
            eed.ShowDialog();
        }

        private void aa_clk(object sender, MouseButtonEventArgs e)
        {
            Edit_Amino_Acid_btn_clk(null, null);
        }

        private void ele_clk(object sender, MouseButtonEventArgs e)
        {
            Element selected_element = this.element_listView.SelectedItem as Element;
            if (selected_element == null)
                return;
            Element_Edit_Dialog eed = new Element_Edit_Dialog(this);
            eed.ShowDialog();
        }

        private void txt_focus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.Foreground = new SolidColorBrush(Colors.Black);
            tb.Text = "";
        }

        private void txt_lostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.Foreground = new SolidColorBrush(Colors.Gray);
            tb.Text = "Search...";
        }

        private void txt_keyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & (ModifierKeys.Control)) == (ModifierKeys.Control))
                return;
            TextBox tb = sender as TextBox;
            string mod_name = tb.Text;
            if (e.Key != Key.Enter)
                mod_name += e.Key;
            mod_name = mod_name.ToLower();
            for (int i = 0; i < modifications.Count; ++i)
            {
                if(modifications[i].Name.ToLower().IndexOf(mod_name) == 0)
                {
                    mod_listView.SelectedItem = modifications[i];
                    this.mod_listView.ScrollIntoView(modifications[i]);
                    break;
                }
            }
        }

        private void txt_keyDown2(object sender, KeyEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string element_name = tb.Text;
            if (e.Key != Key.Enter)
                element_name += e.Key;
            element_name = element_name.ToLower();
            for (int i = 0; i < elements.Count; ++i)
            {
                if (elements[i].Name.ToLower().IndexOf(element_name) == 0)
                {
                    element_listView.SelectedItem = elements[i];
                    this.element_listView.ScrollIntoView(elements[i]);
                    break;
                }
            }
        }

        private void save(object sender, ExecutedRoutedEventArgs e)
        {
            write_file();
            switch (this.tab_control.SelectedIndex)
            {
                case 0:
                    Save_Databse_btn_clk(null, null);
                    break;
                case 1:
                    Save_Modification_btn_clk(null, null);
                    break;
                case 2:
                    Save_Quantification_btn_clk(null, null);
                    break;
                case 3:
                    Save_Enzymes_btn_clk(null, null);
                    break;
                case 4:
                    Save_Amino_Acid_btn_clk(null, null);
                    break;
                case 5:
                    Save_Element_btn_clk(null, null);
                    break;
            }
        }
        //将六个ini文件还原成默认配置，防止用户误操作将ini文件弄坏
        private void restore_ini_files(object sender, RoutedEventArgs e)
        {
            File_Helper.restore_file(Config_Helper.database_ini, Config_Helper.database_default_ini);
            File_Helper.restore_file(Config_Helper.quant_ini, Config_Helper.quant_default_ini);
            File_Helper.restore_file(Config_Helper.modification_ini, Config_Helper.modification_default_ini);
            File_Helper.restore_file(Config_Helper.enzyme_ini, Config_Helper.enzyme_default_ini);
            File_Helper.restore_file(Config_Helper.aa_ini, Config_Helper.aa_default_ini);
            File_Helper.restore_file(Config_Helper.element_ini, Config_Helper.element_default_ini);
            this.databases = null;
            this.modifications = null;
            this.quantifications = null;
            this.enzymes = null;
            this.aas = null;
            this.elements = File_Helper.load_Element(Config_Helper.element_ini, ref base_elements); //加载界面的时候即把元素表加载进来，因为修饰，氨基酸表都依靠它
            //注意！！！更新MainWindow中的elements的时候，同时也要更新Element中的index_hash
            Load_ini();
            MessageBox.Show("OK");
        }

        private void N_count_clk(object sender, RoutedEventArgs e)
        {
            this.aas = File_Helper.load_AA(Config_Helper.aa_ini, this);
            this.modifications = File_Helper.load_Modification(Config_Helper.modification_ini);
            N_Count_window ncw = new N_Count_window(this);
            ncw.Show();
        }
    }
}
