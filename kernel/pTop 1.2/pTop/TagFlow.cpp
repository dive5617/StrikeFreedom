/*
 *  TagFlow.cpp
 *
 *  Created on: 2014-01-09
 *  Author: luolan
 */

#include <iostream>
#include <cstdlib>
#include <cstring>
#include <set>
#include <windows.h>
#include <algorithm>
#include<process.h>

#include "TagFlow.h"
#include "MapAAMass.h"
#include "PrePTMForm.h"

using namespace std;

bool _TagValueCmp(const TAG_ITEM &t1, const TAG_ITEM &t2)
{
    return t1.nKey < t2.nKey;
}

CTagFlow::CTagFlow(CMapAAMass *cAAMass) : m_cMassMap(cAAMass)
{
    m_mapKey2Pos.clear();
	v_mass2pro.resize(MAX_HASH_SIZE+1);
    long int frac = 1;
    for(int i = 0; i < TAG_LEN+1; ++i)
    {
        m_vFrac.push_back(frac);
        frac *= 26;
    }
}

void CTagFlow::Clear()
{
	unordered_map<int, vector<UINT_UINT> >::iterator it;
	for(it = m_mapKey2Pos.begin(); it != m_mapKey2Pos.end(); ++it)
	{
		vector<UINT_UINT> ().swap(it->second);
		it->second.clear();
	}
	unordered_map<int, vector<UINT_UINT> > ().swap(m_mapKey2Pos);
	m_mapKey2Pos.clear();
	for (int i = 0; i < v_mass2pro.size(); ++i){
		v_mass2pro[i].clear();
	}
}

CTagFlow::~CTagFlow()
{
	
}

long int CTagFlow::_GetKey(const string &tag)
{
    long int key = 0;
    size_t len = tag.length();
    if((int)len > TAG_LEN+1)
    {
        cout<<"[Error] Invalid tag: "<<tag.c_str()<<endl;
        return -1;
    }
    for(size_t i = 0; i < len; ++i)
    {
        key += (long int)(tag[i] - 'A') * m_vFrac[i];
    }
    return key;
}
// 将所有的I换为L，两者指代同一种氨基酸
void CTagFlow::_ChangeItoL(string &tag)
{
	for(int i = 0; i < (int)tag.length(); ++i)
	{
		if(tag[i] == 'I') tag[i] = 'L';
	}
}

bool CTagFlow::_CheckCandiPro(int massTol, const string &strProSQ, const double mass[], size_t pos, double &modMass)
{
    string leftSQ = strProSQ.substr(0, pos);
    double leftMass = m_cMassMap->CalculateNeutralMass(leftSQ);
    double deltaMass =  mass[0] - leftMass;
    modMass = deltaMass;
    //cout<<leftSQ.c_str()<<" "<<leftMass<<endl;
	//cout<<leftSQ.substr(0, 10).c_str()<<" "<<leftMass<<" "<<deltaMass<<" ";
    // 这里不考虑质量为负数的修饰，当考虑时可以在最初添加一个较大的数值将所有的负质量修饰转化为正
    if(deltaMass < -massTol - 500 || deltaMass > massTol + 500) // check2014.11.20
        return false;
    string rightSQ = strProSQ.substr(pos + TAG_LEN);
    double rightMass = m_cMassMap->CalculateNeutralMass(rightSQ);
    deltaMass = mass[1] - rightMass;
    //cout<<rightSQ.c_str()<<" "<<rightMass<<endl;
	//cout<<deltaMass<<endl;
    if(deltaMass < -massTol - 500 || deltaMass > massTol + 500) // check2014.11.20
        return false;
    else return true;
}

tag_val CTagFlow::_getKey(const string &tag)
{
	int len = tag.length();
    if((int)len > TAG_LEN+1)
    {
        cout<<"[Error] Invalid tag: "<< tag <<endl;
        return -1;
    }
	tag_val key = 0;
    for(int i = 0; i < len; ++i)
    {
        key = (key << 5)|(tag[i] - 'A');
    }
    return key;
}
	
void CTagFlow::_insertTag(tag_val key, UINT_UINT &pro_pos)
{
	unordered_map<tag_val,vector<UINT_UINT> >::iterator it;
	it = m_mapKey2Pos.find(key);
	if(it == m_mapKey2Pos.end()) // 倒排表中不存在该标签
    {
        vector<UINT_UINT> newVct;
        newVct.push_back(pro_pos);
		m_mapKey2Pos[key] = newVct;
    } else {
        (it->second).push_back(pro_pos);
    }
}

