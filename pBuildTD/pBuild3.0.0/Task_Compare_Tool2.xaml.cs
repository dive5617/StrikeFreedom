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
    /// Task_Compare_Tool2.xaml 的交互逻辑
    /// </summary>
    public partial class Task_Compare_Tool2 : Window
    {
        public Task_Compare_Tool2()
        {
            InitializeComponent();
        }

        private void open_btn_clk(object sender, RoutedEventArgs e)
        {
            Button sender_obj = sender as Button;
            if (sender_obj == this.pFind_open_btn)
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
                if (!check_task.check_task_path(folder_path, "pTop"))
                {
                    MessageBox.Show(Message_Help.PATH_INVALID);
                    return;
                }
                this.all_tasks_lv.Items.Add(folder_path);
            }
            else if (sender_obj == this.maxQuant_open_btn)
            {
                System.Windows.Forms.OpenFileDialog f_dialog = new System.Windows.Forms.OpenFileDialog();
                System.Windows.Forms.DialogResult dresult = f_dialog.ShowDialog();
                if (dresult == System.Windows.Forms.DialogResult.Cancel)
                    return;
                string path = f_dialog.FileName;
                string file_name = path.Split('\\').Last();
                if (file_name != "msms.txt")
                {
                    MessageBox.Show(Message_Help.PATH_INVALID);
                    return;
                }
                this.all_tasks_lv2.Items.Add(path);
            }
        }

        private void compare_btn_clk(object sender, RoutedEventArgs e)
        {
            if (this.all_tasks_lv.Items.Count != 1 || this.all_tasks_lv2.Items.Count != 1)
                return;
            StreamReader sr_max = new StreamReader(all_tasks_lv2.Items[0] as string);
            StreamWriter sw_same = new StreamWriter("same.txt");
            StreamWriter sw_notSame = new StreamWriter("MaxQuant_pFindNot.txt");
            List<string> max_str = new List<string>();
            List<string> find_str = new List<string>();
            string line = sr_max.ReadLine();
            string[] titles = line.Split('\t');
            int raw_index = -1, scan_index = -1, sq_index = -1;
            for (int i = 0; i < titles.Length; ++i)
            {
                if (titles[i] == "Raw file")
                    raw_index = i;
                else if (titles[i] == "Scan number")
                    scan_index = i;
                else if (titles[i] == "Sequence")
                    sq_index = i;
            }
            if (raw_index == -1 || scan_index == -1 || sq_index == -1)
                return;
            while (!sr_max.EndOfStream)
            {
                line = sr_max.ReadLine();
                if (line.Trim() == "")
                    continue;
                string[] strs = line.Split('\t'); //9 scan,11 sq
                string sq = strs[sq_index];
                sq = sq.Replace('L', 'I');
                string scan_sq = strs[raw_index] + "@" + strs[scan_index] + "@" + sq; //raw@scan@sq
                max_str.Add(scan_sq);
            }
            sr_max.Close();

            string pfind_task_path = all_tasks_lv.Items[0] as string;
            Task task = new Task(pfind_task_path, "pTop");
            StreamReader sr_find = new StreamReader(task.identification_file);
            while (!sr_find.EndOfStream)
            {
                line = sr_find.ReadLine();
                if (line.Trim() == "")
                    continue;
                string[] strs = line.Split('\t'); //1 scan, 5 sq
                string sq = strs[5];
                string title = strs[0];
                string[] strs2 = title.Split('.');
                sq = sq.Replace('L', 'I');
                string scan_sq = strs2[0] + "@" + strs[1] + "@" + sq;
                double q_value = double.Parse(strs[4]);
                if (q_value > 0.01)
                    continue;
                find_str.Add(scan_sq);
            }
            sr_find.Close();

            max_str.Sort();
            find_str.Sort();

            int same_num = 0;
            int mi = 0, fi = 0;
            while (mi < max_str.Count && fi < find_str.Count)
            {
                if (string.Compare(max_str[mi], find_str[fi]) < 0)
                {
                    sw_notSame.WriteLine(max_str[mi]);
                    ++mi;
                }
                else if (string.Compare(max_str[mi], find_str[fi]) > 0)
                    ++fi;
                else
                {
                    sw_same.WriteLine(max_str[mi]);
                    ++same_num;
                    ++mi;
                    ++fi;
                }
            }
            sw_same.Flush();
            sw_same.Close();
            sw_notSame.Flush();
            sw_notSame.Close();
            string msg = "Same: " + same_num;
            msg += "\r\nMaxQuant: " + max_str.Count;
            msg += "\r\npFind 3.0: " + find_str.Count;
            List<int> numbers = new List<int>();
            numbers.Add(find_str.Count);
            numbers.Add(max_str.Count);
            numbers.Add(same_num);
            Display_Help dh = new Display_Help();
            //有两个任务，有多个RAW的结果，每个RAW的结果都有一张图
            PlotModel model = dh.display_compare_result(numbers);
            Plot oxyplot = new Plot();
            oxyplot.Model = model;
            oxyplot.Width = 400;
            oxyplot.Height = 400;
            this.compare_result_sp.Children.Add(oxyplot);
            MessageBox.Show(msg);
        }
    }
}
