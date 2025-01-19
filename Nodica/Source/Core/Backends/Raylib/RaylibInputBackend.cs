using Raylib_cs;

namespace Cherris.Backends;

public sealed class RaylibInputBackend : IInputBackend
{
    public override bool IsKeyPressed(KeyCode key)
    {
        return Raylib.IsKeyPressed((KeyboardKey)key);
    }

    public override bool IsKeyReleased(KeyCode key)
    {
        return Raylib.IsKeyReleased((KeyboardKey)key);
    }

    public override bool IsKeyDown(KeyCode key)
    {
        return Raylib.IsKeyDown((KeyboardKey)key);
    }

    public override bool IsMouseButtonPressed(MouseButtonCode button)
    {
        return Raylib.IsMouseButtonPressed((MouseButton)button);
    }

    public override bool IsMouseButtonReleased(MouseButtonCode button)
    {
        return Raylib.IsMouseButtonReleased((MouseButton)button);
    }

    public override bool IsMouseButtonDown(MouseButtonCode button)
    {
        return Raylib.IsMouseButtonDown((MouseButton)button);
    }

    public override Vector2 GetMousePosition()
    {
        return Raylib.GetMousePosition();
    }

    public override float GetMouseWheelMovement()
    {
        return Raylib.GetMouseWheelMove();
    }
}