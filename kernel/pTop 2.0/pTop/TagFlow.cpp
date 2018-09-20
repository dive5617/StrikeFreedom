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
#include <algorithm>
#include<process.h>

#include "TagFlow.h"
#include "MapAAMass.h"
#include "PrePTMForm.h"

using namespace std;

CTagFlow::CTagFlow(CMapAAMass *cAAMass):
	m_nTotalProteinNum(1), m_cMassMap(cAAMass), m_lfMaxTruncatedMass(20000),
	m_bTagLenInc(false), m_nMaxTagLen(m_bTagLenInc ? TAG_LEN + 1 : TAG_LEN)
{
	m_pTrace = Clog::getInstance();
	m_mapKey2Pos.clear();
	long int frac = 1;
	for (int i = 0; i < m_nMaxTagLen; ++i)
	{
		m_vFrac.push_back(frac);
		frac *= 26;
	}
	for (int i = 0; i < ALPHABET_SIZE; ++i)
	{
		m_mapChar2Code[i] = i + 1;
	}
}

CTagFlow::CTagFlow(const CConfiguration *pParameter, CMapAAMass *cAAMass) :
	m_nTotalProteinNum(1), m_cMassMap(cAAMass), m_lfMaxTruncatedMass(pParameter->m_lfMaxTruncatedMass),
	m_bTagLenInc(pParameter->m_bTagLenInc), m_nMaxTagLen(m_bTagLenInc ? TAG_LEN + 1 : TAG_LEN)
{
	m_pTrace = Clog::getInstance();
    m_mapKey2Pos.clear();
    long int frac = 1;
	for (int i = 0; i < m_nMaxTagLen; ++i)
    {
        m_vFrac.push_back(frac);
        frac *= 26;
    }
	for (int i = 0; i < ALPHABET_SIZE; ++i)
	{
		m_mapChar2Code[i] = i+1;
	}
}

void CTagFlow::Clear()
{
	for(auto it = m_mapKey2Pos.begin(); it != m_mapKey2Pos.end(); ++it)
	{
		vector<UINT_UINT> ().swap(it->second.vProID_Pos);
		it->second.vProID_Pos.clear();
	}
	m_mapKey2Pos.clear();

}

CTagFlow::~CTagFlow()
{
	if (m_pTrace){
		m_pTrace = NULL;
	}
	if (m_cMassMap){
		m_cMassMap = NULL;
	}
}

