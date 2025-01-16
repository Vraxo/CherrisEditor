namespace Nodica;

public class TextureRectangle : Node2D
{
    public string SecondName { get; set; } = "Fucker";

    private string _texturePath = "";
    public string TexturePath
    {
        get => _texturePath;

        set
        {
            Texture = new(value);
        }
    }

    [InspectorExclude]
    public Texture? Texture { get; set; }

    public TextureRectangle()
    {
        Size = new(32, 32);
    }

    protected override void Draw()
    {
        if (Texture is null)
        {
            return;
        }

        DrawTextureScaled(
            Texture,
            GlobalPosition,
            Origin,
            0,
            Scale,
            false,
            false);
    }
}