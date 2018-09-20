using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace pFind
{
    public enum FDR_Type : int
    {
        spectra = 0,
        peptides = 1,
        proteins = 2,
    }
    
    public class FilterParam:INotifyPropertyChanged,IDataErrorInfo                           //6个属性
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        
        private FDR fdr=new FDR(1,1);     //binding: FDR
        [Category("Filter")]
        [DisplayName("FDR")]
        public FDR Fdr
        {
            get { return fdr; }
            set 
            {
                if (value != fdr)
                {
                    fdr = value;
                    NotifyPropertyChanged("Fdr");
                }
            }
        }
        private Range pep_mass_range=new Range(600,10000);    //binding: 肽段质量范围
        [Category("Filter")]
        [DisplayName("Peptide Mass")]
        public Range Pep_mass_range
        {
            get { return pep_mass_range; }
            set 
            {
                if (value != pep_mass_range)
                {
                    pep_mass_range = value;
                    NotifyPropertyChanged("Pep_mass_range");
                }
            }
        }
        private Range pep_length_range=new Range(6,100);  //binding: 肽段长度范围
        [Category("Filter")]
        [DisplayName("Peptide Length")]
        public Range Pep_length_range
        {
            get { return pep_length_range; }
            set 
            {
                if (value != pep_length_range)
                {
                    pep_length_range = value;
                    NotifyPropertyChanged("Pep_length_range");
                }
            }
        }

        private List<int> charge_states; //电荷状态
        [Category("Filter")]
        [DisplayName("Charge States")]
        public List<int> Charge_states
        {
            get { return charge_states; }
            set { charge_states = value; }
        }

        private int min_pep_num=1;         //binding: 最少肽段数量，无上下限限制
        [Category("Filter")]
        [DisplayName("Number of Peptides Per Protein≥")]
        public int Min_pep_num
        {
            get { return min_pep_num; }
            set 
            {
                if (value != min_pep_num)
                {
                    min_pep_num = value;
                    NotifyPropertyChanged("Min_pep_num");
                }
            }
        }

        #region 新增属性       
        private double protein_fdr = 1;     //binding: FDR
        [Category("Filter")]
        [DisplayName("Protein_FDR")]
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
        #endregion
        public FilterParam() { }
        public FilterParam(FDR _fdr,Range _pep_mass,Range _pep_length,int _min_pep_num,double _protein_fdr) {
            this.fdr = _fdr;
            this.pep_mass_range = _pep_mass;
            this.pep_length_range = _pep_length;           
            this.min_pep_num = _min_pep_num;
            this.protein_fdr = _protein_fdr;
        }

        //reset
        public void Reset()
        {
            this.fdr.Fdr_value=1;
            this.fdr.IsPeptides = 1;
            this.pep_length_range.Left_value=6;
            this.pep_length_range.Right_value = 100;
            this.pep_mass_range.Left_value=600;
            this.pep_mass_range.Right_value = 10000;
            this.min_pep_num = 1;
            this.protein_fdr = 1;
        }

        public ObservableCollection<_Attribute> getFilterParam()
        {
            ObservableCollection<_Attribute> ftr = new ObservableCollection<_Attribute>();
            string fdrstr = "Less than ";
            fdrstr += this.fdr.Fdr_value.ToString()+"%  ";
            fdrstr +="at "+(this.fdr.IsPeptides==1?"Peptides":"Spectra")+" Level";
            string fdr_check = "";
            if (this.fdr.Fdr_value < 0 || this.fdr.Fdr_value > 100) 
            {
                fdr_check = "invalid";
            }
            ftr.Add(new _Attribute("FDR", fdrstr,fdr_check));
            string massrangestr = "[";
            massrangestr += this.pep_mass_range.Left_value.ToString();
            massrangestr += " , ";
            massrangestr += this.pep_mass_range.Right_value.ToString();
            massrangestr += "]";
            string pep_mass_check = "";
            if ((this.pep_mass_range.Left_value > this.pep_mass_range.Right_value)||this.pep_mass_range.Right_value<0)
            {
                pep_mass_check = "invalid";
            }
            else if (this.pep_mass_range.Left_value < 0)
            {
                pep_mass_check = "pointless";
            }
            ftr.Add(new _Attribute("Peptide Mass", massrangestr,pep_mass_check));
            string lenrangestr = "[";
            lenrangestr += this.pep_length_range.Left_value.ToString();
            lenrangestr += " , ";
            lenrangestr += this.pep_length_range.Right_value.ToString();
            lenrangestr += "]";
            string pep_len_check = "";
            if ((this.pep_length_range.Left_value > this.pep_length_range.Right_value) || this.pep_length_range.Right_value < 0)
            {
                pep_len_check = "invalid";
            }
            else if (this.pep_length_range.Left_value < 0)
            {
                pep_len_check = "pointless";
            }
            ftr.Add(new _Attribute("Peptide Length", lenrangestr,pep_len_check));
            string min_pep_check = "";
            if (this.min_pep_num < 0)
            {
                min_pep_check = "pointless";
            }
            ftr.Add(new _Attribute("Number of Peptides Per Protein", "At least " + this.min_pep_num.ToString(),min_pep_check));
            string pro_fdr_check = "";
            if (this.protein_fdr < 0 || this.protein_fdr > 100)
            {
                pro_fdr_check = "invalid";
            }
            ftr.Add(new _Attribute("Protein FDR",this.protein_fdr.ToString()+"%",pro_fdr_check));     
            return ftr;
        }

        public Hashtable getFilterMap()
        {
            Hashtable ftr = new Hashtable();
            ftr.Add("FDR", this.fdr);
            ftr.Add("Peptide Mass", this.pep_mass_range);
            ftr.Add("Peptide Length", this.pep_length_range);
            ftr.Add("Number of Peptides Per Protein ≥", this.min_pep_num);
            ftr.Add("Protein_FDR",this.protein_fdr);
            return ftr;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            FilterParam ft = obj as FilterParam;
            if ((System.Object)ft == null)
            {
                return false;
            }
            if (!this.fdr.Equals(ft.fdr)) { return false; }
            if (!this.pep_mass_range.Equals(ft.pep_mass_range)) { return false; }
            if (!this.pep_length_range.Equals(ft.pep_length_range)) { return false; }
            if (this.min_pep_num != ft.min_pep_num) { return false; }
            if (this.protein_fdr != ft.protein_fdr) { return false; }
            return true;
        }
        public override int GetHashCode()
        {
            int hc = this.fdr.GetHashCode() + this.pep_mass_range.GetHashCode()
                + this.pep_length_range.GetHashCode() + this.min_pep_num.GetHashCode()+this.protein_fdr.GetHashCode();
            return hc;
        }
        public void copyTo(FilterParam fp)
        {
            fp.fdr.Fdr_value = this.fdr.Fdr_value;
            fp.fdr.IsPeptides = this.fdr.IsPeptides;
            fp.pep_mass_range.Left_value = this.pep_mass_range.Left_value;
            fp.pep_mass_range.Right_value = this.pep_mass_range.Right_value;
            fp.pep_length_range.Left_value = this.pep_length_range.Left_value;
            fp.pep_length_range.Right_value = this.pep_length_range.Right_value;
            fp.min_pep_num = this.min_pep_num;
            fp.protein_fdr = this.protein_fdr;
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
                    case "Fdr":
                        if (fdr.Fdr_value < 0 || fdr.Fdr_value > 100)
                        {
                            result = "The value of FDR must be between 0 and 100.";
                        }
                        break;
                    case "Protein_Fdr":
                        if (protein_fdr < 0 || protein_fdr > 100)
                        {
                            result = "The value of FDR must be between 0 and 100.";
                        }
                        break;
                    //case "Pep_mass_range":
                    case "Min_pep_num":
                        if (min_pep_num < 0)
                        {
                            result = "The value should not be less than 0.";
                        }
                        break;                  
                }
                return result;
            }
        }
    }

    public class Range:INotifyPropertyChanged,IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        private double left_value;

        public double Left_value
        {
          get { return left_value; }
          set 
          {
              if (value != left_value)
              {
                  left_value = value;
                  NotifyPropertyChanged("Left_value");
              }
          }
        }

        private double right_value;

        public double Right_value
        {
           get { return right_value; }
           set 
           {
               if (value != right_value)
               {
                   right_value = value;
                   NotifyPropertyChanged("Right_value");
               }
           }
        }
        public Range(){}

        public Range(double lf,double rt)
        {
            this.left_value=lf;
            this.right_value=rt;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            Range r = obj as Range;
            if ((System.Object)r == null)
            {
                return false;
            }
            if (this.left_value != r.left_value) { return false; }
            if (this.right_value != r.right_value) { return false; }
            return true;
        }

        public static bool operator ==(Range lhs, Range rhs)
        {
            if (lhs.Left_value == rhs.Left_value && lhs.Right_value == rhs.Right_value)
            {
                return true;
            }
            else 
            {
                return false;
            }
        }
        public static bool operator !=(Range lhs, Range rhs)
        {
            return !(lhs == rhs);
        }
        public override int GetHashCode()
        {
            return left_value.GetHashCode()+right_value.GetHashCode();
        }
        string _err = null;
        public string Error  //'pFind.FilterParam' must implement interface member 'System.ComponentModel.IDataErrorInfo.Error'

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
                    case "Right_value":
                        if (right_value < 0)
                        {
                            result = "The value cannot be less than 0.";
                        }
                        break;
                    
                    
                }
                return result;
            }
        }
    }

     public class FDR:INotifyPropertyChanged,IDataErrorInfo
     {

         public event PropertyChangedEventHandler PropertyChanged;
         protected void NotifyPropertyChanged(string propName)
         {
             if (this.PropertyChanged != null)
             {
                 PropertyChanged(this, new PropertyChangedEventArgs(propName));
             }
         }

        private double fdr_value=1.0;    //binding

        public double Fdr_value
        {
           get { return fdr_value; }
           set 
           {
               if (value != fdr_value)
               {
                   fdr_value = value;
                   NotifyPropertyChanged("Fdr_value");
               }
           }
        }

        private int isPeptides=(int)FDR_Type.peptides;      //binding： 0为spectra，1为peptide 

        public int IsPeptides
        {
           get { return isPeptides; }
           set 
           {
               if (value != isPeptides)
               {
                   isPeptides = value;
                   NotifyPropertyChanged("IsPeptides");
               }
           }
        }
       
        public FDR(){}
        public FDR(double fdr,int ispep){
           this.fdr_value=fdr;
           this.isPeptides=ispep;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            FDR r = obj as FDR;
            if ((System.Object)r == null)
            {
                return false;
            }
            if (this.fdr_value != r.fdr_value) { return false; }
            if (this.isPeptides != r.isPeptides) { return false; }
            return true;
        }
        public override int GetHashCode()
        {
            return this.fdr_value.GetHashCode() + this.isPeptides.GetHashCode();
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
                    case "Fdr_value":
                        if (fdr_value < 0 || fdr_value > 100)
                        {
                            result = "The value of FDR must be between 0 and 100.";
                        }
                        break;

                }
                return result;
            }
        }
     }
}
