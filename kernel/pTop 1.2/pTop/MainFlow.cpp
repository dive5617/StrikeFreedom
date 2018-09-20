#include <fstream>
#include <iostream>
#include <cstdio>
#include <cstring>
#include <string>
#include <algorithm>
#include <cmath>

#include "MainFlow.h"
#include "BasicTools.h"

//#define _Debug_Spec

using namespace std;

bool PrSMScoreCmp(const PROTEIN_SPECTRA_MATCH p1, const PROTEIN_SPECTRA_MATCH p2)
{
    if(p1.lfScore != p2.lfScore) 
	{
		return p1.lfScore > p2.lfScore;
	} else {
		return p1.vModSites.size() < p2.vModSites.size(); // 分数相等时，修饰越少越可信
	}
}

/**
*
*	class CMainFlow
*/

CMainFlow::CMainFlow(const string &strFile) : m_strConfigFile(strFile)
{
	m_nTotalProNum = 0;
	m_nIdSpecNum = 0;
	m_nIdScan = 0;
	m_nIdProNum = 0;
	
	m_nSpecNum = 0;
	m_bFirstReadPF = true;
	m_cPara = new CConfiguration(m_strConfigFile);
	m_cMapMass = new CMapAAMass;
	m_cPTMForms = new CPrePTMForm;
	char buf[BUFLEN];
	GetCurrentDirectory(BUFLEN, buf);
	string strDir(buf);
	strDir += "\\tmp";
	makeDir(strDir);
	m_clog = Clog::getInstance("tmp\\pTop.log",LOG_LEVEL::LL_INFORMATION);
}

CMainFlow::~CMainFlow()
{
	delete m_cPara;
	delete m_cMapMass;
	delete m_cPTMForms;
}

void CMainFlow::Welcome()
{
	cout << "****************************************" << endl;
	cout << "*                                      *" << endl;
	cout << "*       Welcome to use pTop!           *" << endl;
	cout << "*                                      *" << endl;
	cout << "*            version 1.2             *" << endl;
	cout << "*                                      *" << endl;
	cout << "*        http://pfind.ict.ac.cn        *" << endl;
	cout << "*                                      *" << endl;
	cout << "****************************************" << endl;
}
void CMainFlow::Usage()
{
	cout << "[pTop] Usage: pTop.exe config.cfg" << endl;
	FILE *fp = fopen("config.cfg", "r");
	if(fp == NULL)
	{
		fclose(fp);
		FILE *fini = fopen("config.cfg", "w");
		fprintf(fini, "[database]");
		fprintf(fini, "Database=test.fasta");
		fprintf(fini, "[spectrum]");
		fprintf(fini, "Spec_path=test.mgf");
		fprintf(fini, "Output_path=test.txt");
		fprintf(fini, "Report_path=filter_test.txt");
		fprintf(fini, "Activation=CID");
		fprintf(fini, "Precursor_Tolerance=2");
		fprintf(fini, "Fragment_Tolerance=20");
		fprintf(fini, "[fixmodify]");
		fprintf(fini, "fixedModify_num=1");
		fprintf(fini, "fix_mod1=Carbamidomethyl[C]");
		fprintf(fini, "[modify]");
		fprintf(fini, "Max_mod=6");
		fprintf(fini, "Modify_num=2");
 		fprintf(fini, "var_mod1=Acetyl[K]");
		fprintf(fini, "var_mod2=Acetyl[S]");
		fprintf(fini, "[filter]");
		fprintf(fini, "threshold=0.01");
		fclose(fini);
	}
}

