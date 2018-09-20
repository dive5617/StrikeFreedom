
#include <vector>
#include <iostream>
#include <iomanip>
#include <fstream>
#include <string>
#include <iomanip>
#include <algorithm> 
#include <cmath>
#include <iterator>  
#include <cctype> 
#include <cstdio>

#include "com.h"
#include "MS1Spectrum.h"
#include "MS1Reader.h"
#include "MS2Reader.h"
#include "TrainingFileReader.h"
#include "BasicFunction.h"
#include "SpecDataIO.h"
#include "emass.h"
#include "Parameters.h"

using namespace std;

//#define _DEBUG_TEST2
// 初始化MS1Reader对象， halfWnd比隔离窗口左右各宽了0.5Da
MS1Reader::MS1Reader(string &infilename, CParaProcess *para, double **vIPV, CEmass *cEmass, double halfWnd) : 
	m_lfHalfWnd(halfWnd), m_PeakMap(MaxBinPerTh, Span, halfWnd), m_cEmass(cEmass), m_para(para), m_vIPV(vIPV)
{
	m_vnIndexListToOutput.reserve(100);
	IsoPattern.assign(MaxIsoFeature, 0);
	ChromSimilarity.assign(ChromSimilarityNum, 0);
	vector<vector<double> > tmp;
	MarsFeature.swap(tmp);
	MS1Scan_Index.resize(MAXSCANNUM);
	Peak_Tol_ppm = 1.0e-6 * atof(m_para->GetValue("mz_error_tolerance", "20").c_str());

	pTrainSet = NULL;
	cout << "[pParseTD] Loading MS1..." << endl;
	
	//long length = CTools::OutputFilesize(infilename.c_str());
	try{
		CspecDataIO *io = new CspecDataIO;
		io->MS1Parse(infilename, this);
		delete io;
	}catch(...){
		cout<<"MS1 parse error!"<<endl;
	}
	MS1Scancounts = MS1List.size();
	cout << "[pParseTD] Survey MS scan number: " << MS1Scancounts <<endl;

	FileterEachSpectrumByBaseLine(); // TODO: TD MS need new method to filter the nosy peaks
	CTools::gSortByCol = 2;//2 MaxSim 3 STD 4 EluteSim
}

MS1Reader::~MS1Reader()
{
	this->ReleaseTrainingSet();
	for(vector<MS1Spectrum *>::iterator it = MS1List.begin(); it != MS1List.end(); ++it)
	{
		delete (*it);
	}
	delete classify;
	if(pTrainSet != NULL){
		delete pTrainSet;
	}
}

int MS1Reader::GetSpectraNum()
{
	return MS1Scancounts;
}

//获取除虚拟谱峰之外的所有其余谱峰的强度之和，建议先进行极小值判断。。
double MS1Reader::GetIntensitySum()
{
	double sumIsopattern = 0;
	for (int ino = IsoProfileNum; ino < MaxIsoFeature; ino++)
	{
		if (ino % IsoProfileNum != 0 )
			sumIsopattern += IsoPattern[ino];
	}
	return sumIsopattern;
}

/*Set TrainingSet.txt*/
void MS1Reader::SetTrainingSet()
{
	if (m_para->GetValue("output_trainingdata") =="0")
	{
		pTrainSet = NULL;
	} else {
		pTrainSet = new TrainingSetreader(m_para->GetValue("trainingset"), 1.0e-6 * atof(m_para->GetValue("mz_error_tolerance").c_str()));
		pTrainSet->InitTrainData();
	}
	
}

void MS1Reader::SetClassifier(string path)
{
	classify = new CClassify(path);
}

/*Release TrainingSet.txt*/
void MS1Reader::ReleaseTrainingSet()
{
	if (pTrainSet != NULL)
	{
		delete pTrainSet;
	}
}


void MS1Reader::FileterEachSpectrumByBaseLine()
{
	// 对于每一张一级谱按照baseline进行过滤
	for (unsigned int i = 0; i < MS1List.size(); ++i)
	{
		MS1List[i]->FilterByBaseline();
	}
}

/*
	For given MS2 spectrum, extract all the related MS1 information.
	Store those infomation in to two data struct: PeakMatrix and PrecursorCandidatePeaks;
	wrm：根据二级谱的scan查找对应的一级谱(找到第一个小于MS2Scan的一级谱Scan)；并根据二级谱中给的碎裂中心，在一级谱中找到相关的母离子
	     可是MS2文件中每张二级谱明明都已经给出了其先导离子的Scan号啊
*/
void MS1Reader::GetIndex_By_MS2Scan(int &index,int &MS2scan) // CHECK luolan
{
	// TODO：这个函数实际上可以加速到log(n) 根据二级谱查找一级谱，没有必要挨个实验；wrm: 或者二级谱和一级谱之间建立哈希表
	for (int i = MS1Scancounts-1; i >= 0; --i)
	{
		if (MS2scan > MS1List[i]->GetCurrentMS1Scan())
		{
			index = i;
			break;
		} 
	}
}

// wrm: 获取给定窗口内的所有谱峰
void MS1Reader::GetMS1_Peaks_In_Window_On_Given_Index(double MZ, double Halfwindow, int MS1Index, vector<CentroidPeaks> &PeakList)
{
	MS1List[MS1Index]->GetPeaksInWindow(MZ, Halfwindow, PeakList);
	//MS1Scan = MS1List[MS1Index]->GetCurrentMS1Scan();
}
void MS1Reader::InitPeaksRecord()
{
	if (!PeakRecord.empty())
	{
		PeakRecord.clear();
	}
	
}
/* Revision here with conporderation */ 
// CHECK luolan
// 根据二级谱查找对应的一级谱；并根据二级谱所给碎裂中心获取候选母离子单同位素峰，枚举最高峰和电荷
// [-5,+5]
void MS1Reader::GetCandidatesPrecursorByMS2scan(int MS2scan, int MS1scan, int userSpanStart,int userSpanEnd,double ActivationCenter,vector<CentroidPeaks> &PrecursorCandidatePeaks)
{
	// Called by AssessPrecursor()
	// Peaks with intensity less than 2% of the highest peak's intensity are deleted.
	// Baseline(0) intensity are deleted.
	
	int MS1_idx = -1;                                       // -1 for empty spectrum
	// 直接读取之前存储的MS2对应的一级谱Scan.  modified by wrm. 2015.10.10
	// GetIndex_By_MS2Scan(MS1_idx, MS2scan);                 // Get the index for the MS1 spectrum.
	MS1_idx = MS1Scan_Index[MS1scan];                  // should first check if MSscan1 is valid 

	// 初始化谱峰关联矩阵为全0，保存前后10张谱的谱峰信息
	InitPeakMatrix();                                     // Initialize all the item as zeros.
	// 清空谱峰记录器中的信息
	InitPeaksRecord();                                    // we store the peaks in this structure. Calc STD of MONO.

	// 记录第-1， 0， 1张一级谱中的谱峰在候选谱峰列表中的起始位置，即PrecursorCandidatePeaks中从第几个谱峰开始是属于第-1/0/1张谱的
	// 0 -> index1 -> index2 -> index3
	vector<int> PeakEndIndex;                             // Record the peak index of the MS1(-1) MS1(0) MS1(1) refer to MS2 
	PeakEndIndex.push_back(0);
	string RelatedMS1 = "";
	//FILE * pfilems1 = NULL;

	double halfWindow = 1.0 * GetIsolationWidth() / 2;

	// 查看-4，-3，-2，-1，0，1，2，3，4，5这10张1级谱
	for ( int j = userSpanStart + 1 ; j <= userSpanEnd && MS1_idx + j < MS1Scancounts ; ++j )
	{
		// Get the 10 * 600 Matrix filled.
		// Skip those negative indexes. When looks up the five spectrum before current spectrum.
		if (MS1_idx + j < 0)
		{
			if(j >= -1 && j <= 1)  // j [-1,0,1], MS1_idx [0]	
			{
				PeakEndIndex.push_back(0);
			}
			continue;
		} 

		// 先取得碎裂中心附近的谱峰
		vector<CentroidPeaks> VectorPeaksTmp;                // Store Peaks temporarily.
		double LargestIntensity = 0.0;                       // Most intense peak

		// Extract MS1 peaks from given MS1 spectrum .
		// int tScan = MS1List[MS1_idx + j]->GetCurrentMS1Scan();
		// wrm: 在halfwindow的基础上向外扩展0.5TH寻找谱峰, 但在后面求碎裂窗口内的最高峰时却没有考虑扩展窗口内的谱峰？
		GetMS1_Peaks_In_Window_On_Given_Index(ActivationCenter, halfWindow + ExtendHalfwindow, MS1_idx + j, VectorPeaksTmp);

		for (unsigned int ino = 0; ino < VectorPeaksTmp.size(); ++ino)
		{
			//每一根峰都要放入到PeakRecord中记录下来。
			PeakRecord.push_back(VectorPeaksTmp[ino]);// 后续用于计算质量校准
			
			if (VectorPeaksTmp[ino].m_lfIntens > LargestIntensity) //求出最强谱峰
			{
				if (VectorPeaksTmp[ino].m_lfMZ < ActivationCenter - halfWindow || VectorPeaksTmp[ino].m_lfMZ > ActivationCenter + halfWindow)
				{
					continue;
				}
				LargestIntensity = VectorPeaksTmp[ino].m_lfIntens;
			}
		}
		//// 用噪音基线过滤 // comment by luolan @2014.12.02
		//// Here can be optimized for the getbaseline only once...
		//double baselineHere = GetBaselineByIndex(MS1_idx + j);
		//for (int ino = VectorPeaksTmp.size() - 1; ino >= 0; ino--)
		//{
		//	//删除噪音峰，实际上什么也没有做。
		//	if (VectorPeaksTmp[ino].m_lfIntens < LargestIntensity * 0.02 || VectorPeaksTmp[ino].m_lfIntens < baselineHere)
		//	{
		//		VectorPeaksTmp.erase(VectorPeaksTmp.begin() + ino);
		//	}
		//}
		
		double baselineHere = GetBaselineByIndex(MS1_idx + j);
		for (unsigned int ino = 0; ino < VectorPeaksTmp.size(); ++ino)
		{
			//构建叠加谱图V。里面所有的谱峰都是候选谱峰。
			if (j >= -1 && j <= 1 && VectorPeaksTmp[ino].m_lfIntens > baselineHere)
			{   //先导MS1谱图的前后各一张一级谱中谱峰强度超过基线的才作为候选母离子的最高峰
				PrecursorCandidatePeaks.push_back(VectorPeaksTmp[ino]);
			}
			//构建关联矩阵，里面记录了所有的相关的谱峰。放入10 * (隔离窗口*100) 的矩阵中。临近谱峰的强度会叠加。	
			m_PeakMap.IntensityMap(VectorPeaksTmp[ino].m_lfMZ, VectorPeaksTmp[ino].m_lfIntens, ActivationCenter,j);
		}
		//判断编号是否是在向前数一张，向后数一张的范围内, modified by luolan @2014.12.02
		if (j >= -1 && j <= 1)
		{
			PeakEndIndex.push_back(PrecursorCandidatePeaks.size());//记录每一个谱图中谱峰在PrecursorCandidatePeaks中的起始位置。便于删除。
		}
	}

	// [wrm]: 若前后10张一级谱中都没有候选谱峰，则返回； 否则对候选谱峰进行过滤
	if (!PrecursorCandidatePeaks.size())
	{
		//How could there be no peak? Let's return and do nothing!		
		return;
	}
	// This Operation reduce 2/3 of the Results
	bool Check_If_ActivationCenter_Peak_Exist_In_Previous_Scan = false;//判断是否发现了碎裂中心。
	if (m_para->GetValue("check_activationcenter").compare("1") == 0)
	{
		for (int ino = PeakEndIndex[1]; ino < PeakEndIndex[2]; ino++)
		{
			//先在中间谱上查是否有碎裂中心。
			if (fabs(PrecursorCandidatePeaks[ino].m_lfMZ - ActivationCenter) < ActivationCenter*Peak_Tol_ppm)
			{
				Check_If_ActivationCenter_Peak_Exist_In_Previous_Scan = true;
			}
		}

		if (Check_If_ActivationCenter_Peak_Exist_In_Previous_Scan)
		{
			//_COUT_DEBUG_INFO_("Candidates: Searched -1");
			//如果查到了，那么就把剩余的两张谱图的谱峰全部删除。并返回，
			PrecursorCandidatePeaks.erase(PrecursorCandidatePeaks.begin()+PeakEndIndex[2], PrecursorCandidatePeaks.end());//erase the last scan

			PrecursorCandidatePeaks.erase(PrecursorCandidatePeaks.begin(), PrecursorCandidatePeaks.begin()+PeakEndIndex[1]);// erase the first scan.
			return;
		}

		Check_If_ActivationCenter_Peak_Exist_In_Previous_Scan = false;
		for (int ino=PeakEndIndex[0];ino<PeakEndIndex[1];ino++)
		{
			//_COUT_DEBUG_INFO_("Candidates: Searched -1 -2");
			//如果没有查到碎裂中心，在前一张谱图上，查找碎裂中心。
			if (fabs(PrecursorCandidatePeaks[ino].m_lfMZ - ActivationCenter)<ActivationCenter*Peak_Tol_ppm)
			{
				Check_If_ActivationCenter_Peak_Exist_In_Previous_Scan = true;
			}
		}
		if (Check_If_ActivationCenter_Peak_Exist_In_Previous_Scan)
		{
			//如果在前一张谱图上查到了碎裂中心谱峰，那么把后一张谱图的信息删除。并返回，否则什么也不要做，返回。
			//_COUT_DEBUG_INFO_("Candidates: Searched +1 -1 -2 ");
			PrecursorCandidatePeaks.erase(PrecursorCandidatePeaks.begin()+PeakEndIndex[2], PrecursorCandidatePeaks.end());//erase the last scan
			return;
		}	
	}
}

