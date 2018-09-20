/********************************************************************
	created:	2012/04/11
	created:	11:4:2012   9:08
	author:		Wu Long
	
	purpose:	
*********************************************************************/

#include <vector>
#include <iomanip>
#include <iostream>
#include <cmath>
#include <cstdio>
#include <algorithm>

#include "MS2Spectrum.h"
#include "Parameters.h"
#include "com.h"

using namespace std;
//#define _DEBUG_MGF

#ifndef _CentroidPeaksComp_
#define _CentroidPeaksComp_
bool compIntensity(CLabelCentroidPeaks x, CLabelCentroidPeaks y)
{
	return x.m_lfIntens > y.m_lfIntens;
}
// Attention these two comp are different.(> and <)
bool compMass(DeconvCentroidPeaks x, DeconvCentroidPeaks y)
{
	return x.m_lfMass < y.m_lfMass;
}
#endif

MS2Spectrum::MS2Spectrum(int CurrentMS2Scan, double **vIPV, CEmass *cemass)
{
	m_bIsDeconved = false;
	nCurrentMS2Scan = CurrentMS2Scan;
	RetentionTime = 0.0;
	ActiveCenter = 0.0;
	ActivationType = "";
	nActivationType = -1;
	InstrumentType = "";
	nInstrumentType = -1;

	m_vIPV = vIPV;
	m_cEmass = cemass;
}
MS2Spectrum::~MS2Spectrum()
{
	
}

/**
*	设置当前二级谱的一些相关信息
**/
void MS2Spectrum::SetRetentiontime(double dRetentiontime)
{
	RetentionTime = dRetentiontime;
}
void MS2Spectrum::SetActivationCenter( double dActiveCenter )
{
	ActiveCenter = dActiveCenter;
}
void MS2Spectrum::SetPrecursorScan( int iPrecursorScan )
{
	PrecursorScan = iPrecursorScan;
}
void MS2Spectrum::SetActivationType( string sActivationType )
{
	nActivationType = -1;
	ActivationType = sActivationType;
	for (int i = 0; i < ActivateTypeNum; i++)
	{
		if (sActivationType == cActivateType[i])
		{
			nActivationType = i+1;
			break;
		}
	}
}
void MS2Spectrum::SetInstrumentType( string sInstrumentType )
{
	nInstrumentType = -1;
	InstrumentType = sInstrumentType;
	for (int i = 0; i < InstrumentTypeNum; i++)
	{
		if (sInstrumentType == cInstrumentType[i])
		{
			nInstrumentType = i+1;
			break;
		}
	}
}
void MS2Spectrum::Setcharge(int dchg)
{
	m_nCharge = dchg;
}
void MS2Spectrum::SetMZ(double lfMZ)
{
	MZ = lfMZ;
}

void MS2Spectrum::AppendPeaks(double mz, double Intensity)
{
	Peaks.push_back(DeconvCentroidPeaks(mz, Intensity, 0.0, 0.0, 1));
}


/**
*	获取该二级谱的一些相关信息
**/
double MS2Spectrum::GetActivationCenter(  )
{
	return ActiveCenter;
}
string MS2Spectrum::GetActivationType(  )
{
	return ActivationType;
}
int MS2Spectrum::GetOutPutFileIdentifier()
{
	int OutPutFileIdentifier = (nInstrumentType - 1) * ActivateTypeNum + nActivationType - 1;
	return OutPutFileIdentifier;
}
int MS2Spectrum::GetCurrentMS2Scan()
{
	return nCurrentMS2Scan;
}
int MS2Spectrum::GetPrecursorScan()
{
	return PrecursorScan;
}
int MS2Spectrum::GetPeakNum()
{
	return Peaks.size();
}
int MS2Spectrum::Getcharge()
{
	return m_nCharge;
}
double MS2Spectrum::GetMZ()
{
	return MZ;
}
bool cmmMList(double x,double y)
{
	return (x <= y);
}

