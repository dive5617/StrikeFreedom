/*
 *  SearchEngine.h
 *
 *  Created on: 2013-10-28
 *  Author: luolan
 */

#ifndef SEARCHER_H
#define SEARCHER_H

#include <iostream>
#include <cstring>
#include <string.h>
#include <set>

#include "sdk.h"
#include "util.h"
#include "MapAAMass.h"
#include "index.h"
#include "PrePTMForm.h"
#include "TagSearcher.h"
#include "TagFlow.h"

using namespace std;

const int CHECK_TAG_NUM = 3;
const int TOTAL_MASS_SHIFT = 1000;

class CSearchEngine
{

private:
	
	int m_nPrecurID;
	int m_nPrecurTol;     // upper(m_lfPrecurTol)
	double m_Scoefficient[2];
	double m_lfPrecurMass;         // 理论母离子质量
	double totalIntensity;
	double baseline;

	vector<int> m_vMatchedPeak;
	vector<size_t> m_hashTbl;
	vector<vector<double> > m_theoNFragIon;
	vector<vector<double> > m_theoCFragIon;
	vector<int> m_theoN_H2OLossAA;  // N端包含失水氨基酸的数目
	vector<int> m_theoN_NH3LossAA;  // N端包含失氨氨基酸的数目
	vector<int> m_theoC_H2OLossAA;  // C端包含失水氨基酸的数目
	vector<int> m_theoC_NH3LossAA;  // C端包含失氨氨基酸的数目
    multimap<int, int> m_mapProLastModSite; //保存每种修饰在蛋白序列中可以发生的最后一个位置, pos->modID
	vector<int> m_vModID2Num;      //保存一种修饰组合中每种修饰的个数
	vector<int> m_vProSeqModNum;   //保存蛋白序列中每种修饰可以发生的位置数目

	CConfiguration *m_cParam;
	CProteinIndex *m_cProIndex;
	SPECTRUM *m_Spectra;
	CMapAAMass *m_mapAAMass;
    CPrePTMForm *m_cPTMIndex;
    CTagFlow *m_cTagFlow;
	CTagSearcher *m_cTag;
	//FragmentIndex *m_pFragIndex;
	Clog *m_clog;
	FILE *fp, *fp2;   // @ debug

protected:

    int _BinSearch(double mass);
	bool _CheckModForm(vector<int> &modForms, const string &proSQ, int startPos, int endPos);
	double _GetModMass(vector<int> &modForm);
	double _GetModSetMass(vector<int> &modSet);
	void _CreateHashTbl();

	void _CheckMatched(vector<double> &ionMass, double modMass, int &matchedIon);
	void _AddNeutralLossIons(int pos, bool isNterm, double mass, vector<double> &lossIons);
	int _AccurateMatch(double mass, vector<double>& smallWnd, double shiftPPM, double &peaktol);
	int _LookupHashTbl(int type, double shiftPPM, vector<double> &ionMass, vector<double> &matchedInts, vector<double> &massVol, double &sumMatchedIntens);
	int _LookupHashTbl(int type, double shiftPPM, int pos, bool isNterm, vector<double> &ionMass, vector<double> &matchedInts, vector<double> &massVol, double &sumMatchedIntens);
	int _LookupHashTbl(int type, double ftol, vector<FRAGMENT_ION> &peaks, vector<int> &vHashTable, vector<double> &ionMass, vector<double> &matchedInts, vector<double> &massVol, int FACTOR);
	
	void _CheckBestPath(double totalModMass, priority_queue<PATH_NODE> &qPaths);
	void _PushBackNode(vector<DP_VERTICE> &nextDist, DP_VERTICE &nextNode, int addWeight, UINT_UINT &addMod);
    void _GenerateIonsbyAAMassLst(vector<double> vProAAMass);
	void _GetTheoryFragIons(const string &proSeq, int condition, vector<UINT_UINT> &modSites);
	
    void _WriteToCandidate(const string &proSeq, const string &proAC, priority_queue<PATH_NODE> &qPaths, int proType);
    void _GetValidCandiPro(vector<int> &modForm, const string &proSeq, int start, int end, priority_queue<PATH_NODE> &bestPaths, double preModMass, double totalModMass);
    
