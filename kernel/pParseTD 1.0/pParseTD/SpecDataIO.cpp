#include <iostream>
#include <fstream>
#include <string>

#include "SpecDataIO.h"
#include "StringProcessor.h"

using namespace std;

CspecDataIO::CspecDataIO()
{

}
CspecDataIO::~CspecDataIO()
{

}

// peaks not in order
int CspecDataIO::OutputToMGF(MS1Reader &MS1, MS2Reader &MS2, string outfileName)
{
	if (outfileName.compare("") == 0)
	{
		outfileName = "F:\\test.mgf";
	}	
	int outputMS2Num = 0;		
	FILE *pfile = fopen(outfileName.c_str(), "w");
	//cout << outfileName << endl;
	if(NULL == pfile)
	{
		cout << "[Error] Cannot open the output file: " << outfileName.c_str() << endl;
		return -1;
	}

	string title = MS1.SpectrumTitle;
	int curOutputInfo = 0;
	for (int k = 0; k < (int)MS1.m_vnIndexListToOutput.size(); k++)
	{
		//cout << k << endl;
		int i = MS1.m_vnIndexListToOutput[k];
		double oldMZ = 0.0;
		int chg = 0, scan = 0, precursorScan = 0;
		MS2.GetScan_MZ_Charge_By_Index(i, scan, chg, oldMZ, precursorScan);
		//cout << curOutputInfo << " " << MS1.OutputInfo.size() << endl;
		if (curOutputInfo == (int)MS1.OutputInfo.size() || MS1.OutputInfo[curOutputInfo][0] > scan)
		{
			double mass = oldMZ * chg - (chg - 1) * pmass;
			//cout << mass << endl;
			fprintf(pfile, "BEGIN IONS\n");
			fprintf(pfile, "TITLE=%s.%d.%d.%d.0.dta\n", title.c_str(), scan, scan, chg);
			fprintf(pfile, "CHARGE=%d+\n", chg);
			fprintf(pfile, "PEPMASS=%f\n", oldMZ);

			// MS2, printPeaksTopFile
			for (int j = 0; j < (int)MS2.MS2List[i]->Peaks.size(); j++)
			{
				//[wrm20151103]: 与PF文件保持一致
				//if(MS2.MS2List[i]->Peaks[j].m_lfMass > mass +10) continue;
				fprintf(pfile, "%f %.1f\n", MS2.MS2List[i]->Peaks[j].m_lfMass, MS2.MS2List[i]->Peaks[j].m_lfIntens);
			}
			fprintf(pfile, "END IONS\n");
			++outputMS2Num;
		} else {
			int counter = 0;
			while (curOutputInfo < (int)MS1.OutputInfo.size() && int(MS1.OutputInfo[curOutputInfo][0]) == scan)
			{
				// convert to single charge 
				double mass = MS1.OutputInfo[curOutputInfo][2] * MS1.OutputInfo[curOutputInfo][1] - (MS1.OutputInfo[curOutputInfo][1] - 1) * pmass;
				fprintf(pfile, "BEGIN IONS\n");
				fprintf(pfile, "TITLE=%s.%d.%d.%d.%d.dta\n", title.c_str(), scan, scan, int(MS1.OutputInfo[curOutputInfo][1]), counter);
				fprintf(pfile, "CHARGE=%d+\n", int(MS1.OutputInfo[curOutputInfo][1]));
				fprintf(pfile, "PEPMASS=%f\n", MS1.OutputInfo[curOutputInfo][2]);
				
				//MS2.printPeaksTopFile(i,pfile);
				for (int j = 0; j < (int)MS2.MS2List[i]->Peaks.size(); j++)
				{
					//[wrm20151103]
					//if(MS2.MS2List[i]->Peaks[j].m_lfMass > mass + 10) continue;
					fprintf(pfile, "%f %.1f\n", MS2.MS2List[i]->Peaks[j].m_lfMass, MS2.MS2List[i]->Peaks[j].m_lfIntens);
				}
				fprintf(pfile,"END IONS\n");
				++counter;
				++curOutputInfo;
				++outputMS2Num;
			}
		}
	}
	fclose(pfile);
	return outputMS2Num;
}

