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

        private static bool itemsFoldout = false;

        protected void OnEnable()
        {
            themeDefinitionProp = serializedObject.FindProperty("themeDefinition");
            themeItemsProp = serializedObject.FindProperty("themeItems");
        }

        /// <summary>
        /// Called by the Unity editor to render custom inspector UI for this component.
        /// </summary>
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(themeDefinitionProp);

            SerializedProperty themeDefinitionArrayProp = themeDefinitionProp.objectReferenceValue != null
                ? new SerializedObject(themeDefinitionProp.objectReferenceValue).FindProperty(InspectorUIUtility.GetBackingField(nameof(ThemeDefinition.ThemeDefinitionItems)))
                : null;

            if (themeDefinitionArrayProp != null)
            {
                itemsFoldout = EditorGUILayout.Foldout(itemsFoldout, "Theme Values", true);
                if (itemsFoldout)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        for (int i = 0; i < themeDefinitionArrayProp.arraySize; i++)
                        {
                            SerializedProperty themeDefinitionItem = themeDefinitionArrayProp.GetArrayElementAtIndex(i);
                            string themeDefinitionItemName = themeDefinitionItem.FindPropertyRelative(InspectorUIUtility.GetBackingField(nameof(ThemeDefinition.ThemeDefinitionItem.Name))).stringValue;

                            SerializedProperty themeItem = themeItemsProp.arraySize > i ? themeItemsProp.GetArrayElementAtIndex(i) : null;
                            if (themeItem == null
                                || themeItem.FindPropertyRelative(InspectorUIUtility.GetBackingField(nameof(Theme.ThemeItem.Name))).stringValue != themeDefinitionItemName)
                            {
                                string valueDataType = themeDefinitionItem.FindPropertyRelative(InspectorUIUtility.GetBackingField(nameof(ThemeDefinition.ThemeDefinitionItem.DataType))).FindPropertyRelative("reference").stringValue;

                                themeItemsProp.InsertArrayElementAtIndex(i);
                                themeItem = themeItemsProp.GetArrayElementAtIndex(i);
                                themeItem.managedReferenceValue = new Theme.ThemeItem(themeDefinitionItemName, Activator.CreateInstance(Type.GetType(valueDataType)));
                            }

                            EditorGUILayout.PropertyField(themeItem, true);
                        }
                    }
                    themeItemsProp.arraySize = themeDefinitionArrayProp.arraySize;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
