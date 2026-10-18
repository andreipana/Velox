using Velox.Controls;

namespace Velox.Tests.Fakes
{
    // Minimal concrete Control with a fixed desired size that records the input it receives.
    public sealed class StubControl : Control
    {
        public StubControl(float desiredW = 0f, float desiredH = 0f)
        {
            DesiredW = desiredW;
            DesiredH = desiredH;
        }

        public float DesiredW { get; set; }
        public float DesiredH { get; set; }

        public int RenderCount { get; private set; }
        public List<(float x, float y)> MouseMoves { get; } = new();
        public List<(float x, float y)> MouseDowns { get; } = new();
        public int MouseLeaveCount { get; private set; }

        public override (float desiredW, float desiredH) Measure(IGraphics graphics, float availableW, float availableH)
            => (DesiredW, DesiredH);

        protected override void OnRender(IGraphics graphics) => RenderCount++;

        public override void OnMouseMove(float px, float py) => MouseMoves.Add((px, py));
        public override void OnMouseDown(float px, float py) => MouseDowns.Add((px, py));
        public override void OnMouseLeave() => MouseLeaveCount++;
    }
}
