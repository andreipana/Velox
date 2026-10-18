using Velox.Controls;
using Velox.Tests.Fakes;

namespace Velox.Tests.Controls
{
    public class StackPanelTests
    {
        [Test]
        public async Task Vertical_Measure_SumsHeightsAndTakesMaxWidth()
        {
            var panel = new StackPanel { Spacing = 4 };
            panel.Add(new StubControl(50, 10));
            panel.Add(new StubControl(80, 20));

            var (w, h) = panel.Measure(new FakeGraphics(), 1000, 1000);

            await Assert.That(w).IsEqualTo(80);
            await Assert.That(h).IsEqualTo(10 + 4 + 20);
        }

        [Test]
        public async Task Horizontal_Measure_SumsWidthsAndTakesMaxHeight()
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
            panel.Add(new StubControl(50, 10));
            panel.Add(new StubControl(30, 25));

            var (w, h) = panel.Measure(new FakeGraphics(), 1000, 1000);

            await Assert.That(w).IsEqualTo(50 + 2 + 30);
            await Assert.That(h).IsEqualTo(25);
        }

        [Test]
        public async Task Vertical_Render_StretchesChildrenAcrossWidthAndStacksThem()
        {
            var a = new StubControl(50, 10);
            var b = new StubControl(80, 20);
            var panel = new StackPanel { Spacing = 5 };
            panel.Add(a);
            panel.Add(b);
            panel.Arrange(0, 0, 200, 100);

            panel.Render(new FakeGraphics());

            await Assert.That((a.X, a.Y, a.Width, a.Height)).IsEqualTo((0f, 0f, 200f, 10f));
            await Assert.That((b.X, b.Y, b.Width, b.Height)).IsEqualTo((0f, 15f, 200f, 20f));
        }

        [Test]
        public async Task Horizontal_Render_StretchesChildrenAcrossHeightAndStacksThem()
        {
            var a = new StubControl(50, 10);
            var b = new StubControl(30, 20);
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5 };
            panel.Add(a);
            panel.Add(b);
            panel.Arrange(0, 0, 200, 100);

            panel.Render(new FakeGraphics());

            await Assert.That((a.X, a.Y, a.Width, a.Height)).IsEqualTo((0f, 0f, 50f, 100f));
            await Assert.That((b.X, b.Y, b.Width, b.Height)).IsEqualTo((55f, 0f, 30f, 100f));
        }

        [Test]
        public async Task InvisibleChildren_AreSkippedAndDoNotAddSpacing()
        {
            var a = new StubControl(10, 10);
            var hidden = new StubControl(10, 99) { IsVisible = false };
            var b = new StubControl(10, 10);
            var panel = new StackPanel { Spacing = 5 };
            panel.Add(a);
            panel.Add(hidden);
            panel.Add(b);
            panel.Arrange(0, 0, 100, 100);

            var (_, h) = panel.Measure(new FakeGraphics(), 100, 100);
            panel.Render(new FakeGraphics());

            await Assert.That(h).IsEqualTo(25);
            await Assert.That(b.Y).IsEqualTo(15);
            await Assert.That(hidden.RenderCount).IsEqualTo(0);
        }

        [Test]
        public async Task Remove_TakesChildOutOfLayout()
        {
            var a = new StubControl(10, 10);
            var panel = new StackPanel();
            panel.Add(a);

            panel.Remove(a);

            await Assert.That(panel.Children).IsEmpty();
            await Assert.That(panel.Measure(new FakeGraphics(), 100, 100)).IsEqualTo((0f, 0f));
        }
    }
}
