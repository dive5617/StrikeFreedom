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

namespace pFind
{
    public enum FormatOptions : int
    {
        MGF = 0,      
        RAW = 1,
        WIFF = 2
    }
    public enum InstrumentOptions : int
    {                                                           
        HCD_FTMS = 0,
        HCD_ITMS = 1,
        CID_FTMS = 2,
        CID_ITMS = 3
    }
    public enum ModelOptions : int
    {
        Normal = 4,      //常规模型，9个特征
        N15 = 15        //15N模型
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
                case (int)FormatOptions.WIFF: file_format = "wiff";
                    break;
            }
        }
        
        private int instrument_index=(int)InstrumentOptions.HCD_FTMS;    //binding:default instrument index

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

        private string instrument="HCD-FTMS";          //仪器类型
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
                case (int)InstrumentOptions.CID_FTMS: instrument = "CID-FTMS"; break;                
                case (int)InstrumentOptions.HCD_FTMS: instrument = "HCD-FTMS"; break;
                case (int)InstrumentOptions.CID_ITMS: instrument = "CID-ITMS"; break;
                case (int)InstrumentOptions.HCD_ITMS: instrument = "HCD-ITMS"; break;
            }
        }
        public void setInstrument_index()
        {
            if (instrument.Equals("CID-FTMS")) 
            {
               instrument_index=(int)InstrumentOptions.CID_FTMS;
            }
            else if(instrument.Equals("HCD-FTMS"))
            {
               instrument_index=(int)InstrumentOptions.HCD_FTMS;
            }
            else if(instrument.Equals("CID-ITMS"))
            {
               instrument_index=(int)InstrumentOptions.CID_ITMS;
            }
            else if (instrument.Equals("HCD-ITMS"))
            {
                instrument_index = (int)InstrumentOptions.HCD_ITMS;
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
               

        private bool mix_spectra=true;           //binding: 是否导出混合谱
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

        private int mz_decimal_index=4;         //binding: default m/z index

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
        private int mz_decimal=5;             //m/z保留的小数点位数
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

        private int intensity_decimal_index=0;   //binding: default intensity index

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

        private int intensity_decimal=1;      //强度保留的小数点位数
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

        private int model=(int)ModelOptions.Normal;                 
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
        private int modelindex = 0;                 //binding: 模型
        [Category("File")]
        public int ModelIndex
        {
            get { return modelindex; }
            set
            {
                if (value != modelindex)
                {
                    modelindex = value;
                    NotifyPropertyChanged("ModelIndex");
                }
            }
        }

        private double threshold=-0.5;          //binding: 阈值
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


        public File(int _file_format_index, int _instrument_index, bool _mix_spectra, int _mz_decimal_index,
            int _intensity_decimal_index,int _model,double _threshold)
        {
            this.file_format_index = _file_format_index;
            this.setFileFormat();
            this.instrument_index = _instrument_index;
            this.setInstrument();          
            this.mix_spectra = _mix_spectra;
            this.mz_decimal_index = _mz_decimal_index;
            this.setMZDecimal();
            this.intensity_decimal_index = _intensity_decimal_index;
            this.setIntensityDecimal();
            this.model = _model;
            this.threshold = _threshold;
        
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
                fl.Add(new _Attribute("Mixture Spectra", this.mix_spectra,""));
                fl.Add(new _Attribute("Decimal Places of M/Z", this.mz_decimal,""));
                fl.Add(new _Attribute("Decimal Places of Intensity", this.intensity_decimal,""));
                string modstr="";
                if (this.model == (int)ModelOptions.Normal) {
                    modstr += "Normal";
                }
                else if (this.model == (int)ModelOptions.N15) {
                    modstr += "15N";
                }
                fl.Add(new _Attribute("Model", modstr,""));
                #region 大小检查
                #endregion
                string thresh_check = "";
                if (this.threshold < -1000 || this.threshold > 1000)
                {
                    thresh_check = "invalid";
                }
                fl.Add(new _Attribute("Threshold", this.threshold,thresh_check));
            }
            return fl;
        }

        public Hashtable getFileMap()  // 功能被getFileParam取代，可删除
        {
            Hashtable fl = new Hashtable();
            fl.Add("Format", this.file_format);
            fl.Add("Instrument", this.instrument);
            fl.Add("Data File List", this.data_file_list);
            fl.Add("Mixture Spectra", this.mix_spectra);
            fl.Add("Decimal Places Of M/Z", this.mz_decimal);
            fl.Add("Decimal Places Of Intensity", this.intensity_decimal);
            fl.Add("Model", this.model);
            fl.Add("Threshold", this.threshold);
            return fl;
        }

        public void Reset()     //reset，暂时没用
        {
            file_format_index = (int)FormatOptions.RAW;
            this.setFileFormat();
            instrument_index = (int)InstrumentOptions.HCD_FTMS;
            this.setInstrument();
            data_file_list.Clear();
            this.mix_spectra = true;
            this.mz_decimal_index = 4;
            this.setMZDecimal();           
            this.intensity_decimal_index = 0;
            this.setIntensityDecimal();
            this.model = (int)ModelOptions.Normal;
            this.threshold = -0.5;
            #region pParse_Advanced
            #endregion
        }

        public override bool Equals(object obj)   // TODO: 可通过序列化判等
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
            if (mix_spectra != f.mix_spectra) { return false; }
            if (mz_decimal_index != f.mz_decimal_index) { return false; }
            if (mz_decimal != f.mz_decimal) { return false; }
            if (intensity_decimal_index != f.intensity_decimal_index) { return false; }
            if (intensity_decimal != f.intensity_decimal) { return false; }
            if (model != f.model) { return false; }
            if (threshold != f.threshold) { return false; }
            if (!this.pparse_advanced.Equals(f.pparse_advanced)) { return false; }
            //MessageBox.Show(base.Equals(obj).ToString());
            return true;
        }

        public override int GetHashCode()
        {
            int hc =this.file_format_index.GetHashCode() + this.file_format.GetHashCode()
                + this.instrument_index.GetHashCode() + this.instrument.GetHashCode()
                + this.data_file_list.GetHashCode() + this.mix_spectra.GetHashCode()
                +this.mz_decimal_index.GetHashCode() + this.mz_decimal.GetHashCode()
                + this.intensity_decimal_index.GetHashCode() + this.intensity_decimal.GetHashCode()
                + this.model.GetHashCode() + this.threshold.GetHashCode()+this.pparse_advanced.GetHashCode();
            return hc;
        }
        /*将该类复制到一个新的类中*/
        public void copyTo(File fl)  // 可通过序列化来进行深拷贝 T Copy_Inter.DeepCopyWithXmlSerializer<T>(T obj)
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
            fl.mix_spectra = this.mix_spectra;
            fl.mz_decimal_index = this.mz_decimal_index;
            fl.mz_decimal = this.mz_decimal;
            fl.intensity_decimal_index = this.intensity_decimal_index;
            fl.intensity_decimal = this.intensity_decimal;
            fl.model = this.model;
            fl.threshold = this.threshold;
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
       
        private double isolation_width = 2;

        public double Isolation_width
        {
            get { return isolation_width; }
            set { isolation_width = value; }
        }

        private string ipv_file = ".\\IPV.txt";

        public string Ipv_file
        {
            get { return ipv_file; }
            set { ipv_file = value; }
        }

        private string trainingset = "EmptyPath";    //".\\TrainingSet.txt";

        public string Trainingset
        {
            get { return trainingset; }
            set { trainingset = value; }
        }

        private int output_mars_y = 0;

        public int Output_mars_y
        {
            get { return output_mars_y; }
            set { output_mars_y = value; }
        }

        private int output_msn = 1;
        public int Output_msn
        {
            get { return output_msn; }
            set
            {
                if (value != output_msn)
                {
                    output_msn = value;
                    NotifyPropertyChanged("Output_msn");
                }
            }
        }

        private int output_mgf = 0;   // 0/1
        public int Output_mgf
        {
            get { return output_mgf; }
            set
            {
                if (value != output_mgf)
                {
                    output_mgf = value;
                    NotifyPropertyChanged("Output_mgf");
                }
            }
        }

        private int output_pf = 1;
        public int Output_pf
        {
            get { return output_pf; }
            set
            {
                if (value != output_pf)
                {
                    output_pf = value;
                    NotifyPropertyChanged("Output_pf");
                }
            }
        }

        private int debug_mode = 0;

        public int Debug_mode
        {
            get { return debug_mode; }
            set { debug_mode = value; }
        }

        private int check_activationcenter = 1;

        public int Check_activationcenter
        {
            get { return check_activationcenter; }
            set { check_activationcenter = value; }
        }

        private int output_all_mars_y = 0;

        public int Output_all_mars_y
        {
            get { return output_all_mars_y; }
            set { output_all_mars_y = value; }
        }

        private int rewrite_files = 0;

        public int Rewrite_files
        {
            get { return rewrite_files; }
            set { rewrite_files = value; }
        }

        private int export_unchecked_mono = 0;

        public int Export_unchecked_mono
        {
            get { return export_unchecked_mono; }
            set { export_unchecked_mono = value; }
        }

        private int cut_similiar_mono = 1;

        public int Cut_similiar_mono
        {
            get { return cut_similiar_mono; }
            set { cut_similiar_mono = value; }
        }

        private int output_trainingdata = 0;   

        public int Output_trainingdata
        {
            get { return output_trainingdata; }
            set { output_trainingdata = value; }
        }

        public pParse_Advanced() { }

        public pParse_Advanced(double _isolation_width,string _ipv_file,string _trainingset,int _output_mars_y,
            int _output_msn, int _output_mgf,int _output_pf,int _debug_mode,int _check_activationcenter,int _output_all_mars_y,
            int _rewrite_files,int _export_unchecked_mono,int _cut_similiar_mono,int _output_trainingdata)
        {
            this.isolation_width = _isolation_width;
            this.ipv_file = _ipv_file;
            this.trainingset = _trainingset;
            this.output_mars_y = _output_mars_y;
            this.output_msn = _output_msn;
            this.output_mgf = _output_mgf;
            this.output_pf = _output_pf;
            this.debug_mode = _debug_mode;
            this.check_activationcenter = _check_activationcenter;
            this.output_all_mars_y = _output_all_mars_y;
            this.rewrite_files = _rewrite_files;
            this.export_unchecked_mono = _export_unchecked_mono;
            this.cut_similiar_mono = _cut_similiar_mono;
            this.output_trainingdata = _output_trainingdata;
        }

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
            if (!ipv_file.Equals(pa.ipv_file)) { return false; }
            if (!trainingset.Equals(pa.trainingset)) { return false; }
            if (this.output_mars_y != pa.output_mars_y) { return false; }
            if (this.output_msn != pa.output_msn) { return false; }
            if (this.output_mgf != pa.output_mgf) { return false; }
            if (this.output_pf != pa.output_pf) { return false; }
            if (this.debug_mode != pa.debug_mode) { return false; }
            if (this.check_activationcenter != pa.check_activationcenter) { return false; }
            if (this.output_all_mars_y != pa.output_all_mars_y) { return false; }
            if (this.rewrite_files != pa.rewrite_files) { return false; } 
            if (this.export_unchecked_mono != pa.export_unchecked_mono) { return false; } 
            if (this.cut_similiar_mono != pa.cut_similiar_mono) { return false; }
            if (this.output_trainingdata != pa.output_trainingdata) { return false; }
            return true;
        }

        public override int GetHashCode()
        {
            int hc = this.isolation_width.GetHashCode() + this.ipv_file.GetHashCode() + this.trainingset.GetHashCode()
                + this.output_mars_y.GetHashCode() + this.output_msn.GetHashCode() + this.output_mgf.GetHashCode()
                + this.output_pf.GetHashCode() + this.debug_mode.GetHashCode() + this.check_activationcenter.GetHashCode()
                + this.output_all_mars_y.GetHashCode() + this.rewrite_files.GetHashCode() + this.export_unchecked_mono.GetHashCode()
                + this.cut_similiar_mono.GetHashCode() + this.output_trainingdata.GetHashCode();
            return hc;
        }

        public void copyTo(pParse_Advanced pa)
        {
            pa.isolation_width = this.isolation_width;
            pa.ipv_file = string.Copy(this.ipv_file);
            pa.trainingset = string.Copy(this.trainingset);
            pa.output_mars_y = this.output_mars_y;
            pa.output_msn = this.output_msn;
            pa.output_mgf = this.output_mgf;
            pa.output_pf = this.output_pf;
            pa.debug_mode = this.debug_mode;
            pa.check_activationcenter = this.check_activationcenter;
            pa.output_all_mars_y = this.output_all_mars_y;
            pa.rewrite_files = this.rewrite_files;
            pa.export_unchecked_mono = this.export_unchecked_mono;
            pa.cut_similiar_mono = this.cut_similiar_mono;
            pa.output_trainingdata = this.output_trainingdata;
        }
    }
}