/* Set PeakMatrix as zero */
void MS1Reader::InitPeakMatrix()
{
	m_PeakMap.Reset();
}

// 归一化谱峰强度
void MS1Reader::ScaledIsoPattern(double sumIsopattern)
{
	for (int ino=IsoProfileNum;ino<MaxIsoFeature ;ino++)
	{
		IsoPattern[ino]=IsoPattern[ino]/sumIsopattern;
	}
}

// 获取前后10张一级谱中同位素峰簇强度和
//The Intensity for all;
double MS1Reader::GetSumIsoPattern()
{
	double sumIsopattern=0;
	for (int ino=IsoProfileNum;ino<MaxIsoFeature;ino++)
	{
		sumIsopattern+=IsoPattern[ino];
	}
	return sumIsopattern;
}


// Rewrite by luolan @2014.05.05
// 根据最高峰和电荷，从前后10张谱图中获取所有的同位素模式特征，这里要用到之前保存的谱峰关联矩阵10*(2*IsolationWidth)
void  MS1Reader::GetAllIsoFeature(int chg, double ActivationCenter, vector<CentroidPeaks> &PrecursorCandidatePeaks, int k)
{
	for ( int inoSpan = 0; inoSpan < 2*Span; ++inoSpan )
	{   // 循环查看目前一级谱的前span和后span张一级谱
		int basePos = IsoProfileNum + IsoProfileNum * inoSpan;
		double Tol = ActivationCenter * Peak_Tol_ppm;
		for (int s = -HalfIsoWidth; s <= HalfIsoWidth; ++s)
		{   // 对枚举的最高峰位置，寻找其前HalfIsoWidth(0.5 + isolation/2)和后HalfIsoWidth根谱峰
			int jmpIndex = int((PrecursorCandidatePeaks[k].m_lfMZ - Tol + s * avgdiv / chg - ActivationCenter + m_lfHalfWnd) * MaxBinPerTh);
			if (jmpIndex >= 2 * m_lfHalfWnd * MaxBinPerTh)
			{
				break;
			} else if(jmpIndex < 1) {
				jmpIndex = 1;
			}
			int EndIndex = int((PrecursorCandidatePeaks[k].m_lfMZ + Tol + s * avgdiv / chg - ActivationCenter + m_lfHalfWnd) * MaxBinPerTh);
			for (int j = jmpIndex; j <= EndIndex; ++j)
			{
				IsoPattern[basePos + s + HalfIsoWidth] += m_PeakMap.GetPeak(inoSpan, j);					
			} // end for j		
		} // end for s
	} // end for inoSpan
}

// 计算最高峰及左右几根峰的色谱相似度 modified by luolan @2014.05.12
void MS1Reader::CalcChromSimilarity()
{
	vector<double> vTmpXXSimilarity = vector<double>(); // 存放每根峰对应的向量的内积
	vector<double> vTmpXYSimilarity = vector<double>(); // 存放每根峰对应的向量与最高峰对应的向量的内积
	// -2,-1,0,1,2
	for(int i = -PeaksforChromSim; i <= PeaksforChromSim; ++i)
	{
		double tmpSim = IsoPatternColumnDotProduct(i + HalfIsoWidth, i + HalfIsoWidth);
		vTmpXXSimilarity.push_back(tmpSim);
		tmpSim = IsoPatternColumnDotProduct(i + HalfIsoWidth, HalfIsoWidth);
		vTmpXYSimilarity.push_back(tmpSim);
	}

	double highestSim = vTmpXXSimilarity.at(PeaksforChromSim);
	ChromSimilarity[0] = 0.0; // 左侧最高相似度
	ChromSimilarity[1] = 0.0; // 右侧最高相似度
	if(fabs(highestSim) < 0.00000001) // highestSim == 0.0 浮点数最好不要直接判断是否相等
	{
		return;
	}
	for(int i = 0; i < (int)vTmpXYSimilarity.size(); ++i) 
	{
		if(fabs(vTmpXXSimilarity[i]) < 0.00000001 || i == PeaksforChromSim)
		{
			continue;
		}
		double tmpChromSimilarity = vTmpXYSimilarity[i] / sqrt(vTmpXXSimilarity[i] * highestSim);
		if(i < PeaksforChromSim && tmpChromSimilarity > ChromSimilarity[0])
		{
			ChromSimilarity[0] = tmpChromSimilarity;  // 【0】最高峰与之前的等间隔谱峰的最大相似度
		} else if(i > PeaksforChromSim && tmpChromSimilarity > ChromSimilarity[1]) {
			ChromSimilarity[1] = tmpChromSimilarity; //  【1】最高峰与之后的等间隔谱峰的最大相似度
		}
	}
}
// 向量点积
double MS1Reader::IsoPatternColumnDotProduct(int col_x, int col_y)
{
	double ret = 0.0;
	for (int i = 1; i < 2*Span+1; ++i)
	{
		ret += IsoPattern[i * IsoProfileNum + col_x] * IsoPattern[i * IsoProfileNum + col_y]; 
	}
	return ret;
}
//----------- This function was changed by luolan @2014.05.05 ------------------
// 给定最高峰和电荷，在候选谱峰列表中获取同位素峰簇
// 用同位素峰的偏差来估计最高峰的偏差
void MS1Reader::GetEachIsoPeak(vector<CentroidPeaks> &PrecursorCandidatePeaks, int chg, int k, int &matchedIsoPeak, 
							   double &stdPPMTol, double &sumMatchedInt, double prePeakMz, bool &isIncluded)
{
	//PrecursorCandidatePeaks[k].MZ是最高峰的mz，其取值不会与其他峰偏差超过1Th，因此这里全部用它的20ppm作为误差
	double TolValueTh = PrecursorCandidatePeaks[k].m_lfMZ * Peak_Tol_ppm;
	double totalPPMTol = 0.0; // 保存14根同位素峰的偏差和
	sumMatchedInt = 0.0;
	vector<double> vminBias;  // 保存每根同位素峰的ppm偏差
	// wrm：同位素模式保存了15根峰
	for (int s = -HalfIsoWidth; s <= HalfIsoWidth; ++s)
	{
		double targetMZ = PrecursorCandidatePeaks[k].m_lfMZ + avgdiv * s / chg;
		if(fabs(targetMZ - prePeakMz) <= TolValueTh)  // 如果前一根峰在该最高峰的同位素峰簇中，那么该最高峰在枚举前一根为最高峰时已经考虑过了。
		{
			isIncluded = true;
		}
		double minBias = 10.0;	
		// [wrm] 在候选谱峰列表中查找同位素峰，（可以采用二分或哈希？），取偏差最小那根峰，保存其强度
		for (int l = 0; l < (int)PrecursorCandidatePeaks.size(); ++l)
		{   // PrecursorCandidatePeaks中mz值是由小到大的
			double tmpBias = PrecursorCandidatePeaks[l].m_lfMZ - targetMZ;
			if (tmpBias < -TolValueTh)
			{
				continue;
			} else if (tmpBias > TolValueTh) {
				break;
			} else {
				if (tmpBias < minBias)
				{
					minBias = tmpBias;
					IsoPattern[s + HalfIsoWidth] = PrecursorCandidatePeaks[l].m_lfIntens;
				}
			}
		}
		if(minBias < 10 && s != 0)   // [wrm]： s=0时可以直接省去一重循环
		{
			minBias = minBias / targetMZ * 1000000;
			vminBias.push_back(minBias);
			totalPPMTol += minBias;  // 有正有负
			sumMatchedInt += IsoPattern[s + HalfIsoWidth];
		}
	}
	sumMatchedInt += IsoPattern[HalfIsoWidth];// 匹配上的谱峰强度和，这里再加上最高峰的强度
	matchedIsoPeak = 1 + (int)vminBias.size();// 匹配上的谱峰的根数
	if(matchedIsoPeak > 1)
	{
		totalPPMTol /= (int)vminBias.size();//所有匹配上的谱峰的平均偏差，但是没有考虑后面对这部分的同位素模式进行切割时要删除的一些谱峰
		stdPPMTol = 0.0;
		// [wrm]如果匹配到的峰太少的话就不考虑用平均偏差来校准偏差  modified by wrm  2015.09.24.
		// TODO：究竟该不该用平均偏差来校准偏差，这个还有待进一步实验
		if(stdPPMTol <= 3)  // matchedIsoPeak
		{
			for(int i = 0; i < (int)vminBias.size(); ++i)
					stdPPMTol += fabs(vminBias[i]);
		} else {
			for(int i = 0; i < (int)vminBias.size(); ++i)
				stdPPMTol += fabs(vminBias[i] - totalPPMTol);
		}
		stdPPMTol /= (int)vminBias.size();
	} else {
		stdPPMTol = 20.0;
	}
}

