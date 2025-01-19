namespace Cherris;

public class ThemedRectangle : Node2D
{
    public BoxTheme Style = new();

    public ThemedRectangle()
    {
        Size = new(32, 32);
        OriginPreset = OriginPreset.TopLeft;
    }

    protected override void Draw()
    {
        DrawRectangleThemed(
            GlobalPosition - Origin,
            Size,
            Style);
    }
}