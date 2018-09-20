#ifndef MAINFLOW_H
#define MAINFLOW_H

#include <iostream>
#include <cstdio>
#include <dirent.h>
#include "util.h"
#include "config.h"
#include "MapAAMass.h"
#include "PrePTMForm.h"
#include "index.h"
#include "searcher.h"
#include "file_utils.h"
#include "rerank\rerank.h"
#include "report\filter.h"
#include "quantify\quantify.h"
//#ifdef _WIN64 // 64-bit Windows Compiler
#pragma comment(lib, "pthreadVC2.lib")//不可缺少,否则编译报错  
//#else
//#endif
using namespace std;

class CMainFlow{
private:
	int m_nTotalProNum;	// for the report
	int m_nIdSpecNum;
	int m_nIdScan;
	int m_nIdProNum;
	int m_nBatchNum;   // load spectra by batch
	int m_nLabelIdx;   // search each label one pass

	vector<int> m_vIdScan;
	vector<SPECTRUM> m_vSpectra;  //
	unordered_set<int> m_IdSpecSet;

	string m_strTitle;
	string m_strConfigFile;

	CConfiguration *m_cPara;
	shared_ptr<Summary> m_pSum;

	CMapAAMass *m_cMapMass;
	CPrePTMForm *m_cPTMForms;
	CProteinIndex *m_cProIndex;
	CTagFlow *m_cTagIndex;
	//CSearchEngine *m_cSearcher;
	FragmentIndex *m_pFragIndex;

	MS2Input *m_pInput;
	Clog *m_pTrace;
	SearchState *m_pState;
	ThreadMonitor *m_pMonitor;
public:
	CMainFlow(const string &strFile);
	~CMainFlow();

	void OutputSummary();

	void RunMainFlow();

	void SearchFlow();  // first separate spectra, then separate database
	void SecondSearcher(const int fileID, const string &strQryRstFile);
	void IonIndexFlow();
	
	void Initialize();

	static void * _TagSearch(void *pArgs);
	static void * _IonSearch(void *pArgs);
	void Searcher(bool isIonIndex);

	void SpectraSegmentation(string specPath);  // separate spectra
	void DatabaseSegmentation(const char *strOutPath, const int fileID); // separate database
	void DBSegmentationFragFlow(const char *strOutPath, const int fileID);

	void Filter();
private:
	void init();

	void mergeSubRes(vector<string> &vSubFiles, string &resFile);
	void OutputTempPrSMRes(const char *strOutPath, const int fileID);
	string getProPTM(const string &seq, vector<UINT_UINT> &vModSites);
	
};

#endif