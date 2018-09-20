using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pConfig
{
    public class Message_Helper
    {
        //数据库的信息
        public static string DB_NAME_PATH_NULL_Message = "Both name and path must be not null.";
        public static string DB_PATH_NOT_EXIST_Message = "The path does not exist!";
        public static string DB_CONTAINMENT_PATH_Message = "A new database has been created: ";
        
        //修饰的信息
        public static string MO_INPUT_WRONG_Message = "Input Wrong.";
        public static string MO_AA_REPEAT = "The Amino Acid repeats. Or the site is wrong. It must be A-Z.";

        //定量的信息
        public static string QU_NAME_NULL_Message = "Quantification name must not be null.";
        public static string QU_AA_A_TO_Z_Message = "The Amino name must be * or A-Z.";
        public static string QU_LABEL0_NAME_Message = "The Label0's name must be one of elements' name.";
        public static string QU_LABEL1_NAME_Message = "The Label1's name must be one of elements' name.";
        public static string QU_NONE_NOT_EDIT = "The 'None' item cannot edit.";
        public static string QU_N15_NOT_EDIT = "The 'N15' item cannot edit";
        public static string QU_NONE_NOT_DELETE = "The 'None' item cannot delete.";
        public static string QU_N15_NOT_DELETE = "The 'N15' item cannot delete.";
        //酶的信息
        public static string EN_CLEAVE_A_TO_Z_Message = "Cleave must be A-Z";
        public static string EN_CLEAVE_NULL_Message = "Cleave must not be null";
        public static string EN_IGNORE_A_TO_Z_Message = "Ignore must be A-Z";
        //氨基酸的信息
        public static string AA_Selected_NULL_Message = "Please select one amino acid!";
        //元素的信息
        public static string EL_MASS_RATIO_DOUBLE_Message = "Both mass and ratio must be double";
        public static string EL_SUM_RATIO_ONE_Message = "The Sum of ratio must be 1.0";
        //元素组成的信息
        public static string EE_NULL_Message = "Null element!";
        public static string EE_INPUT_NUMBER_Message = "Please input a number";
        public static string EE_CANNOT_DELETE = "You can not delete the \"default\" elements.";
        //其它
        public static string Tab_Saved_Prompt = "Some Tabs are not saved, click OK to save all.";
        public static string NAME_IS_USED_Message = "The name is used!";
        public static string NAME_WRONG = "The name must not contain such character: #,{,}.";
        public static string ADMINISTRATOR_Message = "You must run with administrator privileges.";
    }
}
