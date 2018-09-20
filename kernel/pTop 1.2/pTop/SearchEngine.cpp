/*
 *  SearchEngine.cpp
 *
 *  Created on: 2013-10-28
 *  Author: luolan
 */

#include <iostream>
#include <cstdlib>
#include <fstream>
#include <cstring>
#include <algorithm>
#include <cmath>
#include <ctime>
#include <windows.h>

#include "BasicTools.h"
#include "SearchEngine.h"
//#define _DEBUG_REScore

using namespace std;
//FILE *ftest = fopen("test.txt", "w"); // for debug
bool PrSMPeaksCmp(const PROTEIN_SPECTRA_MATCH p1, const PROTEIN_SPECTRA_MATCH p2)
{
    return p1.nMatchedPeakNum < p2.nMatchedPeakNum;
}

bool DDCmp(const DOUBLE_DOUBLE d1, const DOUBLE_DOUBLE d2)
{
    return d1.lfa < d2.lfa;
}
bool DIntCmp(const DOUBLE_INT d1, const DOUBLE_INT d2)
{
    return d1.nb > d2.nb;
}

CSearchEngine::CSearchEngine(CMapAAMass *cMapMass, CPrePTMForm *cPTMForms, CTagFlow *cTagIndex, CTagSearcher *cTagSearcher, double fragTol, int precurTol):
	m_nPrecurTol(precurTol),  m_lfFragTol(fragTol), m_Spectra(NULL), m_mapAAMass(cMapMass),m_getPTMForm(cPTMForms), m_cTagFlow(cTagIndex), m_cTag(cTagSearcher),
	totalIntensity(0),baseline(0)
{
	
}

void CSearchEngine::Init(string &actType)
{
	calAX = false;
	calBY = false;
	calCZ = false;
	if(actType.compare("ETD") == 0)
	{
        calCZ = true;
	} else if(actType.compare("UVPD") == 0){
		calAX = true;
		calBY = true;
		calCZ = true;
	} else {
		calBY = true;
	}

	m_nPrecurID = 0;
	m_lfPrecurMass = 0.0;

	m_vAAMass.clear();
	_GetAllAAMasses();

	m_hashTbl.reserve(MAX_HASH_SIZE + 1);
}

void CSearchEngine::Reset(SPECTRUM &spec)
{
	m_nPrecurID = 0;
	m_lfPrecurMass = 0.0;

	m_vMatchedPeak.clear();

	m_vModID2Num.clear();
	m_vProSeqModNum.clear();
	m_hashTbl.clear();

	m_vAAMass.clear();
	_GetAllAAMasses();

	m_theoNFragIon.clear();
	m_theoCFragIon.clear();
	
	m_mapProLastModSite.clear();

	m_Spectra = &spec;
	m_vMatchedPeak.assign(m_Spectra->vPeaksTbl.size(), 0);
}

CSearchEngine::~CSearchEngine()
{
	m_mapAAMass = NULL;
	m_getPTMForm = NULL;
	m_cTagFlow = NULL;
	m_cTag = NULL;
}

// Seach for the protein in the tolerance window
int CSearchEngine::_BinSearch(const vector<PROTEIN_STRUCT> &m_ProteinList, double mass)
{
    int low = 0, mid, high = m_ProteinList.size() - 1;
    while(low <= high)
    {
        mid = (low + high) >> 1;
        if(fabs(m_ProteinList[mid].lfMass - mass) < DOUCOM_EPS) 
		{
			return mid;
		}
        if(m_ProteinList[mid].lfMass < mass) 
		{
			low = mid + 1;
		} else {
			high = mid - 1;
		}
    }
    return low;
}

// Check if the mod form is valid
bool CSearchEngine::_CheckModForm(int massIdx, int ptmIdx, const string &proSeq, int startPos, int endPos)
{
    //cout<<"_CheckModForm..."<<endl;
    int i = 0, modID = 0;
	int modCnt = (int)m_getPTMForm->GetModNum();  // 可变修饰数目
    bool nTerm = false;
    bool cTerm = false;
    map<int, int>::iterator it;

    // Create a vector (mod ID -> quantity), in order to check if the Set in the vertice is valid
	m_vModID2Num.clear();  // 记录该修饰组合中每种修饰的数目
	for(modID = 0; modID < modCnt; ++modID)
	{
        m_vModID2Num.push_back(0);
	}
	int sumModNumber = m_getPTMForm->m_ModForms[massIdx][ptmIdx].size();
    for(i = 0; i < sumModNumber; ++i)
    {
        ++m_vModID2Num[m_getPTMForm->m_ModForms[massIdx][ptmIdx][i]];
    }
    // Check whether the protein sequence have enough mod sites for this mod form
    if(0 == startPos && endPos == (int)proSeq.length())
    {
        for(modID = 0; modID < modCnt; ++modID)
        {
			if(m_getPTMForm->isNtermMod(modID) || m_getPTMForm->isCtermMod(modID))
				continue;
            if(m_vModID2Num[modID] > m_vProSeqModNum[modID])
            {
                return false;
            }
        }
    } else {
        vector<int> tmpNums(modCnt, 0); // 记录从 start ~ pos 这段序列上每种修饰可发生的数目
		vector<int> vModIDs;
        for(int sPos = startPos; sPos < endPos; ++sPos)
        {
            m_getPTMForm->GetAllIDsbyAA(proSeq[sPos], vModIDs);
            for(size_t i = 0; i < vModIDs.size(); ++i)
            {
                ++tmpNums[vModIDs[i]];
            }
        }
        for(modID = 0; modID < modCnt; ++modID)
        {

            if(m_vModID2Num[modID] > tmpNums[modID])
            {
                return false;
            }
        }
    }

    // Check the N-term mod and C-term mod must be only one
    if(m_getPTMForm->GetNCModNum() == 0)
	{
		return true;
	} else {
        for(i = 0; i < (int)m_getPTMForm->m_ModForms[massIdx][ptmIdx].size(); ++i)
        {
			bool isNterm = m_getPTMForm->isNtermMod(m_getPTMForm->m_ModForms[massIdx][ptmIdx][i]);
			bool isCterm = m_getPTMForm->isCtermMod(m_getPTMForm->m_ModForms[massIdx][ptmIdx][i]);
			if(isNterm == false && isCterm == false)
			{
				continue;
			}
            if(isNterm)
			{
				if(nTerm == false) nTerm = true;
				else return false; // more than 1 nterm mod
			}
			if(isCterm) 
			{
				if(cTerm == false) cTerm = true;
				else return false; // more than 1 cterm mod
			}
        }
    }
    return true;
}
// 计算碎片离子质荷比
//
void CSearchEngine::_GenerateIonsbyAAMassLst(vector<double> vProAAMass)
{
	int pIdx = 0;
	int nProLen = vProAAMass.size();
	m_theoNFragIon.clear();
	m_theoCFragIon.clear();
	vector<double> empty;
	m_theoNFragIon.assign(nProLen - 1, empty); // a,b,c
	m_theoCFragIon.assign(nProLen - 1, empty); // x,y,z

	if (calAX)
	{
		vector<double> theoAIon, theoXIon;
		theoAIon.push_back(- IonMass_Mono_O - IonMass_Mono_C + IonMass_Proton);  // a = [M] -C -O + Mp
		theoXIon.push_back(2 * IonMass_Mono_O + IonMass_Mono_C + IonMass_Proton); // x = [M] +2O +C +Mp
		for (pIdx = 0; pIdx < nProLen - 1; ++pIdx) // masses of N-term ions
		{
			theoAIon.push_back(vProAAMass[pIdx] + theoAIon[pIdx]);
			m_theoNFragIon[pIdx].push_back(theoAIon[pIdx + 1]);
		}
		m_lfPrecurMass = theoAIon[nProLen - 1] + vProAAMass[nProLen - 1] + 2 * IonMass_Mono_O + 2 * IonMass_Mono_H + IonMass_Mono_C;

		for (pIdx = nProLen - 1; pIdx > 0; --pIdx) // masses of C-term ions
		{
			theoXIon.push_back(vProAAMass[pIdx] + theoXIon[nProLen - 1 - pIdx]);
			m_theoCFragIon[nProLen - 1 - pIdx].push_back(theoXIon[nProLen - pIdx]);
		}
	}


	if (calBY)
	{
		vector<double> theoBIon, theoYIon;
		theoBIon.push_back(IonMass_Proton);  //CID [N] +, b = [M] + Mp
		theoYIon.push_back(IonMass_Mono_O + 2 * IonMass_Mono_H + IonMass_Proton); // y = [M] + O + 2H +Mp
		for (pIdx = 0; pIdx < nProLen - 1; ++pIdx) // masses of N-term ions
		{
			theoBIon.push_back(vProAAMass[pIdx] + theoBIon[pIdx]);
			m_theoNFragIon[pIdx].push_back(theoBIon[pIdx + 1]);
		}
		m_lfPrecurMass = theoBIon[nProLen - 1] + vProAAMass[nProLen - 1] + IonMass_Mono_O + 2 * IonMass_Mono_H;

		for (pIdx = nProLen - 1; pIdx > 0; --pIdx) // masses of C-term ions
		{
			theoYIon.push_back(vProAAMass[pIdx] + theoYIon[nProLen - 1 - pIdx]);
			m_theoCFragIon[nProLen - 1 - pIdx].push_back(theoYIon[nProLen - pIdx]);
		}
	}

	if (calCZ)    // [wrm?]考虑c/z和c-H/z+H
	{
		vector<double> theoCIon, theoZIon, theoCHIon, theoZHIon;
		theoCIon.push_back(3 * IonMass_Mono_H + IonMass_Mono_N + IonMass_Proton);  //ETD [N] + NH2 +, c = [M] + 3H + N + Mp
		theoZIon.push_back(IonMass_Mono_O - IonMass_Mono_N + IonMass_Proton); // z = [M] + O - N - H + 2Mp + e = [M] + O - N + Mp
		//theoCHIon.push_back(theoCIon[0] - IonMass_Mono_H);
		//theoZHIon.push_back(theoZIon[0] + IonMass_Mono_H);
		for (pIdx = 0; pIdx < nProLen - 1; ++pIdx) // masses of N-term ions
		{
			theoCIon.push_back(vProAAMass[pIdx] + theoCIon[pIdx]);
			//theoCHIon.push_back(vProAAMass[pIdx] + theoCHIon[pIdx]);
			m_theoNFragIon[pIdx].push_back(theoCIon[pIdx + 1]);
			//m_theoNFragIon[pIdx].push_back(theoCHIon[pIdx + 1]);
		}
		m_lfPrecurMass = theoCIon[nProLen - 1] + vProAAMass[nProLen - 1] + IonMass_Mono_O - IonMass_Mono_N - IonMass_Mono_H;

		for (pIdx = nProLen - 1; pIdx > 0; --pIdx) // masses of C-term ions
		{
			theoZIon.push_back(vProAAMass[pIdx] + theoZIon[nProLen - 1 - pIdx]);
			//theoZHIon.push_back(vProAAMass[pIdx] + theoZHIon[nProLen - 1 - pIdx]);
			m_theoCFragIon[nProLen - 1 - pIdx].push_back(theoZIon[nProLen - pIdx]);
			//m_theoCFragIon[nProLen - 1 - pIdx].push_back(theoZHIon[nProLen - pIdx]);
		}
	}
}



