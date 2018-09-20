
#include <iostream>
#include <algorithm>
#include <string>
#include <sstream>
#include "filter.h"
using namespace std;

CResFilter::CResFilter(CConfiguration *cParam) : m_cPara(cParam),
	m_nRawNum(0), m_lfThreshold(0)
{
	m_clog = Clog::getInstance();
}

CResFilter::CResFilter(CConfiguration *cParam, shared_ptr<Summary> _pSum) : m_cPara(cParam), m_pSummary(_pSum), m_nRawNum(int(cParam->m_vSpecFiles.size())), m_lfThreshold(cParam->m_lfThreshold)
{
	m_clog = Clog::getInstance();
}
CResFilter::~CResFilter()
{
	if (m_cPara){
		m_cPara = NULL;
	}
	if (m_clog){
		m_clog = NULL;
	}
}

void CResFilter::computeQvalues(vector<PRSM> &m_vRes)
{
	if (m_vRes.size() == 0)
	{
		m_clog->error("There is no result, nothing to be filtered!");
		return;
	}
	vector<double> evalFDP;
	double decoynum = 0, targetnum = 0.001;
	for (size_t i = 0; i < m_vRes.size(); ++i)
	{    // sort by evalue, the smaller the better
		if (m_vRes[i].nIsDecoy == 1) decoynum += 1;
		else targetnum += 1;
		evalFDP.push_back(1.0 * decoynum / targetnum);
	}

	int j = evalFDP.size() - 1;
	double filter_score = 0, minFDR = DBL_MAX;
	for (; j >= 0 && evalFDP[j] > m_lfThreshold; --j)
	{   // [wrm] isDecoy = 1表示反库结果，为0表示正库过阈值的结果，为-1表示正库未过阈值的结果  modified by wrm 2015.11.26
		if (m_vRes[j].nIsDecoy == 0){
			m_vRes[j].nIsDecoy = -1;
		}
		minFDR = min(evalFDP[j], minFDR);
		m_vRes[j].lfQvalue = minFDR;
	}
	if (j >= 0)
	{
		m_clog->info("Filter Score: %f", m_vRes[j].lfScore); //过滤阈值
	}
	else {
		m_clog->info("Get nothing!");
	}
	for (; j >= 0; --j){
		minFDR = min(evalFDP[j], minFDR);
		m_vRes[j].lfQvalue = minFDR;
	}
}

int CResFilter::_FindFirstOf(char *buf, char ch, int start)
{
	int i = start;
	for (; buf[i] != '\0'; ++i)
	{
		if (ch == buf[i])
			return i;
	}
	if (i > start)  return i;
	return -1;
}

// 从搜索的结果文件中导入信息，m_vRes保存每个PSM的信息
void CResFilter::GetSearchRes(const string &ResPath, vector<PRSM> &m_vRes)
{
	FILE *pfile = fopen(ResPath.c_str(), "rb");
	if (pfile == NULL)
	{
		m_clog->error("Failed to open file: " + ResPath);
		return;
	}

	fseek(pfile, 0, SEEK_END);
	int filesize = ftell(pfile);
	//cout << filesize << endl;
	try{
		char *buf = new char[filesize + 1];
		fseek(pfile, 0, SEEK_SET);
		fread(buf, sizeof(char), filesize, pfile);
		fclose(pfile);
		buf[filesize] = '\0';
		Parse(buf, m_vRes);
		delete[]buf;
	}
	catch (bad_alloc &ba) {
		m_clog->error(ba.what());
		m_clog->error("The result file is too large, could not load in!");
		exit(-1);
	}
}

void CResFilter::GetSearchRes(const string &ResPath, unordered_map<string, PRSM> &q_Res)
{
	FILE *pfile = fopen(ResPath.c_str(), "rb");   // 此处必须是"rb"，才会原封不动地读取文件
	if (pfile == NULL)
	{
		m_clog->error("Failed to open file: " + ResPath);
		exit(1);
	}
	try{
		fseek(pfile, 0L, SEEK_END);
		long filesize = ftell(pfile);
		//cout << filesize << endl;
		char *buf = new char[filesize + 1];
		rewind(pfile);
		fread(buf, sizeof(char), filesize, pfile);
		fclose(pfile);
		buf[filesize] = '\0';
		Parse(buf, q_Res);
		delete[]buf;
	}
	catch (bad_alloc &ba) {
		m_clog->error(ba.what());
		m_clog->error("The result file is too large, could not load in!");
		exit(1);
	}
}

void CResFilter::GetSearchRes(const string &ResPath, unordered_map<string, priority_queue<PRSM, vector<PRSM>, greater<PRSM> >> &q_Res)
{
	FILE *pfile = fopen(ResPath.c_str(), "rb");   // 此处必须是"rb"，才会原封不动地读取文件
	if (pfile == NULL)
	{
		m_clog->error("Failed to open file: " + ResPath);
		exit(1);
	}
	try{
		fseek(pfile, 0L, SEEK_END);
		long filesize = ftell(pfile);
		//cout << filesize << endl;
		char *buf = new char[filesize + 1];
		rewind(pfile);
		fread(buf, sizeof(char), filesize, pfile);
		fclose(pfile);
		buf[filesize] = '\0';
		Parse(buf, q_Res);
		delete[]buf;
	}
	catch (bad_alloc &ba) {
		m_clog->error(ba.what());
		m_clog->error("The result file is too large, could not load in!");
		exit(1);
	}
}

