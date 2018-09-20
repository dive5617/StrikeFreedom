using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pTop.Interface;
using pTop.classes;
using System.Collections.ObjectModel;

namespace pTop.Function
{
    class pParseWR_Func:pParse_Inter
    {
        void pParse_Inter.pParse_write(_Task _task)
        {
            /**产生pParse.cfg**/
            try
            {
                pTop.classes.File _file = _task.T_File;
                string filepath = _task.Path + "param\\pParseTD.cfg";
                FileStream pPfst = new FileStream(filepath, FileMode.Create, FileAccess.Write);
                StreamWriter pPsw = new StreamWriter(pPfst, Encoding.Default);
                pPsw.WriteLine("# This is a standard pParseTD configuration file");
                pPsw.WriteLine("# For help: mail to wangruimin@ict.ac.cn");
                pPsw.WriteLine("# Time: " + DateTime.Now.ToString());
                pPsw.WriteLine();
                pPsw.WriteLine("[Basic Options]");
                pPsw.WriteLine("datanum=" + _file.Data_file_list.Count.ToString());
                if (_file.Data_file_list.Count == 0)
                {
                    _task.Check_ok = false;
                }
                for (int i = 0; i < _file.Data_file_list.Count; i++)
                {
                    pPsw.WriteLine("datapath" + (i + 1).ToString() + "=" + _file.Data_file_list[i].FilePath);
                }
                pPsw.WriteLine("input_format=" + (_file.File_format_index == (int)FormatOptions.RAW ? "raw":"mgf"));
                pPsw.WriteLine();
                pPsw.WriteLine("[Advanced Options]");
                pPsw.WriteLine("max_charge=" + _file.Pparse_advanced.Max_charge);
                pPsw.WriteLine("max_mass=" + _file.Pparse_advanced.Max_mass);
                pPsw.WriteLine("SN_threshold=" + _file.Pparse_advanced.Sn_ratio);
                pPsw.WriteLine("mz_error_tolerance=" + _file.Pparse_advanced.Mz_tolerance);
                pPsw.WriteLine();
                string co = _file.Pparse_advanced.Mix_spectra == true ? "1" : "0";
                pPsw.WriteLine("co-elute=" + co);
                //pPsw.WriteLine("# 0, output single precursor for single scan;");
                //pPsw.WriteLine("# 1, output all co-eluted precursors.");
                pPsw.WriteLine("isolation_width=" + _file.Pparse_advanced.Isolation_width);               
                pPsw.WriteLine();
                //pPsw.WriteLine("model_type=" +( _file.Pparse_advanced.Model == (int)ModelOptions.SVM ? "svm" : "mars"));
                //pPsw.WriteLine("mars_threshold=" + _file.Pparse_advanced.Threshold); 
                pPsw.WriteLine();
                if (_file.Pparse_advanced.Threshold.ToString() == "" || _file.Pparse_advanced.Mz_decimal.ToString() == "" || _file.Pparse_advanced.Intensity_decimal.ToString() == "")
                {
                    _task.Check_ok = false;
                }
                
                pPsw.WriteLine("[About pXtract]");
                pPsw.WriteLine("m/z=" + _file.Pparse_advanced.Mz_decimal.ToString());
                pPsw.WriteLine("Intensity=" + _file.Pparse_advanced.Intensity_decimal.ToString());
                pPsw.Close();
                pPfst.Close();
            }
            catch(Exception exe)
            {
                throw new Exception(exe.Message);
            }
        }

