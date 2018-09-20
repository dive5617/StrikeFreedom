#include <iostream>
#include <sstream>
#include <fstream>
#include <cstring>
#include <string>
#include <stdexcept>
#include <time.h>

#include "util.h"

using namespace std;

/*
**	class ConsoleAppender
*/


/*
**	class WindowsConsoleAppender
*/
void WindowsConsoleAppender::append(const char *szInfo, AppenderColorType eColor)
{
	HANDLE hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
	switch (eColor){
	case ACT_RED:
		SetConsoleTextAttribute(hConsole, FOREGROUND_RED | FOREGROUND_INTENSITY);
		break;
	case ACT_GREEN:
		SetConsoleTextAttribute(hConsole, FOREGROUND_GREEN | FOREGROUND_INTENSITY);
		break;
	default:
		/* default set on RGB, white */
		break;
	}
	fprintf(m_fp, "%s\n", szInfo);
	fflush(m_fp);
	SetConsoleTextAttribute(hConsole, FOREGROUND_RED | FOREGROUND_GREEN | FOREGROUND_BLUE);
}
void WindowsConsoleAppender::append(const std::string &strInfo, AppenderColorType eColor)
{
	append(strInfo.c_str(), eColor);
}


/*
**	class LinuxConsoleAppender
*/

void LinuxConsoleAppender::append(const char *szInfo, AppenderColorType eColor)
{
	string strCode;
	switch (eColor)
	{
	case ACT_RED:
		strCode = "\033[1;31m";
		break;
	case ACT_GREEN:
		strCode = "\033[1;32m";
		break;
	default:
		// default set white
		break;
	}
	fprintf(m_fp, "%s%s\033[1;37m\n", strCode.c_str(), szInfo);
	fflush(m_fp);
}
void LinuxConsoleAppender::append(const std::string &strInfo, AppenderColorType eColor)
{
	append(strInfo.c_str(), eColor);
}

/*
**  class FileAppender
*/

FileAppender::FileAppender(const char *szLogPath) :Appender(NULL)
{
	m_strLogPath = szLogPath;
	m_fp = getConnection();
}
FileAppender::~FileAppender()
{
	endConnection();
}

FILE * FileAppender::getConnection()
{
	FILE* m_pFile = fopen(m_strLogPath.c_str(), "a+");
	if (!m_pFile){
		CErrInfo err("FileAppender", "getConnection", "Failed to open log file");
		throw runtime_error(err.Get());
	}
	return m_pFile;
}
void FileAppender::endConnection()
{
	if (m_fp){
		fclose(m_fp);
		m_fp = NULL;
	}
}

/*
**  class WindowsFileAppender
*/
void WindowsFileAppender::append(const char *szInfo)
{
	fprintf(m_fp, "%s\r\n", szInfo);
	fflush(m_fp);
}
void WindowsFileAppender::append(const std::string &strInfo)
{	
	append(strInfo.c_str());
}


/*
**  class LinuxFileAppender
*/

void LinuxFileAppender::append(const char *szInfo)
{
	fprintf(m_fp, "%s\n", szInfo);
	fflush(m_fp);
}
void LinuxFileAppender::append(const std::string &strInfo)
{
	append(strInfo.c_str());
}

/*
**  class AppenderFactory
*/


/*
**  class WindowsAppenderFactory
*/
Appender* WindowsAppenderFactory::getConsoleAppender()
{
	return new WindowsConsoleAppender();
}
Appender* WindowsAppenderFactory::getFileAppender(const char *szLogPath)
{
	return new WindowsFileAppender(szLogPath);
}


/*
**  class LinuxAppenderFactory
*/
Appender* LinuxAppenderFactory::getConsoleAppender()
{
	return new LinuxConsoleAppender();
}
Appender* LinuxAppenderFactory::getFileAppender(const char *szLogPath)
{
	return new LinuxFileAppender(szLogPath);
}


/*
**	class Clog
*/

Clog* Clog::m_plog = NULL;

