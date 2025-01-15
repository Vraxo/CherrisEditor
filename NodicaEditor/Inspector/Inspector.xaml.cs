using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Nodica;
using Raylib_cs;

namespace NodicaEditor
{
    public partial class Inspector : UserControl
    {
        private Node _currentNode;

        public Inspector()
        {
            InitializeComponent();
        }

        public void DisplayNodeProperties(Node node)
        {
            _currentNode = node;

            // Clear existing children from InspectorPanel in XAML
            (Content as FrameworkElement)?.FindName("InspectorPanel");
            if (InspectorPanel != null)
            {
                InspectorPanel.Children.Clear();
            }
            else
            {
                // Handle the case where InspectorPanel is not found
                // You might want to log an error or show a message to the user
            }

            // Create the header label
            System.Windows.Controls.Label headerLabel = new System.Windows.Controls.Label
            {
                Content = node.Name,
                Foreground = Brushes.White,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            };
            InspectorPanel.Children.Add(headerLabel); // Add to InspectorPanel in XAML

            // Rest of the logic for displaying properties, same as before...
            Type nodeType = node.GetType();
            PropertyInfo[] properties = nodeType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                if (property.Name == "Name" || property.Name == "Parent" || property.Name == "Children" || property.Name == "Version") continue;
                if (property.GetCustomAttribute<SaveExcludeAttribute>() != null) continue;

                // Get property value with node as target object
                object propertyValue = property.GetValue(node);

                string propertyName = property.Name;

                // Check if this is a nested property (like Position/X)
                PropertyInfo nestedProperty = nodeType.GetProperty(propertyName);
                if (nestedProperty != null && nestedProperty.PropertyType.IsValueType && !nestedProperty.PropertyType.IsPrimitive)
                {
                    // Handle nested value types (like Vector2)
                    object nestedValue = nestedProperty.GetValue(node);
                    Type nestedType = nestedValue.GetType();
                    PropertyInfo[] nestedProperties = nestedType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    foreach (PropertyInfo subProperty in nestedProperties)
                    {
                        // Get sub-property value with nestedValue as target object
                        object subPropertyValue = subProperty.GetValue(nestedValue);
                        AddPropertyControl(node, $"{propertyName}/{subProperty.Name}", subPropertyValue);
                    }
                }
                else
                {
                    // Handle non-nested properties
                    AddPropertyControl(node, propertyName, propertyValue);
                }
            }
        }

        public Dictionary<string, object?> GetPropertyValues(Node node)
        {
            Dictionary<string, object?> propertyValues = new Dictionary<string, object?>();

            foreach (FrameworkElement element in InspectorPanel.Children)
            {
                if (element.Tag is string propertyName)
                {
                    if (element is System.Windows.Controls.TextBox textBox)
                    {
                        propertyValues[propertyName] = textBox.Text;
                    }
                    else if (element is System.Windows.Controls.CheckBox checkBox)
                    {
                        propertyValues[propertyName] = checkBox.IsChecked;
                    }
                    else if (element is System.Windows.Controls.ComboBox comboBox)
                    {
                        propertyValues[propertyName] = comboBox.SelectedItem;
                    }
                    else if (element is System.Windows.Controls.StackPanel stackPanel)
                    {
                        if (stackPanel.Children.Count == 3 &&
                            stackPanel.Children[0] is System.Windows.Controls.TextBox textBoxR &&
                            stackPanel.Children[1] is System.Windows.Controls.TextBox textBoxG &&
                            stackPanel.Children[2] is System.Windows.Controls.TextBox textBoxB)
                        {
                            byte r, g, b;
                            if (byte.TryParse(textBoxR.Text, out r) &&
                                byte.TryParse(textBoxG.Text, out g) &&
                                byte.TryParse(textBoxB.Text, out b))
                            {
                                propertyValues[propertyName] = new Raylib_cs.Color(r, g, b, (byte)255);
                            }
                            else
                            {
                                propertyValues[propertyName] = null; // Or some default color
                            }
                        }
                    }
                }
            }

            return propertyValues;
        }

        private void AddPropertyControl(Node node, string propertyName, object propertyValue)
        {
            if (propertyValue == null)
            {
                // Handle null values differently if needed
                AddLabel(propertyName, "null");
            }
            else if (propertyValue is bool boolValue)
            {
                AddCheckBox(propertyName, boolValue);
            }
            else if (propertyValue is string || propertyValue.GetType().IsPrimitive)
            {
                AddTextBox(node, propertyName, propertyValue.ToString());
            }
            else if (propertyValue is Raylib_cs.Color colorValue)
            {
                AddColorPicker(propertyName, colorValue);
            }
            else if (propertyValue is Enum)
            {
                AddComboBox(propertyName, propertyValue.GetType(), propertyValue);
            }
            else
            {
                // Fallback for other types - just display the value as text
                AddLabel(propertyName, propertyValue.ToString());
            }
        }

