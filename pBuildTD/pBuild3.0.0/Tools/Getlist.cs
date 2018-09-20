using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;

namespace pBuild
{
    public class Getlist
    {
        public Evidence evi = new Evidence();

        public Getlist()
        {
            evi = new Evidence();
        }

        public void GetEvidenceFromChrom(Chrome_Help chrome_help, MainWindow mainW)
        {
            evi.TheoricalIsotopicPeakList.Clear();
            evi.TheoricalIsotopicMZList.Clear();
            evi.ActualIsotopicPeakList.Clear();
            evi.ScanTime.Clear();
            evi.Number = chrome_help.theory_isotopes.Count;
            evi.Ratio = 0.0;
            evi.Sigma = 1.0;
            evi.Peptide = mainW.new_peptide.Sq;
            bool flag = true;
            for (int i = 0; i < chrome_help.theory_isotopes.Count; ++i)
            {
                evi.TheoricalIsotopicMZList.Add(new List<double>());
                evi.TheoricalIsotopicPeakList.Add(new List<double>());
                evi.ActualIsotopicPeakList.Add(new List<List<double>>());
                for (int j = 0; j < chrome_help.theory_isotopes[i].mz.Count; ++j)
                {
                    evi.TheoricalIsotopicMZList[i].Add(chrome_help.theory_isotopes[i].mz[j]);
                    evi.TheoricalIsotopicPeakList[i].Add(chrome_help.theory_isotopes[i].intensity[j]);
                    evi.ActualIsotopicPeakList[i].Add(new List<double>());
                    List<PEAK_MS1> mz_list = mainW.get_List(chrome_help.theory_isotopes[i].mz[j]);
                    for (int k = 0; k < mz_list.Count; ++k)
                    {
                        evi.ActualIsotopicPeakList[i][j].Add(mz_list[k].intensity);
                        if(flag)
                            evi.ScanTime.Add(mz_list[k].scan);
                    }
                    flag = false;
                }
            }
            evi.StartTime = 3;
            evi.EndTime = evi.ScanTime.Count - 4;
        }
        public void GetEvidenceFromList_old(List<String> list)
        {
            try
            {
                evi.Number = int.Parse(list[1]);
                //初始化
                evi.ActualIsotopicPeakList.Clear();
                evi.TheoricalIsotopicMZList.Clear();
                evi.TheoricalIsotopicPeakList.Clear();
                for (int i = 0; i < evi.Number; ++i)
                {
                    evi.ActualIsotopicPeakList.Add(new List<List<double>>());
                    evi.TheoricalIsotopicMZList.Add(new List<double>());
                    evi.TheoricalIsotopicPeakList.Add(new List<double>());
                }

                int evidence_number = int.Parse(list[1]);
                if (evidence_number != 1) //如果是定性跑的定量，那么没有轻重对儿，就没有比值，list[3]==""
                {
                    string ratio_line = list[3]; //Ratio Line
                    string[] all_ratio_and_sigma = ratio_line.Split('\t');
                    string Ratio2 = all_ratio_and_sigma[2];
                    string Sigma2 = all_ratio_and_sigma[3];
                    string[] ratio_2 = Ratio2.Split('=');
                    string[] sigma_2 = Sigma2.Split('=');
                    //轻重比例和Sigma，均取的是最小干扰
                    evi.Ratio = double.Parse(ratio_2[1]);
                    evi.Sigma = double.Parse(sigma_2[1]);
                }
                //鉴定到的肽段
                evi.Peptide = list[6 + evidence_number].Split('\t')[0];
                String[] peaks = list[8 + evidence_number].Split(new Char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);//第一行输入数据 理论同位素信号强度
                //理论同位素强度
                for (int i = 0; i < peaks.Length; i++)
                {
                    evi.TheoricalIsotopicPeakList[0].Add(Double.Parse(peaks[i]));
                    evi.Group.Add(0);
                }
                //理论同位素质荷比
                String[] mz = list[9 + evidence_number].Split(new Char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);//第二行输入数据 理论同位素质荷比
                for (int i = 0; i < mz.Length; i++)
                {
                    evi.TheoricalIsotopicMZList[0].Add(Double.Parse(mz[i]));
                }
                //扫描号
                String[] scans = list[11 + evidence_number].Split(new Char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);//第三行输入数据 观察时间点
                evi.ScanTime.Add(0);
                evi.ScanTime.Add(0);
                evi.ScanTime.Add(0);
                for (int i = 0; i < scans.Length; i++)
                {
                    evi.ScanTime.Add(int.Parse(scans[i]));
                }
                evi.ScanTime.Add(0);
                evi.ScanTime.Add(0);
                evi.ScanTime.Add(0);

                //开始时间和结束时间
                String[] time = list[13 + evidence_number].Split(new Char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                evi.StartTime = int.Parse(time[0]) + 3;
                evi.EndTime = int.Parse(time[1]) + 3;

                int start_index = 15 + evidence_number;
                //实际信号强度  二维数组
                for (int i = 0; i < mz.Length; i++)
                {
                    String[] actual_inten = list[start_index + i].Split(new Char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    List<Double> intenList = new List<double>();
                    intenList.Clear();
                    intenList.Add(0.0);
                    intenList.Add(0.0);
                    intenList.Add(0.0);
                    for (int j = 0; j < actual_inten.Length; j++) //ME 
                    {
                        intenList.Add(Double.Parse(actual_inten[j]));
                    }
                    intenList.Add(0.0);
                    intenList.Add(0.0);
                    intenList.Add(0.0);
                    evi.ActualIsotopicPeakList[0].Add(intenList);

                }

                //如果Number>1，则继续往下读
                for (int k = 1; k < evi.Number; ++k)
                {
                    int start = start_index + mz.Length + 4 + (k - 1) * (11 + mz.Length);
                    peaks = list[start].Split(new Char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);//第一行输入数据 理论同位素信号强度
                    //理论同位素强度
                    for (int i = 0; i < peaks.Length; i++)
                    {
                        evi.TheoricalIsotopicPeakList[k].Add(Double.Parse(peaks[i]));
                        evi.Group.Add(k);
                    }
                    //理论同位素质荷比
                    mz = list[start + 1].Split(new Char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);//第二行输入数据 理论同位素质荷比
                    for (int i = 0; i < mz.Length; i++)
                    {
                        evi.TheoricalIsotopicMZList[k].Add(Double.Parse(mz[i]));
                    }
                    //实际信号强度  二维数组
                    for (int i = 0; i < mz.Length; i++)
                    {
                        String[] actual_inten = list[start + 7 + i].Split(new Char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        List<Double> intenList = new List<double>();
                        intenList.Clear();
                        intenList.Add(0.0);
                        intenList.Add(0.0);
                        intenList.Add(0.0);
                        for (int j = 0; j < actual_inten.Length; j++)
                        {
                            intenList.Add(Double.Parse(actual_inten[j]));
                        }
                        intenList.Add(0.0);
                        intenList.Add(0.0);
                        intenList.Add(0.0);
                        evi.ActualIsotopicPeakList[k].Add(intenList);

                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public void GetEvidenceFromList(List<String> list)
        {
            try
            {
                evi.Number = int.Parse(list[0].Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries).Last());
                //初始化
                evi.ActualIsotopicPeakList.Clear();
                evi.TheoricalIsotopicMZList.Clear();
                evi.TheoricalIsotopicPeakList.Clear();
                for (int i = 0; i < evi.Number; ++i)
                {
                    evi.ActualIsotopicPeakList.Add(new List<List<double>>());
                    evi.TheoricalIsotopicMZList.Add(new List<double>());
                    evi.TheoricalIsotopicPeakList.Add(new List<double>());
                }

                int evidence_number = evi.Number;
                if (evidence_number != 1) //如果是定性跑的定量，那么没有轻重对儿，就没有比值，list[3]==""
                {
                    string[] ratio2_and_sigma = list[3].Split(new char[]{'\t'}, StringSplitOptions.RemoveEmptyEntries);
                    string Ratio2 = ratio2_and_sigma[1];
                    string Sigma2 = ratio2_and_sigma[2];
                    //轻重比例和Sigma，均取的是最小干扰
                    evi.Ratio = double.Parse(Ratio2);
                    evi.Sigma = double.Parse(Sigma2);
                }
                //鉴定到的肽段
                evi.Peptide = list[5].Split('\t')[1];
                String[] peaks = list[6].Split(new Char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);//第一行输入数据 理论同位素信号强度
                //理论同位素强度
                for (int i = 1; i < peaks.Length; i++)
                {
                    evi.TheoricalIsotopicPeakList[0].Add(Double.Parse(peaks[i]));
                    evi.Group.Add(0);
                }
                //理论同位素质荷比
                String[] mz = list[7].Split(new Char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);//第二行输入数据 理论同位素质荷比
                int mz_number = int.Parse(list[9].Split('\t')[1]);
                for (int i = 1; i < mz_number; i++)
                {
                    evi.TheoricalIsotopicMZList[0].Add(Double.Parse(mz[i]));
                }
                //扫描号
                String[] scans = list[8].Split(new Char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);//第三行输入数据 观察时间点
                evi.ScanTime.Add(0);
                evi.ScanTime.Add(0);
                evi.ScanTime.Add(0);
                for (int i = 1; i < scans.Length; i++)
                {
                    evi.ScanTime.Add(int.Parse(scans[i]));
                }
                evi.ScanTime.Add(0);
                evi.ScanTime.Add(0);
                evi.ScanTime.Add(0);
                
                //开始时间和结束时间
                String[] time = list[11 + mz_number].Split(new Char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                evi.StartTime = int.Parse(time[1]) + 3;
                evi.EndTime = int.Parse(time[2]) + 3;

                int start_index = 10;
                //实际信号强度  二维数组
                for (int i = 0; i < mz_number; i++)
                {
                    String[] actual_inten = list[start_index + i].Split(new Char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    List<Double> intenList = new List<double>();
                    intenList.Clear();
                    intenList.Add(0.0);
                    intenList.Add(0.0);
                    intenList.Add(0.0);
                    for (int j = 0; j < actual_inten.Length; j++) //ME 
                    {
                        intenList.Add(Double.Parse(actual_inten[j]));
                    }
                    intenList.Add(0.0);
                    intenList.Add(0.0);
                    intenList.Add(0.0);
                    evi.ActualIsotopicPeakList[0].Add(intenList);
                }
                string[] start_end1 = list[start_index + mz_number + 2].Split('\t');
                evi.StartTimes.Add(int.Parse(start_end1[1]) + 3);
                evi.EndTimes.Add(int.Parse(start_end1[2]) + 3);
                int start = start_index;
                //如果Number>1，则继续往下读
                for (int k = 1; k < evi.Number; ++k)
                {
                    //start += mz_number + 5; //如果evi.Number=3有问题？
                    string start_flag = "I,E," + k + ",02";
                    while (true)
                    {
                        if (list[start].StartsWith(start_flag))
                            break;
                        ++start;
                    }
                    peaks = list[start].Split(new Char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);//第一行输入数据 理论同位素信号强度
                    //理论同位素强度
                    for (int i = 1; i < peaks.Length; i++)
                    {
                        evi.TheoricalIsotopicPeakList[k].Add(Double.Parse(peaks[i]));
                        evi.Group.Add(k);
                    }
                    //理论同位素质荷比
                    mz = list[start + 1].Split(new Char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);//第二行输入数据 理论同位素质荷比
                    mz_number = int.Parse(list[start + 3].Split('\t')[1]);
                    for (int i = 1; i < mz_number; i++)
                    {
                        evi.TheoricalIsotopicMZList[k].Add(Double.Parse(mz[i]));
                    }
                    //实际信号强度  二维数组
                    for (int i = 0; i < mz_number; i++)
                    {
                        String[] actual_inten = list[start + 4 + i].Split(new Char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        List<Double> intenList = new List<double>();
                        intenList.Clear();
                        intenList.Add(0.0);
                        intenList.Add(0.0);
                        intenList.Add(0.0);
                        for (int j = 0; j < actual_inten.Length; j++)
                        {
                            intenList.Add(Double.Parse(actual_inten[j]));
                        }
                        intenList.Add(0.0);
                        intenList.Add(0.0);
                        intenList.Add(0.0);
                        evi.ActualIsotopicPeakList[k].Add(intenList);
                    }
                    string[] start_end2 = list[start + mz_number + 6].Split('\t');
                    evi.StartTimes.Add(int.Parse(start_end2[1]) + 3);
                    evi.EndTimes.Add(int.Parse(start_end2[2]) + 3);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