        //read from pParse.para
        void pParse_Inter.pParse_read(string task_path, ref _Task _task)
        {
            try
            {
                pTop.classes.File _file = _task.T_File;
                FileStream fst = new FileStream(task_path + "\\param\\pParseTD.cfg", FileMode.Open);
                StreamReader sr = new StreamReader(fst, Encoding.Default);
                string strLine = sr.ReadLine();
                string subtitle = "";
                ObservableCollection<_FileInfo> dfl = new ObservableCollection<_FileInfo>();
                #region
                while (strLine != null)
                {
                    strLine = strLine.Trim();
                    if (strLine.Length > 0 && strLine.StartsWith("#"))
                    {
                        
                    }
                    else if (strLine.Length > 0 && strLine[0] == '[' && strLine[strLine.Length - 1] == ']')
                    {
                        subtitle = strLine;
                    }
                    else
                    {
                        if (subtitle.Equals("[Basic Options]"))
                        {
                            if (strLine.Length > 7 && strLine.Substring(0, 7).Equals("datanum"))
                            {
                                //if new a task, then dtnum=0
                                int dtnum = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                                for (int i = 0; i < dtnum; i++)
                                {
                                    strLine = sr.ReadLine();
                                    strLine = strLine.Substring(strLine.LastIndexOf("=") + 1);
                                    if (System.IO.File.Exists(@strLine))
                                    {
                                        _FileInfo _fi = new _FileInfo(strLine);
                                        dfl.Add(_fi);
                                    }
                                    else
                                    {
                                        System.Windows.MessageBox.Show("\"" + strLine + "\" does not exist.");
                                    }
                                }
                            }
                            else if (strLine.Length > 12 && strLine.Substring(0, 12).Equals("input_format"))
                            {
                                _file.File_format = strLine.Substring(strLine.LastIndexOf("=") + 1);
                                if (_file.File_format.Equals("raw"))    //raw
                                {
                                    _file.File_format_index = (int)FormatOptions.RAW;    //format改变会引起Data_file_list的清空                           
                                }
                                else if (_file.File_format.Equals("mgf"))
                                {
                                    _file.File_format_index = (int)FormatOptions.MGF;
                                }
                                _file.Data_file_list.Clear();
                                for (int i = 0; i < dfl.Count; i++)
                                {
                                    _file.Data_file_list.Add(dfl[i]);
                                }
                            }
                        }
                        #region
                        else if (subtitle.Equals("[Advanced Options]"))
                        {
                            if (strLine.Length > ("max_charge").Length && strLine.Substring(0, ("max_charge").Length).Equals("max_charge"))
                            {
                                _file.Pparse_advanced.Max_charge = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1).Trim());
                            }
                            else if (strLine.Length > ("max_mass").Length && strLine.Substring(0, ("max_mass").Length).Equals("max_mass"))
                            {
                                _file.Pparse_advanced.Max_mass = double.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1).Trim());
                            }
                            else if (strLine.Length > ("SN_threshold").Length && strLine.Substring(0, ("SN_threshold").Length).Equals("SN_threshold"))
                            {
                                _file.Pparse_advanced.Sn_ratio = double.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1).Trim());
                            }
                            else if (strLine.Length > ("mz_error_tolerance").Length && strLine.Substring(0, ("mz_error_tolerance").Length).Equals("mz_error_tolerance"))
                            {
                                _file.Pparse_advanced.Mz_tolerance = double.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1).Trim());
                            }
                            else if (strLine.Length > 8 && strLine.Substring(0, 8).Equals("co-elute"))
                            {
                                if (strLine.Substring(strLine.LastIndexOf("=") + 1).Equals("1"))
                                {
                                    _file.Pparse_advanced.Mix_spectra = true;
                                }
                                else
                                {
                                    _file.Pparse_advanced.Mix_spectra = false;
                                }
                            }
                            
                            else if (strLine.Length > 15 && strLine.Substring(0, 15).Equals("isolation_width"))
                            { 
                                _file.Pparse_advanced.Isolation_width=double.Parse(strLine.Substring(strLine.LastIndexOf("=")+1).Trim());
                            }
                            else if (strLine.Length > ("model_type").Length && strLine.Substring(0, ("model_type").Length).Equals("model_type"))
                            {
                                _file.Pparse_advanced.Model = strLine.Substring(strLine.LastIndexOf("=") + 1).Trim().ToLower().Equals("svm") ? (int)ModelOptions.SVM : (int)ModelOptions.MARS;
                            }
                            else if (strLine.Length > 14 && strLine.Substring(0, 14).Equals("mars_threshold"))
                            {
                                _file.Pparse_advanced.Threshold = double.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1).Trim());
                            }
                                            
                        }
                        #endregion
                      
                        else if (subtitle.Equals("[About pXtract]"))
                        {
                            if (strLine.Length > 3 && strLine.Substring(0, 3).Equals("m/z"))
                            {
                                _file.Pparse_advanced.Mz_decimal = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                                _file.Pparse_advanced.Mz_decimal_index = _file.Pparse_advanced.Mz_decimal - 1;
                            }
                            else if (strLine.Length > 9 && (strLine.Substring(0, 9).Equals("intensity") || strLine.Substring(0, 9).Equals("Intensity")))
                            {
                                _file.Pparse_advanced.Intensity_decimal = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                                _file.Pparse_advanced.Intensity_decimal_index = _file.Pparse_advanced.Intensity_decimal - 1;
                            }
                        }
                    }
                    strLine = sr.ReadLine();
                }
                sr.Close();
                fst.Close();
                #endregion
            }
            catch (Exception exe)
            {
                throw new Exception(exe.Message);
            }
        }
    }
}
