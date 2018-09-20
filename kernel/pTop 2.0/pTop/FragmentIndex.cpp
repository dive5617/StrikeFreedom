/*
*  CreateIndex.h
*
*  Created on: 2017-03-13
*  Author: wrm
*/

#include "sdk.h"
#include "config.h"
#include "index.h"

FragmentIndex::FragmentIndex(const CConfiguration *pParameter, CMapAAMass *pMassMap, CPrePTMForm *pPTMForms)
	: m_cPara(pParameter), m_cMassMap(pMassMap), m_cPTMForms(pPTMForms),
	m_tMassRange(size_t((pParameter->m_lfMaxFragMass - pParameter->m_lfMinFragMass) *MZ_INDEX_SCALE)), 
	m_vBFragmentSite(26, false), m_vYFragmentSite(26,false),
	m_vCnt(m_tMassRange + 1, 0), m_ullFragCnt(0)
{
	m_pTrace = Clog::getInstance();
	for (int i = 0; i < m_cPara->m_strBIonsSites.length(); ++i){
		if (m_cPara->m_strBIonsSites[i] < 'A' || m_cPara->m_strBIonsSites[i] > 'Z')  continue;
		m_vBFragmentSite[m_cPara->m_strBIonsSites[i] - 'A'] = true;
	}
	for (int i = 0; i < m_cPara->m_strYIonsSites.length(); ++i){
		if (m_cPara->m_strYIonsSites[i] < 'A' || m_cPara->m_strYIonsSites[i] > 'Z')  continue;
		m_vYFragmentSite[m_cPara->m_strYIonsSites[i] - 'A'] = true;
	}
}

FragmentIndex::~FragmentIndex()
{
	m_pTrace->debug("fragments number: %llu", m_ullFragCnt);
	if (m_pTrace){
		m_pTrace = NULL;
	}
}

void FragmentIndex::Clear()
{
	fill(m_vCnt.begin(), m_vCnt.end(), 0);
	vector<size_t>().swap(m_vFragTable);
	m_vFragTable.clear();
}

void FragmentIndex::CreateFragmentIndex(const vector<PROTEIN_STRUCT> &proList)
{
	try{
		m_pTrace->info("Create fragment index...");
		if (proList.empty())  return;
		Clear();
		int nProNum = proList.size();
		// count
		for (size_t pIdx = 0; pIdx < proList.size(); ++pIdx)
		{
			getFragmentIons(proList[pIdx], -1);
		}
		/* accumulate counts */
		for (size_t tIdx = 1; tIdx <= m_tMassRange; ++tIdx)
			m_vCnt[tIdx] += m_vCnt[tIdx - 1];

		if (m_vCnt[m_tMassRange] > INT_MAX) {
			ostringstream oss;
			oss << "bad allocation! you want to require " << m_vCnt[m_tMassRange]
				<< "bytes, this program forbid to allocate more than 2GB.\n";
			throw runtime_error(oss.str());
		}
		m_vFragTable.resize(m_vCnt[m_tMassRange]);
		// fill
		for (size_t pIdx = 0; pIdx < proList.size(); ++pIdx)
		{
			getFragmentIons(proList[pIdx], pIdx);

		}

	}
	catch (exception & e)
	{
		CErrInfo info("FragmentIndex", "createFragmentIndex", "");
		throw runtime_error(info.Get(e).c_str());
	}
	catch (...)
	{
		CErrInfo info("FragmentIndex", "createFragmentIndex", "caught an unknown exception from creating fragment index.");
		throw runtime_error(info.Get().c_str());
	}
}

