using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild.Test
{
    public class N_Statistic
    {
        public static void get_N(ObservableCollection<PSM> psms)
        {
            List<double> res = new List<double>();
            for (int i = 0; i < 10; ++i)
                res.Add(0.0);
            int fm = psms.Count;
            for (int i = 0; i < psms.Count; ++i)
            {
                int N_count = psms[i].get_N15_number();
                for (int j = 0; j < psms[i].Cand_peptides.Count; ++j)
                {
                    int N_count2 = psms[i].Cand_peptides[j].Get_Number_ByElementName("N");
                    if (N_count != N_count2)
                        res[j] = res[j] + 1.0;
                }
            }
            string line = "";
            for (int i = 0; i < 10; ++i)
            {
                res[i] = res[i] / fm;
                line += res[i].ToString("P2") + "\r\n";
            }
            System.Windows.MessageBox.Show(line);
        }
    }
}
