using Raylib_cs;

namespace Cherris;

public partial class LineEdit : Button
{
    protected class Caret : VisualItem
    {
        #region [ - - - Properties & Fields - - - ]

        private Vector2 position = Vector2.Zero;

        public float MaxTime = 0.5F;
        private const byte minAlpha = 0;
        private const byte maxAlpha = 255;
        private float timer = 0;
        private byte alpha = maxAlpha;
        private LineEdit parent;

        private float arrowKeyTimer = 0f;
        private const float arrowKeyDelay = 0.5f;
        private const float arrowKeySpeed = 0.05f;
        private bool arrowKeyHeld = false;
        private bool movingRight = false;

        private int _x = 0;
        public int X
        {
            get => _x;
            set
            {
                int maxVisibleChars = Math.Max(0, Math.Min(parent.GetDisplayableCharactersCount(), parent.Text.Length - parent.TextStartIndex));
                _x = Math.Clamp(value, 0, maxVisibleChars);
                alpha = maxAlpha;
            }
        }


        //private int _x = 0;
        //public int X
        //{
        //    get => _x;
        //    set
        //    {
        //        _x = Math.Clamp(value, 0, Math.Min(parent.Text.Length, parent.GetDisplayableCharactersCount()));
        //        alpha = maxAlpha;
        //    }
        //}

        private Vector2 GlobalPosition => parent.GlobalPosition + position;

        #endregion

        public Caret(LineEdit parent)
        {
            this.parent = parent;
        }

        public void Update()
        {
            if (!parent.Selected) return;

            HandleInput();
            Draw();
            UpdateAlpha();
            base.Update();
        }

        private void Draw()
        {
            DrawText(
                "|",
                GetPosition(),
                parent.Themes.Current.Font,
                parent.Themes.Current.FontSize,
                parent.Themes.Current.FontSpacing,
                GetColor());
        }

        private void HandleInput()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Right) || Raylib.IsKeyPressed(KeyboardKey.Left))
            {
                movingRight = Raylib.IsKeyPressed(KeyboardKey.Right);
                arrowKeyTimer = 0f;
                MoveCaret(movingRight ? 1 : -1);
            }

            if (Raylib.IsKeyDown(KeyboardKey.Right) || Raylib.IsKeyDown(KeyboardKey.Left))
            {
                arrowKeyTimer += Raylib.GetFrameTime();
                if (arrowKeyTimer >= arrowKeyDelay && arrowKeyTimer % arrowKeySpeed < Raylib.GetFrameTime())
                {
                    MoveCaret(movingRight ? 1 : -1);
                }
            }

            if (Input.IsMouseButtonPressed(MouseButtonCode.Left))
            {
                MoveIntoPosition(Raylib.GetMousePosition().X);
            }
        }

        private void MoveCaret(int direction)
        {
            if (direction > 0 && X == parent.GetDisplayableCharactersCount() && X < parent.Text.Length - parent.TextStartIndex)
            {
                parent.TextStartIndex++;
            }
            else if (direction < 0 && X == 0 && parent.Text.Length > parent.GetDisplayableCharactersCount() && parent.TextStartIndex > 0)
            {
                parent.TextStartIndex--;
            }

            X += direction;
        }

        public void MoveIntoPosition(float mouseX)
        {
            if (parent.Text.Length == 0)
            {
                X = 0;
            }
            else
            {
                float x = mouseX - (parent.GlobalPosition.X - parent.Origin.X) - parent.TextOrigin.X;
                float characterWidth = parent.Themes.Current.Font.Dimensions.X;
                X = Math.Clamp((int)MathF.Floor(x / characterWidth), 0, Math.Min(parent.GetDisplayableCharactersCount(), parent.Text.Length));
            }
        }

        private Vector2 GetPosition()
        {
            Vector2 size = parent.Themes.Current.Font.Dimensions;

            float x = GlobalPosition.X - parent.Origin.X + parent.TextOrigin.X + X * size.X - size.X / 2 + X - X;
            float y = GlobalPosition.Y + parent.Size.Y / 2 - size.Y / 2 - parent.Origin.Y;

            return new(x, y);
        }

        private Color GetColor()
        {
            Color color = parent.Themes.Current.FontColor;
            color.A = alpha;

            return color;
        }

        private void UpdateAlpha()
        {
            timer += Raylib.GetFrameTime();

            if (timer > MaxTime)
            {
                alpha = alpha == maxAlpha ? minAlpha : maxAlpha;
                timer = 0;
            }
        }
    }
}