namespace Cherris;

public class NavigationAgent : Node2D
{
    public Vector2 TargetPosition { get; set; } = new(0, 0);
    public List<Vector2> Path = new() { new() };
    public NavigationRegion? Region { get; set; }
    public Color Color { get; set; } = new(255, 0, 0, 255);

    private float CellSize => Region?.CellSize ?? 32f;

    private Vector2 lastTargetPosition;

    private const float TargetMoveThreshold = 1.0f;

    public override void Update()
    {
        base.Update();

        if (Region != null && TargetPosition.DistanceTo(lastTargetPosition) > TargetMoveThreshold)
        {
            UpdatePath();
            lastTargetPosition = TargetPosition;
        }
    }

    protected override void Draw()
    {
        if (Region != null)
        {
            DrawTarget();
            DrawPath();
        }
    }

    private void UpdatePath()
    {
        Vector2 start = GlobalPosition;
        Vector2 goal = AdjustTargetPosition(TargetPosition);

        Vector2 startGrid = GetGridPosition(start);
        Vector2 goalGrid = GetGridPosition(goal);

        List<Vector2> openList = new() { startGrid };
        Dictionary<Vector2, Vector2> cameFrom = new();
        Dictionary<Vector2, float> gScore = new() { [startGrid] = 0 };
        Dictionary<Vector2, float> fScore = new() { [startGrid] = Heuristic(startGrid, goalGrid) };

        while (openList.Count > 0)
        {
            Vector2 current = GetLowestFScoreNode(openList, fScore);

            if (current == goalGrid)
            {
                ReconstructPath(cameFrom, current);
                break;
            }

            openList.Remove(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (!Region.IsCellWalkable((int)neighbor.X, (int)neighbor.Y))
                {
                    continue;
                }

                float tentativeGScore = gScore[current] + 1;

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, goalGrid);

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }
    }

    private void ReconstructPath(Dictionary<Vector2, Vector2> cameFrom, Vector2 current)
    {
        Path.Clear();
        while (cameFrom.ContainsKey(current))
        {
            Path.Add((current * CellSize) + new Vector2(CellSize / 2, CellSize / 2));
            current = cameFrom[current];
        }
        Path.Reverse();
    }

    private void DrawTarget()
    {
        DrawCircle(
            TargetPosition,
            5,
            Color);
    }

    private void DrawPath()
    {
        for (int i = 0; i < Path.Count - 1; i++)
        {
            Vector2 start = Path[i];
            Vector2 end = Path[i + 1];

            DrawLine(start,
                end,
                Color);
        }
    }

    private static List<Vector2> GetNeighbors(Vector2 node)
    {
        return new List<Vector2>
        {
            node + new Vector2(1, 0),
            node + new Vector2(-1, 0),
            node + new Vector2(0, 1),
            node + new Vector2(0, -1)
        }.Where(n => n.X >= 0 && n.Y >= 0).ToList();
    }

    private Vector2 GetLowestFScoreNode(List<Vector2> openList, Dictionary<Vector2, float> fScore)
    {
        return openList.OrderBy(v => fScore.ContainsKey(v) ? fScore[v] : float.MaxValue).First();
    }

    private Vector2 AdjustTargetPosition(Vector2 target)
    {
        Vector2 targetGrid = GetGridPosition(target);

        if (Region?.IsCellWalkable((int)targetGrid.X, (int)targetGrid.Y) ?? false)
            return target;

        return FindNearbyWalkablePosition(targetGrid);
    }

    private static Vector2 GetGridPosition(Vector2 position)
    {
        return new(MathF.Floor(position.X / 32f), MathF.Floor(position.Y / 32f));
    }

    private Vector2 FindNearbyWalkablePosition(Vector2 targetGrid)
    {
        for (int radius = 1; radius <= 10; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    Vector2 neighbor = targetGrid + new Vector2(dx, dy);
                    
                    if (Region?.IsCellWalkable((int)neighbor.X, (int)neighbor.Y) ?? false)
                    {
                        return (neighbor * 32f) + new Vector2(16f, 16f);
                    }
                }
            }
        }
        return targetGrid * 32f;
    }

    private static float Heuristic(Vector2 a, Vector2 b)
    {
        return MathF.Abs(a.X - b.X) + MathF.Abs(a.Y - b.Y);
    }
}