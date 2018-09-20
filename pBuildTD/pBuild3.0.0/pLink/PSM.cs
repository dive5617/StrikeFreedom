using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild.pLink
{
    public class PSM : pBuild.PSM
    {
        public int Peptide_Number { get; set; }
        public List<Peptide> Peptide { get; set; }
        public List<int> Peptide_Link_Position { get; set; }
        public List<double> Peptide_Score { get; set; }

        public PSM(int id, string title, int charge, double spectra_mass, string sq, string mod_sites,
            double delta_mass, double delta_mass_ppm, int peptide_number, List<Peptide> peptide, List<int> peptide_link_postioin,
            List<double> peptide_score, string pep_flag, bool target)
        {
            this.Id = id;
            this.Title = title;
            this.Charge = charge;
            this.Spectra_mass = spectra_mass;
            this.Sq = sq;
            this.Mod_sites = mod_sites;
            this.Delta_mass = delta_mass;
            this.Delta_mass_PPM = delta_mass_ppm;
            this.Peptide_Number = peptide_number;
            this.Peptide = peptide;
            this.Peptide_Link_Position = peptide_link_postioin;
            this.Peptide_Score = peptide_score;
            this.Score = peptide_score.First();
            this.Pep_flag = pep_flag;
            this.Is_target_flag = target;
        }

        public int get_scan()
        {
            return int.Parse(this.Title.Split('.')[1]);
        }

        public double get_TheoryMass()
        {
            double mass = 0.0;
            for (int i = 0; i < this.Peptide.Count; ++i)
            {
                this.Peptide[i].update();
                mass += this.Peptide[i].Pepmass;
            }
            mass += 0.0;
            return mass;
        }
    }
}
