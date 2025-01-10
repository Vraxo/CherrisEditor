//using Raylib_cs;
//
//Nodica Nodica;
//
//class VerticalRectangleScrollBar : Node2D
//{
//    public Action<VerticalRectangleScrollBar> OnUpdate = (slider) => { };
//    public OptionButtonButton Grabber;
//    public OptionButtonButton TopButton;
//    public OptionButtonButton BottomButton;
//
//    private float _value = 0;
//
//    public float Value
//    {
//        get => _value;
//
//        set
//        {
//            _value = value;
//            _value = Math.Clamp(_value, 0, 1);
//        }
//    }
//
//    public override void Ready()
//    {
//        AddTopButton();
//        AddBottomButton();
//        AddMiddleButton();
//    }
//
//    public override void Process()
//    {
//        Value = Grabber.position.Y / (Dimensions.Y - Grabber.Dimensions.Y);
//        BottomButton.position.Y = Dimensions.Y + 8;
//
//        DrawRectangle();
//        SnapMiddleButtonToClickedPosition();
//
//        OnUpdate(this);
//
//        base.Process();
//    }
//
//    private void OnTopButtonLeftClicked(object? sender, EventArgs e)
//    {
//        Grabber.position.Y -= Grabber.Dimensions.Y;
//        Value -= BottomButton.Dimensions.Y / Dimensions.Y;
//    }
//
//    private void DrawRectangle()
//    {
//        Rectangle rectangle = new()
//        {
//            position = GlobalPosition - Offset,
//            Dimensions = Dimensions
//        };
//        
//        Raylib.DrawRectangleRounded(rectangle, 0.0F, (int)Dimensions.X, FillColor.DarkGray);
//    }
//
//    private void AddTopButton()
//    {
//        TopButton = new()
//        {
//            position = new(0, -8),
//            Dimensions = new(Dimensions.X, 10),
//        };
//
//        TopButton.OnLeftClick += OnTopButtonLeftClicked;
//
//        AddChild(TopButton);
//    }
//
//    private void AddBottomButton()
//    {
//        BottomButton = new()
//        {
//            position = new(0, Dimensions.Y),
//            Dimensions = new(Dimensions.X, 10),
//        };
//
//        BottomButton.OnLeftClick += OnBottomButtonLeftClicked;
//
//        AddChild(BottomButton);
//    }
//
//    private void OnBottomButtonLeftClicked(object? sender, EventArgs e)
//    {
//        Grabber.position.Y += Grabber.Dimensions.Y;
//        Value += BottomButton.Dimensions.Y / Dimensions.Y;
//    }
//
//    private void AddMiddleButton()
//    {
//        Grabber = new()
//        {
//            Dimensions = new(Dimensions.X, Dimensions.X),
//            Offset = new(Dimensions.X / 2, 0),
//            OriginPreset = OriginPreset.TopCenter,
//            OnUpdate = (OptionButtonButton) =>
//            {
//                if (OptionButtonButton.PressedLeft)
//                {
//                    OptionButtonButton.position.Y = Raylib.GetMousePosition().Y - OptionButtonButton.Dimensions.Y;
//                }
//
//                float maxY = Dimensions.Y - OptionButtonButton.Dimensions.Y;
//
//                if (OptionButtonButton.position.Y > maxY)
//                {
//                    OptionButtonButton.position.Y = maxY;
//                }
//
//                if (OptionButtonButton.position.Y < 0)
//                {
//                    OptionButtonButton.position.Y = 0;
//                }
//            }
//        };
//
//        AddChild(Grabber);
//
//        Grabber.ProgressTheme.Normal.FillColor = new(128, 128, 128, 255);
//    }
//
//    private void SnapMiddleButtonToClickedPosition()
//    {
//        if (IsMouseOver() && Raylib.IsMouseButtonPressed(MouseKey.Left))
//        {
//            Grabber.position.Y = Raylib.GetMousePosition().Y - Grabber.Dimensions.Y;
//        }
//    }
//
//    private bool IsMouseOver()
//    {
//        Vector2 mousePosition = Raylib.GetMousePosition();
//
//        bool matchX1 = mousePosition.X > GlobalPosition.X - Offset.X;
//        bool matchX2 = mousePosition.X < GlobalPosition.X + Dimensions.X - Offset.X;
//
//        bool matchY1 = mousePosition.Y > GlobalPosition.Y - Offset.Y;
//        bool matchY2 = mousePosition.Y < GlobalPosition.Y + Dimensions.Y - Offset.Y;
//
//        return matchX1 && matchX2 && matchY1 && matchY2;
//    }
//}