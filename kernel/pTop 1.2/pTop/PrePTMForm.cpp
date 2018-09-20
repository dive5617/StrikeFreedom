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
#include <windows.h>

#include "Configuration.h"
#include "PrePTMForm.h"

//#define _DEBUG_MOD

using namespace std;

CPrePTMForm::CPrePTMForm():
	m_ModNum(0), m_nNCmodNum(0),m_nMaxModNum(10)
{

}

CPrePTMForm::~CPrePTMForm()
{

}

// Get the combinational PTM forms by recursion
// PTM forms stored in a vector of PTM No.(for some PTMs have the same mass shift)
// @2016.06.27 to allow negative mass modification, we add an offset.
void CPrePTMForm::_Combination(int pos, int start, int mass)
{
	if(mass < MAX_SHIFT_MASS && mass > -MAX_SHIFT_MASS)
    {
		m_ModForms[mass+MAX_SHIFT_MASS].push_back(m_Rcd);
    }
    for(int idx = start; idx < m_ModNum; ++idx)
    {
        if(m_ModCnt[idx] > 0)
        {
            --m_ModCnt[idx];
            m_Rcd.push_back(idx);
            if((int)m_Rcd.size() <= m_nMaxModNum && mass + m_ModMass[idx] < MAX_SHIFT_MASS)
			{
                _Combination(pos + 1, idx, mass + m_ModMass[idx]);
			}
            ++m_ModCnt[idx];
            m_Rcd.pop_back();
        }
    }
}


// 删除与固定修饰冲突的可变修饰类型
void CPrePTMForm::_CheckVarMod()
{
	for(size_t i = 0; i < m_vFixmodInfo.size(); ++i)
	{
		if(m_vFixmodInfo[i].cType == 'n')
		{
			string sites = m_vFixmodInfo[i].strSite;
			size_t j = 0;
			while(j < m_vVarmodInfo.size())
			{
				if(m_vVarmodInfo[j].cType == 'n')
				{
					size_t s = 0;
					while(s < m_vVarmodInfo[j].strSite.length())
					{
						if(sites.find(m_vVarmodInfo[j].strSite[s]) != string::npos)
							m_vVarmodInfo[j].strSite.erase(s);
						++s;
					}
					if(m_vVarmodInfo[j].strSite.length() == 0)
					{
						cout << "[Warning] "<< m_vVarmodInfo[j].strName.c_str() << " conflicts with " << m_vFixmodInfo[i].strName.c_str()<<endl;
						m_vVarmodInfo.erase(m_vVarmodInfo.begin()+j);
					}
					else ++j;
				} else ++j;
			}
		} else if(m_vFixmodInfo[i].cType == 'N') {
			size_t j = 0;
			while(j < m_vVarmodInfo.size())
			{
				if(m_vVarmodInfo[j].cType == 'N')
				{
					cout << "[Warning] "<< m_vVarmodInfo[j].strName.c_str() << " conflicts with " << m_vFixmodInfo[i].strName.c_str()<<endl;
					m_vVarmodInfo.erase(m_vVarmodInfo.begin() + j);
				} else ++j;
			}
		} else {
			size_t j = 0;
			while(j < m_vVarmodInfo.size())
			{
				if(m_vVarmodInfo[j].cType == 'C')
				{
					cout << "[Warning] "<< m_vVarmodInfo[j].strName.c_str() << " conflicts with " << m_vFixmodInfo[i].strName.c_str()<<endl;
					m_vVarmodInfo.erase(m_vVarmodInfo.begin() + j);
				} else ++j;
			}
		}  // end if modType
	}// end for fixmod
	m_ModNum = m_vVarmodInfo.size();
}

void CPrePTMForm::SetMaxModNum(const int cnt)
{
	m_nMaxModNum = cnt;
}

