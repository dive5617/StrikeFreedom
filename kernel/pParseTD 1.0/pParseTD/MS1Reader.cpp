
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
// ��ʼ��MS1Reader���� halfWnd�ȸ��봰�����Ҹ�����0.5Da
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

//��ȡ�������׷�֮������������׷��ǿ��֮�ͣ������Ƚ��м�Сֵ�жϡ���
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
	// ����ÿһ��һ���װ���baseline���й���
	for (unsigned int i = 0; i < MS1List.size(); ++i)
	{
		MS1List[i]->FilterByBaseline();
	}
}

/*
	For given MS2 spectrum, extract all the related MS1 information.
	Store those infomation in to two data struct: PeakMatrix and PrecursorCandidatePeaks;
	wrm�����ݶ����׵�scan���Ҷ�Ӧ��һ����(�ҵ���һ��С��MS2Scan��һ����Scan)�������ݶ������и����������ģ���һ�������ҵ���ص�ĸ����
	     ����MS2�ļ���ÿ�Ŷ������������Ѿ����������ȵ����ӵ�Scan�Ű�
*/
void MS1Reader::GetIndex_By_MS2Scan(int &index,int &MS2scan) // CHECK luolan
{
	// TODO���������ʵ���Ͽ��Լ��ٵ�log(n) ���ݶ����ײ���һ���ף�û�б�Ҫ����ʵ�飻wrm: ���߶����׺�һ����֮�佨����ϣ��
	for (int i = MS1Scancounts-1; i >= 0; --i)
	{
		if (MS2scan > MS1List[i]->GetCurrentMS1Scan())
		{
			index = i;
			break;
		} 
	}
}

