namespace Cherris.Backends;

public sealed class RaylibBackend : Backend
{
    public RaylibBackend()
    {
        Window = new RaylibWindowBackend();
        Input = new RaylibInputBackend();
        Time = new RaylibTimeBackend();
        Audio = new RaylibAudioBackend();
        Drawing = new RaylibDrawingBackend();
    }
}