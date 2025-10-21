using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class TypeList
	{
		public TypeList(Type baseType, List<Type> types)
		{
			BaseType = baseType;
			Types = types;
			Paths = types.Select(type => type.Name).ToList();
		}

		public Type BaseType { get; private set; }
		public List<Type> Types { get; private set; }
		public List<string> Paths { get; private set; }
	}

	public static class TypeHelper
	{
		private static readonly Dictionary<string, TypeList> _derivedTypeLists = new Dictionary<string, TypeList>();

		#region Listing

		public static IEnumerable<Type> GetDerivedTypes<BaseType>(bool includeAbstract)
		{
			return typeof(BaseType).GetDerivedTypes(includeAbstract);
		}

		public static IEnumerable<Type> GetTypesWithAttribute<AttributeType>() where AttributeType : Attribute
		{
			return TypeCache.GetTypesWithAttribute<AttributeType>();
		}

		public static IEnumerable<Type> GetTypesWithAttribute(Type attributeType)
		{
			return TypeCache.GetTypesWithAttribute(attributeType);
		}

		public static TypeList GetTypeList<T>(bool includeAbstract)
		{
			return GetTypeList(typeof(T), includeAbstract);
		}

		public static TypeList GetTypeList(Type baseType, bool includeAbstract)
		{
			// include the settings in the name so lists of the same type can be created with different settings
			var listName = string.Format("{0}-{1}", includeAbstract, baseType.AssemblyQualifiedName);

			if (!_derivedTypeLists.TryGetValue(listName, out var typeList))
			{
				var types = baseType.GetDerivedTypes(includeAbstract).OrderBy(type => type.Name);
				typeList = new TypeList(baseType, types.ToList());

				_derivedTypeLists.Add(listName, typeList);
			}

			return typeList;
		}

		#endregion

		#region Utility

		public static Type FindType(string name)
		{
			// search with normal rules
			var type = Type.GetType(name);

			// search in default runtime assembly
			if (type == null)
				type = Type.GetType($"{name}, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

			// search in default editor assembly
			if (type == null)
				type = Type.GetType($"{name}, Assembly-CSharp-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

			// search in Unity
			if (type == null)
				type = typeof(Object).Assembly.GetType(name);

			return type;
		}

		#endregion
	}
}
