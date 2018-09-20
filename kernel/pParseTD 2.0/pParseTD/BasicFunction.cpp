#include <ctime>
#include <cstring>
#include <iostream>
#include <sstream>
#include <fstream>
#include <list>

#include "BasicFunction.h"
#include "StringProcessor.h"
#include "MonoCandidates.h"
#include "dirent.h" /* Attention: If you use visual studio cl.exe compiler, use this header file */
                    /* else if you use gcc compiler, use this header file #include <dirent.h> */
using namespace std;

/*
**	class CErrInfo
*/
CErrInfo::CErrInfo(const string &strClass, const string &strMethod,
		const string &strDetail) 
{
	m_strClass = strClass;
	m_strMethod = strMethod;
	m_strDetail = strDetail;
}

CErrInfo::CErrInfo(const string &strClass, const string &strMethod,
		const string &strDetail, const exception & e) 
{
	m_strClass = strClass;
	m_strMethod = strMethod;
	m_strDetail = strDetail;
	SetException(e);
}
void CErrInfo::Append(const string &strInfo) 
{
	if (strInfo.empty())
		return;
	else
		m_strInfo += "\t\t  " + strInfo+"\n";
}

string CErrInfo::Get() const 
{
	string strError = m_strException;
	strError += "\t  at " + m_strClass + "::" + m_strMethod + "() "
			+ m_strDetail + "\n";
	strError += m_strInfo;
	return strError;
}

string CErrInfo::Get(const exception& e) 
{
	SetException(e);
	return Get();
}
void CErrInfo::SetException(const exception & e) 
{
	m_strException = e.what();
}

ofstream& operator<< ( ofstream& os, const CErrInfo& info)
{
	os << endl << "==========================" << endl;
	time_t current_time;
	time(&current_time);
	os << ctime(&current_time) << endl;
	os << info.Get() << endl;
	return os;
}
ostream& operator<< ( ostream& os, const CErrInfo& info)
{
	os << endl << "==========================" << endl;
	time_t current_time;
	time(&current_time);
	os << ctime(&current_time) << endl;
	os << info.Get() << endl;
	return os;
}

/*
**	class CWelcomeInfo
*/
void CWelcomeInfo::PrintLogo()
{
	cout <<endl;
	cout << "****************************************" << endl;
	cout << "*                                      *" << endl;
	cout << "*       Welcome to use pParseTD!       *" << endl;
	cout << "*                                      *" << endl;
	cout << "*            " << Version << "             *" << endl;
	cout << "*        " << TIMESTRING << "                *"<<endl;
	cout << "*                                      *" << endl;
	cout << "*        http://pfind.ict.ac.cn        *" << endl;
	cout << "*                                      *" << endl;
	cout << "****************************************" << endl;
}

// 过期时间为年底
bool CWelcomeInfo::CheckDate()
{
	tm exptm;
	exptm.tm_year = ExpireYear - 1900;   /* years since 1900 */
	exptm.tm_mon = ExpireMonth - 1;     /* months since January - [0,11] */
	exptm.tm_mday = ExpireDay;
	exptm.tm_hour = 0;
	exptm.tm_min = 0;
	exptm.tm_sec = 0;
	if( mktime(&exptm) < time(0) )
	{
		return false;
	}
	return true;
}

/*
**	class CHelpInfo
*/
//显示版本历史
void CHelpInfo::WhatsNew()
{
	cout << "What's New:" << endl;
	cout << "\t[2014.11.26] pParseTD 1.2.0 was available" << endl;
	cout << "Both SVM and MARS are supported!" << endl;
	cout << "Changed the deconvolution part" << endl;
}
//打印版本号，并显示更新内容
void CHelpInfo::PrintVersion()
{
	cout << " pParse Version" << Version <<endl<<endl;;
	WhatsNew();
}
//显示使用方法
void CHelpInfo::DisplayUsage()
{
	cout << "[pParseTD] " << "The software will expire on " << ExpireDay << "/" << ExpireMonth << "/" << ExpireYear << endl;
	cout << "Usage via config files:"<< endl;
	cout << "\tpParseTD.exe pParseTD.cfg" << endl << endl;
	cout << "\tCarefully configured parameter file is required!" << endl;
	cout <<"\tA standard parameter file named 'pParseTD.cfg' is generating in your current folder." << endl << endl;
	GeneratedParamTemplate();
	DisplayCMDUsage();
}

