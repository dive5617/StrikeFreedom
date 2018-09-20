#include <stdio.h>
#include <string>
#include <iostream>
#include <fstream>
#include <stdexcept>

#include "com.h"
#include "BasicFunction.h"

using namespace std;

/**
*	class CTools
**/
int CTools::gSortByCol = 2;// Initiation

// Attention these two comp are different.(> and <)
bool CTools::compLessMZ(const CentroidPeaks &x, const CentroidPeaks &y)
{
	return x.m_lfMZ < y.m_lfMZ;
}

bool CTools::compHighIntens(const CentroidPeaks &x, const CentroidPeaks &y)
{
	return x.m_lfIntens > y.m_lfIntens;
}

bool CTools::compPrecursorByCol(const vector<double> &X1, const vector<double> &X2)
{
	return X1[gSortByCol] < X2[gSortByCol];
}
bool CTools::compPrecursorByColDescend(const vector<double> &X1, const vector<double> &X2)
{
	return X1[gSortByCol] > X2[gSortByCol];
}

bool CTools::IsFileExists(string Filename, string mode)
{
	FILE *pTest = fopen(Filename.c_str(),mode.c_str());
	if (pTest == NULL)
	{
		return false;
	} else {
		fclose(pTest);
		return true;
	}
}

int CTools::charCount(string &x, char y)
{
	if (x.size() == 0)
	{
		return 0;
	}
	int ret = 0;
	for (size_t i = 0; i < x.size(); ++i)
	{
		if (x[i] == y) ++ret;
	}
	return ret;
}

int CTools::OutputFilesize(const char * filename)
{
	FILE *stream;
	stream = fopen(filename, "rb");
	if (NULL == stream)
	{
		CErrInfo info("CTools", "OutputFilesize", "Failed to open file " +  string(filename));
		throw runtime_error(info.Get().c_str());
	}
	long length = filesize(stream);
	fclose(stream);
	return length;
}

long CTools::filesize(FILE *stream)
{
	long curpos, length;
	curpos = ftell(stream);
	fseek(stream, 0L, SEEK_END);
	length = ftell(stream);
	fseek(stream, curpos, SEEK_SET);
	return length;
}

void CTools::ClearFile(const char * filename)
{
	ofstream fout;
	fout.open(filename,std::ios::out);
	fout.close();
}

string CTools::int2string(int x)
{
	char tmp[40];
	sprintf(tmp, "%d", x);
	return tmp;
}

/**
*	class CStatistic
**/

void CStatistic::CalculateMean(vector<vector<double> > &SampleMatrix,vector<double> &Mean)
{
	//Init
	int SampleMatrix_Col=SampleMatrix[0].size();
	int SampleMatrix_Row=SampleMatrix.size();
	Mean.resize(0);
	for (int j=0;j<SampleMatrix_Col;j++)
	{
		Mean.push_back(0.0);
	}
	//sum
	for (int i=0;i<SampleMatrix_Row;i++)
	{
		for (int j=0;j<SampleMatrix_Col;j++)
		{
			Mean[j]+=SampleMatrix[i][j];
		}
	}
	//cout << "Sum" <<endl;
	//Average
	for (int j=0;j<SampleMatrix_Col;j++)
	{
		//cout << Mean[j] <<endl;
		Mean[j]=Mean[j]/SampleMatrix_Row;	
	}
}
void CStatistic::CalculateSTD(vector<vector<double> > &SampleMatrix,vector<double> &Mean,vector<double> &STD)
{
	//Init
	int SampleMatrix_Col=SampleMatrix[0].size();
	int SampleMatrix_Row=SampleMatrix.size();
	STD.resize(0);
	for (int j=0;j<SampleMatrix_Col;j++)
	{
		
		STD.push_back(0.0);
	}
	
	//STD
	for (int i=0;i<SampleMatrix_Row;i++)
	{
		for (int j=0;j<SampleMatrix_Col;j++)
		{
			STD[j]+=((SampleMatrix[i][j]-Mean[j])*(SampleMatrix[i][j]-Mean[j]));
		}

	}
	for (int j=0;j<SampleMatrix_Col;j++)
	{
		STD[j]=sqrt(STD[j]/(SampleMatrix_Row-1));
	}
}
void CStatistic::Calculate_Mean_STD(vector<vector<double> > &SampleMatrix,vector<double> &Mean,vector<double> &STD)
{
	CalculateMean(SampleMatrix,Mean);
	CalculateSTD(SampleMatrix,Mean,STD);
}

