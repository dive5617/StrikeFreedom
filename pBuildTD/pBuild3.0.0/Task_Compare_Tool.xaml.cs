using OxyPlot;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Task_Compare_Tool.xaml 的交互逻辑
    /// </summary>
    public partial class Task_Compare_Tool : Window
    {
        public Task_Compare_Tool()
        {
            InitializeComponent();
        }
        private void open_btn_clk(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog f_dialog = new System.Windows.Forms.OpenFileDialog();
            f_dialog.Filter = "task files (*.tsk)|*.tsk";
            //f_dialog.Filter = "pFind task files (*.pFind)|*.pFind|pNovo task files (*.pNovo)|*.pNovo";
            System.Windows.Forms.DialogResult dresult = f_dialog.ShowDialog();
            if (dresult == System.Windows.Forms.DialogResult.Cancel)
                return;

            string file_path = f_dialog.FileName;
            int last_index = file_path.LastIndexOf('\\');
            string folder_path = file_path.Substring(0, last_index);
            Task check_task = new Task();
            if (!check_task.check_task_path(folder_path, "pTop")) //目前只支持pFind结果的比较
            {
                MessageBox.Show(Message_Help.PATH_INVALID);
                return;
            }
            this.all_tasks_lv.Items.Add(folder_path);
        }

        private void compare_btn_clk(object sender, RoutedEventArgs e)
        {
            if (this.all_tasks_lv.Items.Count <= 1 || this.all_tasks_lv.Items.Count >= 4)
            {
                MessageBox.Show(Message_Help.TASK_COMPARE_WRONG);
                return;
            }
            List<Task> tasks = new List<Task>();
            List<List<string>> rawss_path = new List<List<string>>();
            for (int i = 0; i < this.all_tasks_lv.Items.Count; ++i)
            {
                Task task = new Task(this.all_tasks_lv.Items[i] as string, "pTop");
                tasks.Add(task);
                rawss_path.Add(task.get_raws_path());
            }
            bool is_right = true;
            for (int i = 0; i < rawss_path.Count; ++i)
            {
                for (int j = i + 1; j < rawss_path.Count; ++j)
                {
                    if (is_not_equal(rawss_path[i], rawss_path[j]))
                    {
                        string caption = "Warning";
                        string wrong_message = Message_Help.TASK_NOT_EQUAL + "\r\n";
                        for (int k = 0; k < rawss_path.Count; ++k)
                        {
                            wrong_message += (k + 1) + ":\r\n";
                            for (int p = 0; p < rawss_path[k].Count; ++p)
                                wrong_message += rawss_path[k][p] + "\r\n";
                        }
                        //MessageBoxButton buttons = MessageBoxButton.YesNo;
                        //MessageBoxImage icon = MessageBoxImage.Warning;
                        //if (MessageBox.Show(wrong_message, caption, buttons, icon) == MessageBoxResult.Yes)
                        //{
                        //    is_right = true;
                        //}
                        //else
                        //{
                        //    is_right=false;
                        //}
                    }
                }
            }
            if (!is_right)
                return;
            
            //对比List<task>的结果
            int raws_number = rawss_path[0].Count;
            System.Collections.Hashtable raw_index = new System.Collections.Hashtable();
            for (int i = 0; i < raws_number; ++i)
            {
                raw_index[rawss_path[0][i]] = i;
            }
            List<Task_Compare_Sample> task_compares = new List<Task_Compare_Sample>();
            for (int i = 0; i < tasks.Count; ++i)
            {
                Task_Compare_Sample tcs = new Task_Compare_Sample(tasks[i], rawss_path[i].Count);
                for (int j = 0; j < rawss_path[i].Count; ++j)
                {
                    string raw_path = rawss_path[i][j];
                    string raw_name = raw_path.Split('\\').Last();
                    raw_name = raw_name.Split('.').First();
                    tcs.update_psms((int)raw_index[rawss_path[i][j]], raw_name);
                }
                task_compares.Add(tcs);
            }
            //分RAW来进行比较
            if (task_compares.Count == 2) //两个任务的比较 
            {
                Task_Compare_Sample ta = task_compares[0];
                Task_Compare_Sample tb = task_compares[1];
                string result = "";
                List<int> numbers = new List<int>();
                for (int i = 0; i < raws_number; ++i)
                {
                    List<PSM> ta_psms = ta.psms[i];
                    List<PSM> tb_psms = tb.psms[i];
                    int same_number = get_same_number(ta_psms, tb_psms);
                    string raw_name = ta_psms[0].Title.Split('.')[0] + ".raw";
                    result += "---------------------" + "\r\n";
                    result += raw_name + " :\r\n";
                    result += "A: " + ta_psms.Count + "\r\n";
                    result += "B: " + tb_psms.Count + "\r\n";
                    result += "A&B: " + same_number + "\r\n";
                    result += "---------------------" + "\r\n";
                    numbers.Add(ta_psms.Count);
                    numbers.Add(tb_psms.Count);
                    numbers.Add(same_number);
                }
                Display_Help dh = new Display_Help();
                //有两个任务，有多个RAW的结果，每个RAW的结果都有一张图
                PlotModel model = dh.display_compare_result(numbers);
                Plot oxyplot = new Plot();
                oxyplot.Model = model;
                oxyplot.Width = 400;
                oxyplot.Height = 400;
                this.compare_result_sp.Children.Add(oxyplot);
                MessageBox.Show(result);
            }
            else if (task_compares.Count == 3) //三个任务的比较
            {
                Task_Compare_Sample ta = task_compares[0];
                Task_Compare_Sample tb = task_compares[1];
                Task_Compare_Sample tc = task_compares[2];
                string result = "";
                List<int> numbers = new List<int>();
                for (int i = 0; i < raws_number; ++i)
                {
                    List<PSM> ta_psms = ta.psms[i];
                    List<PSM> tb_psms = tb.psms[i];
                    List<PSM> tc_psms = tc.psms[i];
                    int same_number1 = get_same_number(ta_psms, tb_psms);
                    int same_number2 = get_same_number(ta_psms, tc_psms);
                    int same_number3 = get_same_number(tb_psms, tc_psms);
                    int same_number4 = get_same_number(ta_psms, tb_psms, tc_psms);
                    string raw_name = ta_psms[0].Title.Split('.')[0] + ".raw";
                    result += "---------------------" + "\r\n";
                    result += raw_name + " :\r\n";
                    result += "A: " + ta_psms.Count + "\r\n";
                    result += "B: " + tb_psms.Count + "\r\n";
                    result += "C: " + tc_psms.Count + "\r\n";
                    result += "A&B: " + same_number1 + "\r\n";
                    result += "A&C: " + same_number2 + "\r\n";
                    result += "B&C: " + same_number3 + "\r\n";
                    result += "A&B&C: " + same_number4 + "\r\n";
                    result += "---------------------" + "\r\n";
                    numbers.Add(ta_psms.Count);
                    numbers.Add(tb_psms.Count);
                    numbers.Add(tc_psms.Count);
                    numbers.Add(same_number1);
                    numbers.Add(same_number2);
                    numbers.Add(same_number3);
                    numbers.Add(same_number4);
                }
                Display_Help dh = new Display_Help();
                //有三个任务，有多个RAW的结果，每个RAW的结果都有一张图
                PlotModel model = dh.display_compare_result2(numbers);
                Plot oxyplot = new Plot();
                oxyplot.Model = model;
                oxyplot.Width = 400;
                oxyplot.Height = 400;
                this.compare_result_sp.Children.Add(oxyplot);
                MessageBox.Show(result);
            }
        }
        private int get_same_number(List<PSM> ta_psms, List<PSM> tb_psms, List<PSM> tc_psms)
        {
            int ta_i = 0, tb_i = 0, tc_i = 0;
            int same_number = 0;
            while (ta_i < ta_psms.Count && tb_i < tb_psms.Count && tc_i < tc_psms.Count)
            {
                if (string.Compare(ta_psms[ta_i].Title, tb_psms[tb_i].Title) < 0 && string.Compare(ta_psms[ta_i].Title, tc_psms[tc_i].Title) < 0)
                    ta_i++;
                else if (string.Compare(tb_psms[tb_i].Title, ta_psms[ta_i].Title) < 0 && string.Compare(tb_psms[tb_i].Title, tc_psms[tc_i].Title) < 0)
                    tb_i++;
                else if (string.Compare(tc_psms[tc_i].Title, ta_psms[ta_i].Title) < 0 && string.Compare(tc_psms[tc_i].Title, tb_psms[tb_i].Title) < 0)
                    tc_i++;
                else if (string.Compare(ta_psms[ta_i].Title, tb_psms[tb_i].Title) == 0 && string.Compare(ta_psms[ta_i].Title, tc_psms[tc_i].Title) < 0)
                {
                    ta_i++;
                    tb_i++;
                }
                else if (string.Compare(ta_psms[ta_i].Title, tc_psms[tc_i].Title) == 0 && string.Compare(ta_psms[ta_i].Title, tb_psms[tb_i].Title) < 0)
                {
                    ta_i++;
                    tc_i++;
                }
                else if (string.Compare(tb_psms[tb_i].Title, tc_psms[tc_i].Title) == 0 && string.Compare(tb_psms[tb_i].Title, ta_psms[ta_i].Title) < 0)
                {
                    tb_i++;
                    tc_i++;
                }
                else
                {
                    if (string.Compare(ta_psms[ta_i].Sq, tb_psms[tb_i].Sq) == 0 && string.Compare(ta_psms[ta_i].Sq, tc_psms[tc_i].Sq) == 0)
                    {
                        same_number++;
                    }
                    ta_i++;
                    tb_i++;
                    tc_i++;
                }
            }
            return same_number;
        }
        private int get_same_number(List<PSM> ta_psms, List<PSM> tb_psms)
        {
            int ta_i = 0, tb_i = 0;
            int same_number = 0;
            StreamWriter sw = new StreamWriter("1.txt");
            StreamWriter sw2 = new StreamWriter("2.txt");
            StreamWriter sw3 = new StreamWriter("notsame.txt");
            System.Collections.Hashtable h1 = new System.Collections.Hashtable();
            System.Collections.Hashtable h2 = new System.Collections.Hashtable();
            for (int i = 0; i < ta_psms.Count; ++i)
            {
                string sq = ta_psms[i].Sq + "@" + ta_psms[i].Charge;
                if (h1[sq] == null)
                {
                    h1[sq] = 1;
                }
                else
                {
                    int tmp = (int)h1[sq];
                    ++tmp;
                    h1[sq] = tmp;
                }
            }
            for (int i = 0; i < tb_psms.Count; ++i)
            {
                string sq = tb_psms[i].Sq + "@" + tb_psms[i].Charge;
                if (h2[sq] == null)
                {
                    h2[sq] = 1;
                }
                else
                {
                    int tmp = (int)h2[sq];
                    ++tmp;
                    h2[sq] = tmp;
                }
            }
            while (ta_i < ta_psms.Count && tb_i < tb_psms.Count)
            {
                if (!ta_psms[ta_i].Is_target_flag)
                {
                    ++ta_i;
                    continue;
                }
                if (!tb_psms[tb_i].Is_target_flag)
                {
                    ++tb_i;
                    continue;
                }
                if (string.Compare(ta_psms[ta_i].Title, tb_psms[tb_i].Title) < 0)
                {
                    sw.WriteLine(ta_psms[ta_i].Title + "\t" + ta_psms[ta_i].Score);
                    ta_i++;
                }
                else if (string.Compare(ta_psms[ta_i].Title, tb_psms[tb_i].Title) > 0)
                {
                    sw2.WriteLine(tb_psms[tb_i].Title + "\t" + tb_psms[tb_i].Score);
                    tb_i++;
                }
                else //如果两个PSM的title一致，查看SQ是否一致
                {
                    if (string.Compare(ta_psms[ta_i].Sq, tb_psms[tb_i].Sq) == 0) //如果Sq一致
                    {
                        same_number++;
                    }
                    else
                    {
                        int c1 = (int)h1[ta_psms[ta_i].Sq + "@" + ta_psms[ta_i].Charge];
                        int c2 = (int)h2[tb_psms[tb_i].Sq + "@" + tb_psms[tb_i].Charge];
                        if (c1 > 1 && c2 > 1)
                            sw3.WriteLine(ta_psms[ta_i].Title + "\t" + ta_psms[ta_i].Sq + "\t" + tb_psms[tb_i].Sq);
                    }
                    ta_i++;
                    tb_i++;
                }
            }
            sw.Flush();
            sw.Close();
            sw2.Flush();
            sw2.Close();
            sw3.Flush();
            sw3.Close();
            return same_number;
        }
        private bool is_not_equal(List<string> a, List<string> b)
        {
            if (a.Count != b.Count)
                return true;
            a.Sort();
            b.Sort();
            for (int i = 0; i < a.Count; ++i)
            {
                if (a[i] != b[i])
                    return true;
            }
            return false;
        }
    }
    public class Task_Compare_Sample
    {
        public Task task;
        public List<List<PSM>> psms; //分RAW来获得psms

        public Task_Compare_Sample(Task task, int raws_number)
        {
            this.task = task;
            this.psms = new List<List<PSM>>();
            for(int i=0;i<raws_number;++i)
                psms.Add(new List<PSM>());
        }

        public void update_psms(int index, string raw_name) //根据卡FDR1%的结果将raw_name的RAW的结果放入到psms[index]中
        {
            const double fdr_value = 0.01;
            if (index < 0 || index >= psms.Count)
                return;
            List<PSM> all_psms = new List<PSM>(File_Help.readPSM(task.identification_file));
            for (int i = 0; i < all_psms.Count; ++i)
            {
                string title = all_psms[i].Title;
                title = title.Split('.')[0];
                if (title == raw_name && all_psms[i].Q_value <= fdr_value)
                {
                    all_psms[i].Sq = all_psms[i].Sq.Replace('L', 'I'); //将得到的序列的L全部转化成I
                    this.psms[index].Add((PSM)all_psms[i]);
                }
            }
            this.psms[index].Sort();
        }
    }
}
