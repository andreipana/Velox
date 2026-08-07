namespace Velox.Controls
{
    public class Image : Control
    {
        private Velox.Bitmap? _source;
        public Velox.Bitmap? Source
        {
            get => _source;
            set
            {
                if (ReferenceEquals(_source, value)) return;
                _nativeBitmap?.Dispose();
                _nativeBitmap = null;
                _source = value;
            }
        }

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

            _nativeBitmap ??= graphics.CreateBitmap(Source.Pixels, Source.PixelWidth, Source.PixelHeight, Source.Stride);

            float w = Width  > 0 ? Width  : Source.PixelWidth;
            float h = Height > 0 ? Height : Source.PixelHeight;
            graphics.DrawBitmap(_nativeBitmap, 0, 0, w, h);
        }
    }
}
