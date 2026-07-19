namespace Velox.Controls
{
    public class Button : Control
    {
        private IControl? _content;

        public IControl? Content
        {
            get => _content;
            set => _content = value;
        }

        // Convenience: sets Content to a plain TextBlock.
        // Getter returns the text if Content is currently a TextBlock, otherwise null.
        public string? Text
        {
            get => (_content as TextBlock)?.Text;
            set => _content = value != null
                ? new TextBlock { Text = value, TextColor = 0xFFFFFFFFU }
                : null;
        }

        public event EventHandler? Click;

        private bool _isHovered = false;
        private bool _isPressed = false;

        protected override void OnRender(IGraphics g)
        {
            uint bgColor     = _isPressed ? 0xFF3D3D3DU : _isHovered ? 0xFF3A3A3AU : 0xFF2D2D2DU;
            uint borderColor = _isPressed ? 0xFF686868U : 0xFF696969U;
            const float r = 6f;

            g.FillRoundedRect(0, 0, Width, Height, r, bgColor);
            g.DrawRoundedRect(0, 0, Width, Height, r, borderColor, strokeWidth: 1f);

            if (!_isPressed)
            {
                float lineY = Height - 2f;
                g.DrawLine(r, lineY, Width - r, lineY, 0xFFA0A0A0U, strokeWidth: 1.5f);
            }

            if (_content != null)
            {
                const float pad = 8f;
                var (dw, dh) = _content.Measure(g, Width - 2 * pad, Height - 2 * pad);
                _content.Arrange((Width - dw) / 2f, (Height - dh) / 2f, dw, dh);
                _content.Render(g);
            }
        }

        public override (float desiredW, float desiredH) Measure(IGraphics g, float availableW, float availableH)
        {
            if (Width > 0 && Height > 0) return (Width, Height);
            if (_content == null) return (Width, Height);
            const float pad = 8f;
            float innerW = Width  > 0 ? Width  - 2 * pad : availableW - 2 * pad;
            float innerH = Height > 0 ? Height - 2 * pad : availableH - 2 * pad;
            var (dw, dh) = _content.Measure(g, innerW, innerH);
            return (Width > 0 ? Width : dw + 2 * pad, Height > 0 ? Height : dh + 2 * pad);
        }

        public override void OnMouseMove(float px, float py)  => _isHovered = HitTest(px, py);
        public override void OnMouseLeave()                   { _isHovered = false; _isPressed = false; }
        public override void OnMouseDown(float px, float py)  { if (HitTest(px, py)) _isPressed = true; }

        public override void OnMouseUp(float px, float py)
        {
            bool wasPressed = _isPressed;
            _isPressed = false;
            if (wasPressed && HitTest(px, py))
                Click?.Invoke(this, EventArgs.Empty);
        }
    }
}