//显示命令行使用帮助
void CHelpInfo::DisplayCMDUsage()
{
	cout << "Usage via command options:" <<endl <<endl;
	cout << "\tpParseTD.exe -D D:\\Dataset\\1.raw -F raw [options]" <<endl <<endl;
	cout << "Or" <<endl<<endl;
	cout << "\tpParseTD.exe -D D:\\Dataset\\ -F path [options]" <<endl <<endl;
	cout << "More Options:" <<endl;
	cout << "\t-D datapath               path of input file (default blank)" <<endl;
	cout << "\t-F input_format           raw or ms1 or path (default raw) " <<endl;
	cout << "\t-O outputpath             path of output file (default the same path with datapath)" <<endl;

	cout << "\t-C co-elute               0 or 1 (default 1)" <<endl;
	cout << "\t-W isolation_width        5-10 (default 10) " <<endl;
	cout << "\t-r rewrite_files          0 or 1 (default 0)" <<endl;
	cout << "\t-M model_type             SVM or MARS (default SVM)" <<endl;
	cout << "\t-t mars_threshold         [-1, 1] (default -0.68) " <<endl;
	cout << "\t-I ipv_file               path of IPV file (default .\\LIPV.txt) " <<endl;
	cout << "\t-T trainingset            path of TrainSet (default .\\TrainingSet.txt)" <<endl;

	cout << "\t-d delete_msn             0 or 1 (default 0) " <<endl;
	cout << "\t-m output_mgf             0 or 1 (default 1) " <<endl;
	cout << "\t-a check_activationcenter 0 or 1 (default 1)" <<endl; 
	cout << "\t-S cut_similiar_mono      0 or 1 (default 1)" <<endl;
	cout << "\t-s output_trainingdata    0 or 1 (default 0) " <<endl;
	cout << "\t-R recalibrate_window     (default 7) " <<endl;

	cout << "\t-c max_charge			(default 30)" << endl;
	cout << "\t-n SN_threshold			(defalut 1.5)" << endl;
	cout << "\t-e mz_error_tolerance	(default 20 ppm)" << endl;
}

//生成参数文件
void CHelpInfo::GeneratedParamTemplate()
{
	FILE *pfile = fopen("pParseTD.cfg", "r");
	if (NULL != pfile)
	{
		fclose(pfile);
		return;
	}
	pfile = fopen("pParseTD.cfg", "w");
	fprintf(pfile,"# pParseTD.cfg template\n");
	fprintf(pfile,"# For help: mail to luolan@ict.ac.cn \n");
	fprintf(pfile,"# Time: 2015.03.06\n\n");

	fprintf(pfile,"[Basic Options]\n");

	fprintf(pfile,"datapath=\n");
	fprintf(pfile,"input_format=raw\n");
	fprintf(pfile,"# raw / ms1 / path\n");
	fprintf(pfile,"outputpath=\n");

	fprintf(pfile, "\n");

	fprintf(pfile,"[Advanced Options]\n");

	fprintf(pfile,"co-elute=1\n");
	fprintf(pfile,"# 0, output single precursor for single scan;\n");
	fprintf(pfile,"# 1, output all co-eluted precursors.\n");
	
	fprintf(pfile,"isolation_width=10\n");

	fprintf(pfile,"rewrite_files=0\n");
	//fprintf(pfile,"# 0 Not rewrite file if exist; 1 Rewrite files anyway.\n");

	fprintf(pfile,"model_type=SVM\n");
	fprintf(pfile,"# SVM or MARS\n");
	fprintf(pfile,"mars_threshold=-0.68\n");
	//fprintf(pfile,"# Be careful with this. Never try to revise this one.\n");

	fprintf(pfile,"ipv_file=.\\LIPV.txt\n");
	fprintf(pfile,"trainingset=.\\TrainingSet.txt\n");

	fprintf(pfile, "\n");

	fprintf(pfile,"[Internal Switches]\n");

	fprintf(pfile,"delete_msn=0\n");
	//fprintf(pfile,"# 0 Keep the MS1/MS2 files [recommended]\n");
	//fprintf(pfile,"# 1 Delete the MS1/MS2 files\n");
	fprintf(pfile,"output_mgf=1\n");
	//fprintf(pfile,"# 0 Do Not output MGF; 1 output MGF\n");
	fprintf(pfile,"check_activationcenter=1\n");
	//fprintf(pfile,"# 0 Do Not check; 1 Check(Recommended).\n");
	
	fprintf(pfile,"cut_similiar_mono=1\n");
	//fprintf(pfile,"# Do not change this usually.\n");
	//fprintf(pfile,"# 1 Keep one precursor in 20ppm with same charge\n");
	//fprintf(pfile,"# 0 Keep all precursors in 20ppm width window.\n");

	fprintf(pfile,"output_trainingdata=0\n");
	//fprintf(pfile,"# 0, Not output the Training Sample\n");
	//fprintf(pfile,"# 1, Output Training Sample\n");
	
	fprintf(pfile,"recalibrate_window=7\n");
	//fprintf(pfile,"# Error tolerance for peak detection between consecutive survy scan.");

	fprintf(pfile, "max_charge=30\n");
	fprintf(pfile, "SN_threshold=1.5\n");
	fprintf(pfile, "mz_error_tolerance=20\n");

	fclose(pfile);
}

