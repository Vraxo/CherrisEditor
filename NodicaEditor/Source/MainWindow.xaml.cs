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

namespace NodicaEditor;

public partial class MainWindow : Window
{
    private SceneHierarchyManager _sceneHierarchyManager;
    private Inspector _propertyInspector;
    private string _currentFilePath;
    private readonly IniSaver _iniSaver;

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

        // Set up the File Explorer
        FileExplorerControl.RootPath = @"D:\Parsa Stuff\Visual Studio\HordeRush\HordeRush\Res";
        FileExplorerControl.Populate(FileExplorerControl.RootPath);
        FileExplorerControl.FileOpened += FileExplorerControl_FileOpened;

        _iniSaver = new IniSaver();
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

            _iniSaver.SaveNodePropertiesToIni(selectedNode, _currentFilePath, propertyValues);
            MessageBox.Show($"Properties for node '{selectedNode.Name}' saved successfully.", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void FileExplorerControl_FileOpened(string filePath)
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