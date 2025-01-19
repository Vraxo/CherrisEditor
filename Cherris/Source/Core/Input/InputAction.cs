using Cherris.Backends;

namespace Cherris;

public sealed class InputAction
{
    public string? Type { get; set; }
    public string? KeyOrButton { get; set; }

    private static Backend Backend => App.Instance.Backend;

    public bool IsPressed()
    {
        if (Type == "Keyboard")
        {
            if (Enum.TryParse(KeyOrButton, true, out KeyCode key))
            {
                return Backend.Input.IsKeyPressed(key);
            }
        }
        else if (Type == "MouseButton")
        {
            if (Enum.TryParse(KeyOrButton, true, out MouseButtonCode button))
            {
                return Backend.Input.IsMouseButtonPressed(button);
            }
        }

        return false;
    }

    public bool IsDown()
    {
        if (Type == "Keyboard")
        {
            if (Enum.TryParse(KeyOrButton, true, out KeyCode key))
            {
                return Backend.Input.IsKeyDown(key);
            }
        }
        else if (Type == "MouseButton")
        {
            if (Enum.TryParse(KeyOrButton, true, out MouseButtonCode button))
            {
                return Backend.Input.IsMouseButtonDown(button);
            }
        }

        return false;
    }
}
