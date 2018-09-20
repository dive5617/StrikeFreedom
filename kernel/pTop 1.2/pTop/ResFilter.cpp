/*
 *  ResFilter.cpp
 *
 *  Created on: 2014-03-03
 *  Author: luolan
 */
#include <iostream>
#include <algorithm>
#include <string>
#include <sstream>
#include <windows.h>
#include "ResFilter.h"
#include "BasicTools.h"
using namespace std;

CResFilter::CResFilter(double lfFDR, const vector<string> &strRes, const vector<string> &strFilter, int dbNum):
	m_nDBnum(dbNum), m_nRawNum(0), m_nIdSpecNum(0), m_nResFileIdx(0), m_lfThreshold(lfFDR), m_strQryRes(strRes), m_strReport(strFilter)
{
	m_nRawNum = m_strQryRes.size();
	m_clog = Clog::getInstance();
}
CResFilter::~CResFilter()
{

}

void CResFilter::_Filter()
{
    if(m_vRes.size() == 0)
    {
        cout<<"[pTop] There is no result, nothing to be done!"<<endl;
		m_clog->alert("There is no result, nothing to be done!");
        return;
    }
	// sort by e-value
    sort(m_vRes.begin(), m_vRes.end());
    vector<double> evalFDP;
	int decoynum = 0, targetnum = 0;
	//for(int i = m_vRes.size() - 1; i >= 0; --i)   // sort by score from high to low
	for(size_t i = 0; i < m_vRes.size(); ++i)
	{    // sort by evalue, the smaller the better
		if(m_vRes[i].nIsDecoy == 1) ++decoynum;
		else ++targetnum;
		if(0 == targetnum)
		{
			evalFDP.push_back(100.0);
		} else {
			evalFDP.push_back(1.0 * decoynum / targetnum);
		}
	}

	int j = evalFDP.size() - 1;
	while(j >= 0 && evalFDP[j] > m_lfThreshold)
	{   // 之后的isDecoy = 1表示不是过滤后的结果，为0表示为过滤后的结果
	    m_vRes[j].nIsDecoy = 1;
	    --j;
	}
	if(j >= 0)
	{
		cout << "[pTop] Filter Score: " << m_vRes[j].lfScore << endl; //过滤阈值
		m_clog->info("Filter Score: %f",m_vRes[j].lfScore);
	} else {
		cout << "[pTop] Get nothing!" << endl; //没有任何过滤结果
		m_clog->info("Get nothing!");
	}

}

int CResFilter::_FindFirstOf(char *buf, char ch, int start, int len)
{
    for(int i = start; i < len; ++i)
    {
        if(ch == buf[i])
            return i;
    }
    return -1;
}

// 从搜索的结果文件中导入信息，m_vRes保存每个PSM的信息
void CResFilter::GetSearchRes()
{
	FILE *pfile = fopen(m_strQryRes[m_nResFileIdx].c_str(), "rb");
	if(pfile == NULL)
	{
		cout << "[Alert] Failed to open file: " << m_strQryRes[m_nResFileIdx] <<endl;
		m_clog->alert("Failed to open file: " + m_strQryRes[m_nResFileIdx]);
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
		Parse(buf);
		delete[]buf;
	} catch(bad_alloc &ba) {

		cout<<ba.what()<<endl;
		m_clog->alert(ba.what());
		cout<<"[Error] The result file is too large, could not load in!"<<endl;
		m_clog->alert("The result file is too large, could not load in!");
		exit(-1);
	}
}

void CResFilter::split(char* str, const char* c, vector<string> &words)
{
    char *p;
    p = strtok(str, c);
    while(p != NULL)
    {
        words.push_back(p);
        p = strtok(NULL, c);
    }
}

