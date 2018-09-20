#ifndef MS1Reader_H_
#define MS1Reader_H_

#include <vector>
#include <string>
#include <iostream>
#include <map>
#include <fstream>

#include "Parameters.h"
#include "MS1Spectrum.h"
#include "TrainingFileReader.h"
#include "MS2Reader.h"
#include "CPeakMap.h"
#include "pParseDebug.h"
#include "BasicFunction.h"
#include "emass.h"
#include "Classification.h"
using namespace std;

class MS1Reader
{
public:
	MS1Reader(string &infilename, CParaProcess *para, double **vIPV, CEmass *cEmass, double halfWnd);
	~MS1Reader();

	void Initialize(); /* Initialize Data Structure for  Mars Model: MarsFeature, MarsFeatureMEAN, MarsFeatureSTD, OutputInfo */
	void InitPeakMatrix(); //������ʼ����Ҫ
	void InitPeaksRecord();//��ʼ���׷��¼��������ձ�������Ҫ��
	void InitIsoPattern();
	void SetTrainingSet();
	void SetSpectrumTitle(string title);      /* Set Spectrum Title */
	void SetClassifier(string path);    // set classifier

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
	void CalcChromSimilarity();//����ɫ�����ƶ�
	int CalcEluteSimilairty();//����ɫ�����ƶȣ��°汾�������˾ֲ���С�и�
	void GetTheoryIsoform(double mass, int &highestPeakPos, vector<double> &vTheoIso);
	double ComputIPVSimilarity(double &SumCurPrecursorInt, vector<double> &vTheoIso, double &IsoLenScore);
	double GetPeakInIsolationWindow(vector<CentroidPeaks> &PrecursorCandidatePeaks,double &ActivationCenter,double &IsolationWidth);
	void CutIsotopicPeak(int &IsoLen, double &sumCurPrecursorInt);
	void CutSamePrecursor(vector<CentroidPeaks> &PrecursorCandidatePeaks);
	void CheckSameScore(vector<vector<double> > &PrecursorsCandidatesArray, int gSortByCol);
	void ScorePrecursors(vector<vector<double> > &PrecursorsCandidatesArray);
	void FindStartEndofEluteProfile(int &Start, int &End, const vector<int> &LocalMinimal, const int &BestIsoSimiScanIndex);

	void AssessPrecursor(double ActivationCenter, int MS2scan, int precursorScan, vector<CentroidPeaks> &PrecursorCandidatePeaks,TrainingSetreader &TrainSet);// Kernel����ר�ã����������������ˡ�
	
	double ReCalcMZ(double oldMZ);// ���ڶ�MZ����У��
	double CalcSTDforMZ(double oldMZ);//�������ĸ���ӵķ���
	double IsoPatternColumnDotProduct(int col_x,int col_y);//ͬλ��ģʽֱ�ӽ����ڻ���
	
	//void SVMPredict();
	//void MarsPredict();
	void OutputMarsPredictY();//CHECK
	void MacLearnClassify(string modelType, bool newModel);
	                                                     
	void ReleaseTrainingSet();

	double CheckCloseChargeState(int scanNo, int precursorScan, int curChg, double curMz, vector<double> &vTheoIso);
	void GetMS1PeakbyMass(double accurateMz, int chg, int ms1Index, vector<CentroidPeaks> &vPeaksTmp);
	
private:

	int MS1Scancounts;// MS1����ͼ����
	int BestIsoSimiScanIndex;
	double m_lfHalfWnd;
	vector<double> ChromSimilarity;//ɫ�����ƶ�,��0����߷���֮ǰ�ĵȼ���׷�����ƶ�,[1]��߷���֮��ĵȼ���׷�����ƶ�
	vector<double> IsoPattern;
	CPeakMap m_PeakMap;  // �׷��������
	CEmass *m_cEmass;
	CClassify *classify;
public:
	vector<int> m_vnIndexListToOutput;   //���ڲ�ͬ������ģʽ�����ǲ�ͬ������������������ʹ洢���ж����׵��±�ֵ
	vector<double> m_vPredictY;
	vector<vector<double> > MarsFeature;
	vector<vector<double> > OutputInfo;  //1:MS2Scan, 2:chg, 3:AccMZ
	vector<MS1Spectrum *> MS1List;//һ������Ϣȫ��
	vector<CentroidPeaks> PeakRecord;//�׷��¼��
	string SpectrumTitle;
	string m_InstrumentType;

	TrainingSetreader *pTrainSet;
	CParaProcess *m_para;// ������Ķ���
	double **m_vIPV;
	double Peak_Tol_ppm;

	vector<int> MS1Scan_Index;  // һ���׵�Scan�ŵ����±��ӳ��
};
#endif
