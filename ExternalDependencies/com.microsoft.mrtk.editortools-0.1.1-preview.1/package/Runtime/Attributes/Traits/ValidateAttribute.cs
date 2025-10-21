
namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class ValidateAttribute : PropertyTraitAttribute
	{
		public string Method { get; private set; }
		public string Message { get; private set; }
		public MessageBoxType Type { get; private set; }
		public TraitLocation Location { get; set; }

		public ValidateAttribute(string method, string message, MessageBoxType type = MessageBoxType.Warning) : base(ValidatePhase, InPhaseOrder)
		{
			Method = method;
			Type = type;
			Message = message;
		}
	}
}