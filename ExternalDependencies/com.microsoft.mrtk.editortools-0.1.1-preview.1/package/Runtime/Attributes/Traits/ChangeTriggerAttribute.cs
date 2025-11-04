
namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class ChangeTriggerAttribute : PropertyTraitAttribute
	{
		public string Method { get; private set; }
		public bool TriggerInPlaymode { get; private set; }
		public bool TriggerInEditor { get; private set; }

		public ChangeTriggerAttribute(string method, bool triggerInPlayMode = true, bool triggerInEditor = true) : base(FieldPhase, BeforePhaseOrder)
		{
			Method = method;
			TriggerInPlaymode = triggerInPlayMode;
			TriggerInEditor = triggerInEditor;
		}
	}
}