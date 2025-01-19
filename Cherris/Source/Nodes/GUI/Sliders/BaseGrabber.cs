using Raylib_cs;

namespace Cherris;

public partial class BaseSlider
{
    public abstract class BaseGrabber : ClickableRectangle
    {
        public ButtonThemePack Themes = new();
        public bool Pressed = false;
        public Action<BaseGrabber> OnUpdate = (button) => { };

        protected bool alreadyClicked = false;
        protected bool initialPositionSet = false;

        public BaseGrabber()
        {
            Size = new(18, 18);
            InheritPosition = false;
        }

        public override void Start()
        {
            UpdatePosition(true);
            base.Start();
        }

        public override void Update()
        {
            OnUpdate(this);
            UpdatePosition();
            CheckForClicks();
            Draw();
            base.Update();
        }

        protected abstract void UpdatePosition(bool initial = false);

        private void CheckForClicks()
        {
            if (Input.IsMouseButtonDown(MouseButtonCode.Left))
            {
                if (!IsMouseOver())
                {
                    alreadyClicked = true;
                }
            }

            if (IsMouseOver())
            {
                Themes.Current = Themes.Hover;

                if (Input.IsMouseButtonDown(MouseButtonCode.Left) && !alreadyClicked && OnTopLeft)
                {
                    OnTopLeft = false;
                    Pressed = true;
                    alreadyClicked = true;
                }

                if (Pressed)
                {
                    Themes.Current = Themes.Pressed;
                }
            }
            else
            {
                Themes.Current = Themes.Normal;
            }

            if (Pressed)
            {
                Themes.Current = Themes.Pressed;
            }

            if (Input.IsMouseButtonReleased(MouseButtonCode.Left))
            {
                Pressed = false;
                Themes.Current = Themes.Normal;
                alreadyClicked = false;
            }
        }

        protected override void Draw()
        {
            DrawShape();
        }

        private void DrawShape()
        {
            DrawOutline();
            DrawInside();
            DrawRectangleThemed(
                GlobalPosition,
                Size,
                Themes.Current);
        }

        private void DrawInside()
        {
            //Rectangle rectangle = new()
            //{
            //    Position = GlobalPosition - Offset,
            //    Dimensions = Dimensions
            //};
            //
            //Raylib.DrawRectangleRounded(
            //    rectangle,
            //    Themes.Current.Roundness,
            //    (int)Dimensions.Y,
            //    Themes.Current.FillColor);
        }

        private void DrawOutline()
        {
            //if (Themes.Current.BorderLength <= 0)
            //{
            //    return;
            //}
            //
            //Vector2 position = GlobalPosition - Offset;
            //
            //Rectangle rectangle = new()
            //{
            //    Position = position,
            //    Dimensions = Dimensions
            //};
            //
            //for (int i = 0; i <= Themes.Current.BorderLength; i++)
            //{
            //    Rectangle outlineRectangle = new()
            //    {
            //        Position = rectangle.Position - new Vector2(i, i),
            //        Dimensions = new(rectangle.Dimensions.X + i + 1, rectangle.Dimensions.Y + i + 1)
            //    };
            //
            //    Raylib.DrawRectangleRounded(
            //        outlineRectangle,
            //        Themes.Current.Roundness,
            //        (int)rectangle.Dimensions.X,
            //        Themes.Current.BorderColor);
            //}
        }

        //private void Draw()
        //{
        //    if (!(Visible && ReadyForVisibility))
        //    {
        //        return;
        //    }
        //
        //    float x = (float)Math.Round(GlobalPosition.X);
        //    float y = (float)Math.Round(GlobalPosition.Y);
        //
        //    Vector2 temporaryPosition = new(x, y);
        //
        //    DrawShapeOutline(temporaryPosition);
        //    DrawShapeInside(temporaryPosition);
        //}
        //
        //private void DrawShapeInside(Vector2 position)
        //{
        //    Rectangle rectangle = new()
        //    {
        //        position = position - Offset,
        //        Dimensions = Dimensions
        //    };
        //
        //    Raylib.DrawRectangleRounded(
        //        rectangle,
        //        BackgroundTheme.Current.Roundness,
        //        (int)Dimensions.Y,
        //        BackgroundTheme.Current.FillColor);
        //}
        //
        //private void DrawShapeOutline(Vector2 position)
        //{
        //    if (BackgroundTheme.Current.BorderLength < 0)
        //    {
        //        return;
        //    }
        //
        //    for (int i = 0; i <= BackgroundTheme.Current.BorderLength; i++)
        //    {
        //        Rectangle rectangle = new()
        //        {
        //            position = position - Offset - new Vector2(i, i),
        //            Dimensions = new(Dimensions.X + i + 1, Dimensions.Y + i + 1)
        //        };
        //
        //        Raylib.DrawRectangleRounded(
        //            rectangle,
        //            BackgroundTheme.Current.Roundness,
        //            (int)Dimensions.Y,
        //            BackgroundTheme.Current.BorderColor);
        //    }
        //}
    }
}