/*
*	二级谱预处理
*/
//void MS2Spectrum::GetEachIsoPeak(int peakPos, int chg, int &isoPeakNums, double &stdPPMTol, double &sumPeakInens, vector<double> &isoPattern, vector<int> &tmpIsoform, double Peak_Tol_ppm)
//{
//	int peakIdx = 0;
//	double TolValueTh = Peaks[peakPos].m_lfMass * Peak_Tol_ppm;
//	stdPPMTol = 0.0;
//	sumPeakInens = Peaks[peakPos].m_lfIntens;
//	isoPattern[HalfIsoWidth] = Peaks[peakPos].m_lfIntens;
//	isoPeakNums = 1; // 匹配上的同位素谱峰的根数
//	for (int s = -HalfIsoWidth; s <= HalfIsoWidth; ++s)
//	{
//		if(s == 0) continue;
//		double targetMZ = Peaks[peakPos].m_lfMass + avgdiv * s / chg;
//		if(fabs(targetMZ - Peaks[peakPos].m_lfMass) > 1.5 + TolValueTh) continue;
//
//		double minBias = 10.0;	
//		while(peakIdx < (int)Peaks.size())
//		{   // Peaks中mz值是由小到大的
//			/*if(m_vIsRemoved[peakIdx]) 
//			{
//				++peakIdx;
//				continue;
//			}*/
//			double tmpBias = Peaks[peakIdx].m_lfMass - targetMZ;
//			if (tmpBias < -TolValueTh)
//			{
//				++peakIdx;
//				continue;
//			} else if (tmpBias > TolValueTh) {
//				break;
//			} else {
//				if (fabs(tmpBias) < minBias && Peaks[peakIdx].m_lfIntens > 0)
//				{
//					minBias = fabs(tmpBias);
//					isoPattern[s + HalfIsoWidth] = Peaks[peakIdx].m_lfIntens;
//					tmpIsoform[s + HalfIsoWidth] = peakIdx;
//				}
//			}
//			++peakIdx;
//		}
//		if(minBias < 10)
//		{
//			minBias = minBias / targetMZ * 1000000;
//			++isoPeakNums;
//			stdPPMTol += minBias;
//			sumPeakInens += isoPattern[s + HalfIsoWidth];
//		}
//	}
//	
//	if(isoPeakNums > 1)
//	{
//		stdPPMTol /= (double)(isoPeakNums - 1);
//	} else {
//		stdPPMTol = 20.0;
//	}
//}
// 获取同位素峰簇
// param: 谱峰位置、电荷、&同位素峰数目、&同位素模式偏差、&谱峰强度和、&同位素模式强度、&同位素模式下标集合、&碎片离子误差
void MS2Spectrum::GetEachIsoPeak(int peakPos, int chg, int &isoPeakNums, double &stdPPMTol, double &sumPeakInens, vector<double> &isoPattern, vector<int> &tmpIsoform, double Peak_Tol_ppm)
{
	stdPPMTol = 0.0;  // 最高峰精度
	sumPeakInens = Peaks[peakPos].m_lfIntens;
	isoPattern[HalfIsoWidth] = Peaks[peakPos].m_lfIntens;
	tmpIsoform[HalfIsoWidth] = peakPos;
	isoPeakNums = 1; // 匹配上的同位素谱峰的根数

	int peakIdx = 0;
	double TolValueTh = Peaks[peakPos].m_lfMass * Peak_Tol_ppm;
	vector<double> vBias(isoPattern.size(), 20.0);
	for (int s = -HalfIsoWidth; s <= HalfIsoWidth; ++s)
	{
		if(s == 0) 
		{
			continue;
		}
		double targetMZ = Peaks[peakPos].m_lfMass + avgdiv * s / chg;
		if(fabs(targetMZ - Peaks[peakPos].m_lfMass) > 1.5 + TolValueTh) // 距离超过1.5Th，则不归入同位素峰队列
		{
			continue;
		}
		double minBias = TolValueTh;
		while(peakIdx < (int)Peaks.size()) // 寻找TargetMZ
		{   // Peaks中mz值是由小到大的
			/*if(m_vIsRemoved[peakIdx]) 
			{
				++peakIdx;
				continue;
			}*/
			double tmpBias = Peaks[peakIdx].m_lfMass - targetMZ;
			if (tmpBias < -TolValueTh)
			{
				++peakIdx;
				continue;
			} else if (tmpBias > TolValueTh){
				break;
			} else {
				if (fabs(tmpBias) < minBias && Peaks[peakIdx].m_lfIntens > 0)
				{
					minBias = fabs(tmpBias);
					vBias[s + HalfIsoWidth] = minBias / targetMZ * 1000000;   // 离子误差转化成ppm
					isoPattern[s + HalfIsoWidth] = Peaks[peakIdx].m_lfIntens;
					tmpIsoform[s + HalfIsoWidth] = peakIdx;
				}
			}
			++peakIdx;
		}
	}
	
	// cut the isoPattern
	int missNum = 0;
	for(int s = HalfIsoWidth - 1; s >= 0; --s) // 从最高峰处向前查找
	{
		if(isoPattern[s] <= eps)
		{
			++missNum;
			continue;
		}
		if(missNum >= 2)  // 如果附近有两根谱峰缺失，则舍弃该谱峰
		{
			isoPattern[s] = 0.0;
			tmpIsoform[s] = -1;
		} else {
			++isoPeakNums;
			stdPPMTol += vBias[s];
			sumPeakInens += isoPattern[s];
		}
	}
	missNum = 0;
	for(int s = HalfIsoWidth + 1; s < (int)isoPattern.size(); ++s) // 从最高峰处向后查找
	{
		if(isoPattern[s] <= eps)
		{
			++missNum;
			continue;
		}
		if(missNum >= 2)
		{
			isoPattern[s] = 0.0;
			tmpIsoform[s] = -1;
		} else {
			++isoPeakNums;
			stdPPMTol += vBias[s];
			sumPeakInens += isoPattern[s];
		}
	}

	if(isoPeakNums > 1)
	{
		stdPPMTol /= (double)(isoPeakNums - 1); //去掉最高峰后的平均偏差
	} else {
		stdPPMTol = 20.0;
	}
}

