/*
 *  TagFlow.h
 *
 *  Created on: 2014-01-09
 *  Author: luolan
 */

#ifndef TAGFLOW_H
#define TAGFLOW_H

#include <iostream>
#include <cstring>
#include <cstring>
#include <string>

#include "sdk.h"
#include "index.h"
#include "MapAAMass.h"

using namespace std;
typedef int tag_val;
const int ZEROPADDING3 = 0x000003ff;
const int ZEROPADDING4 = 0x00007fff;

class CTagFlow{

private:
	bool m_bTagLenInc;   // whether create 5-tag index
	int m_nTotalProteinNum;
	int m_nMaxTagLen;
	double m_lfMaxTruncatedMass;
    vector<long int> m_vFrac;
    CMapAAMass *m_cMassMap;
	Clog *m_pTrace;

    long int _GetKey(const string &tag);
	void _ChangeItoL(string &tag);
	bool _CheckCandiPro(int massTol, const string &strProSQ, const TAGRES &tag, const int pos);
	void _GenerateTags(const string &s, const int pro_id, const int tagLen, unordered_set<tag_val> &tagKeys);
	void _insertTag(tag_val key, UINT_UINT &pro_pos);
	void _UpdateTagWeight();

public:
	char m_mapChar2Code[ALPHABET_SIZE];
	// map<long int, vector<UINT_UINT> >
	// unordered_map<tag_val, vector<UINT_UINT> > m_mapKey2Pos;
	unordered_map<tag_val, DB_TAG_INFO> m_mapKey2Pos;

	CTagFlow(CMapAAMass *cAAMass);
	CTagFlow(const CConfiguration *pParameter, CMapAAMass *cAAMass);
    ~CTagFlow();
	void Clear();
	void CreateTagIndex(const vector<PROTEIN_STRUCT> &proList);
    // void CreateTagIndex1(TAG_ITEM *vTagTbl, const vector<PROTEIN_STRUCT> &proList, long int nTagSize);
    void QueryTag(int massTol, TAGRES &tag, map<size_t, int> &mapID2Pos,
                  vector<PROID_NODE> &vCandiPro, const vector<PROTEIN_STRUCT> &proList);

	tag_val GetKey(const string &tag);
	bool isTagLenIncrease() const;
};

#endif