// Calculate the theory mass of fragment ions(only b or c ions)
// condition = 0(CID no mod),1(CID add mod), 2(ETD no mod), 3(ETD add mod), 4(UVPD no mod), 5(UVPD add mod)
// modSites <modID, modSite>
void CSearchEngine::_GetTheoryFragIons(const string &proSeq, int condition, vector<UINT_UINT> &modSites)
{
    //cout<<"[pTop] _GetTheoryFragIons..."<<endl;
    const int nProLen = (int)proSeq.length();
	// 获取每个氨基酸的质量 (加固定修饰后的质量)
	vector<double> nProAAMass;
	int pIdx = 0;
	double ntermMass = m_mapAAMass->GetAAMass(proSeq[pIdx]) + m_mapAAMass->GetNtermMass(proSeq[pIdx]); // +N端固定修饰
	nProAAMass.push_back(ntermMass);
	for(pIdx = 1; pIdx < nProLen; ++pIdx)
	{
	    try
	    {
            nProAAMass.push_back(m_mapAAMass->GetAAMass(proSeq[pIdx]));
	    } catch(bad_alloc &ba) {
            cout<<ba.what()<<endl;
        }

	}
	nProAAMass[nProLen-1] += m_mapAAMass->GetCtermMass(proSeq[nProLen-1]);  // +C term modification
	// Add modification
	if(condition & 1)
	{
		for(size_t i = 0; i < modSites.size(); ++i)
		{
			if(0 == modSites[i].nSecond) // N端修饰
			{ // N-term modification
				nProAAMass[0] += (m_getPTMForm->GetMassbyID(modSites[i].nFirst));
			} else if(nProLen == (int)modSites[i].nSecond - 1){ // C-term modification
				nProAAMass[nProLen - 1] += (m_getPTMForm->GetMassbyID(modSites[i].nFirst));
			} else {
				nProAAMass[modSites[i].nSecond - 1] += (m_getPTMForm->GetMassbyID(modSites[i].nFirst));
			}
		}
	}

	_GenerateIonsbyAAMassLst(nProAAMass);

	//m_theoNFragIon.clear();
	//m_theoCFragIon.clear();
	//vector<double> empty;
	//m_theoNFragIon.assign(nProLen - 1, empty);
	//m_theoCFragIon.assign(nProLen - 1, empty);

	//if(calAX)
	//{
	//	vector<double> theoAIon, theoXIon;
	//	theoAIon.push_back(-IonMass_Mono_O - IonMass_Mono_C + IonMass_Proton);  // a = [M] -C -O + Mp
	//	theoXIon.push_back(2 * IonMass_Mono_O + IonMass_Mono_C + IonMass_Proton); // x = [M] +2O +C +Mp
	//	for(pIdx = 0; pIdx < nProLen - 1; ++pIdx) // masses of N-term ions
	//	{
	//		theoAIon.push_back( nProAAMass[pIdx] + theoAIon[pIdx] );
	//		m_theoNFragIon[pIdx].push_back(theoAIon[pIdx+1]);
	//	}
	//	m_lfPrecurMass = theoAIon[nProLen-1] + nProAAMass[nProLen-1] + 2 * IonMass_Mono_O + 2 * IonMass_Mono_H + IonMass_Mono_C;

	//	for(pIdx = nProLen - 1; pIdx > 0; --pIdx) // masses of C-term ions
	//	{
	//		theoXIon.push_back( nProAAMass[pIdx] + theoXIon[nProLen - 1 - pIdx] );
	//		m_theoCFragIon[nProLen-1-pIdx].push_back(theoXIon[nProLen - pIdx]);
	//	}
	//}


	//if(calBY)
	//{
	//	vector<double> theoBIon, theoYIon;
	//	theoBIon.push_back( IonMass_Proton );  //CID [N] +, b = [M] + Mp
	//	theoYIon.push_back(IonMass_Mono_O + 2 * IonMass_Mono_H + IonMass_Proton); // y = [M] + O + 2H +Mp
	//	for(pIdx = 0; pIdx < nProLen - 1; ++pIdx) // masses of N-term ions
	//	{
	//		theoBIon.push_back( nProAAMass[pIdx] + theoBIon[pIdx]);
	//		m_theoNFragIon[pIdx].push_back(theoBIon[pIdx+1]);
	//	}
	//	m_lfPrecurMass = theoBIon[nProLen-1] + nProAAMass[nProLen-1] + IonMass_Mono_O + 2 * IonMass_Mono_H;

	//	for(pIdx = nProLen - 1; pIdx > 0; --pIdx) // masses of C-term ions
	//	{
	//		theoYIon.push_back( nProAAMass[pIdx] + theoYIon[nProLen - 1 - pIdx]);
	//		m_theoCFragIon[nProLen-1-pIdx].push_back(theoYIon[nProLen - pIdx]);
	//	}
	//}

	//if(calCZ)
	//{
	//	vector<double> theoCIon, theoZIon;
	//	theoCIon.push_back( 3 * IonMass_Mono_H + IonMass_Mono_N + IonMass_Proton );  //ETD [N] + NH2 +, c = [M] + 3H + N + Mp
	//	theoZIon.push_back(IonMass_Mono_O - IonMass_Mono_N + IonMass_Proton ); // z = [M] + O - N +Mp
	//	for(pIdx = 0; pIdx < nProLen - 1; ++pIdx) // masses of N-term ions
	//	{
	//		theoCIon.push_back( nProAAMass[pIdx] + theoCIon[pIdx] );
	//		m_theoNFragIon[pIdx].push_back(theoCIon[pIdx+1]);
	//	}
	//	m_lfPrecurMass = theoCIon[nProLen-1] + nProAAMass[nProLen-1] + IonMass_Mono_O - IonMass_Mono_N - IonMass_Mono_H;

	//	for(pIdx = nProLen - 1; pIdx > 0; --pIdx) // masses of C-term ions
	//	{
	//		theoZIon.push_back( nProAAMass[pIdx] + theoZIon[nProLen - 1 - pIdx] );
	//		m_theoCFragIon[nProLen-1-pIdx].push_back(theoZIon[nProLen - pIdx]);
	//	}
	//}
}
// 保存候选蛋白质变体
void CSearchEngine::_WriteToCandidate(const string &proSeq, const string &proAC, priority_queue<PATH_NODE> &qPaths, int proType)
{
    double modMass = 0.0;
    while(!qPaths.empty())
    {
        PATH_NODE topPath = qPaths.top();
        if(m_Spectra->vPrecursors[m_nPrecurID].nValidCandidateProNum < (int)TOPSCORE)
        {
			int idx = m_Spectra->vPrecursors[m_nPrecurID].nValidCandidateProNum;
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[idx].strProSQ = proSeq;
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[idx].strProAC = proAC;
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[idx].lfMass = m_lfPrecurMass;
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[idx].nMatchedPeakNum = topPath.nWeight;
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[idx].nIsDecoy = proType;
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[idx].vModSites = topPath.vModSites;

            for(size_t i = 0; i < topPath.vModSites.size(); ++i)
            {
                modMass = m_getPTMForm->GetMassbyID(topPath.vModSites[i].nFirst);
                m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[idx].lfMass += modMass;
            }
			// 按照匹配到的谱峰数目从小到大排序  [wrm?]这里有没有可能所有路径加起来也没有达到TOPSCORE条
            if(++(m_Spectra->vPrecursors[m_nPrecurID].nValidCandidateProNum) == (int)TOPSCORE)
            {
				sort(m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs.begin(), m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs.end(), PrSMPeaksCmp);
            }
        } else if(topPath.nWeight > m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[0].nMatchedPeakNum){
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[0].strProSQ = proSeq;
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[0].strProAC = proAC;
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[0].lfMass = m_lfPrecurMass;
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[0].nMatchedPeakNum = topPath.nWeight;
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[0].nIsDecoy = proType;
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[0].vModSites = topPath.vModSites;
            for(size_t i = 0; i < topPath.vModSites.size(); ++i)
            {
                modMass = m_getPTMForm->GetMassbyID(topPath.vModSites[i].nFirst);
                m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[0].lfMass += modMass;
            }

            size_t tPos = 0;
            while (1)  // HeapSort, 小顶堆之堆调整
            {
                size_t tPosLeftChild = (tPos << 1) + 1;
                if (tPosLeftChild < TOPSCORE
                    && m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[tPos].nMatchedPeakNum
                            > m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[tPosLeftChild].nMatchedPeakNum)
                { // nMatchedPeakNum of the tPos node is greater than its left child
                    if (tPosLeftChild + 1 < TOPSCORE
                                && m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[tPos].nMatchedPeakNum
                                        > m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[tPosLeftChild + 1].nMatchedPeakNum)
                    { // nMatchedPeakNum of the tPos node is greater than its right child
                        if (m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[tPosLeftChild].nMatchedPeakNum < m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[tPosLeftChild + 1].nMatchedPeakNum)
                        { // The nMatchedPeakNum of the left child is smaller
                            swap(m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[tPos], m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[tPosLeftChild]);
                            tPos = tPosLeftChild;
                        } else { // The nMatchedPeakNum of the right child is smaller
                            swap(m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[tPos], m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[tPosLeftChild + 1]);
                            tPos = tPosLeftChild + 1;
                        }

                    } else { // nMatchedPeakNum of the tPos node is no greater than its right child
                        swap(m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[tPos], m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[tPosLeftChild]);
                        tPos = tPosLeftChild;
                    }
                } else { // nMatchedPeakNum of the tPos node is no greater than its left child
                    if (tPosLeftChild + 1 < TOPSCORE
                            && m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[tPos].nMatchedPeakNum
                                    > m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[tPosLeftChild + 1].nMatchedPeakNum)
                    {
                            swap(m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[tPos], m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[tPosLeftChild + 1]);
                            tPos = tPosLeftChild + 1;
                    } else break;
                }
            } // end while
        } // end else if
		qPaths.pop();
    }
}

// The elments in "modForm" are the numbers of the i_th mod
double CSearchEngine::_GetModMass(vector<int> &modForm)
{
	double modMass = 0;
    // Get the mod mass
    for(size_t i = 0; i < modForm.size(); ++i)
    {
        modMass += modForm[i] * m_getPTMForm->GetMassbyID(i);
    }
	return modMass;
}

// The elments in "modSet" are the ids of the mod
double CSearchEngine::_GetModSetMass(vector<int> &modSet)
{
	double modMass = 0;
    // Get the mass of the mods' set
    for(size_t i = 0; i < modSet.size(); ++i)
    {
        modMass += m_getPTMForm->GetMassbyID(modSet[i]);
    }
	return modMass;
}
// ionMass为a/b/c 或 x/y/z
void CSearchEngine::_CheckMatched(vector<double> &ionMass, double modMass, int &matchedIon)
{
	int nIonPos, peakNum = m_Spectra->nPeaksNum;
	int minpeakMass = (int)m_Spectra->vPeaksTbl[0].lfMz;
	int maxpeakMass = (int)m_Spectra->vPeaksTbl[peakNum - 1].lfMz;

	matchedIon = 0;

	vector<double> smallWnd;
	smallWnd.push_back(0.0);
	smallWnd.push_back(-DIFF_13C);
	smallWnd.push_back(DIFF_13C);

	for(size_t j = 0; j < ionMass.size(); ++j)
	{
		for(size_t m = 0; m < smallWnd.size(); ++m) // -1与0两个小窗口是否会发生重叠？质量超过25000时会，所以一旦匹配上一个就break
		{
			double lfMass = ionMass[j] + modMass + smallWnd[m];
			int tmpMass = (int)lfMass;
			if(tmpMass > maxpeakMass || tmpMass < minpeakMass)
			{   // 理论碎片离子质量在所有实验谱峰的最小值和最大值之间才处理
				continue;
			}
			
			nIonPos = (int)m_hashTbl[tmpMass] - 1;
			if(nIonPos < 0)
			{
				continue;
			}
			double lowerBound = m_lfFragTol * lfMass / 1000000;
			double upperBound = lfMass + lowerBound;
			lowerBound = lfMass - lowerBound;
			
			while(nIonPos < peakNum && m_Spectra->vPeaksTbl[nIonPos].lfMz < lowerBound)
			{
				++nIonPos;
			}
			if(nIonPos < peakNum && m_Spectra->vPeaksTbl[nIonPos].lfMz <= upperBound)
			{
				++matchedIon;
				break;
			}
		}
	}
}

