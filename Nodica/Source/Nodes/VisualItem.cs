using Nodica.RenderCommands;

namespace Nodica;

public abstract class VisualItem : Node
{
    public bool Visible { get; set; } = true;

    [InspectorExclude]
    public bool ReadyForVisibility { get; set; } = false;

    private int _layer = 0;
    public int Layer
    {
        get => _layer;

        set
        {
            if (_layer != value)
            {
                _layer = value;
                LayerChanged?.Invoke(this, _layer);
            }
        }
    }

    public delegate void VisualItemEventHandler(VisualItem sender, int layer);
    public event VisualItemEventHandler? LayerChanged;

    public override void Update()
    {
        base.Update();

        if (Visible && ReadyForVisibility)
        {
            Draw();
        }

        ReadyForVisibility = true;
    }

    protected virtual void Draw() { }

    // Circle

    protected void DrawCircle(Vector2 position, float radius, Color color)
    {
        CircleDrawCommand circle = new()
        {
            Position = position,
            Radius = radius,
            Color = color,
            Layer = Layer
        };

        RenderManager.Instance.Submit(circle);
    }

    protected void DrawCircleOutline(Vector2 position, float radius, Color color)
    {
        CircleDrawCommand circleOutline = new()
        {
            Position = position,
            Radius = radius,
            Color = color,
            Layer = Layer
        };

        RenderManager.Instance.Submit(circleOutline);
    }

    // Rectangle

    protected void DrawRectangle(Vector2 position, Vector2 size, Color color)
    {
        RectangleDrawCommand rectangle = new()
        {
            Position = position,
            Size = size,
            Color = color,
            Layer = Layer
        };

        RenderManager.Instance.Submit(rectangle);
    }

    protected void DrawRectangleOutline(Vector2 position, Vector2 size, Color color)
    {
        RectangleOutlineDrawCommand rectangleOutline = new()
        {
            Position = position,
            Size = size,
            Color = color,
            Layer = Layer
        };

        RenderManager.Instance.Submit(rectangleOutline);
    }

    protected void DrawRectangleRounded(Vector2 position, Vector2 size, float roundness, int segments, Color color)
    {
        RectangleRoundedDrawCommand roundedRectangle = new()
        {
            Position = position,
            Size = size,
            Roundness = roundness,
            Segments = segments,
            Color = color,
            Layer = Layer
        };

        RenderManager.Instance.Submit(roundedRectangle);
    }

    protected void DrawRectangleThemed(Vector2 position, Vector2 size, BoxTheme theme)
    {
        // Border lengths for each side
        float top = theme.BorderLengthTop;
        float right = theme.BorderLengthRight;
        float bottom = theme.BorderLengthBottom;
        float left = theme.BorderLengthLeft;

        // Adjust the positions for borders to avoid visual artifacts.
        Vector2 outerRectanglePosition = position;
        Vector2 outerRectangleSize = size;

        // Check if we need to adjust the borders
        if (top > 0 || right > 0 || bottom > 0 || left > 0)
        {
            // We adjust the border size for the outer rectangle
            outerRectanglePosition = new(position.X - left + 1, position.Y - top + 1);
            outerRectangleSize = new(size.X + left + right - 2, size.Y + top + bottom - 2);

            // Draw the border (only where needed)
            DrawRectangleOutlineRounded(
                outerRectanglePosition,
                outerRectangleSize,
                theme.Roundness,
                (int)size.Y,
                1,
                theme.BorderColor);
        }

        // Draw the inner rectangle (the actual filled area of the progress bar)
        DrawRectangleRounded(
            position,
            size,
            theme.Roundness,
            (int)size.Y,
            theme.FillColor);
    }

    protected void DrawRectangleOutlineRounded(Vector2 position, Vector2 size, float roundness, int segments, float thickness, Color color)
    {
        RectangleOutlineRoundedDrawCommand roundedRectangle = new()
        {
            Position = position,
            Size = size,
            Roundness = roundness,
            Segments = segments,
            Thickness = thickness,
            Color = color,
            Layer = Layer
        };

        RenderManager.Instance.Submit(roundedRectangle);
    }

