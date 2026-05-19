namespace Velox.DirectX
{
    public class DirectXRenderingSystem : IDisposable
    {
        internal IntPtr D2dFactory;
        internal IntPtr DWriteFactory;

        public float DpiX { get; }
        public float DpiY { get; }

        public DirectXRenderingSystem()
        {
            var d2dIid = new Guid("06152247-6f50-465a-9245-118bfd3b6007");
            D2D1Native.D2D1CreateFactory(D2D1_FACTORY_TYPE.SINGLE_THREADED, ref d2dIid, IntPtr.Zero, out D2dFactory);

            var dwIid = new Guid("b859ee5a-d838-4b5b-a2e8-1adc7d93db48");
            D2D1Native.DWriteCreateFactory(DWRITE_FACTORY_TYPE.SHARED, ref dwIid, out DWriteFactory);

            D2D1Vtbl.Factory_GetDesktopDpi(D2dFactory, out float dpiX, out float dpiY);
            DpiX = dpiX;
            DpiY = dpiY;
        }

        internal IntPtr CreateTextFormat(string fontFace, float size,
            DWRITE_FONT_WEIGHT fontWeight = DWRITE_FONT_WEIGHT.NORMAL,
            DWRITE_FONT_STYLE  fontStyle  = DWRITE_FONT_STYLE.NORMAL)
        {
            IntPtr fmt = D2D1Vtbl.DWrite_CreateTextFormat(
                DWriteFactory, fontFace, fontWeight, fontStyle, DWRITE_FONT_STRETCH.NORMAL, size);
            D2D1Vtbl.TextFmt_SetTextAlignment(fmt, DWRITE_TEXT_ALIGNMENT.LEADING);
            D2D1Vtbl.TextFmt_SetParagraphAlignment(fmt, DWRITE_PARAGRAPH_ALIGNMENT.NEAR);
            return fmt;
        }

        // Creates a GDI-compatible text layout — the key to matching WPF's
        // crisp rendering. This forces glyph advances to snap to pixel
        // boundaries during measurement, exactly as WPF does internally.
        internal IntPtr CreateWpfCompatibleLayout(IntPtr textFormat, string text, float maxWidth, float maxHeight)
        {
            float pixelsPerDip = DpiY / 96.0f;
            return D2D1Vtbl.DWrite_CreateGdiCompatibleTextLayout(
                DWriteFactory, text, textFormat, maxWidth, maxHeight, pixelsPerDip);
        }

        public void Dispose()
        {
            D2D1Vtbl.Release(D2dFactory);
            D2D1Vtbl.Release(DWriteFactory);
            D2dFactory = DWriteFactory = IntPtr.Zero;
        }
    }
}
