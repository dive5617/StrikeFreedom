#include <iostream>
#include <cstdio>
#include <cstdlib>
#include <fstream>
#include <vector>
#include <cmath>

#include "Classification.h"

using namespace std;

CClassify::CClassify(string &path)
{
	m_strTrainFile = path + "TrainingSetSVM.txt";
	m_strTestFile = path + "TestSetSVM.txt";
	m_strModelFile = path + "SVMmodel.txt";
	m_strOutFile = path + "SVMpredict.txt";
}

CClassify::~CClassify()
{
	RemoveTmpFiles();
	m_strTrainFile.clear();
	m_strTestFile.clear();
	m_strModelFile.clear();
	m_strOutFile.clear();
}

void CClassify::RemoveTmpFiles()
{
	remove(m_strTrainFile.c_str());
	remove(m_strTestFile.c_str());
	remove(m_strModelFile.c_str());
	remove(m_strOutFile.c_str());
}

// 创建训练集和测试集
int CClassify::SVMPrepareData(vector<vector<double> > &featureInfo, vector<vector<double> > &OutputInfo, TrainingSetreader *pTrainSet)
{
	FILE *ptrain = fopen(m_strTrainFile.c_str(), "w");
	FILE *ptest = fopen(m_strTestFile.c_str(), "w");

	int trainSetSize = 0;
	if(pTrainSet != NULL)
	{
		int posTrainNum = 0;
		int negTrainNum = 0;
		for (size_t i = 0; i < OutputInfo.size(); ++i)
		{
			int tmpMS2Scan = (int)OutputInfo[i][0];
			int tmpChg = (int)OutputInfo[i][1];
			double tmpMZ = OutputInfo[i][2];
		
			bool IsPositive = pTrainSet->CheckScanMZCharge(tmpMS2Scan, tmpMZ, tmpChg);
			//pTrainSet->CheckTopNList(tmpMS2Scan, tmpMZ, tmpChg);//To be Checked

			if(IsPositive)
			{
				++posTrainNum;
				fprintf(ptrain, "1 "); //正样本
				fprintf(ptest, "0 ");
				for(int j = 0; j < featureInfo[i].size(); ++j)
				{
					fprintf(ptrain, "%d:%f ", j+1, featureInfo[i][j]);
					fprintf(ptest, "%d:%f ", j+1, featureInfo[i][j]);
				}
				fprintf(ptrain, "\n");
				fprintf(ptest, "\n");
			} else {
				if(negTrainNum < posTrainNum)
				{
					++negTrainNum;
					fprintf(ptrain, "-1 "); //负样本
					for(int j = 0; j < featureInfo[i].size(); ++j)
					{
						fprintf(ptrain, "%d:%f ", j+1, featureInfo[i][j]);
					}
					fprintf(ptrain, "\n");
				}

				fprintf(ptest, "0 ");
				for(int j = 0; j < featureInfo[i].size(); ++j)
				{
					fprintf(ptest, "%d:%f ", j+1, featureInfo[i][j]);
				}
				fprintf(ptest, "\n");
			}
		}
		trainSetSize = posTrainNum + negTrainNum;
	} else { //
		size_t i = 0;
		while(i < OutputInfo.size())
		{
			if(0 == OutputInfo[i].size())
			{
				break;
			}
			int tmpMS2Scan = (int)OutputInfo[i][0];
			int tmpChg = (int)OutputInfo[i][1];
			double score = featureInfo[i][(int)featureInfo[i].size()-1];
			int choosePos = -1;
			if(tmpChg > 3 && score > 1.5)
			{
				++trainSetSize;
				fprintf(ptrain, "1 "); //第一名为正样本
				fprintf(ptest, "0 ");
				for(size_t j = 0; j < featureInfo[i].size(); ++j)
				{
					//if(j == deleteFeatureID) continue;
					fprintf(ptrain, "%d:%f ", j+1, featureInfo[i][j]);
					fprintf(ptest, "%d:%f ", j+1, featureInfo[i][j]);
				}
				fprintf(ptrain, "\n");
				fprintf(ptest, "\n");
				choosePos = i;
			} else {
				fprintf(ptest, "0 ");
				for(size_t j = 0; j < featureInfo[i].size(); ++j)
				{
					//if(j == deleteFeatureID) continue;
					fprintf(ptest, "%d:%f ", j+1, featureInfo[i][j]);
				}
				fprintf(ptest, "\n");
			}
			++i;
			if(0 == OutputInfo[i].size())
			{
				continue;
			} else if(tmpMS2Scan != (int)OutputInfo[i][0]) {
				continue; // only one candidate
			}
			while(i+1 < OutputInfo.size() && tmpMS2Scan == (int)OutputInfo[i+1][0])
			{
				tmpChg = (int)OutputInfo[i][1];
				fprintf(ptest, "0 ");
				for(size_t j = 0; j < featureInfo[i].size(); ++j)
				{
					//if(j == deleteFeatureID) continue;
					fprintf(ptest, "%d:%f ", j+1, featureInfo[i][j]);
				}
				fprintf(ptest, "\n");
				++i;
			}
			if(choosePos >= 0)
			{
				++trainSetSize;
				fprintf(ptrain, "-1 "); //最后一名作负样本
				fprintf(ptest, "0 ");
				for(size_t j = 0; j < featureInfo[i].size(); ++j)
				{
					//if(j == deleteFeatureID) continue;
					fprintf(ptrain, "%d:%f ", j+1, featureInfo[i][j]);
					fprintf(ptest, "%d:%f ", j+1, featureInfo[i][j]);
				}
				fprintf(ptrain, "\n");
				fprintf(ptest, "\n");
			} else {
				fprintf(ptest, "0 ");
				for(size_t j = 0; j < featureInfo[i].size(); ++j)
				{
					//if(j == deleteFeatureID) continue;
					fprintf(ptest, "%d:%f ", j+1, featureInfo[i][j]);
				}
				fprintf(ptest, "\n");
			}
			++i;
		} // end while
	}

	fclose(ptrain);
	fclose(ptest);
	return trainSetSize;
}

