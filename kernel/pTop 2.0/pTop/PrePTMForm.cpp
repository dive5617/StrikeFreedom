/*
 *  PrePTMForm.cpp
 *
 *  Created on: 2013-10-16
 *  Author: luolan
 */

#include <iostream>
#include <cstdio>
#include <set>
#include <vector>
#include <ctime>
#include <cstring>
#include <string>
#include <algorithm>

#include "config.h"
#include "PrePTMForm.h"

//#define _DEBUG_MOD

using namespace std;

CPrePTMForm::CPrePTMForm():
m_ModNum(0), m_nNCmodNum(0), m_nMaxModNum(10), m_lfMaxModMass(500), m_lfMaxNtermVarMass(0), m_lfMaxCtermVarMass(0)
{
	m_clog = Clog::getInstance();
}

CPrePTMForm::CPrePTMForm(CConfiguration *cPara):
	m_ModNum(0), m_nNCmodNum(0), m_nMaxModNum(cPara->m_nMaxModNum), m_lfMaxModMass(cPara->m_lfMaxModMass), m_lfMaxNtermVarMass(0), m_lfMaxCtermVarMass(0),
	m_vVarModFormCnt(2 * MAX_SHIFT_MASS + 2, 0),  m_ullVarModFormNum(0)
{
	m_clog = Clog::getInstance();
}

CPrePTMForm::~CPrePTMForm()
{
	if (m_clog){
		m_clog = NULL;
	}
}

// Get the combinational PTM forms by recursion
// PTM forms stored in a vector of PTM No.(for some PTMs have the same mass shift)
// @2016.06.27 to allow negative mass modification, we add an offset.
//void CPrePTMForm::_Combination(int pos, int start, int mass)
//{
//	if(mass < MAX_SHIFT_MASS && mass > -MAX_SHIFT_MASS)
//    {
//		m_ModForms[mass+MAX_SHIFT_MASS].push_back(m_Rcd);
//    }
//    for(int idx = start; idx < m_vVarmodInfo.size(); ++idx)
//    {
//        if(m_ModCnt[idx] > 0)
//        {
//            --m_ModCnt[idx];
//			m_Rcd.push_back(m_vVarmodInfo[idx]);
//            if((int)m_Rcd.size() <= m_nMaxModNum && abs(mass + m_ModMass[idx]) < MAX_SHIFT_MASS)
//			{
//                _Combination(pos + 1, idx, mass + m_ModMass[idx]);
//			}
//            ++m_ModCnt[idx];
//            m_Rcd.pop_back();
//        }
//    }
//}


// 删除与固定修饰冲突的可变修饰类型
void CPrePTMForm::_CheckVarMod()
{
	for(size_t i = 0; i < m_vFixmodInfo.size(); ++i)
	{
		if(m_vUnimod[m_vFixmodInfo[i]].cType == MOD_TYPE::MOD_NORMAL)
		{
			string sites = m_vUnimod[m_vFixmodInfo[i]].strSite;
			size_t j = 0;
			while(j < m_vVarmodInfo.size())
			{
				if (m_vUnimod[m_vVarmodInfo[j]].cType == MOD_TYPE::MOD_NORMAL)
				{
					size_t s = 0;
					while (s < m_vUnimod[m_vVarmodInfo[j]].strSite.length())
					{
						if (sites.find(m_vUnimod[m_vVarmodInfo[j]].strSite[s]) != string::npos)
							m_vUnimod[m_vVarmodInfo[j]].strSite.erase(s);
						++s;
					}
					if (m_vUnimod[m_vVarmodInfo[j]].strSite.length() == 0)
					{
						m_clog->warning(m_vUnimod[m_vVarmodInfo[j]].strName + " conflicts with " + m_vUnimod[m_vFixmodInfo[i]].strName);
						m_vVarmodInfo.erase(m_vVarmodInfo.begin()+j);
					}
					else ++j;
				} else ++j;
			}
		}
		else if (m_vUnimod[m_vFixmodInfo[i]].cType == MOD_TYPE::MOD_PRO_N) {
			size_t j = 0;
			while(j < m_vVarmodInfo.size())
			{
				if (m_vUnimod[m_vVarmodInfo[j]].cType == MOD_TYPE::MOD_PRO_N)
				{
					m_clog->warning(m_vUnimod[m_vVarmodInfo[j]].strName + " conflicts with " + m_vUnimod[m_vFixmodInfo[i]].strName);
					m_vVarmodInfo.erase(m_vVarmodInfo.begin() + j);
				} else ++j;
			}
		} else {
			size_t j = 0;
			while(j < m_vVarmodInfo.size())
			{
				if (m_vUnimod[m_vVarmodInfo[j]].cType == MOD_TYPE::MOD_PRO_C)
				{
					m_clog->warning(m_vUnimod[m_vVarmodInfo[j]].strName + " conflicts with " + m_vUnimod[m_vFixmodInfo[i]].strName);
					m_vVarmodInfo.erase(m_vVarmodInfo.begin() + j);
				} else ++j;
			}
		}  // end if modType
	}// end for fixmod
}

