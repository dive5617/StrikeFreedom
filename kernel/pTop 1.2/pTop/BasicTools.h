#ifndef BASICTOOLS_H_
#define BASICTOOLS_H_

#include <iostream>
#include <sstream>
#include <fstream>
#include <cstring>
#include <string>
#include <limits>
#include <vector>
#include <algorithm>
#include <stdexcept>
#include <time.h>
#include <cstdarg>
#include <io.h>
#include <direct.h>
#include "predefine.h"
using namespace std;
#ifdef WIN32
	const char cSlash = '\\';
#else
	const char cSlash = '/';
#endif

enum LOG_LEVEL  // 日志级别
{
	LL_ERROR = 1,
	LL_WARN = 2,
	LL_INFORMATION = 3,
	LL_DEBUG = 4
};
enum LogAppenderType {
	LAT_CONSOLE = 0x01, LAT_FILE = 0x02, LAT_CONSOLE_AND_FILE = 0x03
};

class Clog
{
public:
	Clog(const char* logPath, LOG_LEVEL eRank = LL_INFORMATION);
	virtual ~Clog();

	static Clog* getInstance(LOG_LEVEL eRank = LL_INFORMATION, bool bReset = false);
	static Clog* getInstance(const char *logPath, LOG_LEVEL eRank = LL_INFORMATION);
	static void destroyInstance();

	virtual void changeLogRank(LOG_LEVEL eRank);
	virtual void alert(const std::string &strAlert);
	virtual void alert(const char *strFormat, ...);
	virtual void info(const std::string &strInfo);
	virtual void info(const char *strFormat, ...);
	virtual void debug(const std::string &strDebug);
	virtual void debug(const char *strFormat, ...);
	virtual void breakPoint();
	

protected:
	void showInfo(const string &strStr, LOG_LEVEL eRank);
private:
	FILE *m_pFile;							// 日志文件
	static Clog *m_plog;
	LOG_LEVEL m_logLevel;

};

class CErrInfo {
	string m_strClass;
	string m_strMethod;
	string m_strDetail;
	string m_strInfo;
	string m_strException;
public:
	CErrInfo(const string &strClass, const string &strMethod,
			const string &strDetail="");

	CErrInfo(const string &strClass, const string &strMethod,
			const string &strDetail, const exception & e);
	
	void Append(const string &strInfo);
	void SetException(const exception & e);
	string Get() const;
	string Get(const exception& e);

	friend ofstream& operator<<(ofstream& os, const CErrInfo& info);
	friend ostream& operator<<(ostream& os, const CErrInfo& info);
};


class CStringProcess {
public:
	static void Trim(string& str);
	static void Trim(const char *&fptr, const char *&lptr);
	static bool bIsNoUse(const char ch);
	static bool bIsNumber(const char ch);
	static void ToLower(string& str);
	static void Split(const string& strFullString, const string& strSign,
			string& strPrior, string& strLatter);
	static bool bMatchingFix(string strFullString, string strFix, bool bSuffix,bool bCaseSensitive);
	static bool isInSet(const string arr[], int n, string &key);
	static void Reverse(string &str);
};

static inline std::string getFile(const std::string &strFilePath) {
	size_t tStart = strFilePath.find_last_of(cSlash);
	size_t tEnd = strFilePath.length();
	return strFilePath.substr(tStart + 1, tEnd - tStart - 1);
}

static inline void makeDir(const std::string &strDirPath) {
	//Clog *pTrace = Clog::getInstance();
	if (access(strDirPath.c_str(), 0) == -1) {
//		pTrace->alert(strDirPath + " does not exist.");
#ifdef WIN32
		if (mkdir(strDirPath.c_str()) == -1) {
#else
			if(mkdir(strDirPath.c_str(), 777) == -1) {
#endif
			CErrInfo err("BasicTools", "makeDir",
					"failed to create dirctory " + strDirPath);
			throw std::runtime_error(err.Get());
		} else {
			//pTrace->info(getFile(strDirPath) + " has been created.");
			//[TODO:]
			cout << "[pTop] " + getFile(strDirPath) + " has been created." << endl;
		}
	}
}



#endif