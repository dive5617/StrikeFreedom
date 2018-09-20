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

#include "PrePTMForm.h"
#include "TagSearcher.h"

//#define _DEBUG_TAG


CTagSearcher::CTagSearcher()
{
	m_peaks.reserve(MAX_ION_NUM);
	m_cTags = new CPriorityQueue(MAX_TAG_NUM);
	m_pTrace = Clog::getInstance();
}

CTagSearcher::~CTagSearcher()
{
	delete m_cTags;
	if (m_pTrace){
		m_pTrace = NULL;
	}
}

void CTagSearcher::Init(CMapAAMass *mapAAMass, CPrePTMForm *cPTMForms, double fragTol)
{
    
    int i, cIdx;
    m_lfMaxAAMass = 0.0;
    m_lfMinAAMass = AAMass_M_mono;
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
		cPTMForms->GetVarIDsbyAA(ch, vModIDs);
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
void CTagSearcher::PreProcess(const SPECTRUM *spectra)
{
	//cout<<"Preprocess..."<<endl;
	m_peaks.clear();
	for(int pIdx = 0; pIdx <  spectra->nPeaksNum; ++pIdx)
	{
		m_peaks.push_back(FRAGMENT_ION(spectra->vPeaksTbl[pIdx].lfMz, spectra->vPeaksTbl[pIdx].lfIntens));
	}

	sort(m_peaks.begin(), m_peaks.end(), FRAGMENT_ION::PeakMzIncCmp);
	double maxPrecur = 0;
	for (int i = 0; i < spectra->vPrecursors.size(); ++i){
		maxPrecur = max(maxPrecur, spectra->vPrecursors[i].lfPrecursorMass);
	}
	int p = m_peaks.size()-1;
	for (; p >= 0; --p){
		if (m_peaks[p].lfMz < maxPrecur - 50.0){
			break;
		}
	}
	m_peaks.erase(m_peaks.begin() + (p + 1), m_peaks.end());

	m_nPeaksNum = m_peaks.size();

	if(0 == m_nPeaksNum)
	{
		return;
	}

	int topk = TOPINTEN;
	if(m_nPeaksNum > topk)
	{
		sort(m_peaks.begin(), m_peaks.end(), FRAGMENT_ION::PeakIntDesCmp);
		m_nPeaksNum = topk;
	}
	sort(m_peaks.begin(), m_peaks.begin() + m_nPeaksNum, FRAGMENT_ION::PeakMzIncCmp);

	MergeClosePeak(m_peaks, m_nPeaksNum);

	//cout<<m_nPeaksNum<<endl;
	//cout<<"[Info] End of process"<<endl;
}

void CTagSearcher::MergeClosePeak(vector<FRAGMENT_ION> &vPeaks, int &nPeaksNum)
{
	if (nPeaksNum == 0)
		return;
	vector<FRAGMENT_ION> newT;
	double curMz = vPeaks[0].lfMz;
	double curInten = vPeaks[0].lfIntens;
	double curD = curMz*m_lfFragTol / 1000000;
	FRAGMENT_ION p(curMz, curInten);
	newT.push_back(p);
	for (size_t i = 1; i < nPeaksNum; ++i) {
		double newMz = vPeaks[i].lfMz;
		if (newMz >= curMz - curD && newMz <= curMz + curD) {
			newT.back().lfIntens = curInten + vPeaks[i].lfIntens;
			//newT.back().m_lfMz = (newMz + curMz) / 2;
			newT.back().lfMz = (curMz * curInten + newMz * vPeaks[i].lfIntens) / newT.back().lfIntens;
		}
		else {
			FRAGMENT_ION p(vPeaks[i].lfMz, vPeaks[i].lfIntens);
			newT.push_back(p);
		}
		curMz = newT.back().lfMz;
		curInten = newT.back().lfIntens;
		curD = curMz*m_lfFragTol / 1000000;
	}
	sort(newT.begin(), newT.end(), FRAGMENT_ION::PeakMzIncCmp);
	vPeaks.swap(newT);
	nPeaksNum = vPeaks.size();
}


void CTagSearcher::_FindTag(const vector< vector<NODE> > & vEdges, size_t tCurrLen, size_t tCurrPos, const int tagLen)
{
    //cout<<"_FindTag: "<<tCurrLen<<" "<<tCurrPos<<" "<<vEdges.size()<<endl;
	if((int)tCurrLen == tagLen)
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
		_FindTag(vEdges, tCurrLen + 1, vEdges[tCurrPos][i].tPos, tagLen);
		m_lfTmpScore -= vEdges[tCurrPos][i].lfScore;
		m_strTmpTag.pop_back();   //erase(m_strTmpTag.end()-1);
		m_vTagModIDs.pop_back();  //erase(m_vTagModIDs.begin() + m_vTagModIDs.size() - 1);
	}
}

