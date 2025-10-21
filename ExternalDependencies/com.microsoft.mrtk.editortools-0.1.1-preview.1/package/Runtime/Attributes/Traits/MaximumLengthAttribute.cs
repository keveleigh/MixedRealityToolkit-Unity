
namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class MaximumLengthAttribute : PropertyTraitAttribute
	{
		public int MaximumLength { get; private set; }
		public string MaximumLengthSource { get; private set; }
		public bool AutoUpdate { get; private set; }

		public MaximumLengthAttribute(int maximumLength) : base(ValidatePhase, BeforePhaseOrder)
		{
			MaximumLength = maximumLength;
		}

		public MaximumLengthAttribute(string maximumLengthSource, bool autoUpdate = true) : base(ValidatePhase, BeforePhaseOrder)
		{
			MaximumLengthSource = maximumLengthSource;
			AutoUpdate = autoUpdate;
		}
	}
}