namespace Velox.DirectX
{
    internal sealed class NativeBitmap : Velox.INativeBitmap
    {
        internal IntPtr Handle;

        public int Width  { get; }
        public int Height { get; }

        internal NativeBitmap(IntPtr handle, int width, int height)
        {
            Handle = handle;
            Width  = width;
            Height = height;
        }

        public void Dispose()
        {
            if (Handle != IntPtr.Zero)
            {
                D2D1Vtbl.Release(Handle);
                Handle = IntPtr.Zero;
            }
        }
    }
}
