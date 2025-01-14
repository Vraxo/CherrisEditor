using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Numerics;
using System.Windows.Controls.Primitives;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using VerticalAlignment = System.Windows.VerticalAlignment;
using Control = System.Windows.Controls.Control;
using TextBlock = System.Windows.Controls.TextBlock;
using Nodica;
using Raylib_cs;
using Color = Raylib_cs.Color;

namespace NodicaEditor;

public class PropertyInspector2
{
    private readonly StackPanel panel;
    private readonly Dictionary<Node, Dictionary<string, object?>> nodePropertyValues = new();
    private static readonly SolidColorBrush BackgroundColor = new(new() { R = 16, G = 16, B = 16, A = 255 });
    private static readonly SolidColorBrush ForegroundColor = new(Colors.LightGray);
    private static readonly SolidColorBrush SeparatorColor = new(Colors.Gray);

    public PropertyInspector2(StackPanel inspectorPanel)
    {
        panel = inspectorPanel;
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

        // Initialize or retrieve the property dictionary for the node
        if (!nodePropertyValues.ContainsKey(node))
        {
            nodePropertyValues[node] = new Dictionary<string, object?>();
        }

        PropertyInfo[] properties = node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        Type currentType = null;
        Expander currentExpander = null;

        foreach (var property in properties)
        {
            if (property.IsDefined(typeof(InspectorExcludeAttribute), false))
            {
                continue;
            }

            // Check if the property is a nested class and process its properties recursively
            if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                var nestedObject = property.GetValue(node);
                if (nestedObject != null)
                {
                    // Generate full path (e.g., "ThemePack/Current/FontSpacing")
                    string fullPath = $"{property.Name}";
                    DisplayNestedProperties(nestedObject, node, fullPath);
                }
            }
            else
            {
                // For simple, non-nested properties, add them normally
                string propertyName = property.Name;
                if (!nodePropertyValues[node].ContainsKey(propertyName))
                {
                    nodePropertyValues[node][propertyName] = property.GetValue(node);
                }

                // Handle inheritance separator
                if (currentType != property.DeclaringType)
                {
                    currentType = property.DeclaringType;
                    currentExpander = AddInheritanceSeparator(currentType);
                }

                // Generate and add the property control for the simple property
                FrameworkElement propertyControl = GeneratePropertyControl(node, property);
                if (propertyControl != null && currentExpander != null)
                {
                    // Add the property control to the current expander's content
                    AddPropertyControlToExpander(node, property, propertyControl, currentExpander);
                }
            }
        }
    }

    private void DisplayNestedProperties(object nestedObject, Node node, string parentPath)
    {
        if (nestedObject == null)
            return;

        PropertyInfo[] nestedProperties = nestedObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        Type currentType = null;
        Expander currentExpander = null;

        foreach (var property in nestedProperties)
        {
            if (property.IsDefined(typeof(InspectorExcludeAttribute), false))
            {
                continue;
            }

            // Build the full path for the nested property (e.g., "ThemePack/Current/FontSpacing")
            string fullPath = $"{parentPath}/{property.Name}";

            // Add the nested property to the nodePropertyValues dictionary with its full path
            if (!nodePropertyValues[node].ContainsKey(fullPath))
            {
                nodePropertyValues[node][fullPath] = property.GetValue(nestedObject);
            }

            // Handle inheritance separator for nested properties
            if (currentType != property.DeclaringType)
            {
                currentType = property.DeclaringType;
                currentExpander = AddInheritanceSeparator(currentType);
            }

            // Generate the control for the nested property
            FrameworkElement propertyControl = GeneratePropertyControl(node, property, fullPath);
            if (propertyControl != null && currentExpander != null)
            {
                // Add the property control to the current expander's content
                AddPropertyControlToExpander(node, property, propertyControl, currentExpander, fullPath);
            }

            // If the property is itself a nested class, recurse into it
            if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                var nestedValue = property.GetValue(nestedObject);
                if (nestedValue != null)
                {
                    DisplayNestedProperties(nestedValue, node, fullPath); // Continue processing nested objects
                }
            }
        }
    }

    private Expander AddInheritanceSeparator(Type type)
    {
        var expander = new Expander
        {
            Header = CreateTypeLabel(type),
            IsExpanded = true, // You can set this to false if you want the expander to start collapsed
            BorderBrush = SeparatorColor,
            BorderThickness = new Thickness(0, 1, 0, 1),
            Margin = new Thickness(0, 5, 0, 5),
            Foreground = SeparatorColor
        };

        var contentPanel = new StackPanel(); // Create a StackPanel to hold the content
        expander.Content = contentPanel; // Assign the StackPanel as the expander's content

        panel.Children.Add(expander);
        return expander;
    }

    private static TextBlock CreateTypeLabel(Type type) => new() { Text = type.Name, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 5, 0, 5) };

    private void AddPropertyControlToExpander(Node node, PropertyInfo property, FrameworkElement propertyControl, Expander expander, string fullPath = "")
    {
        // Wrap the property control and reset button in a StackPanel
        var controlAndResetPanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right // Align to the right
        };
        controlAndResetPanel.Children.Add(propertyControl);
        controlAndResetPanel.Children.Add(CreateResetButton(node, property, fullPath));

        string propertyName = fullPath != "" ? fullPath : property.Name;

        // Create the label
        TextBlock label = new()
        {
            Text = propertyName + ":",
            MaxWidth = 250,
            TextWrapping = TextWrapping.Wrap,
            Foreground = ForegroundColor,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 5, 0),
            HorizontalAlignment = HorizontalAlignment.Left
        };

        // Create a Grid to hold the label and the controls
        Grid propertyGrid = new()
        {
            Margin = new Thickness(5),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch, // Stretch to fill the width
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto }, // Auto size for label
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) } // Remaining space for controls
            }
        };

        // Add the label and the controls to the Grid
        Grid.SetColumn(label, 0);
        propertyGrid.Children.Add(label);
        Grid.SetColumn(controlAndResetPanel, 1);
        propertyGrid.Children.Add(controlAndResetPanel);

        // Add the Grid to the expander's content
        if (expander.Content is StackPanel contentPanel)
        {
            contentPanel.Children.Add(propertyGrid);
        }
    }



    private System.Windows.Controls.Button CreateResetButton(Node node, PropertyInfo property, string propertyName)
    {
        System.Windows.Controls.Button resetButton = new() { Content = "Reset", Width = 50, Margin = new Thickness(5), Tag = (node, property, propertyName), Background = BackgroundColor, Foreground = ForegroundColor, BorderBrush = BackgroundColor, Style = null };
        resetButton.Click += ResetButton_Click;
        return resetButton;
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is ValueTuple<Node, PropertyInfo, string> tag)
        {
            var (node, property, propertyName) = tag;
            object? defaultValue = GetDefaultValue(property, node, propertyName);
            nodePropertyValues[node][propertyName] = defaultValue;
            DisplayNodeProperties(node);
        }
    }

    //Modified this
    public static object? GetDefaultValue(PropertyInfo property, Node node, string fullPath = "")
    {
        try
        {
            if (string.IsNullOrEmpty(fullPath) || !fullPath.Contains("/"))
            {
                // For non-nested properties, use the original logic
                return property.GetValue(Activator.CreateInstance(node.GetType()));
            }
            else
            {
                // For nested properties, get the nested object and then get the property value
                string[] pathParts = fullPath.Split('/');
                object currentObject = node;
                PropertyInfo currentProperty = null;

                for (int i = 0; i < pathParts.Length; i++)
                {
                    currentProperty = currentObject.GetType().GetProperty(pathParts[i]);
                    if (currentProperty == null)
                    {
                        throw new InvalidOperationException($"Property '{pathParts[i]}' not found in path '{fullPath}'.");
                    }

                    if (i < pathParts.Length - 1)
                    {
                        currentObject = currentProperty.GetValue(currentObject);
                        if (currentObject == null)
                        {
                            // If any intermediate object is null, we can't proceed further
                            return null;
                        }
                    }
                }

                // currentProperty now refers to the actual nested property
                // Create a default instance of the parent object and get the default value of the property
                var parentObject = Activator.CreateInstance(currentObject.GetType());
                return currentProperty.GetValue(parentObject);
            }
        }
        catch (Exception ex)
        {
            // Log the exception details here to help diagnose the issue
            Console.WriteLine($"Exception in GetDefaultValue: {ex.Message}");

            // Fallback for unsupported types or any exceptions
            return property.PropertyType switch
            {
                Type boolType when boolType == typeof(bool) => false,
                Type intType when intType == typeof(int) => 0,
                Type floatType when floatType == typeof(float) => 0f,
                Type vector2Type when vector2Type == typeof(Vector2) => Vector2.Zero,
                Type colorType when colorType == typeof(Color) => default(Color), // Raylib Color
                _ => property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null
            };
        }
    }

    private FrameworkElement GeneratePropertyControl(Node node, PropertyInfo property, string fullPath = "")
    {
        var propertyType = property.PropertyType;

        if (propertyType == typeof(string))
        {
            return CreateStringControl(node, property, fullPath);
        }

        if (propertyType == typeof(float))
        {
            return CreateFloatControl(node, property, fullPath);
        }

        if (propertyType == typeof(bool))
        {
            return CreateBoolControl(node, property, fullPath);
        }

        if (propertyType == typeof(Vector2))
        {
            return GenerateVector2Control(node, property, fullPath);
        }

        // Use Raylib Color type
        if (propertyType == typeof(Color))
        {
            return CreateColorControl(node, property, fullPath);
        }

        if (propertyType.IsEnum)
        {
            return CreateEnumControl(node, property, fullPath);
        }

        return null;
    }

    private TextBox CreateStringControl(Node node, PropertyInfo property, string fullPath)
    {
        string propertyName = fullPath != "" ? fullPath : property.Name;
        TextBox textBox = new() { Text = nodePropertyValues[node][propertyName]?.ToString() ?? "", Width = 100, Height = 22, Tag = Tuple.Create(node, property, propertyName), Background = BackgroundColor, Foreground = ForegroundColor, BorderBrush = BackgroundColor, Style = null };
        textBox.TextChanged += PropertyValueChanged;
        return textBox;
    }

    private TextBox CreateFloatControl(Node node, PropertyInfo property, string fullPath)
    {
        string propertyName = fullPath != "" ? fullPath : property.Name;
        TextBox textBox = new() { Text = nodePropertyValues[node][propertyName]?.ToString() ?? "", Width = 100, Height = 22, Tag = Tuple.Create(node, property, propertyName), Background = BackgroundColor, Foreground = ForegroundColor, BorderBrush = BackgroundColor, Style = null };
        textBox.TextChanged += PropertyValueChanged;
        return textBox;
    }

    private System.Windows.Controls.CheckBox CreateBoolControl(Node node, PropertyInfo property, string fullPath)
    {
        string propertyName = fullPath != "" ? fullPath : property.Name;
        System.Windows.Controls.CheckBox checkBox = new() { IsChecked = (bool?)nodePropertyValues[node][propertyName], Tag = Tuple.Create(node, property, propertyName), Background = BackgroundColor, Foreground = ForegroundColor, BorderBrush = BackgroundColor, VerticalAlignment = VerticalAlignment.Center, Style = null };
        checkBox.Checked += BoolValueChanged;
        checkBox.Unchecked += BoolValueChanged;
        return checkBox;
    }

    private ComboBox CreateEnumControl(Node node, PropertyInfo property, string fullPath)
    {
        string propertyName = fullPath != "" ? fullPath : property.Name;
        ComboBox comboBox = new() { Width = 100, Height = 22, Tag = Tuple.Create(node, property, propertyName), Foreground = ForegroundColor, BorderBrush = BackgroundColor, Style = null };

        comboBox.ItemContainerStyle = new Style(typeof(ComboBoxItem));
        comboBox.ItemContainerStyle.Setters.Add(new Setter(System.Windows.Controls.Control.BackgroundProperty, BackgroundColor));
        comboBox.ItemContainerStyle.Setters.Add(new Setter(System.Windows.Controls.Control.ForegroundProperty, ForegroundColor));

        foreach (var enumValue in Enum.GetValues(property.PropertyType))
        {
            comboBox.Items.Add(enumValue);
        }

        comboBox.SelectedItem = nodePropertyValues[node][propertyName];
        comboBox.SelectionChanged += EnumValueChanged;
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

    private void PropertyValueChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox && textBox.Tag is Tuple<Node, PropertyInfo, string> tag)
        {
            var (node, property, propertyName) = tag;
            try
            {
                object newValue = property.PropertyType == typeof(float)
                    ? float.Parse(textBox.Text)
                    : textBox.Text;

                nodePropertyValues[node][propertyName] = newValue;
            }
            catch
            {
                MessageBox.Show($"Invalid value for {propertyName}. Please enter a valid {property.PropertyType.Name}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void EnumValueChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.Tag is Tuple<Node, PropertyInfo, string> tag)
        {
            var (node, property, propertyName) = tag;
            object? newValue = comboBox.SelectedItem;

            if (newValue != null)
            {
                nodePropertyValues[node][propertyName] = newValue;
            }
        }
    }

    private void BoolValueChanged(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.CheckBox checkBox && checkBox.Tag is Tuple<Node, PropertyInfo, string> tag)
        {
            var (node, property, propertyName) = tag;
            bool? newValue = checkBox.IsChecked;
            nodePropertyValues[node][propertyName] = newValue;
        }
    }

    private FrameworkElement GenerateVector2Control(Node node, PropertyInfo property, string fullPath)
    {
        string propertyName = fullPath != "" ? fullPath : property.Name;
        Vector2 vectorValue = (Vector2)(nodePropertyValues[node][propertyName] ?? Vector2.Zero);

        StackPanel vectorPanel = new() { Orientation = Orientation.Horizontal, Margin = new Thickness(2) };

        TextBlock xLabel = new() { Text = "X:", VerticalAlignment = VerticalAlignment.Center, Foreground = ForegroundColor };
        TextBox xBox = CreateVectorComponentTextBox(vectorValue.X.ToString(), Tuple.Create(node, property, propertyName, "X"));
        xBox.Margin = new Thickness(5, 0, 10, 0);

        TextBlock yLabel = new() { Text = "Y:", VerticalAlignment = VerticalAlignment.Center, Foreground = ForegroundColor };
        TextBox yBox = CreateVectorComponentTextBox(vectorValue.Y.ToString(), Tuple.Create(node, property, propertyName, "Y"));

        vectorPanel.Children.Add(xLabel);
        vectorPanel.Children.Add(xBox);
        vectorPanel.Children.Add(yLabel);
        vectorPanel.Children.Add(yBox);

        return vectorPanel;
    }

    private FrameworkElement CreateColorControl(Node node, PropertyInfo property, string fullPath)
    {
        string propertyName = fullPath != "" ? fullPath : property.Name;
        // Correctly handle nested Color properties
        Color colorValue = GetColorValue(node, property, fullPath);

        StackPanel colorPanel = new() { Orientation = Orientation.Horizontal, Margin = new Thickness(2) };

        TextBlock rLabel = new() { Text = "R:", VerticalAlignment = VerticalAlignment.Center, Foreground = ForegroundColor };
        TextBox rBox = CreateColorComponentTextBox(colorValue.R.ToString(), Tuple.Create(node, property, propertyName, "R"));
        rBox.Margin = new Thickness(5, 0, 10, 0);

        TextBlock gLabel = new() { Text = "G:", VerticalAlignment = VerticalAlignment.Center, Foreground = ForegroundColor };
        TextBox gBox = CreateColorComponentTextBox(colorValue.G.ToString(), Tuple.Create(node, property, propertyName, "G"));
        gBox.Margin = new Thickness(5, 0, 10, 0);

        TextBlock bLabel = new() { Text = "B:", VerticalAlignment = VerticalAlignment.Center, Foreground = ForegroundColor };
        TextBox bBox = CreateColorComponentTextBox(colorValue.B.ToString(), Tuple.Create(node, property, propertyName, "B"));
        bBox.Margin = new Thickness(5, 0, 10, 0);

        TextBlock aLabel = new() { Text = "A:", VerticalAlignment = VerticalAlignment.Center, Foreground = ForegroundColor };
        TextBox aBox = CreateColorComponentTextBox(colorValue.A.ToString(), Tuple.Create(node, property, propertyName, "A"));

        colorPanel.Children.Add(rLabel);
        colorPanel.Children.Add(rBox);
        colorPanel.Children.Add(gLabel);
        colorPanel.Children.Add(gBox);
        colorPanel.Children.Add(bLabel);
        colorPanel.Children.Add(bBox);
        colorPanel.Children.Add(aLabel);
        colorPanel.Children.Add(aBox);

        return colorPanel;
    }

    private Color GetColorValue(Node node, PropertyInfo property, string fullPath)
    {
        if (nodePropertyValues[node].TryGetValue(fullPath, out object? colorValue) && colorValue is Color color)
        {
            return color;
        }

        // Fallback to default color if not found or not a Color type
        return new Color(0, 0, 0, 0);
    }

    private TextBox CreateVectorComponentTextBox(string initialValue, object tag)
    {
        TextBox textBox = new() { Text = initialValue, Width = 50, Height = 22, Tag = tag, Background = BackgroundColor, Foreground = ForegroundColor, BorderBrush = BackgroundColor, Style = null };
        textBox.TextChanged += VectorComponentValueChanged;
        return textBox;
    }

    private TextBox CreateColorComponentTextBox(string initialValue, object tag)
    {
        TextBox textBox = new() { Text = initialValue, Width = 30, Height = 22, Tag = tag, Background = BackgroundColor, Foreground = ForegroundColor, BorderBrush = BackgroundColor, Style = null };
        textBox.TextChanged += ColorComponentValueChanged;
        return textBox;
    }

    private void VectorComponentValueChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox && textBox.Tag is Tuple<Node, PropertyInfo, string, string> tag)
        {
            var (node, property, propertyName, component) = tag;
            string value = textBox.Text;
            try
            {
                float componentValue = float.Parse(value);
                Vector2 vectorValue = (Vector2)nodePropertyValues[node][propertyName];
                if (component == "X")
                {
                    vectorValue.X = componentValue;
                }
                else if (component == "Y")
                {
                    vectorValue.Y = componentValue;
                }
                nodePropertyValues[node][propertyName] = vectorValue;
            }
            catch
            {
                MessageBox.Show($"Invalid value for {component} component of {propertyName}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void ColorComponentValueChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox && textBox.Tag is Tuple<Node, PropertyInfo, string, string> tag)
        {
            var (node, property, propertyName, component) = tag;
            string value = textBox.Text;
            try
            {
                byte componentValue = byte.Parse(value);
                Color colorValue = (Color)nodePropertyValues[node][propertyName];

                if (component == "R")
                {
                    colorValue.R = componentValue;
                }
                else if (component == "G")
                {
                    colorValue.G = componentValue;
                }
                else if (component == "B")
                {
                    colorValue.B = componentValue;
                }
                else if (component == "A")
                {
                    colorValue.A = componentValue;
                }

                nodePropertyValues[node][propertyName] = colorValue;
            }
            catch
            {
                MessageBox.Show($"Invalid value for {component} component of {propertyName}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}