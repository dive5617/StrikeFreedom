using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class Spectra
    {
        public string Title { get; set; }
        public int Charge { get; set; }
        public double Pepmass { get; set; }
        public double Max_inten_E { get; set; }
        public ObservableCollection<PEAK> Peaks { get; set; }

        public Spectra()
        {
            Peaks = new ObservableCollection<PEAK>();
        }
        public Spectra(string title, int charge, double pepmass)
        {
            this.Title = title;
            this.Charge = charge;
            this.Pepmass = pepmass;
            this.Peaks = new ObservableCollection<PEAK>();
        }
       
        public Spectra(string title, int charge, double pepmass, ObservableCollection<PEAK> peaks, double max_inten_E)
        {
            this.Title = title;
            this.Charge = charge;
            this.Pepmass = pepmass;
            this.Peaks = peaks;
            this.Max_inten_E = max_inten_E;
        }
        public int getScan()
        {
            string[] strs = Title.Split('.');
            return int.Parse(strs[1]);
        }
        public string getRawName()
        {
            string[] strs = Title.Split('.');
            return strs[0];
        }
        public void write_MGF(string file_path)
        {
            StreamWriter sw = new StreamWriter(file_path);
            sw.WriteLine("BEGIN IONS");
            sw.WriteLine("TITLE=" + this.Title);
            sw.WriteLine("CHARGE=" + this.Charge + "+");
            sw.WriteLine("PEPMASS=" + this.Pepmass.ToString("F6"));
            for (int i = 0; i < this.Peaks.Count; ++i)
                sw.WriteLine(this.Peaks[i].Mass.ToString("F6") + " " + ((this.Peaks[i].Intensity * this.Max_inten_E) / 100).ToString("F5"));
            sw.WriteLine("END IONS");
            sw.Flush();
            sw.Close();
        }
        public double preprocess(double ppm_me, double da_me, bool isNeed_translate = true)
        {
            delete_isotope(ppm_me, da_me, isNeed_translate);
            if (isNeed_translate)
            {
                merge(ppm_me, da_me);
                delectProcorse(ppm_me, da_me);
                List<PEAK> peaks = new List<PEAK>(this.Peaks);
                peaks.Sort();
                this.Peaks = new ObservableCollection<PEAK>(peaks);
            }
            return this.Peaks.Last().Mass;
        }
        private void delete_isotope(double ppm_me, double da_me, bool isNeed_translate = true) //去同位素峰并转化成单电荷
        {
            Spectra out_spectra = new Spectra();
            List<int> PeaksTag = new List<int>();
            for (int j = 0; j < this.Peaks.Count; ++j)
                PeaksTag.Add(0);
            for (int j = 0; j < this.Peaks.Count; ++j)
            {
                if (PeaksTag[j] > 0)
                    continue;
                for (int c = this.Charge; c >= 1; --c)
                {
                    if (deisotpeChargeHCD(j, out_spectra, c, PeaksTag, ppm_me, da_me, isNeed_translate))
                        break;
                }
                if (PeaksTag[j] == 0)
                {
                    if (isNeed_translate)
                    {
                        translateCharge(j, out_spectra, 1);
                        translateCharge(j, out_spectra, 2);
                    }
                    else
                        translateCharge(j, out_spectra, 1);
                }
            }
            List<PEAK> peaks = new List<PEAK>(out_spectra.Peaks);
            peaks.Sort();
            this.Peaks = new ObservableCollection<PEAK>(peaks);
        }
        private void merge(double ppm_me, double da_me)
        {
            List<PEAK> peaks = new List<PEAK>();
            int nPeaks = 0;
            for (int i = 0; i < this.Peaks.Count;)
            {
                peaks.Add(new PEAK(this.Peaks[i].Mass, this.Peaks[i].Intensity));
                nPeaks = 1;
                int tPos = i + 1;
                while (tPos < this.Peaks.Count && canMerge(i, tPos, ppm_me, da_me))
                {
                    peaks.Last().Mass += this.Peaks[tPos].Mass;
                    peaks.Last().Intensity += this.Peaks[tPos].Intensity;
                    nPeaks++;
                    ++tPos;
                    ++i;
                }
                peaks.Last().Mass /= nPeaks;
                ++i;
            }
            this.Peaks = new ObservableCollection<PEAK>(peaks);
        }
        private void delectProcorse(double ppm_me, double da_me)
        {
            double pepmass = this.Pepmass * this.Charge - (this.Charge - 1) * Config_Help.massZI;
            List<PEAK> peaks = new List<PEAK>();
            for (int t = 0; t < this.Peaks.Count(); ++t)
            {
                double d1 = calc(ppm_me, da_me, pepmass);
                double d2 = calc(ppm_me, da_me, this.Peaks[t].Mass);
                if (Math.Abs(pepmass - this.Peaks[t].Mass) <= (d1 + d2) / 2)
                    continue;
                double mass = this.Peaks[t].Mass + Config_Help.massH2O;
                d2 = calc(ppm_me, da_me, mass);
                if (Math.Abs(pepmass - mass) <= (d1 + d2) / 2)
                    continue;
                peaks.Add(new PEAK(this.Peaks[t].Mass, this.Peaks[t].Intensity));
            }
            this.Peaks = new ObservableCollection<PEAK>(peaks);
        }
        private bool deisotpeChargeHCD(int j, Spectra out_spectra, int charge, List<int> PeaksTag, double ppm_me, double da_me, bool isNeed_translate = true)
        {
            double deviation = Config_Help.mass_ISO / charge;
            double lfCurrentVal = this.Peaks[j].Mass + deviation;
            double d1 = calc(ppm_me, da_me, this.Peaks[j].Mass);
            SortedSet<int> PeakR = new SortedSet<int>();
            bool tag = false;
            for (int k = j + 1; k < this.Peaks.Count; ++k)
            {
                double d2 = calc(ppm_me, da_me, this.Peaks[k].Mass);
                double d = (d1 + d2) / 2;
                if (this.Peaks[k].Mass >= lfCurrentVal - d && this.Peaks[k].Mass <= lfCurrentVal + d)
                {
                    lfCurrentVal = this.Peaks[k].Mass + deviation;
                    d1 = d2;
                    PeaksTag[k] = charge;
                    PeaksTag[j] = charge;
                    PeakR.Add(k);
                    PeakR.Add(j);
                    tag = true;
                }
                else if (this.Peaks[k].Mass > lfCurrentVal + d)
                    break;
            }
            
            if (tag)
            {
                bool flag = false;
                foreach (int i in PeakR)
                {
                    if (flag && this.Peaks[i].Intensity < this.Peaks[PeakR.First()].Intensity)
                        continue;
                    if (isNeed_translate)
                        translateCharge(i, out_spectra, charge);
                    else
                        translateCharge(i, out_spectra, 1);
                    flag = true;
                }
                return true;
            }
            return false;
        }
        private void translateCharge(int j, Spectra out_spectra, int charge)
        {
            double mass = this.Peaks[j].Mass;
            mass *= charge;
            mass -= ((charge - 1) * Config_Help.massZI);
            PEAK peak = new PEAK(mass, this.Peaks[j].Intensity);
            out_spectra.Peaks.Add(peak);
        }
        private bool canMerge(int i, int j, double ppm_me, double da_me)
        {
            double d1 = calc(ppm_me, da_me, this.Peaks[i].Mass);
            double d2 = calc(ppm_me, da_me, this.Peaks[j].Mass);
            return Math.Abs(this.Peaks[i].Mass - this.Peaks[j].Mass) <= (d1 + d2) / 2;
        }
        private double calc(double ppm_me, double da_me, double mass)
        {
            double d = 0.0;
            if (ppm_me != 0)
                d = ppm_me * mass;
            else
                d = da_me;
            return d;
        }
    }
    public class PEAK : IComparable
    {
        public double Mass { get; set; }
        public double Intensity { get; set; }
        public int Charge { get; set; }
        public bool Is_mono { get; set; }

        public PEAK(double mass, double intensity, int charge = 0, bool is_mono = false)
        {
            this.Mass = mass;
            this.Intensity = intensity;
            this.Charge = charge;
            this.Is_mono = is_mono;
        }
        //误差在ppm_mass_error范围内的最高峰的索引号
        public static int IsInWithPPM(double mass, int[] mass_inten, double ppm_mass_error, ObservableCollection<PEAK> peaks)
        {
            int start = (int)(mass - mass * ppm_mass_error);
            int end = (int)(mass + mass * ppm_mass_error);
            if (start <= 0 || end >= Config_Help.MaxMass)
                return -1;
            double maxInten = 0.0;
            int max_k = -1;
            for (int k = mass_inten[start - 1]; k < mass_inten[end]; ++k)
            {
                PEAK peak = (PEAK)(peaks[k]);
                double tmpd = System.Math.Abs((peak.Mass - mass) / mass);
                if (tmpd <= ppm_mass_error && peak.Intensity > maxInten)
                {
                    maxInten = peak.Intensity;
                    max_k = k;
                }
            }
            return max_k;
        }
        //误差在ppm_mass_error范围内的误差最小的峰的索引号
        public static int IsInWithPPM2(double mass, int[] mass_inten, double ppm_mass_error, ObservableCollection<PEAK> peaks)
        {
            int start = (int)(mass - mass * ppm_mass_error);
            int end = (int)(mass + mass * ppm_mass_error);
            if (start <= 0 || end >= Config_Help.MaxMass)
                return -1;
            double min_mz_error = double.MaxValue;
            int min_k = -1;
            for (int k = mass_inten[start - 1]; k < mass_inten[end]; ++k)
            {
                PEAK peak = (PEAK)(peaks[k]);
                double tmpd = System.Math.Abs((peak.Mass - mass) / mass);
                if (tmpd <= ppm_mass_error && tmpd < min_mz_error)
                {
                    min_mz_error = tmpd;
                    min_k = k;
                }
            }
            return min_k;
        }
        public static void parse_charge(ObservableCollection<PEAK> peaks, double max_inten, double inten_t)
        {
            //首先将所有谱峰的电荷清为0
            for (int i = 0; i < peaks.Count; ++i)
            {
                peaks[i].Charge = 0;
                peaks[i].Is_mono = false;
            }
            max_inten /= 100.0;
            int[] mass_inten = new int[Config_Help.MaxMass];
            for (int k = 0; k < peaks.Count; ++k)
            {
                int massi = (int)peaks[k].Mass;
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

            bool[] is_mark_flag = new bool[peaks.Count];
            int max_charge = 4; //最大考虑四电荷的情况
            int max_num = 2; //如果在某根峰后面有连续的二根峰那么则赋值为对应的电荷，否则赋值为0电荷，即不知道电荷
            double ppm_mass_error = 20e-6; //误差为20ppm
            double mass_iso = Config_Help.mass_ISO;
            for (int i = 0; i < peaks.Count; ++i)
            {
                //if (is_mark_flag[i])
                //    continue;
                if (peaks[i].Intensity * max_inten < inten_t)
                    continue;
                is_mark_flag[i] = true;
                for (int charge = max_charge; charge >= 1; --charge)
                {
                    List<int> isotope_index = new List<int>();
                    double cur_mass = peaks[i].Mass + mass_iso / charge;
                    int index = IsInWithPPM(cur_mass, mass_inten, ppm_mass_error, peaks);
                    while (index != -1 && peaks[index].Intensity * max_inten >= inten_t)
                    {
                        isotope_index.Add(index);
                        cur_mass += mass_iso / charge;
                        index = IsInWithPPM(cur_mass, mass_inten, ppm_mass_error, peaks);
                    }
                    if (isotope_index.Count >= max_num)
                    {
                        if (peaks[i].Charge < charge)
                        {
                            peaks[i].Charge = charge;
                            peaks[i].Is_mono = true;
                        }
                        for (int j = 0; j < isotope_index.Count; ++j)
                        {
                            if (peaks[isotope_index[j]].Charge < charge)
                                peaks[isotope_index[j]].Charge = charge;
                            is_mark_flag[isotope_index[j]] = true;
                        }
                    }
                }
            }
        }

        int IComparable.CompareTo(Object obj)
        {
            PEAK temp = (PEAK)obj;
            if (this.Mass > temp.Mass)
                return 1;
            else if (this.Mass < temp.Mass)
                return -1;
            return 0;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            PEAK temp = obj as PEAK;
            if (temp == null) return false;
            else return Equals(temp);
        }
        public override int GetHashCode()
        {
            return this.Mass.GetHashCode();
        }
        public bool Equals(PEAK other)
        {
            if (other == null) return false;
            return (this.Mass.Equals(other.Mass)); // && this.mod_index.Equals(other.mod_index)
        }
    }
    //里面包含mz,intensity及scan
    public class PEAK_MS1
    {
        public double mz { get; set; }
        public double intensity { get; set; }
        public int scan { get; set; }

        public PEAK_MS1(double mz, double intensity, int scan)
        {
            this.mz = mz;
            this.intensity = intensity;
            this.scan = scan;
        }
    }
}
