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
            float x = ItemPadding;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i].Item;
                float iw = item.MeasureWidth(g);
                item.X = x; item.Y = 0; item.Width = iw;
                item.Render(g);
                x += iw;
                if (i < items.Count - 1)
                {
                    x += ItemPadding;
                    DrawSep(g, x);
                    x += SeparatorWidth + ItemPadding;
                }
            }
        }

        private void RenderRight(IGraphics g, List<(StatusBarItem Item, bool)> items)
        {
            if (items.Count == 0) return;

            // Measure all widths first so we can compute the starting x.
            float[] widths = items.Select(t => t.Item.MeasureWidth(g)).ToArray();
            float total = widths.Sum()
                + (items.Count - 1) * (ItemPadding + SeparatorWidth + ItemPadding);

            float x = Width - ItemPadding - total;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i].Item;
                item.X = x; item.Y = 0; item.Width = widths[i];
                item.Render(g);
                x += widths[i];
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
