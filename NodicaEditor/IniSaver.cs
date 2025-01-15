using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using IniParser;
using IniParser.Model;
using Nodica;
using Raylib_cs;

namespace NodicaEditor
{
    public class IniSaver
    {
        private static readonly FileIniDataParser _iniParser = new();

        public void SaveNodePropertiesToIni(Node node, string filePath, Dictionary<string, object?> propertyValues)
        {
            IniData iniData = _iniParser.ReadFile(filePath);

            string originalSectionName = node.Name;
            string newSectionName = propertyValues.ContainsKey("Name")
                ? propertyValues["Name"]?.ToString() ?? originalSectionName
                : originalSectionName;

            // Rename the section if necessary
            if (originalSectionName != newSectionName && iniData.Sections.ContainsSection(originalSectionName))
            {
                var section = iniData.Sections[originalSectionName];
                iniData.Sections.RemoveSection(originalSectionName);
                iniData.Sections.AddSection(newSectionName);

                foreach (var key in section)
                {
                    iniData.Sections[newSectionName][key.KeyName] = key.Value;
                }
            }

            UpdateParentReferences(iniData, originalSectionName, newSectionName);

            foreach (var propertyValue in propertyValues)
            {
                string propertyName = RemovePrefixFromPropertyName(propertyValue.Key);

                // Skip invalid properties or "Name"
                if (string.IsNullOrWhiteSpace(propertyName) || propertyName == "Name")
                    continue;

                // Check for nested properties
                string[] pathParts = propertyValue.Key.Split('/');
                if (pathParts.Length > 1)
                {
                    // Handle nested properties
                    object currentObject = node;
                    PropertyInfo currentProperty = null;
                    bool shouldSkip = false;

                    for (int i = 0; i < pathParts.Length; i++)
                    {
                        currentProperty = currentObject.GetType().GetProperty(pathParts[i]);
                        if (currentProperty == null)
                            continue;

                        // Skip the exact property if marked with SaveExclude
                        if (i == pathParts.Length - 1 && IsSaveExcluded(currentProperty))
                        {
                            shouldSkip = true;
                            break;
                        }

                        // Move to the next level
                        if (i < pathParts.Length - 1)
                        {
                            currentObject = currentProperty.GetValue(currentObject);
                            if (currentObject == null)
                                break;
                        }
                    }

                    if (shouldSkip || currentProperty == null)
                        continue;

                    object? defaultValue = DefaultValueProvider.GetDefaultValue(currentProperty, node, propertyValue.Key);

                    if (AreValuesEqual(propertyValue.Value, defaultValue))
                    {
                        iniData.Sections[newSectionName].RemoveKey(propertyValue.Key);
                    }
                    else
                    {
                        iniData.Sections[newSectionName][propertyValue.Key] = ConvertPropertyValueToString(propertyValue.Value);
                    }
                }
                else
                {
                    // Handle non-nested properties
                    PropertyInfo propertyInfo = node.GetType().GetProperty(propertyName);
                    if (propertyInfo == null || IsSaveExcluded(propertyInfo))
                        continue;

                    object? defaultValue = DefaultValueProvider.GetDefaultValue(propertyInfo, node);

                    if (AreValuesEqual(propertyValue.Value, defaultValue))
                    {
                        iniData.Sections[newSectionName].RemoveKey(propertyName);
                    }
                    else
                    {
                        iniData.Sections[newSectionName][propertyName] = ConvertPropertyValueToString(propertyValue.Value);
                    }
                }
            }

            _iniParser.WriteFile(filePath, iniData);
        }

        private static bool IsSaveExcluded(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttribute<SaveExcludeAttribute>() != null;
        }

        private static void UpdateParentReferences(IniData iniData, string oldName, string newName)
        {
            foreach (var section in iniData.Sections)
            {
                if (section.Keys.ContainsKey("parent") && section.Keys["parent"] == oldName)
                {
                    section.Keys["parent"] = newName;
                }
            }
        }

        private static string RemovePrefixFromPropertyName(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException("Property name cannot be null or empty.");

            string[] parts = propertyName.Split('/');
            return parts.Length > 1 ? string.Join("/", parts.Skip(1)) : propertyName;
        }

        private static string ConvertPropertyValueToString(object? value)
        {
            return value switch
            {
                Vector2 vector => $"({vector.X},{vector.Y})",
                bool boolean => boolean ? "true" : "false",
                Raylib_cs.Color color => $"({color.R},{color.G},{color.B},{color.A})",
                _ => value?.ToString() ?? string.Empty
            };
        }

        private static bool AreValuesEqual(object? value1, object? value2)
        {
            if (value1 is null && value2 is null) return true;
            if (value1 is null || value2 is null) return false;

            if (value1 is Vector2 v1 && value2 is Vector2 v2)
            {
                return v1.Equals(v2);
            }

            if (value1 is Raylib_cs.Color c1 && value2 is Raylib_cs.Color c2)
            {
                return c1.R == c2.R && c1.G == c2.G && c1.B == c2.B && c1.A == c2.A;
            }

            return value1.Equals(value2);
        }
    }
}