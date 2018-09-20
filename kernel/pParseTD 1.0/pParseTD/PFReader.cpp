#include "PFReader.h"
#include "BasicFunction.h"
#include "Com.h"
#include "cstring"
#include "string"

using namespace std;

CPFReader::CPFReader(string filename)
{
	//Clog * plog = Clog::GetClog();
	buf = NULL;
	bufidx = NULL;
	ScanCounts = 0;
	//plog->printInfoTo(cout,"Info","Loading PF file " + filename);  // PF idx file        
	PFlength = CTools::OutputFilesize(filename.c_str());                     // Filesize
	buf = new char[PFlength+1];                                 // Get buffer PF 
	FILE * pfile = fopen(filename.c_str(),"rb");                      
	if(NULL == pfile){
		throw "Fail to Open PF file";
		//plog->printInfoTo(cout,"Error","Fail to open " + filename);
	}
	int len = fread(buf, sizeof(char), PFlength, pfile);               // Read data
	fclose(pfile);                                                   // close file PF

	string idxfile = filename+"idx"; 
	//plog->printInfoTo(cout,"Info","Loading PFIDX file " + idxfile);  // PF idx file                    

	PFIDXlength = CTools::OutputFilesize(idxfile.c_str());
	bufidx = new char[PFIDXlength+1];                              // Get Buffer idx
	pfile = fopen(idxfile.c_str(),"rb");
	if(NULL == pfile){
		//plog->printInfoTo(cout,"Info","Fail to open " + filename);
		throw "Fail to Open PF file";
	}
	len = fread(bufidx, sizeof(char), PFIDXlength, pfile);                   // Read Data idx
	fclose(pfile);                                                   // close file IDX

	char * q = buf;
	ScanCounts = ParseScanCounts(q);
}

void CPFReader::WriteSpectraIDXToPF(FILE * outPF, FILE * outPFIDX, int &HeadInfoOffSet, long &SpectraInfoOffSet)
{
	PrintSpectraToPF(outPF,outPFIDX,HeadInfoOffSet,SpectraInfoOffSet);
}

void CPFReader::WriteHeadToPF(FILE *fp, PFHeadInfo &pfh, int TotalScan)
{
	PrintHeadToPF(fp, pfh, TotalScan);
}

CPFReader::~CPFReader()
{
	delete []buf;                                                   // Release Buffer
	delete []bufidx;
}

void CPFReader::PrintSpectraToPF(FILE * pfile, FILE * pfileidx, int &HeadInfoOffSet, long &SpectraInfoOffSet)
{
	char * p = buf + HeadInfoOffSet;
	fwrite(p,sizeof(char),PFlength-HeadInfoOffSet,pfile);

	p = bufidx;

	for (int i = 0; i < ScanCounts; i ++)
	{
		int Scan = 0, nOffSet = 0;
		Scan = ParseInt(p);
		p += sizeof(Scan);
		nOffSet = ParseInt(p);
		p += sizeof(nOffSet);

		nOffSet = nOffSet - HeadInfoOffSet + SpectraInfoOffSet ;

		fwrite(&Scan,sizeof(int),1,pfileidx);
		fwrite(&nOffSet, sizeof(int),1,pfileidx);
	}
	SpectraInfoOffSet += (PFlength - HeadInfoOffSet);
}



void CPFReader::GetHeadInfo(PFHeadInfo & pfh)
{
	char * p = buf;
	pfh.nScanCounts = ScanCounts;
	p += sizeof(ScanCounts);

	pfh.nTitleLength = ParseTitleLength(p);
	p += sizeof(pfh.nTitleLength);

	pfh.sTitle = ParseTitle(p, pfh.nTitleLength);
	p += pfh.nTitleLength;

	pfh.nOffSet = sizeof(pfh.nScanCounts) + sizeof(pfh.nTitleLength) + pfh.nTitleLength;
}

void CPFReader::PrintHeadToPF(FILE *fp, PFHeadInfo &pfh, int TotalScan)
{	
	fwrite(&TotalScan,sizeof(int),1,fp);
	fwrite(&pfh.nTitleLength,sizeof(int),1,fp);
	fwrite(pfh.sTitle.c_str(),sizeof(char),pfh.nTitleLength,fp);
	cout << "Title: " << pfh.sTitle << endl;

}

string CPFReader::ParseTitle(char *p, int len)
{
	char title[1024];
	strncpy(title,p,len);
	string str(title);// title;
	return str;
}
int CPFReader::ParseInt(char *p)            // Extract to Common is more appropriate
{
	if(p == NULL)
		cout << "Error" << endl;
	int ret = 0;
	try{
		ret = *((int *)p);
	}
	catch(...)
	{
		cout << "Error" << p << endl;
	}
	
	return ret;
}

int CPFReader::ParseTitleLength(char *p)
{
	return ParseInt(p);
}

int CPFReader::ParseScanCounts(char *p)
{
	return ParseInt(p);
}

bool CPFReader::ValidPF()
{
	char *p = bufidx;
	//cout << "ScanCouts:" << ScanCounts << endl; 

	for (int i = 0; i < ScanCounts; i ++)
	{
		int ScanNum = ParseInt(p);
		p += sizeof(ScanNum);

		int OffSet = ParseInt(p);
		p += sizeof(OffSet);

		int ScanFound = ParseInt(buf+OffSet);
		if (ScanNum != ScanFound)
		{
			cout <<"Unmatched Scan: "<<  ScanFound << " != " << ScanNum << endl;
			return false;
		}
	}
	return true;
}
