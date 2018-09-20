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

namespace pBuild
{
    /// <summary>
    /// Protein_Adanvaced_Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class Protein_Adanvaced_Dialog : Window
    {
        public MainWindow mainW;
        public Protein_Adanvaced_Dialog(MainWindow mainW)
        {
            this.mainW = mainW;
            InitializeComponent();
            this.alphabet_txt.Text = this.mainW.protein_panel.pddh.alphabet_num_per_row + "";
        }

        private void update_clk(object sender, RoutedEventArgs e)
        {
            string alphabet_str = this.alphabet_txt.Text;
            try
            {
                int number = int.Parse(alphabet_str);
                if (number < 40)
                {
                    MessageBox.Show(Message_Help.PROTEIN_AA_LENGTH);
                    return;
                }
                mainW.protein_panel.pddh.alphabet_num_per_row = number;
                mainW.display_protein();
            }
            catch (Exception exe)
            {
                MessageBox.Show(Message_Help.PROTEIN_AA_LENGTH_INTEGER);
                return;
            }
        }

        private void closed_event(object sender, EventArgs e)
        {
            this.mainW.protein_adanvaced_dialog = null;
        }
    }
}
