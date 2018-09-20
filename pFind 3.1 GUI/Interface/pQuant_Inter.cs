using pFind.classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pFind.Interface
{
    public interface pQuant_Inter
    {
        //generate pQuant.qnt
        void pQuant_write(_Task _task);

        void pQuant_writeResultFile(_Task _task);

        //read from pQuant.qnt
        void readpQuant_qnt(string task_path, ref _Task _task);

        //read from pIsobariQ param
        void pIsobariQ_read(string task_path, ref _Task _task);
        //generate pIsobariQ.cfg
        void pIsobariQ_write(_Task _task);
    }
}
