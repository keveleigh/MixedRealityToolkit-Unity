
namespace Microsoft.MixedReality.Toolkit.EditorTools
{
    public class HorizontalGroupAttribute : PropertyTraitAttribute
    {
        public string Name { get; private set; }

        public HorizontalGroupAttribute(string name, int drawOrder = InPhaseOrder) : base(ContainerPhase, drawOrder)
        {
            Name = name;
        }
    }
}
