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
                      float x, float y, float maxWidth, float maxHeight, uint argbColor);

        (float width, float height) MeasureText(string text, string fontFace, float fontSize,
                                                float maxWidth, float maxHeight);

        void PushClip(float x, float y, float w, float h);
        void PopClip();
    }
}