/*
**	class CParaProcess
*/
CParaProcess::CParaProcess()
{
	InitiParaMap();
}
CParaProcess::~CParaProcess()
{
	m_mapPara.clear();
}

//初始化默认参数，存储在map中
void CParaProcess::InitiParaMap()
{
	m_mapPara["datapath"] = "";   //
	m_mapPara["input_format"] = "raw";    //
	m_mapPara["logfilepath"] = "EmptyPath";
	m_mapPara["outputpath"] = "EmptyPath";

	m_mapPara["label"] = "none";  //@added by wrm. 2016.10.30

	m_mapPara["co-elute"] = "1";
	m_mapPara["isolation_width"] = "10";   // 隔离窗口
	m_mapPara["rewrite_files"] = "0";
	m_mapPara["model_type"] = "SVM";
	m_mapPara["mars_threshold"] = "-0.15";
	m_mapPara["ipv_file"] = ".\\LIPV.txt";
	m_mapPara["trainingset"] = ".\\TrainingSet.txt";

	m_mapPara["delete_msn"] ="0";
	m_mapPara["output_mgf"] = "1";	
	m_mapPara["check_activationcenter"] = "1";	
	m_mapPara["cut_similiar_mono"] = "1";
	m_mapPara["output_trainingdata"] = "0";
	m_mapPara["recalibrate_window"] = "7";

	m_mapPara["max_charge"] = "30";     //
	m_mapPara["SN_threshold"] = "1.5";  // 信噪比
	m_mapPara["mz_error_tolerance"] = "20";   // 
}
//显示参数map
void CParaProcess::DisplayPara()
{
	int paracount  = 1;
	string parafile = m_mapPara["outputpath"];
	string strtime = GetTimeStr();
	parafile += "pParseTD_" + strtime + ".para";
	//cout << parafile << endl;
	try
	{
		ofstream ofs;
		ofs.open(parafile.c_str(), ofstream::out);

		unordered_map<string, string>::iterator it;
		ofs << "[Para] Start of Parameters" << endl;
		for (it = m_mapPara.begin(); it != m_mapPara.end(); ++it)
		{
			ofs << paracount++ << ": " << it->first << "=" << it->second << endl;
		}
		ofs << "[Para] End of Parameters" << endl;
		ofs.close();
	}
	catch(...) // 输出本次解析设置的参数，一旦出错不影响其他过程，所以不报错。
	{
		cout << "[Warning] Display Para failed." << endl;
	}
}

