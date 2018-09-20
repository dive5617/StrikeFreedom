using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pFind.classes
{
    class Message_Help
    {
        public static string DOT_NET_ERROR = "Please make sure that you have correctly installed .NET Framework 4.5 or higher.";

        public static string LICENSE_WRONG = "The license is invalid.";
        public static string ALL_NOT_NULL = "Please fill in all required fields."; // "All input must not be empty."
        public static string EMAIL_WRONG = "The email address must contain '@'";
        public static string COPY_OK = "Please paste (Ctrl+V) the registration information into the body of your e-mail and send it to pfind@ict.ac.cn.";
        public static string THREAD_NUM_WARNING = "* The number of threads is greater than the number of cores, which may degrade the performance.";

        public static string NEW_TASK_FAIL = "Sorry,some key files were missing.\nPlease reinstall the software.";

        //文件读取失败
        public static string READ_LABEL_FAILURE = "";
        public static string READ_MOD_FAILURE = "";
        public static string READ_DB_FAILURE = "";
        public static string READ_ENZYME_FAILURE = "";
        public static string FOLDER_NOT_EXIST = "The task does not exist!";
        
        //Identification面板
        public static string PEP_MASS_RANGE = "The upper bound of the peptide mass cannot be less than its lower bound.\n And please ensure the upper bound is above 0.";
        public static string PEP_LEN_RANGE = "The upper bound of the peptide length cannot be less than its lower bound.";
        public static string INVALID_INPUT = "Please check your input.";
        public static string INPUT_EMPTY = "The input cannot be empty.";
        public static string DataBase_EMPTY = "Please choose a database before running.";
        
        //before call pBuild
        public static string RESULT_FILE_OK = "Sorry, the resulting files are incomplete. Please make sure that the path is valid and the task has been successfully accomplished.";
    
        //参数检查
        public static string VALUE_ABOVE_ZERO = "The value cannot be less than 0.";

        //
        public static string NONE_QUANTITATION = "No ms1 for quantitation.";
        //分布执行
        public static string NO_PFIND_SPECTRA = "There is no identification result for quantitation. Please run from identification";

        public static string NO_FILTER_FILE = "There is no search result for filter. Please run from identification.";

        public static string NO_EDIT_WHEN_RUN = "Sorry, you can't edit the task when it is running.";

        public static string DATE_FILE_NOT_EXIST = "Sorry, some key files were missing.\nPlease reinstall the software.";
       
        public static string DATE_FILE_INVALID = "Sorry, some key files were invalid. \nPlease reinstall the software.";

        public static string DATE_OVER = "Software has expired, please download and install it again.";
    }
}
