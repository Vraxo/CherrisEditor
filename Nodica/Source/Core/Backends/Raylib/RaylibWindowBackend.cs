using Raylib_cs;

namespace Cherris.Backends;

public sealed class RaylibWindowBackend : IWindowBackend
{
    public override void InitializeWindow(int width, int height, int minWidth, int minHeight, int maxWidth, int maxHeight, string title, string iconPath)
    {
        Raylib.SetTraceLogLevel(TraceLogLevel.None);
        Raylib.SetConfigFlags(ConfigFlags.VSyncHint | ConfigFlags.HighDpiWindow | ConfigFlags.AlwaysRunWindow);
        Raylib.InitWindow(width, height, title);
        Raylib.SetWindowMinSize(minWidth, minHeight);
        Raylib.SetWindowMaxSize(maxWidth, maxHeight);
        Raylib.SetWindowIcon(Raylib.LoadImage(iconPath));
        Raylib.InitAudioDevice();
    }

    public override void SetWindowFlags(bool resizable, bool antiAliasing)
    {
        ConfigFlags flags = ConfigFlags.VSyncHint | ConfigFlags.HighDpiWindow | ConfigFlags.AlwaysRunWindow;

        if (resizable)
        {
            flags |= ConfigFlags.ResizableWindow;
        }

        if (antiAliasing)
        {
            flags |= ConfigFlags.Msaa4xHint;
        }

        Raylib.SetConfigFlags(flags);
    }

    public override void SetWindowMinSize(int width, int height)
    {
        Raylib.SetWindowMinSize(width, height);
    }

    public override bool WindowShouldClose()
    {
        return Raylib.WindowShouldClose();
    }

    public override Vector2 GetWindowSize()
    {
        return new(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
    }

    public override void SetWindowSize(Vector2 size)
    {
        Raylib.SetWindowSize((int)size.X, (int)size.Y);
    }

    public override bool IsWindowFullscreen()
    {
        return Raylib.IsWindowFullscreen();
    }

    public override void ToggleFullscreen()
    {
        Raylib.ToggleFullscreen();
    }
}