#ifndef _SDK_H_
#define _SDK_H_

#include <stdio.h>
#include <cstdio>
#include <cstring>
#include <string.h>
#include <math.h>
#include <string>
#include <vector>
#include <set>
#include <map>
#include <unordered_map>
#include <unordered_set>
#include <queue>
#include <algorithm>
#include <functional>
#include <iostream>
#include <fstream>
#include <iomanip>
#include <sys/stat.h>
#include <pthread.h>
using namespace std;

#ifdef _WIN64 // 64-bit Windows Compiler
#include <windows.h>
#define FORMAT_SIZE_T "%zu"
#define FORMAT_SIZE_XT "%llx"
#define FORMAT_UINT64 "%llu"
const char cSlash = '\\';
#elif _WIN32 // 32-bit Windows Compiler
#include <windows.h>
#define FORMAT_SIZE_T "%u"
#define FORMAT_SIZE_XT "%x"
#define FORMAT_UINT64 "%I64d"
const char cSlash = '\\';
#else // Linux Compiler
#include <unistd.h>
#define FORMAT_SIZE_T "%u"
#define FORMAT_SIZE_XT "%x"
#define FORMAT_UINT64 "%I64d"
const char cSlash = '/';
#endif

#ifdef _WIN64
const size_t SHIFT_BITS = 63;
const size_t SET_MASK = 0x8000000000000000;   // b ions
const size_t RECOVER_MASK = 0x7fffffffffffffff;  // y ions
const size_t PROID_MASK = 0x7fffffffffffffff;

const size_t SHIFT_HIGH_BITS = 32;
const size_t HIGH_MASK = 0xffffffff00000000;
const size_t LOW_MASK = 0x00000000ffffffff;

const size_t ALL_BIT_ON = 0xffffffffffffffff;

#else
const size_t SHIFT_BITS = 31;
const size_t SET_MASK = 0x80000000;
const size_t RECOVER_MASK = 0x7fffffff;
const size_t PROID_MASK = 0x7fffffff;

const size_t SHIFT_HIGH_BITS = 16;
const size_t HIGH_MASK = 0xffff0000;
const size_t LOW_MASK = 0x0000ffff;

const size_t ALL_BIT_ON = 0xffffffff;
#endif


const int READLEN = 256; // 读入配置文件时读取一行的最大长度
//const int tagFlow = 1;
//const int combinedFlag = 0;

const int BUFFERSIZE = 20971520;
const int STR_BUF_SIZE = 1024;
const int PROAC_LEN = 100;
const int MAXSCANNUM = 100000;  // 每个raw最大的Scan数目
//const int REVERSE = 1;
const int BUFLEN = 10000;
const int MAX_SHIFT_MASS = 2000;  // 目前考虑的修饰质量范围，将shift扩大为-10,000 ~ 10,000
const int MAX_MOD_MASS = 500;
const int ALPHABET_SIZE = 26;
const int MIN_TAG_LEN = 3;
const int TAG_LEN = 4;
//const int TAG_TYPE = 0x10;   // 4: 0001 0000 = 0x10  5: 0011 0000 = 0x30
const int TAGDashift = 0; // 是否在提取TAG的时候考虑正负1Da的偏差
//const int TAG3MAX = 17575;  // 26^3 - 1
//const int TAG4MAX = 456975;  // 26^4 - 1
//const int TAG5MAX = 11881375; // 26^5 - 1

//const int MAX_PRO_NUM = 50000; // 分库处理，每次处理5万个蛋白
const int MAX_SPECTRA_NUM = 5000; // 分谱，每5000张搜索一次
const long int MAX_PRO_BLOCK = 20971520; // 每次处理20M=20971520的蛋白质,50M = 52428800, 80M = 83886080
const int FIXED_PRECISION = 6;
const string NONMOD = "NULL";
const string VERSION = "pTop v2.0";
const double DIFF_13C = 1.0033548378;
const double AAMass_M_mono = 131.04048;
const double IonMass_Proton = 1.00727647012;
const double IonMass_Mono_H = 1.007825035;
const double IonMass_Mono_C = 12;
const double IonMass_Mono_N = 14.003074;
const double IonMass_Mono_O = 15.99491463;
const double IonMass_H2O = 2 * IonMass_Mono_H + IonMass_Mono_O;
const double IonMass_NH3 = IonMass_Mono_N + 3 * IonMass_Mono_H;

