using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
    /// <summary>
    /// Create a class specific CustomEditor and derive from this class in order take advantage of UXTools property attributes.
    /// This must exist until Unity supports by default using the VisualElement based API for CustomPropertyDrawers. Until that happens,
    /// a "No GUI Implemented" label will display if UXTools attributes are used without this class. If you would like to globally use
    /// this class as a fallback editor, you can create a csc.rsp file in the Assets folder and add "-define:ENABLE_FALLBACK_EDITORTOOLS_EDITOR"
    /// to it.   Microsoft.MixedReality.Toolkit.EditorTools
    /// </summary>
#if ENABLE_FALLBACK_EDITORTOOLS_EDITOR
	[CanEditMultipleObjects]
	[CustomEditor(typeof(UnityEngine.Object), true, isFallback = true)]
#endif
    public class MRTKEditorBase : Editor
	{
		public override VisualElement CreateInspectorGUI()
		{
			var container = new VisualElement();

			var iterator = serializedObject.GetIterator();
			if (iterator.NextVisible(true))
			{
				do
				{
					var propertyField = new PropertyField(iterator.Copy()) { name = $"PropertyField: {iterator.propertyPath}" };
	
					if (iterator.propertyPath == "m_Script" && serializedObject.targetObject != null)
						propertyField.SetEnabled(false);
	
					container.Add(propertyField);
				}
				while (iterator.NextVisible(false));
			}

			var methods = target
				.GetType()
				.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic)
				.Where(method => method.HasAttribute<MethodButtonAttribute>());

			foreach (var method in methods)
			{
				if (method.HasSignature(null, null))
				{
					var attribute = method.GetCustomAttribute<MethodButtonAttribute>();

					Action callback = method.IsStatic ?
						() => method.Invoke(null, null) :
						() => method.Invoke(target, null);

					var button = new Button(callback)
					{
						text = string.IsNullOrEmpty(attribute.Label) ? method.Name : attribute.Label,
						tooltip = attribute.Tooltip
					};

					container.Add(button);
				}
				else
                {
					Debug.LogWarning($"Invalid method for MethodButtonAttribute: method {method.Name} must return void and be a parameterless.");
				}
			}

			return container;
		}
	}
}