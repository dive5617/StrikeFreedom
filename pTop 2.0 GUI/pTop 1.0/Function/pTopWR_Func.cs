using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using pTop.classes;
using pTop.Interface;

namespace pTop.Function
{
    class pTopWR_Func:pTop_Inter
    {
        //generate pTop.cfg
        void pTop_Inter.pTop_write(_Task _task) 
        {
            try
            {
                pTop.classes.File _file = _task.T_File;
                Identification _search = _task.T_Identify;
                Quantitation _quantitation = _task.T_Quantify;
      
                StreamReader sr = new StreamReader(@"pTop.ini", Encoding.Default);
                string strLine = sr.ReadLine();
                string thread = "";
                while (strLine != null)
                {
                    if (strLine.Length > 6 && strLine.Substring(0, 6).Equals("thread"))
                    {
                        thread = strLine.Substring(strLine.LastIndexOf("=") + 1);
                    }
                    if (thread != "") break;
                    strLine = sr.ReadLine();
                }

                string filepath = _task.Path + "param\\pTop.cfg";
                FileStream pFfst = new FileStream(filepath, FileMode.Create, FileAccess.Write);
                StreamWriter pFsw = new StreamWriter(pFfst, Encoding.Default);
                pFsw.WriteLine("# This is a standard pTop configuration file");
                pFsw.WriteLine("# For help: mail to wangruimin@ict.ac.cn");
                pFsw.WriteLine("# Time: " + DateTime.Now.ToString());
                pFsw.WriteLine();
                pFsw.WriteLine("[Version]");
                pFsw.WriteLine("pTop_Version=EVA.2.0");
                pFsw.WriteLine();
                
                pFsw.WriteLine("[spectrum]");
                pFsw.WriteLine("msmsnum=" + _file.Data_file_list.Count.ToString());
                for (int i = 0; i < _file.Data_file_list.Count; i++)
                {
                    string mgfpath = _file.Data_file_list[i].FilePath;
                    pFsw.WriteLine("msmspath" + (i + 1).ToString() + "=" + mgfpath);
                }
                pFsw.WriteLine("input_format=" + _file.File_format);
                pFsw.WriteLine("Activation=" + _file.Instrument);
                //母离子误差指定为 Da
                pFsw.WriteLine("Precursor_Tolerance=" + _search.Ptl.Tl_value.ToString());
                //pFsw.WriteLine("mstolppm=" + _search.Ptl.Isppm.ToString());
                //碎片离子误差指定为 ppm
                pFsw.WriteLine("Fragment_Tolerance=" + _search.Ftl.Tl_value.ToString());
                //pFsw.WriteLine("msmstolppm=" + _search.Ftl.Isppm.ToString());               
                pFsw.WriteLine();

                pFsw.WriteLine("[param]");
                pFsw.WriteLine("thread_num=" + thread);
                pFsw.WriteLine("workflow=" + _search.SearchModeIndex.ToString());
                pFsw.WriteLine("# 0 tag flow, 1 ion flow");
                pFsw.WriteLine("output_top_k=" + _search.OutputTopK.ToString());
                pFsw.WriteLine("max_truncated_mass=" + _search.MaxTruncatedMass.ToString());
                pFsw.WriteLine("second_search=" + (_search.SecondSearch ? "1" : "0"));
                pFsw.WriteLine();

                pFsw.WriteLine("[fixmodify]");
                pFsw.WriteLine("fixedModify_num=" + _search.Fix_mods.Count.ToString());
                for (int i = 0; i < _search.Fix_mods.Count; i++)
                {
                    if (_search.Fix_mods[i].Trim().Length > 0)
                    {
                        pFsw.WriteLine("fix_mod" + (i + 1).ToString() + "=" + _search.Fix_mods[i]);
                    }
                }
                pFsw.WriteLine();
                // 可变修饰参数
                pFsw.WriteLine("[modify]");
                pFsw.WriteLine("Max_mod_num=" + _search.Max_mod.ToString());
                pFsw.WriteLine("unexpected_mod=" + _search.UnexpectedModNum.ToString());
                pFsw.WriteLine("Max_mod_mass=" + _search.MaxModMass.ToString());
                pFsw.WriteLine("Modify_num=" + _search.Var_mods.Count.ToString());
                for (int i = 0; i < _search.Var_mods.Count; i++)
                {
                    if (_search.Var_mods[i].Trim().Length > 0)
                    {
                        pFsw.WriteLine("var_mod" + (i + 1).ToString() + "=" + _search.Var_mods[i]);
                    }
                }
                pFsw.WriteLine();

                pFsw.WriteLine("[filter]");
                pFsw.WriteLine("threshold=" + (_search.Filter.Fdr_value / 100).ToString());
                pFsw.WriteLine("separate_filtering=" + (_search.Filter.SeparateFiltering ? "1" : "0"));
                pFsw.WriteLine();

                pFsw.WriteLine("[file]");
                pFsw.WriteLine("Database=" + _search.Db.Db_path);
                pFsw.WriteLine("pParseTD_cfg=" + _task.Path + "param\\pParseTD.cfg");
                if (_quantitation.Quantitation_type != (int)Quant_Type.Label_None)
                {
                    pFsw.WriteLine("pQuant_cfg=" + _task.Path + "param\\pQuant.cfg");
                }
                else
                {
                    pFsw.WriteLine("pQuant_cfg=");
                }
                pFsw.WriteLine("outputpath=" + _task.Path);
                pFsw.WriteLine();

                #region Quant
                pFsw.WriteLine("[quantify]");
                if (_quantitation.Quantitation_type == (int)Quant_Type.Label_None)
                {
                    pFsw.WriteLine("quant=1|none");
                }
                else // quantify based on MS: Quant_Type.Labeling_15N || Quant_Type.Labeling_Dimethyl || Quant_Type.Labeling_SILAC || others
                {
                    string labels = "quant=" + _quantitation.Labeling.Multiplicity.ToString();    //Multiplicity
                    if (_quantitation.Labeling.Multiplicity == 1)  // 1种标记
                    {
                        if (_quantitation.Labeling.Medium_label.Count == 0)
                        {
                            labels += "|none";
                        }
                        else
                        {
                            labels += "|" + _quantitation.Labeling.Medium_label[0];
                        }
                    }
                    else if (_quantitation.Labeling.Multiplicity == 2) // 2种标记
                    {
                        if (_quantitation.Labeling.Light_label.Count == 0)   //light label
                        {
                            labels += "|none";
                        }
                        else
                        {
                            labels += "|" + _quantitation.Labeling.Light_label[0];
                        }
                        if (_quantitation.Labeling.Heavy_label.Count == 0)   //heavy label
                        {
                            labels += "|none";
                        }
                        else
                        {
                            labels += "|" + _quantitation.Labeling.Heavy_label[0];
                        }
                    }
                    else if (_quantitation.Labeling.Multiplicity == 3) // 3种标记
                    {
                        if (_quantitation.Labeling.Light_label.Count == 0)   //light label
                        {
                            labels += "|none";
                        }
                        else
                        {
                            labels += "|" + _quantitation.Labeling.Light_label[0];
                        }
                        if (_quantitation.Labeling.Medium_label.Count == 0)   //medium label
                        {
                            labels += "|none";
                        }
                        else
                        {
                            labels += "|" + _quantitation.Labeling.Medium_label[0];
                        }
                        if (_quantitation.Labeling.Heavy_label.Count == 0)   //heavy label
                        {
                            labels += "|none";
                        }
                        else
                        {
                            labels += "|" + _quantitation.Labeling.Heavy_label[0];
                        }
                    }
                    pFsw.WriteLine(labels);
                }
                pFsw.WriteLine();
                #endregion

                pFsw.WriteLine("[system]");
                pFsw.WriteLine("log=LOG_INFO");
                pFsw.Close();
                pFfst.Close();
            }
            catch(Exception exe)
            {
                throw new Exception(exe.Message);
            }
        }

