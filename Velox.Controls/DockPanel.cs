namespace Velox.Controls
{
    public enum Dock { Top, Bottom, Left, Right, Fill }

    // Positions children docked to the edges of the available area, with the last
    // Fill child consuming whatever space remains. Children are arranged in add order:
    // Top/Bottom consume rows, Left/Right consume columns, Fill takes the remainder.
    //
    // Typical usage as a window root:
    //   var dock = new DockPanel();
    //   dock.Add(toolbar,   Dock.Top);
    //   dock.Add(statusBar, Dock.Bottom);
    //   dock.Add(sidebar,   Dock.Right);
    //   dock.Add(content,   Dock.Fill);
    //   window.Content = dock;
    public class DockPanel : Panel
    {
        private readonly List<(IControl child, Dock dock)> _docked = new();

        public void Add(IControl child, Dock dock = Dock.Fill)
        {
            _docked.Add((child, dock));
            AddChild(child);
        }

        public void Remove(IControl child)
        {
            _docked.RemoveAll(t => t.child == child);
            RemoveChild(child);
        }

        protected override void ArrangeChildren(IGraphics g, float w, float h)
        {
            float left = 0, top = 0, right = w, bottom = h;
            foreach (var (child, dock) in _docked)
            {
                if (!child.IsVisible) continue;
                var (dw, dh) = child.Measure(g, right - left, bottom - top);
                switch (dock)
                {
                    case Dock.Top:
                        child.Arrange(left, top, right - left, dh);
                        top += dh;
                        break;
                    case Dock.Bottom:
                        child.Arrange(left, bottom - dh, right - left, dh);
                        bottom -= dh;
                        break;
                    case Dock.Left:
                        child.Arrange(left, top, dw, bottom - top);
                        left += dw;
                        break;
                    case Dock.Right:
                        child.Arrange(right - dw, top, dw, bottom - top);
                        right -= dw;
                        break;
                    case Dock.Fill:
                    default:
                        child.Arrange(left, top, right - left, bottom - top);
                        break;
                }
            }
        }

        // DockPanel fills available space by default; returns explicit size if set.
        public override (float desiredW, float desiredH) Measure(IGraphics g, float availableW, float availableH)
            => (Width > 0 ? Width : availableW, Height > 0 ? Height : availableH);
    }
}
