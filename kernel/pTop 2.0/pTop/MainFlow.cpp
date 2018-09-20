#include <fstream>
#include <iostream>
#include <cstdio>
#include <cstring>
#include <string>
#include <algorithm>
#include <cmath>

#include "flow.h"


//#define _Debug_Spec

using namespace std;

/**
*
*	class CMainFlow
*/

CMainFlow::CMainFlow(const string &strFile) : m_strConfigFile(strFile), m_pInput(NULL),
m_nTotalProNum(0), m_nIdSpecNum(0), m_nIdScan(0), m_nIdProNum(0), m_nLabelIdx(0),
m_cPara(NULL), m_cMapMass(NULL), m_cProIndex(NULL),
m_cPTMForms(NULL), m_cTagIndex(NULL), m_pSum(new Summary()), m_pFragIndex(NULL),
m_pTrace(NULL), m_pState(NULL), m_pMonitor(NULL)
{
	try{
		init();
	}
	catch (exception &e){
		CErrInfo err("MainFlow", "MainFlow", "initialize main flow failed!", e);
		throw runtime_error(err.Get());
	}
	catch (...){
		CErrInfo err("MainFlow", "MainFlow", "caught an unknown exception.");
		throw runtime_error(err.Get());
	}
}


void CMainFlow::init()
{
	try{
		m_cPara = new CConfiguration(m_strConfigFile);

		/**
		*	Get all configuration info from the config.ini
		*	Do some essential check for robust
		**/
		m_cPara->GetAllConfigPara(); // Main parameters from config.ini
		m_cPara->writeConfigFile();

		string strLogFilePath = m_cPara->m_strOutputPath + "\\out.log";
		m_pTrace = Clog::getInstance(strLogFilePath.c_str(), LogAppenderType::LAT_CONSOLE_AND_FILE);
		
		if (m_pInput == NULL){
			MS2InputFactory stFactory;
			m_pInput = stFactory.getImporter(m_cPara->m_inFileType);
		}
	}
	catch (exception & e)
	{
		CErrInfo info("CMainFlow", "init", "Configure failed.");
		throw runtime_error(info.Get(e).c_str());
	}
	catch (...)
	{
		CErrInfo info("CMainFlow", "init", "caught an unknown exception from GetAllConfigPara().");
		throw runtime_error(info.Get().c_str());
	}
}

CMainFlow::~CMainFlow()
{
	if (m_cPara){
		delete m_cPara;
		m_cPara = NULL;
	}
	
	Clog::destroyInstance();
	if (m_pInput){
		delete m_pInput;
		m_pInput = NULL;
	}
}


//[to be removed]

//void CMainFlow::SecondSearcher(vector<PROTEIN_STRUCT> &proteinList, vector<string> &vIdSpec)
//{
//	CTagFlow *cTagIndex = NULL;
//	CTagSearcher *cTagSearcher = NULL;
//	CSearchEngine cSearcher(m_cMapMass, m_cPTMForms, cTagIndex, cTagSearcher, m_cPara->m_lfFragTol, (int)(m_cPara->m_lfPrecurTol) + 1);
//	cSearcher.Init(m_cPara->m_strActivation);
//	
//	for(size_t i = 0; i < m_cPara->m_vSpecFiles.size(); ++i)
//	{
//		FILE *fqryRes = fopen(m_cPara->m_vstrQryResFile[i].c_str(), "a+");
//		if(fqryRes == NULL) 
//		{
//			cout << "[Error] Failed to open output file " << m_cPara->m_vstrQryResFile[i].c_str() << endl;
//			exit(0);
//		}
//
//		vector<SPECTRUM> spectra;
//		spectra.assign(MAX_SPECTRA_NUM, SPECTRUM());
//		
//		cout << "[pTop] Search " << m_cPara->m_vSpecFiles[i].c_str() << endl;
//
//		ifstream fMGF;
//		FILE *fPF = NULL;
//		if(1 == m_cPara->m_inFileType)
//		{
//			fPF = fopen(m_cPara->m_vSpecFiles[i].c_str(), "rb");
//			if(fPF == NULL)
//			{
//				cout << "[Error] Failed to open file: " << m_cPara->m_vSpecFiles[i].c_str() << endl;
//				exit(0);
//			}
//		} else {
//			fMGF.open(m_cPara->m_vSpecFiles[i].c_str(), std::ifstream::in);
//			if(!fMGF)
//			{
//				cout << "[Error] Failed to open file: " << m_cPara->m_vSpecFiles[i].c_str() << endl;
//				exit(0);
//			}
//		}
//
//		int preScan = -1;  // wrm20150728:记录上一个scan号，主要用于分块读取MGF文件
//		while(1)
//		{
//			int specCnt = 0;
//			try
//			{
//				if(1 == m_cPara->m_inFileType)
//				{
//					specCnt = ParsePF(fPF, spectra);
//				} else {
//					int tmpScanCnt = 0;
//					specCnt = ParseMGF(fMGF, spectra, preScan, tmpScanCnt);
//				}
//			}
//			catch(exception & e) 
//			{
//				CErrInfo info("CMainFlow", "Searcher()", "Parsing input file failed.");
//				throw runtime_error(info.Get(e).c_str());
//			}
//			catch(...) 
//			{
//				CErrInfo info("CMainFlow", "Searcher()", "Caught an unknown exception from parsing input file.");
//				throw runtime_error(info.Get().c_str());
//			}
//			
//			int idsIdx = 0, idsLen = (int)vIdSpec.size();
//			for(int s = 0; s < specCnt; ++s)
//			{	
//				if(idsIdx < idsLen && spectra[s].strSpecTitle.compare(vIdSpec[idsIdx]) == 0)
//				{
//					++idsIdx;
//					continue;
//				}
//				//cout << spectra[s].strSpecTitle << "\t";
//				cSearcher.Reset(spectra[s]);
//				cSearcher.SecondSearch(proteinList);
//				
//				printf("[pTop] <Search>: ");
//				printf("%d / %d (%.0f%%)\r", s + 1, specCnt, (s + 1) * 100.0 / specCnt);
//				fflush(stdout);
//			}
//			cout << endl;
//			OutputPrSMRes(fqryRes, spectra, specCnt, i);
//
//			if(specCnt < MAX_SPECTRA_NUM)
//			{
//				break;
//			}
//		}
//		cout << "[pTop] Second Search finished!" << endl;
//
//		fclose(fqryRes);
//		if(fPF != NULL) fclose(fPF);
//		if(fMGF.is_open()) fMGF.close();
//	}
//}

