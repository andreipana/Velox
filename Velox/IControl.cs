namespace Velox
{
    public interface IControl
    {
        bool IsVisible { get; }

        void Render(IGraphics graphics);
        bool HitTest(float px, float py);
        void OnMouseMove(float px, float py);
        void OnMouseDown(float px, float py);
        void OnMouseUp(float px, float py);
        void OnMouseLeave();
    }
}
