using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pFind.Interface;
using pFind.classes;
using System.Collections.ObjectModel;

namespace pFind.Function
{
    class pParseWR_Func:pParse_Inter
    {
        void pParse_Inter.pParse_write(_Task _task)
        {
            /**产生pParse.cfg**/
            try
            {
                File _file = _task.T_File;
                string filepath = _task.Path + "param\\pParse.cfg";
                FileStream pPfst = new FileStream(filepath, FileMode.Create, FileAccess.Write);
                StreamWriter pPsw = new StreamWriter(pPfst, Encoding.Default);
                pPsw.WriteLine("# This is a standard pParse configure file");
                pPsw.WriteLine("# For help: mail to wulong@ict.ac.cn");
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
                pPsw.WriteLine();
                pPsw.WriteLine("[Advanced Options]");
                string co = _file.Mix_spectra == true ? "1" : "0";
                pPsw.WriteLine("co-elute=" + co);
                pPsw.WriteLine("# 0, output single precursor for single scan;");
                pPsw.WriteLine("# 1, output all co-eluted precursors.");
                pPsw.WriteLine("input_format="+_file.File_format);
                pPsw.WriteLine("# raw / ms1 /mgf");
                pPsw.WriteLine("isolation_width="+_file.Pparse_advanced.Isolation_width);
                pPsw.WriteLine("# 2 / 2.5 / 3 / 4");
                if (_file.Threshold.ToString() == "" || _file.Mz_decimal.ToString() == "" || _file.Intensity_decimal.ToString() == "")
                {
                    _task.Check_ok = false;
                }
                pPsw.WriteLine("mars_threshold=" + _file.Threshold.ToString());
                pPsw.WriteLine("ipv_file="+_file.Pparse_advanced.Ipv_file);     //.\\IPV.txt
                pPsw.WriteLine("trainingset="+_file.Pparse_advanced.Trainingset);   //.\\TrainingSet.txt
                pPsw.WriteLine();
                pPsw.WriteLine("[Internal Switches]");
                pPsw.WriteLine("output_mars_y="+_file.Pparse_advanced.Output_mars_y);
                pPsw.WriteLine("delete_msn="+(_file.Pparse_advanced.Output_msn == 1 ? 0 : 1));
                pPsw.WriteLine("output_mgf="+_file.Pparse_advanced.Output_mgf);   //generate MGF file
                pPsw.WriteLine("output_pf="+_file.Pparse_advanced.Output_pf);      //generate pF file
                pPsw.WriteLine("debug_mode="+_file.Pparse_advanced.Debug_mode);
                pPsw.WriteLine("check_activationcenter="+_file.Pparse_advanced.Check_activationcenter);
                pPsw.WriteLine("output_all_mars_y="+_file.Pparse_advanced.Output_all_mars_y);
                pPsw.WriteLine("rewrite_files="+_file.Pparse_advanced.Rewrite_files);
                pPsw.WriteLine("export_unchecked_mono="+_file.Pparse_advanced.Export_unchecked_mono);
                pPsw.WriteLine("cut_similiar_mono="+_file.Pparse_advanced.Cut_similiar_mono);
                pPsw.WriteLine("mars_model=" + _file.Model.ToString());                      //0    combobox                
                pPsw.WriteLine("output_trainingdata="+_file.Pparse_advanced.Output_trainingdata);
                pPsw.WriteLine();
                pPsw.WriteLine("########################################");
                pPsw.WriteLine("[About pXtract]");
                pPsw.WriteLine("m/z=" + _file.Mz_decimal.ToString());
                pPsw.WriteLine("Intensity=" + _file.Intensity_decimal.ToString());
                pPsw.Close();
                pPfst.Close();
            }
            catch(Exception exe)
            {
                throw new Exception(exe.Message);
            }
        }

