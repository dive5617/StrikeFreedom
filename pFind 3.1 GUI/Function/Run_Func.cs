using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using pFind.classes;
using pFind.Interface;

namespace pFind.Function
{
    class Run_Func:Run_Inter
    {                                   
        //save params
        bool Run_Inter.SaveParams(_Task _task)
        {
            try
            {
                string pathName = _task.Path.Trim();
                return SaveTask(pathName, _task);
            }
            catch(Exception exe)
            {
                throw new Exception(exe.Message);
            }
        }

        bool Run_Inter.SaveAsParams(string path,_Task _task)
        {
            try
            {
                return SaveTask(path, _task);
            }
            catch(Exception exe)
            {
                throw new Exception(exe.Message);
            }
        }
        //创建任务文件
        //wrm 2014/10/20:
        void CreateTaskFile(string path,_Task _task)
        {           
            FileStream tskst = new FileStream(path + "\\" + _task.Task_name + ".tsk", FileMode.Create, FileAccess.Write);
            StreamWriter tsksw = new StreamWriter(tskst, Encoding.Default);
            tsksw.WriteLine("pFind Studio Task File, Format Version 3.0");
            tsksw.WriteLine("# pFind 3.0");
            tsksw.Close();
            tskst.Close();
        }

        bool SaveTask(string path,_Task _task)
        {
            try
            {
                if (Directory.Exists(path))
                {

                    //迟浩：感觉现在对话框提示有点多了，只提示有修改的话需要保存即可，至于是否覆盖则不提示。尝试一下，如果有不妥，再修改

                    //String tmp = "The Directory Already Contains An Item Named '" + _task.Task_name + "' .\n Do you want to overwrite it ?";
                    //MessageBoxResult result = System.Windows.MessageBox.Show(tmp, "pFind", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                    //if (result == MessageBoxResult.Cancel)  //No
                    //{
                    //    return false;  //cancel, or rename the task and then save
                    //}
                }
                else
                {
                    Directory.CreateDirectory(path);
                    Directory.CreateDirectory(path + "\\param");
                    Directory.CreateDirectory(path + "\\result");
                }
                
                //wrm 2014/10/20:
                CreateTaskFile(path,_task);
                //System.IO.File.Create(path + "\\pFind.pFind");
                //generate pParse.para
                Factory.Create_pParse_Instance().pParse_write(_task);
                //generate pFind.pfd
                Factory.Create_pFind_Instance().pFind_write(_task);

                if (_task.T_File.File_format.Equals("raw"))
                {
                    Factory.Create_pQuant_Instance().pQuant_write(_task);
                }
                else
                {
                    if (System.IO.File.Exists(path + "\\param\\pQuant.cfg"))
                    {
                        System.IO.File.Delete(path + "\\param\\pQuant.cfg");
                    }
                }
                if (_task.T_MS2Quant.Enable_ms2quant)
                {
                    Factory.Create_pQuant_Instance().pIsobariQ_write(_task);
                }

                if (path[path.Length - 1] == '\\')
                {
                    path = path.Substring(0, path.Length - 1);
                }
                ConfigHelper.update_recent_task(Advanced.recent_tasks_file_path, path);
                return true;
            }
            catch (Exception exe)
            {
                MessageBox.Show(exe.Message + "\nSave Failed!");
                return false;
            }
        }

        void Run_Inter.JustSaveParams(_Task _task)
        {
            string pathName = _task.Path;
            
            Directory.CreateDirectory(pathName);
            CreateTaskFile(pathName,_task);
            Directory.CreateDirectory(pathName + "\\param");
            Directory.CreateDirectory(pathName + "\\result");
           
            if (_task.T_File.File_format.Equals("raw"))
            {
                //generate pParse.para,pQuant.qnt
                Factory.Create_pParse_Instance().pParse_write(_task);
                Factory.Create_pQuant_Instance().pQuant_write(_task);
            }
            //generate pFind.pfd
            Factory.Create_pFind_Instance().pFind_write(_task);
            if (_task.T_MS2Quant.Enable_ms2quant)
            {
                Factory.Create_pQuant_Instance().pIsobariQ_write(_task);
            }
        }
        //begin search
        //void Run_Inter.StartSearchWork(_Task _task)
        //{
        //    try
        //    {
        //        if (_task.Check_ok == false)
        //        {
        //            MessageBox.Show("The configuration is not completed!", "pFind", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        }
        //        else
        //        {
        //            Factory.Create_Run_Instance().SaveParams(_task);
        //            Process process = new Process();