/*
	Output the summary infomation
*/
//void CMainFlow::OutputSummary()
//{
//	for(size_t i = 0; i < m_vScanNum.size(); ++i)
//	{
//		FILE *fsummary = fopen(m_cPara->m_vstrSummary[i].c_str(), "wt");
//		if(NULL == fsummary)
//		{
//			cout << "[Error] Failed to open file: " << m_cPara->m_vstrSummary[i] << endl;
//			continue;
//		}
//
//		int posEnd = m_cPara->m_vSpecFiles[i].find_last_of('.');
//		if(posEnd == string::npos)
//		{
//			posEnd = m_cPara->m_vSpecFiles[i].size();
//		}
//		int posSrt = m_cPara->m_vSpecFiles[i].find_last_of('\\');
//		if(posSrt == string::npos)
//		{
//			posSrt = -1;
//		}
//		string fileName = m_cPara->m_vSpecFiles[i].substr(posSrt+1, posEnd - posSrt - 1);
//		fprintf(fsummary, "%s\n", fileName.c_str());
//		fprintf(fsummary, "Total MS/MS Spectrum: %d\n", m_vSpecNum[i]);
//		fprintf(fsummary, "Total MS/MS Scan: %d\n", m_vScanNum[i]);
//		if(m_vScanNum[i] > 0)
//		{
//			fprintf(fsummary, "ID Rate:%d / %d = %.2f%%\n", m_vIdScan[i], m_vScanNum[i], m_vIdScan[i] * 100.0 / m_vScanNum[i]);
//		}
//
//		fclose(fsummary);
//	}
//}

void CMainFlow::RunMainFlow()
{
	try{
		time_t t_start, t_end;
		t_start = clock();

		m_pTrace->info("Configure...");
		m_pTrace->info("-Database: " + m_cPara->m_strDBFile);
		m_pTrace->info("-Max truncated mass: %f", m_cPara->m_lfMaxTruncatedMass);
		m_pTrace->info("-Second search: %s", m_cPara->m_bSecondSearch ? "true":"false");
		m_pTrace->info("-Tag length for indexing: %s", m_cPara->m_bTagLenInc ? "4/5" : "4");

		// [search]
		if (m_cPara->m_quant.empty()){
			m_nLabelIdx = -1;
			if (m_cPara->m_eWorkFlow == WorkFlowType::IonFlow){
				IonIndexFlow();   //离子索引流程
			}
			else{
				SearchFlow();     // Tag流程
			}
		}
		else{
			for (m_nLabelIdx = 0; m_nLabelIdx < m_cPara->m_quant.size(); ++m_nLabelIdx){  // k labels, search k times
				m_pTrace->info("Label Info: %s", m_cPara->m_quant[m_nLabelIdx].label_name.c_str());
				if (m_cPara->m_eWorkFlow == WorkFlowType::IonFlow){
					IonIndexFlow();   //离子索引流程
				}
				else{
					SearchFlow();     // Tag流程
				}
			}
		}

		//// [filter]
		time_t t_filter = clock();
		Filter();
		t_end = clock();
		m_pTrace->info("filter time used: %fs", (t_end - t_filter) / 1000.0);

		// [quant] [wrm] modified by wrm. 2016.11.14
		if (m_cPara->m_quant.size() > 1){
			shared_ptr<Quantitation> pQuant = make_shared<Quantitation>(m_cPara);
			pQuant->quantify();
		}
		t_end = clock();
		m_pTrace->info("Total Time elapsed: %fs", (t_end - t_start) / 1000.0);
	}
	catch (exception & e) {
		CErrInfo info("CMainFlow", "RunMainFlow", e.what());
		m_pTrace->error(info.Get());
		throw runtime_error(info.Get().c_str());
	}
	catch (...) {
		CErrInfo info("CMainFlow", "RunMainFlow", "caught an unknown exception from running main flow.");
		m_pTrace->error(info.Get());
		throw runtime_error(info.Get().c_str());
	}
}

