namespace Cherris;

public class ProgressBar : Node2D
{
    public bool ShowPercentage { get; set; } = true;
    public BoxTheme BackgroundTheme { get; set; } = new();
    public BoxTheme ProgressTheme { get; set; } = new();

    private float _percentage = 0;
    public float Percentage
    {
        get => _percentage;

        set
        {
            _percentage = Math.Clamp(value, 0, 1);
        }
    }

    public ProgressBar()
    {
        Size = new(250, 10);
        ProgressTheme.FillColor = DefaultTheme.Accent;
    }

    protected override void Draw()
    {
        DrawEmpty();
        DrawFilled();
    }

    private void DrawEmpty()
    {
        DrawRectangleThemed(
            GlobalPosition - Origin,
            Size,
            BackgroundTheme);
    }

    private void DrawFilled()
    {
        if (Percentage == 0)
        {
            return;
        }

        Vector2 filledSize = new(Size.X * Percentage, Size.Y);

        DrawRectangleThemed(
            GlobalPosition - Origin,
            filledSize,
            ProgressTheme);
    }
}