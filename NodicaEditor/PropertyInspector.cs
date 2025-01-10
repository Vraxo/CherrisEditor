using System.Numerics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Nodica;
using VerticalAlignment = System.Windows.VerticalAlignment;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Control = System.Windows.Controls.Control;
using TextBlock = System.Windows.Controls.TextBlock;

namespace NodicaEditor;

public class PropertyInspector
{
    private readonly StackPanel panel;
    private readonly Dictionary<Node, Dictionary<string, object?>> nodePropertyValues = new();
    private static readonly SolidColorBrush BackgroundColor = new(new() { R = 16, G = 16, B = 16, A = 255 });
    private static readonly SolidColorBrush ForegroundColor = new(Colors.LightGray);
    private static readonly SolidColorBrush SeparatorColor = new(Colors.Gray);

    public PropertyInspector(StackPanel inspectorPanel)
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
        if (!nodePropertyValues.ContainsKey(node))
        {
            nodePropertyValues[node] = new Dictionary<string, object?>();
        }

        PropertyInfo[] properties = node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        Type currentType = null;

        foreach (var property in properties)
        {
            if (property.IsDefined(typeof(InspectorExcludeAttribute), false))
            {
                continue;
            }

            if (!nodePropertyValues[node].ContainsKey(property.Name))
            {
                nodePropertyValues[node][property.Name] = property.GetValue(node);
            }

            if (currentType != property.DeclaringType)
            {
                currentType = property.DeclaringType;
                AddInheritanceSeparator(currentType);
            }

            FrameworkElement propertyControl = GeneratePropertyControl(node, property);
            if (propertyControl != null)
            {
                AddPropertyControl(node, property, propertyControl);
            }
        }
    }

    private void AddInheritanceSeparator(Type type)
    {
        panel.Children.Add(CreateSeparatorLine());
        panel.Children.Add(CreateTypeLabel(type));
        panel.Children.Add(CreateSeparatorLine());
    }

    private static Border CreateSeparatorLine() => new() { Height = 1, Background = SeparatorColor, Margin = new Thickness(0, 5, 0, 5) };

    private static TextBlock CreateTypeLabel(Type type) => new() { Text = type.Name, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = SeparatorColor, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 5, 0, 5) };

    private void AddPropertyControl(Node node, PropertyInfo property, FrameworkElement propertyControl)
    {
        StackPanel propertyPanel = new() { Orientation = Orientation.Horizontal, Margin = new Thickness(5), VerticalAlignment = VerticalAlignment.Center };

        TextBlock label = new() { Text = property.Name + ":", Width = 120, Foreground = ForegroundColor, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };

        System.Windows.Controls.Button resetButton = CreateResetButton(node, property);
        resetButton.VerticalAlignment = VerticalAlignment.Center;

        propertyPanel.Children.Add(label);
        propertyPanel.Children.Add(propertyControl);
        propertyPanel.Children.Add(resetButton);
        panel.Children.Add(propertyPanel);
    }

    private System.Windows.Controls.Button CreateResetButton(Node node, PropertyInfo property)
    {
        System.Windows.Controls.Button resetButton = new() { Content = "Reset", Width = 50, Margin = new Thickness(5), Tag = (node, property), Background = BackgroundColor, Foreground = ForegroundColor, BorderBrush = BackgroundColor, Style = null };
        resetButton.Click += ResetButton_Click;
        return resetButton;
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is ValueTuple<Node, PropertyInfo> tag)
        {
            var (node, property) = tag;
            object? defaultValue = GetDefaultValue(property, node);
            nodePropertyValues[node][property.Name] = defaultValue;
            DisplayNodeProperties(node);
        }
    }

    public static object? GetDefaultValue(PropertyInfo property, Node node)
    {
        try
        {
            return property.GetValue(Activator.CreateInstance(node.GetType()));
        }
        catch
        {
            return property.PropertyType switch
            {
                Type boolType when boolType == typeof(bool) => false,
                Type intType when intType == typeof(int) => 0,
                Type floatType when floatType == typeof(float) => 0f,
                Type vector2Type when vector2Type == typeof(Vector2) => Vector2.Zero,
                _ => property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null
            };
        }
    }

    private FrameworkElement GeneratePropertyControl(Node node, PropertyInfo property)
    {
        var propertyType = property.PropertyType;

        if (propertyType == typeof(string))
        {
            return CreateStringControl(node, property);
        }

        if (propertyType == typeof(float))
        {
            return CreateFloatControl(node, property);
        }

        if (propertyType == typeof(bool))
        {
            return CreateBoolControl(node, property);
        }

        if (propertyType == typeof(Vector2))
        {
            return GenerateVector2Control(node, property);
        }

        if (propertyType.IsEnum)
        {
            return CreateEnumControl(node, property);
        }

        return null;
    }

    private TextBox CreateStringControl(Node node, PropertyInfo property)
    {
        TextBox textBox = new() { Text = nodePropertyValues[node][property.Name]?.ToString() ?? "", Width = 100, Height = 22, Tag = Tuple.Create(node, property), Background = BackgroundColor, Foreground = ForegroundColor, BorderBrush = BackgroundColor, Style = null };
        textBox.TextChanged += PropertyValueChanged;
        return textBox;
    }

    private TextBox CreateFloatControl(Node node, PropertyInfo property)
    {
        TextBox textBox = new() { Text = nodePropertyValues[node][property.Name]?.ToString() ?? "", Width = 100, Height = 22, Tag = Tuple.Create(node, property), Background = BackgroundColor, Foreground = ForegroundColor, BorderBrush = BackgroundColor, Style = null };
        textBox.TextChanged += PropertyValueChanged;
        return textBox;
    }

    private System.Windows.Controls.CheckBox CreateBoolControl(Node node, PropertyInfo property)
    {
        System.Windows.Controls.CheckBox checkBox = new() { IsChecked = (bool?)nodePropertyValues[node][property.Name], Tag = Tuple.Create(node, property), Background = BackgroundColor, Foreground = ForegroundColor, BorderBrush = BackgroundColor, VerticalAlignment = VerticalAlignment.Center, Style = null };
        checkBox.Checked += BoolValueChanged;
        checkBox.Unchecked += BoolValueChanged;
        return checkBox;
    }

    private ComboBox CreateEnumControl(Node node, PropertyInfo property)
    {
        ComboBox comboBox = new() { Width = 100, Height = 22, Tag = Tuple.Create(node, property), Foreground = ForegroundColor, BorderBrush = BackgroundColor, Style = null };

        comboBox.ItemContainerStyle = new Style(typeof(ComboBoxItem));
        comboBox.ItemContainerStyle.Setters.Add(new Setter(System.Windows.Controls.Control.BackgroundProperty, BackgroundColor));
        comboBox.ItemContainerStyle.Setters.Add(new Setter(System.Windows.Controls.Control.ForegroundProperty, ForegroundColor));

        foreach (var enumValue in Enum.GetValues(property.PropertyType))
        {
            comboBox.Items.Add(enumValue);
        }

        comboBox.SelectedItem = nodePropertyValues[node][property.Name];
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
        if (sender is TextBox textBox && textBox.Tag is Tuple<Node, PropertyInfo> tag)
        {
            var (node, property) = tag;
            try
            {
                object newValue = property.PropertyType == typeof(float)
                    ? float.Parse(textBox.Text)
                    : textBox.Text;

                nodePropertyValues[node][property.Name] = newValue;
            }
            catch
            {
                MessageBox.Show($"Invalid value for {property.Name}. Please enter a valid {property.PropertyType.Name}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void BoolValueChanged(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.CheckBox checkBox && checkBox.Tag is Tuple<Node, PropertyInfo> tag)
        {
            var (node, property) = tag;
            bool? newValue = checkBox.IsChecked;
            nodePropertyValues[node][property.Name] = newValue;
        }
    }

    private FrameworkElement GenerateVector2Control(Node node, PropertyInfo property)
    {
        Vector2 vectorValue = (Vector2)(nodePropertyValues[node][property.Name] ?? Vector2.Zero);

        StackPanel vectorPanel = new() { Orientation = Orientation.Horizontal, Margin = new Thickness(2) };

        TextBlock xLabel = new() { Text = "X:", VerticalAlignment = VerticalAlignment.Center, Foreground = ForegroundColor };
        TextBox xBox = CreateVectorComponentTextBox(vectorValue.X.ToString(), Tuple.Create(node, property, "X"));
        xBox.Margin = new Thickness(5, 0, 10, 0);

        TextBlock yLabel = new() { Text = "Y:", VerticalAlignment = VerticalAlignment.Center, Foreground = ForegroundColor };
        yLabel.Margin = new Thickness(5, 0, 0, 0);
        TextBox yBox = CreateVectorComponentTextBox(vectorValue.Y.ToString(), Tuple.Create(node, property, "Y"));
        yBox.Margin = new Thickness(5, 0, 0, 0);

        vectorPanel.Children.Add(xLabel);
        vectorPanel.Children.Add(xBox);
        vectorPanel.Children.Add(yLabel);
        vectorPanel.Children.Add(yBox);

        return vectorPanel;
    }

    private TextBox CreateVectorComponentTextBox(string text, Tuple<Node, PropertyInfo, string> tag)
    {
        TextBox textBox = new() { Width = 50, Height = 22, Text = text, Margin = new Thickness(2), Tag = tag, Background = BackgroundColor, Foreground = ForegroundColor, BorderBrush = BackgroundColor, Style = null };
        textBox.TextChanged += VectorValueChanged;
        return textBox;
    }

    private void VectorValueChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox && textBox.Tag is Tuple<Node, PropertyInfo, string> tag)
        {
            var (node, property, axis) = tag;
            Vector2 currentVector = (Vector2)(nodePropertyValues[node][property.Name] ?? Vector2.Zero);

            try
            {
                float newValue = float.Parse(textBox.Text);
                Vector2 newVector = axis == "X" ? new Vector2(newValue, currentVector.Y) : new Vector2(currentVector.X, newValue);
                nodePropertyValues[node][property.Name] = newVector;
            }
            catch
            {
                MessageBox.Show($"Invalid value for {property.Name}. Please enter a valid number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void EnumValueChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.Tag is Tuple<Node, PropertyInfo> tag)
        {
            var (node, property) = tag;
            object? newValue = comboBox.SelectedItem;
            nodePropertyValues[node][property.Name] = newValue;
        }
    }
}