// 
void CMainFlow::Initialize()
{
	/**
	*	Get all variable and fixed PTMs
	*	Chcek if there is any confics
	*	Calculate all the PTM forms
	*	Init the fix mod info
	**/
	//try
	{
		unordered_map<string, double> &eleMass = m_cPara->m_ElemMass;  // store mass of every chemical
		m_pTrace->info("Init AA mass table...");
		m_cMapMass->Init(m_cPara->m_quant, m_nLabelIdx, m_cPara->m_strOutputPath);// Init AA mass table
		
		m_pTrace->info("Getting the PTM forms...");

		vector<MODINI> fixmodInfo;
		// Search for the modification.ini, get more detail PTM info
		m_cPTMForms->SetModInfo(m_cPara, eleMass, m_nLabelIdx);
		//m_cPTMForms->CalculateModForm(m_cPara->m_nUnexpectedModNum);   // get var mods combination
		m_cPTMForms->CreatePTMIndex(m_cPara->m_nUnexpectedModNum);

		m_pTrace->info("Add fixed modifications...");

		m_cPTMForms->GetFixModInfo(fixmodInfo);
		for (size_t i = 0; i < fixmodInfo.size(); ++i)  // 添加固定修饰（未考虑中性丢失）
		{
			if (MOD_TYPE::MOD_NORMAL == fixmodInfo[i].cType)  // normal
			{
				for (size_t j = 0; j < fixmodInfo[i].strSite.length(); ++j)
				{
					m_cMapMass->AddAAMassbyMod(fixmodInfo[i].strSite[j], fixmodInfo[i].lfMass);
				}
			}
			else if (MOD_TYPE::MOD_PRO_N == fixmodInfo[i].cType) { // N-term
				m_cMapMass->SetNtermMass(fixmodInfo[i].lfMass, fixmodInfo[i].strSite);
			}
			else {  // C-term
				m_cMapMass->SetCtermMass(fixmodInfo[i].lfMass, fixmodInfo[i].strSite);
			}
		}
	}
	//catch (exception & e)
	//{
	//	CErrInfo info("CMainFlow", "Initialize", "GetPTMforms failed.");
	//	throw runtime_error(info.Get(e).c_str());
	//}
	//catch (...)
	//{
	//	CErrInfo info("CMainFlow", "Initialize", "caught an unknown exception from CPrePTMForm.");
	//	throw runtime_error(info.Get().c_str());
	//}
}


void *CMainFlow::_TagSearch(void *pArgs)
{
	CMainFlow *pFlow = (CMainFlow *)pArgs;
	while (pFlow->m_pMonitor->isLocked())
		pFlow->m_pMonitor->freeLock();
	size_t tID = pFlow->m_pMonitor->getCurrentID();
	ThreadState &th = pFlow->m_pState->m_vThreadState[tID];
	//cout << pFlow->m_vSpectra.size() << "!!!!!!!!!!!!!" << endl;
	for (size_t i = tID; i < pFlow->m_vSpectra.size(); i += (size_t)pFlow->m_cPara->m_nThreadNum){
		SPECTRUM &spec = pFlow->m_vSpectra[i];
		if (th.m_tCurrentSpectra % 50 == 0){
			if (tID == 0){
				size_t tTotal = 0;
				for (size_t j = 0; j < pFlow->m_cPara->m_nThreadNum; ++j){
					tTotal += pFlow->m_pState->m_vThreadState[j].m_tCurrentSpectra;
				}
				pFlow->m_pTrace->info("<Search>: %d / %d (%.0f%%)\r", tTotal, pFlow->m_vSpectra.size(), (tTotal)* 100.0 / pFlow->m_vSpectra.size());
			}
		}
#ifdef _Debug_Spec
		if (pFlow->m_vSpectra[i].m_nScanNo != 1052){  //vSpectra[s].m_nScanNo != 1669
			continue;
		}
#endif
		CSearchEngine searcher(pFlow->m_cPara, pFlow->m_cMapMass, pFlow->m_cPTMForms, pFlow->m_cProIndex, pFlow->m_cTagIndex, &spec);
		searcher.Search();
		++th.m_tCurrentSpectra;
	}
	pthread_mutex_lock(&pFlow->m_pMonitor->m_stMutex);
	pFlow->m_pMonitor->freeSignal(tID);
	pthread_cond_signal(&pFlow->m_pMonitor->m_stCond);
	pthread_mutex_unlock(&pFlow->m_pMonitor->m_stMutex);
	return NULL;
}