void CTagSearcher::_FindTag(const vector< vector<NODE> > & vEdges, size_t tCurrLen, size_t tCurrPos, const int tagLen, bool noPostfix)
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
		_FindTag(vEdges, tCurrLen + 1, vEdges[tCurrPos][i].tPos, tagLen, noPostfix);
		m_lfTmpScore -= vEdges[tCurrPos][i].lfScore;
		m_strTmpTag.pop_back();   // erase(m_strTmpTag.end()-1)
		m_vTagModIDs.pop_back();  // erase(m_vTagModIDs.begin() + m_vTagModIDs.size() - 1)
	}
}

void CTagSearcher::GeneTag(const SPECTRUM *spectra, const bool bTagLenInc)
{
	PreProcess(spectra);

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
	const double BM25_K1 = 0.5;
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
	//// generate 3-tag
	//for(int i = 0; i < m_nPeaksNum; ++i)
	//{
	//	if(vIn[i] == 0){
	//		m_lfFlankingMass[0] = m_peaks[i].lfMz;
	//		m_lfTmpScore = 0;
	//		m_strTmpTag.clear();
	//		_FindTag(spectra, vEdges, 0, i, 3, true);
	//	}
	//}

	m_vTagModIDs.clear();
	if (bTagLenInc){
		// generate 5-tag
		_ExtractTags(vEdges, TAG_LEN + 1);  
		if (m_cTags->GetTagSize() < 50){
			_ExtractTags(vEdges, TAG_LEN); // generate 4-tag
		}
	}
	else{ // generate 4-tag
		_ExtractTags(vEdges, TAG_LEN);
	}
	//wzz
	//m_cTags->Print();
#ifdef _DEBUG_TAG

	m_cTags->Print();

#endif
}

