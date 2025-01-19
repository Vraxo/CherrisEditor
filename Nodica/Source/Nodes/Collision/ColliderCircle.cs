namespace Cherris;

public class ColliderCircle : Collider
{
    public float Radius { get; set; } = 16;

    public override void Start()
    {
        base.Start();
        CollisionManager.Instance.RegisterCircle(this);
    }

    public override void Destroy()
    {
        base.Destroy();
        CollisionManager.Instance.UnregisterCircle(this);
    }

    public override bool RayIntersects(Vector2 rayStart, Vector2 rayEnd)
    {
        Vector2 closestPoint = ClosestPointOnLine(rayStart, rayEnd, GlobalPosition);
        float distanceToCircle = Vector2.Distance(closestPoint, GlobalPosition);

        return distanceToCircle <= Radius;
    }

    private Vector2 ClosestPointOnLine(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
    {
        Vector2 line = lineEnd - lineStart;
        float t = Vector2.Dot(point - lineStart, line) / Vector2.Dot(line, line);
        t = Math.Clamp(t, 0, 1);
        return lineStart + t * line;
    }

    protected override void Draw()
    {
        base.Draw();
        DrawCircleOutline(
            GlobalPosition - Origin,
            Radius,
            Color.Blue);
    }
}