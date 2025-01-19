using System.Reflection;

namespace Cherris;

public class PackedScene
{
    private static readonly string extension = ".scene";

    private readonly string path;

    public PackedScene(string path)
    {
        this.path = path;
    }

    public T Instantiate<T>(bool isRootNode = false) where T : new()
    {
        T instance = new();
        string[] fileLines = File.ReadAllLines(path);
        object? currentNode = null; // Tracks the current node being processed
        Node? currentReferencedRoot = null; // Tracks the root node of a referenced scene
        bool firstNode = true;

        Dictionary<string, Node> namedNodes = new();
        Dictionary<Node, List<string>> nestedSceneProperties = new();

        if (isRootNode)
        {
            App.Instance.RootNode = instance as Node;
        }

        foreach (string line in fileLines)
        {
            string trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#")) continue;

            if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
            {
                string content = trimmedLine[1..^1].Trim();
                string[] parts = content.Split(new[] { ' ' }, 4);

                string typeName = parts[0];
                string nodeName = ExtractQuotedString(parts[1]);
                string parentName = parts.Length >= 3 ? ExtractQuotedString(parts[2]) : null;

                if (parts.Length == 4)
                {
                    string scenePath = EnsureExtension(ExtractQuotedString(parts[3]));
                    PackedScene referencedScene = new(scenePath);
                    Type rootNodeType = ResolveType(typeName);
                    currentReferencedRoot = referencedScene.Instantiate(rootNodeType) as Node;
                    currentReferencedRoot.Name = nodeName;

                    if (!nestedSceneProperties.ContainsKey(currentReferencedRoot))
                    {
                        nestedSceneProperties[currentReferencedRoot] = new List<string>();
                    }

                    if (parentName == null && firstNode)
                    {
                        instance = (T)(object)currentReferencedRoot;
                        namedNodes[nodeName] = currentReferencedRoot;
                        firstNode = false;
                    }
                    else if (namedNodes.TryGetValue(parentName, out Node parentNode))
                    {
                        parentNode.AddChild(currentReferencedRoot, nodeName, false);
                        namedNodes[nodeName] = currentReferencedRoot;
                    }
                    else
                    {
                        throw new Exception($"Parent node '{parentName}' not found for SceneReference.");
                    }

                    currentNode = currentReferencedRoot; // Switch to the referenced root node
                }
                else
                {
                    Type type = ResolveType(typeName);
                    currentNode = Activator.CreateInstance(type);

                    if (firstNode)
                    {
                        (currentNode as Node).Name = nodeName;
                        instance = (T)currentNode;
                        namedNodes[nodeName] = (Node)currentNode;
                        firstNode = false;
                    }
                    else
                    {
                        if (parentName == null) throw new Exception($"Node '{nodeName}' must specify a parent.");
                        if (namedNodes.TryGetValue(parentName, out Node parentNode))
                        {
                            parentNode.AddChild(currentNode as Node, nodeName, false);
                        }
                        else
                        {
                            throw new Exception($"Parent node '{parentName}' could not be found for node '{nodeName}'.");
                        }
                    }
                    namedNodes[nodeName] = (Node)currentNode;
                }
            }
            else if (trimmedLine.Contains(" = "))
            {
                int equalsIndex = trimmedLine.IndexOf(" = ");
                string fieldName = trimmedLine.Substring(0, equalsIndex).Trim();
                string value = trimmedLine.Substring(equalsIndex + 3).Trim();

                // Check if we're processing a nested scene
                if (currentReferencedRoot != null)
                {
                    SetValue(currentReferencedRoot, fieldName, value); // Apply to the referenced root node
                }
                else if (currentNode != null)
                {
                    SetValue(currentNode, fieldName, value); // Apply to the current regular node
                }
                else
                {
                    throw new Exception($"No valid node to assign property '{fieldName}'.");
                }
            }
        }

        if (isRootNode)
        {
            App.Instance.SetRootNode(instance as Node, true);
            (instance as Node).Start();
        }

        foreach (var namedNode in namedNodes.Values)
        {
            if (namedNode is Node childNode && childNode != instance as Node)
            {
                childNode.Start();
            }
        }

        return instance;
    }

    private object? Instantiate(Type type, bool isRootNode = false)
    {
        MethodInfo method = typeof(PackedScene).GetMethod(nameof(Instantiate))!.MakeGenericMethod(type);
        return method.Invoke(this, [isRootNode]);
    }

    private static string ExtractQuotedString(string str)
    {
        return str.Length >= 2 && str.StartsWith("\"") && str.EndsWith("\"") ? str[1..^1] : str;
    }

    private static string EnsureExtension(string path)
    {
        return path.EndsWith(extension) ? path : path + extension;
    }

