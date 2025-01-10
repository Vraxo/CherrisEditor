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
        _currentFilePath = @"D:\Parsa Stuff\Visual Studio\HordeRush\HordeRush\Res\Scenes\Gun.ini";
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
            SaveNodePropertiesToIni(selectedNode, _currentFilePath);
            MessageBox.Show($"Properties for node '{selectedNode.Name}' saved successfully.", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void SaveNodePropertiesToIni(Node node, string filePath)
    {
        IniData iniData = _iniParser.ReadFile(filePath);

        string originalSectionName = node.Name;
        Dictionary<string, object?> propertyValues = _propertyInspector.GetPropertyValues(node);
        string newSectionName = propertyValues.ContainsKey("Name")
            ? propertyValues["Name"]?.ToString() ?? originalSectionName
            : originalSectionName;

        // Preserve the type key from the original section
        string originalType = iniData.Sections.ContainsSection(originalSectionName) &&
                              iniData.Sections[originalSectionName].ContainsKey("type")
                              ? iniData.Sections[originalSectionName]["type"]
                              : "";

        if (originalSectionName != newSectionName)
        {
            UpdateParentReferences(iniData, originalSectionName, newSectionName);
            iniData.Sections.RemoveSection(originalSectionName);
        }

        // Get or create the new section
        if (!iniData.Sections.ContainsSection(newSectionName))
        {
            iniData.Sections.AddSection(newSectionName);
        }

        KeyDataCollection sectionKeys = iniData.Sections[newSectionName];

        // Always ensure the 'type' key retains its value
        if (!string.IsNullOrWhiteSpace(originalType))
        {
            sectionKeys["type"] = originalType;
        }

        foreach (var propertyValue in propertyValues)
        {
            if (propertyValue.Key == "Name") continue;

            PropertyInfo property = node.GetType().GetProperty(propertyValue.Key);
            if (property != null && !property.IsDefined(typeof(InspectorExcludeAttribute), false))
            {
                object? defaultValue = PropertyInspector.GetDefaultValue(property, node);

                if (AreValuesEqual(propertyValue.Value, defaultValue))
                {
                    // Only remove keys other than 'type' if they match the default value
                    if (sectionKeys.ContainsKey(propertyValue.Key) && propertyValue.Key != "type")
                    {
                        sectionKeys.RemoveKey(propertyValue.Key);
                    }
                }
                else
                {
                    string valueString = ConvertPropertyValueToString(propertyValue.Value);
                    // Update or add the key, but never overwrite 'type'
                    if (propertyValue.Key == "type")
                    {
                        sectionKeys["type"] = originalType; // Ensure 'type' keeps its original value
                    }
                    else
                    {
                        sectionKeys[propertyValue.Key] = valueString;
                    }
                }
            }
        }

        // Write back the updated INI data
        _iniParser.WriteFile(filePath, iniData);
    }



    private void UpdateParentReferences(IniData iniData, string oldName, string newName)
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

        return value1.Equals(value2);
    }

    private string ConvertPropertyValueToString(object? value)
    {
        if (value is Vector2 vector)
        {
            return $"({vector.X},{vector.Y})";
        }
        if (value is bool boolean)
        {
            return boolean ? "true" : "false";
        }

        return value?.ToString() ?? string.Empty;
    }
}