bool CResFilter::parseResLine(const char *line, const PRSM_Column_Index &cols, PRSM &prsm)
{
	
	vector<string> splitWords;
	CStringProcess::Split(line, ",", splitWords);
	
	try
	{
		if (splitWords.size() > 17)
		{
			prsm.nfileID = atoi(splitWords[cols.fileID_col].c_str());
			prsm.strSpecTitle = splitWords[cols.title_col];
			prsm.nScan = atoi(splitWords[cols.scan_col].c_str());
			prsm.nCharge = atoi(splitWords[cols.charge_col].c_str());
			prsm.lfPrecursorMass = atof(splitWords[cols.precusor_mass_col].c_str());  // 谱图母离子质量 
			prsm.lfProMass = atof(splitWords[cols.theoretical_mass_col].c_str());   // 理论母离子质量

			prsm.lfScore = atof(splitWords[cols.score_col].c_str());
			prsm.lfEvalue = atof(splitWords[cols.evalue_col].c_str());
			prsm.nMatchedPeakNum = atoi(splitWords[cols.matched_peaks_col].c_str());
			prsm.strProAC = splitWords[cols.protein_ac_col];
			prsm.strProSQ = splitWords[cols.protein_sequence_col];
			prsm.vModInfo = splitWords[cols.ptms_col];
			prsm.nIsDecoy = atoi(splitWords[cols.isDecoy_col].c_str());
			prsm.nLabelType = atoi(splitWords[cols.label_col].c_str());

			prsm.cMatchedInfo.nNterm_matched_ions = atoi(splitWords[cols.nmatched_ions_col].c_str());
			prsm.cMatchedInfo.nCterm_matched_ions = atoi(splitWords[cols.cmatched_ions_col].c_str());
			prsm.cMatchedInfo.lfNtermMatchedIntensityRatio = atof(splitWords[cols.nmatched_intensity_col].c_str());
			prsm.cMatchedInfo.lfCtermMatchedIntensityRatio = atof(splitWords[cols.cmatched_intensity_col].c_str());

			if (cols.com_ion_col > -1){
				prsm.cFeatureInfo.lfCom_ions_ratio = atof(splitWords[cols.com_ion_col].c_str());
				prsm.cFeatureInfo.lfTag_ratio = atof(splitWords[cols.tag_ratio_col].c_str());
				prsm.cFeatureInfo.lfPTM_score = atof(splitWords[cols.ptm_score_col].c_str());
				prsm.cFeatureInfo.lfFragment_error_std = atof(splitWords[cols.fragment_error_col].c_str());
			}
			return true;
		}
	}
	catch (bad_alloc &ba) 
	{
		CErrInfo info("CResFilter", "parseResLine", ba.what());
		throw runtime_error(info.Get().c_str());
		return false;
	}
	catch (exception & e)
	{
		CErrInfo info("CMainFlow", "parseResLine", "");
		throw runtime_error(info.Get(e).c_str());
		return false;
	}
	catch (...)
	{
		CErrInfo info("CMainFlow", "parseResLine", "caught an unknown exception from Parse.");
		throw runtime_error(info.Get().c_str());
		return false;
	}
	return false;
}

void CResFilter::Parse(char *buf, vector<PRSM> &m_vRes)
{
	string title, proSQ, modForm, proAC;
	double specMass, proMass, score, evalue;
	int sIdx = 0;
	int fileId = 0, scan, matchedPeaks, isDecoy, charge, labeltype;
	int nterm_matched_ions = 0, cterm_matched_ions = 0;
	double nterm_matched_intensity = 0, cterm_matched_intensity = 0;
	int len = strlen(buf);
	int pStart = _FindFirstOf(buf, '\n', 1) + 1; //Skip the title
	int pEnd = _FindFirstOf(buf, '\n', pStart);

	PRSM_Column_Index cols;
	char *tmpline = new char[pStart];
	strncpy(tmpline, buf, pStart - 1);
	tmpline[pStart - 1] = '\0';
	CConfiguration::parseFileTitle(tmpline, ",", cols);
	delete[]tmpline;

	const int LINE_LEN = 20000;
	char *line = new char[LINE_LEN];
	while (pStart < pEnd)
	{
		strncpy(line, buf + pStart, pEnd - pStart);
		line[pEnd - pStart] = '\0';
		if (line[pEnd - pStart - 1] == '\n' || line[pEnd - pStart - 1] == '\r'){
			line[pEnd - pStart - 1] = '\0';
		}
		
		PRSM prsm;
		if (parseResLine(line, cols, prsm))
		{
			m_vRes.push_back(prsm);
		}

		pStart = pEnd + 1;
		pEnd = _FindFirstOf(buf, '\n', pStart);
	}
	delete[]line;

#ifdef _DEBUG_TEST
	cout << pStart << " " << pEnd << endl;
	cout << m_vRes.size() << endl;
	for (size_t i = 0; i < m_vRes.size(); ++i)
	{
		cout << m_vRes[i].strSpecTitle << endl;
	}
#endif
}

void CResFilter::Parse(char *buf, unordered_map<string, PRSM> &q_Res)
{

	int pStart = _FindFirstOf(buf, '\n', 1) + 1; //Skip the title
	int pEnd = _FindFirstOf(buf, '\n', pStart);
	//istringstream iss;

	PRSM_Column_Index cols;
	char *tmpline = new char[pStart];
	strncpy(tmpline, buf, pStart - 1);
	tmpline[pStart - 1] = '\0';
	CConfiguration::parseFileTitle(tmpline, ",", cols);
	delete[]tmpline;

	const int LINE_LEN = 20000;
	char *line = new char[LINE_LEN];
	while (pStart < pEnd)
	{
		strncpy(line, buf + pStart, pEnd - pStart);
		line[pEnd - pStart] = '\0';
		if (line[pEnd - pStart - 1] == '\n' || line[pEnd - pStart - 1] == '\r')
		{
			line[pEnd - pStart - 1] = '\0';
		}
		
		PRSM prsm;
		if (parseResLine(line, cols, prsm))
		{
			if (q_Res.find(prsm.strSpecTitle) == q_Res.end())
			{
				q_Res.insert(make_pair(prsm.strSpecTitle, prsm));
			}
			else if (q_Res[prsm.strSpecTitle].lfEvalue > prsm.lfEvalue)   // more believable
			{
				q_Res[prsm.strSpecTitle] = prsm;
			}
		}

		pStart = pEnd + 1;
		pEnd = _FindFirstOf(buf, '\n', pStart);
	}
}

void CResFilter::Parse(char *buf, unordered_map<string, priority_queue<PRSM, vector<PRSM>, greater<PRSM>>> &q_Res)
{

	int pStart = _FindFirstOf(buf, '\n', 1) + 1; //Skip the title
	int pEnd = _FindFirstOf(buf, '\n', pStart);
	//istringstream iss;

	PRSM_Column_Index cols;
	char *tmpline = new char[pStart];
	strncpy(tmpline, buf, pStart - 1);
	tmpline[pStart - 1] = '\0';
	CConfiguration::parseFileTitle(tmpline, ",", cols);
	delete[]tmpline;

	const int LINE_LEN = 20000;
	char *line = new char[LINE_LEN];
	while (pStart < pEnd)
	{
		strncpy(line, buf + pStart, pEnd - pStart);
		line[pEnd - pStart] = '\0';
		if (line[pEnd - pStart - 1] == '\n' || line[pEnd - pStart - 1] == '\r')
		{
			line[pEnd - pStart - 1] = '\0';
		}

		PRSM prsm;
		if (parseResLine(line, cols, prsm))
		{
			if (q_Res.find(prsm.strSpecTitle) == q_Res.end())
			{
				priority_queue<PRSM, vector<PRSM>, greater<PRSM>> que;
				que.push(prsm);
				q_Res.insert(make_pair(prsm.strSpecTitle, que));
			}
			else
			{
				if (q_Res[prsm.strSpecTitle].size() < m_cPara->m_nOutputTopK)   // more believable
				{
					q_Res[prsm.strSpecTitle].push(prsm);
				}
				else if (q_Res[prsm.strSpecTitle].top().lfEvalue > prsm.lfEvalue)
				{
					q_Res[prsm.strSpecTitle].pop();
					q_Res[prsm.strSpecTitle].push(prsm);
				}
			}
		}
		pStart = pEnd + 1;
		pEnd = _FindFirstOf(buf, '\n', pStart);
	}
}


