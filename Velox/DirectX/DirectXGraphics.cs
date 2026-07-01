using Velox;

namespace Velox.DirectX
{
    internal sealed class DirectXGraphics : IGraphics
    {
        private readonly IntPtr _rt;
        private readonly DirectXRenderingSystem _sys;
        private readonly Stack<D2D1_MATRIX_3X2_F> _transformStack = new();

        public float Width  { get; }
        public float Height { get; }

        internal DirectXGraphics(IntPtr rt, DirectXRenderingSystem sys, float dipWidth, float dipHeight)
        {
            _rt     = rt;
            _sys    = sys;
            Width   = dipWidth;
            Height  = dipHeight;
        }

        public void FillRect(float x, float y, float w, float h, uint argbColor)
        {
            var rect  = ToRect(x, y, w, h);
            var color = Colors.ToColor(argbColor);
            IntPtr brush = D2D1Vtbl.RT_CreateSolidColorBrush(_rt, ref color);
            try   { D2D1Vtbl.RT_FillRectangle(_rt, ref rect, brush); }
            finally { D2D1Vtbl.Release(brush); }
        }

        public void DrawRect(float x, float y, float w, float h, uint argbColor, float strokeWidth = 1f)
        {
            var rect  = ToRect(x, y, w, h);
            var color = Colors.ToColor(argbColor);
            IntPtr brush = D2D1Vtbl.RT_CreateSolidColorBrush(_rt, ref color);
            try   { D2D1Vtbl.RT_DrawRectangle(_rt, ref rect, brush, strokeWidth); }
            finally { D2D1Vtbl.Release(brush); }
        }

        public void FillRoundedRect(float x, float y, float w, float h, float cornerRadius, uint argbColor)
        {
            var rr    = ToRoundedRect(x, y, w, h, cornerRadius);
            var color = Colors.ToColor(argbColor);
            IntPtr brush = D2D1Vtbl.RT_CreateSolidColorBrush(_rt, ref color);
            try   { D2D1Vtbl.RT_FillRoundedRectangle(_rt, ref rr, brush); }
            finally { D2D1Vtbl.Release(brush); }
        }

        public void DrawRoundedRect(float x, float y, float w, float h, float cornerRadius, uint argbColor, float strokeWidth = 1f)
        {
            var rr    = ToRoundedRect(x, y, w, h, cornerRadius);
            var color = Colors.ToColor(argbColor);
            IntPtr brush = D2D1Vtbl.RT_CreateSolidColorBrush(_rt, ref color);
            try   { D2D1Vtbl.RT_DrawRoundedRectangle(_rt, ref rr, brush, strokeWidth); }
            finally { D2D1Vtbl.Release(brush); }
        }

        public void DrawLine(float x1, float y1, float x2, float y2, uint argbColor, float strokeWidth = 1f)
        {
            var p0    = new D2D1_POINT_2F { x = x1, y = y1 };
            var p1    = new D2D1_POINT_2F { x = x2, y = y2 };
            var color = Colors.ToColor(argbColor);
            IntPtr brush = D2D1Vtbl.RT_CreateSolidColorBrush(_rt, ref color);
            try   { D2D1Vtbl.RT_DrawLine(_rt, p0, p1, brush, strokeWidth); }
            finally { D2D1Vtbl.Release(brush); }
        }

        public void DrawText(string text, string fontFace, float fontSize,
                             float x, float y, float maxWidth, float maxHeight, uint argbColor,
                             bool noWrap = false)
        {
            if (maxWidth < 0 || maxHeight < 0) return;

            IntPtr fmt = _sys.CreateTextFormat(fontFace, fontSize);
            try
            {
                if (noWrap) D2D1Vtbl.TextFmt_SetWordWrapping(fmt, DWRITE_WORD_WRAPPING.NO_WRAP);

                IntPtr layout = _sys.CreateWpfCompatibleLayout(fmt, text, maxWidth, maxHeight);
                try
                {
                    var origin = new D2D1_POINT_2F { x = x, y = y };
                    var color  = Colors.ToColor(argbColor);
                    IntPtr brush = D2D1Vtbl.RT_CreateSolidColorBrush(_rt, ref color);
                    try   { D2D1Vtbl.RT_DrawTextLayout(_rt, origin, layout, brush, D2D1_DRAW_TEXT_OPTIONS.NONE); }
                    finally { D2D1Vtbl.Release(brush); }
                }
                finally { D2D1Vtbl.Release(layout); }
            }
            finally { D2D1Vtbl.Release(fmt); }
        }

