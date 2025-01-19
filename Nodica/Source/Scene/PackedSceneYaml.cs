using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Cherris;

public sealed class PackedSceneYaml(string path)
{
    private readonly string path = path;
    private readonly Dictionary<string, Node> namedNodes = [];
    private static readonly IDeserializer deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    public T Instantiate<T>(bool isRootNode = false) where T : Node
    {
        string yamlString = File.ReadAllText(path);
        var nodes = deserializer.Deserialize<List<Dictionary<string, object>>>(yamlString);
        Node rootNode = ParseNodeList(nodes, null);

        if (rootNode != null && nodes.Count > 0)
        {
            rootNode.Name = (string)nodes[0]["name"];
        }

        return (T)rootNode!;
    }

    private Node ParseNodeList(List<Dictionary<string, object>> nodes, Node? parent)
    {
        Node? lastNode = null;

        foreach (var element in nodes)
        {
            string type = (string)element["type"];
            string name = (string)element["name"];
            string? parentName = element.ContainsKey("parent") ? (string?)element["parent"] : null;

            Type nodeType = PackedSceneUtils.ResolveType(type);
            Node node = (Node)Activator.CreateInstance(nodeType)!;

            if (element.TryGetValue("path", out object? value))
            {
                string scenePath = (string)value;
                PackedSceneYaml nestedScene = new(scenePath);
                var nestedRootNode = nestedScene.Instantiate<Node>();
                node = nestedRootNode;
            }

            PackedSceneUtils.SetProperties(node, element);

            if (parent == null && parentName == null)
            {
                lastNode = node;
            }
            else if (parent != null)
            {
                parent.AddChild(node, name);
            }
            else if (parentName != null && namedNodes.TryGetValue(parentName, out Node? _value))
            {
                _value.AddChild(node, name);
            }

            namedNodes[name] = node;
        }

        return lastNode!;
    }
}