// genenrate csv file
void CResFilter::reportFilteredSpectra(const string &resPath, vector<PRSM> &m_vRes)
{
	try
	{
		int nIdSpecNum = 0;
		FILE *freport = fopen(resPath.c_str(), "w");
		if (freport == NULL)
		{
			m_clog->error("Failed to open file: " + resPath);
			exit(-1);
		}
		fprintf(freport, "FileID,Title,Scan,Precursor Rank,Charge State,Precursor MZ,Precursor Mass,Theoretical Mass,Mass Diff Da,Mass Diff PPM,");
		fprintf(freport, "Protein AC,Protein Sequence,PTMs,Matched Peaks,Nterm Matched Ions,Cterm Matched Ions,Nterm Matched Intensity Ratio,Cterm Matched Intensity Ratio,Raw Score,Final Score,Label Type\n");
		for (int i = 0; i < (int)m_vRes.size(); ++i)
		{
			if (m_vRes[i].nIsDecoy == 0)
			{
				++nIdSpecNum;
				int precursor_id = 0;
				int fNo = m_vRes[i].nfileID;
				int scan = m_vRes[i].nScan;
				if (m_vIdScans[fNo].find(scan) == m_vIdScans[fNo].end()){
					m_vIdScans[fNo].insert(make_pair(scan, 1));
				}
				else{
					++m_vIdScans[fNo][scan];
				}
				vector<string> splitElems;
				CStringProcess::Split(m_vRes[i].strSpecTitle, ".", splitElems);

				if (splitElems.size() < 6 || splitElems[(int)splitElems.size() - 1] != "dta") // 不规范的mgf文件，无法解析scan号和precursor rank [TODO?]
				{
				}
				else {
					precursor_id = atoi(splitElems[(int)splitElems.size() - 2].c_str());
				}
				double dist = m_vRes[i].lfPrecursorMass - m_vRes[i].lfProMass;
				int charge = m_vRes[i].nCharge;
				double mz = (m_vRes[i].lfPrecursorMass + (charge - 1)*IonMass_Proton) / charge;
				fprintf(freport, "%u,%s,%d,%d,%d,", m_vRes[i].nfileID, m_vRes[i].strSpecTitle.c_str(), m_vRes[i].nScan, precursor_id, charge);
				fprintf(freport, "%f,%f,%f,%.3f,%.1f,", mz, m_vRes[i].lfPrecursorMass, m_vRes[i].lfProMass, dist, 1000000 * dist / m_vRes[i].lfProMass);
				fprintf(freport, "%s,%s,%s,%d,%d,%d,%.3f,%.3f,%.2f,%e,%u\n", m_vRes[i].strProAC.c_str(), m_vRes[i].strProSQ.c_str(), m_vRes[i].vModInfo.c_str(),
					m_vRes[i].nMatchedPeakNum, m_vRes[i].cMatchedInfo.nNterm_matched_ions, m_vRes[i].cMatchedInfo.nCterm_matched_ions,
					m_vRes[i].cMatchedInfo.lfNtermMatchedIntensityRatio, m_vRes[i].cMatchedInfo.lfCtermMatchedIntensityRatio,
					m_vRes[i].lfScore, m_vRes[i].lfEvalue, m_vRes[i].nLabelType);
			}
		}
		fclose(freport);
		m_clog->info("PrSM number: %d (%s)", nIdSpecNum, resPath.c_str());
	}
	catch (exception & e)
	{
		CErrInfo info("CResFilter", "reportFilteredSpectra", "");
		throw runtime_error(info.Get(e).c_str());
	}
	catch (...)
	{
		CErrInfo info("CResFilter", "reportFilteredSpectra", "caught an unknown exception from generating report.");
		throw runtime_error(info.Get().c_str());
	}
}

