namespace Cherris;

public partial class ConfirmDialog : Dialog
{
    public override void Start()
    {
        GetNode<Button>("ConfirmButton").LeftClicked += OnConfirmButtonLeftClicked;
        base.Start();
    }

    protected virtual void OnConfirmButtonLeftClicked(Button sender)
    {

    }
}