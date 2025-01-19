namespace Cherris.RenderCommands;

public class TextDrawCommand : DrawCommand
{
    public string Content { get; set; } = "";
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Font Font { get; set; } = FontManager.Instance.Get("Res/Cherris/Fonts/RobotoMono:16");
    public float FontSize { get; set; } = 0;
    public float Spacing { get; set; } = 0;
    public Color Color { get; set; } = Color.White;

    public override void Draw()
    {
		App.Instance.Backend.Drawing.DrawText(Content, Position, Font, FontSize, Spacing, Color);
	}
}