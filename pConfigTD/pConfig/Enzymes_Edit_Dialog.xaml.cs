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
    /// Enzymes_Add_Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class Enzymes_Edit_Dialog : Window
    {
        public MainWindow mainW;
        public Enzymes_Edit_Dialog(MainWindow mainW)
        {
            this.mainW = mainW;
            InitializeComponent();
            Enzyme enzy = mainW.enzyme_listView.SelectedItem as Enzyme;
            this.name_txt.Text = enzy.Name;
            this.cleave_txt.Text = enzy.Cleave_site;
            string n_c = enzy.N_C;
            switch (n_c)
            {
                case "N": 
                    n_c = "N-term";
                    break;
                case "C": 
                    n_c = "C-term";
                    break;
            }
            this.N_C_comboBox.Text = n_c;
            this.ignore_txt.Text = enzy.Ignore_site;
            if (this.ignore_txt.Text == "_")
                this.ignore_txt.Text = "";
        }

        private void Apply_btn_clk(object sender, RoutedEventArgs e)
        {
            string name = this.name_txt.Text;
            string cleave = this.cleave_txt.Text;
            string ignore = this.ignore_txt.Text;
            ComboBoxItem cbi = this.N_C_comboBox.SelectedItem as ComboBoxItem;
            string n_c = cbi.Content as string;
            switch (n_c)
            {
                case "N-term":
                    n_c = "N";
                    break;
                case "C-term":
                    n_c = "C";
                    break;
            }
            bool is_flag = true;
            for (int i = 0; i < cleave.Length; ++i)
            {
                if (cleave[i] < 'A' || cleave[i] > 'Z')
                {
                    is_flag = false;
                    break;
                }
            }
            if (!is_flag)
            {
                MessageBox.Show(Message_Helper.EN_CLEAVE_A_TO_Z_Message);
                return;
            }
            if (cleave == "")
            {
                MessageBox.Show(Message_Helper.EN_CLEAVE_NULL_Message);
                return;
            }
            is_flag = true;
            for (int i = 0; i < ignore.Length; ++i)
            {
                if (ignore[i] < 'A' || ignore[i] > 'Z')
                {
                    is_flag = false;
                    break;
                }
            }
            if (!is_flag)
            {
                MessageBox.Show(Message_Helper.EN_IGNORE_A_TO_Z_Message);
                return;
            }
            if (ignore == "")
                ignore = "_";
            Enzyme enzyme = new Enzyme(name, cleave, ignore, n_c);
            bool is_in = false;
            for (int i = 0; i < mainW.enzymes.Count; ++i)
            {
                if (mainW.enzyme_listView.SelectedIndex != i && mainW.enzymes[i].Name == enzyme.Name)
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
            if (!Config_Helper.IsNameRight(enzyme.Name))
            {
                MessageBox.Show(Message_Helper.NAME_WRONG);
                return;
            }
            int index = mainW.enzyme_listView.SelectedIndex;
            mainW.enzymes[index] = enzyme;
            mainW.enzyme_listView.Items.Refresh();
            mainW.is_update[3] = true;
            mainW.is_update_f();
            mainW.enzyme_listView.SelectedItem = enzyme;
            mainW.enzyme_listView.ScrollIntoView(enzyme);
            this.Close();
        }
    }
}
