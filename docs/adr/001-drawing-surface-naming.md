# ADR 001: Naming the per-frame drawing surface interface

**Date:** 2026-05-15  
**Status:** Accepted

## Context

The library needed a public interface representing the drawing surface passed to controls each frame — a thin abstraction over Direct2D's `RenderTarget` that shields higher-level controls (`Button`, `TextBlock`, etc.) from SharpDX types. Three names were considered.

### Canvas

`Canvas` is the term used by Android (`android.graphics.Canvas`) and Skia (`SkCanvas`) for exactly this concept: a drawing surface passed into a paint callback. However, in the Windows/.NET ecosystem "Canvas" is a *layout panel* — a container for placing child elements at absolute coordinates (WPF's `Canvas`, WinUI's `Canvas`). Using it here would confuse Windows developers who expect a container, not a drawing primitive.

### DrawingContext

`DrawingContext` is WPF's own name for the equivalent object — the argument received in `UIElement.OnRender(DrawingContext dc)`. It is semantically accurate and familiar to WPF developers. The downside is that "context" in graphics often implies a persistent, stateful GPU handle (as in an OpenGL context or a D3D device context), whereas this object is a lightweight, per-frame drawing surface with no persistent state.

### Painter

`Painter` is Qt's term (`QPainter`). It communicates the intent well — "the thing you use to paint one frame" — but is unfamiliar to .NET developers and imports Qt idioms into a Windows-native library.

## Decision

Use `Graphics`, naming after `System.Drawing.Graphics` — the .NET type that represents the exact same concept: a drawing surface passed into `Control.OnPaint(PaintEventArgs e)`, where `e.Graphics` is used to issue draw calls. Every .NET developer recognises this pattern immediately, and it requires no explanation.

- Public interface: `IGraphics` (in `Velox`)  
- Direct2D implementation: `DirectXGraphics` (in `Velox`, internal)  
- Controls receive `IGraphics graphics` in their `Render` method

## Consequences

- The name is consistent with .NET idioms and carries no platform-alien connotations.
- `DirectXGraphics` remains internal to `Velox`; controls in `Velox.Controls` only ever see `IGraphics` and are fully decoupled from SharpDX.
- If the rendering backend is ever replaced (e.g. Vulkan, software rasteriser), the interface name remains meaningful without change.
