#include <iostream>
#include <cstdio>
#include <ctime>
#include <cstring>
#include <limits.h>
#include <windows.h>
#include <direct.h>
#include <string>
#include <errno.h>

#include "dirent.h"
#include "predefine.h"
#include "BasicTools.h"
#include "Configuration.h"

using namespace std;

const int COptionTool::BUFFER_SIZE = 81920;

COptionTool::COptionTool(const string szFile)
		:m_strFile(szFile)
{

};

COptionTool::~COptionTool()
{

}

void COptionTool::_GetPrivateProfileString(const string & strSection, const char * strKey,const char * szDefault,
				char szValue[], int nBufSize)
{
	szValue[0] = 0;

	FILE *fp = fopen(m_strFile.c_str(), "r");
	if(!fp)
	{
		strcpy(szValue, szDefault);
		//CErrInfo info("COptionTool", "_GetPrivateProfileString", "cannot find the file.");
		throw runtime_error("COptionTool _GetPrivateProfileString cannot find the file." + m_strFile);
	}

	char szBuf[BUFFER_SIZE] = {0};
	size_t lenKey = strlen(strKey);
	bool bRange = false;
	if(strSection.length() == 0)
	{
		bRange = true;
	} else {
		string str("[");
		str.append(strSection);
		str.append("]");
		size_t lenApp = str.length();
		while(1)
		{
			if(0 == fgets(szBuf, BUFFER_SIZE - 1, fp))
			{
				break;
			}

			szBuf[lenApp] = 0;

			if(strcmp(str.c_str(), szBuf) == 0)
			{
				bRange = true;
				break;
			}
		}
	}
	if(bRange)
	{
		while(1)
		{
			if(0 == fgets(szBuf, BUFFER_SIZE - 1, fp))
			{
				break;
			}
			size_t tCurrLen = strlen(szBuf) - 1;
            if(szBuf[lenKey] != '=')
            {
                continue;
            }
			szBuf[lenKey] = 0;
						
			if(0 == strcmp(szBuf, strKey))
			{
				while(tCurrLen >= 0 && (szBuf[tCurrLen] == 0xa || szBuf[tCurrLen] == 0xd))
				{   // 0xa 换行，0xd回车
					szBuf[tCurrLen--] = 0;
				}
				strcpy(szValue, szBuf + lenKey + 1);
				fclose(fp);
				return;
			}
		}
		strcpy(szValue, szDefault);
		fclose(fp);
		return;
	} else {
		strcpy(szValue, szDefault);
		fclose(fp);
		return;
	}			
}

int COptionTool::_GetPrivateProfileInt(const string & strSection, const char * strKey, const int & nDefault)
{
	char szValue[BUFFER_SIZE] = {0};
	_GetPrivateProfileString(strSection, strKey, "", szValue, BUFFER_SIZE);
	if(0 == strlen(szValue))
	{
		return nDefault;
	} else {
		return atoi(szValue);
	}
}
		
string COptionTool::GetString(const char * strSection, const char * strKey, const char * szDefault)
{
	char szValue[BUFFER_SIZE] = {0};
	_GetPrivateProfileString(strSection, strKey, szDefault, szValue, BUFFER_SIZE);
	return string(szValue);
};

double COptionTool::GetDouble(const char * strSection, const char * strKey, const double lfDefault)
{
	char szValue[BUFFER_SIZE] = {0};
	_GetPrivateProfileString(strSection, strKey, "", szValue, BUFFER_SIZE);
	if(strlen(szValue) == 0)
		return lfDefault;
	else return atof(szValue);
}
		
int COptionTool::GetInteger(const char * strSection, const char * strKey, const int nDefault)
{
	return _GetPrivateProfileInt(strSection, strKey, nDefault);
}
		
size_t COptionTool::GetSizeT(const char * strSection, const char * strKey, const size_t tDefault)
{
	return (size_t)GetInteger(strSection, strKey, (int)tDefault);	
};

bool COptionTool::GetBool(const char * strSection, const char * strKey, const bool bDefault)
{
	char szValue[BUFFER_SIZE] = {0};
	_GetPrivateProfileString(strSection, strKey, "", szValue, BUFFER_SIZE);
	if(strcmp(szValue, "true") == 0 || strcmp(szValue, "TRUE") == 0)
	{
		return true;
	} else if(strcmp(szValue, "false") == 0 || strcmp(szValue, "FALSE") == 0) {
		return false;
	} else {
		return bDefault;
	}
};	

/**
*	class CConfiguration
*/