void CResFilter::OutputLabelRes(const string &plabelFile, vector<PRSM> &m_vRes)
{
	try{
		if (m_vRes.empty()){
			m_clog->warning("No result to write into " + plabelFile);
			return;
		}
		FILE * fp = fopen(plabelFile.c_str(), "w");
		if (fp == NULL){
			m_clog->error("Fail to create plabel file " + plabelFile);
			return;
		}
		vector<MODINI> &fixmodInfo = m_cPara->m_vFixModInfo;
		//vector<MODINI> &varmodInfo = m_cPara->m_vVarModInfo;
		unordered_set<string> varModset;
		unordered_map<string, int> varModmap;
		vector<LABEL_ITEM> vCurrResFile;
		// first scan, get mod information
		for (size_t i = 0; i < m_vRes.size(); ++i)
		{
			if (m_vRes[i].nIsDecoy != 0) continue;
			vector<int> sites, ids;
			if (m_vRes[i].vModInfo.compare(NONMOD) != 0)
			{
				size_t pStart = 1;
				while (pStart < m_vRes[i].vModInfo.length())
				{
					size_t pSite = m_vRes[i].vModInfo.find_first_of(')', pStart);
					size_t pEnd = m_vRes[i].vModInfo.find_first_of(';', pSite);
					int site = atoi(m_vRes[i].vModInfo.substr(pStart, pSite - pStart).c_str());
					string modName = m_vRes[i].vModInfo.substr(pSite + 1, pEnd - pSite - 1);
					varModset.insert(modName);
					pStart = pEnd + 2;
				}
			}
		}
		int id = 1;
		for (auto it = varModset.begin(); it != varModset.end(); ++it)
		{
			varModmap.insert(make_pair(*it, id++));
		}
		// second scan, get records
		int var_mod_num = varModmap.size();
		for (size_t i = 0; i < m_vRes.size(); ++i)
		{
			if (m_vRes[i].nIsDecoy != 0) continue;
			vector<int> sites, ids;
			if (m_vRes[i].vModInfo.compare(NONMOD) != 0)
			{
				size_t pStart = 1;
				while (pStart < m_vRes[i].vModInfo.length())
				{
					size_t pSite = m_vRes[i].vModInfo.find_first_of(')', pStart);
					size_t pEnd = m_vRes[i].vModInfo.find_first_of(';', pSite);
					int site = atoi(m_vRes[i].vModInfo.substr(pStart, pSite - pStart).c_str());
					string modName = m_vRes[i].vModInfo.substr(pSite + 1, pEnd - pSite - 1);
					sites.push_back(site);
					ids.push_back(varModmap[modName]);
					pStart = pEnd + 2;
				}
			}
			// 处理固定修饰
			for (size_t midx = 0; midx < fixmodInfo.size(); ++midx)
			{
				if (MOD_TYPE::MOD_PRO_N == fixmodInfo[midx].cType)
				{
					sites.push_back(0);
					ids.push_back(var_mod_num + midx + 1);
					continue;
				}
				else if (MOD_TYPE::MOD_PRO_C == fixmodInfo[midx].cType) {
					sites.push_back(m_vRes[i].strProSQ.length() + 1);
					ids.push_back(var_mod_num + midx + 1);
					continue;
				}
				for (size_t pos = 0; pos < m_vRes[i].strProSQ.length(); ++pos)
				{
					if (fixmodInfo[midx].strSite.find(m_vRes[i].strProSQ[pos]) != string::npos)
					{
						sites.push_back(pos + 1);
						ids.push_back(var_mod_num + midx + 1);
					}
				}
			}

			LABEL_ITEM tmpRes(m_vRes[i].lfScore, m_vRes[i].strSpecTitle, m_vRes[i].strProSQ, sites, ids);
			vCurrResFile.push_back(tmpRes);
		}
		int fileidx = m_vRes[0].nfileID;
		string mgfFilename = m_cPara->m_vSpecFiles[fileidx];
		size_t found = mgfFilename.find_last_of('.');
		if (found == string::npos)
		{
			throw("Invalid mgf file!");
		}
		string suffix = mgfFilename.substr(found + 1);
		if (suffix.compare("pf2") == 0)
		{
			mgfFilename.erase(mgfFilename.begin() + found + 1, mgfFilename.end());
			mgfFilename.append("mgf");
		}

		fprintf(fp, "[FilePath]\nFile_Path=%s\n", mgfFilename.c_str());
		fprintf(fp, "[Modification]\n");
		id = 1;
		for (auto it = varModset.begin(); it != varModset.end(); ++it)
		{
			fprintf(fp, "%d=%s\n", id++, (*it).c_str());
		}
		for (size_t i = 0; i < fixmodInfo.size(); ++i)
		{
			fprintf(fp, "%d=%s\n", var_mod_num + i + 1, fixmodInfo[i].strName.c_str());
		}
		fprintf(fp, "[xlink]\nxlink=NULL\n[Total]\n");
		fprintf(fp, "total=%d\n", vCurrResFile.size());
		for (size_t s = 0; s < vCurrResFile.size(); ++s)
		{
			string title = "";
			for (size_t i = 0; i < vCurrResFile[s].strTitle.size(); ++i)
			{
				if (vCurrResFile[s].strTitle[i] >= 'a' && vCurrResFile[s].strTitle[i] <= 'z')
					title.push_back(vCurrResFile[s].strTitle[i] - 'a' + 'A');
				else title.push_back(vCurrResFile[s].strTitle[i]);
			}
			fprintf(fp, "[Spectrum%d]\nname=%s\n", s + 1, title.c_str());
			fprintf(fp, "pep1=0 %s %f ", vCurrResFile[s].strSeq.c_str(), vCurrResFile[s].lfScore);

			if (vCurrResFile[s].vmodIds.size() > 0)
			{
				for (size_t mIdx = 0; mIdx < vCurrResFile[s].vmodIds.size(); ++mIdx)
				{
					fprintf(fp, "%d,%d ", vCurrResFile[s].vmodSites[mIdx], vCurrResFile[s].vmodIds[mIdx]);
				}
				fprintf(fp, "\n");
			}
			else {
				fprintf(fp, "\n");
			}
		}
		fclose(fp);
	}
	catch (exception & e) {
		CErrInfo info("CResFilter", "OutputLabelRes", "Output pLabel file failed.");
		m_clog->info(info.Get(e).c_str());
		throw runtime_error(info.Get(e).c_str());
	}
	catch (...) {
		CErrInfo info("CResFilter", "OutputLabelRes", "caught an unknown exception from OutputLabelRes.");
		m_clog->info(info.Get().c_str());
		throw runtime_error(info.Get().c_str());
	}
}

// [to be removed]
//void CResFilter::FirstReport(int fileType, vector<string> &vIdSpec, vector<PROTEIN_STRUCT> &proteinDB)
//{
//	bool isIdentified = true;
//	set<string> setProAC;
//	vector<SPEC_ID_TITLE> tmpIdSpec;
//    for(int i = 0; i < (int)m_vRes.size(); ++i)
//    {
//        if(m_vRes[i].nIsDecoy == 0)
//        {
//			if(isIdentified)
//			{
//				tmpIdSpec.push_back(SPEC_ID_TITLE(m_vRes[i].nIdx, m_vRes[i].strSpecTitle));
//			}
//			if(setProAC.find(m_vRes[i].strProAC) != setProAC.end())
//			{
//				continue;
//			}
//			setProAC.insert(m_vRes[i].strProAC);
//			PROTEIN_STRUCT tmpPro;
//			tmpPro.nIsDecoy = 0;
//			tmpPro.lfMass = 0.0; //结果里存的不是蛋白质序列的理论质量，而是序列加修饰的理论质量，所以这里到MainFlow中之后再算
//			tmpPro.strProAC = m_vRes[i].strProAC;
//			tmpPro.strProSQ = m_vRes[i].strProSQ;
//
//			proteinDB.push_back(tmpPro);
//
//			tmpPro.nIsDecoy = 1;
//			CStringProcess::Reverse(tmpPro.strProSQ);
//			proteinDB.push_back(tmpPro);
//
//		} else {
//			isIdentified = false; //一旦遇到一个反库结果就停止记录鉴定结果，控制0%的FDR
//		}
//    }
//	sort(tmpIdSpec.begin(), tmpIdSpec.end());
//	if(1 == fileType)
//	{
//		for(size_t sidx = 0; sidx < tmpIdSpec.size(); ++sidx)
//		{   // 当采用pf作为输入格式时，谱图的标题中存储的是title.scan.scan，因此这里保持一致
//			string tmpTitle = tmpIdSpec[sidx].strTitle;
//			int slen = tmpTitle.length();
//			int dotnum = 0;
//			for(int delidx = 1; delidx < slen; ++delidx)
//			{
//				if(tmpTitle[tmpTitle.size() - 1] == '.') // vIdSpec[ids].back() == '.'
//				{
//					++dotnum;
//				}
//				tmpTitle.erase(tmpTitle.end() - 1);
//				if(dotnum >= 3)
//				{
//					break;
//				}
//			}
//			if(0 == vIdSpec.size() || vIdSpec[vIdSpec.size() - 1].compare(tmpTitle) != 0)
//			{   // 删除scan重复项，目前不考虑混合谱的二次搜索
//				vIdSpec.push_back(tmpTitle);
//			}
//		}
//	} else {
//		for(size_t sidx = 0; sidx < tmpIdSpec.size(); ++sidx)
//		{
//			vIdSpec.push_back(tmpIdSpec[sidx].strTitle);
//		}
//	}
//}
//


