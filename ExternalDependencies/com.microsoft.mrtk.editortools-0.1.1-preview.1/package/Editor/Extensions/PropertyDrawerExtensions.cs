using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
    public static class PropertyDrawerExtensions
    {
        #region Internal Lookups

        private const string _changedInternalsError = "Failed to setup PropertyDrawer: Unity internals have changed";

        private static FieldInfo m_FieldInfo;
        private static FieldInfo m_Attribute;

        private const string _scriptAttributeUtilityTypeName = "UnityEditor.ScriptAttributeUtility";
        private static MethodInfo _getDrawerTypeForTypeMethod;
        private static object[] _getDrawerTypeForTypeParameters = new object[1];

        static PropertyDrawerExtensions()
        {
            var propertyDrawer = typeof(PropertyDrawer);
            var fieldInfo = propertyDrawer.GetField(nameof(m_FieldInfo), BindingFlags.Instance | BindingFlags.NonPublic);
            var attribute = propertyDrawer.GetField(nameof(m_Attribute), BindingFlags.Instance | BindingFlags.NonPublic);

            if (fieldInfo != null && fieldInfo.FieldType == typeof(FieldInfo))
                m_FieldInfo = fieldInfo;

            if (attribute != null && attribute.FieldType == typeof(PropertyAttribute))
                m_Attribute = attribute;

            var scriptAttributeUtilityType = Assembly.GetAssembly(typeof(Editor))?.CreateInstance(_scriptAttributeUtilityTypeName)?.GetType();
            var getDrawerTypeForTypeMethod = scriptAttributeUtilityType?.GetMethod(nameof(GetDrawerTypeForType), BindingFlags.Static | BindingFlags.NonPublic);
            var getDrawerTypeForTypeParameters = getDrawerTypeForTypeMethod?.GetParameters();

            if (getDrawerTypeForTypeMethod != null)
            {
                if (getDrawerTypeForTypeMethod.HasSignature(typeof(Type), typeof(Type)))
                {
                    _getDrawerTypeForTypeMethod = getDrawerTypeForTypeMethod;
                    _getDrawerTypeForTypeParameters = new object[1];
                }
                else if (getDrawerTypeForTypeMethod.HasSignature(typeof(Type), typeof(Type), typeof(bool)))
                {
                    _getDrawerTypeForTypeMethod = getDrawerTypeForTypeMethod;
                    _getDrawerTypeForTypeParameters = new object[2];
                }
            }

            if (_getDrawerTypeForTypeMethod == null || m_FieldInfo == null || m_Attribute == null)
            {
                Debug.LogError(_changedInternalsError);
            }
        }

        #endregion

        #region Helper Methods

        public static Type GetDrawerTypeForType(Type type, bool isManagedReferenceProperty)
        {
            _getDrawerTypeForTypeParameters[0] = type;
            if (_getDrawerTypeForTypeParameters.Length > 1)
            {
                _getDrawerTypeForTypeParameters[1] = isManagedReferenceProperty;
            }
            return _getDrawerTypeForTypeMethod?.Invoke(null, _getDrawerTypeForTypeParameters) as Type;
        }

        #endregion

        #region Extension Methods

        public static void SetFieldInfo(this PropertyDrawer drawer, FieldInfo value)
        {
            m_FieldInfo?.SetValue(drawer, value);
        }

        public static void SetAttribute(this PropertyDrawer drawer, PropertyAttribute value)
        {
            m_Attribute?.SetValue(drawer, value);
        }

        public static Type GetFieldType(this PropertyDrawer drawer)
        {
            return drawer.fieldInfo.GetFieldType();
        }

        public static string GetTooltip(this PropertyDrawer drawer)
        {
            return drawer.fieldInfo.GetTooltip();
        }

        public static VisualElement CreateNextElement(this PropertyDrawer drawer, SerializedProperty property)
        {
            var nextDrawer = drawer.GetNextDrawer(property.propertyType == SerializedPropertyType.ManagedReference);

            if (nextDrawer != null)
            {
                var element = nextDrawer.CreatePropertyGUI(property);

                return element != null
                    ? element
                    : new ImGuiDrawer(property, nextDrawer);
            }

            return property.CreateField();
        }

        public static PropertyDrawer GetNextDrawer(this PropertyDrawer drawer, bool isManagedReferenceProperty)
        {
            var nextAttribute = GetNextAttribute(drawer, isManagedReferenceProperty);
            var drawerType = GetDrawerTypeForType(nextAttribute?.GetType() ?? GetFieldType(drawer), isManagedReferenceProperty);

            if (drawerType != null)
            {
                var nextDrawer = drawerType.CreateInstance<PropertyDrawer>();
                nextDrawer.SetFieldInfo(drawer.fieldInfo);
                nextDrawer.SetAttribute(nextAttribute);
                return nextDrawer;
            }

            return null;
        }

        public static PropertyAttribute GetNextAttribute(this PropertyDrawer drawer, bool isManagedReferenceProperty)
        {
            return drawer.fieldInfo.GetCustomAttributes<PropertyAttribute>()
#if UNITY_2021_1_OR_NEWER
                .OrderBy(attribute => attribute.order)
#else
                .OrderByDescending(attribute => attribute.order)
#endif
                .SkipWhile(attribute => attribute.GetType() != drawer.attribute.GetType())
                .Where(attribute =>
                {
                    var drawerType = GetDrawerTypeForType(attribute.GetType(), isManagedReferenceProperty);
                    return drawerType != null && drawerType.IsCreatableAs<PropertyDrawer>();
                })
                .ElementAtOrDefault(1);
        }

        public static VisualElement CreateNextElement(FieldInfo field, Attribute attribute, SerializedProperty property)
        {
            var nextDrawer = GetNextDrawer(field, attribute, property.propertyType == SerializedPropertyType.ManagedReference);

            if (nextDrawer != null)
            {
                var element = nextDrawer.CreatePropertyGUI(property);
                return element ?? new ImGuiDrawer(property, nextDrawer);
            }

            return property.CreateField();
        }

        public static PropertyDrawer GetNextDrawer(FieldInfo field, Attribute attribute, bool isManagedReferenceProperty)
        {
            var nextAttribute = GetNextAttribute(field, attribute, isManagedReferenceProperty);
            var drawerType = GetDrawerTypeForType(nextAttribute?.GetType() ?? field.GetFieldType(), isManagedReferenceProperty);

            if (drawerType != null)
            {
                var nextDrawer = drawerType.CreateInstance<PropertyDrawer>();
                nextDrawer.SetFieldInfo(field);
                nextDrawer.SetAttribute(nextAttribute);
                return nextDrawer;
            }

            return null;
        }

        public static PropertyAttribute GetNextAttribute(FieldInfo field, Attribute thisAttribute, bool isManagedReferenceProperty)
        {
            return field.GetCustomAttributes<PropertyAttribute>()
                .OrderByDescending(attribute => attribute.order)
                .ThenBy(attribute => attribute.GetType().FullName) // GetCustomAttributes might return a different order so a secondary sort is needed even though it is a stable sort
                .SkipWhile(attribute => attribute.GetType() != thisAttribute.GetType())
                .Where(attribute =>
                {
                    var drawerType = GetDrawerTypeForType(attribute.GetType(), isManagedReferenceProperty);
                    return drawerType != null && drawerType.IsCreatableAs<PropertyDrawer>();
                })
                .ElementAtOrDefault(1);
        }

        #endregion
    }
}
