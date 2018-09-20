using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace pBuild
{
    public class Config_Help
    {
        public static int MaxMass = 100000;   // [wrm] TD质量范围比较大，暂定100000为上界
        public static double fdr_value = 0.01;
        public static bool is_ppm_flag = true;
        public static double mass_error = 20e-6; //20ppm
        //0表示N14，1表示N15，最多支持四类氨基酸表
        public static double[,] mass_index = new double[4,26];
        public static string[] label_name = new string[4]{"","","",""}; //肽段标记的名字
        public static string[] mod_label_name = new string[4] { "", "", "", "" }; //修饰标记的名字
        public const double massZI = 1.00727647012;
        public const double massH = 1.007825035;
        public const double mass_ISO = 1.003;
        public const double massO = 15.99491463;
        public const double massC = 12.00000;
        public const double massN = 14.0030732;
        public const double massN15 = 15.0001088;
        public const double massH2O = massH * 2 + massO;
        public const double massNH3 = massN + massH * 3;

        public const double B_Mass = 0.0;
        public const double A_Mass = - massC - massO;
        public const double C_Mass = massNH3;

        public const double Y_Mass = massH2O;
        public const double X_Mass = massC + massO * 2;
        public const double Z_Mass = - massN + massO;

        public static System.Collections.Hashtable link_hash = new System.Collections.Hashtable(); //交联剂名对应的质量
        public static System.Collections.Hashtable link_elements_hash = new System.Collections.Hashtable(); //交联剂名对应的元素

        public static System.Collections.Hashtable modStr_hash = new System.Collections.Hashtable(); //修饰名对应的质量
        public static System.Collections.Hashtable modStrLoss_hash = new System.Collections.Hashtable(); //修饰名对应的中性丢失的质量列表，一个List<double>
        public static ObservableCollection<string>[] normal_mod = new ObservableCollection<string>[26];
        public static ObservableCollection<string>[] PEP_N_mod = new ObservableCollection<string>[26];
        public static ObservableCollection<string>[] PEP_C_mod = new ObservableCollection<string>[26];
        public static ObservableCollection<string>[] PRO_N_mod = new ObservableCollection<string>[26];
        public static ObservableCollection<string>[] PRO_C_mod = new ObservableCollection<string>[26];

        public static System.Collections.Hashtable element_hash = new System.Collections.Hashtable(); //元素名对应的质量，以Mono质量为准
        public static System.Collections.Hashtable element_index_hash = new System.Collections.Hashtable(); //元素名对应的索引位置
        public static Aa[,] aas = new Aa[4,26]; //每种氨基酸对应的元素表
        public static System.Collections.Hashtable modStr_elements_hash = new System.Collections.Hashtable(); //修饰名对应的元素，是一个Aa

        public static Theory_IOSTOPE[] all_theory_iostope = new Theory_IOSTOPE[100000 + 1];

        public static int AA_Normal_Index = 0;

        //Config配置参数：
        public static int ratio_index = 1; //默认显示Ratio2，可以选择Ratio1或者Ratio2

        //二级谱定量，选择哪个离子进行定量，默认是a1+离子
        public static string MS2_Match_Ion_Type = "a1+";

        //Filter_Dialog窗口记录的过滤条件
        public static PSM_Filter_Bean psm_filter_bean = new PSM_Filter_Bean(0, "", 0, "", 0, "Show All", "Show All", "Show All", "Show All", "Target", false, false, 1024.0, 0.0, 1.0, 0.0);


        public static bool IsMatch_RegularExpression(string match_string, string regular_expression)
        {
            return Regex.IsMatch(match_string, regular_expression);
        }
        
        //判断字符串是否包含非法字符，"A-Z"以外的字符均为非法字符
        public static bool IsRightAA(string text)
        {
            for (int i = 0; i < text.Length; ++i)
            {
                if (text[i] < 'A' || text[i] > 'Z')
                    return false;
            }
            return true;
        }
        public static string delete_underline(string param)
        {
            string result = "";
            result = param.Replace('_', ' ');
            return result;
        }
        //判断字符串是否为整数
        public static bool IsIntegerAllowed(string text)
        {
            //if (text == "")
            //    return false;
            //Regex regex = new Regex("[^0-9]"); //regex that matches disallowed text
            //return !regex.IsMatch(text);
            try
            {
                int.Parse(text);
                return true;
            }
            catch (Exception exe)
            {
                return false;
            }
        }
        //判断字符串是否为浮点数
        public static bool IsDecimalAllowed(string text)
        {
            //if (text == "")
            //    return false;
            //Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            //return !regex.IsMatch(text);
            try
            {
                double.Parse(text);
                return true;
            }
            catch (Exception exe)
            {
                return false;
            }
        }
        public static string getReport(string information, string[] args)
        {
            string result = "";
            int index = 0;
            for (int i = 0; i < information.Length; ++i)
            {
                if (information[i] != '$')
                    result += information[i];
                else
                    result += args[index++];
            }
            return result;
        }
        //为了支持pNovo，需要根据元素及氨基酸配置文件，来更新氨基酸的质量信息
        public static void update_AA_Mass()
        {
            for (int i = 0; i < 26; ++i)
            {
                double mass = 0.0;
                for (int j = 0; j < Config_Help.aas[0, i].elements.Count; ++j)
                {
                    mass += ((double)Config_Help.element_hash[Config_Help.aas[0, i].elements[j]]) * Config_Help.aas[0, i].numbers[j];
                }
                Config_Help.mass_index[0, i] = mass;
            }
        }
        public static int get_label_index(string label_name)
        {
            for (int i = 0; i < Config_Help.label_name.Length; ++i)
                if (Config_Help.label_name[i] == label_name)
                    return i;
            return 0;
        }
        //根据内核使用的轻重标形式，来重新计算氨基酸表。目前，先按N14和N15生成两个氨基酸质量表
        /*
        public static void update_AA_Mass()
        {
            //N14一张氨基酸质量表
            for (int i = 0; i < 26; ++i)
            {
                Aa aa = aas[i];
                double aa_mass = 0.0;
                for (int j = 0; j < aa.elements.Count; ++j)
                {
                    string element_name=aa.elements[j];
                    if (element_name == "N")
                        element_name = "15N";
                    aa_mass += aa.numbers[j] * (double)element_hash[element_name];
                }
                mass_index[0, i] = aa_mass;
            }
            //N15一张氨基酸质量表
            for (int i = 0; i < 26; ++i)
            {
                Aa aa = aas[i];
                double aa_mass = 0.0;
                for (int j = 0; j < aa.elements.Count; ++j)
                {
                    string element_name = aa.elements[j];
                    if (element_name == "N")
                        element_name = "14N";
                    aa_mass += aa.numbers[j] * (double)element_hash[element_name];
                }
                mass_index[1, i] = aa_mass;
            }
        }
         * */

        private Config_Help() { }
    }
    public class Theory_IOSTOPE
    {
        public List<double> intensity; //理论同位素峰簇的相对强度
        public List<double> mz;

        public Theory_IOSTOPE()
        {
            intensity = new List<double>();
            mz = new List<double>();
        }
    }

    //蛋白质鉴定到的子序列
    public class Interval : IComparable
    {
        public int Start; //相对于蛋白质的开始位置
        public int End; //相对于蛋白质的结束位置
        public int old_index; //原先相对List<Interval>中的索引位置

        public  List<int> psms_index; //有哪些二级谱图（即PSM列表）鉴定到该肽段。为psms的索引号

        public int layer_number; //子序列需要画在哪一层中，由于序列与序列之间有重叠需要以不同的层次进行绘制
        public int Count; //该子序列有多少个PSM鉴定到，值等于psms_index.Count()
        public int interval_next_index; //该子序列的下层子序列
        public int interval_previous_index; //该子序列的上层子序列，因为蛋白质序列每行以60个字母进行绘制，可能该子序列正好被隔断，那么需要记录隔断之前的子序列的索引号
        public int all_interval_index; //方便进行统计，该值意义不大

        //public int row_index, column_index;
        //最后在面板上显示的时候，row_index,column_index分别表示显示的是第几行及第几个列(列表示StackPanel中的第几个元素，该元素是Border，而不是TextBlock

        public List<Protein_Modification> modification;
        //子序列的修饰信息，包括修饰相对于蛋白质的位置及修饰的名字，不同的修饰使用不同的颜色进行绘制


        int IComparable.CompareTo(Object obj)
        {
            Interval temp = (Interval)obj;
            if (this.Start < temp.Start)
                return -1;
            else if (this.Start > temp.Start)
                return 1;
            if (this.End < temp.End)
                return -1;
            else if (this.End > temp.End)
                return 1;
            return 0;
        }

        public Interval(int start, int end, int old_index)
        {
            this.Start = start;
            this.End = end;
            this.old_index = old_index;
            psms_index = new List<int>();
            this.Count = 1;
            this.interval_previous_index = -1;
            this.all_interval_index = -1;
            this.modification = new List<Protein_Modification>();
        }
        public bool is_insert(Interval interval)
        {
            if (this.Start >= interval.Start && this.Start <= interval.End)
                return true;
            return false;
        }

        public override bool Equals(object obj)
        {
            var interval = obj as Interval;
            if (interval == null)
                return false;
            if (this.Start == interval.Start && this.End == interval.End)
                return true;
            return false;
        }
        public static bool operator ==(Interval a, Interval b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Start == b.Start && a.End == b.End;
        }
        public static bool operator !=(Interval a, Interval b)
        {
            return !(a == b);
        }
        public override int GetHashCode()
        {
            return this.Start ^ this.End;
        }
    }

    public class Interval_Sort1 : Comparer<Interval>
    {
        public override int Compare(Interval x, Interval y)
        {
            if (x.Start < y.Start)
                return -1;
            else if (x.Start > y.Start)
                return 1;
            if (x.interval_previous_index != -1 && y.interval_previous_index == -1)
                return -1;
            else if (x.interval_previous_index == -1 && y.interval_previous_index != -1)
                return 1;
            if (x.End < y.End)
                return -1;
            else if (x.End > y.End)
                return 1;
            return 0;
        }
    }
    public class Interval_Sort2 : Comparer<Interval>
    {
        public override int Compare(Interval x, Interval y)
        {
            if (x.old_index < y.old_index)
                return -1;
            else if (x.old_index > y.old_index)
                return 1;
            return 0;
        }
    }

    public class AA_Cover : IComparable
    {
        public int index;
        public int psms_index;

        public AA_Cover(int index, int psms_index)
        {
            this.index = index;
            this.psms_index = psms_index;
        }

        int IComparable.CompareTo(Object obj)
        {
            AA_Cover aa_cover = (AA_Cover)obj;
            if (this.index < aa_cover.index)
                return -1;
            else if (this.index > aa_cover.index)
                return 1;
            return 0;
        }
    }
    //理论谱图预测模型
    public class Interv
    {
        public double split, score;

        public Interv(double split, double score)
        {
            this.split = split;
            this.score = score;
        }
    }
    public class ION
    {
        public double[] feature = new double[193 * 2 + 1];

        public static bool addB(List<ION> ions, double mass, string sq, int j, int flag, int[] aa_int)
        {
            const double min_mass = 0.0;
            const double max_mass = 2000.0;
            const double max_min = max_mass - min_mass;
            int start_i = 0;
            if (flag == 1)
                start_i = 2;
            ION ion = new ION();
            if (mass >= min_mass && mass <= max_mass)
            {
                ion.feature[start_i * 193 + 1] = (mass - min_mass) / max_min;
                if (mass - min_mass < max_mass - mass)
                    ion.feature[start_i * 193 + 2] = mass - min_mass;
                else
                    ion.feature[start_i * 193 + 3] = max_mass - mass;
                if (j - 2 >= 0)
                    ion.feature[start_i * 193 + aa_int[sq[j - 2] - 'A'] + 4] = 1;
                if (j - 1 >= 0)
                    ion.feature[start_i * 193 + aa_int[sq[j - 1] - 'A'] + 23] = 1;
                ion.feature[start_i * 193 + aa_int[sq[j] - 'A'] + 42] = 1;
                if ((j + 1) < sq.Length)
                    ion.feature[start_i * 193 + aa_int[sq[j + 1] - 'A'] + 61] = 1;
                if ((j + 2) < sq.Length)
                    ion.feature[start_i * 193 + aa_int[sq[j + 2] - 'A'] + 80] = 1;
                if ((j + 3) < sq.Length)
                    ion.feature[start_i * 193 + aa_int[sq[j + 3] - 'A'] + 99] = 1;
                int k = j - 3;
                for (; k >= 0; --k)
                {
                    ion.feature[start_i * 193 + aa_int[sq[k] - 'A'] + 118]++;
                }
                ion.feature[start_i * 193 + aa_int[sq[0] - 'A'] + 156] = 1;
		        ion.feature[start_i * 193 + aa_int[sq[sq.Length - 1] - 'A'] + 175] = 1;
		        ions.Add(ion);
                return true;
            }
            return false;
        }

        public static bool addY(List<ION> ions, double mass, string sq, int j, int flag, int[] aa_int)
        {
            const double min_mass = 0.0;
            const double max_mass = 2000.0;
            const double max_min = max_mass - min_mass;
            int start_i = 0;
            if (flag == 1)
                start_i = 2;
            ION ion = new ION();
            if (mass >= min_mass && mass <= max_mass)
            {
                ion.feature[start_i * 193 + 1 + 193] = (mass - min_mass) / max_min;
                if (mass - min_mass < max_mass - mass)
                    ion.feature[start_i * 193 + 2 + 193] = mass - min_mass;
                else
                    ion.feature[start_i * 193 + 3 + 193] = max_mass - mass;
                if (j - 3 >= 0)
                    ion.feature[start_i * 193 + aa_int[sq[j - 3] - 'A'] + 4 + 193] = 1;
                if (j - 2 >= 0)
                    ion.feature[start_i * 193 + aa_int[sq[j - 2] - 'A'] + 23 + 193] = 1;
                if (j - 1 >= 0)
                    ion.feature[start_i * 193 + aa_int[sq[j - 1] - 'A'] + 42 + 193] = 1;
                ion.feature[start_i * 193 + aa_int[sq[j] - 'A'] + 61 + 193] = 1;
                if ((j + 1) < sq.Length)
                    ion.feature[start_i * 193 + aa_int[sq[j + 1] - 'A'] + 80 + 193] = 1;
                if ((j + 2) < sq.Length)
                    ion.feature[start_i * 193 + aa_int[sq[j + 2] - 'A'] + 99 + 193] = 1;
                int k = j - 4;
                for (; k >= 0; --k)
                {
                    ion.feature[start_i * 193 + aa_int[sq[k] - 'A'] + 118 + 193]++;
                }
                k = j + 3;
                for (; k < sq.Length; ++k)
                {
                    ion.feature[start_i * 193 + aa_int[sq[k] - 'A'] + 137 + 193]++;
                }
                ion.feature[start_i * 193 + aa_int[sq[0] - 'A'] + 156 + 193] = 1;
                ion.feature[start_i * 193 + aa_int[sq[sq.Length - 1] - 'A'] + 175 + 193] = 1;
                ions.Add(ion);
                return true;
            }
            return false;
        }
    }
}
