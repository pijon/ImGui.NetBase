# ImGui.NetBase

This repository contains a minimal example of using [ImGui.NET](https://github.com/ocornut/imgui) on Windows, macOS and Linux.

Features demonstrated:

- **DPI awareness** using `SetProcessDpiAwarenessContext` on Windows.
- **Input handling** powered by Veldrid's SDL2 windowing.
- **Cross-platform** rendering using Direct3D11 on Windows, Metal on macOS and OpenGL on Linux.

The project targets .NET 8 and relies on the `ImGui.NET`, `Veldrid`, `Veldrid.StartupUtilities` and `Veldrid.ImGui` NuGet packages. Build it with `dotnet build` on your platform of choice.

