#include <iostream>
#include <cstdio>
#include <ctime>
#include <cstring>
#include <limits.h>
#include <direct.h>
#include <string>
#include <errno.h>
#include <unordered_map>

#include "dirent.h"
#include "sdk.h"
#include "util.h"
#include "config.h"

using namespace std;

/**
*	class CConfiguration
*/

CConfiguration::CConfiguration(const string strFile) : m_strIniFile(strFile), m_eWorkFlow(WorkFlowType::TagFlow),
	m_lfPrecurTol(3.2), m_lfFragTol(15), m_lfThreshold(0.01), m_lfMaxModMass(MAX_MOD_MASS), 
	m_lfMinFragMass(2 * 57.021464), m_lfMaxFragMass(50000), m_lfMaxTruncatedMass(20000),
	m_nThreadNum(1), m_nOutputTopK(10), m_nMaxModNum(5), m_nUnexpectedModNum(0),
	m_bNeutralLoss(false), m_bRerank(true), m_bSeparateFiltering(true), m_bSecondSearch(false), m_bTagLenInc(true),
	calAX(false), calBY(false), calCZ(false),
	m_strpQuant(""),m_strBIonsSites(BIONSITES), m_strYIonsSites(YIONSITES)
{

}
CConfiguration::~CConfiguration()
{

}

void CConfiguration::GetAllConfigPara()
{
	try
	{
		/*size_t found = m_strIniFile.find_last_of(".");
		if(found == string::npos || ( m_strIniFile.substr(found, 4).compare(".ini") != 0 
			&& m_strIniFile.substr(found, 4).compare(".INI") != 0 ) )
		{
			m_strIniFile.append(".ini");
		}*/
		FILE *fp = fopen(m_strIniFile.c_str(), "r");
		if(NULL == fp)
		{
			cout << "Cannot open the config file: " << m_strIniFile << endl;
			exit(0);
		}
		fclose(fp);
		//cout << m_strIniFile << endl;
		COptionTool config(m_strIniFile);

		m_strDBFile = config.GetString("file", "Database", "test.fasta");

		m_strpParseTD = config.GetString("file", "pParseTD_cfg", "pParseTD.cfg");
		m_strpQuant = config.GetString("file", "pQuant_cfg", "pQuant.cfg");   // 
		m_strOutputPath = config.GetString("file","outputpath","");   // 

		//m_strSpecFile = config.GetString("spectrum", "Spec_path", "test.mgf");
		
		m_strFileType = config.GetString("spectrum", "input_format", "RAW");
		for(size_t i = 0; i < m_strFileType.length(); ++i)
		{
			if(m_strFileType[i] >= 'a' && m_strFileType[i] <= 'z')
			{
				m_strFileType[i] += 'A' - 'a';
			}
		}

		int msmsNum = config.GetInteger("spectrum", "msmsnum", 0);
		char msmsPath[PATH_MAX];
		//ms1Str = "";
		//pf1Str = "";
		
		for(int i = 1; i <= msmsNum; ++i)
		{
			sprintf(msmsPath, "msmspath%d", i);
			string strMSPath = config.GetString("spectrum", msmsPath, "");
			m_vSpecFiles.push_back(strMSPath);
		}

		m_strActivation = config.GetString("spectrum", "Activation", "HCD");
		m_lfPrecurTol = config.GetDouble("spectrum", "Precursor_Tolerance", 3.2);
		m_lfFragTol = config.GetDouble("spectrum", "Fragment_Tolerance", 20.0);
		m_nThreadNum = config.GetInteger("param", "thread_num", 1);
		if (m_nThreadNum > getCPUCoreNo()){
			m_nThreadNum = getCPUCoreNo();
		}
		m_eWorkFlow = (WorkFlowType)config.GetInteger("param", "workflow", 0);
		m_nOutputTopK = config.GetInteger("param", "output_top_k", 10);
		m_lfMaxTruncatedMass = config.GetDouble("param", "max_truncated_mass", 20000);
		m_bSecondSearch = (config.GetInteger("param", "second_search", 0) == 1);

		m_lfThreshold = config.GetDouble("filter", "threshold", 0.01);
		m_bSeparateFiltering = (config.GetInteger("filter", "separate_filtering", 1) == 1);

		m_nUnexpectedModNum = config.GetInteger("modify", "unexpected_mod", 0);
		m_nMaxModNum = config.GetInteger("modify", "Max_mod_num", 5);
		m_lfMaxModMass = config.GetDouble("modify", "Max_mod_mass", MAX_MOD_MASS);
		int fixNum = config.GetInteger("fixmodify", "fixedModify_num", 0);
		int varNum = config.GetInteger("modify", "Modify_num", 0);

		char tmpCh[20];
		for(int i = 1; i <= fixNum; ++i)
		{
			sprintf(tmpCh, "fix_mod%d", i);
			string fixName = config.GetString("fixmodify", tmpCh, "");
			m_vFixMod.push_back(fixName);
		}
		for(int i = 1; i <= varNum; ++i)
		{
			sprintf(tmpCh, "var_mod%d", i);
			string varName = config.GetString("modify", tmpCh, "");
			m_vVarMod.push_back(varName);
		}
        // [wrm] used for quantification. 2015.11.12
		string quant = config.GetString("quantify","quant","1|none");
		if(quant != "1|none" && quant != "1|None"){		
			vector<string> tmpQuant;
			CStringProcess::Split(quant,"|",tmpQuant);
			int label_num = atoi(tmpQuant[0].c_str());
			if(tmpQuant.size() > label_num){
				for(int i=0; i<label_num; ++i){
					LABEL_QUANT qt = LABEL_QUANT(0,0);
					size_t last = 0;
					size_t index = tmpQuant[i+1].find(":", last);
					if(index != string::npos){ //
						unordered_map<char, unordered_map<string, string>> quant_R;
						unordered_map<string, unordered_map<string, string>> quant_M;
						while(index != string::npos && index > 0){
							char ctype = tmpQuant[i+1][index-1]; // R / M
							size_t l_idx = tmpQuant[i+1].find("{", last);
							size_t r_idx = tmpQuant[i+1].find("}", last);
							string name = tmpQuant[i+1].substr(index+1,l_idx-index-1);
							string elements = tmpQuant[i+1].substr(l_idx+1,r_idx-l_idx-1);
							size_t comma = elements.find(",");
							string preElement = elements.substr(0,comma);
							string labelElement = elements.substr(comma+1);
							if(ctype=='R'){
								if(name[0]>='A' && name[0]<='Z'){
									if (quant_R.find(name[0]) == quant_R.end()){
										quant_R.insert(make_pair(name[0], unordered_map<string, string>()));
									}
									quant_R[name[0]].insert(make_pair(preElement, labelElement));
									qt.labelR_num ++;
								}else if(name[0]=='*'){
									for(int l=0; l<26; ++l){
										if (quant_R.find((l + 'A')) == quant_R.end()){
											quant_R.insert(make_pair((l + 'A'), unordered_map<string, string>()));
										}
										quant_R[l + 'A'].insert(make_pair(preElement, labelElement));
										qt.labelR_num ++;
									}
								}else{
									throw("Invalid label!");
								}
							}else if(ctype=='M'){
								if (quant_M.find(name) == quant_M.end()){
									quant_M.insert(make_pair(name, unordered_map<string, string>()));
								}
								quant_M[name].insert(make_pair(preElement,labelElement));
								qt.labelM_num ++;
							}
							last = r_idx + 1;
							index = tmpQuant[i+1].find(":", last);
						}
						qt.quant_R = quant_R;
						qt.quant_M = quant_M;
						qt.label_name = tmpQuant[i+1];  // [wrm]  added by wrm 2016.03.01.
						m_quant.push_back(qt);
					}else{
						qt.label_name = tmpQuant[i+1];
						m_quant.push_back(qt);  // none
					}
				}
			}
		}
		CallpParseTD();  // [wrm?] 可由界面调用pParseTD
		setFragmentType();
		_CheckPath();
	} 
	catch(exception & e) 
	{
		CErrInfo info("CConfiguration", "GetAllConfigPara", "GetConfigInfo failed.");
		cout << "Please check the parameter file pTop.cfg!" << endl;
		throw runtime_error(info.Get(e).c_str());
	}
	catch(...) 
	{
		CErrInfo info("CConfiguration", "GetAllConfigPara", "caught an unknown exception from GetConfigInfo.");
		cout << "Please check the path of file config.ini!" << endl;
		throw runtime_error(info.Get().c_str());
	}
}

