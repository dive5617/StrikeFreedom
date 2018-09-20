#ifndef SPECDATAIO_H_
#define SPECDATAIO_H_

#include <iostream>
#include <fstream>
#include <string>

#include "MS1Reader.h"
#include "MS2Reader.h"

using namespace std;

class CspecDataIO
{
public:
	CspecDataIO();
	~CspecDataIO();
	int OutputToMGF(MS1Reader &MS1, MS2Reader &MS2, string outfileName);
	int OutputToTXT(MS1Reader &MS1, MS2Reader &MS2, string outfileName);
	int OutputToPF(MS1Reader &MS1, MS2Reader &MS2, string outfileName);
	bool OutputMS1ToPF(MS1Reader &MS1, string outfileName);
	void OutputTrainingSample(MS1Reader &MS1, FILE *pfilePos, FILE *pfileNeg);
	void OutputPrecursorToCSV(MS1Reader &MS1, MS2Reader &MS2, 
			string &strPrecursorsListToCSV, int &file_idx);// 输出母离子信息到CSV文件中
	

	void MS1Parse(string infilename, MS1Reader *MS1);
	void MS2Parse(string infilename, MS2Reader *MS2);
};

#endif