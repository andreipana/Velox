namespace Velox.Controls
{
    public class Glyph : Control
    {
        public string Symbol       { get; set; } = "";
        public string FontFace     { get; set; } = "Segoe Fluent Icons";
        public float  FontSize     { get; set; } = 16f;
        public uint   Color        { get; set; } = 0xFFCCCCCCU;
        public bool   FlipVertical { get; set; }

        public override (float desiredW, float desiredH) Measure(IGraphics g, float availableW, float availableH)
        {
            if (string.IsNullOrEmpty(Symbol)) return (0, 0);
            return g.MeasureText(Symbol, FontFace, FontSize, 9999f, 9999f);
        }

        protected override void OnRender(IGraphics g)
        {
            if (string.IsNullOrEmpty(Symbol)) return;
            float maxW = Width > 0 ? Width : 9999f;
            float maxH = Height > 0 ? Height : 9999f;
            var (tw, th) = g.MeasureText(Symbol, FontFace, FontSize, 9999f, 9999f);
            if (FlipVertical)
            {
                // PushScale(-1,-1) is a 180° rotation (det=+1, orientation-preserving).
                // D2D's GDI text renderer requires this — PushScale(1,-1) has det=-1 and
                // produces invisible glyphs.
                g.PushScale(1f, -1f, tw / 2f, th / 2f);
                g.DrawText(Symbol, FontFace, FontSize, 0, 0, maxW, maxH, Color);
                g.PopScale();
            }
            else
            {
                g.DrawText(Symbol, FontFace, FontSize, 0, 0, maxW, maxH, Color);
            }
        }
    }
}
