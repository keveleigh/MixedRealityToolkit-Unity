
namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public enum ButtonIcon
	{
		Add,
		CustomAdd,
		Remove,
		Inspect,
		Expanded,
		Collapsed,
		Refresh,
		Reset,
		Load,
		Unload,
		Close,
		LeftArrow,
		RightArrow,
		Info,
		Warning,
		Error,
		Settings,
		View,
		Locked,
		Unlocked
	}

	public class ButtonAttribute : PropertyTraitAttribute
	{
		public string Method { get; private set; }
		public string Label { get; private set; }
		public ButtonIcon Icon { get; private set; }

		public string Tooltip { get; set; }
		public TraitLocation Location { get; set; }

		public ButtonAttribute(string method, string label) : base(PerContainerPhase, InPhaseOrder)
		{
			Method = method;
			Label = label;
		}

		public ButtonAttribute(string method, ButtonIcon icon) : base(PerContainerPhase, InPhaseOrder)
		{
			Method = method;
			Icon = icon;
		}
	}
}