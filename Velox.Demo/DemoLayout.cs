using Velox;
using Velox.Controls;

namespace Velox.Demo
{
    // A titled card that hosts one demo control. Demo chrome only — not library API.
    internal sealed class DemoCell : Panel
    {
        private const float HeaderHeight = 30f;
        private const float Padding      = 12f;

        private readonly string _title;
        private readonly IControl _content;

        public DemoCell(string title, IControl content)
        {
            _title = title;
            _content = content;
            AddChild(content);
        }

        protected override void OnRender(IGraphics g)
        {
            g.FillRoundedRect(0, 0, Width, Height, 8f, 0x14FFFFFFU);
            g.DrawRoundedRect(0, 0, Width, Height, 8f, 0x28FFFFFFU);
            g.DrawText(_title, "Segoe UI Variable Text", 13f, Padding, 8f, Width - 2 * Padding, HeaderHeight, 0xFFAAAAAAU, noWrap: true);

            // Keep overflowing demo content inside the card.
            g.PushClip(0, HeaderHeight, Width, Height - HeaderHeight);
            try { base.OnRender(g); }
            finally { g.PopClip(); }
        }

        protected override void ArrangeChildren(IGraphics g, float w, float h)
            => _content.Arrange(Padding, HeaderHeight, w - 2 * Padding, h - HeaderHeight - Padding);
    }

    // Insets a single child. The library has no Margin/Padding yet, so the demo carries its own.
    internal sealed class Padded : Panel
    {
        private readonly IControl _child;
        private readonly float _left, _top, _right, _bottom;

        public Padded(IControl child, float left, float top, float right, float bottom)
        {
            _child = child;
            (_left, _top, _right, _bottom) = (left, top, right, bottom);
            AddChild(child);
        }

        protected override void ArrangeChildren(IGraphics g, float w, float h)
            => _child.Arrange(_left, _top, w - _left - _right, h - _top - _bottom);

        public override (float desiredW, float desiredH) Measure(IGraphics g, float availableW, float availableH)
        {
            var (cw, ch) = _child.Measure(g, availableW - _left - _right, availableH - _top - _bottom);
            return (cw + _left + _right, ch + _top + _bottom);
        }
    }
}
