namespace Cherris;

public class Node2D : VisualItem
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Rotation { get; set; } = 0;
    public bool InheritPosition { get; set; } = true;
    public bool InheritOrigin { get; set; } = false;
    public bool InheritScale { get; set; } = true;
    public OriginPreset OriginPreset { get; set; } = OriginPreset.Center;
    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Center;
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Center;

    public event EventHandler<Vector2>? SizeChanged;

    private Vector2 _size = Vector2.Zero;
    public Vector2 Size
    {
        get => _size;

        set
        {
            _size = value;
            SizeChanged?.Invoke(this, Size);
        }
    }

    private Vector2 _scale = Vector2.One;
    public Vector2 Scale
    {
        get
        {
            if (InheritScale && Parent is Node2D node2DParent)
            {
                return node2DParent.Scale;
            }

            return _scale;
        }

        set
        {
            _scale = value;
        }
    }

    public Vector2 FinalSize => Size * Scale;

    private Vector2 _globalPosition = Vector2.Zero;

    public Vector2 GlobalPosition
    {
        get
        {
            if (Parent is Node2D parentNode)
            {
                if (InheritPosition)
                {
                    return parentNode.GlobalPosition + Position;
                }

                return _globalPosition;
            }
            else
            {
                return Position;
            }
        }

        set
        {
            _globalPosition = value;

            if (Parent is not Node2D)
            {
                Position = value;
            }
        }
    }

    private Vector2 _offset = Vector2.Zero;
    public Vector2 Offset
    {
        get
        {
            if (InheritOrigin)
            {
                if (Parent is Node2D parentNode)
                {
                    return parentNode.Offset;
                }
            }

            return _offset;
        }

        set
        {
            _offset = value;
        }
    }

    public Vector2 Origin
    {
        get
        {
            float x = HorizontalAlignment switch
            {
                HorizontalAlignment.Center => Size.X * Scale.X / 2,
                HorizontalAlignment.Left => 0,
                HorizontalAlignment.Right => Size.X,
                _ => 0
            };

            float y = VerticalAlignment switch
            {
                VerticalAlignment.Top => 0,
                VerticalAlignment.Center => Size.Y * Scale.Y / 2,
                VerticalAlignment.Bottom => Size.Y * Scale.Y,
                _ => 0
            };

            Vector2 alignmentOffset = new(x, y);
            return alignmentOffset + Offset;
        }
    }

    public void LookAt(Vector2 targetPosition)
    {
        Vector2 direction = targetPosition - GlobalPosition;
        float angle = MathF.Atan2(direction.Y, direction.X) * 57.29578f;
        Rotation = angle;
    }
}