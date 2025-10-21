using UnityEditor;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class BoolPreference
	{
		private readonly string _name;
		private readonly bool _default;

		public static implicit operator bool(BoolPreference preference) => preference.Value;

		public BoolPreference(string name, bool defaultValue)
		{
			_name = name;
			_default = defaultValue;
		}

		public bool Value
		{
			get => EditorPrefs.GetBool(_name, _default);
			set => EditorPrefs.SetBool(_name, value);
		}
	}

	public class IntPreference
	{
		public static implicit operator int(IntPreference preference) => preference.Value;

		private readonly string _name;
		private readonly int _default;

		public IntPreference(string name, int defaultValue)
		{
			_name = name;
			_default = defaultValue;
		}

		public int Value
		{
			get => EditorPrefs.GetInt(_name, _default);
			set => EditorPrefs.SetInt(_name, value);
		}
	}

	public class FloatPreference
	{
		public static implicit operator float(FloatPreference preference) => preference.Value;

		private readonly string _name;
		private readonly float _default;

		public FloatPreference(string name, float defaultValue)
		{
			_name = name;
			_default = defaultValue;
		}

		public float Value
		{
			get => EditorPrefs.GetFloat(_name, _default);
			set => EditorPrefs.SetFloat(_name, value);
		}
	}

	public class StringPreference
	{
		private readonly string _name;
		private readonly string _default;

		public static implicit operator string(StringPreference preference) => preference.Value;

		public StringPreference(string name, string defaultValue)
		{
			_name = name;
			_default = defaultValue;
		}

		public string Value
		{
			get => EditorPrefs.GetString(_name, _default);
			set => EditorPrefs.SetString(_name, value);
		}
	}

	public class JsonPreference<T>
	{
		private readonly string _name;
		private readonly string _default;

		public static implicit operator T(JsonPreference<T> preference) => preference.Value;

		public JsonPreference(string name)
		{
			_name = name;
			_default = "{}";
		}

		public T Value
		{
			get
			{
				var json = EditorPrefs.GetString(_name, _default);
				var state = JsonUtility.FromJson<T>(json);
				return state;
			}
			set
			{
				var json = JsonUtility.ToJson(value);
				EditorPrefs.SetString(_name, json);
			}
		}
	}
}
