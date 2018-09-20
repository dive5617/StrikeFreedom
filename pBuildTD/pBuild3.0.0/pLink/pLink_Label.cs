using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace pBuild.pLink
{
    public class pLink_Label
    {
        public List<string> Element_names; //标记中替换的元素
        public List<double> Masses; //标记中替换的元素的质量差，比如使用15N来替换N，Element_names中会加入"N"，并且Masses中会加入N15-N的质量差
        //上面两个变量不用
        public List<double> Linker_Masses;
        public int Flag; //表示是肽段标记还是交联剂标记，为0表示交联剂标记，为1表示两条肽段的标记，（后面可以：2表示肽段1标记，3表示肽段2标记等）

        public pLink_Label()
        {
            Element_names = new List<string>();
            Masses = new List<double>();
            Linker_Masses = new List<double>();
            Flag = 0;
        }
    }
}
