namespace Nodica;

public class PopUp : ClickableRectangle
{
    public string Title { get; set; } = "";
    public float TitleBarHeight { get; set; } = 32;
    public Color TitleBarColor { get; set; } = DefaultTheme.Accent;
    public HorizontalAlignment TitleHorizontalAlignment { get; set; } = HorizontalAlignment.Center;
    public VerticalAlignment TitleVerticalAlignment { get; set; } = VerticalAlignment.Center;
    public Vector2 MinSize { get; set; } = new(640, 480);
    public Vector2 MaxSize { get; set; } = new(960, 720);

    public BoxTheme TitleBarTheme { get; set; } = new();

    public bool ClipChildren { get; set; } = false;

    private bool isDragging = false;
    private bool isResizingRight = false;
    private bool isResizingLeft = false;
    private bool isResizingBottom = false;
    private bool isResizingTop = false;
    private Vector2 dragOffset;

    // Main

    public PopUp()
    {
        Size = new(640, 480);
        InheritPosition = false;
        VerticalAlignment = VerticalAlignment.Top;
        TitleBarTheme.Roundness = 0;
    }

    public override void Process()
    {
        if (ClipChildren)
        {
            int x = (int)(GlobalPosition.X - Origin.X);
            int y = (int)(GlobalPosition.Y - Origin.Y);
            int width = (int)Size.X;
            int height = (int)Size.Y;

            Raylib_cs.Raylib.BeginScissorMode(x, y, width, height);
        }

        base.Process();

        UpdateResizingCursor();
        HandleResizing();
        HandleDragging();

        if (ClipChildren)
        {
            Raylib_cs.Raylib.EndScissorMode();
        }
    }

    protected override void Draw()
    {
        base.Draw();

        DrawBackground();
        DrawTitleBar();
        DrawTitle();
    }

    // Drawing

    private void DrawBackground()
    {
        DrawRectangleRounded(
            GlobalPosition - Origin,
            Size,
            0,
            0,
            Color.DarkGray);
    }

    private void DrawTitleBar()
    {
        DrawRectangleThemed(
            GlobalPosition - Origin,
            new(Size.X, TitleBarHeight),
            TitleBarTheme);
    }

    private void DrawTitle()
    {
        DrawText(
            Title,
            GetTitlePosition(),
            FontManager.Instance.Get("RobotoMono 32"),
            16,
            0,
            Color.White);
    }

    private Vector2 GetTitlePosition()
    {
        Vector2 titleSize = Font.MeasureText(
            ResourceLoader.Load<Font>("RobotoMono 32"),
            Title,
            16,
            0);

        Vector2 topBarPosition = GlobalPosition - Origin;
        Vector2 topBarSize = new(Size.X, TitleBarHeight);

        float x = TitleHorizontalAlignment switch
        {
            HorizontalAlignment.Center => topBarPosition.X + (topBarSize.X - titleSize.X) / 2,
            HorizontalAlignment.Left => topBarPosition.X + 8,
            HorizontalAlignment.Right => topBarPosition.X + topBarSize.X - titleSize.X - 8,
            _ => topBarPosition.X
        };

        float y = TitleVerticalAlignment switch
        {
            VerticalAlignment.Center => topBarPosition.Y + (topBarSize.Y - titleSize.Y) / 2,
            VerticalAlignment.Top => topBarPosition.Y + 4,
            VerticalAlignment.Bottom => topBarPosition.Y + topBarSize.Y - titleSize.Y - 4,
            _ => topBarPosition.Y
        };

        return new(x, y);
    }

    // Dragging & resizing

