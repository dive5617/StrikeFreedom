/*
 *  SearchEngine.h
 *
 *  Created on: 2013-10-28
 *  Author: luolan
 */

#ifndef SPECTRA_H
#define SPECTRA_H

#include <iostream>
#include <cstring>
#include <string.h>
#include <set>

#include "predefine.h"
#include "MapAAMass.h"
#include "CreateIndex.h"
#include "PrePTMForm.h"
#include "TagSearcher.h"
#include "TagFlow.h"

using namespace std;

class CSearchEngine
{

private:
	bool calAX;
	bool calBY;
	bool calCZ;
	int m_nPrecurID;
	int m_nPrecurTol;
	double m_Scoefficient[2];
	double m_lfFragTol;
	double m_lfPrecurMass;         // 理论母离子质量
	double totalIntensity;
	double baseline;

	vector<int> m_vMatchedPeak;
	vector<int> m_vModID2Num;      //保存一种修饰组合中每种修饰的个数
    vector<int> m_vProSeqModNum;   //保存蛋白序列中每种修饰可以发生的位置数目
	vector<size_t> m_hashTbl;
	vector<double> m_vAAMass;
	vector<vector<double> > m_theoNFragIon;
	vector<vector<double> > m_theoCFragIon;
    multimap<int, int> m_mapProLastModSite; //保存每种修饰在蛋白序列中可以发生的最后一个位置, pos->modID

	SPECTRUM *m_Spectra;
	CMapAAMass *m_mapAAMass;
    CPrePTMForm *m_getPTMForm;
    CTagFlow *m_cTagFlow;
	CTagSearcher *m_cTag;

protected:

    int _BinSearch(const vector<PROTEIN_STRUCT> &proList, double mass);
    bool _CheckModForm(int massIdx, int ptmIdx, const string &proSQ, int startPos, int endPos);
	double _GetModMass(vector<int> &modForm);
	double _GetModSetMass(vector<int> &modSet);
	void _CreateHashTbl();

	void _CheckMatched(vector<double> &ionMass, double modMass, int &matchedIon);
	int _LookupHashTbl(int type, double shiftPPM, vector<double> &ionMass, vector<double> &matchedInts, vector<double> &massVol, double &sumMatchedIntens);
	void _CheckBestPath(double totalModMass, priority_queue<PATH_NODE> &qPaths);
	void _PushBackNode(vector<DP_VERTICE> &nextDist, DP_VERTICE &nextNode, int addWeight, UINT_UINT &addMod);
    void _GenerateIonsbyAAMassLst(vector<double> vProAAMass);
	void _GetTheoryFragIons(const string &proSeq, int condition, vector<UINT_UINT> &modSites);
	
    void _WriteToCandidate(const string &proSeq, const string &proAC, priority_queue<PATH_NODE> &qPaths, int proType);
    void _GetValidCandiPro(vector<int> &modForm, const string &proSeq, int start, int end, priority_queue<PATH_NODE> &bestPaths, double preModMass, double totalModMass);
    void _CombModPaths(priority_queue<PATH_NODE> &bestPaths, priority_queue<PATH_NODE> &tmpPaths, int flag, PATH_NODE modOnTag);
	double _FilterCandiProtein(const string &proSeq, double precurMass);
	double _BM25Score(int type, double shiftPPM);
	double _BM25Score(int type, double shiftPPM, PROTEIN_SPECTRA_MATCH &prsm);

	void _Lsqt( vector<double> &x, vector<double> &y);
	void _FitHighScores(vector<double> &vScore);
	void _GetAllAAMasses();
	double _GetRandomProteoformIons(double mass);
	double _GetTotalIntensity(double PrecursorMass);
	double _GetBaselineByHistogram(double SNratio);
	double _GetBaselineByThreshold(double cutoff);

	void getCandidatesInWindow(double precurMass, const vector<PROTEIN_STRUCT> &proList, vector<PROID_NODE> &candiProID);

public:

    CSearchEngine(CMapAAMass *cMapMass, CPrePTMForm *cPTMForms, CTagFlow *cTagIndex, CTagSearcher *cTagSearcher, double fragTol, int precurTol);
    ~CSearchEngine();
    void Init(string &actType);
	void Reset(SPECTRUM &spec);
    void PreProcess();
	//void SearchTAGCandidate(const vector<PROTEIN_STRUCT> &proList, int proID, int actStatus, bool hasM);
	void MatchCandidate(const string &proSQ, const string &proAC, int isDecoy, int actStatus, int deltaMass);
	void GetMoreCandidate(const vector<PROTEIN_STRUCT> &proList, int actStatus);
	void SketchScore(const vector<PROTEIN_STRUCT> &proList, vector<PROID_NODE> &caidiProID);
    void RefinedScore();
    void Search(const vector<PROTEIN_STRUCT> &proList);
	void SecondSearch(const vector<PROTEIN_STRUCT> &proList);
};
#endif
