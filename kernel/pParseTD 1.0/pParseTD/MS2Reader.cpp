/********************************************************************
	created:	2012/04/11
	author:		Wu Long
	
	purpose:	
*********************************************************************/

#include <vector>
#include <iostream>
#include <fstream>
#include <string>
#include <iomanip>
#include <algorithm>
#include <algorithm> 
#include <cmath>
#include <iterator>  
#include <cctype> // isdigit()
#include <cstdio>

#include "MS2Reader.h"
#include "BasicFunction.h"
#include "MS2Spectrum.h"
#include "SpecDataIO.h"
#include "com.h"

using namespace std;

MS2Reader::MS2Reader(string infilename, double **vIPV, CEmass *cEmass)
{
	filename = infilename;
	ScanCounts = -1;
	BinNum = 2000;
	Freq = new int[BinNum];

	m_vIPV = vIPV;
	m_cEmass =  cEmass;
}
MS2Reader::~MS2Reader()
{
	delete [] Freq;
	for(size_t i = 0; i < MS2List.size(); ++i)
	{
		delete MS2List[i];
	}
}

void MS2Reader::LoadMS2()
{
	for (int i = 0; i < OutPutFileTypeNum; i++)
	{
		OutputFlag[i] = false;
	}
	ResetFreq();

	cout << "[pParseTD] Loading MS2..." <<endl;
	/*long filelen = CTools::OutputFilesize(filename.c_str());
	char *buf = new char[filelen + 1];
	FILE *pfile = fopen(filename.c_str(),"rb");
	int len = fread(buf, 1, filelen, pfile);
	fclose(pfile);
	buf[len] = '\0';
	parse(buf,len);
	delete [] buf;*/
	try{
		CspecDataIO *io = new CspecDataIO;
		io->MS2Parse(filename, this);
		delete io;
	}catch(...){
		cout<<"MS2 parse error!"<<endl;
	}
	cout << "[pParseTD] MS/MS scan number: " << MS2List.size() <<endl;
}

int MS2Reader::MS2Split(string ms2filename, vector<string> &MS2SplitList)
{
	//[TODO:] put this in the Parameter
	const int ScanPerMS2 = 5000; //12000;
	vector<bool> vis(MAXSCANNUM,false);
	//Clog * plog = Clog::GetClog();
	char line[MaxCharInLine];
	string Info = "";

	srand(1);   // seed

	int fileID = 0, ScanCount = 0, ScanNum = 0;
	string outputMS2 = ms2filename.substr(0, ms2filename.length()-4) + "_ID_" + CTools::int2string(fileID) +".ms2";
	MS2SplitList.push_back(outputMS2);
	FILE * pfile = fopen(ms2filename.c_str(), "r");
	FILE * qfile = fopen(outputMS2.c_str(), "w");
	bool flag = false;
	char ch;
	int scan;
	while(!feof(pfile)){
		fgets(line,MaxCharInLine,pfile);
		if(line[0] == 'S'){
			++ ScanNum;
			if(rand()%100 < 30 && ScanCount*1.0/ScanNum > 0.60){
				flag = false;
				continue;
			}
			if(ScanCount == ScanPerMS2)  break;
			flag = true;
			sscanf(line, "%c %d %*d", &ch, &scan);
			vis[scan] = true;
			++ ScanCount;
		}
		if(flag)
			fprintf(qfile, "%s", line);
	}
	fclose(qfile);
	//cout << "Scan Number: " << ScanNum << ", Extract Number: " << ScanCount << endl;
	
	++ fileID;
	outputMS2 = ms2filename.substr(0, ms2filename.length()-4) + "_ID_" + CTools::int2string(fileID) +".ms2";
	MS2SplitList.push_back(outputMS2);
	qfile = fopen(outputMS2.c_str(), "w");
	flag = false;
	ScanCount = 0;
	fseek(pfile,0L, SEEK_SET);
	while(!feof(pfile))
	{
		fgets(line,MaxCharInLine,pfile);
		if (line[0] == 'S')
		{
			sscanf(line, "%c %d %*d", &ch, &scan);
			if(vis[scan]){
				flag = false;
				continue;
			}
			flag = true;
			if (ScanCount % ScanPerMS2 == 0 && ScanCount > 0)
			{
				//Info = "File "+int2string(fileID)+": "+outputMS2+" complete!" ;
				//plog->printInfoTo(cout, "Info", Info);
				
				fclose(qfile);
				++ fileID;			
				outputMS2 = ms2filename.substr(0, ms2filename.length()-4) + "_ID_" + CTools::int2string(fileID) +".ms2";
				MS2SplitList.push_back(outputMS2);
				qfile = fopen(outputMS2.c_str(), "w");

			}
			ScanCount ++;
		}
		if(flag)
			fprintf(qfile, "%s", line);
	}
	fclose(pfile);
	fclose(qfile);
	//Info = "File "+int2string(fileID)+": "+outputMS2+" complete!" ;
	//plog->printInfoTo(cout, "Info", Info);
	
	//cout <<"Output complete!" <<endl;
	return fileID+1;	
}


