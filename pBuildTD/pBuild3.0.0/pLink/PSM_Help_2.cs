using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pBuild;

namespace pBuild.pLink
{
    public class PSM_Help_2 : PSM_Help_Parent
    {
        public Peptide Pep1 { get; set; }
        public Peptide Pep2 { get; set; }
        public int Link_pos1 { get; set; }
        public int Link_pos2 { get; set; }
        public double Xlink_mass { get; set; }
        public static bool isNormal = true, isInternal = false;
     

        public PSM_Help_2(string HCD_ETD_type) //默认匹配误差是20ppm，到时候会根据搜索的误差参数进行设置
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
            this.Peptide_Number = 2;
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
            for (int i = 0; i < 2; ++i)
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
        public override double Compute_Mass()
        {
            double pepmass = 0.0;
            this.Pep = this.Pep1;
            pepmass += base.Compute_Mass();
            this.Pep = this.Pep2;
            pepmass += (base.Compute_Mass() - Config_Help.massZI);
            pepmass += this.Xlink_mass;

            return pepmass;
        }
        public override double Match(int isPPM) //isPPM=1 表示PPM匹配，isPPM=0 表示Da匹配
        {
            //增加三电荷的离子匹配
            this.N_Match_Flag[0] = 1; //a+
            this.N_Match_Flag[1] = 1; //a++
            this.N_Match_Flag[3 * PSM_Help_Parent.charge_num + 2] = 1; //b+++
            this.C_Match_Flag[3 * PSM_Help_Parent.charge_num + 2] = 1; //y+++

            this.Pep1.update(); this.Pep2.update();

            int aa_index1 = this.Pep1.Tag_Flag;
            int aa_index2 = this.Pep2.Tag_Flag;
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

            bool[] isMatched = new bool[this.Spec.Peaks.Count];
            #region
            if (isNormal)
            {
                //匹配母离子
                if (this.Pep1.Sq != "" && this.Pep2.Sq != "") //如果序列是空的话，那么不需要匹配母离子 
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
                            isMatched[k_index] = true;
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
                                mass += this.Pep2.Pepmass + this.Xlink_mass;
                            else if (ss == 1 && k + 1 == this.Link_pos2)
                                mass += this.Pep1.Pepmass + this.Xlink_mass;
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
                                            isMatched[k_index] = true;
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
                                mass += this.Pep2.Pepmass + this.Xlink_mass;
                            else if (ss == 1 && k + 1 == this.Link_pos2)
                                mass += this.Pep1.Pepmass + this.Xlink_mass;
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
                                            isMatched[k_index] = true;
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
            }
            #endregion

            if (isPPM == 1 && isInternal) //计算内部离子
                getAA(mass_inten, ref isMatched);

            double fz = 0.0, fm = 0.0;
            for (int k = 0; k < Spec.Peaks.Count; ++k)
            {
                if (isMatched[k])
                    fz += Spec.Peaks[k].Intensity;
                fm += Spec.Peaks[k].Intensity;
            }
            Match_score = fz / fm;
            return Match_score;
        }
        private void getAA(int[] mass_inten, ref bool[] isMatched) //计算内部离子
        {
            //内部离子
            List<double> allM = new List<double>();
            List<string> allI = new List<string>();
            //单肽段的内部离子，当然包含了另一个肽段
            for (int k = 0; k < 2; ++k)
            {
                Peptide p = null, p2 = null;
                int linkPos = -1;
                if (k == 0)
                {
                    p = this.Pep1; p2 = this.Pep2; linkPos = this.Link_pos1;
                }
                else
                {
                    p = this.Pep2; p2 = this.Pep1; linkPos = this.Link_pos2;
                }
                for (int i = 1; i < p.Sq.Length - 1; ++i)
                {
                    for (int j = i; j < p.Sq.Length - 1; ++j)
                    {
                        double mass = 0.0;
                        if (i <= linkPos - 1 && linkPos - 1 >= j) //计算得到的包含了交联剂
                        {
                            mass = p.getAAMass(i, j) + p2.Pepmass + this.Xlink_mass;
                        }
                        else //不包含交联剂
                        {
                            mass = p.getAAMass(i, j);
                        }
                        allM.Add(mass + Config_Help.massZI);
                        allI.Add("[I]" + (k + 1) + ": " + i + "-" + j);
                    }
                }
            }
            //计算两个肽段之间的内部离子
            for (int i = 1; i < this.Pep1.Sq.Length - 1; ++i)
            {
                int start1 = -1, end1 = -1;
                if (i <= this.Link_pos1 - 1)
                {
                    start1 = i;
                    end1 = this.Pep1.Sq.Length - 1;
                }
                if (i >= this.Link_pos1 - 1)
                {
                    start1 = 0;
                    end1 = i;
                }
                double mass1 = this.Pep1.getAAMass(start1, end1);
                for (int j = 1; j < this.Pep2.Sq.Length - 1; ++j)
                {
                    int start2 = -1, end2 = -1;
                    if (j <= this.Link_pos2 - 1)
                    {
                        start2 = j;
                        end2 = this.Pep2.Sq.Length - 1;
                    }
                    if (j >= this.Link_pos2 - 1)
                    {
                        start2 = 0;
                        end2 = j;
                    }
                    double mass2 = this.Pep2.getAAMass(start2, end2);
                    allM.Add(mass1 + mass2 + this.Xlink_mass + Config_Help.massH2O + Config_Help.massZI);
                    allI.Add("[I]12: " + i + "-" + j);
                }
            }
            for (int k = 0; k < allM.Count(); ++k)
            {
                double mass_error = 0.0;
                int k_index = IsInWithPPM(allM[k], mass_inten, ref mass_error);
                if (k_index != -1)
                {
                    isMatched[k_index] = true;
                    this.Psm_detail.BOry[k_index] = 128; //内部离子表示位，到时候画图的时候会根据这个值来调整绘图的颜色
                    this.Psm_detail.By_num[k_index].Add(allI[k]);
                    this.Psm_detail.Mass_error[k_index].Add(mass_error);
                }
            }
        }
        public void get_masses_peptide(List<double> masses, Peptide pp)
        {
            int label_index1 = pp.Tag_Flag;
            int label_index2 = (label_index1 == 1 ? 2 : 1);
            for (int i = 0; i < pp.Sq.Length; ++i)
            {
                masses[0] += Config_Help.mass_index[label_index1, pp.Sq[i] - 'A'];
                masses[1] += Config_Help.mass_index[label_index2, pp.Sq[i] - 'A'];
            }
            List<int> mod_flag = new List<int>();
            string modSites = Modification.get_modSites(pp.Mods);
            Modification.get_modSites_list(modSites, ref mod_flag);
            for (int i = 0; i < pp.Mods.Count; ++i)
            {
                int index1 = mod_flag[i];
                int index2 = (index1 == 0 ? 2 : 0);
                if (label_index1 == 2) //如果鉴定到的是重标，那么轻标的修饰标记为全部为0
                    index2 = 0;
                //查看对儿的信息是否存在着的，比如鉴定到的是轻标，对儿应该是重标，修饰也认为是重标，但由于修饰重标没有考虑，则使用轻标信息
                if (Config_Help.mod_label_name[index2] == "")
                    index2 = 0;
                double[] tmp_double = Config_Help.modStr_hash[pp.Mods[i].Mod_name] as double[];
                if (pp.Mods[i].Mod_name == "Deamidated_15[N]") //代码写得很差，也是方便pLink2使用pbuild软件来加载0Da修饰，查看一级谱
                {
                    tmp_double = Config_Help.modStr_hash["Deamidated[N]"] as double[];
                }
                masses[0] += tmp_double[index1];
                masses[1] += tmp_double[index2];
            }
            masses[0] = masses[0] + Config_Help.massH2O;
            masses[1] = masses[1] + Config_Help.massH2O;
        }
        public void get_masses(List<double> masses, List<double> linker_masses)
        {
            masses[0] += Xlink_mass;
            int max_index = -1;
            double max = double.MinValue;
            for (int i = 0; i < linker_masses.Count; ++i)
            {
                double tmp = Math.Abs(linker_masses[i] - Xlink_mass);
                if (tmp > max)
                {
                    max = tmp;
                    max_index = i;
                }
            }
            masses[1] += linker_masses[max_index];
        }
        public List<double> get_masses(pLink_Label pll)
        {
            List<double> masses = new List<double>();
            masses.Add(0.0);
            masses.Add(0.0);
            if (pll.Flag == 1) //肽段标记
            {
                get_masses_peptide(masses, this.Pep1);
                get_masses_peptide(masses, this.Pep2);
                masses[0] = (masses[0] + Xlink_mass + this.Spec.Charge * Config_Help.massZI) / this.Spec.Charge;
                masses[1] = (masses[1] + Xlink_mass + this.Spec.Charge * Config_Help.massZI) / this.Spec.Charge;
            }
            else if (pll.Flag == 0) //交联剂标记
            {
                get_masses_peptide(masses, this.Pep1);
                get_masses_peptide(masses, this.Pep2);
                masses[1] = masses[0];
                get_masses(masses, pll.Linker_Masses);
                masses[0] = (masses[0] + this.Spec.Charge * Config_Help.massZI) / this.Spec.Charge;
                masses[1] = (masses[1] + this.Spec.Charge * Config_Help.massZI) / this.Spec.Charge;
            }
            return masses;
        }
    }
}
