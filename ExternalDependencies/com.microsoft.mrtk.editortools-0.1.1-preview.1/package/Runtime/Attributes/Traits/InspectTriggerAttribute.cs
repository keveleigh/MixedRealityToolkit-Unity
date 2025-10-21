
namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class InspectTriggerAttribute : PropertyTraitAttribute
	{
		public string Method { get; private set; }

		public InspectTriggerAttribute(string method) : base(TestPhase, InPhaseOrder)
		{
			Method = method;
		}
	}
}