    // Same as V1, except the outline is moved down.
    //protected void DrawRectangleThemed(Vector2 position, Vector2 size, BoxTheme theme)
    //{
    //    float top = theme.BorderLengthTop;
    //    float right = theme.BorderLengthRight;
    //    float bottom = theme.BorderLengthBottom;
    //    float left = theme.BorderLengthLeft;
    //
    //    Vector2 outerRectanglePosition = new(position.X - left, position.Y - top + 10);
    //    Vector2 outerRectangleSize = new(size.X + left + right, size.Y + top + bottom - 10);
    //
    //    if (top > 0 || right > 0 || bottom > 0 || left > 0)
    //    {
    //        DrawRectangleRounded(
    //            outerRectanglePosition,
    //            outerRectangleSize,
    //            theme.Roundness,
    //            (int)size.Y,
    //            theme.BorderColor);
    //    }
    //
    //    DrawRectangleRounded(
    //        position,
    //        size,
    //        theme.Roundness,
    //        (int)size.Y,
    //        theme.FillColor);
    //}

    // V1 - Has artifacts.
    //protected void DrawRectangleThemed(Vector2 position, Vector2 size, BoxTheme theme)
    //{
    //    float top = theme.BorderLengthTop;
    //    float right = theme.BorderLengthRight;
    //    float bottom = theme.BorderLengthBottom;
    //    float left = theme.BorderLengthLeft;
    //
    //    if (top > 0)
    //    {
    //        Vector2 borderPosition = new(position.X - left, position.Y - top);
    //        Vector2 borderSize = new(size.X + left + right, top);
    //        DrawRectangleRounded(
    //            borderPosition,
    //            borderSize,
    //            theme.Roundness,
    //            (int)size.Y,
    //            theme.BorderColor);
    //    }
    //
    //    if (right > 0)
    //    {
    //        Vector2 borderPosition = new(position.X + size.X, position.Y - top);
    //        Vector2 borderSize = new(right, size.Y + top + bottom);
    //        DrawRectangleRounded(
    //             borderPosition,
    //            borderSize,
    //            theme.Roundness,
    //             (int)size.Y,
    //            theme.BorderColor);
    //    }
    //
    //
    //    if (bottom > 0)
    //    {
    //        Vector2 borderPosition = new(position.X - left, position.Y + size.Y);
    //        Vector2 borderSize = new(size.X + left + right, bottom);
    //        DrawRectangleRounded(
    //            borderPosition,
    //           borderSize,
    //            theme.Roundness,
    //            (int)size.Y,
    //            theme.BorderColor);
    //    }
    //
    //    if (left > 0)
    //    {
    //        Vector2 borderPosition = new(position.X - left, position.Y - top);
    //        Vector2 borderSize = new(left, size.Y + top + bottom);
    //        DrawRectangleRounded(
    //           borderPosition,
    //           borderSize,
    //           theme.Roundness,
    //           (int)size.Y,
    //           theme.BorderColor);
    //    }
    //
    //
    //    DrawRectangleRounded(
    //        position,
    //        size,
    //        theme.Roundness,
    //        (int)size.Y,
    //        theme.FillColor);
    //}



    // Texture

    protected void DrawTexture(Texture texture, Vector2 position, float rotation, Vector2 scale, Color tint)
    {
        TextureDrawCommand textureDrawCommand = new()
        {
            Texture = texture,
            Position = position,
            Rotation = rotation,
            Scale = scale,
            Tint = tint,
            Layer = Layer
        };

        RenderManager.Instance.Submit(textureDrawCommand);
    }

    protected void DrawTextureScaled(Texture texture, Vector2 position, Vector2 origin, float rotation, Vector2 scale, bool flipH = false, bool flipV = false)
    {
        TextureScaledDrawCommand textureDrawCommand = new()
        {
            Texture = texture,
            Position = position,
            Origin = origin,
            Rotation = rotation,
            Scale = scale,
            FlipH = flipH,
            FlipV = flipV,
            Layer = Layer
        };

        RenderManager.Instance.Submit(textureDrawCommand);
    }

    // Text

    protected void DrawText(string content, Vector2 position, Font font, float fontSize, float spacing, Color color)
    {
        TextDrawCommand text = new()
        {
            Content = content,
            Position = position,
            Font = font,
            FontSize = fontSize,
            Spacing = spacing,
            Color = color,
            Layer = Layer
        };

        RenderManager.Instance.Submit(text);
    }

    // Line

    protected void DrawLine(Vector2 start, Vector2 end, Color color)
    {
        LineDrawCommand line = new()
        {
            Start = start,
            End = end,
            Color = color,
            Layer = Layer
        };

        RenderManager.Instance.Submit(line);
    }


    protected void DrawGrid(Vector2 size, float cellSize, Color color)
    {
        for (float x = 0; x < size.X; x += cellSize)
        {
            DrawLine(new(x, 0), new(x, size.Y), color);
        }

        for (float y = 0; y < size.Y; y += cellSize)
        {
            DrawLine(new(0, y), new(size.X, y), color);
        }
    }
}