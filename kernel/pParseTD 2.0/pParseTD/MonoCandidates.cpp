#include <fstream>
#include <iostream>
#include <cstdio>
#include <conio.h>
#include <io.h>
#include <cstdio>
#include <ctime>
#include <vector>
#include <stdexcept>

#include "dirent.h"
#include "MonoCandidates.h"
#include "MS1Reader.h"
#include "BasicFunction.h"
#include "MS2Reader.h"
#include "TrainingFileReader.h"
#include "com.h"
#include "pParseDebug.h"
#include "StringProcessor.h"
#include "SpecDataIO.h"

using namespace std;

/*
**	class CMainFlow
*/
CMainFlow::CMainFlow()
{
	m_vIPV.resize(LIPV_MASSMAX+10);
	m_cEmass = new CEmass(m_vIPV);
	m_para = new CParaProcess();
}
CMainFlow::~CMainFlow()
{
	
	delete m_cEmass;
	delete m_para;
}

//处理每一个MS1文件
void CMainFlow::ProcessEachMS1(string filename)
{
	cout << "[pParseTD] Processing " << filename << endl;
	string ms2filename = filename.substr(0, filename.length() - 1);
	ms2filename.push_back('2');

	try
	{   // 检查对应的 ms2是否存在
		FILE *fms2 = fopen(ms2filename.c_str(), "r");
		if(NULL == fms2)
		{
			cout << "[Error] Cannot open MS2 file: " << ms2filename << endl;
			return;
		}
		fclose(fms2);
	}
	catch(exception & e)
	{
		CErrInfo info("CMainFlow", "ProcessEachMS1", "open ms2fiel failed.");
		throw runtime_error(info.Get(e).c_str());
		//throw ("CMainFlow::ProcessEachMS1() Open ms2fiel failed.");
	}
	catch(...)
	{
		CErrInfo info("CMainFlow", "ProcessEachMS1", "Caught an unknown exception from opening ms2fiel.");
		throw runtime_error(info.Get().c_str());
		//throw ("CMainFlow::ProcessEachMS1() Caught an unknown exception from opening ms2fiel.");
	}	

	FlowKernel(filename, ms2filename);

}
//处理每个Raw文件
void CMainFlow::ProcessEachRaw(string filename)
{
	string filePrefix = filename.substr(0, filename.length() - 4);
	string MS1filepath = filePrefix + ".ms1";
	string MS2filepath = filePrefix + ".ms2"; 

	FILE *pMS1 = fopen(MS1filepath.c_str(), "r");
	FILE *pMS2 = fopen(MS2filepath.c_str(), "r");
	
	// 不存在 ms1或 ms2，则调用pXtract，导出ms1和ms2
	if (pMS1 == NULL || pMS2 == NULL)  
	{
		try
		{
			cout << "[pParseTD] " << filename << " Converting to MS1 & MS2..." << endl;
			string mscmdline = "xtract_raw.exe -a -ms -z 2-2 ";
			mscmdline += "-m " + m_para->GetValue("m/z", "5") + " -i " + m_para->GetValue("Intensity", "1");
			mscmdline += " \"" + filename + "\"";

			cout << "[Call pXtract] " << mscmdline << endl;
			int err_no = system(mscmdline.c_str());
			if(err_no){  // pXtract运行异常
				throw err_no;
			}
		}
		catch(...)
		{
			cout << "[Error] xtract_raw.exe doesn't work, please check!" << endl;
			exit(1);
		}
	} else {
		cout << "[pParseTD] " << filename << ": MS1 & MS2 files already exists." << endl;
		fclose(pMS1);
		fclose(pMS2);	
	}
}

//获取指定路径下的所有raw文件
void CMainFlow::GetFilelistbyDirent(string &filepath, string format, list<string> &fileList)
{
	DIR *dir = opendir(filepath.c_str()); //CHECK luolan
	if (dir == NULL) 
	{
		//CErrInfo info("CMainFlow", "GetFilelistbyDirent", "opendir: " + filepath + " failed.");
		//throw runtime_error(info.Get().c_str());
		throw ("CMainFlow::GetFilelistbyDirent() Opendir failed.");
	}
	struct dirent *de;
	while(NULL != (de = readdir(dir)))
	{
		if (de->d_name[0] == '.') 
		{
			continue;
		}
		string FileName = de->d_name;
		
		if (FileName.length() < 5)
		{
			continue;

		} else {
			string fileSuffix = FileName.substr(FileName.length() - 4, 4);
			CStringProcess::ToLower(fileSuffix);
			if(fileSuffix.compare(format) != 0)
			{
				continue;
			}
		}
		fileList.push_back(filepath + de->d_name);
	}
	closedir(dir);
}