CConfiguration::CConfiguration(const string strFile) : m_strIniFile(strFile)
{

}
CConfiguration::~CConfiguration()
{
	for(size_t i = 0; i < m_vfLabelFiles.size(); ++i)
	{
		if(m_vfLabelFiles[i] != NULL)
		{
			fclose(m_vfLabelFiles[i]);
			m_vfLabelFiles[i] = NULL;
		}
	}
}

void CConfiguration::GetAllConfigPara()
{
	try
	{
		/*size_t found = m_strIniFile.find_last_of(".");
		if(found == string::npos || ( m_strIniFile.substr(found, 4).compare(".ini") != 0 
			&& m_strIniFile.substr(found, 4).compare(".INI") != 0 ) )
		{
			m_strIniFile.append(".ini");
		}*/
		FILE *fp = fopen(m_strIniFile.c_str(), "r");
		if(NULL == fp)
		{
			cout << "Cannot open the config file: " << m_strIniFile << endl;
			exit(0);
		}
		fclose(fp);
		//cout << m_strIniFile << endl;
		COptionTool config(m_strIniFile);

		m_strDBFile = config.GetString("database", "Database", "test.fasta");

		m_strpParseTD = config.GetString("database", "pParseTD_cfg", "pParseTD.cfg");

		//m_strSpecFile = config.GetString("spectrum", "Spec_path", "test.mgf");
		m_strActivation = config.GetString("spectrum", "Activation", "HCD");
		
		m_strFileType = config.GetString("spectrum", "input_format", "RAW");
		for(size_t i = 0; i < m_strFileType.length(); ++i)
		{
			if(m_strFileType[i] >= 'a' && m_strFileType[i] <= 'z')
			{
				m_strFileType[i] += 'A' - 'a';
			}
		}

		int msmsNum = config.GetInteger("spectrum", "msmsnum", 0);
		char msmsPath[PATH_MAX];
		for(int i = 1; i <= msmsNum; ++i)
		{
			sprintf(msmsPath, "msmspath%d", i);
			string strMSPath = config.GetString("spectrum", msmsPath, "");
			m_vSpecFiles.push_back(strMSPath);
		}

		//m_strOutputFile = config.GetString("output", "outputpath", "");
		
		m_lfPrecurTol = config.GetDouble("spectrum", "Precursor_Tolerance", 2.0);
		m_lfFragTol = config.GetDouble("spectrum", "Fragment_Tolerance", 20.0);
		m_lfThreshold = config.GetDouble("filter", "threshold", 0.01);

		m_nMaxVarModNum = config.GetInteger("modify", "Max_mod", 5);
		int fixNum = config.GetInteger("fixmodify", "fixedModify_num", 0);
		int varNum = config.GetInteger("modify", "Modify_num", 0);

		char tmpCh[20];
		for(int i = 1; i <= fixNum; ++i)
		{
			sprintf(tmpCh, "fix_mod%d", i);
			string fixName = config.GetString("fixmodify", tmpCh, "");
			m_vFixMod.push_back(fixName);
		}
		for(int i = 1; i <= varNum; ++i)
		{
			sprintf(tmpCh, "var_mod%d", i);
			string varName = config.GetString("modify", tmpCh, "");
			m_vVarMod.push_back(varName);
		}
		CallpParseTD();
		_CheckPath();
	} 
	catch(exception & e) 
	{
		CErrInfo info("CConfiguration", "GetAllConfigPara", "GetConfigInfo failed.");
		cout << "Please check the path of file config.ini!" << endl;
		throw runtime_error(info.Get(e).c_str());
	}
	catch(...) 
	{
		CErrInfo info("CConfiguration", "GetAllConfigPara", "caught an unknown exception from GetConfigInfo.");
		cout << "Please check the path of file config.ini!" << endl;
		throw runtime_error(info.Get().c_str());
	}

	//PrintParas();
}

