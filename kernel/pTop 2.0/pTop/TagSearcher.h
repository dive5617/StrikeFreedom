/*
 *  TagSearcher.h
 *
 *  Created on: 2014-01-03
 *  Author: luolan
 */

#ifndef TAGSEARCHER_H
#define TAGSEARCHER_H

#include <iostream>
#include <cstdio>
#include <cstring>
#include <queue>

#include "sdk.h"
#include "MapAAMass.h"
#include "TagFlow.h"
#include "TAGPriorityQueue.h"

using namespace std;

class CTagSearcher
{
private:

	int m_nPeaksNum;
    double m_lfMASS_H2O;
    double m_lfMaxAAMass;
    double m_lfMinAAMass;
    double m_lfFlankingMass[2];
    double m_lfTmpScore;
	double m_lfFragTol;
    string m_strTmpTag;
    vector<int> m_vTagModIDs;
	vector<MODAA> modAATbl;
	
	vector<FRAGMENT_ION> m_peaks;

    CPriorityQueue *m_cTags;
	Clog *m_pTrace;

	void _ReverseTag(TAGRES &tag);
    void _FindTag(const vector< vector<NODE> > & vEdges, size_t tCurrLen, size_t tCurrPos, const int tagLen);
	void _FindTag(const vector< vector<NODE> > & vEdges, size_t tCurrLen, size_t tCurrPos, const int tagLen, bool noPostfix);
	void _ExtractTags(const vector< vector<NODE> > & vEdges, const int tagLen);

public:

    CTagSearcher();
    ~CTagSearcher();
    void Init(CMapAAMass *mapAAMass, CPrePTMForm *cPTMForms, double fragTol);
    void GeneTag(const SPECTRUM *spectra, const bool bTagLenInc=false);
    void Run(int massTol, double precurMass, CTagFlow *tagFlow, const vector<PROTEIN_STRUCT> &proList, vector<PROID_NODE> &vCandiPro);
	double GetMinAAMass() const;
	double GetMaxAAMass() const;
	void PreProcess(const SPECTRUM *spectra);
	void MergeClosePeak(vector<FRAGMENT_ION> &vPeaks, int &nPeaksNum);
	void GetTopPeaks(vector<FRAGMENT_ION> &m_peaks) const;
	void GetSpectraMassHash(vector<int> &vHashTable, const int factor);
	static void GeneratePeakHashTable(vector<FRAGMENT_ION> &vPeaks, vector<int> &vHashTable, const int factor);
};
#endif
