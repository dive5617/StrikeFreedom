using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class PSM_Help_Parent
    {
        public static int charge_num = 3; //charge_num至少为2，因为默认最小值为2，支持b++和y++
        public const int charge_num_max = 30;
        public int[] M_Match_Flag = new int[charge_num_max * 3]; //[M],[M]-H2O,[M]-NH3
        public int[] N_Match_Flag = new int[charge_num_max * 9]; //a,a-H2O,a-NH3,b,b-H2O,b-NH3,c,c-H2O,c-NH3
        public int[] C_Match_Flag = new int[charge_num_max * 9]; //x,x-H2O,x-NH3,y,y-H2O,y-NH3,z,z-H2O,z-NH3
        public int[] I_Match_Flag = new int[charge_num_max]; //内部离子，考虑+\++\+++，默认仅考虑+1价的内部离子
        public int[] O_Match_Flag = new int[charge_num_max]; //亚氨离子

        public Spectra Spec { get; set; }
        public Peptide Pep { get; set; }
        public int Peptide_Number { get; set; }
        public int Mix_Flag { get; set; } //Mix_Flag = 0，单谱匹配，Mix_Flag = 1，混合谱或者多肽段交联
        public int cand_index { get; set; } //匹配的是第几个候选肽段 


        public double Ppm_mass_error { get; set; }
        public double Da_mass_error { get; set; }
        public PSM_Detail Psm_detail { get; set; }
        public double Match_score { get; set; }


        public virtual double Match(int isPPM) //定义一个虚函数，这样大家都可以调用它
        {
            return 0.0;
        }
        
        public virtual double Compute_Mass()
        {
            double pepmass = 0.0;
            int aa_index = this.Pep.Tag_Flag;
            for (int i = 0; i < this.Pep.Sq.Length; ++i)
            {
                pepmass += Config_Help.mass_index[aa_index, this.Pep.Sq[i] - 'A'];
            }
            //增加修饰的质量
            for (int i = 0; i < this.Pep.Mods.Count; ++i)
            {
                double[] mod_mass = Config_Help.modStr_hash[this.Pep.Mods[i].Mod_name] as double[];
                pepmass += mod_mass[this.Pep.Mods[i].Flag_Index];
            }
            pepmass += Config_Help.massH2O + Config_Help.massZI;
            return pepmass;
        }
        
        public int IsInWithPPM(double mass, int[] mass_inten, ref double mass_error) //误差窗口内最高峰
        {
            int start = (int)(mass - mass * Ppm_mass_error);
            int end = (int)(mass + mass * Ppm_mass_error);
            if (start <= 0 || end >= Config_Help.MaxMass)
                return -1;
            double maxInten = 0.0;
            int max_k = -1;
            for (int k = mass_inten[start - 1]; k < mass_inten[end]; ++k)
            {
                PEAK peak = (PEAK)(Spec.Peaks[k]);
                double tmpd = System.Math.Abs((peak.Mass - mass) / mass);
                if (tmpd <= Ppm_mass_error && peak.Intensity > maxInten)
                {
                    maxInten = peak.Intensity;
                    max_k = k;
                }
            }
            if (max_k != -1)
            {
                mass_error = (((PEAK)(Spec.Peaks[max_k])).Mass - mass) * 1.0e6 / mass; //实验质量-理论质量
            }
            return max_k;
        }
        public int IsInWithDa(double mass, int[] mass_inten, ref double mass_error) //误差窗口内最高峰
        {
            int start = (int)(mass - Da_mass_error);
            int end = (int)(mass + Da_mass_error);
            if (start <= 0 || end >= Config_Help.MaxMass)
                return -1;
            double maxInten = 0.0;
            int max_k = -1;
            for (int k = mass_inten[start - 1]; k < mass_inten[end]; ++k)
            {
                PEAK peak = (PEAK)(Spec.Peaks[k]);
                double tmpd = System.Math.Abs(peak.Mass - mass);
                if (tmpd <= Da_mass_error && peak.Intensity > maxInten)
                {
                    maxInten = peak.Intensity;
                    max_k = k;
                }
            }
            if (max_k != -1)
            {
                mass_error = ((PEAK)(Spec.Peaks[max_k])).Mass - mass;
            }
            return max_k;
        }
        
    }
}
