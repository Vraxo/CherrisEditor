using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using IniParser;
using Microsoft.Win32;
using Nodica;
using IniParser.Model;
using System.Reflection;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Raylib_cs;
using Color = Raylib_cs.Color;
using System.Text;

namespace NodicaEditor;

public partial class MainWindow : Window
{
    private SceneHierarchyManager _sceneHierarchyManager;
    private PropertyInspector _propertyInspector;
    private static readonly FileIniDataParser _iniParser = new();
    private string _currentFilePath;

    public MainWindow()
    {
        InitializeComponent();
        _propertyInspector = new PropertyInspector(InspectorPanel);
        _sceneHierarchyManager = new SceneHierarchyManager(SceneHierarchyTreeView, _propertyInspector);
        SceneHierarchyTreeView.SelectedItemChanged += SceneHierarchyTreeView_SelectedItemChanged;
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, Save_Executed, Save_CanExecute));

        // Automatically load the specified INI file
        _currentFilePath = @"D:\Parsa Stuff\Visual Studio\HordeRush\HordeRush\Res\Scenes\Gun.ini"; // Replace with your desired path
        if (File.Exists(_currentFilePath))
        {
            _sceneHierarchyManager.LoadScene(_currentFilePath);
        }
        else
        {
            MessageBox.Show($"The file '{_currentFilePath}' does not exist.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void OpenIniFile_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new()
        {
            Filter = "INI files (*.ini)|*.ini|All files (*.*)|*.*",
            Title = "Open INI File"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            _currentFilePath = openFileDialog.FileName;
            _sceneHierarchyManager.LoadScene(_currentFilePath);
        }
    }

    private void SceneHierarchyTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is TreeViewItem selectedItem && selectedItem.Tag is Node selectedNode)
        {
            _propertyInspector.DisplayNodeProperties(selectedNode);
        }
    }

    private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = _sceneHierarchyManager.CurrentNode != null;
    }

    private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        Node selectedNode = _sceneHierarchyManager.CurrentNode;
        if (selectedNode != null && _currentFilePath != null)
        {
            // Get property values from the inspector
            Dictionary<string, object?> propertyValues = _propertyInspector.GetPropertyValues(selectedNode);

            // Display the dictionary in a dialog (for debugging)
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in propertyValues)
            {
                sb.AppendLine($"{kvp.Key}: {kvp.Value}");
            }
            MessageBox.Show(sb.ToString(), "Properties to be Saved");

            // Save the properties to the INI file
            SaveNodePropertiesToIni(selectedNode, _currentFilePath, propertyValues);
            MessageBox.Show($"Properties for node '{selectedNode.Name}' saved successfully.", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private static void SaveNodePropertiesToIni(Node node, string filePath, Dictionary<string, object?> propertyValues)
    {
        IniData iniData = _iniParser.ReadFile(filePath);

        // 1. Get the original section name (current node name)
        string originalSectionName = node.Name;

        // 2. Get the new section name (potentially modified node name)
        string newSectionName = propertyValues.ContainsKey("Name")
            ? propertyValues["Name"]?.ToString() ?? originalSectionName
            : originalSectionName;

        // 3. Get the original type (preserve type)
        string originalType = "";
        if (iniData.Sections.ContainsSection(originalSectionName) && iniData.Sections[originalSectionName].ContainsKey("type"))
        {
            originalType = iniData.Sections[originalSectionName]["type"];
        }

        // 4. Update Parent References (if name changed)
        if (originalSectionName != newSectionName)
        {
            UpdateParentReferences(iniData, originalSectionName, newSectionName);
        }

        // 5. Remove the old section if the name has changed
        if (originalSectionName != newSectionName && iniData.Sections.ContainsSection(originalSectionName))
        {
            iniData.Sections.RemoveSection(originalSectionName);
        }

        // 6. Get or create the section with the new name
        KeyDataCollection sectionKeys;
        if (!iniData.Sections.ContainsSection(newSectionName))
        {
            sectionKeys = new KeyDataCollection();
            iniData.Sections.AddSection(newSectionName);
            iniData.Sections[newSectionName].Merge(sectionKeys);
        }
        else
        {
            sectionKeys = iniData.Sections[newSectionName];
        }

        // 7. Preserve the type key - Always ensure 'type' is present and correct
        sectionKeys["type"] = originalType;

        // 8. Iterate through the property values (including nested properties)
        foreach (var propertyValue in propertyValues)
        {
            // Remove Prefix:
            string propertyNameWithoutPrefix = RemovePrefixFromPropertyName(propertyValue.Key);

            if (propertyNameWithoutPrefix == "Name" || propertyNameWithoutPrefix == "type") continue; // Skip Name and type

            // Check if the property should be excluded from saving
            string[] parts = propertyNameWithoutPrefix.Split('/'); // Use the unprefixed name for lookup
            PropertyInfo? propertyInfo = null;
            object? targetObject = node;
            Type? targetType = node.GetType();

            for (int i = 0; i < parts.Length; i++)
            {
                if (targetType == null) break;

                propertyInfo = targetType.GetProperty(parts[i]);
                if (propertyInfo == null) break;

                if (i < parts.Length - 1)
                {
                    targetObject = propertyInfo.GetValue(targetObject);
                    targetType = targetObject?.GetType();
                }
            }

            if (propertyInfo != null && !propertyInfo.IsDefined(typeof(SaveExcludeAttribute), false))
            {
                // If this is a nested property, ensure we get the correct default value
                object? defaultValue = targetObject != null ? GetDefaultValueForNestedProperty(propertyInfo, targetObject) : null;

                if (AreValuesEqual(propertyValue.Value, defaultValue))
                {
                    // Remove the key if it's the same as the default value
                    if (sectionKeys.ContainsKey(propertyNameWithoutPrefix))
                    {
                        sectionKeys.RemoveKey(propertyNameWithoutPrefix);
                    }
                }
                else
                {
                    string valueString = ConvertPropertyValueToString(propertyValue.Value);
                    // Update or add the key using the UNPREFIXED name:
                    sectionKeys[propertyNameWithoutPrefix] = valueString;
                }
            }
        }

        // 9. Write the modified INI data back to the file
        _iniParser.WriteFile(filePath, iniData);
    }

    // Helper function to remove the prefix

    private static string RemovePrefixFromPropertyName(string propertyName)
    {
        string[] parts = propertyName.Split('/');
        return string.Join("/", parts.Skip(1)); // Skip the first part (e.g., "Node2D")
    }

    private static object? GetDefaultValueForNestedProperty(PropertyInfo property, object targetObject)
    {
        // Get the default value of the property from a new instance of the target object's type
        return property.GetValue(Activator.CreateInstance(targetObject.GetType()));
    }

    private static void UpdateParentReferences(IniData iniData, string oldName, string newName)
    {
        foreach (var section in iniData.Sections)
        {
            if (section.Keys.ContainsKey("parent"))
            {
                if (section.Keys["parent"] == oldName)
                {
                    section.Keys["parent"] = newName;
                }
            }
        }
    }

    private static bool AreValuesEqual(object? value1, object? value2)
    {
        if (value1 is null && value2 is null) return true;
        if (value1 is null || value2 is null) return false;

        if (value1 is Vector2 v1 && value2 is Vector2 v2)
        {
            return v1.Equals(v2);
        }

        if (value1 is Color c1 && value2 is Color c2)
        {
            return c1.R == c2.R && c1.G == c2.G && c1.B == c2.B && c1.A == c2.A;
        }

        return value1.Equals(value2);
    }

    private static string ConvertPropertyValueToString(object? value)
    {
        return value switch
        {
            Vector2 vector => $"({vector.X},{vector.Y})",
            bool boolean => boolean ? "true" : "false",
            Color color => $"({color.R},{color.G},{color.B},{color.A})",
            _ => value?.ToString() ?? string.Empty
        };
    }
}