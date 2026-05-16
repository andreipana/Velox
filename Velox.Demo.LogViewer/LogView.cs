namespace Velox.Demo.LogViewer
{
    public readonly record struct LogEntry(string Timestamp, string Level, string Message);

    public class LogView : Velox.Controls.Control
    {
        public IReadOnlyList<LogEntry> Lines { get; set; } = Array.Empty<LogEntry>();
        public float FirstLine { get; set; } = 0f;

        public int VisibleLineCount => (int)Math.Floor(Height / LineHeight);

        private int _hoveredLine = -1;

        private const float  LineHeight  = 20f;
        private const string FontFace    = "Cascadia Code";
        private const float  FontSize    = 12f;
        private const float  PaddingL    =  8f;
        private const float  TimestampW  = 200f;
        private const float  LevelW      =  44f;
        private const float  ColGap      =   4f;

        public override void Render(Velox.IGraphics graphics)
        {
            if (!IsVisible) return;

            graphics.PushClip(X, Y, Width, Height);

            int firstLineInt = (int)Math.Floor(FirstLine);
            int count        = VisibleLineCount;
            float msgX       = X + PaddingL + TimestampW + ColGap + LevelW + ColGap;
            float msgW       = Width - (msgX - X) - PaddingL;

            for (int i = 0; i < count; i++)
            {
                int lineIdx = firstLineInt + i;
                if (lineIdx >= Lines.Count) break;

                float rowY = Y + i * LineHeight;

                if (lineIdx == _hoveredLine)
                    graphics.FillRect(X, rowY, Width, LineHeight, 0x14FFFFFFU);
                else if (lineIdx % 2 == 1)
                    graphics.FillRect(X, rowY, Width, LineHeight, 0x0D000000U);

                var entry  = Lines[lineIdx];
                float textY = rowY + (LineHeight - FontSize) / 2f;

                graphics.DrawText(entry.Timestamp, FontFace, FontSize,
                    X + PaddingL, textY, TimestampW, LineHeight, 0xFF9D9D9DU, noWrap: true);

                uint levelColor = entry.Level switch
                {
                    "WARN"  => 0xFFFFCC00U,
                    "ERROR" => 0xFFFF5F5FU,
                    _       => 0xFF60CDFFU,
                };
                graphics.DrawText(entry.Level, FontFace, FontSize,
                    X + PaddingL + TimestampW + ColGap, textY, LevelW, LineHeight, levelColor, noWrap: true);

                uint msgColor = entry.Level switch
                {
                    "WARN"  => 0xFFFFEEAAU,
                    "ERROR" => 0xFFFFCCCCU,
                    _       => 0xFFE8E8E8U,
                };
                graphics.DrawText(entry.Message, FontFace, FontSize,
                    msgX, textY, msgW, LineHeight, msgColor, noWrap: true);
            }

            graphics.PopClip();
        }

        public override void OnMouseMove(float px, float py)
        {
            if (!HitTest(px, py)) { _hoveredLine = -1; return; }
            int firstLineInt = (int)Math.Floor(FirstLine);
            int idx = firstLineInt + (int)Math.Floor((py - Y) / LineHeight);
            _hoveredLine = idx < Lines.Count ? idx : -1;
        }

        public override void OnMouseLeave() => _hoveredLine = -1;
    }
}
