namespace Velox.Controls
{
    public class TextBlock : Control
    {
        public string Text       { get; set; } = "";
        public string FontFace   { get; set; } = "Segoe UI Variable Text";
        public float  FontSize   { get; set; } = 14f;
        public uint   TextColor  { get; set; } = 0xFFFFFFFFU;
        public uint?  Background { get; set; }

        public float ActualWidth  { get; private set; }
        public float ActualHeight { get; private set; }

        protected override void OnRender(Velox.IGraphics graphics)
        {
            float maxW = Width  > 0 ? Width  : float.PositiveInfinity;
            float maxH = Height > 0 ? Height : float.PositiveInfinity;

            var (tw, th) = graphics.MeasureText(Text, FontFace, FontSize, maxW, maxH);
            ActualWidth  = tw;
            ActualHeight = th;

            if (Background.HasValue)
                graphics.FillRect(0, 0, tw, th, Background.Value);

            graphics.DrawText(Text, FontFace, FontSize, 0, 0, maxW, maxH, TextColor);
        }
    }
}