// 输出格式，每张谱图的头信息与MGF相同，谱峰信息如下：
// MH, Intensity, MonoMz, HighestMz, Charge 
int CspecDataIO::OutputToTXT(MS1Reader &MS1, MS2Reader &MS2, string outfileName)
{
	if (outfileName.compare("") == 0)
	{
		outfileName = "F:\\test.txt";
	}	
	int outputMS2Num = 0;
	string fname = outfileName.substr(0, outfileName.size() - 4);
	fname.append(".txt");
	FILE *pfile = fopen(fname.c_str(), "w");
	if(NULL == pfile)
	{
		cout << "[Error] Cannot open the output file: " << fname.c_str() << endl;
		return -1;
	}

	string title = MS1.SpectrumTitle;
	int curOutputInfo = 0;
	for (int k = 0; k < (int)MS1.m_vnIndexListToOutput.size(); k++)
	{
		int i = MS1.m_vnIndexListToOutput[k];
		double oldMZ = 0.0;
		int chg = 0, scan = 0, precursorScan = 0;
		MS2.GetScan_MZ_Charge_By_Index(i, scan, chg, oldMZ, precursorScan);

		if (curOutputInfo == (int)MS1.OutputInfo.size() || MS1.OutputInfo[curOutputInfo][0] > scan)
		{
			double mass = oldMZ * chg - (chg - 1) * pmass;
			fprintf(pfile, "BEGIN IONS\n");
			fprintf(pfile, "TITLE=%s.%d.%d.%d.0.dta\n", title.c_str(), scan, scan, chg);
			fprintf(pfile, "CHARGE=%d+\n", chg);
			fprintf(pfile, "PEPMASS=%f\n", oldMZ);

			// MS2, printPeaksTopFile
			for (int j = 0; j < (int)MS2.MS2List[i]->Peaks.size(); j++)
			{
				if(MS2.MS2List[i]->Peaks[j].m_lfMass > mass + 10) continue;
				fprintf(pfile, "%f %.1f %f %f %d\n", MS2.MS2List[i]->Peaks[j].m_lfMass, MS2.MS2List[i]->Peaks[j].m_lfIntens,
					MS2.MS2List[i]->Peaks[j].m_lfMonoMz, MS2.MS2List[i]->Peaks[j].m_lfHighestMz, MS2.MS2List[i]->Peaks[j].m_nCharge);
			}
			fprintf(pfile, "END IONS\n");
			++outputMS2Num;
		} else {
			int counter = 0;
			while (curOutputInfo < (int)MS1.OutputInfo.size() && int(MS1.OutputInfo[curOutputInfo][0]) == scan)
			{
				double mass = MS1.OutputInfo[curOutputInfo][2] * MS1.OutputInfo[curOutputInfo][1] - (MS1.OutputInfo[curOutputInfo][1] - 1) * pmass;
				fprintf(pfile, "BEGIN IONS\n");
				fprintf(pfile, "TITLE=%s.%d.%d.%d.%d.dta\n", title.c_str(), scan, scan, int(MS1.OutputInfo[curOutputInfo][1]), counter);
				fprintf(pfile, "CHARGE=%d+\n", int(MS1.OutputInfo[curOutputInfo][1]));
				fprintf(pfile, "PEPMASS=%f\n", MS1.OutputInfo[curOutputInfo][2]);
				
				//MS2.printPeaksTopFile(i,pfile);
				for (int j = 0; j < (int)MS2.MS2List[i]->Peaks.size(); j++)
				{
					if(MS2.MS2List[i]->Peaks[j].m_lfMass > mass + 10) continue;
					fprintf(pfile, "%f %.1f %f %f %d\n", MS2.MS2List[i]->Peaks[j].m_lfMass, MS2.MS2List[i]->Peaks[j].m_lfIntens,
						MS2.MS2List[i]->Peaks[j].m_lfMonoMz, MS2.MS2List[i]->Peaks[j].m_lfHighestMz, MS2.MS2List[i]->Peaks[j].m_nCharge);
				}
				fprintf(pfile,"END IONS\n");
				++counter;
				++curOutputInfo;
				++outputMS2Num;
			}
		}
	}
	fclose(pfile);
	return outputMS2Num;
}

