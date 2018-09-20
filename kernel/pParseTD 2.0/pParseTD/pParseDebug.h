#ifndef _PPARSEDEBUG_H_
#define _PPARSEDEBUG_H_
#include <iostream>
#include <cstdio>
#include <cstdlib>
#include <fstream>
#include <vector>

using namespace std;

class pParseDebug
{
public:
	pParseDebug();
	~pParseDebug();
	void Display();
	bool IsExist(int scan);
protected:
	void _LoadScan();
	void _parse();
private:
	vector<int> m_vnScanlist;
	string m_strScanlistfile;

};

#endif