    private static Type ResolveType(string typeName)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly assembly in assemblies)
        {
            Type? type = assembly.GetType(typeName, false, true);

            if (type is not null)
            {
                return type;
            }

            string defaultNamespace = assembly.GetName().Name!;
            string namespacedTypeName = defaultNamespace + "." + typeName;
            type = assembly.GetType(namespacedTypeName, false, true);

            if (type is not null)
            {
                return type;
            }
        }

        throw new Exception($"Type '{typeName}' not found.");
    }

    private static void SetValue(object obj, string name, object value)
    {
        string[] pathSegments = name.Split('/');
        Type type = obj.GetType();
        PropertyInfo? propertyInfo = null;

        for (int i = 0; i < pathSegments.Length; i++)
        {
            string segment = pathSegments[i];
            propertyInfo = type.GetProperty(segment, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (propertyInfo == null)
            {
                throw new Exception($"Property '{segment}' not found on type '{type.Name}'.");
            }
            if (i < pathSegments.Length - 1)
            {
                obj = propertyInfo.GetValue(obj)!;
                type = obj.GetType();
            }
        }

        if (propertyInfo != null && propertyInfo.CanWrite)
        {
            string stringValue = value.ToString()!;
            object convertedValue = ConvertValue(propertyInfo.PropertyType, stringValue);
            propertyInfo.SetValue(obj, convertedValue);
        }
    }

    private static object ConvertValue(Type targetType, object value)
    {
        string stringValue = value.ToString()!;

        return targetType switch
        {
            Type t when t == typeof(Vector2) => ParseVector2(stringValue),
            Type t when t == typeof(Color) => ParseColor(stringValue),
            Type t when t == typeof(Texture) => ParseTexture(stringValue),
            Type t when t == typeof(Audio) => ParseAudio(stringValue),
            Type t when t.IsEnum => Enum.Parse(targetType, stringValue),
            Type t when t == typeof(int) => int.Parse(stringValue),
            Type t when t == typeof(uint) => uint.Parse(stringValue),
            Type t when t == typeof(float) => float.Parse(stringValue),
            Type t when t == typeof(double) => double.Parse(stringValue),
            Type t when t == typeof(bool) => bool.Parse(stringValue),
            Type t when t == typeof(string) => ExtractQuotedString(stringValue),
            Type t when t == typeof(ButtonThemePack) => LoadButtonThemeFile(stringValue),
            _ => value
        };
    }

    private static Audio ParseAudio(string value)
    {
        string stringValue = value.Trim();

        if (stringValue.StartsWith("Audio(") && stringValue.EndsWith(")"))
        {
            string audioPath = ExtractQuotedString(stringValue[6..^1]);
            return new(audioPath);
        }
        else
        {
            throw new Exception($"Invalid Audio format, expected format: Audio(path)");
        }
    }

    private static ButtonThemePack LoadButtonThemeFile(string value)
    {
        string path = ExtractQuotedString(value);
        return PropertyLoader.Load<ButtonThemePack>(path);
    }

    private static Vector2 ParseVector2(string value)
    {
        string stringValue = value.Trim();

        if (stringValue.StartsWith("Vector2(") && stringValue.EndsWith(")"))
        {
            string vectorValues = stringValue.Substring(8, stringValue.Length - 9);
            string[] tokens = vectorValues.Split(',');

            if (tokens.Length == 2)
            {
                float x = float.Parse(tokens[0].Trim());
                float y = float.Parse(tokens[1].Trim());

                return new(x, y);
            }
            else
            {
                throw new Exception("Vector2 should contain exactly two numeric values.");
            }
        }
        else
        {
            throw new Exception($"Invalid Vector2 format, expected format: Vector2(x, y)");
        }
    }

    private static Color ParseColor(string value)
    {
        string stringValue = value.Trim();

        if (stringValue.StartsWith("Color(") && stringValue.EndsWith(")"))
        {
            string colorValues = stringValue.Substring(6, stringValue.Length - 7);
            string[] tokens = colorValues.Split(',');

            if (tokens.Length == 4)
            {
                byte r = byte.Parse(tokens[0].Trim());
                byte g = byte.Parse(tokens[1].Trim());
                byte b = byte.Parse(tokens[2].Trim());
                byte a = byte.Parse(tokens[3].Trim());

                return new Color(r, g, b, a);
            }
            else
            {
                throw new Exception("Color should contain exactly four numeric values (RGBA).");
            }
        }
        else
        {
            throw new Exception($"Invalid Color format, expected format: Color(r, g, b, a)");
        }
    }

    private static Texture ParseTexture(string value)
    {
        string stringValue = value.Trim();

        if (stringValue.StartsWith("Texture(") && stringValue.EndsWith(")"))
        {
            string texturePath = ExtractQuotedString(stringValue[8..^1]);
            return TextureManager.Instance.Get(texturePath);
        }
        else
        {
            throw new Exception($"Invalid Texture format, expected format: Texture(path)");
        }
    }
}