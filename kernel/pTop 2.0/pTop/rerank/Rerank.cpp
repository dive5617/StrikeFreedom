
#include "Rerank.h"

#ifdef __cplusplus
extern "C" {
#endif

	extern int svm_train(int argc, char **argv);
	extern int svm_predict(int argc, char **argv);

#ifdef __cplusplus
}
#endif

//#define _Debug_Rank
#define _Debug_Train


bool PrSMFeature::cmpByOldScore(const PrSMFeature &stItem1, const PrSMFeature &stItem2)
{
	return stItem1.basic_info.lfScore > stItem2.basic_info.lfScore;
}

bool operator<(const PrSMFeature &stItem1, const PrSMFeature &stItem2)
{
	return stItem1.svm_score < stItem2.svm_score;
}
bool operator<=(const PrSMFeature &stItem1, const PrSMFeature &stItem2)
{
	return stItem1.svm_score <= stItem2.svm_score;
}
bool operator>(const PrSMFeature &stItem1, const PrSMFeature &stItem2)
{
	return stItem1.svm_score > stItem2.svm_score;
}
bool operator>=(const PrSMFeature &stItem1, const PrSMFeature &stItem2)
{
	return stItem1.svm_score >= stItem2.svm_score;
}


Rerank::Rerank(const CConfiguration *cPara, const string &path) : m_cPara(cPara), m_strInputFile(path), m_lfThreshold(0.005), 
m_lfMaxPrecurErr(0.001), m_nFeature_Num(9), m_nSample_Num(0), m_eNormal_type(e_minmax), m_vPrecursor_err_freq(PrecursorErrorBin+1,0),
m_cFeatureConverter(new FeatureNormalization())  //
{
	string dir = "";
	int idx = path.rfind(cSlash);
	if (idx != string::npos)  dir = path.substr(0, idx + 1);
	m_strTrainFile = dir + "TrainingSetSVM.txt";
	m_strTestFile = dir + "TestSetSVM.txt";
	m_strModelFile = dir + "SVMmodel.txt";
	m_strOutFile = dir + "SVMpredict.txt";
	m_pTrace = Clog::getInstance();
}
Rerank::~Rerank(){
	ClearVector(m_vPrSMInfo);
	RemoveTmpFiles();

	if (m_cFeatureConverter){
		delete m_cFeatureConverter;
		m_cFeatureConverter = NULL;
	}
	if (m_cPara){
		m_cPara = NULL;
	}
	if (m_pTrace){
		m_pTrace = NULL;
	}
}

void Rerank::PrepareData(vector<vector<double> > &train, vector<vector<double> > &test)
{
	if (train.empty() || test.empty()){
		CErrInfo info("Rerank", "PrepareData", "Empty training set or testing set.");
		throw runtime_error(info.Get().c_str());
		return;
	}
	FILE *ptrain = fopen(m_strTrainFile.c_str(), "wb");
	if (ptrain == NULL){
		CErrInfo info("Rerank", "PrepareData", "create training set file failed.");
		throw runtime_error(info.Get().c_str());
		return;
	}
	FILE *ptest = fopen(m_strTestFile.c_str(), "wb");
	if (ptest == NULL){
		CErrInfo info("Rerank", "PrepareData", "create testing set file failed.");
		throw runtime_error(info.Get().c_str());
		return;
	}
	char *buf = new char[BUFFERSIZE];
	int len = 0;
	for (int i = 0; i<train.size(); ++i){
		len += sprintf(buf + len, "%f", train[i][0]);
		for (int j = 1; j <= m_nFeature_Num; ++j){
			len += sprintf(buf + len, " %d:%f", j, train[i][j]);
		}
		len += sprintf(buf + len, "\n");
		if (len + BUFLEN > BUFFERSIZE){
			buf[len] = '\0';
			fwrite(buf, 1, len, ptrain);
			len = 0;
		}
	}
	if (len){
		buf[len] = '\0';
		fwrite(buf, 1, len, ptrain);
	}

	len = 0;
	for (int i = 0; i<test.size(); ++i){
		len += sprintf(buf + len, "%f", test[i][0]);
		for (int j = 1; j <= m_nFeature_Num; ++j){
			len += sprintf(buf + len, " %d:%f", j, test[i][j]);
		}
		len += sprintf(buf + len, "\n");
		if (len + BUFLEN > BUFFERSIZE){
			buf[len] = '\0';
			fwrite(buf, 1, len, ptest);
			len = 0;
		}
	}
	if (len){
		buf[len] = '\0';
		fwrite(buf, 1, len, ptest);
	}
	delete []buf;
	fclose(ptrain);
	fclose(ptest);
}
void Rerank::SVMTraining()
{
	m_pTrace->info("linear SVM online training ...");
	FILE *ftrain = fopen(m_strTrainFile.c_str(), "r");
	if (ftrain == NULL)
	{
		fclose(ftrain);
		CErrInfo info("Rerank", "SVMTraining", "There is no training data!");
		throw runtime_error(info.Get().c_str());
		return;
	}
	else {
		fclose(ftrain);
	}
	int train_argc = 10;
	const char *train_argv[] = { "train.exe", "-q", "-s", "2", "-B", "1", "-e", "0.0001",
		m_strTrainFile.c_str(), m_strModelFile.c_str() };

	try
	{
		svm_train(train_argc, const_cast<char **>(train_argv));   // remove the const property
	}
	catch (exception & e)
	{
		CErrInfo info("Rerank", "SVMTraining", "failed.");
		throw runtime_error(info.Get(e).c_str());
	}
	catch (...)
	{
		CErrInfo info("Rerank", "SVMTraining", "Caught an unknown exception from SVMTraining.");
		throw runtime_error(info.Get().c_str());
	}

}
void Rerank::SVMPredict(vector<double> &vPredictY)
{
	m_pTrace->info("begin to predict ...");
	vPredictY.clear();
	FILE *fmodel = fopen(m_strModelFile.c_str(), "r");
	if (fmodel == NULL)
	{
		fclose(fmodel);
		CErrInfo info("Rerank", "SVMPredict", "There is no model!");
		throw runtime_error(info.Get().c_str());
		return;
	}
	else {
		fclose(fmodel);
	}
	int predict_argc = 5;
	const char *predict_argv[] = { "predict.exe", "-q", m_strTestFile.c_str(), m_strModelFile.c_str(), m_strOutFile.c_str() };

	try
	{
		svm_predict(predict_argc, const_cast<char **>(predict_argv));
	}
	catch (exception &e)
	{
		CErrInfo info("Rerank", "SVMPredict", "failed.");
		throw runtime_error(info.Get(e).c_str());
	}
	catch (...)
	{
		throw("Something wrong in SVM predict!");
	}
	FILE *fp = fopen(m_strOutFile.c_str(), "r");
	double predictY = 0.0;
	while (fscanf(fp, "%lf", &predictY) > 0)
	{
		vPredictY.push_back(predictY);
	}
	fclose(fp);
}

