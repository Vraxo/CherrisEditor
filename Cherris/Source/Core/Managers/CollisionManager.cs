namespace Cherris;

public sealed class CollisionManager
{
    private static CollisionManager? _instance;
    public static CollisionManager Instance => _instance ??= new();

    public List<Collider> Colliders = [];
    public List<ColliderCircle> ColliderCircles = [];
    public List<ColliderRectangle> ColliderRectangles = [];

    private CollisionManager() { }

    public void RegisterCircle(ColliderCircle collider)
    {
        ColliderCircles.Add(collider);
        Colliders.Add(collider);
    }

    public void UnregisterCircle(ColliderCircle collider)
    {
        ColliderCircles.Remove(collider);
        Colliders.Remove(collider);
    }

    public void RegisterRectangle(ColliderRectangle collider)
    {
        ColliderRectangles.Add(collider);
        Colliders.Add(collider);
    }

    public void UnregisterRectangle(ColliderRectangle collider)
    {
        ColliderRectangles.Remove(collider);
        Colliders.Remove(collider);
    }

    public void Process()
    {
        ProcessCircleCollisions();
        ProcessRectangleCollisions();
    }

    private void ProcessCircleCollisions()
    {
        for (int i = 0; i < ColliderCircles.Count; i++)
        {
            for (int j = i + 1; j < ColliderCircles.Count; j++)
            {
                HandleCircleCollision(ColliderCircles[i], ColliderCircles[j]);
            }
        }
    }

    private static void HandleCircleCollision(ColliderCircle colliderA, ColliderCircle colliderB)
    {
        float scaledRadiusA = colliderA.Radius * colliderA.Scale.X;
        float scaledRadiusB = colliderB.Radius * colliderB.Scale.X;
        float distance = (colliderA.GlobalPosition - colliderA.Origin).DistanceTo(colliderB.GlobalPosition - colliderB.Origin);
        float combinedRadii = scaledRadiusA + scaledRadiusB;

        if (distance < combinedRadii)
        {
            float overlap = combinedRadii - distance;
            ResolveCircleOverlap(colliderA, colliderB, overlap);
        }
    }

    private static void ResolveCircleOverlap(ColliderCircle colliderA, ColliderCircle colliderB, float overlap)
    {
        if (colliderA.IsStatic && colliderB.IsStatic) return;

        Vector2 direction = (colliderB.GlobalPosition - colliderB.Origin - colliderA.GlobalPosition + colliderA.Origin).Normalized();
        Vector2 pushVector = direction * overlap / 2;

        if (!colliderA.IsStatic) colliderA.Position -= pushVector;
        if (!colliderB.IsStatic) colliderB.Position += pushVector;
    }

    private void ProcessRectangleCollisions()
    {
        for (int i = 0; i < ColliderRectangles.Count; i++)
        {
            for (int j = i + 1; j < ColliderRectangles.Count; j++)
            {
                HandleRectangleCollision(ColliderRectangles[i], ColliderRectangles[j]);
            }
        }
    }

    private void HandleRectangleCollision(ColliderRectangle colliderA, ColliderRectangle colliderB)
    {
        if (IsRectangleColliding(colliderA, colliderB))
        {
            Vector2 overlap = GetRectangleOverlap(colliderA, colliderB);
            ResolveRectangleOverlap(colliderA, colliderB, overlap);
        }
    }

    private static bool IsRectangleColliding(ColliderRectangle colliderA, ColliderRectangle colliderB)
    {
        float scaledWidthA = colliderA.Size.X * colliderA.Scale.X;
        float scaledHeightA = colliderA.Size.Y * colliderA.Scale.Y;
        float scaledWidthB = colliderB.Size.X * colliderB.Scale.X;
        float scaledHeightB = colliderB.Size.Y * colliderB.Scale.Y;

        return colliderA.GlobalPosition.X - colliderA.Origin.X < colliderB.GlobalPosition.X + scaledWidthB - colliderB.Origin.X &&
               colliderA.GlobalPosition.X + scaledWidthA - colliderA.Origin.X > colliderB.GlobalPosition.X - colliderB.Origin.X &&
               colliderA.GlobalPosition.Y - colliderA.Origin.Y < colliderB.GlobalPosition.Y + scaledHeightB - colliderB.Origin.Y &&
               colliderA.GlobalPosition.Y + scaledHeightA - colliderA.Origin.Y > colliderB.GlobalPosition.Y - colliderB.Origin.Y;
    }

    private static Vector2 GetRectangleOverlap(ColliderRectangle colliderA, ColliderRectangle colliderB)
    {
        float scaledWidthA = colliderA.Size.X * colliderA.Scale.X;
        float scaledHeightA = colliderA.Size.Y * colliderA.Scale.Y;
        float scaledWidthB = colliderB.Size.X * colliderB.Scale.X;
        float scaledHeightB = colliderB.Size.Y * colliderB.Scale.Y;

        float overlapX = MathF.Min(colliderA.GlobalPosition.X + scaledWidthA - colliderA.Origin.X, colliderB.GlobalPosition.X + scaledWidthB - colliderB.Origin.X) -
                         MathF.Max(colliderA.GlobalPosition.X - colliderA.Origin.X, colliderB.GlobalPosition.X - colliderB.Origin.X);

        float overlapY = MathF.Min(colliderA.GlobalPosition.Y + scaledHeightA - colliderA.Origin.Y, colliderB.GlobalPosition.Y + scaledHeightB - colliderB.Origin.Y) -
                         MathF.Max(colliderA.GlobalPosition.Y - colliderA.Origin.Y, colliderB.GlobalPosition.Y - colliderB.Origin.Y);

        return new Vector2(overlapX, overlapY);
    }

    private void ResolveRectangleOverlap(ColliderRectangle colliderA, ColliderRectangle colliderB, Vector2 overlap)
    {
        if (colliderA.IsStatic && colliderB.IsStatic) return;

        if (overlap.X < overlap.Y)
        {
            ResolveHorizontalOverlap(colliderA, colliderB, overlap.X);
        }
        else
        {
            ResolveVerticalOverlap(colliderA, colliderB, overlap.Y);
        }
    }

    private static void ResolveHorizontalOverlap(ColliderRectangle colliderA, ColliderRectangle colliderB, float overlapX)
    {
        float pushVectorX = overlapX / 2;

        if (colliderA.GlobalPosition.X - colliderA.Origin.X < colliderB.GlobalPosition.X - colliderB.Origin.X)
        {
            if (!colliderA.IsStatic) colliderA.Position -= new Vector2(pushVectorX, 0);
            if (!colliderB.IsStatic) colliderB.Position += new Vector2(pushVectorX, 0);
        }
        else
        {
            if (!colliderA.IsStatic) colliderA.Position += new Vector2(pushVectorX, 0);
            if (!colliderB.IsStatic) colliderB.Position -= new Vector2(pushVectorX, 0);
        }
    }

    private static void ResolveVerticalOverlap(ColliderRectangle colliderA, ColliderRectangle colliderB, float overlapY)
    {
        float pushVectorY = overlapY / 2;

        if (colliderA.GlobalPosition.Y - colliderA.Origin.Y < colliderB.GlobalPosition.Y - colliderB.Origin.Y)
        {
            if (!colliderA.IsStatic) colliderA.Position -= new Vector2(0, pushVectorY);
            if (!colliderB.IsStatic) colliderB.Position += new Vector2(0, pushVectorY);
        }
        else
        {
            if (!colliderA.IsStatic) colliderA.Position += new Vector2(0, pushVectorY);
            if (!colliderB.IsStatic) colliderB.Position -= new Vector2(0, pushVectorY);
        }
    }
}