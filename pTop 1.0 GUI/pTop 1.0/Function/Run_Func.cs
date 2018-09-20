using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using pTop.classes;
using pTop.Interface;

namespace pTop.Function
{
    class Run_Func:Run_Inter
    {                                   
        //save params
        bool Run_Inter.SaveParams(_Task _task)
        {          
                string pathName = _task.Path.Trim();
                return SaveTask(pathName, _task);                
        }

 
        //创建任务文件

        void CreateTaskFile(string path,_Task _task)
        {           
            FileStream tskst = new FileStream(path + "\\" + _task.Task_name + ".tsk", FileMode.Create, FileAccess.Write);
            StreamWriter tsksw = new StreamWriter(tskst, Encoding.Default);
            tsksw.WriteLine("pTop Task File, Format Version " + ConfigHelper.ptop_version);
            tsksw.WriteLine("# pTop "+ConfigHelper.ptop_version);
            tsksw.Close();
            tskst.Close();
        }

        bool SaveTask(string path,_Task _task)
        {
            try
            {

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                if (!Directory.Exists(path + "\\param"))
                {
                    Directory.CreateDirectory(path + "\\param");
                }
                //Directory.CreateDirectory(path + "\\result");


                //CreateTaskFile(path,_task); // comment by luolan @20150610
                
                //generate pParse.cfg
                Factory.Create_pParse_Instance().pParse_write(_task);
                Factory.Create_pTop_Instance().pTop_write(_task);
                
                return true;
            }
            catch (Exception exe)
            {
                MessageBox.Show(exe.Message + "\nSave Failed!");
                return false;
            }
        }

        
        //check params of the task
        bool Run_Inter.Check_Task(_Task _task)
        {
        //    File _file = _task.T_File;
        //    SearchParam _sp = _task.T_Search;
        //    FilterParam _fp = _task.T_Filter;
        //    QuantitationParam _qp = _task.T_Quantitation;
        //    //check file
        //    if (_file.File_format_index != (int)FormatOptions.MGF && _file.File_format_index != (int)FormatOptions.RAW)
        //    {                
        //        return false;
        //    }
        //    if (_file.Data_file_list == null || _file.Data_file_list.Count == 0)
        //    {               
        //        return false;
        //    }
        //    #region Todo
        //    //参数检查
        //    #endregion
        //    if (_file.Threshold.ToString() == "")
        //    {               
        //        return false;
        //    }
        //    //check search
        //    if (_sp.Ptl.Tl_value.ToString() == "" || _sp.Ftl.Tl_value.ToString() == "")
        //    {                
        //        return false;
        //    }
        //    if (_sp.Db.Db_name == "null" || _sp.Db.Db_path == "null")
        //    {
        //        return false;
        //    }
        //    //check filter
        //    if (_fp.Fdr.Fdr_value.ToString() == "")
        //    {              
        //        return false;
        //    }
        //    if (_fp.Pep_mass_range.Left_value.ToString() == "" || _fp.Pep_mass_range.Right_value.ToString() == "" || _fp.Pep_mass_range.Left_value > _fp.Pep_mass_range.Right_value)
        //    {               
        //        return false;
        //    }
        //    if (_fp.Pep_length_range.Left_value.ToString() == "" || _fp.Pep_length_range.Right_value.ToString() == "" || _fp.Pep_length_range.Left_value > _fp.Pep_length_range.Right_value)
        //    {             
        //        return false;
        //    }
        //    if (_fp.Min_pep_num.ToString() == "")
        //    {              
        //        return false;
        //    }
        //    //check Quantitation
        //    _task.Check_ok = true;
            return true;
        }
    }
}