void CConfiguration::setFragmentType()
{
	if (m_strActivation.compare("ETD") == 0)
	{
		calCZ = true;
	}
	else if (m_strActivation.compare("UVPD") == 0){
		calAX = true;
		calBY = true;
		calCZ = true;
	}
	else if (m_strActivation.compare("ETHCD") == 0 || m_strActivation.compare("ETCID") == 0){
		calCZ = true;
		calBY = true;
	}
	else{
		calBY = true;
	}
}

//如果输入的是RAW，则返回true，其他情况返回false
//若输入为路径，则最后一个字符一定是'\'
void CConfiguration::CallpParseTD()
{
	if(0 == m_vSpecFiles.size())
	{
		cout << "[pTop] There is no input file!" << endl;
		return;
	}

	if(0 == m_strFileType.compare("MGF"))
	{
		m_inFileType = MFT_MGF;
		return;
	}

	if(0 == m_strFileType.compare("PF2"))
	{
		m_inFileType = MFT_PF;
		return;
	}

	// if(0 == m_strFileType.compare("RAW") || 0 == m_strFileType.compare("PATH"))
	// call pParseTD
	try
	{
		m_inFileType = MFT_PF;
		FILE *fp = fopen(m_strpParseTD.c_str(), "r");
		if(NULL == fp)
		{
			throw runtime_error("[Error] Cannot open the config file of pParseTD: " + m_strpParseTD);
		} else {
			fclose(fp);
		}
		string mscmdline = "pParseTD.exe \"" + m_strpParseTD + "\""; //配置文件路径
		cout << "[Call pParseTD] " << mscmdline << endl;
		int err_no = system(mscmdline.c_str());
		if(err_no){  // 调用pParseTD异常
			throw err_no;
		}
	}
	catch(exception & e) 
	{
		//CErrInfo info("CConfiguration", "CallpParseTD", "Call pParseTD failed.");
		//throw runtime_error(info.Get(e).c_str());
		string info = e.what();
		ofstream ofs("pTop.err.log", ios::app);
		ofs << info;
		cout << info << endl;
		exit(1);
	}
	catch(...) 
	{
		//CErrInfo info("CConfiguration", "CallpParseTD", "caught an unknown exception from Calling pParseTD.");
		//throw runtime_error(info.Get().c_str());
		string info = "[pTop] Call pParseTD failed.";
		ofstream ofs("pTop.err.log", ios::app);
		ofs << info;
		cout << info << endl;
		exit(1);
	}


	if(0 == m_strFileType.compare("PATH")) // path
	{
		string pathName = m_vSpecFiles[0];
		m_vSpecFiles.clear();
		_GetFilelistbyDirent(pathName, ".pf2", m_vSpecFiles);
	} else {	
		try
		{   // 检查谱图文件是否存在
			string actType = m_strActivation;
			if(actType.compare("UVPD") == 0)
			{
				actType = "HCD";
			}
			size_t i = 0;
			while(i < m_vSpecFiles.size())
			{
				m_vSpecFiles[i].erase(m_vSpecFiles[i].end() - 4, m_vSpecFiles[i].end());
				string tmpFile = m_vSpecFiles[i] + "_" + actType + "FT.pf2";
				FILE *fp = fopen(tmpFile.c_str(), "r");
				if(fp != NULL)
				{   // 对于一个raw，pXtract导出时会生成多个临时文件，如CID碎裂的会得到CID_FT.mgf和CID_IT.mgf，这里也是先枚举，再看看这些文件是否存在
					m_vSpecFiles[i].append("_" + actType + "FT.pf2");
					fclose(fp);
					++i;
				} else {
					fclose(fp);
					tmpFile = m_vSpecFiles[i] + "_" + actType + "IT.pf2";
					fp = fopen(tmpFile.c_str(), "r");
					if(fp != NULL)
					{
						m_vSpecFiles[i].append("_" + actType + "IT.pf2");
						fclose(fp);
						++i;
					} else {
						fclose(fp);
						cout << "[Warning] Can not open file: " << m_vSpecFiles[i].c_str() << "_" << actType.c_str() << "FT.pf2" << endl;		
						m_vSpecFiles.erase(m_vSpecFiles.begin() + i);
					}
				}
			}
		} 
		catch(exception & e) 
		{
			CErrInfo info("CConfiguration", "CallpParseTD", "Set Input and Output failed.");
			throw runtime_error(info.Get(e).c_str());
		}
		catch(...) 
		{
			CErrInfo info("CConfiguration", "CallpParseTD", "caught an unknown exception from Setting Input and Output.");
			throw runtime_error(info.Get().c_str());
		}
	}
}