void CResFilter::FirstFilter(const string strQryRst, double lfThreshold, unordered_set<int> &vIdSpec)
{
	unordered_map<string,PRSM> mapRes;
	m_clog->info("Fileter " + strQryRst);
	GetSearchRes(strQryRst, mapRes);  // get top one
	if (mapRes.size() == 0)
	{
		m_clog->info("There is no result, nothing to be filtered!");
		return;
	}
	vector<PRSM> vRes;
	vRes.reserve(mapRes.size());
	for (auto it = mapRes.begin(); it != mapRes.end(); ++it){
		vRes.push_back(it->second);
	}
	// sort by score
	sort(vRes.begin(), vRes.end(), PRSM::ScoreGreater);
	vector<double> evalFDP;
	double decoynum = 0, targetnum = 0.001;
	for (size_t i = 0; i < vRes.size(); ++i)
	{    // sort by evalue, the smaller the better
		if (vRes[i].nIsDecoy == 1)  decoynum += 1;
		else targetnum += 1;
		evalFDP.push_back(decoynum / targetnum);
	}

	int j = evalFDP.size() - 1;
	double filter_score = 0, minFDR = DBL_MAX;
	for (; j >= 0 && evalFDP[j] > lfThreshold; --j)
	{   // [wrm] isDecoy = 1表示反库结果，为0表示正库过阈值的结果，为-1表示正库未过阈值的结果  modified by wrm 2015.11.26
		if (vRes[j].nIsDecoy == 0){
			vRes[j].nIsDecoy = -1;
		}
		minFDR = min(evalFDP[j], minFDR);
		vRes[j].lfQvalue = minFDR;
	}
	if (j >= 0)
	{
		m_clog->info("Filter Score: %f", vRes[j].lfScore); //过滤阈值
	}
	else {
		m_clog->info("Get nothing!");
	}
	for (; j >= 0; --j){
		vIdSpec.insert(vRes[j].nScan);
	}
}

// filter one by one
void CResFilter::SeparateFiltering(vector<string> &v_filePath)
{
	vector<PRSM> vResults;
	for (m_nResFileIdx = 0; m_nResFileIdx < m_nRawNum; ++m_nResFileIdx)
	{
		vector<PRSM> vRes;
		m_clog->info("Fileter " + v_filePath[m_nResFileIdx]);
		GetSearchRes(v_filePath[m_nResFileIdx], vRes);

		// sort by e-value
		sort(vRes.begin(), vRes.end());
		computeQvalues(vRes);

		int pos = v_filePath[m_nResFileIdx].find_last_of(cSlash);
		if (pos == string::npos){
			pos = 0;
		}
		string resPath = v_filePath[m_nResFileIdx].substr(0, pos) + "\\" + m_cPara->m_vstrFilenames[m_nResFileIdx] + "_filtered.csv";
		reportFilteredSpectra(resPath, vRes);
		string labelPath = v_filePath[m_nResFileIdx].substr(0, pos) + "\\" + m_cPara->m_vstrFilenames[m_nResFileIdx] + ".plabel";
		OutputLabelRes(labelPath, vRes);
		string specPath = v_filePath[m_nResFileIdx].substr(0, pos) + "\\" + m_cPara->m_vstrFilenames[m_nResFileIdx] + ".spectra";
		//WriteSpectraResult(specPath, vRes);
		m_clog->info("The results in " + resPath);
		vResults.insert(vResults.end(), vRes.begin(), vRes.end());
	}
	string filteredSpecFile = m_cPara->m_strOutputPath + "\\pTop_filtered.spectra";
	WriteSpectraResult(filteredSpecFile, vResults, true);
	string specFile = m_cPara->m_strOutputPath + "\\pTop.spectra";
	WriteSpectraResult(specFile, vResults, false);
}

// merge search result
void CResFilter::MergeFiltering(vector<string> &v_filePath)
{
	m_clog->info("Merge the results of different raws...");
	vector<PRSM> m_vRes;
	// compute q-value for each raw
	for (m_nResFileIdx = 0; m_nResFileIdx < m_nRawNum; ++m_nResFileIdx){ 
		m_vRes.clear();
		GetSearchRes(v_filePath[m_nResFileIdx], m_vRes);
		// sort by e-value
		sort(m_vRes.begin(), m_vRes.end());
		computeQvalues(m_vRes);
	}
	// merge the result of multiple raw/mgf files into a .spectra file
	long long aboutNum = m_vRes.size()*(m_nRawNum + 1);
	m_vRes.clear();
	m_vRes.reserve(aboutNum);
	for (m_nResFileIdx = 0; m_nResFileIdx < m_nRawNum; ++m_nResFileIdx){  // [TODO?]这里一下子将所有RAW的结果加载进内存，数据量太大时可能会崩溃。建议使用64位系统
		GetSearchRes(v_filePath[m_nResFileIdx], m_vRes);
	}
	m_clog->info("Compute q-values...");
	// sort by q-value
	sort(m_vRes.begin(), m_vRes.end(), PRSM::ConfidentByQV);
	computeQvalues(m_vRes);
	string resPath = m_cPara->m_strOutputPath + "\\pTop_filtered.csv";
	reportFilteredSpectra(resPath, m_vRes);
	string filteredSpecFile = m_cPara->m_strOutputPath + "\\pTop_filtered.spectra";
	WriteSpectraResult(filteredSpecFile, m_vRes, true);
	string specFile = m_cPara->m_strOutputPath + "\\pTop.spectra";
	WriteSpectraResult(specFile, m_vRes, false);
}