void CResFilter::Parse(char *buf)
{
	//cout << "Parse" << endl;
	m_vRes.clear();
	//char scan[PROAC_LEN], modForm[PROAC_LEN], proAC[PROAC_LEN]; //这里的静态长度不可能太长
	//char *proSQ = new char[MAXSQSIZE]; // MAXSQSIZE可能会比较大，静态分配不够
    string title, proSQ, modForm, proAC;
    double specMass, proMass, score, evalue, nterm_intens, cterm_intens;
    int sIdx = 0;
	int fileId, matchedPeaks, isDecoy, charge, nterm_ions, cterm_ions;
	int len = strlen(buf);
    int pStart = _FindFirstOf(buf, '\n', 1, len) + 1; //Skip the title
    int pEnd = _FindFirstOf(buf, '\n', pStart, len);
	//istringstream iss;
	//int cnt = 0;

	vector<string> splitWords;
	if(m_nDBnum > 1)
	{
		unordered_map<string, int> mapTitleExist;
		while(pStart < pEnd)
		{
			//++cnt;
			//if(cnt % 100 == 0) cout << cnt << endl;

			splitWords.clear();
			char *line = new char[pEnd - pStart + 1];
			strncpy(line, buf + pStart, pEnd - pStart);
			line[pEnd - pStart] = '\0';
			split(line, "\t", splitWords);
			delete[]line;
			
			try
			{
				if(splitWords.size() >= 16)
				{
					fileId = atoi(splitWords[0].c_str());
					title = splitWords[1];
					charge = atoi(splitWords[2].c_str());
					specMass = atof(splitWords[3].c_str());
					proMass = atof(splitWords[4].c_str());
					score = atof(splitWords[5].c_str());
					evalue = atof(splitWords[6].c_str());
					matchedPeaks = atoi(splitWords[7].c_str());
					nterm_ions = atoi(splitWords[8].c_str());
					cterm_ions = atoi(splitWords[9].c_str());
					nterm_intens = atof(splitWords[10].c_str());
					cterm_intens = atof(splitWords[11].c_str());
					isDecoy = atoi(splitWords[12].c_str());
					proAC = splitWords[13];
					proSQ = splitWords[14];
					modForm = splitWords[15];
					modForm.erase(modForm.end() - 1);

					PRSM tmpPrSM(sIdx, fileId, isDecoy, charge, matchedPeaks, nterm_ions, cterm_ions, specMass, proMass, score, evalue, nterm_intens, cterm_intens, title, proSQ, proAC, modForm);
					++sIdx;

					unordered_map<string, int>::iterator it = mapTitleExist.find(title);
					if(it == mapTitleExist.end())
					{
						m_vRes.push_back(tmpPrSM);
						mapTitleExist.insert(pair<string, int>(title, m_vRes.size() - 1));
					} else {
						if(m_vRes[(it->second)].lfScore < score)
						{
							m_vRes[(it->second)] = tmpPrSM;
						}
					}
				}
			} catch(bad_alloc &ba) {
				cout << "CResFilter::Parse " << ba.what() << endl;
				m_clog->alert("CResFilter::Parse %s", ba.what());
			}
			
			pStart = pEnd + 1;
			pEnd = _FindFirstOf(buf, '\n', pStart, len);
		}
	} else {
		while(pStart < pEnd)
		{
			//++cnt;
			//if(cnt % 100 == 0) cout << cnt << endl;

			splitWords.clear();
			char *line = new char[pEnd - pStart + 1];
			strncpy(line, buf + pStart, pEnd - pStart);
			line[pEnd-pStart] = '\0';
			//cout << line << endl;
			split(line, "\t", splitWords);
			delete[]line;

			//cout << splitWords.size() << endl;
			try
			{
				if(splitWords.size() >= 16)
				{
					fileId = atoi(splitWords[0].c_str());
					title = splitWords[1];
					charge = atoi(splitWords[2].c_str());
					specMass = atof(splitWords[3].c_str());
					proMass = atof(splitWords[4].c_str());
					score = atof(splitWords[5].c_str());
					evalue = atof(splitWords[6].c_str());
					matchedPeaks = atoi(splitWords[7].c_str());
					nterm_ions = atoi(splitWords[8].c_str());
					cterm_ions = atoi(splitWords[9].c_str());
					nterm_intens = atof(splitWords[10].c_str());
					cterm_intens = atof(splitWords[11].c_str());
					isDecoy = atoi(splitWords[12].c_str());
					proAC = splitWords[13];
					proSQ = splitWords[14];
					modForm = splitWords[15];
					modForm.erase(modForm.end() - 1);

					PRSM tmpPrSM(sIdx, fileId, isDecoy, charge, matchedPeaks, nterm_ions, cterm_ions, specMass, proMass, score, evalue, nterm_intens, cterm_intens, title, proSQ, proAC, modForm);
					++sIdx;

					m_vRes.push_back(tmpPrSM);
				}
			} catch(bad_alloc &ba) {
				cout << "CResFilter::Parse " << ba.what() << endl;
				m_clog->alert("CResFilter::Parse %s", ba.what());
			}
			pStart = pEnd + 1;
			pEnd = _FindFirstOf(buf, '\n', pStart, len);
		}
	}
	//delete []proSQ;

#ifdef _DEBUG_TEST
    cout<<pStart<<" "<<pEnd<<endl;
    cout<<m_vRes.size()<<endl;
    for(size_t i = 0; i < m_vRes.size(); ++i)
    {
        cout<<m_vRes[i].strSpecTitle<<endl;
    }
#endif

}

