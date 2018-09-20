using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class MS2_Help_Denovol
    {
        public List<Series> all_series = new List<Series>();
        public List<int> all_series_index = new List<int>(); //所有可以显示出来的series
        public List<int> all_series_index2 = new List<int>(); //所有氨基酸的series
        public List<Annotation> all_annotation = new List<Annotation>();
        public List<Annotation> arrow_annotation = new List<Annotation>(); //虚线表示的所有氨基酸的质量，然后根据这些里面将那个点击的保存下来
        public List<Annotation> arrow_annotation_last = new List<Annotation>(); //上一次的所有箭头
        public List<double> arrow_mass = new List<double>(); //当前虚线箭头所指向的末端的质量
        public double arrow_mass0 = 0.0;
        public System.Collections.Hashtable arrowMass_to_index = new System.Collections.Hashtable();

        public MS2_Help_Denovol()
        { }

        public MS2_Help_Denovol(MS2_Help_Denovol ms2_help_denvol)
        {
            this.all_series = new List<Series>(ms2_help_denvol.all_series);
            this.all_series_index = new List<int>(ms2_help_denvol.all_series_index);
            this.all_series_index2 = new List<int>(ms2_help_denvol.all_series_index2);
            this.all_annotation = new List<Annotation>(ms2_help_denvol.all_annotation);
            this.arrow_annotation = new List<Annotation>(ms2_help_denvol.arrow_annotation);
            this.arrow_annotation_last = new List<Annotation>(ms2_help_denvol.arrow_annotation_last);
            this.arrow_mass = new List<double>(ms2_help_denvol.arrow_mass);
            this.arrow_mass0 = ms2_help_denvol.arrow_mass0;
            this.arrowMass_to_index = new System.Collections.Hashtable(ms2_help_denvol.arrowMass_to_index);
        }

        public MS2_Help_Denovol(List<Series> all_series, List<int> all_series_index, List<int> all_series_index2, List<Annotation> all_annotation,
            List<Annotation> arrow_annotation, List<Annotation> arrow_annotation_last, List<double> arrow_mass, double arrow_mass0, System.Collections.Hashtable arrowMass_to_index)
        {
            this.all_series = new List<Series>(all_series);
            this.all_series_index = new List<int>(all_series_index);
            this.all_series_index2 = new List<int>(all_series_index2);
            this.all_annotation = new List<Annotation>(all_annotation);
            this.arrow_annotation = new List<Annotation>(arrow_annotation);
            this.arrow_annotation_last = new List<Annotation>(arrow_annotation_last);
            this.arrow_mass = new List<double>(arrow_mass);
            this.arrow_mass0 = arrow_mass0;
            this.arrowMass_to_index = new System.Collections.Hashtable(arrowMass_to_index);
        }
    }

    public class Denovol_Config
    {
        public static List<DCC> All_mass = new List<DCC>();

        static Denovol_Config()
        {
            initial();
        }

        public static void initial()
        {
            All_mass.Clear();
            for (int i = 0; i < 26; ++i)
            {
                char tmp = (char)('A' + i);
                if (tmp == 'B' || tmp == 'J' || tmp == 'O' || tmp == 'U' || tmp == 'X' || tmp == 'Z' || tmp == 'L')
                    continue;
                int index = Config_Help.AA_Normal_Index;
                All_mass.Add(new DCC(Config_Help.mass_index[index, i], tmp + ""));
            }
        }

        public class DCC
        {
            public double Mass;
            public string Name;

            public bool CanUse;

            public DCC(double mass, string name)
            {
                this.Mass = mass;
                this.Name = name;
                this.CanUse = true;
            }
        }
    }
}