void CClassify::SVMTraining()
{
	FILE *ftrain = fopen(m_strTrainFile.c_str(), "r");
	if(ftrain == NULL)
	{
		fclose(ftrain);
		cout<<"[Error] There is no training data!" << endl;
		return;
	} else {
		fclose(ftrain);
	}
	string mscmdline = "svm_learn.exe -v 0 -c 1 -t 2 -g 0.01 -x 1 \"" + m_strTrainFile + "\" \"" + m_strModelFile + "\"";
	try
	{
		//cout << mscmdline << endl;
		system(mscmdline.c_str());
	}
	catch(exception & e)
	{
		CErrInfo info("CClassify", "SVMTraining", "failed.");
		throw runtime_error(info.Get(e).c_str());
		//throw ("CClassify::SVMTraining() failed.");
	}
	catch(...)
	{
		CErrInfo info("CClassify", "SVMTraining", "Caught an unknown exception from SVMTraining.");
		throw runtime_error(info.Get().c_str());
		//throw ("CClassify::SVMTraining() Caught an unknown exception from SVMTraining.");
	}
}

void CClassify::SVMPredict(vector<double> &vPredictY)
{
	FILE *fmodel = fopen(m_strModelFile.c_str(), "r");
	if(fmodel == NULL)
	{
		fclose(fmodel);
		cout<<"[Error] There is no model!" << endl;
		return;
	} else {
		fclose(fmodel);
	}
	string mscmdline = "svm_classify.exe  -v 0 \"" + m_strTestFile + "\" \"" + m_strModelFile + "\" \"" + m_strOutFile + "\"";
	try
	{
			system(mscmdline.c_str());
	}
	catch(...)
	{
		throw("Something wrong in SVM predict!");
	}
	FILE *fp = fopen(m_strOutFile.c_str(), "r");
	double predictY = 0.0;
	while(fscanf(fp, "%lf", &predictY) > 0)
	{
		vPredictY.push_back(predictY);
	}
	fclose(fp);
}

void CClassify::MARSPredict(vector<vector<double> > &featureInfo, vector<double> &vPredictY)
{
	for (size_t i = 0; i < featureInfo.size(); ++i)
	{
		double predictY = 0.0;
		predictY = PredictMARSer1013(&featureInfo[i][0]);
		vPredictY.push_back(predictY);
	}
}

