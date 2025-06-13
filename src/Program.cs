namespace ImGui.NetBase;

internal static class Program
{
    private static void Main()
    {   
        Context ctx = UI.Initialize();

        while (ctx.Window.Exists)
        {
            UI.ProcessEvents(ctx);
            UI.Render(ctx);
        }

        unsafe
        {
            try
            {
                while (ctx.Window.Exists && !ctx.QuitNow)
                {
                    int pending = SdlNative.SDL_PeepEvents(IntPtr.Zero, 1, SdlNative.SDL_eventaction.SDL_PEEKEVENT, (uint)Veldrid.Sdl2.SDL_EventType.FirstEvent, (uint)Veldrid.Sdl2.SDL_EventType.LastEvent);
                    if (pending > 0)
                    {
                        while (SdlNative.SDL_PollEvent(out Veldrid.Sdl2.SDL_Event e) == 1)
                        {
                            if (e.type == Veldrid.Sdl2.SDL_EventType.Quit)
                            {
                                ctx.QuitNow = true;
                            }

                            ctx.Window.HandleEvent(&e);
                        }

                        UI.ProcessEvents(ctx);
                        UI.Render(ctx);
                    }
                    else if (ctx.NeedsRender || ImGuiNET.ImGui.GetIO().WantTextInput || ImGuiNET.ImGui.IsAnyItemActive() || ImGuiNET.ImGui.IsPopupOpen(string.Empty, ImGuiNET.ImGuiPopupFlags.AnyPopupId))
                    {
                        ctx.NeedsRender = false;
                        UI.ProcessEvents(ctx);
                        UI.Render(ctx);
                        Thread.Sleep(16);
                    }
                    else
                    {
                        // No events and nothing to render: wait for next event
                        if (SdlNative.SDL_WaitEventTimeout(out Veldrid.Sdl2.SDL_Event e, 100) == 1)
                        {
                            if (e.type == Veldrid.Sdl2.SDL_EventType.Quit)
                            {
                                ctx.QuitNow = true;
                            }

                            ctx.Window.HandleEvent(&e);
                            UI.ProcessEvents(ctx);
                            UI.Render(ctx);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unhandled Exception:");
                Console.Error.WriteLine(ex.ToString());
                Environment.Exit(1);
            }
        }

        UI.Shutdown(ctx);
    }
}
