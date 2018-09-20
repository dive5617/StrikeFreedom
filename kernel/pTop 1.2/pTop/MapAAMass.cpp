/*
 *  MapAAMass.cpp
 *
 *  Created on: 2013-6
 *  Author: luolan
 */

#include <iostream>
#include <cstring>
#include <map>
#include <windows.h>

#include "MapAAMass.h"
#include "Configuration.h"

using namespace std;

CMapAAMass::CMapAAMass()
{
	m_lfNterm = 0.0;		
	m_lfCterm = 0.0;	
	m_strNtermSites = "";
	m_strCtermSites = "";
	m_vAAMass.clear();
}

CMapAAMass::~CMapAAMass()
{

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

void CMapAAMass::LoadElement(map<string, vector<FRAGMENT_ION> > &mapElem)
{
	COptionTool config("element.ini");
	int num = config.GetInteger("", "@NUMBER_ELEMENT", 0);
	
	if(0 == num)
	{
		cout << "[Error] There is no element, please check element.ini!" << endl;
		exit(0);
	}
	char eName[20];
    for(int i = 0; i < num; ++i)
    {
		sprintf(eName, "E%d", i+1);
		string temp = config.GetString("", eName, ""); 
		string delim = "|";
		vector<string> parts;
		Split(temp, delim, parts); // H|1.0078246,2.0141021,|0.99985,0.00015,|
		if(parts.size() < 3)
		{
			cout << "[Warning] Invalid element: " << temp << endl;
			continue;
		}
		string strName = parts[0];
		string masses = parts[1];
		string intens = parts[2];
		//cout << strName << " " << masses << " " << intens << endl;
		vector<FRAGMENT_ION> vCompod;
		delim = ",";

		parts.clear();
		Split(masses, delim, parts); // 1.0078246,2.0141021,
		for(size_t j = 0; j < parts.size(); ++j)
		{
			//cout << parts[j] << " ";
			if(parts[j].size() <= 0)
			{
				continue;
			}
			double mass = atof(parts[j].c_str());
			FRAGMENT_ION oneElem(mass, 0.0);
			vCompod.push_back(oneElem);
		}
		//cout << endl;

		parts.clear();
		Split(intens, delim, parts); // 0.99985,0.00015,
		for(size_t j = 0; j < parts.size(); ++j)
		{
			//cout << parts[j] << " ";
			if(parts[j].size() <= 0)
			{
				continue;
			}
			if(j >= vCompod.size())
			{
				cout << "[Warning] Invalid element: " << temp << endl;
				continue;
			}
			double inten = atof(parts[j].c_str());
			vCompod[j].lfIntens = inten;
		}
		//cout << endl;
		mapElem.insert(pair<string, vector<FRAGMENT_ION> >(strName, vCompod));
	}
}

// 初始化，读取aa.ini文件，将26个氨基酸及其对应的质量存储到m_vAAMass中
void CMapAAMass::Init()
{
	map<string, vector<FRAGMENT_ION> > mapElem;
	map<string, vector<FRAGMENT_ION> >::iterator it;
	LoadElement(mapElem);

	COptionTool config("aa.ini");
	int num = config.GetInteger("", "@NUMBER_RESIDUE", 0);
	if(0 == num)
	{
		cout << "[Error] There is no residue, please check aa.ini!" << endl;
		exit(0);
	}

	m_vAAMass.assign(26, 0);
	char name[READLEN]; //, fullName[READLEN];
	for(int i = 0; i < num; ++i)
    {
		sprintf(name, "R%d", i+1);
		string comp = config.GetString("", name, ""); 
		string delim = "|";
		vector<string> parts;
		Split(comp, delim, parts); // A|C(3)H(5)N(1)O(1)S(0)|
		if(parts.size() < 2)
		{
			cout << "[Warning] Invalid residue: " << comp << endl;
			continue;
		}
		//cout << parts[0] << " " << parts[1] << endl;
		char aa = parts[0][0];
		if(aa < 'A' || aa > 'Z')
		{
			cout << "[Warning] Invalid residue: " << comp << endl;
			continue;
		}
		double lfMass = 0.0;
		string strComp = parts[1];
		parts.clear();
		delim = ")";
		Split(strComp, delim, parts); // C(3)H(5)N(1)O(1)S(0)
		for(size_t j = 0; j < parts.size(); ++j)
		{
			if(parts[j].size() <= 0)
			{
				continue;
			}
			vector<string> one;
			delim = "(";
			Split(parts[j], delim, one);
			if(one.size() != 2)
			{
				continue;
			}
			
			it = mapElem.find(one[0]);
			if(it != mapElem.end())
			{
				vector<FRAGMENT_ION> isotope = it->second;
				double monoMass = 0.0;
				double maxIntsns = 0.0;
				for(size_t k = 0; k < isotope.size(); ++k)
				{
					if(isotope[k].lfIntens > maxIntsns)
					{
						maxIntsns = isotope[k].lfIntens;
						monoMass = isotope[k].lfMz;
					}
				}
				lfMass += monoMass * atoi(one[1].c_str());
			}
		}
		m_vAAMass[(int)(aa - 'A')] = lfMass;
		//cout << aa << " " << lfMass << endl;
	}

	for(it = mapElem.begin(); it != mapElem.end(); ++it)
	{
		vector<FRAGMENT_ION> ().swap(it->second);
	}
	map<string, vector<FRAGMENT_ION> > ().swap(mapElem);

	m_lfNterm = 0.0;
	m_lfCterm = 0.0;
	m_strNtermSites = "";
	m_strCtermSites = "";
}

// 计算输入氨基酸字符串的质量，+水+质子
double CMapAAMass::CalculateMass(const char sq[])
{
	if(0 == m_vAAMass.size())
	{
		cout << "[Error] Please init the object before calling this function!" << endl;
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
        total_mass += m_vAAMass[idx];
    }

    total_mass += 2*IonMass_Mono_H + IonMass_Mono_O;
	if(m_lfCterm > 0.0 && m_strCtermSites.find_first_of(sq[len-1]) != string::npos)
		total_mass += m_lfCterm;

    return total_mass;
}

// 计算输入氨基酸字符串的残基质量和
double CMapAAMass::CalculateNeutralMass(const string &sq)
{
	if(0 == m_vAAMass.size())
	{
		cout << "[Error] Please init the object before calling this function!" << endl;
		exit(-1);
	}
    int i, len = sq.length();
    double total_mass = 0;
    map<char, double>::iterator iter;

    for(i = 0; i < len; ++i)
    {
		int idx = sq[i] - 'A';
		if(idx > 26)
		{
			cout << "[Warning] Invalid amino acid: " << sq[i] << endl;
		}
        total_mass += m_vAAMass[idx];
    }
    return total_mass;
}


double CMapAAMass::GetAAMass(const char ch)
{
	return m_vAAMass[ch - 'A'];
}

void CMapAAMass::AddAAMassbyMod(const char ch, double modMass)
{
	m_vAAMass[ch - 'A'] += modMass;
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

double CMapAAMass::GetNtermMass(const char ch)
{
	double mass = 0;
	if(m_lfNterm > 0.0 && m_strNtermSites.find_first_of(ch) != string::npos)
		mass += m_lfNterm;
	return mass;
}

double CMapAAMass::GetCtermMass(const char ch)
{
	double mass = 0;
	if(m_lfCterm > 0.0 && m_strCtermSites.find_first_of(ch) != string::npos)
		mass += m_lfCterm;
	return mass;
}