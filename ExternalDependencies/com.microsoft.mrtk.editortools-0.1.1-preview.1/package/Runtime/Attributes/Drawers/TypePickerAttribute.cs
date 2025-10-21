using System;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class TypePickerAttribute : PropertyTraitAttribute
	{
		public Type BaseType { get; private set; }
		public bool ShowAbstract { get; private set; }

		public TypePickerAttribute(Type baseType, bool showAbstract = false) : base(ControlPhase, InPhaseOrder)
		{
			BaseType = baseType;
			ShowAbstract = showAbstract;
		}
	}
}
