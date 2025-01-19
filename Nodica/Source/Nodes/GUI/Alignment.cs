namespace Cherris;

public class Alignment
{
    public HorizontalAlignment Horizontal { get; set; } = HorizontalAlignment.Center;
    public VerticalAlignment Vertical { get; set; } = VerticalAlignment.Center;

    public Alignment() { }

    public Alignment(HorizontalAlignment horizontal, VerticalAlignment vertical)
    {
        Horizontal = horizontal;
        Vertical = vertical;
    }

    public override string ToString()
    {
        return $"<{Horizontal}, {Vertical}?";
    }

    public static implicit operator string(Alignment alignment)
    {
        return alignment.ToString();
    }
}