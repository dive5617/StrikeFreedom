using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class Task
    {
        public static string pFind_license = "pFind.license";
        public bool has_ratio = false;
        public bool is_notMGF = true;

        public string folder_path = ""; //当前Task任务的路径
        public string folder_param_path = ""; //当前Task任务下的param的路径
        public string folder_result_path = ""; //当前Task任务下的result的路径。下面都是该路径下的文件名
        public string identification_file = ""; //pFind.spectra定性文件，result文件夹下
        public string identification_protein_file = ""; //pFind.protein定性文件，result文件夹下
        public string quantification_file = ""; //pQuant.spectra定量文件，result文件夹下
        public string quantification_list_file = ""; //pQuant.spectra.list定量文件，可以提取对应的ratio和sigma，result文件夹下
        public string quantification_protein_file = ""; //pQuant.protein定量文件，result文件下
        public string summary_file = ""; //.summary文件,定性得到的统计信息文件，result文件夹下
        public string pFind_param_file = ""; //.pfd文件，pFind内核使用的参数文件，param文件夹下
        public string pParse_param_file = ""; //.para文件，pParse使用的参数文件,param文件夹下
        public string pQuant_param_file = ""; //.qnt文件，pQuant使用的参数文件，param文件夹下
        public List<string> all_aa_files; //.aa文件，pFind内核输出的所有氨基酸质量文件，result文件夹下
        public List<string> all_mod_files; //.mod文件，pFind内核输出的所有修饰质量文件，result文件夹下
        public string fasta_file_path = ""; //在.pfd文件中，有一项是搜索的数据库的路径
        public List<string> all_res_files; //.qry.res文件，所有的候选肽段信息，在result文件夹下，最终存入了一个F1.qry.res文件里面
        public string HCD_ETD_Type = ""; //读取参数文件，来获取是HCD还是ETD的碎裂类型

        public static string pfind_ini_file = "pTop.ini"; // "pFind.ini"; //关于线程数及任务的默认路径
        public static string raw_ini_file = "raws.rawini"; //raw路径文件。需要修改，对参数文件的支持，默认与exe同路径，文件名为raws.rawini
        public static string recent_task_path_file = "recentTaskPath.tpini"; //记录最近打开的一些Task的路径，默认与exe同路径。
        public static int recent_task_path_num = 10;
        public static string model_path = "model.txt";

        public List<string> mgf_file_paths = new List<string>();

        //public string raw_file_path = ""; //raw文件的路径，用来获取pf索引文件，默认索引文件路径与raw文件路径保持一致
        public System.Collections.Hashtable title_PF2_Path = new System.Collections.Hashtable();
        public System.Collections.Hashtable title_PF1_Path = new System.Collections.Hashtable();

        public static string modify_ini_file = "modification.ini"; //.ini修饰文件，可以与exe同路径
        public static string modify_pLink_ini_file = "modify.ini";
        public static string pLink_ini_file = "xlink.ini";
        public static string pLink_xxINI_file = "xx.ini"; //配置pLink中交联剂的元素组成
        public static string element_ini_file = "element.ini"; //.ini元素文件，可以与exe同路径
        public static string aa_ini_file = "aa.ini"; //.ini氨基酸文件，可以与exe同路径
        public static string lipv_txt_file = "LIPV.txt"; //.txt同位素峰簇的理论相对强度，可以与exe同路径

        public static string task_path_process = "task.proini";

        public static string quant_index_start = "quant_start.dat";
        public static string quant_index_end = "quant_end.dat";
        public static string quant_index_hash = "quant_hash.dat";
        public static string ratio_sigma_index1 = "psm_ratio_sigma1.dat";
        public static string ratio_sigma_index2 = "psm_ratio_sigma2.dat";
        public static string hash_next = "psm_hash_next.dat";
        public static string hash_first = "psm_hash_first.dat";
        public static string hash_next_onlySq = "psm_hash_next_onlySq.dat";
        public static string hash_first_onlySq = "psm_hash_first_onlySq.dat";
        public static string title_index_hash = "psm_hash_title_index.dat";
        public static string sq_index_hash = "psm_hash_sq_index.dat";
        public static string cand_peptides = "psm_cand_pep.dat";


        #region
        //pNovo的路径,其中上面的三个folder_path,folder_param_path和folder_result_path是一样的
        public string pNovo_result_path = "";
        public string pNovo_param_path = "";

        public string pLink_result_path = "";
        public string pLink_param_path = "";
        #endregion


        public Task()
        {
            has_ratio = false;
            modify_ini_file = "modification.ini";
            element_ini_file = "element.ini";
            aa_ini_file = "aa.ini";
            lipv_txt_file = "LIPV.txt";
            all_aa_files = new List<string>();
            all_mod_files = new List<string>();
            all_res_files = new List<string>();
            HCD_ETD_Type = "HCD";
        }

        public Task(string file_path, string file_type)
        {
            if (file_type == "pTop")
            {
                has_ratio = false;
                modify_ini_file = "modification.ini";
                element_ini_file = "element.ini";
                aa_ini_file = "aa.ini";
                lipv_txt_file = "LIPV.txt";
                all_aa_files = new List<string>();
                all_mod_files = new List<string>();
                all_res_files = new List<string>();
                this.folder_path = file_path;
                this.folder_result_path = file_path;
                this.folder_param_path = file_path + "\\param\\";
                get_File_byname(this.folder_result_path, Task_Help.pFind_spectra_name, ref this.identification_file); //获取定性结果文件名
                get_File_byname(this.folder_result_path, Task_Help.pQuant_spectra_name, ref this.quantification_file); //获取定量结果文件名
                get_File_byname(this.folder_result_path, Task_Help.pQuant_spectra_list_name, ref this.quantification_list_file);
                get_File_byname(this.folder_result_path, Task_Help.pFind_protein_name, ref this.identification_protein_file);
                get_File_byname(this.folder_result_path, Task_Help.pQuant_protein_name, ref this.quantification_protein_file);
                get_File_byname(this.folder_result_path, Task_Help.pFind_summary_name, ref this.summary_file);
                get_File_byname(this.folder_param_path, Task_Help.pFind_param_name, ref this.pFind_param_file);
                get_File_byname(this.folder_param_path, Task_Help.pParse_param_name, ref this.pParse_param_file);
                get_File_byname(this.folder_param_path, Task_Help.pQuant_param_name, ref this.pQuant_param_file);
                get_Files(this.folder_result_path, "aa", ref this.all_aa_files);
                get_Files(this.folder_result_path, "mod", ref this.all_mod_files);
                get_Files2(this.folder_result_path, "qry.res", ref this.all_res_files);
                if (this.all_aa_files.Count > 1) //如果有多张氨基酸表，那么说明有轻重对儿，说明会有比值。
                    has_ratio = true;
                write_Raw_ini();
                update_fasta_path();
                update_activation_type();
            }
            else if (file_type == "pNovo")
            {
                this.folder_path = file_path;
                this.folder_result_path = file_path + "\\result\\";
                this.folder_param_path = file_path + "\\param\\";
                get_File_byname(this.folder_param_path, Task_Help.pNovo_param_name, ref this.pNovo_param_path);
                get_File_byname(this.folder_result_path, Task_Help.pNovo_result_name, ref this.pNovo_result_path);
                write_MGF_ini_pNovo();
                //update_activation_type();
                this.HCD_ETD_Type = "HCD";
            }
            else if (file_type == "pLink")
            {
                this.folder_path = file_path;
                this.folder_result_path = file_path + "\\result\\";
                this.folder_param_path = file_path + "\\param\\";
                get_File_byname(this.folder_param_path, Task_Help.pLink_param_name, ref this.pLink_param_path);
                get_File_byname(this.folder_result_path, Task_Help.pLink_spectra_name, ref this.pLink_result_path);
                get_File_byname(this.folder_param_path, Task_Help.pQuant_param_name, ref this.pQuant_param_file);
                get_File_byname(this.folder_result_path, Task_Help.pQuant_spectra_name, ref this.quantification_file);
                get_File_byname(this.folder_result_path, Task_Help.pQuant_spectra_list_name, ref this.quantification_list_file);
                if (this.quantification_file != "")
                    this.has_ratio = true;
                get_Files(this.folder_result_path, "aa", ref this.all_aa_files);
                get_Files(this.folder_result_path, "mod", ref this.all_mod_files);
            }
            //get_File("rawini", ref this.raw_ini_file); //这个将在后面进行改动
            //最后获取多个raw的文件名
        }
        // modified by wrm 2016.03.04
        private void update_activation_type()
        {
            this.HCD_ETD_Type = "HCD";
            StreamReader sr = new StreamReader(this.pFind_param_file, Encoding.Default);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine().Trim();
                if (line == "")
                    continue;
                if (line.StartsWith("Activation"))   //[wrm?]不支持CID吗
                {
                    string[] strs = line.Split('=');
                    if (strs[1].StartsWith("HCD"))
                        this.HCD_ETD_Type = "HCD";
                    else if (strs[1].StartsWith("ETD"))
                        this.HCD_ETD_Type = "ETD";
                }
            }
            sr.Close();
        }
        // modified by wrm 2016.03.04
        private void update_fasta_path()
        {
            //FileStream fs = new FileStream(this.pFind_param_file, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(this.pFind_param_file, Encoding.Default);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Length >= 9 && line.Substring(0, 8) == "Database")
                {
                    this.fasta_file_path = line.Substring(9);
                }
            }
            sr.Close();
        }
        private void mgf_hash(string mgf_path)
        {
            string title_name = "";
            int last_index = mgf_path.LastIndexOf('\\');
            string mgf_folder = mgf_path.Substring(0, last_index);
            string mgf_name = mgf_path.Substring(last_index + 1);
            int last_index0 = mgf_name.LastIndexOf('.');
            mgf_name = mgf_name.Substring(0, last_index0);
            string mgf_path2 = mgf_folder + "\\" + mgf_name;
            StreamReader sr_tmp = new StreamReader(mgf_path, Encoding.Default);
            while (!sr_tmp.EndOfStream)
            {
                string line_tmp = sr_tmp.ReadLine();
                if (line_tmp.Trim() != "" && line_tmp.StartsWith("TITLE"))
                {
                    string title_all = line_tmp.Split('=')[1];
                    title_name = title_all.Split('.')[0];
                    title_PF2_Path[title_name] = mgf_path2;
                    sr_tmp.Close();
                    break;
                }
            }
        }
        
        private void write_MGF_ini_pNovo()
        {
            StreamReader sr = new StreamReader(this.folder_param_path + Task_Help.pNovo_param_name, Encoding.Default);
            string spec1_path = "", spec_type = "mgf";
            bool is_folder = false;
            List<string> mgf_paths = new List<string>();
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                line = line.Trim();
                if (line == "")
                    continue;
                string[] strs = line.Split('=');
                if (strs.Length >= 2 && strs[0] == "spec_path1")
                {
                    spec1_path = strs[1];
                }
                else if (strs.Length >= 2 && strs[0] == "folder")
                {
                    if (strs[1] == "yes")
                        is_folder = true;
                }
                else if (strs.Length >= 2 && strs[0] == "spec_type")
                {
                    spec_type = strs[1].ToLower();
                }
            }
            sr.Close();
            if (is_folder)
            {
                if (!spec1_path.EndsWith("\\"))
                    spec1_path += "\\";
                get_Files(spec1_path, spec_type, ref mgf_paths);
            }
            else
            {
                mgf_paths.Add(spec1_path);
            }
            this.mgf_file_paths = new List<string>(mgf_paths);
            //for (int i = 0; i < mgf_paths.Count; ++i)
            //    mgf_hash(mgf_paths[i]);
        }
        public string get_pf2Index_path(string raw_name)
        {
            if (raw_name == "")
                return "";
            string pf2_path = get_pf2_path(raw_name);
            if (pf2_path == "")
                return "";
            string pf2_index_path = pf2_path.Substring(0, pf2_path.LastIndexOf('.')) + ".pf2idx";
            return pf2_index_path;
        }
        public string get_pf2_path(string raw_name)
        {
            if (raw_name == "")
                return "";
            if (this.title_PF2_Path[raw_name] == null)
            {
                foreach(string key in this.title_PF2_Path.Keys)
                    return this.title_PF2_Path[key] as string;
            }
            return this.title_PF2_Path[raw_name] as string;
        }
        public string get_pf1Index_path(string raw_name)
        {
            if (raw_name == "")
                return "";
            string pf1_path = get_pf1_path(raw_name);
            if (pf1_path == "")
                return "";
            string pf1_index_path = pf1_path.Substring(0, pf1_path.LastIndexOf('.')) + ".pf1idx";
            return pf1_index_path;
        }
        public string get_pf1_path(string raw_name)
        {
            if (raw_name == "")
                return "";
            if (this.title_PF1_Path[raw_name] != null)
                return this.title_PF1_Path[raw_name] as string;
            string pf2_path = this.title_PF2_Path[raw_name] as string;
            string pf2_path_noPF = pf2_path.Substring(0, pf2_path.LastIndexOf('.'));
            if (is_notMGF) //raw
                pf2_path_noPF = pf2_path_noPF.Substring(0, pf2_path_noPF.Length - 6);
            string folder_path = pf2_path.Substring(0, pf2_path.LastIndexOf('\\')) + "\\";
            List<string> pf1_paths = new List<string>();
            get_Files(folder_path, "pf1", ref pf1_paths);
            for (int i = 0; i < pf1_paths.Count; ++i)
            {
                string noPF = pf1_paths[i].Substring(0, pf1_paths[i].LastIndexOf('.'));
                if (pf2_path_noPF == noPF)
                {
                    this.title_PF1_Path[raw_name] = pf1_paths[i];
                    return pf1_paths[i];
                }
            }
            return "";
        }
        // title of input data
        // modified by wrm 2016.03.04
        private void write_Raw_ini()
        {
            //FileStream fs_open = new FileStream(this.folder_param_path + "pFind.pfd", FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(this.folder_param_path + Task_Help.pFind_param_name, Encoding.Default);
            string raw_file = raw_ini_file; //自定义的rawini与exe本路径
            FileStream fs = new FileStream(raw_file, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs, Encoding.Default);
            List<string> files = new List<string>();
            string HCD_FT_str = "";
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Trim() == "")
                    continue;
                string[] strs = line.Split('=');
                if (strs[0] == "msmsnum")
                {
                    int raw_num = int.Parse(strs[1]);
                    for (int i = 0; i < raw_num; ++i)
                    {
                        line = sr.ReadLine();
                        string path = line.Split('=')[1];
                        string[] names = path.Split('\\');
                        string path0 = "";
                        for (int j = 0; j < names.Length - 1; ++j)
                        {
                            path0 += names[j] + "\\";
                        }
                        // modified by wrm 2016.03.04
                        int index=names[names.Length - 1].LastIndexOf('.');
                        string last_name = names[names.Length - 1].Substring(0,index);
                        //index = last_name.LastIndexOf('_');
                        
                        //if (index >= 0 && !path.EndsWith("mgf"))
                        //{
                        //    HCD_FT_str = last_name.Substring(index + 1);
                        //    last_name = last_name.Substring(0, index);
                        //}
                        if (path.EndsWith(".mgf") || path.EndsWith(".MGF"))
                        {
                            //如果是mgf，读取mgf文件然后将last_name从Mgf的名字改成mgf里面的title的第一项
                            last_name = getMGF_title(path);
                            string ss = path.Substring(0, path.LastIndexOf('.'));
                            path = ss + ".pf2"; 
                        }
                        title_PF2_Path[last_name] = path0;
                        files.Add(last_name);                      
                    }
                    
                }
                else if (strs[0] == "Activation")
                {
                    strs[1] = strs[1].TrimEnd(new char[]{'\r','\n'});
                    HCD_FT_str = strs[1].ToUpper() + "FT";
                    break;
                }
            }
            sw.WriteLine(files.Count + " " + HCD_FT_str);
            for (int i = 0; i < files.Count; ++i) 
            {
                sw.WriteLine(files[i]);
                title_PF2_Path[files[i]] += (files[i] + "_" + HCD_FT_str + ".pf2");
            }
            sw.Flush();
            sr.Close();
            fs.Close();
        }
        private string getMGF_title(string mgf_path)
        {
            StreamReader sr = new StreamReader(mgf_path);
            string res = "";
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine().Trim();
                if (line.ToUpper().StartsWith("TITLE"))
                {
                    string[] strs = line.Split(new char[] { '=', '.' }, StringSplitOptions.RemoveEmptyEntries);
                    res = strs[1];
                    break;
                }
            }
            sr.Close();
            return res;
        }
        private void get_File(string path, string suffix_name, ref string file_path) //根据后缀名来提取对应的文件名
        {
            if (!Directory.Exists(path))
                return;
            string[] files = Directory.GetFiles(path, "*");
            for (int i = 0; i < files.Length; ++i)
            {
                string[] paths = files[i].Split('\\');
                string[] names = paths.Last().Split('.');
                string cur_suffix_name = names.Last();
                if (cur_suffix_name == suffix_name)
                {
                    file_path = paths[paths.Length - 1];
                    file_path = path + file_path;
                    return ;
                }
            }
        }
        private void get_File_byname(string path, string name, ref string file_path)
        {
            if (!Directory.Exists(path))
                return;
            if (path[path.Length - 1] != '\\') 
            {
                path = path + "\\";
            }
            string[] files = Directory.GetFiles(path, "*");
            for (int i = 0; i < files.Length; ++i)
            {
                string[] paths = files[i].Split('\\');
                if (paths.Last() == name)
                {
                    file_path = paths.Last();
                    file_path = path + file_path;
                    return;
                }
            }
            file_path = "";
        }
        private void get_Files(string path, string suffix_name, ref List<string> files_name) //根据后缀名来提取对应的文件名
        {
            if (!Directory.Exists(path))
                return;
            if (path[path.Length - 1] != '\\')
            {
                path = path + "\\";
            }
            files_name = new List<string>();
            string[] files = Directory.GetFiles(path, "*");
            for (int i = 0; i < files.Length; ++i)
            {
                string[] paths = files[i].Split('\\');
                string[] names = paths[paths.Length - 1].Split('.');
                string cur_suffix_name = names[names.Length - 1];
                if (cur_suffix_name == suffix_name)
                {
                    string file_name = paths[paths.Length - 1];
                    file_name = path + file_name;
                    files_name.Add(file_name);
                }
            }
        }
        private void get_Files2(string path, string suffix_name, ref List<string> files_name) //根据后缀名来提取对应的文件名
        {
            if (!Directory.Exists(path))
                return;
            files_name = new List<string>();
            string[] files = Directory.GetFiles(path, "*");
            for (int i = 0; i < files.Length; ++i)
            {
                if (files[i].EndsWith(suffix_name))
                {
                    string file_name = files[i];
                    //string[] strs = file_name.Split('\\');
                    //string file_name0 = strs.Last();
                    //string[] strs0 = file_name0.Split('.');
                    //if (strs0[strs0.Length - 3][0] != 'L')
                        files_name.Add(file_name);
                }
            }
        }
        //判断该路径下的result的spectra，quant，summary及aa文件是否存在，以及param的pfd和para文件是否存在
        public bool check_task_path(string path, string file_type = "pTop")
        {
            List<string> tsk_paths = new List<string>();
            get_Files(path, "tsk", ref tsk_paths);
            if (tsk_paths.Count == 0) //不存在tsk文件
                return false;
            if (file_type == "pTop") //如果打开的是pTop任务
                return check_pTop_task_path(path);
            else if (file_type == "pNovo") //如果打开的是pNovo任务
                return check_pNovo_task_path(path);
            else if (file_type == "pLink")
                return check_pLink_task_path(path);
            return false;
        }
        private bool check_pLink_task_path(string path)
        {
            string result_path = path + "\\result";
            string param_path = path + "\\param";
            if (!Directory.Exists(result_path))
                return false;
            if (!Directory.Exists(param_path))
                return false;
            return true;
        }
        private bool check_pNovo_task_path(string path)
        {
            string result_path = path + "\\result";
            string param_path = path + "\\param";
            if (!Directory.Exists(result_path))
                return false;
            if (!Directory.Exists(param_path))
                return false;
            return true;
        }
        private bool check_pTop_task_path(string path)
        {
            string result_path = path;
            if (!result_path.EndsWith("\\")) result_path += "\\";
            string param_path = path + "\\param";
            if (!Directory.Exists(param_path)) return false;
            string spectra_file = "";
            string quant_file = ""; //可能没有跑定量，可以没有.quant文件
            //string aa_file = "";
            get_File_byname(result_path, Task_Help.pFind_spectra_name, ref spectra_file);
            get_File_byname(result_path, Task_Help.pQuant_spectra_name, ref quant_file);
            //get_File(result_path, "aa", ref aa_file);
            if (spectra_file == "")
                return false;
            string pfd_file = "";
            string para_file = "";
            get_File_byname(param_path, Task_Help.pFind_param_name, ref pfd_file);
            get_File_byname(param_path, Task_Help.pParse_param_name, ref para_file);

            if (pfd_file == "") // || para_file == ""
                return false;

            return true;
        }
        private bool check_pFind_task_path(string path)
        {
            string result_path = path + "\\result";
            string param_path = path + "\\param";
            if (!Directory.Exists(result_path))
                return false;
            if (!Directory.Exists(param_path))
                return false;
            string spectra_file = "";
            string quant_file = ""; //可能没有跑定量，可以没有.quant文件
            string summary_file = "";
            string aa_file = "";
            get_File_byname(result_path, Task_Help.pFind_spectra_name, ref spectra_file);
            get_File_byname(result_path, Task_Help.pQuant_spectra_name, ref quant_file);
            get_File_byname(result_path, Task_Help.pFind_summary_name, ref summary_file);
            get_File(result_path, "aa", ref aa_file);
            if (spectra_file == "" || summary_file == "" || aa_file == "")
                return false;
            string pfd_file = "";
            string para_file = "";
            get_File_byname(param_path, Task_Help.pFind_param_name, ref pfd_file);
            get_File_byname(param_path, Task_Help.pParse_param_name, ref para_file);
            
            if (pfd_file == "" || para_file == "")
                return false;
            
            return true;
        }
        // modified by wrm 2016.03.04
        public List<string> get_raws_path()
        {
            List<string> raws_path = new List<string>();
            StreamReader sr = new StreamReader(this.pFind_param_file, Encoding.Default);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.StartsWith("msmspath"))
                {
                    string raw_path = line.Split('=')[1];
                    //int index = pf_path.LastIndexOf('_'); //将_HCDFT.pf去掉
                    //string raw_path = pf_path.Substring(0, index) + ".raw";

                    if (!raw_path.EndsWith("raw") && !raw_path.EndsWith("RAW"))
                    {
                        int index = raw_path.LastIndexOf('_');
                        if (index > 0)
                        {
                            raw_path = raw_path.Substring(0, index) + ".raw";
                        }
                        else 
                        {
                            continue;    
                        }
                    }
                    raws_path.Add(raw_path);
                }
            }
            return raws_path;
        }
    }
    public class TaskInfo : IComparable
    {
        public string tname { get; set; }
        public string tpath { get; set; }
        public string ttimespan { get; set; }
        public TaskInfo(string tname, string tpath)
        {
            this.tname = tname;
            this.tpath = tpath;
        }
        public static string ShowTime(DateTime date)
        {
            return "(" + showtime(date) + ")";
        }
        public static string showtime(DateTime date)
        {
            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;
            TimeSpan ts = DateTime.Now - date;
            double delta = ts.TotalSeconds;
            if (delta < 0)
            {
                return "not yet";
            }
            if (delta < 1 * MINUTE)
            {
                return ts.Seconds == 1 ? "1 second ago" : ts.Seconds + " seconds ago";
            }
            if (delta < 2 * MINUTE)
            {
                return "1 minute ago";
            }
            if (delta < 45 * MINUTE)
            {
                return ts.Minutes + " minutes";
            }
            if (delta < 90 * MINUTE)
            {
                return "1 hour ago";
            }
            if (delta < 24 * HOUR)
            {
                return ts.Hours + " hours ago";
            }
            if (delta < 48 * HOUR)
            {
                return "yesterday";
            }
            if (delta < 30 * DAY)
            {
                return ts.Days + " days ago";
            }
            if (delta < 12 * MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "1 month ago" : months + " months ago";
            }
            else
            {
                int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "one year ago" : years + " years ago";
            }
        }
        int IComparable.CompareTo(Object obj)
        {
            TaskInfo temp = (TaskInfo)obj;
            return this.tpath.CompareTo(temp.tpath);
        }
        public override bool Equals(object obj)
        {
            var taskinfo = obj as TaskInfo;
            if (taskinfo == null)
                return false;
            if (this.tpath == taskinfo.tpath)
                return true;
            return false;
        }
        public static bool operator ==(TaskInfo a, TaskInfo b)
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
            return a.tpath == b.tpath;
        }
        public static bool operator !=(TaskInfo a, TaskInfo b)
        {
            return !(a == b);
        }
        public override int GetHashCode()
        {
            return this.tpath.GetHashCode();
        }
    }

    public class Task_Help
    {
        public static string pFind_spectra_name = "pTop.spectra";
        public static string pFind_protein_name = "pTop.protein";
        public static string pQuant_spectra_name = "pQuant.spectra";
        public static string pQuant_spectra_list_name = "pQuant.spectra.list";
        public static string pQuant_protein_name = "pQuant.protein";
        public static string pFind_summary_name = "pTop.summary";

        public static string pFind_param_name = "pTop.cfg";
        public static string pParse_param_name = "pParseTD.cfg";
        public static string pQuant_param_name = "pQuant.cfg";

        public static string pNovo_param_name = "pNovo.param";
        public static string pNovo_result_name = "pNovo.res";

        public static string pLink_param_name = "pLink.cfg";
        public static string pLink_spectra_name = "pLink.spectra";
    }
}
