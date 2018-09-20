/*
 *  PrePTMForm.h
 *
 *  Created on: 2013-10-16
 *  Author: luolan
 */

#ifndef PREPTMFORM_H
#define PREPTMFORM_H

#include <iostream>
#include <cstdio>
#include <vector>
#include <map>
#include <unordered_set>
#include <ctime>
#include <cstring>
#include <algorithm>

#include "predefine.h"

using namespace std;

class CPrePTMForm
{
private:
    int m_ModNum;
	int m_nNCmodNum;
    int m_nMaxModNum;
    vector<int> m_ModCnt;  // times each mod can happen on one protein
    vector<int> m_Rcd;
	vector<int> m_ModMass;

	vector<int> m_vNtermMod;
	vector<int> m_vCtermMod;

	vector<MODINI> m_vFixmodInfo;
	vector<MODINI> m_vVarmodInfo;

    multimap<char, int> m_MapModAA2ID;   // mod IDs available on each acid


private:
    void _Combination(int pos, int start, int mass);
	void _CheckVarMod();

public:
    vector<vector<int> > m_ModForms[2*MAX_SHIFT_MASS];

	CPrePTMForm();
    ~CPrePTMForm();

	void SetMaxModNum(const int cnt);
	void SetModInfo(vector<string> &fixmod, vector<string> &varmod);

	int GetModNum();
	int GetNCModNum();
	double GetMassbyID(const int idx);
	string GetNamebyID(const int idx);
	void GetFixModInfo(vector<MODINI> &fixmodInfo);
	void GetAllIDsbyAA(const char aa, vector<int> &vModIDs);

	vector<int> & GetNtermMods();
	vector<int> & GetCtermMods();

	bool isNtermMod(const int idx);
	bool isCtermMod(const int idx);
	
    void InitGivenModInfo();
    void CalculateModForm();
};
#endif