// Changed at 2014.07.27
// Changed strcut SPECTRUM，每张谱图对应一个母离子列表 
// 这个函数兼容其他的mgf文件，即其实每张谱图的母离子列表中只有一个元素
// 如果处理的是pParseTD导出的MGF文件，则scan号的数目通过解析title获得，否则scan数目等于谱图数目
// wrm20150728: 增加参数preScan，避免分块读取时丢失上一块的最后一个scan号
int CMainFlow::ParseMGF(ifstream &finMGF, vector<SPECTRUM> &spectra, int &preScan, int &scanCnt)
{
	scanCnt = 0;
	//int preScan = -1;
    int specNum = 0;
	size_t ionIdx = 0;
	string line;
	FRAGMENT_ION tmp;
	PRECURSOR precur;
	
	while( getline(finMGF, line) )
	{
		int len = line.length();
		if(0 >= len) 
		{
			continue;
		}
		//删除行头的无用字符
		int i = 0;
		while (i < (int)line.length() && (' ' == line[i] || '\r' == line[i] || '\t' == line[i]))
		{
			++i;
		}
		if (i == (int)line.length()) 
		{
			continue;
		} else if(i > 0) {
			line.erase(0, i);
		}

		if('T' == line[0]) {                      //TITLE=
			size_t found = line.find("=");
			if(found == string::npos)
			{
				continue;
			}
			string specTitle = line.substr(found + 1, line.size() - found - 1);
			found = specTitle.find_first_of(' ');
			while(found != string::npos)
			{
				specTitle[found] = '_';
				found = specTitle.find_first_of(' ', found + 1);
			}
			spectra[specNum].strSpecTitle = specTitle;

			vector<string> parts;
			m_cMapMass->Split(specTitle, ".", parts);
			if(parts.size() >= 5) 
			{
				int scan = atoi(parts[(int)parts.size() - 4].c_str());
				if(scan != preScan)
				{
					preScan = scan;
					++scanCnt;
				}
			} else {   // 不规范的mgf文件，无法解析scan号，计数不考虑混合谱
				++scanCnt;
			}
			
		} else if('P' == line[0]) {               //PEPMASS=
			size_t found = line.find("=");
			precur.lfPrecursorMass = atof(line.substr(found+1, line.size() - found - 1).c_str());
		} else if('C' ==  line[0]){               //CHARGE=
				while(line.size() > 0 && (line[line.size()-1] > '9' || line[line.size()-1] < '0'))
				{
					line.erase(line.end() - 1);
				}
				size_t found = line.find("=");
				precur.nPrecursorCharge = atoi(line.substr(found+1, line.size() - found - 1).c_str());
		} else if(line[0] >= '0' && line[0] <= '9'){ //MZ & Intensity
				size_t found = line.find(" ");
				if (found == string::npos)
					found = line.find("\t");
				if(found == string::npos)
				{
					continue;
				}
				tmp.lfMz = atof(line.substr(0, found).c_str());
				//tmp.lfIntens = log10(atof(line.substr(found + 1, line.size() - found).c_str())); // test2014.09.10
				tmp.lfIntens = atof(line.substr(found + 1, line.size() - found).c_str());
				//cout<<tmp.lfMz<<" "<<tmp.lfIntens<<endl;

				spectra[specNum].vPeaksTbl.push_back(tmp);
				++ionIdx;
		} else if('E' == line[0]){              //END IONS
				spectra[specNum].nPeaksNum = ionIdx;
				//计算单电荷母离子质量
				precur.lfPrecursorMass = (precur.lfPrecursorMass - IonMass_Proton) * (double)precur.nPrecursorCharge + IonMass_Proton;
				spectra[specNum].vPrecursors.push_back(precur);
				//cout<<spectra[specNum].strSpecTitle<<" "<<spectra[specNum].lfPrecursorMass<<" "<<spectra[specNum].nPeaksNum<<" "<<spectra[specNum].nPrecursorCharge<<endl;
				
				if(++specNum >= MAX_SPECTRA_NUM) 
				{
					break;
				}
				
		} else if('B' == line[0]) {            //BEGIN IONS
				ionIdx = 0;
				
				spectra[specNum].nPrecurNum = 1;	// 必要的初始化，以防MGF文件不完整
				spectra[specNum].nPeaksNum = 0;
				spectra[specNum].nCandidateProNum = 0;
				spectra[specNum].isPF = false;
				spectra[specNum].strSpecTitle = "NULL"; 
				spectra[specNum].vPrecursors.clear();
				spectra[specNum].vPeaksTbl.clear();
				
				precur.nValidCandidateProNum = 0;
				precur.lfPrecursorMass = 0.0;
				precur.nPrecursorCharge = 2;
				precur.aBestPrSMs.clear();
				
		} else continue;
	}//end while
	return specNum;
}
// 解析pF文件
// pF二进制文件的格式：谱图数目、谱图文件名长度、谱图文件名
// [scan号、谱峰数目、谱峰列表、母离子数目、母离子列表]
int CMainFlow::ParsePF(FILE *finPF, vector<SPECTRUM> &spectra)
{
	if(m_bFirstReadPF)
	{
		fread(&m_nSpecNum, sizeof(int), 1, finPF);
		//cout << m_nSpecNum << endl;

		int nSize = 0;
		fread(&nSize, sizeof(int), 1, finPF);
		//cout << "Size of title " << nSize << endl;

		char *title = new char[nSize + 1];
		fread(title, sizeof(char), nSize, finPF);
		title[nSize] = '\0';
		m_strTitle = title;
		
		size_t found = m_strTitle.find_first_of(' ');
		while(found != string::npos)
		{
			m_strTitle[found] = '_';
			found = m_strTitle.find_first_of(' ', found + 1);
		}
		
		//cout << "Title " << title <<endl;
		delete [] title;
		
		m_bFirstReadPF = false;
	}
	//cout << m_nSpecNum << endl;
	int cnt = 0;
	while(cnt < MAX_SPECTRA_NUM && cnt < m_nSpecNum)
	{
		spectra[cnt].nPrecurNum = 0;
		spectra[cnt].nPeaksNum = 0;
		spectra[cnt].nCandidateProNum = 0;
		spectra[cnt].isPF = true;
		spectra[cnt].strSpecTitle = "";

		spectra[cnt].vPrecursors.clear();
		spectra[cnt].vPeaksTbl.clear();

		int Scan = 0;
		fread(&Scan, sizeof(int), 1, finPF);
		//cout << "Scan No. " << Scan <<endl;
		char strScan[20];
		sprintf(strScan, ".%d.%d", Scan, Scan);
		spectra[cnt].strSpecTitle = m_strTitle;
		spectra[cnt].strSpecTitle.append(strScan);


		int PeakNum = 0;
		fread(&PeakNum, sizeof(int), 1, finPF);
		//cout << "PeakNum " << PeakNum <<endl;
		spectra[cnt].nPeaksNum = PeakNum;

		PeakNum <<= 1;  // 谱峰数目*2 = m/z数目 + Intensity数目
		double * Peaks = new double[PeakNum];
		fread(Peaks, sizeof(double), PeakNum, finPF);
		//cout << "Peaks" <<endl;

		for (int i = 0; i < PeakNum ; i += 2)
		{
			FRAGMENT_ION tmp(Peaks[i], Peaks[i + 1]);
			spectra[cnt].vPeaksTbl.push_back(tmp);
			//cout << Peaks[i] << " " << Peaks[i + 1] <<endl;
		}
		delete [] Peaks;

		fread(&spectra[cnt].nPrecurNum, sizeof(int), 1, finPF);
		//cout <<"Number of Precursors: " << spectra[cnt].nPrecurNum <<endl;

		for (int j = 0; j < spectra[cnt].nPrecurNum; j ++)
		{
			double mz = 0;
			int charge = 0;
			fread(&mz, sizeof(double), 1, finPF);
			fread(&charge, sizeof(int), 1, finPF);
			PRECURSOR precur(charge, (mz - IonMass_Proton) * (double)charge + IonMass_Proton, 0);
			spectra[cnt].vPrecursors.push_back(precur);
			//cout << "mz = " << mz << " charge = " << charge <<endl;
		}
		++cnt;
	}
	
	m_nSpecNum -= cnt;
	if(0 == m_nSpecNum)
	{   // 为下一个pf文件打开开关
		m_bFirstReadPF = true;
	}
	//cout << "cnt = " << cnt << endl;
	return cnt;
}
/*
// add @2014.08.28
int CMainFlow::ParseCombMGF(ifstream &finMGF, vector<SPECTRUM> &spectra)
{
    int specNum = 0;
	size_t ionIdx = 0;
	string line;
	FRAGMENT_ION tmp;
	while( getline(finMGF, line) )
	{
		//cout<<line<<endl;
		if(0 >= line.length()) continue;
		if('T' == line[0]) {                      //TITLE=
			size_t found = line.find("=");
			spectra[specNum].strSpecTitle = line.substr(found+1, line.size() - found - 1);
		} else if('P' == line[0]) {               //PEPMASS=
			size_t found = line.find("=");
			spectra[specNum].lfPrecursorMass = atof(line.substr(found+1, line.size() - found - 1).c_str());
		} else if('C' ==  line[0]){               //CHARGE=
			while(line.size() > 0 && (line[line.size()-1] > '9' || line[line.size()-1] < '0'))
			{
				line.erase(line.end() - 1);
			}
			size_t found = line.find("=");
			spectra[specNum].nPrecursorCharge = atoi(line.substr(found+1, line.size() - found - 1).c_str());
		} else if('N' == line[0]) {
			size_t found = line.find("=");
			spectra[specNum].nbIonsNum = atoi(line.substr(found + 1, line.size() - found - 1).c_str());
		} else if(line[0] >= '0' && line[0] <= '9'){ //MZ & Intensity
				size_t found = line.find(" ");
				tmp.lfMz = atof(line.substr(0, found).c_str());
				tmp.lfIntens = log10(atof(line.substr(found + 1, line.size() - found).c_str()));
				spectra[specNum].vPeaksTbl.push_back(tmp);
				++ionIdx;
		} else if('E' == line[0]){              //END IONS
				spectra[specNum].nPeaksNum = ionIdx;
				spectra[specNum].nCandidateProNum = 0;
				spectra[specNum].nValidCandidateProNum = 0;
				spectra[specNum].lfPrecursorMass = (spectra[specNum].lfPrecursorMass - IonMass_Proton) * (double)spectra[specNum].nPrecursorCharge + IonMass_Proton;
				//cout<<spectra[specNum].strSpecTitle<<" "<<spectra[specNum].lfPrecursorMass<<" "<<spectra[specNum].nPeaksNum<<" "<<spectra[specNum].nPrecursorCharge<<endl;
				if(++specNum > 5000) break;
				//cout<<specNum<<endl;

		} else if('B' == line[0]) {            //BEGIN IONS
				ionIdx = 0;
				spectra[specNum].nbIonsNum =0;
		} else continue;
	}//end while
	return specNum;
}
*/
// Out put the Top k res as the format of pFind 
void CMainFlow::OutputpFindRes(FILE *f, vector<SPECTRUM> &spectra, int cnt, int specNum)
{
	for(int s = 0; s < cnt; ++s)
	{
		for(int p = 0; p < spectra[s].nPrecurNum; ++p)
		{
			if(0 < spectra[s].vPrecursors[p].nValidCandidateProNum)
			{
				sort(spectra[s].vPrecursors[p].aBestPrSMs.begin(), spectra[s].vPrecursors[p].aBestPrSMs.begin() + spectra[s].vPrecursors[p].nValidCandidateProNum,  PrSMScoreCmp);
			}

			fprintf(f, "[Spectrum%d]\n", specNum + s);
			fprintf(f, "Input=%s\n", spectra[s].strSpecTitle.c_str());
			fprintf(f, "Charge=1\nIntensity=0.00000\n");
			fprintf(f, "MH=%lf\n", spectra[s].vPrecursors[p].lfPrecursorMass);
			fprintf(f, "MZ=%lf\n", spectra[s].vPrecursors[p].lfPrecursorMass);
			fprintf(f, "Candidate_Total=%d\n", spectra[s].nCandidateProNum);
			if(spectra[s].vPrecursors[p].nValidCandidateProNum > 0)
			{
				fprintf(f, "ValidCandidate=1\n");
			} else {
				fprintf(f, "ValidCandidate=0\n");
			}

			for(int i = 0; i < spectra[s].vPrecursors[p].nValidCandidateProNum && i < TOP_K; ++i)
			{
				fprintf(f, "NO%d_Score=%lf\n", i+1, spectra[s].vPrecursors[p].aBestPrSMs[i].lfScore);
				fprintf(f, "NO%d_EValue=%.6e\n", i+1, exp(-spectra[s].vPrecursors[p].aBestPrSMs[i].lfScore));
				fprintf(f, "NO%d_MH=%lf\n", i+1, spectra[s].vPrecursors[p].aBestPrSMs[i].lfMass);
				fprintf(f, "NO%d_Matched_Peaks=%d\n", i+1, spectra[s].vPrecursors[p].aBestPrSMs[i].nMatchedPeakNum);
				fprintf(f, "NO%d_Matched_Intensity=0.0\n", i+1);
				fprintf(f, "NO%d_SQ=%s\n", i+1, spectra[s].vPrecursors[p].aBestPrSMs[i].strProSQ.c_str());
				fprintf(f, "NO%d_MissCleave=0\n", i+1);
				if(spectra[s].vPrecursors[p].aBestPrSMs[i].nIsDecoy)
				{
					fprintf(f, "NO%d_Proteins=1,REVERSE_%s\n", i+1, spectra[s].vPrecursors[p].aBestPrSMs[i].strProAC.substr(0,50).c_str());
				} else {
					fprintf(f, "NO%d_Proteins=1,%s\n", i+1, spectra[s].vPrecursors[p].aBestPrSMs[i].strProAC.substr(0,50).c_str());
				}
				fprintf(f, "NO%d_ProteinIDs=1,1\n", i+1);
				fprintf(f, "NO%d_PrevNextAA=1,-:-\nNO%d_ProStartPos=1,0\n", i+1, i+1);
				if(spectra[s].vPrecursors[p].aBestPrSMs[i].vModSites.size() > 0)
				{
					int mIdx;
					fprintf(f, "NO%d_Modify_Pos=%d", i+1, spectra[s].vPrecursors[p].aBestPrSMs[i].vModSites.size());
					for(mIdx = (int)spectra[s].vPrecursors[p].aBestPrSMs[i].vModSites.size() - 1; mIdx >= 0 ; --mIdx)
					{
						fprintf(f, ",%d", spectra[s].vPrecursors[p].aBestPrSMs[i].vModSites[mIdx].nSecond);
					}
					fprintf(f, "\nNO%d_Modify_Name=%d", i+1, spectra[s].vPrecursors[p].aBestPrSMs[i].vModSites.size());
					for(mIdx = (int)spectra[s].vPrecursors[p].aBestPrSMs[i].vModSites.size() - 1; mIdx >= 0 ; --mIdx)
					{
						fprintf(f, ",%s", m_cPTMForms->GetNamebyID(spectra[s].vPrecursors[p].aBestPrSMs[i].vModSites[mIdx].nFirst).c_str());
					}
					fprintf(f, "\n");
				}else{
					fprintf(f, "NO%d_Modify_Pos=0\nNO%d_Modify_Name=0\n", i+1, i+1);
				}
				fprintf(f, "NO%d_ModSite=-1\nNO%d_deltaMass=0.000000\n", i+1, i+1);
			}
		}
	}
}