void MS2Reader::ResetFreq()
{
	for (int i = 0; i < BinNum; i++)
	{
		Freq[i] = 0;
	}
}

void MS2Reader::parse(char *buf, int len)
{
	std::ios::sync_with_stdio(false);
	for (const char *p = buf; *p && p - buf < len; ++p)
	{
		if (*p >= 'A' && *p <= 'Z')             /* Head Information */
		{
			if (*p == 'S')						/* Read scan */
			{
				int scan = 0;
				p = p + 2;
				while(*p != '\t')
				{
					scan = scan * 10 + (*p - '0');
					++p;
				}
				MS2Spectrum *tmpMS2 = new MS2Spectrum(scan, m_vIPV, m_cEmass);
				MS2List.push_back(tmpMS2);
				++ScanCounts;
				while(*p != '\n') ++p;
			} else if (*p == 'Z') {				/* Read mz and charge state */
				int chg = 0;
				p = p + 2;
				while(*p != '\t')
				{
					chg = chg * 10 + (*p - '0');
					++p;
				}
				if (chg == 0) chg = 2;
				MS2List[ScanCounts]->Setcharge(chg);

				++p;
				double mz = 0;
				int IndexOfdot = 1;
				while(*p != '\r')
				{
					if (*p == '.')
					{
						--IndexOfdot;
						++p;
						continue;
					}
					if(IndexOfdot != 1) --IndexOfdot;
					mz = mz * 10 + (*p - '0');
					++p;
				}
				++p;
				mz = mz * pow(10.0, IndexOfdot);
				if (mz < 1e-5 && mz > -1e-5) //????????????????????
				{
					mz = MS2List[ScanCounts]->GetActivationCenter() * chg - (chg - 1) * pmass;

				}
				MS2List[ScanCounts]->SetMZ(mz);
				while(*p != '\n') ++p;
			} else if (*p == 'I' && *(p+1) == '\t' && *(p+2) == 'R') {     /* Read retention time */
				double rettime = 0;
				p = p + 10;
				int IndexOfdot = 1;
				while(*p != '\r')
				{
					if (*p == '.')
					{
						IndexOfdot--;
						p++;
						continue;
					}
					if(IndexOfdot!=1) IndexOfdot--;
					rettime = rettime * 10 + (*p - '0');
					p++;
				}
				p++;
				rettime = rettime * pow(10.0, IndexOfdot);
				MS2List[ScanCounts]->SetRetentiontime(rettime);
				while(*p!='\n') ++p;
			} else if (*p == 'I' && *(p+1) == '\t' && *(p+2) == 'A' && *(p+12) == 'C'){    /* Read ActivateCenter */
				double ActiveCenter = 0;
				p = p + 19;
				int IndexOfdot = 1;
				while(*p != '\r')
				{
					if (*p == '.')
					{
						IndexOfdot--;
						p++;
						continue;
					}
					if(IndexOfdot!=1)	IndexOfdot--;
					ActiveCenter = ActiveCenter * 10 + (*p - '0');
					p++;
				}
				p++;
				ActiveCenter=ActiveCenter * pow(10.0, IndexOfdot);
				MS2List[ScanCounts]->SetActivationCenter(ActiveCenter);           //.SetRetentiontime(rettime);
				while(*p != '\n') p++;
			} else if (*p == 'I' && *(p+1) == '\t' && *(p+2) == 'A' && *(p+12) == 'T') {     /* Read ActivateType */
			
				string ActiveType = "";
				ActiveType = ActiveType + *(p+17) + *(p+18) + *(p+19);
				MS2List[ScanCounts]->SetActivationType(ActiveType);
			} else if (*p == 'I' && *(p+1) == '\t' && *(p+2) == 'I' && *(p+12) == 'T') {    /* Read InstrumentType */
				string InstrumentType = "";
				if (*(p+17) == 'F' || *(p+17) == 'Q')
				{
					InstrumentType = InstrumentType + "FT";
				} else {
					InstrumentType = InstrumentType + "IT";
				}
				MS2List[ScanCounts]->SetInstrumentType(InstrumentType);
				OutputFlag[MS2List[ScanCounts]->GetOutPutFileIdentifier()] = true;
			} else if(*p == 'I' && *(p+1) == '\t' && *(p+2) == 'P' && *(p+12) == 'c'){       /* Read PrecursorScan */
				int PrecursorScan = 0;
				p = p + 16;
				while(*p != '\r' && *p != '\n')
				{
					PrecursorScan = PrecursorScan * 10 + (*p - '0');
					p++;
				}
				if(*p == '\r') p++;
				MS2List[ScanCounts]->SetPrecursorScan(PrecursorScan); 
				while(*p != '\n') p++;
			}else {             /* Go to the end of line,Ignore other information */
				while(*p != '\n') p++;
			}
		}
		if (*p >= '0' && *p <= '9')                                                    /* Read peaks */
		{
			double mz = 0;
			int IndexOfdot = 1;
			while(*p != ' ' && *p != '\t')
			{
				if (*p == '.')
				{
					IndexOfdot--;
					p++;
					continue;
				}
				if(IndexOfdot != 1) IndexOfdot--;
				mz = mz * 10 + (*p - '0');
				p++;
			}
			p++;
			mz=mz * pow(10.0, IndexOfdot);
			//These code being replaced because its bad behavior.
			double intensity = 0;
			IndexOfdot = 1;
			while(*p != '\r'&& *p != '\n' && *p != ' ' && *p != '\t')
			{
				if (*p == '.')
				{
					IndexOfdot--;
					p++;
					continue;
				}
				if(IndexOfdot!=1)	IndexOfdot--;
				intensity = intensity * 10 + (*p-'0');
				p++;
			}
			p++;
			intensity = intensity * pow(10.0, IndexOfdot);
			MS2List[ScanCounts]->AppendPeaks(mz, intensity);
		}
	}
}

