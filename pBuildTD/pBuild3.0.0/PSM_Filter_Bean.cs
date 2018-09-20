using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class PSM_Filter_Bean
    {
        public int Mix_number { get; set; }
        public string Title { get; set; }
        public int Title_Regular_Flag { get; set; }
        public string SQ { get; set; }
        public int SQ_Regular_Flag { get; set; }
        public string Modification { get; set; }
        public string Modification_label_name { get; set; }
        public string Specific { get; set; }
        public string Label_name { get; set; }
        public string Target { get; set; }
        public bool isContainment { get; set; }
        public bool isUnique { get; set; }
        public double Ratio1 { get; set; }
        public double Ratio2 { get; set; }
        public double Sigma1 { get; set; }
        public double Sigma2 { get; set; }

        public PSM_Filter_Bean(int mix_number, string title, int title_regular_flag, string sq, int sq_regular_flag, string modification, string modification_lable_name, string specific,
            string label_name, string target, bool isContainment, bool isUnique, double ratio1, double ratio2, double sigma, double sigma2)
        {
            this.Mix_number = mix_number;
            this.Title = title;
            this.Title_Regular_Flag = title_regular_flag;
            this.SQ = sq;
            this.SQ_Regular_Flag = sq_regular_flag;
            this.Modification = modification;
            this.Modification_label_name = modification_lable_name;
            this.Specific = specific;
            this.Label_name = label_name;
            this.Target = target;
            this.isContainment = isContainment;
            this.isUnique = isUnique;
            this.Ratio1 = ratio1;
            this.Ratio2 = ratio2;
            this.Sigma1 = sigma;
            this.Sigma2 = sigma2;
        }
    }
}
