using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Data;

namespace pFind
{
    class MyColorConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            bool result = false;
            if (value != null)
            {
                DataRowView drv = value as DataRowView;
                try
                {
                    if (drv != null)
                        if (Boolean.TryParse(drv.Row["Choice"].ToString(), out result))
                        {
                            if (result)
                                brush = new SolidColorBrush(Colors.Red);

                            else if (result == false)
                                brush = new SolidColorBrush(Colors.YellowGreen);
                        }
                }
                catch { }
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