Clog::Clog(const char* szLogPath, LogAppenderType eLogApp, LOG_LEVEL erank) :
m_pConsole(NULL), m_pFile(NULL), m_logLevel(erank),
m_eLogApp((szLogPath == NULL)? LAT_CONSOLE : eLogApp) 
{
	AppenderFactory *pAf = NULL;
#ifdef WIN32
	pAf = new WindowsAppenderFactory();
#else
	pAf = new LinuxAppenderFactory();
#endif
	if (m_eLogApp & LAT_CONSOLE)
		m_pConsole = pAf->getConsoleAppender();
	if (m_eLogApp & LAT_FILE)
		m_pFile = pAf->getFileAppender(szLogPath);

	if (pAf) {
		delete pAf;
		pAf = NULL;
	}
}

Clog* Clog::getInstance(const char *logPath, LogAppenderType eLogApp, LOG_LEVEL eRank)
{
	destroyInstance();
	if(m_plog == NULL){
		m_plog = new Clog(logPath, eLogApp, eRank);
	}
	return m_plog;
}

Clog* Clog::getInstance(LOG_LEVEL eRank, bool bReset)
{
	if(bReset)
		destroyInstance();

	if(m_plog == NULL){
		m_plog = new Clog(NULL, LAT_CONSOLE, eRank);
	}

	return m_plog;
}

void Clog::destroyInstance()
{
	if(m_plog){
		delete m_plog;
		m_plog = NULL;
	}
}

void Clog::changeLogRank(LOG_LEVEL eRank)
{
	m_logLevel = eRank;
}

void Clog::error(const std::string &strAlert)
{
	string strStr = "[Error] " + strAlert;
	showInfo(strStr, LL_ERROR, ACT_RED);
}

void Clog::error(const char *strFormat, ...)
{
	va_list ap;
	va_start(ap, strFormat);
	char szBuf[STR_BUF_SIZE];
	vsprintf(szBuf, strFormat, ap);
	va_end(ap);
	error(string(szBuf));
}

void Clog::warning(const std::string &strAlert)
{
	string strStr = "[Warning] " + strAlert;
	showInfo(strStr, LL_WARN, ACT_RED);
}

void Clog::warning(const char *strFormat, ...)
{
	va_list ap;
	va_start(ap, strFormat);
	char szBuf[STR_BUF_SIZE];
	vsprintf(szBuf, strFormat, ap);
	va_end(ap);
	warning(string(szBuf));
}

void Clog::info(const std::string &strInfo)
{
	string strStr = "[pTop] " + strInfo;
	showInfo(strStr, LL_INFORMATION);
}

void Clog::info(const char *strFormat, ...)
{
	va_list ap;
	va_start(ap, strFormat);
	char szBuf[STR_BUF_SIZE];
	vsprintf(szBuf, strFormat, ap);
	va_end(ap);
	info(string(szBuf));
}
	
void Clog::debug(const std::string &strDebug)
{
	string strStr = "[debug] " + strDebug;
	showInfo(strStr, LL_DEBUG, ACT_GREEN);
}

void Clog::debug(const char *strFormat, ...)
{
	va_list ap;  // 用于获取不确定个数的参数
	va_start(ap, strFormat);
	char szBuf[STR_BUF_SIZE];
	vsprintf(szBuf, strFormat, ap);
	va_end(ap);
	debug(string(szBuf));
}
	
void Clog::breakPoint()
{
	string strStr = "[stop] Press any key to continue ...";
	showInfo(strStr, LL_DEBUG, ACT_RED);
	getchar();
}

Clog::~Clog()
{
	if (m_pConsole){
		delete m_pConsole;
		m_pConsole = NULL;
	}
	if(m_pFile){
		delete m_pFile;
		m_pFile = NULL;
	}
}

void Clog::showInfo(const string &strStr, LOG_LEVEL eRank, AppenderColorType eColor)
{
	if(m_logLevel >= eRank){
		if (m_pConsole){
			if (ConsoleAppender *pCon = dynamic_cast<ConsoleAppender *>(m_pConsole)){
				pCon->append(strStr, eColor);
			}
		}
		if (m_pFile){
			if(FileAppender *pF = dynamic_cast<FileAppender *>(m_pFile)){
				pF->append(strStr);
			}
		}
	}
}

