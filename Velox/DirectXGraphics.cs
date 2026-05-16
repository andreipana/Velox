using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace Velox
{
    internal sealed class DirectXGraphics : IGraphics
    {
        private readonly RenderTarget _rt;
        private readonly DirectXRenderingSystem _sys;

        public float Width  { get; }
        public float Height { get; }

        internal DirectXGraphics(RenderTarget rt, DirectXRenderingSystem sys, float dipWidth, float dipHeight)
        {
            _rt    = rt;
            _sys   = sys;
            Width  = dipWidth;
            Height = dipHeight;
        }

        public void FillRect(float x, float y, float w, float h, uint argbColor)
        {
            using var brush = new SolidColorBrush(_rt, ToColor(argbColor));
            _rt.FillRectangle(ToRect(x, y, w, h), brush);
        }

        public void DrawRect(float x, float y, float w, float h, uint argbColor, float strokeWidth = 1f)
        {
            using var brush = new SolidColorBrush(_rt, ToColor(argbColor));
            _rt.DrawRectangle(ToRect(x, y, w, h), brush, strokeWidth);
        }

        public void FillRoundedRect(float x, float y, float w, float h, float cornerRadius, uint argbColor)
        {
            using var brush = new SolidColorBrush(_rt, ToColor(argbColor));
            _rt.FillRoundedRectangle(
                new RoundedRectangle { Rect = ToRect(x, y, w, h), RadiusX = cornerRadius, RadiusY = cornerRadius },
                brush);
        }

        public void DrawRoundedRect(float x, float y, float w, float h, float cornerRadius, uint argbColor, float strokeWidth = 1f)
        {
            using var brush = new SolidColorBrush(_rt, ToColor(argbColor));
            _rt.DrawRoundedRectangle(
                new RoundedRectangle { Rect = ToRect(x, y, w, h), RadiusX = cornerRadius, RadiusY = cornerRadius },
                brush,
                strokeWidth);
        }

        public void DrawLine(float x1, float y1, float x2, float y2, uint argbColor, float strokeWidth = 1f)
        {
            using var brush = new SolidColorBrush(_rt, ToColor(argbColor));
            _rt.DrawLine(new RawVector2(x1, y1), new RawVector2(x2, y2), brush, strokeWidth);
        }

        public void DrawText(string text, string fontFace, float fontSize,
                             float x, float y, float maxWidth, float maxHeight, uint argbColor,
                             bool noWrap = false)
        {
            using var fmt    = _sys.CreateTextFormat(fontFace, fontSize);
            if (noWrap) 
                fmt.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;
            if (maxWidth < 0)
                return;
            if (maxHeight < 0)
                return;
            using var layout = _sys.CreateWpfCompatibleLayout(fmt, text, maxWidth, maxHeight);
            using var brush  = new SolidColorBrush(_rt, ToColor(argbColor));
            _rt.DrawTextLayout(new RawVector2(x, y), layout, brush, DrawTextOptions.None);
        }

        public (float width, float height) MeasureText(string text, string fontFace, float fontSize,
                                                       float maxWidth, float maxHeight)
        {
            using var fmt    = _sys.CreateTextFormat(fontFace, fontSize);
            using var layout = _sys.CreateWpfCompatibleLayout(fmt, text, maxWidth, maxHeight);
            var m = layout.Metrics;
            return (m.Width, m.Height);
        }

        public void PushClip(float x, float y, float w, float h)
            => _rt.PushAxisAlignedClip(ToRect(x, y, w, h), AntialiasMode.PerPrimitive);

        public void PopClip()
            => _rt.PopAxisAlignedClip();

        private static RawColor4 ToColor(uint argb) => Colors.ToRawColor4(argb);

        private static RawRectangleF ToRect(float x, float y, float w, float h)
            => new RawRectangleF(x, y, x + w, y + h);
    }
}
