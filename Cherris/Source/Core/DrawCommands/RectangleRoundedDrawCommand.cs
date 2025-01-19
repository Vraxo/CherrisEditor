namespace Cherris.RenderCommands;

public class RectangleRoundedDrawCommand : DrawCommand
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Size { get; set; } = Vector2.Zero;
    public float Roundness { get; set; } = 0.5f;
    public int Segments { get; set; } = 0;
    public Color Color { get; set; } = Color.White;

    public override void Draw()
    {
        App.Instance.Backend.Drawing.DrawRoundedRectangle(Position, Size, Roundness, Segments, Color);
    }
}