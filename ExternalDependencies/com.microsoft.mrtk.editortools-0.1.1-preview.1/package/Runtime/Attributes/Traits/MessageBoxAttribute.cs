
namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public enum MessageBoxType
	{
		Info,
		Warning,
		Error
	}

	public class MessageBoxAttribute : PropertyTraitAttribute
	{
		public string Message { get; private set; }
		public MessageBoxType Type { get; private set; }
		public TraitLocation Location { get; set; }

		public MessageBoxAttribute(string message, MessageBoxType type) : base(PerContainerPhase, InPhaseOrder)
		{
			Message = message;
			Type = type;
		}
	}
}