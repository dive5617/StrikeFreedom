#ifndef PREDEFINE_H_
#define PREDEFINE_H_

#include <cstdio>
#include <cstring>
#include <string.h>
#include<vector>
#include<set>
#include<map>
#include<queue>
using namespace std;

const bool SECONDSEARCH = false;
const int READLEN = 128; // 读入配置文件时读取一行的最大长度
//const int tagFlow = 1;
//const int combinedFlag = 0;

const int STR_BUF_SIZE = 1024;
const int PROAC_LEN = 100;
//const int REVERSE = 1;
const int BUFLEN = 10000;
const int MAX_SHIFT_MASS = 1000;
const int TAG_LEN = 4;
const int TAGDashift = 0; // 是否在提取TAG的时候考虑正负1Da的偏差
//const int TAG3MAX = 17575;  // 26^3 - 1
//const int TAG4MAX = 456975;  // 26^4 - 1
//const int TAG5MAX = 11881375; // 26^5 - 1

//const int MAX_PRO_NUM = 50000; // 分库处理，每次处理5万个蛋白
const int MAX_SPECTRA_NUM = 5000; // 分谱，每5000张搜索一次
const long int MAX_PRO_BLOCK = 20971520; // 每次处理20M=20971520的蛋白质,50M = 52428800, 80M = 83886080

const string NONMOD = "NULL";
const string VERSION = "pTop v1.2.0";
const double DIFF_13C = 1.0033548378;
const double AAMass_M_mono = 131.04048;
const double IonMass_Proton = 1.00727647012;
const double IonMass_Mono_H = 1.007825035;
const double IonMass_Mono_C = 12;
const double IonMass_Mono_N = 14.003074;
const double IonMass_Mono_O = 15.99491463;

const double AVG_PRO_MASS = 111.1;
const double DOUCOM_EPS = 1.0e-10;// 用于double数据比较的较小值
const double pi = 3.1415926;

//const int MAXSQSIZE = 1000000; // Maximum buffer for protein sequence
const int WIND = 1000;         // Precurser mass window (Da)
const int TOPINTEN = 400;     // Preprocessing the MS2, keep the top k intensity peeks
const size_t TOPSCORE = 20;       // Keep the top k score PSMs
const int TOPK_SETS = 100;       // Keep the top k scored-sets on each level of DP
const int TOP_K = 20;           // Output the top k PSMs for each spctra
const int MAX_HASH_SIZE = 200000; // max mass of the protein
const string strETD = "ETD";
const string strCID = "CID";
//const bool bTagFlow = true;
const int TOP_PATH_NUM = 10;

const unsigned int MAX_TAG = 512;
const int MAX_CANDIPRO_NUM = 100;
const int MAX_ION_NUM = 20000;


const int ActivateTypeNum = 3;
const char cActivateType[ActivateTypeNum][8] = {"CID","ETD","HCD"};

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

