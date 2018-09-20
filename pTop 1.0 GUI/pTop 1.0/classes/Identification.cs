using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pTop.classes
{
    public class Identification:INotifyPropertyChanged    // 7
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        
        private int db_index = -1;       //binding: database index

        public int Db_index
        {
          get { return db_index; }
          set 
          {
              if (value != db_index)
              {
                  db_index = value;
                  NotifyPropertyChanged("Db_index");
              }
          }
        }

        private DB db=new DB();   //数据库

        [Category("Search")]
        [DisplayName("Database")]  
        public DB Db
        {
            get { return db; }
            set { db = value; }
        }

        public void setDatabase()
        {
            if (db_index >= 0 && db_index < ConfigHelper.dblist.Count)
            {
                db.Db_name = ConfigHelper.dblist[db_index];
                db.Db_path = ConfigHelper.DBmap[db.Db_name].ToString();
            }
            else
            {
                db.Db_name = "null";
                db.Db_path = "null";
            }
        }
        public int setDatabaseIndex()    //当db已存在的情况下
        {                       
            return ConfigHelper.dblist.IndexOf(db.Db_name);                     
        }

        private Tolerance ptl=new Tolerance(3,0);        //binding: 母离子误差
        [Category("Search")]
        [DisplayName("Precursor Tolerance")]
        public Tolerance Ptl
        {
            get { return ptl; }
            set 
            {
                if (value != ptl)
                {
                    ptl = value;
                    NotifyPropertyChanged("Ptl");
                }
            }
        }

        private Tolerance ftl=new Tolerance(20,1);        //binding: 碎片离子误差
        [Category("Search")]
        [DisplayName("Fragment Tolerance")]
        public Tolerance Ftl
        {
            get { return ftl; }
            set 
            {
                if (value != ftl)
                {
                    ftl = value;
                    NotifyPropertyChanged("Ftl");
                }
            }
        }                        
               

        private ObservableCollection<string> fix_mods=new ObservableCollection<string>(); //固定修饰列表
        [Category("Search")]
        [DisplayName("Fix Modifications")]
        public ObservableCollection<string> Fix_mods
        {
            get { return fix_mods; }
            set 
            {
                if (value != fix_mods)
                {
                    fix_mods = value;
                    NotifyPropertyChanged("Fix_mods");
                }
            }
        }

        private ObservableCollection<string> var_mods=new ObservableCollection<string>(); //可变修饰列表
        [Category("Search")]
        [DisplayName("Var Modifications")]
        public ObservableCollection<string> Var_mods
        {
            get { return var_mods; }
            set
            {
                if (value != var_mods)
                {
                    var_mods = value;
                    NotifyPropertyChanged("Var_mods");
                }
            }
        }

        private int max_mod = 3;

        public int Max_mod
        {
            get { return max_mod; }
            set 
            {
                if (value != max_mod)
                {
                    max_mod = value;
                    NotifyPropertyChanged("Max_mod");
                }
            }
        }

        private FilterParam filter = new FilterParam();

        public FilterParam Filter
        {
            get { return filter; }
            set
            {
                if (value != filter)
                {
                    filter = value;
                    NotifyPropertyChanged("Filter");
                }
            }
        }

        
        public Identification() { }
        public Identification(int _db_index, Tolerance _ptl, Tolerance _ftl , int maxmod,  FilterParam _filter) {
                this.db_index = _db_index;
                this.setDatabase();
                this.ptl = _ptl;
                this.ftl = _ftl;
                this.max_mod = maxmod;
                this.filter = _filter;
        }

        public ObservableCollection<_Attribute> getSearchParam()
        {
            ObservableCollection<_Attribute> sch = new ObservableCollection<_Attribute>();
            sch.Add(new _Attribute("Database",this.db.Db_name,""));
            string ptlstr = "±";
            ptlstr += this.ptl.Tl_value.ToString();
            ptlstr += this.ptl.Isppm == 1 ? " ppm" : " Da";
            string ftlstr = "±";
            ftlstr += this.ftl.Tl_value.ToString();
            ftlstr += this.ftl.Isppm == 1 ? " ppm" : " Da";
            string ptl_check = "";
            if (this.ptl.Tl_value < 0) 
            {
                ptl_check = "invalid";
            }
            string ftl_check = "";
            if (this.ftl.Tl_value < 0)
            {
                ftl_check = "invalid";
            }
            sch.Add(new _Attribute("Precursor Tolerance", ptlstr,ptl_check));
            sch.Add(new _Attribute("Fragment Tolerance", ftlstr,ftl_check));
            sch.Add(new _Attribute("Max Modify Position", this.max_mod, ""));
            #region
            
            string fixstr="";
            if (this.fix_mods.Count > 0)
            {
                fixstr += this.fix_mods[0];
            }
            else 
            {
                fixstr = "";
            }
            for (int i = 1; i < this.fix_mods.Count; i++) 
            {
                fixstr += "\n";
                fixstr += this.fix_mods[i];
            }
            sch.Add(new _Attribute("Fixed     Modifications", fixstr,""));
            string varstr = "";
            if (this.var_mods.Count > 0)
            {
                varstr += this.var_mods[0];
            }
            else
            {
                varstr = "";
            }
            for (int i = 1; i < this.var_mods.Count; i++)
            {
                varstr += "\n";
                varstr += this.var_mods[i];
            }
            sch.Add(new _Attribute("Variable Modifications", varstr,""));
            sch.Add(new _Attribute("FDR", this.filter.Fdr_value.ToString()+"%", ""));            
            #endregion
            return sch;
        }

        
        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }
            if(obj.GetType()!=this.GetType()){return false;}
            Identification sp = obj as Identification;
            if ((System.Object)sp == null) 
            { 
                return false; 
            }
            if (this.db_index != sp.db_index) { return false; }
            if (this.db.Db_name!= sp.db.Db_name || this.db.Db_path!=sp.db.Db_path) { return false; }
            if (this.ptl.Tl_value != sp.ptl.Tl_value||this.ptl.Isppm!=sp.ptl.Isppm) { return false; }
            if (this.ftl.Tl_value != sp.ftl.Tl_value||this.ftl.Isppm!=sp.ftl.Isppm) { return false; }
            if (this.fix_mods.Count != sp.fix_mods.Count) { return false; }
            for (int i = 0; i < this.fix_mods.Count; i++)
            {
                if (this.fix_mods[i]!=sp.fix_mods[i]) { return false; }
            }
            if (this.var_mods.Count != sp.var_mods.Count) { return false; }
            for (int i = 0; i < this.var_mods.Count; i++)
            {
                if (this.var_mods[i] != sp.var_mods[i]) { return false; }
            }
            if (this.max_mod != sp.max_mod) { return false; }
            if (!this.filter.Equals(sp.filter)) { return false; }
            return true;
        }

        public override int GetHashCode()
        {
            int hc = this.db_index.GetHashCode() + this.db.GetHashCode()
                + this.ptl.GetHashCode() + this.ftl.GetHashCode() 
                + this.fix_mods.GetHashCode() + this.var_mods.GetHashCode()
                + this.max_mod.GetHashCode() + this.filter.GetHashCode();
            return hc;
        }

        public void CopyTo(Identification sp)
        {
            sp.db_index = this.db_index;
            sp.db.Db_name = string.Copy(this.db.Db_name);
            sp.db.Db_path = string.Copy(this.db.Db_path);
            sp.ptl.Tl_value = this.ptl.Tl_value;
            sp.ptl.Isppm = this.ptl.Isppm;
            sp.ftl.Tl_value = this.ftl.Tl_value;
            sp.ftl.Isppm = this.ftl.Isppm;
            sp.fix_mods.Clear();
            for (int i = 0; i < fix_mods.Count; i++)
            {
                sp.fix_mods.Add(this.fix_mods[i]);
            }
            sp.var_mods.Clear();
            for (int i = 0; i < var_mods.Count; i++)
            {
                sp.var_mods.Add(this.var_mods[i]);
            }
            sp.max_mod = this.max_mod;
            this.filter.CopyTo(sp.filter);
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
                    case "Filter":
                        if (this.Filter.Fdr_value < 0 || this.Filter.Fdr_value > 100)
                        {
                            result = "The value of FDR must be between 0 and 100.";
                        }
                        break;

                }
                return result;
            }
        }
    }

    public class Tolerance :INotifyPropertyChanged,IDataErrorInfo    //2个属性
    {

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        private double tl_value=20;   //binding

        public double Tl_value
        {
            get { return tl_value; }
            set 
            {
                if (value != tl_value)
                {
                    tl_value = value; 
                    NotifyPropertyChanged("Tl_value");
                }
            }
        }

        private int isppm=1;    //binding : ppm， Da

        public int Isppm
        {
            get { return isppm; }
            set 
            {
                if (value != isppm)
                {
                    isppm = value;
                    NotifyPropertyChanged("Isppm");
                }
            }
        }
              

        public Tolerance(){
       
        }
        public Tolerance(double _tl_value, int _tl_unit) {
            this.tl_value = _tl_value;
            this.isppm = _tl_unit;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != this.GetType()) { return false; }
            Tolerance tl = obj as Tolerance;
            if ((System.Object)tl == null)
            {
                return false;
            }
            if (this.tl_value != tl.tl_value || this.isppm != tl.isppm) { return false; }
            return true;
        }

        public override int GetHashCode()
        {
            return this.tl_value.GetHashCode()+this.isppm.GetHashCode();
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
                    case "Tl_value":
                        if (tl_value < 0)
                        {
                            result = "The value of the tolerance cannot be less than 0.";
                        }
                        break;
                }
                return result;
            }
        }
    }

    public class DB {
        private string db_name="null";

        public string Db_name
        {
            get { return db_name; }
            set { db_name = value; }
        }
        private string db_path="null";

        public string Db_path
        {
            get { return db_path; }
            set { db_path = value; }
        }

        public DB() { }
        public DB(string _db_name,string _db_path)
        {
            this.db_name = _db_name;
            this.db_path = _db_path;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != this.GetType()) { return false; }
            DB d = obj as DB;
            if ((System.Object)d == null)
            {
                return false;
            }
            if (this.db_name != d.db_name || this.db_path != d.db_path) { return false; }
            return true;
        }
        public override int GetHashCode()
        {
            return this.db_name.GetHashCode()+this.db_path.GetHashCode();
        }

    }

    public class FilterParam :INotifyPropertyChanged  //1
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        private double fdr_value=1;    //binding

        public double Fdr_value
        {
           get { return fdr_value; }
           set 
           {
               if (value != fdr_value)
               {
                   if (value < 0 || value > 100) { fdr_value = 1; }
                   else
                   {
                       fdr_value = value;
                   }
                   NotifyPropertyChanged("Fdr_value");
               }
           }
        }

  
        public FilterParam() { }
        public FilterParam(double fdr)
        {
            this.fdr_value = fdr;
        }
        
        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }
            if (obj.GetType() != this.GetType()) { return false; }
            FilterParam fl = obj as FilterParam;
            if ((System.Object)fl == null)
            {
                return false;
            }
            if (this.fdr_value != fl.fdr_value) { return false; }
            return true;
        }
        public override int GetHashCode()
        {
            int hc = this.fdr_value.GetHashCode();
            return hc;
        }

        public void CopyTo(FilterParam fl)
        {
            fl.fdr_value = this.fdr_value;
                          
        }
        
    }
}
