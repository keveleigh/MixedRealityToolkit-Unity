using System;
using UnityEditor;
using UnityEngine;

namespace MixedReality.Toolkit.Themes.Editor
{
    [CustomPropertyDrawer(typeof(ConvertTypeAttribute))]
    public class TypePropertyDrawer : PropertyDrawer
    {
        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect fieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(fieldRect, property, label);

            Rect labelRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, Type.GetType(property.stringValue)?.ToString());

            EditorGUI.EndProperty();
        }

        /// <inheritdoc/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
    }
}
