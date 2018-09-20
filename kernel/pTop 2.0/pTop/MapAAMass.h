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
	double m_lfNterm;			// ������N�˵��������̶�����
	double m_lfCterm;			// ������N�˵��������̶�����
	string m_strNtermSites;		// ������N�˵Ĺ̶����η����İ���������
	string m_strCtermSites;		// ������C�˵Ĺ̶����η����İ���������
	vector<double> m_vAAMonoMass;	// ���������mono����
	vector<vector<pair<string,int>>> m_vAAComp;  // ������������(C,H,N,O,S)
	
	unordered_map<string, double> &m_ElemMass;

	Clog *m_clog;

public:
	vector<double> m_vAAMass;   // for SearchEngine
	vector<byte> m_vNeutralLossAA; //for SearchEngine


	CMapAAMass(unordered_map<string, double> &elemMass);
    ~CMapAAMass();

	void Split(string &str, string delim, vector<string > &ret);
	void Init(vector<LABEL_QUANT> &m_quant, int passNumber, string outpath);									// ��ʼ������ȡaa.ini�ļ�����26�������ἰ���Ӧ�������洢��map��

    double CalculateMass(const char sq[]);			// �������백�����ַ�����������+ˮ+����
    double CalculateNeutralMass(const string &sq);	// �������백�����ַ����Ĳл�������
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
