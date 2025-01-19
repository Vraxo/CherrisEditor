namespace Cherris;

public class Timer : Node
{
    public float TimePassed { get; private set; } = 0;
    public bool AutoStart { get; set; } = false;
    public bool Loop { get; set; } = false;

    public float WaitTime { get; set; } = 1.0f;
    private bool fired = false;

    public delegate void TimerEventHandler(Timer sender);
    public event TimerEventHandler? TimedOut;

    public override void Ready()
    {
        base.Ready();

        if (AutoStart)
        {
            Fire();
        }
    }

    public override void Update()
    {
        base.Update();

        if (fired)
        {
            TimePassed += TimeManager.Delta;

            if (TimePassed >= WaitTime)
            {
                fired = false;
                TimePassed = 0;

                TimedOut?.Invoke(this);

                if (Loop)
                {
                    Fire();
                }
            }
        }
    }

    public void Fire()
    {
        fired = true;
        TimePassed = 0;
    }

    public void Stop()
    {
        fired = false;
    }

    public void Reset()
    {
        TimePassed = 0;
        fired = false;
    }
}