using Spectre.Console;

namespace Cherris;

/// <summary>
/// Represents a basic node in a scene tree with support for children, activation, and destruction.
/// </summary>
public class Node
{
    /// <summary>Gets the root node of the application scene tree.</summary>
    public static Node RootNode => App.Instance.RootNode!;

    public string Name { get; set; } = "";

    [InspectorExclude]
    public Node? Parent { get; set; } = null;

    [InspectorExclude]
    //[SaveExclude]
    public List<Node> Children { get; set; } = [];

    [InspectorExclude]
    public bool Active { get; private set; } = true;

    /// <summary>Gets the absolute path to this node, starting with "/root/".</summary>
    [InspectorExclude]
    public string AbsolutePath
    {
        get
        {
            if (Parent is null)
            {
                // This is the root node
                return "/root/";
            }

            Stack<string> pathStack = new();
            Node? current = this;

            // Traverse the hierarchy upwards, collecting names
            while (current is not null && current.Parent is not null)
            {
                // Skip the root node's name (Parent will be null for root)
                pathStack.Push(current.Name);
                current = current.Parent;
            }

            // Build the absolute path
            return $"/root/{string.Join("/", pathStack)}";
        }
    }

    private bool started = false;

    public delegate void NodeChildAddedEventHandler(Node sender, Node child);
    public NodeChildAddedEventHandler? ChildAdded;

    /// <summary>Add the node's children to it before starting it.</summary>
    public virtual void Make() { }

    /// <summary>Initializes the node when added to the scene.</summary>
    public virtual void Start() { }

    /// <summary>Called once when the node becomes active.</summary>
    public virtual void Ready() { }

    /// <summary>Updates the node on each frame while active.</summary>
    public virtual void Update() { }

    public virtual void Update(float delta) { }

    /// <summary>Recursively destroys this node and its children, removing it from the parent's children.</summary>
    public virtual void Destroy()
    {
        List<Node> childrenToDestroy = new(Children);

        foreach (Node child in childrenToDestroy)
        {
            child.Destroy();
        }

        Parent?.Children.Remove(this);
    }

    /// <summary>Processes the node and its children, handling initialization and updating.</summary>
    public virtual void Process()
    {
        if (!Active)
        {
            return;
        }

        if (!started)
        {
            Ready();
            started = true;
        }

        Update();

        for (int i = 0; i < Children.Count; i++)
        {
            Children[i].Process();
        }
    }

    /// <summary>Prints the node and all its children in a tree structure.</summary>
    //public void PrintChildren(string indent = "")
    //{
    //    Console.WriteLine(indent + "-" + Name);
    //
    //    foreach (var child in Children)
    //    {
    //        child.PrintChildren(indent + "-");
    //    }
    //
    //    Console.WriteLine(AbsolutePath);
    //    
    //    foreach (var child in Children)
    //    {
    //        child.PrintChildren();
    //    }
    //}

    //public void PrintChildren()
    //{
    //    Console.OutputEncoding = System.Text.Encoding.UTF8;
    //
    //    Tree root = new($"🍎[green]{Name}[/]");
    //    AddChildrenToTree(this, root);
    //    AnsiConsole.Write(root);
    //}

    public void PrintChildren()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // Create root tree node with emoji based on root node type
        string rootEmoji = NodeEmoji.GetEmojiForNodeType(this);
        Tree root = new($"{rootEmoji}[green]{Name}[/]");

        // Add children nodes recursively, each with its own emoji
        AddChildrenToTree(this, root);

