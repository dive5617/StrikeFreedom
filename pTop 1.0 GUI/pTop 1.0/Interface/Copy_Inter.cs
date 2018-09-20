using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pTop.classes;

namespace pTop.Interface
{
    public interface Copy_Inter
    {
        //copy File
        void pParseAdvancedCopy(pParse_Advanced spa,pParse_Advanced dpa);
        void FileCopy(File sf, File df);
        //copy SearchParam
        void SearchParamCopy(Identification ssp,Identification dsp);
        
        

    }
}
