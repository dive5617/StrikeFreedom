/***********************************************************************/
/*                                                                     */
/*   Classification.h                                                  */
/*                                                                     */
/*   Machine Learning method to classify                               */
/*     the right or wrong precursors  （support SVM & MARS)            */
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
		vector<vector<double> > &OutputInfo, TrainingSetreader *pTrainSet); // 预处理数据为SVM需要的格式
	void SVMTraining();														// SVM训练，调用svm_learn.exe
	void SVMPredict(vector<double> &vPredictY);							// SVM预测，调用svm_classify.exe

	double PredictMARSer1013(double Feature[]);							// MARS模型
	void MARSPredict(vector<vector<double> > &featureInfo, vector<double> &vPredictY);
	
	void RemoveTmpFiles();

private:
	string m_strTrainFile;													// SVM训练数据集文件名
	string m_strTestFile;													// SVM测试数据集文件名
	string m_strModelFile;													// SVM模型文件名
	string m_strOutFile;													// SVM输出文件名
};

#endif