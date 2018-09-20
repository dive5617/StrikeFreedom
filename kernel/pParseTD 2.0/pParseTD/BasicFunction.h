/***********************************************************************/
/*                                                                     */
/*   BasicFunction.h                                                   */
/*                                                                     */
/*    Some useful tools of pParseTD                                    */
/*                                                                     */
/*   Author: Luo Lan                                                   */
/*   Date: 2014.05.22                                                  */
/*                                                                     */
/*   Copyright (c) 2014 - All rights reserved                          */
/*                                                                     */
/***********************************************************************/

#ifndef _BASICFUNCTION_H_
#define _BASICFUNCTION_H_

#include <cstring>
#include <string>
#include <iostream>
#include <fstream>
#include <limits>
#include <cstdlib>
#include <stdexcept>
#include <map>
#include <unordered_map>
#include <list>

#include "Parameters.h"
using namespace std;

const int ExpireYear = 2019;
const int ExpireMonth = 1;
const int ExpireDay = 1;

class CErrInfo {
private:

	string m_strClass;
	string m_strMethod;
	string m_strDetail;
	string m_strInfo;
	string m_strException;

public:

	CErrInfo(const string &strClass, const string &strMethod,
			const string &strDetail="");

	CErrInfo(const string &strClass, const string &strMethod,
			const string &strDetail, const exception & e);
	void Append(const string &strInfo);

	string Get() const;

	string Get(const exception& e);

	void SetException(const exception & e);

	friend ofstream& operator<<(ofstream& os, const CErrInfo& info);
	friend ostream& operator<<(ostream& os, const CErrInfo& info);

};

class CWelcomeInfo{
public:

	CWelcomeInfo(){};
	~CWelcomeInfo(){};

	void PrintLogo();						// ��ӡpParse��ӭҳ��
	bool CheckDate();						// �������Ƿ����
};

class CHelpInfo{
public:

	CHelpInfo(){};
	~CHelpInfo(){};
	void WhatsNew();						// ��ʾ�汾��ʷ
	void PrintVersion();					// ��ӡ�汾�ţ�����ʾ��������
	void DisplayUsage();					// ��ʾʹ�÷���
	void DisplayCMDUsage();					// ��ʾ������ʹ�ð���
	void GeneratedParamTemplate();			// ���ɲ����ļ�
	
};

class CentroidPeaks
{
public:

	double m_lfMZ;							// �׷��ʺɱ�
	double m_lfIntens;						// �׷�ǿ��
	CentroidPeaks(double MZ, double intensity)
	{
		m_lfMZ = MZ;
		m_lfIntens = intensity;
	}
};

// add @20150317 ȥ��������Ļ�����׷�
class DeconvCentroidPeaks
{
public:
	double m_lfMass;						// ������׷�����
	double m_lfIntens;						// �׷�ǿ��
	double m_lfMonoMz;						// ��ͬλ�ط��ʺɱ�
	double m_lfHighestMz;					// ��߷��ʺɱ�
	int m_nCharge;							//���״̬
	DeconvCentroidPeaks(double mass, double intensity, double mz1, double mz2, int charge)
	{
		m_lfMass = mass;
		m_lfIntens = intensity;
		m_lfMonoMz = mz1;
		m_lfHighestMz = mz2;
		m_nCharge = charge;
	}
};

// �洢�׷尴��mz�ı�š��ʺɱȡ��׷�ǿ��
class CLabelCentroidPeaks
{
public:

	int m_nIdx;								// ��ţ���mz
	double m_lfMZ;							// �׷��ʺɱ�
	double m_lfIntens;						// �׷�ǿ��
	CLabelCentroidPeaks()
	{
		m_nIdx = 0;
		m_lfMZ = 0.0;
		m_lfIntens = 0.0;
	}
	CLabelCentroidPeaks(int idx, double MZ, double intensity)
	{
		m_lfMZ = MZ;
		m_lfIntens = intensity;
		m_nIdx = idx;
	}
	CLabelCentroidPeaks(CentroidPeaks &p, int idx)
	{
		m_lfMZ = p.m_lfMZ;
		m_lfIntens = p.m_lfIntens;
		m_nIdx = idx;
	}
};

class CParaProcess {
public:

	CParaProcess();
	~CParaProcess();

	void InitiParaMap();                                      // ��ʼ��Ĭ�ϲ������洢��map��
	void DisplayPara();                                       // ��ʾ����
	void CheckParam();                                        // �������ļ���·�������Ƿ�Ϸ�

	void GetCMDOption(int argc, char  *argv[]);               // �����������еĲ���
	void GetFilePara(string &filename);						  // ��������ini�ļ�����ע��
	string GetValue(const string strKey, string strDef = ""); // ���ݲ������ؼ��ֻ�ȡ����ֵ
	string GetTimeStr();									  // ����ʱ���ǩ����1014.09.19.15.15
	
	void SetValue(const string strKey, const string strValue);//���ݲ������ؼ������õ�������ֵ

private:

	unordered_map<string, string> m_mapPara;                            // �洢������Ϣ��map

	bool _isPath(string &strpath);                            // �ж�����·���Ƿ�Ϸ�
};

#endif