unsigned int MS2Reader::GetSpectraNum()
{
	return MS2List.size();
}

int MS2Reader::GetOutPutidentifier( int index )
{
	return MS2List[index]->GetOutPutFileIdentifier();
}

double  MS2Reader::GetActivationCenter(int i)
{
	return MS2List[i]->GetActivationCenter();
}

void MS2Reader::GetScan_MZ_Charge_By_Index(int index, int &Scan, int &charge, double &MZ, int &precursorScan)
{
	Scan = MS2List[index]->GetCurrentMS2Scan();
	charge = MS2List[index]->Getcharge();
	MZ = MS2List[index]->GetMZ();
	precursorScan = MS2List[index]->GetPrecursorScan();
}

void MS2Reader::GetActivationType_By_Index(int index, string &ActivationType)
{
	ActivationType = MS2List[index]->GetActivationType();
}

void MS2Reader::GetPeakNumByIndex(int index, int &PeakNum)
{
	PeakNum = MS2List[index]->GetPeakNum();
}

bool MS2Reader::CheckOutputFlag( int file_idx )
{
	return OutputFlag[file_idx];
}

void MS2Reader::outputFreq(string filename)
{
	ofstream fout;
	fout.open(filename.c_str(),ios::out);
	for (int i=0;i<BinNum;i++)
	{
		fout <<Freq[i]<<'\t';
	}
	//fout <<'\r'<<endl;
	fout.close();
}