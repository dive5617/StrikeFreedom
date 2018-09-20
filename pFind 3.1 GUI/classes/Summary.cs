using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pFind
{
    public class Summary
    {
        private File file=new File();
        public  File File
        {
            get { return file; }
            set { file = value; }
        }
        private SearchParam search=new SearchParam();

        public SearchParam Search
        {
            get { return search; }
            set { search = value; }
        }
        private FilterParam filter=new FilterParam();

        public FilterParam Filter
        {
            get { return filter; }
            set { filter = value; }
        }
        private QuantitationParam quantitation=new QuantitationParam();

        public QuantitationParam Quantitation
        {
            get { return quantitation; }
            set { quantitation = value; }
        }

        public Summary() { }
        public Summary(File _file,SearchParam _search,FilterParam _filter,QuantitationParam _quantitation) {
            this.file = _file;
            this.search = _search;
            this.filter = _filter;
            this.quantitation = _quantitation;
        }
    }
}
