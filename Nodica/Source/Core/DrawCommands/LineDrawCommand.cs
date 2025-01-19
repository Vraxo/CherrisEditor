namespace Cherris.RenderCommands;

public class LineDrawCommand : DrawCommand
{
    public Vector2 Start { get; set; } = Vector2.Zero;
    public Vector2 End { get; set; } = Vector2.Zero;
    public Color Color { get; set; } = Color.White;

    public override void Draw()
    {
        App.Instance.Backend.Drawing.DrawLine(Start, End, Color);
    }
}