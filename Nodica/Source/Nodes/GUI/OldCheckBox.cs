//using Raylib_cs;
//
//namespace Cherris;
//
//public class CheckBox : ClickableRectangle
//{
//    public Vector2 CheckSize = new(10, 10);
//    public ButtonThemePack BackgroundStyle = new();
//    public ButtonThemePack CheckStyle = new();
//    public bool Selected = false;
//    public Action<CheckBox> OnUpdate = (checkBox) => { };
//    public event EventHandler? Selected;
//
//    public CheckBox()
//    {
//        Dimensions = new(20, 20);
//        OriginPreset = OriginPreset.Center;
//
//        BackgroundStyle.Roundness = 1;
//
//        CheckStyle.Normal.FillColor = new(71, 114, 179, 255);
//        CheckStyle.Current = CheckStyle.Normal;
//    }
//
//    public override void Process()
//    {
//        Draw();
//        HandleClicks();
//        OnUpdate(this);
//        base.Process();
//    }
//
//    protected override void Draw()
//    {
//        Rectangle rectangle = new()
//        {
//            Position = GlobalPosition - Offset,
//            Dimensions = Dimensions
//        };
//
//        DrawInside(rectangle);
//        DrawOutline(rectangle);
//        DrawCheck();
//    }
//
//    private void DrawInside(Rectangle rectangle)
//    {
//        //Raylib.DrawRectangleRounded(
//        //    rectangle,
//        //    BackgroundStyle.Current.Roundness,
//        //    (int)Dimensions.Y,
//        //    BackgroundStyle.Current.FillColor);
//    }
//
//    private void DrawOutline(Rectangle rectangle)
//    {
//        //if (BackgroundStyle.Current.BorderLength > 0)
//        //{
//        //    Raylib.DrawRectangleRoundedLines(
//        //        rectangle,
//        //        BackgroundStyle.Current.Roundness,
//        //        (int)Dimensions.Y,
//        //        BackgroundStyle.Current.BorderLength,
//        //        BackgroundStyle.Current.BorderColor);
//        //}
//    }
//
//    private void DrawCheck()
//    {
//        if (!Selected)
//        {
//            return;
//        }
//
//        Rectangle rectangle = new()
//        {
//            Position = GlobalPosition - Offset / 2,
//            Dimensions = CheckSize
//        };
//
//        Raylib.DrawRectangleRounded(
//            rectangle,
//            BackgroundStyle.Current.Roundness,
//            (int)CheckSize.Y,
//            CheckStyle.Current.FillColor);
//    }
//
//    private void HandleClicks()
//    {
//        if (Raylib.IsMouseButtonPressed(MouseKey.Left))
//        {
//            if (IsMouseOver() && OnTopLeft)
//            {
//                Selected = !Selected;
//                Selected?.Invoke(this, EventArgs.Empty);
//            }
//        }
//    }
//}