// 初始化IsoPattern，其中需要存放当前一级谱的最高峰附近的同位素峰簇强度，以及连续地前后各几张一级谱中对应的同位素峰簇强度
void MS1Reader::InitIsoPattern()
{
	for (int i_Iso = 0; i_Iso < MaxIsoFeature; ++i_Iso)
	{
		IsoPattern[i_Iso] = 0.0;
	}
}

void MS1Reader::SetSpectrumTitle(string title)
{
	SpectrumTitle = title;
}

// 给定最高峰质量，计算理论同位素峰簇
void MS1Reader::GetTheoryIsoform(double mass, int &highestPeakPos, vector<double> &vTheoIso)
{
	vector<double> emptyVct;
	vTheoIso.swap(emptyVct);
	highestPeakPos = 0;

	int mIdx = int(mass);
	if(mIdx >= LIPV_MASSMAX || mIdx < 0)
	{
		return;
	} else if (mIdx < IPV_MASSMAX) {   // 小于10K则直接读取
		double highestPeak = 0.0;
		//cout << mIdx << endl;
		for (int i = 0; i < 2 * IPV_PEAKMAX; i += 2)
		{
			vTheoIso.push_back(m_vIPV[mIdx][i+1]);
			if(m_vIPV[mIdx][i+1] > highestPeak)
			{
				highestPeak = m_vIPV[mIdx][i+1];
				highestPeakPos = i / 2;
			}
		}
	} else {              // 大于等于10K则在线生成
		vector<CentroidPeaks> calIso;
		m_cEmass->Calculate(mass, calIso);
		double highestPeak = 0.0;
		for (size_t i = 0; i < calIso.size(); ++i)
		{
			vTheoIso.push_back(calIso[i].m_lfIntens);
			if(calIso[i].m_lfIntens > highestPeak)
			{
				highestPeak = calIso[i].m_lfIntens;
				highestPeakPos = i;
			}
		}
	}

	// 对齐理论谱中 IsoProfileNum (default 15)根峰的位置，使中间为最高峰
	if(highestPeakPos == HalfIsoWidth) // 左侧有7根峰
	{
		//恰好第HalfIsoWidth根峰为最高峰，若总的谱峰数目不够，在右侧补零
		if((int)vTheoIso.size() < IsoProfileNum)
		{
			vTheoIso.insert(vTheoIso.end(), IsoProfileNum - vTheoIso.size(), 0.0);
		} else if((int)vTheoIso.size() > IsoProfileNum) {
			vTheoIso.erase(vTheoIso.begin() + IsoProfileNum, vTheoIso.end());
		}
	} else if(highestPeakPos < HalfIsoWidth) { 
	    //最高峰左边的谱峰不够，补零
		vTheoIso.insert(vTheoIso.begin(), HalfIsoWidth - highestPeakPos, 0.0);
		if((int)vTheoIso.size() > IsoProfileNum)
		{   //删除最高峰右边多余的谱峰
			vTheoIso.erase(vTheoIso.begin() + IsoProfileNum, vTheoIso.end());
		} else if((int)vTheoIso.size() < IsoProfileNum){
			vTheoIso.insert(vTheoIso.end(), IsoProfileNum - vTheoIso.size(), 0.0);
		}
	} else if(highestPeakPos > HalfIsoWidth) {
		//最高峰左边的谱峰太多，删除较小的部分
		vTheoIso.erase(vTheoIso.begin(), vTheoIso.begin() + highestPeakPos - HalfIsoWidth);
		if((int)vTheoIso.size() < IsoProfileNum)
		{
			vTheoIso.insert(vTheoIso.end(), IsoProfileNum - vTheoIso.size(), 0.0);
		} else if((int)vTheoIso.size() > IsoProfileNum) {
			vTheoIso.erase(vTheoIso.begin() + IsoProfileNum, vTheoIso.end());
		}
	}
}

//从给定文件F:\Skydrive\coding\pParsesrc\IPV.txt中获取平均氨基酸模型的结果。并存放到全局变量IPV[10000][28]中。其中行代表质量的整数值，列代表同位素模式。没有归一化。
double MS1Reader::ComputIPVSimilarity(double &SumCurPrecursorInt, vector<double> &vTheoIso, double &IsoLenScore)
{
	// 从理论同位素峰簇来看，有些峰不存在，实验谱中计算强度和时可能有些不属于该同位素模式
	// [wrm]:由于在计算理论同位素分布时已经进行了补0对齐，故vTheoIso.size() <= HalfIsoWidth基本不存在
	if((int)vTheoIso.size() <= HalfIsoWidth || fabs(vTheoIso[HalfIsoWidth]) < eps)
	{
		return 0.0;
	}
	int isoLen = 0, valid = 0;
	for(int k = 0; k < IsoProfileNum; ++k)
	{    
		if(vTheoIso[k] / vTheoIso[HalfIsoWidth] < 0.05)  // [wrm？]:  0.05这个阈值是从何而来？ 前面的谱峰很有可能低于最高峰的5%啊
		{
			SumCurPrecursorInt -= IsoPattern[k];
			IsoPattern[k] = 0.0;
		} else {
			++valid;
			if(IsoPattern[k] > 0)
			{
				++isoLen;
			}
		}
	}
	IsoLenScore = 1.0 * isoLen / valid; // 与理论同位素峰簇匹配的同位素峰召回率

	
	vector<double> V2;
	double MaxSimilarity = -1;
	BestIsoSimiScanIndex = -1;
	for (int j = 0; j < 2*Span+1; ++j)
	{
		V2.resize(0);
		for(int k = 0; k < IsoProfileNum; ++k)
		{
			V2.push_back(IsoPattern[IsoProfileNum * j + k]);
		}
		// 计算两个向量的相关系数
		double VectorCovariance = CStatistic::CalculateVectorCov(vTheoIso, V2);
		if (VectorCovariance > MaxSimilarity)
		{
			MaxSimilarity = VectorCovariance;
			BestIsoSimiScanIndex = j;//这一处记录了最佳同位素模式相似度的地方
		}
	}
	return  MaxSimilarity;	
}

/*Compute the Eluted profile similarity, Likes some kind of convolution. every len-5 sub array involved.*/
int MS1Reader::GetIsotopeProfileLen(int &leftLen)
{
	int Len = 0;
	for (int i = 0; i < 2*Span+1; i++)
	{
		for (int j = IsoProfileNum - 1; j >=  HalfIsoWidth; --j)
		{
			if(IsoPattern[IsoProfileNum * i + j] > 0 )
			{
				if (j - HalfIsoWidth > Len)
				{
					Len = j - HalfIsoWidth;
				}
				break;
			} 
		}
	}
	leftLen = 0;
	for (int i = 0; i < 2*Span+1; i++)
	{
		for (int j = 0; j < HalfIsoWidth; ++j)
		{
			if(IsoPattern[IsoProfileNum*i+j] > 0 )
			{
				if (HalfIsoWidth - j > leftLen)
				{
					leftLen = HalfIsoWidth - j;
				}
				break;
			}
		} 
	}
	return Len;
}

// Get the Sum Intensity in Isolation Windows.
// 计算落入碎裂窗口内的候选谱峰的强度总和
double MS1Reader::GetPeakInIsolationWindow(vector<CentroidPeaks> &PrecursorCandidatePeaks,double &ActivationCenter,double &IsolationWidth)
{
	double SumPeakIntensityInIsolation = 0.0;
	double dist = ExtendHalfwindow + IsolationWidth / 2;
	for (size_t i = 0; i < PrecursorCandidatePeaks.size(); ++i)
	{
		if (PrecursorCandidatePeaks[i].m_lfMZ > ActivationCenter + dist)
		{
			continue;
		}
		else if (PrecursorCandidatePeaks[i].m_lfMZ < ActivationCenter - dist)
		{
			continue;
		} else {
			SumPeakIntensityInIsolation += PrecursorCandidatePeaks[i].m_lfIntens;
		}
	}
	return SumPeakIntensityInIsolation;
}


