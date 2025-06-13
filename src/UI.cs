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
    private const string ConfigFile = "imgui.ini";

    public static Context Context { get; private set; } = null!;

    [DllImport("user32.dll")]
    private static extern bool SetProcessDpiAwarenessContext(IntPtr context);
    private static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new IntPtr(-4);

    public static bool Exists => Context.Window.Exists;

    public static unsafe void Initialize()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);

        SdlNative.SDL_Init((uint)SdlNative.SDL_InitFlags.Video);

        SdlNative.SDL_GetDisplayBounds(0, out SdlNative.SDL_Rect bounds);
        float initialWidth = bounds.w * 0.8f;
        float initialHeight = bounds.h * 0.8f;
        int x = bounds.x + (bounds.w - (int)initialWidth) / 2;
        int y = bounds.y + (bounds.h - (int)initialHeight) / 2;

        string? windowTitle = Assembly.GetExecutingAssembly().GetName().Name;
        WindowCreateInfo windowCI = new(
            x, y,
            (int)initialWidth, (int)initialHeight,
            WindowState.Normal,
            string.IsNullOrEmpty(windowTitle) ? "Unknown Log Viewer" : windowTitle);

        Sdl2Window window = new Sdl2Window(
            windowCI.WindowTitle,
            windowCI.X,
            windowCI.Y,
            windowCI.WindowWidth,
            windowCI.WindowHeight,
            SDL_WindowFlags.Resizable | SDL_WindowFlags.AllowHighDpi,
            false);

        int drawableWidth, drawableHeight;
        SdlNative.SDL_GL_GetDrawableSize(window.SdlWindowHandle, out drawableWidth, out drawableHeight);

        GraphicsDeviceOptions gdOptions = new GraphicsDeviceOptions(
            debug: false,
            syncToVerticalBlank: true,
            resourceBindingModel: ResourceBindingModel.Improved,
            swapchainDepthFormat: null);

        Veldrid.GraphicsBackend backend = OperatingSystem.IsMacOS() ? Veldrid.GraphicsBackend.Metal : Veldrid.GraphicsBackend.Direct3D11;
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
        Context = new Context(window, gd, cl, uiRenderer, io);

        window.Resized += () =>
        {
            SdlNative.SDL_GL_GetDrawableSize(window.SdlWindowHandle, out int newDrawableWidth, out int newDrawableHeight);
            gd.MainSwapchain.Resize((uint)newDrawableWidth, (uint)newDrawableHeight);

            uiRenderer.WindowResized(newDrawableWidth, newDrawableHeight); // Use pixel width/height

            float updatedScaleX = (float)newDrawableWidth / window.Width;
            float updatedScaleY = (float)newDrawableHeight / window.Height;
            io.DisplayFramebufferScale = new Vector2(updatedScaleX, updatedScaleY);
        };
    }

    public static void ProcessEvents()
    {
        InputSnapshot snapshot = Context.Window.PumpEvents();
        if (!Context.Window.Exists)
        {
            return;
        }

        Context.UIRenderer.Update(1f / 60f, snapshot);
    }

    public static void Render()
    {
        HelloView.Render();

        Context.CommandList.Begin();
        Context.CommandList.SetFramebuffer(Context.GraphicsDevice.MainSwapchain.Framebuffer);
        Context.CommandList.ClearColorTarget(0, RgbaFloat.Black);
        Context.UIRenderer.Render(Context.GraphicsDevice, Context.CommandList);
        Context.CommandList.End();

        Context.GraphicsDevice.SubmitCommands(Context.CommandList);
        Context.GraphicsDevice.SwapBuffers(Context.GraphicsDevice.MainSwapchain);
    }

    public static void Shutdown()
    {
        Context.GraphicsDevice.WaitForIdle();
        Context.Dispose();
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
