namespace Cherris.Backends;

public sealed class SDL2Backend : Backend
{
    public SDL2Backend()
    {
        Window = new SDL2WindowBackend();
        Input = new SDL2InputBackend();
        Time = new SDL2TimeBackend();
        Audio = new SDL2AudioBackend();
        Drawing = new SDL2DrawingBackend();
    }
}