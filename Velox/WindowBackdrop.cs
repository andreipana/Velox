namespace Velox
{
    // System backdrop material drawn behind a window's transparent areas.
    // Requires Windows 11 22H2 (build 22621) or later; on older builds the window stays solid.
    public enum WindowBackdrop
    {
        // No backdrop material.
        None,
        // Mica: subtle, wallpaper-tinted material for long-lived main windows.
        Mica,
        // Acrylic: blurred translucent material, intended for transient windows.
        Acrylic,
        // Mica Alt: stronger-tinted Mica, intended for windows with a tab strip.
        MicaAlt,
    }
}
