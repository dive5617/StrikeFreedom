#include <iostream>
#include <fstream>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <ctime>
// #include <tchar.h>
// #include <dbghelp.h>
#include <string.h>

#include "flow.h"
#include "test.h"
using namespace std;

#pragma comment(lib,"pthreadVC2.lib")

// #pragma auto_inline (off)
// #pragma comment( lib, "DbgHelp" )

// // 创建Dump文件    
// void CreateDumpFile1(LPCWSTR lpstrDumpFilePathName, EXCEPTION_POINTERS *pException)  
// {  
    // // 创建Dump文件   
    // HANDLE hDumpFile = CreateFile((LPCTSTR)lpstrDumpFilePathName, GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);  
  
    // // Dump信息  
    // MINIDUMP_EXCEPTION_INFORMATION dumpInfo;  
    // dumpInfo.ExceptionPointers = pException;  
    // dumpInfo.ThreadId = GetCurrentThreadId();  
    // dumpInfo.ClientPointers = TRUE;  
  
    // // 写入Dump文件内容   
    // MiniDumpWriteDump(GetCurrentProcess(), GetCurrentProcessId(), hDumpFile, MiniDumpNormal, &dumpInfo, NULL, NULL);  
  
    // CloseHandle(hDumpFile);  
// }  

// // 处理Unhandled Exception的回调函数  
// LONG ApplicationCrashHandler(EXCEPTION_POINTERS *pException)  
// {     
    // // 这里弹出一个错误对话框并退出程序    
	// CreateDumpFile1((LPCWSTR)_T("pTop.dmp"), pException);  
    // FatalAppExit(-1,  " Unhandled Exception! \n You can check pTop.dmp");  
  
    // return EXCEPTION_EXECUTE_HANDLER;  
// }  

void Welcome()
{
	cout << "****************************************" << endl;
	cout << "*                                      *" << endl;
	cout << "*       Welcome to use pTop!           *" << endl;
	cout << "*                                      *" << endl;
	cout << "*            version 2.0               *" << endl;
	cout << "*                                      *" << endl;
	cout << "*        http://pfind.ict.ac.cn        *" << endl;
	cout << "*                                      *" << endl;
	cout << "****************************************" << endl;
}

void Usage()
{
	cout << "[pTop] Usage: pTop.exe config.cfg" << endl;
	FILE *fp = fopen("config.cfg", "r");
	if (fp == NULL)
	{
		fclose(fp);
		FILE *fini = fopen("config.cfg", "w");
		fprintf(fini, "[database]");
		fprintf(fini, "Database=test.fasta");
		fprintf(fini, "[spectrum]");
		fprintf(fini, "Spec_path=test.mgf");
		fprintf(fini, "Output_path=test.txt");
		fprintf(fini, "Report_path=filter_test.txt");
		fprintf(fini, "Activation=CID");
		fprintf(fini, "Precursor_Tolerance=2");
		fprintf(fini, "Fragment_Tolerance=20");
		fprintf(fini, "[fixmodify]");
		fprintf(fini, "fixedModify_num=1");
		fprintf(fini, "fix_mod1=Carbamidomethyl[C]");
		fprintf(fini, "[modify]");
		fprintf(fini, "Max_mod=6");
		fprintf(fini, "Modify_num=2");
		fprintf(fini, "var_mod1=Acetyl[K]");
		fprintf(fini, "var_mod2=Acetyl[S]");
		fprintf(fini, "[filter]");
		fprintf(fini, "threshold=0.01");
		fclose(fini);
	}
}

int main(int argc, char * argv[])
{
	// 设置处理Unhandled Exception的回调函数  
    //SetUnhandledExceptionFilter((LPTOP_LEVEL_EXCEPTION_FILTER)ApplicationCrashHandler);   

	//string config = "D:\\pTop1.0.2\\kernel\\pTop\\Release\\pTop.cfg";
	//string config = "pTop.cfg";
	string config = "E:\\simulation_debug\\param\\pTop.cfg";
	if(argc == 2)
	{
		config = argv[1];
	} else {
		cout << "Usage: pTop.exe [configFile]" << endl;
		return 0;
	}
	time_t t_start,t_end;
	t_start = clock();

	// temporarily changed line remember to delete or uncomment it
	// fr wangzhenzhen
	//config = "E:\\simulation_debug\\param\\pTop.cfg";

	try
	{
		/**
		*	Print verson and welcome UI of pTop
		*	and start the timer
		**/
		Welcome();

		//Test::testRerank(config);
		CMainFlow cflow(config);
		cflow.RunMainFlow();
	}
	catch(exception & e) 
	{
		CErrInfo info("pTop.exe", "Main", "failed.", e);
		ofstream ofs("pTop.err.log", ios::app);
		ofs << info;
		//cout << info << endl;
		return 1;
	}
	catch(...)
	{
		CErrInfo info("pTop.exe", "Main", "caught an unknown exception.");
		ofstream ofs("pTop.err.log", ios::app);
		ofs << info;
		//cout << info << endl;
		return 1;
	}	

    t_end = clock();

    //cout << "[pTop] Total Execution time: " << (double)(t_end - t_start) / 1000.0 << "s" << endl;

    return 0;
}

