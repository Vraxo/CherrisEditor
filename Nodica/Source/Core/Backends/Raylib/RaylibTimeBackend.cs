using Raylib_cs;

namespace Cherris.Backends;

public sealed class RaylibTimeBackend : ITimeBackend
{
    public override float GetDeltaTime()
    {
        return Raylib.GetFrameTime();
    }

    public override float GetElapsedTime()
    {
        return (float)Raylib.GetTime();
    }
}