void CPrePTMForm::SetModInfo(vector<string> &fixmod, vector<string> &varmod)
{
	COptionTool config("modification.ini");
	unordered_set<string> setModNames;
	int modId = 0;
	int totalNum = config.GetInteger("", "@NUMBER_MODIFICATION", 0);
	//cout << totalNum << endl;
	char tmpStr[READLEN];

	for(modId = 1; modId <= totalNum; ++modId)
	{
		sprintf(tmpStr, "name%d", modId);
		string modName = config.GetString("", tmpStr, "");
		if(modName.size() == 0) continue;
		int idx = modName.length() - 1;
		while(idx > 0 && modName[idx] != ']')
		{
			--idx;
			modName.erase(modName.end()-1);
		}
		setModNames.insert(modName);
	}

	for(size_t i = 0; i < fixmod.size(); ++i)
	{
		if(setModNames.find(fixmod[i]) != setModNames.end())
		{
			string modInfo = config.GetString("", fixmod[i].c_str(), "");
			MODINI tmpMod;
			tmpMod.strName = fixmod[i];
			char site[30], type[10];
			sscanf(modInfo.c_str(), "%s %s %lf", site, type, &tmpMod.lfMass);
			tmpMod.strSite = site;
			if(strcmp(type, "NORMAL") == 0)
				tmpMod.cType = 'n';
			else if(strcmp(type, "PRO_N") == 0)
				tmpMod.cType = 'N';
			else if(strcmp(type, "PRO_C") == 0)
				tmpMod.cType = 'C';
			else {
				cout<<"[Error] Invalid modification type: " << type << endl;
				exit(-1);
			}
			//cout<<modName<<" "<<tmpMod.strSite<<" "<<tmpMod.cType<<" "<<tmpMod.lfMass<<endl;
			m_vFixmodInfo.push_back(tmpMod);
		} else {
			cout<<"[Warning] Can not find the modification " << fixmod[i].c_str() << ", please check the modification.ini!" << endl;
		}
	}

	
	for(size_t i = 0; i < varmod.size(); ++i)
	{
		if(setModNames.find(varmod[i]) != setModNames.end())
		{
			string modInfo = config.GetString("", varmod[i].c_str(), "");
			MODINI tmpMod;
			tmpMod.strName = varmod[i];
			char site[30], type[10];
			sscanf(modInfo.c_str(), "%s %s %lf", site, type, &tmpMod.lfMass);
			tmpMod.strSite = site;
			if(strcmp(type, "NORMAL") == 0)
				tmpMod.cType = 'n';
			else if(strcmp(type, "PRO_N") == 0)
				tmpMod.cType = 'N';
			else if(strcmp(type, "PRO_C") == 0)
				tmpMod.cType = 'C';
			else {
				cout<<"[Error] Invalid modification type: " << type << endl;
				exit(-1);
			}
			// TODO: get the nutral loss
			//cout<<modName<<" "<<tmpMod.strSite<<" "<<tmpMod.cType<<" "<<tmpMod.lfMass<<endl;
			m_vVarmodInfo.push_back(tmpMod);
		} else {
			cout<<"[Warning] Can not find the modification \"" << varmod[i].c_str() << "\", please check the modification.ini!" << endl;
		}
	}

	_CheckVarMod();
}

// Accoding to the file config.ini, initialize the member data of the class
void CPrePTMForm::InitGivenModInfo()
{
    // get the variable mod information
	m_nNCmodNum = 0;
    for(size_t i = 0; i < m_vVarmodInfo.size(); ++i)
    {
		int nMass = int(m_vVarmodInfo[i].lfMass + 0.5);
        m_ModMass.push_back(nMass);

		if(m_vVarmodInfo[i].cType == 'N')
        {
			++m_nNCmodNum;
            m_vNtermMod.push_back(i);
            m_ModCnt.push_back(1);
        } else if(m_vVarmodInfo[i].cType == 'C') {
			++m_nNCmodNum;
            m_vCtermMod.push_back(i);
            m_ModCnt.push_back(1);
        } else {
            m_ModCnt.push_back(m_nMaxModNum);
			for(int cIdx = 0; cIdx < (int)m_vVarmodInfo[i].strSite.length(); ++cIdx)
			{
				m_MapModAA2ID.insert(pair<char, int>(m_vVarmodInfo[i].strSite[cIdx], i));
			}
        }
    }
	for(int i=0; i<2*MAX_SHIFT_MASS; ++i){
		m_ModForms[i].clear();
	}
}

// Get all the combinational PTM forms through the given known PTMs
void CPrePTMForm::CalculateModForm()
{
	InitGivenModInfo();
    _Combination(0, 0, 0);
    //-----------------------------------------------
    //Display all the combinational forms and its mass
#ifdef _DEBUG_MOD
    for(int i = 0; i < MAX_SHIFT_MASS; ++i)
    {
        if(m_ModForms[i].size())
        {
            cout<<"[Debug] mass="<<i<<":"<<m_ModForms[i].size()<<endl;
            for(int j = 0; j < (int)m_ModForms[i].size(); ++j)
            {
                for(int t = 0; t < (int)m_ModForms[i][j].size(); ++t)
                    cout<<m_ModForms[i][j][t]<<" ";
                cout<<endl;
            }
        }
    }
#endif
}

int CPrePTMForm::GetModNum()
{
	return m_ModNum;
}

int CPrePTMForm::GetNCModNum()
{
	return m_nNCmodNum;
}

double CPrePTMForm::GetMassbyID(const int idx)
{
	if(idx >= m_ModNum)
	{
		cout << "[Error] Invalid mod ID!" << endl;
		return 0.0;
	} else return m_vVarmodInfo[idx].lfMass;
}

string CPrePTMForm::GetNamebyID(const int idx)
{
	if(idx >= m_ModNum)
	{
		cout << "[Error] Invalid mod ID!" << endl;
		return "*";
	} else return m_vVarmodInfo[idx].strName;
}

void CPrePTMForm::GetFixModInfo(vector<MODINI> &fixmodInfo)
{
	fixmodInfo = m_vFixmodInfo;
}

void CPrePTMForm::GetAllIDsbyAA(const char aa, vector<int> &vModIDs)
{
	vModIDs.clear();
	pair<MAP_IT, MAP_IT> mapRet = m_MapModAA2ID.equal_range(aa);
    for(MAP_IT it = mapRet.first; it != mapRet.second; ++it)
    {
		vModIDs.push_back(it->second);
    }
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
	return m_vVarmodInfo.at(idx).cType == 'N';
}

bool CPrePTMForm::isCtermMod(const int idx)
{
	return m_vVarmodInfo.at(idx).cType == 'C';
}
