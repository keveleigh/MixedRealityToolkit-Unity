
namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class FrameAttribute : PropertyTraitAttribute
	{
		public bool IsCollapsable = true;

		public FrameAttribute() : base(ControlPhase, InPhaseOrder)
		{
		}
	}
}
