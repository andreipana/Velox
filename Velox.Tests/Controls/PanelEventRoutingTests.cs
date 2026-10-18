using Velox.Controls;
using Velox.Tests.Fakes;

namespace Velox.Tests.Controls
{
    public class PanelEventRoutingTests
    {
        [Test]
        public async Task MouseEvents_AreTranslatedToPanelLocalCoordinates()
        {
            var child = new StubControl();
            var panel = new StackPanel();
            panel.Add(child);
            panel.Arrange(100, 50, 200, 200);

            panel.OnMouseMove(110, 60);
            panel.OnMouseDown(120, 70);

            await Assert.That(child.MouseMoves.Single()).IsEqualTo((10f, 10f));
            await Assert.That(child.MouseDowns.Single()).IsEqualTo((20f, 20f));
        }

        [Test]
        public async Task MouseEvents_SkipInvisibleChildren()
        {
            var child = new StubControl { IsVisible = false };
            var panel = new StackPanel();
            panel.Add(child);

            panel.OnMouseMove(1, 1);
            panel.OnMouseLeave();

            await Assert.That(child.MouseMoves).IsEmpty();
            await Assert.That(child.MouseLeaveCount).IsEqualTo(0);
        }

        [Test]
        public async Task HitTestDeep_ReturnsInnermostControl()
        {
            var child = new StubControl();
            var panel = new StackPanel();
            panel.Add(child);
            panel.Arrange(100, 100, 200, 200);
            child.Arrange(10, 10, 50, 50);

            // (115, 115) is panel-local (15, 15), inside the child.
            await Assert.That(panel.HitTestDeep(115, 115)).IsSameReferenceAs(child);
            // Inside the panel but outside the child.
            await Assert.That(panel.HitTestDeep(250, 250)).IsSameReferenceAs(panel);
            // Outside the panel.
            await Assert.That(panel.HitTestDeep(10, 10)).IsNull();
        }

        [Test]
        public async Task HitTestDeep_PrefersLaterAddedOverlappingChild()
        {
            var below = new StubControl();
            var above = new StubControl();
            var panel = new StackPanel();
            panel.Add(below);
            panel.Add(above);
            panel.Arrange(0, 0, 100, 100);
            below.Arrange(0, 0, 50, 50);
            above.Arrange(0, 0, 50, 50);

            await Assert.That(panel.HitTestDeep(10, 10)).IsSameReferenceAs(above);
        }
    }
}
