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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace pBuild
{
    /// <summary>
    /// Tmp.xaml 的交互逻辑
    /// </summary>
    public partial class TmpWindow : Window
    {
        public TmpWindow()
        {
            InitializeComponent();
        }

        private void key_down(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                EMFCopy.CopyVisualToWmfClipboard((Visual)this.tmp_border, Window.GetWindow(this));
                this.tmp_oxy.RefreshPlot(true);
                MessageBox.Show("OK");
            }
        }
    }
}
