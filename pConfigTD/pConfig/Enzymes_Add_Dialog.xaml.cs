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
    public partial class Enzymes_Add_Dialog : Window
    {
        public MainWindow mainW;
        public Enzymes_Add_Dialog(MainWindow mainW)
        {
            this.mainW = mainW;
            InitializeComponent();
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
            if (mainW.enzymes.Contains(enzyme))
            {
                MessageBox.Show(Message_Helper.NAME_IS_USED_Message);
                return;
            }
            if (!Config_Helper.IsNameRight(enzyme.Name))
            {
                MessageBox.Show(Message_Helper.NAME_WRONG);
                return;
            }
            mainW.add_enzymes.Add(enzyme);
            mainW.enzymes.Add(enzyme);
            mainW.is_update[3] = true;
            mainW.is_update_f();
            mainW.enzyme_listView.SelectedItem = enzyme;
            mainW.enzyme_listView.ScrollIntoView(enzyme);
            this.Close();
        }
    }
}
