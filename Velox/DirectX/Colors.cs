namespace Velox.DirectX
{
    internal static class Colors
    {
        public static D2D1_COLOR_F ToColor(uint argb) => new D2D1_COLOR_F(
            ((argb >> 16) & 0xFF) / 255f,
            ((argb >>  8) & 0xFF) / 255f,
            ((argb >>  0) & 0xFF) / 255f,
            ((argb >> 24) & 0xFF) / 255f);
    }
}
