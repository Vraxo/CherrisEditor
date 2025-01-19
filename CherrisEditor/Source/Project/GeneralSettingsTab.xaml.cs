using System.IO;
using System.Windows;
using System.Windows.Controls;
using YamlDotNet.Serialization;
using Cherris;

namespace CherrisEditor;

public partial class GeneralSettingsTab : UserControl
{
    private const string configFilePath = @"D:\Parsa Stuff\Visual Studio\HordeRush\Nodica\Res\Nodica\Config.yaml";
    private readonly Configuration config;

    public GeneralSettingsTab()
    {
        InitializeComponent();

        var deserializer = new DeserializerBuilder().Build();
        string json = File.ReadAllText(configFilePath);
        config = deserializer.Deserialize<Configuration>(json);
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        LoadConfig();
    }

    public void LoadConfig()
    {
        if (File.Exists(configFilePath))
        {
            try
            {
                TitleTextBox.Text = config.Title;
                WidthTextBox.Text = config.Width.ToString();
                HeightTextBox.Text = config.Height.ToString();
                MinWidthTextBox.Text = config.MinWidth.ToString();
                MinHeightTextBox.Text = config.MinHeight.ToString();
                MaxWidthTextBox.Text = config.MaxWidth.ToString();
                MaxHeightTextBox.Text = config.MaxHeight.ToString();
                ResizableWindowCheckBox.IsChecked = config.ResizableWindow;
                AntiAliasingCheckBox.IsChecked = config.AntiAliasing;
                BackendTextBox.Text = config.Backend;
                MainScenePathTextBox.Text = config.MainScenePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading config: {ex.Message}");
            }
        }
    }

    public void SaveConfig()
    {
        try
        {
            config.Title = TitleTextBox.Text;
            config.Width = int.Parse(WidthTextBox.Text);
            config.Height = int.Parse(HeightTextBox.Text);
            config.MinWidth = int.Parse(MinWidthTextBox.Text);
            config.MinHeight = int.Parse(MinHeightTextBox.Text);
            config.MaxWidth = int.Parse(MaxWidthTextBox.Text);
            config.MaxHeight = int.Parse(MaxHeightTextBox.Text);
            config.ResizableWindow = ResizableWindowCheckBox.IsChecked ?? false;
            config.AntiAliasing = AntiAliasingCheckBox.IsChecked ?? false;
            config.Backend = BackendTextBox.Text;
            config.MainScenePath = MainScenePathTextBox.Text;

            var serializer = new SerializerBuilder().Build();
            string yaml = serializer.Serialize(config);

            File.WriteAllText(configFilePath, yaml);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving config: {ex.Message}");
        }
    }
}