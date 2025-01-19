namespace Cherris;

public class Sprite : Node2D
{
    public bool FlipH { get; set; } = false;
    public bool FlipV { get; set; } = false;

    private Texture? _texture = null;
    public Texture? Texture
    {
        get => _texture;

        set
        {
            _texture = value;
            Size = _texture!.Size;
        }
    }

    protected override void Draw()
    {
        base.Draw();

        if (Texture is null)
        {
            return;
        }

        DrawTextureScaled(
            Texture,
            GlobalPosition,
            Origin,
            Rotation,
            Scale,
            FlipH,
            FlipV);
    }
}