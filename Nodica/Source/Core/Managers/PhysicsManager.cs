namespace Cherris;

public sealed class PhysicsManager
{
    private static PhysicsManager? _instance;
    public static PhysicsManager Instance => _instance ??= new();

    public List<KinematicBody> KinematicBodies { get; } = [];

    public void RegisterKinematicBody(KinematicBody body)
    {
        if (!KinematicBodies.Contains(body))
        {
            KinematicBodies.Add(body);
        }
    }
}