#include "quantify.h"

using namespace std;


Quantitation::Quantitation(CConfiguration *m_cPara) :m_cPara(m_cPara)
{
	m_pTrace = Clog::getInstance();
}

Quantitation::~Quantitation()
{
	if (m_cPara){
		m_cPara = NULL;
	}
	if (m_pTrace){
		m_pTrace = NULL;
	}
}

// 15N_Labeling=R:*{N,15N}M:Deamidated[N]{N,15N}M:Gln->pyro-Glu[AnyN-termQ]{N,15N}
void Quantitation::getLabelbyName(string name, LABEL_QUANT &label_info)
{
	try{
		COptionTool config("quant.ini");
		string labelInfo = config.GetString("",name.c_str(),"");
		size_t last = 0;
		size_t index = labelInfo.find(":", last);
		label_info.labelM_num = label_info.labelR_num = 0;
		if(index != string::npos){ //
			unordered_map<char,unordered_map<string, string>> quant_R;
			unordered_map<string,unordered_map<string, string>> quant_M;
			unordered_map<string, string> lb;
			while(index != string::npos && index > 0){
				char ctype = labelInfo[index-1];
				size_t l_idx = labelInfo.find("{", last);
				size_t r_idx = labelInfo.find("}", last);
				string eName = labelInfo.substr(index+1,l_idx-index-1);
				string elements = labelInfo.substr(l_idx+1,r_idx-l_idx-1);
				size_t comma = elements.find(",");
				string preElement = elements.substr(0,comma);
				string labelElement = elements.substr(comma+1);
				if(ctype=='R'){
					if(eName[0]>='A' && eName[0]<='Z'){
						if (quant_R.find(eName[0]) == quant_R.end()){
							quant_R.insert(make_pair(eName[0], unordered_map<string, string>()));
						}
						quant_R[eName[0]].insert(make_pair(preElement,labelElement));
						label_info.labelR_num ++;
					}else if(name[0]=='*'){
						for(int l=0; l<26; ++l){
							if (quant_R.find(l + 'A') == quant_R.end()){
								quant_R.insert(make_pair(l+'A', unordered_map<string, string>()));
							}
							quant_R[l+'A'].insert(make_pair(preElement,labelElement));
							label_info.labelR_num ++;
						}
					}else{
						throw("Invalid label!");
					}
				}else if(ctype=='M'){
					if (quant_M.find(eName) == quant_M.end()){
						quant_M.insert(make_pair(eName, unordered_map<string, string>()));
					}
					quant_M[eName].insert(make_pair(preElement,labelElement));
					label_info.labelM_num ++;
				}
				last = r_idx + 1;
				index = labelInfo.find(":", last);
			}
			label_info.quant_R = quant_R;
			label_info.quant_M = quant_M;
			label_info.label_name = name;
		}
	}catch(exception &e){
		CErrInfo info("Quantitation", "getLabelbyName()", "Get label information failed.");
		throw runtime_error(info.Get(e).c_str());
	}catch(...){
	    CErrInfo info("Quantitation", "getLabelbyName()", "Caught an unknown exception from getting label.");
		throw runtime_error(info.Get().c_str());
	}
}

void Quantitation::quantify()
{
	try{
		//if (m_cPara->m_bSeparateFiltering){
		//	for (int i = 0; i < m_cPara->m_vSpecFiles.size(); ++i){
		//		int pos = m_cPara->m_vstrQryResFile[i][0].find_last_of(cSlash);
		//		if (pos == string::npos){
		//			pos = 0;
		//		}
		//		string cfgPath = m_cPara->m_vstrQryResFile[i][0].substr(0, pos) + "\\pQuant.cfg";
		//		rewriteQuantCfg(cfgPath, i);
		//		callpQuant(cfgPath);
		//	}
		//}
		//else{
			string cfgPath = m_cPara->m_strOutputPath + "\\pQuant.cfg";
			rewriteQuantCfg(cfgPath, -1);
			callpQuant(cfgPath);
		//}
		
	}
	catch (exception & e) {
		CErrInfo info("Quantitation", "quantify", "quantify failed.");
		throw runtime_error(info.Get(e).c_str());
	}
	catch (...) {
		CErrInfo info("Quantitation", "quantify", "caught an unknown exception from quantifying.");
		throw runtime_error(info.Get().c_str());
	}
}