// type = 0: 预匹配，不考虑±1Da的情况
int CSearchEngine::_LookupHashTbl(int type, double shiftPPM, vector<double> &ionMass, vector<double> &matchedInts, vector<double> &massVol, double &sumMatchedIntens)
{
	int nIonPos, peakNum = m_Spectra->nPeaksNum;
	int minpeakMass = (int)m_Spectra->vPeaksTbl[0].lfMz;
	int maxpeakMass = (int)m_Spectra->vPeaksTbl[peakNum - 1].lfMz;

	vector<double> smallWnd;
	smallWnd.push_back(0.0);
	if(1 == type)
	{
		smallWnd.push_back(-DIFF_13C);
		smallWnd.push_back(DIFF_13C);
	}
	int cnt = 0;
	sumMatchedIntens = 0;
	for(size_t j = 0; j < ionMass.size(); ++j)
	{
		for(size_t m = 0; m < smallWnd.size(); ++m) // -1与0两个小窗口是否会发生重叠？质量超过25000时会
		{
			double lfMass = ionMass[j] + smallWnd[m];
			int tmpMass = (int)lfMass;
			if(tmpMass > maxpeakMass || tmpMass < minpeakMass)
			{   // 理论碎片离子质量在所有实验谱峰的最小值和最大值之间才处理
				continue;
			}
			
			nIonPos = (int)m_hashTbl[tmpMass] - 1;
			if(nIonPos < 0)
			{
				continue;
			}
			double lowerBound = lfMass + (shiftPPM - m_lfFragTol) * lfMass / 1000000;
			double upperBound = lfMass + (shiftPPM + m_lfFragTol) * lfMass / 1000000;
			
			while(nIonPos < peakNum && m_Spectra->vPeaksTbl[nIonPos].lfMz < lowerBound)
			{
				++nIonPos;
			}
			if(nIonPos < peakNum && m_Spectra->vPeaksTbl[nIonPos].lfMz <= upperBound)
			{
				//if (fp)
				//	fprintf(fp, "(T,P,W): %f,%f,%f  ", ionMass[j], m_Spectra->vPeaksTbl[nIonPos].lfMz, smallWnd[m]);
				++ cnt;
				if(0 == m_vMatchedPeak[nIonPos]) //该谱峰没有被其他的碎片离子匹配上
				{
					matchedInts.push_back( m_Spectra->vPeaksTbl[nIonPos].lfIntens );
					m_vMatchedPeak[nIonPos] = 1;
				} else {
					matchedInts.push_back( DOUCOM_EPS );
				}
				if(matchedInts.back() > baseline){
					sumMatchedIntens += matchedInts.back();
				}
				double tmpTol = m_lfFragTol;
				if(0 != m) // 对偏离±1Da的进行惩罚
				{
					if(m_Spectra->vPeaksTbl[nIonPos].lfMz > lfMass)
					{
						tmpTol = 5 - shiftPPM + 1000000 * (m_Spectra->vPeaksTbl[nIonPos].lfMz - lfMass) / lfMass;
					} else {
						tmpTol = -5 - shiftPPM + 1000000 * (m_Spectra->vPeaksTbl[nIonPos].lfMz - lfMass) / lfMass;
					}
				} else {
					 tmpTol = 1000000 * (m_Spectra->vPeaksTbl[nIonPos].lfMz - lfMass) / lfMass - shiftPPM;
				}
				massVol.push_back(tmpTol);
				break;
			}
		}
	}
	return cnt;
}

// update node
// param: vertices of the current layer, current vertice v, w(u,v), modId_modSite(v)
void CSearchEngine::_PushBackNode(vector<DP_VERTICE> &nextDist, DP_VERTICE &nextNode, int addWeight, UINT_UINT &addMod)
{
	DP_VERTICE addNode, tmpNode = nextNode;
	PATH_NODE topItem;
	addNode.sModForm = tmpNode.sModForm;
	if(addMod.nSecond > 0)
	{ // Have mod
		while(!tmpNode.qPath.empty())
		{
			topItem = tmpNode.qPath.top();
			topItem.nWeight += addWeight;
			topItem.vModSites.push_back(addMod);
			addNode.qPath.push(topItem);
			tmpNode.qPath.pop();
		}
	}else { // No mod
		while(!tmpNode.qPath.empty())
		{
			topItem = tmpNode.qPath.top();
			topItem.nWeight += addWeight;
			addNode.qPath.push(topItem);
			tmpNode.qPath.pop();
		}
	}

    int setPos;
    for(setPos = 0; setPos < (int)nextDist.size(); ++setPos) 
    {
        if(nextDist[setPos].sModForm == addNode.sModForm)
		{
            break;
		}
    }

    if(setPos < (int)nextDist.size())
    {   // The Mod form already exists, 合并节点
		while(!addNode.qPath.empty())
		{
			topItem = addNode.qPath.top();
			if((int)nextDist[setPos].qPath.size() >= TOP_PATH_NUM)
			{
				if(nextDist[setPos].qPath.top().nWeight < topItem.nWeight)
				{
					nextDist[setPos].qPath.pop();
					nextDist[setPos].qPath.push(topItem);
				}
			} else {
				nextDist[setPos].qPath.push(topItem);
			}
			addNode.qPath.pop();
		}
    } else if(setPos >= (int)nextDist.size()) {  // 没有修饰形式与当前节点相同
        try
        {
			nextDist.push_back(addNode);
        } catch(bad_alloc &ba) {
            cout<<"[Error] _PushBackNode(): "<<ba.what()<<endl;
        }
	} //else if(setPos >= nextDist.size())
}

