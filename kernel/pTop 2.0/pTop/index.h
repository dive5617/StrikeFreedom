/*
 *  CreateIndex.h
 *
 *  Created on: 2013-10-28
 *  Author: luolan
 */

#ifndef CREATEINDEX_H
#define CREATEINDEX_H

#include <iostream>
#include <cstring>
#include <fstream>
#include <vector>

#include "sdk.h"
#include "util.h"
#include "MapAAMass.h"
#include "PrePTMForm.h"

using namespace std;

class CProteinIndex
{
private:
	int m_ProteinNum;        // Total number of target proteins
	long int m_nProTotalLen; // Total length of all proteins, to determine the size of TAG table
    string m_DBFile;         // Path and name of the database file
	ifstream m_fpro;
	CMapAAMass *mapProMass;
	Clog *m_clog;
	
	void _Trim(string &str);
	void _CreateMassHash();

public:
    vector<PROTEIN_STRUCT> m_ProteinList; // All peotein should sort by mass
	vector<int> m_vMassCnt;   // start pos of proteins with mass i, supporting mass hash table(m_ProteinList)

    CProteinIndex(const string &strDBFile, CMapAAMass *mapMass);
	~CProteinIndex();
    bool IndexofProtein();                // Get all protein info and sort by mass
	int GetProteinNum();
	long int GetProTotalLen();
	bool OpenFastaFile();
	void CloseFile();

};



class FragmentIndex{
private:
	const CConfiguration *m_cPara;
	CMapAAMass *m_cMassMap;
	CPrePTMForm *m_cPTMForms;
	Clog *m_pTrace;
	size_t m_tMassRange;
	unsigned long long m_ullFragCnt;  // number of fragment ions
	std::vector<size_t> m_vCnt;
	std::vector<size_t> m_vFragTable;
	vector<bool> m_vBFragmentSite;
	vector<bool> m_vYFragmentSite;

public:
	FragmentIndex(const CConfiguration *pParameter, CMapAAMass *pMassMap, CPrePTMForm *pPTMForms);
	~FragmentIndex();

	void CreateFragmentIndex(const vector<PROTEIN_STRUCT> &proList);
	void Clear();
	bool SearchNTerms(std::unordered_map<size_t, double> &mpTerms, const FRAGMENT_ION &stPeak);
	bool SearchCTerms(std::unordered_map<size_t, double> &mpTerms, const FRAGMENT_ION &stPeak);
	bool SearchTerms(std::unordered_map<size_t, double> &mpTerms, const FRAGMENT_ION &stPeak);
	void ShowFragmentTable(const vector<PROTEIN_STRUCT> &proList);

protected:
	void getFragmentIons(const PROTEIN_STRUCT &pro, int pEntry);
	void addToFragTable(const double lfMass, const int pEntry);
	size_t findMaxIndex(const std::string &strPattern, char cSite);

};

#endif
