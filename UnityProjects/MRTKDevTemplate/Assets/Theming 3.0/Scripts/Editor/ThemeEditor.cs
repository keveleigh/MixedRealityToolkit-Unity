using System;
using UnityEditor;

namespace MixedReality.Toolkit.Themes.Editor
{
    [CustomEditor(typeof(Theme), true)]
    public class ThemeEditor : UnityEditor.Editor
    {
        private SerializedProperty themeDefinitionProp = null;
        private SerializedProperty themeItemsProp = null;

        private Theme theme;

        protected void OnEnable()
        {
            theme = target as Theme;
            themeDefinitionProp = serializedObject.FindProperty("themeDefinition");
            themeItemsProp = serializedObject.FindProperty("ThemeItems");
        }

        /// <summary>
        /// Called by the Unity editor to render custom inspector UI for this component.
        /// </summary>
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(themeDefinitionProp);

            if (themeDefinitionProp.objectReferenceValue is ThemeDefinition themeDefinition && themeDefinition.ThemeDefinitionItems.Length != theme.ThemeItems.Count)
            {
                theme.ThemeItems.Clear();
                foreach (ThemeDefinition.ThemeDefinitionItem themeDefinitionItem in themeDefinition.ThemeDefinitionItems)
                {
                    theme.ThemeItems.Add(new() { ItemName = themeDefinitionItem.ItemName, ThemeItemData = Activator.CreateInstance(themeDefinitionItem.ThemeItemData) });
                }
                EditorUtility.SetDirty(theme);
            }

            EditorGUILayout.PropertyField(themeItemsProp);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
