namespace Velox
{
    public class DirectXRenderer : IDisposable
    {
        private IntPtr _renderTarget;
        private readonly DirectXRenderingSystem _renderingSystem;

        public DirectXRenderer(DirectXRenderingSystem renderingSystem, nint hwnd)
        {
            _renderingSystem = renderingSystem;

            var rtProps = new D2D1_RENDER_TARGET_PROPERTIES
            {
                type        = D2D1_RENDER_TARGET_TYPE.DEFAULT,
                pixelFormat = new D2D1_PIXEL_FORMAT
                {
                    format    = DXGI_FORMAT.UNKNOWN,
                    alphaMode = D2D1_ALPHA_MODE.PREMULTIPLIED,
                },
                dpiX     = renderingSystem.DpiX,
                dpiY     = renderingSystem.DpiY,
                usage    = D2D1_RENDER_TARGET_USAGE.NONE,
                minLevel = D2D1_FEATURE_LEVEL.DEFAULT,
            };

            Win32.GetClientRect(hwnd, out var rect);
            var hwndProps = new D2D1_HWND_RENDER_TARGET_PROPERTIES
            {
                hwnd           = hwnd,
                pixelSize      = new D2D1_SIZE_U { width = (uint)rect.right, height = (uint)rect.bottom },
                presentOptions = D2D1_PRESENT_OPTIONS.NONE,
            };

            _renderTarget = D2D1Vtbl.Factory_CreateHwndRenderTarget(
                renderingSystem.D2dFactory, ref rtProps, ref hwndProps);

            // Grayscale antialiasing required for transparent/backdrop surfaces —
            // ClearType assumes a known opaque background for subpixel rendering.
            D2D1Vtbl.RT_SetTextAntialiasMode(_renderTarget, D2D1_TEXT_ANTIALIAS_MODE.CLEARTYPE);

            // Set rendering mode explicitly to match WPF's ClearType quality.
            // NATURAL_SYMMETRIC is what modern WPF targets.
            IntPtr renderingParams = D2D1Vtbl.DWrite_CreateCustomRenderingParams(
                renderingSystem.DWriteFactory,
                gamma: 1.8f, enhancedContrast: 0.5f, clearTypeLevel: 1.0f,
                DWRITE_PIXEL_GEOMETRY.RGB, DWRITE_RENDERING_MODE.NATURAL_SYMMETRIC);
            D2D1Vtbl.RT_SetTextRenderingParams(_renderTarget, renderingParams);
            D2D1Vtbl.Release(renderingParams);
        }

        public void Render(Action<IGraphics> render, float dipWidth, float dipHeight)
        {
            var clearColor = new D2D1_COLOR_F(0, 0, 0, 0); // transparent — lets DWM backdrop show through
            D2D1Vtbl.RT_BeginDraw(_renderTarget);
            D2D1Vtbl.RT_Clear(_renderTarget, ref clearColor);
            try
            {
                var graphics = new DirectXGraphics(_renderTarget, _renderingSystem, dipWidth, dipHeight);
                render?.Invoke(graphics);
            }
            finally
            {
                D2D1Vtbl.RT_EndDraw(_renderTarget);
            }
        }

        public void Resize(int width, int height)
        {
            if (_renderTarget == IntPtr.Zero) return;
            var size = new D2D1_SIZE_U { width = (uint)width, height = (uint)height };
            D2D1Vtbl.RT_Resize(_renderTarget, ref size);
        }

        public void Dispose()
        {
            D2D1Vtbl.Release(_renderTarget);
            _renderTarget = IntPtr.Zero;
        }
    }
}
