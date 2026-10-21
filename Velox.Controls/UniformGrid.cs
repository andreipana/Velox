namespace Velox.Controls
{
    // Lays children out in a grid of equally sized cells, filling row by row.
    // The number of columns is fixed; rows are added as needed. Like DockPanel it
    // fills the available space unless given an explicit size.
    //
    //   var grid = new UniformGrid { Columns = 2, Spacing = 12 };
    //   grid.Add(first);
    //   grid.Add(second);
    //   grid.Add(third);   // wraps to the second row
    public class UniformGrid : Panel
    {
        private int _columns = 1;

        // Values below 1 are treated as 1.
        public int Columns
        {
            get => _columns;
            set => _columns = Math.Max(1, value);
        }

        // Gap between neighbouring cells, both horizontally and vertically.
        public float Spacing { get; set; } = 0f;

        public void Add(IControl child)    => AddChild(child);
        public void Remove(IControl child) => RemoveChild(child);

        protected override void ArrangeChildren(IGraphics g, float w, float h)
        {
            int count = _children.Count(c => c.IsVisible);
            if (count == 0) return;

            int rows = (count + Columns - 1) / Columns;
            float cellW = Math.Max(0f, (w - (Columns - 1) * Spacing) / Columns);
            float cellH = Math.Max(0f, (h - (rows - 1) * Spacing) / rows);

            int index = 0;
            foreach (var child in _children)
            {
                if (!child.IsVisible) continue;
                int row = index / Columns;
                int col = index % Columns;
                child.Arrange(col * (cellW + Spacing), row * (cellH + Spacing), cellW, cellH);
                index++;
            }
        }

        public override (float desiredW, float desiredH) Measure(IGraphics g, float availableW, float availableH)
            => (Width > 0 ? Width : availableW, Height > 0 ? Height : availableH);
    }
}
