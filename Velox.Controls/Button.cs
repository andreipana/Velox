namespace Velox.Controls
{
    public class Button : Control
    {
        public string Text     { get; set; } = "Button";
        public string FontFace { get; set; } = "Segoe UI Variable Text";
        public float  FontSize { get; set; } = 14f;

        public event EventHandler? Click;

        private bool _isHovered = false;
        private bool _isPressed = false;

        protected override void OnRender(Velox.IGraphics graphics)
        {
            uint bgColor     = _isPressed ? 0xFF3D3D3DU : _isHovered ? 0xFF3A3A3AU : 0xFF2D2D2DU;
            uint borderColor = _isPressed ? 0xFF686868U : 0xFF696969U;
            const float r = 6f;

            graphics.FillRoundedRect(0, 0, Width, Height, r, bgColor);
            graphics.DrawRoundedRect(0, 0, Width, Height, r, borderColor, strokeWidth: 1f);

            if (!_isPressed)
            {
                float lineY = Height - 2f;
                graphics.DrawLine(r, lineY, Width - r, lineY, 0xFFA0A0A0U, strokeWidth: 1.5f);
            }

            var (tw, th) = graphics.MeasureText(Text, FontFace, FontSize, Width, Height);
            float tx = (Width  - tw) / 2f;
            float ty = (Height - th) / 2f;
            graphics.DrawText(Text, FontFace, FontSize, tx, ty, Width, Height, 0xFFFFFFFFU);
        }

        public override void OnMouseMove(float px, float py)
        {
            _isHovered = HitTest(px, py);
        }

        public override void OnMouseLeave()
        {
            _isHovered = false;
            _isPressed = false;
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
                Click?.Invoke(this, EventArgs.Empty);
        }
    }
}