const double AVG_AA_MASS = 110; // 108.8; // 111.05;
const double DOUCOM_EPS = 1.0e-10;// 用于double数据比较的较小值
const double pi = 3.1415926;

//const int MAXSQSIZE = 1000000; // Maximum buffer for protein sequence
const int WIND = 1000;         // Precurser mass window (Da)
const int TOPINTEN = 400;     // 400 Preprocessing the MS2, keep the top k intensity peeks
const size_t TOPSCORE = 20;       // Keep the top k score PSMs
const size_t MZ_INDEX_SCALE = 100;
const size_t MZ_SCALE = 10;
const int TOPK_SETS = 100;       // Keep the top k scored-sets on each level of DP
const int TOP_K = 10;           // Output the top k PSMs for each spctra
const int MAX_HASH_SIZE = 200000; // max mass of the protein
const string strETD = "ETD";
const string strCID = "CID";
//const bool bTagFlow = true;
const int TOP_PATH_NUM = 5;   // 10, keep top k paths after modification location

const unsigned int MAX_TAG_NUM = 512;
const int MAX_CANDIPRO_NUM = 100;
const int MAX_ION_NUM = 20000;

const double UNKNOWN_QVALUE = 100.0; // unreachable

const int ActivateTypeNum = 3;
const char cActivateType[ActivateTypeNum][8] = {"CID","ETD","HCD"};
const string BIONSITES = "DEV";  // DV
const string YIONSITES = "GPY";  // AP ALP

//---simplified data types
typedef unsigned short int USHORT;
typedef unsigned int UINT;
typedef unsigned char byte;
typedef unsigned long long int ULLINT;

typedef multimap<char, int>::iterator MAP_IT;
typedef multiset<int>::iterator MULTISET_IT;

struct SPEC_ID_TITLE
{
	int nIdx;
	string strTitle;
	SPEC_ID_TITLE(int n, string s)
	{
		this->nIdx = n;
		this->strTitle = s;
	}
	bool operator < (const SPEC_ID_TITLE &tmp) const
    {
		return this->nIdx < tmp.nIdx;
    }
};

// add by wrm. @2016.05.13
struct PrSM_Matched_Info{
	int nNterm_matched_ions;  // 匹配到的N端离子数目 a,b,c
	int nCterm_matched_ions;  // 匹配到的C端离子数目 x,y,z
	double lfNtermMatchedIntensityRatio;  // N端匹配到的谱峰强度比
	double lfCtermMatchedIntensityRatio;  // C端匹配到的谱峰强度比

	PrSM_Matched_Info() :nNterm_matched_ions(0), nCterm_matched_ions(0),
		lfNtermMatchedIntensityRatio(0), lfCtermMatchedIntensityRatio(0){

	}
	PrSM_Matched_Info(const PrSM_Matched_Info &co){
		this->nNterm_matched_ions = co.nNterm_matched_ions;
		this->nCterm_matched_ions = co.nCterm_matched_ions;
		this->lfNtermMatchedIntensityRatio = co.lfNtermMatchedIntensityRatio;
		this->lfCtermMatchedIntensityRatio = co.lfCtermMatchedIntensityRatio;
	}
	PrSM_Matched_Info(int _nions_ratio, int _cions_ratio, double n_intensity_ratio, double c_intensity_ratio){
		nNterm_matched_ions = _nions_ratio;
		nCterm_matched_ions = _cions_ratio;
		lfNtermMatchedIntensityRatio = n_intensity_ratio;
		lfCtermMatchedIntensityRatio = c_intensity_ratio;
	}
	PrSM_Matched_Info& operator=(const PrSM_Matched_Info &other){
		nNterm_matched_ions = other.nNterm_matched_ions;
		nCterm_matched_ions = other.nCterm_matched_ions;
		lfNtermMatchedIntensityRatio = other.lfNtermMatchedIntensityRatio;
		lfCtermMatchedIntensityRatio = other.lfCtermMatchedIntensityRatio;
		return *this;
	}

};

