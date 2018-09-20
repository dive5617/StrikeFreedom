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

namespace pFind
{
    /// <summary>
    /// About.xaml 的交互逻辑
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
        }

        private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://pfind.ict.ac.cn");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Hyperlink_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                string sEmailMSG = "mailto:" + "pfind@ict.ac.cn" + "?subject=" + "Report problems of pFind/pBuild:";
                System.Diagnostics.Process.Start(sEmailMSG);
            }
            catch(Exception exe)
            {
                MessageBox.Show(exe.Message);
            }
        }
    }
}