void * CMainFlow::_IonSearch(void *pArgs)
{
	CMainFlow *pFlow = (CMainFlow *)pArgs;
	while (pFlow->m_pMonitor->isLocked())
		pFlow->m_pMonitor->freeLock();
	size_t tID = pFlow->m_pMonitor->getCurrentID();
	ThreadState &th = pFlow->m_pState->m_vThreadState[tID];
	for (size_t i = tID; i < pFlow->m_vSpectra.size(); i += (size_t)pFlow->m_cPara->m_nThreadNum){
		SPECTRUM &spec = pFlow->m_vSpectra[i];
		if (th.m_tCurrentSpectra % 50 == 0){
			if (tID == 0){
				size_t tTotal = 0;
				for (size_t j = 0; j < pFlow->m_cPara->m_nThreadNum; ++j){
					tTotal += pFlow->m_pState->m_vThreadState[j].m_tCurrentSpectra;
				}
				pFlow->m_pTrace->info("<Search>: %d / %d (%.0f%%)\r", tTotal, pFlow->m_vSpectra.size(), (tTotal)* 100.0 / pFlow->m_vSpectra.size());
			}
		}
#ifdef _Debug_Spec
		if (pFlow->m_vSpectra[i].m_nScanNo != 1052){  //vSpectra[s].m_nScanNo != 1669
			continue;
		}
#endif
		++th.m_tCurrentSpectra;
		if (pFlow->m_IdSpecSet.find(spec.m_nScanNo) != pFlow->m_IdSpecSet.end()){
			continue;
		}
		CSearchEngine searcher(pFlow->m_cPara, pFlow->m_cMapMass, pFlow->m_cPTMForms, pFlow->m_cProIndex, pFlow->m_cTagIndex, &spec);
		searcher.IonIndexSearch(pFlow->m_pFragIndex);
	}
	pthread_mutex_lock(&pFlow->m_pMonitor->m_stMutex);
	pFlow->m_pMonitor->freeSignal(tID);
	pthread_cond_signal(&pFlow->m_pMonitor->m_stCond);
	pthread_mutex_unlock(&pFlow->m_pMonitor->m_stMutex);
	return NULL;
}

void CMainFlow::Searcher(bool isIonIndex)  // 多线程
{
	if(m_pState == NULL)
		m_pState = new SearchState(m_cPara->m_nThreadNum);;
	if(m_pMonitor == NULL)
		m_pMonitor = new ThreadMonitor(m_cPara->m_nThreadNum);

	for (size_t i = 0; i < (size_t)m_cPara->m_nThreadNum; ++i){
		m_pMonitor->setSignal(i);
		m_pMonitor->setLock();
		pthread_t tid;
		m_pMonitor->setCurrentID(i);
		if (isIonIndex){
			pthread_create(&tid, NULL, &_IonSearch, this);
		}
		else{
			pthread_create(&tid, NULL, &_TagSearch, this);
		}
		m_pMonitor->waitForLock();
	}

	// hold on till all thread finish its task
	for (int i = 0; i < m_cPara->m_nThreadNum; ++i){
		pthread_mutex_lock(&m_pMonitor->m_stMutex);
		while (!m_pMonitor->isFreeSignal(i)){
			pthread_cond_wait(&m_pMonitor->m_stCond, &m_pMonitor->m_stMutex);
		}
		pthread_mutex_unlock(&m_pMonitor->m_stMutex);
	}
	if (m_pState){
		delete m_pState;
		m_pState = NULL;
	}
	if (m_pMonitor){
		delete m_pMonitor;
		m_pMonitor = NULL;
	}
}
// separate database
void CMainFlow::DatabaseSegmentation(const char *strOutPath, const int fileID)
{
	try{
		m_cProIndex = new CProteinIndex(m_cPara->m_strDBFile, m_cMapMass);
		m_cProIndex->OpenFastaFile();
		m_nTotalProNum = 0;
		int nDBBlock = 0;
		while (1)
		{

			//MEMORYSTATUS memStatus;
			//GlobalMemoryStatus(&memStatus);
			//cout<<"Begin: Avali Pspace = "<<memStatus.dwAvailVirtual<<endl<<endl;

			bool stop = m_cProIndex->IndexofProtein();
			m_nTotalProNum += m_cProIndex->m_ProteinList.size();

			m_cTagIndex->Clear();
			m_cTagIndex->CreateTagIndex(m_cProIndex->m_ProteinList);

			++nDBBlock;

			m_pTrace->info("search...");

			Searcher(false);
//			//@test
//			FILE *fp = fopen("peakNum.txt", "w");
//			for (int s = 0; s < m_vSpectra.size(); ++s)  //
//			{
//				//@ test
//#ifdef _Debug_Spec
//				if (vSpectra[s].m_nScanNo != 1157){  //vSpectra[s].m_nScanNo != 1669
//					continue;
//				}
//#endif
//				fprintf(fp, "%d %d\n", m_vSpectra[s].m_nScanNo, m_vSpectra[s].nPeaksNum);
//				CSearchEngine m_cSearcher(m_cPara, m_cMapMass, m_cPTMForms, m_cProIndex, m_cTagIndex, m_vSpectra[s]);
//				m_cSearcher.Search();
//
//				/* Print the Rate of Process */
//				printf("[pTop] <Search>: ");
//				printf("%d / %d (%.0f%%)\r", s + 1, m_vSpectra.size(), (s + 1) * 100.0 / m_vSpectra.size());
//				fflush(stdout);
//			}
//			fclose(fp);

			m_pTrace->info("<Batch %d> Block %d 100%%", m_nBatchNum, nDBBlock);

			if (stop == true) break;
		}
		m_cProIndex->CloseFile();

		OutputTempPrSMRes(strOutPath, fileID);

		delete m_cProIndex;
	}
	catch (exception & e)
	{
		CErrInfo info("CMainFlow", "DatabaseSegmentation", "Searcher failed.");
		throw runtime_error(info.Get(e).c_str());
	}
	catch (...)
	{
		CErrInfo info("CMainFlow", "DatabaseSegmentation", "caught an unknown exception from Searcher.");
		throw runtime_error(info.Get().c_str());
	}
}