int CspecDataIO::OutputToPF(MS1Reader &MS1, MS2Reader &MS2, string outfileName)
{
	if (outfileName.compare("") == 0)
	{
		outfileName = "F:\\test.pf";
	}	
	int outputMS2Num = 0;		
	FILE *pfile = fopen(outfileName.c_str(), "wb");
	if(NULL == pfile)
	{
		cout << "[Error] Cannot open the output file: " << outfileName.c_str() << endl;
		return -1;
	}
	string pf2Idx = outfileName + "idx";
	FILE *pfileIdx = fopen(pf2Idx.c_str(), "wb");
	if(NULL == pfileIdx){
		cout << "[Error] Cannot open the output file: " << pf2Idx.c_str() << endl;
		return -1;
	}

	int nSize = MS1.m_vnIndexListToOutput.size();
	fwrite(&nSize, sizeof(int), 1, pfile);  // write spectra number

	string title = MS1.SpectrumTitle;
	nSize = title.length() + 1;             // c_str() will plus an additional terminating null-character ('\0') at the end.
	fwrite(&nSize, sizeof(int), 1, pfile);  // write spectra title length
	fwrite(title.c_str(), sizeof(char), nSize, pfile);

	int curOutputInfo = 0;
	for (int k = 0; k < (int)MS1.m_vnIndexListToOutput.size(); ++k)
	{
		int i = MS1.m_vnIndexListToOutput[k];
		double oldMZ = 0;
		int chg = 0, scan = 0, precursorScan = 0;
		MS2.GetScan_MZ_Charge_By_Index(i, scan, chg, oldMZ, precursorScan);
		int peakNum = 0;
		MS2.GetPeakNumByIndex(i, peakNum);
		nSize = ftell(pfile);
		fwrite(&scan,sizeof(int),1,pfileIdx);
		fwrite(&nSize,sizeof(int),1,pfileIdx);

		fwrite(&scan, sizeof(int), 1, pfile);     // write scan number
		fwrite(&peakNum, sizeof(int), 1, pfile);  // write peakNum

		// MS2, PrintPeaksToBinaryFile
		// [wrm?] one fwrite can solve
		for (int j = 0; j < (int)MS2.MS2List[i]->Peaks.size(); ++j)
		{
			fwrite(&(MS2.MS2List[i]->Peaks[j].m_lfMass), sizeof(double), 1, pfile);
			fwrite(&(MS2.MS2List[i]->Peaks[j].m_lfIntens), sizeof(double), 1, pfile);
		}

		if (curOutputInfo == (int)MS1.OutputInfo.size() || MS1.OutputInfo[curOutputInfo][0] > scan)
		{   // We cannot find the precursor of this MS2 in the result, then just give the default one
			int preNum = 1;
			fwrite(&preNum, sizeof(int), 1, pfile);
			fwrite(&oldMZ, sizeof(double), 1, pfile);
			fwrite(&chg, sizeof(int), 1, pfile);
			++outputMS2Num;
		} else {
			int counter = 0; // Count the number of the precursors                                            
			while(int(MS1.OutputInfo[curOutputInfo][0]) == scan)                                    
			{
				curOutputInfo++;                                                                  
				counter++;
				if (curOutputInfo == (int)MS1.OutputInfo.size())                                     
				{
					break;
				}
			}
			outputMS2Num += counter;
			curOutputInfo -= counter;     
			fwrite(&counter, sizeof(int), 1, pfile);

			while(int(MS1.OutputInfo[curOutputInfo][0]) == scan)
			{
				int tmpCharge = int(MS1.OutputInfo[curOutputInfo][1]);
				double tmpMZ = MS1.OutputInfo[curOutputInfo][2];

				fwrite(&tmpMZ, sizeof(double), 1, pfile);
				fwrite(&tmpCharge, sizeof(int), 1, pfile);
				curOutputInfo++;
				if (curOutputInfo == (int)MS1.OutputInfo.size())                                     
				{
					break;
				}
			} // end while	
		} // end else
	} // end for
	fclose(pfile);
	fclose(pfileIdx);
	return outputMS2Num;
}

