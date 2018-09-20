using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
    /// Database_Add_Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class Database_Add_Dialog : Window
    {
        public MainWindow MainW;
        public Database_Add_Dialog(MainWindow MainW)
        {
            this.MainW = MainW;
            InitializeComponent();
        }

        private void cancel_btn_clk(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ok_btn_clk(object sender, RoutedEventArgs e)
        {
            if (Name_txt.Text == "" || Path_txt.Text == "")
            {
                MessageBox.Show(Message_Helper.DB_NAME_PATH_NULL_Message);
                return;
            }
            if (!File.Exists(Path_txt.Text))
            {
                MessageBox.Show(Message_Helper.DB_PATH_NOT_EXIST_Message);
                return;
            }
            Database new_database = new Database(Name_txt.Text, Path_txt.Text);
            if (MainW.databases.Contains(new_database))
            {
                MessageBox.Show(Message_Helper.NAME_IS_USED_Message);
                return;
            }
            if (!Config_Helper.IsNameRight(new_database.DB_Name))
            {
                MessageBox.Show(Message_Helper.NAME_WRONG);
                return;
            }
            if ((bool)this.add_con_cbx.IsChecked)
            {
                string path = Path_txt.Text;
                string old_path = path;
                path = path.Replace(".fasta", "_contaminant.fasta");
                Path_txt.Text = path;
                if (!File_Helper.add_database_containment(old_path, path, Config_Helper.containment_path))
                    return;
                new_database.DB_Path = Path_txt.Text;
                MessageBox.Show(Message_Helper.DB_CONTAINMENT_PATH_Message + new_database.DB_Path +".");
            }
            MainW.add_databases.Add(new_database);
            MainW.databases.Add(new_database);
            MainW.is_update[0] = true;
            MainW.is_update_f();
            MainW.db_listView.SelectedItem = new_database;
            MainW.db_listView.ScrollIntoView(new_database);
            this.Close();

            if (Application.Current.Properties["ArbitraryArgName"] != null)
            {
                MainW.is_update[0] = false;
                bool save_ok = File_Helper.save_DB(Config_Helper.database_ini, MainW.databases);
                MainW.Close();
            }
        }

        private void open_btn_clk(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog op = new Microsoft.Win32.OpenFileDialog();
            op.InitialDirectory = @"c:\";
            op.RestoreDirectory = true;
            op.Filter = "文本文件(*.fasta)|*.fasta";
            op.ShowDialog();
            if (op.FileName == "")
                return;
            Path_txt.Text = op.FileName;
            int index = op.FileName.LastIndexOf('\\');
            int index2 = op.FileName.LastIndexOf('.');
            string fasta_name = op.FileName.Substring(index + 1, index2 - index - 1);
            if (Name_txt.Text == "")
                Name_txt.Text = fasta_name;
        }

        private void add_con_check_clk(object sender, RoutedEventArgs e)
        {
            string conFlag = "_contaminant";
            if ((bool)this.add_con_cbx.IsChecked)
            {
                this.Name_txt.Text += conFlag;
            }
            else
            {
                this.Name_txt.Text = this.Name_txt.Text.Substring(0, this.Name_txt.Text.Length - conFlag.Length);
            }
           
        }
    }
}
