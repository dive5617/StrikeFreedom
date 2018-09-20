using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    //绘制一级谱的色谱曲线
    //包含了理论谱图的色谱曲线及起始及终止的扫描号,index表示当前绘制的是轻重的第几根同位素峰簇信息,all_index表示总共多少根
    public class Chrome_Help
    {
        public List<Theory_IOSTOPE> theory_isotopes = new List<Theory_IOSTOPE>();
        public int start_scan, end_scan;
        public int index, all_index;
    }
}