string CConfiguration::GetTimeStr()
{
	time_t tt = time(NULL);//这句返回的只是一个时间戳
	tm* t = localtime(&tt);
	char chtime[128];
	sprintf(chtime, "%d%02d%02d%02d%02d%02d", 
	t->tm_year + 1900,
	t->tm_mon + 1,
	t->tm_mday,
	t->tm_hour,
	t->tm_min,
	t->tm_sec);
	string strtime(chtime);
	return strtime;
}

//获取指定路径下的所有指定类型的文件 fileType = ".raw"
void CConfiguration::_GetFilelistbyDirent(string &filepath, string fileType, vector<string> &fileList)
{
	DIR *dir = opendir(filepath.c_str());
	if (dir == NULL) 
	{
		cout << "[Error] Failed to open the directory: " << filepath << endl;
		exit(0);
	}
	struct dirent *de;
	while(NULL != (de = readdir(dir)))
	{
		if (de->d_name[0] == '.') 
		{
			continue;
		}
		string FileName = de->d_name;

		size_t dotpos = FileName.find_last_of('.');
		if(dotpos == string::npos)
		{
			continue;
		}
		string fileSuffix = FileName.substr(dotpos);
		for (int i = 1; i < (int)fileSuffix.size(); i++) 
		{
			fileSuffix[i] = tolower(fileSuffix[i]);
		}
		if(fileSuffix.compare(fileType) != 0)
		{
			continue;
		}

		string fileName = filepath;
		fileName.append(de->d_name);
		fileList.push_back(fileName);
	}
	closedir(dir);
}