// 计算理论同位素峰簇与实验同位素峰簇相似度
// param: 谱峰中性质量、&最高峰位置、&同位素峰数目、&同位素峰强度和、&同位素模式强度、&理论同位素模式、&同位素模式下标
double MS2Spectrum::ComputIPVSimilarity(double mass, int &highestPeakPos, int &isoPeakNum, double &sumPeakInt, vector<double> &isoPattern, vector<double> &thoIsoPattern, vector<int> &tmpIsoform)
{	
	int mIdx = int(mass);
	
	vector<double> thoPattern;
	highestPeakPos = 0;

	if(mIdx >= LIPV_MASSMAX || mIdx < 0)
	{   // 超出IPV的长度
		return 0.0;
	} else if (mIdx < IPV_MASSMAX) {
		double highestPeak = 0.0;
		//cout << mIdx << endl;
		for (int i = 0; i < 2 * IPV_PEAKMAX; i += 2)
		{
			thoPattern.push_back(m_vIPV[mIdx][i+1]);
			if(m_vIPV[mIdx][i+1] > highestPeak)
			{
				highestPeak = m_vIPV[mIdx][i+1];
				highestPeakPos = i / 2;
			}
		}
	} else {
		vector<CentroidPeaks> calIso;
		m_cEmass->Calculate(mass, calIso);
		double highestPeak = 0.0;
		for (size_t i = 0; i < calIso.size(); ++i) // 查找最高峰
		{
			thoPattern.push_back(calIso[i].m_lfIntens);
			if(calIso[i].m_lfIntens > highestPeak)
			{
				highestPeak = calIso[i].m_lfIntens;
				highestPeakPos = i;
			}
		}
	}

	if(highestPeakPos == HalfIsoWidth)
	{
		//恰好第HalfIsoWidth根峰为最高峰，若总的谱峰数目不够，在右侧补零
		if((int)thoPattern.size() < IsoProfileNum)
		{
			thoPattern.insert(thoPattern.end(), IsoProfileNum - thoPattern.size(), 0.0);
		} else if((int)thoPattern.size() > IsoProfileNum) {
			thoPattern.erase(thoPattern.begin() + IsoProfileNum, thoPattern.end());
		}
	} else if(highestPeakPos < HalfIsoWidth) { 
	    //最高峰左边的谱峰不够，补零
		thoPattern.insert(thoPattern.begin(), HalfIsoWidth - highestPeakPos, 0.0);
		if((int)thoPattern.size() > IsoProfileNum)
		{   //删除最高峰右边多余的谱峰
			thoPattern.erase(thoPattern.begin() + IsoProfileNum, thoPattern.end());
		} else if((int)thoPattern.size() < IsoProfileNum){
			thoPattern.insert(thoPattern.end(), IsoProfileNum - thoPattern.size(), 0.0);
		}
	} else if(highestPeakPos > HalfIsoWidth) {
		//最高峰左边的谱峰太多，删除较小的部分
		thoPattern.erase(thoPattern.begin(), thoPattern.begin() + highestPeakPos - HalfIsoWidth);
		if((int)thoPattern.size() < IsoProfileNum)
		{
			thoPattern.insert(thoPattern.end(), IsoProfileNum - thoPattern.size(), 0.0);
		} else if((int)thoPattern.size() > IsoProfileNum) {
			thoPattern.erase(thoPattern.begin() + IsoProfileNum, thoPattern.end());
		}
	}

	int bestisoPeakNum = isoPeakNum, bestHighest = highestPeakPos;
	double bestsumPeakInt = sumPeakInt, bestSim = 0;
	vector<int> besttmpIsoform = tmpIsoform;

	int limit = 1;
	if(highestPeakPos == 0) limit = 0; // 最高峰为mono峰，则不往左右移动
	if(mass > 3300) limit = 2;
	for(int m = -limit; m <= limit; ++m)
	{
		if(isoPattern[HalfIsoWidth + m] < 0.00000001) // 左右没有峰
		{
			continue;
		}
		int oneisoPeakNum = isoPeakNum;
		double onesumPeakInt = sumPeakInt;
		vector<int> oneIsoformPos = tmpIsoform;
		vector<double> oneIsoform = isoPattern;

		vector<double> vTheo = thoPattern;
		if(m < 0)    // 左移 m根峰, 实验谱最高峰的位置其实是右移了 m
		{
			vTheo.erase(vTheo.begin(), vTheo.begin() - m);
			for(int s = 0; s < -m; ++s)
				vTheo.push_back(0);
		} else if(m > 0) {  // 右移 m根峰, 实验谱最高峰的位置其实是左移了 m
			for(int s = 0; s < m; ++s)
				vTheo.pop_back();
			for(int s = 0; s < m; ++s)
				vTheo.insert(vTheo.begin(), 0);
			
		}

		// 从理论同位素峰簇来看，有些峰不存在，实验谱中计算强度和时可能有些不属于该同位素模式
		for(int k = 0; k < IsoProfileNum; ++k)
		{    
			if(vTheo[k] / vTheo[HalfIsoWidth + m] < 0.05 && isoPattern[k] > 0.00000001)
			{
				onesumPeakInt -= isoPattern[k];
				oneIsoform[k] = 0.0;
				oneIsoformPos[k] = -1;
				--oneisoPeakNum;
			}
		}
		double similarity = CStatistic::CalculateVectorCov(vTheo, oneIsoform);
		if(similarity > bestSim)
		{
			bestSim = similarity;
			bestisoPeakNum = oneisoPeakNum;
			bestsumPeakInt = onesumPeakInt;
			besttmpIsoform = oneIsoformPos;
			thoIsoPattern = vTheo;
			bestHighest = highestPeakPos - m; // 实际上是实验谱峰的最高峰改变了，这里通过改变理论最高峰的位置使得最后求得的mono峰准确
			
		}
	}
	isoPeakNum = bestisoPeakNum;
	sumPeakInt = bestsumPeakInt;
	tmpIsoform = besttmpIsoform;
	highestPeakPos = bestHighest;
	return  bestSim;
}


