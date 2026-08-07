namespace Velox
{
    public interface INativeBitmap : IDisposable
    {
        int Width  { get; }
        int Height { get; }
    }
}
