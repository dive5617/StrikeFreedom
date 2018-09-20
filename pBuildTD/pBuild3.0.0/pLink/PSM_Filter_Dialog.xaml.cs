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

namespace pBuild.pLink
{
    /// <summary>
    /// PSM_Filter_Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class PSM_Filter_Dialog : Window
    {
        public MainWindow mainW;
        public PSM_Filter_Dialog(MainWindow mainW)
        {
            InitializeComponent();
            this.mainW = mainW;
        }

        private void filter_btn(object sender, RoutedEventArgs e)
        {
            int mix_num = 0;
            if(this.filter_mix_num.Text != "")
            { 
                if (!Config_Help.IsIntegerAllowed(this.filter_mix_num.Text))
                    return;
                mix_num = int.Parse(this.filter_mix_num.Text);
            }
            string title = this.title_subStr_txt.Text;
            string sq = this.sq_subStr_txt.Text;
            double ratio = 1024.0;
            double ratio2 = 0.0;
            if (this.ratio1_tbx.Text != "")
                ratio = double.Parse(this.ratio1_tbx.Text);
            if (this.ratio2_tbx.Text != "")
                ratio2 = double.Parse(this.ratio2_tbx.Text);
            pLink.pLink_Result pLink_result = mainW.pLink_result;
            ObservableCollection<pLink.PSM> all_psms = pLink_result.psms;
            all_psms = filter_by_title(all_psms, title);
            all_psms = filter_by_sq(all_psms, sq);
            all_psms = filter_by_mixNumber(all_psms, mix_num);
            all_psms = filter_byRatio_Less(all_psms, ratio);
            all_psms = filter_byRatio_Bigger(all_psms, ratio2);
            mainW.data.ItemsSource = all_psms;
            mainW.display_size.Text = all_psms.Count + "";
        }
        public ObservableCollection<pLink.PSM> filter_byRatio_Less(ObservableCollection<pLink.PSM> all_psms, double ratio)
        {
            if (ratio == 1024.0)
                return all_psms;
            ObservableCollection<pLink.PSM> psms = new ObservableCollection<pLink.PSM>();
            for (int i = 0; i < all_psms.Count; ++i)
            {
                if (all_psms[i].Ratio <= ratio)
                {
                    psms.Add(all_psms[i]);
                }
            }
            return psms;
        }
        public ObservableCollection<pLink.PSM> filter_byRatio_Bigger(ObservableCollection<pLink.PSM> all_psms, double ratio)
        {
            if (ratio == 0.0)
                return all_psms;
            ObservableCollection<pLink.PSM> psms = new ObservableCollection<pLink.PSM>();
            for (int i = 0; i < all_psms.Count; ++i)
            {
                if (all_psms[i].Ratio >= ratio)
                {
                    psms.Add(all_psms[i]);
                }
            }
            return psms;
        }
        private ObservableCollection<pLink.PSM> filter_by_mixNumber(ObservableCollection<pLink.PSM> all_psms, int mix_num)
        {
            if (mix_num <= 1)
                return all_psms;
            ObservableCollection<pLink.PSM> psms = new ObservableCollection<PSM>();
            System.Collections.Hashtable scan_hash = new System.Collections.Hashtable();
            for(int i=0;i<all_psms.Count;++i)
            {
                int scan = all_psms[i].get_scan();
                if (scan_hash[scan] == null)
                    scan_hash[scan] = 1;
                else
                {
                    int num = (int)scan_hash[scan];
                    scan_hash[scan] = num + 1;
                }
            }
            for(int i=0;i<all_psms.Count;++i)
            {
                int scan = all_psms[i].get_scan();
                int number = (int)scan_hash[scan];
                if (number >= mix_num)
                    psms.Add(all_psms[i]);
            }
            return psms;
        }
        private ObservableCollection<pLink.PSM> filter_by_title(ObservableCollection<pLink.PSM> all_psms, string title)
        {
            if (title == "")
                return all_psms;
            ObservableCollection<pLink.PSM> psms = new ObservableCollection<PSM>();
            for(int i=0;i<all_psms.Count;++i)
            {
                if (all_psms[i].Title.Contains(title))
                    psms.Add(all_psms[i]);
            }
            return psms;
        }
        private ObservableCollection<pLink.PSM> filter_by_sq(ObservableCollection<pLink.PSM> all_psms, string sq)
        {
            if (sq == "")
                return all_psms;
            ObservableCollection<pLink.PSM> psms = new ObservableCollection<PSM>();
            for(int i=0;i<all_psms.Count;++i)
            {
                if (all_psms[i].Sq.Contains(sq))
                    psms.Add(all_psms[i]);
            }
            return psms;
        }
        private void enter_keyDown(object sender, KeyEventArgs e)
        {
            filter_btn(null, null);
        }

        private void closed_event(object sender, EventArgs e)
        {
            this.mainW.pLink_psm_filter_dialog = null;
        }
    }
}