double MS2Spectrum::FilterByBaseline()
{
	//cout<<"FilterByBaseline"<<endl;
	if(Peaks.size() == 0) return 0.0;
	// 把所有的谱峰强度取log后存储在LogInten中
	vector<double> logInten;
	logInten.reserve(Peaks.size()+1);
	vector<size_t> removePeakID; 
	size_t i = 0;
	int cntWidth = 200;
	//double endMz = Peaks[0].m_lfMZ + width;//CHECK
	size_t sId = 0, eId = 0;
	int cnt = 0;
	double MaxIntensity = 0; 
	double MinIntensity = 100;
	double baseline = 0;
	const double distDalton = 0.1;
	while(i < Peaks.size())
	{
		while(i < Peaks.size() && ++cnt <= cntWidth)
		{
			logInten.push_back(log10(Peaks[i].m_lfIntens));
			if (logInten[i] < MinIntensity)
			{
				MinIntensity = logInten[i];
			}
			if (logInten[i] > MaxIntensity)
			{
				MaxIntensity = logInten[i];
			}
			++i;
		}
		eId = i - 1;

		vector<double> SliceBin;  // 分Bin
		SliceBin.push_back(distDalton/2 + MinIntensity);
		size_t j = 0;
		while (SliceBin[j] + distDalton < MaxIntensity)
		{
			SliceBin.push_back(SliceBin[j] + distDalton);
			++j;
		}
		SliceBin.push_back(SliceBin[j] + distDalton);
		//Init
		int SizeofSliceBin = SliceBin.size();
		vector<int> HistCounter;
		int MaxIndex = -1;
		int MaxLoghist = 0;
		HistCounter.assign(SizeofSliceBin, 0);

		for (j = sId; j <= eId; ++j)
		{
			size_t idx = 0;
			if (logInten[j] <= SliceBin[0])
			{
				idx = 0;
			} else {
				idx = (unsigned int)ceil((logInten[j] - SliceBin[0]) / distDalton);
			}
			if(idx > HistCounter.size())
			{
				cout << "[Error] HistCounter Off normal upper bound" << endl;
				exit(0);
			}
			++HistCounter[idx];
			if (MaxLoghist < HistCounter[idx])
			{
				MaxIndex = idx;
				MaxLoghist = HistCounter[idx];
			}
		}
		if(SliceBin[MaxIndex] - distDalton / 2 > baseline)
		{
			baseline = SliceBin[MaxIndex] - distDalton / 2;
		}
		for (j = sId; j <= eId; ++j)
		{
			if (logInten[j] <= SliceBin[MaxIndex] - distDalton/2)
			{
				removePeakID.push_back(j);
			}
		}
		sId = i;
		cnt = 0;
		MaxIntensity = 0; 
		MinIntensity = 100;
	}
	if(removePeakID.size() == 0) return baseline;
	size_t pos = 0, removePos = 0;
	for(size_t i = 0; i < Peaks.size(); ++i)
	{
		if(removePos < removePeakID.size() && i == removePeakID[removePos])
		{
			++removePos;
		} else {
			if(pos != i) Peaks[pos++] = Peaks[i];
			else ++pos;
		}
	}
	Peaks.erase(Peaks.begin() + pos, Peaks.end() - 1);

	return baseline;
}

