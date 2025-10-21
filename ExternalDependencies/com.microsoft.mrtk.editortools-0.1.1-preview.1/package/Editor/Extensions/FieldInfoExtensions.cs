using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public static class FieldInfoExtensions
	{
		public static string GetTooltip(this FieldInfo field)
		{
			return field.GetCustomAttribute<TooltipAttribute>()?.tooltip ?? string.Empty;
		}

		public static bool IsSerializable(this FieldInfo field)
		{
			var included = field.IsPublic || field.GetAttribute<SerializeField>() != null;
			var excluded = field.GetAttribute<NonSerializedAttribute>() != null;
			var compatible = !field.IsStatic && !field.IsLiteral && !field.IsInitOnly && field.FieldType.IsSerializable();

			return included && !excluded && compatible;
		}

		public static Type GetFieldType(this FieldInfo fieldInfo)
		{
			var interfaces = fieldInfo.FieldType.GetInterfaces();

			var ilist = interfaces.FirstOrDefault(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>));

			if (ilist != null)
				return ilist.GetGenericArguments()[0];

			var iDict = interfaces.FirstOrDefault(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>));

			if (iDict != null)
				return iDict.GetGenericArguments()[1];

			return fieldInfo.FieldType;
		}
	}
}