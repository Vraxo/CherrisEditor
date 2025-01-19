namespace Cherris;

public class BoxTheme
{
    public float Roundness { get; set; } = 0.2f;
    public Color FillColor { get; set; } = DefaultTheme.NormalFill;
    public Color BorderColor { get; set; } = DefaultTheme.NormalBorder;

    public float BorderLengthTop { get; set; } = 0;
    public float BorderLengthRight { get; set; } = 0;
    public float BorderLengthBottom { get; set; } = 0;
    public float BorderLengthLeft { get; set; } = 0;

    public float BorderLength 
    { 
        set
        {
            BorderLengthTop = value;
            BorderLengthRight = value;
            BorderLengthBottom = value;
            BorderLengthLeft = value;
        }
    }
}