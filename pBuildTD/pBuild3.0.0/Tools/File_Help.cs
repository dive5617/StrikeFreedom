using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace pBuild
{
    class File_Help
    {
        public static Boolean Cand_file_Exist = false;
        public static string pBuild_dat_file = "pBuild_dat";
        public static string pBuild_tmp_file = "pBuild_tmp";
        public static string pNovo_exe_file = "pNovo";
        public static Spectra readOneMGF(string filepath)
        {
            Spectra spec = new Spectra();
            try
            {
                ObservableCollection<PEAK> peaks = new ObservableCollection<PEAK>();
                StreamReader sr = new StreamReader(filepath, Encoding.Default);
                string strLine = sr.ReadLine();
                double maxIntensity = 0.0;
                while (strLine != "" && strLine != null)
                {
                    if (strLine[0] < 'A' || strLine[0] > 'Z')
                    {
                        string[] arr = strLine.Split(' ');
                        peaks.Add(new PEAK(double.Parse(arr[0]), double.Parse(arr[1])));
                        if (double.Parse(arr[1]) > maxIntensity)
                            maxIntensity = double.Parse(arr[1]);
                    }
                    else if (strLine[0] == 'T')
                    {
                        string[] arr = strLine.Split('=');
                        spec.Title = arr[1];
                    }
                    else if (strLine[0] == 'C')
                    {
                        string[] arr = strLine.Split('=');
                        string[] arr2 = arr[1].Split('+');
                        spec.Charge = int.Parse(arr2[0]);
                    }
                    else if (strLine[0] == 'P')
                    {
                        string[] arr = strLine.Split('=');
                        spec.Pepmass = double.Parse(arr[1]);
                    }
                    strLine = sr.ReadLine();
                }
                sr.Close();
                for (int k = 0; k < peaks.Count; ++k)
                {
                    peaks[k].Intensity = peaks[k].Intensity * 100 / maxIntensity;
                }
                spec.Peaks = peaks;
            }
            catch (Exception exe)
            {
                MessageBox.Show(exe.ToString());
            }
            return spec;
        }
        public static void readMGF(string filepath, ref ObservableCollection<Spectra> spec)
        {
            ObservableCollection<Spectra> s = readMGF(filepath);
            for (int i = 0; i < s.Count; ++i)
                spec.Add(s[i]);
        }
        public static ObservableCollection<Spectra> readMGF(string filepath)
        {
            ObservableCollection<Spectra> specs = new ObservableCollection<Spectra>();
            Spectra spec = new Spectra();
            string line = "";
            try
            {
                ObservableCollection<PEAK> peaks = new ObservableCollection<PEAK>();
                StreamReader sr = new StreamReader(filepath, Encoding.Default);
                string strLine = sr.ReadLine();
                line = strLine;
                double maxIntensity = 0.0;
                while (strLine != null)
                {
                    if (strLine == "" || strLine[0] == '#')
                    {
                        strLine = sr.ReadLine();
                        continue;
                    }
                    if (strLine[0] < 'A' || strLine[0] > 'Z')
                    {
                        string[] arr = strLine.Split(new char[]{' ', '\t'});
                        peaks.Add(new PEAK(double.Parse(arr[0]), double.Parse(arr[1])));
                        if (double.Parse(arr[1]) > maxIntensity)
                            maxIntensity = double.Parse(arr[1]);
                    }
                    else if (strLine[0] == 'T')
                    {
                        string[] arr = strLine.Split('=');
                        spec.Title = arr[1];
                    }
                    else if (strLine[0] == 'C')
                    {
                        string[] arr = strLine.Split('=');
                        string[] arr2 = arr[1].Split('+');
                        spec.Charge = int.Parse(arr2[0]);
                    }
                    else if (strLine[0] == 'P')
                    {
                        string[] arr = strLine.Split('=');
                        if(arr[1].Contains('\t'))
                            spec.Pepmass = double.Parse(arr[1].Split('\t')[0]);
                        else
                        spec.Pepmass = double.Parse(arr[1]);
                    }
                    else if (strLine[0] == 'E')
                    {
                        for (int k = 0; k < peaks.Count; ++k)
                        {
                            peaks[k].Intensity = peaks[k].Intensity * 100 / maxIntensity;
                        }
                        spec.Peaks = peaks;
                        spec.Max_inten_E = maxIntensity;
                        specs.Add(spec);
                        peaks = new ObservableCollection<PEAK>();
                    }
                    else if (strLine[0] == 'B')
                    {
                        maxIntensity = 0.0;
                        spec = new Spectra();
                    }
                    strLine = sr.ReadLine();
                }
                sr.Close();
            }
            catch (Exception exe)
            {
                MessageBox.Show(exe.ToString() + "\r\n" + line);
            }
            return specs;
        }
        private static void read_hash(string filepath, ref ObservableCollection<int> hash)
        {
            FileStream filestream = new FileStream(filepath, FileMode.Open);
            BinaryReader objBinaryReader = new BinaryReader(filestream);
            try
            {
                while (true)
                {
                    int number = objBinaryReader.ReadInt32();
                    hash.Add(number);
                }
            }
            catch (Exception exe) { }
            objBinaryReader.Close();
            filestream.Close();
        }
        private static void write_hash(string filepath, ObservableCollection<int> hash)
        {
            FileStream filestream = new FileStream(filepath, FileMode.Create);
            BinaryWriter objBinaryWriter = new BinaryWriter(filestream);
            try
            {
                for (int i = 0; i < hash.Count; ++i)
                    objBinaryWriter.Write(hash[i]);
            }
            catch (Exception exe) { }
            objBinaryWriter.Flush();
            objBinaryWriter.Close();
            filestream.Close();
        }
        private static void read_hash2(string filepath, ref System.Collections.Hashtable hash)
        {
            FileStream filestream = new FileStream(filepath, FileMode.Open);
            BinaryReader objBinaryReader = new BinaryReader(filestream);
            try
            {
                while (true)
                {
                    string str = objBinaryReader.ReadString();
                    List<int> tmp_int = new List<int>();
                    int count = objBinaryReader.ReadInt32();
                    for (int i = 0; i < count; ++i)
                    {
                        int tmp = objBinaryReader.ReadInt32();
                        tmp_int.Add(tmp);
                    }
                    hash[str] = tmp_int;
                }
            }
            catch (Exception exe) { }
            objBinaryReader.Close();
            filestream.Close();
        }
        private static void write_hash2(string filepath, System.Collections.Hashtable hash)
        {
            FileStream filestream = new FileStream(filepath, FileMode.Create);
            BinaryWriter objBinaryWriter = new BinaryWriter(filestream);
            try
            {
                foreach (string key in hash.Keys)
                {
                    objBinaryWriter.Write(key);
                    List<int> tmp_int = hash[key] as List<int>;
                    objBinaryWriter.Write(tmp_int.Count);
                    for (int i = 0; i < tmp_int.Count; ++i)
                        objBinaryWriter.Write(tmp_int[i]);
                }
            }
            catch (Exception exe) { }
            objBinaryWriter.Flush();
            objBinaryWriter.Close();
            filestream.Close();
        }
        private static void read_filter_hash(string hn, string hf, string hno, string hfo, string tih, string sih
            , ref ObservableCollection<int> hash_next, ref ObservableCollection<int> hash_first
            , ref ObservableCollection<int> hash_next_OnlySQ, ref ObservableCollection<int> hash_first_OnlySQ
            , ref System.Collections.Hashtable title_index_hash, ref System.Collections.Hashtable sq_index_hash)
        {
            read_hash(hn, ref hash_next);
            read_hash(hf, ref hash_first);
            read_hash(hno, ref hash_next_OnlySQ);
            read_hash(hfo, ref hash_first_OnlySQ);
            read_hash2(tih, ref title_index_hash);
            read_hash2(sih, ref sq_index_hash);
        }
        private static void write_filter_hash(string hn, string hf, string hno, string hfo, string tih, string sih
            , ObservableCollection<int> hash_next, ObservableCollection<int> hash_first
            , ObservableCollection<int> hash_next_OnlySQ, ObservableCollection<int> hash_first_OnlySQ
            , System.Collections.Hashtable title_index_hash, System.Collections.Hashtable sq_index_hash)
        {
            write_hash(hn, hash_next);
            write_hash(hf, hash_first);
            write_hash(hno, hash_next_OnlySQ);
            write_hash(hfo, hash_first_OnlySQ);
            write_hash2(tih, title_index_hash);
            write_hash2(sih, sq_index_hash);
        }
        public static void updatePSM(string path, ObservableCollection<PSM> psms, ref ObservableCollection<int> hash_next, ref ObservableCollection<int> hash_first
            , ref ObservableCollection<int> hash_next_OnlySQ, ref ObservableCollection<int> hash_first_OnlySQ, ref System.Collections.Hashtable title_index_hash
            , ref System.Collections.Hashtable sq_index_hash)
        {
            System.Collections.Hashtable sq_index = new System.Collections.Hashtable();
            System.Collections.Hashtable onlySq_index = new System.Collections.Hashtable();
            ObservableCollection<string> all_sq = new ObservableCollection<string>();
            ObservableCollection<string> all_onlySq = new ObservableCollection<string>();

            hash_next.Clear();
            hash_next_OnlySQ.Clear();
            hash_first.Clear();
            hash_first_OnlySQ.Clear();
            title_index_hash.Clear();
            sq_index_hash.Clear();

            string folder = path + "\\" + pBuild_dat_file;
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string hn_filepath = folder + "\\" + Task.hash_next;
            string hf_filepath = folder + "\\" + Task.hash_first;
            string hno_filepath = folder + "\\" + Task.hash_next_onlySq;
            string hfo_filepath = folder + "\\" + Task.hash_first_onlySq;
            string tih_filepath = folder + "\\" + Task.title_index_hash;
            string sih_filepath = folder + "\\" + Task.sq_index_hash;
            if (File.Exists(hn_filepath))
            {
                read_filter_hash(hn_filepath, hf_filepath, hno_filepath, hfo_filepath, tih_filepath, sih_filepath, ref hash_next,
                    ref hash_first, ref hash_next_OnlySQ, ref hash_first_OnlySQ, ref title_index_hash, ref sq_index_hash);
                return;
            }
            hash_next.Add(0);
            hash_next_OnlySQ.Add(0);
            hash_first.Add(0);
            hash_first_OnlySQ.Add(0);

            for (int j = 0; j < psms.Count; ++j)
            {
                PSM psm = psms[j];
                int id = psm.Id;
                string title = psm.Title;
                for (int i = 0; i < title.Length - 2; ++i)
                {
                    string title_substring = title.Substring(i, 3);
                    if (title_index_hash[title_substring] == null)
                    {
                        title_index_hash[title_substring] = new List<int>();
                        List<int> cur_list = (List<int>)(title_index_hash[title_substring]);
                        cur_list.Add(j);
                    }
                    else
                    {
                        List<int> cur_list = (List<int>)(title_index_hash[title_substring]);
                        if (cur_list[cur_list.Count - 1] != j)
                            cur_list.Add(j);
                    }
                }
                string sq = psm.Sq;
                for (int i = 0; i < sq.Length - 2; ++i)
                {
                    string sq_substring = sq.Substring(i, 3);
                    if (sq_index_hash[sq_substring] == null)
                    {
                        sq_index_hash[sq_substring] = new List<int>();
                        List<int> cur_list = (List<int>)(sq_index_hash[sq_substring]);
                        cur_list.Add(j);
                    }
                    else
                    {
                        List<int> cur_list = (List<int>)(sq_index_hash[sq_substring]);
                        if (cur_list[cur_list.Count - 1] != j)
                            cur_list.Add(j);
                    }
                }
                string tmp_sq = psm.Sq + psm.Mod_sites;
                string tmp_onlySq = psm.Sq;
                if (!all_sq.Contains(tmp_sq))
                {
                    sq_index[tmp_sq] = id;
                    hash_first.Add(id);
                    all_sq.Add(tmp_sq);
                }
                else
                {
                    hash_next[(int)sq_index[tmp_sq]] = id;
                    hash_first.Add(hash_first[(int)sq_index[tmp_sq]]);
                    sq_index[tmp_sq] = id;
                }
                if (!all_onlySq.Contains(tmp_onlySq))
                {
                    onlySq_index[tmp_onlySq] = id;
                    hash_first_OnlySQ.Add(id);
                    all_onlySq.Add(tmp_onlySq);
                }
                else
                {
                    hash_next_OnlySQ[(int)onlySq_index[tmp_onlySq]] = id;
                    hash_first_OnlySQ.Add(hash_first_OnlySQ[(int)onlySq_index[tmp_onlySq]]);
                    onlySq_index[tmp_onlySq] = id;
                }
                hash_next.Add(0);
                hash_next_OnlySQ.Add(0);
            }
            write_filter_hash(hn_filepath, hf_filepath, hno_filepath, hfo_filepath, tih_filepath, sih_filepath, hash_next, hash_first,
                hash_next_OnlySQ, hash_first_OnlySQ, title_index_hash, sq_index_hash);
        }
        // 读取PSM信息
        public static ObservableCollection<PSM> readPSM(string filepath)
        {
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            StreamReader sr = new StreamReader(filepath, Encoding.Default);
            string strLine = sr.ReadLine();
            if (strLine.StartsWith("File_Name"))
                strLine = sr.ReadLine();
            int id = 1;
            while (strLine != "" && strLine != null)
            {
                string[] arr = strLine.Split('\t');
                PSM psm = null;
                if (arr.Length > 6) //结果中可能有没结果的（即没有肽段信息的），那么程序也会读入，并且将这些没有的信息赋值为空
                {
                    string[] strs = arr[14].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] strs2 = arr[10].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    string mod_sits = "";
                    for (int i = 1; i < strs.Length; ++i)
                    {
                        mod_sits += strs2[i - 1] + "#" + strs[i] + ";";
                    }
                    int missed = 0;
                    if (arr.Length > 16)
                        missed = int.Parse(arr[16]);
                    psm = new PSM(id, arr[0], arr[5], mod_sits, double.Parse(arr[9]), double.Parse(arr[2]), double.Parse(arr[4]), double.Parse(arr[6]),
                        double.Parse(arr[7]), double.Parse(arr[7]) * 1e6 / double.Parse(arr[2]), arr[11][0], strs[0], (arr[15] == "target"), arr[12], missed);
                }
                else
                {
                    psm = new PSM(id, arr[0], "", "", 0.0, double.Parse(arr[2]), double.Parse(arr[4]), 0.0,
                        0.0, 0.0, '0', "1", true, "", 0);
                }
                switch (psm.Specific_flag)
                {
                    case '3':
                        psm.Specific_flag = 'S'; //特异
                        break;
                    case '2':
                        psm.Specific_flag = 'N'; //N端特异
                        break;
                    case '1':
                        psm.Specific_flag = 'C'; //C端特异
                        break;
                    case '0':
                        psm.Specific_flag = 'O'; //非特异
                        break;
                }
                psm.Charge = psm.get_charge();
                psms.Add(psm);

                id++;
                strLine = sr.ReadLine();
            }
            sr.Close();
            return psms;
        }
        private static void read_cand(string path, ref ObservableCollection<PSM> all_psms)
        {
            FileStream filestream = new FileStream(path, FileMode.Open);
            BinaryReader objBinaryReader = new BinaryReader(filestream);
            try
            {
                while (true)
                {
                    int index = objBinaryReader.ReadInt32();
                    int pep_count = objBinaryReader.ReadInt32();
                    List<Peptide> cand_peps = new List<Peptide>(pep_count);
                    for (int i = 0; i < pep_count; ++i)
                    {
                        string sq = objBinaryReader.ReadString();
                        Peptide pep = new Peptide(sq);
                        int mod_count = objBinaryReader.ReadInt32();
                        ObservableCollection<Modification> mods = new ObservableCollection<Modification>();
                        for (int j = 0; j < mod_count; ++j)
                        {
                            int m_index = objBinaryReader.ReadInt32();
                            string m_name = objBinaryReader.ReadString();
                            int m_flag_index = objBinaryReader.ReadInt32();
                            mods.Add(new Modification(m_index, m_name, m_flag_index));
                        }
                        pep.Mods = mods;
                        int p_flag = objBinaryReader.ReadInt32();
                        pep.Tag_Flag = p_flag;
                        cand_peps.Add(pep);
                    }
                    all_psms[index].Cand_peptides = cand_peps;
                }
            }
            catch (Exception exe) { }
            objBinaryReader.Close();
            filestream.Close();
        }
        private static void write_cand(string path, ObservableCollection<PSM> all_psms)
        {
            FileStream filestream = new FileStream(path, FileMode.Create);
            BinaryWriter objBinaryWriter = new BinaryWriter(filestream);
            for (int i = 0; i < all_psms.Count; ++i)
            {
                objBinaryWriter.Write(i);
                List<Peptide> cand_peptides = all_psms[i].Cand_peptides;
                objBinaryWriter.Write(cand_peptides.Count); //写候选肽段的数目
                for (int j = 0; j < cand_peptides.Count; ++j)
                {
                    objBinaryWriter.Write(cand_peptides[j].Sq);
                    ObservableCollection<Modification> mods = cand_peptides[j].Mods;
                    objBinaryWriter.Write(mods.Count);
                    for (int k = 0; k < mods.Count; ++k)
                    {
                        objBinaryWriter.Write(mods[k].Index);
                        objBinaryWriter.Write(mods[k].Mod_name);
                        objBinaryWriter.Write(mods[k].Flag_Index);
                    }
                    objBinaryWriter.Write(cand_peptides[j].Tag_Flag);
                }
            }
            objBinaryWriter.Flush();
            objBinaryWriter.Close();
            filestream.Close();
        }
        public static void update_Cand_PSMs(List<string> filepaths, ref ObservableCollection<PSM> all_psms)
        {
            if (filepaths.Count == 0)
            {
                MessageBox.Show("No cand files");
                return;
            }
            string folder_path = filepaths[0].Substring(0, filepaths[0].LastIndexOf('\\')) + "\\" + pBuild_dat_file;
            if (!Directory.Exists(folder_path))
            {
                Directory.CreateDirectory(folder_path);
            }
            string cand_filepath = folder_path + "\\" + Task.cand_peptides;
            if (File.Exists(cand_filepath))
            {
                Cand_file_Exist = true;
                read_cand(cand_filepath, ref all_psms);
                return;
            }
            System.Collections.Hashtable title_int = new Hashtable();
            for (int i = 0; i < all_psms.Count; ++i)
                title_int[all_psms[i].Title] = i;
            for (int i = 0; i < filepaths.Count; ++i)
                update_Cand_PSM(filepaths[i], title_int, ref all_psms);
            write_cand(cand_filepath, all_psms);
        }
        private static void update_Cand_PSM(string filepath, System.Collections.Hashtable title_int, ref ObservableCollection<PSM> all_psms)
        {
            string[] strs0 = filepath.Split('.');
            string flag = strs0[strs0.Length - 3];
            int tagFlag = int.Parse(flag.Substring(1));
            StreamReader sr = new StreamReader(filepath, Encoding.Default);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.EndsWith(".dta")) //谱图结尾
                {
                    string title = line;
                    if (title_int[title] == null)
                        continue;
                    int index = (int)title_int[title];
                    List<Peptide> cand_peptides = new List<Peptide>();
                    while (true)
                    {
                        string line2 = sr.ReadLine();
                        if (line2 == null)
                            break;
                        string[] strs = line2.Split('\t');
                        if (strs.Length <= 4 || sr.EndOfStream)
                            break;
                        string sq = strs[1];
                        int mod_num = int.Parse(strs[6]);
                        int start = 7;
                        ObservableCollection<Modification> mods = new ObservableCollection<Modification>();
                        for (int i = 0; i < mod_num; ++i)
                        {
                            string mod_name = strs[start + 1];
                            string[] strs2 = mod_name.Split('#');
                            mods.Add(new Modification(int.Parse(strs[start]), strs2[0], int.Parse(strs2[1])));
                            start += 2;
                        }
                        Peptide peptide = new Peptide(sq);
                        peptide.Mods = mods;
                        peptide.Tag_Flag = tagFlag; //int.Parse(strs.Last())
                        cand_peptides.Add(peptide);
                    }
                    all_psms[index].Cand_peptides = cand_peptides;
                }
            }
            sr.Close();
        }
        public static System.Collections.Hashtable read_ms2_index(string path)
        {
            System.Collections.Hashtable ms2_scan_hash = new System.Collections.Hashtable();
            if (!File.Exists(path))
                return null;
            FileStream filestream = new FileStream(path, FileMode.Open);
            BinaryReader objBinaryReader = new BinaryReader(filestream);
            try
            {
                while (true)
                {
                    int scan_num = objBinaryReader.ReadInt32();
                    int index = objBinaryReader.ReadInt32();
                    ms2_scan_hash[scan_num] = index;
                }
            }
            catch (Exception exe)
            {
            }
            objBinaryReader.Close();
            filestream.Close();
            return ms2_scan_hash;
        }
        public static System.Collections.Hashtable read_ms1_index(string path, ref int min_scan, ref int max_scan)
        {
            if (!File.Exists(path))
                return null;
            System.Collections.Hashtable ms1_scan_hash = new System.Collections.Hashtable();
            FileStream filestream = new FileStream(path, FileMode.Open);
            BinaryReader objBinaryReader = new BinaryReader(filestream);
            try
            {
                while (true)
                {
                    int scan_num = objBinaryReader.ReadInt32();
                    if (scan_num < min_scan)
                        min_scan = scan_num;
                    if (scan_num > max_scan)
                        max_scan = scan_num;
                    int index = objBinaryReader.ReadInt32();
                    ms1_scan_hash[scan_num] = index;
                }
            }
            catch (Exception exe)
            {
            }
            objBinaryReader.Close();
            filestream.Close();
            return ms1_scan_hash;
        }
        public static void read_ModifyINI(string path, int flag = 1) //flag =1 表示正常的modification.ini，而flag = 2 表示pLink的modify.ini
        {
            for (int k = 0; k < 26; ++k)
            {
                Config_Help.normal_mod[k] = new ObservableCollection<string>();
                Config_Help.PEP_N_mod[k] = new ObservableCollection<string>();
                Config_Help.PEP_C_mod[k] = new ObservableCollection<string>();
                Config_Help.PRO_N_mod[k] = new ObservableCollection<string>();
                Config_Help.PRO_C_mod[k] = new ObservableCollection<string>();
            }
            StreamReader sr = new StreamReader(path, Encoding.Default);
            //sr.ReadLine(); //[modify]
            sr.ReadLine(); //@NUMBER_MODIFICATION=1609
            string strLine = sr.ReadLine();
            strLine = sr.ReadLine();
            while (strLine != null)
            {
                if (strLine.Trim() == "")
                    continue;
                string[] strs = strLine.Split('=');
                string mod_name = strs[0];
                string[] strs2 = strs[1].Split(' ');
                if (flag == 1)
                {
                    if (strs2.Length > 6) //有中性丢失
                    {
                        List<double> loss_masses = new List<double>();
                        for (int i = 5; i < strs2.Length - 1; i = i + 2)
                        {
                            loss_masses.Add(double.Parse(strs2[i]));
                        }
                        Config_Help.modStrLoss_hash[mod_name] = loss_masses;
                    }
                    else
                    {
                        List<double> loss_masses = new List<double>();
                        loss_masses.Add(0.0);
                        Config_Help.modStrLoss_hash[mod_name] = loss_masses;
                    }
                }
                else
                {
                    if (strs2.Length > 5) //有中性丢失
                    {
                        List<double> loss_masses = new List<double>();
                        for (int i = 5; i < strs2.Length - 1; i = i + 3)
                        {
                            double tmp = double.Parse(strs2[i]);
                            if (tmp != 0.0)
                                loss_masses.Add(tmp);
                        }
                        Config_Help.modStrLoss_hash[mod_name] = loss_masses;
                    }
                    else
                    {
                        List<double> loss_masses = new List<double>();
                        loss_masses.Add(0.0);
                        Config_Help.modStrLoss_hash[mod_name] = loss_masses;
                    }
                }
                double[] tmp_double = Config_Help.modStr_hash[mod_name] as double[];
                if (tmp_double == null)
                    tmp_double = new double[4];
                tmp_double[0] = double.Parse(strs2[2]);
                Config_Help.modStr_hash[mod_name] = tmp_double;
                string aa = strs2[0];
                if (aa == "[ALL]") //pLink这个字段显示ALL,表示ABCDEFGHIJKLMNOPQRSTUVWXYZ
                    aa = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                string type = strs2[1];
                if (type == "NORMAL") //NORMAL
                {
                    for (int k = 0; k < aa.Length; ++k)
                    {
                        Config_Help.normal_mod[aa[k] - 'A'].Add(mod_name);
                    }
                }
                else if (type == "PEP_N") //PEP_N
                {
                    for (int k = 0; k < aa.Length; ++k)
                    {
                        Config_Help.PEP_N_mod[aa[k] - 'A'].Add(mod_name);
                    }
                }
                else if (type == "PEP_C") //PEP_C
                {
                    for (int k = 0; k < aa.Length; ++k)
                    {
                        Config_Help.PEP_C_mod[aa[k] - 'A'].Add(mod_name);
                    }
                }
                else if (type == "PRO_N") //PRO_N
                {
                    for (int k = 0; k < aa.Length; ++k)
                    {
                        Config_Help.PRO_N_mod[aa[k] - 'A'].Add(mod_name);
                    }
                }
                else //PRO_C
                {
                    for (int k = 0; k < aa.Length; ++k)
                    {
                        Config_Help.PRO_C_mod[aa[k] - 'A'].Add(mod_name);
                    }
                }
                if (flag == 1) //pLink没有元素字段，如果读取pLink就不需要这个信息了
                {
                    string element_str = strs2.Last(); //获取修饰对应的元素信息，是以字符串形式表示，解析成Aa对象
                    Aa aa_obj = Aa.parse_Aa_byString(element_str);
                    Aa[] tmp_aa = Config_Help.modStr_elements_hash[mod_name] as Aa[];
                    if (tmp_aa == null)
                        tmp_aa = new Aa[4];
                    tmp_aa[0] = aa_obj;
                    Config_Help.modStr_elements_hash[mod_name] = tmp_aa;
                }
                strLine = sr.ReadLine();
                strLine = sr.ReadLine();
            }
            sr.Close();
        }
        private static bool read_quant(string start_path, string end_path, string hash_path, ref ObservableCollection<long> start, ref ObservableCollection<long> end, ref System.Collections.Hashtable qut_scan_hash)
        {
            start.Clear();
            end.Clear();
            qut_scan_hash.Clear();

            FileStream filestream = new FileStream(start_path, FileMode.Open);
            BinaryReader objBinaryReader = new BinaryReader(filestream);
            try
            {
                while (true)
                {
                    long number = 0;
                    if (sizeof(long) == 4) //4字节
                        number = objBinaryReader.ReadInt32();
                    else //8字节
                        number = objBinaryReader.ReadInt64();
                    start.Add(number);
                }
            }
            catch (Exception exe) { }
            objBinaryReader.Close();
            filestream.Close();
            filestream = new FileStream(end_path, FileMode.Open);
            objBinaryReader = new BinaryReader(filestream);
            try
            {
                while (true)
                {
                    long number = 0;
                    if (sizeof(long) == 32)
                        number = objBinaryReader.ReadInt32();
                    else
                        number = objBinaryReader.ReadInt64();
                    end.Add(number);
                }
            }
            catch (Exception exe) { }
            objBinaryReader.Close();
            filestream.Close();
            filestream = new FileStream(hash_path, FileMode.Open);
            objBinaryReader = new BinaryReader(filestream);
            try
            {
                while (true)
                {
                    //int length = objBinaryReader.ReadInt32();
                    string scan = objBinaryReader.ReadString();
                    int index = objBinaryReader.ReadInt32();
                    qut_scan_hash[scan] = index;
                }
            }
            catch (Exception exe) { }
            objBinaryReader.Close();
            filestream.Close();
            return true;
        }
        private static bool write_quant(string start_path, string end_path, string hash_path, ObservableCollection<long> start, ObservableCollection<long> end, System.Collections.Hashtable qut_scan_hash)
        {
            FileStream filestream = new FileStream(start_path, FileMode.Create);
            BinaryWriter objBinaryWriter = new BinaryWriter(filestream);
            for (int i = 0; i < start.Count; ++i)
                objBinaryWriter.Write(start[i]);
            objBinaryWriter.Flush();
            objBinaryWriter.Close();
            filestream.Close();
            filestream = new FileStream(end_path, FileMode.Create);
            objBinaryWriter = new BinaryWriter(filestream);
            for (int i = 0; i < end.Count; ++i)
                objBinaryWriter.Write(end[i]);
            objBinaryWriter.Flush();
            objBinaryWriter.Close();
            filestream.Close();
            filestream = new FileStream(hash_path, FileMode.Create);
            objBinaryWriter = new BinaryWriter(filestream);
            foreach (string key in qut_scan_hash.Keys)
            {
                objBinaryWriter.Write(key);
                objBinaryWriter.Write((int)qut_scan_hash[key]);
            }
            objBinaryWriter.Flush();
            objBinaryWriter.Close();
            filestream.Close();
            return true;
        }
        public static bool create_3D_index(string path, int osType, System.Collections.Hashtable title_hash, ref ObservableCollection<long> start, ref ObservableCollection<long> end, ref System.Collections.Hashtable qut_scan_hash)
        {
            FileInfo f = new FileInfo(path);
            long s1 = f.Length;
            if (s1 == 0)
                return false;
            string folder_path = path.Substring(0, path.LastIndexOf('\\')) + "\\" + pBuild_dat_file;
            if (!Directory.Exists(folder_path))
            {
                Directory.CreateDirectory(folder_path);
            }
            string quant_start_filepath = folder_path + "\\" + Task.quant_index_start;
            string quant_end_filepath = folder_path + "\\" + Task.quant_index_end;
            string quant_hash_filepath = folder_path + "\\" + Task.quant_index_hash;
            if (File.Exists(quant_start_filepath))
            {
                return read_quant(quant_start_filepath, quant_end_filepath, quant_hash_filepath, ref start, ref end, ref qut_scan_hash);
            }
            start.Clear();
            end.Clear();
            qut_scan_hash.Clear();
            //long[] position = new long[100000000]; //这句老是崩
            List<long> position = new List<long>();

            StreamReader reader = new StreamReader(path, Encoding.Default);

            int i = 0;
            int line = 0;
            int pos = 0;
            string strLine;
            string[] array;
            while (!reader.EndOfStream)
            {
                strLine = reader.ReadLine();
                pos += strLine.Length + osType;//换行回车
                //position[line] = pos;
                position.Add(pos);
                //建索引
                array = strLine.Split(new Char[] { '\t', ' ' });
                if (array[0] == "@")
                {
                    i++;
                    start.Add(position[line]);//一条记录起始位置  下一条  i++怎么办  
                    if (i == 1) { }
                    else
                    {
                        end.Add(position[line - 1]);
                    }
                    string title = array[2];
                    string[] strs = title.Split('.');
                    int scan_num = int.Parse(strs[1]);
                    int pParse_num = int.Parse(strs[4]);
                    if (title_hash[title.Split('.')[0]] == null) //如果定量跑的数据与定性跑的不是同一个数据
                    {
                        MessageBox.Show(title);
                        reader.Close();
                        return false;
                    }
                    qut_scan_hash[(int)(title_hash[title.Split('.')[0]]) + "." + scan_num + "." + pParse_num] = i - 1;
                    while (!reader.EndOfStream)
                    {
                        strLine = reader.ReadLine();
                        line++;
                        pos += strLine.Length + osType;
                        //position[line] = pos;
                        position.Add(pos);
                        array = strLine.Split(new Char[] { '\t', ' ' });
                        if (array[0] == "@")
                        {
                            start.Add(position[line]);//下一条记录开始位置
                            end.Add(position[line - 1]);//上一条记录终止位置
                            i++;
                            title = array[2];
                            strs = title.Split('.');
                            scan_num = int.Parse(strs[1]);
                            pParse_num = int.Parse(strs[4]);
                            qut_scan_hash[(int)(title_hash[title.Split('.')[0]]) + "." + scan_num + "." + pParse_num] = i - 1;
                            break;
                        }
                    }
                }
                line++;
            }
            end.Add(position[line - 1]);
            reader.Close();
            return write_quant(quant_start_filepath, quant_end_filepath, quant_hash_filepath, start, end, qut_scan_hash);
        }
        public static void read_3D_List(string path, int osType, int index, ObservableCollection<long> start, ObservableCollection<long> end, ref List<string> list)
        {
            if (path == "")
                return;
            StreamReader reader = new StreamReader(path, Encoding.Default);
            reader.BaseStream.Seek(start[index], SeekOrigin.Begin);
            long pos = start[index];
            while (pos < end[index])
            {
                string temp = reader.ReadLine();
                pos += temp.Length + osType;
                list.Add(temp);
            }
            reader.Close();
        }

        public static List<string> read_RawINI(string raw_ini_file, ref System.Collections.Hashtable title_hash, ref string fragment_type)
        {
            title_hash.Clear();
            List<string> raw_file_paths = new List<string>();
            StreamReader reader = new StreamReader(raw_ini_file, Encoding.Default);
            string line = reader.ReadLine(); //读取有几个Raw文件。
            fragment_type = line.Split(' ')[1];
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                string raw_name = line;
                title_hash[raw_name] = raw_file_paths.Count;
                raw_file_paths.Add(raw_name);
            }
            reader.Close();
            return raw_file_paths;
        }

        public static List<double> read_Mass_Error(string spectra_file, ref List<double> target_mass_error, ref List<double> decoy_mass_error
            , ref List<double> target_mass_error_ppm, ref List<double> decoy_mass_error_ppm, ref List<double> target_score, ref List<double> decoy_score)
        {
            List<double> max_mass_error_score = new List<double>();
            double max_mass_error = 0, max_mass_error_ppm = 0, max_score = 0;
            StreamReader reader = new StreamReader(spectra_file, Encoding.Default);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] strs = line.Split('\t');
                string target_decoy = strs[12];
                double mass_theroy = double.Parse(strs[5]);
                double mass_error = double.Parse(strs[6]);
                double mass_error_ppm = mass_error * 1.0e6 / mass_theroy;
                double score = double.Parse(strs[8]);
                score = -Math.Log10(score);
                if (Math.Abs(mass_error) > max_mass_error)
                    max_mass_error = Math.Abs(mass_error);
                if (score > max_score)
                    max_score = score;
                if (Math.Abs(mass_error_ppm) > max_mass_error_ppm)
                    max_mass_error_ppm = Math.Abs(mass_error_ppm);
                if (target_decoy == "target")
                {
                    target_mass_error.Add(mass_error);
                    target_mass_error_ppm.Add(mass_error_ppm);
                    target_score.Add(score);
                }
                else
                {
                    decoy_mass_error.Add(mass_error);
                    decoy_mass_error_ppm.Add(mass_error_ppm);
                    decoy_score.Add(score);
                }
            }
            reader.Close();
            max_mass_error_score.Add(max_mass_error);
            max_mass_error_score.Add(max_mass_error_ppm);
            max_mass_error_score.Add(max_score);
            return max_mass_error_score;
        }
        //读取元素表及氨基酸表
        public static void read_ElementINI(string element_ini_file)
        {
            StreamReader reader = new StreamReader(element_ini_file, Encoding.Default);
            string line = reader.ReadLine();
            int element_index = 0;
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                if (line.Trim() == "")
                    continue;
                string last = line.Split('=')[1];
                string[] all = last.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                string element_name = all[0];
                string[] mass_str = all[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string[] percent_str = all[2].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                double max_percent = 0.0;
                int max_index = -1;
                for (int i = 0; i < percent_str.Length; ++i)
                {
                    double percent_double = double.Parse(percent_str[i]);
                    if (percent_double > max_percent)
                    {
                        max_percent = percent_double;
                        max_index = i;
                    }
                }
                Config_Help.element_hash[element_name] = double.Parse(mass_str[max_index]);
                Config_Help.element_index_hash[element_name] = element_index++;

                SuperAtom superAtom = new SuperAtom();
                double max_prop = double.Parse(percent_str[0]);
                int max_prop_index = 0;
                int m_i = 0;
                superAtom.mass[m_i] = double.Parse(mass_str[0]);
                superAtom.prop[m_i++] = double.Parse(percent_str[0]);
                for (int i = 1; i < percent_str.Length; ++i)
                {
                    int delta_t = (int)(Math.Round(double.Parse(mass_str[i]) - superAtom.mass[m_i - 1], 0));
                    for (int j = 1; j < delta_t; ++j)
                    {
                        superAtom.mass[m_i] = superAtom.mass[m_i - 1] + 1;
                        superAtom.prop[m_i++] = 0.0;
                    }
                    superAtom.mass[m_i] = double.Parse(mass_str[i]);
                    superAtom.prop[m_i++] = double.Parse(percent_str[i]);
                    if (superAtom.prop[m_i - 1] > max_prop)
                    {
                        max_prop = superAtom.prop[m_i - 1];
                        max_prop_index = m_i - 1;
                    }
                }
                superAtom.type = m_i;
                superAtom.mono_mass = superAtom.mass[max_prop_index];
                SuperAtom.super[element_index - 1] = superAtom;
            }
            reader.Close();
        }
        public static void read_AAINI(string aa_ini_file)
        {
            Config_Help.label_name[0] = "None";
            Config_Help.mod_label_name[0] = "None";
            Config_Help.AA_Normal_Index = 0;
            for (int i = 0; i < 26; ++i)
            {
                Config_Help.aas[0, i] = new Aa();
            }
            StreamReader reader = new StreamReader(aa_ini_file, Encoding.Default);
            string line = reader.ReadLine();
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                if (line.Trim() == "")
                    continue;
                string last = line.Split('=')[1];
                string[] all = last.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                char aa_name = all[0][0];
                if (all[0].Length == 1 && aa_name >= 'A' && aa_name <= 'Z')
                {
                    Config_Help.aas[0, aa_name - 'A'] = Aa.parse_Aa_byString(all[1]);
                    Config_Help.mass_index[0, aa_name - 'A'] = Config_Help.aas[0, aa_name - 'A'].get_mass();
                }
            }
            reader.Close();
        }


        //public static void read_All_theory_isotope(string LIPV_file)
        //{
        //    StreamReader reader = new StreamReader(LIPV_file);
        //    string line = "";
        //    int index = 1;
        //    while (!reader.EndOfStream)
        //    {
        //        line = reader.ReadLine();
        //        if (line.Trim() == "")
        //            continue;
        //        string[] intensity_str = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        //        Config_Help.all_theory_iostope[index] = new Theory_IOSTOPE();
        //        for(int i=0;i<intensity_str.Length;++i)
        //        {
        //            Config_Help.all_theory_iostope[index].intensity.Add(double.Parse(intensity_str[i]));
        //        }
        //        ++index;
        //    }
        //    reader.Close();
        //}
        public static void read_Summary_Modification_TXT(string summary_file, ref List<Identification_Modification> modifications)
        {
            modifications = new List<Identification_Modification>();
            if (summary_file == "")
            {
                return;
            }
            StreamReader reader = new StreamReader(summary_file, Encoding.Default);
            bool flag = false;
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (line.Trim() == "Modifications:")
                {
                    flag = true;
                    line = reader.ReadLine(); //Modification的标题行直接略过
                    continue;
                }
                if (!flag)
                    continue;
                if (line[0] == '-')
                    break;
                string[] cur_strs = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                string name = cur_strs[0];
                int mod_count = int.Parse(cur_strs[2]);
                string[] cur_strs2 = cur_strs[1].Split(new char[] { ' ', '(', ')', '%' }, StringSplitOptions.RemoveEmptyEntries);
                modifications.Add(new Identification_Modification(name, int.Parse(cur_strs2[0]), double.Parse(cur_strs2[1]) * 0.01, mod_count));
            }
            reader.Close();
        }
        public static void read_Summary_TXT(string summary_file, ref Summary_Result_Information summary_information)
        {
            if (summary_file == "")
            {
                summary_information = null; return;
            }
            summary_information = new Summary_Result_Information();
            StreamReader reader = new StreamReader(summary_file, Encoding.Default);
            int flag = 0;
            List<Identification_Modification> modifications = new List<Identification_Modification>();
            List<Length_Distribution> length_distribution = new List<Length_Distribution>();
            List<Charge_Distribution> charge_distribution = new List<Charge_Distribution>();
            List<Missed_Cleavage_Distribution> missed_distribution = new List<Missed_Cleavage_Distribution>();
            List<Mixed_Spectra> mixed_spectra = new List<Mixed_Spectra>();
            List<Raw_Rate> raw_rates = new List<Raw_Rate>();
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (line.Trim() == "" || line[0] == '-')
                {
                    flag = 0;
                    continue;
                }
                else if (line.Trim() == "Peptide Level:")
                {
                    flag = 1;
                    continue;
                }
                else if (line.Trim() == "Cleavage:")
                {
                    flag = 2;
                    continue;
                }
                else if (line.Trim() == "Modifications:")
                {
                    flag = 3;
                    line = reader.ReadLine(); //读取下一行没用的信息，直接跳过
                    continue;
                }
                else if (line.Trim() == "Length Distribution of Sequences:")
                {
                    flag = 4;
                    continue;
                }
                else if (line.Trim() == "Charge Distribution of Peptides:")
                {
                    flag = 5;
                    continue;
                }
                else if (line.Trim() == "Missed Cleavages of Peptides:")
                {
                    flag = 6;
                    continue;
                }
                else if (line.Trim() == "Mixed Spectra of MS/MS Scans:")
                {
                    flag = 7;
                    continue;
                }
                else if (line.Trim() == "ID Rate:")
                {
                    flag = 8;
                    continue;
                }
                switch (flag)
                {
                    case 1: //Peptide Level:
                        string[] cur_strs = line.Split(':');
                        if (cur_strs[0] == "Spectra")
                        {
                            summary_information.spectra_number = int.Parse(cur_strs[1].Trim());
                        }
                        else if (cur_strs[0] == "Scans")
                        {
                            summary_information.scans_number = int.Parse(cur_strs[1].Trim());
                        }
                        else if (cur_strs[0] == "Peptides")
                        {
                            summary_information.peptides_number = int.Parse(cur_strs[1].Trim());
                        }
                        else if (cur_strs[0] == "Sequences")
                        {
                            summary_information.sequences_number = int.Parse(cur_strs[1].Trim());
                        }
                        else if (cur_strs[0] == "Proteins")
                        {
                            summary_information.proteins_number = int.Parse(cur_strs[1].Trim());
                        }
                        else if (cur_strs[0] == "Protein Groups")
                        {
                            summary_information.protein_groups_number = int.Parse(cur_strs[1].Trim());
                        }
                        else if (cur_strs[0] == "Decoy Spectra")
                        {
                            summary_information.decoy_spectra_number = int.Parse(cur_strs[1].Trim());
                        }
                        else if (cur_strs[0] == "Decoy Peptides")
                        {
                            summary_information.decoy_peptides_number = int.Parse(cur_strs[1].Trim());
                        }
                        else if (cur_strs[0] == "Decoy Proteins")
                        {
                            summary_information.decoy_proteins_number = int.Parse(cur_strs[1].Trim());
                        }
                        else if (cur_strs[0] == "Decoy Protein Groups")
                        {
                            summary_information.decoy_protein_groups_number = int.Parse(cur_strs[1].Trim());
                        }
                        break;
                    case 2: //Cleavage:
                        cur_strs = line.Split(':');
                        if (cur_strs[0] == "Specific")
                        {
                            cur_strs = cur_strs[1].Split(new char[] { ' ', '%' }, StringSplitOptions.RemoveEmptyEntries);
                            summary_information.Specific_number = int.Parse(cur_strs[0]);
                            summary_information.Specific = double.Parse(cur_strs[1]) * 0.01;
                        }
                        else if (cur_strs[0] == "C-term Specific")
                        {
                            cur_strs = cur_strs[1].Split(new char[] { ' ', '%' }, StringSplitOptions.RemoveEmptyEntries);
                            summary_information.C_term_specific_number = int.Parse(cur_strs[0]);
                            summary_information.C_term_specific = double.Parse(cur_strs[1]) * 0.01;
                        }
                        else if (cur_strs[0] == "N-term Specific")
                        {
                            cur_strs = cur_strs[1].Split(new char[] { ' ', '%' }, StringSplitOptions.RemoveEmptyEntries);
                            summary_information.N_term_specific_number = int.Parse(cur_strs[0]);
                            summary_information.N_term_specific = double.Parse(cur_strs[1]) * 0.01;
                        }
                        else if (cur_strs[0] == "Non-Specific")
                        {
                            cur_strs = cur_strs[1].Split(new char[] { ' ', '%' }, StringSplitOptions.RemoveEmptyEntries);
                            summary_information.Non_specific_number = int.Parse(cur_strs[0]);
                            summary_information.Non_specific = double.Parse(cur_strs[1]) * 0.01;
                        }
                        break;
                    case 3: //Modifications:
                        cur_strs = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        string name = cur_strs[0];
                        int mod_count = int.Parse(cur_strs[2]);
                        string[] cur_strs2 = cur_strs[1].Split(new char[] { ' ', '(', ')', '%' }, StringSplitOptions.RemoveEmptyEntries);
                        modifications.Add(new Identification_Modification(name, int.Parse(cur_strs2[0]), double.Parse(cur_strs2[1]) * 0.01, mod_count));
                        break;
                    case 4: //Length Distribution:
                        cur_strs = line.Split('\t');
                        cur_strs2 = cur_strs[2].Split(new char[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
                        length_distribution.Add(new Length_Distribution(int.Parse(cur_strs[0]), int.Parse(cur_strs[1]), double.Parse(cur_strs2[0]) * 0.01));
                        break;
                    case 5: //Charge Distribution:
                        cur_strs = line.Split('\t');
                        cur_strs2 = cur_strs[2].Split(new char[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
                        charge_distribution.Add(new Charge_Distribution(int.Parse(cur_strs[0]), int.Parse(cur_strs[1]), double.Parse(cur_strs2[0]) * 0.01));
                        break;
                    case 6: //Missed Cleavages of Peptides:
                        cur_strs = line.Split('\t');
                        cur_strs2 = cur_strs[2].Split(new char[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
                        missed_distribution.Add(new Missed_Cleavage_Distribution(int.Parse(cur_strs[0]), int.Parse(cur_strs[1]), double.Parse(cur_strs2[0]) * 0.01));
                        break;
                    case 7: //Mixed Spectra
                        cur_strs = line.Split('\t');
                        cur_strs2 = cur_strs[2].Split(new char[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
                        mixed_spectra.Add(new Mixed_Spectra(int.Parse(cur_strs[0]), int.Parse(cur_strs[1]), double.Parse(cur_strs2[0]) * 0.01));
                        break;
                    case 8: //ID Rate
                        try
                        {
                            string[] strs = line.Split(new char[] { '\t', ' ', '/', '=', '%' }, StringSplitOptions.RemoveEmptyEntries);
                            string raw_name = strs[0];
                            int ident_num = int.Parse(strs[1]);
                            int all_num = int.Parse(strs[2]);
                            double raw_rate = double.Parse(strs[3]) * 0.01;
                            raw_rates.Add(new Raw_Rate(raw_name, ident_num, all_num, raw_rate));
                        }
                        catch (Exception exe)
                        {

                        }
                        break;
                }
            }
            summary_information.modifications = modifications;
            summary_information.length_distribution = length_distribution;
            summary_information.charge_distribution = charge_distribution;
            summary_information.missed_cleavage_distribution = missed_distribution;
            summary_information.mixed_spectra = mixed_spectra;
            summary_information.raw_rates = raw_rates;
            reader.Close();
        }
        //读取pQuant.cfg文件，获取搜索的标记信息，是None、N15还是SILAC，同时将信息设置给Config_Help.label_type

        public static void read_pQuant_QNT(string pQuant_param_file, ref Summary_Param_Information summary_param_information)
        {
            StreamReader reader = new StreamReader(pQuant_param_file, Encoding.Default);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] cur_strs = line.Split('=');
                switch (cur_strs[0])
                {
                    case "LL_INFO_LABEL":
                        summary_param_information.ll_info_label = cur_strs[1];
                        break;
                    case "PPM_HALF_WIN_ACCURACY_PEAK":
                        summary_param_information.chrom_tolerance = cur_strs[1].Split(';')[0] + "ppm";
                        break;
                }
            }
            summary_param_information.label_efficiency = "99.0%";
            reader.Close();
        }
        public static bool read_pParse_PARA(string pParse_param_file)
        {
            bool isnot_mgf = true;
            StreamReader reader = new StreamReader(pParse_param_file, Encoding.Default);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] cur_strs = line.Split('=');
                switch (cur_strs[0])
                {
                    case "input_format":
                        if (cur_strs[1] == "raw")
                            isnot_mgf = true;
                        else if (cur_strs[1] == "mgf")
                            isnot_mgf = false;
                        break;
                }
            }
            reader.Close();
            return isnot_mgf;
        }
        public static void read_pParse_PARA(string pParse_param_file, ref Summary_Param_Information summary_param_information)
        {
            StreamReader reader = new StreamReader(pParse_param_file, Encoding.Default);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] cur_strs = line.Split('=');
                switch (cur_strs[0])
                {
                    case "co-elute":
                        summary_param_information.co_elute = (cur_strs[1] == "1" ? true : false);
                        break;
                    case "input_format":
                        summary_param_information.input_format = cur_strs[1];
                        break;
                }
            }
            reader.Close();
        }
        // modified by wrm 2016.03.04
        public static void read_pFind_PFD(string pFind_param_file, ref double fdr_value)
        {
            StreamReader reader = new StreamReader(pFind_param_file, Encoding.Default);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] cur_strs = line.Split('=');
                switch (cur_strs[0])
                {
                    case "threshold":    //
                        fdr_value = double.Parse(cur_strs[1]);
                        reader.Close();
                        return;
                }
            }
            fdr_value = 0.01;
            reader.Close();
        }
        // modified by wrm 2016.03.04
        public static void read_pFind_PFD(string pFind_param_file, ref Summary_Param_Information summary_param_information)
        {
            summary_param_information = new Summary_Param_Information();
            summary_param_information.open_search = true; //默认设置为开启了Open Search
            //FileStream fs = new FileStream(pFind_param_file, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(pFind_param_file, Encoding.Default);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] cur_strs = line.Split('=');
                switch (cur_strs[0])
                {
                    case "Precursor_Tolerance":
                        summary_param_information.ms_tolerance = cur_strs[1] + "Da";
                        break;
                    //case "mstolppm":
                    //    if (cur_strs[1] == "0")
                    //    {
                    //        summary_param_information.ms_tolerance += "Da";
                    //    }
                    //    else
                    //    {
                    //        summary_param_information.ms_tolerance += "ppm";
                    //    }
                    //    break;
                    case "Fragment_Tolerance":
                        Config_Help.mass_error = double.Parse(cur_strs[1]) * 1e-6;
                        Config_Help.is_ppm_flag = true;
                        summary_param_information.msms_tolerance = cur_strs[1] + "ppm";
                        break;
                    //case "msmstolppm":
                    //    if (cur_strs[1] == "0")
                    //    {
                    //        summary_param_information.msms_tolerance += "Da";
                    //        Config_Help.is_ppm_flag = false;
                    //    }
                    //    else
                    //    {
                    //        summary_param_information.msms_tolerance += "ppm";
                    //        Config_Help.is_ppm_flag = true;
                    //        Config_Help.mass_error *= 1e-6;
                    //    }
                    //    break;
                    //case "enzyme":
                    //    summary_param_information.enzyme = cur_strs[1];
                    //    break;
                    case "Database":
                        summary_param_information.fasta_path = cur_strs[1];
                        break;
                    //case "thread":
                    //    summary_param_information.thread_number = int.Parse(cur_strs[1]);
                    //    break;
                    case "Max_mod":
                        summary_param_information.max_var_mod_num = int.Parse(cur_strs[1]);
                        break;
                    case "Modify_num":
                        int mod_num = int.Parse(cur_strs[1]);
                        for (int i = 1; i <= mod_num; ++i)
                        {
                            line = reader.ReadLine();
                            summary_param_information.variable_modification += (line.Split('=')[1] + ";");
                        }
                        break;
                    case "fixedModify_num":
                        int fix_num = int.Parse(cur_strs[1]);
                        for (int i = 1; i <= fix_num; ++i)
                        {
                            line = reader.ReadLine();
                            summary_param_information.fix_modification += (line.Split('=')[1] + ";");
                        }
                        break;
                    //case "max_clv_sites":
                    //    summary_param_information.max_missing_cleavage_number = int.Parse(cur_strs[1]);
                    //    break;
                    //case "modpath":
                    //    summary_param_information.modification_path = cur_strs[1];
                    //    break;
                    //case "contaminant":
                    //    summary_param_information.contaminant_path = cur_strs[1];
                    //    break;
                    case "outputpath":
                        summary_param_information.task_path = cur_strs[1];
                        break;
                    case "outputname":
                        summary_param_information.task_path += cur_strs[1];
                        break;
                    case "msmsnum":
                        int raw_num = int.Parse(cur_strs[1]);
                        for (int i = 1; i <= raw_num; ++i)
                        {
                            line = reader.ReadLine();
                            summary_param_information.raws_path.Add(line.Split('=')[1]);
                        }
                        break;

                }
            }
            if (summary_param_information.contaminant_path == null || summary_param_information.contaminant_path == "")
                summary_param_information.contaminant_path = "NULL";
            if (summary_param_information.fix_modification == "")
                summary_param_information.fix_modification = "NULL";
            if (summary_param_information.variable_modification == "")
                summary_param_information.variable_modification = "NULL";
            //如果固定修饰或者可变修饰设置上了一个，那么就不是开放式搜索，Open_search=false
            if (summary_param_information.fix_modification != "NULL" || summary_param_information.variable_modification != "NULL")
                summary_param_information.open_search = false;
            reader.Close();
        }
       
        public static void read_protein(string fasta_file, ObservableCollection<PSM> psms, ref Protein_Panel protein_panel,
            ref System.Collections.Hashtable AC_Protein_index)
        {
            if (!File.Exists(fasta_file))
            {
                MessageBox.Show("The fasta path doesn't exist.");
                return;
            }
            const string REV_str = "REV_";
            //首先根据psms中的AC值，穷举所有的AC蛋白，并建立AC号对应于psms索引号的索引。
            System.Collections.Hashtable ac_hash = new System.Collections.Hashtable();
            System.Collections.Hashtable ac_hash2 = new Hashtable(); //从fasta文件读取蛋白信息，会根据AC来查重，如果重复则不记录
            for (int i = 0; i < psms.Count; ++i)
            {
                string[] all_acs = psms[i].AC.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < all_acs.Length; ++j)
                {
                    if (ac_hash[all_acs[j]] == null)
                    {
                        ac_hash[all_acs[j]] = new List<int>();
                    }
                    ((List<int>)ac_hash[all_acs[j]]).Add(i);
                }
            }
            //END
            protein_panel = new Protein_Panel();
            ObservableCollection<Protein> identification_proteins = new ObservableCollection<Protein>();
            StreamReader reader = new StreamReader(fasta_file, Encoding.Default);
            int id = 0;
            string ac = "";
            string de = "";
            string sq = "";
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine().Trim();
                if (line == "")
                    continue;
                if (line[0] == '>')
                {
                    if (sq != "") //读取到一个蛋白
                    {
                        for (int t = 0; t < 2; ++t)
                        {
                            switch (t)
                            {
                                case 1:
                                    ac = REV_str + ac; //查找REV的蛋白质
                                    de = REV_str + de;
                                    char[] charArray = sq.ToCharArray();
                                    Array.Reverse(charArray);
                                    sq = new string(charArray); //反转SQ序列
                                    break;
                            }
                            if (ac_hash[ac] != null)
                            {
                                Protein protein = new Protein(++id, ac, de, sq, (List<int>)ac_hash[ac]);
                                bool peptide_isInProtein = true;
                                //更新蛋白的覆盖率
                                int[] all_aa_flag = new int[protein.SQ.Length];
                                for (int i = 0; i < protein.psm_index.Count; ++i)
                                {
                                    int aa_index = sq.IndexOf(psms[protein.psm_index[i]].Sq);
                                    if (aa_index < 0)
                                    {
                                        //MessageBox.Show(protein.AC + "\r\nThe Fasta file is not consitent with pFind protein file.");
                                        peptide_isInProtein = false;
                                        continue;
                                    }
                                    for (int j = aa_index; j < aa_index + psms[protein.psm_index[i]].Sq.Length; ++j)
                                    {
                                        all_aa_flag[j] = 1;
                                    }
                                }
                                int aa_coverage = 0;
                                for (int i = 0; i < protein.SQ.Length; ++i)
                                {
                                    if (all_aa_flag[i] == 1)
                                        ++aa_coverage;
                                }
                                protein.Coverage = aa_coverage * 1.0 / protein.SQ.Length;
                                if (ac_hash2[ac] == null && peptide_isInProtein)
                                {
                                    ac_hash2[ac] = 1;
                                    identification_proteins.Add(protein);
                                    //更新PSMS中的protein_index索引号
                                    List<int> ac_psms_list = (List<int>)ac_hash[ac];
                                    for (int i = 0; i < ac_psms_list.Count; ++i)
                                    {
                                        if (!psms[ac_psms_list[i]].Protein_index.Contains(identification_proteins.Count - 1))
                                            psms[ac_psms_list[i]].Protein_index.Add(identification_proteins.Count - 1);
                                    }
                                }
                            }
                        }
                        sq = "";
                    }
                    int index0 = line.IndexOf('|');
                    if (index0 > 5)
                    {
                        ac = line.Substring(1, index0 - 1).Trim();
                        de = line.Substring(index0 + 1);
                    }
                    else
                    {
                        int index = line.IndexOfAny(new char[] { '\t', ' ' });
                        if (index < 0)
                        {
                            ac = line.Substring(1).Trim();
                            de = "";
                        }
                        else
                        {
                            ac = line.Substring(1, index - 1).Trim();
                            de = line.Substring(index + 1);
                        }
                    }
                }
                else
                {
                    sq += line;
                }
            }
            //最后还有一个蛋白
            for (int t = 0; t < 2; ++t)
            {
                switch (t)
                {
                    case 1:
                        ac = REV_str + ac; //查找REV的蛋白质
                        de = REV_str + de;
                        char[] charArray = sq.ToCharArray();
                        Array.Reverse(charArray);
                        sq = new string(charArray); //反转SQ序列
                        break;
                }
                if (ac_hash[ac] != null)
                {
                    Protein protein = new Protein(++id, ac, de, sq, (List<int>)ac_hash[ac]);
                    //更新蛋白的覆盖率
                    int[] all_aa_flag = new int[protein.SQ.Length];
                    for (int i = 0; i < protein.psm_index.Count; ++i)
                    {
                        int aa_index = sq.IndexOf(psms[protein.psm_index[i]].Sq);
                        for (int j = aa_index; j < aa_index + psms[protein.psm_index[i]].Sq.Length; ++j)
                        {
                            if (j >= 0 && j < all_aa_flag.Length)
                                all_aa_flag[j] = 1;
                        }
                    }
                    int aa_coverage = 0;
                    for (int i = 0; i < protein.SQ.Length; ++i)
                    {
                        if (all_aa_flag[i] == 1)
                            ++aa_coverage;
                    }
                    protein.Coverage = aa_coverage * 1.0 / protein.SQ.Length;
                    if (ac_hash2[ac] == null)
                    {
                        ac_hash2[ac] = 1;
                        identification_proteins.Add(protein);
                        //更新PSMS中的protein_index索引号
                        List<int> ac_psms_list = (List<int>)ac_hash[ac];
                        for (int i = 0; i < ac_psms_list.Count; ++i)
                        {
                            if (!psms[ac_psms_list[i]].Protein_index.Contains(identification_proteins.Count - 1))
                                psms[ac_psms_list[i]].Protein_index.Add(identification_proteins.Count - 1);
                        }
                    }
                }
            }
            //identification_proteins = new ObservableCollection<Protein>(identification_proteins.OrderByDescending(a => a.psm_index.Count));
            //for (int i = 0; i < identification_proteins.Count; ++i)
            //{
            //    identification_proteins[i].ID = i + 1;
            //}
            protein_panel.identification_proteins = identification_proteins;
            for (int i = 0; i < identification_proteins.Count; ++i)
            {
                AC_Protein_index[identification_proteins[i].AC] = i;
            }
            reader.Close();
        }
        public static void update_Mod_Mass(List<string> all_mod_files) //读取1.mod和2.mod
        {
            List<int> all_indexes = new List<int>();
            for (int i = 0; i < all_mod_files.Count; ++i)
            {
                string path = all_mod_files[i];
                string[] tmps = path.Split('\\');
                int index = int.Parse(tmps.Last().Split('.')[0]);
                if (index >= 4)
                    break;
                StreamReader sr = new StreamReader(path, Encoding.Default);
                string line = sr.ReadLine(); //label_name
                line = sr.ReadLine(); //@Number=
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine(); //nameX=...
                    line = sr.ReadLine();
                    string[] strs = line.Split(new char[] { '#', '=', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    int index_tmp = int.Parse(strs[1]);
                    if (!all_indexes.Contains(index_tmp))
                        all_indexes.Add(index_tmp);   // 保存标记号
                    string name = strs[0];
                    double mass = double.Parse(strs[4]);   // 修饰质量
                    Aa aa = Aa.parse_Aa_byString(strs.Last());  // 获取氨基酸组成
                    double[] tmp_double = Config_Help.modStr_hash[name] as double[];
                    if (tmp_double == null)
                        continue;
                    tmp_double[index_tmp] = mass;
                    Config_Help.modStr_hash[name] = tmp_double;
                    Aa[] tmp_aa = Config_Help.modStr_elements_hash[name] as Aa[];
                    tmp_aa[index_tmp] = aa;
                    Config_Help.modStr_elements_hash[name] = tmp_aa;
                }
                sr.Close();
            }
            int max_index = 0;
            for (int i = 0; i < all_indexes.Count(); ++i)
            {
                Config_Help.mod_label_name[all_indexes[i]] = Config_Help.label_name[all_indexes[i]];  // 统计标记名称
                if (max_index < all_indexes[i])
                    max_index = all_indexes[i];
            }
            foreach (string key in Config_Help.modStr_hash.Keys)
            {
                double[] tmp_double = Config_Help.modStr_hash[key] as double[];
                for (int j = 1; j <= max_index; ++j)
                {
                    if (tmp_double[j] == 0.0)
                        tmp_double[j] = tmp_double[0];
                }
            }
        }
        public static void update_AA_Mass(List<string> all_aa_files)
        {
            Config_Help.label_name[0] = "None";
            for (int i = 0; i < all_aa_files.Count; ++i)
            {
                string path = all_aa_files[i];
                string[] tmps = path.Split('\\');
                int index = int.Parse(tmps.Last().Split('.')[0]);
                if (index >= Config_Help.mass_index.Length)
                    break;
                StreamReader reader = new StreamReader(path, Encoding.Default);
                string line = reader.ReadLine(); //Read first line, it is label name
                string[] line_str = line.Split('=');
                if (line_str[1] == Config_Help.label_name[0])
                    Config_Help.AA_Normal_Index = index;
                Config_Help.label_name[index] = line.Split('=')[1];
                line = reader.ReadLine(); //Read second line, it is number of aas
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    string[] strs = line.Split('=');
                    string information = strs[1];
                    string[] strs2 = information.Split('|');
                    Aa aa = Aa.parse_Aa_byString(strs2[1]);
                    Config_Help.mass_index[index, strs2[0][0] - 'A'] = aa.get_mass();
                    Config_Help.aas[index, strs2[0][0] - 'A'] = aa;
                }
                reader.Close();
            }
        }
        private static void read_ratio(string path, ObservableCollection<PSM> psms)
        {
            FileStream filestream = new FileStream(path, FileMode.Open);
            BinaryReader objBinaryReader = new BinaryReader(filestream);
            try
            {
                while (true)
                {
                    int index = objBinaryReader.ReadInt32();
                    double ratio = objBinaryReader.ReadDouble();
                    double sigma = objBinaryReader.ReadDouble();
                    psms[index].Ratio = ratio;
                    psms[index].Sigma = sigma;
                }
            }
            catch (Exception exe) { }
            objBinaryReader.Close();
            filestream.Close();
        }
        private static void write_ratio(string path, ObservableCollection<PSM> psms)
        {
            FileStream filestream = new FileStream(path, FileMode.Create);
            BinaryWriter objBinaryWriter = new BinaryWriter(filestream);
            for (int i = 0; i < psms.Count; ++i)
            {
                objBinaryWriter.Write(i);
                objBinaryWriter.Write(psms[i].Ratio);
                objBinaryWriter.Write(psms[i].Sigma);
            }
            objBinaryWriter.Flush();
            objBinaryWriter.Close();
            filestream.Close();
        }
        public static void getPSM_Ratio(string file_path, ObservableCollection<PSM> psms)
        {
            if (file_path == "")
                return;
            string folder_path = file_path.Substring(0, file_path.LastIndexOf('\\')) + "\\" + pBuild_dat_file;
            if (!Directory.Exists(folder_path))
            {
                Directory.CreateDirectory(folder_path);
            }
            string file_path1 = "";
            if (Config_Help.ratio_index == 1)
                file_path1 = folder_path + "\\" + Task.ratio_sigma_index1;
            else
                file_path1 = folder_path + "\\" + Task.ratio_sigma_index2;
            if (File.Exists(file_path1))
            {
                read_ratio(file_path1, psms);
                return;
            }
            //PSM的title哈希到PSM的索引号
            System.Collections.Hashtable title_hash = new System.Collections.Hashtable();
            for (int i = 0; i < psms.Count; ++i)
            {
                title_hash[psms[i].Title] = i;
            }

            StreamReader reader = new StreamReader(file_path, Encoding.Default);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine().Trim();
                if (line == "")
                    continue;
                string[] strs = line.Split('\t');
                string title = strs[0];
                if (title_hash[title] != null)
                {
                    int psm_index = (int)title_hash[title];
                    if (strs.Length < 11) //pQuant-new : 11 (old : 12)
                        continue;
                    if (strs.Length != 12) //pQuant-new
                    {
                        if (Config_Help.ratio_index == 2) //显示ratio2
                        {
                            psms[psm_index].Ratio = double.Parse(strs[11]); //pQuant-new : 8 (old : 7)
                            psms[psm_index].Sigma = double.Parse(strs[12]); //pQuant-new : 9 (old : 8)
                        }
                        else if (Config_Help.ratio_index == 1) //显示ratio1
                        {
                            psms[psm_index].Ratio = double.Parse(strs[8]); //pQuant-new : 11 (old : 10)
                            psms[psm_index].Sigma = double.Parse(strs[9]); //pQuant-new : 12 (old : 11)
                        }
                    }
                    else if (strs.Length == 12) //pQuant-old
                    {
                        if (Config_Help.ratio_index == 2)
                        {
                            psms[psm_index].Ratio = double.Parse(strs[7]);
                            psms[psm_index].Sigma = double.Parse(strs[8]);
                        }
                        else if (Config_Help.ratio_index == 1)
                        {
                            psms[psm_index].Ratio = double.Parse(strs[10]);
                            psms[psm_index].Sigma = double.Parse(strs[11]);
                        }
                    }
                }
            }
            reader.Close();
            write_ratio(file_path1, psms);
        }
        public static ObservableCollection<TaskInfo> load_all_recent_taskInfo(string ini_path)
        {
            ObservableCollection<TaskInfo> all_recent_taskInfo = new ObservableCollection<TaskInfo>();
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(ini_path, Encoding.Default);

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line.Trim() == "")
                        continue;
                    int index = line.IndexOf(' ');
                    string path = line.Substring(index + 1);
                    string[] strs = path.Split('\\');
                    TaskInfo ti = new TaskInfo(strs.Last(), path);
                    ti.ttimespan = TaskInfo.ShowTime(Directory.GetLastWriteTime(ti.tpath));
                    if (Directory.Exists(ti.tpath))
                        all_recent_taskInfo.Add(ti);
                }
                reader.Close();
            }
            catch (Exception e)
            {
                if (reader != null)
                    reader.Close();
                return all_recent_taskInfo;
            }
            return all_recent_taskInfo;
        }
        public static List<string> load_all_recent_taskPath(string ini_path)
        {
            List<string> all_recent_taskPath = new List<string>();
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(ini_path, Encoding.Default);


                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line.Trim() == "")
                        continue;
                    all_recent_taskPath.Add(line);
                }
                reader.Close();
            }
            catch (Exception e)
            {
                if (reader != null)
                    reader.Close();
                return all_recent_taskPath;
            }
            return all_recent_taskPath;
        }
        public static void update_recent_task_byRename(string ini_path, string old_task_path, string new_task_path)
        {
            old_task_path += "\r\n";
            new_task_path += "\r\n";
            string text = File.ReadAllText(ini_path);
            text = text.Replace(old_task_path, new_task_path);
            File.WriteAllText(ini_path, text);
        }
        public static void update_recent_task(string ini_path, string new_task_path)
        {
            //首先加载所有的task_path
            List<string> all_recent_taskPath = new List<string>();
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(ini_path, Encoding.Default);

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line.Trim() == "")
                        continue;
                    int index0 = line.IndexOf(' ');
                    line = line.Substring(index0 + 1);
                    all_recent_taskPath.Add(line);
                }
                reader.Close();
            }
            catch (Exception e)
            {
                if (reader != null)
                    reader.Close();
            }
            //查看是否文件中存在新的Task_path，如果存在，那么将位置放在最前面，如果不存在，则在最前面加上它
            int index = all_recent_taskPath.IndexOf(new_task_path);
            if (index == -1)
            {
                if (all_recent_taskPath.Count != 0)
                {
                    string old_last_path = all_recent_taskPath.Last();
                    for (int i = all_recent_taskPath.Count - 2; i >= 0; --i)
                    {
                        all_recent_taskPath[i + 1] = all_recent_taskPath[i];
                    }
                    all_recent_taskPath[0] = new_task_path;
                    all_recent_taskPath.Add(old_last_path);
                }
                else
                {
                    all_recent_taskPath.Add(new_task_path);
                }
            }
            else
            {
                for (int i = index - 1; i >= 0; --i)
                {
                    all_recent_taskPath[i + 1] = all_recent_taskPath[i];
                }
                all_recent_taskPath[0] = new_task_path;
            }
            FileStream fs = new FileStream(ini_path, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, Encoding.Default);
            for (int i = 0; i < Task.recent_task_path_num && i < all_recent_taskPath.Count; ++i)
            {
                sw.WriteLine((i + 1) + " " + all_recent_taskPath[i]);
            }
            sw.Flush();
            sw.Close();
            fs.Close();
        }
        public static void update_Rename_Task(string param_file_path, string old_task_name, string new_task_name)
        {
            string text = File.ReadAllText(param_file_path);
            text = text.Replace(old_task_name, new_task_name);
            File.WriteAllText(param_file_path, text);
        }
        public static void write_recent_task(string ini_path, ObservableCollection<TaskInfo> taskInfos)
        {
            FileStream fs = new FileStream(ini_path, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, Encoding.Default);
            for (int i = 0; i < taskInfos.Count; ++i)
            {
                sw.WriteLine((i + 1) + " " + taskInfos[i].tpath);
            }
            sw.Flush();
            sw.Close();
            fs.Close();
        }
        public static void write_recent_task2(string ini_path, List<string> taskInfos)
        {
            FileStream fs = new FileStream(ini_path, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, Encoding.Default);
            for (int i = 0; i < taskInfos.Count; ++i)
            {
                string path = taskInfos[i].Split(' ')[1];
                sw.WriteLine((i + 1) + " " + path);
            }
            sw.Flush();
            sw.Close();
            fs.Close();
        }
        public static void write_task_path_Process(string path, string task_path)
        {
            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, Encoding.Default);
            sw.WriteLine(task_path);
            sw.Flush();
            sw.Close();
            fs.Close();
        }
        public static string read_task_path_Process(string path)
        {
            StreamReader sr = new StreamReader(path, Encoding.Default);
            string result_path = sr.ReadLine();
            sr.Close();
            return result_path;
        }
        //AC_Protein_index为蛋白质AC号到identification_protein的索引号
        public static void read_protein_file(string path, System.Collections.Hashtable AC_Protein_index,
            ref ObservableCollection<Protein> identification_protein)
        {
            StreamReader reader = new StreamReader(path, Encoding.Default);
            string parent_AC = "";
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (line.Trim() == "")
                    continue;
                if (line.StartsWith("---")) //直接显示的是FDR1%
                    break;
                if (line[0] == '-') //去掉一行全部为"-----"的行
                    continue;
                if (line[0] == '\t' && line[1] == '\t') //如果开始为\t表示是蛋白质中的PSM列表信息，该信息我已经处理了，放在identificatiion_protein中
                    continue;
                if (line[0] != '\t') //Group
                {
                    string[] strs = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    string AC = strs[1];
                    parent_AC = AC;
                    if (AC_Protein_index[AC] == null)
                        continue;
                    int protein_index = (int)AC_Protein_index[AC];
                    identification_protein[protein_index].Score = double.Parse(strs[2]);
                    identification_protein[protein_index].Q_value = double.Parse(strs[3]);
                    identification_protein[protein_index].Coverage = double.Parse(strs[4]);
                    identification_protein[protein_index].PSM_num = int.Parse(strs[5]);
                    identification_protein[protein_index].Same_set_num = int.Parse(strs[6]);
                    identification_protein[protein_index].Sub_set_num = int.Parse(strs[7]);
                    identification_protein[protein_index].Have_Distinct_PSM = (strs[8] == "1" ? true : false);
                    identification_protein[protein_index].Parent_Protein_AC = ""; //本身这个蛋白就是蛋白Group，那么将它的蛋白Group的AC设置成空字符串
                }
                else if (line[0] == '\t') //SameSet或者是SubSet
                {
                    string[] strs = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    string AC = strs[1];
                    if (AC_Protein_index[AC] == null)
                        continue;
                    int protein_index = (int)AC_Protein_index[AC];
                    identification_protein[protein_index].Parent_Protein_AC += parent_AC + "/";
                    identification_protein[protein_index].Coverage = double.Parse(strs[2]);
                    identification_protein[protein_index].Same_Sub_Flag += strs[0] + "/";
                }
            }
            reader.Close();

            Hashtable Parent_AC_index = new Hashtable();
            ObservableCollection<Protein> identification_protein2 = new ObservableCollection<Protein>();
            for (int i = 0; i < identification_protein.Count; ++i)
            {
                if (identification_protein[i].Parent_Protein_AC != null)
                    identification_protein2.Add(identification_protein[i]);
            }
            identification_protein = new ObservableCollection<Protein>(identification_protein2);
            for (int i = 0; i < identification_protein.Count; ++i)
            {
                if (identification_protein[i].Parent_Protein_AC == "")
                    continue;
                string[] parent_acs = identification_protein[i].Parent_Protein_AC.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < parent_acs.Length; ++j)
                {
                    List<int> ac_list = null;
                    if (Parent_AC_index[parent_acs[j]] == null)
                        ac_list = new List<int>();
                    else
                        ac_list = Parent_AC_index[parent_acs[j]] as List<int>;
                    ac_list.Add(i);
                    Parent_AC_index[parent_acs[j]] = ac_list;
                }
            }
            for (int i = 0; i < identification_protein.Count; ++i)
            {
                if (identification_protein[i].Parent_Protein_AC != "")
                {
                    identification_protein[i].Protein_index = new List<int>();
                    continue;
                }
                List<int> ac_list = null;
                if (Parent_AC_index[identification_protein[i].AC] == null)
                    ac_list = new List<int>();
                else
                    ac_list = Parent_AC_index[identification_protein[i].AC] as List<int>;
                identification_protein[i].Protein_index = ac_list;
            }
        }

        public static string read_task_path_pfind_ini(string file_path)
        {
            string task_path = "";
            StreamReader sr = new StreamReader(file_path, Encoding.Default);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                string[] strs = line.Split('=');
                if (strs.Length == 2 && strs[0] == "outputpath")
                {
                    task_path = strs[1];
                    break;
                }
            }
            sr.Close();
            return task_path;
        }

        public static void update_pfind_cfg(string pfind_cfg_path, string old_str, string new_str)
        {
            old_str += "\r\n";
            new_str += "\r\n";
            string text = File.ReadAllText(pfind_cfg_path, Encoding.Default);
            text = text.Replace(old_str, new_str);
            File.WriteAllText(pfind_cfg_path, text, Encoding.Default);
        }

        public static string get_license(string license_path, ref bool flag)
        {
            string text = File.ReadAllText(license_path);
            string[] strs = text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (strs.Length < 2)
                flag = false;
            return strs.First();
        }
        public static string get_userInformation_inLicense(string license_path)
        {
            string text = File.ReadAllText(license_path);
            string[] strs = text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string information = "";
            for (int i = 1; i < strs.Length - 1; ++i)
                information += strs[i] + "\n";
            information += strs[strs.Length - 1];
            return information;
        }

        //将一个RAW的一级谱打了二级的谱峰提取出来
        public static List<List<Spectra_MS1>> load_ms1(string pf_path, string raw_name, MainWindow mainW)
        {
            List<List<Spectra_MS1>> ms1_list = new List<List<Spectra_MS1>>();
            List<Spectra_MS1> ms1_fragment_list = new List<Spectra_MS1>(); //所有一级谱图中的所有碎裂谱峰
            List<Spectra_MS1> ms1_identification_list = new List<Spectra_MS1>(); //所有一级谱图中的所有鉴定到的谱峰
            ms1_list.Add(ms1_fragment_list);
            ms1_list.Add(ms1_identification_list);
            FileStream filestream = new FileStream(pf_path, FileMode.Open);
            if (filestream == null)
                return ms1_list;
            BinaryReader objBinaryReader = new BinaryReader(filestream);
            int all_scan_num = objBinaryReader.ReadInt32();
            int title_length = objBinaryReader.ReadInt32();
            string title = new string(objBinaryReader.ReadChars(title_length));
            for (int i = 0; i < all_scan_num; ++i)
            {
                int scan_number = objBinaryReader.ReadInt32();
                int peaks_num = objBinaryReader.ReadInt32();
                ObservableCollection<PEAK> peaks = new ObservableCollection<PEAK>();
                for (int j = 0; j < peaks_num; ++j)
                {
                    double mz = objBinaryReader.ReadDouble();
                    double intensity = objBinaryReader.ReadDouble();
                    peaks.Add(new PEAK(mz, intensity));
                }
                int precusor_num = objBinaryReader.ReadInt32();
                double precusor_mz = objBinaryReader.ReadDouble();
                int charge = objBinaryReader.ReadInt32();
                int index = pf_path.LastIndexOf('.');
                string ms2_pf_file = pf_path.Substring(0, index) + "_" + mainW.fragment_type + ".pf2";
                List<double> fragment_mzs = new List<double>();
                List<double> identification_mzs = new List<double>();
                List<int> identification_mzs_index = new List<int>();
                mainW.read_peaks2(ms2_pf_file, raw_name, mainW.ms2_scan_hash[(int)(mainW.title_hash[raw_name])], scan_number,
                    ref fragment_mzs, ref identification_mzs, ref identification_mzs_index);
                Spectra_MS1 ms1 = new Spectra_MS1(peaks, scan_number, 0, 0.0, 0.0, 0, 0.0);
                ms1.update_peaks(fragment_mzs, null); //只考虑碎裂的谱峰
                ms1_fragment_list.Add(ms1);
                ms1 = new Spectra_MS1(peaks, scan_number, 0, 0.0, 0.0, 0, 0.0);
                ms1.update_peaks(identification_mzs, identification_mzs_index); //只考虑鉴定的谱峰

                ms1_identification_list.Add(ms1);
            }
            objBinaryReader.Close();
            filestream.Close();
            return ms1_list;
        }
        //feature_number = 193*2=386
        public static void read_model(string model_path, int feature_number, ref List<int> all_feature, ref List<double> all_aa, ref List<List<Interv>> all_interv)
        {
            for (int i = 0; i <= feature_number; ++i)
            {
                all_aa.Add(0.0);
                all_interv.Add(new List<Interv>());
            }
            StreamReader sr = new StreamReader(model_path, Encoding.Default);
            int feature_id = 1;
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (feature_id > feature_number)
                    break;
                if ((feature_id >= 4 && feature_id <= 117) || (feature_id >= 156 && feature_id <= 193) ||
                (feature_id >= 4 + 193 && feature_id <= 117 + 193) || (feature_id >= 156 + 193 && feature_id <= 193 + 193))
                {
                    all_aa[feature_id] = double.Parse(line);
                    if (all_aa[feature_id] != 0.0)
                        all_feature.Add(feature_id);
                }
                else
                {
                    while (line != "")
                    {
                        int index = line.IndexOf(' ');
                        string one = line.Substring(0, index);
                        string[] strs = one.Split(':');
                        Interv ii = new Interv(double.Parse(strs[0]), double.Parse(strs[1]));
                        all_interv[feature_id].Add(ii);
                        line = line.Substring(index + 1);
                    }
                    if (all_interv[feature_id].Count > 0)
                        all_feature.Add(feature_id);
                }
                ++feature_id;
            }
            sr.Close();
        }
        public static void load_pNovo_param(string pNovo_param_path)
        {
            StreamReader sr = new StreamReader(pNovo_param_path, Encoding.Default);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Trim() == "")
                    continue;
                string[] str = line.Split('=');
                if (str[0].Length == 1 && str[0][0] >= 'A' && str[0][0] <= 'Z')
                {
                    string line2 = sr.ReadLine();
                    string[] srs2 = line2.Split('=');
                    Config_Help_pNovo.AA_Modification[line[0]] = srs2[1];
                }
            }
            sr.Close();
        }
        public static ObservableCollection<PSM> load_pNovo_result(string pNovo_result_path)
        {
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            StreamReader sr = new StreamReader(pNovo_result_path, Encoding.Default);
            string title = "";
            List<Peptide> cand_peptides = new List<Peptide>();
            List<double> scores = new List<double>();
            int id = 0;
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                line = line.Trim();
                if (line == "")
                    continue;
                if (line[0] == 'S')
                {
                    if (title != "")
                    {
                        string sq = "", mod_sites = "";
                        double score = 0.0;
                        if (cand_peptides.Count > 0)
                        {
                            sq = cand_peptides[0].Sq;
                            mod_sites = Modification.get_modSites(cand_peptides[0].Mods, 1);
                            score = scores[0];
                        }
                        PSM psm = new PSM(++id, title, sq, mod_sites, score);
                        psm.Cand_peptides = cand_peptides;
                        psms.Add(psm);
                    }
                    title = line.Split('\t')[1];
                    cand_peptides = new List<Peptide>();
                    scores = new List<double>();
                }
                else if (line[0] == 'P')
                {
                    string[] strs = line.Split('\t');
                    string sq = strs[1];
                    double score = double.Parse(strs[2]);
                    cand_peptides.Add(Config_Help_pNovo.parseBySQ(sq));
                    scores.Add(score);
                }
            }
            string sq0 = "", mod_sites0 = "";
            double score0 = 0.0;
            if (cand_peptides.Count > 0)
            {
                sq0 = cand_peptides[0].Sq;
                mod_sites0 = Modification.get_modSites(cand_peptides[0].Mods, 1);
                score0 = scores[0];
            }
            PSM psm0 = new PSM(++id, title, sq0, mod_sites0, score0);
            psm0.Cand_peptides = cand_peptides;
            psms.Add(psm0);
            sr.Close();
            return psms;
        }
        public static string write_dta_file(string file_path, Spectra spec)
        {
            string tmp_file = spec.Title.Replace('.', '_');
            tmp_file = tmp_file.Substring(0, tmp_file.LastIndexOf('_'));
            string folder = file_path + File_Help.pBuild_tmp_file;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            file_path = folder + "\\" + tmp_file + ".dta";
            spec.write_MGF(file_path);
            return file_path;
        }
    }
}
