using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class BoxPlot_Help
    {
        public double Q1 { get; set; }
        public double Median { get; set; }
        public double Q3 { get; set; }
        public double MIN { get; set; }
        public double MAX { get; set; }

        public List<double> Outliers { get; set; }

        public BoxPlot_Help(double MIN, double Q1, double Median, double Q3, double MAX, List<double> Outliers)
        {
            this.MIN = MIN;
            this.Q1 = Q1;
            this.Median = Median;
            this.Q3 = Q3;
            this.MAX = MAX;
            this.Outliers = Outliers;
        }

        public static BoxPlot_Help parse_boxplot(List<double> values)
        {
            values.Sort();
            double q1 = values[(int)(values.Count * 0.25)];
            double q3 = values[(int)(values.Count * 0.75)];
            double med = values[(int)(values.Count * 0.5)];
            double delta = 1.5 * (q3 - q1);
            double max = (values.Last() > q3 + delta ? q3 + delta : values.Last());
            double min = (values.First() < q1 - delta ? q1 - delta : values.First());
            List<double> outlies = new List<double>();
            for (int i = 0; i < values.Count; ++i)
            {
                if (values[i] < min)
                    outlies.Add(values[i]);
                else
                    break;
            }
            for (int i = values.Count - 1; i >= 0; --i)
            {
                if (values[i] > max)
                    outlies.Add(values[i]);
                else
                    break;
            }
            BoxPlot_Help boxplot = new BoxPlot_Help(min, q1, med, q3, max, outlies);
            return boxplot;
        }
    }
}
