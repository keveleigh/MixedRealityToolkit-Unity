
namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class ReadOnlyAttribute : PropertyTraitAttribute
	{
		public ReadOnlyAttribute() : base(PerContainerPhase, BeforePhaseOrder)
		{
		}
	}
}