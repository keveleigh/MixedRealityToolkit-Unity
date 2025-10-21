using System;
using System.Reflection;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public static class MemberInfoExtensions
	{
		public static bool HasAttribute<AttributeType>(this MemberInfo member) where AttributeType : Attribute
		{
			return member.GetCustomAttribute<AttributeType>() != null;
		}

		public static AttributeType GetAttribute<AttributeType>(this MemberInfo member) where AttributeType : Attribute
		{
			return member.TryGetAttribute<AttributeType>(out var attribute) ? attribute : null;
		}

		public static bool TryGetAttribute<AttributeType>(this MemberInfo member, out AttributeType attribute) where AttributeType : Attribute
		{
			var attributes = member.GetCustomAttributes(typeof(AttributeType), false);
			attribute = attributes != null && attributes.Length > 0 ? attributes[0] as AttributeType : null;

			return attribute != null;
		}

		public static bool HasAttribute(this MemberInfo member, Type attribute)
		{
			return member.GetCustomAttribute(attribute) != null;
		}

		public static Attribute GetAttribute(MemberInfo member, Type attributeType)
		{
			return member.TryGetAttribute(attributeType, out var attribute) ? attribute : null;
		}

		public static bool TryGetAttribute(this MemberInfo member, Type attributeType, out Attribute attribute)
		{
			var attributes = member.GetCustomAttributes(attributeType, false);
			attribute = attributes != null && attributes.Length > 0 ? attributes[0] as Attribute : null;

			return attribute != null;
		}
	}
}