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

#include "searcher.h"

//#define _DEBUG_CHECK
//#define _DEBUG_SEARCH
using namespace std;


bool DDCmp(const DOUBLE_DOUBLE d1, const DOUBLE_DOUBLE d2)
{
    return d1.lfa < d2.lfa;
}
bool DIntCmp(const DOUBLE_INT d1, const DOUBLE_INT d2)
{
    return d1.nb > d2.nb;
}

CSearchEngine::CSearchEngine(CConfiguration *cParam, CMapAAMass *cMapMass, CPrePTMForm *cPTMForms, CTagFlow *cTagIndex, SPECTRUM *spec) :
m_cParam(cParam), m_nPrecurTol((int)(cParam->m_lfPrecurTol) + 1), m_cProIndex(NULL),
m_Spectra(spec), m_mapAAMass(cMapMass), m_cPTMIndex(cPTMForms), m_cTagFlow(cTagIndex), m_cTag(NULL),
totalIntensity(0), baseline(0), m_nPrecurID(0), m_lfPrecurMass(0)
{
	m_clog = Clog::getInstance();
	Init();
}

CSearchEngine::CSearchEngine(CConfiguration *cParam, CMapAAMass *cMapMass, CPrePTMForm *cPTMForms, CProteinIndex *cProIndex, CTagFlow *cTagIndex, SPECTRUM *spec) :
m_cParam(cParam), m_nPrecurTol((int)(cParam->m_lfPrecurTol) + 1), 
m_Spectra(spec), m_mapAAMass(cMapMass), m_cPTMIndex(cPTMForms), m_cProIndex(cProIndex), m_cTagFlow(cTagIndex), m_cTag(NULL),
totalIntensity(0), baseline(0), m_nPrecurID(0), m_lfPrecurMass(0)
{
	m_clog = Clog::getInstance();
	Init();
}

void CSearchEngine::SetProteinIndex(CProteinIndex* proIndex)
{
	m_cProIndex = proIndex;
}

void CSearchEngine::Init()
{
	if (m_cTag == NULL)
		m_cTag = new CTagSearcher();
	m_cTag->Init(m_mapAAMass, m_cPTMIndex, m_cParam->m_lfFragTol);

	m_nPrecurID = 0;
	m_lfPrecurMass = 0.0;

	m_vMatchedPeak.clear();

	int modCnt = (int)m_cPTMIndex->GetModNum();  // 修饰数目
	m_vModID2Num.assign(modCnt + 1, 0);

	m_vProSeqModNum.clear();
	m_hashTbl.clear();

	m_theoNFragIon.clear();
	m_theoCFragIon.clear();
	
	m_mapProLastModSite.clear();

	m_vMatchedPeak.assign(m_Spectra->vPeaksTbl.size(), 0);

#ifdef _DEBUG_SEARCH
	fp = fopen("tmp\\allCandi.txt", "w");  //@test  modForms.txt
	fp2 = fopen("tmp\\candiNum.txt", "w");
#endif
}

CSearchEngine::~CSearchEngine()
{
	if (m_Spectra){
		m_Spectra = NULL;
	}
	if (m_mapAAMass){
		m_mapAAMass = NULL;
	}
	if (m_cPTMIndex){
		m_cPTMIndex = NULL;
	}
	if (m_cTagFlow){
		m_cTagFlow = NULL;
	}
	if (m_cTag){
		delete m_cTag;
		m_cTag = NULL;
	}
	if (m_clog){
		m_clog = NULL;
	}

#ifdef _DEBUG_SEARCH
	fclose(fp);  //@test
	fclose(fp2);
#endif
}