bool CspecDataIO::OutputMS1ToPF(MS1Reader &MS1, string outfileName)
{
	if (outfileName.compare("") == 0)
	{
		outfileName = "F:\\test.mgf";
	}	
	//int outputMS2Num = 0;		
	FILE *pfile = fopen(outfileName.c_str(), "wb");
	if(NULL == pfile)
	{
		cout << "[Error] Cannot open the output file: " << outfileName.c_str() << endl;
		return false;
	}
	string pf1idx = outfileName + "idx";
	FILE *pfileIdx = fopen(pf1idx.c_str(), "wb");
	if(NULL == pfileIdx)
	{
		cout << "[Error] Cannot open the output file: " << pf1idx.c_str() << endl;
		return false;
	}
	int nSize = (int)MS1.MS1List.size();// MS1 number
	fwrite(&nSize, sizeof(int), 1, pfile);

	string title = MS1.SpectrumTitle;// title length
	nSize = title.length() + 1;
	fwrite(&nSize, sizeof(int), 1, pfile);
	fwrite(title.c_str(), sizeof(char), nSize, pfile);

	for (int i = 0; i < (int)MS1.MS1List.size(); ++i)
	{
		int scan = MS1.MS1List[i]->GetCurrentMS1Scan();
		int peakNum = MS1.MS1List[i]->GetPeakNum();

		fwrite(&scan,sizeof(int),1,pfileIdx);
		nSize = ftell(pfile);
		fwrite(&nSize,sizeof(int),1,pfileIdx);

		fwrite(&scan, sizeof(int), 1, pfile);
		fwrite(&peakNum, sizeof(int), 1, pfile);

		//PrintPeakToBinaryFile
		for (int j = 0; j < (int)MS1.MS1List[i]->Peaks.size(); ++j)
		{
			fwrite(&(MS1.MS1List[i]->Peaks[j].m_lfMZ), sizeof(double), 1, pfile);
			fwrite(&(MS1.MS1List[i]->Peaks[j].m_lfIntens), sizeof(double), 1, pfile);
		}

		double RT = MS1.MS1List[i]->GetRetiontime();
		int fakePreNum = 1;
		int fakeCharge = 1;
		//double fakeMZ = 0.0;
		
		fwrite(&fakePreNum, sizeof(int), 1, pfile);
		fwrite(&RT, sizeof(double), 1, pfile);     // Attention: Used to be FakeMZ.. Now it is taken by the RT
		fwrite(&fakeCharge, sizeof(int), 1, pfile);	
	}
	fclose(pfile);
	fclose(pfileIdx);
	return true;
}