// 设置特征值相同的母离子其排名相同
void MS1Reader::CheckSameScore(vector<vector<double> > &PrecursorsCandidatesArray, int gSortByCol)
{
	PrecursorsCandidatesArray[0][gSortByCol+7] = 1;
	for (unsigned int i = 0; i < PrecursorsCandidatesArray.size()-1; i++)
	{
		if (fabs(PrecursorsCandidatesArray[i][gSortByCol] - PrecursorsCandidatesArray[i+1][gSortByCol]) < eps){
				PrecursorsCandidatesArray[i+1][gSortByCol+7] = PrecursorsCandidatesArray[i][gSortByCol+7];
		}
		else{
				PrecursorsCandidatesArray[i+1][gSortByCol+7] = PrecursorsCandidatesArray[i][gSortByCol+7]+1;
		}
	}
}

// 母离子排序粗打分
void MS1Reader::ScorePrecursors(vector<vector<double> > &PrecursorsCandidatesArray)
{
	// Changed by luolan @2014.05.05 /////////////////////////////////////////////////
	for(int gSortByCol = 2; gSortByCol < 8; ++gSortByCol) // TODO: 规范化常量
	{   // 所有打分都由高到低排序，这里不做区分，前面设计打分时需要注意
		CTools::gSortByCol = gSortByCol;
		sort(PrecursorsCandidatesArray.begin(),PrecursorsCandidatesArray.end(), CTools::compPrecursorByColDescend);
		//Remember the rank of the score
		for (int i = 0; i < (int)PrecursorsCandidatesArray.size(); ++i)
		{
			PrecursorsCandidatesArray[i][gSortByCol+7] = i+1;
		}
		CheckSameScore(PrecursorsCandidatesArray, gSortByCol);
	}
		//Remember the Score
		
		//获取最终打分
		for (unsigned int i = 0; i < PrecursorsCandidatesArray.size(); i++)
		{
			PrecursorsCandidatesArray[i][15] = 1;
			PrecursorsCandidatesArray[i][15] *= PrecursorsCandidatesArray[i][9];//maxSim
			//PrecursorsCandidatesArray[i][15] *= PrecursorsCandidatesArray[i][9];
			//PrecursorsCandidatesArray[i][15] *= PrecursorsCandidatesArray[i][9];
			//PrecursorsCandidatesArray[i][15] *= PrecursorsCandidatesArray[i][10];// peaksNum
			PrecursorsCandidatesArray[i][15] *= (PrecursorsCandidatesArray[i][11]); // 色谱相似度排名
			PrecursorsCandidatesArray[i][15] *= (PrecursorsCandidatesArray[i][12]);//stdDeviation
			//PrecursorsCandidatesArray[i][15] *= (PrecursorsCandidatesArray[i][12]);
			//PrecursorsCandidatesArray[i][15] *= (PrecursorsCandidatesArray[i][13]);  // chroSim[0]
			PrecursorsCandidatesArray[i][15] *= (PrecursorsCandidatesArray[i][14]);//pif
			//PrecursorsCandidatesArray[i][15] *= (PrecursorsCandidatesArray[i][14]);
			//PrecursorsCandidatesArray[i][15] *= (PrecursorsCandidatesArray[i][14]);
			
		}
		
		CTools::gSortByCol = 15;
		// 按最终排名从小到大排序  越小排名越靠前
		sort(PrecursorsCandidatesArray.begin(),PrecursorsCandidatesArray.end(), CTools::compPrecursorByCol);
}

// 去除相似的母离子： 合并mz在误差范围内的谱峰，强度叠加，mz取二者归一化结果（mz1*intens1 + mz2*intens2）/(intens1+intens2)
void MS1Reader::CutSamePrecursor(vector<CentroidPeaks> &PrecursorCandidatePeaks)
{
	if (0 == PrecursorCandidatePeaks.size())
	{
		return;
	}
	double tol=PrecursorCandidatePeaks[0].m_lfMZ*Peak_Tol_ppm;  // 离子误差
	vector<int> DeleteList;
	int LastIndex = 0;
	for (unsigned int i = 1; i < PrecursorCandidatePeaks.size(); ++i)
	{
		if(fabs(PrecursorCandidatePeaks[LastIndex].m_lfMZ - PrecursorCandidatePeaks[i].m_lfMZ) < tol)
		{
			//if (PrecursorCandidatePeaks[LastIndex].Intensity>PrecursorCandidatePeaks[i].Intensity)
			//{
				DeleteList.push_back(i);
				PrecursorCandidatePeaks[LastIndex].m_lfMZ=(PrecursorCandidatePeaks[LastIndex].m_lfMZ*PrecursorCandidatePeaks[LastIndex].m_lfIntens+PrecursorCandidatePeaks[i].m_lfIntens*PrecursorCandidatePeaks[i].m_lfMZ);
				
				PrecursorCandidatePeaks[LastIndex].m_lfIntens +=PrecursorCandidatePeaks[i].m_lfIntens;
			
				PrecursorCandidatePeaks[LastIndex].m_lfMZ/=PrecursorCandidatePeaks[LastIndex].m_lfIntens;
			//}
			/*else // Comment by luolan
			{
				DeleteList.push_back(LastIndex);
				PrecursorCandidatePeaks[i].MZ=(PrecursorCandidatePeaks[i].MZ*PrecursorCandidatePeaks[i].Intensity+PrecursorCandidatePeaks[LastIndex].Intensity*PrecursorCandidatePeaks[LastIndex].MZ);
				
				PrecursorCandidatePeaks[i].Intensity+=PrecursorCandidatePeaks[LastIndex].Intensity;
			
				PrecursorCandidatePeaks[i].MZ/=PrecursorCandidatePeaks[i].Intensity;
				LastIndex=i;
			}*/

		} else LastIndex=i; // Changed by luolan
	}
	if (DeleteList.size()==0)
	{
		return;
	}
	//sort(DeleteList.begin(),DeleteList.end()); // Comment by luolan
	
	for (int j = (int)DeleteList.size() - 1; j >= 0; --j)
	{
		PrecursorCandidatePeaks.erase(PrecursorCandidatePeaks.begin() + DeleteList[j]);
	}
}
void MS1Reader::Initialize()
{
	m_vPredictY.resize(0);
	MarsFeature.resize(0);
	OutputInfo.resize(0);
	m_vnIndexListToOutput.clear();
}
double MS1Reader::GetIsolationWidth()
{
	string tmp = m_para->GetValue("isolation_width", "10");
	return atof(tmp.c_str()); 
}



