#ifndef _CONFIGURATION_H_
#define _CONFIGURATION_H_

#include <iostream>
#include <cstdio>
#include <cstring>
#include <limits.h>
#include <windows.h>

#include "predefine.h"
#include "BasicTools.h"

using namespace std;

class COptionTool
{
private:
	static const int BUFFER_SIZE;
	string m_strFile;

	void _GetPrivateProfileString(const string & strSection, const char * strKey, 
			const char * szDefault, char szValue[], int nBufSize);
		
	int _GetPrivateProfileInt(const string & strSection, const char * strKey, const int & nDefault);
					
public:
	COptionTool(const string szFile);
	~COptionTool();

	string GetString(const char * strSection, const char * strKey, const char * szDefault);
	double GetDouble(const char * strSection, const char * strKey, const double lfDefault);
	int GetInteger(const char * strSection, const char * strKey, const int nDefault);		
	size_t GetSizeT(const char * strSection, const char * strKey, const size_t tDefault);
	bool GetBool(const char * strSection, const char * strKey, const bool bDefault);
};

class CConfiguration{
public:
	double m_lfPrecurTol;
	double m_lfFragTol;
	double m_lfThreshold;
	int m_nMaxVarModNum;
	int m_inFileType; // 0->mgf 1->PF
	 
	string m_strFileType; // raw / mgf / pf
	string m_strDBFile;
	string m_strpParseTD;
	string m_strActivation;

	vector<string> m_vFixMod;
	vector<string> m_vVarMod;
	vector<string> m_vSpecFiles;
	vector<string> m_vstrSummary;
	vector<string> m_vstrQryResFile;
	vector<string> m_vstrFilterResFile;
	vector<FILE *> m_vfLabelFiles;

	CConfiguration(const string strFile);
	~CConfiguration();
	string GetTimeStr();
	void GetAllConfigPara();
	void CallpParseTD();
	void PrintParas(int idx, string &outpath);

private:
	string m_strIniFile;

	void _GetFilelistbyDirent(string &filepath, string fileType, vector<string> &fileList);
	void _CheckPath();	
	
};

#endif