void CMainFlow::DBSegmentationFragFlow(const char *strOutPath, const int fileID)
{
	try{
		m_cProIndex = new CProteinIndex(m_cPara->m_strDBFile, m_cMapMass);
		m_cProIndex->OpenFastaFile();

		m_nTotalProNum = 0;
		int nDBBlock = 0;
		while (1)
		{

			//MEMORYSTATUS memStatus;
			//GlobalMemoryStatus(&memStatus);
			//cout<<"Begin: Avali Pspace = "<<memStatus.dwAvailVirtual<<endl<<endl;

			bool stop = m_cProIndex->IndexofProtein();
			m_nTotalProNum += m_cProIndex->m_ProteinList.size();

			m_pFragIndex->CreateFragmentIndex(m_cProIndex->m_ProteinList);
			//m_pFragIndex->ShowFragmentTable(m_cProIndex->m_ProteinList);

			++nDBBlock;
			m_pTrace->info("search...");

			Searcher(true);
//			//@test
//			FILE *fp = fopen("peakNum.txt", "w");
//			for (int s = 0; s < m_vSpectra.size(); ++s)  //
//			{
//				if (setIdSpec.find(m_vSpectra[s].m_nScanNo) != setIdSpec.end()){
//					continue;
//				}
//				//@ test
//#ifdef _Debug_Spec
//				if (vSpectra[s].m_nScanNo != 1578){  //vSpectra[s].m_nScanNo != 1669
//					continue;
//				}
//#endif
//				fprintf(fp, "%d %d\n", m_vSpectra[s].m_nScanNo, m_vSpectra[s].nPeaksNum);
//				CSearchEngine m_cSearcher(m_cPara, m_cMapMass, m_cPTMForms, m_cProIndex, m_cTagIndex, m_vSpectra[s]); //
//				m_cSearcher.IonIndexSearch(m_pFragIndex);
//
//				/* Print the Rate of Process */
//				printf("[pTop] <Search>: ");
//				printf("%d / %d (%.0f%%)\r", s + 1, m_vSpectra.size(), (s + 1) * 100.0 / m_vSpectra.size());
//				fflush(stdout);
//			}
//			fclose(fp);

			m_pTrace->info("<Batch %d> Block %d 100%%", m_nBatchNum, nDBBlock);

			if (stop == true) break;
		}
		m_cProIndex->CloseFile();

		OutputTempPrSMRes(strOutPath, fileID);

		delete m_cProIndex;
	}
	catch (exception & e)
	{
		CErrInfo info("CMainFlow", "DatabaseSegmentation", "Searcher failed.");
		throw runtime_error(info.Get(e).c_str());
	}
	catch (...)
	{
		CErrInfo info("CMainFlow", "DatabaseSegmentation", "caught an unknown exception from Searcher.");
		throw runtime_error(info.Get().c_str());
	}
}


string CMainFlow::getProPTM(const string &seq, vector<UINT_UINT> &vModSites)
{
	string pro_ptm = seq + "#";
	sort(vModSites.begin(), vModSites.end(), UINT_UINT::cmpByFirstInc);
	for (int i = 0; i < vModSites.size(); ++i){
		pro_ptm += m_cPTMForms->GetNamebyID(vModSites[i].nFirst) + "#";
	}
	return pro_ptm;
}

