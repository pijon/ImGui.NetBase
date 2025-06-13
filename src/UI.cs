using System.Runtime.InteropServices;
using ImGuiNET;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using System.Reflection;
using System.Numerics;

namespace ImGui.NetBase;

internal static class UI
{
    [DllImport("user32.dll")]
    private static extern bool SetProcessDpiAwarenessContext(IntPtr context);
    private static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new IntPtr(-4);

    public static unsafe Context Initialize()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);

        SdlNative.SDL_Init((uint)SdlNative.SDL_InitFlags.Video);

        SdlNative.SDL_GetDisplayBounds(0, out SdlNative.SDL_Rect bounds);
        float initialWidth = bounds.w * 0.5f;
        float initialHeight = bounds.h * 0.5f;
        int x = bounds.x + (bounds.w - (int)initialWidth) / 2;
        int y = bounds.y + (bounds.h - (int)initialHeight) / 2;

        Sdl2Window window = new Sdl2Window(
            "ImGui.NetBase",
            x,
            y,
            (int)initialWidth,
            (int)initialHeight,
            SDL_WindowFlags.Resizable | SDL_WindowFlags.AllowHighDpi,
            false);

        int drawableWidth, drawableHeight;
        SdlNative.SDL_GL_GetDrawableSize(window.SdlWindowHandle, out drawableWidth, out drawableHeight);

        GraphicsDeviceOptions gdOptions = new GraphicsDeviceOptions(
            debug: false,
            syncToVerticalBlank: true,
            resourceBindingModel: ResourceBindingModel.Improved,
            swapchainDepthFormat: null);

        GraphicsBackend backend = OperatingSystem.IsMacOS() ? Veldrid.GraphicsBackend.Metal : Veldrid.GraphicsBackend.Direct3D11;
        GraphicsDevice gd = VeldridStartup.CreateGraphicsDevice(window, gdOptions, backend);

        foreach (string name in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            Console.WriteLine(name);

        float scaleX = (float)drawableWidth / window.Width;
        float scaleY = (float)drawableHeight / window.Height;
        float dpiScale = MathF.Max(scaleX, scaleY);

        CommandList cl = gd.ResourceFactory.CreateCommandList();
        gd.MainSwapchain.Resize((uint)drawableWidth, (uint)drawableHeight);

        UIRenderer uiRenderer = new(
            gd,
            gd.MainSwapchain.Framebuffer.OutputDescription,
            drawableWidth,
            drawableHeight,
            dpiScale);

        ImGuiIOPtr io = ImGuiNET.ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard | ImGuiConfigFlags.NavEnableGamepad;
        io.DisplayFramebufferScale = new Vector2(dpiScale, dpiScale);
        Context ctx = new Context(window, gd, cl, uiRenderer, io);

        window.Resized += () =>
        {
            SdlNative.SDL_GL_GetDrawableSize(window.SdlWindowHandle, out int newDrawableWidth, out int newDrawableHeight);
            gd.MainSwapchain.Resize((uint)newDrawableWidth, (uint)newDrawableHeight);

            uiRenderer.WindowResized(newDrawableWidth, newDrawableHeight);

            float updatedScaleX = (float)newDrawableWidth / window.Width;
            float updatedScaleY = (float)newDrawableHeight / window.Height;
            io.DisplayFramebufferScale = new Vector2(updatedScaleX, updatedScaleY);
        };

        return ctx;
    }

    public static void ProcessEvents(Context ctx)
    {
        InputSnapshot snapshot = ctx.Window.PumpEvents();
        ctx.UIRenderer.Update(1f / 60f, snapshot);
    }

    public static void Render(Context ctx)
    {
        ImGuiNET.ImGui.SetNextWindowSize(new Vector2(ctx.Window.Width, ctx.Window.Height), ImGuiCond.Always);

        ImGuiNET.ImGui.Begin("##MainWindow", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBringToFrontOnFocus);

        HelloView.Render(ctx);

        ImGuiNET.ImGui.End();

        ImGuiNET.ImGui.Render();
        ctx.CmdList.Begin();
        ctx.CmdList.SetFramebuffer(ctx.GraphicsDevice.MainSwapchain.Framebuffer);
        ctx.CmdList.ClearColorTarget(0, RgbaFloat.Black);
        ctx.UIRenderer.Render(ctx.GraphicsDevice, ctx.CmdList);
        ctx.CmdList.End();
        ctx.GraphicsDevice.SubmitCommands(ctx.CmdList);
        ctx.GraphicsDevice.SwapBuffers(ctx.GraphicsDevice.MainSwapchain);
    }

    public static void Shutdown(Context ctx)
    {
        ctx.GraphicsDevice.WaitForIdle();
        ctx.Dispose();
    }
    
    static string GetAssemblyResourceBasePath()
    {
        string resourcesBaseDirPath;
        if (OperatingSystem.IsMacOS())
        {
            string baseDir = AppContext.BaseDirectory;
            if (baseDir.Contains(".app/Contents/MacOS/", StringComparison.OrdinalIgnoreCase))
            {
                string? bundleRoot = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(baseDir)));
                if (string.IsNullOrEmpty(bundleRoot))
                {
                    throw new InvalidOperationException($"Failed to resolve bundle root from: {baseDir}");
                }

                resourcesBaseDirPath = Path.Combine(bundleRoot, "Contents", "MacOS");
            }
            else
            {
                resourcesBaseDirPath = baseDir;
            }
        }
        else
        {
            resourcesBaseDirPath = AppContext.BaseDirectory;
        }
        return resourcesBaseDirPath;
    }
}
