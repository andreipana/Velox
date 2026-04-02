using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using static Velox.Colors;

namespace Velox
{
    public enum SizeMode
    {
        /// <summary>Width and Height are explicit bounds — text wraps and clips to fit.</summary>
        Fixed,
        /// <summary>Size is computed from text content. Width and Height are outputs (ActualWidth/ActualHeight), not inputs.</summary>
        Auto,
        /// <summary>Parent sets Width/Height as maximum available constraints before calling Render. Text wraps within those bounds.</summary>
        Fill,
    }

    internal class TextBlock
    {
        public string Text { get; set; } = "";

        public RawColor4 Color { get; set; } = ToRawColor4(0xFFFFFFFF);

        public RawColor4? Background { get; set; }

        public float X { get; set; } = 0;
        public float Y { get; set; } = 0;

        /// <summary>
        /// Meaning depends on SizeMode:
        ///   Fixed — exact bound (text clips at this width)
        ///   Auto  — ignored; computed from text content
        ///   Fill  — maximum available width supplied by parent
        /// </summary>
        public float Width { get; set; } = 0;

        /// <summary>
        /// Meaning depends on SizeMode:
        ///   Fixed — exact bound (text clips at this height)
        ///   Auto  — ignored; computed from text content
        ///   Fill  — maximum available height supplied by parent
        /// </summary>
        public float Height { get; set; } = 0;

        public SizeMode SizeMode { get; set; } = SizeMode.Auto;

        /// <summary>Actual rendered width — valid after Render() is called.</summary>
        public float ActualWidth { get; private set; }

        /// <summary>Actual rendered height — valid after Render() is called.</summary>
        public float ActualHeight { get; private set; }

        public void Render(DirectXRenderingSystem renderingSystem, RenderTarget renderTarget, float parentWidth, float parentHeight)
        {
            using var textFormat = renderingSystem.CreateTextFormat("Cascadia Code", 14f, FontWeight.UltraLight);
            textFormat.ParagraphAlignment = ParagraphAlignment.Center;
            textFormat.TextAlignment = TextAlignment.Center;

            using var textBrush = new SolidColorBrush(renderTarget, Color);

            float maxWidth = Width;
            float maxHeight = Height;
            switch (SizeMode)
            {
                case SizeMode.Fill:
                    maxWidth = parentWidth;
                    maxHeight = parentHeight;
                    break;

                case SizeMode.Auto:
                    maxWidth = float.PositiveInfinity;
                    maxHeight = float.PositiveInfinity;
                    break;
            }

            using var layout = renderingSystem.CreateWpfCompatibleLayout(textFormat, Text, maxWidth, maxHeight);

            var metrics = layout.Metrics;
            ActualWidth  = metrics.Width;
            ActualHeight = metrics.Height;

            if (Background.HasValue)
            {
                using var bgBrush = new SolidColorBrush(renderTarget, Background.Value);
                renderTarget.FillRectangle(new SharpDX.Mathematics.Interop.RawRectangleF(X, Y, X + ActualWidth, Y + ActualHeight), bgBrush);
            }

            var drawOptions = SizeMode == SizeMode.Fixed ? DrawTextOptions.Clip : DrawTextOptions.None;

            renderTarget.DrawTextLayout(
                new RawVector2(X, Y),
                layout,
                textBrush,
                drawOptions
            );
        }
    }
}
