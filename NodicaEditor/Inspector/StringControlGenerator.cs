using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Nodica;

namespace NodicaEditor
{
    public class StringControlGenerator
    {
        private static readonly SolidColorBrush BackgroundColor = new SolidColorBrush(Color.FromArgb(255, 16, 16, 16));
        private static readonly SolidColorBrush ForegroundColor = new SolidColorBrush(Colors.LightGray);

        public static TextBox CreateStringControl(Node node, PropertyInfo property, string fullPath, Dictionary<string, object?> nodePropertyValues)
        {
            string propertyName = fullPath != "" ? fullPath : property.Name;
            TextBox textBox = new TextBox
            {
                Text = GetStringValue(nodePropertyValues, propertyName) ?? "",
                Width = 100,
                Height = 22,
                Background = BackgroundColor,
                Foreground = ForegroundColor,
                BorderBrush = BackgroundColor,
                Style = null,
                AllowDrop = true
            };

            textBox.DragEnter += (sender, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.Text))
                    e.Effects = DragDropEffects.Copy;
                else
                    e.Effects = DragDropEffects.None;
            };

            textBox.Drop += (sender, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.Text))
                {
                    string filePath = (string)e.Data.GetData(DataFormats.Text);
                    textBox.Text = filePath;
                    SetStringValue(nodePropertyValues, propertyName, filePath);
                }
            };

            textBox.TextChanged += (sender, _) =>
            {
                if (sender is TextBox tb)
                {
                    OnStringTextChanged(tb, nodePropertyValues, propertyName);
                }
            };

            return textBox;
        }

        private static void OnStringTextChanged(TextBox tb, Dictionary<string, object?> nodePropertyValues, string propertyName)
        {
            SetStringValue(nodePropertyValues, propertyName, tb.Text);
        }

        private static string? GetStringValue(Dictionary<string, object?> propertyValues, string propertyName)
        {
            return propertyValues.TryGetValue(propertyName, out object? value) && value is string str
                ? str
                : null;
        }

        private static void SetStringValue(Dictionary<string, object?> propertyValues, string propertyName, string? value)
        {
            propertyValues[propertyName] = value;
        }
    }
}