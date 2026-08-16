namespace Velox.Controls
{
    // Base class for layout containers. Subclasses implement ArrangeChildren to
    // position children, and expose their own Add/Remove API.
    // Event routing converts from the parent's coordinate space to panel-local space
    // before dispatching to children, so children only ever see local coordinates.
    public abstract class Panel : Control
    {
        protected readonly List<IControl> _children = new();

        public IReadOnlyList<IControl> Children => _children;

        protected void AddChild(IControl child)    => _children.Add(child);
        protected void RemoveChild(IControl child) => _children.Remove(child);

        protected override void OnRender(IGraphics g)
        {
            ArrangeChildren(g, Width, Height);
            foreach (var child in _children)
                if (child.IsVisible) child.Render(g);
        }

        protected abstract void ArrangeChildren(IGraphics g, float w, float h);

        // All mouse events subtract this panel's (X, Y) so children receive local coords.

        public override void OnMouseMove(float px, float py)
        {
            float lx = px - X, ly = py - Y;
            foreach (var child in _children)
                if (child.IsVisible) child.OnMouseMove(lx, ly);
        }

        public override void OnMouseDown(float px, float py)
        {
            float lx = px - X, ly = py - Y;
            foreach (var child in _children)
                if (child.IsVisible) child.OnMouseDown(lx, ly);
        }

        public override void OnMouseUp(float px, float py)
        {
            float lx = px - X, ly = py - Y;
            foreach (var child in _children)
                if (child.IsVisible) child.OnMouseUp(lx, ly);
        }

        public override void OnRightMouseDown(float px, float py)
        {
            float lx = px - X, ly = py - Y;
            foreach (var child in _children)
                if (child.IsVisible) child.OnRightMouseDown(lx, ly);
        }

        public override void OnRightMouseUp(float px, float py)
        {
            float lx = px - X, ly = py - Y;
            foreach (var child in _children)
                if (child.IsVisible) child.OnRightMouseUp(lx, ly);
        }

        public override void OnMouseLeave()
        {
            foreach (var child in _children)
                if (child.IsVisible) child.OnMouseLeave();
        }

        public override void OnMouseWheel(float delta, float px, float py)
        {
            float lx = px - X, ly = py - Y;
            foreach (var child in _children)
                if (child.IsVisible) child.OnMouseWheel(delta, lx, ly);
        }

        public override void OnKeyDown(VirtualKey key, bool ctrl, bool shift, bool alt)
        {
            foreach (var child in _children)
                if (child.IsVisible) child.OnKeyDown(key, ctrl, shift, alt);
        }

        // Reverse order so a later-added (visually topmost, e.g. an OverlayPanel
        // badge) child wins over one it overlaps.
        public override IControl? HitTestDeep(float px, float py)
        {
            float lx = px - X, ly = py - Y;
            for (int i = _children.Count - 1; i >= 0; i--)
            {
                var child = _children[i];
                if (!child.IsVisible) continue;
                var hit = child.HitTestDeep(lx, ly);
                if (hit != null) return hit;
            }
            return HitTest(px, py) ? this : null;
        }
    }
}
