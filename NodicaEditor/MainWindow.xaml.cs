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
        private Inspector _propertyInspector;
        private string _currentFilePath;
        private readonly IniSaver _iniSaver;

        public MainWindow()
        {
            InitializeComponent();
            _propertyInspector = InspectorControl;
            SceneHierarchyControl.PropertyChanged += SceneHierarchyControl_PropertyChanged;

            // Add KeyDown event handler
            this.KeyDown += Window_KeyDown;

            _currentFilePath = @"D:\Parsa Stuff\Visual Studio\HordeRush\HordeRush\Res\Scenes\Gun.ini";
            if (File.Exists(_currentFilePath))
            {
                SceneHierarchyControl.LoadScene(_currentFilePath);
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

            // CommandBinding for Save
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, Save_Executed, Save_CanExecute));
        }

        public void OpenIniFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "INI files (*.ini)|*.ini|All files (*.*)|*.*";
            openFileDialog.Title = "Open INI File";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                SceneHierarchyControl.LoadScene(filePath);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                Save_Executed(this, null);
                e.Handled = true;
            }
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SceneHierarchyControl.CurrentNode != null;
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Node selectedNode = SceneHierarchyControl.CurrentNode;
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

        private void SceneHierarchyControl_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SceneHierarchy.CurrentNode))
            {
                Node selectedNode = SceneHierarchyControl.CurrentNode;
                if (selectedNode != null)
                {
                    _propertyInspector.DisplayNodeProperties(selectedNode);
                }
            }
        }

        private void FileExplorerControl_FileOpened(string filePath)
        {
            if (Path.GetExtension(filePath).Equals(".ini", StringComparison.OrdinalIgnoreCase))
            {
                _currentFilePath = filePath;
                SceneHierarchyControl.LoadScene(_currentFilePath);
            }
            else
            {
                MessageBox.Show($"Opening file: {filePath}", "Open File", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}