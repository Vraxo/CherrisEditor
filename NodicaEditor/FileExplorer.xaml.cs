using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CherrisEditor;

public partial class FileExplorer : UserControl
{
    public string RootPath { get; set; } = "";
    private string currentPath = "";

    public event Action<string>? FileOpened;

    public FileExplorer()
    {
        InitializeComponent();

        RootPath = "D:\\Parsa Stuff\\Visual Studio\\HordeRush\\HordeRush\\Res";

        if (!RootPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            RootPath += Path.DirectorySeparatorChar;
        }
    }

    public void Populate(string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        FileExplorerItemsControl.Items.Clear();
        currentPath = path;

        try
        {
            AddBackButtonIfNotRoot();
            AddDirectoriesToExplorer(path);
            AddFilesToExplorer(path);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error accessing path '{path}': {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AddBackButtonIfNotRoot()
    {
        if (currentPath != RootPath)
        {
            Button backButton = CreateBackButton();
            FileExplorerItemsControl.Items.Add(backButton);
        }
    }

    private void AddDirectoriesToExplorer(string path)
    {
        foreach (string dir in Directory.GetDirectories(path))
        {
            string dirName = Path.GetFileName(dir);
            Grid fileItem = CreateItem(dirName, true);
            fileItem.Tag = dir;
            fileItem.PreviewMouseLeftButtonDown += (sender, e) => HandleDirectoryItemClick(fileItem, e);

            FileExplorerItemsControl.Items.Add(fileItem);
        }
    }

    private void HandleDirectoryItemClick(Grid fileItem, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            string? path = fileItem.Tag.ToString();

            if (path is not null)
            {
                Populate(path);
            }
        }
    }

    private void AddFilesToExplorer(string path)
    {
        foreach (string file in Directory.GetFiles(path))
        {
            string fileName = Path.GetFileName(file);
            var fileItem = CreateItem(fileName, false);
            fileItem.Tag = file;
            fileItem.PreviewMouseLeftButtonDown += (sender, e) => HandleFileItemClick(fileItem, e);

            FileExplorerItemsControl.Items.Add(fileItem);
        }
    }

    private void HandleFileItemClick(Grid fileItem, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            string? fullPath = fileItem.Tag.ToString();
            if (fullPath is not null)
            {
                string relativePath = GetRelativePath(fullPath);
                DragDrop.DoDragDrop(fileItem, relativePath, DragDropEffects.Copy);
            }

            e.Handled = true;
        }
        else if (e.ClickCount == 2)
        {
            string? path = fileItem.Tag.ToString();
            if (path is not null)
            {
                FileOpened?.Invoke(path);
            }
        }
    }

    private string GetRelativePath(string fullPath)
    {
        if (fullPath.StartsWith(RootPath, StringComparison.OrdinalIgnoreCase))
        {
            string relativePath = fullPath.Substring(RootPath.Length).TrimStart(Path.DirectorySeparatorChar);
            return $"Res{Path.DirectorySeparatorChar}{relativePath}";
        }
        else
        {
            return fullPath;
        }
    }

    private Grid CreateItem(string name, bool isDirectory)
    {
        Grid grid = new()
        {
            Width = 64,
            Height = 64,
            Margin = new(5),
            AllowDrop = false,
            RowDefinitions =
        {
            new() { Height = new(48) },
            new() { Height = GridLength.Auto }
        },
            Background = Brushes.Transparent, // Default background
        };

        string fullPath = Path.Combine(currentPath, name);

        Image image = new()
        {
            Source = isDirectory
                ? new BitmapImage(new Uri("D:\\Parsa Stuff\\Visual Studio\\CherrisEditor\\CherrisEditor\\bin\\Debug\\net8.0-windows\\Res\\Icons\\Folder.png", UriKind.RelativeOrAbsolute))
                : GetImageSourceForFile(fullPath),
            Width = 48,
            Height = 48,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetRow(image, 0);
        grid.Children.Add(image);

        TextBlock textBlock = new()
        {
            Text = name,
            TextAlignment = TextAlignment.Center,
            Foreground = Brushes.White,
            TextWrapping = TextWrapping.Wrap,
        };

        Grid.SetRow(textBlock, 1);
        grid.Children.Add(textBlock);

        // Apply hover effect only for non-back items
        if (name != "..") // Skip back button (named "..")
        {
            grid.MouseEnter += (sender, e) =>
            {
                grid.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x2D, 0x6B, 0x99)); // Hover color
            };

            grid.MouseLeave += (sender, e) =>
            {
                grid.Background = Brushes.Transparent; // Reset background when mouse leaves
            };
        }

        return grid;
    }

    private static BitmapImage GetImageSourceForFile(string filePath)
    {
        string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".webp" };
        string fileExtension = Path.GetExtension(filePath).ToLowerInvariant();

        if (Array.Exists(imageExtensions, ext => ext.Equals(fileExtension)))
        {
            try
            {
                Debug.WriteLine($"Attempting to load image from path: {filePath}");

                if (File.Exists(filePath))
                {
                    Debug.WriteLine($"File exists: {filePath}");
                    return new BitmapImage(new Uri(filePath, UriKind.Absolute));
                }
                else
                {
                    Debug.WriteLine($"File does not exist: {filePath}");
                    return new BitmapImage(new Uri("D:\\Parsa Stuff\\Visual Studio\\CherrisEditor\\CherrisEditor\\bin\\Debug\\net8.0-windows\\Res\\Icons\\File.png", UriKind.RelativeOrAbsolute)); // fallback icon
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading image: {ex.Message}");
                return new BitmapImage(new Uri("D:\\Parsa Stuff\\Visual Studio\\CherrisEditor\\CherrisEditor\\bin\\Debug\\net8.0-windows\\Res\\Icons\\File.png", UriKind.RelativeOrAbsolute));
            }
        }

        return new BitmapImage(new Uri("D:\\Parsa Stuff\\Visual Studio\\CherrisEditor\\CherrisEditor\\bin\\Debug\\net8.0-windows\\Res\\Icons\\File.png", UriKind.RelativeOrAbsolute));
    }

    private Button CreateBackButton()
    {
        Button backButton = new()
        {
            Content = CreateItem("..", true),
            Tag = "Back",
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(5)
        };

        backButton.PreviewMouseLeftButtonDown += (sender, e) =>
        {
            if (e.ClickCount == 2)
            {
                string? parentDirectory = Directory.GetParent(currentPath)?.FullName;

                if (parentDirectory is not null)
                {
                    Populate(parentDirectory);
                }
            }
        };

        return backButton;
    }
}