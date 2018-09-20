using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace pBuild
{
    /// <summary>
    /// display_ms2_table.xaml 的交互逻辑
    /// </summary>
    public partial class Display_ms2_table : Window
    {
        MainWindow main_win;
        public Display_ms2_table(MainWindow main_win)
        {
            InitializeComponent();
            this.main_win = main_win;
            const int margin = 10;
            if (main_win.Dis_help.Psm_help == null)
                return;
            List<string> sqs = new List<string>();
            if (main_win.Dis_help.Psm_help is PSM_Help)
            {
                PSM_Help ph = main_win.Dis_help.Psm_help as PSM_Help;
                if (main_win.Dis_help.Psm_help.Mix_Flag == 0)
                {
                    sqs.Add(ph.Pep.Sq);
                }
                else
                {
                    for (int i = 0; i < ph.Mix_peps.Count; ++i)
                        sqs.Add(ph.Mix_peps[i].Sq);
                }
            }
            else if (main_win.Dis_help.Psm_help is pLink.PSM_Help_2)
            {
                pLink.PSM_Help_2 ph2 = main_win.Dis_help.Psm_help as pLink.PSM_Help_2;
                sqs.Add(ph2.Pep1.Sq);
                sqs.Add(ph2.Pep2.Sq);
            }
            else if (main_win.Dis_help.Psm_help is pLink.PSM_Help_3)
            {
                pLink.PSM_Help_3 ph3 = main_win.Dis_help.Psm_help as pLink.PSM_Help_3;
                sqs.Add(ph3.Pep1.Sq);
                sqs.Add(ph3.Pep2.Sq);
                sqs.Add(ph3.Pep3.Sq);
            }
            for (int p = 0; p < main_win.Dis_help.Psm_help.Peptide_Number; ++p)
            {
                RowDefinition rd_pep = new RowDefinition();
                rd_pep.Height = new GridLength();
                this.psm_table.RowDefinitions.Add(rd_pep);
                Grid grid = new Grid();
                grid.ShowGridLines = true;
                Grid.SetRow(grid, p);
                this.psm_table.Children.Add(grid);

                string sq = sqs[p];
                List<PSM_Match> N_all_matches = main_win.Dis_help.Psm_help.Psm_detail.N_all_matches;
                List<PSM_Match> C_all_matches = main_win.Dis_help.Psm_help.Psm_detail.C_all_matches;
                for (int i = 0; i <= sq.Length; ++i)
                {
                    ColumnDefinition cd = new ColumnDefinition();
                    grid.ColumnDefinitions.Add(cd);
                }
                RowDefinition rd0 = new RowDefinition();
                rd0.Height = new GridLength();
                grid.RowDefinitions.Add(rd0);
                for (int i = 0; i < N_all_matches.Count; ++i)
                {
                    if (N_all_matches[i].fragment_type != "")
                    {
                        RowDefinition rd = new RowDefinition();
                        rd.Height = new GridLength();
                        grid.RowDefinitions.Add(rd);
                    }
                }
                for (int i = 0; i < C_all_matches.Count; ++i)
                {
                    if (C_all_matches[i].fragment_type != "")
                    {
                        RowDefinition rd = new RowDefinition();
                        rd.Height = new GridLength();
                        grid.RowDefinitions.Add(rd);
                    }
                }
                TextBlock tb0 = new TextBlock();
                tb0.Text = "#";
                tb0.HorizontalAlignment = HorizontalAlignment.Center;
                tb0.Margin = new Thickness(margin);
                Grid.SetColumn(tb0, 0);
                Grid.SetRow(tb0, 0);
                grid.Children.Add(tb0);
                for (int i = 0; i < sq.Length; ++i)
                {
                    TextBlock tb = new TextBlock();
                    if (i == 0)
                    {
                        tb.Text = sq[i] + "(1/*)";
                    }
                    else if (i == sq.Length - 1)
                    {
                        tb.Text = sq[i] + "(*/1)";
                    }
                    else
                    {
                        tb.Text = sq[i] + "(" + (i + 1) + "/" + (sq.Length - i) + ")";
                    }
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    tb.Margin = new Thickness(margin);
                    Grid.SetColumn(tb, i + 1);
                    Grid.SetRow(tb, 0);
                    grid.Children.Add(tb);
                }
                int all_row = 0;
                for (int i = 0; i < N_all_matches.Count; ++i)
                {
                    if (N_all_matches[i].fragment_type != "")
                    {
                        ++all_row;
                        TextBlock tb_type = new TextBlock();
                        tb_type.Text = N_all_matches[i].fragment_type;
                        tb_type.HorizontalAlignment = HorizontalAlignment.Center;
                        tb_type.Margin = new Thickness(margin);
                        Grid.SetRow(tb_type, all_row);
                        Grid.SetColumn(tb_type, 0);
                        grid.Children.Add(tb_type);
                        for (int j = 0; j < sq.Length; ++j)
                        {
                            TextBlock tb = new TextBlock();
                            if (j < sq.Length - 1)
                            {
                                tb.Text = N_all_matches[i].fragment_theory_mass[p][j]; //+ "," + N_all_matches[i].fragment_match_intensity[p][j]
                                if (N_all_matches[i].fragment_match_flag[p][j] == 1)
                                {
                                    Color color = new Color();
                                    if (tb_type.Text[0] == 'a')
                                    {
                                        color = Color.FromArgb(main_win.Dis_help.ddh.A_Match_Color.A, main_win.Dis_help.ddh.A_Match_Color.R,
                                            main_win.Dis_help.ddh.A_Match_Color.G, main_win.Dis_help.ddh.A_Match_Color.B);
                                    }
                                    else if (tb_type.Text[0] == 'b')
                                    {
                                        color = Color.FromArgb(main_win.Dis_help.ddh.B_Match_Color.A, main_win.Dis_help.ddh.B_Match_Color.R,
                                            main_win.Dis_help.ddh.B_Match_Color.G, main_win.Dis_help.ddh.B_Match_Color.B);
                                    }
                                    else if (tb_type.Text[0] == 'c')
                                    {
                                        color = Color.FromArgb(main_win.Dis_help.ddh.C_Match_Color.A, main_win.Dis_help.ddh.C_Match_Color.R,
                                            main_win.Dis_help.ddh.C_Match_Color.G, main_win.Dis_help.ddh.C_Match_Color.B);
                                    }
                                    tb.Foreground = new SolidColorBrush(color);
                                    tb.FontWeight = FontWeights.Bold;
                                }
                            }
                            else
                                tb.Text = "*";
                            tb.HorizontalAlignment = HorizontalAlignment.Center;
                            tb.Margin = new Thickness(margin);
                            Grid.SetRow(tb, all_row);
                            Grid.SetColumn(tb, j + 1);
                            grid.Children.Add(tb);
                        }
                    }
                }
                for (int i = 0; i < C_all_matches.Count; ++i)
                {
                    if (C_all_matches[i].fragment_type != "")
                    {
                        ++all_row;
                        TextBlock tb_type = new TextBlock();
                        tb_type.Text = C_all_matches[i].fragment_type;
                        tb_type.HorizontalAlignment = HorizontalAlignment.Center;
                        tb_type.Margin = new Thickness(margin);
                        Grid.SetRow(tb_type, all_row);
                        Grid.SetColumn(tb_type, 0);
                        grid.Children.Add(tb_type);
                        for (int j = 0; j < sq.Length; ++j)
                        {
                            TextBlock tb = new TextBlock();
                            if (j < sq.Length - 1)
                            {
                                tb.Text = C_all_matches[i].fragment_theory_mass[p][j]; // + "," + C_all_matches[i].fragment_match_intensity[p][j]
                                if (C_all_matches[i].fragment_match_flag[p][j] == 1)
                                {
                                    Color color = new Color();
                                    if (tb_type.Text[0] == 'x')
                                    {
                                        color = Color.FromArgb(main_win.Dis_help.ddh.X_Match_Color.A, main_win.Dis_help.ddh.X_Match_Color.R,
                                            main_win.Dis_help.ddh.X_Match_Color.G, main_win.Dis_help.ddh.X_Match_Color.B);
                                    }
                                    else if (tb_type.Text[0] == 'y')
                                    {
                                        color = Color.FromArgb(main_win.Dis_help.ddh.Y_Match_Color.A, main_win.Dis_help.ddh.Y_Match_Color.R,
                                            main_win.Dis_help.ddh.Y_Match_Color.G, main_win.Dis_help.ddh.Y_Match_Color.B);
                                    }
                                    else if (tb_type.Text[0] == 'z')
                                    {
                                        color = Color.FromArgb(main_win.Dis_help.ddh.Z_Match_Color.A, main_win.Dis_help.ddh.Z_Match_Color.R,
                                            main_win.Dis_help.ddh.Z_Match_Color.G, main_win.Dis_help.ddh.Z_Match_Color.B);
                                    }
                                    else if (tb_type.Text[0] == '*') //中性丢失
                                    {
                                        color = Color.FromArgb(main_win.Dis_help.ddh.Lose_Match_Color.A, main_win.Dis_help.ddh.Lose_Match_Color.R,
                                            main_win.Dis_help.ddh.Lose_Match_Color.G, main_win.Dis_help.ddh.Lose_Match_Color.B);
                                    }
                                    tb.Foreground = new SolidColorBrush(color);
                                    tb.FontWeight = FontWeights.Bold;
                                }
                            }
                            else
                                tb.Text = "*";
                            tb.HorizontalAlignment = HorizontalAlignment.Center;
                            tb.Margin = new Thickness(margin);
                            Grid.SetRow(tb, all_row);
                            Grid.SetColumn(tb, sq.Length - j);
                            grid.Children.Add(tb);
                        }
                    }
                }
            }
        }

        private void closed_event(object sender, EventArgs e)
        {
            this.main_win.display_ms2_dialog = null;
        }
    }
}