// 对给定的二级谱，找到其候选母离子，并计算所有的特征，进行粗打分保留前几名
// Called by CpParseFlow::FlowKernel
void MS1Reader::AssessPrecursor(double ActivationCenter,int MS2scan,int MS1scan,vector<CentroidPeaks> &PrecursorCandidatePeaks,TrainingSetreader &TrainSet)
{
	bool filter_noMixSpec = false;
	if (atoi((m_para->GetValue("co-elute")).c_str()) == 0)
	{
		filter_noMixSpec = true;  // 不导出混合谱
	}	
	int chargeStart = 3;
	// 按照mz对候选谱峰进行排序
	sort(PrecursorCandidatePeaks.begin(), PrecursorCandidatePeaks.end(), CTools::compLessMZ);

	// Cut similiar precursors
	if (m_para->GetValue("cut_similiar_mono") == "1")
	{
		CutSamePrecursor(PrecursorCandidatePeaks);   // 合并候选最高峰中邻近的谱峰
	}
	int lenVec = (int)PrecursorCandidatePeaks.size();
	
	// 对碎裂窗口（扩展0.5Da）内的所有谱峰按照mz进行排序，便于后续计算最高峰精度
	sort(PeakRecord.begin(), PeakRecord.end(), CTools::compLessMZ);
	
	vector<vector<double> > PrecursorsCandidatesArray;//存放给定二级谱的所有母离子
	vector<double> PrecursorsCandidatesItem;//存放母离子候选
	int PrecursorsCounter = 0;
	double IsolationWidth = GetIsolationWidth(); //程序里有一个Halfwindow（最大）限制，这里又去读参数文件，这里是实验参数，需要注意 -luolan
	//cout<<"isolation width: "<<IsolationWidth<<endl;
	// Get peak intensity sum in the isolation window for scale down other intensity measure.
	// Sum intensity recorded in the Isopattern Array from index 6 to 66.
	// 计算落入碎裂窗口(左右扩展0.5Da)内的候选谱峰的强度和
	double dIntensitySumInIsolationWindow = GetPeakInIsolationWindow(PrecursorCandidatePeaks,ActivationCenter,IsolationWidth);

	double prePeakMz = -1.0;
	double preScore = 0.0;
	bool isIncluded; // 依次考虑每根峰作为最高峰时，对于相同的同位素峰簇中的谱峰，只保留一根最好的
	const int chgEnd = atoi(m_para->GetValue("max_charge", "30").c_str());
	const double limitMass = atof(m_para->GetValue("max_mass", "50000").c_str());  // wrm: 母离子最大质量

	for (int k = 0; k < lenVec-1; k++) //枚举每根峰为最高峰的情况
	{
		//if(k == 212)
		//	cout<<"k: "<<k<<endl;
		double bestScore = 0.0;  // 粗打分
		double bestChg = 0.0;    // 母离子电荷
		double bestSim = 0.0;
		double bestSTD = 0.0;    // 最高峰精度，最高峰质荷比在相邻10张谱图中的标准差
		double bestChroSim1 = 0.0;
		double bestChroSim2 = 0.0;
		double bestTolScore = 0.0;
		double bestIsoPeakNum = 0.0; // 碎裂窗口内同位素峰数目
		double bestMonoRevInts = 0.0; // 单同位素峰相对强度差（理论与实验）
		double bestMonoMz = 0.0;  // 母离子质量
		double bestIsoLen = 0.0;  // 同位素模式长度，与理论同位素峰簇匹配的同位素峰数目
		double bestPIF = 0.0;     // 碎裂窗口内谱峰强度比

		// Initialize the PrecursorsCandidatesItem
		PrecursorsCandidatesItem.resize(0);

		//对于每一个候选最高峰M/Z枚举电荷，从3电荷开始
		for (int chg = chargeStart; chg <= chgEnd; chg++)
		{	
			InitIsoPattern(); // Initializing the Array IsoPattern[]

		    int IsoPeakInWindow = 0;  // 候选谱峰列表中的同位素峰数目
			double stdPPMTol = 0.0;
			double SumCurrentPrecursor = 0.0;	// 从获选谱峰列表中获取的同位素峰强度和
			isIncluded = false;

			// 根据最高峰和电荷获取同位素峰簇（共保存15根峰）
			GetEachIsoPeak(PrecursorCandidatePeaks, chg, k, IsoPeakInWindow, stdPPMTol, SumCurrentPrecursor, prePeakMz, isIncluded);
			//if(IsoLen / IsolationWidth < 0.5)
			//{   //若匹配上的谱峰还不足一半，则直接过滤掉该候选母离子
			//	continue;
			//}

			// 获取给定最高峰和电荷下的前后10张一级谱中的同位素模式 [wrm ?] 从获取候选峰时对谱峰分bin，到现在查询同位素时搜索合并bin，这其中对下标强制转为为int的处理会不会造成很大误差？
			// Fill in the Isopattern Array from the 10 related spectrum.
			GetAllIsoFeature(chg, ActivationCenter, PrecursorCandidatePeaks, k);
					
			// Isopattern Array index 15-165 is come from the peaks candidates array
			double sumIsopattern = GetSumIsoPattern();
			
			// [wrm ?]: 谱峰强度归一化，为什么是用10张谱中的强度和来归一化，而不是用每张谱中同位素峰簇强度和分别归一化呢
			// For each peak in the Isopattern Array with index 16 to 165. Devide by the sum intensity
			ScaledIsoPattern(sumIsopattern);
			
			//CutIsotopicPeak(IsoLen, SumCurrentPrecursor);// comment by luolan @2014.05.16
			
			// * 计算色谱相似度
			CalcChromSimilarity();
			int highestPeakPos = 0;
			vector<double> vTheoIso;
			//cout << PrecursorCandidatePeaks[k].m_lfMZ << endl;
			// 计算理论同位素分布
			GetTheoryIsoform(PrecursorCandidatePeaks[k].m_lfMZ*chg - chg*pmass, highestPeakPos, vTheoIso);
			if(0 == vTheoIso.size()) continue;
			// 根据理论同位素分布、最高峰位置计算Mono质量
			double monoMass = PrecursorCandidatePeaks[k].m_lfMZ - highestPeakPos * avgdiv / chg;
			if(monoMass > limitMass)
			{
				continue;
			}

			// * 添加特征单同位素峰相对强度差(MonoInts)，计算实验中mono峰的相对强度（相对于最高峰）与理论的相对强度之差
			int monoIdx = HalfIsoWidth - highestPeakPos;
			if(monoIdx < 0)  // 保存的15根实验谱峰中没有Mono峰，默认第一根为Mono峰
			{
				monoIdx = 0;
			}
			double expRelativeInts = 0.0;
			if(IsoPattern[HalfIsoWidth] > eps) // 最高峰
			{
				expRelativeInts = IsoPattern[monoIdx] / IsoPattern[HalfIsoWidth];  // 计算实验谱中Mono与最高峰的相对强度差
			}
			if(expRelativeInts > 1.0) expRelativeInts = 1.0;
			double theoRelativeInts = 0.0;
			if(vTheoIso[HalfIsoWidth] > eps)
			{
				theoRelativeInts = vTheoIso[monoIdx] / vTheoIso[HalfIsoWidth];   // 计算理论谱中Mono与最高峰的相对强度差
			}
			double tmpMonoDiv = 0.0;
			if(expRelativeInts > theoRelativeInts)
			{
				tmpMonoDiv = (expRelativeInts - theoRelativeInts) / expRelativeInts;
			} else {
				tmpMonoDiv = (theoRelativeInts - expRelativeInts) / theoRelativeInts;
			}
			//cout << monoMass << " " << chg << " " << expRelativeInts << " " << theoRelativeInts << " " << tmpMonoDiv << endl;

			
			// * Important Function 计算理论与实验同位素峰簇的相似度
			double IsoLenScore = 0.0;  // 与理论同位素峰簇匹配的同位素峰召回率 isoLen / theoValid
			// 候选谱峰列表中对应的同位素峰簇 + 10张一级谱中对应的同位素峰簇，保留与理论同位素分布相似度最大的值
			double MaxSim = ComputIPVSimilarity(SumCurrentPrecursor, vTheoIso, IsoLenScore);
			
			if(chg > 3 && IsoLenScore < 0.5)  // 如果匹配到的谱峰数目小于一半，则舍去该母离子
			{
				continue;
			}
			// * 添加特征(SimIso2)，一级谱中同一离子的不同电荷状态信息 2014.09.24
			double closeChgSim = CheckCloseChargeState(MS2scan, MS1scan, chg, monoMass, vTheoIso);

		
			//int RetentionTimeProfileLen = CalcEluteSimilairty();
				

			if (pTrainSet != NULL)
			{
				this->pTrainSet->CheckMonoList(MS2scan, monoMass, chg);
			}

			/*int IsoLeftLen = 0;
			int IsoLen = GetIsotopeProfileLen(IsoLeftLen);
			IsoLen += IsoLeftLen;*/

			//cout << k << " " << PrecursorCandidatePeaks[k].m_lfMZ << " " << chg << " " << IsoLenScore << endl;

			/*SumCurrentPrecursor = 0.0;		
			for (int Peak_i = 0; Peak_i < (int)PrecursorCandidatePeaks.size() ; Peak_i++)
			{
				if ( PrecursorCandidatePeaks[Peak_i].MZ > ActivationCenter - IsolationWidth/2 
					&& PrecursorCandidatePeaks[Peak_i].MZ < ActivationCenter + IsolationWidth/2 )
				{
					for (int IsoPattern_i = 0; IsoPattern_i < IsoLen; IsoPattern_i++)
					{
						if (fabs(PrecursorCandidatePeaks[k].MZ + IsoPattern_i*avgdiv/chg - PrecursorCandidatePeaks[Peak_i].MZ) < ActivationCenter * Peak_Tol_ppm)
						{
							if(k == 45 && (chg == 8||chg == 16))
								cout<<Peak_i<<": "<<PrecursorCandidatePeaks[Peak_i].Intensity<<endl;
							SumCurrentPrecursor += PrecursorCandidatePeaks[Peak_i].Intensity;
						}
					}

					for (int IsoPattern_i = 1; IsoPattern_i < IsoLeftLen; IsoPattern_i++)
					{
						if (fabs(PrecursorCandidatePeaks[k].MZ - IsoPattern_i*avgdiv/chg - PrecursorCandidatePeaks[Peak_i].MZ) < ActivationCenter*Peak_Tol_ppm)
						{
							if(k == 45 && (chg == 8||chg == 16))
								cout<<Peak_i<<": "<<PrecursorCandidatePeaks[Peak_i].Intensity<<endl;
							SumCurrentPrecursor += PrecursorCandidatePeaks[Peak_i].Intensity;
						}
					}				
				}
				else
					continue;
			}*/
			//cout<< "SumCurrentPrecursor "<<SumCurrentPrecursor<<endl;
			double PIF = 0.0;   // 碎裂窗口内谱峰强度比
			if (dIntensitySumInIsolationWindow != 0 )  // 与理论谱匹配的同位素峰簇和/碎裂窗口内候选谱峰强度和
			{
				PIF = SumCurrentPrecursor / dIntensitySumInIsolationWindow;
				if(PIF > 1.0) PIF = 1.0;
				//PIF /= (0.1 + IsoPeakInWindow);
			}


			// TODO 以下注释部分为一些很散的过滤条件，后期需要根据TD的特点再定夺如何进行快速过滤，最好抽象成一个函数 -luolan
			
			if (PIF<0.01)
			{
				continue;
			}

			if (chg > 5 && IsoPeakInWindow < 3)
			{
				continue;
			}
	
			/*double IsolationExtend = 0.5;
			if (PrecursorCandidatePeaks[k].m_lfMZ > ActivationCenter + IsolationWidth / 2 + IsolationExtend)
			{
				continue;
			}*/

			/*if (IsoPeakInWindow < 2 && (chg != 2 || PrecursorCandidatePeaks[k].m_lfMZ > ActivationCenter+IsolationWidth/4))
			{
				continue;
			}*/

			if ( MaxSim < 0.3)
			{
				continue;
			}
			if(chg < 4 && MaxSim < 0.8) // 限制低电荷
			{
				continue;
			}
			if(chg > 20 && MaxSim < 0.6) // 限制高电荷
			{
				continue;
			}
			
			/*if (ChromSimilarity[0]>0.99)
			{
				continue;
			}
			
			if (ChromSimilarity[1]<0.5)
			{
				continue;
			}
			if (STD>3 && STD!=200)
			{
				continue;
			}*/
			double chromMaxSim = ChromSimilarity[1] >= ChromSimilarity[0] ? ChromSimilarity[1] : ChromSimilarity[0];
			// 用碎裂窗口内谱峰强度比、同位素模式相似度和谱峰精度来进行粗打分 stdPPMTol为绝对偏差和，故 > 0
			double tmpScore = PIF + MaxSim + (20.0 - stdPPMTol) / 20.0;  
#ifdef _DEBUG_TEST2
			if(MS2scan == 2732)
			{
				cout<<highestPeakPos<<" "<<PrecursorCandidatePeaks[k].m_lfMZ<<" "<<chg<<" "<<IsoPeakInWindow<<" "<<PIF<<" "<<MaxSim<<" "<<chromMaxSim<<" "<<tmpScore<<endl;
			}
#endif
			if(tmpScore > bestScore) // 记录最高分的特征值，保留打分最高的电荷特征
			{
				bestScore = tmpScore;    // 粗打分
				bestChg = (double)chg;   // * 电荷
				bestSim = MaxSim;        // * 同位素模式相似度
				bestSTD = CalcSTDforMZ( PrecursorCandidatePeaks[k].m_lfMZ );  // * 最高峰精度
				bestChroSim1 = chromMaxSim;  // * 色谱相似度
				bestChroSim2 = closeChgSim;  // * 相邻电荷状态同位素模式相似度
				bestTolScore = 20.0 - stdPPMTol;  // * 同位素模式偏差
				bestIsoPeakNum = IsoPeakInWindow; // * 同位素峰数目
				bestMonoRevInts = 1 - tmpMonoDiv; // test 2014.10.08  * 单同位素峰相对强度差
				bestIsoLen = IsoLenScore;    // * 同位素模式长度分值
				bestPIF = PIF;   // * 碎裂窗口内谱峰强度比

				double AccMZ = ReCalcMZ( PrecursorCandidatePeaks[k].m_lfMZ );  // 最高峰质量校准
				//根据理论同位素峰的最高峰位置推算实验同位素峰簇的mono峰位置
				bestMonoMz = AccMZ - highestPeakPos * avgdiv / chg;  // * Mono峰的估计质量
			}
		}//for charge

		if(bestScore < eps)// bestScore == 0.0
		{
			continue;
		}

		if(isIncluded && bestScore <= preScore)
		{
			continue;
		} else if(isIncluded){  // 若该谱峰已使用过，则保留打分最高的那个
			prePeakMz = PrecursorCandidatePeaks[k].m_lfMZ;
			preScore = bestScore;
			int last = (int)MarsFeature.size() - 1;
			if(last < 0)
			{
				continue;
			}
			// 所有的特征都归一化
			MarsFeature[last][0] = (bestChg * 1.0) / chgEnd;
			MarsFeature[last][1] = bestSim;
			MarsFeature[last][2] = bestSTD / 20.0; // CHECK
			MarsFeature[last][3] = bestChroSim1;
			MarsFeature[last][4] = bestTolScore / 20.0; // CHECK
			MarsFeature[last][5] = bestChroSim2;
			MarsFeature[last][6] = (bestIsoPeakNum * 1.0) / IsoProfileNum;
			MarsFeature[last][7] = bestMonoRevInts;
			MarsFeature[last][8] = bestMonoMz / 2000.0; // CHECK
			MarsFeature[last][9] = bestIsoLen;
			MarsFeature[last][10] = bestPIF;
			MarsFeature[last][11] = bestScore;

			//MarsFeature[last][0] = (bestChg * 1.0) / chgEnd;
			//MarsFeature[last][1] = bestSim;
			//MarsFeature[last][2] = bestSTD / 20.0; // CHECK
			////MarsFeature[last][3] = bestChroSim1;
			//MarsFeature[last][3] = bestTolScore / 20.0; // CHECK
			//MarsFeature[last][4] = bestChroSim2;
			//MarsFeature[last][5] = (bestIsoPeakNum * 1.0) / IsoProfileNum;
			////MarsFeature[last][7] = bestMonoRevInts;
			//MarsFeature[last][6] = bestMonoMz / 2000.0; // CHECK
			//MarsFeature[last][7] = bestIsoLen;
			//MarsFeature[last][8] = bestPIF;
			//MarsFeature[last][9] = bestScore;
			
			last = (int)OutputInfo.size() - 1;
			OutputInfo[last][0] = (double)MS2scan;
			OutputInfo[last][1] = (double)bestChg;
			OutputInfo[last][2] = (double)bestMonoMz;
			
			last = (int)PrecursorsCandidatesArray.size() - 1;
			PrecursorsCandidatesArray[last][0] = bestMonoMz;//0
			PrecursorsCandidatesArray[last][1] = bestChg;//1
			PrecursorsCandidatesArray[last][2] = bestSim;//2
			PrecursorsCandidatesArray[last][3] = (bestIsoPeakNum * 1.0) / IsoProfileNum;//3
			PrecursorsCandidatesArray[last][4] = bestChroSim1;//4
			PrecursorsCandidatesArray[last][5] = bestTolScore;//5  //将实验同位素峰簇的偏差作为一维特征，主要用于区分相邻的高电荷，打分越高越好 -luolan
			PrecursorsCandidatesArray[last][6] = bestChroSim2;//6
			PrecursorsCandidatesArray[last][7] = bestPIF;//7
		} else {  // 
			prePeakMz = PrecursorCandidatePeaks[k].m_lfMZ;
			preScore = bestScore;
			vector<double> MarsFeatureTmp;
			vector<double> OutputInfoTmp;
			MarsFeatureTmp.push_back((bestChg * 1.0) / chgEnd);  // 电荷
			MarsFeatureTmp.push_back(bestSim);         // 同位素模式相似度
			MarsFeatureTmp.push_back(bestSTD / 20.0);  // 最高峰精度
			MarsFeatureTmp.push_back(bestChroSim1);    // 色谱相似度
			MarsFeatureTmp.push_back(bestTolScore / 20.0);  // 同位素模式偏差
			MarsFeatureTmp.push_back(bestChroSim2);         // 相邻电荷状态的同位素峰簇相似度
			MarsFeatureTmp.push_back((bestIsoPeakNum * 1.0) / IsoProfileNum); // 同位素峰数目
			MarsFeatureTmp.push_back(bestMonoRevInts);    // 单同位素峰相对强度差
			MarsFeatureTmp.push_back(bestMonoMz / 2000.0);  // 母离子质量
			MarsFeatureTmp.push_back(bestIsoLen);  // 同位素模式长度打分
			MarsFeatureTmp.push_back(bestPIF);     // 碎裂窗口内谱峰强度比
			MarsFeatureTmp.push_back(bestScore);   // 粗打分值

			//MarsFeatureTmp.push_back((bestChg * 1.0) / chgEnd);
			//MarsFeatureTmp.push_back(bestSim);
			//MarsFeatureTmp.push_back(bestSTD / 20.0);
			////MarsFeatureTmp.push_back(bestChroSim1);
			//MarsFeatureTmp.push_back(bestTolScore / 20.0);
			//MarsFeatureTmp.push_back(bestChroSim2);
			//MarsFeatureTmp.push_back((bestIsoPeakNum * 1.0) / IsoProfileNum);
			////MarsFeatureTmp.push_back(bestMonoRevInts);
			//MarsFeatureTmp.push_back(bestMonoMz / 2000.0);
			//MarsFeatureTmp.push_back(bestIsoLen);
			//MarsFeatureTmp.push_back(bestPIF);
			//MarsFeatureTmp.push_back(bestScore);
			
			MarsFeature.push_back(MarsFeatureTmp);

			OutputInfoTmp.push_back((double)MS2scan);
			OutputInfoTmp.push_back((double)bestChg);
			OutputInfoTmp.push_back((double)bestMonoMz);
			OutputInfo.push_back(OutputInfoTmp);
			
			// Record a precursor
			PrecursorsCounter++;
			PrecursorsCandidatesItem.push_back(bestMonoMz);//0
			PrecursorsCandidatesItem.push_back(bestChg);//1
			PrecursorsCandidatesItem.push_back(bestSim);//2
			PrecursorsCandidatesItem.push_back(bestSTD);//3
			//PrecursorsCandidatesItem.push_back(EluteSim);//(ChromSimilarity[1]);
			PrecursorsCandidatesItem.push_back(bestChroSim1);//4
			//PrecursorsCandidatesItem.push_back(newsumIsopattern);//5
			PrecursorsCandidatesItem.push_back(bestTolScore);//5 
			//将实验同位素峰簇的偏差作为一维特征，主要用于区分相邻的高电荷，打分越高越好 -luolan
			PrecursorsCandidatesItem.push_back(bestChroSim2);//6【引入虚拟谱峰】
			PrecursorsCandidatesItem.push_back(bestPIF);//7
			PrecursorsCandidatesItem.push_back(PrecursorsCounter);//8
			PrecursorsCandidatesItem.push_back(0);//MaxSimScore 9
			PrecursorsCandidatesItem.push_back(0);//STDScore 10
			PrecursorsCandidatesItem.push_back(0);//EluteScore 11
			PrecursorsCandidatesItem.push_back(0);//intenstiyScore 12
			PrecursorsCandidatesItem.push_back(0);//Elute Score Isotope 0 13
			PrecursorsCandidatesItem.push_back(0);//PIF 14
			PrecursorsCandidatesItem.push_back(0);//FinalScore 15
			//成功记录了一个母离子。
					
			//bCorrectSample=TrainSet.CheckScanMZCharge(MS2scan,PrecursorCandidatePeaks[k].MZ,chg);
				
			PrecursorsCandidatesArray.push_back(PrecursorsCandidatesItem);
		}

	}//for peak


	// Not any precursor candidates.
	if (PrecursorsCandidatesArray.size()==0)
	{
			return;
	}
	// Ranking Score, Then keep top N precursors. 对母离子进行排序粗打分
	ScorePrecursors(PrecursorsCandidatesArray);
	int OutputTopN = 10;
	if(filter_noMixSpec)  // 不导出混合谱
	{
		OutputTopN = 1;
	}
	
	//PrecursorSumAll += OutputTopN > (int)PrecursorsCandidatesArray.size() ? (int)PrecursorsCandidatesArray.size() : OutputTopN;

#ifdef _DEBUG_TEST
	cout<<endl;
	for(int k = 0; k < OutputTopN; ++k)
	{
		if (k >= (int)PrecursorsCandidatesArray.size())
		{
			break;
		}
		cout<<k<<": MZ=" <<PrecursorsCandidatesArray[k][0]
			<<" Charge=" <<int(PrecursorsCandidatesArray[k][1])
			<< " Mass=" <<(PrecursorsCandidatesArray[k][0] - pmass)*PrecursorsCandidatesArray[k][1]
			<<  " maxSim=" << PrecursorsCandidatesArray[k][2]
			<<  " TolPPM=" << PrecursorsCandidatesArray[k][5]
			<<  " PIF=" << PrecursorsCandidatesArray[k][7]
			<< " ChromSim1=" << PrecursorsCandidatesArray[k][4]
			<< " ChromSim2=" << PrecursorsCandidatesArray[k][6]
			<<  " Score=" << PrecursorsCandidatesArray[k][15]<<endl <<endl;;
	}
#endif

	if ((int)PrecursorsCandidatesArray.size() > OutputTopN)  // 预先淘汰不正确的母离子
	{
		PrecursorsCandidatesArray.erase(PrecursorsCandidatesArray.begin() + OutputTopN, PrecursorsCandidatesArray.end());	
		//PrecursorSumAll += OutputTopN > (int)PrecursorsCandidatesArray.size() ? (int)PrecursorsCandidatesArray.size() : OutputTopN;
	}
	
	//if (PrecursorsCounter <= OutputTopN - 1) // comment by luolan 2014.09.16，当导出的母离子少于指定数目时，outputInfo和MarseFeature里面的候选母离子顺序与上面粗打分排序后的不一致
	//{
	//	return;
	//}
	//cout<<"MarsFeature.size "<<MarsFeature.size()<<endl;
	// MarsFeature中存放了所有二级谱的母离子，PrecursorsCandidatesArray存放了当前二级谱的所有母离子
	int DeleteStart = MarsFeature.size() - PrecursorsCounter;  
	
	int MarsLen = MarsFeature.size();
	int outputLen = OutputInfo.size();
	
	// MarsFeature.size() - PrecursorsCounter到MarsFeature当前最后，为本张MS2的母离子，先将这部分中打分高的挑出来push进去，然后将前面这部分全部删掉
	for (unsigned int Precursor_i = 0; Precursor_i < PrecursorsCandidatesArray.size(); Precursor_i++)
	{
		MarsFeature.push_back( MarsFeature[MarsLen-PrecursorsCounter+(int)PrecursorsCandidatesArray[Precursor_i][8]-1]);
		OutputInfo.push_back(OutputInfo[outputLen-PrecursorsCounter+(int)PrecursorsCandidatesArray[Precursor_i][8]-1]);
	}
	
	MarsFeature.erase(MarsFeature.begin()+DeleteStart,MarsFeature.begin()+DeleteStart+PrecursorsCounter);
	
	OutputInfo.erase(OutputInfo.begin()+DeleteStart,OutputInfo.begin()+DeleteStart+PrecursorsCounter);

	//cout<<OutputInfo[0][1]<<" "<<OutputInfo[0][2]<<endl;
}

