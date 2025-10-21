using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public interface IDictionaryProxy<KeyType>
	{
		VisualElement CreateElement(int index, KeyType key);

		int Count { get; }
		KeyType GetKey(int index);

		bool CanAdd(KeyType key);
		bool CanAdd(Type type);
		bool AddItem(KeyType key, Type type);

		bool CanRemove(int index, KeyType key);
		void RemoveItem(int index, KeyType key);

		bool IsReorderable { get; }
		void ReorderItem(int from, int to);
	}

	public class DictionaryProxy : IDictionaryProxy<string>
	{
		public IDictionary Items { get; private set; }
		public Func<IDictionary, string, VisualElement> Creator { get; private set; }

		public DictionaryProxy(IDictionary items, Func<IDictionary, string, VisualElement> creator)
		{
			Items = items;
			Creator = creator;
		}

		public VisualElement CreateElement(int index, string key) => Creator.Invoke(Items, key);

		public int Count => Items.Count;
		public string GetKey(int index) => Items.Keys.Cast<string>().ElementAt(index);

		public bool CanAdd(string key) => !Items.Contains(key);
		public bool CanAdd(Type type) => type != null;

		public bool AddItem(string key, Type type)
		{
			try
			{
				var item = Activator.CreateInstance(type);
				Items.Add(key, item);
			}
			catch
			{
				return false;
			}

			return true;
		}

		public bool CanRemove(int index, string key) => true;
		public void RemoveItem(int index, string key) => Items.Remove(key);

		public bool IsReorderable => false;
		public void ReorderItem(int from, int to) { }
	}

	public class DictionaryProxy<ValueType> : IDictionaryProxy<string>
	{
		public IDictionary<string, ValueType> Items { get; private set; }
		public Func<IDictionary<string, ValueType>, string, VisualElement> Creator { get; private set; }

		public DictionaryProxy(Dictionary<string, ValueType> items, Func<IDictionary<string, ValueType>, string, VisualElement> creator)
		{
			Items = items;
			Creator = creator;
		}

		public VisualElement CreateElement(int index, string key) => Creator.Invoke(Items, key);

		public int Count => Items.Count;
		public string GetKey(int index) => Items.Keys.ElementAt(index);

		public bool CanAdd(string key) => !Items.ContainsKey(key);
		public bool CanAdd(Type type) => type == null || (type == typeof(ValueType) && type.IsValueType) || type.IsCreatableAs<ValueType>();

		public bool AddItem(string key, Type type)
		{
			var item = type == null || type.IsValueType
				? default
				: (ValueType)Activator.CreateInstance(type);

			Items.Add(key, item);
			return true;
		}

		public bool CanRemove(int index, string key) => true;
		public void RemoveItem(int index, string key) => Items.Remove(key);

		public bool IsReorderable => false;
		public void ReorderItem(int from, int to) { }
	}

	public abstract class PropertyDictionaryProxy<KeyType> : IDictionaryProxy<KeyType>
	{
		public Func<KeyType, bool> CanAddKeyCallback;
		public Func<Type, bool> CanAddTypeCallback;
		public Func<KeyType, bool> CanRemoveCallback;

		protected readonly SerializedProperty _property;
		protected readonly SerializedProperty _keysProperty;
		protected readonly SerializedProperty _valuesProperty;
		protected readonly PropertyDrawer _drawer;

		public abstract KeyType GetKey(int index);
		protected abstract void SetKey(SerializedProperty property, KeyType key);
		protected abstract string GetLabel(KeyType key);

		public PropertyDictionaryProxy(SerializedProperty property, SerializedProperty keys, SerializedProperty values, PropertyDrawer drawer)
		{
			_property = property;
			_keysProperty = keys;
			_valuesProperty = values;
			_drawer = drawer;
		}

		public VisualElement CreateElement(int index, KeyType key)
		{
			var value = _valuesProperty.GetArrayElementAtIndex(index);
			var field = _drawer?.CreatePropertyGUI(value) ?? value.CreateField();

			field.Bind(_property.serializedObject);

			var label = GetLabel(key);

			if (string.IsNullOrEmpty(label))
				label = " "; // An empty label will cause the label to be removed;

			if (field.SetFieldLabel(label)) // TODO: for references this should include the type name
			{
				return field;
			}
			else
			{
				var container = new FieldContainer(label);
				container.Add(field);
				return container;
			}
		}

		public int Count => _keysProperty.arraySize;

		public bool CanAdd(KeyType key)
		{
			if (key == null)
				return false;

			for (var i = 0; i < _keysProperty.arraySize; i++)
			{
				if (GetKey(i).GetHashCode() == key.GetHashCode())
					return false;
			}

			return CanAddKeyCallback != null
				? CanAddKeyCallback.Invoke(key)
				: true;
		}

		public bool CanAdd(Type type)
		{
			return type != null && CanAddTypeCallback != null
				? CanAddTypeCallback.Invoke(type)
				: true;
		}

		public bool AddItem(KeyType key, Type type)
		{
			try
			{
				var newSize = _keysProperty.arraySize + 1;
				_valuesProperty.ResizeArray(newSize);

				if (type != null)
				{
					var newValue = Activator.CreateInstance(type);
					var valueProperty = _valuesProperty.GetArrayElementAtIndex(newSize - 1);

					if (!valueProperty.TrySetValue(newValue))
					{
						_valuesProperty.arraySize = newSize - 1;
						return false;
					}
				}

				_keysProperty.arraySize = newSize;
				var newItem = _keysProperty.GetArrayElementAtIndex(newSize - 1);
				SetKey(newItem, key);

				_property.serializedObject.ApplyModifiedProperties(); // TODO: not applying new reference values for some reason
				return true;
			}
			catch
			{
				// Technically a user could do something really wierd like set the item type on the DictionaryField
				// to Float when the property is actually a string

				// TODO: this also happens if the type is not Serializable (_valuesProperty will be null)
				return false;
			}
		}

		public bool CanRemove(int index, KeyType key)
		{
			return CanRemoveCallback != null
				? CanRemoveCallback.Invoke(key)
				: true;
		}

		public void RemoveItem(int index, KeyType key)
		{
			_keysProperty.RemoveFromArray(index);
			_valuesProperty.RemoveFromArray(index);
			_property.serializedObject.ApplyModifiedProperties();
		}

		public bool IsReorderable => true;

		public void ReorderItem(int from, int to)
		{
			_keysProperty.MoveArrayElement(from, to);
			_valuesProperty.MoveArrayElement(from, to);
			_property.serializedObject.ApplyModifiedProperties();
		}
	}

	public class EnumPropertyDictionaryProxy : PropertyDictionaryProxy<Enum>
	{
		public EnumPropertyDictionaryProxy(SerializedProperty property, SerializedProperty keys, SerializedProperty values, PropertyDrawer drawer)
			: base(property, keys, values, drawer)
		{
		}

		public override Enum GetKey(int index) => _keysProperty.GetArrayElementAtIndex(index).GetEnumValue();
		protected override void SetKey(SerializedProperty property, Enum key) => property.SetEnumValue(key);
		protected override string GetLabel(Enum key) => key.ToString();
	}

	public class StringPropertyDictionaryProxy : PropertyDictionaryProxy<string>
	{
		public StringPropertyDictionaryProxy(SerializedProperty property, SerializedProperty keys, SerializedProperty values, PropertyDrawer drawer)
			: base(property, keys, values, drawer)
		{
		}

		public override string GetKey(int index) => _keysProperty.GetArrayElementAtIndex(index).stringValue;
		protected override void SetKey(SerializedProperty property, string key) => property.stringValue = key;
		protected override string GetLabel(string key) => key;
	}
}
