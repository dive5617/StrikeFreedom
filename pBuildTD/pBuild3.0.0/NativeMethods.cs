using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace pBuild
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct COPYDATASTRUCT
    {
        public IntPtr dwData;
        public int cbData;
        [MarshalAs(UnmanagedType.LPStr)]
        public string path;
    }
    internal static class NativeMethods
    {
        //public const int HWND_BROADCAST = 0xffff;
        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");
        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr wnd, int msg, IntPtr wP, IntPtr lP);
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage
        (
            IntPtr hWnd,    //目标窗体句柄
            int Msg,        //WM_COPYDATA
            IntPtr wParam,     //自定义数值
            ref COPYDATASTRUCT lParam //结构体
        );
        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);
        //
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool OpenClipboard(IntPtr hWndNewOwner);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EmptyClipboard();
        [DllImport("user32.dll")]
        internal static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseClipboard();
        [DllImport("gdi32.dll")]
        internal static extern IntPtr CopyEnhMetaFile(IntPtr hemfSrc, IntPtr hNULL);
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteEnhMetaFile(IntPtr hemf);

        /// <see href="http://msdn2.microsoft.com/en-us/library/ms648063.aspx"/>
        [DllImport("User32.dll", SetLastError = true)]
        internal static extern int DestroyIcon(IntPtr hIcon);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr hObject);

    }
}
