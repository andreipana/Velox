using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Velox
{
    public class Application
    {
        public Application()
        {
            Win32.SetHighDpiMode();
        }

        public Window CreateWindow()
        {
            return new Window();
        }

        public void Run()
        {
            Win32.MSG msg;
            while (Win32.GetMessage(out msg, IntPtr.Zero, 0, 0))
            {
                Win32.TranslateMessage(ref msg);
                Win32.DispatchMessage(ref msg);
            }
        }
    }
}
