namespace Velox.Controls
{
    public class ToolBarButton : Control
    {
        private IControl? _content;

        public IControl? Content
        {
            get => _content;
            set => _content = value;
        }

        // Convenience: sets Content to a glyph TextBlock using the specified font/size.
        // GlyphFont and GlyphSize must be set before Glyph in object initializers.
        public string? Glyph
        {
            get => (_content as TextBlock)?.Text;
            set => _content = value != null
                ? new TextBlock { Text = value, FontFace = GlyphFont, FontSize = GlyphSize }
                : null;
        }

        public string GlyphFont { get; set; } = "Segoe Fluent Icons";
        public float  GlyphSize { get; set; } = 16f;

        public bool IsToggle  { get; set; } = false;
        public bool IsChecked { get; set; } = false;

        public event EventHandler? Click;

        private bool _isHovered;
        private bool _isPressed;

        protected override void OnRender(IGraphics g)
        {
            uint bg = (_isPressed || (IsToggle && IsChecked))
                ? 0xFF3A3A3AU
                : _isHovered ? 0x18FFFFFFU
                : 0x00000000U;

            if (bg != 0)
                g.FillRoundedRect(0, 0, Width, Height, 4f, bg);

            if (IsToggle && IsChecked && !_isPressed)
                g.DrawRoundedRect(0, 0, Width, Height, 4f, 0xFF606060U, 1f);

            if (_content != null)
            {
                if (_content is TextBlock tb)
                    tb.TextColor = IsEnabled ? 0xFFCCCCCCU : 0xFF666666U;

                var (dw, dh) = _content.Measure(g, Width, Height);
                _content.Arrange((Width - dw) / 2f, (Height - dh) / 2f, dw, dh);
                _content.Render(g);
            }
        }

        public override void OnMouseMove(float px, float py)  => _isHovered = HitTest(px, py);
        public override void OnMouseLeave()                   { _isHovered = false; _isPressed = false; }
        public override void OnMouseDown(float px, float py)  { if (HitTest(px, py)) _isPressed = true; }

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
    }
}
