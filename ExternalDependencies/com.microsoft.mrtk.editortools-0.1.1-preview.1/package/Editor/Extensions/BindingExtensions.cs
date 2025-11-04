using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public static class BindingExtensions
	{
		#region Internal Lookups

		private const string _changedInternalsError = "Failed to setup BindingExtensions: Unity internals have changed";
		private const string _typeName = "UnityEditor.UIElements.BindingExtensions, UnityEditor";

		private const string _serializedObjectBindingContextName = "UnityEditor.UIElements.Bindings.SerializedObjectBindingContext, UnityEditor";
		private static readonly Type _serializedObjectBindingContextType;

		private const string _defaultBindName = "DefaultBind";
		private static readonly MethodInfo _defaultBindEnumMethod;

		private const string _serializedObjectBindingName = "UnityEditor.UIElements.Bindings.SerializedObjectBinding`1, UnityEditor";
		private const string _createBindName = "CreateBind";
		private static readonly Type _serializedObjectBindingType;
		private static readonly Dictionary<Type, MethodInfo> _createBindMethods = new Dictionary<Type, MethodInfo>();

		static BindingExtensions()
		{
			var type = Type.GetType(_typeName);

			var serializedObjectBindingContextType = Type.GetType(_serializedObjectBindingContextName);
			var serializedObjectBindingContextConstructor = serializedObjectBindingContextType?.GetConstructor(new Type[] { typeof(SerializedObject) });

			if (serializedObjectBindingContextConstructor != null)
				_serializedObjectBindingContextType = serializedObjectBindingContextType;

			var defaultBindMethod = serializedObjectBindingContextType?.GetMethod(_defaultBindName, BindingFlags.Instance | BindingFlags.NonPublic);
			var defaultBindEnumMethod = defaultBindMethod?.MakeGenericMethod(typeof(Enum));
			var defaultBindParameters = defaultBindEnumMethod?.GetParameters();
			
			if (defaultBindEnumMethod != null && defaultBindEnumMethod.HasSignature(null,
				typeof(VisualElement),
				typeof(SerializedProperty),
				typeof(Func<SerializedProperty, Enum>),
				typeof(Action<SerializedProperty, Enum>),
				typeof(Func<Enum, SerializedProperty, Func<SerializedProperty, Enum>, bool>)))
			{
				_defaultBindEnumMethod = defaultBindEnumMethod;
			}

			if (_serializedObjectBindingContextType == null || _defaultBindEnumMethod == null)
				Debug.LogError(_changedInternalsError);

			_serializedObjectBindingType = Type.GetType(_serializedObjectBindingName);
			//var serializedObjectBindingObjectType = _serializedObjectBindingType?.MakeGenericType(typeof(object));
			//var createBindMethod = serializedObjectBindingObjectType?.GetMethod(_createBindName, BindingFlags.Public | BindingFlags.Static);
			
			// TODO: check CreateBind signature
		}

		#endregion

		#region Helper Methods

		public static void CreateBind<ValueType>(INotifyValueChanged<ValueType> field, SerializedProperty property, Func<SerializedProperty, ValueType> getter, Action<SerializedProperty, ValueType> setter, Func<ValueType, SerializedProperty, Func<SerializedProperty, ValueType>, bool> comparer)
		{
			if (!_createBindMethods.TryGetValue(typeof(ValueType), out var createBindMethod))
			{
				var serializedObjectBindingType = _serializedObjectBindingType?.MakeGenericType(typeof(ValueType));
				createBindMethod = serializedObjectBindingType?.GetMethod(_createBindName, BindingFlags.Public | BindingFlags.Static);
				_createBindMethods.Add(typeof(ValueType), createBindMethod);
			}

			var wrapper = Activator.CreateInstance(_serializedObjectBindingContextType, property.serializedObject);
			createBindMethod.Invoke(null, new object[] { field, wrapper, property, getter, setter, comparer });
		}

		public static void DefaultEnumBind(INotifyValueChanged<Enum> field, SerializedProperty property)
		{
			var type = field.value.GetType();
			var wrapper = Activator.CreateInstance(_serializedObjectBindingContextType, property.serializedObject);

			Func<SerializedProperty, Enum> getter = p => Enum.ToObject(type, p.intValue) as Enum;
			Action<SerializedProperty, Enum> setter = (p, v) => p.intValue = (int)Enum.Parse(type, v.ToString());
			Func<Enum, SerializedProperty, Func<SerializedProperty, Enum>, bool> comparer = (v, p, g) => g(p).Equals(v);

			_defaultBindEnumMethod.Invoke(wrapper, new object[] { field, property, getter, setter, comparer });
		}

		public static void BindManagedReference<ReferenceType>(INotifyValueChanged<ReferenceType> field, SerializedProperty property, Action onSet)
		{
			CreateBind(field, property, GetManagedReference<ReferenceType>, (p, v) => { SetManagedReference(p, v); onSet?.Invoke(); }, CompareManagedReferences);
		}

		private static ReferenceType GetManagedReference<ReferenceType>(SerializedProperty property)
		{
			var value = property.managedReferenceValue;
			if (value is ReferenceType reference)
				return reference;

			return default;
		}

		private static void SetManagedReference<ReferenceType>(SerializedProperty property, ReferenceType value)
		{
			Undo.RegisterCompleteObjectUndo(property.serializedObject.targetObject, "Change reference");

			property.managedReferenceValue = value;
			property.serializedObject.ApplyModifiedProperties();

			Undo.FlushUndoRecordObjects();
		}

		private static bool CompareManagedReferences<ReferenceType>(ReferenceType value, SerializedProperty property, Func<SerializedProperty, ReferenceType> getter)
		{
			var currentValue = getter(property);
			return ReferenceEquals(value, currentValue);
		}

		#endregion
	}
}