//依次处理每个路径指定的文件
void CMainFlow::ProcessFileList()
{
	string logPath = m_para->GetValue("logfilepath");
	string dataPath = m_para->GetValue("datapath");
	string fileFormat = m_para->GetValue("input_format");
	CStringProcess::ToLower(fileFormat);

	size_t found = dataPath.find_last_of("/\\");
	string inputPath = dataPath.substr(0, found + 1);

	if (fileFormat.compare("path") == 0) // 处理指定目录下所有的raw和ms1
	{
		// 获取当前目录下的所有raw文件
		GetFilelistbyDirent(inputPath, ".raw", m_Rawlist);
		if (m_Rawlist.empty()) 
		{
			cout << "[Warning] No Raw files in datapath!" << endl;
		}

		for(list<string>::iterator it = m_Rawlist.begin(); it != m_Rawlist.end(); ++it)
		{
			ProcessEachRaw(*it);
		}

		// 获取当前目录下的所有MS1文件
		GetFilelistbyDirent(inputPath, ".ms1", m_MS1list);
		if (m_MS1list.empty()) 
		{
			cout << "[Error] No MS1 files in datapath: " << dataPath << endl;
			exit(1);
		}
		//cout << "[pParseTD] Start to process each MS1" << endl; // 依次处理每个MS1文件
		for(list<string>::iterator it = m_MS1list.begin(); it != m_MS1list.end(); ++it) 
		{
			ProcessEachMS1(*it);
		}
		
	} else if (fileFormat.compare("raw") == 0) {		// 待处理文件格式为Raw
		/*if(dataPath.length() < 5)
		{
			cout<<"[Error] Invalid input file: " << dataPath <<endl;
			exit(1);
		}
		string fileSuffix = dataPath.substr(dataPath.length() - 4, 4);
		CStringProcess::ToLower(fileSuffix);

		//cout << "[pParseTD] Start to process MS1" << endl;
		if(fileSuffix.compare(".raw") == 0)				
		{*/	
		ProcessEachRaw(dataPath);
		string ms1File = dataPath.substr(0, dataPath.length() - 4);
		ms1File.append(".MS1");

		ProcessEachMS1(ms1File);

	} else if(fileFormat.compare("ms1") == 0){		// 待处理文件格式为ms1

		ProcessEachMS1(dataPath);

	} else {											// 格式参数只有两种path or file， 否则出错，默认为file

		cout<<"[pParseTD] Error: Invalid input_format " << fileFormat << " Please type in 'path' or 'raw' or ms1!" << endl;
		exit(1);
	}
}
//开始整个流程
void CMainFlow::GetStart()
{
	try
	{

		string IPVfilepath = "LIPV.txt";  //m_para->GetValue("ipv_file");
		if (m_para->GetValue("label") == "N15"){
			IPVfilepath = "LIPVN15.txt";
		}
		else if (m_para->GetValue("label") == "C13"){
			IPVfilepath = "LIPVC13.txt";
		}
		GetIPV(IPVfilepath);    // Load the isotopic cluster

		ProcessFileList(); // 开始依次处理每个文件
	}
	catch(exception & e)
	{
		CErrInfo info("CMainFlow", "GetStart", "GetStart failed.");
		throw runtime_error(info.Get(e).c_str());
		//throw ("CMainFlow::GetStart() GetStart failed.");
	}
	catch(...)
	{
		CErrInfo info("CMainFlow", "GetStart", "Caught an unknown exception from GetStart.");
		throw runtime_error(info.Get().c_str());
		//throw ("CMainFlow::GetStart() Caught an unknown exception from GetStart.");
	}
}
// 通过参数文件进行配置
void CMainFlow::RunFromFile(string filename)
{	
	try
	{
		m_para->InitiParaMap();   // 初始化参数列表
		m_para->GetFilePara(filename);  // 解析参数文件
		//m_para->DisplayPara();
	}
	catch(exception & e)
	{
		CErrInfo info("CMainFlow", "RunFromFile", "InitParameters failed.");
		throw runtime_error(info.Get(e).c_str());
		//throw ("CMainFlow::RunFromFile() InitParameters failed.");
	}
	catch(...)
	{
		CErrInfo info("CMainFlow", "RunFromFile", "Caught an unknown exception from InitParameters.");
		throw runtime_error(info.Get().c_str());
		//throw ("CMainFlow::RunFromFile() Caught an unknown exception from InitParameters.");
	}

	int dataNum = atoi(m_para->GetValue("datanum").c_str());
	char number[5];
	for(int idx = 0; idx < dataNum; ++idx)
	{
		string inPath = "datapath";
		sprintf(number, "%d", idx+1);
		inPath.append(number);
		//cout << inPath << endl;
		m_para->SetValue("datapath", m_para->GetValue(inPath));
		m_para->CheckParam();
		GetStart();
	}
}
//通过命令行配置参数
void CMainFlow::RunFromCMD(int argc , char * argv[])
{
	if (argc == 1)
	{
		try
		{
			CHelpInfo cHelp;
			cHelp.DisplayUsage();
		}
		catch(exception & e)
		{
			CErrInfo info("CMainFlow", "RunFromCMD", "DisplayUsage failed.");
			throw runtime_error(info.Get(e).c_str());
			//throw ("CMainFlow::RunFromCMD() DisplayUsage failed.");
		}
		catch(...)
		{
			CErrInfo info("CMainFlow", "RunFromCMD", "Caught an unknown exception from DisplayUsage.");
			throw runtime_error(info.Get().c_str());
			//throw ("CMainFlow::RunFromCMD() Caught an unknown exception from DisplayUsage.");
		}
	} else {
		
		try
		{
			m_para->InitiParaMap();
			m_para->GetCMDOption(argc, argv);
			m_para->DisplayPara();
		}
		catch(exception & e)
		{
			CErrInfo info("CMainFlow", "RunFromCMD", "InitParameters failed.");
			throw runtime_error(info.Get(e).c_str());
			//throw ("CMainFlow::RunFromCMD() InitParameters failed.");
		}
		catch(...)
		{
			CErrInfo info("CMainFlow", "RunFromCMD", "Caught an unknown exception from InitParameters.");
			throw runtime_error(info.Get().c_str());
			//throw ("CMainFlow::RunFromCMD() Caught an unknown exception from InitParameters.");
		}
	
		GetStart();
	}
}

