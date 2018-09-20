#ifndef _CONFIGURATION_H_
#define _CONFIGURATION_H_

#include <iostream>
#include <cstdio>
#include <cstring>
#include <limits.h>
#include <vector>
#include "sdk.h"
#include "util.h"

using namespace std;

enum MOD_TYPE
{
	MOD_NORMAL = 0,
	MOD_PRO_C = 1,
	MOD_PRO_N = 2,
	//MOD_PEP_C = 3,
	//MOD_PEP_N = 4,
	MOD_UNKNOWN = 5
};

class CConfiguration{
public:
	double m_lfPrecurTol;
	double m_lfFragTol;
	double m_lfThreshold;
	double m_lfMaxModMass;
	double m_lfMinFragMass;  //
	double m_lfMaxFragMass;  // for ion index
	double m_lfMaxTruncatedMass;  //

	int m_nMaxModNum;
	int m_nUnexpectedModNum;  // added by wrm @2016.12.1 
	int m_nOutputTopK;
	int m_nThreadNum;    // number of threads, m_nProcessorNo number of cpu
	WorkFlowType m_eWorkFlow;
	MS2FormatType m_inFileType; // 0->mgf 1->PF

	 
	string m_strFileType; // raw / mgf / pf
	string m_strDBFile;
	string m_strpParseTD;
	string m_strActivation;

	vector<string> m_vFixMod;
	vector<string> m_vVarMod;
	vector<MODINI> m_vFixModInfo;
	vector<MODINI> m_vVarModInfo;

	vector<string> m_vSpecFiles;
	vector<string> m_vstrFilenames;   // file names for folder names when separate filtering
	vector<vector<string>> m_vstrQryResFile;  // paths for query results
	
	// [wrm] used for quantification. 2015.11.12
	vector<LABEL_QUANT> m_quant;
	string m_strpQuant;   // pQuant.cfg
	
	//
	string m_strBIonsSites;  // C端易断裂的氨基酸
	string m_strYIonsSites;  // N端易断裂的氨基酸

	// [wrm] used for output to a given path. 2016.01.13
	string m_strOutputPath;

	bool calAX;
	bool calBY;
	bool calCZ;

	bool m_bNeutralLoss;  //
	bool m_bRerank;   // rescore with svm
	bool m_bSeparateFiltering; //
	bool m_bSecondSearch;
	bool m_bTagLenInc;   // whether create 5-tag index

	unordered_map<string, double> m_ElemMass;     // 各元素的mono质量

	CConfiguration(const string strFile);
	~CConfiguration();

	static string GetTimeStr();
	static void MergeFiles(const vector<string> &vSrc, const string &strDst, const string &heads, bool bDelete = true);
	static void Message(const string & msg, LOG_LEVEL lt = LL_INFORMATION);
	static void parseFileTitle(const string &title, const string delim, PRSM_Column_Index &columns);

	void GetAllConfigPara();
	void writeConfigFile();
	void CallpParseTD();
	void PrintParas(int idx, string &outpath);
	void LoadElement();
	void GetElementMass(unordered_map<string, double> &elemMass);
	void ReadModInfo();

private:
	string m_strIniFile;

	void _GetFilelistbyDirent(string &filepath, string fileType, vector<string> &fileList);
	void _CheckPath();	
	void _GenTmpAA(const int labelID);
	void _GenTmpMod(const int labelID);
	string getModInfo(const string strVal, MODINI &tmpMod, int passNumber);
	void setFragmentType();
};

#endif