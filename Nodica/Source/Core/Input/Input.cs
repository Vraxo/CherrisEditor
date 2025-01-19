using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Cherris;

public sealed class Input
{
    private static Input? _instance;
    public static Input Instance => _instance ??= new();

    private readonly Dictionary<string, List<InputAction>> actionKeyMap = [];
    private readonly string mappingsFilePath = "Res/Cherris/InputMappings.yaml";

    public static MouseCursorCode Cursor
    {
        set
        {
            Instance.CursorImpl = value;
        }
    }

    public static Vector2 MousePosition => Instance.GetMousePositionImpl();
    public static bool IsActionDown(string actionName) => Instance.IsActionDownImpl(actionName);
    public static bool IsActionPressed(string actionName) => Instance.IsActionPressedImpl(actionName);
    public static bool IsKeyPressed(KeyCode keyboardKey) => Instance.IsKeyPressedImpl(keyboardKey);
    public static bool IsKeyReleased(KeyCode keyboardKey) => Instance.IsKeyReleasedImpl(keyboardKey);
    public static bool IsKeyDown(KeyCode keyboardKey) => Instance.IsKeyDownImpl(keyboardKey);
    public static bool IsMouseButtonPressed(MouseButtonCode button) => Instance.IsMouseButtonPressedImpl(button);
    public static bool IsMouseButtonReleased(MouseButtonCode button) => Instance.IsMouseButtonReleasedImpl(button);
    public static bool IsMouseButtonDown(MouseButtonCode button) => Instance.IsMouseButtonDownImpl(button);
    public static Vector2 GetVector(string _x, string x, string _y, string y, float deadzone = -1.0f) => Instance.GetVectorImpl(_x, x, _y, y, deadzone);
    public static float GetMouseWheelMovement() => Instance.GetMouseWheelMovementImpl();

    private Input()
    {
        LoadMappings(mappingsFilePath);
    }

    private MouseCursorCode CursorImpl
    {
        set
        {
            Raylib_cs.Raylib.SetMouseCursor((Raylib_cs.MouseCursor)value);
        }
    }

    private bool IsActionDownImpl(string actionName)
    {
        if (actionKeyMap.TryGetValue(actionName, out List<InputAction>? actions))
        {
            foreach (InputAction action in actions)
            {
                if (action.IsDown())
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsActionPressedImpl(string actionName)
    {
        if (actionKeyMap.TryGetValue(actionName, out List<InputAction>? actions))
        {
            foreach (InputAction action in actions)
            {
                if (action.IsPressed())
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsKeyPressedImpl(KeyCode keyboardKey) => App.Instance.Backend.Input.IsKeyPressed(keyboardKey);
    private bool IsKeyReleasedImpl(KeyCode keyboardKey) => App.Instance.Backend.Input.IsKeyReleased(keyboardKey);
    private bool IsKeyDownImpl(KeyCode keyboardKey) => App.Instance.Backend.Input.IsKeyDown(keyboardKey);
    private bool IsMouseButtonPressedImpl(MouseButtonCode button) => App.Instance.Backend.Input.IsMouseButtonPressed(button);
    private bool IsMouseButtonReleasedImpl(MouseButtonCode button) => App.Instance.Backend.Input.IsMouseButtonReleased(button);
    private bool IsMouseButtonDownImpl(MouseButtonCode button) => App.Instance.Backend.Input.IsMouseButtonDown(button);
    private float GetMouseWheelMovementImpl() => App.Instance.Backend.Input.GetMouseWheelMovement();
    private Vector2 GetMousePositionImpl() => App.Instance.Backend.Input.GetMousePosition();

    private Vector2 GetVectorImpl(string negativeX, string positiveX, string negativeY, string positiveY, float deadzone = -1.0f)
    {
        float x = 0.0f;
        float y = 0.0f;

        if (IsActionDownImpl(negativeX))
        {
            x -= 1.0f;
        }

        if (IsActionDownImpl(positiveX))
        {
            x += 1.0f;
        }

        if (IsActionDownImpl(negativeY))
        {
            y -= 1.0f;
        }

        if (IsActionDownImpl(positiveY))
        {
            y += 1.0f;
        }

        if (deadzone >= 0.0f)
        {
            if (MathF.Abs(x) < deadzone)
            {
                x = 0.0f;
            }

            if (MathF.Abs(y) < deadzone)
            {
                y = 0.0f;
            }
        }

        return new(x, y);
    }

    private void LoadMappings(string filePath)
    {
        string yaml = File.ReadAllText(filePath);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(NullNamingConvention.Instance)
            .Build();

        var mappings = deserializer.Deserialize<Dictionary<string, List<InputAction>>>(yaml);

        if (mappings is null)
        {
            return;
        }

        actionKeyMap.Clear();

        foreach (var mapping in mappings)
        {
            actionKeyMap[mapping.Key] = mapping.Value;
        }
    }
}