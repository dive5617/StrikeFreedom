using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class Ladder_Help
    {
        //MS2
        public static double Scale_Width = 0.0; //scale*(max_mz-min_mz)
        public static double Width = 0.0; //The screen width of the model

        public static double Scale_Height = 0.0;
        public static double Height = 0.0;

        public static void Initial()
        {
            Scale_Width = 0.0;
            Width = 0.0;
            Scale_Height = 0.0;
            Height = 0.0;
        }

    }
}
