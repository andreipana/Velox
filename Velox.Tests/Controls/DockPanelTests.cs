using Velox.Controls;
using Velox.Tests.Fakes;

namespace Velox.Tests.Controls
{
    public class DockPanelTests
    {
        [Test]
        public async Task Docked_Children_ConsumeEdgesAndFillTakesRemainder()
        {
            var top    = new StubControl(0, 30);
            var bottom = new StubControl(0, 20);
            var left   = new StubControl(40, 0);
            var right  = new StubControl(60, 0);
            var fill   = new StubControl();
            var dock = new DockPanel();
            dock.Add(top, Dock.Top);
            dock.Add(bottom, Dock.Bottom);
            dock.Add(left, Dock.Left);
            dock.Add(right, Dock.Right);
            dock.Add(fill, Dock.Fill);
            dock.Arrange(0, 0, 400, 300);

            dock.Render(new FakeGraphics());

            await Assert.That((top.X, top.Y, top.Width, top.Height)).IsEqualTo((0f, 0f, 400f, 30f));
            await Assert.That((bottom.X, bottom.Y, bottom.Width, bottom.Height)).IsEqualTo((0f, 280f, 400f, 20f));
            await Assert.That((left.X, left.Y, left.Width, left.Height)).IsEqualTo((0f, 30f, 40f, 250f));
            await Assert.That((right.X, right.Y, right.Width, right.Height)).IsEqualTo((340f, 30f, 60f, 250f));
            await Assert.That((fill.X, fill.Y, fill.Width, fill.Height)).IsEqualTo((40f, 30f, 300f, 250f));
        }

        [Test]
        public async Task Measure_FillsAvailableSpaceUnlessSizeIsExplicit()
        {
            var dock = new DockPanel();
            var g = new FakeGraphics();

            await Assert.That(dock.Measure(g, 300, 200)).IsEqualTo((300f, 200f));

            dock.Width = 120;
            await Assert.That(dock.Measure(g, 300, 200)).IsEqualTo((120f, 200f));
        }
    }
}