void CResFilter::Run()
{
	try{
		m_nRawNum = (int)m_cPara->m_vSpecFiles.size();
		m_vIdScans.assign(m_nRawNum, unordered_map<int, byte>());
		vector<string> v_filePath;
		//if there're labels, first merge the result of multiple labels
		if (m_cPara->m_quant.size() > 1){
			MergeResultsofDiffLabels(v_filePath);
		}
		else{
			for (m_nResFileIdx = 0; m_nResFileIdx < m_nRawNum; ++m_nResFileIdx){  //
				v_filePath.push_back(m_cPara->m_vstrQryResFile[m_nResFileIdx][0]);
			}
		}
		if (m_cPara->m_bSeparateFiltering){
			SeparateFiltering(v_filePath);
		}
		else{
			MergeFiltering(v_filePath);
		}
		reportSummary();
		m_clog->info("Complete filter and report.");
		// deleteTmpFiles(v_filePath);
	}
	catch (exception & e) {
		CErrInfo info("CResFilter", "Run", "run filter failed.");
		throw runtime_error(info.Get(e).c_str());
	}
	catch (...) {
		CErrInfo info("CResFilter", "Run", "caught an unknown exception from runing filter.");
		throw runtime_error(info.Get().c_str());
	}
}

void CResFilter::MergeResultsofDiffLabels(vector<string> &filePath)
{
	m_clog->info("Merge the results of different labels...");
	for (m_nResFileIdx = 0; m_nResFileIdx < m_nRawNum; ++m_nResFileIdx){
		unordered_map<string, PRSM> q_Result;
		for (int i = 0; i< m_cPara->m_vstrQryResFile[m_nResFileIdx].size(); ++i){
			GetSearchRes(m_cPara->m_vstrQryResFile[m_nResFileIdx][i], q_Result);
		}
		string dir = m_cPara->m_vstrQryResFile[m_nResFileIdx][0];
		int pos = m_cPara->m_vstrQryResFile[m_nResFileIdx][0].find_last_of(cSlash);
		dir = dir.substr(0, pos);
		string resPath = dir + "\\" + m_cPara->m_vstrFilenames[m_nResFileIdx] + ".qry.csv";
		filePath.push_back(resPath);
		FILE *fp = fopen(resPath.c_str(), "wb");
		if (fp == NULL){
			m_clog->error("Failed to create file: " + resPath);
			exit(1);
		}
		char *buf = new char[BUFFERSIZE];
		size_t len = 0;
		len += sprintf(buf + len, "FileID,Title,Scan,Charge,Precursor Mass,Theoretical Mass,Mass Diff Da,Mass Diff PPM,");
		len += sprintf(buf + len, "Protein AC,Protein Sequence,PTMs,Raw Score,Final Score,Matched Peaks,");
		len += sprintf(buf + len, "Nterm Matched Ions,Cterm Matched Ions,Nterm Matched Intensity Ratio,Cterm Matched Intensity Ratio,isDecoy,Label Type\n");
		unordered_map<string, PRSM>::iterator it;
		for (it = q_Result.begin(); it != q_Result.end(); ++it){
			len += sprintf(buf + len, "%u,%s,%d,%d,", it->second.nfileID, it->second.strSpecTitle.c_str(), it->second.nScan, it->second.nCharge);
			double dist = it->second.lfPrecursorMass - it->second.lfProMass;
			len += sprintf(buf + len, "%f,%f,%.3f,%.1f,", it->second.lfPrecursorMass, it->second.lfProMass, dist, dist * 1000000 / it->second.lfProMass);
			len += sprintf(buf + len, "%s,%s,%s,%f,%e,", it->second.strProAC.c_str(), it->second.strProSQ.c_str(), it->second.vModInfo.c_str(), it->second.lfScore, it->second.lfEvalue);
			len += sprintf(buf + len, "%d,%d,%d,%.3f,%.3f,", it->second.nMatchedPeakNum, it->second.cMatchedInfo.nNterm_matched_ions, it->second.cMatchedInfo.nCterm_matched_ions,
				it->second.cMatchedInfo.lfNtermMatchedIntensityRatio, it->second.cMatchedInfo.lfCtermMatchedIntensityRatio);
			len += sprintf(buf + len, "%d,%u\n", it->second.nIsDecoy, it->second.nLabelType);
			if (len > (BUFFERSIZE - 20000)){
				buf[len] = '\0';
				fwrite(buf, sizeof(char), len, fp);
				len = 0;
			}
		}
		if (len){
			buf[len] = '\0';
			fwrite(buf, sizeof(char), len, fp);
		}
		delete[]buf;
		fclose(fp);
	}
}

void CResFilter::MergeQryResult(vector<string> &vFilePath, const string &strQryRstFile)
{
	unordered_map<string, priority_queue<PRSM, vector<PRSM>, greater<PRSM> >> q_Result;
	for (int i = 0; i< vFilePath.size(); ++i){
		GetSearchRes(vFilePath[i], q_Result);
	}
	m_clog->info("Identified %d spectra.", q_Result.size());

	FILE *fp = fopen(strQryRstFile.c_str(), "wb");
	if (fp == NULL){
		m_clog->error("Failed to create file: " + strQryRstFile);
		exit(1);
	}
	char *buf = new char[BUFFERSIZE];
	size_t len = 0;
	len += sprintf(buf + len, "FileID,Title,Scan,Charge,Precursor Mass,Theoretical Mass,Mass Diff Da,Mass Diff PPM,");
	len += sprintf(buf + len, "Protein AC,Protein Sequence,PTMs,Raw Score,Final Score,Matched Peaks,");
	len += sprintf(buf + len, "Nterm Matched Ions,Cterm Matched Ions,Nterm Matched Intensity Ratio,Cterm Matched Intensity Ratio,");
	len += sprintf(buf + len, "Com_Ion_Ratio,Tag_Ratio,PTM_Score,Fragment_Error_STD,isDecoy,Label Type\n");
	for (auto it = q_Result.begin(); it != q_Result.end(); ++it){
		while (!it->second.empty()){
			PRSM &prsm = it->second.top();
			len += sprintf(buf + len, "%u,%s,%d,%d,", prsm.nfileID, prsm.strSpecTitle.c_str(), prsm.nScan, prsm.nCharge);
			double dist = prsm.lfPrecursorMass - prsm.lfProMass;
			len += sprintf(buf + len, "%f,%f,%.3f,%.1f,", prsm.lfPrecursorMass, prsm.lfProMass, dist, dist * 1000000 / prsm.lfProMass);
			len += sprintf(buf + len, "%s,%s,%s,%f,%e,", prsm.strProAC.c_str(), prsm.strProSQ.c_str(), prsm.vModInfo.c_str(), prsm.lfScore, prsm.lfEvalue);
			len += sprintf(buf + len, "%d,%d,%d,%.3f,%.3f,", prsm.nMatchedPeakNum, prsm.cMatchedInfo.nNterm_matched_ions, prsm.cMatchedInfo.nCterm_matched_ions,
				prsm.cMatchedInfo.lfNtermMatchedIntensityRatio, prsm.cMatchedInfo.lfCtermMatchedIntensityRatio);
			len += sprintf(buf + len, "%f,%f,%f,%f,", prsm.cFeatureInfo.lfCom_ions_ratio,
				prsm.cFeatureInfo.lfTag_ratio,
				prsm.cFeatureInfo.lfPTM_score,
				prsm.cFeatureInfo.lfFragment_error_std);
			len += sprintf(buf + len, "%d,%u\n", prsm.nIsDecoy, prsm.nLabelType);
			if (len > (BUFFERSIZE - 20000)){
				buf[len] = '\0';
				fwrite(buf, sizeof(char), len, fp);
				len = 0;
			}
			it->second.pop();
		}		
	}
	if (len){
		buf[len] = '\0';
		fwrite(buf, sizeof(char), len, fp);
	}
	delete[]buf;
	fclose(fp);
}