//将IPV矩阵10000 * 38读入内存
//void CMainFlow::GetIPV(string IPVfilepath)
//{
//	FILE *pFile = fopen(IPVfilepath.c_str(), "r");
//	if (pFile == NULL)
//	{
//		CErrInfo info("CMainFlow", "GetIPV", "Open the ipv file failed.");
//		info.Append(IPVfilepath);
//		throw runtime_error(info.Get().c_str());
//		throw ("CMainFlow::GetIPV() Open the ipv file failed.");
//	}
//	int i = 0;
//	int maxSize  = 2 * IPV_PEAKMAX;
//	while (!feof(pFile))
//	{
//		for(int j = 0; j < maxSize; ++j)
//		{
//			fscanf(pFile,"%lf", &m_vIPV[i][j]);  // [wrm]: 换成二进制读写(fread)可以加速
//		}
//		if(++i >= IPV_MASSMAX)
//		{
//			break;
//		}
//	}
//	fclose(pFile);
//}

// IPV信息: 100000 * (15*2)
void CMainFlow::GetIPV(string IPVfilepath)
{
	FILE *pFile = fopen(IPVfilepath.c_str(), "r");
	if (pFile == NULL)
	{
		CErrInfo info("CMainFlow", "GetIPV", "Open the ipv file failed.");
		info.Append(IPVfilepath);
		throw runtime_error(info.Get().c_str());
		throw ("CMainFlow::GetIPV() Open the ipv file failed.");
	}
	int i = 0;
	int maxSize  = 2 * IPV_PEAKMAX;
	int mass, gapnum, gap;
	vector<string> vtmp;
	const int MaxLineLen = 1024;
	char * line = new char[MaxLineLen];
	try{
		while (!feof(pFile))
		{
			fgets(line,MaxLineLen,pFile);
			CStringProcess::Split(line," ",vtmp);
			if(vtmp.size() > 3){
				mass = atoi(vtmp[1].c_str());
				gapnum = atoi(vtmp[2].c_str());;
				for(int g = 0; g < gapnum; ++ g){
					gap = atoi(vtmp[3+g].c_str());
					vector<CentroidPeaks> tmpPeaks;
					fgets(line,MaxLineLen,pFile);
					CStringProcess::Split(line," ",vtmp);					
					if(vtmp.size() >= IPV_PEAKMAX){
						for(int j = 0; j < 2*IPV_PEAKMAX; j += 2){
							double mz = atof(vtmp[j].c_str());
							double intens = atof(vtmp[j+1].c_str());
							tmpPeaks.push_back(CentroidPeaks(mz,intens));
						}
						m_vIPV[mass].insert(make_pair(gap,tmpPeaks));
					}else{
						cout << "Error info in IPV: " << line << endl;
					}
					// 
					/*double mz, intens;
					for(int j=0; j<IPV_PEAKMAX; ++j){
						fscanf(pFile,"%lf %lf",&mz, &intens);
						tmpPeaks.push_back(CentroidPeaks(mz,intens));
					}
					m_vIPV[mass].insert(make_pair(gap,tmpPeaks));*/
				}
				if(++i >= LIPV_MASSMAX)
				{
					break;
				}
			}
		}
	}catch(exception & e)
	{
		CErrInfo info("CMainFlow", "GetIPV", "read IPV file failed.");
		throw runtime_error(info.Get(e).c_str());
		//throw ("CMainFlow::FlowKernel() output .mgf/.pf files failed.");
	}
	catch(...)
	{
		CErrInfo info("CMainFlow", "GetIPV", "Caught an unknown exception from reading ipv file.");
		throw runtime_error(info.Get().c_str());
		//throw ("CMainFlow::FlowKernel() Caught an unknown exception from outputing .mgf/.pf/.csv files.");
	}	
	delete []line;
	fclose(pFile);
}
//根据指定的质量获取一行IPV的值
//int CMainFlow::GetOneIPV(int mass, vector<double> &vInt)
//{
//	int highestPos = -1;
//	double highestIntens = 0;
//	vInt.clear();
//	for (int i = 0; i < 2 * IPV_PEAKMAX; i += 2)
//	{
//		vInt.push_back(m_vIPV[mass][i+1]);
//		if(m_vIPV[mass][i+1] > highestIntens)
//		{
//			highestIntens = m_vIPV[mass][i+1];
//			highestPos = i / 2;
//		}
//	}
//	return highestPos;
//}