        // Print the tree structure using Spectre.Console
        AnsiConsole.Write(root);
    }

    private static void AddChildrenToTree(Node node, IHasTreeNodes parentNode)
    {
        foreach (var child in node.Children)
        {
            // Get emoji for each child node based on its type
            string childEmoji = NodeEmoji.GetEmojiForNodeType(child);

            // Add the child node to the tree with the emoji
            TreeNode childNode = parentNode.AddNode($"{childEmoji} [blue]{child.Name}[/]");

            // Recursively add children to this child node
            AddChildrenToTree(child, childNode);
        }
    }

    /// <summary>Activates the node and recursively activates all its children.</summary>
    public virtual void Activate()
    {
        Active = true;

        foreach (Node child in Children)
        {
            child.Activate();
        }
    }

    /// <summary>Deactivates the node and recursively deactivates all its children.</summary>
    public virtual void Deactivate()
    {
        Active = false;

        foreach (Node child in Children)
        {
            child.Deactivate();
        }
    }

    /// <summary>
    /// Returns the parent node cast to type <typeparamref name="T"/> if available, otherwise returns the current node.
    /// </summary>
    public T? GetParent<T>() where T : Node
    {
        if (Parent != null)
        {
            return (T)Parent;
        }

        return (T)this;
    }

    /// <summary>
    /// Retrieves a node of type <typeparamref name="T"/> from the scene tree based on a specified path.
    /// Throws an exception if the node is not found.
    /// </summary>
    /// <typeparam name="T">The expected type of the node to be returned.</typeparam>
    /// <param name="path">
    /// The path to the target node, which can be either absolute (starting with "/root") or relative.
    /// </param>
    public T GetNode<T>(string path) where T : Node
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }

        Node? currentNode;

        if (path.StartsWith("/root"))
        {
            path = path["/root".Length..];
            currentNode = App.Instance.RootNode;

            if (path.StartsWith('/'))
            {
                path = path.Substring(1);
            }

            if (!string.IsNullOrEmpty(path))
            {
                string[] nodeNames = path.Split('/');
                foreach (var name in nodeNames)
                {
                    currentNode = currentNode!.GetChild(name);

                    if (currentNode == null)
                    {
                        throw new InvalidOperationException($"Node '{name}' not found in the scene tree.");
                    }
                }
            }
        }
        else
        {
            currentNode = this;
            string[] nodeNames = path.Split('/');
            foreach (var name in nodeNames)
            {
                if (name == "")
                {
                    return currentNode as T ?? throw new InvalidOperationException("Current node is not of the expected type.");
                }

                currentNode = currentNode.GetChild(name);
                if (currentNode == null)
                {
                    throw new InvalidOperationException($"Node '{name}' not found in the scene tree.");
                }
            }
        }

        return currentNode as T ?? throw new InvalidOperationException("Node is not of the expected type.");
    }

    /// <summary>
    /// Retrieves a node of type <typeparamref name="T"/> from the scene tree based on a specified path.
    /// Returns null if the node is not found.
    /// </summary>
    /// <typeparam name="T">The expected type of the node to be returned.</typeparam>
    /// <param name="path">
    /// The path to the target node, which can be either absolute (starting with "/root") or relative.
    /// </param>
    public T? GetNodeOrNull<T>(string path) where T : Node
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        Node currentNode;

        if (path.StartsWith("/root"))
        {
            path = path.Substring("/root".Length);
            currentNode = App.Instance.RootNode;

            if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }

            if (!string.IsNullOrEmpty(path))
            {
                string[] nodeNames = path.Split('/');
                foreach (var name in nodeNames)
                {
                    currentNode = currentNode.GetChild(name);

                    if (currentNode == null)
                    {
                        return null;
                    }
                }
            }
        }
        else
        {
            currentNode = this;
            string[] nodeNames = path.Split('/');
            foreach (var name in nodeNames)
            {
                if (name == "")
                {
                    return currentNode as T;
                }

                currentNode = currentNode.GetChild(name);
                if (currentNode == null)
                {
                    return null;
                }
            }
        }

        return currentNode as T;
    }

    /// <summary>
    /// Returns a child node by name if it exists, cast to <typeparamref name="T"/>.
    /// </summary>
    public T? GetChild<T>(string name) where T : Node
    {
        foreach (Node child in Children)
        {
            if (child.Name == name)
            {
                return (T)child;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the first child node matching type <typeparamref name="T"/>, if it exists.
    /// </summary>
    public T? GetChild<T>() where T : Node
    {
        foreach (Node child in Children)
        {
            if (child.GetType() == typeof(T))
            {
                return (T)child;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns a child node by name. Throws an exception if the child is not found.
    /// </summary>
    /// <param name="name">The name of the child node to find.</param>
    /// <returns>The child node with the specified name.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no child node with the specified name exists.</exception>
    public Node GetChild(string name)
    {
        foreach (Node child in Children)
        {
            if (child.Name == name)
            {
                return child;
            }
        }

        App.Instance.RootNode.PrintChildren();

        throw new InvalidOperationException($"Child node with name '{name}' not found.");
    }

    /// <summary>
    /// Returns a child node by name if it exists, or null if it does not.
    /// </summary>
    /// <param name="name">The name of the child node to find.</param>
    /// <returns>The child node with the specified name, or null if no such child exists.</returns>
    public Node? GetChildOrNull(string name)
    {
        foreach (Node child in Children)
        {
            if (child.Name == name)
            {
                return child;
            }
        }

        return null;
    }

    /// <summary>
    /// Adds a child node with a specified name and optionally starts it.
    /// </summary>
    public Node AddChild(Node node, string name, bool start = true)
    {
        node.Name = name;
        node.Parent = this;

        node.Make();

        if (start)
        {
            node.Start();
        }

        Children.Add(node);
        ChildAdded?.Invoke(this, node);

        return node;
    }

    /// <summary>
    /// Adds a child node using its type name and optionally starts it.
    /// </summary>
    public Node AddChild(Node node, bool start = true)
    {
        node.Name = node.GetType().Name;
        node.Parent = this;

        node.Make();

        if (start)
        {
            node.Start();
        }

        Children.Add(node);

        return node;
    }

    /// <summary>
    /// Replaces the current scene with a new root node.
    /// </summary>
    public static void ChangeScene(Node node)
    {
        App.Instance.RootNode.Destroy();
        App.Instance.RootNode = node;

        node.Name = node.GetType().Name;
    }
}