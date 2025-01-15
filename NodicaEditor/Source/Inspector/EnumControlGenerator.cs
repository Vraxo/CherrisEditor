using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Nodica;

namespace NodicaEditor;

public class EnumControlGenerator
{
    private static readonly SolidColorBrush BackgroundColor = new(new() { R = 16, G = 16, B = 16, A = 255 });
    private static readonly SolidColorBrush ForegroundColor = new(Colors.LightGray);

    public static ComboBox CreateEnumControl(Node node, PropertyInfo property, string fullPath, Dictionary<string, object?> nodePropertyValues)
    {
        string propertyName = fullPath != "" ? fullPath : property.Name;

        ComboBox comboBox = new()
        {
            Width = 100,
            Height = 22,
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
        comboBox.SelectedItem = GetEnumValue(nodePropertyValues, property, propertyName);

        // Event handler for selection change
        comboBox.SelectionChanged += (sender, e) =>
        {
            if (sender is ComboBox cb)
            {
                SetEnumValue(nodePropertyValues, propertyName, cb.SelectedItem);
            }
        };

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

    private static object GetEnumValue(Dictionary<string, object?> propertyValues, PropertyInfo property, string propertyName)
    {
        if (propertyValues.TryGetValue(propertyName, out object? value))
        {
            return value;
        }

        // Return the default value of the enum type if not found
        return Activator.CreateInstance(property.PropertyType);
    }

    private static void SetEnumValue(Dictionary<string, object?> propertyValues, string propertyName, object value)
    {
        propertyValues[propertyName] = value;
    }
}