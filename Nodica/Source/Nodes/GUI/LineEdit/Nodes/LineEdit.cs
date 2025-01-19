using Raylib_cs;
using System.Collections.Generic;

namespace Cherris;

public partial class LineEdit : Button
{
    #region [ - - - Properties & Fields - - - ]

    public static readonly Vector2 DefaultSize = new(300, 26);

    public new string Text { get; set; } = "";
    public string DefaultText { get; set; } = "";
    public string PlaceholderText { get; set; } = "";
    public Vector2 TextOrigin { get; set; } = new(8, 0);
    public int MaxCharacters { get; set; } = int.MaxValue;
    public int MinCharacters { get; set; } = 0;
    public List<char> ValidCharacters { get; set; } = [];
    public bool Selected { get; set; } = false;
    public bool Editable { get; set; } = true;
    public bool ExpandWidthToText { get; set; } = false;
    public bool RevertToDefaultText { get; set; } = true;
    public bool TemporaryDefaultText { get; set; } = true;
    public bool Secret { get; set; } = false;
    public char SecretCharacter { get; set; } = '*';
    public int TextStartIndex { get; private set; } = 0;

    public event EventHandler? FirstCharacterEntered;
    public event EventHandler? Cleared;
    public event EventHandler<string>? TextChanged;
    public event EventHandler<string>? Confirmed;

    protected Caret caret;
    private readonly TextDisplayer textDisplayer;
    private readonly PlaceholderTextDisplayer placeholderTextDisplayer;

    private const int minAscii = 32;
    private const int maxAscii = 125;
    private const float backspaceDelay = 0.5f;
    private const float backspaceSpeed = 0.05f;
    private const float undoDelay = 0.5f;
    private const float undoSpeed = 0.05f;

    private float backspaceTimer = 0f;
    private bool backspaceHeld = false;
    private float undoTimer = 0f;
    private bool undoHeld = false;

    private float previousWidth = 0;

    private Stack<LineEditState> undoStack = new();
    private Stack<LineEditState> redoStack = new();
    private const int historyLimit = 50; // Limit the number of undo/redo states

    #endregion

    public LineEdit()
    {
        Size = DefaultSize;
        FocusOnClick = true;
        Themes.Focused.BorderLength = 0;
        Themes.Focused.BorderLengthBottom = 1;

        caret = new(this);
        textDisplayer = new(this);
        placeholderTextDisplayer = new(this);

        FocusChanged += OnFocusChanged;
        LeftClicked += OnLeftClicked;
        ClickedOutside += OnClickedOutside;
        LayerChanged += OnLayerChanged;
        SizeChanged += OnSizeChanged;
    }

    public override void Update()
    {
        HandleInput();
        PasteText();
        HandleUndoRedo();
        UpdateSizeToFitText();

        base.Update();

        caret?.Update();
        textDisplayer?.Update();
        placeholderTextDisplayer?.Update();
    }

    protected override void OnEnterPressed()
    {
        Selected = false;
        Focused = false;
        Themes.Current = base.Themes.Normal;

        base.OnEnterPressed();
    }

    private void OnLayerChanged(VisualItem sender, int layer)
    {
        caret.Layer = layer + 1;
        textDisplayer.Layer = layer + 1;
        placeholderTextDisplayer.Layer = layer + 1;
    }

    private void OnClickedOutside(object? sender, EventArgs e)
    {
        Selected = false;
    }

    private void OnLeftClicked(Button sender)
    {
        Selected = true;
    }

    private void OnFocusChanged(object? sender, bool e)
    {
        Selected = e;
    }

    private void OnSizeChanged(object? sender, Vector2 e)
    {
        if (previousWidth != e.X)
        {
            previousWidth = e.X;
            TextStartIndex = 0;
        }
    }

    private void UpdateSizeToFitText()
    {
        if (!ExpandWidthToText)
        {
            return;
        }

        int textWidth = (int)Raylib.MeasureTextEx(
            Themes.Current.Font,
            Text,
            Themes.Current.FontSize,
            Themes.Current.FontSpacing).X;

        Size = new(textWidth + TextOrigin.X * 2, Size.Y);
    }

