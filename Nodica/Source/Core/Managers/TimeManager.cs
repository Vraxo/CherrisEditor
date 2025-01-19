using Cherris.Backends;

namespace Cherris;

public static class TimeManager
{
    private static Backend Backend => App.Instance.Backend;

    public static float Delta;
    public static float Elapsed => Backend.Time.GetElapsedTime();

    public static void Process()
    {
        Delta = Backend.Time.GetDeltaTime();
    }
}