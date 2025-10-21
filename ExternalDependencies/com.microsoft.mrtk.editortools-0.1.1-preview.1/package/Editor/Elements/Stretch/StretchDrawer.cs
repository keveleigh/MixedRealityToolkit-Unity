using UnityEditor;
using UnityEngine.UIElements;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	[CustomPropertyDrawer(typeof(StretchAttribute))]
	public class StretchDrawer : PropertyDrawer
	{
		public const string Stylesheet = "StretchStyle.uss";
		public const string UssClassName = "uxtools-stretch";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var element = this.CreateNextElement(property);
			element.AddToClassList(UssClassName);
			element.AddStyleSheet(Stylesheet);

			return element;
		}
	}
}