// 检查路径
void CConfiguration::_CheckPath()
{
	string strtime = GetTimeStr(); //"20170305173508"; //
    string dir = "";
	vector<string> newDirs;
	for (size_t i = 0; i < m_vSpecFiles.size(); ++i)
	{
		string inFile = m_vSpecFiles[i];
		size_t found = inFile.find_last_of(cSlash);
		string filename = "";
		if (found == string::npos)
		{
			filename = inFile;
		}
		else {
			filename = inFile.substr(found + 1);
			if (dir == ""){
				dir = inFile.substr(0, found);
			}
		}


		filename.erase(filename.end() - 4, filename.end());
		m_vstrFilenames.push_back(filename);

		// [wrm] if the user didn't set the outputpath, then create one in the directory of the first raw  
		if (m_strOutputPath == ""){
			m_strOutputPath = dir + "\\search_task_" + strtime + cSlash;
			newDirs.push_back(m_strOutputPath);
		}
		vector<string> tmpQry;
		if (m_bSeparateFiltering){  // 分开过滤
			string folder = m_strOutputPath + "\\" + filename + cSlash;
			if (folder.length()+filename.length() > 250){
				folder = m_strOutputPath + "\\f" + to_string(i+1) + cSlash;
			}
			if (m_quant.size() > 1){
				char s[10];
				for (int q = 0; q < m_quant.size(); ++q){
					itoa(q + 1, s, 10);
					tmpQry.push_back(folder + filename + ".L" + s + ".qry.csv");
				}
			}
			else{
				tmpQry.push_back(folder + filename + ".qry.csv");
			}
			newDirs.push_back(folder);
		}
		else{  // 合并过滤
			if (m_quant.size() > 1){
				char s[10];
				for (int q = 0; q < m_quant.size(); ++q){
					itoa(q + 1, s, 10);
					tmpQry.push_back(m_strOutputPath + filename + ".L" + s + ".qry.csv");
				}
			}
			else{
				tmpQry.push_back(m_strOutputPath + filename + ".qry.csv");
			}
		}
		m_vstrQryResFile.push_back(tmpQry);

		//filename = outpath + "search_task_" + strtime + ".plabel"; //outpath + filename + "_" + strtime + ".plabel";
		//printf("%s\n", filename.c_str());
		//printf("errno is: %d\n",errno);
	}
	for (int i = 0; i < newDirs.size(); ++i){
		#if defined(_WIN32)
			_mkdir(newDirs[i].c_str());
		#else 
			mkdir(newDirs[i].c_str(), 0777);
		#endif
	}

	try
	{
		PrintParas(-1, m_strOutputPath + "\\search_task_" + strtime + ".cfg");
	}
	catch(exception & e) 
	{
		CErrInfo info("CConfiguration", "_CheckPath", "Failed to output .cfg file!");
		throw runtime_error(info.Get(e).c_str());
	}
	catch(...) 
	{
		CErrInfo info("CConfiguration", "_CheckPath", "caught an unknown exception from PrintParas().");
		throw runtime_error(info.Get().c_str());
	}

}

void CConfiguration::PrintParas(int idx, string &outpath)
{
	FILE *fp = fopen(outpath.c_str(), "w");
	if(NULL == fp)
	{
		cout << "[Warning] Failed to create param file" << outpath << endl;
	} else {
		fprintf(fp, "%s\n", VERSION.c_str());
		fprintf(fp, "# This is a pTop configure file\n");
        if(idx == -1){
			int filenum = m_vSpecFiles.size();
			fprintf(fp, "\n[spectrum]\nmsmsnum=%d\n",filenum);
			for(int i=0; i<filenum; ++i){
				fprintf(fp, "msmspath%d=%s\n", i+1,m_vSpecFiles[i].c_str());
			}
		}else{
			fprintf(fp, "\n[spectrum]\nmsmsnum=1\n");
			fprintf(fp, "msmspath1=%s\n", m_vSpecFiles[idx].c_str());
		}
		//fprintf(fp, "querypath1=%s\n", m_vstrQryResFile[idx].c_str());
		//fprintf(fp, "filterpath1=%s\n", m_vstrFilterResFile[idx].c_str());
		//fprintf(fp, "summarypath1=%s\n\n", m_vstrSummary[idx].c_str());

		fprintf(fp, "\ninput_format=%s\n", m_strFileType.c_str());
		fprintf(fp, "Activation=%s\n", m_strActivation.c_str());
		fprintf(fp, "Precursor_Tolerance=%.2f\n", m_lfPrecurTol);
		fprintf(fp, "Fragment_Tolerance=%.2f\n", m_lfFragTol);

		fprintf(fp, "\n[param]\n");
		fprintf(fp, "thread_num=%d\n", m_nThreadNum);
		fprintf(fp, "output_top_k=%d\n", m_nOutputTopK);
		fprintf(fp, "max_truncated_mass=%f\n", m_lfMaxTruncatedMass);
		fprintf(fp, "second_search=%d\n", m_bSecondSearch ? 1 : 0);

		fprintf(fp, "\n[fixmodify]\nfixedModify_num=%d\n", m_vFixMod.size());
		for(size_t i = 0; i < m_vFixMod.size(); ++i)
		{
			fprintf(fp, "fix_mod%d=%s\n", i+1, m_vFixMod[i].c_str());
		}

		fprintf(fp, "\n[modify]\nunexpected_mod=%d\n", m_nUnexpectedModNum);
		fprintf(fp, "Max_mod_num=%d\nMax_mod_mass=%f\nModify_num=%d\n", m_nMaxModNum, m_lfMaxModMass, m_vVarMod.size());
		for(size_t i = 0; i < m_vVarMod.size(); ++i)
		{
			fprintf(fp, "var_mod%d=%s\n", i+1, m_vVarMod[i].c_str());
		}
		fprintf(fp, "\n[filter]\nthreshold=%f\n", m_lfThreshold);
		fprintf(fp, "separate_filtering=%d\n", m_bSeparateFiltering ? 1 : 0);
		fprintf(fp, "\n[file]\nDatabase=%s\n", m_strDBFile.c_str());
		fprintf(fp, "outputpath=%s\n", m_strOutputPath.c_str());

        if(m_quant.size()>0){
			char buf[2000];
			int len = 0;
			for(int i=0; i<m_quant.size(); ++i){
				if(m_quant[i].labelM_num == 0 && m_quant[i].labelR_num == 0){
					len += sprintf(buf+len,"%s","|none");
				}else{
					len += sprintf(buf+len,"%c",'|');
					for(auto rit = m_quant[i].quant_R.begin(); rit != m_quant[i].quant_R.end(); ++rit){
						for (auto at = rit->second.begin(); at != rit->second.end(); ++at){
							len += sprintf(buf + len, "R:%c{%s,%s}", rit->first, at->first.c_str(), at->second.c_str());
						}
					}
					for(auto mit = m_quant[i].quant_M.begin(); mit != m_quant[i].quant_M.end(); ++mit){
						for (auto at = mit->second.begin(); at != mit->second.end(); ++at){
							len += sprintf(buf + len, "M:%s{%s,%s}", mit->first.c_str(), at->first.c_str(), at->second.c_str());
						}
					}
				}
			}
			buf[len] = '\0';
			fprintf(fp,"\n[quantify]\nquant=%d%s\n",m_quant.size(),buf);
		}
		fclose(fp);
	}
}

