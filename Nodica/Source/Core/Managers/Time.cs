using Nodica.Backends;

namespace Nodica;

public static class Time
{
    private static Backend Backend => App.Instance.Backend;

    public static float Delta;
    public static float Elapsed => Backend.Time.GetElapsedTime();

    public static void Process()
    {
        Delta = Backend.Time.GetDeltaTime();
    }
}