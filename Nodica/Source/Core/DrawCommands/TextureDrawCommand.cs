namespace Nodica.RenderCommands;

public class TextureDrawCommand : DrawCommand
{
    public Texture Texture { get; set; } = new();
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Rotation { get; set; } = 0f;
    public Vector2 Scale { get; set; } = Vector2.One;
    public Color Tint { get; set; } = Color.White;

    public override void Draw()
    {
        App.Instance.Backend.Drawing.DrawTexture(Texture, Position, Rotation, Scale, Tint);
    }
}