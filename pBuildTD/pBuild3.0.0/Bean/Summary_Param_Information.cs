using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class Summary_Param_Information
    {
        //param
        //public int thread_number { get; set; }
        public string ms_tolerance { get; set; } //母离子误差+Da\ppm
        public string msms_tolerance { get; set; } //碎裂离子误差+Da\ppm
        public bool open_search { get; set; } //是否pFind内核开启Open Search功能
        public string input_format { get; set; } //搜索的是MGF还是RAW还是MS1
        //public int peptide_number { get; set; } //每个PSM考虑的候选肽数目
        public string fix_modification { get; set; } //固定修饰的数目
        public int max_var_mod_num { get; set; }
        public string variable_modification { get; set; } //可变修饰的数目
        public string enzyme { get; set; } //酶切方式
        //public int max_missing_cleavage_number { get; set; } //最大遗漏酶切位点数
        public bool co_elute { get; set; } //pParse是否开启了混合谱图功能
        public string ll_info_label { get; set; } //pQuant 的标记信息
        public string chrom_tolerance { get; set; } //pQuant的色谱误差窗口
        public string label_efficiency { get; set; } //pQuant的标记效率

        //file
        public List<string> aas_path = new List<string>(); //所有氨基酸质量表的路径
        public string modification_path { get; set; } //修饰文件的路径
        public string fasta_path { get; set; } //数据库路径
        public string contaminant_path { get; set; } //污染数据库的路径
        public string task_path { get; set; } //输出任务的路径
        public List<string> raws_path = new List<string>(); //搜索的各个raws的路径

    }
}
