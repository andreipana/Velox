namespace Velox.Controls
{
    public class ScrollBar : Control
    {
        public float Min          { get; set; } = 0f;
        public float Max          { get; set; } = 100f;
        public float ViewportSize { get; set; } = 0f;
        public float Value        { get; private set; } = 0f;

        public event EventHandler<float>? ValueChanged;

        public void SetValue(float v) => SetValueAndNotify(v);

        private const float ThumbPadding = 2f;
        private const float ThumbMinH    = 20f;
        private const float CornerRadius = 4f;

        private bool  _isHovered      = false;
        private bool  _isThumbHovered = false;
        private bool  _isDragging     = false;
        private float _dragStartY     = 0f;
        private float _dragStartValue = 0f;

        protected override void OnRender(Velox.IGraphics graphics)
        {
            if (_isHovered)
                graphics.FillRect(0, 0, Width, Height, 0xFF1E1E1EU);

            float trackH = Height - 2 * ThumbPadding;
            float thumbH = ComputeThumbH(trackH);
            float thumbTopRel = ComputeThumbY(trackH, thumbH);

            float thumbX = ThumbPadding;
            float thumbY = ThumbPadding + thumbTopRel;
            float thumbW = Width - 2 * ThumbPadding;

            uint thumbColor = _isDragging     ? 0xFFCDCDCDU
                            : _isThumbHovered ? 0xFF9B9B9BU
                            :                   0xFF6B6B6BU;

            graphics.FillRoundedRect(thumbX, thumbY, thumbW, thumbH, CornerRadius, thumbColor);
        }

        public override void OnMouseMove(float px, float py)
        {
            _isHovered      = HitTest(px, py);
            _isThumbHovered = _isHovered && IsOverThumb(py);

            if (_isDragging)
            {
                float trackH = Height - 2 * ThumbPadding;
                float thumbH = ComputeThumbH(trackH);
                float travel = trackH - thumbH;
                float range  = Max - Min;

                float newValue = travel > 0
                    ? _dragStartValue + (py - _dragStartY) / travel * range
                    : Min;

                SetValueAndNotify(Math.Clamp(newValue, Min, Max));
            }
        }

        public override void OnMouseDown(float px, float py)
        {
            if (!HitTest(px, py)) return;

            if (IsOverThumb(py))
            {
                _isDragging     = true;
                _dragStartY     = py;
                _dragStartValue = Value;
            }
            else
            {
                float trackH  = Height - 2 * ThumbPadding;
                float thumbH  = ComputeThumbH(trackH);
                float thumbTop = Y + ThumbPadding + ComputeThumbY(trackH, thumbH);

                float newValue = py < thumbTop
                    ? Value - ViewportSize
                    : Value + ViewportSize;

                SetValueAndNotify(Math.Clamp(newValue, Min, Max));
            }
        }

        public override void OnMouseUp(float px, float py)
        {
            _isDragging = false;
        }

        public override void OnMouseLeave()
        {
            _isDragging     = false;
            _isHovered      = false;
            _isThumbHovered = false;
        }

        public override void OnMouseWheel(float delta)
        {
            // delta > 0 = scrolled up → decrease value; delta < 0 = down → increase value
            SetValueAndNotify(Math.Clamp(Value + (-delta * 3f), Min, Max));
        }

        private float ComputeThumbH(float trackH)
        {
            float total = Max - Min + ViewportSize;
            float ratio = total > 0 ? ViewportSize / total : 1f;
            return Math.Max(ThumbMinH, ratio * trackH);
        }

        private float ComputeThumbY(float trackH, float thumbH)
        {
            float range  = Max - Min;
            float travel = trackH - thumbH;
            return range > 0 ? (Value - Min) / range * travel : 0f;
        }

        private bool IsOverThumb(float py)
        {
            float trackH  = Height - 2 * ThumbPadding;
            float thumbH  = ComputeThumbH(trackH);
            float thumbTop = Y + ThumbPadding + ComputeThumbY(trackH, thumbH);
            return py >= thumbTop && py < thumbTop + thumbH;
        }

        private void SetValueAndNotify(float newValue)
        {
            float clamped = Math.Clamp(newValue, Min, Max);
            if (Math.Abs(clamped - Value) < 0.001f) return;
            Value = clamped;
            ValueChanged?.Invoke(this, Value);
        }
    }
}
