using SharpDX.Direct2D1;
using System.Runtime.InteropServices;

namespace Velox
{
    public class Window
    {
        DirectXRenderingSystem renderingSystem;

        private Win32.WndProcDelegate _wndProc;

        IntPtr hwnd;

        private DirectXRenderer _renderer2;

        TextBlock _textBlock = new TextBlock()
        {
            Text = "Strumming my pain with his fingers",
            X = 0,
            Y = 0,
            SizeMode = SizeMode.Fill,
            Background = Colors.ToRawColor4(0x44FF00FF),
        };

        public Window()
        {
            _wndProc = WndProc; // must be kept alive - GC will collect a temporary delegate

            var wc = new Win32.WNDCLASSEX();
            wc.cbSize = Marshal.SizeOf(wc);
            wc.lpfnWndProc = _wndProc;
            wc.hInstance = Win32.GetModuleHandle(null);
            wc.lpszClassName = "D2DWindow";
            wc.hCursor = Win32.LoadCursor(IntPtr.Zero, Win32.IDC_ARROW);
            wc.hbrBackground = IntPtr.Zero; // no background brush - DWM backdrop fills it

            Win32.RegisterClassEx(ref wc);

            hwnd = Win32.CreateWindowEx(
                0,
                wc.lpszClassName,
                "Raw Win32 + Direct2D (C#)",
                Win32.WS_OVERLAPPEDWINDOW,
                100, 100, (int)(800 / 1.5f), (int)(600 / 1.5f),
                IntPtr.Zero, IntPtr.Zero,
                wc.hInstance, IntPtr.Zero
            );

            EnableBackdrop(hwnd, Win32.DWMSBT_MAINWINDOW); // Mica
            SetTheme(hwnd, WindowTheme.Dark);

            renderingSystem = new DirectXRenderingSystem();
            _renderer2 = new DirectXRenderer(renderingSystem, hwnd);

            RenderInternal(); // pre-paint the DirectX surface before the window is visible, so that there's no flicker.

            Win32.ShowWindow(hwnd, Win32.SW_SHOW); // DWM already has content therefore no flash
        }

        IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case Win32.WM_PAINT:
                    RenderInternal();
                    break;

                case Win32.WM_SIZE:
                    Resize();
                    RenderInternal();
                    Win32.ValidateRect(hWnd, IntPtr.Zero); // suppress the WM_PAINT Windows queues after WM_SIZE
                    break;

                case Win32.WM_DESTROY:
                    Win32.PostQuitMessage(0);
                    break;
            }
            return Win32.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private void RenderInternal()
        {
            _renderer2.Render(Render);
        }

        public virtual void Render(RenderTarget renderTarget)
        {
            Win32.GetClientRect(hwnd, out Win32.RECT r);
            float dipWidth  = r.right  * 96f / renderingSystem.DpiX;
            float dipHeight = r.bottom * 96f / renderingSystem.DpiY;
            _textBlock.Render(renderingSystem, renderTarget, dipWidth, dipHeight);
        }

        private static void EnableBackdrop(IntPtr hwnd, int backdropType)
        {
            // Extend the DWM glass frame across the entire client area
            var margins = new Win32.MARGINS { cxLeftWidth = -1, cxRightWidth = -1, cyTopHeight = -1, cyBottomHeight = -1 };
            Win32.DwmExtendFrameIntoClientArea(hwnd, ref margins);

            // Request the Windows 11 system backdrop material (requires build 22621+)
            Win32.DwmSetWindowAttribute(hwnd, Win32.DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, sizeof(int));
        }

        public enum WindowTheme { Light, Dark, System }

        public static void SetTheme(IntPtr hwnd, WindowTheme theme)
        {
            int useDark = theme switch
            {
                WindowTheme.Dark   => 1,
                WindowTheme.Light  => 0,
                WindowTheme.System => IsSystemDarkMode() ? 1 : 0,
                _                  => 0,
            };
            Win32.DwmSetWindowAttribute(hwnd, Win32.DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDark, sizeof(int));
        }

        private static bool IsSystemDarkMode()
        {
            // AppsUseLightTheme: 0 = dark, 1 (or missing) = light
            const string key = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            using var reg = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(key);
            return reg?.GetValue("AppsUseLightTheme") is int v && v == 0;
        }

        private float SnapToPhysicalPixel(float dipValue, float dpi)
        {
            float scale = dpi / 96.0f;
            return MathF.Floor(dipValue * scale + 0.5f) / scale;
        }

        void Resize()
        {
            if (_renderer2 == null)
                return;

            Win32.GetClientRect(hwnd, out Win32.RECT r);
            _renderer2.Resize(r.right, r.bottom);
        }
    }
}
