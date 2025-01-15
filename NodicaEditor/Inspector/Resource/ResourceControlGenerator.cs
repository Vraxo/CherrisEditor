using System;
using System.Reflection;
using System.Windows;
using Nodica;

namespace NodicaEditor;

public class ResourceControlGenerator
{
    private const string ResourcePathSuffix = "_ResourcePath";

    public static ResourceControl CreateResourceControl(Node node, PropertyInfo property, string fullPath,
        Dictionary<string, object?> nodePropertyValues)
    {
        string propertyName = fullPath != "" ? fullPath : property.Name;
        string resourcePathKey = propertyName + ResourcePathSuffix;

        // Get the initial path from the dictionary (if it exists)
        string initialPath = GetStringValue(nodePropertyValues, resourcePathKey) ?? "";

        ResourceControl resourceControl = new ResourceControl
        {
            ResourcePath = initialPath
        };

        resourceControl.ResourcePathChanged += (sender, path) =>
        {
            OnResourcePathChanged(resourceControl, node, property, nodePropertyValues, propertyName, path);
        };

        return resourceControl;
    }

    private static void OnResourcePathChanged(ResourceControl resourceControl, Node node, PropertyInfo property,
        Dictionary<string, object?> nodePropertyValues, string propertyName, string newPath)
    {
        string resourcePathKey = propertyName + ResourcePathSuffix;

        // Store the path in the dictionary
        SetStringValue(nodePropertyValues, resourcePathKey, newPath);

        // Attempt to load the resource and update the associated property
        try
        {
            if (property.PropertyType == typeof(Texture))
            {
                Texture newTexture = LoadTexture(newPath); // Implement LoadTexture as needed
                property.SetValue(node, newTexture);
                if (node is Sprite sprite)
                {
                    sprite.Size = newTexture.Size;
                }

                // Store the loaded texture in the dictionary (so we don't have to reload it every time)
                nodePropertyValues[propertyName] = newTexture;
            }
            // Add other resource types if needed in the future
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading resource: {ex.Message}", "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
            resourceControl.ResourcePath = ""; // Reset the path on error
            SetStringValue(nodePropertyValues, resourcePathKey, "");
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

    // Placeholder for texture loading logic (replace with your actual implementation)
    private static Texture LoadTexture(string path)
    {
        // Your actual texture loading logic goes here
        // ... (e.g., using Raylib or another library) ...

        // Return a dummy texture for now (or throw an exception if loading fails)
        return new Texture();
    }
}