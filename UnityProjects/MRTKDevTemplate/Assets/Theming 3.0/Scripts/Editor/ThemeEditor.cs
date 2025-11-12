using System;
using System.Collections.Generic;
using UnityEditor;

namespace MixedReality.Toolkit.Themes.Editor
{
    [CustomEditor(typeof(Theme), true)]
    public class ThemeEditor : UnityEditor.Editor
    {
        private SerializedProperty themeDefinitionProp = null;

        private List<string> availableNames = new List<string>();
        private string[] availableNamesArray = Array.Empty<string>();

        protected void OnEnable()
        {
            Theme theme = (Theme)target;
            themeDefinitionProp = serializedObject.FindProperty("themeDefinition");
        }

        /// <summary>
        /// Called by the Unity editor to render custom inspector UI for this component.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(themeDefinitionProp);

            if (themeDefinitionProp.objectReferenceValue is ThemeDefinition themeDefinition)
            {
                foreach (ThemeDefinition.ThemeDefinitionItem themeDefinitionItem in themeDefinition.ThemeDefinitionItems)
                {
                    availableNames.Add(themeDefinitionItem.ItemName);
                }

                int selected = EditorGUILayout.Popup(string.Empty, 0, availableNamesArray);
            }
        }
    }
}