double MS2Spectrum::FilterByBaselineWhole(double SNratio)
{
	if(Peaks.size() == 0) return 0.0;
	// 把所有的谱峰强度取log后存储在LogInten中
	vector<double> logInten;
	logInten.reserve(Peaks.size()+1);
	vector<int> removePeakID; 
	size_t i = 0;
	double MaxIntensity = 0; 
	double MinIntensity = 100;
	while(i < Peaks.size())
	{
		if(Peaks[i].m_lfIntens <= 0)
		{
			continue;
		}
		logInten.push_back(log10(Peaks[i].m_lfIntens));
		if (logInten[i] < MinIntensity)
		{
			MinIntensity = logInten[i];
		}
		if (logInten[i] > MaxIntensity)
		{
			MaxIntensity = logInten[i];
		}
		++i;
	}
	const double distIntens = 0.08;
	vector<double> SliceBin;
	SliceBin.push_back(distIntens / 2 + MinIntensity);
	int j = 0;
	while (SliceBin[j] + distIntens < MaxIntensity)
	{
		SliceBin.push_back(SliceBin[j] + distIntens);
		++j;
	}
	SliceBin.push_back(SliceBin[j] + distIntens);
	//Init
	int SizeofSliceBin = SliceBin.size();
	vector<int> HistCounter;
	int MaxIndex = -1;
	int MaxLoghist = 0;
	HistCounter.assign(SizeofSliceBin, 0);

	for (j = 0; j < (int)Peaks.size(); ++j)
	{
		size_t idx = 0;
		if (logInten[j] <= SliceBin[0])
		{
			idx = 0;
		} else {
			idx = (unsigned int)ceil((logInten[j] - SliceBin[0]) / distIntens);   // 向上取整
		}
		if(idx > HistCounter.size())
		{
			cout << "[Error] HistCounter get normal upper bound" << endl;
			exit(0);
		}
		++HistCounter[idx];
		if (MaxLoghist < HistCounter[idx])
		{
			MaxIndex = idx;
			MaxLoghist = HistCounter[idx];
		}
	}
	double baseline = SliceBin[MaxIndex] - distIntens / 2;
	//cout << baseline << " ";
	int leftidx = MaxIndex - 1, rightidx = MaxIndex + 1;
	while(leftidx >= 0 && HistCounter[leftidx] > (HistCounter[MaxIndex]+1) / 2)
	{
		--leftidx;
	}
	while(rightidx < (int)HistCounter.size() && HistCounter[rightidx] > (HistCounter[MaxIndex]+1) / 2)
	{
		++rightidx;
	}
	baseline += SNratio * distIntens * (rightidx - leftidx - 1) / 2.355;
	//cout << rightidx - leftidx << " " << SNratio << " " << baseline << endl;
	//cout<<"Filter baseline: " << pow(10, SliceBin[MaxIndex] - 0.04)<<endl;
	for (j = 0; j < (int)Peaks.size(); ++j)
	{
		if (logInten[j] <= SliceBin[MaxIndex] - distIntens / 2)
		{
			removePeakID.push_back(j);
		}
	}

	if(removePeakID.size() == 0) return baseline;
	size_t pos = 0, removePos = 0;
	for(size_t i = 0; i < Peaks.size(); ++i)
	{
		if(removePos < removePeakID.size() && i == removePeakID[removePos])
		{
			++removePos;
		} else {
			if(pos != i) Peaks[pos++] = Peaks[i];
			else ++pos;
		}
	}
	Peaks.erase(Peaks.begin() + pos, Peaks.end()-1);
	return baseline;
}