// 检查同一母离子相邻电荷状态的同位素峰簇
// param: 二级谱scan,当前母离子电荷,当前电荷下的Mono质量,理论同位素分布
double MS1Reader::CheckCloseChargeState(int scanNo, int precursorScan, int curChg, double curMz, vector<double> &vTheoIso)
{
	// 先取得候选母离子相邻电荷状态的谱峰信息
	vector<CentroidPeaks> vPeaksTmp; 
	double mass = curMz * curChg;   // 母离子质量
	double closeMz = (mass + pmass) / (curChg + 1);  // 带curChg+1电荷时的母离子mz

	int ms1Index = -1;
	// 已经存储了每张二级谱对应的一级谱 modified by wrm. 2015.10.10
	// GetIndex_By_MS2Scan(ms1Index, scanNo);  // [TODO] [wrm]: 为每张二级谱提前存储其对应的一级谱，则无需每次查找
	ms1Index = MS1Scan_Index[precursorScan];


	if(-1 == ms1Index)
	{
		return 0.0;
	}

	GetMS1PeakbyMass(closeMz, curChg + 1, ms1Index, vPeaksTmp);

	vector<double> vCloseIso(vTheoIso.size(), 0);

	int highest = -1;  // 记录最高峰的位置
	double highesPeak = 0;
	for(size_t i = 0; i < vPeaksTmp.size(); ++i)
	{
		if(vPeaksTmp[i].m_lfIntens > highesPeak)
		{
			highesPeak = vPeaksTmp[i].m_lfIntens;
			highest = i;
		}
	}
	int idx = HalfIsoWidth; // 以最高峰为基准对其实验谱峰
	for(int j = highest; j >= 0; --j)
	{
		vCloseIso[idx--] = vPeaksTmp[j].m_lfIntens;
		if(idx < 0) break;
	}
	idx = HalfIsoWidth + 1; // 8,9,10,11,12,13,14
	for(int j = highest + 1; j < (int)vPeaksTmp.size(); ++j)
	{
		vCloseIso[idx++] = vPeaksTmp[j].m_lfIntens;
		if(idx >= IsoProfileNum) break;
	}
	// 计算与理论谱峰的相似度
	double cov1 = CStatistic::CalculateVectorCov(vTheoIso, vCloseIso);

	vector<CentroidPeaks> emptyVctCPeak;
	vPeaksTmp.swap( emptyVctCPeak );
	vector<double> tmpVct(vTheoIso.size(), 0);
	vCloseIso.swap( tmpVct );
	closeMz = (mass - pmass) / (curChg - 1);  // 带curChg-1电荷时的母离子mz. [wrm] from + to - modified by wrm 20150929.  

	GetMS1PeakbyMass(closeMz, curChg - 1, ms1Index, vPeaksTmp);


	highest = -1; // 记录最高峰的位置
	highesPeak = 0;
	for(size_t i = 0; i < vPeaksTmp.size(); ++i)
	{
		if(vPeaksTmp[i].m_lfIntens > highesPeak)
		{
			highesPeak = vPeaksTmp[i].m_lfIntens;
			highest = i;
		}
	}
	idx = HalfIsoWidth; // 以最高峰为基准对其实验谱峰
	for(int j = highest; j >= 0; --j)
	{
		vCloseIso[idx--] = vPeaksTmp[j].m_lfIntens;
		if(idx < 0) break;
	}
	idx = HalfIsoWidth + 1;
	for(int j = highest + 1; j < (int)vPeaksTmp.size(); ++j)
	{
		vCloseIso[idx++] = vPeaksTmp[j].m_lfIntens;
		if(idx >= IsoProfileNum) break;
	}
	// 计算与理论谱峰的相似度
	double cov2 = CStatistic::CalculateVectorCov(vTheoIso, vCloseIso);
	// 保留相邻两个电荷状态中与理论谱的最大相似度
	double maxSim = cov1 >= cov2 ? cov1 : cov2;
	return maxSim;
}

