using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace pTop.classes
{
    public static class CustomCommands
    {
        private static RoutedUICommand run;

        public static RoutedUICommand Run
        {
            get 
            {
                if (run == null)
                { 
                   run=new RoutedUICommand(
                      "Run",
                      "Run",
                      typeof(MainWindow),
                      new InputGestureCollection{new KeyGesture(Key.F5,ModifierKeys.None)}
                   );
                }
                return run;
            }

            set { CustomCommands.run = value; }
        }

        private static RoutedUICommand rename;

        public static RoutedUICommand Rename
        {
            get 
            {
                if (rename == null)
                {
                    rename = new RoutedUICommand(
                       "Rename",
                       "Rename",
                       typeof(MainWindow),
                       new InputGestureCollection { new KeyGesture(Key.F2, ModifierKeys.None) }
                    );
                }
                return rename;
            }
            set { CustomCommands.rename = value; }
        }
        private static RoutedUICommand callpbuild;

        public static RoutedUICommand CallpBuild
        {
            get
            {
                if (callpbuild == null)
                {
                    callpbuild = new RoutedUICommand(
                       "Call pBuild",
                       "CallpBuild",
                       typeof(MainWindow),
                       new InputGestureCollection { new KeyGesture(Key.F7, ModifierKeys.None) }
                    );
                }
                return callpbuild;
            }
            set { CustomCommands.callpbuild = value; }
        }

        private static RoutedUICommand _new;

        public static RoutedUICommand New
        {
            get
            {
                if (_new == null)
                {
                    _new = new RoutedUICommand(
                       "New",
                       "New",
                       typeof(MainWindow),
                       new InputGestureCollection { new KeyGesture(Key.N, ModifierKeys.Control) }
                    );
                }
                return _new;
            }
            set { CustomCommands._new = value; }
        }

        private static RoutedUICommand open;

        public static RoutedUICommand Open
        {
            get 
            {
                if (open == null)
                {
                    open = new RoutedUICommand(
                        "Open",
                        "Open",
                        typeof(MainWindow),
                        new InputGestureCollection { new KeyGesture(Key.O, ModifierKeys.Control) }
                    );
                }
                return open;            
            }
            set { CustomCommands.open = value; }
        }

        private static RoutedUICommand save;

        public static RoutedUICommand Save
        {
            get
            {
                if (save == null)
                {
                    save = new RoutedUICommand(
                        "Save",
                        "Save",
                        typeof(MainWindow),
                        new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control) }
                    );
                }
                return save;
            }
            set { CustomCommands.save = value; }
        }

        private static RoutedUICommand saveas;

        public static RoutedUICommand SaveAs
        {
            get
            {
                if (saveas == null)
                {
                    saveas = new RoutedUICommand(
                        "SaveAs",
                        "SaveAs",
                        typeof(MainWindow),
                        new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Shift | ModifierKeys.Control) }
                    );
                }
                return saveas;
            }
            set { CustomCommands.saveas = value; }
        }

        

    }
}
