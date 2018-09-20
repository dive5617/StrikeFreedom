/*
*  rerank.h
*
*  Created on: 2016-06-20
*  Author: wang ruimin
*/

#ifndef RERANK_H
#define RERANK_H
#include <io.h>
#include <iterator>
#include <unordered_map>
#include <unordered_set>
#include "../sdk.h"
#include "../util.h"
#include "../config.h"
using namespace std;
const int PrecursorErrorBin = 200;   // 200
const int IterationTimes = 10;
const int MinSampleNum = 50;
const int MaxFeatureNum = 50;

enum NormalizationMethod
{
	e_zscore = 0,
	e_minmax = 1,
	e_log = 2,
	e_atan = 3
};

struct PrSMFeature
{
	double svm_score;  // update every iteration
	double precursor_id;
	double precursor_error;   // ppm 多谱特征, update every iteration
	double mod_ratio;  // 多谱特征，表征修饰的正确率, update every iteration
	double diff_score; // score difference between the first and the last, update every iteration
	vector<string> ptms;
	PRSM basic_info;

	PrSMFeature() :svm_score(0), precursor_id(0), precursor_error(1), mod_ratio(1), diff_score(0)
	{}
	PrSMFeature(const PrSMFeature &pr)
	{
		svm_score = pr.svm_score;
		precursor_id = pr.precursor_id;
		precursor_error = pr.precursor_error;
		mod_ratio = pr.mod_ratio;
		diff_score = pr.diff_score;
		ptms = pr.ptms;
		basic_info = pr.basic_info;
	}
	PrSMFeature(double _svm, double _pParseTDid, double _precur_error, double _mod_ratio, double _diff_sc, vector<string> &_ptms, PRSM _prsm, FeatureInfo _feature)
		: svm_score(_svm), precursor_id(_pParseTDid), precursor_error(_precur_error), mod_ratio(_mod_ratio), diff_score(_diff_sc),
		ptms(_ptms), basic_info(_prsm)
	{

	}
	PrSMFeature& operator=(const PrSMFeature &other){
		svm_score = other.svm_score;
		precursor_id = other.precursor_id;
		precursor_error = other.precursor_error;
		mod_ratio = other.mod_ratio;
		diff_score = other.diff_score;
		ptms = other.ptms;
		basic_info = other.basic_info;
		return *this;
	}

	friend bool operator<(const PrSMFeature &stItem1, const PrSMFeature &stItem2);
	friend bool operator<=(const PrSMFeature &stItem1, const PrSMFeature &stItem2);
	friend bool operator>(const PrSMFeature &stItem1, const PrSMFeature &stItem2);
	friend bool operator>=(const PrSMFeature &stItem1, const PrSMFeature &stItem2);
	static bool cmpByOldScore(const PrSMFeature &stItem1, const PrSMFeature &stItem2);

};


class FeatureNormalization
{
private:
	std::vector<double> m_vFeatureMax;
	std::vector<double> m_vFeatureMin;
	std::vector<double> m_vFeatureSum;
	std::vector<double> m_vFeatureSumXX;
	std::size_t m_tSampleSize;

	void updateMaxMin(vector<double> &nums, int l, int r);
	void updateSum(vector<double> &nums, int l, int r);
	void updateSumXX(vector<double> &nums, int l, int r);

	void minmax_normalization(vector<double> &nums, int l, int r);
	void log_normalization(vector<double> &nums, int l, int r);
	void zscore_normalization(vector<double> &nums, int l, int r);
	void atan_normalization(vector<double> &nums, int l, int r);  // 反余切函数转换
public:
	FeatureNormalization();
	~FeatureNormalization();

	void Normalization(vector<double> &nums, int l, int r, NormalizationMethod type);
	void UpdateFeatureStatistics(vector<double> &nums, int l, int r, NormalizationMethod type);
	void Clear();
	
};

class Rerank
{
public:
	Rerank(const CConfiguration *cPara, const string &path);
	~Rerank();

	void run();
	void PrepareData(vector<vector<double> > &train, vector<vector<double> > &test);
	void SVMTraining();
	void SVMPredict(vector<double> &vPredictY);

	bool SamplePartition(vector<vector<double> > &train, vector<vector<double> > &test, vector<double> &Y);

	void OutputSamples();

private:
	//bool samplePartition(vector<vector<double> > &train, vector<vector<double> > &test);
	void updateFeature(vector<PrSMFeature> &prsmInfoCpy);
	void updateSVMScore(vector<double> &Y);
	void calFDR(vector<PrSMFeature> &prsmInfoCpy, bool first);
	void getFeature(PrSMFeature &prsm, vector<double> &featureVal);

	double getTolPpm(double precursor, double proMass);

	void readSearchResultFile();
	void writeResultFile();
	void parseResFile(FILE *pf);
	void parseResFile(const char *buf, int len);
	bool parseResLine(const char *line, PrSMFeature &prsm, const PRSM_Column_Index &cols);
	void RemoveTmpFiles();

	string m_strInputFile;

	string m_strTrainFile;													// SVM训练数据集文件名
	string m_strTestFile;													// SVM测试数据集文件名
	string m_strModelFile;													// SVM模型文件名
	string m_strOutFile;

	double m_lfMaxPrecurErr;
	double m_lfThreshold;
	int m_nFeature_Num;
	int m_nSample_Num;
	NormalizationMethod m_eNormal_type;

	vector<vector<PrSMFeature>> m_vPrSMInfo;  // 保存每张谱的结果
	//vector<vector<double> > m_vTrain;
	//vector<vector<double> > m_vTest;

	vector<double> m_vPrecursor_err_freq;
	unordered_map<string, double> m_umModPro;

	FeatureNormalization *m_cFeatureConverter;
	const CConfiguration *m_cPara;
	Clog *m_pTrace;

};



#endif