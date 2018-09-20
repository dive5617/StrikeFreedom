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
#include <unordered_map>
#include <unordered_set>
#include <ctime>
#include <cstring>
#include <algorithm>

#include "sdk.h"
#include "util.h"
#include "config.h"

using namespace std;

class CPrePTMForm
{
private:
    int m_ModNum;
	int m_nNCmodNum;
    int m_nMaxModNum;
	double m_lfMaxModMass;
	double m_lfMaxNtermVarMass;
	double m_lfMaxCtermVarMass;
    vector<int> m_ModCnt;  // times each mod can happen on one protein
    vector<int> m_Rcd;
	vector<int> m_ModMass;

	vector<int> m_vNtermMod;   // N-term mods in var mods
	vector<int> m_vCtermMod;   // C-term mods in var mods

	vector<int> m_vFixmodInfo;
	vector<int> m_vVarmodInfo;
	vector<int> m_vCommonMods;

	vector<MODINI> m_vUnimod;

	vector<int> m_vVarModAA2ID[26];  // var mod IDs available on each acid
	vector<int> m_MapModAA2ID[26];   // mod IDs available on each acid, including var mods and unexpected mods
	
	unsigned long long m_ullVarModFormNum;  // number of var mod forms
	vector<int> m_vVarModFormCnt; // count for var mod forms
	vector<vector<int>> m_vVarModForms;  // mod forms only including var mods

	//unsigned long long m_ullModFormNum;  // number of mod forms
	//vector<int> m_vModFormCnt;  // count for mod forms including at least one unexpected mods
	//vector<vector<int>> m_vModForms; // mod forms including at least one unexpected mods

	Clog *m_clog;

private:
    void _Combination(int pos, int start, int mass);
	void _Combination(int start, int mass, bool isCnt);
	void _CommonModIndex(const int unexpectModNum);
	void _CheckVarMod();
	void getModbyName(COptionTool &config, const string modName, MODINI &tmpMod, unordered_map<string, double> &eleMass, vector<LABEL_QUANT> &m_quant, int passNumber);
	void getModInfo(const string strVal, MODINI &tmpMod, unordered_map<string, double> &eleMass, vector<LABEL_QUANT> &m_quant, int passNumber);
	void readCommonMods(vector<string> &commonMods);

public:
    //vector<vector<int> > m_ModForms[2*MAX_SHIFT_MASS];  // **

	vector<vector<int>> m_vModForms[2 * MAX_SHIFT_MASS + 1];  // mod forms including at least one unexpected mods

	CPrePTMForm();
	CPrePTMForm(CConfiguration *cPara);
    ~CPrePTMForm();

	void SetMaxModNum(const int cnt);
	void SetModInfo(CConfiguration *cPara, unordered_map<string, double> &eleMass, int passNumber);

	int GetModNum() const;
	int GetNCModNum();
	double GetMassbyID(const int idx);
	string GetNamebyID(const int idx);
	MODINI GetMODINIbyID(const int idx);
	void GetSitesbyID(const int idx, string &sites);
	void GetFixModInfo(vector<MODINI> &fixmodInfo);
	void GetAllIDsbyAA(const char aa, vector<int> &vModIDs);
	void GetVarIDsbyAA(const char aa, vector<int> &vModIDs);

	double GetMaxNtermVarMass();
	double GetMaxCtermVarMass();

	vector<int> & GetNtermMods();
	vector<int> & GetCtermMods();

	bool isNtermMod(const int idx);
	bool isCtermMod(const int idx);
	
    void InitGivenModInfo();
    void CalculateModForm(int unexpectedModNum);

	void CreatePTMIndex(int unexpectModNum);
	void GetVarModForms(int massIdx, vector<vector<int>> &modForms);
};
#endif

