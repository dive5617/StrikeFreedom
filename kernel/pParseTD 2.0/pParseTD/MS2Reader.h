// This file can load MS2 very fast. 5.765s can be more fast??
#ifndef MS2Reader_H_
#define MS2Reader_H_

#include <vector>
#include <iostream>
#include <fstream>
#include <string>
#include <iomanip>
#include <algorithm>
#include <cmath>
#include <iterator>  
#include <cctype>
#include <stdio.h>

#include "Parameters.h"
#include "BasicFunction.h"
#include "MS2Spectrum.h"

using namespace std;


class MS2Reader
{
public:
	MS2Reader(string infilename, vector<unordered_map<int,vector<CentroidPeaks>>>& vIPV, CEmass *cEmass);
	~MS2Reader();

	void LoadMS2();
	void parse(char *buf, int len = MAXBUF);
	void ResetFreq();
	
	
	unsigned int GetSpectraNum();//Remember the scanCounts is from 0,1,2,..., scanCounts
	int GetOutPutidentifier(int index);
	double GetActivationCenter(int i);
	void GetScan_MZ_Charge_By_Index(int index,int &Scan,int &charge, double &MZ, int &precusorScan);
	void GetActivationType_By_Index(int index,string &ActivationType);
	void GetPeakNumByIndex(int index, int &PeakNum);
	bool CheckOutputFlag(int file_idx);//�жϵ�ǰ�����������Ƿ���Ҫ�����������Ҫ����������true,����false��
	
	void outputFreq(string filename);// test
	
	vector<MS2Spectrum *> MS2List;
	bool OutputFlag[6];//{"_CIDIT.mgf","_ETDIT.mgf","_HCDIT.mgf","_CIDFT.mgf","_ETDFT.mgf","_HCDFT.mgf"} ��¼�������������Ƿ���ֹ����Ӷ������Ƿ���Ҫ����
	//double **m_vIPV;
	vector<unordered_map<int,vector<CentroidPeaks>>> &m_vIPV;
	CEmass *m_cEmass;


private:
	
	int ScanCounts;// this one is 0,1,2,...,ScanCounts
	int BinNum;//=2000;
	int *Freq;//[BinNum];
	string filename;
	
};

#endif

