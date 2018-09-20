using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild.pLink
{
    public class PSM_Help_3 : PSM_Help_Parent
    {
        public Peptide Pep1 { get; set; }
        public Peptide Pep2 { get; set; }
        public Peptide Pep3 { get; set; }
        public int Link_pos1 { get; set; }
        public int Link_pos2 { get; set; }
        public int Link_pos3 { get; set; }
        public int Link_pos4 { get; set; }
        public double Xlink_mass { get; set; }

        public PSM_Help_3(string HCD_ETD_type) //默认匹配误差是20ppm，到时候会根据搜索的误差参数进行设置
        {
            Ppm_mass_error = 20e-6;
            Da_mass_error = 0;
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
            this.Mix_Flag = 1;
            this.Peptide_Number = 3;
            //初始化只考虑HCD，即b+\b++\y+\y++，并且加上母离子[M]+和[M]++
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
            this.Psm_detail.B_flag_mix = new List<List<int>>();
            this.Psm_detail.C_flag_mix = new List<List<int>>();
            this.Psm_detail.Y_flag_mix = new List<List<int>>();
            this.Psm_detail.Z_flag_mix = new List<List<int>>();
            for (int i = 0; i < 3; ++i)
            {
                this.Psm_detail.B_flag_mix.Add(new List<int>());
                this.Psm_detail.C_flag_mix.Add(new List<int>());
                this.Psm_detail.Y_flag_mix.Add(new List<int>());
                this.Psm_detail.Z_flag_mix.Add(new List<int>());
                Peptide p = this.Pep1;
                switch (i)
                {
                    case 0:
                        p = this.Pep1;
                        break;
                    case 1:
                        p = this.Pep2;
                        break;
                    case 2:
                        p = this.Pep3;
                        break;
                }
                for (int j = 0; j < p.Sq.Length + 1; ++j)
                {
                    this.Psm_detail.B_flag_mix[i].Add(0);
                    this.Psm_detail.C_flag_mix[i].Add(0);
                    this.Psm_detail.Y_flag_mix[i].Add(0);
                    this.Psm_detail.Z_flag_mix[i].Add(0);
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

        public override double Match(int isPPM) //isPPM=1 表示PPM匹配，isPPM=0 表示Da匹配
        {
            int aa_index1 = this.Pep1.Tag_Flag;
            int aa_index2 = this.Pep2.Tag_Flag;
            int aa_index3 = this.Pep3.Tag_Flag;
            this.Pep1.update(); this.Pep2.update(); this.Pep3.update();
            Initial();
            //初始化all_matches
            for (int i = 0; i < N_Match_Flag.Length; ++i)
            {
                Psm_detail.N_all_matches.Add(new PSM_Match(this.Peptide_Number));
            }
            for (int i = 0; i < C_Match_Flag.Length; ++i)
            {
                Psm_detail.C_all_matches.Add(new PSM_Match(this.Peptide_Number));
            }
            double fz = 0.0, fm = 0.0;
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
            //匹配母离子
            if (this.Pep1.Sq != "" && this.Pep2.Sq != "" && this.Pep3.Sq != "") //如果序列是空的话，那么不需要匹配母离子 
            {
                for (int k = 0; k < M_Match_Flag.Length; ++k)
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
                    for (int j = 0; j < match_type_charge; ++j)
                    {
                        charge_str += "+";
                    }
                    double pepmass = 0.0;
                    pepmass = (Spec.Pepmass - Config_Help.massZI) * Spec.Charge;

                    double M_match_mass = (pepmass + tmp_mass + match_type_charge * Config_Help.massZI) / match_type_charge;
                    double mass_error = 0.0;
                    int k_index = -1;
                    if (isPPM == 1)
                        k_index = IsInWithPPM(M_match_mass, mass_inten, ref mass_error);
                    else
                        k_index = IsInWithDa(M_match_mass, mass_inten, ref mass_error);
                    if (k_index != -1)
                    {
                        this.Psm_detail.Peptide_index[k_index].Add(0);
                        this.Psm_detail.BOry[k_index] |= 64;
                        this.Psm_detail.By_num[k_index].Add(type_str + charge_str);
                        this.Psm_detail.Mass_error[k_index].Add(mass_error);
                        fz += this.Spec.Peaks[k_index].Intensity;
                    }
                }
            }
            for (int ss = 0; ss < this.Peptide_Number; ++ss)
            {
                double mass = 0;
                List<double> mod_mass = new List<double>();
                List<List<double>> mod_loss_mass = new List<List<double>>();
                List<List<double>> mod_loss_mass_b = new List<List<double>>();
                List<List<double>> mod_loss_mass_y = new List<List<double>>();
                Peptide Pep = this.Pep1;
                int aa_index = aa_index1;
                switch (ss)
                {
                    case 0:
                        Pep = this.Pep1;
                        aa_index = aa_index1;
                        break;
                    case 1:
                        Pep = this.Pep2;
                        aa_index = aa_index2;
                        break;
                    case 2:
                        Pep = this.Pep3;
                        aa_index = aa_index3;
                        break;
                }
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
                for (int l = 0; l < mod_loss_mass_b[0].Count; ++l)
                {
                    mass = mod_mass[0] - mod_loss_mass_b[0][l];
                    for (int k = 0; k < Pep.Sq.Length - 1; ++k)
                    {
                        mass += Config_Help.mass_index[aa_index, Pep.Sq[k] - 'A'] + mod_mass[k + 1];
                        if (ss == 0 && k + 1 == this.Link_pos1)
                            mass += this.Pep2.Pepmass + this.Pep3.Pepmass + this.Xlink_mass * 2;
                        else if (ss == 1)
                        {
                            if (k + 1 == this.Link_pos2)
                                mass += this.Pep1.Pepmass + this.Xlink_mass;
                            if (k + 1 == this.Link_pos3)
                                mass += this.Pep3.Pepmass + this.Xlink_mass;
                            
                        }
                        else if (ss == 2 && k + 1 == this.Link_pos4)
                            mass += this.Pep1.Pepmass + this.Pep2.Pepmass + this.Xlink_mass * 2;
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
                                    if (mod_loss_mass_b[k + 1][ll] != 0.0)
                                    {
                                        str_type = "* " + str_type;
                                    }
                                    if (mod_loss_mass_b[k + 1][ll] == 0.0)
                                    {
                                        Psm_detail.N_all_matches[p].fragment_type = str_type + str_type_last + str_type_charge;
                                        Psm_detail.N_all_matches[p].fragment_theory_mass[ss].Add(match_mass.ToString("f4"));
                                        Psm_detail.N_all_matches[p].fragment_theory_mass_double[ss].Add(match_mass);
                                    }
                                    int k_index = -1;
                                    if (isPPM == 1)
                                        k_index = IsInWithPPM(match_mass, mass_inten, ref mass_error);
                                    else
                                        k_index = IsInWithDa(match_mass, mass_inten, ref mass_error);
                                    if (k_index != -1)
                                    {
                                        this.Psm_detail.Peptide_index[k_index].Add(ss);
                                        this.Psm_detail.BOry[k_index] |= 1;
                                        this.Psm_detail.By_num[k_index].Add(str_type + (k + 1) + str_type_last + str_type_charge + "|" + ss);
                                        this.Psm_detail.Mass_error[k_index].Add(mass_error);
                                        if (match_type == 3) //匹配到b离子，那么会在对应的阶梯中显示N端匹配
                                        {
                                            this.Psm_detail.B_flag_mix[ss][k + 1] = 1;
                                        }
                                        else if (match_type == 6)
                                        {
                                            this.Psm_detail.C_flag_mix[ss][k + 1] = 1;
                                        }
                                        fz += this.Spec.Peaks[k_index].Intensity;
                                        if (mod_loss_mass_b[k + 1][ll] == 0.0)
                                            Psm_detail.N_all_matches[p].fragment_match_flag[ss].Add(1);
                                    }
                                    else
                                    {
                                        if (mod_loss_mass_b[k + 1][ll] == 0.0)
                                            Psm_detail.N_all_matches[p].fragment_match_flag[ss].Add(0);
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
                        if (ss == 0 && k + 1 == this.Link_pos1)
                            mass += this.Pep2.Pepmass + this.Pep3.Pepmass + this.Xlink_mass * 2;
                        else if (ss == 1)
                        {
                            if (k + 1 == this.Link_pos2)
                                mass += this.Pep1.Pepmass + this.Xlink_mass;
                            if (k + 1 == this.Link_pos3)
                                mass += this.Pep3.Pepmass + this.Xlink_mass;
                        }
                        else if (ss == 2 && k + 1 == this.Link_pos4)
                            mass += this.Pep1.Pepmass + this.Pep2.Pepmass + this.Xlink_mass * 2;
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
                                    if (mod_loss_mass_y[k + 1][ll] != 0.0)
                                    {
                                        str_type = "* " + str_type;
                                    }
                                    if (mod_loss_mass_y[k + 1][ll] == 0.0)
                                    {
                                        Psm_detail.C_all_matches[p].fragment_type = str_type + str_type_last + str_type_charge;
                                        Psm_detail.C_all_matches[p].fragment_theory_mass[ss].Add(match_mass.ToString("f4"));
                                        Psm_detail.C_all_matches[p].fragment_theory_mass_double[ss].Add(match_mass);
                                    }
                                    int k_index = -1;
                                    if (isPPM == 1)
                                        k_index = IsInWithPPM(match_mass, mass_inten, ref mass_error);
                                    else
                                        k_index = IsInWithDa(match_mass, mass_inten, ref mass_error);
                                    if (k_index != -1)
                                    {
                                        this.Psm_detail.Peptide_index[k_index].Add(ss);
                                        this.Psm_detail.BOry[k_index] |= 2;
                                        this.Psm_detail.By_num[k_index].Add(str_type + (Pep.Sq.Length - k) + str_type_last + str_type_charge + "|" + ss);
                                        this.Psm_detail.Mass_error[k_index].Add(mass_error);
                                        if (match_type == 3)
                                        {
                                            this.Psm_detail.Y_flag_mix[ss][Pep.Sq.Length - k] = 1;
                                        }
                                        else if (match_type == 6)
                                        {
                                            this.Psm_detail.Z_flag_mix[ss][Pep.Sq.Length - k] = 1;
                                        }
                                        fz += this.Spec.Peaks[k_index].Intensity;
                                        if (mod_loss_mass_y[k + 1][ll] == 0.0)
                                            Psm_detail.C_all_matches[p].fragment_match_flag[ss].Add(1);
                                    }
                                    else
                                    {
                                        if (mod_loss_mass_y[k + 1][ll] == 0.0)
                                            Psm_detail.C_all_matches[p].fragment_match_flag[ss].Add(0);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            for (int k = 0; k < Spec.Peaks.Count; ++k)
            {
                fm += Spec.Peaks[k].Intensity;
            }
            Match_score = fz / fm;
            return Match_score;
        }
    }
}
