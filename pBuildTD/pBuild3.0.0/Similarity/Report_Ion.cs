using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild.Similarity
{
    public class Report_Ion
    {
        public PSM_Help Psm_help;

        public Report_Ion(PSM_Help psm_help)
        {
            this.Psm_help = new PSM_Help(psm_help);
        }
        public string write_file(string folder_path)
        {
            if (!Directory.Exists(folder_path))
                Directory.CreateDirectory(folder_path);
            string file_path = folder_path + "\\ion.txt";
            StreamWriter sw = new StreamWriter(file_path);
            sw.WriteLine("Ion Type\tmz\tintensity\tmz error");
            List<Ion> ions = get_Ion();
            for (int i = 0; i < ions.Count; ++i)
            {
                sw.WriteLine(ions[i].name + "\t" + ions[i].mz + "\t" + ions[i].intensity + "\t" + ions[i].mz_error);
            }
            sw.Flush();
            sw.Close();
            return file_path;
        }
        public void write_file(StreamWriter sw)
        {
            List<Ion> ions = get_Ion();
            for (int i = 0; i < ions.Count; ++i)
            {
                if (ions[i].mz_error == 0.0)
                    continue;
                sw.WriteLine(ions[i].mz_error);
            }
        }
        public List<Ion> get_Ion()
        {
            int aa_index = this.Psm_help.Pep.Tag_Flag;
            List<Ion> ions = new List<Ion>();
            int[] mass_inten = new int[Config_Help.MaxMass];
            for (int k = 0; k < this.Psm_help.Spec.Peaks.Count; ++k)
            {
                int massi = (int)this.Psm_help.Spec.Peaks[k].Mass;
                mass_inten[massi] = k + 1;
            }
            int currindex = 0;
            for (int k = 0; k < Config_Help.MaxMass; ++k)
            {
                if (mass_inten[k] == 0)
                    mass_inten[k] = currindex;
                else
                    currindex = mass_inten[k];
            }
            List<double> mod_mass = new List<double>();
            for (int i = 0; i < this.Psm_help.Pep.Sq.Length + 2; ++i)
                mod_mass.Add(0.0);
            for (int i = 0; i < this.Psm_help.Pep.Mods.Count; ++i)
            {
                mod_mass[this.Psm_help.Pep.Mods[i].Index] = this.Psm_help.Pep.Mods[i].Mass;
            }
            double mass = mod_mass[0];
            for (int i = 0; i < this.Psm_help.Pep.Sq.Length - 1; ++i)
            {
                mass += Config_Help.mass_index[aa_index, this.Psm_help.Pep.Sq[i] - 'A'] + mod_mass[i + 1];
                double bmass = (mass + Config_Help.B_Mass + Config_Help.massZI);
                double bmass2 = (mass + Config_Help.B_Mass + 2 * Config_Help.massZI) / 2;
                double me = 0.0;
                int index_k = -1;
                if (this.Psm_help.Ppm_mass_error != 0.0)
                    index_k = this.Psm_help.IsInWithPPM(bmass, mass_inten, ref me);
                else
                    index_k = this.Psm_help.IsInWithDa(bmass, mass_inten, ref me);
                if (index_k != -1)
                    ions.Add(new Ion("b" + (i + 1) + "+", bmass, this.Psm_help.Spec.Peaks[index_k].Intensity, me));
                else
                    ions.Add(new Ion("b" + (i + 1) + "+", bmass, 0.0, 0.0));
                if (this.Psm_help.Ppm_mass_error != 0.0)
                    index_k = this.Psm_help.IsInWithPPM(bmass2, mass_inten, ref me);
                else
                    index_k = this.Psm_help.IsInWithDa(bmass2, mass_inten, ref me);
                if (index_k != -1)
                    ions.Add(new Ion("b" + (i + 1) + "++", bmass2, this.Psm_help.Spec.Peaks[index_k].Intensity, me));
                else
                    ions.Add(new Ion("b" + (i + 1) + "++", bmass2, 0.0, 0.0));
            }
            mass = mod_mass[this.Psm_help.Pep.Sq.Length + 1];
            for (int i = this.Psm_help.Pep.Sq.Length - 1; i > 0; --i)
            {
                mass += Config_Help.mass_index[aa_index, this.Psm_help.Pep.Sq[i] - 'A'] + mod_mass[i + 1];
                double ymass = (mass + Config_Help.Y_Mass + Config_Help.massZI);
                double ymass2 = (mass + Config_Help.Y_Mass + 2 * Config_Help.massZI) / 2;
                double me = 0.0;
                int index_k = -1;
                if (this.Psm_help.Ppm_mass_error != 0.0)
                    index_k = this.Psm_help.IsInWithPPM(ymass, mass_inten, ref me);
                else 
                    index_k = this.Psm_help.IsInWithDa(ymass, mass_inten, ref me);
                if (index_k != -1)
                    ions.Add(new Ion("y" + (this.Psm_help.Pep.Sq.Length - i) + "+", ymass, this.Psm_help.Spec.Peaks[index_k].Intensity, me));
                else
                    ions.Add(new Ion("y" + (this.Psm_help.Pep.Sq.Length - i) + "+", ymass, 0.0, 0.0));
                if (this.Psm_help.Ppm_mass_error != 0.0)
                    index_k = this.Psm_help.IsInWithPPM(ymass2, mass_inten, ref me);
                else
                    index_k = this.Psm_help.IsInWithDa(ymass2, mass_inten, ref me);
                if (index_k != -1)
                    ions.Add(new Ion("y" + (this.Psm_help.Pep.Sq.Length - i) + "++", ymass2, this.Psm_help.Spec.Peaks[index_k].Intensity, me));
                else
                    ions.Add(new Ion("y" + (this.Psm_help.Pep.Sq.Length - i) + "++", ymass2, 0.0, 0.0));
            }
            return ions;
        }

        public class Ion
        {
            public string name;
            public double mz;
            public double intensity;
            public double mz_error;

            public Ion(string name, double mz, double intensity, double mz_error)
            {
                this.name = name;
                this.mz = mz;
                this.intensity = intensity;
                this.mz_error = mz_error;
            }
        }
    }
}