void CConfiguration::Message(const string & msg, LOG_LEVEL lt)
{
	if (lt == LL_INFORMATION){
		cout << "[pTop] " << msg << flush;
	}
	else if (lt == LL_DEBUG){
		cout << "[pTop] " << "DEBUG: " << msg << flush;
	}
	else if (lt == LL_WARN){
		cout << "[pTop] " << "WARNING: " << msg << flush;
	}
	else if (lt == LL_ERROR){
#ifdef WIN32
		HANDLE console = GetStdHandle(STD_OUTPUT_HANDLE);
		SetConsoleTextAttribute(console, FOREGROUND_RED | FOREGROUND_INTENSITY);
		cout << "[pTop] " << "Error: " << msg << endl;
		SetConsoleTextAttribute(console, FOREGROUND_RED | FOREGROUND_GREEN | FOREGROUND_BLUE);
#else
		cout << "[pTop] " << "Error: " << msg << endl;
#endif
	}
}
void CConfiguration::MergeFiles(const vector<string> &vSrc, const string &strDst, const string &heads, bool bDelete)
{
	FILE *fout = fopen(strDst.c_str(), "wb");  // create the file
	if (fout == NULL){
		Message(string("Failed to create the result file: ") + strDst, LL_ERROR);
		cout << errno << endl;
		exit(1);
	}
	fclose(fout);
	fout = fopen(strDst.c_str(), "ab+");
	if (fout == NULL){
		Message(string("Failed to write the result file: ") + strDst, LL_ERROR);
		exit(1);
	}
	if (heads != ""){
		fwrite(heads.c_str(), 1, heads.size(), fout);
	}
	for (int i = 0; i < vSrc.size(); ++i){
		FILE * fin = fopen(vSrc[i].c_str(), "rb");
		if (fin == NULL){
			Message(string("Failed to open the temporary result file: ") + vSrc[i], LL_ERROR);
			exit(1);
		}
		// linux way
		struct stat st;
		int res = stat(vSrc[i].c_str(), &st);
		long fsize = st.st_size;  // bytes of the file

		char *buf = new char[fsize + 1];
		memset(buf, 0x0, fsize + 1);
		fread(buf, 1, fsize, fin);
		fclose(fin);

		fwrite(buf, 1, fsize, fout);

		delete[] buf;

		if (bDelete){
			remove(vSrc[i].c_str());
			//cout << "remove: " << errno << endl;
		}
	}

	fclose(fout);
}

void CConfiguration::writeConfigFile()
{
	LoadElement();    // read element.ini
	ReadModInfo();   // read modification.ini
	
	for (int i = 0; i < m_quant.size(); ++i){
		_GenTmpAA(i);
		_GenTmpMod(i);
	}
}

void CConfiguration::LoadElement()
{
	char cwd[1024] = { 0 };  // current work directory
	getcwd(cwd, 1024);
	string element_path = string(cwd) + cSlash + "element.ini";
	ifstream if_in(element_path.c_str());
	string strline;
	int num = 0;
	char eName[20];
	while (1){
		getline(if_in, strline);
		if (!if_in)   break;
		if (strline.find("E") != 0){  // must be started with E, e.g. E1=X|1,2,|0.9,0.1,|
			continue;
		}
		string strValue = strline.substr(strline.find('=') + 1);
		string delim = "|";
		vector<string> parts;
		CStringProcess::Split(strValue, delim, parts); // H|1.0078246,2.0141021,|0.99985,0.00015,|
		if (parts.size() < 3)
		{
			cout << "[Warning] Invalid element: " << strline << endl;
			continue;
		}
		string strName = parts[0];
		string masses = parts[1];
		string intens = parts[2];
		//cout << strName << " " << masses << " " << intens << endl;
		vector<FRAGMENT_ION> vCompod;
		delim = ",";

		parts.clear();
		CStringProcess::Split(masses, delim, parts); // 1.0078246,2.0141021,
		for (size_t j = 0; j < parts.size(); ++j)
		{
			//cout << parts[j] << " ";
			if (parts[j].size() <= 0)
			{
				continue;
			}
			double mass = atof(parts[j].c_str());
			FRAGMENT_ION oneElem(mass, 0.0);
			vCompod.push_back(oneElem);
		}
		//cout << endl;

		parts.clear();
		CStringProcess::Split(intens, delim, parts); // 0.99985,0.00015,
		double maxIntens = 0.0;
		int k = 0;
		for (size_t j = 0; j < parts.size(); ++j)
		{
			//cout << parts[j] << " ";
			if (parts[j].size() <= 0)
			{
				continue;
			}
			if (j >= vCompod.size())
			{
				cout << "[Warning] Invalid element: " << strline << endl;
				continue;
			}
			double inten = atof(parts[j].c_str());
			if (inten > maxIntens){
				maxIntens = inten;
				k = j;
			}
			vCompod[j].lfIntens = inten;
		}
		//cout << endl;
		m_ElemMass.insert(pair<string, double>(strName, vCompod[k].lfMz));
		//mapElem.insert(pair<string, vector<FRAGMENT_ION> >(strName, vCompod));
		++num;
	}
	if (0 == num)
	{
		cout << "[Error] There is no element, please check element.ini!" << endl;
		exit(0);
	}
}

