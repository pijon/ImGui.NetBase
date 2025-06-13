namespace ImGui.NetBase;

internal static class Program
{
    private static void Main()
    {
        UI.Initialize();

        while (UI.Exists)
        {
            UI.ProcessEvents();
            UI.Render();
        }

        UI.Shutdown();
    }
}
