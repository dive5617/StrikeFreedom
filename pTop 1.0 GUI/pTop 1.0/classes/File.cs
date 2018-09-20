using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows;

namespace pTop.classes
{
    public enum FormatOptions : int
    {
        MGF = 0,      
        RAW = 1
    }
    public enum InstrumentOptions : int
    {        
        HCD = 0,                                                   
        CID = 1,
        ETD = 2,
        UVPD = 3
    }
    public enum ModelOptions : int
    {
        SVM = 0,      //SVM
        MARS = 1        //mars模型
    }

    public class File:INotifyPropertyChanged,IDataErrorInfo              //12个属性
    {
        public File()
        { 
        
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this,new PropertyChangedEventArgs(propName));
            }
        }

        private int file_format_index=(int)FormatOptions.RAW;   //binding:default format index

        public int File_format_index
        {
            get { return file_format_index; }
            set 
            {
                if (value != file_format_index)
                {
                    file_format_index = value;
                    NotifyPropertyChanged("File_format_index");    //改变时通知
                }
            }
        }

        private string file_format="raw";         //文件格式              
        [Category("File")]
        [DisplayName("Format")]
        public string File_format
        {
            get { return file_format; }
            set { file_format = value; }
        }
        public void setFileFormat()
        {
            switch (file_format_index)
            {
                case (int)FormatOptions.MGF: file_format = "mgf";
                    break;               
                case (int)FormatOptions.RAW: file_format = "raw";
                    break;
            }
        }
        
        private int instrument_index=(int)InstrumentOptions.HCD;    //binding:default instrument index

        public int Instrument_index
        {
            get { return instrument_index; }
            set 
            {
                if (value != instrument_index)
                {
                    instrument_index = value;
                    NotifyPropertyChanged("Instrument_index");
                }
            }
        }

        private string instrument="HCD";          //仪器类型
        [Category("File")]
        public string Instrument
        {
            get { return instrument; }
            set { instrument = value; }
        }

        public void setInstrument()
        {
            switch (instrument_index)
            {
                case (int)InstrumentOptions.CID: instrument = "CID"; break;                
                case (int)InstrumentOptions.HCD: instrument = "HCD"; break;
                case (int)InstrumentOptions.ETD: instrument = "ETD"; break;
                case (int)InstrumentOptions.UVPD: instrument = "UVPD"; break;
            }
        }
        public void setInstrument_index()
        {
            if (instrument.Equals("CID")) 
            {
               instrument_index=(int)InstrumentOptions.CID;
            }
            else if(instrument.Equals("HCD"))
            {
               instrument_index=(int)InstrumentOptions.HCD;
            }
            else if(instrument.Equals("ETD"))
            {
               instrument_index=(int)InstrumentOptions.ETD;
            }
            else if (instrument.Equals("UVPD"))
            {
                instrument_index = (int)InstrumentOptions.UVPD;
            }
        }

        private ObservableCollection<_FileInfo> data_file_list=new ObservableCollection<_FileInfo>(); //数据文件列表
        [Category("File")]
        [DisplayName("Data File List")]
        public ObservableCollection<_FileInfo> Data_file_list
        {
            get { return data_file_list; }
            set 
            {
                if (value != data_file_list)
                {
                    data_file_list = value;
                    NotifyPropertyChanged("Data_file_list");
                }
            }
        }   
       
        private pParse_Advanced pparse_advanced = new pParse_Advanced();

        public pParse_Advanced Pparse_advanced
        {
            get { return pparse_advanced; }
            set
            {
                if (value != pparse_advanced)
                {
                    pparse_advanced = value;
                    NotifyPropertyChanged("Pparse_advanced");
                }
            }
        }


        public ObservableCollection<_Attribute> getFileParam()
        {
            ObservableCollection<_Attribute> fl = new ObservableCollection<_Attribute>();
            fl.Add(new _Attribute("Format",this.file_format.ToUpper(),""));
            fl.Add(new _Attribute("Instrument",this.instrument,""));
            string datafiles="";
            if (data_file_list.Count > 0)
            {
                datafiles += data_file_list[0].FilePath.ToString();
            }
            else 
            {
                datafiles = "null";
            }
            for(int i=1;i<data_file_list.Count;i++){
              datafiles +="\n";
              datafiles+=data_file_list[i].FilePath.ToString();             
            }
            fl.Add(new _Attribute("Data File List",datafiles,""));
            if (file_format.Equals("raw"))
            {
                fl.Add(new _Attribute("Isolation Width", this.pparse_advanced.Isolation_width, ""));
                fl.Add(new _Attribute("Mixture Spectra", this.pparse_advanced.Mix_spectra,""));                
                string modstr="";
                if (this.pparse_advanced.Model == (int)ModelOptions.SVM) {
                    modstr += "svm";
                }
                else if (this.pparse_advanced.Model == (int)ModelOptions.MARS) {
                    modstr += "mars";
                }
                //fl.Add(new _Attribute("Model", modstr,""));
                #region 大小检查
                #endregion
                if (this.pparse_advanced.Model == (int)ModelOptions.MARS)
                {
                    string thresh_check = "";
                    if (this.pparse_advanced.Threshold < -1000 || this.pparse_advanced.Threshold > 1000)
                    {
                        thresh_check = "invalid";
                    }
                    fl.Add(new _Attribute("Threshold", this.pparse_advanced.Threshold, thresh_check));
                }
                fl.Add(new _Attribute("Max Charge", this.pparse_advanced.Max_charge, ""));
                fl.Add(new _Attribute("M/Z Tolerance", this.pparse_advanced.Mz_tolerance, ""));
                fl.Add(new _Attribute("Max Mass", this.pparse_advanced.Max_mass, ""));
                fl.Add(new _Attribute("S/N Ratio", this.pparse_advanced.Sn_ratio, ""));
                fl.Add(new _Attribute("Decimal Places of M/Z", this.pparse_advanced.Mz_decimal, ""));
                fl.Add(new _Attribute("Decimal Places of Intensity", this.pparse_advanced.Intensity_decimal, ""));
            }
            return fl;
        }


        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            File f = obj as File;
            if ((System.Object)f == null)
            {
                return false;
            }
            if (file_format_index != f.file_format_index) { return false; }
            if (file_format != f.file_format) { return false; }
            if (instrument_index != f.instrument_index) { return false; }
            if (instrument != f.instrument) { return false; }
            if (data_file_list.Count != f.data_file_list.Count) { return false; }
            for (int i = 0; i < this.data_file_list.Count; i++)
            {
                if (!(this.data_file_list[i].Equals(f.data_file_list[i]))) { return false; }
            } 
            if (!this.pparse_advanced.Equals(f.pparse_advanced)) { return false; }
            //MessageBox.Show(base.Equals(obj).ToString());
            return true;
        }

        public override int GetHashCode()
        {
            int hc =this.file_format_index.GetHashCode() + this.file_format.GetHashCode()
                + this.instrument_index.GetHashCode() + this.instrument.GetHashCode()
                + this.data_file_list.GetHashCode() + this.pparse_advanced.GetHashCode();
            return hc;
        }
        /*将该类复制到一个新的类中*/
        public void copyTo(File fl)
        {
            fl.file_format_index = this.file_format_index;
            fl.file_format=string.Copy(this.file_format);
            fl.instrument_index = this.instrument_index;
            fl.instrument = string.Copy(this.instrument);
            fl.data_file_list.Clear();
            for (int i = 0; i < this.data_file_list.Count; i++) 
            {
                fl.data_file_list.Add(this.data_file_list[i]);
            }
            this.pparse_advanced.copyTo(fl.pparse_advanced);
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
                    
                }
                return result;
            }
        }
    
    }

    public class _FileInfo
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string FileSize { get; set; }
        public string Type { get; set; }

        public _FileInfo(string FullPath) 
        {
            try 
            {
                this.FilePath = FullPath;
                FileInfo fileInf = new FileInfo(FullPath);
                this.Name = fileInf.Name;
                this.FileSize = (Math.Round(fileInf.Length /1024.0001/1024.0001,3)).ToString() + "MB";
                this.Type = this.Name.Substring(this.Name.LastIndexOf(".")+1);
            
            }catch(System.Exception e)
            {
                #region Todo
                //异常处理
                #endregion              
                MessageBox.Show(e.Message);            
                //throw new Exception(e.Message);
            }
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != this.GetType()) { return false; }
            _FileInfo fi = obj as _FileInfo;
            if ((System.Object)fi == null)
            {
                return false;
            }
            if (Name != fi.Name) { return false; }
            if (FilePath != fi.FilePath) { return false; }
            if (FileSize != fi.FileSize) { return false; }
            if (Type != fi.Type) { return false; }
            return true;
        }
        public override int GetHashCode()
        {
            int hc = this.Name.GetHashCode() + this.FilePath.GetHashCode() + this.FileSize.GetHashCode() + this.Type.GetHashCode();
            return hc;
        }

        public void copyTo(_FileInfo fi)
        {
            fi.Name = string.Copy(this.Name);
            fi.FilePath = string.Copy(this.FilePath);
            fi.FileSize = string.Copy(this.FileSize);
            fi.Type = string.Copy(this.Type);
        }
    }

    public class _Attribute 
    {
        public string _property { get; set; }
        public object _value { get; set; }
        public string _check { get; set; }
        public _Attribute(string __property,object __value,string __check) {
            this._property = __property;
            this._value = __value;
            this._check = __check;
        }    
    }

    public class pParse_Advanced:INotifyPropertyChanged     //14个不常变更的属性
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
       
        private double isolation_width = 10;

        public double Isolation_width
        {
            get { return isolation_width; }
            set
            {
                if (value != isolation_width)
                {
                    isolation_width = value;
                    NotifyPropertyChanged("Isolation_width");
                }
            }            
        }

        private bool mix_spectra = true;           //binding: 是否导出混合谱
        [Category("File")]
        [DisplayName("Whether Mixture Spectra")]
        public bool Mix_spectra
        {
            get { return mix_spectra; }
            set
            {
                if (value != mix_spectra)
                {
                    mix_spectra = value;
                    NotifyPropertyChanged("Mix_spectra");
                }
            }
        }

        private int model = (int)ModelOptions.SVM;
        [Category("File")]
        public int Model
        {
            get { return model; }
            set
            {
                if (value != model)
                {
                    model = value;
                    NotifyPropertyChanged("Model");
                }
            }
        }

        private double threshold = -0.68;          //binding: 阈值
        [Category("File")]
        public double Threshold
        {
            get { return threshold; }
            set
            {
                if (value != threshold)
                {
                    threshold = value;
                    NotifyPropertyChanged("Threshold");
                }
            }
        }

        private int max_charge = 30;          //binding: 阈值
        [Category("File")]
        public int Max_charge
        {
            get { return max_charge; }
            set
            {
                if (value != max_charge)
                {
                    max_charge = value;
                    NotifyPropertyChanged("Max_charge");
                }
            }
        }

        private double mz_tolerance = 20;          //binding: 阈值   ppm
        [Category("File")]
        public double Mz_tolerance
        {
            get { return mz_tolerance; }
            set
            {
                if (value != mz_tolerance)
                {
                    mz_tolerance = value;
                    NotifyPropertyChanged("Mz_tolerance");
                }
            }
        }

        private double max_mass = 50000.0;          //binding: 阈值   
        [Category("File")]
        public double Max_mass
        {
            get { return max_mass; }
            set
            {
                if (value != max_mass)
                {
                    max_mass = value;
                    NotifyPropertyChanged("Max_mass");
                }
            }
        }

        private double sn_ratio = 1.5;          //binding: 阈值   
        [Category("File")]
        public double Sn_ratio
        {
            get { return sn_ratio; }
            set
            {
                if (value != sn_ratio)
                {
                    sn_ratio = value;
                    NotifyPropertyChanged("Sn_ratio");
                }
            }
        }

        private int mz_decimal_index = 4;         //binding: default m/z index

        public int Mz_decimal_index
        {
            get { return mz_decimal_index; }
            set
            {
                if (value != mz_decimal_index)
                {
                    mz_decimal_index = value;
                    NotifyPropertyChanged("Mz_decimal_index");
                }
            }
        }
        private int mz_decimal = 5;             //m/z保留的小数点位数
        [Category("File")]
        [DisplayName("Decimal Places of M/Z")]
        public int Mz_decimal
        {
            get { return mz_decimal; }
            set { mz_decimal = value; }
        }

        public void setMZDecimal()
        {
            mz_decimal = mz_decimal_index + 1;
        }


        private int intensity_decimal_index = 0;   //binding: default intensity index

        public int Intensity_decimal_index
        {
            get { return intensity_decimal_index; }
            set
            {
                if (value != intensity_decimal_index)
                {
                    intensity_decimal_index = value;
                    NotifyPropertyChanged("Intensity_decimal_index");
                }
            }
        }

        private int intensity_decimal = 1;      //强度保留的小数点位数
        [Category("File")]
        [DisplayName("Decimal Places of Intensity")]
        public int Intensity_decimal
        {
            get { return intensity_decimal; }
            set { intensity_decimal = value; }
        }

        public void setIntensityDecimal()
        {
            intensity_decimal = intensity_decimal_index + 1;
        }    

        public pParse_Advanced() { }

        //public void Reset()   //reset
        //{ 
            
        //}

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            pParse_Advanced pa = obj as pParse_Advanced;
            if ((System.Object)pa == null)
            {
                return false;
            }
            if (isolation_width!=pa.isolation_width) { return false; }
            if (this.mix_spectra!= pa.mix_spectra) { return false; }
            if (this.model!=pa.model) { return false; }
            if (this.threshold != pa.threshold) { return false; }
            if (this.max_charge != pa.max_charge) { return false; }
            if (this.mz_tolerance != pa.mz_tolerance) { return false; }
            if (this.max_mass != pa.max_mass) { return false; }
            if (this.sn_ratio != pa.sn_ratio) { return false; }
            if (this.mz_decimal_index != pa.mz_decimal_index) { return false; }           
            if (this.mz_decimal != pa.mz_decimal) { return false; }
            if (this.intensity_decimal_index != pa.intensity_decimal_index) { return false; } 
            if (this.intensity_decimal != pa.intensity_decimal) { return false; } 
           
            return true;
        }

        public override int GetHashCode()
        {
            int hc = this.isolation_width.GetHashCode() + this.mix_spectra.GetHashCode() + this.model.GetHashCode()
                + this.threshold.GetHashCode() + this.max_charge.GetHashCode() + this.mz_tolerance.GetHashCode()
                + this.max_mass.GetHashCode() + this.sn_ratio.GetHashCode() + this.mz_decimal_index.GetHashCode()
                + this.mz_decimal.GetHashCode() + this.intensity_decimal_index.GetHashCode() + this.intensity_decimal.GetHashCode();           
            return hc;
        }

        public void copyTo(pParse_Advanced pa)
        {
            pa.isolation_width = this.isolation_width;
            pa.mix_spectra = this.mix_spectra;
            pa.model = this.model;
            pa.threshold = this.threshold;
            pa.max_charge = this.max_charge;
            pa.mz_tolerance = this.mz_tolerance;
            pa.max_mass = this.max_mass;
            pa.sn_ratio = this.sn_ratio;
            pa.mz_decimal_index = this.mz_decimal_index;
            pa.mz_decimal = this.mz_decimal;
            pa.intensity_decimal = this.intensity_decimal;
            pa.intensity_decimal_index = this.intensity_decimal_index;
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
                    case "Threshold":
                        if (threshold < -1000.0 || threshold > 1000.0)
                        {
                            result = "Threshold for MARS model should not be less than -1000 or larger than 1000.";
                        }
                        break;
                }
                return result;
            }
        }
    }
}
