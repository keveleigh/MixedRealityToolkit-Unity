using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	[CustomPropertyDrawer(typeof(DisableAttribute))]
	class DisableDrawer : PropertyDrawer
	{
		public const string UssClassName = "uxtools-disable";
		private const string _invalidSourceError = "Invalid value source for DisableAttribute on field '{0}': a field, method, or property of type '{1}' named '{2}' could not be found";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var disableAttribute = attribute as DisableAttribute;
			var element = this.CreateNextElement(property);
			element.AddToClassList(UssClassName);

			if (!SetupCondition(element, property, disableAttribute))
				Debug.LogWarningFormat(_invalidSourceError, property.propertyPath, disableAttribute.Type.ToString(), disableAttribute.ValueSource);

			return element;
		}

		private bool SetupCondition(VisualElement element, SerializedProperty property, DisableAttribute disableAttribute)
		{
			switch (disableAttribute.Type)
			{
				case DisableAttribute.TestType.Bool:
					return ReflectionHelper.SetupValueSourceCallback(disableAttribute.ValueSource, fieldInfo.DeclaringType, property, element, true, true, value => UpdateBoolVisibility(element, value, disableAttribute.BoolTest));
				case DisableAttribute.TestType.Int:
					return ReflectionHelper.SetupValueSourceCallback(disableAttribute.ValueSource, fieldInfo.DeclaringType, property, element, 0, true, value => UpdateNumberVisibility(element, value, disableAttribute.IntValue, disableAttribute.NumberTest));
				case DisableAttribute.TestType.Float:
					return ReflectionHelper.SetupValueSourceCallback(disableAttribute.ValueSource, fieldInfo.DeclaringType, property, element, 0.0f, true, value => UpdateNumberVisibility(element, value, disableAttribute.FloatValue, disableAttribute.NumberTest));
				case DisableAttribute.TestType.String:
					return ReflectionHelper.SetupValueSourceCallback(disableAttribute.ValueSource, fieldInfo.DeclaringType, property, element, string.Empty, true, value => UpdateStringVisibility(element, value, disableAttribute.StringValue, disableAttribute.StringTest));
				case DisableAttribute.TestType.Enum:
					return ReflectionHelper.SetupValueSourceCallback(disableAttribute.ValueSource, fieldInfo.DeclaringType, property, element, default(Enum), true, value => UpdateEnumVisibility(element, value, disableAttribute.IntValue, disableAttribute.EnumTest));
				case DisableAttribute.TestType.Object:
					return ReflectionHelper.SetupValueSourceCallback(disableAttribute.ValueSource, fieldInfo.DeclaringType, property, element, (Object)null, true, value => UpdateObjectVisibility(element, value, disableAttribute.ObjectTest));
			}

			return false;
		}

		private static void UpdateBoolVisibility(VisualElement element, bool value, DisableIfBool test)
		{
			element.SetEnabled(!((value && test == DisableIfBool.IsTrue) || (!value && test == DisableIfBool.IsFalse)));
		}

		private static void UpdateNumberVisibility<T>(VisualElement element, T value, T condition, DisableIfNumber test) where T : IComparable<T>
		{
			var comparison = value.CompareTo(condition);
			var disable = false;

			switch (test)
			{
				case DisableIfNumber.IsEqual: disable = comparison == 0; break;
				case DisableIfNumber.IsInequal: disable = comparison != 0; break;
				case DisableIfNumber.IsLessThan: disable = comparison < 0; break;
				case DisableIfNumber.IsGreaterThan: disable = comparison > 0; break;
				case DisableIfNumber.IsLessThanOrEqual: disable = comparison <= 0; break;
				case DisableIfNumber.IsGreaterThanOrEqual: disable = comparison >= 0; break;
			}

			element.SetEnabled(!disable);
		}

		private static void UpdateStringVisibility(VisualElement element, string value, string comparison, DisableIfString test)
		{
			var disable = false;

			switch (test)
			{
				case DisableIfString.IsSame: disable = value == comparison; break;
				case DisableIfString.IsDifferent: disable = value != comparison; break;
			}

			element.SetEnabled(!disable);
		}

		private static void UpdateEnumVisibility(VisualElement element, Enum value, int comparison, DisableIfEnum test)
		{
			var disable = false;

			if (value != null)
			{
				var type = value.GetType();
				var intValue = (int)Enum.Parse(type, value.ToString());

				switch (test)
				{
					case DisableIfEnum.IsEqual: disable = intValue == comparison; break;
					case DisableIfEnum.IsInequal: disable = intValue != comparison; break;
				}
			}

			element.SetEnabled(!disable);
		}

		private static void UpdateObjectVisibility(VisualElement element, Object value, DisableIfObject test)
		{
			element.SetEnabled(!((value && test == DisableIfObject.IsSet) || (!value && test == DisableIfObject.IsNotSet)));
		}
	}
}