void CMainFlow::OutputPrSMRes(FILE *f, vector<SPECTRUM> &spectra, int cnt, int fileID)
{
	for(int s = 0; s < cnt; ++s) // Scan Number
	{
		for(int p = 0; p < spectra[s].nPrecurNum; ++p)
		{
			//cout<<s<<" "<<spectra[s].vPrecursors[p].nValidCandidateProNum<<endl;
			if(0 < spectra[s].vPrecursors[p].nValidCandidateProNum)
			{
				// sort by score
				sort(spectra[s].vPrecursors[p].aBestPrSMs.begin(), spectra[s].vPrecursors[p].aBestPrSMs.begin() + spectra[s].vPrecursors[p].nValidCandidateProNum, PrSMScoreCmp);
				// 排好序其他地方可以继续用
				int bestPSM = 0;
				// get the best score within tolerance
				while(fabs(spectra[s].vPrecursors[p].lfPrecursorMass - spectra[s].vPrecursors[p].aBestPrSMs[bestPSM].lfMass) > m_cPara->m_lfPrecurTol)
				{
					if(++bestPSM >= (int)spectra[s].vPrecursors[p].aBestPrSMs.size()) 
					{
						break;
					}
				}
				if(bestPSM >= (int)spectra[s].vPrecursors[p].nValidCandidateProNum) // no candidate proteins
				{
					continue;
				}

				if(true == spectra[s].isPF)
				{
					fprintf(f, "%d\t%s.%d.%d.dta\t", fileID, spectra[s].strSpecTitle.c_str(), spectra[s].vPrecursors[p].nPrecursorCharge, p);
				} else {
					fprintf(f, "%d\t%s\t", fileID, spectra[s].strSpecTitle.c_str());
				}
				
				//cout << spectra[s].aBestPrSMs[0].strProSQ << endl;

				fprintf(f, "%d\t", spectra[s].vPrecursors[p].nPrecursorCharge);
				fprintf(f, "%lf\t", spectra[s].vPrecursors[p].lfPrecursorMass);
				fprintf(f, "%lf\t", spectra[s].vPrecursors[p].aBestPrSMs[bestPSM].lfMass);
				fprintf(f, "%lf\t", spectra[s].vPrecursors[p].aBestPrSMs[bestPSM].lfScore);
				fprintf(f, "%e\t", spectra[s].vPrecursors[p].aBestPrSMs[bestPSM].lfQvalue);
				fprintf(f, "%d\t", spectra[s].vPrecursors[p].aBestPrSMs[bestPSM].nMatchedPeakNum);
				fprintf(f, "%d\t%d\t%f\t%f\t",spectra[s].vPrecursors[p].aBestPrSMs[bestPSM].ntermIons,
					spectra[s].vPrecursors[p].aBestPrSMs[bestPSM].ctermIons,
					spectra[s].vPrecursors[p].aBestPrSMs[bestPSM].ntermMatchedIntensityRatio,
					spectra[s].vPrecursors[p].aBestPrSMs[bestPSM].ctermMatchedIntensityRatio);
				fprintf(f, "%d\t", spectra[s].vPrecursors[p].aBestPrSMs[bestPSM].nIsDecoy);
				fprintf(f, "%s\t", spectra[s].vPrecursors[p].aBestPrSMs[bestPSM].strProAC.c_str());
				fprintf(f, "%s\t", spectra[s].vPrecursors[p].aBestPrSMs[bestPSM].strProSQ.c_str());
				
				if(spectra[s].vPrecursors[p].aBestPrSMs[bestPSM].vModSites.size() > 0)
				{
					for(int mIdx = (int)spectra[s].vPrecursors[p].aBestPrSMs[bestPSM].vModSites.size() - 1; mIdx >= 0 ; --mIdx)
					{
						fprintf(f, "(%d)%s;", spectra[s].vPrecursors[p].aBestPrSMs[bestPSM].vModSites[mIdx].nSecond,
								 m_cPTMForms->GetNamebyID(spectra[s].vPrecursors[p].aBestPrSMs[bestPSM].vModSites[mIdx].nFirst).c_str());
					}
					fprintf(f, "\n");
				} else {
					fprintf(f, "NULL\n");
				}
			}
		}
	}
}

