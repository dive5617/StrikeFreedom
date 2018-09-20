#ifndef Parameters_H_
#define Parameters_H_

//#define _COUT_DEBUG_INFO_(Info)

#include <cstdio>
#include <string>
#include <vector>
#include <set>

#include "BasicFunction.h"

using namespace std;

const string Version = "version 1.0.0";
const string TIMESTRING = "2014.12.01";

//const bool VectorPeakMatrixFlag = true;
//const int ReleaseLevel = 0;
const int MIN_SAMPLE_NUM = 1000;
const int ChromSimilarityNum = 3;
const unsigned int MAXBUF = 128*1024*1024;
const int MAXSCANNUM = 2000000;
//const int MaxBinNum = 600;
const int MaxBinPerTh = 100; // default 100

const int LIPV_MASSMAX = 100000; // 比100 k Da还要大的暂时不处理
const int IPV_MASSMAX = 10000;  // IPV文件存储最多1w Da
const int IPV_PEAKMAX = 15;

const int Span = 5; //default 5
const int HalfIsoWidth = 7; // half of the isotopic pattern
const int IsoProfileNum = 2 * HalfIsoWidth + 1; // Unchangable
const int MaxIsoFeature = 2*Span*IsoProfileNum + IsoProfileNum;
//const int NotOutputMonoIndex = -1;
const int MaxCharInLine = 512;
const double ExtendHalfwindow = 0.5;
const double pmass = 1.007276e0;
const double massH = 1.007825035;
const double massH2O = 18.0105647;
const double massNH3 = 17.026549105;
const double eps = 1e-8;

const double avgdiv = 1.003e0;  // 单电荷时的平均谱峰间隔
const double stddiv = 0.0016e0;
const int chgStart = 2;
//const int chgEnd = 30;
//const double Peak_Tol_ppm = 20e-6;
const int Threshold_Out = 16*1024*1024;
const int MGFBufferSize = 32*1024*1024;
const int unmatchedNum = 2; // 进行同位素模式截断时允许两端遗漏的极小值 add by luolan
const int PeaksforChromSim = 2; // 考虑最高峰左右k根峰，进行计算色谱相似度


//ErrorInfo
const char ErrorInfo_Checkfile[1000] = "[Exit] pParse was halted for a fatal error.\
 Retry with correct parameters.\n[Help] If the error reccured, contact wulong@ict.ac.cn for help.";
const char tab = '\t';
const char space = ' ';

// Output 
const int OutPutFileTypeNum = 6;
const int StringMinLength = 20;// 一些常用的类型，用于存储如CID，ETD的字符串长度。
const int ActivateTypeNum = 3;
const int InstrumentTypeNum = 2;
const char cActivateType[ActivateTypeNum][StringMinLength] = {"CID","ETD","HCD"};
const char cInstrumentType[InstrumentTypeNum][StringMinLength] = {"IT","FT"};
const char OutputSuffix[OutPutFileTypeNum][StringMinLength] = {"_CIDIT_","_ETDIT_","_HCDIT_","_CIDFT_","_ETDFT_","_HCDFT_"};
const char MGFSuffix[OutPutFileTypeNum][StringMinLength] = {"_CIDIT.mgf","_ETDIT.mgf","_HCDIT.mgf","_CIDFT.mgf","_ETDFT.mgf","_HCDFT.mgf"};
const char PFSuffix[OutPutFileTypeNum][StringMinLength] = {"_CIDIT.pf2","_ETDIT.pf2","_HCDIT.pf2","_CIDFT.pf2","_ETDFT.pf2","_HCDFT.pf2"};

//const bool OutputBool = false;
//const int Threshold_Random_Positive = 3000;
//const int Threshold_Random_Negtive = 2;//1-10 >10 means no threshold
//const string TestFilePositive = "PositiveSample.csv";
// Just for SVM_light

//const string Train_SVMlight = "Train.txt";
//const string TestFileNegtive = "NegtiveSample.csv";
//const string FeatureModelFile = "FeatureModel.txt";
//const string inFilename = "TrainingSet.txt";
//Candidates Filters
//const bool filter_Isolength = true;// This is Recommend to be false as it cut many good ones.
//const bool filter_SameCut = true;// delete the candidate similar to the ones have been output
//const bool filter_charge = true;
//const bool filter_chg3_length = true;
//const bool filter_IsolationWindow = true;
//const bool filter_PeakRatio = true;
//const bool filter_Intensity = true;
//const double Ratio = 10.0;
//const bool outputcsv = true;
//Common Functions

const int setSize = 2;
const string HighPreSet[setSize] = {"FTMS", "Q-TOF"};  // 高精度
const string LowPreSet[setSize] = {"ITMS", ""};   // 低精度

#endif

