using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class DataGrid_Compare_Peptide : IComparer
    {
        private ListSortDirection direction;
        private string header_name;

        public DataGrid_Compare_Peptide(ListSortDirection direction, string header_name)
        {
            this.direction = direction;
            this.header_name = header_name;
        }

        public int Compare(object x, object y)
        {
            PSM psm1 = x as PSM;
            PSM psm2 = y as PSM;
            //sort int
            if (header_name == "#")
            {
                int psm1_ID = psm1.Id;
                int psm2_ID = psm2.Id;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (psm1_ID > psm2_ID)
                        return 1;
                    else if (psm1_ID < psm2_ID)
                        return -1;
                    return 0;
                }
                else
                {
                    if (psm1_ID > psm2_ID)
                        return -1;
                    else if (psm1_ID < psm2_ID)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "Spectrum")
            {
                string psm1_title = psm1.Title;
                string psm2_title = psm2.Title;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (string.Compare(psm1_title, psm2_title) > 0)
                        return 1;
                    else if (string.Compare(psm1_title, psm2_title) < 0)
                        return -1;
                    return 0;
                }
                else
                {
                    if (string.Compare(psm1_title, psm2_title) > 0)
                        return -1;
                    else if (string.Compare(psm1_title, psm2_title) < 0)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "Charge")
            {
                int psm1_charge = psm1.Charge;
                int psm2_charge = psm2.Charge;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (psm1_charge > psm2_charge)
                        return 1;
                    else if (psm1_charge < psm2_charge)
                        return -1;
                    return 0;
                }
                else
                {
                    if (psm1_charge > psm2_charge)
                        return -1;
                    else if (psm1_charge < psm2_charge)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "Spectrum mass")
            {
                double psm1_mass = psm1.Spectra_mass;
                double psm2_mass = psm2.Spectra_mass;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (psm1_mass > psm2_mass)
                        return 1;
                    else if (psm1_mass < psm2_mass)
                        return -1;
                    return 0;
                }
                else
                {
                    if (psm1_mass > psm2_mass)
                        return -1;
                    else if (psm1_mass < psm2_mass)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "Sequence")
            {
                string psm1_sq = psm1.Sq;
                string psm2_sq = psm2.Sq;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (string.Compare(psm1_sq, psm2_sq) > 0)
                        return 1;
                    else if (string.Compare(psm1_sq, psm2_sq) < 0)
                        return -1;
                    return 0;
                }
                else
                {
                    if (string.Compare(psm1_sq, psm2_sq) > 0)
                        return -1;
                    else if (string.Compare(psm1_sq, psm2_sq) < 0)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "Mod_Sites")
            {
                string psm1_modsites = psm1.Mod_sites;
                string psm2_modsites = psm2.Mod_sites;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (string.Compare(psm1_modsites, psm2_modsites) > 0)
                        return 1;
                    else if (string.Compare(psm1_modsites, psm2_modsites) < 0)
                        return -1;
                    return 0;
                }
                else
                {
                    if (string.Compare(psm1_modsites, psm2_modsites) > 0)
                        return -1;
                    else if (string.Compare(psm1_modsites, psm2_modsites) < 0)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "Specific")
            {
                char psm1_specific_flag = psm1.Specific_flag;
                char psm2_specific_flag = psm2.Specific_flag;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (psm1_specific_flag > psm2_specific_flag)
                        return 1;
                    else if (psm1_specific_flag < psm2_specific_flag)
                        return -1;
                    return 0;
                }
                else
                {
                    if (psm1_specific_flag > psm2_specific_flag)
                        return -1;
                    else if (psm1_specific_flag < psm2_specific_flag)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "Missed_Cleavage")
            {
                int psm1_missed_number = psm1.Missed_cleavage_number;
                int psm2_missed_number = psm2.Missed_cleavage_number;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (psm1_missed_number > psm2_missed_number)
                        return 1;
                    else if (psm1_missed_number < psm2_missed_number)
                        return -1;
                    return 0;
                }
                else
                {
                    if (psm1_missed_number > psm2_missed_number)
                        return -1;
                    else if (psm1_missed_number < psm2_missed_number)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "Score")
            {
                double psm1_score = psm1.Score;
                double psm2_score = psm2.Score;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (psm1_score > psm2_score)
                        return 1;
                    else if (psm1_score < psm2_score)
                        return -1;
                    return 0;
                }
                else
                {
                    if (psm1_score > psm2_score)
                        return -1;
                    else if (psm1_score < psm2_score)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "Ratio")
            {
                double psm1_ratio = psm1.Ratio;
                double psm2_ratio = psm2.Ratio;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (psm1_ratio > psm2_ratio)
                        return 1;
                    else if (psm1_ratio < psm2_ratio)
                        return -1;
                    return 0;
                }
                else
                {
                    if (psm1_ratio > psm2_ratio)
                        return -1;
                    else if (psm1_ratio < psm2_ratio)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "Sigma")
            {
                double psm1_sigma = psm1.Sigma;
                double psm2_sigma = psm2.Sigma;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (psm1_sigma > psm2_sigma)
                        return 1;
                    else if (psm1_sigma < psm2_sigma)
                        return -1;
                    return 0;
                }
                else
                {
                    if (psm1_sigma > psm2_sigma)
                        return -1;
                    else if (psm1_sigma < psm2_sigma)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "AC")
            {
                string psm1_ac = psm1.AC;
                string psm2_ac = psm2.AC;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (string.Compare(psm1_ac, psm2_ac) > 0)
                        return 1;
                    else if (string.Compare(psm1_ac, psm2_ac) < 0)
                        return -1;
                    return 0;
                }
                else
                {
                    if (string.Compare(psm1_ac, psm2_ac) > 0)
                        return -1;
                    else if (string.Compare(psm1_ac, psm2_ac) < 0)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "Target_Decoy")
            {
                string psm1_is_target_flag = psm1.Is_target_flag.ToString();
                string psm2_is_target_flag = psm2.Is_target_flag.ToString();
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (string.Compare(psm1_is_target_flag, psm2_is_target_flag) > 0)
                        return 1;
                    else if (string.Compare(psm1_is_target_flag, psm2_is_target_flag) < 0)
                        return -1;
                    return 0;
                }
                else
                {
                    if (string.Compare(psm1_is_target_flag, psm2_is_target_flag) > 0)
                        return -1;
                    else if (string.Compare(psm1_is_target_flag, psm2_is_target_flag) < 0)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "Label_Name")
            {
                string psm1_N15 = psm1.Pep_flag;
                string psm2_N15 = psm2.Pep_flag;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (string.Compare(psm1_N15, psm2_N15) > 0)
                        return 1;
                    else if (string.Compare(psm1_N15, psm2_N15) < 0)
                        return -1;
                    return 0;
                }
                else
                {
                    if (string.Compare(psm1_N15, psm2_N15) > 0)
                        return -1;
                    else if (string.Compare(psm1_N15, psm2_N15) < 0)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "Detla_Mass (Da)")
            {
                double psm1_da_mass = psm1.Delta_mass;
                double psm2_da_mass = psm2.Delta_mass;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (psm1_da_mass > psm2_da_mass)
                        return 1;
                    else if (psm1_da_mass < psm2_da_mass)
                        return -1;
                    return 0;
                }
                else
                {
                    if (psm1_da_mass > psm2_da_mass)
                        return -1;
                    else if (psm1_da_mass < psm2_da_mass)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "Detla_Mass (PPM)")
            {
                double psm1_ppm_mass = psm1.Delta_mass_PPM;
                double psm2_ppm_mass = psm2.Delta_mass_PPM;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (psm1_ppm_mass > psm2_ppm_mass)
                        return 1;
                    else if (psm1_ppm_mass < psm2_ppm_mass)
                        return -1;
                    return 0;
                }
                else
                {
                    if (psm1_ppm_mass > psm2_ppm_mass)
                        return -1;
                    else if (psm1_ppm_mass < psm2_ppm_mass)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "MS2 Ratio")
            {
                double psm1_ms2_ratio = psm1.Ms2_ratio;
                double psm2_ms2_ratio = psm2.Ms2_ratio;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (psm1_ms2_ratio > psm2_ms2_ratio)
                        return 1;
                    else if (psm1_ms2_ratio < psm2_ms2_ratio)
                        return -1;
                    return 0;
                }
                else
                {
                    if (psm1_ms2_ratio > psm2_ms2_ratio)
                        return -1;
                    else if (psm1_ms2_ratio < psm2_ms2_ratio)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "MS2 Ratio a1")
            {
                double psm1_ms2_ratio = psm1.Ms2_ratio_a1;
                double psm2_ms2_ratio = psm2.Ms2_ratio_a1;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (psm1_ms2_ratio > psm2_ms2_ratio)
                        return 1;
                    else if (psm1_ms2_ratio < psm2_ms2_ratio)
                        return -1;
                    return 0;
                }
                else
                {
                    if (psm1_ms2_ratio > psm2_ms2_ratio)
                        return -1;
                    else if (psm1_ms2_ratio < psm2_ms2_ratio)
                        return 1;
                    return 0;
                }
            }
            else
                return 0;
        }
    }
    public class DataGrid_Compare_Protein : IComparer
    {
        private ListSortDirection direction;
        private string header_name;

        public DataGrid_Compare_Protein(ListSortDirection direction, string header_name)
        {
            this.direction = direction;
            this.header_name = header_name;
        }

        public int Compare(object x, object y)
        {
            Protein protein1 = x as Protein;
            Protein protein2 = y as Protein;
            if (header_name == "#")
            {
                int protein1_ID = protein1.ID;
                int protein2_ID = protein2.ID;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (protein1_ID > protein2_ID)
                        return 1;
                    else if (protein1_ID < protein2_ID)
                        return -1;
                    return 0;
                }
                else
                {
                    if (protein1_ID > protein2_ID)
                        return -1;
                    else if (protein1_ID < protein2_ID)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "AC")
            {
                string protein1_AC = protein1.AC;
                string protein2_AC = protein2.AC;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (string.Compare(protein1_AC, protein2_AC) > 0)
                        return 1;
                    else if (string.Compare(protein1_AC, protein2_AC) < 0)
                        return -1;
                    return 0;
                }
                else
                {
                    if (string.Compare(protein1_AC, protein2_AC) > 0)
                        return -1;
                    else if (string.Compare(protein1_AC, protein2_AC) < 0)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "DE")
            {
                string protein1_DE = protein1.DE;
                string protein2_DE = protein2.DE;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (string.Compare(protein1_DE, protein2_DE) > 0)
                        return 1;
                    else if (string.Compare(protein1_DE, protein2_DE) < 0)
                        return -1;
                    return 0;
                }
                else
                {
                    if (string.Compare(protein1_DE, protein2_DE) > 0)
                        return -1;
                    else if (string.Compare(protein1_DE, protein2_DE) < 0)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "PSM Count")
            {
                int protein1_psm_count = protein1.psm_index.Count;
                int protein2_psm_count = protein2.psm_index.Count;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (protein1_psm_count > protein2_psm_count)
                        return 1;
                    else if (protein1_psm_count < protein2_psm_count)
                        return -1;
                    return 0;
                }
                else
                {
                    if (protein1_psm_count > protein2_psm_count)
                        return -1;
                    else if (protein1_psm_count < protein2_psm_count)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "Coverage")
            {
                double protein1_coverage = protein1.Coverage;
                double protein2_coverage = protein2.Coverage;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (protein1_coverage > protein2_coverage)
                        return 1;
                    else if (protein1_coverage < protein2_coverage)
                        return -1;
                    return 0;
                }
                else
                {
                    if (protein1_coverage > protein2_coverage)
                        return -1;
                    else if (protein1_coverage < protein2_coverage)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "Score")
            {
                double protein1_score = protein1.Score;
                double protein2_score = protein2.Score;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (protein1_score > protein2_score)
                        return 1;
                    else if (protein1_score < protein2_score)
                        return -1;
                    return 0;
                }
                else
                {
                    if (protein1_score > protein2_score)
                        return -1;
                    else if (protein1_score < protein2_score)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "Ratio")
            {
                double protein1_ratio = protein1.Ratio;
                double protein2_ratio = protein2.Ratio;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (protein1_ratio > protein2_ratio)
                        return 1;
                    else if (protein1_ratio < protein2_ratio)
                        return -1;
                    return 0;
                }
                else
                {
                    if (protein1_ratio > protein2_ratio)
                        return -1;
                    else if (protein1_ratio < protein2_ratio)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "Protein Ratio")
            {
                double protein1_ms2_ratio = protein1.MS2_Ratio;
                double protein2_ms2_ratio = protein2.MS2_Ratio;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (protein1_ms2_ratio > protein2_ms2_ratio)
                        return 1;
                    else if (protein1_ms2_ratio < protein2_ms2_ratio)
                        return -1;
                    return 0;
                }
                else
                {
                    if (protein1_ms2_ratio > protein2_ms2_ratio)
                        return -1;
                    else if (protein1_ms2_ratio < protein2_ms2_ratio)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "Protein Ratio a1")
            {
                double protein1_ms2_ratio = protein1.MS2_Ratio_a1;
                double protein2_ms2_ratio = protein2.MS2_Ratio_a1;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (protein1_ms2_ratio > protein2_ms2_ratio)
                        return 1;
                    else if (protein1_ms2_ratio < protein2_ms2_ratio)
                        return -1;
                    return 0;
                }
                else
                {
                    if (protein1_ms2_ratio > protein2_ms2_ratio)
                        return -1;
                    else if (protein1_ms2_ratio < protein2_ms2_ratio)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "Group")
            {
                string protein1_Group = protein1.Parent_Protein_AC;
                string protein2_Group = protein2.Parent_Protein_AC;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (string.Compare(protein1_Group, protein2_Group) > 0)
                        return 1;
                    else if (string.Compare(protein1_Group, protein2_Group) < 0)
                        return -1;
                    return 0;
                }
                else
                {
                    if (string.Compare(protein1_Group, protein2_Group) > 0)
                        return -1;
                    else if (string.Compare(protein1_Group, protein2_Group) < 0)
                        return 1;
                    return 0;
                }
            }
            else if (header_name == "Flag")
            {
                string protein1_Flag = protein1.Same_Sub_Flag;
                string protein2_Flag = protein2.Same_Sub_Flag;
                if (this.direction == ListSortDirection.Ascending)
                {
                    if (string.Compare(protein1_Flag, protein2_Flag) > 0)
                        return 1;
                    else if (string.Compare(protein1_Flag, protein2_Flag) < 0)
                        return -1;
                    return 0;
                }
                else
                {
                    if (string.Compare(protein1_Flag, protein2_Flag) > 0)
                        return -1;
                    else if (string.Compare(protein1_Flag, protein2_Flag) < 0)
                        return 1;
                    return 0;
                }
            }
            return 1;
        }
    }
}
