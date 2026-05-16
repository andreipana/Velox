using Velox;

namespace Velox.Demo.LogViewer
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            var application = new Application();
            var window = new LogViewerWindow();
            window.Size = (1024, 768);
            application.Run();
        }
    }
}
