using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Cherris;
using Microsoft.Win32;
using Button = System.Windows.Controls.Button;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace CherrisEditor;

public class ResourceControlGenerator
{
    private static readonly SolidColorBrush BackgroundColor = new(new() { R = 16, G = 16, B = 16, A = 255 });
    private static readonly SolidColorBrush ForegroundColor = new(Colors.LightGray);

    // **Hardcoded root path for the File Explorer (you can change this)**
    private static readonly string FileExplorerRootPath = "D:\\Parsa Stuff\\Visual Studio\\HordeRush\\HordeRush\\Res";

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
            // Use the base Resource type to determine the filter
            Filter = GetFilterForResourceType(typeof(Resource)),
            Title = $"Select a Resource for {propertyName}",
            InitialDirectory = FileExplorerRootPath // Set initial directory
        };

        if (openFileDialog.ShowDialog() == true)
        {
            string filePath = openFileDialog.FileName;

            // Check if the selected file is within the FileExplorerRootPath
            if (filePath.StartsWith(FileExplorerRootPath, StringComparison.OrdinalIgnoreCase))
            {
                // Make the path relative to the "Res" folder
                string relativePath = GetRelativePathFromRes(filePath);

                textBox.Text = relativePath;
                SetStringValue(nodePropertyValues, propertyName, relativePath);
            }
            else
            {
                // Optionally, display a message or clear the textbox if the file is not from the correct location
                MessageBox.Show("Please select a file from the project's Res directory.", "Invalid File Location", MessageBoxButton.OK, MessageBoxImage.Warning);
                textBox.Text = string.Empty; // Clear the textbox
                SetStringValue(nodePropertyValues, propertyName, string.Empty); // Clear the value in nodePropertyValues
            }
        }
    }

    // Helper method to get the path relative to the "Res" folder
    private static string GetRelativePathFromRes(string fullPath)
    {
        // Assuming FileExplorerRootPath ends with "Res" or "Res\"
        int resIndex = fullPath.IndexOf("\\Res\\", StringComparison.OrdinalIgnoreCase);
        if (resIndex >= 0)
        {
            return fullPath.Substring(resIndex + 1); // +1 to include the leading slash
        }

        return fullPath; // Should not happen if the file is selected from the correct directory
    }

    private static string GetFilterForResourceType(Type resourceType)
    {
        // Now that we have a common Resource base type, we can simplify the filter
        if (resourceType == typeof(Resource))
        {
            return "Resource Files (*.wav;*.mp3;*.ogg;*.ttf;*.otf;*.png;*.jpg;*.jpeg;*.bmp)|*.wav;*.mp3;*.ogg;*.ttf;*.otf;*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*";
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