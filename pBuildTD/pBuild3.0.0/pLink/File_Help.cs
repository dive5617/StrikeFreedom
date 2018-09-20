using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild.pLink
{
    public class File_Help
    {
        public static ObservableCollection<PSM> load_plabel_file(string file_path, ref ObservableCollection<Spectra> spectra, ref Hashtable title_index, ref string link_name)
        {
            ObservableCollection<PSM> psms = new ObservableCollection<PSM>();
            spectra = new ObservableCollection<Spectra>();
            title_index = new Hashtable();
            List<string> modification = new List<string>();
            StreamReader sr = new StreamReader(file_path, Encoding.Default);
            link_name = "";
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line == "[FilePath]")
                {
                    line = sr.ReadLine();
                    spectra = load_spectra(line.Split('=').Last(), ref title_index);
                }
                else if (line == "[Modification]")
                {
                    while (true)
                    {
                        line = sr.ReadLine();
                        if (!line.Contains('='))
                        {
                            if (line == "[xlink]")
                            {
                                line = sr.ReadLine();
                                link_name = line.Split('=').Last();
                            }
                            break;
                        }
                        string mod_name = line.Split('=').Last();
                        modification.Add(mod_name);
                    }
                }
                else if (line == "[Total]")
                    break;
            }
            int ID = 1;
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.StartsWith("name"))
                {
                    string title = line.Split('=').Last();
                    title = title.ToUpper();
                    List<int> index = new List<int>();
                    List<Peptide> peptide = new List<Peptide>();
                    List<double> score = new List<double>();
                    line = sr.ReadLine().Split('=').Last();
                    string[] strs = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    int flag = int.Parse(strs[0]);
                    if (flag == 3) //二肽段
                    {
                        index.Add(int.Parse(strs[1]));
                        index.Add(int.Parse(strs[2]));
                        Peptide p1 = new Peptide(strs[3]);
                        Peptide p2 = new Peptide(strs[5]);

                        peptide.Add(p1);
                        peptide.Add(p2);
                        score.Add(double.Parse(strs[4]));
                        score.Add(double.Parse(strs[6]));
                        if (strs.Length > 7) //说明有修饰
                        {
                            for (int i = 7; i < strs.Length; ++i)
                            {
                                string[] strs2 = strs[i].Split(',');
                                int mod_position_index = int.Parse(strs2[0]);
                                int mod_index = int.Parse(strs2[1]) - 1;
                                string mod_name = modification[mod_index];
                                if (mod_position_index <= p1.Sq.Length) //修饰在第一条肽段上
                                {
                                    double[] tmp_double = Config_Help.modStr_hash[mod_name] as double[];
                                    p1.Mods.Add(new Modification(mod_position_index, tmp_double[0], mod_name, (List<double>)Config_Help.modStrLoss_hash[mod_name],
                                        0));
                                }
                                else //修饰在第二条肽段上
                                {
                                    double[] tmp_double = Config_Help.modStr_hash[mod_name] as double[];
                                    p2.Mods.Add(new Modification(mod_position_index - p1.Sq.Length, tmp_double[0], mod_name, (List<double>)Config_Help.modStrLoss_hash[mod_name],
                                        0));
                                }
                            }
                        }
                        p1.update();
                        p2.update();
                    }
                    else if (flag == 4) //三肽段
                    {
                        index.Add(int.Parse(strs[1]));
                        index.Add(int.Parse(strs[2]));
                        index.Add(int.Parse(strs[3]));
                        index.Add(int.Parse(strs[4]));
                        Peptide p1 = new Peptide(strs[5]);
                        Peptide p2 = new Peptide(strs[7]);
                        Peptide p3 = new Peptide(strs[9]);

                        peptide.Add(p1);
                        peptide.Add(p2);
                        peptide.Add(p3);
                        score.Add(double.Parse(strs[6]));
                        score.Add(double.Parse(strs[8]));
                        score.Add(double.Parse(strs[10]));
                        if (strs.Length > 11) //说明有修饰
                        {

                        }
                        p1.update();
                        p2.update();
                        p3.update();
                    }
                    if (title_index[title] == null)
                        continue;
                    int title_i = (int)title_index[title];
                    string sq = "", modsites = "";
                    double spectra_mass = 0.0, delta_mass = 0.0, delta_mass_ppm = 0.0;
                    spectra_mass = (spectra[title_i].Pepmass * spectra[title_i].Charge - Config_Help.massZI * spectra[title_i].Charge);
                    if (flag == 3)
                    {
                        sq = peptide[0].Sq + "(" + index[0] + ")-(" + index[1] + ")" + peptide[1].Sq;
                        for (int i = 0; i < peptide[0].Mods.Count; ++i)
                        {
                            modsites += peptide[0].Mods[i].Index + "," + peptide[0].Mods[i].Mod_name + "#"
                                + peptide[0].Mods[i].Flag_Index + ";";
                        }
                        for (int i = 0; i < peptide[1].Mods.Count; ++i)
                        {
                            modsites += (peptide[1].Mods[i].Index + peptide[0].Sq.Length) + "," + peptide[1].Mods[i].Mod_name + "#"
                                + peptide[1].Mods[i].Flag_Index + ";";
                        }
                        delta_mass = spectra_mass - (peptide[0].Pepmass + peptide[1].Pepmass + (double)Config_Help.link_hash[link_name]);
                    }
                    else if (flag == 4)
                    {
                        sq = peptide[0].Sq + "(" + index[0] + ")-(" + index[1] + ")" + peptide[1].Sq + "(" + index[2] + ")-(" + index[3] + ")" + peptide[2].Sq;
                        for (int i = 0; i < peptide[0].Mods.Count; ++i)
                        {
                            modsites += peptide[0].Mods[i].Index + "," + peptide[0].Mods[i].Mod_name + "#"
                                + peptide[0].Mods[i].Flag_Index + ";";
                        }
                        for (int i = 0; i < peptide[1].Mods.Count; ++i)
                        {
                            modsites += (peptide[1].Mods[i].Index + peptide[0].Sq.Length) + "," + peptide[1].Mods[i].Mod_name + "#"
                                + peptide[1].Mods[i].Flag_Index + ";";
                        }
                        for (int i = 0; i < peptide[2].Mods.Count; ++i)
                        {
                            modsites += (peptide[2].Mods[i].Index + peptide[0].Sq.Length + peptide[1].Sq.Length) + ","
                                + peptide[2].Mods[i].Mod_name + "#" + peptide[2].Mods[i].Flag_Index + ";";
                        }
                        delta_mass = spectra_mass - (peptide[0].Pepmass + peptide[1].Pepmass + peptide[2].Pepmass + 2 * (double)Config_Help.link_hash[link_name]);
                    }
                    delta_mass_ppm = delta_mass * 1.0e6 / spectra_mass;
                    psms.Add(new PSM(ID++, title, spectra[title_i].Charge, spectra[title_i].Pepmass, sq, modsites, delta_mass, delta_mass_ppm, peptide.Count, peptide, index, score, "", true));
                }
            }
            sr.Close();
            return psms;
        }
        public static ObservableCollection<pLink.PSM> readPSM_pLink(string filepath)
        {
            ObservableCollection<pLink.PSM> psms = new ObservableCollection<pLink.PSM>();
            StreamReader sr = new StreamReader(filepath, Encoding.Default);
            int id = 1;
            while (!sr.EndOfStream)
            {
                string strLine = sr.ReadLine();
                if (strLine.Trim() == "")
                    continue;
                string[] strs = strLine.Split('\t');
                if (!strs[5].Contains('-'))
                    continue;
                psms.Add(pLink.File_Help.parse_psm(id, strLine));
                ++id;
            }
            sr.Close();
            return psms;
        }
        public static List<string> load_xlink_name(string file_path, Task task, ref List<string> raw_file_paths, ref Hashtable title_hash)
        {
            List<string> xLink_names = new List<string>();
            StreamReader sr = new StreamReader(file_path);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Trim() == "")
                    continue;
                if (line.StartsWith("linker_total"))
                {
                    int count = int.Parse(line.Split('=')[1]);
                    for (int i = 0; i < count; ++i)
                    {
                        line = sr.ReadLine();
                        xLink_names.Add(line.Split('=')[1]);
                    }
                }
                else if (line.StartsWith("spec_num"))
                {
                    int count = int.Parse(line.Split('=')[1]);
                    for (int i = 0; i < count; ++i)
                    {
                        line = sr.ReadLine();
                        string path = line.Split('=')[1];
                        string[] strs = path.Split('\\');
                        string pf_name = strs.Last().Split('.').First();
                        if (pf_name.EndsWith("HCDFT"))
                            pf_name = pf_name.Substring(0, pf_name.Length - 6);
                        if (path.EndsWith("mgf"))
                            path = path.Substring(0, path.Length - 3) + "pf2";
                        task.title_PF2_Path[pf_name] = path;
                        raw_file_paths.Add(pf_name);
                        title_hash[pf_name] = i;
                    }
                }
            }
            sr.Close();
            return xLink_names;
        }
        public static void load_label_information(string file_path, pLink_Label pll)
        {
            StreamReader sr = new StreamReader(file_path);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Trim() == "")
                    continue;
                if (line.StartsWith("LL_INFO_LABEL"))
                {
                    string[] strs = line.Split(new char[]{'=',';'})[1].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    string label_name = strs.Last();
                    pll.Flag = 2; //无标记
                    if (label_name.StartsWith("R:-")) //表示交联剂的标记
                    {
                        pll.Flag = 0;
                    }
                    else if(label_name.StartsWith("R:*")) //表示肽段的标记
                    {
                        pll.Flag = 1;
                    }
                    string[] strs2 = label_name.Split(new char[] { ',', '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int start = 1; start < strs2.Length; start += 3)
                    {
                        string element_name1 = strs2[1];
                        string element_name2 = strs2[2];
                        double mass = (double)Config_Help.element_hash[element_name1] - (double)Config_Help.element_hash[element_name2];
                        pll.Element_names.Add(element_name1);
                        pll.Masses.Add(mass);
                    }
                }
            }
            sr.Close();
        }
        public static PSM parse_psm(int id, string line)
        {
            string[] strs = line.Split('\t');
            string[] strs2 = strs[0].Split('.');
            int charge = int.Parse(strs2[3]);
            string modsites = "";
            List<int> index = new List<int>();
            List<Peptide> peptide = new List<Peptide>();
            List<double> score = new List<double>();
            string sq = strs[5];
            string mod_str = strs[10];
            string[] tag_flag_strs = strs[14].Split('|');
            string[] sq_strs = sq.Split(new char[]{'-'}, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < sq_strs.Length; ++i)
            {
                string[] tmp_strs = sq_strs[i].Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                Peptide p = new Peptide(tmp_strs[0]);
                p.Tag_Flag = int.Parse(tag_flag_strs[2 * i]);
                peptide.Add(p);
                index.Add(int.Parse(tmp_strs[1]));
                score.Add(double.Parse(strs[9]));
            }
            if (mod_str != "" && mod_str != "nomod")
            {
                string[] strs3 = mod_str.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < strs3.Length; i += 2)
                {
                    int mod_index = int.Parse(strs3[i]);
                    string mod_name = strs3[i + 1];
                    if (mod_index <= peptide[0].Sq.Length + 1) //第一条肽段的修饰
                    {
                        peptide[0].Mods.Add(new Modification(mod_index, ((double[])Config_Help.modStr_hash[mod_name])[0], mod_name,
                        (List<double>)Config_Help.modStrLoss_hash[mod_name], 0));
                    }
                    else //第二条肽段的修饰
                    {
                        mod_index -= (peptide[0].Sq.Length + 3);
                        peptide[1].Mods.Add(new Modification(mod_index, ((double[])Config_Help.modStr_hash[mod_name])[0], mod_name,
                        (List<double>)Config_Help.modStrLoss_hash[mod_name], 0));
                    }
                }

            }
            for (int i = 0; i < peptide[0].Mods.Count; ++i)
            {
                modsites += peptide[0].Mods[i].Index + "," + peptide[0].Mods[i].Mod_name + "#"
                                + 0 + ";";
            }
            for (int i = 0; i < peptide[1].Mods.Count; ++i)
            {
                modsites += (peptide[1].Mods[i].Index + peptide[0].Sq.Length)  + "," + peptide[1].Mods[i].Mod_name + "#"
                                + 0 + ";";
            }
            return new PSM(id, strs[0], charge, double.Parse(strs[2]), sq, modsites, double.Parse(strs[7]), double.Parse(strs[8]), peptide.Count, peptide, index, score, strs[14], (strs[15] == "target"));
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
            string folder_path = file_path.Substring(0, file_path.LastIndexOf('\\')) + "\\" + pBuild.File_Help.pBuild_dat_file;
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
                    if (strs.Length < 13)
                        continue;
                    if (Config_Help.ratio_index == 2) //显示ratio2
                    {
                        psms[psm_index].Ratio = double.Parse(strs[8]);
                        psms[psm_index].Sigma = double.Parse(strs[9]);
                    }
                    else if (Config_Help.ratio_index == 1) //显示ratio1
                    {
                        psms[psm_index].Ratio = double.Parse(strs[11]);
                        psms[psm_index].Sigma = double.Parse(strs[12]);
                    }
                }
            }
            reader.Close();
            write_ratio(file_path1, psms);
        }
        private static ObservableCollection<Spectra> load_spectra(string file_path, ref Hashtable title_index)
        {
            ObservableCollection<Spectra> spectra = pBuild.File_Help.readMGF(file_path);
            for (int i = 0; i < spectra.Count; ++i)
            {
                title_index[spectra[i].Title.ToUpper()] = i;
            }
            return spectra;
        }
        public static void load_xlink(string file_path)
        {
            StreamReader sr = new StreamReader(file_path);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.StartsWith("name"))
                {
                    line = sr.ReadLine();
                    string[] strs = line.Split('=');
                    string name = strs[0];
                    string[] strs2 = strs[1].Split(' ');
                    double mass = double.Parse(strs2[2]);
                    Config_Help.link_hash[name] = mass;
                }
            }
            sr.Close();
        }
        public static void load_xx(string file_path)
        {
            StreamReader sr = new StreamReader(file_path);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Trim() == "")
                    continue;
                if (line[0] != 'L')
                    continue;
                string[] strs = line.Split(new char[] { '=', '|' }, StringSplitOptions.RemoveEmptyEntries);
                string element_name = strs[1];
                string element_str = strs[2];
                Config_Help.link_elements_hash[element_name] = Aa.parse_Aa_byString(element_str);
            }
            sr.Close();
        }
    }
}