// give protein sequence, generate the 3-tag, 4-tag, 5-tag
// binary coding method
void CTagFlow::_GenerateTags(const string &s, int pro_id)
{
	string ss = s;
    _ChangeItoL(ss); // 将序列中的I换成L
    int sIdx = TAG_LEN, endPos = ss.size() - 1, pos = 0;
    string tag = ss.substr(0,TAG_LEN);
	tag_val key1 =  _getKey(tag);  // 获取编码值
	UINT_UINT tmpPro;
	tmpPro.nFirst = pro_id;         // 蛋白ID
	tmpPro.nSecond = pos;  // 在蛋白序列中的起始位置
	_insertTag(((key1 << 3)|TAG_LEN), tmpPro);

	//tag_val key2 = (key1 << 5)|(ss[TAG_LEN] - 'A');
	//_insertTag(((key2 << 3)|(TAG_LEN+1)),tmpPro);
    for(sIdx = TAG_LEN; sIdx < endPos; ++sIdx)
    {
		key1 = ((key1&ZEROPADDING4) << 5) | (ss[sIdx] - 'A');
		tmpPro.nSecond = ++pos;
		_insertTag(((key1 << 3)|TAG_LEN),tmpPro);
        //
  //      key2 = (key1 << 5)|(ss[sIdx+1] - 'A');
		//_insertTag(((key2 << 3)|(TAG_LEN+1)),tmpPro);
        //
    }
	key1 = ((key1&ZEROPADDING4) << 5) | (ss[sIdx] - 'A');
	tmpPro.nSecond = ++pos;
	_insertTag(((key1 << 3)|TAG_LEN),tmpPro);
}

///< 创建序列标签索引 >
void CTagFlow::CreateTagIndex(const vector<PROTEIN_STRUCT> &proList)
{
	Clear();
	int nTagNum = 0;
	unordered_map<tag_val, vector<UINT_UINT> >::iterator it;
    for(size_t pIdx = 0; pIdx < proList.size(); ++pIdx)
    {

        int endPos = (int)proList[pIdx].strProSQ.length();
		if(endPos < TAG_LEN)
		{
			continue;
		}
		_GenerateTags(proList[pIdx].strProSQ, pIdx);
		//string preTag = "*";
		//preTag.append(proList[pIdx].strProSQ.substr(0, TAG_LEN - 1));
  //      for(int sIdx = TAG_LEN - 1; sIdx < endPos; ++sIdx)
  //      {
		//	string tag = preTag.substr(1);
		//	tag.push_back(proList[pIdx].strProSQ[sIdx]);
		//	preTag = tag;
		//	_ChangeItoL(tag);    // 将序列中的I换成L
  //          long int key =  _GetKey(tag);  // 获取编码值
		//	UINT_UINT tmpPro;
		//	tmpPro.nFirst = pIdx;         // 蛋白ID
		//	tmpPro.nSecond = sIdx + 1 - TAG_LEN;  // 在蛋白序列中的起始位置
		//	it = m_mapKey2Pos.find(key);
		//	if(it == m_mapKey2Pos.end()) // 倒排表中不存在该标签
		//	{
		//		vector<UINT_UINT> newVct;
		//		newVct.push_back(tmpPro);
		//		m_mapKey2Pos[key] = newVct;
		//	} else {
		//		(it->second).push_back(tmpPro);
		//	}
  //      }
    }

	_createMassHash(proList);  // 创建质量哈希表

}

// create mass-protein hash table 
void CTagFlow::_createMassHash(const vector<PROTEIN_STRUCT> &proList)
{
	for(int i=0; i<proList.size(); ++i){
		int massIdx = int(proList[i].lfMass+0.5);
		if(massIdx > MAX_HASH_SIZE){
			v_mass2pro[MAX_HASH_SIZE].push_back(i);
		}else{
			v_mass2pro[massIdx].push_back(i);
		}
	}
}


