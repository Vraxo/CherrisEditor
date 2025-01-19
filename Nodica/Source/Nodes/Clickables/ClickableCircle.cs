using Raylib_cs;

namespace Cherris;

public abstract class ClickableCircle : Clickable
{
    public float Radius = 10F;

    public override bool IsMouseOver()
    {
        Vector2 mousePosition = Raylib.GetMousePosition();
        float distance = mousePosition.DistanceTo(GlobalPosition);
        bool isMouseOver = distance < Radius;

        return isMouseOver;
    }
}