        //read from pParse.para
        void pParse_Inter.readpParse_para(string task_path, ref _Task _task)
        {
            try
            {
                File _file = _task.T_File;
                FileStream fst = new FileStream(task_path + "\\param\\pParse.cfg", FileMode.Open);
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
                        }
                        #region
                        else if (subtitle.Equals("[Advanced Options]"))
                        {
                            if (strLine.Length > 8 && strLine.Substring(0, 8).Equals("co-elute"))
                            {
                                if (strLine.Substring(strLine.LastIndexOf("=") + 1).Equals("1"))
                                {
                                    _file.Mix_spectra = true;
                                }
                                else
                                {
                                    _file.Mix_spectra = false;
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
                                else if (_file.File_format.Equals("wiff"))
                                {
                                    _file.File_format_index = (int)FormatOptions.WIFF;
                                }
                                _file.Data_file_list.Clear();
                                for (int i = 0; i < dfl.Count; i++)
                                {
                                    _file.Data_file_list.Add(dfl[i]);
                                }
                            }
                            else if (strLine.Length > 15 && strLine.Substring(0, 15).Equals("isolation_width"))
                            { 
                                _file.Pparse_advanced.Isolation_width=double.Parse(strLine.Substring(strLine.LastIndexOf("=")+1).Trim());
                            }
                            else if (strLine.Length > 14 && strLine.Substring(0, 14).Equals("mars_threshold"))
                            {
                                _file.Threshold = double.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1).Trim());
                            }
                            else if (strLine.Length > 8 && strLine.Substring(0, 8).Equals("ipv_file"))
                            {
                                _file.Pparse_advanced.Ipv_file = strLine.Substring(strLine.LastIndexOf("=") + 1).Trim();
                            }
                            else if (strLine.Length > 11 && strLine.Substring(0, 11).Equals("trainingset"))
                            {
                                _file.Pparse_advanced.Trainingset = strLine.Substring(strLine.LastIndexOf("=") + 1).Trim();
                            }
                        }
                        #endregion
                        #region
                        else if (subtitle.Equals("[Internal Switches]"))
                        {
                            if (strLine.Length > 13 && strLine.Substring(0, 13).Equals("output_mars_y"))
                            {
                                _file.Pparse_advanced.Output_mars_y = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));                   
                            }
                            else if (strLine.Length > 10 && strLine.Substring(0, 10).Equals("delete_msn"))
                            {
                                _file.Pparse_advanced.Output_msn = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1)) == 0 ? 1 : 0;               
                            }
                            else if (strLine.Length > 10 && strLine.Substring(0, 10).Equals("output_mgf"))
                            {
                                _file.Pparse_advanced.Output_mgf = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > 9 && strLine.Substring(0, 9).Equals("output_pf"))
                            {
                                _file.Pparse_advanced.Output_pf = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > 10 && strLine.Substring(0, 10).Equals("debug_mode"))
                            {
                                _file.Pparse_advanced.Debug_mode = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > ("check_activationcenter").Length && strLine.Substring(0, ("check_activationcenter").Length).Equals("check_activationcenter"))
                            {
                                _file.Pparse_advanced.Check_activationcenter = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > ("output_all_mars_y").Length && strLine.Substring(0, ("output_all_mars_y").Length).Equals("output_all_mars_y"))
                            {
                                _file.Pparse_advanced.Output_all_mars_y = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > 13 && strLine.Substring(0, 13).Equals("rewrite_files"))
                            {
                                _file.Pparse_advanced.Rewrite_files = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > ("export_unchecked_mono").Length && strLine.Substring(0, ("export_unchecked_mono").Length).Equals("export_unchecked_mono"))
                            {
                                _file.Pparse_advanced.Export_unchecked_mono = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > ("cut_similiar_mono").Length && strLine.Substring(0, ("cut_similiar_mono").Length).Equals("cut_similiar_mono"))
                            {
                                _file.Pparse_advanced.Cut_similiar_mono = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > 10 && strLine.Substring(0, 10).Equals("mars_model"))
                            {
                                _file.Model = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                                _file.ModelIndex = ConfigHelper.mars_model.IndexOf(_file.Model);
                                //if the value is invalid
                                if (_file.ModelIndex == -1)
                                {
                                    _file.ModelIndex = 0;
                                    _file.Model=ConfigHelper.mars_model[_file.ModelIndex];
                                }
                            }
                            else if (strLine.Length > ("output_trainingdata").Length && strLine.Substring(0, ("output_trainingdata").Length).Equals("output_trainingdata"))
                            {
                                _file.Pparse_advanced.Output_trainingdata = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                        }
                        #endregion
                        else if (subtitle.Equals("[About pXtract]"))
                        {
                            if (strLine.Length > 3 && strLine.Substring(0, 3).Equals("m/z"))
                            {
                                _file.Mz_decimal = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                                _file.Mz_decimal_index = _file.Mz_decimal - 1;
                            }
                            else if (strLine.Length > 9 && (strLine.Substring(0, 9).Equals("intensity") || strLine.Substring(0, 9).Equals("Intensity")))
                            {
                                _file.Intensity_decimal = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                                _file.Intensity_decimal_index = _file.Intensity_decimal - 1;
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