// main flow of reranking
void Rerank::run()
{
	try{
		m_pTrace->info("Start re-ranking using SVM...");
		readSearchResultFile();
		vector<vector<double> > train;
		vector<vector<double> > test;
		vector<double>  Y;
		if (!m_cPara->m_bRerank || !SamplePartition(train, test, Y)){
			writeResultFile();
			return;
		}
		m_pTrace->info("The FDR threshold for positive samples is %f", m_lfThreshold);
		// 迭代 k次
		for (size_t it = 0; it < IterationTimes; ++it){
			m_pTrace->info("Iterative counter: %d", it + 1);
			PrepareData(train, test);
			SVMTraining();
			SVMPredict(Y);
			SamplePartition(train, test, Y);   // update svm score and resample
		}
		writeResultFile();
	}
	catch (exception & e)
	{
		CErrInfo info("Rerank", "run", "");
		m_pTrace->error(info.Get(e));
		throw runtime_error(info.Get(e).c_str());
	}
	catch (...)
	{
		CErrInfo info("Rerank", "run", "caught an unknown exception from reranking.");
		m_pTrace->error(info.Get());
		throw runtime_error(info.Get().c_str());
	}
}

// update svm score, and rerank the spectra of the same title
void Rerank::updateSVMScore(vector<double> &Y)
{
	int t = 0;
	for (int i = 0; i<m_vPrSMInfo.size(); ++i){
		for (int j = 0; j < m_vPrSMInfo[i].size(); ++j){
			m_vPrSMInfo[i][j].svm_score = Y[t];
			m_vPrSMInfo[i][j].basic_info.lfEvalue = 1.0 / (1.0 + exp(10 * Y[t]));
			++t;
		}	
	}
	// 单谱内排序
	for (int i = 0; i<m_vPrSMInfo.size(); ++i){
		sort(m_vPrSMInfo[i].begin(), m_vPrSMInfo[i].end(), std::greater<PrSMFeature>());  // 降序：从大到小
	}
}

// prsmInfoCpy保存了每张谱的第一名，对每张谱的第一名进行计算FDR
void Rerank::calFDR(vector<PrSMFeature> &prsmInfoCpy, bool first)
{
	if (first){
		sort(prsmInfoCpy.begin(), prsmInfoCpy.end(), PrSMFeature::cmpByOldScore);
	}
	else{
		sort(prsmInfoCpy.begin(), prsmInfoCpy.end(), std::greater<PrSMFeature>());
	}
	double decoynum = 0, targetnum = 0.001;
	for (size_t i = 0; i < prsmInfoCpy.size(); ++i)
	{    // sort by score, the higher the better
		if (prsmInfoCpy[i].basic_info.nIsDecoy == 1) decoynum += 1;
		else targetnum += 1;
		prsmInfoCpy[i].basic_info.lfQvalue = (1.0 * decoynum / targetnum);
	}
}

// calculate precursor_error, mod_ratio, score_diff
void Rerank::updateFeature(vector<PrSMFeature> &prsmInfoCpy)
{
	
	vector<int> precur_err_hist(PrecursorErrorBin + 1, 0);
	unordered_map<string, int> umModCnt;
	int i = prsmInfoCpy.size() - 1;
	double dis = m_lfMaxPrecurErr / PrecursorErrorBin;
	while (i >= 0 && prsmInfoCpy[i].basic_info.lfQvalue > m_lfThreshold){   
		// [wrm] isDecoy = 1表示反库结果，为0表示正库过阈值的结果，为-1表示正库未过阈值的结果
		if (prsmInfoCpy[i].basic_info.nIsDecoy == 0){
			prsmInfoCpy[i].basic_info.nIsDecoy = -1;
		}
		--i;
	}
	// pos
	unordered_set<string> ptms;
	int pos = 0;
	while (i >= 0){
		if (prsmInfoCpy[i].basic_info.nIsDecoy == 0){
			++pos;
			double proMass = prsmInfoCpy[i].basic_info.lfProMass;
			double ppm = getTolPpm(prsmInfoCpy[i].basic_info.lfPrecursorMass, proMass);
			++precur_err_hist[(unsigned int)floor(ppm / dis)];
			ptms.clear();
			for (int j = 0; j < prsmInfoCpy[i].ptms.size(); ++j){
				ptms.insert(prsmInfoCpy[i].ptms[j]);
			}
			for (auto it : ptms){
				if (umModCnt.find(it) == umModCnt.end()){
					umModCnt[it] = 1;
				}
				else{
					++umModCnt[it];
				}
			}
		}
		--i;
	}

	m_vPrecursor_err_freq[PrecursorErrorBin] = precur_err_hist[PrecursorErrorBin - 1]*1.0 / pos;
	for (int i = PrecursorErrorBin - 1; i >= 0; --i){
		m_vPrecursor_err_freq[i] = m_vPrecursor_err_freq[i + 1] + precur_err_hist[i] * 1.0 / pos;
	}
	for (auto it = umModCnt.begin(); it != umModCnt.end(); ++it){
		m_umModPro[it->first] = it->second*1.0 / pos;
	}
	m_cFeatureConverter->Clear();
	
	vector<double> feat(m_nFeature_Num, 0);
	for (int i = 0; i < prsmInfoCpy.size(); ++i){
		// update precursor_error, mod_ratio, diff_score
		double proMass = prsmInfoCpy[i].basic_info.lfProMass;
		double ppm = getTolPpm(prsmInfoCpy[i].basic_info.lfPrecursorMass, proMass);
		int binx = (unsigned int)floor(ppm / dis);
		prsmInfoCpy[i].precursor_error = m_vPrecursor_err_freq[binx > PrecursorErrorBin ? PrecursorErrorBin : binx];
		prsmInfoCpy[i].mod_ratio = 1;
		for (int k = 0; k < prsmInfoCpy[i].ptms.size(); ++k){
			if (m_umModPro.find(prsmInfoCpy[i].ptms[k]) != m_umModPro.end()){
				prsmInfoCpy[i].mod_ratio *= m_umModPro[prsmInfoCpy[i].ptms[k]];
			}
			else{  // a mod didn't appear in positive samples
				prsmInfoCpy[i].mod_ratio *= 0;
			}
		}
		// diff score TODO?
	}

	for (int i = 0; i < m_vPrSMInfo.size(); ++i){
		for (int j = 0; j < m_vPrSMInfo[i].size(); ++j){
			// update precursor_error, mod_ratio, diff_score
			double proMass = m_vPrSMInfo[i][j].basic_info.lfProMass;
			double ppm = getTolPpm(m_vPrSMInfo[i][j].basic_info.lfPrecursorMass, proMass);
			int binx = (unsigned int)floor(ppm / dis);
			m_vPrSMInfo[i][j].precursor_error = m_vPrecursor_err_freq[binx > PrecursorErrorBin ? PrecursorErrorBin : binx];
			m_vPrSMInfo[i][j].mod_ratio = 1;
			for (int k = 0; k < m_vPrSMInfo[i][j].ptms.size(); ++k){
				if (m_umModPro.find(m_vPrSMInfo[i][j].ptms[k]) != m_umModPro.end()){
					m_vPrSMInfo[i][j].mod_ratio *= m_umModPro[m_vPrSMInfo[i][j].ptms[k]];
				}
				else{  // a mod didn't appear in positive samples
					m_vPrSMInfo[i][j].mod_ratio *= 0;
				}
			}
			// diff score

			getFeature(m_vPrSMInfo[i][j], feat);
			m_cFeatureConverter->UpdateFeatureStatistics(feat, 0, m_nFeature_Num, e_minmax);
		}
	}


}

