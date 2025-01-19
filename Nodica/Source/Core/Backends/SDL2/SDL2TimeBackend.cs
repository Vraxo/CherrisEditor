using SDL2;

namespace Cherris.Backends;

public sealed class SDL2TimeBackend : ITimeBackend
{
    float lastTicks = 0;

    public override float GetDeltaTime()
    {
        uint currentTicks = SDL.SDL_GetTicks();
        float deltaTime = (lastTicks > 0) ? (currentTicks - lastTicks) / 1000f : 0f;
        lastTicks = currentTicks;
        return deltaTime;
    }

    public override float GetElapsedTime()
    {
        return SDL.SDL_GetTicks() / 1000.0f;
    }
}