// 计算两个向量X和Y的皮尔逊相关系数 cov(X，Y)/sqrt(D(X),D(Y)) = 0, 不相关； >0 正相关； <0 负相关
double CStatistic::CalculateVectorCov(vector<double> &X, vector<double> &Y)
{
	double SumXY = 0;
	double SumXX = 0;
	double SumYY = 0;
	double SumX = 0;
	double SumY = 0;
	double MeanX = 0;
	double MeanY = 0;
	int L = 0;

	if(X.size() <= Y.size())	
		L = (int)X.size();
	else L = (int)Y.size();
	for (int i = 0; i < L; ++i)
	{
		SumXY += X[i] * Y[i];
		SumXX += X[i] * X[i];
		SumYY += Y[i] * Y[i];
		SumX += X[i];
		SumY += Y[i];
	}
	MeanX = SumX / L;
	MeanY = SumY / L;
	double A = sqrt((SumXX - L * MeanX * MeanX) * (SumYY - L * MeanY * MeanY));
	double B = (SumXY - L * MeanX * MeanY);
	if (fabs(A - 0.0) < eps) 
	{
		return 0.0;
	}
	return (B / A);
}

void CStatistic::Normalization(vector<vector<double> > &SampleMatrix)
{

	vector<double> Mean;
	vector<double> STD;
	Calculate_Mean_STD(SampleMatrix,Mean,STD);
	//cout << "Mean\tSTD" <<endl;
	//for (int Mean_i = 0; Mean_i < Mean.size(); Mean_i++)
	//{
	//	cout << Mean_i << "\t" << Mean[Mean_i] << "\t" << STD[Mean_i] <<endl;
	//}
	for (int i = 0; i < (int)SampleMatrix.size(); i++)
	{
		for (int j = 0; j < (int)SampleMatrix[0].size(); j++)
		{
			SampleMatrix[i][j]=(SampleMatrix[i][j]-Mean[j])/STD[j];
		}

	}
}

// For given vector Data, if we find the point which is less than the one after it
// while not greater than the one before it, then we push_back the index of it.
// If we found the min return ture, otherwise false.
bool CStatistic::FindLocalMinimal(vector<int> &localMinimal, vector<double> &data)
{
	if(0 == data.size()) return false;
	if (data[0] < eps) localMinimal.push_back(0);
	for (int i = 1; i < (int)data.size() - 1; i++)
	{
		if (data[i-1] >= data[i] && data[i] < data[i+1])
		//if (data[i] < eps)
		{
			localMinimal.push_back(i);
		}
	}
	if (data.back() < eps) localMinimal.push_back((int)data.size() - 1);

	return (0 != (int)localMinimal.size());
}

bool CStatistic::FindLocalMax(vector<int> &posRes, vector<double> &data)
{
	if(data.size() == 0)
	{
		return false;
	} else if(data.size() == 1) {
		posRes.push_back(0);
		return true;
	}
	if(data[0] > data[1])
	{
		posRes.push_back(0);
	}
	for(int i = 1; i < (int)data.size() - 1; ++i)
	{
		if(data[i-1] < data[i] && data[i+1] < data[i]) 
		{
			posRes.push_back(i);
		}
	}
	return (0 != (int)posRes.size());
}


bool ErrorStruct::Errorcmp(const ErrorStruct x, const ErrorStruct y)
{
	return x.ErrorRate > y.ErrorRate;
}