void Rerank::getFeature(PrSMFeature &prsm, vector<double> &featureVal)
{
	int t = 0;
	//featureVal[t++] = prsm.svm_score;
	featureVal[t++] = prsm.basic_info.lfScore;
	//featureVal[t++] = prsm.basic_info.nCharge;
	///featureVal[t++] = prsm.precursor_id;
	featureVal[t++] = prsm.precursor_error;
	featureVal[t++] = prsm.mod_ratio;
	///featureVal[t++] = prsm.diff_score;

	featureVal[t++] = prsm.basic_info.cMatchedInfo.lfNtermMatchedIntensityRatio + prsm.basic_info.cMatchedInfo.lfCtermMatchedIntensityRatio;
	featureVal[t++] = (prsm.basic_info.cMatchedInfo.nNterm_matched_ions + prsm.basic_info.cMatchedInfo.nCterm_matched_ions) / (2.0*(prsm.basic_info.strProSQ.length() - 1));
	featureVal[t++] = prsm.basic_info.cFeatureInfo.lfCom_ions_ratio;
	featureVal[t++] = prsm.basic_info.cFeatureInfo.lfTag_ratio;
	featureVal[t++] = prsm.basic_info.cFeatureInfo.lfPTM_score;
	featureVal[t++] = prsm.basic_info.cFeatureInfo.lfFragment_error_std;
}

double Rerank::getTolPpm(double precursor, double proMass)
{
	double diffDa = precursor - proMass;
	return fabs(diffDa - round(diffDa)) * 1000000.0 / proMass;
}

bool cmp_by_score(const pair<int, double> &a, const pair<int, double> &b) {
	return a.second > b.second;
}
bool Rerank::SamplePartition(vector<vector<double> > &train, vector<vector<double> > &test, vector<double> &Y)
{
	if (m_vPrSMInfo.empty())   return false;
	try{
		if (!Y.empty()){
			updateSVMScore(Y);
		}	
		m_pTrace->info("rerank the spectra according to the svm score and resample..." );
		// resample
		const double neg_pos_ratio = 1.25;
		vector<PrSMFeature> prsmInfoCpy;
		prsmInfoCpy.reserve(m_vPrSMInfo.size());
		vector<pair<int, double>> decoyPrSMs;
		for (int i = 0; i < m_vPrSMInfo.size(); ++i){
			prsmInfoCpy.push_back(m_vPrSMInfo[i][0]);
		}
		calFDR(prsmInfoCpy, Y.empty());

		updateFeature(prsmInfoCpy);

		int pos = 0, neg = 0;
		vector<double> tmp(m_nFeature_Num + 1, 0);
		vector<vector<double>> trainset;
		trainset.reserve(prsmInfoCpy.size()*(1.1 + neg_pos_ratio));
		vector<vector<double>> testset(m_nSample_Num, tmp);
		vector<double> feat(m_nFeature_Num, 0);
		NormalizationMethod  normType = e_minmax;
		// train set: positive
#ifdef _Debug_Rank
		FILE *fpos = fopen((m_cPara->m_strOutputPath + "\\positive.txt").c_str(), "w");
		FILE *fneg = fopen((m_cPara->m_strOutputPath + "\\negative.txt").c_str(), "w");
		string heads = "SVM_Score\tRaw_Score\tCharge\tPrecursor_Error\tMod_Ratio\t";  //PrecursorID\t
		heads += "Matched_Intensity_Ratio\tMatched_Ion_Ratio\tComplementary_Ion_Ratio\tTag_Ratio\tPTM_Score\tFragment_Error_STD\n";
		fprintf(fpos, "%s", heads.c_str());
		fprintf(fneg, "%s", heads.c_str());
#endif
		for (int i = 0; i<prsmInfoCpy.size(); ++i){
			if (prsmInfoCpy[i].basic_info.nIsDecoy == 0){  // positive
				++pos;
#ifdef _Debug_Rank
				fprintf(fpos, "%f\t%f\t%f\t%f\t%f\t", prsmInfoCpy[i].svm_score, prsmInfoCpy[i].basic_info.lfScore, (double)prsmInfoCpy[i].basic_info.nCharge, 
					prsmInfoCpy[i].precursor_error, prsmInfoCpy[i].mod_ratio);
				fprintf(fpos, "%f\t%f\t%f\t%f\t%f\t%f\n", prsmInfoCpy[i].basic_info.cMatchedInfo.lfNtermMatchedIntensityRatio + prsmInfoCpy[i].basic_info.cMatchedInfo.lfCtermMatchedIntensityRatio,
					(prsmInfoCpy[i].basic_info.cMatchedInfo.nNterm_matched_ions + prsmInfoCpy[i].basic_info.cMatchedInfo.nCterm_matched_ions) / (2.0*(prsmInfoCpy[i].basic_info.strProSQ.length() - 1)),
					prsmInfoCpy[i].basic_info.cFeatureInfo.lfCom_ions_ratio, prsmInfoCpy[i].basic_info.cFeatureInfo.lfTag_ratio, 
					prsmInfoCpy[i].basic_info.cFeatureInfo.lfPTM_score, prsmInfoCpy[i].basic_info.cFeatureInfo.lfFragment_error_std);
#endif
				getFeature(prsmInfoCpy[i], feat);
				m_cFeatureConverter->Normalization(feat, 0, feat.size(), normType);
				tmp[0] = 1.0;
				for (int k = 0; k < feat.size(); ++k){
					tmp[k + 1] = feat[k];
				}
				trainset.push_back(tmp);
			}
			else if (prsmInfoCpy[i].basic_info.nIsDecoy == 1){  // negative
				++neg;
#ifdef _Debug_Rank
				fprintf(fneg, "%f\t%f\t%f\t%f\t%f\t", prsmInfoCpy[i].svm_score, prsmInfoCpy[i].basic_info.lfScore, (double)prsmInfoCpy[i].basic_info.nCharge,
					prsmInfoCpy[i].precursor_error, prsmInfoCpy[i].mod_ratio);
				fprintf(fneg, "%f\t%f\t%f\t%f\t%f\t%f\n", prsmInfoCpy[i].basic_info.cMatchedInfo.lfNtermMatchedIntensityRatio + prsmInfoCpy[i].basic_info.cMatchedInfo.lfCtermMatchedIntensityRatio,
					(prsmInfoCpy[i].basic_info.cMatchedInfo.nNterm_matched_ions + prsmInfoCpy[i].basic_info.cMatchedInfo.nCterm_matched_ions) / (2.0*(prsmInfoCpy[i].basic_info.strProSQ.length() - 1)),
					prsmInfoCpy[i].basic_info.cFeatureInfo.lfCom_ions_ratio, prsmInfoCpy[i].basic_info.cFeatureInfo.lfTag_ratio,
					prsmInfoCpy[i].basic_info.cFeatureInfo.lfPTM_score, prsmInfoCpy[i].basic_info.cFeatureInfo.lfFragment_error_std);
#endif
				getFeature(prsmInfoCpy[i], feat);
				m_cFeatureConverter->Normalization(feat, 0, feat.size(), normType);
				tmp[0] = -1.0;
				for (int k = 0; k < feat.size(); ++k){
					tmp[k + 1] = feat[k];
				}
				trainset.push_back(tmp);
			}
		}
		// train set: negative
		int t = 1;
		while (t < m_cPara->m_nOutputTopK && neg < pos*neg_pos_ratio){
			for (int i = 0; i < m_vPrSMInfo.size(); ++i){
				if (m_vPrSMInfo[i].size() > t && m_vPrSMInfo[i][t].basic_info.nIsDecoy == 1){
					++neg;
#ifdef _Debug_Rank
					fprintf(fneg, "%f\t%f\t%f\t%f\t%f\t", m_vPrSMInfo[i][t].svm_score, m_vPrSMInfo[i][t].basic_info.lfScore, (double)m_vPrSMInfo[i][t].basic_info.nCharge,
						m_vPrSMInfo[i][t].precursor_error, m_vPrSMInfo[i][t].mod_ratio);
					fprintf(fneg, "%f\t%f\t%f\t%f\t%f\t%f\n", m_vPrSMInfo[i][t].basic_info.cMatchedInfo.lfNtermMatchedIntensityRatio + m_vPrSMInfo[i][t].basic_info.cMatchedInfo.lfCtermMatchedIntensityRatio,
						(m_vPrSMInfo[i][t].basic_info.cMatchedInfo.nNterm_matched_ions + m_vPrSMInfo[i][t].basic_info.cMatchedInfo.nCterm_matched_ions) / (2.0*(m_vPrSMInfo[i][t].basic_info.strProSQ.length() - 1)),
						m_vPrSMInfo[i][t].basic_info.cFeatureInfo.lfCom_ions_ratio, m_vPrSMInfo[i][t].basic_info.cFeatureInfo.lfTag_ratio,
						m_vPrSMInfo[i][t].basic_info.cFeatureInfo.lfPTM_score, m_vPrSMInfo[i][t].basic_info.cFeatureInfo.lfFragment_error_std);
#endif
					getFeature(m_vPrSMInfo[i][t], feat);
					m_cFeatureConverter->Normalization(feat, 0, feat.size(), normType);
					tmp[0] = -1.0;
					for (int k = 0; k < feat.size(); ++k){
						tmp[k + 1] = feat[k];
					}
					trainset.push_back(tmp);
					if (neg > pos*neg_pos_ratio)  break;
				}
			}
			++t;
		}
		m_pTrace->info("[Training Set]  %d Positive Samples, %d Negative Samples.", pos, neg);
#ifdef _Debug_Rank
		fclose(fpos);
		fclose(fneg);
#endif
		// test set
		int idx = 0;
		for (int i = 0; i < m_vPrSMInfo.size(); ++i){
			for (int j = 0; j < m_vPrSMInfo[i].size(); ++j){
				getFeature(m_vPrSMInfo[i][j], feat);
				m_cFeatureConverter->Normalization(feat, 0, m_nFeature_Num, normType);
				testset[idx][0] = 0;
				for (int k = 0; k < feat.size(); ++k){
					testset[idx][k + 1] = feat[k];
				}
				++idx;
			}
		}

		if (pos < MinSampleNum || neg < MinSampleNum){   // 200
			m_pTrace->info("Training sample is too small...");
			return false;
		}

		trainset.swap(train);
		testset.swap(test);
	}
	catch (exception &ex) {
		CErrInfo info("Rerank", "UpdateFeature", "error!");
		throw runtime_error(info.Get(ex).c_str());
	}
	return true;
}

