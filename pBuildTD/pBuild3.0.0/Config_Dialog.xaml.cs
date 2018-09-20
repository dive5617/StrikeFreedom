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
    /// Config_Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class Config_Dialog : Window
    {
        public MainWindow mainW;
        public Config_Dialog(MainWindow mainW)
        {
            InitializeComponent();
            this.mainW = mainW;
            this.display_ratio_index_cb.Text = Config_Help.ratio_index + "";
        }

        private void ok_clk(object sender, RoutedEventArgs e)
        {
            ComboBoxItem selected_cb = this.display_ratio_index_cb.SelectedItem as ComboBoxItem;
            int ratio_index = int.Parse(selected_cb.Content as string);
            if (Config_Help.ratio_index == ratio_index)
            {
                MessageBox.Show(Message_Help.NO_CHANGE);
                return;
            }
            Config_Help.ratio_index = ratio_index;
            this.Close();

            string task_path = mainW.task.folder_path;
            mainW.task = null;
            mainW.Remove_Task_TreeView(task_path);
            mainW.Initial_Task_AddTreeView(task_path);
        }
    }
}
