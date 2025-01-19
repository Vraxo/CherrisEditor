namespace Cherris;

public sealed class RenderManager
{
    private static RenderManager? _instance;
    public static RenderManager Instance => _instance ??= new();

    private readonly List<DrawCommand> drawCommands = [];

    private RenderManager() { }

    public void Process()
    {
        foreach (DrawCommand command in drawCommands.OrderBy(c => c.Layer))
        {
            command.Draw();
        }

        drawCommands.Clear();
    }

    public void Submit(DrawCommand drawCommand)
    {
        drawCommands.Add(drawCommand);
    }
}