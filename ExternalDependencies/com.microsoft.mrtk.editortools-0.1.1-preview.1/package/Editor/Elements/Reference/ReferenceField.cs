using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public interface IReferenceDrawer
	{
		VisualElement CreateElement(object value);
	}

	public class ReferenceField : BindableElement, INotifyValueChanged<object>
	{
		#region Errors

		private const string _invalidBindingError = "Invalid binding '{0}' for ReferenceField: property '{1}' is type '{2}' but should be type 'ManagedReference'";

		#endregion

		#region Class Names

		public const string Stylesheet = "Reference.uss";
		public const string UssClassName = "uxtools-reference-field";
		public const string FoldoutUssClassName = UssClassName + "__foldout";
		public const string SetUssClassName = UssClassName + "--set";
		public const string NullUssClassName = UssClassName + "--null";
		public const string MissingUssClassName = UssClassName + "--missing";
		public const string SetButtonUssClassName = UssClassName + "__set-button";
		public const string ClearButtonUssClassName = UssClassName + "__clear-button";

		#endregion

		#region Icons

		private static readonly Icon _setIcon = Icon.CustomAdd;
		private static readonly Icon _clearIcon = Icon.Reset;

		#endregion

		#region Labels

		private const string _setButtonLabel = "Select type";
		private const string _clearButtonLabel = "Reset to null";

		#endregion

		#region Members

		private string _label;
		private Type _referenceType;
		private IReferenceDrawer _drawer;
		private object _value;

		private Foldout _foldout;
		private IconButton _setButton;
		private IconButton _clearButton;

		private class TypeProvider : PickerProvider<Type> { }
		private TypeProvider _typeProvider;

		#endregion

		#region Public Interface

		public string Label
		{
			get => _label;
			set => SetLabel(value);
		}

		public Type ReferenceType
		{
			get => _referenceType;
			set => SetReferenceType(value);
		}

		public IReferenceDrawer Drawer
		{
			get => _drawer;
			set => SetDrawer(value);
		}

		public object value
		{
			get => _value;
			set => SetValue(value);
		}

		public ReferenceField()
		{
			BuildUi();
		}

		public ReferenceField(string label) : this()
		{
			Label = label;
		}

		public ReferenceField(string label, Type referenceType) : this(label, referenceType, null)
		{
		}

		public ReferenceField(Type referenceType) : this(null, referenceType, null)
		{
		}

		public ReferenceField(Type referenceType, IReferenceDrawer drawer) : this(null, referenceType, drawer)
		{
		}

		public ReferenceField(string label, Type referenceType, IReferenceDrawer drawer) : this(label)
		{
			ReferenceType = referenceType;
			Drawer = drawer;
		}

		public void SetValueWithoutNotify(object value)
		{
			_value = value;
		}

		#endregion

		#region Property Setters

		private void SetLabel(string label)
		{
			_label = label;
			UpdateLabel();
		}

		private void SetValue(object value)
		{
			var previous = _value;

			if (!ReferenceEquals(previous, value))
			{
				SetValueWithoutNotify(value);
				this.SendChangeEvent(previous, value);
				Rebuild();
			}
		}

		private void SetReferenceType(Type type)
		{
			_referenceType = type;

			if (type != null)
			{
				var types = TypeHelper.GetTypeList(type, false);
				_typeProvider.Setup(type.Name, types.Paths, types.Types, GetIcon, SetType);
			}

			UpdateLabel();
		}

		private void SetDrawer(IReferenceDrawer drawer)
		{
			_drawer = drawer;
			Rebuild();
		}

		#endregion

		#region UI

		private void BuildUi()
		{
			_typeProvider = ScriptableObject.CreateInstance<TypeProvider>();

			_foldout = new Foldout();
			_foldout.AddToClassList(FoldoutUssClassName);

			_setButton = new IconButton(SelectType) { tooltip = _setButtonLabel };
			_setButton.AddToClassList(SetButtonUssClassName);
			_setButton.SetIcon(_setIcon);

			_clearButton = new IconButton(SetNull) { tooltip = _clearButtonLabel };
			_clearButton.AddToClassList(ClearButtonUssClassName);
			_clearButton.SetIcon(_clearIcon);

			Add(_foldout);

			var foldoutToggle = _foldout.Q(className: Foldout.toggleUssClassName);
			foldoutToggle.Add(_setButton);
			foldoutToggle.Add(_clearButton);

			AddToClassList(UssClassName);
			this.AddStyleSheet(Stylesheet);
		}

		private void Rebuild()
		{
			UpdateLabel();

			_foldout.Clear();

			if (_drawer != null)
			{
				var content = _drawer.CreateElement(_value);
				_foldout.Add(content);
			}
		}

		private void UpdateLabel()
		{
			var type = _value?.GetType() ?? _referenceType;

			_foldout.text = _label != null && type != null
				? $"{_label} ({type.Name})"
				: _label;

			EnableInClassList(SetUssClassName, _value != null);
			EnableInClassList(NullUssClassName, _value == null);
			EnableInClassList(MissingUssClassName, type == null);
		}

		#endregion

		#region Type Management

		private void SelectType()
		{
			var position = new Vector2(_setButton.worldBound.center.x, _setButton.worldBound.yMax + _setButton.worldBound.height * 0.5f);
			SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(position)), _typeProvider);
		}

		private Texture GetIcon(Type type)
		{
			return AssetPreview.GetMiniTypeThumbnail(type);
		}

		private void SetType(Type selected)
		{
			value = Activator.CreateInstance(selected);
			_foldout.value = true;
		}

		private void SetNull()
		{
			value = null;
		}

		#endregion

		#region Binding

		protected override void ExecuteDefaultActionAtTarget(EventBase evt)
		{
			base.ExecuteDefaultActionAtTarget(evt);

			if (this.TryGetPropertyBindEvent(evt, out var property))
			{
				if (property.propertyType == SerializedPropertyType.ManagedReference)
				{
					BindingExtensions.CreateBind(_foldout, property, GetExpandedProperty, SetExpandedProperty, CompareExpandedProperties);

					if (_label == null)
						Label = property.displayName;

					if (ReferenceType == null)
						ReferenceType = property.GetManagedReferenceFieldType();

					if (Drawer == null)
						Drawer = new PropertyReferenceDrawer(property, null);

					BindingExtensions.BindManagedReference(this, property, Rebuild);
				}
				else
				{
					Debug.LogErrorFormat(_invalidBindingError, bindingPath, property.propertyPath, property.propertyType);
				}

				evt.StopPropagation();
			}
		}

		private bool GetExpandedProperty(SerializedProperty property)
		{
			return property.isExpanded;
		}

		private void SetExpandedProperty(SerializedProperty property, bool value)
		{
			property.isExpanded = value;
		}

		private bool CompareExpandedProperties(bool value, SerializedProperty property, Func<SerializedProperty, bool> getter)
		{
			var currentValue = getter(property);
			return value == currentValue;
		}

		#endregion

		#region UXML

		public new class UxmlFactory : UxmlFactory<ReferenceField, UxmlTraits> { }
		public new class UxmlTraits : BindableElement.UxmlTraits
		{
			private readonly UxmlStringAttributeDescription _label = new UxmlStringAttributeDescription { name = "label" };
			private readonly UxmlStringAttributeDescription _type = new UxmlStringAttributeDescription { name = "type" };

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);

				var field = ve as ReferenceField;

				field.Label = _label.GetValueFromBag(bag, cc);

				var typeName = _type.GetValueFromBag(bag, cc);

				if (!string.IsNullOrEmpty(typeName))
					field.ReferenceType = Type.GetType(typeName, false);
			}
		}

		#endregion
	}
}
