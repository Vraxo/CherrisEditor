namespace Cherris;

public sealed class ClickManager
{
    private static ClickManager? _instance;
    public static ClickManager Instance => _instance ??= new();

    public int MinLayer = -1;

    private readonly List<Clickable> clickables = [];

    private ClickManager() { }

    public void Register(Clickable clickable)
    {
        clickables.Add(clickable);
    }

    public void Unregister(Clickable clickable)
    {
        clickables.Remove(clickable);
    }

    public void Process()
    {
        if (Input.IsMouseButtonPressed(MouseButtonCode.Left))
        {
            SignalClick(MouseButtonCode.Left);
        }

        if (Input.IsMouseButtonPressed(MouseButtonCode.Right))
        {
            SignalClick(MouseButtonCode.Right);
        }
    }

    public int GetHighestLayer()
    {
        int highestLayer = MinLayer;

        foreach (var clickable in clickables)
        {
            if (clickable.Layer > highestLayer)
            {
                highestLayer = clickable.Layer;
            }
        }

        return highestLayer;
    }

    private void SignalClick(MouseButtonCode mouseButton)
    {
        List<Clickable> viableClickables = GetViableClickables();

        if (viableClickables.Count > 0)
        {
            Clickable? topClickable = GetTopClickable(viableClickables);

            if (topClickable != null)
            {
                if (mouseButton == MouseButtonCode.Left)
                {
                    topClickable.OnTopLeft = true;
                    Log.Info($"[ClickManager] '{topClickable}' has been left clicked.", "ClickManager");
                }
                else
                {
                    topClickable.OnTopRight = true;
                    Log.Info($"[ClickManager] '{topClickable}' has been right clicked.", "ClickManager");
                }
            }
        }
    }

    private List<Clickable> GetViableClickables()
    {
        List<Clickable> viableClickables = [];

        foreach (Clickable clickable in clickables)
        {
            if (IsMouseOverNode2D(clickable))
            {
                viableClickables.Add(clickable);
            }
        }

        Log.Info($"[ClickManager] {viableClickables.Count} viable clickables.", "ClickManager");

        return viableClickables;
    }

    private Clickable? GetTopClickable(List<Clickable> viableClickables)
    {
        Clickable? topClickable = null;
        int highestLayer = MinLayer;

        foreach (Clickable clickable in viableClickables)
        {
            if (clickable.Layer >= highestLayer)
            {
                highestLayer = clickable.Layer;
                topClickable = clickable;
            }
        }

        Log.Info($"[ClickManager] The highest layer is {viableClickables.Count}.", "ClickManager");

        return topClickable;
    }

    private bool IsMouseOverNode2D(Node2D node)
    {
        Vector2 mousePosition = Input.MousePosition;

        bool isMouseOver =
            mousePosition.X > node.GlobalPosition.X - node.Origin.X &&
            mousePosition.X < node.GlobalPosition.X + node.FinalSize.X - node.Origin.X &&
            mousePosition.Y > node.GlobalPosition.Y - node.Origin.Y &&
            mousePosition.Y < node.GlobalPosition.Y + node.FinalSize.Y - node.Origin.Y;

        return isMouseOver;
    }
}