void CPrePTMForm::SetMaxModNum(const int cnt)
{
	m_nMaxModNum = cnt;
}

void CPrePTMForm::getModInfo(const string strVal, MODINI &tmpMod, unordered_map<string, double> &eleMass, vector<LABEL_QUANT> &m_quant, int passNumber)
{
	vector<string> parts;
	CStringProcess::Split(strVal, " ", parts);
	tmpMod.strSite = parts[0];
	if (parts[1].compare("NORMAL") == 0)
		tmpMod.cType = MOD_TYPE::MOD_NORMAL;
	else if (parts[1].compare("PRO_N") == 0)
		tmpMod.cType = MOD_TYPE::MOD_PRO_N;
	else if (parts[1].compare("PRO_C") == 0)
		tmpMod.cType = MOD_TYPE::MOD_PRO_C;
	else {
		cout << "[Error] Invalid modification type: " << parts[1] << endl;
		exit(-1);
	}
	tmpMod.lfMass = atof(parts[2].c_str());
	// [wrm] neutral loss info added by wrm. 2016.02.29
	int neutral = atoi(parts[4].c_str());
	for (int i = 0; i<neutral; ++i){
		tmpMod.vNeutralLoss.push_back(atof(parts[5 + 2 * i].c_str()));
	}
}

void CPrePTMForm::readCommonMods(vector<string> &commonMods)
{
	string str_com_mod_path = "common_mods.ini";
	if (access(str_com_mod_path.c_str(), 0) != 0)
	{
		m_clog->error(string("Invalid modification file: ") + str_com_mod_path + "\n");
		exit(1);
	}
	ifstream if_in(str_com_mod_path.c_str());
	string strline;
	// read mod
	while (1){
		getline(if_in, strline);
		if (!if_in)  break;
		size_t tPos = strline.find('=');
		if (tPos != string::npos){
			string strKey = strline.substr(0, tPos);
			string strVal = strline.substr(tPos + 1);
			if (strKey == "@NUMBER_MODIFICATION"){
				continue;
			}
			if (strKey.find("name") != string::npos){  //
				commonMods.push_back(strVal);
			}
		}
	}
	if_in.close();

}

