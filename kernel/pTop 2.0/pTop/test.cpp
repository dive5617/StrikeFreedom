/// test file

#include <cstdio>
#include <cstring>
#include <string>
#include <algorithm>
#include <cmath>

#include "flow.h"
#include "util.h"
#include "test.h"
#include "sdk.h"

using namespace std;




void Test::testRerank(string configfile)
{
	CConfiguration* cPara = new CConfiguration(configfile);
	cPara->GetAllConfigPara(); // Main parameters from config.ini
	cPara->writeConfigFile();
	Rerank *rerank = new Rerank(cPara, "J:\\pTop\\Data\\Yeast_HD\\search_task_20170327152501\\20160306_YEAST_controlD_HD_A4_1_HCDFT\\20160306_YEAST_controlD_HD_A4_1_HCDFT.qry.csv");
	rerank->run();
	delete rerank;
	delete cPara;
}

void Test::testFilter(string configfile)
{
	try
	{
		CConfiguration* cPara = new CConfiguration(configfile);
		cPara->GetAllConfigPara(); // Main parameters from config.ini
		cPara->writeConfigFile();
		cout << "[pTop] Begin to filter..." << endl;
		vector<string> qryRes, filRes;
		vector<FILE *> labl;
		string m_vstrQryResFile = "L:\\pTop\\Data\\Yeast_HD\\20160306_YEAST_controlD_HD_A4_1_HCDFT\\search_task_20160608102158_query.txt";
		string m_testRes = "L:\\pTop\\Data\\Yeast_HD\\20160306_YEAST_controlD_HD_A4_1_HCDFT\\test.csv";
		string m_labl = "L:\\pTop\\Data\\Yeast_HD\\20160306_YEAST_controlD_HD_A4_1_HCDFT\\test.plabel";
		qryRes.push_back(m_vstrQryResFile);
		filRes.push_back(m_testRes);
		//CResFilter filter(0.01, qryRes, filRes, 1);
		//FILE *fp = fopen(m_labl.c_str(),"w");
		//labl.push_back(fp);
		//vector<int> m_vIdScan;
		//CPrePTMForm *m_cPTMForms = new CPrePTMForm();
		//vector<MODINI> fixmodInfo;
		//filter.Run(labl, m_cPTMForms, fixmodInfo, m_vIdScan);

		//for(size_t i = 0; i < labl.size(); ++i)
		//{
		//	if(labl[i] != NULL)
		//	{
		//		fclose(labl[i]);
		//		labl[i] = NULL;
		//	}
		//}
		//cout << "[pTop] Fileter finished!" << endl;
		//cout << "[pTop] The results in "<<m_cPara->m_strFilterResFile<<endl;
		delete cPara;
	}
	catch(exception & e) 
	{
		CErrInfo info("CMainFlow", "RunMainFlow", "Filter failed.");
		throw runtime_error(info.Get(e).c_str());
	}
	catch(...) 
	{
		CErrInfo info("CMainFlow", "RunMainFlow", "caught an unknown exception from Filter.");
		throw runtime_error(info.Get().c_str());
	}
}