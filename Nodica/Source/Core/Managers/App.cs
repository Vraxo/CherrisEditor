using System.Reflection;
using YamlDotNet.Serialization;
using Nodica.Backends;

namespace Nodica;

public sealed class App
{
    private static App? _instance;
    public static App Instance => _instance ??= new();

    public readonly Backend Backend;
    public Node? RootNode;

    private readonly Configuration config;

    private App()
    {
        string configFilePath = "Res/Nodica/Config.yaml";
        config = LoadConfig(configFilePath);

        if (config?.Backend == "Raylib")
        {
            Backend = new RaylibBackend();
            Log.Info("[APP] Using Raylib as backend.");
        }
        else if (config?.Backend == "SDL2")
        {
            Backend = new SDL2Backend();
            Log.Info("[APP] Using SDL2 as backend.");
        }
        else
        {
            throw new InvalidOperationException("Invalid backend specified in the configuration.");
        }
    }

    public void Run()
    {
        Start();
        Update();
    }

    public void SetRootNode(Node node, bool packedScene = false)
    {
        RootNode = node;

        if (!packedScene)
        {
            RootNode.Make();
        }
    }

    private void Start()
    {
        SetCurrentDirectory();

        if (config is null)
        {
            return;
        }

        WindowManager.OriginalSize = new(config.Width, config.Height);

        Backend.Window.SetWindowFlags(config.ResizableWindow, config.AntiAliasing);

        Backend.Window.InitializeWindow(
            config.Width,
            config.Height,
            config.MinWidth,
            config.MinHeight,
            config.MaxWidth,
            config.MaxHeight,
            config.Title,
            "Res/Icon/Icon.png");

        Backend.Window.SetWindowMinSize(config.MinWidth, config.MinHeight);

        SetRootNodeFromConfig(config.MainScenePath);
    }

    private void SetRootNodeFromConfig(string scenePath)
    {
        PackedSceneIni packedScene = new(scenePath);
        RootNode = packedScene.Instantiate<Node>(true);

        Console.WriteLine(RootNode is null);
    }

    private void Update()
    {
        while (!Backend.Window.WindowShouldClose())
        {
            Backend.Drawing.BeginDrawing();
            Backend.Drawing.ClearBackground(DefaultTheme.Background);
            ProcessManagers();
            RootNode?.Process();
            Backend.Drawing.EndDrawing();

            PrintTree();
        }
    }

    private static void ProcessManagers()
    {
        Time.Process();
        ClickManager.Instance.Process();
        CollisionManager.Instance.Process();
        RenderManager.Instance.Process();
    }

    private void PrintTree()
    {
        if (Backend.Input.IsKeyPressed(KeyCode.Enter))
        {
            Console.Clear();
            RootNode?.PrintChildren();
        }
    }

    private static void SetCurrentDirectory()
    {
        string? assemblyLocation = Assembly.GetEntryAssembly()?.Location;
        string? directoryName = Path.GetDirectoryName(assemblyLocation);

        if (directoryName is null)
        {
            return;
        }

        Environment.CurrentDirectory = directoryName;
    }

    private static Configuration LoadConfig(string filePath)
    {
        var deserializer = new DeserializerBuilder().Build();
        return deserializer.Deserialize<Configuration>(System.IO.File.ReadAllText(filePath));
    }
}
