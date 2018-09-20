using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
namespace pFind
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args != null && e.Args.Count() > 0)
            {
                this.Properties["ArbitraryArgName"] = e.Args[0];
            }
            base.OnStartup(e);
        }
    }

    //class Program
    //{
    //    [STAThread]
    //    static void Main(string[] args)
    //    {
    //        SingleInstanceApp a = new SingleInstanceApp();
    //        a.Run(args);
    //    }
    //}

    ///// <summary>
    ///// App.xaml 的交互逻辑
    ///// </summary>
    //public partial class App : System.Windows.Application
    //{
    //    protected override void OnStartup(System.Windows.StartupEventArgs e)
    //    {
    //        base.OnStartup(e);
    //        InitializeComponent();
    //    }
    //}

    //public class SingleInstanceApp : WindowsFormsApplicationBase
    //{
    //    App win = null;
    //    public SingleInstanceApp()
    //    {
    //        this.IsSingleInstance = true;
    //    }

    //    protected override bool OnStartup(Microsoft.VisualBasic.ApplicationServices.StartupEventArgs eventArgs)
    //    {
    //        win = new App();
    //        win.Run();
    //        return false;
    //    }

    //    protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
    //    {
    //        foreach (System.Windows.Window _win in win.Windows)
    //        {
    //            if (_win.Visibility == System.Windows.Visibility.Visible)
    //            {
    //                _win.Activate();
    //            }
    //        }
    //    }
    //}

}
