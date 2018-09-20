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
        //generate pTop.pfd
        void pTop_Inter.pTop_write(_Task _task) 
        {
            try
            {
                pTop.classes.File _file = _task.T_File;
                Identification _search = _task.T_Identify;
      
                string filepath = _task.Path + "param\\pTop.cfg";
                FileStream pFfst = new FileStream(filepath, FileMode.Create, FileAccess.Write);
                StreamWriter pFsw = new StreamWriter(pFfst, Encoding.Default);
                pFsw.WriteLine("# This is a standard pTop configuration file");
                pFsw.WriteLine("# For help: mail to wangruimin@ict.ac.cn");
                pFsw.WriteLine("# Time: " + DateTime.Now.ToString());
                pFsw.WriteLine();
                pFsw.WriteLine("[Version]");
                pFsw.WriteLine("pTop_Version=EVA.1.2");
                pFsw.WriteLine();
                pFsw.WriteLine("[database]");
                pFsw.WriteLine("Database=" + _search.Db.Db_path);
                pFsw.WriteLine("pParseTD_cfg=" + _task.Path + "param\\pParseTD.cfg");
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
                pFsw.WriteLine("Max_mod=" + _search.Max_mod.ToString());
                pFsw.WriteLine("Modify_num=" + _search.Var_mods.Count.ToString());
                for (int i = 0; i < _search.Var_mods.Count; i++)
                {
                    if (_search.Var_mods[i].Trim().Length > 0)
                    {
                        pFsw.WriteLine("var_mod" + (i + 1).ToString() + "=" + _search.Var_mods[i]);
                    }
                }
                pFsw.WriteLine();

                
                pFsw.WriteLine();
                pFsw.WriteLine("[filter]");
                pFsw.WriteLine("threshold=" + (_search.Filter.Fdr_value / 100).ToString());
                pFsw.WriteLine();

                //pFsw.WriteLine("[output]");
                //pFsw.WriteLine("outputpath=" + _task.Path + "result\\");
                //pFsw.WriteLine("outputname=" + _task.Task_name);
                pFsw.WriteLine();

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
            //try
            {
                pTop.classes.File _file = _task.T_File;
                Identification _search = _task.T_Identify;

                FileStream fst = new FileStream(task_path + "\\param\\pTop.cfg", FileMode.Open);
                StreamReader sr = new StreamReader(fst, Encoding.Default);
                string strLine = sr.ReadLine();
                string subtitle = "";
                #region
                while (strLine != null)
                {
                    strLine = strLine.Trim();
                    if (strLine.Length > 0 && strLine[0] == '[' && strLine[strLine.Length - 1] == ']')
                    {
                        subtitle = strLine;
                    }
                    else
                    {
                        #region [database]
                        if (subtitle.Equals("[database]"))
                        {

                            if (strLine.Length > 9 && strLine.Substring(0, ("Database").Length).Equals("Database"))
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
                                    _search.Db=new DB();
                                }
                                #region Todo
                                //当不存在指定数据库时
                                #endregion
                            }
                        
                        }
                        #endregion
                        #region[spectrum]
                        if (subtitle.Equals("[spectrum]"))
                        {
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
                        #endregion

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
                            if (strLine.Length > ("Max_mod").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("Max_mod"))
                            {
                                _search.Max_mod = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
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
                            if (strLine.Length > ("threshold").Length && strLine.Substring(0, ("threshold").Length).Equals("threshold"))
                            {
                                _search.Filter.Fdr_value = double.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1))*100;
                            }
                        }
                       
                        else if (subtitle.Equals("[output]"))
                        {
                            if (strLine.Length > 10 && strLine.Substring(0, 10).Equals("outputpath"))
                            {
                                string tpath = strLine.Substring(strLine.LastIndexOf("=") + 1); 
                                if (tpath.Trim().Length > 0)
                                {
                                    _task.Path = task_path + "\\";
                                    _task.Task_name = sr.ReadLine().Trim().Substring(strLine.LastIndexOf("=") + 1);
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
            //catch(Exception exe)
            //{
            //    throw new Exception(exe.Message);
            //}
        }

    }
}
