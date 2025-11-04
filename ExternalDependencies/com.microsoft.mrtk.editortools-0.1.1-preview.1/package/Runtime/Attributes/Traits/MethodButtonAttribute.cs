using System;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MethodButtonAttribute : Attribute
	{
		public string Label { get; set; }
		public string Tooltip { get; set; }
	}
}