void CConfiguration::_GenTmpAA(const int labelID)
{
	string aa_path = "aa.ini";
	ifstream if_in(aa_path.c_str());
	if (access(aa_path.c_str(), 0) != 0)
	{
		cout << "Invalid aa file: " << aa_path << endl;
		exit(1);
	}

	char *aapath = new char[READLEN];
	int len = 0;
	len = sprintf(aapath, "%s\\%d.aa", m_strOutputPath.c_str(), labelID + 1);
	aapath[len] = '\0';
	FILE * faa = fopen(aapath, "w");
	if (faa == NULL){
		cout << "[Alert] Failed to create " << labelID+1 << ".aa file." << endl;
		exit(1);
	}
	fprintf(faa, "label_name=%s\n", m_quant[labelID].label_name.c_str());
	int k = 0;
	string strline;
	while (!if_in.eof()){
		getline(if_in, strline);
		size_t tPos = strline.find('=');
		if (tPos != string::npos){
			string strKey = strline.substr(0, tPos);
			string strVal = strline.substr(tPos + 1);
			if (strKey == "@NUMBER_RESIDUE"){  //
				int num = atoi(strVal.c_str());
				fprintf(faa, "@NUMBER_RESIDUE=%d\n", num);
				continue;
			}
			string delim = "|";
			vector<string> parts;
			CStringProcess::Split(strVal, delim, parts); // A|C(3)H(5)N(1)O(1)S(0)|
			if (parts.size() < 2)
			{
				cout << "[Warning] Invalid residue: " << strline << endl;
				continue;
			}
			//cout << parts[0] << " " << parts[1] << endl;
			char aa = parts[0][0];
			if (aa < 'A' || aa > 'Z')
			{
				cout << "[Warning] Invalid residue: " << strline << endl;
				continue;
			}
			double lfMass = 0.0;
			string strComp = parts[1];
			parts.clear();
			delim = ")";
			CStringProcess::Split(strComp, delim, parts); // C(3)H(5)N(1)O(1)S(0)
			vector<pair<string, int>> aa_comp;
			for (size_t j = 0; j < parts.size(); ++j)
			{
				if (parts[j].size() <= 0)
				{
					continue;
				}
				vector<string> one;
				delim = "(";
				CStringProcess::Split(parts[j], delim, one);
				if (one.size() != 2)
				{
					continue;
				}
				auto it = m_ElemMass.find(one[0]);
				int eleNum = atoi(one[1].c_str());
				if (it != m_ElemMass.end())
				{
					double monoMass = it->second;
					lfMass += monoMass * eleNum;
				}
				aa_comp.push_back(make_pair(one[0], eleNum));
			}
			//cout << aa << " " << lfMass << endl;
			// pair<unordered_multimap<char, LABEL_ELEMENT>::iterator, unordered_multimap<char, LABEL_ELEMENT>::iterator>
			if (m_quant[labelID].quant_R.find(aa) == m_quant[labelID].quant_R.end()){
				fprintf(faa, "R%d=%s\n", ++ k, strVal.c_str());
			}
			else{
				unordered_map<string, string> &quantR = m_quant[labelID].quant_R[aa];
				int eleNum = 0;
				for (auto comp_it = aa_comp.begin(); comp_it != aa_comp.end(); ++comp_it){
					if (quantR.find(comp_it->first) != quantR.end()){
						eleNum = comp_it->second;
						comp_it->first = quantR[comp_it->first];
						lfMass += eleNum*(m_ElemMass[quantR[comp_it->first]] - m_ElemMass[comp_it->first]);
					}
				}
				fprintf(faa, "R%d=%c|", ++ k, aa);
				for (auto comp_it = aa_comp.begin(); comp_it != aa_comp.end(); ++ comp_it){
					fprintf(faa, "%s(%d)", comp_it->first.c_str(), comp_it->second);
				}
				fprintf(faa, "|\n");
			}
		}
	}
	if (0 == k)
	{
		cout << "[Error] There is no residue, please check aa.ini!" << endl;
		exit(1);
	}

	delete[]aapath;
	fclose(faa);
	if_in.close();
}