    private void HandleDragging()
    {
        Vector2 mousePosition = Input.MousePosition;

        if (Input.IsMouseButtonDown(MouseButtonCode.Left))
        {
            if (!isDragging && !IsResizing() && IsMouseOnTitleBar())
            {
                isDragging = true;
                dragOffset = mousePosition - GlobalPosition;
            }

            if (isDragging)
            {
                // Check boundaries: Mouse position should be within the window size
                if (mousePosition.X >= 0 && mousePosition.X <= WindowManager.Size.X && mousePosition.Y >= 0 && mousePosition.Y <= WindowManager.Size.Y)
                {
                    GlobalPosition = mousePosition - dragOffset;
                }
            }
        }
        else
        {
            isDragging = false;
        }
    }

    // Resizing

    private void UpdateResizingCursor()
    {
        if (isResizingRight)
        {
            Input.Cursor = MouseCursorCode.ResizeHorizontal;

            if (isResizingBottom)
            {
                Input.Cursor = MouseCursorCode.ResizeBottomLeftToTopRight;
            }

            if (isResizingTop)
            {
                Input.Cursor = MouseCursorCode.ResizeTopLeftToBottomRight;
            }
        }
        else if (isResizingLeft)
        {
            Input.Cursor = MouseCursorCode.ResizeHorizontal;

            if (isResizingBottom)
            {
                Input.Cursor = MouseCursorCode.ResizeTopLeftToBottomRight;
            }

            if (isResizingTop)
            {
                Input.Cursor = MouseCursorCode.ResizeBottomLeftToTopRight;
            }
        }
        else if (isResizingTop || isResizingBottom)
        {
            Input.Cursor = MouseCursorCode.ResizeVertical;
        }
        else if (IsMouseOnRightEdge())
        {
            Input.Cursor = MouseCursorCode.ResizeHorizontal;

            if (IsMouseOnTopEdge())
            {
                Input.Cursor = MouseCursorCode.ResizeTopLeftToBottomRight;
            }

            if (IsMouseOnBottomEdge())
            {
                Input.Cursor = MouseCursorCode.ResizeBottomLeftToTopRight;
            }
        }
        else if (IsMouseOnLeftEdge())
        {
            Input.Cursor = MouseCursorCode.ResizeHorizontal;

            if (IsMouseOnTopEdge())
            {
                Input.Cursor = MouseCursorCode.ResizeBottomLeftToTopRight;
            }

            if (IsMouseOnBottomEdge())
            {
                Input.Cursor = MouseCursorCode.ResizeTopLeftToBottomRight;
            }
        }
        else if (IsMouseOnTopEdge() || IsMouseOnRightEdge())
        {
            Input.Cursor = MouseCursorCode.ResizeVertical;
        }
        else
        {
            Input.Cursor = MouseCursorCode.Default;
        }
    }

    private void HandleResizing()
    {
        if (Input.IsMouseButtonDown(MouseButtonCode.Left) && !isDragging)
        {
            CheckForResizeStart();

            Vector2 mousePosition = Input.MousePosition;

            if (isResizingRight)
            {
                ResizeRight(mousePosition);
            }

            if (isResizingLeft)
            {
                ResizeLeft(mousePosition);
            }

            if (isResizingBottom)
            {
                ResizeDown(mousePosition);
            }

            if (isResizingTop)
            {
                ResizeUp(mousePosition);
            }
        }
        else
        {
            ResetAfterResize();
        }
    }

    private void ResetAfterResize()
    {
        isResizingRight = false;
        isResizingLeft = false;
        isResizingBottom = false;
        isResizingTop = false;

        if (HorizontalAlignment == HorizontalAlignment.Left)
        {
            HorizontalAlignment = HorizontalAlignment.Center;
            GlobalPosition = new(GlobalPosition.X + Size.X / 2, GlobalPosition.Y);
            MoveChildrenToRight();
        }
        else if (HorizontalAlignment == HorizontalAlignment.Right)
        {
            HorizontalAlignment = HorizontalAlignment.Center;
            GlobalPosition = new(GlobalPosition.X - Size.X / 2, GlobalPosition.Y);
            MoveChildrenToLeft();
        }
        else if (VerticalAlignment == VerticalAlignment.Bottom)
        {
            VerticalAlignment = VerticalAlignment.Top;
            GlobalPosition = new(GlobalPosition.X, GlobalPosition.Y - Size.Y);
            MoveChildrenUp();
        }
    }

