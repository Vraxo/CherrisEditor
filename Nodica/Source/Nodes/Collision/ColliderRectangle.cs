namespace Cherris;

public class ColliderRectangle : Collider
{
    public override void Start()
    {
        base.Start();
        CollisionManager.Instance.RegisterRectangle(this);
    }

    public override void Destroy()
    {
        base.Destroy();
        CollisionManager.Instance.UnregisterRectangle(this);
    }

    public override bool RayIntersects(Vector2 rayStart, Vector2 rayEnd)
    {
        Vector2 rectMin = GlobalPosition - Origin;
        Vector2 rectMax = rectMin + Size;

        return RayIntersectsLine(rayStart, rayEnd, rectMin, new(rectMax.X, rectMin.Y)) ||
               RayIntersectsLine(rayStart, rayEnd, new(rectMax.X, rectMin.Y), rectMax) ||
               RayIntersectsLine(rayStart, rayEnd, rectMax, new(rectMin.X, rectMax.Y)) ||
               RayIntersectsLine(rayStart, rayEnd, new(rectMin.X, rectMax.Y), rectMin);
    }

    protected override void Draw()
    {
        base.Draw();
        DrawRectangleOutline(
            GlobalPosition - Origin,
            Size * Scale,
            Color.SkyBlue);
    }

    private static bool RayIntersectsLine(Vector2 rayStart, Vector2 rayEnd, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 r = rayEnd - rayStart;
        Vector2 s = lineEnd - lineStart;

        float denominator = r.X * s.Y - r.Y * s.X;
        if (denominator == 0)
            return false;

        Vector2 delta = lineStart - rayStart;
        float t = (delta.X * s.Y - delta.Y * s.X) / denominator;
        float u = (delta.X * r.Y - delta.Y * r.X) / denominator;

        return t >= 0 && u >= 0 && u <= 1;
    }
}