// 开1.5TH的窗口，获取窗口及误差范围内的所有谱峰强度和
double MS2Spectrum::GetAllPeakIntens(int peakPos, double Peak_Tol_ppm)
{
	double TolValueTh = Peaks[peakPos].m_lfMass * Peak_Tol_ppm;
	double sumPeakInens = Peaks[peakPos].m_lfIntens;
	int peakIdx = peakPos - 1;
	while(peakIdx >= 0)
	{
		if(Peaks[peakPos].m_lfMass - Peaks[peakIdx].m_lfMass > 1.5 + TolValueTh) break;
		sumPeakInens += Peaks[peakIdx].m_lfIntens;
		--peakIdx;
	}
	peakIdx = peakPos + 1;
	while(peakIdx < (int)Peaks.size())
	{
		if(Peaks[peakIdx].m_lfMass - Peaks[peakPos].m_lfMass  > 1.5 + TolValueTh) break;
		sumPeakInens += Peaks[peakIdx].m_lfIntens;
		++peakIdx;
	}
	return sumPeakInens;
}


// 1. 计算噪声基线，去除基线之下的谱峰
// 2. 找出所有的极大值点
// 3. 依次枚举每个极大值点处的谱峰为最高峰，枚举电荷，计算单同位素峰
void MS2Spectrum::Deconvolution(int precurChg, vector<double> &precurMass, double Peak_Tol_ppm, double SNratio)
{
	//cout << "deconvolution" << endl;
	if(m_bIsDeconved) 
	{
		return;
	}

	// wrm:
	double baseline = FilterByBaselineWhole(SNratio);
	baseline = pow(10, baseline);
	//double baseline = 0; // debug

	int peakNum = (int)Peaks.size();
	vector<int> vIntensOrder(peakNum,0);

	CLabelCentroidPeaks *newPeaks = new CLabelCentroidPeaks[peakNum];
	for(int i = 0; i < peakNum; ++i)
	{
		newPeaks[i].m_lfMZ = Peaks[i].m_lfMass;
		newPeaks[i].m_lfIntens = Peaks[i].m_lfIntens;
		newPeaks[i].m_nIdx = i;

	}
	// sort all peaks by intensity, and the order recod in the array?
	sort(newPeaks, newPeaks + peakNum, compIntensity);

	for(int i = 0; i < peakNum; ++i)
	{
		vIntensOrder[i] = newPeaks[i].m_nIdx;
	}
	delete[]newPeaks;

	vector<int> m_vIsRemoved(peakNum, 0);

	vector<DeconvCentroidPeaks> vMonoMZ;
	vector<double> vMaxSim;
	
	// 依次计算最大强度峰的mono峰和电荷
	for(int k = 0; k < peakNum; ++k) // 枚举每根峰为最高峰
	{
		int peakPos = vIntensOrder[k];
		double mz = Peaks[peakPos].m_lfMass;
		/*if(fabs(mz - 499.28) < 0.01)
			cout << "test" << endl;*/
		double ints = Peaks[peakPos].m_lfIntens;
		if(1 == m_vIsRemoved[peakPos] || ints < baseline) // 在其他谱峰的同位素峰中出现过的谱峰不再作为最高峰
		{
			continue;
		}
		double peakIntsInWind = GetAllPeakIntens(peakPos, Peak_Tol_ppm);  // [ +- 1.5TH ]获取误差范围内的所有谱峰强度和

		double bestScore = 0.0;
		double bestChg = 0.0;
		double bestSim = 0.0;
		double bestTolScore = 0.0;
		double bestIsoPeakNum = 0.0;
		double bestMonoMz = 0.0;
		double bestSumInt = 0.0;
		vector<int> bestIsoform;
		vector<double> bestThoIsoPattern;

		//对于每一个候选最高峰M/Z枚举电荷
		for (int chg = 1; chg <= precurChg; ++chg)
		{
		    int isoPeakNums = 0;
			double stdPPMTol = 0.0;
			double sumPeakInens = 0.0;
		
			vector<int> tmpIsoform;
			vector<double> isoPattern;
			vector<double> thoIsoPattern;
			isoPattern.assign(IsoProfileNum, 0);   // 同位素模式，保留15根峰，中间为最高峰
			tmpIsoform.assign(IsoProfileNum, -1);
			
			GetEachIsoPeak(peakPos, chg, isoPeakNums, stdPPMTol, sumPeakInens, isoPattern, tmpIsoform, Peak_Tol_ppm);
			int highestPeakPos = 0;
			
			// Important Function 计算理论与实验同位素峰簇的相似度
			double MaxSim = ComputIPVSimilarity(Peaks[peakPos].m_lfMass * chg - chg * pmass, highestPeakPos, isoPeakNums, sumPeakInens, isoPattern, thoIsoPattern, tmpIsoform);
			
			if(MaxSim < 0.6) continue;   // [wrm?]: 如果二级谱中没有同位素峰簇呢，直接continue会不会丢失谱峰？
			double monoMass = Peaks[peakPos].m_lfMass - highestPeakPos * avgdiv / chg;
			if(monoMass * chg - (chg - 1) * pmass > 6000 && isoPeakNums < 5) //TO CHECK
				continue;
			if(chg == 1 && monoMass * chg - (chg - 1) * pmass > 1500 && isoPeakNums == 1) // [wrm?]:单电荷的单一谱峰不可能存在吗？
				continue;

			//double avgIntens = sumPeakInens / isoPeakNums;
			double pif = sumPeakInens / peakIntsInWind;
			
			//if(pif < 0.05) continue; //comment at 2015.03.19
			// [wrm?]: 平均偏差有没有可能大于21ppm
			double tmpScore = MaxSim * MaxSim * pif * (21 - stdPPMTol)/21.0;

			if(tmpScore > bestScore)  // [wrm?]:两个打分有没有可能相同？
			{
				bestScore = tmpScore;
				bestChg = (double)chg;
				bestSim = MaxSim;
				bestTolScore = 20.0 - stdPPMTol;
				bestIsoPeakNum = isoPeakNums;
				bestSumInt = sumPeakInens;
				bestMonoMz = monoMass;
				bestIsoform = tmpIsoform;
				bestThoIsoPattern = thoIsoPattern;
			}
		}//for charge
		
		// [test]
		/*t_IsoPeakNum.push_back(bestIsoPeakNum); 
		t_charge.push_back(bestChg);*/

		if(bestScore <= 0) continue;
		DeconvCentroidPeaks tmpMonoPeak(bestMonoMz * bestChg - (bestChg - 1) * pmass, bestSumInt, bestMonoMz, mz, bestChg);
		//cout << bestMonoMz * bestChg - (bestChg - 1) * pmass << " " << bestSumInt << " " << bestMonoMz << " " << " " << mz << " " << bestChg << endl;
		vMonoMZ.push_back(tmpMonoPeak);
		vMaxSim.push_back(bestSim);

		double greatPart = bestThoIsoPattern[0];
		for(size_t j = 1; j < bestThoIsoPattern.size(); ++j)
		{
			if(bestThoIsoPattern[j] < bestThoIsoPattern[j-1])
				break;
			if(bestThoIsoPattern[j] > greatPart)
				greatPart = bestThoIsoPattern[j];
		}

		for(size_t j = 0; j < bestIsoform.size(); ++j)
		{
			//if(bestIsoform[j] == 463)
			//	cout << peakPos << " " << mz << endl;
			if(bestIsoform[j] != -1)
			{
				m_vIsRemoved[bestIsoform[j]] = 1;
				
				Peaks[bestIsoform[j]].m_lfIntens -= (bestThoIsoPattern[j] / greatPart) * ints; // 谱峰强度按比例缩减
				if(Peaks[bestIsoform[j]].m_lfIntens < 0)
					Peaks[bestIsoform[j]].m_lfIntens = 0;
			}
		}
	}
	// [test]
	// t_pre_charge = precurChg;

	sort(vMonoMZ.begin(), vMonoMZ.end(), compMass);
	Peaks.swap(vMonoMZ);
	//PreProcess(); //合并的话就分不清楚谱峰来源了
	
	//FilterByBaselineWhole(SNratio);
	RemovePrecursor(precurMass, Peak_Tol_ppm);
	m_bIsDeconved = true;
}