// 获取有效候选蛋白，包含修饰位点
// Dynamic programming
// Get the best path at which the mod sites are more credible
void CSearchEngine::_GetValidCandiPro(vector<int> &modForm, const string &proSeq, int start, int end,
                                      priority_queue<PATH_NODE> &bestPaths, double preModMass, double totalModMass)
{
    //cout<<"_GetValidCandiPro..."<<endl;
    vector<int> emptySet;
	vector<UINT_UINT> modSites;
	PATH_NODE pathNode(0, modSites);
    priority_queue<PATH_NODE> emptyPath;
	emptyPath.push(pathNode);
    vector<DP_VERTICE> leftTmpDist[2];       // The mod sets in the last tier
    vector<DP_VERTICE> rightTmpDist[2];       // The mod sets in the last tier
	// 每个vertice中保存能够到达该节点的多条路径，每个节点有唯一的修饰组合状态
    emptySet.assign(m_getPTMForm->GetModNum(), 0);
    DP_VERTICE startNode(emptySet, emptyPath); // Start Node of mod set
    leftTmpDist[0].push_back(startNode);   // 添加起点
	rightTmpDist[0].push_back(startNode);
    // If mod form has N-term mod add another new Node
	for(int i = 0; i < (int)modForm.size(); ++i)
	{
		int nTermModID = modForm[i];
		if(m_getPTMForm->GetNCModNum() > 0)
		{
			if(m_getPTMForm->isNtermMod(nTermModID)) // N-term mod
			{
				modSites.clear();
			    UINT_UINT tmpModSite(nTermModID, 0);
			    modSites.push_back(tmpModSite);
				pathNode.vModSites = modSites;
			    priority_queue<PATH_NODE> initPath;
			    initPath.push(pathNode);
			    DP_VERTICE startNodeN(emptySet, initPath);
				startNodeN.sModForm[nTermModID] = 1;
				leftTmpDist[0].push_back(startNodeN);
			}else if(m_getPTMForm->isCtermMod(nTermModID)) { // C-term mod
				modSites.clear();
			    UINT_UINT tmpModSite(nTermModID, 0);  // [wrm?] C端修饰位点为什么是0？ 两端同时开始扫描
			    modSites.push_back(tmpModSite);
			    pathNode.vModSites = modSites;
			    priority_queue<PATH_NODE> initPath;
			    initPath.push(pathNode);
			    DP_VERTICE startNodeC(emptySet, initPath);
				startNodeC.sModForm[nTermModID] = 1;
				rightTmpDist[0].push_back(startNodeC);
			}
		}
	}

    int leftIdx, rightIdx, dIdx;
    int proLen = proSeq.length();
    int lastModID = 0;
	bool lastModSite = false;
    bool canDelNode = false, modSiteCanDelNode = false;
    int matchedCIons = 0, matchedZIons = 0;
	vector<double> ionMassN, ionMassC;
	double modMass, addedModMass;
	pair<MAP_IT, MAP_IT> mapRet;
	int addWeight;

	leftIdx = start;
	rightIdx = end;
	int lPre = 0, lNext = 1; //记录每次使用的是leftTmpDist中的哪一项
	int rPre = 0, rNext = 1; //记录每次使用的是rightTmpDist中的哪一项
	while(leftIdx < rightIdx)
    {
		size_t distSize;
		vector<int> canAddModID;
		leftTmpDist[lNext].clear(); // 记录当前layer的所有vertices
		ionMassN = m_theoNFragIon[leftIdx];  // a|b|c
		ionMassC = m_theoCFragIon[proLen - leftIdx - 2];   // x|y|z
		
        distSize = leftTmpDist[lPre].size();
		m_getPTMForm->GetAllIDsbyAA(proSeq[leftIdx], canAddModID);  // 获取该位点上可添加的修饰列表

		// 检查该位置是否为某种修饰可以发生的最后一个位置
		// 若是，则这种修饰在前面序列上必须全部发生
		lastModSite = false;
        if(m_mapProLastModSite.find(leftIdx) != m_mapProLastModSite.end())
		{
			lastModSite = true;
		}

        for(dIdx = 0; dIdx < (int)distSize; ++dIdx)  // 遍历该节点的每一个前驱节点
        {
            DP_VERTICE nextNode = leftTmpDist[lPre][dIdx];
            addWeight = 0;
			canDelNode = false;
			modSiteCanDelNode = false;
            if(lastModSite == true)  // 该位置为修饰 m1,m2,... 可以发生的最后一个位点
            {
				canDelNode = true;
				modSiteCanDelNode = true;
				pair <multimap<int,int>::iterator, multimap<int,int>::iterator> ret;
				ret = m_mapProLastModSite.equal_range(leftIdx);
				for(multimap<int,int>::iterator it = ret.first; it != ret.second; ++it) // [wrm?]若ret为空呢？但该位点可以不发生修饰
				{
					lastModID = it->second;  // mod ID of the one that last occurs at this site
					if((int)nextNode.sModForm[lastModID] == m_vModID2Num[lastModID] )
					{ // 已发生且只需发生m_vModID2Num[lastModID]个修饰lastModID
						canDelNode = false;
					}else if((int)nextNode.sModForm[lastModID] == m_vModID2Num[lastModID] - 1) { // 必须在该位点上发生修饰lastModID
						modSiteCanDelNode = false;
					}
				}
            }
            // Case 1: Site pIdx doesn't have PTM
            modMass = preModMass +_GetModMass(nextNode.sModForm);  // 已发生修饰的质量
            if( canDelNode == false ) // 该位点可以不发生修饰
            {
                // Chech if matching one more peak
                _CheckMatched(ionMassN, modMass, matchedCIons); // 获取N端匹配到的离子数目
				_CheckMatched(ionMassC, totalModMass - modMass, matchedZIons);  // 获取C端匹配到的离子数目
                if(matchedCIons > 0) 
				{
					++addWeight;
				}
                if(matchedZIons > 0) 
				{
					++addWeight;  // [wrm?] 这里有没有可能同1根峰即匹配到 b 离子又匹配到 y 离子
				}
                UINT_UINT addMod(0, 0);
                _PushBackNode(leftTmpDist[lNext], nextNode, addWeight, addMod);
            }

            // Case 2: Site pIdx could have PTM
            for(int modIdx = 0; modIdx < (int)canAddModID.size(); ++modIdx)  // 遍历该位点可以发生的修饰列表
            {
                addWeight = 0;
                int addModID = canAddModID[modIdx];  // [wrm?]有没有可能这个可以在该位点发生的修饰并不在此次的修饰组合中？
                if(m_vModID2Num[addModID] <= 0)
				{
                    continue; // The addModID is not in the mod form
				}
                if( nextNode.sModForm[addModID] >= m_vModID2Num[addModID] )
				{
                    continue; // The addModID already occured, and could not add to the set
				}

                if( canDelNode == false || (canDelNode == true && modSiteCanDelNode == false) )
               {
				    ++nextNode.sModForm[addModID];
                    addedModMass = modMass + m_getPTMForm->GetMassbyID(addModID);
                    _CheckMatched(ionMassN, addedModMass, matchedCIons);
					_CheckMatched(ionMassC, totalModMass - addedModMass, matchedZIons);
                    if(matchedCIons > 0) 
					{
						++addWeight;
					}
                    if(matchedZIons > 0) 
					{
						++addWeight;
					}
                    UINT_UINT addMod(addModID, leftIdx+1);
                    _PushBackNode(leftTmpDist[lNext], nextNode, addWeight, addMod);
					--nextNode.sModForm[addModID];
               }
            }
        }
        int tmp = lPre;
        lPre = lNext;
        lNext = tmp;

		if(++leftIdx >= rightIdx) 
		{
			break;
		}

		// From the Cterm to the Nterm
		size_t tmpDistSize;
		rightTmpDist[rNext].clear();

		ionMassN = m_theoNFragIon[rightIdx - 1];
		ionMassC = m_theoCFragIon[proLen - rightIdx - 1];
		
        tmpDistSize = rightTmpDist[rPre].size();

		vector<int> rightCanAddModID;
		m_getPTMForm->GetAllIDsbyAA(proSeq[rightIdx], rightCanAddModID);

        for(dIdx = 0; dIdx < (int)tmpDistSize; ++dIdx)  // 遍历rightIdx的前驱节点
        {
			// Case 1: Site pIdx doesn't have PTM
            DP_VERTICE preNode = rightTmpDist[rPre][dIdx];
            addWeight = 0;
            modMass = preModMass + _GetModMass(preNode.sModForm);
            // Check if matching one more peak
            _CheckMatched(ionMassN, totalModMass - modMass, matchedCIons);
			_CheckMatched(ionMassC, modMass, matchedZIons);
            if(matchedCIons > 0) 
			{
				++addWeight;
			}
            if(matchedZIons > 0) 
			{
				++addWeight;
			}
            // Put the node in the rightTmpDist[rNext] vector
            UINT_UINT addMod(0, 0);
            _PushBackNode(rightTmpDist[rNext], preNode, addWeight, addMod);

            // Case 2: Site pIdx could have PTM
            for(int modIdx = 0; modIdx < (int)rightCanAddModID.size(); ++modIdx)
            {
                addWeight = 0;
                int addModID = rightCanAddModID[modIdx];
                if(m_vModID2Num[addModID] <= 0)
				{
                    continue; // The addModID is not in the mod form
				}
                if((int)preNode.sModForm[addModID] >= m_vModID2Num[addModID])
				{
                    continue; // The addModID could not add to the set
				}

                ++preNode.sModForm[addModID];
                addedModMass = modMass + m_getPTMForm->GetMassbyID(addModID);
                _CheckMatched(ionMassN, totalModMass - addedModMass, matchedCIons);
				_CheckMatched(ionMassC, addedModMass, matchedZIons);
                if(matchedCIons > 0) 
				{
					++addWeight;
				}
                if(matchedZIons > 0) 
				{
					++addWeight;
				}
                UINT_UINT addMod(addModID, rightIdx+1);
                _PushBackNode(rightTmpDist[rNext], preNode, addWeight, addMod);
                --preNode.sModForm[addModID];
            }
        }
        tmp = rPre;
        rPre = rNext;
        rNext = tmp;
		--rightIdx;
    }
	// [wrm?] 假设说left和right在第z层相遇，left中存的是从s到z的所有路径，right中存的是从t到z的所有路径，两边的路径都包含了z，或许左边和右边都在z上加了修饰，此时直接合并会不会有问题？
    // Find the top K weighted path
    int modNum = (int)m_getPTMForm->GetModNum();
    for(dIdx = 0; dIdx < (int)leftTmpDist[lPre].size(); ++dIdx)
    {
		for(int rdIdx = 0; rdIdx < (int)rightTmpDist[rPre].size(); ++rdIdx)
		{
		    int mm;
		    for(mm = 0; mm < modNum; ++mm)
		    {
		        if( leftTmpDist[lPre][dIdx].sModForm[mm] + rightTmpDist[rPre][rdIdx].sModForm[mm] != m_vModID2Num[mm])
				{
					break;
				}
		    }
			if(mm < modNum)  // 修饰组合不满足条件
			{
				continue;
			}

			priority_queue<PATH_NODE> qLeft, qRight;
			PATH_NODE lTop, rTop;
            qLeft = leftTmpDist[lPre][dIdx].qPath;
            while(!qLeft.empty())
            {
                lTop = qLeft.top();
                qRight = rightTmpDist[rPre][rdIdx].qPath;
                while(!qRight.empty())
                {
                    rTop = qRight.top();
                    int tmpWeight = lTop.nWeight + rTop.nWeight;
                    if(tmpWeight <= 0)
                    {
                        qRight.pop();
                        continue;
                    }
                    if(bestPaths.size() < 10)
                    {
                        rTop.nWeight = tmpWeight;
                        for(size_t v = 0; v < lTop.vModSites.size(); ++v)
                        {
                            rTop.vModSites.push_back(lTop.vModSites[v]);
                        }
                        bestPaths.push(rTop);
                    } else if(tmpWeight > bestPaths.top().nWeight) {  // 队首为最小元素
                        rTop.nWeight = tmpWeight;
                        for(size_t v = 0; v < lTop.vModSites.size(); ++v)
                        {
                            rTop.vModSites.push_back(lTop.vModSites[v]);
                        }
                        bestPaths.pop();
                        bestPaths.push(rTop);
                    }
                    qRight.pop();
                } // end while qRight
                qLeft.pop();
            } // end while qLeft
		} // end for rdIdx
    } // end for dIdx
}

// Preprocess the fragment ions, such as only keep the top 1000
void CSearchEngine::PreProcess()
{
    //按20ppm合并谱峰,合并时质荷比按照强度加权，强度则求和
    if(m_Spectra->nPeaksNum < 2)
    {
        return;
    }
    double preMZ = m_Spectra->vPeaksTbl[0].lfMz;
    double sumMZ = preMZ * m_Spectra->vPeaksTbl[0].lfIntens;
    double sumInten = m_Spectra->vPeaksTbl[0].lfIntens;
    int peakIdx = 0;
    for(int tIdx = 1; tIdx < m_Spectra->nPeaksNum; ++tIdx)
    {
        if( 1000000 * (m_Spectra->vPeaksTbl[tIdx].lfMz-preMZ)/preMZ <= m_lfFragTol)
        {
            sumInten += m_Spectra->vPeaksTbl[tIdx].lfIntens;
            sumMZ += m_Spectra->vPeaksTbl[tIdx].lfMz * m_Spectra->vPeaksTbl[tIdx].lfIntens;
            continue;
        } else {
            m_Spectra->vPeaksTbl[peakIdx].lfMz = sumMZ / sumInten;
            m_Spectra->vPeaksTbl[peakIdx].lfIntens = sumInten;
            ++peakIdx;

            preMZ = m_Spectra->vPeaksTbl[tIdx].lfMz;
            sumMZ = preMZ * m_Spectra->vPeaksTbl[tIdx].lfIntens;
            sumInten = m_Spectra->vPeaksTbl[tIdx].lfIntens;
        }
    }
    m_Spectra->nPeaksNum = peakIdx;
}

