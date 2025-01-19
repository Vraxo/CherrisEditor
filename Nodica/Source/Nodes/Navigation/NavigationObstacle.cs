namespace Cherris;

public class NavigationObstacle : Node2D
{
    public override void Ready()
    {
        base.Ready();

        NavigationManager.Instance.RegisterObstacle(this);

        Visible = true;
    }

    public override void Destroy()
    {
        base.Destroy();

        NavigationManager.Instance.UnregisterObstacle(this);
    }

    protected override void Draw()
    {
        base.Draw();

        DrawRectangleOutline(
            GlobalPosition - Origin,
            Size * Scale,
            Color.Pink);
    }
}