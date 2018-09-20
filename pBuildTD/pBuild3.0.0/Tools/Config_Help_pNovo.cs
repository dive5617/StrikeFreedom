using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class Config_Help_pNovo
    {
        //pNovo将一个修饰用一个大写的英文字母表示，需要重新以修饰的名字进行显示
        //比如：J=M,Oxidation[M]，则AA_Modification['J']="M,Oxidation[M]"
        public static System.Collections.Hashtable AA_Modification = new System.Collections.Hashtable();

        public static Peptide parseBySQ(string sq)
        {
            Peptide pep = new Peptide();
            string newSQ = "";
            ObservableCollection<Modification> modifications = new ObservableCollection<Modification>();
            for (int i = 0; i < sq.Length; ++i)
            {
                if (AA_Modification.Contains(sq[i]))
                {
                    string value = (string)AA_Modification[sq[i]];
                    string[] strs = value.Split(',');
                    newSQ += strs[0].Trim()[0];
                    Modification modification = new Modification(i + 1, strs[1]);
                    modifications.Add(modification);
                }
                else
                    newSQ += sq[i];
            }
            pep.Sq = newSQ;
            pep.Mods = modifications;
            return pep;
        }
    }
}
