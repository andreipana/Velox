namespace Velox.Controls
{
    public class ToolBar : Control
    {
        private readonly List<ToolBarButton?> _items = new(); // null = separator

        public float ButtonSize   { get; set; } = 30f;
        public float ItemPadding  { get; set; } = 2f;
        public float SepPadding   { get; set; } = 5f;

        public void Add(ToolBarButton button) => _items.Add(button);
        public void AddSeparator()            => _items.Add(null);

        protected override void OnRender(IGraphics g)
        {
            float x = ItemPadding;
            float btnY = (Height - ButtonSize) / 2f;

            foreach (var item in _items)
            {
                if (item == null) // separator
                {
                    x += SepPadding;
                    g.DrawLine(x, 5f, x, Height - 5f, 0xFF3C3C3CU, 1f);
                    x += 1f + SepPadding;
                    continue;
                }

                if (!item.IsVisible) continue;

                item.X = x; item.Y = btnY;
                item.Width = ButtonSize; item.Height = ButtonSize;
                item.Render(g);
                x += ButtonSize + ItemPadding;
            }
        }

        private IEnumerable<ToolBarButton> VisibleButtons
            => _items.OfType<ToolBarButton>().Where(b => b.IsVisible);

        // Forward mouse events with toolbar-local coordinates so child HitTest works.
        public override void OnMouseMove(float px, float py)
        {
            float lx = px - X, ly = py - Y;
            foreach (var item in VisibleButtons) item.OnMouseMove(lx, ly);
        }

        public override void OnMouseDown(float px, float py)
        {
            float lx = px - X, ly = py - Y;
            foreach (var item in VisibleButtons) item.OnMouseDown(lx, ly);
        }

        public override void OnMouseUp(float px, float py)
        {
            float lx = px - X, ly = py - Y;
            foreach (var item in VisibleButtons) item.OnMouseUp(lx, ly);
        }

        public override void OnMouseLeave()
        {
            foreach (var item in VisibleButtons) item.OnMouseLeave();
        }
    }
}
