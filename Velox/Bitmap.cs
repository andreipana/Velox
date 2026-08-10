using Velox.DirectX;

namespace Velox
{
    // Decodes an image file into a 32bpp BGRA, premultiplied-alpha, top-down
    // pixel buffer via WIC — the pixel layout ID2D1Bitmap expects directly.
    public sealed class Bitmap
    {
        public int PixelWidth  { get; }
        public int PixelHeight { get; }
        public int Stride      { get; }
        public byte[] Pixels   { get; }

        public Bitmap(string filePath)
            : this(WicVtbl.Factory_CreateDecoderFromFilename(
                Wic.Factory, filePath, WicNative.GENERIC_READ, WICDecodeOptions.MetadataCacheOnDemand))
        {
        }

        // Decodes from an arbitrary stream (e.g. Assembly.GetManifestResourceStream
        // for a PNG/JPEG embedded as a resource) via a hand-rolled IStream COM
        // wrapper (ManagedIStream) instead of WIC's filename-based decoder.
        public Bitmap(Stream stream) : this(CreateDecoderFromStream(stream))
        {
        }

        private static IntPtr CreateDecoderFromStream(Stream stream)
        {
            using var ms = new MemoryStream();
            stream.CopyTo(ms);

            IntPtr comStream = ManagedIStream.Create(ms.ToArray());
            try
            {
                // The decoder AddRefs its own reference to the stream, so
                // releasing ours immediately after this call is correct.
                return WicVtbl.Factory_CreateDecoderFromStream(Wic.Factory, comStream, WICDecodeOptions.MetadataCacheOnDemand);
            }
            finally { WicVtbl.Release(comStream); }
        }

        private Bitmap(IntPtr decoder)
        {
            try
            {
                IntPtr frame = WicVtbl.Decoder_GetFrame(decoder, 0);
                try
                {
                    IntPtr converter = WicVtbl.Factory_CreateFormatConverter(Wic.Factory);
                    try
                    {
                        Guid pbgra = WicGuids.GUID_WICPixelFormat32bppPBGRA;
                        WicVtbl.Converter_Initialize(converter, frame, ref pbgra,
                            WICBitmapDitherType.None, WICBitmapPaletteType.Custom);

                        WicVtbl.Source_GetSize(converter, out uint width, out uint height);
                        PixelWidth  = (int)width;
                        PixelHeight = (int)height;
                        Stride      = PixelWidth * 4;

                        Pixels = new byte[Stride * PixelHeight];
                        WicVtbl.Source_CopyPixels(converter, (uint)Stride, Pixels);
                    }
                    finally { WicVtbl.Release(converter); }
                }
                finally { WicVtbl.Release(frame); }
            }
            finally { WicVtbl.Release(decoder); }
        }
    }
}
