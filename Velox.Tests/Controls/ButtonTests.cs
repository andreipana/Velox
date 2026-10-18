using Velox.Controls;
using Velox.Tests.Fakes;

namespace Velox.Tests.Controls
{
    public class ButtonTests
    {
        private static Button CreateButton()
        {
            var b = new Button();
            b.Arrange(0, 0, 100, 30);
            return b;
        }

        [Test]
        public async Task Click_FiresOnPressAndReleaseInside()
        {
            var b = CreateButton();
            int clicks = 0;
            b.Click += (_, _) => clicks++;

            b.OnMouseDown(10, 10);
            b.OnMouseUp(10, 10);

            await Assert.That(clicks).IsEqualTo(1);
        }

        [Test]
        public async Task Click_DoesNotFireWhenReleasedOutside()
        {
            var b = CreateButton();
            int clicks = 0;
            b.Click += (_, _) => clicks++;

            b.OnMouseDown(10, 10);
            b.OnMouseUp(500, 500);

            await Assert.That(clicks).IsEqualTo(0);
        }

        [Test]
        public async Task Click_DoesNotFireWithoutPressInside()
        {
            var b = CreateButton();
            int clicks = 0;
            b.Click += (_, _) => clicks++;

            b.OnMouseDown(500, 500);
            b.OnMouseUp(10, 10);

            await Assert.That(clicks).IsEqualTo(0);
        }

        [Test]
        public async Task Click_DoesNotFireAfterMouseLeaveCancelsPress()
        {
            var b = CreateButton();
            int clicks = 0;
            b.Click += (_, _) => clicks++;

            b.OnMouseDown(10, 10);
            b.OnMouseLeave();
            b.OnMouseUp(10, 10);

            await Assert.That(clicks).IsEqualTo(0);
        }

        [Test]
        public async Task Text_RoundTripsThroughContent()
        {
            var b = new Button { Text = "OK" };

            await Assert.That(b.Text).IsEqualTo("OK");
            await Assert.That(b.Content).IsTypeOf<TextBlock>();

            b.Text = null;
            await Assert.That(b.Content).IsNull();
        }

        [Test]
        public async Task Measure_WithContent_AddsPaddingAroundContent()
        {
            var b = new Button { Content = new StubControl(40, 12) };

            await Assert.That(b.Measure(new FakeGraphics(), 500, 500)).IsEqualTo((56f, 28f));
        }

        [Test]
        public async Task Measure_ExplicitSize_WinsOverContent()
        {
            var b = new Button { Content = new StubControl(40, 12) };
            b.Arrange(0, 0, 100, 30);

            await Assert.That(b.Measure(new FakeGraphics(), 500, 500)).IsEqualTo((100f, 30f));
        }

        [Test]
        public async Task Render_CentersContent()
        {
            var content = new StubControl(40, 10);
            var b = new Button { Content = content };
            b.Arrange(0, 0, 100, 30);

            b.Render(new FakeGraphics());

            await Assert.That((content.X, content.Y, content.Width, content.Height)).IsEqualTo((30f, 10f, 40f, 10f));
            await Assert.That(content.RenderCount).IsEqualTo(1);
        }
    }
}
