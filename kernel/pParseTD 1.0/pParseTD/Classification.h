/***********************************************************************/
/*                                                                     */
/*   Classification.h                                                  */
/*                                                                     */
/*   Machine Learning method to classify                               */
/*     the right or wrong precursors  ��support SVM & MARS)            */
/*																	   */
/*   Author:  Luo Lan                                                  */
/*   Date: 2014.05.23                                                  */
/*                                                                     */
/*   Copyright (c) 2014 - All rights reserved                          */
/*                                                                     */
/***********************************************************************/
#ifndef _CLASSIFICATION_H_
#define _CLASSIFICATION_H_

#include <iostream>
#include <cstdio>
#include <cstdlib>
#include <fstream>
#include <vector>

#include "BasicFunction.h"
#include "TrainingFileReader.h"

using namespace std;

class CClassify
{
public:
	CClassify(string &path);
	~CClassify();

	int SVMPrepareData(vector<vector<double> > &featureInfo, 
		vector<vector<double> > &OutputInfo, TrainingSetreader *pTrainSet); // Ԥ��������ΪSVM��Ҫ�ĸ�ʽ
	void SVMTraining();														// SVMѵ��������svm_learn.exe
	void SVMPredict(vector<double> &vPredictY);							// SVMԤ�⣬����svm_classify.exe

	double PredictMARSer1013(double Feature[]);							// MARSģ��
	void MARSPredict(vector<vector<double> > &featureInfo, vector<double> &vPredictY);
	
	void RemoveTmpFiles();

private:
	string m_strTrainFile;													// SVMѵ�����ݼ��ļ���
	string m_strTestFile;													// SVM�������ݼ��ļ���
	string m_strModelFile;													// SVMģ���ļ���
	string m_strOutFile;													// SVM����ļ���
};

#endif