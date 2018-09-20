using pFind.classes;
using pFind.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace pFind.Function
{
    class pQuantWR_Func:pQuant_Inter
    {
        //generate pQuant.qnt
        void pQuant_Inter.pQuant_write(_Task _task)
        {
            try
            {
                QuantitationParam _quant = _task.T_Quantitation;
                File _file = _task.T_File;
                string filepath = _task.Path + "param\\pQuant.cfg";
                FileStream qfst = new FileStream(filepath, FileMode.Create, FileAccess.Write);
                StreamWriter qsw = new StreamWriter(qfst, Encoding.Default);
                qsw.WriteLine("# This is a standard pQuant configure file.");
                qsw.WriteLine("# Dear user,After '=' and before ';' is the content you can modify.");
                qsw.WriteLine("# Please keep serious when configuring. For some of the options,you can use the default value.");
                qsw.WriteLine("# For help: mail to liuchao1016@ict.ac.cn");
                qsw.WriteLine("# Time: " + DateTime.Now.ToString());
                qsw.WriteLine();
                qsw.WriteLine("[INI]");               
                qsw.WriteLine("PATH_INI_ELEMENT=" + ConfigHelper.startup_path + "\\element.ini;");
                qsw.WriteLine("PATH_INI_MODIFICATION=" + ConfigHelper.startup_path + "\\modification.ini;");
                qsw.WriteLine("PATH_INI_RESIDUE=" + ConfigHelper.startup_path + "\\aa.ini;");
                qsw.WriteLine();

                qsw.WriteLine("[Performance]");
                qsw.WriteLine("PATH_BIN=" + ConfigHelper.startup_path + ";");
                qsw.WriteLine("NUMBER_MAX_PSM_PER_BLOCK=" +  _quant.Quant_advanced.Number_max_psm_per_block + ";");
                qsw.WriteLine("TYPE_START=" + _quant.Quant_advanced.Type_start + ";");
                qsw.WriteLine();

                qsw.WriteLine("[MS1]");
                string ms1path = "";
                for (int i = 0; i < _file.Data_file_list.Count; i++)
                {
                    string tmp = _file.Data_file_list[i].FilePath;
                    ms1path += tmp.Substring(0, tmp.LastIndexOf(".")) + ".pf1|";
                }
                qsw.WriteLine("PATH_MS1=" + ms1path + ";");
                qsw.WriteLine("EXTENSION_TEXT_MS1="+_quant.Quant_advanced.Extension_text_ms1+";");
                qsw.WriteLine();

                qsw.WriteLine("[Identification]");
                #region TODO是否有多个spectra
                #endregion
                string resultfile = ConfigHelper.getFileBySuffix(_task.Path + "result\\", "spectra");
                qsw.WriteLine("PATH_IDENTIFICATION_FILE=" + resultfile + "|;");
                qsw.WriteLine("TYPE_IDENTIFICATION_FILE=2;");  //0是老pBuild;1是prolucid;2是pFind3.0

                //2014.10.29 chihao：pQuant过滤阈值修改，保持与过滤时一致
                qsw.WriteLine("THRESHOLD_FDR=" + (_task.T_Filter.Fdr.Fdr_value / 100.0).ToString() + ";");
                qsw.WriteLine();

                qsw.WriteLine("[Quantitation]");
                //迟浩：定量初始值设置为0，表示标记定量。在搜非标记数据时，也是跑标记流程（画三维图）
                string type_label = "0";
                if (_quant.Quantitation_type==(int)Quant_Type.Labeling_None||_quant.Quantitation_type == (int)Quant_Type.Labeling_15N||_quant.Quantitation_type==(int)Quant_Type.Labeling_SILAC)
                {
                    type_label = "0";
                }
                else if (_quant.Quantitation_type == (int)Quant_Type.Label_free)
                {
                    type_label = "1";
                }
                qsw.WriteLine("TYPE_LABEL=" + type_label + ";");
                string labelInfo = "";    
                #region 代码冗余 
                //迟浩：这一段代码写得不好，建议一起过一遍功能然后优化实现    
                if (_quant.Quantitation_type == (int)Quant_Type.Labeling_None)
                {
                     labelInfo = "1|none";  
                }
                else if (_quant.Quantitation_type == (int)Quant_Type.Labeling_15N)  //multiplicity=2
                {
                    if (_quant.Labeling.Light_label.Count == 0 && _quant.Labeling.Heavy_label.Count == 0)
                    {
                        labelInfo = "1|none";
                    }
                    else
                    {
                        labelInfo = "2|";
                        if (_quant.Labeling.Light_label.Count == 0) { labelInfo += "none"; }
                        else
                        {
                            labelInfo += ConfigHelper.Labelmap[_quant.Labeling.Light_label[0]];
                        }
                        labelInfo += "|";
                        if (_quant.Labeling.Heavy_label.Count == 0) { labelInfo += "none"; }
                        else
                        {
                            labelInfo += ConfigHelper.Labelmap[_quant.Labeling.Heavy_label[0]];
                        }
                    }
                }
                else if (_quant.Quantitation_type == (int)Quant_Type.Labeling_SILAC)
                {
                    int label_num = _quant.Labeling.Multiplicity;
                    labelInfo = label_num.ToString();
                    if (label_num == 1)
                    {
                        labelInfo += "|";
                        if (_quant.Labeling.Medium_label.Count == 0) { labelInfo += "none"; }
                        else
                        {
                            labelInfo += ConfigHelper.Labelmap[_quant.Labeling.Medium_label[0]];
                        }
                    }
                    else if (label_num == 2)
                    {
                        if (_quant.Labeling.Light_label.Count == 0 && _quant.Labeling.Heavy_label.Count == 0)
                        {
                            labelInfo = "1|none";
                        }
                        else
                        {
                            labelInfo += "|";
                            if (_quant.Labeling.Light_label.Count == 0) { labelInfo += "none"; }
                            else
                            {
                                labelInfo += ConfigHelper.Labelmap[_quant.Labeling.Light_label[0]];
                            }
                            labelInfo += "|";
                            if (_quant.Labeling.Heavy_label.Count == 0) { labelInfo += "none"; }
                            else
                            {
                                labelInfo += ConfigHelper.Labelmap[_quant.Labeling.Heavy_label[0]];
                            }
                        }
                    }
                    else if (label_num == 3)
                    {
                        labelInfo += "|";
                        if (_quant.Labeling.Light_label.Count == 0) { labelInfo += "none"; }
                        else
                        {
                            labelInfo += ConfigHelper.Labelmap[_quant.Labeling.Light_label[0]];
                        }
                        labelInfo += "|";
                        if (_quant.Labeling.Medium_label.Count == 0) { labelInfo += "none"; }
                        else
                        {
                            labelInfo += ConfigHelper.Labelmap[_quant.Labeling.Medium_label[0]];
                        }
                        labelInfo += "|";
                        if (_quant.Labeling.Heavy_label.Count == 0) { labelInfo += "none"; }
                        else
                        {
                            labelInfo += ConfigHelper.Labelmap[_quant.Labeling.Heavy_label[0]];
                        }
                    }
                }
                qsw.WriteLine("LL_INFO_LABEL=" + labelInfo + ";");
                #endregion
                string lfec = "none";
                if (_quant.Quant_advanced.Ll_element_enrichment_calibration == (int)LL_ELEMENT_ENRICHMENT_CALIBRATION.N15)
                {
                    lfec = "15N";
                }
                else if (_quant.Quant_advanced.Ll_element_enrichment_calibration == (int)LL_ELEMENT_ENRICHMENT_CALIBRATION.C13)
                {
                    lfec = "13C";
                }
                qsw.WriteLine("LL_ELEMENT_ENRICHMENT_CALIBRATION=" + lfec + ";");
                #region Todo LabelFree
                //Label Free
                #endregion
                qsw.WriteLine("LF_INFO_SAMPLE=; //Label Free暂不考虑");
                qsw.WriteLine();

                qsw.WriteLine("[Evidence]");
                qsw.WriteLine("NUMBER_SCANS_HALF_CMTG=" + _quant.Quant_advanced.Number_scans_half_cmtg + ";");
                qsw.WriteLine("PPM_FOR_CALIBRATION=" + _quant.Quant_advanced.Ppm_for_calibration + ";");
                qsw.WriteLine("PPM_HALF_WIN_ACCURACY_PEAK=" + _quant.Quant_advanced.Ppm_half_win_accuracy_peak + ";");
                qsw.WriteLine("NUMBER_HOLE_IN_CMTG=" + (_quant.Quant_advanced.Number_hole_in_cmtg + 1).ToString() + ";");
                qsw.WriteLine("TYPE_SAME_START_END_BETWEEN_EVIDENCE=" + _quant.Quant_advanced.Type_same_start_end_between_evidence + ";");
                qsw.WriteLine();

                qsw.WriteLine("[Inference]");
                qsw.WriteLine("TYPE_PEPTIDE_RATIO=" + _quant.Quant_inference.Type_peptide_ratio + ";");
                qsw.WriteLine("TYPE_PROTEIN_RATIO_CALCULATION="+_quant.Quant_inference.Type_protein_ratio_calculation+";");
                qsw.WriteLine("TYPE_UNIQUE_PEPTIDE_ONLY="+_quant.Quant_inference.Type_unique_peptide_only+";");
                qsw.WriteLine("THRESHOLD_SCORE_INTERFERENCE=" + _quant.Quant_inference.Threshold_score_interference + ";");
                qsw.WriteLine("THRESHOLD_SCORE_INTENSITY=" + _quant.Quant_inference.Threshold_score_intensity + ";");
                qsw.WriteLine("TYPE_GET_GROUP=" + _quant.Quant_inference.Type_get_group + ";");
                qsw.WriteLine("PATH_FASTA=" + _task.T_Search.Db.Db_path + ";");
                qsw.WriteLine();

                qsw.WriteLine("[Export]");
                qsw.WriteLine("DIR_EXPORT=" + _task.Path + "result\\;");
                qsw.WriteLine("FLAG_CREATE_NEW_FOLDER=0;");   //0表示直接使用dir_export的导出目录
                qsw.Close();
                qfst.Close();
            }
            catch(Exception exe)
            {
                throw new Exception(exe.Message);
            }
        }
        //写入鉴定结果文件路径
        void pQuant_Inter.pQuant_writeResultFile(_Task _task)
        {
            try
            {
                FileStream fst = new FileStream(_task.Path + "\\param\\pQuant.cfg", FileMode.Open);
                StreamReader sr = new StreamReader(fst, Encoding.Default);
                string strLine = sr.ReadLine();
                List<string> text = new List<string>();
                while (strLine != null)
                {
                    if (strLine.Trim() == "")
                    {
                        text.Add("");
                    }
                    else if (strLine.Trim().Length > 0 && strLine.Contains("=") && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("PATH_IDENTIFICATION_FILE"))
                    {
                        string resultfile = ConfigHelper.getFileBySuffix(_task.Path + "result\\", "spectra");
                        text.Add("PATH_IDENTIFICATION_FILE=" + resultfile + "|;");
                    }
                    else
                    {
                        text.Add(strLine);
                    }
                    strLine = sr.ReadLine();
                }
                sr.Close();
                fst.Close();
                fst = new FileStream(_task.Path + "\\param\\pQuant.cfg", FileMode.Open, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fst, Encoding.Default);
                foreach (string str in text)
                {
                    sw.WriteLine(str);
                }
                sw.Flush();
                sw.Close();
                fst.Close();
            }
            catch(Exception exe)
            {
                throw new Exception(exe.Message);
            }
        }
        //read from pQuant.qnt
        //wrm20150821:将来代码重构时可考虑在读参数文件时，先获取‘=’前和‘=’后的部分，再给参数赋值
        void pQuant_Inter.readpQuant_qnt(string task_path, ref _Task _task)
        {          
            QuantitationParam _quantitation = _task.T_Quantitation;
            try
            {
                FileStream fst = new FileStream(task_path + "\\param\\pQuant.cfg", FileMode.Open);
                StreamReader sr = new StreamReader(fst, Encoding.Default);
                string strLine = sr.ReadLine();
                #region
                QuantitationParam _quant = _task.T_Quantitation;
                int lb = 0;
                string[] labels = {""}; ;
                while (strLine != null)
                {
                    strLine = strLine.Trim();
                    if (strLine.Length > 0 && strLine.Contains("="))
                    {
                        if (strLine.Length > ("NUMBER_MAX_PSM_PER_BLOCK").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("NUMBER_MAX_PSM_PER_BLOCK"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            _quant.Quant_advanced.Number_max_psm_per_block = int.Parse(strLine.Substring(tmp + 1, strLine.LastIndexOf(";") - tmp - 1));
                        }
                        else if (strLine.Length > ("TYPE_START").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("TYPE_START"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            _quant.Quant_advanced.Type_start = int.Parse(strLine.Substring(tmp + 1, strLine.LastIndexOf(";") - tmp - 1));
                        }
                        else if (strLine.Length > ("EXTENSION_TEXT_MS1").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("EXTENSION_TEXT_MS1"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            _quant.Quant_advanced.Extension_text_ms1 = strLine.Substring(tmp + 1, strLine.LastIndexOf(";") - tmp - 1); 
                        }
                        //else if (strLine.Length > ("LL_INFO_LABEL").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("LL_INFO_LABEL"))
                        //{
                        //    int tmp=strLine.LastIndexOf("=");
                        //    string qt = strLine.Substring(tmp + 1,strLine.LastIndexOf(";")-tmp-1);
                        //    labels = qt.Split('|');
                        //    lb = int.Parse(labels[0]);
                        //}
                        else if (strLine.Length > ("LL_ELEMENT_ENRICHMENT_CALIBRATION").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("LL_ELEMENT_ENRICHMENT_CALIBRATION"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            string lfec = strLine.Substring(tmp + 1, strLine.LastIndexOf(";") - tmp - 1);
                            _quant.Quant_advanced.Ll_element_enrichment_calibration = (int)LL_ELEMENT_ENRICHMENT_CALIBRATION.None;
                            if(lfec.ToLower() == "15n")
                            {
                                _quant.Quant_advanced.Ll_element_enrichment_calibration = (int)LL_ELEMENT_ENRICHMENT_CALIBRATION.N15;
                            }
                            else if(lfec.ToLower() == "13c")
                            {
                                _quant.Quant_advanced.Ll_element_enrichment_calibration = (int)LL_ELEMENT_ENRICHMENT_CALIBRATION.C13;
                            }
                            //bool is15N = false;
                            //if (lb == 2 && labels.Length == 3 && (labels[1].Equals(ConfigHelper.N15_label) || labels[2].Equals(ConfigHelper.N15_label)))
                            //{
                            //    is15N = true;
                            //}
                            //if (is15N && (flag == 1 || flag == 2))
                            //{
                            //    _quant.Quantitation_type = (int)Quant_Type.Labeling_15N;
                            //}
                        }
                        #region Todo LabelFree
                        else if (strLine.Length > ("LF_INFO_SAMPLE").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("LF_INFO_SAMPLE"))
                        {

                        }
                        #endregion
                        else if (strLine.Length > ("NUMBER_SCANS_HALF_CMTG").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("NUMBER_SCANS_HALF_CMTG"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            _quant.Quant_advanced.Number_scans_half_cmtg = int.Parse(strLine.Substring(tmp + 1, strLine.LastIndexOf(";") - tmp - 1));
                        }
                        else if (strLine.Length > ("PPM_FOR_CALIBRATION").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("PPM_FOR_CALIBRATION"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            _quant.Quant_advanced.Ppm_for_calibration = double.Parse(strLine.Substring(tmp + 1, strLine.LastIndexOf(";") - tmp - 1));
                        }
                        else if (strLine.Length > ("PPM_HALF_WIN_ACCURACY_PEAK").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("PPM_HALF_WIN_ACCURACY_PEAK"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            _quant.Quant_advanced.Ppm_half_win_accuracy_peak = double.Parse(strLine.Substring(tmp + 1, strLine.LastIndexOf(";") - tmp - 1));
                        }
                        else if (strLine.Length > ("NUMBER_HOLE_IN_CMTG").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("NUMBER_HOLE_IN_CMTG"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            _quant.Quant_advanced.Number_hole_in_cmtg = int.Parse(strLine.Substring(tmp + 1, strLine.LastIndexOf(";") - tmp - 1)) - 1;
                        }
                        else if (strLine.Length > ("TYPE_SAME_START_END_BETWEEN_EVIDENCE").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("TYPE_SAME_START_END_BETWEEN_EVIDENCE"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            _quant.Quant_advanced.Type_same_start_end_between_evidence = int.Parse(strLine.Substring(tmp + 1, strLine.LastIndexOf(";") - tmp - 1));
                        }
                        else if (strLine.Length > ("TYPE_PEPTIDE_RATIO").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("TYPE_PEPTIDE_RATIO"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            _quant.Quant_inference.Type_peptide_ratio = int.Parse(strLine.Substring(tmp + 1, strLine.LastIndexOf(";") - tmp - 1));
                        }
                        else if (strLine.Length > ("TYPE_PROTEIN_RATIO_CALCULATION").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("TYPE_PROTEIN_RATIO_CALCULATION"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            _quant.Quant_inference.Type_protein_ratio_calculation = int.Parse(strLine.Substring(tmp + 1, strLine.LastIndexOf(";") - tmp - 1));
                        }
                        else if (strLine.Length > ("TYPE_UNIQUE_PEPTIDE_ONLY").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("TYPE_UNIQUE_PEPTIDE_ONLY"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            _quant.Quant_inference.Type_unique_peptide_only = int.Parse(strLine.Substring(tmp + 1, strLine.LastIndexOf(";") - tmp - 1));
                        }
                        else if (strLine.Length > ("THRESHOLD_SCORE_INTERFERENCE").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("THRESHOLD_SCORE_INTERFERENCE"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            _quant.Quant_inference.Threshold_score_interference = double.Parse(strLine.Substring(tmp + 1, strLine.LastIndexOf(";") - tmp - 1));
                        }
                        else if (strLine.Length > ("THRESHOLD_SCORE_INTENSITY").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("THRESHOLD_SCORE_INTENSITY"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            _quant.Quant_inference.Threshold_score_intensity = double.Parse(strLine.Substring(tmp + 1, strLine.LastIndexOf(";") - tmp - 1));
                        }
                        else if (strLine.Length > ("TYPE_GET_GROUP").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("TYPE_GET_GROUP"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            _quant.Quant_inference.Type_get_group = int.Parse(strLine.Substring(tmp + 1, strLine.LastIndexOf(";") - tmp - 1));
                        }
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


        //read from pIsobariQ param
        void pQuant_Inter.pIsobariQ_read(string task_path, ref _Task _task)
        {
            try
            {
                FileStream fst = new FileStream(task_path + "\\param\\pIsobariQ.cfg", FileMode.Open);
                StreamReader sr = new StreamReader(fst, Encoding.Default);
                string strLine = sr.ReadLine();
                #region
                MS2Quant _ms2quant = _task.T_MS2Quant;
                while (strLine != null)
                {
                    strLine = strLine.Trim();
                    if (strLine.Length > 0 && strLine.Contains("="))
                    {
                        if (strLine.Length > ("quantitativeMethod").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("quantitativeMethod"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            _ms2quant.QuantitativeMethod = int.Parse(strLine.Substring(tmp + 1));
                        }
                        else if (strLine.Length > ("reporterIonMZ").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("reporterIonMZ"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            string tmpstr = strLine.Substring(tmp + 1);
                            string[] tmparray = tmpstr.Split(new string[]{", "},StringSplitOptions.RemoveEmptyEntries);
                            foreach (string str in tmparray)
                            {
                                _ms2quant.ReporterIonMZ.Add(double.Parse(str));
                            }
                        }
                        else if (strLine.Length > ("pIDLplex").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("pIDLplex"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            string tmpstr = strLine.Substring(tmp + 1);
                            string[] tmparray = tmpstr.Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string str in tmparray)
                            {
                                string[] modmass = str.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                                if (modmass.Length != 2) continue;
                                string[] nterm = modmass[0].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                string[] cterm = modmass[1].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                if (nterm.Length != 2 || cterm.Length != 2) continue;
                                _ms2quant.PIDL.Add(new pIDLplex(nterm[0],double.Parse(nterm[1]),cterm[0],double.Parse(cterm[1])));
                            }
                        }
                        else if (strLine.Length > ("FTMSType").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("FTMSType"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            string ftoltype = strLine.Substring(tmp + 1);
                            if (ftoltype.ToLower() == "da")
                            {
                                _ms2quant.MS2_Advanced.FTMS_Tolerance.Isppm = 0;
                            }
                            else if (ftoltype.ToLower() == "ppm")
                            {
                                _ms2quant.MS2_Advanced.FTMS_Tolerance.Isppm = 1;
                            }
                        }
                        else if (strLine.Length > ("FTMS").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("FTMS"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            _ms2quant.MS2_Advanced.FTMS_Tolerance.Tl_value = double.Parse(strLine.Substring(tmp + 1));
                        }
                        else if (strLine.Length > ("minRange").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("minRange"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            _ms2quant.MS2_Advanced.Peak_Range.Left_value = double.Parse(strLine.Substring(tmp + 1));
                        }
                        else if (strLine.Length > ("maxRange").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("maxRange"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            _ms2quant.MS2_Advanced.Peak_Range.Right_value = double.Parse(strLine.Substring(tmp + 1));
                        }
                        else if (strLine.Length > ("PIF").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("PIF"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            _ms2quant.MS2_Advanced.Pif = double.Parse(strLine.Substring(tmp + 1));
                        }
                        else if (strLine.Length > ("PsmFDR").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("PsmFDR"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            _ms2quant.MS2_Advanced.Psm_Fdr = double.Parse(strLine.Substring(tmp + 1))*100;
                        }
                        else if (strLine.Length > ("ProteinFDR").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("ProteinFDR"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            _ms2quant.MS2_Advanced.Protein_Fdr = double.Parse(strLine.Substring(tmp + 1))*100;
                        }
                        else if (strLine.Length > ("Correct").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("Correct"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            _ms2quant.MS2_Advanced.Correct = (int.Parse(strLine.Substring(tmp + 1)) == 1) ? true : false;
                        }
                        else if (strLine.Length > ("runVSN").Length && strLine.Substring(0, strLine.LastIndexOf("=")).Equals("runVSN"))
                        {
                            int tmp = strLine.LastIndexOf("=");
                            _ms2quant.MS2_Advanced.RunVSN = (int.Parse(strLine.Substring(tmp + 1)) == 1) ? true : false;
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
                throw new Exception("[pIsobariQ_read] Error in pIsobariQ configuration file.\n" + exe.Message);
            }           
        }
        //generate pIsobariQ.cfg
        void pQuant_Inter.pIsobariQ_write(_Task _task)
        {
            try
            {
                MS2Quant _ms2quant = _task.T_MS2Quant;
                File _file = _task.T_File;
                string filepath = _task.Path + "param\\pIsobariQ.cfg";
                FileStream qfst = new FileStream(filepath, FileMode.Create, FileAccess.Write);
                StreamWriter qsw = new StreamWriter(qfst, Encoding.Default);
                qsw.WriteLine("# This is a standard pIsobariQ configure file.");
                qsw.WriteLine("# For help: mail to wangzhaowei@ict.ac.cn");
                qsw.WriteLine("# Time: " + DateTime.Now.ToString());
                qsw.WriteLine();
                
                qsw.WriteLine("[Basic Options]");
                qsw.WriteLine("resultDatapath=" + _task.Path+"result\\pFind.spectra");
                string pf1path = "";
                for (int i = 0; i < _file.Data_file_list.Count; i++)
                {
                    string tmp = _file.Data_file_list[i].FilePath;
                    pf1path += tmp.Substring(0, tmp.LastIndexOf(".")) + ".pf1|";
                }
                // 这里我只给出了所有pf1的路径，内核可以自动切割字符串并查找相关的pf2的路径
                qsw.WriteLine("pfDatapath=" + pf1path);
                qsw.WriteLine("quantResultDatapath="+_task.Path+"result\\pQuant-ms2_result.spectra");
                qsw.WriteLine();

                qsw.WriteLine("[Database Options]");
                qsw.WriteLine("fastaDatapath=" + _task.T_Search.Db.Db_path);
                qsw.WriteLine("modificationDatapath=" + ConfigHelper.startup_path + "\\modification.ini");
                qsw.WriteLine();

                qsw.WriteLine("[Method Options]");
                qsw.WriteLine("quantitativeMethod=" + _task.T_MS2Quant.QuantitativeMethod);
                string tmpstr = "";
                if(_task.T_MS2Quant.ReporterIonMZ.Count>0)
                {
                    tmpstr += _task.T_MS2Quant.ReporterIonMZ[0].ToString();
                }
                for (int i = 1; i < _task.T_MS2Quant.ReporterIonMZ.Count; ++i)
                {
                    tmpstr += ", " + _task.T_MS2Quant.ReporterIonMZ[i].ToString();
                }
                qsw.WriteLine("reporterIonMZ=" + tmpstr);
                tmpstr = "";
                if (_task.T_MS2Quant.PIDL.Count > 0)
                {
                    tmpstr += _task.T_MS2Quant.PIDL[0].Nterm_modmass.Name + " " + _task.T_MS2Quant.PIDL[0].Nterm_modmass.Mass.ToString() + ", ";
                    tmpstr += _task.T_MS2Quant.PIDL[0].Cterm_modmass.Name + " " + _task.T_MS2Quant.PIDL[0].Cterm_modmass.Mass.ToString();
                }
                for (int i = 1; i < _task.T_MS2Quant.PIDL.Count; ++i)
                {
                    tmpstr += "; " + _task.T_MS2Quant.PIDL[i].Nterm_modmass.Name + " " + _task.T_MS2Quant.PIDL[i].Nterm_modmass.Mass.ToString() + ", ";
                    tmpstr += _task.T_MS2Quant.PIDL[i].Cterm_modmass.Name + " " + _task.T_MS2Quant.PIDL[i].Cterm_modmass.Mass.ToString();
                }

                qsw.WriteLine("pIDLplex=" + tmpstr);
                qsw.WriteLine();

                qsw.WriteLine("[Advanced Options]");
                qsw.WriteLine("FTMSType=" + ((_task.T_MS2Quant.MS2_Advanced.FTMS_Tolerance.Isppm==1) ? "ppm":"Da"));
                qsw.WriteLine("FTMS=" + _task.T_MS2Quant.MS2_Advanced.FTMS_Tolerance.Tl_value.ToString());

                qsw.WriteLine("minRange=" + _task.T_MS2Quant.MS2_Advanced.Peak_Range.Left_value.ToString());
                qsw.WriteLine("maxRange=" + _task.T_MS2Quant.MS2_Advanced.Peak_Range.Right_value.ToString());
                qsw.WriteLine("PIF=" + _task.T_MS2Quant.MS2_Advanced.Pif.ToString());
                qsw.WriteLine("PsmFDR=" + (_task.T_MS2Quant.MS2_Advanced.Psm_Fdr/100.0).ToString());
                qsw.WriteLine("ProteinFDR=" + (_task.T_MS2Quant.MS2_Advanced.Protein_Fdr / 100.0).ToString());
                qsw.WriteLine("Correct=" + (_task.T_MS2Quant.MS2_Advanced.Correct ? "1":"0"));
                qsw.WriteLine("runVSN=" + (_task.T_MS2Quant.MS2_Advanced.RunVSN ? "1":"0"));
                qsw.WriteLine();
                qsw.Close();
                qfst.Close();
            }
            catch (Exception exe)
            {
                throw new Exception("[pIsobariQ_write] Error in pIsobariQ configuration file.\n" + exe.Message);
            }
        }
    }
}