// 添加mono峰相对强度理论与实验的偏差特征
double CClassify::PredictMARSer1013(double Feature[])
{
double x1=Feature[0];
double x2=Feature[1];
double x3=Feature[2];
double x4=Feature[3];
double x5=Feature[4];
double x6=Feature[5];
double x7=Feature[6];
double x8=Feature[7];
double x9=Feature[8];
double x10=Feature[9];
double x11=Feature[10];
double f6_1=0;
double f4_1=0;
double BF3=0;
double BF2=0;
double BF1=0;
double BF7=0;
double BF6=0;
double BF5=0;
double BF4=0;
double BF9=0;
double BF8=0;
double r1_1=0;
double f11_1=0;
double f2_1=0;
double f7_1=0;
double r4_1=0;
double f5_1=0;
double r2_1=0;
double p8_1=0;
double r9_1=0;
double r10_1=0;
double f3_1=0;
double BF12=0;
double BF10=0;
double r12_1=0;
double f1_1=0;
double r7_1=0;
double p10_1=0;
double f9_1=0;
double r5_1=0;
double p2_1=0;
double r3_1=0;
double p4_1=0;
double p6_1=0;
double f8_1=0;
double r8_1=0;
double p7_1=0;
double p9_1=0;
double BF11=0;
double p12_1=0;
double p1_1=0;
double p11_1=0;
double p3_1=0;
double p5_1=0;
double r11_1=0;
double f10_1=0;
double y=0;
double r6_1=0;
double f12_1=0;
if (x11 <= 0.309529) 
{
 f1_1 = 0;
}
if (0.309529 < x11 && x11 < 0.6550595) 
{
  p1_1 = (2*(0.6550595) + (0.309529) - 3*(0.331474)) / pow(((0.6550595) - (0.309529)),2);
  r1_1 = (2*(0.331474) - (0.6550595) - (0.309529)) / pow(((0.6550595) - (0.309529)),3);
  f1_1 = p1_1*pow((x11-(0.309529)),2) + r1_1*pow((x11-(0.309529)),3);
}

if (x11 >= (0.6550595)) 
{
 f1_1 = x11 - (0.331474);
}
BF1 = f1_1;
if (x11 <= 0.309529) 
{
 f2_1 = -(x11 - (0.331474));
}
if (0.309529 < x11 && x11 < 0.6550595) 
{
  p2_1 = (3*(0.331474) - 2*(0.309529) - (0.6550595)) / pow(((0.309529) - (0.6550595)),2);
  r2_1 = ((0.309529) + (0.6550595) - 2*(0.331474)) / pow(((0.309529) - (0.6550595)),3);
  f2_1 = p2_1*pow((x11-(0.6550595)),2) + r2_1*pow((x11-(0.6550595)),3);
}

if (x11 >= (0.6550595)) 
{
 f2_1 = 0;
}
BF2 = f2_1;
if (x2 <= 0.435024) 
{
 f3_1 = 0;
}
if (0.435024 < x2 && x2 < 0.9323325) 
{
  p3_1 = (2*(0.9323325) + (0.435024) - 3*(0.870048)) / pow(((0.9323325) - (0.435024)),2);
  r3_1 = (2*(0.870048) - (0.9323325) - (0.435024)) / pow(((0.9323325) - (0.435024)),3);
  f3_1 = p3_1*pow((x2-(0.435024)),2) + r3_1*pow((x2-(0.435024)),3);
}

if (x2 >= (0.9323325)) 
{
 f3_1 = x2 - (0.870048);
}
BF3 = f3_1;
if (x7 <= 0.3333335) 
{
 f4_1 = 0;
}
if (0.3333335 < x7 && x7 < 0.8333335) 
{
  p4_1 = (2*(0.8333335) + (0.3333335) - 3*(0.666667)) / pow(((0.8333335) - (0.3333335)),2);
  r4_1 = (2*(0.666667) - (0.8333335) - (0.3333335)) / pow(((0.8333335) - (0.3333335)),3);
  f4_1 = p4_1*pow((x7-(0.3333335)),2) + r4_1*pow((x7-(0.3333335)),3);
}

if (x7 >= (0.8333335)) 
{
 f4_1 = x7 - (0.666667);
}
BF4 = f4_1;
if (x1 <= 0.1833335) 
{
 f5_1 = 0;
}
if (0.1833335 < x1 && x1 < 0.616667) 
{
  p5_1 = (2*(0.616667) + (0.1833335) - 3*(0.366667)) / pow(((0.616667) - (0.1833335)),2);
  r5_1 = (2*(0.366667) - (0.616667) - (0.1833335)) / pow(((0.616667) - (0.1833335)),3);
  f5_1 = p5_1*pow((x1-(0.1833335)),2) + r5_1*pow((x1-(0.1833335)),3);
}

if (x1 >= (0.616667)) 
{
 f5_1 = x1 - (0.366667);
}
BF5 = f5_1;
if (x1 <= 0.1833335) 
{
 f6_1 = -(x1 - (0.366667));
}
if (0.1833335 < x1 && x1 < 0.616667) 
{
  p6_1 = (3*(0.366667) - 2*(0.1833335) - (0.616667)) / pow(((0.1833335) - (0.616667)),2);
  r6_1 = ((0.1833335) + (0.616667) - 2*(0.366667)) / pow(((0.1833335) - (0.616667)),3);
  f6_1 = p6_1*pow((x1-(0.616667)),2) + r6_1*pow((x1-(0.616667)),3);
}

if (x1 >= (0.616667)) 
{
 f6_1 = 0;
}
BF6 = f6_1;
if (x8 <= 0.4826525) 
{
 f7_1 = 0;
}
if (0.4826525 < x8 && x8 < 0.9826525) 
{
  p7_1 = (2*(0.9826525) + (0.4826525) - 3*(0.965305)) / pow(((0.9826525) - (0.4826525)),2);
  r7_1 = (2*(0.965305) - (0.9826525) - (0.4826525)) / pow(((0.9826525) - (0.4826525)),3);
  f7_1 = p7_1*pow((x8-(0.4826525)),2) + r7_1*pow((x8-(0.4826525)),3);
}

if (x8 >= (0.9826525)) 
{
 f7_1 = x8 - (0.965305);
}
BF7 = f7_1;
if (x11 <= 0.2252495) 
{
 f8_1 = 0;
}
if (0.2252495 < x11 && x11 < 0.309529) 
{
  p8_1 = (2*(0.309529) + (0.2252495) - 3*(0.287584)) / pow(((0.309529) - (0.2252495)),2);
  r8_1 = (2*(0.287584) - (0.309529) - (0.2252495)) / pow(((0.309529) - (0.2252495)),3);
  f8_1 = p8_1*pow((x11-(0.2252495)),2) + r8_1*pow((x11-(0.2252495)),3);
}

if (x11 >= (0.309529)) 
{
 f8_1 = x11 - (0.287584);
}
BF8 = f8_1;
if (x9 <= 0.35642) 
{
 f9_1 = 0;
}
if (0.35642 < x9 && x9 < 0.7637205) 
{
  p9_1 = (2*(0.7637205) + (0.35642) - 3*(0.71284)) / pow(((0.7637205) - (0.35642)),2);
  r9_1 = (2*(0.71284) - (0.7637205) - (0.35642)) / pow(((0.7637205) - (0.35642)),3);
  f9_1 = p9_1*pow((x9-(0.35642)),2) + r9_1*pow((x9-(0.35642)),3);
}

if (x9 >= (0.7637205)) 
{
 f9_1 = x9 - (0.71284);
}
BF9 = f9_1;
if (x11 <= 0.0814575) 
{
 f10_1 = -(x11 - (0.162915));
}
if (0.0814575 < x11 && x11 < 0.2252495) 
{
  p10_1 = (3*(0.162915) - 2*(0.0814575) - (0.2252495)) / pow(((0.0814575) - (0.2252495)),2);
  r10_1 = ((0.0814575) + (0.2252495) - 2*(0.162915)) / pow(((0.0814575) - (0.2252495)),3);
  f10_1 = p10_1*pow((x11-(0.2252495)),2) + r10_1*pow((x11-(0.2252495)),3);
}

if (x11 >= (0.2252495)) 
{
 f10_1 = 0;
}
BF10 = f10_1;
if (x4 <= 0.470986) 
{
 f11_1 = 0;
}
if (0.470986 < x4 && x4 < 0.9709485) 
{
  p11_1 = (2*(0.9709485) + (0.470986) - 3*(0.941972)) / pow(((0.9709485) - (0.470986)),2);
  r11_1 = (2*(0.941972) - (0.9709485) - (0.470986)) / pow(((0.9709485) - (0.470986)),3);
  f11_1 = p11_1*pow((x4-(0.470986)),2) + r11_1*pow((x4-(0.470986)),3);
}

if (x4 >= (0.9709485)) 
{
 f11_1 = x4 - (0.941972);
}
BF11 = f11_1;
if (x4 <= 0.470986) 
{
 f12_1 = -(x4 - (0.941972));
}
if (0.470986 < x4 && x4 < 0.9709485) 
{
  p12_1 = (3*(0.941972) - 2*(0.470986) - (0.9709485)) / pow(((0.470986) - (0.9709485)),2);
  r12_1 = ((0.470986) + (0.9709485) - 2*(0.941972)) / pow(((0.470986) - (0.9709485)),3);
  f12_1 = p12_1*pow((x4-(0.9709485)),2) + r12_1*pow((x4-(0.9709485)),3);
}

if (x4 >= (0.9709485)) 
{
 f12_1 = 0;
}
BF12 = f12_1;
y = 0.50160708354254 +11.5019054156235*BF1 -9.06203363136123*BF2 +3.25968184901274*BF3 +2.12238497237616*BF4 -1.07989966447974*BF5 -1.25571866488606*BF6 +2.41251506109264*BF7 -11.5861457927547*BF8 -4.54009739145506*BF9 +9.80600591069255*BF10 +1.17102661871694*BF11 +0.265569301542246*BF12;
return y;
}
