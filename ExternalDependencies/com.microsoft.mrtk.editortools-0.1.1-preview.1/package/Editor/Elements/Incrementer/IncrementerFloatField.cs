using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class IncrementerFloatField : IncrementerField<float>
	{
		#region Defaults

		public const float DefaultIncrement = 1.0f;

		#endregion

		#region Public Interface

		public IncrementerFloatField() : this(null)
		{
		}

		public IncrementerFloatField(string label) : base(label, new IncrementerFloatControl())
		{
		}

		public IncrementerFloatField(string label, float increment) : this(label)
		{
			Increment = increment;
		}

		public IncrementerFloatField(float increment) : this(null, increment)
		{
		}

		#endregion

		#region Visual Input

		private class IncrementerFloatControl : IncrementerControl
		{
			public override float Increment { get; set; }

			private readonly IconButton _decrement;
			private readonly IconButton _increment;
			private readonly FloatField _text;

			public IncrementerFloatControl()
			{
				_decrement = new IconButton(() => _text.value -= Increment);
				_decrement.AddToClassList(DecrementUssClassName);
				_decrement.SetIcon(Icon.Remove);

				_text = new FloatField();
				_text.AddToClassList(TextUssClassName);

				_increment = new IconButton(() => _text.value += Increment);
				_increment.AddToClassList(IncrementUssClassName);
				_increment.SetIcon(Icon.Add);

				Add(_decrement);
				Add(_text);
				Add(_increment);
			}

			public override void SetValueWithoutNotify(float value)
			{
				_text.SetValueWithoutNotify(value);
			}
		}

		#endregion

		#region UXML Support

		public new class UxmlFactory : UxmlFactory<IncrementerFloatField, UxmlTraits> { }
		public new class UxmlTraits : UxmlTraits<UxmlFloatAttributeDescription>
		{
			public UxmlTraits()
			{
				_increment.defaultValue = DefaultIncrement;
			}
		}

		#endregion
	}
}