void Rerank::readSearchResultFile()
{
	FILE *pfile = fopen(m_strInputFile.c_str(), "rb");   // 此处必须是"rb"，才会原封不动地读取文件
	if (pfile == NULL)
	{
		fclose(pfile);
		CErrInfo info("Rerank", "readSearchResultFile", "Failed to open file:" + m_strInputFile);
		m_pTrace->error(info.Get().c_str());
		exit(1);
	}
	try{
		fseek(pfile, 0L, SEEK_END);
		long filesize = ftell(pfile);
		//cout << filesize << endl;
		char *buf = new char[filesize + 1];
		fseek(pfile, 0L, SEEK_SET);
		int len = (int)fread(buf, sizeof(char), filesize, pfile);
		buf[filesize] = '\0';
		parseResFile(buf, len);
		delete[]buf;
	}
	catch (bad_alloc &ba) {
		fclose(pfile);
		CErrInfo info("Rerank", "readSearchResultFile", "The result file is too large, could not load in!");
		throw runtime_error(info.Get(ba).c_str());
	}
	fclose(pfile);
}

void Rerank::parseResFile(const char *buf, int len)
{
	try{
		m_vPrSMInfo.clear();
		int offset = CStringProcess::find_first_of(buf, '\n', 0, len) + 1;
		int lfpos = CStringProcess::find_first_of(buf, '\n', offset, len);   // skip the first title line
		
		PRSM_Column_Index cols;
		char *tmpline = new char[offset];
		strncpy(tmpline, buf, offset - 1);
		tmpline[offset - 1] = '\0';
		CConfiguration::parseFileTitle(tmpline, ",", cols);
		delete[]tmpline;

		m_nSample_Num = 0;
		const int LINE_LEN = 20000;
		char *line = new char[LINE_LEN];
		while (offset < lfpos){
			strncpy(line, buf + offset, lfpos - offset);
			line[lfpos - offset] = '\0';
			PrSMFeature prsminfo;
			if (parseResLine(line, prsminfo, cols)){
				++m_nSample_Num;
				if (m_vPrSMInfo.empty() || prsminfo.basic_info.strSpecTitle != m_vPrSMInfo.back()[0].basic_info.strSpecTitle){
					vector<PrSMFeature> tmp;
					tmp.push_back(prsminfo);
					m_vPrSMInfo.push_back(tmp);
				}
				else{  // same spectrum
					m_vPrSMInfo.back().push_back(prsminfo);
				}
			}
			offset = lfpos + 1;
			lfpos = CStringProcess::find_first_of(buf, '\n', offset, len);
		}
		delete[] line;

		// 单谱内排序
		for (int i = 0; i < m_vPrSMInfo.size(); ++i){
			sort(m_vPrSMInfo[i].begin(), m_vPrSMInfo[i].end(), PrSMFeature::cmpByOldScore);
		}

		m_pTrace->info("Load %d spectra... ", m_vPrSMInfo.size());
	}
	catch (exception &ex){
		CErrInfo info("Rerank", "parseResFile", "");
		throw runtime_error(info.Get(ex).c_str());
	}
}