long int CTagFlow::_GetKey(const string &tag)
{
    long int key = 0;
    size_t len = tag.length();
    if((int)len > m_nMaxTagLen)
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

bool CTagFlow::_CheckCandiPro(int massTol, const string &strProSQ, const TAGRES &tag, const int pos)
{
    string leftSQ = strProSQ.substr(0, pos);
    double leftMass = m_cMassMap->CalculateNeutralMass(leftSQ);
    double deltaMass =  tag.lfFlankingMass[0] - leftMass;
    //modMass = deltaMass;
    //cout<<leftSQ.c_str()<<" "<<leftMass<<endl;
	//cout<<leftSQ.substr(0, 10).c_str()<<" "<<leftMass<<" "<<deltaMass<<" ";
    // 这里不考虑质量为负数的修饰，当考虑时可以在最初添加一个较大的数值将所有的负质量修饰转化为正
	if (deltaMass > massTol + 500 || deltaMass < -massTol - m_lfMaxTruncatedMass) // deltaMass < -massTol - 500 may be N-term truncated
        return false;
    string rightSQ = strProSQ.substr(pos + tag.strTag.length());
    double rightMass = m_cMassMap->CalculateNeutralMass(rightSQ);
    deltaMass = tag.lfFlankingMass[1] - rightMass;
    //cout<<rightSQ.c_str()<<" "<<rightMass<<endl;
	//cout<<deltaMass<<endl;
	if (deltaMass > massTol + 500 || deltaMass < -massTol - m_lfMaxTruncatedMass) // deltaMass < -massTol - 500 may be C-term truncated
        return false;
    else return true;
}

// tag encoding
tag_val CTagFlow::GetKey(const string &tag)
{
	int len = tag.length();
    if((int)len > m_nMaxTagLen)
    {
        cout<<"[Error] Invalid tag: "<< tag <<endl;
        return -1;
    }
	tag_val key = 0;
    for(int i = 0; i < len; ++i)
    {
		key = ((key << 5) | (m_mapChar2Code[tag[i] - 'A']));
    }
    return key;
}
	
void CTagFlow::_insertTag(tag_val key, UINT_UINT &pro_pos)
{
	auto it = m_mapKey2Pos.find(key);
	if(it == m_mapKey2Pos.end()) // 倒排表中不存在该标签
    {
		m_mapKey2Pos[key] = DB_TAG_INFO(0);
		m_mapKey2Pos[key].vProID_Pos.push_back(pro_pos);
    } else {
        (it->second).vProID_Pos.push_back(pro_pos);
    }
}

// give protein sequence, generate the 3-tag, 4-tag, 5-tag
// binary coding method
void CTagFlow::_GenerateTags(const string &s, const int pro_id, const int tagLen, unordered_set<tag_val> &tagKeys)
{
	const int ZEROPADDING = (0xffffffff >> (32 - 5*(tagLen-1)));
	if (s.length() < tagLen)  return;
	string ss = s;
    _ChangeItoL(ss); // 将序列中的I换成L
	int sIdx = tagLen, endPos = ss.size(), pos = 0;
	string tag = ss.substr(0, tagLen);
	tag_val key = GetKey(tag);  // 获取编码值
	UINT_UINT tmpPro;
	tmpPro.nFirst = pro_id;         // 蛋白ID
	tmpPro.nSecond = pos;  // 在蛋白序列中的起始位置
	_insertTag(key, tmpPro);
	tagKeys.insert(key);

	for (sIdx = tagLen; sIdx < endPos; ++sIdx)
    {
		key = ((key&ZEROPADDING) << 5) | m_mapChar2Code[ss[sIdx] - 'A'];
		tmpPro.nSecond = ++pos;
		_insertTag(key, tmpPro);
		tagKeys.insert(key);
    }
}

///< 创建序列标签索引 >
void CTagFlow::CreateTagIndex(const vector<PROTEIN_STRUCT> &proList)
{
	m_pTrace->info("Create tag index...");
	if (proList.empty())  return;
	Clear();
	m_nTotalProteinNum = proList.size();
	int nTagNum = 0;
    for(size_t pIdx = 0; pIdx < proList.size(); ++pIdx)
    {

        int endPos = (int)proList[pIdx].strProSQ.length();
		if(endPos < TAG_LEN)
		{
			continue;
		}
		unordered_set<tag_val> tagKeys;
		_GenerateTags(proList[pIdx].strProSQ, pIdx, TAG_LEN, tagKeys);
		if (m_bTagLenInc){
			_GenerateTags(proList[pIdx].strProSQ, pIdx, TAG_LEN + 1, tagKeys);
		}

		for (auto it = tagKeys.begin(); it != tagKeys.end(); ++it){
			++ m_mapKey2Pos[*it].nFrequency;
		}
    }

	//_UpdateTagWeight();
}

void CTagFlow::_UpdateTagWeight()
{
	for (auto it = m_mapKey2Pos.begin(); it != m_mapKey2Pos.end(); ++it){
		unordered_set<int> proIds;
		for (int t = 0; t < it->second.vProID_Pos.size(); ++t){
			proIds.insert(it->second.vProID_Pos[t].nFirst);
		}
		it->second.nFrequency = proIds.size();
	}
}

//void CTagFlow::CreateTagIndex1(TAG_ITEM *vTagTbl, const vector<PROTEIN_STRUCT> &proList, long int nTagSize)
//{
//	if(0 == nTagSize)
//	{
//		return;
//	}
//
//    TAG_ITEM tagItem;
//	int nTagNum = 0;
//    for(size_t pIdx = 0; pIdx < proList.size(); ++pIdx)
//    {
//        tagItem.nProID = pIdx;
//        int endPos = (int)proList[pIdx].strProSQ.length() - TAG_LEN;
//        for(int sIdx = 0; sIdx <= endPos; ++sIdx)
//        {
//            string tag = proList[pIdx].strProSQ.substr(sIdx, TAG_LEN);
//			_ChangeItoL(tag);
//            long int key =  _GetKey(tag);
//            tagItem.nPos = sIdx;
//            tagItem.nKey = key;
//			vTagTbl[nTagNum++] = tagItem;
//        }
//    }
//    //cout << "[Info] Number of TAGs: " << nTagNum << endl;
//    sort(vTagTbl, vTagTbl + nTagNum, _TagValueCmp);
//
//    //for(int i = 0; i < m_nTagNum; ++i)
//    //{
//    //    //每个key值对应含相应的Tag的第一个蛋白在m_vTagTbl中的编号，从1开始
//    //    if(m_mapKey2Pos.find(m_vTagTbl[i].nKey) == m_mapKey2Pos.end())
//    //        m_mapKey2Pos.insert(pair<int, size_t>(m_vTagTbl[i].nKey, i));
//    //}
//
//	//GlobalMemoryStatus(&memStatus);
//	//cout<<"Memory3: Avali Pspace = "<<memStatus.dwAvailVirtual<<endl<<endl;
//
//	if(nTagNum > 0)
//	{
//		long int preKey = vTagTbl[0].nKey;
//		vector<UINT_UINT> candiPros; // proid, tag position
//		UINT_UINT tmpPro;
//		int idx = 0;
//		while(idx < nTagNum)
//		{
//			preKey = vTagTbl[idx].nKey;
//			tmpPro.nFirst = vTagTbl[idx].nProID;
//			tmpPro.nSecond = vTagTbl[idx].nPos;
//			candiPros.push_back(tmpPro);
//			++idx;
//			while(idx < nTagNum && preKey == vTagTbl[idx].nKey)
//			{
//				tmpPro.nFirst = vTagTbl[idx].nProID;
//				tmpPro.nSecond = vTagTbl[idx].nPos;
//				candiPros.push_back(tmpPro);
//				++idx;
//			}
//			//m_mapKey2Pos.insert(pair<int, vector<UINT_UINT> >(preKey, candiPros));
//			m_mapKey2Pos[preKey] = candiPros;
//			vector<UINT_UINT> ().swap(candiPros);	
//		}
//	}
//
////    FILE *ftest = fopen("tag_table.txt", "wt");
////    for(int j = 0; j < m_nTagNum; ++j)
////        fprintf(ftest, "%d %u\n", m_vTagTbl[j].nKey, m_vTagTbl[j].nProID);
////    fclose(ftest);
////
////    FILE *ftest1 = fopen("tag_index.txt", "wt");
////    for(int j = 0; j < TAG_INDEX_SIZE; ++j)
////        fprintf(ftest, "%d\n", m_vTagIndex[j]);
////    fclose(ftest1);
//}

void CTagFlow::QueryTag(int massTol, TAGRES &tag, map<size_t, int> &mapID2Pos,
                        vector<PROID_NODE> &vCandiPro, const vector<PROTEIN_STRUCT> &proList)
{
	//if(tag.strTag.compare("NMMN") == 0)
	//	cout<<"NMMN"<<endl;
	tag_val key = GetKey(tag.strTag);

    //cout<<tag.strTag.c_str()<<" "<<tag.lfFlankingMass[0]<<" "<<tag.lfFlankingMass[1]<<endl;
    unordered_map<tag_val, DB_TAG_INFO >::iterator it = m_mapKey2Pos.find(key);
    if(it == m_mapKey2Pos.end())
    {
        return;
    }
	double tag_weight = 1.0/it->second.nFrequency + 1.0;  // log10(1.0*m_nTotalProteinNum / it->second.nFrequency);
	for(size_t p = 0; p < (it->second).vProID_Pos.size(); ++p)
    {
        try{
			size_t proID = it->second.vProID_Pos[p].nFirst;
			size_t tagPos = it->second.vProID_Pos[p].nSecond;
			//cout << proList[proID].strProSQ.c_str() << endl;


			/*if (proList[proID].strProSQ.find("MLLILLSVALLALSSAQNLNEDVSQEESPSLIAGNPQGAPPQGGNKPQGPPSPPGKPQGPPPQGGNQPQGPPPPPGKPQGPPPQGGNKPQGPPPPGKPQGPPPQGDKSRSPRSPPGKPQGPPPQGGNQPQGPPPPPGKPQGPPPQGGNKPQGPPPPGKPQGPPPQGDNKSRSSRSPPGKPQGPPPQGGNQPQGPPPPPGKPQGPPPQGGNKPQGPPPPGKPQGPPPQGDNKSQSARSPPGKPQGPPPQGGNQPQGPPPPPGKPQGPPPQGGNKSQGPPPPGKPQGPPPQGGSKSRSSRSPPGKPQGPPPQGGNQPQGPPPPPGKPQGPPPQGGNKPQGPPPPGKPQGPPPQGGSKSRSARSPPGKPQGPPQQEGNNPQGPPPPAGGNPQQPQAPPAGQPQGPPRPPQGGRPSRPPQ") != string::npos)
			{
				cout << "Find it!!" << endl;
				cout << tag.strTag.c_str() << endl;
				cout << proList[proID].strProSQ << endl;
			}
*/
			/*if (proList[proID].strProSQ.find("MVKIVTVKTQAYQDQKPGTSGLRKRVKVFQSSANYAENFIQSIISTVEPAQRQEATLVVGGDGRFYMKEAIQLIARIAAANGIGRLVIGQNGILSTPAVSCIIRKIKAIGGIILTASHNPGGPNGDFGIKFNISNGGPAPEAITDKIFQISKTIEEYAVCPDLKVDLGVLGKQQFDLENKFKPFTVEIVDSVEAYATMLRSIFDFSALKELLSGPNRLKIRIDAMHGVVGPYVKKILCEELGAPANSAVNCVPLEDFGGHHPDPNLTYAADLVETMKSGEHDFGAAFDGDGDRNMILGKHGFFVNPSDSVAVIAANIFSIPYFQQTGVRGFARSMPTSGALDRVASATKIALYETPTGWKFFGNLMDASKLSLCGEESFGTGSDHIREKDGLWAVLAWLSILATRKQSVEDILKDHWQKYGRNFFTRYDYEEVEAEGANKMMKDLEALMFDRSFVGKQFSANDKVYTVEKADNFEYSDPVDGSISRNQGLRLIFTDGSRIVFRLSGTGSAGATIRLYIDSYEKDVAKINQDPQVMLAPLISIALKVSQLQERTGRTAPTVIT") != string::npos)
			{
				cout << "find it!!!!!!!!!!!!!!!!" << endl;
				cout << tag.strTag.c_str() << endl;
				cout << proID << endl;
			}*/
			if(_CheckCandiPro(massTol, proList[proID].strProSQ, tag, tagPos) == true)
			//if(proList[proID].strProSQ.length() < 300)

            {
				/*if (proList[proID].strProSQ.find("MVKIVTVKTQAYQDQKPGTSGLRKRVKVFQSSANYAENFIQSIISTVEPAQRQEATLVVGGDGRFYMKEAIQLIARIAAANGIGRLVIGQNGILSTPAVSCIIRKIKAIGGIILTASHNPGGPNGDFGIKFNISNGGPAPEAITDKIFQISKTIEEYAVCPDLKVDLGVLGKQQFDLENKFKPFTVEIVDSVEAYATMLRSIFDFSALKELLSGPNRLKIRIDAMHGVVGPYVKKILCEELGAPANSAVNCVPLEDFGGHHPDPNLTYAADLVETMKSGEHDFGAAFDGDGDRNMILGKHGFFVNPSDSVAVIAANIFSIPYFQQTGVRGFARSMPTSGALDRVASATKIALYETPTGWKFFGNLMDASKLSLCGEESFGTGSDHIREKDGLWAVLAWLSILATRKQSVEDILKDHWQKYGRNFFTRYDYEEVEAEGANKMMKDLEALMFDRSFVGKQFSANDKVYTVEKADNFEYSDPVDGSISRNQGLRLIFTDGSRIVFRLSGTGSAGATIRLYIDSYEKDVAKINQDPQVMLAPLISIALKVSQLQERTGRTAPTVIT") != string::npos)
				{
					cout << "find it!!!!!!!!!!!!!!!!" << endl;
					cout << tag.strTag.c_str() << endl;
					cout << proID << endl;
				}*/
				//cout<<tag.strTag.c_str()<<endl;
				//cout << proID << "!" << endl;
				//cout << tag.strTag.c_str() << " " << proList[proID].strProSQ.c_str() << endl;
				
                if(mapID2Pos.find(proID) == mapID2Pos.end())
                {
                    mapID2Pos.insert(pair<size_t, int>(proID, (int)vCandiPro.size()));
                    PROID_NODE tmpCandiPro;
					tmpCandiPro.lfScore = tag.lfScore*tag_weight;
                    tmpCandiPro.nProID = proID;
					/*if (proID == 4982) {
						cout << "ok ~~~~~~" << endl;
					}*/
					tag.nPos = tagPos;
                    tmpCandiPro.vTagInfo.push_back(tag);
                    vCandiPro.push_back(tmpCandiPro);
                } else {
					vCandiPro[mapID2Pos[proID]].lfScore += tag.lfScore*tag_weight;
					tag.nPos = tagPos;
					vCandiPro[mapID2Pos[proID]].vTagInfo.push_back(tag);
                }
            }
        } catch(bad_alloc &ba) {
            cout<<"[error] CTagFlow::QueryTag() Not enough memory!" << ba.what() << endl;
        }
    }
}

bool CTagFlow::isTagLenIncrease() const
{
	return m_bTagLenInc;
}
