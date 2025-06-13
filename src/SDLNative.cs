using System.Runtime.InteropServices;
using Veldrid.Sdl2;
namespace ImGui.NetBase;

public static class SdlNative
{
    private const string Sdl2LibraryName = "SDL2";

    [DllImport(Sdl2LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_GL_GetDrawableSize(IntPtr window, out int w, out int h);

    [DllImport(Sdl2LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_WaitEventTimeout(out SDL_Event sdlEvent, int timeout);

    [DllImport(Sdl2LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_PushEvent(ref SDL_Event sdlEvent);

    [DllImport(Sdl2LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_PollEvent(out SDL_Event sdlEvent);

    [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_PeepEvents(IntPtr events, int numevents, SDL_eventaction action, uint minType, uint maxType);

    [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_GetDisplayBounds(int displayIndex, out SDL_Rect rect);

    [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_Init(uint flags);

    [DllImport(Sdl2LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_bool SDL_SetHint(string name, string value);

    public static void SetClipboard(string text)
    {
        Sdl2Native.SDL_SetClipboardText(text);
    }

    public static string GetClipboard()
    {
        return Sdl2Native.SDL_GetClipboardText();
    }

    public enum SDL_bool
    {
        SDL_FALSE = 0,
        SDL_TRUE = 1
    }

    [Flags]
    public enum SDL_InitFlags : uint
    {
        Timer = 0x00000001,
        Audio = 0x00000010,
        Video = 0x00000020,
        Joystick = 0x00000200,
        Haptic = 0x00001000,
        GameController = 0x00002000,
        Events = 0x00004000,
        Everything = 0x0000FFFF
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_Rect
    {
        public int x;
        public int y;
        public int w;
        public int h;
    }

    public enum SDL_eventaction
    {
        SDL_ADDEVENT,
        SDL_PEEKEVENT,
        SDL_GETEVENT
    }
}