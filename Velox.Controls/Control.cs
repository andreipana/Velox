namespace Velox.Controls
{
    public abstract class Control : Velox.IControl
    {
        public float X      { get; set; }
        public float Y      { get; set; }
        public float Width  { get; set; }
        public float Height { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsVisible { get; set; } = true;

        public void Render(Velox.IGraphics graphics)
        {
            if (!IsVisible) return;
            graphics.PushTranslation(X, Y);
            try { OnRender(graphics); }
            finally { graphics.PopTranslation(); }
        }

        protected abstract void OnRender(Velox.IGraphics graphics);

        public virtual bool HitTest(float px, float py)
            => IsVisible && IsEnabled
               && px >= X && px < X + Width
               && py >= Y && py < Y + Height;

        public virtual void OnMouseMove(float px, float py)       { }
        public virtual void OnMouseDown(float px, float py)       { }
        public virtual void OnMouseUp(float px, float py)         { }
        public virtual void OnRightMouseDown(float px, float py)  { }
        public virtual void OnRightMouseUp(float px, float py)    { }
        public virtual void OnMouseLeave()                        { }
        public virtual void OnMouseWheel(float delta)             { }
    }
}
