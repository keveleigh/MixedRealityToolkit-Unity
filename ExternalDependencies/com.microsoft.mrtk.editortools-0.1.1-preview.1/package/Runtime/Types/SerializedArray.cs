using System;
using System.Collections;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	// This class is nothing more than a direct wrapper of the built in Array class from the .net framework. Its purpose
	// is to provide a base class for array types to derive from that can then be targeted by an ArrayDisplay.

	[Serializable]
	public class SerializedArray<T> : ICloneable, IList, IStructuralComparable, IStructuralEquatable, ICollection, IEnumerable
	{
		[SerializeField]
		private T[] _items;

		public T[] Array => _items;
		public int Length => _items.Length;
        public bool IsFixedSize => _items.IsFixedSize;
        public bool IsReadOnly => _items.IsReadOnly;
        
		public SerializedArray(int count)
		{
			_items = new T[count];
		}

        public T this[int index]
		{
			get => _items[index];
			set => _items[index] = value;
		}

        public object Clone() => _items.Clone();
        public void CopyTo(Array array, int index) => _items.CopyTo(array, index);
        public IEnumerator GetEnumerator() => _items.GetEnumerator();
        
		#region ICollection Implementation

        int ICollection.Count => ((ICollection)_items).Count;
		bool ICollection.IsSynchronized => _items.IsSynchronized;
		object ICollection.SyncRoot => _items.SyncRoot;

		#endregion

		#region IComparable Implementation

		int IStructuralComparable.CompareTo(object other, IComparer comparer) => ((IStructuralComparable)_items).CompareTo(other, comparer);

		#endregion

		#region IStructuralEquatable Implementation

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) => ((IStructuralEquatable)_items).Equals(other, comparer);
		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) => ((IStructuralEquatable)_items).GetHashCode(comparer);

		#endregion

		#region IList Implementation

		int IList.Add(object value) => ((IList)_items).Add(value);
		void IList.Clear() => ((IList)_items).Clear();
		bool IList.Contains(object value) => ((IList)_items).Contains(value);
		int IList.IndexOf(object value) => ((IList)_items).IndexOf(value);
		void IList.Insert(int index, object value) => ((IList)_items).Insert(index, value);
		void IList.Remove(object value) => ((IList)_items).Remove(value);
		void IList.RemoveAt(int index) => ((IList)_items).RemoveAt(index);

        object IList.this[int index]
        {
            get => _items[index];
            set => ((IList)_items)[index] = value;
        }
        
		#endregion
    }
}
