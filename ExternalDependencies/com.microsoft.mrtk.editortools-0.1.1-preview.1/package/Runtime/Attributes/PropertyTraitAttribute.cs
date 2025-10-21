using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public enum TraitLocation
	{
		Above,
		Below,
		Left,
		Right
	}

	public abstract class PropertyTraitAttribute : PropertyAttribute
	{
		// Controls the order at which traits are drawn from low to high
		public const int TestPhase = 1; // Testing prior to any drawing
		public const int PerContainerPhase = 2; // Adjusting the drawing of a container
		public const int ContainerPhase = 3; // Drawing of a container field
		public const int FieldPhase = 4; // Adjusting how the field is drawn
		public const int ValidatePhase = 5; // Validating drawing of the field
		public const int ControlPhase = 6; // Actual drawing of the field

		// For fine grained control per phase
		public const int BeforePhaseOrder = -1;
		public const int InPhaseOrder = 0;
		public const int AfterPhaseOrder = 1;

		protected PropertyTraitAttribute(int drawPhase, int drawOrder)
		{
#if UNITY_2021_1_OR_NEWER
			order = int.MinValue + (drawPhase * 1000 + drawOrder);
#else
			order = int.MaxValue - (drawPhase * 1000 + drawOrder);
#endif
		}
	}
}