void FragmentIndex::ShowFragmentTable(const vector<PROTEIN_STRUCT> &proList)
{
	FILE *fp = fopen("tmp\\FragmentTable.txt", "w");
	if (fp == NULL){
		printf("cannot create fragment index file...\n");
		return;
	}
	fprintf(fp, "begin to show the fragment table information......\n");
	for (size_t i = 0; i < m_vCnt.size() - 1; ++i) {
		/* attention: m_vCnt[i+1] > m_vCnt[i] not equal to m_vCnt[i+1] - m_vCnt[i] */
		for (size_t j = m_vCnt[i]; j < m_vCnt[i + 1]; ++j) {
			fprintf(fp, "mass = %.3lf, store value: %llu, %c ion.\n", m_cPara->m_lfMinFragMass + i*1.0 / MZ_INDEX_SCALE, m_vFragTable[j], (m_vFragTable[j] & SET_MASK) ? 'b' : 'y');  //FORMAT_SIZE_XT
			size_t proID = (m_vFragTable[j] & PROID_MASK);
			fprintf(fp, "proID: %lu,  sequence: %s\n", proID, proList[proID].strProSQ.c_str());
		}
		if (m_vCnt[i + 1] > m_vCnt[i])
			fprintf(fp, "\n");
	}
	fprintf(fp, "end................................................\n\n");
	fclose(fp);

	FILE *ff = fopen("tmp\\FragmentIndexFreq.txt", "w");
	if (ff == NULL){
		printf("cannot create fragment index file...\n");
		return;
	}
	for (size_t i = 0; i < m_vCnt.size() - 1; ++i) {
		/* attention: m_vCnt[i+1] > m_vCnt[i] not equal to m_vCnt[i+1] - m_vCnt[i] */
		if (m_vCnt[i] < m_vCnt[i + 1]) {
			fprintf(ff, "%.3f %lu\n", m_cPara->m_lfMinFragMass + i*1.0 / MZ_INDEX_SCALE, m_vCnt[i + 1] - m_vCnt[i]);
		}
	}
	fclose(ff);

}

// consider 'M' at N-term 
void FragmentIndex::getFragmentIons(const PROTEIN_STRUCT &pro, int pEntry)
{
	if (pro.strProSQ.length() < 2)  return;
	vector<double> bmass(1,IonMass_Proton);
	vector<double> ymass(1,IonMass_H2O + IonMass_Proton);
	vector<int> vNtermMod = m_cPTMForms->GetNtermMods();
	vector<int> vCtermMod = m_cPTMForms->GetCtermMods();
	int proLen = pro.strProSQ.length();
	double aamass = 0;
	vector<bool> vis(proLen, false);  // whether y ion should be calculated
	for (int i = 0; i < vNtermMod.size(); ++i){  // n-term mod
		MODINI &mod = m_cPTMForms->GetMODINIbyID(vNtermMod[i]);
		if (mod.strSite.find(pro.strProSQ[0]) != string::npos){
			bmass.push_back(IonMass_Proton + mod.lfMass);
		}
	}
	if (pro.strProSQ[0] == 'M'){
		bmass.push_back(IonMass_Proton - AAMass_M_mono);
		for (int i = 0; i < vNtermMod.size(); ++i){  // n-term mod
			MODINI &mod = m_cPTMForms->GetMODINIbyID(vNtermMod[i]);
			if (mod.strSite.find(pro.strProSQ[1]) != string::npos){
				bmass.push_back(IonMass_Proton - AAMass_M_mono + mod.lfMass);
			}
		}
	}
	for (int i = 0; i < vCtermMod.size(); ++i){  // c-term mod
		MODINI &mod = m_cPTMForms->GetMODINIbyID(vCtermMod[i]);
		if (mod.strSite.find(pro.strProSQ.back()) != string::npos){
			ymass.push_back(ymass[0] + mod.lfMass);
		}
	}
	for (int i = 0; i < pro.strProSQ.length(); ++ i){
		aamass = m_cMassMap->GetAAMass(pro.strProSQ[i]);
		for (int j = 0; j < bmass.size(); ++j){
			bmass[j] += aamass;
		}
		if (m_vBFragmentSite[pro.strProSQ[i] - 'A']){		
			for (int j = 0; j < bmass.size(); ++j){
				if (pEntry != -1){
					pEntry |= SET_MASK;  // highest bit set to 1
				}
				addToFragTable(bmass[j], pEntry);
			}
			if (i < pro.strProSQ.length() - 1){
				vis[i + 1] = true;
			}
		}
	}
	for (int i = proLen - 1; i >= 0; --i){
		aamass = m_cMassMap->GetAAMass(pro.strProSQ[i]);
		for (int j = 0; j < ymass.size(); ++j){
			ymass[j] += aamass;
		}
		if (m_vYFragmentSite[pro.strProSQ[i] - 'A'] || vis[i]){	
			for (int j = 0; j < ymass.size(); ++j){
				if (pEntry != -1){
					pEntry &= RECOVER_MASK;  // highest bit set to 0
				}
				addToFragTable(ymass[j], pEntry);
			}
		}
	}
}

