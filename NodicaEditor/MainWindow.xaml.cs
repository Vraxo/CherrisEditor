using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using IniParser;
using IniParser.Model;
using Microsoft.Win32;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nodica;
using Raylib_cs;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;
using Button = System.Windows.Controls.Button;

namespace NodicaEditor;

public partial class MainWindow : Window
{
    private SceneHierarchyManager _sceneHierarchyManager;
    private Inspector _propertyInspector;
    private static readonly FileIniDataParser _iniParser = new();
    private string _currentFilePath;
    private string _fileExplorerRootPath;
    private string _currentFileExplorerPath;

    public MainWindow()
    {
        InitializeComponent();
        _propertyInspector = new Inspector(InspectorPanel);
        _sceneHierarchyManager = new SceneHierarchyManager(SceneHierarchyTreeView, _propertyInspector);
        SceneHierarchyTreeView.SelectedItemChanged += SceneHierarchyTreeView_SelectedItemChanged;
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, Save_Executed, Save_CanExecute));

        _currentFilePath = @"D:\Parsa Stuff\Visual Studio\HordeRush\HordeRush\Res\Scenes\Gun.ini";
        if (File.Exists(_currentFilePath))
        {
            _sceneHierarchyManager.LoadScene(_currentFilePath);
        }
        else
        {
            MessageBox.Show($"The file '{_currentFilePath}' does not exist.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // Set the root path for the file explorer
        _fileExplorerRootPath = @"D:\Parsa Stuff\Visual Studio\HordeRush\HordeRush\Res";
        _currentFileExplorerPath = _fileExplorerRootPath;
        PopulateFileExplorer(_currentFileExplorerPath);
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
            Dictionary<string, object?> propertyValues = _propertyInspector.GetPropertyValues(selectedNode);

            StringBuilder sb = new StringBuilder();
            foreach (var kvp in propertyValues)
            {
                sb.AppendLine($"{kvp.Key}: {kvp.Value}");
            }
            MessageBox.Show(sb.ToString(), "Properties to be Saved");

            SaveNodePropertiesToIni(selectedNode, _currentFilePath, propertyValues);
            MessageBox.Show($"Properties for node '{selectedNode.Name}' saved successfully.", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private static void SaveNodePropertiesToIni(Node node, string filePath, Dictionary<string, object?> propertyValues)
    {
        IniData iniData = _iniParser.ReadFile(filePath);

        string originalSectionName = node.Name;
        string newSectionName = propertyValues.ContainsKey("Name")
            ? propertyValues["Name"]?.ToString() ?? originalSectionName
            : originalSectionName;

        // Rename the section if necessary
        if (originalSectionName != newSectionName && iniData.Sections.ContainsSection(originalSectionName))
        {
            var section = iniData.Sections[originalSectionName];
            iniData.Sections.RemoveSection(originalSectionName);
            iniData.Sections.AddSection(newSectionName);

            foreach (var key in section)
            {
                iniData.Sections[newSectionName][key.KeyName] = key.Value;
            }
        }

        UpdateParentReferences(iniData, originalSectionName, newSectionName);

        foreach (var propertyValue in propertyValues)
        {
            string propertyName = RemovePrefixFromPropertyName(propertyValue.Key);

            // Skip invalid properties or "Name"
            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == "Name")
                continue;

            // Check for nested properties
            string[] pathParts = propertyValue.Key.Split('/');
            if (pathParts.Length > 1)
            {
                // Handle nested properties
                object currentObject = node;
                PropertyInfo currentProperty = null;
                bool shouldSkip = false;

                for (int i = 0; i < pathParts.Length; i++)
                {
                    currentProperty = currentObject.GetType().GetProperty(pathParts[i]);
                    if (currentProperty == null)
                        continue;

                    // Skip the exact property if marked with SaveExclude
                    if (i == pathParts.Length - 1 && IsSaveExcluded(currentProperty))
                    {
                        shouldSkip = true;
                        break;
                    }

                    // Move to the next level
                    if (i < pathParts.Length - 1)
                    {
                        currentObject = currentProperty.GetValue(currentObject);
                        if (currentObject == null)
                            break;
                    }
                }

                if (shouldSkip || currentProperty == null)
                    continue;

                object? defaultValue = DefaultValueProvider.GetDefaultValue(currentProperty, node, propertyValue.Key);

                if (AreValuesEqual(propertyValue.Value, defaultValue))
                {
                    iniData.Sections[newSectionName].RemoveKey(propertyValue.Key);
                }
                else
                {
                    iniData.Sections[newSectionName][propertyValue.Key] = ConvertPropertyValueToString(propertyValue.Value);
                }
            }
            else
            {
                // Handle non-nested properties
                PropertyInfo propertyInfo = node.GetType().GetProperty(propertyName);
                if (propertyInfo == null || IsSaveExcluded(propertyInfo))
                    continue;

                object? defaultValue = DefaultValueProvider.GetDefaultValue(propertyInfo, node);

                if (AreValuesEqual(propertyValue.Value, defaultValue))
                {
                    iniData.Sections[newSectionName].RemoveKey(propertyName);
                }
                else
                {
                    iniData.Sections[newSectionName][propertyName] = ConvertPropertyValueToString(propertyValue.Value);
                }
            }
        }

        _iniParser.WriteFile(filePath, iniData);
    }

    private static bool IsSaveExcluded(PropertyInfo propertyInfo)
    {
        return propertyInfo.GetCustomAttribute<SaveExcludeAttribute>() != null;
    }

    private static void UpdateParentReferences(IniData iniData, string oldName, string newName)
    {
        foreach (var section in iniData.Sections)
        {
            if (section.Keys.ContainsKey("parent") && section.Keys["parent"] == oldName)
            {
                section.Keys["parent"] = newName;
            }
        }
    }

    private static string RemovePrefixFromPropertyName(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentException("Property name cannot be null or empty.");

        string[] parts = propertyName.Split('/');
        return parts.Length > 1 ? string.Join("/", parts.Skip(1)) : propertyName;
    }

    private static string ConvertPropertyValueToString(object? value)
    {
        return value switch
        {
            Vector2 vector => $"({vector.X},{vector.Y})",
            bool boolean => boolean ? "true" : "false",
            Raylib_cs.Color color => $"({color.R},{color.G},{color.B},{color.A})",
            _ => value?.ToString() ?? string.Empty
        };
    }

    private static bool AreValuesEqual(object? value1, object? value2)
    {
        if (value1 is null && value2 is null) return true;
        if (value1 is null || value2 is null) return false;

        if (value1 is Vector2 v1 && value2 is Vector2 v2)
        {
            return v1.Equals(v2);
        }

        if (value1 is Raylib_cs.Color c1 && value2 is Raylib_cs.Color c2)
        {
            return c1.R == c2.R && c1.G == c2.G && c1.B == c2.B && c1.A == c2.A;
        }

        return value1.Equals(value2);
    }

    private void PopulateFileExplorer(string path)
    {
        FileExplorerItemsControl.Items.Clear();
        _currentFileExplorerPath = path;

        try
        {
            // Add a "Back" button if not in the root directory
            if (_currentFileExplorerPath != _fileExplorerRootPath)
            {
                var backButton = CreateBackButton();
                FileExplorerItemsControl.Items.Add(backButton);
            }

            // Add directories
            foreach (string dir in Directory.GetDirectories(path))
            {
                string dirName = Path.GetFileName(dir);
                var fileItem = CreateFileExplorerItem(dirName, true);
                fileItem.Tag = dir;
                fileItem.PreviewMouseLeftButtonDown += (sender, e) =>
                {
                    if (e.ClickCount == 2) // Double-click
                    {
                        PopulateFileExplorer(fileItem.Tag.ToString());
                    }
                };

                FileExplorerItemsControl.Items.Add(fileItem);
            }

            // Add files
            foreach (string file in Directory.GetFiles(path))
            {
                string fileName = Path.GetFileName(file);
                var fileItem = CreateFileExplorerItem(fileName, false);
                fileItem.Tag = file;
                fileItem.PreviewMouseLeftButtonDown += (sender, e) =>
                {
                    if (e.ClickCount == 2) // Double-click
                    {
                        OpenFile(fileItem.Tag.ToString());
                    }
                };

                FileExplorerItemsControl.Items.Add(fileItem);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error accessing path '{path}': {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private Grid CreateFileExplorerItem(string name, bool isDirectory)
    {
        // Create the main container grid
        var grid = new Grid
        {
            Width = 64,
            Height = 64,
            Margin = new Thickness(5)
        };
        grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(48) });
        grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

        // Image (icon)
        var image = new System.Windows.Controls.Image
        {
            Source = isDirectory
                ? new BitmapImage(new Uri($"D:\\Parsa Stuff\\Visual Studio\\NodicaEditor\\NodicaEditor\\bin\\Debug\\net8.0-windows\\Res\\Icons\\Folder.png", UriKind.RelativeOrAbsolute))
                : new BitmapImage(new Uri($"D:\\Parsa Stuff\\Visual Studio\\NodicaEditor\\NodicaEditor\\bin\\Debug\\net8.0-windows\\Res\\Icons\\File.png", UriKind.RelativeOrAbsolute)),
            Width = 48,
            Height = 48,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        Grid.SetRow(image, 0);
        grid.Children.Add(image);

        // TextBlock (name)
        var textBlock = new TextBlock
        {
            Text = name,
            TextAlignment = TextAlignment.Center,
            Foreground = Brushes.White,
            TextWrapping = TextWrapping.Wrap
        };
        Grid.SetRow(textBlock, 1);
        grid.Children.Add(textBlock);

        return grid;
    }

    private Button CreateBackButton()
    {
        var backButton = new Button
        {
            // Set the content to be the grid created by CreateFileExplorerItem
            Content = CreateFileExplorerItem("..", true),
            Tag = "Back",
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(5)
        };

        // Handle the click event to go back to the parent directory
        backButton.Click += (sender, e) =>
        {
            string parentDirectory = Directory.GetParent(_currentFileExplorerPath)?.FullName;
            if (parentDirectory != null)
            {
                PopulateFileExplorer(parentDirectory);
            }
        };

        return backButton;
    }

    private void OpenFile(string filePath)
    {
        if (Path.GetExtension(filePath).Equals(".ini", StringComparison.OrdinalIgnoreCase))
        {
            _currentFilePath = filePath;
            _sceneHierarchyManager.LoadScene(_currentFilePath);
        }
        else
        {
            // Handle opening other file types if needed
            MessageBox.Show($"Opening file: {filePath}", "Open File", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}