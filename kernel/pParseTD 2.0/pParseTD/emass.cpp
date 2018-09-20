#include <iostream>
#include <fstream>
#include <string>
#include <vector>

#include "emass.h"

//#define _DEBUG_EMASS1

using namespace std;


CEmass::CEmass(vector<unordered_map<int,vector<CentroidPeaks>>>& vIPV)
	:m_vIPV(vIPV)
{
	m_nBase = 10000;
	Init();
}

CEmass::~CEmass()
{
	m_vMass.clear();
	m_vAveraine.clear();
	m_vIsotope.clear();
}

// 
void CEmass::Init()
{
	// C, H, N, O, S
	double MonoMass[m_nElemNum] = {12, 1.0078246, 14.0030732, 15.9949141, 31.972070};
	m_vMass.assign(MonoMass, MonoMass + 5);

	double Model[m_nElemNum] = {4.9384, 7.8602, 1.2758, 1.4202, 0.03168};  // tte数据库统计得出的平均氨基酸分子式的下标
	// [wrm] 
	//double Model[m_nElemNum] = {4.9384, 7.7583, 1.3577, 1.4773, 0.0417};  // 经典的PIR数据库统计得出的平均氨基酸分子式的下标
	m_vAveraine.assign(Model, Model + 5);

	m_lfAvgMass = 0.0;   // 平均氨基酸质量
	for (int i = 0; i < m_nElemNum; ++i)
	{
		m_lfAvgMass += m_vAveraine[i] * m_vMass[i];

	}

	vector<CentroidPeaks> tmp; // C
	tmp.push_back(CentroidPeaks(12, 0.988930));
	tmp.push_back(CentroidPeaks(13.0033554, 0.011070));
	m_vIsotope.push_back(tmp);

	tmp.clear(); // H
	tmp.push_back(CentroidPeaks(1.0078246, 0.99985));
	tmp.push_back(CentroidPeaks(2.0141021, 0.00015));
	m_vIsotope.push_back(tmp);

	tmp.clear(); // N
	tmp.push_back(CentroidPeaks(14.0030732, 0.996337));
	tmp.push_back(CentroidPeaks(15.0001088, 0.003663));
	m_vIsotope.push_back(tmp);

	tmp.clear(); // O
	tmp.push_back(CentroidPeaks(15.9949141, 0.997590));
	tmp.push_back(CentroidPeaks(16.9991322, 0.000374));
	tmp.push_back(CentroidPeaks(17.9991616, 0.002036));
	m_vIsotope.push_back(tmp);

	tmp.clear(); // S
	tmp.push_back(CentroidPeaks(31.972070, 0.9502));
	tmp.push_back(CentroidPeaks(32.971456, 0.0075));
	tmp.push_back(CentroidPeaks(33.967866, 0.0421));
	tmp.push_back(CentroidPeaks(35.967080, 0.0002));
	m_vIsotope.push_back(tmp);


	m_vBaseIsotope.clear();
	vector<CentroidPeaks> baseIso;
	m_vBaseIsotope.push_back(baseIso); // 0

	vector<int> pattern;
	GetFormbyAvegine((double)m_nBase, pattern);  // 求10000的平均氨基酸分子式
	// 计算平均氨基酸分子的同位素峰簇
	for(size_t i = 0; i < pattern.size(); ++i)  // C/H/N/O/S下标
	{
		//cout << pattern[i] << endl;
		if(pattern[i] == 0) continue;
		vector<CentroidPeaks> oneElemRes;
		Coumpound(pattern[i], m_vIsotope[i], oneElemRes);
		vector<CentroidPeaks> res;
		Convolution(oneElemRes, baseIso, res);
		baseIso.swap(res);
		Prunnig(baseIso, 1e-10);
	}
	m_vBaseIsotope.push_back(baseIso); // 10k
	/*for(int j = 0; j < baseIso.size(); ++j)
	{
		cout << baseIso[j].m_lfMZ << " " << baseIso[j].m_lfIntens << endl;
	}*/

	int cnt = LIPV_MASSMAX / m_nBase;
	for(int i = 1; i < cnt; ++i)
	{
		baseIso.clear();
		Convolution(m_vBaseIsotope[1], m_vBaseIsotope[i], baseIso);
		m_vBaseIsotope.push_back(baseIso);  // 20K,30K,40K,50K,60K,70K,80K,90K,100K

		/*cout << m_nBase * (i + 1) << endl;
		for(int j = 0; j < baseIso.size(); ++j)
		{
			cout << baseIso[j].m_lfMZ << " " << baseIso[j].m_lfIntens << endl;
		}*/
	}
}
// 已知元素数目num，及其同位素，获取num个元素的同位素峰 (E)^num
void CEmass::Coumpound(const int num, const vector<CentroidPeaks> &com, vector<CentroidPeaks> &res)
{
	res.push_back(CentroidPeaks(0, 1));
	if(num == 0) return;
	vector<CentroidPeaks> nutralCom = com;
	vector<CentroidPeaks> tmp;
	//unsigned int bit = 1;
	// [wrm]: 借用矩阵快速幂的思想
	// lluo version
	/*while(1)     
	{
		if(num < bit) break;
		if(num & bit)
		{
			Convolution(nutralCom, res, tmp);
			res.swap(tmp);
		}
		Convolution(nutralCom, nutralCom, tmp);
		nutralCom.swap(tmp);
		bit <<= 1;
	}*/
	// modified by wrm 2016.03.14, to reduce the time of convolution
	int n = num;
	while((n&1) == 0){
		n >>= 1;
		Convolution(nutralCom, nutralCom, tmp);
		nutralCom.swap(tmp);
	}
	res = tmp;
	n >>= 1;
	while(n){
		Convolution(nutralCom, nutralCom, tmp);
		nutralCom.swap(tmp);
		if(n&1){
			Convolution(nutralCom, res, tmp);
			res.swap(tmp);
		}
		n >>= 1;
	}
}

