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


int main(int argc, char * argv[])
{
	// 设置处理Unhandled Exception的回调函数  
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

