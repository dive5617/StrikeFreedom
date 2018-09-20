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

namespace pConfig
{
    /// <summary>
    /// Modification_Element_Edit_Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class Modification_Element_Edit_Dialog : Window
    {
        public List<Element> selected_elements = new List<Element>();
        public Modification_Add_Dialog mad;
        public Modification_Edit_Dialog med;
        public Amino_Acid_Edit_Dialog aaed;
        public MainWindow mainW;
        public Modification_Element_Edit_Dialog(Modification_Add_Dialog mad)
        {
            this.mad = mad;
            InitializeComponent();
            this.element_listView.ItemsSource = mad.mainW.elements;
        }
        public Modification_Element_Edit_Dialog(Modification_Edit_Dialog med)
        {
            this.med = med;
            InitializeComponent();
            this.element_listView.ItemsSource = med.mainW.elements;
        }
        public Modification_Element_Edit_Dialog(Amino_Acid_Edit_Dialog aaed, MainWindow mainW)
        {
            this.aaed = aaed;
            this.mainW = mainW;
            InitializeComponent();
            this.element_listView.ItemsSource = mainW.elements;
        }
       
        private void add_element_btn_clk(object sender, RoutedEventArgs e)
        {
            Element selected_element = this.element_listView.SelectedItem as Element; 
            if (selected_element == null)
                return;
            if (selected_elements.Contains(selected_element))
                return;
            selected_elements.Add(selected_element);
            const int margin = 4;
            string name = selected_element.Name;
            RowDefinition rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            this.element_grid.RowDefinitions.Add(rd);
            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            Grid.SetRow(sp, this.element_grid.RowDefinitions.Count - 1);
            this.element_grid.Children.Add(sp);
            TextBlock name_tbk = new TextBlock();
            name_tbk.Width = 90;
            name_tbk.Text = "element_name:";
            name_tbk.Margin = new Thickness(margin);
            sp.Children.Add(name_tbk);
            TextBlock name_tbk2 = new TextBlock();
            name_tbk2.Width = 52;
            name_tbk2.Text = name;
            name_tbk2.FontWeight = FontWeights.Bold;
            name_tbk2.Margin = new Thickness(margin);
            sp.Children.Add(name_tbk2);
            TextBlock number_tbk = new TextBlock();
            number_tbk.Width = 55;
            number_tbk.Text = "number:";
            number_tbk.Margin = new Thickness(margin);
            sp.Children.Add(number_tbk);
            TextBox number_tbx = new TextBox();
            number_tbx.Width = 50;
            number_tbx.Margin = new Thickness(margin);
            sp.Children.Add(number_tbx);
            Button button = new Button();
            button.Content = "-";
            button.Margin = new Thickness(margin);
            button.Width = this.Add_element_btn.Width;
            button.Click += (s, er) =>
            {
                Button btn = s as Button;
                StackPanel sp_tmp = btn.Parent as StackPanel;
                int row_index = Grid.GetRow(sp_tmp);
                this.element_grid.Children[row_index].Visibility = Visibility.Collapsed;
                TextBlock tbk = sp_tmp.Children[1] as TextBlock;
                string element_name = tbk.Text;
                Element element = new Element(element_name);
                selected_elements.Remove(element);
            };
            sp.Children.Add(button);
        }

        private void apply_btn_clk(object sender, RoutedEventArgs e)
        {
            int row_Count = 0;
            for (int i = 0; i < this.element_grid.RowDefinitions.Count; ++i)
            {
                StackPanel sp = this.element_grid.Children[i] as StackPanel;
                if (sp.Visibility != Visibility.Collapsed)
                    ++row_Count;
            }
            if (row_Count == 0)
            {
                MessageBox.Show(Message_Helper.EE_NULL_Message);
                return;
            }
            List<Element_composition> element_composition = new List<Element_composition>();
            for (int i = 0; i < this.element_grid.RowDefinitions.Count; ++i)
            {
                StackPanel sp = this.element_grid.Children[i] as StackPanel;
                if (sp.Visibility == Visibility.Collapsed)
                    continue;
                TextBlock name_tbk=sp.Children[1] as TextBlock;
                TextBox number_tbx = sp.Children[3] as TextBox;
                bool is_Digit = Config_Helper.IsIntegerAllowed(number_tbx.Text);
                if (!is_Digit)
                {
                    MessageBox.Show(Message_Helper.EE_INPUT_NUMBER_Message);
                    return;
                }
                element_composition.Add(new Element_composition(name_tbk.Text, int.Parse(number_tbx.Text), (int)Element.index_hash[name_tbk.Text]));
            }
            string element_str = ""; //根据元素的组成生成字符串，并同时计算质量
            double element_mass = 0.0;
            if (med != null)
            {
                for (int i = 0; i < element_composition.Count; ++i)
                {
                    element_str += element_composition[i].Element_name + "(" + element_composition[i].Element_number + ")";
                    element_mass += med.mainW.elements[element_composition[i].Element_index].MMass * element_composition[i].Element_number;
                }
            }
            else if (mad != null)
            {
                for (int i = 0; i < element_composition.Count; ++i)
                {
                    element_str += element_composition[i].Element_name + "(" + element_composition[i].Element_number + ")";
                    element_mass += mad.mainW.elements[element_composition[i].Element_index].MMass * element_composition[i].Element_number;
                }
            }
            else if (aaed != null)
            {
                for (int i = 0; i < element_composition.Count; ++i)
                {
                    element_str += element_composition[i].Element_name + "(" + element_composition[i].Element_number + ")";
                    element_mass += mainW.elements[element_composition[i].Element_index].MMass * element_composition[i].Element_number;
                }
            }
            if (mad != null)
            {
                mad.composition_txt.Background = new SolidColorBrush(Colors.Transparent);
                mad.mass_txt.Background = new SolidColorBrush(Colors.Transparent);
                mad.composition_txt.Text = element_str;
                mad.mass_txt.Text = element_mass.ToString("F6");
            }
            else if (aaed != null)
            {
                aaed.composition_txt.Text = element_str;
                aaed.mass_txt.Text = element_mass.ToString("F6");
            }
            else if (med != null)
            {
                med.composition_txt.Text = element_str;
                med.mass_txt.Text = element_mass.ToString("F6");
            }
            this.Close();
        }
    }
}
