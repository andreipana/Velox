using System.Runtime.InteropServices;

namespace Velox.DirectX
{
    // ─── Enumerations ─────────────────────────────────────────────────────────

    internal enum D2D1_FACTORY_TYPE { SINGLE_THREADED = 0, MULTI_THREADED = 1 }
    internal enum D2D1_RENDER_TARGET_TYPE { DEFAULT = 0, SOFTWARE = 1, HARDWARE = 2 }
    internal enum DXGI_FORMAT { UNKNOWN = 0 }
    internal enum D2D1_ALPHA_MODE { UNKNOWN = 0, PREMULTIPLIED = 1, STRAIGHT = 2, IGNORE = 3 }
    internal enum D2D1_RENDER_TARGET_USAGE { NONE = 0 }
    internal enum D2D1_FEATURE_LEVEL { DEFAULT = 0 }
    internal enum D2D1_PRESENT_OPTIONS { NONE = 0, RETAIN_CONTENTS = 1, IMMEDIATELY = 2 }
    internal enum D2D1_TEXT_ANTIALIAS_MODE { DEFAULT = 0, CLEARTYPE = 1, GRAYSCALE = 2, ALIASED = 3 }
    internal enum D2D1_ANTIALIAS_MODE { PER_PRIMITIVE = 0, ALIASED = 1 }
    internal enum D2D1_DRAW_TEXT_OPTIONS { NONE = 0, NO_SNAP = 1, CLIP = 2 }

    internal enum DWRITE_FACTORY_TYPE { SHARED = 0, ISOLATED = 1 }
    internal enum DWRITE_FONT_WEIGHT { THIN = 100, LIGHT = 300, NORMAL = 400, BOLD = 700, BLACK = 900 }
    internal enum DWRITE_FONT_STYLE { NORMAL = 0, OBLIQUE = 1, ITALIC = 2 }
    internal enum DWRITE_FONT_STRETCH { NORMAL = 5 }
    internal enum DWRITE_TEXT_ALIGNMENT { LEADING = 0, TRAILING = 1, CENTER = 2 }
    internal enum DWRITE_PARAGRAPH_ALIGNMENT { NEAR = 0, FAR = 1, CENTER = 2 }
    internal enum DWRITE_WORD_WRAPPING { WRAP = 0, NO_WRAP = 1 }
    internal enum DWRITE_PIXEL_GEOMETRY { FLAT = 0, RGB = 1, BGR = 2 }
    internal enum DWRITE_RENDERING_MODE
    {
        DEFAULT = 0, ALIASED = 1, GDI_CLASSIC = 2, GDI_NATURAL = 3,
        NATURAL = 4, NATURAL_SYMMETRIC = 5, OUTLINE = 6,
    }

    // ─── Structures ───────────────────────────────────────────────────────────

