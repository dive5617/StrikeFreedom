using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace pBuild
{
    public class Protein_Help
    {
        public int alphabet_num_per_row = 60;
        public Grid grid;
        public Protein_Help(int alphabet, Grid grid)
        {
            this.alphabet_num_per_row = alphabet;
            this.grid = grid;
        }
        //length表示一行显示多少个字母
        public static List<Protein_Fragment> split(Protein protein, ObservableCollection<PSM> psms, int length)
        {
            List<Protein_Fragment> fragments = new List<Protein_Fragment>();
            int start = 0;
            int end = length - 1;
            while (true)
            {
                if (end >= protein.SQ.Length - 1)
                {
                    end = protein.SQ.Length - 1;
                    break;
                }
                fragments.Add(new Protein_Fragment(start, end));
                start += length;
                end += length; 
            }
            fragments.Add(new Protein_Fragment(start, end));
            List<Interval> all_intervals = new List<Interval>(); // 为了去冗余
            string sq = protein.SQ;
            for (int i = 0; i < protein.psm_index.Count; ++i)
            {
                int sq_index = sq.IndexOf(psms[protein.psm_index[i]].Sq);
                Interval interval = new Interval(sq_index, sq_index + psms[protein.psm_index[i]].Sq.Length - 1, -1);
                bool is_in = false;
                int j = 0;
                for (; j < all_intervals.Count; ++j)
                {
                    if (all_intervals[j] == interval)
                    {
                        is_in = true;
                        break;
                    }
                }
                if (!is_in)
                {
                    interval.psms_index.Add(protein.psm_index[i]);
                    all_intervals.Add(interval);
                }
                else
                {
                    all_intervals[j].psms_index.Add(protein.psm_index[i]);
                    ++all_intervals[j].Count;
                    continue;
                }
                int frag_index = interval.Start / length;
                if (interval.End <= fragments[frag_index].End)
                {
                    interval = new Interval(interval.Start, interval.End, fragments[frag_index].intervals.Count());
                    interval.all_interval_index = all_intervals.Count - 1;
                    fragments[frag_index].intervals.Add(interval);
                }
                else
                {
                    interval = new Interval(interval.Start, interval.End, fragments[frag_index].intervals.Count());
                    interval.interval_next_index = fragments[frag_index + 1].intervals.Count;
                    interval.all_interval_index = all_intervals.Count - 1;
                    int old_end = interval.End;
                    interval.End = fragments[frag_index].End;
                    fragments[frag_index].intervals.Add(interval);
                    interval = new Interval(fragments[frag_index + 1].Start, old_end, fragments[frag_index + 1].intervals.Count());
                    interval.interval_previous_index = fragments[frag_index].intervals.Count - 1;
                    interval.all_interval_index = all_intervals.Count - 1;
                    fragments[frag_index + 1].intervals.Add(interval);
                }
            }
            int max_layer_number = 0;
            for (int i = 0; i < fragments.Count; ++i)
            {
                if (fragments[i].intervals.Count == 0)
                    continue;
                //更新interval的Count
                for (int j = 0; j < fragments[i].intervals.Count; ++j)
                {
                    fragments[i].intervals[j].Count = all_intervals[fragments[i].intervals[j].all_interval_index].Count;
                    fragments[i].intervals[j].psms_index = all_intervals[fragments[i].intervals[j].all_interval_index].psms_index;
                }
                int max_layer = 1;
                List<Interval> intervals = fragments[i].intervals;
                //List<Interval> old_intervals = new List<Interval>(intervals);

                intervals.Sort(new Interval_Sort1());
                if(max_layer_number < intervals.Count)
                    max_layer_number = intervals.Count;
                int[] layer_max_number = new int[max_layer_number + 2];
                max_layer_number = intervals.Count;

                if (i == 0)
                {
                    for (int j = 0; j < layer_max_number.Length; ++j)
                    {
                        layer_max_number[j] = -2;
                    }
                }
                else
                {
                    for (int j = 0; j < layer_max_number.Length; ++j)
                    {
                        layer_max_number[j] = fragments[i - 1].End - 1;
                    }
                }
                /*
                if (intervals[0].interval_index == -1)
                    intervals[0].layer_number = 1;
                else
                {
                    intervals[0].layer_number = fragments[i - 1].intervals[intervals[0].interval_index].layer_number;
                    max_layer = intervals[0].layer_number;
                }
                layer_max_number[1] = intervals[0].End;
                */
                for (int j = 0; j < intervals.Count; ++j)
                {
                    if (intervals[j].interval_previous_index != -1)
                    {
                        intervals[j].layer_number = fragments[i - 1].intervals[intervals[j].interval_previous_index].layer_number;
                        if (max_layer < intervals[j].layer_number)
                            max_layer = intervals[j].layer_number;
                        layer_max_number[intervals[j].layer_number] = intervals[j].End;
                        continue;
                    }
                    bool is_insert_in = false;
                    int lay_number = 1;
                    while (lay_number <= max_layer) //intervals[j - 1].layer_number
                    {
                        if (intervals[j].Start > layer_max_number[lay_number] + 1)
                        {
                            is_insert_in = true;
                            intervals[j].layer_number = lay_number;
                            if (intervals[j].layer_number > max_layer)
                                max_layer = intervals[j].layer_number;
                            if (layer_max_number[lay_number] < intervals[j].End)
                                layer_max_number[lay_number] = intervals[j].End;
                            break;
                        }
                        ++lay_number;
                    }
                    if (!is_insert_in)
                    {
                        intervals[j].layer_number = max_layer + 1;
                        //if (intervals[j].layer_number > max_layer)
                        max_layer = intervals[j].layer_number;
                        if (layer_max_number[intervals[j].layer_number] < intervals[j].End)
                            layer_max_number[intervals[j].layer_number] = intervals[j].End;
                    }
                }
                //更新intervals的layer_number
                intervals.Sort(new Interval_Sort2());
                fragments[i].Max_layer = max_layer;
            }
            //更新protein的修饰列表
            update_protein_modification(protein, psms);
            //只有先更新了protein的修饰列表，才能更新interval中的修饰列表，因为interval中的修饰列表需要protein的修饰列表的索引号
            for (int i = 0; i < fragments.Count; ++i)
            {
                for (int j = 0; j < fragments[i].intervals.Count; ++j)
                {
                    update_interval_mods(fragments, i, fragments[i].intervals[j], protein, psms);
                }
                fragments[i].intervals.Sort(new Interval_Sort1());
            }
            return fragments;
        }
        //查看一个序列中的是否包含修饰，返回蛋白protein中的modification修饰列表的索引位置
        private static void update_protein_modification(Protein protein, ObservableCollection<PSM> psms)
        {
            List<Protein_Mod> modification = new List<Protein_Mod>();
            for (int i = 0; i < protein.psm_index.Count; ++i)
            {
                string mod_sits = psms[protein.psm_index[i]].Mod_sites;
                List<string> mods = Modification.get_modSites_list(mod_sits);
                for (int p = 0; p < mods.Count; ++p)
                {
                    Protein_Mod mod_tmp = new Protein_Mod(mods[p]);
                    int index = modification.IndexOf(mod_tmp);
                    if (index == -1)
                    {
                        modification.Add(mod_tmp);
                    }
                    else
                    {
                        ++modification[index].mod_count;
                    }
                }
            }
            modification.Sort();
            protein.modification = modification;
        }
        private static void update_interval_mods(List<Protein_Fragment> fragments, int floor, Interval one_interval, Protein protein, ObservableCollection<PSM> psms)
        {
            List<Protein_Modification> modification = new List<Protein_Modification>();
            for (int k = 0; k < one_interval.psms_index.Count; ++k)
            {
                string mod_sits = psms[one_interval.psms_index[k]].Mod_sites;
                int start = one_interval.Start;
                if (one_interval.interval_previous_index != -1)
                {
                    start = fragments[floor - 1].intervals[one_interval.interval_previous_index].Start;
                }
                List<Protein_Modification> mods = Modification.get_modSites_list(start, psms[one_interval.psms_index[k]].Sq.Length, mod_sits, protein);
                for (int p = 0; p < mods.Count; ++p)
                {
                    if (!modification.Contains(mods[p]) && mods[p].mod_site >= one_interval.Start && mods[p].mod_site <= one_interval.End)
                    {
                        modification.Add(mods[p]);
                    }
                }
            }
            modification.Sort();
            one_interval.modification = modification;
        }
        public static ObservableCollection<Protein_Group> update_group(ObservableCollection<Protein> proteins)
        {
            ObservableCollection<Protein_Group> groups = new ObservableCollection<Protein_Group>();
            for (int i = 0; i < proteins.Count; ++i)
            {
                if (proteins[i].Parent_Protein_AC != "")
                    continue;
                Protein_Group group = new Protein_Group(proteins[i]);
                for (int j = 0; j < proteins[i].Protein_index.Count; ++j)
                {
                    group.Protein_Children.Add(new Protein_Group(proteins[proteins[i].Protein_index[j]]));
                }
                groups.Add(group);
            }
            return groups;
        }
        public static void update_MS2_ratio(ObservableCollection<Protein> proteins, ObservableCollection<PSM> psms)
        {
            for (int i = 0; i < proteins.Count; ++i)
            {
                List<double> ratios = new List<double>();
                List<double> a1_ratios = new List<double>(); 
                System.Collections.Hashtable sq_hash = new System.Collections.Hashtable();
                System.Collections.Hashtable sq_hash_a1 = new System.Collections.Hashtable();
                List<int> psm_index = proteins[i].psm_index;
                for (int j = 0; j < psm_index.Count; ++j)
                {
                    int index = psm_index[j];
                    PSM one_psm = psms[index];
                    if (sq_hash[one_psm.Sq] == null)
                    {
                        List<double> rr = new List<double>();
                        if (one_psm.Ms2_ratio != 0.0)
                        {
                            rr.Add(Math.Log10(one_psm.Ms2_ratio) / Math.Log10(2.0));
                        }
                        sq_hash[one_psm.Sq] = rr;
                        rr = new List<double>();
                        if (one_psm.Ms2_ratio_a1 != 0.0)
                        {
                            rr.Add(Math.Log10(one_psm.Ms2_ratio_a1) / Math.Log10(2.0));
                        }
                        sq_hash_a1[one_psm.Sq] = rr;
                    }
                    else
                    {
                        if (one_psm.Ms2_ratio != 0.0)
                        {
                            List<double> rr = sq_hash[one_psm.Sq] as List<double>;
                            rr.Add(Math.Log10(one_psm.Ms2_ratio) / Math.Log10(2.0));
                            sq_hash[one_psm.Sq] = rr;
                        }
                        if (one_psm.Ms2_ratio_a1 != 0.0)
                        {
                            List<double> rr = sq_hash_a1[one_psm.Sq] as List<double>;
                            rr.Add(Math.Log10(one_psm.Ms2_ratio_a1) / Math.Log10(2.0));
                            sq_hash_a1[one_psm.Sq] = rr;
                        }
                    }
                }
                foreach(string key in sq_hash.Keys)
                {
                    List<double> rr = sq_hash[key] as List<double>;
                    if(rr.Count>0)
                    {
                        rr.Sort();
                        if(rr.Count % 2 != 0)
                            ratios.Add(rr[rr.Count / 2]);
                        else
                            ratios.Add((rr[rr.Count / 2] + rr[rr.Count / 2 - 1]) / 2);
                    }
                }
                foreach (string key in sq_hash_a1.Keys)
                {
                    List<double> rr = sq_hash_a1[key] as List<double>;
                    if (rr.Count > 0)
                    {
                        rr.Sort();
                        if (rr.Count % 2 != 0)
                            a1_ratios.Add(rr[rr.Count / 2]);
                        else
                            a1_ratios.Add((rr[rr.Count / 2] + rr[rr.Count / 2 - 1]) / 2);
                    }
                }
                if (ratios.Count > 0)
                {
                    ratios.Sort();
                    if (ratios.Count % 2 != 0)
                        proteins[i].MS2_Ratio = Math.Pow(2.0, ratios[ratios.Count / 2]);
                    else
                        proteins[i].MS2_Ratio = Math.Pow(2.0, (ratios[ratios.Count / 2] + ratios[ratios.Count / 2 - 1]) / 2);
                }
                if (a1_ratios.Count > 0)
                {
                    a1_ratios.Sort();
                    if (a1_ratios.Count % 2 != 0)
                        proteins[i].MS2_Ratio_a1 = Math.Pow(2.0, a1_ratios[a1_ratios.Count / 2]);
                    else
                        proteins[i].MS2_Ratio_a1 = Math.Pow(2.0, (a1_ratios[a1_ratios.Count / 2] + a1_ratios[a1_ratios.Count / 2 - 1]) / 2);
                }
            }
        }
        public Border previous_floor(StackPanel sp, int floor_index, int sp_index) //根据该Border找到上一层与之连接的Border
        {
            if (sp_index == 1) //往上找序列与之拼接上
            {
                int r_num = 0, r_index = floor_index - 1;
                while (r_index >= 0)
                {
                    StackPanel sp0 = grid.Children[r_index] as StackPanel;
                    if (is_Protein_Row(sp0))
                        break;
                    ++r_num;
                    --r_index;
                }
                --r_index;
                while (r_index >= 0)
                {
                    StackPanel sp0 = grid.Children[r_index] as StackPanel;
                    if (is_Protein_Row(sp0))
                        break;
                    --r_index;
                }
                r_index += r_num + 1;
                if (r_index >= 0)
                {
                    StackPanel sp_previous = grid.Children[r_index] as StackPanel;
                    if (sp_previous.Children.Count > 2 && sp_previous.Children[sp_previous.Children.Count - 1] is Border)
                    {
                        return (Border)sp_previous.Children[sp_previous.Children.Count - 1];
                    }
                }
            }
            return null;
        }
        public bool is_Protein_Row(StackPanel sp0) //是一行蛋白质序列吗？
        {
            if (sp0.Children.Count == 3 && sp0.Children[0] is TextBlock && ((TextBlock)sp0.Children[0]).Text.Length > 0 &&
                ((TextBlock)sp0.Children[0]).Text[0] == '[')
                return true;
            return false;
        }
        public Border next_floor(StackPanel sp, int floor_index, int sp_index) //根据该Border找到下一层与之连接的Border
        {
            if (sp_index == sp.Children.Count - 1) //往下找序列与之拼接上
            {
                int r_num = 0, r_index = floor_index - 1;
                while (r_index >= 0)
                {
                    StackPanel sp0 = grid.Children[r_index] as StackPanel;
                    if (is_Protein_Row(sp0))
                        break;
                    ++r_num;
                    --r_index;
                }
                r_index = floor_index + 1;
                while (r_index < grid.Children.Count)
                {
                    StackPanel sp0 = grid.Children[r_index] as StackPanel;
                    if (is_Protein_Row(sp0))
                        break;
                    ++r_index;
                }
                r_index += r_num + 1;
                if (r_index < grid.Children.Count)
                {
                    StackPanel sp_next = grid.Children[r_index] as StackPanel;
                    if (sp_next.Children.Count > 2 && sp_next.Children[1] is Border)
                    {
                        return (Border)sp_next.Children[1];
                    }
                }
            }
            return null;
        }
        public string getStringByBorder(Border bd)
        {
            string sq = "";
            IEnumerator<Inline> lines = ((TextBlock)bd.Child).Inlines.GetEnumerator();
            while (lines.MoveNext())
            {
                Inline line = lines.Current;
                Run run = line as Run;
                sq += run.Text;
            }
            return sq;
        }
    }

    public class Protein_Fragment
    {
        public int Start;
        public int End;
        public List<Interval> intervals;

        public int Max_layer;

        public Protein_Fragment(int start, int end)
        {
            this.Start = start;
            this.End = end;
            this.intervals = new List<Interval>();
        }
    }
}
