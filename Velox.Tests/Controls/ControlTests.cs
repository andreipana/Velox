using Velox.Tests.Fakes;

namespace Velox.Tests.Controls
{
    public class ControlTests
    {
        [Test]
        public async Task Width_IsClampedToMinAndMax()
        {
            var c = new StubControl { MinWidth = 50, MaxWidth = 100 };

            c.Width = 10;
            await Assert.That(c.Width).IsEqualTo(50);

            c.Width = 500;
            await Assert.That(c.Width).IsEqualTo(100);

            c.Width = 70;
            await Assert.That(c.Width).IsEqualTo(70);
        }

        [Test]
        public async Task Arrange_SetsBoundsAndAppliesClamping()
        {
            var c = new StubControl { MaxHeight = 20 };

            c.Arrange(5, 6, 30, 90);

            await Assert.That((c.X, c.Y, c.Width, c.Height)).IsEqualTo((5f, 6f, 30f, 20f));
        }

        [Test]
        [Arguments(0f, 0f, true)]
        [Arguments(9.9f, 19.9f, true)]
        [Arguments(10f, 5f, false)]   // right edge is exclusive
        [Arguments(5f, 20f, false)]   // bottom edge is exclusive
        [Arguments(-0.1f, 5f, false)]
        public async Task HitTest_UsesHalfOpenBounds(float px, float py, bool expected)
        {
            var c = new StubControl();
            c.Arrange(0, 0, 10, 20);

            await Assert.That(c.HitTest(px, py)).IsEqualTo(expected);
        }

        [Test]
        public async Task HitTest_IsFalseWhenHiddenOrDisabled()
        {
            var c = new StubControl();
            c.Arrange(0, 0, 10, 10);

            c.IsEnabled = false;
            await Assert.That(c.HitTest(1, 1)).IsFalse();

            c.IsEnabled = true;
            c.IsVisible = false;
            await Assert.That(c.HitTest(1, 1)).IsFalse();
        }

        [Test]
        public async Task Render_TranslatesToControlOrigin_AndRestoresState()
        {
            var g = new FakeGraphics();
            var c = new StubControl();
            c.Arrange(12, 34, 10, 10);

            c.Render(g);

            await Assert.That(c.RenderCount).IsEqualTo(1);
            await Assert.That(g.TranslationDepth).IsEqualTo(0);
            var push = g.CallsNamed("PushTranslation").Single();
            await Assert.That(push.Args).IsEquivalentTo(new object[] { 12f, 34f });
        }

        [Test]
        public async Task Render_DoesNothingWhenInvisible()
        {
            var g = new FakeGraphics();
            var c = new StubControl { IsVisible = false };

            c.Render(g);

            await Assert.That(c.RenderCount).IsEqualTo(0);
            await Assert.That(g.Calls).IsEmpty();
        }
    }
}
