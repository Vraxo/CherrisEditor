namespace Cherris;

public abstract class DrawCommand
{
    public int Layer { get; set; } = 0;

    public abstract void Draw();
}