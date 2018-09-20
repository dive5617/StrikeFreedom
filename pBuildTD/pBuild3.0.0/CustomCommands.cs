using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace pBuild
{
    public class CustomCommands
    {
        public static RoutedCommand DoF5 = new RoutedCommand(); //F5重新导入当前任务
        public static RoutedCommand DoLoad = new RoutedCommand();
        public static RoutedCommand DoExportPDF_Summary = new RoutedCommand();
        public static RoutedCommand DoExportPDF_Result = new RoutedCommand();
        public static RoutedCommand DoExportPDF_Param = new RoutedCommand();
        public static RoutedCommand DoExportPDF_Graph = new RoutedCommand();
        public static RoutedCommand DoHelp_About = new RoutedCommand();
        public static RoutedCommand DoHelp_Compare = new RoutedCommand();
        public static RoutedCommand DoHelp_Compare2 = new RoutedCommand();
        public static RoutedCommand DoSwitch_To_StartPage = new RoutedCommand();
        public static RoutedCommand DoExportCliboard_PDF = new RoutedCommand();
        public static RoutedCommand DoExportCliboard_Da = new RoutedCommand();
        public static RoutedCommand DoExportCliboard_PPM = new RoutedCommand();
        public static RoutedCommand DoExportCliboard_Score = new RoutedCommand();
        public static RoutedCommand DoExportCliboard_Mix = new RoutedCommand();
        public static RoutedCommand DoExportCliboard_Specific = new RoutedCommand();
        public static RoutedCommand DoExportCliboard_Modification = new RoutedCommand();
        public static RoutedCommand DoExportCliboard_Length = new RoutedCommand();
        public static RoutedCommand DoExportCliboard_Raw_Rate = new RoutedCommand();
        public static RoutedCommand DoExportCliboard_Ratio = new RoutedCommand();
    }
}
