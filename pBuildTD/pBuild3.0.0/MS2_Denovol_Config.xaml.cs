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
    /// MS2_Denovol_Config.xaml 的交互逻辑
    /// </summary>
    public partial class MS2_Denovol_Config : Window
    {
        public MS2_Denovol_Config()
        {
            InitializeComponent();

            update();
        }

        private void update()
        {
            this.table.RowDefinitions.Clear();
            this.table.Children.Clear();
            List<Denovol_Config.DCC> dcc = Denovol_Config.All_mass;
            for (int i = 0; i < dcc.Count; ++i)
            {
                if (!dcc[i].CanUse)
                    continue;
                RowDefinition rd = new RowDefinition();
                rd.Height = GridLength.Auto;
                this.table.RowDefinitions.Add(rd);
                string name = dcc[i].Name;
                double mass = dcc[i].Mass;
                TextBlock tb = new TextBlock();
                tb.Margin = new Thickness(3);
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                tb.Text = name;
                TextBlock tb2 = new TextBlock();
                tb2.Margin = new Thickness(3);
                tb2.HorizontalAlignment = HorizontalAlignment.Center;
                tb2.Text = mass.ToString("F2") + " Da";
                Button delete_btn = new Button();
                delete_btn.Margin = new Thickness(3);
                delete_btn.Content = "－";
                TextBlock del_tb = new TextBlock();
                del_tb.Text = (this.table.RowDefinitions.Count - 1) + "";
                del_tb.Visibility = Visibility.Collapsed;
                delete_btn.ToolTip = del_tb;
                delete_btn.Click += (s, e) =>
                {
                    TextBlock del_tb0 = delete_btn.ToolTip as TextBlock;
                    int index = int.Parse(del_tb0.Text);
                    Denovol_Config.All_mass[index].CanUse = false;
                    for (int c = 0; c < this.table.Children.Count; ++c)
                    {
                        UIElement ui_element = this.table.Children[c];
                        if (Grid.GetRow(ui_element) == index)
                            ui_element.Visibility = Visibility.Collapsed;
                    }
                };
                Grid.SetRow(tb, this.table.RowDefinitions.Count - 1);
                Grid.SetColumn(tb, 0);
                Grid.SetRow(tb2, this.table.RowDefinitions.Count - 1);
                Grid.SetColumn(tb2, 1);
                Grid.SetRow(delete_btn, this.table.RowDefinitions.Count - 1);
                Grid.SetColumn(delete_btn, 2);
                this.table.Children.Add(tb);
                this.table.Children.Add(tb2);
                this.table.Children.Add(delete_btn);
            }
        }

        private void add_btn_clk(object sender, RoutedEventArgs e)
        {
            string name = name_tb.Text;
            if (!Config_Help.IsDecimalAllowed(mass_tb.Text))
                return;
            double mass = double.Parse(mass_tb.Text);
            Denovol_Config.All_mass.Add(new Denovol_Config.DCC(mass, name));
            update();
        }

        private void reset_clk(object sender, RoutedEventArgs e)
        {
            Denovol_Config.initial();
            update();
        }
    }
}