void CTagFlow::CreateTagIndex1(TAG_ITEM *vTagTbl, const vector<PROTEIN_STRUCT> &proList, long int nTagSize)
{
	if(0 == nTagSize)
	{
		return;
	}

    TAG_ITEM tagItem;
	int nTagNum = 0;
    for(size_t pIdx = 0; pIdx < proList.size(); ++pIdx)
    {
        tagItem.nProID = pIdx;
        int endPos = (int)proList[pIdx].strProSQ.length() - TAG_LEN;
        for(int sIdx = 0; sIdx <= endPos; ++sIdx)
        {
            string tag = proList[pIdx].strProSQ.substr(sIdx, TAG_LEN);
			_ChangeItoL(tag);
            long int key =  _GetKey(tag);
            tagItem.nPos = sIdx;
            tagItem.nKey = key;
			vTagTbl[nTagNum++] = tagItem;
        }
    }
    //cout << "[Info] Number of TAGs: " << nTagNum << endl;
    sort(vTagTbl, vTagTbl + nTagNum, _TagValueCmp);

    //for(int i = 0; i < m_nTagNum; ++i)
    //{
    //    //每个key值对应含相应的Tag的第一个蛋白在m_vTagTbl中的编号，从1开始
    //    if(m_mapKey2Pos.find(m_vTagTbl[i].nKey) == m_mapKey2Pos.end())
    //        m_mapKey2Pos.insert(pair<int, size_t>(m_vTagTbl[i].nKey, i));
    //}

	//GlobalMemoryStatus(&memStatus);
	//cout<<"Memory3: Avali Pspace = "<<memStatus.dwAvailVirtual<<endl<<endl;

	if(nTagNum > 0)
	{
		long int preKey = vTagTbl[0].nKey;
		vector<UINT_UINT> candiPros; // proid, tag position
		UINT_UINT tmpPro;
		int idx = 0;
		while(idx < nTagNum)
		{
			preKey = vTagTbl[idx].nKey;
			tmpPro.nFirst = vTagTbl[idx].nProID;
			tmpPro.nSecond = vTagTbl[idx].nPos;
			candiPros.push_back(tmpPro);
			++idx;
			while(idx < nTagNum && preKey == vTagTbl[idx].nKey)
			{
				tmpPro.nFirst = vTagTbl[idx].nProID;
				tmpPro.nSecond = vTagTbl[idx].nPos;
				candiPros.push_back(tmpPro);
				++idx;
			}
			//m_mapKey2Pos.insert(pair<int, vector<UINT_UINT> >(preKey, candiPros));
			m_mapKey2Pos[preKey] = candiPros;
			vector<UINT_UINT> ().swap(candiPros);	
		}
	}

//    FILE *ftest = fopen("tag_table.txt", "wt");
//    for(int j = 0; j < m_nTagNum; ++j)
//        fprintf(ftest, "%d %u\n", m_vTagTbl[j].nKey, m_vTagTbl[j].nProID);
//    fclose(ftest);
//
//    FILE *ftest1 = fopen("tag_index.txt", "wt");
//    for(int j = 0; j < TAG_INDEX_SIZE; ++j)
//        fprintf(ftest, "%d\n", m_vTagIndex[j]);
//    fclose(ftest1);
}

void CTagFlow::QueryTag(int massTol, const TAGRES &tag, map<size_t, int> &mapID2Pos,
                        vector<PROID_NODE> &vCandiPro, const vector<PROTEIN_STRUCT> &proList)
{
	//if(tag.strTag.compare("NMMN") == 0)
	//	cout<<"NMMN"<<endl;
	tag_val key = (_getKey(tag.strTag) << 3)|(tag.strTag.length());

    //cout<<tag.strTag.c_str()<<" "<<tag.lfFlankingMass[0]<<" "<<tag.lfFlankingMass[1]<<endl;
    unordered_map<tag_val, vector<UINT_UINT> >::iterator it = m_mapKey2Pos.find(key);
    if(it == m_mapKey2Pos.end())
    {
        return;
    }
	for(size_t p = 0; p < (it->second).size(); ++p)
    {
        try{
            double deltaMass = 0.0;
			size_t proID = it->second[p].nFirst;
			size_t tagPos = it->second[p].nSecond;
			if(_CheckCandiPro(massTol, proList[proID].strProSQ, tag.lfFlankingMass, tagPos, deltaMass) == true)
			//if(proList[proID].strProSQ.length() < 300)
            {
				//cout<<tag.strTag.c_str()<<endl;
                if(mapID2Pos.find(proID) == mapID2Pos.end())
                {
                    mapID2Pos.insert(pair<size_t, int>(proID, (int)vCandiPro.size()));
                    PROID_NODE tmpCandiPro;
                    tmpCandiPro.lfScore = tag.lfScore;
                    tmpCandiPro.nProID = proID;
                    TAG_INFO tmpTagInfo(tagPos, deltaMass, tag.vmodID);
                    tmpCandiPro.vTagInfo.push_back(tmpTagInfo);
                    vCandiPro.push_back(tmpCandiPro);
                } else {
                    vCandiPro[mapID2Pos[proID]].lfScore += tag.lfScore;
                    TAG_INFO tmpTagInfo(tagPos, deltaMass, tag.vmodID);
                    vCandiPro[mapID2Pos[proID]].vTagInfo.push_back(tmpTagInfo);
                }
            }
        } catch(bad_alloc &ba) {
            cout<<"[error] CTagFlow::QueryTag() Not enough memory!" << ba.what() << endl;
        }
    }
}
