using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pFind.Interface;
using pFind.Function;

namespace pFind
{
    public class Factory
    {
        public static pParse_Inter Create_pParse_Instance() 
        {
            return new pParseWR_Func();     
        }

        public static pFind_Inter Create_pFind_Instance()
        {
            return new pFindWR_Func();
        }

        public static pQuant_Inter Create_pQuant_Instance()
        {
            return new pQuantWR_Func();
        }

        public static Run_Inter Create_Run_Instance()
        {
            return new Run_Func();
        }
        public static Copy_Inter Create_Copy_Instance()
        {
            return new Copy_Func();
        }
        public static Reset_Inter Create_Reset_Instance()
        {
            return new Reset_Func();
        }
        
    }
}
