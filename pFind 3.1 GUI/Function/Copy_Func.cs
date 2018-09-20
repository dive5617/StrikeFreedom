using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pFind.classes;
using pFind.Interface;
using System.IO;
using System.Xml.Serialization;

namespace pFind.Function
{
    class Copy_Func:Copy_Inter
    {

        T Copy_Inter.DeepCopyWithXmlSerializer<T>(T obj)  // good example!
        {
            object retval;
            using(MemoryStream ms = new MemoryStream())
            {
                XmlSerializer xml = new XmlSerializer(typeof(T));
                xml.Serialize(ms, obj);
                ms.Seek(0,SeekOrigin.Begin);
                retval = xml.Deserialize(ms);
                ms.Close();
            }
            return (T)retval;
        }


        void Copy_Inter.pParseAdvancedCopy(pParse_Advanced spa,pParse_Advanced dpa)
        {
            dpa.Isolation_width = spa.Isolation_width;
            dpa.Ipv_file = string.Copy(spa.Ipv_file);
            dpa.Trainingset = string.Copy(spa.Trainingset);
            dpa.Output_mars_y = spa.Output_mars_y;
            dpa.Output_msn = spa.Output_msn;
            dpa.Output_mgf = spa.Output_mgf;
            dpa.Output_pf = spa.Output_pf;
            dpa.Debug_mode = spa.Debug_mode;
            dpa.Check_activationcenter = spa.Check_activationcenter;
            dpa.Output_all_mars_y = spa.Output_all_mars_y;
            dpa.Rewrite_files = spa.Rewrite_files;
            dpa.Export_unchecked_mono = spa.Export_unchecked_mono;
            dpa.Cut_similiar_mono = spa.Cut_similiar_mono;
            dpa.Output_trainingdata = spa.Output_trainingdata;
        }
        
        void Copy_Inter.FileCopy(File sf, File df)
        {
            df.File_format_index = sf.File_format_index;
            df.File_format = string.Copy(sf.File_format);
            df.Instrument_index = sf.Instrument_index;
            df.Instrument = string.Copy(sf.Instrument);
            df.Data_file_list.Clear();
            for (int i = 0; i < sf.Data_file_list.Count; i++)
            {
                df.Data_file_list.Add(sf.Data_file_list[i]);
            }
            df.Mix_spectra = sf.Mix_spectra;
            df.Mz_decimal_index = sf.Mz_decimal_index;
            df.Mz_decimal = sf.Mz_decimal;
            df.Intensity_decimal_index = sf.Intensity_decimal_index;
            df.Intensity_decimal = sf.Intensity_decimal;
            df.Model = sf.Model;
            df.ModelIndex = sf.ModelIndex;
            df.Threshold = sf.Threshold;
            Factory.Create_Copy_Instance().pParseAdvancedCopy(sf.Pparse_advanced,df.Pparse_advanced);
        }

        //copy SearchParam
        void Copy_Inter.SearchParamCopy(SearchParam ssp, SearchParam dsp)
        {
            dsp.Db_index = ssp.Db_index;
            dsp.Db.Db_name = string.Copy(ssp.Db.Db_name);
            dsp.Db.Db_path = string.Copy(ssp.Db.Db_path);
            dsp.Enzyme_index = ssp.Enzyme_index;
            dsp.Enzyme = string.Copy(ssp.Enzyme);
            dsp.Enzyme_Spec = string.Copy(ssp.Enzyme_Spec);
            dsp.Enzyme_Spec_index = ssp.Enzyme_Spec_index;
            dsp.Cleavages = ssp.Cleavages;
            dsp.Ptl.Tl_value = ssp.Ptl.Tl_value;
            dsp.Ptl.Isppm = ssp.Ptl.Isppm;
            dsp.Ftl.Tl_value = ssp.Ftl.Tl_value;
            dsp.Ftl.Isppm = ssp.Ftl.Isppm;
            dsp.Open_search = ssp.Open_search;
            dsp.Fix_mods.Clear();
            for (int i = 0; i < ssp.Fix_mods.Count; i++)
            {
                dsp.Fix_mods.Add(ssp.Fix_mods[i]);
            }
            dsp.Var_mods.Clear();
            for (int i = 0; i < ssp.Var_mods.Count; i++)
            {
                dsp.Var_mods.Add(ssp.Var_mods[i]);
            }
            Factory.Create_Copy_Instance().SearchAdvancedCopy(ssp.Search_advanced,dsp.Search_advanced);
        }

