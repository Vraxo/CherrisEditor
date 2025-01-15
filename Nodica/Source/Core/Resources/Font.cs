using SDL2;
using Raylib_cs;
using Nodica.Backends;
using System.Buffers;

namespace Nodica;

public class Font
{
    public string Name = "";
    public int Size = 0;

    private Raylib_cs.Font raylibFont;
    private IntPtr sdl2Font;

    private static bool IsRaylibBackend => App.Instance.Backend is RaylibBackend;
    private static bool IsSdl2Backend => App.Instance.Backend is SDL2Backend;

    public Vector2 Dimensions
    {
        get
        {
            if (IsRaylibBackend)
            {
                return Raylib.MeasureTextEx(raylibFont, " ", Size, 0);
            }
            else if (IsSdl2Backend)
            {
                if (sdl2Font != IntPtr.Zero)
                {
                    _ = SDL_ttf.TTF_SizeText(sdl2Font, " ", out int width, out int height);
                    return new Vector2(width, height);
                }

                return Vector2.Zero;
            }
            return Vector2.Zero;
        }
    }

    public Font(string filePath, int size)
    {
        Size = size;
        Name = Path.GetFileNameWithoutExtension(filePath);

        if (IsRaylibBackend)
        {
            int[] codepoints = new int[255 - 32 + 1];
            for (int i = 0; i < codepoints.Length; i++)
            {
                codepoints[i] = 32 + i;
            }

            //raylibFont = Raylib.LoadFontEx(filePath, size, codepoints, codepoints.Length);
            //Raylib.SetTextureFilter(raylibFont.Texture, TextureFilter.Bilinear);
        }
        else if (IsSdl2Backend)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Font file '{filePath}' not found.");
                return;
            }

            sdl2Font = SDL_ttf.TTF_OpenFont(filePath, size);
            if (sdl2Font == IntPtr.Zero)
            {
                Console.WriteLine($"Failed to load font: {SDL.SDL_GetError()}");
            }
        }
    }

    public static implicit operator Raylib_cs.Font(Font textFont)
    {
        if (IsRaylibBackend)
        {
            return textFont.raylibFont;
        }
        return default;
    }

    public static implicit operator IntPtr(Font textFont)
    {
        if (IsSdl2Backend)
        {
            return textFont.sdl2Font;
        }
        return IntPtr.Zero;
    }

    public static Vector2 MeasureText(Font font, string text, int size, float spacing)
    {
        Vector2 measurements = Raylib.MeasureTextEx(
            font,
            text,
            size,
            spacing);

        return measurements;
    }
}