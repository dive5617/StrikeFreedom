#ifndef UTIL_H_
#define UTIL_H_
#include "sdk.h"
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
#include <winnt.h>
#include <processenv.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <memory>
using namespace std;

enum AppenderColorType {
	ACT_WHITE, ACT_RED, ACT_GREEN
};

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

// abstract class
class Appender {
protected:
	FILE *m_fp;

public:
	Appender(FILE *fp) :
		m_fp(fp) {
	}
	virtual ~Appender() {
		if (m_fp) {
			if (m_fp != stdout)
				delete m_fp;
			m_fp = NULL;
		}
	}

protected:
	virtual FILE *getConnection() = 0;
	virtual void endConnection() = 0;
};

class ConsoleAppender : public Appender {
public:
	ConsoleAppender() :
		Appender(stdout) {
	}
	virtual ~ConsoleAppender() {
	}

	virtual void append(const char *szInfo, AppenderColorType eColor = ACT_WHITE) = 0;
	virtual void append(const std::string &strInfo, AppenderColorType eColor = ACT_WHITE) = 0;

protected:
	virtual FILE *getConnection() {
		return stdout;
	}
	virtual void endConnection() {
	}
};

class WindowsConsoleAppender : public ConsoleAppender {
public:
	WindowsConsoleAppender() {
	}
	virtual ~WindowsConsoleAppender() {
	}

	virtual void append(const char *szInfo, AppenderColorType eColor = ACT_WHITE);
	virtual void append(const std::string &strInfo, AppenderColorType eColor = ACT_WHITE);
};

class LinuxConsoleAppender : public ConsoleAppender {
public:
	LinuxConsoleAppender() {
	}
	virtual ~LinuxConsoleAppender() {
	}

	virtual void append(const char *szInfo, AppenderColorType eColor = ACT_WHITE);
	virtual void append(const std::string &strInfo, AppenderColorType eColor = ACT_WHITE);
};

class FileAppender : public Appender {
protected:
	std::string m_strLogPath;

public:
	FileAppender(const char *szLogPath);
	virtual ~FileAppender();

	virtual void append(const char *szInfo) = 0;
	virtual void append(const std::string &strInfo) = 0;

protected:
	virtual FILE *getConnection();
	virtual void endConnection();
};

class WindowsFileAppender : public FileAppender {
public:
	WindowsFileAppender(const char *szLogPath) :
		FileAppender(szLogPath) {
	}
	virtual ~WindowsFileAppender() {
	}

	virtual void append(const char *szInfo);
	virtual void append(const std::string &strInfo);
};

class LinuxFileAppender : public FileAppender {
public:
	LinuxFileAppender(const char *szLogPath) :
		FileAppender(szLogPath) {
	}
	virtual ~LinuxFileAppender() {
	}

	virtual void append(const char *szInfo);
	virtual void append(const std::string &strInfo);
};

class AppenderFactory {
public:
	AppenderFactory() {
	}
	virtual ~AppenderFactory() {
	}

	virtual Appender *getConsoleAppender() = 0;
	virtual Appender *getFileAppender(const char *LogPath) = 0;
};

class WindowsAppenderFactory : public AppenderFactory {
public:
	WindowsAppenderFactory() {
	}
	virtual ~WindowsAppenderFactory() {
	}

	virtual Appender *getConsoleAppender();
	virtual Appender *getFileAppender(const char *szLogPath);
};

class LinuxAppenderFactory : public AppenderFactory {
public:
	LinuxAppenderFactory() {
	}
	virtual ~LinuxAppenderFactory() {
	}

	virtual Appender *getConsoleAppender();
	virtual Appender *getFileAppender(const char *szLogPath);
};


class Clog
{
public:
	virtual ~Clog();

	static Clog* getInstance(LOG_LEVEL eRank = LL_INFORMATION, bool bReset = false);
	static Clog* getInstance(const char *logPath, LogAppenderType eLogApp = LAT_FILE, LOG_LEVEL eRank = LL_INFORMATION);
	static void destroyInstance();

