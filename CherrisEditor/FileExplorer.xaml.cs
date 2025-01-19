using System.Diagnostics;
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
    public string currentPath = "";

    public event Action<string>? FileOpened;

    private const string defaultIconPath = "D:\\Parsa Stuff\\Visual Studio\\CherrisEditor\\CherrisEditor\\bin\\Debug\\net8.0-windows\\Res\\Icons\\File.png";
    private const string folderIconPath = "D:\\Parsa Stuff\\Visual Studio\\CherrisEditor\\CherrisEditor\\bin\\Debug\\net8.0-windows\\Res\\Icons\\Folder.png";
    private const string fontIconPath = "D:\\Parsa Stuff\\Visual Studio\\CherrisEditor\\CherrisEditor\\bin\\Debug\\net8.0-windows\\Res\\Icons\\Font.png";
    private const string audioIconPath = "D:\\Parsa Stuff\\Visual Studio\\CherrisEditor\\CherrisEditor\\bin\\Debug\\net8.0-windows\\Res\\Icons\\Audio.png";
    private const string themeIconPath = "D:\\Parsa Stuff\\Visual Studio\\CherrisEditor\\CherrisEditor\\bin\\Debug\\net8.0-windows\\Res\\Icons\\Theme.png";
    private const string sceneIconPath = "D:\\Parsa Stuff\\Visual Studio\\CherrisEditor\\CherrisEditor\\bin\\Debug\\net8.0-windows\\Res\\Icons\\Scene.png";

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
            string relativePath = fullPath[RootPath.Length..].TrimStart(Path.DirectorySeparatorChar);
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
            Width = 72,
            Height = 82,
            Margin = new(5),
            AllowDrop = false,
            RowDefinitions =
            {
                new() { Height = new(48) },
                new() { Height = GridLength.Auto }
            },
            Background = Brushes.Transparent,
        };

        string fullPath = Path.Combine(currentPath, name);

        Image image = new()
        {
            Source = isDirectory
                ? new BitmapImage(new(folderIconPath, UriKind.Absolute))
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
            FontSize = 10,
            Margin = new(0, 2, 0, 0)
        };

        Grid.SetRow(textBlock, 1);
        grid.Children.Add(textBlock);

        if (name != "..")
        {
            grid.MouseEnter += (sender, e) =>
            {
                grid.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x2D, 0x6B, 0x99));
            };

            grid.MouseLeave += (sender, e) =>
            {
                grid.Background = Brushes.Transparent;
            };
        }

        return grid;
    }

    private static BitmapImage GetImageSourceForFile(string filePath)
    {
        string fileExtension = Path.GetExtension(filePath).ToLowerInvariant();

        string iconPath = fileExtension switch
        {
            ".ttf" => fontIconPath,
            ".mp3" => audioIconPath,
            ".ini" when Path.GetFileNameWithoutExtension(filePath).EndsWith(".theme", StringComparison.OrdinalIgnoreCase) => themeIconPath,
            ".ini" => sceneIconPath,
            _ => defaultIconPath,
        };

        string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".webp" };

        if (Array.Exists(imageExtensions, ext => ext.Equals(fileExtension)))
        {
            try
            {
                if (File.Exists(filePath))
                {
                    return new(new(filePath, UriKind.Absolute));
                }
                else
                {
                    Debug.WriteLine($"File does not exist: {filePath}");
                    return new(new(defaultIconPath, UriKind.RelativeOrAbsolute));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading image: {ex.Message}");
                return new(new(defaultIconPath, UriKind.RelativeOrAbsolute));
            }
        }

        return new(new(iconPath, UriKind.RelativeOrAbsolute));
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
            if (e.ClickCount != 2)
            {
                return;
            }

            string? parentDirectory = Directory.GetParent(currentPath)?.FullName;

            if (parentDirectory is not null)
            {
                Populate(parentDirectory);
            }
        };

        return backButton;
    }
}