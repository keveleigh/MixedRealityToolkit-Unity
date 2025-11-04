
namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public enum DisableIfNumber
	{
		IsEqual,
		IsInequal,
		IsLessThan,
		IsGreaterThan,
		IsLessThanOrEqual,
		IsGreaterThanOrEqual
	}

	public enum DisableIfBool
	{
		IsTrue,
		IsFalse
	}

	public enum DisableIfObject
	{
		IsSet,
		IsNotSet
	}

	public enum DisableIfString
	{
		IsSame,
		IsDifferent
	}

	public enum DisableIfEnum
	{
		IsEqual,
		IsInequal
	}

	public class DisableAttribute : PropertyTraitAttribute
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
		public DisableIfNumber NumberTest { get; private set; }

		public string StringValue { get; private set; }
		public DisableIfString StringTest { get; private set; }

		public DisableIfBool BoolTest { get; private set; }
		public DisableIfEnum EnumTest { get; private set; }
		public DisableIfObject ObjectTest { get; private set; }

		public DisableAttribute(string valueSource, int intValue, DisableIfNumber test) : base(TestPhase, InPhaseOrder)
		{
			ValueSource = valueSource;
			IntValue = intValue;
			NumberTest = test;
			Type = TestType.Int;
		}

		public DisableAttribute(string valueSource, float floatValue, DisableIfNumber test) : base(TestPhase, InPhaseOrder)
		{
			ValueSource = valueSource;
			FloatValue = floatValue;
			NumberTest = test;
			Type = TestType.Float;
		}

		public DisableAttribute(string valueSource, string stringValue, DisableIfString test) : base(TestPhase, InPhaseOrder)
		{
			ValueSource = valueSource;
			StringValue = stringValue;
			StringTest = test;
			Type = TestType.String;
		}

		public DisableAttribute(string valueSource, DisableIfBool test) : base(TestPhase, InPhaseOrder)
		{
			ValueSource = valueSource;
			BoolTest = test;
			Type = TestType.Bool;
		}

		public DisableAttribute(string valueSource, int valueAsInt, DisableIfEnum test) : base(TestPhase, InPhaseOrder)
		{
			ValueSource = valueSource;
			IntValue = valueAsInt;
			EnumTest = test;
			Type = TestType.Enum;
		}

		public DisableAttribute(string valueSource, DisableIfObject test) : base(TestPhase, InPhaseOrder)
		{
			ValueSource = valueSource;
			ObjectTest = test;
			Type = TestType.Object;
		}
	}
}