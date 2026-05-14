# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Velox is a simple and fast UI library for Windows written in C#, built on Direct2D/DirectWrite via SharpDX. It targets .NET 10.0 and provides WPF-quality text rendering with hardware acceleration and Windows 11 visual effects.

## Build & Run

```powershell
dotnet build              # Build all projects
dotnet build -c Release   # Release build
dotnet run --project Velox.Demo   # Run the demo app
```

There are no tests currently.

## Architecture

The library has a small, layered architecture (~7 source files):

```
Application.Run()
  └─ Win32 message loop
       └─ Window.WndProc()
            └─ DirectXRenderer.Render()
                 └─ User-supplied Render(RenderTarget) callback
                      └─ Controls (e.g. TextBlock.Render())
```

**`Application.cs`** — Entry point. Sets per-monitor DPI awareness and drives the Win32 message pump.

**`Window.cs`** — Win32 window creation and message handling (WM_PAINT, WM_SIZE, WM_DESTROY). Applies Windows 11 Mica backdrop via DWM APIs and detects system dark/light theme from the registry.

**`DirectXRenderingSystem.cs`** — Singleton-style factory that initializes Direct2D and DirectWrite. Owns DPI calculations and creates text formats/layouts using GDI-compatible measurement to match WPF text quality.

**`DirectXRenderer.cs`** — Per-window render target lifecycle (create, resize, BeginDraw/EndDraw). Configures ClearType antialiasing with WPF-matching gamma/contrast. Transparent background allows DWM backdrop to show through.

**`Text.cs`** — `TextBlock` control with three layout modes: `Fixed`, `Auto`, and `Fill`. Supports background fills and custom colors.

**`Win32.cs`** — All P/Invoke declarations (DWM, DPI, message loop APIs).

**`Colors.cs`** — Converts `System.Drawing.Color` / ARGB to SharpDX `RawColor4`.

## Key Dependencies

- **SharpDX 4.2.0** (`SharpDX`, `SharpDX.Direct2D1`, `SharpDX.DXGI`) — DirectX interop
- **Target frameworks**: `net10.0` (library), `net10.0-windows` (demo/app)

## Important Patterns

- The library is Windows-only despite the library project targeting `net10.0`; anything using Win32 P/Invoke or SharpDX implicitly requires Windows.
- Text rendering is tuned to match WPF: GDI-compatible text layouts, ClearType with explicit gamma/contrast/enhanced contrast settings.
- Windows 11 Mica effect requires DWM backdrop attributes set before the render target is created.
- DPI scaling is handled centrally in `DirectXRenderingSystem`; always use DPI-aware measurements when adding new controls.