// genenrate csv file
void CResFilter::GeneReport(vector<int> &vIdScan)
{
    //int filtePSMnum = 0;
	char * title = new char[MAX_PATH + 18];
	vector<unordered_set<int> > setIdScan;
	unordered_set<int> emptySet;
	setIdScan.assign(m_nRawNum, emptySet);
	m_nIdSpecNum = 0;
    FILE *freport = fopen(m_strReport[m_nResFileIdx].c_str(), "w");
	if(freport == NULL)
	{
		cout<<"[Error] Failed to open file: "<< m_strReport[m_nResFileIdx]<<endl;
		m_clog->alert("Failed to open file: " + m_strReport[m_nResFileIdx]);
		exit(-1);
	}
	fprintf(freport, "ID,Title,Precursor Rank,Charge State,Precursor MZ,Precursor Mass,Theory Mass,Mass Diff Da, Mass Diff ppm,Protein AC,Sequence,PTMs,Nterm Matched Ions,Cterm Matched Ions,Nterm Matched Intensity Ratio,Cterm Matched Intensity Ratio,Score,Evalue\n");
    for(int i = 0; i < (int)m_vRes.size(); ++i)
    {
        if(m_vRes[i].nIsDecoy == 0)
        {
			int precursor_id = 0;
			try
			{
				//if(setIdScan.find(m_vRes[i].))
				vector<string> splitElems;
				strcpy(title, m_vRes[i].strSpecTitle.c_str());
				split(title, ".", splitElems);
				
				if(splitElems.size() < 5 || splitElems[(int)splitElems.size()-1] != "dta") // 不规范的mgf文件，无法解析scan号，计数不考虑混合谱
				{
					++vIdScan[m_vRes[i].nfileID];

				} else {
					int fNo = m_vRes[i].nfileID;
					
					int scan = atoi(splitElems[(int)splitElems.size() - 4].c_str());
					if(setIdScan[fNo].find(scan) == setIdScan[fNo].end())
					{
						++vIdScan[fNo];
						setIdScan[fNo].insert(scan);
					}
					precursor_id = atoi(splitElems[(int)splitElems.size() - 2].c_str());
				}
			}
			catch(...)
			{
				char errInfo[100];
				sprintf( errInfo, "[Error] Invalid fileID: %d in the record of the results!", m_vRes[i].nfileID);
				throw(errInfo);
			}

			double dist = m_vRes[i].lfPrecursorMass - m_vRes[i].lfProMass;
			int charge = m_vRes[i].nCharge;
			double mz = (m_vRes[i].lfPrecursorMass + (charge - 1)*IonMass_Proton)/charge;
			fprintf(freport, "%d,%s,%d,%d,%f,%f,%f,%.3f,%.1f,%s,%s,%s,%d,%d,%.3f,%.3f,%.2f,%e\n", ++m_nIdSpecNum, m_vRes[i].strSpecTitle.c_str(), precursor_id, charge, 
				mz, m_vRes[i].lfPrecursorMass, m_vRes[i].lfProMass, dist, 1000000 * dist / m_vRes[i].lfProMass, m_vRes[i].strProAC.c_str(), m_vRes[i].strProSQ.c_str(), m_vRes[i].vModInfo.c_str(), 
				m_vRes[i].ntermIons, m_vRes[i].ctermIons, m_vRes[i].ntermMatchedIntensityRatio, m_vRes[i].ctermMatchedIntensityRatio, m_vRes[i].lfScore, m_vRes[i].lfEvalue);
        }
    }
	delete[]title;
    fclose(freport);
    cout << "[pTop] Total PSM number: " << m_nIdSpecNum << endl;
	m_clog->info("Total PSM number: %d",m_nIdSpecNum);
}

