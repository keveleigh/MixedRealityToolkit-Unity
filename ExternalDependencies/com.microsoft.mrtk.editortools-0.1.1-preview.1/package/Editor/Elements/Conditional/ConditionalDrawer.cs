using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	[CustomPropertyDrawer(typeof(ConditionalAttribute))]
	class ConditionalDrawer : PropertyDrawer
	{
		public const string UssClassName = "uxtools-conditional";
		private const string _invalidSourceError = "Invalid value source for ConditionalAttribute on field '{0}': a field, method, or property of type '{1}' named '{2}' could not be found";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var conditionalAttribute = attribute as ConditionalAttribute;
			var element = this.CreateNextElement(property);
			element.AddToClassList(UssClassName);

			if (!SetupCondition(element, property, conditionalAttribute))
				Debug.LogWarningFormat(_invalidSourceError, property.propertyPath, conditionalAttribute.Type.ToString(), conditionalAttribute.ValueSource);

			return element;
		}

		private bool SetupCondition(VisualElement element, SerializedProperty property, ConditionalAttribute conditionalAttribute)
		{
			switch (conditionalAttribute.Type)
			{
				case ConditionalAttribute.TestType.Bool:
					return ReflectionHelper.SetupValueSourceCallback(conditionalAttribute.ValueSource, fieldInfo.DeclaringType, property, element, true, true, value => UpdateBoolVisibility(element, value, conditionalAttribute.BoolTest));
				case ConditionalAttribute.TestType.Int:
					return ReflectionHelper.SetupValueSourceCallback(conditionalAttribute.ValueSource, fieldInfo.DeclaringType, property, element, 0, true, value => UpdateNumberVisibility(element, value, conditionalAttribute.IntValue, conditionalAttribute.NumberTest));
				case ConditionalAttribute.TestType.Float:
					return ReflectionHelper.SetupValueSourceCallback(conditionalAttribute.ValueSource, fieldInfo.DeclaringType, property, element, 0.0f, true, value => UpdateNumberVisibility(element, value, conditionalAttribute.FloatValue, conditionalAttribute.NumberTest));
				case ConditionalAttribute.TestType.String:
					return ReflectionHelper.SetupValueSourceCallback(conditionalAttribute.ValueSource, fieldInfo.DeclaringType, property, element, string.Empty, true, value => UpdateStringVisibility(element, value, conditionalAttribute.StringValue, conditionalAttribute.StringTest));
				case ConditionalAttribute.TestType.Enum:
					return ReflectionHelper.SetupValueSourceCallback(conditionalAttribute.ValueSource, fieldInfo.DeclaringType, property, element, default(Enum), true, value => UpdateEnumVisibility(element, value, conditionalAttribute.IntValue, conditionalAttribute.EnumTest));
				case ConditionalAttribute.TestType.Object:
					return ReflectionHelper.SetupValueSourceCallback(conditionalAttribute.ValueSource, fieldInfo.DeclaringType, property, element, (Object)null, true, value => UpdateObjectVisibility(element, value, conditionalAttribute.ObjectTest));
			}

			return false;
		}

		private static void UpdateBoolVisibility(VisualElement element, bool value, ShowIfBool test)
		{
			element.SetDisplayed((value && test == ShowIfBool.IsTrue) || (!value && test == ShowIfBool.IsFalse));
		}

		private static void UpdateNumberVisibility<T>(VisualElement element, T value, T condition, ShowIfNumber test) where T : IComparable<T>
		{
			var comparison = value.CompareTo(condition);
			var visible = false;

			switch (test)
			{
				case ShowIfNumber.IsEqual:
					visible = comparison == 0;
					break;
				case ShowIfNumber.IsInequal:
					visible = comparison != 0;
					break;
				case ShowIfNumber.IsLessThan:
					visible = comparison < 0;
					break;
				case ShowIfNumber.IsGreaterThan:
					visible = comparison > 0;
					break;
				case ShowIfNumber.IsLessThanOrEqual:
					visible = comparison <= 0;
					break;
				case ShowIfNumber.IsGreaterThanOrEqual:
					visible = comparison >= 0;
					break;
			}

			element.SetDisplayed(visible);
		}

		private static void UpdateStringVisibility(VisualElement element, string value, string comparison, ShowIfString test)
		{
			var visible = false;

			switch (test)
			{
				case ShowIfString.IsSame:
					visible = value == comparison;
					break;
				case ShowIfString.IsDifferent:
					visible = value != comparison;
					break;
			}

			element.SetDisplayed(visible);
		}

		private static void UpdateEnumVisibility(VisualElement element, Enum value, int comparison, ShowIfEnum test)
		{
			var visible = false;

			if (value != null)
			{
				var type = value.GetType();
				var intValue = (int)Enum.Parse(type, value.ToString());

				switch (test)
				{
					case ShowIfEnum.IsEqual:
						visible = intValue == comparison;
						break;
					case ShowIfEnum.IsInequal:
						visible = intValue != comparison;
						break;
				}
			}

			element.SetDisplayed(visible);
		}

		private static void UpdateObjectVisibility(VisualElement element, Object value, ShowIfObject test)
		{
			element.SetDisplayed((value && test == ShowIfObject.IsSet) || (!value && test == ShowIfObject.IsNotSet));
		}
	}
}