void CMainFlow::OutputLabelHead(FILE *f, const string &mgfFile, const vector<MODINI> &fixmodInfo)
{
	string mgfFilename = mgfFile;
	size_t found = mgfFilename.find_last_of('.');
	if(found == string::npos)
	{
		throw("Invalid mgf file!");
	}
	string suffix = mgfFilename.substr(found + 1);
	if(suffix.compare("pf2") == 0)
	{
		mgfFilename.erase(mgfFilename.begin() + found + 1, mgfFilename.end());
		mgfFilename.append("mgf");
	}
	
	fprintf(f, "[FilePath]\nFile_Path=%s\n", mgfFilename.c_str());
	fprintf(f, "[Modification]\n");
	int var_mod_num = m_cPTMForms->GetModNum();
	for(int i = 0; i < var_mod_num; ++i)
	{
		fprintf(f, "%d=%s\n", i+1, m_cPTMForms->GetNamebyID(i).c_str());
	}
	for(size_t i = 0; i < fixmodInfo.size(); ++i)
	{
		fprintf(f, "%d=%s\n", var_mod_num+i+1, fixmodInfo[i].strName.c_str());
	}
	fprintf(f, "[xlink]\nxlink=NULL\n[Total]\n");
}

void CMainFlow::Searcher(vector<PROTEIN_STRUCT> &proteinList, CTagFlow *cTagIndex, CTagSearcher *cTagSearcher)
{
	CSearchEngine cSearcher(m_cMapMass, m_cPTMForms, cTagIndex, cTagSearcher, m_cPara->m_lfFragTol, (int)(m_cPara->m_lfPrecurTol) + 1);
	cSearcher.Init(m_cPara->m_strActivation);
	
	//int totalNum = 0;
	m_vSpecNum.clear();
	m_vScanNum.clear();
	
	for(size_t i = 0; i < m_cPara->m_vSpecFiles.size(); ++i)
	{
		FILE *fqryRes = fopen(m_cPara->m_vstrQryResFile[i].c_str(), "a+");
		if(fqryRes == NULL) 
		{
			cout << "[Error] Failed to open output file " << m_cPara->m_vstrQryResFile[i].c_str() << endl;
			exit(0);
		}

		vector<SPECTRUM> spectra;
		spectra.assign(MAX_SPECTRA_NUM, SPECTRUM());
		
		cout << "[pTop] Search " << m_cPara->m_vSpecFiles[i] << endl;
		m_clog->info("Search " + m_cPara->m_vSpecFiles[i]);

		ifstream fMGF;
		FILE *fPF = NULL;
		if(1 == m_cPara->m_inFileType)  // PF
		{
			fPF = fopen(m_cPara->m_vSpecFiles[i].c_str(), "rb");
			if(fPF == NULL)
			{
				cout << "[Error] Failed to open file: " << m_cPara->m_vSpecFiles[i].c_str() << endl;
				m_clog->alert("Failed to open file: " + m_cPara->m_vSpecFiles[i]);
				exit(0);
			}
		} else {     // MGF
			fMGF.open(m_cPara->m_vSpecFiles[i].c_str(), std::ifstream::in);
			if(!fMGF)
			{
				cout << "[Error] Failed to open file: " << m_cPara->m_vSpecFiles[i].c_str() << endl;
				m_clog->alert("Failed to open file: " + m_cPara->m_vSpecFiles[i]);
				exit(0);
			}
		}
		
		int rawSpecNum = 0;
		int rawScanNum = 0;
		int preScan = -1;    // wrm20150728:记录上一个scan号，主要用于分块读取MGF文件
		while(1)
		{
			int specCnt = 0;
			try
			{
				if(1 == m_cPara->m_inFileType)
				{
					//cout << "parse pf" << endl;
					//解析PF文件，获得每张谱图的碎片离子和母离子信息，返回scan数目
					specCnt = ParsePF(fPF, spectra);  
					rawScanNum += specCnt;
				} else {
					//cout << "parse mgf" << endl;
					//对于pParseTD产生的mgf，tmpScanCnt = scan数目，specCnt是导出的谱图数目
					int tmpScanCnt = 0;
					specCnt = ParseMGF(fMGF, spectra, preScan, tmpScanCnt);
					rawScanNum += tmpScanCnt;
				}
			}			
			catch(exception & e) 
			{
				CErrInfo info("CMainFlow", "Searcher()", "Parsing input file failed.");
				throw runtime_error(info.Get(e).c_str());
			}
			catch(...) 
			{
				CErrInfo info("CMainFlow", "Searcher()", "Caught an unknown exception from parsing input file.");
				throw runtime_error(info.Get().c_str());
			}

			for(int s = 0; s < specCnt; ++s)
			{
				rawSpecNum += spectra[s].vPrecursors.size();
				
				//@ test
				#ifdef _Debug_Spec
				if(s < 1700 || s > 1800){
					continue;
				}else{
					//cout << spectra[s].strSpecTitle << "\t";
				}
				#endif

				cSearcher.Reset(spectra[s]); 
				cSearcher.Search(proteinList);
				
				/* Print the Rate of Process */  
				//fprintf(stdout, "\r[pTop] Search: %d / %d (%.0f%%)", s + 1, specCnt, (s + 1) * 100.0 / specCnt);
				printf("[pTop] <Search>: ");
				printf("%d / %d (%.0f%%)\r", s + 1, specCnt, (s + 1) * 100.0 / specCnt);
				fflush(stdout);
			}
			cout << endl;
			OutputPrSMRes(fqryRes, spectra, specCnt, i);

			//OutputpFindRes(fqryRes2, spectra, specCnt, totalNum, cPTMForms);

			//totalNum += specCnt;
			if(specCnt < MAX_SPECTRA_NUM)
			{
				break;
			}
		}
		cout << "[pTop] Search finished!" << endl;
		m_clog->info("Search finished!");

		cout << "[pTop] The results in "<< m_cPara->m_vstrQryResFile[i]<<endl;
		m_clog->info("The results in " + m_cPara->m_vstrQryResFile[i]);

		fclose(fqryRes);
		if(fPF != NULL) fclose(fPF);
		if(fMGF.is_open()) fMGF.close();
		
		m_vSpecNum.push_back(rawSpecNum);
		m_vScanNum.push_back(rawScanNum);
	}
}

