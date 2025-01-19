namespace Cherris.Backends;

public abstract class IWindowBackend
{
    public abstract void InitializeWindow(int width, int height, int minWidth, int minHeight, int maxWidth, int maxHeight, string title, string iconPath);
    public abstract void SetWindowFlags(bool resizable, bool antiAliasing);
    public abstract void SetWindowMinSize(int width, int height);
    public abstract bool WindowShouldClose();
    public abstract Vector2 GetWindowSize();
    public abstract void SetWindowSize(Vector2 size);
    public abstract bool IsWindowFullscreen();
    public abstract void ToggleFullscreen();
}