void CResFilter::WriteSpectraResult(const string &specFile, vector<PRSM> &m_vRes, bool filtered)
{
	sort(m_vRes.begin(), m_vRes.end(), PRSM::ConfidentByQV);
	vector<LABEL_QUANT> &m_quant = m_cPara->m_quant;
	try{
		FILE *fspec = fopen(specFile.c_str(), "wb");
		if (fspec == NULL){
			m_clog->error("Failed to create file: " + specFile);
			exit(1);
		}
		// get fix mods information
		vector<MODINI> &fixmodInfo = m_cPara->m_vFixModInfo;
		vector<string> A_mods(26, "");
		string Nterm = "", Cterm = "";
		for (int i = 0; i<fixmodInfo.size(); ++i){
			if (MOD_TYPE::MOD_PRO_N == fixmodInfo[i].cType)
			{
				Nterm = fixmodInfo[i].strName;
				continue;
			}
			else if (MOD_TYPE::MOD_PRO_C == fixmodInfo[i].cType) {
				Cterm = fixmodInfo[i].strName;
				continue;
			}
			else{
				for (int j = 0; j<fixmodInfo[i].strSite.size(); ++j){
					A_mods[fixmodInfo[i].strSite[j] - 'A'] = fixmodInfo[i].strName;
				}
			}
		}
		//Title,Scan,Precusor Mass,Charge State,FDR,Sequence,Theory Mass,Mass Diff Da,Score,Evalue,PTMs,00,Protein AC,(0,K,K/),labelType,isDecoy,0 
		char *buf = new char[BUFFERSIZE];
		int len = 0;
		len += sprintf(buf + len, "File_Name\tScan_No\tExp.MH+\tCharge\tQ-value\tSequence\tCalc.MH+\tMass_Shift(Exp-Cal)\tRaw_Score\tFinal_Score\t");
		len += sprintf(buf + len, "Modification\tSpecificity\tProtein.ACs\tPositions\tLabel\tTarget/Decoy\tMiss.Clv.Sites\tAvg.Frag.Mass.Shift\tOthers\n");
		for (int i = 0; i < (int)m_vRes.size(); ++i){
			if (filtered && m_vRes[i].nIsDecoy != 0){  // only output those pass the FDR threshold
				continue;
			}
			double dist = m_vRes[i].lfPrecursorMass - m_vRes[i].lfProMass;
			string targetDecoy = (m_vRes[i].nIsDecoy == 1) ? "decoy" : "target";
			string proSQ = m_vRes[i].strProSQ;
			string ptms = "";
			string labelInfo = "";    // [wrm]
			int labelIdx = m_vRes[i].nLabelType;
			labelInfo.push_back('0' + labelIdx);
			labelInfo.push_back('|');
			if (m_vRes[i].vModInfo.compare(NONMOD) != 0){  // var mods string
				vector<string> vmods;
				CStringProcess::Split(m_vRes[i].vModInfo, ";", vmods);
				if (m_quant.empty()){
					for (int mi = 0; mi<vmods.size(); ++mi){
						if (vmods[mi].length() == 0)  continue;
						size_t lp = vmods[mi].find('(');
						size_t rp = vmods[mi].find(')');
						if (lp != string::npos && rp != string::npos){
							ptms.append(vmods[mi].substr(lp + 1, rp - lp - 1) + "," + vmods[mi].substr(rp + 1) + ";");
						}
						labelInfo.append("0|");
					}
				}
				else{  // has label info
					for (int mi = 0; mi<vmods.size(); ++mi){
						if (vmods[mi].length() == 0)  continue;
						size_t lp = vmods[mi].find('(');
						size_t rp = vmods[mi].find(')');
						string modname = vmods[mi].substr(rp + 1);
						if (lp != string::npos && rp != string::npos){
							ptms.append(vmods[mi].substr(lp + 1, rp - lp - 1) + "," + modname + ";");
						}
						if (m_quant[labelIdx - 1].labelM_num > 0){
							if (m_quant[labelIdx - 1].quant_M.find(modname) == m_quant[labelIdx - 1].quant_M.end()){
								labelInfo.append("0|");
							}
							else{  // this mod is labeled
								labelInfo.push_back('0' + labelIdx);
								labelInfo.push_back('|');
							}
						}
						else{
							labelInfo.append("0|");
						}
					}
				}
			}
			string tmp;
			char ssite[10];
			if (Nterm != ""){
				tmp = "0," + Nterm + ";";
				ptms += tmp;
				if (m_quant.empty() || (m_quant[labelIdx - 1].quant_M.find(Nterm) == m_quant[labelIdx - 1].quant_M.end())){
					labelInfo.append("0|");
				}
				else{
					labelInfo.push_back('0' + labelIdx);
					labelInfo.push_back('|');
				}
			}
			if (Cterm != ""){
				itoa(m_vRes[i].strProSQ.size() + 1, ssite, 10);
				tmp = ssite;
				tmp.append("," + Cterm + ";");
				ptms += tmp;
				if (m_quant.empty() || (m_quant[labelIdx - 1].quant_M.find(Cterm) == m_quant[labelIdx - 1].quant_M.end())){
					labelInfo.append("0|");
				}
				else{
					labelInfo.push_back('0' + labelIdx);
					labelInfo.push_back('|');
				}
			}
			for (int site = 0; site<proSQ.size(); ++site){
				if (A_mods[proSQ[site] - 'A'] != ""){
					itoa(site + 1, ssite, 10);
					tmp = ssite;
					tmp.append("," + A_mods[proSQ[site] - 'A'] + ";");
					ptms += tmp;
					if (m_quant.empty() || (m_quant[labelIdx - 1].quant_M.find(A_mods[m_vRes[i].strProSQ[site] - 'A']) == m_quant[labelIdx - 1].quant_M.end())){
						labelInfo.append("0|");
					}
					else{
						labelInfo.push_back('0' + labelIdx);
						labelInfo.push_back('|');
					}
				}
			}

			len += sprintf(buf + len, "%s\t%d\t%f\t%d\t%g\t%s\t%f\t%f\t", m_vRes[i].strSpecTitle.c_str(), m_vRes[i].nScan, m_vRes[i].lfPrecursorMass, m_vRes[i].nCharge,
				m_vRes[i].lfQvalue, proSQ.c_str(), m_vRes[i].lfProMass, dist);
			len += sprintf(buf + len, "%f\t%e\t%s\t00\t%s/\t0,%c,%c/\t%s\t%s\t0\t0\t0\n", m_vRes[i].lfScore, m_vRes[i].lfEvalue, ptms.c_str(),
				m_vRes[i].strProAC.c_str(), proSQ[0], proSQ.back(), labelInfo.c_str(), targetDecoy.c_str());
			if (len > (BUFFERSIZE - 20000)){
				fwrite(buf, sizeof(char), len, fspec);
				len = 0;
			}
		}
		if (len){
			fwrite(buf, sizeof(char), len, fspec);
		}
		delete[]buf;
		fclose(fspec);
	}
	catch (exception & e) {
		CErrInfo info("CResFilter", "WriteSpectraResult", "WriteSpectraResult failed.");
		throw runtime_error(info.Get(e).c_str());
	}
	catch (...) {
		CErrInfo info("CResFilter", "WriteSpectraResult", "caught an unknown exception from WriteSpectraResult.");
		throw runtime_error(info.Get().c_str());
	}
}

