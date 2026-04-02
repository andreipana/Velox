using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Velox
{
    public class DirectXRenderingSystem
    {
        public SharpDX.DirectWrite.Factory DirectWriteFactory { get; }

        public SharpDX.Direct2D1.Factory1 Direct2DFactory { get; }

        public float DpiX { get; }

        public float DpiY { get; }

        public DirectXRenderingSystem()
        {
            Direct2DFactory = new SharpDX.Direct2D1.Factory1(SharpDX.Direct2D1.FactoryType.SingleThreaded);

            DirectWriteFactory = new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Shared);

            var ddpi = Direct2DFactory.DesktopDpi;
            DpiX = ddpi.Width;
            DpiY = ddpi.Height;
        }

        internal TextFormat CreateTextFormat(string fontFace, float size, FontWeight fontWeight = FontWeight.Normal, FontStyle fontStyle = FontStyle.Normal)
        {
            return new TextFormat(DirectWriteFactory, fontFace, fontWeight, fontStyle, size)
            {
                TextAlignment = TextAlignment.Leading,
                ParagraphAlignment = ParagraphAlignment.Near,
                //WordWrapping = WordWrapping.NoWrap
            };
        }

        /// <summary>
        /// Creates a GDI-compatible text layout — the key to matching WPF's
        /// crisp rendering. This forces glyph advances to snap to pixel
        /// boundaries during measurement, exactly as WPF does internally.
        /// </summary>
        public TextLayout CreateWpfCompatibleLayout(TextFormat textFormat, string text, float maxWidth, float maxHeight)
        {
            float pixelsPerDip = DpiY / 96.0f;

            // CreateGdiCompatibleTextLayout snaps glyph advances to whole pixels,
            // which is the single biggest reason WPF text looks sharper than
            // naive Direct2D text rendering.
            return new TextLayout(
                DirectWriteFactory,
                text,
                textFormat,
                maxWidth,
                maxHeight,
                pixelsPerDip,
                null,       // no additional transform
                true        // useGdiNatural — matches WPF's measuring mode
            );
        }
    }
}
