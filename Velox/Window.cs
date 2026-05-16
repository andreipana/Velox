using SharpDX.Direct2D1;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Velox
{
    public class Window
    {
        DirectXRenderingSystem renderingSystem;

        private Win32.WndProcDelegate _wndProc;

        IntPtr hwnd;

        private DirectXRenderer _renderer2;

        private readonly List<IControl> _controls = new();
        private bool _mouseTracking = false;

        public event EventHandler? Resized;

        public (float Width, float Height) ClientSizeDip
        {
            get
            {
                Win32.GetClientRect(hwnd, out Win32.RECT r);
                return (r.right  * 96f / renderingSystem.DpiX,
                        r.bottom * 96f / renderingSystem.DpiY);
            }
        }

        public (int Width, int Height) Size
        {
            get
            {
                Win32.GetWindowRect(hwnd, out Win32.RECT r);
                return (r.right - r.left, r.bottom - r.top);
            }
            set => Win32.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, value.Width, value.Height, Win32.SWP_NOMOVE | Win32.SWP_NOZORDER);
        }

        public void AddControl(IControl control)
        {
            _controls.Add(control);
            Win32.InvalidateRect(hwnd, IntPtr.Zero, false);
        }

        public Window()
        {
            _wndProc = WndProc; // must be kept alive — GC will collect a temporary delegate

            var wc = new Win32.WNDCLASSEX();
            wc.cbSize = Marshal.SizeOf(wc);
            wc.lpfnWndProc = _wndProc;
            wc.hInstance = Win32.GetModuleHandle(null);
            wc.lpszClassName = "D2DWindow";
            wc.hCursor = Win32.LoadCursor(IntPtr.Zero, Win32.IDC_ARROW);
            wc.hbrBackground = IntPtr.Zero; // no background brush — DWM backdrop fills it

            Win32.RegisterClassEx(ref wc);

            hwnd = Win32.CreateWindowEx(
                0,
                wc.lpszClassName,
                "Velox",
                Win32.WS_OVERLAPPEDWINDOW,
                100, 100, (int)(800 / 1.5f), (int)(600 / 1.5f),
                IntPtr.Zero, IntPtr.Zero,
                wc.hInstance, IntPtr.Zero
            );

            EnableBackdrop(hwnd, Win32.DWMSBT_MAINWINDOW); // Mica
            SetTheme(hwnd, WindowTheme.Dark);

            renderingSystem = new DirectXRenderingSystem();
            _renderer2 = new DirectXRenderer(renderingSystem, hwnd);

            RenderInternal(); // pre-paint before the window is visible to avoid flicker

            Win32.ShowWindow(hwnd, Win32.SW_SHOW);
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
                    Resized?.Invoke(this, EventArgs.Empty);
                    break;

                case Win32.WM_DESTROY:
                    Win32.PostQuitMessage(0);
                    break;

                case Win32.WM_MOUSEMOVE:
                {
                    EnsureMouseTracking(hWnd);
                    var (mx, my) = Win32.GetMousePos(lParam);
                    float dx = mx * 96f / renderingSystem.DpiX;
                    float dy = my * 96f / renderingSystem.DpiY;
                    foreach (var c in _controls) c.OnMouseMove(dx, dy);
                    Win32.InvalidateRect(hWnd, IntPtr.Zero, false);
                    break;
                }

                case Win32.WM_LBUTTONDOWN:
                {
                    var (mx, my) = Win32.GetMousePos(lParam);
                    float dx = mx * 96f / renderingSystem.DpiX;
                    float dy = my * 96f / renderingSystem.DpiY;
                    foreach (var c in _controls) c.OnMouseDown(dx, dy);
                    Win32.InvalidateRect(hWnd, IntPtr.Zero, false);
                    break;
                }

                case Win32.WM_LBUTTONUP:
                {
                    var (mx, my) = Win32.GetMousePos(lParam);
                    float dx = mx * 96f / renderingSystem.DpiX;
                    float dy = my * 96f / renderingSystem.DpiY;
                    foreach (var c in _controls) c.OnMouseUp(dx, dy);
                    Win32.InvalidateRect(hWnd, IntPtr.Zero, false);
                    break;
                }

                case Win32.WM_MOUSELEAVE:
                    _mouseTracking = false;
                    foreach (var c in _controls) c.OnMouseLeave();
                    Win32.InvalidateRect(hWnd, IntPtr.Zero, false);
                    break;

                case Win32.WM_MOUSEWHEEL:
                {
                    float delta = Win32.GetWheelDelta(wParam) / 120f;
                    foreach (var c in _controls) c.OnMouseWheel(delta);
                    Win32.InvalidateRect(hWnd, IntPtr.Zero, false);
                    break;
                }
            }
            return Win32.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private void EnsureMouseTracking(IntPtr hWnd)
        {
            if (_mouseTracking) return;
            var tme = new Win32.TRACKMOUSEEVENT
            {
                cbSize    = Marshal.SizeOf<Win32.TRACKMOUSEEVENT>(),
                dwFlags   = Win32.TME_LEAVE,
                hwndTrack = hWnd,
                dwHoverTime = 0,
            };
            Win32.TrackMouseEvent(ref tme);
            _mouseTracking = true;
        }

        private void RenderInternal() => _renderer2.Render(Render);

        public virtual void Render(RenderTarget renderTarget)
        {
            Win32.GetClientRect(hwnd, out Win32.RECT r);
            float dipWidth  = r.right  * 96f / renderingSystem.DpiX;
            float dipHeight = r.bottom * 96f / renderingSystem.DpiY;

            var graphics = new DirectXGraphics(renderTarget, renderingSystem, dipWidth, dipHeight);
            foreach (var control in _controls)
                if (control.IsVisible) control.Render(graphics);
        }

        private static void EnableBackdrop(IntPtr hwnd, int backdropType)
        {
            var margins = new Win32.MARGINS { cxLeftWidth = -1, cxRightWidth = -1, cyTopHeight = -1, cyBottomHeight = -1 };
            Win32.DwmExtendFrameIntoClientArea(hwnd, ref margins);
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
            const string key = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            using var reg = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(key);
            return reg?.GetValue("AppsUseLightTheme") is int v && v == 0;
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