void CTagSearcher::_ExtractTags(const vector< vector<NODE> > & vEdges, const int tagLen)
{
	for (int i = 0; i < m_nPeaksNum; ++i)
	{
		m_lfFlankingMass[0] = m_peaks[i].lfMz;
		m_lfTmpScore = 0;
		m_strTmpTag.clear();
		_FindTag(vEdges, 0, i, tagLen);
	}
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
	m_cTags->SortTagsBySeq();   // sorted by sequence and score
	int tagNum = m_cTags->GetTagSize();
	TAGRES pre;
	const double flankingTol = 50.0;
	for(int i = 0; i < tagNum; ++i)
	{
		TAGRES tmpTag = m_cTags->GetTagItem(i);
		//cout << tmpTag.strTag << "?" << endl;
		if (tmpTag.strTag == pre.strTag && fabs(tmpTag.lfFlankingMass[0] - pre.lfFlankingMass[0]) < flankingTol && fabs(tmpTag.lfFlankingMass[1] - pre.lfFlankingMass[1]) < flankingTol){
			continue;
		}
		//cout << tmpTag.strTag << "!" << endl;
		pre = tmpTag;
		// TODO? 由于不知道是b离子（不含水）还是y离子（含水），因此会有一个大小至少为18的误差
		tmpTag.lfFlankingMass[1] = precurMass + IonMass_Proton - tmpTag.lfFlankingMass[1];   //precurMass - IonMass_Proton - m_lfMASS_H2O - tmpTag.lfFlankingMass[1];  
	    tagFlow->QueryTag(massTol, tmpTag, mapID2Pos, vCandiPro, proList);
		_ReverseTag(tmpTag);   // TODO? what if the tag seq is palindrome? 
		tagFlow->QueryTag(massTol, tmpTag, mapID2Pos, vCandiPro, proList);
	}
	sort(vCandiPro.begin(), vCandiPro.end());


	if((int)vCandiPro.size() > MAX_CANDIPRO_NUM)
	{
	    vCandiPro.erase(vCandiPro.begin(), vCandiPro.begin()+((int)vCandiPro.size() - MAX_CANDIPRO_NUM));
	}
	
	#ifdef _DEBUG_TAG
	FILE *fp = fopen("tmp\\candidates.txt", "w");
	fprintf(fp, "--------------- candidate proteins ------------------\n");
	for (int k = (int)vCandiPro.size() - 1; k >= 0; --k)
	{
		fprintf(fp, "%d %d %f\n%s\n%s\n\n", k, vCandiPro[k].nProID, vCandiPro[k].lfScore, proList[vCandiPro[k].nProID].strProAC.c_str(), proList[vCandiPro[k].nProID].strProSQ.c_str());
		//fprintf(fp, "%f %s %d\n", vCandiPro[k].lfScore, proList[vCandiPro[k].nProID].strProAC.c_str(), vCandiPro[k].vTagInfo.size());
		//fprintf(fp, "%s\n", proList[vCandiPro[k].nProID].strProSQ.c_str());
		//	    for(int tt = 0; tt < (int)vCandiPro[k].vTagInfo.size(); ++tt)
		//            cout<<vCandiPro[k].vTagInfo[tt].nPos<<" "<<vCandiPro[k].vTagInfo[tt].lfFlankingMass[0]<<endl;
	}
	fprintf(fp, "-------------------end---------------------------------\n");
	fclose(fp);
	#endif
	
	//cout<<"[Info] Candidate proteins: "<<vCandiPro.size()<<endl;

}

double CTagSearcher::GetMinAAMass() const
{
	return m_lfMinAAMass;
}
double CTagSearcher::GetMaxAAMass() const
{
	return m_lfMaxAAMass;
}

void CTagSearcher::GetTopPeaks(vector<FRAGMENT_ION> &peaks) const
{
	peaks = m_peaks;
}

// 创建谱峰质量哈希表，vHashTable[i]中保存了质量大于等于i的第一根谱峰的下标
void CTagSearcher::GetSpectraMassHash(vector<int> &vHashTable, const int FACTOR)
{
	fill(vHashTable.begin(), vHashTable.end(), -1);
	for (int i = m_nPeaksNum-1; i >= 0; --i)
	{
		if (int(m_peaks[i].lfMz*FACTOR) < vHashTable.size()){
			vHashTable[int(m_peaks[i].lfMz*FACTOR)] = i;
		}
	}
	int prePos = m_nPeaksNum-1;
	if (m_nPeaksNum == 0)
	{
		return;
	}
	for (int m = (int)(m_peaks.back().lfMz*FACTOR); m >= 0; --m)
	{
		if (vHashTable[m] == -1)
		{
			vHashTable[m] = prePos;
		}
		else {
			prePos = vHashTable[m];
		}
	}
}

// generate mass hash table for given peaks 
void CTagSearcher::GeneratePeakHashTable(vector<FRAGMENT_ION> &vPeaks, vector<int> &vHashTable, const int FACTOR)
{
	fill(vHashTable.begin(), vHashTable.end(), -1);
	for (int i = vPeaks.size() - 1; i >= 0; --i)
	{
		vHashTable[int(vPeaks[i].lfMz*FACTOR)] = i;
	}
	int prePos = vPeaks.size() - 1;
	if (vPeaks.empty())
	{
		return;
	}
	for (int m = (int)(vPeaks.back().lfMz*FACTOR); m >= 0; --m)
	{
		if (vHashTable[m] == -1)
		{
			vHashTable[m] = prePos;
		}
		else {
			prePos = vHashTable[m];
		}
	}
}
