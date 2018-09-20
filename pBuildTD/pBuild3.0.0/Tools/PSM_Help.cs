using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace pBuild
{
    public class PSM_Help : PSM_Help_Parent
    {
        //mass_error为匹配的误差,PPM的匹配，下面还需要增加类似的函数Da的匹配，为什么使用两个，这样处理更加方便，增加一个参数，反而感觉不好
        
        public List<double> Mix_spec_mass { get; set; } //显示混合谱，需要知道所有谱图的母离子的m/z
        public List<int> Mix_spec_charge { get; set; } //显示混合谱，需要知道所有谱图的母离子的电荷
        public List<Peptide> Mix_peps { get; set; } //混合谱或者是二肽、三肽的标注
        
        //public int aa_index { get; set; } //匹配的氨基酸表

        

        public PSM_Help(PSM_Help psm_help)
        {
            this.M_Match_Flag = psm_help.M_Match_Flag;
            this.N_Match_Flag = psm_help.N_Match_Flag;
            this.C_Match_Flag = psm_help.C_Match_Flag;
            this.I_Match_Flag = psm_help.I_Match_Flag;
            this.O_Match_Flag = psm_help.O_Match_Flag;
            this.Spec = psm_help.Spec;
            this.Mix_spec_mass = psm_help.Mix_spec_mass;
            this.Mix_spec_charge = psm_help.Mix_spec_charge;
            this.Pep = new Peptide(psm_help.Pep);
            this.Mix_peps = psm_help.Mix_peps;
            this.Mix_Flag = psm_help.Mix_Flag;
            this.Ppm_mass_error = psm_help.Ppm_mass_error;
            this.Da_mass_error = psm_help.Da_mass_error;
            this.Psm_detail = psm_help.Psm_detail;
            this.Match_score = psm_help.Match_score;
            this.cand_index = psm_help.cand_index;
            this.Peptide_Number = psm_help.Peptide_Number;
        }

        public PSM_Help(string HCD_ETD_type) //默认匹配误差是20ppm，到时候会根据搜索的误差参数进行设置
        {
            charge_num = 3;
            if (Config_Help.is_ppm_flag)
            {
                Ppm_mass_error = Config_Help.mass_error;
                Da_mass_error = 0;
            }
            else
            {
                Ppm_mass_error = 0;
                Da_mass_error = Config_Help.mass_error;
            }
            this.Psm_detail = new PSM_Detail();
            if (HCD_ETD_type == "HCD")
            {
                this.N_Match_Flag[3 * charge_num] = this.N_Match_Flag[3 * charge_num + 1] = 1;
                this.C_Match_Flag[3 * charge_num] = this.C_Match_Flag[3 * charge_num + 1] = 1;
            }
            else if (HCD_ETD_type == "ETD")
            {
                this.N_Match_Flag[6 * charge_num] = this.N_Match_Flag[6 * charge_num + 1] = 1;
                this.C_Match_Flag[6 * charge_num] = this.C_Match_Flag[6 * charge_num + 1] = 1;
            }
            this.M_Match_Flag[0] = this.M_Match_Flag[1] = 1;
            this.cand_index = 0;
            this.Peptide_Number = 1;
            this.Mix_Flag = 0;
            //初始化只考虑HCD，即b+\b++\y+\y++，并且加上母离子[M]+和[M]++
        }
        private void update_NC_arrray(int charge_num_old)
        {
            int[] N_Match_Flag_New = new int[9 * charge_num];
            int[] C_Match_Flag_New = new int[9 * charge_num];
            for (int i = 0; i < this.N_Match_Flag.Length; ++i)
            {
                int index = i / charge_num_old;
                int reminder = i % charge_num_old;
                int index_new = index * charge_num + reminder;
                if (this.N_Match_Flag[i] == 1)
                {
                    if (index_new < N_Match_Flag_New.Length)
                        N_Match_Flag_New[index_new] = 1;
                }
                else
                {
                    if (index_new < N_Match_Flag_New.Length)
                        N_Match_Flag_New[index_new] = 0;
                }
            }
            for (int i = 0; i < this.C_Match_Flag.Length; ++i)
            {
                int index = i / charge_num_old;
                int reminder = i % charge_num_old;
                int index_new = index * charge_num + reminder;
                if (this.C_Match_Flag[i] == 1)
                {
                    if (index_new < C_Match_Flag_New.Length)
                        C_Match_Flag_New[index_new] = 1;
                }
                else
                {
                    if (index_new < C_Match_Flag_New.Length)
                        C_Match_Flag_New[index_new] = 0;
                }
            }
            N_Match_Flag = N_Match_Flag_New;
            C_Match_Flag = C_Match_Flag_New;
        }
        public void Switch_PSM_Help(Spectra spec, Peptide pep)
        {
            this.Spec = spec;
            this.Pep = pep;
            this.Psm_detail = new PSM_Detail();
            this.cand_index = 0;
            this.Peptide_Number = 1;
            this.Mix_Flag = 0;
            if (spec.Charge > 3 && charge_num != spec.Charge - 1)
            {
                int charge_num_old = charge_num;
                charge_num = spec.Charge - 1;
                if (charge_num < 3)
                    charge_num = 3;
                update_NC_arrray(charge_num_old);
            }
        }
        public void Switch_PSM_Help(Spectra spec, List<Peptide> peps, List<double> spec_mass, List<int> spec_charge)
        {
            this.Spec = spec;
            this.Mix_peps = peps;
            this.Mix_spec_mass = spec_mass;
            this.Mix_spec_charge = spec_charge;
            this.Mix_Flag = 1;
            this.Psm_detail = new PSM_Detail();
            this.cand_index = 0;
            this.Peptide_Number = peps.Count;
            this.Mix_Flag = 1;
            if (spec.Charge > 3 && charge_num != spec.Charge - 1)
            {
                int charge_num_old = charge_num;
                charge_num = spec.Charge - 1;
                if (charge_num < 3)
                    charge_num = 3;
                update_NC_arrray(charge_num_old);
            }
        }
        private void Initial()
        {
            this.Psm_detail.Peptide_index = new List<int>[Spec.Peaks.Count];
            for (int i = 0; i < this.Psm_detail.Peptide_index.Length; ++i)
                this.Psm_detail.Peptide_index[i] = new List<int>();
            this.Psm_detail.BOry = new int[Spec.Peaks.Count];
            this.Psm_detail.Mass_error = new List<double>[Spec.Peaks.Count];
            this.Psm_detail.By_num = new List<string>[Spec.Peaks.Count];
            for (int i = 0; i < Spec.Peaks.Count; ++i)
            {
                this.Psm_detail.Mass_error[i] = new List<double>();
                this.Psm_detail.By_num[i] = new List<string>();
            }
            if (this.Mix_Flag == 0)
            {
                this.Psm_detail.B_flag = new int[Pep.Sq.Length + 1];
                this.Psm_detail.C_flag = new int[Pep.Sq.Length + 1];
                this.Psm_detail.Y_flag = new int[Pep.Sq.Length + 1];
                this.Psm_detail.Z_flag = new int[Pep.Sq.Length + 1];
            }
            else
            {
                this.Psm_detail.B_flag_mix = new List<List<int>>();
                this.Psm_detail.C_flag_mix = new List<List<int>>();
                this.Psm_detail.Y_flag_mix = new List<List<int>>();
                this.Psm_detail.Z_flag_mix = new List<List<int>>();
                for(int i=0;i<this.Mix_peps.Count;++i)
                {
                    this.Psm_detail.B_flag_mix.Add(new List<int>());
                    this.Psm_detail.C_flag_mix.Add(new List<int>());
                    this.Psm_detail.Y_flag_mix.Add(new List<int>());
                    this.Psm_detail.Z_flag_mix.Add(new List<int>());
                    for (int j = 0; j < this.Mix_peps[i].Sq.Length + 1; ++j)
                    {
                        this.Psm_detail.B_flag_mix[i].Add(0);
                        this.Psm_detail.C_flag_mix[i].Add(0);
                        this.Psm_detail.Y_flag_mix[i].Add(0);
                        this.Psm_detail.Z_flag_mix[i].Add(0);
                    }
                }
            }
            this.Psm_detail.N_all_matches = new List<PSM_Match>();
            this.Psm_detail.C_all_matches = new List<PSM_Match>();
            for (int i = 0; i < N_Match_Flag.Length; ++i)
            {
                Psm_detail.N_all_matches.Add(new PSM_Match(this.Peptide_Number));
            }
            for (int i = 0; i < C_Match_Flag.Length; ++i)
            {
                Psm_detail.C_all_matches.Add(new PSM_Match(this.Peptide_Number));
            }
        }
        private List<List<double>> copy_list(List<List<double>> list)
        {
            List<List<double>> result = new List<List<double>>();
            for (int i = 0; i < list.Count; ++i)
            {
                result.Add(new List<double>(list[i]));
            }
            return result;
        }
        
        public Similarity.N_C_Term_Intensity Match_SIM()
        {
            Similarity.N_C_Term_Intensity nc_inten = new Similarity.N_C_Term_Intensity(this.Spec.Title, this.Pep.Tag_Flag, this.Spec.Charge, this.Pep.Sq);
            Match_BY();
            int b_index = Psm_detail.getNIndex("b+");
            int b2_index = Psm_detail.getNIndex("b++");
            int y_index = Psm_detail.getCIndex("y+");
            int y2_index = Psm_detail.getCIndex("y++");
            PSM_Match b_match = Psm_detail.N_all_matches[b_index];
            PSM_Match b2_match = Psm_detail.N_all_matches[b2_index];
            PSM_Match y_match = Psm_detail.C_all_matches[y_index];
            PSM_Match y2_match = Psm_detail.C_all_matches[y2_index];
            for(int i=0;i<b_match.fragment_theory_mass[0].Count;++i)
            {
                nc_inten.N_mass.Add(b_match.fragment_theory_mass_double[0][i]);
                nc_inten.N_intensity.Add(b_match.fragment_match_intensity[0][i]);
            }
            for(int i=0;i<b2_match.fragment_theory_mass[0].Count;++i)
            {
                nc_inten.N_mass.Add(b2_match.fragment_theory_mass_double[0][i]);
                nc_inten.N_intensity.Add(b2_match.fragment_match_intensity[0][i]);
            }
            for(int i=0;i<y_match.fragment_theory_mass[0].Count;++i)
            {
                nc_inten.C_mass.Add(y_match.fragment_theory_mass_double[0][i]);
                nc_inten.C_intensity.Add(y_match.fragment_match_intensity[0][i]);
            }
            for(int i=0;i<y2_match.fragment_theory_mass[0].Count;++i)
            {
                nc_inten.C_mass.Add(y2_match.fragment_theory_mass_double[0][i]);
                nc_inten.C_intensity.Add(y2_match.fragment_match_intensity[0][i]);
            }
            return nc_inten;
        }
        public void Match_BY()
        {
            int b_index = 0, b2_index = 1, y_index = 0, y2_index = 1;
            this.Psm_detail.N_all_matches.Add(new PSM_Match("b+", 1));
            this.Psm_detail.N_all_matches.Add(new PSM_Match("b++", 1));
            this.Psm_detail.C_all_matches.Add(new PSM_Match("y+", 1));
            this.Psm_detail.C_all_matches.Add(new PSM_Match("y++", 1));
            this.Psm_detail.BOry = new int[Spec.Peaks.Count];
            this.Psm_detail.Mass_error = new List<double>[Spec.Peaks.Count];
            this.Psm_detail.By_num = new List<string>[Spec.Peaks.Count];
            for(int i=0;i<Spec.Peaks.Count;++i)
            {
                this.Psm_detail.Mass_error[i] = new List<double>();
                this.Psm_detail.By_num[i] = new List<string>();
            }

            int isPPM = 0;
            if (this.Ppm_mass_error != 0)
                isPPM = 1;
            int aa_index = this.Pep.Tag_Flag;
            int[] mass_inten = new int[Config_Help.MaxMass];   // modified by wrm
            for (int k = 0; k < Spec.Peaks.Count; ++k)
            {
                int massi = (int)Spec.Peaks[k].Mass;
                mass_inten[massi] = k + 1;
            }
            int currindex = 0;
            for (int k = 0; k < Config_Help.MaxMass; ++k)  // modified by wrm
            {
                if (mass_inten[k] == 0)
                    mass_inten[k] = currindex;
                else
                    currindex = mass_inten[k];
            }
            double mass = 0;
            List<double> mod_mass = new List<double>();
            for (int k = 0; k < Pep.Sq.Length + 2; ++k)
            {
                mod_mass.Add(0.0);
            }
            for (int k = 0; k < Pep.Mods.Count; ++k)
            {
                mod_mass[Pep.Mods[k].Index] = Pep.Mods[k].Mass;  
            }
            mass = mod_mass[0];
            for (int i = 0; i < Pep.Sq.Length - 1; ++i)
            {
                mass += Config_Help.mass_index[aa_index, Pep.Sq[i] - 'A'] + mod_mass[i + 1];
                double match_mass = mass + Config_Help.B_Mass + Config_Help.massZI;
                double match_mass2 = (match_mass + Config_Help.massZI) / 2;
                Psm_detail.N_all_matches[b_index].fragment_theory_mass[0].Add(match_mass.ToString("F4"));
                Psm_detail.N_all_matches[b_index].fragment_theory_mass_double[0].Add(match_mass);
                int k_index = -1;
                double me = 0.0;
                if (isPPM == 1)
                    k_index = IsInWithPPM(match_mass, mass_inten, ref me);
                else
                    k_index = IsInWithDa(match_mass, mass_inten, ref me);
                if (k_index != -1)
                {
                    Psm_detail.N_all_matches[b_index].fragment_match_intensity[0].Add(Spec.Peaks[k_index].Intensity);
                    Psm_detail.N_all_matches[b_index].fragment_match_flag[0].Add(1);
                    Psm_detail.BOry[k_index] |= 1;
                    Psm_detail.Mass_error[k_index].Add(me);
                    Psm_detail.By_num[k_index].Add("b" + (i + 1) + "+");
                    if (i == 1) //b2->a2
                    {
                        double a2_match_mass = match_mass - Config_Help.B_Mass + Config_Help.A_Mass;
                        int k_index2 = -1;
                        double me2 = 0.0;
                        if (isPPM == 1)
                            k_index2 = IsInWithPPM(a2_match_mass, mass_inten, ref me2);
                        else
                            k_index2 = IsInWithDa(a2_match_mass, mass_inten, ref me2);
                        Psm_detail.BOry[k_index2] |= 1;
                        Psm_detail.Mass_error[k_index2].Add(me2);
                        Psm_detail.By_num[k_index2].Add("a2+");
                    }
                }
                else
                {
                    Psm_detail.N_all_matches[b_index].fragment_match_intensity[0].Add(0.0);
                    Psm_detail.N_all_matches[b_index].fragment_match_flag[0].Add(0);
                }
                Psm_detail.N_all_matches[b2_index].fragment_theory_mass[0].Add(match_mass2.ToString("F4"));
                Psm_detail.N_all_matches[b2_index].fragment_theory_mass_double[0].Add(match_mass2);
                if (isPPM == 1)
                    k_index = IsInWithPPM(match_mass2, mass_inten, ref me);
                else
                    k_index = IsInWithDa(match_mass2, mass_inten, ref me);
                if (k_index != -1)
                {
                    Psm_detail.N_all_matches[b2_index].fragment_match_intensity[0].Add(Spec.Peaks[k_index].Intensity);
                    Psm_detail.N_all_matches[b2_index].fragment_match_flag[0].Add(1);
                    Psm_detail.BOry[k_index] |= 1;
                    Psm_detail.Mass_error[k_index].Add(me);
                    Psm_detail.By_num[k_index].Add("b" + (i + 1) + "++");
                }
                else
                {
                    Psm_detail.N_all_matches[b2_index].fragment_match_intensity[0].Add(0.0);
                    Psm_detail.N_all_matches[b2_index].fragment_match_flag[0].Add(0);
                }
            }
            mass = mod_mass[this.Pep.Sq.Length + 1];
            for (int i = this.Pep.Sq.Length - 1; i > 0; --i)
            {
                mass += Config_Help.mass_index[aa_index, this.Pep.Sq[i] - 'A'] + mod_mass[i + 1];
                double match_mass = mass + Config_Help.Y_Mass + Config_Help.massZI;
                double match_mass2 = (match_mass + Config_Help.massZI) / 2;
                Psm_detail.C_all_matches[y_index].fragment_theory_mass[0].Add(match_mass.ToString("F4"));
                Psm_detail.C_all_matches[y_index].fragment_theory_mass_double[0].Add(match_mass);
                int k_index = -1;
                double me = 0.0;
                if (isPPM == 1)
                    k_index = IsInWithPPM(match_mass, mass_inten, ref me);
                else
                    k_index = IsInWithDa(match_mass, mass_inten, ref me);
                if (k_index != -1)
                {
                    Psm_detail.C_all_matches[y_index].fragment_match_intensity[0].Add(Spec.Peaks[k_index].Intensity);
                    Psm_detail.C_all_matches[y_index].fragment_match_flag[0].Add(1);
                    Psm_detail.BOry[k_index] |= 2;
                    Psm_detail.Mass_error[k_index].Add(me);
                    Psm_detail.By_num[k_index].Add("y" + (this.Pep.Sq.Length - i) + "+");
                }
                else
                {
                    Psm_detail.C_all_matches[y_index].fragment_match_intensity[0].Add(0.0);
                    Psm_detail.C_all_matches[y_index].fragment_match_flag[0].Add(0);
                }
                Psm_detail.C_all_matches[y2_index].fragment_theory_mass[0].Add(match_mass2.ToString("F4"));
                Psm_detail.C_all_matches[y2_index].fragment_theory_mass_double[0].Add(match_mass2);
                if (isPPM == 1)
                    k_index = IsInWithPPM(match_mass2, mass_inten, ref me);
                else
                    k_index = IsInWithDa(match_mass2, mass_inten, ref me);
                if (k_index != -1)
                {
                    Psm_detail.C_all_matches[y2_index].fragment_match_intensity[0].Add(Spec.Peaks[k_index].Intensity);
                    Psm_detail.C_all_matches[y2_index].fragment_match_flag[0].Add(1);
                    Psm_detail.BOry[k_index] |= 2;
                    Psm_detail.Mass_error[k_index].Add(me);
                    Psm_detail.By_num[k_index].Add("y" + (this.Pep.Sq.Length - i) + "++");
                }
                else
                {
                    Psm_detail.C_all_matches[y2_index].fragment_match_intensity[0].Add(0.0);
                    Psm_detail.C_all_matches[y2_index].fragment_match_flag[0].Add(0);
                }
            }
        }
        //返回匹配率，匹配的强度/总强度
        public double Match(int isPPM, int pep_index = 0) //isPPM=1 表示PPM匹配，isPPM=0 表示Da匹配，aa_index表示第几组氨基酸质量表,pep_index表示混合谱中的第几个肽段
        {
            Pep.update();
            int aa_index = this.Pep.Tag_Flag;
            if (this.Mix_Flag == 0)
                Initial();
            //[wrm] TD数据质量比较大，暂时以10万为上界
            int[] mass_inten = new int[Config_Help.MaxMass];   
            for (int k = 0; k < Spec.Peaks.Count; ++k)
            {
                int massi = (int)Spec.Peaks[k].Mass;
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
            
            double mass = 0;
            List<double> mod_mass = new List<double>();
            List<double> mod_loss_mass0 = new List<double>();
            List<List<double>> mod_loss_mass = new List<List<double>>();
            List<List<double>> mod_loss_mass_b = new List<List<double>>();
            List<List<double>> mod_loss_mass_y = new List<List<double>>();
            mod_loss_mass0.Add(0.0);
            for (int k = 0; k < Pep.Sq.Length + 2; ++k)
            {
                mod_mass.Add(0.0);
                List<double> ls0 = new List<double>();
                ls0.Add(0.0);
                mod_loss_mass.Add(ls0);
                ls0 = new List<double>();
                ls0.Add(0.0);
                mod_loss_mass_b.Add(ls0);
                ls0 = new List<double>();
                ls0.Add(0.0);
                mod_loss_mass_y.Add(ls0);
            }
            for (int k = 0; k < Pep.Mods.Count; ++k)
            {
                mod_mass[Pep.Mods[k].Index] = Pep.Mods[k].Mass;
                for (int p = 0; p < Pep.Mods[k].Mass_Loss.Count; ++p)
                {
                    mod_loss_mass[Pep.Mods[k].Index].Add(Pep.Mods[k].Mass_Loss[p]);
                    if (!mod_loss_mass0.Contains(Pep.Mods[k].Mass_Loss[p]))
                        mod_loss_mass0.Add(Pep.Mods[k].Mass_Loss[p]);
                }
            }
            //mod_loss_mass_b = copy_list(mod_loss_mass);
            //mod_loss_mass_y = copy_list(mod_loss_mass);
            for (int k = 0; k < Pep.Sq.Length + 2; ++k)
            {
                for (int p = 0; p < mod_loss_mass[k].Count; ++p)
                {
                    if (mod_loss_mass[k][p] != 0.0)
                    {
                        for (int q = k; q < Pep.Sq.Length + 2; ++q)
                        {
                            int count = mod_loss_mass_b[q].Count;
                            for (int qq = 0; qq < count; ++qq)
                            {
                                if (!mod_loss_mass_b[q].Contains(mod_loss_mass_b[q][qq] + mod_loss_mass[k][p]))
                                    mod_loss_mass_b[q].Add(mod_loss_mass_b[q][qq] + mod_loss_mass[k][p]);
                            }
                        }
                    }
                }
            }
            for (int k = Pep.Sq.Length + 1; k >= 0; --k)
            {
                for (int p = 0; p < mod_loss_mass[k].Count; ++p)
                {
                    if (mod_loss_mass[k][p] != 0.0)
                    {
                        for (int q = k; q >= 0; --q)
                        {
                            int count = mod_loss_mass_y[q].Count;
                            for (int qq = 0; qq < count; ++qq)
                            {
                                if (!mod_loss_mass_y[q].Contains(mod_loss_mass_y[q][qq] + mod_loss_mass[k][p]))
                                    mod_loss_mass_y[q].Add(mod_loss_mass_y[q][qq] + mod_loss_mass[k][p]);
                            }
                        }
                    }
                }
            }
            //匹配母离子
            if (this.Pep.Sq != "") //如果序列是空的话，那么不需要匹配母离子 
            {
                for (int k = 0; k < M_Match_Flag.Length; ++k)
                {
                    for (int q = 0; q < mod_loss_mass0.Count; ++q)
                    {
                        if (M_Match_Flag[k] == 0)
                            continue;
                        int match_type = k / charge_num;
                        int match_type_charge = (k % charge_num) + 1;
                        string type_str = "";
                        string charge_str = "";
                        double tmp_mass = 0.0;
                        switch (match_type)
                        {
                            case 0:
                                type_str = "[M]";
                                tmp_mass = 0.0;
                                break;
                            case 1:
                                type_str = "[M]-H2O";
                                tmp_mass = -Config_Help.massH2O;
                                break;
                            case 2:
                                type_str = "[M]-NH3";
                                tmp_mass = -Config_Help.massNH3;
                                break;
                        }
                        if (mod_loss_mass0[q] != 0.0)
                            type_str += "-neutral loss ";
                        for (int j = 0; j < match_type_charge; ++j)
                        {
                            charge_str += "+";
                        }
                        double pepmass = 0.0;
                        if (this.Mix_Flag == 0 || this.Mix_spec_mass == null || this.Mix_spec_mass.Count == 0)
                            pepmass = Pep.Pepmass;
                        else
                        {
                            for (int p = 0; p < Mix_peps.Count; ++p)
                                pepmass += Mix_peps[p].Pepmass;
                        }
                        double M_match_mass = (pepmass + tmp_mass - mod_loss_mass0[q] + match_type_charge * Config_Help.massZI) / match_type_charge;
                        double mass_error = 0.0;
                        int k_index = -1;
                        if (isPPM == 1)
                            k_index = IsInWithPPM(M_match_mass, mass_inten, ref mass_error);
                        else
                            k_index = IsInWithDa(M_match_mass, mass_inten, ref mass_error);
                        if (k_index != -1)
                        {
                            this.Psm_detail.Peptide_index[k_index].Add(pep_index);
                            this.Psm_detail.BOry[k_index] |= 64;
                            if (Mix_Flag == 0)
                                this.Psm_detail.By_num[k_index].Add(type_str + charge_str);
                            else
                                this.Psm_detail.By_num[k_index].Add(type_str + charge_str + "|" + pep_index);
                            this.Psm_detail.Mass_error[k_index].Add(mass_error);
                        }
                    }
                }
            }
            //默认匹配a2离子
            if (Pep.Sq != "" && this.N_Match_Flag[0] == 0) //表示没选择a+离子进行标注，那么会默认标注a2离子
            {
                mass = mod_mass[0] + mod_mass[1] + mod_mass[2] + Config_Help.mass_index[aa_index, Pep.Sq[0] - 'A'] +
                    Config_Help.mass_index[aa_index, Pep.Sq[1] - 'A'] + Config_Help.A_Mass;
                double match_mass = (mass + Config_Help.massZI); //a2+离子的m/z
                double mass_error = 0.0;
                int k_index = -1;
                if (isPPM == 1)
                    k_index = IsInWithPPM(match_mass, mass_inten, ref mass_error);
                else
                    k_index = IsInWithDa(match_mass, mass_inten, ref mass_error);
                if (k_index != -1)
                {
                    this.Psm_detail.Peptide_index[k_index].Add(pep_index);
                    this.Psm_detail.BOry[k_index] |= 1;
                    if (Mix_Flag == 0)
                        this.Psm_detail.By_num[k_index].Add("a2+");
                    else
                        this.Psm_detail.By_num[k_index].Add("a2+|" + pep_index);
                    this.Psm_detail.Mass_error[k_index].Add(mass_error);
                }
            }
            for (int l = 0; l < mod_loss_mass_b[0].Count; ++l)
            {
                mass = mod_mass[0] - mod_loss_mass_b[0][l];
                for (int k = 0; k < Pep.Sq.Length - 1; ++k)
                {
                    mass += Config_Help.mass_index[aa_index, Pep.Sq[k] - 'A'] + mod_mass[k + 1];
                    for (int ll = 0; ll < mod_loss_mass_b[k + 1].Count; ++ll)
                    {
                        double new_mass = mass - mod_loss_mass_b[k + 1][ll];
                        for (int p = 0; p < N_Match_Flag.Length; ++p)
                        {
                            if (N_Match_Flag[p] == 1)
                            {
                                double match_mass = 0.0;
                                int match_type = p / charge_num;
                                int match_type_charge = (p % charge_num) + 1;
                                string str_type = "";
                                string str_type_last = "";
                                string str_type_charge = "";
                                for (int i = 0; i < match_type_charge; ++i)
                                    str_type_charge += "+";
                                double mass_error = 0;
                                switch (match_type)
                                {
                                    case 0: //a
                                        match_mass = (new_mass + Config_Help.A_Mass + match_type_charge * Config_Help.massZI) / match_type_charge;
                                        str_type = "a";
                                        str_type_last = "";
                                        break;
                                    case 1: //a-H2O
                                        match_mass = (new_mass + Config_Help.A_Mass - Config_Help.massH2O + match_type_charge * Config_Help.massZI) / match_type_charge;
                                        str_type = "a";
                                        str_type_last = "-H2O";
                                        break;
                                    case 2: //a-NH3
                                        match_mass = (new_mass + Config_Help.A_Mass - Config_Help.massNH3 + match_type_charge * Config_Help.massZI) / match_type_charge;
                                        str_type = "a";
                                        str_type_last = "-NH3";
                                        break;
                                    case 3: //b
                                        match_mass = (new_mass + Config_Help.B_Mass + match_type_charge * Config_Help.massZI) / match_type_charge;
                                        str_type = "b";
                                        str_type_last = "";
                                        break;
                                    case 4: //b-H2O
                                        match_mass = (new_mass + Config_Help.B_Mass - Config_Help.massH2O + match_type_charge * Config_Help.massZI) / match_type_charge;
                                        str_type = "b";
                                        str_type_last = "-H2O";
                                        break;
                                    case 5: //b-NH3
                                        match_mass = (new_mass + Config_Help.B_Mass - Config_Help.massNH3 + match_type_charge * Config_Help.massZI) / match_type_charge;
                                        str_type = "b";
                                        str_type_last = "-NH3";
                                        break;
                                    case 6: //c
                                        match_mass = (new_mass + Config_Help.C_Mass + match_type_charge * Config_Help.massZI) / match_type_charge;
                                        str_type = "c";
                                        str_type_last = "";
                                        break;
                                    case 7: //c-H2O
                                        match_mass = (new_mass + Config_Help.C_Mass - Config_Help.massH2O + match_type_charge * Config_Help.massZI) / match_type_charge;
                                        str_type = "c";
                                        str_type_last = "-H2O";
                                        break;
                                    case 8: //c-NH3
                                        match_mass = (new_mass + Config_Help.C_Mass - Config_Help.massNH3 + match_type_charge * Config_Help.massZI) / match_type_charge;
                                        str_type = "c";
                                        str_type_last = "-NH3";
                                        break;
                                }
                                if (mod_loss_mass_b[k + 1][ll] != 0.0 || mod_loss_mass_b[0][l] != 0.0)
                                {
                                    str_type = "* " + str_type;
                                }
                                if (mod_loss_mass_b[k + 1][ll] == 0.0 && mod_loss_mass_b[0][l] == 0.0)
                                {
                                    Psm_detail.N_all_matches[p].fragment_type = str_type + str_type_last + str_type_charge;
                                    Psm_detail.N_all_matches[p].fragment_theory_mass[pep_index].Add(match_mass.ToString("f4"));
                                    Psm_detail.N_all_matches[p].fragment_theory_mass_double[pep_index].Add(match_mass);
                                }
                                int k_index = -1;
                                if (isPPM == 1)
                                    k_index = IsInWithPPM(match_mass, mass_inten, ref mass_error);
                                else
                                    k_index = IsInWithDa(match_mass, mass_inten, ref mass_error);
                                if (k_index != -1)
                                {
                                    this.Psm_detail.Peptide_index[k_index].Add(pep_index);
                                    this.Psm_detail.BOry[k_index] |= 1;
                                    if (Mix_Flag == 0)
                                        this.Psm_detail.By_num[k_index].Add(str_type + (k + 1) + str_type_last + str_type_charge);
                                    else
                                        this.Psm_detail.By_num[k_index].Add(str_type + (k + 1) + str_type_last + str_type_charge + "|" + pep_index);
                                    this.Psm_detail.Mass_error[k_index].Add(mass_error);
                                    if (match_type == 3) //匹配到b离子，那么会在对应的阶梯中显示N端匹配
                                    {
                                        if (this.Mix_Flag == 0)
                                            this.Psm_detail.B_flag[k + 1] = 1;
                                        else
                                            this.Psm_detail.B_flag_mix[pep_index][k + 1] = 1;
                                    }
                                    else if (match_type == 6)
                                    {
                                        if (this.Mix_Flag == 0)
                                            this.Psm_detail.C_flag[k + 1] = 1;
                                        else
                                            this.Psm_detail.C_flag_mix[pep_index][k + 1] = 1;
                                    }
                                    if (mod_loss_mass_b[k + 1][ll] == 0.0 && mod_loss_mass_b[0][l] == 0.0)
                                    { 
                                        Psm_detail.N_all_matches[p].fragment_match_flag[pep_index].Add(1);
                                        Psm_detail.N_all_matches[p].fragment_match_intensity[pep_index].Add(this.Spec.Peaks[k_index].Intensity);
                                    }
                                }
                                else
                                {
                                    if (mod_loss_mass_b[k + 1][ll] == 0.0 && mod_loss_mass_b[0][l] == 0.0)
                                    {
                                        Psm_detail.N_all_matches[p].fragment_match_flag[pep_index].Add(0);
                                        Psm_detail.N_all_matches[p].fragment_match_intensity[pep_index].Add(0.0);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            for (int l = 0; l < mod_loss_mass_y[Pep.Sq.Length + 1].Count; ++l)
            {
                mass = mod_mass[Pep.Sq.Length + 1] - mod_loss_mass_y[Pep.Sq.Length + 1][l];
                for (int k = Pep.Sq.Length - 1; k > 0; --k)
                {
                    mass += Config_Help.mass_index[aa_index, Pep.Sq[k] - 'A'] + mod_mass[k + 1];
                    for (int ll = 0; ll < mod_loss_mass_y[k + 1].Count; ++ll)
                    {
                        double new_mass = mass - mod_loss_mass_y[k + 1][ll];
                        for (int p = 0; p < C_Match_Flag.Length; ++p)
                        {
                            if (C_Match_Flag[p] == 1)
                            {
                                double match_mass = 0.0;
                                int match_type = p / charge_num;
                                int match_type_charge = (p % charge_num) + 1;
                                string str_type = "";
                                string str_type_last = "";
                                string str_type_charge = "";
                                for (int i = 0; i < match_type_charge; ++i)
                                    str_type_charge += "+";
                                double mass_error = 0;
                                switch (match_type)
                                {
                                    case 0: //x
                                        match_mass = (new_mass + Config_Help.X_Mass + match_type_charge * Config_Help.massZI) / match_type_charge;
                                        str_type = "x";
                                        str_type_last = "";
                                        break;
                                    case 1: //x-H2O
                                        match_mass = (new_mass + Config_Help.X_Mass - Config_Help.massH2O + match_type_charge * Config_Help.massZI) / match_type_charge;
                                        str_type = "x";
                                        str_type_last = "-H2O";
                                        break;
                                    case 2: //x-NH3
                                        match_mass = (new_mass + Config_Help.X_Mass - Config_Help.massNH3 + match_type_charge * Config_Help.massZI) / match_type_charge;
                                        str_type = "x";
                                        str_type_last = "-NH3";
                                        break;
                                    case 3: //y
                                        match_mass = (new_mass + Config_Help.Y_Mass + match_type_charge * Config_Help.massZI) / match_type_charge;
                                        str_type = "y";
                                        str_type_last = "";
                                        break;
                                    case 4: //y-H2O
                                        match_mass = (new_mass + Config_Help.Y_Mass - Config_Help.massH2O + match_type_charge * Config_Help.massZI) / match_type_charge;
                                        str_type = "y";
                                        str_type_last = "-H2O";
                                        break;
                                    case 5: //y-NH3
                                        match_mass = (new_mass + Config_Help.Y_Mass - Config_Help.massNH3 + match_type_charge * Config_Help.massZI) / match_type_charge;
                                        str_type = "y";
                                        str_type_last = "-NH3";
                                        break;
                                    case 6: //z
                                        match_mass = (new_mass + Config_Help.Z_Mass + match_type_charge * Config_Help.massZI) / match_type_charge;
                                        str_type = "z";
                                        str_type_last = "";
                                        break;
                                    case 7: //z-H2O
                                        match_mass = (new_mass + Config_Help.Z_Mass - Config_Help.massH2O + match_type_charge * Config_Help.massZI) / match_type_charge;
                                        str_type = "z";
                                        str_type_last = "-H2O";
                                        break;
                                    case 8: //z-NH3
                                        match_mass = (new_mass + Config_Help.Z_Mass - Config_Help.massNH3 + match_type_charge * Config_Help.massZI) / match_type_charge;
                                        str_type = "z";
                                        str_type_last = "-NH3";
                                        break;
                                }
                                if (mod_loss_mass_y[k + 1][ll] != 0.0 || mod_loss_mass_y[Pep.Sq.Length + 1][l] != 0.0)
                                {
                                    str_type = "* " + str_type;
                                }
                                if (mod_loss_mass_y[k + 1][ll] == 0.0 && mod_loss_mass_y[Pep.Sq.Length + 1][l] == 0.0)
                                {
                                    Psm_detail.C_all_matches[p].fragment_type = str_type + str_type_last + str_type_charge;
                                    Psm_detail.C_all_matches[p].fragment_theory_mass[pep_index].Add(match_mass.ToString("f4"));
                                    Psm_detail.C_all_matches[p].fragment_theory_mass_double[pep_index].Add(match_mass);
                                }
                                int k_index = -1;
                                if (isPPM == 1)
                                    k_index = IsInWithPPM(match_mass, mass_inten, ref mass_error);
                                else
                                    k_index = IsInWithDa(match_mass, mass_inten, ref mass_error);
                                if (k_index != -1)
                                {
                                    this.Psm_detail.Peptide_index[k_index].Add(pep_index);
                                    this.Psm_detail.BOry[k_index] |= 2;
                                    if (Mix_Flag == 0)
                                        this.Psm_detail.By_num[k_index].Add(str_type + (Pep.Sq.Length - k) + str_type_last + str_type_charge);
                                    else
                                        this.Psm_detail.By_num[k_index].Add(str_type + (Pep.Sq.Length - k) + str_type_last + str_type_charge + "|" + pep_index);
                                    this.Psm_detail.Mass_error[k_index].Add(mass_error);
                                    if (match_type == 3)
                                    {
                                        if (this.Mix_Flag == 0)
                                            this.Psm_detail.Y_flag[Pep.Sq.Length - k] = 1;
                                        else
                                            this.Psm_detail.Y_flag_mix[pep_index][Pep.Sq.Length - k] = 1;
                                    }
                                    else if (match_type == 6)
                                    {
                                        if (this.Mix_Flag == 0)
                                            this.Psm_detail.Z_flag[Pep.Sq.Length - k] = 1;
                                        else
                                            this.Psm_detail.Z_flag_mix[pep_index][Pep.Sq.Length - k] = 1;
                                    }
                                    if (mod_loss_mass_y[k + 1][ll] == 0.0 && mod_loss_mass_y[Pep.Sq.Length + 1][l] == 0.0)
                                    {
                                        Psm_detail.C_all_matches[p].fragment_match_flag[pep_index].Add(1);
                                        Psm_detail.C_all_matches[p].fragment_match_intensity[pep_index].Add(this.Spec.Peaks[k_index].Intensity);
                                    }
                                }
                                else
                                {
                                    if (mod_loss_mass_y[k + 1][ll] == 0.0 && mod_loss_mass_y[Pep.Sq.Length + 1][l] == 0.0)
                                    {
                                        Psm_detail.C_all_matches[p].fragment_match_flag[pep_index].Add(0);
                                        Psm_detail.C_all_matches[p].fragment_match_intensity[pep_index].Add(0.0);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //匹配内部离子
            bool is_internal_flag = false;
            for (int i = 0; i < I_Match_Flag.Length; ++i)
            {
                if (I_Match_Flag[i] == 1)
                {
                    is_internal_flag = true;
                    break;
                }
            }
            //psm_detail.N_all_matches[9]为b+的质量
            //if (Psm_detail.N_all_matches[9].fragment_theory_mass_double.Count == 0)
            //    MessageBox.Show("b+ must be selected!");
            //else
            //{
            if (is_internal_flag)
            {
                for (int p = 0; p < Pep.Sq.Length - 1; ++p)
                {
                    for (int q = p + 1; q < Pep.Sq.Length - 1; ++q)
                    {
                        double match_mass = Psm_detail.N_all_matches[9].fragment_theory_mass_double[pep_index][q] - Psm_detail.N_all_matches[9].fragment_theory_mass_double[pep_index][p];
                        for (int k = 0; k < I_Match_Flag.Length; ++k)
                        {
                            if (I_Match_Flag[k] == 1)
                            {
                                match_mass += (k + 1) * Config_Help.massZI;
                                match_mass /= (k + 1);
                                double mass_error = 0.0;
                                int k_index = -1;
                                if (isPPM == 1)
                                    k_index = IsInWithPPM(match_mass, mass_inten, ref mass_error);
                                else
                                    k_index = IsInWithDa(match_mass, mass_inten, ref mass_error);
                                if (k_index != -1)
                                {
                                    this.Psm_detail.Peptide_index[k_index].Add(pep_index);
                                    this.Psm_detail.BOry[k_index] |= 128;
                                    string charge_type = "";
                                    for (int c = 0; c <= k; ++c)
                                    {
                                        charge_type += "+";
                                    }
                                    if (this.Mix_Flag == 0)
                                        this.Psm_detail.By_num[k_index].Add("[I]" + (p + 2) + "-" + (q + 1) + charge_type);
                                    else
                                        this.Psm_detail.By_num[k_index].Add("[I]" + (p + 2) + "-" + (q + 1) + charge_type + "|" + pep_index);
                                    this.Psm_detail.Mass_error[k_index].Add(mass_error);
                                }
                            }
                        }
                    }
                }
            }
            bool is_immon_flag = false;
            for (int i = 0; i < O_Match_Flag.Length; ++i)
            {
                if (O_Match_Flag[i] == 1)
                {
                    is_immon_flag = true;
                    break;
                }
            }
            if (is_immon_flag)
            {
                for (int i = 0; i < this.Pep.Sq.Length; ++i)
                {
                    double aa_mass = Config_Help.mass_index[aa_index, Pep.Sq[i] - 'A'] + Config_Help.A_Mass;
                    for (int j = 0; j < O_Match_Flag.Length; ++j)
                    {
                        if (O_Match_Flag[j] == 1)
                        {
                            double match_mass = (aa_mass + Config_Help.massZI * (j + 1)) / (j + 1);
                            double mass_error = 0.0;
                            int k_index = -1;
                            if (isPPM == 1)
                                k_index = IsInWithPPM(match_mass, mass_inten, ref mass_error);
                            else
                                k_index = IsInWithDa(match_mass, mass_inten, ref mass_error);
                            if (k_index != -1)
                            {
                                this.Psm_detail.Peptide_index[k_index].Add(pep_index);
                                this.Psm_detail.BOry[k_index] |= 256;
                                string charge_type = "";
                                for (int c = 0; c <= j; ++c)
                                {
                                    charge_type += "+";
                                }
                                if (this.Mix_Flag == 0)
                                    this.Psm_detail.By_num[k_index].Add("[O]" + Pep.Sq[i] + charge_type);
                                else
                                    this.Psm_detail.By_num[k_index].Add("[O]" + Pep.Sq[i] + charge_type + "|" + pep_index);
                                this.Psm_detail.Mass_error[k_index].Add(mass_error);
                            }
                        }
                    }
                }
            }
            double fz = 0.0, fm = 0.0;
            for (int k = 0; k < Spec.Peaks.Count; ++k)
            {
                fm += Spec.Peaks[k].Intensity;
                if (this.Psm_detail.BOry[k] != 0)
                    fz += Spec.Peaks[k].Intensity;
            }
            Match_score = fz / fm;
            return Match_score;
        }
        public List<PEAK> Match3(MS2_Quant_Help2 ms2_quant_help2)
        {
            List<PEAK> peaks = new List<PEAK>();
            // [wrm] TD的质量范围比较大，暂定100000为上界
            int[] mass_inten = new int[Config_Help.MaxMass];
            for (int k = 0; k < Spec.Peaks.Count; ++k)
            {
                int massi = (int)Spec.Peaks[k].Mass;
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
            for (int i = 0; i < ms2_quant_help2.Masses.Count; ++i)
            {
                int index = -1;
                double mass_error = 0.0;
                if (ms2_quant_help2.Mass_error_flags[i] == 0) //PPM
                {
                    this.Ppm_mass_error = ms2_quant_help2.Mass_errors[i];
                    index = IsInWithPPM(ms2_quant_help2.Masses[i], mass_inten, ref mass_error);
                }
                else //Da
                {
                    this.Da_mass_error = ms2_quant_help2.Mass_errors[i];
                    index = IsInWithDa(ms2_quant_help2.Masses[i], mass_inten, ref mass_error);
                }
                if (index != -1)
                    peaks.Add(Spec.Peaks[index]);
                else
                    peaks.Add(new PEAK(ms2_quant_help2.Masses[i], 0.0));
            }
            return peaks;
        }
        //支持二级谱定量，根据谱图及肽段，以及N、C端的质量偏差，可以找到by离子匹配的峰，返回两种的匹配谱峰，一种对应的是“轻标”，一种对应的是“重标”
        public List<List<PEAK>> Match2(int isPPM, MS2_Quant_Help ms2_quant_help, ref List<string> annotations)
        {
            int aa_index = this.Pep.Tag_Flag;
            List<List<PEAK>> peaks = new List<List<PEAK>>();
            for (int i = 0; i < ms2_quant_help.nc_terms.Count; ++i)
                peaks.Add(new List<PEAK>());

            List<double> all_masses = new List<double>();
            for (int i = 0; i < ms2_quant_help.nc_terms.Count; ++i)
            {
                all_masses.Add(ms2_quant_help.nc_terms[i].n_mass);
                all_masses.Add(ms2_quant_help.nc_terms[i].c_mass);
            }
            List<double> mod_mass = new List<double>();
            for (int k = 0; k < Pep.Sq.Length + 2; ++k)
            {
                mod_mass.Add(0.0);
            }
            const double me = 0.00001;
            for (int k = 0; k < Pep.Mods.Count; ++k)
            {
                bool isIn = false;
                for (int p = 0; p < all_masses.Count; ++p)
                {
                    if (Math.Abs(all_masses[p] - Pep.Mods[k].Mass) <= me)
                    {
                        isIn = true;
                        break;
                    }
                }
                if (!isIn)
                    mod_mass[Pep.Mods[k].Index] = Pep.Mods[k].Mass;
            }
            //mod_mass[0] = 0.0;
            ////mod_mass[1] = 0.0;//???
            //mod_mass[mod_mass.Count - 1] = 0.0;
            //mod_mass[mod_mass.Count - 2] = 0.0;//???
            int[] mass_inten = new int[Config_Help.MaxMass];
            for (int k = 0; k < Spec.Peaks.Count; ++k)
            {
                int massi = (int)Spec.Peaks[k].Mass;
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
            for (int i = 0; i < ms2_quant_help.nc_terms.Count; ++i)
            {
                double n_mass = ms2_quant_help.nc_terms[i].n_mass;
                double c_mass = ms2_quant_help.nc_terms[i].c_mass;
                string n_str = ms2_quant_help.nc_terms[i].n_str;
                string c_str = ms2_quant_help.nc_terms[i].c_str;
                if (n_str == "")
                    mod_mass[0] = n_mass;
                else
                {
                    for (int j = 0; j < this.Pep.Sq.Length; ++j)
                    {
                        if (n_str.Contains(Pep.Sq[j]))
                            mod_mass[j + 1] = n_mass;
                    }
                }
                if (c_str == "")
                    mod_mass[Pep.Sq.Length + 1] = c_mass;
                else
                {
                    for (int j = 0; j < this.Pep.Sq.Length; ++j)
                    {
                        if (c_str.Contains(Pep.Sq[j]))
                            mod_mass[j + 1] = c_mass;
                    }
                }
                double tmpmass = mod_mass[0];
                //只算a1
                double a1 = tmpmass + Config_Help.mass_index[aa_index, this.Pep.Sq[0] - 'A'] + mod_mass[1] + Config_Help.A_Mass + Config_Help.massZI;
                double mass_error0 = 0.0;
                int index0 = -1;
                if (isPPM == 1)
                    index0 = IsInWithPPM2(a1, mass_inten, ref mass_error0);
                else
                    index0 = IsInWithDa2(a1, mass_inten, ref mass_error0);
                if (index0 != -1)
                    peaks[i].Add(this.Spec.Peaks[index0]);
                else
                    peaks[i].Add(new PEAK(a1, 0.0));
                annotations.Add("a1+");

                for (int j = 0; j < this.Pep.Sq.Length - 1; ++j)
                {
                    tmpmass += Config_Help.mass_index[aa_index, this.Pep.Sq[j] - 'A'] + mod_mass[j + 1];
                    double bmass1 = tmpmass + Config_Help.B_Mass + Config_Help.massZI;
                    double mass_error = 0.0;
                    int index = -1;
                    if (isPPM == 1)
                        index = IsInWithPPM2(bmass1, mass_inten, ref mass_error);
                    else
                        index = IsInWithDa2(bmass1, mass_inten, ref mass_error);
                    if (index != -1)
                        peaks[i].Add(this.Spec.Peaks[index]);
                    else
                        peaks[i].Add(new PEAK(bmass1, 0.0));
                    annotations.Add("b" + (j + 1) + "+");
                    double bmass2 = (tmpmass + Config_Help.B_Mass + 2 * Config_Help.massZI) / 2;
                    if (isPPM == 1)
                        index = IsInWithPPM2(bmass2, mass_inten, ref mass_error);
                    else
                        index = IsInWithDa2(bmass2, mass_inten, ref mass_error);
                    if (index != -1)
                        peaks[i].Add(this.Spec.Peaks[index]);
                    else
                        peaks[i].Add(new PEAK(bmass2, 0.0));
                    annotations.Add("b" + (j + 1) + "++");
                }
                tmpmass = mod_mass[this.Pep.Sq.Length + 1];
                for (int j = this.Pep.Sq.Length - 1; j > 0; --j)
                {
                    tmpmass += Config_Help.mass_index[aa_index, this.Pep.Sq[j] - 'A'] + mod_mass[j + 1];
                    double ymass1 = tmpmass + Config_Help.Y_Mass + Config_Help.massZI;
                    double mass_error = 0.0;
                    int index = -1;
                    if (isPPM == 1)
                        index = IsInWithPPM2(ymass1, mass_inten, ref mass_error);
                    else
                        index = IsInWithDa2(ymass1, mass_inten, ref mass_error);
                    if (index != -1)
                        peaks[i].Add(this.Spec.Peaks[index]);
                    else
                        peaks[i].Add(new PEAK(ymass1, 0.0));
                    annotations.Add("y" + (this.Pep.Sq.Length - j) + "+");
                    double ymass2 = (tmpmass + Config_Help.Y_Mass + 2 * Config_Help.massZI) / 2;
                    if (isPPM == 1)
                        index = IsInWithPPM2(ymass2, mass_inten, ref mass_error);
                    else
                        index = IsInWithDa2(ymass2, mass_inten, ref mass_error);
                    if (index != -1)
                        peaks[i].Add(this.Spec.Peaks[index]);
                    else
                        peaks[i].Add(new PEAK(ymass2, 0.0));
                    annotations.Add("y" + (this.Pep.Sq.Length - j) + "++");
                }
            }

            return peaks;
        }
        //返回匹配率，匹配的强度/总强度
        public double Match_Mix(int isPPM)
        {
            Initial();
            //初始化all_matches
            
            double match_intensity = 0.0;
            for (int i = 0; i < this.Mix_peps.Count; ++i)
            {
                this.Pep = this.Mix_peps[i];
                match_intensity += Match(isPPM, i);
            }
            this.Match_score = match_intensity;
            return match_intensity;
        }
        private int IsInWithPPM2(double mass, int[] mass_inten, ref double mass_error) //误差窗口内最高峰
        {
            int start = (int)(mass - mass * Ppm_mass_error);
            int end = (int)(mass + mass * Ppm_mass_error);
            if (start <= 0 || end >= Config_Help.MaxMass)
                return -1;
            double max_intensity = double.MinValue;
            int max_k = -1;
            for (int k = mass_inten[start - 1]; k < mass_inten[end]; ++k)
            {
                PEAK peak = (PEAK)(Spec.Peaks[k]);
                double tmpd_error = System.Math.Abs((peak.Mass - mass) / mass);
                double tmpd = peak.Intensity;
                if (tmpd_error <= Ppm_mass_error && tmpd > max_intensity)
                {
                    max_intensity = tmpd;
                    max_k = k;
                }
            }
            if (max_k != -1)
            {
                mass_error = (((PEAK)(Spec.Peaks[max_k])).Mass - mass) * 1.0e6 / mass;
            }
            return max_k;
        }
        private int IsInWithDa2(double mass, int[] mass_inten, ref double mass_error) //误差窗口内最高峰
        {
            int start = (int)(mass - Da_mass_error);
            int end = (int)(mass + Da_mass_error);
            if (start <= 0 || end >= Config_Help.MaxMass)
                return -1;
            double max_intensity = double.MinValue;
            int max_k = -1;
            for (int k = mass_inten[start - 1]; k < mass_inten[end]; ++k)
            {
                PEAK peak = (PEAK)(Spec.Peaks[k]);
                double tmpd_error = System.Math.Abs((peak.Mass - mass));
                double tmpd = peak.Intensity;
                if (tmpd_error <= Da_mass_error && tmpd > max_intensity)
                {
                    max_intensity = tmpd;
                    max_k = k;
                }
            }
            if (max_k != -1)
            {
                mass_error = ((PEAK)(Spec.Peaks[max_k])).Mass - mass;
            }
            return max_k;
        }
        //如果有轻重标记，那么将鉴定到的肽段的理论m/z放到list[0]中，而对应的“对儿”的理论m/z放到list[1]中
        public List<double> get_masses(int mass_count)
        {
            List<double> masses = new List<double>();
            for (int i = 0; i <= mass_count; ++i)
            {
                masses.Add(0.0);
            }
            int label_index1 = this.Pep.Tag_Flag;
            for (int i = 0; i < this.Pep.Sq.Length; ++i)
            {
                masses[0] += Config_Help.mass_index[label_index1, this.Pep.Sq[i] - 'A'];
                int ind = 1;
                for (int j = 1; j <= mass_count; ++j)
                {
                    if (j == label_index1)
                        continue;
                    masses[ind++] += Config_Help.mass_index[j, this.Pep.Sq[i] - 'A'];
                }
            }
            List<int> mod_flag = new List<int>();
            string modSites = Modification.get_modSites(this.Pep.Mods);
            Modification.get_modSites_list(modSites, ref mod_flag);
            for (int i = 0; i < this.Pep.Mods.Count; ++i)
            {
                int index1 = mod_flag[i];
                double[] tmp_double = Config_Help.modStr_hash[this.Pep.Mods[i].Mod_name] as double[];
                masses[0] += tmp_double[index1];
                int ind = 1;
                for (int j = 1; j <= mass_count; ++j)
                {
                    if (j == label_index1)
                        continue;
                    int index2 = (index1 == 0 ? 2 : 0);
                    if (label_index1 != 1) //如果鉴定到的是重标，那么轻标的修饰标记为全部为0
                        index2 = 0;
                    //查看对儿的信息是否存在着的，比如鉴定到的是轻标，对儿应该是重标，修饰也认为是重标，但由于修饰重标没有考虑，则使用轻标信息
                    if (Config_Help.mod_label_name[index2] == "")
                        index2 = 0;
                    masses[ind++] += tmp_double[index2];
                }
            }
            for (int i = 0; i < masses.Count; ++i)
                masses[i] = (masses[i] + Config_Help.massH2O + this.Spec.Charge * Config_Help.massZI) / this.Spec.Charge;
            return masses;
        }
    }

    public class PSM_Detail
    {
        public List<int>[] Peptide_index; //第i根峰匹配上的是哪个肽段
        //每根峰是N端匹配还是C端，0不匹配，1为N端匹配，2为C端匹配，3为NC均匹配，2^6为M匹配
        //即第一位表示N端匹配，第二位表示C端匹配。以后增加：第一位表示a离子匹配（包括a\a-H2O\a-NH3)，第二位表示b离子匹配，第三位表示c离子
        //第四位表示x离子匹配，第五位表示y离子匹配，第六位表示z离子匹配，第七位表示M离子匹配
        public int[] BOry { get; set; }
        //每根峰的匹配误差，可能一根峰与多种离子匹配则记录多个匹配误差
        public List<double>[] Mass_error { get; set; }
        //每根峰的匹配的字符串，可能一根峰与多种离子匹配则记录多个匹配的字符串
        public List<string>[] By_num { get; set; }
        //下面两项才是标阶梯图用的
        //肽段字符串中N端中有哪些匹配上了，HCD只有b\y匹配上了才标记，ETD只有c\z匹配上了才标记.
        public int[] B_flag { get; set; }
        public int[] C_flag { get; set; }
        //肽段字符串中C端中有哪些匹配上了
        public int[] Y_flag { get; set; }
        public int[] Z_flag { get; set; }
        //下面的四个是用来显示混合谱
        public List<List<int>> B_flag_mix { get; set; }
        public List<List<int>> C_flag_mix { get; set; }
        public List<List<int>> Y_flag_mix { get; set; }
        public List<List<int>> Z_flag_mix { get; set; }

        public List<PSM_Match> N_all_matches = new List<PSM_Match>(); //离子类型：a,a-H2O,a-NH3,b...
        public List<PSM_Match> C_all_matches = new List<PSM_Match>();

        public int getNIndex(string fragment_type)
        {
            for (int i = 0; i < N_all_matches.Count; ++i)
            {
                if (N_all_matches[i].fragment_type == fragment_type)
                    return i;
            }
            return -1;
        }
        public int getCIndex(string fragment_type)
        {
            for (int i = 0; i < C_all_matches.Count; ++i)
            {
                if (C_all_matches[i].fragment_type == fragment_type)
                    return i;
            }
            return -1;
        }
    }
    public class PSM_Match
    {
        public string fragment_type;
        public List<List<string>> fragment_theory_mass; //二维数组，第一维表示第几条肽段（混合谱、二肽、三肽），第二维表示该肽段对应的理论离子，比如考虑b离子有：b1,b2....
        public List<List<double>> fragment_theory_mass_double;
        public List<List<double>> fragment_match_intensity;
        public List<List<int>> fragment_match_flag;

        public PSM_Match(int pep_number)
        {
            this.fragment_type = "";
            this.fragment_theory_mass = new List<List<string>>();
            this.fragment_theory_mass_double = new List<List<double>>();
            this.fragment_match_intensity = new List<List<double>>();
            this.fragment_match_flag = new List<List<int>>();
            for (int i = 0; i < pep_number; ++i)
            {
                this.fragment_theory_mass.Add(new List<string>());
                this.fragment_theory_mass_double.Add(new List<double>());
                this.fragment_match_intensity.Add(new List<double>());
                this.fragment_match_flag.Add(new List<int>());
            }
        }
        public PSM_Match(string fragment_type, int pep_number)
        {
            this.fragment_type = fragment_type;
            this.fragment_theory_mass = new List<List<string>>();
            this.fragment_theory_mass_double = new List<List<double>>();
            this.fragment_match_intensity = new List<List<double>>();
            this.fragment_match_flag = new List<List<int>>();
            for (int i = 0; i < pep_number; ++i)
            {
                this.fragment_theory_mass.Add(new List<string>());
                this.fragment_theory_mass_double.Add(new List<double>());
                this.fragment_match_intensity.Add(new List<double>());
                this.fragment_match_flag.Add(new List<int>());
            }
        }
    }
}