// 给定母离子mz、电荷、MS1scan获取同位素峰簇
void MS1Reader::GetMS1PeakbyMass(double accurateMz, int chg, int ms1Index, vector<CentroidPeaks> &vPeaksTmp)
{
	try
	{
		MS1List[ms1Index]->GetPeaksbyMass(accurateMz, chg, vPeaksTmp, Peak_Tol_ppm);
	}
	catch(...)
	{
		cout << "Something wrong in the function GetMS1PeakbyMass()" << endl;
	}
}

//// Mars Model predict calculation
//void MS1Reader::SVMPredict()
//{
//	if (MarsFeature.size()==0)
//	{
//		return;
//	}
//	//m_vPredictY.assign(MarsFeature.size(), 1);
//
//	CClassify classify(m_para->GetValue("outputpath"));
//	classify.SVMPrepareData(MarsFeature, OutputInfo,  pTrainSet);
//	classify.SVMTraining();
//	classify.SVMPredict(m_vPredictY);
//}
//
//
///* Mars Model predict calculation */
//void MS1Reader::MarsPredict()
//{
//	//CStatistic stat; //CHECK luolan
//	//stat.Normalization(MarsFeature); // This function is in the com.h file
//	CClassify classify(m_para->GetValue("outputpath"));
//	classify.MARSPredict(MarsFeature, m_vPredictY);
//}

/**
*	Fill in the outputinfo vector with threshold, it decides  which precursor to be exported
*/ 
void MS1Reader::OutputMarsPredictY()
{
	// Output the predictY into a file.
	if (m_vPredictY.size() == 0)               // If there are no PredictY, Just Return!                                               
	{
		return;
	}
	if (OutputInfo.size() == 0)
	{
		cout << "Empty outputinfo" << endl;
		return;
	}

	vector<vector<double> > exportInfo;      // To delete items from another vector, this vector record the indexes!                                                               
	
	double MarsThreshold = atof(m_para->GetValue("mars_threshold").c_str());
		
	for (unsigned int i = 0; i < m_vPredictY.size() && i < OutputInfo.size(); i++)
	{
		if (m_vPredictY[i] > MarsThreshold)
		{
			exportInfo.push_back(OutputInfo[i]);	
		}	
	}
	OutputInfo.swap(exportInfo);
}
// 机器学习法对母离子进行分类
void MS1Reader::MacLearnClassify(string modelType, bool newModel)
{
	if (MarsFeature.size() == 0)
	{
		return;
	}
	//string outPath = m_para->GetValue("outputpath");
	//string timestamp = m_para->GetTimeStr();
	//outPath += timestamp;
	//CClassify classify(outPath);
	if(modelType.compare("MARS") == 0)
	{
		classify->MARSPredict(MarsFeature, m_vPredictY);
	} else {
		int sampNum = classify->SVMPrepareData(MarsFeature, OutputInfo,  pTrainSet);
		cout << "Sample Number: " << sampNum << endl;
		if(sampNum > MIN_SAMPLE_NUM)
		{
			if(newModel){
				cout << "SVM online training ..." << endl;
				classify->SVMTraining();
			}
			classify->SVMPredict(m_vPredictY);
		} else {
			cout << "Predict with MARS model ..." << endl;
			classify->MARSPredict(MarsFeature, m_vPredictY);
		}
	}
	OutputMarsPredictY();
}

