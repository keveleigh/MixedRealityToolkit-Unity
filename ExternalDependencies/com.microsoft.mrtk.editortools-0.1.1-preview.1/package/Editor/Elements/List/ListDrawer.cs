using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	[CustomPropertyDrawer(typeof(ListAttribute))]
	class ListDrawer : PropertyDrawer
	{
		private const string _invalidTypeWarning = "Invalid type for ListAttribute on field '{0}': List can only be applied to SerializedList or SerializedArray fields";
		private const string _invalidAddCallbackWarning = "Invalid add callback for ListAttribute on field '{0}': The method must accept an int or have no parameters";
		private const string _invalidAddReferenceCallbackWarning = "Invalid add callback for ListAttribute on field '{0}': The method must accept an int and/or an object in either order or have no parameters";
		private const string _invalidRemoveCallbackWarning = "Invalid remove callback for ListAttribute on field '{0}': The method must accept an int or have no parameters";
		private const string _invalidReorderCallbackWarning = "Invalid reorder callback for ListAttribute on field '{0}': The method must accept two ints or have no parameters";
		private const string _invalidChangeCallbackWarning = "Invalid change callback for ListAttribute on field '{0}': The method must have no parameters";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var items = property.FindPropertyRelative(SerializedList<string>.ItemsProperty);

			if (items != null && items.isArray)
			{
				var isReference = IsReferenceList();
				var referenceType = isReference ? fieldInfo.GetFieldType() : null;
				var declaringType = fieldInfo.DeclaringType;
				var listAttribute = attribute as ListAttribute;
				var drawer = this.GetNextDrawer(property.propertyType == SerializedPropertyType.ManagedReference);
				var proxy = new PropertyListProxy(items, drawer);

				var field = new ListField
				{
					Label = property.displayName,
					tooltip = property.GetTooltip(),
					bindingPath = items.propertyPath,

					AllowAdd = listAttribute.AllowAdd != ListAttribute.Never,
					AllowRemove = listAttribute.AllowRemove != ListAttribute.Never,
					AllowReorder = listAttribute.AllowReorder
				};

				SetupAdd(listAttribute, proxy, field, property, declaringType, isReference);
				SetupRemove(listAttribute, proxy, field, property, declaringType);
				SetupReorder(listAttribute, field, property, declaringType);
				SetupChange(listAttribute, field, property, declaringType);

				field.SetProxy(proxy, referenceType, true);

				return field;
			}
			else
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				return new FieldContainer(property.displayName, string.Empty);
			}
		}

		private bool IsReferenceList()
		{
			return (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(ReferenceList<>))
				|| (fieldInfo.FieldType.BaseType.IsGenericType && fieldInfo.FieldType.BaseType.GetGenericTypeDefinition() == typeof(ReferenceList<>));
		}

		private void SetupAdd(ListAttribute listAttribute, PropertyListProxy proxy, ListField field, SerializedProperty property, Type declaringType, bool isReference)
		{
			if (field.AllowAdd)
			{
				if (!string.IsNullOrEmpty(listAttribute.AllowAdd))
					proxy.CanAddCallback = ReflectionHelper.CreateValueSourceFunction(listAttribute.AllowAdd, property, field, declaringType, true);

				if (!string.IsNullOrEmpty(listAttribute.AddCallback))
				{
					if (!isReference)
					{
						var addCallback = ReflectionHelper.CreateActionCallback(listAttribute.AddCallback, declaringType, property);
						if (addCallback != null)
						{
							field.RegisterCallback<ListField.ItemAddedEvent>(evt => addCallback.Invoke());
						}
						else
						{
							var addCallbackIndex = ReflectionHelper.CreateActionCallback<int>(listAttribute.AddCallback, declaringType, property);
							if (addCallbackIndex != null)
								field.RegisterCallback<ListField.ItemAddedEvent>(evt => addCallbackIndex.Invoke(evt.Index));
							else
								Debug.LogWarningFormat(_invalidAddCallbackWarning, property.propertyPath);
						}
					}
					else
					{
						var addCallback = ReflectionHelper.CreateActionCallback(listAttribute.AddCallback, declaringType, property);
						if (addCallback != null)
						{
							field.RegisterCallback<ListField.ItemAddedEvent>(evt => addCallback.Invoke());
						}
						else
						{
							var addCallbackIndex = ReflectionHelper.CreateActionCallback<int>(listAttribute.AddCallback, declaringType, property);
							if (addCallbackIndex != null)
								field.RegisterCallback<ListField.ItemAddedEvent>(evt => addCallbackIndex.Invoke(evt.Index));
							else
								Debug.LogWarningFormat(_invalidAddReferenceCallbackWarning, property.propertyPath);
						}
					}
				}
			}
		}

		private void SetupRemove(ListAttribute listAttribute, PropertyListProxy proxy, ListField field, SerializedProperty property, Type declaringType)
		{
			if (field.AllowRemove)
			{
				if (!string.IsNullOrEmpty(listAttribute.AllowRemove))
				{
					proxy.CanRemoveCallback = ReflectionHelper.CreateFunctionCallback<int, bool>(listAttribute.AllowRemove, declaringType, property);

					if (proxy.CanRemoveCallback == null)
					{
						var canRemove = ReflectionHelper.CreateValueSourceFunction(listAttribute.AllowRemove, property, field, declaringType, true);
						proxy.CanRemoveCallback = index => canRemove();
					}
				}

				if (!string.IsNullOrEmpty(listAttribute.RemoveCallback))
				{
					var removeCallback = ReflectionHelper.CreateActionCallback(listAttribute.RemoveCallback, declaringType, property);
					if (removeCallback != null)
					{
						field.RegisterCallback<ListField.ItemRemovedEvent>(evt => removeCallback.Invoke());
					}
					else
					{
						var removeCallbackIndex = ReflectionHelper.CreateActionCallback<int>(listAttribute.RemoveCallback, declaringType, property);
						if (removeCallbackIndex != null)
							field.RegisterCallback<ListField.ItemRemovedEvent>(evt => removeCallbackIndex.Invoke(evt.Index));
						else
							Debug.LogWarningFormat(_invalidRemoveCallbackWarning, property.propertyPath);
					}
				}
			}
		}

		private void SetupReorder(ListAttribute listAttribute, ListField field, SerializedProperty property, Type declaringType)
		{
			if (field.AllowReorder)
			{
				if (!string.IsNullOrEmpty(listAttribute.ReorderCallback))
				{
					var reorderCallback = ReflectionHelper.CreateActionCallback(listAttribute.ReorderCallback, declaringType, property);
					if (reorderCallback != null)
					{
						field.RegisterCallback<ListField.ItemReorderedEvent>(evt => reorderCallback.Invoke());
					}
					else
					{
						var reorderCallbackFromTo = ReflectionHelper.CreateActionCallback<int, int>(listAttribute.ReorderCallback, declaringType, property);
						if (reorderCallbackFromTo != null)
							field.RegisterCallback<ListField.ItemReorderedEvent>(evt => reorderCallbackFromTo.Invoke(evt.FromIndex, evt.ToIndex));
						else
							Debug.LogWarningFormat(_invalidReorderCallbackWarning, property.propertyPath);
					}
				}
			}
		}

		private void SetupChange(ListAttribute listAttribute, ListField field, SerializedProperty property, Type declaringType)
		{
			if (!string.IsNullOrEmpty(listAttribute.ChangeCallback))
			{
				var changeCallback = ReflectionHelper.CreateActionCallback(listAttribute.ChangeCallback, declaringType, property);
				if (changeCallback != null)
					field.RegisterCallback<ListField.ItemsChangedEvent>(evt => changeCallback.Invoke());
				else
					Debug.LogWarningFormat(_invalidChangeCallbackWarning, property.propertyPath);
			}
		}
	}
}
