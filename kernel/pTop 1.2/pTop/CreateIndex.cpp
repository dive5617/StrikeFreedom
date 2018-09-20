/*
 *  CreateIndex.cpp
 *
 *  Created on: 2013-10-28
 *  Author: luolan
 */

#include <iostream>
#include <cstring>
#include <fstream>
#include <vector>
#include <cstring>
#include <string>
#include <algorithm>
#include <windows.h>
#include <ctime>

#include "BasicTools.h"
#include "CreateIndex.h"

//#define _DEBUG_PRO

using namespace std;

// Comparision between two proteins by mass
bool proteinCmp(const PROTEIN_STRUCT &pro1, const PROTEIN_STRUCT &pro2)
{
    return pro1.lfMass < pro2.lfMass;
}

CCreateIndex::CCreateIndex(const string &strDBFile, CMapAAMass *mapMass):
	m_ProteinNum(0), m_nProTotalLen(0), m_DBFile(strDBFile), mapProMass(mapMass), m_ProteinList(NULL)
{
	m_fpro.open(m_DBFile.c_str(), std::iostream::in);
	m_clog = Clog::getInstance();
}
CCreateIndex::~CCreateIndex()
{
	mapProMass = NULL;
	if(m_fpro.is_open()) m_fpro.close();
}

void CCreateIndex::_Trim(string &str)
{
	size_t idx = 0;
	for(size_t i = 0; i < str.length(); ++i)
	{
		if(str[i] >= 'A' && str[i] <= 'Z')
		{
			if(i != idx)
			{
				str[idx] = str[i];
			}
			++idx;
		}
	}
	if(idx < str.length())
	{
		str.erase(str.begin() + idx, str.end());
	}
}

//// Generate the decoy sequence
//void CCreateIndex::GetDecoyProtein(string &sequence)
//{
///*
////---------Shuffle---------
//   srand(time(NULL));
//    size_t len = sequence.length();
//    for(size_t i = 0; i < len; ++i)
//    {
//        int idx = rand() % len;
//        swap(sequence[i], sequence[idx]);
//    }
//*/
//	
////----------Reverse--------
// //   size_t len = sequence.length();
//	//for(size_t i = 0; i < len / 2; ++i)
// //   {
// //       swap(sequence[i], sequence[len - 1 - i]);
// //   }
////------------------------
//
//
//    //cout<<"target-"<<sequence<<endl;
//    if(sequence.length() <= 1)
//	{
//        return;
//	} else if(sequence.length() == 2) {
//        swap(sequence[0], sequence[1]);
//        return;
//    }
//    size_t len = sequence.length();
//    size_t idx = len / 2;
//    string subStr = sequence.substr(idx);
//    sequence.erase(sequence.begin()+idx, sequence.end());
//    for(size_t i = 0; i < idx/2; ++i)
//    {
//        swap(sequence[i], sequence[idx - 1 - i]);
//    }
//    for(size_t i = 0; i < subStr.length()/2; ++i)
//    {
//        swap(subStr[i], subStr[subStr.length() - 1 - i]);
//    }
//    sequence.append(subStr);
//    //cout<<"decoy-"<<sequence<<endl;
//	
////------------------------------------
///*	
//	size_t len = sequence.length();
//    char lastCh = sequence[len - 1];
//    sequence.erase(sequence.end() - 1, sequence.end());
//	sequence.insert(sequence.begin(), lastCh);
//*/
//}

// Create the index of the proteins
bool CCreateIndex::IndexofProtein()
{

	if(!m_fpro)
	{
		cout<<"[Error] Cannot open the database file: " << m_DBFile << endl;
		exit(0);
	}
	m_ProteinNum = 0;
	m_nProTotalLen = 0;
	//m_ProteinList.clear();
	vector<PROTEIN_STRUCT> ().swap(m_ProteinList);

	bool isFinished = true;
	string line;
	PROTEIN_STRUCT proteinInfo;
	

	while(getline(m_fpro, line) && (0 == line.size() || '>' != line[0])) ;
	while('>' == line[0])
    {
        ++m_ProteinNum;
		// Get the protein AC, ignore DE now
		size_t found = line.find_first_of(" ");
		proteinInfo.strProAC = line.substr(1, found - 1);
		if((int)proteinInfo.strProAC.length() > PROAC_LEN)
		{
			proteinInfo.strProAC.erase(proteinInfo.strProAC.begin() + PROAC_LEN, proteinInfo.strProAC.end());
		}
        
		found = proteinInfo.strProAC.find_first_of(',');
		while(found != string::npos)
		{
			proteinInfo.strProAC[found] = '_';
			found = proteinInfo.strProAC.find_first_of(',', found + 1);
		}
		
        // Get protein SQ
		proteinInfo.strProSQ = "";
        while(getline(m_fpro, line))
        {
			if(0 < line.size() && '>' == line[0])
			{
				break;
			}
			_Trim(line);
			proteinInfo.strProSQ.append(line);
        }
		//cout << proteinInfo.strProSQ << endl;

		m_nProTotalLen += ((long int)(proteinInfo.strProSQ.length()) << 1);
        // Calculate the mass of the protein
        proteinInfo.lfMass = mapProMass->CalculateMass(proteinInfo.strProSQ.c_str());
		//if(proteinInfo.strProSQ.substr(0, 10).compare("MSPKKAKKRA") == 0)
		//	cout << proteinInfo.lfMass << endl;

        try
        {
            proteinInfo.nIsDecoy = 0;
            //cout<<proteinInfo.strProAC<<endl;
			//cout<<proteinInfo.strProSQ.c_str()<<endl;
			string proSeq = proteinInfo.strProSQ;
            m_ProteinList.push_back(proteinInfo);

            CStringProcess::Reverse(proteinInfo.strProSQ);
            //cout<<"*Reverse_" << proteinInfo.strProSQ.c_str()<<endl;
            proteinInfo.nIsDecoy = 1;
            m_ProteinList.push_back(proteinInfo);
        }
        catch(bad_alloc &ba)
        {
            cout << "IndexofProtein() " << ba.what( ) << endl;
			
        }
		if(0 == line.size())
		{   // skip the blank line
			while(0 == line.size()) 
			{
				if(getline(m_fpro, line)) continue;
				else break;
			}
			if(0 == line.size()) break;
		}
		if(m_nProTotalLen >= MAX_PRO_BLOCK)
		{
			isFinished = false;
			break;
		}
    }

    sort(m_ProteinList.begin(), m_ProteinList.end(), proteinCmp);


    printf("[pTop] Number of proteins: %d\n", m_ProteinNum);
    
#ifdef _DEBUG_PRO
	// Display all the some info of proteins for check
    for(size_t i = 0; i < m_ProteinList.size(); ++i)
    {
		cout<<m_ProteinList[i].lfMass<<" "<<m_ProteinList[i].strProAC<<" "<<m_ProteinList[i].strProSQ<<endl;
    }
#endif
	return isFinished;
}


long int CCreateIndex::GetProTotalLen()
{
	return m_nProTotalLen;
}

int CCreateIndex::GetProteinNum()
{
	return m_ProteinNum;
}

