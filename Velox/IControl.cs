namespace Velox
{
    public interface IControl
    {
        bool IsVisible { get; }

        IControl? ToolTip { get; set; }

        // Returns the control's desired size given the available space.
        // Called by panels during layout; graphics is needed for text measurement.
        (float desiredW, float desiredH) Measure(IGraphics graphics, float availableW, float availableH);

        // Sets the control's position and size. Called by panels after Measure.
        void Arrange(float x, float y, float w, float h);

        void Render(IGraphics graphics);
        bool HitTest(float px, float py);

        // Finds the innermost control whose bounds contain (px, py), in the same
        // coordinate space as HitTest. Containers recurse into their children (so
        // the most specific control wins) before falling back to themselves.
        // Currently used to resolve which control's ToolTip to show under the
        // cursor, but the traversal itself is generic (not tooltip-specific).
        IControl? HitTestDeep(float px, float py);
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
