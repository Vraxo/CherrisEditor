namespace Nodica.RenderCommands;

public class TextureScaledDrawCommand : DrawCommand
{
    public Texture Texture { get; set; } = new();
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Origin { get; set; } = Vector2.Zero;
    public float Rotation { get; set; } = 0f;
    public Vector2 Scale { get; set; } = Vector2.One;
    public bool FlipH { get; set; } = false;
    public bool FlipV { get; set; } = false;

    public override void Draw()
    {
        App.Instance.Backend.Drawing.DrawScaledTexture(Texture, Position, Origin, Rotation, Scale, FlipH, FlipV);
    }
}
