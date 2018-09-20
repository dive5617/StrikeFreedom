using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class Aa
    {
        public List<string> elements; //该氨基酸对应的元素组成
        public List<int> numbers; //该氨基酸对应的元素的个数
        public Aa()
        {
            this.elements = new List<string>();
            this.numbers = new List<int>();
        }
        public void add(string element, int number)
        {
            elements.Add(element);
            numbers.Add(number);
        }
        public double get_mass()
        {
            double mass = 0.0;
            for (int i = 0; i < elements.Count; ++i)
            {
                double tmp = (double)Config_Help.element_hash[elements[i]];
                mass += tmp * numbers[i];
            }
            return mass;
        }
        // 该氨基酸中元素及对应数目
        public static Aa parse_Aa_byString(string elements)
        {
            string[] element_str = elements.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            string element_name = "";
            Aa aa = new Aa();
            for (int i = 0; i < element_str.Length; ++i)
            {
                if (i % 2 == 0)
                {
                    element_name = element_str[i];
                }
                else
                {
                    int number = int.Parse(element_str[i]);
                    aa.add(element_name, number);
                }
            }
            return aa;
        }
        public static string parse_String_byAa(Aa aa)
        {
            if (aa == null)
                return "";
            string res = "";
            for (int i = 0; i < aa.elements.Count; ++i)
            {
                res += aa.elements[i] + "(";
                res += aa.numbers[i] + ")";
            }
            return res;
        }
    }
}
