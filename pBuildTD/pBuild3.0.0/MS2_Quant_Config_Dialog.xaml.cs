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
    /// MS2_Quant_Config_Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class MS2_Quant_Config_Dialog : Window
    {
        public MainWindow mainW;
        

        public MS2_Quant_Config_Dialog(MainWindow mainW)
        {
            InitializeComponent();
            this.mainW = mainW;
            if (this.mainW.ms2_quant_help == null || this.mainW.ms2_quant_help.nc_terms.Count != 2)
                return;
            this.n_term1_tbx.Text = this.mainW.ms2_quant_help.nc_terms[0].n_mass.ToString("F5");
            this.n_term2_tbx.Text = this.mainW.ms2_quant_help.nc_terms[1].n_mass.ToString("F5");
            this.c_term1_tbx.Text = this.mainW.ms2_quant_help.nc_terms[0].c_mass.ToString("F5");
            this.c_term2_tbx.Text = this.mainW.ms2_quant_help.nc_terms[1].c_mass.ToString("F5");
            if (this.mainW.summary_param_information == null)
                return;
            double ppm_mass_error = this.mainW.ms2_quant_help.Ppm_mass_error;
            double da_mass_error = this.mainW.ms2_quant_help.Da_mass_error;
            if (ppm_mass_error != 0.0)
            {
                this.mass_error_cb.SelectedIndex = 0;
                this.mass_error_tbx.Text = (ppm_mass_error * 1e6).ToString("F0");
            }
            else
            {
                this.mass_error_cb.SelectedIndex = 1;
                this.mass_error_tbx.Text = da_mass_error.ToString("F1");
            }
            this.n_term1_aa_tbx.Text = this.mainW.ms2_quant_help.nc_terms[0].n_str;
            this.n_term2_aa_tbx.Text = this.mainW.ms2_quant_help.nc_terms[1].n_str;
            this.c_term1_aa_tbx.Text = this.mainW.ms2_quant_help.nc_terms[0].c_str;
            this.c_term2_aa_tbx.Text = this.mainW.ms2_quant_help.nc_terms[1].c_str;
            this.mz1_tbx.Text = this.mainW.ms2_quant_help.mz1.ToString("F1");
            this.mz2_tbx.Text = this.mainW.ms2_quant_help.mz2.ToString("F1");
            this.match_Ion_type_tbx.Text = Config_Help.MS2_Match_Ion_Type;
        }

        private void ok_btn(object sender, RoutedEventArgs e)
        {
            string n1_str = this.n_term1_tbx.Text;
            string n2_str = this.n_term2_tbx.Text;
            string c1_str = this.c_term1_tbx.Text;
            string c2_str = this.c_term2_tbx.Text;
            string me_str = this.mass_error_tbx.Text;
            string n1_aa_str = this.n_term1_aa_tbx.Text;
            string n2_aa_str = this.n_term2_aa_tbx.Text;
            string c1_aa_str = this.c_term1_aa_tbx.Text;
            string c2_aa_str = this.c_term2_aa_tbx.Text;

            string mz1_str = this.mz1_tbx.Text;
            string mz2_str = this.mz2_tbx.Text;

            Config_Help.MS2_Match_Ion_Type = this.match_Ion_type_tbx.Text;

            if (!Config_Help.IsDecimalAllowed(n1_str) || !Config_Help.IsDecimalAllowed(n2_str) || !Config_Help.IsDecimalAllowed(c1_str)
                || !Config_Help.IsDecimalAllowed(c2_str) || !Config_Help.IsDecimalAllowed(me_str))
            {
                MessageBox.Show(Message_Help.ALL_TERM_BE_DOUBLE);
                return;
            }
            if (!Config_Help.IsRightAA(n1_aa_str) || !Config_Help.IsRightAA(n2_aa_str) || !Config_Help.IsRightAA(c1_aa_str)
                || !Config_Help.IsRightAA(c2_aa_str))
            {
                MessageBox.Show(Message_Help.ALL_AA_WRONG);
                return;
            }

            if (!Config_Help.IsDecimalAllowed(mz1_str) || !Config_Help.IsDecimalAllowed(mz2_str))
            {
                MessageBox.Show(Message_Help.ALL_TERM_BE_DOUBLE);
                return;
            }

            double n1_mass = double.Parse(n1_str);
            double n2_mass = double.Parse(n2_str);
            double c1_mass = double.Parse(c1_str);
            double c2_mass = double.Parse(c2_str);
            double me_mass = double.Parse(me_str);
            double mz1 = double.Parse(mz1_str);
            double mz2 = double.Parse(mz2_str);
            if (this.mass_error_cb.SelectedIndex == 0)
                mainW.ms2_quant_help = new MS2_Quant_Help(n1_mass, n2_mass, c1_mass, c2_mass, n1_aa_str,
                    n2_aa_str, c1_aa_str, c2_aa_str, me_mass * 1e-6, 0.0, mz1, mz2);
            else
                mainW.ms2_quant_help = new MS2_Quant_Help(n1_mass, n2_mass, c1_mass, c2_mass, n1_aa_str,
                    n2_aa_str, c1_aa_str, c2_aa_str, 0.0, me_mass, mz1, mz2);

            this.Cursor = Cursors.Wait;
            mainW.ms2_quant();

            this.Close();
            this.Cursor = null;
        }

        private void closed_event(object sender, EventArgs e)
        {
            this.mainW.ms2_quant_config_dialog = null;
        }
    }
}