// Spectral convolution
// Check whether the candidate protein could be filered
double CSearchEngine::_FilterCandiProtein(const string &proSeq, double precurMass)
{
    //cout<<"Filter..."<<endl;
    int maxSize = 20000;
    vector<DOUBLE_DOUBLE> deltaMass;
    deltaMass.assign(maxSize, DOUBLE_DOUBLE());
    int proLen = proSeq.length();
    int idx = 0, tIdx, eIdx, wStart = 0;
    double tmpDeltaN, tmpDeltaC;
	/*double maxDeltaMass = precurMass - m_lfPrecurMass + 1;
	if('M' == proSeq[0])
	{
		maxDeltaMass += AAMass_M_mono;
	}*/
	double maxDeltaMass = WIND / 2;
    for(tIdx = 0; tIdx < proLen - 1; ++tIdx)
    {
		for(int s = 0; s < (int)m_theoNFragIon[tIdx].size(); ++s)
		{
			double ionMassN = m_theoNFragIon[tIdx][s];
			double ionMassC = m_theoCFragIon[tIdx][s];
			for(eIdx = wStart; eIdx < m_Spectra->nPeaksNum; ++eIdx)
			{
				if(m_Spectra->vPeaksTbl[eIdx].lfMz - ionMassN < -0.5 && m_Spectra->vPeaksTbl[eIdx].lfMz - ionMassC < -0.5)
				{
					wStart = eIdx; //匹配下一个离子不用从实验谱的0开始，而是从这里开始
					continue;
				} else {
					if(m_Spectra->vPeaksTbl[eIdx].lfMz - ionMassN >= -0.5)
					{
						tmpDeltaN = m_Spectra->vPeaksTbl[eIdx].lfMz - ionMassN;
						if(tmpDeltaN < maxDeltaMass)
						{
							deltaMass[idx].lfa = tmpDeltaN;
							deltaMass[idx].lfb = ionMassN;
							if(++idx >= maxSize)
							{
								maxSize = maxSize << 1;
								deltaMass.resize(maxSize, DOUBLE_DOUBLE());
							}
						}
					} else {
						tmpDeltaN = 0;
					}
					if(m_Spectra->vPeaksTbl[eIdx].lfMz - ionMassC >= -0.5){
						tmpDeltaC = m_Spectra->vPeaksTbl[eIdx].lfMz - ionMassC;
						if(tmpDeltaC < maxDeltaMass)
						{
							deltaMass[idx].lfa = tmpDeltaC;
							deltaMass[idx].lfb = ionMassC;

							if(++idx >= maxSize)
							{
								maxSize = maxSize << 1;
								deltaMass.resize(maxSize, DOUBLE_DOUBLE());
							}
						}
					} else {
						tmpDeltaC = 0;
					}
					if(tmpDeltaN > maxDeltaMass && tmpDeltaC > maxDeltaMass)
					{
						break;
					}
				}
			}
		}
    }
    // P S^R
    wStart = m_Spectra->nPeaksNum - 1;
	for(tIdx = 0; tIdx < proLen - 1; ++tIdx)
    {
		for(int s = 0; s < (int)m_theoNFragIon[tIdx].size(); ++s)
		{
			double ionMassN = m_theoNFragIon[tIdx][s];
			double ionMassC = m_theoCFragIon[tIdx][s];
			for(eIdx = wStart; eIdx > 0; --eIdx)
			{
				double reverseIon = precurMass - m_Spectra->vPeaksTbl[eIdx].lfMz;
				if(reverseIon - ionMassN < -0.5 && reverseIon - ionMassC < -0.5)
				{
					wStart = eIdx; //匹配下一个离子不用从实验谱的0开始，而是从这里开始
					continue;
				} else {
					if(reverseIon - ionMassN >= -0.5)
					{
						tmpDeltaN = reverseIon - ionMassN;
						if(tmpDeltaN < maxDeltaMass)
						{
							deltaMass[idx].lfa = tmpDeltaN;
							deltaMass[idx].lfb = ionMassN;
                     
							if(++idx >= maxSize)
							{
								maxSize = maxSize << 1;
								deltaMass.resize(maxSize, DOUBLE_DOUBLE());
							}
						}
					} else {
						tmpDeltaN = 0;
					}
					if(reverseIon - ionMassC >= -0.5){
						tmpDeltaC = reverseIon - ionMassC;
						if(tmpDeltaC < maxDeltaMass)
						{
							deltaMass[idx].lfa = tmpDeltaC;
							deltaMass[idx].lfb = ionMassC;
                        
							if(++idx >= maxSize)
							{
								maxSize = maxSize << 1;
								deltaMass.resize(maxSize, DOUBLE_DOUBLE());
							}
						}
					} else {
						tmpDeltaC = 0;
					}
					if(tmpDeltaN > maxDeltaMass && tmpDeltaC > maxDeltaMass)
					{
						break;
					}
				}
			}
		}
    }

    sort(deltaMass.begin(), deltaMass.begin()+idx, DDCmp);

    vector<DOUBLE_INT> deltaMassCnt;
    maxSize = 1000;
    deltaMassCnt.assign(maxSize, DOUBLE_INT());
    int cIdx = 0, flag = 0;
    double preDelta = deltaMass[0].lfa;
    deltaMassCnt[cIdx].lfa = preDelta;
    deltaMassCnt[cIdx].nb = 1;
    for(tIdx = 1; tIdx < idx; ++tIdx)
    {
        if( 1000000 * (deltaMass[tIdx].lfa-preDelta)/deltaMass[tIdx].lfb <= 20)
        {
            preDelta = deltaMass[tIdx].lfa;
            ++deltaMassCnt[cIdx].nb;
            flag = 1;
        } else {
            if(1 == flag)
            {
                if(++cIdx >= maxSize)
                {
                    maxSize = maxSize << 1;
                    deltaMassCnt.resize(maxSize, DOUBLE_INT());
                }
            }
            flag = 0;  // 只保留数目大于1的质量差
            preDelta = deltaMass[tIdx].lfa;
            deltaMassCnt[cIdx].lfa = preDelta;
            deltaMassCnt[cIdx].nb = 1;
        }
    }
    sort(deltaMassCnt.begin(), deltaMassCnt.end(), DIntCmp);
    int matchedPeaks = 0;
    for(tIdx = 0; tIdx < 5; ++tIdx)
	{
        matchedPeaks += deltaMassCnt[tIdx].nb;
	}
	
	//cout << proSeq.substr(0, 10) << " " << idx << " " << matchedPeaks << endl;
//    vector<int> deltaMassCnt;
//    deltaMassCnt.assign(1000, 0);
//    for(tIdx = 0; tIdx < idx; ++tIdx)
//    {
//         ++deltaMassCnt[(int)(deltaMass[tIdx].lfa+0.5)];
//    }
//    sort(deltaMassCnt.begin(), deltaMassCnt.end());
//    int matchedPeaks = 0;
//    for(tIdx = 999; tIdx > 991; --tIdx)
//        matchedPeaks += deltaMassCnt[tIdx];

	int avgLen = 2 * int(precurMass / 111.1);
    return (1.0 * matchedPeaks / avgLen);
	//return 1.0 * matchedPeaks / idx;
}

// Combine the mod sites of different part in the protein
// flag = 1:some mods happened on the tag, 0:others
void CSearchEngine::_CombModPaths(priority_queue<PATH_NODE> &bestPaths, priority_queue<PATH_NODE> &tmpPaths,
                                  int flag, PATH_NODE modOnTag)
{
    priority_queue<PATH_NODE> qBest, qTemp;
    PATH_NODE lTop, rTop;
    if(flag == 1)
    { // Add the mods on the tag to the best paths
        while(!bestPaths.empty())
        {
            lTop = bestPaths.top();
            lTop.nWeight += modOnTag.nWeight;
            for(size_t v = 0; v < modOnTag.vModSites.size(); ++v)
            {
                lTop.vModSites.push_back(modOnTag.vModSites[v]);
            }
            qBest.push(lTop);
            bestPaths.pop();
        }
        bestPaths = qBest;
    }
    if(bestPaths.empty())
    {
        bestPaths = tmpPaths;
        return;
    }
    if(tmpPaths.empty())
    {
        return;
    }

    while(!bestPaths.empty())
    {
        lTop = bestPaths.top();
        bestPaths.pop();
        qTemp = tmpPaths;
        while(!qTemp.empty())
        {
            rTop = qTemp.top();
            int tmpWeight = lTop.nWeight + rTop.nWeight;
            if(tmpWeight <= 0)
            {
                qTemp.pop();
                continue;
            }
            if(qBest.size() < 10)
            {
                rTop.nWeight = tmpWeight;
                for(size_t v = 0; v < lTop.vModSites.size(); ++v)
                {
                    rTop.vModSites.push_back(lTop.vModSites[v]);
                }
                qBest.push(rTop);
            } else if(tmpWeight > qBest.top().nWeight) {
                rTop.nWeight = tmpWeight;
                for(size_t v = 0; v < lTop.vModSites.size(); ++v)
                {
                    rTop.vModSites.push_back(lTop.vModSites[v]);
                }
                qBest.pop();
                qBest.push(rTop);
            }
            qTemp.pop();
        }
    }
    bestPaths = qBest;
}

void CSearchEngine::_CheckBestPath(double totalModMass, priority_queue<PATH_NODE> &qPaths)
{
	priority_queue<PATH_NODE> bestPath;
	double modMass = 0.0;
	while(!qPaths.empty())
    {
        PATH_NODE topPath = qPaths.top();
		for(int i = 0; i < (int)topPath.vModSites.size(); ++i)
		{
			modMass += m_getPTMForm->GetMassbyID(topPath.vModSites[i].nFirst);
		}
		if(fabs(modMass - totalModMass) <= 2.0 && topPath.nWeight > 0)
		{
			bestPath.push(topPath);
		}
		qPaths.pop();
    }
	qPaths = bestPath;
}

//void CSearchEngine::SearchTAGCandidate(const vector<PROTEIN_STRUCT> &proList, int proID, int actStatus, bool hasM)
//{
//	double totalModMass;
//	string proSeq;
//	if(hasM)
//	{
//		totalModMass = m_Spectra->lfPrecursorMass - proList[proID].lfMass;
//		proSeq = proList[proID].strProSQ;
//	} else {
//		totalModMass = m_Spectra->lfPrecursorMass - proList[proID].lfMass + AAMass_M_mono;
//		proSeq = proList[proID].strProSQ;
//		proSeq.erase(0, 1);
//	}
//	//cout<<totalModMass<<" "<<proList[proID].lfMass<<" "<<proSeq.c_str()<<endl;
//    int deltaMass = int( totalModMass + 0.5);
//	int isDecoy = proList[proID].nIsDecoy;
//	vector<UINT_UINT> emptySite;
//
//    _GetTheoryFragIons(proSeq, actStatus, emptySite);
//
//    // Create a vector to record the sites could be moded in the sequence of the protein
//    m_vProSeqModNum.clear();
//    vector<int> vProLastModSite;
//    size_t modID;
//    for(modID = 0; modID < m_getPTMForm->m_ModMass.size(); ++modID)
//    {
//        m_vProSeqModNum.push_back(0);
//        vProLastModSite.push_back(0);
//    }
//    for(size_t sPos = 0; sPos < proSeq.length(); ++sPos)
//    {
//        pair<MAP_IT, MAP_IT> mapRet = m_getPTMForm->m_MapModAA2ID.equal_range(proSeq[sPos]);
//        for(MAP_IT it = mapRet.first; it != mapRet.second; ++it)
//        {
//            ++m_vProSeqModNum[it->second];
//            vProLastModSite[it->second] = sPos;
//        }
//    }
//
//    m_mapProLastModSite.clear();
//    for(modID = 0; modID < m_getPTMForm->m_ModMass.size(); ++modID)
//    {
//        m_mapProLastModSite.insert(pair<int,int>(vProLastModSite[modID], modID));
//    }
//   
//	// Consider all the mod forms satisfied the delta mass (不利用Tag对蛋白质进行分治)
//	int minDeltaMass = deltaMass - 1 > 0 ? deltaMass - 1 : 0;
//    int maxDeltaMass = deltaMass + 1 < MAX_SHIFT_MASS-1 ? deltaMass + 1 : MAX_SHIFT_MASS-1;
//    for(int dIdx = minDeltaMass; dIdx <= maxDeltaMass; ++dIdx) // +/-1.5 Da
//    {
//        int ptmFormNum = m_getPTMForm->m_ModForms[dIdx].size();
//        for(int mIdx = 0; mIdx < ptmFormNum; ++mIdx)
//        {
//            if( _CheckModForm(dIdx, mIdx, proSeq, 0, (int)proSeq.length()) )
//            {
//				for(int tt = 0; tt < m_getPTMForm->m_ModForms[dIdx][mIdx].size(); ++tt)
//				{
//					cout<<m_getPTMForm->m_ModForms[dIdx][mIdx][tt]<<" ";
//				}
//				cout<<endl;
//                priority_queue<PATH_NODE> bestPaths;
//                totalModMass = _GetModSetMass(m_getPTMForm->m_ModForms[dIdx][mIdx]);
//                _GetValidCandiPro(m_getPTMForm->m_ModForms[dIdx][mIdx], proSeq, 0, (int)proSeq.length()-1, bestPaths, 0.0, totalModMass);
//				_WriteToCandidate(proSeq, proList[proID].strProAC, bestPaths, isDecoy);
//            } // end if
//        } // end for mIdx
//    } // end for dIdx
//		//------------------------------------------------------------------
//		//
//		//vector<int> emptyIDs;
//		//priority_queue<PATH_NODE> bestPaths;
//  //      int startPos = 0, endPos = 0;
//  //      double preDeltaMass = 0.0;
//  //      sort(caidiProID[i].vTagInfo.begin(), caidiProID[i].vTagInfo.end());
//  //      TAG_INFO tmpTagInfo(proSeq.length(), m_Spectra->lfPrecursorMass - proList[proID].lfMass, emptyIDs);
//  //      caidiProID[i].vTagInfo.push_back(tmpTagInfo);
//  //      for(int dividNum = 0; dividNum < (int)caidiProID[i].vTagInfo.size(); ++dividNum)
//  //      {
//  //          endPos = caidiProID[i].vTagInfo[dividNum].nPos;
//		//	//cout<<startPos<<" "<<endPos<<endl;
//  //          int partDeltaMass = int(caidiProID[i].vTagInfo[dividNum].lfDeltaMass - preDeltaMass + 0.5);
//  //          priority_queue<PATH_NODE> tmpPaths;
//  //          if(endPos > startPos)
//  //          {
//  //              int minDeltaMass = partDeltaMass - 1 > 0 ? partDeltaMass - 1 : 0;
//  //              int maxDeltaMass = partDeltaMass + 1 < MAX_SHIFT_MASS-1 ? partDeltaMass + 1 : MAX_SHIFT_MASS-1;
//  //              for(int dIdx = minDeltaMass; dIdx <= maxDeltaMass; ++dIdx) // +/-1.5 Da
//  //              {
//  //                  int ptmFormNum = m_getPTMForm->m_ModForms[dIdx].size();
//  //                  //cout<<"[pTop] PTM form number: "<<ptmFormNum<<endl;
//  //                  for(int mIdx = 0; mIdx < ptmFormNum; ++mIdx)
//  //                  {
//  //                      if( _CheckModForm(dIdx, mIdx, proSeq, startPos, endPos) )
//  //                      {
//  //                          _GetValidCandiPro(m_getPTMForm->m_ModForms[dIdx][mIdx], proSeq, startPos, endPos-1, tmpPaths, preDeltaMass, totalModMass);
//  //                      } // end if
//  //                  } // end for mIdx
//  //              } // end for dIdx
//		//		preDeltaMass += partDeltaMass;
//  //          } // end if
//  //          PATH_NODE modOnTag;
//  //          double modMassOnTag = 0.0;
//  //          int flag = 0;
//  //          for(size_t t = 0; t < caidiProID[i].vTagInfo[dividNum].vmodID.size(); ++t)
//  //          {
//  //              int modOnTagID = caidiProID[i].vTagInfo[dividNum].vmodID[t];
//  //              if(modOnTagID != -1 && endPos + (int)t >= startPos)
//  //              {
//  //                  flag = 1;
//  //                  UINT_UINT modSiteOnTag(modOnTagID, endPos+t+1);
//  //                  modOnTag.vModSites.push_back(modSiteOnTag);
//  //                  modMassOnTag += m_getPTMForm->m_ModMonoMass[modOnTagID];
//  //              }
//  //          }
//  //          if(flag && dividNum+1 < (int)caidiProID[i].vTagInfo.size())
//  //          {
//  //              int distPos = caidiProID[i].vTagInfo[dividNum+1].nPos - endPos;
//  //              modOnTag.nWeight = distPos >= TAG_LEN ? TAG_LEN+1 : distPos;
//  //          } else if(flag) {
//  //              modOnTag.nWeight = 0;
//  //          }
//  //          _CombModPaths(bestPaths, tmpPaths, flag, modOnTag);
//  //          startPos = endPos + TAG_LEN;
//  //          //preDeltaMass = caidiProID[i].vTagInfo[dividNum].lfDeltaMass + modMassOnTag;
//		//	//if(dividNum > 0)
//		//	//	preDeltaMass += caidiProID[i].vTagInfo[dividNum].lfDeltaMass -caidiProID[i].vTagInfo[dividNum-1].lfDeltaMass;
//		//	preDeltaMass += modMassOnTag;
//  //      } // end for dividNum
//		//_CheckBestPath(totalModMass, bestPaths);
//		//_WriteToCandidate(proSeq, proList[proID].strProAC, bestPaths, isDecoy);
//		//
//		//-----------------------------------------------------------------
//}

