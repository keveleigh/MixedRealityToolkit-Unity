
namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class CustomLabelAttribute : PropertyTraitAttribute
	{
		public string Label { get; private set; }
		public string LabelSource { get; private set; }
		public bool AutoUpdate { get; private set; }

		public CustomLabelAttribute(string label) : base(PerContainerPhase, InPhaseOrder)
		{
			Label = label;
		}

		public CustomLabelAttribute(string labelSource, bool autoUpdate) : base(PerContainerPhase, InPhaseOrder)
		{
			LabelSource = labelSource;
			AutoUpdate = autoUpdate;
		}
	}
}