        public (float width, float height) MeasureText(string text, string fontFace, float fontSize,
                                                       float maxWidth, float maxHeight)
        {
            IntPtr fmt = _sys.CreateTextFormat(fontFace, fontSize);
            try
            {
                IntPtr layout = _sys.CreateWpfCompatibleLayout(fmt, text, maxWidth, maxHeight);
                try
                {
                    var m = D2D1Vtbl.Layout_GetMetrics(layout);
                    return (m.width, m.height);
                }
                finally { D2D1Vtbl.Release(layout); }
            }
            finally { D2D1Vtbl.Release(fmt); }
        }

        public void DrawBitmap(IntPtr scan0, int srcWidth, int srcHeight, int stride,
                               float dstX, float dstY, float dstW, float dstH)
        {
            var size  = new D2D1_SIZE_U { width = (uint)srcWidth, height = (uint)srcHeight };
            var props = new D2D1_BITMAP_PROPERTIES
            {
                pixelFormat = new D2D1_PIXEL_FORMAT { format = DXGI_FORMAT.B8G8R8A8_UNORM, alphaMode = D2D1_ALPHA_MODE.IGNORE },
                dpiX = _sys.DpiX,
                dpiY = _sys.DpiY,
            };
            var dstRect = new D2D1_RECT_F { left = dstX, top = dstY, right = dstX + dstW, bottom = dstY + dstH };
            IntPtr bitmap = D2D1Vtbl.RT_CreateBitmap(_rt, size, scan0, (uint)stride, ref props);
            try   { D2D1Vtbl.RT_DrawBitmap(_rt, bitmap, ref dstRect, 1.0f, D2D1_BITMAP_INTERPOLATION_MODE.NEAREST_NEIGHBOR); }
            finally { D2D1Vtbl.Release(bitmap); }
        }

        public void PushClip(float x, float y, float w, float h)
        {
            var rect = ToRect(x, y, w, h);
            D2D1Vtbl.RT_PushAxisAlignedClip(_rt, ref rect, D2D1_ANTIALIAS_MODE.PER_PRIMITIVE);
        }

        public void PopClip() => D2D1Vtbl.RT_PopAxisAlignedClip(_rt);

        public void PushTranslation(float dx, float dy)
        {
            var current = D2D1Vtbl.RT_GetTransform(_rt);
            _transformStack.Push(current);
            var translated = current;
            translated._31 += dx;
            translated._32 += dy;
            D2D1Vtbl.RT_SetTransform(_rt, ref translated);
        }

        public void PopTranslation()
        {
            var saved = _transformStack.Pop();
            D2D1Vtbl.RT_SetTransform(_rt, ref saved);
        }

        public void PushScale(float scaleX, float scaleY, float centerX, float centerY)
        {
            var cur = D2D1Vtbl.RT_GetTransform(_rt);
            _transformStack.Push(cur);
            // Convert center from local space to screen space, then scale around
            // that screen-space point: M = C * Scale_screen(sx, sy, cxScreen, cyScreen)
            float cxScreen = centerX * cur._11 + centerY * cur._21 + cur._31;
            float cyScreen = centerX * cur._12 + centerY * cur._22 + cur._32;
            var scaled = new D2D1_MATRIX_3X2_F
            {
                _11 = cur._11 * scaleX,
                _12 = cur._12 * scaleY,
                _21 = cur._21 * scaleX,
                _22 = cur._22 * scaleY,
                _31 = cur._31 * scaleX + cxScreen * (1 - scaleX),
                _32 = cur._32 * scaleY + cyScreen * (1 - scaleY),
            };
            D2D1Vtbl.RT_SetTransform(_rt, ref scaled);
        }

        public void PopScale()
        {
            var saved = _transformStack.Pop();
            D2D1Vtbl.RT_SetTransform(_rt, ref saved);
        }

        private static D2D1_RECT_F ToRect(float x, float y, float w, float h)
            => new D2D1_RECT_F { left = x, top = y, right = x + w, bottom = y + h };

        private static D2D1_ROUNDED_RECT ToRoundedRect(float x, float y, float w, float h, float r)
            => new D2D1_ROUNDED_RECT { rect = ToRect(x, y, w, h), radiusX = r, radiusY = r };
    }
}
