using System.Numerics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Nodica;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
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
            return CreateStringControl(node, property, fullPath, nodePropertyValues);
        }

        if (propertyType == typeof(float))
        {
            return CreateFloatControl(node, property, fullPath, nodePropertyValues);
        }

        if (propertyType == typeof(bool))
        {
            return CreateBoolControl(node, property, fullPath, nodePropertyValues);
        }

        if (propertyType == typeof(Vector2))
        {
            return CreateVector2Control(node, property, fullPath, nodePropertyValues);
        }

        if (propertyType == typeof(Color))
        {
            return ColorControlGenerator.CreateColorControl(node, property, fullPath, nodePropertyValues);
        }

        if (propertyType.IsEnum)
        {
            return CreateEnumControl(node, property, fullPath, nodePropertyValues);
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

    private static TextBox CreateStringControl(Node node, PropertyInfo property, string fullPath, Dictionary<string, object?> nodePropertyValues)
    {
        string propertyName = fullPath != "" ? fullPath : property.Name;
        TextBox textBox = new() { Text = GetPropertyValue(nodePropertyValues, propertyName)?.ToString() ?? "", Width = 100, Height = 22, Tag = Tuple.Create(node, property, propertyName), Background = BackgroundColor, Foreground = ForegroundColor, BorderBrush = BackgroundColor, Style = null };
        textBox.TextChanged += (sender, e) => SetPropertyValue(nodePropertyValues, propertyName, ((TextBox)sender).Text);
        return textBox;
    }

    private static TextBox CreateFloatControl(Node node, PropertyInfo property, string fullPath, Dictionary<string, object?> nodePropertyValues)
    {
        string propertyName = fullPath != "" ? fullPath : property.Name;
        TextBox textBox = new() { Text = GetPropertyValue(nodePropertyValues, propertyName)?.ToString() ?? "", Width = 100, Height = 22, Tag = Tuple.Create(node, property, propertyName), Background = BackgroundColor, Foreground = ForegroundColor, BorderBrush = BackgroundColor, Style = null };
        textBox.TextChanged += (sender, e) =>
        {
            if (float.TryParse(((TextBox)sender).Text, out float value))
            {
                SetPropertyValue(nodePropertyValues, propertyName, value);
            }
        };
        return textBox;
    }

    private static CheckBox CreateBoolControl(Node node, PropertyInfo property, string fullPath, Dictionary<string, object?> nodePropertyValues)
    {
        string propertyName = fullPath != "" ? fullPath : property.Name;
        System.Windows.Controls.CheckBox checkBox = new() { IsChecked = (bool?)GetPropertyValue(nodePropertyValues, propertyName), Tag = Tuple.Create(node, property, propertyName), Background = BackgroundColor, Foreground = ForegroundColor, BorderBrush = BackgroundColor, VerticalAlignment = VerticalAlignment.Center, Style = null };
        checkBox.Checked += (sender, e) => SetPropertyValue(nodePropertyValues, propertyName, true);
        checkBox.Unchecked += (sender, e) => SetPropertyValue(nodePropertyValues, propertyName, false);
        return checkBox;
    }

    private static ComboBox CreateEnumControl(Node node, PropertyInfo property, string fullPath, Dictionary<string, object?> nodePropertyValues)
    {
        string propertyName = fullPath != "" ? fullPath : property.Name;

        ComboBox comboBox = new()
        {
            Width = 100,
            Height = 22,
            Tag = Tuple.Create(node, property, propertyName),
            Foreground = ForegroundColor,
            BorderBrush = BackgroundColor,
            BorderThickness = new Thickness(0),
            Style = null
        };

        // Create a new Style for ComboBoxItem
        Style itemStyle = new(typeof(ComboBoxItem));

        // Setters for the style (Background and Foreground)
        itemStyle.Setters.Add(new Setter(System.Windows.Controls.Control.BackgroundProperty, BackgroundColor));
        itemStyle.Setters.Add(new Setter(System.Windows.Controls.Control.ForegroundProperty, ForegroundColor));

        // Create a ControlTemplate
        ControlTemplate itemTemplate = new(typeof(ComboBoxItem));
        FrameworkElementFactory borderFactory = new(typeof(Border));

        // Set properties of the Border in the template
        borderFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(System.Windows.Controls.Control.BackgroundProperty));
        borderFactory.SetValue(Border.BorderThicknessProperty, new Thickness(0)); // No border
                                                                                  // OR: borderFactory.SetValue(Border.BorderBrushProperty, BackgroundColor); // Border same color as background

        // ContentPresenter to display the content
        FrameworkElementFactory contentPresenter = new(typeof(ContentPresenter));
        contentPresenter.SetValue(ContentPresenter.ContentTemplateProperty, new TemplateBindingExtension(ComboBoxItem.ContentTemplateProperty));
        contentPresenter.SetValue(ContentPresenter.ContentProperty, new TemplateBindingExtension(ComboBoxItem.ContentProperty));
        contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, new TemplateBindingExtension(ComboBoxItem.HorizontalContentAlignmentProperty));
        contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, new TemplateBindingExtension(ComboBoxItem.VerticalContentAlignmentProperty));

        // Add the ContentPresenter to the Border
        borderFactory.AppendChild(contentPresenter);

        // Set the VisualTree of the ControlTemplate to the Border
        itemTemplate.VisualTree = borderFactory;

        // Set the ControlTemplate of the Style
        itemStyle.Setters.Add(new Setter(ComboBoxItem.TemplateProperty, itemTemplate));

        // Apply the style to the ComboBox
        comboBox.ItemContainerStyle = itemStyle;

        // Populate the ComboBox with enum values
        foreach (var enumValue in Enum.GetValues(property.PropertyType))
        {
            comboBox.Items.Add(enumValue);
        }

        // Set the selected item
        comboBox.SelectedItem = GetPropertyValue(nodePropertyValues, propertyName);

        // Event handler for selection change
        comboBox.SelectionChanged += (sender, e) => SetPropertyValue(nodePropertyValues, propertyName, ((ComboBox)sender).SelectedItem);

        // Apply the ComboBox background on load
        comboBox.Loaded += ApplyComboBoxBackground;

        return comboBox;
    }

    private static void ApplyComboBoxBackground(object sender, RoutedEventArgs e)
    {
        if (sender is ComboBox loadedComboBox)
        {
            ToggleButton toggleButton = loadedComboBox.Template.FindName("toggleButton", loadedComboBox) as ToggleButton;
            if (toggleButton != null)
            {
                Border border = toggleButton.Template.FindName("templateRoot", toggleButton) as Border;
                if (border != null)
                {
                    border.Background = BackgroundColor;
                }
            }
        }
    }

    private static StackPanel CreateVector2Control(Node node, PropertyInfo property, string fullPath, Dictionary<string, object?> nodePropertyValues)
    {
        string propertyName = fullPath != "" ? fullPath : property.Name;
        Vector2 vectorValue = (Vector2)(GetPropertyValue(nodePropertyValues, propertyName) ?? Vector2.Zero);

        StackPanel vectorPanel = new() { Orientation = Orientation.Horizontal, Margin = new Thickness(2) };

        // X component
        TextBlock xLabel = new() { Text = "X:", VerticalAlignment = VerticalAlignment.Center, Foreground = ForegroundColor };
        TextBox xBox = CreateVectorComponentTextBox(vectorValue.X.ToString(), Tuple.Create(node, property, propertyName, "X"), nodePropertyValues);
        xBox.Margin = new Thickness(5, 0, 5, 0); // Add margin to the right of X component

        // Y component
        TextBlock yLabel = new() { Text = "Y:", VerticalAlignment = VerticalAlignment.Center, Foreground = ForegroundColor };
        TextBox yBox = CreateVectorComponentTextBox(vectorValue.Y.ToString(), Tuple.Create(node, property, propertyName, "Y"), nodePropertyValues);
        yBox.Margin = new Thickness(5, 0, 0, 0); // Add margin to the right of Y component (but not to the last element)

        vectorPanel.Children.Add(xLabel);
        vectorPanel.Children.Add(xBox);
        vectorPanel.Children.Add(yLabel);
        vectorPanel.Children.Add(yBox);

        return vectorPanel;
    }

    private static TextBox CreateVectorComponentTextBox(string initialValue, object tag, Dictionary<string, object?> nodePropertyValues)
    {
        TextBox textBox = new() { Text = initialValue, Width = 50, Height = 22, Tag = tag, Background = BackgroundColor, Foreground = ForegroundColor, BorderBrush = BackgroundColor, Style = null };
        textBox.TextChanged += (sender, e) =>
        {
            if (sender is TextBox tb && tb.Tag is Tuple<Node, PropertyInfo, string, string> tuple)
            {
                var (node, property, propertyName, component) = tuple;
                string value = tb.Text;
                try
                {
                    float componentValue = float.Parse(value);
                    Vector2 vectorValue = (Vector2)GetPropertyValue(nodePropertyValues, propertyName);
                    if (component == "X")
                    {
                        vectorValue.X = componentValue;
                    }
                    else if (component == "Y")
                    {
                        vectorValue.Y = componentValue;
                    }
                    SetPropertyValue(nodePropertyValues, propertyName, vectorValue);
                }
                catch (Exception ex)
                {
                    //MessageBox.Show($"Invalid value for {component} component of {propertyName}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        };
        return textBox;
    }

    private static void SetPropertyValue(Dictionary<string, object?> propertyValues, string propertyName, object? newValue)
    {
        propertyValues[propertyName] = newValue;
    }

    private static object? GetPropertyValue(Dictionary<string, object?> propertyValues, string propertyName)
    {
        if (propertyValues.TryGetValue(propertyName, out var value))
        {
            return value;
        }
        return null;
    }
}