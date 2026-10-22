using Velox;
using Velox.Controls;

namespace Velox.Demo.Pages
{
    // Overview page: one cell per control in a 2-column grid.
    public class GeneralPage : IPage
    {
        private const string IconFont = DemoTheme.IconFont;
        private const uint Muted = DemoTheme.Muted;

        public IControl Build()
        {
            var grid = new UniformGrid { Columns = 2, Spacing = 12 };
            grid.Add(new DemoCell("Button", CreateButtonDemo()));
            grid.Add(new DemoCell("Button - content and states", CreateButtonContentDemo()));
            grid.Add(new DemoCell("ScrollBar", CreateScrollBarDemo()));
            grid.Add(new DemoCell("Image", CreateImageDemo()));
            grid.Add(new DemoCell("Glyph and OverlayPanel", CreateGlyphDemo()));
            grid.Add(new DemoCell("TextBlock", CreateTextBlockDemo()));
            return grid;
        }

        private static StackPanel CreateButtonDemo()
        {
            var status = new TextBlock { Text = "Press a button...", TextColor = Muted };

            var hello = new Button { Width = 140, Height = 36, Text = "Say Hello" };
            var reset = new Button { Width = 100, Height = 36, Text = "Reset" };
            hello.Click += (_, _) => status.Text = "Hello, World!";
            reset.Click += (_, _) => status.Text = "Press a button...";

            var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
            row.Add(hello);
            row.Add(reset);

            var stack = new StackPanel { Spacing = 12 };
            stack.Add(row);
            stack.Add(status);
            return stack;
        }

        private static StackPanel CreateButtonContentDemo()
        {
            var iconContent = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            iconContent.Add(new Glyph { Symbol = "", FontFace = IconFont, FontSize = 16f, Color = 0xFFFFFFFFU });
            iconContent.Add(new TextBlock { Text = "Copy" });

            var iconButton = new Button { Content = iconContent };
            var disabledButton = new Button { Width = 140, Height = 36, Text = "Disabled", IsEnabled = false };

            var clicks = 0;
            var counter = new TextBlock { Text = "Clicks: 0", TextColor = Muted };
            iconButton.Click += (_, _) => counter.Text = $"Clicks: {++clicks}";

            var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
            row.Add(iconButton);
            row.Add(disabledButton);

            var stack = new StackPanel { Spacing = 12 };
            stack.Add(row);
            stack.Add(counter);
            return stack;
        }

        private static StackPanel CreateScrollBarDemo()
        {
            var label = new TextBlock { Text = "Scroll: 0", TextColor = Muted };
            var scrollBar = new ScrollBar
            {
                Width = 12, Height = 110,
                Min = 0, Max = 100, ViewportSize = 20,
            };
            scrollBar.ValueChanged += (_, v) => label.Text = $"Scroll: {(int)v}";

            var stack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
            stack.Add(scrollBar);
            stack.Add(label);
            return stack;
        }

        private static StackPanel CreateImageDemo()
        {
            string imagesDir = Path.Combine(AppContext.BaseDirectory, "images");

            // Max* is set first so Arrange can't stretch the images beyond their natural size.
            var stack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
            stack.Add(new Image { MaxWidth = 96, MaxHeight = 96, Width = 96, Height = 96, Source = new Bitmap(Path.Combine(imagesDir, "1.png")) });
            stack.Add(new Image { MaxWidth = 96, MaxHeight = 96, Width = 96, Height = 96, Source = new Bitmap(Path.Combine(imagesDir, "2.png")) });
            stack.Add(new Image { MaxWidth = 128, MaxHeight = 96, Width = 128, Height = 96, Source = new Bitmap(Path.Combine(imagesDir, "3.jpg")) });
            return stack;
        }

        private static StackPanel CreateGlyphDemo()
        {
            var icons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16 };
            foreach (var symbol in new[] { "", "", "", "" }) // Copy, Save, Delete, Settings
                icons.Add(new Glyph { Symbol = symbol, FontFace = IconFont, FontSize = 32f, Color = 0xFF60CDFFU });

            var badged = new OverlayPanel();
            badged.Add(new Glyph { Symbol = "", FontFace = IconFont, FontSize = 32f, Color = 0xFFCCCCCCU });
            badged.Add(new Glyph { Symbol = "", FontFace = IconFont, FontSize = 16f, Color = 0xFF6CCB5FU }, 20, 18); // CheckMark badge

            var stack = new StackPanel { Spacing = 14 };
            stack.Add(icons);
            stack.Add(badged);
            return stack;
        }

        private static StackPanel CreateTextBlockDemo()
        {
            var stack = new StackPanel { Spacing = 8 };
            stack.Add(new TextBlock { Text = "Custom size and color", FontSize = 18f, TextColor = 0xFF60CDFFU });
            stack.Add(new TextBlock { Text = " Text with a background ", Background = 0xFF3A3A8AU });
            stack.Add(new TextBlock
            {
                Text = "Long text wraps to the width of its container, so resizing the window reflows it.",
                TextColor = Muted,
            });
            return stack;
        }
    }
}
