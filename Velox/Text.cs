namespace Velox
{
    public enum SizeMode
    {
        /// <summary>Width and Height are explicit bounds — text wraps and clips to fit.</summary>
        Fixed,
        /// <summary>Size is computed from text content. Width and Height are outputs, not inputs.</summary>
        Auto,
        /// <summary>Parent sets Width/Height as maximum available constraints before calling Render.</summary>
        Fill,
    }

    internal class TextBlock
    {
        public string Text       { get; set; } = "";
        public uint   Color      { get; set; } = 0xFFFFFFFFU;
        public uint?  Background { get; set; }
        public float  X          { get; set; }
        public float  Y          { get; set; }
        public float  Width      { get; set; }
        public float  Height     { get; set; }
        public SizeMode SizeMode { get; set; } = SizeMode.Auto;
        public float ActualWidth  { get; private set; }
        public float ActualHeight { get; private set; }

        public void Render(IGraphics graphics)
        {
            float maxW = SizeMode == SizeMode.Auto ? float.PositiveInfinity : Width;
            float maxH = SizeMode == SizeMode.Auto ? float.PositiveInfinity : Height;

            var (tw, th) = graphics.MeasureText(Text, "Cascadia Code", 14f, maxW, maxH);
            ActualWidth  = tw;
            ActualHeight = th;

            if (Background.HasValue)
                graphics.FillRect(X, Y, ActualWidth, ActualHeight, Background.Value);

            graphics.DrawText(Text, "Cascadia Code", 14f, X, Y, maxW, maxH, Color,
                noWrap: SizeMode == SizeMode.Fixed);
        }
    }
}