// modified by wrm @2016.06.20
struct FeatureInfo{
	double lfCom_ions_ratio;  // 互补离子占匹配到的离子的比例
	double lfTag_ratio;  // 最长TAG比例
	double lfPTM_score;  // 支持修饰的离子比例
	double lfHL_pair_score; // 轻重对儿离子占匹配到的离子的比例
	double lfFragment_error_std;  // 碎片离子误差方差

	FeatureInfo() :lfCom_ions_ratio(0), lfTag_ratio(0), lfPTM_score(0), lfHL_pair_score(0), lfFragment_error_std(0){

	}
	FeatureInfo(double _com_ions, double _tag_ratio, double _ptm_score, double _hl_score, double _fragment_error){
		lfCom_ions_ratio = _com_ions;
		lfTag_ratio = _tag_ratio;
		lfPTM_score = _ptm_score;
		lfHL_pair_score = _hl_score;
		lfFragment_error_std = _fragment_error;
	}
	FeatureInfo(const FeatureInfo &co){
		this->lfCom_ions_ratio = co.lfCom_ions_ratio;
		this->lfTag_ratio = co.lfTag_ratio;
		this->lfPTM_score = co.lfPTM_score;
		this->lfHL_pair_score = co.lfHL_pair_score;
		this->lfFragment_error_std = co.lfFragment_error_std;
	}
	FeatureInfo& operator=(const FeatureInfo &other){
		lfCom_ions_ratio = other.lfCom_ions_ratio;
		lfTag_ratio = other.lfTag_ratio;
		lfPTM_score = other.lfPTM_score;
		lfHL_pair_score = other.lfHL_pair_score;
		lfFragment_error_std = other.lfFragment_error_std;
		return *this;
	}
};


struct PRSM
{
	byte nfileID;   // file index
	int nScan;
	int nCharge;
	int nMatchedPeakNum;
	int nIsDecoy;

    double lfPrecursorMass;
	double lfProMass;
    double lfScore;
	double lfEvalue;
	double lfQvalue;
	
	string strSpecTitle;
    string strProSQ;
    string strProAC;
    string vModInfo;

	PrSM_Matched_Info cMatchedInfo;
	FeatureInfo cFeatureInfo;

	byte nLabelType;   // [wrm] new add 2015.11.19

	PRSM(byte n0 = 0, int n1 = 0, int n2 = 0, int n3 = 0, 
		double f1 = 0, double f2 = 0, double f3 = 0, double f4 = 1,
		const string &s1 = "", const string &s2 = "", const string &s3 = "", const string &s4 = "",
		int b1 = 0, byte b2 = 0): 
	    nfileID(n0), nScan(n1), nCharge(n2), nMatchedPeakNum(n3), 
		lfPrecursorMass(f1), lfProMass(f2), lfScore(f3), lfEvalue(f4),
		strSpecTitle(s1), strProSQ(s2),strProAC(s3), vModInfo(s4),
		nIsDecoy(b1), nLabelType(b2), lfQvalue(UNKNOWN_QVALUE)
	{
		
	}

	PRSM(byte n0, int n1, int n2, int n3, double f1, double f2, double f3, double f4,
		const string &s1, const string &s2, const string &s3, const string &s4,
		PrSM_Matched_Info &matchedInfo, int b1 = 0, byte b2 = 0) :
		nfileID(n0), nScan(n1), nCharge(n2), nMatchedPeakNum(n3),
		lfPrecursorMass(f1), lfProMass(f2), lfScore(f3), lfEvalue(f4),
		strSpecTitle(s1), strProSQ(s2), strProAC(s3), vModInfo(s4),
		cMatchedInfo(matchedInfo), nIsDecoy(b1), nLabelType(b2), lfQvalue(UNKNOWN_QVALUE)
	{

	}

	PRSM(const PRSM &co){
		this->nfileID = co.nfileID;
		this->nScan = co.nScan;
		this->nCharge = co.nCharge;
		this->nMatchedPeakNum = co.nMatchedPeakNum;
		this->lfPrecursorMass = co.lfPrecursorMass;
		this->lfProMass = co.lfProMass;
		this->lfScore = co.lfScore;
		this->lfEvalue = co.lfEvalue;
		this->lfQvalue = co.lfQvalue;
		this->strSpecTitle = co.strSpecTitle;
		this->strProSQ = co.strProSQ;
		this->strProAC = co.strProAC;
		this->vModInfo = co.vModInfo;
		this->cMatchedInfo = co.cMatchedInfo;
		this->cFeatureInfo = co.cFeatureInfo;
		this->nIsDecoy = co.nIsDecoy;
		this->nLabelType = co.nLabelType;
	}

