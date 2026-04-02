using System.Runtime.InteropServices;

namespace Velox
{
    internal static class Win32
    {
        // ---------------- Constants ----------------

        public const int WS_OVERLAPPEDWINDOW = 0x00CF0000;
        public const int WS_VISIBLE          = 0x10000000;
        public const int WM_DESTROY          = 0x0002;
        public const int WM_PAINT            = 0x000F;
        public const int WM_SIZE             = 0x0005;

        public static readonly IntPtr IDC_ARROW = (IntPtr)32512;

        // DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = -2  (equivalent to HighDpiMode.SystemAware)
        private static readonly IntPtr DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = new IntPtr(-2);

        // PROCESS_DPI_AWARENESS: PROCESS_SYSTEM_DPI_AWARE = 1
        private const int PROCESS_SYSTEM_DPI_AWARE = 1;

        // ---------------- Structs / Delegates ----------------

        public delegate IntPtr WndProcDelegate(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct WNDCLASSEX
        {
            public int cbSize;
            public int style;
            public WndProcDelegate lpfnWndProc;
            public int cbClsExtra, cbWndExtra;
            public IntPtr hInstance, hIcon, hCursor, hbrBackground;
            public string lpszMenuName, lpszClassName;
            public IntPtr hIconSm;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public int message;
            public IntPtr wParam, lParam;
            public uint time;
            public int x, y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left, top, right, bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int cxLeftWidth, cxRightWidth, cyTopHeight, cyBottomHeight;
        }

        // ---------------- DPI ----------------

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetProcessDpiAwarenessContext(IntPtr value);

        [DllImport("shcore.dll", SetLastError = true)]
        private static extern int SetProcessDpiAwareness(int value);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetProcessDPIAware();

        /// <summary>
        /// Sets the process DPI awareness to SystemAware using native Win32 calls,
        /// equivalent to Application.SetHighDpiMode(HighDpiMode.SystemAware).
        /// Falls back gracefully on older Windows versions.
        /// </summary>
        public static void SetHighDpiMode()
        {
            // Try the modern API first (Windows 10 1607+)
            if (SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_SYSTEM_AWARE))
                return;

            // Fall back to Windows 8.1+ API
            if (SetProcessDpiAwareness(PROCESS_SYSTEM_DPI_AWARE) == 0 /* S_OK */)
                return;

            // Last resort: Windows Vista+
            SetProcessDPIAware();
        }

        // ---------------- P/Invoke ----------------

        [DllImport("user32.dll")]   public static extern bool RegisterClassEx(ref WNDCLASSEX lpwcx);
        [DllImport("user32.dll")]   public static extern IntPtr CreateWindowEx(
                                        int exStyle, string className, string windowName,
                                        int style, int x, int y, int w, int h,
                                        IntPtr parent, IntPtr menu, IntPtr instance, IntPtr param);
        [DllImport("user32.dll")]   public static extern bool GetMessage(out MSG msg, IntPtr hWnd, uint min, uint max);
        [DllImport("user32.dll")]   public static extern void TranslateMessage(ref MSG msg);
        [DllImport("user32.dll")]   public static extern void DispatchMessage(ref MSG msg);
        [DllImport("user32.dll")]   public static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]   public static extern void PostQuitMessage(int code);
        [DllImport("user32.dll")]   public static extern bool GetClientRect(IntPtr hWnd, out RECT rect);
        [DllImport("user32.dll", SetLastError = true)]
                                    public static extern IntPtr LoadCursor(IntPtr hInstance, IntPtr lpCursorName);
        [DllImport("kernel32.dll")] public static extern IntPtr GetModuleHandle(string? name);
        [DllImport("user32.dll")]   public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]   public static extern bool ValidateRect(IntPtr hWnd, IntPtr lpRect);

        public const int SW_SHOW = 5;

        // ---------------- DWM (Windows 11 backdrop) ----------------

        // DWMWA_USE_IMMERSIVE_DARK_MODE (Windows 10 20H1+, build 19041+)
        public const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        // DWMWA_SYSTEMBACKDROP_TYPE (Windows 11 22H2+, build 22621+)
        public const int DWMWA_SYSTEMBACKDROP_TYPE = 38;

        public const int DWMSBT_MAINWINDOW      = 2; // Mica
        public const int DWMSBT_TRANSIENTWINDOW = 3; // Acrylic
        public const int DWMSBT_TABBEDWINDOW    = 4; // Mica Alt

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS pMarInset);
    }
}
