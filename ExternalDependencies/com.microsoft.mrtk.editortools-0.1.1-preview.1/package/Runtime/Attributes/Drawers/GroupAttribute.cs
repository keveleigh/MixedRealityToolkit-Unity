
namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class GroupAttribute : PropertyTraitAttribute
	{
		public string Name { get; private set; }

		public GroupAttribute(string name, int drawOrder = InPhaseOrder) : base(ContainerPhase, drawOrder)
		{
			Name = name;
		}
	}
}
