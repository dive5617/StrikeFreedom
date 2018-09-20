/*
 *  MapAAMass.cpp
 *
 *  Created on: 2013-6
 *  Author: luolan
 */

#include <iostream>
#include <cstring>
#include <map>

#include "MapAAMass.h"
#include "config.h"

using namespace std;

CMapAAMass::CMapAAMass(unordered_map<string, double> &elemMass) :m_lfNterm(0.0), m_lfCterm(0.0), 
m_strNtermSites(""), m_strCtermSites(""), m_ElemMass(elemMass)
{
	m_clog = Clog::getInstance();
	m_vAAMonoMass.clear();
}

CMapAAMass::~CMapAAMass()
{
	if (m_clog){
		m_clog = NULL;
	}
}


void CMapAAMass::Split(string &str, string delim, vector<string > &ret)  
{  
    size_t last = 0;  
    size_t index = str.find_first_of(delim, last);  
    while(index != string::npos)  
    {  
        ret.push_back(str.substr(last, index - last));  
        last = index + 1;  
        index = str.find_first_of(delim, last);  
    }  
    if (index - last > 0)  
    {  
        ret.push_back(str.substr(last, index - last));  
    }  
}  



// 初始化，读取aa.ini文件，将26个氨基酸及其对应的质量存储到m_vAAMonoMass中
void CMapAAMass::Init(vector<LABEL_QUANT> &m_quant, int passNumber, string outpath)
{
	try{
		string aa_path = "aa.ini";
		if (passNumber > -1){
			char aapath[READLEN];
			int len = 0;
			len = sprintf(aapath, "%s\\%d.aa", outpath.c_str(), passNumber + 1);
			aapath[len] = '\0';
			aa_path = aapath;
		}
		

		COptionTool config(aa_path);
		int num = config.GetInteger("", "@NUMBER_RESIDUE", 0);
		if (0 == num)
		{
			m_clog->error("There is no residue, please check aa.ini!");
			exit(1);
		}

		m_vAAMonoMass.assign(26, 0);
		m_vAAComp.resize(26);
		char name[READLEN]; //, fullName[READLEN];
		for (int i = 0; i < num; ++i)
		{
			sprintf(name, "R%d", i + 1);
			string comp = config.GetString("", name, "");
			string delim = "|";
			vector<string> parts;
			Split(comp, delim, parts); // A|C(3)H(5)N(1)O(1)S(0)|
			if (parts.size() < 2)
			{
				cout << "[Warning] Invalid residue: " << comp << endl;
				continue;
			}
			//cout << parts[0] << " " << parts[1] << endl;
			char aa = parts[0][0];
			if (aa < 'A' || aa > 'Z')
			{
				cout << "[Warning] Invalid residue: " << comp << endl;
				continue;
			}
			double lfMass = 0.0;
			string strComp = parts[1];
			parts.clear();
			delim = ")";
			Split(strComp, delim, parts); // C(3)H(5)N(1)O(1)S(0)
			vector<pair<string, int>> aa_comp;
			for (size_t j = 0; j < parts.size(); ++j)
			{
				if (parts[j].size() <= 0)
				{
					continue;
				}
				vector<string> one;
				delim = "(";
				Split(parts[j], delim, one);
				if (one.size() != 2)
				{
					continue;
				}

				auto it = m_ElemMass.find(one[0]);
				int eleNum = atoi(one[1].c_str());
				if (it != m_ElemMass.end())
				{
					double monoMass = it->second;
					lfMass += monoMass * eleNum;
				}
				aa_comp.push_back(make_pair(one[0], eleNum));
			}
			m_vAAMonoMass[(int)(aa - 'A')] = lfMass;
			m_vAAComp[(int)(aa - 'A')] = aa_comp;
			//cout << aa << " " << lfMass << endl;
		}  // end for amino acide
		m_lfNterm = 0.0;
		m_lfCterm = 0.0;
		m_strNtermSites = "";
		m_strCtermSites = "";
	}
	catch (exception &e){
		CErrInfo info("CMapAAMass", "Init()", "Get amino acid information failed.");
		throw runtime_error(info.Get(e).c_str());
	}
	catch (...){
		CErrInfo info("CMapAAMass", "Init()", "Caught an unknown exception from getting amino acids.");
		throw runtime_error(info.Get().c_str());
	}
}