// 19 col: (Title,Scan,Charge,Precusor Mass,Theoretical Mass,Mass Diff Da,Mass Diff ppm,Protein AC,Protein Sequence,PTMs,Score,Evalue,
// Number of Matched Peaks,Nterm Matched Ions,Cterm Matched Ions,Nterm Matched Intensity Raio,Cterm Matched Intensity Ratio,isDecoy,Label Type);
void CMainFlow::OutputTempPrSMRes(const char *strOutPath, const int fileID)
{
	m_pTrace->info("Output Top %d PrSMs of each Spectrum to temporary file %s", m_cPara->m_nOutputTopK, strOutPath);
	
	FILE *ftmp = fopen(strOutPath, "w");
	if (ftmp == NULL){
		m_pTrace->error("Failed to open output file : %s.",strOutPath);
		exit(0);
	}
	unordered_set<string> proPTMSet;
	///
	// 选择top k 参与重打分
	for (int s = 0; s < m_vSpectra.size(); ++s) // spectrum
	{
		for (int p = 0; p < m_vSpectra[s].nPrecurNum; ++p)
		{
			//cout<<s<<" "<<spectra[s].vPrecursors[p].nValidCandidateProNum<<endl;
			if (0 < m_vSpectra[s].vPrecursors[p].nValidCandidateProNum)
			{
				// sort by score
				sort(m_vSpectra[s].vPrecursors[p].aBestPrSMs.begin(), m_vSpectra[s].vPrecursors[p].aBestPrSMs.begin() + m_vSpectra[s].vPrecursors[p].nValidCandidateProNum, PROTEIN_SPECTRA_MATCH::CmpByScoreDes);
				//int outputnum = min(vSpectra[s].vPrecursors[p].nValidCandidateProNum, m_cPara->m_nOutputTopK);
				proPTMSet.clear();
				for (int k = 0; k < m_vSpectra[s].vPrecursors[p].nValidCandidateProNum; ++k)
				{
					string pro_ptm = getProPTM(m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].strProSQ, m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].vModSites);
					if (proPTMSet.find(pro_ptm) == proPTMSet.end())
					{
						if (fabs(m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].lfScore - 0) < DOUCOM_EPS)  continue;
						vector<string> tmpVec;
						CStringProcess::Split(m_vSpectra[s].strSpecTitle, ".", tmpVec);
						int scan = m_vSpectra[s].m_nScanNo;
						if (MS2FormatType::MFT_PF == m_cPara->m_inFileType)
						{
							fprintf(ftmp, "%u,%s.%d.%d.dta,%d,", fileID, m_vSpectra[s].strSpecTitle.c_str(), m_vSpectra[s].vPrecursors[p].nPrecursorCharge, p, scan);  // FileID,Title, Scan
						}
						else {
							fprintf(ftmp, "%u,%s,%d,", fileID, m_vSpectra[s].strSpecTitle.c_str(), scan);  // FileID, title, scan
						}

						//cout << spectra[s].aBestPrSMs[0].strProSQ << endl;
						double dist = m_vSpectra[s].vPrecursors[p].lfPrecursorMass - m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].lfMass;
						fprintf(ftmp, "%d,", m_vSpectra[s].vPrecursors[p].nPrecursorCharge);  // charge
						fprintf(ftmp, "%f,", m_vSpectra[s].vPrecursors[p].lfPrecursorMass);  // precursor mass 
						fprintf(ftmp, "%f,", m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].lfMass);  // theoretical mass
						fprintf(ftmp, "%.3f,%.1f,", dist, 1000000 * dist / m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].lfMass);  // mass diff da,  mass diff ppm
						fprintf(ftmp, "%s,%s,", m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].strProAC.c_str(),
							m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].strProSQ.c_str());  // protein ac, protein sq

						if (!m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].vModSites.empty())
						{
							for (int mIdx = (int)m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].vModSites.size() - 1; mIdx >= 0; --mIdx)
							{
								fprintf(ftmp, "(%d)%s;", m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].vModSites[mIdx].nSecond,
									m_cPTMForms->GetNamebyID(m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].vModSites[mIdx].nFirst).c_str());
							}
							fprintf(ftmp, ",");
						}
						else {
							fprintf(ftmp, "%s,", NONMOD.c_str());  // ptms
						}
						fprintf(ftmp, "%f,%e,", m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].lfScore, m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].lfQvalue); // score, evalue
						fprintf(ftmp, "%d,", m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].nMatchedPeakNum); // matched peaks
						fprintf(ftmp, "%d,%d,%f,%f,", m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].matchedInfo.nNterm_matched_ions,
							m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].matchedInfo.nCterm_matched_ions,
							m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].matchedInfo.lfNtermMatchedIntensityRatio,
							m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].matchedInfo.lfCtermMatchedIntensityRatio);
						fprintf(ftmp, "%f,%f,%f,%f,", m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].featureInfo.lfCom_ions_ratio,
							m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].featureInfo.lfTag_ratio,
							m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].featureInfo.lfPTM_score,
							m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].featureInfo.lfFragment_error_std);
						fprintf(ftmp, "%d,%u\n", m_vSpectra[s].vPrecursors[p].aBestPrSMs[k].nIsDecoy, m_nLabelIdx + 1); // isDecoy, label type
						
						proPTMSet.insert(pro_ptm);
					}
					if (proPTMSet.size() == m_cPara->m_nOutputTopK)  break;
				}
			}
		}
	}
	fclose(ftmp);
}

void CMainFlow::mergeSubRes(vector<string> &vSubFiles, string &resFile)
{
	string heads = "FileID,Title,Scan,Charge,Precursor Mass,Theoretical Mass,Mass Diff Da,Mass Diff PPM,";  // 8
	heads += "Protein AC,Protein Sequence,PTMs,Raw Score,Final Score,"; // 5
	heads += "Matched Peaks,Nterm Matched Ions,Cterm Matched Ions,Nterm Matched Intensity Ratio,Cterm Matched Intensity Ratio,";  // 5
	heads += "Com_Ion_Ratio,Tag_Ratio,PTM_Score,Fragment_Error_STD,isDecoy,Label Type\n";  // 6
	CConfiguration::MergeFiles(vSubFiles, resFile, heads);
}

