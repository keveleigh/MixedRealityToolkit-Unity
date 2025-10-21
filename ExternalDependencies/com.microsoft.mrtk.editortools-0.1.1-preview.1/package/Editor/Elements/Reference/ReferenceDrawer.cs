using UnityEditor;
using UnityEngine.UIElements;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	[CustomPropertyDrawer(typeof(ReferenceAttribute))]
	public class ReferenceDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var type = this.GetFieldType();
			var next = this.GetNextDrawer();
			var drawer = new PropertyReferenceDrawer(property, next);
			var field = new ReferenceField(type, drawer)
			{
				bindingPath = property.propertyPath // TODO: other stuff from ConfigureField
			};

			return field;
		}
	}
}
