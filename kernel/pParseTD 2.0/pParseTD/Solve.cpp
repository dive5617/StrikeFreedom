// ============================================================================
// Project	   : pParseTD
// File        : Solve.cpp
//
// Author      : luo lan (from Wu Long's pParsePlus)
// Version     : 1.0.1
//
// Copyright   : Copyright (c) 2014 - All rights reserved
// Description : Extract correct monoisotopic peaks in MS1 and MS2

// Last update : 2014.9.19
// ============================================================================

#include <iostream>
#include <string>
#include <cstring>
#include <cstdio>
#include <ctime>
#include <exception>
#include <map>
#include <windows.h>

#include "StringProcessor.h"
#include "TrainingFilereader.h"
#include "BasicFunction.h"
#include "MonoCandidates.h"
#include "Parameters.h"

using namespace std;

int main(int argc , char * argv[])
{
	try
	{
		CWelcomeInfo *cIn = new CWelcomeInfo;
		//cIn->PrintLogo();           // Print the LOGO of pParse @ pfind.ict.ac.cn
		if(false == cIn->CheckDate()) // Check whether pParse is expired today!
		{
			cout << "[Error] Sorry, the software has expired.\nPlease visit http://pfind.ict.ac.cn//software.html"
			" and download the newest version." << endl;
			exit(1);
		}
		delete cIn;

		cout << "[pParseTD] Start pParseTD!" << endl;

		CMainFlow parseFlow;

		int start = clock();
		
		/*if(1 == argc)
		{
			parseFlow.RunFromFile("D:\\pTop1.0.2\\kernel\\pParseTD\\Release\\pParseTD.cfg");
		}
		else*/
		//wzz
		argv[1] = "E:\\个人文件整理\\源程序\\kernel\\pParseTD 2.0\\x64\\Release\\pParseTD.cfg";
		if (argc == 2 && CStringProcess::bMatchingFix(argv[1], ".cfg", true, true)) 
		{
			parseFlow.RunFromFile(argv[1]);
		} else {
			parseFlow.RunFromCMD(argc, argv);
		}

		int seconds = (clock() - start) / CLOCKS_PER_SEC;
		cout << "[pParseTD] Time elapsed: " << seconds << "s." << endl;
	}
	catch(exception & e)
	{
		CErrInfo info("pParseTD.exe", "Main", "failed.", e);
		ofstream ofs("pParseTD.err.log", ios::app);
		ofs << info;
		cout << info << endl;
		cout << "Please contact pFind group for help." << endl;
		exit(1);
	}
	catch(...)
	{
		CErrInfo info("pParseTD.exe", "Main", "Caught an unknown exception.");
		ofstream ofs("pParseTD.err.log", ios::app);
		ofs << info;
		cout << info << endl;
		cout << "Please contact pfind group for help." << endl;
		exit(1);
	}	
	return 0;
}


