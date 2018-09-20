using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using pFind.classes;
using pFind.Interface;

namespace pFind.Function
{
    class pFindWR_Func:pFind_Inter
    {
        //generate pFind.pfd
        void pFind_Inter.pFind_write(_Task _task) 
        {
            try
            {
                File _file = _task.T_File;
                SearchParam _search = _task.T_Search;
                FilterParam _filter = _task.T_Filter;
                QuantitationParam _quantitation = _task.T_Quantitation;
                StreamReader sr = new StreamReader(@"pFind.ini", Encoding.Default);
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
                string filepath = _task.Path + "param\\pFind.cfg";
                FileStream pFfst = new FileStream(filepath, FileMode.Create, FileAccess.Write);
                StreamWriter pFsw = new StreamWriter(pFfst, Encoding.Default);
                pFsw.WriteLine("# This is a standard pFind configure file");
                pFsw.WriteLine("# For help: mail to chihao@ict.ac.cn");
                pFsw.WriteLine("# Time: " + DateTime.Now.ToString());
                pFsw.WriteLine();
                pFsw.WriteLine("[Version]");
                pFsw.WriteLine("pFind_Version=EVA.3.0.11");
                pFsw.WriteLine();
                pFsw.WriteLine("[param]");
                pFsw.WriteLine("thread=" + thread);
                pFsw.WriteLine("activation_type=" + _file.Instrument);
                pFsw.WriteLine("mstol=" + _search.Ptl.Tl_value.ToString());
                pFsw.WriteLine("mstolppm=" + _search.Ptl.Isppm.ToString());
                pFsw.WriteLine("msmstol=" + _search.Ftl.Tl_value.ToString());
                pFsw.WriteLine("msmstolppm=" + _search.Ftl.Isppm.ToString());
                pFsw.WriteLine("temppepnum="+_search.Search_advanced.Temppepnum.ToString());   //中间结果数目，Hidden parameters
                pFsw.WriteLine("pepnum="+_search.Search_advanced.Pepnum.ToString());       //number of output_peptides
                pFsw.WriteLine("selectpeak="+_search.Search_advanced.Selectpeak.ToString());    //预处理保存谱峰数目,Hidden parameters
                pFsw.WriteLine("maxprolen="+_search.Search_advanced.Maxprolen.ToString()); //切分数据库长度限制,Hidden parameters or advanced
                pFsw.WriteLine("maxspec="+_search.Search_advanced.Maxspec.ToString());  //切分谱图规模限制,Hidden parameters or advanced
                pFsw.WriteLine("IeqL="+(_search.Search_advanced.IeqL?"1":"0"));
                pFsw.WriteLine("npep="+_search.Search_advanced.NPeP.ToString());
                pFsw.WriteLine("maxdelta="+_search.Search_advanced.MAXDelta.ToString()); // 
                string varmods = "selectmod=";
                string fixmods = "fixmod=";
                //if (!_search.Open_search)   //开放和限定都要写修饰信息
                {
                    for (int i = 0; i < _search.Var_mods.Count; i++)
                    {
                        if (_search.Var_mods[i].Trim().Length > 0)
                        {
                            varmods += _search.Var_mods[i] + ";";
                        }
                    }
                    for (int i = 0; i < _search.Fix_mods.Count; i++)
                    {
                        if (_search.Fix_mods[i].Trim().Length > 0)
                        {
                            fixmods += _search.Fix_mods[i] + ";";
                        }
                    }
                }
                pFsw.WriteLine(varmods);
                pFsw.WriteLine(fixmods);
                pFsw.WriteLine("maxmod="+_search.Search_advanced.Maxmod);    //when beam search, advanced
                pFsw.WriteLine("enzyme=" + _search.Enzyme);
                int digest = 3;
                if (_search.Enzyme_Spec_index == 1)
                {
                    digest = 1;
                }
                else if (_search.Enzyme_Spec_index == 2)
                {
                    digest = 0;
                }
                pFsw.WriteLine("digest=" + digest);
                pFsw.WriteLine("max_clv_sites=" + _search.Cleavages.ToString());
                pFsw.WriteLine();
                pFsw.WriteLine("[filter]");
                pFsw.WriteLine("psm_fdr=" + (_filter.Fdr.Fdr_value / 100).ToString());
                pFsw.WriteLine("psm_fdr_type=" + _filter.Fdr.IsPeptides.ToString());    //1是peptide，0是spectra
                pFsw.WriteLine("mass_lower=" + _filter.Pep_mass_range.Left_value.ToString());
                pFsw.WriteLine("mass_upper=" + _filter.Pep_mass_range.Right_value.ToString());
                pFsw.WriteLine("len_lower=" + _filter.Pep_length_range.Left_value.ToString());
                pFsw.WriteLine("len_upper=" + _filter.Pep_length_range.Right_value.ToString());
                pFsw.WriteLine("pep_per_pro=" + _filter.Min_pep_num.ToString());
                pFsw.WriteLine("pro_fdr=" + (_filter.Protein_Fdr / 100).ToString());
                pFsw.WriteLine();

                pFsw.WriteLine("[engine]");
                pFsw.WriteLine("open=" + (_search.Open_search ? "1" : "0"));   //open search default param
                pFsw.WriteLine("open_tag_len="+_search.Search_advanced.Open_tag_len.ToString());   //open search default param
                pFsw.WriteLine();
                pFsw.WriteLine("rest_tag_iteration="+_search.Search_advanced.Rest_tag_iteration.ToString());
                pFsw.WriteLine("rest_tag_len="+_search.Search_advanced.Rest_tag_len.ToString());
                pFsw.WriteLine("rest_mod_num="+_search.Search_advanced.Rest_mod_num.ToString());
                pFsw.WriteLine();
                pFsw.WriteLine("salvo_iteration="+_search.Search_advanced.Salvo_iteration.ToString());
                pFsw.WriteLine("salvo_mod_num="+_search.Search_advanced.Salvo_mod_num.ToString());    //
                pFsw.WriteLine();
                pFsw.WriteLine("[file]");
                string modpath = "modpath=";
                //获取modification.ini路径
                //获取启动了应用程序的可执行文件的路径，不包括可执行文件的名称
                modpath += ConfigHelper.startup_path;
                modpath += "\\modification.ini";   //relative path
                pFsw.WriteLine(modpath);
                pFsw.WriteLine("fastapath=" + _search.Db.Db_path);
                pFsw.WriteLine("outputpath=" + _task.Path + "result\\");
                pFsw.WriteLine("outputname=" + _task.Task_name);
                pFsw.WriteLine();
                pFsw.WriteLine("[datalist]");
                pFsw.WriteLine("msmsnum=" + _file.Data_file_list.Count.ToString());
                string format = "PF2";
                string instr = "_";
                if (_file.File_format.Equals("raw")||_file.File_format.Equals("wiff"))
                {
                    switch (_file.Instrument_index)  // TODO: 冗余！ 可直接调用 setInstrument()
                    {
                        case (int)InstrumentOptions.CID_FTMS: instr += "CIDFT"; break;
                        case (int)InstrumentOptions.HCD_ITMS: instr += "HCDIT"; break;
                        case (int)InstrumentOptions.HCD_FTMS: instr += "HCDFT"; break;
                        case (int)InstrumentOptions.CID_ITMS: instr += "CIDIT"; break;
                    }
                    for (int i = 0; i < _file.Data_file_list.Count; i++)
                    {
                        string pfpath = _file.Data_file_list[i].FilePath;
                        pfpath = pfpath.Substring(0, pfpath.LastIndexOf(".")) + instr + ".pf2";   //原raw的路径
                        pFsw.WriteLine("msmspath" + (i + 1).ToString() + "=" + pfpath);
                    }
                }
                else if(_file.File_format.Equals("mgf"))
                {
                    format = "MGF";
                    instr = "";
                    for (int i = 0; i < _file.Data_file_list.Count; i++)
                    {
                        string mgfpath = _file.Data_file_list[i].FilePath;
                        pFsw.WriteLine("msmspath" + (i + 1).ToString() + "=" + mgfpath);
                    }
                }
                
                //else
                //{
                //    format = _file.File_format.ToUpper();
                //    for (int i = 0; i < _file.Data_file_list.Count; i++)
                //    {
                //        string pfpath = _file.Data_file_list[i].FilePath;
                //        pFsw.WriteLine("msmspath" + (i + 1).ToString() + "=" + pfpath);
                //    }
                //}
                pFsw.WriteLine("msmstype=" + format);
                pFsw.WriteLine();
                #region Quant
                pFsw.WriteLine("[quant]");
                if (_quantitation.Quantitation_type == (int)Quant_Type.Labeling_None)
                {
                     pFsw.WriteLine("quant=1|None");
                }
                else if (_quantitation.Quantitation_type == (int)Quant_Type.Labeling_15N || _quantitation.Quantitation_type == (int)Quant_Type.Labeling_SILAC)
                {
                    string labels = "quant=" + _quantitation.Labeling.Multiplicity.ToString();    //Multiplicity
                    if (_quantitation.Labeling.Multiplicity == 1)
                    {
                        if (_quantitation.Labeling.Medium_label.Count == 0)
                        {
                            labels += "|None";
                        }
                        else
                        {
                            labels += "|" + _quantitation.Labeling.Medium_label[0];
                        }
                    }
                    else if (_quantitation.Labeling.Multiplicity == 2)
                    {
                        if (_quantitation.Labeling.Light_label.Count == 0)   //light label
                        {
                            labels += "|None";
                        }
                        else
                        {
                            labels += "|" + _quantitation.Labeling.Light_label[0];
                        }
                        if (_quantitation.Labeling.Heavy_label.Count == 0)   //heavy label
                        {
                            labels += "|None";
                        }
                        else
                        {
                            labels += "|" + _quantitation.Labeling.Heavy_label[0];
                        }
                    }
                    else if (_quantitation.Labeling.Multiplicity == 3)
                    {
                        if (_quantitation.Labeling.Light_label.Count == 0)   //light label
                        {
                            labels += "|None";
                        }
                        else
                        {
                            labels += "|" + _quantitation.Labeling.Light_label[0];
                        }
                        /*
                        for (int i = 1; i < _quantitation.Labeling.Light_label.Count; i++)
                        {
                            labels += "," + _quantitation.Labeling.Light_label[i];
                        }*/
                        if (_quantitation.Labeling.Medium_label.Count == 0)   //medium label
                        {
                            labels += "|None";
                        }
                        else
                        {
                            labels += "|" + _quantitation.Labeling.Medium_label[0];
                        }
                        if (_quantitation.Labeling.Heavy_label.Count == 0)   //heavy label
                        {
                            labels += "|None";
                        }
                        else
                        {
                            labels += "|" + _quantitation.Labeling.Heavy_label[0];
                        }
                    }
                    pFsw.WriteLine(labels);
                }
                else if (_quantitation.Quantitation_type == (int)Quant_Type.Label_free)
                {

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

        //read from pFind.pfd
        void pFind_Inter.readpFind_pfd(string task_path, bool pParse, ref _Task _task)
        {
            try
            {
                File _file = _task.T_File;
                SearchParam _search = _task.T_Search;
                FilterParam _filter = _task.T_Filter;
                QuantitationParam _quantitation = _task.T_Quantitation;

                FileStream fst = new FileStream(task_path + "\\param\\pFind.cfg", FileMode.Open);
                StreamReader sr = new StreamReader(fst, Encoding.Default);
                string strLine = sr.ReadLine();
                string subtitle = "";
                #region
                while (strLine != null)
                {
                    strLine = strLine.Trim();
                    if(strLine.Length > 0 && strLine[0] == '#')
                    {
                        strLine = sr.ReadLine();
                        continue;
                    }
                    if (strLine.Length > 0 && strLine[0] == '[' && strLine[strLine.Length - 1] == ']')
                    {
                        subtitle = strLine;
                    }
                    else
                    {
                        #region [param]
                        if (subtitle.Equals("[param]"))
                        {

                            if (strLine.Length > 15 && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("activation_type"))
                            {
                                _file.Instrument = strLine.Substring(strLine.LastIndexOf("=") + 1);
                                _file.setInstrument_index();
                            }
                            else if (strLine.Length > 5 && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("mstol"))
                            {
                                _search.Ptl.Tl_value = double.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > 8 && strLine.Substring(0, 8).Equals("mstolppm"))
                            {
                                _search.Ptl.Isppm = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > 7 && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("msmstol"))
                            {
                                _search.Ftl.Tl_value = double.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > 10 && strLine.Substring(0, 10).Equals("msmstolppm"))
                            {
                                _search.Ftl.Isppm = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }

                            else if (strLine.Length > 10 && strLine.Substring(0, 10).Equals("temppepnum"))
                            {
                                _search.Search_advanced.Temppepnum = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > 6 && strLine.Substring(0, 6).Equals("pepnum"))
                            {
                                _search.Search_advanced.Pepnum = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > 10 && strLine.Substring(0, 10).Equals("selectpeak"))
                            {
                                _search.Search_advanced.Selectpeak = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > 9 && strLine.Substring(0, 9).Equals("maxprolen"))
                            {
                                _search.Search_advanced.Maxprolen = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > 7 && strLine.Substring(0, 7).Equals("maxspec"))
                            {
                                _search.Search_advanced.Maxspec = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > "IeqL".Length && strLine.Substring(0, "IeqL".Length).Equals("IeqL"))
                            {
                                _search.Search_advanced.IeqL = (strLine.Substring(strLine.LastIndexOf("=") + 1) == "1") ? true : false;
                            }
                            else if (strLine.Length > "npep".Length && strLine.Substring(0, "npep".Length).Equals("npep"))
                            {
                                _search.Search_advanced.NPeP = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > "maxdelta".Length && strLine.Substring(0, "maxdelta".Length).Equals("maxdelta"))
                            {
                                _search.Search_advanced.MAXDelta = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > 9 && strLine.Substring(0, 9).Equals("selectmod"))
                            {
                                string tmpvar = strLine.Trim();
                                string tmpfix = sr.ReadLine().Trim();
                                if (tmpvar.Equals("selectmod="))
                                {
                                    if (tmpfix.Equals("fixmod="))
                                    {
                                        //_search.Open_search = true;
                                    }
                                    _search.Var_mods.Clear();
                                }
                                else
                                {
                                    tmpvar = tmpvar.Substring(strLine.LastIndexOf("=") + 1);
                                    //_search.Open_search = false;
                                    string[] tmpv = tmpvar.Split(';');
                                    foreach (string c in tmpv)
                                    {
                                        if (c != "")
                                        {
                                            _search.Var_mods.Add(c);
                                        }
                                    }
                                }
                                if (tmpfix.Equals("fixmod="))
                                {
                                    _search.Fix_mods.Clear();
                                }
                                else
                                {
                                    tmpfix = tmpfix.Substring(tmpfix.LastIndexOf("=") + 1);
                                    //_search.Open_search = false;
                                    string[] tmpf = tmpfix.Split(';');
                                    foreach (string c in tmpf)
                                    {
                                        if (c != null)
                                        {
                                            _search.Fix_mods.Add(c);
                                        }
                                    }
                                }
                            }
                            else if (strLine.Length > 6 && strLine.Substring(0, 6).Equals("maxmod"))
                            {
                                _search.Search_advanced.Maxmod = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }   
                            else if (strLine.Length > 6 && strLine.Substring(0, 6).Equals("enzyme"))
                            {
                                _search.Enzyme = strLine.Substring(strLine.LastIndexOf("=") + 1);
                                _search.Enzyme_index = _search.setEnzymeIndex();
                            }
                            else if (strLine.Length > 6 && strLine.Substring(0, 6).Equals("digest"))
                            {
                                int digestway = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                                if (digestway == 3)
                                {
                                    _search.Enzyme_Spec_index = 0;
                                }
                                else if (digestway > 0)
                                {
                                    _search.Enzyme_Spec_index = 1;
                                }
                                else
                                {
                                    _search.Enzyme_Spec_index = 2;
                                }
                                _search.setEnzymeSpec();
                            }
                            else if (strLine.Length > 13 && strLine.Substring(0, 13).Equals("max_clv_sites"))
                            {
                                _search.Cleavages = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                        }
                        #endregion
                        #region[filter]
                        if (subtitle.Equals("[filter]"))
                        {
                            if (strLine.Length > 7 && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("psm_fdr"))
                            {
                                _filter.Fdr.Fdr_value = double.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1)) * 100;
                            }
                            else if (strLine.Length > 12 && strLine.Substring(0, 8).Equals("psm_fdr_type"))
                            {
                                _filter.Fdr.IsPeptides = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > 10 && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("mass_lower"))
                            {
                                _filter.Pep_mass_range.Left_value = double.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > 10 && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("mass_upper"))
                            {
                                _filter.Pep_mass_range.Right_value = double.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > 9 && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("len_lower"))
                            {
                                _filter.Pep_length_range.Left_value = double.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > 9 && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("len_upper"))
                            {
                                _filter.Pep_length_range.Right_value = double.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > 11 && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("pep_per_pro"))
                            {
                                _filter.Min_pep_num = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > 7 && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("pro_fdr"))
                            {
                                _filter.Protein_Fdr = double.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1)) * 100;
                            }
                        }
                        #endregion

                        else if (subtitle.Equals("[engine]"))
                        {
                            if (strLine.Length > 4 && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("open"))
                            {
                                _search.Open_search = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1)) == 1 ? true : false;
                            }
                            else if (strLine.Length > 12 && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("open_tag_len"))
                            {
                                _search.Search_advanced.Open_tag_len = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > 18 && strLine.Substring(0, 18).Equals("rest_tag_iteration"))
                            {
                                _search.Search_advanced.Rest_tag_iteration = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > 12 && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("rest_tag_len"))
                            {
                                _search.Search_advanced.Rest_tag_len = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > 12 && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("rest_mod_num"))
                            {
                                _search.Search_advanced.Rest_mod_num = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > 15 && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("salvo_iteration"))
                            {
                                _search.Search_advanced.Salvo_iteration= int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }
                            else if (strLine.Length > 13 && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("salvo_mod_num"))
                            {
                                _search.Search_advanced.Salvo_mod_num = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            }

                        }
                        #region file
                        else if (subtitle.Equals("[file]"))
                        {
                            if (strLine.Length > 9 && strLine.Substring(0, 9).Equals("fastapath"))
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
                            else if (strLine.Length > 10 && strLine.Substring(0, 10).Equals("outputpath"))
                            {
                                string tpath=strLine.Substring(strLine.LastIndexOf("=")+1);                        
                                
                                if (tpath.Trim().Length > 0)
                                {  
                                    if(task_path.EndsWith("\\"))
                                    {
                                       task_path=task_path.Substring(0,task_path.Length-1);
                                    }
                                    _task.Path = task_path + "\\";
                                    _task.Task_name = task_path.Substring(task_path.LastIndexOf("\\")+1);
                                    sr.ReadLine();
                                }
                            }
                        }
                        #endregion
                        #region [datalist]
                        else if (subtitle.Equals("[datalist]"))
                        {
                            //if (strLine.Length > 7 && strLine.Substring(0, 7).Equals("msmsnum") && (!pParse))
                            //{
                            //    #region
                            //    //默认：raw经处理后统一用PF2，只要是MGF文件，则必定是用户添加的
                            //    #endregion
                            //    //if new a task, then dtnum=0
                            //    int msmsnum = int.Parse(strLine.Substring(strLine.LastIndexOf("=") + 1));
                            //    _file.Data_file_list.Clear();
                            //    for (int i = 0; i < msmsnum; i++)
                            //    {
                            //        strLine = sr.ReadLine().Trim();
                            //        if (strLine.Length > 8 && strLine.Substring(0, 8).Equals("msmspath"))
                            //        {
                            //            string filep = strLine.Substring(strLine.LastIndexOf("=") + 1);
                            //            if (filep.Substring(filep.LastIndexOf(".") + 1).Equals("mgf") || filep.Substring(filep.LastIndexOf(".") + 1).Equals("MGF"))
                            //            {
                            //                _file.File_format_index = (int)FormatOptions.MGF;
                            //                _file.File_format = "mgf";
                            //            }
                            //            if (System.IO.File.Exists(filep))
                            //            {
                            //                _file.Data_file_list.Add(new _FileInfo(filep));
                            //            }
                            //            else
                            //            {
                            //                System.Windows.MessageBox.Show("\"" + filep + "\" does not exist.");
                            //            }
                            //        }
                            //    }
                            //}
                        }
                        #endregion
                        #region Todo quant
                        else if (subtitle.Equals("[quant]"))
                        {
                            if (strLine.Length > 5 && strLine.Substring(0, 5).Equals("quant"))
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
                                    _quantitation.Quantitation_type = (int)Quant_Type.Labeling_None;
                                    _quantitation.Labeling.Multiplicity = 1;
                                    _quantitation.Labeling.Multiplicity_index = 0;
                                    _quantitation.Labeling.Medium_label.Add(lb[1]);
                                }
                                else
                                {
                                    _quantitation.Quantitation_type = (int)Quant_Type.Labeling_SILAC;    //if 15N, it will be changed in pQuant_Read
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
                                        if (lb[1].Equals(ConfigHelper.N15_label) || lb[2].Equals(ConfigHelper.N15_label))
                                        {
                                            _quantitation.Quantitation_type = (int)Quant_Type.Labeling_15N;
                                        }

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
                        #endregion
                    }
                    strLine = sr.ReadLine();
                }
                #endregion
                sr.Close();
                fst.Close();
            }
            catch(Exception exe)
            {
                throw new Exception(exe.Message);
            }
        }

    }
}
