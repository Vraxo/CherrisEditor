using System.Windows;

namespace CherrisEditor;

public partial class ProjectWindow : Window
{
    public ProjectWindow()
    {
        InitializeComponent();
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        GeneralTab.SaveConfig();
        // Call SaveInputMap() on the InputMapTab instance
        InputMapTab.SaveInputMap();
    }
}