struct PRSM
{
	int nIdx;
	int nfileID;
	int nIsDecoy;
	int nCharge;
	int nMatchedPeakNum;
	int ntermIons;  // N端匹配到的离子数目, a/b/c
	int ctermIons;  // C端匹配到的离子数目，x/y/z
    double lfPrecursorMass;
	double lfProMass;
    double lfScore;
	double lfEvalue;
	double ntermMatchedIntensityRatio;  // N端匹配到的谱峰强度比
	double ctermMatchedIntensityRatio;  // C端匹配到的谱峰强度比
	string strSpecTitle;
    string strProSQ;
    string strProAC;
    string vModInfo;
	PRSM(int n0 = 0, int n1 = 0, int n2 = 0, int n3 = 0, int n4 = 0, int n5=0, int n6=0,
		double f1 = 0, double f2 = 0, double f3 = 0, double f4 = 1, double f5 = 0, double f6 = 0,
		string s1 = 0, string s2 = 0, string s3 = 0, string s4 = 0): 
	    nIdx(n0), nfileID(n1), nIsDecoy(n2), nCharge(n3), nMatchedPeakNum(n4), ntermIons(n5), ctermIons(n6), 
		lfPrecursorMass(f1), lfProMass(f2), lfScore(f3), lfEvalue(f4), ntermMatchedIntensityRatio(f5), ctermMatchedIntensityRatio(f6),
		strSpecTitle(s1), strProSQ(s2),strProAC(s3), vModInfo(s4)
	{
		
	}
	PRSM(int n0, int n1, int n2, int n3, int n4, int n5, int n6, double f1, double f2, double f3, double f4, double f5, double f6, char s1[], char s2[], char s3[], char s4[])
	{
		this->nIdx = n0;
		this->nfileID = n1;
		this->nIsDecoy = n2;
		this->nCharge = n3;
		this->nMatchedPeakNum = n4;
		this->ntermIons = n5;
		this->ctermIons = n6;
		this->lfPrecursorMass = f1;
		this->lfProMass = f2;
		this->lfScore = f3;
		this->lfEvalue = f4; 
		this->ntermMatchedIntensityRatio = f5;
		this->ctermMatchedIntensityRatio = f6;
		this->strSpecTitle = s1;
        this->strProSQ = s2;
		this->strProAC = s3;
        this->vModInfo = s4;
	}
    PRSM & operator =(const PRSM &co)
    {
		this->nIdx = co.nIdx;
		this->nfileID = co.nfileID;
		this->nIsDecoy = co.nIsDecoy;
		this->nCharge = co.nCharge;
		this->nMatchedPeakNum = co.nMatchedPeakNum;
		this->ntermIons = co.ntermIons;
		this->ctermIons = co.ctermIons;
		this->lfPrecursorMass = co.lfPrecursorMass;
		this->lfProMass = co.lfProMass;
		this->lfScore = co.lfScore;
		this->lfEvalue = co.lfEvalue; 
		this->ntermMatchedIntensityRatio = co.ntermMatchedIntensityRatio;
		this->ctermMatchedIntensityRatio = co.ctermMatchedIntensityRatio;
		this->strSpecTitle = co.strSpecTitle;
        this->strProSQ = co.strProSQ;
		this->strProAC = co.strProAC;
        this->vModInfo = co.vModInfo;
        return *this;
    }
    bool operator < (const PRSM &prsm) const
    {
        //return lfScore < prsm.lfScore;
		return lfEvalue < prsm.lfEvalue;
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
};

struct PROTEIN_SPECTRA_MATCH{
    double lfMass;  // 理论母离子质量
    double lfScore;
	double lfQvalue;
	double ntermMatchedIntensityRatio;  // N端匹配到的谱峰强度比
	double ctermMatchedIntensityRatio;  // C端匹配到的谱峰强度比
    int nMatchedPeakNum;
	int ntermIons;  // N端匹配到的离子数目, a/b/c
	int ctermIons;  // C端匹配到的离子数目，x/y/z
    int nIsDecoy;
    string strProSQ;
    string strProAC;
    vector<UINT_UINT> vModSites; //<modID:modSite>
	PROTEIN_SPECTRA_MATCH():lfMass(0.0), lfScore(0.0), lfQvalue(1.0), ntermMatchedIntensityRatio(0), ctermMatchedIntensityRatio(0),
		nMatchedPeakNum(0), ntermIons(0), ctermIons(0), nIsDecoy(0),  strProSQ(""), strProAC("")
	{
	}
    PROTEIN_SPECTRA_MATCH& operator =(const PROTEIN_SPECTRA_MATCH &co)
    {
		this->lfMass = co.lfMass;
        this->lfScore = co.lfScore;
		this->lfQvalue = co.lfQvalue;
		this->ntermMatchedIntensityRatio = co.ntermMatchedIntensityRatio;
		this->ctermMatchedIntensityRatio = co.ctermMatchedIntensityRatio;
		this->nMatchedPeakNum = co.nMatchedPeakNum;
		this->ntermIons = co.ntermIons;
		this->ctermIons = co.ctermIons;
        this->nIsDecoy = co.nIsDecoy;
		this->strProSQ = co.strProSQ;
        this->strProAC = co.strProAC;
        this->vModSites = co.vModSites;
        return *this;
    }
    bool operator < (const PROTEIN_SPECTRA_MATCH &prsm) const
    {
        return lfScore < prsm.lfScore;
    }
};

struct PATH_NODE{
    int nWeight;
	vector<UINT_UINT> vModSites; //<modID:modSite>
	PATH_NODE()
    {
        nWeight = 0;
        vModSites.clear();
    }
	PATH_NODE(int w, vector<UINT_UINT> &m)
    {
        nWeight = w;
        vModSites = m;
    }
	PATH_NODE& operator =(const PATH_NODE &node)
	{
		this->nWeight = node.nWeight;
		this->vModSites = node.vModSites;
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
    unsigned int nPos;    // Record the position of the tag in the protein sequence, Only one!!!????
	TAG_ITEM()
	{
		nKey = 0;
		nProID = 0;
		nPos = 0;
	}
};

struct TAGRES{
    double lfScore;
    double lfFlankingMass[2];
    vector<int> vmodID;
    string strTag;

