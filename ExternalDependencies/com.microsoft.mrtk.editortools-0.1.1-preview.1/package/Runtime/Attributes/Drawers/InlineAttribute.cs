
namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public enum InlineDisplayMode
	{
		MembersOnly,
		ContainerOnly,
		MembersAndContainer,
		ContainerAsMember
	}
	public class InlineAttribute : PropertyTraitAttribute
	{
		public InlineDisplayMode DisplayMode { get; private set; }

		public InlineAttribute(InlineDisplayMode displayMode = InlineDisplayMode.MembersAndContainer) : base(ControlPhase, InPhaseOrder)
		{
			DisplayMode = displayMode;
		}
	}
}