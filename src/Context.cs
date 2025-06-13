using Veldrid;

using Veldrid.Sdl2;
using ImGuiNET;

namespace ImGui.NetBase;

internal sealed class Context : IDisposable
{
    public Sdl2Window Window { get; }
    public GraphicsDevice GraphicsDevice { get; }
    public CommandList CommandList { get; }
    public UIRenderer UIRenderer { get; }
    public ImGuiIOPtr IO { get; }

    public Context(Sdl2Window window, GraphicsDevice graphicsDevice, CommandList commandList, UIRenderer renderer, ImGuiIOPtr io)
    {
        Window = window;
        GraphicsDevice = graphicsDevice;
        CommandList = commandList;
        UIRenderer = renderer;
        IO = io;
    }

    public void Dispose()
    {
        UIRenderer.Dispose();
        CommandList.Dispose();
        GraphicsDevice.Dispose();
    }
}
