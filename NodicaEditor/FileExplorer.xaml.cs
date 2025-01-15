using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NodicaEditor;

public partial class FileExplorer : UserControl
{
    public string RootPath { get; set; }
    private string _currentPath;

    public event Action<string> FileOpened; // Event to notify about opened files

    public FileExplorer()
    {
        InitializeComponent();

        // Initialize with the path you want to start with
        RootPath = "D:\\Parsa Stuff\\Visual Studio\\HordeRush\\HordeRush\\Res";
        Populate(RootPath);
    }

    public void Populate(string path)
    {
        FileExplorerItemsControl.Items.Clear();
        _currentPath = path;

        try
        {
            // Only add a "Back" button if we're not at the root directory
            if (_currentPath != RootPath)
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
                        Populate(fileItem.Tag.ToString());
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
                        FileOpened?.Invoke(fileItem.Tag.ToString()); // Raise event
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

        // Use PreviewMouseLeftButtonDown and check for double-click
        backButton.PreviewMouseLeftButtonDown += (sender, e) =>
        {
            if (e.ClickCount == 2)
            {
                string parentDirectory = Directory.GetParent(_currentPath)?.FullName;
                if (parentDirectory != null)
                {
                    Populate(parentDirectory);
                }
            }
        };

        return backButton;
    }
}
