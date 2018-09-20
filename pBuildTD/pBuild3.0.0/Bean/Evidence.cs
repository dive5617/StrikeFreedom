using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;

namespace pBuild
{
    public class Evidence
    {//基本数据结构
        private int number; //Number of Evidence
        public int Number
        {
            get { return number; }
            set { number = value; }
        }

        private double ratio; //Ratio(Based on least interfered peak)
        public double Ratio
        {
            get { return ratio; }
            set { ratio = value; }
        }

        private double sigma;
        public double Sigma
        {
            get { return sigma; }
            set { sigma = value; }
        }

        private String peptide;//@后面的标记 肽段
        public String Peptide
        {
            get { return peptide; }
            set { peptide = value; }
        }

        private List<List<Double>> theoricalIsotopicPeakList = new List<List<double>>();
        public List<List<Double>> TheoricalIsotopicPeakList
        {
            get { return theoricalIsotopicPeakList; }
            set { theoricalIsotopicPeakList = value; }
        }

        private List<List<Double>> theoricalIsotopicMZList = new List<List<double>>(); //各种同位素峰的理论质荷比
        public List<List<Double>> TheoricalIsotopicMZList
        {
            get { return theoricalIsotopicMZList; }
            set { theoricalIsotopicMZList = value; }
        }

        private List<Int32> scanTime = new List<Int32>();//扫描时间(时间跨度为定值)
        public List<Int32> ScanTime
        {
            get { return scanTime; }
            set { scanTime = value; }
        }
        public List<int> StartTimes = new List<int>();
        public List<int> EndTimes = new List<int>();
        private int startTime, endTime;
        public int StartTime
        {
            get { return startTime; }
            set { startTime = value; }
        }
        public int EndTime
        {
            get { return endTime; }
            set { endTime = value; }
        }

        private List<int> group = new List<int>();//用于着色，可不用
        public List<int> Group
        {
            get { return group; }
            set { group = value; }
        }

        private List<List<List<Double>>> actualIsotopicPeakList = new List<List<List<Double>>>();//实际的信号强度
        public List<List<List<Double>>> ActualIsotopicPeakList
        {
            get { return actualIsotopicPeakList; }
            set { actualIsotopicPeakList = value; }
        }
    }
}
