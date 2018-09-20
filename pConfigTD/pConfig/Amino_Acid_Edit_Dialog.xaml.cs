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
    /// Amino_Acid_Edit_Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class Amino_Acid_Edit_Dialog : Window
    {
        public MainWindow mainW;
        public Amino_Acid aa;
        public Amino_Acid_Edit_Dialog(MainWindow mainW, Amino_Acid aa)
        {
            this.mainW = mainW;
            this.aa = aa;
            InitializeComponent();
            this.name_txt.Text = aa.Name;
            this.composition_txt.Text = aa.Composition;
            this.mass_txt.Text = aa.Mass.ToString("F6");
        }

        private void Edit_composition_btn_clk(object sender, RoutedEventArgs e)
        {
            Modification_Element_Edit_Dialog meed = new Modification_Element_Edit_Dialog(this, this.mainW);
            meed.ShowDialog();
        }

        private void Apply_btn_clk(object sender, RoutedEventArgs e)
        {
            aa.Composition = this.composition_txt.Text;
            double mass = 0.0;
            aa.Element_composition = Element_composition.parse(mainW, aa.Composition, ref mass);
            aa.Mass = mass;
            int aa_index = mainW.aas.IndexOf(aa);
            mainW.aas[aa_index] = aa;
            mainW.aa_listView.ItemsSource = mainW.aas;
            mainW.aa_listView.Items.Refresh();
            mainW.is_update[4] = true;
            mainW.is_update_f();
            mainW.aa_listView.SelectedItem = aa;
            mainW.aa_listView.ScrollIntoView(aa);
            this.Close();
        }
    }
}
