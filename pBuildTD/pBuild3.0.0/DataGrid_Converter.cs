using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace pBuild
{
    public class DataGrid_Converter_TargetDecoy : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool is_target_flag = (bool)value;
            if (is_target_flag)
                return "target";
            return "decoy";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = value as string;
            if (strValue == "target")
                return true;
            return false;
        }
    }
    public class DataGrid_Converter_LabelName : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string value_str = value as string;
            if (value_str == null)
                return null;
            try
            {
                int label_index = int.Parse(value_str);
                return Config_Help.label_name[label_index];
            }
            catch (Exception exe)
            {
                return value_str;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string label_str = value as string;
            for (int i = 0; i < Config_Help.label_name.Length; ++i)
            {
                if (Config_Help.label_name[i] == label_str)
                    return i.ToString();
            }
            return "1";
        }
    }
    public class DataGrid_Converter_ModSites : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string value_str = value as string;
            if (value_str == null)
                return null;
            if (!value_str.Contains("#"))
                return null;
            string result = "";
            string[] strs = value_str.Split(new char[] { ',', '#', ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 2; i < strs.Length; i += 3)
            {
                result += strs[i - 2] + "," + strs[i - 1] + "(" + Config_Help.label_name[int.Parse(strs[i])] + ");";
            }
            return result;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string value_str = value as string;
            string result = "";
            string[] strs = value_str.Split(new char[] { ',', '(', ')', ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 2; i < strs.Length; i += 3)
            {
                int index = 0;
                for (int j = 0; j < Config_Help.label_name.Length; ++j)
                {
                    if (Config_Help.label_name[j] == strs[i])
                    {
                        index = j;
                        break;
                    }
                }
                result += strs[i - 2] + "," + strs[i - 1] + "#" + index + ";";
            }
            return result;
        }
    }
}
