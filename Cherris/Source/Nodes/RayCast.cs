namespace Cherris;

public class RayCast : Node2D
{
    public float Length { get; set; } = 100f;
    public bool IgnoreFirst { get; set; } = false;

    [InspectorExclude]
    public bool IsColliding { get; private set; } = false;

    private Collider? _collider;
    [InspectorExclude]
    public Collider? Collider
    {
        get => _collider;

        private set
        {
            if (_collider != value)
            {
                _collider = value;
            }
        }
    }

    [InspectorExclude]
    public Vector2 TargetPosition
    {
        get
        {
            float x = MathF.Cos(MathF.PI * Rotation / 180) * Length;
            float y = MathF.Sin(MathF.PI * Rotation / 180) * Length;

            return new(x, y);
        }
    }

    public RayCast()
    {
        ReadyForVisibility = true;
    }

    public override void Update()
    {
        PerformRaycast();
        base.Update();
    }

    protected override void Draw()
    {
        base.Draw();

        Vector2 rayEnd = GlobalPosition + TargetPosition;
        //DrawLine(GlobalPosition, rayEnd, Color.Red);
    }

    private void PerformRaycast()
    {
        IsColliding = false;
        Collider = null;

        Vector2 rayStart = GlobalPosition;
        Vector2 rayEnd = rayStart + TargetPosition;

        float closestDistance = float.MaxValue;
        bool firstHitSkipped = false;

        Collider? closestCollider = null;
        float closestColliderDistance = float.MaxValue;

        // Store all potential hits
        List<(Collider collider, float distance)> hits = new List<(Collider, float)>();

        foreach (Collider collider in CollisionManager.Instance.Colliders)
        {
            if (collider.RayIntersects(rayStart, rayEnd))
            {
                float distance = Vector2.Distance(rayStart, collider.GlobalPosition);
                hits.Add((collider, distance));
            }
        }

        // Sort hits by distance
        hits.Sort((a, b) => a.distance.CompareTo(b.distance));

        // Loop through sorted hits and apply IgnoreFirst logic
        foreach (var (collider, distance) in hits)
        {
            if (IgnoreFirst && !firstHitSkipped)
            {
                firstHitSkipped = true;
                continue; // Skip the first hit (which is the closest)
            }

            // Once we find the closest hit, mark it
            if (distance < closestDistance)
            {
                closestDistance = distance;
                IsColliding = true;
                Collider = collider;

                Log.Info($"[RayCast] Hit {Collider.Name}.", "RayCast");
            }
        }
    }
}