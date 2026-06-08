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
        void OnRightMouseDown(float px, float py);
        void OnRightMouseUp(float px, float py);
        void OnMouseLeave();
        void OnMouseWheel(float delta, float px, float py);
        void OnKeyDown(VirtualKey key, bool ctrl, bool shift, bool alt);
    }
}
