using UnityEngine.UIElements;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public abstract class IncrementerField<ValueType> : BaseField<ValueType>
	{
		#region Class Names

		public const string Stylesheet = "IncrementerStyle.uss";
		public const string UssClassName = "uxtools-incrementer-field";
		public const string LabelUssClassName = UssClassName + "__label";
		public const string InputUssClassName = UssClassName + "__input";

		#endregion

		#region Members

		protected readonly IncrementerControl _control;

		#endregion

		#region Public Interface

		public ValueType Increment
		{
			get => _control.Increment;
			set => _control.Increment = value;
		}

		protected IncrementerField(string label, IncrementerControl control) : base(label, control)
		{
			_control = control;
			_control.AddToClassList(InputUssClassName);
			_control.RegisterCallback<ChangeEvent<ValueType>>(evt =>
			{
				base.value = evt.newValue;
				evt.StopImmediatePropagation();
			});

			labelElement.AddToClassList(LabelUssClassName);

			AddToClassList(UssClassName);
			this.AddStyleSheet(Stylesheet);
		}

		public override void SetValueWithoutNotify(ValueType newValue)
		{
			base.SetValueWithoutNotify(newValue);
			_control.SetValueWithoutNotify(newValue);
		}

		#endregion

		#region Visual Input

		protected abstract class IncrementerControl : VisualElement
		{
			public const string DecrementUssClassName = InputUssClassName + "__decrement";
			public const string TextUssClassName = InputUssClassName + "__text";
			public const string IncrementUssClassName = InputUssClassName + "__increment";

			public abstract ValueType Increment { get; set; }

			public abstract void SetValueWithoutNotify(ValueType value);
		}

		#endregion

		#region UXML Support

		public abstract class UxmlTraits<AttributeType> : BaseFieldTraits<ValueType, AttributeType> where AttributeType : TypedUxmlAttributeDescription<ValueType>, new()
		{
			protected readonly AttributeType _increment = new AttributeType { name = "increment" };

			public override void Init(VisualElement element, IUxmlAttributes bag, CreationContext cc)
			{
				var field = element as IncrementerField<ValueType>;
				field.Increment = _increment.GetValueFromBag(bag, cc);

				base.Init(element, bag, cc);
			}
		}

		#endregion
	}
}
