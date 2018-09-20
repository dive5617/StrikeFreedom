#ifndef _PFREADER_H_
#define _PFREADER_H_

#include <vector>
#include <iostream>
#include <fstream>
#include <string>
using namespace std;

struct PFHeadInfo
{
	int nScanCounts;
	int nTitleLength;
	int nOffSet;   // the total size of pf Head info
	string sTitle;
	PFHeadInfo():nScanCounts(0),nTitleLength(0),sTitle(""){}
};

class CPFReader
{
public:
	CPFReader(string filename);
	~CPFReader();

	void GetHeadInfo( PFHeadInfo & pfh);
	
	void WriteHeadToPF(FILE *fp, PFHeadInfo &pfh, int TotalScan);

	int ParseScanCounts(char *p);
	int ParseTitleLength(char *p);
	string ParseTitle(char *p, int len);
	
	void WriteSpectraIDXToPF(FILE * outPF,FILE *outPFIDX, int &HeadInfoOffSet, long &SpectraInfoOffSet);

	bool ValidPF();
private:
	void parse();
	void PrintHeadToPF(FILE *fp, PFHeadInfo &pfh, int TotalScan);
	void PrintSpectraToPF( FILE * pfile, FILE *pfileidx, int &HeadInfoOffSet, long &SpectraInfoOffSet);
	int ParseInt(char *p); // Extract to Common is more appropriate;

public:
	//vector<MS2Spectrum> MS2List;
	int ScanCounts;
	string MGFfilename;
	string SpectrumTitle;
	long PFlength;
	long PFIDXlength;
	char *buf;
	char *bufidx;
};
#endif



