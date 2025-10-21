using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	// This class is nothing more than a direct wrapper of the built in List class from the .net framework. Its purpose
	// is to provide a base class for list types to derive from that can then be targeted by a ListDisplay.
	[Serializable]
	public class ReferenceList<T> : ICollection<T>, IEnumerable<T>, IEnumerable, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection, IList
	{
		public const string ItemsProperty = nameof(_items);

		[SerializeReference]
		private List<T> _items = new();

		public List<T> List => _items;

		public int Count => _items.Count;

		public void Add(T item) => _items.Add(item);
		public bool Remove(T item) => _items.Remove(item);
		public void Clear() => _items.Clear();
		public bool Contains(T item) => _items.Contains(item);
        public int IndexOf(T item) => _items.IndexOf(item);
        public void Insert(int index, T item) => _items.Insert(index, item);
        public void RemoveAt(int index) => _items.RemoveAt(index);
        public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
		public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        public T this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }

        #region ICollection Implementation

        bool ICollection<T>.IsReadOnly => false;
		bool ICollection.IsSynchronized => false;
		object ICollection.SyncRoot => this;
		void ICollection.CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);

        #endregion

        #region IEnumerable Implementation

        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

		#endregion

		#region IList Implementation

		object IList.this[int index]
		{
			get => _items[index];
			set => ((IList)_items)[index] = value;
		}

		bool IList.IsFixedSize => false;
		bool IList.IsReadOnly => false;
		int IList.Add(object value) => ((IList)_items).Add(value);
		void IList.Insert(int index, object value) => ((IList)_items).Insert(index, value);
		void IList.Remove(object value) => ((IList)_items).Remove(value);
		bool IList.Contains(object value) => ((IList)_items).Contains(value);
		int IList.IndexOf(object value) => ((IList)_items).IndexOf(value);

		#endregion
	}
}
