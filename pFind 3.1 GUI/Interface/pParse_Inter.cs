using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pFind.classes;

namespace pFind.Interface
{
    public interface pParse_Inter
    {
        //write to pParse.para
        void pParse_write(_Task _task);
        //read from pParse.para
        void readpParse_para(string task_path, ref _Task _task);
    }
}