void CEmass::Convolution(const vector<CentroidPeaks> &comA, const vector<CentroidPeaks> &comB, vector<CentroidPeaks> &res)
{
	vector<CentroidPeaks> emptyVec;
	res.swap(emptyVec);
	if(0 == comA.size())
	{
		res = comB;
		return;
	}
	if(0 == comB.size())
	{
		res = comA;
		return;
	}
	int len = (int)comA.size() + (int)comB.size() - 1;
	for (int i = 0; i < len; ++i)
	{
		res.push_back(CentroidPeaks(0, 0));
	}
	double x = 0, y = 0, z = 0, xm = 0, ym = 0;
	for(int i = 0; i < (int)comA.size(); ++i)
	{
		x = comA[i].m_lfIntens;
		xm = comA[i].m_lfMZ;
		for(int j = 0; j < (int)comB.size(); ++j)
		{
			y = comB[j].m_lfIntens;
			ym = comB[j].m_lfMZ;
			z = x * y; // 如何防止z超过double表示的最小范围？  [wrm]: 强度乘积后岂不是更小了？这里用的是原始强度
			res[i+j].m_lfIntens += z;
			res[i+j].m_lfMZ += ((xm + ym) * z);
		}
	}
	// 归一化，mz为加权平均后的mz
	for (int k = 0; k < len; ++k)
	{
		if (fabs(res[k].m_lfIntens - 0) > cutoff_eps)
		{
			res[k].m_lfMZ = res[k].m_lfMZ / res[k].m_lfIntens;
		} else {
			res[k].m_lfMZ = -1;
		}
		
	}
}

// 剪枝 [wrm20150928]: limit=1e-10有效果吗？
void CEmass::Prunnig(vector<CentroidPeaks> &com, double limit)   
{
	/*int idx = 0, len = (int)com.size();
	while(idx < len)
	{
		if(com[idx].m_lfIntens > limit)
			break;
		++idx;
	}
	if(idx > 0)
	{
		com.erase(com.begin(), com.begin() + idx);
	}*/
	// 去掉右边较低的部分
	while(!com.empty())
	{
		if(com.back().m_lfIntens > limit)
			break;
		com.pop_back();
	}
}

void CEmass::GetFormbyAvegine(double mass, vector<int> &pattern)
{
	vector<int>().swap(pattern);
	pattern.assign(m_nElemNum, 0);
	double fold = mass / m_lfAvgMass;
	double totalMass = 0.0;
	for(int i = 0; i < m_nElemNum; ++i)
	{
		pattern[i] = int(m_vAveraine[i] * fold);
		totalMass += pattern[i] * m_vMass[i];
	}
	// Element H 质量不足的用H来补
	pattern[1] += int(mass - totalMass + 0.5);
}

//void CEmass::Calculate(double mass, vector<CentroidPeaks> &res)
//{
//	vector<int> pattern;
//	vector<CentroidPeaks> tmpRes;
//	double calmass = mass;
//	const double base = 10000;
//	int level = (int)(mass / base);
//
//	if(mass == base * level)
//	{
//		res = m_vBaseIsotope[level];
//		return;
//	}
//
//	if(mass > m_lfpreMass)
//	{
//		if(m_lfpreMass > base * level)
//		{
//			tmpRes = m_vPrePartten;
//			calmass -= m_lfpreMass;
//		} else {
//			tmpRes = m_vBaseIsotope[level];
//			calmass -= base * level;
//		}
//	} else {
//		tmpRes = m_vBaseIsotope[level];
//		calmass -= base * level;
//	}
//
//	GetFormbyAvegine(calmass, pattern);
//	for(size_t i = 0; i < pattern.size(); ++i)
//	{
//		if(pattern[i] == 0) continue;
//		vector<CentroidPeaks> oneElemRes;
//		Coumpound(pattern[i], m_vIsotope[i], oneElemRes);
//		if(tmpRes.size() == 0)
//		{
//			tmpRes.swap(oneElemRes);
//		} else {
//			Convolution(oneElemRes, tmpRes, res);
//			tmpRes = res;
//		}
//		Prunnig(tmpRes, 1e-10);
//	}
//	res = tmpRes;
//	if(mass > m_lfpreMass)
//	{
//		m_lfpreMass = mass;
//		m_vPrePartten = res;
//	}
//
//#ifdef _DEBUG_EMASS1
//	for(size_t i = 0; i < res.size(); ++i)
//	{
//		cout << res[i].m_lfMZ << " " << res[i].m_lfIntens << endl;
//	}
//	cout<<"--------------------------------"<<endl;
//#endif
//}

// 根据质量计算谱峰
void CEmass::Calculate(double mass, vector<CentroidPeaks> &res)
{
	vector<int> pattern;
	vector<CentroidPeaks> tmpRes;
	int calmass = (int)mass;
	int level = calmass / m_nBase;

	if(calmass == m_nBase * level)
	{
		res = m_vBaseIsotope[level];
		return;
	}
		
	tmpRes = m_vBaseIsotope[level];

	calmass -= m_nBase * level;
	vector<CentroidPeaks> oneElemRes = m_vIPV[calmass].begin()->second;

	Convolution(oneElemRes, tmpRes, res);
	Prunnig(res, 1e-10);

#ifdef _DEBUG_EMASS1
	for(size_t i = 0; i < res.size(); ++i)
	{
		cout << res[i].m_lfMZ << " " << res[i].m_lfIntens << endl;
	}
	cout<<"--------------------------------"<<endl;
#endif 
}