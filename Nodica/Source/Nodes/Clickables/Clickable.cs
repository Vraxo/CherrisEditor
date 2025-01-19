namespace Cherris;

public abstract class Clickable : Node2D
{
    public bool OnTopLeft = false;
    public bool OnTopRight = false;

    public override void Start()
    {
        ClickManager.Instance.Register(this);
    }

    public override void Destroy()
    {
        ClickManager.Instance.Unregister(this);
        base.Destroy();
    }

    public abstract bool IsMouseOver();
}