/* 
	对每一组ms1和ms2进行处理，根据ms2的碎裂中心寻找合适的母离子，提取特征并评估母离子，然后处理二级谱
*/
void CMainFlow::FlowKernel(string MS1file, string MS2file)
{
	
	// Prepare to output for the CSV file
	// First line of CSVfile: strPrecursorListToCSV, the other data will be appended later
	string strPrecursorsListToCSV = "ActivationType,InstrumentType,MS2Scan,OrigMZ,OrigCharge,TotleNumOfPrecursor,"
								    "PrecursorIdentifier,MZ,Charge,"
								    "PrecursorIdentifier,MZ,Charge,"
								    "PrecursorIdentifier,MZ,Charge,"
								    "PrecursorIdentifier,MZ,Charge,"
								    "PrecursorIdentifier,MZ,Charge\n";

	strPrecursorsListToCSV.reserve(1024*1024*16);                      // 16M buffer will be enough! 

	// Get the MS1 file name, with path in it! 
	int slashfound = MS1file.find_last_of("/\\");
	string FileName = MS1file.substr(slashfound + 1);
	string FilePath = MS1file.substr(0, slashfound + 1);
	string Outputtitle = m_para->GetValue("outputpath") + FileName.substr(0, FileName.size() - 4); // 4:".MS1" 

	string PrecursorslistFilePath_NameCSV = Outputtitle + ".csv";       // CSV filename
	if (m_para->GetValue("rewrite_files") == "0" &&  CTools::IsFileExists(PrecursorslistFilePath_NameCSV))
	{
		cout << "[pParseTD] .CSV file already exists, pParse Complete!" << endl;
		return;
	}

	// Load the MS1 and MS2 files.
	cout << "[pParseTD] Loading MS1 and MS2 files..." << endl;
	MS1Reader MS1(MS1file, m_para, m_vIPV, m_cEmass, ExtendHalfwindow + atof(m_para->GetValue("isolation_width").c_str()) / 2);
	
	// If the MS1 is ITMS, pParse will use the xtract to convert RAWs into MGFs.
	if (MS1.m_InstrumentType != "FTMS") // TO CHECK 对于低精度的应该如何处理？可能不会有低精度的
	{
		cout << "[pParseTD] ITMS encountered.. use xtract_raw.exe..." << endl;
		string cmdline = "xtract_raw.exe -mgf -z 2-2 \"" + MS1file.substr(0, MS1file.size() - 4) + ".Raw\"";
		system(cmdline.c_str());
		return;
	}

	MS2Reader MS2(MS2file, m_vIPV, m_cEmass);
	MS2.LoadMS2();
	
	MS1.SetTrainingSet();              // wrm: 默认训练集为空？
	
	CspecDataIO *outInfo = new CspecDataIO;

	vector<string> MGFNameList;                                        // File_Path_Name List for different activationtype,CID,ETD,HCD.
	vector<string> PFNameList;
	for (int file_idx = 0; file_idx < OutPutFileTypeNum; ++file_idx)
	{
		MGFNameList.push_back(Outputtitle + MGFSuffix[file_idx]);
		PFNameList.push_back(Outputtitle + PFSuffix[file_idx]);
	}
	
	MS1.SetSpectrumTitle(FileName.substr(0, FileName.size() - 4));
	outInfo->OutputMS1ToPF(MS1,m_para->GetValue("outputpath") + MS1.SpectrumTitle + ".pf1"); // Comment by luolan
	
	int MS2Scancounts = MS2.GetSpectraNum();                           // Statistic the Number of MS1 and MS2 spectra! 
	//int MS1Scancounts = MS1.GetSpectraNum();
	                                                                   // Suggest using the buffer allocated here! To Speedup! 
	string Buffer;                                                     /* 32MB buffer for Data of MGF */
	Buffer.reserve(MGFBufferSize);
	
	int ProcessedMS2 = 0;                                              /* Represent the MS2 spectrum which has been processed! */

	FILE *pfilePositive = NULL;
	FILE *pfileNegative = NULL;
	if (m_para->GetValue("output_trainingdata") == "1")   
	{
		string NegativeSample = m_para->GetValue("outputpath") + "Negative.csv";             /* Output Negative Sample */
		string PositiveSample = m_para->GetValue("outputpath") + "Positive.csv";             /* Output Positive Sample */
		
		pfilePositive = fopen(PositiveSample.c_str(),"w");
		pfileNegative = fopen(NegativeSample.c_str(),"w");
		if (pfileNegative == NULL || pfilePositive == NULL)
		{
			//CErrInfo info("CMainFlow", "FlowKernel", "open Negative.csv or Positive.csv file failed.");
			//throw runtime_error(info.Get().c_str());
			throw ("CMainFlow::FlowKernel() open Negative.csv or Positive.csv file failed.");
		}
	}
	
	// load charge_mz_tolerance table
	Tables::InitChargeMzTolTable();

	int outputPrecursor = 0;
	double Peak_Tol_ppm = 1.0e-6 * atof(m_para->GetValue("mz_error_tolerance").c_str());
	double SNration = atof(m_para->GetValue("SN_threshold").c_str());
	// 遍历每一种碎裂模式
	for (int file_idx = 0; file_idx < OutPutFileTypeNum; ++file_idx)   /* For each activation type! */
	{
		if (!MS2.CheckOutputFlag(file_idx))  // 检查该谱图是否存在该碎裂模式
		{
			continue;
		}
		MS1.Initialize();
		//MS1.m_vnIndexListToOutput.clear();
		ofstream MGFfout;
		int scan;                                                         /* MS2 Scan */
		int precursorScan;                                                      /* MS1 Scan */
		int charge;                                                       /* Charge state of the precursor */
		double MZ;                                                        /* MZ of the precursor */
		int MS2Index = 0;                                                 /* The Index of current MS2 scan */
		int lastMS2Scan = 0;                                              /* The Scan of the previous spectrum, in case of two same scan */
		
		//#test
		int start = clock();
		
		// 遍历每一张二级谱
		for (MS2Index = 0; MS2Index < MS2Scancounts ; ++MS2Index)        /* For each MS/MS spectrum of current activationtype! */
		{
			string ActivationType;                                        /* Activationtype of current spectrum! */ 
			MS2.GetActivationType_By_Index(MS2Index, ActivationType);

			if (file_idx != MS2.GetOutPutidentifier(MS2Index))
			{
				continue;                                                /* If the ActivationType is not Consist, Continue ..*/ 
			}
			ProcessedMS2++;

			MS2.GetScan_MZ_Charge_By_Index(MS2Index, scan, charge, MZ, precursorScan); /* Get the Scan, Charge, MZ by the index of the spectrum! */ 
			
			if (scan == lastMS2Scan)
			{
				continue;                                               /* If current spectrum has the same scan as the last one, skip it. */
			} else {
				lastMS2Scan = scan;
			}
			MS1.m_vnIndexListToOutput.push_back(MS2Index);
			
			double activationCenter = MS2.GetActivationCenter(MS2Index);/* Get ActivationCenter by MS2 Index */
                                       
			if (eps >= fabs(activationCenter))
			{
				activationCenter = MZ;  // wrm:如果碎裂中心为0，则设单电荷母离子质荷比为碎裂中心
			}

			vector<CentroidPeaks> PrecursorCandidatePeaks;           /* Monoisotopic peak candidates. */
			// cout << "scan = " << scan << endl;
			/*if(scan == 3277){
				cout << scan << "," << MZ << endl;
			}*/
			/* Get information from the ms1 scan before and after the ms2 spectrum */
			// 获取候选母离子单同位素峰，从前5张、后5张同时查找,记录所有相关谱峰；从前后3张谱中获取候选最高峰，若当前谱图中包含碎裂中心，则不再要相邻谱图中的候选峰
			MS1.GetCandidatesPrecursorByMS2scan(scan, precursorScan, -Span, Span, activationCenter, PrecursorCandidatePeaks);

			//cout << "Precursor Candidate Peaks Number: " << PrecursorCandidatePeaks.size() << endl;
			//debug
			/*for(size_t x = 0; x < PrecursorCandidatePeaks.size(); ++x)
			{
				cout << PrecursorCandidatePeaks[x].m_lfMZ << "\t" << PrecursorCandidatePeaks[x].m_lfIntens << endl;
			}*/

			/*Get precursors and their features, assess with threshold and ranking score. */
			//cout << MS2Index << " " << activationCenter << " " << scan << endl;
			// 评估母离子，获取所有候选母离子的特征
			 MS1.AssessPrecursor(activationCenter, scan, precursorScan, PrecursorCandidatePeaks, *(MS1.pTrainSet));
			// MS1.AssessPrecursors(activationCenter, scan, precursorScan, PrecursorCandidatePeaks, *(MS1.pTrainSet));

			//cout << MS2Index << " " << ProcessedMS2 << endl;
			// 每41张谱输出一次进度
			if ( MS2Index % 41 == 0 || ProcessedMS2 == MS2Scancounts)
			{                                                        /* Speed | Schedule | Rate of Process */  
				//fprintf(stdout, "\r%s-%s MS1：%.0f%%", cActivateType[(file_idx)%ActivateTypeNum], cInstrumentType[(file_idx)/ActivateTypeNum], ProcessedMS2*100.0/MS2Scancounts);
				printf("[pParseTD] <Exporting %s-%s MS1>: ", cActivateType[(file_idx)%ActivateTypeNum], cInstrumentType[(file_idx)/ActivateTypeNum]);
				printf("%.0lf%%\r", (MS2Index + 1) * 100.0 / MS2Scancounts);
				fflush(stdout);
			}
		}
	
        /* Speed | Schedule | Rate of Process */  
		//fprintf(stdout, "\r%s-%s MS1：%.0f%%", cActivateType[(file_idx)%ActivateTypeNum], cInstrumentType[(file_idx)/ActivateTypeNum], ProcessedMS2*100.0/MS2Scancounts);
		printf("[pParseTD] <Exporting %s-%s MS1>: ", cActivateType[file_idx % ActivateTypeNum], cInstrumentType[file_idx / ActivateTypeNum]);
		printf("%.0lf%%\r", MS2Scancounts * 100.0 / MS2Scancounts);
		fflush(stdout);
		cout << endl;
		if (m_para->GetValue("output_trainingdata") == "1" && MS1.pTrainSet != NULL)
		{
			outInfo->OutputTrainingSample(MS1, pfilePositive, pfileNegative);
		}
		
		//#test
		int end = clock();
		int seconds = (end - start) / CLOCKS_PER_SEC;
		cout << "Time of Precursor Detection: " << seconds << "s." << endl;		
		start = clock();

		/* Machine Learning Method, classify the precursors! */
		//MS1.SVMPredict();
		/* Check which precursor to be exported */
		//MS1.OutputMarsPredictY();
		MS1.MacLearnClassify(m_para->GetValue("model_type"));

		//#test
		end = clock();
		seconds = (end - start) / CLOCKS_PER_SEC;
		cout << "Time of SVM: " << seconds << "s." << endl;		
		start = clock();

		// Deconvolution 对二级谱解卷积，去同位素峰簇，
		int curOutputInfo = 0;
		for (int k = 0; k < (int)MS1.m_vnIndexListToOutput.size(); ++k)
		{
			//cout << k << ": " << MS1.m_vnIndexListToOutput[k] << endl;
			int idx = MS1.m_vnIndexListToOutput[k];
			double oldMZ = 0.0;
			int chg = 0, scan = 0, precursorScan = 0;
			vector<double> precurMass;   // 中性母离子质量表
			MS2.GetScan_MZ_Charge_By_Index(idx, scan, chg, oldMZ, precursorScan);
			//cout << idx << " " << scan << " " << chg << " " << oldMZ << endl;
			if (curOutputInfo == (int)MS1.OutputInfo.size() || MS1.OutputInfo[curOutputInfo][0] > scan)
			{
				//cout << "first " << chg << endl;
				precurMass.push_back(chg * (oldMZ - pmass));
				if(chg > 1) 
				{
					MS2.MS2List[idx]->Deconvolution(chg, precurMass, Peak_Tol_ppm, SNration);
				}
			} else if(curOutputInfo < (int)MS1.OutputInfo.size()){
				chg = (int)MS1.OutputInfo[curOutputInfo][1];
				//cout << chg << endl;
				while (curOutputInfo < (int)MS1.OutputInfo.size() && int(MS1.OutputInfo[curOutputInfo][0]) == scan)
				{
					precurMass.push_back(MS1.OutputInfo[curOutputInfo][1] * (MS1.OutputInfo[curOutputInfo][2] - pmass));
					++curOutputInfo;	
				}
				// [wrm]: 二级谱去卷积，多个母离子为什么只传入了一个电荷？ 因为默认第一个母离子的电荷最准
				MS2.MS2List[idx]->Deconvolution(chg, precurMass, Peak_Tol_ppm, SNration);
				
			}
			if ( k % 41 == 0 || k+1 == (int)MS1.m_vnIndexListToOutput.size())
			{   /* Print the Rate of Process */  
				//fprintf(stdout, "\r%s-%s MS2：%.0f%%", cActivateType[(file_idx)%ActivateTypeNum], cInstrumentType[(file_idx)/ActivateTypeNum], (k+1) * 100.0 / (int)MS1.m_vnIndexListToOutput.size());
				printf("[pParseTD] <Exporting %s-%s MS2>: ", cActivateType[(file_idx)%ActivateTypeNum], cInstrumentType[(file_idx)/ActivateTypeNum]);
				printf("%.0lf%%\r", (k+1) * 100.0 / (int)MS1.m_vnIndexListToOutput.size());
				fflush(stdout);
			}
		}
        // #test
		cout << endl;
		end = clock();
		seconds = (end - start) / CLOCKS_PER_SEC;
		cout << "Deconvolution of MS2: " << seconds << "s." << endl;
		cout << endl;

		try
		{
			//outInfo->OutputToTXT(MS1, MS2, MGFNameList[file_idx]); // Output the mono highest peak in the raw file, for debug, or further use
			outputPrecursor += outInfo->OutputToMGF(MS1, MS2, MGFNameList[file_idx]);
			
			outInfo->OutputToPF(MS1, MS2, PFNameList[file_idx]);// add @2014.09.11
		
			if (MS1.pTrainSet != NULL)
			{
				MS1.pTrainSet->ResetCheckFlag();
			}
			// The precursors are checked again when output CSV file.
			outInfo->OutputPrecursorToCSV(MS1, MS2, strPrecursorsListToCSV, file_idx);
		}
		catch(exception & e)
		{
			CErrInfo info("CMainFlow", "FlowKernel", "output .mgf/.pf2 files failed.");
			throw runtime_error(info.Get(e).c_str());
			//throw ("CMainFlow::FlowKernel() output .mgf/.pf files failed.");
		}
		catch(...)
		{
			CErrInfo info("CMainFlow", "FlowKernel", "Caught an unknown exception from outputing .mgf/.pf2/.csv files.");
			throw runtime_error(info.Get().c_str());
			//throw ("CMainFlow::FlowKernel() Caught an unknown exception from outputing .mgf/.pf/.csv files.");
		}	
	}

	// [test]
	/*FILE * fp1 = fopen("isoPeakNum.txt","w");
	FILE * fp2 = fopen("preChg.txt","w");
	if(fp1 == NULL || fp2 == NULL){
	    printf("Create IsoPeakNum file error!\n");
		exit(-1);
	}
	char *buf = new char[2000000];
	int len = 0;
	for(int i=0; i<MS2.MS2List.size(); ++i){
		for(int j=0; j<MS2.MS2List[i]->t_IsoPeakNum.size(); ++j){
			len += sprintf(buf+len,"%d %d\n",MS2.MS2List[i]->t_IsoPeakNum[j],MS2.MS2List[i]->t_charge[j]);
		}
		if(i && i%1000==0){
		    fwrite(buf,1,len,fp1);
			len = 0;
		}
	}
	if(len){
	    fwrite(buf,1,len,fp1);
		len = 0;
	}
	fclose(fp1);
	for(int i=0; i<MS2.MS2List.size(); ++i){
		len += sprintf(buf+len,"%d\n",MS2.MS2List[i]->t_pre_charge);
		if(i && i%10000==0){
		    fwrite(buf,1,len,fp2);
			len = 0;
		}
	}
	if(len){
	    fwrite(buf,1,len,fp2);
	}
	fclose(fp2);*/

	/* Release the resources! */
	delete outInfo;
	if (pfileNegative != NULL)
	{
		fclose(pfileNegative);
	}
	if(pfilePositive != NULL)
	{
		fclose(pfilePositive);
	}

	try
	{
		ofstream csvout;
		csvout.open(PrecursorslistFilePath_NameCSV.c_str(), ios::out);
		csvout<< strPrecursorsListToCSV;
		csvout.close();
	}
	catch(exception & e)
	{
		CErrInfo info("CMainFlow", "FlowKernel", "output .csv files failed.");
		throw runtime_error(info.Get(e).c_str());
		//throw ("CMainFlow::FlowKernel() output .csv files failed.");
	}
	catch(...)
	{
		CErrInfo info("CMainFlow", "FlowKernel", "Caught an unknown exception from outputing .csv files.");
		throw runtime_error(info.Get().c_str());
		//throw ("CMainFlow::FlowKernel() Caught an unknown exception from outputing .csv files.");
	}	

	if (m_para->GetValue("delete_msn") == "1")
	{
		cout << "[pParseTD] Delete MS1 & MS2..." << endl;
		
		string cmdline = "del " + MS1file;
		system(cmdline.c_str());
		cmdline = "del " + MS2file;
		system(cmdline.c_str());
	}
	cout << "[pParseTD] Output " << CTools::int2string(outputPrecursor) << " precursors" << endl;
}