//判断输入路径是否合法，借用dirent.h中的opendir()函数
bool CParaProcess::_isPath(string &strpath)
{
	DIR *p = opendir(strpath.c_str());
	if (p == NULL)
	{
		return false; 
	} else {
		closedir(p);
		return true;
	}
}
//检查参数文件中路径参数是否合法
void CParaProcess::CheckParam()
{
	string DataPath = m_mapPara["datapath"];//map的访问是log(n)的，虽然这里n很小，还是应该尽量少访问
	size_t len = DataPath.size();
	if(0 == len)
	{
		cout << "[Error] Datapath is empty." << endl;
		cout << "Please Input full path of the data, we only supports path or .RAW or .MS1" << endl;
		exit(1);
	}

	string outFile = m_mapPara["outputpath"];
	if(0 == outFile.length())
	{
		outFile = "EmptyPath";
	}

	if (_isPath(DataPath))
	{
		if (DataPath[len - 1] != '\\')
		{
			DataPath.push_back('\\');
		}
		m_mapPara["datapath"] = DataPath;
		if (outFile.compare("EmptyPath") == 0)
		{
			m_mapPara["outputpath"] = DataPath;
		} else if (outFile[(int)outFile.length() - 1] != '\\')
		{
			m_mapPara["outputpath"].push_back('\\');
		}
	} else {
		size_t found = DataPath.find_last_of(".");
		if(found == string::npos)
		{
			cout << "[Error] Invalid datapath 1: " << DataPath << endl;
			cout << "Please Input full path of the data, we only supports path or .RAW or .MS1" << endl;
			exit(1);
		}
		string fileExt = DataPath.substr(found);
		CStringProcess::ToLower(fileExt);
		//cout << fileExt << endl;

		if(fileExt.compare(".raw") == 0 || fileExt.compare(".ms1") == 0)
		{
			found = DataPath.find_last_of("/\\");
			if (outFile.compare("EmptyPath") == 0)
			{
				m_mapPara["outputpath"] = DataPath.substr(0, found + 1);;
			} else if (outFile[(int)outFile.length() - 1] != '\\') 
			{
				m_mapPara["outputpath"].push_back('\\');
			}
		} else {
			cout << "[Error] Invalid datapath : " << DataPath << endl;
			cout << "Please Input full path of the data, we only support path or .RAW or .MS1" << endl;
			exit(1);
		}
	}

	string logFile = m_mapPara["logfilepath"];
	if(0 == logFile.length())
	{
		logFile = "EmptyPath";
	}
	if (logFile.compare("EmptyPath") == 0)
	{
		m_mapPara["logfilepath"] = m_mapPara["outputpath"];
	} else if (logFile[(int)logFile.length() - 1] != '\\'){
		m_mapPara["logfilepath"].push_back('\\');
	}
}

//解析参数ini文件忽略注释，参数值填入m_mapPara
void CParaProcess::GetFilePara(string &filename)
{
	ifstream fin;
	string ss;
	fin.open(filename.c_str(), ios::in);
	if (!fin.good()) 
	{
		cout << "[Error] Cannot open the configure file: " << filename << endl;
		exit(1);
	}
	while (!fin.eof())
	{
		getline(fin, ss);
		if (ss == "" || ss[0] == '#' || ss[0] == ';' || ss[0] == '[')
		{
			continue;
		}
		size_t pos = ss.find('=');
		string forward = ss.substr(0, pos);
		string backward = "";
		if(pos != string::npos)
		{
			backward = ss.substr(pos + 1, ss.size() - pos);
		}
		
		m_mapPara[forward] = backward;
	}
	/*try
	{
		CheckParam();
	}
	catch(exception & e)
	{
		//CErrInfo info("CParaProcess", "GetFilePara", "CheckParam failed.");
		//info.Append("configPath = " + filename);
		//throw runtime_error(info.Get(e).c_str());
		throw ("CParaProcess::GetFilePara() CheckParam failed.");
	}
	catch(...)
	{
		//CErrInfo info("CParaProcess", "GetFilePara", "Caught an unknown exception from CheckParam().");
		//info.Append("configPath = " + filename);
		//throw runtime_error(info.Get().c_str());
		throw ("CParaProcess::GetFilePara() Caught an unknown exception from CheckParam().");
	}*/	
}

