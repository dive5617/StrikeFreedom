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

namespace pConfig
{
    /// <summary>
    /// Modification_Edit_Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class Modification_Edit_Dialog : Window
    {
        public MainWindow mainW;
        public Modification_Edit_Dialog(MainWindow mainW)
        {
            this.mainW = mainW;
            InitializeComponent();
            Modification mod = mainW.mod_listView.SelectedItem as Modification;
            this.name_txt.Text = mod.Name;
            this.composition_txt.Text = mod.Composition;
            this.mass_txt.Text = mod.Mod_mass.ToString("F6");
            string position = mod.Position;
            switch (position)
            {
                case "NORMAL": 
                    position = "Anywhere";
                    break;
                case "PEP_N": 
                    position = "PepN-term";
                    break;
                case "PEP_C": 
                    position = "PepC-term";
                    break;
                case "PRO_N": 
                    position = "ProN-term";
                    break;
                case "PRO_C": 
                    position = "ProC-term";
                    break;
            }
            this.position_comboBox.Text = position;
            this.site_txt.Text = mod.Mod_site;
            for (int i = 0; i < mod.Neutral_loss.Count; ++i)
                this.Neutral_Loss_txt.Text += mod.Neutral_loss[i].ToString("F6") + ";";
            this.Common_checkBox.IsChecked = mod.Is_common;
        }

        private void Edit_btn_clk(object sender, RoutedEventArgs e)
        {
            Modification_Element_Edit_Dialog meed = new Modification_Element_Edit_Dialog(this);
            meed.ShowDialog();
        }
        private bool Site_wrong(string site)
        {
            for (int i = 0; i < site.Length; ++i)
            {
                char tmp = site[i];
                if (tmp < 'A' || tmp > 'Z')
                    return true;
                for (int j = 0; j < site.Length; ++j)
                {
                    if (i != j && site[j] == tmp)
                        return true;
                }
            }
            return false;
        }
        private void apply_btn_clk(object sender, RoutedEventArgs e)
        {
            string name = name_txt.Text;
            string composition = composition_txt.Text;
            string mass_str = mass_txt.Text;
            ComboBoxItem cbi = position_comboBox.SelectedItem as ComboBoxItem;
            string position = cbi.Content as string;
            string position_display = "";
            string site = site_txt.Text;
            string neutral_loss = Neutral_Loss_txt.Text;
            bool is_common = (bool)Common_checkBox.IsChecked;
            if (name == "" || composition == "" || !Config_Helper.IsDecimalAllowed(mass_str) || position == "" || site == "")
            {
                if (name == "")
                    name_txt.Background = new SolidColorBrush(Colors.Red);
                if (composition == "")
                    composition_txt.Background = new SolidColorBrush(Colors.Red);
                if (!Config_Helper.IsDecimalAllowed(mass_str))
                    mass_txt.Background = new SolidColorBrush(Colors.Red);
                if (site == "")
                    site_txt.Background = new SolidColorBrush(Colors.Red);
                if (neutral_loss != "")
                {
                    string[] strs = neutral_loss.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < strs.Length; ++i)
                    {
                        if (!Config_Helper.IsDecimalAllowed(strs[i]))
                        {
                            Neutral_Loss_txt.Background = new SolidColorBrush(Colors.Red);
                        }
                    }
                }
                MessageBox.Show(Message_Helper.MO_INPUT_WRONG_Message);
                return;
            }
            if (Site_wrong(site))
            {
                site_txt.Background = new SolidColorBrush(Colors.Red);
                MessageBox.Show(Message_Helper.MO_AA_REPEAT);
                return;
            }
            ObservableCollection<double> neutral_loss_list = new ObservableCollection<double>(); //默认以;进行分隔
            if (neutral_loss != "")
            {
                string[] strs = neutral_loss.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < strs.Length; ++i)
                {
                    if (!Config_Helper.IsDecimalAllowed(strs[i]))
                    {
                        Neutral_Loss_txt.Background = new SolidColorBrush(Colors.Red);
                        MessageBox.Show(Message_Helper.MO_INPUT_WRONG_Message);
                        return;
                    }
                    neutral_loss_list.Add(double.Parse(strs[i]));
                }
            }
            switch (position)
            {
                case "Anywhere":
                    position = "NORMAL";
                    position_display = "NORMAL";
                    break;
                case "PepN-term":
                    position = "PEP_N";
                    position_display = "Peptide N-term";
                    break;
                case "PepC-term":
                    position = "PEP_C";
                    position_display = "Peptide C-term";
                    break;
                case "ProN-term":
                    position = "PRO_N";
                    position_display = "Protein N-term";
                    break;
                case "ProC-term":
                    position = "PRO_C";
                    position_display = "Protein C-term";
                    break;
            }
            Modification modification = new Modification(name, is_common, site, position, double.Parse(mass_str), composition, neutral_loss_list);
            modification.Position_Display = position_display;
            bool is_in = false;
            for(int i=0;i<mainW.modifications.Count;++i)
            {
                if (mainW.mod_listView.SelectedIndex != i && mainW.modifications[i].Name == modification.Name)
                {
                    is_in = true;
                    break;
                }
            }
            if (is_in)
            {
                MessageBox.Show(Message_Helper.NAME_IS_USED_Message);
                return;
            }
            if (!Config_Helper.IsNameRight(modification.Name))
            {
                MessageBox.Show(Message_Helper.NAME_WRONG);
                return;
            }
            int index = mainW.mod_listView.SelectedIndex;
            mainW.modifications[index] = modification;
            mainW.mod_listView.Items.Refresh();
            mainW.is_update[1] = true;
            mainW.is_update_f();
            mainW.mod_listView.SelectedItem = modification;
            mainW.mod_listView.ScrollIntoView(modification);
            this.Close();
        }

        private void txt_focus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.Background = new SolidColorBrush(Colors.Transparent);
        }
    }
}
