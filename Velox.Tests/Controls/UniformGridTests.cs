using Velox.Controls;
using Velox.Tests.Fakes;

namespace Velox.Tests.Controls
{
    public class UniformGridTests
    {
        private static (UniformGrid grid, StubControl[] items) CreateGrid(int columns, int count, float spacing = 0f)
        {
            var grid = new UniformGrid { Columns = columns, Spacing = spacing };
            var items = Enumerable.Range(0, count).Select(_ => new StubControl()).ToArray();
            foreach (var item in items) grid.Add(item);
            return (grid, items);
        }

        [Test]
        public async Task Children_FillRowsLeftToRightWithEqualCells()
        {
            var (grid, items) = CreateGrid(columns: 2, count: 6, spacing: 10);
            grid.Arrange(0, 0, 410, 320);

            grid.Render(new FakeGraphics());

            // cell = (410 - 10) / 2 = 200 wide, (320 - 2 * 10) / 3 = 100 tall
            await Assert.That((items[0].X, items[0].Y, items[0].Width, items[0].Height)).IsEqualTo((0f, 0f, 200f, 100f));
            await Assert.That((items[1].X, items[1].Y, items[1].Width, items[1].Height)).IsEqualTo((210f, 0f, 200f, 100f));
            await Assert.That((items[2].X, items[2].Y)).IsEqualTo((0f, 110f));
            await Assert.That((items[5].X, items[5].Y, items[5].Width, items[5].Height)).IsEqualTo((210f, 220f, 200f, 100f));
        }

        [Test]
        public async Task IncompleteLastRow_StillUsesFullRowCount()
        {
            var (grid, items) = CreateGrid(columns: 2, count: 3);
            grid.Arrange(0, 0, 200, 200);

            grid.Render(new FakeGraphics());

            await Assert.That((items[2].X, items[2].Y, items[2].Width, items[2].Height)).IsEqualTo((0f, 100f, 100f, 100f));
        }

        [Test]
        public async Task InvisibleChildren_DoNotTakeACell()
        {
            var (grid, items) = CreateGrid(columns: 2, count: 3);
            items[0].IsVisible = false;
            grid.Arrange(0, 0, 200, 100);

            grid.Render(new FakeGraphics());

            await Assert.That((items[1].X, items[1].Y)).IsEqualTo((0f, 0f));
            await Assert.That((items[2].X, items[2].Y)).IsEqualTo((100f, 0f));
        }

        [Test]
        public async Task Columns_BelowOne_AreTreatedAsOne()
        {
            var grid = new UniformGrid { Columns = 0 };

            await Assert.That(grid.Columns).IsEqualTo(1);
        }

        [Test]
        public async Task Measure_FillsAvailableSpaceUnlessSizeIsExplicit()
        {
            var grid = new UniformGrid();
            var g = new FakeGraphics();

            await Assert.That(grid.Measure(g, 300, 200)).IsEqualTo((300f, 200f));

            grid.Height = 50;
            await Assert.That(grid.Measure(g, 300, 200)).IsEqualTo((300f, 50f));
        }

        [Test]
        public async Task Render_WithNoChildren_DoesNotThrow()
        {
            var grid = new UniformGrid { Columns = 3 };
            grid.Arrange(0, 0, 100, 100);

            grid.Render(new FakeGraphics());

            await Assert.That(grid.Children).IsEmpty();
        }
    }
}
