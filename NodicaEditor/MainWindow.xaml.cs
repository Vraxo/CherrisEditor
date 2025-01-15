using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Nodica;

namespace NodicaEditor
{
    public partial class MainWindow : Window
    {
        private string _currentFilePath;
        private readonly IniSaver _iniSaver;

        public MainWindow()
        {
            InitializeComponent();

            SceneHierarchyControl.NodeSelected += OnNodeSelected;
            _currentFilePath = @"D:\Parsa Stuff\Visual Studio\HordeRush\HordeRush\Res\Scenes\Gun.ini";

            if (File.Exists(_currentFilePath))
            {
                SceneHierarchyControl.LoadScene(_currentFilePath);
            }
            else
            {
                MessageBox.Show($"The file '{_currentFilePath}' does not exist.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            _iniSaver = new IniSaver();
        }

        private void OnNodeSelected(Node selectedNode)
        {
            var propertyInspector = new Inspector(InspectorPanel);
            propertyInspector.DisplayNodeProperties(selectedNode);
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
                SceneHierarchyControl.LoadScene(_currentFilePath);
            }
        }
    }
}
