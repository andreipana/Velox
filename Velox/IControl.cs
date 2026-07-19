namespace Velox
{
    public interface IControl
    {
        bool IsVisible { get; }

        // Returns the control's desired size given the available space.
        // Called by panels during layout; graphics is needed for text measurement.
        (float desiredW, float desiredH) Measure(IGraphics graphics, float availableW, float availableH);

        // Sets the control's position and size. Called by panels after Measure.
        void Arrange(float x, float y, float w, float h);

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
