using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pFind.classes;

namespace pFind
{
    public class SearchParam:INotifyPropertyChanged                  //11+1个属性
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

        private int enzyme_index = 0;        //binding: enzyme index

        public int Enzyme_index
        {
            get { return enzyme_index; }
            set 
            {
                if (value != enzyme_index)
                {
                    enzyme_index = value;
                    NotifyPropertyChanged("Enzyme_index");
                }
            }
        }

        private int enzyme_spec_index = 0;        //binding: enzyme spec index

        public int Enzyme_Spec_index
        {
            get { return enzyme_spec_index; }
            set
            {
                if (value != enzyme_spec_index)
                {
                    enzyme_spec_index = value;
                    NotifyPropertyChanged("Enzyme_Spec_index");
                }
            }
        }

        private string enzyme_spec = "Full-Specific";      //特异性
        [Category("Search")]
        public string Enzyme_Spec
        {
            get { return enzyme_spec; }
            set { enzyme_spec = value; }
        }

        public void setEnzymeSpec()
        {
            enzyme_spec = ConfigHelper.enzymespeclist[enzyme_spec_index];
        }

        public int setEnzymeSpecIndex()  //已知enzyme时
        {
            return ConfigHelper.enzymespeclist.IndexOf(enzyme_spec);
        }

        private string enzyme="";      //酶
        [Category("Search")]
        public string Enzyme
        {
            get { return enzyme; }
            set { enzyme = value; }
        }

        public void setEnzyme()
        { 
           enzyme=ConfigHelper.enzymelist[enzyme_index];
        }

        public int setEnzymeIndex()  //已知enzyme时
        {
            if (ConfigHelper.enzymelist.Contains(enzyme))
            {
                return ConfigHelper.enzymelist.IndexOf(enzyme);
            }
            else return 0;
        }

        private int cleavages=3;      //binding: 遗漏酶切位点个数
        [Category("Search")]
        [DisplayName("Number of Cleavages")]
        public int Cleavages
        {
            get { return cleavages; }
            set 
            {
                if (value != cleavages)
                {
                    cleavages = value;
                    NotifyPropertyChanged("Cleavages");
                }
            }
        }

        private Tolerance ptl=new Tolerance();        //binding: 母离子误差
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

        private Tolerance ftl=new Tolerance();        //binding: 碎片离子误差
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
               
        private bool open_search=true;  //binding: 是否开放搜索
        [Category("Search")]
        public bool Open_search
        {
            get { return open_search; }
            set 
            {
                if (value != open_search)
                {
                    open_search = value;
                    NotifyPropertyChanged("Open_search");
                }
            }
        }

        private ObservableCollection<string> fix_mods=new ObservableCollection<string>(); //固定修饰列表
        [Category("Search")]
        [DisplayName("Fix Modifications")]
        public ObservableCollection<string> Fix_mods
        {
            get { return fix_mods; }
            set { fix_mods = value; }
        }

        private ObservableCollection<string> var_mods=new ObservableCollection<string>(); //可变修饰列表
        [Category("Search")]
        [DisplayName("Var Modifications")]
        public ObservableCollection<string> Var_mods
        {
            get { return var_mods; }
            set { var_mods = value; }
        }

        private Search_Advanced search_advanced = new Search_Advanced();

        public Search_Advanced Search_advanced
        {
            get { return search_advanced; }
            set
            {
                if (value != search_advanced)
                {
                    search_advanced = value;
                    NotifyPropertyChanged("Search_advanced");
                }
            }
        }

        
        public SearchParam() { }
        public SearchParam(int _db_index,int _enzyme_index,int _enzyme_spec_index, int _cleavages,
            Tolerance _ptl,Tolerance _ftl,bool _open_search) {
                this.db_index = _db_index;
                this.setDatabase();
                this.enzyme_index = _enzyme_index;
                this.enzyme_spec_index = _enzyme_spec_index;
                this.setEnzyme();
                this.cleavages = _cleavages;
                this.ptl = _ptl;
                this.ftl = _ftl;
                this.open_search = _open_search;
        }
        //reset
        public void Reset()
        {
            this.db_index = -1;
            this.setDatabase();
            this.enzyme_index = 0;
            this.enzyme_spec_index = 0;
            this.setEnzyme();
            this.cleavages = 3;      //default
            this.ptl.Tl_value=20;
            this.ptl.Isppm = 1;
            this.ftl.Tl_value=20;
            this.ftl.Isppm = 1;
            this.open_search = true;
            this.fix_mods.Clear();
            this.var_mods.Clear();
            this.search_advanced = new Search_Advanced();

        }

        public ObservableCollection<_Attribute> getSearchParam()
        {
            ObservableCollection<_Attribute> sch = new ObservableCollection<_Attribute>();
            sch.Add(new _Attribute("Database",this.db.Db_name,""));
            sch.Add(new _Attribute("Enzyme",this.enzyme,""));
            sch.Add(new _Attribute("Enzyme Specificity", this.enzyme_spec,""));
            sch.Add(new _Attribute("Number of Missed Cleavages",this.cleavages,""));
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
            sch.Add(new _Attribute("Open Search", this.open_search,""));
            #region
            //if (!this.open_search) // 开放和限定都需要显示修饰信息
            {
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
            }
            #endregion
            return sch;
        }