//如果输入的是RAW，则返回true，其他情况返回false
//若输入为路径，则最后一个字符一定是'\'
void CConfiguration::CallpParseTD()
{
	if(0 == m_vSpecFiles.size())
	{
		cout << "[pTop] There is no input file!" << endl;
		return;
	}

	if(0 == m_strFileType.compare("MGF"))
	{
		m_inFileType = 0;
		return;
	}

	if(0 == m_strFileType.compare("PF"))
	{
		m_inFileType = 1;
		return;
	}

	// if(0 == m_strFileType.compare("RAW") || 0 == m_strFileType.compare("PATH"))
	// call pParseTD
	try
	{
		m_inFileType = 1;
		FILE *fp = fopen(m_strpParseTD.c_str(), "r");
		if(NULL == fp)
		{
			throw runtime_error("[Error] Cannot open the config file of pParseTD: " + m_strpParseTD);
		} else {
			fclose(fp);
		}
		string mscmdline = "pParseTD.exe \"" + m_strpParseTD + "\""; //配置文件路径
		cout << "[Call pParseTD] " << mscmdline << endl;
		int err_no = system(mscmdline.c_str());
		if(err_no){  // 调用pParseTD异常
			throw err_no;
		}
	}
	catch(exception & e) 
	{
		//CErrInfo info("CConfiguration", "CallpParseTD", "Call pParseTD failed.");
		//throw runtime_error(info.Get(e).c_str());
		string info = e.what();
		ofstream ofs("pTop.err.log", ios::app);
		ofs << info;
		cout << info << endl;
		exit(1);
	}
	catch(...) 
	{
		//CErrInfo info("CConfiguration", "CallpParseTD", "caught an unknown exception from Calling pParseTD.");
		//throw runtime_error(info.Get().c_str());
		string info = "[pTop] Call pParseTD failed.";
		ofstream ofs("pTop.err.log", ios::app);
		ofs << info;
		cout << info << endl;
		exit(1);
	}


	if(0 == m_strFileType.compare("PATH")) // path
	{
		string pathName = m_vSpecFiles[0];
		m_vSpecFiles.clear();
		_GetFilelistbyDirent(pathName, ".pf2", m_vSpecFiles);
	} else {	
		try
		{   // 检查谱图文件是否存在
			string actType = m_strActivation;
			if(actType.compare("UVPD") == 0)
			{
				actType = "HCD";
			}
			size_t i = 0;
			while(i < m_vSpecFiles.size())
			{
				m_vSpecFiles[i].erase(m_vSpecFiles[i].end() - 4, m_vSpecFiles[i].end());
				string tmpFile = m_vSpecFiles[i] + "_" + actType + "FT.pf2";
				FILE *fp = fopen(tmpFile.c_str(), "r");
				if(fp != NULL)
				{   // 对于一个raw，pXtract导出时会生成多个临时文件，如CID碎裂的会得到CID_FT.mgf和CID_IT.mgf，这里也是先枚举，再看看这些文件是否存在
					m_vSpecFiles[i].append("_" + actType + "FT.pf2");
					fclose(fp);
					++i;
				} else {
					tmpFile = m_vSpecFiles[i] + "_" + actType + "IT.pf2";
					fp = fopen(tmpFile.c_str(), "r");
					if(fp != NULL)
					{
						m_vSpecFiles[i].append("_" + actType + "IT.pf2");
						fclose(fp);
						++i;
					} else {
						cout << "[Warning] Can not open file: " << m_vSpecFiles[i].c_str() << "_" << actType.c_str() << "FT.pf2" << endl;		
						m_vSpecFiles.erase(m_vSpecFiles.begin() + i);
					}
				}
			}
		} 
		catch(exception & e) 
		{
			CErrInfo info("CConfiguration", "CallpParseTD", "Set Input and Output failed.");
			throw runtime_error(info.Get(e).c_str());
		}
		catch(...) 
		{
			CErrInfo info("CConfiguration", "CallpParseTD", "caught an unknown exception from Setting Input and Output.");
			throw runtime_error(info.Get().c_str());
		}
	}
}

string CConfiguration::GetTimeStr()
{
	time_t tt = time(NULL);//这句返回的只是一个时间戳
	tm* t = localtime(&tt);
	char chtime[128];
	sprintf(chtime, "%d%02d%02d%02d%02d%02d", 
	t->tm_year + 1900,
	t->tm_mon + 1,
	t->tm_mday,
	t->tm_hour,
	t->tm_min,
	t->tm_sec);
	string strtime(chtime);
	return strtime;
}

//获取指定路径下的所有指定类型的文件 fileType = ".raw"
void CConfiguration::_GetFilelistbyDirent(string &filepath, string fileType, vector<string> &fileList)
{
	DIR *dir = opendir(filepath.c_str());
	if (dir == NULL) 
	{
		cout << "[Error] Failed to open the directory: " << filepath << endl;
		exit(0);
	}
	struct dirent *de;
	while(NULL != (de = readdir(dir)))
	{
		if (de->d_name[0] == '.') 
		{
			continue;
		}
		string FileName = de->d_name;

		size_t dotpos = FileName.find_last_of('.');
		if(dotpos == string::npos)
		{
			continue;
		}
		string fileSuffix = FileName.substr(dotpos);
		for (int i = 1; i < (int)fileSuffix.size(); i++) 
		{
			fileSuffix[i] = tolower(fileSuffix[i]);
		}
		if(fileSuffix.compare(fileType) != 0)
		{
			continue;
		}

		string fileName = filepath;
		fileName.append(de->d_name);
		fileList.push_back(fileName);
	}
	closedir(dir);
}

