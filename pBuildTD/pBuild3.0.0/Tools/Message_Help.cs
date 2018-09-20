using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pBuild
{
    public class Message_Help
    {
        public static string FOLDER_NOT_EXIST = "This folder does not exist!";
        public static string PATH_INVALID = "Invalid Path";
        public static string TSK_FOLDER_NOT_EXIST = "The task path does not contain tsk file.";
        public static string TASK_ALREADY_LOAD = "This task you want to add is already open.";
        public static string PDF_DELETE_WRONG = "Please close the pdf files!";
        public static string TASK_DELETE_PROMPT = "If you delete this task folder, all data in this folder will lost.";
        public static string TASK_DELETE_REMOVE_WRONG = "You have opened the folder or sub-folder! Please close them!";
        public static string FDR_NOT_MORE_5 = "The FDR value must be ≤ 5.00%";
        public static string FDR_BE_NUMBER = "The FDR value must be a number!";
        public static string FDR_END_BY_PERCENT = "Please add a '%' in the end of FDR value!";
        public static string PDF_OPEN_WRONG = "Error!!! Pdf is opened! Please close this pdf";
        public static string MODEL_OPEN_WRONG = "There is no picture! Please select one PSM.";
        public static string SCAN_IS_FIRST = "It is the first scan!";
        public static string SCAN_IS_LAST = "It is the last scan!";
        public static string FILTER_OK = "Filter is done!";
        public static string MIXED_NUMBER_BE_INTEGER = "Mixed number must be a positive integer!";
        public static string RATIO_NUMBER_BE_DOUBLE = "Ratio number must be a positive double!";
        public static string NULL_SELECT_PSM = "There is no picture! Please select one PSM.";
        public static string MZ_BE_DOUBLE = "The minimum or maximum m/z is not a double";
        public static string MZ_INPUT_WRONG = "Wrong input! The m/z is too large or too small.";
        public static string SHOW_GROUP_PROTEIN_WRONG = "The selected protein is a group protein.";
        public static string FASTA_PATH_NOT_EXIST0 = "The fasta path does not exist in ";
        public static string FASTA_PATH_NOT_EQUAL = "The fasta path is invalid.";
        public static string FASTA_PATH_NOT_EXIST1 = "Please select your fasta path.";
        public static string RAW_PATH_NOT_EXIST0 = "The raw(s) folder path does not exist in ";
        public static string RAW_PATH_NOT＿EXIST1 = "Please select your raw(s) folder path.";
        public static string LICENSE_WRONG = "The license is invalid.";
        public static string ALL_NOT_NULL = "Please fill in all required fields.";
        public static string EMAIL_WRONG = "The email address must contain '@'";
        public static string COPY_OK = "Please paste (Ctrl+V) the registration information into the body of your email and send it to pfind@ict.ac.cn.";
        public static string NO_EMAIL_CLIENT = "The email client is not found. Please click the right button and send the email manually.";
        public static string PROTEIN_AA_LENGTH = "This number must be ≥ 40";
        public static string PROTEIN_AA_LENGTH_INTEGER = "This number must be a integer!";
        public static string TASK_COMPARE_WRONG = "The number of tasks must be 2 or 3.";
        public static string TASK_NOT_EQUAL = "The raw(s) of tasks you want to compare are not consistent. Are you sure to compare?";
        public static string NO_CHANGE = "Not changed.";
        public static string Quant_Wrong = "The \"pQuant.spectra\" file has something wrong.";
        public static string NO_RATIO = "No ratios.";
        public static string ALL_TERM_BE_DOUBLE = "All Mass Terms must be positive double.";
        public static string ALL_AA_WRONG = "All AA Terms must be 'A-Z'.";
        public static string NO_DATA = "You searched the MGF file(s), so there is no MS1 data. Please don't switch to MS1 Tab or Chrom Tab";
        public static string INDEX_OVER = "pFind only outputs ";
        public static string INDEX_OVER2 = " candidate PSMs.";
        public static string DATE_FILE_NOT_EXIST = "Sorry, some key files were missing.\nPlease reinstall the software.";
        public static string DATE_FILE_INVALID = "Sorry, some key files were invalid. \nPlease reinstall the software.";
        public static string DATE_OVER = "Sorry, pBuild has expired.\nPlease reinstall the software.";
        public static string CLIPBOARD_ERROR = "Copy to clipboard has something error, please operate again.";
        public static string OTHER_CHECKED_ERROR = "The \"Other\" checked box has been clicked. The software has no function. Please switch off \"Other\" checked box.";
        public static string PROTEIN_PEP_NUMBER = "The peptide number must be integer.";

        public static string LOAD_INI_START = "Reading ini files...";
        public static string LOAD_PSMS_START = "Reading PSMs file...";
        public static string LOAD_SS_START = "Writing dat file...";
        public static string LOAD_PARSE_QUANT_START = "Parsing quantification file...";
        public static string LOAD_RATIO_START = "Reading ratio...";
        public static string LOAD_CAND_START = "Reading candidates peptides...";
        public static string LOAD_OK = "Completed";
    }
}
