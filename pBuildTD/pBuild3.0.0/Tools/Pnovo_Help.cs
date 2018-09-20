using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class Pnovo_Help
    {
        public string Param_filename = "param.txt";
        public string mgf_path_title = "spec_path1";
        public string output_path_title = "out_path";
        public string mgf_path, output_path;

        public Spectra spectra;

        public Pnovo_Help(string mgf_path, string output_path, Spectra spectra)
        {
            this.mgf_path = mgf_path;
            this.output_path = output_path;
            this.spectra = spectra;
        }
        public void write_MGF()
        {
            this.spectra.write_MGF(this.mgf_path);
        }
        public void write_pNovo_param_file()
        {
            string all_txt = "";
            StreamReader sr = new StreamReader(File_Help.pNovo_exe_file + "\\"  + Param_filename, Encoding.Default);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                string[] strs = line.Split('=');
                if (strs.Length < 2)
                {
                    all_txt += line + "\r\n";
                    continue;
                }
                if (strs[0] == this.mgf_path_title)
                {
                    line = strs[0] + "=" + this.mgf_path;
                }
                else if (strs[0] == this.output_path_title)
                {
                    line = strs[0] + "=" + this.output_path;
                }
                all_txt += line + "\r\n";
            }
            sr.Close();
            FileStream fs = new FileStream(Param_filename, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, Encoding.Default);
            sw.Write(all_txt);
            sw.Flush();
            sw.Close();
            fs.Close();
        }
        public void run_pnovo()
        {
            Process process = new Process();
            process.StartInfo.FileName = File_Help.pNovo_exe_file + "\\pNovo2.exe";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            process.StartInfo.Arguments = Param_filename;
            process.Start();
            process.WaitForExit();
        }
        public List<Pnovo_Result> get_results()
        {
            List<Pnovo_Result> results = new List<Pnovo_Result>();
            string mgf_name = this.mgf_path.Split('\\').Last().Split('.').First();
            string pNovo_result_path = this.output_path + "\\" + mgf_name + ".txt";
            if (!File.Exists(pNovo_result_path))
            {
                System.Windows.MessageBox.Show("pNovo2.exe has unpredictable problems.");
                return results;
            }
            StreamReader sr = new StreamReader(pNovo_result_path);
            int id = 1;
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine().Trim();
                if (line == "" || line[0] != 'P')
                    continue;
                string[] strs = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                Pnovo_Result pr = new Pnovo_Result(id++, strs[1], double.Parse(strs[2]));
                results.Add(pr);
            }
            sr.Close();
            return results;
        }
    }
    public class Pnovo_Result
    {
        public int ID { get; set; }
        public string SQ { get; set; }
        public string Modification_Str { get; set; }
        public List<Modification> Modification { get; set; }
        public double Score { get; set; }

        public Pnovo_Result(int id, string SQ, double score)
        {
            this.ID = id;
            this.SQ = SQ;
            this.Score = score;
            this.Modification = new List<Modification>();
            update();
        }

        public void update()
        {
            string modification_C = "Carbamidomethyl[C]";
            string modification_M = "Oxidation[M]";
            double[] mass1 = Config_Help.modStr_hash[modification_C] as double[];
            double[] mass2 = Config_Help.modStr_hash[modification_M] as double[];
            int aa_index = 0; //Config_Help.AA_Normal_Index
            string modification_str = "";
            for (int i = 0; i < this.SQ.Length; ++i)
            {
                if (this.SQ[i] == 'C')
                {
                    this.Modification.Add(new Modification(i + 1, mass1[aa_index], modification_C));
                    modification_str += (i + 1) + "," + modification_C + ";";
                }
                else if (this.SQ[i] == 'J')
                {
                    this.Modification.Add(new Modification(i + 1, mass2[aa_index], modification_M));
                    modification_str += (i + 1) + "," + modification_M + ";";
                }
            }
            string new_str = this.SQ.Replace('J', 'M');
            this.SQ = new_str;
            this.Modification_Str = modification_str;
        }

        public List<double> get_all_bMass()
        {
            List<double> bmass = new List<double>();
            List<double> mod_mass = new List<double>();
            int aa_index = Config_Help.AA_Normal_Index;
            for (int i = 0; i <= this.SQ.Length + 1; ++i)
                mod_mass.Add(0.0);
            for (int i = 0; i < this.Modification.Count; ++i)
            {
                int index = this.Modification[i].Index;
                mod_mass[index] = this.Modification[i].Mass;
            }
            double tmpmass = mod_mass[0];
            for (int i = 0; i < this.SQ.Length - 1; ++i)
            {
                tmpmass += mod_mass[i + 1] + Config_Help.mass_index[aa_index, this.SQ[i] - 'A'];
                bmass.Add(tmpmass + Config_Help.massZI);
            }
            return bmass;
        }

        public List<double> get_all_yMass()
        {
            List<double> ymass = new List<double>();
            List<double> mod_mass = new List<double>();
            int aa_index = Config_Help.AA_Normal_Index;
            for (int i = 0; i <= this.SQ.Length + 1; ++i)
                mod_mass.Add(0.0);
            for (int i = 0; i < this.Modification.Count; ++i)
            {
                int index = this.Modification[i].Index;
                mod_mass[index] = this.Modification[i].Mass;
            }
            double tmpmass = mod_mass.Last();
            for (int i = this.SQ.Length - 1; i > 0; --i)
            {
                tmpmass += mod_mass[i + 1] + Config_Help.mass_index[aa_index, this.SQ[i] - 'A'];
                ymass.Add(tmpmass + Config_Help.massH2O + Config_Help.massZI);
            }
            return ymass;
        }
    }
}
