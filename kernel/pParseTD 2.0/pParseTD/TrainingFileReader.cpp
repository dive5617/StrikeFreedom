
#include <string>
#include <iostream>

#include "com.h"
#include "Parameters.h"
#include "TrainingFileReader.h"

using namespace std;

TrainingSetreader::TrainingSetreader(string Filename, double mz_error_tol) : m_sTrainSetfile(Filename), Index(0), m_mzerror_tol(mz_error_tol)
{
	
}

TrainingSetreader::~TrainingSetreader()
{
	//cout << "Destruction Training Set" <<endl;
}

void TrainingSetreader::InitTrainData()
{
	unsigned filelen = CTools::OutputFilesize(m_sTrainSetfile.c_str());
	FILE *pfile = fopen(m_sTrainSetfile.c_str(),"rb");
	if(pfile == NULL)
	{
		cout << "Can not open training set file: " << m_sTrainSetfile <<endl;
		exit(0);
	}
	char *buf = new char[filelen+1];
	fread(buf, 1, filelen, pfile);
	fclose(pfile);
	buf[filelen] = '\0';
	parse(buf, filelen);
	delete [] buf;
}

void TrainingSetreader::parse( char * buf, int len )
{
	for (const char *p=buf; *p && p-buf<len; p++)
	{
		if (*p>='0' ||*p<='9')
		{
			int scan=0;
			//printf("old scan=%c\n",*p);
			sscanf(p,"%d",&scan);//=atoi(p);
			while (*p!='\t')
			{
				p++;
			}
			p++;
			//printf("old scan=%d\n",scan);
			double MZ;
			sscanf(p,"%lf",&MZ);//=atof(p);
			while (*p!='\t')
			{
				p++;
			}
			p++;
			int chg;
			sscanf(p,"%d",&chg);//=atoi(p);
			while (*p!='\t')
			{
				p++;
			}
			p++;
			double evalue;
			sscanf(p,"%lf",&evalue);//=atof(p);
			while (*p!='\n')
			{
				p++;
			}
			//p++;
			MS2ScanNoList.push_back(scan);
			MZList.push_back(MZ);
			chargeList.push_back(chg);
			evalueList.push_back(evalue);

			// Make sure all the other vectors is initialized here.
			m_causes_abandon_by_mars.push_back(1);
			m_causes_not_in_mono_list.push_back(1);
			m_causes_not_in_ranking_topn.push_back(1);

			CheckFlag.push_back(0);
			TopThreeCheckflag.push_back(0);
			
		}
	}
}

bool TrainingSetreader::CheckScanMZCharge( int &scan,double &MZ,int charge )
{
	
	bool ret=false;
	for (unsigned int i=0; i<MS2ScanNoList.size() && ret==false; ++i)
	{
		// The precursor must in 20 ppm window of the original precursors.
		// ToDo: 
		// The match error should be record in some list called match_info
		// Then we will know how accuracy the pParse+ could achieve. 
		if (MS2ScanNoList[i]==scan && (MZ-MZList[i]<MZList[i]*m_mzerror_tol) && (MZ-MZList[i]>-MZList[i]*m_mzerror_tol) && charge==chargeList[i])
		{
			CheckFlag[i]=1;
			ret=true;
			break;
		}
		// As the MS2 scan list is not sorted in ahead, we have to check all the items in the list.
		// This could be improved by sort the list, but I have not enough time for this.
		// If anyone sort the list, take care on the MS2 scan: it is duplicated when you use pParse+ (brute force to generate the training set).
	}
	
	return ret;

}
void TrainingSetreader::OutPutUnchecked()
{
	for (unsigned int i=0;i< CheckFlag.size();i++)
	{
		if (CheckFlag[i]==0)
		{
			cout << "Unchecked precursor" << tab << MS2ScanNoList[i] << tab << chargeList[i] << tab << MZList[i] <<tab << evalueList[i]<< endl;
		}
	}

}

void TrainingSetreader::OutPutUncheckedIntoFile(string strfilename)
{
	FILE * pfile = fopen(strfilename.c_str(),"w");
	for (unsigned int i=0;i< CheckFlag.size();i++)
	{
		if (CheckFlag[i]==0)
		{
			fprintf(pfile,"%d,%.5lf,%d,%e\n",MS2ScanNoList[i],MZList[i],chargeList[i],evalueList[i]);
				//cout << MS2ScanNoList[i] << tab << MZList[i] << tab << chargeList[i] <<tab << evalueList[i]<< endl;
		}
	}
	fclose(pfile);
}


int TrainingSetreader::sumCheckFlag()
{
	int ret = 0;
	for (unsigned int i = 0; i < CheckFlag.size(); i++)
	{
		ret += CheckFlag[i];
	}
	return ret;
}

