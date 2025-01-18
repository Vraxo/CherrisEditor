using System;
using System.IO;
using System.Windows;
using Nodica;
using YamlDotNet.Serialization;

namespace NodicaEditor
{
    public partial class ProjectWindow : Window
    {
        private string configFilePath = @"D:\Parsa Stuff\Visual Studio\NodicaEditor\Nodica\Res\Nodica\Config.yaml";

        public ProjectWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConfig();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();
        }

        private void LoadConfig()
        {
            if (File.Exists(configFilePath))
            {
                try
                {
                    var deserializer = new DeserializerBuilder().Build();
                    var config = deserializer.Deserialize<Configuration>(File.ReadAllText(configFilePath));

                    // Set values to UI elements
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

        private void SaveConfig()
        {
            try
            {
                var config = new Configuration
                {
                    Title = TitleTextBox.Text,
                    Width = int.Parse(WidthTextBox.Text),
                    Height = int.Parse(HeightTextBox.Text),
                    MinWidth = int.Parse(MinWidthTextBox.Text),
                    MinHeight = int.Parse(MinHeightTextBox.Text),
                    MaxWidth = int.Parse(MaxWidthTextBox.Text),
                    MaxHeight = int.Parse(MaxHeightTextBox.Text),
                    ResizableWindow = ResizableWindowCheckBox.IsChecked ?? false,
                    AntiAliasing = AntiAliasingCheckBox.IsChecked ?? false,
                    Backend = BackendTextBox.Text,
                    MainScenePath = MainScenePathTextBox.Text
                };

                var serializer = new SerializerBuilder().Build();
                var yaml = serializer.Serialize(config);

                File.WriteAllText(configFilePath, yaml);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving config: {ex.Message}");
            }
        }
    }
}