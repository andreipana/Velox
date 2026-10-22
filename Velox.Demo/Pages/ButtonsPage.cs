using Velox;
using Velox.Controls;

namespace Velox.Demo.Pages
{
    // Starting point for the Button-focused page; to be filled out with the Button work.
    public class ButtonsPage : IPage
    {
        public IControl Build()
        {
            var status = new TextBlock { Text = "Press a button...", TextColor = DemoTheme.Muted };

            var button = new Button { Width = 140, Height = 36, Text = "Button" };
            button.Click += (_, _) => status.Text = "Clicked";

            var stack = new StackPanel { Spacing = 12 };
            stack.Add(new TextBlock { Text = "Buttons", FontSize = 18f });
            stack.Add(button);
            stack.Add(status);
            return stack;
        }
    }
}