/**
* search flow: first separate spectra, then separate database. [wrm]
**/
void CMainFlow::SearchFlow()  //
{
	time_t t_start, t_end;
	t_start = clock();

//	try{
		m_cMapMass = new CMapAAMass(m_cPara->m_ElemMass);
		m_cPTMForms = new CPrePTMForm(m_cPara);
		m_cTagIndex = new CTagFlow(m_cPara, m_cMapMass);

		// init AA and Mods
		Initialize();
		/**
		*	Init tag seacher, title of query results, and output plabel files
		*	Create protein index
		*	Create tag index
		*	Call searcher to finish the search
		**/
		/*
		* 将分库分谱改成分谱分库，便于后续合并，为每张谱保留Top K 的结果
		* modified by wrm 2015.11.16. 2016.11.21
		*/
		
		m_cMapMass->InitSearchParam(m_cPTMForms);

		m_pSum->m_vSpecNum.clear();
		m_pSum->m_vScanNum.clear();
		// 
		int lbIdx = (m_nLabelIdx > -1) ? m_nLabelIdx : 0;

		for (size_t i = 0; i < m_cPara->m_vSpecFiles.size(); ++i)
		{
			t_start = clock();
			m_pTrace->info("Start processing " + m_cPara->m_vSpecFiles[i]);
			// spectra segmentation
			bool bLoading(true);
			m_pInput->startLoad(m_cPara->m_vSpecFiles[i]);
			int nTotalSpecNum = m_pInput->GetSpecNum();
			int nTotalScanNum = m_pInput->GetScanNum();
			int nOneLoad(MAX_SPECTRA_NUM), nKeptSpectra(0);
			int nTotalNum = nTotalSpecNum;
			if (m_cPara->m_inFileType == MFT_PF){
				nTotalNum = nTotalScanNum;
			}
			if (nTotalNum <= 0){
				continue;
			}
			m_pTrace->info("Total spectra: %d", nTotalNum);
			m_nBatchNum = 0;
			vector<string> vSubFiles;
			while (bLoading){
				if (nTotalNum - nKeptSpectra < nOneLoad){
					nOneLoad = nTotalNum - nKeptSpectra;
				}
				m_vSpectra.clear();
				m_vSpectra.reserve(nOneLoad);

				m_pInput->loadNextBatch(nOneLoad, m_vSpectra, nKeptSpectra);
				m_pTrace->info("Loaded %d spectra, %d / %d", nOneLoad, nKeptSpectra, nTotalNum);
				bLoading = (nKeptSpectra < nTotalNum);

				++ m_nBatchNum;

				char temp[1024] = { 0 };
				sprintf(temp, "%s\\%s.%d.%d.txt", m_cPara->m_strOutputPath.c_str(), m_cPara->m_vstrFilenames[i].c_str(), lbIdx, m_nBatchNum);
				
				DatabaseSegmentation(temp, i);

				vSubFiles.push_back(temp);
			}

			mergeSubRes(vSubFiles, m_cPara->m_vstrQryResFile[i][lbIdx]);

			m_pInput->endLoad();
			m_pTrace->info("Search finished!");
			m_pTrace->info("The results in " + m_cPara->m_vstrQryResFile[i][lbIdx]);
			t_end = clock();
			m_pTrace->info("Time used: %fs", (t_end - t_start) / 1000.0);

			m_pSum->m_vSpecNum.push_back(nTotalSpecNum);
			m_pSum->m_vScanNum.push_back(nTotalScanNum);
			
			if (m_cPara->m_bSecondSearch && m_cPara->calBY){
				SecondSearcher(i, m_cPara->m_vstrQryResFile[i][lbIdx]);
			}

			Rerank *pRerank = new Rerank(m_cPara, m_cPara->m_vstrQryResFile[i][lbIdx]);
			pRerank->run();
			delete pRerank;

		}
		delete m_cTagIndex;
		delete m_cPTMForms;
		delete m_cMapMass;
	//}
	//catch (exception & e)
	//{
	//	CErrInfo info("CMainFlow", "RunMainFlow", "Searcher failed.");
	//	throw runtime_error(info.Get(e).c_str());
	//}
	//catch (...)
	//{
	//	CErrInfo info("CMainFlow", "RunMainFlow", "caught an unknown exception from Searcher.");
	//	throw runtime_error(info.Get().c_str());
	//}
}


void CMainFlow::SecondSearcher(const int fileID, const string &strQryRstFile)
{
	time_t t_start = clock();

	m_pFragIndex = new FragmentIndex(m_cPara, m_cMapMass, m_cPTMForms);

	CResFilter *pFilter = new CResFilter(m_cPara);
	m_IdSpecSet.clear();
	pFilter->FirstFilter(strQryRstFile, m_cPara->m_lfThreshold, m_IdSpecSet);

	m_pTrace->info("%d spectra identified by first search", m_IdSpecSet.size());
	m_pTrace->info("Start second search...");
	// spectra segmentation
	bool bLoading(true);
	m_pInput->startLoad(m_cPara->m_vSpecFiles[fileID]);
	int nTotalSpecNum = m_pInput->GetSpecNum();
	int nTotalScanNum = m_pInput->GetScanNum();
	int nOneLoad(MAX_SPECTRA_NUM), nCurSpectraIdx(0);
	int nTotalNum = nTotalSpecNum;
	if (m_cPara->m_inFileType == MFT_PF){
		nTotalNum = nTotalScanNum;
	}
	//m_pTrace->info("Start processing " + m_cPara->m_vSpecFiles[fileID]);;
	m_nBatchNum = 0;
	vector<string> vSubFiles;
	while (bLoading){
		if (nTotalNum - nCurSpectraIdx < MAX_SPECTRA_NUM){
			nOneLoad = nTotalNum - nCurSpectraIdx;
		}
		m_vSpectra.clear();
		m_vSpectra.reserve(nOneLoad);

		m_pInput->loadNextBatch(nOneLoad, m_vSpectra, nCurSpectraIdx);
		m_pTrace->info("Loaded %d spectra, %d / %d", nOneLoad, nCurSpectraIdx, nTotalNum);
		bLoading = (nCurSpectraIdx < nTotalNum);

		++m_nBatchNum;

		char temp[1024] = { 0 };
		sprintf(temp, "%s\\ss.%d.txt", m_cPara->m_strOutputPath.c_str(), m_nBatchNum);
		
		DBSegmentationFragFlow(temp, fileID);
		
		vSubFiles.push_back(temp);
	}
	string ss = m_cPara->m_strOutputPath + (cSlash)+"ss.txt";
	mergeSubRes(vSubFiles, ss);
	vSubFiles.clear();
	vSubFiles.push_back(strQryRstFile);
	vSubFiles.push_back(ss);
	pFilter->MergeQryResult(vSubFiles, strQryRstFile);
	remove(ss.c_str());  // delete tempary file
	m_pInput->endLoad();
	m_pTrace->info("Search finished!");
	m_pTrace->info("The results in " + strQryRstFile);
	delete pFilter;
	delete m_pFragIndex;
	time_t t_end = clock();
	m_pTrace->info("Time used for second search: %fs", (t_end - t_start) / 1000.0);

}

