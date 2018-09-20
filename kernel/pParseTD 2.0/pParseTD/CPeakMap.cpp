#include <iostream>
#include <cstdio>
#include <vector>
#include <string>
#include <map>
#include <cmath>

#include "CPeakMap.h"

using namespace std;

CPeakMap::CPeakMap(int BinsPerTh, int Span, double MaximalHalfWidth)
{
	m_BinsPerTh = BinsPerTh; // default 100 bins per Th
	m_r = 2 * Span; // default 10 scan
	//cout << "half window:" << MaximalHalfWidth << " Bins Per Th" << BinsPerTh <<endl; 
	m_c = (int)floor( (2 * MaximalHalfWidth) * BinsPerTh ); // Bins per scan (Row) floorΪ����ȡ��
	//cout << "m_c is " << m_c <<endl; 
	m_span = Span;
	m_halfwindow = MaximalHalfWidth;
	Initialize(); // used first time �� mr�У�mc+100��
}
void CPeakMap::Initialize()
{
	for (int i = 0; i < m_r; i ++)
	{
		vector<double> tmp;
		tmp.assign(m_c+100, 0);
		m_PeakMatrix.push_back(tmp);
	}
}
void CPeakMap::Reset()
{
	for (int i = 0; i < m_r; i ++)
	{
		for (int j = 0; j < m_c+100; j ++)
		{
			m_PeakMatrix[i][j] = 0;
		}
	}
}
// �׷�ǿ�ȹ�������
// �׷��mz,�׷�ǿ��,��������,һ���������������Ķ�Ӧscan��ƫ��λ��
void CPeakMap::IntensityMap(double mz, double intensity, double ac, int scanidx)
{
	int x, y;
	x = scanidx + m_span - 1;
	y = int((mz-ac+m_halfwindow)*m_BinsPerTh);
	if (x>= m_r || x <0 || y >= m_c || y < 0)  // ����������Χ
	{
		return;
	}
	
	m_PeakMatrix[x][y] += intensity;  // �������bin�е�ǿ�Ⱥͣ����׷���ӣ�
}
// scanidx <- {0,1,2,...,2*Span-1}
// binidx <- {0,1,2,...,2*MaximalHalfWindow*BinsPerTh-1}
double CPeakMap::GetPeak(int scanidx, int binidx)
{
	if (scanidx >= m_r || binidx >= m_c || scanidx < 0 || binidx < 0)
	{
		return 0;
	}
	return m_PeakMatrix[scanidx][binidx];
}
void CPeakMap::Print()
{
	cout << "Bins per Th: " << m_BinsPerTh <<endl;
	cout << "Row and col: " << m_r << " " << m_c <<endl;
	cout << "Span of ms1: " << m_span <<endl;
	double s = 0;
	for (int i = 0; i< m_r; i++)
	{
		for (int j = 0; j< m_c; j++)
		{
			s += m_PeakMatrix[i][j];
		}
	}
	cout << "Sum of Peak: " << s <<endl; 
}

CPeakMap::~CPeakMap()
{
	//cout << "Releasing CPeakMap..." <<endl;
}