void CMainFlow::SecondSearcher(vector<PROTEIN_STRUCT> &proteinList, vector<string> &vIdSpec)
{
	CTagFlow *cTagIndex = NULL;
	CTagSearcher *cTagSearcher = NULL;
	CSearchEngine cSearcher(m_cMapMass, m_cPTMForms, cTagIndex, cTagSearcher, m_cPara->m_lfFragTol, (int)(m_cPara->m_lfPrecurTol) + 1);
	cSearcher.Init(m_cPara->m_strActivation);
	
	for(size_t i = 0; i < m_cPara->m_vSpecFiles.size(); ++i)
	{
		FILE *fqryRes = fopen(m_cPara->m_vstrQryResFile[i].c_str(), "a+");
		if(fqryRes == NULL) 
		{
			cout << "[Error] Failed to open output file " << m_cPara->m_vstrQryResFile[i].c_str() << endl;
			exit(0);
		}

		vector<SPECTRUM> spectra;
		spectra.assign(MAX_SPECTRA_NUM, SPECTRUM());
		
		cout << "[pTop] Search " << m_cPara->m_vSpecFiles[i].c_str() << endl;

		ifstream fMGF;
		FILE *fPF = NULL;
		if(1 == m_cPara->m_inFileType)
		{
			fPF = fopen(m_cPara->m_vSpecFiles[i].c_str(), "rb");
			if(fPF == NULL)
			{
				cout << "[Error] Failed to open file: " << m_cPara->m_vSpecFiles[i].c_str() << endl;
				exit(0);
			}
		} else {
			fMGF.open(m_cPara->m_vSpecFiles[i].c_str(), std::ifstream::in);
			if(!fMGF)
			{
				cout << "[Error] Failed to open file: " << m_cPara->m_vSpecFiles[i].c_str() << endl;
				exit(0);
			}
		}

		int preScan = -1;  // wrm20150728:记录上一个scan号，主要用于分块读取MGF文件
		while(1)
		{
			int specCnt = 0;
			try
			{
				if(1 == m_cPara->m_inFileType)
				{
					specCnt = ParsePF(fPF, spectra);
				} else {
					int tmpScanCnt = 0;
					specCnt = ParseMGF(fMGF, spectra, preScan, tmpScanCnt);
				}
			}
			catch(exception & e) 
			{
				CErrInfo info("CMainFlow", "Searcher()", "Parsing input file failed.");
				throw runtime_error(info.Get(e).c_str());
			}
			catch(...) 
			{
				CErrInfo info("CMainFlow", "Searcher()", "Caught an unknown exception from parsing input file.");
				throw runtime_error(info.Get().c_str());
			}
			
			int idsIdx = 0, idsLen = (int)vIdSpec.size();
			for(int s = 0; s < specCnt; ++s)
			{	
				if(idsIdx < idsLen && spectra[s].strSpecTitle.compare(vIdSpec[idsIdx]) == 0)
				{
					++idsIdx;
					continue;
				}
				//cout << spectra[s].strSpecTitle << "\t";
				cSearcher.Reset(spectra[s]);
				cSearcher.SecondSearch(proteinList);
				
				printf("[pTop] <Search>: ");
				printf("%d / %d (%.0f%%)\r", s + 1, specCnt, (s + 1) * 100.0 / specCnt);
				fflush(stdout);
			}
			cout << endl;
			OutputPrSMRes(fqryRes, spectra, specCnt, i);

			if(specCnt < MAX_SPECTRA_NUM)
			{
				break;
			}
		}
		cout << "[pTop] Second Search finished!" << endl;

		fclose(fqryRes);
		if(fPF != NULL) fclose(fPF);
		if(fMGF.is_open()) fMGF.close();
	}
}

