using Cherris.Backends;

namespace Cherris;

public static class WindowManager
{
    public static Vector2 OriginalSize = Vector2.Zero;
    public static Vector2 PreviousSize = Vector2.Zero;

    private static Backend Backend => App.Instance.Backend;

    public static Vector2 Size
    {
        get => Backend.Window.GetWindowSize();

        set
        {
            Backend.Window.SetWindowSize(value);
        }
    }

    public static bool Fullscreen => Backend.Window.IsWindowFullscreen();

    public static void ToggleFullscreen()
    {
        Backend.Window.ToggleFullscreen();
    }
}