void CSearchEngine::MatchCandidate(const string &proSQ, const string &proAC, int isDecoy, int actStatus, int deltaMass)
{
	vector<UINT_UINT> emptySite;
	_GetTheoryFragIons(proSQ, actStatus, emptySite);  // 获取理论碎片离子

	// Create a vector to record the sites could be moded in the sequence of the protein
	m_vProSeqModNum.clear();
	vector<int> vProLastModSite;
	int modID;
	for(modID = 0; modID < m_getPTMForm->GetModNum(); ++ modID)
	{
		m_vProSeqModNum.push_back(0); // 保存该蛋白序列上每种修饰可发生的数目
		vProLastModSite.push_back(0); // 保存该蛋白序列上每种修饰可发生的最后一个位点
	}
	for(size_t sPos = 0; sPos < proSQ.length(); ++ sPos)
	{
		vector<int> vModIDs;
		m_getPTMForm->GetAllIDsbyAA(proSQ[sPos], vModIDs);

		for(size_t i = 0; i < vModIDs.size(); ++ i)
		{
			++m_vProSeqModNum[vModIDs[i]];
			vProLastModSite[vModIDs[i]] = sPos;
		}
	}
	m_mapProLastModSite.clear();
	for(modID = 0; modID < m_getPTMForm->GetModNum(); ++modID)
	{
		m_mapProLastModSite.insert(pair<int,int>(vProLastModSite[modID], modID));
	}
	vector<int> emptyIDs;
	// Consider all the mod forms satisfied the delta mass
	//cout<<"Precursor tol: "<<m_lfPrecurTol<<endl;
	int minDeltaMass = deltaMass - m_nPrecurTol > (1 - MAX_SHIFT_MASS) ? deltaMass - m_nPrecurTol : (1 - MAX_SHIFT_MASS);
    int maxDeltaMass = deltaMass + m_nPrecurTol < MAX_SHIFT_MASS - 1 ? deltaMass + m_nPrecurTol : MAX_SHIFT_MASS - 1;
	//if(maxDeltaMass < 0) maxDeltaMass = 0;
    for(int dIdx = minDeltaMass; dIdx <= maxDeltaMass; ++dIdx) // +/- PRECUR_DELTA Da
    {
		int massIdx = dIdx+MAX_SHIFT_MASS;
		int ptmFormNum = m_getPTMForm->m_ModForms[massIdx].size();
		//if(0 < ptmFormNum)
		//	cout << dIdx << " " << ptmFormNum << endl;
        for(int mIdx = 0; mIdx < ptmFormNum; ++mIdx)
        {
			if( _CheckModForm(massIdx, mIdx, proSQ, 0, (int)proSQ.length()) )
            {
                    priority_queue<PATH_NODE> bestPaths;
					double totalModMass = _GetModSetMass(m_getPTMForm->m_ModForms[massIdx][mIdx]);
					_GetValidCandiPro(m_getPTMForm->m_ModForms[massIdx][mIdx], proSQ, 0, (int)proSQ.length()-1, bestPaths, 0.0, totalModMass);
					//cout<<bestPaths.size()<<endl;
					_WriteToCandidate(proSQ, proAC, bestPaths, isDecoy);
					//cout<<m_Spectra->nValidCandidateProNum<<endl;
            } // end if
        } // end for mIdx
    } // end for dIdx
}

void CSearchEngine::GetMoreCandidate(const vector<PROTEIN_STRUCT> &proList, int actStatus)
{
	/*double minPremass = m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass;
	double maxPremass = minPremass;

	for(int p = 1; p < m_Spectra->nPrecurNum; ++p)
	{
		if(m_Spectra->vPrecursors[p].lfPrecursorMass < minPremass)
		{
			minPremass = m_Spectra->vPrecursors[p].lfPrecursorMass;
		}
		if(m_Spectra->vPrecursors[p].lfPrecursorMass > maxPremass)
		{
			maxPremass = m_Spectra->vPrecursors[p].lfPrecursorMass;
		}
	}*/

	double tmpPrecurMass = m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass;
	double minCandiMass = tmpPrecurMass - WIND / 2; //modified at 2014.11.27
    double maxCandiMass = tmpPrecurMass + WIND / 2;
	// [wrm]: 蛋白质信息已按照质量排序，故可进行二分； [优化]：哈希
    int left = _BinSearch(proList, minCandiMass);
    int right = _BinSearch(proList, maxCandiMass);
	int candiProCnt = 0, pId = 0;
	vector<DOUBLE_INT> vCandiProFiltScore;
	vector<UINT_UINT> emptySite;
	vector<PROID_NODE> caidiProID;
	//cout << right - left << endl;
    for(pId = left; pId < right; ++pId)
    {
		/*if(pId == 29699)
		{
			cout << "test high score" << endl;
		}
		if(pId == 30908)
		{
			cout << "test h4" << endl;
		}*/
        _GetTheoryFragIons(proList[pId].strProSQ, actStatus, emptySite);
        double filterScore = _FilterCandiProtein(proList[pId].strProSQ, tmpPrecurMass);
		//cout << filterScore << " " << pId << " " << proList[pId].strProSQ.substr(0, 10) << endl;

        DOUBLE_INT tmpcandiPro(filterScore, pId);
        vCandiProFiltScore.push_back(tmpcandiPro);
    }
    sort(vCandiProFiltScore.begin(), vCandiProFiltScore.end());
    
	
    for(pId = (int)vCandiProFiltScore.size() - 1; pId >= 0; --pId)
    {
		PROID_NODE tmp;
		tmp.nProID = vCandiProFiltScore[pId].nb;
        caidiProID.push_back(tmp);
        if(++candiProCnt >= MAX_CANDIPRO_NUM)	
		{
			double lowestScore = vCandiProFiltScore[pId].lfa;
			//cout << "lowestScore = " << lowestScore << " " << vCandiProFiltScore[vCandiProFiltScore.size() - 1].lfa << endl;
			int idx = pId - 1;
			// 与上一个候选蛋白打分相同，则加入候选
			while(idx >= 0 && fabs(vCandiProFiltScore[idx].lfa - lowestScore) < DOUCOM_EPS)  
			{
				PROID_NODE tmp;
				tmp.nProID = vCandiProFiltScore[idx].nb;
				caidiProID.push_back(tmp);
				--idx;
				if(++candiProCnt >= (MAX_CANDIPRO_NUM << 1))   //  >>
				{
					break;
				}
			}
			break;
		}
    }
	//cout << cnt << endl;
	m_Spectra->nCandidateProNum = candiProCnt;
	for(int i = 0; i < candiProCnt; ++i)
	{
		size_t pIdx = caidiProID[i].nProID;
		//for(m_nPrecurID = 0; m_nPrecurID < m_Spectra->nPrecurNum; ++m_nPrecurID)
		//{
			double totalModMass = m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass - proList[pIdx].lfMass;
			int deltaMass = int( totalModMass + 0.5);

			MatchCandidate(proList[pIdx].strProSQ, proList[pIdx].strProAC, proList[pIdx].nIsDecoy, actStatus, deltaMass);
			if(proList[pIdx].strProSQ[0] == 'M')  // 去掉N端M，再次匹配打分
			{
				string proSeq = proList[pIdx].strProSQ.substr(1,proList[pIdx].strProSQ.length() - 1);
				deltaMass = (int)(totalModMass + AAMass_M_mono + 0.5);
				MatchCandidate(proSeq, proList[pIdx].strProAC, proList[pIdx].nIsDecoy, actStatus, deltaMass);
			}
		//} // end for p
	}// end for i
}

