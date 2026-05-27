namespace Velox.Controls
{
    public class StatusBarItem : Control
    {
        private const string Font      = "Segoe UI Variable Text";
        private const float  FontSize  = 12f;
        private const float  SwatchW   = 60f;
        private const float  SwatchH   = 20f;
        private const float  SwatchGap = 5f;

        // Optional dim label prefix, e.g. "X: "
        public string? Label { get; set; }

        // Called at render time to get the display value.
        public Func<string>  GetValue       { get; set; } = () => "";

        // When non-null, a color swatch is drawn before the label. The func is called
        // at render time; return null to hide the swatch while still reserving space.
        public Func<uint?>? GetSwatchColor  { get; set; }

        // Computes the preferred pixel width of this item. Call after Height is set.
        public float MeasureWidth(IGraphics g)
        {
            float w = 0;
            if (GetSwatchColor != null) w += SwatchW + SwatchGap; // always reserve swatch space
            if (!string.IsNullOrEmpty(Label))
                w += g.MeasureText(Label, Font, FontSize, 9999f, 9999f).width;
            w += g.MeasureText(GetValue(), Font, FontSize, 9999f, 9999f).width;
            return w;
        }

        protected override void OnRender(IGraphics g)
        {
            float x = 0;
            string value = GetValue();
            uint? swatch = GetSwatchColor?.Invoke();

            if (GetSwatchColor != null)
            {
                if (swatch.HasValue)
                {
                    float sy = (Height - SwatchH) / 2f;
                    g.FillRect(x, sy, SwatchW, SwatchH, swatch.Value);
                    g.DrawRect(x, sy, SwatchW, SwatchH, 0xFF505050U, 0.5f);
                }
                x += SwatchW + SwatchGap;
            }

            if (!string.IsNullOrEmpty(Label))
            {
                var (lw, lh) = g.MeasureText(Label, Font, FontSize, 9999f, 9999f);
                g.DrawText(Label, Font, FontSize, x, (Height - lh) / 2f, lw + 1f, lh + 1f, 0xFF888888U);
                x += lw;
            }

            if (!string.IsNullOrEmpty(value))
            {
                var (vw, vh) = g.MeasureText(value, Font, FontSize, 9999f, 9999f);
                g.DrawText(value, Font, FontSize, x, (Height - vh) / 2f, vw + 1f, vh + 1f, 0xFFDDDDDDU);
            }
        }
    }
}
