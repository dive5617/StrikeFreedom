using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pTop.classes;
using pTop.Interface;

namespace pTop.Function
{
    class Copy_Func:Copy_Inter
    {
        void Copy_Inter.pParseAdvancedCopy(pParse_Advanced spa,pParse_Advanced dpa)
        {
            dpa.Isolation_width = spa.Isolation_width;
            dpa.Mix_spectra = spa.Mix_spectra;
            dpa.Model = spa.Model;
            dpa.Threshold = spa.Threshold;
            dpa.Max_charge = spa.Max_charge;
            dpa.Max_mass = spa.Max_mass;
            dpa.Mz_tolerance = spa.Mz_tolerance;
            dpa.Sn_ratio = spa.Sn_ratio;
            
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
            
            Factory.Create_Copy_Instance().pParseAdvancedCopy(sf.Pparse_advanced,df.Pparse_advanced);
        }

        //copy SearchParam
        void Copy_Inter.SearchParamCopy(Identification ssp, Identification dsp)
        {
            dsp.Db_index = ssp.Db_index;
            dsp.Db.Db_name = string.Copy(ssp.Db.Db_name);
            dsp.Db.Db_path = string.Copy(ssp.Db.Db_path);
            dsp.Max_mod = ssp.Max_mod;
            
            dsp.Ptl.Tl_value = ssp.Ptl.Tl_value;
            dsp.Ptl.Isppm = ssp.Ptl.Isppm;
            dsp.Ftl.Tl_value = ssp.Ftl.Tl_value;
            dsp.Ftl.Isppm = ssp.Ftl.Isppm;
           
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
            dsp.Filter.Fdr_value = ssp.Filter.Fdr_value;
        }

       
    }
}