// Get the PTM forms that the protein could have by delta_mass
// Using Dynamic Programming to get several valid candidate proteoforms
void CSearchEngine::SketchScore(const vector<PROTEIN_STRUCT> &proList, vector<PROID_NODE> &caidiProID)
{
    //cout<<"SketchScore..."<<endl;

    int actStatus = 0;
	
	//cout<<"[Debug] Candidate number: "<<m_Spectra.nCandidateProNum<<endl;
	m_Spectra->nCandidateProNum = (int)caidiProID.size();
	
	// The score of candidate proteins is increased
	for(int i = (int)caidiProID.size() - 1; i >= 0 ; --i)
	{
		size_t pIdx = caidiProID[i].nProID;

		string proSQ = proList[pIdx].strProSQ;
		//cout << proSQ << endl;
		double totalModMass = m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass - proList[pIdx].lfMass;
		int deltaMass = (int)(totalModMass + 0.5); // 谱图母离子质量 - 无修饰蛋白质量 = 修饰质量
		MatchCandidate(proSQ, proList[pIdx].strProAC, proList[pIdx].nIsDecoy, actStatus, deltaMass);
		if(proList[pIdx].strProSQ[0] == 'M')
		{
			proSQ.erase(proSQ.begin());
			deltaMass = (int)(totalModMass + 0.5 + AAMass_M_mono);
			MatchCandidate(proSQ, proList[pIdx].strProAC, proList[pIdx].nIsDecoy, actStatus, deltaMass);
		}
	}

	// When there is no tag, use filter
	// [wrm]Key: 当谱图提取不到Tag时，使用母离子质量获取候选蛋白
	if(0 == (int)caidiProID.size())
	{
		GetMoreCandidate(proList, actStatus);
	}
}

double CSearchEngine::_BM25Score(int type, double shiftPPM)
{
	const double BM25_K1 = 0.03;
	double score = 0.0;

	m_vMatchedPeak.clear();
	m_vMatchedPeak.assign(m_Spectra->vPeaksTbl.size(), 0);

	vector<double> sumIntens, massVol;
	size_t len = m_theoNFragIon.size();
	double nmatched_intensity = 0.0;
	double cmatched_intensity = 0.0;
    
    for(size_t i = 0; i < len; ++i)
    {
		sumIntens.clear();
		massVol.clear();
		int nterm = _LookupHashTbl(type, shiftPPM, m_theoNFragIon[i], sumIntens, massVol, nmatched_intensity);
		int cterm = _LookupHashTbl(type, shiftPPM, m_theoCFragIon[len - i - 1], sumIntens, massVol, cmatched_intensity);
		for(size_t j = 0; j < sumIntens.size(); ++j)
		{
			double TF = (BM25_K1 + 1) * (sumIntens[j]) / (BM25_K1 + sumIntens[j]);
			double tmpVol = 0.5 + cos( fabs(massVol[j]) / m_lfFragTol * pi / 2);
			score += TF * tmpVol;
		}
    }

    return score;
}

double CSearchEngine::_BM25Score(int type, double shiftPPM, PROTEIN_SPECTRA_MATCH &prsm)
{
	//@test
	//string tmpOut = m_Spectra->strSpecTitle + string(1, '0' + m_nPrecurID) + ".txt";
	//FILE* fp = fopen(tmpOut.c_str(), "w");

	const double BM25_K1 = 0.03;
	double score = 0.0;

	m_vMatchedPeak.clear();
	m_vMatchedPeak.assign(m_Spectra->vPeaksTbl.size(), 0);

	vector<double> sumIntens, massVol;
	size_t len = m_theoNFragIon.size();
	double nmatched_intensity = 0.0, cmatched_intensity = 0.0, tmpSumIntens = 0.0;
	int nterm_matched_ions = 0;
	int cterm_matched_ions = 0;
    
    for(size_t i = 0; i < len; ++i)
    {
		sumIntens.clear();
		massVol.clear();
		//fprintf(fp, "\nn-term %d:", i+1);
		int nterm = _LookupHashTbl(type, shiftPPM, m_theoNFragIon[i], sumIntens, massVol, tmpSumIntens);
		nterm_matched_ions += nterm;
		nmatched_intensity += tmpSumIntens;
		//fprintf(fp, "\nc-term %d:", len - i);
		int cterm = _LookupHashTbl(type, shiftPPM, m_theoCFragIon[len - i - 1], sumIntens, massVol, tmpSumIntens);
		cterm_matched_ions += cterm;
		cmatched_intensity += tmpSumIntens;
		for(size_t j = 0; j < sumIntens.size(); ++j)
		{
			double TF = (BM25_K1 + 1) * (sumIntens[j]) / (BM25_K1 + sumIntens[j]);
			double tmpVol = 0.5 + cos( fabs(massVol[j]) / m_lfFragTol * pi / 2);
			score += TF * tmpVol;
		}
    }
	prsm.ntermIons = nterm_matched_ions;
	prsm.ctermIons = cterm_matched_ions;
	prsm.ntermMatchedIntensityRatio = nmatched_intensity/totalIntensity;
	prsm.ctermMatchedIntensityRatio = cmatched_intensity/totalIntensity;

	//@test
	//fclose(fp);
    return score;
}
double CSearchEngine::_GetBaselineByThreshold(double cutoff)
{
	if(m_Spectra->vPeaksTbl.empty())  return 0;
	double maxIntensity = m_Spectra->vPeaksTbl[0].lfIntens;
	for(int i=1; i< m_Spectra->vPeaksTbl.size(); ++i){
		if(m_Spectra->vPeaksTbl[i].lfIntens > maxIntensity){
			maxIntensity = m_Spectra->vPeaksTbl[i].lfIntens;
		}
	}
	return cutoff*maxIntensity;
}

double CSearchEngine::_GetBaselineByHistogram(double SNratio)
{
	if(m_Spectra->vPeaksTbl.empty()) return 0.0;
	// 把所有的谱峰强度取log后存储在LogInten中
	int peaknum = m_Spectra->vPeaksTbl.size();
	vector<double> logInten;
	logInten.reserve(peaknum+1);
	size_t i = 0;
	double MaxIntensity = 0; 
	double MinIntensity = 100;
	while(i < peaknum)
	{
		if(m_Spectra->vPeaksTbl[i].lfIntens <= 0)
		{
			continue;
		}
		logInten.push_back(log10(m_Spectra->vPeaksTbl[i].lfIntens));
		if (logInten[i] < MinIntensity)
		{
			MinIntensity = logInten[i];
		}
		if (logInten[i] > MaxIntensity)
		{
			MaxIntensity = logInten[i];
		}
		++i;
	}
	const double distIntens = 0.08;  //0.05
	vector<double> SliceBin;
	SliceBin.push_back(distIntens + MinIntensity);  //[wrm] distIntens / 2 + MinIntensity  wrm20150916: 第一个bin的大小为什么和后面不一样？
	int j = 0;
	while (SliceBin[j] + distIntens < MaxIntensity)
	{
		SliceBin.push_back(SliceBin[j] + distIntens);
		++j;
	}
	SliceBin.push_back(SliceBin[j] + distIntens);
	//Init
	int SizeofSliceBin = SliceBin.size();
	vector<int> HistCounter;
	HistCounter.assign(SizeofSliceBin, 0);

	for (j = 0; j < (int)peaknum; ++j)
	{
		size_t idx = 0;
		if (logInten[j] <= SliceBin[0])
		{
			idx = 0;
		} else {
			idx = (unsigned int)ceil((logInten[j] - SliceBin[0]) / distIntens);   // 向上取整
		}
		if(idx > HistCounter.size())
		{
			cout << "[Error] HistCounter get normal upper bound" << endl;
			exit(0);
		}
		++HistCounter[idx];		
	}
	// 去除最高的 top_num 的谱峰
	int top_num = peaknum/3;
	int cnt_num = 0;
	for(j = SizeofSliceBin-1; j>=0; --j){
		cnt_num += HistCounter[j];
		if(cnt_num > top_num){
			break;
		}
	}
	int MaxIndex = j;
	for( ; j>=0; --j){
		if (HistCounter[MaxIndex] < HistCounter[j])
		{
			MaxIndex = j;
		}
	}
	double baseline = SliceBin[MaxIndex] - distIntens / 2;
	//int leftidx = MaxIndex - 1, rightidx = MaxIndex + 1;
	//while(leftidx >= 0 && HistCounter[leftidx] > (HistCounter[MaxIndex]+1) / 2)
	//{
	//	--leftidx;
	//}
	//while(rightidx < (int)HistCounter.size() && HistCounter[rightidx] > (HistCounter[MaxIndex]+1) / 2)
	//{
	//	++rightidx;
	//}
	//baseline += SNratio * distIntens * (rightidx - leftidx - 1) / 2.355;
	//
	return SNratio*pow(10, baseline);
}


double CSearchEngine::_GetTotalIntensity(double PrecursorMass)
{
	if(m_Spectra->vPeaksTbl.empty())  return 0;
	double sumIntens = 0;
	for(int i=0; i< m_Spectra->vPeaksTbl.size(); ++i){
		if(m_Spectra->vPeaksTbl[i].lfMz > PrecursorMass){
			break;
		}
		if(m_Spectra->vPeaksTbl[i].lfIntens > baseline){
			sumIntens += m_Spectra->vPeaksTbl[i].lfIntens;
		}
	}
	return sumIntens;
}

void CSearchEngine::RefinedScore()
{
	//cout<<"RefineScore"<<endl;
    int actStatus = 1; 
	baseline = _GetBaselineByHistogram(1.25);
	//baseline = _GetBaselineByThreshold(0.02);
    for(int prsm = 0; prsm < m_Spectra->vPrecursors[m_nPrecurID].nValidCandidateProNum; ++prsm)
    {
		totalIntensity = _GetTotalIntensity(m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[prsm].lfMass-55.0);
		int len = m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[prsm].strProSQ.length();
        _GetTheoryFragIons(m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[prsm].strProSQ, actStatus, m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[prsm].vModSites);

		// 预匹配，先求出所有匹配谱峰的误差中位数，然后细打分的时候进行校正
		double adjustTol = 0.0;
		double intens = 0;
		int cnt = 0;
		for(int i = 0; i < len - 1; ++i)
        {
			 vector<double> sumIntens, massVol;
			_LookupHashTbl(0, 0.0, m_theoNFragIon[i], sumIntens, massVol, intens);
			_LookupHashTbl(0, 0.0, m_theoCFragIon[len - i - 2], sumIntens, massVol, intens);
			for(size_t j = 0; j < massVol.size(); ++j)
			{
				adjustTol += massVol[j];
				++cnt;
			}
        }
		if(cnt <= 0)
		{
			m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[prsm].lfScore = 0.0;
			continue;
		} else {
			adjustTol /= cnt;
		}

		double score = _BM25Score( 1, adjustTol,m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[prsm]);
		double logs = m_Scoefficient[1] * score + m_Scoefficient[0];

		//cout << score << " " << m_Scoefficient[1] << " " << m_Scoefficient[0] << " " << exp(logs) << endl;
        m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[prsm].lfScore = score;
		if(score < DOUCOM_EPS)
		{
			m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[prsm].lfQvalue = 1.0;
		} else {
			m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[prsm].lfQvalue = exp(logs);
		}

		#ifdef _DEBUG_REScore
		//cout << m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[prsm].strProSQ << endl;
		cout << "matched = " << m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[prsm].nMatchedPeakNum << endl;
		for(int test = 0; test < (int)m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[prsm].vModSites.size(); ++test)
		{
			cout << m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[prsm].vModSites[test].nFirst << " " << m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[prsm].vModSites[test].nSecond << endl;
		}
		cout << "adjustTol = " << adjustTol << endl;
		cout << "score = " << score << endl;
		#endif
    }
}
// 创建谱峰质量哈希表，m_hashTbl[i]中保存了质量大于等于i的第一根谱峰的下标
void CSearchEngine::_CreateHashTbl()
{
	m_hashTbl.assign(MAX_HASH_SIZE, 0);
	for(size_t i = 0; i < m_Spectra->vPeaksTbl.size(); ++i)
	{
		if(m_hashTbl[int(m_Spectra->vPeaksTbl[i].lfMz)] == 0)
		{
			m_hashTbl[int(m_Spectra->vPeaksTbl[i].lfMz)] = i + 1;
		}
	}
	int prePos = m_Spectra->vPeaksTbl.size();
	if(m_Spectra->vPeaksTbl.size() == 0)
	{
		return;
	}
	for(int m = (int)m_Spectra->vPeaksTbl.back().lfMz; m >= 0; --m)
	{
		if(m_hashTbl[m] == 0) 
		{
			m_hashTbl[m] = prePos;
		} else {
			prePos = m_hashTbl[m];
		}
	}
}

