namespace Cherris.Backends;

public abstract class IDrawingBackend
{
    public abstract void BeginDrawing();
    public abstract void ClearBackground(Color color);
    public abstract void EndDrawing();
    public abstract void DrawRectangleOutline(Vector2 position, Vector2 size, Color color);
    public abstract void DrawRectangleOutlineRounded(Vector2 position, Vector2 size, float roundness, int segments, float thickness, Color color);
    public abstract void DrawRectangle(Vector2 position, Vector2 size, Color color);
    public abstract void DrawRoundedRectangle(Vector2 position, Vector2 size, float roundness, int segments, Color color);
    public abstract void DrawTexture(Texture texture, Vector2 position, float rotation, Vector2 scale, Color tint);
    public abstract void DrawScaledTexture(Texture texture, Vector2 position, Vector2 origin, float rotation, Vector2 scale, bool flipH = false, bool flipV = false);
    public abstract void DrawText(string text, Vector2 position, Font font, float fontSize, float spacing, Color color);
    public abstract void DrawLine(Vector2 from, Vector2 to, Color color);
    public abstract void DrawCircle(Vector2 position, float radius, Color color);
    public abstract void DrawCircleOutline(Vector2 position, float radius, Color color);
}