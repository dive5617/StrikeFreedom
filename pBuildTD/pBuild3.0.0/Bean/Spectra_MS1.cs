using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class Spectra_MS1
    {
        public ObservableCollection<PEAK> Peaks { get; set; }
        public ObservableCollection<int> Peaks_index { get; set; } //一级谱图中的鉴定到的峰为一张二级谱，peaks_index表示该峰对应的PSM的索引号
        public int Scan { get; set; }
        public int Scan_relative { get; set; }
        //MS1上显示的峰
        public double Pepmass { get; set; }
        //MS2上导出的峰
        public double Pepmass_ms2 { get; set; }
        //鉴定到的肽段的理论Mz
        public double Pepmass_theory { get; set; }
        public int Charge { get; set; }
        public double Max_inten_E { get; set; }
        public int Fragment_index { get; set; }
        public double Retention_Time { get; set; }
        public Spectra_MS1(ObservableCollection<PEAK> peaks, int scan, int scan_relative, double pepmass_ms2, double pepmass_theory, int charge, double max_inten_E)
        {
            this.Peaks = peaks;
            this.Scan = scan;
            this.Scan_relative = scan_relative;
            this.Pepmass_ms2 = pepmass_ms2;
            this.Pepmass_theory = pepmass_theory;
            this.Charge = charge;
            this.Max_inten_E = max_inten_E;
        }
        //将一级谱图中的所有谱峰删除，只保留mzs附近的谱峰，该函数用来保留一级谱图的碎裂谱峰
        public void update_peaks(List<double> mzs, List<int> peaks_index)
        {
            const double mz_error = 20e-6;
            int[] mass_inten = new int[Config_Help.MaxMass];
            for (int k = 0; k < this.Peaks.Count; ++k)
            {
                int massi = (int)this.Peaks[k].Mass;
                mass_inten[massi] = k + 1;
            }
            int currindex = 0;
            for (int k = 0; k < Config_Help.MaxMass; ++k)
            {
                if (mass_inten[k] == 0)
                    mass_inten[k] = currindex;
                else
                    currindex = mass_inten[k];
            }
            ObservableCollection<PEAK> fragment_peaks = new ObservableCollection<PEAK>();
            for (int i = 0; i < mzs.Count; ++i)
            {
                double me = 0.0;
                int index = IsInWithPPM(mzs[i], mass_inten, mz_error, ref me);
                if (index != -1)
                {
                    fragment_peaks.Add(Peaks[index]);
                }
            }
            this.Peaks = fragment_peaks;
            if (peaks_index != null)
                this.Peaks_index = new ObservableCollection<int>(peaks_index);
        }
        private int IsInWithPPM(double mass, int[] mass_inten, double Ppm_mass_error, ref double mass_error)
        {
            int start = (int)(mass - mass * Ppm_mass_error);
            int end = (int)(mass + mass * Ppm_mass_error);
            if (start <= 0 || end >= Config_Help.MaxMass)
                return -1;
            double maxInten = 0.0;
            int max_k = -1;
            for (int k = mass_inten[start - 1]; k < mass_inten[end]; ++k)
            {
                PEAK peak = (PEAK)(this.Peaks[k]);
                double tmpd = System.Math.Abs((peak.Mass - mass) / mass);
                if (tmpd <= Ppm_mass_error && peak.Intensity > maxInten)
                {
                    maxInten = peak.Intensity;
                    max_k = k;
                }
            }
            if (max_k != -1)
            {
                mass_error = (((PEAK)(this.Peaks[max_k])).Mass - mass) * 1.0e6 / mass;
            }
            return max_k;
        }
        
    }
}
