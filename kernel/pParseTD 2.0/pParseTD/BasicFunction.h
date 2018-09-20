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

	void PrintLogo();						// 打印pParse欢迎页面
	bool CheckDate();						// 检查软件是否过期
};

class CHelpInfo{
public:

	CHelpInfo(){};
	~CHelpInfo(){};
	void WhatsNew();						// 显示版本历史
	void PrintVersion();					// 打印版本号，并显示更新内容
	void DisplayUsage();					// 显示使用方法
	void DisplayCMDUsage();					// 显示命令行使用帮助
	void GeneratedParamTemplate();			// 生成参数文件
	
};

class CentroidPeaks
{
public:

	double m_lfMZ;							// 谱峰质荷比
	double m_lfIntens;						// 谱峰强度
	CentroidPeaks(double MZ, double intensity)
	{
		m_lfMZ = MZ;
		m_lfIntens = intensity;
	}
};

// add @20150317 去卷积、中心化后的谱峰
class DeconvCentroidPeaks
{
public:
	double m_lfMass;						// 单电荷谱峰质量
	double m_lfIntens;						// 谱峰强度
	double m_lfMonoMz;						// 单同位素峰质荷比
	double m_lfHighestMz;					// 最高峰质荷比
	int m_nCharge;							//电荷状态
	DeconvCentroidPeaks(double mass, double intensity, double mz1, double mz2, int charge)
	{
		m_lfMass = mass;
		m_lfIntens = intensity;
		m_lfMonoMz = mz1;
		m_lfHighestMz = mz2;
		m_nCharge = charge;
	}
};

// 存储谱峰按照mz的编号、质荷比、谱峰强度
class CLabelCentroidPeaks
{
public:

	int m_nIdx;								// 编号，按mz
	double m_lfMZ;							// 谱峰质荷比
	double m_lfIntens;						// 谱峰强度
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

	void InitiParaMap();                                      // 初始化默认参数，存储在map中
	void DisplayPara();                                       // 显示参数
	void CheckParam();                                        // 检查参数文件中路径参数是否合法

	void GetCMDOption(int argc, char  *argv[]);               // 解析命令行中的参数
	void GetFilePara(string &filename);						  // 解析参数ini文件忽略注释
	string GetValue(const string strKey, string strDef = ""); // 根据参数名关键字获取参数值
	string GetTimeStr();									  // 返回时间标签，如1014.09.19.15.15
	
	void SetValue(const string strKey, const string strValue);//根据参数名关键字设置单个参数值

private:

	unordered_map<string, string> m_mapPara;                            // 存储参数信息的map

	bool _isPath(string &strpath);                            // 判断输入路径是否合法
};

#endif