        public Hashtable getSearchMap()
        {
            Hashtable sch = new Hashtable();
            sch.Add("DataBase", this.db);
            sch.Add("Enzyme", this.enzyme);
            sch.Add("Enzyme Specificity", this.enzyme);
            sch.Add("Number of Cleavages", this.cleavages);
            sch.Add("Precursor Tolerance", this.ptl);
            sch.Add("Fragment Tolerance", this.ftl);
            sch.Add("Open Search", this.open_search);
            sch.Add("Fixed    Modifications", this.fix_mods);
            sch.Add("Variable Modifications", this.var_mods);
            return sch;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }
            if(obj.GetType()!=this.GetType()){return false;}
            SearchParam sp = obj as SearchParam;
            if ((System.Object)sp == null) 
            { 
                return false; 
            }
            if (this.db_index != sp.db_index) { return false; }
            if (this.db.Db_name!= sp.db.Db_name || this.db.Db_path!=sp.db.Db_path) { return false; }
            if (this.enzyme_index != sp.enzyme_index) { return false; }
            if (this.enzyme_spec_index != sp.enzyme_spec_index) { return false; }
            if (this.enzyme != sp.enzyme) { return false; }
            if (this.enzyme_spec != sp.enzyme_spec) { return false; }
            if (this.cleavages != sp.cleavages) { return false; }
            if (this.ptl.Tl_value != sp.ptl.Tl_value||this.ptl.Isppm!=sp.ptl.Isppm) { return false; }
            if (this.ftl.Tl_value != sp.ftl.Tl_value||this.ftl.Isppm!=sp.ftl.Isppm) { return false; }
            if (this.open_search != sp.open_search) { return false; }
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
            if (!this.search_advanced.Equals(sp.search_advanced)) { return false; }
            return true;
        }

        public override int GetHashCode()
        {
            int hc = this.db_index.GetHashCode() + this.db.GetHashCode()
                + this.enzyme_index.GetHashCode() + this.enzyme_spec_index.GetHashCode() + this.enzyme_spec.GetHashCode() + this.enzyme.GetHashCode() + this.cleavages.GetHashCode()
                + this.ptl.GetHashCode() + this.ftl.GetHashCode() + this.open_search.GetHashCode()
                + this.fix_mods.GetHashCode() + this.var_mods.GetHashCode()+this.search_advanced.GetHashCode();
            return hc;
        }

        public void CopyTo(SearchParam sp)
        {
            sp.db_index = this.db_index;
            sp.db.Db_name = string.Copy(this.db.Db_name);
            sp.db.Db_path = string.Copy(this.db.Db_path);
            sp.enzyme_index = this.enzyme_index;
            sp.enzyme_spec_index = this.enzyme_spec_index;
            sp.enzyme = string.Copy(this.enzyme);
            sp.enzyme_spec = string.Copy(this.enzyme_spec);
            sp.cleavages = this.cleavages;
            sp.ptl.Tl_value = this.ptl.Tl_value;
            sp.ptl.Isppm = this.ptl.Isppm;
            sp.ftl.Tl_value = this.ftl.Tl_value;
            sp.ftl.Isppm = this.ftl.Isppm;
            sp.open_search = this.open_search;
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
            this.search_advanced.CopyTo(sp.search_advanced);
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

        private int isppm=1;    //binding

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

    public class Search_Advanced:INotifyPropertyChanged  //12
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        private int temppepnum = 100;  //中间结果数目，Hidden parameters

        public int Temppepnum
        {
            get { return temppepnum; }
            set { temppepnum = value; }
        }

        private int pepnum = 10;    //number of output_peptides

        public int Pepnum
        {
            get { return pepnum; }
            set { pepnum = value; }
        }

        private int selectpeak = 200;  //预处理保存谱峰数目,Hidden parameters

        public int Selectpeak
        {
            get { return selectpeak; }
            set { selectpeak = value; }
        }

        private int maxprolen = 60000000;

        public int Maxprolen
        {
            get { return maxprolen; }
            set { maxprolen = value; }
        }

        private int maxspec = 100000;

        public int Maxspec
        {
            get { return maxspec; }
            set { maxspec = value; }
        }

        private bool ieql = true;
        public bool IeqL
        {
            get { return ieql; }
            set { ieql = value; }
        }

        private int npep = 2; // template模板参数中默认是2，只要是非零值效果都是一样的
        public int NPeP
        {
            get { return npep; }
            set { npep = value; }
        }

        private int maxdelta = 500;
        public int MAXDelta
        {
            get { return maxdelta; }
            set { maxdelta = value; }
        }

        private int maxmod = 3;

        public int Maxmod
        {
            get { return maxmod; }
            set { maxmod = value; }
        }

        
        private int open_tag_len = 5;

        public int Open_tag_len
        {
            get { return open_tag_len; }
            set { open_tag_len = value; }
        }

        private int rest_tag_iteration = 1;

        public int Rest_tag_iteration
        {
            get { return rest_tag_iteration; }
            set { rest_tag_iteration = value; }
        }

        private int rest_tag_len = 4;

        public int Rest_tag_len
        {
            get { return rest_tag_len; }
            set { rest_tag_len = value; }
        }

        private int rest_mod_num = 10;

        public int Rest_mod_num
        {
            get { return rest_mod_num; }
            set { rest_mod_num = value; }
        }

        private int salvo_iteration = 1;

        public int Salvo_iteration
        {
            get { return salvo_iteration; }
            set { salvo_iteration = value; }
        }

        private int salvo_mod_num = 5;

        public int Salvo_mod_num
        {
            get { return salvo_mod_num; }
            set { salvo_mod_num = value; }
        }

        public Search_Advanced() { }
        public Search_Advanced(int _temppepnum,int _pepnum,int _selectpeak,int _maxprolen,int _maxspec, bool _ieql, int _npep, int _maxdelta, int _maxmod,
            int _open_tag_len,int _rest_tag_iteration,int _rest_tag_len,int _rest_mod_num,int _salvo_iteration,int _salvo_mod_num)
        {
            this.temppepnum = _temppepnum;
            this.pepnum = _pepnum;
            this.selectpeak = _selectpeak;
            this.maxprolen = _maxprolen;
            this.maxspec = _maxspec;
            this.ieql = _ieql;
            this.npep = _npep;
            this.maxdelta = _maxdelta;
            this.maxmod = _maxmod;
            this.open_tag_len = _open_tag_len;
            this.rest_tag_iteration = _rest_tag_iteration;
            this.rest_tag_len = _rest_tag_len;
            this.rest_mod_num = _rest_mod_num;
            this.salvo_iteration = _salvo_iteration;
            this.salvo_mod_num = _salvo_mod_num;
        }
        //添加模板任务之后，reset似乎已经不需要了
        //public void reset()
        //{ 
        
        //}
        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }
            if (obj.GetType() != this.GetType()) { return false; }
            Search_Advanced sa = obj as Search_Advanced;
            if ((System.Object)sa == null)
            {
                return false;
            }
            if(this.temppepnum!=sa.temppepnum){return false;}
            if (this.pepnum != sa.pepnum) { return false; }
            if (this.selectpeak != sa.selectpeak) { return false; }
            if (this.maxprolen != sa.maxprolen) { return false; }
            if (this.maxspec != sa.maxspec) { return false; }
            if (this.ieql != sa.ieql) { return false; }
            if (this.npep != sa.npep) { return false; }
            if (this.maxdelta != sa.maxdelta) { return false; }
            if (this.maxmod != sa.maxmod) { return false; }
            if (this.open_tag_len != sa.open_tag_len) { return false; }
            if (this.rest_tag_iteration != sa.rest_tag_iteration) { return false; }
            if (this.rest_tag_len != sa.rest_tag_len) { return false; }
            if (this.rest_mod_num != sa.rest_mod_num) { return false; }
            if (this.salvo_iteration != sa.salvo_iteration) { return false; }
            if (this.salvo_mod_num != sa.salvo_mod_num) { return false; }
            return true;
        }
        public override int GetHashCode()
        {
            int hc = this.temppepnum.GetHashCode() + this.pepnum.GetHashCode()
                + this.selectpeak.GetHashCode() + this.maxprolen.GetHashCode() + this.maxspec.GetHashCode()+this.maxmod.GetHashCode()
                + this.ieql.GetHashCode() + this.npep.GetHashCode() + this.maxdelta.GetHashCode()
                + this.open_tag_len.GetHashCode() + this.rest_tag_iteration.GetHashCode() + this.rest_tag_len.GetHashCode()
                + this.rest_mod_num.GetHashCode() + this.salvo_iteration.GetHashCode()+this.salvo_mod_num;
            return hc;
        }

        public void CopyTo(Search_Advanced sa)
        {
            sa.temppepnum = this.temppepnum;
            sa.pepnum = this.pepnum;
            sa.selectpeak = this.selectpeak;
            sa.maxprolen = this.maxprolen;
            sa.maxspec = this.maxspec;
            sa.ieql = this.ieql;
            sa.npep = this.npep;
            sa.maxdelta = this.maxdelta;
            sa.maxmod = this.maxmod;
            sa.open_tag_len = this.open_tag_len;
            sa.rest_tag_iteration = this.rest_tag_iteration;
            sa.rest_tag_len = this.rest_tag_len;
            sa.rest_mod_num = this.rest_mod_num;
            sa.salvo_mod_num = this.salvo_mod_num;
            sa.salvo_iteration = this.salvo_iteration;                     
        }




    }

}