/*
	Output the summary infomation
*/
void CMainFlow::OutputSummary()
{
	for(size_t i = 0; i < m_vScanNum.size(); ++i)
	{
		FILE *fsummary = fopen(m_cPara->m_vstrSummary[i].c_str(), "wt");
		if(NULL == fsummary)
		{
			cout << "[Error] Failed to open file: " << m_cPara->m_vstrSummary[i] << endl;
			continue;
		}

		int posEnd = m_cPara->m_vSpecFiles[i].find_last_of('.');
		if(posEnd == string::npos)
		{
			posEnd = m_cPara->m_vSpecFiles[i].size();
		}
		int posSrt = m_cPara->m_vSpecFiles[i].find_last_of('\\');
		if(posSrt == string::npos)
		{
			posSrt = -1;
		}
		string fileName = m_cPara->m_vSpecFiles[i].substr(posSrt+1, posEnd - posSrt - 1);
		fprintf(fsummary, "%s\n", fileName.c_str());
		fprintf(fsummary, "Total MS/MS Spectrum: %d\n", m_vSpecNum[i]);
		fprintf(fsummary, "Total MS/MS Scan: %d\n", m_vScanNum[i]);
		if(m_vScanNum[i] > 0)
		{
			fprintf(fsummary, "ID Rate:%d / %d = %.2f%%\n", m_vIdScan[i], m_vScanNum[i], m_vIdScan[i] * 100.0 / m_vScanNum[i]);
		}

		fclose(fsummary);
	}
}

