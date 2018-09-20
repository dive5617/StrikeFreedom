using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pTop.classes;

namespace pTop.Interface
{
    public interface pParse_Inter
    {
        //write to pParse.para
        void pParse_write(_Task _task);
        //read from pParse.para
        void pParse_read(string task_path, ref _Task _task);
    }
}
