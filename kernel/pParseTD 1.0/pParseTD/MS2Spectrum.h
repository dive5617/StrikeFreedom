#ifndef _MS2Spectrum_
#define _MS2Spectrum_

#include <string>
#include <vector>
#include <cstdio>

#include "BasicFunction.h"
#include "emass.h"

using namespace std;

class MS2Spectrum
{
public:

	MS2Spectrum(int CurrentMS2Scan, double **vIPV, CEmass *cemass);
	~MS2Spectrum();
	void SetRetentiontime(double dRetentiontime);
	void SetActivationCenter(double dActiveCenter);
	void SetActivationType(string sActivationType);
	void SetInstrumentType(string sInstrementType);
	void SetPrecursorScan( int iPrecursorScan );
	void Setcharge(int dchg);
	void SetMZ(double lfMZ);
	void AppendPeaks(double mz, double Intensity);

	double GetActivationCenter( );
	string GetActivationType( );
	int GetOutPutFileIdentifier();
	int GetCurrentMS2Scan();
	int GetPrecursorScan();
	int GetPeakNum();
	int Getcharge();
	double GetMZ();
	
	//void FindLocalExtremum(vector<int> &posRes, int type);
	//void GetEachIsoPeak(int peakPos, int chg, int &isoPeakNums, double &stdPPMTol, double &sumPeakInens, vector<double> &isoPattern);
	//double ComputIPVSimilarity(double mass, int &highestPeakPos, double &sumPeakInt, vector<double> &isoPattern);
	//bool FindLocalMaxPeak(vector<int> &posRes);
	//void Deconvolution(int precurChg);

	double FilterByBaseline();
	double FilterByBaselineWhole(double SNratio);
	double GetAllPeakIntens(int peakPos, double Peak_Tol_ppm);
	double CalculateVectorCov(vector<double> &X, vector<double> &Y);
	
	void GetEachIsoPeak(int peakPos, int chg, int &isoPeakNums, double &stdPPMTol, double &sumPeakInens, 
		vector<double> &isoPattern, vector<int> &tmpIsoform, double Peak_Tol_ppm);
	double ComputIPVSimilarity(double mass, int &highestPeakPos, int &isoPeakNum, double &sumPeakInt, 
		vector<double> &isoPattern, vector<double> &thoIsoPattern, vector<int> &tmpIsoform);
	void Deconvolution(int precurChg, vector<double> &precurMass, double Peak_Tol_ppm, double SNratio);
	void RemovePrecursor(vector<double> &precurMass, double Peak_Tol_ppm);
	void PreProcess();

	vector<DeconvCentroidPeaks> Peaks;    // 谱峰信息

	// [test]
	/*vector<int> t_IsoPeakNum;  
	vector<int> t_charge;
	int t_pre_charge; */
	
private:
	
	bool m_bIsDeconved;
	int m_nCharge;         // 母离子电荷
	int nCurrentMS2Scan;   // scan号
	int nActivationType;   // 碎裂类型
	int nInstrumentType;   // 仪器类型 FTMS ITMS
	double MZ;             // 母离子单电荷mz
	double RetentionTime;  // 保留时间
	double ActiveCenter;   // 碎裂中心
	double m_lfBaseline;   // 噪音基线
	string ActivationType; 
	string InstrumentType;
	int PrecursorScan;    // 母离子Scan号

	double **m_vIPV;

	//vector<int> m_vIsRemoved;

	CEmass *m_cEmass;
};

#endif
