using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	[CustomPropertyDrawer(typeof(DictionaryAttribute))]
	class DictionaryDrawer : PropertyDrawer
	{
		private const string _invalidTypeWarning = "Invalid type for DictionaryAttribute on field '{0}': Dictionary can only be applied to SerializedDictionary fields with string or enum keys";
		private const string _invalidAddCallbackWarning = "Invalid add callback for DictionaryAttribute on field '{0}': The method must accept a string or have no parameters";
		private const string _invalidAddReferenceCallbackWarning = "Invalid add callback for DictionaryAttribute on field '{0}': The method must accept a string or have no parameters";
		private const string _invalidRemoveCallbackWarning = "Invalid remove callback for DictionaryAttribute on field '{0}': The method must accept an string or have no parameters";
		private const string _invalidReorderCallbackWarning = "Invalid reorder callback for DictionaryAttribute on field '{0}': The method must accept two ints or have no parameters";
		private const string _invalidChangeCallbackWarning = "Invalid change callback for DictionaryAttribute on field '{0}': The method must have no parameters";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var keyType = GetKeyType();
			var keys = property.FindPropertyRelative(SerializedDictionary<string, int>.KeyProperty);
			var values = property.FindPropertyRelative(SerializedDictionary<string, int>.ValueProperty);

			if (keys != null && keys.isArray && values != null && values.isArray && IsValidKeyType(keyType))
			{
				var isReference = fieldInfo.FieldType.BaseType.GetGenericTypeDefinition() == typeof(ReferenceDictionary<,>);
				var referenceType = isReference ? fieldInfo.GetFieldType() : null;
				var declaringType = fieldInfo.DeclaringType;
				var dictionaryAttribute = attribute as DictionaryAttribute;
				var drawer = this.GetNextDrawer();

				VisualElement field = null;

				if (keyType == typeof(string))
					field = CreateStringDictionary(dictionaryAttribute, declaringType, isReference, referenceType, property, keys, values, drawer);
				else if (keyType.IsEnum)
					field = CreateEnumDictionary(dictionaryAttribute, declaringType, keyType, isReference, referenceType, property, keys, values, drawer);

				return field;
			}
			else
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				return new FieldContainer(property.displayName);
			}
		}

		private bool IsValidKeyType(Type keyType)
		{
			return keyType != null
				&& (keyType == typeof(string)
				|| keyType.IsEnum);
		}

		private Type GetKeyType()
		{
			if (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(SerializedDictionary<,>))
				return fieldInfo.FieldType.GetGenericArguments()[0];
			else if (fieldInfo.FieldType.BaseType.IsGenericType && fieldInfo.FieldType.BaseType.GetGenericTypeDefinition() == typeof(SerializedDictionary<,>))
				return fieldInfo.FieldType.BaseType.GetGenericArguments()[0];
			else if (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(ReferenceDictionary<,>))
				return fieldInfo.FieldType.GetGenericArguments()[0];
			else if (fieldInfo.FieldType.BaseType.IsGenericType && fieldInfo.FieldType.BaseType.GetGenericTypeDefinition() == typeof(ReferenceDictionary<,>))
				return fieldInfo.FieldType.BaseType.GetGenericArguments()[0];

			return null;
		}

		private bool IsReferenceDictionary()
		{
			return (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(ReferenceList<>))
				|| (fieldInfo.FieldType.BaseType.IsGenericType && fieldInfo.FieldType.BaseType.GetGenericTypeDefinition() == typeof(ReferenceList<>));
		}

		private VisualElement CreateStringDictionary(DictionaryAttribute dictionaryAttribute, Type declaringType, bool isReference, Type referenceType, SerializedProperty property, SerializedProperty keys, SerializedProperty values, PropertyDrawer drawer)
		{
			var proxy = new StringPropertyDictionaryProxy(property, keys, values, drawer);

			var field = new StringDictionaryField
			{
				IsCollapsable = dictionaryAttribute.IsCollapsable,
				bindingPath = keys.propertyPath,
				Label = property.displayName
			};

			// TODO: other stuff from ConfigureField

			if (!string.IsNullOrEmpty(dictionaryAttribute.AddPlaceholder))
				field.AddPlaceholder = dictionaryAttribute.AddPlaceholder;

			if (!string.IsNullOrEmpty(dictionaryAttribute.EmptyLabel))
				field.EmptyLabel = dictionaryAttribute.EmptyLabel;

			field.AllowAdd = dictionaryAttribute.AllowAdd != DictionaryAttribute.Never;
			field.AllowRemove = dictionaryAttribute.AllowRemove != DictionaryAttribute.Never;
			field.AllowReorder = dictionaryAttribute.AllowReorder;

			SetupAdd(dictionaryAttribute, proxy, field, property, declaringType, isReference);
			SetupRemove(dictionaryAttribute, proxy, field, property, declaringType);
			SetupReorder(dictionaryAttribute, field, property, declaringType);
			SetupChange(dictionaryAttribute, field, property, declaringType);

			field.SetProxy(proxy, referenceType, true);

			return field;
		}

		private VisualElement CreateEnumDictionary(DictionaryAttribute dictionaryAttribute, Type declaringType, Type keyType, bool isReference, Type referenceType, SerializedProperty property, SerializedProperty keys, SerializedProperty values, PropertyDrawer drawer)
		{
			var proxy = new EnumPropertyDictionaryProxy(property, keys, values, drawer);

			var field = new EnumDictionaryField
			{
				IsCollapsable = dictionaryAttribute.IsCollapsable,
				bindingPath = keys.propertyPath,
				Label = property.displayName
			};

			var defaultValue = keyType.GetEnumValues().GetValue(0) as Enum;
			field.Initialize(defaultValue);

			// TODO: other stuff from ConfigureField

			if (!string.IsNullOrEmpty(dictionaryAttribute.AddPlaceholder))
				field.AddPlaceholder = dictionaryAttribute.AddPlaceholder;

			if (!string.IsNullOrEmpty(dictionaryAttribute.EmptyLabel))
				field.EmptyLabel = dictionaryAttribute.EmptyLabel;

			field.AllowAdd = dictionaryAttribute.AllowAdd != DictionaryAttribute.Never;
			field.AllowRemove = dictionaryAttribute.AllowRemove != DictionaryAttribute.Never;
			field.AllowReorder = dictionaryAttribute.AllowReorder;

			SetupAdd(dictionaryAttribute, proxy, field, property, declaringType, isReference);
			SetupRemove(dictionaryAttribute, proxy, field, property, declaringType);
			SetupReorder(dictionaryAttribute, field, property, declaringType);
			SetupChange(dictionaryAttribute, field, property, declaringType);

			field.SetProxy(proxy, referenceType, true);

			return field;
		}

		private void SetupAdd<KeyType>(DictionaryAttribute dictionaryAttribute, PropertyDictionaryProxy<KeyType> proxy, DictionaryField<KeyType> field, SerializedProperty property, Type declaringType, bool isReference) where KeyType : class
		{
			if (field.AllowAdd)
			{
				if (!string.IsNullOrEmpty(dictionaryAttribute.AllowAdd))
				{
					proxy.CanAddKeyCallback = ReflectionHelper.CreateFunctionCallback<KeyType, bool>(dictionaryAttribute.AllowAdd, declaringType, property);
					if (proxy.CanAddKeyCallback == null)
					{
						var canAdd = ReflectionHelper.CreateValueSourceFunction(dictionaryAttribute.AllowAdd, property, field, declaringType, true);
						proxy.CanAddKeyCallback = index => canAdd();
					}
				}

				if (!string.IsNullOrEmpty(dictionaryAttribute.AddCallback))
				{
					if (!isReference)
					{
						var addCallback = ReflectionHelper.CreateActionCallback(dictionaryAttribute.AddCallback, declaringType, property);
						if (addCallback != null)
						{
							field.RegisterCallback<DictionaryField<KeyType>.ItemAddedEvent>(evt => addCallback.Invoke());
						}
						else
						{
							var addCallbackKey = ReflectionHelper.CreateActionCallback<KeyType>(dictionaryAttribute.AddCallback, declaringType, property);
							if (addCallbackKey != null)
								field.RegisterCallback<DictionaryField<KeyType>.ItemAddedEvent>(evt => addCallbackKey.Invoke(evt.Key));
							else
								Debug.LogWarningFormat(_invalidAddCallbackWarning, property.propertyPath);
						}
					}
					else
					{
						var addCallback = ReflectionHelper.CreateActionCallback(dictionaryAttribute.AddCallback, declaringType, property);
						if (addCallback != null)
						{
							field.RegisterCallback<DictionaryField<KeyType>.ItemAddedEvent>(evt => addCallback.Invoke());
						}
						else
						{
							var addCallbackKey = ReflectionHelper.CreateActionCallback<KeyType>(dictionaryAttribute.AddCallback, declaringType, property);
							if (addCallbackKey != null)
								field.RegisterCallback<DictionaryField<KeyType>.ItemAddedEvent>(evt => addCallbackKey.Invoke(evt.Key));
							else
								Debug.LogWarningFormat(_invalidAddReferenceCallbackWarning, property.propertyPath);
						}
					}
				}
			}
		}

		private void SetupRemove<KeyType>(DictionaryAttribute dictionaryAttribute, PropertyDictionaryProxy<KeyType> proxy, DictionaryField<KeyType> field, SerializedProperty property, Type declaringType) where KeyType : class
		{
			if (field.AllowRemove)
			{
				if (!string.IsNullOrEmpty(dictionaryAttribute.AllowRemove))
				{
					proxy.CanRemoveCallback = ReflectionHelper.CreateFunctionCallback<KeyType, bool>(dictionaryAttribute.AllowRemove, declaringType, property);
					if (proxy.CanRemoveCallback == null)
					{
						var canRemove = ReflectionHelper.CreateValueSourceFunction(dictionaryAttribute.AllowRemove, property, field, declaringType, true);
						proxy.CanRemoveCallback = index => canRemove();
					}
				}

				if (!string.IsNullOrEmpty(dictionaryAttribute.RemoveCallback))
				{
					var removeCallback = ReflectionHelper.CreateActionCallback(dictionaryAttribute.RemoveCallback, declaringType, property);
					if (removeCallback != null)
					{
						field.RegisterCallback<DictionaryField<KeyType>.ItemRemovedEvent>(evt => removeCallback.Invoke());
					}
					else
					{
						var removeCallbackKey = ReflectionHelper.CreateActionCallback<KeyType>(dictionaryAttribute.RemoveCallback, declaringType, property);
						if (removeCallbackKey != null)
							field.RegisterCallback<DictionaryField<KeyType>.ItemRemovedEvent>(evt => removeCallbackKey.Invoke(evt.Key));
						else
							Debug.LogWarningFormat(_invalidRemoveCallbackWarning, property.propertyPath);
					}
				}
			}
		}

		private void SetupReorder<KeyType>(DictionaryAttribute dictionaryAttribute, DictionaryField<KeyType> field, SerializedProperty property, Type declaringType) where KeyType : class
		{
			if (field.AllowReorder)
			{
				if (!string.IsNullOrEmpty(dictionaryAttribute.ReorderCallback))
				{
					var reorderCallback = ReflectionHelper.CreateActionCallback(dictionaryAttribute.ReorderCallback, declaringType, property);
					if (reorderCallback != null)
					{
						field.RegisterCallback<DictionaryField<KeyType>.ItemReorderedEvent>(evt => reorderCallback.Invoke());
					}
					else
					{
						var reorderCallbackFromTo = ReflectionHelper.CreateActionCallback<int, int>(dictionaryAttribute.ReorderCallback, declaringType, property);
						if (reorderCallbackFromTo != null)
							field.RegisterCallback<DictionaryField<KeyType>.ItemReorderedEvent>(evt => reorderCallbackFromTo.Invoke(evt.FromIndex, evt.ToIndex));
						else
							Debug.LogWarningFormat(_invalidReorderCallbackWarning, property.propertyPath);
					}
				}
			}
		}

		private void SetupChange<KeyType>(DictionaryAttribute dictionaryAttribute, DictionaryField<KeyType> field, SerializedProperty property, Type declaringType) where KeyType : class
		{
			if (!string.IsNullOrEmpty(dictionaryAttribute.ChangeCallback))
			{
				var changeCallback = ReflectionHelper.CreateActionCallback(dictionaryAttribute.ChangeCallback, declaringType, property);
				if (changeCallback != null)
					field.RegisterCallback<DictionaryField<KeyType>.ItemsChangedEvent>(evt => changeCallback.Invoke());
				else
					Debug.LogWarningFormat(_invalidChangeCallbackWarning, property.propertyPath);
			}
		}
	}
}
