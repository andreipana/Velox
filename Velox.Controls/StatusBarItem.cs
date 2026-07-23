namespace Velox.Controls
{
    public class StatusBarItem : Control
    {
        public IControl? Content { get; set; }

        public float MeasureWidth(IGraphics g)
        {
            if (Content == null) return 0;
            var (desiredW, _) = Content.Measure(g, 9999f, Height);
            return desiredW;
        }

        protected override void OnRender(IGraphics g)
        {
            if (Content == null) return;
            var (dw, dh) = Content.Measure(g, Width, Height);
            Content.Arrange(0, (Height - dh) / 2f, dw, dh);
            Content.Render(g);
        }
    }
}