	// added by wrm @2016.12.28
	void _ModificationLocation(vector<int> &modForm, const string &proSeq, int start, int end, priority_queue<PATH_NODE> &bestPaths, double totalModMass);
	void _NextLayer(const string &proSeq, unordered_set<int> &mods, unordered_map<int, int> &modCnt, unordered_map<int, int> &modLastSite, unordered_map<string, VERTICE> &preLayer, int idx, bool isNterm, double totalModMass);
	void _InsertNode(const VERTICE &preNodes, unordered_map<string, VERTICE> &nextLayer, string preKey, int addWeight, double newMass, UINT_UINT &newSite);
	
	string hashCode(vector<int> &modForm);
	string hashCode(unordered_map<int, int> &modCnt);
	// 
	void _CombModPaths(priority_queue<PATH_NODE> &bestPaths, priority_queue<PATH_NODE> &tmpPaths, int flag, PATH_NODE modOnTag);
	double _FilterCandiProtein(const string &proSeq, double precurMass);
	double _BM25Score(int type, double shiftPPM);
	double _BM25Score(int type, double shiftPPM, PROTEIN_SPECTRA_MATCH &prsm);
	double _BM25Score(const string &seq, const double lfMass, vector<FRAGMENT_ION> &peaks, vector<int> &vHashTable, const int FACTOR);

	void _Lsqt( vector<double> &x, vector<double> &y);
	void _FitHighScores(vector<double> &vScore);

	double _GetRandomProteoformIons(double mass);
	double _GetTotalIntensity(double PrecursorMass);
	double _GetBaselineByHistogram(double SNratio);
	double _GetBaselineByThreshold(double cutoff);
	void _GetTopPeak(vector<FRAGMENT_ION> &vtPeaks, const int topk);

	void _GetProteinsInWindow(double precurMass, double halfWindow, vector<DOUBLE_INT> &pro_delta);
	void _GetCandidatesInWindow(double precurMass, double halfWindow, unordered_map<int, unordered_map<string, double>> &candiPros);
	// ion index flow
	void _SearchTerms(FragmentIndex *pFragIndex, unordered_map<size_t, double> &mpProCnt, vector<FRAGMENT_ION> &vtPeaks);
	void _GetValidCandidates(double precurMass, unordered_map<size_t, double> &mpProCnt, unordered_map<int, unordered_map<string, double>> &candiPros);

	void _CheckTerminal(const bool isNterm, const string &seq, double proMass, double precurMass, unordered_map<string, double> &proSeqs);
	void _CheckNCterm(const PROTEIN_STRUCT& pro, PROID_NODE& candiPro, unordered_map<string,double> &vCandiProSeq);
	void _CheckCandiPros(vector<PROID_NODE> &candiProID, unordered_map<int, unordered_map<string, double>> &candiPros, vector<CANDI_PROTEIN> &validCandi);

	void _GetComplementaryIons(const bool isNterm, const vector<double> &vIonMasses, vector<double> &vComIons);
	void _GetComplementaryPeaks(vector<FRAGMENT_ION> &vtPeaks, vector<FRAGMENT_ION> &vtComPeaks);

	void _CoarseScore(vector<FRAGMENT_ION> &peaks, vector<int> &vHashTable, unordered_map<int, unordered_map<string, double>> &candiPros, vector<CANDI_PROTEIN> &filteredPros);
	
	void _RefinedScore();
	void _CalculateEvalue();
	void _MatchCandidateWithPTM(vector<CANDI_PROTEIN> &candiPros);

public:

	CSearchEngine(CConfiguration *cParam, CMapAAMass *cMapMass, CPrePTMForm *cPTMForms, CTagFlow *cTagIndex, SPECTRUM *spec);
	CSearchEngine(CConfiguration *cParam, CMapAAMass *cMapMass, CPrePTMForm *cPTMForms, CProteinIndex *cProIndex, CTagFlow *cTagIndex, SPECTRUM *spec);
    ~CSearchEngine();
	void SetProteinIndex(CProteinIndex* proIndex);
	void Init();
    void PreProcess();

	void MatchCandidate(const string &proSQ, const string &proAC, int isDecoy, int actStatus, int deltaMass);
	void GetMoreCandidate(int actStatus);
	void SketchScore(vector<CANDI_PROTEIN> &candiPros);
    
	void Search();  // tag flow
	void IonIndexSearch(FragmentIndex *pFragIndex);  // ion index flow

	void SecondSearch();		
};
#endif