    bool operator < (const TAGRES t) const
    {
        return lfScore < t.lfScore;
    }
};

struct TAG_INFO{
    int nPos;
    double lfDeltaMass;
    vector<int> vmodID;
    TAG_INFO()
    {
        nPos = 0;
        lfDeltaMass = 0;
    }
    TAG_INFO(const int p, const double f, const vector<int> mods)
    {
        nPos = p;
        lfDeltaMass = f;
        vmodID = mods;
    }
    TAG_INFO& operator = (const TAG_INFO &co)
    {
        this->nPos = co.nPos;
        this->lfDeltaMass = co.lfDeltaMass;
        this->vmodID = co.vmodID;
        return *this;
    }
    bool operator < (const TAG_INFO &co) const
    {
        return nPos < co.nPos;
    }
};

struct PROID_NODE{
    size_t nProID;
    double lfScore;
    vector<TAG_INFO> vTagInfo;
	PROID_NODE(){}
	PROID_NODE(size_t proid):nProID(proid),lfScore(0){

	}

    bool operator < (const PROID_NODE p) const
    {
        return lfScore < p.lfScore;
    }
};

struct FRAGMENT_ION{
    double lfMz;
    double lfIntens;
    FRAGMENT_ION(double a = 0.0, double b = 0.0)
    { // Note: struct doesn't have default constructor
        lfMz = a;
        lfIntens = b;
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
    MZ_SCOPE_ION(double m = 0.0, double a = 0.0, double b = 0.0, double c = 0.0 /*, int d = 0*/ )
    {
        lfMz = m;
        lfMinMz = a;
        lfMinMz = b;
        lfIntens = c;
		//isOrigiPeak = d;
    }
};

struct PRECURSOR{
    int nPrecursorCharge;
    int nValidCandidateProNum;
	double lfPrecursorMass;
    vector<PROTEIN_SPECTRA_MATCH> aBestPrSMs;
	PRECURSOR()
	{
		nPrecursorCharge = 0;
		nValidCandidateProNum = 0;
		lfPrecursorMass = 0.0;
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
    int nPrecurNum;
    int nPeaksNum;
	int nCandidateProNum;
	bool isPF;
	string strSpecTitle;
	vector<PRECURSOR> vPrecursors; // precursor information
	vector<FRAGMENT_ION> vPeaksTbl;
	SPECTRUM()
	{
		nPrecurNum = 0;
		nPeaksNum = 0;
		nCandidateProNum = 0;
		isPF = false;
		strSpecTitle = "";
	}
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

#endif