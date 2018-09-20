using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pTop.classes;

namespace pTop.Interface
{
    public interface pTop_Inter
    {
        //generate pFind parameter file
        void pTop_write(_Task _task);

        //read from pFind parameter file
        void pTop_read(string task_path,bool pParse,ref _Task _task);
    }
}