// 解析搜索结果文件的一行
bool Rerank::parseResLine(const char *line, PrSMFeature &prsm, const PRSM_Column_Index &cols)
{
	try{
		//printf("line: %s\n", line);
		vector<string> tmpv;
		CStringProcess::Split(line, ",", tmpv);
		if (tmpv.size() < 23){  // 20
			m_pTrace->error("Invalid line \n%s\n", line);
			exit(1);
		}
		prsm.basic_info.nfileID = atoi(tmpv[cols.fileID_col].c_str());
		prsm.basic_info.strSpecTitle = tmpv[cols.title_col];
		prsm.basic_info.nScan = atoi(tmpv[cols.scan_col].c_str());
		prsm.basic_info.nCharge = atoi(tmpv[cols.charge_col].c_str());
		vector<string> splitElems;
		CStringProcess::Split(tmpv[cols.title_col], ".", splitElems);
		int precursor_id = 0;
		if (splitElems.size() < 6 || splitElems[(int)splitElems.size() - 1] != "dta"){ // 不规范的mgf文件，无法解析scan号和precursor rank [TODO?]
		}
		else {
			precursor_id = atoi(splitElems[(int)splitElems.size() - 2].c_str());
		}
		prsm.precursor_id = precursor_id;
		double precurMass = atof(tmpv[cols.precusor_mass_col].c_str());
		double proMass = atof(tmpv[cols.theoretical_mass_col].c_str());
		double ppm = getTolPpm(precurMass, proMass);  //double diffDa = precurMass - proMass; (diffDa - (int)diffDa)*1000000.0 / proMass;
		prsm.basic_info.lfPrecursorMass = precurMass;
		prsm.basic_info.lfProMass = proMass;
		prsm.precursor_error = ppm;    // 记录 mass_diff_ppm绝对值，便于后续计算

		prsm.basic_info.nMatchedPeakNum = atoi(tmpv[cols.matched_peaks_col].c_str());
		prsm.basic_info.strProAC = tmpv[cols.protein_ac_col];
		prsm.basic_info.strProSQ = tmpv[cols.protein_sequence_col];
		prsm.basic_info.vModInfo = tmpv[cols.ptms_col];
		prsm.basic_info.lfScore = atof(tmpv[cols.score_col].c_str());
		prsm.basic_info.lfEvalue = atof(tmpv[cols.evalue_col].c_str());
		prsm.basic_info.cMatchedInfo.nNterm_matched_ions = atoi(tmpv[cols.nmatched_ions_col].c_str());
		prsm.basic_info.cMatchedInfo.nCterm_matched_ions = atoi(tmpv[cols.cmatched_ions_col].c_str());
		prsm.basic_info.cMatchedInfo.lfNtermMatchedIntensityRatio = atof(tmpv[cols.nmatched_intensity_col].c_str());
		prsm.basic_info.cMatchedInfo.lfCtermMatchedIntensityRatio = atof(tmpv[cols.cmatched_intensity_col].c_str());
		prsm.basic_info.nIsDecoy = atoi(tmpv[cols.isDecoy_col].c_str());
		prsm.basic_info.nLabelType = atoi(tmpv[cols.label_col].c_str());
		prsm.basic_info.cFeatureInfo.lfCom_ions_ratio = atof(tmpv[cols.com_ion_col].c_str());
		prsm.basic_info.cFeatureInfo.lfTag_ratio = atof(tmpv[cols.tag_ratio_col].c_str());
		prsm.basic_info.cFeatureInfo.lfPTM_score = atof(tmpv[cols.ptm_score_col].c_str());
		prsm.basic_info.cFeatureInfo.lfFragment_error_std = atof(tmpv[cols.fragment_error_col].c_str());
		prsm.basic_info.cFeatureInfo.lfFragment_error_std = max(0, m_cPara->m_lfFragTol - prsm.basic_info.cFeatureInfo.lfFragment_error_std);
		prsm.svm_score = prsm.basic_info.lfScore;
		//get var mod info
		tmpv.clear();
		if (prsm.basic_info.vModInfo == NONMOD){
			prsm.ptms.push_back(NONMOD);
		}
		else{
			CStringProcess::Split(prsm.basic_info.vModInfo, ";", tmpv);
			for (int mi = 0; mi < tmpv.size(); ++mi){
				size_t rp = tmpv[mi].find(')');
				if (rp != string::npos){
					prsm.ptms.push_back(tmpv[mi].substr(rp + 1));
				}
			}
		}
		m_lfMaxPrecurErr = max(m_lfMaxPrecurErr, prsm.precursor_error);
	}
	catch (exception &ex)
	{
		CErrInfo info("Rerank", "parseResLine", "Invalid line \n" + string(line));
		throw runtime_error(info.Get(ex).c_str());
		return false;
	}
	return true;
}

void Rerank::parseResFile(FILE *fp)
{
	try{
		const int LINE_LEN = 20000;
		char *line = new char[LINE_LEN];
		fgets(line, LINE_LEN, fp);   // read the first line with title
		string title(line);
		while (title.back() == '\r' || title.back() == '\n')  title.pop_back();
		PRSM_Column_Index cols;
		CConfiguration::parseFileTitle(title, ",", cols);
		m_nSample_Num = 0;
		while (fgets(line, LINE_LEN, fp)){
			PrSMFeature prsm;
			if (parseResLine(line, prsm, cols)){
				m_nSample_Num += 1;
				if (m_vPrSMInfo.empty() || prsm.basic_info.strSpecTitle != m_vPrSMInfo.back()[0].basic_info.strSpecTitle){
					vector<PrSMFeature> tmp;
					tmp.push_back(prsm);
					m_vPrSMInfo.push_back(tmp);
				}
				else{  // same spectrum
					m_vPrSMInfo.back().push_back(prsm);
				}
			}
		}
		cout << "Input PrSM Number: " << m_vPrSMInfo.size() << endl;
		delete[] line;
	}
	catch (exception &ex)
	{
		CErrInfo info("Rerank", "parseResFile", "");
		throw runtime_error(info.Get(ex).c_str());
	}
}


