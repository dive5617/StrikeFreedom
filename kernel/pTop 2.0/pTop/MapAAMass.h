/*
 *  MapAAMass.h
 *
 *  Created on: 2013-6
 *  Author: luolan
 */

#ifndef MAPAAMASS_H
#define MAPAAMASS_H

#include <iostream>
#include <cstring>
#include <map>
#include <unordered_map>

#include "sdk.h"
#include "util.h"
#include "PrePTMForm.h"
using namespace std;

class CMapAAMass
{
private:
	double m_lfNterm;			// 蛋白质N端的质量，固定修饰
	double m_lfCterm;			// 蛋白质N端的质量，固定修饰
	string m_strNtermSites;		// 蛋白质N端的固定修饰发生的氨基酸限制
	string m_strCtermSites;		// 蛋白质C端的固定修饰发生的氨基酸限制
	vector<double> m_vAAMonoMass;	// 各氨基酸的mono质量
	vector<vector<pair<string,int>>> m_vAAComp;  // 各氨基酸的组成(C,H,N,O,S)
	
	unordered_map<string, double> &m_ElemMass;

	Clog *m_clog;

public:
	vector<double> m_vAAMass;   // for SearchEngine
	vector<byte> m_vNeutralLossAA; //for SearchEngine


	CMapAAMass(unordered_map<string, double> &elemMass);
    ~CMapAAMass();

	void Split(string &str, string delim, vector<string > &ret);
	void Init(vector<LABEL_QUANT> &m_quant, int passNumber, string outpath);									// 初始化，读取aa.ini文件，将26个氨基酸及其对应的质量存储到map中

    double CalculateMass(const char sq[]);			// 计算输入氨基酸字符串的质量，+水+质子
    double CalculateNeutralMass(const string &sq);	// 计算输入氨基酸字符串的残基质量和
	double GetAAMass(const char ch);

	void AddAAMassbyMod(const char ch, double modMass);
	void SetNtermMass(const double modMass, const string &modSites);
	void SetCtermMass(const double modMass, const string &modSites);
	double GetNtermMass(const char ch);
	double GetCtermMass(const char ch);

	void InitSearchParam(CPrePTMForm *pPTMIndex);  // for SearchEngine
	void GetAllAAMasses(CPrePTMForm *pPTMIndex);  // for SearchEngine
};

#endif