void TrainingSetreader::ResetCheckFlag()
{
	for (int i = 0; i < (int)CheckFlag.size(); i++)
	{
		CheckFlag[i]=0;
	}
}

int TrainingSetreader::sumCheckTopThreeFlag()
{
	int ret=0;
	for (unsigned int i=0;i< TopThreeCheckflag.size();i++)
	{
		ret+=TopThreeCheckflag[i];
	}
	return ret;
}
bool TrainingSetreader::CheckTopThreeScanMZCharge( int &scan,double &MZ,int charge )
{
	bool ret=false;
	for (int i=Index;i<(int)MS2ScanNoList.size();i++)
	{
		if (MS2ScanNoList[i]==scan && MZ-MZList[i]<MZList[i]*2*m_mzerror_tol && MZ-MZList[i]>-MZList[i]*2*m_mzerror_tol && charge==chargeList[i])
		{
			TopThreeCheckflag[i]=1;
			ret=true;
		}
	}
	return ret;
}

int TrainingSetreader::TotalPrecursors()
{
	return CheckFlag.size();
}

bool TrainingSetreader::CheckMonoList( int &scan,double &MZ,int charge )
{
	bool ret=false;
	for (unsigned int i=0;i<MS2ScanNoList.size() && ret==false;i++)
	{
		// The precursor must in 20 ppm window of the original precursors.
		// ToDo: 
		// The match error should be record in some list called match_info
		// Then we will know how accuracy the pParseTD could achieve. 
		if (MS2ScanNoList[i]==scan && (MZ-MZList[i]<MZList[i]*m_mzerror_tol) && (MZ-MZList[i]>-MZList[i]*m_mzerror_tol) && charge==chargeList[i])
		{
			m_causes_not_in_mono_list[i]=0;
			ret=true;
			break;
		}
		// As the MS2 scan list is not sorted in ahead, we have to check all the items in the list.
		// This could be improved by sort the list, but I have not enough time for this.
		// If anyone sort the list, take care on the MS2 scan: it is duplicated when you use pParse+ (brute force to generate the training set).
	}

	return ret;

}

bool TrainingSetreader::CheckTopNList( int &scan,double &MZ,int charge )
{
	bool ret=false;
	for (unsigned int i=0;i<MS2ScanNoList.size() && ret==false;i++)
	{
		// The precursor must in 20 ppm window of the original precursors.
		// ToDo: 
		// The match error should be record in some list called match_info
		// Then we will know how accuracy the pParse+ could achieve. 
		if (MS2ScanNoList[i]==scan && (MZ-MZList[i]<MZList[i]*m_mzerror_tol) && (MZ-MZList[i]>-MZList[i]*m_mzerror_tol) && charge==chargeList[i])
		{
			m_causes_not_in_ranking_topn[i]=0;
			ret=true;
			break;
		}
		// As the MS2 scan list is not sorted in ahead, we have to check all the items in the list.
		// This could be improved by sort the list, but I have not enough time for this.
		// If anyone sort the list, take care on the MS2 scan: it is duplicated when you use pParse+ (brute force to generate the training set).
	}

	return ret;
}

bool TrainingSetreader::CheckMARSList( int &scan,double &MZ,int charge )
{
	bool ret=false;
	for (unsigned int i=0;i<MS2ScanNoList.size() && ret==false;i++)
	{
		// The precursor must in 20 ppm window of the original precursors.
		// ToDo: 
		// The match error should be record in some list called match_info
		// Then we will know how accuracy the pParse+ could achieve. 
		if (MS2ScanNoList[i]==scan && (MZ-MZList[i]<MZList[i]*m_mzerror_tol) && (MZ-MZList[i]>-MZList[i]*m_mzerror_tol) && charge==chargeList[i])
		{
			m_causes_abandon_by_mars[i]=0;
			ret=true;
			break;
		}
		// As the MS2 scan list is not sorted in ahead, we have to check all the items in the list.
		// This could be improved by sort the list, but I have not enough time for this.
		// If anyone sort the list, take care on the MS2 scan: it is duplicated when you use pParse+ (brute force to generate the training set).
	}

	return ret;
}

void TrainingSetreader::OutputCuases(string CauseFile)
{
	FILE * pfile = fopen(CauseFile.c_str(),"w");
	for (unsigned int i=0;i< CheckFlag.size();i++)
	{
		if (CheckFlag[i]==0)
		{
			fprintf(pfile,"%d,%.5lf,%d,%e,%d,%d,%d\n",MS2ScanNoList[i],MZList[i],chargeList[i],evalueList[i],m_causes_not_in_mono_list[i],m_causes_not_in_ranking_topn[i],m_causes_abandon_by_mars[i]);
			//cout << MS2ScanNoList[i] << tab << MZList[i] << tab << chargeList[i] <<tab << evalueList[i]<< endl;
		}
	}
	fclose(pfile);
}
