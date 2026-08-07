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
        {
            IntPtr decoder = WicVtbl.Factory_CreateDecoderFromFilename(
                Wic.Factory, filePath, WicNative.GENERIC_READ, WICDecodeOptions.MetadataCacheOnDemand);
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
