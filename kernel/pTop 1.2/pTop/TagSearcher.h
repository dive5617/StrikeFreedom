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

#include "predefine.h"
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
	
	MZ_SCOPE_ION *m_peaks;
    CPriorityQueue *m_cTags;

	void _PreProcess(const SPECTRUM *spectra);
	void _ReverseTag(TAGRES &tag);
    void _FindTag(const SPECTRUM *spectra, const vector< vector<NODE> > & vEdges, size_t tCurrLen, size_t tCurrPos);
	void _FindTag(const SPECTRUM *spectra, const vector< vector<NODE> > & vEdges, size_t tCurrLen, size_t tCurrPos, const int tagLen, bool noPostfix);

public:

    CTagSearcher();
    ~CTagSearcher();
    void Init(CMapAAMass *mapAAMass, CPrePTMForm *cPTMForms, double fragTol);
    void GeneTag(const SPECTRUM *spectra);
    void Run(int massTol, double precurMass, CTagFlow *tagFlow, const vector<PROTEIN_STRUCT> &proList, vector<PROID_NODE> &vCandiPro);
};
#endif
