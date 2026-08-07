using System.Runtime.InteropServices;

namespace Velox.DirectX
{
    // ─── Enumerations ─────────────────────────────────────────────────────────

    internal enum WICDecodeOptions { MetadataCacheOnDemand = 0, MetadataCacheOnLoad = 1 }
    internal enum WICBitmapDitherType { None = 0 }
    internal enum WICBitmapPaletteType { Custom = 0 }

    // ─── Structures ───────────────────────────────────────────────────────────

    [StructLayout(LayoutKind.Sequential)]
    internal struct WICRect { public int X, Y, Width, Height; }

    // ─── GUIDs ────────────────────────────────────────────────────────────────

    internal static class WicGuids
    {
        public static readonly Guid CLSID_WICImagingFactory       = new("cacaf262-9370-4615-a13b-9f5539da4c0a");
        public static readonly Guid IID_IWICImagingFactory        = new("ec5ec8a9-c395-4314-9c77-54d7a935ff70");
        public static readonly Guid GUID_WICPixelFormat32bppPBGRA = new("6fddc324-4e03-4bfe-b185-3d77768dc910");
    }

    // ─── Native P/Invokes ─────────────────────────────────────────────────────

    internal static class WicNative
    {
        [DllImport("ole32.dll")]
        internal static extern int CoCreateInstance(
            ref Guid rclsid, IntPtr pUnkOuter, uint dwClsContext, ref Guid riid, out IntPtr ppv);

        internal const uint CLSCTX_INPROC_SERVER = 0x1;
        internal const uint GENERIC_READ = 0x80000000;
    }

    // ─── Vtable Dispatch ──────────────────────────────────────────────────────
    //
    // Same rationale as D2D1Vtbl: WIC COM objects are called directly through
    // their raw vtable pointers rather than via the CLR's managed COM interop.
    // Slot numbers include IUnknown (0=QI, 1=AddRef, 2=Release) so the first
    // custom method is always at slot 3.

    internal static unsafe class WicVtbl
    {
        private static void** V(IntPtr p) => *(void***)p;

        public static void Release(IntPtr p)
        {
            if (p != IntPtr.Zero)
                ((delegate* unmanaged[Stdcall]<IntPtr, uint>)V(p)[2])(p);
        }

        // ── IWICImagingFactory ──────────────────────────────────────────────
        // Slot 3: CreateDecoderFromFilename
        public static IntPtr Factory_CreateDecoderFromFilename(IntPtr factory, string filename,
                                                                uint desiredAccess, WICDecodeOptions options)
        {
            IntPtr decoder;
            int hr;
            fixed (char* pFilename = filename)
                hr = ((delegate* unmanaged[Stdcall]<IntPtr, char*, Guid*, uint, WICDecodeOptions, IntPtr*, int>)
                        V(factory)[3])(factory, pFilename, null, desiredAccess, options, &decoder);
            ThrowIfFailed(hr, nameof(Factory_CreateDecoderFromFilename));
            return decoder;
        }

        // Slot 10: CreateFormatConverter
        public static IntPtr Factory_CreateFormatConverter(IntPtr factory)
        {
            IntPtr converter;
            int hr = ((delegate* unmanaged[Stdcall]<IntPtr, IntPtr*, int>)V(factory)[10])(factory, &converter);
            ThrowIfFailed(hr, nameof(Factory_CreateFormatConverter));
            return converter;
        }

        // ── IWICBitmapDecoder ────────────────────────────────────────────────
        // Slot 13: GetFrame
        public static IntPtr Decoder_GetFrame(IntPtr decoder, uint index)
        {
            IntPtr frame;
            int hr = ((delegate* unmanaged[Stdcall]<IntPtr, uint, IntPtr*, int>)V(decoder)[13])(decoder, index, &frame);
            ThrowIfFailed(hr, nameof(Decoder_GetFrame));
            return frame;
        }

        // ── IWICBitmapSource ─────────────────────────────────────────────────
        // (shared layout for IWICBitmapFrameDecode and IWICFormatConverter, both
        // of which derive from IWICBitmapSource with GetSize/CopyPixels at the
        // same slots)
        // Slot 3: GetSize
        public static void Source_GetSize(IntPtr source, out uint width, out uint height)
        {
            uint w, h;
            int hr = ((delegate* unmanaged[Stdcall]<IntPtr, uint*, uint*, int>)V(source)[3])(source, &w, &h);
            ThrowIfFailed(hr, nameof(Source_GetSize));
            width = w; height = h;
        }

        // Slot 7: CopyPixels
        public static void Source_CopyPixels(IntPtr source, uint stride, byte[] buffer)
        {
            int hr;
            fixed (byte* pBuffer = buffer)
                hr = ((delegate* unmanaged[Stdcall]<IntPtr, WICRect*, uint, uint, byte*, int>)
                        V(source)[7])(source, null, stride, (uint)buffer.Length, pBuffer);
            ThrowIfFailed(hr, nameof(Source_CopyPixels));
        }

        // ── IWICFormatConverter ──────────────────────────────────────────────
        // Slot 8: Initialize
        public static void Converter_Initialize(IntPtr converter, IntPtr source, ref Guid dstFormat,
                                                 WICBitmapDitherType dither, WICBitmapPaletteType paletteTranslate)
        {
            int hr;
            fixed (Guid* pFormat = &dstFormat)
                hr = ((delegate* unmanaged[Stdcall]<IntPtr, IntPtr, Guid*, WICBitmapDitherType, IntPtr, double, WICBitmapPaletteType, int>)
                        V(converter)[8])(converter, source, pFormat, dither, IntPtr.Zero, 0.0, paletteTranslate);
            ThrowIfFailed(hr, nameof(Converter_Initialize));
        }

        private static void ThrowIfFailed(int hr, string method)
        {
            if (hr < 0)
                throw new COMException($"WIC call failed: {method}", hr);
        }
    }

    // ─── Factory Singleton ────────────────────────────────────────────────────

    internal static class Wic
    {
        private static IntPtr s_factory;

        public static IntPtr Factory
        {
            get
            {
                if (s_factory == IntPtr.Zero)
                {
                    var clsid = WicGuids.CLSID_WICImagingFactory;
                    var iid   = WicGuids.IID_IWICImagingFactory;
                    int hr = WicNative.CoCreateInstance(ref clsid, IntPtr.Zero, WicNative.CLSCTX_INPROC_SERVER, ref iid, out s_factory);
                    if (hr < 0)
                        throw new COMException("Failed to create WIC imaging factory — was CoInitializeEx called on this thread?", hr);
                }
                return s_factory;
            }
        }
    }
}