	PRSM(byte n0, int n1, int n2, int n3, double f1, double f2, double f3, double f4, 
		char s1[], char s2[], char s3[], char s4[], 
		PrSM_Matched_Info &matchedInfo, int b1 = 0, byte b2 = 0) : cMatchedInfo(matchedInfo), lfQvalue(UNKNOWN_QVALUE)
	{
		this->nfileID = n0;
		this->nScan = n1;
		this->nCharge = n2;
		this->nMatchedPeakNum = n3;
		this->lfPrecursorMass = f1;
		this->lfProMass = f2;
		this->lfScore = f3;
		this->lfEvalue = f4; 
		this->strSpecTitle = s1;
        this->strProSQ = s2;
		this->strProAC = s3;
        this->vModInfo = s4;
		this->nIsDecoy = b1;
		this->nLabelType = b2;
	}
    PRSM & operator =(const PRSM &co)
    {
		this->nfileID = co.nfileID;
		this->nScan = co.nScan;
		this->nCharge = co.nCharge;
		this->nMatchedPeakNum = co.nMatchedPeakNum;
		this->lfPrecursorMass = co.lfPrecursorMass;
		this->lfProMass = co.lfProMass;
		this->lfScore = co.lfScore;
		this->lfEvalue = co.lfEvalue; 
		this->lfQvalue = co.lfQvalue;
		this->strSpecTitle = co.strSpecTitle;
        this->strProSQ = co.strProSQ;
		this->strProAC = co.strProAC;
        this->vModInfo = co.vModInfo;
		this->cMatchedInfo = co.cMatchedInfo;
		this->cFeatureInfo = co.cFeatureInfo;
		this->nIsDecoy = co.nIsDecoy;
		this->nLabelType = co.nLabelType;
        return *this;
    }
    bool operator < (const PRSM &prsm) const
    {
		return lfEvalue < prsm.lfEvalue;
    }
	bool operator >(const PRSM &prsm) const  // 
	{
		return lfScore > prsm.lfScore;
	}
	static bool ScoreGreater(const PRSM &p1, const PRSM &p2)
	{
		return p1.lfScore > p2.lfScore;
	}
	static bool ConfidentByQV(const PRSM &p1, const PRSM &p2)
	{
		if (p1.lfQvalue == p2.lfQvalue){
			return p1.lfEvalue < p2.lfEvalue;
		}
		return p1.lfQvalue < p2.lfQvalue;
	}
};

struct PROTEIN_STRUCT{
    //int nACstart;         // Start site of AC of the protein
    //int nAClen;           // Length of AC of the protein
    //int nSQstart;         // Start site of SQ of the protein
    //int nSQlen;           // Length of SQ of the protein
    double lfMass;          // Mass of the protein
    int nIsDecoy;
	string strProAC;
	string strProDE;
	string strProSQ;
};

struct UINT_UINT{
    unsigned int nFirst;
    unsigned int nSecond;
    UINT_UINT(const unsigned int a = 0, const unsigned int b = 0)
    {
        nFirst = a;
        nSecond = b;
    }
    UINT_UINT(const UINT_UINT &db)
    {
        nFirst = db.nFirst;
        nSecond = db.nSecond;
    }
	bool operator == (const UINT_UINT &oth) const
	{
		return nFirst == oth.nFirst && nSecond == oth.nSecond;
	}

	bool operator < (const UINT_UINT &oth) const
	{
		return nFirst < oth.nFirst;
	}
	static bool cmpByFirstInc(const UINT_UINT &a, const UINT_UINT &b)
	{
		return a.nFirst < b.nFirst;
	}
	static bool cmpBySecondInc(const UINT_UINT &a, const UINT_UINT &b)
	{
		return a.nSecond < b.nSecond;
	}
};


