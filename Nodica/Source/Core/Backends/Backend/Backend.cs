namespace Cherris.Backends;

public abstract class Backend
{
    public IWindowBackend Window;
    public IInputBackend Input;
    public ITimeBackend Time;
    public IAudioBackend Audio;
    public IDrawingBackend Drawing;
}