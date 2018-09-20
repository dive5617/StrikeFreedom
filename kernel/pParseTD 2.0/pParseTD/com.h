/***********************************************************************/
/*                                                                     */
/*   com.h                                                             */
/*                                                                     */
/*    Some useful function tools                                       */
/*                                                                     */
/*   Author: Wu Long   Reconsituted: Luo Lan                           */
/*   Date: 2014.06.06                                                  */
/*                                                                     */
/*   Copyright (c) 2014 - All rights reserved                          */
/*                                                                     */
/***********************************************************************/

#ifndef _com_H_
#define _com_H_

#include <vector>
#include <cmath>
#include <cstdlib>
#include <cstdio>

#include "BasicFunction.h"

using namespace std;

class CTools{
public:
	static int gSortByCol;
	CTools(){ };
	~CTools(){ };
	static bool compLessMZ(const CentroidPeaks &x, const CentroidPeaks &y);				// �Ƚ��׷��MZ��<��
	static bool compHighIntens(const CentroidPeaks &x, const CentroidPeaks &y);			// �Ƚ��׷��ǿ��(>)
	static bool compPrecursorByCol(const vector<double> &X1, const vector<double> &X2);	// �����еĺ�ѡĸ���ӵ�������������ıȽϺ���������
	static bool compPrecursorByColDescend(const vector<double> &X1, const vector<double> &X2);// �����еĺ�ѡĸ���ӵ�������������ıȽϺ������ݼ�
	static bool IsFileExists(string Filename);											// �ж��ļ��Ƿ����
	static int charCount(string &x, char y);											// �����ַ����а������ַ�y�ĸ���
	static int OutputFilesize(const char * filename);									// �����ļ���С
	static long filesize(FILE *stream);													// �����ļ���С
	static string int2string(int x);													// ������ת��Ϊ�ַ���string
	static void ClearFile(const char * filename);										// �ر��ļ�
};

class CStatistic
{
public:
	CStatistic() { };
	~CStatistic() { };
	static void CalculateMean(vector<vector<double> > &SampleMatrix,vector<double> &Mean);
	static void CalculateSTD(vector<vector<double> > &SampleMatrix,vector<double> &Mean,vector<double> &STD);
	static void Calculate_Mean_STD(vector<vector<double> > &SampleMatrix,vector<double> &Mean,vector<double> &STD);
	static double CalculateVectorCov(vector<double> &X, vector<double> &Y);
	static void Normalization(vector<vector<double> > &SampleMatrix);
	static bool FindLocalMinimal(vector<int> &LocalMinimal,vector<double> &Data);
	static bool FindLocalMax(vector<int> &posRes, vector<double> &data);
};


class ErrorStruct
{
public:
	static bool Errorcmp(ErrorStruct x,ErrorStruct y);
	double ErrorRate;
	int Index;
};

// added by wrm. @2016.03.23
class HEAP{
public:
	HEAP(){};
	~HEAP(){};
	static void BuildHeap(vector<double> &A, bool isMin);
	static void AjustHeap(vector<double> &A, int len, int i, bool isMin);
	static void HeapSort(vector<double> &A, bool desc);
};

const int chg_beg = 5;
const int chg_end = 60;
const int mz_beg = 200;
const int mz_end = 1999;
const int MaxMZ = 6000;
class Tables{
public:
	static std::vector<vector<double>> s_vf_charge_mz_tol;  // ppm*10-6
	static void InitChargeMzTolTable();
};

#endif