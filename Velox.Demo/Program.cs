using Velox;

namespace Velox.Demo
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            var application = new Application();
            _ = application.CreateWindow();
            application.Run();
        }
    }
}