        //            process.StartInfo.UseShellExecute = false;
        //            process.StartInfo.CreateNoWindow = true;
        //            process.StartInfo.RedirectStandardOutput = true;
        //            process.StartInfo.RedirectStandardError = true;

        //            if (_task.T_File.File_format.Equals("raw"))
        //            {
        //                for (int i = 0; i < _task.T_File.Data_file_list.Count; i++)
        //                {

        //                    string xtract = "-a -ms -m " + _task.T_File.Mz_decimal.ToString() + " -i " + _task.T_File.Intensity_decimal.ToString() + " \"" + _task.T_File.Data_file_list[i].FilePath.ToString() + "\"";
        //                    process.StartInfo.FileName = "xtract.exe";
        //                    process.StartInfo.Arguments = xtract;
        //                    if (process.Start())
        //                    { 
                               
        //                    }
        //                    process.WaitForExit();

        //                }

        //                process.StartInfo.FileName = "pParse.exe";
        //                process.StartInfo.Arguments = _task.Path + "param\\pParse.para";
        //                if (process.Start())
        //                { 
                        
        //                }
        //                process.WaitForExit();

        //            }
        //            process.StartInfo.FileName = "pFind.exe";
        //            process.StartInfo.Arguments = _task.Path + "param\\pFind.pfd";
        //            if (process.Start())
        //            { 
                    
        //            }
        //            process.WaitForExit();

        //            process.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.Write(ex.Message);
        //        System.Windows.MessageBox.Show("Sorry, there is something wrong with pFind!");
        //    } 
        //}

        
        //check params of the task
        bool Run_Inter.Check_Task(_Task _task)
        {
            File _file = _task.T_File;
            SearchParam _sp = _task.T_Search;
            FilterParam _fp = _task.T_Filter;
            QuantitationParam _qp = _task.T_Quantitation;
            //check file
            if (_file.File_format_index != (int)FormatOptions.MGF && _file.File_format_index != (int)FormatOptions.RAW)
            {                
                return false;
            }
            if (_file.Data_file_list == null || _file.Data_file_list.Count == 0)
            {               
                return false;
            }
            #region Todo
            //参数检查
            #endregion
            if (_file.Threshold.ToString() == "")
            {               
                return false;
            }
            //check search
            if (_sp.Ptl.Tl_value.ToString() == "" || _sp.Ftl.Tl_value.ToString() == "")
            {                
                return false;
            }
            if (_sp.Db.Db_name == "null" || _sp.Db.Db_path == "null")
            {
                return false;
            }
            //check filter
            if (_fp.Fdr.Fdr_value.ToString() == "")
            {              
                return false;
            }
            if (_fp.Pep_mass_range.Left_value.ToString() == "" || _fp.Pep_mass_range.Right_value.ToString() == "" || _fp.Pep_mass_range.Left_value > _fp.Pep_mass_range.Right_value)
            {               
                return false;
            }
            if (_fp.Pep_length_range.Left_value.ToString() == "" || _fp.Pep_length_range.Right_value.ToString() == "" || _fp.Pep_length_range.Left_value > _fp.Pep_length_range.Right_value)
            {             
                return false;
            }
            if (_fp.Min_pep_num.ToString() == "")
            {              
                return false;
            }
            //check Quantitation

            _task.Check_ok = true;

            return true;
        }
    }
}