struct PROTEIN_SPECTRA_MATCH{
	double lfMass;
	double lfScore;
	double lfQvalue;
	int nMatchedPeakNum;
	int nIsDecoy;
	string strProSQ;
	string strProAC;
	vector<UINT_UINT> vModSites; //<modID:modSite>
	PrSM_Matched_Info matchedInfo;
	FeatureInfo featureInfo;
	PROTEIN_SPECTRA_MATCH() :lfMass(0), lfScore(0), lfQvalue(1.0), nMatchedPeakNum(0), nIsDecoy(0), strProSQ(""), strProAC("")
	{

	}
	PROTEIN_SPECTRA_MATCH& operator =(const PROTEIN_SPECTRA_MATCH &co)
	{
		this->lfMass = co.lfMass;
		this->lfScore = co.lfScore;
		this->lfQvalue = co.lfQvalue;
		this->nMatchedPeakNum = co.nMatchedPeakNum;
		this->nIsDecoy = co.nIsDecoy;
		this->strProSQ = co.strProSQ;
		this->strProAC = co.strProAC;
		this->vModSites = co.vModSites;
		this->matchedInfo = co.matchedInfo;
		this->featureInfo = co.featureInfo;
		return *this;
	}
	bool operator < (const PROTEIN_SPECTRA_MATCH &prsm) const
	{
		return lfScore < prsm.lfScore;
	}

	static bool CmpByScoreDes(const PROTEIN_SPECTRA_MATCH p1, const PROTEIN_SPECTRA_MATCH p2)
	{
		if (p1.lfScore != p2.lfScore)
		{
			return p1.lfScore > p2.lfScore;
		}
		else {
			return p1.vModSites.size() < p2.vModSites.size(); // 分数相等时，修饰越少越可信
		}
	}

	static bool MatchedPeaksLess(const PROTEIN_SPECTRA_MATCH p1, const PROTEIN_SPECTRA_MATCH p2)
	{
		return p1.nMatchedPeakNum < p2.nMatchedPeakNum;
	}
};

struct PATH_NODE{
    int nWeight;
	//long long lHashVal;
	double fModMass;
	vector<UINT_UINT> vModSites; //<modID:modSite>
	PATH_NODE() :nWeight(0), fModMass(0) //, lHashVal(0)
    {
        vModSites.clear();
    }
	PATH_NODE(int w, vector<UINT_UINT> &m) :nWeight(w), vModSites(m), fModMass(0) //, lHashVal(0)
    {
    }
	PATH_NODE(int w, double f, vector<UINT_UINT> &m) :nWeight(w), fModMass(f), vModSites(m) //, lHashVal(0)
	{
	}

	PATH_NODE& operator =(const PATH_NODE &node)
	{
		this->nWeight = node.nWeight;
		this->vModSites = node.vModSites;
		this->fModMass = node.fModMass;
		return *this;
	}

	bool operator <(const PATH_NODE &node) const
	{   //priority_queue 默认按照从大到小的顺序排 ，这里直接修改小于的规则，使其从小到大有序
		return nWeight > node.nWeight;
	}

};

struct DP_VERTICE{
    vector<int> sModForm; //The elments in "sModForm" are the numbers of the i_th mod
	priority_queue<PATH_NODE> qPath;
    DP_VERTICE(){}
    DP_VERTICE(vector<int> &form, priority_queue<PATH_NODE> &q)
    {
        sModForm = form;
        qPath = q;
    }
    bool operator ==(const DP_VERTICE &node) // 这里重构了==
    {   // If two vertice have same layer and same mod set, we see they are the same vertice
        return (sModForm == node.sModForm);
    }
	DP_VERTICE& operator =(const DP_VERTICE &node)
	{
		this->qPath = node.qPath;
		this->sModForm = node.sModForm;
		return *this;
	}
};

struct VERTICE{
	unordered_map<int, int> sModForm; //The elments in "sModForm" are the numbers of the i_th mod
	priority_queue<PATH_NODE> qPath;
	VERTICE(){}
	VERTICE(unordered_map<int, int> &form, priority_queue<PATH_NODE> &q)
	{
		sModForm = form;
		qPath = q;
	}
	bool operator ==(const VERTICE &node) // 这里重构了==
	{   // If two vertice have same layer and same mod set, we see they are the same vertice
		return (sModForm == node.sModForm);
	}
	VERTICE& operator =(const VERTICE &node)
	{
		this->qPath = node.qPath;
		this->sModForm = node.sModForm;
		return *this;
	}
};

struct DOUBLE_DOUBLE{
    double lfa;
    double lfb;
};

