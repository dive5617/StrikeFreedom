/*
 *  TagSearcher.cpp
 *
 *  Created on: 2014-01-03
 *  Author: luolan
 */

#include <iostream>
#include <cstdio>
#include <cstring>
#include <string>
#include <algorithm>
#include <cmath>
#include <windows.h>

#include "PrePTMForm.h"
#include "TagSearcher.h"

//#define _DEBUG_TEST3

bool PeakIntDesCmp(const MZ_SCOPE_ION &d1, const MZ_SCOPE_ION &d2)
{
	return d1.lfIntens > d2.lfIntens;
}
bool PeakMzIncCmp(const MZ_SCOPE_ION &d1, const MZ_SCOPE_ION &d2)
{
	return d1.lfMz < d2.lfMz;
}

CTagSearcher::CTagSearcher()
{
	m_peaks = new MZ_SCOPE_ION[MAX_ION_NUM];
	m_cTags = new CPriorityQueue(MAX_TAG);
}

CTagSearcher::~CTagSearcher()
{
	delete[]m_peaks;
	delete m_cTags;
}

void CTagSearcher::Init(CMapAAMass *mapAAMass, CPrePTMForm *cPTMForms, double fragTol)
{
    // Read the config, get the PTM information
    int i, cIdx;
    m_lfMaxAAMass = 0.0;
    m_lfMinAAMass = 0.0;
	m_lfMASS_H2O = 2*IonMass_Mono_H + IonMass_Mono_O;
	m_lfFragTol = fragTol;

    MODAA addModAA;
    for(i = 0; i < 26; ++i)
    {   //不加修饰的20种氨基酸
        addModAA.cAA = 'A' + i;
        addModAA.lfMass = mapAAMass->GetAAMass('A' + i);
        addModAA.nModID = -1;
        modAATbl.push_back(addModAA);
        if(addModAA.lfMass > m_lfMaxAAMass)
		{
            m_lfMaxAAMass = addModAA.lfMass;
		}
        if(addModAA.lfMass < m_lfMinAAMass)
		{
            m_lfMinAAMass = addModAA.lfMass;
		}
    }

	for(i = 0; i < 26; ++i)
    {
		char ch = 'A' + i;
		addModAA.cAA = ch;

		vector<int> vModIDs;
		cPTMForms->GetAllIDsbyAA(ch, vModIDs);
		for(cIdx = 0; cIdx < (int)vModIDs.size(); ++cIdx)
        {   //加修饰的氨基酸
			addModAA.lfMass = mapAAMass->GetAAMass(ch) + cPTMForms->GetMassbyID(vModIDs[cIdx]);
            addModAA.nModID = vModIDs[cIdx];
            modAATbl.push_back(addModAA);
            if(addModAA.lfMass > m_lfMaxAAMass)
			{
                m_lfMaxAAMass = addModAA.lfMass;
			}
            if(addModAA.lfMass < m_lfMinAAMass)
			{
                m_lfMinAAMass = addModAA.lfMass;
			}
       }
    }
#ifdef _DEBUG_TEST1
    for(i = 0; i < (int)modAATbl.size(); ++i)
    {
        cout<<i<<" "<<modAATbl[i].cAA<<" "<<modAATbl[i].lfMass<<" "<<modAATbl[i].nModID<<endl;
    }
#endif

}
void CTagSearcher::_PreProcess(const SPECTRUM *spectra)
{
	//cout<<"Preprocess..."<<endl;
	for(int pIdx = 0; pIdx <  spectra->nPeaksNum; ++pIdx)
	{
		m_peaks[pIdx].lfMz = spectra->vPeaksTbl[pIdx].lfMz;
        m_peaks[pIdx].lfIntens = spectra->vPeaksTbl[pIdx].lfIntens;
	}
	
    m_nPeaksNum = spectra->nPeaksNum;
	if(0 == m_nPeaksNum)
	{
		return;
	}

	int topk = TOPINTEN;
	if(m_nPeaksNum > topk)
	{
		sort(m_peaks, m_peaks + m_nPeaksNum, PeakIntDesCmp);
		sort(m_peaks, m_peaks + topk, PeakMzIncCmp);
		m_nPeaksNum = topk;
	}
	//cout<<m_nPeaksNum<<endl;
	//cout<<"[Info] End of process"<<endl;
}

