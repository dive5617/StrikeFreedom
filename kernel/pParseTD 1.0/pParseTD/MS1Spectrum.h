#ifndef MS1Spectrum_H_
#define MS1Spectrum_H_
#include <vector>
#include <iostream>
#include <cmath>
#include <string>

#include "BasicFunction.h"

using namespace std;

class MS1Spectrum
{
public:

	MS1Spectrum(int CurrentMS1Scan);
	~MS1Spectrum();

	void SetRetentiontime(double dRetentiontime);
	void SetInstrumentType(string InstrumentType);	
	void AppendPeaks(double MH,double Intensity);

	int GetPeakNum();
	int GetCurrentMS1Scan();
	double GetBaseline();
	double GetRetiontime();
	void GetPeaksInWindow(double MZ, double Halfwindow, vector<CentroidPeaks> &PeakList);
	void GetPeaksbyMass(double MZ, int chg, vector<CentroidPeaks> &PeakList, double Peak_Tol_ppm);
	void FilterByBaseline();
	void PrintPeakToBinaryFile(FILE *pfile);
	//void PrintInfo();

	vector<CentroidPeaks> Peaks;  // 谱峰

private:

	
	int nCurrentMS1Scan;
	double Baseline;      // 噪声基线
	double RetentionTime; // 保留时间
	string SpectrumTitle;
	string m_InstrumentType; // 碎裂类型
	
	vector<double> LogInten;
	
};

#endif


