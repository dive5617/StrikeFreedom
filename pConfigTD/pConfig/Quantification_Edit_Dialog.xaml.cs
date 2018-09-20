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
    /// Quantification_Edit_Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class Quantification_Edit_Dialog : Window
    {
        public MainWindow mainW;
        public Quantification_Edit_Dialog(MainWindow mainW)
        {
            this.mainW = mainW;
            InitializeComponent();
            Quantification quant = mainW.quant_listView.SelectedItem as Quantification;
            this.name_txt.Text = quant.Name;
        }

        private void Add_btn_clk(object sender, RoutedEventArgs e)
        {
            const int margin = 5;
            RowDefinition rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            this.grid.RowDefinitions.Add(rd);
            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            Grid.SetRow(sp, this.grid.RowDefinitions.Count - 1);
            this.grid.Children.Add(sp);
            TextBlock aa_tbk = new TextBlock();
            aa_tbk.Text = "AA:";
            aa_tbk.Margin = new Thickness(margin);
            sp.Children.Add(aa_tbk);
            TextBox aa_tbx = new TextBox();
            aa_tbx.Margin = new Thickness(margin);
            aa_tbx.Width = 100;
            sp.Children.Add(aa_tbx);
            TextBlock label0_tbk = new TextBlock();
            label0_tbk.Text = "Label0:";
            label0_tbk.Margin = new Thickness(margin);
            sp.Children.Add(label0_tbk);
            TextBox label0_tbx = new TextBox();
            label0_tbx.Margin = new Thickness(margin);
            label0_tbx.Width = 50;
            sp.Children.Add(label0_tbx);
            TextBlock label1_tbk = new TextBlock();
            label1_tbk.Text = "Label1:";
            label1_tbk.Margin = new Thickness(margin);
            sp.Children.Add(label1_tbk);
            TextBox label1_tbx = new TextBox();
            label1_tbx.Margin = new Thickness(margin);
            label1_tbx.Width = 50;
            sp.Children.Add(label1_tbx);
            Button btn = new Button();
            btn.Content = "-";
            btn.Margin = new Thickness(margin);
            btn.Width = this.add_btn.Width;
            btn.Click += (s, er) =>
            {
                Button btn_tmp = s as Button;
                StackPanel sp_tmp = btn_tmp.Parent as StackPanel;
                int row_index = Grid.GetRow(sp_tmp);
                this.grid.Children[row_index].Visibility = Visibility.Collapsed;
            };
            sp.Children.Add(btn);
        }

        private void Apply_btn_clk(object sender, RoutedEventArgs e)
        {
            string name = name_txt.Text;
            if (name == "")
            {
                MessageBox.Show(Message_Helper.QU_NAME_NULL_Message);
                return;
            }

            Quantification new_q = new Quantification(name);
            for (int i = 0; i < this.grid.RowDefinitions.Count; ++i)
            {
                if (this.grid.Children[i].Visibility == Visibility.Collapsed)
                    continue;
                StackPanel sp = this.grid.Children[i] as StackPanel;
                TextBox aa_tbx = sp.Children[1] as TextBox;
                TextBox label0_tbx = sp.Children[3] as TextBox;
                TextBox label1_tbx = sp.Children[5] as TextBox;
                string aa_str = aa_tbx.Text;
                string label0_str = label0_tbx.Text;
                string label1_str = label1_tbx.Text;
                bool is_right = false;
                if (aa_str.Length == 1 && (aa_str[0] == '*' || (aa_str[0] >= 'A' && aa_str[0] <= 'Z')))
                {
                    is_right = true;
                }
                if (!is_right)
                {
                    MessageBox.Show(Message_Helper.QU_AA_A_TO_Z_Message);
                    return;
                }
                is_right = false;
                for (int j = 0; j < mainW.elements.Count; ++j)
                {
                    if (mainW.elements[j].Name == label0_str)
                    {
                        is_right = true;
                        break;
                    }
                }
                if (!is_right)
                {
                    MessageBox.Show(Message_Helper.QU_LABEL0_NAME_Message);
                    return;
                }
                is_right = false;
                for (int j = 0; j < mainW.elements.Count; ++j)
                {
                    if (mainW.elements[j].Name == label1_str)
                    {
                        is_right = true;
                        break;
                    }
                }
                if (!is_right)
                {
                    MessageBox.Show(Message_Helper.QU_LABEL1_NAME_Message);
                    return;
                }
                Quant_Simple qs = new Quant_Simple(aa_str[0], label0_str, label1_str);
                new_q.All_quant.Add(qs);
            }
            new_q.All_quant_str = Quantification.get_string(new_q.Name, new_q.All_quant);
            bool is_in = false;
            for (int i = 0; i < mainW.quantifications.Count; ++i)
            {
                if (mainW.quant_listView.SelectedIndex != i && mainW.quantifications[i].Name == new_q.Name)
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
            if (!Config_Helper.IsNameRight(new_q.Name))
            {
                MessageBox.Show(Message_Helper.NAME_WRONG);
                return;
            }
            int index = mainW.quant_listView.SelectedIndex;
            mainW.quantifications[index] = new_q;
            mainW.quant_listView.Items.Refresh();
            mainW.is_update[2] = true;
            mainW.is_update_f();
            mainW.quant_listView.SelectedItem = new_q;
            mainW.quant_listView.ScrollIntoView(new_q);
            this.Close();
        }
    }
}