void CMainFlow::RunMainFlow()
{
	/**
	*	Print verson and welcome UI of pTop
	*	and start the timer 
	**/
	Welcome();

	time_t t_start,t_end;
	t_start = clock();

	/**
	*	Get all configuration info from the config.ini
	*	Do some essential check for robust
	**/
	try
	{
		//读取参数文件中的参数信息，调用pParseTD，生成输出文件路径
		cout << "[pTop] Configure..." << endl;
		m_clog->info("Configure...");

		m_cPara->GetAllConfigPara(); // Main parameters from config.ini
		cout << endl;

		cout << "[pTop] Database: " << m_cPara->m_strDBFile << endl;
		m_clog->info("Database: " + m_cPara->m_strDBFile);
		//cout << "[pTop] Spectra: " << m_cPara->m_strSpecFile << endl;
	} 
	catch(exception & e) 
	{
		CErrInfo info("CMainFlow", "RunMainFlow", "Configure failed.");
		throw runtime_error(info.Get(e).c_str());
	}
	catch(...) 
	{
		CErrInfo info("CMainFlow", "RunMainFlow", "caught an unknown exception from GetAllConfigPara().");
		throw runtime_error(info.Get().c_str());
	}
	
	/**
	*	Get all variable and fixed PTMs
	*	Chcek if there is any confics
	*	Calculate all the PTM forms
	*	Init the fix mod info
	**/
	vector<MODINI> fixmodInfo;
	try
	{
		cout << "[pTop] Getting the PTM forms..." << endl;
		m_clog->info("Getting the PTM forms...");

		m_cPTMForms->SetMaxModNum(m_cPara->m_nMaxVarModNum);
		m_cPTMForms->SetModInfo(m_cPara->m_vFixMod, m_cPara->m_vVarMod);// Search for the modification.ini, get more detail PTM info
		m_cPTMForms->CalculateModForm();

		m_cPTMForms->GetFixModInfo(fixmodInfo);

		m_clog->info("Init AA mass table...");

		m_cMapMass->Init();// Init AA mass table

		m_clog->info("Add fixed modifications...");

		for(size_t i = 0; i < fixmodInfo.size(); ++i)  // 添加固定修饰（未考虑中性丢失）
		{
			if('n' == fixmodInfo[i].cType)  // normal
			{
				for(size_t j = 0; j < fixmodInfo[i].strSite.length(); ++j)
				{
					m_cMapMass->AddAAMassbyMod(fixmodInfo[i].strSite[j], fixmodInfo[i].lfMass);
				}
			} else if('N' == fixmodInfo[i].cType) {
				m_cMapMass->SetNtermMass(fixmodInfo[i].lfMass, fixmodInfo[i].strSite);
			} else {
				m_cMapMass->SetCtermMass(fixmodInfo[i].lfMass, fixmodInfo[i].strSite);
			}
		}
	}
	catch(exception & e) 
	{
		CErrInfo info("CMainFlow", "RunMainFlow", "GetPTMforms failed.");
		throw runtime_error(info.Get(e).c_str());
	}
	catch(...) 
	{
		CErrInfo info("CMainFlow", "RunMainFlow", "caught an unknown exception from CPrePTMForm.");
		throw runtime_error(info.Get().c_str());
	}

	t_end = clock();
	cout << "[pTop] Time elapsed: " << (t_end - t_start) / 1000.0 << "s" <<endl;

	m_clog->info("Time elapsed: %f s", (t_end - t_start) / 1000.0);

	/**
	*	Init tag seacher, title of query results, and output plabel files
	*	Create protein index
	*	Create tag index
	*	Call searcher to finish the search
	**/
	int nDBnum = 0;
	CTagSearcher *cTagSearcher = new CTagSearcher;
	cTagSearcher->Init(m_cMapMass, m_cPTMForms, m_cPara->m_lfFragTol);
	try
	{
		// 每个结果文件和pLabel文件头的书写，在分库搜索之前
		for(size_t i = 0; i < m_cPara->m_vSpecFiles.size(); ++i)
		{
			FILE *fqryRes = fopen(m_cPara->m_vstrQryResFile[i].c_str(), "w");
			if(NULL == fqryRes)
			{
				cout << "[Error] Failed to open output file: " << m_cPara->m_vstrQryResFile[i] << endl;
				exit(0);
			}
			fprintf(fqryRes, "FileBlock\tTitle\tCharge_State\tPrecursor_Mass\tTheory_Mass\tScore\tEvalue\tMatched_Peaks\tNterm_MatchedIons\tCterm_MatchedIons\tNterm_MatchedIntensityRaio\tCterm_MatchedIntensityRatio\tisDecoy\tProtein_AC\tProtein_Sequence\tPTMs\n");
			fclose(fqryRes);

			if(m_cPara->m_vfLabelFiles[i] != NULL) 
			{
				OutputLabelHead(m_cPara->m_vfLabelFiles[i], m_cPara->m_vSpecFiles[i], fixmodInfo);
			}	
		}

		CCreateIndex *cProIndex = new CCreateIndex(m_cPara->m_strDBFile, m_cMapMass);
		CTagFlow *cTagIndex = new CTagFlow(m_cMapMass);


		while(1)
		{
			++nDBnum;

			//MEMORYSTATUS memStatus;
			//GlobalMemoryStatus(&memStatus);
			//cout<<"Begin: Avali Pspace = "<<memStatus.dwAvailVirtual<<endl<<endl;

			cout << "[pTop] Indexing..." << endl;
			m_clog->info("Indexing...");

			bool stop = cProIndex->IndexofProtein();
			m_nTotalProNum += cProIndex->m_ProteinList.size();

			//cout << "[pTop] Create tag index..." << endl;
			cTagIndex->Clear();
			//cTagIndex->CreateTagIndex1(vTagTbl, cProIndex->m_ProteinList, cProIndex->GetProTotalLen());
			cTagIndex->CreateTagIndex(cProIndex->m_ProteinList);

#ifdef _DEBUG_MAP
			char testfname[20];
			sprintf(testfname, "test%d.txt", nDBnum);
			FILE *ftest = fopen(testfname, "w");
			for(unordered_map<long int, vector<UINT_UINT> >::iterator it = cTagIndex->m_mapKey2Pos.begin(); it != cTagIndex->m_mapKey2Pos.end(); ++it)
			{
				fprintf(ftest, "%ld->", it->first);
				for(int tt = 0; tt < (it->second).size(); ++tt)
				{
					fprintf(ftest, "%d ", (it->second)[tt]);
				}
				fprintf(ftest, "\n");
			}
			fclose(ftest);
#endif

			cout << "[pTop] Begin to search..." << endl;
			m_clog->info("Begin to search...");

			Searcher(cProIndex->m_ProteinList, cTagIndex, cTagSearcher);

			//cout << "[pTop] Search finished!" << endl;
			//cout << "[pTop] The results in "<< m_cPara->m_strQryResFile<<endl;
			cTagIndex->Clear();

			if(stop == true) break;
		}

		delete cTagIndex;
		delete cProIndex;
	}
	catch(exception & e) 
	{
		CErrInfo info("CMainFlow", "RunMainFlow", "Searcher failed.");
		throw runtime_error(info.Get(e).c_str());
	}
	catch(...) 
	{
		CErrInfo info("CMainFlow", "RunMainFlow", "caught an unknown exception from Searcher.");
		throw runtime_error(info.Get().c_str());
	}

	delete cTagSearcher;
	t_end = clock();
	cout << "[pTop] Time elapsed: " << (t_end - t_start) / 1000.0 << "s" <<endl;

	m_clog->info("Time elapsed: %f s", (t_end - t_start) / 1000.0);

	/**
	*	Second Search
	**/
	if(SECONDSEARCH)
	{
		try
		{
			cout << "[pTop] Second Search..." << endl;
		
			m_clog->info("Second Search...");

			CResFilter filter(0.01, m_cPara->m_vstrQryResFile, m_cPara->m_vstrFilterResFile, nDBnum);
			vector<string> vIdSpec; // 按照scan号有序，不同raw的放在一起
			vector<PROTEIN_STRUCT> proteinDB;
			for(size_t qf = 0; qf < m_cPara->m_vstrQryResFile.size(); ++qf)
			{
				filter.FirstFilter(m_cPara->m_inFileType, qf, vIdSpec, proteinDB);
			}

			for(size_t pId = 0; pId < proteinDB.size(); pId += 2)
			{
				proteinDB[pId].lfMass = m_cMapMass->CalculateMass(proteinDB[pId].strProSQ.c_str());
				proteinDB[pId + 1].lfMass = proteinDB[pId].lfMass;
			}
			/*for(size_t tt = 0; tt < vIdSpec.size(); ++tt)
			{
				cout << vIdSpec[tt] << endl;
			}*/
			if(proteinDB.size() > 0)
			{
				SecondSearcher(proteinDB, vIdSpec);
			}
		}
		catch(exception & e) 
		{
			CErrInfo info("CMainFlow", "RunMainFlow", "Second Search failed.");
			throw runtime_error(info.Get(e).c_str());
		}
		catch(...) 
		{
			CErrInfo info("CMainFlow", "RunMainFlow", "caught an unknown exception from the Second Search.");
			throw runtime_error(info.Get().c_str());
		}
	}
	
	/**
	*	Filter part
	**/
	try
	{
		cout << "[pTop] Begin to filter..." << endl;
		m_clog->info("Begin to filter...");

		m_vIdScan.assign(m_vScanNum.size(), 0);
		CResFilter filter(m_cPara->m_lfThreshold, m_cPara->m_vstrQryResFile, m_cPara->m_vstrFilterResFile, nDBnum);
		filter.Run(m_cPara->m_vfLabelFiles, m_cPTMForms, fixmodInfo, m_vIdScan);
	
		for(size_t i = 0; i < m_cPara->m_vfLabelFiles.size(); ++i)
		{
			if(m_cPara->m_vfLabelFiles[i] != NULL)
			{
				fclose(m_cPara->m_vfLabelFiles[i]);
				m_cPara->m_vfLabelFiles[i] = NULL;
			}
		}
		//cout << "[pTop] Fileter finished!" << endl;
		//cout << "[pTop] The results in "<<m_cPara->m_strFilterResFile<<endl;
	}
	catch(exception & e) 
	{
		CErrInfo info("CMainFlow", "RunMainFlow", "Filter failed.");
		throw runtime_error(info.Get(e).c_str());
	}
	catch(...) 
	{
		CErrInfo info("CMainFlow", "RunMainFlow", "caught an unknown exception from Filter.");
		throw runtime_error(info.Get().c_str());
	}

	m_clog->info("Output summary...");

	OutputSummary();

	t_end = clock();
	cout << "[pTop] Time elapsed: " << (t_end - t_start) / 1000.0 << "s" << endl;
	m_clog->info("Time elapsed: %f s", (t_end - t_start) / 1000.0);
	
}