//@wrm.2016.11.15
void CPrePTMForm::SetModInfo(CConfiguration *cPara, unordered_map<string, double> &eleMass, int passNumber)
{
	try{
		string str_mod_path = "modification.ini";
		if (passNumber > -1){
			char modpath[2*READLEN];
			int len = 0;
			len = sprintf(modpath, "%s\\%d.mod", cPara->m_strOutputPath.c_str(), passNumber + 1);
			modpath[len] = '\0';
			str_mod_path = modpath;
		}
		m_clog->info(string("Reading modifications from ") + str_mod_path);
		if (access(str_mod_path.c_str(), 0) != 0)
		{
			m_clog->error(string("Invalid modification file: ") + str_mod_path);
			exit(1);
		}

		vector<LABEL_QUANT> &m_quant = cPara->m_quant;
		vector<string> fixmod = cPara->m_vFixMod;
		vector<string> varmod = cPara->m_vVarMod;
		ifstream if_in(str_mod_path.c_str());
		string strline;
		unordered_map<string, int> modNames;
		// read mod
		while (1){
			getline(if_in, strline);
			if (!if_in)  break;
			size_t tPos = strline.find('=');
			if (tPos != string::npos){
				string strKey = strline.substr(0, tPos);
				string strVal = strline.substr(tPos + 1);
				if (strKey.find("name") != string::npos){  //
					continue;
				}
				if (strKey == "@NUMBER_MODIFICATION"){
					continue;
				}
				if (strKey == "label_name"){
					continue;
				}
				MODINI tmpMod;
				tmpMod.strName = strKey;
				size_t pos = strKey.find('#');
				if (pos != string::npos){
					tmpMod.strName = strKey.substr(0, pos);
				}
				getModInfo(strVal, tmpMod, eleMass, m_quant, passNumber);
				if (abs(tmpMod.lfMass) <= m_lfMaxModMass){  // TODO?  tmpMod.lfMass <= 200.0 && tmpMod.lfMass > 0
					m_vUnimod.push_back(tmpMod);
					modNames.insert(make_pair(tmpMod.strName, (int)m_vUnimod.size() - 1));
				}
				
			}	
		}
		if_in.close();

		m_vFixmodInfo.clear();
		m_vVarmodInfo.clear();
		m_vCommonMods.clear();
		// get information of fix modification 
		for (size_t i = 0; i < fixmod.size(); ++i)
		{
			if (modNames.find(fixmod[i]) != modNames.end())
			{
				m_vFixmodInfo.push_back(modNames[fixmod[i]]);
			}
			else {
				m_clog->warning("Can not find the modification " + fixmod[i] + ", please check the modification.ini!");
			}
		}

		// get information of variable modification
		for (size_t i = 0; i < varmod.size(); ++i)
		{
			if (modNames.find(varmod[i]) != modNames.end())
			{
				// TODO: get the nutral loss
				//cout<<modName<<" "<<tmpMod.strSite<<" "<<tmpMod.cType<<" "<<tmpMod.lfMass<<endl;
				m_vVarmodInfo.push_back(modNames[varmod[i]]);
				if (m_vUnimod[modNames[varmod[i]]].cType == MOD_TYPE::MOD_PRO_N){
					if (m_lfMaxNtermVarMass == 0){
						m_lfMaxNtermVarMass = m_vUnimod[modNames[varmod[i]]].lfMass;
					}
					else{
						m_lfMaxNtermVarMass = max(m_lfMaxNtermVarMass, m_vUnimod[modNames[varmod[i]]].lfMass);
					}
				}
				else if (m_vUnimod[modNames[varmod[i]]].cType == MOD_TYPE::MOD_PRO_C){
					if (m_lfMaxCtermVarMass == 0){
						m_lfMaxCtermVarMass = m_vUnimod[modNames[varmod[i]]].lfMass;
					}
					else{
						m_lfMaxCtermVarMass = max(m_lfMaxCtermVarMass, m_vUnimod[modNames[varmod[i]]].lfMass);
					}
				}
			}
			else {
				m_clog->warning("[Warning] Can not find the modification \"" + varmod[i] + "\", please check the modification.ini!");
			}
		}

		// get information of common modification
		vector<string> strCommonMods;
		readCommonMods(strCommonMods);
		for (size_t i = 0; i < strCommonMods.size(); ++i)
		{
			if (modNames.find(strCommonMods[i]) != modNames.end())
			{
				m_vCommonMods.push_back(modNames[strCommonMods[i]]);
			}
		}

		m_ModNum = m_vUnimod.size();
		_CheckVarMod();
	}
	catch (exception &e){
		CErrInfo info("CPrePTMForm", "SetModInfo()", "Get modification information failed.");
		throw runtime_error(info.Get(e).c_str());
	}
	catch (...){
		CErrInfo info("CPrePTMForm", "SetModInfo()", "Caught an unknown exception from getting modifications.");
		throw runtime_error(info.Get().c_str());
	}
}

