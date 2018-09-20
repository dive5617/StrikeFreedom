using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class Protein_Ratio_Help
    {
        public static void compute_protein_ratio(ObservableCollection<Protein> proteins, ObservableCollection<PSM> psms)
        {
            const int number_t = 3;
            for (int i = 0; i < proteins.Count; ++i)
            {
                if (proteins[i].psm_index.Count <= number_t)
                {
                    int min_t = -1;
                    double min_sigma = double.MaxValue;
                    for (int j = 0; j < proteins[i].psm_index.Count; ++j)
                    {
                        if (min_sigma > psms[proteins[i].psm_index[j]].Sigma)
                        {
                            min_sigma = psms[proteins[i].psm_index[j]].Sigma;
                            min_t = j;
                        }
                    }
                    proteins[i].Ratio = psms[proteins[i].psm_index[min_t]].Ratio;
                }
                else
                {
                    List<double> ratios = new List<double>();
                    for (int j = 0; j < proteins[i].psm_index.Count; ++j)
                        ratios.Add(psms[proteins[i].psm_index[j]].Ratio);
                    ratios.Sort();
                    proteins[i].Ratio = ratios[ratios.Count / 2];
                }
            }
        }
    }
}
