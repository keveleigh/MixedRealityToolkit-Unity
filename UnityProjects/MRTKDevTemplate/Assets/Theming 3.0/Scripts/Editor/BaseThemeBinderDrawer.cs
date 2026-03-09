// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MixedReality.Toolkit.Theming.Editor
{
    [CustomPropertyDrawer(typeof(BaseThemeBinder<,>), true)]
    public class BaseThemeBinderDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label.text = label.text.Replace("Element", "Binder");
            label = EditorGUI.BeginProperty(position, label, property);

            EditorGUI.PropertyField(position, property, label, true);

            string parentPropertyPath = property.propertyPath.Substring(0, property.propertyPath.LastIndexOf(property.propertyPath.Contains(".Array.data[") ? ".Array.data[" : "."));
            SerializedProperty parentProperty = property.serializedObject.FindProperty(parentPropertyPath);
            SerializedProperty themeDataSourceProperty = parentProperty.serializedObject.FindProperty("themeDataSource");

            if (property.managedReferenceValue != null && themeDataSourceProperty != null && themeDataSourceProperty.objectReferenceValue != null)
            {
                SerializedProperty themeDefinitionItemName = property.FindPropertyRelative(InspectorUIUtility.GetBackingField("ThemeDefinitionItemName"));
                SerializedProperty themeDefinitionProperty = new SerializedObject(themeDataSourceProperty.objectReferenceValue).FindProperty("themeDefinition");

                List<string> names = ParseThemeItems(themeDefinitionProperty.boxedValue as ThemeDefinition, (dynamic)property.managedReferenceValue);
                if (names != null)
                {
                    int selected = names.IndexOf(themeDefinitionItemName.stringValue);

                    using (new EditorGUI.IndentLevelScope())
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        selected = EditorGUILayout.Popup("Bound Theme Item", selected, names.ToArray(), GUILayout.ExpandWidth(true));
                        if (check.changed)
                        {
                            themeDefinitionItemName.stringValue = names[selected];
                        }
                    }
                }
                else
                {
                    themeDefinitionItemName.stringValue = null;
                }
            }

            EditorGUI.EndProperty();

            property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property);

        private List<string> ParseThemeItems<T, K>(ThemeDefinition themeDefinition, BaseThemeBinder<T, K> _)
        {
            if (themeDefinition == null || themeDefinition.ThemeDefinitionItems == null)
            {
                return null;
            }

            List<string> matchingItemNames = new();

            foreach (ThemeDefinition.ThemeDefinitionItem item in themeDefinition.ThemeDefinitionItems)
            {
                if (item.DataType.Type.BaseType.GenericTypeArguments[0].IsAssignableFrom(typeof(T)) && !string.IsNullOrWhiteSpace(item.Name))
                {
                    matchingItemNames.Add(item.Name);
                }
            }

            return matchingItemNames;
        }
    }
}