struct DOUBLE_INT{
    double lfa;
    int nb;
    DOUBLE_INT(double a = 0.0, int b = 0)
    {
        lfa = a;
        nb = b;
    }
    bool operator < (const DOUBLE_INT &node) const
    {
        return (lfa < node.lfa);
    }
};

struct TAG_ITEM{
    long int nKey;
    unsigned int nProID;
    unsigned int nPos;    // Record the position of the tag in the protein sequence
	TAG_ITEM() :nKey(0), nProID(0), nPos(0)
	{
	}
	TAG_ITEM(long int key, unsigned int proid, unsigned int pos) : nKey(key), nProID(proid), nPos(pos)
	{
	}
	static bool _TagValueCmp(const TAG_ITEM &t1, const TAG_ITEM &t2)
	{
		return t1.nKey < t2.nKey;
	}
};

struct DB_TAG_INFO{
	int nFrequency;   // document frequency: number of proteins containing this tag
	vector<UINT_UINT> vProID_Pos;
	DB_TAG_INFO() : nFrequency(0)
	{}
	DB_TAG_INFO(int frequency) : nFrequency(frequency)
	{}
};

// modified by wrm@20170209 @20170424
struct TAGRES{
    double lfScore;
    double lfFlankingMass[2];
    vector<int> vmodID;
    string strTag;
	int nPos;
	TAGRES() : lfScore(0), strTag(""), nPos(0)
	{
		memset(lfFlankingMass, 0, sizeof(lfFlankingMass));
	}
	TAGRES(double sc, const double *f, const vector<int> mods, const string tag) : 
		lfScore(sc), strTag(tag), vmodID(mods), nPos(0)
	{
		lfFlankingMass[0] = f[0];
		lfFlankingMass[1] = f[1];
	}
	TAGRES(const int p, const string tag, const double score, const double *f, const vector<int> mods):
		nPos(p), strTag(tag), lfScore(score), vmodID(mods)
	{
		lfFlankingMass[0] = f[0];
		lfFlankingMass[1] = f[1];
	}
	TAGRES& operator = (const TAGRES &co)
	{
		this->nPos = co.nPos;
		this->strTag = co.strTag;
		this->lfScore = co.lfScore;
		this->lfFlankingMass[0] = co.lfFlankingMass[0];
		this->lfFlankingMass[1] = co.lfFlankingMass[1];
		this->vmodID = co.vmodID;
		return *this;
	}
	bool operator < (const TAGRES t) const
	{
		return lfScore < t.lfScore;
	}

	static bool TagSeqLess(const TAGRES& t1, const TAGRES& t2)
	{
		if (t1.strTag == t2.strTag){
			return t1.lfScore > t2.lfScore;
		}
		return t1.strTag < t2.strTag;
	}
	static bool ProPosLess(const TAGRES & t1, const TAGRES & t2)
	{
		if (t1.nPos == t2.nPos){
			return t1.lfScore > t2.lfScore;
		}
		return t1.nPos < t2.nPos;
	}
};

struct PROID_NODE{
    size_t nProID;
    double lfScore;
    vector<TAGRES> vTagInfo;
	PROID_NODE(){}
	PROID_NODE(size_t proid):nProID(proid),lfScore(0){

	}

    bool operator < (const PROID_NODE p) const
    {
        return lfScore < p.lfScore;
    }
};

struct CANDI_PROTEIN{
	size_t nProID;
	string strSeq;
	double lfMass;
	double lfScore;
	CANDI_PROTEIN() : nProID(0), strSeq(""), lfMass(0), lfScore(0)
	{}
	CANDI_PROTEIN(size_t proid, string seq, double mass, double score) : nProID(proid), strSeq(seq), lfMass(mass), lfScore(score)
	{

	}
	bool operator < (const CANDI_PROTEIN p) const
	{
		return lfScore < p.lfScore;
	}
	bool operator > (const CANDI_PROTEIN p) const
	{
		return lfScore > p.lfScore;
	}
	static bool LESS(const CANDI_PROTEIN &a, const CANDI_PROTEIN &b)
	{
		return a.lfScore < b.lfScore;
	}
	static bool LARGER(const CANDI_PROTEIN &a, const CANDI_PROTEIN &b)
	{
		return a.lfScore > b.lfScore;
	}
};

