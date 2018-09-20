#include <iostream>
#include <fstream>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <ctime>
#include <windows.h>
// #include <tchar.h>
// #include <dbghelp.h>
#include <string.h>

#include "MainFlow.h"
#include "test.h"
using namespace std;

// #pragma auto_inline (off)
// #pragma comment( lib, "DbgHelp" )

// // ����Dump�ļ�    
// void CreateDumpFile1(LPCWSTR lpstrDumpFilePathName, EXCEPTION_POINTERS *pException)  
// {  
    // // ����Dump�ļ�   
    // HANDLE hDumpFile = CreateFile((LPCTSTR)lpstrDumpFilePathName, GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);  
  
    // // Dump��Ϣ  
    // MINIDUMP_EXCEPTION_INFORMATION dumpInfo;  
    // dumpInfo.ExceptionPointers = pException;  
    // dumpInfo.ThreadId = GetCurrentThreadId();  
    // dumpInfo.ClientPointers = TRUE;  
  
    // // д��Dump�ļ�����   
    // MiniDumpWriteDump(GetCurrentProcess(), GetCurrentProcessId(), hDumpFile, MiniDumpNormal, &dumpInfo, NULL, NULL);  
  
    // CloseHandle(hDumpFile);  
// }  

// // ����Unhandled Exception�Ļص�����  
// LONG ApplicationCrashHandler(EXCEPTION_POINTERS *pException)  
// {     
    // // ���ﵯ��һ������Ի����˳�����    
	// CreateDumpFile1((LPCWSTR)_T("pTop.dmp"), pException);  
    // FatalAppExit(-1,  " Unhandled Exception! \n You can check pTop.dmp");  
  
    // return EXCEPTION_EXECUTE_HANDLER;  
// }  


int main(int argc, char * argv[])
{
	// ���ô���Unhandled Exception�Ļص�����  
    //SetUnhandledExceptionFilter((LPTOP_LEVEL_EXCEPTION_FILTER)ApplicationCrashHandler);   

	//string config = "D:\\pTop1.0.2\\kernel\\pTop\\Release\\pTop.cfg";
	string config = "pTop.cfg";
	if(argc == 2)
	{
		config = argv[1];
	} else {
		cout << "Usage: pTop.exe [configFile]" << endl;
		return 0;
	}
	time_t t_start,t_end;
	t_start = clock();

	try
	{
		//Test::testFilter(config);
		CMainFlow cflow(config);
		cflow.RunMainFlow();
	}
	catch(exception & e) 
	{
		CErrInfo info("pTop.exe", "Main", "failed.", e);
		ofstream ofs("pTop.err.log", ios::app);
		ofs << info;
		cout << info << endl;
		return 1;
	}
	catch(...)
	{
		CErrInfo info("pTop.exe", "Main", "caught an unknown exception.");
		ofstream ofs("pTop.err.log", ios::app);
		ofs << info;
		cout << info << endl;
		return 1;
	}	

    t_end = clock();

    printf("[pTop] Total Execution time : %lfs\n",(double)(t_end - t_start) / 1000.0);

    return 0;
}

