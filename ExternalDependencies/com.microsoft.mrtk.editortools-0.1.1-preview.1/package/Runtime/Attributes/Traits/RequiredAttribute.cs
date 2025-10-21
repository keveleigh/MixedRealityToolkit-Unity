
namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class RequiredAttribute : PropertyTraitAttribute
	{
		public string Message { get; private set; }
		public MessageBoxType Type { get; private set; }

		public RequiredAttribute(string message, MessageBoxType type = MessageBoxType.Warning) : base(ValidatePhase, InPhaseOrder)
		{
			Message = message;
			Type = type;
		}
	}
}