/**
*	CErrInfo
**/
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
	{
		m_strInfo += "\t\t  " + strInfo+"\n";
	}
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

/**
* COptionTool
**/

const int COptionTool::BUFFER_SIZE = 81920;

COptionTool::COptionTool(const string szFile)
	:m_strFile(szFile)
{

};

COptionTool::~COptionTool()
{

}

void COptionTool::_GetPrivateProfileString(const string & strSection, const char * strKey, const char * szDefault,
	char szValue[], int nBufSize)
{
	szValue[0] = 0;

	FILE *fp = fopen(m_strFile.c_str(), "r");
	if (!fp)
	{
		strcpy(szValue, szDefault);
		//CErrInfo info("COptionTool", "_GetPrivateProfileString", "cannot find the file.");
		throw runtime_error("COptionTool _GetPrivateProfileString cannot find the file." + m_strFile);
	}

	char szBuf[BUFFER_SIZE] = { 0 };
	size_t lenKey = strlen(strKey);
	bool bRange = false;
	if (strSection.length() == 0)
	{
		bRange = true;
	}
	else {
		string str("[");
		str.append(strSection);
		str.append("]");
		size_t lenApp = str.length();
		while (1)
		{
			if (0 == fgets(szBuf, BUFFER_SIZE - 1, fp))
			{
				break;
			}

			szBuf[lenApp] = 0;

			if (strcmp(str.c_str(), szBuf) == 0)
			{
				bRange = true;
				break;
			}
		}
	}
	if (bRange)
	{
		while (1)
		{
			if (0 == fgets(szBuf, BUFFER_SIZE - 1, fp))
			{
				break;
			}
			size_t tCurrLen = strlen(szBuf) - 1;
			if (szBuf[lenKey] != '=')
			{
				continue;
			}
			szBuf[lenKey] = 0;

			if (0 == strcmp(szBuf, strKey))
			{
				while (tCurrLen >= 0 && (szBuf[tCurrLen] == 0xa || szBuf[tCurrLen] == 0xd))
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
	}
	else {
		strcpy(szValue, szDefault);
		fclose(fp);
		return;
	}
}

int COptionTool::_GetPrivateProfileInt(const string & strSection, const char * strKey, const int & nDefault)
{
	char szValue[BUFFER_SIZE] = { 0 };
	_GetPrivateProfileString(strSection, strKey, "", szValue, BUFFER_SIZE);
	if (0 == strlen(szValue))
	{
		return nDefault;
	}
	else {
		return atoi(szValue);
	}
}

string COptionTool::GetString(const char * strSection, const char * strKey, const char * szDefault)
{
	char szValue[BUFFER_SIZE] = { 0 };
	_GetPrivateProfileString(strSection, strKey, szDefault, szValue, BUFFER_SIZE);
	return string(szValue);
};

double COptionTool::GetDouble(const char * strSection, const char * strKey, const double lfDefault)
{
	char szValue[BUFFER_SIZE] = { 0 };
	_GetPrivateProfileString(strSection, strKey, "", szValue, BUFFER_SIZE);
	if (strlen(szValue) == 0)
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
	char szValue[BUFFER_SIZE] = { 0 };
	_GetPrivateProfileString(strSection, strKey, "", szValue, BUFFER_SIZE);
	if (strcmp(szValue, "true") == 0 || strcmp(szValue, "TRUE") == 0)
	{
		return true;
	}
	else if (strcmp(szValue, "false") == 0 || strcmp(szValue, "FALSE") == 0) {
		return false;
	}
	else {
		return bDefault;
	}
};




/**
*	CStringProcess
**/

void CStringProcess::Trim(string& str) {
	int i;
	for (i = 0; i < (int)str.length() && bIsNoUse(str.at(i)); ++i)
		;
	if (i == (int)str.length()) {
		str.erase(0, i - 1);
		return;
	}
	str.erase(0, i);
	for (i = str.length() - 1; i >= 0 && bIsNoUse(str.at(i)); --i)
		;
	str.erase(i + 1, str.length() - 1 - i);
	return;
}

void CStringProcess::Trim(const char *&fptr, const char *&lptr) {
	while (fptr < lptr && bIsNoUse(*fptr))
		++fptr;
	while (fptr < lptr && bIsNoUse(*(lptr - 1)))
		--lptr;
	return;
}

bool CStringProcess::bIsNoUse(const char ch) {
	if (' ' == ch || '\r' == ch || '\t' == ch) {
		return true;
	}
	return false;
}

bool CStringProcess::bIsNumber(const char ch) {
	if ('0' <= ch && '9' >= ch) {
		return true;
	}
	return false;
}

void CStringProcess::ToLower(string& str) {
	for (size_t i = 1; i < str.size(); i++) {
		str[i] = tolower(str[i]);
	}
}

void CStringProcess::Split(const string& strFullString, const string& strSign,
		string& strPrior, string& strLatter) {
	size_t i = strFullString.find(strSign);
	if (i != string::npos) {
		strPrior = strFullString.substr(0, i);
		strLatter = strFullString.substr(i + strSign.length(),
				strFullString.length() - i - strSign.length());
	} else {
		strPrior = strFullString;
		strLatter.clear();
	}
}

void CStringProcess::Split(const string &str, const string delim, vector<string> &ret)
{
	size_t last = 0;
	size_t index = str.find_first_of(delim, last);
	while (index != string::npos)
	{
		ret.push_back(str.substr(last, index - last));
		last = index + delim.length();
		index = str.find_first_of(delim, last);
	}
	if (str.length() > last && (str[last] != '\r') && (str[last] != '\n'))
	{
		ret.push_back(str.substr(last, str.size() - last));
	}
}

bool CStringProcess::bMatchingFix(string strFullString, string strFix,
		bool bSuffix, bool bCaseSensitive) {
	size_t tLength = strFix.size();
	if (tLength >= strFullString.size()) {
		return false;
	}
	if (bSuffix) {
		strFullString.erase(strFullString.begin(), strFullString.end()
				- tLength);
	} else {
		strFullString.erase(strFullString.begin() + tLength,
				strFullString.end());
	}
	if (!bCaseSensitive) {
		CStringProcess::ToLower(strFullString);
		CStringProcess::ToLower(strFix);
	}
	return strFullString == strFix;
}

bool CStringProcess::isInSet(const string arr[], int n, string &key)
{
	for(int i = 0; i < n && arr[i] != ""; ++i)
	{
		if(arr[i].compare(key) == 0) return true;
	}
	return false;
}

// Generate the decoy sequence
void CStringProcess::Reverse(string &str)
{

/*
//---------Shuffle---------
   srand(time(NULL));
    size_t len = sequence.length();
    for(size_t i = 0; i < len; ++i)
    {
        int idx = rand() % len;
        swap(sequence[i], sequence[idx]);
    }
*/
	
//----------Reverse--------
 //   size_t len = sequence.length();
	//for(size_t i = 0; i < len / 2; ++i)
 //   {
 //       swap(sequence[i], sequence[len - 1 - i]);
 //   }
//------------------------


    //cout<<"target-"<<sequence<<endl;
    if(str.length() <= 1)
	{
        return;
	} else if(str.length() == 2) {
        swap(str[0], str[1]);
        return;
    }
    size_t len = str.length();
    size_t idx = len / 2;
    string subStr = str.substr(idx);
    str.erase(str.begin()+idx, str.end());
    for(size_t i = 0; i < idx/2; ++i)
    {
        swap(str[i], str[idx - 1 - i]);
    }
    for(size_t i = 0; i < subStr.length()/2; ++i)
    {
        swap(subStr[i], subStr[subStr.length() - 1 - i]);
    }
    str.append(subStr);
    //cout<<"decoy-"<<sequence<<endl;
	
//------------------------------------
/*	
	size_t len = sequence.length();
    char lastCh = sequence[len - 1];
    sequence.erase(sequence.end() - 1, sequence.end());
	sequence.insert(sequence.begin(), lastCh);
*/

}

int CStringProcess::find_first_of(const char *s, char ch, int startPos, int len)
{
	for (int i = startPos; i < len; ++i){
		if (s[i] == ch){
			return i;
		}
	}
	return -1;
}


/**
* SearchState
**/
SearchState::SearchState(size_t tThreadSize) :
m_bSet(false), m_tStartClock(clock()), m_tStartTime(0),
m_vThreadState(tThreadSize, ThreadState())
{
	time(&m_tStartTime);
}

SearchState::~SearchState()
{
}

string SearchState::getProgress()
{
	ostringstream oss;
	string strBlanks("      ");
	oss << "Searching...\n"
		<< strBlanks << "Start Time: " << ctime(&m_tStartTime) << "\n"
		<< strBlanks << "Time cost: " << (clock() - m_tStartClock)*1.0 / CLOCKS_PER_SEC << "s\n";
	size_t tCompleted = m_stState.m_tCurrentSpectra;
	for (int i = 0; i < m_vThreadState.size(); ++i){
		oss << strBlanks << "Tread" << m_vThreadState[i].m_tID << ": "
			<< m_vThreadState[i].m_tCurrentSpectra << " / " << m_vThreadState[i].m_tTotalSpectra << "\n";
		tCompleted += m_vThreadState[i].m_tCurrentSpectra;
	}

	oss << strBlanks << "Total: " << tCompleted << " / " << m_stState.m_tTotalSpectra << "\n";
	return oss.str();
}

void SearchState::setStateID(size_t tID)
{
	while (m_bSet) {}
	m_bSet = true;
	m_stState.m_tID = tID;
	m_bSet = false;
}

size_t SearchState::getStateID() const
{
	return m_stState.m_tID;
}

void SearchState::setTotalSpectra(size_t tTotal)
{
	while (m_bSet){}
	m_bSet = true;
	m_stState.m_tTotalSpectra = tTotal;
	m_bSet = false;
}
void SearchState::setCurrentSpectra(size_t tCurrent)
{
	while (m_bSet) {}
	m_bSet = true;
	m_stState.m_tCurrentSpectra += tCurrent;
	m_bSet = false;
}
void SearchState::setThreadTotalSpectra(size_t tID, size_t tTotal)
{
	while (m_bSet){}
	m_bSet = true;
	m_vThreadState[tID].m_tTotalSpectra = tTotal;
	m_bSet = false;
}
void SearchState::setThreadCurrentSpectra(size_t tID, size_t tCurrent)
{
	while (m_bSet) {}
	m_bSet = true;
	m_vThreadState[tID].m_tCurrentSpectra = tCurrent;
	m_bSet = false;
}
clock_t SearchState::getStartClock() const
{
	return m_tStartClock;
}
time_t SearchState::getStartTime() const
{
	return m_tStartTime;
}

/*
* ThreadMonitor
*/

ThreadMonitor::ThreadMonitor(size_t tThreadNo)
	:m_vFlowFlag(tThreadNo, true), m_tCurrentThreadID(0), m_bLocked(false)
{
	pthread_mutex_init(&m_stMutex, NULL);
	pthread_cond_init(&m_stCond, NULL);
}

ThreadMonitor::~ThreadMonitor()
{
	pthread_mutex_destroy(&m_stMutex);
	pthread_cond_destroy(&m_stCond);
}

bool ThreadMonitor::isAllBusy()
{
	for (size_t i = 0; i < m_vFlowFlag.size(); ++i) {
		if (m_vFlowFlag[i]) {
			m_tCurrentThreadID = i;
			return false;
		}
	}
	return true;
}

void ThreadMonitor::waitForLock()
{

	while (m_bLocked) {
#ifdef _WIN32
		Sleep(100);  // 毫秒
#else
		usleep(100000); // 微秒
#endif
	}
}