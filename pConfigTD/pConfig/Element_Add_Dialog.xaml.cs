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
    /// Element_Add_Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class Element_Add_Dialog : Window
    {
        public MainWindow mainW;
        public Element_Add_Dialog(MainWindow mainW)
        {
            this.mainW = mainW;
            InitializeComponent();
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
            TextBlock mass_tbk = new TextBlock();
            mass_tbk.Text = "Mass:";
            mass_tbk.Margin = new Thickness(margin);
            sp.Children.Add(mass_tbk);
            TextBox mass_tbx = new TextBox();
            mass_tbx.Margin = new Thickness(margin);
            mass_tbx.Width = 100;
            sp.Children.Add(mass_tbx);
            TextBlock ratio_tbk = new TextBlock();
            ratio_tbk.Text = "Abundance:";
            ratio_tbk.Margin = new Thickness(margin);
            sp.Children.Add(ratio_tbk);
            TextBox ratio_tbx = new TextBox();
            ratio_tbx.Margin = new Thickness(margin);
            ratio_tbx.Width = 100;
            sp.Children.Add(ratio_tbx);
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
            string name = this.name_txt.Text;
            Element element = new Element(name);
            if (mainW.elements.Contains(element))
            {
                MessageBox.Show(Message_Helper.NAME_IS_USED_Message);
                return;
            }
            if (!Config_Helper.IsNameRight(element.Name))
            {
                MessageBox.Show(Message_Helper.NAME_WRONG);
                return;
            }
            List<double> mass_list = new List<double>();
            List<double> ratio_list = new List<double>();
            for (int i = 0; i < this.grid.RowDefinitions.Count; ++i)
            {
                if (this.grid.Children[i].Visibility == Visibility.Collapsed)
                    continue;
                StackPanel sp = this.grid.Children[i] as StackPanel;
                TextBox mass_tbx = sp.Children[1] as TextBox;
                TextBox ratio_tbx = sp.Children[3] as TextBox;
                string mass_str = mass_tbx.Text;
                string ratio_str = ratio_tbx.Text;
                if (!Config_Helper.IsDecimalAllowed(mass_str) || !Config_Helper.IsDecimalAllowed(ratio_str))
                {
                    MessageBox.Show(Message_Helper.EL_MASS_RATIO_DOUBLE_Message);
                    return;
                }
                mass_list.Add(double.Parse(mass_str));
                ratio_list.Add(double.Parse(ratio_str));
            }
            double all_ratio = 0.0;
            double max_mass = 0.0, max_ratio = 0.0;
            for (int i = 0; i < mass_list.Count; ++i)
            {
                all_ratio += ratio_list[i];
                if (ratio_list[i] > max_ratio)
                {
                    max_ratio = ratio_list[i];
                    max_mass = mass_list[i];
                }
            }
            if (Math.Abs(all_ratio - 1.0) >= 1.0e-9)
            {
                MessageBox.Show(Message_Helper.EL_SUM_RATIO_ONE_Message);
                return;
            }
            element.MMass = max_mass;
            element.Mass = mass_list;
            element.Ratio = ratio_list;
            element.update_mass_str();
            mainW.elements.Add(element);
            Element.index_hash[element.Name] = mainW.elements.Count - 1;
            mainW.is_update[5] = true;
            mainW.is_update_f();
            mainW.element_listView.SelectedItem = element;
            mainW.element_listView.ScrollIntoView(element);
            this.Close();
        }
    }
}
