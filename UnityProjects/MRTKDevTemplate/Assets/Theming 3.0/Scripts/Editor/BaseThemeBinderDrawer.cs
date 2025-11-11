using MixedReality.Toolkit.Themes;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BaseThemeBinder<>), true)]
public class BaseThemeBinderDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label.text = label.text.Replace("Element", "Binder");
        label = EditorGUI.BeginProperty(position, label, property);

        EditorGUI.PropertyField(position, property, label, true);
        property.serializedObject.ApplyModifiedProperties();

        if (property.managedReferenceValue != null)
        {
            (List<string> names, int selected, Action<int> setValue) result = ParseThemeItems((dynamic)property.managedReferenceValue);

            if (result.names != null)
            {
                using (new EditorGUI.IndentLevelScope())
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    result.selected = EditorGUILayout.Popup("Theme Options", result.selected, result.names.ToArray(), GUILayout.ExpandWidth(true));
                    if (check.changed)
                    {
                        result.setValue?.Invoke(result.selected);
                        property.serializedObject.Update();
                    }
                }
            }
            else
            {
                result.setValue?.Invoke(result.selected);
                property.serializedObject.Update();
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property);

    private (List<string>, int, Action<int>) ParseThemeItems<T>(BaseThemeBinder<T> binder) where T : BaseThemeItemData
    {
        if (binder.ThemeDefinition == null || binder.ThemeDefinition.ThemeItems == null)
        {
            return (null, -1, (_) => binder.ThemedItemData = default);
        }

        List<string> matchingItemNames = new();
        List<T> matchingItems = new();

        foreach (BaseThemeItemData item in binder.ThemeDefinition.ThemeItems)
        {
            if (item is T itemT && !string.IsNullOrWhiteSpace(item.ItemName))
            {
                matchingItems.Add(itemT);
                matchingItemNames.Add(item.ItemName);
            }
        }

        return (matchingItemNames, matchingItems.IndexOf(binder.ThemedItemData), (i) => binder.ThemedItemData = matchingItems[i]);
    }
}
