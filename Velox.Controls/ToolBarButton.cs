namespace Velox.Controls
{
    public class ToolBarButton : Control
    {
        private const string DefaultFont = "Segoe Fluent Icons";
        private const float  DefaultSize = 16f;

        public string Glyph      { get; set; } = "";
        public string GlyphFont  { get; set; } = DefaultFont;
        public float  GlyphSize  { get; set; } = DefaultSize;
        public bool   IsToggle   { get; set; } = false;
        public bool   IsChecked  { get; set; } = false;

        public event EventHandler? Click;

        private bool _isHovered;
        private bool _isPressed;

        protected override void OnRender(IGraphics g)
        {
            // Background
            uint bg = (_isPressed || (IsToggle && IsChecked))
                ? 0xFF3A3A3AU
                : _isHovered
                    ? 0x18FFFFFFU
                    : 0x00000000U;

            if (bg != 0)
                g.FillRoundedRect(0, 0, Width, Height, 4f, bg);

            if (IsToggle && IsChecked && !_isPressed)
                g.DrawRoundedRect(0, 0, Width, Height, 4f, 0xFF606060U, 1f);

            // Glyph centered
            if (!string.IsNullOrEmpty(Glyph))
            {
                var (tw, th) = g.MeasureText(Glyph, GlyphFont, GlyphSize, Width, Height);
                float tx = (Width  - tw) / 2f;
                float ty = (Height - th) / 2f;
                uint color = IsEnabled ? 0xFFCCCCCCU : 0xFF666666U;
                g.DrawText(Glyph, GlyphFont, GlyphSize, tx, ty, tw + 1f, th + 1f, color);
            }
        }

        public override void OnMouseMove(float px, float py)
        {
            _isHovered = HitTest(px, py);
        }

        public override void OnMouseDown(float px, float py)
        {
            if (HitTest(px, py)) _isPressed = true;
        }

        public override void OnMouseUp(float px, float py)
        {
            bool wasPressed = _isPressed;
            _isPressed = false;
            if (wasPressed && HitTest(px, py))
            {
                if (IsToggle) IsChecked = !IsChecked;
                Click?.Invoke(this, EventArgs.Empty);
            }
        }

        public override void OnMouseLeave()
        {
            _isHovered = false;
            _isPressed = false;
        }
    }
}