void Rerank::writeResultFile()
{
	FILE *fp = fopen(m_strInputFile.c_str(), "wb"); 
	if (fp == NULL)
	{
		CErrInfo info("Rerank", "writeResultFile", "Failed to open file:" + m_strInputFile);
		m_pTrace->error(info.Get());
		throw runtime_error(info.Get().c_str());
		return;
	}

	size_t idx = m_strInputFile.rfind('.');
	string strOutputPath = m_strInputFile.substr(0, idx) + ".top" + to_string(m_cPara->m_nOutputTopK) + ".csv";
	FILE *fp2 = fopen(strOutputPath.c_str(), "wb");   // 
	if (fp2 == NULL)
	{
		CErrInfo info("Rerank", "writeResultFile", "Failed to open file:" + strOutputPath);
		m_pTrace->error(info.Get());
		throw runtime_error(info.Get().c_str());
		return;
	}

	string heads = "FileID,Title,Scan,Charge,Precursor Mass,Theoretical Mass,Mass Diff Da,Mass Diff PPM,";  // 8
	heads += "Protein AC,Protein Sequence,PTMs,Raw Score,Final Score,"; // 5
	heads += "Matched Peaks,Nterm Matched Ions,Cterm Matched Ions,Nterm Matched Intensity Ratio,Cterm Matched Intensity Ratio,";  // 5
	heads += "isDecoy,Label Type";  // 
#ifdef _Debug_Train
	heads += ",Raw_Score,Precursor_Error,Mod_Ratio,";  //SVM_Score,Charge,PrecursorID\t
	heads += "Matched_Intensity_Ratio,Matched_Ion_Ratio,Complementary_Ion_Ratio,Tag_Ratio,PTM_Score,Fragment_Error_STD";
#endif
	heads.push_back('\n');
	try{
		char *buf = new char[BUFFERSIZE];
		// write top 1
		int len = 0;
		len += sprintf(buf + len, heads.c_str());
		for (int s = 0; s < m_vPrSMInfo.size(); ++s) // prsm, 同一张的谱的多个结果之间已经有序
		{
			len += sprintf(buf + len, "%u,%s,%d,%d,", m_vPrSMInfo[s][0].basic_info.nfileID, m_vPrSMInfo[s][0].basic_info.strSpecTitle.c_str(), m_vPrSMInfo[s][0].basic_info.nScan, m_vPrSMInfo[s][0].basic_info.nCharge);
			double dist = m_vPrSMInfo[s][0].basic_info.lfPrecursorMass - m_vPrSMInfo[s][0].basic_info.lfProMass;
			len += sprintf(buf + len, "%f,%f,%.3f,%.1f,", m_vPrSMInfo[s][0].basic_info.lfPrecursorMass, m_vPrSMInfo[s][0].basic_info.lfProMass, dist, dist * 1000000 / m_vPrSMInfo[s][0].basic_info.lfProMass);
			len += sprintf(buf + len, "%s,%s,%s,", m_vPrSMInfo[s][0].basic_info.strProAC.c_str(), m_vPrSMInfo[s][0].basic_info.strProSQ.c_str(), m_vPrSMInfo[s][0].basic_info.vModInfo.c_str());
			len += sprintf(buf + len, "%f,%e,", m_vPrSMInfo[s][0].basic_info.lfScore, m_vPrSMInfo[s][0].basic_info.lfEvalue);
			len += sprintf(buf + len, "%d,%d,%d,%.3f,%.3f,", m_vPrSMInfo[s][0].basic_info.nMatchedPeakNum,
				m_vPrSMInfo[s][0].basic_info.cMatchedInfo.nNterm_matched_ions, m_vPrSMInfo[s][0].basic_info.cMatchedInfo.nCterm_matched_ions,
				m_vPrSMInfo[s][0].basic_info.cMatchedInfo.lfNtermMatchedIntensityRatio, m_vPrSMInfo[s][0].basic_info.cMatchedInfo.lfCtermMatchedIntensityRatio);
			len += sprintf(buf + len, "%d,%u", m_vPrSMInfo[s][0].basic_info.nIsDecoy, m_vPrSMInfo[s][0].basic_info.nLabelType);
#ifdef _Debug_Train
			len += sprintf(buf + len, ",%f,%f,%f,", m_vPrSMInfo[s][0].basic_info.lfScore, m_vPrSMInfo[s][0].precursor_error, m_vPrSMInfo[s][0].mod_ratio);
			len += sprintf(buf + len, "%f,%f,%f,%f,%f,%f", m_vPrSMInfo[s][0].basic_info.cMatchedInfo.lfNtermMatchedIntensityRatio + m_vPrSMInfo[s][0].basic_info.cMatchedInfo.lfCtermMatchedIntensityRatio,
				(m_vPrSMInfo[s][0].basic_info.cMatchedInfo.nNterm_matched_ions + m_vPrSMInfo[s][0].basic_info.cMatchedInfo.nCterm_matched_ions) / (2.0*(m_vPrSMInfo[s][0].basic_info.strProSQ.length() - 1)),
				m_vPrSMInfo[s][0].basic_info.cFeatureInfo.lfCom_ions_ratio, m_vPrSMInfo[s][0].basic_info.cFeatureInfo.lfTag_ratio,
				m_vPrSMInfo[s][0].basic_info.cFeatureInfo.lfPTM_score, m_vPrSMInfo[s][0].basic_info.cFeatureInfo.lfFragment_error_std);
#endif
			buf[len++] = '\n';
			if (len + 2 * BUFLEN > BUFFERSIZE){
				buf[len] = '\0';
				fwrite(buf, 1, len, fp);
				len = 0;
			}

		}
		if (len){
			buf[len] = '\0';
			fwrite(buf, 1, len, fp);
		}
		fclose(fp);

		len = 0;
		len += sprintf(buf + len, heads.c_str());
		// write top k
		for (int s = 0; s < m_vPrSMInfo.size(); ++s) // prsm, 同一张的谱的多个结果之间已经有序
		{
			for (int k = 0; k < m_vPrSMInfo[s].size(); ++k){
				len += sprintf(buf + len, "%u,%s,%d,%d,", m_vPrSMInfo[s][k].basic_info.nfileID, m_vPrSMInfo[s][k].basic_info.strSpecTitle.c_str(), m_vPrSMInfo[s][k].basic_info.nScan, m_vPrSMInfo[s][k].basic_info.nCharge);
				double dist = m_vPrSMInfo[s][k].basic_info.lfPrecursorMass - m_vPrSMInfo[s][k].basic_info.lfProMass;
				len += sprintf(buf + len, "%f,%f,%.3f,%.1f,", m_vPrSMInfo[s][k].basic_info.lfPrecursorMass, m_vPrSMInfo[s][k].basic_info.lfProMass, dist, dist * 1000000 / m_vPrSMInfo[s][k].basic_info.lfProMass);
				len += sprintf(buf + len, "%s,%s,%s,", m_vPrSMInfo[s][k].basic_info.strProAC.c_str(), m_vPrSMInfo[s][k].basic_info.strProSQ.c_str(), m_vPrSMInfo[s][k].basic_info.vModInfo.c_str());
				len += sprintf(buf + len, "%f,%e,", m_vPrSMInfo[s][k].basic_info.lfScore, m_vPrSMInfo[s][k].basic_info.lfEvalue);
				len += sprintf(buf + len, "%d,%d,%d,%.3f,%.3f,", m_vPrSMInfo[s][k].basic_info.nMatchedPeakNum,
					m_vPrSMInfo[s][k].basic_info.cMatchedInfo.nNterm_matched_ions, m_vPrSMInfo[s][k].basic_info.cMatchedInfo.nCterm_matched_ions,
					m_vPrSMInfo[s][k].basic_info.cMatchedInfo.lfNtermMatchedIntensityRatio, m_vPrSMInfo[s][k].basic_info.cMatchedInfo.lfCtermMatchedIntensityRatio);
				len += sprintf(buf + len, "%d,%u", m_vPrSMInfo[s][k].basic_info.nIsDecoy, m_vPrSMInfo[s][k].basic_info.nLabelType);
#ifdef _Debug_Train
				len += sprintf(buf + len, ",%f,%f,%f,", m_vPrSMInfo[s][k].basic_info.lfScore, m_vPrSMInfo[s][k].precursor_error, m_vPrSMInfo[s][k].mod_ratio);
				len += sprintf(buf + len, "%f,%f,%f,%f,%f,%f", m_vPrSMInfo[s][k].basic_info.cMatchedInfo.lfNtermMatchedIntensityRatio + m_vPrSMInfo[s][k].basic_info.cMatchedInfo.lfCtermMatchedIntensityRatio,
					(m_vPrSMInfo[s][k].basic_info.cMatchedInfo.nNterm_matched_ions + m_vPrSMInfo[s][k].basic_info.cMatchedInfo.nCterm_matched_ions) / (2.0*(m_vPrSMInfo[s][k].basic_info.strProSQ.length() - 1)),
					m_vPrSMInfo[s][k].basic_info.cFeatureInfo.lfCom_ions_ratio, m_vPrSMInfo[s][k].basic_info.cFeatureInfo.lfTag_ratio,
					m_vPrSMInfo[s][k].basic_info.cFeatureInfo.lfPTM_score, m_vPrSMInfo[s][k].basic_info.cFeatureInfo.lfFragment_error_std);
#endif
				buf[len++] = '\n';
				if (len + 2 * BUFLEN > BUFFERSIZE){
					buf[len] = '\0';
					fwrite(buf, 1, len, fp2);
					len = 0;
				}
			}

		}
		if (len){
			buf[len] = '\0';
			fwrite(buf, 1, len, fp2);
		}
		fclose(fp2);

		delete[]buf;
		
	}
	catch (exception &ex) {
		CErrInfo info("Rerank", "writeResultFile", "");
		m_pTrace->error(info.Get(ex));
		throw runtime_error(info.Get(ex).c_str());
		return;
	}
}

