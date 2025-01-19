namespace Cherris;

public abstract class ClickableRectangle : Clickable
{
    public override bool IsMouseOver()
    {
        Vector2 mousePosition = Input.MousePosition;

        bool isMouseOver =
            mousePosition.X > GlobalPosition.X - Origin.X &&
            mousePosition.X < GlobalPosition.X + FinalSize.X - Origin.X &&
            mousePosition.Y > GlobalPosition.Y - Origin.Y &&
            mousePosition.Y < GlobalPosition.Y + FinalSize.Y - Origin.Y;

        return isMouseOver;
    }
}