namespace Nodica;

public class Sprite : Node2D
{
    public bool FlipH { get; set; } = false;
    public bool FlipV { get; set; } = false;

    private string? _texturePath;
    public string? TexturePath
    {
        get => _texturePath;
        set
        {
            _texturePath = value;
            if (!string.IsNullOrEmpty(_texturePath))
            {
                // Load texture based on _texturePath
                Texture = ResourceLoader.Load<Texture>(_texturePath); // You'll need a LoadTexture method
            }
            else
            {
                Texture = null;
            }
        }
    }

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