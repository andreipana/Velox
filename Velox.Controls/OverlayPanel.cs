namespace Velox.Controls
{
    // Stacks children directly on top of one another, each nudged from (0,0) by
    // its own offset — e.g. layering a small badge glyph over a base icon.
    // Panel size is the bounding box of all children at their natural
    // (measured) size plus offset.
    public class OverlayPanel : Panel
    {
        private readonly List<(IControl child, float offsetX, float offsetY)> _offsets = new();

        public void Add(IControl child, float offsetX = 0f, float offsetY = 0f)
        {
            _offsets.Add((child, offsetX, offsetY));
            AddChild(child);
        }

        public void Remove(IControl child)
        {
            _offsets.RemoveAll(t => t.child == child);
            RemoveChild(child);
        }

        protected override void ArrangeChildren(IGraphics g, float w, float h)
        {
            foreach (var (child, offsetX, offsetY) in _offsets)
            {
                if (!child.IsVisible) continue;
                var (dw, dh) = child.Measure(g, w, h);
                child.Arrange(offsetX, offsetY, dw, dh);
            }
        }

        public override (float desiredW, float desiredH) Measure(IGraphics g, float availableW, float availableH)
        {
            float maxW = 0f, maxH = 0f;
            foreach (var (child, offsetX, offsetY) in _offsets)
            {
                if (!child.IsVisible) continue;
                var (dw, dh) = child.Measure(g, availableW, availableH);
                maxW = Math.Max(maxW, offsetX + dw);
                maxH = Math.Max(maxH, offsetY + dh);
            }
            return (maxW, maxH);
        }
    }
}