// Seach for the protein in the tolerance window
int CSearchEngine::_BinSearch(double mass)
{
	const vector<PROTEIN_STRUCT> &m_ProteinList = m_cProIndex->m_ProteinList;
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
bool CSearchEngine::_CheckModForm(vector<int> &modForms, const string &proSeq, int startPos, int endPos)
{
    //cout<<"_CheckModForm..."<<endl;
    int i = 0;
	int modCnt = (int)m_cPTMIndex->GetModNum();  // 修饰数目

    // Create a vector (mod ID -> quantity), in order to check if the Set in the vertice is valid
	// 记录该修饰组合中每种修饰的数目
	for(int modID = 0; modID < modCnt; ++modID)
	{
        m_vModID2Num[modID] = 0;
	}
	//vector<int> &modForms = m_getPTMForm->m_ModForms[massIdx][ptmIdx];
	int sumModNumber = modForms.size();
    for(i = 0; i < sumModNumber; ++i)
    {
        ++m_vModID2Num[modForms[i]];
    }
    // Check whether the protein sequence have enough mod sites for this mod form
    if(0 == startPos && endPos == (int)proSeq.length())
    {
        for(int i = 0; i < modForms.size(); ++i)
        {
			string sites = "";
			m_cPTMIndex->GetSitesbyID(modForms[i], sites);
			if (m_cPTMIndex->isNtermMod(modForms[i])){  // check N-term
				if (sites.find(proSeq[0]) == string::npos){
					return false;
				}
			}
			else if (m_cPTMIndex->isCtermMod(modForms[i])){  // check C-term
				if (sites.find(proSeq.back()) == string::npos){
					return false;
				}
			}
			else if (m_vModID2Num[modForms[i]] > m_vProSeqModNum[modForms[i]])
            {
                return false;
            }
        }
    } else {
        vector<int> tmpNums(modCnt, 0); // 记录从 start ~ pos 这段序列上每种修饰可发生的数目
		vector<int> vModIDs;
        for(int sPos = startPos; sPos < endPos; ++sPos)
        {
			m_cPTMIndex->GetAllIDsbyAA(proSeq[sPos], vModIDs);
            for(size_t i = 0; i < vModIDs.size(); ++i)
            {
                ++tmpNums[vModIDs[i]];
            }
        }
        for(int i = 0; i < modForms.size(); ++i)
        {

            if(m_vModID2Num[modForms[i]] > tmpNums[modForms[i]])
            {
                return false;
            }
        }
    }

    // Check the N-term mod and C-term mod must be only one
	bool nTerm = false, cTerm = false;
	if (m_mapAAMass->GetNtermMass(proSeq[0]) > 0)
	{
		nTerm = true;
	}
	if (m_mapAAMass->GetCtermMass(proSeq.back()) > 0)
	{
		cTerm = true;
	} else {
        for(i = 0; i < (int)modForms.size(); ++i)
        {
			bool isNterm = m_cPTMIndex->isNtermMod(modForms[i]);
			bool isCterm = m_cPTMIndex->isCtermMod(modForms[i]);
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
	m_theoNFragIon.clear();
	m_theoCFragIon.clear();

	if (vProAAMass.empty()){  // bounds check
		char err[READLEN];
		sprintf(err, "vProAAMass of spectra[%d] is empty.", m_Spectra->m_nScanNo);
		CErrInfo info("CSearchEngine", "_GenerateIonsbyAAMassLst", err);
		m_clog->error(info.Get());
		exit(0);
	}

	int pIdx = 0;
	int nProLen = vProAAMass.size();
	vector<double> empty;
	m_theoNFragIon.assign(nProLen - 1, empty); // a,b,c
	m_theoCFragIon.assign(nProLen - 1, empty); // x,y,z

	if (m_cParam->calAX)
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


	if (m_cParam->calBY)
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

	if (m_cParam->calCZ)    // [wrm?]考虑c/z和c-H/z+H
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
				nProAAMass[0] += (m_cPTMIndex->GetMassbyID(modSites[i].nFirst));
			} else if(nProLen == (int)modSites[i].nSecond - 1){ // C-term modification
				nProAAMass[nProLen - 1] += (m_cPTMIndex->GetMassbyID(modSites[i].nFirst));
			} else {
				nProAAMass[modSites[i].nSecond - 1] += (m_cPTMIndex->GetMassbyID(modSites[i].nFirst));
			}
		}
	}

	_GenerateIonsbyAAMassLst(nProAAMass);

	if (m_cParam->m_bNeutralLoss){
		vector<byte> &m_vNeutralLossAA = m_mapAAMass->m_vNeutralLossAA;
		m_theoN_H2OLossAA.assign(nProLen, 0);
		m_theoN_NH3LossAA.assign(nProLen, 0);
		m_theoC_H2OLossAA.assign(nProLen, 0);
		m_theoC_NH3LossAA.assign(nProLen, 0);
		if (m_vNeutralLossAA[proSeq[0] - 'A'] == 1){
			m_theoN_H2OLossAA[0] = 1;
		}
		else if (m_vNeutralLossAA[proSeq[0] - 'A'] == 2){
			m_theoN_NH3LossAA[0] = 1;
		}
		if (m_vNeutralLossAA[proSeq[nProLen - 1] - 'A'] == 1){
			m_theoC_H2OLossAA[0] = 1;
		}
		else if (m_vNeutralLossAA[proSeq[nProLen - 1] - 'A'] == 2){
			m_theoC_NH3LossAA[0] = 1;
		}
		for (pIdx = 1; pIdx < nProLen; ++pIdx){
			m_theoN_H2OLossAA[pIdx] = m_theoN_H2OLossAA[pIdx - 1] + (m_vNeutralLossAA[proSeq[pIdx] - 'A'] == 1 ? 1 : 0);
			m_theoN_NH3LossAA[pIdx] = m_theoN_NH3LossAA[pIdx - 1] + (m_vNeutralLossAA[proSeq[pIdx] - 'A'] == 2 ? 1 : 0);
			
			m_theoC_H2OLossAA[pIdx] = m_theoC_H2OLossAA[pIdx - 1] + (m_vNeutralLossAA[proSeq[nProLen - 1 - pIdx] - 'A'] == 1 ? 1 : 0);
			m_theoC_NH3LossAA[pIdx] = m_theoC_NH3LossAA[pIdx - 1] + (m_vNeutralLossAA[proSeq[nProLen - 1 - pIdx] - 'A'] == 2 ? 1 : 0);
		}
	}
	
}
// 保存候选蛋白质变体
void CSearchEngine::_WriteToCandidate(const string &proSeq, const string &proAC, priority_queue<PATH_NODE> &qPaths, int proType)
{
    double modMass = 0.0;
    while(!qPaths.empty())
    {
        PATH_NODE topPath = qPaths.top();
		qPaths.pop();
		double proMass = m_lfPrecurMass;
		for (size_t i = 0; i < topPath.vModSites.size(); ++i)
		{
			modMass = m_cPTMIndex->GetMassbyID(topPath.vModSites[i].nFirst);
			proMass += modMass;
		}
		if (fabs(m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass -proMass) > m_cParam->m_lfPrecurTol){
			continue;
		}
        if(m_Spectra->vPrecursors[m_nPrecurID].nValidCandidateProNum < (int)TOPSCORE)
        {
			int idx = m_Spectra->vPrecursors[m_nPrecurID].nValidCandidateProNum;
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[idx].strProSQ = proSeq;
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[idx].strProAC = proAC;
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[idx].lfMass = proMass;
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[idx].nMatchedPeakNum = topPath.nWeight;
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[idx].nIsDecoy = proType;
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[idx].vModSites = topPath.vModSites;

            
			// 按照匹配到的谱峰数目从小到大排序  [wrm?]这里有没有可能所有路径加起来也没有达到TOPSCORE条
            if(++(m_Spectra->vPrecursors[m_nPrecurID].nValidCandidateProNum) == (int)TOPSCORE)
            {
				sort(m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs.begin(), m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs.end(), PROTEIN_SPECTRA_MATCH::MatchedPeaksLess);
            }
        } else if(topPath.nWeight > m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[0].nMatchedPeakNum){
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[0].strProSQ = proSeq;
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[0].strProAC = proAC;
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[0].lfMass = proMass;
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[0].nMatchedPeakNum = topPath.nWeight;
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[0].nIsDecoy = proType;
            m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[0].vModSites = topPath.vModSites;

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
    }
}

// The elments in "modForm" are the numbers of the i_th mod
double CSearchEngine::_GetModMass(vector<int> &modForm)
{
	double modMass = 0;
    // Get the mod mass
    for(size_t i = 0; i < modForm.size(); ++i)
    {
		modMass += modForm[i] * m_cPTMIndex->GetMassbyID(i);
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
		modMass += m_cPTMIndex->GetMassbyID(modSet[i]);
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
			double lowerBound = m_cParam->m_lfFragTol * lfMass / 1000000;
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
			double lowerBound = lfMass + (shiftPPM - m_cParam->m_lfFragTol) * lfMass / 1000000;
			double upperBound = lfMass + (shiftPPM + m_cParam->m_lfFragTol) * lfMass / 1000000;
			
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
				double tmpTol = m_cParam->m_lfFragTol;
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

// return matched_pos
int CSearchEngine::_AccurateMatch(double mass, vector<double>& smallWnd, double shiftPPM, double &peaktol)
{
	int nIonPos, peakNum = m_Spectra->nPeaksNum;
	int minpeakMass = (int)m_Spectra->vPeaksTbl[0].lfMz;
	int maxpeakMass = (int)m_Spectra->vPeaksTbl[peakNum - 1].lfMz;
	for (size_t m = 0; m < smallWnd.size(); ++m) // -1与0两个小窗口是否会发生重叠？质量超过25000时会
	{
		double lfMass = mass + smallWnd[m];
		int tmpMass = (int)lfMass;
		if (tmpMass > maxpeakMass || tmpMass < minpeakMass)
		{   // 理论碎片离子质量在所有实验谱峰的最小值和最大值之间才处理
			continue;
		}

		nIonPos = (int)m_hashTbl[tmpMass] - 1;
		if (nIonPos < 0)
		{
			continue;
		}
		double lowerBound = lfMass + (shiftPPM - m_cParam->m_lfFragTol) * lfMass / 1000000;
		double upperBound = lfMass + (shiftPPM + m_cParam->m_lfFragTol) * lfMass / 1000000;

		while (nIonPos < peakNum && m_Spectra->vPeaksTbl[nIonPos].lfMz < lowerBound)
		{
			++nIonPos;
		}
		// [wrm] there maybe many matched peaks, we keep the closest one. if->while. modified by wrm. @2016.05.13
		int matchedPos = -1;
		double tol = 100;
		while (nIonPos < peakNum && m_Spectra->vPeaksTbl[nIonPos].lfMz <= upperBound)
		{
			double tmpTol = m_cParam->m_lfFragTol;
			if (0 != m) // 对偏离±1Da的进行惩罚
			{
				if (m_Spectra->vPeaksTbl[nIonPos].lfMz > lfMass)
				{
					tmpTol = 5 - shiftPPM + 1000000 * (m_Spectra->vPeaksTbl[nIonPos].lfMz - lfMass) / lfMass;
				}
				else {
					tmpTol = -5 - shiftPPM + 1000000 * (m_Spectra->vPeaksTbl[nIonPos].lfMz - lfMass) / lfMass;
				}
			}
			else {
				tmpTol = 1000000 * (m_Spectra->vPeaksTbl[nIonPos].lfMz - lfMass) / lfMass - shiftPPM;
			}

			if (fabs(tmpTol) < fabs(tol)){
				tol = tmpTol;
				matchedPos = nIonPos;
			}
			++nIonPos;
		}
		if (matchedPos > -1){
			peaktol = tol;
			return matchedPos;
		}
	}
	return -1;
}

// consider neutral_loss  D/E/S/T -H2O, K/N/Q/R -NH3
int CSearchEngine::_LookupHashTbl(int type, double shiftPPM, int pos, bool isNterm, vector<double> &ionMass, vector<double> &matchedInts, vector<double> &massVol, double &sumMatchedIntens)
{
	vector<double> smallWnd;
	smallWnd.push_back(0.0);
	if (1 == type)
	{
		smallWnd.push_back(-DIFF_13C);
		smallWnd.push_back(DIFF_13C);
	}
	vector<double> lossIons;
	double intensity = DOUCOM_EPS;
	double peaktol = 0;
	int matchedIons = 0;
	sumMatchedIntens = 0;
	for (size_t j = 0; j < ionMass.size(); ++j)
	{
		int matchedPos = _AccurateMatch(ionMass[j], smallWnd, shiftPPM, peaktol);
		if (matchedPos > -1){
			if (0 == m_vMatchedPeak[matchedPos]) //该谱峰没有被其他的碎片离子匹配上
			{
				matchedInts.push_back(m_Spectra->vPeaksTbl[matchedPos].lfIntens);
				m_vMatchedPeak[matchedPos] = 1;
			}
			else {
				matchedInts.push_back(DOUCOM_EPS);
			}
			if (matchedInts.back() > baseline){
				sumMatchedIntens += matchedInts.back();
			}
			massVol.push_back(peaktol);
			++ matchedIons;
			if (m_cParam->m_bNeutralLoss){
				_AddNeutralLossIons(pos, isNterm, ionMass[j], lossIons);
				for (int l = 0; l<lossIons.size(); ++l){
					matchedPos = _AccurateMatch(lossIons[l], smallWnd, shiftPPM, peaktol);
					if (matchedPos > -1){
						if (0 == m_vMatchedPeak[matchedPos]) //该谱峰没有被其他的碎片离子匹配上
						{
							matchedInts.push_back(m_Spectra->vPeaksTbl[matchedPos].lfIntens);
							m_vMatchedPeak[matchedPos] = 1;
							if (matchedInts.back() > baseline){
								sumMatchedIntens += matchedInts.back();
							}
						}
						else {
							matchedInts.push_back(DOUCOM_EPS);
						}
						massVol.push_back(peaktol);
						++ matchedIons;
					}
				}
			}
		}
	}
	return matchedIons;
}

void CSearchEngine::_AddNeutralLossIons(int pos, bool isNterm, double mass, vector<double> &ionMass)
{
	ionMass.clear();
	if (isNterm){
		if (m_theoN_H2OLossAA[pos] >= 2){
			ionMass.push_back(mass - IonMass_H2O);   // loss one H2O
			ionMass.push_back(mass - 2 * IonMass_H2O);  // loss two H2O
		}
		else if (m_theoN_H2OLossAA[pos] >= 1){
			ionMass.push_back(mass - IonMass_H2O);  // loss one H2O
		}
		if (m_theoN_NH3LossAA[pos] >= 2){
			ionMass.push_back(mass - IonMass_NH3);   // loss one NH3
			ionMass.push_back(mass - 2 * IonMass_NH3);  // loss two NH3
		}
		else if (m_theoN_NH3LossAA[pos] >= 1){
			ionMass.push_back(mass - IonMass_NH3);  // loss one NH3
		}
		if (m_theoN_H2OLossAA[pos] >= 1 && m_theoN_NH3LossAA[pos] >= 1){
			ionMass.push_back(mass - IonMass_H2O - IonMass_NH3);  // loss one NH3, one H2O
		}
	}
	else{  // C term
		if (m_theoC_H2OLossAA[pos] >= 2){
			ionMass.push_back(mass - IonMass_H2O);   // loss one H2O
			ionMass.push_back(mass - 2 * IonMass_H2O);  // loss two H2O
		}
		else if (m_theoC_H2OLossAA[pos] >= 1){
			ionMass.push_back(mass - IonMass_H2O);  // loss one H2O
		}
		if (m_theoC_NH3LossAA[pos] >= 2){
			ionMass.push_back(mass - IonMass_NH3);   // loss one NH3
			ionMass.push_back(mass - 2 * IonMass_NH3);  // loss two NH3
		}
		else if (m_theoC_NH3LossAA[pos] >= 1){
			ionMass.push_back(mass - IonMass_NH3);  // loss one NH3
		}
		if (m_theoC_H2OLossAA[pos] >= 1 && m_theoC_NH3LossAA[pos] >= 1){
			ionMass.push_back(mass - IonMass_H2O - IonMass_NH3);  // loss one NH3, one H2O
		}
	}
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
	emptySet.assign(m_cPTMIndex->GetModNum(), 0);
    DP_VERTICE startNode(emptySet, emptyPath); // Start Node of mod set
    leftTmpDist[0].push_back(startNode);   // 添加起点
	rightTmpDist[0].push_back(startNode);
    // If mod form has N-term mod add another new Node
	for(int i = 0; i < (int)modForm.size(); ++i)
	{
		int nTermModID = modForm[i];
		if (m_cPTMIndex->GetNCModNum() > 0)
		{
			if (m_cPTMIndex->isNtermMod(nTermModID)) // N-term mod
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
			}
			else if (m_cPTMIndex->isCtermMod(nTermModID)) { // C-term mod
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
		m_cPTMIndex->GetAllIDsbyAA(proSeq[leftIdx], canAddModID);  // 获取该位点上可添加的修饰列表

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
					addedModMass = modMass + m_cPTMIndex->GetMassbyID(addModID);
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
		m_cPTMIndex->GetAllIDsbyAA(proSeq[rightIdx], rightCanAddModID);

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
				addedModMass = modMass + m_cPTMIndex->GetMassbyID(addModID);
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
	int modNum = (int)m_cPTMIndex->GetModNum();
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
        if( 1000000 * (m_Spectra->vPeaksTbl[tIdx].lfMz-preMZ)/preMZ <= m_cParam->m_lfFragTol)
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

	int avgLen = 2 * int(precurMass / AVG_AA_MASS);
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
			modMass += m_cPTMIndex->GetMassbyID(topPath.vModSites[i].nFirst);
		}
		if(fabs(modMass - totalModMass) <= 2.0 && topPath.nWeight > 0)
		{
			bestPath.push(topPath);
		}
		qPaths.pop();
    }
	qPaths = bestPath;
}

// Get the PTM forms that the protein could have by delta_mass
// Using Dynamic Programming to get several valid candidate proteoforms
void CSearchEngine::SketchScore(vector<CANDI_PROTEIN> &candiPros)
{
	//cout<<"SketchScore..."<<endl;

	int actStatus = 0;

	//cout<<"[Debug] Candidate number: "<<m_Spectra.nCandidateProNum<<endl;
	m_Spectra->nCandidateProNum = (int)candiPros.size();
	// The score of candidate proteins is 

	//cout << candiPros.size() << endl;
	for (auto it = candiPros.begin(); it != candiPros.end(); ++it)
	{
		size_t pIdx = it->nProID;
		string proSQ = it->strSeq;
		//printf("%d %s\n", pIdx, proSQ.c_str());

		//fprintf(fp, "%s\n", (it->strSeq).c_str());
		//double totalModMass = m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass - proList[pIdx].lfMass;
		double totalModMass = m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass - it->lfMass;
		int deltaMass = (int)(totalModMass + 0.5); // 谱图母离子质量 - 无修饰蛋白质量 = 修饰质量
		MatchCandidate(it->strSeq, m_cProIndex->m_ProteinList[pIdx].strProAC, m_cProIndex->m_ProteinList[pIdx].nIsDecoy, actStatus, deltaMass);
	}
}

void CSearchEngine::MatchCandidate(const string &proSQ, const string &proAC, int isDecoy, int actStatus, int deltaMass)
{
	vector<UINT_UINT> emptySite;
	_GetTheoryFragIons(proSQ, actStatus, emptySite);  // 获取理论碎片离子

	// Create a vector to record the sites could be moded in the sequence of the protein
	m_vProSeqModNum.clear();
	vector<int> vProLastModSite;
	int modID;
	for(modID = 0; modID < m_cPTMIndex->GetModNum(); ++ modID)
	{
		m_vProSeqModNum.push_back(0); // 保存该蛋白序列上每种修饰可发生的数目
		vProLastModSite.push_back(0); // 保存该蛋白序列上每种修饰可发生的最后一个位点
	}
	for(size_t sPos = 0; sPos < proSQ.length(); ++ sPos)
	{
		vector<int> vModIDs;
		m_cPTMIndex->GetAllIDsbyAA(proSQ[sPos], vModIDs);

		for(size_t i = 0; i < vModIDs.size(); ++ i)
		{
			++m_vProSeqModNum[vModIDs[i]];
			vProLastModSite[vModIDs[i]] = sPos;
		}
	}
	m_mapProLastModSite.clear();
	for (modID = 0; modID < m_cPTMIndex->GetModNum(); ++modID)
	{
		m_mapProLastModSite.insert(pair<int,int>(vProLastModSite[modID], modID));
	}
	vector<int> emptyIDs;
	// Consider all the mod forms satisfied the delta mass
	//cout<<"Precursor tol: "<<m_lfPrecurTol<<endl;
	int minDeltaMass = max(deltaMass - m_nPrecurTol, - MAX_SHIFT_MASS);
    int maxDeltaMass = min(deltaMass + m_nPrecurTol, MAX_SHIFT_MASS);
	//if(maxDeltaMass < 0) maxDeltaMass = 0;

    for(int dIdx = minDeltaMass; dIdx <= maxDeltaMass; ++dIdx) // +/- PRECUR_DELTA Da
    {
		int massIdx = dIdx+MAX_SHIFT_MASS;
		vector<vector<int>> modForms;
		m_cPTMIndex->GetVarModForms(massIdx, modForms);
		int ptmFormNum = modForms.size();
		//fprintf(fp, "%d\n", ptmFormNum);
		//printf("%d %d %d %s\n", m_nPrecurID, dIdx, ptmFormNum, proSQ.c_str());
        for(int mIdx = 0; mIdx < ptmFormNum; ++mIdx)
        {
			if( _CheckModForm(modForms[mIdx], proSQ, 0, (int)proSQ.length()) )
            {
                    priority_queue<PATH_NODE> bestPaths;
					double totalModMass = _GetModSetMass(modForms[mIdx]);
					//_GetValidCandiPro(m_getPTMForm->m_ModForms[massIdx][mIdx], proSQ, 0, (int)proSQ.length()-1, bestPaths, 0.0, totalModMass);
					_ModificationLocation(modForms[mIdx], proSQ, 0, proSQ.length() - 1, bestPaths, totalModMass);
					//cout<<bestPaths.size()<<endl;
					_WriteToCandidate(proSQ, proAC, bestPaths, isDecoy);
					//cout<<m_Spectra->nValidCandidateProNum<<endl;
            } // end if
        } // end for mIdx
    } // end for dIdx

}

void CSearchEngine::_MatchCandidateWithPTM(vector<CANDI_PROTEIN> &candiPros)
{
	int actStatus = 0;

	//cout<<"[Debug] Candidate number: "<<m_Spectra.nCandidateProNum<<endl;
	m_Spectra->nCandidateProNum = (int)candiPros.size();
	// The score of candidate proteins is increased
	for (auto it = candiPros.begin(); it != candiPros.end(); ++it)
	{
		size_t pIdx = it->nProID;
		double totalModMass = m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass - it->lfMass;
		int deltaMass = (int)(totalModMass + 0.5); // 谱图母离子质量 - 无修饰蛋白质量 = 修饰质量
		string proSQ = it->strSeq;
		vector<UINT_UINT> emptySite;
		_GetTheoryFragIons(proSQ, actStatus, emptySite);  // 获取理论碎片离子
		// Create a vector to record the sites could be moded in the sequence of the protein
		m_vProSeqModNum.clear();
		vector<int> vProLastModSite;
		int modID;
		for (modID = 0; modID < m_cPTMIndex->GetModNum(); ++modID)
		{
			m_vProSeqModNum.push_back(0); // 保存该蛋白序列上每种修饰可发生的数目
			vProLastModSite.push_back(0); // 保存该蛋白序列上每种修饰可发生的最后一个位点
		}
		for (size_t sPos = 0; sPos < proSQ.length(); ++sPos)
		{
			vector<int> vModIDs;
			m_cPTMIndex->GetAllIDsbyAA(proSQ[sPos], vModIDs);

			for (size_t i = 0; i < vModIDs.size(); ++i)
			{
				++m_vProSeqModNum[vModIDs[i]];
				vProLastModSite[vModIDs[i]] = sPos;
			}
		}
		m_mapProLastModSite.clear();
		for (modID = 0; modID < m_cPTMIndex->GetModNum(); ++modID)
		{
			m_mapProLastModSite.insert(pair<int, int>(vProLastModSite[modID], modID));
		}
		vector<int> emptyIDs;
		// Consider all the mod forms satisfied the delta mass
		//cout<<"Precursor tol: "<<m_lfPrecurTol<<endl;
		int minDeltaMass = max(deltaMass - m_nPrecurTol, -MAX_SHIFT_MASS);
		int maxDeltaMass = min(deltaMass + m_nPrecurTol, MAX_SHIFT_MASS);
		//if(maxDeltaMass < 0) maxDeltaMass = 0;

		for (int dIdx = minDeltaMass; dIdx <= maxDeltaMass; ++dIdx) // +/- PRECUR_DELTA Da
		{
			int massIdx = dIdx + MAX_SHIFT_MASS;
			vector<vector<int>> &modForms = m_cPTMIndex->m_vModForms[massIdx];
			int ptmFormNum = modForms.size();
			//fprintf(fp, "%d\n", ptmFormNum);
			//printf("%d %d %d %s\n", m_nPrecurID, dIdx, ptmFormNum, proSQ.c_str());
			for (int mIdx = 0; mIdx < ptmFormNum; ++mIdx)
			{
				if (_CheckModForm(modForms[mIdx], proSQ, 0, (int)proSQ.length()))
				{
					priority_queue<PATH_NODE> bestPaths;
					double totalModMass = _GetModSetMass(modForms[mIdx]);
					//_GetValidCandiPro(m_getPTMForm->m_ModForms[massIdx][mIdx], proSQ, 0, (int)proSQ.length()-1, bestPaths, 0.0, totalModMass);
					_ModificationLocation(modForms[mIdx], proSQ, 0, proSQ.length() - 1, bestPaths, totalModMass);
					//cout<<bestPaths.size()<<endl;
					_WriteToCandidate(proSQ, m_cProIndex->m_ProteinList[pIdx].strProAC, bestPaths, m_cProIndex->m_ProteinList[pIdx].nIsDecoy);
					//cout<<m_Spectra->nValidCandidateProNum<<endl;
				} // end if
			} // end for mIdx
		} // end for dIdx
	}
}

void CSearchEngine::GetMoreCandidate(int actStatus)
{
	const vector<PROTEIN_STRUCT> &proList = m_cProIndex->m_ProteinList;
	double tmpPrecurMass = m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass;
	double minCandiMass = tmpPrecurMass - WIND / 2; //modified at 2014.11.27
    double maxCandiMass = tmpPrecurMass + WIND / 2;
	// [wrm]: 蛋白质信息已按照质量排序，故可进行二分； [优化]：哈希
    int left = _BinSearch(minCandiMass);
    int right = _BinSearch(maxCandiMass);
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
    sort(vCandiProFiltScore.begin(), vCandiProFiltScore.end());  // order by score inc
    
	
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

// check the N/C term of the candidate proteins get from tag index
void CSearchEngine::_CheckCandiPros(vector<PROID_NODE> &candiProID, unordered_map<int, unordered_map<string, double>> &candiPros, vector<CANDI_PROTEIN> &validCandi)
{
	for (int i = (int)candiProID.size() - 1; i >= 0; --i)
	{
		int pIdx = candiProID[i].nProID;
		unordered_map<string, double> candiProSQ;
#ifdef _DEBUG_SEARCH
		if (m_cProIndex->m_ProteinList[pIdx].strProAC == "sp|P50263|SIP18_YEAST"){
			cout << m_cProIndex->m_ProteinList[pIdx].strProAC << endl;
		}
#endif
		_CheckNCterm(m_cProIndex->m_ProteinList[pIdx], candiProID[i], candiProSQ);
		if (!candiProSQ.empty()){
			if (candiProID[i].vTagInfo.size() >= 6){
				for (auto it = candiProSQ.begin(); it != candiProSQ.end(); ++it){
					validCandi.push_back(CANDI_PROTEIN(pIdx, it->first, it->second, candiProID[i].lfScore));
				}
			}
			else{
				candiPros[pIdx] = candiProSQ;
			}
		}
	}
#ifdef _DEBUG_SEARCH
	// @test


	for (auto i : validCandi) {
		cout << i.nProID << " " << i.strSeq << " is vaild" << endl;
	}
	for (auto i : candiPros) {
		cout << i.first << " -> ";
		for (auto j : i.second) {
			cout << j.first << " " << j.second << endl;
		}
	}
	cout << endl;
	/*FILE *fc = fopen("tmp\\tagCandiP.txt", "w");
	for (int i = (int)candiProID.size() - 1; i >= 0; --i){
		int pIdx = candiProID[i].nProID;
		fprintf(fc, "%s %d\n", m_cProIndex->m_ProteinList[pIdx].strProSQ.c_str(), candiProID[i].vTagInfo.size());
	}
	fclose(fc);*/
#endif
}

// check whether N/C term has been truncated
void CSearchEngine::_CheckNCterm(const PROTEIN_STRUCT& pro, PROID_NODE& candiPro, unordered_map<string, double>& vCandiProSeq)
{	
	/*if (pro.strProSQ.find("MVKIVTVKTQAYQDQKPGTSGLRKRVKVFQSSANYAENFIQSIISTVEPAQRQEATLVVGGDGRFYMKEAIQLIARIAAANGIGRLVIGQNGILSTPAVSCIIRKIKAIGGIILTASHNPGGPNGDFGIKFNISNGGPAPEAITDKIFQISKTIEEYAVCPDLKVDLGVLGKQQFDLENKFKPFTVEIVDSVEAYATMLRSIFDFSALKELLSGPNRLKIRIDAMHGVVGPYVKKILCEELGAPANSAVNCVPLEDFGGHHPDPNLTYAADLVETMKSGEHDFGAAFDGDGDRNMILGKHGFFVNPSDSVAVIAANIFSIPYFQQTGVRGFARSMPTSGALDRVASATKIALYETPTGWKFFGNLMDASKLSLCGEESFGTGSDHIREKDGLWAVLAWLSILATRKQSVEDILKDHWQKYGRNFFTRYDYEEVEAEGANKMMKDLEALMFDRSFVGKQFSANDKVYTVEKADNFEYSDPVDGSISRNQGLRLIFTDGSRIVFRLSGTGSAGATIRLYIDSYEKDVAKINQDPQVMLAPLISIALKVSQLQERTGRTAPTVIT") == string::npos) {
		return;
	}*/
	const double lfMaxNegModMass = 200.0;
	double deltaMass = pro.lfMass - m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass;
	if (deltaMass < lfMaxNegModMass  && -deltaMass <= TOTAL_MASS_SHIFT){
		vCandiProSeq.insert(make_pair(pro.strProSQ, pro.lfMass));
	}
	if (pro.strProSQ[0] == 'M' && (deltaMass - AAMass_M_mono) <= lfMaxNegModMass && (AAMass_M_mono - deltaMass) <= TOTAL_MASS_SHIFT){
		vCandiProSeq.insert(make_pair(pro.strProSQ.substr(1), pro.lfMass - AAMass_M_mono));
	}
	if (candiPro.vTagInfo.empty())  return;
	double minAAMass = m_cTag->GetMinAAMass();
	double maxAAMass = m_cTag->GetMaxAAMass();
	double maxNtermMass = m_cPTMIndex->GetMaxNtermVarMass();
	double maxCtermMass = m_cPTMIndex->GetMaxCtermVarMass();
	if (deltaMass < minAAMass)   return;
	vector<TAGRES> &vTagInfo = candiPro.vTagInfo;
	sort(vTagInfo.begin(), vTagInfo.end(), TAGRES::ProPosLess);  // sort by the pos in the sequence
	int NC = min(vTagInfo.size(), CHECK_TAG_NUM);
	for (int i = 0; i < NC; ++i){
		string Seq = pro.strProSQ;
		TAGRES leftTag = vTagInfo[i];
		TAGRES rightTag = vTagInfo[(int)vTagInfo.size() - 1 - i];

		string originalSeq = Seq;
		for (int k = 0; k < 1; ++k) {
			if (k == 1) {
				Seq = originalSeq;
				swap(leftTag, rightTag);
			}
			string leftSeq = Seq.substr(0, leftTag.nPos);
			string rightSeq = Seq.substr(rightTag.nPos + rightTag.strTag.length());
			double leftMass = m_mapAAMass->CalculateNeutralMass(leftSeq);
			double rightMass = m_mapAAMass->CalculateNeutralMass(rightSeq);
			double LeftDelta = leftMass - leftTag.lfFlankingMass[0] + maxNtermMass;
			double rightDelta = rightMass - rightTag.lfFlankingMass[1] + maxCtermMass + IonMass_H2O;


			if (LeftDelta >= minAAMass) {
				int minnum = LeftDelta / maxAAMass;
				int maxnum = LeftDelta / minAAMass;
				double aamass = m_mapAAMass->CalculateNeutralMass(Seq.substr(0, minnum));
				for (int j = minnum; j < maxnum && j < Seq.length(); ++j) {
					if (LeftDelta - aamass < maxNtermMass) {
						Seq = Seq.substr(j);
						double proMass = m_mapAAMass->CalculateMass(Seq.c_str());
						deltaMass = proMass - m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass;
						if (Seq.length() >= 6 && vCandiProSeq.find(Seq) == vCandiProSeq.end()) {
							if (vTagInfo.size() < 3) {  // low confidence 
								if (deltaMass <= m_lfPrecurMass && (-deltaMass) <= MAX_MOD_MASS) {
									vCandiProSeq.insert(make_pair(Seq, proMass));
								}
							}
							else {
								if (deltaMass <= lfMaxNegModMass && (-deltaMass) <= TOTAL_MASS_SHIFT) {
									vCandiProSeq.insert(make_pair(Seq, proMass));
								}
							}
						}
						break;
					}
					aamass += m_mapAAMass->GetAAMass(Seq[j]);
				}
			}
			if (rightDelta >= minAAMass) {  // there are amino acids at right being cut
				int minnum = rightDelta / maxAAMass;
				int maxnum = rightDelta / minAAMass;
				if (minnum >= Seq.length())   continue;
				double aamass = m_mapAAMass->CalculateNeutralMass(Seq.substr(Seq.length() - minnum));
				int seqlen = Seq.length();
				for (int j = seqlen - minnum - 1; j >= 0 && j >= seqlen - maxnum; --j) {
					if (rightDelta - aamass < maxCtermMass + IonMass_H2O) {
						Seq = Seq.substr(0, j + 1);
						double proMass = m_mapAAMass->CalculateMass(Seq.c_str());
						deltaMass = proMass - m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass;
						if (Seq.length() >= 6 && vCandiProSeq.find(Seq) == vCandiProSeq.end()) {
							if (vTagInfo.size() < 3) {  // low confidence
								if (deltaMass <= m_lfPrecurMass && (-deltaMass) <= MAX_MOD_MASS) {
									vCandiProSeq.insert(make_pair(Seq, proMass));
								}
							}
							else {
								if (deltaMass <= lfMaxNegModMass && (-deltaMass) <= TOTAL_MASS_SHIFT) {
									vCandiProSeq.insert(make_pair(Seq, proMass));
								}
							}
						}
						break;
					}
					aamass += m_mapAAMass->GetAAMass(Seq[j]);
				}
			}
		}
		
	}
	// 如果该蛋白提取的Tag数目大于k却找不到合适的候选序列，很可能是Tag的两翼质量有偏差; 为了弥补这部分，此处只检测其一端剪切
	//if (vCandiProSeq.empty() && vTagInfo.size() > 6){
	//	if (pro.lfMass - m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass >= minAAMass){
	//		_CheckTerminal(true, pro.strProSQ, pro.lfMass, m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass, vCandiProSeq);
	//		_CheckTerminal(false, pro.strProSQ, pro.lfMass, m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass, vCandiProSeq);
	//	}
	//	if (pro.strProSQ[0] == 'M'){
	//		string seq = pro.strProSQ.substr(1);
	//		double proMass = pro.lfMass - AAMass_M_mono;
	//		if (proMass - m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass >= minAAMass){
	//			_CheckTerminal(false, seq, proMass, m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass, vCandiProSeq);
	//		}
	//	}
	//}
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
			double tmpVol = 0.5 + cos( fabs(massVol[j]) / m_cParam->m_lfFragTol * pi / 2);
			score += TF * tmpVol;
		}
    }

    return score;
}

double CSearchEngine::_BM25Score(int type, double shiftPPM, PROTEIN_SPECTRA_MATCH &prsm)
{

	const double BM25_K1 = 0.03;
	double score = 0.0;

	m_vMatchedPeak.clear();
	m_vMatchedPeak.assign(m_Spectra->vPeaksTbl.size(), 0);

	vector<double> sumIntens, massVol;
	size_t len = m_theoNFragIon.size();
	double nmatched_intensity = 0.0, cmatched_intensity = 0.0, tmpSumIntens = 0.0;
	int nterm_matched_ions = 0;
	int cterm_matched_ions = 0;
	int com_ions = 0;
	int max_tag_len = 0, tmpn = 0, tmpc = 0;
	vector<double> fragment_tols;
	vector<int> nflag(len, 0), cflag(len, 0);

    for(size_t i = 0; i < len; ++i)
    {
		sumIntens.clear();
		massVol.clear();
		int nterm = _LookupHashTbl(type, shiftPPM, i, true, m_theoNFragIon[i], sumIntens, massVol, tmpSumIntens);
		nterm_matched_ions += nterm, nmatched_intensity += tmpSumIntens;
		if (nterm){
			++tmpn;
			nflag[i] = 1;
		}
		else{
			max_tag_len = max(max_tag_len, tmpn);
			tmpn = 0;
		}
		int cterm = _LookupHashTbl(type, shiftPPM, len - i - 1, false, m_theoCFragIon[len - i - 1], sumIntens, massVol, tmpSumIntens);
		cterm_matched_ions += cterm, cmatched_intensity += tmpSumIntens;
		if (cterm){
			++tmpc;
			cflag[len - i - 1] = 1;
			if (nterm > 0)  ++com_ions; 
		}
		else{
			max_tag_len = max(max_tag_len, tmpc);
			tmpc = 0;
		}
		for(size_t j = 0; j < sumIntens.size(); ++j)
		{
			double TF = (BM25_K1 + 1) * (sumIntens[j]) / (BM25_K1 + sumIntens[j]);
			double tmpVol = 0.5 + cos( fabs(massVol[j]) / m_cParam->m_lfFragTol * pi / 2);
			score += TF * tmpVol;
			fragment_tols.push_back(massVol[j]);
		}
    }
	for (int i = len - 2; i >= 0; --i){
		nflag[i] += nflag[i + 1];
		cflag[i] += cflag[i + 1];
	}
	double ptm_score = 1.0;
	if (!prsm.vModSites.empty()){
		ptm_score = 0;
		for (int i = 0; i<prsm.vModSites.size(); ++i){
			if (0 == prsm.vModSites[i].nSecond || 1 == prsm.vModSites[i].nSecond) // N端可变修饰   
			{ // N-term modification
				ptm_score += nflag[0] * 1.0 / (1 + nterm_matched_ions);
			}
			else if ((len == (int)prsm.vModSites[i].nSecond - 2) || (len == (int)prsm.vModSites[i].nSecond - 1)){ // C-term var modification  [wrm?]修饰位点为什么是 nSecond -1
				ptm_score += cflag[0] * 1.0 / (1 + cterm_matched_ions);
			}
			else {
				int pos = (int)prsm.vModSites[i].nSecond - 1;
				ptm_score += (nflag[pos] + cflag[len - pos] )* 1.0 / (1 + nterm_matched_ions + cterm_matched_ions);
			}
		}
		ptm_score /= prsm.vModSites.size();
	}
	max_tag_len = max(max_tag_len, tmpn);
	max_tag_len = max(max_tag_len, tmpc);
	prsm.matchedInfo.lfNtermMatchedIntensityRatio = nmatched_intensity / totalIntensity;
	prsm.matchedInfo.lfCtermMatchedIntensityRatio = cmatched_intensity / totalIntensity;
	prsm.matchedInfo.nNterm_matched_ions = nterm_matched_ions;
	prsm.matchedInfo.nCterm_matched_ions = cterm_matched_ions;
	prsm.featureInfo.lfCom_ions_ratio = 2.0*com_ions / (nterm_matched_ions + cterm_matched_ions);
	prsm.featureInfo.lfTag_ratio = max_tag_len*1.0 / len;
	prsm.featureInfo.lfPTM_score = ptm_score;
	prsm.featureInfo.lfFragment_error_std = CalculateSTD(fragment_tols);
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

void CSearchEngine::_RefinedScore()
{
	//cout<<"RefineScore"<<endl;
    int actStatus = 1; 
	baseline = _GetBaselineByHistogram(1.25);
	//baseline = _GetBaselineByThreshold(0.02);
    for(int prsm = 0; prsm < m_Spectra->vPrecursors[m_nPrecurID].nValidCandidateProNum; ++prsm)
    {
		// wzz temp
		try {

			double temp = m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[prsm].lfMass - 55.0;
			//cout << temp << "???" << endl;
		}
		catch (exception e) {
			throw e;
		}
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

		//cout << score << " " << m_Scoefficient[1] << " " << m_Scoefficient[0] << " " << exp(logs) << endl;
        m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[prsm].lfScore = score;

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
// MGF和PF文件的谱峰都是按照mz有序的
void CSearchEngine::_CreateHashTbl()
{
	m_hashTbl.assign(m_Spectra->vPeaksTbl.back().lfMz+10, 0);
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


void CSearchEngine::_GetComplementaryIons(const bool isNterm, const vector<double> &vIonMasses, vector<double> &vComIons)
{
	vComIons.clear();
	int i = 0;
	if (m_cParam->calAX){
		vComIons.push_back(m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass + IonMass_Proton - 2*IonMass_Mono_H - vIonMasses[i++]);
	}
	if (m_cParam->calBY){
		vComIons.push_back(m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass + IonMass_Proton - vIonMasses[i++]);
	}
	if (m_cParam->calCZ){
		vComIons.push_back(m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass + IonMass_Proton + IonMass_Mono_H - vIonMasses[i++]);
	}
}

void CSearchEngine::_GetComplementaryPeaks(vector<FRAGMENT_ION> &vtPeaks, vector<FRAGMENT_ION> &vtComPeaks)
{
	vtComPeaks.clear();
	int peakNum = vtPeaks.size();
	if (m_cParam->calAX){
		for (int i = 0; i < peakNum; ++i){
			vtComPeaks.push_back(FRAGMENT_ION(m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass + IonMass_Proton - 2 * IonMass_Mono_H - vtPeaks[i].lfMz, vtPeaks[i].lfIntens));
		}
	}
	if (m_cParam->calBY){
		for (int i = 0; i < peakNum; ++i){
			vtComPeaks.push_back(FRAGMENT_ION(m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass + IonMass_Proton - vtPeaks[i].lfMz, vtPeaks[i].lfIntens));
		}
	}
	if (m_cParam->calCZ){
		for (int i = 0; i < peakNum; ++i){
			vtComPeaks.push_back(FRAGMENT_ION(m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass + IonMass_Proton + IonMass_Mono_H - vtPeaks[i].lfMz, vtPeaks[i].lfIntens));
		}
	}
	
}

void CSearchEngine::_CoarseScore(vector<FRAGMENT_ION> &peaks, vector<int> &vHashTable, 
	unordered_map<int, unordered_map<string, double>> &candiPros, vector<CANDI_PROTEIN> &filteredPros)
{
	if (candiPros.empty() || peaks.empty())  return;
	vector<UINT_UINT> emptySite;
	double score = 0;
	for (auto it = candiPros.begin(); it != candiPros.end(); ++it){
		for (auto pt = it->second.begin(); pt != it->second.end(); ++pt){
			_GetTheoryFragIons(pt->first, 0, emptySite);
			score = _BM25Score(pt->first, pt->second, peaks, vHashTable, MZ_SCALE);
			CANDI_PROTEIN candi = CANDI_PROTEIN(it->first, pt->first, pt->second, score);
			filteredPros.push_back(candi);
		}
	}
	sort(filteredPros.begin(), filteredPros.end(), CANDI_PROTEIN::LARGER);
#ifdef _DEBUG_SEARCH
	for (int i = 0; i < filteredPros.size(); ++i){
		fprintf(fp, "%s.%d.%d.dta %s %d %f\n", m_Spectra->strSpecTitle.c_str(), m_Spectra->vPrecursors[m_nPrecurID].nPrecursorCharge, m_nPrecurID, filteredPros[i].strSeq.c_str(), i + 1, filteredPros[i].lfScore);
	}
#endif

	vector<CANDI_PROTEIN>::iterator it = filteredPros.begin();
	// wzz fr 10 to 5
	double Coarse_Score_Threshold = 10.0;  // TODO?
	int Count_Above_Threshold = 0;
	for (it = filteredPros.begin(); it != filteredPros.end(); ++it){
		if (it->lfScore < Coarse_Score_Threshold){
			break;
		}
		++Count_Above_Threshold;
	}
	if (Count_Above_Threshold < 10){  // 当score大于10的候选小于10时，则取前10名
		// modified by wzz size fr 10 to 20
		if (filteredPros.size() > 15){
			filteredPros.erase(filteredPros.begin() + 15, filteredPros.end());
		}
	}
	else{
		filteredPros.erase(it, filteredPros.end());
		if (filteredPros.size() > MAX_CANDIPRO_NUM){
			filteredPros.erase(filteredPros.begin() + MAX_CANDIPRO_NUM, filteredPros.end());
		}
	}
}

double CSearchEngine::_BM25Score(const string &seq, const double lfMass, vector<FRAGMENT_ION> &peaks, vector<int> &vHashTable, const int FACTOR)
{
	if (peaks.empty())  return 0;
	const double BM25_K1 = 0.05, BM25_K11 = 0.03, b = 0.75;
	double score = 0.0;
	double ftol = 0.05;
	double lfPrecurTol = m_cParam->m_lfPrecurTol;
	double lfFragTol = m_cParam->m_lfFragTol;
	vector<double> sumIntens, massVol;
	vector<double> vComIons;
	size_t len = m_theoNFragIon.size();
	vector<int> &vNMods = m_cPTMIndex->GetNtermMods();
	vector<int> &vCMods = m_cPTMIndex->GetCtermMods();
	for (size_t i = 0; i < len; ++i)
	{
		sumIntens.clear();
		massVol.clear();
		vector<double> nionMass = m_theoNFragIon[i];
		for (int mi = 0; mi < vNMods.size(); ++mi){
			for (int t = 0; t < m_theoNFragIon[i].size(); ++t){
				nionMass.push_back(m_cPTMIndex->GetMassbyID(vNMods[mi]) + m_theoNFragIon[i][t]);
			}
		}
		vector<double> cionMass = m_theoCFragIon[len - i - 1];
		for (int mi = 0; mi < vCMods.size(); ++mi){
			for (int t = 0; t < m_theoCFragIon[len - i - 1].size(); ++t){
				cionMass.push_back(m_cPTMIndex->GetMassbyID(vCMods[mi]) + m_theoCFragIon[len - i - 1][t]);
			}
		}
		ftol = lfFragTol*m_theoNFragIon[i][0] / 1000000.0;
		int nterm = _LookupHashTbl(1, ftol, peaks, vHashTable, nionMass, sumIntens, massVol, FACTOR);
		ftol = lfFragTol*m_theoCFragIon[len - i - 1][0] / 1000000.0;
		int cterm = _LookupHashTbl(1, ftol, peaks, vHashTable, cionMass, sumIntens, massVol, FACTOR);

		for (size_t j = 0; j < sumIntens.size(); ++j)
		{
			double TF = (BM25_K1 + 1) * (sumIntens[j]) / (BM25_K1 + sumIntens[j]);
			double tmpVol = 0.5 + cos(massVol[j] * pi / 2);
			score += TF * tmpVol;
		}

		sumIntens.clear();
		massVol.clear();
		if (nterm == 0){
			_GetComplementaryIons(false, m_theoCFragIon[len - i - 1], vComIons);
			ftol = lfFragTol*vComIons[0] / 1000000.0 + lfPrecurTol;
			nterm = _LookupHashTbl(1, ftol, peaks, vHashTable, vComIons, sumIntens, massVol, FACTOR);
		}
		//if (cterm == 0){
		//	_GetComplementaryIons(true, m_theoNFragIon[i], vComIons);
		//	ftol = m_lfFragTol*vComIons[0] / 1000000.0 + m_lfPrecurTol;
		//	cterm = _LookupHashTbl(1, ftol, peaks, vHashTable, vComIons, sumIntens, massVol, FACTOR);
		//}
		for (size_t j = 0; j < sumIntens.size(); ++j)
		{
			double normLen = seq.length() / (lfMass / AVG_AA_MASS);
			double TF = (sumIntens[j])*(BM25_K11 + 1) / (sumIntens[j] + BM25_K11*(1 - b + b*normLen));
			score += TF;
		}
	}
	return score;
}


// Note: ftol (Da)
int CSearchEngine::_LookupHashTbl(int type, double ftol, vector<FRAGMENT_ION> &peaks, vector<int> &vHashTable, vector<double> &ionMass, vector<double> &matchedInts, vector<double> &massVol, int FACTOR)
{
	if (peaks.empty())  return 0;
	int nIonPos, peakNum = peaks.size();
	int minpeakMass = (int)(peaks[0].lfMz*FACTOR);
	int maxpeakMass = (int)(peaks[peakNum - 1].lfMz*FACTOR);
	int matchedIons = 0;
	vector<double> smallWnd;
	smallWnd.push_back(0.0);
	if (1 == type)
	{
		smallWnd.push_back(-DIFF_13C);
		smallWnd.push_back(DIFF_13C);
	}

	for (size_t j = 0; j < ionMass.size(); ++j)
	{
		for (size_t m = 0; m < smallWnd.size(); ++m) // -1与0两个小窗口是否会发生重叠？质量超过25000时会
		{
			double lfMass = ionMass[j] + smallWnd[m];			
			double lowerBound = lfMass - ftol;
			double upperBound = lfMass + ftol;
			int tmpMass = (int)(lowerBound*FACTOR);

			if (tmpMass > maxpeakMass || tmpMass < minpeakMass)
			{   // 理论碎片离子质量在所有实验谱峰的最小值和最大值之间才处理
				continue;
			}

			nIonPos = vHashTable[tmpMass];
			if (nIonPos < 0)
			{
				continue;
			}

			while (nIonPos < peakNum && peaks[nIonPos].lfMz < lowerBound)
			{
				++nIonPos;
			}
			if (nIonPos < peakNum && peaks[nIonPos].lfMz <= upperBound)
			{
				++matchedIons;
				matchedInts.push_back(peaks[nIonPos].lfIntens);
				double tmpTol = 1;
				if (0 != m) // 对偏离±1Da的进行惩罚
				{
					tmpTol = min(1, 0.2 + fabs(peaks[nIonPos].lfMz - lfMass) / ftol);
				}
				else 
				{
					tmpTol = fabs(peaks[nIonPos].lfMz - lfMass) / ftol;
				}
				massVol.push_back(tmpTol);
				break;
			}
		}
		if (matchedIons)   break;
	}
	return matchedIons;
}

void CSearchEngine::_GetTopPeak(vector<FRAGMENT_ION> &vtPeaks, const int topk)
{
	vtPeaks.clear();
	double lfMaxIntensity = 0;
	for (int pIdx = 0; pIdx < m_Spectra->nPeaksNum; ++pIdx)
	{
		vtPeaks.push_back(m_Spectra->vPeaksTbl[pIdx]);
		lfMaxIntensity = max(lfMaxIntensity, m_Spectra->vPeaksTbl[pIdx].lfIntens);
	}

	sort(vtPeaks.begin(), vtPeaks.end(), FRAGMENT_ION::PeakMzIncCmp);
	double maxPrecur = 0;
	for (int i = 0; i < m_Spectra->vPrecursors.size(); ++i){
		maxPrecur = max(maxPrecur, m_Spectra->vPrecursors[i].lfPrecursorMass);
	}
	int p = vtPeaks.size() - 1;
	for (; p >= 0; --p){
		if (vtPeaks[p].lfMz < maxPrecur - 50.0){
			break;
		}
	}
	vtPeaks.erase(vtPeaks.begin() + (p + 1), vtPeaks.end());

	if (0 == vtPeaks.size())
	{
		return;
	}

	if (vtPeaks.size() > topk)
	{
		sort(vtPeaks.begin(), vtPeaks.end(), FRAGMENT_ION::PeakIntDesCmp);
		vtPeaks.erase(vtPeaks.begin() + topk, vtPeaks.end());
	}
	sort(vtPeaks.begin(), vtPeaks.end(), FRAGMENT_ION::PeakMzIncCmp);
	int peakNum = vtPeaks.size();
	//  normalize the intensity
	for (int i = 0; i < peakNum; ++i){
		vtPeaks[i].lfIntens = vtPeaks[i].lfIntens / lfMaxIntensity;
	}
	// merge close peaks
	m_cTag->MergeClosePeak(vtPeaks, peakNum);
}


// tag flow
void CSearchEngine::Search()
{

	if(m_Spectra->nPeaksNum <= 0 || m_Spectra->nPrecurNum == 0) 
	{
		return;
	}

	try{
		// 创建质量谱峰哈希表
		_CreateHashTbl();

		vector<PROID_NODE> candiProID;
		vector<FRAGMENT_ION> peaks;
		vector<int> vHashTable(MAX_HASH_SIZE*MZ_SCALE + 1);
		//cout<<"[pTop] Getting the tags..."<<endl;
		m_cTag->GeneTag(m_Spectra, m_cParam->m_bTagLenInc);
		m_cTag->GetTopPeaks(peaks);
		m_cTag->GetSpectraMassHash(vHashTable, MZ_SCALE);
	
		for (m_nPrecurID = 0; m_nPrecurID < m_Spectra->nPrecurNum; ++m_nPrecurID)
		{
			
			if (m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass <= 0.0) continue;

			m_cTag->Run(m_nPrecurTol, m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass, m_cTagFlow, m_cProIndex->m_ProteinList, candiProID);

			unordered_map<int, unordered_map<string, double>> candiPros;
			vector<CANDI_PROTEIN> validCandi, newCandi;
			_CheckCandiPros(candiProID, candiPros, validCandi);


			if ((validCandi.size() + candiPros.size()) < MAX_CANDIPRO_NUM){ // (validCandi.size() > 0 || candiPros.size() > 0) && 
				_GetCandidatesInWindow(m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass, 500, candiPros);
			}
			else{
				_GetCandidatesInWindow(m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass, 200, candiPros);
			}

			_CoarseScore(peaks, vHashTable, candiPros, newCandi);
			validCandi.insert(validCandi.end(), newCandi.begin(), newCandi.end());

#ifdef _DEBUG_SEARCH
			//fprintf(fp2, "%d %d\n", validCandi.size(), validCandi.size() - newCandi.size());
			//if (m_nPrecurID != 1)  continue;
			/*for (auto i : validCandi) {
			
				cout << i.nProID << " " << i.strSeq << endl;
			}*/
#endif
			if (m_Spectra->vPrecursors[m_nPrecurID].nValidCandidateProNum == 0){
				m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs.clear();
				m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs.assign(TOPSCORE, PROTEIN_SPECTRA_MATCH());
			}
			//cout << "sketch" << endl;
			SketchScore(validCandi);

			// 细打分
			if (m_Spectra->vPrecursors[m_nPrecurID].nValidCandidateProNum > 0)
			{
				_RefinedScore();   // 细打分
			}
			if (m_cParam->m_nUnexpectedModNum > 0){   // search with unexpected modifications
				if (m_Spectra->vPrecursors[m_nPrecurID].nValidCandidateProNum == 0){
					_MatchCandidateWithPTM(validCandi);
					_RefinedScore();
				}
				else{
					sort(m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs.begin(), m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs.begin() + m_Spectra->vPrecursors[m_nPrecurID].nValidCandidateProNum, PROTEIN_SPECTRA_MATCH::CmpByScoreDes);
					if (m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[0].lfScore < 18.0){  // TODO: BM25Score threshold
						_MatchCandidateWithPTM(validCandi);
						_RefinedScore();
					}
				}
			}
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

	_CalculateEvalue();  // evalue计算
}


// fragment index flow
void CSearchEngine::IonIndexSearch(FragmentIndex *pFragIndex)
{
	if (m_Spectra->nPeaksNum <= 0 || m_Spectra->nPrecurNum == 0)
	{
		return;
	}
	// 创建质量谱峰哈希表
	_CreateHashTbl();

	vector<FRAGMENT_ION> vtPeaks;
	vector<FRAGMENT_ION> peaks;
	vector<int> vHashTable(MAX_HASH_SIZE*MZ_SCALE+1);
	_GetTopPeak(vtPeaks, 120);  // 100
	m_cTag->PreProcess(m_Spectra);  // for _CoarseScore
	m_cTag->GetTopPeaks(peaks);
	m_cTag->GetSpectraMassHash(vHashTable, MZ_SCALE);

	for (m_nPrecurID = 0; m_nPrecurID < m_Spectra->nPrecurNum; ++m_nPrecurID)
	{
		//cout << m_Spectra->strSpecTitle << "\t" << m_nPrecurID << "\t";
		if (m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass <= 0.0) continue;
		unordered_map<size_t, double> mpProCnt;
		_SearchTerms(pFragIndex, mpProCnt, vtPeaks);

#ifdef _DEBUG_SEARCH
		if (m_nPrecurID != 0)  continue;
		fprintf(fp, "Spectra: %s.%d\n", m_Spectra->strSpecTitle.c_str(), m_nPrecurID);
		printf("Candidates Number: %d\n", mpProCnt.size());
		fprintf(fp, "Candidates Number: %d\n", mpProCnt.size());
		for (auto it = mpProCnt.begin(); it != mpProCnt.end(); ++it){
			fprintf(fp, "%d\t%s\t%f\n", it->first, m_cProIndex->m_ProteinList[it->first].strProSQ.c_str(), (double)it->second);
		}
#endif
		unordered_map<int, unordered_map<string, double>> candiPros;
		_GetValidCandidates(m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass, mpProCnt, candiPros);
#ifdef _DEBUG_SEARCH
		fprintf(fp2, "%d\n", candiPros.size());
#endif
		vector<CANDI_PROTEIN> validCandi;
		_CoarseScore(peaks, vHashTable, candiPros, validCandi);

		if (m_Spectra->vPrecursors[m_nPrecurID].nValidCandidateProNum == 0){
			m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs.clear();
			m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs.assign(TOPSCORE, PROTEIN_SPECTRA_MATCH());
		}
		//cout << "sketch" << endl;
		SketchScore(validCandi);

		// 细打分
		if (m_Spectra->vPrecursors[m_nPrecurID].nValidCandidateProNum > 0)
		{
			_RefinedScore();   // 细打分
		}
		if (m_cParam->m_nUnexpectedModNum > 0){   // search with unexpected modifications
			if (m_Spectra->vPrecursors[m_nPrecurID].nValidCandidateProNum == 0){
				_MatchCandidateWithPTM(validCandi);
				_RefinedScore();
			}
			else{
				sort(m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs.begin(), m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs.begin() + m_Spectra->vPrecursors[m_nPrecurID].nValidCandidateProNum, PROTEIN_SPECTRA_MATCH::CmpByScoreDes);
				if (m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[0].lfScore < 18.0){  // TODO: BM25Score threshold
					_MatchCandidateWithPTM(validCandi);
					_RefinedScore();
				}
			}
		}
	}
	_CalculateEvalue();  // evalue计算

}

void CSearchEngine::_SearchTerms(FragmentIndex *pFragIndex, unordered_map<size_t, double> &mpProCnt, vector<FRAGMENT_ION> &vtPeaks)
{
	for (int i = 0; i < vtPeaks.size(); ++i){
		// b\y-ions
		if (!pFragIndex->SearchTerms(mpProCnt, vtPeaks[i])){
			// complementary ions
			FRAGMENT_ION comPeak(m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass + IonMass_Proton - vtPeaks[i].lfMz, vtPeaks[i].lfIntens);
			pFragIndex->SearchTerms(mpProCnt, comPeak);
		}	
	}
	//for (int i = 0; i < vtPeaks.size(); ++i){
	//	// b\y-ions
	//	pFragIndex->SearchTerms(mpProCnt, vtPeaks[i]);
	//	// complementary ions
	//	FRAGMENT_ION comPeak(m_Spectra->vPrecursors[m_nPrecurID].lfPrecursorMass + IonMass_Proton - vtPeaks[i].lfMz, vtPeaks[i].lfIntens);
	//	pFragIndex->SearchTerms(mpProCnt, comPeak);
	//}
}

void CSearchEngine::_CalculateEvalue()
{
	if (m_Spectra->nPrecurNum == 0 || m_Spectra->vPrecursors[0].lfPrecursorMass <= 0.0)  return;

	if (m_Spectra->nPeaksNum > 200){

		m_Scoefficient[0] = 0.0023 * m_Spectra->nPeaksNum + 1.115;
		m_Scoefficient[1] = -0.0001 * m_Spectra->nPeaksNum - 0.9222;
	}
	else {
		int maxRandomPnum = 5000;
		srand((unsigned int)(1000));   // 伪随机数种子为1000，则每次产生的随机数序列都是相同的
		vector<double> vRandScore;
		for (int i = 0; i < maxRandomPnum; ++i)
		{
			double randscore = _GetRandomProteoformIons(m_Spectra->vPrecursors[0].lfPrecursorMass);
			if (randscore > 0)
			{
				vRandScore.push_back(randscore);
			}
			//cout << randscore << endl;
		}
		_FitHighScores(vRandScore);
		//cout << m_Spectra->nPeaksNum << " " << m_Scoefficient[0] << " " << m_Scoefficient[1] << endl;
		//cout << "refine" << endl;
	}
	for (m_nPrecurID = 0; m_nPrecurID < m_Spectra->nPrecurNum; ++m_nPrecurID)
	{
		for (int prsm = 0; prsm < m_Spectra->vPrecursors[m_nPrecurID].nValidCandidateProNum; ++prsm)
		{
			double score = m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[prsm].lfScore;
			double logs = m_Scoefficient[1] * score + m_Scoefficient[0];
			if (score < DOUCOM_EPS)
			{
				m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[prsm].lfQvalue = 1.0;
			}
			else 
			{
				m_Spectra->vPrecursors[m_nPrecurID].aBestPrSMs[prsm].lfQvalue = exp(logs);
			}
		}
	}
}


void CSearchEngine::_GetValidCandidates(double precurMass, unordered_map<size_t, double> &mpProCnt, unordered_map<int, unordered_map<string, double>> &candiPros)
{
	const vector<PROTEIN_STRUCT> &proList = m_cProIndex->m_ProteinList;
	double minAAMass = m_cTag->GetMinAAMass();
	priority_queue<CANDI_PROTEIN, vector<CANDI_PROTEIN>, greater<CANDI_PROTEIN> > que;
	for (auto it = mpProCnt.begin(); it != mpProCnt.end(); ++it){
		if (it->second >= 2){  // at least two matched peaks
			if (que.size() < MAX_CANDIPRO_NUM){
				que.push(CANDI_PROTEIN(it->first, proList[it->first].strProSQ, proList[it->first].lfMass, it->second));
			}
			else{
				if (it->second > que.top().lfScore){
					que.pop();
					que.push(CANDI_PROTEIN(it->first, proList[it->first].strProSQ, proList[it->first].lfMass, it->second));
				}
			}
		}
#ifdef _DEBUG_SEARCH
		if (proList[it->first].strProSQ.find("SDAGRKGFGEKASEALKPDSQK") != string::npos){
			cout << it->second << endl;
		}
#endif
	}
	while (!que.empty()){  //  proIDs are different in the que
		CANDI_PROTEIN &pro = que.top();
		unordered_map<string, double> seq_mass;
		string seq = pro.strSeq;
		if (pro.lfMass - precurMass >= minAAMass){
			_CheckTerminal(true, seq, pro.lfMass, precurMass, seq_mass);
			_CheckTerminal(false, seq, pro.lfMass, precurMass, seq_mass);
		}
		else{
			seq_mass.insert(make_pair(seq, pro.lfMass));
		}
		if (pro.strSeq[0] == 'M'){
			seq = seq.substr(1);
			double proMass = pro.lfMass - AAMass_M_mono;
			if (proMass - precurMass >= minAAMass){
				_CheckTerminal(false, seq, proMass, precurMass, seq_mass);
			}
			else{
				seq_mass.insert(make_pair(seq, proMass));
			}
		}
		candiPros.insert(make_pair(pro.nProID, seq_mass));
		que.pop();
	}
}

// can only handle truncated in one end
void CSearchEngine::_CheckTerminal(const bool isNterm, const string &seq, double proMass, double precurMass, unordered_map<string, double> &proSeqs)
{
	const double tolerance = 5.2;
	const double maxAAMass = 186.1;
	int minnum = int((proMass - precurMass) / maxAAMass);
	if (isNterm){
		proMass -= m_mapAAMass->CalculateNeutralMass(seq.substr(0, minnum));
		for (int i = minnum; i < seq.length(); ++i){
			proMass -= m_mapAAMass->GetAAMass(seq[i]);
			if (proMass - precurMass < tolerance){
				proSeqs[seq.substr(i + 1)] = proMass;
				break;
			}
		}
	}
	else{
		proMass -= m_mapAAMass->CalculateNeutralMass(seq.substr(seq.length() - minnum));
		for (int i = seq.length() - 1 - minnum; i >= 0; --i){
			proMass -= m_mapAAMass->GetAAMass(seq[i]);
			if (proMass - precurMass < tolerance){
				proSeqs[seq.substr(0, i)] = proMass;
				break;
			}
		}

	}
}

void CSearchEngine::_GetProteinsInWindow(double precurMass, double halfWindow, vector<DOUBLE_INT> &pro_delta)
{
	pro_delta.clear();
	int left = max(0, int(precurMass - halfWindow));
	int right = min(int(precurMass + halfWindow) + 1, MAX_HASH_SIZE);
	for (int mi = m_cProIndex->m_vMassCnt[left]; mi < m_cProIndex->m_vMassCnt[right]; ++mi){
		DOUBLE_INT tmp;
		tmp.nb = mi;
		tmp.lfa = fabs(precurMass - m_cProIndex->m_ProteinList[tmp.nb].lfMass);
		pro_delta.push_back(tmp);
	}
}

void CSearchEngine::_GetCandidatesInWindow(double precurMass, double halfWindow, unordered_map<int, unordered_map<string, double>> &candiPros)
{
	//int halfWindow = 500;
	const vector<PROTEIN_STRUCT> &proList = m_cProIndex->m_ProteinList;
	double minAAMass = m_cTag->GetMinAAMass();
	vector<DOUBLE_INT> pro_delta;
	_GetProteinsInWindow(precurMass, halfWindow, pro_delta);
	int proNum = pro_delta.size();
	if (pro_delta.size() > 2 * MAX_CANDIPRO_NUM){
		sort(pro_delta.begin(), pro_delta.end());  // order by double
		proNum = 2 * MAX_CANDIPRO_NUM;
	}
	int tagCandiNum = candiPros.size();
	for(int i=0; i<proNum; ++i){		
		unordered_map<string, double> proSeqs;
		double proMass = proList[pro_delta[i].nb].lfMass;
		string seq = proList[pro_delta[i].nb].strProSQ;
#ifdef _DEBUG_SEARCH
		if (proList[pro_delta[i].nb].strProAC == "sp|P50263|SIP18_YEAST"){
			cout << proList[pro_delta[i].nb].strProAC << endl;
		}
#endif
		if (proMass - precurMass >= minAAMass){
			_CheckTerminal(true, seq, proMass, precurMass, proSeqs);
			_CheckTerminal(false, seq, proMass, precurMass, proSeqs);
		}
		else{
			proSeqs.insert(make_pair(seq, proMass));
		}
		if (proList[pro_delta[i].nb].strProSQ[0] == 'M'){
			seq = proList[pro_delta[i].nb].strProSQ.substr(1);
			proMass = proMass - AAMass_M_mono;
			if (proMass - precurMass >= minAAMass){
				_CheckTerminal(false, seq, proMass, precurMass, proSeqs);
			}
			else{
				proSeqs[seq] = proMass;
			}
		}
		if (candiPros.find(pro_delta[i].nb) == candiPros.end()){
			candiPros.insert(make_pair(pro_delta[i].nb, proSeqs));
		}
		else{
			for (auto it = proSeqs.begin(); it != proSeqs.end(); ++it){
				if (candiPros[pro_delta[i].nb].find(it->first) == candiPros[pro_delta[i].nb].end()){
					candiPros[pro_delta[i].nb].insert(*it);
				}
			}
		}
	}
}


void CSearchEngine::SecondSearch()
{

	if(m_Spectra->nPeaksNum <= 0 || m_Spectra->nPrecurNum == 0) 
	{
		return;
	}

	try
	{
		const vector<PROTEIN_STRUCT> &proList = m_cProIndex->m_ProteinList;
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
		throw runtime_error(info.Get(e));
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
			_RefinedScore();
		}
	}
}

/**
*	modification location
*	param: modification set, protein sequence, ...
*   by wrm @2016.12.28
**/
void CSearchEngine::_ModificationLocation(vector<int> &modForm, const string &proSeq, int start, int end, priority_queue<PATH_NODE> &bestPaths, double totalModMass)
{
	// assume there are no more than 64 mods
	int modnum = modForm.size();
	string mark = hashCode(modForm);
	unordered_map<int, int> vModCnt;
	unordered_map<int, int> modLastSite;  // last mod site of each mod	
	unordered_map<int, int> modFirstSite; // first mod site of each mod	
	for (int i = 0; i < modnum; ++i){
		++vModCnt[modForm[i]];
		modLastSite[modForm[i]] = proSeq.length();
		modFirstSite[modForm[i]] = 0;
	}
	unordered_set<int> aaMod[26];
	// bidirectional bfs
	unordered_map<string, VERTICE> leftDist, rightDist;
	// init left set and right set
	vector<UINT_UINT> modSites;
	unordered_map<int, int> modCount;
	priority_queue<PATH_NODE> vPathNodes;   // save the top 10 paths to this node 
	PATH_NODE pathNode(0, modSites);
	vPathNodes.push(pathNode);
	leftDist.insert(make_pair("", VERTICE(modCount, vPathNodes)));
	rightDist.insert(make_pair("", VERTICE(modCount, vPathNodes)));
	// If mod form has N/C-term mod, then add another new Node
	for (int i = 0; i < modnum; ++i)
	{
		int nTermModID = modForm[i];
		priority_queue<PATH_NODE> termNodes;
		unordered_map<int, int> tmpCount;
		if (m_cPTMIndex->isNtermMod(nTermModID)) // N-term mod
		{
			modSites.clear();
			UINT_UINT tmpModSite(nTermModID, 0); // modID->site
			modSites.push_back(tmpModSite);
			pathNode.vModSites = modSites;
			pathNode.fModMass = m_cPTMIndex->GetMassbyID(nTermModID);
			termNodes.push(pathNode);
			tmpCount[nTermModID] = 1;
			string modKey = hashCode(tmpCount);
			leftDist.insert(make_pair(modKey, VERTICE(tmpCount,termNodes)));
		}
		else if (m_cPTMIndex->isCtermMod(nTermModID)) { // C-term mod
			modSites.clear();
			UINT_UINT tmpModSite(nTermModID, proSeq.length() + 1);  //[wrm?] site of C-term
			modSites.push_back(tmpModSite);
			pathNode.vModSites = modSites;
			pathNode.fModMass = m_cPTMIndex->GetMassbyID(nTermModID);
			termNodes.push(pathNode);
			tmpCount[nTermModID] = 1;
			string modKey = hashCode(tmpCount);
			rightDist.insert(make_pair(modKey, VERTICE(tmpCount, termNodes)));
		}
		else{
			MODINI &modini = m_cPTMIndex->GetMODINIbyID(nTermModID);
			for (int j = 0; j < modini.strSite.length(); ++j){
				aaMod[modini.strSite[j] - 'A'].insert(nTermModID);
			}
		}
	}
	// last mod site of each mod	
	for (int i = 0; i < proSeq.length(); ++i){
		for (auto j = aaMod[proSeq[i] - 'A'].begin(); j != aaMod[proSeq[i] - 'A'].end(); ++j){
			modLastSite[*j] = i;
		}
	}
	// first mod site of each mod
	for (int i = proSeq.length() - 1; i >= 0; --i){
		for (auto j = aaMod[proSeq[i] - 'A'].begin(); j != aaMod[proSeq[i] - 'A'].end(); ++j){
			modFirstSite[*j] = i;
		}
	}
	// bidirectional bfs
	int leftIdx = start, rightIdx = end;
	while (leftIdx < rightIdx){
		if (leftDist.size() <= rightDist.size()){  // from N-term
			_NextLayer(proSeq, aaMod[proSeq[leftIdx] - 'A'], vModCnt, modLastSite, leftDist, leftIdx, true, totalModMass);
			++leftIdx;
		}
		else{  // from C-term
			_NextLayer(proSeq, aaMod[proSeq[rightIdx] - 'A'], vModCnt, modFirstSite, rightDist, rightIdx, false, totalModMass);
			--rightIdx;
		}
	}
	// combine left and right and get paths
	for (auto lit = leftDist.begin(); lit != leftDist.end(); ++lit)
	{
		for (auto rit = rightDist.begin(); rit != rightDist.end(); ++rit)
		{
			vector<string> lmods, rmods;
			vector<int> vlrmods;
			CStringProcess::Split(lit->first, "#", lmods);
			CStringProcess::Split(rit->first, "#", rmods);
			lmods.insert(lmods.end(), rmods.begin(), rmods.end());
			for (int s = 0; s < lmods.size(); ++s){
				if (lmods[s] == "")  continue;
				vlrmods.push_back(atoi(lmods[s].c_str()));
			}
			string keyStr = hashCode(vlrmods);
			if (keyStr != mark)  continue;
			priority_queue<PATH_NODE> leftque = lit->second.qPath;
			while (!leftque.empty()){
				PATH_NODE leftPath = leftque.top();
				leftque.pop();
				priority_queue<PATH_NODE> rightque = rit->second.qPath;
				while (!rightque.empty()){
					PATH_NODE rightPath = rightque.top();
					rightque.pop();
					int tmpWeight = leftPath.nWeight + rightPath.nWeight;
					if (tmpWeight <= 0)  continue;
					if (bestPaths.size() < TOP_PATH_NUM){
						vector<UINT_UINT> modSites = leftPath.vModSites;
						for (int k = 0; k < rightPath.vModSites.size(); ++k){
							modSites.push_back(rightPath.vModSites[k]);
						}
						bestPaths.push(PATH_NODE(tmpWeight, totalModMass, modSites));
					}
					else if (tmpWeight > bestPaths.top().nWeight){
						vector<UINT_UINT> modSites = leftPath.vModSites;
						for (int k = 0; k < rightPath.vModSites.size(); ++k){
							modSites.push_back(rightPath.vModSites[k]);
						}
						bestPaths.pop();
						bestPaths.push(PATH_NODE(tmpWeight, totalModMass, modSites));
					}
				}
			}
		} // end for rit
	} // end for lit
}

void CSearchEngine::_NextLayer(const string &proSeq, unordered_set<int> &mods, unordered_map<int, int> &modCnt, unordered_map<int, int> &modLastSite,
	unordered_map<string, VERTICE> &curLayer, int idx, bool isNterm, double totalModMass)
{
	int proLen = proSeq.length();
	unordered_map<string, VERTICE> nextLayer;
	int matchedNIons = 0, matchedCIons = 0, addWeight = 0;
	if (isNterm){
		vector<double> ionMassN = m_theoNFragIon[idx];
		vector<double> ionMassC = m_theoCFragIon[proLen - 2 - idx];
		for (auto it = curLayer.begin(); it != curLayer.end(); ++it){
			string modVal = it->first;
			bool canDelete = false;
			for (auto mi = modLastSite.begin(); mi != modLastSite.end(); ++mi){
				if (modCnt[mi->first] < it->second.sModForm[mi->first]){
					canDelete = true;
					break;
				}
				if (mi->second < idx && (modCnt[mi->first] > it->second.sModForm[mi->first])){
					canDelete = true;
					break;
				}
			}
			if (canDelete) continue;
			priority_queue<PATH_NODE> &pathNodes = it->second.qPath;
			// Chech if matching one or more ions when there is no mod on idx
			matchedNIons = matchedNIons = 0;
			_CheckMatched(ionMassN, pathNodes.top().fModMass, matchedNIons); // 获取N端匹配到的离子数目
			_CheckMatched(ionMassC, totalModMass - pathNodes.top().fModMass, matchedCIons);  // 获取C端匹配到的离子数目
			addWeight = matchedNIons + matchedCIons;
			_InsertNode(it->second, nextLayer, modVal, addWeight, pathNodes.top().fModMass, UINT_UINT(INT_MAX, 0));

			// for each mod
			for (auto modi = mods.begin(); modi != mods.end(); ++modi){
				double modMass = pathNodes.top().fModMass + m_cPTMIndex->GetMassbyID(*modi);
				_CheckMatched(ionMassN, modMass, matchedNIons); // 获取N端匹配到的离子数目
				_CheckMatched(ionMassC, totalModMass - modMass, matchedCIons);  // 获取C端匹配到的离子数目
				addWeight = matchedNIons + matchedCIons;
				_InsertNode(it->second, nextLayer, modVal, addWeight, modMass, UINT_UINT(*modi, idx + 1));
			}  // end for mod on idx
		} // end for node in the pre layer
	}
	else{   // C-term
		vector<double> ionMassN = m_theoNFragIon[idx - 1];
		vector<double> ionMassC = m_theoCFragIon[proLen - 1 - idx];
		for (auto it = curLayer.begin(); it != curLayer.end(); ++it){
			string modVal = it->first;
			bool canDelete = false;
			for (auto mi = modLastSite.begin(); mi != modLastSite.end(); ++mi){
				if (modCnt[mi->first] < it->second.sModForm[mi->first]){
					canDelete = true;
					break;
				}
				if (mi->second > idx && (modCnt[mi->first] > it->second.sModForm[mi->first])){
					canDelete = true;
					break;
				}
			}
			if (canDelete) continue;
			priority_queue<PATH_NODE> &pathNodes = it->second.qPath;
			// Chech if matching one or more ions when there is no mod on idx
			matchedNIons = matchedNIons = 0;
			_CheckMatched(ionMassN, totalModMass - pathNodes.top().fModMass, matchedNIons); // 获取N端匹配到的离子数目
			_CheckMatched(ionMassC, pathNodes.top().fModMass, matchedCIons);  // 获取C端匹配到的离子数目
			addWeight = matchedNIons + matchedCIons;
			_InsertNode(it->second, nextLayer, modVal, addWeight, pathNodes.top().fModMass, UINT_UINT(INT_MAX, 0));

			// for each mod
			for (auto modit = mods.begin(); modit != mods.end(); ++modit){
				double modMass = pathNodes.top().fModMass + m_cPTMIndex->GetMassbyID(*modit);
				_CheckMatched(ionMassN, totalModMass - modMass, matchedNIons); // 获取N端匹配到的离子数目
				_CheckMatched(ionMassC, modMass, matchedCIons);  // 获取C端匹配到的离子数目
				addWeight = matchedNIons + matchedCIons;
				_InsertNode(it->second, nextLayer, modVal, addWeight, modMass, UINT_UINT(*modit, idx + 1));

			}  // end for mod on idx
		} // end for node in the pre layer

	}
	curLayer.swap(nextLayer);
}

// 
void CSearchEngine::_InsertNode(const VERTICE &preNodes, unordered_map<string, VERTICE> &nextLayer,
	string preKey, int addWeight, double newMass, UINT_UINT &newSite)
{
	unordered_map<int, int> modCnt = preNodes.sModForm;
	priority_queue<PATH_NODE> tmpNodes = preNodes.qPath;
	string newKey = preKey;
	if (newSite.nSecond > 0){
		++modCnt[newSite.nFirst];
		newKey = hashCode(modCnt);
	}
	while (!tmpNodes.empty())
	{
		PATH_NODE prenode = tmpNodes.top();
		tmpNodes.pop();
		vector<UINT_UINT> modSites = prenode.vModSites;
		if (newSite.nSecond > 0){
			modSites.push_back(newSite);
		}
		PATH_NODE newNode(prenode.nWeight + addWeight, newMass, modSites);
		if (nextLayer.find(newKey) == nextLayer.end()){
			priority_queue<PATH_NODE> vNewNodes;
			vNewNodes.push(newNode);
			nextLayer.insert(make_pair(newKey, VERTICE(modCnt,vNewNodes)));
		}
		else if (nextLayer[newKey].qPath.size() < TOP_PATH_NUM){   // 10
			nextLayer[newKey].qPath.push(newNode);
		}
		else if (nextLayer[newKey].qPath.top().nWeight < newNode.nWeight){
			nextLayer[newKey].qPath.pop();
			nextLayer[newKey].qPath.push(newNode);
		}
	}
}

string CSearchEngine::hashCode(vector<int> &modForm)
{
	sort(modForm.begin(), modForm.end());
	string ans = "";
	for (int i = 0; i < modForm.size(); ++i){
		ans += to_string(modForm[i]) + "#";
	}
	return ans;
}

string CSearchEngine::hashCode(unordered_map<int, int> &modCnt)
{
	vector<int> modForm;
	for (auto it = modCnt.begin(); it != modCnt.end(); ++it){
		modForm.insert(modForm.end(), it->second, it->first);
	}
	sort(modForm.begin(), modForm.end());
	string ans = "";
	for (int i = 0; i < modForm.size(); ++i){
		ans += to_string(modForm[i]) + "#";
	}
	return ans;
}

/**
*	caculate the e-value
**/


// Calculate the theory mass of fragment ions of random proteoforms
double CSearchEngine::_GetRandomProteoformIons(double mass)
{
	vector<double> &m_vAAMass = m_mapAAMass->m_vAAMass;
	int pIdx = 0;
	int nUnbond = m_vAAMass.size();
	double randProMass = 0.0;
	vector<double> nProAAMass;
	while(randProMass < mass)
	{
		int idx = rand() % nUnbond;
		nProAAMass.push_back( m_vAAMass[idx] );
		randProMass += m_vAAMass[idx];
		if(mass - randProMass < AVG_AA_MASS)
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