    private void CheckForResizeStart()
    {
        if (!isResizingRight && IsMouseOnRightEdge() && !isResizingLeft)
        {
            isResizingRight = true;
            HorizontalAlignment = HorizontalAlignment.Left;
            GlobalPosition = new(GlobalPosition.X - Size.X / 2, GlobalPosition.Y);
            MoveChildrenToLeft();
        }

        if (!isResizingLeft && IsMouseOnLeftEdge() && !isResizingRight)
        {
            isResizingLeft = true;
            HorizontalAlignment = HorizontalAlignment.Right;
            GlobalPosition = new(GlobalPosition.X + Size.X / 2, GlobalPosition.Y);
            MoveChildrenToRight();
        }

        if (!isResizingBottom && IsMouseOnBottomEdge() && !isResizingTop)
        {
            isResizingBottom = true;
        }

        if (!isResizingTop && IsMouseOnTopEdge() && !isResizingBottom)
        {
            isResizingTop = true;
            VerticalAlignment = VerticalAlignment.Bottom;
            GlobalPosition = new(GlobalPosition.X, GlobalPosition.Y + Size.Y);
            MoveChildrenDown();
        }
    }
    
    private void ResizeRight(Vector2 mousePosition)
    {
        float difference = mousePosition.X - (GlobalPosition.X + Size.X);

        float previousX = Size.X;

        if (mousePosition.X >= 0 && mousePosition.X <= WindowManager.Size.X)
        {
            float width = Math.Clamp(Size.X + difference, MinSize.X, MaxSize.X);
            Size = new(width, Size.Y);

            foreach (Node node in Children)
            {
                if (node is Node2D child)
                {
                    child.Position = new(child.Position.X + (Size.X - previousX) / 2, child.Position.Y);
                }
            }
        }
    }

    private void ResizeLeft(Vector2 mousePosition)
    {
        float difference = GlobalPosition.X - Size.X - mousePosition.X;

        float previousX = Size.X;

        if (mousePosition.X >= 0 && mousePosition.X <= WindowManager.Size.X)
        {
            float width = Math.Clamp(Size.X + difference, MinSize.X, MaxSize.X);
            Size = new(width, Size.Y);

            foreach (Node node in Children)
            {
                if (node is Node2D child)
                {
                    child.Position = new(child.Position.X - (Size.X - previousX) / 2, child.Position.Y);
                }
            }
        }
    }

    private void ResizeDown(Vector2 mousePosition)
    {
        float newHeight = mousePosition.Y - (GlobalPosition.Y - Origin.Y) - Size.Y;

        if (mousePosition.Y >= 0 && mousePosition.Y <= WindowManager.Size.Y)
        {
            Size = new(Size.X, MathF.Max(Size.Y + newHeight, MinSize.Y));
        }
    }

    private void ResizeUp(Vector2 mousePosition)
    {
        float difference = mousePosition.Y - (GlobalPosition.Y - Origin.Y);

        float previousY = Size.Y;

        if (mousePosition.Y >= 0 && mousePosition.Y <= WindowManager.Size.Y)
        {
            float height = Math.Clamp(Size.Y - difference, MinSize.Y, MaxSize.Y);
            Size = new(Size.X, height);

            foreach (Node node in Children)
            {
                if (node is Node2D child)
                {
                    child.Position = new(child.Position.X, child.Position.Y - (Size.Y - previousY));
                }
            }
        }
    }

    // Checks

    private bool IsMouseOnRightEdge()
    {
        float edgeWidth = 8;
        Vector2 mousePosition = Input.MousePosition;
        Vector2 rightEdgePosition = GlobalPosition - Origin + new Vector2(Size.X - edgeWidth / 2, 0);

        return mousePosition.X >= rightEdgePosition.X &&
               mousePosition.X <= rightEdgePosition.X + edgeWidth &&
               mousePosition.Y >= rightEdgePosition.Y && // Remove TitleBarHeight condition
               mousePosition.Y <= rightEdgePosition.Y + Size.Y;
    }

