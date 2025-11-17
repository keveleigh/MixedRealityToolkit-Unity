using MixedReality.Toolkit.Themes;
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
            IBinder binder = property.managedReferenceValue as IBinder;
            List<string> names = ParseThemeItems((dynamic)property.managedReferenceValue);
            if (names != null)
            {
                int selected = names.IndexOf(binder.ThemeDefinitionItemName);

                using (new EditorGUI.IndentLevelScope())
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    selected = EditorGUILayout.Popup("Bound Theme Item", selected, names.ToArray(), GUILayout.ExpandWidth(true));
                    if (check.changed)
                    {
                        binder.ThemeDefinitionItemName = names[selected];
                        property.serializedObject.Update();
                    }
                }
            }
            else
            {
                binder.ThemeDefinitionItemName = null;
                property.serializedObject.Update();
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property);

    private List<string> ParseThemeItems<T>(BaseThemeBinder<T> binder)
    {
        if (binder.ThemeDefinition == null || binder.ThemeDefinition.ThemeDefinitionItems == null)
        {
            return null;
        }

        List<string> matchingItemNames = new();

        foreach (ThemeDefinition.ThemeDefinitionItem item in binder.ThemeDefinition.ThemeDefinitionItems)
        {
            if (item.ThemeItemData.Type.BaseType.GenericTypeArguments[0].IsAssignableFrom(typeof(T)) && !string.IsNullOrWhiteSpace(item.ItemName))
            {
                matchingItemNames.Add(item.ItemName);
            }
        }

        return matchingItemNames;
    }
}