void CTagSearcher::_FindTag(const SPECTRUM *spectra, const vector< vector<NODE> > & vEdges, size_t tCurrLen, size_t tCurrPos)
{
    //cout<<"_FindTag: "<<tCurrLen<<" "<<tCurrPos<<" "<<vEdges.size()<<endl;
	if((int)tCurrLen == TAG_LEN)
	{
		TAGRES tag;
		tag.lfScore = m_lfTmpScore;
		tag.strTag = m_strTmpTag;
		tag.vmodID = m_vTagModIDs;
			
		m_lfFlankingMass[1] = m_peaks[tCurrPos].lfMz;
		memcpy(tag.lfFlankingMass, m_lfFlankingMass, sizeof(m_lfFlankingMass));
		m_cTags->PushNode(tag);
		return;
	}
	for(size_t i = 0;i < vEdges[tCurrPos].size();++i)
	{
		m_strTmpTag.push_back(modAATbl[vEdges[tCurrPos][i].uName].cAA);
		m_vTagModIDs.push_back(modAATbl[vEdges[tCurrPos][i].uName].nModID);
		m_lfTmpScore += vEdges[tCurrPos][i].lfScore;
		_FindTag(spectra, vEdges, tCurrLen + 1, vEdges[tCurrPos][i].tPos);
		m_lfTmpScore -= vEdges[tCurrPos][i].lfScore;
		m_strTmpTag.erase(m_strTmpTag.end()-1);
		m_vTagModIDs.erase(m_vTagModIDs.begin() + m_vTagModIDs.size() - 1);
	}
}

void CTagSearcher::_FindTag(const SPECTRUM *spectra, const vector< vector<NODE> > & vEdges, size_t tCurrLen, size_t tCurrPos, const int tagLen, bool noPostfix)
{
	//cout<<"_FindTag: "<<tCurrLen<<" "<<tCurrPos<<" "<<vEdges.size()<<endl;
	if((int)tCurrLen == tagLen)
	{
		if((!noPostfix) || (noPostfix && vEdges[tCurrPos].empty())){
			TAGRES tag;
			tag.lfScore = m_lfTmpScore;
			tag.strTag = m_strTmpTag;
			tag.vmodID = m_vTagModIDs;
			
			m_lfFlankingMass[1] = m_peaks[tCurrPos].lfMz;
			memcpy(tag.lfFlankingMass, m_lfFlankingMass, sizeof(m_lfFlankingMass));
			m_cTags->PushNode(tag);
		}
		return;
	}
	for(size_t i = 0;i < vEdges[tCurrPos].size();++i)
	{
		m_strTmpTag.push_back(modAATbl[vEdges[tCurrPos][i].uName].cAA);
		m_vTagModIDs.push_back(modAATbl[vEdges[tCurrPos][i].uName].nModID);
		m_lfTmpScore += vEdges[tCurrPos][i].lfScore;
		_FindTag(spectra, vEdges, tCurrLen + 1, vEdges[tCurrPos][i].tPos);
		m_lfTmpScore -= vEdges[tCurrPos][i].lfScore;
		m_strTmpTag.pop_back();   // erase(m_strTmpTag.end()-1)
		m_vTagModIDs.pop_back();  // erase(m_vTagModIDs.begin() + m_vTagModIDs.size() - 1)
	}
}