// wrm: ��ȡ���������ڵ������׷�
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
// ���ݶ����ײ��Ҷ�Ӧ��һ���ף������ݶ����������������Ļ�ȡ��ѡĸ���ӵ�ͬλ�ط壬ö����߷�͵��
// [-5,+5]
void MS1Reader::GetCandidatesPrecursorByMS2scan(int MS2scan, int MS1scan, int userSpanStart,int userSpanEnd,double ActivationCenter,vector<CentroidPeaks> &PrecursorCandidatePeaks)
{
	// Called by AssessPrecursor()
	// Peaks with intensity less than 2% of the highest peak's intensity are deleted.
	// Baseline(0) intensity are deleted.
	
	int MS1_idx = -1;                                       // -1 for empty spectrum
	// ֱ�Ӷ�ȡ֮ǰ�洢��MS2��Ӧ��һ����Scan.  modified by wrm. 2015.10.10
	// GetIndex_By_MS2Scan(MS1_idx, MS2scan);                 // Get the index for the MS1 spectrum.
	MS1_idx = MS1Scan_Index[MS1scan];                  // should first check if MSscan1 is valid 

	// ��ʼ���׷��������Ϊȫ0������ǰ��10���׵��׷���Ϣ
	InitPeakMatrix();                                     // Initialize all the item as zeros.
	// ����׷��¼���е���Ϣ
	InitPeaksRecord();                                    // we store the peaks in this structure. Calc STD of MONO.

	// ��¼��-1�� 0�� 1��һ�����е��׷��ں�ѡ�׷��б��е���ʼλ�ã���PrecursorCandidatePeaks�дӵڼ����׷忪ʼ�����ڵ�-1/0/1���׵�
	// 0 -> index1 -> index2 -> index3
	vector<int> PeakEndIndex;                             // Record the peak index of the MS1(-1) MS1(0) MS1(1) refer to MS2 
	PeakEndIndex.push_back(0);
	string RelatedMS1 = "";
	//FILE * pfilems1 = NULL;

	double halfWindow = 1.0 * GetIsolationWidth() / 2;

	// �鿴-4��-3��-2��-1��0��1��2��3��4��5��10��1����
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

		// ��ȡ���������ĸ������׷�
		vector<CentroidPeaks> VectorPeaksTmp;                // Store Peaks temporarily.
		double LargestIntensity = 0.0;                       // Most intense peak

		// Extract MS1 peaks from given MS1 spectrum .
		// int tScan = MS1List[MS1_idx + j]->GetCurrentMS1Scan();
		// wrm: ��halfwindow�Ļ�����������չ0.5THѰ���׷�, ���ں��������Ѵ����ڵ���߷�ʱȴû�п�����չ�����ڵ��׷壿
		GetMS1_Peaks_In_Window_On_Given_Index(ActivationCenter, halfWindow + ExtendHalfwindow, MS1_idx + j, VectorPeaksTmp);

		for (unsigned int ino = 0; ino < VectorPeaksTmp.size(); ++ino)
		{
			//ÿһ���嶼Ҫ���뵽PeakRecord�м�¼������
			PeakRecord.push_back(VectorPeaksTmp[ino]);// �������ڼ�������У׼
			
			if (VectorPeaksTmp[ino].m_lfIntens > LargestIntensity) //�����ǿ�׷�
			{
				if (VectorPeaksTmp[ino].m_lfMZ < ActivationCenter - halfWindow || VectorPeaksTmp[ino].m_lfMZ > ActivationCenter + halfWindow)
				{
					continue;
				}
				LargestIntensity = VectorPeaksTmp[ino].m_lfIntens;
			}
		}
		//// ���������߹��� // comment by luolan @2014.12.02
		//// Here can be optimized for the getbaseline only once...
		//double baselineHere = GetBaselineByIndex(MS1_idx + j);
		//for (int ino = VectorPeaksTmp.size() - 1; ino >= 0; ino--)
		//{
		//	//ɾ�������壬ʵ����ʲôҲû������
		//	if (VectorPeaksTmp[ino].m_lfIntens < LargestIntensity * 0.02 || VectorPeaksTmp[ino].m_lfIntens < baselineHere)
		//	{
		//		VectorPeaksTmp.erase(VectorPeaksTmp.begin() + ino);
		//	}
		//}
		
		double baselineHere = GetBaselineByIndex(MS1_idx + j);
		for (unsigned int ino = 0; ino < VectorPeaksTmp.size(); ++ino)
		{
			//����������ͼV���������е��׷嶼�Ǻ�ѡ�׷塣
			if (j >= -1 && j <= 1 && VectorPeaksTmp[ino].m_lfIntens > baselineHere)
			{   //�ȵ�MS1��ͼ��ǰ���һ��һ�������׷�ǿ�ȳ������ߵĲ���Ϊ��ѡĸ���ӵ���߷�
				PrecursorCandidatePeaks.push_back(VectorPeaksTmp[ino]);
			}
			//�����������������¼�����е���ص��׷塣����10 * (���봰��*100) �ľ����С��ٽ��׷��ǿ�Ȼ���ӡ�	
			m_PeakMap.IntensityMap(VectorPeaksTmp[ino].m_lfMZ, VectorPeaksTmp[ino].m_lfIntens, ActivationCenter,j);
		}
		//�жϱ���Ƿ�������ǰ��һ�ţ������һ�ŵķ�Χ��, modified by luolan @2014.12.02
		if (j >= -1 && j <= 1)
		{
			PeakEndIndex.push_back(PrecursorCandidatePeaks.size());//��¼ÿһ����ͼ���׷���PrecursorCandidatePeaks�е���ʼλ�á�����ɾ����
		}
	}

	// [wrm]: ��ǰ��10��һ�����ж�û�к�ѡ�׷壬�򷵻أ� ����Ժ�ѡ�׷���й���
	if (!PrecursorCandidatePeaks.size())
	{
		//How could there be no peak? Let's return and do nothing!		
		return;
	}
	// This Operation reduce 2/3 of the Results
	bool Check_If_ActivationCenter_Peak_Exist_In_Previous_Scan = false;//�ж��Ƿ������������ġ�
	if (m_para->GetValue("check_activationcenter").compare("1") == 0)
	{
		for (int ino = PeakEndIndex[1]; ino < PeakEndIndex[2]; ino++)
		{
			//�����м����ϲ��Ƿ����������ġ�
			if (fabs(PrecursorCandidatePeaks[ino].m_lfMZ - ActivationCenter) < ActivationCenter*Peak_Tol_ppm)
			{
				Check_If_ActivationCenter_Peak_Exist_In_Previous_Scan = true;
			}
		}

		if (Check_If_ActivationCenter_Peak_Exist_In_Previous_Scan)
		{
			//_COUT_DEBUG_INFO_("Candidates: Searched -1");
			//����鵽�ˣ���ô�Ͱ�ʣ���������ͼ���׷�ȫ��ɾ���������أ�
			PrecursorCandidatePeaks.erase(PrecursorCandidatePeaks.begin()+PeakEndIndex[2], PrecursorCandidatePeaks.end());//erase the last scan

			PrecursorCandidatePeaks.erase(PrecursorCandidatePeaks.begin(), PrecursorCandidatePeaks.begin()+PeakEndIndex[1]);// erase the first scan.
			return;
		}

		Check_If_ActivationCenter_Peak_Exist_In_Previous_Scan = false;
		for (int ino=PeakEndIndex[0];ino<PeakEndIndex[1];ino++)
		{
			//_COUT_DEBUG_INFO_("Candidates: Searched -1 -2");
			//���û�в鵽�������ģ���ǰһ����ͼ�ϣ������������ġ�
			if (fabs(PrecursorCandidatePeaks[ino].m_lfMZ - ActivationCenter)<ActivationCenter*Peak_Tol_ppm)
			{
				Check_If_ActivationCenter_Peak_Exist_In_Previous_Scan = true;
			}
		}
		if (Check_If_ActivationCenter_Peak_Exist_In_Previous_Scan)
		{
			//�����ǰһ����ͼ�ϲ鵽�����������׷壬��ô�Ѻ�һ����ͼ����Ϣɾ���������أ�����ʲôҲ��Ҫ�������ء�
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

// ��һ���׷�ǿ��
void MS1Reader::ScaledIsoPattern(double sumIsopattern)
{
	for (int ino=IsoProfileNum;ino<MaxIsoFeature ;ino++)
	{
		IsoPattern[ino]=IsoPattern[ino]/sumIsopattern;
	}
}

// ��ȡǰ��10��һ������ͬλ�ط��ǿ�Ⱥ�
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
// ������߷�͵�ɣ���ǰ��10����ͼ�л�ȡ���е�ͬλ��ģʽ����������Ҫ�õ�֮ǰ������׷��������10*(2*IsolationWidth)
void  MS1Reader::GetAllIsoFeature(int chg, double ActivationCenter, vector<CentroidPeaks> &PrecursorCandidatePeaks, int k)
{
	for ( int inoSpan = 0; inoSpan < 2*Span; ++inoSpan )
	{   // ѭ���鿴Ŀǰһ���׵�ǰspan�ͺ�span��һ����
		int basePos = IsoProfileNum + IsoProfileNum * inoSpan;
		double Tol = ActivationCenter * Peak_Tol_ppm;
		for (int s = -HalfIsoWidth; s <= HalfIsoWidth; ++s)
		{   // ��ö�ٵ���߷�λ�ã�Ѱ����ǰHalfIsoWidth(0.5 + isolation/2)�ͺ�HalfIsoWidth���׷�
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

// ������߷弰���Ҽ������ɫ�����ƶ� modified by luolan @2014.05.12
void MS1Reader::CalcChromSimilarity()
{
	vector<double> vTmpXXSimilarity = vector<double>(); // ���ÿ�����Ӧ���������ڻ�
	vector<double> vTmpXYSimilarity = vector<double>(); // ���ÿ�����Ӧ����������߷��Ӧ���������ڻ�
	// -2,-1,0,1,2
	for(int i = -PeaksforChromSim; i <= PeaksforChromSim; ++i)
	{
		double tmpSim = IsoPatternColumnDotProduct(i + HalfIsoWidth, i + HalfIsoWidth);
		vTmpXXSimilarity.push_back(tmpSim);
		tmpSim = IsoPatternColumnDotProduct(i + HalfIsoWidth, HalfIsoWidth);
		vTmpXYSimilarity.push_back(tmpSim);
	}

	double highestSim = vTmpXXSimilarity.at(PeaksforChromSim);
	ChromSimilarity[0] = 0.0; // ���������ƶ�
	ChromSimilarity[1] = 0.0; // �Ҳ�������ƶ�
	if(fabs(highestSim) < 0.00000001) // highestSim == 0.0 ��������ò�Ҫֱ���ж��Ƿ����
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
			ChromSimilarity[0] = tmpChromSimilarity;  // ��0����߷���֮ǰ�ĵȼ���׷��������ƶ�
		} else if(i > PeaksforChromSim && tmpChromSimilarity > ChromSimilarity[1]) {
			ChromSimilarity[1] = tmpChromSimilarity; //  ��1����߷���֮��ĵȼ���׷��������ƶ�
		}
	}
}
// �������
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
// ������߷�͵�ɣ��ں�ѡ�׷��б��л�ȡͬλ�ط��
// ��ͬλ�ط��ƫ����������߷��ƫ��
void MS1Reader::GetEachIsoPeak(vector<CentroidPeaks> &PrecursorCandidatePeaks, int chg, int k, int &matchedIsoPeak, 
							   double &stdPPMTol, double &sumMatchedInt, double prePeakMz, bool &isIncluded)
{
	//PrecursorCandidatePeaks[k].MZ����߷��mz����ȡֵ������������ƫ���1Th���������ȫ��������20ppm��Ϊ���
	double TolValueTh = PrecursorCandidatePeaks[k].m_lfMZ * Peak_Tol_ppm;
	double totalPPMTol = 0.0; // ����14��ͬλ�ط��ƫ���
	sumMatchedInt = 0.0;
	vector<double> vminBias;  // ����ÿ��ͬλ�ط��ppmƫ��
	// wrm��ͬλ��ģʽ������15����
	for (int s = -HalfIsoWidth; s <= HalfIsoWidth; ++s)
	{
		double targetMZ = PrecursorCandidatePeaks[k].m_lfMZ + avgdiv * s / chg;
		if(fabs(targetMZ - prePeakMz) <= TolValueTh)  // ���ǰһ�����ڸ���߷��ͬλ�ط���У���ô����߷���ö��ǰһ��Ϊ��߷�ʱ�Ѿ����ǹ��ˡ�
		{
			isIncluded = true;
		}
		double minBias = 10.0;	
		// [wrm] �ں�ѡ�׷��б��в���ͬλ�ط壬�����Բ��ö��ֻ��ϣ������ȡƫ����С�Ǹ��壬������ǿ��
		for (int l = 0; l < (int)PrecursorCandidatePeaks.size(); ++l)
		{   // PrecursorCandidatePeaks��mzֵ����С�����
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
		if(minBias < 10 && s != 0)   // [wrm]�� s=0ʱ����ֱ��ʡȥһ��ѭ��
		{
			minBias = minBias / targetMZ * 1000000;
			vminBias.push_back(minBias);
			totalPPMTol += minBias;  // �����и�
			sumMatchedInt += IsoPattern[s + HalfIsoWidth];
		}
	}
	sumMatchedInt += IsoPattern[HalfIsoWidth];// ƥ���ϵ��׷�ǿ�Ⱥͣ������ټ�����߷��ǿ��
	matchedIsoPeak = 1 + (int)vminBias.size();// ƥ���ϵ��׷�ĸ���
	if(matchedIsoPeak > 1)
	{
		totalPPMTol /= (int)vminBias.size();//����ƥ���ϵ��׷��ƽ��ƫ�����û�п��Ǻ�����ⲿ�ֵ�ͬλ��ģʽ�����и�ʱҪɾ����һЩ�׷�
		stdPPMTol = 0.0;
		// [wrm]���ƥ�䵽�ķ�̫�ٵĻ��Ͳ�������ƽ��ƫ����У׼ƫ��  modified by wrm  2015.09.24.
		// TODO�������ò�����ƽ��ƫ����У׼ƫ�������д���һ��ʵ��
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

// ��ʼ��IsoPattern��������Ҫ��ŵ�ǰһ���׵���߷帽����ͬλ�ط��ǿ�ȣ��Լ�������ǰ�������һ�����ж�Ӧ��ͬλ�ط��ǿ��
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

// ������߷���������������ͬλ�ط��
void MS1Reader::GetTheoryIsoform(double mass, int &highestPeakPos, vector<double> &vTheoIso)
{
	vector<double> emptyVct;
	vTheoIso.swap(emptyVct);
	highestPeakPos = 0;

	int mIdx = int(mass);
	if(mIdx >= LIPV_MASSMAX || mIdx < 0)
	{
		return;
	} else if (mIdx < IPV_MASSMAX) {   // С��10K��ֱ�Ӷ�ȡ
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
	} else {              // ���ڵ���10K����������
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

	// ������������ IsoProfileNum (default 15)�����λ�ã�ʹ�м�Ϊ��߷�
	if(highestPeakPos == HalfIsoWidth) // �����7����
	{
		//ǡ�õ�HalfIsoWidth����Ϊ��߷壬���ܵ��׷���Ŀ���������Ҳಹ��
		if((int)vTheoIso.size() < IsoProfileNum)
		{
			vTheoIso.insert(vTheoIso.end(), IsoProfileNum - vTheoIso.size(), 0.0);
		} else if((int)vTheoIso.size() > IsoProfileNum) {
			vTheoIso.erase(vTheoIso.begin() + IsoProfileNum, vTheoIso.end());
		}
	} else if(highestPeakPos < HalfIsoWidth) { 
	    //��߷���ߵ��׷岻��������
		vTheoIso.insert(vTheoIso.begin(), HalfIsoWidth - highestPeakPos, 0.0);
		if((int)vTheoIso.size() > IsoProfileNum)
		{   //ɾ����߷��ұ߶�����׷�
			vTheoIso.erase(vTheoIso.begin() + IsoProfileNum, vTheoIso.end());
		} else if((int)vTheoIso.size() < IsoProfileNum){
			vTheoIso.insert(vTheoIso.end(), IsoProfileNum - vTheoIso.size(), 0.0);
		}
	} else if(highestPeakPos > HalfIsoWidth) {
		//��߷���ߵ��׷�̫�࣬ɾ����С�Ĳ���
		vTheoIso.erase(vTheoIso.begin(), vTheoIso.begin() + highestPeakPos - HalfIsoWidth);
		if((int)vTheoIso.size() < IsoProfileNum)
		{
			vTheoIso.insert(vTheoIso.end(), IsoProfileNum - vTheoIso.size(), 0.0);
		} else if((int)vTheoIso.size() > IsoProfileNum) {
			vTheoIso.erase(vTheoIso.begin() + IsoProfileNum, vTheoIso.end());
		}
	}
}

//�Ӹ����ļ�F:\Skydrive\coding\pParsesrc\IPV.txt�л�ȡƽ��������ģ�͵Ľ��������ŵ�ȫ�ֱ���IPV[10000][28]�С������д�������������ֵ���д���ͬλ��ģʽ��û�й�һ����
double MS1Reader::ComputIPVSimilarity(double &SumCurPrecursorInt, vector<double> &vTheoIso, double &IsoLenScore)
{
	// ������ͬλ�ط����������Щ�岻���ڣ�ʵ�����м���ǿ�Ⱥ�ʱ������Щ�����ڸ�ͬλ��ģʽ
	// [wrm]:�����ڼ�������ͬλ�طֲ�ʱ�Ѿ������˲�0���룬��vTheoIso.size() <= HalfIsoWidth����������
	if((int)vTheoIso.size() <= HalfIsoWidth || fabs(vTheoIso[HalfIsoWidth]) < eps)
	{
		return 0.0;
	}
	int isoLen = 0, valid = 0;
	for(int k = 0; k < IsoProfileNum; ++k)
	{    
		if(vTheoIso[k] / vTheoIso[HalfIsoWidth] < 0.05)  // [wrm��]:  0.05�����ֵ�ǴӺζ����� ǰ����׷���п��ܵ�����߷��5%��
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
	IsoLenScore = 1.0 * isoLen / valid; // ������ͬλ�ط��ƥ���ͬλ�ط��ٻ���

	
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
		// �����������������ϵ��
		double VectorCovariance = CStatistic::CalculateVectorCov(vTheoIso, V2);
		if (VectorCovariance > MaxSimilarity)
		{
			MaxSimilarity = VectorCovariance;
			BestIsoSimiScanIndex = j;//��һ����¼�����ͬλ��ģʽ���ƶȵĵط�
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
// �����������Ѵ����ڵĺ�ѡ�׷��ǿ���ܺ�
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


// ��������ֵ��ͬ��ĸ������������ͬ
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

// ĸ��������ִ��
void MS1Reader::ScorePrecursors(vector<vector<double> > &PrecursorsCandidatesArray)
{
	// Changed by luolan @2014.05.05 /////////////////////////////////////////////////
	for(int gSortByCol = 2; gSortByCol < 8; ++gSortByCol) // TODO: �淶������
	{   // ���д�ֶ��ɸߵ����������ﲻ�����֣�ǰ����ƴ��ʱ��Ҫע��
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
		
		//��ȡ���մ��
		for (unsigned int i = 0; i < PrecursorsCandidatesArray.size(); i++)
		{
			PrecursorsCandidatesArray[i][15] = 1;
			PrecursorsCandidatesArray[i][15] *= PrecursorsCandidatesArray[i][9];//maxSim
			//PrecursorsCandidatesArray[i][15] *= PrecursorsCandidatesArray[i][9];
			//PrecursorsCandidatesArray[i][15] *= PrecursorsCandidatesArray[i][9];
			//PrecursorsCandidatesArray[i][15] *= PrecursorsCandidatesArray[i][10];// peaksNum
			PrecursorsCandidatesArray[i][15] *= (PrecursorsCandidatesArray[i][11]); // ɫ�����ƶ�����
			PrecursorsCandidatesArray[i][15] *= (PrecursorsCandidatesArray[i][12]);//stdDeviation
			//PrecursorsCandidatesArray[i][15] *= (PrecursorsCandidatesArray[i][12]);
			//PrecursorsCandidatesArray[i][15] *= (PrecursorsCandidatesArray[i][13]);  // chroSim[0]
			PrecursorsCandidatesArray[i][15] *= (PrecursorsCandidatesArray[i][14]);//pif
			//PrecursorsCandidatesArray[i][15] *= (PrecursorsCandidatesArray[i][14]);
			//PrecursorsCandidatesArray[i][15] *= (PrecursorsCandidatesArray[i][14]);
			
		}
		
		CTools::gSortByCol = 15;
		// ������������С��������  ԽС����Խ��ǰ
		sort(PrecursorsCandidatesArray.begin(),PrecursorsCandidatesArray.end(), CTools::compPrecursorByCol);
}

// ȥ�����Ƶ�ĸ���ӣ� �ϲ�mz����Χ�ڵ��׷壬ǿ�ȵ��ӣ�mzȡ���߹�һ�������mz1*intens1 + mz2*intens2��/(intens1+intens2)
void MS1Reader::CutSamePrecursor(vector<CentroidPeaks> &PrecursorCandidatePeaks)
{
	if (0 == PrecursorCandidatePeaks.size())
	{
		return;
	}
	double tol=PrecursorCandidatePeaks[0].m_lfMZ*Peak_Tol_ppm;  // �������
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



// �Ը����Ķ����ף��ҵ����ѡĸ���ӣ����������е����������дִ�ֱ���ǰ����
// Called by CpParseFlow::FlowKernel
void MS1Reader::AssessPrecursor(double ActivationCenter,int MS2scan,int MS1scan,vector<CentroidPeaks> &PrecursorCandidatePeaks,TrainingSetreader &TrainSet)
{
	bool filter_noMixSpec = false;
	if (atoi((m_para->GetValue("co-elute")).c_str()) == 0)
	{
		filter_noMixSpec = true;  // �����������
	}	
	int chargeStart = 3;
	// ����mz�Ժ�ѡ�׷��������
	sort(PrecursorCandidatePeaks.begin(), PrecursorCandidatePeaks.end(), CTools::compLessMZ);

	// Cut similiar precursors
	if (m_para->GetValue("cut_similiar_mono") == "1")
	{
		CutSamePrecursor(PrecursorCandidatePeaks);   // �ϲ���ѡ��߷����ڽ����׷�
	}
	int lenVec = (int)PrecursorCandidatePeaks.size();
	
	// �����Ѵ��ڣ���չ0.5Da���ڵ������׷尴��mz�������򣬱��ں���������߷徫��
	sort(PeakRecord.begin(), PeakRecord.end(), CTools::compLessMZ);
	
	vector<vector<double> > PrecursorsCandidatesArray;//��Ÿ��������׵�����ĸ����
	vector<double> PrecursorsCandidatesItem;//���ĸ���Ӻ�ѡ
	int PrecursorsCounter = 0;
	double IsolationWidth = GetIsolationWidth(); //��������һ��Halfwindow��������ƣ�������ȥ�������ļ���������ʵ���������Ҫע�� -luolan
	//cout<<"isolation width: "<<IsolationWidth<<endl;
	// Get peak intensity sum in the isolation window for scale down other intensity measure.
	// Sum intensity recorded in the Isopattern Array from index 6 to 66.
	// �����������Ѵ���(������չ0.5Da)�ڵĺ�ѡ�׷��ǿ�Ⱥ�
	double dIntensitySumInIsolationWindow = GetPeakInIsolationWindow(PrecursorCandidatePeaks,ActivationCenter,IsolationWidth);

	double prePeakMz = -1.0;
	double preScore = 0.0;
	bool isIncluded; // ���ο���ÿ������Ϊ��߷�ʱ��������ͬ��ͬλ�ط���е��׷壬ֻ����һ����õ�
	const int chgEnd = atoi(m_para->GetValue("max_charge", "30").c_str());
	const double limitMass = atof(m_para->GetValue("max_mass", "50000").c_str());  // wrm: ĸ�����������

	for (int k = 0; k < lenVec-1; k++) //ö��ÿ����Ϊ��߷�����
	{
		//if(k == 212)
		//	cout<<"k: "<<k<<endl;
		double bestScore = 0.0;  // �ִ��
		double bestChg = 0.0;    // ĸ���ӵ��
		double bestSim = 0.0;
		double bestSTD = 0.0;    // ��߷徫�ȣ���߷��ʺɱ�������10����ͼ�еı�׼��
		double bestChroSim1 = 0.0;
		double bestChroSim2 = 0.0;
		double bestTolScore = 0.0;
		double bestIsoPeakNum = 0.0; // ���Ѵ�����ͬλ�ط���Ŀ
		double bestMonoRevInts = 0.0; // ��ͬλ�ط����ǿ�Ȳ������ʵ�飩
		double bestMonoMz = 0.0;  // ĸ��������
		double bestIsoLen = 0.0;  // ͬλ��ģʽ���ȣ�������ͬλ�ط��ƥ���ͬλ�ط���Ŀ
		double bestPIF = 0.0;     // ���Ѵ������׷�ǿ�ȱ�

		// Initialize the PrecursorsCandidatesItem
		PrecursorsCandidatesItem.resize(0);

		//����ÿһ����ѡ��߷�M/Zö�ٵ�ɣ���3��ɿ�ʼ
		for (int chg = chargeStart; chg <= chgEnd; chg++)
		{	
			InitIsoPattern(); // Initializing the Array IsoPattern[]

		    int IsoPeakInWindow = 0;  // ��ѡ�׷��б��е�ͬλ�ط���Ŀ
			double stdPPMTol = 0.0;
			double SumCurrentPrecursor = 0.0;	// �ӻ�ѡ�׷��б��л�ȡ��ͬλ�ط�ǿ�Ⱥ�
			isIncluded = false;

			// ������߷�͵�ɻ�ȡͬλ�ط�أ�������15���壩
			GetEachIsoPeak(PrecursorCandidatePeaks, chg, k, IsoPeakInWindow, stdPPMTol, SumCurrentPrecursor, prePeakMz, isIncluded);
			//if(IsoLen / IsolationWidth < 0.5)
			//{   //��ƥ���ϵ��׷廹����һ�룬��ֱ�ӹ��˵��ú�ѡĸ����
			//	continue;
			//}

			// ��ȡ������߷�͵���µ�ǰ��10��һ�����е�ͬλ��ģʽ [wrm ?] �ӻ�ȡ��ѡ��ʱ���׷��bin�������ڲ�ѯͬλ��ʱ�����ϲ�bin�������ж��±�ǿ��תΪΪint�Ĵ���᲻����ɺܴ���
			// Fill in the Isopattern Array from the 10 related spectrum.
			GetAllIsoFeature(chg, ActivationCenter, PrecursorCandidatePeaks, k);
					
			// Isopattern Array index 15-165 is come from the peaks candidates array
			double sumIsopattern = GetSumIsoPattern();
			
			// [wrm ?]: �׷�ǿ�ȹ�һ����Ϊʲô����10�����е�ǿ�Ⱥ�����һ������������ÿ������ͬλ�ط��ǿ�Ⱥͷֱ��һ����
			// For each peak in the Isopattern Array with index 16 to 165. Devide by the sum intensity
			ScaledIsoPattern(sumIsopattern);
			
			//CutIsotopicPeak(IsoLen, SumCurrentPrecursor);// comment by luolan @2014.05.16
			
			// * ����ɫ�����ƶ�
			CalcChromSimilarity();
			int highestPeakPos = 0;
			vector<double> vTheoIso;
			//cout << PrecursorCandidatePeaks[k].m_lfMZ << endl;
			// ��������ͬλ�طֲ�
			GetTheoryIsoform(PrecursorCandidatePeaks[k].m_lfMZ*chg - chg*pmass, highestPeakPos, vTheoIso);
			if(0 == vTheoIso.size()) continue;
			// ��������ͬλ�طֲ�����߷�λ�ü���Mono����
			double monoMass = PrecursorCandidatePeaks[k].m_lfMZ - highestPeakPos * avgdiv / chg;
			if(monoMass > limitMass)
			{
				continue;
			}

			// * ���������ͬλ�ط����ǿ�Ȳ�(MonoInts)������ʵ����mono������ǿ�ȣ��������߷壩�����۵����ǿ��֮��
			int monoIdx = HalfIsoWidth - highestPeakPos;
			if(monoIdx < 0)  // �����15��ʵ���׷���û��Mono�壬Ĭ�ϵ�һ��ΪMono��
			{
				monoIdx = 0;
			}
			double expRelativeInts = 0.0;
			if(IsoPattern[HalfIsoWidth] > eps) // ��߷�
			{
				expRelativeInts = IsoPattern[monoIdx] / IsoPattern[HalfIsoWidth];  // ����ʵ������Mono����߷�����ǿ�Ȳ�
			}
			if(expRelativeInts > 1.0) expRelativeInts = 1.0;
			double theoRelativeInts = 0.0;
			if(vTheoIso[HalfIsoWidth] > eps)
			{
				theoRelativeInts = vTheoIso[monoIdx] / vTheoIso[HalfIsoWidth];   // ������������Mono����߷�����ǿ�Ȳ�
			}
			double tmpMonoDiv = 0.0;
			if(expRelativeInts > theoRelativeInts)
			{
				tmpMonoDiv = (expRelativeInts - theoRelativeInts) / expRelativeInts;
			} else {
				tmpMonoDiv = (theoRelativeInts - expRelativeInts) / theoRelativeInts;
			}
			//cout << monoMass << " " << chg << " " << expRelativeInts << " " << theoRelativeInts << " " << tmpMonoDiv << endl;

			
			// * Important Function ����������ʵ��ͬλ�ط�ص����ƶ�
			double IsoLenScore = 0.0;  // ������ͬλ�ط��ƥ���ͬλ�ط��ٻ��� isoLen / theoValid
			// ��ѡ�׷��б��ж�Ӧ��ͬλ�ط�� + 10��һ�����ж�Ӧ��ͬλ�ط�أ�����������ͬλ�طֲ����ƶ�����ֵ
			double MaxSim = ComputIPVSimilarity(SumCurrentPrecursor, vTheoIso, IsoLenScore);
			
			if(chg > 3 && IsoLenScore < 0.5)  // ���ƥ�䵽���׷���ĿС��һ�룬����ȥ��ĸ����
			{
				continue;
			}
			// * �������(SimIso2)��һ������ͬһ���ӵĲ�ͬ���״̬��Ϣ 2014.09.24
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
			double PIF = 0.0;   // ���Ѵ������׷�ǿ�ȱ�
			if (dIntensitySumInIsolationWindow != 0 )  // ��������ƥ���ͬλ�ط�غ�/���Ѵ����ں�ѡ�׷�ǿ�Ⱥ�
			{
				PIF = SumCurrentPrecursor / dIntensitySumInIsolationWindow;
				if(PIF > 1.0) PIF = 1.0;
				//PIF /= (0.1 + IsoPeakInWindow);
			}


			// TODO ����ע�Ͳ���ΪһЩ��ɢ�Ĺ���������������Ҫ����TD���ص��ٶ�����ν��п��ٹ��ˣ���ó����һ������ -luolan
			
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
			if(chg < 4 && MaxSim < 0.8) // ���Ƶ͵��
			{
				continue;
			}
			if(chg > 20 && MaxSim < 0.6) // ���Ƹߵ��
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
			// �����Ѵ������׷�ǿ�ȱȡ�ͬλ��ģʽ���ƶȺ��׷徫�������дִ�� stdPPMTolΪ����ƫ��ͣ��� > 0
			double tmpScore = PIF + MaxSim + (20.0 - stdPPMTol) / 20.0;  
#ifdef _DEBUG_TEST2
			if(MS2scan == 2732)
			{
				cout<<highestPeakPos<<" "<<PrecursorCandidatePeaks[k].m_lfMZ<<" "<<chg<<" "<<IsoPeakInWindow<<" "<<PIF<<" "<<MaxSim<<" "<<chromMaxSim<<" "<<tmpScore<<endl;
			}
#endif
			if(tmpScore > bestScore) // ��¼��߷ֵ�����ֵ�����������ߵĵ������
			{
				bestScore = tmpScore;    // �ִ��
				bestChg = (double)chg;   // * ���
				bestSim = MaxSim;        // * ͬλ��ģʽ���ƶ�
				bestSTD = CalcSTDforMZ( PrecursorCandidatePeaks[k].m_lfMZ );  // * ��߷徫��
				bestChroSim1 = chromMaxSim;  // * ɫ�����ƶ�
				bestChroSim2 = closeChgSim;  // * ���ڵ��״̬ͬλ��ģʽ���ƶ�
				bestTolScore = 20.0 - stdPPMTol;  // * ͬλ��ģʽƫ��
				bestIsoPeakNum = IsoPeakInWindow; // * ͬλ�ط���Ŀ
				bestMonoRevInts = 1 - tmpMonoDiv; // test 2014.10.08  * ��ͬλ�ط����ǿ�Ȳ�
				bestIsoLen = IsoLenScore;    // * ͬλ��ģʽ���ȷ�ֵ
				bestPIF = PIF;   // * ���Ѵ������׷�ǿ�ȱ�

				double AccMZ = ReCalcMZ( PrecursorCandidatePeaks[k].m_lfMZ );  // ��߷�����У׼
				//��������ͬλ�ط����߷�λ������ʵ��ͬλ�ط�ص�mono��λ��
				bestMonoMz = AccMZ - highestPeakPos * avgdiv / chg;  // * Mono��Ĺ�������
			}
		}//for charge

		if(bestScore < eps)// bestScore == 0.0
		{
			continue;
		}

		if(isIncluded && bestScore <= preScore)
		{
			continue;
		} else if(isIncluded){  // �����׷���ʹ�ù������������ߵ��Ǹ�
			prePeakMz = PrecursorCandidatePeaks[k].m_lfMZ;
			preScore = bestScore;
			int last = (int)MarsFeature.size() - 1;
			if(last < 0)
			{
				continue;
			}
			// ���е���������һ��
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
			PrecursorsCandidatesArray[last][5] = bestTolScore;//5  //��ʵ��ͬλ�ط�ص�ƫ����Ϊһά��������Ҫ�����������ڵĸߵ�ɣ����Խ��Խ�� -luolan
			PrecursorsCandidatesArray[last][6] = bestChroSim2;//6
			PrecursorsCandidatesArray[last][7] = bestPIF;//7
		} else {  // 
			prePeakMz = PrecursorCandidatePeaks[k].m_lfMZ;
			preScore = bestScore;
			vector<double> MarsFeatureTmp;
			vector<double> OutputInfoTmp;
			MarsFeatureTmp.push_back((bestChg * 1.0) / chgEnd);  // ���
			MarsFeatureTmp.push_back(bestSim);         // ͬλ��ģʽ���ƶ�
			MarsFeatureTmp.push_back(bestSTD / 20.0);  // ��߷徫��
			MarsFeatureTmp.push_back(bestChroSim1);    // ɫ�����ƶ�
			MarsFeatureTmp.push_back(bestTolScore / 20.0);  // ͬλ��ģʽƫ��
			MarsFeatureTmp.push_back(bestChroSim2);         // ���ڵ��״̬��ͬλ�ط�����ƶ�
			MarsFeatureTmp.push_back((bestIsoPeakNum * 1.0) / IsoProfileNum); // ͬλ�ط���Ŀ
			MarsFeatureTmp.push_back(bestMonoRevInts);    // ��ͬλ�ط����ǿ�Ȳ�
			MarsFeatureTmp.push_back(bestMonoMz / 2000.0);  // ĸ��������
			MarsFeatureTmp.push_back(bestIsoLen);  // ͬλ��ģʽ���ȴ��
			MarsFeatureTmp.push_back(bestPIF);     // ���Ѵ������׷�ǿ�ȱ�
			MarsFeatureTmp.push_back(bestScore);   // �ִ��ֵ

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
			//��ʵ��ͬλ�ط�ص�ƫ����Ϊһά��������Ҫ�����������ڵĸߵ�ɣ����Խ��Խ�� -luolan
			PrecursorsCandidatesItem.push_back(bestChroSim2);//6�����������׷塿
			PrecursorsCandidatesItem.push_back(bestPIF);//7
			PrecursorsCandidatesItem.push_back(PrecursorsCounter);//8
			PrecursorsCandidatesItem.push_back(0);//MaxSimScore 9
			PrecursorsCandidatesItem.push_back(0);//STDScore 10
			PrecursorsCandidatesItem.push_back(0);//EluteScore 11
			PrecursorsCandidatesItem.push_back(0);//intenstiyScore 12
			PrecursorsCandidatesItem.push_back(0);//Elute Score Isotope 0 13
			PrecursorsCandidatesItem.push_back(0);//PIF 14
			PrecursorsCandidatesItem.push_back(0);//FinalScore 15
			//�ɹ���¼��һ��ĸ���ӡ�
					
			//bCorrectSample=TrainSet.CheckScanMZCharge(MS2scan,PrecursorCandidatePeaks[k].MZ,chg);
				
			PrecursorsCandidatesArray.push_back(PrecursorsCandidatesItem);
		}

	}//for peak


	// Not any precursor candidates.
	if (PrecursorsCandidatesArray.size()==0)
	{
			return;
	}
	// Ranking Score, Then keep top N precursors. ��ĸ���ӽ�������ִ��
	ScorePrecursors(PrecursorsCandidatesArray);
	int OutputTopN = 10;
	if(filter_noMixSpec)  // �����������
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

	if ((int)PrecursorsCandidatesArray.size() > OutputTopN)  // Ԥ����̭����ȷ��ĸ����
	{
		PrecursorsCandidatesArray.erase(PrecursorsCandidatesArray.begin() + OutputTopN, PrecursorsCandidatesArray.end());	
		//PrecursorSumAll += OutputTopN > (int)PrecursorsCandidatesArray.size() ? (int)PrecursorsCandidatesArray.size() : OutputTopN;
	}
	
	//if (PrecursorsCounter <= OutputTopN - 1) // comment by luolan 2014.09.16����������ĸ��������ָ����Ŀʱ��outputInfo��MarseFeature����ĺ�ѡĸ����˳��������ִ�������Ĳ�һ��
	//{
	//	return;
	//}
	//cout<<"MarsFeature.size "<<MarsFeature.size()<<endl;
	// MarsFeature�д�������ж����׵�ĸ���ӣ�PrecursorsCandidatesArray����˵�ǰ�����׵�����ĸ����
	int DeleteStart = MarsFeature.size() - PrecursorsCounter;  
	
	int MarsLen = MarsFeature.size();
	int outputLen = OutputInfo.size();
	
	// MarsFeature.size() - PrecursorsCounter��MarsFeature��ǰ���Ϊ����MS2��ĸ���ӣ��Ƚ��ⲿ���д�ָߵ�������push��ȥ��Ȼ��ǰ���ⲿ��ȫ��ɾ��
	for (unsigned int Precursor_i = 0; Precursor_i < PrecursorsCandidatesArray.size(); Precursor_i++)
	{
		MarsFeature.push_back( MarsFeature[MarsLen-PrecursorsCounter+(int)PrecursorsCandidatesArray[Precursor_i][8]-1]);
		OutputInfo.push_back(OutputInfo[outputLen-PrecursorsCounter+(int)PrecursorsCandidatesArray[Precursor_i][8]-1]);
	}
	
	MarsFeature.erase(MarsFeature.begin()+DeleteStart,MarsFeature.begin()+DeleteStart+PrecursorsCounter);
	
	OutputInfo.erase(OutputInfo.begin()+DeleteStart,OutputInfo.begin()+DeleteStart+PrecursorsCounter);

	//cout<<OutputInfo[0][1]<<" "<<OutputInfo[0][2]<<endl;
}

// ���ͬһĸ�������ڵ��״̬��ͬλ�ط��
// param: ������scan,��ǰĸ���ӵ��,��ǰ����µ�Mono����,����ͬλ�طֲ�
double MS1Reader::CheckCloseChargeState(int scanNo, int precursorScan, int curChg, double curMz, vector<double> &vTheoIso)
{
	// ��ȡ�ú�ѡĸ�������ڵ��״̬���׷���Ϣ
	vector<CentroidPeaks> vPeaksTmp; 
	double mass = curMz * curChg;   // ĸ��������
	double closeMz = (mass + pmass) / (curChg + 1);  // ��curChg+1���ʱ��ĸ����mz

	int ms1Index = -1;
	// �Ѿ��洢��ÿ�Ŷ����׶�Ӧ��һ���� modified by wrm. 2015.10.10
	// GetIndex_By_MS2Scan(ms1Index, scanNo);  // [TODO] [wrm]: Ϊÿ�Ŷ�������ǰ�洢���Ӧ��һ���ף�������ÿ�β���
	ms1Index = MS1Scan_Index[precursorScan];


	if(-1 == ms1Index)
	{
		return 0.0;
	}

	GetMS1PeakbyMass(closeMz, curChg + 1, ms1Index, vPeaksTmp);

	vector<double> vCloseIso(vTheoIso.size(), 0);

	int highest = -1;  // ��¼��߷��λ��
	double highesPeak = 0;
	for(size_t i = 0; i < vPeaksTmp.size(); ++i)
	{
		if(vPeaksTmp[i].m_lfIntens > highesPeak)
		{
			highesPeak = vPeaksTmp[i].m_lfIntens;
			highest = i;
		}
	}
	int idx = HalfIsoWidth; // ����߷�Ϊ��׼����ʵ���׷�
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
	// �����������׷�����ƶ�
	double cov1 = CStatistic::CalculateVectorCov(vTheoIso, vCloseIso);

	vector<CentroidPeaks> emptyVctCPeak;
	vPeaksTmp.swap( emptyVctCPeak );
	vector<double> tmpVct(vTheoIso.size(), 0);
	vCloseIso.swap( tmpVct );
	closeMz = (mass - pmass) / (curChg - 1);  // ��curChg-1���ʱ��ĸ����mz. [wrm] from + to - modified by wrm 20150929.  

	GetMS1PeakbyMass(closeMz, curChg - 1, ms1Index, vPeaksTmp);


	highest = -1; // ��¼��߷��λ��
	highesPeak = 0;
	for(size_t i = 0; i < vPeaksTmp.size(); ++i)
	{
		if(vPeaksTmp[i].m_lfIntens > highesPeak)
		{
			highesPeak = vPeaksTmp[i].m_lfIntens;
			highest = i;
		}
	}
	idx = HalfIsoWidth; // ����߷�Ϊ��׼����ʵ���׷�
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
	// �����������׷�����ƶ�
	double cov2 = CStatistic::CalculateVectorCov(vTheoIso, vCloseIso);
	// ���������������״̬���������׵�������ƶ�
	double maxSim = cov1 >= cov2 ? cov1 : cov2;
	return maxSim;
}

// ����ĸ����mz����ɡ�MS1scan��ȡͬλ�ط��
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
// ����ѧϰ����ĸ���ӽ��з���
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

// ���ڸ������׷��������У׼��������PeakRecordʵ�ּ�¼�˴������׷壬��ʱ����ȡ����ĸ���������йصĲ��֣�����Intensity��Ȩƽ�����д���
// [wrm?]:�б�Ҫ����߷���������У׼�� 
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
//��һ����������μ�������׷�ķ��������ĸ���ӵľ��ȸ��ߡ���һ��û���ڱ�������п�������������˵���������Ŵ�Ҷ����ᷴ�ԡ�
//��ʵ���������Ĳ�����STD�����ص�ʱ����΢����һ��仯�������һ���������������Ķ�������λ��ppm.
// ������߷��mz������������10����ͼ�еı�׼��
double  MS1Reader::CalcSTDforMZ(double oldMZ)
{
	double tol = oldMZ * Peak_Tol_ppm;
	double LeftLimit = oldMZ - tol;
	double RightLimit = oldMZ + tol;
	double ProductSum = 0;
	double IntensitySum = 0;
	double SumXX = 0;
	double FindPeakNum = 0;
	// PeakRecord�м�¼������10����ͼ�����Ѵ����ڵ������׷�
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
//������ϸ˼�����о�Ҳ����Բ�������ϣ�ֱ�Ӽ�����������ĵ�MS1Scan���Ǹ�Scan�����ڽ��м��㣬Ҳ����á����߿���ֱ�Ӵջ������е�������ƶȼ��㹤�ߡ�
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

	//cout << "�и�����" <<endl;	
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
	//����ͬλ��ģʽ������ǿ�ȵľֲ���С����нضϣ����ͬλ��ģʽ�ص����⡣����
	//��TD�ϣ�����߷�������Ѱ�ң��ҵ��ֲ���Сֵ֮��ֱ����˽ضϣ����ǿ��ܴ���©��ʱ��ÿ���ܹ�����һ���ֲ���Сֵ
	vector<int> LocalMinimal;
	vector<double> Data(IsoPattern);//,IsoPattern+sizeof(IsoPattern)/sizeof(double));
	bool hasMin = CStatistic::FindLocalMinimal(LocalMinimal, Data);
	if(!hasMin)
	{
		return;
	}
	int StartIsotopeIndex = 0;
	int EndIsotopeIndex = IsoProfileNum - 1;

	// ��ɨ����߷��Ҳ�ļ�Сֵ��
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
			{   // ���˽ض���Сֵ�⣬���ض���Щ����߷�߳�һ�����׷�
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

	// ��ɨ����߷����ļ�Сֵ��
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
			{   // ���˽ض���Сֵ�⣬���ض���Щ����߷�߳�һ�����׷�
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
