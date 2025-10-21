
namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class IncrementerAttribute : PropertyTraitAttribute
	{
		public float Increment { get; private set; }

		public string IncrementSource { get; private set; }
		public bool AutoUpdate { get; private set; }

		public IncrementerAttribute(float increment = 1) : base(ControlPhase, InPhaseOrder)
		{
			Increment = increment;
		}

		public IncrementerAttribute(string incrementSource, bool autoUpdate = true) : base(ControlPhase, InPhaseOrder)
		{
			IncrementSource = incrementSource;
			AutoUpdate = autoUpdate;
		}
	}
}