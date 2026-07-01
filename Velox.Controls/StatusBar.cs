namespace Velox.Controls
{
    public class StatusBar : Control
    {
        private readonly List<(StatusBarItem Item, bool AlignRight)> _items = new();

        public float ItemPadding    { get; set; } = 8f;
        public float SeparatorWidth { get; set; } = 1f;
        public uint  Background     { get; set; } = 0xFF1C1C1CU;

        public void Add(StatusBarItem item, bool alignRight = false)
            => _items.Add((item, alignRight));

        protected override void OnRender(IGraphics g)
        {
            g.FillRect(0, 0, Width, Height, Background);
            g.DrawLine(0, 0, Width, 0, 0xFF3C3C3CU, 1f);

            var left  = _items.Where(t => !t.AlignRight).ToList();
            var right = _items.Where(t =>  t.AlignRight).ToList();

            // Pre-set heights so items can measure text height correctly.
            foreach (var (item, _) in _items) item.Height = Height;

            RenderLeft(g, left);
            RenderRight(g, right);
        }

        private void RenderLeft(IGraphics g, List<(StatusBarItem Item, bool)> items)
        {
            var visible = items.Where(t => t.Item.GetIsVisible?.Invoke() ?? true).ToList();
            float x = 3;
            for (int i = 0; i < visible.Count; i++)
            {
                var item = visible[i].Item;
                item.X = x; item.Y = 0;
                item.Width = item.MeasureWidth(g); // setter applies MinWidth/MaxWidth
                item.Render(g);
                x += item.Width;
                if (i < visible.Count - 1)
                {
                    x += ItemPadding;
                    DrawSep(g, x);
                    x += SeparatorWidth + ItemPadding;
                }
            }
        }

        private void RenderRight(IGraphics g, List<(StatusBarItem Item, bool)> items)
        {
            items = items.Where(t => t.Item.GetIsVisible?.Invoke() ?? true).ToList();
            if (items.Count == 0) return;

            // Apply MinWidth/MaxWidth by assigning through the setter, then read back.
            foreach (var (item, _) in items)
                item.Width = item.MeasureWidth(g);

            float total = items.Sum(t => t.Item.Width)
                + (items.Count - 1) * (ItemPadding + SeparatorWidth + ItemPadding);

            float x = Width - ItemPadding - total;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i].Item;
                item.X = x; item.Y = 0; // Width already set above
                item.Render(g);
                x += item.Width;
                if (i < items.Count - 1)
                {
                    x += ItemPadding;
                    DrawSep(g, x);
                    x += SeparatorWidth + ItemPadding;
                }
            }
        }

        private void DrawSep(IGraphics g, float x)
            => g.DrawLine(x, 3f, x, Height - 3f, 0xFF3C3C3CU, 1f);
    }
}
