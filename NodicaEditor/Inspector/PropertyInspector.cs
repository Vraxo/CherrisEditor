using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Nodica;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using TextBlock = System.Windows.Controls.TextBlock;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace NodicaEditor;

public partial class PropertyInspector : Window
{
    private readonly StackPanel panel;
    private readonly Dictionary<Node, Dictionary<string, object?>> nodePropertyValues = new();
    private static readonly SolidColorBrush BackgroundColor = new(new() { R = 16, G = 16, B = 16, A = 255 });
    private static readonly SolidColorBrush ForegroundColor = new(Colors.LightGray);
    private static readonly SolidColorBrush SeparatorColor = new(Colors.Gray);
    private readonly Dictionary<string, Expander> expanderMap = new();

    public PropertyInspector(StackPanel inspectorPanel)
    {
        panel = inspectorPanel;
        expanderMap.Clear();
    }

    public Dictionary<string, object?> GetPropertyValues(Node node)
    {
        if (nodePropertyValues.TryGetValue(node, out Dictionary<string, object?>? values))
        {
            return values;
        }

        return new Dictionary<string, object?>();
    }

    public void DisplayNodeProperties(Node node)
    {
        panel.Children.Clear();
        expanderMap.Clear();

        // Initialize the dictionary for the node if it doesn't exist
        if (!nodePropertyValues.ContainsKey(node))
        {
            nodePropertyValues[node] = new Dictionary<string, object?>();
        }

        List<Type> hierarchy = GetInheritanceHierarchy(node.GetType());

        foreach (Type currentType in hierarchy)
        {
            PropertyInfo[] properties = currentType.GetProperties(
                BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance
            );

            if (properties.Length > 0)
            {
                Expander expander = AddInheritanceSeparator(currentType, node.GetType().Name, currentType.Name, null);
                expanderMap[expander.Header.ToString()] = expander;

                foreach (PropertyInfo property in properties)
                {
                    if (property.IsDefined(typeof(InspectorExcludeAttribute), false))
                    {
                        continue;
                    }

                    if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                    {
                        object? nestedObject = property.GetValue(node);
                        if (nestedObject != null)
                        {
                            string fullPath = $"{currentType.Name}/{property.Name}";
                            Expander nestedExpander = AddInheritanceSeparator(
                                nestedObject.GetType(), currentType.Name, fullPath, expander
                            );
                            expanderMap[nestedExpander.Header.ToString()] = nestedExpander;
                            DisplayNestedProperties(nestedObject, node, fullPath, nestedExpander);
                        }
                    }
                    else
                    {
                        string propertyName = $"{currentType.Name}/{property.Name}";

                        // Load value from dictionary if it exists, otherwise get it from the node
                        if (!nodePropertyValues[node].ContainsKey(propertyName))
                        {
                            nodePropertyValues[node][propertyName] = property.GetValue(node);
                        }

                        FrameworkElement propertyControl = PropertyControlFactory.CreateControl(
                            node, property, propertyName, nodePropertyValues[node]
                        );

                        if (propertyControl != null)
                        {
                            AddPropertyControlToExpander(
                                node, property, propertyControl, expander, propertyName, nodePropertyValues[node]
                            );
                        }
                    }
                }
            }
        }
    }

    private List<Type> GetInheritanceHierarchy(Type type)
    {
        List<Type> hierarchy = new();
        while (type != null && type != typeof(object))
        {
            hierarchy.Add(type);
            type = type.BaseType;
        }

        hierarchy.Reverse();
        return hierarchy;
    }

    private void DisplayNestedProperties(object nestedObject, Node node, string parentPath,
        Expander parentExpander)
    {
        if (nestedObject == null)
        {
            return;
        }

        Type nestedType = nestedObject.GetType();
        PropertyInfo[] properties = nestedType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (PropertyInfo property in properties)
        {
            if (property.IsDefined(typeof(InspectorExcludeAttribute), false))
            {
                continue;
            }

            string fullPath = $"{parentPath}/{property.Name}";

            // Load value from dictionary if it exists, otherwise get it from the nested object
            if (!nodePropertyValues[node].ContainsKey(fullPath))
            {
                nodePropertyValues[node][fullPath] = property.GetValue(nestedObject);
            }

            if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                object? nestedValue = property.GetValue(nestedObject);
                if (nestedValue != null)
                {
                    Expander nestedExpander = AddInheritanceSeparator(
                        nestedValue.GetType(), nestedType.Name, fullPath, parentExpander
                    );
                    expanderMap[nestedExpander.Header.ToString()] = nestedExpander;
                    DisplayNestedProperties(nestedValue, node, fullPath, nestedExpander);
                }
            }
            else
            {
                FrameworkElement propertyControl = PropertyControlFactory.CreateControl(
                    node, property, fullPath, nodePropertyValues[node]
                );

                if (propertyControl != null && parentExpander != null)
                {
                    AddPropertyControlToExpander(
                        node, property, propertyControl, parentExpander, fullPath, nodePropertyValues[node]
                    );
                }
            }
        }
    }

    private Expander AddInheritanceSeparator(Type type, string nodeTypeName, string fullPath,
        Expander parentExpander = null)
    {
        string expanderKey = $"{nodeTypeName}/{fullPath}";
        if (expanderMap.ContainsKey(expanderKey))
        {
            return expanderMap[expanderKey];
        }

        Expander expander = new()
        {
            Header = fullPath.Split('/').Last(),
            IsExpanded = true,
            BorderBrush = SeparatorColor,
            BorderThickness = new(0, 1, 0, 1),
            Margin = new(0, 5, 0, 5), // Default margin for class-level expanders
            Foreground = SeparatorColor
        };

        // Indentation for nested expanders
        if (parentExpander != null)
        {
            expander.Margin = new(20, 5, 0, 5); // Indent nested expanders
        }

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

    private void AddPropertyControlToExpander(Node node, PropertyInfo property, FrameworkElement propertyControl,
        Expander expander, string fullPath, Dictionary<string, object?> nodePropertyValues)
    {
        StackPanel controlAndResetPanel = new()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        controlAndResetPanel.Children.Add(propertyControl);
        controlAndResetPanel.Children.Add(
            PropertyControlFactory.CreateResetButton(node, property, fullPath, nodePropertyValues)
        );

        TextBlock label = new()
        {
            Text = property.Name + ":",
            MaxWidth = 250,
            TextWrapping = TextWrapping.Wrap,
            Foreground = ForegroundColor,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new(0, 0, 5, 0),
            HorizontalAlignment = HorizontalAlignment.Left
        };

        Grid propertyGrid = new()
        {
            Margin = new(5),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnDefinitions =
        {
            new() { Width = GridLength.Auto }, // Column for the label (auto-sized)
            new() { Width = new GridLength(1, GridUnitType.Star) }  // Column for the control (takes up remaining space)
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
}