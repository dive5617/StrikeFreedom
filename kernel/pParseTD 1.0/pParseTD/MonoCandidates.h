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
	
	void ProcessEachMS1(string filename);					// ����һ��MS1�ļ�
	void ProcessEachRaw(string filename);					// ����һ��Raw�ļ�
	void GetFilelistbyDirent(string &filepath,				// ��ȡָ��·���µ����е�ָ���ļ���ʽ���ļ�
		string format, list<string> &fileList);
	void ProcessFileList();									// ���δ�������ļ� 
	void GetIPV(string IPVfilepath);						// ��ȡIPV�ļ���������������
	int GetOneIPV(int mass, vector<double> &vInt);			// ���������õ�IPV�����صڼ��������

	void GetStart();										// ��ʼ��������
	void RunFromFile(string filename);						// ͨ�������ļ��������ã����1
	void RunFromCMD(int argc , char * argv[]);				// ͨ�����������ò��������2

	void FlowKernel(string MS1file, MS1Reader &MS1, string MS2file, bool newModel);			// ��ÿ��MS1��MS2�ļ����д���

	// [test]
	// vector<int> t_isoPeakNum;

private:

	double **m_vIPV;										// ���ڴ洢emass���������ͬλ�طֲ�
	CEmass *m_cEmass;										// �������߼�������ͬλ�طֲ�
	CParaProcess *m_para;									// ������Ķ���
	list<string> m_MS1list;									// �������MS1�б�
	list<string> m_Rawlist;									// �������RAW�б�
};

#endif