    [StructLayout(LayoutKind.Sequential)]
    internal struct D2D1_COLOR_F
    {
        public float r, g, b, a;
        public D2D1_COLOR_F(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct D2D1_POINT_2F { public float x, y; }

    [StructLayout(LayoutKind.Sequential)]
    internal struct D2D1_RECT_F { public float left, top, right, bottom; }

    [StructLayout(LayoutKind.Sequential)]
    internal struct D2D1_SIZE_U { public uint width, height; }

    [StructLayout(LayoutKind.Sequential)]
    internal struct D2D1_PIXEL_FORMAT
    {
        public DXGI_FORMAT format;
        public D2D1_ALPHA_MODE alphaMode;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct D2D1_RENDER_TARGET_PROPERTIES
    {
        public D2D1_RENDER_TARGET_TYPE type;
        public D2D1_PIXEL_FORMAT pixelFormat;
        public float dpiX, dpiY;
        public D2D1_RENDER_TARGET_USAGE usage;
        public D2D1_FEATURE_LEVEL minLevel;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct D2D1_HWND_RENDER_TARGET_PROPERTIES
    {
        public IntPtr hwnd;
        public D2D1_SIZE_U pixelSize;
        public D2D1_PRESENT_OPTIONS presentOptions;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct D2D1_ROUNDED_RECT
    {
        public D2D1_RECT_F rect;
        public float radiusX, radiusY;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DWRITE_TEXT_METRICS
    {
        public float left, top, width, widthIncludingTrailingWhitespace, height;
        public float layoutWidth, layoutHeight;
        public uint maxBidiReorderingDepth, lineCount;
    }

    // ─── Factory P/Invokes ────────────────────────────────────────────────────

    internal static class D2D1Native
    {
        [DllImport("d2d1.dll")]
        internal static extern int D2D1CreateFactory(
            D2D1_FACTORY_TYPE factoryType,
            ref Guid riid,
            IntPtr pFactoryOptions,
            out IntPtr ppIFactory);

        [DllImport("dwrite.dll")]
        internal static extern int DWriteCreateFactory(
            DWRITE_FACTORY_TYPE factoryType,
            ref Guid riid,
            out IntPtr ppFactory);
    }

    // ─── Vtable Dispatch ──────────────────────────────────────────────────────
    //
    // Direct2D COM objects do NOT support QueryInterface for their derived
    // interface IIDs (only IUnknown). We therefore bypass the CLR's managed COM
    // interop (which always forces a QI) and call every method directly through
    // the raw vtable pointer. All COM objects are stored as IntPtr.
    //
    // Slot numbers include IUnknown (0=QI, 1=AddRef, 2=Release) so the first
    // custom method is always at slot 3.

    internal static unsafe class D2D1Vtbl
    {
        // Get the vtable from any COM object pointer.
        private static void** V(IntPtr p) => *(void***)p;

        public static void Release(IntPtr p)
        {
            if (p != IntPtr.Zero)
                ((delegate* unmanaged[Stdcall]<IntPtr, uint>)V(p)[2])(p);
        }

        // ── ID2D1Factory ────────────────────────────────────────────────────
        // Slot 4: GetDesktopDpi
        public static void Factory_GetDesktopDpi(IntPtr f, out float dpiX, out float dpiY)
        {
            float x, y;
            ((delegate* unmanaged[Stdcall]<IntPtr, float*, float*, void>)V(f)[4])(f, &x, &y);
            dpiX = x; dpiY = y;
        }

        // Slot 14: CreateHwndRenderTarget
        public static IntPtr Factory_CreateHwndRenderTarget(
            IntPtr f,
            ref D2D1_RENDER_TARGET_PROPERTIES rtProps,
            ref D2D1_HWND_RENDER_TARGET_PROPERTIES hwndProps)
        {
            IntPtr rt;
            fixed (D2D1_RENDER_TARGET_PROPERTIES* pRt = &rtProps)
            fixed (D2D1_HWND_RENDER_TARGET_PROPERTIES* pH = &hwndProps)
                ((delegate* unmanaged[Stdcall]<IntPtr, D2D1_RENDER_TARGET_PROPERTIES*, D2D1_HWND_RENDER_TARGET_PROPERTIES*, IntPtr*, int>)
                    V(f)[14])(f, pRt, pH, &rt);
            return rt;
        }

        // ── ID2D1HwndRenderTarget ────────────────────────────────────────────
        // Slot 8: CreateSolidColorBrush
        public static IntPtr RT_CreateSolidColorBrush(IntPtr rt, ref D2D1_COLOR_F color)
        {
            IntPtr brush;
            fixed (D2D1_COLOR_F* p = &color)
                ((delegate* unmanaged[Stdcall]<IntPtr, D2D1_COLOR_F*, IntPtr, IntPtr*, int>)
                    V(rt)[8])(rt, p, IntPtr.Zero, &brush);
            return brush;
        }

        // Slot 15: DrawLine
        public static void RT_DrawLine(IntPtr rt, D2D1_POINT_2F p0, D2D1_POINT_2F p1,
                                       IntPtr brush, float strokeWidth)
            => ((delegate* unmanaged[Stdcall]<IntPtr, D2D1_POINT_2F, D2D1_POINT_2F, IntPtr, float, IntPtr, void>)
                    V(rt)[15])(rt, p0, p1, brush, strokeWidth, IntPtr.Zero);

        // Slot 16: DrawRectangle
        public static void RT_DrawRectangle(IntPtr rt, ref D2D1_RECT_F rect,
                                            IntPtr brush, float strokeWidth)
        {
            fixed (D2D1_RECT_F* p = &rect)
                ((delegate* unmanaged[Stdcall]<IntPtr, D2D1_RECT_F*, IntPtr, float, IntPtr, void>)
                    V(rt)[16])(rt, p, brush, strokeWidth, IntPtr.Zero);
        }

        // Slot 17: FillRectangle
        public static void RT_FillRectangle(IntPtr rt, ref D2D1_RECT_F rect, IntPtr brush)
        {
            fixed (D2D1_RECT_F* p = &rect)
                ((delegate* unmanaged[Stdcall]<IntPtr, D2D1_RECT_F*, IntPtr, void>)
                    V(rt)[17])(rt, p, brush);
        }

        // Slot 18: DrawRoundedRectangle
        public static void RT_DrawRoundedRectangle(IntPtr rt, ref D2D1_ROUNDED_RECT rr,
                                                   IntPtr brush, float strokeWidth)
        {
            fixed (D2D1_ROUNDED_RECT* p = &rr)
                ((delegate* unmanaged[Stdcall]<IntPtr, D2D1_ROUNDED_RECT*, IntPtr, float, IntPtr, void>)
                    V(rt)[18])(rt, p, brush, strokeWidth, IntPtr.Zero);
        }

        // Slot 19: FillRoundedRectangle
        public static void RT_FillRoundedRectangle(IntPtr rt, ref D2D1_ROUNDED_RECT rr, IntPtr brush)
        {
            fixed (D2D1_ROUNDED_RECT* p = &rr)
                ((delegate* unmanaged[Stdcall]<IntPtr, D2D1_ROUNDED_RECT*, IntPtr, void>)
                    V(rt)[19])(rt, p, brush);
        }

        // Slot 28: DrawTextLayout
        public static void RT_DrawTextLayout(IntPtr rt, D2D1_POINT_2F origin,
                                             IntPtr layout, IntPtr brush,
                                             D2D1_DRAW_TEXT_OPTIONS options)
            => ((delegate* unmanaged[Stdcall]<IntPtr, D2D1_POINT_2F, IntPtr, IntPtr, D2D1_DRAW_TEXT_OPTIONS, void>)
                    V(rt)[28])(rt, origin, layout, brush, options);

        // Slot 34: SetTextAntialiasMode
        public static void RT_SetTextAntialiasMode(IntPtr rt, D2D1_TEXT_ANTIALIAS_MODE mode)
            => ((delegate* unmanaged[Stdcall]<IntPtr, D2D1_TEXT_ANTIALIAS_MODE, void>)V(rt)[34])(rt, mode);

        // Slot 36: SetTextRenderingParams
        public static void RT_SetTextRenderingParams(IntPtr rt, IntPtr renderingParams)
            => ((delegate* unmanaged[Stdcall]<IntPtr, IntPtr, void>)V(rt)[36])(rt, renderingParams);

        // Slot 45: PushAxisAlignedClip
        public static void RT_PushAxisAlignedClip(IntPtr rt, ref D2D1_RECT_F rect,
                                                  D2D1_ANTIALIAS_MODE mode)
        {
            fixed (D2D1_RECT_F* p = &rect)
                ((delegate* unmanaged[Stdcall]<IntPtr, D2D1_RECT_F*, D2D1_ANTIALIAS_MODE, void>)
                    V(rt)[45])(rt, p, mode);
        }

        // Slot 46: PopAxisAlignedClip
        public static void RT_PopAxisAlignedClip(IntPtr rt)
            => ((delegate* unmanaged[Stdcall]<IntPtr, void>)V(rt)[46])(rt);

        // Slot 47: Clear
        public static void RT_Clear(IntPtr rt, ref D2D1_COLOR_F color)
        {
            fixed (D2D1_COLOR_F* p = &color)
                ((delegate* unmanaged[Stdcall]<IntPtr, D2D1_COLOR_F*, void>)V(rt)[47])(rt, p);
        }

        // Slot 48: BeginDraw
        public static void RT_BeginDraw(IntPtr rt)
            => ((delegate* unmanaged[Stdcall]<IntPtr, void>)V(rt)[48])(rt);

        // Slot 49: EndDraw
        public static int RT_EndDraw(IntPtr rt)
        {
            ulong t1, t2;
            return ((delegate* unmanaged[Stdcall]<IntPtr, ulong*, ulong*, int>)V(rt)[49])(rt, &t1, &t2);
        }

        // Slot 58: Resize
        public static void RT_Resize(IntPtr rt, ref D2D1_SIZE_U size)
        {
            fixed (D2D1_SIZE_U* p = &size)
                ((delegate* unmanaged[Stdcall]<IntPtr, D2D1_SIZE_U*, int>)V(rt)[58])(rt, p);
        }

        // ── IDWriteFactory ───────────────────────────────────────────────────
        // Slot 12: CreateCustomRenderingParams
        public static IntPtr DWrite_CreateCustomRenderingParams(
            IntPtr f, float gamma, float enhancedContrast, float clearTypeLevel,
            DWRITE_PIXEL_GEOMETRY pixelGeometry, DWRITE_RENDERING_MODE renderingMode)
        {
            IntPtr p;
            ((delegate* unmanaged[Stdcall]<IntPtr, float, float, float, DWRITE_PIXEL_GEOMETRY, DWRITE_RENDERING_MODE, IntPtr*, int>)
                V(f)[12])(f, gamma, enhancedContrast, clearTypeLevel, pixelGeometry, renderingMode, &p);
            return p;
        }

        // Slot 15: CreateTextFormat
        public static IntPtr DWrite_CreateTextFormat(
            IntPtr f, string fontFamily,
            DWRITE_FONT_WEIGHT weight, DWRITE_FONT_STYLE style,
            DWRITE_FONT_STRETCH stretch, float fontSize)
        {
            IntPtr fmt;
            fixed (char* pFamily = fontFamily)
            fixed (char* pLocale = "")
                ((delegate* unmanaged[Stdcall]<IntPtr, char*, IntPtr, DWRITE_FONT_WEIGHT, DWRITE_FONT_STYLE, DWRITE_FONT_STRETCH, float, char*, IntPtr*, int>)
                    V(f)[15])(f, pFamily, IntPtr.Zero, weight, style, stretch, fontSize, pLocale, &fmt);
            return fmt;
        }

        // Slot 19: CreateGdiCompatibleTextLayout
        public static IntPtr DWrite_CreateGdiCompatibleTextLayout(
            IntPtr f, string text, IntPtr textFormat,
            float layoutWidth, float layoutHeight, float pixelsPerDip)
        {
            IntPtr layout;
            fixed (char* pText = text)
                ((delegate* unmanaged[Stdcall]<IntPtr, char*, uint, IntPtr, float, float, float, IntPtr, int, IntPtr*, int>)
                    V(f)[19])(f, pText, (uint)text.Length, textFormat,
                               layoutWidth, layoutHeight, pixelsPerDip,
                               IntPtr.Zero, 1 /* TRUE = useGdiNatural */, &layout);
            return layout;
        }

        // ── IDWriteTextFormat ────────────────────────────────────────────────
        // Slot 3: SetTextAlignment
        public static void TextFmt_SetTextAlignment(IntPtr fmt, DWRITE_TEXT_ALIGNMENT alignment)
            => ((delegate* unmanaged[Stdcall]<IntPtr, DWRITE_TEXT_ALIGNMENT, int>)V(fmt)[3])(fmt, alignment);

        // Slot 4: SetParagraphAlignment
        public static void TextFmt_SetParagraphAlignment(IntPtr fmt, DWRITE_PARAGRAPH_ALIGNMENT alignment)
            => ((delegate* unmanaged[Stdcall]<IntPtr, DWRITE_PARAGRAPH_ALIGNMENT, int>)V(fmt)[4])(fmt, alignment);

        // Slot 5: SetWordWrapping
        public static void TextFmt_SetWordWrapping(IntPtr fmt, DWRITE_WORD_WRAPPING wrapping)
            => ((delegate* unmanaged[Stdcall]<IntPtr, DWRITE_WORD_WRAPPING, int>)V(fmt)[5])(fmt, wrapping);

        // ── IDWriteTextLayout ────────────────────────────────────────────────
        // Slot 60: GetMetrics
        public static DWRITE_TEXT_METRICS Layout_GetMetrics(IntPtr layout)
        {
            DWRITE_TEXT_METRICS m;
            ((delegate* unmanaged[Stdcall]<IntPtr, DWRITE_TEXT_METRICS*, int>)V(layout)[60])(layout, &m);
            return m;
        }
    }
}
