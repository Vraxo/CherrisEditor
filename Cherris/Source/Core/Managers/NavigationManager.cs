namespace Cherris;

public sealed class NavigationManager
{
    private static NavigationManager? _instance;
    public static NavigationManager Instance => _instance ??= new();

    public List<NavigationRegion> Regions = [];

    private NavigationManager() { }

    public void RegisterRegion(NavigationRegion region)
    {
        if (!Regions.Contains(region))
        {
            Regions.Add(region);
        }
    }

    public void UnregisterRegion(NavigationRegion region)
    {
        Regions.Remove(region);
    }

    public void RegisterObstacle(NavigationObstacle obstacle)
    {
        Vector2 start = obstacle.GlobalPosition - obstacle.Origin;
        Vector2 end = start + obstacle.Size * obstacle.Scale;

        foreach (var region in Regions)
        {
            int startX = (int)Math.Floor(start.X / region.CellSize);
            int startY = (int)Math.Floor(start.Y / region.CellSize);
            int endX = (int)Math.Ceiling(end.X / region.CellSize);
            int endY = (int)Math.Ceiling(end.Y / region.CellSize);

            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    region.MarkUnwalkable(x, y);
                }
            }
        }
    }

    public void UnregisterObstacle(NavigationObstacle obstacle)
    {
        Vector2 start = obstacle.GlobalPosition;
        Vector2 end = start + obstacle.Size;

        foreach (var region in Regions)
        {
            for (float x = start.X; x < end.X; x += region.CellSize)
            {
                for (float y = start.Y; y < end.Y; y += region.CellSize)
                {
                    Vector2 gridPosition = new(x / region.CellSize, y / region.CellSize);
                    region.MarkWalkable((int)gridPosition.X, (int)gridPosition.Y);
                }
            }
        }
    }
}