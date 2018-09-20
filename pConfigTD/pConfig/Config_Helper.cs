using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace pConfig
{
    public class Config_Helper
    {
        public static string database_ini = "db.ini";
        public static string modification_ini = "modification.ini";
        public static string element_ini = "element.ini";
        public static string enzyme_ini = "enzyme.ini";
        public static string aa_ini = "aa.ini";
        public static string quant_ini = "quant.ini";

        public static string database_default_ini = "default\\db_default.ini";
        public static string modification_default_ini = "default\\modification_default.ini";
        public static string element_default_ini = "default\\element_default.ini";
        public static string enzyme_default_ini = "default\\enzyme_default.ini";
        public static string aa_default_ini = "default\\aa_default.ini";
        public static string quant_default_ini = "default\\quant_default.ini";

        public static string containment_path = "contaminant.fasta";

        public static bool IsNameRight(string name)
        {
            for (int i = 0; i < name.Length; ++i)
            {
                if (name[i] == '#' || name[i] == '{' || name[i] == '}')
                    return false;
            }
            return true;
        }

        public static bool IsIntegerAllowed(string text)
        {
            //if (text == "")
            //    return false;
            //Regex regex = new Regex("[^0-9]"); //regex that matches disallowed text
            //return !regex.IsMatch(text);
            try
            {
                int res = int.Parse(text);
                return true;
            }
            catch (Exception exe)
            {
                return false;
            }
        }
        public static bool IsDecimalAllowed(string text)
        {
            //if (text == "")
            //    return false;
            //Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            //return !regex.IsMatch(text);
            try
            {
                double res = double.Parse(text);
                return true;
            }
            catch (Exception exe)
            {
                return false;
            }
        }
    }
}