// 对于给定的谱峰进行质量校准。方法是PeakRecord实现记录了大量的谱峰，用时从中取出和母离子质量有关的部分，按照Intensity加权平均进行处理。
// [wrm?]:有必要对最高峰质量进行校准吗？ 
double  MS1Reader::ReCalcMZ(double oldMZ)
{
	double RecalibrateWindow = atof(m_para->GetValue("recalibrate_window").c_str()) ;  // ppm
	double tol = oldMZ * RecalibrateWindow * 1e-6;
	double LeftLimit = oldMZ - tol;
	double RightLimit = oldMZ + tol;
	double ProductSum = 0;
	double IntensitySum = 0;
	for (unsigned int i = 0; i < PeakRecord.size(); i++)
	{
		if (PeakRecord[i].m_lfMZ < LeftLimit)
		{
			continue;
		}
		else if (PeakRecord[i].m_lfMZ > RightLimit)
		{
			break;
		}
		else
		{
			ProductSum += PeakRecord[i].m_lfMZ * PeakRecord[i].m_lfIntens;
			IntensitySum += PeakRecord[i].m_lfIntens;
		}
	}
	double newMZ = oldMZ;
	if (IntensitySum != 0)
	{
		newMZ = ProductSum / IntensitySum;
	} 
	
	return newMZ;
}
//进一步，考虑如何计算给定谱峰的方差。理论上母离子的精度更高。这一点没有在别的文献中看到过。但是我说出来，相信大家都不会反对。
//事实上这里计算的并不是STD，返回的时候，稍微做了一点变化，变成了一个类似于误差区间的东西，单位是ppm.
// 根据最高峰的mz计算其在相邻10张谱图中的标准差
double  MS1Reader::CalcSTDforMZ(double oldMZ)
{
	double tol = oldMZ * Peak_Tol_ppm;
	double LeftLimit = oldMZ - tol;
	double RightLimit = oldMZ + tol;
	double ProductSum = 0;
	double IntensitySum = 0;
	double SumXX = 0;
	double FindPeakNum = 0;
	// PeakRecord中记录了相邻10张谱图中碎裂窗口内的所有谱峰
	for (unsigned int i = 0; i < PeakRecord.size(); i++)
	{
		if (PeakRecord[i].m_lfMZ < LeftLimit)
		{
			continue;
		} else if (PeakRecord[i].m_lfMZ > RightLimit) {
			break;
		} else {
			FindPeakNum++;
			SumXX += PeakRecord[i].m_lfMZ * PeakRecord[i].m_lfMZ * PeakRecord[i].m_lfIntens;
			ProductSum += PeakRecord[i].m_lfMZ * PeakRecord[i].m_lfIntens;
			IntensitySum += PeakRecord[i].m_lfIntens;
		}
	}
	double STD = 0;
	double MeanX = oldMZ;
	if (abs(IntensitySum) >= eps)
	{
		MeanX = ProductSum / IntensitySum;
		double A = SumXX / IntensitySum;
		STD = sqrt(abs(A - MeanX * MeanX)) / oldMZ * 1e6;
	} 
	return STD;
}

// If Local minimal not exist,  use largest window.
// else, if the best simiscan is before the localminimal, use the forward, else use backward.
void MS1Reader::FindStartEndofEluteProfile(int &Start, int &End, const vector<int> &LocalMinimal, const int &BestIsoSimiScanIndex)
{
	Start = 0;
	End = 2 * Span - 1;
	if (0 == LocalMinimal.size())
	{
		return;
	}
	/*for (unsigned int i = 0; i < LocalMinimal.size(); i++)
	{
		if (BestIsoSimiScanIndex >= LocalMinimal[i])
		{
			Start = LocalMinimal[i];
		}
		else
		{
			End = LocalMinimal[i];
			return;
		}
	}*/
	int mid = 0;
	int idx = 0;
	while(idx < (int)LocalMinimal.size())
	{
		if (LocalMinimal[idx] < Span)
		{
			++idx;
		} else {
			mid = idx - 1;
			if(++idx < (int)LocalMinimal.size())
			{
				End = LocalMinimal[idx];
			}
			break;
		}
	}
	if(mid > 0)
	{
		Start = LocalMinimal[mid-1];
	} 
}
//但是仔细思考，感觉也许可以不用它配合，直接计算包含了中心的MS1Scan的那个Scan区间内进行计算，也许更好。或者可以直接凑活用现有的这个相似度计算工具。
int MS1Reader::CalcEluteSimilairty() 
{
	vector<double> MonoElute, Isotope1Elute;
	//cout<<"highest peak: "<<IsoPattern[HalfIsoWidth]<<endl;
	for (int j = 1; j <= 2*Span; j++)
	{
		// Get Lc-profile of highest peak
		MonoElute.push_back(IsoPattern[HalfIsoWidth + j*IsoProfileNum]);
		// Get Lc-profile of the next isotopic peak of highest.
		Isotope1Elute.push_back(IsoPattern[HalfIsoWidth+1+j*IsoProfileNum]);

	}
	
	//Calculate the LocalMinimal of the Lc-profile.
	vector<int> MonoLocalMinimal, Isotope1LocalMinimal;
	CStatistic::FindLocalMinimal(MonoLocalMinimal, MonoElute);
	CStatistic::FindLocalMinimal(Isotope1LocalMinimal, Isotope1Elute);

	//cout << "切割向量" <<endl;	
	// cut the best part of Lc-profile by local minimal
	int Monostart = 0, Monoend = 2*Span;
	FindStartEndofEluteProfile(Monostart, Monoend, MonoLocalMinimal, BestIsoSimiScanIndex);
	//cout<<"mono:"<<Monostart<<" "<< Monoend<<endl;
	int Isostart = 0, Isoend = 2*Span;
	FindStartEndofEluteProfile(Isostart, Isoend, Isotope1LocalMinimal, BestIsoSimiScanIndex);

	int len1 = Monoend - Monostart;
	int len2 = Isoend - Isostart;
	if(len2 < len1) len1 = len2;

	return len1;
}

double MS1Reader::GetBaselineByIndex(int MS1Index)
{
	return MS1List[MS1Index]->GetBaseline();
}

void MS1Reader::CutIsotopicPeak(int &IsoLen, double &sumCurPrecursorInt)
{
	//对于同位素模式，按照强度的局部极小点进行截断，解决同位素模式重叠问题。。。
	//在TD上，从最高峰向两端寻找，找到局部最小值之后分别将两端截断，考虑可能存在漏峰时，每端能够容忍一个局部最小值
	vector<int> LocalMinimal;
	vector<double> Data(IsoPattern);//,IsoPattern+sizeof(IsoPattern)/sizeof(double));
	bool hasMin = CStatistic::FindLocalMinimal(LocalMinimal, Data);
	if(!hasMin)
	{
		return;
	}
	int StartIsotopeIndex = 0;
	int EndIsotopeIndex = IsoProfileNum - 1;

	// 先扫描最高峰右侧的极小值点
	int unmatched = 0;
	for (int i = 0; i < (int)LocalMinimal.size(); ++i)
	{
		if (LocalMinimal[i] <= StartIsotopeIndex + HalfIsoWidth)
		{
			continue;
		} else if (LocalMinimal[i] >= EndIsotopeIndex) {
			if(0 == StartIsotopeIndex)
			{
				IsoLen = HalfIsoWidth + 1;
			}
			StartIsotopeIndex += IsoProfileNum;
			EndIsotopeIndex += IsoProfileNum;
			unmatched = 0;
		} else if(++unmatched < unmatchedNum){
			continue;
		} else {
			for (int j = LocalMinimal[i]+1; j <= EndIsotopeIndex ; ++j)
			{
				sumCurPrecursorInt -= IsoPattern[j];
				IsoPattern[j] = 0.0;
			}
			int cutoffpos = LocalMinimal[i];
			bool cutoff = false;
			for(int k = HalfIsoWidth + 1; k < IsoProfileNum; ++k)
			{   // 除了截断最小值外，还截断那些比最高峰高出一倍的谱峰
				if(IsoPattern[StartIsotopeIndex + k] > 2 * IsoPattern[StartIsotopeIndex + HalfIsoWidth])
				{
					cutoff = true;
					cutoffpos = StartIsotopeIndex + k;
				}
				if(cutoff)
				{
					sumCurPrecursorInt -= IsoPattern[StartIsotopeIndex + k];
					IsoPattern[StartIsotopeIndex + k] = 0.0;
				}
			}
			if(0 == StartIsotopeIndex)
			{
				IsoLen = cutoffpos - (StartIsotopeIndex + HalfIsoWidth) + 1;
			}
			StartIsotopeIndex += IsoProfileNum;
			EndIsotopeIndex += IsoProfileNum;
			unmatched = 0;
		}
	}

	// 再扫描最高峰左侧的极小值点
	unmatched = 0;
	for (int i = (int)LocalMinimal.size() - 1; i >= 0; --i)
	{
		if (LocalMinimal[i] >= StartIsotopeIndex + HalfIsoWidth)
		{
			continue;
		} else if (LocalMinimal[i] <= StartIsotopeIndex) {
			if(0 == StartIsotopeIndex)
			{
				IsoLen += HalfIsoWidth;
			}
			StartIsotopeIndex -= IsoProfileNum;
			EndIsotopeIndex -= IsoProfileNum;
			unmatched = 0;
		} else if(++unmatched < unmatchedNum){
			continue;
		} else {
			for (int j = LocalMinimal[i]-1; j >= StartIsotopeIndex ; --j)
			{
				sumCurPrecursorInt -= IsoPattern[j];
				IsoPattern[j] = 0.0;
			}
			bool cutoff = false;
			int cutoffpos = LocalMinimal[i];
			for(int k = HalfIsoWidth - 1; k >= 0; --k)
			{   // 除了截断最小值外，还截断那些比最高峰高出一倍的谱峰
				if(IsoPattern[StartIsotopeIndex + k] > 1.2 * IsoPattern[StartIsotopeIndex + HalfIsoWidth])
				{
					cutoff = true;
					cutoffpos = StartIsotopeIndex + k;
				}
				if(cutoff)
				{
					sumCurPrecursorInt -= IsoPattern[StartIsotopeIndex + k];
					IsoPattern[StartIsotopeIndex + k] = 0.0;
				}
			}
			if(0 == StartIsotopeIndex)
			{
				IsoLen += (StartIsotopeIndex + HalfIsoWidth) - cutoffpos;
			}
			StartIsotopeIndex -= IsoProfileNum;
			if(StartIsotopeIndex < 0)
			{
				break;
			}
			EndIsotopeIndex -= IsoProfileNum;
			unmatched = 0;
		}
	}
}
