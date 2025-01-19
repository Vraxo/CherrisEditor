using IniParser;
using IniParser.Model;

namespace Cherris;

public sealed class PackedSceneIni(string path)
{
    private readonly string path = path;
    private readonly Dictionary<string, Node> namedNodes = new();
    private static readonly FileIniDataParser iniParser = new();

    public T Instantiate<T>(bool isRootNode = false) where T : Node
    {
        IniData iniData = iniParser.ReadFile(path);
        List<Dictionary<string, object>> nodeDataList = ParseIniData(iniData);
        List<Node> createdNodes = ParseNodeList(nodeDataList);
        Node? rootNode = createdNodes.FirstOrDefault(); // Assume the first node is the root

        return (T)rootNode!;
    }

    private List<Dictionary<string, object>> ParseIniData(IniData iniData)
    {
        List<Dictionary<string, object>> nodes = new();
        foreach (SectionData section in iniData.Sections)
        {
            Dictionary<string, object> nodeData = new();
            nodeData["name"] = section.SectionName;  // Use section name as node name
            foreach (KeyData keyData in section.Keys)
            {
                string key = keyData.KeyName;
                string value = keyData.Value;
                nodeData[key] = value;
            }
            nodes.Add(nodeData);
        }
        return nodes;
    }

    private List<Node> ParseNodeList(List<Dictionary<string, object>> nodes)
    {
        List<Node> createdNodes = new();

        // First pass: Create all nodes and populate namedNodes
        foreach (var element in nodes)
        {
            string type = (string)element["type"];
            string name = (string)element["name"];

            Type nodeType = PackedSceneUtils.ResolveType(type);
            Node node = (Node)Activator.CreateInstance(nodeType)!;

            if (element.TryGetValue("path", out object? value))
            {
                string scenePath = (string)value;
                PackedSceneIni nestedScene = new(scenePath);
                var nestedRootNode = nestedScene.Instantiate<Node>();
                node = nestedRootNode;
            }

            PackedSceneUtils.SetProperties(node, element);
            namedNodes[name] = node;
            createdNodes.Add(node);
        }

        // Second pass: Establish parent-child relationships
        foreach (var element in nodes)
        {
            string name = (string)element["name"];
            string? parentName = element.ContainsKey("parent") ? (string?)element["parent"] : null;

            if (parentName != null && namedNodes.TryGetValue(parentName, out Node? parent))
            {
                namedNodes[name].Parent = parent; // Assuming your Node class has a Parent property
                parent.AddChild(namedNodes[name], name);
            }
        }

        return createdNodes;
    }
}