// 检查路径
void CConfiguration::_CheckPath()
{
	string strtime = GetTimeStr();
	for(size_t i = 0; i < m_vSpecFiles.size(); ++i)
	{
		string inFile = m_vSpecFiles[i];
		size_t found = inFile.find_last_of(cSlash);
		string filename;
		if(found == string::npos)
		{
			filename = inFile;
		} else {
			filename = inFile.substr(found + 1);
		}

		
		inFile.erase(inFile.end() - 4, inFile.end());
		filename.erase(filename.end() - 4, filename.end());
		

		// 设置输出文件路径
		string outpath = inFile + cSlash;

	#if defined(_WIN32)
		_mkdir(outpath.c_str());
	#else 
		mkdir(outpath.c_str(), 0777);
	#endif

		m_vstrSummary.push_back(outpath + "search_task_" + strtime + "_summary.txt");
		m_vstrQryResFile.push_back(outpath + "search_task_" + strtime + "_query.txt");
		m_vstrFilterResFile.push_back(outpath + "search_task_" + strtime + "_filter.csv");

		filename = outpath + "search_task_" + strtime + ".plabel"; //outpath + filename + "_" + strtime + ".plabel";
		FILE *fLabel = fopen(filename.c_str(), "w+"); 
		//printf("%s\n", filename.c_str());
		//printf("errno is: %d\n",errno);
		m_vfLabelFiles.push_back(fLabel);


		try
		{
			outpath.append("search_task_" + strtime + ".cfg");
			PrintParas(i, outpath);
		}
		catch(exception & e) 
		{
			CErrInfo info("CConfiguration", "_CheckPath", "Failed to output .cfg file!");
			throw runtime_error(info.Get(e).c_str());
		}
		catch(...) 
		{
			CErrInfo info("CConfiguration", "_CheckPath", "caught an unknown exception from PrintParas().");
			throw runtime_error(info.Get().c_str());
		}
	}
}

void CConfiguration::PrintParas(int idx, string &outpath)
{
	FILE *fp = fopen(outpath.c_str(), "w");
	if(NULL == fp)
	{
		cout << "[Warning] Failed to create param file" << outpath << endl;
	} else {
		fprintf(fp, "%s\n", VERSION.c_str());
		fprintf(fp, "# This is a pTop configure file\n");
		fprintf(fp, "\n[database]\nDatabase=%s\n", m_strDBFile.c_str());

		fprintf(fp, "\n[spectrum]\nmsmsnum=1\n");
		fprintf(fp, "msmspath1=%s\n", m_vSpecFiles[idx].c_str());
		fprintf(fp, "querypath1=%s\n", m_vstrQryResFile[idx].c_str());
		fprintf(fp, "filterpath1=%s\n", m_vstrFilterResFile[idx].c_str());
		fprintf(fp, "summarypath1=%s\n\n", m_vstrSummary[idx].c_str());

		fprintf(fp, "\ninput_format=%s\n", m_strFileType.c_str());
		fprintf(fp, "Activation=%s\n", m_strActivation.c_str());
		fprintf(fp, "Precursor_Tolerance=%.1lf\n", m_lfPrecurTol);
		fprintf(fp, "Fragment_Tolerance=%.1lf\n", m_lfFragTol);

		fprintf(fp, "\n[fixmodify]\nfixedModify_num=%d\n", m_vFixMod.size());
		for(size_t i = 0; i < m_vFixMod.size(); ++i)
		{
			fprintf(fp, "fix_mod%d=%s\n", i+1, m_vFixMod[i].c_str());
		}

		fprintf(fp, "\n[modify]\nMax_mod=%d\n", m_nMaxVarModNum);
		fprintf(fp, "Modify_num=%d\n",  m_vVarMod.size());
		for(size_t i = 0; i < m_vVarMod.size(); ++i)
		{
			fprintf(fp, "var_mod%d=%s\n", i+1, m_vVarMod[i].c_str());
		}
		fprintf(fp, "\n[filter]\nthreshold=%lf\n", m_lfThreshold);
	}
}