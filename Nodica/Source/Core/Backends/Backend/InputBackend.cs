namespace Cherris.Backends;

public abstract class IInputBackend
{
    public abstract bool IsKeyPressed(KeyCode key);
    public abstract bool IsKeyReleased(KeyCode key);
    public abstract bool IsKeyDown(KeyCode key);
    public abstract bool IsMouseButtonPressed(MouseButtonCode button);
    public abstract bool IsMouseButtonReleased(MouseButtonCode button);
    public abstract bool IsMouseButtonDown(MouseButtonCode button);
    public abstract Vector2 GetMousePosition();
    public abstract float GetMouseWheelMovement();
}