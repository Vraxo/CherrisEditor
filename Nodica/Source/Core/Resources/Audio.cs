using Cherris.Backends;
using Raylib_cs;
using SDL2;

namespace Cherris;

public class Audio
{
    public float Length { get; private set; } = 0.1f;
    private Music raylibAudio;
    private readonly IntPtr sdl2Audio;

    public Audio(string filePath)
    {
        if (App.Instance.Backend is RaylibBackend)
        {
            raylibAudio = Raylib.LoadMusicStream(filePath);
            Length = Raylib.GetMusicTimeLength(raylibAudio);
        }
        else if (App.Instance.Backend is SDL2Backend)
        {
            sdl2Audio = SDL_mixer.Mix_LoadMUS(filePath);
        }
    }

    public static implicit operator Music(Audio audio)
    {
        if (App.Instance.Backend is RaylibBackend)
        {
            return audio.raylibAudio;
        }

        return default;
    }

    public static implicit operator IntPtr(Audio audio)
    {
        if (App.Instance.Backend is SDL2Backend)
        {
            return audio.sdl2Audio;
        }

        return IntPtr.Zero;
    }
}