using Veldrid;
using Veldrid.ImGui;
using Veldrid.Sdl2;
using ImGuiNET;

namespace ImGui.NetBase;

internal sealed class Context : IDisposable
{
    public Sdl2Window Window { get; }
    public GraphicsDevice GraphicsDevice { get; }
    public CommandList CommandList { get; }
    public ImGuiController Controller { get; }
    public ImGuiIOPtr IO { get; }

    public Context(Sdl2Window window, GraphicsDevice graphicsDevice, CommandList commandList, ImGuiController controller, ImGuiIOPtr io)
    {
        Window = window;
        GraphicsDevice = graphicsDevice;
        CommandList = commandList;
        Controller = controller;
        IO = io;
    }

    public void Dispose()
    {
        Controller.Dispose();
        CommandList.Dispose();
        GraphicsDevice.Dispose();
    }
}
