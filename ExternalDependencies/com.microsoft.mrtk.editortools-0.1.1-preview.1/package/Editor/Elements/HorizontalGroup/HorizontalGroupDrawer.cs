using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
    [CustomPropertyDrawer(typeof(HorizontalGroupAttribute))]
    public class HorizontalGroupDrawer : PropertyDrawer
    {
        public const string Stylesheet = "HorizontalGroupStyle.uss";
        public const string UssClassName = "uxtools-horizontal-group";
        public const string LabelUssClassName = UssClassName + "__label";
        public const string ContainerUssClassName = UssClassName + "__container";
        public const string ItemUssClassName = UssClassName + "__item";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var horizontal = attribute as HorizontalGroupAttribute;
            var parent = property.GetParent();

            VisualElement group = null;
            VisualElement container = null;

            foreach (var sibling in parent.Children())
            {
                var field = fieldInfo.DeclaringType.GetField(sibling.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (field != null && field.TryGetAttribute<HorizontalGroupAttribute>(out var groupAttribute) && groupAttribute.Name == horizontal.Name)
                {
                    if (container != null)
                    {
                        // this is a subsequent property so draw it and add it to the list
                        var element = this.CreateNextElement(sibling);//PropertyDrawerExtensions.CreateNextElement(field, groupAttribute, sibling);
                        element.AddToClassList(ItemUssClassName);
                        container.Add(element);
                    }
                    else if (SerializedProperty.EqualContents(property, sibling))
                    {
                        // this property is first and is responsible for drawing
                        group = new VisualElement() { name = groupAttribute.Name };
                        group.AddToClassList(UssClassName);
                        group.AddStyleSheet(Stylesheet);

                        container = new VisualElement();
                        container.AddToClassList(ContainerUssClassName);

                        var label = new Label(groupAttribute.Name);
                        label.AddToClassList(LabelUssClassName);

                        var element = this.CreateNextElement(sibling);
                        element.AddToClassList(ItemUssClassName);

                        group.Add(label);
                        group.Add(container);
                        container.Add(element);
                    }
                    else
                    {
                        // a different property was first and handled the drawing
                        break;
                    }
                }
            }

            return group ?? new VisualElement();
        }
    }
}