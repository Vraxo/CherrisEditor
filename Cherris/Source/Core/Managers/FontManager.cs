namespace Cherris;

public sealed class FontManager
{
    private static FontManager? _instance;
    public static FontManager Instance => _instance ??= new();

    private readonly Dictionary<string, Font> fonts = new();

    private FontManager() { }

    // Get the font by the full string (e.g., "Res/Fonts/FontName.ttf:32")
    public Font Get(string fontKey)
    {
        if (fonts.TryGetValue(fontKey, out Font? font))
        {
            return font;
        }

        (string fontPath, int fontSize) = ParseFontKey(fontKey);

        // Load the font and store it
        Font newFont = new(fontPath, fontSize);
        fonts.Add(fontKey, newFont);

        return newFont;
    }

    // Parses the font key string to extract the font path and size
    private static (string fontPath, int fontSize) ParseFontKey(string fontKey)
    {
        int colonIndex = fontKey.LastIndexOf(':');

        if (colonIndex == -1)
        {
            throw new ArgumentException($"Invalid font key format: {fontKey}. Expected format: 'FontPath:Size'.");
        }

        string fontPath = fontKey[..colonIndex];
        string sizeString = fontKey[(colonIndex + 1)..];

        if (!int.TryParse(sizeString, out int fontSize))
        {
            throw new ArgumentException($"Invalid font size in: {fontKey}. Size must be a valid integer.");
        }

        return (fontPath, fontSize);
    }
}
