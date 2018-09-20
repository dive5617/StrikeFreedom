using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pFind.classes;

namespace pFind.Interface
{
    public interface Copy_Inter
    {
        //copy File
        void pParseAdvancedCopy(pParse_Advanced spa,pParse_Advanced dpa);
        void FileCopy(File sf, File df);
        //copy SearchParam
        void SearchParamCopy(SearchParam ssp,SearchParam dsp);
        void SearchAdvancedCopy(Search_Advanced ssa,Search_Advanced dsa);
        //copy FilterParam
        void FilterParamCopy(FilterParam sfp,FilterParam dfp);
        //copy QuantitationParam
        void LabelingCopy(Labeling slb,Labeling dlb);
        void QuantAdvancedCopy(Quant_Advanced sqad,Quant_Advanced dqad);
        void QuantInferenceCopy(Quant_Inference sqinf, Quant_Inference dqinf);
        void QuantitaionCopy(QuantitationParam sq,QuantitationParam dq);

        T DeepCopyWithXmlSerializer<T>(T obj);

    }
}
