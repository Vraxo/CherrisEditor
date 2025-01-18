using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Nodica;

namespace NodicaEditor;

public class StringControlGenerator
{
    private static readonly SolidColorBrush BackgroundColor = new SolidColorBrush(Color.FromArgb(255, 16, 16, 16));
    private static readonly SolidColorBrush ForegroundColor = new SolidColorBrush(Colors.LightGray);

    public static TextBox CreateStringControl(Node node, PropertyInfo property, string fullPath, Dictionary<string, object?> nodePropertyValues)
    {
        string propertyName = fullPath != "" ? fullPath : property.Name;

        // Get the current value or default to empty string
        string? currentValue = GetStringValue(nodePropertyValues, propertyName) ?? "";

        TextBox textBox = new TextBox
        {
            Text = currentValue,
            Width = 100,
            Height = 22,
            Background = BackgroundColor,
            Foreground = ForegroundColor,
            BorderBrush = BackgroundColor,
            Style = null,
            AllowDrop = true
        };

        // Assign event handlers with captured variables
        textBox.DragEnter += OnTextBoxDragEnter;
        textBox.PreviewDragOver += OnTextBoxPreviewDragOver;
        textBox.PreviewDrop += (sender, e) => OnTextBoxPreviewDrop(sender, e, nodePropertyValues, propertyName);
        textBox.PreviewDragEnter += OnTextBoxPreviewDragEnter;
        textBox.TextChanged += (sender, e) => OnTextBoxTextChanged(sender, nodePropertyValues, propertyName);

        return textBox;
    }

    private static void OnTextBoxDragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.Text))
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

    private static void OnTextBoxPreviewDrop(object sender, DragEventArgs e, Dictionary<string, object?> nodePropertyValues, string propertyName)
    {
        if (e.Data.GetDataPresent(DataFormats.Text))
        {
            // Prevent default TextBox drop behavior
            e.Handled = true;

            // Get the dropped file path
            var filePath = (string)e.Data.GetData(DataFormats.Text);

            // Update TextBox text and node property value
            if (sender is TextBox textBox)
            {
                textBox.Text = filePath;
                SetStringValue(nodePropertyValues, propertyName, filePath);
            }
        }
    }

    private static void OnTextBoxPreviewDragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.Text))
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
    }

    private static void OnTextBoxTextChanged(object sender, Dictionary<string, object?> nodePropertyValues, string propertyName)
    {
        if (sender is TextBox textBox)
        {
            SetStringValue(nodePropertyValues, propertyName, textBox.Text);
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
