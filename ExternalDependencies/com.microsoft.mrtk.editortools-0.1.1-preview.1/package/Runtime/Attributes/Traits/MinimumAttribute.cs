
namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class MinimumAttribute : PropertyTraitAttribute
	{
		public float Minimum { get; private set; }
		public string MinimumSource { get; private set; }

		public MinimumAttribute(float minimum) : base(ValidatePhase, BeforePhaseOrder)
		{
			Minimum = minimum;
		}

		public MinimumAttribute(int minimum) : base(ValidatePhase, BeforePhaseOrder)
		{
			Minimum = minimum;
		}

		public MinimumAttribute(string maximumSource) : base(ValidatePhase, BeforePhaseOrder)
		{
			MinimumSource = maximumSource;
		}
	}
}