// rewrite pQuant的参数文件：将鉴定结果文件路径写入pQuant.cfg
void Quantitation::rewriteQuantCfg(const string &cfgPath, int fileIdx)
{
	try{
		// read
		FILE * fp = fopen(m_cPara->m_strpQuant.c_str(),"r");
		if(fp == NULL){
			m_pTrace->error("Failed to open pQuant configuration file" + m_cPara->m_strpQuant);
			exit(1);
		}
		
		string pathms1 = "PATH_MS1";
		string extenms1 = "EXTENSION_TEXT_MS1";
		string pathId = "PATH_IDENTIFICATION_FILE";
		string outpath = "DIR_EXPORT";
			
		string ms1Type = "pf1"; //一级谱文件格式
		string ms1_files = "";
		string id_files = "";
		string out_dir = "";
		if (fileIdx == -1){ // merge
			for (int i = 0; i < m_cPara->m_vSpecFiles.size(); ++i){
				string infile = m_cPara->m_vSpecFiles[i];
				ms1_files.append(getMS1bySpecFile(infile, ms1Type));
			}
			id_files = m_cPara->m_strOutputPath + "\\pTop_filtered.spectra|";
			out_dir = m_cPara->m_strOutputPath;
		}
		else{ // separate
			string infile = m_cPara->m_vSpecFiles[fileIdx];
			ms1_files.append(getMS1bySpecFile(infile, ms1Type));
			int pos = m_cPara->m_vstrQryResFile[fileIdx][0].find_last_of(cSlash);
			if (pos == string::npos){
				pos = 0;
			}
			id_files = m_cPara->m_vstrQryResFile[fileIdx][0].substr(0,pos) + "\\" + m_cPara->m_vstrFilenames[fileIdx] + ".spectra|";
			out_dir = m_cPara->m_vstrQryResFile[fileIdx][0].substr(0, pos) + "\\";
		}

		char *cfgBuf = new char[BUFFERSIZE];  // [wrm] assume that the buffer size is enough for pQuant configuration
		int len = 0;
		const int LINE_LEN = 1024;
		char line[LINE_LEN];
		int equalIdx, semiIdx;
		while(!feof(fp))
		{
			if(NULL == fgets(line, LINE_LEN - 1, fp)) // NULL，文件结束 ; including the line break('\n')
			{
				break;
			}
			if(line[0] == '#' || line[0] == '['){
				len += sprintf(cfgBuf+len,"%s",line);
			}else{
				string tmpline = line;
				if(tmpline.find('=') != string::npos){
					equalIdx = tmpline.find("=");
					semiIdx = tmpline.find(";");
					string key = tmpline.substr(0,equalIdx);
					string val = tmpline.substr(equalIdx+1,semiIdx-equalIdx-1);
					if(key.compare(pathms1)==0){
						val = ms1_files;
					}else if(key.compare(extenms1)==0){
						val = ms1Type;
					}else if(key.compare(pathId)==0){
						val = id_files;
					}else if(key.compare(outpath)==0){
						val = out_dir;
					}
					len += sprintf(cfgBuf+len,"%s=%s;\n",key.c_str(),val.c_str());
				}else{
					len += sprintf(cfgBuf+len,"%s",line);
				}
	
			}	
		}
		fclose(fp);
		// write
		FILE *fout = fopen(cfgPath.c_str(),"wb");
		if (fout == NULL){
			m_pTrace->error("Failed to rewrite pQuant configuration file" + cfgPath);
			exit(1);
		}
		fwrite(cfgBuf, sizeof(char), len, fout);
		fclose(fout);
		delete []cfgBuf;
	}
	catch (exception & e) {
		CErrInfo info("Quantitation", "rewriteQuantCfg", "rewrite pQuant.cfg failed.");
		throw runtime_error(info.Get(e).c_str());
	}
	catch (...) {
		CErrInfo info("Quantitation", "rewriteQuantCfg", "caught an unknown exception from rewriting pQuant.cfg.");
		throw runtime_error(info.Get().c_str());
	}
}

string Quantitation::getMS1bySpecFile(const string &infile, string &ms1Type)
{
	string ms1file = "";
	size_t pos = infile.find_last_of("_");   // _HCDFT.pf2
	if (pos != string::npos){
		ms1file = infile.substr(0, pos);	
	}
	else{
		pos = infile.find_last_of(".");
		if (pos != string::npos){
			ms1file = infile.substr(0, pos);			
		}
	}
	if (ms1file == "")  return "";
	// [wrm?]如何确定是用pf1还是ms1？
	if (m_cPara->m_inFileType == MS2FormatType::MFT_RAW || m_cPara->m_inFileType == MS2FormatType::MFT_PF){
		ms1file.append(".pf1|");
		ms1Type = "pf1";
	}
	else{  // ms1
		ms1file.append(".ms1|");
		ms1Type = "ms1";
	}
	return ms1file;
}

void Quantitation::callpQuant(const string &cfgPath)
{
	try{
		FILE *fp = fopen(cfgPath.c_str(), "r");
		if (NULL == fp){
			cout << "[Error] Cannot open the config file of pQuant: " << cfgPath << endl;
			return;
		}
		else {
			fclose(fp);
		}
		string mscmdline = "pQuant.exe \"" + cfgPath + "\""; //配置文件路径
		m_pTrace->info("[Call pQuant] " + mscmdline);
		system(mscmdline.c_str());
	}
	catch (exception & e) {
		CErrInfo info("Quantitation", "callpQuant", "call pQuant failed.");
		throw runtime_error(info.Get(e).c_str());
	}
	catch (...) {
		CErrInfo info("Quantitation", "callpQuant", "caught an unknown exception from calling pQuant.");
		throw runtime_error(info.Get().c_str());
	}
}