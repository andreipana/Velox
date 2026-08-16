using System.Runtime.InteropServices;
using Velox.DirectX;

namespace Velox
{
    // Single reusable, topmost, D2D-rendered popup used to display any Control's
    // ToolTip content. One instance is lazily created per owning Window and reused
    // for every tooltip that Window shows — never recreated, just repositioned,
    // resized and repainted for whatever control is currently hovered.
    //
    // Reuses the owning Window's DirectXRenderingSystem (D2D/DWrite factories) so
    // it shares DPI and doesn't spin up a second D2D device.
    internal sealed class TooltipWindow : IDisposable
    {
        private const float Padding      = 8f;
        private const float MaxWidth     = 350f;
        private const float MaxHeight    = 2000f;
        private const float CornerRadius = 4f;
        private const uint  BackgroundColor = 0xFF2B2B2BU;
        private const uint  BorderColor     = 0xFF454545U;

        private readonly Win32.WndProcDelegate _wndProc; // held to prevent GC collection
        private readonly IntPtr _hwnd;
        private readonly DirectXRenderingSystem _sys;
        private readonly DirectXRenderer _renderer;
        private bool _visible;

        public TooltipWindow(DirectXRenderingSystem renderingSystem)
        {
            _sys = renderingSystem;

            // This window never receives input (WS_EX_NOACTIVATE + topmost/tool-window),
            // so it needs no custom message handling beyond the default.
            _wndProc = Win32.DefWindowProc;

            var wc = new Win32.WNDCLASSEX
            {
                cbSize        = Marshal.SizeOf<Win32.WNDCLASSEX>(),
                style         = Win32.CS_DROPSHADOW,
                lpfnWndProc   = _wndProc,
                hInstance     = Win32.GetModuleHandle(null),
                lpszClassName = "VeloxTooltip",
                hbrBackground = IntPtr.Zero,
            };
            Win32.RegisterClassEx(ref wc);

            // Created at a plausible placeholder size, not 0x0/1x1 — degenerate
            // initial dimensions aren't needed now that Show() always resizes the
            // render target from the window's real, post-SetWindowPos client rect.
            _hwnd = Win32.CreateWindowEx(
                Win32.WS_EX_TOPMOST | Win32.WS_EX_NOACTIVATE | Win32.WS_EX_TOOLWINDOW,
                wc.lpszClassName, "",
                Win32.WS_POPUP,
                0, 0, 200, 100,
                IntPtr.Zero, IntPtr.Zero, wc.hInstance, IntPtr.Zero);

            _renderer = new DirectXRenderer(_sys, _hwnd);
        }

        // screenX/screenY: preferred top-left corner, in physical screen pixels
        // (clamped to the nearest monitor's work area before showing).
        public void Show(IControl content, int screenX, int screenY, float dpiScale)
        {
            // MeasureText only touches the DWrite factory, never the render target,
            // so this can measure without a live BeginDraw/EndDraw pair.
            var measureGraphics = new DirectXGraphics(IntPtr.Zero, _sys, MaxWidth, MaxHeight);
            var (dw, dh) = content.Measure(measureGraphics, MaxWidth, MaxHeight);

            float w = dw + Padding * 2f;
            float h = dh + Padding * 2f;
            int pxW = Math.Max(1, (int)MathF.Ceiling(w * dpiScale));
            int pxH = Math.Max(1, (int)MathF.Ceiling(h * dpiScale));

            (screenX, screenY) = ClampToMonitor(screenX, screenY, pxW, pxH);

            // The window must be moved/resized (and its swap chain surface reallocated
            // by the OS) BEFORE the D2D render target is resized and painted — resizing
            // the render target first and only moving the window afterward presents a
            // stale/blank frame instead of the one just drawn.
            Win32.SetWindowPos(_hwnd, Win32.HWND_TOPMOST, screenX, screenY, pxW, pxH,
                Win32.SWP_NOACTIVATE | Win32.SWP_SHOWWINDOW);

            _renderer.Resize(pxW, pxH);
            _renderer.Render(g =>
            {
                g.FillRoundedRect(0, 0, w, h, CornerRadius, BackgroundColor);
                g.DrawRoundedRect(0, 0, w, h, CornerRadius, BorderColor);
                content.Arrange(Padding, Padding, dw, dh);
                content.Render(g);
            }, w, h);

            _visible = true;
        }

        public void Hide()
        {
            if (!_visible) return;
            Win32.ShowWindow(_hwnd, Win32.SW_HIDE);
            _visible = false;
        }

        private static (int x, int y) ClampToMonitor(int x, int y, int w, int h)
        {
            IntPtr monitor = Win32.MonitorFromPoint(new Win32.POINT { x = x, y = y }, Win32.MONITOR_DEFAULTTONEAREST);
            var info = new Win32.MONITORINFO { cbSize = Marshal.SizeOf<Win32.MONITORINFO>() };
            if (monitor == IntPtr.Zero || !Win32.GetMonitorInfo(monitor, ref info))
                return (x, y);

            int clampedX = Math.Clamp(x, info.rcWork.left, Math.Max(info.rcWork.left, info.rcWork.right - w));
            int clampedY = Math.Clamp(y, info.rcWork.top, Math.Max(info.rcWork.top, info.rcWork.bottom - h));
            return (clampedX, clampedY);
        }

        public void Dispose()
        {
            _renderer.Dispose();
            Win32.DestroyWindow(_hwnd);
        }
    }
}
