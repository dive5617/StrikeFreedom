using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace pTop
{
    public static class Common
    {

        public static readonly int WM_SETTEXT = 0x000C;
        public static readonly int WM_GETTEXT = 0x000D;
        public static readonly int WM_COPYDATA = 0x004A;
        public static readonly int WM_CLICK = 0x00F5;

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(
        int hWnd, // handle to destination window
        int Msg, // message
        int wParam, // first message parameter
        ref COPYDATASTRUCT lParam // second message parameter
        );
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(int hWnd,
        int Msg, int wParam, string lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true)]
        public static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public static int SendCopyData(IntPtr hWnd, int dwData, byte[] lpdata)
        {

            COPYDATASTRUCT cds = new COPYDATASTRUCT();

            cds.dwData = (IntPtr)dwData;

            cds.cbData = lpdata.Length;

            cds.lpData = Marshal.AllocHGlobal(lpdata.Length);
            Marshal.Copy(lpdata, 0, cds.lpData, lpdata.Length);
            IntPtr lParam = Marshal.AllocHGlobal(Marshal.SizeOf(cds));
            Marshal.StructureToPtr(cds, lParam, true);
            int result = 0;

            try
            {
                result = SendMessage(hWnd, WM_COPYDATA, IntPtr.Zero, lParam);
            }

            finally
            {
                Marshal.FreeHGlobal(cds.lpData);
                Marshal.DestroyStructure(lParam, typeof(COPYDATASTRUCT));
                Marshal.FreeHGlobal(lParam);
            }
            return result;
        }

        public struct COPYDATASTRUCT
        {
            /// 用户自定义数据
            public IntPtr dwData;

            /// 数据长度
            public int cbData;

            /// 数据地址指针
            public IntPtr lpData;
        }


        //public struct COPYDATASTRUCT
        //{
        //    public IntPtr dwData;
        //    public int cbData;
        //    [MarshalAs(UnmanagedType.LPStr)]
        //    public string lpData;
        //}
    }
    public static class MiniDump
   {
        /**//*
         * 导入DbgHelp.dll
         */
        [DllImport("DbgHelp.dll")]
        private static extern Boolean MiniDumpWriteDump(
                                    IntPtr hProcess,
                                    Int32 processId,
                                    IntPtr fileHandle,
                                    MiniDumpType dumpType, 
                                    ref MinidumpExceptionInfo excepInfo,
                                    IntPtr userInfo, 
                                    IntPtr extInfo );

        /**//*
         *  MINIDUMP_EXCEPTION_INFORMATION  这个宏的信息
         */
        struct MinidumpExceptionInfo
        {
            public Int32 ThreadId;
            public IntPtr ExceptionPointers;
            public Boolean ClientPointers;
        }

        /**//*
         * 自己包装的一个函数
         */
        public static Boolean TryDump(String dmpPath, MiniDumpType dmpType)
        {

            //使用文件流来创健 .dmp文件
            using (FileStream stream = new FileStream(dmpPath, FileMode.Create,FileAccess.ReadWrite))
            {
                //取得进程信息
                Process process = Process.GetCurrentProcess();

                // MINIDUMP_EXCEPTION_INFORMATION 信息的初始化
                MinidumpExceptionInfo mei = new MinidumpExceptionInfo();

                mei.ThreadId = Thread.CurrentThread.ManagedThreadId;
                mei.ExceptionPointers = Marshal.GetExceptionPointers();
                mei.ClientPointers = true;

                Boolean res = MiniDumpWriteDump(
                                    process.Handle,
                                    process.Id,
                                    stream.Handle,//stream.SafeFileHandle.DangerousGetHandle(),
                                    dmpType,
                                    ref mei,
                                    IntPtr.Zero,
                                    IntPtr.Zero);

                //清空 stream
                stream.Flush();
                stream.Close();

                return res;
            }
        }

        public enum MiniDumpType
        {
            None = 0x00010000,
            Normal = 0x00000000,
            WithDataSegs = 0x00000001,
            WithFullMemory = 0x00000002,
            WithHandleData = 0x00000004,
            FilterMemory = 0x00000008,
            ScanMemory = 0x00000010,
            WithUnloadedModules = 0x00000020,
            WithIndirectlyReferencedMemory = 0x00000040,
            FilterModulePaths = 0x00000080,
            WithProcessThreadData = 0x00000100,
            WithPrivateReadWriteMemory = 0x00000200,
            WithoutOptionalData = 0x00000400,
            WithFullMemoryInfo = 0x00000800,
            WithThreadInfo = 0x00001000,
            WithCodeSegs = 0x00002000
        }
    }


}
