using UnityEditor;
using UnityEngine.UIElements;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	[CustomPropertyDrawer(typeof(InlineAttribute))]
	public class InlineDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var inline = attribute as InlineAttribute;
			return new InlineField(property, inline.DisplayMode);
		}
	}
}