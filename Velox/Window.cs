using System.Collections.Generic;
using System.Runtime.InteropServices;
using Velox.DirectX;

namespace Velox
{
    public class Window : IDisposable
    {
        DirectXRenderingSystem renderingSystem;

        private Win32.WndProcDelegate _wndProc;

        IntPtr hwnd;

        private IntPtr _hIconBig   = IntPtr.Zero;
        private IntPtr _hIconSmall = IntPtr.Zero;

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

        public float DpiX => renderingSystem.DpiX;
        public float DpiY => renderingSystem.DpiY;

        public bool AlwaysOnTop
        {
            get => _alwaysOnTop;
            set
            {
                _alwaysOnTop = value;
                Win32.SetWindowPos(hwnd,
                    value ? Win32.HWND_TOPMOST : Win32.HWND_NOTOPMOST,
                    0, 0, 0, 0,
                    Win32.SWP_NOMOVE | Win32.SWP_NOSIZE);
            }
        }
        private bool _alwaysOnTop;

        public string Title
        {
            get
            {
                int len = Win32.GetWindowTextLength(hwnd);
                if (len == 0) return string.Empty;
                var sb = new System.Text.StringBuilder(len + 1);
                Win32.GetWindowText(hwnd, sb, sb.Capacity);
                return sb.ToString();
            }
            set => Win32.SetWindowText(hwnd, value);
        }

        // Extracts the first icon from the running exe (the one embedded by <ApplicationIcon>)
        // and sets it as the window icon. ExtractIconEx reads from the file on disk, which is
        // more reliable than LoadImage with a module handle in .NET 6+ hosted processes.
        public void LoadIconFromResource()
        {
            if (_hIconBig   != IntPtr.Zero) { Win32.DestroyIcon(_hIconBig);   _hIconBig   = IntPtr.Zero; }
            if (_hIconSmall != IntPtr.Zero) { Win32.DestroyIcon(_hIconSmall); _hIconSmall = IntPtr.Zero; }

            string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName;
            Win32.ExtractIconEx(exePath, 0, out _hIconBig, out _hIconSmall, 1);

            Win32.SendMessage(hwnd, Win32.WM_SETICON, new IntPtr(Win32.ICON_BIG),   _hIconBig);
            Win32.SendMessage(hwnd, Win32.WM_SETICON, new IntPtr(Win32.ICON_SMALL), _hIconSmall);
        }

        public void Dispose()
        {
            if (_hIconBig   != IntPtr.Zero) { Win32.DestroyIcon(_hIconBig);   _hIconBig   = IntPtr.Zero; }
            if (_hIconSmall != IntPtr.Zero) { Win32.DestroyIcon(_hIconSmall); _hIconSmall = IntPtr.Zero; }
        }

        public void Invalidate()   => Win32.InvalidateRect(hwnd, IntPtr.Zero, false);
        public void CaptureMouse() => Win32.SetCapture(hwnd);
        public void ReleaseMouse() => Win32.ReleaseCapture();

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
            wc.lpszClassName = "Velox";
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
                    Resized?.Invoke(this, EventArgs.Empty);
                    RenderInternal();
                    Win32.ValidateRect(hWnd, IntPtr.Zero); // suppress the WM_PAINT Windows queues after WM_SIZE
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

                case Win32.WM_RBUTTONDOWN:
                {
                    var (mx, my) = Win32.GetMousePos(lParam);
                    float dx = mx * 96f / renderingSystem.DpiX;
                    float dy = my * 96f / renderingSystem.DpiY;
                    foreach (var c in _controls) c.OnRightMouseDown(dx, dy);
                    Win32.InvalidateRect(hWnd, IntPtr.Zero, false);
                    break;
                }

                case Win32.WM_RBUTTONUP:
                {
                    var (mx, my) = Win32.GetMousePos(lParam);
                    float dx = mx * 96f / renderingSystem.DpiX;
                    float dy = my * 96f / renderingSystem.DpiY;
                    foreach (var c in _controls) c.OnRightMouseUp(dx, dy);
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
                    // lParam carries screen coordinates for WM_MOUSEWHEEL (unlike other mouse
                    // messages which carry client coordinates). Convert before scaling to DIPs.
                    var (sx, sy) = Win32.GetMousePos(lParam);
                    var pt = new Win32.POINT { x = sx, y = sy };
                    Win32.ScreenToClient(hWnd, ref pt);
                    float dx = pt.x * 96f / renderingSystem.DpiX;
                    float dy = pt.y * 96f / renderingSystem.DpiY;
                    foreach (var c in _controls) c.OnMouseWheel(delta, dx, dy);
                    Win32.InvalidateRect(hWnd, IntPtr.Zero, false);
                    break;
                }

                case Win32.WM_KEYDOWN:
                case Win32.WM_SYSKEYDOWN:
                {
                    var  key   = (VirtualKey)wParam.ToInt32();
                    bool ctrl  = (Win32.GetKeyState(Win32.VK_CONTROL) & 0x8000) != 0;
                    bool shift = (Win32.GetKeyState(Win32.VK_SHIFT)   & 0x8000) != 0;
                    bool alt   = (Win32.GetKeyState(Win32.VK_MENU)    & 0x8000) != 0;
                    foreach (var c in _controls) c.OnKeyDown(key, ctrl, shift, alt);
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

        private void RenderInternal()
        {
            Win32.GetClientRect(hwnd, out Win32.RECT r);
            float dipWidth  = r.right  * 96f / renderingSystem.DpiX;
            float dipHeight = r.bottom * 96f / renderingSystem.DpiY;
            _renderer2.Render(Render, dipWidth, dipHeight);
        }

        public virtual void Render(IGraphics graphics)
        {
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