// Accoding to the file config.ini, initialize the member data of the class
void CPrePTMForm::InitGivenModInfo()
{
    // get the variable mod information
	m_nNCmodNum = 0;
	m_ModMass.clear();
    for(int i = 0; i < m_vVarmodInfo.size(); ++i)
    {
		int nMass = int(m_vUnimod[m_vVarmodInfo[i]].lfMass + 0.5);
        m_ModMass.push_back(nMass);

		if (m_vUnimod[m_vVarmodInfo[i]].cType == MOD_TYPE::MOD_PRO_N)
        {
			++m_nNCmodNum;
			m_vNtermMod.push_back(m_vVarmodInfo[i]);
            m_ModCnt.push_back(1);
		}
		else if (m_vUnimod[m_vVarmodInfo[i]].cType == MOD_TYPE::MOD_PRO_C) {
			++m_nNCmodNum;
			m_vCtermMod.push_back(m_vVarmodInfo[i]);
            m_ModCnt.push_back(1);
        } else {
			for (int cIdx = 0; cIdx < (int)m_vUnimod[m_vVarmodInfo[i]].strSite.length(); ++cIdx)
			{
				m_vVarModAA2ID[m_vUnimod[m_vVarmodInfo[i]].strSite[cIdx] - 'A'].push_back(m_vVarmodInfo[i]);
			}
            m_ModCnt.push_back(m_nMaxModNum);
        }
    }
}

// Get all the combinational PTM forms through the given known PTMs
//void CPrePTMForm::CalculateModForm(int unexpectedModNum)
//{
//	time_t t_start = clock();
//	m_clog->info("Calculate Modification Forms...");
//	m_clog->info("Max mass shift is %d", MAX_SHIFT_MASS);
//	InitGivenModInfo();
//    _Combination(0, 0, 0);  // var mods
//    //-----------------------------------------------
//    //Display all the combinational forms and its mass
//#ifdef _DEBUG_MOD
//    for(int i = 0; i < MAX_SHIFT_MASS; ++i)
//    {
//        if(m_ModForms[i].size())
//        {
//            cout<<"[Debug] mass="<<i<<":"<<m_ModForms[i].size()<<endl;
//            for(int j = 0; j < (int)m_ModForms[i].size(); ++j)
//            {
//                for(int t = 0; t < (int)m_ModForms[i][j].size(); ++t)
//                    cout<<m_ModForms[i][j][t]<<" ";
//                cout<<endl;
//            }
//        }
//    }
//#endif
//	//---------------------------------------------------------
//
//	for (int i = 0; i < 26; ++i){
//		m_MapModAA2ID[i] = m_vVarModAA2ID[i];
//	}
//}

void CPrePTMForm::_Combination(int start, int mass, bool isCnt)
{
	if(mass <= MAX_SHIFT_MASS && mass >= -MAX_SHIFT_MASS){
		int massIdx = mass + MAX_SHIFT_MASS;
		if (isCnt){
			++m_ullVarModFormNum;
			++m_vVarModFormCnt[massIdx];
		}
		else{
			--m_vVarModFormCnt[massIdx];
			m_vVarModForms[m_vVarModFormCnt[massIdx]] = m_Rcd;
		}
    }
    for(int idx = start; idx < m_vVarmodInfo.size(); ++idx){
        if(m_ModCnt[idx] > 0){
            --m_ModCnt[idx];
			m_Rcd.push_back(m_vVarmodInfo[idx]);
            if((int)m_Rcd.size() <= m_nMaxModNum && abs(mass + m_ModMass[idx]) <= MAX_SHIFT_MASS)
			{
                _Combination(idx, mass + m_ModMass[idx], isCnt);
			}
            ++m_ModCnt[idx];
            m_Rcd.pop_back();
        }
    }
}

