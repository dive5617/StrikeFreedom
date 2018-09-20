using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pFind.classes;

namespace pFind.Interface
{
    public interface pFind_Inter
    {
        //generate pFind parameter file
        void pFind_write(_Task _task);

        //read from pFind parameter file
        void readpFind_pfd(string task_path,bool pParse,ref _Task _task);
    }
}
