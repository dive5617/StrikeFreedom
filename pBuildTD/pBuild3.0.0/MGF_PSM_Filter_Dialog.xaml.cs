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

namespace pBuild
{
    /// <summary>
    /// MGF_psm_filter_dialog.xaml 的交互逻辑
    /// </summary>
    public partial class MGF_PSM_Filter_Dialog : Window
    {
        public MainWindow mainW;

        public MGF_PSM_Filter_Dialog(MainWindow mainW)
        {
            InitializeComponent();
            this.mainW = mainW;
        }

        private ObservableCollection<PSM> filter_byTitle(string title, ObservableCollection<PSM> all_psms)
        {
            if (title == "")
                return all_psms;
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            for (int i = 0; i < all_psms.Count; ++i)
            {
                if (all_psms[i].Title.Contains(title))
                    psms.Add(all_psms[i]);
            }
            return psms;
        }
        private void filter_btn_clk(object sender, RoutedEventArgs e)
        {
            string title = this.title_subStr_txt.Text;
            ObservableCollection<PSM> psms = filter_byTitle(title, mainW.psms);
            mainW.data.ItemsSource = psms;
            mainW.display_size.Text = psms.Count + "";
        }

        private void enter_keyDown(object sender, KeyEventArgs e)
        {
            filter_btn_clk(null, null);
        }
    }
}
