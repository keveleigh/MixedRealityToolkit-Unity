
namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class PlaceholderAttribute : PropertyTraitAttribute
	{
		public string Text { get; private set; }
		public string TextSource { get; private set; }
		public bool AutoUpdate { get; private set; }

		public PlaceholderAttribute(string text) : base(FieldPhase, InPhaseOrder)
		{
			Text = text;
		}

		public PlaceholderAttribute(string textSource, bool autoUpdate) : base(FieldPhase, InPhaseOrder)
		{
			TextSource = textSource;
			AutoUpdate = autoUpdate;
		}
	}
}