        //read from pTop.pfd
        void pTop_Inter.pTop_read(string task_path, bool pParse, ref _Task _task)
        {
            string strLine = "";
            try
            {
                pTop.classes.File _file = _task.T_File;
                Identification _search = _task.T_Identify;
                Quantitation _quantitation = _task.T_Quantify;

                FileStream fst = new FileStream(task_path + "\\param\\pTop.cfg", FileMode.Open);
                StreamReader sr = new StreamReader(fst, Encoding.Default);
                strLine = sr.ReadLine();
                string subtitle = "";
                #region
                while (strLine != null)
                {
                    strLine = strLine.Trim();
                    if (strLine.Length > 0 && strLine[0] == '[' && strLine[strLine.Length - 1] == ']')
                    {
                        subtitle = strLine;
                    }
                    else if (strLine.Length > 0 && strLine.IndexOf("=") > -1)
                    {
                        if (subtitle.Equals("[spectrum]"))
                        {
                            // msmspath and input_format were read from pParseTD.cfg
                            if (strLine.Length > ("Activation").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("Activation"))
                            {
                                _file.Instrument = strLine.Substring(strLine.LastIndexOf("=") + 1);
                                _file.setInstrument_index();
                            }
                            else  if (strLine.Length > ("Precursor_Tolerance").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("Precursor_Tolerance"))
                            {
                                _search.Ptl.Tl_value = double.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > ("Fragment_Tolerance").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("Fragment_Tolerance"))
                            {
                                _search.Ftl.Tl_value = double.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            
                        }
                        else if(subtitle.Equals("[param]"))
                        {
                            if(strLine.Length > ("workflow").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("workflow"))
                            {
                                 _search.SearchModeIndex = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if(strLine.Length > ("output_top_k").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("output_top_k"))
                            {
                                _search.OutputTopK = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > ("max_truncated_mass").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("max_truncated_mass"))
                            {
                                _search.MaxTruncatedMass = double.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > ("second_search").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("second_search"))
                            {
                                if (strLine.Substring(strLine.LastIndexOf("=") + 1).Equals("1"))
                                {
                                    _search.SecondSearch = true;
                                }
                                else
                                {
                                    _search.SecondSearch = false;
                                }
                            }
                        }

                        else if (subtitle.Equals("[fixmodify]"))
                        {
                            if (strLine.Length > ("fixedModify_num").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("fixedModify_num"))
                            {
                                int fix_num = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                                _search.Fix_mods.Clear();
                                for (int i = 0; i < fix_num; i++)
                                {
                                    strLine = sr.ReadLine();
                                    string fixmod = strLine.Substring(strLine.LastIndexOf("=") + 1);
                                    _search.Fix_mods.Add(fixmod);
                                }
                            }
                            
                        }
                        
                        else if (subtitle.Equals("[modify]"))
                        {

                            if (strLine.Length > ("Max_mod_num").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("Max_mod_num"))
                            {
                                _search.Max_mod = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > ("unexpected_mod").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("unexpected_mod"))
                            {
                                _search.UnexpectedModNum = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > ("Max_mod_mass").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("Max_mod_mass"))
                            {
                                _search.MaxModMass = double.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > ("Modify_num").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("Modify_num"))
                            {
                                int var_num = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                                _search.Var_mods.Clear();
                                for (int i = 0; i < var_num; i++)
                                {
                                    strLine = sr.ReadLine();
                                    string varmod = strLine.Substring(strLine.LastIndexOf("=") + 1);
                                    _search.Var_mods.Add(varmod);
                                }
                            }
                        }  
                        else if (subtitle.Equals("[filter]"))
                        {
                            if (strLine.Length > ("threshold").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("threshold"))
                            {
                                _search.Filter.Fdr_value = double.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1))*100;
                            }
                            else if (strLine.Length > ("separate_filtering").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("separate_filtering"))
                            {

                                if (strLine.Substring(strLine.LastIndexOf("=") + 1).Equals("1"))
                                {
                                    _search.Filter.SeparateFiltering = true;
                                }
                                else
                                {
                                    _search.Filter.SeparateFiltering = false;
                                }
                            }
                        }

                        #region [file]
                        if (subtitle.Equals("[file]"))
                        {

                            if (strLine.Length > ("Database").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("Database"))
                            {
                                string db_path = strLine.Substring(strLine.LastIndexOf("=") + 1);
                                if (db_path.Trim().Length > 0)
                                {
                                    DB db = new DB();
                                    db.Db_path = db_path;
                                    if (ConfigHelper.ReDBmap.Contains(db.Db_path))
                                    {
                                        db.Db_name = ConfigHelper.ReDBmap[db.Db_path].ToString();
                                        _search.Db = db;
                                        _search.Db_index = _search.setDatabaseIndex();
                                    }
                                    else     //the database is damaged
                                    {
                                        _search.Db_index = -1;
                                        _search.Db = db;
                                    }
                                }
                                else   //when new a task
                                {
                                    _search.Db_index = -1;
                                    _search.Db = new DB();
                                }
                                #region Todo
                                //当不存在指定数据库时
                                #endregion
                            }
                            else if (strLine.Length > ("pParseTD_cfg").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("pParseTD_cfg"))
                            {

                            }
                            else if (strLine.Length > ("pQuant_cfg").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("pQuant_cfg"))
                            {

                            }
                            else if (strLine.Length > ("outputpath").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("outputpath"))
                            {
                                string tpath = strLine.Substring(strLine.LastIndexOf("=") + 1);

                                if (tpath.Trim().Length > 0)
                                {
                                    if (task_path.EndsWith("\\"))
                                    {
                                        task_path = task_path.Substring(0, task_path.Length - 1);
                                    }
                                    _task.Path = task_path + "\\";
                                    _task.Task_name = task_path.Substring(task_path.LastIndexOf("\\") + 1);
                                }
                            }

                        }
                        #endregion

                        else if (subtitle.Equals("[quantify]"))
                        {
                            if (strLine.Length > 5 && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("quant"))
                            {
                                string qt = strLine.Substring(strLine.LastIndexOf("=") + 1);
                                string[] lb = qt.Split('|');
                                int lbcount = lb.GetUpperBound(0);
                                int lbnum = int.Parse(lb[0]);
                                #region 如果是MGF文件怎么办
                                #endregion
                                //labeling
                                if (lbnum == 1 && (lb[1].Equals("none") || lb[1].Equals("None")))
                                {
                                    _quantitation.Quantitation_type = (int)Quant_Type.Label_None;
                                    
                                }
                                else
                                {
                                    _quantitation.Quantitation_type = (int)Quant_Type.Labeling;    //if 15N, it will be changed in pQuant_Read
                                    _quantitation.Labeling.Multiplicity = lbnum;
                                    _quantitation.Labeling.Multiplicity_index = lbnum - 1;
                                    if (lbnum == 1)
                                    {
                                        /*
                                        string[] lbs = lb[1].Split(',');
                                        foreach (string c in lbs)
                                        {
                                            if (c.Trim() != ""&&c.ToLower()!="none")
                                            {
                                                _quantitation.Labeling.Medium_label.Add(c.Trim());
                                            }
                                        }*/
                                        _quantitation.Labeling.Medium_label.Add(lb[1]);
                                    }
                                    else if (lbnum == 2)
                                    {
                                        _quantitation.Labeling.Light_label.Add(lb[1]);

                                        _quantitation.Labeling.Heavy_label.Add(lb[2]);
                                    }
                                    else if (lbnum == 3)
                                    {
                                        _quantitation.Labeling.Light_label.Add(lb[1]);

                                        _quantitation.Labeling.Medium_label.Add(lb[2]);

                                        _quantitation.Labeling.Heavy_label.Add(lb[3]);
                                    }
                                }
                            }
                        }
                    }
                    strLine = sr.ReadLine();
                }
                #endregion
                sr.Close();
                fst.Close();
            }
            catch (Exception exe)
            {
                throw new Exception("[pTop_read] line: " + strLine + "\n" + exe.Message);
            }
        }

    }
}
