using Raylib_cs;
using SkiaSharp;

namespace Nodica.Backends;

public sealed class RaylibDrawingBackend : IDrawingBackend
{
    // Drawing

    public override void BeginDrawing()
    {
        Raylib.BeginDrawing();
    }

    public override void ClearBackground(Color color)
    {
        Raylib.ClearBackground(color);
    }

    public override void EndDrawing()
    {
        Raylib.EndDrawing();
    }

    // Circle

    public override void DrawCircle(Vector2 position, float radius, Color color)
    {
        Raylib.DrawCircle((int)position.X, (int)position.Y, radius, color);
    }

    public override void DrawCircleOutline(Vector2 position, float radius, Color color)
    {
        Raylib.DrawCircleLines((int)position.X, (int)position.Y, radius, color);
    }

    // Rectangle

    public override void DrawRectangle(Vector2 position, Vector2 size, Color color)
    {
        Raylib.DrawRectangleV(
            position,
            size,
            color);
    }

    public override void DrawRectangleOutline(Vector2 position, Vector2 size, Color color)
    {
        Raylib.DrawRectangleLines((int)position.X, (int)position.Y, (int)size.X, (int)size.Y, color);
    }

    public override void DrawRectangleOutlineRounded(Vector2 position, Vector2 size, float roundness, int segments, float thickness, Color color)
    {
        Rectangle rectangle = new()
        {
            Position = position,
            Size = size
        };

        Raylib.DrawRectangleRoundedLines(rectangle, roundness, segments, thickness, color);
    }

    public override void DrawRoundedRectangle(Vector2 position, Vector2 size, float roundness, int segments, Color color)
    {
        Raylib.DrawRectangleRounded(new Rectangle(position.X, position.Y, size.X, size.Y), roundness, segments, color);
    }

    // Texture

    public override void DrawTexture(Texture texture, Vector2 position, float rotation, Vector2 scale, Color tint)
    {
        Raylib.DrawTextureEx(texture, position, rotation, scale.X, tint);
    }

    public override void DrawScaledTexture(Texture texture, Vector2 position, Vector2 origin, float rotation, Vector2 scale, bool flipH = false, bool flipV = false)
    {
        Rectangle source = new()
        {
            Position = new(0, 0),
            Width = texture.Size.X * (flipH ? -1 : 1),
            Height = texture.Size.Y * (flipV ? -1 : 1),
        };

        Rectangle destination = new()
        {
            Position = position,
            Size = texture.Size * scale
        };

        Raylib.DrawTexturePro(
            texture,
            source,
            destination,
            origin,
            rotation,
            Color.White);
    }

    // Text

    public override void DrawText(string text, Vector2 position, Nodica.Font font, float fontSize, float spacing, Color color)
    {
        Raylib.DrawTextEx(font, text, position, fontSize, spacing, color);
    }

    // Line

    public override void DrawLine(Vector2 from, Vector2 to, Color color)
    {
        Raylib.DrawLine((int)from.X, (int)from.Y, (int)to.X, (int)to.Y, color);
    }
}