using System;
using System.Runtime.InteropServices;
using ImGuiNET;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Veldrid.ImGui;

namespace ImGui.NetBase;

internal static class UI
{
    private const string ConfigFile = "imgui.ini";

    public static Context Context { get; private set; } = null!;

    [DllImport("user32.dll")]
    private static extern bool SetProcessDpiAwarenessContext(IntPtr context);
    private static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new IntPtr(-4);

    public static bool Exists => Context.Window.Exists;

    public static unsafe void Initialize()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Enable per-monitor DPI awareness
            SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
        }

        var windowCI = new WindowCreateInfo(100, 100, 1280, 720, "ImGui.NetBase");
        Sdl2Window window = VeldridStartup.CreateWindow(ref windowCI);

        GraphicsBackend backend = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? GraphicsBackend.Direct3D11 :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? GraphicsBackend.Metal : GraphicsBackend.OpenGL;

        GraphicsDeviceOptions options = new GraphicsDeviceOptions(
            debug: false,
            swapchainDepthFormat: null,
            syncToVerticalBlank: true,
            resourceBindingModel: ResourceBindingModel.Improved,
            preferredBackend: backend);
        GraphicsDevice gd = VeldridStartup.CreateGraphicsDevice(window, options);
        CommandList cl = gd.ResourceFactory.CreateCommandList();

        ImGui.CreateContext();
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        io.IniFilename = ConfigFile;

        io.Fonts.AddFontDefault();

        ImGuiController controller = new(gd, gd.MainSwapchain.Framebuffer.OutputDescription, window.Width, window.Height);
        controller.RecreateFontDeviceTexture();

        Context = new Context(window, gd, cl, controller, io);

        Context.Window.Resized += () =>
        {
            Context.GraphicsDevice.MainSwapchain.Resize((uint)Context.Window.Width, (uint)Context.Window.Height);
            Context.Controller.WindowResized(Context.Window.Width, Context.Window.Height);
        };
    }

    public static void ProcessEvents()
    {
        InputSnapshot snapshot = Context.Window.PumpEvents();
        if (!Context.Window.Exists)
        {
            return;
        }

        Context.Controller.Update(1f / 60f, snapshot);
    }

    public static void Render()
    {
        HelloView.Render();

        Context.CommandList.Begin();
        Context.CommandList.SetFramebuffer(Context.GraphicsDevice.MainSwapchain.Framebuffer);
        Context.CommandList.ClearColorTarget(0, RgbaFloat.Black);
        Context.Controller.Render(Context.GraphicsDevice, Context.CommandList);
        Context.CommandList.End();

        Context.GraphicsDevice.SubmitCommands(Context.CommandList);
        Context.GraphicsDevice.SwapBuffers(Context.GraphicsDevice.MainSwapchain);
    }

    public static void Shutdown()
    {
        Context.GraphicsDevice.WaitForIdle();
        Context.Dispose();
    }
}