void CConfiguration::ReadModInfo()
{
	string str_mod_path = "modification.ini";
	if (access(str_mod_path.c_str(), 0) != 0)
	{
		cout << ("Invalid modification file: ") + str_mod_path << endl;
		exit(1);
	}

	ifstream if_in(str_mod_path.c_str());
	string strline;
	unordered_map<string, int> modNames;
	vector<MODINI> vUnimod;
	// read mod
	while (1){
		getline(if_in, strline);
		if (!if_in)  break;
		size_t tPos = strline.find('=');
		if (tPos != string::npos){
			string strKey = strline.substr(0, tPos);
			string strVal = strline.substr(tPos + 1);
			if (strKey.find("name") != string::npos){  //  name1=2-dimethylsuccinyl[C] 1
				continue;
			}
			if (strKey == "@NUMBER_MODIFICATION"){
				continue;
			}
			MODINI tmpMod;
			tmpMod.strName = strKey;
			string comp = getModInfo(strVal, tmpMod, -1);
			vUnimod.push_back(tmpMod);
			modNames.insert(make_pair(strKey, (int)vUnimod.size() - 1));
		}

	}
	if_in.close();

	// get information of fix modification 
	for (size_t i = 0; i < m_vFixMod.size(); ++i)
	{
		if (modNames.find(m_vFixMod[i]) != modNames.end())
		{
			m_vFixModInfo.push_back(vUnimod[modNames[m_vFixMod[i]]]);
		}
		else {
			Message("Can not find the modification " + m_vFixMod[i] + ", please check the modification.ini!", LL_WARN);
		}
	}

	// get information of variable modification
	for (size_t i = 0; i < m_vVarMod.size(); ++i)
	{
		if (modNames.find(m_vVarMod[i]) != modNames.end())
		{
			// TODO: get the nutral loss
			//cout<<modName<<" "<<tmpMod.strSite<<" "<<tmpMod.cType<<" "<<tmpMod.lfMass<<endl;
			m_vVarModInfo.push_back(vUnimod[modNames[m_vVarMod[i]]]);
		}
		else {
			Message("Can not find the modification \"" + m_vVarMod[i] + "\", please check the modification.ini!", LL_WARN);
		}
	}
}

string CConfiguration::getModInfo(const string strVal, MODINI &tmpMod, int passNumber)
{
	vector<string> parts;
	CStringProcess::Split(strVal, " ", parts);
	tmpMod.strSite = parts[0];
	if (parts[1].compare("NORMAL") == 0)
		tmpMod.cType = MOD_TYPE::MOD_NORMAL;
	else if (parts[1].compare("PRO_N") == 0)
		tmpMod.cType = MOD_TYPE::MOD_PRO_N;
	else if (parts[1].compare("PRO_C") == 0)
		tmpMod.cType = MOD_TYPE::MOD_PRO_C;
	else {
		cout << "[Error] Invalid modification type: " << parts[1] << endl;
		exit(-1);
	}
	tmpMod.lfMass = atof(parts[2].c_str());
	// [wrm] neutral loss info added by wrm. 2016.02.29
	int neutral = atoi(parts[4].c_str());
	for (int i = 0; i<neutral; ++i){
		tmpMod.vNeutralLoss.push_back(atof(parts[5 + 2 * i].c_str()));
	}

	if (passNumber < 0)  
		return "";

	vector<pair<string, int>> ret;
	string tmpComp = parts.back();
	parts.clear();
	CStringProcess::Split(tmpComp, ")", parts);
	unordered_map<string, int> modComp;
	for (int i = 0; i<parts.size(); ++i){
		if (parts[i].size() <= 0){
			continue;
		}
		vector<string> one;
		CStringProcess::Split(parts[i], "(", one);
		if (one.size() != 2){
			continue;
		}
		int eleNum = atoi(one[1].c_str());
		modComp.insert(pair<string, int>(one[0], eleNum));
		ret.push_back(pair<string, int>(one[0], eleNum));
	}
	//tmpMod.ModComposition = modComp;
	try{
		if (m_quant[passNumber].quant_M.find(tmpMod.strName) != m_quant[passNumber].quant_M.end()){
			unordered_map<string, string> &quant_M = m_quant[passNumber].quant_M[tmpMod.strName];
			unordered_map<string, string> labels;
			for (auto it = quant_M.begin(); it != quant_M.end(); ++it){
				if (modComp.find(it->first) != modComp.end()){ // the mod contains this element
					tmpMod.lfMass += modComp[it->first] * (m_ElemMass[it->second] - m_ElemMass[it->first]);
					labels.insert(make_pair(it->first, it->second));
				}
			}
			for (int i = 0; i<ret.size(); ++i){
				if (labels.find(ret[i].first) != labels.end()){
					ret[i].first = labels[ret[i].first];
				}
			}
		}
	}
	catch (exception &e){
		CErrInfo info("CPrePTMForm", "getModInfobyName", "Get modification information failed.");
		throw runtime_error(info.Get(e).c_str());
	}
	string compStr = "";
	//char s[10];
	for (int i = 0; i<ret.size(); ++i){
		compStr += ret[i].first;
		compStr.push_back('(');
		compStr += to_string(ret[i].second);
		//itoa(ret[i].second, s, 10);
		//compStr.append(s);
		compStr.push_back(')');
	}
	return compStr;
}