void CTagSearcher::GeneTag(const SPECTRUM *spectra)
{
	_PreProcess(spectra);

	m_cTags->Clear();

	vector< vector<NODE> > vEdges;
	vector<UCHAR> vIn(m_nPeaksNum, 0);
	vector<UCHAR> vOut(m_nPeaksNum, 0);

	vector<double> smallWnd;
	smallWnd.push_back(0.0);
	if(1 == TAGDashift)
	{
		smallWnd.push_back(-DIFF_13C);
		smallWnd.push_back(DIFF_13C);
	}

	int nodeNum = 0;
	double tmpVol, dist;
	const double BM25_K1 = 0.3;
	for(int i = 0; i < m_nPeaksNum; ++i)
	{
		//对m_lfMaxAAMass和m_lfMinAAMass考虑质量误差窗口
		tmpVol = m_peaks[i].lfMz * m_lfFragTol / 1000000;

		for(int j = i + 1; j < m_nPeaksNum; ++j)
		{
			
			double deltaMZ = m_peaks[j].lfMz - m_peaks[i].lfMz;

			if(deltaMZ > m_lfMaxAAMass + tmpVol)
			{
				break;
			}
			if(deltaMZ < m_lfMinAAMass - tmpVol)
			{
				continue;
			}

			dist = 2 * (m_peaks[j].lfMz / (1 - m_lfFragTol / 1000000) - m_peaks[j].lfMz);
			for(size_t shift = 0; shift < smallWnd.size(); ++shift)
			{
				deltaMZ += smallWnd[shift];
				for(size_t k = 0; k < modAATbl.size(); ++k)
				{
					if(modAATbl[k].lfMass < 0.001 || modAATbl[k].cAA == 'I') //生成TAG的时候只考虑L
					{
						continue;
					}
					if(modAATbl[k].lfMass >= deltaMZ - dist && modAATbl[k].lfMass <= deltaMZ + dist)
					{
						++vIn[j];
						++vOut[i];
						nodeNum += 2;
					}
				}
			}
		}
	}
	NODE tmpNode;
	for(int i = 0; i < m_nPeaksNum; ++i)
	{
	    vector<NODE> vTmpEdges;
		tmpVol = m_peaks[i].lfMz * m_lfFragTol / 1000000;
		for(int j = i + 1;j < m_nPeaksNum;++j)
		{
			double deltaMZ = m_peaks[j].lfMz - m_peaks[i].lfMz;

			if(deltaMZ > m_lfMaxAAMass + tmpVol)
			{
				break;
			}
			if(deltaMZ < m_lfMinAAMass - tmpVol)
			{
				continue;
			}

			dist = 2 * (m_peaks[j].lfMz / (1 - m_lfFragTol / 1000000) - m_peaks[j].lfMz);
		
			for(size_t shift = 0; shift < smallWnd.size(); ++shift)
			{
				deltaMZ += smallWnd[shift];
				for(size_t k = 0; k < modAATbl.size(); ++k)
				{
					if(modAATbl[k].lfMass < 0.001 || modAATbl[k].cAA == 'I')
					{
						continue;
					}
					if(modAATbl[k].lfMass >= deltaMZ - dist && modAATbl[k].lfMass <= deltaMZ + dist)
					{
						//double score = (m_peaks[j].lfIntens + m_peaks[i].lfIntens) *
						//    (1 + exp(-fabs(modAATbl[k].lfMass - deltaMZ) / m_peaks[j].lfMz * 1000000)) / (vIn[j] + vOut[i]);

						/*double degree;
						if(i > 0 && j < m_nPeaksNum)
							degree = vIn[j]/_maxVal(vIn[j-1], vIn[j], vIn[j+1]) + vOut[i]/_maxVal(vOut[i-1], vOut[i], vOut[i+1]);
						else if(i > 0)
							degree = vIn[j]/_maxVal(vIn[j-1], vIn[j], vIn[j+1]) + vOut[i]/_maxVal(vOut[i], vOut[i+1]);
						else if(j < m_nPeaksNum)
							degree = vIn[j]/_maxVal(vIn[j-1], vIn[j]) + vOut[i]/_maxVal(vOut[i-1], vOut[i], vOut[i+1]);
						else degree = vIn[j]/_maxVal(vIn[j-1], vIn[j]) + vOut[i]/_maxVal(vOut[i], vOut[i+1]);
						double IDF = 1 + 2.0 / degree;*/

						double IDF = log10((nodeNum + 0.5 - (vIn[j]+vOut[i])) / (vIn[j]+vOut[i]+0.5));
						double TF = (BM25_K1 + 1) * (m_peaks[j].lfIntens + m_peaks[i].lfIntens) / (BM25_K1 + m_peaks[j].lfIntens + m_peaks[i].lfIntens);
						double lfVol = fabs(modAATbl[k].lfMass - deltaMZ) / m_peaks[j].lfMz * 1000000;
						lfVol = lfVol > 40.0 ? 40.0 : lfVol;
						lfVol = 1.0 + cos( lfVol / 40 * pi / 2);
						double score = TF * IDF * lfVol;

						if(score > 0.0)
						{
							tmpNode.lfScore = score;
							tmpNode.uName = k;
							tmpNode.tPos = j;
							vTmpEdges.push_back(tmpNode);
							#ifdef _DEBUG_TEST3
							cout << "edge: " << m_peaks[i].lfMz << ' ' << m_peaks[j].lfMz << ' ';
							cout << modAATbl[tmpNode.uName].cAA << ' ' << modAATbl[tmpNode.uName].nModID << ' ' << tmpNode.lfScore << ' ' << endl;
							#endif
						}
					}
				}
			}
		}
        vEdges.push_back(vTmpEdges);
	}
#ifdef _DEBUG_TEST4
    for(int t1 = 0; t1 < (int)vEdges.size(); ++t1)
    {
        cout<<t1<<": "<<endl;
        for(int t2 = 0; t2 < (int)vEdges[t1].size(); ++t2)
            cout<<t2<<" "<<vEdges[t1][t2].uName<<" "<<vEdges[t1][t2].lfScore<<" "<<vEdges[t1][t2].tPos<<endl;
        cout<<endl;
    }
#endif
	// generate 3-tag
	//for(int i = 0; i < m_nPeaksNum; ++i)
	//{
	//	if(vIn[i] == 0){
	//		m_lfFlankingMass[0] = m_peaks[i].lfMz;
	//		m_lfTmpScore = 0;
	//		m_strTmpTag.clear();
	//		_FindTag(spectra, vEdges, 0, i, 3, true);
	//	}
	//}
	// generate 4-tag
	for(int i = 0; i < m_nPeaksNum; ++i)
	{
		m_lfFlankingMass[0] = m_peaks[i].lfMz;
		m_lfTmpScore = 0;
		m_strTmpTag.clear();
		_FindTag(spectra, vEdges, 0, i);
	}
	// generate 5-tag
	//for(int i = 0; i < m_nPeaksNum; ++i)
	//{
	//	m_lfFlankingMass[0] = m_peaks[i].lfMz;
 //       m_lfTmpScore = 0;
 //       m_strTmpTag.clear();
	//	_FindTag(spectra, vEdges, 0, i, TAG_LEN+1, false);
	//}

#ifdef _DEBUG_TEST3

	m_cTags->Print();

#endif
}

