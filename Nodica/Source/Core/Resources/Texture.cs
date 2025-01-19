using Raylib_cs;
using SixLabors.ImageSharp;
using Cherris.Backends;
using SDL2;

namespace Cherris;

public class Texture
{
    public Vector2 Size { get; private set; } = Vector2.Zero;
    private Texture2D raylibTexture;
    private readonly IntPtr sdl2Texture;

    private static bool IsRaylibBackend => App.Instance.Backend is RaylibBackend;
    private static bool IsSdl2Backend => App.Instance.Backend is SDL2Backend;

    public uint Id => IsRaylibBackend ? raylibTexture.Id : 0;

    public Texture(string filePath)
    {
        //string pngPath =
        //    Path.GetExtension(filePath).ToLower() == ".png" ?
        //    filePath :
        //    GetPngPath(filePath);
        //
        //if (IsRaylibBackend)
        //{
        //    raylibTexture = Raylib.LoadTexture(pngPath);
        //    Size = new(raylibTexture.Width, raylibTexture.Height);
        //}
        //else if (IsSdl2Backend)
        //{
        //    var renderer = ((SDL2DrawingBackend)((SDL2Backend)(App.Instance.Backend)).Drawing).Renderer;
        //
        //    sdl2Texture = SDL_image.IMG_LoadTexture(renderer, pngPath);
        //    _ = SDL.SDL_QueryTexture(sdl2Texture, out _, out _, out int width, out int height);
        //    Size = new(width, height);
        //}
        //
        //if (pngPath != filePath)
        //{
        //    File.Delete(pngPath);
        //}
    }

    public Texture()
    {
    }

    public static implicit operator Texture2D(Texture texture) => texture.raylibTexture;

    public static implicit operator IntPtr(Texture texture) => texture.sdl2Texture;

    private static string GetPngPath(string imagePath)
    {
        if (!Directory.Exists("Res/Cherris/Temporary"))
        {
            Directory.CreateDirectory("Res/Temporary");
        }

        string pngPath = $"Res/Cherris/Temporary/{Path.GetFileNameWithoutExtension(imagePath)}.png";

        if (!File.Exists(pngPath))
        {
            using var image = SixLabors.ImageSharp.Image.Load(imagePath);
            image.SaveAsPng(pngPath);
        }

        return pngPath;
    }
}