using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Cherris;
using Button = System.Windows.Controls.Button;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using TextBlock = System.Windows.Controls.TextBlock;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace CherrisEditor;

public partial class Inspector : UserControl
{
    private readonly StackPanel panel;
    private readonly Dictionary<Node, Dictionary<string, object?>> nodePropertyValues = [];
    private static readonly SolidColorBrush ForegroundColor = new(Colors.LightGray);
    private static readonly SolidColorBrush SeparatorColor = new(Colors.Gray);
    private readonly Dictionary<string, Expander> expanderMap = [];

    public Inspector()
    {
        InitializeComponent();
        panel = InspectorPanel;
        expanderMap.Clear();
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
        expanderMap.Clear();

        if (!nodePropertyValues.ContainsKey(node))
        {
            nodePropertyValues[node] = new Dictionary<string, object?>();
        }

        List<Type> hierarchy = GetInheritanceHierarchy(node.GetType());

        foreach (Type currentType in hierarchy)
        {
            Expander expander = AddInheritanceSeparator(currentType, currentType.Name, null);
            expanderMap[expander.Header.ToString()] = expander;

            PropertyInfo[] properties = currentType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                // Skip properties without setters
                if (!property.CanWrite)
                {
                    continue;
                }

                if (property.IsDefined(typeof(InspectorExcludeAttribute), false))
                {
                    continue;
                }

                // Handle resource types (like Texture) directly
                if (property.PropertyType == typeof(Texture))
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
                // Handle nested objects
                else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    var nestedObject = property.GetValue(node);
                    if (nestedObject != null)
                    {
                        string parentPath = property.Name;
                        Expander nestedExpander = AddInheritanceSeparator(nestedObject.GetType(), parentPath, expander);
                        expanderMap[nestedExpander.Header.ToString()] = nestedExpander;
                        DisplayNestedProperties(nestedObject, node, parentPath, nestedExpander);
                    }
                }
                // Handle other value types
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

    private static List<Type> GetInheritanceHierarchy(Type type)
    {
        List<Type> hierarchy = [];

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
        if (nestedObject is null)
        {
            return;
        }

        Type nestedType = nestedObject.GetType();
        PropertyInfo[] properties = nestedType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Skip properties that don't have a getter
            if (property.GetGetMethod() == null || property.IsDefined(typeof(InspectorExcludeAttribute), false))
            {
                continue;
            }

            string fullPath = $"{parentPath}/{property.Name}";

            if (!nodePropertyValues[node].ContainsKey(fullPath))
            {
                nodePropertyValues[node][fullPath] = property.GetValue(nestedObject);
            }

            // Handle resource types (like Texture) within nested objects
            if (property.PropertyType == typeof(Texture))
            {
                FrameworkElement propertyControl = PropertyControlFactory.CreateControl(node, property, fullPath, nodePropertyValues[node]);
                if (propertyControl != null && parentExpander != null)
                {
                    AddPropertyControlToExpander(node, property, propertyControl, parentExpander, fullPath, nodePropertyValues[node]);
                }
            }
            // Handle nested objects recursively
            else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                var nestedValue = property.GetValue(nestedObject);
                if (nestedValue != null)
                {
                    Expander nestedExpander = AddInheritanceSeparator(nestedValue.GetType(), fullPath, parentExpander);
                    expanderMap[nestedExpander.Header.ToString()] = nestedExpander;
                    DisplayNestedProperties(nestedValue, node, fullPath, nestedExpander);
                }
            }
            // Handle other value types within nested objects
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
        // Use the full path as the expander key
        string expanderKey = path;

        if (expanderMap.TryGetValue(expanderKey, out Expander? value))
        {
            return value;
        }

        Expander expander = new()
        {
            Header = expanderKey.Split('/').Last(), // Display only the last part
            IsExpanded = true,
            BorderBrush = SeparatorColor,
            BorderThickness = new(0, 1, 0, 1),
            Margin = new(parentExpander != null ? 20 : 0, 5, 0, 5),
            Foreground = SeparatorColor
        };

        StackPanel contentPanel = new();
        expander.Content = contentPanel;

        if (parentExpander != null && parentExpander.Content is StackPanel parentContentPanel)
        {
            parentContentPanel.Children.Add(expander);
        }
        else
        {
            panel.Children.Add(expander);
        }

        expanderMap[expanderKey] = expander;
        return expander;
    }

    private void AddPropertyControlToExpander(Node node, PropertyInfo property, FrameworkElement propertyControl, Expander expander, string fullPath, Dictionary<string, object?> nodePropertyValues)
    {
        StackPanel controlAndResetPanel = new()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        Button resetButton = PropertyControlFactory.CreateResetButton(
            node,
            property,
            fullPath,
            nodePropertyValues,
            propertyControl);

        controlAndResetPanel.Children.Add(propertyControl);
        controlAndResetPanel.Children.Add(resetButton);

        // Get only the last part of the path for the label
        string labelText = fullPath.Split('/').Last();

        // If you still want to split CamelCase, you can apply it to labelText
        labelText = SplitCamelCase(labelText);

        TextBlock label = new()
        {
            Text = labelText,
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
                new() { Width = new GridLength(1, GridUnitType.Star) }
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

    private static string SplitCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        string result = Regex.Replace(
            input,
            "([a-z])([A-Z])",
            "$1 $2",
            RegexOptions.Compiled);

        result = Regex.Replace(
            result,
            "([A-Z])([A-Z][a-z])",
            "$1 $2",
            RegexOptions.Compiled);

        return result;
    }
}