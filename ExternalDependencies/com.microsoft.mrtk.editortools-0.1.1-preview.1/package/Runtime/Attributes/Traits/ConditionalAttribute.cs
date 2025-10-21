
namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public enum ShowIfNumber
	{
		IsEqual,
		IsInequal,
		IsLessThan,
		IsGreaterThan,
		IsLessThanOrEqual,
		IsGreaterThanOrEqual
	}

	public enum ShowIfBool
	{
		IsTrue,
		IsFalse
	}

	public enum ShowIfObject
	{
		IsSet,
		IsNotSet
	}

	public enum ShowIfString
	{
		IsSame,
		IsDifferent
	}

	public enum ShowIfEnum
	{
		IsEqual,
		IsInequal
	}

	public class ConditionalAttribute : PropertyTraitAttribute
	{
		public enum TestType
		{
			Bool,
			Int,
			Float,
			String,
			Enum,
			Object
		}

		public string ValueSource { get; private set; }
		public TestType Type { get; private set; }

		public int IntValue { get; private set; }
		public float FloatValue { get; private set; }
		public ShowIfNumber NumberTest { get; private set; }

		public string StringValue { get; private set; }
		public ShowIfString StringTest { get; private set; }

		public ShowIfBool BoolTest { get; private set; }
		public ShowIfEnum EnumTest { get; private set; }
		public ShowIfObject ObjectTest { get; private set; }

		public ConditionalAttribute(string valueSource, int intValue, ShowIfNumber test) : base(TestPhase, InPhaseOrder)
		{
			ValueSource = valueSource;
			IntValue = intValue;
			NumberTest = test;
			Type = TestType.Int;
		}

		public ConditionalAttribute(string valueSource, float floatValue, ShowIfNumber test) : base(TestPhase, InPhaseOrder)
		{
			ValueSource = valueSource;
			FloatValue = floatValue;
			NumberTest = test;
			Type = TestType.Float;
		}

		public ConditionalAttribute(string valueSource, string stringValue, ShowIfString test) : base(TestPhase, InPhaseOrder)
		{
			ValueSource = valueSource;
			StringValue = stringValue;
			StringTest = test;
			Type = TestType.String;
		}

		public ConditionalAttribute(string valueSource, ShowIfBool test) : base(TestPhase, InPhaseOrder)
		{
			ValueSource = valueSource;
			BoolTest = test;
			Type = TestType.Bool;
		}

		public ConditionalAttribute(string valueSource, int valueAsInt, ShowIfEnum test) : base(TestPhase, InPhaseOrder)
		{
			ValueSource = valueSource;
			IntValue = valueAsInt;
			EnumTest = test;
			Type = TestType.Enum;
		}

		public ConditionalAttribute(string valueSource, ShowIfObject test) : base(TestPhase, InPhaseOrder)
		{
			ValueSource = valueSource;
			ObjectTest = test;
			Type = TestType.Object;
		}
	}
}