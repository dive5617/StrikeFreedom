#include "MS1Spectrum.h"
#include "Parameters.h"

MS1Spectrum::MS1Spectrum(int CurrentMS1Scan)
{
	nCurrentMS1Scan = CurrentMS1Scan;
	RetentionTime = 0.0;
	Baseline = 0.0;
}

MS1Spectrum::~MS1Spectrum()
{
	SpectrumTitle.clear();
	m_InstrumentType.clear();
	Peaks.clear();
	LogInten.clear();
}

void MS1Spectrum::SetRetentiontime(double dRetentiontime)
{
	RetentionTime = dRetentiontime;
}
void MS1Spectrum::SetInstrumentType(string InstrumentType)
{
	m_InstrumentType = InstrumentType;
}
//void MS1Spectrum::PrintInfo()
//{
//	cout << GetCurrentMS1Scan() << endl;
//}
void MS1Spectrum::AppendPeaks(double MH, double Intensity)
{
	Peaks.push_back(CentroidPeaks(MH, Intensity));
}

int MS1Spectrum::GetPeakNum()
{
	return Peaks.size();
}
int MS1Spectrum::GetCurrentMS1Scan()
{
	return nCurrentMS1Scan;
}
double MS1Spectrum::GetBaseline()
{
	return Baseline;
}
double MS1Spectrum::GetRetiontime()
{
	return RetentionTime;
}

// wrm�������׷壬���� MZ ���Ұ봰�ڵ��׷壻 �׷���ҿ��Ż�
// Ĭ���׷�����
void MS1Spectrum::GetPeaksInWindow(double MZ, double Halfwindow, vector<CentroidPeaks> &PeakList)
{
	for (unsigned int i = 0; i < Peaks.size(); i++)
	{
		if (Peaks[i].m_lfMZ < MZ - Halfwindow)
		{
			continue;
		} else if (Peaks[i].m_lfMZ > MZ + Halfwindow) {
			break;
		} else {
			PeakList.push_back(Peaks[i]);
		}
	}
	//int left = 0, right = Peaks.size()-1;
	//int pos = 0;
	//while(left < right){
	//	int mid = (left +right) >> 1;
	//	if(Peaks[mid].m_lfMZ < MZ - Halfwindow){
	//		left = mid + 1;
	//	}else if(Peaks[mid].m_lfMZ > MZ + Halfwindow){
	//		right = mid - 1;
	//	}else{  // fabs(Peaks[mid].m_lfMZ - MZ) <= halfwindow
	//		pos = mid;
	//		break;
	//	}
	//}
	//if(pos == 0)  pos = left;
	//for(int i = pos; i >= 0; -- i){
	//	if(fabs(Peaks[i].m_lfMZ - MZ) > Halfwindow){
	//		break;
	//	}else{
	//		PeakList.push_back(Peaks[i]);
	//	}
	//}
	//for(int i = pos+1; i < Peaks.size(); ++ i){
	//	if(fabs(Peaks[i].m_lfMZ - MZ) > Halfwindow){
	//		break;
	//	}else{
	//		PeakList.push_back(Peaks[i]);
	//	}
	//}
}

// ����ĸ����Mono mz����ɡ�ĸ��������ڵ�ǰһ�����л�ȡͬλ�ط�
// [TODO][wrm]�����ҹ��̻�����Ż�
void MS1Spectrum::GetPeaksbyMass(double MZ, int chg, vector<CentroidPeaks> &PeakList, double Peak_Tol_ppm)
{
	vector<CentroidPeaks> emptyVct;
	PeakList.swap(emptyVct);
	double curMz = MZ;
	double massTol = curMz * Peak_Tol_ppm;
	double addMz = 1.0 / chg;  // �׷���
	double maxLen = 1.0;
	//cout << "** " << Peaks.size() << endl;
	for (size_t i = 0; i < Peaks.size(); ++i)
	{
		
		if (Peaks[i].m_lfMZ < curMz - massTol)
		{
			continue;
		} else if(Peaks[i].m_lfMZ <= curMz + massTol) {  
			//cout << i << endl;   // [wrm?]: ���x1��x2������Χ������Mono�����ڽ�x1ѹ����к�curMZ���ˣ�x2����Χ����curMz��ƥ��...�������Ҳ�����һͬλ�ط�
			PeakList.push_back(Peaks[i]);
			curMz += addMz;     // ������ǰ
			massTol = curMz * Peak_Tol_ppm;
		} else if (Peaks[i].m_lfMZ > MZ + maxLen) { // �׷��Ѱ���mz����
			break;
		}
	}
}


// ������������
// ������ͼ�������кܶ�Ͱ���������
// wrm: ����ֻ�Ǽ������������ߣ���û���������ˣ�
void MS1Spectrum::FilterByBaseline()
{
	if((int)Peaks.size() <= 1) return;
	// �����е��׷�ǿ��ȡlog��洢��LogInten��
	LogInten.reserve(Peaks.size()+1);
	double MaxIntensity = 0; 
	double MinIntensity = 100;
	unsigned int i = 0;
	for (i = 0; i < Peaks.size(); i++)
	{
		LogInten.push_back(log10(Peaks[i].m_lfIntens));
		if (LogInten[i] < MinIntensity)
		{
			MinIntensity = LogInten[i];
		}
		if (LogInten[i] > MaxIntensity)
		{
			MaxIntensity = LogInten[i];
		}
	}
	const double distIntens = 0.08;
	// ��Bin
	vector<double> SliceBin;
    SliceBin.push_back(distIntens + MinIntensity);    //[wrm] distIntens / 2 + MinIntensity wrm20150916: ��һ��bin�Ĵ�СΪʲô�ͺ��治һ����
	i = 0;
	while (SliceBin[i] + distIntens < MaxIntensity)
	{
		SliceBin.push_back(SliceBin[i] + distIntens);
		i++;
	}
	SliceBin.push_back(SliceBin[i] + distIntens);
	//Init
	unsigned int SizeofSliceBin = SliceBin.size();
	vector<int> HistCounter;
	HistCounter.assign(SizeofSliceBin + 1, 0);

	for (i = 0; i < LogInten.size(); i++)
	{
		if (LogInten[i] <= SliceBin[0])
		{
			++HistCounter[0];
		} else if (LogInten[i] > SliceBin[SizeofSliceBin-1]) {
			++HistCounter[SizeofSliceBin];
		} else {
			++HistCounter[(unsigned int)ceil((LogInten[i] - SliceBin[0]) / distIntens)];  // ����ȡ��
		}
	}

	int MaxIndex = -1;
	double MaxLoghist = 0;
	for (i = 0; i < SizeofSliceBin + 1; ++i)
	{
		if (MaxLoghist < HistCounter[i])
		{
			MaxIndex = i;
			MaxLoghist = HistCounter[i];
		}
	}
	Baseline = pow(10, SliceBin[MaxIndex] - distIntens / 2);  // ����֮ǰ���׷�ǿ��ȡ��log10������Ҫ��ԭ����
}

