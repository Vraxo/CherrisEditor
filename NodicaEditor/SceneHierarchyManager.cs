using System.Windows.Controls;
using IniParser;
using IniParser.Model;
using Nodica;

namespace NodicaEditor;

public class SceneHierarchyManager
{
    private Dictionary<string, Node> _nodeMap;
    private TreeView _sceneHierarchyTreeView;
    private PropertyInspector _propertyInspector;
    private static readonly FileIniDataParser _iniParser = new();
    public Node? CurrentNode { get; private set; }

    public SceneHierarchyManager(TreeView treeView, PropertyInspector propertyInspector)
    {
        _sceneHierarchyTreeView = treeView;
        _propertyInspector = propertyInspector;
        _nodeMap = new Dictionary<string, Node>();
        _sceneHierarchyTreeView.SelectedItemChanged += SceneHierarchyTreeView_SelectedItemChanged;
    }

    public void LoadScene(string filePath)
    {
        _nodeMap = ParseIniFile(filePath);
        BuildTreeView();
    }

    public void BuildTreeView()
    {
        _sceneHierarchyTreeView.Items.Clear();
        foreach (var node in _nodeMap.Values)
        {
            if (node.Parent == null)
            {
                TreeViewItem treeItem = CreateTreeItem(node);
                _sceneHierarchyTreeView.Items.Add(treeItem);
            }
        }
    }

    private Dictionary<string, Node> ParseIniFile(string filePath)
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

    private TreeViewItem CreateTreeItem(Node node)
    {
        var treeItem = new TreeViewItem { Header = node.Name, Tag = node };
        foreach (var child in node.Children)
        {
            treeItem.Items.Add(CreateTreeItem(child));
        }
        return treeItem;
    }

    private void SceneHierarchyTreeView_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
    {
        if (_sceneHierarchyTreeView.SelectedItem is TreeViewItem selectedItem && selectedItem.Tag is Node selectedNode)
        {
            CurrentNode = selectedNode;
            _propertyInspector.DisplayNodeProperties(selectedNode);
        }
    }
}