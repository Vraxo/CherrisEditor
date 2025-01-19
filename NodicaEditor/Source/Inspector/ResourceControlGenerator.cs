using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Cherris;
using Button = System.Windows.Controls.Button;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace CherrisEditor;

public class ResourceControlGenerator
{
    private static readonly SolidColorBrush BackgroundColor = new(new() { R = 16, G = 16, B = 16, A = 255 });
    private static readonly SolidColorBrush ForegroundColor = new(Colors.LightGray);

    public static FrameworkElement CreateResourceControl(Node node, PropertyInfo property, string fullPath, Dictionary<string, object?> nodePropertyValues)
    {
        string propertyName = fullPath != "" ? fullPath : property.Name;

        // Get the current value or default to empty string
        string? currentValue = GetStringValue(nodePropertyValues, propertyName) ?? "";

        TextBox textBox = new()
        {
            Text = currentValue,
            Width = 100,
            Height = 22,
            Background = BackgroundColor,
            Foreground = ForegroundColor,
            BorderBrush = BackgroundColor,
            Style = null,
            AllowDrop = true,
            IsReadOnly = true
        };

        Button selectButton = new()
        {
            Content = "Select",
            Width = 50,
            Height = 22,
            Margin = new(5, 0, 0, 0),
            Background = BackgroundColor,
            Foreground = ForegroundColor,
            BorderBrush = BackgroundColor,
            Style = null
        };

        // Assign event handlers with captured variables
        textBox.DragEnter += OnTextBoxDragEnter;
        textBox.PreviewDragOver += OnTextBoxPreviewDragOver;
        textBox.PreviewDrop += (sender, e) => OnTextBoxPreviewDrop(sender, e, nodePropertyValues, propertyName, textBox);
        textBox.PreviewDragEnter += OnTextBoxPreviewDragEnter;
        selectButton.Click += (sender, e) => OnSelectButtonClick(sender, e, node, property, nodePropertyValues, propertyName, textBox);

        StackPanel resourcePanel = new()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        resourcePanel.Children.Add(textBox);
        resourcePanel.Children.Add(selectButton);

        return resourcePanel;
    }

    private static void OnTextBoxDragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effects = DragDropEffects.Copy;
        else
            e.Effects = DragDropEffects.None;
    }

    private static void OnTextBoxPreviewDragOver(object sender, DragEventArgs e)
    {
        // Suppress default drag behavior
        e.Effects = DragDropEffects.Copy;
        e.Handled = true;
    }

    private static void OnTextBoxPreviewDrop(object sender, DragEventArgs e, Dictionary<string, object?> nodePropertyValues, string propertyName, TextBox textBox)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            // Prevent default TextBox drop behavior
            e.Handled = true;

            // Get the dropped file path
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                string filePath = files[0];

                // Update TextBox text and node property value
                textBox.Text = filePath;
                SetStringValue(nodePropertyValues, propertyName, filePath);
            }
        }
    }

    private static void OnTextBoxPreviewDragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
    }

    private static void OnSelectButtonClick(object sender, RoutedEventArgs e, Node node, PropertyInfo property, Dictionary<string, object?> nodePropertyValues, string propertyName, TextBox textBox)
    {
        OpenFileDialog openFileDialog = new()
        {
            Filter = GetFilterForResourceType(property.PropertyType),
            Title = $"Select a Resource for {propertyName}"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            string filePath = openFileDialog.FileName;
            textBox.Text = filePath;
            SetStringValue(nodePropertyValues, propertyName, filePath);
        }
    }

    private static string GetFilterForResourceType(Type resourceType)
    {
        if (resourceType == typeof(Audio))
        {
            return "Audio Files (*.wav;*.mp3;*.ogg)|*.wav;*.mp3;*.ogg|All files (*.*)|*.*";
        }
        else if (resourceType == typeof(Font))
        {
            return "Font Files (*.ttf;*.otf)|*.ttf;*.otf|All files (*.*)|*.*";
        }
        else if (resourceType == typeof(Texture))
        {
            return "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*";
        }
        else
        {
            return "All files (*.*)|*.*";
        }
    }

    private static string? GetStringValue(Dictionary<string, object?> propertyValues, string propertyName)
    {
        return propertyValues.TryGetValue(propertyName, out object? value) && value is string str
            ? str
            : null;
    }

    private static void SetStringValue(Dictionary<string, object?> propertyValues, string propertyName, string? value)
    {
        propertyValues[propertyName] = value;
    }
}