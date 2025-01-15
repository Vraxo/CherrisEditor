using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace NodicaEditor;

public class ResourceControl : StackPanel
{
    private TextBox _textBox;
    private Button _button;

    public string ResourcePath
    {
        get { return _textBox.Text; }
        set { _textBox.Text = value; }
    }

    public event EventHandler<string> ResourcePathChanged;

    public ResourceControl()
    {
        Orientation = Orientation.Horizontal;

        _textBox = new TextBox
        {
            Width = 150,
            Margin = new Thickness(0, 0, 5, 0),
            IsReadOnly = true
        };

        _button = new Button
        {
            Content = "Browse",
            Padding = new Thickness(5, 2, 5, 2)
        };
        _button.Click += Button_Click;

        Children.Add(_textBox);
        Children.Add(_button);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*"; // Customize filter as needed

        if (openFileDialog.ShowDialog() == true)
        {
            ResourcePath = openFileDialog.FileName;
            ResourcePathChanged?.Invoke(this, ResourcePath);
        }
    }
}