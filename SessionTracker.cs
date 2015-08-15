using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FileSaver
{
    class SessionTracker
    {
        private delegate IntPtr WindowProcDelegate(IntPtr hwnd, int uMsg, IntPtr wParam, IntPtr lParam);
        private const int WS_POPUP = int.MinValue;
        private const int GWLP_WNDPROC = -4;
        private const int WM_QUERYENDSESSION = 0x11;
        private const int WM_ENDSESSION = 0x16;
        private static readonly IntPtr HWND_MESSAGE = IntPtr.Subtract(IntPtr.Zero, 3);
        private static readonly IntPtr hInstance = GetModuleHandle(IntPtr.Zero);
        private readonly WindowProcDelegate windowProcDelegate;
        private IntPtr messageWindow = CreateWindowEx(0, "Static", "FileSaver.SessionTracker", WS_POPUP, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);
        private IntPtr newWindowProc, originalWindowProc;
        private bool blockingShutdown;
        private Dispatcher dispatcher;

        public SessionTracker()
        {
            windowProcDelegate = WindowProc;
            newWindowProc = Marshal.GetFunctionPointerForDelegate(windowProcDelegate);
            originalWindowProc = SetWindowLong(messageWindow, GWLP_WNDPROC, newWindowProc);
            dispatcher = Dispatcher.CurrentDispatcher;
        }

        public void BlockShutdown(string reason)
        {
            if(dispatcher.Thread == Thread.CurrentThread)
                blockingShutdown = ShutdownBlockReasonCreate(messageWindow, reason);
            else
            {
                Action<string> action = BlockShutdown;
                dispatcher.Invoke(action, reason);
            }
        }

        public void UnblockShutdown()
        {
            if(dispatcher.Thread == Thread.CurrentThread)
            {
                ShutdownBlockReasonDestroy(messageWindow);
                blockingShutdown = false;
            }
            else
            {
                Action action = UnblockShutdown;
                dispatcher.Invoke(action);
            }
        }

        private IntPtr WindowProc(IntPtr hwnd, int uMsg, IntPtr wParam, IntPtr lParam)
        {
            switch(uMsg)
            {
            case WM_QUERYENDSESSION:
                if(blockingShutdown)
                    return IntPtr.Zero;
                break;
            case WM_ENDSESSION:
                break;
            }
            return CallWindowProc(originalWindowProc, hwnd, uMsg, wParam, lParam);
        }

        [DllImport("kernel32")]
        private extern static IntPtr GetModuleHandle(IntPtr lpModuleName);
        [DllImport("user32")]
        private extern static IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32")]
        private extern static IntPtr CreateWindowEx(int dwExStyle, string lpClassName, string lpWindowName, int dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);
        [DllImport("user32")]
        private extern static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32", CharSet = CharSet.Unicode)]
        private extern static bool ShutdownBlockReasonCreate(IntPtr hWnd, string pwszReason);
        [DllImport("user32", CharSet = CharSet.Unicode)]
        private extern static bool ShutdownBlockReasonDestroy(IntPtr hWnd);
    }
}
