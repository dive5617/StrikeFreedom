using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace pConfig
{
    public class File_Helper
    {
        public static ObservableCollection<Database> load_DB(string db_ini_path)
        {
            ObservableCollection<Database> databases = new ObservableCollection<Database>();
            StreamReader sr = new StreamReader(db_ini_path, Encoding.Default); //, System.Text.Encoding.Default
            string cur_db_name = "", cur_db_path = "";
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Length >= 2 && line.Substring(0, 2) == "DB")
                {
                    string[] strs = line.Split(new char[] { '=', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    cur_db_name = strs[1];
                }
                else if (line.Length >= 4 && line.Substring(0, 4) == "PATH")
                {
                    string[] strs = line.Split(new char[] { '=', '"' }, StringSplitOptions.RemoveEmptyEntries);
                    cur_db_path = strs[1];
                    databases.Add(new Database(cur_db_name, cur_db_path));
                }
            }
            sr.Close();
            return databases;
        }
        public static bool save_DB(string db_ini_path, ObservableCollection<Database> databases)
        {
            try
            {
                FileStream fs = new FileStream(db_ini_path, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, Encoding.Default);
                sw.WriteLine("@NUMBER_DB=" + databases.Count);
                for (int i = 0; i < databases.Count; ++i)
                {
                    sw.WriteLine("");
                    sw.WriteLine("DB" + (i + 1) + "=" + databases[i].DB_Name + ";");
                    sw.WriteLine("PATH" + (i + 1) + "=\"" + databases[i].DB_Path + "\"");
                }
                sw.Flush();
                sw.Close();
                fs.Close();
                return true;
            }
            catch (Exception exe)
            {
                MessageBox.Show(Message_Helper.ADMINISTRATOR_Message);
                return false;
            }
        }
        public static ObservableCollection<Modification> load_Modification(string mod_ini_path)
        {
            ObservableCollection<Modification> modifications = new ObservableCollection<Modification>();
            StreamReader sr = new StreamReader(mod_ini_path, Encoding.Default);
            Modification cur_modification = new Modification();
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Length >= 4 && line.Substring(0, 4) == "name")
                {
                    string[] strs = line.Split(new char[] { '=', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    cur_modification.Name = strs[1];
                    cur_modification.Is_common = ((strs[2] == "0") ? true : false);
                    line = sr.ReadLine(); //读取下一行
                    strs = line.Split(new char[] { '=', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    cur_modification.Mod_site = strs[1];
                    cur_modification.Position = strs[2];
                    switch (cur_modification.Position)
                    {
                        case "PEP_N":
                            cur_modification.Position_Display = "Peptide N-term";
                            break;
                        case "PEP_C":
                            cur_modification.Position_Display = "Peptide C-term";
                            break;
                        case "PRO_N":
                            cur_modification.Position_Display = "Protein N-term";
                            break;
                        case "PRO_C":
                            cur_modification.Position_Display = "Protein C-term";
                            break;
                        case "NORMAL":
                            cur_modification.Position_Display = "NORMAL";
                            break;
                            
                    }
                    cur_modification.Mod_mass = double.Parse(strs[3]);
                    int neutral_loss_start_index = 5;
                    int neutral_loss_num = int.Parse(strs[5]);
                    if (neutral_loss_num > 0)
                    {
                        for (int i = 1; i <= neutral_loss_num; ++i)
                        {
                            double neutral_loss_mass = double.Parse(strs[neutral_loss_start_index + 1 + (i - 1) * 2]);
                            cur_modification.Neutral_loss.Add(neutral_loss_mass);
                        }
                    }
                    cur_modification.Composition = strs.Last();
                    cur_modification.parse_netural_loss();
                    modifications.Add(cur_modification);
                    cur_modification = new Modification();
                }
            }
            sr.Close();
            return modifications;
        }

        public static ObservableCollection<Element> load_Element(string element_ini_path, ref List<string> base_elements)
        {
            ObservableCollection<Element> elements = new ObservableCollection<Element>();
            StreamReader sr = new StreamReader(element_ini_path, Encoding.Default);
            Element cur_element = new Element();
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Length >= 1 && line.Substring(0, 1) == "E")
                {
                    string[] strs = line.Split(new char[] { '=', '|' }, StringSplitOptions.RemoveEmptyEntries);
                    cur_element.Name = strs[1];
                    string[] mass_strs = strs[2].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] ratio_strs = strs[3].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    double max_ratio_mass = double.MinValue;
                    double max_mass = 0.0;
                    for (int i = 0; i < mass_strs.Length; ++i)
                    {
                        double mass_tmp = double.Parse(mass_strs[i]);
                        double ratio_tmp = double.Parse(ratio_strs[i]);
                        cur_element.Mass.Add(mass_tmp);
                        cur_element.Ratio.Add(ratio_tmp);
                        if (ratio_tmp > max_ratio_mass)
                        {
                            max_mass = mass_tmp;
                            max_ratio_mass = ratio_tmp;
                        }
                    }
                    cur_element.MMass = max_mass;
                    cur_element.update_mass_str();
                    elements.Add(cur_element);
                    base_elements.Add(cur_element.Name);
                    Element.index_hash[cur_element.Name] = elements.Count - 1;
                    cur_element = new Element();
                }
            }
            sr.Close();
            return elements;
        }

        public static bool save_Mod(string modification_ini_path, ObservableCollection<Modification> mods)
        {
            try
            {
                FileStream fs = new FileStream(modification_ini_path, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, Encoding.Default);
                //sw.WriteLine("[modify]");
                sw.WriteLine("@NUMBER_MODIFICATION=" + mods.Count);
                for (int i = 0; i < mods.Count; ++i)
                {
                    int mod_index = i + 1;
                    int is_common = (mods[i].Is_common ? 0 : 1);
                    sw.WriteLine("name" + mod_index + "=" + mods[i].Name + " " + is_common);
                    string next_line = mods[i].Name + "=" + mods[i].Mod_site + " " + mods[i].Position + " "
                        + mods[i].Mod_mass.ToString("F6") + " " + mods[i].Mod_mass.ToString("F6") + " " + mods[i].Neutral_loss.Count;
                    for (int j = 0; j < mods[i].Neutral_loss.Count; ++j)
                    {
                        next_line += " " + mods[i].Neutral_loss[j].ToString("F6") + " " + mods[i].Neutral_loss[j].ToString("F6");
                    }
                    next_line += " " + mods[i].Composition;
                    sw.WriteLine(next_line);
                }
                sw.Flush();
                sw.Close();
                fs.Close();
                return true;
            }
            catch (Exception exe)
            {
                MessageBox.Show(Message_Helper.ADMINISTRATOR_Message);
                return false;
            }
        }
        public static ObservableCollection<Enzyme> load_Enzyme(string enzyme_ini_path)
        {
            ObservableCollection<Enzyme> enzymes = new ObservableCollection<Enzyme>();
            StreamReader sr = new StreamReader(enzyme_ini_path, Encoding.Default);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Length >= 2 && line.Substring(0, 2) == "EN")
                {
                    string[] strs = line.Split(new char[] { '=', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string name = strs[1];
                    string cleave_site = strs[2];
                    string ignore_site = strs[3];
                    string N_C = strs[4];
                    enzymes.Add(new Enzyme(name, cleave_site, ignore_site, N_C));
                }
            }
            sr.Close();
            return enzymes;
        }

        public static bool save_Enzyme(string enzyme_ini_path, ObservableCollection<Enzyme> enzymes)
        {
            try
            {
                FileStream fs = new FileStream(enzyme_ini_path, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, Encoding.Default);
                sw.WriteLine("@NUMBER_ENZYME=" + enzymes.Count);
                sw.WriteLine("");
                for (int i = 0; i < enzymes.Count; ++i)
                {
                    sw.WriteLine("EN" + (i + 1) + "=" + enzymes[i].Name + " " + enzymes[i].Cleave_site + " " +
                        enzymes[i].Ignore_site + " " + enzymes[i].N_C);
                }
                sw.Flush();
                sw.Close();
                fs.Close();
                return true;
            }
            catch (Exception exe)
            {
                MessageBox.Show(Message_Helper.ADMINISTRATOR_Message);
                return false;
            }
        }

        public static ObservableCollection<Amino_Acid> load_AA(string aa_ini_path, MainWindow mainW)
        {
            ObservableCollection<Amino_Acid> aas = new ObservableCollection<Amino_Acid>();
            StreamReader sr = new StreamReader(aa_ini_path, Encoding.Default);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Length >= 1 && line.Substring(0, 1) == "R")
                {
                    string[] strs = line.Split(new char[] { '=', '|' }, StringSplitOptions.RemoveEmptyEntries);
                    string name = strs[1];
                    string compostion_str = strs[2];
                    Amino_Acid aa = new Amino_Acid(name);
                    aa.Composition = compostion_str;
                    double mass = 0.0;
                    aa.Element_composition = Element_composition.parse(mainW, aa.Composition, ref mass);
                    aa.Mass = mass;
                    aas.Add(aa);
                }
            }
            sr.Close();
            return aas;
        }

        public static bool save_AA(string aa_ini_path, ObservableCollection<Amino_Acid> aas)
        {
            try
            {
                FileStream fs = new FileStream(aa_ini_path, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, Encoding.Default);
                sw.WriteLine("@NUMBER_RESIDUE=" + aas.Count);
                sw.WriteLine("");
                for (int i = 0; i < aas.Count; ++i)
                {
                    sw.WriteLine("R" + (i + 1) + "=" + aas[i].Name + "|" + aas[i].Composition + "|");
                }
                sw.Flush();
                sw.Close();
                fs.Close();
                return true;
            }
            catch (Exception exe)
            {
                MessageBox.Show(Message_Helper.ADMINISTRATOR_Message);
                return false;
            }
        }

        public static bool save_Element(string element_ini_path, ObservableCollection<Element> elements)
        {
            try
            {
                FileStream fs = new FileStream(element_ini_path, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, Encoding.Default);
                sw.WriteLine("@NUMBER_ELEMENT=" + elements.Count);
                for (int i = 0; i < elements.Count; ++i)
                {
                    string line = "E" + (i + 1) + "=" + elements[i].Name + "|";
                    for (int j = 0; j < elements[i].Mass.Count; ++j)
                    {
                        line += elements[i].Mass[j].ToString("F8") + ",";
                    }
                    line += "|";
                    for (int j = 0; j < elements[i].Ratio.Count; ++j)
                    {
                        line += elements[i].Ratio[j].ToString("F6") + ",";
                    }
                    line += "|";
                    sw.WriteLine(line);
                }
                sw.Flush();
                sw.Close();
                fs.Close();
                return true;
            }
            catch (Exception exe)
            {
                MessageBox.Show(Message_Helper.ADMINISTRATOR_Message);
                return false;
            }
        }

        public static ObservableCollection<Quantification> load_Quant(string quant_ini_path)
        {
            ObservableCollection<Quantification> quantifications = new ObservableCollection<Quantification>();
            StreamReader sr = new StreamReader(quant_ini_path, Encoding.Default);
            string line = sr.ReadLine(); //@NUMBER_LABEL=X
    
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();
                if (line == "") continue;
                string[] strs = line.Split(new char[]{'='}, StringSplitOptions.RemoveEmptyEntries);
                if (strs.Length == 1)
                {
                    quantifications.Add(new Quantification(strs[0], ""));
                }
                else
                {
                    Quantification quant=new Quantification(strs[0], strs[1]);
                    line = strs[1];
                    strs = line.Split(new char[] { ':', '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
                    List<Quant_Simple> qss = new List<Quant_Simple>();
                    for (int i = 1; i < strs.Length; i += 3)
                    {
                        if (strs[i - 1][0] == 'R')
                        {
                            char aa_name = strs[i][0];
                            string tmp_str = strs[i + 1];
                            string[] strs2 = tmp_str.Split(',');
                            string label0 = strs2[0];
                            string label1 = strs2[1];
                            qss.Add(new Quant_Simple(aa_name, label0, label1));
                        }
                    }
                    quant.All_quant = qss;
                    quantifications.Add(quant);
                }
            }
            sr.Close();
            return quantifications;
        }

        public static bool save_Quantification(string quant_ini_path, ObservableCollection<Quantification> quantifications)
        {
            try
            {
                FileStream fs = new FileStream(quant_ini_path, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, Encoding.Default);
                sw.WriteLine("@NUMBER_LABEL=" + quantifications.Count);
                for (int i = 0; i < quantifications.Count; ++i)
                {
                    sw.WriteLine(quantifications[i].Name + "=" + quantifications[i].All_quant_str);
                }
                sw.Flush();
                sw.Close();
                fs.Close();
                return true;
            }
            catch (Exception exe)
            {
                MessageBox.Show(Message_Helper.ADMINISTRATOR_Message);
                return false;
            }
        }

        public static bool add_database_containment(string old_fasta_path, string new_fasta_path, string containment_path)
        {
            try
            {
                StreamReader sr = new StreamReader(old_fasta_path, Encoding.Default);
                StreamReader sr2 = new StreamReader(containment_path, Encoding.Default);
                FileStream fs = new FileStream(new_fasta_path, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, Encoding.Default);
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    sw.WriteLine(line);
                }
                while (!sr2.EndOfStream)
                {
                    string line = sr2.ReadLine();
                    sw.WriteLine(line);
                }
                sr.Close();
                sr2.Close();
                sw.Close();
                fs.Close();
                return true;
            }
            catch (Exception exe)
            {
                MessageBox.Show(Message_Helper.ADMINISTRATOR_Message);
                return false;
            }
        }
        public static void restore_file(string new_file, string old_file) //使用old_file将new_file替换掉
        {
            string text = File.ReadAllText(old_file);
            File.WriteAllText(new_file, text);
        }
    }
}