struct FRAGMENT_ION{
    double lfMz;
    double lfIntens;
	FRAGMENT_ION(double a = 0.0, double b = 0.0) : lfMz(a), lfIntens(b)
    { // Note: struct doesn't have default constructor
        
    }
	static bool PeakIntDesCmp(const FRAGMENT_ION &d1, const FRAGMENT_ION &d2)
	{
		return d1.lfIntens > d2.lfIntens;
	}
	static bool PeakMzIncCmp(const FRAGMENT_ION &d1, const FRAGMENT_ION &d2)
	{
		return d1.lfMz < d2.lfMz;
	}

};

struct COMBINED_IONS{
    double lfMz;
    double lfIntens;
	//int isOrigiPeak;
    COMBINED_IONS(double a = 0.0, double b = 100.0 /*, int c = 0*/ )
    { // Note: Three is no default constructor in struct
        lfMz = a;
        lfIntens = b;
		//isOrigiPeak = c;
    }

};

struct MZ_SCOPE_ION{
    double lfMz;
    double lfMinMz;
    double lfMaxMz;
    double lfIntens;
	//int isOrigiPeak;
	MZ_SCOPE_ION() :lfMz(0), lfMinMz(0), lfMaxMz(0), lfIntens(0)
	{}
	MZ_SCOPE_ION(double mz, double intens) : lfMz(mz), lfIntens(intens), lfMinMz(0), lfMaxMz(0)
	{
	}
	MZ_SCOPE_ION(double m, double a, double b, double c /*, int d = 0*/) : lfMz(m), lfMinMz(a), lfMaxMz(b), lfIntens(c)
    {
		//isOrigiPeak = d;
    }
	static bool PeakIntDesCmp(const MZ_SCOPE_ION &d1, const MZ_SCOPE_ION &d2)
	{
		return d1.lfIntens > d2.lfIntens;
	}
	static bool PeakMzIncCmp(const MZ_SCOPE_ION &d1, const MZ_SCOPE_ION &d2)
	{
		return d1.lfMz < d2.lfMz;
	}
};

struct PRECURSOR{
    int nPrecursorCharge;
    int nValidCandidateProNum;
	double lfPrecursorMass;   // 1+ mass
    vector<PROTEIN_SPECTRA_MATCH> aBestPrSMs;
	PRECURSOR() : nPrecursorCharge(0), nValidCandidateProNum(0), lfPrecursorMass(0)
	{
		aBestPrSMs.clear();
	}
	PRECURSOR(int chg, double mass, int n)
	{
		nPrecursorCharge = chg;
		nValidCandidateProNum = n;
		lfPrecursorMass = mass;
		aBestPrSMs.clear();
	}
};

struct SPECTRUM
{
	int m_nScanNo;
    int nPrecurNum;
    int nPeaksNum;
	int nCandidateProNum;
	
	string strSpecTitle;
	vector<PRECURSOR> vPrecursors; // precursor information
	vector<FRAGMENT_ION> vPeaksTbl;
	SPECTRUM() : m_nScanNo(0), nPrecurNum(0), nPeaksNum(0), nCandidateProNum(0), 
		strSpecTitle("")
	{
	
	}

	static bool intesityGreater(const FRAGMENT_ION &stPeak1, const FRAGMENT_ION &stPeak2);
	static bool intesityLesser(const FRAGMENT_ION &stPeak1, const FRAGMENT_ION &stPeak2);
	static bool mzGreater(const FRAGMENT_ION &stPeak1, const FRAGMENT_ION &stPeak2);
	static bool mzLesser(const FRAGMENT_ION &stPeak1, const FRAGMENT_ION &stPeak2);

	friend std::ostream &operator<<(std::ostream &stOut, const SPECTRUM &stSpec);
	friend std::fstream &operator<<(std::fstream &stOut, const SPECTRUM &stSpec);
	friend std::istream &operator>>(std::istream &stIn, SPECTRUM &stSpec);
	friend std::fstream &operator>>(std::fstream &stIn, SPECTRUM &stSpec);
	void copyBasicItems(const SPECTRUM &stSpec);
	//void createHashIndex();
};

