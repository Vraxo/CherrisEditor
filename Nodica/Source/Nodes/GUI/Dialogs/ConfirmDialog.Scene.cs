namespace Cherris;

public partial class ConfirmDialog : Dialog
{
    public override void Make()
    {
        base.Make();

        AddChild(new Button
        {
            Position = new(0, 50),
            Layer = ClickableLayer.DialogButtons,
            Text = "Confirm",
        }, "ConfirmButton");
    }
}