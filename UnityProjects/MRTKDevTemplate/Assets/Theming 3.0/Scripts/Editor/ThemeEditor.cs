// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.Editor;
using System;
using UnityEditor;

namespace MixedReality.Toolkit.Theming.Editor
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

            // TODO: Rewrite to use serialized props?
            if (themeDefinitionProp.objectReferenceValue is ThemeDefinition themeDefinition && themeDefinition.ThemeDefinitionItems.Length != theme.ThemeItems.Count)
            {
                for (int i = 0; i < themeDefinition.ThemeDefinitionItems.Length; i++)
                {
                    ThemeDefinition.ThemeDefinitionItem themeDefinitionItem = themeDefinition.ThemeDefinitionItems[i];
                    Theme.ThemeItem themeItem = theme.ThemeItems.Count > i ? theme.ThemeItems[i] : null;
                    if (themeItem != null && themeItem.ItemName == themeDefinitionItem.ItemName)
                    {
                        continue;
                    }
                    theme.ThemeItems.Insert(i, new() { ItemName = themeDefinitionItem.ItemName, ThemeItemData = Activator.CreateInstance(themeDefinitionItem.ThemeItemData) });
                }
                EditorUtility.SetDirty(theme);
            }

            EditorGUILayout.PropertyField(themeItemsProp);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
