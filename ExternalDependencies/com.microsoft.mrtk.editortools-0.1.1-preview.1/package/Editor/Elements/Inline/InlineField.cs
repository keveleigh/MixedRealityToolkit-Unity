using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class InlineField : VisualElement
	{
		public const string Stylesheet = "InlineStyle.uss";
		public const string UssClassName = "uxtools-inline";
		public const string ContainerUssClassName = UssClassName + "--show-container";
		public const string LabelUssClassName = UssClassName + "__label";
		public const string ChildrenUssClassName = UssClassName + "__children";

		public InlineField(SerializedProperty property, InlineDisplayMode displayMode)
		{
			var childContainer = new VisualElement();
			childContainer.AddToClassList(ChildrenUssClassName);

			if (displayMode == InlineDisplayMode.ContainerOnly || displayMode == InlineDisplayMode.MembersAndContainer)
			{
				var label = new FieldContainer(property.displayName);
				label.AddToClassList(LabelUssClassName);
				Add(label);
			}

			if (displayMode == InlineDisplayMode.MembersAndContainer)
			{
				AddToClassList(ContainerUssClassName);
			}

			foreach (var child in property.Children())
			{
				var field = new PropertyField(child);
				if (displayMode == InlineDisplayMode.ContainerOnly)
					field.SetFieldLabel(null);
				else if (displayMode == InlineDisplayMode.ContainerAsMember)
					field.SetFieldLabel(property.displayName);

				childContainer.Add(field);
			}

			Add(childContainer);
			AddToClassList(UssClassName);
			this.AddStyleSheet(Stylesheet);
		}
	}
}
