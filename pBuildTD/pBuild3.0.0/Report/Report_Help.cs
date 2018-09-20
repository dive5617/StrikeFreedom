using OxyPlot;
using OxyPlot.Wpf;
using Spire.Doc;
using Spire.Doc.Documents;
using Spire.Doc.Fields;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;


namespace pBuild.Report
{
    public class Report_Help
    {
        public MainWindow mainW;
        public Report_Help(MainWindow mainW)
        {
            this.mainW = mainW;
        }
        private void report_save_png(string filename, PlotModel model)
        {
            try
            {
                using (var stream = File.Create(filename))
                {
                    var pngExporter = new PngExporter();
                    pngExporter.Export(model, stream);
                }
            }
            catch (Exception exe)
            {
                System.Windows.MessageBox.Show(exe.ToString());
            }
        }
        private List<string> report_image()
        {
            List<string> all_pngs = new List<string>();
            string folder = mainW.task.folder_result_path + "\\" + File_Help.pBuild_tmp_file;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            string filename1 = folder + "\\mixed_spectra.png";
            all_pngs.Add(filename1);
            report_save_png(filename1, mainW.Model_Mixed_Spectra);
            string filename2 = folder + "\\specific.png";
            all_pngs.Add(filename2);
            report_save_png(filename2, mainW.Model_Specific);
            string filename3 = folder + "\\modification.png";
            all_pngs.Add(filename3);
            report_save_png(filename3, mainW.Model_Modification);
            string filename4 = folder + "\\length.png";
            all_pngs.Add(filename4);
            report_save_png(filename4, mainW.Model_Length);
            string filename5 = folder + "\\raw_rate.png";
            all_pngs.Add(filename5);
            report_save_png(filename5, mainW.Model_RawRate);
            if (mainW.task.quantification_file != "")
            {
                string filename6 = folder + "\\quantification.png";
                all_pngs.Add(filename6);
                report_save_png(filename6, mainW.Model_Quantification);
            }
            return all_pngs;
        }
        public void report_word()
        {
            mainW.initial_Protein();
            List<string> all_pngs = report_image();
            string information = "Dear users of pFind3, you search $ raw file(s) in this run, there are $ ms2 scans. In your searching and filter parameters($), pFind3 report $ credible psms, corresponding to $ scans, raw rate is $. Graph $ shows relevant details. This searching result shows $ peptides and $ proteins.";
            //"尊敬的pFind用户，您这次搜索了$个raw文件，共有二级谱$张。在您设置的搜索、过滤参数$下，pFind3报告可信的肽谱匹配$个，对应二级谱$张，解析率为$。图$展示了相关细节。该搜索结果，对应肽段序列$条，蛋白质$个。"
            string[] args = new string[10];
            args[0] = (mainW.summary_result_information.raw_rates.Count - 1) + "";
            args[1] = mainW.summary_result_information.scans_number.ToString("N0");
            args[2] = "FDR ≤ " + Config_Help.fdr_value.ToString("F2");
            args[3] = mainW.summary_result_information.spectra_number.ToString("N0");
            args[4] = args[1];
            args[5] = mainW.summary_result_information.raw_rates.Last().Rate.ToString("P2");
            args[6] = " 1 - " + all_pngs.Count + " "; //
            args[7] = mainW.summary_result_information.peptides_number.ToString("N0");
            args[8] = mainW.protein_panel.identification_proteins.Count.ToString("N0");
            args[9] = " 1 - 2 ";
            information = Config_Help.getReport(information, args);
            
            string folder = mainW.task.folder_result_path + "\\" + File_Help.pBuild_tmp_file;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            try
            {
                Document document = new Document();
                ParagraphStyle style = new ParagraphStyle(document);
                style.Name = "FontStyle";
                style.CharacterFormat.FontSize = 12;
                document.Styles.Add(style);
                ParagraphStyle style2 = new ParagraphStyle(document);
                style2.Name = "FontStyle2";
                style2.CharacterFormat.FontSize = 14;
                document.Styles.Add(style2);
                Section s = document.AddSection();
                Paragraph p = s.AddParagraph();
                p.ApplyStyle(style2.Name);
                p.AppendText(information);
                for (int i = 0; i < all_pngs.Count; ++i)
                {
                    string flag = "";
                    switch (i)
                    {
                        case 0:
                            flag = "Mixed spectra percentage"; //混合谱图比例
                            break;
                        case 1:
                            flag = "Cleavage percentage"; //酶切情况
                            break;
                        case 2:
                            flag = "Modification percentage"; //修饰情况
                            break;
                        case 3:
                            flag = "Peptide length percentage"; //肽段长度比例
                            break;
                        case 4:
                            flag = "Id rate"; //谱图解析率
                            break;
                        case 5:
                            flag = "Quantification ratio distribution"; //定量比值分布图
                            break;
                    }
                    p = s.AddParagraph();
                    p.ApplyStyle(style.Name);
                    p.Format.HorizontalAlignment = Spire.Doc.Documents.HorizontalAlignment.Center;
                    p.AppendText("Graph " + (i + 1) + " ：" + flag);
                    DocPicture pic = p.AppendPicture(Image.FromFile(all_pngs[i])); //DocPicture pic = 
                    pic.HorizontalAlignment = ShapeHorizontalAlignment.Center;
                    //pic.Width = 450;
                    //pic.Height = 468;

                }
                document.SaveToFile(mainW.task.folder_result_path + "\\" + File_Help.pBuild_tmp_file + "\\report.docx", FileFormat.Docx);
                System.Diagnostics.Process.Start(mainW.task.folder_result_path + "\\" + File_Help.pBuild_tmp_file + "\\report.docx");
            }
            catch (Exception exe)
            {
                System.Windows.MessageBox.Show(exe.ToString());
            }
        }
    }
}
