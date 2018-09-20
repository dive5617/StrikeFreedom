using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pFind.classes;

namespace pFind.Interface
{
    public interface Reset_Inter
    {
        //reset file
        void ResetFile(File fi);
        //reset search
        void ResetSearch(SearchParam sp);
        //reset filter
        void ResetFilter(FilterParam fp);
        //reset quantitation
        void ResetQuantatition(QuantitationParam qp);
    }
}