void CTagSearcher::_ReverseTag(TAGRES &tag)
{
	swap(tag.lfFlankingMass[0], tag.lfFlankingMass[1]);
	int l = 0, r = (int)tag.strTag.length() - 1;
	while(l < r)
	{
		swap(tag.strTag[l++], tag.strTag[r--]);
	}
}

///<summary> 
///母离子误差范围、母离子质量
/// 
///</summary>
void CTagSearcher::Run(int massTol, double precurMass, CTagFlow *tagFlow, const vector<PROTEIN_STRUCT> &proList, vector<PROID_NODE> &vCandiPro)
{
    vCandiPro.clear();
    map<size_t, int> mapID2Pos;

	int tagNum = m_cTags->GetTagSize();
	for(int i = 0; i < tagNum; ++i)
	{
		TAGRES tmpTag = m_cTags->GetTagItem(i);
		tmpTag.lfFlankingMass[1] = precurMass - IonMass_Proton - m_lfMASS_H2O - tmpTag.lfFlankingMass[1];

	    tagFlow->QueryTag(massTol, tmpTag, mapID2Pos, vCandiPro, proList);
		_ReverseTag(tmpTag);
		tagFlow->QueryTag(massTol, tmpTag, mapID2Pos, vCandiPro, proList);
	}
	sort(vCandiPro.begin(), vCandiPro.end());
	if((int)vCandiPro.size() > MAX_CANDIPRO_NUM)
	{
	    vCandiPro.erase(vCandiPro.begin(), vCandiPro.begin()+((int)vCandiPro.size() - MAX_CANDIPRO_NUM));
	}

	//cout<<"[Info] Candidate proteins: "<<vCandiPro.size()<<endl;
#ifdef _DEBUG_TEST3
	cout<<"--------------- candidate proteins ------------------"<<endl;
    for(int k = vCandiPro.size()-1; k >= 0; --k)
	{
		cout<<vCandiPro[k].lfScore<<" "<<proList[vCandiPro[k].nProID].nIsDecoy<<endl;
	    cout<<proList[vCandiPro[k].nProID].strProSQ.c_str()<<endl;
	    for(int tt = 0; tt < (int)vCandiPro[k].vTagInfo.size(); ++tt)
            cout<<vCandiPro[k].vTagInfo[tt].nPos<<" "<<vCandiPro[k].vTagInfo[tt].lfDeltaMass<<endl;
	}
	cout<<"-------------------end---------------------------------"<<endl;
#endif
}