// 计算输入氨基酸字符串的质量，+水+质子
double CMapAAMass::CalculateMass(const char sq[])
{
	if (0 == m_vAAMonoMass.size())
	{
		m_clog->error("Please init the AA mass before calling this function!");
		exit(-1);
	}
    int i, len = strlen(sq);
	if(0 == len) return 0.0;

    double total_mass = IonMass_Proton;
	if(m_lfNterm > 0.0 && m_strNtermSites.find_first_of(sq[0]) != string::npos)
		total_mass += m_lfNterm;

    for(i = 0; i < len; ++i)
    {
		int idx = sq[i] - 'A';
		if(idx > 26)
		{
			cout << "[Warning] Invalid amino acid: " << sq[i] << endl;
		}
		total_mass += m_vAAMonoMass[idx];
    }

    total_mass += 2*IonMass_Mono_H + IonMass_Mono_O;
	if(m_lfCterm > 0.0 && m_strCtermSites.find_first_of(sq[len-1]) != string::npos)
		total_mass += m_lfCterm;

    return total_mass;
}

// 计算输入氨基酸字符串的残基质量和
double CMapAAMass::CalculateNeutralMass(const string &sq)
{
	if (0 == m_vAAMonoMass.size())
	{
		cout << "[Error] Please init the object before calling this function!" << endl;
		exit(-1);
	}
    int i, len = sq.length();
    double total_mass = 0;

    for(i = 0; i < len; ++i)
    {
		int idx = sq[i] - 'A';
		if(idx > 26)
		{
			cout << "[Warning] Invalid amino acid: " << sq[i] << endl;
		}
		total_mass += m_vAAMonoMass[idx];
    }
    return total_mass;
}


double CMapAAMass::GetAAMass(const char ch)
{
	return m_vAAMonoMass[ch - 'A'];
}

void CMapAAMass::AddAAMassbyMod(const char ch, double modMass)
{
	m_vAAMonoMass[ch - 'A'] += modMass;
}

void CMapAAMass::SetNtermMass(const double modMass, const string &modSites)
{
	m_lfNterm = modMass;
	m_strNtermSites = modSites;
}
void CMapAAMass::SetCtermMass(const double modMass, const string &modSites)
{
	m_lfCterm = modMass;
	m_strCtermSites = modSites;
}

// return the mass of the N-term fix mod
double CMapAAMass::GetNtermMass(const char ch)  
{
	double mass = 0;
	if(m_lfNterm > 0.0 && m_strNtermSites.find_first_of(ch) != string::npos)
		mass += m_lfNterm;
	return mass;
}

// return the mass of the C-term fix mod
double CMapAAMass::GetCtermMass(const char ch)
{
	double mass = 0;
	if(m_lfCterm > 0.0 && m_strCtermSites.find_first_of(ch) != string::npos)
		mass += m_lfCterm;
	return mass;
}

void CMapAAMass::InitSearchParam(CPrePTMForm *pPTMIndex)
{
	m_vAAMass.clear();
	GetAllAAMasses(pPTMIndex);

	// neutral loss AA
	m_vNeutralLossAA.assign(26, 0);
	// -H2O:  D, E, S, T
	string lossH2O = "DEST";
	for (int i = 0; i < lossH2O.size(); ++i)
		m_vNeutralLossAA[lossH2O[i] - 'A'] = 1;
	// -NH3: K, N, Q, R
	string lossNH3 = "KNQR";
	for (int i = 0; i < lossNH3.size(); ++i)
		m_vNeutralLossAA[lossNH3[i] - 'A'] = 2;
}

void CMapAAMass::GetAllAAMasses(CPrePTMForm *pPTMIndex)
{
	m_vAAMass.clear();
	for (size_t i = 0; i < 26; ++i)
	{   //不加修饰的20种氨基酸
		m_vAAMass.push_back(m_vAAMonoMass[i]);
	}

	for (size_t i = 0; i < 26; ++i)
	{
		char ch = 'A' + i;
		vector<int> vModIDs;
		pPTMIndex->GetVarIDsbyAA(ch, vModIDs);   //get mod IDs that can happen at ch.
		for (size_t cIdx = 0; cIdx < (int)vModIDs.size(); ++cIdx)
		{   //加修饰的氨基酸
			double mass = m_vAAMonoMass[i] + pPTMIndex->GetMassbyID(vModIDs[cIdx]);
			m_vAAMass.push_back(mass);
		}
	}
}