        private void AddLabel(string propertyName, string value)
        {
            System.Windows.Controls.Label label = new System.Windows.Controls.Label
            {
                Content = $"{propertyName}: {value}",
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 5)
            };
            InspectorPanel.Children.Add(label);
        }

        private void AddTextBox(Node node, string propertyName, string initialValue)
        {
            System.Windows.Controls.Label label = new System.Windows.Controls.Label
            {
                Content = propertyName,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 2)
            };
            InspectorPanel.Children.Add(label);

            System.Windows.Controls.TextBox textBox = new System.Windows.Controls.TextBox
            {
                Text = initialValue,
                Margin = new Thickness(0, 0, 0, 5),
                Tag = propertyName
            };
            textBox.TextChanged += (sender, e) =>
            {
                string[] parts = propertyName.Split('/');
                if (parts.Length > 1)
                {
                    // Handle nested properties
                    PropertyInfo nestedPropertyInfo = node.GetType().GetProperty(parts[0]);
                    if (nestedPropertyInfo == null) return;

                    object nestedPropertyValue = nestedPropertyInfo.GetValue(node);
                    if (nestedPropertyValue == null) return;

                    PropertyInfo subPropertyInfo = nestedPropertyInfo.PropertyType.GetProperty(parts[1]);
                    if (subPropertyInfo == null) return;

                    // Try to set the value, with type checking
                    try
                    {
                        // **Change here:** Use Convert.ChangeType for more robust type conversion
                        object convertedValue = Convert.ChangeType(textBox.Text, subPropertyInfo.PropertyType);
                        subPropertyInfo.SetValue(nestedPropertyValue, convertedValue);

                        // Finally, set the updated nested object back to the main object
                        nestedPropertyInfo.SetValue(node, nestedPropertyValue);
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions like type conversion errors
                        MessageBox.Show($"Error setting property: {ex.Message}");
                    }
                }
                else
                {
                    // Handle non-nested properties
                    PropertyInfo propertyInfo = node.GetType().GetProperty(propertyName);
                    if (propertyInfo == null) return;

                    // Try to set the value, with type checking
                    try
                    {
                        // **Change here:** Use Convert.ChangeType for more robust type conversion
                        object convertedValue = Convert.ChangeType(textBox.Text, propertyInfo.PropertyType);
                        propertyInfo.SetValue(node, convertedValue);
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions like type conversion errors
                        MessageBox.Show($"Error setting property: {ex.Message}");
                    }
                }
            };

            InspectorPanel.Children.Add(textBox);
        }

        private void AddCheckBox(string propertyName, bool initialValue)
        {
            System.Windows.Controls.CheckBox checkBox = new System.Windows.Controls.CheckBox
            {
                Content = propertyName,
                IsChecked = initialValue,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 5),
                Tag = propertyName
            };
            InspectorPanel.Children.Add(checkBox);
        }

        private void AddColorPicker(string propertyName, Raylib_cs.Color initialValue)
        {
            System.Windows.Controls.Label label = new System.Windows.Controls.Label
            {
                Content = propertyName,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 2)
            };
            InspectorPanel.Children.Add(label);

            System.Windows.Controls.StackPanel colorPanel = new System.Windows.Controls.StackPanel
            {
                Orientation = Orientation.Horizontal,
                Tag = propertyName
            };

            System.Windows.Controls.TextBox textBoxR = new System.Windows.Controls.TextBox { Width = 30, Text = initialValue.R.ToString() };
            System.Windows.Controls.TextBox textBoxG = new System.Windows.Controls.TextBox { Width = 30, Text = initialValue.G.ToString() };
            System.Windows.Controls.TextBox textBoxB = new System.Windows.Controls.TextBox { Width = 30, Text = initialValue.B.ToString() };

            colorPanel.Children.Add(textBoxR);
            colorPanel.Children.Add(textBoxG);
            colorPanel.Children.Add(textBoxB);

            InspectorPanel.Children.Add(colorPanel);
        }

        private void AddComboBox(string propertyName, Type enumType, object initialValue)
        {
            System.Windows.Controls.Label label = new System.Windows.Controls.Label
            {
                Content = propertyName,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 2)
            };
            InspectorPanel.Children.Add(label);

            System.Windows.Controls.ComboBox comboBox = new System.Windows.Controls.ComboBox
            {
                ItemsSource = Enum.GetValues(enumType),
                SelectedItem = initialValue,
                Margin = new Thickness(0, 0, 0, 5),
                Tag = propertyName
            };
            InspectorPanel.Children.Add(comboBox);
        }
    }
}