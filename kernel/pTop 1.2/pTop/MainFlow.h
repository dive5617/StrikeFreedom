#ifndef MAINFLOW_H
#define MAINFLOW_H

#include <iostream>
#include <cstdio>
#include <dirent.h>

#include "Configuration.h"
#include "MapAAMass.h"
#include "PrePTMForm.h"
#include "CreateIndex.h"
#include "SearchEngine.h"
#include "ResFilter.h"

using namespace std;

class CMainFlow{
private:
	int m_nTotalProNum;	// for the report
	int m_nIdSpecNum;
	int m_nIdScan;
	int m_nIdProNum;
	vector<int> m_vSpecNum;
	vector<int> m_vScanNum; // for calculating the id rate
	vector<int> m_vIdScan;

	int m_nSpecNum;
	bool m_bFirstReadPF;
	string m_strTitle;
	string m_strConfigFile;
	CConfiguration *m_cPara;
	CMapAAMass *m_cMapMass;
	CPrePTMForm *m_cPTMForms;
	Clog *m_clog;

public:
	CMainFlow(const string &strFile);
	~CMainFlow();

	void Welcome();
	void Usage();

	int ParseMGF(ifstream &finMGF, vector<SPECTRUM> &spectra, int &preScan, int &scanCnt);
	int ParsePF(FILE *finPF, vector<SPECTRUM> &spectra);
	//int ParseCombMGF(ifstream &finMGF, vector<SPECTRUM> &spectra);

	void OutputSummary();
	void OutputPrSMRes(FILE *f, vector<SPECTRUM> &spectra, int cnt, int fileID);
	void OutputpFindRes(FILE *f, vector<SPECTRUM> &spectra, int cnt, int specNum);
	void OutputLabelHead(FILE *f, const string &mgfFile, const vector<MODINI> &fixmodInfo);

	void Searcher(vector<PROTEIN_STRUCT> &proteinList, CTagFlow *cTagIndex, CTagSearcher *cTagSearcher);
	void SecondSearcher(vector<PROTEIN_STRUCT> &proteinList, vector<string> &vIdSpec);
	void RunMainFlow();
};

#endif