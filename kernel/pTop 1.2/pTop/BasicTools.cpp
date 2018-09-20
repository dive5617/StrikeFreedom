#include <iostream>
#include <sstream>
#include <fstream>
#include <cstring>
#include <string>
#include <stdexcept>
#include <time.h>

#include "BasicTools.h"

using namespace std;

/*
**	class Clog
*/

Clog* Clog::m_plog = NULL;

Clog::Clog(const char* logPath, LOG_LEVEL erank):m_logLevel(erank)
{
	if(logPath == NULL){
		
	}else{
		m_pFile = fopen(logPath, "a+");
		if(!m_pFile){
			CErrInfo err("Clog","Clog","Failed to open log file");
			throw runtime_error(err.Get());
		}
	}
}

Clog* Clog::getInstance(const char *logPath, LOG_LEVEL eRank)
{
	destroyInstance();
	if(m_plog == NULL){
		m_plog = new Clog(logPath, eRank);
	}
	return m_plog;
}

Clog* Clog::getInstance(LOG_LEVEL eRank, bool bReset)
{
	if(bReset)
		destroyInstance();

	if(m_plog == NULL){
		m_plog = new Clog(NULL, eRank);
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

void Clog::alert(const std::string &strAlert)
{
	string strStr = "[warning] " + strAlert;
	showInfo(strStr, LL_WARN);
}

void Clog::alert(const char *strFormat, ...)
{
	va_list ap;
	va_start(ap, strFormat);
	char szBuf[STR_BUF_SIZE];
	vsprintf(szBuf, strFormat, ap);
	va_end(ap);
	alert(string(szBuf));
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
	showInfo(strStr, LL_DEBUG);
}

void Clog::debug(const char *strFormat, ...)
{
	va_list ap;
	va_start(ap, strFormat);
	char szBuf[STR_BUF_SIZE];
	vsprintf(szBuf, strFormat, ap);
	va_end(ap);
	debug(string(szBuf));
}
	
void Clog::breakPoint()
{
	string strStr = "[stop] Press any key to continue ...";
	showInfo(strStr, LL_DEBUG);
	getchar();
}

Clog::~Clog()
{
	if(m_plog){
		delete m_plog;
		m_plog = NULL;
	}
	if(m_pFile){
		fclose(m_pFile);
		//delete m_pFile;
		m_pFile = NULL;
	}
}

void Clog::showInfo(const string &strStr, LOG_LEVEL eRank)
{
	if(m_logLevel >= eRank){
		fprintf(m_pFile,"%s\n",strStr.c_str());
		fflush(m_pFile);
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