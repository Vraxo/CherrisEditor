using Raylib_cs;

namespace Cherris;

public sealed class TextureManager
{
    private static TextureManager? _instance;
    public static TextureManager Instance => _instance ??= new();

    private readonly Dictionary<string, Texture> textures = [];

    private TextureManager() { }

    public Texture Get(string path)
    {
        if (textures.TryGetValue(path, out Texture? value))
        {
            return value;
        }

        Texture textureWrapper = new(path);
        textures[path] = textureWrapper;

        return textureWrapper;
    }

    public void Remove(string path)
    {
        if (textures.TryGetValue(path, out Texture? value))
        {
            Raylib.UnloadTexture(value);
            textures.Remove(path);

            string pngPath = GetPngPath(path);

            if (pngPath != path && File.Exists(pngPath))
            {
                File.Delete(pngPath);
            }
        }
    }

    private string GetPngPath(string path)
    {
        string pngPath =
            Path.GetExtension(path).ToLower() == ".png" ?
            path :
            $"Res/Temporary/{Path.GetFileNameWithoutExtension(path)}.png";

        return pngPath;
    }
}