void Rerank::RemoveTmpFiles()
{
	if ((_access(m_strTrainFile.c_str(), 0)) != -1){
		if (remove(m_strTrainFile.c_str()) != 0){
			cerr << "Error deleting file: " << m_strTrainFile << endl;
		}
	}
	if ((_access(m_strTestFile.c_str(), 0)) != -1){
		if (remove(m_strTestFile.c_str()) != 0){
			cerr << "Error deleting file: " << m_strTestFile << endl;
		}
	}
	if ((_access(m_strModelFile.c_str(), 0)) != -1){
		if (remove(m_strModelFile.c_str()) != 0){
			cerr << "Error deleting file: " << m_strModelFile << endl;
		}
	}
	if ((_access(m_strOutFile.c_str(), 0)) != -1){
		if (remove(m_strOutFile.c_str()) != 0){
			cerr << "Error deleting file: " << m_strOutFile << endl;
		}
	}
	//cout << "temporary files removed." << endl;
}

//bool Rerank::samplePartition(vector<vector<double> > &train, vector<vector<double> > &test)
//{
//	if (m_vPrSMInfo.empty())  return false;
//	try{
//		vector<PrSMFeature> CP_prsm_info(m_vPrSMInfo);
//		vector<int> topPrSMs, lastPrSMs;
//		vector<pair<int, double>> decoyPrSMs;
//		int pos = 0, neg = 0;
//		topPrSMs.push_back(0);
//		if (CP_prsm_info[0].basic_info.nIsDecoy == 1){
//			decoyPrSMs.push_back(make_pair(0, CP_prsm_info[0].basic_info.lfScore));
//		}
//		int cnt = 1;
//		for (int i = 1; i<CP_prsm_info.size(); ++i){  // 取每张谱的第一名进行排序卡fdr
//			if (CP_prsm_info[i].basic_info.strSpecTitle != CP_prsm_info[i - 1].basic_info.strSpecTitle){
//				topPrSMs.push_back(i);
//				if (cnt >= m_cPara->m_nOutputTopK){
//					lastPrSMs.push_back(i-1);
//				}
//				cnt = 1;
//			}
//			else{
//				++cnt;
//			}
//			if (CP_prsm_info[i].basic_info.nIsDecoy == 1){
//				decoyPrSMs.push_back(make_pair(i, CP_prsm_info[i].basic_info.lfScore));
//			}
//		}
//		sort(topPrSMs.begin(), topPrSMs.end(), [&CP_prsm_info](int l, int r){return CP_prsm_info[l].basic_info.lfScore > CP_prsm_info[r].basic_info.lfScore; });
//		vector<double> fdrs;
//		double decoynum = 0, targetnum = 0.001;
//		double max_precur_err = 0;
//		for (size_t i = 0; i < topPrSMs.size(); ++i)
//		{    // sort by score, the higher the better
//			if (CP_prsm_info[topPrSMs[i]].basic_info.nIsDecoy == 1) decoynum += 1;
//			else targetnum += 1;
//			fdrs.push_back(1.0 * decoynum / targetnum);
//
//			max_precur_err = max(max_precur_err, CP_prsm_info[topPrSMs[i]].precursor_error);
//		}
//		int j = fdrs.size() - 1;
//		// 
//		vector<int> pos_hist(PrecursorErrorBin + 1, 0);
//		vector<double> pos_err_freq(PrecursorErrorBin + 1, 0);
//		double dis = max_precur_err / PrecursorErrorBin;
//		//unordered_map<string,int> mod_hist;
//		//unordered_map<string,double> mod_freq;
//		m_pTrace->info("The FDR threshold for positive samples is %f", m_lfThreshold);
//		while (j >= 0 && fdrs[j] > m_lfThreshold)
//		{   // [wrm] isDecoy = 1表示反库结果，为0表示正库过阈值的结果，为-1表示正库未过阈值的结果
//			if (CP_prsm_info[topPrSMs[j]].basic_info.nIsDecoy == 0){
//				CP_prsm_info[topPrSMs[j]].basic_info.nIsDecoy = -1;
//			}
//			--j;
//		}
//		while (j >= 0){  //
//			if (CP_prsm_info[topPrSMs[j]].basic_info.nIsDecoy == 0){
//				++ pos;
//				++ pos_hist[(unsigned int)floor(CP_prsm_info[topPrSMs[j]].precursor_error / dis)];
//				/*for(int mi = 0; mi < topPrSMs[j].varmods.size(); ++mi){
//				++ mod_hist[topPrSMs[j].varmods[mi]];
//				}*/
//
//			}
//			--j;
//		}
//		neg = decoyPrSMs.size();
//		cout << "Decoy Number: " << neg << endl;
//		if (neg > pos * 2.5){
//			neg = 2.5 * pos;
//			sort(decoyPrSMs.begin(), decoyPrSMs.end(), cmp_by_score);
//		}
//		// neg += lastPrSMs.size();
//		m_pTrace->info("[Training Set] %d Positive Samples, %d Negative Samples.", pos, neg);
//
//		if(pos < 50 || neg < 50){   // 200
//			m_pTrace->info("Training sample is too small...");
//			return false;
//		}
//
//		pos_err_freq[Precursor_Error_Bin] = pos_hist[Precursor_Error_Bin] * 1.0 / pos;
//		for (int i = Precursor_Error_Bin - 1; i >= 0; --i){
//			pos_err_freq[i] = pos_err_freq[i + 1] + pos_hist[i] * 1.0 / pos;
//		}
//		vector<double> tmp1(pos + neg, 0);
//		vector<vector<double> > trainset(11, tmp1);
//		vector<double> tmp2(CP_prsm_info.size(), 0);
//		vector<vector<double> > testset(11, tmp2);
//		// generate testing set, including training set
//		for (int i = 0; i<CP_prsm_info.size(); ++i){
//			testset[0][i] = 0;  // label
//			int binx = (unsigned int)floor(prsm_info[i].precursor_error / dis);
//			prsm_info[i].precursor_error = pos_err_freq[binx > Precursor_Error_Bin ? Precursor_Error_Bin : binx];
//			CP_prsm_info[i].precursor_error = prsm_info[i].precursor_error;
//			testset[1][i] = 0;  // svm score
//			testset[2][i] = CP_prsm_info[i].basic_info.lfScore; // bm25score
//			testset[3][i] = 1.0*CP_prsm_info[i].precursor_id;
//			testset[4][i] = CP_prsm_info[i].precursor_error;
//
//			testset[5][i] = CP_prsm_info[i].basic_info.cMatchedInfo.lfNtermMatchedIntensityRatio + CP_prsm_info[i].basic_info.cMatchedInfo.lfCtermMatchedIntensityRatio;
//			testset[6][i] = (CP_prsm_info[i].basic_info.cMatchedInfo.nNterm_matched_ions + CP_prsm_info[i].basic_info.cMatchedInfo.nCterm_matched_ions) / 2.0*(CP_prsm_info[i].basic_info.strProSQ.length() - 1);
//			testset[7][i] = CP_prsm_info[i].basic_feature.lfCom_ions_ratio;
//			testset[8][i] = CP_prsm_info[i].basic_feature.lfTag_ratio;
//			testset[9][i] = CP_prsm_info[i].basic_feature.lfPTM_score;
//			testset[10][i] = CP_prsm_info[i].basic_feature.lfFragment_error_std;
//		}
//		// normalization
//		for (int i = 2; i<5; ++i){
//			normalization(testset[i], e_minmax);
//		}
//		// generate training set
//		FILE *fneg = fopen("negative.txt", "w");
//		FILE *fpos = fopen("positive.txt", "w");
//		int k = 0;
//		for (int i = 0; i < neg; ++i){
//			//if (CP_prsm_info[i].basic_info.nIsDecoy == 1){  // negative
//			trainset[0][k] = -1.0; // label
//			for (int j = 1; j<11; ++j){
//				trainset[j][k] = testset[j][decoyPrSMs[i].first];
//			}
//			++k;
//			fprintf(fneg, "%s %f\n", CP_prsm_info[i].basic_info.strSpecTitle.c_str(), CP_prsm_info[i].basic_info.lfScore);
//			
//		}
//		for (int i = 0; i<topPrSMs.size(); ++i){
//			if (CP_prsm_info[topPrSMs[i]].basic_info.nIsDecoy == 0){  // positive
//				trainset[0][k] = 1.0;   // label	
//				for (int j = 1; j<11; ++j){
//					trainset[j][k] = testset[j][topPrSMs[i]];
//				}
//				++k;
//				fprintf(fpos, "%s %f\n", CP_prsm_info[topPrSMs[i]].basic_info.strSpecTitle.c_str(), CP_prsm_info[topPrSMs[i]].basic_info.lfScore);
//			}
//		}
//		// negative
//		//for (int i = 0; i < lastPrSMs.size(); ++i){
//		//	trainset[0][k] = -1.0;   // label	
//		//	for (int j = 1; j<11; ++j){
//		//		trainset[j][k] = testset[j][lastPrSMs[i]];
//		//	}
//		//	++k;
//		//	fprintf(fneg, "%s %f\n", CP_prsm_info[lastPrSMs[i]].basic_info.strSpecTitle.c_str(), CP_prsm_info[lastPrSMs[i]].basic_info.lfScore);
//		//}
//		fclose(fneg);
//		fclose(fpos);
//
//		trainset.swap(train);
//		testset.swap(test);
//	}
//	catch (exception &ex) {
//		CErrInfo info("Rerank", "samplePartition", "error!");
//		m_pTrace->info(info.Get(ex).c_str());
//		throw runtime_error(info.Get(ex).c_str());
//		return false;
//		//exit(-1);
//	}
//	return true;
//}



