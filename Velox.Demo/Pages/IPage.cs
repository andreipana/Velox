using Velox;

namespace Velox.Demo.Pages
{
    // A screen of the demo, hosted in the window below the backdrop row.
    public interface IPage
    {
        // Builds the page's root control. It is arranged to fill the area below the header.
        IControl Build();
    }
}
