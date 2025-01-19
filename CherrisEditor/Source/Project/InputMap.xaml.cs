using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using YamlDotNet.Serialization;

namespace CherrisEditor;

public partial class InputMap : UserControl
{
    private const string InputMapFilePath = @"D:\Parsa Stuff\Visual Studio\HordeRush\HordeRush\bin\Debug\net8.0\Res\Nodica\InputMap.yaml";
    private Dictionary<string, List<Dictionary<string, string>>> inputMapData;

    public InputMap()
    {
        InitializeComponent();
        LoadInputMap();
    }

    private void LoadInputMap()
    {
        try
        {
            var deserializer = new DeserializerBuilder().Build();
            string yaml = File.ReadAllText(InputMapFilePath);
            inputMapData = deserializer.Deserialize<Dictionary<string, List<Dictionary<string, string>>>>(yaml);

            InputMapPanel.Children.Clear();

            if (inputMapData != null)
            {
                foreach (var action in inputMapData)
                {
                    AddActionToUI(action.Key, action.Value);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading input map: {ex.Message}");
        }
    }

    public void SaveInputMap()
    {
        try
        {
            // Update inputMapData from the UI elements before serializing
            UpdateInputMapDataFromUI();

            var serializer = new SerializerBuilder().Build();
            string yaml = serializer.Serialize(inputMapData);

            File.WriteAllText(InputMapFilePath, yaml);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving input map: {ex.Message}");
        }
    }

    private void UpdateInputMapDataFromUI()
    {
        inputMapData.Clear();

        foreach (Expander actionExpander in InputMapPanel.Children)
        {
            string actionName = actionExpander.Header.ToString(); // The header is now a string
            List<Dictionary<string, string>> bindings = new List<Dictionary<string, string>>();

            if (actionExpander.Content is StackPanel bindingsPanel)
            {
                foreach (object child in bindingsPanel.Children)
                {
                    if (child is Grid bindingGrid)
                    {
                        if (bindingGrid.Children[0] is ComboBox typeComboBox &&
                            bindingGrid.Children[1] is TextBox keyTextBox)
                        {
                            string type = typeComboBox.SelectedItem.ToString();
                            string key = keyTextBox.Text;

                            bindings.Add(new Dictionary<string, string>
                            {
                                { "Type", type },
                                { "KeyOrButton", key }
                            });
                        }
                    }
                }
            }

            inputMapData.Add(actionName, bindings);
        }
    }

    private void AddActionToUI(string actionName, List<Dictionary<string, string>> bindings)
    {
        Expander actionExpander = new Expander();
        actionExpander.Header = actionName; // Set header directly
        actionExpander.IsExpanded = true;

        StackPanel bindingsPanel = new StackPanel();

        foreach (var binding in bindings)
        {
            Grid bindingGrid = CreateBindingGrid(binding["Type"], binding["KeyOrButton"]);
            bindingsPanel.Children.Add(bindingGrid);
        }

        Button addBindingButton = new Button { Content = "Add Binding" };
        addBindingButton.Click += (sender, e) =>
        {
            Grid newBindingGrid = CreateBindingGrid("Keyboard", ""); // Default to Keyboard
            bindingsPanel.Children.Insert(bindingsPanel.Children.Count - 1, newBindingGrid); // Insert before Add Binding button
        };

        // Apply the style to the Add Binding button
        addBindingButton.Style = (Style)FindResource("AddBindingButtonStyle");

        bindingsPanel.Children.Add(addBindingButton);
        actionExpander.Content = bindingsPanel;
        InputMapPanel.Children.Add(actionExpander);
    }

    private Grid CreateBindingGrid(string type, string keyOrButton)
    {
        Grid grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        ComboBox typeComboBox = new ComboBox();
        typeComboBox.Items.Add("Keyboard");
        typeComboBox.Items.Add("MouseButton");
        typeComboBox.SelectedItem = type;
        Grid.SetColumn(typeComboBox, 0);

        TextBox keyTextBox = new TextBox { Text = keyOrButton };
        Grid.SetColumn(keyTextBox, 1);

        grid.Children.Add(typeComboBox);
        grid.Children.Add(keyTextBox);

        return grid;
    }

    private void AddActionButton_Click(object sender, RoutedEventArgs e)
    {
        // Add a new action with a default name
        AddActionToUI("NewAction", new List<Dictionary<string, string>>());
    }

    private void DeleteAction_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.TemplatedParent is ToggleButton toggleButton)
        {
            // Traverse up to find the Expander
            DependencyObject parent = toggleButton.TemplatedParent;
            while (parent != null && !(parent is Expander))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            Expander expander = parent as Expander;
            if (expander != null && InputMapPanel.Children.Contains(expander))
            {
                // Get the action name from the Expander's Header
                string actionName = expander.Header.ToString();

                // Remove the Expander from the UI
                InputMapPanel.Children.Remove(expander);

                // Remove the action from data if it exists
                if (!string.IsNullOrEmpty(actionName) && inputMapData.ContainsKey(actionName))
                {
                    inputMapData.Remove(actionName);
                }
            }
        }
    }

    private void HeaderTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            // Update the inputMapData when the header text is changed
            UpdateInputMapDataFromUI();
        }
    }

    private void HeaderTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && sender is TextBox textBox)
        {
            // Remove focus from the TextBox when Enter is pressed
            Keyboard.ClearFocus();

            // Optionally, you can expand the current Expander here if needed
            if (textBox.TemplatedParent is ToggleButton toggleButton && toggleButton.TemplatedParent is Expander expander)
            {
                expander.IsExpanded = true;
            }

            e.Handled = true; // Prevent further handling of the Enter key
        }
    }
}