void CResFilter::OutputLabelRes(int fileID, FILE *f, CPrePTMForm *cPTMForms, const vector<MODINI> &fixmodInfo)
{
	vector<LABEL_ITEM> vCurrResFile;
	int var_mod_num = cPTMForms->GetModNum();
	for(size_t i = 0; i < m_vRes.size(); ++i)
	{
		if(m_vRes[i].nIsDecoy == 1) continue;
		if(m_vRes[i].nfileID == fileID)
		{
			vector<int> sites, ids;
			if(m_vRes[i].vModInfo.compare("NULL") != 0)
			{
				size_t pStart = 1;
				while(pStart < m_vRes[i].vModInfo.length())
				{
					size_t pSite = m_vRes[i].vModInfo.find_first_of(')', pStart);
					size_t pEnd = m_vRes[i].vModInfo.find_first_of(';', pSite);
					int site = atoi(m_vRes[i].vModInfo.substr(pStart, pSite - pStart).c_str());
					sites.push_back(site);
					string modName = m_vRes[i].vModInfo.substr(pSite + 1, pEnd - pSite - 1);
					for(int m = 0; m < var_mod_num; ++m)
					{			
						if(modName.compare(cPTMForms->GetNamebyID(m)) == 0)
						{
							ids.push_back(m + 1);
							break;
						}
					}
					pStart = pEnd + 2;
				}
			}
			// 处理固定修饰
			for(size_t midx = 0; midx < fixmodInfo.size(); ++midx)
			{
				if('N' == fixmodInfo[midx].cType)
				{
					sites.push_back(0);
					ids.push_back(var_mod_num + midx + 1);
					continue;
				} else if('C' == fixmodInfo[midx].cType) {
					sites.push_back(m_vRes[i].strProSQ.length() + 1);
					ids.push_back(var_mod_num + midx + 1);
					continue;
				}
				for(size_t pos = 0; pos < m_vRes[i].strProSQ.length(); ++pos)
				{
					if(fixmodInfo[midx].strSite.find(m_vRes[i].strProSQ[pos]) != string::npos)
					{
						sites.push_back(pos + 1);
						ids.push_back(var_mod_num + midx + 1);
					}
				}
			}

			LABEL_ITEM tmpRes(m_vRes[i].lfScore, m_vRes[i].strSpecTitle, m_vRes[i].strProSQ, sites, ids);
			vCurrResFile.push_back(tmpRes);
		}
	}
	fprintf(f, "total=%d\n", vCurrResFile.size());
	for(size_t s = 0; s < vCurrResFile.size(); ++s)
	{
		string title = "";
		for(size_t i = 0; i < vCurrResFile[s].strTitle.size(); ++i)
		{
			if(vCurrResFile[s].strTitle[i] >= 'a' && vCurrResFile[s].strTitle[i] <= 'z')
				title.push_back(vCurrResFile[s].strTitle[i] - 'a' + 'A');
			else title.push_back(vCurrResFile[s].strTitle[i]);
		}
		fprintf(f, "[Spectrum%d]\nname=%s\n", s + 1, title.c_str());
		fprintf(f, "pep1=0 %s %lf ", vCurrResFile[s].strSeq.c_str(), vCurrResFile[s].lfScore);
		
		if(vCurrResFile[s].vmodIds.size() > 0)
		{
			for(size_t mIdx = 0; mIdx < vCurrResFile[s].vmodIds.size(); ++mIdx)
			{
				fprintf(f, "%d,%d ", vCurrResFile[s].vmodSites[mIdx], vCurrResFile[s].vmodIds[mIdx]);
			}
			fprintf(f, "\n");
		} else {
			fprintf(f, "\n");
		}
	}
}

