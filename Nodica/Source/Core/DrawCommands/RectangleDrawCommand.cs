namespace Nodica.RenderCommands;

public class RectangleDrawCommand : DrawCommand
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Size { get; set; } = Vector2.Zero;
    public Color Color { get; set; } = Color.White;

    public override void Draw()
    {
        App.Instance.Backend.Drawing.DrawRectangle(Position, Size, Color);
    }
}