/* Reload the function, output the precursors into the strPrecursorsListToCSV */
void CspecDataIO::OutputPrecursorToCSV(MS1Reader &MS1, MS2Reader &MS2, string &strPrecursorsListToCSV, int &file_idx)
{
	// statistic of the mixture spectra.
	int precursorsum = 0;
	int cur_of_OutputInfo = 0;
	char *csvbuf = new char[1024];
	for (int k = 0; k < (int)MS1.m_vnIndexListToOutput.size(); k++)
	{
		int i = MS1.m_vnIndexListToOutput[k];
		double oldMZ = 0;
		int chg = 0, Scan = 0, precursorScan = 0;
		MS2.GetScan_MZ_Charge_By_Index(i, Scan, chg, oldMZ, precursorScan);        // Get the oldmz and charge info.

		strPrecursorsListToCSV.append(cActivateType[(file_idx)%ActivateTypeNum]);                     // Output the instrument type.
		strPrecursorsListToCSV.append(",");
		
		strPrecursorsListToCSV.append(cInstrumentType[(file_idx)/ActivateTypeNum]);                        
		//Append Scan
		strPrecursorsListToCSV.append(",");
		sprintf(csvbuf,"%d",Scan);
		strPrecursorsListToCSV.append(csvbuf);
		strPrecursorsListToCSV.append(",");
		sprintf(csvbuf,"%f",oldMZ);                                                                // Revised by lwu 2013.10.25: extract original mz 
		strPrecursorsListToCSV.append(csvbuf);
		strPrecursorsListToCSV.append(",");
		sprintf(csvbuf,"%d",chg);                                                                   // Revised by lwu 2013.10.25: extract original chg 
		strPrecursorsListToCSV.append(csvbuf);                                                          
		strPrecursorsListToCSV.append(",");
		
		int counter = 0;
		if (cur_of_OutputInfo == (int)MS1.OutputInfo.size() || MS1.OutputInfo[cur_of_OutputInfo][0] > Scan)   // If reach the end of precursor list or there are no more precursor.
		{
			counter = -1;                                                                               // Set counter as -1.
		} else {
			counter = 0;                                               
			while (int(MS1.OutputInfo[cur_of_OutputInfo][0])==Scan)                                    // If find current scan
			{
				cur_of_OutputInfo++;                                                                   // Counter inc
				counter++;
				if (cur_of_OutputInfo == (int)MS1.OutputInfo.size())                                          // If reach then end of precursor list, break.
				{
					break;
				}
				
			}
			cur_of_OutputInfo -= counter;                                                               // Reset the cur, while we had got the precursor to be exported.
		
		}
		if (counter == -1)                                                                                // Output the number of precursor to be exported.
		{
			sprintf(csvbuf,"1");                                                                        // No precursor, output the original one.
		} else {
			sprintf(csvbuf,"%d",counter);                                                               // Number of precursors.
		}
		strPrecursorsListToCSV.append(csvbuf);
		strPrecursorsListToCSV.append(",");
		
		if (counter == -1)                                                                               // Export the original one.
		{
			sprintf(csvbuf,"1,");
			strPrecursorsListToCSV.append(csvbuf);
			

			sprintf(csvbuf,"%.6lf,",oldMZ);
			strPrecursorsListToCSV.append(csvbuf);
			
			sprintf(csvbuf,"%d,",chg);
			strPrecursorsListToCSV.append(csvbuf);
			precursorsum ++;
			if (MS1.pTrainSet!=NULL)
			{
				MS1.pTrainSet->CheckScanMZCharge(Scan,oldMZ,chg);
				MS1.pTrainSet->CheckMARSList(Scan,oldMZ,chg);
			}
			
		} else {                                                                                          // Export each of the new one.
			int cnt=1;
			while (int(MS1.OutputInfo[cur_of_OutputInfo][0])==Scan)                                      // We should use the counter here. I think.
			{
				sprintf(csvbuf,"%d,",cnt);
				strPrecursorsListToCSV.append(csvbuf);
				

				sprintf(csvbuf,"%.6lf,",MS1.OutputInfo[cur_of_OutputInfo][2]);
				strPrecursorsListToCSV.append(csvbuf);
				
				sprintf(csvbuf,"%d,",int(MS1.OutputInfo[cur_of_OutputInfo][1]));

				strPrecursorsListToCSV.append(csvbuf);
				if (MS1.pTrainSet!=NULL)
				{
					MS1.pTrainSet->CheckScanMZCharge(Scan,MS1.OutputInfo[cur_of_OutputInfo][2],int(MS1.OutputInfo[cur_of_OutputInfo][1]));
					MS1.pTrainSet->CheckMARSList(Scan,MS1.OutputInfo[cur_of_OutputInfo][2],int(MS1.OutputInfo[cur_of_OutputInfo][1]));
				}
				
				cur_of_OutputInfo++;
				precursorsum++;
				//
				if (cur_of_OutputInfo == (int)MS1.OutputInfo.size())                                               // In case of overflow.
				{
					break;
				}
				cnt++;
			}

		}
		strPrecursorsListToCSV.append("\n");                                                                 // Another line.
	
	}// for another scan.
	delete[]csvbuf;
}