void MS2Spectrum::PreProcess()
{
	int peakSize = (int)Peaks.size();
	if(0 >= peakSize) return;

	vector<DeconvCentroidPeaks> vCombPeaks;
    
	//按10ppm合并谱峰，强度则求和
	double baseMZ = Peaks[0].m_lfMass;
    double sumMZ = baseMZ * Peaks[0].m_lfIntens;
    double sumInten = Peaks[0].m_lfIntens;

    for(int tIdx = 1; tIdx < peakSize; ++tIdx)
    {
        if( 1000000 * (Peaks[tIdx].m_lfMass - baseMZ) / baseMZ <= 10)
        {
            sumInten += Peaks[tIdx].m_lfIntens;
            sumMZ += Peaks[tIdx].m_lfMass * Peaks[tIdx].m_lfIntens;
        } else {
            DeconvCentroidPeaks combedPeak(sumMZ / sumInten, sumInten, 0.0, 0.0, 1);
			vCombPeaks.push_back(combedPeak);

            baseMZ = Peaks[tIdx].m_lfMass;
            sumInten = Peaks[tIdx].m_lfIntens;
            sumMZ = baseMZ * sumInten;
        }
    }
	Peaks = vCombPeaks;
}

// 移除母离子谱峰
// param: 中性母离子，离子误差
void MS2Spectrum::RemovePrecursor(vector<double> &precurMass, double Peak_Tol_ppm)
{
	int i, j, n = precurMass.size();
	vector<double> removeList = precurMass;
	for(i = 0; i < n; ++i)
	{
		double originalMass = precurMass[i];
		removeList.push_back(originalMass - massH2O - massH);
		removeList.push_back(originalMass - massH2O);
		removeList.push_back(originalMass - massH2O + massH);

		removeList.push_back(originalMass - massNH3 - massH);
		removeList.push_back(originalMass - massNH3);
		removeList.push_back(originalMass - massNH3 + massH);

		removeList.push_back(originalMass - massH);
		removeList.push_back(originalMass + massH);
	}
	sort(removeList.begin(), removeList.end());
	i = (int)removeList.size() - 1;
	j = (int)Peaks.size() - 1;   // MZ最大的谱峰
	vector<int> remove;
	while(j >= 0 && i >= 0) 
	{
		double dist = removeList[i] * Peak_Tol_ppm;
		if(Peaks[j].m_lfMass < removeList[i] - dist)
		{
			--i;
		} else if(Peaks[j].m_lfMass  >= removeList[i] - dist && Peaks[j].m_lfMass  <= removeList[i] + dist) {
			// 该谱峰可能为母离子
			remove.push_back(j);
			--j;
		} else if(Peaks[j].m_lfMass  > removeList[i] + dist) {
			--j;
		}
	}
	if(remove.empty()) return;
	int idx = remove.back();
	j = idx;
	i = (int)remove.size() - 1;
	while(j < (int)Peaks.size() - (int)remove.size())
	{
		if(i >= 0 && remove[i] == j)
		{
			++j;
			--i;
		} else {
			Peaks[idx++] = Peaks[j++];
		}
	}
	Peaks.erase(Peaks.begin() + idx, Peaks.end() - 1);
}
