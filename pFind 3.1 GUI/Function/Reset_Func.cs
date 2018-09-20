using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pFind.classes;
using pFind.Interface;


namespace pFind.Function
{
    //添加了模板任务template之后，Reset函数似乎已经不再需要了
    class Reset_Func:Reset_Inter
    {        
        //reset file
        void Reset_Inter.ResetFile(File fi)
        {
            fi.File_format_index = (int)FormatOptions.RAW;
            fi.setFileFormat();
            fi.Instrument_index = (int)InstrumentOptions.HCD_FTMS;
            fi.setInstrument();
            fi.Data_file_list.Clear();
            fi.Mix_spectra = true;
            fi.Mz_decimal_index = 4;
            fi.setMZDecimal();
            fi.Intensity_decimal_index = 0;
            fi.setIntensityDecimal();
            fi.Model = (int)ModelOptions.Normal;
            fi.ModelIndex = 0;
            fi.Threshold = -0.68;
        }
        //reset search
        void Reset_Inter.ResetSearch(SearchParam sp)
        {
            sp.Db_index = -1;
            sp.setDatabase();
            sp.Enzyme_index = 0;
            sp.setEnzyme();
            sp.Enzyme_Spec_index = 0;
            sp.setEnzymeSpec();
            sp.Cleavages = 3;      //default
            sp.Ptl.Tl_value = 20;
            sp.Ptl.Isppm = 1;
            sp.Ftl.Tl_value = 20;
            sp.Ftl.Isppm = 1;
            sp.Open_search = true;
            sp.Fix_mods.Clear();
            sp.Var_mods.Clear();
        }
        //reset filter
        void Reset_Inter.ResetFilter(FilterParam fp)
        {
            fp.Fdr.Fdr_value = 1.0;
            fp.Fdr.IsPeptides = 1;
            fp.Pep_length_range.Left_value = 6;
            fp.Pep_length_range.Right_value = 100;
            fp.Pep_mass_range.Left_value = 600;
            fp.Pep_mass_range.Right_value = 10000;
            fp.Min_pep_num = 1;
            fp.Protein_Fdr = 1.0;
        }
        //reset quantitation
        void Reset_Inter.ResetQuantatition(QuantitationParam qp)
        {
            qp.Quantitation_type = (int)Quant_Type.Labeling_None;
            qp.Labeling.Multiplicity_index = 0;   //multiplicity=1
            qp.Labeling.setMultiplicity();
            #region Todo
            //标记初始化
            #endregion
            qp.Labeling.Light_label.Clear();
            qp.Labeling.Medium_label.Clear();
            qp.Labeling.Medium_label.Add(ConfigHelper.labels[0]);
            qp.Labeling.Heavy_label.Clear();

            #region Todo
            //Labelfree
            #endregion
            qp.Lf = new LabelFree();
            qp.Quant_advanced.Number_scans_half_cmtg = 100;
            qp.Quant_advanced.Ppm_for_calibration = 0.0;
            qp.Quant_advanced.Ppm_half_win_accuracy_peak = 10.0;
            qp.Quant_advanced.Number_hole_in_cmtg = 1;    //index=1,value=2
            qp.Quant_advanced.Type_same_start_end_between_evidence = 0;
            qp.Quant_advanced.Ll_element_enrichment_calibration = (int)LL_ELEMENT_ENRICHMENT_CALIBRATION.None;
        }
    }
}