void CspecDataIO::OutputTrainingSample(MS1Reader &MS1, FILE *pfilePos, FILE *pfileNeg)
{
	if (MS1.MarsFeature.size() == 0)   
	{
		cout << "No sample to be exported!" <<endl;
		return;
	}

	bool IsPositive = false;
	cout << "[pParseTD] Write training data set: " << MS1.MarsFeature.size() << endl;
	for (int i = 0; i < (int)MS1.MarsFeature.size(); ++i)
	{
		int tmpMS2Scan = (int)MS1.OutputInfo[i][0];
		int tmpChg = (int)MS1.OutputInfo[i][1];
		double tmpMZ = MS1.OutputInfo[i][2];
		IsPositive = MS1.pTrainSet->CheckScanMZCharge(tmpMS2Scan, tmpMZ, tmpChg);
		MS1.pTrainSet->CheckTopNList(tmpMS2Scan, tmpMZ, tmpChg);
		
		if (IsPositive)
		{
			/*fprintf(pfilePos,"%d,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f\n",tmpMS2Scan,
				MS1.MarsFeature[i][0],MS1.MarsFeature[i][1],MS1.MarsFeature[i][2],MS1.MarsFeature[i][3],MS1.MarsFeature[i][4],
				MS1.MarsFeature[i][5],MS1.MarsFeature[i][6],MS1.MarsFeature[i][7],MS1.MarsFeature[i][8],MS1.MarsFeature[i][9],
				MS1.MarsFeature[i][10]);*/
			fprintf(pfilePos, "%d", tmpMS2Scan);
			for(size_t j = 0; j < MS1.MarsFeature[i].size(); ++j)
			{
				fprintf(pfilePos, ",%f", MS1.MarsFeature[i][j]);
			}
			fprintf(pfilePos, "\n");
		}else{
			/*fprintf(pfileNeg,"%d,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f\n",tmpMS2Scan,
				MS1.MarsFeature[i][0],MS1.MarsFeature[i][1],MS1.MarsFeature[i][2],MS1.MarsFeature[i][3],MS1.MarsFeature[i][4],
				MS1.MarsFeature[i][5],MS1.MarsFeature[i][6],MS1.MarsFeature[i][7],MS1.MarsFeature[i][8],MS1.MarsFeature[i][9],
				MS1.MarsFeature[i][10]);*/
			fprintf(pfileNeg, "%d", tmpMS2Scan);
			for(size_t j = 0; j < MS1.MarsFeature[i].size(); ++j)
			{
				fprintf(pfileNeg, ",%f", MS1.MarsFeature[i][j]);
			}
			fprintf(pfileNeg, "\n");
		}
	}	
}

void CspecDataIO::MS1Parse(string infilename, MS1Reader *MS1)
{
	ifstream ifMS;
	string oneline;
	ifMS.open(infilename.c_str(), ifstream::in);
	char ch, tmps[1024], msType[128];
	int scan, peakNum;
	double retTime, injTime;
	while(ifMS.good())
	{
		getline(ifMS, oneline);
		if(oneline.size() <= 0) continue;
		if('H' == oneline[0]) 
		{
			continue;
		} else if('S' == oneline[0]) {
			sscanf(oneline.c_str(), "%c %d %*d", &ch, &scan);
			try{
				MS1Spectrum *tmpMs1 = new MS1Spectrum(scan);
				MS1->MS1List.push_back(tmpMs1);
			} catch(bad_alloc &ba) {
				cout << "MS1List " << ba.what() <<endl;
			}
		} else if('I' == oneline[0]) {
			if(oneline.find("NumberOfPeaks") != string::npos)
			{
				sscanf(oneline.c_str(), "%c %s %d", &ch, tmps, &peakNum);
				//MS1->MS1List.back()->Peaks.assign(peakNum, pair<double, double>(0,0));
			} else if(oneline.find("RetTime") != string::npos) {
				sscanf(oneline.c_str(), "%c %s %lf", &ch, tmps, &retTime);
				MS1->MS1List.back()->SetRetentiontime(retTime);
			} else if(oneline.find("IonInjectionTime") != string::npos) {
				sscanf(oneline.c_str(), "%c %s %lf", &ch, tmps, &injTime);
			} else if(oneline.find("InstrumentType") != string::npos) {
				sscanf(oneline.c_str(), "%c %s %s", &ch, tmps, msType);
				string tmpType(msType);
				CStringProcess::Trim(tmpType);
				if(CStringProcess::isInSet(HighPreSet, setSize, tmpType))
				{
					MS1->m_InstrumentType = "FTMS";
				} else if(CStringProcess::isInSet(LowPreSet, setSize, tmpType)) {
					MS1->m_InstrumentType = "ITMS";
				} else {
					MS1->m_InstrumentType = "Unknown";
				}
			}
		} else if(oneline[0] >= '0' && oneline[0] <= '9') {
			double mz = 0, intens = 0;
			sscanf(oneline.c_str(), "%lf %lf", &mz, &intens);
			MS1->MS1List.back()->AppendPeaks(mz, intens);
		}
	} // end while
	// 增加了一级谱Scan号到下标之间的映射，用空间换时间. modified by wrm  2015.10.10.
	for(int i=0; i<MS1->MS1List.size(); ++i){
		MS1->MS1Scan_Index[MS1->MS1List[i]->GetCurrentMS1Scan()] = i;
	}
}

