using Raylib_cs;

namespace Cherris;

public abstract class BaseButton : Control
{
    public enum ClickMode { Limited, Limitless }
    public enum ActionMode { Release, Press }
    public enum ClickBehavior { Left, Right, Both }

    #region [ - - - Properties & Fields - - - ]

    public ClickMode Mode { get; set; } = ClickMode.Limited;
    public ActionMode LeftClickActionMode { get; set; } = ActionMode.Release;
    public ActionMode RightClickActionMode { get; set; } = ActionMode.Release;
    public bool StayPressed { get; set; } = false;
    public ClickBehavior Behavior { get; set; } = ClickBehavior.Both;

    public bool PressedLeft = false;
    public bool PressedRight = false;

    private bool _disabled = false;
    public bool Disabled
    {
        get => _disabled;
        set
        {
            _disabled = value;
            OnDisable();
        }
    }

    protected virtual void OnDisable() { }

    #endregion

    public override void Update()
    {
        if (!Disabled)
        {
            HandleClicks();
            HandleKeyboardInput();
        }

        base.Update();
    }

    private void HandleKeyboardInput()
    {
        if (Focused && Raylib.IsKeyPressed(KeyboardKey.Enter))
        {
            OnEnterKeyPressed();
        }
    }

    protected virtual void OnEnterKeyPressed() { }

    private void HandleClicks()
    {
        if (Disabled) return;

        bool mouseOver = IsMouseOver();
        bool anyPressed = false;

        if (Behavior == ClickBehavior.Left || Behavior == ClickBehavior.Both)
        {
            HandleClick(ref PressedLeft, MouseButtonCode.Left, LeftClickActionMode);
            if (PressedLeft) anyPressed = true;
        }

        if (Behavior == ClickBehavior.Right || Behavior == ClickBehavior.Both)
        {
            HandleClick(ref PressedRight, MouseButtonCode.Right, RightClickActionMode);
            if (PressedRight) anyPressed = true;
        }
    }

    private void HandleClick(ref bool pressed, MouseButtonCode button, ActionMode actionMode)
    {
        if (Disabled) return;

        bool mouseOver = IsMouseOver();

        if (mouseOver)
        {
            if (Input.IsMouseButtonPressed(button))
            {
                pressed = true;
                HandleClickFocus();

                if (actionMode == ActionMode.Press)
                {
                    OnClick();
                }
            }
        }

        if (Input.IsMouseButtonReleased(button))
        {
            if (mouseOver && pressed && actionMode == ActionMode.Release)
            {
                OnClick();
            }

            pressed = false;
        }
    }

    protected virtual void OnClick() { }
}