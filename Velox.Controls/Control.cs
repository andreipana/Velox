namespace Velox.Controls
{
    public abstract class Control : Velox.IControl
    {
        public float X { get; set; }
        public float Y { get; set; }

        public float? MinWidth  { get; set; }
        public float? MaxWidth  { get; set; }
        public float? MinHeight { get; set; }
        public float? MaxHeight { get; set; }

        private float _width;
        private float _height;

        public float Width
        {
            get => _width;
            set
            {
                _width = MinWidth.HasValue ? Math.Max(value, MinWidth.Value) : value;
                if (MaxWidth.HasValue) _width = Math.Min(_width, MaxWidth.Value);
            }
        }

        public float Height
        {
            get => _height;
            set
            {
                _height = MinHeight.HasValue ? Math.Max(value, MinHeight.Value) : value;
                if (MaxHeight.HasValue) _height = Math.Min(_height, MaxHeight.Value);
            }
        }

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
        public virtual void OnMouseWheel(float delta, float px, float py) { }
        public virtual void OnKeyDown(VirtualKey key, bool ctrl, bool shift, bool alt) { }
    }
}
