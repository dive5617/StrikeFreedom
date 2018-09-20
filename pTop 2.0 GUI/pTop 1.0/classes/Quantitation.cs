using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace pTop.classes
{
    public enum Quant_Type : int
    {
        Label_None = 0,
        Labeling = 1,
        Label_free = 3
    }

    public enum Mixed_Samples : int
    {
        Mixed_Samples_1_1 = 0,
        Mixed_Samples_10to1_or_1to10 = 1,
        Independent = 2
    }

    public enum LL_ELEMENT_ENRICHMENT_CALIBRATION : int
    {
        None = 0,
        N15 = 1,
        C13 = 2
    }

    public class LabelInfo
    {
        public LabelInfo() { }
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

        public LabelInfo(string _label_name, string _label_element)
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
            LabelInfo l = obj as LabelInfo;
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


    [Serializable]
    public class Quantitation : INotifyPropertyChanged    //4个
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        public Quantitation() { }

        int quantitation_type = (int)Quant_Type.Label_None;    //binding: 定量类型
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

        public Quantitation(int _quantitation)
        {
            this.quantitation_type = _quantitation;
        }

        public Quantitation(int _quantitation_type, Labeling __label, LabelFree __lf, Quant_Advanced qad)
        {
            this.quantitation_type = _quantitation_type;
            this._labeling = __label;
            this._lf = __lf;
            this.quant_advanced = qad;
        }

        public void Reset()
        {
            this.quantitation_type = (int)Quant_Type.Label_None;
            this._labeling.Multiplicity_index = 0;
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
                case (int)Quant_Type.Label_None: quantstr += "Label_None"; break;
                case (int)Quant_Type.Labeling: quantstr += "Labeling"; break;
                case (int)Quant_Type.Label_free: quantstr += "Label Free"; break;
            }
            quant.Add(new _Attribute("Quantitation", quantstr, ""));
            quant.Add(new _Attribute("Multiple Labeling", _labeling.Multiplicity.ToString(), ""));
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
            quant.Add(new _Attribute("TYPE_SAME_START_END_BETWEEN_EVIDENCE", tssebe, ""));
            string lfec = "none";
            if (this.quant_advanced.Ll_element_enrichment_calibration == (int)LL_ELEMENT_ENRICHMENT_CALIBRATION.N15)
            {
                lfec = "15N";
            }
            if (this.quant_advanced.Ll_element_enrichment_calibration == (int)LL_ELEMENT_ENRICHMENT_CALIBRATION.C13)
            {
                lfec = "13C";
            }
            quant.Add(new _Attribute("LL_ELEMENT_ENRICHMENT_CALIBRATION", lfec, ""));
            #endregion
            return quant;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != this.GetType()) { return false; }
            Quantitation qp = obj as Quantitation;
            if ((System.Object)qp == null)
            {
                return false;
            }
            if (this.quantitation_type != qp.quantitation_type) { return false; }
            if (!this._labeling.Equals(qp._labeling)) { return false; }
            if (!this.quant_advanced.Equals(qp.quant_advanced)) { return false; }
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
            int hc = this.quantitation_type.GetHashCode() + this._labeling.GetHashCode() + this._lf.GetHashCode()
                + this.quant_advanced.GetHashCode() + this.quant_inference.GetHashCode();
            return hc;
        }

    }

    [Serializable]
    public class Labeling : INotifyPropertyChanged     //5个属性
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

        private int multiplicity = 2;    //num of kinds of labels
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
            if (this.multiplicity != lg.multiplicity) { return false; }
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

    [Serializable]
    public class Quant_Advanced : INotifyPropertyChanged, IDataErrorInfo   //9个属性
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
        public Quant_Advanced(int lfec, int nhsc, double pfc, double phwap, int nhic, int tssebe,
            string _extension_text_ms1, int nmppb, int _type_start)
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
            if (this.number_hole_in_cmtg != lgad.number_hole_in_cmtg) { return false; }
            if (this.type_same_start_end_between_evidence != lgad.type_same_start_end_between_evidence) { return false; }
            if (this.extension_text_ms1 != lgad.extension_text_ms1) { return false; }
            if (this.number_max_psm_per_block != lgad.number_max_psm_per_block) { return false; }
            if (this.type_start != lgad.type_start) { return false; }
            return true;
        }
        public override int GetHashCode()
        {
            int hc = this.ll_element_enrichment_calibration.GetHashCode() + this.number_scans_half_cmtg.GetHashCode() + this.ppm_for_calibration.GetHashCode()
                + this.ppm_half_win_accuracy_peak.GetHashCode() + this.number_hole_in_cmtg.GetHashCode()
                + this.type_same_start_end_between_evidence.GetHashCode() + this.extension_text_ms1.GetHashCode()
                + this.number_max_psm_per_block.GetHashCode() + this.type_start.GetHashCode();
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
    [Serializable]
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
        private int type_peptide_ratio = 2;
        // 0  Mono峰; 1  干扰最小的峰; 2  最高峰
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

    public class LabelFree : INotifyPropertyChanged
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
                ObservableCollection<string> one_lf_info = new ObservableCollection<string>();
                for (int j = 0; j < this.lf_info_sample[i].Count; j++)
                {
                    one_lf_info.Add(this.lf_info_sample[i][j]);
                }
                lf.lf_info_sample.Add(one_lf_info);
            }
        }

    }
}