//解析命令行中的参数，参数值填入m_mapPara，是否还需要检查合法性？
void CParaProcess::GetCMDOption(int argc, char  *argv[])
{
	for (int i = 0; i < argc; ++i)
	{
		if(i + 1 >= argc) break;

		if (argv[i][0] == '-')
		{
			switch(argv[i][1])
			{
				case 'C': m_mapPara["co-elute"] = argv[i+1]; i=i+1; break;
				case 'D': m_mapPara["datapath"] = argv[i+1]; i=i+1; break;
				case 'F': m_mapPara["input_format"] = argv[i+1]; i=i+1; break;
				case 'I': m_mapPara["ipv_file"] = argv[i+1]; i=i+1; break;
				case 'M': m_mapPara["model_type"] = argv[i+1]; i=i+1; break;
				case 'O': m_mapPara["outputpath"] = argv[i+1]; i=i+1; break;
				case 'R': m_mapPara["recalibrate_window"] = argv[i+1]; i=i+1; break;
				case 'S': m_mapPara["cut_similiar_mono"] = argv[i+1]; i=i+1; break;
				case 'T': m_mapPara["trainingset"] = argv[i+1]; i=i+1; break;
				case 'W': m_mapPara["isolation_width"] = argv[i+1]; i=i+1; break;
				
				case 'a': m_mapPara["check_activationcenter"] = argv[i+1]; i=i+1; break;
				case 'd': m_mapPara["delete_msn"] = argv[i+1]; i=i+1; break;
				case 'r': m_mapPara["rewrite_files"] = argv[i+1]; i=i+1; break;
				case 's': m_mapPara["output_trainingdata"]= argv[i+1]; i=i+1; break;
				case 't': m_mapPara["mars_threshold"] = argv[i+1]; i=i+1; break;
				case 'u': m_mapPara["export_unchecked_mono"] = argv[i+1]; i=i+1; break;

				case 'c': m_mapPara["max_charge"] = argv[i+1]; i=i+1; break;
				case 'm': m_mapPara["max_mass"] = argv[i+1]; i=i+1; break;
				case 'n': m_mapPara["SN_threshold"] = argv[i+1]; i=i+1; break;
				case 'e': m_mapPara["mz_error_tolerance"] = argv[i+1]; i = i+1; break;
				
				default: i=i+1; break;
			}
		}
	}
	try
	{
		CheckParam();
	}
	catch(exception & e)
	{
		CErrInfo info("CParaProcess", "GetCMDOption", "CheckParam failed.");
		throw runtime_error(info.Get(e).c_str());
		//throw ("CParaProcess::GetCMDOption() CheckParam failed.");
	}
	catch(...)
	{
		CErrInfo info("CParaProcess", "GetCMDOption", "Caught an unknown exception from CheckParam().");
		throw runtime_error(info.Get().c_str());
		//throw ("CParaProcess::GetCMDOption() Caught an unknown exception from CheckParam().");
	}	
}

//根据参数名关键字获取参数值
string CParaProcess::GetValue(const string strKey, string strDef) 
{
	unordered_map<string, string>::iterator it = m_mapPara.find(strKey);
	if(it != m_mapPara.end())
	{
		return it->second;
	} else {
		return strDef;
	}
}

string CParaProcess::GetTimeStr()
{
	time_t tt = time(NULL);//这句返回的只是一个时间戳
	tm* t = localtime(&tt);
	char chtime[128];
	sprintf(chtime, "%d.%02d.%02d.%02d.%02d.%02d", 
	t->tm_year + 1900,
	t->tm_mon + 1,
	t->tm_mday,
	t->tm_hour,
	t->tm_min,
	t->tm_sec);
	string strtime(chtime);
	return strtime;
}

//根据参数名关键字设置单个参数值
void CParaProcess::SetValue(const string strKey, const string strValue) 
{
	m_mapPara[strKey] = strValue;
}

