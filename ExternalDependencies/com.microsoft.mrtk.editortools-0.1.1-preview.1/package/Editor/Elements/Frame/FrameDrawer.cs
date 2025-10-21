using UnityEditor;
using UnityEngine.UIElements;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	[CustomPropertyDrawer(typeof(FrameAttribute))]
	class FrameDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var frameAttribute = attribute as FrameAttribute;
			var nameProp = property.FindPropertyRelative("name");
			var frame = new Frame
			{
				IsCollapsable = frameAttribute.IsCollapsable,
				bindingPath = property.propertyPath,
				Label = nameProp != null ? nameProp.stringValue : property.displayName,
			};

			// TODO: other stuff from ConfigureField

			return frame;
		}
	}
}
