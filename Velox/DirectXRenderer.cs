using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;

namespace Velox
{

    public class DirectXRenderer : IDisposable
    {
        private WindowRenderTarget _renderTarget;

        public DirectXRenderer(DirectXRenderingSystem renderingSystem, nint hwnd)
        {
            var renderTargetProps = new RenderTargetProperties
            {
                Type = RenderTargetType.Default,
                PixelFormat = new SharpDX.Direct2D1.PixelFormat(
                    SharpDX.DXGI.Format.Unknown,
                    SharpDX.Direct2D1.AlphaMode.Premultiplied),
                DpiX = renderingSystem.DpiX,
                DpiY = renderingSystem.DpiY,
                Usage = RenderTargetUsage.None,
                MinLevel = FeatureLevel.Level_DEFAULT
            };

            Win32.GetClientRect(hwnd, out var rect);

            var hwndRenderTargetProps = new HwndRenderTargetProperties
            {
                Hwnd = hwnd,
                PixelSize = new Size2(rect.right, rect.bottom),
                PresentOptions = PresentOptions.None
            };

            _renderTarget = new WindowRenderTarget(renderingSystem.Direct2DFactory, renderTargetProps, hwndRenderTargetProps)
            {
                // Grayscale antialiasing required for transparent/backdrop surfaces —
                // ClearType assumes a known opaque background for subpixel rendering.
                TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Cleartype,
            };

            // Set rendering mode explicitly to match WPF's ClearType quality.
            // CLEARTYPE_NATURAL_SYMMETRIC is what modern WPF targets.
            using var renderingParams = new RenderingParams(
                renderingSystem.DirectWriteFactory,
                gamma: 1.8f,            // WPF default gamma
                enhancedContrast: 0.5f, // WPF default contrast
                clearTypeLevel: 1.0f,   // Full ClearType
                pixelGeometry: PixelGeometry.Rgb,
                renderingMode: RenderingMode.CleartypeNaturalSymmetric
            );

            _renderTarget.TextRenderingParams = renderingParams;
        }

        public void Render(Action<RenderTarget> render)
        {
            _renderTarget.BeginDraw();

            _renderTarget.Clear(new RawColor4(0, 0, 0, 0)); // transparent — lets DWM backdrop show through

            try
            {
                render?.Invoke(_renderTarget);
            }
            finally
            {
                _renderTarget.EndDraw();
            }
        }

        public void Resize(int width, int height)
        {
            _renderTarget?.Resize(new Size2(width, height));
        }

        public void Dispose()
        {
            _renderTarget?.Dispose();
        }
    }
}