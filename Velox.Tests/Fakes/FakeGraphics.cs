namespace Velox.Tests.Fakes
{
    // IGraphics test double: records every call as a DrawCall and measures text
    // deterministically (CharWidth per character, LineHeight per line) so layout
    // tests don't depend on fonts or a GPU.
    public sealed class FakeGraphics : IGraphics
    {
        public const float CharWidth  = 8f;
        public const float LineHeight = 16f;

        public readonly record struct DrawCall(string Name, object[] Args);

        private readonly List<DrawCall> _calls = new();
        private readonly Stack<(float dx, float dy)> _translations = new();

        public FakeGraphics(float width = 800f, float height = 600f)
        {
            Width = width;
            Height = height;
        }

        public float Width  { get; }
        public float Height { get; }

        public IReadOnlyList<DrawCall> Calls => _calls;

        // Accumulated translation of the currently open PushTranslation scopes.
        public (float x, float y) CurrentOffset
            => (_translations.Sum(t => t.dx), _translations.Sum(t => t.dy));

        public int ClipDepth { get; private set; }
        public int TranslationDepth => _translations.Count;
        public int ScaleDepth { get; private set; }

        public IEnumerable<DrawCall> CallsNamed(string name) => _calls.Where(c => c.Name == name);

        private void Record(string name, params object[] args) => _calls.Add(new DrawCall(name, args));

        public void FillRect(float x, float y, float w, float h, uint argbColor)
            => Record(nameof(FillRect), x, y, w, h, argbColor);
        public void DrawRect(float x, float y, float w, float h, uint argbColor, float strokeWidth = 1f)
            => Record(nameof(DrawRect), x, y, w, h, argbColor, strokeWidth);
        public void FillRoundedRect(float x, float y, float w, float h, float cornerRadius, uint argbColor)
            => Record(nameof(FillRoundedRect), x, y, w, h, cornerRadius, argbColor);
        public void DrawRoundedRect(float x, float y, float w, float h, float cornerRadius, uint argbColor, float strokeWidth = 1f)
            => Record(nameof(DrawRoundedRect), x, y, w, h, cornerRadius, argbColor, strokeWidth);
        public void DrawLine(float x1, float y1, float x2, float y2, uint argbColor, float strokeWidth = 1f)
            => Record(nameof(DrawLine), x1, y1, x2, y2, argbColor, strokeWidth);

        public void DrawText(string text, string fontFace, float fontSize,
                             float x, float y, float maxWidth, float maxHeight, uint argbColor,
                             bool noWrap = false)
            => Record(nameof(DrawText), text, fontFace, fontSize, x, y, maxWidth, maxHeight, argbColor, noWrap);

        public (float width, float height) MeasureText(string text, string fontFace, float fontSize,
                                                       float maxWidth, float maxHeight)
        {
            float full = text.Length * CharWidth;
            if (maxWidth <= 0 || full <= maxWidth) return (full, LineHeight);
            int lines = (int)MathF.Ceiling(full / maxWidth);
            return (maxWidth, lines * LineHeight);
        }

        public INativeBitmap CreateBitmap(ReadOnlySpan<byte> pixels, int width, int height, int stride)
            => new FakeBitmap(width, height);

        public void DrawBitmap(INativeBitmap bitmap, float dstX, float dstY, float dstW, float dstH, bool interpolated = true)
            => Record(nameof(DrawBitmap), bitmap, dstX, dstY, dstW, dstH, interpolated);

        public void DrawBitmap(IntPtr scan0, int srcWidth, int srcHeight, int stride,
                               float dstX, float dstY, float dstW, float dstH, bool interpolated = true)
            => Record(nameof(DrawBitmap), srcWidth, srcHeight, stride, dstX, dstY, dstW, dstH, interpolated);

        public void PushClip(float x, float y, float w, float h)
        {
            ClipDepth++;
            Record(nameof(PushClip), x, y, w, h);
        }

        public void PopClip()
        {
            if (ClipDepth == 0) throw new InvalidOperationException("PopClip without matching PushClip.");
            ClipDepth--;
            Record(nameof(PopClip));
        }

        public void PushTranslation(float dx, float dy)
        {
            _translations.Push((dx, dy));
            Record(nameof(PushTranslation), dx, dy);
        }

        public void PopTranslation()
        {
            if (_translations.Count == 0) throw new InvalidOperationException("PopTranslation without matching PushTranslation.");
            _translations.Pop();
            Record(nameof(PopTranslation));
        }

        public void PushScale(float scaleX, float scaleY, float centerX, float centerY)
        {
            ScaleDepth++;
            Record(nameof(PushScale), scaleX, scaleY, centerX, centerY);
        }

        public void PopScale()
        {
            if (ScaleDepth == 0) throw new InvalidOperationException("PopScale without matching PushScale.");
            ScaleDepth--;
            Record(nameof(PopScale));
        }

        private sealed class FakeBitmap(int width, int height) : INativeBitmap
        {
            public int Width  { get; } = width;
            public int Height { get; } = height;
            public void Dispose() { }
        }
    }
}
