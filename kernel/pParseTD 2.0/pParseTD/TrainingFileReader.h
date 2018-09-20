#ifndef _TrainingSetReader_H
#define _TrainingSetReader_H
#include <vector>
#include <string>
using namespace std;
class TrainingSetreader
{
public:
	TrainingSetreader(string Filename, double mz_error_tol);
	~TrainingSetreader();

	void InitTrainData();
	void parse(char * buf, int len);
	bool CheckScanMZCharge(int &scan,double &MZ,int charge);
	bool CheckMonoList(int &scan,double &MZ,int charge);
	bool CheckTopNList(int &scan,double &MZ,int charge);
	bool CheckMARSList(int &scan,double &MZ,int charge);
	bool CheckTopThreeScanMZCharge(int &scan,double &MZ,int charge);
	int sumCheckFlag();
	void OutPutUnchecked();
	int sumCheckTopThreeFlag();
	int TotalPrecursors();
	void ResetCheckFlag();
	void OutPutUncheckedIntoFile(string strfilename);
	void OutputCuases(string CauseFile);

private:
	std::vector<int> MS2ScanNoList;
	std::vector<double> MZList;
	std::vector<int> chargeList;
	std::vector<double> evalueList;

	// causes of pParse+ failure on some precursors
	std::vector<int> m_causes_not_in_mono_list;
	std::vector<int> m_causes_not_in_ranking_topn;
	std::vector<int> m_causes_abandon_by_mars;

	std::vector<int> CheckFlag;   //判断是否被导出 mars
	std::vector<int> TopThreeCheckflag;
	string m_sTrainSetfile;
	int Index;                // 在默认非降序列的前提下。设置好Index可以减少不必要的检索代价
	double m_mzerror_tol;
};
#endif