void CPrePTMForm::CreatePTMIndex(int unexpectModNum)
{
	time_t t_start = clock();
	m_clog->info("Calculate Modification Forms...");
	m_clog->info("Max mass shift is %d", MAX_SHIFT_MASS);
	InitGivenModInfo();
	/* index for var mods */
	/*count*/
	_Combination(0, 0, true);
	/* accumulate counts */
	for (size_t tIdx = 1; tIdx <= 2*MAX_SHIFT_MASS; ++tIdx)
		m_vVarModFormCnt[tIdx] += m_vVarModFormCnt[tIdx - 1];
	if (m_vVarModFormCnt[2*MAX_SHIFT_MASS] > INT_MAX) {
		ostringstream oss;
		oss << "bad allocation! you want to require " << m_vVarModFormCnt[2 * MAX_SHIFT_MASS]
			<< "bytes, this program forbid to allocate more than 2GB.\n";
		throw runtime_error(oss.str());
	}
	m_vVarModFormCnt[2 * MAX_SHIFT_MASS + 1] = m_ullVarModFormNum;
	m_vVarModForms.resize(m_vVarModFormCnt[2 * MAX_SHIFT_MASS]);
	/*fill*/
	_Combination(0, 0, false);

	for (int i = 0; i < 26; ++i){
		m_MapModAA2ID[i] = m_vVarModAA2ID[i];
	}

	if (unexpectModNum > 0){
		/* index for mods with at least one unexpected mod */
		_CommonModIndex(unexpectModNum);
	}
	time_t t_end = clock();
	m_clog->info("Execution time for Calculating Modification Forms: %fs", (double)(t_end - t_start) / 1000.0);
}

/* index for mods with at least one unexpected mod */
void CPrePTMForm::_CommonModIndex(const int unexpectModNum)
{
	if (unexpectModNum > 2){
		m_clog->info("Sorry, pTop now support at most two unexpected modifications.");
	}
	for (int i = 0; i <= 2 * MAX_SHIFT_MASS; ++i){
		m_vModForms[i].clear();
	}
	unordered_set<int> varIDs;
	for (int i = 0; i < m_vVarmodInfo.size(); ++i){
		varIDs.insert(m_vVarmodInfo[i]);
	}
	for (int i = 0; i < m_vCommonMods.size(); ++i){
		if (varIDs.find(m_vCommonMods[i]) != varIDs.end()){  // it is a var mod
			continue;
		}
		MODINI &modini = m_vUnimod[m_vCommonMods[i]];
		int nMassIdx = int(modini.lfMass + 0.5) + MAX_SHIFT_MASS;
		vector<int> tmp(1, m_vCommonMods[i]);
		m_vModForms[nMassIdx].push_back(tmp);
		for (int cIdx = 0; cIdx < (int)modini.strSite.length(); ++cIdx)
		{
			m_MapModAA2ID[modini.strSite[cIdx] - 'A'].push_back(m_vCommonMods[i]);
		}
		if (unexpectModNum > 1){
			tmp.push_back(-1);
			for (int j = i; j < m_vCommonMods.size(); ++j){
				if (varIDs.find(m_vCommonMods[j]) != varIDs.end()){   // it is a var mod
					continue;
				}
				MODINI &mod2 = m_vUnimod[m_vCommonMods[j]];
				int massSumIdx = int(modini.lfMass + mod2.lfMass + 0.5) + MAX_SHIFT_MASS;
				tmp.back() = m_vCommonMods[j];
				if (massSumIdx <= 2*MAX_SHIFT_MASS){
					m_vModForms[massSumIdx].push_back(tmp);
				}
			}
		}
	}
	vector<pair<int, vector<vector<int> >>> unimods;
	vector<int> varmods;
	for (int i = 0; i <= 2 * MAX_SHIFT_MASS; ++i){
		if (m_vVarModFormCnt[i] < m_vVarModFormCnt[i+1]){
			varmods.push_back(i - MAX_SHIFT_MASS);
		}
		if (!m_vModForms[i].empty()){
			unimods.push_back(make_pair(i - MAX_SHIFT_MASS, m_vModForms[i]));
		}
	}
	m_clog->info("Number of Variable Modification Mass: %d", varmods.size());
	m_clog->info("Number of %d Unimod Modification Mass: %d", unexpectModNum, unimods.size());

	for (int i = 0; i < unimods.size(); ++i){
		int mass1 = unimods[i].first;
		for (int j = 0; j < varmods.size(); ++j){
			int mass2 = varmods[j];
			int massIdx = mass1 + mass2 + MAX_SHIFT_MASS;
			if (massIdx <= 2 * MAX_SHIFT_MASS){
				vector<vector<int>> comV;
				for (auto u = unimods[i].second.begin(); u != unimods[i].second.end(); ++u){
					for (auto v = m_vVarModFormCnt[varmods[j]]; v < m_vVarModFormCnt[varmods[j]+1]; ++v){
						if (u->size() + m_vVarModForms[v].size() <= m_nMaxModNum){
							vector<int> tmp(*u);
							tmp.insert(tmp.end(), m_vVarModForms[v].begin(), m_vVarModForms[v].end());
							comV.push_back(tmp);
						}
					}
				}
				m_vModForms[massIdx].insert(m_vModForms[massIdx].end(), comV.begin(), comV.end());
			}
		}
		
	}

	//test
	//FILE *fmod = fopen("modForm.txt", "w");
	//for (int i = 0; i < 2 * MAX_SHIFT_MASS; ++i){
	//	if (!m_vModForms[i].empty()){
	//		fprintf(fmod, "%d %d\n", i - MAX_SHIFT_MASS, m_vModForms[i].size());
	//	}
	//}
	//fclose(fmod);

}

