using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pTop.Interface;
using pTop.Function;

namespace pTop
{
    public class Factory
    {
        public static pParse_Inter Create_pParse_Instance() 
        {
            return new pParseWR_Func();     
        }

        public static pTop_Inter Create_pTop_Instance()
        {
            return new pTopWR_Func();
        }

        public static Run_Inter Create_Run_Instance()
        {
            return new Run_Func();
        }

        public static Copy_Inter Create_Copy_Instance()
        {
            return new Copy_Func();
        }
        //public static Reset_Inter Create_Reset_Instance()
        //{
        //    return new Reset_Func();
        //}
        
    }
}
