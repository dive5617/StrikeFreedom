#ifndef EMASS_H_
#define EMASS_H_

#include <iostream>
#include <string>
#include <vector>
#include <math.h>
#include "BasicFunction.h"

using namespace std;

const int m_nElemNum = 5;
const double cutoff_eps = 1e-10; 

class CEmass{  // 用于在线计算理论同位素分布

public:

	CEmass(double **vIPV);
	~CEmass();
	void Init();
	void Coumpound(const int num, const vector<CentroidPeaks> &com, vector<CentroidPeaks> &res);
	void Convolution(const vector<CentroidPeaks> &comA, const vector<CentroidPeaks> &comB, vector<CentroidPeaks> &res);
	void Prunnig(vector<CentroidPeaks> &com, double limit);
	void Calculate(double mass, vector<CentroidPeaks> &res);
	void GetFormbyAvegine(double mass, vector<int> &pattern);

private:
	int m_nBase;
	double m_lfAvgMass;  // 平均氨基酸质量
	double **m_vIPV;
	vector<double> m_vMass;  // 存储C\H\N\O\S的Mono质量
	vector<double> m_vAveraine;
	vector<vector<CentroidPeaks> > m_vBaseIsotope;
	vector<vector<CentroidPeaks> > m_vIsotope;  // 存储C、H、N、O、S的同位素质量和丰度
	
};
#endif