void CPrePTMForm::GetVarModForms(int massIdx, vector<vector<int>> &modForms)
{
	if (massIdx > 2 * MAX_SHIFT_MASS)  return;
	if (m_vVarModFormCnt[massIdx] < m_vVarModFormCnt[massIdx + 1]){
		modForms.insert(modForms.end(), m_vVarModForms.begin() + m_vVarModFormCnt[massIdx], m_vVarModForms.begin() + m_vVarModFormCnt[massIdx + 1]);
	}
}

// return total unimod number
int CPrePTMForm::GetModNum() const
{
	return m_vUnimod.size();
}

int CPrePTMForm::GetNCModNum()
{
	return m_nNCmodNum;
}

// 
double CPrePTMForm::GetMassbyID(const int idx)
{
	if(idx >= m_ModNum)
	{
		m_clog->error("Invalid mod ID!");
		return 0.0;
	}
	else return m_vUnimod[idx].lfMass;
}

string CPrePTMForm::GetNamebyID(const int idx)
{
	if(idx >= m_ModNum)
	{
		m_clog->error("Invalid mod ID!");
		return "*";
	}
	else return m_vUnimod[idx].strName;
}

void CPrePTMForm::GetSitesbyID(const int idx, string &sites)
{
	if (idx >= m_ModNum)
	{
		m_clog->error("Invalid mod ID!");
		return;
	}
	else{
		sites = m_vUnimod[idx].strSite;
	}
}

MODINI CPrePTMForm::GetMODINIbyID(const int idx)
{
	if (idx >= m_ModNum)
	{
		m_clog->error("Invalid mod ID %d!",idx);
		exit(1);
	}
	else return m_vUnimod[idx];
}

void CPrePTMForm::GetFixModInfo(vector<MODINI> &fixmodInfo)
{
	for (int i = 0; i < m_vFixmodInfo.size(); ++i){
		fixmodInfo.push_back(m_vUnimod[m_vFixmodInfo[i]]);
	}
}

void CPrePTMForm::GetAllIDsbyAA(const char aa, vector<int> &vModIDs)
{
	vModIDs.clear();
	vModIDs = m_MapModAA2ID[aa - 'A'];
}

void CPrePTMForm::GetVarIDsbyAA(const char aa, vector<int> &vModIDs)
{
	vModIDs.clear();
	vModIDs = m_vVarModAA2ID[aa - 'A'];
}

vector<int> & CPrePTMForm::GetNtermMods()
{
	return m_vNtermMod;
}
vector<int> & CPrePTMForm::GetCtermMods()
{
	return m_vCtermMod;
}

bool CPrePTMForm::isNtermMod(const int idx)
{
	return m_vUnimod[idx].cType == MOD_TYPE::MOD_PRO_N;
}

bool CPrePTMForm::isCtermMod(const int idx)
{
	return m_vUnimod[idx].cType == MOD_TYPE::MOD_PRO_C;
}

double CPrePTMForm::GetMaxNtermVarMass()
{
	return m_lfMaxNtermVarMass;
}
double CPrePTMForm::GetMaxCtermVarMass()
{
	return m_lfMaxCtermVarMass;
}
