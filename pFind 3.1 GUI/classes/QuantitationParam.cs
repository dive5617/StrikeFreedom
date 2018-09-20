using pFind.classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace pFind
{
    public enum Quant_Type:int
    {
       Labeling_None=0,
       Labeling_15N=1,
       Labeling_SILAC=2,
       Label_free=3
    }

    public enum Mixed_Samples : int
    { 
        Mixed_Samples_1_1=0,
        Mixed_Samples_10to1_or_1to10=1,
        Independent = 2
    }

    public enum LL_ELEMENT_ENRICHMENT_CALIBRATION : int
    { 
        None = 0,
        N15 = 1,
        C13 = 2
    }

    public class QuantitationParam :INotifyPropertyChanged    //4个
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }        
        
        public QuantitationParam() { }
        
        int quantitation_type=(int)Quant_Type.Labeling_None;    //binding: 定量类型
        [Category("Quantitation")]
        public int Quantitation_type
        {
            get { return quantitation_type; }
            set 
            {
                if (value != quantitation_type)
                {
                    quantitation_type = value;
                    NotifyPropertyChanged("Quantitation_type");
                }
            }
        }

        Labeling _labeling = new Labeling();    //binding:

        public Labeling Labeling
        {
            get { return _labeling; }
            set 
            {
                if (value != _labeling)
                {
                    _labeling = value;
                    NotifyPropertyChanged("Labeling");
                }
            }
        }

        LabelFree _lf = new LabelFree();

        public LabelFree Lf
        {
            get { return _lf; }
            set 
            {
                if (value != _lf)
                {
                    _lf = value;
                    NotifyPropertyChanged("Lf");
                }
            }
        }

        Quant_Advanced quant_advanced = new Quant_Advanced();

        public Quant_Advanced Quant_advanced
        {
            get { return quant_advanced; }
            set
            {
                if (value != quant_advanced)
                {
                    quant_advanced = value;
                    NotifyPropertyChanged("Quant_advanced");
                }
            }
        }

        Quant_Inference quant_inference = new Quant_Inference();

        public Quant_Inference Quant_inference
        {
            get { return quant_inference; }
            set
            {
                if (value != quant_inference)
                {
                    quant_inference = value;
                    NotifyPropertyChanged("Quant_inference");
                }
            }
        }

        public QuantitationParam(int _quantitation_type)
        {
            this.quantitation_type = _quantitation_type;            
        }

        public QuantitationParam(int _quantitation_type,Labeling __label,LabelFree __lf,Quant_Advanced qad)
        {
            this.quantitation_type = _quantitation_type;
            this._labeling = __label;
            this._lf = __lf;
            this.quant_advanced = qad;
        }

        public void Reset()
        {
            this.quantitation_type = (int)Quant_Type.Labeling_None;
            this._labeling.Multiplicity_index=0;
            this.Labeling.setMultiplicity();
            this._labeling.Light_label.Clear();
            this._labeling.Medium_label.Clear();
            this._labeling.Heavy_label.Clear();
            #region Todo
            //Labelfree
            #endregion
            this._lf = new LabelFree();
            this.quant_advanced.Number_scans_half_cmtg = 100;
            this.quant_advanced.Ppm_for_calibration = 0.0;
            this.quant_advanced.Ppm_half_win_accuracy_peak = 10.0;
            this.quant_advanced.Number_hole_in_cmtg = 1;    //index=1,value=2
            this.quant_advanced.Type_same_start_end_between_evidence = 0;
            this.quant_advanced.Ll_element_enrichment_calibration = (int)LL_ELEMENT_ENRICHMENT_CALIBRATION.None;
        }

        public ObservableCollection<_Attribute> getQuantitationParam()
        {
            ObservableCollection<_Attribute> quant = new ObservableCollection<_Attribute>();
            string quantstr = "";
            switch (quantitation_type)
            {
                case (int)Quant_Type.Labeling_None: quantstr += "Labeling_None"; break;
                case (int)Quant_Type.Labeling_15N: quantstr += "Labeling_15N"; break;
                case (int)Quant_Type.Labeling_SILAC: quantstr += "Labeling_SILAC etc."; break;
                case (int)Quant_Type.Label_free: quantstr += "Label Free"; break;
            }
            quant.Add(new _Attribute("Quantitation", quantstr, ""));
            #region
            if (quantitation_type != (int)Quant_Type.Label_free)
            {
                quant.Add(new _Attribute("Multiplicity", _labeling.Multiplicity.ToString(), ""));
                if (_labeling.Multiplicity == 1)
                {
                    string lbs = "";
                    for (int i = 0; i < _labeling.Medium_label.Count; i++)
                    {
                        lbs += _labeling.Medium_label[i] + "; ";
                    }
                    quant.Add(new _Attribute("Label", lbs, ""));
                }
                else if (_labeling.Multiplicity == 2)
                {
                    string lightlbs = "";
                    for (int i = 0; i < _labeling.Light_label.Count; i++)
                    {
                        lightlbs += _labeling.Light_label[i] + "; ";
                    }
                    quant.Add(new _Attribute("Light Label", lightlbs, ""));
                    string heavylbs = "";
                    for (int i = 0; i < _labeling.Heavy_label.Count; i++)
                    {
                        heavylbs += _labeling.Heavy_label[i] + "; ";
                    }
                    quant.Add(new _Attribute("Heavy Label", heavylbs, ""));
                }
                else if (_labeling.Multiplicity == 3)
                {
                    string lightlbs = "";
                    for (int i = 0; i < _labeling.Light_label.Count; i++)
                    {
                        lightlbs += _labeling.Light_label[i] + "; ";
                    }
                    quant.Add(new _Attribute("Light Label", lightlbs, ""));

                    string mediumlbs = "";
                    for (int i = 0; i < _labeling.Medium_label.Count; i++)
                    {
                        mediumlbs += _labeling.Medium_label[i] + "; ";
                    }
                    quant.Add(new _Attribute("Medium Label", mediumlbs, ""));

                    string heavylbs = "";
                    for (int i = 0; i < _labeling.Heavy_label.Count; i++)
                    {
                        heavylbs += _labeling.Heavy_label[i] + "; ";
                    }
                    quant.Add(new _Attribute("Heavy Label", heavylbs, ""));
                }
            }
            #endregion
            else if (quantitation_type == (int)Quant_Type.Label_free)
            {

            }
            if (quantitation_type != (int)Quant_Type.Labeling_None)
            {
                #region Advanced_Quant
                string nshc_check = "";
                if (this.quant_advanced.Number_scans_half_cmtg < 0)
                {
                    nshc_check = "invalid";
                }
                quant.Add(new _Attribute("NUMBER_SCANS_HALF_CMTG", this.quant_advanced.Number_scans_half_cmtg.ToString(), nshc_check));
                string pfc_check = "";
                //if (this.quant_advanced.Ppm_for_calibration < 0)
                //{
                //    pfc_check = "invalid";
                //}
                quant.Add(new _Attribute("PPM_FOR_CALIBRATION", this.quant_advanced.Ppm_for_calibration.ToString(), pfc_check));
                string pfwap_check = "";
                if (this.quant_advanced.Ppm_half_win_accuracy_peak < 0)
                {
                    pfwap_check = "invalid";
                }
                quant.Add(new _Attribute("PPM_HALF_WIN_ACCURACY_PEAK", this.quant_advanced.Ppm_half_win_accuracy_peak, pfwap_check));
                quant.Add(new _Attribute("NUMBER_HOLE_IN_CMTG", this.quant_advanced.Number_hole_in_cmtg + 1, ""));
                string tssebe = "";
                switch (this.quant_advanced.Type_same_start_end_between_evidence)
                {
                    case (int)Mixed_Samples.Independent: tssebe = "Independent"; break;
                    case (int)Mixed_Samples.Mixed_Samples_1_1: tssebe = "For 1:1 Mixed Samples"; break;
                    case (int)Mixed_Samples.Mixed_Samples_10to1_or_1to10: tssebe = "For 1:10 or 10:1 Mixed Samples"; break;
                }
                if (quantitation_type != (int)Quant_Type.Labeling_None)
                {
                    quant.Add(new _Attribute("TYPE_SAME_START_END_BETWEEN_EVIDENCE", tssebe, ""));
                }
                string lfec = "none";
                if (this.quant_advanced.Ll_element_enrichment_calibration == (int)LL_ELEMENT_ENRICHMENT_CALIBRATION.N15)
                {
                    lfec = "15N";
                }
                if (this.quant_advanced.Ll_element_enrichment_calibration == (int)LL_ELEMENT_ENRICHMENT_CALIBRATION.C13)
                {
                    lfec = "13C";
                }
                if (quantitation_type == (int)Quant_Type.Labeling_15N)
                {
                    quant.Add(new _Attribute("LL_ELEMENT_ENRICHMENT_CALIBRATION", lfec, ""));
                }
                #endregion
            }
            return quant;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != this.GetType()) { return false; }
            QuantitationParam qp = obj as QuantitationParam;
            if ((System.Object)qp == null)
            {
                return false;
            }
            if (this.quantitation_type!=qp.quantitation_type) { return false; }
            if(!this._labeling.Equals(qp._labeling)){return false; }
            if(!this.quant_advanced.Equals(qp.quant_advanced)) { return false; }
            if (!this.quant_inference.Equals(qp.quant_inference)) { return false; }
            #region Todo
            //Label Free
            #endregion
            if (!this._lf.Equals(qp._lf)) { return false; }
            return true;
        }
        public override int GetHashCode()
        {
            #region Todo
            //Label Free
            #endregion
            int hc = this.quantitation_type.GetHashCode() + this._labeling.GetHashCode()+this._lf.GetHashCode()
                +this.quant_advanced.GetHashCode() + this.quant_inference.GetHashCode();
            return hc;
        }
    
    }

    public class Labeling :INotifyPropertyChanged     //5个属性
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        } 

        public Labeling() { }

        private int multiplicity_index = 1;    //binding:

        public int Multiplicity_index
        {
            get { return multiplicity_index; }
            set 
            {
                if (value != multiplicity_index)
                {
                    multiplicity_index = value;
                    NotifyPropertyChanged("Multiplicity_index");
                }
            }
        }

        private int multiplicity=2;    //num of kinds of labels
        public int Multiplicity
        {
            get { return multiplicity; }
            set { multiplicity = value; }
        }
        public void setMultiplicity()
        {
            multiplicity = multiplicity_index + 1;
        }


        private ObservableCollection<string> light_label = new ObservableCollection<string>();

        public ObservableCollection<string> Light_label
        {
            get { return light_label; }
            set 
            {
                if (value != light_label)
                {
                    light_label = value;
                    NotifyPropertyChanged("Light_label");
                }
            }
        }


        private ObservableCollection<string> medium_label = new ObservableCollection<string>();

        public ObservableCollection<string> Medium_label
        {
            get { return medium_label; }
            set 
            {
                if (value != medium_label)
                {
                    medium_label = value;
                    NotifyPropertyChanged("Medium_label");
                }
            }
        }
       

        private ObservableCollection<string> heavy_label = new ObservableCollection<string>();

        public ObservableCollection<string> Heavy_label
        {
            get { return heavy_label; }
            set 
            {
                if (value != heavy_label)
                {
                    heavy_label = value;
                    NotifyPropertyChanged("Heavy_label");
                }
            }
        }

        
        public Labeling(int _multiplicity, ObservableCollection<string> _lightlb, ObservableCollection<string> _mediumlb, ObservableCollection<string> _heavylb) 
        {
            this.multiplicity = _multiplicity;
            this.light_label = _lightlb;
            this.medium_label = _mediumlb;
            this.heavy_label = _heavylb;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != this.GetType()) { return false; }
            Labeling lg = obj as Labeling;
            if ((System.Object)lg == null)
            {
                return false;
            }
            if (this.multiplicity!=lg.multiplicity) { return false; }
            if (this.light_label.Count != lg.light_label.Count) { return false; }
            for (int i = 0; i < this.light_label.Count; i++)
            {
                if (this.light_label[i] != lg.light_label[i]) { return false; }
            }
            if (this.medium_label.Count != lg.medium_label.Count) { return false; }
            for (int i = 0; i < this.medium_label.Count; i++)
            {
                if (this.medium_label[i] != lg.medium_label[i]) { return false; }
            }
            if (this.heavy_label.Count != lg.heavy_label.Count) { return false; }
            for (int i = 0; i < heavy_label.Count; i++)
            {
                if (this.heavy_label[i] != lg.heavy_label[i]) { return false; }
            }
            return true;
        }
        public override int GetHashCode()
        {
            int hc = this.multiplicity.GetHashCode() + this.light_label.GetHashCode() 
                + this.medium_label.GetHashCode() + this.heavy_label.GetHashCode();
            return hc;
        }
        public void copyTo(Labeling lb)
        {
            lb.multiplicity_index = this.multiplicity_index;
            lb.multiplicity = this.multiplicity;
            lb.light_label.Clear();
            for (int i = 0; i < this.light_label.Count; i++)
            {
                lb.light_label.Add(this.light_label[i]);
            }
            lb.medium_label.Clear();
            for (int i = 0; i < this.medium_label.Count; i++)
            {
                lb.medium_label.Add(this.medium_label[i]);
            }
            lb.heavy_label.Clear();
            for (int i = 0; i < heavy_label.Count; i++)
            {
                lb.heavy_label.Add(this.heavy_label[i]);
            }
        }
    }

    public class Quant_Advanced : INotifyPropertyChanged,IDataErrorInfo   //9个属性
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        private int ll_element_enrichment_calibration = 0;

        public int Ll_element_enrichment_calibration
        {
            get { return ll_element_enrichment_calibration; }
            set
            {
                if (value != ll_element_enrichment_calibration)
                {
                    ll_element_enrichment_calibration = value;
                    NotifyPropertyChanged("Ll_element_enrichment_calibration");
                }
            }
        }      


        private int number_scans_half_cmtg = 200;     //100-10000

        public int Number_scans_half_cmtg
        {
            get { return number_scans_half_cmtg; }
            set
            {
                if (value != number_scans_half_cmtg)
                { 
                    number_scans_half_cmtg = value;
                    NotifyPropertyChanged("Number_scans_half_cmtg");
                }
            }
        }
        private double ppm_for_calibration = 0.0;      //

        public double Ppm_for_calibration
        {
            get { return ppm_for_calibration; }
            set
            {
                if (value != ppm_for_calibration)
                {
                    ppm_for_calibration = value;
                    NotifyPropertyChanged("Ppm_for_calibration");
                }
            }
        }
        private double ppm_half_win_accuracy_peak = 15.0;  //

        public double Ppm_half_win_accuracy_peak
        {
            get { return ppm_half_win_accuracy_peak; }
            set
            {
                if (value != ppm_half_win_accuracy_peak)
                {
                    ppm_half_win_accuracy_peak = value;
                    NotifyPropertyChanged("Ppm_half_win_accuracy_peak");
                }
            }
        }
        private int number_hole_in_cmtg = 1;   //default=2,index=1

        public int Number_hole_in_cmtg
        {
            get { return number_hole_in_cmtg; }
            set
            {
                if (value != number_hole_in_cmtg)
                {
                    number_hole_in_cmtg = value;
                    NotifyPropertyChanged("Number_hole_in_cmtg");
                }
            }
        }

        private int type_same_start_end_between_evidence = (int)Mixed_Samples.Mixed_Samples_1_1;

        public int Type_same_start_end_between_evidence
        {
            get { return type_same_start_end_between_evidence; }
            set
            {
                if (value != type_same_start_end_between_evidence)
                {
                    type_same_start_end_between_evidence = value;
                    NotifyPropertyChanged("Type_same_start_end_between_evidence");
                }
            }
        }

        private int number_max_psm_per_block = 30000;

        public int Number_max_psm_per_block
        {
            get { return number_max_psm_per_block; }
            set { number_max_psm_per_block = value; }
        }

        private int type_start = 0;

        public int Type_start
        {
            get { return type_start; }
            set { type_start = value; }
        }

        private string extension_text_ms1 = "pf1";

        public string Extension_text_ms1
        {
            get { return extension_text_ms1; }
            set { extension_text_ms1 = value; }
        }

        //double threshold_fdr = 0.01;

        public Quant_Advanced() { }
        public Quant_Advanced(int lfec,int nhsc,double pfc,double phwap,int nhic,int tssebe,
            string _extension_text_ms1,int nmppb,int _type_start)
        {
            this.ll_element_enrichment_calibration = lfec;
            this.number_scans_half_cmtg = nhsc;
            this.ppm_for_calibration = pfc;
            this.ppm_half_win_accuracy_peak = phwap;
            this.number_hole_in_cmtg = nhic;
            this.type_same_start_end_between_evidence = tssebe;
            this.extension_text_ms1 = _extension_text_ms1;
            this.number_max_psm_per_block = nmppb;
            this.type_start = _type_start;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != this.GetType()) { return false; }
            Quant_Advanced lgad = obj as Quant_Advanced;
            if ((System.Object)lgad == null)
            {
                return false;
            }
            if (this.ll_element_enrichment_calibration != lgad.ll_element_enrichment_calibration) { return false; }
            if (this.number_scans_half_cmtg != lgad.number_scans_half_cmtg) { return false; }       
            if (this.ppm_for_calibration != lgad.ppm_for_calibration) { return false; }
            if (this.ppm_half_win_accuracy_peak != lgad.ppm_half_win_accuracy_peak) { return false; }
            if (this.number_hole_in_cmtg!=lgad.number_hole_in_cmtg) { return false; }
            if (this.type_same_start_end_between_evidence != lgad.type_same_start_end_between_evidence) { return false; }
            if (this.extension_text_ms1 != lgad.extension_text_ms1) { return false; }
            if (this.number_max_psm_per_block != lgad.number_max_psm_per_block) { return false; }
            if (this.type_start != lgad.type_start) { return false; }
            return true;
        }
        public override int GetHashCode()
        {
            int hc =this.ll_element_enrichment_calibration.GetHashCode()+ this.number_scans_half_cmtg.GetHashCode() + this.ppm_for_calibration.GetHashCode()
                + this.ppm_half_win_accuracy_peak.GetHashCode() + this.number_hole_in_cmtg.GetHashCode()
                + this.type_same_start_end_between_evidence.GetHashCode()+this.extension_text_ms1.GetHashCode()
                +this.number_max_psm_per_block.GetHashCode()+this.type_start.GetHashCode();
            return hc;
        }
        public void CopyTo(Quant_Advanced qa)
        {
            qa.ll_element_enrichment_calibration = this.ll_element_enrichment_calibration;
            qa.number_scans_half_cmtg = this.number_scans_half_cmtg;
            qa.ppm_for_calibration = this.ppm_for_calibration;
            qa.ppm_half_win_accuracy_peak = this.ppm_half_win_accuracy_peak;
            qa.number_hole_in_cmtg = this.number_hole_in_cmtg;
            qa.type_same_start_end_between_evidence = this.type_same_start_end_between_evidence;
            qa.extension_text_ms1 = this.extension_text_ms1;
            qa.number_max_psm_per_block = this.number_max_psm_per_block;
            qa.type_start = this.type_start;
        }

        string _err = null;
        public string Error
        {
            get { return _err; }
        }
        public string this[string columnName]
        {
            get
            {
                string result = null;
                switch (columnName)
                {
                    case "Number_scans_half_cmtg":
                        if (this.number_scans_half_cmtg < 0)
                        {
                            result = Message_Help.VALUE_ABOVE_ZERO;
                        }
                        break;
                    //case "Ppm_for_calibration":
                    //    if (this.ppm_for_calibration < 0)
                    //    {
                    //        result = Message_Help.VALUE_ABOVE_ZERO;
                    //    }
                    //    break;
                    case "Ppm_half_win_accuracy_peak":
                        if (this.ppm_half_win_accuracy_peak < 0)
                        {
                            result = Message_Help.VALUE_ABOVE_ZERO;
                        }
                        break;
                }
                return result;
            }
        }
    }

    // wrm20150820: 按照参数文件分离并添加几个参数
    public class Quant_Inference : INotifyPropertyChanged, IDataErrorInfo   //6个属性
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        private int type_peptide_ratio = 1;

        public int Type_peptide_ratio 
        {
            get { return type_peptide_ratio; }
            set
            {
                if (value != type_peptide_ratio)
                {
                    type_peptide_ratio = value;
                    //NotifyPropertyChanged("Type_peptide_ratio");
                }
            }
        }


        private int type_protein_ratio_calculation = 0;     

        public int Type_protein_ratio_calculation
        {
            get { return type_protein_ratio_calculation; }
            set
            {
                if (value != type_protein_ratio_calculation)
                {
                    type_protein_ratio_calculation = value;
                    //NotifyPropertyChanged("Type_protein_ratio_calculation");
                }
            }
        }

        private int type_unique_peptide_only = 0;

        public int Type_unique_peptide_only
        {
            get { return type_unique_peptide_only; }
            set { type_unique_peptide_only = value; }
        }

        private double threshold_score_interference = 1.0;      //

        public double Threshold_score_interference
        {
            get { return threshold_score_interference; }
            set
            {
                if (value != threshold_score_interference)
                {
                    threshold_score_interference = value;
                    //NotifyPropertyChanged("Threshold_score_interference");
                }
            }
        }
        private double threshold_score_intensity = 10000.0;  //

        public double Threshold_score_intensity
        {
            get { return threshold_score_intensity; }
            set
            {
                if (value != threshold_score_intensity)
                {
                    threshold_score_intensity = value;
                    //NotifyPropertyChanged("Threshold_score_intensity");
                }
            }
        }
        private int type_get_group = 1;   //default=1

        public int Type_get_group
        {
            get { return type_get_group; }
            set
            {
                if (value != type_get_group)
                {
                    type_get_group = value;
                    //NotifyPropertyChanged("Type_get_group");
                }
            }
        }

        //private string path_fasta = "";

        //public string Path_fasta
        //{
        //    get { return path_fasta; }
        //    set
        //    {
        //        if (value != path_fasta)
        //        {
        //            path_fasta = value;
        //            //NotifyPropertyChanged("Path_fasta");
        //        }
        //    }
        //}

        public Quant_Inference() { }
        public Quant_Inference(int tpr, int tprc, int tupo, double ts_interference, double ts_intensity, int tgg)
        {
            this.type_peptide_ratio = tpr;
            this.type_protein_ratio_calculation = tprc;
            this.type_unique_peptide_only = tupo;
            this.threshold_score_interference = ts_interference;
            this.threshold_score_intensity = ts_intensity;
            this.type_get_group = tgg;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != this.GetType()) { return false; }
            Quant_Inference q_infer = obj as Quant_Inference;
            if ((System.Object)q_infer == null)
            {
                return false;
            }
            if (this.type_peptide_ratio != q_infer.type_peptide_ratio) { return false; }
            if (this.type_protein_ratio_calculation != q_infer.type_protein_ratio_calculation) { return false; }
            if (this.type_unique_peptide_only != q_infer.type_unique_peptide_only) { return false; }
            if (this.threshold_score_interference != q_infer.threshold_score_interference) { return false; }
            if (this.threshold_score_intensity != q_infer.threshold_score_intensity) { return false; }
            if (this.type_get_group != q_infer.type_get_group) { return false; }
            return true;
        }
        public override int GetHashCode()
        {
            int hc = this.type_peptide_ratio.GetHashCode() + this.type_protein_ratio_calculation.GetHashCode()
                + this.type_unique_peptide_only.GetHashCode()
                + this.threshold_score_interference.GetHashCode() + this.threshold_score_intensity.GetHashCode()
                + this.type_get_group.GetHashCode();
              
            return hc;
        }
        public void CopyTo(Quant_Inference qa)
        {
            qa.type_peptide_ratio = this.type_peptide_ratio;
            qa.type_protein_ratio_calculation = this.type_protein_ratio_calculation;
            qa.type_unique_peptide_only = this.type_unique_peptide_only;
            qa.threshold_score_interference = this.threshold_score_interference;
            qa.threshold_score_intensity = this.threshold_score_intensity;
            qa.type_get_group = this.type_get_group;
        }

        string _err = null;
        public string Error
        {
            get { return _err; }
        }
        public string this[string columnName]
        {
            get
            {
                string result = null;
                switch (columnName)
                {
                    case "Threshold_score_interference":
                        if (this.threshold_score_interference < 0)
                        {
                            result = Message_Help.VALUE_ABOVE_ZERO;
                        }
                        break;
                    //case "Ppm_for_calibration":
                    //    if (this.ppm_for_calibration < 0)
                    //    {
                    //        result = Message_Help.VALUE_ABOVE_ZERO;
                    //    }
                    //    break;
                    case "Threshold_score_intensity":
                        if (this.threshold_score_intensity < 0)
                        {
                            result = Message_Help.VALUE_ABOVE_ZERO;
                        }
                        break;
                }
                return result;
            }
        }
    }



    public class Label
    {
        public Label() { }
        string label_name;
        public string Label_name
        {
            get { return label_name; }
            set { label_name = value; }
        }

        string label_element;
        public string Label_element
        {
            get { return label_element; }
            set { label_element = value; }
        }

        public Label(string _label_name,string _label_element) 
        {
            this.label_name = _label_name;
            this.label_element = _label_element;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != this.GetType()) { return false; }
            Label l = obj as Label;
            if ((System.Object)l == null)
            {
                return false;
            }
            if (this.label_name != l.label_name || this.label_element != l.label_element) { return false; }
            return true;
        }
        public override int GetHashCode()
        {
            int hc = this.label_name.GetHashCode() + this.label_element.GetHashCode();
            return hc;
        }
    }

    public class LabelFree:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        
        public LabelFree() { }
        private ObservableCollection<ObservableCollection<string>> lf_info_sample = new ObservableCollection<ObservableCollection<string>>();

        public ObservableCollection<ObservableCollection<string>> Lf_info_sample
        {
            get { return lf_info_sample; }
            set { lf_info_sample = value; }
        }

        public LabelFree(ObservableCollection<ObservableCollection<string>> _lf_info_sample)
        {
            this.lf_info_sample = _lf_info_sample;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != this.GetType()) { return false; }
            LabelFree lf = obj as LabelFree;
            if ((System.Object)lf == null)
            {
                return false;
            }
            if (this.lf_info_sample.Count != lf.lf_info_sample.Count) { return false; }
            for (int i = 0; i < this.lf_info_sample.Count; i++)
            {
                if (this.lf_info_sample[i].Count != lf.lf_info_sample[i].Count) { return false; }
                for (int j = 0; j < this.lf_info_sample[i].Count; j++)
                {
                    if (!(this.lf_info_sample[i][j].Equals(lf.lf_info_sample[i][j]))) { return false; }
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            return this.lf_info_sample.GetHashCode();
        }
        public void CopyTo(LabelFree lf)
        {
            lf.lf_info_sample.Clear();
            for (int i = 0; i < this.lf_info_sample.Count; i++)
            {
                ObservableCollection<string> one_lf_info=new ObservableCollection<string>();
                for (int j = 0; j < this.lf_info_sample[i].Count; j++)
                {
                   one_lf_info.Add(this.lf_info_sample[i][j]);
                }
                lf.lf_info_sample.Add(one_lf_info);
            }
        }

    }

    public enum MS2QuantitativeMethod : int
    {
        iTRAQ_4plex = 0,
        iTRAQ_8plex = 1,
        TMT_6plex = 2,
        TMT_10plex = 3,
        pIDL = 4
    }

    [Serializable]
    public class Modification_Mass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        private string name;
        public string Name 
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }
        private double mass;
        public double Mass 
        {
            get { return mass; }
            set
            {
                if (value != mass)
                {
                    mass = value;
                    NotifyPropertyChanged("Mass");
                }
            }
        }
        public Modification_Mass() { }
        public Modification_Mass(string _name, double _mass)
        {
            name = _name;
            mass = _mass;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != this.GetType()) { return false; }
            Modification_Mass mm = obj as Modification_Mass;
            if ((System.Object)mm == null)
            {
                return false;
            }
            if (name != mm.name) { return false; }
            if (mass != mm.mass) { return false; }
            return true;
        }

        public static bool operator ==(Modification_Mass a, Modification_Mass b)
        {
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }
            if (a.mass == b.mass && a.name == b.name) return true;
            return false;
        }

        public static bool operator !=(Modification_Mass a, Modification_Mass b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode()^mass.GetHashCode();
        }
    }

    [Serializable]
    public class pIDLplex : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        private Modification_Mass nterm_modmass;
        public Modification_Mass Nterm_modmass 
        {
            get{return nterm_modmass;}
            set
            {
                if (value != nterm_modmass)
                {
                    nterm_modmass = value;
                    NotifyPropertyChanged("Nterm_modmass");
                }
            }
        }
        private Modification_Mass cterm_modmass;
        public Modification_Mass Cterm_modmass
        { 
            get { return cterm_modmass;}
            set
            {
                if (value != cterm_modmass)
                {
                    cterm_modmass = value;
                    NotifyPropertyChanged("Cterm_modmass");
                }
            }
        }
        public pIDLplex() { }
        public pIDLplex(Modification_Mass _nterm, Modification_Mass _cterm)
        {
            nterm_modmass = _nterm;
            cterm_modmass = _cterm;
        }
        public pIDLplex(string mod1, double mass1, string mod2, double mass2)
        {
            nterm_modmass = new Modification_Mass(mod1, mass1);
            cterm_modmass = new Modification_Mass(mod2, mass2);
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != this.GetType()) { return false; }
            pIDLplex mm = obj as pIDLplex;
            if ((System.Object)mm == null)
            {
                return false;
            }
            if (cterm_modmass != mm.cterm_modmass) { return false; }
            if (nterm_modmass != mm.nterm_modmass) { return false; }
            return true;
        }

        public static bool operator ==(pIDLplex a, pIDLplex b)
        {
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }
            if (a.nterm_modmass == b.nterm_modmass && a.cterm_modmass == b.cterm_modmass) return true;
            return false;
        }

        public static bool operator !=(pIDLplex a, pIDLplex b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return nterm_modmass.GetHashCode()^cterm_modmass.GetHashCode();
        }
    }

    [Serializable]
    public class MS2Quant : INotifyPropertyChanged //, IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }


        private bool enable_ms2quant = false;

        public bool Enable_ms2quant
        {
            get { return enable_ms2quant; }
            set
            {
                if (value != enable_ms2quant)
                {
                    enable_ms2quant = value;
                    NotifyPropertyChanged("Enable_ms2quant");
                }
            }
        }

        private int quantitative_method = (int)MS2QuantitativeMethod.pIDL;
        public int QuantitativeMethod
        {
            get { return quantitative_method; }
            set
            {
                if (value != quantitative_method)
                {
                    quantitative_method = value;
                    NotifyPropertyChanged("QuantitativeMethod");
                }
            }
        }

        private ObservableCollection<double> reporterIonMZ = new ObservableCollection<double>();

        public ObservableCollection<double> ReporterIonMZ
        {
            get { return reporterIonMZ; }
            set 
            {
                if (value != reporterIonMZ)
                {
                    reporterIonMZ = value;
                    NotifyPropertyChanged("ReporterIonMZ");
                }
            }
        }

        private ObservableCollection<pIDLplex> pIDL = new ObservableCollection<pIDLplex>();

        public ObservableCollection<pIDLplex> PIDL
        {
            get { return pIDL; }
            set
            {
                if (value != pIDL)
                {
                    pIDL = value;
                    NotifyPropertyChanged("PIDL");
                }
            }
        }

        private MS2Quant_Advanced ms2_advanced = new MS2Quant_Advanced();

        public MS2Quant_Advanced MS2_Advanced
        {
            get { return ms2_advanced; }
            set 
            {
                if (value != ms2_advanced)
                {
                    ms2_advanced = value;
                    NotifyPropertyChanged("MS2_Advanced");
                }
            }
        }

        public MS2Quant() { }
        public MS2Quant(int _method)
        {
            this.quantitative_method = _method;
        }
        public MS2Quant(int _method, ObservableCollection<double> _reportions, ObservableCollection<pIDLplex> _pidl, MS2Quant_Advanced _ms2quant)
        {
            this.quantitative_method = _method;
            this.reporterIonMZ = _reportions;
            this.pIDL = _pidl;
            this.ms2_advanced = _ms2quant;
        }


        //
        public ObservableCollection<_Attribute> getMS2QuantParam()
        {
            ObservableCollection<_Attribute> ms2quant = new ObservableCollection<_Attribute>();
            string quantstr = "";
            switch (quantitative_method)
            {
                case (int)MS2QuantitativeMethod.iTRAQ_4plex: quantstr += "iTRAQ_4plex"; break;
                case (int)MS2QuantitativeMethod.iTRAQ_8plex: quantstr += "iTRAQ_8plex"; break;
                case (int)MS2QuantitativeMethod.TMT_6plex: quantstr += "TMT_6plex"; break;
                case (int)MS2QuantitativeMethod.TMT_10plex: quantstr += "TMT_10plex"; break;
                case (int)MS2QuantitativeMethod.pIDL: quantstr += "pIDL"; break;
            }
            ms2quant.Add(new _Attribute("Quantitation Method", quantstr, ""));
     
            if (quantitative_method == (int)MS2QuantitativeMethod.pIDL)
            {
                string pidl_modmass = "";
                if (this.pIDL.Count > 0)
                {
                    pidl_modmass += this.pIDL[0].Nterm_modmass.Name + " " + this.pIDL[0].Nterm_modmass.Mass.ToString() + ", ";
                    pidl_modmass += this.pIDL[0].Cterm_modmass.Name + " " + this.pIDL[0].Cterm_modmass.Mass.ToString();
                }
                else
                {
                    pidl_modmass = "null";
                }
                for (int i = 1; i < this.pIDL.Count; i++)
                {
                    pidl_modmass += "\n";
                    pidl_modmass += this.pIDL[i].Nterm_modmass.Name + " " + this.pIDL[i].Nterm_modmass.Mass.ToString() + ", ";
                    pidl_modmass += this.pIDL[i].Cterm_modmass.Name + " " + this.pIDL[i].Cterm_modmass.Mass.ToString();
                }
                ms2quant.Add(new _Attribute("pIDLplex", pidl_modmass, ""));
            }
            else
            {
                string reportionstr = "";
                if (this.reporterIonMZ.Count > 0)
                {
                    reportionstr += this.reporterIonMZ[0].ToString();
                }
                else 
                {
                    reportionstr = "null";
                }
                for (int i = 1; i < this.reporterIonMZ.Count; ++i)
                {
                    reportionstr += ", " + this.reporterIonMZ[i].ToString(); 
                }
                ms2quant.Add(new _Attribute("Reporter Ion MZ", reportionstr,""));
            }
            #region Advanced_Quant
            string tmp = this.MS2_Advanced.FTMS_Tolerance.Tl_value.ToString() + ((this.ms2_advanced.FTMS_Tolerance.Isppm == 1)?"ppm":"Da");
            ms2quant.Add(new _Attribute("Fragment Tolerance",tmp,""));
            tmp = this.ms2_advanced.Peak_Range.Left_value.ToString()+" - " +this.ms2_advanced.Peak_Range.Right_value.ToString();
            ms2quant.Add(new _Attribute("Peak Range",tmp,""));
            ms2quant.Add(new _Attribute("PIF",this.ms2_advanced.Pif,""));
            ms2quant.Add(new _Attribute("PSM FDR",this.ms2_advanced.Psm_Fdr.ToString()+"%",""));
            ms2quant.Add(new _Attribute("Protein FDR",this.ms2_advanced.Protein_Fdr.ToString()+"%",""));
            // 不显示
            //ms2quant.Add(new _Attribute("Correct Matrix", (this.ms2_advanced.Correct)?"True":"False", ""));
            //ms2quant.Add(new _Attribute("Run VSN", (this.ms2_advanced.RunVSN)?"True":"False", ""));
            #endregion
            return ms2quant;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            MS2Quant temp = obj as MS2Quant;
            if (temp == null) return false;
            return Equals(temp);
        }

        public bool Equals(MS2Quant other)
        {
            object value1, value2;
            PropertyInfo[] properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                if (!property.CanRead) continue;
                value1 = property.GetValue(this, null);
                value2 = property.GetValue(other, null);
                if (!value1.Equals(value2))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator ==(MS2Quant a, MS2Quant b)
        {
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }
            return a.Equals(b);
        }

        public static bool operator !=(MS2Quant a, MS2Quant b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            int hashcode = 0;
            object value;
            PropertyInfo[] properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                if (!property.CanRead) continue;
                value = property.GetValue(this, null);
                hashcode ^= value.GetHashCode();
            }
            return hashcode;
        }


    }
    // 二级谱定量的高级参数
    [Serializable]
    public class MS2Quant_Advanced : INotifyPropertyChanged   //, IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }


        private Tolerance FTMS_tolerance = new Tolerance(20,1);

        public Tolerance FTMS_Tolerance
        {
          get { return FTMS_tolerance; }
          set 
          {
              if (!value.Equals(FTMS_tolerance))
              {
                  FTMS_tolerance = value;
                  NotifyPropertyChanged("FTMS_Tolerance");
              }
          }
        }

        private Range peak_range = new Range(0,200);

        public Range Peak_Range
        {
            get { return peak_range; }
            set 
            {
                if (value != peak_range)
                {
                    peak_range = value;
                    NotifyPropertyChanged("Peak_Range");
                }
            }
        }

        private double pif = 0.75;

        public double Pif
        {
            get { return pif; }
            set 
            {
                if (value != pif)
                {
                    pif = value;
                    NotifyPropertyChanged("Pif");
                }
            }
        }

        private double psm_fdr = 1;  //%

        public double Psm_Fdr
        {
            get { return psm_fdr; }
            set
            {
                if (value != psm_fdr)
                {
                    psm_fdr = value;
                    NotifyPropertyChanged("Psm_Fdr");
                }
            }
        }

        private double protein_fdr = 1; // %

        public double Protein_Fdr
        {
            get { return protein_fdr; }
            set
            {
                if (value != protein_fdr)
                {
                    protein_fdr = value;
                    NotifyPropertyChanged("Protein_Fdr");
                }
            }
        }

        private bool correct = true;

        public bool Correct
        {
            get { return correct; }
            set
            {
                if (value != correct)
                {
                    correct = value;
                    NotifyPropertyChanged("Correct");
                }
            }
        }

        private bool runVSN = true;

        public bool RunVSN
        {
            get { return runVSN; }
            set
            {
                if (value != runVSN)
                {
                    runVSN = value;
                    NotifyPropertyChanged("RunVSN");
                }
            }
        }

        public MS2Quant_Advanced() { }

        public MS2Quant_Advanced(Tolerance _tol, Range _range, double _pif, double _psmfdr, double _profdr, bool _correct, bool _runvsn)
        {
            this.FTMS_tolerance = _tol;
            this.peak_range = Peak_Range;
            this.pif = _pif;
            this.psm_fdr = _psmfdr;
            this.protein_fdr = _profdr;
            this.correct = _correct;
            this.runVSN = _runvsn;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            MS2Quant_Advanced temp = obj as MS2Quant_Advanced;
            if (temp == null) return false;
            return Equals(temp);
        }

        public bool Equals(MS2Quant_Advanced other)
        {
            object value1, value2;
            PropertyInfo[] properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                if (!property.CanRead) continue;
                value1 = property.GetValue(this,null);
                value2 = property.GetValue(other,null);
                if (!value1.Equals(value2)) 
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator ==(MS2Quant_Advanced a, MS2Quant_Advanced b)
        { 
            if(System.Object.ReferenceEquals(a,b))
            {
                return true;
            }
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }
            return a.Equals(b);
        }

        public static bool operator !=(MS2Quant_Advanced a, MS2Quant_Advanced b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            int hashcode = 0;
            object value;
            PropertyInfo[] properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                if (!property.CanRead) continue;
                value = property.GetValue(this, null);
                hashcode ^= value.GetHashCode();
            }
            return hashcode;
        }
    }
}
