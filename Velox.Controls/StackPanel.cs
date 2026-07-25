namespace Velox.Controls
{
    public enum Orientation { Horizontal, Vertical }

    // Stacks children in a single row or column.
    // Each child is measured for its desired size; children fill the cross-axis
    // (width for Vertical, height for Horizontal). Use MaxWidth/MaxHeight on a
    // child to prevent it from being stretched beyond its natural size.
    public class StackPanel : Panel
    {
        public Orientation Orientation { get; set; } = Orientation.Vertical;
        public float Spacing { get; set; } = 0f;

        public void Add(IControl child)    => AddChild(child);
        public void Remove(IControl child) => RemoveChild(child);

        protected override void ArrangeChildren(IGraphics g, float w, float h)
        {
            float offset = 0f;
            bool first = true;
            foreach (var child in _children)
            {
                if (!child.IsVisible) continue;
                if (!first) offset += Spacing;
                var (dw, dh) = child.Measure(g, w, h);
                if (Orientation == Orientation.Vertical)
                {
                    child.Arrange(0, offset, w, dh);
                    offset += dh;
                }
                else
                {
                    child.Arrange(offset, 0, dw, h);
                    offset += dw;
                }
                first = false;
            }
        }

        public override (float desiredW, float desiredH) Measure(IGraphics g, float availableW, float availableH)
        {
            float totalW = 0f, totalH = 0f;
            bool first = true;
            foreach (var child in _children)
            {
                if (!child.IsVisible) continue;
                var (dw, dh) = child.Measure(g, availableW, availableH);
                if (Orientation == Orientation.Vertical)
                {
                    if (!first) totalH += Spacing;
                    totalH += dh;
                    totalW = Math.Max(totalW, dw);
                }
                else
                {
                    if (!first) totalW += Spacing;
                    totalW += dw;
                    totalH = Math.Max(totalH, dh);
                }
                first = false;
            }
            return (totalW, totalH);
        }
    }
}