// counting sort: first count, second fill in
void FragmentIndex::addToFragTable(const double lfMass, const int pEntry)
{
	if (lfMass >= m_cPara->m_lfMinFragMass && lfMass <= m_cPara->m_lfMaxFragMass) {
		size_t tDiffIdx = (size_t)((lfMass - m_cPara->m_lfMinFragMass) * MZ_INDEX_SCALE);
		if (pEntry < 0) {  // count
			++m_ullFragCnt;
			++m_vCnt[tDiffIdx];
		}
		else {
			--m_vCnt[tDiffIdx];
			m_vFragTable[m_vCnt[tDiffIdx]] = (size_t)pEntry;
		}
	}
}


//  b ions
bool FragmentIndex::SearchNTerms(std::unordered_map<size_t, double> &mpTerms, const FRAGMENT_ION &stPeak)
{   
	bool flag = false;
	double win = stPeak.lfMz*m_cPara->m_lfFragTol / 1e6;
	double low = max(stPeak.lfMz - win, m_cPara->m_lfMinFragMass);
	double high = min(stPeak.lfMz + win, m_cPara->m_lfMaxFragMass);
	int lowIdx = floor((low - m_cPara->m_lfMinFragMass)*MZ_INDEX_SCALE);
	int highIdx = ceil((high - m_cPara->m_lfMinFragMass)*MZ_INDEX_SCALE);
	for (size_t tDiffIdx = lowIdx; tDiffIdx <= highIdx && tDiffIdx < m_tMassRange; ++tDiffIdx) {
		for (size_t tIdx = m_vCnt[tDiffIdx]; tIdx < m_vCnt[tDiffIdx + 1]; ++tIdx) {
			if (m_vFragTable[tIdx] & SET_MASK){
				size_t tProID = m_vFragTable[tIdx] & RECOVER_MASK;
				mpTerms[tProID] += 1.0;
				flag = true;
			}
		}
	}
	return flag;
}

// y ions
bool FragmentIndex::SearchCTerms(std::unordered_map<size_t, double> &mpTerms, const FRAGMENT_ION &stPeak)
{
	bool flag = false;
	double win = stPeak.lfMz*m_cPara->m_lfFragTol / 1e6;
	double low = max(stPeak.lfMz - win, m_cPara->m_lfMinFragMass);
	double high = min(stPeak.lfMz + win, m_cPara->m_lfMaxFragMass);
	int lowIdx = floor((low - m_cPara->m_lfMinFragMass)*MZ_INDEX_SCALE);
	int highIdx = ceil((high - m_cPara->m_lfMinFragMass)*MZ_INDEX_SCALE);
	for (size_t tDiffIdx = lowIdx; tDiffIdx <= highIdx && tDiffIdx < m_tMassRange; ++tDiffIdx) {
		for (size_t tIdx = m_vCnt[tDiffIdx]; tIdx < m_vCnt[tDiffIdx + 1]; ++tIdx) {
			if (!(m_vFragTable[tIdx] & SET_MASK)){
				size_t tProID = m_vFragTable[tIdx];
				mpTerms[tProID] += 1;
				flag = true;
			}
		}
	}
	return flag;
}

bool FragmentIndex::SearchTerms(std::unordered_map<size_t, double> &mpTerms, const FRAGMENT_ION &stPeak)
{
	bool flag = false;
	double win = stPeak.lfMz*m_cPara->m_lfFragTol / 1e6;
	double low = max(stPeak.lfMz - win, m_cPara->m_lfMinFragMass);
	double high = min(stPeak.lfMz + win, m_cPara->m_lfMaxFragMass);
	int lowIdx = floor((low - m_cPara->m_lfMinFragMass)*MZ_INDEX_SCALE);
	int highIdx = ceil((high - m_cPara->m_lfMinFragMass)*MZ_INDEX_SCALE);
	for (size_t tDiffIdx = lowIdx; tDiffIdx <= highIdx && tDiffIdx < m_tMassRange; ++tDiffIdx) {
		for (size_t tIdx = m_vCnt[tDiffIdx]; tIdx < m_vCnt[tDiffIdx + 1]; ++tIdx) {
			// ignore the highest bit, the difference between b and y
			size_t tProID = m_vFragTable[tIdx] & RECOVER_MASK;
			mpTerms[tProID] +=  1.0;  //stPeak.lfIntens; //
			flag = true;
		}
	}
	return flag;
}

