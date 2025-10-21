
namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class EnumButtonsAttribute : PropertyTraitAttribute
	{
		public bool? Flags { get; private set; }

		public EnumButtonsAttribute() : base(ControlPhase, InPhaseOrder) => Flags = null;
		public EnumButtonsAttribute(bool flags) : base(ControlPhase, InPhaseOrder) => Flags = flags;
	}
}