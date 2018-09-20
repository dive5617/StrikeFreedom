using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pTop.classes;

namespace pTop.Interface
{
    public interface Run_Inter
    {
        //save params
        bool SaveParams(_Task _task);
        
        //begin search
        //void StartSearchWork(_Task _task);
      
        //check params of the task
        bool Check_Task( _Task _task);
    }
}
