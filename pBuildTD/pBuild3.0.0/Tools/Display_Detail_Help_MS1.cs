using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class Display_Detail_Help_MS1
    {
        public MarkerType mgf_marker; //MGF给出的m/z
        public MarkerType theory_marker; //鉴定到的肽段的m/z
        public MarkerType other_marker; //超哥使用EMASS计算出来的该肽段的理论同位素峰簇（包括轻标及重标的理论同位素峰簇)
        public OxyColor mgf_color;
        public OxyColor theory_color;
        public OxyColor other_color;
        public double mgf_size;
        public double theory_size;
        public double other_size;

        public double peak_size;

        public static int Start_Scan = 30;
        public static int End_Scan = 30;

        public Display_Detail_Help_MS1()
        {
            this.mgf_marker = MarkerType.Plus;
            this.theory_marker = MarkerType.Circle;
            this.other_marker = MarkerType.Cross;
            this.mgf_color = OxyColors.Red;
            this.theory_color = OxyColors.Red;
            this.other_color = OxyColors.Red;
            this.mgf_size = 4;
            this.theory_size = 4;
            this.other_size = 4;
            this.peak_size = 2;
        }
        
    }
}
