using YamlDotNet.Serialization;

public class LogSettings
{
    public static LogSettings Instance => instance ??= new();
    private static LogSettings? instance;

    public Dictionary<string, bool> Settings { get; private set; }

    private LogSettings()
    {
        string yamlFilePath = "Res/Nodica/LogSettings.yaml";
        string yamlString = File.ReadAllText(yamlFilePath);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
            .Build();

        Settings = deserializer.Deserialize<Dictionary<string, bool>>(yamlString);
    }

    public bool GetLogCondition(string key)
    {
        return Settings.ContainsKey(key) && Settings[key];
    }
}