void CResFilter::reportSummary()
{
	m_pSummary->totalIdScanNum = 0;
	m_pSummary->totalIdSpecNum = 0;
	m_pSummary->m_vIdScanNums.clear();
	m_pSummary->m_vMixSpecScanNums.clear();
	vector<byte> fScanRecords(m_nRawNum*MAXSCANNUM, 0);
	int MaxMix = 0;
	for (int i = 0; i < m_vIdScans.size(); ++i){
		m_pSummary->totalIdScanNum += m_vIdScans[i].size();
		m_pSummary->m_vIdScanNums.push_back(m_vIdScans[i].size());
		for (auto it = m_vIdScans[i].begin(); it != m_vIdScans[i].end(); ++it){
			m_pSummary->totalIdSpecNum += it->second;
			int fscan = i*MAXSCANNUM + it->first;
			fScanRecords[fscan] = it->second;
			if (fScanRecords[fscan] > MaxMix){
				MaxMix = fScanRecords[fscan];
			}
		}
	}
	m_pSummary->m_vMixSpecScanNums.resize(MaxMix + 1, 0);
	for (int i = 0; i < fScanRecords.size(); ++i){
		++m_pSummary->m_vMixSpecScanNums[fScanRecords[i]];
	}
	vector<byte>().swap(fScanRecords);
	m_clog->info("Total PrSM number: %d", m_pSummary->totalIdSpecNum);

	// ---------------------------------------------------
	string summaryFile = m_cPara->m_strOutputPath + "\\pTop.summary.txt";
	FILE *fsum = fopen(summaryFile.c_str(), "w");
	if (NULL == fsum){
		m_clog->error("Cannot create the summary file: " + summaryFile);
		exit(1);
	}
	char *buf = new char[BUFFERSIZE];
	int len = 0;
	len += sprintf(buf + len, "Spectra:\t%d\nScans:\t%d\n", m_pSummary->totalIdSpecNum, m_pSummary->totalIdScanNum);
	len += sprintf(buf + len, "---------------------\n");
	len += sprintf(buf + len, "Mixed Spectra of MS/MS Scans:\n");
	for (int i = 1; i <= MaxMix; ++i){
		len += sprintf(buf + len, "%d\t%d\t%.2f%%\n", i, m_pSummary->m_vMixSpecScanNums[i], m_pSummary->m_vMixSpecScanNums[i] * 100.0 / m_pSummary->totalIdScanNum);
	}
	len += sprintf(buf + len, "---------------------\n");
	len += sprintf(buf + len, "ID Rate:\n");
	int totalScanNum = 0;
	for (int i = 0; i<m_pSummary->m_vIdScanNums.size(); ++i){
		int posEnd = m_cPara->m_vSpecFiles[i].find_last_of('.');
		if (posEnd == string::npos){
			posEnd = m_cPara->m_vSpecFiles[i].size();
		}
		int posBeg = m_cPara->m_vSpecFiles[i].find_last_of(cSlash);
		if (posBeg == string::npos){
			posBeg = -1;
		}
		string fileName = m_cPara->m_vSpecFiles[i].substr(posBeg + 1, posEnd - posBeg - 1);
		len += sprintf(buf + len, "%s\t%d / %d = %.2f%%\n", fileName.c_str(), m_pSummary->m_vIdScanNums[i], m_pSummary->m_vScanNum[i], m_pSummary->m_vIdScanNums[i] * 100.0 / m_pSummary->m_vScanNum[i]);
		totalScanNum += m_pSummary->m_vScanNum[i];
	}
	len += sprintf(buf + len, "Overall\t%d / %d = %.2f%%\n", m_pSummary->totalIdScanNum, totalScanNum, m_pSummary->totalIdScanNum*100.0 / totalScanNum);
	fwrite(buf, sizeof(char), len, fsum);
	fclose(fsum);
	delete[]buf;

}

void CResFilter::deleteTmpFiles(vector<string> &v_filePath)  // 删除临时文件
{
	for (int i = 0; i<v_filePath.size(); ++i){
		remove(v_filePath[i].c_str());
	}
}