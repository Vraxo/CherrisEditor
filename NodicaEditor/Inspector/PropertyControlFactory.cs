using System.Numerics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Nodica;
using Button = System.Windows.Controls.Button;
using Color = Raylib_cs.Color;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using TextBlock = System.Windows.Controls.TextBlock;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace NodicaEditor;

public static class PropertyControlFactory
{
    private static readonly SolidColorBrush BackgroundColor = new(new() { R = 16, G = 16, B = 16, A = 255 });
    private static readonly SolidColorBrush ForegroundColor = new(Colors.LightGray);
    private static readonly SolidColorBrush SeparatorColor = new(Colors.Gray);

    public static FrameworkElement CreateControl(Node node, PropertyInfo property, string fullPath = "", Dictionary<string, object?> nodePropertyValues = null)
    {
        var propertyType = property.PropertyType;

        if (propertyType == typeof(string))
        {
            return StringControlGenerator.CreateStringControl(node, property, fullPath, nodePropertyValues);
        }

        if (propertyType == typeof(float))
        {
            return FloatControlGenerator.CreateFloatControl(node, property, fullPath, nodePropertyValues);
        }

        if (propertyType == typeof(bool))
        {
            return BoolControlGenerator.CreateBoolControl(node, property, fullPath, nodePropertyValues);
        }

        if (propertyType == typeof(Vector2))
        {
            return Vector2ControlGenerator.CreateVector2Control(node, property, fullPath, nodePropertyValues);
        }

        if (propertyType == typeof(Color))
        {
            return ColorControlGenerator.CreateColorControl(node, property, fullPath, nodePropertyValues);
        }

        if (propertyType.IsEnum)
        {
            return EnumControlGenerator.CreateEnumControl(node, property, fullPath, nodePropertyValues);
        }

        return null;
    }

    public static Border CreateSeparatorLine() => new() { Height = 1, Background = SeparatorColor, Margin = new Thickness(0, 5, 0, 5) };

    public static TextBlock CreateTypeLabel(Type type) => new() { Text = type.Name, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 5, 0, 5), Foreground = SeparatorColor };

    public static Button CreateResetButton(Node node, PropertyInfo property, string propertyName, Dictionary<string, object?> nodePropertyValues)
    {
        System.Windows.Controls.Button resetButton = new() { Content = "Reset", Width = 50, Margin = new Thickness(5), Tag = (node, property, propertyName), Background = BackgroundColor, Foreground = ForegroundColor, BorderBrush = BackgroundColor, Style = null };
        resetButton.Click += (sender, e) =>
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is ValueTuple<Node, PropertyInfo, string> tag)
            {
                var (resetNode, resetProperty, resetPropertyName) = tag;
                object? defaultValue = DefaultValueProvider.GetDefaultValue(resetProperty, resetNode, resetPropertyName);
                SetPropertyValue(nodePropertyValues, resetPropertyName, defaultValue); // Update through SetPropertyValue

                // Find the control and update its value
                DependencyObject parent = VisualTreeHelper.GetParent(button);
                while (parent != null && !(parent is StackPanel))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                if (parent is StackPanel stackPanel)
                {
                    foreach (var child in stackPanel.Children)
                    {
                        if (child is Grid grid)
                        {
                            foreach (var gridChild in grid.Children)
                            {
                                if (gridChild is StackPanel controlPanel)
                                {
                                    foreach (var panelChild in controlPanel.Children)
                                    {
                                        if (panelChild is FrameworkElement fe && fe.Tag is Tuple<Node, PropertyInfo, string> elementTag && elementTag.Item3 == resetPropertyName)
                                        {
                                            UpdateControlValue(fe, defaultValue);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
        return resetButton;
    }

    private static void UpdateControlValue(FrameworkElement control, object? value)
    {
        switch (control)
        {
            case TextBox textBox:
                textBox.Text = value?.ToString() ?? "";
                break;
            case System.Windows.Controls.CheckBox checkBox:
                checkBox.IsChecked = (bool?)value;
                break;
            case ComboBox comboBox:
                comboBox.SelectedItem = value;
                break;
                // Add cases for other control types as needed
        }
    }

    private static void SetPropertyValue(Dictionary<string, object?> propertyValues, string propertyName, object? newValue)
    {
        propertyValues[propertyName] = newValue;
    }
}