void CResFilter::FirstReport(int fileType, vector<string> &vIdSpec, vector<PROTEIN_STRUCT> &proteinDB)
{
	bool isIdentified = true;
	set<string> setProAC;
	vector<SPEC_ID_TITLE> tmpIdSpec;
    for(int i = 0; i < (int)m_vRes.size(); ++i)
    {
        if(m_vRes[i].nIsDecoy == 0)
        {
			if(isIdentified)
			{
				tmpIdSpec.push_back(SPEC_ID_TITLE(m_vRes[i].nIdx, m_vRes[i].strSpecTitle));
			}
			if(setProAC.find(m_vRes[i].strProAC) != setProAC.end())
			{
				continue;
			}
			setProAC.insert(m_vRes[i].strProAC);
			PROTEIN_STRUCT tmpPro;
			tmpPro.nIsDecoy = 0;
			tmpPro.lfMass = 0.0; //结果里存的不是蛋白质序列的理论质量，而是序列加修饰的理论质量，所以这里到MainFlow中之后再算
			tmpPro.strProAC = m_vRes[i].strProAC;
			tmpPro.strProSQ = m_vRes[i].strProSQ;

			proteinDB.push_back(tmpPro);

			tmpPro.nIsDecoy = 1;
			CStringProcess::Reverse(tmpPro.strProSQ);
			proteinDB.push_back(tmpPro);

		} else {
			isIdentified = false; //一旦遇到一个反库结果就停止记录鉴定结果，控制0%的FDR
		}
    }
	sort(tmpIdSpec.begin(), tmpIdSpec.end());
	if(1 == fileType)
	{
		for(size_t sidx = 0; sidx < tmpIdSpec.size(); ++sidx)
		{   // 当采用pf作为输入格式时，谱图的标题中存储的是title.scan.scan，因此这里保持一致
			string tmpTitle = tmpIdSpec[sidx].strTitle;
			int slen = tmpTitle.length();
			int dotnum = 0;
			for(int delidx = 1; delidx < slen; ++delidx)
			{
				if(tmpTitle[tmpTitle.size() - 1] == '.') // vIdSpec[ids].back() == '.'
				{
					++dotnum;
				}
				tmpTitle.erase(tmpTitle.end() - 1);
				if(dotnum >= 3)
				{
					break;
				}
			}
			if(0 == vIdSpec.size() || vIdSpec[vIdSpec.size() - 1].compare(tmpTitle) != 0)
			{   // 删除scan重复项，目前不考虑混合谱的二次搜索
				vIdSpec.push_back(tmpTitle);
			}
		}
	} else {
		for(size_t sidx = 0; sidx < tmpIdSpec.size(); ++sidx)
		{
			vIdSpec.push_back(tmpIdSpec[sidx].strTitle);
		}
	}
}

void CResFilter::FirstFilter(int fileType, int fidx, vector<string> &vIdSpec, vector<PROTEIN_STRUCT> &proteinDB)
{
	m_nResFileIdx = fidx;
	GetSearchRes();
	_Filter();
	FirstReport(fileType, vIdSpec, proteinDB);
}

void CResFilter::Run(vector<FILE*> &fLabels, CPrePTMForm *cPTMForms, const vector<MODINI> &fixmodInfo, vector<int> &vIdScan)
{
	m_nRawNum = (int)fLabels.size();
	for(m_nResFileIdx = 0; m_nResFileIdx < m_nRawNum; ++m_nResFileIdx)
	{
		cout << "[pTop] Fileter " << m_strQryRes[m_nResFileIdx] << endl;
		m_clog->info("Fileter "+m_strQryRes[m_nResFileIdx]);
		m_clog->info("Read Search Results...");

		GetSearchRes();

		m_clog->info("Filter...");
		_Filter();
		GeneReport(vIdScan);
		if(fLabels[m_nResFileIdx] != NULL)
		{
			OutputLabelRes(m_nResFileIdx, fLabels[m_nResFileIdx], cPTMForms, fixmodInfo);
		}
		cout << "[pTop] Fileter finished!" << endl;
		m_clog->info("Fileter finished!");

		cout << "[pTop] The results in "<< m_strReport[m_nResFileIdx] << endl;
		m_clog->info("The results in " + m_strReport[m_nResFileIdx]);
	}
}
