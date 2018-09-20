/*
 *  ResFilter.h
 *
 *  Created on: 2014-03-03
 *  Author: luolan
 */

#ifndef FILTER_H
#define FILTER_H

#include <iostream>
#include <algorithm>
#include <unordered_set>
#include "../sdk.h"
#include "../util.h"
#include "../searcher.h"
#include "../index.h"
#include "../rerank/rerank.h"
using namespace std;


class CResFilter{
private:
	int m_nRawNum;
	int m_nResFileIdx;
	double m_lfThreshold;

	vector<unordered_map<int, byte>> m_vIdScans;  // scans of each raw

	shared_ptr<Summary> m_pSummary;

	CConfiguration *m_cPara;
	Clog *m_clog;

	void GetSearchRes(const string &ResPath, vector<PRSM> &m_vRes);
	void GetSearchRes(const string &ResPath, unordered_map<string, PRSM> &q_Res);
	void GetSearchRes(const string &ResPath, unordered_map<string, priority_queue<PRSM, vector<PRSM>, greater<PRSM> >> &q_Res);
	void Parse(char *buf, vector<PRSM> &m_vRes);
	void Parse(char *buf, unordered_map<string, PRSM> &q_Res);  // 合并多种标记下的query结果时要用到
	void Parse(char *buf, unordered_map<string, priority_queue<PRSM, vector<PRSM>, greater<PRSM> >> &q_Res);
	bool parseResLine(const char *line, const PRSM_Column_Index &cols, PRSM &prsm);

	void computeQvalues(vector<PRSM> &m_vRes);
    int _FindFirstOf(char *buf, char ch, int start);

	void MergeResultsofDiffLabels(vector<string> &vFilePath);
	void deleteTmpFiles(vector<string> &v_filePath);
	void OutputLabelRes(const string &plabelFile, vector<PRSM> &m_vRes);
public:
	CResFilter(CConfiguration *m_cParam);
	CResFilter(CConfiguration *m_cParam, shared_ptr<Summary> _pSum);
	~CResFilter();
	
	void Run();
	void reportFilteredSpectra(const string &resPath, vector<PRSM> &m_vRes);
	void WriteSpectraResult(const string &specFile, vector<PRSM> &m_vRes, bool filtered);
	void reportSummary();
    
	void SeparateFiltering(vector<string> &v_filePath);
	void MergeFiltering(vector<string> &v_filePath);

	void FirstFilter(const string strQryRst, double lfThreshold, unordered_set<int> &vIdSpec);
	void FirstReport(int fileType, vector<string> &vIdSpec, vector<PROTEIN_STRUCT> &proteinDB);

	void MergeQryResult(vector<string> &vFilePath, const string &strQryRstFile);
};

#endif
