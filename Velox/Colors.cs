using SharpDX.Mathematics.Interop;

namespace Velox
{
    public readonly struct Colors
    {
        public readonly RawColor4 Value { get; init; }

        public static implicit operator Colors(uint color) => new Colors { Value = ToRawColor4(color) };
        public static implicit operator RawColor4(Colors c) => c.Value;

        public static RawColor4 ToRawColor4(uint color)
        {
            return new RawColor4(
                ((color >> 16) & 0xFF) / 255f,  // R
                ((color >> 8) & 0xFF) / 255f,  // G
                ((color >> 0) & 0xFF) / 255f,  // B
                ((color >> 24) & 0xFF) / 255f   // A
            );
        }
    }
}
