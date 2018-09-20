/***********************************************************************/
/*                                                                     */
/*   BasicFunction.h                                                   */
/*                                                                     */
/*   Main flow of pParseTD                                             */
/*                                                                     */
/*   Author: Wu Long   Reconsituted: Luo Lan                           */
/*   Date: 2014.05.22                                                  */
/*                                                                     */
/*   Copyright (c) 2014 - All rights reserved                          */
/*                                                                     */
/*                                                                     */
/***********************************************************************/
#ifndef _MONOCANDIDATES_H_
#define _MONOCANDIDATES_H_

#include <string>
#include <map>

#include "Parameters.h"
#include "BasicFunction.h"
#include "MS1Reader.h"
#include "MS2Reader.h"

using namespace std;

class CMainFlow{
public:
	CMainFlow();
	~CMainFlow();
	
	void ProcessEachMS1(string filename);					// 处理一个MS1文件
	void ProcessEachRaw(string filename);					// 处理一个Raw文件
	void GetFilelistbyDirent(string &filepath,				// 获取指定路径下的所有的指定文件格式的文件
		string format, list<string> &fileList);
	void ProcessFileList();									// 依次处理各个文件 
	void GetIPV(string IPVfilepath);						// 读取IPV文件，导入整个矩阵
	int GetOneIPV(int mass, vector<double> &vInt);			// 根据质量得到IPV，返回第几根峰最高

	void GetStart();										// 开始整个流程
	void RunFromFile(string filename);						// 通过参数文件进行配置，入口1
	void RunFromCMD(int argc , char * argv[]);				// 通过命令行配置参数，入口2

	void FlowKernel(string MS1file, MS1Reader &MS1, string MS2file, bool newModel);			// 对每个MS1和MS2文件进行处理

	// [test]
	// vector<int> t_isoPeakNum;

private:

	double **m_vIPV;										// 用于存储emass计算的理论同位素分布
	CEmass *m_cEmass;										// 用于在线计算理论同位素分布
	CParaProcess *m_para;									// 参数类的对象
	list<string> m_MS1list;									// 待处理的MS1列表
	list<string> m_Rawlist;									// 待处理的RAW列表
};

#endif