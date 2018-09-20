#ifndef MS1Reader_H_
#define MS1Reader_H_

#include <vector>
#include <string>
#include <iostream>
#include <map>
#include <fstream>
#include <math.h>
#include <unordered_set>
#include "Parameters.h"
#include "MS1Spectrum.h"
#include "TrainingFileReader.h"
#include "MS2Reader.h"
#include "CPeakMap.h"
#include "pParseDebug.h"
#include "BasicFunction.h"
#include "emass.h"

using namespace std;

const int FACTOR = 1000;  // 谱峰哈希因子

class CandidatePrecursorPeak{
public:
	int peakIdx;  // 最高峰在所有候选谱峰中的下标
	int charge;   // 电荷
	CandidatePrecursorPeak(int _peakIdx, int _chg){
		peakIdx = _peakIdx;
		charge = _chg;
	}
	bool operator==(const CandidatePrecursorPeak &x) const
	{
		return (peakIdx == x.peakIdx) && (charge == x.charge);
	}
	bool operator!=(const CandidatePrecursorPeak &x) const
	{
		return (peakIdx != x.peakIdx) || (charge != x.charge);
	}
	bool operator<(const CandidatePrecursorPeak &x) const{
		if(peakIdx == x.peakIdx){
			return charge < x.charge;
		}
		return peakIdx < x.peakIdx;
	}
};
namespace std {
    template <>
        class hash<CandidatePrecursorPeak>{
        public :
        size_t operator()(const CandidatePrecursorPeak &x ) const{
			return hash<int>()(x.charge)*13131 + hash<int>()(x.peakIdx);
        }
    };
}
struct PrecursorPeakIntensity{
	int peakIdx; // 谱峰在所有候选谱峰中的下标
	double m_lfIntens;
	PrecursorPeakIntensity(int _peakIdx, double _intensity){
		peakIdx = _peakIdx;
		m_lfIntens = _intensity;
	}
};


class MS1Reader
{
public:
	MS1Reader(string &infilename, CParaProcess *para, vector<unordered_map<int,vector<CentroidPeaks>>>& vIPV, CEmass *cEmass, double halfWnd);
	~MS1Reader();

	void Initialize(); /* Initialize Data Structure for  Mars Model: MarsFeature, MarsFeatureMEAN, MarsFeatureSTD, OutputInfo */
	void InitPeakMatrix(); //变量初始化重要
	void InitPeaksRecord();//初始化谱峰记录，用于清空变量，重要。
	void InitIsoPattern();
	void SetTrainingSet();
	void SetSpectrumTitle(string title);      /* Set Spectrum Title */

	void parse(char *buf,int len = MAXBUF);
	void FileterEachSpectrumByBaseLine();

	int GetSpectraNum();
	int GetIsotopeProfileLen(int &leftLen);
	double GetIsolationWidth(); /* Return IsolationWidth Parameter in double format */
	double GetIntensitySum();
	double GetSumIsoPattern(); /* Compute the intensity summation of the Isotope pattern, before Normalizing */ 
	double GetBaselineByIndex(int MS1Index);
	void GetIndex_By_MS2Scan(int &index,int &MS2scan);	
	void GetMS1_Peaks_In_Window_On_Given_Index(double MZ,double Halfwindow,int MS1Index,vector<CentroidPeaks> &PeakList);
	void GetCandidatesPrecursorByMS2scan(int MS2scan,int MS1scan,int userSpanStart,int userSpanEnd,double ActivationCenter,vector<CentroidPeaks> &PrecursorCandidatePeaks);

	void GetEachIsoPeak(vector<CentroidPeaks> &PrecursorCandidatePeaks, int chg, int k, int &matchedIsoPeak, 
			double &stdPPMTol, double &sumMatchedInt, double prePeakMz, bool &isIncluded);
	
