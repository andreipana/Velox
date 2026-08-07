namespace Velox.Controls
{
    public class Image : Control
    {
        public Velox.Bitmap? Source { get; set; }

        private Velox.Bitmap?        _cachedSourceRef;
        private Velox.INativeBitmap? _nativeBitmap;

        public override (float desiredW, float desiredH) Measure(Velox.IGraphics graphics, float availableW, float availableH)
        {
            if (Source == null) return (0, 0);
            float w = Width  > 0 ? Width  : Source.PixelWidth;
            float h = Height > 0 ? Height : Source.PixelHeight;
            return (w, h);
        }

        protected override void OnRender(Velox.IGraphics graphics)
        {
            if (Source == null) return;

            if (!ReferenceEquals(_cachedSourceRef, Source))
            {
                _nativeBitmap?.Dispose();
                _nativeBitmap = graphics.CreateBitmap(Source.Pixels, Source.PixelWidth, Source.PixelHeight, Source.Stride);
                _cachedSourceRef = Source;
            }

            float w = Width  > 0 ? Width  : Source.PixelWidth;
            float h = Height > 0 ? Height : Source.PixelHeight;
            graphics.DrawBitmap(_nativeBitmap!, 0, 0, w, h);
        }
    }
}
