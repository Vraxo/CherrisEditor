namespace Cherris;

public partial class LineEdit : Button
{
    private abstract class BaseText(LineEdit parent) : Node2D
    {
        protected LineEdit parent = parent;

        private new Vector2 GlobalPosition => parent.GlobalPosition + Position;

        protected override void Draw()
        {
            if (!parent.Visible || ShouldSkipDrawing())
            {
                return;
            }

            DrawText(
                GetText(),
                GetPosition(),
                parent.Themes.Current.Font,
                parent.Themes.Current.FontSize,
                parent.Themes.Current.FontSpacing,
                parent.Themes.Current.FontColor);
        }

        protected Vector2 GetPosition()
        {
            Vector2 position = new(GetX(), GetY());
            return position;
        }

        private int GetX()
        {
            int x = (int)(GlobalPosition.X - parent.Origin.X + parent.TextOrigin.X);
            return x;
        }

        private int GetY()
        {
            int halfFontHeight = (int)parent.Themes.Current.Font.Dimensions.Y / 2;
            int y = (int)(GlobalPosition.Y + (parent.Size.Y / 2) - halfFontHeight - parent.Origin.Y);
            return y;
        }

        protected abstract string GetText();

        protected abstract bool ShouldSkipDrawing();
    }
}