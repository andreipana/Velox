using Velox;
using Velox.Controls;
using Velox.Demo.Pages;

namespace Velox.Demo
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Pick the page to host below the backdrop row: comment one line, uncomment another.
            //IPage page = new GeneralPage();
            IPage page = new ButtonsPage();

            var application = new Application();
            var window = application.CreateWindow();
            window.Title = "Velox Controls Demo";
            window.Size = (1200, 900);

            var root = new DockPanel();
            root.Add(new Padded(CreateBackdropRow(window), 16, 14, 16, 6), Dock.Top);
            root.Add(new Padded(page.Build(), 16, 0, 16, 16), Dock.Fill);
            window.Content = root;

            application.Run();
        }

        private static StackPanel CreateBackdropRow(Window window)
        {
            var header = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
            header.Add(new TextBlock { Text = "Window Backdrop:", FontSize = 20f });

            // The current backdrop's button is marked by wrapping its text in > <.
            var backdropButtons = new List<(WindowBackdrop backdrop, Button button)>();
            void UpdateBackdropButtons()
            {
                foreach (var (backdrop, button) in backdropButtons)
                    button.Text = backdrop == window.Backdrop ? $">{backdrop}<" : backdrop.ToString();
            }

            foreach (var backdrop in Enum.GetValues<WindowBackdrop>())
            {
                var button = new Button { Width = 90, Height = 30, Text = backdrop.ToString() };
                button.Click += (_, _) =>
                {
                    window.Backdrop = backdrop;
                    UpdateBackdropButtons();
                };
                backdropButtons.Add((backdrop, button));
                header.Add(button);
            }
            UpdateBackdropButtons();

            return header;
        }
    }
}