    public void Insert(string input)
    {
        if (!Editable)
        {
            return;
        }

        InsertTextAtCaret(input);
    }

    private void HandleInput()
    {
        if (!Editable)
        {
            return;
        }

        if (!Selected)
        {
            return;
        }

        GetTypedCharacters();
        HandleBackspace();
        Confirm();
    }

    private void GetTypedCharacters()
    {
        int key = Raylib.GetCharPressed();

        while (key > 0)
        {
            InsertCharacter(key);
            key = Raylib.GetCharPressed();
        }
    }

    private void InsertCharacter(int key)
    {
        bool isKeyInRange = key >= minAscii && key <= maxAscii;
        bool isSpaceLeft = Text.Length < MaxCharacters;

        if (isKeyInRange && isSpaceLeft)
        {
            if (ValidCharacters.Count > 0 && !ValidCharacters.Contains((char)key))
            {
                return;
            }

            PushStateForUndo();

            if (TemporaryDefaultText && Text == DefaultText)
            {
                Text = "";
            }

            if (caret.X < 0 || caret.X > Text.Length)
            {
                caret.X = Text.Length;
            }

            Text = Text.Insert(caret.X + TextStartIndex, ((char)key).ToString());

            // Check if caret is out of view, and adjust TextStartIndex
            if (caret.X >= GetDisplayableCharactersCount())
            {
                TextStartIndex++;
            }
            else
            {
                caret.X++;
            }

            TextChanged?.Invoke(this, Text);

            if (Text.Length == 1)
            {
                FirstCharacterEntered?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void InsertTextAtCaret(string text)
    {
        bool isSpaceLeft = Text.Length + text.Length <= MaxCharacters;

        if (isSpaceLeft)
        {
            PushStateForUndo();

            if (TemporaryDefaultText && Text == DefaultText)
            {
                Text = "";
            }

            if (caret.X < 0 || caret.X > Text.Length)
            {
                caret.X = Text.Length;
            }

            Text = Text.Insert(caret.X + TextStartIndex, text);
            caret.X += text.Length;

            // Shift text if caret moves out of view
            if (caret.X > GetDisplayableCharactersCount())
            {
                TextStartIndex = caret.X - GetDisplayableCharactersCount();
            }

            TextChanged?.Invoke(this, Text);

            if (Text.Length == text.Length)
            {
                FirstCharacterEntered?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void HandleBackspace()
    {
        bool ctrlHeld = Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl);
        bool backspacePressed = Raylib.IsKeyPressed(KeyboardKey.Backspace);

        if (backspacePressed)
        {
            backspaceHeld = true;
            backspaceTimer = 0f;

            if (ctrlHeld)
            {
                DeletePreviousWord();
            }
            else
            {
                DeleteLastCharacter();
            }
        }
        else if (Raylib.IsKeyDown(KeyboardKey.Backspace) && backspaceHeld)
        {
            backspaceTimer += Raylib.GetFrameTime();

            if (backspaceTimer >= backspaceDelay)
            {
                if (backspaceTimer % backspaceSpeed < Raylib.GetFrameTime())
                {
                    if (ctrlHeld)
                    {
                        DeletePreviousWord();
                    }
                    else
                    {
                        DeleteLastCharacter();
                    }
                }
            }
        }

        if (Raylib.IsKeyReleased(KeyboardKey.Backspace))
        {
            backspaceHeld = false;
        }
    }

    private void DeletePreviousWord()
    {
        if (Text.Length == 0 || (caret.X == 0 && TextStartIndex == 0)) return;

        PushStateForUndo();

        // Calculate the actual index within the full text based on caret position and TextStartIndex
        int removeIndex = caret.X + TextStartIndex - 1;

        // If we're at the very start of the text that's displayed but there is more hidden to the left
        if (caret.X == 0 && TextStartIndex > 0)
        {
            // Shift TextStartIndex left, and move removeIndex accordingly
            TextStartIndex--;
            removeIndex = TextStartIndex;
        }

        // Find the start of the previous word by moving back from removeIndex
        int wordStartIndex = removeIndex;
        while (wordStartIndex > 0 && Text[wordStartIndex - 1] != ' ')
        {
            wordStartIndex--;
        }

        // Calculate the number of characters to delete
        int lengthToDelete = removeIndex - wordStartIndex + 1;
        Text = Text.Remove(wordStartIndex, lengthToDelete);

        // Adjust TextStartIndex if characters were deleted from the hidden portion
        if (wordStartIndex < TextStartIndex)
        {
            int charactersRemoved = removeIndex - wordStartIndex + 1;
            TextStartIndex = Math.Max(0, TextStartIndex - charactersRemoved);
        }

        // Reposition the caret
        caret.X = Math.Clamp(wordStartIndex - TextStartIndex, 0, GetDisplayableCharactersCount());

        RevertTextToDefaultIfEmpty();
        TextChanged?.Invoke(this, Text);

        // Trigger Cleared event if the text is now empty
        if (Text.Length == 0)
        {
            Cleared?.Invoke(this, EventArgs.Empty);
        }
    }

    private void DeleteLastCharacter()
    {
        int textLengthBeforeDeletion = Text.Length;

        if (Text.Length > 0)
        {
            PushStateForUndo();

            if (caret.X == 0 && TextStartIndex > 0)
            {
                TextStartIndex--;
                Text = Text.Remove(TextStartIndex, 1);
            }
            else if (caret.X > 0)
            {
                int removeIndex = caret.X - 1 + TextStartIndex;
                if (removeIndex >= TextStartIndex && removeIndex < Text.Length)
                {
                    Text = Text.Remove(removeIndex, 1);
                    caret.X = Math.Clamp(caret.X - 1, 0, Math.Min(Text.Length, GetDisplayableCharactersCount()));
                }
            }
        }

        RevertTextToDefaultIfEmpty();
        TextChanged?.Invoke(this, Text);

        if (Text.Length == 0 && textLengthBeforeDeletion != 0)
        {
            Cleared?.Invoke(this, EventArgs.Empty);
        }
    }

    private void PasteText()
    {
        bool pressedLeftControl = Raylib.IsKeyDown(KeyboardKey.LeftControl);
        bool pressedV = Raylib.IsKeyPressed(KeyboardKey.V);

        if (pressedLeftControl && pressedV)
        {
            string clipboardText = Raylib.GetClipboardText_();

            if (!string.IsNullOrEmpty(clipboardText))
            {
                PushStateForUndo(); // Store the state before pasting

                string textBeforePaste = Text;
                int caretBeforePaste = caret.X;
                int startIndexBeforePaste = TextStartIndex;

                char[] clipboardContent = [.. clipboardText];

                foreach (char character in clipboardContent)
                {
                    if (character == '\r' || character == '\n') continue; // Ignore line breaks
                    InsertCharacterInternal(character);
                }

                // Store the state after pasting for undo (capturing the entire paste operation)
                if (undoStack.Count > 0)
                {
                    undoStack.Peek().Text = textBeforePaste;
                    undoStack.Peek().CaretPosition = caretBeforePaste;
                    undoStack.Peek().TextStartIndex = startIndexBeforePaste;
                }
            }
        }
    }

    private void InsertCharacterInternal(int key)
    {
        bool isKeyInRange = key >= minAscii && key <= maxAscii;
        bool isSpaceLeft = Text.Length < MaxCharacters;

        if (isKeyInRange && isSpaceLeft)
        {
            if (ValidCharacters.Count > 0 && !ValidCharacters.Contains((char)key))
            {
                return;
            }

            if (TemporaryDefaultText && Text == DefaultText)
            {
                Text = "";
            }

            if (caret.X < 0 || caret.X > Text.Length)
            {
                caret.X = Text.Length;
            }

            Text = Text.Insert(caret.X + TextStartIndex, ((char)key).ToString());

            // Check if caret is out of view, and adjust TextStartIndex
            if (caret.X >= GetDisplayableCharactersCount())
            {
                TextStartIndex++;
            }
            else
            {
                caret.X++;
            }

            TextChanged?.Invoke(this, Text);

            if (Text.Length == 1)
            {
                FirstCharacterEntered?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void Confirm()
    {
        if (Raylib.IsKeyDown(KeyboardKey.Enter))
        {
            Selected = false;
            Themes.Current = Themes.Normal;
            Confirmed?.Invoke(this, Text);
        }
    }

    private void RevertTextToDefaultIfEmpty()
    {
        if (Text.Length == 0 && RevertToDefaultText)
        {
            Text = DefaultText;
        }
    }

    private int GetDisplayableCharactersCount()
    {
        float availableWidth = Size.X - TextOrigin.X * 2;

        float oneCharacterWidth = Raylib.MeasureTextEx(
            Themes.Current.Font,
            ".",
            Themes.Current.FontSize,
            Themes.Current.FontSpacing).X;

        int displayableCharactersCount = (int)(availableWidth / oneCharacterWidth);

        return displayableCharactersCount;
    }

    private void PushStateForUndo()
    {
        if (undoStack.Count >= historyLimit)
        {
            undoStack.Pop();
        }
        undoStack.Push(new LineEditState(Text, caret.X, TextStartIndex));
        redoStack.Clear(); // Clear redo stack when a new action is performed
    }

    private void HandleUndoRedo()
    {
        bool ctrlHeld = Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl);

        // Undo
        if (ctrlHeld && Raylib.IsKeyPressed(KeyboardKey.Z))
        {
            Undo();
            undoHeld = true;
            undoTimer = 0f;
        }
        else if (ctrlHeld && Raylib.IsKeyDown(KeyboardKey.Z) && undoHeld)
        {
            undoTimer += Raylib.GetFrameTime();
            if (undoTimer >= undoDelay)
            {
                if (undoTimer % undoSpeed < Raylib.GetFrameTime())
                {
                    Undo();
                }
            }
        }
        if (Raylib.IsKeyReleased(KeyboardKey.Z))
        {
            undoHeld = false;
        }

        // Redo
        if (ctrlHeld && Raylib.IsKeyPressed(KeyboardKey.Y))
        {
            Redo();
        }
    }

    public void Undo()
    {
        if (undoStack.Count > 0)
        {
            LineEditState previousState = undoStack.Pop();

            // Push the current state to the redo stack
            if (redoStack.Count >= historyLimit)
            {
                redoStack.Push(new LineEditState(Text, caret.X, TextStartIndex));
            }
            else
            {
                redoStack.Push(new LineEditState(Text, caret.X, TextStartIndex));
            }

            Text = previousState.Text;
            caret.X = previousState.CaretPosition;
            TextStartIndex = previousState.TextStartIndex;
            TextChanged?.Invoke(this, Text);
        }
    }

    public void Redo()
    {
        if (redoStack.Count > 0)
        {
            LineEditState nextState = redoStack.Pop();

            // Push the current state to the undo stack
            if (undoStack.Count >= historyLimit)
            {
                undoStack.Push(new LineEditState(Text, caret.X, TextStartIndex));
            }
            else
            {
                undoStack.Push(new LineEditState(Text, caret.X, TextStartIndex));
            }

            Text = nextState.Text;
            caret.X = nextState.CaretPosition;
            TextStartIndex = nextState.TextStartIndex;
            TextChanged?.Invoke(this, Text);
        }
    }
}

internal class LineEditState(string text, int caretPosition, int textStartIndex)
{
    public string Text { get; set; } = text;
    public int CaretPosition { get; set; } = caretPosition;
    public int TextStartIndex { get; set; } = textStartIndex;
}