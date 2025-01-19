using Raylib_cs;

namespace Cherris;

public class KinematicBody : Node2D
{
    public float Speed { get; set; } = 200f;

    public virtual void MoveAndSlide(Vector2 velocity)
    {
        // Get the target position by adding velocity
        Vector2 targetPosition = GlobalPosition + velocity;

        // Check for potential collisions with other KinematicBodies
        foreach (var other in PhysicsManager.Instance.KinematicBodies)
        {
            // Skip self
            if (other == this) continue;

            // Check if there's an intersection with another KinematicBody
            if (IsIntersecting(this, other))
            {
                // Calculate the push direction to separate the objects
                Vector2 separationDirection = (GlobalPosition - other.GlobalPosition).Normalized();
                float overlapAmount = GetOverlapAmount(this, other);

                // Push the objects apart by the overlap amount in the separation direction
                Vector2 pushAmount = separationDirection * overlapAmount;

                // Adjust the position of the other KinematicBody
                other.GlobalPosition += pushAmount;
            }
        }

        // After handling collisions, move this object to the target position
        GlobalPosition = targetPosition;
    }

    // This method checks if two KinematicBodies are intersecting based on their bounding boxes
    private bool IsIntersecting(KinematicBody body1, KinematicBody body2)
    {
        // Calculate the bounding box for both bodies
        float left1 = body1.GlobalPosition.X - body1.Origin.X;
        float right1 = body1.GlobalPosition.X + body1.Size.X - body1.Origin.X;
        float top1 = body1.GlobalPosition.Y - body1.Origin.Y;
        float bottom1 = body1.GlobalPosition.Y + body1.Size.Y - body1.Origin.Y;

        float left2 = body2.GlobalPosition.X - body2.Origin.X;
        float right2 = body2.GlobalPosition.X + body2.Size.X - body2.Origin.X;
        float top2 = body2.GlobalPosition.Y - body2.Origin.Y;
        float bottom2 = body2.GlobalPosition.Y + body2.Size.Y - body2.Origin.Y;

        // Check for overlap in the bounding boxes
        return !(right1 < left2 || left1 > right2 || bottom1 < top2 || top1 > bottom2);
    }

    // Calculates the overlap amount between two colliding KinematicBodies
    private float GetOverlapAmount(KinematicBody body1, KinematicBody body2)
    {
        float horizontalOverlap = MathF.Min(body1.GlobalPosition.X + body1.Size.X - body2.GlobalPosition.X, body2.GlobalPosition.X + body2.Size.X - body1.GlobalPosition.X);
        float verticalOverlap = MathF.Min(body1.GlobalPosition.Y + body1.Size.Y - body2.GlobalPosition.Y, body2.GlobalPosition.Y + body2.Size.Y - body1.GlobalPosition.Y);

        // Return the smallest overlap to avoid unwanted large displacements
        return MathF.Min(horizontalOverlap, verticalOverlap);
    }

    // Override the Start method to register this KinematicBody in the PhysicsManager
    public override void Start()
    {
        base.Start();
        PhysicsManager.Instance.RegisterKinematicBody(this);
    }

    // For debugging, draw the bounding box for this KinematicBody
    protected override void Draw()
    {
        Raylib.DrawRectangleLines(
            (int)(GlobalPosition.X - Origin.X),
            (int)(GlobalPosition.Y - Origin.Y),
            (int)Size.X,
            (int)Size.Y,
            Color.White
        );

        base.Draw();
    }
}
