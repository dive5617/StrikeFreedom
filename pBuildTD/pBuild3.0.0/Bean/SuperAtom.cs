using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class SuperAtom
    {
        public int type; //有几种同位素
        public double[] mass = new double[100]; //每种同位素的质量
        public double[] prop = new double[100]; // 每种同位素的丰度
        public double mono_mass = 0.0; //元素的mono质量

        public static double low_scale = double.MinValue;
        public static double high_scale = 1;
        public static SuperAtom[] super = new SuperAtom[150]; //每个元素的质量及丰度

        public static SuperAtom CalculateMass(SuperAtom a, SuperAtom b)
        {
            SuperAtom newSuperAtom = new SuperAtom();
            int length_f = a.type;
            int length_g = b.type;
            int new_type = 0;
            for (int k = 0; k < length_f + length_g; ++k)
            {
                double sumweight = 0, summass = 0;
                int start = k < (length_f - 1) ? 0 : k - length_f + 1;
                int end = k < (length_g - 1) ? k : length_g - 1;
                if (start > end)
                    break;
                for (int i = start; i <= end; ++i) //start==end==0,单同位素峰
                {
                    double weight = b.prop[i] * a.prop[k - i];
                    double ma = b.mass[i] + a.mass[k - i];
                    sumweight += weight;
                    summass += weight * ma;
                }
                if (sumweight >= low_scale && sumweight <= high_scale && sumweight != 0)
                    new_type++;
                else
                    break;
                if (new_type > newSuperAtom.mass.Length)
                    break;
                newSuperAtom.mass[new_type - 1] = summass / sumweight;
                newSuperAtom.prop[new_type - 1] = sumweight;
            }
            if (new_type > newSuperAtom.mass.Length)
                new_type = newSuperAtom.mass.Length;
            newSuperAtom.type = new_type;
            double new_mono_mass = a.mono_mass + b.mono_mass;
            double new_min_me = double.MaxValue;
            int min_index = -1;
            for (int i = 0; i < new_type; ++i)
            {
                double me = Math.Abs(new_mono_mass - newSuperAtom.mass[i]);
                if (me < new_min_me)
                {
                    min_index = i;
                    new_min_me = me;
                }
            }
            newSuperAtom.mass[min_index] = new_mono_mass;
            newSuperAtom.mono_mass = new_mono_mass;
            return newSuperAtom;
        }
        public static void ParseElement(Aa aa, int[] element_num, ref int num)
        {
            num = super.Length;
            for (int i = 0; i < num; ++i)
                element_num[i] = 0;
            for (int j = 0; j < aa.elements.Count; ++j)
            {
                int index0 = (int)Config_Help.element_index_hash[aa.elements[j]];

                if (index0 != -1)
                    element_num[index0] += aa.numbers[j];
            }
        }
        public static void ParseElement(Peptide p, int index, int[] element_num, ref int num)
        {
            //根据肽段来获取CHNOSP,13C,2H,15N,元素的个数
            num = super.Length;
            for (int i = 0; i < num; ++i)
                element_num[i] = 0;
            for (int i = 0; i < p.Sq.Length; ++i)
            {
                Aa aa = Config_Help.aas[index, p.Sq[i] - 'A'];
                for (int j = 0; j < aa.elements.Count; ++j)
                {
                    int index0 = (int)Config_Help.element_index_hash[aa.elements[j]];

                    if (index0 != -1)
                        element_num[index0] += aa.numbers[j];
                }
            }
            //考虑修饰对应的元素个数
            for (int i = 0; i < p.Mods.Count; ++i)
            {
                Aa[] aas = Config_Help.modStr_elements_hash[p.Mods[i].Mod_name] as Aa[];
                
                for (int j = 0; j < aas[0].elements.Count; ++j)
                {
                    int index0 = (int)Config_Help.element_index_hash[aas[0].elements[j]];
                    if (index0 != -1)
                    {
                        if (aas[0].numbers[j] < 0 && element_num[index0] == 0) //处理肽段没有对应的元素，但是修饰需要减元素
                        {
                            string noLabelName = aas[0].elements[j].Last() + "";
                            int index1 = (int)Config_Help.element_index_hash[noLabelName];
                            if (index1 != -1)
                                element_num[index1] += aas[0].numbers[j];
                        }
                        else
                            element_num[index0] += aas[0].numbers[j];
                    }
                }
            }
            //增加一个H2O
            element_num[(int)Config_Help.element_index_hash["H"]] += 2;
            element_num[(int)Config_Help.element_index_hash["O"]] += 1;
        }
        public static void ParseElement(Peptide p, int index, List<int> mod_flag, int[] element_num, ref int num)
        {
            //根据肽段来获取CHNOSP,13C,2H,15N,元素的个数
            num = super.Length;
            for (int i = 0; i < num; ++i)
                element_num[i] = 0;
            for (int i = 0; i < p.Sq.Length; ++i)
            {
                Aa aa = Config_Help.aas[index, p.Sq[i] - 'A'];
                for (int j = 0; j < aa.elements.Count; ++j)
                {
                    int index0 = (int)Config_Help.element_index_hash[aa.elements[j]];
                    
                    if (index0 != -1)
                        element_num[index0] += aa.numbers[j];
                }
            }
            //考虑修饰对应的元素个数
            for (int i = 0; i < p.Mods.Count; ++i)
            {
                Aa[] aas = Config_Help.modStr_elements_hash[p.Mods[i].Mod_name] as Aa[];
                if (aas[mod_flag[i]] == null)
                    mod_flag[i] = 0;
                for (int j = 0; j < aas[mod_flag[i]].elements.Count; ++j)
                {
                    int index0 = (int)Config_Help.element_index_hash[aas[mod_flag[i]].elements[j]];
                    if (index0 != -1)
                    {
                        element_num[index0] += aas[mod_flag[i]].numbers[j];
                    }
                }
            }
            //增加一个H2O
            element_num[(int)Config_Help.element_index_hash["H"]] += 2;
            element_num[(int)Config_Help.element_index_hash["O"]] += 1;
        }
        public static SuperAtom ControlMass(Peptide p, int index, List<int> mod_flag)
        {
            if (p.Tag_Flag != index)
            {
                for (int i = 0; i < mod_flag.Count; ++i)
                    mod_flag[i] = (mod_flag[i] == 2 ? 0 : 2);
            }
            if (index == 1) //如果是轻标，那么需要用轻标进行计算
            {
                for (int i = 0; i < mod_flag.Count; ++i)
                    mod_flag[i] = 0;
            }
            //对mod_flag进行更新，如果本来是重标，但是搜索的时候没有考虑修饰的重标，不存在质量表，则进行更新
            for (int i = 0; i < mod_flag.Count; ++i)
            {
                if (Config_Help.mod_label_name[mod_flag[i]] == "")
                    mod_flag[i] = 0;
            }
            int[] element_num = new int[super.Length];
            int num = 0;
            ParseElement(p, index, mod_flag, element_num, ref num);
            return parse(element_num);
        }
        private static SuperAtom parse(int[] element_num)
        {
            SuperAtom origin;
            SuperAtom newSuperAtom = new SuperAtom();
            newSuperAtom.type = 1;
            newSuperAtom.mass[0] = 0;
            newSuperAtom.prop[0] = 1;
            newSuperAtom.mono_mass = newSuperAtom.mass[0];
            int numAtom = 0;
            for (int i = 0; i < element_num.Length; ++i)
            {
                origin = super[i];
                numAtom = element_num[i];
                while (numAtom > 0)
                {
                    if (numAtom % 2 == 1)
                    {
                        newSuperAtom = CalculateMass(newSuperAtom, origin);
                    }
                    origin = CalculateMass(origin, origin);
                    numAtom /= 2;
                }
            }
            return newSuperAtom;
        }
        public static SuperAtom ControlMass(Peptide p1, Peptide p2, string link_name, int index1, int index2)
        {
            int[] element_num1 = new int[super.Length];
            int[] element_num2 = new int[super.Length];
            int[] element_num3 = new int[super.Length];
            int[] element_num = new int[super.Length];
            int num = 0;
            ParseElement(p1, index1, element_num1, ref num);
            ParseElement(p2, index2, element_num2, ref num);
            ParseElement(Config_Help.link_elements_hash[link_name] as Aa, element_num3, ref num);
            for (int i = 0; i < num; ++i)
                element_num[i] = element_num1[i] + element_num2[i] + element_num3[i];
            return parse(element_num);
        }
    }
    
}
