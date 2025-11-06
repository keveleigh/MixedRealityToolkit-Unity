using MixedReality.Toolkit.Themes;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(BaseThemeBinder<>), true)]
public class BaseThemeBinderEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //EditorGUI.PropertyField(position, property, label);

        string themeDefinitionPropertyPath = $"{property.propertyPath}.<{nameof(BaseThemeBinder<BaseThemedItemData>.ThemeDefinition)}>k__BackingField";
        string themedItemDataPropertyPath = $"{property.propertyPath}.<{nameof(BaseThemeBinder<BaseThemedItemData>.ThemedItemData)}>k__BackingField";

        SerializedProperty themeDefinition = property.serializedObject.FindProperty(themeDefinitionPropertyPath);
        SerializedProperty themedItemData = property.serializedObject.FindProperty(themedItemDataPropertyPath);

        //position.height = GetPropertyHeight(themeDefinition, label);
        //position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
        EditorGUILayout.PropertyField(themeDefinition, new GUIContent(themeDefinition.name));

        //position.height = GetPropertyHeight(themedItemData, label);
        //position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
        EditorGUILayout.PropertyField(themedItemData, new GUIContent(themedItemData.name));

        EditorGUI.BeginChangeCheck();
        //EditorGUI.DefaultPropertyField(sizeRect, m_ArraySize, GUIContent.none);
        //EditorGUI.LabelField(position, new GUIContent("", "Array Size"));
        if (EditorGUI.EndChangeCheck())
        {
            //m_ReorderableList.InvalidateForGUI();
        }

        if (property.managedReferenceValue != null)
        {
            (List<string> names, int selected, Action<int> setValue) result = OnGUI((dynamic)property.managedReferenceValue);

            if (result.names != null)
            {
                EditorGUI.indentLevel++;
                result.selected = EditorGUILayout.Popup("Theme Options", result.selected != -1 ? result.selected : 0, result.names.ToArray(), GUILayout.ExpandWidth(true));
                result.setValue?.Invoke(result.selected);
                EditorGUI.indentLevel--;
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property);

    // Can this just return dynamic BaseThemeBinder<T>?
    private (List<string>, int, Action<int>) OnGUI<T>(BaseThemeBinder<T> target) where T : BaseThemedItemData
    {
        if (target.ThemeDefinition == null || target.ThemeDefinition.ThemeItems == null)
        {
            return (null, -1, null);
        }

        List<string> matchingItemNames = new();
        List<T> matchingItems = new();

        foreach (BaseThemedItemData item in target.ThemeDefinition.ThemeItems)
        {
            if (item is T itemT && !string.IsNullOrWhiteSpace(item.ItemName))
            {
                matchingItems.Add(itemT);
                matchingItemNames.Add(item.ItemName);
            }
        }

        return (matchingItemNames, matchingItems.IndexOf(target.ThemedItemData), (i) => target.ThemedItemData = matchingItems[i]);
    }
}
