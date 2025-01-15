using Nodica;
using NodicaEditor;
using System.Numerics;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using TextBlock = System.Windows.Controls.TextBlock;
using VerticalAlignment = System.Windows.VerticalAlignment;
using Button = System.Windows.Controls.Button;

public partial class PropertyInspector : Window
{
    private readonly StackPanel panel;
    private readonly Dictionary<Node, Dictionary<string, object?>> nodePropertyValues = new();
    private static readonly SolidColorBrush BackgroundColor = new(new() { R = 16, G = 16, B = 16, A = 255 });
    private static readonly SolidColorBrush ForegroundColor = new(Colors.LightGray);
    private static readonly SolidColorBrush SeparatorColor = new(Colors.Gray);
    private readonly Dictionary<string, Expander> _expanderMap = new();

    public PropertyInspector(StackPanel inspectorPanel)
    {
        panel = inspectorPanel;
        _expanderMap.Clear();
    }

    public Dictionary<string, object?> GetPropertyValues(Node node)
    {
        if (nodePropertyValues.TryGetValue(node, out var values))
        {
            return values;
        }
        return new Dictionary<string, object?>();
    }

    public void DisplayNodeProperties(Node node)
    {
        panel.Children.Clear();
        _expanderMap.Clear();

        if (!nodePropertyValues.ContainsKey(node))
        {
            nodePropertyValues[node] = new Dictionary<string, object?>();
        }

        List<Type> hierarchy = GetInheritanceHierarchy(node.GetType());

        foreach (Type currentType in hierarchy)
        {
            Expander expander = AddInheritanceSeparator(currentType, currentType.Name, null);
            _expanderMap[expander.Header.ToString()] = expander;

            PropertyInfo[] properties = currentType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (property.IsDefined(typeof(InspectorExcludeAttribute), false))
                    continue;

                if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    var nestedObject = property.GetValue(node);
                    if (nestedObject != null)
                    {
                        string parentPath = property.Name;
                        Expander nestedExpander = AddInheritanceSeparator(nestedObject.GetType(), parentPath, expander);
                        _expanderMap[nestedExpander.Header.ToString()] = nestedExpander;
                        DisplayNestedProperties(nestedObject, node, parentPath, nestedExpander);
                    }
                }
                else
                {
                    string propertyName = property.Name;

                    if (!nodePropertyValues[node].ContainsKey(propertyName))
                    {
                        nodePropertyValues[node][propertyName] = property.GetValue(node);
                    }

                    FrameworkElement propertyControl = PropertyControlFactory.CreateControl(node, property, propertyName, nodePropertyValues[node]);
                    if (propertyControl != null)
                    {
                        AddPropertyControlToExpander(node, property, propertyControl, expander, propertyName, nodePropertyValues[node]);
                    }
                }
            }
        }
    }

    private List<Type> GetInheritanceHierarchy(Type type)
    {
        List<Type> hierarchy = new List<Type>();
        while (type != null && type != typeof(object))
        {
            hierarchy.Add(type);
            type = type.BaseType;
        }
        hierarchy.Reverse();
        return hierarchy;
    }

    private void DisplayNestedProperties(object nestedObject, Node node, string parentPath, Expander parentExpander)
    {
        if (nestedObject == null)
            return;

        Type nestedType = nestedObject.GetType();
        PropertyInfo[] properties = nestedType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (property.IsDefined(typeof(InspectorExcludeAttribute), false))
                continue;

            string fullPath = $"{parentPath}/{property.Name}";

            if (!nodePropertyValues[node].ContainsKey(fullPath))
            {
                nodePropertyValues[node][fullPath] = property.GetValue(nestedObject);
            }

            if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                var nestedValue = property.GetValue(nestedObject);
                if (nestedValue != null)
                {
                    Expander nestedExpander = AddInheritanceSeparator(nestedValue.GetType(), fullPath, parentExpander);
                    _expanderMap[nestedExpander.Header.ToString()] = nestedExpander;
                    DisplayNestedProperties(nestedValue, node, fullPath, nestedExpander);
                }
            }
            else
            {
                FrameworkElement propertyControl = PropertyControlFactory.CreateControl(node, property, fullPath, nodePropertyValues[node]);
                if (propertyControl != null && parentExpander != null)
                {
                    AddPropertyControlToExpander(node, property, propertyControl, parentExpander, fullPath, nodePropertyValues[node]);
                }
            }
        }
    }

    private Expander AddInheritanceSeparator(Type type, string path, Expander parentExpander = null)
    {
        string expanderKey = path.Split('/').Last();
        if (_expanderMap.ContainsKey(expanderKey))
            return _expanderMap[expanderKey];

        var expander = new Expander
        {
            Header = expanderKey,
            IsExpanded = true,
            BorderBrush = SeparatorColor,
            BorderThickness = new Thickness(0, 1, 0, 1),
            Margin = new Thickness(parentExpander != null ? 20 : 0, 5, 0, 5),
            Foreground = SeparatorColor
        };

        var contentPanel = new StackPanel();
        expander.Content = contentPanel;

        if (parentExpander != null && parentExpander.Content is StackPanel parentContentPanel)
        {
            parentContentPanel.Children.Add(expander);
        }
        else
        {
            panel.Children.Add(expander);
        }

        _expanderMap[expanderKey] = expander;
        return expander;
    }

    private void AddPropertyControlToExpander(Node node, PropertyInfo property, FrameworkElement propertyControl, Expander expander, string fullPath, Dictionary<string, object?> nodePropertyValues)
    {
        var controlAndResetPanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        // Create and attach the reset button directly here
        Button resetButton = PropertyControlFactory.CreateResetButton(node, property, fullPath, nodePropertyValues, propertyControl);

        controlAndResetPanel.Children.Add(propertyControl);
        controlAndResetPanel.Children.Add(resetButton);

        TextBlock label = new()
        {
            Text = property.Name,
            MaxWidth = 250,
            TextWrapping = TextWrapping.Wrap,
            Foreground = ForegroundColor,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 5, 0),
            HorizontalAlignment = HorizontalAlignment.Left
        };

        Grid propertyGrid = new()
        {
            Margin = new Thickness(5),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnDefinitions =
            {
                new() { Width = GridLength.Auto },
                new() { Width = new(1, GridUnitType.Star) }
            }
        };

        Grid.SetColumn(label, 0);
        propertyGrid.Children.Add(label);
        Grid.SetColumn(controlAndResetPanel, 1);
        propertyGrid.Children.Add(controlAndResetPanel);

        if (expander != null)
        {
            if (expander.Content is StackPanel contentPanel)
            {
                contentPanel.Children.Add(propertyGrid);
            }
        }
        else
        {
            panel.Children.Add(propertyGrid);
        }
    }

    public static object? GetDefaultValue(PropertyInfo property, Node node, string fullPath = "")
    {
        try
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                return property.GetValue(Activator.CreateInstance(node.GetType()));
            }
            else
            {
                string[] pathParts = fullPath.Split('/');
                object currentObject = node;
                PropertyInfo? currentProperty = null;

                foreach (var part in pathParts)
                {
                    currentProperty = currentObject.GetType().GetProperty(part);

                    if (currentProperty is null)
                    {
                        throw new InvalidOperationException($"Property '{part}' not found in path '{fullPath}'.");
                    }

                    currentObject = currentProperty.GetValue(currentObject);
                    if (currentObject == null)
                    {
                        return null;
                    }
                }

                return currentProperty.GetValue(Activator.CreateInstance(currentObject.GetType()));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in GetDefaultValue: {ex.Message}");
            return property.PropertyType switch
            {
                Type boolType when boolType == typeof(bool) => false,
                Type intType when intType == typeof(int) => 0,
                Type floatType when floatType == typeof(float) => 0f,
                Type vector2Type when vector2Type == typeof(Vector2) => Vector2.Zero,
                Type colorType when colorType == typeof(Color) => default(Color),
                _ => property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null
            };
        }
    }
}