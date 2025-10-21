using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
    [CustomPropertyDrawer(typeof(ChangeTriggerAttribute))]
	class ChangeTriggerDrawer : PropertyDrawer
	{
		private const string _invalidTypeWarning = "Invalid type for ChangeTriggerAttribute on field '{0}': ChangeTrigger can only be applied to serializable fields";
		private const string _invalidMethodWarning = "Invalid method for ChangeTriggerAttribute on field '{0}': the method '{1}' should take 0, 1, or 2 parameters of type '{2}'";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var element = this.CreateNextElement(property);
			var changeAttribute = attribute as ChangeTriggerAttribute;
			var change = CreateControl(property, fieldInfo.DeclaringType, changeAttribute.Method, changeAttribute.TriggerInPlaymode, changeAttribute.TriggerInEditor);

			if (change != null)
				element.Add(change);

			return element;
		}

		private PropertyWatcher CreateControl(SerializedProperty property, Type declaringType, string method, bool triggerInPlayMode, bool triggerInEditor)
		{
			switch (property.propertyType)
			{
				case SerializedPropertyType.Generic: return CreateControl<object>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.Integer: return CreateControl<int>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.Boolean: return CreateControl<bool>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.Float: return CreateControl<float>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.String: return CreateControl<string>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.Color: return CreateControl<Color>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.ObjectReference: return CreateControl<Object>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.LayerMask: return CreateControl<int>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.Enum: return CreateControl<Enum>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.Vector2: return CreateControl<Vector2>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.Vector2Int: return CreateControl<Vector2Int>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.Vector3: return CreateControl<Vector3>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.Vector3Int: return CreateControl<Vector3Int>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.Vector4: return CreateControl<Vector4>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.Rect: return CreateControl<Rect>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.RectInt: return CreateControl<RectInt>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.Bounds: return CreateControl<Bounds>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.BoundsInt: return CreateControl<BoundsInt>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.Character: return CreateControl<char>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.AnimationCurve: return CreateControl<AnimationCurve>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.Gradient: return CreateControl<Gradient>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.Quaternion: return CreateControl<Quaternion>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.ExposedReference: return CreateControl<Object>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.FixedBufferSize: return CreateControl<int>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
				case SerializedPropertyType.ManagedReference: return CreateControl<object>(property, declaringType, method, triggerInPlayMode, triggerInEditor);
			}

			Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath, this.GetFieldType().Name);
			return null;
		}

		private PropertyWatcher CreateControl<T>(SerializedProperty property, Type declaringType, string method, bool triggerInPlayMode, bool triggerInEditor)
		{
			var none = ReflectionHelper.CreateActionCallback(method, declaringType, property);
			if (none != null)
			{
				return new ChangeTrigger<T>(property, (_, oldValue, newValue) =>
				{
					if ((triggerInPlayMode && EditorApplication.isPlaying) ||
						(triggerInEditor && !EditorApplication.isPlaying))
					{
						none();
					}
				});
			}
			else
			{
				var one = ReflectionHelper.CreateActionCallback<T>(method, declaringType, property);
				if (one != null)
				{
					return new ChangeTrigger<T>(property, (_, oldValue, newValue) =>
					{
						if ((triggerInPlayMode && EditorApplication.isPlaying) ||
							(triggerInEditor && !EditorApplication.isPlaying))
						{
							one(newValue);
						}
					});
				}
				else
				{
					var two = ReflectionHelper.CreateActionCallback<T, T>(method, declaringType, property);
					if (two != null)
					{
						return new ChangeTrigger<T>(property, (_, oldValue, newValue) =>
						{
							if ((triggerInPlayMode && EditorApplication.isPlaying) ||
								(triggerInEditor && !EditorApplication.isPlaying))
							{
								two(oldValue, newValue);
							}
						});
					}
				}
			}

			Debug.LogWarningFormat(_invalidMethodWarning, property.propertyPath, method, typeof(T).Name);
			return null;
		}
	}
}