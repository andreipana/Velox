namespace Velox
{
    public interface IGraphics
    {
        float Width  { get; }
        float Height { get; }

        void FillRect(float x, float y, float w, float h, uint argbColor);
        void DrawRect(float x, float y, float w, float h, uint argbColor, float strokeWidth = 1f);
        void FillRoundedRect(float x, float y, float w, float h, float cornerRadius, uint argbColor);
        void DrawRoundedRect(float x, float y, float w, float h, float cornerRadius, uint argbColor, float strokeWidth = 1f);
        void DrawLine(float x1, float y1, float x2, float y2, uint argbColor, float strokeWidth = 1f);

        void DrawText(string text, string fontFace, float fontSize,
                      float x, float y, float maxWidth, float maxHeight, uint argbColor,
                      bool noWrap = false);

        (float width, float height) MeasureText(string text, string fontFace, float fontSize,
                                                float maxWidth, float maxHeight);

        void DrawBitmap(IntPtr scan0, int srcWidth, int srcHeight, int stride,
                        float dstX, float dstY, float dstW, float dstH);

        void PushClip(float x, float y, float w, float h);
        void PopClip();

        void PushTranslation(float dx, float dy);
        void PopTranslation();

        void PushScale(float scaleX, float scaleY, float centerX, float centerY);
        void PopScale();
    }
}
