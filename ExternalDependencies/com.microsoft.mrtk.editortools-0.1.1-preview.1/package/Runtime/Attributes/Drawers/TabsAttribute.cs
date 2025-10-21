
namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class TabsAttribute : PropertyTraitAttribute
	{
		public string Group { get; private set; }
		public string Tab { get; private set; }

		public TabsAttribute(string group, string tab, int drawOrder = InPhaseOrder) : base(ContainerPhase, drawOrder)
		{
			Group = group;
			Tab = tab;
		}
	}
}