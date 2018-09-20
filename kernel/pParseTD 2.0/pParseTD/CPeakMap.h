#ifndef _CPEAKMAP_H_
#define _CPEADMAP_H_
#include <iostream>
#include <vector>
using namespace std;

class CPeakMap
{
public:
	CPeakMap(int BinsPerTh, int Span, double MaximalHalfWidth);
	~CPeakMap();
	void Initialize();
	void Reset();
	void IntensityMap(double mz, double intensity, double ac, int scanidx);
	double GetPeak(int scanidx, int binidx);
	void Print();
	
private:
	double m_halfwindow;
	int m_BinsPerTh;  // 每个Th的bin数目
	int m_r;
	int m_c;
	int m_span;
	vector<vector<double> > m_PeakMatrix;
};

#endif

