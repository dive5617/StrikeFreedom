// We need some better function to process strings 
// 
#include "StringProcessor.h"
/////////////////////////////////////////////////
///////////////// CStringProcess ////////////////
/////////////////////////////////////////////////
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

void CStringProcess::Split(const string str, const string &delim, vector<string> &ret)
{
	ret.clear();
	size_t last = 0;  
    size_t index = str.find_first_of(delim, last);  
    while(index != string::npos)  
    {  
        ret.push_back(str.substr(last, index - last));  
		last = index + delim.length();  
        index = str.find_first_of(delim, last);  
    } 
	index = str.length();
	while(str[index-1] == '\n' || str[index-1] == '\r'){
		-- index;
	}
	if (index - last > 0)  
    {  
        ret.push_back(str.substr(last, index - last));  
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