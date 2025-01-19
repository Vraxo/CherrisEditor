namespace Cherris;

public class NavigationRegion : Node2D
{
    public int Width { get; set; } = 1024;
    public int Height { get; set; } = 768;
    public float CellSize { get; set; } = 32f;
    public Color WalkableColor { get; set; } = new(255, 255, 255, 64);
    public Color UnwalkableColor { get; set; } = new(255, 0, 0, 64);

    private bool[,] grid = new bool[0, 0];

    public NavigationRegion()
    {
        Visible = false;
    }

    public override void Ready()
    {
        NavigationManager.Instance.RegisterRegion(this);

        grid = new bool[Width, Height];

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                grid[x, y] = true;
            }
        }
    }

    public override void Destroy()
    {
        NavigationManager.Instance.UnregisterRegion(this);
    }

    protected override void Draw()
    {
        DrawGrid();
        DrawUnwalkableCells();
    }

    private void DrawGrid()
    {
        DrawGrid(
            new(Width * 10, Height * 10),
            CellSize,
            WalkableColor);
    }

    public bool IsCellWalkable(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height && grid[x, y];
    }

    public void MarkUnwalkable(int x, int y)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            grid[x, y] = false;
        }
    }

    public void MarkWalkable(int x, int y)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            grid[x, y] = true;
        }
    }

    private void DrawUnwalkableCells()
    {
        for (int x = 0; x < Width / CellSize * 10; x++)
        {
            for (int y = 0; y < Height / CellSize * 10; y++)
            {
                if (!IsCellWalkable(x, y))
                {
                    Vector2 cellPosition = new(x * CellSize, y * CellSize);

                    DrawRectangle(
                        cellPosition,
                        new(CellSize, CellSize),
                        UnwalkableColor);
                }
            }
        }
    }
}