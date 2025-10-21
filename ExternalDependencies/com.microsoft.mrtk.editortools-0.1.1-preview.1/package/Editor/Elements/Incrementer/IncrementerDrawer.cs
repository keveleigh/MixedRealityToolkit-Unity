using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	[CustomPropertyDrawer(typeof(IncrementerAttribute))]
	class IncrementerDrawer : PropertyDrawer
	{
		private const string _invalidTypeWarning = "Invalid type for IncrementerAttribute on field {0}: Incrementer can only be applied to int or float";
		private const string _invalidSourceError = "Invalid incrmeent source for IncrementerAttribute on field '{0}': a field, method, or property of type '{1}' named '{2}' could not be found";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var incrementerAttribute = attribute as IncrementerAttribute;

			if (property.propertyType == SerializedPropertyType.Integer)
				return CreateIncrementer(new IncrementerIntField(), incrementerAttribute, property, Mathf.RoundToInt(incrementerAttribute.Increment));
			else if (property.propertyType == SerializedPropertyType.Float)
				return CreateIncrementer(new IncrementerFloatField(), incrementerAttribute, property, incrementerAttribute.Increment);
			else
				Debug.LogWarningFormat(property.serializedObject.targetObject, _invalidTypeWarning, property.propertyPath);

			return new FieldContainer(property.displayName);
		}

		private VisualElement CreateIncrementer<T>(IncrementerField<T> incrementer, IncrementerAttribute incrementerAttribute, SerializedProperty property, T defaultIncrement)
		{
			void setIncrement(T value) => incrementer.Increment = value;

			if (!ReflectionHelper.SetupValueSourceCallback(incrementerAttribute.IncrementSource, fieldInfo.DeclaringType, property, incrementer, defaultIncrement, incrementerAttribute.AutoUpdate, setIncrement))
				Debug.LogWarningFormat(_invalidSourceError, property.propertyPath, nameof(T), incrementerAttribute.IncrementSource);

			return incrementer.ConfigureProperty(property);
		}
	}
}