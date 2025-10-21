
namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class MaximumAttribute : PropertyTraitAttribute
	{
		public const int Order = 1;

		public float Maximum { get; private set; }
		public string MaximumSource { get; private set; }

		public MaximumAttribute(float maximum) : base(ValidatePhase, BeforePhaseOrder)
		{
			Maximum = maximum;
		}

		public MaximumAttribute(int maximum) : base(ValidatePhase, BeforePhaseOrder)
		{
			Maximum = maximum;
		}

		public MaximumAttribute(string maximumSource) : base(ValidatePhase, BeforePhaseOrder)
		{
			MaximumSource = maximumSource;
		}
	}
}