	void GetAllIsoFeature(int chg,double ActivationCenter,vector<CentroidPeaks> &PrecursorCandidatePeaks,int k);
	void ScaledIsoPattern(double sumIsopattern); /* Normalize the Isotope patterns */
	void CalcChromSimilarity();//计算色谱相似度
	int CalcEluteSimilairty();//计算色谱相似度，新版本，考虑了局部最小切割
	void GetTheoryIsoform(double mass, int &highestPeakPos, vector<double> &vTheoIso);
	double ComputIPVSimilarity(double &SumCurPrecursorInt, vector<double> &vTheoIso, double &IsoLenScore);
	double GetPeakInIsolationWindow(vector<CentroidPeaks> &PrecursorCandidatePeaks,double &ActivationCenter,double &IsolationWidth);
	void CutIsotopicPeak(int &IsoLen, double &sumCurPrecursorInt);
	void CutSamePrecursor(vector<CentroidPeaks> &PrecursorCandidatePeaks);
	void CheckSameScore(vector<vector<double> > &PrecursorsCandidatesArray, int gSortByCol);
	void ScorePrecursors(vector<vector<double> > &PrecursorsCandidatesArray);
	void FindStartEndofEluteProfile(int &Start, int &End, const vector<int> &LocalMinimal, const int &BestIsoSimiScanIndex);

	void AssessPrecursor(double ActivationCenter, int MS2scan, int precursorScan, vector<CentroidPeaks> &PrecursorCandidatePeaks,TrainingSetreader &TrainSet);// Kernel函数专用，导出特征，并过滤。
	void AssessPrecursors(double ActivationCenter, int MS2scan, int precursorScan, vector<CentroidPeaks> &PrecursorCandidatePeaks,TrainingSetreader &TrainSet);// Kernel函数专用，导出特征，并过滤。

	double ReCalcMZ(double oldMZ);// 用于对MZ进行校正
	double CalcSTDforMZ(double oldMZ);//计算给定母离子的方差
	double IsoPatternColumnDotProduct(int col_x,int col_y);//同位素模式直接进行内积，
	
	//void SVMPredict();
	//void MarsPredict();
	void OutputMarsPredictY();//CHECK
	void MacLearnClassify(string modelType);
	                                                     
	void ReleaseTrainingSet();

	double CheckCloseChargeState(int scanNo, int precursorScan, int curChg, double curMz, vector<double> &vTheoIso);

	void GetMS1PeakbyMass(double accurateMz, int chg, int ms1Index, vector<CentroidPeaks> &vPeaksTmp);
	
private:

	void ReconstructIsotopePeakCluster(vector<CentroidPeaks> &PrecursorCandidatePeaks, vector<CandidatePrecursorPeak> &candidatePrecursorPeaks, const double baseline);
	void DetectIsotopePeakCluster(vector<CentroidPeaks> &PrecursorCandidatePeaks, vector<CandidatePrecursorPeak> &candidatePrecursorPeaks, const double baseline);
	int BinarySearchPeak(vector<CentroidPeaks> &allPeaks, double targetMz, double peakTol);
	int SearchPeak(vector<CentroidPeaks> &allPeaks, vector<int> &peakHash, double targetMz, double peakTol);

	int MS1Scancounts;// MS1的谱图个数
	int BestIsoSimiScanIndex;
	double m_lfHalfWnd;
	vector<double> ChromSimilarity;//色谱相似度,【0】最高峰与之前的等间隔谱峰的相似度,[1]最高峰与之后的等间隔谱峰的相似度
	vector<double> IsoPattern;
	CPeakMap m_PeakMap;  // 谱峰关联矩阵
	CEmass *m_cEmass;

public:
	vector<int> m_vnIndexListToOutput;   //对于不同的碎裂模式，考虑不同的输出，按照碎裂类型存储所有二级谱的下标值
	vector<double> m_vPredictY;
	vector<vector<double> > MarsFeature;
	vector<vector<double> > OutputInfo;  //1:MS2Scan, 2:chg, 3:AccMZ
	vector<MS1Spectrum *> MS1List;//一级谱信息全体
	vector<CentroidPeaks> PeakRecord;//谱峰记录器
	string SpectrumTitle;
	string m_InstrumentType;

	TrainingSetreader *pTrainSet;
	CParaProcess *m_para;// 参数类的对象
	//double **m_vIPV;
	vector<unordered_map<int,vector<CentroidPeaks>>>  &m_vIPV;
	double Peak_Tol_ppm;

	vector<int> MS1Scan_Index;  // 一级谱的Scan号到其下标的映射
};
#endif
