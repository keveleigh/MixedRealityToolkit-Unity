using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class IncrementerIntField : IncrementerField<int>
	{
		#region Defaults

		public const int DefaultIncrement = 0;

		#endregion

		#region Public Interface

		public IncrementerIntField() : this(null)
		{
		}

		public IncrementerIntField(string label) : base(label, new IncrementerIntControl())
		{
		}

		public IncrementerIntField(string label, int increment) : this(label)
		{
			Increment = increment;
		}

		public IncrementerIntField(int increment) : this(null, increment)
		{
		}

		#endregion

		#region Visual Input

		private class IncrementerIntControl : IncrementerControl
		{
			public override int Increment { get; set; }

			private readonly IconButton _decrement;
			private readonly IconButton _increment;
			private readonly IntegerField _text;

			public IncrementerIntControl()
			{
				_decrement = new IconButton(() => _text.value -= Increment);
				_decrement.AddToClassList(DecrementUssClassName);
				_decrement.SetIcon(Icon.Remove);

				_text = new IntegerField();
				_text.AddToClassList(TextUssClassName);

				_increment = new IconButton(() => _text.value += Increment);
				_increment.AddToClassList(IncrementUssClassName);
				_increment.SetIcon(Icon.Add);

				Add(_decrement);
				Add(_text);
				Add(_increment);
			}

			public override void SetValueWithoutNotify(int value)
			{
				_text.SetValueWithoutNotify(value);
			}
		}

		#endregion

		#region UXML Support


		public new class UxmlFactory : UxmlFactory<IncrementerIntField, UxmlTraits> { }
		public new class UxmlTraits : UxmlTraits<UxmlIntAttributeDescription>
		{
			public UxmlTraits()
			{
				_increment.defaultValue = DefaultIncrement;
			}
		}

		#endregion
	}
}
