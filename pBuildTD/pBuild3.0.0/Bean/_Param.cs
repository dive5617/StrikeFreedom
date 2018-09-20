using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class _Param
    {
        public string _property { get; set; }
        public string _value { get; set; }

        public _Param(string _property, string _value)
        {
            this._property = _property;
            this._value = _value;
        }
    }
}