void CConfiguration::_GenTmpMod(const int labelID)
{
	string str_mod_path = "modification.ini";
	if (access(str_mod_path.c_str(), 0) != 0)
	{
		cout << ("Invalid modification file: ") + str_mod_path << endl;
		exit(1);
	}

	ifstream if_in(str_mod_path.c_str());
	string strline;
	unordered_map<string, int> modNames;
	vector<string> modsCompStr;
	vector<MODINI> vUnimod;
	// read mod
	while (1){
		getline(if_in, strline);
		if (!if_in)  break;
		size_t tPos = strline.find('=');
		if (tPos != string::npos){
			string strKey = strline.substr(0, tPos);
			string strVal = strline.substr(tPos + 1);
			if (strKey.find("name") != string::npos){  //  name1=2-dimethylsuccinyl[C] 1
				continue;
			}
			if (strKey == "@NUMBER_MODIFICATION"){
				continue;
			}
			MODINI tmpMod;
			tmpMod.strName = strKey;
			string comp = getModInfo(strVal, tmpMod, labelID);
			vUnimod.push_back(tmpMod);
			modsCompStr.push_back(comp);
			modNames.insert(make_pair(strKey, (int)vUnimod.size() - 1));
		}

	}
	if_in.close();
	// write .mod
	int totalNum = vUnimod.size();
	char *modpath = new char[2 * READLEN];
	int len = 0;
	len = sprintf(modpath, "%s\\%d.mod", m_strOutputPath.c_str(), labelID + 1);
	modpath[len] = '\0';
	FILE *fmod = fopen(modpath, "wb");
	if (fmod == NULL){
		cout << "[Alert] Failed to create .mod file." << endl;
	}
	char *buf = new char[500000];
	len = 0;
	len += sprintf(buf + len, "label_name=%s\n", m_quant[labelID].label_name.c_str());
	len += sprintf(buf + len, "@NUMBER_MODIFICATION=%d\n", totalNum);

	for (int modId = 0; modId < totalNum; ++modId)
	{
		string ctype = "NORMAL";
		switch (vUnimod[modId].cType)
		{
		case MOD_TYPE::MOD_NORMAL:
			ctype = "NORMAL";
			break;
		case MOD_TYPE::MOD_PRO_N:
			ctype = "PRO_N";
			break;
		case MOD_TYPE::MOD_PRO_C:
			ctype = "PRO_C";
			break;
		default:
			break;
		}
		len += sprintf(buf + len, "name%d=%s#%d 1\n", modId + 1, vUnimod[modId].strName.c_str(), labelID + 1);
		len += sprintf(buf + len, "%s#%d=%s %s %lf %lf %d ", vUnimod[modId].strName.c_str(), labelID + 1, vUnimod[modId].strSite.c_str(), ctype.c_str(), 
			vUnimod[modId].lfMass, vUnimod[modId].lfMass, vUnimod[modId].vNeutralLoss.size());
		for (int i = 0; i < vUnimod[modId].vNeutralLoss.size(); ++i){
			len += sprintf(buf + len, "%lf %lf ", vUnimod[modId].vNeutralLoss[i], vUnimod[modId].vNeutralLoss[i]);
		}
		len += sprintf(buf + len, "%s\n", modsCompStr[modId].c_str());
	}
	fwrite(buf, sizeof(char), len, fmod);
	delete[]modpath;
	delete[]buf;
	fclose(fmod);
}

void CConfiguration::GetElementMass(unordered_map<string, double> &elemMass)
{
	elemMass = m_ElemMass;
}

// FileID,Title,Scan,Charge,Precusor Mass,Theoretical Mass,Mass Diff Da,Mass Diff ppm,Score,Evalue,Number of Matched Peaks,Protein AC,Protein Sequence,PTMs,
//isDecoy,Label Type,Nterm Matched Ions,Cterm Matched Ions,Nterm Matched Intensity Ratio,Cterm Matched Intensity Ratio
void CConfiguration::parseFileTitle(const string &title, const string delim, PRSM_Column_Index &columns)
{
	vector<string> tmp;
	CStringProcess::Split(title, delim, tmp);
	for (int i = 0; i<tmp.size(); ++i){
		if (tmp[i] == "FileID"){
			columns.fileID_col = i;
		}
		else if (tmp[i] == "Title"){
			columns.title_col = i;
		}
		else if (tmp[i] == "Scan"){
			columns.scan_col = i;
		}
		else if (tmp[i] == "Charge"){
			columns.charge_col = i;
		}
		else if (tmp[i] == "Precursor Mass"){
			columns.precusor_mass_col = i;
		}
		else if (tmp[i] == "Theoretical Mass"){
			columns.theoretical_mass_col = i;
		}
		else if (tmp[i] == "Raw Score"){
			columns.score_col = i;
		}
		else if (tmp[i] == "Final Score"){  // E-value, because now it's svm score
			columns.evalue_col = i;
		}
		else if (tmp[i] == "Matched Peaks"){
			columns.matched_peaks_col = i;
		}
		else if (tmp[i] == "Protein AC"){
			columns.protein_ac_col = i;
		}
		else if (tmp[i] == "Protein Sequence"){
			columns.protein_sequence_col = i;
		}
		else if (tmp[i] == "PTMs"){
			columns.ptms_col = i;
		}
		else if (tmp[i] == "isDecoy"){
			columns.isDecoy_col = i;
		}
		else if (tmp[i] == "Label Type"){
			columns.label_col = i;
		}
		else if (tmp[i] == "Nterm Matched Ions"){
			columns.nmatched_ions_col = i;
		}
		else if (tmp[i] == "Cterm Matched Ions"){
			columns.cmatched_ions_col = i;
		}
		else if (tmp[i] == "Nterm Matched Intensity Ratio"){
			columns.nmatched_intensity_col = i;
		}
		else if (tmp[i] == "Cterm Matched Intensity Ratio"){
			columns.cmatched_intensity_col = i;
		}
		else if (tmp[i] == "Q-value"){
			columns.qvalue_col = i;
		}
		else if (tmp[i] == "Com_Ion_Ratio"){
			columns.com_ion_col = i;
		}
		else if (tmp[i] == "Tag_Ratio"){
			columns.tag_ratio_col = i;
		}
		else if (tmp[i] == "PTM_Score"){  //PTM_Score
			columns.ptm_score_col = i;
		}
		else if (tmp[i] == "Fragment_Error_STD"){
			columns.fragment_error_col = i;
		}
	}
}