struct PRSM_Column_Index
{
	int fileID_col;
	int title_col;
	int scan_col;
	int charge_col;
	int precusor_mass_col;
	int theoretical_mass_col;
	int matched_peaks_col;
	int protein_ac_col;
	int protein_sequence_col;
	int ptms_col;
	int score_col;
	int evalue_col;
	int qvalue_col;
	int nmatched_ions_col, cmatched_ions_col, nmatched_intensity_col, cmatched_intensity_col;
	int com_ion_col, tag_ratio_col, ptm_score_col, fragment_error_col;
	int isDecoy_col;
	int label_col;
	PRSM_Column_Index() :fileID_col(-1), title_col(-1), scan_col(-1), charge_col(-1), precusor_mass_col(-1), theoretical_mass_col(-1),
		matched_peaks_col(-1), protein_ac_col(-1), protein_sequence_col(-1), ptms_col(-1), score_col(-1), evalue_col(-1), qvalue_col(-1),
		nmatched_ions_col(-1), cmatched_ions_col(-1), nmatched_intensity_col(-1), cmatched_intensity_col(-1),
		com_ion_col(-1), tag_ratio_col(-1), ptm_score_col(-1), fragment_error_col(-1),
		isDecoy_col(-1), label_col(-1)
	{}

};


struct NODE{
    double lfScore;
    int uName;
    int tPos;
};

struct MODAA{
    char cAA;
    int nModID;
    double lfMass;
};

struct MODINI{
	char cType; // n: Normal, N: PRO_N, C: PRO_C
	double lfMass;
	string strName;
	string strSite;
	vector<double> vNeutralLoss;
};

struct LABEL_ITEM{
	double lfScore;
	string strTitle;
	string strSeq;
	vector<int> vmodSites;
	vector<int> vmodIds;
	LABEL_ITEM(double s, string title, string seq, vector<int> &sites, vector<int> &ids)
	{
		lfScore = s;
		strTitle = title;
		strSeq = seq;
		vmodSites = sites;
		vmodIds = ids;
	}
};

// [wrm]  2015.11.15  added for quantification.

enum ElemIdx
{
	C = 0, H, N, O, S
};

enum FragmentType
{
	NONE,Ion_B,Ion_Y,Ion_A,Ion_X,Ion_C,Ion_Z
};

enum WorkFlowType
{
	TagFlow, IonFlow
};

enum MS2FormatType {
	MFT_MGF,
	MFT_PF,
	MFT_RAW,
	MFT_MS2,
	MFT_DTA,
	MFT_DTAS,
	MFT_PKL,
	MFT_MZML,
	MFT_SDTA
};

struct LABEL_ELEMENT{
	string preElement;   // 待替换的元素
	string labelElement; // 用于替换的元素
};

struct LABEL_QUANT{
	string label_name; // 标记名称, 读取quant.ini
	int labelR_num;   // 氨基酸标记数目
	int labelM_num;   // 修饰标记数目
	unordered_map<char, unordered_map<string,string>> quant_R;         // 一个氨基酸上可能加多种标记, （无序哈希）
	unordered_map<string, unordered_map<string, string>> quant_M;
	LABEL_QUANT(){}
	LABEL_QUANT(int rnum, int mnum)
	{
		labelR_num = rnum;
		labelM_num = mnum;;
	}
};

struct Summary{
	vector<int> m_vSpecNum;
	vector<int> m_vScanNum; // for calculating the id rate
	//vector<int> vChargeScanNum;   
	vector<int> m_vMixSpecScanNums;  // 鉴定到的scan中，混合谱数目，1，2，3，...
	//vector<int> vModScanNums;  // 鉴定到的spectra中，每种修饰对应的spectra数目
	vector<int> m_vIdScanNums;  // 每个RAW鉴定到的scan数目
	int totalIdScanNum;      // 总的鉴定到的scan数目
	int totalIdSpecNum;     // 总的鉴定到的spectra数目
	//int totalIdProNum;      // 总的鉴定到的蛋白数目
	//int totalIdProteoforms; // 总的鉴定到的蛋白质变体的数目
	Summary(): totalIdScanNum(0), totalIdSpecNum(0)
	{}
};

struct ThreadState {
	size_t m_tID;
	size_t m_tCurrentSpectra;  // number of spectra that complete search
	size_t m_tTotalSpectra;

	ThreadState() : m_tID(0), m_tCurrentSpectra(0), m_tTotalSpectra(0)
	{}
};


#endif