void CMainFlow::IonIndexFlow()
{
	time_t t_start, t_end;
	t_start = clock();

	//	try{
	m_cMapMass = new CMapAAMass(m_cPara->m_ElemMass);
	m_cPTMForms = new CPrePTMForm(m_cPara);
	m_cTagIndex = new CTagFlow(m_cPara, m_cMapMass);

	// init AA and Mods
	Initialize();

	m_pFragIndex = new FragmentIndex(m_cPara, m_cMapMass, m_cPTMForms);
	m_cMapMass->InitSearchParam(m_cPTMForms);

	m_pSum->m_vSpecNum.clear();
	m_pSum->m_vScanNum.clear();
	m_IdSpecSet.clear();
	// 
	int lbIdx = (m_nLabelIdx > -1) ? m_nLabelIdx : 0;

	for (size_t i = 0; i < m_cPara->m_vSpecFiles.size(); ++i)
	{
		t_start = clock();

		// spectra segmentation
		bool bLoading(true);
		m_pInput->startLoad(m_cPara->m_vSpecFiles[i]);
		int nTotalSpecNum = m_pInput->GetSpecNum();
		int nTotalScanNum = m_pInput->GetScanNum();
		int nOneLoad(MAX_SPECTRA_NUM), nCurSpectraIdx(0);
		int nTotalNum = nTotalSpecNum;
		if (m_cPara->m_inFileType == MFT_PF){
			nTotalNum = nTotalScanNum;
		}
		m_pTrace->info("Start processing " + m_cPara->m_vSpecFiles[i]);;
		m_nBatchNum = 0;
		vector<string> vSubFiles;
		while (bLoading){
			if (nTotalNum - nCurSpectraIdx < MAX_SPECTRA_NUM){
				nOneLoad = nTotalNum - nCurSpectraIdx;
			}
			m_vSpectra.clear();
			m_vSpectra.reserve(nOneLoad);

			m_pInput->loadNextBatch(nOneLoad, m_vSpectra, nCurSpectraIdx);
			m_pTrace->info("Loaded %d spectra, %d / %d", nOneLoad, nCurSpectraIdx, nTotalNum);
			bLoading = (nCurSpectraIdx < nTotalNum);

			++m_nBatchNum;

			char temp[1024] = { 0 };
			sprintf(temp, "%s\\%s.%d.%d.txt", m_cPara->m_strOutputPath.c_str(), m_cPara->m_vstrFilenames[i].c_str(), lbIdx, m_nBatchNum);

			DBSegmentationFragFlow(temp, i);

			vSubFiles.push_back(temp);
		}
		mergeSubRes(vSubFiles, m_cPara->m_vstrQryResFile[i][lbIdx]);

		m_pInput->endLoad();
		m_pTrace->info("Search finished!");
		m_pTrace->info("The results in " + m_cPara->m_vstrQryResFile[i][lbIdx]);
		t_end = clock();
		m_pTrace->info("Time used: %fs", (t_end - t_start) / 1000.0);

		m_pSum->m_vSpecNum.push_back(nTotalSpecNum);
		m_pSum->m_vScanNum.push_back(nTotalScanNum);

		
		Rerank *pRerank = new Rerank(m_cPara, m_cPara->m_vstrQryResFile[i][lbIdx]);
		pRerank->run();
		delete pRerank;

	}
	delete m_pFragIndex;
	delete m_cTagIndex;
	delete m_cPTMForms;
	delete m_cMapMass;
	//}
	//catch (exception & e)
	//{
	//	CErrInfo info("CMainFlow", "RunMainFlow", "Searcher failed.");
	//	throw runtime_error(info.Get(e).c_str());
	//}
	//catch (...)
	//{
	//	CErrInfo info("CMainFlow", "RunMainFlow", "caught an unknown exception from Searcher.");
	//	throw runtime_error(info.Get().c_str());
	//}
}

/**
*	Filter part
**/
void CMainFlow::Filter()
{
	try
	{
		m_pTrace->info("Begin to filter...");
		CResFilter filter(m_cPara, m_pSum);
		filter.Run();

	}
	catch (exception & e)
	{
		CErrInfo info("CMainFlow", "RunMainFlow", "Filter failed.");
		throw runtime_error(info.Get(e).c_str());
	}
	catch (...)
	{
		CErrInfo info("CMainFlow", "RunMainFlow", "caught an unknown exception from Filter.");
		throw runtime_error(info.Get().c_str());
	}
}