/** class FeatureNormalization
**
**/

FeatureNormalization::FeatureNormalization() : m_tSampleSize(0), m_vFeatureMax(MaxFeatureNum, -FLT_MAX), m_vFeatureMin(MaxFeatureNum, FLT_MAX), 
m_vFeatureSum(MaxFeatureNum, 0), m_vFeatureSumXX(MaxFeatureNum, 0)
{

}

FeatureNormalization::~FeatureNormalization()
{

}

void FeatureNormalization::Normalization(vector<double> &nums, int l, int r, NormalizationMethod type)
{
	switch (type)
	{
	case e_zscore:
		zscore_normalization(nums, l, r);
		break;
	case e_minmax:
		minmax_normalization(nums, l, r);
		break;
	case e_log:
		log_normalization(nums, l, r);
		break;
	case e_atan:  // 反余切函数转换
		atan_normalization(nums, l, r);
		break;
	default:
		break;
	}

}

void FeatureNormalization::minmax_normalization(vector<double> &nums, int l, int r)
{
	double minx = DBL_MAX, maxx = 0;
	for (int i = l; i < r; ++i){
		minx = m_vFeatureMin[i];
		maxx = m_vFeatureMax[i];
		if (fabs(minx - maxx) < DOUCOM_EPS){
			nums[i] = 0.0;
		}
		else{
			nums[i] = (nums[i] - minx) / (maxx - minx);
		}
	}
}

void FeatureNormalization::log_normalization(vector<double> &nums, int l, int r)
{
	for (int i = l; i<r; ++i){
		nums[i] = log10(nums[i]);
	}
}

void FeatureNormalization::zscore_normalization(vector<double> &nums, int l, int r)
{
	double avg_x = 0, sigma = 0;
	for (int i = l; i < r; ++i){
		avg_x = m_vFeatureSum[i] / m_tSampleSize;
		sigma = sqrt((m_vFeatureSumXX[i] / m_tSampleSize) - avg_x*avg_x);
		nums[i] = (nums[i] - avg_x) / sigma;
	}
}

void FeatureNormalization::atan_normalization(vector<double> &nums, int l, int r)
{
	for (int i = l; i < r; ++i){
		nums[i] = atan(nums[i]) * 2 / pi;
	}
}

void FeatureNormalization::UpdateFeatureStatistics(vector<double> &nums, int l, int r, NormalizationMethod type)
{
	m_tSampleSize += 1;
	switch (type)
	{
	case e_zscore:
		updateSum(nums, l, r);
		updateSumXX(nums, l, r);
		break;
	case e_minmax:
		updateMaxMin(nums, l, r);
		break;
	case e_log:
		break;
	case e_atan:  // 反余切函数转换
		break;
	default:
		break;
	}
}

void FeatureNormalization::updateMaxMin(vector<double> &nums, int l, int r)
{
	for (int i = l; i < r; ++i){
		m_vFeatureMax[i] = max(m_vFeatureMax[i], nums[i]);
		m_vFeatureMin[i] = min(m_vFeatureMax[i], nums[i]);
	}
}
void FeatureNormalization::updateSum(vector<double> &nums, int l, int r)
{
	for (int i = l; i < r; ++i){
		m_vFeatureSum[i] += nums[i];
	}
}
void FeatureNormalization::updateSumXX(vector<double> &nums, int l, int r)
{
	for (int i = l; i < r; ++i){
		m_vFeatureSumXX[i] += (nums[i] * nums[i]);
	}
}

void FeatureNormalization::Clear()
{
	m_tSampleSize = 0;
	for (int i = 0; i < MaxFeatureNum; ++i){
		m_vFeatureMax[i] = -FLT_MAX;
		m_vFeatureMin[i] = FLT_MAX;
		m_vFeatureSum[i] = 0;
		m_vFeatureSumXX[i] = 0;
	}
}
