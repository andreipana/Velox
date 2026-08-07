using Velox;
using Velox.Controls;

namespace Velox.Demo
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            var application = new Application();
            var window = application.CreateWindow();
            window.Size = (900, 650);

            // --- Button demo ---

            var title = new TextBlock
            {
                X = 40, Y = 40,
                Text = "Velox Controls Demo",
                FontSize = 20f,
            };

            var status = new TextBlock
            {
                X = 40, Y = 180,
                Text = "Press a button...",
                FontSize = 14f,
                TextColor = 0xFFAAAAAAU,
            };

            var helloButton = new Button
            {
                X = 40, Y = 110, Width = 140, Height = 36,
                Text = "Say Hello",
            };

            var resetButton = new Button
            {
                X = 196, Y = 110, Width = 100, Height = 36,
                Text = "Reset",
            };

            helloButton.Click += (_, _) => status.Text = "Hello, World!";
            resetButton.Click += (_, _) => status.Text = "Press a button...";

            // --- ScrollBar demo ---

            var scrollLabel = new TextBlock
            {
                X = 280, Y = 40,
                Text = "Scroll: 0",
                FontSize = 14f,
                TextColor = 0xFFAAAAAAU,
            };

            var scrollBar = new ScrollBar
            {
                X = 340, Y = 40,
                Width = 12, Height = 200,
                Min = 0, Max = 100, ViewportSize = 20,
            };

            scrollBar.ValueChanged += (_, v) => scrollLabel.Text = $"Scroll: {(int)v}";

            var glyph = new Glyph()
            {
                Symbol = "\uE8C8", // Copy
                FontFace = "Segoe MDL2 Assets",
                FontSize = 32f,
                Color = 0xFF0000FF,
                X = 360,
                Y = 40
            };

            window.AddControl(title);
            window.AddControl(helloButton);
            window.AddControl(resetButton);
            window.AddControl(status);
            window.AddControl(scrollLabel);
            window.AddControl(scrollBar);
            window.AddControl(glyph);

            // --- Image demo ---

            string imagesDir = Path.Combine(AppContext.BaseDirectory, "images");

            var pngImage1 = new Image { X = 480, Y = 40, Source = new Bitmap(Path.Combine(imagesDir, "1.png")) };
            var pngImage2 = new Image { X = 690, Y = 40, Source = new Bitmap(Path.Combine(imagesDir, "2.png")) };
            var jpgImage  = new Image
            {
                X = 40, Y = 280, Width = 380, Height = 286,
                Source = new Bitmap(Path.Combine(imagesDir, "3.jpg")),
            };

            window.AddControl(pngImage1);
            window.AddControl(pngImage2);
            window.AddControl(jpgImage);

            application.Run();
        }
    }
}
