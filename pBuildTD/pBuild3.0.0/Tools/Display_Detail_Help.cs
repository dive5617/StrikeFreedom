using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    //全部为屏幕坐标
    public class Display_Detail_Help
    {
        public double X_Delta_SQ;
        public double Y_Delta_Interval;
        public double B_Delta_Interval;
        public double FontHeight_SQ;
        public double FontHeight_BY;
        public double FontWidth_SQ;
        public double LineWidth;
        public double Delta_Width;
        public double BY_Mod_Width;

        public double ME_Weight;

        //一级谱图中右上角的边框的颜色
        public static OxyColor border_color = OxyColor.FromArgb(70, OxyColors.Black.R, OxyColors.Black.G, OxyColors.Black.B);

        //左下角的屏幕坐标系
        public ScreenPoint LeftBottomPoint;

        //SQ 和 BY的字体大小和字体风格
        public double FontSize_SQ;
        public double FontSize_BY;
        public double FontSize_BY_NUM;
        public string Font_SQ;
        public string Font_BY;

        //标记的by[M]及内部离子的颜色
        public OxyColor A_Match_Color;
        public OxyColor B_Match_Color;
        public OxyColor C_Match_Color;
        public OxyColor X_Match_Color;
        public OxyColor Y_Match_Color;
        public OxyColor Z_Match_Color;
        public OxyColor M_Match_Color; //母离子匹配颜色
        public OxyColor I_Match_Color; //内部离子匹配颜色
        public OxyColor Lose_Match_Color; //中性丢失的颜色
        public OxyColor O_Match_Color; //亚氨离子匹配颜色

        //阶梯图的高度，一般显示混合谱，阶梯图有几行，可以把高度调小些
        // [!!!] modified by wrm. @2016.03.22
        public static double Ladder_Height = 0.09 , Old_Ladder_Height = 0.09;
        public static int pTop_Ladder_Count = 3;

        //匹配和
        public double Match_StrokeWidth;
        public double NoNMatch_StrokeWidth;

        //二级谱图及一级谱图的标注颜色
        public static OxyColor title_color = OxyColors.Black;
        public static OxyColor value_color = OxyColors.Blue;
        public static string font_type = "Calibri";

        public Display_Detail_Help()
        {
            this.LeftBottomPoint = new ScreenPoint();
            BY_Mod_Width = 0.0;
            Font_SQ = font_type; //"Arial Unicode MS"
            Font_BY = font_type; //"Arial Unicode MS"

            FontSize_SQ = 0;
            FontSize_BY = 0;

            B_Match_Color = OxyColors.Green;
            A_Match_Color = OxyColor.FromArgb(B_Match_Color.A, (byte)(B_Match_Color.R >> 1),
                (byte)(B_Match_Color.G >> 1), (byte)(B_Match_Color.B >> 1));
            C_Match_Color = OxyColor.FromArgb(B_Match_Color.A, (byte)(B_Match_Color.R << 1),
                (byte)(B_Match_Color.G << 1), (byte)(B_Match_Color.B << 1));
            Y_Match_Color = OxyColor.FromArgb(255, 202, 29, 82); //血红色
            X_Match_Color = OxyColor.FromArgb(Y_Match_Color.A, (byte)(Y_Match_Color.R >> 1),
                (byte)(Y_Match_Color.G >> 1), (byte)(Y_Match_Color.B >> 1));
            Z_Match_Color = OxyColor.FromArgb(Y_Match_Color.A, (byte)(Y_Match_Color.R << 1),
                (byte)(Y_Match_Color.G << 1), (byte)(Y_Match_Color.B << 1));
            M_Match_Color = OxyColors.Gray;
            I_Match_Color = OxyColors.DarkOrange;

            Lose_Match_Color = OxyColors.Blue;

            O_Match_Color = OxyColors.YellowGreen;

            Match_StrokeWidth = 3.0;
            NoNMatch_StrokeWidth = Match_StrokeWidth - 1.0;
            ME_Weight = 2.0;
        }
        public void update_Mix(int mix_number) //如果显示混合谱，字体大小都会等比例下降
        {
            this.FontSize_SQ /= mix_number;
            this.FontSize_BY /= mix_number;
            this.FontSize_BY_NUM /= mix_number;
        }
    }
}
