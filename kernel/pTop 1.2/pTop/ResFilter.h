/*
 *  ResFilter.h
 *
 *  Created on: 2014-03-03
 *  Author: luolan
 */

#ifndef RESFILTER_H
#define RESFILTER_H

#include <iostream>
#include <algorithm>
#include <unordered_set>
#include "predefine.h"
#include "SearchEngine.h"
#include "CreateIndex.h"
#include "BasicTools.h"
using namespace std;

class CResFilter{
private:
	int m_nDBnum;
	int m_nRawNum;
	int m_nIdSpecNum;
	int m_nResFileIdx;
	double m_lfThreshold;
	vector<string> m_strQryRes;
    vector<string> m_strReport;
    vector<PRSM> m_vRes;

	Clog *m_clog;

    void _Filter();
    int _FindFirstOf(char *buf, char ch, int start, int len);

public:
	CResFilter(double lfFDR, const vector<string> &strRes, const vector<string> &strFilter, int dbNum);
	~CResFilter();
    void GetSearchRes();
	void split(char* str, const char* c, vector<string> &words);
    void Parse(char *buf);
    void GeneReport(vector<int> &vIdScan);
	void OutputLabelRes(int fileID, FILE *f, CPrePTMForm *cPTMForms, const vector<MODINI> &fixmodInfo);
    void Run(vector<FILE*> &fLabels, CPrePTMForm *cPTMForms, const vector<MODINI> &fixmodInfo, vector<int> &vIdScan);
	void FirstReport(int fileType, vector<string> &vIdSpec, vector<PROTEIN_STRUCT> &proteinDB);
	void FirstFilter(int fileType, int fidx, vector<string> &vIdSpec, vector<PROTEIN_STRUCT> &proteinDB);
};

#endif
