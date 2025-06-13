# ImGui.NetBase

This repository contains a minimal example of using [ImGui.NET](https://github.com/ocornut/imgui) on Windows, macOS and Linux.

Features demonstrated:

- **DPI awareness** using `SetProcessDpiAwarenessContext` on Windows.
- **Config persistence** via `imgui.ini`.
- **Input handling** powered by Veldrid's SDL2 windowing.
- **Cross-platform** rendering using Direct3D11 on Windows, Metal on macOS and OpenGL on Linux.

The project targets .NET 8 and relies on the `ImGui.NET`, `Veldrid`, `Veldrid.StartupUtilities` and `Veldrid.ImGui` NuGet packages. Build it with `dotnet build` on your platform of choice.

A `Context` class stores references to runtime objects like `GraphicsDevice` and `ImGuiIO`. The static `UI` helper sets up the window and graphics state, processes input events and renders frames. The static `HelloView` demonstrates usage by printing **Hello World** each frame.
