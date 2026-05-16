using Velox;
using Velox.Controls;

namespace Velox.Demo.LogViewer
{
    internal sealed class LogViewerWindow : Window
    {
        private const float Margin     =  8f;
        private const float ScrollBarW = 12f;

        private readonly IReadOnlyList<LogEntry> _lines;
        private readonly LogView   _logView;
        private readonly ScrollBar _scrollBar;

        public LogViewerWindow()
        {
            Size = (1024, 768);

            _lines     = LoadLog(Path.Combine(AppContext.BaseDirectory, "Data", "i2.log"));
            _logView   = new LogView   { Lines = _lines };
            _scrollBar = new ScrollBar { Min = 0f, Width = ScrollBarW };

            _scrollBar.ValueChanged += (_, v) => _logView.FirstLine = v;
            Resized += (_, _) => UpdateLayout();

            UpdateLayout();

            AddControl(_logView);
            AddControl(_scrollBar);
        }

        private void UpdateLayout()
        {
            var (w, h) = ClientSizeDip;

            _logView.X      = Margin;
            _logView.Y      = Margin;
            _logView.Width  = w - Margin - ScrollBarW - Margin;
            _logView.Height = h - 2 * Margin;

            _scrollBar.X      = w - Margin - ScrollBarW;
            _scrollBar.Y      = Margin;
            _scrollBar.Height = h - 2 * Margin;

            int visible = _logView.VisibleLineCount;
            _scrollBar.ViewportSize = visible;
            _scrollBar.Max          = Math.Max(0, _lines.Count - visible);
            _scrollBar.SetValue(_scrollBar.Value);
            _logView.FirstLine = _scrollBar.Value;
        }

        private static IReadOnlyList<LogEntry> LoadLog(string path)
        {
            var result = new List<LogEntry>();
            foreach (var line in File.ReadAllLines(path))
            {
                var p = line.Split('|', 4);
                if (p.Length >= 4) result.Add(new LogEntry(p[0], p[1], p[3]));
            }
            return result;
        }
    }
}