void CSearchEngine::Search(const vector<PROTEIN_STRUCT> &proList)
{

	if(m_Spectra->nPeaksNum <= 0 || m_Spectra->nPrecurNum == 0) 
	{
		return;
	}

	try
	{
		// 创建质量谱峰哈希表
		_CreateHashTbl();
		//m_Spectra->aBestPrSMs.assign(TOPSCORE, PROTEIN_SPECTRA_MATCH());
		vector<PROID_NODE> candiProID;

		//cout<<"[pTop] Getting the tags..."<<endl;
		m_cTag->GeneTag(m_Spectra);
		
		for(m_nPrecurID = 0; m_nPrecurID < m_Spectra->nPrecurNum; ++m_nPrecurID)
		{
			//cout << m_Spectra->strSpecTitle << "\t" << m_nPrecurID << "\t";
			m_cTag->Run(m_nPrecurTol, m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass, m_cTagFlow, proList, candiProID);


			if(candiProID.size() > 0 && candiProID.size() < 10){
				getCandidatesInWindow(m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass, proList, candiProID);
			}

			//cout<<"[Debug] size of candidate protein: "<<caidiProID.size()<<endl;
			
			m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs.clear();
			m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs.assign(TOPSCORE, PROTEIN_SPECTRA_MATCH());
			//cout << "sketch" << endl;
			SketchScore(proList, candiProID);
		}
	}
	catch(exception & e) 
	{
		CErrInfo info("CSearchEngine", "Search()", "Get candidate proteoforms failed.");
		throw runtime_error(info.Get(e).c_str());
	}
	catch(...) 
	{
		CErrInfo info("CSearchEngine", "Search()", "Caught an unknown exception from getting candidate.");
		throw runtime_error(info.Get().c_str());
	}
	//fprintf(ftest, "%d, ", m_Spectra->nPeaksNum);
	
	if(m_Spectra->nPeaksNum > 200)
	{
		m_Scoefficient[0] = 0.0023 * m_Spectra->nPeaksNum + 1.115;
		m_Scoefficient[1] = -0.0001 * m_Spectra->nPeaksNum - 0.9222;
	} else {
		int maxRandomPnum = 5000;
		srand((unsigned int)(1000));   // 伪随机数种子为1000，则每次产生的随机数序列都是相同的
		vector<double> vRandScore;
		for(int i = 0; i < maxRandomPnum; ++i)
		{
			double randscore = _GetRandomProteoformIons(m_Spectra->vPrecursors[0].lfPrecursorMass);
			if(randscore > 0)
			{
				vRandScore.push_back(randscore);
			}
			//fprintf(ftest, "%lf,", randscore);
			//cout << randscore << endl;
		}
		//fprintf(ftest, "%\n");
		_FitHighScores(vRandScore);
		//cout << m_Spectra->nPeaksNum << " " << m_Scoefficient[0] << " " << m_Scoefficient[1] << endl;
		//cout << "refine" << endl;
	}
	for(m_nPrecurID = 0; m_nPrecurID < m_Spectra->nPrecurNum; ++m_nPrecurID)
	{
		if(m_Spectra->vPrecursors[m_nPrecurID].nValidCandidateProNum > 0)
		{
			RefinedScore();   // 细打分
		}
	}
}

void CSearchEngine::getCandidatesInWindow(double precurMass, const vector<PROTEIN_STRUCT> &proList, vector<PROID_NODE> &candiProID)
{
	int massIdx = int(precurMass+0.5);
	const int halfWindow = 200;
	vector<DOUBLE_INT> pro_delta;
	int maxmass = min(massIdx + halfWindow, MAX_HASH_SIZE);
	for(int mi = massIdx - halfWindow; mi <= maxmass; ++mi){
		for(int j=0; j< m_cTagFlow->v_mass2pro[mi].size(); ++j){
			DOUBLE_INT tmp;
			tmp.nb = m_cTagFlow->v_mass2pro[mi][j];
			tmp.lfa = fabs(precurMass - proList[tmp.nb].lfMass);
			pro_delta.push_back(tmp);
		}
	}
	sort(pro_delta.begin(), pro_delta.end());
	int maxidx = min(pro_delta.size(), MAX_CANDIPRO_NUM);
	for(int i=0; i<maxidx; ++i){
		candiProID.push_back(PROID_NODE(pro_delta[i].nb));
	}
}


void CSearchEngine::SecondSearch(const vector<PROTEIN_STRUCT> &proList)
{

	if(m_Spectra->nPeaksNum <= 0 || m_Spectra->nPrecurNum == 0) 
	{
		return;
	}

	try
	{
		_CreateHashTbl();

		for(m_nPrecurID = 0; m_nPrecurID < m_Spectra->nPrecurNum; ++m_nPrecurID)
		{	
			m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs.clear();
			m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs.assign(TOPSCORE, PROTEIN_SPECTRA_MATCH());
			
			m_Spectra->nCandidateProNum = proList.size();
			for(size_t pIdx = 0; pIdx < proList.size(); ++pIdx)
			{
				//cout << proList[pIdx].strProSQ << endl;
				double totalModMass = m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass - proList[pIdx].lfMass;
				int deltaMass = int( totalModMass + 0.5);

				//cout << m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass << " - " << proList[pIdx].lfMass << " = " << deltaMass << endl;
				MatchCandidate(proList[pIdx].strProSQ, proList[pIdx].strProAC, proList[pIdx].nIsDecoy, 0, deltaMass);
				if(proList[pIdx].strProSQ[0] == 'M')
				{
					string proSeq = proList[pIdx].strProSQ.substr(1,proList[pIdx].strProSQ.length() - 1);
					deltaMass = (int)(totalModMass + AAMass_M_mono + 0.5);

					//cout << deltaMass << endl;
					MatchCandidate(proSeq, proList[pIdx].strProAC, proList[pIdx].nIsDecoy, 0, deltaMass);
				}
			}// end for pIdx
		}
	}
	catch(exception & e) 
	{
		CErrInfo info("CSearchEngine", "SecondSearch()", "Get candidate proteoforms failed.");
		throw runtime_error(info.Get(e).c_str());
	}
	catch(...) 
	{
		CErrInfo info("CSearchEngine", "SecondSearch()", "Caught an unknown exception from getting candidate.");
		throw runtime_error(info.Get().c_str());
	}
	
	
	if(m_Spectra->nPeaksNum > 200)
	{
		m_Scoefficient[0] = 0.0023 * m_Spectra->nPeaksNum + 1.115;
		m_Scoefficient[1] = -0.0001 * m_Spectra->nPeaksNum - 0.9222;
	} else {
		int maxRandomPnum = 5000;
		srand((unsigned int)(1000));
		vector<double> vRandScore;
		for(int i = 0; i < maxRandomPnum; ++i)
		{
			double randscore = _GetRandomProteoformIons(m_Spectra->vPrecursors[0].lfPrecursorMass);
			if(randscore > 0)
			{
				vRandScore.push_back(randscore);
			}
		}
		_FitHighScores(vRandScore);
	}

	for(m_nPrecurID = 0; m_nPrecurID < m_Spectra->nPrecurNum; ++m_nPrecurID)
	{
		if(m_Spectra->vPrecursors[m_nPrecurID].nValidCandidateProNum > 0)
		{
			RefinedScore();
		}
	}
}

/**
*	caculate the e-value
**/

void CSearchEngine::_GetAllAAMasses()
{
	m_vAAMass.clear();
	for(size_t i = 0; i < 26; ++i)
    {   //不加修饰的20种氨基酸
		m_vAAMass.push_back( m_mapAAMass->GetAAMass('A' + i) );
    }

	for(size_t i = 0; i < 26; ++i)
    {
		char ch = 'A' + i;
		vector<int> vModIDs;
		m_getPTMForm->GetAllIDsbyAA(ch, vModIDs);
		for(size_t cIdx = 0; cIdx < (int)vModIDs.size(); ++cIdx)
        {   //加修饰的氨基酸
			double mass = m_mapAAMass->GetAAMass(ch) + m_getPTMForm->GetMassbyID(vModIDs[cIdx]);
			m_vAAMass.push_back( mass );
        }
    }
}

// Calculate the theory mass of fragment ions of random proteoforms
double CSearchEngine::_GetRandomProteoformIons(double mass)
{
	int pIdx = 0;
	int nUnbond = m_vAAMass.size();
	double randProMass = 0.0;
	vector<double> nProAAMass;
	while(randProMass < mass)
	{
		int idx = rand() % nUnbond;
		nProAAMass.push_back( m_vAAMass[idx] );
		randProMass += m_vAAMass[idx];
		if(mass - randProMass < AVG_PRO_MASS)
		{
			break;
		}
	}

	_GenerateIonsbyAAMassLst(nProAAMass);

	double score = _BM25Score(1, 0.0);
    return score;
}


void CSearchEngine::_Lsqt( vector<double> &x, vector<double> &y)
{
	size_t n = x.size();
	if(0 == n || n != y.size())
	{
		cout << "[Error] Invalid input for the Least Squares function!" << endl;
		return;
	}
	double xx = 0.0;
	double yy = 0.0;
	for(size_t i = 0; i < n; ++i)
	{
		xx += x[i];
		yy += y[i];
	}
	xx /= n;
	yy /= n;

	double e = 0.0;
	double f = 0.0;
	for(size_t i = 0; i < n; ++i)
	{
		double q = x[i] - xx;
		e = e + q * q;
		f = f + q * (y[i] - yy);
	}
	
	m_Scoefficient[1] = f / e;
	m_Scoefficient[0] = yy - m_Scoefficient[1] * xx;
	//cout << m_Scoefficient[0] << " " << m_Scoefficient[1] << endl;
}

// 
void CSearchEngine::_FitHighScores(vector<double> &vScore)
{	
	m_Scoefficient[0] = 1;
	m_Scoefficient[1] = 0;
	if(vScore.size() < 2)
	{
		return;
	}
	sort(vScore.begin(), vScore.end());
	int itmp = 0;
	for (size_t i = vScore.size() - 1; i > 0; --i )
	{
		if ( vScore[i-1] == vScore[i])
		{
			++itmp;
		} else {
			break;
		}
	}
	size_t start = (size_t)( (vScore.size() - itmp) * 0.9 + 1);
	size_t n = vScore.size() - itmp - start;
	
	if ( n> 20)//the number of scores cannot be too small
	{
		vector<double> logs(n, 0);
		vector<double> logx(n, 0);
		double temp = log(0.1 / n);

		for(size_t i = 0; i < n; ++i)
		{
			logx[i] = vScore[start + i];
			logs[i] = temp + log((double)(n - i));
			//cout << logx[i] << " " << logs[i] << endl;
		}

		_Lsqt(logx, logs);
	}	
}