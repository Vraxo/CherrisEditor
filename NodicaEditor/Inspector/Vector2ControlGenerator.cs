using System.Numerics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Nodica;
using TextBlock = System.Windows.Controls.TextBlock;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace NodicaEditor;

public class Vector2ControlGenerator
{
    private static readonly SolidColorBrush BackgroundColor = new(new() { R = 16, G = 16, B = 16, A = 255 });
    private static readonly SolidColorBrush ForegroundColor = new(Colors.LightGray);

    public static StackPanel CreateVector2Control(Node node, PropertyInfo property, string fullPath, Dictionary<string, object?> nodePropertyValues)
    {
        string propertyName = fullPath != "" ? fullPath : property.Name;
        Vector2 initialValue = GetVector2(nodePropertyValues, propertyName);

        StackPanel vectorPanel = new()
        {
            Orientation = Orientation.Horizontal,
            Margin = new(2)
        };

        (string Name, string InitialValue)[] components =
        [
            ("X", initialValue.X.ToString()),
            ("Y", initialValue.Y.ToString())
        ];

        foreach ((string componentName, string componentInitialValue) in components)
        {
            FrameworkElement componentControl = CreateComponentControl(
                node,
                property,
                propertyName,
                componentName,
                componentInitialValue,
                nodePropertyValues);

            vectorPanel.Children.Add(componentControl);
        }

        return vectorPanel;
    }

    private static FrameworkElement CreateComponentControl(Node node, PropertyInfo property, string propertyName, string componentName, string initialValue, Dictionary<string, object?> nodePropertyValues)
    {
        TextBlock label = new()
        {
            Text = $"{componentName}:",
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = ForegroundColor
        };

        TextBox textBox = CreateComponentTextBox(
            initialValue,
            node,
            property,
            propertyName,
            componentName,
            nodePropertyValues
        );
        textBox.Margin = new(5, 0, (componentName == "Y" ? 0 : 5), 0);

        StackPanel componentPanel = new() { Orientation = Orientation.Horizontal };
        componentPanel.Children.Add(label);
        componentPanel.Children.Add(textBox);

        return componentPanel;
    }

    private static TextBox CreateComponentTextBox(string initialValue, Node node, PropertyInfo property, string propertyName, string componentName, Dictionary<string, object?> nodePropertyValues)
    {
        TextBox textBox = new()
        {
            Text = initialValue,
            Width = 50,
            Height = 22,
            Background = BackgroundColor,
            Foreground = ForegroundColor,
            BorderBrush = BackgroundColor,
            Style = null
        };

        textBox.TextChanged += (sender, _) =>
        {
            if (sender is TextBox tb)
            {
                OnComponentTextChanged(
                    tb,
                    node,
                    property,
                    propertyName,
                    componentName,
                    nodePropertyValues
                );
            }
        };

        return textBox;
    }

    private static void OnComponentTextChanged(TextBox tb, Node node, PropertyInfo property, string propertyName, string componentName, Dictionary<string, object?> nodePropertyValues)
    {
        try
        {
            float newComponentValue = float.Parse(tb.Text);
            Vector2 currentVector = GetVector2(nodePropertyValues, propertyName);
            Vector2 updatedVector = UpdateVectorComponent(
                currentVector,
                componentName,
                newComponentValue
            );

            SetVector2(nodePropertyValues, propertyName, updatedVector);
        }
        catch (Exception ex)
        {
            // Handle potential parsing errors, etc.
        }
    }

    private static Vector2 UpdateVectorComponent(Vector2 vector, string componentName, float value)
    {
        return componentName switch
        {
            "X" => vector with { X = value },
            "Y" => vector with { Y = value },
            _ => throw new ArgumentException($"Invalid component name: {componentName}")
        };
    }

    private static Vector2 GetVector2(Dictionary<string, object?> propertyValues, string propertyName)
    {
        return propertyValues.TryGetValue(propertyName, out object? value) && value is Vector2 vector
            ? vector
            : Vector2.Zero;
    }

    private static void SetVector2(Dictionary<string, object?> propertyValues, string propertyName, Vector2 vector)
    {
        propertyValues[propertyName] = vector;
    }
}