	virtual void changeLogRank(LOG_LEVEL eRank);
	virtual void warning(const std::string &strAlert);
	virtual void warning(const char *strFormat, ...);
	virtual void error(const std::string &strAlert);
	virtual void error(const char *strFormat, ...);
	virtual void info(const std::string &strInfo);
	virtual void info(const char *strFormat, ...);
	virtual void debug(const std::string &strDebug);
	virtual void debug(const char *strFormat, ...);
	virtual void breakPoint();
	

protected:
	Clog(const char* logPath, LogAppenderType eLogApp, LOG_LEVEL eRank = LL_INFORMATION);
	void showInfo(const string &strStr, LOG_LEVEL eRank, AppenderColorType eColor = ACT_WHITE);
private:
	Appender *m_pFile; 
	Appender *m_pConsole;

	static Clog *m_plog;
	LOG_LEVEL m_logLevel;
	LogAppenderType m_eLogApp;

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


class CStringProcess {
public:
	static void Trim(string& str);
	static void Trim(const char *&fptr, const char *&lptr);
	static bool bIsNoUse(const char ch);
	static bool bIsNumber(const char ch);
	static void ToLower(string& str);
	static void Split(const string& strFullString, const string& strSign,
			string& strPrior, string& strLatter);
	static void CStringProcess::Split(const string &str, const string delim, vector<string> &ret);
	static bool bMatchingFix(string strFullString, string strFix, bool bSuffix,bool bCaseSensitive);
	static bool isInSet(const string arr[], int n, string &key);
	static void Reverse(string &str);
	static int find_first_of(const char *s, char ch, int startPos, int len);
};

class SearchState {
protected:
	bool m_bSet;
	clock_t m_tStartClock;
	time_t m_tStartTime;

public:
	ThreadState m_stState;  //记录总的执行情况
	std::vector<ThreadState> m_vThreadState;  // 记录每个线程的执行情况

	SearchState(size_t tThreadSize);
	virtual ~SearchState();

	virtual std::string getProgress();
	virtual void setStateID(size_t tID);
	virtual size_t getStateID() const;
	virtual void setTotalSpectra(size_t tTotal);
	virtual void setCurrentSpectra(size_t tCurrent);
	virtual void setThreadTotalSpectra(size_t tID, size_t tTotal);
	virtual void setThreadCurrentSpectra(size_t tID, size_t tCurrent);
	virtual clock_t getStartClock() const;
	virtual time_t getStartTime() const;
};

class ThreadMonitor {
	std::vector<bool> m_vFlowFlag;  //指示某个线程是否空闲
	size_t m_tCurrentThreadID;
	bool m_bLocked;

public:
	pthread_mutex_t m_stMutex;  //互斥锁，多线程中对共享变量的包保护
	pthread_cond_t m_stCond;  // 线程间同步，一般和pthread_mutex_t一起使用，以防止出现逻辑错误，即如果单独使用条件变量，某些情况下（条件变量前后出现对共享变量的读写）会出现问题

public:
	ThreadMonitor(size_t tThreadNo);
	~ThreadMonitor();

	bool isAllBusy(); 

	void setSignal(size_t tID) {
		m_vFlowFlag[tID] = false;
	}

	void freeSignal(size_t tID) {
		m_vFlowFlag[tID] = true;
	}

	bool isFreeSignal(size_t tID) const {
		return m_vFlowFlag[tID];
	}

	void setLock() {
		m_bLocked = true;
	}

	void freeLock() {
		m_bLocked = false;
	}

	bool isLocked() const {
		return m_bLocked;
	}

	void setCurrentID(size_t tID) {
		m_tCurrentThreadID = tID;
	}

	size_t getCurrentID() const {
		return m_tCurrentThreadID;
	}

	void waitForLock();
};

static inline size_t getCPUCoreNo() {
#ifdef _WIN32
	SYSTEM_INFO info;
	GetSystemInfo(&info);
	return info.dwNumberOfProcessors;
//#elif _WIN64
	
#else //Linux Compiler
	//return 1;
	return get_nprocs();  
//#error
#endif
}

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


static inline double CalculateSTD(vector<double>& nums)
{
	double std = 0.0;
	double sum = 0;
	for (int i = 0; i < nums.size(); ++i){
		sum += nums[i];
	}
	double mean = sum*1.0 / nums.size();
	sum = 0;
	for (int i = 0; i < nums.size(); ++i){
		sum += (nums[i] - mean)*(nums[i] - mean);
	}
	std = sqrt(sum / nums.size());
	return std;
}

// vector 内存释放
template < class T >
inline void ClearVector(vector<T>& vt)
{
	vector<T> vtTemp;
	vtTemp.swap(vt);
}



#endif