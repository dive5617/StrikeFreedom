using OxyPlot.Annotations;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class MS2_Help_Denovol_pNovol
    {
        public List<ArrowAnnotation> ArrowAnnotation = new List<ArrowAnnotation>();
        public List<TextAnnotation> TxtAnnotation = new List<TextAnnotation>();
        public List<LineSeries> LineSeries = new List<LineSeries>();

        public List<ScatterSeries> ScatterSeries = new List<ScatterSeries>();

        public MS2_Help_Denovol_pNovol()
        {
            this.ArrowAnnotation = new List<ArrowAnnotation>();
            this.TxtAnnotation = new List<TextAnnotation>();
            this.LineSeries = new List<LineSeries>();
            this.ScatterSeries = new List<ScatterSeries>();
        }
    }
}