    private bool IsMouseOnLeftEdge()
    {
        float edgeWidth = 8;
        Vector2 mousePosition = Input.MousePosition;
        Vector2 leftEdgePosition = GlobalPosition - Origin + new Vector2(-edgeWidth / 2, 0);

        return mousePosition.X >= leftEdgePosition.X &&
               mousePosition.X <= leftEdgePosition.X + edgeWidth &&
               mousePosition.Y >= leftEdgePosition.Y && // Remove TitleBarHeight condition
               mousePosition.Y <= leftEdgePosition.Y + Size.Y;
    }

    private bool IsMouseOnBottomEdge()
    {
        float edgeHeight = 8;
        Vector2 mousePosition = Input.MousePosition;
        Vector2 bottomEdgePosition = GlobalPosition - Origin + new Vector2(0, Size.Y - edgeHeight / 2);

        DrawCircle(GlobalPosition - Origin + new Vector2(0, Size.Y - 4), 1, Color.Red);

        return mousePosition.X >= bottomEdgePosition.X &&
               mousePosition.X <= bottomEdgePosition.X + Size.X &&
               mousePosition.Y >= bottomEdgePosition.Y &&
               mousePosition.Y <= bottomEdgePosition.Y + edgeHeight;
    }

    private bool IsMouseOnTopEdge()
    {
        float edgeHeight = TitleBarHeight / 8; // Top third
        Vector2 mousePosition = Input.MousePosition;
        Vector2 titleBarPosition = GlobalPosition - Origin; // Top-left corner of the title bar

        // Check if the mouse is within the top third of the title bar
        return mousePosition.X >= titleBarPosition.X &&
               mousePosition.X <= titleBarPosition.X + Size.X &&
               mousePosition.Y >= titleBarPosition.Y &&
               mousePosition.Y <= titleBarPosition.Y + edgeHeight;
    }

    private bool IsMouseOnTitleBar()
    {
        Vector2 mousePosition = Input.MousePosition;
        Vector2 titleBarPosition = GlobalPosition - Origin; // Top-left corner of the title bar
        float topEdgeHeight = TitleBarHeight / 8;          // Top third
        float bottomAreaHeight = TitleBarHeight - topEdgeHeight; // Bottom two-thirds

        // Check if the mouse is within the bottom two-thirds of the title bar
        return mousePosition.X >= titleBarPosition.X &&
               mousePosition.X <= titleBarPosition.X + Size.X &&
               mousePosition.Y >= titleBarPosition.Y + topEdgeHeight &&
               mousePosition.Y <= titleBarPosition.Y + TitleBarHeight;
    }

    private bool IsResizing()
    {
        return isResizingRight || isResizingBottom || isResizingLeft || isResizingTop;
    }

    // Move children

    private void MoveChildrenToRight()
    {
        foreach (Node node in Children)
        {
            if (node is Node2D child)
            {
                child.Position = new(child.Position.X - Size.X / 2, child.Position.Y);
            }
        }
    }

    private void MoveChildrenToLeft()
    {
        foreach (Node node in Children)
        {
            if (node is Node2D child)
            {
                child.Position = new(child.Position.X + Size.X / 2, child.Position.Y);
            }
        }
    }

    private void MoveChildrenDown()
    {
        foreach (Node node in Children)
        {
            if (node is Node2D child)
            {
                child.Position = new(child.Position.X, child.Position.Y - Size.Y);
            }
        }
    }

    private void MoveChildrenUp()
    {
        foreach (Node node in Children)
        {
            if (node is Node2D child)
            {
                child.Position = new(child.Position.X, child.Position.Y + Size.Y);
            }
        }
    }
}