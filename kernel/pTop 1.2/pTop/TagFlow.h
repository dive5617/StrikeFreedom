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
#include <map>
#include <unordered_map>
#include <cstring>
#include <string>

#include "predefine.h"
#include "CreateIndex.h"
#include "MapAAMass.h"

using namespace std;
typedef int tag_val;
const int ZEROPADDING3 = 0x000003ff;
const int ZEROPADDING4 = 0x00007fff;

class CTagFlow{

private:
	
    
    vector<long int> m_vFrac;
    CMapAAMass *m_cMassMap;

    long int _GetKey(const string &tag);
	void _ChangeItoL(string &tag);
    bool _CheckCandiPro(int massTol, const string &strProSQ, const double mass[], size_t pos, double &modMass);
	void _GenerateTags(const string &s, int pro_id);
	tag_val _getKey(const string &tag);
	void _insertTag(tag_val key, UINT_UINT &pro_pos);
	void _createMassHash(const vector<PROTEIN_STRUCT> &proList);

public:
	// map<long int, vector<UINT_UINT> >
	unordered_map<tag_val, vector<UINT_UINT> > m_mapKey2Pos;
	vector<vector<int> > v_mass2pro;

    CTagFlow(CMapAAMass *cAAMass);
    ~CTagFlow();
	void Clear();
	void CreateTagIndex(const vector<PROTEIN_STRUCT> &proList);
    void CreateTagIndex1(TAG_ITEM *vTagTbl, const vector<PROTEIN_STRUCT> &proList, long int nTagSize);
    void QueryTag(int massTol, const TAGRES &tag, map<size_t, int> &mapID2Pos,
                  vector<PROID_NODE> &vCandiPro, const vector<PROTEIN_STRUCT> &proList);
};

#endif
