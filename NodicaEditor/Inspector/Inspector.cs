using Nodica;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using TextBlock = System.Windows.Controls.TextBlock;
using VerticalAlignment = System.Windows.VerticalAlignment;
using Button = System.Windows.Controls.Button;

namespace NodicaEditor;

public partial class Inspector : Window
{
    private readonly StackPanel panel;
    private readonly Dictionary<Node, Dictionary<string, object?>> nodePropertyValues = [];
    private static readonly SolidColorBrush ForegroundColor = new(Colors.LightGray);
    private static readonly SolidColorBrush SeparatorColor = new(Colors.Gray);
    private readonly Dictionary<string, Expander> expanderMap = [];

    public Inspector(StackPanel inspectorPanel)
    {
        panel = inspectorPanel;
        expanderMap.Clear();
    }

    public Dictionary<string, object?> GetPropertyValues(Node node)
    {
        if (nodePropertyValues.TryGetValue(node, out var values))
        {
            return values;
        }

        return [];
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
                if (property.IsDefined(typeof(InspectorExcludeAttribute), false))
                    continue;

                if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
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
                    expanderMap[nestedExpander.Header.ToString()] = nestedExpander;
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
        if (expanderMap.ContainsKey(expanderKey))
            return expanderMap[expanderKey];

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

        expanderMap[expanderKey] = expander;
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

        // Split CamelCase and PascalCase property names into separate words
        string[] pathParts = fullPath.Split('/');
        string labelText = "";
        for (int i = 0; i < pathParts.Length; i++)
        {
            labelText += SplitCamelCase(pathParts[i]);
            if (i < pathParts.Length - 1)
            {
                labelText += "/";
            }
        }

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

    private static string SplitCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        string result = System.Text.RegularExpressions.Regex.Replace(
            input,
            "([a-z])([A-Z])",
            "$1 $2",
            System.Text.RegularExpressions.RegexOptions.Compiled);

        result = System.Text.RegularExpressions.Regex.Replace(
            result,
            "([A-Z])([A-Z][a-z])",
            "$1 $2",
            System.Text.RegularExpressions.RegexOptions.Compiled);

        return result;
    }
}