void CspecDataIO::MS2Parse(string infilename, MS2Reader *MS2)
{
	ifstream ifMS;
	string oneline;
	ifMS.open(infilename.c_str(), ifstream::in);
	char ch, tmps[1024], msType[128], actType[128];
	int scan, peakNum, peakCnt, precursorScan;
	double retTime, injTime, mz, actCenter;
	while(ifMS.good())
	{
		getline(ifMS, oneline);
		if(oneline.size() <= 0) continue;
		if('H' == oneline[0]) 
		{
			continue;
		} else if('S' == oneline[0]) {
			//cout<<"S: "<<oneline.c_str()<<endl;
			sscanf(oneline.c_str(), "%c %d %*d", &ch, &scan);
			try{
				MS2Spectrum *tmpMs2 = new MS2Spectrum(scan, MS2->m_vIPV, MS2->m_cEmass);
				MS2->MS2List.push_back(tmpMs2);
			} catch(bad_alloc &ba) {
				cout << "MS1List " << ba.what() <<endl;
			}
			peakCnt = 0;
		} else if('I' == oneline[0]) {
			//cout<<"I: "<<oneline.c_str()<<endl;
			if(oneline.find("NumberOfPeaks") != string::npos)
			{
				sscanf(oneline.c_str(), "%c %s %d", &ch, tmps, &peakNum);
			} else if(oneline.find("RetTime") != string::npos) {
				sscanf(oneline.c_str(), "%c %s %lf", &ch, tmps, &retTime);
				MS2->MS2List.back()->SetRetentiontime(retTime);
			} else if(oneline.find("IonInjectionTime") != string::npos) {
				sscanf(oneline.c_str(), "%c %s %lf", &ch, tmps, &injTime);
			} else if(oneline.find("ActivationType") != string::npos) {
				// todo
				sscanf(oneline.c_str(), "%c %s %s", &ch, tmps, actType);
				MS2->MS2List.back()->SetActivationType(actType);
			} else if(oneline.find("InstrumentType") != string::npos) {
				sscanf(oneline.c_str(), "%c %s %s", &ch, tmps, msType);
				string tmpType(msType);
				CStringProcess::Trim(tmpType);
				if(CStringProcess::isInSet(HighPreSet, setSize, tmpType))
				{
					MS2->MS2List.back()->SetInstrumentType("FT");
				} else {
					MS2->MS2List.back()->SetInstrumentType("IT");
				}
			} else if(oneline.find("PrecursorScan") != string::npos) {
				// 记录二级谱对应的一级谱Scan.  modified by wrm. 2015.10.10
				sscanf(oneline.c_str(), "%c %s %d", &ch, tmps, &precursorScan);
				MS2->MS2List.back()->SetPrecursorScan(precursorScan);
			} else if(oneline.find("ActivationCenter") != string::npos) {
				sscanf(oneline.c_str(), "%c %s %lf", &ch, tmps, &actCenter);
				MS2->MS2List.back()->SetActivationCenter(actCenter);
			}else if(oneline.find("MonoiosotopicMz") != string::npos) {
				sscanf(oneline.c_str(), "%c %s %lf", &ch, tmps, &mz);
				if (mz < 1e-5 && mz > -1e-5)
					mz = actCenter;
				MS2->MS2List.back()->SetMZ(mz);
			}
		} else if('Z' == oneline[0]) {
			// 这里需要用到InstrumentType和ActivationType，但是两者在I中的先后关系可能会改变，所以放在这里更保险
			int pos = MS2->MS2List.back()->GetOutPutFileIdentifier();
			if(pos >= 0 && pos < 6)
			{
				MS2->OutputFlag[pos] = true;
			} else {
				cout<<"[Error] The InstrumentType is not recognizable!"<<endl;
				exit(0);
			}
			
			//cout<<"Z: "<<oneline.c_str()<<endl;
			int chg;
			double mass;
			sscanf(oneline.c_str(), "%c %d %lf", &ch, &chg, &mass);
			MS2->MS2List.back()->Setcharge(chg);
		} else if(oneline[0] >= '0' && oneline[0] <= '9') {
			double mz = 0, intens = 0;
			sscanf(oneline.c_str(), "%lf %lf", &mz, &intens);
			MS2->MS2List.back()->AppendPeaks(mz, intens);
			++peakCnt;
		}
	} // end while
}