        void Copy_Inter.SearchAdvancedCopy(Search_Advanced ssa,Search_Advanced dsa)
        {
            dsa.Temppepnum = ssa.Temppepnum;
            dsa.Pepnum = ssa.Pepnum;
            dsa.Selectpeak = ssa.Selectpeak;
            dsa.Maxprolen = ssa.Maxprolen;
            dsa.Maxspec = ssa.Maxspec;
            dsa.Maxmod = ssa.Maxmod;
            dsa.Open_tag_len = ssa.Open_tag_len;
            dsa.Rest_tag_iteration = ssa.Rest_tag_iteration;
            dsa.Rest_tag_len = ssa.Rest_tag_len;
            dsa.Rest_mod_num = ssa.Rest_mod_num;
            dsa.Salvo_mod_num = ssa.Salvo_mod_num;
            dsa.Salvo_iteration = ssa.Salvo_iteration;            
        }
        //copy FilterParam
        void Copy_Inter.FilterParamCopy(FilterParam sfp, FilterParam dfp)
        {
            dfp.Fdr.Fdr_value = sfp.Fdr.Fdr_value;
            dfp.Fdr.IsPeptides = sfp.Fdr.IsPeptides;
            dfp.Pep_mass_range.Left_value = sfp.Pep_mass_range.Left_value;
            dfp.Pep_mass_range.Right_value = sfp.Pep_mass_range.Right_value;
            dfp.Pep_length_range.Left_value = sfp.Pep_length_range.Left_value;
            dfp.Pep_length_range.Right_value = sfp.Pep_length_range.Right_value;
            dfp.Min_pep_num = sfp.Min_pep_num;
            dfp.Protein_Fdr = sfp.Protein_Fdr;
        }
        
        //copy QuantitationParam
        void Copy_Inter.LabelingCopy(Labeling slb, Labeling dlb)
        {
            dlb.Multiplicity_index = slb.Multiplicity_index;
            dlb.Multiplicity = slb.Multiplicity;
            
            dlb.Light_label.Clear();
            for (int i = 0; i < slb.Light_label.Count; i++)
            {
                dlb.Light_label.Add(slb.Light_label[i]);
            }        
            dlb.Medium_label.Clear();
            for (int i = 0; i < slb.Medium_label.Count; i++)
            {
                dlb.Medium_label.Add(slb.Medium_label[i]);
            }    
            dlb.Heavy_label.Clear();
            for (int i = 0; i < slb.Heavy_label.Count; i++)
            {
                dlb.Heavy_label.Add(slb.Heavy_label[i]);
            }
        }
        void Copy_Inter.QuantAdvancedCopy(Quant_Advanced sqad,Quant_Advanced dqad)
        {
            dqad.Number_scans_half_cmtg = sqad.Number_scans_half_cmtg;
            dqad.Ppm_for_calibration = sqad.Ppm_for_calibration;
            dqad.Ppm_half_win_accuracy_peak = sqad.Ppm_half_win_accuracy_peak;
            dqad.Number_hole_in_cmtg = sqad.Number_hole_in_cmtg;
            dqad.Type_same_start_end_between_evidence = sqad.Type_same_start_end_between_evidence;
            dqad.Ll_element_enrichment_calibration = sqad.Ll_element_enrichment_calibration;
            dqad.Extension_text_ms1 = sqad.Extension_text_ms1;
            dqad.Number_max_psm_per_block = sqad.Number_max_psm_per_block;
            dqad.Type_start = sqad.Type_start;
        }
        void Copy_Inter.QuantInferenceCopy(Quant_Inference sqinf, Quant_Inference dqinf)
        {
            dqinf.Type_peptide_ratio = sqinf.Type_peptide_ratio;
            dqinf.Type_protein_ratio_calculation = sqinf.Type_protein_ratio_calculation;
            dqinf.Type_unique_peptide_only = sqinf.Type_unique_peptide_only;
            dqinf.Threshold_score_interference = sqinf.Threshold_score_interference;
            dqinf.Threshold_score_intensity = sqinf.Threshold_score_intensity;
            dqinf.Type_get_group = sqinf.Type_get_group;
        }
        void Copy_Inter.QuantitaionCopy(QuantitationParam sq, QuantitationParam dq)
        {
            dq.Quantitation_type = sq.Quantitation_type;
            Factory.Create_Copy_Instance().LabelingCopy(sq.Labeling,dq.Labeling);
            Factory.Create_Copy_Instance().QuantAdvancedCopy(sq.Quant_advanced,dq.Quant_advanced);
            Factory.Create_Copy_Instance().QuantInferenceCopy(sq.Quant_inference,dq.Quant_inference);
            #region Todo
            //Label Free
            #endregion
        }
    
    }
}
