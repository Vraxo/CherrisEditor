
using IniParser.Model;
using IniParser;
using Nodica;

namespace NodicaEditor;

public class IniFileParser
{
    private readonly FileIniDataParser _iniParser;

    public IniFileParser()
    {
        _iniParser = new FileIniDataParser();
    }

    public Dictionary<string, Node> ParseFile(string filePath)
    {
        IniData iniData = _iniParser.ReadFile(filePath);
        List<Dictionary<string, object>> nodeDataList = ParseIniData(iniData);
        List<Node> createdNodes = ParseNodeList(nodeDataList);
        return createdNodes.ToDictionary(node => node.Name, node => node);
    }

    private List<Dictionary<string, object>> ParseIniData(IniData iniData)
    {
        List<Dictionary<string, object>> nodes = new();
        foreach (SectionData section in iniData.Sections)
        {
            Dictionary<string, object> nodeData = new();
            nodeData["name"] = section.SectionName;
            foreach (KeyData keyData in section.Keys)
            {
                nodeData[keyData.KeyName] = keyData.Value;
            }
            nodes.Add(nodeData);
        }
        return nodes;
    }

    private List<Node> ParseNodeList(List<Dictionary<string, object>> nodes)
    {
        List<Node> createdNodes = new();
        Dictionary<string, Node> namedNodes = new();

        foreach (var element in nodes)
        {
            string type = (string)element["type"];
            string name = (string)element["name"];

            Type nodeType = PackedSceneUtils.ResolveType(type);
            Node node = (Node)Activator.CreateInstance(nodeType)!;
            node.Name = name;
            PackedSceneUtils.SetProperties(node, element);
            namedNodes[name] = node;
            createdNodes.Add(node);
        }

        foreach (var element in nodes)
        {
            string name = (string)element["name"];
            string? parentName = element.ContainsKey("parent") ? (string?)element["parent"] : null;

            if (parentName != null && namedNodes.TryGetValue(parentName, out Node? parent))
            {
                namedNodes[name].Parent = parent;
                parent.AddChild(namedNodes[name], name);
            }
        }

        return createdNodes;
    }
}
